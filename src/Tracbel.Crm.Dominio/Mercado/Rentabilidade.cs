using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>
/// A CAMADA DE CUSTO QUE A MARGEM USA — decisão D-P07, em aberto.
///
/// <para>A CONAB publica duas: o <b>operacional</b> (custo variável + fixo, o desembolso da safra) e o
/// <b>total</b> (operacional + remuneração do capital e da terra). A margem sobre o operacional diz se a
/// safra se paga; sobre o total, se o negócio remunera o patrimônio. São perguntas diferentes, e o
/// número muda muito entre elas.</para>
/// </summary>
public enum CamadaDoCusto
{
    /// <summary>Custo variável + fixo: o desembolso da safra.</summary>
    Operacional = 0,

    /// <summary>Operacional + renda de fatores: remunera também capital e terra.</summary>
    Total = 1
}

/// <summary>
/// POR QUE A MARGEM NÃO SAIU — o motivo dito em frase, para a tela não mostrar vazio mudo.
///
/// <para>A regra da Inteligência de Mercado é a mesma em todo lugar: <b>parâmetro sem valor decidido ou
/// dado ausente devolve vazio COM O MOTIVO</b>, nunca um número inventado.</para>
/// </summary>
public enum MotivoSemMargem
{
    /// <summary>A margem saiu.</summary>
    Nenhum = 0,

    /// <summary>A cultura não tem local de referência escolhido (D-P07).</summary>
    SemLocalDeReferencia = 1,

    /// <summary>A cultura não tem camada de custo escolhida (D-P07).</summary>
    SemCamadaDeCusto = 2,

    /// <summary>Não há série de custo da CONAB para a cultura e o local escolhidos.</summary>
    SemCustoNaReferencia = 3,

    /// <summary>A CONAB publicou só o custo operacional naquela safra, e a margem pedida é sobre o total.</summary>
    SemCustoTotalNaSafra = 4,

    /// <summary>Não há preço da cultura nos meses da safra.</summary>
    SemPreco = 5,

    /// <summary>A PAM não traz produtividade: falta quantidade ou a área colhida é zero.</summary>
    SemProdutividade = 6
}

/// <summary>
/// A RECEITA, O CUSTO E A MARGEM POR HECTARE DE UMA CULTURA (issue 159).
///
/// <para><b>Por que existe.</b> Preço e custo já estavam na tela, <b>separados</b>: quem quisesse saber
/// se a lavoura paga a conta tinha de fazer a subtração de cabeça — e sem saber que a produtividade de
/// um lado e o custo do outro podiam ser de anos diferentes. A margem junta os três e <b>carrega as
/// competências de cada parte</b>, que é o que o pedido chama de "local, sistema, safra, fonte e data no
/// tooltip".</para>
///
/// <para><b>Tudo em quilos, por dentro.</b> A CONAB publica o preço em R$/kg e a PAM a quantidade em
/// toneladas; a unidade comercial (saca de 60 kg, caixa de 40,8 kg) é do catálogo e serve para
/// <b>mostrar</b>, não para calcular. Converter no meio da conta é como se perde casa decimal.</para>
///
/// <para><b>O que este cálculo NÃO faz:</b> não multiplica margem por área plantada. O protótipo faz
/// isso (achado C-07 do documento 49), e é errado — o que se colhe é a área <b>colhida</b>, e é ela que
/// multiplica a margem por hectare.</para>
/// </summary>
public static class Rentabilidade
{
    /// <summary>
    /// A RECEITA POR HECTARE de uma cultura comum: produtividade × preço.
    ///
    /// <para>A produtividade vem em quilos por hectare (a quantidade da PAM ÷ a área <b>colhida</b>,
    /// regra da issue 152), e o preço em reais por quilo — o que a CONAB publica. Nulo quando falta
    /// qualquer um dos dois.</para>
    /// </summary>
    /// <param name="produtividadeKgPorHa">Quilos por hectare colhido.</param>
    /// <param name="precoPorKg">O preço médio dos meses da safra, em R$/kg.</param>
    public static decimal? ReceitaPorHectare(decimal? produtividadeKgPorHa, decimal? precoPorKg) =>
        produtividadeKgPorHa is { } kg && precoPorKg is { } preco ? kg * preco : null;

