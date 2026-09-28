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
/// </summary>
public sealed class ObterDemandaEPrevisao(
    IRepositorioIndicadoresTerritoriais territorio,
    IRepositorioDeIndicadoresDeMercado mercado,
    IRepositorioDoPlanejamento planejamento,
    IRepositorioDeParametrosDoPotencial parametros,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
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
    public async Task<Resultado<ComProcedencia<DemandaEPrevisaoDaRegiao>>> ExecutarAsync(
        string? regiao, string? lojaCodigo, string? visao, string? categoria, CancellationToken ct)
    {
        var (filtros, erros, semPermissao) = await FiltrosDeAlcance.LerAsync(territorio, acesso.Atual, visao, null, null, null, null, ct);

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
        var mesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(agora);
        var ultimoFechado = mesCorrente.AddMonths(-1);

        // O PERÍODO NÃO MUDA NADA AQUI: a demanda é de um ano de parque, e não de vendas. A apuração pede um período, e
        // os 12 meses fechados são o mais neutro.
        var indicadores = await territorio.ApurarAsync(
            new ConsultaDeIndicadoresTerritoriais(
                ultimoFechado.AddMonths(-11), ultimoFechado, regiaoEscolhida,
                string.IsNullOrWhiteSpace(lojaCodigo) ? null : lojaCodigo.Trim(),
                filtros!.Visao, null, null, null, null, mesCorrente),
            agora,
            ct);

        var hoje = ParametroComVigencia.HojeNoBrasil(agora);
        var daAdr = indicadores.Municipios.Where(m => m.PertenceAAdr).ToList();
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
        var parcelas = daAdr
            .SelectMany(m => (m.DemandaPorCategoriaECultura ?? [])
                .Where(p => todas || string.Equals(p.CategoriaCodigo, codigoDaCategoria, StringComparison.Ordinal))
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

        var totais = Totais(parcelas, comDemanda, daAdr.Count, indicadores.Regras, codigoDaCategoria, todas);
        var porCultura = PorCultura(parcelas);
        var previsao = MesesDoAnoFiscal
            .Select(mes => new PrevisaoDoMes(
                mes,
                decimal.Round(fracoes[mes], 4),
                Vezes(totais.DemandaEstrutural, fracoes[mes]),
                Vezes(totais.DemandaAjustada, fracoes[mes]),
                Vezes(totais.AEntregar, fracoes[mes]),
                Vezes(totais.AEntregarAjustada, fracoes[mes])))
            .ToList();

        var porLoja = comDemanda
            .GroupBy(p => (p.Municipio.LojaCodigo, p.Municipio.LojaNome))
            .Select(g =>
            {
                var anual = SomaOuNulo(g.Select(p => p.AEntregarAjustada ?? p.AEntregar));
                return new EntregaDaLoja(
                    g.Key.LojaCodigo,
                    g.Key.LojaNome ?? "Sem loja responsável",
                    Arredondar(anual),
                    [.. MesesDoAnoFiscal.Select(mes => Vezes(anual, fracoes[mes]))]);
            })
            .OrderByDescending(l => l.AEntregarNoAno ?? -1)
            .ToList();

        var municipios = daAdr
            .Select(m => Municipio(m, parcelas.Where(p => p.Municipio.CodigoIbge == m.CodigoIbge).ToList()))
            .OrderByDescending(m => m.DemandaAjustada ?? m.DemandaEstrutural ?? -1)
            .ThenBy(m => m.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        var culturas = parcelas
            .GroupBy(p => (p.Demanda.CulturaCodigo, p.Demanda.Cultura))
            .Select(g => new CulturaDaMatriz(g.Key.CulturaCodigo, g.Key.Cultura, Arredondar(SomaOuNulo(g.Select(p => p.Demanda.DemandaAnual)))))
            .OrderByDescending(c => c.Demanda ?? -1)
            .ToList();

        var ordem = filtros.Categorias.ToDictionary(c => c.Codigo, c => c.Ordem, StringComparer.Ordinal);
        var categoriasComDemanda = daAdr
            .SelectMany(m => m.DemandaPorCategoriaECultura ?? [])
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
            indicadores.AnoDaAreaPlantada,
            totais,
            porCultura,
            previsao,
            porLoja,
            municipios,
            culturas,
            Lacunas(indicadores, doPlanejamento, gerais, todas, codigoDaCategoria, shareDe, comDemanda));

        return Resultado<ComProcedencia<DemandaEPrevisaoDaRegiao>>.Ok(
            ComProcedencia<DemandaEPrevisaoDaRegiao>.DoNossoBanco(
                resultado,
                "motor do potencial (área plantada da PAM × regras do potencial) · fator de ciclo · share-alvo e sazonalidade do planejamento",
                relogio));
    }

    private sealed record Parcela(IndicadoresDoMunicipio Municipio, DemandaNoMunicipio Demanda, PotencialAjustado Ajuste, decimal? Share)
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
            Arredondar(SomaOuNulo(parcelas.Select(p => p.Demanda.Parque))),
            Arredondar(SomaOuNulo(comDemanda.Select(p => p.Demanda.DemandaAnual))),
            ajustadas.Count > 0 && ajustadas.All(a => a is not null) ? Arredondar(ajustadas.Sum()) : null,
            Arredondar(SomaOuNulo(comDemanda.Select(p => p.AEntregar))),
            Arredondar(SomaOuNulo(comDemanda.Select(p => p.AEntregarAjustada))),
            culturasComRegra,
            municipios,
            comDemanda.Select(p => p.Municipio.CodigoIbge).Distinct().Count(),
            parcelas.Any(p => p.Ajuste.Fator.Estimativa));
    }

    private static List<DemandaDaCultura> PorCultura(List<Parcela> parcelas) =>
    [
        .. parcelas
            .GroupBy(p => (p.Demanda.CulturaCodigo, p.Demanda.Cultura))
            .Select(g =>
            {
                var area = SomaOuNulo(g.Select(p => p.Demanda.AreaUtilHectares));
                var parque = SomaOuNulo(g.Select(p => p.Demanda.Parque));
                var demanda = SomaOuNulo(g.Select(p => p.Demanda.DemandaAnual));
                var ajustadas = g.Where(p => p.Demanda.DemandaAnual is not null).Select(p => p.Ajuste.DemandaAjustada).ToList();
                decimal? ajustada = ajustadas.Count > 0 && ajustadas.All(a => a is not null) ? ajustadas.Sum() : null;
                return new DemandaDaCultura(
                    g.Key.CulturaCodigo,
                    g.Key.Cultura,
                    Arredondar(area),
                    // OS PARÂMETROS SAEM DA PRÓPRIA CONTA: ha/máquina = área ÷ parque e ciclo = parque ÷ demanda. Numa
                    // cultura com uma regra só, são exatamente os da regra; com duas categorias, é a média delas.
                    area is > 0 && parque is > 0 ? decimal.Round(area.Value / parque.Value, 1) : null,
                    parque is > 0 && demanda is > 0 ? decimal.Round(parque.Value / demanda.Value, 1) : null,
                    Arredondar(parque),
                    Arredondar(demanda),
                    Arredondar(ajustada),
                    demanda is > 0 && ajustada is { } a ? decimal.Round((a / demanda.Value - 1) * 100m, 1) : null);
            })
            .OrderByDescending(c => c.DemandaEstrutural ?? -1)
    ];

    private static DemandaDoMunicipioNaPrevisao Municipio(IndicadoresDoMunicipio m, List<Parcela> doMunicipio)
    {
        var comDemanda = doMunicipio.Where(p => p.Demanda.DemandaAnual is not null).ToList();
        var estrutural = SomaOuNulo(comDemanda.Select(p => p.Demanda.DemandaAnual));
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
            Arredondar(SomaOuNulo(doMunicipio.Select(p => p.Demanda.AreaUtilHectares))),
            Arredondar(SomaOuNulo(doMunicipio.Select(p => p.Demanda.Parque))),
            [.. comDemanda
                .GroupBy(p => p.Demanda.CulturaCodigo)
                .Select(g => new DemandaDaCulturaNoMunicipio(g.Key, Arredondar(g.Sum(p => p.Demanda.DemandaAnual!.Value))!.Value))],
            Arredondar(estrutural),
            Arredondar(ajustada),
            Arredondar(SomaOuNulo(comDemanda.Select(p => p.AEntregar))),
            Arredondar(SomaOuNulo(comDemanda.Select(p => p.AEntregarAjustada))),
            Ponderado(f => f.ParcelaDePreco),
            Ponderado(f => f.ParcelaDeCredito),
            predominante,
            estrutural is > 0 && ajustada is { } a ? decimal.Round((a / estrutural.Value - 1) * 100m, 1) : null);
    }

    private static List<MetricaSemDado> Lacunas(
        IndicadoresTerritoriais indicadores,
        ParametroDoPlanejamento? doPlanejamento,
        ParametroDoPotencial? gerais,
        bool todas,
        string categoria,
        IReadOnlyDictionary<string, ShareAlvoDaCategoria> shareDe,
        List<Parcela> comDemanda)
    {
        var lacunas = new List<MetricaSemDado>();

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

        if (indicadores.Regras.Any(r => r.Situacao == nameof(SituacaoDaRegraDePotencial.AConfirmar)))
            lacunas.Add(new MetricaSemDado(
                "regraDePotencial",
                "Parte das regras de potencial ainda está a confirmar: a demanda é estimativa de renovação de frota, e não previsão de venda."));

        lacunas.Add(new MetricaSemDado(
            "porCliente",
            "A demanda é por município, e não por cliente: não há área plantada por propriedade — o protótipo também não tem essa leitura."));

        return lacunas;
    }

    private static decimal? SomaOuNulo(IEnumerable<decimal?> valores)
    {
        var com = valores.OfType<decimal>().ToList();
        return com.Count > 0 ? com.Sum() : null;
    }

    private static decimal? Vezes(decimal? valor, decimal fracao) => valor is { } v ? decimal.Round(v * fracao, 2) : null;

    private static decimal? Arredondar(decimal? valor) => valor is { } v ? decimal.Round(v, 2) : null;
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
    IReadOnlyList<MetricaSemDado> Lacunas);

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
public sealed record TotaisDaDemanda(
    decimal? Parque,
    decimal? DemandaEstrutural,
    decimal? DemandaAjustada,
    decimal? AEntregar,
    decimal? AEntregarAjustada,
    IReadOnlyList<string> CulturasComRegra,
    int Municipios,
    int MunicipiosComDemanda,
    bool Estimativa);

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
public sealed record DemandaDaCultura(
    string CulturaCodigo,
    string Cultura,
    decimal? AreaUtilHectares,
    decimal? HectaresPorMaquina,
    decimal? AnosDeRenovacao,
    decimal? Parque,
    decimal? DemandaEstrutural,
    decimal? DemandaAjustada,
    decimal? VariacaoPercentual);

