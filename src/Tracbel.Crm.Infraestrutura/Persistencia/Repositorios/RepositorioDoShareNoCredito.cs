using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A leitura do share da Tracbel no crédito de mecanização (issue 262).
///
/// <para><b>Somado no banco, montado aqui.</b> Os dois lados são agrupados por município e mês no SQL Server; a Região,
/// as filiais e os municípios saem da área de atuação vigente, a mesma que o painel do SICOR usa.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDoShareNoCredito(CrmDbContext contexto) : IRepositorioDoShareNoCredito
{
    private static readonly int[] ProdutosDeMaquina = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

    /// <summary>O rótulo do formulário que não tem a linha de crédito.</summary>
    public const string SemLinha = "(sem linha)";

    /// <inheritdoc />
    public async Task<ShareNoCreditoDeMecanizacao> LerAsync(short mesesPorJanela, short? mesesDeCarencia, CancellationToken ct)
    {
        var financiamentos = contexto.FinanciamentosDaVenda.AsNoTracking().Where(f => f.ExcluidoEm == null);
        var ultimoPedido = await financiamentos.MaxAsync(f => (DateOnly?)f.PedidoEm, ct);

        var credito = contexto.CreditosRuraisDeInvestimento.AsNoTracking();
        var ultimo = await credito
            .GroupBy(c => 1)
            .Select(g => new { Numero = g.Max(c => c.Ano * 12 + c.Mes) })
            .FirstOrDefaultAsync(ct);

        var vazio = new FinanciadoForaDaRegiao(0, 0, 0, 0);
        if (ultimo is null) return new ShareNoCreditoDeMecanizacao(null, null, ultimoPedido, null, [], [], [], [], vazio);

        // A MESMA JANELA DO PAINEL DO SICOR: termina no último mês com dado menos a carência, e tem o tamanho do parâmetro.
        var fim = ultimo.Numero - (mesesDeCarencia ?? 0);
        var inicio = fim - (mesesPorJanela - 1);
        var primeiroDia = MesDe(inicio);
        var diaSeguinteAoFim = MesDe(fim).AddMonths(1);

        var sicor = await credito
            .Where(c => ProdutosDeMaquina.Contains(c.CodigoProduto) && c.Ano * 12 + c.Mes >= inicio && c.Ano * 12 + c.Mes <= fim)
            .GroupBy(c => new { c.MunicipioId, c.Ano, c.Mes })
            .Select(g => new { g.Key.MunicipioId, g.Key.Ano, g.Key.Mes, Linhas = g.Count(), Valor = g.Sum(c => c.Valor) })
            .ToListAsync(ct);

        var naJanela = financiamentos.Where(f => f.PedidoEm >= primeiroDia && f.PedidoEm < diaSeguinteAoFim);

        var tracbel = await naJanela
            .Where(f => f.ContaNoCreditoRural)
            .GroupBy(f => new { f.MunicipioId, f.PedidoEm.Year, f.PedidoEm.Month })
            .Select(g => new { g.Key.MunicipioId, Ano = g.Key.Year, Mes = g.Key.Month, Quantos = g.Count(), Valor = g.Sum(f => f.ValorFinanciado) })
            .ToListAsync(ct);

        var porLinhaNoBanco = await naJanela
            .GroupBy(f => new { f.MunicipioId, f.LinhaDeCredito, f.ContaNoCreditoRural })
            .Select(g => new { g.Key.MunicipioId, g.Key.LinhaDeCredito, g.Key.ContaNoCreditoRural, Quantos = g.Count(), Valor = g.Sum(f => f.ValorFinanciado) })
            .ToListAsync(ct);

        // UMA LINHA VIGENTE POR MUNICÍPIO (o índice único filtrado garante), como no painel do SICOR.
        var area = await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
            .Where(a => a.EncerradoEm == null)
            .ToDictionaryAsync(a => a.MunicipioId, a => new { a.PertenceAAdr, a.EmpresaResponsavelId }, ct);
        var daAdr = area.Where(a => a.Value.PertenceAAdr).Select(a => a.Key).ToHashSet();

        var nomesDasFiliais = await contexto.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Id, e => e.Nome, ct);
        string? FilialDe(int municipioId) =>
            area.TryGetValue(municipioId, out var a) && a.EmpresaResponsavelId is { } id && nomesDasFiliais.TryGetValue(id, out var nome)
                ? nome
                : null;

        // ---- por município ----
        var sicorPorMunicipio = sicor.GroupBy(s => s.MunicipioId)
            .ToDictionary(g => g.Key, g => (Linhas: g.Sum(s => s.Linhas), Valor: g.Sum(s => s.Valor)));
        var tracbelPorMunicipio = tracbel.Where(t => t.MunicipioId is not null).GroupBy(t => t.MunicipioId!.Value)
            .ToDictionary(g => g.Key, g => (Quantos: g.Sum(t => t.Quantos), Valor: g.Sum(t => t.Valor)));

        ShareNoRecorte DoMunicipio(int id)
        {
            var s = sicorPorMunicipio.GetValueOrDefault(id);
            var t = tracbelPorMunicipio.GetValueOrDefault(id);
            return new ShareNoRecorte(t.Valor, t.Quantos, s.Valor, s.Linhas);
        }

        static ShareNoRecorte Somar(IEnumerable<ShareNoRecorte> partes) =>
            partes.Aggregate(new ShareNoRecorte(0, 0, 0, 0), (soma, p) => new ShareNoRecorte(
                soma.ValorTracbel + p.ValorTracbel, soma.Financiamentos + p.Financiamentos,
                soma.ValorSicor + p.ValorSicor, soma.LinhasSicor + p.LinhasSicor));

        var regiao = Somar(daAdr.Select(DoMunicipio));

        var comAlgo = daAdr.Where(id => sicorPorMunicipio.ContainsKey(id) || tracbelPorMunicipio.ContainsKey(id)).ToList();
        var catalogo = await contexto.Municipios.AsNoTracking()
            .Where(m => comAlgo.Contains(m.Id) && m.CodigoIbge != null)
            .ToDictionaryAsync(m => m.Id, m => new { CodigoIbge = m.CodigoIbge!.Value, m.Nome }, ct);

        var porMunicipio = comAlgo
            .Where(catalogo.ContainsKey)
            .Select(id => new ShareNoMunicipio(catalogo[id].CodigoIbge, catalogo[id].Nome, FilialDe(id), DoMunicipio(id)))
            .OrderByDescending(m => m.Share.ValorSicor)
            .ThenByDescending(m => m.Share.ValorTracbel)
            .ToList();

        // ---- por filial: os municípios da área que ela responde (da ADR ou não) ----
        var porFilial = area
            .Where(a => a.Value.EmpresaResponsavelId is not null)
            .GroupBy(a => a.Value.EmpresaResponsavelId!.Value)
            .Select(g => new ShareDaFilial(
                g.Key,
                nomesDasFiliais.GetValueOrDefault(g.Key, $"Filial {g.Key}"),
                g.Count(),
                Somar(g.Select(a => DoMunicipio(a.Key)))))
            .Where(f => f.Share.ValorSicor > 0 || f.Share.ValorTracbel > 0)
            .OrderByDescending(f => f.Share.ValorSicor)
            .ToList();

        // ---- por linha, na Região ----
        var porLinha = porLinhaNoBanco
            .Where(l => l.MunicipioId is { } id && daAdr.Contains(id))
            .GroupBy(l => (Linha: string.IsNullOrWhiteSpace(l.LinhaDeCredito) ? SemLinha : l.LinhaDeCredito!, l.ContaNoCreditoRural))
            .Select(g => new FinanciadoPorLinha(g.Key.Linha, g.Key.ContaNoCreditoRural, g.Sum(l => l.Quantos), g.Sum(l => l.Valor)))
            .OrderByDescending(l => l.ContaNoShare)
            .ThenByDescending(l => l.Valor)
            .ToList();

        // ---- mês a mês, na Região ----
        var porMes = new List<ShareNoMes>();
        for (var numero = inicio; numero <= fim; numero++)
        {
            var mes = MesDe(numero);
            porMes.Add(new ShareNoMes(
                mes,
                tracbel.Where(t => t.Ano == mes.Year && t.Mes == mes.Month && t.MunicipioId is { } id && daAdr.Contains(id)).Sum(t => t.Valor),
                sicor.Where(s => s.Ano == mes.Year && s.Mes == mes.Month && daAdr.Contains(s.MunicipioId)).Sum(s => s.Valor)));
        }

        // ---- o que ficou de fora da Região ----
        var semMunicipio = tracbel.Where(t => t.MunicipioId is null).ToList();
        var foraDaAdr = tracbel.Where(t => t.MunicipioId is { } id && !daAdr.Contains(id)).ToList();
        var fora = new FinanciadoForaDaRegiao(
            semMunicipio.Sum(t => t.Quantos), semMunicipio.Sum(t => t.Valor), foraDaAdr.Sum(t => t.Quantos), foraDaAdr.Sum(t => t.Valor));

        return new ShareNoCreditoDeMecanizacao(
            primeiroDia, MesDe(fim), ultimoPedido, regiao, porFilial, porMunicipio, porLinha, porMes, fora);
    }

    /// <summary>O mês (dia 1) de um número "ano × 12 + mês".</summary>
    private static DateOnly MesDe(int numero) => new((numero - 1) / 12, (numero - 1) % 12 + 1, 1);
}
