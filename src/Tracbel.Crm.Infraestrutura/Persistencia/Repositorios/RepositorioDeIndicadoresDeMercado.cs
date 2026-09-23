using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// OS INDICADORES DE MERCADO DE UM RECORTE (issues 73 e 74) — o momento de preço por cultura e o
/// crédito de um município, prontos para o fator de ciclo.
///
/// <para><b>Leitura enxuta, e não o painel inteiro.</b> O painel de crédito agrega 200 mil linhas por
/// ano, produto e município para a tela; aqui a consulta filtra <b>um</b> município e duas janelas, e
/// agrega no banco. Reaproveitar o painel faria cada clique em "Simular" pagar a conta da tela.</para>
///
/// <para><b>A vigência é escolhida aqui</b>, pela data pedida — como em todo parâmetro do potencial: o
/// cálculo de uma data passada usa o que valia naquela data.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeIndicadoresDeMercado(CrmDbContext contexto) : IRepositorioDeIndicadoresDeMercado
{
    /// <inheritdoc />
    public async Task<IndicadoresDoRecorte> LerAsync(DateOnly data, int? municipioCodigoIbge, CancellationToken ct)
    {
        var vigente = ParametroComVigencia.VigenteEm(
            await contexto.ParametrosDoPotencial.AsNoTracking()
                .Where(p => p.RevogadoEm == null && p.VigenteDesde <= data)
                .ToListAsync(ct),
            data);

        var janela = vigente?.MesesDaJanela ?? 12;

        var precos = await MomentoPorCulturaAsync(janela, vigente, ct);
        var credito = municipioCodigoIbge is { } codigo ? await CreditoAsync(codigo, janela, vigente, ct) : null;
        var percepcao = municipioCodigoIbge is { } doGestor ? await PercepcaoAsync(doGestor, data, ct) : null;

        return new IndicadoresDoRecorte(precos.Indices, credito, percepcao, precos.UltimoMes);
    }

    /// <summary>
    /// O MOMENTO DE PREÇO DE CADA CULTURA — a média dos meses recentes sobre a dos anteriores.
    ///
    /// <para><b>As janelas terminam no último mês DA SÉRIE DAQUELA CULTURA</b>, e não num mês comum a
    /// todas: a CONAB publica cada produto no seu ritmo, e alinhar tudo pelo mais atrasado descartaria
    /// o mês novo das outras — que é justamente onde a virada aparece.</para>
    /// </summary>
    private async Task<(Dictionary<string, IndiceDeMomento> Indices, DateOnly? UltimoMes)> MomentoPorCulturaAsync(
        short janela, ParametroDoPotencial? vigente, CancellationToken ct)
    {
        var culturas = await contexto.Culturas.AsNoTracking()
            .Where(c => c.EstaAtiva && c.FonteDoPreco != null && c.ProdutoDoPreco != null)
            .Select(c => new { c.Codigo, c.FonteDoPreco, c.ProdutoDoPreco })
            .ToListAsync(ct);

        var indices = new Dictionary<string, IndiceDeMomento>(StringComparer.Ordinal);
        if (culturas.Count == 0) return (indices, null);

        var codigos = culturas.Select(c => c.ProdutoDoPreco!).Distinct().ToList();

        var series = (await contexto.CotacoesDeProdutos.AsNoTracking()
                .Where(c => codigos.Contains(c.CodigoNaFonte))
                .Select(c => new { c.Fonte, c.CodigoNaFonte, c.Mes, c.ValorEmReais })
                .ToListAsync(ct))
            .ToLookup(c => (c.Fonte, c.CodigoNaFonte));

        DateOnly? ultimoMes = null;

        foreach (var cultura in culturas)
        {
            var serie = series[(cultura.FonteDoPreco!, cultura.ProdutoDoPreco!)]
                .OrderBy(c => c.Mes)
                .ToList();

            if (serie.Count == 0)
            {
                indices[cultura.Codigo] = new IndiceDeMomento(
                    null, null, null, null, 0, 0, nameof(MotivoSemIndicador.SemFonte));
                continue;
            }

            if (ultimoMes is null || serie[^1].Mes > ultimoMes) ultimoMes = serie[^1].Mes;

            // AS DUAS JANELAS, CONTADAS DO FIM: os `janela` meses mais recentes contra os `janela`
            // anteriores a eles. Série curta devolve listas menores, e o domínio recusa com o motivo.
            var recentes = serie.TakeLast(janela).Select(c => c.ValorEmReais).ToList();
            var anteriores = serie.SkipLast(janela).TakeLast(janela).Select(c => c.ValorEmReais).ToList();

            indices[cultura.Codigo] = IndicadoresDeMercado.MomentoDePreco(recentes, anteriores, janela, vigente);
        }

        return (indices, ultimoMes);
    }

    /// <summary>
    /// O ÍNDICE DE CRÉDITO DE UM MUNICÍPIO — duas janelas de linhas e valor, só dos produtos de máquina.
    ///
    /// <para><b>A carência sai dos dois lados</b> (issue 157): o Banco Central acrescenta contrato
    /// registrado com atraso nos meses recentes, e terminar a janela no último mês com dado compara 12
    /// meses cheios com 12 que ainda estão enchendo.</para>
    /// </summary>
    private async Task<IndiceDeCredito?> CreditoAsync(
        int municipioCodigoIbge, short janela, ParametroDoPotencial? vigente, CancellationToken ct)
    {
        if (vigente is null) return null;

        var municipioId = await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge == municipioCodigoIbge)
            .Select(m => (int?)m.Id)
            .FirstOrDefaultAsync(ct);

        if (municipioId is not { } id) return null;

        var maquinas = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

        var ultimo = await contexto.CreditosRuraisDeInvestimento.AsNoTracking()
            .Where(c => maquinas.Contains(c.CodigoProduto))
            .MaxAsync(c => (int?)(c.Ano * 12 + c.Mes), ct);

        if (ultimo is not { } fimBruto) return null;

        var fim = fimBruto - (vigente.MesesDeCarenciaDoSicor ?? 0);
        var inicioUltima = fim - (janela - 1);
        var inicioAnterior = fim - (2 * janela - 1);

        var janelas = await contexto.CreditosRuraisDeInvestimento.AsNoTracking()
            .Where(c => c.MunicipioId == id
                        && maquinas.Contains(c.CodigoProduto)
                        && c.Ano * 12 + c.Mes >= inicioAnterior
                        && c.Ano * 12 + c.Mes <= fim)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Linhas = g.Count(c => c.Ano * 12 + c.Mes >= inicioUltima),
                Valor = g.Where(c => c.Ano * 12 + c.Mes >= inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0,
                LinhasAnteriores = g.Count(c => c.Ano * 12 + c.Mes < inicioUltima),
                ValorAnterior = g.Where(c => c.Ano * 12 + c.Mes < inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0
            })
            .FirstOrDefaultAsync(ct);

        var duas = janelas is null
            ? new JanelasDeCredito(0, 0, 0, 0)
            : new JanelasDeCredito(janelas.Linhas, janelas.Valor, janelas.LinhasAnteriores, janelas.ValorAnterior);

        return IndicadoresDeMercado.Credito(
            duas, vigente.PesoDosContratosNoCredito, vigente.MinimoDeLinhasNoCredito, vigente);
    }

    /// <summary>A percepção do gestor vigente na data, em pontos percentuais.</summary>
    private async Task<decimal?> PercepcaoAsync(int municipioCodigoIbge, DateOnly data, CancellationToken ct)
    {
        var municipioId = await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge == municipioCodigoIbge)
            .Select(m => (int?)m.Id)
            .FirstOrDefaultAsync(ct);

        if (municipioId is not { } id) return null;

        var vigencias = await contexto.PercepcoesDoGestor.AsNoTracking()
            .Where(p => p.MunicipioId == id && p.RevogadoEm == null && p.VigenteDesde <= data)
            .ToListAsync(ct);

        return ParametroComVigencia.VigenteEm(vigencias, data)?.Percentual;
    }
}
