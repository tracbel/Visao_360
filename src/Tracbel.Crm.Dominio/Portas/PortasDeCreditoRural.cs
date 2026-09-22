namespace Tracbel.Crm.Dominio.Portas;

/// <summary>A leitura do crédito rural de investimento do SICOR (issue 68).</summary>
public interface IRepositorioDeCreditoRural
{
    /// <summary>
    /// O painel do crédito: por ano, por produto, por município e por recorte, com as duas janelas.
    ///
    /// <para>A janela e a carência vêm de fora porque são <b>parâmetro com vigência</b> (issue 157):
    /// quem sabe qual vigência vale hoje é a aplicação, não a consulta.</para>
    /// </summary>
    /// <param name="mesesPorJanela">Quantos meses tem cada lado da comparação.</param>
    /// <param name="mesesDeCarencia">Meses recentes a descartar; nulo é "não decidida" e vale zero.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<PainelDeCreditoRural> LerAsync(short mesesPorJanela, short? mesesDeCarencia, CancellationToken ct);
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
/// <param name="Janela">O intervalo exato das duas janelas e a carência aplicada; nulo sem dado.</param>
/// <param name="PorAno">Linhas e valor por ano: máquinas e todos os produtos.</param>
/// <param name="PorProduto">Os produtos, com as duas janelas, do maior valor ao menor.</param>
/// <param name="PorMunicipio">Os municípios com crédito de máquinas nas duas janelas.</param>
/// <param name="Regiao">O crédito de máquinas da Região (a ADR), somado; nulo sem dado.</param>
/// <param name="SaoPaulo">O mesmo para São Paulo inteiro — o denominador da comparação.</param>
public sealed record PainelDeCreditoRural(
    DateOnly? UltimoMes,
    JanelaDoCredito? Janela,
    IReadOnlyList<CreditoNoAno> PorAno,
    IReadOnlyList<CreditoPorProduto> PorProduto,
    IReadOnlyList<CreditoDeMaquinasNoMunicipio> PorMunicipio,
    CreditoNoRecorte? Regiao,
    CreditoNoRecorte? SaoPaulo);

/// <summary>
/// A JANELA DE COMPARAÇÃO, DITA POR EXTENSO (issue 157) — para a tela não precisar recalculá-la e
/// para quem conferir saber exatamente que meses entraram.
///
/// <para><b>Ela não termina no último mês com dado.</b> O Banco Central continua acrescentando
/// contrato registrado com atraso nos meses recentes; comparar 12 meses cheios com 12 que ainda
/// estão enchendo mostraria o crédito caindo sem ter caído. Os <see cref="MesesDeCarencia"/> mais
/// recentes ficam de fora dos dois lados.</para>
///
/// <para><b>Carência não decidida é zero, dito.</b> Quando <see cref="CarenciaDecidida"/> é falso,
/// nenhum mês é descartado e a tela avisa — em vez de descartar meses por um palpite (D-IM-03).</para>
/// </summary>
/// <param name="UltimoMesComDado">O mês mais recente que o SICOR trouxe (dia 1).</param>
/// <param name="MesesDeCarencia">Quantos meses recentes ficaram de fora; zero quando não decidida.</param>
/// <param name="CarenciaDecidida">Se o parâmetro vigente traz um valor.</param>
/// <param name="Inicio">O primeiro mês da última janela (dia 1).</param>
/// <param name="Fim">O último mês da última janela — o de corte, já descontada a carência.</param>
/// <param name="InicioAnterior">O primeiro mês da janela anterior.</param>
/// <param name="FimAnterior">O último mês da janela anterior.</param>
/// <param name="MesesPorJanela">Quantos meses tem cada lado.</param>
public sealed record JanelaDoCredito(
    DateOnly UltimoMesComDado,
    short MesesDeCarencia,
    bool CarenciaDecidida,
    DateOnly Inicio,
    DateOnly Fim,
    DateOnly InicioAnterior,
    DateOnly FimAnterior,
    short MesesPorJanela);

/// <summary>
/// O CRÉDITO DE MÁQUINAS DE UM RECORTE — a Região e São Paulo, somados no banco (issue 157).
///
/// <para>Sem São Paulo ao lado, "R$ 300 mi na Região" é número solto: ele pode ser um terço do
/// estado ou um vigésimo. A tela mostrava só a Região, com um filtro "só a ADR".</para>
/// </summary>
/// <param name="Recorte">O nome do recorte, como a tela o chama.</param>
/// <param name="Municipios">Quantos municípios do recorte têm linha de máquina na última janela.</param>
/// <param name="Janelas">As duas janelas do recorte.</param>
public sealed record CreditoNoRecorte(string Recorte, int Municipios, JanelasDeCredito Janelas);

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
