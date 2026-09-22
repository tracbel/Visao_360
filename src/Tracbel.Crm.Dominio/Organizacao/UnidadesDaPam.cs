namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>A unidade da quantidade produzida de um produto da PAM, e a da produtividade que sai dela.</summary>
/// <param name="Nome">A unidade da quantidade, como a tela escreve: "toneladas", "mil frutos", "mil cachos".</param>
/// <param name="DaProdutividade">A unidade da produtividade: "t/ha", "mil frutos/ha", "mil cachos/ha".</param>
/// <param name="EhMassa">Se é peso — só peso se converte para saca, caixa ou arroba (issue 165).</param>
public sealed record UnidadeDaQuantidade(string Nome, string DaProdutividade, bool EhMassa);

/// <summary>
/// A UNIDADE DE CADA PRODUTO DA PRODUÇÃO AGRÍCOLA MUNICIPAL, e a produtividade calculada como o IBGE calcula
/// (issue 152, documento 49 §7, C-01).
///
/// <para><b>Por que é regra de domínio e não coluna lida da fonte.</b> O SIDRA devolve, em cada valor, um campo de
/// "unidade de medida" — e ele diz "Toneladas" para os 85 produtos da tabela 5457, em 2024 e em 2000
/// (medido em 22/09/2026). É a unidade da <i>variável</i>, não a do produto. As exceções estão nas notas da
/// tabela, e é delas que esta classe vem:</para>
/// <list type="bullet">
/// <item><b>Nota 6:</b> "as quantidades produzidas de abacaxi e de coco-da-baía são expressas em mil frutos e o
/// rendimento médio em frutos/ha" — em todos os anos.</item>
/// <item><b>Nota 2:</b> a partir de 2001, abacate, banana, caqui, figo, goiaba, laranja, limão, maçã, mamão,
/// manga, maracujá, marmelo, melancia, melão, pera, pêssego e tangerina passam a ser expressos em toneladas;
/// antes, em mil frutos — a banana em mil cachos.</item>
/// </list>
///
/// <para><b>Produtividade é quantidade sobre área COLHIDA, no mesmo ano.</b> É a regra do rendimento médio do
/// IBGE (variável 112). A planilha do comercial dividia pela área plantada — e ainda com rótulos de anos
/// diferentes —, o que dá outro número em cultura perene nova, onde parte da área plantada ainda não produz.
/// Conferido contra o IBGE: café em SP, 2024, 335.310 t ÷ 190.255 ha = 1,762 t/ha; a variável 112 publica
/// 1.762 kg/ha.</para>
/// </summary>
public static class UnidadesDaPam
{
    /// <summary>O primeiro ano em que as frutas da nota 2 aparecem em tonelada.</summary>
    public const short PrimeiroAnoDasFrutasEmToneladas = 2001;

    /// <summary>Banana (cacho) — antes de 2001, em mil cachos.</summary>
    public const int Banana = 40136;

    /// <summary>Abacaxi e coco-da-baía: sempre em mil frutos (nota 6).</summary>
    public static readonly IReadOnlySet<int> SempreEmMilFrutos = new HashSet<int> { 40092, 40145 };

    /// <summary>As frutas da nota 2, que só passam a tonelada em 2001.</summary>
    public static readonly IReadOnlySet<int> FrutasEmToneladasDesde2001 = new HashSet<int>
    {
        40129, // abacate
        40136, // banana (cacho)
        40142, // caqui
        40148, // figo
        40149, // goiaba
        40151, // laranja
        40152, // limão
        40260, // maçã
        40261, // mamão
        40262, // manga
        40263, // maracujá
        40264, // marmelo
        40120, // melancia
        40121, // melão
        40267, // pera
        40268, // pêssego
        40271  // tangerina
    };

    private static readonly UnidadeDaQuantidade Toneladas = new("toneladas", "t/ha", EhMassa: true);
    private static readonly UnidadeDaQuantidade MilFrutos = new("mil frutos", "mil frutos/ha", EhMassa: false);
    private static readonly UnidadeDaQuantidade MilCachos = new("mil cachos", "mil cachos/ha", EhMassa: false);

    /// <summary>A unidade da quantidade produzida de um produto num ano.</summary>
    /// <param name="produtoCodigoIbge">O código do produto na classificação 782.</param>
    /// <param name="ano">O ano da PAM.</param>
    public static UnidadeDaQuantidade DaQuantidade(int produtoCodigoIbge, short ano)
    {
        if (SempreEmMilFrutos.Contains(produtoCodigoIbge)) return MilFrutos;

        if (ano < PrimeiroAnoDasFrutasEmToneladas && FrutasEmToneladasDesde2001.Contains(produtoCodigoIbge))
            return produtoCodigoIbge == Banana ? MilCachos : MilFrutos;

        return Toneladas;
    }

    /// <summary>
    /// A produtividade — quantidade sobre área colhida, no mesmo ano e na unidade da quantidade por hectare.
    /// Nula quando falta uma das duas ou quando não se colheu nada: dividir por zero não é "produtividade zero".
    /// </summary>
    /// <param name="quantidade">A quantidade produzida.</param>
    /// <param name="areaColhidaHectares">A área colhida do mesmo produto, recorte e ano.</param>
    public static decimal? Produtividade(decimal? quantidade, decimal? areaColhidaHectares) =>
        quantidade is { } q && areaColhidaHectares is > 0 ? decimal.Round(q / areaColhidaHectares.Value, 4) : null;
}
