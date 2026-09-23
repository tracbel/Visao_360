using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>Por que o fator agregado do recorte não saiu.</summary>
public enum MotivoSemFatorAgregado
{
    /// <summary>Saiu.</summary>
    Nenhum = 0,

    /// <summary>Nenhuma cultura do recorte tem regra com ciclo de renovação (D-P01, issue 63).</summary>
    SemDemandaEstrutural = 1,

    /// <summary>Há demanda, mas nenhuma cultura produziu fator — faltam os pesos (D-P05).</summary>
    SemFatorPorCultura = 2
}

/// <summary>
/// O MOMENTO DE UMA CULTURA — o fator dela, com a demanda que ela representa.
///
/// <para><b>O fator é por cultura</b> (issue 74): a cana pode estar retraída enquanto o café está
/// aquecido, porque o preço e a rentabilidade são de cada uma. Crédito e percepção são do recorte e
/// entram iguais em todas.</para>
/// </summary>
/// <param name="CulturaCodigo">O código da cultura no catálogo.</param>
/// <param name="Cultura">O nome, como a tela o mostra.</param>
/// <param name="DemandaEstrutural">O que esta cultura renova por ano, pelo motor (issue 72).</param>
/// <param name="AreaUtilHectares">A área útil dela — o critério da cultura predominante.</param>
/// <param name="IndiceDePreco">O momento de preço desta cultura; nulo é desvio zero.</param>
/// <param name="Fator">O fator desta cultura, com as três parcelas.</param>
/// <param name="DemandaAjustada">Estrutural × fator; nula quando um dos dois falta.</param>
public sealed record MomentoDaCultura(
    string CulturaCodigo,
    string Cultura,
    decimal? DemandaEstrutural,
    decimal? AreaUtilHectares,
    decimal? IndiceDePreco,
    FatorDoCiclo Fator,
    decimal? DemandaAjustada);

/// <summary>
/// A CULTURA PREDOMINANTE — contexto, e não regra de agregação.
///
/// <para>Ela responde "o que se planta aqui?", que é uma pergunta legítima e útil. O que ela <b>não</b>
/// faz é decidir o fator do município: isso seria deixar uma cultura falar pelas outras.</para>
/// </summary>
/// <param name="Cultura">O nome da cultura.</param>
/// <param name="Fatia">A fatia dela na área útil das culturas com regra, em percentual.</param>
/// <param name="Criterio">O critério, dito por extenso — a tela não deduz qual foi.</param>
public sealed record CulturaPredominante(string Cultura, decimal Fatia, string Criterio);

