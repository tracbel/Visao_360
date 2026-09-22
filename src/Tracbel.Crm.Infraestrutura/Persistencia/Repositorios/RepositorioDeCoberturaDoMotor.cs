using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A COBERTURA DO DADO QUE O MOTOR DE MERCADO VAI USAR, medida no banco a cada leitura (issue 150).
///
/// <para><b>Só contagem.</b> Nenhuma consulta daqui devolve valor em reais, nome de cliente ou de CEN: conta
/// linhas, municípios, meses e campos preenchidos. O dado interno (vendas, equipamentos, vendas perdidas,
/// endereços) entra pelo filtro global de filial, como toda consulta do CRM — não há <c>Where</c> de empresa
/// aqui, e ninguém conta o que não alcança.</para>
///
/// <para><b>A soma e o agrupamento são em memória</b> onde a tabela é pequena (a PAM de um ano na ADR, as
/// cotações, os custos, os meses do SICOR): o SQLite dos testes não soma decimal, e são milhares de linhas, não
/// milhões. As contagens do dado interno ficam no banco.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeCoberturaDoMotor(CrmDbContext contexto) : IRepositorioDeCoberturaDoMotor
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private const string MunicipiosDaAdr = "municípios da ADR";

    /// <inheritdoc />
    public async Task<CoberturaDoMotor> MedirAsync(CancellationToken ct)
    {
        var adr = await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
            .Where(a => a.PertenceAAdr && a.EncerradoEm == null)
            .Select(a => a.MunicipioId)
            .Distinct()
            .ToListAsync(ct);

        return new CoberturaDoMotor(
            adr.Count,
            [
                await ProducaoAgricolaAsync(adr, ct),
                await PrecosAsync(ct),
                await CustosAsync(ct),
                await CreditoAsync(adr, ct),
                await VendasDeMaquinaAsync(ct),
                await EquipamentosAsync(ct),
                await VendasPerdidasAsync(ct),
                await EnderecosAsync(ct)
            ]);
    }

    // =============================================================================================
    // Dado público
    // =============================================================================================

    /// <summary>
    /// A PAM do último ano, cultura a cultura, nos municípios da ADR.
    ///
    /// <para><b>"Com o dado" é área plantada divulgada</b> — zero incluído, porque zero é medida; fica de fora o
    /// nulo, que é sigilo ou "não disponível" no IBGE. Só entram as culturas plantadas em algum município da
    /// ADR, da maior área para a menor: é a ordem em que elas pesam no potencial.</para>
    /// </summary>
    private async Task<GrupoDeCobertura> ProducaoAgricolaAsync(List<int> adr, CancellationToken ct)
    {
        const string ParaQue = "o potencial estrutural desta cultura";
        var ano = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking().MaxAsync(p => (short?)p.Ano, ct);

        if (ano is null)
            return new("PAM", "Produção agrícola por cultura", "IBGE — PAM (SIDRA 5457)", false,
                [new("PAM", "Produção agrícola", MunicipiosDaAdr, 0, 0, "o potencial estrutural")]);

        var linhas = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
            .Where(p => p.Ano == ano && adr.Contains(p.MunicipioId))
            .Select(p => new
            {
                p.ProdutoCodigoIbge,
                p.ProdutoNome,
                p.MunicipioId,
                p.AreaPlantadaHectares,
                p.AreaColhidaHectares,
                p.QuantidadeProduzida,
                p.ValorDaProducaoMilReais
            })
            .ToListAsync(ct);

        var anos = (await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                .Select(p => new { p.ProdutoCodigoIbge, p.Ano })
                .Distinct()
                .ToListAsync(ct))
            .GroupBy(p => p.ProdutoCodigoIbge)
            .ToDictionary(g => g.Key, g => (Inicio: g.Min(p => p.Ano), Fim: g.Max(p => p.Ano)));

        static int Municipios<T>(IEnumerable<T> linhas, Func<T, bool> tem, Func<T, int> municipio) =>
            linhas.Where(tem).Select(municipio).Distinct().Count();

        var itens = linhas
            .GroupBy(l => l.ProdutoCodigoIbge)
            .Select(g => new
            {
                Codigo = g.Key,
                Nome = g.First().ProdutoNome,
                Area = g.Sum(l => l.AreaPlantadaHectares ?? 0),
                Linhas = g.ToList()
            })
            .Where(p => p.Area > 0)
            .OrderByDescending(p => p.Area)
            .ThenBy(p => p.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .Select(p =>
            {
                var divulgada = Municipios(p.Linhas, l => l.AreaPlantadaHectares is not null, l => l.MunicipioId);
                var plantam = Municipios(p.Linhas, l => l.AreaPlantadaHectares > 0, l => l.MunicipioId);
                var colhida = Municipios(p.Linhas, l => l.AreaColhidaHectares is not null, l => l.MunicipioId);
                var quantidade = Municipios(p.Linhas, l => l.QuantidadeProduzida is not null, l => l.MunicipioId);
                var unidade = UnidadesDaPam.DaQuantidade(p.Codigo, ano.Value).Nome;
                var valor = Municipios(p.Linhas, l => l.ValorDaProducaoMilReais is not null, l => l.MunicipioId);
                var (inicio, fim) = anos.GetValueOrDefault(p.Codigo, (ano.Value, ano.Value));

                return new ItemDeCobertura(
                    $"PAM.{p.Codigo}",
                    p.Nome,
                    MunicipiosDaAdr,
                    adr.Count,
                    divulgada,
                    ParaQue,
                    Ano(inicio),
                    Ano(fim),
                    $"planta-se em {plantam} · área colhida divulgada em {colhida}, quantidade em {quantidade} ({unidade}) e valor em {valor} · " +
                    $"{p.Area.ToString("N0", PtBr)} ha plantados na ADR em {ano}");
            })
            .ToList();

        return new("PAM", "Produção agrícola por cultura", $"IBGE — PAM (SIDRA 5457), {ano}", false, itens);
    }

    /// <summary>
    /// O PREÇO DE CADA SÉRIE, MÊS A MÊS: quantos meses faltam entre o primeiro e o último.
    ///
    /// <para>A CONAB publica uma janela de 12 meses e às vezes pula um mês de um produto (o café de SP, em
    /// 2026). O buraco não se preenche — mas precisa aparecer, porque o momento 12 ÷ 12 só existe com 24 meses
    /// completos.</para>
    /// </summary>
    private async Task<GrupoDeCobertura> PrecosAsync(CancellationToken ct)
    {
        const string ParaQue = "o momento de preço e a rentabilidade";

        var cotacoes = await contexto.CotacoesDeProdutos.AsNoTracking()
            .Select(c => new { c.Fonte, c.CodigoNaFonte, c.Nivel, c.Produto, c.Classificacao, c.Mes })
            .ToListAsync(ct);

        if (cotacoes.Count == 0)
            return new("PRECOS", "Preço das culturas em SP", "CONAB · Socicana", false,
                [new("PRECOS", "Cotações", "meses", 0, 0, ParaQue)]);

        var series = cotacoes
            .GroupBy(c => (c.Fonte, c.CodigoNaFonte, c.Nivel))
            .Select(g => new
            {
                g.Key.Fonte,
                g.Key.CodigoNaFonte,
                g.Key.Nivel,
                Base = $"{ComoSeEscreve(g.First().Produto)} — {Minusculo(g.First().Classificacao)}",
                Meses = g.Select(c => c.Mes).Distinct().Order().ToList()
            })
            .ToList();

        // O NÍVEL SÓ ENTRA NO NOME QUANDO DUAS SÉRIES TERIAM O MESMO NOME — o kg de ATR mensal e o acumulado da
        // safra, por exemplo. Nas outras, ele só alongaria a linha.
        var repetidos = series.GroupBy(s => (s.Fonte, s.Base)).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

        var itens = series
            .Select(s =>
            {
                var primeiro = s.Meses[0];
                var ultimo = s.Meses[^1];
                var total = ((ultimo.Year - primeiro.Year) * 12) + ultimo.Month - primeiro.Month + 1;
                var faltando = Enumerable.Range(0, total).Select(primeiro.AddMonths).Except(s.Meses).ToList();

                var detalhe = (faltando.Count == 0
                                  ? string.Empty
                                  : $"faltam {string.Join(", ", faltando.Take(6).Select(m => m.ToString("MM/yyyy", CultureInfo.InvariantCulture)))}" +
                                    $"{(faltando.Count > 6 ? "…" : string.Empty)} · ")
                              + $"{Fonte(s.Fonte)} · o momento 12 ÷ 12 pede 24 meses completos; a série tem {s.Meses.Count}";

                return new ItemDeCobertura(
                    $"PRECO.{s.Fonte}.{s.CodigoNaFonte}.{s.Nivel}",
                    repetidos.Contains((s.Fonte, s.Base)) ? $"{s.Base} ({s.Nivel.ToLower(PtBr)})" : s.Base,
                    "meses",
                    total,
                    s.Meses.Count,
                    ParaQue,
                    Mes(primeiro),
                    Mes(ultimo),
                    detalhe);
            })
            .OrderBy(i => i.Situacao == SituacaoDaCobertura.Completa)
            .ThenBy(i => i.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        return new("PRECOS", "Preço das culturas em SP", "CONAB · Socicana", false, itens);
    }

    /// <summary>
    /// O CUSTO DE CADA CULTURA: quantas abas da série de SP chegam ao custo total.
    ///
    /// <para>As abas antigas de laranja e duas de cana param no custo operacional; a rentabilidade com custo
    /// total não sai para elas. O custo operacional existe em todas.</para>
    /// </summary>
    private async Task<GrupoDeCobertura> CustosAsync(CancellationToken ct)
    {
        const string ParaQue = "a rentabilidade com custo total";

        var abas = await contexto.CustosDeProducao.AsNoTracking()
            .Select(c => new { c.Cultura, c.Local, c.Variante, c.Safra, TemTotal = c.CustoTotalHa != null })
            .ToListAsync(ct);

        if (abas.Count == 0)
            return new("CUSTOS", "Custo de produção por cultura", "CONAB — séries históricas de SP", false,
                [new("CUSTOS", "Custo de produção", "abas da série", 0, 0, ParaQue)]);

        var itens = abas
            .GroupBy(a => a.Cultura)
            .Select(g => new ItemDeCobertura(
                $"CUSTO.{g.Key}",
                ComoSeEscreve(g.Key),
                "abas da série",
                g.Count(),
                g.Count(a => a.TemTotal),
                ParaQue,
                Ano(g.Min(a => a.Safra)),
                Ano(g.Max(a => a.Safra)),
                $"{Locais(g.Select(a => (a.Local, a.Variante)).Distinct().Count())} de SP · última safra {g.Max(a => a.Safra)}"))
            .OrderBy(i => i.Situacao == SituacaoDaCobertura.Completa)
            .ThenBy(i => i.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        return new("CUSTOS", "Custo de produção por cultura", "CONAB — séries históricas de SP", false, itens);
    }

    /// <summary>
    /// O CRÉDITO DO SICOR: os meses publicados e a base do índice de crédito por município.
    ///
    /// <para><b>O índice compara duas janelas de 12 meses</b> (a última e a anterior), contadas do último mês com
    /// dado, como o painel de crédito. Município sem linha de máquina numa das duas não tem razão para calcular —
    /// é o que o segundo item conta. Linha é linha do SICOR, não contrato.</para>
    ///
    /// <para><b>As contas de mês são em memória</b>: "ano × 12 + mês" em colunas smallint e tinyint já estourou
    /// no SQL Server (painel de fontes, 21/09/2026).</para>
    /// </summary>
    private async Task<GrupoDeCobertura> CreditoAsync(List<int> adr, CancellationToken ct)
    {
        const string Nome = "Crédito rural de investimento";
        const string Origem = "Banco Central — SICOR";

        var meses = (await contexto.CreditosRuraisDeInvestimento.AsNoTracking()
                .Select(c => new { c.Ano, c.Mes })
                .Distinct()
                .ToListAsync(ct))
            .Select(m => new DateOnly(m.Ano, m.Mes, 1))
            .Order()
            .ToList();

        if (meses.Count == 0)
            return new("CREDITO", Nome, Origem, false,
                [new("CREDITO.MESES", "Meses publicados", "meses", 0, 0, "o índice de crédito")]);

        var primeiro = meses[0];
        var ultimo = meses[^1];
        var total = ((ultimo.Year - primeiro.Year) * 12) + ultimo.Month - primeiro.Month + 1;

        var inicioDaUltima = ultimo.AddMonths(-11);
        var inicioDaAnterior = ultimo.AddMonths(-23);
        var anoMinimo = (short)inicioDaAnterior.Year;
        var produtos = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

        var linhasNaJanela = (await contexto.CreditosRuraisDeInvestimento.AsNoTracking()
                .Where(c => c.Ano >= anoMinimo && produtos.Contains(c.CodigoProduto) && adr.Contains(c.MunicipioId))
                .Select(c => new { c.MunicipioId, c.Ano, c.Mes })
                .Distinct()
                .ToListAsync(ct))
            .Select(c => (c.MunicipioId, Mes: new DateOnly(c.Ano, c.Mes, 1)))
            .Where(c => c.Mes >= inicioDaAnterior && c.Mes <= ultimo)
            .ToList();

        var naUltima = linhasNaJanela.Where(c => c.Mes >= inicioDaUltima).Select(c => c.MunicipioId).ToHashSet();
        var naAnterior = linhasNaJanela.Where(c => c.Mes < inicioDaUltima).Select(c => c.MunicipioId).ToHashSet();
        var nasDuas = naUltima.Intersect(naAnterior).Count();
        var soNaUltima = naUltima.Except(naAnterior).Count();
        var soNaAnterior = naAnterior.Except(naUltima).Count();

        return new("CREDITO", Nome, Origem, false,
        [
            new ItemDeCobertura(
                "CREDITO.MESES", "Meses publicados, todos os produtos", "meses", total, meses.Count, "a série mensal do crédito",
                Mes(primeiro), Mes(ultimo),
                $"último mês com dado: {ultimo.ToString("MM/yyyy", CultureInfo.InvariantCulture)} · os meses recentes ainda recebem registro atrasado do Banco Central"),
            new ItemDeCobertura(
                "CREDITO.MUNICIPIOS", "Municípios com crédito de máquina nas duas janelas de 12 meses", MunicipiosDaAdr, adr.Count, nasDuas,
                "o índice de crédito do município",
                Mes(inicioDaAnterior), Mes(ultimo),
                $"trator, máquinas e implementos, colheitadeiras · só na última janela: {soNaUltima} · só na anterior: {soNaAnterior} · " +
                $"em nenhuma: {adr.Count - nasDuas - soNaUltima - soNaAnterior}")
        ]);
    }

    // =============================================================================================
    // Dado interno — contagem sob a fronteira de filial
    // =============================================================================================

    /// <summary>Os clientes cujo endereço principal aponta para um município com código IBGE — os que têm lugar no mapa.</summary>
    private IQueryable<long> ClientesNoMapa() =>
        from endereco in contexto.Enderecos.AsNoTracking()
        where endereco.EhPrincipal && endereco.ExcluidoEm == null
        join municipio in contexto.Municipios.AsNoTracking() on endereco.MunicipioId equals (int?)municipio.Id
        where municipio.CodigoIbge != null
        select endereco.ClienteId;

    private async Task<GrupoDeCobertura> VendasDeMaquinaAsync(CancellationToken ct)
    {
        const string Unidade = "vendas";
        var vendas = contexto.VendasDeMaquina.AsNoTracking().Where(v => v.ExcluidoEm == null);
        var clientesNoMapa = ClientesNoMapa();

        var total = await vendas.CountAsync(ct);
        var comMunicipio = await vendas.CountAsync(v => clientesNoMapa.Contains(v.CompradorId), ct);
        var comModelo = await vendas.CountAsync(
            v => contexto.Equipamentos.Any(e => e.Id == v.EquipamentoId && e.ModeloId != null), ct);
        var comData = await vendas.CountAsync(v => v.VendidaEm != null || v.EntregueEm != null || v.FaturadaEm != null, ct);
        var inicio = await vendas.MinAsync(v => v.VendidaEm, ct);
        var fim = await vendas.MaxAsync(v => v.VendidaEm, ct);

        return new("VENDAS_DE_MAQUINA", "Vendas de máquina da Tracbel", "ART", true,
        [
            new ItemDeCobertura("INTERNO.VENDA.MUNICIPIO", "Município do comprador, com código IBGE", Unidade, total, comMunicipio,
                "as vendas da Tracbel por município", Mes(inicio), Mes(fim)),
            new ItemDeCobertura("INTERNO.VENDA.MODELO", "Modelo da máquina", Unidade, total, comModelo,
                "as vendas por categoria e modelo", Mes(inicio), Mes(fim)),
            new ItemDeCobertura("INTERNO.VENDA.DATA", "Data da venda, do faturamento ou da entrega", Unidade, total, comData,
                "as vendas por período", Mes(inicio), Mes(fim))
        ]);
    }

    private async Task<GrupoDeCobertura> EquipamentosAsync(CancellationToken ct)
    {
        const string Unidade = "equipamentos";
        var equipamentos = contexto.Equipamentos.AsNoTracking().Where(e => e.ExcluidoEm == null);
        var clientesNoMapa = ClientesNoMapa();

        var total = await equipamentos.CountAsync(ct);
        var comAno = await equipamentos.CountAsync(e => e.AnoFabricacao != null, ct);
        var comModelo = await equipamentos.CountAsync(e => e.ModeloId != null, ct);
        var comMunicipio = await equipamentos.CountAsync(e => e.ClienteId != null && clientesNoMapa.Contains(e.ClienteId.Value), ct);

        return new("EQUIPAMENTOS", "Máquinas dos clientes", "CRM · ART", true,
        [
            new ItemDeCobertura("INTERNO.EQUIPAMENTO.ANO", "Ano de fabricação", Unidade, total, comAno, "a idade do parque e a renovação"),
            new ItemDeCobertura("INTERNO.EQUIPAMENTO.MODELO", "Modelo", Unidade, total, comModelo, "o parque por categoria de máquina"),
            new ItemDeCobertura("INTERNO.EQUIPAMENTO.MUNICIPIO", "Cliente com município, com código IBGE", Unidade, total, comMunicipio,
                "o parque por município")
        ]);
    }

    private async Task<GrupoDeCobertura> VendasPerdidasAsync(CancellationToken ct)
    {
        const string Unidade = "vendas perdidas";
        var perdidas = contexto.VendasPerdidas.AsNoTracking().Where(v => v.ExcluidoEm == null);
        var clientesNoMapa = ClientesNoMapa();

        var total = await perdidas.CountAsync(ct);
        var comModelo = await perdidas.CountAsync(v => v.ModeloOfertado != null && v.ModeloOfertado != "", ct);
        var comPreco = await perdidas.CountAsync(v => v.PrecoOfertado != null, ct);
        var comConcorrente = await perdidas.CountAsync(v => v.ConcorrenteId != null, ct);
        var comMunicipio = await perdidas.CountAsync(v => v.ClienteId != null && clientesNoMapa.Contains(v.ClienteId.Value), ct);
        var inicio = await perdidas.MinAsync(v => (DateTime?)v.RegistradaEm, ct);
        var fim = await perdidas.MaxAsync(v => (DateTime?)v.RegistradaEm, ct);

        return new("VENDAS_PERDIDAS", "Vendas perdidas registradas", "CRM — formulário de venda perdida", true,
        [
            new ItemDeCobertura("INTERNO.PERDA.MODELO", "Modelo ofertado", Unidade, total, comModelo,
                "a referência de preço por modelo", Mes(inicio), Mes(fim)),
            new ItemDeCobertura("INTERNO.PERDA.PRECO", "Preço ofertado", Unidade, total, comPreco,
                "a referência de preço por modelo", Mes(inicio), Mes(fim)),
            new ItemDeCobertura("INTERNO.PERDA.CONCORRENTE", "Concorrente", Unidade, total, comConcorrente,
                "a oportunidade registrada por concorrente", Mes(inicio), Mes(fim)),
            new ItemDeCobertura("INTERNO.PERDA.MUNICIPIO", "Cliente com município, com código IBGE", Unidade, total, comMunicipio,
                "a oportunidade registrada por município", Mes(inicio), Mes(fim))
        ]);
    }

    private async Task<GrupoDeCobertura> EnderecosAsync(CancellationToken ct)
    {
        const string Unidade = "endereços";
        var enderecos = contexto.Enderecos.AsNoTracking().Where(e => e.ExcluidoEm == null);

        var total = await enderecos.CountAsync(ct);
        var comArea = await enderecos.CountAsync(e => e.Hectares != null, ct);
        var comCultura = await enderecos.CountAsync(e => e.CulturaId != null, ct);
        var comAmbos = await enderecos.CountAsync(e => e.Hectares != null && e.CulturaId != null, ct);
        var comMunicipio = await enderecos.CountAsync(
            e => contexto.Municipios.Any(m => m.Id == e.MunicipioId && m.CodigoIbge != null), ct);

        return new("ENDERECOS", "Endereços dos clientes", "CRM", true,
        [
            new ItemDeCobertura("INTERNO.ENDERECO.AREA_E_CULTURA", "Área em hectares e cultura, juntas", Unidade, total, comAmbos,
                "o potencial do cliente", Detalhe: $"só a área: {comArea} · só a cultura: {comCultura}"),
            new ItemDeCobertura("INTERNO.ENDERECO.MUNICIPIO", "Município com código IBGE", Unidade, total, comMunicipio,
                "o cliente no mapa")
        ]);
    }

    // =============================================================================================
    // Formatação
    // =============================================================================================

    private static string Ano(short ano) => ano.ToString(CultureInfo.InvariantCulture);

    private static string Mes(DateOnly mes) => mes.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    private static string? Mes(DateOnly? mes) => mes is { } m ? Mes(m) : null;

    private static string? Mes(DateTime? instante) => instante is { } i ? i.ToString("yyyy-MM", CultureInfo.InvariantCulture) : null;

    private static string Fonte(string codigo) => codigo switch
    {
        "CONAB" => "CONAB",
        "SOCICANA" => "Socicana",
        _ => codigo
    };

    private static string Locais(int quantos) => quantos == 1 ? "1 local" : $"{quantos} locais";

    /// <summary>
    /// "EM GRÃOS" → "em grãos", "KG DE ATR" → "kg de ATR": a classificação em minúsculas, menos a sigla do açúcar
    /// recuperável, que é sigla.
    /// </summary>
    private static string Minusculo(string texto) =>
        System.Text.RegularExpressions.Regex.Replace(texto.Trim().ToLower(PtBr), @"\batr\b", "ATR");

    /// <summary>"CANA DE AÇÚCAR" → "Cana de açúcar": só a primeira letra, como se escreve.</summary>
    private static string ComoSeEscreve(string texto)
    {
        var minusculo = texto.Trim().ToLower(PtBr);
        return minusculo.Length == 0 ? minusculo : char.ToUpper(minusculo[0], PtBr) + minusculo[1..];
    }
}
