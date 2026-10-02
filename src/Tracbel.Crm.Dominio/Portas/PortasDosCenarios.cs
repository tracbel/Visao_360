using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// AS METAS ESCOLHIDAS NOS CENÁRIOS DE MERCADO (issue 263) — uma por município, categoria e ano fiscal.
///
/// <para>A meta não tem filial: o município é da ADR, e a loja responsável é atributo dele. Por isso a porta devolve, junto
/// do município, a filial responsável — é com ela que o caso de uso confere se quem grava alcança o município.</para>
/// </summary>
public interface IRepositorioDosCenarios
{
    /// <summary>As metas vigentes (não excluídas) de uma categoria num ano fiscal, com o nome de quem gravou por último.</summary>
    /// <param name="categoriaDeMaquinaId">A categoria.</param>
    /// <param name="anoFiscal">O ano fiscal.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<MetaGravadaNoCenario>> ListarAsync(int categoriaDeMaquinaId, short anoFiscal, CancellationToken ct);

    /// <summary>A filial responsável de cada município da área de atuação, pelo código do IBGE — nula quando a fonte não a declara.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyDictionary<int, int?>> FiliaisResponsaveisAsync(CancellationToken ct);

    /// <summary>O município pelo código do IBGE, com a filial responsável na área de atuação; nulo quando não existe.</summary>
    /// <param name="codigoIbge">O código do IBGE.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<MunicipioDoCenario?> ObterMunicipioAsync(int codigoIbge, CancellationToken ct);

    /// <summary>A meta vigente, rastreada para alteração; nula quando ainda não foi escolhida.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="categoriaDeMaquinaId">A categoria.</param>
    /// <param name="anoFiscal">O ano fiscal.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<MetaDoCenarioNoMunicipio?> ObterParaAlterarAsync(int municipioId, int categoriaDeMaquinaId, short anoFiscal, CancellationToken ct);

    /// <summary>Acrescenta uma meta escolhida pela primeira vez.</summary>
    /// <param name="meta">A meta.</param>
    /// <param name="ct">Cancelamento.</param>
    Task AdicionarAsync(MetaDoCenarioNoMunicipio meta, CancellationToken ct);
}

/// <summary>Uma meta gravada, como a tela a mostra.</summary>
/// <param name="CodigoIbge">O município.</param>
/// <param name="Cenario">O cenário escolhido.</param>
/// <param name="ValorManual">O número digitado, no manual.</param>
/// <param name="MetaNaEscolha">A meta combinada, em máquinas.</param>
/// <param name="GravadaPor">Quem gravou por último.</param>
/// <param name="GravadaEm">Quando (UTC).</param>
public sealed record MetaGravadaNoCenario(
    int CodigoIbge, CenarioDeMercado Cenario, decimal? ValorManual, decimal MetaNaEscolha, string? GravadaPor, DateTime GravadaEm);

/// <summary>O município de uma gravação, com o que decide se ela pode acontecer.</summary>
/// <param name="Id">O identificador interno.</param>
/// <param name="CodigoIbge">O código do IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="PertenceAAdr">Se está na área de atuação da Tracbel (a ADR).</param>
/// <param name="EmpresaResponsavelId">A filial responsável pelo município, quando declarada.</param>
public sealed record MunicipioDoCenario(int Id, int CodigoIbge, string Nome, bool PertenceAAdr, int? EmpresaResponsavelId);
