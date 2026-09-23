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
}

/// <summary>
/// O QUE O FATOR DE CICLO PRECISA SABER SOBRE UM RECORTE.
/// </summary>
/// <param name="PrecoPorCultura">O momento de preço de cada cultura do catálogo, pelo código dela.</param>
/// <param name="Credito">O índice de crédito do município; nulo sem município ou sem parâmetro vigente.</param>
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
