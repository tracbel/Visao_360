using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia.Cache;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>O território de referência lido do banco — duas consultas, a mesma leitura que a apuração fazia.</summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDoTerritorioDeReferencia(CrmDbContext contexto) : IRepositorioDoTerritorioDeReferencia
{
    private const string SaoPaulo = "SP";

    /// <inheritdoc />
    public async Task<TerritorioDeReferencia> LerAsync(CancellationToken ct)
    {
        var area = await (
                from linha in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking().Where(a => a.EncerradoEm == null)
                join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                join loja in contexto.Empresas.AsNoTracking() on linha.EmpresaResponsavelId equals (int?)loja.Id into lojas
                from loja in lojas.DefaultIfEmpty()
                where municipio.CodigoIbge != null
                select new MunicipioNaAreaDeAtuacao(
                    municipio.CodigoIbge!.Value,
                    municipio.Nome,
                    linha.PertenceAAdr,
                    linha.Regiao,
                    loja == null ? null : loja.Codigo,
                    loja == null ? null : loja.Nome,
                    loja == null ? (bool?)null : loja.EstaAtiva))
            .ToDictionaryAsync(a => a.Codigo, ct);

        var municipios = await contexto.Municipios.AsNoTracking()
            .Select(m => new MunicipioDoCatalogo(m.Id, m.CodigoIbge, m.Uf, m.Nome))
            .ToDictionaryAsync(m => m.Id, ct);

        var nomeOficial = municipios.Values
            .Where(m => m.CodigoIbge is not null && m.Uf == SaoPaulo)
            .GroupBy(m => m.CodigoIbge!.Value)
            .ToDictionary(g => g.Key, g => g.First().Nome);

        return new TerritorioDeReferencia(area, municipios, nomeOficial);
    }
}

/// <summary>
/// O TERRITÓRIO DE REFERÊNCIA GUARDADO (documento 54 §3.2): a conta roda sob contexto de sistema, num banco próprio,
/// porque ela é de todas as telas que esperam por ela — não da requisição que a disparou.
/// </summary>
/// <param name="cache">O cache de referência.</param>
/// <param name="fabrica">De onde sai o banco próprio da conta.</param>
public sealed class TerritorioDeReferenciaEmCache(CacheDeReferencia cache, IServiceScopeFactory fabrica) : IRepositorioDoTerritorioDeReferencia
{
    /// <inheritdoc />
    public Task<TerritorioDeReferencia> LerAsync(CancellationToken ct) =>
        cache.ObterAsync(AssuntoDeReferencia.Territorio, "territorio", CalcularAsync, ct);

    private async Task<TerritorioDeReferencia> CalcularAsync()
    {
        using var escopo = fabrica.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await new RepositorioDoTerritorioDeReferencia(db).LerAsync(CancellationToken.None);
    }
}
