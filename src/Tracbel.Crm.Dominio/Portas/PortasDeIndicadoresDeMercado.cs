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
}

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
/// <param name="PercepcaoPorCultura">
/// A percepção de campo de cada cultura, pelo código dela, já em pontos percentuais (27/09/2026). Só as culturas com
/// nota vigente aparecem; ausente vale zero, como toda percepção.
/// </param>
public sealed record IndicadoresDoRecorte(
    IReadOnlyDictionary<string, IndiceDeMomento> PrecoPorCultura,
    IndiceDeCredito? Credito,
    decimal? PercepcaoDoGestor,
    DateOnly? UltimoMesDePreco,
    IReadOnlyDictionary<string, decimal>? PercepcaoPorCultura = null)
{
    /// <summary>
    /// A PERCEPÇÃO QUE ENTRA NO FATOR DE UMA CULTURA: a de campo da cultura mais o ajuste do gestor sobre o município.
    /// Nula quando nenhuma das duas foi informada — e aí o fator a trata como zero, sem dizer que alguém a declarou
    /// neutra.
    /// </summary>
    /// <param name="culturaCodigo">O código da cultura no catálogo.</param>
    public decimal? PercepcaoDaCulturaNoRecorte(string culturaCodigo)
    {
        decimal? daCultura = PercepcaoPorCultura is not null && PercepcaoPorCultura.TryGetValue(culturaCodigo, out var p)
            ? p
            : null;

        return daCultura is null && PercepcaoDoGestor is null ? null : (daCultura ?? 0m) + (PercepcaoDoGestor ?? 0m);
    }
}
