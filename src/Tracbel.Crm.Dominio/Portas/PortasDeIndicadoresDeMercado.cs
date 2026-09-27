using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// OS INDICADORES DE MERCADO DE UM RECORTE, na data pedida (issues 73 e 74) — o que o fator de ciclo
/// consome.
///
/// <para><b>Leitura enxuta, e não o painel inteiro.</b> O painel de crédito (issue 157) agrega 200 mil
/// linhas por ano, produto e município para a tela; a calculadora precisa de <b>um</b> município e de
/// duas janelas. Reaproveitar o painel faria cada clique em "Simular" pagar a conta da tela inteira.</para>
/// </summary>
public interface IRepositorioDeIndicadoresDeMercado
{
    /// <summary>Lê o momento de preço de cada cultura e o crédito do município.</summary>
    /// <param name="data">A data cujas vigências valem.</param>
    /// <param name="municipioCodigoIbge">O município do recorte; nulo lê só os preços, que são de São Paulo.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IndicadoresDoRecorte> LerAsync(DateOnly data, int? municipioCodigoIbge, CancellationToken ct);

    /// <summary>
    /// Lê o momento de preço de cada cultura e o crédito SOMADO dos municípios de um recorte (27/09/2026).
    ///
    /// <para><b>O crédito do recorte é a soma das linhas e do valor dos municípios dele</b>, nas mesmas duas
    /// janelas e com a mesma composição de um município só — e não a média dos índices de cada um, que daria
    /// ao município de três linhas o mesmo peso do de trezentas. Era o que faltava para o Momento do mercado da
    /// tela: ele chamava a leitura sem município, e o crédito nunca era calculado, com treze anos de SICOR no
    /// banco.</para>
    ///
    /// <para><b>A percepção só vem com um município</b>: ela é opinião registrada município a município (D-P04),
    /// e a regra de juntar as de vários não foi decidida (issue 71).</para>
    /// </summary>
    /// <param name="data">A data cujas vigências valem.</param>
    /// <param name="municipiosCodigoIbge">Os municípios do recorte; vazio lê só os preços.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IndicadoresDoRecorte> LerDoRecorteAsync(
        DateOnly data, IReadOnlyCollection<int> municipiosCodigoIbge, CancellationToken ct);

    /// <summary>
    /// Lê o momento de preço de cada cultura e, PARA CADA MUNICÍPIO, o crédito e a percepção — numa leitura só
    /// (issue 257, Diagnóstico Comercial).
    ///
    /// <para><b>Não é a leitura do recorte repetida 203 vezes</b>: são as mesmas duas janelas e a mesma conta, com o
    /// SICOR agrupado por município no banco. O índice de cada município é o dele — o de três linhas continua marcado
    /// como base pequena, em vez de herdar o índice da região.</para>
    /// </summary>
    /// <param name="data">A data cujas vigências valem.</param>
    /// <param name="municipiosCodigoIbge">Os municípios.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IndicadoresPorMunicipio> LerPorMunicipioAsync(
        DateOnly data, IReadOnlyCollection<int> municipiosCodigoIbge, CancellationToken ct);
}

/// <summary>
/// OS INDICADORES DE MERCADO MUNICÍPIO A MUNICÍPIO — o preço é de São Paulo (um por cultura); o crédito e a percepção
/// são de cada município.
/// </summary>
/// <param name="PrecoPorCultura">O momento de preço de cada cultura do catálogo, pelo código dela.</param>
/// <param name="CreditoPorMunicipio">O índice de crédito de cada município pedido, pelo código IBGE; ausente sem parâmetro vigente.</param>
/// <param name="PercepcaoPorMunicipio">A percepção vigente, em pontos percentuais, dos municípios que têm uma.</param>
/// <param name="UltimoMesDePreco">O mês mais recente da série de preço.</param>
public sealed record IndicadoresPorMunicipio(
    IReadOnlyDictionary<string, IndiceDeMomento> PrecoPorCultura,
    IReadOnlyDictionary<int, IndiceDeCredito> CreditoPorMunicipio,
    IReadOnlyDictionary<int, decimal> PercepcaoPorMunicipio,
    DateOnly? UltimoMesDePreco);

/// <summary>
/// O QUE O FATOR DE CICLO PRECISA SABER SOBRE UM RECORTE.
/// </summary>
/// <param name="PrecoPorCultura">O momento de preço de cada cultura do catálogo, pelo código dela.</param>
/// <param name="Credito">O índice de crédito do município ou do recorte; nulo sem município ou sem parâmetro vigente.</param>
/// <param name="PercepcaoDoGestor">
/// O ajuste do gestor sobre o município, em pontos percentuais; nulo quando ninguém informou. É opinião
/// registrada, com autor e vigência (D-P04).
/// </param>
/// <param name="UltimoMesDePreco">O mês mais recente da série de preço — a competência do índice.</param>
public sealed record IndicadoresDoRecorte(
    IReadOnlyDictionary<string, IndiceDeMomento> PrecoPorCultura,
    IndiceDeCredito? Credito,
    decimal? PercepcaoDoGestor,
    DateOnly? UltimoMesDePreco);
