using System.Net;
using System.Text;
using FluentAssertions;
using Tracbel.Crm.Integracao.BancoCentral;
using Tracbel.Crm.Integracao.Conab;
using Tracbel.Crm.Integracao.Socicana;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Precos;

/// <summary>
/// A LEITURA DOS PREÇOS DE MERCADO (issue 66) — CONAB, Socicana e dólar PTAX.
///
/// <para>O que quebra em silêncio e é travado aqui:</para>
/// <list type="bullet">
///   <item>a CONAB em Latin-1 lida como UTF-8 — "PREÇO RECEBIDO" vira lixo e o filtro não acha nada;</item>
///   <item>o preço PAGO pelo produtor (adubo, trator) misturado ao RECEBIDO (o que ele vende);</item>
///   <item>janeiro a março da safra de cana no ano errado — a safra 26/27 termina em março de 2027;</item>
///   <item>o mês da safra corrente que ainda não aconteceu virando zero;</item>
///   <item>o SGS respondendo erro com status 200 e um objeto no lugar da lista.</item>
/// </list>
///
/// <para><b>Nenhuma chamada sai da máquina.</b> As amostras seguem o formato conferido contra as
/// fontes reais em 21/09/2026.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitoresDePrecosTestes
{
    private sealed class TratadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(responder(pedido));
    }

    private static HttpClient Cliente(byte[] corpo, List<Uri>? pedidos = null) =>
        new(new TratadorFalso(pedido =>
        {
            pedidos?.Add(pedido.RequestUri!);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(corpo) };
        }));

    // =============================================================================================
    // CONAB
    // =============================================================================================

    // O ARQUIVO REAL: colunas de texto com espaços à direita, vírgula decimal, quatro níveis.
    private const string ArquivoDaConab =
        "produto;classificao_produto;id_produto;uf;regiao;ano;mes;dsc_nivel_comercializacao;valor_produto_kg\n" +
        "SOJA                     ;EM GRÃOS            ;4744;SP        ;SUDESTE        ;2026;8;PREÇO RECEBIDO P/ PRODUTOR                                  ;2,16\n" +
        "SOJA                     ;EM GRÃOS            ;4744;SP        ;SUDESTE        ;2026;8;ATACADO                                                     ;2,40\n" +
        "CAFE                     ;ARÁBICA TIPO 6, BEBI;11195;SP        ;SUDESTE        ;2026;2;PREÇO RECEBIDO P/ PRODUTOR                                  ;31,57\n" +
        "TRATOR                   ;110 4X4             ;9001;SP        ;SUDESTE        ;2026;3;PREÇO PAGO PELO PRODUTOR                                    ;350000,00\n" +
        "SOJA                     ;EM GRÃOS            ;4744;MT        ;CENTRO-OESTE   ;2026;8;PREÇO RECEBIDO P/ PRODUTOR                                  ;1,95\n";

    [Fact]
    public async Task A_CONAB_e_lida_em_Latin1_e_so_o_preco_RECEBIDO_de_SP_entra()
    {
        var pedidos = new List<Uri>();
        var leitor = new LeitorDaConab(Cliente(Encoding.Latin1.GetBytes(ArquivoDaConab), pedidos));

        var linhas = await leitor.LerPrecosMensaisAsync("SP", LeitorDaConab.NivelRecebidoPeloProdutor, CancellationToken.None);

        pedidos.Should().ContainSingle().Which.ToString().Should().Be(LeitorDaConab.EnderecoDosPrecosMensaisPorUf);

        // Fora: o atacado da soja, o trator (preço PAGO) e a soja de MT.
        linhas.Should().HaveCount(2);
        linhas.Select(l => l.Produto).Should().BeEquivalentTo("SOJA", "CAFE");

        var soja = linhas.Single(l => l.Produto == "SOJA");
        soja.CodigoDoProduto.Should().Be("4744");
        soja.Classificacao.Should().Be("EM GRÃOS", "o acento só chega legível se o arquivo for lido em Latin-1");
        soja.Uf.Should().Be("SP");
        (soja.Ano, soja.Mes).Should().Be((2026, 8));
        soja.ValorPorKgBruto.Should().Be("2,16");
    }

    [Fact]
    public void As_colunas_sao_achadas_pelo_nome_e_nao_pela_posicao()
    {
        // O arquivo por município tem duas colunas a mais no meio. Uma mudança igual no de UF não
        // pode deslocar o valor para a coluna do nível.
        const string comColunasAMais =
            "produto;classificao_produto;id_produto;nom_municipio;cod_ibge;uf;regiao;ano;mes;dsc_nivel_comercializacao;valor_produto_kg\n" +
            "MILHO;EM GRÃOS;4742;ASSIS-SP;3504008;SP;SUDESTE;2026;8;PREÇO RECEBIDO P/ PRODUTOR;1,01\n";

        var linha = LeitorDaConab.Interpretar(comColunasAMais, "SP", "RECEBIDO").Should().ContainSingle().Subject;

        linha.Produto.Should().Be("MILHO");
        linha.ValorPorKgBruto.Should().Be("1,01");
    }

    [Fact]
    public void Cabecalho_diferente_para_com_erro_em_vez_de_devolver_nada()
    {
        var ler = () => LeitorDaConab.Interpretar("produto;preco\nSOJA;2,16\n", "SP", "RECEBIDO");

        ler.Should().Throw<InvalidDataException>().WithMessage("*colunas esperadas*");
    }

    [Theory]
    [InlineData("2,16", 2.16)]
    [InlineData("22,0", 22.0)]
    [InlineData("31,57", 31.57)]
    public void O_valor_da_CONAB_tem_virgula_decimal(string bruto, decimal esperado) =>
        LeitorDaConab.ValorPorKg(bruto).Should().Be(esperado);

    [Theory]
    [InlineData("2.32")]      // ponto: o formato mudou — "tolerar" daria 232
    [InlineData("1.234,56")]  // milhar com ponto
    [InlineData("0")]
    [InlineData("-1,00")]
    [InlineData("")]
    public void Valor_fora_do_formato_da_CONAB_e_recusado(string bruto)
    {
        var converter = () => LeitorDaConab.ValorPorKg(bruto);
        converter.Should().Throw<FormatException>();
    }

    // =============================================================================================
    // Socicana
    // =============================================================================================

    // DUAS SAFRAS, como a página real: a corrente pela metade, a anterior completa, com o estilo da
    // tabela dentro dela e a entidade HTML do "ç" de março.
    private const string PaginaDaSocicana = """
        <html><body>
        <table class="tableizer-table"><style>.tableizer-table td { padding: 4px; }</style>
        <thead><tr><th>SAFRA 26/27</th></tr><tr><th>R$ por kg de ATR</th><th>MENSAL</th><th>ACUMULADO</th></tr></thead>
        <tbody>
        <tr><td>Abril</td><td>0,9398</td><td>0,9398</td></tr>
        <tr><td>Agosto</td><td>0,8692</td><td>0,8770</td></tr>
        <tr><td>Setembro</td><td>&nbsp;</td><td>&nbsp;</td></tr>
        <tr><td>Mar&ccedil;o</td><td></td><td></td></tr>
        </tbody></table>
        <p>Preço do KG – Safra 25/26</p>
        <table class="tableizer-table">
        <thead><tr><th>SAFRA 25/26</th></tr></thead>
        <tbody>
        <tr><td>Dezembro</td><td>1,0587</td><td>1,0952</td></tr>
        <tr><td>Janeiro</td><td>1,0567</td><td>1,0929</td></tr>
        <tr><td>Mar&ccedil;o</td><td>1,0007</td><td>1,0816</td></tr>
        </tbody></table>
        </body></html>
        """;

    [Fact]
    public void A_safra_vai_de_abril_a_marco_e_janeiro_cai_no_ano_seguinte()
    {
        var linhas = LeitorDaSocicana.Interpretar(PaginaDaSocicana);

        linhas.Select(l => (l.Mes, l.MensalBruto)).Should().BeEquivalentTo(new[]
        {
            (new DateOnly(2026, 4, 1), "0,9398"),
            (new DateOnly(2026, 8, 1), "0,8692"),    // o valor conferido contra a planilha
            (new DateOnly(2025, 12, 1), "1,0587"),
            (new DateOnly(2026, 1, 1), "1,0567"),    // janeiro da 25/26 é de 2026, não de 2025
            (new DateOnly(2026, 3, 1), "1,0007")     // "Mar&ccedil;o" reconhecido
        });

        linhas.Single(l => l.Mes == new DateOnly(2026, 8, 1)).AcumuladoBruto.Should().Be("0,8770");
        linhas.Where(l => l.Mes.Year == 2026 && l.Mes.Month == 3).Should().ContainSingle()
            .Which.Safra.Should().Be("25/26");
    }

    [Fact]
    public void Mes_da_safra_corrente_que_ainda_nao_aconteceu_nao_vira_zero() =>
        LeitorDaSocicana.Interpretar(PaginaDaSocicana)
            .Should().NotContain(l => l.Mes == new DateOnly(2026, 9, 1) || l.Mes == new DateOnly(2027, 3, 1));

    [Fact]
    public void Pagina_que_mudou_de_forma_para_com_erro()
    {
        var ler = () => LeitorDaSocicana.Interpretar("<html><body><p>Página em manutenção</p></body></html>");

        ler.Should().Throw<InvalidDataException>().WithMessage("*nenhuma safra*");
    }

    [Fact]
    public async Task A_Socicana_e_lida_do_endereco_publico()
    {
        var pedidos = new List<Uri>();
        var leitor = new LeitorDaSocicana(Cliente(Encoding.UTF8.GetBytes(PaginaDaSocicana), pedidos));

        (await leitor.LerAsync(CancellationToken.None)).Should().HaveCount(5);
        pedidos.Should().ContainSingle().Which.ToString().Should().Be(LeitorDaSocicana.EnderecoDoPrecoDoKg);
    }

    // =============================================================================================
    // Dólar PTAX
    // =============================================================================================

    [Fact]
    public async Task O_PTAX_mensal_vem_do_SGS_3698_desde_2015()
    {
        var pedidos = new List<Uri>();
        const string resposta = """[{"data":"01/01/2015","valor":"2.6342"},{"data":"01/08/2026","valor":"5.1532"}]""";
        var leitor = new LeitorDoPtax(Cliente(Encoding.UTF8.GetBytes(resposta), pedidos));

        var linhas = await leitor.LerAsync(LeitorDoPtax.InicioDaSerie, CancellationToken.None);

        pedidos.Should().ContainSingle().Which.ToString().Should()
            .Contain("bcdata.sgs.3698").And.Contain("dataInicial=01/01/2015");

        linhas.Should().HaveCount(2);
        LeitorDoPtax.Mes(linhas[1].DataBruta).Should().Be(new DateOnly(2026, 8, 1));
        LeitorDoPtax.ReaisPorDolar(linhas[1].ValorBruto).Should().Be(5.1532m, "o SGS usa PONTO decimal, ao contrário da CONAB");
    }

    [Fact]
    public void O_SGS_respondendo_objeto_no_lugar_da_lista_e_erro_e_nao_lista_vazia()
    {
        var ler = () => LeitorDoPtax.Interpretar("""{"erro":"O sistema aceita uma janela de consulta de, no máximo, 10 anos"}""");

        ler.Should().Throw<InvalidDataException>().WithMessage("*não respondeu uma lista*");
    }

    [Fact]
    public void Mes_da_serie_mensal_fora_do_dia_1_e_recusado()
    {
        var converter = () => LeitorDoPtax.Mes("15/08/2026");
        converter.Should().Throw<FormatException>();
    }
}
