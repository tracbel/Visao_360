using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga dos financiamentos fez (ou faria, na simulação), em número — sem nome de ninguém.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDosFinanciamentos(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo.</summary>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// O FINANCIAMENTO DAS VENDAS, DOS FORMULÁRIOS DO VÓRTICE (issue 262, decisão do Ricardo em 28/09/2026) —
/// <c>--somente-financiamentos-vortice</c>, o terceiro modo da rotina <c>PROCESSOS_VORTICE</c>.
///
/// <para><b>Um por processo</b> (<see cref="LeitorDeFinanciamentosDoVortice.UmPorProcesso"/>). O processo cancelado não é
/// financiamento: não entra, e o que já estava gravado sai (excluído, nunca apagado). O que volta é reativado.</para>
///
/// <para><b>O município é o do cadastro do cliente</b>, casado pelo de-para de município da própria fonte
/// (<see cref="CorrespondenciaDeMunicipios"/>, fluxo <see cref="FluxoDasCidades"/>, chave <c>SeqCidade</c>). Fora de SP, ou
/// sem par, o financiamento entra sem município: conta no total, e não no share de filial nenhuma.</para>
///
/// <para><b>As travas das outras cargas</b>: mais de <see cref="FracaoMaximaDeRecusa"/> de linhas ilegíveis, ou excluir mais
/// de <see cref="FracaoMaximaDeRemocao"/> do que está vigente, aborta a rodada inteira. Só <c>--aceitar-queda</c>, no
/// terminal, passa por cima da segunda.</para>
/// </summary>
internal sealed class CargaDosFinanciamentosDoVortice(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<IReadOnlyList<FinanciamentoNoVortice>>>> ler,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>A trava de fluxo: uma rodada por vez.</summary>
    internal const string Fluxo = FinanciamentoDaVenda.FluxoDaCarga;

    /// <summary>O de-para das cidades do Vórtice com o catálogo.</summary>
    internal const string FluxoDasCidades = "VORTICE.CIDADE";

    /// <summary>A maior fração do que está vigente que uma rodada pode excluir sem abortar.</summary>
    internal const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>Abaixo disto a trava de remoção não se aplica.</summary>
    internal const int JanelaMinima = 50;

    /// <summary>A maior fração de linhas ilegíveis que uma rodada aceita.</summary>
    internal const double FracaoMaximaDeRecusa = 0.05;

    internal const string Etapa = "Financiamentos das vendas";
    internal const string RotuloDeLidas = "respostas lidas (com valor financiado)";
    internal const string RotuloDeProcessos = "processos (um financiamento por processo)";
    internal const string RotuloDeCancelados = "processos cancelados (não entram)";
    internal const string RotuloDeValorForaDoRazoavel = "valor acima de 20 milhões (digitação; não entra)";
    internal const string RotuloSemData = "sem data do pedido e sem data de preenchimento legível (não entra)";
    internal const string RotuloDataDoPreenchimento = "  com a data do preenchimento, por falta da do pedido";
    internal const string RotuloDeCredito = "  contam no crédito rural";
    internal const string RotuloForaDoCredito = "  recurso próprio, consórcio ou sem instituição (não contam no share)";
    internal const string RotuloForaDeSaoPaulo = "  fora de SP ou sem cidade no cadastro (sem município)";
    internal const string RotuloSemPar = "  cidade de SP sem par no catálogo (sem município; a correspondência fica para resolver)";
    internal const string RotuloDeNovas = "novos";
    internal const string RotuloDeRevisadas = "revisados pela origem";
    internal const string RotuloDeIguais = "sem mudança";
    internal const string RotuloDeReativadas = "voltaram à origem (reativados)";
    internal const string RotuloDeExcluidas = "saíram da leitura — cancelados ou apagados (excluídos, nunca apagados)";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Se a rodada excluiria demais — leitura parcial.</summary>
    internal static bool RemocaoPassaDaTrava(int vigentes, int aExcluir) =>
        vigentes >= JanelaMinima && aExcluir > vigentes * FracaoMaximaDeRemocao;

    /// <summary>Executa a sincronia.</summary>
    /// <param name="simular">Só planeja e conta.</param>
    /// <param name="aceitarQueda">Passa por cima da trava de remoção — só no terminal.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDosFinanciamentos>> ExecutarAsync(bool simular, bool aceitarQueda, CancellationToken ct)
    {
        relatar("Lendo os formulários da venda no Vórtice (só SELECT, NOLOCK)…");
        var lida = await ler(ct);
        if (!lida.EhSucesso) return Resultado<RelatorioDosFinanciamentos>.Indisponivel(lida.Erro!);
        if (lida.Valor.Count == 0)
            return Resultado<RelatorioDosFinanciamentos>.Indisponivel(
                "Os formulários vieram vazios. Isso é uma leitura a conferir — nada foi excluído e nada foi gravado.");

        var agora = relogio();
        var hoje = DateOnly.FromDateTime(agora.AddHours(-3));

        await using var banco = abrirContexto();
        var esquemaExiste = await EsquemaExisteAsync(banco, ct);
        var existentes = esquemaExiste
            ? (await banco.FinanciamentosDaVenda.AsNoTracking()
                    .Select(f => new { f.ProcessoNoVortice, f.HashDaOrigem, f.MunicipioId, f.ContaNoCreditoRural, f.ExcluidoEm })
                    .ToListAsync(ct))
                .ToDictionary(f => f.ProcessoNoVortice, f => (f.HashDaOrigem, f.MunicipioId, f.ContaNoCreditoRural, f.ExcluidoEm))
            : [];

        var dePara = await CorrespondenciaDeMunicipios.AbrirAsync(banco, FluxoDasCidades, "SP", usuarioId, ct);

        var processos = LeitorDeFinanciamentosDoVortice.UmPorProcesso(lida.Valor);
        var validos = new List<(long Processo, FinanciamentoDaVenda.Dados Dados)>(processos.Count);
        var recusas = new List<(object Conteudo, string Motivo)>();
        int cancelados = 0, foraDoRazoavel = 0, semData = 0, foraDeSp = 0, semPar = 0;

        foreach (var f in processos)
        {
            if (FinanciamentoDaVenda.ProcessoCancelado(f.Fase, f.Status))
            {
                cancelados++;
                continue;
            }

            if (f.Valor is not { } valor || valor <= 0 || valor > FinanciamentoDaVenda.ValorMaximo)
            {
                foraDoRazoavel++;
                continue;
            }

            if (DataDoPedido(f, hoje) is not { } data)
            {
                semData++;
                continue;
            }

            var (pedidoEm, doPreenchimento) = data;

            int? municipioId = null;
            if (!string.Equals(f.Uf?.Trim(), "SP", StringComparison.OrdinalIgnoreCase) || f.SeqCidade is not { } seqCidade || string.IsNullOrWhiteSpace(f.Cidade))
            {
                foraDeSp++;
            }
            else if (dePara.Resolver(seqCidade.ToString(CultureInfo.InvariantCulture), f.Cidade, agora, out var id))
            {
                municipioId = id;
            }
            else
            {
                semPar++;
                recusas.Add((new { SeqCidade = seqCidade, f.Cidade, f.Uf }, CorrespondenciaDeMunicipios.MotivoSemPar(f.Cidade)));
            }

            var hash = Resumir(f.Formulario, pedidoEm.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), doPreenchimento ? "P" : "V",
                valor.ToString(CultureInfo.InvariantCulture), f.Instituicao, f.Linha, f.SeqCidade?.ToString(CultureInfo.InvariantCulture));

            validos.Add((f.Processo, new FinanciamentoDaVenda.Dados(
                f.Formulario, pedidoEm, doPreenchimento, municipioId, valor, f.Instituicao, f.Linha, hash)));
        }

        // ---- o plano ----
        var vistos = validos.Select(v => v.Processo).ToHashSet();
        int novas = 0, revisadas = 0, iguais = 0, reativadas = 0;
        foreach (var (processo, dados) in validos)
        {
            if (!existentes.TryGetValue(processo, out var e))
            {
                novas++;
                continue;
            }

            if (e.ExcluidoEm is not null) reativadas++;
            var conta = FinanciamentoDaVenda.ContaComoCreditoRural(dados.LinhaDeCredito, dados.InstituicaoFinanceira);
            if (e.HashDaOrigem != dados.HashDaOrigem || e.MunicipioId != dados.MunicipioId || e.ContaNoCreditoRural != conta) revisadas++;
            else if (e.ExcluidoEm is null) iguais++;
        }

        var vigentes = existentes.Values.Count(e => e.ExcluidoEm is null);
        var aExcluir = existentes.Where(e => e.Value.ExcluidoEm is null && !vistos.Contains(e.Key)).Select(e => e.Key).ToList();

        Contar(RotuloDeLidas, lida.Valor.Count);
        Contar(RotuloDeProcessos, processos.Count);
        Contar(RotuloDeCancelados, cancelados);
        Contar(RotuloDeValorForaDoRazoavel, foraDoRazoavel);
        Contar(RotuloSemData, semData);
        Contar("financiamentos válidos", validos.Count);
        Contar(RotuloDataDoPreenchimento, validos.Count(v => v.Dados.DataDoPreenchimento));
        Contar(RotuloDeCredito, validos.Count(v => FinanciamentoDaVenda.ContaComoCreditoRural(v.Dados.LinhaDeCredito, v.Dados.InstituicaoFinanceira)));
        Contar(RotuloForaDoCredito, validos.Count(v => !FinanciamentoDaVenda.ContaComoCreditoRural(v.Dados.LinhaDeCredito, v.Dados.InstituicaoFinanceira)));
        Contar(RotuloForaDeSaoPaulo, foraDeSp);
        Contar(RotuloSemPar, semPar);
        Contar("  cidades casadas pelo par já gravado (respostas)", dePara.CasadosPelaCorrespondencia);
        Contar("  cidades casadas pelo nome nesta rodada (pares novos)", dePara.CasadosPorNome);
        Contar("vigentes antes da rodada", vigentes);
        Contar(RotuloDeNovas, novas);
        Contar(RotuloDeRevisadas, revisadas);
        Contar(RotuloDeIguais, iguais);
        Contar(RotuloDeReativadas, reativadas);
        Contar(RotuloDeExcluidas, aExcluir.Count);

        var ilegiveis = foraDoRazoavel + semData;
        if (processos.Count > 0 && ilegiveis > processos.Count * FracaoMaximaDeRecusa)
            return Resultado<RelatorioDosFinanciamentos>.Indisponivel(string.Create(CultureInfo.InvariantCulture,
                $"{ilegiveis} de {processos.Count} processos com valor ou data ilegível (acima de {FracaoMaximaDeRecusa:P0}). O formulário mudou de forma — A CARGA INTEIRA FOI ABORTADA: nada foi gravado."));

        if (RemocaoPassaDaTrava(vigentes, aExcluir.Count))
        {
            var texto = string.Create(CultureInfo.InvariantCulture,
                $"A rodada excluiria {aExcluir.Count} de {vigentes} financiamentos vigentes (acima de {FracaoMaximaDeRemocao:P0}) — leitura parcial?");
            if (!aceitarQueda)
                return Resultado<RelatorioDosFinanciamentos>.Indisponivel(
                    texto + " A CARGA INTEIRA FOI ABORTADA: nada foi gravado. Se a queda for real, rode no terminal com --aceitar-queda.");
            _observacoes.Add(texto + " Aceita por --aceitar-queda, no terminal.");
        }

        if (!esquemaExiste && !simular)
            return Resultado<RelatorioDosFinanciamentos>.Indisponivel(
                "O banco ainda não tem a migração dos financiamentos. Publique a versão com ela antes — nada foi gravado.");

        if (simular)
        {
            if (!esquemaExiste)
                _observacoes.Add("O banco lido ainda não tem a migração dos financiamentos: o plano parte de nada gravado.");
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura.");
            return Resultado<RelatorioDosFinanciamentos>.Ok(new RelatorioDosFinanciamentos(true, _contagens, _observacoes));
        }

        // ---- a gravação: uma transação ----
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);
        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeCargaDoVortice.CodigoDoSistema, "Vórtice CRM (sistema legado)", "SQL Server, somente leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var gravados = (await banco.FinanciamentosDaVenda.ToListAsync(ct)).ToDictionary(f => f.ProcessoNoVortice);
        foreach (var (processo, dados) in validos)
        {
            if (!gravados.TryGetValue(processo, out var existente))
            {
                banco.FinanciamentosDaVenda.Add(FinanciamentoDaVenda.Registrar(processo, dados, usuarioId, agora));
                continue;
            }

            existente.Reativar();
            existente.AtualizarDaOrigem(dados, usuarioId, agora);
        }

        foreach (var processo in aExcluir) gravados[processo].Excluir(agora);

        dePara.Gravar(banco);
        await CargaDeTerritorio.SubstituirRecusasAsync(banco, FluxoDasCidades, recusas, ct);
        await banco.SaveChangesAsync(ct);

        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, Fluxo, lida.Valor.Count,
            novas + revisadas + reativadas + aExcluir.Count, ilegiveis, ct);

        await transacao.CommitAsync(ct);
        relatar("Gravado. A sincronia pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
        return Resultado<RelatorioDosFinanciamentos>.Ok(new RelatorioDosFinanciamentos(false, _contagens, _observacoes));
    }

    /// <summary>
    /// A DATA DO FINANCIAMENTO: a do pedido, quando é plausível (de 2012 até um ano adiante); senão, a do preenchimento do
    /// formulário. Os anos 1900, 2045, 2106 e 2301 foram vistos no campo do pedido em 28/09/2026 — é digitação.
    /// </summary>
    /// <param name="f">O financiamento.</param>
    /// <param name="hoje">O dia de hoje, em São Paulo.</param>
    internal static (DateOnly PedidoEm, bool DoPreenchimento)? DataDoPedido(FinanciamentoNoVortice f, DateOnly hoje)
    {
        bool Plausivel(DateTime data) => data.Year >= FinanciamentoDaVenda.PrimeiroAno && DateOnly.FromDateTime(data) <= hoje.AddYears(1);

        if (f.PedidoEm is { } pedido && Plausivel(pedido)) return (DateOnly.FromDateTime(pedido), false);
        if (f.PreenchidoEm is { } preenchido && Plausivel(preenchido)) return (DateOnly.FromDateTime(preenchido), true);
        return null;
    }

    private static async Task<bool> EsquemaExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'organizacao.FinanciamentoDaVenda')")
            .SingleAsync(ct) > 0;

    private void Contar(string rotulo, int valor)
    {
        _contagens.Add((Etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    private static string Resumir(params string?[] partes) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\u001f', partes.Select(p => p ?? string.Empty)))));
}
