using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A leitura e a gravação do catálogo de culturas e categorias de máquina (issue 165).
///
/// <para>O catálogo é pequeno — dezenas de linhas — e é lido inteiro: a tela precisa de todas as culturas
/// para desenhar as listas que antes estavam fixas no código dela.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDoCatalogoDoMercado(CrmDbContext contexto) : IRepositorioDoCatalogoDoMercado
{
    /// <inheritdoc />
    public async Task<CatalogoDoMercado> LerAsync(CancellationToken ct)
    {
        var culturas = await contexto.Culturas.AsNoTracking().OrderBy(c => c.Nome).ToListAsync(ct);
        var produtos = await contexto.ProdutosDaPamNasCulturas.AsNoTracking().ToListAsync(ct);
        var categorias = await contexto.CategoriasDeMaquina.AsNoTracking().OrderBy(c => c.Ordem).ToListAsync(ct);
        var doSicor = await contexto.ProdutosDoSicorNasCategorias.AsNoTracking().ToListAsync(ct);

        var porCultura = produtos.ToLookup(p => p.CulturaId);
        var porCategoria = doSicor.ToLookup(p => p.CategoriaDeMaquinaId);

        return new CatalogoDoMercado(
            [.. culturas.Select(c => Montar(c, porCultura))],
            [
                .. categorias.Select(c => new CategoriaNoCatalogo(
                    c.Codigo, c.Nome, c.Ordem, c.EstaAtiva,
                    [.. porCategoria[c.Id].Select(p => p.CodigoProduto).Order()]))
            ]);
    }

    /// <inheritdoc />
    public Task<Cultura?> ObterPorCodigoAsync(string codigo, CancellationToken ct) =>
        contexto.Culturas.FirstOrDefaultAsync(c => c.Codigo == codigo.Trim().ToUpper(), ct);

    /// <inheritdoc />
    public async Task<CulturaNoCatalogo> ObterNaTelaAsync(string codigo, CancellationToken ct)
    {
        var cultura = await contexto.Culturas.AsNoTracking().FirstAsync(c => c.Codigo == codigo, ct);
        var produtos = await contexto.ProdutosDaPamNasCulturas.AsNoTracking()
            .Where(p => p.CulturaId == cultura.Id).ToListAsync(ct);

        return Montar(cultura, produtos.ToLookup(p => p.CulturaId));
    }

    /// <inheritdoc />
    public async Task AdicionarAsync(Cultura cultura, CancellationToken ct)
    {
        await contexto.Culturas.AddAsync(cultura, ct);
    }

    private static CulturaNoCatalogo Montar(Cultura c, ILookup<int, ProdutoDaPamNaCultura> produtos) => new(
        c.Codigo,
        c.Nome,
        c.Segmento.ToString(),
        c.UnidadeComercial,
        c.QuilosPorUnidade,
        c.FonteDoPreco,
        c.ProdutoDoPreco,
        c.SerieDeCusto,
        c.EstaAtiva,
        [
            .. produtos[c.Id]
                .OrderByDescending(p => p.EntraNaSomaDaLavoura)
                .ThenBy(p => p.ProdutoCodigoIbge)
                .Select(p => new ProdutoDaPamNoCatalogo(p.ProdutoCodigoIbge, p.ProdutoNome, p.EntraNaSomaDaLavoura))
        ]);
}
