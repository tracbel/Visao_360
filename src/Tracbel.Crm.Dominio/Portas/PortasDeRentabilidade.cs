using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A RENTABILIDADE POR CULTURA (issue 159) — receita, custo e margem por hectare, com a competência de
/// cada parte.
/// </summary>
public interface IRepositorioDeRentabilidade
{
    /// <summary>A rentabilidade de cada cultura ativa do catálogo.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<RentabilidadeDaCultura>> LerAsync(CancellationToken ct);
}

/// <summary>
/// A RENTABILIDADE DE UMA CULTURA, com o rastro de cada parcela.
///
/// <para><b>As três competências andam juntas do número</b>: o ano da PAM que deu a produtividade, os
/// meses de preço que entraram na média e a safra do custo. Elas costumam ser diferentes, e um número
/// que não diz de quando é não pode ser conferido.</para>
///
/// <para><b>Margem nula vem com <see cref="Motivo"/>.</b> É a regra que atravessa a Inteligência de
/// Mercado: dado ausente ou parâmetro não decidido devolve vazio com a frase, nunca um número inventado.</para>
/// </summary>
/// <param name="CulturaCodigo">O código da cultura no catálogo.</param>
/// <param name="CulturaNome">O nome de exibição.</param>
/// <param name="UnidadeComercial">A unidade em que o mercado negocia — só para mostrar.</param>
/// <param name="AnoDaProdutividade">O ano da PAM de onde veio a produtividade.</param>
/// <param name="ProdutividadeKgPorHa">Quilos por hectare colhido; nula quando a PAM não permite calcular.</param>
/// <param name="PrecoMedioPorKg">A média dos meses de preço do ano, em R$/kg.</param>
/// <param name="MesesDePrecoNaMedia">Quantos meses entraram na média.</param>
/// <param name="ReceitaPorHectare">Produtividade × preço.</param>
/// <param name="LocalDoCusto">O local de referência da CONAB; nulo enquanto ninguém decide (D-P07).</param>
/// <param name="CamadaDoCusto">Operacional ou Total; nula enquanto ninguém decide.</param>
/// <param name="SafraDoCusto">A safra da série de custo usada.</param>
/// <param name="CustoPorHectare">O custo por hectare na camada escolhida.</param>
/// <param name="MargemPorHectare">Receita menos custo.</param>
/// <param name="MargemPorUnidade">
/// A mesma margem na unidade em que o mercado negocia — por saca, caixa ou tonelada (issue 73). A margem
/// por hectare responde "a terra paga a conta?"; esta responde "cada saca que eu vendo sobra quanto?".
/// </param>
/// <param name="AreaColhidaHectares">A área colhida de São Paulo, que multiplica a margem.</param>
/// <param name="MargemTotal">Margem por hectare × área colhida.</param>
/// <param name="Motivo">Por que a margem não saiu, como TEXTO; <c>Nenhum</c> quando saiu — a convenção do contrato é enum em texto.</param>
/// <param name="FraseDoMotivo">A frase que a tela mostra no lugar do número.</param>
/// <param name="Tendencia">
/// A MESMA CULTURA UM ANO ANTES, para a tendência da margem e a variação do custo (28/09/2026). Nula quando a PAM não
/// tem o ano anterior.
/// </param>
public sealed record RentabilidadeDaCultura(
    string CulturaCodigo,
    string CulturaNome,
    string UnidadeComercial,
    short? AnoDaProdutividade,
    decimal? ProdutividadeKgPorHa,
    decimal? PrecoMedioPorKg,
    int MesesDePrecoNaMedia,
    decimal? ReceitaPorHectare,
    string? LocalDoCusto,
    string? CamadaDoCusto,
    short? SafraDoCusto,
    decimal? CustoPorHectare,
    decimal? MargemPorHectare,
    decimal? MargemPorUnidade,
    decimal? AreaColhidaHectares,
    decimal? MargemTotal,
    string Motivo,
    string FraseDoMotivo,
    TendenciaDaRentabilidade? Tendencia = null);

/// <summary>
/// A RENTABILIDADE DE UM ANO CONTRA A DO ANTERIOR, na MESMA RÉGUA (28/09/2026).
///
/// <para><b>O preço é o recebido pelo produtor, da PAM</b> — valor da produção ÷ quantidade —, nos dois anos. A margem da
/// tela usa a média dos meses da CONAB, mas a CONAB guarda só a janela de 12 meses: não há preço dela para o ano anterior.
/// Comparar a CONAB de um ano com a PAM do outro mediria a troca de fonte, e não o mercado; por isso a tendência
/// compara PAM com PAM, e diz isso.</para>
///
/// <para><b>O custo de cada ano é a safra mais recente até ele</b>, no local de referência — a mesma regra da margem.
/// Quando a CONAB não publicou safra nova, os dois anos usam a mesma, e a variação do custo é zero, dita como tal.</para>
/// </summary>
/// <param name="Ano">O ano mais recente da PAM da cultura.</param>
/// <param name="AnoAnterior">O ano anterior.</param>
/// <param name="MargemPorHectare">A margem do ano pelo preço da PAM; nula sem preço, produtividade ou custo.</param>
/// <param name="MargemPorHectareAnterior">A margem do ano anterior, na mesma régua.</param>
/// <param name="SafraDoCusto">A safra de custo que valeu para o ano.</param>
/// <param name="CustoPorHectare">O custo por hectare dessa safra.</param>
/// <param name="SafraDoCustoAnterior">A safra de custo que valeu para o ano anterior.</param>
/// <param name="CustoPorHectareAnterior">O custo dessa safra.</param>
public sealed record TendenciaDaRentabilidade(
    short Ano,
    short AnoAnterior,
    decimal? MargemPorHectare,
    decimal? MargemPorHectareAnterior,
    short? SafraDoCusto,
    decimal? CustoPorHectare,
    short? SafraDoCustoAnterior,
    decimal? CustoPorHectareAnterior)
{
    /// <summary>A variação da margem, em fração — nula sem as duas margens ou com a anterior não positiva.</summary>
    public decimal? VariacaoDaMargem =>
        MargemPorHectare is { } atual && MargemPorHectareAnterior is > 0 && MargemPorHectareAnterior is { } anterior
            ? decimal.Round(atual / anterior - 1m, 4)
            : null;

    /// <summary>A variação do custo por hectare entre as duas safras, em fração.</summary>
    public decimal? VariacaoDoCusto =>
        CustoPorHectare is { } atual && CustoPorHectareAnterior is > 0 && CustoPorHectareAnterior is { } anterior
            ? decimal.Round(atual / anterior - 1m, 4)
            : null;
}
