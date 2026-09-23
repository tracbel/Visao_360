namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// UMA REGRA COMO O MOTOR A LÊ (issues 72 e 161): uma linha por cultura e categoria de máquina, já com
/// os produtos da PAM que compõem a área da cultura e com o grupo de compartilhamento.
///
/// <para><b>Ela nasceu privada dentro do repositório do mapa, e saiu de lá na issue 161.</b> A
/// calculadora precisa exatamente do mesmo catálogo; deixá-la montar o seu daria duas leituras que
/// divergem no dia em que uma mudar — o mesmo defeito que o motor acabou de eliminar no cálculo.</para>
/// </summary>
/// <param name="CulturaCodigo">O código da cultura no catálogo, ou <c>PAM-{produto}</c> quando a regra não está nele.</param>
/// <param name="CulturaNome">O nome de exibição.</param>
/// <param name="CategoriaCodigo">A categoria de máquina, ou <c>SEM-CATEGORIA</c>.</param>
/// <param name="CategoriaNome">O nome da categoria.</param>
/// <param name="HectaresPorMaquina">A regra vigente na data pedida.</param>
/// <param name="AnosDeRenovacao">O ciclo de troca, quando informado.</param>
/// <param name="Confirmada">Se o comercial confirmou a regra (D-P01).</param>
/// <param name="ProdutosDaPam">Os produtos da classificação 782 cuja área soma esta cultura.</param>
/// <param name="GrupoCodigo">O grupo de compartilhamento desta cultura nesta categoria, quando há.</param>
public sealed record RegraNoMotor(
    string CulturaCodigo,
    string CulturaNome,
    string CategoriaCodigo,
    string CategoriaNome,
    decimal HectaresPorMaquina,
    decimal? AnosDeRenovacao,
    bool Confirmada,
    IReadOnlyList<int> ProdutosDaPam,
    string? GrupoCodigo);

/// <summary>Onde um produto da PAM caiu no motor — a categoria e a cultura da regra dele.</summary>
/// <param name="CategoriaCodigo">A categoria de máquina.</param>
/// <param name="CulturaCodigo">A cultura do catálogo.</param>
public sealed record ChaveNoMotor(string CategoriaCodigo, string CulturaCodigo);

/// <summary>
/// O CATÁLOGO QUE O MOTOR USA numa data, mais o de-para que devolve o número de cada regra à ficha dela.
/// </summary>
/// <param name="Regras">Uma linha por cultura e categoria.</param>
/// <param name="PorProdutoDaRegra">Para cada produto com regra vigente, em que categoria e cultura ele caiu.</param>
public sealed record CatalogoDoMotor(
    IReadOnlyList<RegraNoMotor> Regras,
    IReadOnlyDictionary<int, ChaveNoMotor> PorProdutoDaRegra);

/// <summary>
/// A ÁREA PLANTADA MEDIDA DE UM MUNICÍPIO, por cultura do catálogo — o ponto de partida da calculadora.
///
/// <para><b>Cultura ausente do dicionário é ausência de dado</b>, e não zero: o IBGE mantém em sigilo o
/// município com poucos produtores, e a calculadora precisa dizer isso em vez de partir de zero.</para>
/// </summary>
/// <param name="CodigoIbge">O código do município.</param>
/// <param name="Nome">O nome oficial.</param>
/// <param name="AreaPorCultura">A área plantada de cada cultura que o IBGE divulgou aqui.</param>
/// <param name="AnoPorCultura">O ano da PAM de cada cultura — cada uma no seu (issue 152).</param>
public sealed record AreasDoMunicipio(
    int CodigoIbge,
    string Nome,
    IReadOnlyDictionary<string, decimal> AreaPorCultura,
    IReadOnlyDictionary<string, short> AnoPorCultura);

/// <summary>
/// A ENTRADA DO MOTOR DO POTENCIAL (issues 72 e 161) — as regras vigentes numa data e a área medida de
/// um município.
///
/// <para><b>Porta própria, separada do mapa.</b> O mapa territorial cruza cliente, carteira e
/// faturamento; isto aqui é só "o que vale hoje, e quanto tem plantado". Juntos dariam uma porta com
/// duas razões para mudar — e a calculadora não precisa de nada do faturamento.</para>
/// </summary>
public interface IRepositorioDoMotorDoPotencial
{
    /// <summary>As regras vigentes NA DATA, ligadas à cultura, à categoria e ao grupo.</summary>
    /// <param name="data">A data das vigências — "hoje" para o mapa, a data pedida na simulação.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<CatalogoDoMotor> LerCatalogoAsync(DateOnly data, CancellationToken ct);

    /// <summary>
    /// A área plantada de cada cultura do catálogo num município, cada produto no ano dele.
    /// </summary>
    /// <param name="municipioCodigoIbge">O código IBGE de sete dígitos.</param>
    /// <param name="catalogo">O catálogo já lido — é ele que diz quais produtos somam cada cultura.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <returns>Nulo quando o município não está no catálogo de municípios.</returns>
    Task<AreasDoMunicipio?> LerAreasDoMunicipioAsync(int municipioCodigoIbge, CatalogoDoMotor catalogo, CancellationToken ct);
}