    /// <summary>
    /// A RECEITA POR HECTARE DA CANA, que não se calcula como as outras.
    ///
    /// <para>A cana não é vendida por quilo de cana: é vendida pelo <b>açúcar que ela carrega</b>. O
    /// produtor recebe pelo ATR — açúcar total recuperável —, em quilos de ATR por tonelada de cana,
    /// multiplicado pelo preço do quilo de ATR que a Socicana publica. Usar o preço da tonelada de cana
    /// aqui daria outro número, e é o erro que o protótipo comete ao aplicar o ATR de uma safra a 24
    /// meses (achado C-06 do documento 49).</para>
    /// </summary>
    /// <param name="toneladasPorHa">Toneladas de cana por hectare colhido.</param>
    /// <param name="atrKgPorTonelada">Quilos de ATR por tonelada de cana, na safra.</param>
    /// <param name="precoDoKgDeAtr">O preço do quilo de ATR, em reais.</param>
    public static decimal? ReceitaDaCanaPorHectare(
        decimal? toneladasPorHa, decimal? atrKgPorTonelada, decimal? precoDoKgDeAtr) =>
        toneladasPorHa is { } t && atrKgPorTonelada is { } atr && precoDoKgDeAtr is { } preco
            ? t * atr * preco
            : null;

    /// <summary>
    /// A MARGEM POR HECTARE: receita menos custo, na mesma unidade de área.
    ///
    /// <para><b>Margem negativa é resultado, não erro.</b> Safra em que o preço não cobre o custo
    /// existe, e esconder isso seria mentir sobre o mercado.</para>
    /// </summary>
    /// <param name="receitaPorHa">A receita por hectare.</param>
    /// <param name="custoPorHa">O custo por hectare, na camada escolhida.</param>
    public static decimal? MargemPorHectare(decimal? receitaPorHa, decimal? custoPorHa) =>
        receitaPorHa is { } receita && custoPorHa is { } custo ? receita - custo : null;

    /// <summary>
    /// A MARGEM TOTAL de um recorte: a margem por hectare vezes a área <b>colhida</b>.
    ///
    /// <para><b>Não é a área plantada</b> (achado C-07). O que se vende é o que se colheu; em cultura
    /// perene a plantada inclui o pomar novo que ainda não produz, e multiplicar por ela inventaria
    /// receita que não existiu.</para>
    /// </summary>
    /// <param name="margemPorHa">A margem por hectare.</param>
    /// <param name="areaColhidaHa">A área colhida, em hectares.</param>
    public static decimal? MargemTotal(decimal? margemPorHa, decimal? areaColhidaHa) =>
        margemPorHa is { } margem && areaColhidaHa is { } area ? margem * area : null;

    /// <summary>
    /// O ÍNDICE DE RENTABILIDADE: a razão receita ÷ custo da safra, sobre a média das N safras anteriores.
    ///
    /// <para>Acima de 1, a safra rende mais do que rendia; abaixo, menos. É a mesma forma dos outros
    /// índices do motor — uma janela contra a anterior —, e por isso a leitura é a mesma.</para>
    ///
    /// <para><b>Nulo quando falta base.</b> Sem as N safras anteriores completas, o índice não sai: uma
    /// média de duas safras onde se pediu cinco é outro indicador, com outro nome.</para>
    /// </summary>
    /// <param name="razaoDaSafra">Receita ÷ custo da safra em questão.</param>
    /// <param name="razoesAnteriores">As razões das safras anteriores, da mais recente para a mais antiga.</param>
    /// <param name="safrasDaMedia">Quantas safras a média usa; nulo é "não decidido" e o índice não sai.</param>
    public static decimal? IndiceDeRentabilidade(
        decimal? razaoDaSafra, IReadOnlyList<decimal> razoesAnteriores, short? safrasDaMedia)
    {
        if (razaoDaSafra is not { } razao || safrasDaMedia is not { } n || n <= 0) return null;
        if (razoesAnteriores.Count < n) return null;

        var media = razoesAnteriores.Take(n).Average();
        return media == 0 ? null : razao / media;
    }

