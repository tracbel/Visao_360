using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O TERRITÓRIO DE REFERÊNCIA (documento 54 §3.1) — a área de atuação, o catálogo de municípios e as lojas. Igual para
/// todo mundo: nenhuma das tabelas tem dono de filial, e por isso o resultado é compartilhado por todas as telas.
/// </summary>
public interface IRepositorioDoTerritorioDeReferencia
{
    /// <summary>O território de agora.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<TerritorioDeReferencia> LerAsync(CancellationToken ct);
}

/// <summary>Um município da área de atuação, com a loja responsável.</summary>
/// <param name="Codigo">O código do IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="PertenceAAdr">Se a fonte o marca como ADR.</param>
/// <param name="Regiao">A região declarada.</param>
/// <param name="LojaCodigo">A filial responsável, quando declarada.</param>
/// <param name="LojaNome">O nome dela.</param>
/// <param name="LojaAtiva">Se a filial está ativa no CRM.</param>
public sealed record MunicipioNaAreaDeAtuacao(
    int Codigo, string Nome, bool PertenceAAdr, RegiaoDaAreaDeAtuacao Regiao, string? LojaCodigo, string? LojaNome, bool? LojaAtiva);

/// <summary>Um município do catálogo nacional.</summary>
/// <param name="Id">O identificador interno.</param>
/// <param name="CodigoIbge">O código do IBGE, quando reconhecido.</param>
/// <param name="Uf">A UF.</param>
/// <param name="Nome">O nome.</param>
public sealed record MunicipioDoCatalogo(int Id, int? CodigoIbge, string Uf, string Nome);

/// <summary>O território inteiro.</summary>
/// <param name="Area">A área de atuação vigente, pelo código do IBGE.</param>
/// <param name="MunicipiosPorId">O catálogo inteiro de municípios, pelo identificador interno.</param>
/// <param name="NomeOficialEmSp">O nome oficial de cada município de São Paulo, pelo código do IBGE.</param>
public sealed record TerritorioDeReferencia(
    IReadOnlyDictionary<int, MunicipioNaAreaDeAtuacao> Area,
    IReadOnlyDictionary<int, MunicipioDoCatalogo> MunicipiosPorId,
    IReadOnlyDictionary<int, string> NomeOficialEmSp);
