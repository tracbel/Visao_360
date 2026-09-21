using FluentAssertions;
using Tracbel.Crm.Integracao.Conab;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Precos;

/// <summary>
/// A LEITURA DOS CUSTOS DE PRODUÇÃO DA CONAB (issue 67) — uma aba por layout, como a CONAB os escreveu.
///
/// <para>As linhas abaixo reproduzem, célula a célula, as abas conferidas em 21/09/2026:</para>
/// <list type="bullet">
///   <item><b>layout novo</b> (Piracicaba 2025, cana): produtividade num texto só, mês por extenso;</item>
///   <item><b>layout de 2016–2018</b> (Franca 2017, café): produtividade como texto NA CÉLULA AO LADO;</item>
///   <item><b>layout antigo</b> (Potirendaba 2010, laranja): rótulos em outra caixa, produtividade em
///   número e unidade separados, e a aba <b>para no custo operacional</b>.</item>
/// </list>
///
/// <para>O aceite da issue é o número da planilha do comercial: <b>Piracicaba 2025 = R$ 13.903,20</b> e
/// <b>Franca 2025 = R$ 29.279,94</b> de custo total por hectare.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDeCustosDaConabTestes
{
    private static object?[] L(params object?[] celulas) => celulas;

    private static readonly IReadOnlyList<object?[]> Piracicaba2025 =
    [
        L("Ciclo de Cultura: SEMI-PERMANENTE", null, "Tipo do Relatório: Estimado"),
        L("Mês/Ano: Setembro/2025", null, "Etapa de Cultivo: PRODUÇÃO"),
        L("Produtividade Média: 77987,01 kg/ha"),
        L("DISCRIMINAÇÃO", "CUSTO POR HA", "CUSTO / t", "PARTICIPAÇÃO CV(%)", "PARTICIPAÇÃO CT(%)"),
        L("TOTAL DAS DESPESAS DE CUSTEIO (A)", 9285.13, 118.8743, 89.87, 66.8),
        L("CUSTO VARIÁVEL (A+B+C=D)", 10330.594025974025, 132.24702, 100.0, 74.31),
        L("TOTAL DE OUTROS CUSTOS FIXOS (F)", 2303.45, 29.90914, 22.3, 16.57),
        L("CUSTO FIXO (E+F=G)", 2663.6899999999996, 34.52645, 25.78, 19.17),
        L("CUSTO OPERACIONAL (D+G=H)", 12994.284025974026, 166.77347, 125.78, 93.48),
        L("VI - RENDA DE FATORES"),
        L("TOTAL DE RENDA DE FATORES (F)", 908.92, 11.76423, 8.8, 6.53),
        L("CUSTO TOTAL (H+I=J)", 13903.204025974026, 178.5377, 134.58, 100.01),
        L("Elaboração: CONAB/DIPAI/SUINF/GECUP")
    ];

    private static readonly IReadOnlyList<object?[]> Franca2017 =
    [
        L("SAFRA ANUAL - 2017/18 - FRANCA - SP"),
        L("MÊS/ANO: NOVEMBRO/2017", null, "ETAPA DE CULTIVO: PRODUÇÃO"),
        L("PRODUTIVIDADE", "1800,00 kg/ha", "Ex-Ante"),
        L("DISCRIMINAÇÃO", "CUSTO POR HA", "CUSTO / 60 kg"),
        L("CUSTO VARIÁVEL (A+B+C=D)", 10200.11, 340.0),
        L("CUSTO FIXO (E+F=G)", 1048.64, 34.97),
        L("CUSTO OPERACIONAL (D+G=H)", 11248.75, 374.97),
        L("TOTAL DA RENDA DE FATORES (I)", 1466.96, 48.9),
        L("CUSTO TOTAL (H+I=J)", 12715.71, 423.87)
    ];

    private static readonly IReadOnlyList<object?[]> Potirendaba2010 =
    [
        L("CUSTO DE PRODUÇÃO ESTIMADO - AGRICULTURA"),
        L("LOCAL: POTIRENDABA"),
        L("PRODUTIVIDADE MÉDIA:", 65280.0, "KG/HA"),
        L("Custo Variável  (A+B+C = D)", 11906.23, 7.34, 0.84),
        L("V - OUTROS CUSTOS FIXOS", 0.0, 0.0),
        L("TOTAL DE OUTROS CUSTOS FIXOS (F)", 353.13, 0.22, 0.02),
        L("Custo Fixo  (E+F = G)", 2280.78, 1.42, 0.16),
        L("CUSTO OPERACIONAL  (D+G = H)", 14187.0, 8.76, 1.0)
    ];

    [Fact]
    public void Layout_novo_Piracicaba_2025_bate_com_a_planilha_do_comercial()
    {
        var c = LeitorDeCustosDaConab.InterpretarAba("CANA DE AÇÚCAR", "Piracicaba-SP-2025", Piracicaba2025)!;

        c.Should().NotBeNull();
        c.CustoTotalHa.Should().Be(13903.20m, "é o número que a planilha do comercial usa");
        c.CustoTotalUnidade.Should().Be(178.53770m);
        c.CustoOperacionalHa.Should().Be(12994.28m);
        c.CustoVariavelHa.Should().Be(10330.59m, "a CONAB calcula com dízima; o CRM guarda duas casas no hectare");
        c.CustoFixoHa.Should().Be(2663.69m, "\"CUSTO FIXO\" não pode casar com \"TOTAL DE OUTROS CUSTOS FIXOS\"");
        c.RendaDeFatoresHa.Should().Be(908.92m);
        (c.Produtividade, c.UnidadeDaProdutividade).Should().Be((77987.01m, "kg/ha"));
        (c.Local, c.Variante, c.Safra, c.MesDoRelatorio).Should().Be(("Piracicaba", (string?)null, (short)2025, (byte?)9));
    }

    [Fact]
    public void Layout_de_2017_com_a_produtividade_na_celula_ao_lado_e_renda_escrita_com_DA()
    {
        var c = LeitorDeCustosDaConab.InterpretarAba("CAFÉ ARÁBICA", "Franca-SP-2017", Franca2017)!;

        (c.Produtividade, c.UnidadeDaProdutividade).Should().Be((1800.00m, "kg/ha"));
        c.RendaDeFatoresHa.Should().Be(1466.96m, "\"TOTAL DA RENDA DE FATORES\" é a mesma camada");
        c.CustoTotalHa.Should().Be(12715.71m);
        c.MesDoRelatorio.Should().Be((byte)11);
    }

    [Fact]
    public void Layout_antigo_que_para_no_operacional_fica_sem_total_e_nao_com_zero()
    {
        var c = LeitorDeCustosDaConab.InterpretarAba("LARANJA", "Potirendaba-SP-2010", Potirendaba2010)!;

        c.CustoOperacionalHa.Should().Be(14187.00m);
        c.CustoTotalHa.Should().BeNull("a aba não traz custo total; zero seria afirmar uma coisa falsa");
        c.RendaDeFatoresHa.Should().BeNull();
        (c.Produtividade, c.UnidadeDaProdutividade).Should().Be((65280.0m, "kg/ha"));
    }

    [Fact]
    public void Aba_sem_custo_operacional_nao_e_aba_de_custo() =>
        LeitorDeCustosDaConab.InterpretarAba("CAFÉ ARÁBICA", "Franca-SP-2003", [L("CUSTO VARIÁVEL", 1.0, 2.0)])
            .Should().BeNull();

    [Theory]
    [InlineData("Franca-SP-2010", "Franca", null, 2010, null)]
    [InlineData("SP-Franca 2025", "Franca", null, 2025, null)]
    [InlineData("Rasteiro-Jaboticabal-SP-2014", "Jaboticabal", "Rasteiro", 2014, null)]
    [InlineData("Ereto-Tupã-SP-2005", "Tupã", "Ereto", 2005, null)]
    [InlineData("Santa Salete-SP-2022-11", "Santa Salete", null, 2022, (byte)11)]
    [InlineData("Aparecida D´Oeste-SP-2008", "Aparecida D'Oeste", null, 2008, null)]
    public void O_nome_da_aba_da_o_local_a_variante_a_safra_e_o_mes(string aba, string local, string? variante, int safra, byte? mes) =>
        LeitorDeCustosDaConab.DesmontarNomeDaAba(aba).Should().Be((local, variante, (short)safra, mes));

    [Theory]
    [InlineData("Franca-SP-2010", true)]
    [InlineData("SP-Franca 2025", true)]
    [InlineData("Santa Salete-SP-2022-11", true)]
    [InlineData("Guaxupé-MG-2025", false)]
    [InlineData("Espera Feliz-MG-SPE-2020", false)]
    public void So_as_abas_de_Sao_Paulo_entram(string aba, bool esperado) =>
        LeitorDeCustosDaConab.EhAbaDeSaoPaulo(aba).Should().Be(esperado);

    [Fact]
    public void Aba_sem_ano_no_nome_para_com_erro()
    {
        var desmontar = () => LeitorDeCustosDaConab.DesmontarNomeDaAba("Resumo SP");
        desmontar.Should().Throw<FormatException>();
    }

    [Fact]
    public async Task A_planilha_e_achada_pelo_nome_da_cultura_na_pagina_e_o_milho_abre_a_pasta()
    {
        const string pagina = """
            <a href="https://www.gov.br/conab/x/agricolas/serie-historica-custos-cana-de-acucar-2008-a-2025.xls">Cana-de-açúcar</a>
            <a href="https://www.gov.br/conab/x/agricolas/seriehistoricacustoscafeconilon2007a2025.xls">Café conilon</a>
            <a href="https://www.gov.br/conab/x/agricolas/seriehistoricacustoscafearabica2003a2025.xls">Café arábica</a>
            <a href="https://www.gov.br/conab/x/agricolas/milho">Milho</a>
            """;
        const string pastaDoMilho = """
            <a href="https://www.gov.br/conab/x/agricolas/milho/milho_2a_safra_serie_historica_2005-2025.xls/view">Milho_2ª_Safra_Serie_Historica_2005-2025.xls</a>
            """;

        var leitor = new LeitorDeCustosDaConab(new HttpClient(new Tratador(pedido =>
            pedido.RequestUri!.AbsolutePath.EndsWith("/milho", StringComparison.Ordinal) ? pastaDoMilho : pagina)));

        var planilhas = await leitor.LocalizarPlanilhasAsync(CancellationToken.None);

        planilhas.Select(p => p.Cultura).Should().BeEquivalentTo("CANA DE AÇÚCAR", "CAFÉ ARÁBICA", "MILHO 2ª SAFRA");
        planilhas.Should().NotContain(p => p.Endereco.AbsolutePath.Contains("conilon"), "conilon não é arábica");
        planilhas.Single(p => p.Cultura.StartsWith("MILHO")).Endereco.AbsolutePath
            .Should().EndWith(".xls", "o /view do Plone é a página, não o arquivo");
        planilhas.Single(p => p.Cultura == "CANA DE AÇÚCAR").UnidadeComercial.Should().Be("t");
    }

    private sealed class Tratador(Func<HttpRequestMessage, string> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(responder(pedido)) });
    }
}
