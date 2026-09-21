using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O ESTADO DE CADA FONTE PÚBLICA NO BANCO — a última rodada do fluxo e o que a tabela dele guarda (issue 77).
///
/// <para><b>A última rodada vem do <c>integracao.PontoDeSincronismo</c></b>, que toda carga de fonte pública já
/// grava ao terminar bem. Uma rodada que falha não grava nada — e é por isso que a fonte aparece atrasada, em
/// vez de "em dia com erro".</para>
/// </summary>
public interface IRepositorioDeFontesPublicas
{
    /// <summary>Lê o estado de cada fonte do catálogo.</summary>
    /// <param name="fontes">O catálogo (<see cref="FontesPublicas.Todas"/>).</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<EstadoDoFluxo>> LerAsync(IReadOnlyList<FontePublica> fontes, CancellationToken ct);
}

/// <summary>O que o banco sabe de um fluxo.</summary>
/// <param name="Fluxo">O fluxo.</param>
/// <param name="UltimaAtualizacaoEm">A última rodada bem-sucedida (UTC); nula quando nunca rodou.</param>
/// <param name="RegistrosLidos">Linhas lidas na última rodada.</param>
/// <param name="RegistrosGravados">Linhas gravadas ou alteradas na última rodada.</param>
/// <param name="Recusados">Linhas recusadas na última rodada.</param>
/// <param name="RecusasPendentes">Recusas guardadas para revisão e ainda não tratadas.</param>
/// <param name="ExemplosDeRecusa">Até três motivos de recusa distintos.</param>
/// <param name="Linhas">Linhas da tabela da fonte (da fonte, quando a tabela é dividida).</param>
/// <param name="PeriodoInicial">O primeiro período com dado (ano, safra ou mês aaaa-mm).</param>
/// <param name="PeriodoFinal">O último período com dado.</param>
/// <param name="MunicipiosCobertos">Municípios da ADR com dado; nulo quando a fonte não é por município.</param>
public sealed record EstadoDoFluxo(
    string Fluxo,
    DateTime? UltimaAtualizacaoEm,
    int RegistrosLidos,
    int RegistrosGravados,
    int Recusados,
    int RecusasPendentes,
    IReadOnlyList<string> ExemplosDeRecusa,
    int Linhas,
    string? PeriodoInicial,
    string? PeriodoFinal,
    int? MunicipiosCobertos);

/// <summary>
/// AS OPÇÕES DOS FORMULÁRIOS DE PARÂMETROS — os produtos que a regra pode dividir e os municípios da ADR, para
/// a tela oferecer uma lista em vez de pedir código digitado (issue 77).
/// </summary>
public interface IRepositorioDeOpcoesDosParametros
{
    /// <summary>Os produtos da PAM carregada, do ano mais recente, com a área plantada na ADR.</summary>
    Task<IReadOnlyList<ProdutoDaPam>> ListarProdutosDaPamAsync(CancellationToken ct);

    /// <summary>Os municípios vigentes da ADR, com o código IBGE.</summary>
    Task<IReadOnlyList<MunicipioDoParametro>> ListarMunicipiosDaAdrAsync(CancellationToken ct);

    /// <summary>Quantos municípios a ADR tem hoje — o denominador da cobertura das fontes.</summary>
    Task<int> ContarMunicipiosDaAdrAsync(CancellationToken ct);
}

/// <summary>Um produto da PAM, como a lista de escolha da regra o mostra.</summary>
/// <param name="CodigoIbge">O código da classificação 782.</param>
/// <param name="Nome">O rótulo oficial.</param>
/// <param name="Ano">O ano da PAM de onde veio.</param>
/// <param name="AreaPlantadaNaAdrHectares">A área plantada somada nos municípios da ADR; nula quando não divulgada.</param>
public sealed record ProdutoDaPam(int CodigoIbge, string Nome, short Ano, decimal? AreaPlantadaNaAdrHectares);
