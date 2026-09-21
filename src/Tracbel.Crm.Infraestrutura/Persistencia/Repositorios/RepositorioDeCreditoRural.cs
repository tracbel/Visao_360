using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A leitura do crédito rural do SICOR (issue 68).
///
/// <para><b>Tudo agregado no banco.</b> São 200 mil linhas; a tela precisa de ~14 anos, ~100 produtos e
/// ~600 municípios. As três consultas agrupam no SQL Server e trazem só o resultado.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeCreditoRural(CrmDbContext contexto) : IRepositorioDeCreditoRural
{
    /// <summary>Os três produtos de máquina da planilha do comercial.</summary>
    public static readonly int[] ProdutosDeMaquina = [7080, 4860, 2700];

    /// <inheritdoc />
    public async Task<PainelDeCreditoRural> LerAsync(CancellationToken ct)
    {
        var credito = contexto.CreditosRuraisDeInvestimento.AsNoTracking();

        // PROJEÇÃO ANÔNIMA E ORDEM PELA CHAVE: o EF não traduz ordenação por propriedade de um record
        // construído dentro da consulta (medido: "could not be translated"). O record é montado depois.
        var porAno = (await credito
                .GroupBy(c => c.Ano)
                .Select(g => new
                {
                    Ano = g.Key,
                    LinhasDeMaquinas = g.Count(c => ProdutosDeMaquina.Contains(c.CodigoProduto)),
                    ValorDeMaquinas = g.Where(c => ProdutosDeMaquina.Contains(c.CodigoProduto)).Sum(c => (decimal?)c.Valor) ?? 0,
                    LinhasTotais = g.Count(),
                    ValorTotal = g.Sum(c => c.Valor),
                    UltimoMes = g.Max(c => c.Mes)
                })
                .OrderBy(a => a.Ano)
                .ToListAsync(ct))
            .Select(a => new CreditoNoAno(a.Ano, a.LinhasDeMaquinas, a.ValorDeMaquinas, a.LinhasTotais, a.ValorTotal, a.UltimoMes))
            .ToList();

        if (porAno.Count == 0) return new PainelDeCreditoRural(null, [], [], []);

        var ultimoAno = porAno[^1];
        var ultimoMes = new DateOnly(ultimoAno.Ano, ultimoAno.UltimoMes, 1);

        // AS JANELAS EM "ANO × 12 + MÊS": comparar (ano, mês) com duas colunas exigiria OR encadeado, e o
        // número único vira um BETWEEN que o índice de período atende.
        var fim = ultimoMes.Year * 12 + ultimoMes.Month;
        var inicioUltima = fim - 11;
        var inicioAnterior = fim - 23;

        var janela = credito.Where(c => c.Ano * 12 + c.Mes >= inicioAnterior && c.Ano * 12 + c.Mes <= fim);

        var produtos = await janela
            .GroupBy(c => c.CodigoProduto)
            .Select(g => new
            {
                Codigo = g.Key,
                Linhas = g.Count(c => c.Ano * 12 + c.Mes >= inicioUltima),
                Valor = g.Where(c => c.Ano * 12 + c.Mes >= inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0,
                LinhasAnteriores = g.Count(c => c.Ano * 12 + c.Mes < inicioUltima),
                ValorAnterior = g.Where(c => c.Ano * 12 + c.Mes < inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0
            })
            .ToListAsync(ct);

        var nomes = await contexto.ItensDoSicor.AsNoTracking()
            .Where(i => i.Tipo == "PRODUTO")
            .ToDictionaryAsync(i => i.Codigo, i => i.Descricao, ct);

        var porProduto = produtos
            .Select(p => new CreditoPorProduto(
                p.Codigo,
                nomes.TryGetValue(p.Codigo, out var nome) ? nome : $"Produto {p.Codigo}",
                ProdutosDeMaquina.Contains(p.Codigo),
                new JanelasDeCredito(p.Linhas, p.Valor, p.LinhasAnteriores, p.ValorAnterior)))
            .OrderByDescending(p => p.Janelas.Valor)
            .ToList();

        var municipios = await janela
            .Where(c => ProdutosDeMaquina.Contains(c.CodigoProduto))
            .GroupBy(c => c.MunicipioId)
            .Select(g => new
            {
                MunicipioId = g.Key,
                Linhas = g.Count(c => c.Ano * 12 + c.Mes >= inicioUltima),
                Valor = g.Where(c => c.Ano * 12 + c.Mes >= inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0,
                LinhasAnteriores = g.Count(c => c.Ano * 12 + c.Mes < inicioUltima),
                ValorAnterior = g.Where(c => c.Ano * 12 + c.Mes < inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0
            })
            .ToListAsync(ct);

        var ids = municipios.Select(m => m.MunicipioId).ToList();
        var catalogo = await contexto.Municipios.AsNoTracking()
            .Where(m => ids.Contains(m.Id) && m.CodigoIbge != null)
            .ToDictionaryAsync(m => m.Id, m => new { CodigoIbge = m.CodigoIbge!.Value, m.Nome }, ct);

        var daAdr = (await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
                .Where(a => a.EncerradoEm == null && a.PertenceAAdr)
                .Select(a => a.MunicipioId)
                .ToListAsync(ct))
            .ToHashSet();

        var porMunicipio = municipios
            .Where(m => catalogo.ContainsKey(m.MunicipioId))
            .Select(m => new CreditoDeMaquinasNoMunicipio(
                catalogo[m.MunicipioId].CodigoIbge,
                catalogo[m.MunicipioId].Nome,
                daAdr.Contains(m.MunicipioId),
                new JanelasDeCredito(m.Linhas, m.Valor, m.LinhasAnteriores, m.ValorAnterior)))
            .OrderByDescending(m => m.Janelas.Valor)
            .ToList();

        return new PainelDeCreditoRural(ultimoMes, porAno, porProduto, porMunicipio);
    }
}
