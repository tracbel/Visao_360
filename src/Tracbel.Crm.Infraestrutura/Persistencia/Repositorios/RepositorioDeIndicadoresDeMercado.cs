using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// OS INDICADORES DE MERCADO DE UM RECORTE (issues 73 e 74) — o momento de preço por cultura e o
/// crédito de um município ou de um recorte, prontos para o fator de ciclo.
///
/// <para><b>Leitura enxuta, e não o painel inteiro.</b> O painel de crédito agrega 200 mil linhas por
/// ano, produto e município para a tela; aqui a consulta filtra os municípios pedidos e duas janelas, e
/// agrega no banco. Reaproveitar o painel faria cada clique em "Simular" pagar a conta da tela.</para>
///
/// <para><b>A vigência é escolhida aqui</b>, pela data pedida — como em todo parâmetro do potencial: o
/// cálculo de uma data passada usa o que valia naquela data.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeIndicadoresDeMercado(CrmDbContext contexto) : IRepositorioDeIndicadoresDeMercado
{
    /// <inheritdoc />
    public Task<IndicadoresDoRecorte> LerAsync(DateOnly data, int? municipioCodigoIbge, CancellationToken ct) =>
        LerDoRecorteAsync(data, municipioCodigoIbge is { } codigo ? [codigo] : [], ct);

    /// <inheritdoc />
    public async Task<IndicadoresDoRecorte> LerDoRecorteAsync(
        DateOnly data, IReadOnlyCollection<int> municipiosCodigoIbge, CancellationToken ct)
    {
        var vigente = ParametroComVigencia.VigenteEm(
            await contexto.ParametrosDoPotencial.AsNoTracking()
                .Where(p => p.RevogadoEm == null && p.VigenteDesde <= data)
                .ToListAsync(ct),
            data);

        var janela = vigente?.MesesDaJanela ?? 12;

        var precos = await MomentoPorCulturaAsync(janela, vigente, ct);
        var credito = municipiosCodigoIbge.Count > 0 ? await CreditoAsync(municipiosCodigoIbge, janela, vigente, ct) : null;
        var percepcao = municipiosCodigoIbge.Count == 1 ? await PercepcaoAsync(municipiosCodigoIbge.First(), data, ct) : null;
        var porCultura = await PercepcaoPorCulturaAsync(data, ct);

        return new IndicadoresDoRecorte(precos.Indices, credito, percepcao, precos.UltimoMes, porCultura);
    }

    /// <summary>
    /// A PERCEPÇÃO DE CAMPO VIGENTE DE CADA CULTURA na data, em pontos percentuais (27/09/2026) — a nota da cultura na
    /// escala da D-P04.
    /// </summary>
    private async Task<Dictionary<string, decimal>> PercepcaoPorCulturaAsync(DateOnly data, CancellationToken ct)
    {
        var linhas = await (
                from p in contexto.PercepcoesDasCulturas.AsNoTracking()
                join c in contexto.Culturas.AsNoTracking() on p.CulturaId equals c.Id
                where p.RevogadoEm == null && p.VigenteDesde <= data
                select new { c.Codigo, Percepcao = p })
            .ToListAsync(ct);

        return linhas
            .GroupBy(l => l.Codigo, StringComparer.Ordinal)
            .Select(g => (g.Key, Vigente: ParametroComVigencia.VigenteEm(g.Select(l => l.Percepcao).ToList(), data)))
            .Where(x => x.Vigente is not null)
            .ToDictionary(x => x.Key, x => x.Vigente!.Percentual, StringComparer.Ordinal);
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
        // A SÉRIE DO ÍNDICE, QUANDO A CULTURA DECLARA UMA (27/09/2026), e senão a do preço. A cana mede o momento pelo
        // ATR mensal da Socicana — mais de dez anos — enquanto o preço dela na Rentabilidade continua o da CONAB.
        var culturas = await contexto.Culturas.AsNoTracking()
            .Where(c => c.EstaAtiva
                        && ((c.FonteDoIndice != null && c.ProdutoDoIndice != null)
                            || (c.FonteDoPreco != null && c.ProdutoDoPreco != null)))
            .Select(c => new
            {
                c.Codigo,
                Fonte = c.FonteDoIndice ?? c.FonteDoPreco,
                Produto = c.ProdutoDoIndice ?? c.ProdutoDoPreco,
                c.NivelDoIndice
            })
            .ToListAsync(ct);

        var indices = new Dictionary<string, IndiceDeMomento>(StringComparer.Ordinal);
        if (culturas.Count == 0) return (indices, null);

        var codigos = culturas.Select(c => c.Produto!).Distinct().ToList();

        var series = (await contexto.CotacoesDeProdutos.AsNoTracking()
                .Where(c => codigos.Contains(c.CodigoNaFonte))
                .Select(c => new { c.Fonte, c.CodigoNaFonte, c.Nivel, c.Mes, c.ValorEmReais })
                .ToListAsync(ct))
            .ToLookup(c => (c.Fonte, c.CodigoNaFonte));

        DateOnly? ultimoMes = null;

        foreach (var cultura in culturas)
        {
            // O NÍVEL SEPARA O MENSAL DO ACUMULADO DA SAFRA na Socicana: o acumulado é média corrida, e misturado ao
            // mensal na mesma janela daria dois "preços" para o mesmo mês.
            var serie = series[(cultura.Fonte!, cultura.Produto!)]
                .Where(c => cultura.NivelDoIndice is null || c.Nivel == cultura.NivelDoIndice)
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

        // ONDE A SÉRIE MENSAL NÃO FECHA AS DUAS JANELAS, O ANUAL DA PAM (decisão do Ricardo em 27/09/2026). A CONAB
        // é janela móvel de 12 meses e o CRM a acumula desde 09/2025: o 12 contra 12 só fecha em 09/2027. Até lá, o
        // momento da cultura é o preço implícito do último ano da PAM contra o anterior — e quando o mensal fechar,
        // ele volta sozinho, porque só se recorre ao anual quando o mensal não saiu.
        var semMensal = indices
            .Where(i => i.Value.Motivo != nameof(MotivoSemIndicador.Nenhum))
            .Select(i => i.Key)
            .ToList();

        foreach (var (codigo, anual) in await MomentoAnualDaPamAsync(semMensal, vigente, ct))
            if (anual.Motivo == nameof(MotivoSemIndicador.Nenhum))
                indices[codigo] = anual;

        return (indices, ultimoMes);
    }

    /// <summary>
    /// O MOMENTO ANUAL PELA PAM das culturas pedidas — o preço implícito (issue 198) do último ano contra o anterior,
    /// somado na área de atuação.
    ///
    /// <para><b>A área de atuação, e não São Paulo inteiro, e não o município</b>: o momento é do mercado em que a
    /// Tracbel vende, e o preço de um município só pode vir de um punhado de produtores (ou sob sigilo). A soma vem
    /// antes da divisão — <c>Σ valor ÷ Σ quantidade</c> —, a média ponderada pela colheita, como na issue 198.</para>
    ///
    /// <para><b>Só os produtos que entram na soma da lavoura</b>: o café entra pelo "Total", e somar Arábica e
    /// Canephora junto dobraria a conta.</para>
    /// </summary>
    private async Task<Dictionary<string, IndiceDeMomento>> MomentoAnualDaPamAsync(
        IReadOnlyCollection<string> culturas, ParametroDoPotencial? vigente, CancellationToken ct)
    {
        var resultado = new Dictionary<string, IndiceDeMomento>(StringComparer.Ordinal);
        if (culturas.Count == 0) return resultado;

        var produtos = await (
                from p in contexto.ProdutosDaPamNasCulturas.AsNoTracking()
                join c in contexto.Culturas.AsNoTracking() on p.CulturaId equals c.Id
                where p.EntraNaSomaDaLavoura && culturas.Contains(c.Codigo)
                select new { c.Codigo, p.ProdutoCodigoIbge })
            .ToListAsync(ct);

        if (produtos.Count == 0) return resultado;

        var codigosIbge = produtos.Select(p => p.ProdutoCodigoIbge).Distinct().ToList();

        var somado = await (
                from p in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                join a in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking() on p.MunicipioId equals a.MunicipioId
                where a.PertenceAAdr && codigosIbge.Contains(p.ProdutoCodigoIbge)
                group p by new { p.ProdutoCodigoIbge, p.Ano } into g
                select new
                {
                    g.Key.ProdutoCodigoIbge,
                    g.Key.Ano,
                    Valor = g.Sum(p => p.ValorDaProducaoMilReais),
                    Quantidade = g.Sum(p => p.QuantidadeProduzida)
                })
            .ToListAsync(ct);

        foreach (var cultura in produtos.GroupBy(p => p.Codigo))
        {
            // UM PRODUTO POR CULTURA NA SOMA, hoje em todas as seis. Se um dia houver dois, o ano soma os dois — e só
            // quando os dois têm a mesma unidade, que é o que o domínio confere ao comparar os anos.
            var doProduto = cultura.Select(p => p.ProdutoCodigoIbge).ToHashSet();
            var principal = cultura.First().ProdutoCodigoIbge;

            var serie = somado
                .Where(s => doProduto.Contains(s.ProdutoCodigoIbge))
                .GroupBy(s => s.Ano)
                .Select(g => PrecoImplicitoDaPam.De(
                    principal,
                    g.Key,
                    g.Any(s => s.Valor is not null) ? g.Sum(s => s.Valor ?? 0m) : null,
                    g.Any(s => s.Quantidade is not null) ? g.Sum(s => s.Quantidade ?? 0m) : null))
                .ToList();

            resultado[cultura.Key] = IndicadoresDeMercado.MomentoAnualDaPam(serie, vigente);
        }

        return resultado;
    }

    /// <summary>
    /// O ÍNDICE DE CRÉDITO DE UM MUNICÍPIO OU DE UM RECORTE — duas janelas de linhas e valor, só dos produtos
    /// de máquina, somadas nos municípios pedidos.
    ///
    /// <para><b>A carência sai dos dois lados</b> (issue 157): o Banco Central acrescenta contrato
    /// registrado com atraso nos meses recentes, e terminar a janela no último mês com dado compara 12
    /// meses cheios com 12 que ainda estão enchendo.</para>
    /// </summary>
    private async Task<IndiceDeCredito?> CreditoAsync(
        IReadOnlyCollection<int> municipiosCodigoIbge, short janela, ParametroDoPotencial? vigente, CancellationToken ct)
    {
        if (vigente is null) return null;

        var codigos = municipiosCodigoIbge.Select(c => (int?)c).ToList();
        var ids = await contexto.Municipios.AsNoTracking()
            .Where(m => codigos.Contains(m.CodigoIbge))
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (ids.Count == 0) return null;

        var maquinas = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

        if (await JanelasDoSicorAsync(janela, vigente, ct) is not { } janelasDoSicor) return null;
        var (inicioAnterior, inicioUltima, fim) = janelasDoSicor;

        var janelas = await contexto.CreditosRuraisDeInvestimento.AsNoTracking()
            .Where(c => ids.Contains(c.MunicipioId)
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

    /// <summary>
    /// AS DUAS JANELAS DO SICOR, contadas em meses corridos (ano × 12 + mês) a partir do último mês de máquina com
    /// dado, descontada a carência; nulas sem crédito carregado.
    /// </summary>
    private async Task<(int InicioAnterior, int InicioUltima, int Fim)?> JanelasDoSicorAsync(
        short janela, ParametroDoPotencial vigente, CancellationToken ct)
    {
        var maquinas = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

        var ultimo = await contexto.CreditosRuraisDeInvestimento.AsNoTracking()
            .Where(c => maquinas.Contains(c.CodigoProduto))
            .MaxAsync(c => (int?)(c.Ano * 12 + c.Mes), ct);

        if (ultimo is not { } fimBruto) return null;

        var fim = fimBruto - (vigente.MesesDeCarenciaDoSicor ?? 0);
        return (fim - (2 * janela - 1), fim - (janela - 1), fim);
    }

    /// <inheritdoc />
    public async Task<IndicadoresPorMunicipio> LerPorMunicipioAsync(
        DateOnly data, IReadOnlyCollection<int> municipiosCodigoIbge, CancellationToken ct)
    {
        var vigente = ParametroComVigencia.VigenteEm(
            await contexto.ParametrosDoPotencial.AsNoTracking()
                .Where(p => p.RevogadoEm == null && p.VigenteDesde <= data)
                .ToListAsync(ct),
            data);

        var janela = vigente?.MesesDaJanela ?? 12;
        var precos = await MomentoPorCulturaAsync(janela, vigente, ct);

        var codigos = municipiosCodigoIbge.Select(c => (int?)c).ToList();
        var municipios = await contexto.Municipios.AsNoTracking()
            .Where(m => codigos.Contains(m.CodigoIbge))
            .Select(m => new { m.Id, Codigo = m.CodigoIbge!.Value })
            .ToListAsync(ct);
        var ids = municipios.Select(m => m.Id).ToList();

        var credito = new Dictionary<int, IndiceDeCredito>();
        if (vigente is not null && ids.Count > 0 && await JanelasDoSicorAsync(janela, vigente, ct) is { } janelasDoSicor)
        {
            var (inicioAnterior, inicioUltima, fim) = janelasDoSicor;
            var maquinas = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

            // A MESMA CONTA DO RECORTE, AGRUPADA POR MUNICÍPIO NO BANCO: uma ida só, em vez de uma por município.
            var porMunicipio = (await contexto.CreditosRuraisDeInvestimento.AsNoTracking()
                    .Where(c => ids.Contains(c.MunicipioId)
                                && maquinas.Contains(c.CodigoProduto)
                                && c.Ano * 12 + c.Mes >= inicioAnterior
                                && c.Ano * 12 + c.Mes <= fim)
                    .GroupBy(c => c.MunicipioId)
                    .Select(g => new
                    {
                        MunicipioId = g.Key,
                        Linhas = g.Count(c => c.Ano * 12 + c.Mes >= inicioUltima),
                        Valor = g.Where(c => c.Ano * 12 + c.Mes >= inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0,
                        LinhasAnteriores = g.Count(c => c.Ano * 12 + c.Mes < inicioUltima),
                        ValorAnterior = g.Where(c => c.Ano * 12 + c.Mes < inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0
                    })
                    .ToListAsync(ct))
                .ToDictionary(x => x.MunicipioId);

            // MUNICÍPIO SEM LINHA NENHUMA TAMBÉM TEM ÍNDICE — o do domínio, que sai vazio com o motivo "sem base". É
            // a mesma resposta que a leitura de um município só daria.
            foreach (var m in municipios)
            {
                var duas = porMunicipio.TryGetValue(m.Id, out var j)
                    ? new JanelasDeCredito(j.Linhas, j.Valor, j.LinhasAnteriores, j.ValorAnterior)
                    : new JanelasDeCredito(0, 0, 0, 0);
                credito[m.Codigo] = IndicadoresDeMercado.Credito(
                    duas, vigente.PesoDosContratosNoCredito, vigente.MinimoDeLinhasNoCredito, vigente);
            }
        }

        var codigoPorId = municipios.ToDictionary(m => m.Id, m => m.Codigo);
        var percepcoes = (await contexto.PercepcoesDoGestor.AsNoTracking()
                .Where(p => ids.Contains(p.MunicipioId) && p.RevogadoEm == null && p.VigenteDesde <= data)
                .ToListAsync(ct))
            .GroupBy(p => p.MunicipioId)
            .Select(g => (Codigo: codigoPorId[g.Key], Vigente: ParametroComVigencia.VigenteEm(g, data)))
            .Where(p => p.Vigente is not null)
            .ToDictionary(p => p.Codigo, p => p.Vigente!.Percentual);

        return new IndicadoresPorMunicipio(precos.Indices, credito, percepcoes, precos.UltimoMes);
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