/// <summary>
/// O MOMENTO DO MERCADO NO RECORTE CONSULTADO (issue 76; corrigido na fase T3.1).
///
/// <para><b>PORTE E MOMENTO SÃO DOIS NÚMEROS, NUNCA UM.</b> O porte é o tamanho do mercado e muda
/// devagar; o momento é o fator de ciclo e muda todo mês. Fundi-los esconderia a leitura que a
/// diretoria precisa: "estruturalmente grande, mas agora retraído" é uma decisão diferente de
/// "pequeno e aquecido".</para>
///
/// <para><b>O FATOR AGREGADO NÃO É UM ÍNDICE MÉDIO DE COMMODITY.</b> Ele é a razão entre dois números
/// que o motor <b>já calcula</b>:</para>
///
/// <code>
/// fatorAgregado = Σ demandaAjustada(cultura) ÷ Σ demandaEstrutural(cultura)
/// </code>
///
/// <para>Cada cultura mantém o preço e a rentabilidade dela, e pesa no resultado exatamente pela
/// demanda estrutural que representa. Nenhuma fórmula nova foi criada: se todas as culturas forem
/// neutras, a soma ajustada é igual à estrutural e o agregado dá 1. E como cada fator já vem dentro
/// dos limites registrados, a média ponderada deles não escapa deles.</para>
///
/// <para><b>A versão anterior usava o índice da cultura de maior área</b> (23/09/2026), e isso foi
/// recusado: era uma solução técnica sem decisão de negócio que a sustentasse, e conflitava com a
/// issue 74, que define o fator por cultura. A cultura predominante continua, como <b>contexto</b>.</para>
///
/// <para><b>Sem demanda estrutural não há agregado, e não há retorno para a regra antiga.</b>
/// Enquanto o D-P01 (issue 63) não fixar o ciclo de renovação, o fator sai ausente com o motivo —
/// ausência honesta é melhor que regra provisória com aparência de definitiva.</para>
/// </summary>
/// <param name="FatorAgregado">Demanda ajustada total ÷ estrutural total; nulo com motivo.</param>
/// <param name="MotivoSemFator">Por que o agregado não saiu; <c>Nenhum</c> quando saiu.</param>
/// <param name="DemandaEstruturalTotal">A soma das demandas estruturais que entraram.</param>
/// <param name="DemandaAjustadaTotal">A soma das ajustadas correspondentes.</param>
/// <param name="PorCultura">O fator de cada cultura, com as parcelas — a composição do agregado.</param>
/// <param name="Predominante">A cultura que domina a área; contexto, não regra.</param>
/// <param name="IndiceDeCredito">O índice de crédito do recorte; nulo sem município ou sem parâmetro.</param>
/// <param name="PercepcaoPercentual">A percepção do gestor, em pontos percentuais; nula sem registro.</param>
/// <param name="Porte">O nome do porte estrutural; <b>nulo enquanto a issue 166 não tiver bandas</b>.</param>
/// <param name="FaixaDoMomento">Retraído, normal, aquecido ou superaquecido; nula sem fator.</param>
/// <param name="Leitura">A frase que junta porte e momento, quando os dois existem.</param>
/// <param name="Procedencia">De onde o momento veio.</param>
public sealed record MomentoDoRecorte(
    decimal? FatorAgregado,
    string MotivoSemFator,
    decimal? DemandaEstruturalTotal,
    decimal? DemandaAjustadaTotal,
    IReadOnlyList<MomentoDaCultura> PorCultura,
    CulturaPredominante? Predominante,
    decimal? IndiceDeCredito,
    decimal? PercepcaoPercentual,
    string? Porte,
    string? FaixaDoMomento,
    string Leitura,
    ProcedenciaDoIndicador? Procedencia);

/// <summary>
/// A AGREGAÇÃO DO MOMENTO — a razão entre o que o motor ajustou e o que ele estruturou.
/// </summary>
public static class MomentoAgregado
{
    /// <summary>
    /// O fator agregado do recorte, a partir do resultado por cultura.
    ///
    /// <para><b>Só entram as culturas com os DOIS números.</b> Cultura sem demanda estrutural não
    /// tem peso a exercer, e cultura sem fator não tem ajuste a contribuir — incluí-la com zero
    /// puxaria o agregado para baixo afirmando uma retração que ninguém mediu.</para>
    ///
    /// <para><b>Total estrutural zero ou nulo devolve ausência</b>, e não 1,00: "o mercado está
    /// neutro" é uma afirmação, e "não há base para dizer" é outra.</para>
    /// </summary>
    /// <param name="porCultura">O momento de cada cultura do recorte.</param>
    public static (decimal? Fator, decimal? Estrutural, decimal? Ajustada, string Motivo) Agregar(
        IReadOnlyList<MomentoDaCultura> porCultura)
    {
        var validas = porCultura
            .Where(c => c is { DemandaEstrutural: > 0, DemandaAjustada: not null })
            .ToList();

        if (validas.Count == 0)
        {
            // A distinção importa para a tela: falta o ciclo (D-P01) é uma coisa; ter demanda e não
            // ter fator, porque os pesos não foram decididos (D-P05), é outra.
            var temDemanda = porCultura.Any(c => c.DemandaEstrutural is > 0);
            return (null, null, null,
                temDemanda
                    ? nameof(MotivoSemFatorAgregado.SemFatorPorCultura)
                    : nameof(MotivoSemFatorAgregado.SemDemandaEstrutural));
        }

        var estrutural = validas.Sum(c => c.DemandaEstrutural!.Value);
        var ajustada = validas.Sum(c => c.DemandaAjustada!.Value);

        return (ajustada / estrutural, estrutural, ajustada, nameof(MotivoSemFatorAgregado.Nenhum));
    }

