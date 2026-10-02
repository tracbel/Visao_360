using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A GESTÃO DE FINANCIAMENTOS — o crédito de mecanização do SICOR com o período, o produto e o programa escolhidos (issue
/// 261, a aba "Gestão de Financiamentos" do protótipo da pasta 360).
///
/// <para><b>Porta própria, e não o painel do crédito com mais parâmetros.</b> O painel (issue 157) compara sempre os
/// últimos 12 meses com os 12 anteriores, todos os produtos; aqui o período é livre, o produto e o programa recortam, e a
/// série vai até o começo do SICOR para a granularidade de mês, trimestre, semestre e ano. São perguntas diferentes, e
/// juntá-las faria cada abertura do painel pagar a conta desta tela.</para>
///
/// <para><b>Linha não é contrato</b> (ver <see cref="JanelasDeCredito"/>): o Banco Central não publica quantidade de
/// contrato. O "contratos (chassi)" do protótipo é, aqui, a contagem de linhas do SICOR — e a tela diz isso.</para>
/// </summary>
public interface IRepositorioDosFinanciamentos
{
    /// <summary>O alcance da série e as opções dos filtros — os produtos de máquina e os programas com crédito deles.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<CatalogoDosFinanciamentos> LerCatalogoAsync(CancellationToken ct);

    /// <summary>Os municípios de São Paulo com o que a área de atuação diz de cada um.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<MunicipioNoCredito>> LerMunicipiosAsync(CancellationToken ct);

    /// <summary>
    /// O crédito de máquinas MÊS A MÊS de um conjunto de municípios, do primeiro ao último mês do SICOR, somado no banco.
    /// </summary>
    /// <param name="filtro">O produto e o programa.</param>
    /// <param name="municipios">Os códigos IBGE do recorte; nulo é São Paulo inteiro (inclusive o município sem código).</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<LinhasDoSicorNoMes>> LerSerieAsync(FiltroDoSicor filtro, IReadOnlySet<int>? municipios, CancellationToken ct);

    /// <summary>
    /// O crédito de máquinas de CADA município de São Paulo nas duas janelas pedidas — o período e o mesmo período do ano
    /// anterior —, somado no banco.
    /// </summary>
    /// <param name="filtro">O produto e o programa.</param>
    /// <param name="periodo">A janela recente.</param>
    /// <param name="anterior">A janela comparada.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<CreditoDoMunicipioNasJanelas>> LerPorMunicipioAsync(
        FiltroDoSicor filtro, PeriodoDoSicor periodo, PeriodoDoSicor anterior, CancellationToken ct);
}

/// <summary>O produto e o programa do SICOR; nulo é todos os produtos de máquina, todos os programas.</summary>
/// <param name="Produto">O código do produto — um dos de máquina.</param>
/// <param name="Programa">O código do programa.</param>
public sealed record FiltroDoSicor(int? Produto = null, int? Programa = null);

/// <summary>
/// Um intervalo de meses do SICOR, os dois inclusive, sempre no dia 1.
/// </summary>
/// <param name="De">O primeiro mês.</param>
/// <param name="Ate">O último mês.</param>
public sealed record PeriodoDoSicor(DateOnly De, DateOnly Ate)
{
    /// <summary>Quantos meses o período tem.</summary>
    public int Meses => (Ate.Year * 12 + Ate.Month) - (De.Year * 12 + De.Month) + 1;

    /// <summary>O mesmo período, doze meses antes — a comparação que respeita a sazonalidade.</summary>
    public PeriodoDoSicor DoAnoAnterior() => new(De.AddMonths(-12), Ate.AddMonths(-12));

    /// <summary>Os últimos <paramref name="meses"/> meses que terminam em <see cref="Ate"/>.</summary>
    /// <param name="meses">Quantos meses.</param>
    public PeriodoDoSicor UltimosMeses(int meses) => new(Ate.AddMonths(-(meses - 1)), Ate);
}

/// <summary>Um código do catálogo do SICOR com o nome publicado.</summary>
/// <param name="Codigo">O código.</param>
/// <param name="Nome">A descrição do Banco Central.</param>
public sealed record OpcaoDoSicor(int Codigo, string Nome);

/// <summary>O alcance da série e as opções dos filtros.</summary>
/// <param name="PrimeiroMes">O mês mais antigo com crédito de máquina; nulo sem SICOR carregado.</param>
/// <param name="UltimoMes">O mais recente.</param>
/// <param name="Produtos">Os produtos de máquina, com o nome.</param>
/// <param name="Programas">Os programas que financiaram máquina em algum mês, com o nome.</param>
public sealed record CatalogoDosFinanciamentos(
    DateOnly? PrimeiroMes,
    DateOnly? UltimoMes,
    IReadOnlyList<OpcaoDoSicor> Produtos,
    IReadOnlyList<OpcaoDoSicor> Programas);

/// <summary>Um município de São Paulo, com o que a área de atuação diz dele.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome oficial.</param>
/// <param name="PertenceAAdr">Se é da ADR.</param>
/// <param name="Regiao">A sub-região da ADR; nula fora da área de atuação.</param>
/// <param name="LojaCodigo">A filial responsável; nula sem loja.</param>
/// <param name="LojaNome">O nome dela.</param>
/// <param name="Usinas">As usinas de etanol vigentes na ANP que ficam aqui.</param>
public sealed record MunicipioNoCredito(
    int CodigoIbge,
    string Nome,
    bool PertenceAAdr,
    string? Regiao,
    string? LojaCodigo,
    string? LojaNome,
    int Usinas);

/// <summary>As linhas e o valor de máquina num mês. Linha do SICOR não é contrato.</summary>
/// <param name="Mes">O mês, no dia 1.</param>
/// <param name="Linhas">As linhas do SICOR.</param>
/// <param name="Valor">O valor financiado, em reais.</param>
public sealed record LinhasDoSicorNoMes(DateOnly Mes, int Linhas, decimal Valor);

/// <summary>O crédito de máquinas de um município nas duas janelas.</summary>
/// <param name="CodigoIbge">O município; nulo no que ainda não tem código do IBGE (entra só no total do estado).</param>
/// <param name="Janelas">As linhas e o valor no período e no mesmo período do ano anterior.</param>
public sealed record CreditoDoMunicipioNasJanelas(int? CodigoIbge, JanelasDeCredito Janelas);
