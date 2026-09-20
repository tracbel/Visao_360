using System.Net;
using System.Text;
using FluentAssertions;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Ibge;

/// <summary>
/// A LEITURA DA PRODUÇÃO AGRÍCOLA MUNICIPAL — três defeitos reais moram nestes testes.
///
/// <list type="number">
///   <item><b>A variável certa (issue 83).</b> Na tabela 5457 a <b>8331</b> é "Área plantada ou
///   destinada à colheita" e a <b>216</b> é "Área colhida". O leitor pedia a 216 e chamava o
///   resultado de área plantada; em cultura perene a colhida fica abaixo da plantada enquanto o
///   pomar novo não produz.</item>
///   <item><b>O lote (issue 95).</b> Pedir todos os produtos de uma vez passou a devolver 400: são
///   mais valores do que o SIDRA aceita numa consulta.</item>
///   <item><b>As quatro medidas numa linha só (issue 64).</b> O SIDRA devolve uma linha POR
///   VARIÁVEL; o leitor as junta pela chave (recorte, ano, produto), senão a carga gravaria o mesmo
///   município quatro vezes.</item>
/// </list>
///
/// <para><b>Nenhuma chamada sai da máquina.</b> A leitura usa um tratador falso. Os códigos são os
/// reais do SIDRA (município 3501608, UF 35, produtos 40106 e 40139); os números são inventados —
/// servem só para exercitar o formato.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDoIbgeTestes
{
    private const int SaoPaulo = 35;
    private const short Ano = 2025;

    /// <summary>O código da variável "Área colhida", que o leitor pedia sozinho, por engano.</summary>
    private const string SoAreaColhida = "/v/216/";

    private sealed class TratadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(responder(pedido));
    }

    /// <summary>Uma linha da resposta do SIDRA — uma variável de um produto num recorte.</summary>
    private static string Linha(int recorte, short variavel, int produto, string nome, string valor) => $$"""
        {
          "NC": "6", "NN": "Município",
          "D1C": "{{recorte}}", "D1N": "Recorte",
          "D2C": "{{variavel}}", "D2N": "Variável",
          "D3C": "{{Ano}}", "D3N": "{{Ano}}",
          "D4C": "{{produto}}", "D4N": "{{nome}}",
          "V": "{{valor}}"
        }
        """;

    /// <summary>A resposta inteira: o cabeçalho que vem sempre primeiro, mais as linhas.</summary>
    private static string RespostaCom(params string[] linhas) => $$"""
        [
          {
            "NC": "Nível Territorial (Código)", "NN": "Nível Territorial",
            "D1C": "Município (Código)", "D1N": "Município",
            "D2C": "Variável (Código)", "D2N": "Variável",
            "D3C": "Ano (Código)", "D3N": "Ano",
            "D4C": "Produto das lavouras temporárias e permanentes (Código)",
            "D4N": "Produto das lavouras temporárias e permanentes",
            "V": "Valor"
          }{{(linhas.Length == 0 ? "" : "," + string.Join(",", linhas))}}
        ]
        """;

    /// <summary>As quatro variáveis de um produto, como o SIDRA as devolve: quatro linhas.</summary>
    private static string QuatroMedidas(
        int recorte, int produto, string nome, string plantada, string colhida, string quantidade, string valor) =>
        string.Join(",", [
            Linha(recorte, 8331, produto, nome, plantada),
            Linha(recorte, 216, produto, nome, colhida),
            Linha(recorte, 214, produto, nome, quantidade),
            Linha(recorte, 215, produto, nome, valor)
        ]);

    /// <summary>Os metadados, com a classificação 782, o "Total" que fica de fora e o último ano.</summary>
    private static string MetadadosCom(params int[] produtos)
    {
        // A categoria 0 vem sempre primeiro, como no IBGE — é o "Total" que o leitor precisa descartar.
        var categorias = string.Join(",", produtos.Select(p => $"{{ \"id\": {p}, \"nome\": \"Produto {p}\" }}"));

        return $$"""
            {
              "id": 5457,
              "periodicidade": { "frequencia": "anual", "inicio": 1974, "fim": {{Ano}} },
              "variaveis": [ { "id": 8331, "nome": "Área plantada ou destinada à colheita" } ],
              "classificacoes": [
                {
                  "id": 782,
                  "nome": "Produto das lavouras temporárias e permanentes",
                  "categorias": [ { "id": 0, "nome": "Total" }{{(produtos.Length == 0 ? "" : "," + categorias)}} ]
                },
                { "id": 12345, "nome": "Outra classificação", "categorias": [ { "id": 7, "nome": "Sete" } ] }
              ]
            }
            """;
    }

    /// <summary>
    /// Um leitor com tratador falso que responde os metadados no endereço deles e a resposta do
    /// SIDRA em qualquer outro — anotando cada pedido, que é como os testes contam as requisições.
    /// </summary>
    private static LeitorDoIbge Leitor(string metadados, string sidra, List<Uri>? pedidos = null) =>
        new(new HttpClient(new TratadorFalso(pedido =>
        {
            pedidos?.Add(pedido.RequestUri!);

            var corpo = pedido.RequestUri!.ToString().Contains("agregados", StringComparison.Ordinal)
                ? metadados
                : sidra;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(corpo, Encoding.UTF8, "application/json")
            };
        })));

    [Fact]
    public void O_endereco_pede_as_quatro_variaveis_comecando_pela_area_plantada()
    {
        var endereco = LeitorDoIbge.EnderecoDaProducaoNosMunicipios(SaoPaulo, [40106, 40139], Ano);

        endereco.Should().Contain("/v/8331,216,214,215/",
            "8331 é a área plantada, 216 a colhida, 214 a quantidade e 215 o valor — nesta ordem");
        endereco.Should().NotContain(SoAreaColhida,
            "pedir só a 216 e chamar de área plantada foi o defeito da issue 83");
        endereco.Should().NotContain("/v/112/",
            "rendimento médio é quantidade ÷ área colhida: número derivado não se guarda ao lado das parcelas");
    }

    [Fact]
    public void O_endereco_do_municipio_pede_a_uf_inteira_e_o_do_estado_pede_o_total()
    {
        var municipios = LeitorDoIbge.EnderecoDaProducaoNosMunicipios(SaoPaulo, [40106], Ano);
        var estado = LeitorDoIbge.EnderecoDaProducaoNoEstado(SaoPaulo, [40106], Ano);

        municipios.Should().StartWith("https://apisidra.ibge.gov.br/values/t/5457/");
        municipios.Should().Contain("/n6/in%20n3%2035/", "nível 6 é município, dentro da UF pedida");
        estado.Should().Contain("/n3/35/", "nível 3 é a UF inteira — e o total dela não é a soma dos municípios");

        foreach (var endereco in new[] { municipios, estado })
        {
            endereco.Should().Contain($"/p/{Ano}/", "o ano vai explícito: a carga traz a série curta, ano a ano");
            endereco.Should().Contain("/c782/40106", "o lote de produtos vai explícito (issue 95)");
            endereco.Should().EndWith("?formato=json");
        }
    }

    [Fact]
    public async Task Os_produtos_vem_dos_metadados_e_o_total_fica_de_fora()
    {
        var produtos = await Leitor(MetadadosCom(40106, 40139), RespostaCom())
            .LerProdutosDaPamAsync(CancellationToken.None);

        produtos.Should().Equal(40106, 40139);
        produtos.Should().NotContain(0, "a categoria 0 é o Total: somá-la dobraria a área do município");
    }

    [Fact]
    public async Task O_ultimo_ano_publicado_vem_dos_metadados()
    {
        var ano = await Leitor(MetadadosCom(40106), RespostaCom()).LerUltimoAnoDaPamAsync(CancellationToken.None);

        ano.Should().Be(Ano);
    }

    [Fact]
    public async Task As_quatro_linhas_de_um_produto_viram_uma_linha_com_as_quatro_medidas()
    {
        var resposta = RespostaCom(
            QuatroMedidas(3501608, 40106, "Cana-de-açúcar", "71500", "71000", "5720000", "1200000"));

        var linhas = await Leitor(MetadadosCom(40106), resposta)
            .LerProducaoNosMunicipiosAsync(SaoPaulo, Ano, CancellationToken.None);

        linhas.Should().HaveCount(1, "o SIDRA manda quatro linhas, uma por variável — o leitor junta");

        var linha = linhas[0];
        linha.CodigoDoRecorte.Should().Be(3501608);
        linha.Ano.Should().Be(Ano);
        linha.ProdutoCodigo.Should().Be(40106);
        linha.ProdutoNome.Should().Be("Cana-de-açúcar");
        linha.AreaPlantadaBruta.Should().Be("71500");
        linha.AreaColhidaBruta.Should().Be("71000");
        linha.QuantidadeProduzidaBruta.Should().Be("5720000");
        linha.ValorDaProducaoBruto.Should().Be("1200000");
    }

    [Fact]
    public async Task Variavel_que_o_ibge_nao_devolveu_fica_nula_e_os_simbolos_chegam_crus()
    {
        // O município tem área plantada zero ("-") e valor sigiloso ("X"); a quantidade não veio.
        var resposta = RespostaCom(
            Linha(3501608, 8331, 40139, "Café (em grão) Total", "-"),
            Linha(3501608, 216, 40139, "Café (em grão) Total", "..."),
            Linha(3501608, 215, 40139, "Café (em grão) Total", "X"));

        var linhas = await Leitor(MetadadosCom(40139), resposta)
            .LerProducaoNosMunicipiosAsync(SaoPaulo, Ano, CancellationToken.None);

        var linha = linhas.Single();
        linha.AreaPlantadaBruta.Should().Be("-", "quem decide que '-' é zero é o saneamento, não o leitor");
        linha.AreaColhidaBruta.Should().Be("...");
        linha.ValorDaProducaoBruto.Should().Be("X");
        linha.QuantidadeProduzidaBruta.Should().BeNull("a variável 214 não veio nesta resposta");
    }

    [Fact]
    public async Task Mais_produtos_que_o_lote_viram_mais_de_uma_consulta_e_nada_se_perde()
    {
        // O DEFEITO DA ISSUE 95 EM UMA LINHA: o SIDRA recusa a consulta grande demais. Com 25
        // produtos e lotes de 10, são três consultas — e as três respostas precisam chegar juntas ao
        // fim, cada uma sem o seu cabeçalho.
        var produtos = Enumerable.Range(40100, 25).ToArray();
        var pedidos = new List<Uri>();
        var resposta = RespostaCom(QuatroMedidas(3501608, 40106, "Cana-de-açúcar", "1", "1", "1", "1"));

        var linhas = await Leitor(MetadadosCom(produtos), resposta, pedidos)
            .LerProducaoNosMunicipiosAsync(SaoPaulo, Ano, CancellationToken.None);

        pedidos.Should().HaveCount(4, "um pedido de metadados e três lotes de até 10 produtos");
        linhas.Should().HaveCount(1, "as três respostas falam do mesmo município, ano e produto");

        var lotes = pedidos.Skip(1).Select(p => Uri.UnescapeDataString(p.ToString())).ToList();
        lotes[0].Should().Contain("/c782/40100,", "o primeiro lote começa no primeiro produto");
        lotes[^1].Should().Contain(",40124?", "o último lote termina no último produto — nenhum fica para trás");
    }

    [Fact]
    public async Task O_total_do_estado_e_lido_no_nivel_da_uf()
    {
        var pedidos = new List<Uri>();
        var resposta = RespostaCom(
            QuatroMedidas(35, 40106, "Cana-de-açúcar", "5430681", "5415896", "400000000", "50000000"));

        var linhas = await Leitor(MetadadosCom(40106), resposta, pedidos)
            .LerProducaoNoEstadoAsync(SaoPaulo, Ano, CancellationToken.None);

        linhas.Single().CodigoDoRecorte.Should().Be(35, "no nível 3 o recorte é a UF, não o município");
        Uri.UnescapeDataString(pedidos[^1].ToString()).Should().Contain("/n3/35/");
    }
}
