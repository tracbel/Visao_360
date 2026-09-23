using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>
/// O MOMENTO DO MERCADO NO RECORTE CONSULTADO (issue 76, fase T3) — o fator de ciclo e as parcelas
/// que o explicam, ao lado do porte estrutural.
///
/// <para><b>PORTE E MOMENTO SÃO DOIS NÚMEROS, NUNCA UM.</b> O porte é o tamanho do mercado e muda
/// devagar — safra, censo; o momento é o fator de ciclo e muda todo mês. Fundi-los num só esconderia
/// exatamente a leitura que a diretoria precisa: "estruturalmente grande, mas agora retraído" é uma
/// decisão diferente de "pequeno e aquecido".</para>
///
/// <para><b>O porte nasce sem nome</b>, e isso é de propósito: nomear exige um corte, e um corte sem
/// dono é parâmetro inventado (R-27 do documento 46). As bandas são a issue 166; enquanto forem
/// nulas, o porte se expressa pelo número e pela comparação. O <b>momento</b>, esse já tem faixas
/// decididas (issues 73 e 74) e pode ser nomeado hoje.</para>
///
/// <para><b>O ÍNDICE DE PREÇO DO RECORTE É O DA CULTURA DE MAIOR ÁREA, e o campo diz qual.</b> O
/// momento de preço é apurado por cultura — o café pode subir enquanto a cana cai —, e o fator pede
/// um número. Uma média ponderada seria uma fórmula nova que ninguém decidiu; escolher a cultura que
/// domina a área é uma <b>seleção declarada</b>, que a tela mostra e que se troca sem refazer conta
/// nenhuma.</para>
/// </summary>
/// <param name="Potencial">A demanda estrutural, o fator com suas parcelas, a ajustada e os cenários.</param>
/// <param name="IndiceDePreco">O momento de preço usado — o da cultura de maior área.</param>
/// <param name="CulturaDoIndiceDePreco">Qual cultura deu esse índice; nula quando não há preço.</param>
/// <param name="IndiceDeCredito">O índice de crédito do recorte; nulo sem município ou sem parâmetro.</param>
/// <param name="PercepcaoPercentual">A percepção do gestor, em pontos percentuais; nula sem registro.</param>
/// <param name="Porte">O nome do porte estrutural; <b>nulo enquanto a issue 166 não tiver bandas</b>.</param>
/// <param name="FaixaDoMomento">Retraído, normal, aquecido ou superaquecido; nula sem índice.</param>
/// <param name="Leitura">A frase que junta os dois, quando os dois existem.</param>
/// <param name="Procedencia">De onde o momento veio.</param>
public sealed record MomentoDoRecorte(
    PotencialAjustado Potencial,
    decimal? IndiceDePreco,
    string? CulturaDoIndiceDePreco,
    decimal? IndiceDeCredito,
    decimal? PercepcaoPercentual,
    string? Porte,
    string? FaixaDoMomento,
    string Leitura,
    ProcedenciaDoIndicador? Procedencia);

/// <summary>A leitura conjunta de porte e momento, em uma frase.</summary>
public static class LeituraDoMercado
{
    /// <summary>
    /// "Mercado grande, agora retraído." — e só o que existir, quando um dos dois faltar.
    ///
    /// <para><b>Ela não inventa a metade que falta.</b> Sem banda de porte, sobra o momento; sem
    /// índice, sobra o porte; sem nenhum dos dois, a frase é vazia e a tela mostra os números.</para>
    /// </summary>
    /// <param name="porte">O nome do porte, quando as bandas existem.</param>
    /// <param name="faixaDoMomento">A faixa do fator, quando há índice.</param>
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
