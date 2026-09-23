using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>Uma área informada na simulação.</summary>
/// <param name="CulturaCodigo">O código da cultura no catálogo (ex.: <c>CAFE</c>).</param>
/// <param name="AreaHectares">A área em hectares, com ponto ou vírgula decimal.</param>
public sealed record AreaSimulada(string? CulturaCodigo = null, string? AreaHectares = null);

/// <summary>
/// O QUE SE PERGUNTA À CALCULADORA (issue 161).
///
/// <para><b>Tudo é opcional, e cada ausência tem um sentido:</b> sem município, a simulação parte do
/// zero e vale só o que for digitado; sem área digitada, ela devolve o número medido do município — que
/// é o critério de aceite da issue; sem categoria, entram todas; sem data, vale hoje.</para>
/// </summary>
/// <param name="MunicipioCodigoIbge">O código IBGE de sete dígitos; sem ele, não há área medida de partida.</param>
/// <param name="CategoriaCodigo">A categoria de máquina, para simular só ela.</param>
/// <param name="Data">A data das vigências (aaaa-mm-dd); sem ela, hoje.</param>
/// <param name="Areas">As áreas informadas, que substituem a medida da cultura.</param>
public sealed record SimulacaoDeMaquinas(
    string? MunicipioCodigoIbge = null,
    string? CategoriaCodigo = null,
    string? Data = null,
    IReadOnlyList<AreaSimulada>? Areas = null);

/// <summary>
/// UMA CULTURA QUE A CALCULADORA OFERECE — o que a tela usa para montar o formulário.
///
/// <para><b>A lista vem do servidor, e não do código do front</b> (issue 165): cultura nova entra pelo
/// Administrador e aparece aqui sem publicação.</para>
/// </summary>
/// <param name="Codigo">O código no catálogo.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="CategoriaCodigo">A categoria de máquina da regra.</param>
/// <param name="CategoriaNome">O nome da categoria.</param>
/// <param name="HectaresPorMaquina">A regra vigente na data.</param>
/// <param name="AnosDeRenovacao">O ciclo de troca; nulo quando ninguém informou (D-P01).</param>
/// <param name="RegraConfirmada">Se o comercial confirmou a regra.</param>
/// <param name="AreaMedidaHectares">A área plantada que o IBGE divulgou no município; nula sem município ou sob sigilo.</param>
/// <param name="AnoDaArea">O ano da PAM desta cultura — cada uma no seu.</param>
/// <param name="AreaInformadaHectares">A área que veio na simulação, quando veio.</param>
public sealed record CulturaNaCalculadora(
    string Codigo,
    string Nome,
    string CategoriaCodigo,
    string CategoriaNome,
    decimal HectaresPorMaquina,
    decimal? AnosDeRenovacao,
    bool RegraConfirmada,
    decimal? AreaMedidaHectares,
    short? AnoDaArea,
    decimal? AreaInformadaHectares);

/// <summary>O resultado de uma categoria de máquina na simulação.</summary>
/// <param name="Codigo">O código da categoria.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="ParqueDeMaquinas">As máquinas que a área comporta.</param>
/// <param name="DemandaAnualDeMaquinas">Quantas por ano o parque pede.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta, sem a terra contada duas vezes.</param>
/// <param name="Estimativa">Se alguma regra usada ainda não foi confirmada (D-P01).</param>
/// <param name="MotivoSemParque">Por que o parque não saiu, como texto.</param>
/// <param name="MotivoSemDemanda">Por que a demanda não saiu, como texto.</param>
/// <param name="Frase">O que a tela mostra ao lado do número, ou no lugar dele.</param>
/// <param name="PorCultura">Uma linha por cultura dominante, com quem divide a terra com ela.</param>
public sealed record CategoriaNaCalculadora(
    string Codigo,
    string Nome,
    decimal? ParqueDeMaquinas,
    decimal? DemandaAnualDeMaquinas,
    decimal? AreaUtilHectares,
    bool Estimativa,
    string MotivoSemParque,
    string MotivoSemDemanda,
    string Frase,
    IReadOnlyList<ParcelaDoParque> PorCultura);

/// <summary>
/// O QUE A CALCULADORA RESPONDE.
///
/// <para><b>Nada disto é gravado.</b> Simulação é pergunta; gravar a resposta criaria um número
/// "oficial" que ninguém decidiu adotar.</para>
/// </summary>
/// <param name="Data">A data cujas vigências valeram.</param>
/// <param name="MunicipioCodigoIbge">O município de partida, quando houve.</param>
/// <param name="MunicipioNome">O nome dele.</param>
/// <param name="ParqueDeMaquinas">O parque total, somando as categorias.</param>
/// <param name="DemandaAnualDeMaquinas">A demanda anual total.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta, contada uma vez.</param>
/// <param name="Estimativa">Se alguma regra usada ainda não foi confirmada (D-P01).</param>
/// <param name="MotivoSemParque">Por que o parque não saiu, como texto.</param>
/// <param name="MotivoSemDemanda">Por que a demanda não saiu, como texto.</param>
/// <param name="Frase">O selo de estimativa e o que falta, prontos para a tela.</param>
/// <param name="PorCultura">O parque por cultura dominante.</param>
/// <param name="PorCategoria">O parque por categoria de máquina.</param>
/// <param name="Culturas">As culturas que a calculadora oferece, com a área medida de cada uma.</param>
/// <param name="SobreOsCenarios">Por que o ajuste por cenário de mercado ainda não entra na conta.</param>
public sealed record ResultadoDaCalculadora(
    DateOnly Data,
    int? MunicipioCodigoIbge,
    string? MunicipioNome,
    decimal? ParqueDeMaquinas,
    decimal? DemandaAnualDeMaquinas,
    decimal? AreaUtilHectares,
    bool Estimativa,
    string MotivoSemParque,
    string MotivoSemDemanda,
    string Frase,
    IReadOnlyList<ParcelaDoParque> PorCultura,
    IReadOnlyList<CategoriaNaCalculadora> PorCategoria,
    IReadOnlyList<CulturaNaCalculadora> Culturas,
    string SobreOsCenarios);
