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
/// <param name="Maquinas">
/// O preço de cada categoria de máquina, mês a mês — a mediana das notas de venda do Protheus (issue 70, D-P12). É a
/// outra metade do termo de troca: sacas de cada cultura para comprar um trator. Vazio enquanto a rotina
/// <c>PRECOS_DE_MAQUINA</c> não rodou.
/// </param>
public sealed record PrecosDeMercado(
    IReadOnlyList<SerieDePreco> Series,
    DateOnly? PrimeiroMesDoDolar,
    DateOnly? UltimoMesDoDolar,
    IReadOnlyList<SerieDePrecoDeMaquina>? Maquinas = null);

/// <summary>
/// O PREÇO DE UMA CATEGORIA DE MÁQUINA, MÊS A MÊS (issue 70, D-P12) — a mediana do valor unitário das notas de venda
/// do Protheus casadas com as vendas do ART. Só o agregado: o valor de cada venda fica no ERP.
/// </summary>
/// <param name="CategoriaCodigo">A categoria (<c>TRATOR</c>, <c>COLHEITADEIRA</c>…).</param>
/// <param name="CategoriaNome">O nome de exibição.</param>
/// <param name="Meses">Os meses com nota, do mais antigo ao mais recente — mês sem venda não aparece.</param>
/// <param name="Procedencia">De onde a série veio e até quando ela vai (issue 167).</param>
public sealed record SerieDePrecoDeMaquina(
    string CategoriaCodigo,
    string CategoriaNome,
    IReadOnlyList<PrecoDeMaquinaLido> Meses,
    Tracbel.Crm.Dominio.Mercado.ProcedenciaDoIndicador? Procedencia);

/// <summary>O preço de uma categoria num mês.</summary>
/// <param name="Mes">O mês da emissão (dia 1).</param>
/// <param name="Mediana">A mediana do valor unitário, em reais.</param>
/// <param name="Menor">O menor valor unitário.</param>
/// <param name="Maior">O maior valor unitário.</param>
/// <param name="Notas">Quantas notas entraram — o tamanho da base.</param>
public sealed record PrecoDeMaquinaLido(DateOnly Mes, decimal Mediana, decimal Menor, decimal Maior, int Notas);

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
