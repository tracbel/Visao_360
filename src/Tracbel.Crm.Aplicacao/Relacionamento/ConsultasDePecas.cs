using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>Uma fatia dos doze meses de peças (um setor ou um grupo comercial).</summary>
/// <param name="Nome">O setor ou o grupo, como o Protheus e o BI escrevem.</param>
/// <param name="Valor">O valor líquido.</param>
public sealed record FatiaDasPecas(string Nome, decimal Valor);

/// <summary>Um mês da série de peças.</summary>
/// <param name="Competencia">O mês, no dia 1.</param>
/// <param name="Valor">O valor líquido.</param>
public sealed record MesDasPecas(DateOnly Competencia, decimal Valor);

/// <summary>
/// As peças de um cliente, resumidas para a ficha: os doze meses (com balcão × oficina e o grupo comercial), a série, o
/// vendedor principal e os orçamentos em aberto.
/// </summary>
/// <param name="De">O primeiro mês dos doze.</param>
/// <param name="Ate">O último mês dos doze (o corrente).</param>
/// <param name="DozeMeses">O valor líquido dos doze meses, com as devoluções descontadas.</param>
/// <param name="DevolucoesNosDozeMeses">Quanto disso é devolução.</param>
/// <param name="DescontoNosDozeMeses">O desconto concedido.</param>
/// <param name="ItensNosDozeMeses">Os itens de nota.</param>
/// <param name="PorSetor">Os doze meses por setor (balcão, oficina…), do maior para o menor.</param>
/// <param name="PorGrupo">Os doze meses por grupo comercial, do maior para o menor.</param>
/// <param name="Serie">Os doze meses, mês a mês — o mês sem compra entra com zero.</param>
/// <param name="UltimaCompraEm">O mês da compra mais recente nos três anos da carga.</param>
/// <param name="VendedorPrincipal">Quem mais vendeu peça ao cliente nos doze meses.</param>
/// <param name="OrcamentosEmAberto">Quantos orçamentos esperam o cliente.</param>
/// <param name="ValorEmOrcamentosAbertos">O total deles.</param>
/// <param name="OrcamentosVencidos">Quantos dos abertos já passaram da validade.</param>
/// <param name="Orcamentos">Os orçamentos em aberto, os que vencem antes primeiro (no máximo dez).</param>
/// <param name="CarregadoEm">A última apuração do faturamento de peças (UTC).</param>
/// <param name="OrcamentosCarregadosEm">A última sincronia dos orçamentos (UTC).</param>
/// <param name="MetricasSemDado">O que a ficha pediu e não pôde mostrar, com o motivo.</param>
public sealed record PecasDoClienteResumidas(
    DateOnly De,
    DateOnly Ate,
    decimal DozeMeses,
    decimal DevolucoesNosDozeMeses,
    decimal DescontoNosDozeMeses,
    int ItensNosDozeMeses,
    IReadOnlyList<FatiaDasPecas> PorSetor,
    IReadOnlyList<FatiaDasPecas> PorGrupo,
    IReadOnlyList<MesDasPecas> Serie,
    DateOnly? UltimaCompraEm,
    string? VendedorPrincipal,
    int OrcamentosEmAberto,
    decimal ValorEmOrcamentosAbertos,
    int OrcamentosVencidos,
    IReadOnlyList<OrcamentoDePecasParaConsulta> Orcamentos,
    DateTime? CarregadoEm,
    DateTime? OrcamentosCarregadosEm,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// AS PEÇAS DO CLIENTE NA FICHA (02/10/2026) — o faturamento de peças por mês e os orçamentos que a rotina 15
/// <c>POS_VENDA_PROTHEUS</c> traz das views do BI. Hoje a ficha só sabia, pelo faturamento do cliente, quanto da nota é peça;
/// agora sabe balcão × oficina, o grupo comercial (peças, pneus, lubrificantes…), o vendedor e o que está orçado.
///
/// <para><b>Os doze meses ancoram no mês corrente</b>, que ainda está em curso; o painel do BI faz o mesmo. A cliente que
/// comprava e parou aparece pelo mês da última compra, nos três anos que a carga mantém.</para>
/// </summary>
/// <param name="repositorio">O acesso às peças.</param>
/// <param name="relogio">O relógio.</param>
public sealed class ObterPecasDoCliente(IRepositorioDePecas repositorio, IRelogio relogio)
{
    /// <summary>Quantos orçamentos a lista traz no máximo.</summary>
    public const int OrcamentosNaLista = 10;

    /// <summary>Executa a leitura.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PecasDoClienteResumidas>>> ExecutarAsync(Guid chave, CancellationToken ct)
    {
        var hoje = DateOnly.FromDateTime(relogio.Agora);
        var mesCorrente = new DateOnly(hoje.Year, hoje.Month, 1);
        var de = mesCorrente.AddMonths(-11);

        var lidas = await repositorio.DoClienteAsync(chave, mesCorrente.AddYears(-3), ct);
        if (lidas is null)
            return Resultado<ComProcedencia<PecasDoClienteResumidas>>.NaoEncontrado($"Não há cliente {chave} ao seu alcance.");

        var ausentes = new List<MetricaSemDado>();
        if (lidas.FaturamentoCarregadoEm is null)
            ausentes.Add(new MetricaSemDado(
                "pecas",
                "O faturamento de peças ainda não foi carregado: ele vem das views do BI no banco do Protheus, pelo modo de peças da " +
                "rotina POS_VENDA_PROTHEUS, que nasce desligada."));
        else if (lidas.Faturamento.Count == 0)
            ausentes.Add(new MetricaSemDado(
                "pecasDoCliente",
                "Nenhuma compra de peça deste cliente nos últimos três anos, nas filiais ao seu alcance. A nota entra aqui quando o " +
                "CPF/CNPJ do cliente, no Protheus, é o de um cliente do CRM."));
        if (lidas.OrcamentosCarregadosEm is null)
            ausentes.Add(new MetricaSemDado(
                "orcamentosDePecas",
                "Os orçamentos de peças ainda não foram carregados — vêm com o mesmo modo da rotina POS_VENDA_PROTHEUS."));

        var doze = lidas.Faturamento.Where(f => f.Competencia >= de && f.Competencia <= mesCorrente).ToList();

        static IReadOnlyList<FatiaDasPecas> Fatias(IEnumerable<IGrouping<string, PecasDoClienteNoMes>> grupos) =>
            [.. grupos.Select(g => new FatiaDasPecas(g.Key, g.Sum(f => f.ValorLiquido)))
                .Where(f => f.Valor != 0)
                .OrderByDescending(f => f.Valor)
                .ThenBy(f => f.Nome, StringComparer.Ordinal)];

        var serie = new List<MesDasPecas>(12);
        for (var mes = de; mes <= mesCorrente; mes = mes.AddMonths(1))
            serie.Add(new MesDasPecas(mes, doze.Where(f => f.Competencia == mes).Sum(f => f.ValorLiquido)));

        var comCompra = lidas.Faturamento.Where(f => f.ValorLiquido > 0).ToList();
        var vendedor = doze.Where(f => f.VendedorNome is not null)
            .GroupBy(f => f.VendedorNome!, StringComparer.Ordinal)
            .Select(g => (Nome: g.Key, Valor: g.Sum(f => f.ValorLiquido)))
            .Where(v => v.Valor > 0)
            .OrderByDescending(v => v.Valor)
            .Select(v => v.Nome)
            .FirstOrDefault();

        var abertos = lidas.Orcamentos.Where(o => OrcamentoDePecas.EstaEmAbertoNa(o.Situacao)).ToList();

        var resumo = new PecasDoClienteResumidas(
            de,
            mesCorrente,
            doze.Sum(f => f.ValorLiquido),
            doze.Sum(f => f.ValorDeDevolucoes),
            doze.Sum(f => f.ValorDeDesconto),
            doze.Sum(f => f.Itens),
            Fatias(doze.GroupBy(f => f.Setor, StringComparer.Ordinal)),
            Fatias(doze.GroupBy(f => f.Grupo, StringComparer.Ordinal)),
            serie,
            comCompra.Count == 0 ? null : comCompra.Max(f => f.Competencia),
            vendedor,
            abertos.Count,
            abertos.Sum(o => o.ValorTotal),
            abertos.Count(o => o.ValidoAte < hoje),
            [.. abertos.OrderBy(o => o.ValidoAte ?? DateOnly.MaxValue).ThenByDescending(o => o.ValorTotal).Take(OrcamentosNaLista)],
            lidas.FaturamentoCarregadoEm,
            lidas.OrcamentosCarregadosEm,
            ausentes);

        return Resultado<ComProcedencia<PecasDoClienteResumidas>>.Ok(
            ComProcedencia<PecasDoClienteResumidas>.DoNossoBanco(resumo, "comercial.FaturamentoDePecasNoMes", relogio));
    }
}
