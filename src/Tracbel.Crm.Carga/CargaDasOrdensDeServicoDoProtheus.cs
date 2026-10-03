using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga das ordens de serviço fez (ou faria, na simulação), em número — sem nome de ninguém.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana — inclusive os totais em reais, para conferir com o BI.</param>
/// <param name="Alcance">Curta ou completa, desde quando, e o que a completa corrigiu fora do alcance curto — vai para o resumo.</param>
internal sealed record RelatorioDasOrdensDeServico(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes,
    string Alcance = "")
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// AS ORDENS DE SERVIÇO DO PROTHEUS (pedido do Ricardo em 02/10/2026) — <c>--somente-ordens-de-servico</c>, o modo da
/// rotina 15 <c>POS_VENDA_PROTHEUS</c>.
///
/// <para><b>O que entra.</b> Toda OS aberta nos últimos <see cref="AnosDaJanela"/> anos e toda OS ainda na oficina, de
/// qualquer data, de uma filial que o CRM tem — uma linha por OS (<see cref="OrdemDeServico"/>), com o total de peças e
/// de serviços na régua do painel de pós-venda do BI (<see cref="RegrasDoValorDaOrdemDeServico"/>). O cliente é casado
/// pelo CPF/CNPJ do proprietário, e a máquina pelo chassi; a OS que não casa entra assim mesmo, sem cliente ou sem
/// máquina — ela é da filial e conta no pós-venda dela.</para>
///
/// <para><b>É SINCRONIA, e é repetível.</b> A OS nova entra, a que mudou é atualizada (a trilha guarda a situação, a
/// filial e o cliente), a que sumiu da origem DENTRO da janela é excluída sem apagar, e a que volta é reativada. A OS
/// fechada que saiu da janela por idade não é excluída: ela continua sendo história da máquina.</para>
///
/// <para><b>As travas das outras cargas</b>: mais de <see cref="FracaoMaximaDeRecusa"/> de OS ilegíveis (sem situação
/// conhecida ou sem abertura), ou excluir mais de <see cref="FracaoMaximaDeRemocao"/> do que está vigente na janela,
/// aborta a rodada inteira. Só <c>--aceitar-remocao</c>, no terminal, passa por cima da segunda.</para>
///
/// <para><b>Curta nos dias comuns, completa no domingo</b> (plano 3 do documento 54; <see cref="AlcanceDaLeitura"/>). A curta lê
/// as OS abertas desde o mês dos últimos três dias, as ainda na oficina e as fechadas ou canceladas desde então — a VO1 não tem
/// data de alteração —, e só exclui a OS aberta dentro dela: a antiga que não veio espera a completa, que conta o que corrigiu
/// fora do alcance curto.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="ler">A leitura das views do Protheus, a partir da abertura dada e, na curta, das mudanças desde a data dada.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relogio">O relógio (UTC).</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDasOrdensDeServicoDoProtheus(
    Func<CrmDbContext> abrirContexto,
    Func<DateOnly, DateOnly?, CancellationToken, Task<Resultado<LeituraDasOrdensDeServico>>> ler,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>Os anos da janela — os mesmos três do faturamento.</summary>
    internal const int AnosDaJanela = 3;

    /// <summary>A maior fração do que está vigente na janela que uma rodada pode excluir sem abortar.</summary>
    internal const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>Abaixo disto a trava de remoção não se aplica.</summary>
    internal const int JanelaMinima = 50;

    /// <summary>A maior fração de OS ilegíveis que uma rodada aceita.</summary>
    internal const double FracaoMaximaDeRecusa = 0.05;

    /// <summary>Etapa da leitura.</summary>
    internal const string EtapaDaLeitura = "1. Leitura das views do BI (X_V_BI_SERVICOS_*)";

    /// <summary>Etapa das OS.</summary>
    internal const string EtapaDasOrdens = "2. Ordens de serviço";

    /// <summary>Etapa dos casamentos.</summary>
    internal const string EtapaDosCasamentos = "3. Cliente e máquina do CRM";

    /// <summary>Etapa da sincronia.</summary>
    internal const string EtapaDaSincronia = "4. Sincronia (frota.OrdemDeServico)";

    /// <summary>Rótulo: OS lidas.</summary>
    internal const string RotuloDeOrdensLidas = "ordens de serviço distintas (filial e número)";

    /// <summary>Rótulo: OS de filial que o CRM não tem.</summary>
    internal const string RotuloSemFilial = "OS de filial que o CRM não tem — contadas à parte, não gravadas";

    /// <summary>Rótulo: OS ilegíveis.</summary>
    internal const string RotuloDeRecusadas = "OS ilegíveis (situação desconhecida, sem abertura ou chave fora do tamanho) — a que já existia fica como estava";

    /// <summary>Rótulo: com cliente.</summary>
    internal const string RotuloComCliente = "OS com o cliente do CRM (pelo CPF/CNPJ do proprietário)";

    /// <summary>Rótulo: com máquina.</summary>
    internal const string RotuloComMaquina = "OS com a máquina do CRM (pelo chassi)";

    /// <summary>Rótulo: novas.</summary>
    internal const string RotuloDeNovas = "novas";

    /// <summary>Rótulo: atualizadas.</summary>
    internal const string RotuloDeAtualizadas = "atualizadas (mudou na origem ou ganhou cliente ou máquina)";

    /// <summary>Rótulo: iguais.</summary>
    internal const string RotuloDeIguais = "sem mudança";

    /// <summary>Rótulo: excluídas.</summary>
    internal const string RotuloDeExcluidas = "sumiram da origem dentro da janela (excluídas, nunca apagadas)";

    /// <summary>Rótulo: reativadas.</summary>
    internal const string RotuloDeReativadas = "voltaram à origem (reativadas na mesma linha)";

    /// <summary>Rótulo: o que só a completa vê.</summary>
    internal const string RotuloCorrigidasForaDoCurto = "corrigidas fora do alcance curto — o que a leitura curta não teria visto";

    /// <summary>O ponto da leitura completa: o instante dela decide quando a próxima é completa.</summary>
    internal const string FluxoDaLeituraCompleta = "PROTHEUS.ORDENS_DE_SERVICO_COMPLETO";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>A primeira abertura da janela: o dia 1 do mesmo mês, <see cref="AnosDaJanela"/> anos atrás.</summary>
    /// <param name="hoje">A data de referência.</param>
    internal static DateOnly InicioDaJanela(DateOnly hoje) => new DateOnly(hoje.Year, hoje.Month, 1).AddYears(-AnosDaJanela);

    /// <summary>Se a rodada excluiria demais — leitura parcial.</summary>
    internal static bool RemocaoPassaDaTrava(int vigentes, int aExcluir) =>
        vigentes >= JanelaMinima && aExcluir > vigentes * FracaoMaximaDeRemocao;

    /// <summary>Executa a sincronia — curta ou completa pela agenda da semana.</summary>
    /// <param name="simular">Só planeja e conta.</param>
    /// <param name="aceitarRemocao">Passa por cima da trava de remoção — só no terminal.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<RelatorioDasOrdensDeServico>> ExecutarAsync(bool simular, bool aceitarRemocao, CancellationToken ct) =>
        ExecutarAsync(simular, aceitarRemocao, completaPedida: false, ct);

    /// <summary>Executa a sincronia.</summary>
    /// <param name="simular">Só planeja e conta.</param>
    /// <param name="aceitarRemocao">Passa por cima da trava de remoção — só no terminal.</param>
    /// <param name="completaPedida">Força a leitura completa em qualquer dia (<c>--completa</c>).</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDasOrdensDeServico>> ExecutarAsync(bool simular, bool aceitarRemocao, bool completaPedida, CancellationToken ct)
    {
        var agora = relogio();
        var hoje = DateOnly.FromDateTime(agora);

        DateTime? ultimaCompleta;
        await using (var banco = abrirContexto())
            ultimaCompleta = await RodadaCompleta.LerAsync(banco, FluxoDaLeituraCompleta, ct);
        var alcance = AlcanceDaLeitura.Decidir(agora, ultimaCompleta, completaPedida, InicioDaJanela(hoje));
        var curta = alcance.Modo == ModoDaLeitura.Curta;

        relatar(curta
            ? $"Leitura curta das ordens de serviço: abertas, fechadas ou canceladas desde {alcance.Desde:dd/MM/yyyy}, e as ainda abertas ({alcance.Motivo})…"
            : $"Lendo as ordens de serviço do Protheus abertas desde {alcance.Desde:dd/MM/yyyy}, e as ainda abertas — leitura COMPLETA ({alcance.Motivo})…");
        var lida = await ler(alcance.Desde, curta ? alcance.Desde : null, ct);
        if (!lida.EhSucesso) return Resultado<RelatorioDasOrdensDeServico>.Indisponivel(lida.Erro!);

        var origem = lida.Valor;

        // A ORIGEM VAZIA NÃO É UMA OFICINA SEM SERVIÇO: é uma leitura que falhou sem dizer. Sincronizar contra ela
        // excluiria todas as OS do CRM de uma vez.
        if (origem.Itens.Count == 0)
            return Resultado<RelatorioDasOrdensDeServico>.Indisponivel(
                "As ordens de serviço vieram vazias. Isso não é uma oficina sem serviço, é uma leitura a conferir — nada foi " +
                "excluído e nada foi gravado.");

        Plano plano;
        await using (var banco = abrirContexto())
        {
            plano = await PlanejarAsync(banco, origem, alcance, ct);
        }

        Relatar(origem, plano, hoje);

        if (plano.Ordens > 0 && plano.Recusadas > plano.Ordens * FracaoMaximaDeRecusa)
            return Resultado<RelatorioDasOrdensDeServico>.Indisponivel(string.Create(CultureInfo.InvariantCulture,
                $"{plano.Recusadas} de {plano.Ordens} ordens de serviço ilegíveis (acima de {FracaoMaximaDeRecusa:P0}): a view mandou a " +
                $"situação ou a data noutra forma. A CARGA INTEIRA FOI ABORTADA: nada foi gravado e o frescor não foi carimbado."));

        if (RemocaoPassaDaTrava(plano.VigentesNaJanela, plano.AExcluir.Count))
        {
            var texto = string.Create(CultureInfo.InvariantCulture,
                $"A rodada excluiria {plano.AExcluir.Count} de {plano.VigentesNaJanela} ordens de serviço vigentes na janela (acima de " +
                $"{FracaoMaximaDeRemocao:P0}) — leitura parcial do Protheus.");
            if (!aceitarRemocao)
                return Resultado<RelatorioDasOrdensDeServico>.Indisponivel(
                    texto + " A CARGA INTEIRA FOI ABORTADA: nada foi gravado. Se a remoção for mesmo o que aconteceu, rode no terminal com --aceitar-remocao.");
            _observacoes.Add(texto + " Aceita por --aceitar-remocao, no terminal.");
        }

        if (!plano.EsquemaExiste && !simular)
            return Resultado<RelatorioDasOrdensDeServico>.Indisponivel(
                "O banco ainda não tem a migração das ordens de serviço. Publique a versão com ela antes de ligar a rotina — nada foi gravado.");

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura.");
            return Resultado<RelatorioDasOrdensDeServico>.Ok(new RelatorioDasOrdensDeServico(true, _contagens, _observacoes, TextoDoAlcance(alcance, plano)));
        }

        await AplicarAsync(plano, agora, origem, ct);
        return Resultado<RelatorioDasOrdensDeServico>.Ok(new RelatorioDasOrdensDeServico(false, _contagens, _observacoes, TextoDoAlcance(alcance, plano)));
    }

    /// <summary>O alcance da rodada em uma frase, para o resumo da rotina.</summary>
    private static string TextoDoAlcance(AlcanceDaLeitura alcance, Plano plano) => alcance.Modo == ModoDaLeitura.Curta
        ? string.Format(CultureInfo.GetCultureInfo("pt-BR"), "leitura curta desde {0:dd/MM/yyyy}", alcance.Desde)
        : string.Format(CultureInfo.GetCultureInfo("pt-BR"), "leitura COMPLETA desde {0:dd/MM/yyyy}, {1:N0} OS corrigida(s) fora do alcance curto",
            alcance.Desde, plano.CorrigidasForaDoCurto);

    /// <summary>
    /// SE SÓ A COMPLETA VERIA ESTA OS: aberta antes do início curto, fora da oficina, e sem fechamento nem cancelamento depois
    /// dele. Na leitura curta nada é "fora do alcance curto".
    /// </summary>
    private static bool ForaDoAlcanceCurto(
        AlcanceDaLeitura alcance, DateOnly abertaEm, SituacaoDaOrdemDeServico situacao, DateOnly? fechadaEm, DateOnly? canceladaEm) =>
        alcance.Modo == ModoDaLeitura.Completa && abertaEm < alcance.InicioDaJanelaCurta && !OrdemDeServico.EstaEmAbertoNa(situacao)
        && !(fechadaEm >= alcance.InicioDaJanelaCurta) && !(canceladaEm >= alcance.InicioDaJanelaCurta);

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private static async Task<Plano> PlanejarAsync(CrmDbContext banco, LeituraDasOrdensDeServico origem, AlcanceDaLeitura alcance, CancellationToken ct)
    {
        var plano = new Plano { EsquemaExiste = await EsquemaExisteAsync(banco, ct), Completa = alcance.Modo == ModoDaLeitura.Completa };

        var empresaPorCodigo = await banco.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);

        var clientesPorDocumento = (await banco.Clientes.AsNoTracking()
                .Where(c => c.ExcluidoEm == null && c.Documento != null)
                .Select(c => new { c.Id, c.Documento })
                .ToListAsync(ct))
            .GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList(), StringComparer.Ordinal);

        var maquinaPorChassi = (await banco.Equipamentos.AsNoTracking()
                .Where(e => e.ExcluidoEm == null)
                .Select(e => new { e.Id, e.Chassi })
                .ToListAsync(ct))
            .GroupBy(e => e.Chassi.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.Ordinal);

        var sistema = plano.EsquemaExiste
            ? await banco.Sistemas.AsNoTracking().Where(s => s.Codigo == LeitorDeClientesDoProtheus.CodigoDoSistema)
                .Select(s => (int?)s.Id).FirstOrDefaultAsync(ct)
            : null;

        var noCrm = sistema is { } s1
            ? (await banco.OrdensDeServico.IgnoreQueryFilters().AsNoTracking().Where(o => o.SistemaId == s1)
                    .Select(o => new NoCrm(o.ChaveNaOrigem, o.HashDaOrigem, o.ExcluidoEm != null, o.EmpresaId, o.ClienteId, o.EquipamentoId,
                        o.AbertaEm, o.Situacao))
                    .ToListAsync(ct))
                .ToDictionary(o => o.Chave, StringComparer.Ordinal)
            : new Dictionary<string, NoCrm>(StringComparer.Ordinal);

        // OS SERVIÇOS POR OS, DISTINTOS — o LOAD DISTINCT do BI, pelos campos lidos.
        var servicosPorOs = origem.Servicos
            .Where(s => s.Filial.Length > 0 && s.NumeroOs.Length > 0)
            .Distinct()
            .GroupBy(s => OrdemDeServico.ChaveDe(s.Filial, s.NumeroOs), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var chavesDasOrdens = new HashSet<string>(StringComparer.Ordinal);

        foreach (var grupo in origem.Itens.GroupBy(i => (Filial: i.Filial, Numero: i.NumeroOs)))
        {
            plano.Ordens++;
            var chave = OrdemDeServico.ChaveDe(grupo.Key.Filial, grupo.Key.Numero);
            chavesDasOrdens.Add(chave);

            var itens = grupo.ToList();
            var situacao = itens.Select(i => OrdemDeServico.SituacaoPelaLetra(i.StatusDaCapa)).FirstOrDefault(s => s is not null);
            var abertaEm = itens.Select(i => i.AbertaEm).FirstOrDefault(d => d is not null);
            if (grupo.Key.Filial.Length == 0 || grupo.Key.Numero.Length is 0 or > OrdemDeServico.TamanhoDoNumero
                || chave.Length > OrdemDeServico.TamanhoDaChave || situacao is null || abertaEm is null)
            {
                plano.Recusadas++;
                plano.Vistas.Add(chave);
                continue;
            }

            if (!empresaPorCodigo.TryGetValue(grupo.Key.Filial, out var empresaId))
            {
                plano.SemFilial++;
                plano.Vistas.Add(chave);
                continue;
            }

            // O CLIENTE PELO DOCUMENTO — só quando um cliente só o tem; o documento não sai daqui.
            var documento = itens.Select(i => i.DocumentoDoProprietario).FirstOrDefault(d => d is not null);
            long? clienteId = null;
            if (documento is null) plano.SemDocumento++;
            else if (!clientesPorDocumento.TryGetValue(documento, out var candidatos)) plano.ClienteForaDoCrm++;
            else if (candidatos.Count > 1) plano.ClienteAmbiguo++;
            else clienteId = candidatos[0];

            var chassiNaOrigem = itens.Select(i => i.Chassi).FirstOrDefault(c => c is not null);
            if (chassiNaOrigem is { Length: > OrdemDeServico.TamanhoDoChassi })
            {
                plano.ChassiForaDoTamanho++;
                chassiNaOrigem = null;
            }

            long? equipamentoId = chassiNaOrigem is not null && maquinaPorChassi.TryGetValue(Chassi.Normalizar(chassiNaOrigem), out var maquina)
                ? maquina
                : null;

            var horimetro = itens.Select(i => i.Horimetro).FirstOrDefault(h => h is not null);
            if (horimetro is < 0 or > OrdemDeServico.HorimetroMaximo)
            {
                plano.HorimetroForaDaFaixa++;
                horimetro = null;
            }

            var servicos = servicosPorOs.GetValueOrDefault(chave, []);
            var valoresDosServicos = servicos.Select(RegrasDoValorDaOrdemDeServico.ValorDoServico).ToList();
            plano.ServicosSemValor += valoresDosServicos.Count(v => v is null);

            var pecas = itens.Where(i => i.EhPeca).Select(RegrasDoValorDaOrdemDeServico.ChaveDaPeca).Distinct(StringComparer.Ordinal).Count();
            var valorDasPecas = RegrasDoValorDaOrdemDeServico.ValorDasPecas(itens);
            var valorDosServicos = valoresDosServicos.Sum(v => v ?? 0m);

            var modelo = Limitar(itens.Select(i => i.Modelo).FirstOrDefault(m => m is not null), OrdemDeServico.TamanhoDoTexto);
            var tipo = Limitar(itens.Select(i => i.TipoDeAtendimento).FirstOrDefault(t => t is not null), OrdemDeServico.TamanhoDoTexto);
            var liberadaEm = itens.Select(i => i.LiberadaEm).FirstOrDefault(d => d is not null);
            var fechadaEm = itens.Select(i => i.FechadaEm).FirstOrDefault(d => d is not null);
            var canceladaEm = itens.Select(i => i.CanceladaEm).FirstOrDefault(d => d is not null);

            var hash = Resumir(chave, situacao.ToString(), Data(abertaEm), Data(liberadaEm), Data(fechadaEm), Data(canceladaEm), chassiNaOrigem,
                modelo, horimetro?.ToString(CultureInfo.InvariantCulture), tipo, documento,
                decimal.Round(valorDasPecas, 2).ToString(CultureInfo.InvariantCulture),
                decimal.Round(valorDosServicos, 2).ToString(CultureInfo.InvariantCulture),
                pecas.ToString(CultureInfo.InvariantCulture), servicos.Count.ToString(CultureInfo.InvariantCulture));

            var dados = new DadosDaOrdemDeServico(
                empresaId, grupo.Key.Numero, clienteId, equipamentoId, chassiNaOrigem, modelo, horimetro, situacao.Value, tipo,
                abertaEm.Value, liberadaEm, fechadaEm, canceladaEm, valorDasPecas, valorDosServicos, pecas, servicos.Count, hash);

            plano.Validas.Add((chave, dados));
            plano.Vistas.Add(chave);

            var mudou = true;
            if (!noCrm.TryGetValue(chave, out var existente)) plano.Novas++;
            else
            {
                if (existente.Excluida) plano.AReativar++;
                if (existente.Hash != hash || existente.EmpresaId != empresaId || existente.ClienteId != clienteId
                    || existente.EquipamentoId != equipamentoId) plano.AAtualizar++;
                else if (!existente.Excluida) { plano.Iguais++; mudou = false; }
            }

            if (mudou && ForaDoAlcanceCurto(alcance, abertaEm.Value, situacao.Value, fechadaEm, canceladaEm)) plano.CorrigidasForaDoCurto++;
        }

        plano.ServicosSemOrdem = servicosPorOs.Where(s => !chavesDasOrdens.Contains(s.Key)).Sum(s => s.Value.Count);

        // A EXCLUSÃO SÓ DENTRO DA JANELA: a OS fechada que envelheceu e saiu da leitura continua sendo história da máquina. NA
        // LEITURA CURTA a janela é só a das aberturas: a OS antiga, aberta aqui, que não veio pode ter sido fechada com a rotina
        // parada — quem a exclui ou a atualiza é a completa.
        var desde = alcance.Desde;
        bool NaJanela(NoCrm o) => o.AbertaEm >= desde || (plano.Completa && OrdemDeServico.EstaEmAbertoNa(o.Situacao));
        plano.VigentesNaJanela = noCrm.Values.Count(o => !o.Excluida && NaJanela(o));
        plano.AExcluir.AddRange(noCrm.Values.Where(o => !o.Excluida && NaJanela(o) && !plano.Vistas.Contains(o.Chave)).Select(o => o.Chave));
        if (plano.Completa)
            plano.CorrigidasForaDoCurto += noCrm.Values.Count(o => o.AbertaEm < alcance.InicioDaJanelaCurta && plano.AExcluir.Contains(o.Chave));
        return plano;
    }

    // =============================================================================================
    // A gravação — uma transação, executando o plano
    // =============================================================================================

    private async Task AplicarAsync(Plano plano, DateTime lidaEm, LeituraDasOrdensDeServico origem, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeClientesDoProtheus.CodigoDoSistema, "Protheus (TOTVS) — ERP", "SQL Server, somente leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var existentes = (await banco.OrdensDeServico.IgnoreQueryFilters().Where(o => o.SistemaId == sistemaId).ToListAsync(ct))
            .ToDictionary(o => o.ChaveNaOrigem, StringComparer.Ordinal);

        var gravadas = 0;
        foreach (var (chave, dados) in plano.Validas)
        {
            bool mudou;
            if (!existentes.TryGetValue(chave, out var existente))
            {
                banco.OrdensDeServico.Add(OrdemDeServico.Registrar(sistemaId, chave, dados, lidaEm, usuarioId));
                mudou = true;
            }
            else
            {
                var reativada = existente.Reativar(usuarioId);
                var atualizada = existente.AtualizarDaOrigem(dados, lidaEm, usuarioId);
                mudou = reativada || atualizada;
            }

            if (!mudou) continue;
            gravadas++;

            // EM BLOCOS: dezenas de milhares de OS numa gravação só seguram a memória do rastreador inteira.
            if (gravadas % 5000 == 0) await banco.SaveChangesAsync(ct);
        }

        foreach (var chave in plano.AExcluir)
        {
            existentes[chave].Excluir(usuarioId);
            gravadas++;
        }

        await banco.SaveChangesAsync(ct);

        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, OrdemDeServico.FluxoDaCarga, plano.Ordens, gravadas,
            plano.Recusadas + plano.SemFilial, ct, lidaEm.ToString("O", CultureInfo.InvariantCulture));

        // O PONTO DA COMPLETA NA MESMA TRANSAÇÃO: a completa que cai no meio não vira "última completa".
        if (plano.Completa) await RodadaCompleta.RegistrarAsync(banco, sistemaId, FluxoDaLeituraCompleta, lidaEm, plano.Ordens, gravadas, ct);

        await transacao.CommitAsync(ct);
        relatar($"Gravado ({gravadas:N0} OS novas, mudadas, reativadas ou excluídas; {origem.Itens.Count:N0} itens lidos). A sincronia pode " +
                "rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
    }

    // =============================================================================================
    // O relatório — contagens, e os totais em reais para conferir com o painel do BI
    // =============================================================================================

    private void Relatar(LeituraDasOrdensDeServico origem, Plano plano, DateOnly hoje)
    {
        Contar(EtapaDaLeitura, "linhas de item lidas (peças e serviços, com a capa repetida)", origem.Itens.Count);
        Contar(EtapaDaLeitura, "  delas, de peça", origem.Itens.Count(i => i.EhPeca));
        Contar(EtapaDaLeitura, "linhas de serviço executado lidas (VO4)", origem.Servicos.Count);
        Contar(EtapaDaLeitura, "  serviços de OS fora da janela ou sem capa (descartados)", plano.ServicosSemOrdem);
        Contar(EtapaDaLeitura, "  serviços cuja conta do BI dá nulo (não entram na soma, como no Qlik)", plano.ServicosSemValor);

        Contar(EtapaDasOrdens, RotuloDeOrdensLidas, plano.Ordens);
        Contar(EtapaDasOrdens, RotuloSemFilial, plano.SemFilial);
        Contar(EtapaDasOrdens, RotuloDeRecusadas, plano.Recusadas);
        foreach (var situacao in Enum.GetValues<SituacaoDaOrdemDeServico>())
            Contar(EtapaDasOrdens, $"  {situacao}", plano.Validas.Count(v => v.Dados.Situacao == situacao));
        Contar(EtapaDasOrdens, "  horímetro fora de 0–999.999 (gravado vazio)", plano.HorimetroForaDaFaixa);
        Contar(EtapaDasOrdens, "  chassi com mais de 40 caracteres (gravado vazio)", plano.ChassiForaDoTamanho);

        Contar(EtapaDosCasamentos, RotuloComCliente, plano.Validas.Count(v => v.Dados.ClienteId is not null));
        Contar(EtapaDosCasamentos, "  sem CPF/CNPJ do proprietário na view", plano.SemDocumento);
        Contar(EtapaDosCasamentos, "  proprietário que não é cliente do CRM", plano.ClienteForaDoCrm);
        Contar(EtapaDosCasamentos, "  documento de mais de um cliente do CRM (fica sem cliente)", plano.ClienteAmbiguo);
        Contar(EtapaDosCasamentos, RotuloComMaquina, plano.Validas.Count(v => v.Dados.EquipamentoId is not null));
        Contar(EtapaDosCasamentos, "clientes do CRM com OS", plano.Validas.Select(v => v.Dados.ClienteId).OfType<long>().Distinct().Count());
        Contar(EtapaDosCasamentos, "máquinas do CRM com OS", plano.Validas.Select(v => v.Dados.EquipamentoId).OfType<long>().Distinct().Count());

        Contar(EtapaDaSincronia, "vigentes na janela antes da rodada", plano.VigentesNaJanela);
        Contar(EtapaDaSincronia, RotuloDeNovas, plano.Novas);
        Contar(EtapaDaSincronia, RotuloDeAtualizadas, plano.AAtualizar);
        Contar(EtapaDaSincronia, RotuloDeIguais, plano.Iguais);
        Contar(EtapaDaSincronia, RotuloDeReativadas, plano.AReativar);
        Contar(EtapaDaSincronia, RotuloDeExcluidas, plano.AExcluir.Count);
        // A CONTA DA COMPLETA SAI ATÉ QUANDO É ZERO: "nenhuma OS corrigida" é a confirmação de que a leitura curta basta.
        if (plano.Completa) Contar(EtapaDaSincronia, RotuloCorrigidasForaDoCurto, plano.CorrigidasForaDoCurto);

        // OS TOTAIS EM REAIS, nos recortes do painel "Pós-Venda (Serviços)" do BI — é por eles que se confere a régua.
        var brasil = CultureInfo.GetCultureInfo("pt-BR");
        var emAberto = plano.Validas.Where(v => OrdemDeServico.EstaEmAbertoNa(v.Dados.Situacao)).Select(v => v.Dados).ToList();
        _observacoes.Add(string.Format(brasil,
            "Para conferir com o BI — OS abertas e liberadas: {0:N0}; peças R$ {1:N2} (\"Vlr Peças (A e L)\"); serviços R$ {2:N2} " +
            "(\"Vlr Srv c/desc (A e L)\"); mais de 45 dias abertas: {3:N0}.",
            emAberto.Count, emAberto.Sum(d => d.ValorDePecas), emAberto.Sum(d => d.ValorDeServicos),
            emAberto.Count(d => hoje.DayNumber - d.AbertaEm.DayNumber > 45)));

        foreach (var ano in plano.Validas.Select(v => v.Dados)
                     .Where(d => !OrdemDeServico.EstaEmAbertoNa(d.Situacao))
                     .GroupBy(d => d.AbertaEm.Year)
                     .OrderBy(g => g.Key))
            _observacoes.Add(string.Format(brasil,
                "Para conferir com o BI — OS fechadas e canceladas abertas em {0}: {1:N0}; peças R$ {2:N2} (\"Vlr Peças (F e C)\"); " +
                "serviços R$ {3:N2} (\"Vlr Srv c/desc (F e C)\").",
                ano.Key, ano.Count(), ano.Sum(d => d.ValorDePecas), ano.Sum(d => d.ValorDeServicos)));

        if (!plano.EsquemaExiste)
            _observacoes.Add("O banco lido ainda não tem a migração das ordens de serviço: o plano parte de nada gravado, que é o que o banco " +
                             "terá logo depois dela.");
    }

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>
    /// SE O BANCO JÁ TEM A MIGRAÇÃO. A simulação roda contra a produção ANTES da publicação que traz a tabela — é assim que
    /// se mede o que a rotina fará —, e lá ela ainda não existe. No SQLite dos testes o modelo é sempre o de hoje.
    /// </summary>
    private static async Task<bool> EsquemaExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'frota.OrdemDeServico')")
            .SingleAsync(ct) > 0;

    private static string? Limitar(string? texto, int tamanho) =>
        texto is null ? null : texto.Length <= tamanho ? texto : texto[..tamanho];

    private static string Data(DateOnly? data) => data?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>O resumo do conteúdo — o documento entra no cálculo e não sai dele: o resumo não é reversível.</summary>
    private static string Resumir(params string?[] partes) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\u001f', partes.Select(p => p ?? string.Empty)))));

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    private sealed record NoCrm(
        string Chave, string Hash, bool Excluida, int EmpresaId, long? ClienteId, long? EquipamentoId, DateOnly AbertaEm,
        SituacaoDaOrdemDeServico Situacao);

    private sealed class Plano
    {
        public bool EsquemaExiste { get; init; }
        public bool Completa { get; init; }
        public int CorrigidasForaDoCurto { get; set; }
        public int Ordens { get; set; }
        public int Recusadas { get; set; }
        public int SemFilial { get; set; }
        public int SemDocumento { get; set; }
        public int ClienteForaDoCrm { get; set; }
        public int ClienteAmbiguo { get; set; }
        public int ChassiForaDoTamanho { get; set; }
        public int HorimetroForaDaFaixa { get; set; }
        public int ServicosSemOrdem { get; set; }
        public int ServicosSemValor { get; set; }
        public int Novas { get; set; }
        public int AAtualizar { get; set; }
        public int Iguais { get; set; }
        public int AReativar { get; set; }
        public int VigentesNaJanela { get; set; }
        public List<string> AExcluir { get; } = [];
        public HashSet<string> Vistas { get; } = new(StringComparer.Ordinal);
        public List<(string Chave, DadosDaOrdemDeServico Dados)> Validas { get; } = [];
    }
}
