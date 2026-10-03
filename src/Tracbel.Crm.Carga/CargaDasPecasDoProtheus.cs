using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga das peças fez (ou faria, na simulação), em número — sem nome de cliente.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana — inclusive os totais em reais, para conferir com o BI.</param>
/// <param name="Alcance">Curta ou completa, desde quando, e o que a completa corrigiu fora do alcance curto — vai para o resumo.</param>
internal sealed record RelatorioDasPecas(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes,
    string Alcance = "")
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// AS PEÇAS DO PROTHEUS (pedido do Ricardo em 02/10/2026) — <c>--somente-pecas-protheus</c>, o segundo modo da rotina 15
/// <c>POS_VENDA_PROTHEUS</c>, depois das ordens de serviço.
///
/// <para><b>O faturamento de peças</b> (<see cref="FaturamentoDePecasNoMes"/>) é APURAÇÃO: a rodada regrava inteira a janela de
/// três anos — por mês, filial, cliente, setor, grupo comercial, linha e vendedor, nas regras do painel "Faturamento Peças"
/// do BI (<see cref="RegrasDasPecas"/>). <b>Os orçamentos</b> (<see cref="OrcamentoDePecas"/>) são SINCRONIA, como as ordens de
/// serviço: o novo entra, o que mudou é atualizado com trilha, o que some dentro da janela é excluído sem apagar, e o que
/// volta é reativado.</para>
///
/// <para><b>As travas:</b> leitura vazia não apaga nada; o faturamento que encolheria a menos da metade das combinações que
/// já estão na janela é leitura parcial; orçamento ilegível acima de 5%, ou excluir mais de 20% dos vigentes, aborta tudo. Só
/// <c>--aceitar-remocao</c>, no terminal, passa por cima das travas de encolhimento.</para>
///
/// <para><b>Curta nos dias comuns, completa no domingo</b> (plano 3 do documento 54; <see cref="AlcanceDaLeitura"/>). A curta
/// regrava o faturamento desde o mês dos últimos três dias de emissão, lê os orçamentos orçados desde então, os abertos e os
/// ALTERADOS desde então (a view tem a data de alteração), e só exclui o orçamento orçado dentro dela. A completa conta o que
/// corrigiu fora do alcance curto: os meses-filial do faturamento com valor diferente, com os reais, e os orçamentos.</para>
/// </summary>
internal sealed class CargaDasPecasDoProtheus(
    Func<CrmDbContext> abrirContexto,
    Func<DateOnly, DateOnly, DateOnly?, CancellationToken, Task<Resultado<LeituraDasPecas>>> ler,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>Os meses da janela dos orçamentos — o aberto entra de qualquer data.</summary>
    internal const int MesesDosOrcamentos = 24;

    /// <summary>A menor fração das combinações que já estão na janela que uma rodada pode trazer sem abortar.</summary>
    internal const double FracaoMinimaDoFaturamento = 0.50;

    /// <summary>Abaixo disto a trava do faturamento não se aplica.</summary>
    internal const int FaturamentoMinimoParaATrava = 100;

    /// <summary>A maior fração dos orçamentos vigentes na janela que uma rodada pode excluir.</summary>
    internal const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>Abaixo disto a trava de remoção dos orçamentos não se aplica.</summary>
    internal const int JanelaMinima = 50;

    /// <summary>A maior fração de orçamentos ilegíveis que uma rodada aceita.</summary>
    internal const double FracaoMaximaDeRecusa = 0.05;

    /// <summary>Etapa do faturamento.</summary>
    internal const string EtapaDoFaturamento = "1. Faturamento de peças (X_V_BI_FATURAMENTO_PECAS)";

    /// <summary>Etapa dos orçamentos.</summary>
    internal const string EtapaDosOrcamentos = "2. Orçamentos de peças (X_V_BI_POSICAO_ORC_PECAS)";

    /// <summary>Rótulo: combinações gravadas.</summary>
    internal const string RotuloDeCombinacoes = "combinações de mês, filial, cliente, setor, grupo, linha e vendedor";

    /// <summary>Rótulo: combinações com cliente.</summary>
    internal const string RotuloDeCombinacoesComCliente = "  delas, com o cliente do CRM";

    /// <summary>Rótulo: linhas de filial que o CRM não tem.</summary>
    internal const string RotuloSemFilial = "linhas somadas de filial que o CRM não tem — contadas à parte, não gravadas";

    /// <summary>Rótulo: orçamentos lidos.</summary>
    internal const string RotuloDeOrcamentos = "orçamentos distintos (filial e número)";

    /// <summary>Rótulo: orçamentos ilegíveis.</summary>
    internal const string RotuloDeOrcamentosRecusados = "orçamentos ilegíveis (sem situação ou sem data) — o que já existia fica como estava";

    /// <summary>Rótulo: orçamentos novos.</summary>
    internal const string RotuloDeNovos = "orçamentos novos";

    /// <summary>Rótulo: atualizados.</summary>
    internal const string RotuloDeAtualizados = "orçamentos atualizados";

    /// <summary>Rótulo: excluídos.</summary>
    internal const string RotuloDeExcluidos = "orçamentos que sumiram da origem dentro da janela (excluídos, nunca apagados)";

    /// <summary>Rótulo: reativados.</summary>
    internal const string RotuloDeReativados = "orçamentos que voltaram à origem (reativados)";

    /// <summary>Rótulo: os meses-filial do faturamento que só a completa corrige.</summary>
    internal const string RotuloDeMesesCorrigidosForaDoCurto = "meses-filial do faturamento corrigidos fora do alcance curto — o que a leitura curta não teria visto";

    /// <summary>Rótulo: os orçamentos que só a completa corrige.</summary>
    internal const string RotuloDeOrcamentosCorrigidosForaDoCurto = "orçamentos corrigidos fora do alcance curto — o que a leitura curta não teria visto";

    /// <summary>O ponto da leitura completa: o instante dela decide quando a próxima é completa.</summary>
    internal const string FluxoDaLeituraCompleta = "PROTHEUS.FATURAMENTO_PECAS_COMPLETO";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>O primeiro dia da janela dos orçamentos.</summary>
    internal static DateOnly InicioDosOrcamentos(DateOnly hoje) => new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-MesesDosOrcamentos);

    /// <summary>Executa a carga — curta ou completa pela agenda da semana.</summary>
    /// <param name="simular">Só planeja e conta.</param>
    /// <param name="aceitarRemocao">Passa por cima das travas de encolhimento — só no terminal.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<RelatorioDasPecas>> ExecutarAsync(bool simular, bool aceitarRemocao, CancellationToken ct) =>
        ExecutarAsync(simular, aceitarRemocao, completaPedida: false, ct);

    /// <summary>Executa a carga.</summary>
    /// <param name="simular">Só planeja e conta.</param>
    /// <param name="aceitarRemocao">Passa por cima das travas de encolhimento — só no terminal.</param>
    /// <param name="completaPedida">Força a leitura completa em qualquer dia (<c>--completa</c>).</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDasPecas>> ExecutarAsync(bool simular, bool aceitarRemocao, bool completaPedida, CancellationToken ct)
    {
        var agora = relogio();
        var hoje = DateOnly.FromDateTime(agora);

        DateTime? ultimaCompleta;
        await using (var banco = abrirContexto())
            ultimaCompleta = await RodadaCompleta.LerAsync(banco, FluxoDaLeituraCompleta, ct);
        var alcance = AlcanceDaLeitura.Decidir(agora, ultimaCompleta, completaPedida, CargaDasOrdensDeServicoDoProtheus.InicioDaJanela(hoje));
        var curta = alcance.Modo == ModoDaLeitura.Curta;
        var desdeDoFaturamento = alcance.Desde;
        var desdeDosOrcamentos = curta ? alcance.Desde : InicioDosOrcamentos(hoje);

        relatar(curta
            ? $"Leitura curta das peças: o faturamento desde {desdeDoFaturamento:MM/yyyy}, os orçamentos orçados ou alterados desde {desdeDosOrcamentos:dd/MM/yyyy} e os abertos ({alcance.Motivo})…"
            : $"Lendo o faturamento de peças desde {desdeDoFaturamento:MM/yyyy} e os orçamentos desde {desdeDosOrcamentos:MM/yyyy} — leitura COMPLETA ({alcance.Motivo})…");
        var lida = await ler(desdeDoFaturamento, desdeDosOrcamentos, curta ? alcance.Desde : null, ct);
        if (!lida.EhSucesso) return Resultado<RelatorioDasPecas>.Indisponivel(lida.Erro!);

        var origem = lida.Valor;
        if (origem.Faturamento.Count == 0)
            return Resultado<RelatorioDasPecas>.Indisponivel(
                "O faturamento de peças veio vazio. Isso não é uma empresa que parou de vender peça, é uma leitura a conferir — nada foi " +
                "apagado e nada foi gravado.");

        Plano plano;
        await using (var banco = abrirContexto())
        {
            plano = await PlanejarAsync(banco, origem, alcance, desdeDosOrcamentos, ct);
        }

        Relatar(plano);

        if (plano.FaturamentoNaJanela >= FaturamentoMinimoParaATrava && plano.Combinacoes.Count < plano.FaturamentoNaJanela * FracaoMinimaDoFaturamento)
        {
            var texto = string.Create(CultureInfo.InvariantCulture,
                $"O faturamento de peças trouxe {plano.Combinacoes.Count} combinações contra {plano.FaturamentoNaJanela} já gravadas na janela (menos de " +
                $"{FracaoMinimaDoFaturamento:P0}) — leitura parcial do Protheus.");
            if (!aceitarRemocao)
                return Resultado<RelatorioDasPecas>.Indisponivel(texto + " A CARGA INTEIRA FOI ABORTADA: nada foi gravado. Se for mesmo o que aconteceu, rode no terminal com --aceitar-remocao.");
            _observacoes.Add(texto + " Aceita por --aceitar-remocao, no terminal.");
        }

        if (plano.Orcamentos > 0 && plano.OrcamentosRecusados > plano.Orcamentos * FracaoMaximaDeRecusa)
            return Resultado<RelatorioDasPecas>.Indisponivel(string.Create(CultureInfo.InvariantCulture,
                $"{plano.OrcamentosRecusados} de {plano.Orcamentos} orçamentos ilegíveis (acima de {FracaoMaximaDeRecusa:P0}): a view mandou a situação " +
                $"ou a data noutra forma. A CARGA INTEIRA FOI ABORTADA: nada foi gravado."));

        if (plano.OrcamentosVigentes >= JanelaMinima && plano.AExcluir.Count > plano.OrcamentosVigentes * FracaoMaximaDeRemocao)
        {
            var texto = string.Create(CultureInfo.InvariantCulture,
                $"A rodada excluiria {plano.AExcluir.Count} de {plano.OrcamentosVigentes} orçamentos vigentes na janela (acima de {FracaoMaximaDeRemocao:P0}) — " +
                $"leitura parcial do Protheus.");
            if (!aceitarRemocao)
                return Resultado<RelatorioDasPecas>.Indisponivel(texto + " A CARGA INTEIRA FOI ABORTADA: nada foi gravado. Se for mesmo o que aconteceu, rode no terminal com --aceitar-remocao.");
            _observacoes.Add(texto + " Aceita por --aceitar-remocao, no terminal.");
        }

        if (!plano.EsquemaExiste && !simular)
            return Resultado<RelatorioDasPecas>.Indisponivel(
                "O banco ainda não tem a migração das peças. Publique a versão com ela antes de ligar o modo — nada foi gravado.");

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura.");
            return Resultado<RelatorioDasPecas>.Ok(new RelatorioDasPecas(true, _contagens, _observacoes, TextoDoAlcance(alcance, plano)));
        }

        await AplicarAsync(plano, agora, origem, ct);
        return Resultado<RelatorioDasPecas>.Ok(new RelatorioDasPecas(false, _contagens, _observacoes, TextoDoAlcance(alcance, plano)));
    }

    /// <summary>O alcance da rodada em uma frase, para o resumo da rotina.</summary>
    private static string TextoDoAlcance(AlcanceDaLeitura alcance, Plano plano) => alcance.Modo == ModoDaLeitura.Curta
        ? string.Format(CultureInfo.GetCultureInfo("pt-BR"), "leitura curta desde {0:dd/MM/yyyy}", alcance.Desde)
        : string.Format(CultureInfo.GetCultureInfo("pt-BR"),
            "leitura COMPLETA desde {0:dd/MM/yyyy}, {1:N0} mês(es)-filial do faturamento corrigido(s) (R$ {2:N0}) e {3:N0} orçamento(s) fora do alcance curto",
            alcance.Desde, plano.MesesCorrigidosForaDoCurto, plano.ValorCorrigidoForaDoCurto, plano.OrcamentosCorrigidosForaDoCurto);

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private static async Task<Plano> PlanejarAsync(
        CrmDbContext banco, LeituraDasPecas origem, AlcanceDaLeitura alcance, DateOnly desdeDosOrcamentos, CancellationToken ct)
    {
        // A JANELA É A DO ALCANCE, e não o "desde" da leitura: na curta, a apuração regrava só o mês curto.
        var plano = new Plano
        {
            EsquemaExiste = await EsquemaExisteAsync(banco, ct),
            FaturamentoDesde = alcance.Desde,
            Completa = alcance.Modo == ModoDaLeitura.Completa
        };

        var empresaPorCodigo = await banco.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);
        var clientesPorDocumento = (await banco.Clientes.AsNoTracking()
                .Where(c => c.ExcluidoEm == null && c.Documento != null)
                .Select(c => new { c.Id, c.Documento })
                .ToListAsync(ct))
            .GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList(), StringComparer.Ordinal);

        long? Cliente(string? documento) =>
            documento is not null && clientesPorDocumento.TryGetValue(documento, out var candidatos) && candidatos.Count == 1 ? candidatos[0] : null;

        var sistema = plano.EsquemaExiste
            ? await banco.Sistemas.AsNoTracking().Where(s => s.Codigo == LeitorDeClientesDoProtheus.CodigoDoSistema)
                .Select(s => (int?)s.Id).FirstOrDefaultAsync(ct)
            : null;

        // ---- o faturamento: as linhas somadas no banco viram a combinação final, nas regras do BI ----
        var combinacoes = new Dictionary<ChaveDaCombinacao, Acumulado>();
        foreach (var linha in origem.Faturamento)
        {
            if (RegrasDasPecas.CodigoDaFilial(linha.Filial) is not { } codigo || !empresaPorCodigo.TryGetValue(codigo, out var empresaId))
            {
                plano.LinhasSemFilial++;
                plano.ValorSemFilial += linha.ValorLiquido;
                continue;
            }

            var devolucao = RegrasDasPecas.EhDevolucao(linha.Origem);
            var chave = new ChaveDaCombinacao(
                empresaId, linha.Mes, Cliente(linha.Documento),
                Texto(linha.Setor, FaturamentoDePecasNoMes.TamanhoDoTextoCurto),
                Texto(RegrasDasPecas.Grupo(linha.Origem, linha.Linha, linha.CodigoDaFamilia), FaturamentoDePecasNoMes.TamanhoDoTextoCurto),
                Texto(linha.Linha, FaturamentoDePecasNoMes.TamanhoDoTexto),
                Limitar(linha.VendedorCodigo, FaturamentoDePecasNoMes.TamanhoDoCodigo));

            if (!combinacoes.TryGetValue(chave, out var acumulado)) combinacoes[chave] = acumulado = new Acumulado();
            acumulado.VendedorNome ??= Limitar(linha.VendedorNome, FaturamentoDePecasNoMes.TamanhoDoTexto);
            if (RegrasDasPecas.QuantidadeConta(linha.Cortesia, linha.TipoDeOrdem, linha.Operacao)) acumulado.Quantidade += linha.Quantidade;
            acumulado.ValorLiquido += linha.ValorLiquido;
            if (devolucao)
            {
                acumulado.Devolucoes += linha.ValorLiquido;
                if (linha.ValorLiquido > 0) plano.DevolucoesPositivas++;
            }

            acumulado.Desconto += linha.ValorDeDesconto;
            acumulado.Tabela += linha.ValorDeTabela;
            acumulado.Itens += Math.Max(1, linha.Itens);
            if (linha.Documento is not null && chave.ClienteId is null) plano.DocumentosForaDoCrm.Add(linha.Documento);
        }

        plano.Combinacoes.AddRange(combinacoes.Select(c => new DadosDoFaturamentoDePecas(
            c.Key.EmpresaId, c.Key.Mes, c.Key.ClienteId, c.Key.Setor, c.Key.Grupo, c.Key.Linha, c.Key.VendedorCodigo, c.Value.VendedorNome,
            c.Value.Quantidade, c.Value.ValorLiquido, c.Value.Devolucoes, c.Value.Desconto, c.Value.Tabela, c.Value.Itens)));

        plano.FaturamentoNaJanela = sistema is { } s0
            ? await banco.FaturamentosDePecasNoMes.IgnoreQueryFilters().AsNoTracking()
                .CountAsync(f => f.SistemaId == s0 && f.Competencia >= plano.FaturamentoDesde, ct)
            : 0;

        // A CONTA DA COMPLETA NO FATURAMENTO: o total de cada mês-filial antes do início curto, como está gravado e como a leitura
        // o refaz. O que difere é o que a leitura curta, sozinha, não teria corrigido.
        if (plano.Completa && sistema is { } s2)
        {
            var antes = (await banco.FaturamentosDePecasNoMes.IgnoreQueryFilters().AsNoTracking()
                    .Where(f => f.SistemaId == s2 && f.Competencia >= plano.FaturamentoDesde && f.Competencia < alcance.InicioDaJanelaCurta)
                    .Select(f => new { f.EmpresaId, f.Competencia, f.ValorLiquido })
                    .ToListAsync(ct))
                .GroupBy(f => (f.EmpresaId, f.Competencia))
                .ToDictionary(g => g.Key, g => g.Sum(f => f.ValorLiquido));
            var depois = plano.Combinacoes.Where(c => c.Competencia < alcance.InicioDaJanelaCurta)
                .GroupBy(c => (c.EmpresaId, c.Competencia))
                .ToDictionary(g => g.Key, g => g.Sum(c => c.ValorLiquido));

            foreach (var chave in antes.Keys.Union(depois.Keys))
            {
                var diferenca = depois.GetValueOrDefault(chave) - antes.GetValueOrDefault(chave);
                if (diferenca == 0) continue;
                plano.MesesCorrigidosForaDoCurto++;
                plano.ValorCorrigidoForaDoCurto += Math.Abs(diferenca);
            }
        }

        // ---- os orçamentos: o cabeçalho, com o total dos itens ----
        var noCrm = sistema is { } s1
            ? (await banco.OrcamentosDePecas.IgnoreQueryFilters().AsNoTracking().Where(o => o.SistemaId == s1)
                    .Select(o => new OrcamentoNoCrm(o.ChaveNaOrigem, o.HashDaOrigem, o.ExcluidoEm != null, o.EmpresaId, o.ClienteId, o.OrcadoEm, o.Situacao))
                    .ToListAsync(ct))
                .ToDictionary(o => o.Chave, StringComparer.Ordinal)
            : new Dictionary<string, OrcamentoNoCrm>(StringComparer.Ordinal);

        foreach (var grupo in origem.Orcamentos.GroupBy(i => (i.Filial, i.Numero)))
        {
            plano.Orcamentos++;
            var chave = OrcamentoDePecas.ChaveDe(grupo.Key.Filial, grupo.Key.Numero);
            plano.Vistos.Add(chave);

            var itens = grupo.ToList();
            var situacao = itens.Select(i => i.Situacao).FirstOrDefault(s => s is not null);
            var orcadoEm = itens.Select(i => i.OrcadoEm).FirstOrDefault(d => d is not null);
            if (grupo.Key.Filial.Length == 0 || grupo.Key.Numero.Length is 0 or > OrcamentoDePecas.TamanhoDoNumero
                || chave.Length > OrcamentoDePecas.TamanhoDaChave || situacao is null || situacao.Length > OrcamentoDePecas.TamanhoDoTextoCurto
                || orcadoEm is null)
            {
                plano.OrcamentosRecusados++;
                continue;
            }

            if (RegrasDasPecas.CodigoDaFilial(grupo.Key.Filial) is not { } codigo || !empresaPorCodigo.TryGetValue(codigo, out var empresaId))
            {
                plano.OrcamentosSemFilial++;
                continue;
            }

            var documento = itens.Select(i => i.Documento).FirstOrDefault(d => d is not null);
            var clienteId = Cliente(documento);
            var codigosDosItens = itens.Select(i => i.CodigoDoItem).OfType<string>().Distinct(StringComparer.Ordinal).Count();
            var total = itens.Sum(i => i.ValorTotal ?? 0m);
            var desconto = itens.Sum(i => i.ValorDeDesconto ?? 0m);
            var dados = new DadosDoOrcamentoDePecas(
                empresaId, grupo.Key.Numero, clienteId, situacao,
                Limitar(itens.Select(i => i.Prazo).FirstOrDefault(p => p is not null), OrcamentoDePecas.TamanhoDoTextoCurto),
                Limitar(itens.Select(i => i.Reserva).FirstOrDefault(r => r is not null), OrcamentoDePecas.TamanhoDoTextoCurto),
                Limitar(itens.Select(i => i.TipoDeAtendimento).FirstOrDefault(t => t is not null), OrcamentoDePecas.TamanhoDoTextoCurto),
                Limitar(itens.Select(i => i.TipoDeOrcamento).FirstOrDefault(t => t is not null), OrcamentoDePecas.TamanhoDoTextoCurto),
                orcadoEm.Value,
                itens.Select(i => i.ValidoAte).FirstOrDefault(d => d is not null),
                itens.Select(i => i.AlteradoEm).FirstOrDefault(d => d is not null),
                Limitar(itens.Select(i => i.VendedorCodigo).FirstOrDefault(v => v is not null), FaturamentoDePecasNoMes.TamanhoDoCodigo),
                Limitar(itens.Select(i => i.VendedorNome).FirstOrDefault(v => v is not null), OrcamentoDePecas.TamanhoDoTexto),
                total, desconto, Math.Max(1, codigosDosItens > 0 ? codigosDosItens : itens.Count), string.Empty);

            dados = dados with
            {
                HashDaOrigem = Resumir(chave, situacao, dados.Prazo, dados.Reserva, dados.TipoDeAtendimento, dados.TipoDeOrcamento,
                    Data(dados.OrcadoEm), Data(dados.ValidoAte), Data(dados.AlteradoNaOrigemEm), dados.VendedorCodigo, dados.VendedorNome, documento,
                    decimal.Round(total, 2).ToString(CultureInfo.InvariantCulture), decimal.Round(desconto, 2).ToString(CultureInfo.InvariantCulture),
                    dados.Itens.ToString(CultureInfo.InvariantCulture))
            };
            plano.OrcamentosValidos.Add((chave, dados));

            var mudou = true;
            if (!noCrm.TryGetValue(chave, out var existente)) plano.Novos++;
            else
            {
                if (existente.Excluido) plano.AReativar++;
                if (existente.Hash != dados.HashDaOrigem || existente.EmpresaId != empresaId || existente.ClienteId != clienteId) plano.AAtualizar++;
                else if (!existente.Excluido) mudou = false;
            }

            // O QUE SÓ A COMPLETA VÊ: orçado antes do início curto, fechado, e sem alteração depois dele.
            if (mudou && plano.Completa && orcadoEm.Value < alcance.InicioDaJanelaCurta && !OrcamentoDePecas.EstaEmAbertoNa(situacao)
                && !(dados.AlteradoNaOrigemEm >= alcance.InicioDaJanelaCurta))
                plano.OrcamentosCorrigidosForaDoCurto++;
        }

        // NA LEITURA CURTA a janela da exclusão é só a dos orçados nela: o orçamento antigo que não veio espera a completa.
        bool NaJanela(OrcamentoNoCrm o) => o.OrcadoEm >= desdeDosOrcamentos || (plano.Completa && OrcamentoDePecas.EstaEmAbertoNa(o.Situacao));
        plano.OrcamentosVigentes = noCrm.Values.Count(o => !o.Excluido && NaJanela(o));
        plano.AExcluir.AddRange(noCrm.Values.Where(o => !o.Excluido && NaJanela(o) && !plano.Vistos.Contains(o.Chave)).Select(o => o.Chave));
        if (plano.Completa)
            plano.OrcamentosCorrigidosForaDoCurto += noCrm.Values.Count(o => o.OrcadoEm < alcance.InicioDaJanelaCurta && plano.AExcluir.Contains(o.Chave));
        return plano;
    }

    // =============================================================================================
    // A gravação — uma transação
    // =============================================================================================

    private async Task AplicarAsync(Plano plano, DateTime lidaEm, LeituraDasPecas origem, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeClientesDoProtheus.CodigoDoSistema, "Protheus (TOTVS) — ERP", "SQL Server, somente leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        // O FATURAMENTO É APURAÇÃO: a janela é regravada inteira, como a conferência com a Gestão de Negócios.
        await banco.FaturamentosDePecasNoMes.IgnoreQueryFilters()
            .Where(f => f.SistemaId == sistemaId && f.Competencia >= plano.FaturamentoDesde)
            .ExecuteDeleteAsync(ct);
        foreach (var bloco in plano.Combinacoes.Chunk(5000))
        {
            banco.FaturamentosDePecasNoMes.AddRange(bloco.Select(d => FaturamentoDePecasNoMes.Apurar(sistemaId, d, lidaEm, usuarioId)));
            await banco.SaveChangesAsync(ct);
            banco.ChangeTracker.Clear();
            banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);
        }

        // OS ORÇAMENTOS SÃO SINCRONIA, como as ordens de serviço.
        var existentes = (await banco.OrcamentosDePecas.IgnoreQueryFilters().Where(o => o.SistemaId == sistemaId).ToListAsync(ct))
            .ToDictionary(o => o.ChaveNaOrigem, StringComparer.Ordinal);
        var gravados = 0;
        foreach (var (chave, dados) in plano.OrcamentosValidos)
        {
            if (!existentes.TryGetValue(chave, out var existente))
            {
                banco.OrcamentosDePecas.Add(OrcamentoDePecas.Registrar(sistemaId, chave, dados, lidaEm, usuarioId));
                gravados++;
                continue;
            }

            var reativado = existente.Reativar(usuarioId);
            var atualizado = existente.AtualizarDaOrigem(dados, lidaEm, usuarioId);
            if (reativado || atualizado) gravados++;
        }

        foreach (var chave in plano.AExcluir)
        {
            existentes[chave].Excluir(usuarioId);
            gravados++;
        }

        await banco.SaveChangesAsync(ct);

        var instante = lidaEm.ToString("O", CultureInfo.InvariantCulture);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, FaturamentoDePecasNoMes.FluxoDaCarga, origem.Faturamento.Count,
            plano.Combinacoes.Count, plano.LinhasSemFilial, ct, instante);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, OrcamentoDePecas.FluxoDaCarga, plano.Orcamentos, gravados,
            plano.OrcamentosRecusados + plano.OrcamentosSemFilial, ct, instante);

        // O PONTO DA COMPLETA NA MESMA TRANSAÇÃO: a completa que cai no meio não vira "última completa".
        if (plano.Completa)
            await RodadaCompleta.RegistrarAsync(banco, sistemaId, FluxoDaLeituraCompleta, lidaEm, origem.Faturamento.Count, plano.Combinacoes.Count + gravados, ct);

        await transacao.CommitAsync(ct);
        relatar($"Gravado ({plano.Combinacoes.Count:N0} combinações do faturamento de peças; {gravados:N0} orçamentos novos, mudados, reativados ou excluídos).");
    }

    // =============================================================================================
    // O relatório
    // =============================================================================================

    private void Relatar(Plano plano)
    {
        Contar(EtapaDoFaturamento, RotuloDeCombinacoes, plano.Combinacoes.Count);
        Contar(EtapaDoFaturamento, RotuloDeCombinacoesComCliente, plano.Combinacoes.Count(c => c.ClienteId is not null));
        Contar(EtapaDoFaturamento, "  clientes do CRM com compra de peça", plano.Combinacoes.Select(c => c.ClienteId).OfType<long>().Distinct().Count());
        Contar(EtapaDoFaturamento, "  CPF/CNPJ que não é cliente do CRM (fica sem cliente)", plano.DocumentosForaDoCrm.Count);
        Contar(EtapaDoFaturamento, RotuloSemFilial, plano.LinhasSemFilial);
        Contar(EtapaDoFaturamento, "combinações já gravadas na janela (regravada inteira)", plano.FaturamentoNaJanela);
        Contar(EtapaDoFaturamento, "linhas de devolução com valor positivo (a conferir com o BI)", plano.DevolucoesPositivas);

        Contar(EtapaDosOrcamentos, RotuloDeOrcamentos, plano.Orcamentos);
        Contar(EtapaDosOrcamentos, RotuloDeOrcamentosRecusados, plano.OrcamentosRecusados);
        Contar(EtapaDosOrcamentos, "orçamentos de filial que o CRM não tem — contados à parte, não gravados", plano.OrcamentosSemFilial);
        Contar(EtapaDosOrcamentos, "  em aberto (aberto ou parcialmente atendido)", plano.OrcamentosValidos.Count(o => OrcamentoDePecas.EstaEmAbertoNa(o.Dados.Situacao)));
        Contar(EtapaDosOrcamentos, "  com o cliente do CRM", plano.OrcamentosValidos.Count(o => o.Dados.ClienteId is not null));
        Contar(EtapaDosOrcamentos, "vigentes na janela antes da rodada", plano.OrcamentosVigentes);
        Contar(EtapaDosOrcamentos, RotuloDeNovos, plano.Novos);
        Contar(EtapaDosOrcamentos, RotuloDeAtualizados, plano.AAtualizar);
        Contar(EtapaDosOrcamentos, RotuloDeReativados, plano.AReativar);
        Contar(EtapaDosOrcamentos, RotuloDeExcluidos, plano.AExcluir.Count);

        // A CONTA DA COMPLETA SAI ATÉ QUANDO É ZERO: "nada corrigido" é a confirmação de que a leitura curta basta.
        if (plano.Completa)
        {
            Contar(EtapaDoFaturamento, RotuloDeMesesCorrigidosForaDoCurto, plano.MesesCorrigidosForaDoCurto);
            Contar(EtapaDosOrcamentos, RotuloDeOrcamentosCorrigidosForaDoCurto, plano.OrcamentosCorrigidosForaDoCurto);
        }

        // OS TOTAIS EM REAIS, pelo ano fiscal do painel "Faturamento Peças" (novembro a outubro) — é por eles que se confere.
        var brasil = CultureInfo.GetCultureInfo("pt-BR");
        foreach (var ano in plano.Combinacoes.GroupBy(c => c.Competencia.Month >= 11 ? c.Competencia.Year + 1 : c.Competencia.Year).OrderBy(g => g.Key))
            _observacoes.Add(string.Format(brasil,
                "Para conferir com o BI — faturamento de peças do ano fiscal {0} (nov a out): R$ {1:N2}, das quais R$ {2:N2} de devoluções; " +
                "balcão e oficina pelo setor da nota.",
                ano.Key, ano.Sum(c => c.ValorLiquido), ano.Sum(c => c.ValorDeDevolucoes)));
        if (plano.ValorSemFilial != 0)
            _observacoes.Add(string.Format(brasil, "Das filiais que o CRM não tem: R$ {0:N2}, fora do que foi gravado.", plano.ValorSemFilial));

        var abertos = plano.OrcamentosValidos.Where(o => OrcamentoDePecas.EstaEmAbertoNa(o.Dados.Situacao)).ToList();
        _observacoes.Add(string.Format(brasil, "Para conferir com o BI — orçamentos em aberto: {0:N0}, somando R$ {1:N2}.",
            abertos.Count, abertos.Sum(o => o.Dados.ValorTotal)));

        if (!plano.EsquemaExiste)
            _observacoes.Add("O banco lido ainda não tem a migração das peças: o plano parte de nada gravado, que é o que o banco terá logo depois dela.");
    }

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private static async Task<bool> EsquemaExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'comercial.OrcamentoDePecas')")
            .SingleAsync(ct) > 0;

    private static string Texto(string? texto, int tamanho) =>
        Limitar(string.IsNullOrWhiteSpace(texto) ? FaturamentoDePecasNoMes.SemClassificacao : texto.Trim(), tamanho)!;

    private static string? Limitar(string? texto, int tamanho)
    {
        var limpo = texto?.Trim();
        if (string.IsNullOrEmpty(limpo)) return null;
        return limpo.Length <= tamanho ? limpo : limpo[..tamanho];
    }

    private static string Data(DateOnly? data) => data?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>O resumo do conteúdo — o documento entra no cálculo e não sai dele.</summary>
    private static string Resumir(params string?[] partes) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\u001f', partes.Select(p => p ?? string.Empty)))));

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    private sealed record ChaveDaCombinacao(int EmpresaId, DateOnly Mes, long? ClienteId, string Setor, string Grupo, string Linha, string? VendedorCodigo);

    private sealed class Acumulado
    {
        public string? VendedorNome { get; set; }
        public decimal Quantidade { get; set; }
        public decimal ValorLiquido { get; set; }
        public decimal Devolucoes { get; set; }
        public decimal Desconto { get; set; }
        public decimal Tabela { get; set; }
        public int Itens { get; set; }
    }

    private sealed record OrcamentoNoCrm(string Chave, string Hash, bool Excluido, int EmpresaId, long? ClienteId, DateOnly OrcadoEm, string Situacao);

    private sealed class Plano
    {
        public bool EsquemaExiste { get; init; }
        public bool Completa { get; init; }
        public int MesesCorrigidosForaDoCurto { get; set; }
        public decimal ValorCorrigidoForaDoCurto { get; set; }
        public int OrcamentosCorrigidosForaDoCurto { get; set; }
        public DateOnly FaturamentoDesde { get; init; }
        public List<DadosDoFaturamentoDePecas> Combinacoes { get; } = [];
        public int FaturamentoNaJanela { get; set; }
        public int LinhasSemFilial { get; set; }
        public decimal ValorSemFilial { get; set; }
        public int DevolucoesPositivas { get; set; }
        public HashSet<string> DocumentosForaDoCrm { get; } = new(StringComparer.Ordinal);
        public int Orcamentos { get; set; }
        public int OrcamentosRecusados { get; set; }
        public int OrcamentosSemFilial { get; set; }
        public int OrcamentosVigentes { get; set; }
        public int Novos { get; set; }
        public int AAtualizar { get; set; }
        public int AReativar { get; set; }
        public HashSet<string> Vistos { get; } = new(StringComparer.Ordinal);
        public List<string> AExcluir { get; } = [];
        public List<(string Chave, DadosDoOrcamentoDePecas Dados)> OrcamentosValidos { get; } = [];
    }
}
