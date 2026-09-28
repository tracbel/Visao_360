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
/// O DIAGNÓSTICO COMERCIAL — o IOC de cada município da ADR, com a situação e o plano de ação (issue 257, a aba 5 do
/// protótipo da pasta 360).
///
/// <para><b>Nenhuma leitura nova de negócio.</b> A demanda por categoria e cultura, as vendas do ART, a carteira e a
/// cobertura saem da MESMA apuração dos Indicadores Geográficos, com os mesmos filtros e o mesmo alcance; o preço, o
/// crédito e a percepção, da leitura dos indicadores de mercado — agora por município, porque o IOC compara municípios
/// e o crédito de cada um é o dele. A conta é do domínio (<see cref="DiagnosticoComercial"/>).</para>
/// </summary>
public sealed class ObterDiagnosticoComercial(
    IRepositorioIndicadoresTerritoriais territorio,
    IRepositorioDeIndicadoresDeMercado mercado,
    IRepositorioDoPlanejamento planejamento,
    IRepositorioDeParametrosDoPotencial parametros,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>A categoria de todas — a soma das que têm demanda no município.</summary>
    public const string TodasAsCategorias = "TODAS";

    /// <summary>A categoria padrão, a do protótipo: tratores.</summary>
    public const string CategoriaPadrao = "TRATOR";

    /// <summary>Executa o diagnóstico.</summary>
    /// <param name="competenciaInicial">O primeiro mês das vendas, <c>aaaa-mm</c>. Nulo: 12 meses antes do último.</param>
    /// <param name="competenciaFinal">O último mês, <c>aaaa-mm</c>. Nulo: o último mês fechado.</param>
    /// <param name="regiao">Norte ou Noroeste; nulo é a ADR inteira.</param>
    /// <param name="lojaCodigo">A filial responsável; nulo é todas.</param>
    /// <param name="visao"><c>Filial</c> (padrão) ou <c>Empresa</c>.</param>
    /// <param name="categoria">O código da categoria de máquina, ou <c>TODAS</c>. Nulo é trator.</param>
    /// <param name="responsavel">O CEN/gestor da carteira; nulo é todos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<DiagnosticoComercialDaRegiao>>> ExecutarAsync(
        string? competenciaInicial,
        string? competenciaFinal,
        string? regiao,
        string? lojaCodigo,
        string? visao,
        string? categoria,
        string? responsavel,
        CancellationToken ct)
    {
        var (filtros, erros, semPermissao) = await FiltrosDeAlcance.LerAsync(
            territorio, acesso.Atual, visao, null, null, null, responsavel, ct);

        var agora = relogio.Agora;
        var mesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(agora);

        // OS ÚLTIMOS 12 MESES FECHADOS, como no protótipo: o IOC compara com a demanda de UM ANO, e um ano fechado não
        // depende de sazonalidade. Outro período é aceito — e é levado a um ano pela sazonalidade, não por regra de três.
        var ultimoFechado = mesCorrente.AddMonths(-1);
        var final = LerCompetencia(competenciaFinal, "competenciaFinal", ultimoFechado, erros);
        var inicial = LerCompetencia(competenciaInicial, "competenciaInicial", final.AddMonths(-11), erros);
        if (final > mesCorrente)
            erros.Registrar("competenciaFinal", "O período não pode terminar depois do mês corrente.", competenciaFinal);
        if (inicial > final)
            erros.Registrar("competenciaInicial", "O primeiro mês vem antes do último.", competenciaInicial);
        else if (new JanelaDeCompetencia(inicial, final).Meses > 36)
            erros.Registrar("competenciaInicial", "O período vai até 36 meses.", competenciaInicial);

        RegiaoDaAreaDeAtuacao? regiaoEscolhida = null;
        if (!string.IsNullOrWhiteSpace(regiao))
        {
            if (Enum.TryParse<RegiaoDaAreaDeAtuacao>(regiao.Trim(), ignoreCase: true, out var lida) && lida != RegiaoDaAreaDeAtuacao.NaoInformada)
                regiaoEscolhida = lida;
            else
                erros.Registrar("regiao", "A ADR tem duas regiões: Norte e Noroeste.", regiao);
        }

        // A CATEGORIA É CONFERIDA NO CATÁLOGO, e não na lista do filtro "Tipo de produto": aquela lista só tem as
        // categorias com linha do ART ligada, e o IOC vale para qualquer categoria com regra de potencial.
        var codigoDaCategoria = string.IsNullOrWhiteSpace(categoria) ? CategoriaPadrao : categoria.Trim().ToUpperInvariant();
        var todas = codigoDaCategoria == TodasAsCategorias;
        var daCategoria = todas ? null : await catalogo.ObterCategoriaDeMaquinaAsync(codigoDaCategoria, somenteAtiva: false, ct);
        if (!todas && daCategoria is null)
            erros.Registrar(
                "categoria",
                $"Não há categoria de máquina com este código no catálogo. Use o código de uma (TRATOR, COLHEITADEIRA…) ou {TodasAsCategorias}.",
                categoria);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<DiagnosticoComercialDaRegiao>>("A consulta tem parâmetros que não valem.");
        if (semPermissao is not null)
            return Resultado<ComProcedencia<DiagnosticoComercialDaRegiao>>.SemPermissao(semPermissao);

        // O TIPO DE PRODUTO NÃO VAI PARA A APURAÇÃO: ela devolve a demanda e as vendas de todas as categorias, e a
        // escolha é feita aqui, categoria a categoria — senão o "todas" perderia a quebra por categoria da meta.
        var indicadores = await territorio.ApurarAsync(
            new ConsultaDeIndicadoresTerritoriais(
                inicial, final, regiaoEscolhida, string.IsNullOrWhiteSpace(lojaCodigo) ? null : lojaCodigo.Trim(),
                filtros!.Visao, null, null, null, filtros.ResponsavelId, mesCorrente),
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
        var sharePorCategoria = shares
            .Where(s => categoriasDosShares.ContainsKey(s.CategoriaDeMaquinaId))
            .ToDictionary(s => categoriasDosShares[s.CategoriaDeMaquinaId].Codigo, s => s.Percentual, StringComparer.Ordinal);

        var janela = new JanelaDeCompetencia(inicial, final);
        var fracaoDoAno = FracaoDoAno(janela, doPlanejamento);

        var entradas = daAdr.Select(m => Entrada(m, codigoDaCategoria, todas, deMercado, gerais, sharePorCategoria, fracaoDoAno)).ToList();
        var iocs = doPlanejamento is null
            ? []
            : DiagnosticoComercial.Calcular([.. entradas.Select(e => e.ParaOIoc)], doPlanejamento.Pesos).ToDictionary(r => r.CodigoIbge);

        var linhas = entradas
            .Select(e => Linha(e, iocs.GetValueOrDefault(e.Municipio.CodigoIbge), deMercado))
            .OrderByDescending(l => l.Ioc ?? -1)
            .ThenBy(l => l.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        var nomeDaCategoria = todas ? "Todas as categorias" : daCategoria!.Nome;

        // AS CATEGORIAS QUE O SELETOR OFERECE são as que têm demanda em algum município da ADR — sem demanda, o IOC da
        // categoria não teria potencial para medir. A ordem é a do catálogo, quando o filtro a conhece.
        var ordem = filtros.Categorias.ToDictionary(c => c.Codigo, c => c.Ordem, StringComparer.Ordinal);
        var comDemanda = daAdr
            .SelectMany(m => m.DemandaPorCategoriaECultura ?? [])
            .Where(p => p.DemandaAnual is not null)
            .Select(p => (p.CategoriaCodigo, p.CategoriaNome))
            .Distinct()
            .Select(c => new CategoriaParaFiltro(c.CategoriaCodigo, c.CategoriaNome, ordem.GetValueOrDefault(c.CategoriaCodigo, short.MaxValue)))
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        var diagnostico = new DiagnosticoComercialDaRegiao(
            inicial,
            final,
            decimal.Round(fracaoDoAno, 4),
            codigoDaCategoria,
            nomeDaCategoria,
            comDemanda,
            doPlanejamento is null ? null : PesosDoIocDetalhe.De(doPlanejamento.Pesos),
            doPlanejamento?.VigenteDesde,
            doPlanejamento is { InformadoPorId: null },
            [.. shares.Where(s => categoriasDosShares.ContainsKey(s.CategoriaDeMaquinaId))
                .Select(s => new ShareDoDiagnostico(categoriasDosShares[s.CategoriaDeMaquinaId].Codigo, categoriasDosShares[s.CategoriaDeMaquinaId].Nome, s.Percentual, s.InformadoPorId is null))],
            DiagnosticoComercial.Percentil90([.. entradas.Select(e => e.ParaOIoc.DemandaDoIoc).OfType<decimal>().Where(d => d > 0)]),
            Resumir(linhas, indicadores.MaquinasVendidas is not null),
            linhas,
            Lacunas(indicadores, doPlanejamento, gerais, todas, codigoDaCategoria, nomeDaCategoria, sharePorCategoria, janela));

        return Resultado<ComProcedencia<DiagnosticoComercialDaRegiao>>.Ok(
            ComProcedencia<DiagnosticoComercialDaRegiao>.DoNossoBanco(
                diagnostico,
                "organizacao.MunicipioDaAreaDeAtuacao · organizacao.RegraDePotencial · frota.VendaDeMaquina · comercial.ClienteCarteira · " +
                "organizacao.CreditoRuralDeInvestimento · organizacao.CotacaoDeProduto · organizacao.ParametroDoPlanejamento · organizacao.ShareAlvoDaCategoria",
                relogio));
    }

    /// <summary>
    /// QUANTO DO ANO O PERÍODO REPRESENTA — a soma das frações da sazonalidade dos meses dele (issue 256). Doze meses
    /// seguidos dão 1, qualquer que seja o começo; sem sazonalidade vigente, meses ÷ 12.
    /// </summary>
    private static decimal FracaoDoAno(JanelaDeCompetencia janela, ParametroDoPlanejamento? planejamento)
    {
        if (planejamento is null) return janela.Meses / 12m;

        var soma = 0m;
        for (var mes = janela.Inicial; mes <= janela.Final; mes = mes.AddMonths(1))
            soma += planejamento.FracaoDoMes(mes.Month);
        return soma;
    }

    private sealed record EntradaDoMunicipio(
        IndicadoresDoMunicipio Municipio,
        MunicipioParaOIoc ParaOIoc,
        int? VendidasNoPeriodo,
        string? CulturaPrincipalCodigo,
        bool Estimativa);

    private static EntradaDoMunicipio Entrada(
        IndicadoresDoMunicipio m,
        string categoria,
        bool todas,
        IndicadoresPorMunicipio deMercado,
        ParametroDoPotencial? gerais,
        IReadOnlyDictionary<string, decimal> sharePorCategoria,
        decimal fracaoDoAno)
    {
        var parcelas = (m.DemandaPorCategoriaECultura ?? [])
            .Where(p => todas || string.Equals(p.CategoriaCodigo, categoria, StringComparison.Ordinal))
            .ToList();
        var comDemanda = parcelas.Where(p => p.DemandaAnual is not null).ToList();

        var credito = deMercado.CreditoPorMunicipio.GetValueOrDefault(m.CodigoIbge)?.Indice;

        // A DEMANDA AJUSTADA COM O CRÉDITO DESTE MUNICÍPIO, o preço de cada cultura e a PERCEPÇÃO DE CADA CULTURA somada
        // ao ajuste do gestor sobre o município (27/09/2026) — o fator de ciclo da issue 74, parcela a parcela, igual ao
        // dos Indicadores Geográficos. Sem parâmetros gerais vigentes não há fator, e a ajustada fica vazia.
        decimal? Ajustada(DemandaNoMunicipio parcela) =>
            FatorDeCiclo.Ajustar(
                parcela.DemandaAnual,
                deMercado.PrecoPorCultura.GetValueOrDefault(parcela.CulturaCodigo)?.Indice,
                credito,
                deMercado.PercepcaoDaCulturaNoMunicipio(m.CodigoIbge, parcela.CulturaCodigo),
                gerais).DemandaAjustada;

        var ajustadas = comDemanda.Select(x => (x.CategoriaCodigo, Ajustada: Ajustada(x))).ToList();
        decimal? estrutural = comDemanda.Count > 0 ? comDemanda.Sum(x => x.DemandaAnual!.Value) : null;
        decimal? ajustada = ajustadas.Count > 0 && ajustadas.All(a => a.Ajustada is not null) ? ajustadas.Sum(a => a.Ajustada!.Value) : null;

        // A META DE PLANEJAMENTO = demanda de cada categoria × share-alvo dela, só nas que têm share (issue 256).
        var comShare = ajustadas.Zip(comDemanda)
            .Where(x => sharePorCategoria.ContainsKey(x.First.CategoriaCodigo))
            .ToList();
        decimal? meta = comShare.Count > 0
            ? comShare.Sum(x => (x.First.Ajustada ?? x.Second.DemandaAnual!.Value) * sharePorCategoria[x.First.CategoriaCodigo] / 100m)
            : null;

        // AS VENDAS DAS MESMAS CATEGORIAS DA DEMANDA — a base da captura (27/09/2026). Sem ART, nulo: não é venda zero.
        var categoriasDaDemanda = comDemanda.Select(x => x.CategoriaCodigo).ToHashSet(StringComparer.Ordinal);
        if (!todas) categoriasDaDemanda.Add(categoria);
        int? vendidas = m.MaquinasVendidas is null
            ? null
            : (m.MaquinasPorCategoria ?? []).Where(v => categoriasDaDemanda.Contains(v.CategoriaCodigo)).Sum(v => v.Unidades);
        decimal? vendidasNoAno = vendidas is { } unidades && fracaoDoAno > 0 ? unidades / fracaoDoAno : null;

        // A CULTURA PRINCIPAL é a de maior área útil nas categorias do diagnóstico — como no protótipo.
        var principal = parcelas
            .GroupBy(x => (x.CulturaCodigo, x.Cultura))
            .Select(g => (g.Key, Area: g.Sum(x => x.AreaUtilHectares ?? 0)))
            .Where(g => g.Area > 0)
            .OrderByDescending(g => g.Area)
            .Select(g => ((string, string)?)g.Key)
            .FirstOrDefault();

        return new EntradaDoMunicipio(
            m,
            new MunicipioParaOIoc(
                m.CodigoIbge,
                estrutural,
                ajustada,
                meta,
                vendidasNoAno,
                m.Cobertura.Clientes,
                m.Cobertura.VinculosComCadencia,
                m.Cobertura.Cobertos,
                credito,
                principal is { } c ? deMercado.PrecoPorCultura.GetValueOrDefault(c.Item1)?.Indice : null,
                principal?.Item2),
            vendidas,
            principal?.Item1,
            m.PotencialEstrutural?.Estimativa ?? false);
    }

    private static MunicipioNoDiagnostico Linha(EntradaDoMunicipio e, IocDoMunicipio? ioc, IndicadoresPorMunicipio deMercado)
    {
        var m = e.Municipio;
        var o = e.ParaOIoc;
        var credito = deMercado.CreditoPorMunicipio.GetValueOrDefault(m.CodigoIbge);

        return new MunicipioNoDiagnostico(
            m.CodigoIbge,
            m.Nome,
            m.Regiao,
            m.LojaCodigo,
            m.LojaNome,
            o.CulturaPrincipal,
            o.IndiceDePrecoDaPrincipal,
            o.IndiceDeCredito,
            credito?.BasePequena ?? false,
            Arredondar(o.DemandaEstrutural),
            Arredondar(o.DemandaAjustada),
            Arredondar(o.MetaDePlanejamento),
            e.VendidasNoPeriodo,
            Arredondar(o.VendidasNoAno),
            o.Clientes,
            m.Cobertura.PorClasse,
            m.Cobertura.ClientesEmCarteira,
            m.Vendas.ClientesQueCompraram,
            o.VinculosComCadencia,
            o.Cobertos,
            ioc?.Cobertura is { } cob ? decimal.Round(cob, 4) : null,
            ioc?.Penetracao is { } pen ? decimal.Round(pen, 4) : null,
            ioc?.Componentes,
            ioc?.Ioc,
            ioc?.Classe?.ToString(),
            ioc is null ? "" : string.Join(", ", ioc.Situacao),
            ioc is null ? "" : string.Join(" · ", ioc.PlanoDeAcao),
            ioc?.ComponentesAusentes ?? [],
            e.Estimativa);
    }

    private static decimal? Arredondar(decimal? valor) => valor is { } v ? decimal.Round(v, 2) : null;

    private static ResumoDoDiagnostico Resumir(IReadOnlyList<MunicipioNoDiagnostico> linhas, bool comArt)
    {
        int Da(ClasseDePrioridade classe) => linhas.Count(l => l.Classe == classe.ToString());
        var comIndice = linhas.Where(l => l.Ioc is not null).Select(l => l.Ioc!.Value).ToList();

        // AS SOMAS SÓ DOS MUNICÍPIOS QUE TÊM O NÚMERO, e nulas quando nenhum tem: somar "sem dado" como zero daria um
        // total menor que o real sem aviso. Quantos entraram vai junto, para a tela dizer a base.
        static decimal? Soma(IEnumerable<decimal?> valores)
        {
            var com = valores.OfType<decimal>().ToList();
            return com.Count > 0 ? decimal.Round(com.Sum(), 1) : null;
        }

        var estrutural = Soma(linhas.Select(l => l.DemandaEstrutural));
        var vendidasNoAno = comArt ? Soma(linhas.Select(l => l.VendidasNoAno)) : null;

        return new ResumoDoDiagnostico(
            Da(ClasseDePrioridade.Maxima),
            Da(ClasseDePrioridade.Alta),
            Da(ClasseDePrioridade.Moderada),
            Da(ClasseDePrioridade.Baixa),
            Da(ClasseDePrioridade.Manutencao),
            linhas.Count - comIndice.Count,
            linhas.Count,
            comIndice.Count > 0 ? decimal.Round(comIndice.Average(), 1) : null,
            estrutural,
            Soma(linhas.Select(l => l.DemandaAjustada)),
            linhas.Count(l => l.DemandaEstrutural is not null),
            Soma(linhas.Select(l => l.MetaDePlanejamento)),
            comArt ? linhas.Sum(l => l.VendidasNoPeriodo ?? 0) : null,
            vendidasNoAno,
            estrutural is > 0 && vendidasNoAno is { } ano ? decimal.Round(ano / estrutural.Value, 4) : null,
            linhas.Sum(l => l.Clientes),
            linhas.Sum(l => l.ClientesQueCompraram));
    }

    /// <summary>O que o diagnóstico não consegue afirmar, cada frase com o motivo.</summary>
    private static List<MetricaSemDado> Lacunas(
        IndicadoresTerritoriais indicadores,
        ParametroDoPlanejamento? doPlanejamento,
        ParametroDoPotencial? gerais,
        bool todas,
        string categoria,
        string nomeDaCategoria,
        IReadOnlyDictionary<string, decimal> sharePorCategoria,
        JanelaDeCompetencia janela)
    {
        var lacunas = new List<MetricaSemDado>();

        if (doPlanejamento is null)
            lacunas.Add(new MetricaSemDado(
                "pesosDoIoc",
                "Não há pesos do IOC vigentes: sem eles o índice não sai. Registre-os em Configurações › Potencial de mercado."));
        else if (doPlanejamento.InformadoPorId is null)
            lacunas.Add(new MetricaSemDado(
                "pesosDoIoc",
                "Os pesos do IOC e a sazonalidade ainda são os do protótipo da pasta 360, a confirmar. A ordem dos municípios " +
                "vale como leitura; os pesos da Tracbel entram por Configurações › Potencial de mercado."));

        if (indicadores.MaquinasVendidas is null)
            lacunas.Add(new MetricaSemDado(
                "vendasEmUnidades",
                "O ART não trouxe venda de máquina ao alcance desta consulta: a realização e a penetração ficam fora do " +
                "índice em todos os municípios, e o IOC sai dos outros componentes. Ausência de carga não é venda zero."));

        if (gerais?.PesoDoIndicadorDePreco is null)
            lacunas.Add(new MetricaSemDado(
                "fatorDeCiclo",
                "Sem os pesos do fator de ciclo (D-P05), a demanda não é ajustada pelo momento: o potencial do IOC usa a demanda estrutural."));

        if (!todas && !sharePorCategoria.ContainsKey(categoria))
            lacunas.Add(new MetricaSemDado(
                "shareAlvo",
                $"{nomeDaCategoria} não tem share-alvo vigente: sem meta de planejamento, a realização fica fora do índice."));

        if (janela.Meses != 12)
            lacunas.Add(new MetricaSemDado(
                "periodo",
                $"O período tem {janela.Meses} meses. As vendas foram levadas a um ano pela sazonalidade vigente, e não por " +
                "regra de três: meses de safra pesam mais que os outros."));

        lacunas.Add(new MetricaSemDado(
            "cobertura",
            "A cobertura é a da cadência de cada classe de cliente no CRM (vínculos com contato dentro do prazo), e não o " +
            "corte fixo de 90 dias do protótipo."));

        if (indicadores.Regras.Any(r => r.Situacao == nameof(SituacaoDaRegraDePotencial.AConfirmar)))
            lacunas.Add(new MetricaSemDado(
                "regraDePotencial",
                "Parte das regras de potencial ainda está a confirmar: a demanda de cada município é estimativa de necessidade " +
                "de frota, e não previsão de venda."));

        return lacunas;
    }

    private static DateOnly LerCompetencia(string? texto, string campo, DateOnly padrao, ColetorDeErros erros)
    {
        if (string.IsNullOrWhiteSpace(texto)) return padrao;

        if (DateOnly.TryParseExact(texto.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes))
            return mes;

        erros.Registrar(campo, "Use o formato aaaa-mm, por exemplo 2026-08.", texto);
        return padrao;
    }
}

/// <summary>O diagnóstico comercial da região consultada.</summary>
/// <param name="CompetenciaInicial">O primeiro mês das vendas.</param>
/// <param name="CompetenciaFinal">O último mês.</param>
/// <param name="FracaoDoAnoNoPeriodo">Quanto do ano o período representa pela sazonalidade — 1 para doze meses.</param>
/// <param name="Categoria">O código da categoria do diagnóstico, ou TODAS.</param>
/// <param name="CategoriaNome">O nome dela.</param>
/// <param name="Categorias">As categorias que o filtro oferece.</param>
/// <param name="Pesos">Os pesos do IOC usados; nulos sem vigência.</param>
/// <param name="PesosVigentesDesde">Desde quando valem.</param>
/// <param name="PesosDoPrototipo">Se ainda são os da semente do protótipo, a confirmar.</param>
/// <param name="Shares">O share-alvo vigente de cada categoria.</param>
/// <param name="Percentil90">A régua do componente de potencial.</param>
/// <param name="Resumo">Quantos municípios em cada classe.</param>
/// <param name="Municipios">Os municípios da ADR, do maior IOC para o menor.</param>
/// <param name="Lacunas">O que o diagnóstico não consegue afirmar, com o motivo.</param>
public sealed record DiagnosticoComercialDaRegiao(
    DateOnly CompetenciaInicial,
    DateOnly CompetenciaFinal,
    decimal FracaoDoAnoNoPeriodo,
    string Categoria,
    string CategoriaNome,
    IReadOnlyList<CategoriaParaFiltro> Categorias,
    PesosDoIocDetalhe? Pesos,
    DateOnly? PesosVigentesDesde,
    bool PesosDoPrototipo,
    IReadOnlyList<ShareDoDiagnostico> Shares,
    decimal? Percentil90,
    ResumoDoDiagnostico Resumo,
    IReadOnlyList<MunicipioNoDiagnostico> Municipios,
    IReadOnlyList<MetricaSemDado> Lacunas);

/// <summary>O share-alvo de uma categoria, como o diagnóstico o usou.</summary>
/// <param name="CategoriaCodigo">A categoria.</param>
/// <param name="CategoriaNome">O nome.</param>
/// <param name="Percentual">O share-alvo.</param>
/// <param name="DoPrototipo">Se é a semente do protótipo, a confirmar.</param>
public sealed record ShareDoDiagnostico(string CategoriaCodigo, string CategoriaNome, decimal Percentual, bool DoPrototipo);

/// <summary>Quantos municípios em cada classe de prioridade.</summary>
/// <param name="Maxima">IOC ≥ 80.</param>
/// <param name="Alta">60 a 80.</param>
/// <param name="Moderada">40 a 60.</param>
/// <param name="Baixa">20 a 40.</param>
/// <param name="Manutencao">Abaixo de 20.</param>
/// <param name="SemIndice">Sem componente nenhum com dado.</param>
/// <param name="Total">Municípios no diagnóstico.</param>
/// <param name="IocMedio">A média dos que têm índice.</param>
/// <param name="DemandaEstrutural">A soma da demanda estrutural dos municípios que a têm; nula sem nenhum.</param>
/// <param name="DemandaAjustada">A soma da ajustada dos que a têm.</param>
/// <param name="MunicipiosComDemanda">Quantos municípios entraram na soma da demanda.</param>
/// <param name="MetaDePlanejamento">A soma da meta (demanda × share-alvo) dos que a têm.</param>
/// <param name="VendidasNoPeriodo">Máquinas vendidas no período, pelo ART; nula sem carga do ART.</param>
/// <param name="VendidasNoAno">As mesmas levadas a um ano pela sazonalidade.</param>
/// <param name="Penetracao">Vendidas no ano ÷ demanda estrutural da região, de 0 a 1; nula sem ART ou sem demanda.</param>
/// <param name="Clientes">Clientes com endereço principal nos municípios.</param>
/// <param name="ClientesQueCompraram">Deles, os com faturamento no período.</param>
public sealed record ResumoDoDiagnostico(
    int Maxima,
    int Alta,
    int Moderada,
    int Baixa,
    int Manutencao,
    int SemIndice,
    int Total,
    decimal? IocMedio,
    decimal? DemandaEstrutural = null,
    decimal? DemandaAjustada = null,
    int MunicipiosComDemanda = 0,
    decimal? MetaDePlanejamento = null,
    int? VendidasNoPeriodo = null,
    decimal? VendidasNoAno = null,
    decimal? Penetracao = null,
    int Clientes = 0,
    int ClientesQueCompraram = 0);

/// <summary>Um município no diagnóstico — a linha da tabela.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Regiao">A sub-região da ADR.</param>
/// <param name="LojaCodigo">O código da filial responsável — o que o filtro de loja recebe.</param>
/// <param name="Loja">A filial responsável.</param>
/// <param name="CulturaPrincipal">A cultura de maior área útil.</param>
/// <param name="IndiceDePreco">O momento de preço dela (1,00 estável).</param>
/// <param name="IndiceDeCredito">O índice de crédito do município (1,00 estável).</param>
/// <param name="CreditoBasePequena">Se o crédito tem base pequena demais para ser tendência.</param>
/// <param name="DemandaEstrutural">Máquinas por ano que a área pede.</param>
/// <param name="DemandaAjustada">A demanda ajustada pelo momento do município.</param>
/// <param name="MetaDePlanejamento">Demanda × share-alvo.</param>
/// <param name="VendidasNoPeriodo">Máquinas vendidas no período (ART); nulo sem carga.</param>
/// <param name="VendidasNoAno">As mesmas, levadas a um ano pela sazonalidade.</param>
/// <param name="Clientes">Clientes com endereço no município.</param>
/// <param name="ClientesPorClasse">Os mesmos pela classe ABC; nulo onde a apuração não separa.</param>
/// <param name="ClientesEmCarteira">Deles, os com vínculo em carteira comercial.</param>
/// <param name="ClientesQueCompraram">Deles, os com faturamento no período.</param>
/// <param name="VinculosComCadencia">Vínculos com cadência declarada.</param>
/// <param name="Cobertos">Deles, os cobertos.</param>
/// <param name="Cobertura">Cobertos ÷ vínculos com cadência, de 0 a 1.</param>
/// <param name="Penetracao">Vendidas no ano ÷ demanda estrutural.</param>
/// <param name="Componentes">Os sete componentes do IOC; nulos sem pesos.</param>
/// <param name="Ioc">O índice, de 0 a 100.</param>
/// <param name="Classe">Maxima, Alta, Moderada, Baixa ou Manutencao.</param>
/// <param name="Situacao">A leitura em frases.</param>
/// <param name="PlanoDeAcao">Até três ações.</param>
/// <param name="ComponentesAusentes">O que ficou fora da conta, e por quê.</param>
/// <param name="Estimativa">Se alguma regra de potencial usada aqui está a confirmar.</param>
public sealed record MunicipioNoDiagnostico(
    int CodigoIbge,
    string Nome,
    string Regiao,
    string? LojaCodigo,
    string? Loja,
    string? CulturaPrincipal,
    decimal? IndiceDePreco,
    decimal? IndiceDeCredito,
    bool CreditoBasePequena,
    decimal? DemandaEstrutural,
    decimal? DemandaAjustada,
    decimal? MetaDePlanejamento,
    int? VendidasNoPeriodo,
    decimal? VendidasNoAno,
    int Clientes,
    ClientesPorClasse? ClientesPorClasse,
    int? ClientesEmCarteira,
    int ClientesQueCompraram,
    int VinculosComCadencia,
    int Cobertos,
    decimal? Cobertura,
    decimal? Penetracao,
    ComponentesDoIoc? Componentes,
    decimal? Ioc,
    string? Classe,
    string Situacao,
    string PlanoDeAcao,
    IReadOnlyList<string> ComponentesAusentes,
    bool Estimativa);
