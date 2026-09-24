using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// Os parâmetros do potencial no banco do CRM (issue 71). Nenhuma das três tabelas tem filial: o parâmetro
/// vale para a empresa inteira, e a fronteira de multiempresa não se aplica.
/// </summary>
public sealed class RepositorioDeParametrosDoPotencial(CrmDbContext contexto)
    : IRepositorioDeParametrosDoPotencial,
      IRepositorioDeReferenciasDoPotencial,
      IRepositorioDoCatalogoNoPotencial,
      IRepositorioDeVigenciasDoPotencial
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<RegraDePotencial>> ListarRegrasAsync(CancellationToken ct) =>
        await contexto.RegrasDePotencial.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ParametroDoPotencial>> ListarGeraisAsync(CancellationToken ct) =>
        await contexto.ParametrosDoPotencial.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PercepcaoDoGestor>> ListarPercepcoesAsync(CancellationToken ct) =>
        await contexto.PercepcoesDoGestor.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<long, string>> NomesDosUsuariosAsync(IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<long, string>();

        return await contexto.Usuarios.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NomeExibicao, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, MunicipioDoParametro>> MunicipiosAsync(IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<int, MunicipioDoParametro>();

        return await contexto.Municipios.AsNoTracking()
            .Where(m => ids.Contains(m.Id))
            .Select(m => new MunicipioDoParametro(m.Id, m.CodigoIbge ?? 0, m.Nome, m.Uf))
            .ToDictionaryAsync(m => m.Id, ct);
    }

    /// <inheritdoc />
    public Task<MunicipioDoParametro?> ObterMunicipioPorCodigoIbgeAsync(int codigoIbge, CancellationToken ct) =>
        contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge == codigoIbge)
            .Select(m => new MunicipioDoParametro(m.Id, codigoIbge, m.Nome, m.Uf))
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public Task<string?> ObterNomeDoProdutoNaPamAsync(int produtoCodigoIbge, CancellationToken ct) =>
        contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
            .Where(p => p.ProdutoCodigoIbge == produtoCodigoIbge)
            .OrderByDescending(p => p.Ano)
            .Select(p => p.ProdutoNome)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public Task<ItemDoCatalogoDoPotencial?> ObterCulturaAtivaAsync(string codigo, CancellationToken ct) =>
        contexto.Culturas.AsNoTracking()
            .Where(c => c.Codigo == codigo && c.EstaAtiva)
            .Select(c => new ItemDoCatalogoDoPotencial(c.Id, c.Codigo, c.Nome))
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public Task<ItemDoCatalogoDoPotencial?> ObterCategoriaDeMaquinaAsync(string codigo, bool somenteAtiva, CancellationToken ct) =>
        contexto.CategoriasDeMaquina.AsNoTracking()
            .Where(c => c.Codigo == codigo && (!somenteAtiva || c.EstaAtiva))
            .Select(c => new ItemDoCatalogoDoPotencial(c.Id, c.Codigo, c.Nome))
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial>> CulturasAsync(
        IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<int, ItemDoCatalogoDoPotencial>();

        // SEM FILTRO DE ATIVA: a regra de uma cultura desligada precisa continuar tendo NOME na trilha e na
        // lista de vigências — o histórico dela é o que explica os números do passado.
        return await contexto.Culturas.AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new ItemDoCatalogoDoPotencial(c.Id, c.Codigo, c.Nome))
            .ToDictionaryAsync(c => c.Id, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial>> CategoriasDeMaquinaAsync(
        IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<int, ItemDoCatalogoDoPotencial>();

        return await contexto.CategoriasDeMaquina.AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new ItemDoCatalogoDoPotencial(c.Id, c.Codigo, c.Nome))
            .ToDictionaryAsync(c => c.Id, ct);
    }

    /// <inheritdoc />
    public Task<RegraDePotencial?> ObterRegraAsync(
        int produtoCodigoIbge, int? categoriaDeMaquinaId, DateOnly vigenteDesde, CancellationToken ct) =>
        contexto.RegrasDePotencial
            .FirstOrDefaultAsync(
                r => r.ProdutoCodigoIbge == produtoCodigoIbge
                     && r.CategoriaDeMaquinaId == categoriaDeMaquinaId
                     && r.VigenteDesde == vigenteDesde
                     && r.RevogadoEm == null,
                ct);

    /// <inheritdoc />
    public Task<ParametroDoPotencial?> ObterGeralAsync(DateOnly vigenteDesde, CancellationToken ct) =>
        contexto.ParametrosDoPotencial
            .FirstOrDefaultAsync(p => p.VigenteDesde == vigenteDesde && p.RevogadoEm == null, ct);

    /// <inheritdoc />
    public Task<PercepcaoDoGestor?> ObterPercepcaoAsync(int municipioId, DateOnly vigenteDesde, CancellationToken ct) =>
        contexto.PercepcoesDoGestor
            .FirstOrDefaultAsync(p => p.MunicipioId == municipioId && p.VigenteDesde == vigenteDesde && p.RevogadoEm == null, ct);

    /// <inheritdoc />
    public async Task AdicionarAsync(ParametroComVigencia parametro, CancellationToken ct) =>
        await contexto.AddAsync((object)parametro, ct);
}