    /// <summary>
    /// A CULTURA PREDOMINANTE por ÁREA ÚTIL, com a fatia e o critério ditos.
    ///
    /// <para>Contexto, e não regra: ela responde "o que se planta aqui?" e não decide o fator. O
    /// denominador é a área útil das culturas <b>com regra</b>, porque é sobre essas que o potencial
    /// fala — e a tela diz isso no próprio critério.</para>
    /// </summary>
    /// <param name="porCultura">O momento de cada cultura do recorte.</param>
    public static CulturaPredominante? Predominante(IReadOnlyList<MomentoDaCultura> porCultura)
    {
        var comArea = porCultura.Where(c => c.AreaUtilHectares is > 0).ToList();
        if (comArea.Count == 0) return null;

        var total = comArea.Sum(c => c.AreaUtilHectares!.Value);
        var maior = comArea.MaxBy(c => c.AreaUtilHectares!.Value)!;

        return new CulturaPredominante(
            maior.Cultura,
            100m * maior.AreaUtilHectares!.Value / total,
            "maior área útil entre as culturas com regra de potencial");
    }
}

/// <summary>A leitura conjunta de porte e momento, em uma frase.</summary>
public static class LeituraDoMercado
{
    /// <summary>
    /// "Mercado grande, agora retraído." — e só o que existir, quando um dos dois faltar.
    ///
    /// <para><b>Ela não inventa a metade que falta.</b> Sem banda de porte, sobra o momento; sem
    /// fator, sobra o porte; sem nenhum dos dois, a frase é vazia e a tela mostra os números.</para>
    /// </summary>
    /// <param name="porte">O nome do porte, quando as bandas existem.</param>
    /// <param name="faixaDoMomento">A faixa do fator, quando há fator.</param>
    public static string Frase(string? porte, string? faixaDoMomento)
    {
        var temPorte = !string.IsNullOrWhiteSpace(porte);
        var temMomento = !string.IsNullOrWhiteSpace(faixaDoMomento);

        if (temPorte && temMomento) return $"{porte}, agora {faixaDoMomento!.ToLowerInvariant()}.";
        if (temMomento) return $"Mercado {faixaDoMomento!.ToLowerInvariant()}.";
        if (temPorte) return $"{porte}.";
        return string.Empty;
    }

    /// <summary>
    /// A FAIXA DO MOMENTO a partir do fator, pelas mesmas fronteiras dos índices (issues 73 e 74).
    ///
    /// <para>O fator é neutro em 1,00, e as faixas do parâmetro são as mesmas que classificam preço e
    /// crédito — reusá-las mantém uma régua só na tela inteira.</para>
    /// </summary>
    /// <param name="fator">O fator de ciclo; nulo devolve nulo.</param>
    /// <param name="parametro">Os parâmetros vigentes, que trazem as fronteiras.</param>
    public static string? FaixaDoFator(decimal? fator, ParametroDoPotencial? parametro)
    {
        if (fator is not { } f || parametro is null) return null;

        if (f < parametro.LimiteDeRetracao) return "Retraído";
        if (f >= parametro.LimiteDeSuperaquecimento) return "Superaquecido";
        if (f >= parametro.LimiteDeAquecimento) return "Aquecido";
        return parametro.NomeDaFaixaIntermediaria ?? "Normal";
    }
}
