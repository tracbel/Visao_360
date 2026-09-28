using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>A leitura do share da Tracbel no crédito de mecanização (issue 262).</summary>
public interface IRepositorioDoShareNoCredito
{
    /// <summary>
    /// O share na janela recente do crédito — a mesma do painel do SICOR: os últimos <paramref name="mesesPorJanela"/>
    /// meses até o último mês com dado, menos a carência.
    /// </summary>
    /// <param name="mesesPorJanela">Quantos meses tem a janela.</param>
    /// <param name="mesesDeCarencia">Meses recentes a descartar; nulo vale zero.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<ShareNoCreditoDeMecanizacao> LerAsync(short mesesPorJanela, short? mesesDeCarencia, CancellationToken ct);
}

/// <summary>
/// O SHARE DA TRACBEL NO CRÉDITO DE MECANIZAÇÃO — o valor que ela financiou dividido pelo crédito de máquinas do SICOR, no
/// mesmo território e na mesma janela (decisão do Ricardo em 28/09/2026: "agregado por filial").
///
/// <para><b>É uma estimativa, e diz por quê.</b> O numerador é o pedido de venda, no município do cadastro do cliente; o
/// denominador é a cédula, no município do empreendimento. Por filial e em 12 meses as duas diferenças se diluem; no
/// município, não — e por isso o município vem marcado quando a Tracbel passa do SICOR.</para>
/// </summary>
/// <param name="Inicio">O primeiro mês da janela (dia 1); nulo sem SICOR carregado.</param>
/// <param name="Fim">O último mês da janela.</param>
/// <param name="UltimoPedido">A data do financiamento mais recente gravado; nulo sem nenhum.</param>
/// <param name="Regiao">A Região Tracbel (a ADR inteira); nulo sem janela.</param>
/// <param name="PorFilial">As filiais responsáveis pelos municípios, do maior crédito ao menor.</param>
/// <param name="PorMunicipio">Os municípios da ADR com crédito ou financiamento na janela — indicativo.</param>
/// <param name="PorLinha">O que a Tracbel financiou na Região, por linha de crédito — as que contam e as que não contam.</param>
/// <param name="PorMes">A Região mês a mês.</param>
/// <param name="ForaDaRegiao">O crédito rural da Tracbel na janela que não entra no share da Região, e por quê.</param>
/// <param name="Procedencia">De onde os dois lados vêm.</param>
public sealed record ShareNoCreditoDeMecanizacao(
    DateOnly? Inicio,
    DateOnly? Fim,
    DateOnly? UltimoPedido,
    ShareNoRecorte? Regiao,
    IReadOnlyList<ShareDaFilial> PorFilial,
    IReadOnlyList<ShareNoMunicipio> PorMunicipio,
    IReadOnlyList<FinanciadoPorLinha> PorLinha,
    IReadOnlyList<ShareNoMes> PorMes,
    FinanciadoForaDaRegiao ForaDaRegiao,
    ProcedenciaDoIndicador? Procedencia = null);

/// <summary>Os dois lados de um recorte.</summary>
/// <param name="ValorTracbel">O que a Tracbel financiou em crédito rural.</param>
/// <param name="Financiamentos">Quantos financiamentos (processos).</param>
/// <param name="ValorSicor">O crédito de máquinas do SICOR.</param>
/// <param name="LinhasSicor">As linhas do SICOR — não contratos.</param>
public sealed record ShareNoRecorte(decimal ValorTracbel, int Financiamentos, decimal ValorSicor, int LinhasSicor)
{
    /// <summary>A participação, de 0 a 1 (pode passar de 1 no município); nula quando o SICOR não tem crédito.</summary>
    public decimal? Share => ValorSicor > 0 ? decimal.Round(ValorTracbel / ValorSicor, 4) : null;

    /// <summary>O que o SICOR registrou e não foi financiado pela Tracbel — a concorrência, e o que foi financiado por fora.</summary>
    public decimal ValorDaConcorrencia => Math.Max(0, ValorSicor - ValorTracbel);
}

/// <summary>Uma filial: os municípios pelos quais ela responde, somados.</summary>
/// <param name="EmpresaId">A filial.</param>
/// <param name="Filial">O nome.</param>
/// <param name="Municipios">Quantos municípios ela responde.</param>
/// <param name="Share">Os dois lados.</param>
public sealed record ShareDaFilial(int EmpresaId, string Filial, int Municipios, ShareNoRecorte Share);

/// <summary>Um município da ADR — indicativo.</summary>
/// <param name="CodigoIbge">O código do município.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Filial">A filial responsável, quando há.</param>
/// <param name="Share">Os dois lados.</param>
public sealed record ShareNoMunicipio(int CodigoIbge, string Nome, string? Filial, ShareNoRecorte Share)
{
    /// <summary>
    /// A Tracbel financiou mais do que o SICOR registrou: o cliente mora aqui e financiou a máquina noutro município, ou o
    /// contrato caiu noutro mês. Não é erro de conta — é o limite da leitura por município.
    /// </summary>
    public bool AcimaDoSicor => Share.ValorTracbel > Share.ValorSicor;
}

/// <summary>O que a Tracbel financiou por linha de crédito.</summary>
/// <param name="Linha">A linha como o formulário a escreve; "(sem linha)" no formulário antigo.</param>
/// <param name="ContaNoShare">Se entra no share.</param>
/// <param name="Financiamentos">Quantos.</param>
/// <param name="Valor">O valor.</param>
public sealed record FinanciadoPorLinha(string Linha, bool ContaNoShare, int Financiamentos, decimal Valor);

/// <summary>A Região num mês.</summary>
/// <param name="Mes">O mês (dia 1).</param>
/// <param name="ValorTracbel">O crédito rural financiado pela Tracbel.</param>
/// <param name="ValorSicor">O crédito de máquinas do SICOR.</param>
public sealed record ShareNoMes(DateOnly Mes, decimal ValorTracbel, decimal ValorSicor);

/// <summary>O crédito rural da Tracbel na janela que ficou fora do share da Região.</summary>
/// <param name="SemMunicipio">Cliente fora de SP ou sem cidade casada: quantos.</param>
/// <param name="ValorSemMunicipio">E quanto.</param>
/// <param name="ForaDaAdr">Cliente num município de SP fora da ADR: quantos.</param>
/// <param name="ValorForaDaAdr">E quanto.</param>
public sealed record FinanciadoForaDaRegiao(int SemMunicipio, decimal ValorSemMunicipio, int ForaDaAdr, decimal ValorForaDaAdr);
