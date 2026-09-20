using System.Net;
using System.Text;
using FluentAssertions;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Ibge;

/// <summary>
/// A ÁREA PLANTADA TEM DE SER A ÁREA PLANTADA (issue [083]).
///
/// <para>Na tabela 5457 do SIDRA a variável <b>8331</b> é "Área plantada ou destinada à colheita" e a
/// <b>216</b> é "Área colhida". O leitor pedia a 216 e chamava o resultado de área plantada; em cultura
/// perene (café, laranja) a colhida fica abaixo da plantada enquanto o pomar novo não produz, e o mapa C
/// saía subestimado justamente onde há plantio novo. Estes testes prendem a variável e a forma do
/// endereço para que a troca não volte sem alguém perceber.</para>
///
/// <para><b>Nenhuma chamada sai da máquina.</b> A leitura usa um tratador falso. Os códigos da resposta
/// de exemplo são os reais do SIDRA (município 3501608, produtos 40106 e 40139); as áreas são
/// inventadas — servem só para exercitar o formato.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDoIbgeTestes
{
    private const int SaoPaulo = 35;

    /// <summary>O código da variável "Área colhida", que o leitor pedia por engano.</summary>
    private const string AreaColhida = "/v/216/";

    private sealed class TratadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(responder(pedido));
    }

    /// <summary>Duas linhas de resposta do SIDRA, com o cabeçalho que vem sempre na primeira posição.</summary>
    private const string RespostaDoSidra = """
        [
          {
            "NC": "Nível Territorial (Código)", "NN": "Nível Territorial",
            "D1C": "Município (Código)", "D1N": "Município",
            "D2C": "Variável (Código)", "D2N": "Variável",
            "D3C": "Ano (Código)", "D3N": "Ano",
            "D4C": "Produto das lavouras temporárias e permanentes (Código)",
            "D4N": "Produto das lavouras temporárias e permanentes",
            "V": "Valor"
          },
          {
            "NC": "6", "NN": "Município",
            "D1C": "3501608", "D1N": "Araraquara (SP)",
            "D2C": "8331", "D2N": "Área plantada ou destinada à colheita",
            "D3C": "2025", "D3N": "2025",
            "D4C": "40106", "D4N": "Cana-de-açúcar",
            "V": "71500"
          },
          {
            "NC": "6", "NN": "Município",
            "D1C": "3501608", "D1N": "Araraquara (SP)",
            "D2C": "8331", "D2N": "Área plantada ou destinada à colheita",
            "D3C": "2025", "D3N": "2025",
            "D4C": "40139", "D4N": "Café (em grão) Total",
            "V": "-"
          }
        ]
        """;

    private static LeitorDoIbge LeitorQueResponde(string corpo, Action<HttpRequestMessage>? anotarPedido = null) =>
        new(new HttpClient(new TratadorFalso(pedido =>
        {
            anotarPedido?.Invoke(pedido);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(corpo, Encoding.UTF8, "application/json")
            };
        })));

    [Fact]
    public void O_endereco_da_area_plantada_pede_a_variavel_8331_e_nunca_a_area_colhida()
    {
        var endereco = LeitorDoIbge.EnderecoDaAreaPlantada(SaoPaulo);

        endereco.Should().Contain("/v/8331/", "8331 é \"Área plantada ou destinada à colheita\" na tabela 5457");
        endereco.Should().NotContain(AreaColhida, "216 é a área colhida, que é outra coisa (issue 83)");
    }

    [Fact]
    public void O_endereco_da_area_plantada_pede_todos_os_municipios_da_uf_e_todos_os_produtos()
    {
        var endereco = LeitorDoIbge.EnderecoDaAreaPlantada(SaoPaulo);

        endereco.Should().StartWith("https://apisidra.ibge.gov.br/values/t/5457/");
        endereco.Should().Contain("/n6/in%20n3%2035/", "a consulta é por município, dentro da UF pedida");
        endereco.Should().Contain("/p/last%201/", "vale o último ano publicado");
        endereco.Should().Contain("/c782/allxt", "todos os produtos da classificação 782");
        endereco.Should().EndWith("?formato=json");
    }

    [Fact]
    public async Task A_leitura_chama_o_endereco_da_area_plantada()
    {
        Uri? chamado = null;
        var leitor = LeitorQueResponde(RespostaDoSidra, pedido => chamado = pedido.RequestUri);

        await leitor.LerAreaPlantadaAsync(SaoPaulo, CancellationToken.None);

        chamado.Should().NotBeNull();
        Uri.UnescapeDataString(chamado!.ToString()).Should().Contain("/v/8331/");
    }

    [Fact]
    public async Task A_leitura_descarta_o_cabecalho_e_devolve_uma_linha_por_produto()
    {
        var linhas = await LeitorQueResponde(RespostaDoSidra).LerAreaPlantadaAsync(SaoPaulo, CancellationToken.None);

        linhas.Should().HaveCount(2, "a primeira posição da resposta do SIDRA é o cabeçalho");
        linhas[0].Should().Be(new LinhaDaAreaPlantada(3501608, 2025, 40106, "Cana-de-açúcar", "71500"));
        linhas[1].ValorBruto.Should().Be("-", "o símbolo do IBGE chega cru; quem converte é a carga");
    }
}
