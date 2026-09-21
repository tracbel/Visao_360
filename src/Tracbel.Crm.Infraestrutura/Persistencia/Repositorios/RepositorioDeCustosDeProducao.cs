using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A leitura dos custos de produção (issue 67). Poucas centenas de linhas no total — uma leitura só,
/// agrupada em memória por cultura, local e variante.
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeCustosDeProducao(CrmDbContext contexto) : IRepositorioDeCustosDeProducao
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SerieDeCusto>> LerAsync(CancellationToken ct)
    {
        var linhas = await contexto.CustosDeProducao.AsNoTracking().ToListAsync(ct);

        var ids = linhas.Where(l => l.MunicipioId != null).Select(l => l.MunicipioId!.Value).Distinct().ToList();
        var codigoPorMunicipio = await contexto.Municipios.AsNoTracking()
            .Where(m => ids.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.CodigoIbge, ct);

        return [.. linhas
            .GroupBy(l => (l.Cultura, l.Local, l.Variante))
            .Select(g => new SerieDeCusto(
                g.Key.Cultura,
                g.Key.Local,
                g.Key.Variante,
                g.First().MunicipioId is { } id && codigoPorMunicipio.TryGetValue(id, out var codigo) ? codigo : null,
                g.First().UnidadeComercial,
                [.. g.OrderBy(l => l.Safra).ThenBy(l => l.MesDoRelatorio ?? 0)
                    .Select(l => new CustoNaSafra(
                        l.Aba, l.Safra, l.MesDoRelatorio, l.Produtividade, l.UnidadeDaProdutividade,
                        l.CustoVariavelHa, l.CustoFixoHa, l.CustoOperacionalHa, l.RendaDeFatoresHa, l.CustoTotalHa,
                        l.CustoOperacionalUnidade, l.CustoTotalUnidade))]))
            .OrderBy(s => s.Cultura, StringComparer.Ordinal)
            .ThenBy(s => s.Local, StringComparer.Ordinal)
            .ThenBy(s => s.Variante, StringComparer.Ordinal)];
    }
}
