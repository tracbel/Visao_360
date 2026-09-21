namespace Tracbel.Crm.Dominio.Portas;

/// <summary>A leitura do crédito rural de investimento do SICOR (issue 68).</summary>
public interface IRepositorioDeCreditoRural
{
    /// <summary>O painel do crédito: por ano, por produto e por município, com a janela de 12 meses.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<PainelDeCreditoRural> LerAsync(CancellationToken ct);
}

/// <summary>
/// O CRÉDITO RURAL DE SÃO PAULO, como a tela o mostra.
///
/// <para><b>A janela</b> é a do texto-base — "os últimos 12 meses divididos pelos meses anteriores" —,
/// contada a partir do último mês com dado no SICOR. O índice (70% linhas, 30% valor) e as faixas são
/// da issue 73; aqui estão só as duas janelas, lado a lado.</para>
///
/// <para><b>Máquinas</b> são os três produtos que a planilha do comercial usa: trator (7080), máquinas e
/// implementos (4860) e colheitadeiras (2700).</para>
/// </summary>
/// <param name="UltimoMes">O último mês com dado (dia 1); nulo quando não há crédito carregado.</param>
/// <param name="PorAno">Linhas e valor por ano: máquinas e todos os produtos.</param>
/// <param name="PorProduto">Os produtos, com as duas janelas, do maior valor ao menor.</param>
/// <param name="PorMunicipio">Os municípios com crédito de máquinas nas duas janelas.</param>
public sealed record PainelDeCreditoRural(
    DateOnly? UltimoMes,
    IReadOnlyList<CreditoNoAno> PorAno,
    IReadOnlyList<CreditoPorProduto> PorProduto,
    IReadOnlyList<CreditoDeMaquinasNoMunicipio> PorMunicipio);

/// <summary>O crédito de um ano.</summary>
public sealed record CreditoNoAno(short Ano, int LinhasDeMaquinas, decimal ValorDeMaquinas, int LinhasTotais, decimal ValorTotal, byte UltimoMes);

/// <summary>Duas janelas de 12 meses: a última e a anterior.</summary>
/// <param name="Linhas">Linhas nos últimos 12 meses.</param>
/// <param name="Valor">Valor nos últimos 12 meses.</param>
/// <param name="LinhasAnteriores">Linhas nos 12 meses anteriores.</param>
/// <param name="ValorAnterior">Valor nos 12 meses anteriores.</param>
public sealed record JanelasDeCredito(int Linhas, decimal Valor, int LinhasAnteriores, decimal ValorAnterior);

/// <summary>Um produto financiado.</summary>
public sealed record CreditoPorProduto(int Codigo, string Nome, bool EhMaquina, JanelasDeCredito Janelas);

/// <summary>O crédito de máquinas de um município.</summary>
public sealed record CreditoDeMaquinasNoMunicipio(int CodigoIbge, string Nome, bool PertenceAAdr, JanelasDeCredito Janelas);
