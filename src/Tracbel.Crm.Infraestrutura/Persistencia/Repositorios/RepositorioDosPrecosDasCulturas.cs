using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A SÉRIE DE PREÇO DE CADA CULTURA (issue 260) — a mesma escolha do momento de preço: a série do índice quando a cultura
/// declara uma (a cana, pelo ATR mensal da Socicana), e senão a do preço (a CONAB).
///
/// <para><b>O nível separa o mensal do acumulado da safra</b> na Socicana, como no momento: misturados, dariam dois "preços"
/// para o mesmo mês.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDosPrecosDasCulturas(CrmDbContext contexto) : IRepositorioDosPrecosDasCulturas
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SerieDaCultura>> LerSeriesAsync(CancellationToken ct)
    {
        var culturas = await contexto.Culturas.AsNoTracking()
            .Where(c => c.EstaAtiva
                        && ((c.FonteDoIndice != null && c.ProdutoDoIndice != null)
                            || (c.FonteDoPreco != null && c.ProdutoDoPreco != null)))
            .OrderBy(c => c.Id)
            .Select(c => new
            {
                c.Codigo,
                c.Nome,
                Fonte = c.FonteDoIndice ?? c.FonteDoPreco,
                Produto = c.ProdutoDoIndice ?? c.ProdutoDoPreco,
                c.NivelDoIndice
            })
            .ToListAsync(ct);

        if (culturas.Count == 0) return [];

        var codigos = culturas.Select(c => c.Produto!).Distinct().ToList();
        var cotacoes = (await contexto.CotacoesDeProdutos.AsNoTracking()
                .Where(c => codigos.Contains(c.CodigoNaFonte))
                .Select(c => new { c.Fonte, c.CodigoNaFonte, c.Nivel, c.Unidade, c.Mes, c.ValorEmReais })
                .ToListAsync(ct))
            .ToLookup(c => (c.Fonte, c.CodigoNaFonte));

        return
        [
            .. culturas.Select(c =>
            {
                var serie = cotacoes[(c.Fonte!, c.Produto!)]
                    .Where(x => c.NivelDoIndice is null || x.Nivel == c.NivelDoIndice)
                    .OrderBy(x => x.Mes)
                    .ToList();
                return new SerieDaCultura(
                    c.Codigo,
                    c.Nome,
                    c.Fonte!,
                    c.Produto!,
                    serie.Count > 0 ? serie[^1].Unidade : string.Empty,
                    [.. serie.Select(x => new PrecoDaCulturaNoMes(x.Mes, x.ValorEmReais))]);
            })
        ];
    }
}
