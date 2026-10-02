using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Aplicacao.Territorio;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>
/// O DIMENSIONAMENTO DA ADR — o tamanho do território da Tracbel dentro de São Paulo, o perfil de cada loja, a matriz
/// município a município e a carteira que cobre tudo isso (issue 259, a aba "Dimensionamento ADR" do protótipo da pasta 360).
///
/// <para><b>Nenhum número novo de potencial.</b> Área, quantidade e valor são os da PAM do IBGE (tabela 5457), dos dois
/// lados da fatia: o recorte é a soma dos municípios, e São Paulo é o total que o IBGE publica para o estado. A carteira é a
/// mesma dos Indicadores — vínculo em carteira comercial, município do endereço principal, classe da curva ABC —, contada
/// em faixas de dias desde o último contato.</para>
///
/// <para><b>O que o protótipo tem e esta leitura não traz</b> fica em <see cref="DimensionamentoDaAdr.Lacunas"/> com o
/// motivo: o peso 0–100 da filial (pesos que ninguém decidiu) e a área responsável (atributo da planilha).</para>
/// </summary>
public sealed class ObterDimensionamentoDaAdr(
    IRepositorioDoDimensionamento dimensionamento,
    IRepositorioIndicadoresTerritoriais territorio,
    IRepositorioDeIndicadoresDeMercado mercado,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Quantos municípios o ranking de prioridade mostra.</summary>
    public const int MunicipiosPrioritarios = 20;

    /// <summary>O mínimo de clientes para um município entrar no ranking de prioridade — como o protótipo.</summary>
    public const int MinimoDeClientesNaPrioridade = 3;

    /// <summary>Executa a leitura.</summary>
    /// <param name="anoBase">O ano da PAM; nulo é o mais recente carregado.</param>
    /// <param name="cultura">O código da cultura no catálogo, ou OUTRAS; nulo é todas.</param>
    /// <param name="regiao">Norte ou Noroeste; nulo é a ADR inteira.</param>
    /// <param name="lojaCodigo">A filial responsável; nulo é todas.</param>
    /// <param name="visao"><c>Filial</c> (padrão) ou <c>Empresa</c>.</param>
    /// <param name="responsavel">O CEN dono da carteira.</param>
    /// <param name="classe">A classe do cliente na curva ABC — A, B, C ou D.</param>
    /// <param name="usina"><c>com</c> só os municípios com usina de etanol; <c>sem</c>, os sem.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<DimensionamentoDaAdr>>> ExecutarAsync(
        string? anoBase,
        string? cultura,
        string? regiao,
        string? lojaCodigo,
        string? visao,
        string? responsavel,
        string? classe,
        string? usina,
        CancellationToken ct)
    {
        var (filtros, erros, semPermissao) = await FiltrosDeAlcance.LerAsync(territorio, acesso.Atual, visao, null, null, null, responsavel, ct);

        RegiaoDaAreaDeAtuacao? regiaoEscolhida = null;
        if (!string.IsNullOrWhiteSpace(regiao))
        {
            if (Enum.TryParse<RegiaoDaAreaDeAtuacao>(regiao.Trim(), ignoreCase: true, out var lida) && lida != RegiaoDaAreaDeAtuacao.NaoInformada)
                regiaoEscolhida = lida;
            else
                erros.Registrar("regiao", "A ADR tem duas regiões: Norte e Noroeste.", regiao);
        }

        ClasseDeCliente? classeEscolhida = null;
        if (!string.IsNullOrWhiteSpace(classe))
        {
            if (Enum.TryParse<ClasseDeCliente>(classe.Trim(), ignoreCase: true, out var lida) && Enum.IsDefined(lida))
                classeEscolhida = lida;
            else
                erros.Registrar("classe", "A classe do cliente é A, B, C ou D.", classe);
        }

        bool? comUsina = null;
        if (!string.IsNullOrWhiteSpace(usina))
        {
            comUsina = usina.Trim().ToLowerInvariant() switch { "com" => true, "sem" => false, _ => null };
            if (comUsina is null)
                erros.Registrar("usina", "O filtro de usina é \"com\" (só os municípios com usina de etanol) ou \"sem\".", usina);
        }

        var anos = await dimensionamento.AnosDaPamAsync(ct);
        short? ano = null;
        if (!string.IsNullOrWhiteSpace(anoBase))
        {
            if (short.TryParse(anoBase.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var lido) && anos.Contains(lido))
                ano = lido;
            else
                erros.Registrar("anoBase", "Não há PAM carregada deste ano. Os anos carregados vêm em `anosDisponiveis`.", anoBase);
        }
        else if (anos.Count > 0)
        {
            ano = anos[0];
        }

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<DimensionamentoDaAdr>>("A consulta tem parâmetros que não valem.");
        if (semPermissao is not null)
            return Resultado<ComProcedencia<DimensionamentoDaAdr>>.SemPermissao(semPermissao);

        short? anterior = ano is { } a ? (short)(a - 1) : null;
        var producao = await dimensionamento.LerProducaoAsync(ano is { } b ? [b, anterior!.Value] : [], ct);

        // A CULTURA E A LOJA SÓ SE CONFEREM DEPOIS DA LEITURA: as duas listas vêm do banco (o catálogo e a área de atuação), e
        // um código que não está nelas é recusado em vez de devolver uma tela zerada.
        var culturaEscolhida = string.IsNullOrWhiteSpace(cultura)
            ? null
            : producao.Culturas.FirstOrDefault(c => string.Equals(c.Codigo, cultura.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(cultura) && culturaEscolhida is null)
            erros.Registrar("cultura", "Não há cultura com este código no catálogo. Use uma das de `culturas`, ou deixe vazio para todas.", cultura);

        var daAdr = producao.Municipios.Where(m => m.PertenceAAdr).ToList();
        var lojas = daAdr
            .Where(m => m.LojaCodigo is not null)
            .GroupBy(m => m.LojaCodigo!, StringComparer.Ordinal)
            .Select(g => new LojaDaAdr(g.Key, g.First().LojaNome ?? g.Key, g.Count()))
            .OrderBy(l => l.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();
        var lojaEscolhida = string.IsNullOrWhiteSpace(lojaCodigo)
            ? null
            : lojas.FirstOrDefault(l => string.Equals(l.Codigo, lojaCodigo.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(lojaCodigo) && lojaEscolhida is null)
            erros.Registrar("lojaCodigo", "Não há município da ADR com esta loja responsável. As lojas da ADR vêm em `lojas`.", lojaCodigo);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<DimensionamentoDaAdr>>("A consulta tem parâmetros que não valem.");

        // O RECORTE: os municípios da ADR pela região, pela loja e pela usina. Com recorte de território, a carteira conta só
        // os clientes dele — e "fora da região" deixa de existir.
        var recorte = daAdr
            .Where(m => regiaoEscolhida is null || m.Regiao == regiaoEscolhida.Value.ToString())
            .Where(m => lojaEscolhida is null || m.LojaCodigo == lojaEscolhida.Codigo)
            .Where(m => comUsina is null || (m.Usinas > 0) == comUsina)
            .ToList();
        var filtradoNoTerritorio = regiaoEscolhida is not null || lojaEscolhida is not null || comUsina is not null;

        var agora = relogio.Agora;
        var carteira = await dimensionamento.LerCarteiraAsync(
            new ConsultaDaCarteiraNoDimensionamento(
                filtros!.Visao,
                filtros.ResponsavelId,
                classeEscolhida,
                filtradoNoTerritorio ? recorte.Select(m => m.CodigoIbge).ToHashSet() : null),
            agora,
            ct);
        var carteiraDe = carteira.PorMunicipio.ToDictionary(m => m.CodigoIbge);

        // COM O CEN, O TERRITÓRIO É O DELE: os municípios onde ele tem cliente — como o protótipo, que recorta a produção
        // pelos municípios do vendedor.
        if (filtros.ResponsavelId is not null)
            recorte = [.. recorte.Where(m => carteiraDe.ContainsKey(m.CodigoIbge))];

        var conta = new Conta(producao, culturaEscolhida?.Codigo);
        var codigosDoRecorte = recorte.Select(m => m.CodigoIbge).ToList();
        var codigosDaAdr = daAdr.Select(m => m.CodigoIbge).ToList();

        var atual = ano is { } x ? conta.Somar(codigosDoRecorte, x) : Medidas.Vazias;
        var doAnoAnterior = anterior is { } y ? conta.Somar(codigosDoRecorte, y) : Medidas.Vazias;
        var doEstado = ano is { } z ? conta.SomarNoEstado(z) : Medidas.Vazias;

        var culturaDaProdutividade = culturaEscolhida is not null && culturaEscolhida.Codigo != CulturaDoDimensionamento.Outras;

        var totais = new TotaisDaProducao(
            ComAnterior(atual.Area, doAnoAnterior.Area),
            ComAnterior(atual.Quantidade, doAnoAnterior.Quantidade),
            ComAnterior(atual.Valor, doAnoAnterior.Valor),
            ComAnterior(atual.ReaisPorHectare, doAnoAnterior.ReaisPorHectare),
            culturaDaProdutividade ? atual.Produtividade : null,
            ano is { } w ? recorte.Count(m => conta.Somar([m.CodigoIbge], w).Valor is > 0) : 0);

        var noEstado = new TracbelNoEstado(
            Fatia.De(atual.Area, doEstado.Area),
            Fatia.De(atual.Quantidade, doEstado.Quantidade),
            Fatia.De(atual.Valor, doEstado.Valor));

        RepresentatividadeDaLoja? representatividade = null;
        if (lojaEscolhida is not null && ano is { } anoDaLoja)
        {
            // TODAS AS CULTURAS, como o protótipo: é o peso da loja no território, e não numa cultura só.
            var todas = new Conta(producao, null);
            var daLoja = todas.Somar(codigosDoRecorte, anoDaLoja);
            var daRegiao = todas.Somar(codigosDaAdr, anoDaLoja);
            representatividade = new RepresentatividadeDaLoja(
                lojaEscolhida.Codigo,
                lojaEscolhida.Nome,
                Fatia.De(daLoja.Area, daRegiao.Area),
                Fatia.De(daLoja.Quantidade, daRegiao.Quantidade),
                Fatia.De(daLoja.Valor, daRegiao.Valor));
        }

        MomentoDaCultura? momento = null;
        if (culturaDaProdutividade)
        {
            var precos = await mercado.LerAsync(ParametroComVigencia.HojeNoBrasil(agora), null, ct);
            if (precos.PrecoPorCultura.TryGetValue(culturaEscolhida!.Codigo, out var indice))
                momento = new MomentoDaCultura(indice.Indice, indice.Faixa, indice.Motivo, precos.UltimoMesDePreco);
            else
                momento = new MomentoDaCultura(null, null, nameof(MotivoSemIndicador.SemFonte), precos.UltimoMesDePreco);
        }

        var emFoco = codigosDoRecorte.ToHashSet();
        var mapa = ano is { } anoDoMapa
            ? producao.Municipios
                .Select(m =>
                {
                    var medidas = conta.Somar([m.CodigoIbge], anoDoMapa);
                    return new MunicipioNoMapa(
                        m.CodigoIbge,
                        m.Nome,
                        emFoco.Contains(m.CodigoIbge),
                        Arredondar(medidas.Area),
                        Arredondar(medidas.Quantidade),
                        Arredondar(medidas.Valor),
                        carteiraDe.TryGetValue(m.CodigoIbge, out var c) ? c.Clientes.Clientes : null);
                })
                .ToList()
            : [];

        var porCultura = ano is { } anoDaCultura
            ? producao.Culturas
                .Select(c =>
                {
                    var daCultura = new Conta(producao, c.Codigo);
                    var aqui = daCultura.Somar(codigosDoRecorte, anoDaCultura);
                    var estado = daCultura.SomarNoEstado(anoDaCultura);
                    decimal? daLojaNaTracbel = null;
                    if (lojaEscolhida is not null)
                        daLojaNaTracbel = Fatia.De(aqui.Valor, daCultura.Somar(codigosDaAdr, anoDaCultura).Valor).Percentual;
                    return new CulturaTracbelNoEstado(
                        c.Codigo, c.Nome, Fatia.De(aqui.Area, estado.Area), Fatia.De(aqui.Quantidade, estado.Quantidade),
                        Fatia.De(aqui.Valor, estado.Valor), daLojaNaTracbel);
                })
                .OrderByDescending(c => c.Valor.Parte ?? -1)
                .ToList()
            : [];

        var porLoja = ano is { } anoDoPerfil
            ? recorte
                .GroupBy(m => (m.LojaCodigo, m.LojaNome))
                .Select(g =>
                {
                    var codigos = g.Select(m => m.CodigoIbge).ToList();
                    var daLoja = conta.Somar(codigos, anoDoPerfil);
                    return new PerfilDaLoja(
                        g.Key.LojaCodigo,
                        g.Key.LojaNome ?? "Sem loja responsável",
                        g.Count(),
                        Arredondar(daLoja.Area),
                        Arredondar(daLoja.Valor),
                        Arredondar(daLoja.ReaisPorHectare),
                        culturaDaProdutividade ? Tecnificacao(daLoja.Produtividade, atual.Produtividade) : null,
                        conta.CulturaPrincipal(codigos, anoDoPerfil));
                })
                .OrderByDescending(l => l.Valor ?? -1)
                .ToList()
            : [];

        var prioritarios = ano is { } anoDaPrioridade
            ? recorte
                .Where(m => carteiraDe.TryGetValue(m.CodigoIbge, out var c) && c.Clientes.Clientes >= MinimoDeClientesNaPrioridade)
                .Select(m =>
                {
                    var clientes = carteiraDe[m.CodigoIbge].Clientes;
                    var valor = conta.Somar([m.CodigoIbge], anoDaPrioridade).Valor;
                    var lacuna = (decimal)clientes.Faixas.Sem120 / clientes.Clientes;
                    return new MunicipioPrioritario(
                        m.CodigoIbge,
                        m.Nome,
                        Arredondar(valor),
                        clientes.Clientes,
                        decimal.Round(100m * clientes.Faixas.Ate90 / clientes.Clientes, 1),
                        decimal.Round(100m * lacuna, 1),
                        valor is { } v ? decimal.Round(v * lacuna, 2) : null);
                })
                .Where(m => m.Prioridade is > 0)
                .OrderByDescending(m => m.Prioridade)
                .Take(MunicipiosPrioritarios)
                .ToList()
            : [];

        var matriz = ano is { } anoDaMatriz
            ? recorte
                .Select(m =>
                {
                    var deste = conta.Somar([m.CodigoIbge], anoDaMatriz);
                    var antes = conta.Somar([m.CodigoIbge], anterior!.Value);
                    var naCarteira = carteiraDe.GetValueOrDefault(m.CodigoIbge);
                    return new LinhaDaMatrizMunicipal(
                        m.CodigoIbge,
                        m.Nome,
                        m.Regiao,
                        m.LojaCodigo,
                        m.LojaNome,
                        naCarteira?.ResponsavelPrincipal,
                        conta.CulturaPrincipal([m.CodigoIbge], anoDaMatriz),
                        ComAnterior(deste.Area, antes.Area),
                        ComAnterior(deste.Quantidade, antes.Quantidade),
                        ComAnterior(deste.Valor, antes.Valor),
                        Fatia.De(deste.Valor, atual.Valor).Percentual,
                        Fatia.De(deste.Valor, doEstado.Valor).Percentual,
                        naCarteira?.Clientes ?? ClientesDaCarteira.Nenhum,
                        m.Usinas);
                })
                .OrderByDescending(l => l.Valor.Atual ?? -1)
                .ThenBy(l => l.Nome, StringComparer.Create(PtBr, ignoreCase: true))
                .ToList()
            : [];

        var total = carteira.Total;
        var classificados = total.A + total.B + total.C + total.D;
        var daCarteira = new CarteiraDoRecorte(
            total,
            carteira.NaRegiao,
            filtradoNoTerritorio ? null : carteira.Fora,
            classificados > 0 ? decimal.Round((4m * total.A + 3m * total.B + 2m * total.C + total.D) / classificados, 2) : null);

        var resultado = new DimensionamentoDaAdr(
            ano,
            anterior,
            anos,
            culturaEscolhida?.Codigo,
            culturaEscolhida?.Nome ?? "Todas as culturas",
            producao.Culturas,
            lojas,
            filtros.Responsaveis,
            lojaEscolhida is not null ? $"Loja {lojaEscolhida.Nome}" : regiaoEscolhida is not null ? $"Região {regiaoEscolhida}" : $"ADR ({daAdr.Count} municípios)",
            recorte.Count,
            noEstado,
            representatividade,
            totais,
            momento,
            mapa,
            porCultura,
            porLoja,
            daCarteira,
            prioritarios,
            matriz,
            carteira.PorResponsavel,
            Lacunas(producao, ano, anterior, culturaEscolhida, filtros.ResponsavelId, filtradoNoTerritorio));

        return Resultado<ComProcedencia<DimensionamentoDaAdr>>.Ok(
            ComProcedencia<DimensionamentoDaAdr>.DoNossoBanco(
                resultado,
                "PAM do IBGE (tabela 5457, municípios e estado) · área de atuação · carteiras comerciais e último contato",
                relogio));
    }

    /// <summary>A TECNIFICAÇÃO: a produtividade de um recorte contra a média do recorte inteiro, em %.</summary>
    private static decimal? Tecnificacao(decimal? produtividade, decimal? media) =>
        produtividade is { } p && media is > 0 ? decimal.Round(100m * p / media.Value, 1) : null;

    private static MedidaComAnterior ComAnterior(decimal? atual, decimal? anterior) =>
        new(Arredondar(atual), Arredondar(anterior), atual is { } a && anterior is > 0 ? decimal.Round((a / anterior.Value - 1) * 100m, 1) : null);

    private static decimal? Arredondar(decimal? valor) => valor is { } v ? decimal.Round(v, 2) : null;

    private static List<MetricaSemDado> Lacunas(
        ProducaoParaODimensionamento producao,
        short? ano,
        short? anterior,
        CulturaDoDimensionamento? cultura,
        long? responsavelId,
        bool filtradoNoTerritorio)
    {
        var lacunas = new List<MetricaSemDado>();

        if (ano is null)
        {
            lacunas.Add(new MetricaSemDado("pam", "A PAM do IBGE ainda não foi carregada: não há área, quantidade nem valor da produção para dimensionar."));
            return lacunas;
        }

        if (!producao.NoEstado.Any(p => p.Ano == ano))
            lacunas.Add(new MetricaSemDado(
                "totalDoEstado",
                $"O total de São Paulo de {ano} não foi carregado: a fatia da Tracbel no estado fica vazia, em vez de virar a soma dos municípios com outro nome."));

        if (!producao.NosMunicipios.Any(p => p.Ano == anterior))
            lacunas.Add(new MetricaSemDado(
                "anoAnterior",
                $"Não há PAM de {anterior} carregada: as variações contra o ano anterior ficam vazias."));

        lacunas.Add(new MetricaSemDado(
            "quantidade",
            "A quantidade soma só o que o IBGE publica em toneladas: abacaxi e coco-da-baía vêm em mil frutos e entram na área e no valor das outras culturas, fora da quantidade."));

        if (cultura is null || cultura.Codigo == CulturaDoDimensionamento.Outras)
            lacunas.Add(new MetricaSemDado(
                "produtividade",
                "Produtividade, tecnificação e rentabilidade são de uma cultura: escolha uma para vê-las."));

        lacunas.Add(new MetricaSemDado(
            "pesoDaFilial",
            "O peso 0–100 da filial do protótipo combina as três fatias com pesos fixos (40% valor, 30% área, 30% produção) que ninguém decidiu no CRM: as três fatias estão na tela, e o peso entra quando os pesos forem um parâmetro com vigência."));

        lacunas.Add(new MetricaSemDado(
            "areaResponsavel",
            "A área responsável do protótipo (Varejo, Digital, Grandes Contas) vem da planilha do comercial: a carteira do CRM não tem esse atributo."));

        lacunas.Add(new MetricaSemDado(
            "carteiraPorVendedor",
            "Um cliente em carteiras de dois responsáveis aparece nas duas linhas da carteira por vendedor: a soma da tabela pode passar do total de clientes, que conta cada um uma vez."));

        if (filtradoNoTerritorio)
            lacunas.Add(new MetricaSemDado(
                "foraDaRegiao",
                "Com região, loja ou usina escolhida, a carteira conta só os clientes dos municípios do recorte: \"fora da região\" só existe sem esses filtros."));

        if (responsavelId is not null)
            lacunas.Add(new MetricaSemDado(
                "territorioDoCen",
                "Com o CEN escolhido, o território é o dele: só os municípios onde ele tem cliente entram na produção, no mapa e na matriz."));

        return lacunas;
    }

    /// <summary>
    /// AS SOMAS DA PAM de um conjunto de municípios (ou do estado) num ano, numa cultura ou em todas.
    /// </summary>
    private sealed class Conta
    {
        private readonly ILookup<(int?, short), ProducaoDaCultura> porLugarEAno;
        private readonly string? cultura;
        private readonly IReadOnlyList<CulturaDoDimensionamento> culturas;

        public Conta(ProducaoParaODimensionamento producao, string? cultura)
        {
            this.cultura = cultura;
            culturas = producao.Culturas;
            porLugarEAno = producao.NosMunicipios.Concat(producao.NoEstado).ToLookup(p => (p.CodigoIbge, p.Ano));
        }

        public Medidas Somar(IEnumerable<int> codigos, short ano) =>
            Medidas.De(codigos.SelectMany(c => porLugarEAno[(c, ano)]).Where(p => cultura is null || p.Cultura == cultura));

        public Medidas SomarNoEstado(short ano) =>
            Medidas.De(porLugarEAno[(null, ano)].Where(p => cultura is null || p.Cultura == cultura));

        /// <summary>
        /// A CULTURA DE MAIOR VALOR no ano — entre as do catálogo: "outras" é um balaio de produtos, e não uma cultura que
        /// alguém vá chamar de principal.
        /// </summary>
        public string? CulturaPrincipal(IEnumerable<int> codigos, short ano)
        {
            var linhas = codigos.SelectMany(c => porLugarEAno[(c, ano)]).Where(p => p.Cultura != CulturaDoDimensionamento.Outras).ToList();
            var maior = linhas
                .GroupBy(p => p.Cultura)
                .Select(g => (Cultura: g.Key, Valor: g.Sum(p => p.ValorMilReais ?? 0)))
                .Where(x => x.Valor > 0)
                .OrderByDescending(x => x.Valor)
                .FirstOrDefault();
            return maior.Cultura is null ? null : culturas.FirstOrDefault(c => c.Codigo == maior.Cultura)?.Nome ?? maior.Cultura;
        }
    }

    private sealed record Medidas(decimal? Area, decimal? AreaColhida, decimal? Quantidade, decimal? Valor)
    {
        public static readonly Medidas Vazias = new(null, null, null, null);

        public static Medidas De(IEnumerable<ProducaoDaCultura> linhas)
        {
            var lista = linhas.ToList();
            return new Medidas(
                SomaOuNulo(lista.Select(p => p.AreaPlantadaHectares)),
                SomaOuNulo(lista.Select(p => p.AreaColhidaHectares)),
                SomaOuNulo(lista.Select(p => p.QuantidadeToneladas)),
                SomaOuNulo(lista.Select(p => p.ValorMilReais)));
        }

        /// <summary>A densidade econômica: o valor (em reais) por hectare plantado.</summary>
        public decimal? ReaisPorHectare => Valor is { } v && Area is > 0 ? v * 1000m / Area.Value : null;

        /// <summary>A produtividade, como o IBGE: quantidade sobre área COLHIDA.</summary>
        public decimal? Produtividade => UnidadesDaPam.Produtividade(Quantidade, AreaColhida);

        private static decimal? SomaOuNulo(IEnumerable<decimal?> valores)
        {
            var com = valores.OfType<decimal>().ToList();
            return com.Count > 0 ? com.Sum() : null;
        }
    }
}

/// <summary>O Dimensionamento da ADR consultado.</summary>
/// <param name="AnoBase">O ano da PAM; nulo quando a PAM não foi carregada.</param>
/// <param name="AnoAnterior">O ano comparado — o anterior ao ano-base.</param>
/// <param name="AnosDisponiveis">Os anos da PAM carregados, do mais novo para o mais antigo.</param>
/// <param name="Cultura">O código da cultura escolhida; nulo é todas.</param>
/// <param name="CulturaNome">O nome dela.</param>
/// <param name="Culturas">As culturas que o filtro oferece — as do catálogo e as outras.</param>
/// <param name="Lojas">As lojas responsáveis por municípios da ADR — as opções do filtro.</param>
/// <param name="Responsaveis">Os responsáveis das carteiras comerciais ao alcance — as opções do filtro "CEN".</param>
/// <param name="Recorte">O recorte em palavras: a ADR, uma região ou uma loja.</param>
/// <param name="MunicipiosNoRecorte">Quantos municípios entraram.</param>
/// <param name="TracbelNoEstado">A fatia do recorte em São Paulo — área, quantidade e valor.</param>
/// <param name="Representatividade">Com uma loja, o peso dela na ADR inteira.</param>
/// <param name="Totais">Os números do topo, com o ano anterior.</param>
/// <param name="Momento">Com uma cultura, o momento do preço dela (12 meses contra os 12 anteriores).</param>
/// <param name="Mapa">Os 645 municípios de São Paulo, com os do recorte em foco.</param>
/// <param name="PorCultura">A participação de cada cultura, recorte contra estado.</param>
/// <param name="PorLoja">O perfil comercial de cada loja do recorte.</param>
/// <param name="Carteira">A carteira de clientes do recorte.</param>
/// <param name="Prioritarios">Os municípios de alto potencial e baixa cobertura.</param>
/// <param name="Matriz">A matriz municipal — os municípios do recorte.</param>
/// <param name="PorVendedor">A carteira por responsável.</param>
/// <param name="Lacunas">O que a leitura não afirma, com o motivo.</param>
public sealed record DimensionamentoDaAdr(
    short? AnoBase,
    short? AnoAnterior,
    IReadOnlyList<short> AnosDisponiveis,
    string? Cultura,
    string CulturaNome,
    IReadOnlyList<CulturaDoDimensionamento> Culturas,
    IReadOnlyList<LojaDaAdr> Lojas,
    IReadOnlyList<ResponsavelDeCarteira> Responsaveis,
    string Recorte,
    int MunicipiosNoRecorte,
    TracbelNoEstado TracbelNoEstado,
    RepresentatividadeDaLoja? Representatividade,
    TotaisDaProducao Totais,
    MomentoDaCultura? Momento,
    IReadOnlyList<MunicipioNoMapa> Mapa,
    IReadOnlyList<CulturaTracbelNoEstado> PorCultura,
    IReadOnlyList<PerfilDaLoja> PorLoja,
    CarteiraDoRecorte Carteira,
    IReadOnlyList<MunicipioPrioritario> Prioritarios,
    IReadOnlyList<LinhaDaMatrizMunicipal> Matriz,
    IReadOnlyList<CarteiraDoResponsavel> PorVendedor,
    IReadOnlyList<MetricaSemDado> Lacunas);

/// <summary>Uma loja responsável por municípios da ADR.</summary>
/// <param name="Codigo">O código da filial.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Municipios">Quantos municípios da ADR são dela.</param>
public sealed record LojaDaAdr(string Codigo, string Nome, int Municipios);

/// <summary>
/// UMA FATIA — a parte, o todo e quanto a parte é do todo, em %. Nula quando falta um dos dois ou o todo é zero: dividir por
/// zero não é "0%".
/// </summary>
/// <param name="Parte">O recorte.</param>
/// <param name="Todo">O total contra o qual ele é medido.</param>
/// <param name="Percentual">Parte ÷ todo, em pontos percentuais com duas casas.</param>
public sealed record Fatia(decimal? Parte, decimal? Todo, decimal? Percentual)
{
    /// <summary>Monta a fatia.</summary>
    /// <param name="parte">A parte.</param>
    /// <param name="todo">O todo.</param>
    public static Fatia De(decimal? parte, decimal? todo) =>
        new(
            parte is { } p ? decimal.Round(p, 2) : null,
            todo is { } t ? decimal.Round(t, 2) : null,
            parte is { } a && todo is > 0 ? decimal.Round(100m * a / todo.Value, 2) : null);
}

/// <summary>A fatia do recorte em São Paulo.</summary>
/// <param name="Area">Área plantada, em hectares.</param>
/// <param name="Quantidade">Quantidade produzida, em toneladas.</param>
/// <param name="Valor">Valor da produção, em mil reais.</param>
public sealed record TracbelNoEstado(Fatia Area, Fatia Quantidade, Fatia Valor);

/// <summary>O peso de uma loja na ADR inteira, em todas as culturas.</summary>
/// <param name="LojaCodigo">A filial.</param>
/// <param name="Loja">O nome.</param>
/// <param name="Area">A área da loja contra a da ADR.</param>
/// <param name="Quantidade">A quantidade.</param>
/// <param name="Valor">O valor.</param>
public sealed record RepresentatividadeDaLoja(string LojaCodigo, string Loja, Fatia Area, Fatia Quantidade, Fatia Valor);

/// <summary>Uma medida no ano-base e no anterior.</summary>
/// <param name="Atual">No ano-base.</param>
/// <param name="Anterior">No ano anterior.</param>
/// <param name="VariacaoPercentual">Atual ÷ anterior − 1, em %; nula sem os dois.</param>
public sealed record MedidaComAnterior(decimal? Atual, decimal? Anterior, decimal? VariacaoPercentual);

/// <summary>Os números do topo.</summary>
/// <param name="Area">Área plantada, em hectares.</param>
/// <param name="Quantidade">Quantidade produzida, em toneladas.</param>
/// <param name="Valor">Valor da produção, em mil reais.</param>
/// <param name="Densidade">Valor por hectare plantado, em reais.</param>
/// <param name="Produtividade">Com uma cultura, toneladas por hectare colhido.</param>
/// <param name="MunicipiosComProducao">Os municípios do recorte com valor de produção no ano-base.</param>
public sealed record TotaisDaProducao(
    MedidaComAnterior Area,
    MedidaComAnterior Quantidade,
    MedidaComAnterior Valor,
    MedidaComAnterior Densidade,
    decimal? Produtividade,
    int MunicipiosComProducao);

/// <summary>O momento do preço de uma cultura.</summary>
/// <param name="Indice">Os 12 meses contra os 12 anteriores — 1,10 é 10% acima; nulo com o motivo.</param>
/// <param name="Faixa">A faixa de mercado do índice.</param>
/// <param name="Motivo">Por que o índice não saiu; <c>Nenhum</c> quando saiu.</param>
/// <param name="UltimoMes">O mês mais recente da série de preço.</param>
public sealed record MomentoDaCultura(decimal? Indice, string? Faixa, string Motivo, DateOnly? UltimoMes);

/// <summary>Um município no mapa de São Paulo.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="EmFoco">Se está no recorte.</param>
/// <param name="Area">Área plantada no ano-base.</param>
/// <param name="Quantidade">Quantidade produzida.</param>
/// <param name="Valor">Valor da produção, em mil reais.</param>
/// <param name="Clientes">Clientes em carteira com endereço aqui; nulo sem nenhum.</param>
public sealed record MunicipioNoMapa(
    int CodigoIbge, string Nome, bool EmFoco, decimal? Area, decimal? Quantidade, decimal? Valor, int? Clientes);

/// <summary>Uma cultura, recorte contra estado.</summary>
/// <param name="Codigo">O código da cultura.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Area">Área do recorte contra a do estado.</param>
/// <param name="Quantidade">Quantidade.</param>
/// <param name="Valor">Valor.</param>
/// <param name="FatiaDaLojaNaTracbel">Com uma loja, quanto do valor da cultura na ADR é da loja, em %.</param>
public sealed record CulturaTracbelNoEstado(
    string Codigo, string Nome, Fatia Area, Fatia Quantidade, Fatia Valor, decimal? FatiaDaLojaNaTracbel);

/// <summary>O perfil comercial de uma loja.</summary>
/// <param name="LojaCodigo">A filial; nula nos municípios sem loja.</param>
/// <param name="Loja">O nome.</param>
/// <param name="Municipios">Os municípios da loja no recorte.</param>
/// <param name="Area">Área plantada.</param>
/// <param name="Valor">Valor da produção, em mil reais.</param>
/// <param name="ReaisPorHectare">A densidade econômica.</param>
/// <param name="Tecnificacao">Com uma cultura, a produtividade da loja em % da média do recorte.</param>
/// <param name="CulturaDominante">A cultura de maior valor.</param>
public sealed record PerfilDaLoja(
    string? LojaCodigo,
    string Loja,
    int Municipios,
    decimal? Area,
    decimal? Valor,
    decimal? ReaisPorHectare,
    decimal? Tecnificacao,
    string? CulturaDominante);

/// <summary>A carteira de clientes do recorte.</summary>
/// <param name="Clientes">Os clientes distintos, por classe e por faixa de dias.</param>
/// <param name="NaRegiao">Os com endereço num município da ADR.</param>
/// <param name="Fora">Os de fora; nulo com recorte de território, em que só os de dentro são contados.</param>
/// <param name="PotencialMedio">A média da classe, com A = 4 e D = 1 (sem classe fora da conta).</param>
public sealed record CarteiraDoRecorte(ClientesDaCarteira Clientes, int NaRegiao, int? Fora, decimal? PotencialMedio);

/// <summary>Um município de alto potencial e baixa cobertura.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Valor">O valor da produção, em mil reais.</param>
/// <param name="Clientes">Os clientes em carteira.</param>
/// <param name="CoberturaAte90Percentual">Os contatados nos últimos 90 dias, em % dos clientes.</param>
/// <param name="LacunaPercentual">Os sem contato nos últimos 120 dias, em % dos clientes.</param>
/// <param name="Prioridade">Valor × lacuna — o valor de produção que está sem cobertura, em mil reais.</param>
public sealed record MunicipioPrioritario(
    int CodigoIbge, string Nome, decimal? Valor, int Clientes, decimal CoberturaAte90Percentual, decimal LacunaPercentual, decimal? Prioridade);

/// <summary>Uma linha da matriz municipal.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Regiao">A sub-região.</param>
/// <param name="LojaCodigo">A filial responsável.</param>
/// <param name="Loja">O nome dela.</param>
/// <param name="Vendedor">O responsável com mais vínculos no município.</param>
/// <param name="CulturaPrincipal">A cultura de maior valor no ano-base.</param>
/// <param name="Area">Área plantada, com o ano anterior.</param>
/// <param name="Quantidade">Quantidade produzida.</param>
/// <param name="Valor">Valor da produção, em mil reais.</param>
/// <param name="FatiaNaRegiao">Quanto do valor do recorte é deste município, em %.</param>
/// <param name="FatiaNoEstado">Quanto do valor de São Paulo, em %.</param>
/// <param name="Clientes">Os clientes em carteira, por classe e por faixa de dias.</param>
/// <param name="Usinas">As usinas de etanol vigentes.</param>
public sealed record LinhaDaMatrizMunicipal(
    int CodigoIbge,
    string Nome,
    string? Regiao,
    string? LojaCodigo,
    string? Loja,
    string? Vendedor,
    string? CulturaPrincipal,
    MedidaComAnterior Area,
    MedidaComAnterior Quantidade,
    MedidaComAnterior Valor,
    decimal? FatiaNaRegiao,
    decimal? FatiaNoEstado,
    ClientesDaCarteira Clientes,
    int Usinas);
