namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A leitura da base de preços de mercado (issue 66).
/// </summary>
public interface IRepositorioDePrecosDeMercado
{
    /// <summary>
    /// Todas as séries de preço de São Paulo, mês a mês, com o valor em reais e em dólares.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    Task<PrecosDeMercado> LerAsync(CancellationToken ct);
}

/// <summary>A base de preços inteira, como a tela a mostra.</summary>
/// <param name="Series">Uma série por produto, fonte e nível.</param>
/// <param name="PrimeiroMesDoDolar">O mês mais antigo com dólar PTAX gravado.</param>
/// <param name="UltimoMesDoDolar">O mês mais recente com dólar PTAX gravado.</param>
public sealed record PrecosDeMercado(
    IReadOnlyList<SerieDePreco> Series,
    DateOnly? PrimeiroMesDoDolar,
    DateOnly? UltimoMesDoDolar);

/// <summary>
/// UMA SÉRIE DE PREÇO — um produto, numa fonte, num nível, mês a mês.
///
/// <para>O valor vem na unidade da FONTE (<see cref="Unidade"/>) e também na unidade em que o
/// mercado o negocia (<see cref="UnidadeComercial"/>): a CONAB publica o café a R$ 31,57/kg, e o
/// produtor pensa em R$ 1.894,20 a saca.</para>
/// </summary>
/// <param name="Fonte">Quem publicou (<c>CONAB</c>, <c>SOCICANA</c>).</param>
/// <param name="CodigoNaFonte">O código do produto na fonte.</param>
/// <param name="Nivel">O nível do preço.</param>
/// <param name="Produto">O nome do produto.</param>
/// <param name="Classificacao">A classificação mais recente.</param>
/// <param name="Unidade">A unidade da fonte.</param>
/// <param name="UnidadeComercial">A unidade de negociação (<c>saca de 60 kg</c>).</param>
/// <param name="FatorComercial">Quantas unidades da fonte cabem numa unidade comercial.</param>
/// <param name="Meses">Os meses, do mais antigo ao mais recente.</param>
/// <param name="Procedencia">De onde esta série veio e até quando ela vai (issue 167).</param>
public sealed record SerieDePreco(
    string Fonte,
    string CodigoNaFonte,
    string Nivel,
    string Produto,
    string Classificacao,
    string Unidade,
    string UnidadeComercial,
    decimal FatorComercial,
    IReadOnlyList<PrecoNoMes> Meses,
    Tracbel.Crm.Dominio.Mercado.ProcedenciaDoIndicador? Procedencia = null);

/// <summary>O preço de um mês.</summary>
/// <param name="Mes">O mês (dia 1).</param>
/// <param name="ValorEmReais">Na unidade da fonte.</param>
/// <param name="ValorEmDolares">Na unidade da fonte, pelo PTAX do mês; nulo quando o mês ainda não tem dólar.</param>
public sealed record PrecoNoMes(DateOnly Mes, decimal ValorEmReais, decimal? ValorEmDolares);
