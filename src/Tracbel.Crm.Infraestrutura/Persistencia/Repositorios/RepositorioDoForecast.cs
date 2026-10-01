using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O FORECAST DA GERÊNCIA (28/09/2026) — o forecast e o best guess de cada gestor, ao lado do PO (a meta) e do realizado do
/// time dele, num mês.
///
/// <para><b>O time é o de-para de consultores</b> (<see cref="GestorDoConsultor"/>): o consultor da meta e o vendedor do ART
/// entram no gestor dele pela chave da pessoa — a mesma do cartão da meta. A venda cujo vendedor não está no de-para entra
/// só no total, contada à parte.</para>
///
/// <para><b>O PO e o realizado passam pelo filtro global</b> (as filiais ao alcance de quem lê); o forecast, que é do gestor
/// inteiro e não tem filial, não. Por isso a regra de aplicação só abre o relatório a partir do alcance da filial, e diz
/// quando o PO e o realizado não são da organização inteira. Fora de "Todas as filiais", só entra o forecast do gestor que
/// pertence às filiais escolhidas (01/10/2026), e os que ficaram fora são contados.</para>
///
/// <para><b>Somas em memória</b>, como na meta: é um mês de metas, vendas e previsões.</para>
/// </summary>
public sealed class RepositorioDoForecast(CrmDbContext contexto) : IRepositorioDoForecast
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<DateOnly>> MesesComForecastAsync(CancellationToken ct) =>
        await contexto.ForecastsDaGerencia.AsNoTracking()
            .Where(f => f.ExcluidoEm == null)
            .Select(f => f.Competencia)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<ForecastApurado> ApurarAsync(DateOnly competencia, CancellationToken ct, IReadOnlySet<int>? filiaisDoRecorte = null)
    {
        var deParas = await contexto.GestoresDosConsultores.AsNoTracking()
            .Where(g => g.ExcluidoEm == null)
            .Select(g => new { g.ConsultorNaOrigem, g.GestorNaOrigem, g.VigenteDesde, g.IdNaOrigem, g.FilialNumero })
            .ToListAsync(ct);
        var time = GestorDoConsultor.GestorPorConsultor(deParas.Select(g => (g.ConsultorNaOrigem, g.GestorNaOrigem, g.VigenteDesde, g.IdNaOrigem)));

        var todasAsPrevisoes = await contexto.ForecastsDaGerencia.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && f.Competencia == competencia)
            .Select(f => new { f.GestorNaOrigem, f.CodigoDaLinha, f.LinhaNaOrigem, f.Forecast, f.BestGuess })
            .ToListAsync(ct);

        // O PO: a meta de máquinas do mês (sem consórcio, que é à parte), consultor a consultor.
        var metas = (await contexto.MetasDeVenda.AsNoTracking()
                .Where(m => m.ExcluidoEm == null && m.Competencia == competencia)
                .Select(m => new { m.CodigoDaLinha, m.LinhaNaOrigem, m.ConsultorNaOrigem, m.Origem, m.Quantidade })
                .ToListAsync(ct))
            .Where(m => !MetaDeVenda.EhConsorcio(m.Origem, m.CodigoDaLinha))
            .Select(m => (m.CodigoDaLinha, m.LinhaNaOrigem, Consultor: MetaDeVenda.ChaveDaPessoa(m.ConsultorNaOrigem), m.Quantidade))
            .ToList();

        // O REALIZADO: as máquinas ENTREGUES no mês, pela data da entrega — a régua da GN e do cartão da meta (28/09/2026).
        var inicio = competencia;
        var fim = competencia.AddMonths(1);
        var vendas = (await contexto.VendasDeMaquina.AsNoTracking()
                .Where(v => v.ExcluidoEm == null && v.EntregueEm != null && v.EntregueEm >= inicio && v.EntregueEm < fim)
                .Select(v => new { v.LinhaNaOrigem, v.VendedorNaOrigem })
                .ToListAsync(ct))
            .Select(v => (Codigo: CodigoEstavel.De(v.LinhaNaOrigem, MetaDeVenda.TamanhoDaLinha), v.LinhaNaOrigem,
                Vendedor: MetaDeVenda.ChaveDaPessoa(v.VendedorNaOrigem)))
            .ToList();

        string? GestorDe(string consultor) => consultor.Length > 0 && time.TryGetValue(consultor, out var gestor) ? gestor : null;

        // O RECORTE DAS FILIAIS (01/10/2026): o forecast não tem filial, e sem recorte a filial de Ribeirão via o forecast dos
        // gestores de Franca com PO zero ao lado. Fica o gestor que pertence às filiais escolhidas — um consultor do time é
        // delas pelo de-para (o NN da GN é o fim do código 0101NN), ou o time tem meta ou venda nelas (já recortadas pelo
        // filtro global).
        var previsoes = todasAsPrevisoes;
        var foraDoRecorte = 0;
        if (filiaisDoRecorte is not null)
        {
            var codigos = deParas.Where(d => d.FilialNumero is >= 1 and <= 99)
                .Select(d => "0101" + d.FilialNumero!.Value.ToString("00", CultureInfo.InvariantCulture))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var empresaPorCodigo = await contexto.Empresas.AsNoTracking()
                .Where(e => codigos.Contains(e.Codigo))
                .Select(e => new { e.Codigo, e.Id })
                .ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);

            var doRecorte = deParas
                .Where(d => d.FilialNumero is >= 1 and <= 99
                            && GestorDe(MetaDeVenda.ChaveDaPessoa(d.ConsultorNaOrigem)) == d.GestorNaOrigem
                            && empresaPorCodigo.TryGetValue("0101" + d.FilialNumero!.Value.ToString("00", CultureInfo.InvariantCulture), out var empresa)
                            && filiaisDoRecorte.Contains(empresa))
                .Select(d => d.GestorNaOrigem)
                .Concat(metas.Select(m => GestorDe(m.Consultor)).OfType<string>())
                .Concat(vendas.Select(v => GestorDe(v.Vendedor)).OfType<string>())
                .ToHashSet(StringComparer.Ordinal);

            previsoes = [.. todasAsPrevisoes.Where(p => doRecorte.Contains(p.GestorNaOrigem))];
            foraDoRecorte = todasAsPrevisoes.Select(p => p.GestorNaOrigem).Where(g => !doRecorte.Contains(g)).Distinct(StringComparer.Ordinal).Count();
        }

        var nomeDaLinha = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in previsoes) nomeDaLinha.TryAdd(p.CodigoDaLinha, p.LinhaNaOrigem);
        foreach (var m in metas) nomeDaLinha.TryAdd(m.CodigoDaLinha, m.LinhaNaOrigem);
        foreach (var v in vendas) nomeDaLinha.TryAdd(v.Codigo, v.LinhaNaOrigem);

        List<LinhaDoForecast> Linhas(
            IEnumerable<(string Codigo, int? Forecast, int? BestGuess)> previsto,
            IEnumerable<(string Codigo, int Quantidade)> meta,
            IEnumerable<string> vendido)
        {
            var porPrevisao = previsto.GroupBy(p => p.Codigo, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var porMeta = meta.GroupBy(m => m.Codigo, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Sum(m => m.Quantidade), StringComparer.Ordinal);
            var porVenda = vendido.GroupBy(v => v, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            // O FORECAST DE VÁRIAS LINHAS DA ORIGEM SE SOMA; nenhuma informada fica nula, e não zero.
            static int? Somar(IEnumerable<int?> valores) => valores.Any(v => v is not null) ? valores.Sum(v => v ?? 0) : null;

            return [.. porPrevisao.Keys.Concat(porMeta.Keys).Concat(porVenda.Keys).Distinct(StringComparer.Ordinal)
                .Select(codigo => new LinhaDoForecast(
                    codigo,
                    nomeDaLinha.GetValueOrDefault(codigo, codigo),
                    porMeta.GetValueOrDefault(codigo),
                    porPrevisao.TryGetValue(codigo, out var p) ? Somar(p.Select(x => x.Forecast)) : null,
                    porPrevisao.TryGetValue(codigo, out var b) ? Somar(b.Select(x => x.BestGuess)) : null,
                    porVenda.GetValueOrDefault(codigo)))
                .OrderByDescending(l => l.Meta).ThenByDescending(l => l.Forecast ?? 0).ThenBy(l => l.Nome, StringComparer.Ordinal)];
        }

        var gestores = previsoes.Select(p => p.GestorNaOrigem)
            .Concat(metas.Select(m => GestorDe(m.Consultor)).OfType<string>())
            .Concat(vendas.Select(v => GestorDe(v.Vendedor)).OfType<string>())
            .Distinct(StringComparer.Ordinal)
            .Select(gestor => new ForecastDoGestor(
                gestor,
                time.Count(t => t.Value == gestor),
                Linhas(
                    previsoes.Where(p => p.GestorNaOrigem == gestor).Select(p => (p.CodigoDaLinha, p.Forecast, p.BestGuess)),
                    metas.Where(m => GestorDe(m.Consultor) == gestor).Select(m => (m.CodigoDaLinha, m.Quantidade)),
                    vendas.Where(v => GestorDe(v.Vendedor) == gestor).Select(v => v.Codigo))))
            .OrderBy(g => g.Gestor, StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: true))
            .ToList();

        var total = Linhas(
            previsoes.Select(p => (p.CodigoDaLinha, p.Forecast, p.BestGuess)),
            metas.Select(m => (m.CodigoDaLinha, m.Quantidade)),
            vendas.Select(v => v.Codigo));

        var ponto = await contexto.PontosDeSincronismo.AsNoTracking()
            .Where(p => p.Fluxo == ForecastDaGerencia.FluxoDaCarga)
            .Select(p => new { p.ProcessadoEm, p.UltimoValor })
            .FirstOrDefaultAsync(ct);
        DateTime? gerado = ponto is not null
                           && DateTime.TryParse(ponto.UltimoValor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var instante)
            ? DateTime.SpecifyKind(instante.Kind == DateTimeKind.Local ? instante.ToUniversalTime() : instante, DateTimeKind.Utc)
            : null;

        return new ForecastApurado(
            competencia,
            gestores,
            total,
            vendas.Count(v => GestorDe(v.Vendedor) is null),
            ponto is null ? null : DateTime.SpecifyKind(ponto.ProcessadoEm, DateTimeKind.Utc),
            gerado,
            foraDoRecorte);
    }
}
