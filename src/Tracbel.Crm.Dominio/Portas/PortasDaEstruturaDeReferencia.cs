using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A ESTRUTURA DE REFERÊNCIA (plano 2 do documento 54) — o que o IBGE e a ANP publicam sobre cada município e sobre o
/// estado, e os totais da Região Tracbel. Nenhuma tabela tem dono de filial: o resultado é o mesmo para todas as telas.
/// </summary>
public interface IRepositorioDaEstruturaDeReferencia
{
    /// <summary>A estrutura de agora.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<EstruturaDeReferencia> LerAsync(CancellationToken ct);
}

/// <summary>O ano carregado de cada fonte — o que as procedências carimbam ao lado do número.</summary>
/// <param name="Lavoura">O ano mais recente da PAM.</param>
/// <param name="Censo">O ano do Censo Agropecuário (a frota de tratores).</param>
/// <param name="Rebanho">O ano da Pesquisa da Pecuária Municipal.</param>
/// <param name="TemUsina">Se a ANP trouxe alguma usina.</param>
public sealed record AnosDasFontes(short? Lavoura, short? Censo, short? Rebanho, bool TemUsina);

/// <summary>A estrutura inteira.</summary>
/// <param name="PorMunicipio">O que existe em cada município, pelo código IBGE — sem a vocação, que é do recorte.</param>
/// <param name="TotaisDoEstado">O total publicado de São Paulo, ao lado da soma dos municípios.</param>
/// <param name="TotaisDaRegiao">A ADR inteira, sem os filtros da consulta.</param>
/// <param name="Anos">O ano carregado de cada fonte.</param>
public sealed record EstruturaDeReferencia(
    IReadOnlyDictionary<int, EstruturaDoMunicipio> PorMunicipio,
    TotaisDoEstado? TotaisDoEstado,
    TotaisDaRegiaoTracbel? TotaisDaRegiao,
    AnosDasFontes Anos);
