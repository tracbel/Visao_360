using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A GESTÃO DE FINANCIAMENTOS (issue 261) — o crédito de mecanização do SICOR com o produto e o programa escolhidos.
///
/// <para><b>Tudo agregado no banco</b>, como o painel do crédito: a série sai somada por mês (uns 170 meses), e as janelas
/// por município (uns 600). Os meses são comparados em "ano × 12 + mês", que vira um BETWEEN que o índice de período
/// atende.</para>
///
/// <para><b>Os produtos de máquina são os do domínio</b> (<see cref="ParametroDoPotencial.ProdutosDeMaquinaNoSicor"/>) — a
/// mesma régua do painel do crédito e do índice do fator de ciclo.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDosFinanciamentos(CrmDbContext contexto) : IRepositorioDosFinanciamentos
{
    private const string SaoPaulo = "SP";
    private static readonly int[] ProdutosDeMaquina = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

    /// <inheritdoc />
    public async Task<CatalogoDosFinanciamentos> LerCatalogoAsync(CancellationToken ct)
    {
        var deMaquina = contexto.CreditosRuraisDeInvestimento.AsNoTracking().Where(c => ProdutosDeMaquina.Contains(c.CodigoProduto));

        int? primeiro = null, ultimo = null;
        if (await deMaquina.AnyAsync(ct))
        {
            primeiro = await deMaquina.MinAsync(c => c.Ano * 12 + c.Mes, ct);
            ultimo = await deMaquina.MaxAsync(c => c.Ano * 12 + c.Mes, ct);
        }

        var programas = await deMaquina.Select(c => c.CodigoPrograma).Distinct().ToListAsync(ct);

        var nomes = (await contexto.ItensDoSicor.AsNoTracking()
                .Where(i => i.Tipo == "PRODUTO" || i.Tipo == "PROGRAMA")
                .Select(i => new { i.Tipo, i.Codigo, i.Descricao })
                .ToListAsync(ct))
            .GroupBy(i => (i.Tipo, i.Codigo))
            .ToDictionary(g => g.Key, g => g.First().Descricao);

        string Nome(string tipo, int codigo, string padrao) => nomes.TryGetValue((tipo, codigo), out var nome) ? nome : padrao;

        return new CatalogoDosFinanciamentos(
            primeiro is { } inicio ? MesDe(inicio) : null,
            ultimo is { } fim ? MesDe(fim) : null,
            [.. ProdutosDeMaquina.Select(p => new OpcaoDoSicor(p, Nome("PRODUTO", p, $"Produto {p}")))],
            [.. programas.Order().Select(p => new OpcaoDoSicor(p, Nome("PROGRAMA", p, $"Programa {p}")))]);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MunicipioNoCredito>> LerMunicipiosAsync(CancellationToken ct)
    {
        var municipios = await contexto.Municipios.AsNoTracking()
            .Where(m => m.Uf == SaoPaulo && m.CodigoIbge != null)
            .Select(m => new { m.Id, Codigo = m.CodigoIbge!.Value, m.Nome })
            .ToListAsync(ct);
        var codigoDe = municipios.ToDictionary(m => m.Id, m => m.Codigo);

        var area = (await (
                    from linha in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking().Where(a => a.EncerradoEm == null)
                    join loja in contexto.Empresas.AsNoTracking() on linha.EmpresaResponsavelId equals (int?)loja.Id into lojas
                    from loja in lojas.DefaultIfEmpty()
                    orderby linha.Id
                    select new
                    {
                        linha.MunicipioId,
                        linha.PertenceAAdr,
                        linha.Regiao,
                        LojaCodigo = loja == null ? null : loja.Codigo,
                        LojaNome = loja == null ? null : loja.Nome
                    })
                .ToListAsync(ct))
            .Where(a => codigoDe.ContainsKey(a.MunicipioId))
            .GroupBy(a => codigoDe[a.MunicipioId])
            .ToDictionary(g => g.Key, g => g.First());

        var usinas = (await contexto.UsinasDeEtanol.AsNoTracking().Where(u => u.EncerradaEm == null).Select(u => u.MunicipioId).ToListAsync(ct))
            .Where(codigoDe.ContainsKey)
            .GroupBy(id => codigoDe[id])
            .ToDictionary(g => g.Key, g => g.Count());

        return
        [
            .. municipios
                .GroupBy(m => m.Codigo)
                .Select(g => g.OrderBy(m => m.Id).First())
                .Select(m =>
                {
                    var daArea = area.GetValueOrDefault(m.Codigo);
                    return new MunicipioNoCredito(
                        m.Codigo, m.Nome, daArea?.PertenceAAdr ?? false, daArea?.Regiao.ToString(), daArea?.LojaCodigo, daArea?.LojaNome,
                        usinas.GetValueOrDefault(m.Codigo));
                })
                .OrderBy(m => m.CodigoIbge)
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LinhasDoSicorNoMes>> LerSerieAsync(
        FiltroDoSicor filtro, IReadOnlySet<int>? municipios, CancellationToken ct)
    {
        var consulta = Filtrar(filtro);

        if (municipios is not null)
        {
            var codigos = municipios.ToList();
            var ids = await contexto.Municipios.AsNoTracking()
                .Where(m => m.Uf == SaoPaulo && m.CodigoIbge != null && codigos.Contains(m.CodigoIbge!.Value))
                .Select(m => m.Id)
                .ToListAsync(ct);
            consulta = consulta.Where(c => ids.Contains(c.MunicipioId));
        }

        // PROJEÇÃO ANÔNIMA E ORDEM PELA CHAVE: o EF não traduz a ordem por propriedade de um record montado na consulta.
        return (await consulta
                .GroupBy(c => new { c.Ano, c.Mes })
                .Select(g => new { g.Key.Ano, g.Key.Mes, Linhas = g.Count(), Valor = g.Sum(c => (decimal?)c.Valor) ?? 0 })
                .OrderBy(m => m.Ano).ThenBy(m => m.Mes)
                .ToListAsync(ct))
            .Select(m => new LinhasDoSicorNoMes(new DateOnly(m.Ano, m.Mes, 1), m.Linhas, m.Valor))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CreditoDoMunicipioNasJanelas>> LerPorMunicipioAsync(
        FiltroDoSicor filtro, PeriodoDoSicor periodo, PeriodoDoSicor anterior, CancellationToken ct)
    {
        int Numero(DateOnly mes) => mes.Year * 12 + mes.Month;
        int de = Numero(periodo.De), ate = Numero(periodo.Ate), deAntes = Numero(anterior.De), ateAntes = Numero(anterior.Ate);

        var somas = await Filtrar(filtro)
            .Where(c => (c.Ano * 12 + c.Mes >= de && c.Ano * 12 + c.Mes <= ate) || (c.Ano * 12 + c.Mes >= deAntes && c.Ano * 12 + c.Mes <= ateAntes))
            .GroupBy(c => c.MunicipioId)
            .Select(g => new
            {
                MunicipioId = g.Key,
                Linhas = g.Count(c => c.Ano * 12 + c.Mes >= de && c.Ano * 12 + c.Mes <= ate),
                Valor = g.Where(c => c.Ano * 12 + c.Mes >= de && c.Ano * 12 + c.Mes <= ate).Sum(c => (decimal?)c.Valor) ?? 0,
                LinhasAnteriores = g.Count(c => c.Ano * 12 + c.Mes >= deAntes && c.Ano * 12 + c.Mes <= ateAntes),
                ValorAnterior = g.Where(c => c.Ano * 12 + c.Mes >= deAntes && c.Ano * 12 + c.Mes <= ateAntes).Sum(c => (decimal?)c.Valor) ?? 0
            })
            .ToListAsync(ct);

        var ids = somas.Select(s => s.MunicipioId).ToList();
        var codigoDe = await contexto.Municipios.AsNoTracking()
            .Where(m => ids.Contains(m.Id))
            .Select(m => new { m.Id, m.CodigoIbge, m.Uf })
            .ToDictionaryAsync(m => m.Id, ct);

        // O MUNICÍPIO SEM CÓDIGO DO IBGE CONTINUA NO TOTAL DO ESTADO, como no painel do crédito: ele tem crédito do mesmo jeito.
        return
        [
            .. somas.Select(s => new CreditoDoMunicipioNasJanelas(
                codigoDe.TryGetValue(s.MunicipioId, out var m) && m.Uf == SaoPaulo ? m.CodigoIbge : null,
                new JanelasDeCredito(s.Linhas, s.Valor, s.LinhasAnteriores, s.ValorAnterior)))
        ];
    }

    private IQueryable<CreditoRuralDeInvestimento> Filtrar(FiltroDoSicor filtro)
    {
        var consulta = contexto.CreditosRuraisDeInvestimento.AsNoTracking().Where(c => ProdutosDeMaquina.Contains(c.CodigoProduto));
        if (filtro.Produto is { } produto) consulta = consulta.Where(c => c.CodigoProduto == produto);
        if (filtro.Programa is { } programa) consulta = consulta.Where(c => c.CodigoPrograma == programa);
        return consulta;
    }

    /// <summary>O mês (dia 1) de um número "ano × 12 + mês".</summary>
    private static DateOnly MesDe(int numero) => new((numero - 1) / 12, (numero - 1) % 12 + 1, 1);
}