/// <summary>Um mês da previsão.</summary>
/// <param name="Mes">O mês do calendário (1 a 12).</param>
/// <param name="Fracao">A fração do ano pela sazonalidade.</param>
/// <param name="DemandaEstrutural">A demanda do mercado no mês.</param>
/// <param name="DemandaAjustada">A ajustada no mês.</param>
/// <param name="AEntregar">O que a Tracbel tem de entregar no mês.</param>
/// <param name="AEntregarAjustada">O mesmo pela ajustada.</param>
public sealed record PrevisaoDoMes(
    int Mes, decimal Fracao, decimal? DemandaEstrutural, decimal? DemandaAjustada, decimal? AEntregar, decimal? AEntregarAjustada);

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
    decimal? VariacaoPercentual);

/// <summary>A demanda de uma cultura num município.</summary>
/// <param name="CulturaCodigo">A cultura.</param>
/// <param name="Demanda">Máquinas por ano.</param>
public sealed record DemandaDaCulturaNoMunicipio(string CulturaCodigo, decimal Demanda);

/// <summary>Uma coluna da matriz.</summary>
/// <param name="Codigo">O código da cultura.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Demanda">A demanda da cultura no recorte.</param>
public sealed record CulturaDaMatriz(string Codigo, string Nome, decimal? Demanda);
