using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Potencial;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Aplicacao.Territorio;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>
/// A DEMANDA E A PREVISÃO — quantas máquinas a região pede por ano, quanto a Tracbel tem de entregar pelo share-alvo, e
/// em que mês e em que loja (issue 258, a aba "Análise de Demanda" do protótipo da pasta 360).
///
/// <para><b>Nenhuma conta nova de potencial.</b> O parque e a demanda de cada cultura em cada município são as parcelas
/// do motor que os Indicadores Geográficos já usam (a mesma apuração, os mesmos filtros e o mesmo alcance); o ajuste é
/// o fator de ciclo do CRM — preço de cada cultura, crédito do município e percepção —, e não as elasticidades do
/// protótipo. O que esta leitura acrescenta é a distribuição: pelo share-alvo da categoria e pela sazonalidade mensal,
/// os dois parâmetros com vigência da issue 256.</para>
///
/// <para><b>O mês é o do ano fiscal</b> (novembro a outubro): a previsão mensal começa em novembro, como o ano da
/// Tracbel.</para>
///
/// <para><b>A maquete do Ricardo de 02/10/2026</b> acrescentou, com as decisões dele: a ENTREGA REALIZADA do ART mês a mês,
/// pela data da entrega, no ano fiscal escolhido; o mesmo trecho do ano fiscal anterior para comparar; e a demanda com a
/// área plantada do ANO ANTERIOR da PAM — parque, demanda, culturas com aumento de área e a variação de cada município.
/// O filtro de cultura recorta a demanda; a entrega não tem cultura, e com ele fica de fora, com o motivo.</para>
/// </summary>
public sealed class ObterDemandaEPrevisao(
    IRepositorioIndicadoresTerritoriais territorio,
    IRepositorioDoTerritorioDeReferencia territorioDeReferencia,
    IRepositorioDoPotencialDeReferencia potencialDeReferencia,
    IRepositorioDeIndicadoresDeMercado mercado,
    IRepositorioDoPlanejamento planejamento,
    IRepositorioDeParametrosDoPotencial parametros,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IRepositorioDeEntregas entregas,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Quantos anos fiscais o filtro "Período" oferece: o corrente e os anteriores.</summary>
    public const int AnosFiscaisNoFiltro = 4;
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>A categoria de todas — a soma das que têm demanda.</summary>
    public const string TodasAsCategorias = "TODAS";

    /// <summary>A categoria padrão, a do protótipo: tratores.</summary>
    public const string CategoriaPadrao = "TRATOR";

    /// <summary>Os meses na ordem do ano fiscal da Tracbel: novembro primeiro.</summary>
    public static readonly IReadOnlyList<int> MesesDoAnoFiscal = [11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    /// <summary>Executa a leitura.</summary>
    /// <param name="regiao">Norte ou Noroeste; nulo é a ADR inteira.</param>
    /// <param name="lojaCodigo">A filial responsável; nulo é todas.</param>
    /// <param name="visao"><c>Filial</c> (padrão) ou <c>Empresa</c>.</param>
    /// <param name="categoria">O código da categoria de máquina, ou <c>TODAS</c>. Nulo é trator.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <param name="anoFiscal">O ano fiscal das entregas (<c>2026</c> é nov/2025 a out/2026); nulo é o corrente.</param>
    /// <param name="cultura">O código da cultura que recorta a demanda; nulo é todas.</param>
    public async Task<Resultado<ComProcedencia<DemandaEPrevisaoDaRegiao>>> ExecutarAsync(
        string? regiao, string? lojaCodigo, string? visao, string? categoria, CancellationToken ct,
        string? anoFiscal = null, string? cultura = null)
    {
        var (filtros, erros, semPermissao) = await FiltrosDeAlcance.LerAsync(territorio, acesso.Atual, visao, null, null, null, null, ct);

        var anoFiscalCorrente = AnoFiscal.Do(AnoFiscal.MesCorrenteEmSaoPaulo(relogio.Agora));
        var anoEscolhido = anoFiscalCorrente;
        if (!string.IsNullOrWhiteSpace(anoFiscal))
        {
            if (int.TryParse(anoFiscal.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var lido)
                && lido <= anoFiscalCorrente && lido > anoFiscalCorrente - AnosFiscaisNoFiltro)
                anoEscolhido = lido;
            else
                erros.Registrar("anoFiscal",
                    $"Informe um ano fiscal de {anoFiscalCorrente - AnosFiscaisNoFiltro + 1} a {anoFiscalCorrente} (2026 é nov/2025 a out/2026).",
                    anoFiscal);
        }

        var culturaPedida = string.IsNullOrWhiteSpace(cultura) ? null : cultura.Trim().ToUpperInvariant();

        RegiaoDaAreaDeAtuacao? regiaoEscolhida = null;
        if (!string.IsNullOrWhiteSpace(regiao))
        {
            if (Enum.TryParse<RegiaoDaAreaDeAtuacao>(regiao.Trim(), ignoreCase: true, out var lida) && lida != RegiaoDaAreaDeAtuacao.NaoInformada)
                regiaoEscolhida = lida;
            else
                erros.Registrar("regiao", "A ADR tem duas regiões: Norte e Noroeste.", regiao);
        }

        var codigoDaCategoria = string.IsNullOrWhiteSpace(categoria) ? CategoriaPadrao : categoria.Trim().ToUpperInvariant();
        var todas = codigoDaCategoria == TodasAsCategorias;
        var daCategoria = todas ? null : await catalogo.ObterCategoriaDeMaquinaAsync(codigoDaCategoria, somenteAtiva: false, ct);
        if (!todas && daCategoria is null)
            erros.Registrar(
                "categoria",
                $"Não há categoria de máquina com este código no catálogo. Use o código de uma (TRATOR, COLHEITADEIRA…) ou {TodasAsCategorias}.",
                categoria);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<DemandaEPrevisaoDaRegiao>>("A consulta tem parâmetros que não valem.");
        if (semPermissao is not null)
            return Resultado<ComProcedencia<DemandaEPrevisaoDaRegiao>>.SemPermissao(semPermissao);

        var agora = relogio.Agora;
        var hoje = ParametroComVigencia.HojeNoBrasil(agora);

        // O TERRITÓRIO E O POTENCIAL SÃO DADO DE REFERÊNCIA (documento 54): a mesma conta para todas as telas, guardada pela
        // versão do assunto. A Demanda não lê carteira nem faturamento — nunca usou.
        var doTerritorio = await territorioDeReferencia.LerAsync(ct);
        var potencial = await potencialDeReferencia.LerAsync(hoje, ct);
        var lojaPedida = string.IsNullOrWhiteSpace(lojaCodigo) ? null : lojaCodigo.Trim();
        var comMotor = potencial.Categorias.Count > 0;

        var daAdr = doTerritorio.Area.Values
            .Where(m => m.PertenceAAdr
                        && (regiaoEscolhida is null || m.Regiao == regiaoEscolhida)
                        && (lojaPedida is null || m.LojaCodigo == lojaPedida))
            .OrderBy(m => m.Codigo)
            .Select(m => new MunicipioDaDemanda(
                m.Codigo, m.Nome, m.Regiao.ToString(), m.LojaCodigo, m.LojaNome,
                potencial.DoMunicipio(m.Codigo).Demanda,
                comMotor ? potencial.DoMunicipio(m.Codigo, doAnoAnterior: true).Demanda : []))
            .ToList();
        var deMercado = await mercado.LerPorMunicipioAsync(hoje, [.. daAdr.Select(m => m.CodigoIbge)], ct);
        var gerais = ParametroComVigencia.VigenteEm(await parametros.ListarGeraisAsync(ct), hoje);
        var doPlanejamento = ParametroComVigencia.VigenteEm(await planejamento.ListarParametrosAsync(ct), hoje);
        var shares = (await planejamento.ListarSharesAsync(ct))
            .GroupBy(s => s.CategoriaDeMaquinaId)
            .Select(g => ParametroComVigencia.VigenteEm(g, hoje))
            .OfType<ShareAlvoDaCategoria>()
            .ToList();
        var categoriasDosShares = await catalogo.CategoriasDeMaquinaAsync(shares.Select(s => s.CategoriaDeMaquinaId).ToHashSet(), ct);
        var shareDe = shares
            .Where(s => categoriasDosShares.ContainsKey(s.CategoriaDeMaquinaId))
            .ToDictionary(s => categoriasDosShares[s.CategoriaDeMaquinaId].Codigo, s => s, StringComparer.Ordinal);

        // AS PARCELAS DA CATEGORIA, AJUSTADAS: cada cultura de cada município com o preço dela, o crédito do município e
        // a percepção da cultura somada à do município — a mesma conta do Diagnóstico e dos Indicadores.
        // A CULTURA RECORTA A DEMANDA (02/10/2026): a cultura pedida tem de existir entre as parcelas da categoria no recorte
        // — senão é recusada, e não devolve uma tela vazia sem dizer por quê.
        bool DaCategoria(DemandaNoMunicipio p) => todas || string.Equals(p.CategoriaCodigo, codigoDaCategoria, StringComparison.Ordinal);
        bool DaCultura(DemandaNoMunicipio p) => culturaPedida is null || string.Equals(p.CulturaCodigo, culturaPedida, StringComparison.OrdinalIgnoreCase);

        var culturasDaCategoria = daAdr
            .SelectMany(m => m.DemandaPorCategoriaECultura.Where(DaCategoria))
            .Select(p => (p.CulturaCodigo, p.Cultura))
            .DistinctBy(c => c.CulturaCodigo, StringComparer.Ordinal)
            .OrderBy(c => c.Cultura, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();
        if (culturaPedida is not null && !culturasDaCategoria.Any(c => string.Equals(c.CulturaCodigo, culturaPedida, StringComparison.OrdinalIgnoreCase)))
        {
            var errosDaCultura = new ColetorDeErros();
            errosDaCultura.Registrar("cultura", "Esta cultura não tem demanda na categoria e no recorte escolhidos.", cultura);
            return errosDaCultura.Recusar<ComProcedencia<DemandaEPrevisaoDaRegiao>>("A consulta tem parâmetros que não valem.");
        }

        var parcelas = daAdr
            .SelectMany(m => m.DemandaPorCategoriaECultura
                .Where(p => DaCategoria(p) && DaCultura(p))
                .Select(p =>
                {
                    var ajuste = FatorDeCiclo.Ajustar(
                        p.DemandaAnual,
                        deMercado.PrecoPorCultura.GetValueOrDefault(p.CulturaCodigo)?.Indice,
                        deMercado.CreditoPorMunicipio.GetValueOrDefault(m.CodigoIbge)?.Indice,
                        deMercado.PercepcaoDaCulturaNoMunicipio(m.CodigoIbge, p.CulturaCodigo),
                        gerais);
                    decimal? share = shareDe.TryGetValue(p.CategoriaCodigo, out var s) ? s.Percentual / 100m : null;
                    return new Parcela(m, p, ajuste, share);
                }))
            .ToList();

        var comDemanda = parcelas.Where(p => p.Demanda.DemandaAnual is not null).ToList();
        var fracoes = MesesDoAnoFiscal.ToDictionary(mes => mes, mes => doPlanejamento?.FracaoDoMes(mes) ?? 1m / 12m);

        // O ANO ANTERIOR DA PAM — as mesmas parcelas com a área de um ano antes, cada cultura no dela. Sem ajuste: o momento é
        // o de hoje, e a comparação é de estrutural com estrutural.
        var anteriores = daAdr
            .SelectMany(m => m.DemandaNoAnoAnterior.Where(p => DaCategoria(p) && DaCultura(p)).Select(p => (Municipio: m.CodigoIbge, Demanda: p)))
            .ToList();

        // AS ENTREGAS DO ART NO ANO FISCAL ESCOLHIDO E NO ANTERIOR, pela data da entrega, dos compradores dos municípios do
        // recorte e das categorias da demanda. Sem venda do ART na apuração, ou com uma cultura escolhida, ficam nulas.
        var fiscal = AnoFiscal.Inteiro(anoEscolhido);
        var inicioDoAno = fiscal.Inicial;
        var fimDoAno = fiscal.Final.AddMonths(1);
        var amanha = ParametroComVigencia.HojeNoBrasil(agora).AddDays(1);
        var corte = fimDoAno < amanha ? fimDoAno : amanha;
        var semArt = !await entregas.ExisteVendaAsync(ct);
        var comEntregas = !semArt && culturaPedida is null;
        IReadOnlyList<DateOnly> entregues = [];
        if (comEntregas)
        {
            var doRecorte = daAdr.Select(m => m.CodigoIbge).ToHashSet();
            var categoriasDaDemanda = todas
                ? comDemanda.Select(p => p.Demanda.CategoriaCodigo).ToHashSet(StringComparer.Ordinal)
                : new HashSet<string>([codigoDaCategoria], StringComparer.Ordinal);
            entregues = [.. (await entregas.ListarAsync(inicioDoAno.AddYears(-1), fimDoAno, ct))
                .Where(e => e.MunicipioIbge is { } codigo && doRecorte.Contains(codigo)
                            && e.CategoriaCodigo is { } c && categoriasDaDemanda.Contains(c))
                .Select(e => e.EntregueEm)];
        }

        var totais = Totais(parcelas, comDemanda, daAdr.Count, potencial.RegrasAplicadas, codigoDaCategoria, todas) with
        {
            ParqueAnoAnterior = Numeros.Arredondar(Numeros.SomaOuNulo(anteriores.Select(a => a.Demanda.Parque))),
            DemandaEstruturalAnoAnterior = Numeros.Arredondar(Numeros.SomaOuNulo(anteriores.Select(a => a.Demanda.DemandaAnual))),
            EntreguesNoPeriodo = comEntregas ? entregues.Count(d => d >= inicioDoAno && d < corte) : null,
            EntreguesNoPeriodoAnterior = comEntregas ? entregues.Count(d => d >= inicioDoAno.AddYears(-1) && d < corte.AddYears(-1)) : null,
            EntregasAte = comEntregas ? corte.AddDays(-1) : null
        };
        var porCultura = PorCultura(parcelas, anteriores.Select(a => a.Demanda).ToList());
        totais = totais with
        {
            CulturasComAumentoDeArea =
            [
                .. porCultura
                    .Where(c => c.AreaUtilHectares is { } atual && c.AreaAnoAnterior is { } antes && atual > antes)
                    .Select(c => c.Cultura)
            ]
        };

        var mesCorrenteDoAno = AnoFiscal.MesCorrenteEmSaoPaulo(agora);
        var previsao = MesesDoAnoFiscal
            .Select(mes =>
            {
                // O MÊS NO ANO FISCAL ESCOLHIDO: novembro e dezembro são do ano civil anterior. Mês que ainda não começou não
                // tem entrega — nulo, e não zero.
                var competencia = new DateOnly(mes >= AnoFiscal.MesInicial ? anoEscolhido - 1 : anoEscolhido, mes, 1);
                int? noMes = comEntregas && competencia <= mesCorrenteDoAno
                    ? entregues.Count(d => d.Year == competencia.Year && d.Month == competencia.Month)
                    : null;
                return new PrevisaoDoMes(
                    mes,
                    decimal.Round(fracoes[mes], 4),
                    Vezes(totais.DemandaEstrutural, fracoes[mes]),
                    Vezes(totais.DemandaAjustada, fracoes[mes]),
                    Vezes(totais.AEntregar, fracoes[mes]),
                    Vezes(totais.AEntregarAjustada, fracoes[mes]),
                    noMes);
            })
            .ToList();

        var porLoja = comDemanda
            .GroupBy(p => (p.Municipio.LojaCodigo, p.Municipio.LojaNome))
            .Select(g =>
            {
                var anual = Numeros.SomaOuNulo(g.Select(p => p.AEntregarAjustada ?? p.AEntregar));
                return new EntregaDaLoja(
                    g.Key.LojaCodigo,
                    g.Key.LojaNome ?? "Sem loja responsável",
                    Numeros.Arredondar(anual),
                    [.. MesesDoAnoFiscal.Select(mes => Vezes(anual, fracoes[mes]))]);
            })
            .OrderByDescending(l => l.AEntregarNoAno ?? -1)
            .ToList();

        var municipios = daAdr
            .Select(m => Municipio(
                m,
                parcelas.Where(p => p.Municipio.CodigoIbge == m.CodigoIbge).ToList(),
                [.. anteriores.Where(a => a.Municipio == m.CodigoIbge).Select(a => a.Demanda)]))
            .OrderByDescending(m => m.DemandaAjustada ?? m.DemandaEstrutural ?? -1)
            .ThenBy(m => m.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        var culturas = parcelas
            .GroupBy(p => (p.Demanda.CulturaCodigo, p.Demanda.Cultura))
            .Select(g => new CulturaDaMatriz(g.Key.CulturaCodigo, g.Key.Cultura, Numeros.Arredondar(Numeros.SomaOuNulo(g.Select(p => p.Demanda.DemandaAnual)))))
            .OrderByDescending(c => c.Demanda ?? -1)
            .ToList();

        var ordem = filtros!.Categorias.ToDictionary(c => c.Codigo, c => c.Ordem, StringComparer.Ordinal);
        var categoriasComDemanda = daAdr
            .SelectMany(m => m.DemandaPorCategoriaECultura)
            .Where(p => p.DemandaAnual is not null)
            .Select(p => (p.CategoriaCodigo, p.CategoriaNome))
            .Distinct()
            .Select(c => new CategoriaParaFiltro(c.CategoriaCodigo, c.CategoriaNome, ordem.GetValueOrDefault(c.CategoriaCodigo, short.MaxValue)))
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        var shareDaCategoria = todas ? null : shareDe.GetValueOrDefault(codigoDaCategoria);

        var resultado = new DemandaEPrevisaoDaRegiao(
            codigoDaCategoria,
            todas ? "Todas as categorias" : daCategoria!.Nome,
            categoriasComDemanda,
            shareDaCategoria?.Percentual,
            shareDaCategoria is { InformadoPorId: null },
            doPlanejamento?.VigenteDesde,
            doPlanejamento is { InformadoPorId: null },
            potencial.AnoDaAreaPlantada,
            totais,
            porCultura,
            previsao,
            porLoja,
            municipios,
            culturas,
            Lacunas(potencial.RegrasAplicadas, doPlanejamento, gerais, todas, codigoDaCategoria, shareDe, comDemanda, semArt, culturaPedida is not null, anteriores.Count > 0),
            anoEscolhido,
            [.. Enumerable.Range(0, AnosFiscaisNoFiltro).Select(i => anoFiscalCorrente - i)],
            culturaPedida is null ? null : culturasDaCategoria.First(c => string.Equals(c.CulturaCodigo, culturaPedida, StringComparison.OrdinalIgnoreCase)).CulturaCodigo,
            [.. culturasDaCategoria.Select(c => new CulturaDaMatriz(c.CulturaCodigo, c.Cultura, null))],
            potencial.AnoDaAreaPlantada is { } anoDaArea ? (short)(anoDaArea - 1) : null);

        return Resultado<ComProcedencia<DemandaEPrevisaoDaRegiao>>.Ok(
            ComProcedencia<DemandaEPrevisaoDaRegiao>.DoNossoBanco(
                resultado,
                "motor do potencial (área plantada da PAM × regras do potencial) · fator de ciclo · share-alvo e sazonalidade do planejamento",
                relogio));
    }

    /// <summary>Um município da ADR no recorte, com a demanda do motor — o que a Demanda usa da área e do potencial.</summary>
    private sealed record MunicipioDaDemanda(
        int CodigoIbge, string Nome, string Regiao, string? LojaCodigo, string? LojaNome,
        IReadOnlyList<DemandaNoMunicipio> DemandaPorCategoriaECultura, IReadOnlyList<DemandaNoMunicipio> DemandaNoAnoAnterior);

    private sealed record Parcela(MunicipioDaDemanda Municipio, DemandaNoMunicipio Demanda, PotencialAjustado Ajuste, decimal? Share)
    {
        public decimal? AEntregar => Demanda.DemandaAnual is { } d && Share is { } s ? d * s : null;
        public decimal? AEntregarAjustada => Ajuste.DemandaAjustada is { } d && Share is { } s ? d * s : null;
    }

    private static TotaisDaDemanda Totais(
        List<Parcela> parcelas, List<Parcela> comDemanda, int municipios, IReadOnlyList<RegraDePotencialAplicada> regras,
        string categoria, bool todas)
    {
        var ajustadas = comDemanda.Select(p => p.Ajuste.DemandaAjustada).ToList();
        var culturasComRegra = regras
            .Where(r => todas || string.Equals(r.CategoriaCodigo, categoria, StringComparison.Ordinal))
            .Select(r => r.ProdutoNome)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        return new TotaisDaDemanda(
            Numeros.Arredondar(Numeros.SomaOuNulo(parcelas.Select(p => p.Demanda.Parque))),
            Numeros.Arredondar(Numeros.SomaOuNulo(comDemanda.Select(p => p.Demanda.DemandaAnual))),
            ajustadas.Count > 0 && ajustadas.All(a => a is not null) ? Numeros.Arredondar(ajustadas.Sum()) : null,
            Numeros.Arredondar(Numeros.SomaOuNulo(comDemanda.Select(p => p.AEntregar))),
            Numeros.Arredondar(Numeros.SomaOuNulo(comDemanda.Select(p => p.AEntregarAjustada))),
            culturasComRegra,
            municipios,
            comDemanda.Select(p => p.Municipio.CodigoIbge).Distinct().Count(),
            parcelas.Any(p => p.Ajuste.Fator.Estimativa));
    }

    private static List<DemandaDaCultura> PorCultura(List<Parcela> parcelas, List<DemandaNoMunicipio> anteriores) =>
    [
        .. parcelas
            .GroupBy(p => (p.Demanda.CulturaCodigo, p.Demanda.Cultura))
            .Select(g =>
            {
                var area = Numeros.SomaOuNulo(g.Select(p => p.Demanda.AreaUtilHectares));
                var parque = Numeros.SomaOuNulo(g.Select(p => p.Demanda.Parque));
                var demanda = Numeros.SomaOuNulo(g.Select(p => p.Demanda.DemandaAnual));
                var ajustadas = g.Where(p => p.Demanda.DemandaAnual is not null).Select(p => p.Ajuste.DemandaAjustada).ToList();
                decimal? ajustada = ajustadas.Count > 0 && ajustadas.All(a => a is not null) ? ajustadas.Sum() : null;
                var antes = anteriores.Where(a => string.Equals(a.CulturaCodigo, g.Key.CulturaCodigo, StringComparison.Ordinal)).ToList();
                return new DemandaDaCultura(
                    g.Key.CulturaCodigo,
                    g.Key.Cultura,
                    Numeros.Arredondar(area),
                    // OS PARÂMETROS SAEM DA PRÓPRIA CONTA: ha/máquina = área ÷ parque e ciclo = parque ÷ demanda. Numa
                    // cultura com uma regra só, são exatamente os da regra; com duas categorias, é a média delas.
                    area is > 0 && parque is > 0 ? decimal.Round(area.Value / parque.Value, 1) : null,
                    parque is > 0 && demanda is > 0 ? decimal.Round(parque.Value / demanda.Value, 1) : null,
                    Numeros.Arredondar(parque),
                    Numeros.Arredondar(demanda),
                    Numeros.Arredondar(ajustada),
                    demanda is > 0 && ajustada is { } a ? decimal.Round((a / demanda.Value - 1) * 100m, 1) : null,
                    Numeros.Arredondar(Numeros.SomaOuNulo(antes.Select(x => x.AreaUtilHectares))),
                    Numeros.Arredondar(Numeros.SomaOuNulo(antes.Select(x => x.Parque))),
                    Numeros.Arredondar(Numeros.SomaOuNulo(antes.Select(x => x.DemandaAnual))));
            })
            .OrderByDescending(c => c.DemandaEstrutural ?? -1)
    ];

    private static DemandaDoMunicipioNaPrevisao Municipio(MunicipioDaDemanda m, List<Parcela> doMunicipio, List<DemandaNoMunicipio> anteriores)
    {
        var comDemanda = doMunicipio.Where(p => p.Demanda.DemandaAnual is not null).ToList();
        var estrutural = Numeros.SomaOuNulo(comDemanda.Select(p => p.Demanda.DemandaAnual));
        var antes = Numeros.SomaOuNulo(anteriores.Select(a => a.DemandaAnual));
        var ajustadas = comDemanda.Select(p => p.Ajuste.DemandaAjustada).ToList();
        decimal? ajustada = ajustadas.Count > 0 && ajustadas.All(a => a is not null) ? ajustadas.Sum() : null;

        // OS FATORES DO MUNICÍPIO PONDERADOS PELA DEMANDA de cada cultura: o preço é da cultura, o crédito é do município
        // — e as duas parcelas podem ir para lados opostos, que é o que o protótipo mostra na matriz.
        decimal? Ponderado(Func<FatorDoCiclo, decimal?> parcela)
        {
            var com = comDemanda.Where(p => parcela(p.Ajuste.Fator) is not null).ToList();
            var peso = com.Sum(p => p.Demanda.DemandaAnual!.Value);
            return peso > 0 ? decimal.Round(1m + com.Sum(p => parcela(p.Ajuste.Fator)!.Value * p.Demanda.DemandaAnual!.Value) / peso, 3) : null;
        }

        var predominante = doMunicipio
            .GroupBy(p => p.Demanda.Cultura)
            .Select(g => (Cultura: g.Key, Parque: g.Sum(p => p.Demanda.Parque ?? 0)))
            .Where(x => x.Parque > 0)
            .OrderByDescending(x => x.Parque)
            .Select(x => x.Cultura)
            .FirstOrDefault();

        return new DemandaDoMunicipioNaPrevisao(
            m.CodigoIbge,
            m.Nome,
            m.Regiao,
            m.LojaCodigo,
            m.LojaNome,
            Numeros.Arredondar(Numeros.SomaOuNulo(doMunicipio.Select(p => p.Demanda.AreaUtilHectares))),
            Numeros.Arredondar(Numeros.SomaOuNulo(doMunicipio.Select(p => p.Demanda.Parque))),
            [.. comDemanda
                .GroupBy(p => p.Demanda.CulturaCodigo)
                .Select(g => new DemandaDaCulturaNoMunicipio(g.Key, Numeros.Arredondar(g.Sum(p => p.Demanda.DemandaAnual!.Value))!.Value))],
            Numeros.Arredondar(estrutural),
            Numeros.Arredondar(ajustada),
            Numeros.Arredondar(Numeros.SomaOuNulo(comDemanda.Select(p => p.AEntregar))),
            Numeros.Arredondar(Numeros.SomaOuNulo(comDemanda.Select(p => p.AEntregarAjustada))),
            Ponderado(f => f.ParcelaDePreco),
            Ponderado(f => f.ParcelaDeCredito),
            predominante,
            estrutural is > 0 && ajustada is { } a ? decimal.Round((a / estrutural.Value - 1) * 100m, 1) : null,
            Numeros.Arredondar(antes),
            // A VARIAÇÃO CONTRA O ANO ANTERIOR é de estrutural com estrutural: a área plantada mudou, e só ela.
            estrutural is { } e && antes is > 0 ? decimal.Round((e / antes.Value - 1) * 100m, 1) : null);
    }

    private static List<MetricaSemDado> Lacunas(
        IReadOnlyList<RegraDePotencialAplicada> regras,
        ParametroDoPlanejamento? doPlanejamento,
        ParametroDoPotencial? gerais,
        bool todas,
        string categoria,
        IReadOnlyDictionary<string, ShareAlvoDaCategoria> shareDe,
        List<Parcela> comDemanda,
        bool semArt,
        bool comCultura,
        bool comAnoAnterior)
    {
        var lacunas = new List<MetricaSemDado>();

        if (semArt)
            lacunas.Add(new MetricaSemDado(
                "entregaRealizada",
                "O ART não trouxe venda de máquina ao alcance desta consulta: a entrega realizada e o atendimento ficam vazios. Ausência de carga não é entrega zero."));
        else if (comCultura)
            lacunas.Add(new MetricaSemDado(
                "entregaPorCultura",
                "A máquina entregue não diz para que cultura foi: com uma cultura escolhida, a entrega realizada e o atendimento ficam de fora."));
        else
            lacunas.Add(new MetricaSemDado(
                "entregaRealizada",
                "A entrega realizada é a do ART pela data da ENTREGA (a régua da Gestão de Negócios), dos compradores com endereço principal nos municípios do recorte e das categorias da demanda. A venda sem categoria ou sem endereço fica de fora."));

        lacunas.Add(new MetricaSemDado(
            "anoAnterior",
            comAnoAnterior
                ? "O ano anterior é a mesma conta com a área plantada da PAM de um ano antes, cada cultura no dela, e pelas regras de hoje: a variação é da área, e só dela. A demanda ajustada não se compara — o momento é o de hoje."
                : "A PAM não tem o ano anterior das culturas do recorte: as comparações com o ano anterior ficam vazias."));

        if (doPlanejamento is null)
            lacunas.Add(new MetricaSemDado(
                "sazonalidade",
                "Não há sazonalidade vigente: a previsão mensal divide o ano em doze partes iguais. Registre-a em Configurações › Potencial de mercado."));
        else if (doPlanejamento.InformadoPorId is null)
            lacunas.Add(new MetricaSemDado(
                "sazonalidade",
                "A sazonalidade ainda é a do protótipo da pasta 360, a confirmar: a previsão mensal vale como leitura."));

        if (!todas && !shareDe.ContainsKey(categoria))
            lacunas.Add(new MetricaSemDado(
                "shareAlvo",
                "A categoria não tem share-alvo vigente: sem ele não há o que a Tracbel tem de entregar."));
        else if (todas && comDemanda.Any(p => p.Share is null))
            lacunas.Add(new MetricaSemDado(
                "shareAlvo",
                "Parte das categorias não tem share-alvo vigente: o \"a entregar\" soma só as que têm."));
        else if (!todas && shareDe[categoria].InformadoPorId is null)
            lacunas.Add(new MetricaSemDado(
                "shareAlvo",
                "O share-alvo ainda é o do protótipo da pasta 360, a confirmar."));

        if (gerais?.PesoDoIndicadorDePreco is null)
            lacunas.Add(new MetricaSemDado(
                "fatorDeCiclo",
                "Sem os pesos do fator de ciclo (D-P05), a demanda não é ajustada pelo momento: a ajustada fica vazia."));

        if (regras.Any(r => r.Situacao == nameof(SituacaoDaRegraDePotencial.AConfirmar)))
            lacunas.Add(new MetricaSemDado(
                "regraDePotencial",
                "Parte das regras de potencial ainda está a confirmar: a demanda é estimativa de renovação de frota, e não previsão de venda."));

        lacunas.Add(new MetricaSemDado(
            "porCliente",
            "A demanda é por município, e não por cliente: não há área plantada por propriedade — o protótipo também não tem essa leitura."));

        return lacunas;
    }

    private static decimal? Vezes(decimal? valor, decimal fracao) => valor is { } v ? decimal.Round(v * fracao, 2) : null;
}

/// <summary>A demanda e a previsão da região consultada.</summary>
/// <param name="Categoria">O código da categoria, ou TODAS.</param>
/// <param name="CategoriaNome">O nome dela.</param>
/// <param name="Categorias">As categorias que o filtro oferece — as que têm demanda em algum município.</param>
/// <param name="ShareAlvo">O share-alvo vigente da categoria, em %; nulo em TODAS ou sem vigência.</param>
/// <param name="ShareDoPrototipo">Se o share ainda é a semente do protótipo, a confirmar.</param>
/// <param name="SazonalidadeVigenteDesde">Desde quando a sazonalidade vale.</param>
/// <param name="SazonalidadeDoPrototipo">Se ela ainda é a semente do protótipo.</param>
/// <param name="AnoDaAreaPlantada">O ano da PAM que dimensionou o parque.</param>
/// <param name="Totais">Os números do topo.</param>
/// <param name="PorCultura">A tabela de parâmetros e demanda por cultura.</param>
/// <param name="PrevisaoMensal">Os doze meses do ano fiscal, de novembro a outubro.</param>
/// <param name="PorLoja">O que cada loja tem de entregar, no ano e mês a mês.</param>
/// <param name="Municipios">A matriz município × potencial.</param>
/// <param name="Culturas">As culturas da matriz, da maior demanda para a menor — as colunas.</param>
/// <param name="Lacunas">O que a leitura não afirma, com o motivo.</param>
/// <param name="AnoFiscal">O ano fiscal das entregas (2026 é nov/2025 a out/2026).</param>
/// <param name="AnosFiscais">Os anos fiscais que o filtro "Período" oferece, do mais novo para o mais velho.</param>
/// <param name="Cultura">O código da cultura que recorta a demanda; nulo é todas.</param>
/// <param name="CulturasDoFiltro">As culturas com demanda na categoria e no recorte — o filtro "Cultura".</param>
/// <param name="AnoDaAreaAnterior">O ano da PAM da comparação — um antes do da área plantada.</param>
public sealed record DemandaEPrevisaoDaRegiao(
    string Categoria,
    string CategoriaNome,
    IReadOnlyList<CategoriaParaFiltro> Categorias,
    decimal? ShareAlvo,
    bool ShareDoPrototipo,
    DateOnly? SazonalidadeVigenteDesde,
    bool SazonalidadeDoPrototipo,
    short? AnoDaAreaPlantada,
    TotaisDaDemanda Totais,
    IReadOnlyList<DemandaDaCultura> PorCultura,
    IReadOnlyList<PrevisaoDoMes> PrevisaoMensal,
    IReadOnlyList<EntregaDaLoja> PorLoja,
    IReadOnlyList<DemandaDoMunicipioNaPrevisao> Municipios,
    IReadOnlyList<CulturaDaMatriz> Culturas,
    IReadOnlyList<MetricaSemDado> Lacunas,
    int AnoFiscal = 0,
    IReadOnlyList<int>? AnosFiscais = null,
    string? Cultura = null,
    IReadOnlyList<CulturaDaMatriz>? CulturasDoFiltro = null,
    short? AnoDaAreaAnterior = null);

/// <summary>Os números do topo.</summary>
/// <param name="Parque">As máquinas que a área da região comporta.</param>
/// <param name="DemandaEstrutural">A renovação anual do parque — 100% do mercado.</param>
/// <param name="DemandaAjustada">A mesma pelo momento; nula sem fator.</param>
/// <param name="AEntregar">Demanda × share-alvo — a meta anual da Tracbel.</param>
/// <param name="AEntregarAjustada">A ajustada × share-alvo.</param>
/// <param name="CulturasComRegra">As culturas com regra de potencial na categoria.</param>
/// <param name="Municipios">Municípios da ADR no recorte.</param>
/// <param name="MunicipiosComDemanda">Deles, os com demanda.</param>
/// <param name="Estimativa">Se algum parâmetro usado ainda está a confirmar.</param>
/// <param name="ParqueAnoAnterior">O parque com a área do ano anterior da PAM.</param>
/// <param name="DemandaEstruturalAnoAnterior">A demanda estrutural com a área do ano anterior.</param>
/// <param name="CulturasComAumentoDeArea">As culturas cuja área útil no recorte cresceu sobre o ano anterior.</param>
/// <param name="EntreguesNoPeriodo">As máquinas entregues no ano fiscal até hoje (ou nele inteiro, se já fechou); nulo sem ART ou com cultura.</param>
/// <param name="EntreguesNoPeriodoAnterior">O mesmo trecho do ano fiscal anterior.</param>
/// <param name="EntregasAte">O último dia contado nas entregas.</param>
public sealed record TotaisDaDemanda(
    decimal? Parque,
    decimal? DemandaEstrutural,
    decimal? DemandaAjustada,
    decimal? AEntregar,
    decimal? AEntregarAjustada,
    IReadOnlyList<string> CulturasComRegra,
    int Municipios,
    int MunicipiosComDemanda,
    bool Estimativa,
    decimal? ParqueAnoAnterior = null,
    decimal? DemandaEstruturalAnoAnterior = null,
    IReadOnlyList<string>? CulturasComAumentoDeArea = null,
    int? EntreguesNoPeriodo = null,
    int? EntreguesNoPeriodoAnterior = null,
    DateOnly? EntregasAte = null);

/// <summary>Uma cultura na tabela de parâmetros.</summary>
/// <param name="CulturaCodigo">O código.</param>
/// <param name="Cultura">O nome.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta.</param>
/// <param name="HectaresPorMaquina">Área ÷ parque.</param>
/// <param name="AnosDeRenovacao">Parque ÷ demanda.</param>
/// <param name="Parque">As máquinas que a área comporta.</param>
/// <param name="DemandaEstrutural">A renovação anual.</param>
/// <param name="DemandaAjustada">A mesma pelo momento.</param>
/// <param name="VariacaoPercentual">Quanto a ajustada difere da estrutural.</param>
/// <param name="AreaAnoAnterior">A área útil com a PAM do ano anterior.</param>
/// <param name="ParqueAnoAnterior">O parque com ela.</param>
/// <param name="DemandaAnoAnterior">A demanda estrutural com ela.</param>
public sealed record DemandaDaCultura(
    string CulturaCodigo,
    string Cultura,
    decimal? AreaUtilHectares,
    decimal? HectaresPorMaquina,
    decimal? AnosDeRenovacao,
    decimal? Parque,
    decimal? DemandaEstrutural,
    decimal? DemandaAjustada,
    decimal? VariacaoPercentual,
    decimal? AreaAnoAnterior = null,
    decimal? ParqueAnoAnterior = null,
    decimal? DemandaAnoAnterior = null);

/// <summary>Um mês da previsão.</summary>
/// <param name="Mes">O mês do calendário (1 a 12).</param>
/// <param name="Fracao">A fração do ano pela sazonalidade.</param>
/// <param name="DemandaEstrutural">A demanda do mercado no mês.</param>
/// <param name="DemandaAjustada">A ajustada no mês.</param>
/// <param name="AEntregar">O que a Tracbel tem de entregar no mês.</param>
/// <param name="AEntregarAjustada">O mesmo pela ajustada.</param>
/// <param name="Entregues">As máquinas entregues no mês do ano fiscal escolhido (ART, pela entrega); nulo no mês que não começou, sem ART ou com cultura.</param>
public sealed record PrevisaoDoMes(
    int Mes, decimal Fracao, decimal? DemandaEstrutural, decimal? DemandaAjustada, decimal? AEntregar, decimal? AEntregarAjustada,
    int? Entregues = null);

/// <summary>O que uma loja tem de entregar.</summary>
/// <param name="LojaCodigo">O código da filial.</param>
/// <param name="Loja">O nome.</param>
/// <param name="AEntregarNoAno">A soma do ano — pela ajustada quando existe, senão pela estrutural.</param>
/// <param name="PorMes">Os doze meses do ano fiscal, de novembro a outubro.</param>
public sealed record EntregaDaLoja(string? LojaCodigo, string Loja, decimal? AEntregarNoAno, IReadOnlyList<decimal?> PorMes);

/// <summary>Um município na matriz.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Regiao">A sub-região.</param>
/// <param name="LojaCodigo">A filial responsável.</param>
/// <param name="Loja">O nome dela.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta.</param>
/// <param name="Parque">As máquinas que a área comporta.</param>
/// <param name="PorCultura">A demanda de cada cultura.</param>
/// <param name="DemandaEstrutural">A renovação anual.</param>
/// <param name="DemandaAjustada">A mesma pelo momento.</param>
/// <param name="AEntregar">Demanda × share.</param>
/// <param name="AEntregarAjustada">Ajustada × share.</param>
/// <param name="FatorDePreco">1 + a parcela do preço, ponderada pela demanda das culturas.</param>
/// <param name="FatorDeCredito">1 + a parcela do crédito.</param>
/// <param name="CulturaPredominante">A de maior parque.</param>
/// <param name="VariacaoPercentual">O efeito líquido do momento sobre a demanda.</param>
/// <param name="DemandaEstruturalAnoAnterior">A demanda estrutural com a área do ano anterior da PAM.</param>
/// <param name="VariacaoAnoAnterior">A estrutural de hoje sobre a do ano anterior, em %.</param>
public sealed record DemandaDoMunicipioNaPrevisao(
    int CodigoIbge,
    string Nome,
    string Regiao,
    string? LojaCodigo,
    string? Loja,
    decimal? AreaUtilHectares,
    decimal? Parque,
    IReadOnlyList<DemandaDaCulturaNoMunicipio> PorCultura,
    decimal? DemandaEstrutural,
    decimal? DemandaAjustada,
    decimal? AEntregar,
    decimal? AEntregarAjustada,
    decimal? FatorDePreco,
    decimal? FatorDeCredito,
    string? CulturaPredominante,
    decimal? VariacaoPercentual,
    decimal? DemandaEstruturalAnoAnterior = null,
    decimal? VariacaoAnoAnterior = null);

/// <summary>A demanda de uma cultura num município.</summary>
/// <param name="CulturaCodigo">A cultura.</param>
/// <param name="Demanda">Máquinas por ano.</param>
public sealed record DemandaDaCulturaNoMunicipio(string CulturaCodigo, decimal Demanda);

/// <summary>Uma coluna da matriz.</summary>
/// <param name="Codigo">O código da cultura.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Demanda">A demanda da cultura no recorte.</param>
public sealed record CulturaDaMatriz(string Codigo, string Nome, decimal? Demanda);
