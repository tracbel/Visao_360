namespace Tracbel.Crm.Dominio.Portas;

/// <summary>A leitura dos custos de produção da CONAB (issue 67).</summary>
public interface IRepositorioDeCustosDeProducao
{
    /// <summary>Todas as séries de custo de São Paulo, por cultura e local de referência.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<SerieDeCusto>> LerAsync(CancellationToken ct);
}

/// <summary>A série de custo de uma cultura num local de referência — uma linha por aba da CONAB.</summary>
/// <param name="Cultura">A cultura.</param>
/// <param name="Local">O local de referência da CONAB.</param>
/// <param name="Variante">O sistema de cultivo, quando a CONAB o separa.</param>
/// <param name="CodigoIbge">O código IBGE do município do local, quando casou com o catálogo.</param>
/// <param name="UnidadeComercial">A unidade do custo por unidade.</param>
/// <param name="Safras">As abas, da safra mais antiga à mais recente.</param>
/// <param name="Procedencia">De onde esta série veio — CONAB, a localidade e a safra (issue 167).</param>
public sealed record SerieDeCusto(
    string Cultura,
    string Local,
    string? Variante,
    int? CodigoIbge,
    string UnidadeComercial,
    IReadOnlyList<CustoNaSafra> Safras,
    Tracbel.Crm.Dominio.Mercado.ProcedenciaDoIndicador? Procedencia = null);

/// <summary>O custo de uma aba — por hectare e por unidade, nas cinco camadas da CONAB.</summary>
public sealed record CustoNaSafra(
    string Aba,
    short Safra,
    byte? MesDoRelatorio,
    decimal? Produtividade,
    string? UnidadeDaProdutividade,
    decimal CustoVariavelHa,
    decimal CustoFixoHa,
    decimal CustoOperacionalHa,
    decimal? RendaDeFatoresHa,
    decimal? CustoTotalHa,
    decimal CustoOperacionalUnidade,
    decimal? CustoTotalUnidade);