    /// <summary>
    /// A RAZÃO RECEITA ÷ CUSTO de uma safra — a parcela do índice.
    ///
    /// <para>Custo zero não devolve infinito: devolve nulo. Uma cultura com custo zero é erro de leitura,
    /// e um índice infinito contaminaria toda média que o usasse.</para>
    /// </summary>
    /// <param name="receitaPorHa">A receita por hectare.</param>
    /// <param name="custoPorHa">O custo por hectare.</param>
    public static decimal? RazaoDeRentabilidade(decimal? receitaPorHa, decimal? custoPorHa) =>
        receitaPorHa is { } receita && custoPorHa is { } custo && custo > 0 ? receita / custo : null;

    /// <summary>
    /// O CUSTO POR HECTARE na camada pedida, direto da série da CONAB.
    ///
    /// <para><b>Custo total ausente não é zero</b> (issue 67): as abas antigas de laranja e duas de cana
    /// param no operacional. Pedir o total nessas devolve nulo, e a tela diz por quê.</para>
    /// </summary>
    /// <param name="custo">A linha de custo da safra.</param>
    /// <param name="camada">A camada escolhida.</param>
    public static decimal? CustoPorHectare(CustoDeProducao custo, CamadaDoCusto camada) =>
        camada == CamadaDoCusto.Operacional ? custo.CustoOperacionalHa : custo.CustoTotalHa;

    /// <summary>
    /// POR QUE A MARGEM NÃO SAIU, na ordem em que as peças faltam.
    ///
    /// <para>A ordem importa: quem não escolheu o local de referência precisa ouvir isso primeiro, e não
    /// "sem preço" — o segundo é consequência do primeiro.</para>
    /// </summary>
    /// <param name="temLocalDeReferencia">Se a cultura tem local escolhido (D-P07).</param>
    /// <param name="temCamada">Se a cultura tem camada de custo escolhida (D-P07).</param>
    /// <param name="temCusto">Se há série de custo na referência.</param>
    /// <param name="custoNaCamada">O custo na camada pedida; nulo quando a safra parou no operacional.</param>
    /// <param name="produtividade">A produtividade da PAM.</param>
    /// <param name="preco">O preço médio da safra.</param>
    public static MotivoSemMargem PorQueNaoSaiu(
        bool temLocalDeReferencia,
        bool temCamada,
        bool temCusto,
        decimal? custoNaCamada,
        decimal? produtividade,
        decimal? preco)
    {
        if (!temLocalDeReferencia) return MotivoSemMargem.SemLocalDeReferencia;
        if (!temCamada) return MotivoSemMargem.SemCamadaDeCusto;
        if (!temCusto) return MotivoSemMargem.SemCustoNaReferencia;
        if (custoNaCamada is null) return MotivoSemMargem.SemCustoTotalNaSafra;
        if (produtividade is null) return MotivoSemMargem.SemProdutividade;
        if (preco is null) return MotivoSemMargem.SemPreco;

        return MotivoSemMargem.Nenhum;
    }

    /// <summary>A frase que a tela mostra no lugar do número.</summary>
    /// <param name="motivo">O motivo.</param>
    /// <param name="cultura">O nome da cultura, para a frase citar.</param>
    public static string Frase(MotivoSemMargem motivo, string cultura) => motivo switch
    {
        MotivoSemMargem.SemLocalDeReferencia =>
            $"Falta escolher o local de referência do custo de {cultura} — a CONAB publica vários, e a margem muda com ele.",
        MotivoSemMargem.SemCamadaDeCusto =>
            $"Falta escolher a camada de custo de {cultura}: operacional (o desembolso da safra) ou total (que remunera capital e terra).",
        MotivoSemMargem.SemCustoNaReferencia =>
            $"A CONAB não publica custo de {cultura} no local de referência escolhido.",
        MotivoSemMargem.SemCustoTotalNaSafra =>
            $"A CONAB parou no custo operacional nesta safra de {cultura}: não há custo total para comparar.",
        MotivoSemMargem.SemPreco =>
            $"Sem preço de {cultura} nos meses da safra.",
        MotivoSemMargem.SemProdutividade =>
            $"Sem produtividade de {cultura} na PAM: falta quantidade colhida ou a área colhida é zero.",
        _ => string.Empty
    };
}
