using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A leitura da base de preços de mercado (issue 66).
///
/// <para><b>O dólar é aplicado aqui, na leitura.</b> O banco guarda o preço em reais e o PTAX do mês,
/// separados; o valor em dólar é a divisão dos dois, no mesmo mês. Mês sem PTAX — o corrente, antes do
/// Banco Central fechar a média — sai sem dólar, e não com o dólar do mês anterior.</para>
///
/// <para><b>O volume é pequeno e fechado</b>: 34 produtos da CONAB × 12 meses e 2 séries da Socicana ×
/// ~140 meses cabem numa leitura só, sem paginação. Quando a CONAB acumular anos, continuam sendo
/// poucas centenas de linhas por ano.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDePrecosDeMercado(CrmDbContext contexto) : IRepositorioDePrecosDeMercado
{
    /// <inheritdoc />
    public async Task<PrecosDeMercado> LerAsync(CancellationToken ct)
    {
        var dolar = await contexto.CotacoesDoDolar.AsNoTracking()
            .ToDictionaryAsync(d => d.Mes, d => d.ReaisPorDolar, ct);

        var cotacoes = await contexto.CotacoesDeProdutos.AsNoTracking()
            .OrderBy(c => c.Mes)
            .Select(c => new
            {
                c.Fonte, c.CodigoNaFonte, c.Nivel, c.Produto, c.Classificacao, c.Unidade, c.Mes, c.ValorEmReais
            })
            .ToListAsync(ct);

        var series = cotacoes
            .GroupBy(c => (c.Fonte, c.CodigoNaFonte, c.Nivel))
            .Select(g =>
            {
                // O NOME DA SÉRIE É O DO MÊS MAIS RECENTE: a fonte corrige grafia, e a tela mostra a
                // grafia de hoje.
                var ultimo = g.Last();
                var (unidadeComercial, fator) = UnidadeComercial.Para(ultimo.Produto, ultimo.Unidade);

                return new SerieDePreco(
                    g.Key.Fonte,
                    g.Key.CodigoNaFonte,
                    g.Key.Nivel,
                    ultimo.Produto,
                    ultimo.Classificacao,
                    ultimo.Unidade,
                    unidadeComercial,
                    fator,
                    [.. g.Select(c => new PrecoNoMes(
                        c.Mes,
                        c.ValorEmReais,
                        dolar.TryGetValue(c.Mes, out var reaisPorDolar)
                            ? decimal.Round(c.ValorEmReais / reaisPorDolar, 4)
                            : null))],
                    // CADA SÉRIE CARREGA A PRÓPRIA ORIGEM (issue 167): a fonte e o código são os da
                    // linha, e a competência é o intervalo que ESTA série tem — não um texto fixo que
                    // envelhece quando a carga avança.
                    new ProcedenciaDoIndicador(
                        g.Key.Fonte,
                        $"Preços — {g.Key.Nivel}",
                        g.Key.CodigoNaFonte,
                        ultimo.Produto,
                        $"{g.First().Mes:MM/yyyy} a {ultimo.Mes:MM/yyyy}",
                        DateTime.UtcNow,
                        "O preço é publicado para São Paulo; não existe série municipal. O valor em dólares usa o " +
                        "PTAX do mês, e o mês sem PTAX fica sem valor em dólar — não é zero."));
            })
            .OrderBy(s => s.Produto, StringComparer.Ordinal)
            .ThenBy(s => s.Fonte, StringComparer.Ordinal)
            .ThenBy(s => s.Nivel, StringComparer.Ordinal)
            .ToList();

        return new PrecosDeMercado(
            series,
            dolar.Count == 0 ? null : dolar.Keys.Min(),
            dolar.Count == 0 ? null : dolar.Keys.Max());
    }
}
