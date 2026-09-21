using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O estado das fontes públicas e as opções dos formulários de parâmetros (issue 77). Nenhuma das tabelas lidas
/// tem filial: é dado público, o mesmo para a empresa inteira.
/// </summary>
public sealed class RepositorioDeFontesPublicas(CrmDbContext contexto)
    : IRepositorioDeFontesPublicas, IRepositorioDeOpcoesDosParametros
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <inheritdoc />
    public async Task<IReadOnlyList<EstadoDoFluxo>> LerAsync(IReadOnlyList<FontePublica> fontes, CancellationToken ct)
    {
        var fluxos = fontes.Select(f => f.Fluxo).ToList();

        // UM PONTO POR FLUXO, na prática; se houver dois, vale a rodada mais recente.
        var pontos = (await contexto.PontosDeSincronismo.AsNoTracking()
                .Where(p => fluxos.Contains(p.Fluxo))
                .Select(p => new { p.Fluxo, p.ProcessadoEm, p.RegistrosLidos, p.RegistrosGravados, p.RegistrosErro })
                .ToListAsync(ct))
            .GroupBy(p => p.Fluxo)
            .ToDictionary(g => g.Key, g => g.MaxBy(p => p.ProcessadoEm)!, StringComparer.Ordinal);

        var pendentes = (await contexto.MensagensDescartadas.AsNoTracking()
                .Where(m => fluxos.Contains(m.Fluxo) && m.TratadaEm == null)
                .Select(m => new { m.Fluxo, m.Erro })
                .ToListAsync(ct))
            .GroupBy(m => m.Fluxo)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var adr = await IdsDaAdrAsync(ct);
        var estados = new List<EstadoDoFluxo>(fontes.Count);

        foreach (var fonte in fontes)
        {
            var (linhas, inicio, fim, cobertos) = await EstatisticaAsync(fonte.Fluxo, adr, ct);
            var ponto = pontos.GetValueOrDefault(fonte.Fluxo);
            var recusas = pendentes.GetValueOrDefault(fonte.Fluxo) ?? [];

            estados.Add(new EstadoDoFluxo(
                fonte.Fluxo,
                ponto?.ProcessadoEm,
                ponto?.RegistrosLidos ?? 0,
                ponto?.RegistrosGravados ?? 0,
                ponto?.RegistrosErro ?? 0,
                recusas.Count,
                [.. recusas.Select(r => r.Erro).Distinct(StringComparer.Ordinal).Take(3)],
                linhas,
                inicio,
                fim,
                fonte.EhMunicipal ? cobertos : null));
        }

        return estados;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProdutoDaPam>> ListarProdutosDaPamAsync(CancellationToken ct)
    {
        var ano = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking().MaxAsync(p => (short?)p.Ano, ct);
        if (ano is null) return [];

        var produtos = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
            .Where(p => p.Ano == ano)
            .Select(p => new { p.ProdutoCodigoIbge, p.ProdutoNome })
            .Distinct()
            .ToListAsync(ct);

        // A SOMA É FEITA AQUI, e não no banco: o SQLite dos testes não soma decimal, e são poucas linhas
        // (os municípios da ADR × os produtos de um ano).
        var adr = await IdsDaAdrAsync(ct);
        var areas = (await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                .Where(p => p.Ano == ano && adr.Contains(p.MunicipioId))
                .Select(p => new { p.ProdutoCodigoIbge, p.AreaPlantadaHectares })
                .ToListAsync(ct))
            .GroupBy(p => p.ProdutoCodigoIbge)
            .ToDictionary(g => g.Key, g => g.Any(p => p.AreaPlantadaHectares is not null) ? g.Sum(p => p.AreaPlantadaHectares ?? 0) : (decimal?)null);

        return
        [
            .. produtos
                .GroupBy(p => p.ProdutoCodigoIbge)
                .Select(g => new ProdutoDaPam(g.Key, g.First().ProdutoNome, ano.Value, areas.GetValueOrDefault(g.Key)))
                .OrderByDescending(p => p.AreaPlantadaNaAdrHectares ?? -1)
                .ThenBy(p => p.Nome, StringComparer.Create(PtBr, ignoreCase: true))
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MunicipioDoParametro>> ListarMunicipiosDaAdrAsync(CancellationToken ct)
    {
        var municipios = await (
                from area in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
                join municipio in contexto.Municipios.AsNoTracking() on area.MunicipioId equals municipio.Id
                where area.PertenceAAdr && area.EncerradoEm == null && municipio.CodigoIbge != null
                select new MunicipioDoParametro(municipio.Id, municipio.CodigoIbge!.Value, municipio.Nome, municipio.Uf))
            .ToListAsync(ct);

        return [.. municipios.OrderBy(m => m.Nome, StringComparer.Create(PtBr, ignoreCase: true))];
    }

    /// <inheritdoc />
    public async Task<int> ContarMunicipiosDaAdrAsync(CancellationToken ct) => (await IdsDaAdrAsync(ct)).Count;

    private Task<List<int>> IdsDaAdrAsync(CancellationToken ct) =>
        contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
            .Where(a => a.PertenceAAdr && a.EncerradoEm == null)
            .Select(a => a.MunicipioId)
            .Distinct()
            .ToListAsync(ct);

    // =============================================================================================
    // O que cada tabela guarda. UM CASO POR FLUXO DO CATÁLOGO: fluxo novo sem caso aqui cai no
    // `_`, que lança — e o teste da API, que percorre o catálogo inteiro, pega na hora.
    // =============================================================================================

    private Task<(int Linhas, string? Inicio, string? Fim, int? Cobertos)> EstatisticaAsync(
        string fluxo, List<int> adr, CancellationToken ct) => fluxo switch
    {
        "IBGE.PRODUCAO_AGRICOLA" => PorAnoAsync(contexto.ProducoesAgricolasNosMunicipios, p => p.Ano, p => p.MunicipioId, adr, ct),
        "IBGE.FROTA_DE_TRATORES" => PorAnoAsync(contexto.FrotasDeTratoresNosMunicipios, p => p.Ano, p => p.MunicipioId, adr, ct),
        "IBGE.ESTABELECIMENTOS_POR_AREA" => PorAnoAsync(contexto.EstabelecimentosPorAreaNosMunicipios, p => p.Ano, p => p.MunicipioId, adr, ct),
        "IBGE.REBANHO" => PorAnoAsync(contexto.RebanhosNosMunicipios, p => p.Ano, p => p.MunicipioId, adr, ct),
        "IBGE.AREA_TERRITORIAL" => PorAnoAsync(contexto.AreasTerritoriaisDosMunicipios, p => p.Ano, p => p.MunicipioId, adr, ct),
        "ANP.USINA_DE_ETANOL" => PorMesAsync(contexto.UsinasDeEtanol, u => u.MesDeReferencia, ct),
        "CONAB.PRECO_RECEBIDO" => PorMesAsync(contexto.CotacoesDeProdutos.Where(c => c.Fonte == "CONAB"), c => c.Mes, ct),
        "SOCICANA.PRECO_DO_ATR" => PorMesAsync(contexto.CotacoesDeProdutos.Where(c => c.Fonte == "SOCICANA"), c => c.Mes, ct),
        "BCB.PTAX_MENSAL" => PorMesAsync(contexto.CotacoesDoDolar, c => c.Mes, ct),
        "CONAB.CUSTO_DE_PRODUCAO" => PorAnoAsync(contexto.CustosDeProducao, c => c.Safra, null, adr, ct),
        "BCB.SICOR_INVESTIMENTO" => CreditoAsync(adr, ct),
        "BCB.SICOR_TABELAS" => SoLinhasAsync(contexto.ItensDoSicor, ct),
        _ => throw new InvalidOperationException(
            $"O fluxo {fluxo} está em FontesPublicas.Todas e não tem estatística em RepositorioDeFontesPublicas.")
    };

    private static async Task<(int, string?, string?, int?)> PorAnoAsync<T>(
        IQueryable<T> tabela, Expression<Func<T, short>> ano, Expression<Func<T, int>>? municipio, List<int> adr, CancellationToken ct)
        where T : class
    {
        var consulta = tabela.AsNoTracking();
        var linhas = await consulta.CountAsync(ct);
        if (linhas == 0) return (0, null, null, municipio is null ? null : 0);

        var inicio = await consulta.Select(ano).MinAsync(ct);
        var fim = await consulta.Select(ano).MaxAsync(ct);
        int? cobertos = municipio is null
            ? null
            : await consulta.Select(municipio).Where(m => adr.Contains(m)).Distinct().CountAsync(ct);

        return (linhas, inicio.ToString(CultureInfo.InvariantCulture), fim.ToString(CultureInfo.InvariantCulture), cobertos);
    }

    private static async Task<(int, string?, string?, int?)> PorMesAsync<T>(
        IQueryable<T> tabela, Expression<Func<T, DateOnly>> mes, CancellationToken ct) where T : class
    {
        var consulta = tabela.AsNoTracking();
        var linhas = await consulta.CountAsync(ct);
        if (linhas == 0) return (0, null, null, null);

        var inicio = await consulta.Select(mes).MinAsync(ct);
        var fim = await consulta.Select(mes).MaxAsync(ct);
        return (linhas, inicio.ToString("yyyy-MM", CultureInfo.InvariantCulture), fim.ToString("yyyy-MM", CultureInfo.InvariantCulture), null);
    }

    private async Task<(int, string?, string?, int?)> CreditoAsync(List<int> adr, CancellationToken ct)
    {
        var consulta = contexto.CreditosRuraisDeInvestimento.AsNoTracking();
        var linhas = await consulta.CountAsync(ct);
        if (linhas == 0) return (0, null, null, 0);

        // SEM CONTA NO BANCO. "Ano * 100 + Mes" estoura no SQL Server: Ano é smallint, Mes é tinyint, e ele faz a
        // conta no tipo deles — 2024 × 100 passa do smallint. Nem o cast para int salva: o EF o descarta, por ser
        // alargamento. O SQLite dos testes não reproduz; a tela mostrou, em 21/09/2026. O primeiro mês é o menor mês
        // do menor ano, e o último, o maior mês do maior ano.
        var anoInicial = await consulta.MinAsync(c => c.Ano, ct);
        var mesInicial = await consulta.Where(c => c.Ano == anoInicial).MinAsync(c => c.Mes, ct);
        var anoFinal = await consulta.MaxAsync(c => c.Ano, ct);
        var mesFinal = await consulta.Where(c => c.Ano == anoFinal).MaxAsync(c => c.Mes, ct);
        var cobertos = await consulta.Select(c => c.MunicipioId).Where(m => adr.Contains(m)).Distinct().CountAsync(ct);

        return (linhas, $"{anoInicial:0000}-{mesInicial:00}", $"{anoFinal:0000}-{mesFinal:00}", cobertos);
    }

    private static async Task<(int, string?, string?, int?)> SoLinhasAsync<T>(IQueryable<T> tabela, CancellationToken ct) where T : class =>
        (await tabela.AsNoTracking().CountAsync(ct), null, null, null);
}
