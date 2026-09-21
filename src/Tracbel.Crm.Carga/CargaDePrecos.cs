using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.BancoCentral;
using Tracbel.Crm.Integracao.Conab;
using Tracbel.Crm.Integracao.Socicana;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CARGA DOS PREÇOS DE MERCADO — a base que "não varia, só vamos acrescentando" (issue 66).
///
/// <para>Três fontes abertas, cada uma na sua transação:</para>
/// <list type="number">
///   <item>o preço recebido pelo produtor em São Paulo, por produto e mês — CONAB;</item>
///   <item>o preço do kg de ATR da cana, mensal e acumulado da safra — Socicana;</item>
///   <item>o dólar PTAX mensal, para converter — Banco Central.</item>
/// </list>
///
/// <para><b>Nada é apagado.</b> O mês que saiu da janela de 12 meses da CONAB fica. O mês que a fonte
/// revisou é atualizado, e a trilha de auditoria guarda o valor anterior.</para>
///
/// <para><b>Reexecutável.</b> Rodar duas vezes no mesmo dia não grava nada na segunda: o mês igual não
/// é tocado, nem no carimbo.</para>
///
/// <para><b>Mensal.</b> É a rotina <c>TracbelCrmPrecos</c> do servidor, que roda todo dia 20 — a CONAB
/// fecha o mês anterior na primeira quinzena.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="conab">A leitura dos preços da CONAB.</param>
/// <param name="socicana">A leitura do preço do kg de ATR.</param>
/// <param name="ptax">A leitura do dólar PTAX.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDePrecos(
    Func<CrmDbContext> abrirContexto,
    LeitorDaConab conab,
    LeitorDaSocicana socicana,
    LeitorDoPtax ptax,
    long usuarioId,
    Action<string> relatar)
{
    /// <summary>A praça de todas as séries: o estado de São Paulo.</summary>
    public const string SaoPaulo = "SP";

    /// <summary>O nome da fonte da CONAB, gravado em cada cotação.</summary>
    public const string FonteConab = "CONAB";

    /// <summary>O nome da fonte da Socicana, gravado em cada cotação.</summary>
    public const string FonteSocicana = "SOCICANA";

    /// <summary>O nível gravado nas cotações da CONAB.</summary>
    public const string NivelRecebido = "RECEBIDO PELO PRODUTOR";

    /// <summary>O nível do preço do mês, na Socicana.</summary>
    public const string NivelMensal = "MENSAL";

    /// <summary>O nível do preço acumulado da safra, na Socicana.</summary>
    public const string NivelAcumulado = "ACUMULADO DA SAFRA";

    private const string FluxoDaConab = "CONAB.PRECO_RECEBIDO";
    private const string FluxoDaSocicana = "SOCICANA.PRECO_DO_ATR";
    private const string FluxoDoPtax = "BCB.PTAX_MENSAL";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];

    /// <summary>Executa as três etapas.</summary>
    /// <param name="ct">Cancelamento.</param>
    /// <returns>As contagens de cada etapa, na ordem em que aconteceram.</returns>
    public async Task<IReadOnlyList<(string Etapa, string Rotulo, int Valor)>> ExecutarAsync(CancellationToken ct)
    {
        await CarregarConabAsync(ct);
        await CarregarSocicanaAsync(ct);
        await CarregarPtaxAsync(ct);
        return _contagens;
    }

    // =============================================================================================
    // 1. CONAB — preço recebido pelo produtor
    // =============================================================================================

    private async Task CarregarConabAsync(CancellationToken ct)
    {
        const string etapa = "Preço recebido pelo produtor em SP (CONAB)";

        await using var trava = await TravaDeFluxo.TomarAsync(abrirContexto(), FluxoDaConab, ct);

        relatar("Lendo os preços mensais da CONAB (preço recebido pelo produtor, SP)…");
        var lidas = await conab.LerPrecosMensaisAsync(SaoPaulo, LeitorDaConab.NivelRecebidoPeloProdutor, ct);

        var recusas = new List<(object Conteudo, string Motivo)>();
        var validas = new List<(LinhaDaCotacaoDaConab Linha, DateOnly Mes, decimal Valor)>();

        foreach (var linha in lidas)
        {
            try
            {
                validas.Add((linha, new DateOnly(linha.Ano, linha.Mes, 1), LeitorDaConab.ValorPorKg(linha.ValorPorKgBruto)));
            }
            catch (Exception erro) when (erro is FormatException or ArgumentOutOfRangeException)
            {
                recusas.Add((linha, erro.Message));
            }
        }

        var gravadas = await GravarCotacoesAsync(
            FluxoDaConab, FonteConab, "CONAB — preços agropecuários (dados abertos)",
            validas.Select(v => (v.Linha.CodigoDoProduto, NivelRecebido, v.Linha.Produto, v.Linha.Classificacao, "kg", v.Mes, v.Valor)),
            recusas, lidas.Count, ct);

        Contar(etapa, $"linhas lidas ({Periodo(validas.Select(v => v.Mes))})", lidas.Count);
        Contar(etapa, "produtos distintos", validas.Select(v => v.Linha.CodigoDoProduto).Distinct().Count());
        ContarGravacao(etapa, gravadas, recusas.Count);
    }

    // =============================================================================================
    // 2. Socicana — preço do kg de ATR
    // =============================================================================================

    private async Task CarregarSocicanaAsync(CancellationToken ct)
    {
        const string etapa = "Preço do kg de ATR da cana (Socicana)";

        await using var trava = await TravaDeFluxo.TomarAsync(abrirContexto(), FluxoDaSocicana, ct);

        relatar("Lendo o preço do kg de ATR da Socicana, todas as safras…");
        var lidas = await socicana.LerAsync(ct);

        var recusas = new List<(object Conteudo, string Motivo)>();
        var validas = new List<(string Nivel, DateOnly Mes, decimal Valor, string Safra)>();

        foreach (var linha in lidas)
        {
            try
            {
                validas.Add((NivelMensal, linha.Mes, LeitorDaSocicana.Valor(linha.MensalBruto), linha.Safra));
                validas.Add((NivelAcumulado, linha.Mes, LeitorDaSocicana.Valor(linha.AcumuladoBruto), linha.Safra));
            }
            catch (FormatException erro)
            {
                recusas.Add((linha, erro.Message));
            }
        }

        var gravadas = await GravarCotacoesAsync(
            FluxoDaSocicana, FonteSocicana, "Socicana — preço do kg de ATR (página pública)",
            validas.Select(v => ("ATR", v.Nivel, "CANA DE AÇÚCAR", $"ATR — safra {v.Safra}", "kg de ATR", v.Mes, v.Valor)),
            recusas, lidas.Count, ct);

        Contar(etapa, $"meses lidos ({Periodo(validas.Select(v => v.Mes))})", lidas.Count);
        Contar(etapa, "safras distintas", validas.Select(v => v.Safra).Distinct().Count());
        ContarGravacao(etapa, gravadas, recusas.Count);
    }

    // =============================================================================================
    // 3. Banco Central — dólar PTAX mensal
    // =============================================================================================

    private async Task CarregarPtaxAsync(CancellationToken ct)
    {
        const string etapa = "Dólar PTAX mensal (Banco Central)";

        await using var trava = await TravaDeFluxo.TomarAsync(abrirContexto(), FluxoDoPtax, ct);

        relatar("Lendo o dólar PTAX mensal do Banco Central (SGS 3698)…");
        var lidas = await ptax.LerAsync(LeitorDoPtax.InicioDaSerie, ct);

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            contexto, "BCB", "Banco Central — SGS", "REST público, somente leitura", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var existentes = (await contexto.CotacoesDoDolar.ToListAsync(ct)).ToDictionary(c => c.Mes);

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<CotacaoDoDolar>();
        var vistos = new HashSet<DateOnly>();
        int revisadas = 0, mantidas = 0;

        foreach (var linha in lidas)
        {
            DateOnly mes;
            decimal valor;
            try
            {
                mes = LeitorDoPtax.Mes(linha.DataBruta);
                valor = LeitorDoPtax.ReaisPorDolar(linha.ValorBruto);
            }
            catch (FormatException erro)
            {
                recusas.Add((linha, erro.Message));
                continue;
            }

            if (!vistos.Add(mes))
            {
                recusas.Add((linha, $"O mês {mes:MM/yyyy} veio duas vezes. Vale a primeira."));
                continue;
            }

            if (existentes.TryGetValue(mes, out var existente))
            {
                if (existente.Revisar(valor, usuarioId, agora)) revisadas++;
                else mantidas++;
            }
            else
            {
                novas.Add(CotacaoDoDolar.Registrar(mes, valor, usuarioId, agora));
            }
        }

        contexto.CotacoesDoDolar.AddRange(novas);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDoPtax, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var periodo = Periodo(vistos);
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDoPtax, lidas.Count, novas.Count + revisadas, recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"meses lidos ({periodo})", lidas.Count);
        ContarGravacao(etapa, (novas.Count, revisadas, mantidas), recusas.Count);
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>
    /// Grava as cotações de uma fonte numa transação: insere o mês novo, revisa o que mudou, não toca
    /// no igual e <b>nunca apaga</b> o que deixou de vir.
    /// </summary>
    private async Task<(int Novas, int Revisadas, int Mantidas)> GravarCotacoesAsync(
        string fluxo,
        string fonte,
        string nomeDoSistema,
        IEnumerable<(string Codigo, string Nivel, string Produto, string Classificacao, string Unidade, DateOnly Mes, decimal Valor)> cotacoes,
        List<(object Conteudo, string Motivo)> recusas,
        int lidas,
        CancellationToken ct)
    {
        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            contexto, fonte, nomeDoSistema, "Arquivo público, somente leitura", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var existentes = (await contexto.CotacoesDeProdutos
                .Where(c => c.Fonte == fonte && c.Praca == SaoPaulo)
                .ToListAsync(ct))
            .ToDictionary(c => (c.CodigoNaFonte, c.Nivel, c.Mes));

        var novas = new List<CotacaoDeProduto>();
        var vistas = new HashSet<(string, string, DateOnly)>();
        int revisadas = 0, mantidas = 0;

        foreach (var c in cotacoes)
        {
            var chave = (c.Codigo, c.Nivel, c.Mes);

            if (!vistas.Add(chave))
            {
                // OBJETO ANÔNIMO, e não a tupla: o serializador não grava campo de tupla, e a recusa
                // chegaria ao banco como "{}" — sem dizer o que foi recusado.
                recusas.Add((new { c.Codigo, c.Nivel, c.Produto, c.Classificacao, Mes = c.Mes.ToString("yyyy-MM"), c.Valor },
                    $"{c.Produto} ({c.Codigo}, {c.Nivel}) veio duas vezes para {c.Mes:MM/yyyy}. Vale a primeira."));
                continue;
            }

            if (existentes.TryGetValue(chave, out var existente))
            {
                if (existente.Revisar(c.Produto, c.Classificacao, c.Valor, usuarioId, agora)) revisadas++;
                else mantidas++;
            }
            else
            {
                novas.Add(CotacaoDeProduto.Registrar(
                    fonte, c.Codigo, SaoPaulo, c.Nivel, c.Produto, c.Classificacao, c.Unidade,
                    c.Mes, c.Valor, usuarioId, agora));
            }
        }

        contexto.CotacoesDeProdutos.AddRange(novas);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, fluxo, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, fluxo, lidas, novas.Count + revisadas, recusas.Count, ct,
            Periodo(vistas.Select(v => v.Item3)));
        await transacao.CommitAsync(ct);

        return (novas.Count, revisadas, mantidas);
    }

    private void ContarGravacao(string etapa, (int Novas, int Revisadas, int Mantidas) g, int recusadas)
    {
        Contar(etapa, "meses novos", g.Novas);
        Contar(etapa, "meses revisados pela fonte (valor anterior na trilha)", g.Revisadas);
        Contar(etapa, "meses mantidos sem mudança", g.Mantidas);
        Contar(etapa, "linhas recusadas", recusadas);
    }

    /// <summary>O rótulo do período de uma rodada, para o ponto de sincronismo.</summary>
    private static string Periodo(IEnumerable<DateOnly> meses)
    {
        var lista = meses.ToList();
        return lista.Count == 0 ? "sem linhas" : $"{lista.Min():MM/yyyy} a {lista.Max():MM/yyyy}";
    }

    private void Contar(string etapa, string rotulo, int valor) => _contagens.Add((etapa, rotulo, valor));
}
