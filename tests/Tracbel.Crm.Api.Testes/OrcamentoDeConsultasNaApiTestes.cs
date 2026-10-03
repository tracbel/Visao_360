using System.Globalization;
using System.Net;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O ORÇAMENTO DE CONSULTAS (documento 54 §3.5): quantas vezes cada rota pesada vai ao banco, na primeira leitura (cache
/// frio) e na segunda (cache quente). O teto de cada rota é o número medido — e só pode descer. Uma mudança que faça a
/// rota voltar a ir ao banco dezenas de vezes reprova aqui, antes de chegar ao servidor.
///
/// <para><b>Cada linha sobe a sua API</b>, com a janela da assinatura de produção (15 s): a primeira chamada é fria de
/// verdade — nenhuma outra rota encheu o cache de referência antes dela — e a segunda é a que o servidor faz dentro da
/// janela.</para>
///
/// <para>O cenário é o dos Cenários de mercado: três municípios da ADR com café, a regra do trator e as entregas do ART.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class OrcamentoDeConsultasNaApiTestes(ITestOutputHelper saida) : IAsyncLifetime
{
    /// <summary>
    /// A ASSINATURA DO TERRITÓRIO (documento 54 §3.2): cinco contas de máximo e contagem, relidas na primeira leitura de
    /// cada janela de 15 s. É o preço, pago uma vez por janela, de não reler a área de atuação a cada chamada.
    /// </summary>
    private const int AssinaturaDoTerritorio = 5;

    /// <summary>A ASSINATURA DO POTENCIAL: as mesmas cinco contas, nas tabelas da PAM, das regras e do estado.</summary>
    private const int AssinaturaDoPotencial = 5;

    /// <summary>
    /// O ANO ANTERIOR DA PAM vem sempre junto no potencial de referência — uma leitura a mais por versão, em vez de duas
    /// versões guardadas. A Demanda e os Cenários já o pediam; os Indicadores e o Diagnóstico passam a pagá-lo no frio.
    /// </summary>
    private const int AnoAnteriorDaPam = 1;

    /// <summary>
    /// A segunda leitura, com o cache de referência cheio — o medido depois do território e do potencial de referência
    /// (doc 54 §3.1). A LINHA DE BASE DE 03/10/2026, antes de qualquer otimização, era: indicadores 73, demanda 81,
    /// diagnóstico 77, cenários 87, dimensionamento 24, financiamentos 16, preços 16.
    /// </summary>
    private static readonly Dictionary<string, int> TetoQuente = new(StringComparer.Ordinal)
    {
        ["/api/v1/territorio/indicadores"] = 58,
        ["/api/v1/mercado/demanda"] = 65,
        ["/api/v1/mercado/diagnostico"] = 62,
        ["/api/v1/mercado/cenarios"] = 71,
        ["/api/v1/mercado/dimensionamento"] = 24,
        ["/api/v1/mercado/financiamentos"] = 16,
        ["/api/v1/mercado/precos"] = 16,
    };

    /// <summary>A primeira leitura, com o cache de referência vazio: a linha de base mais a assinatura de cada assunto lido.</summary>
    private static readonly Dictionary<string, int> TetoFrio = new(StringComparer.Ordinal)
    {
        ["/api/v1/territorio/indicadores"] = 73 + AssinaturaDoTerritorio + AssinaturaDoPotencial + AnoAnteriorDaPam,
        ["/api/v1/mercado/demanda"] = 81 + AssinaturaDoTerritorio + AssinaturaDoPotencial,
        ["/api/v1/mercado/diagnostico"] = 77 + AssinaturaDoTerritorio + AssinaturaDoPotencial + AnoAnteriorDaPam,
        ["/api/v1/mercado/cenarios"] = 87 + AssinaturaDoTerritorio + AssinaturaDoPotencial,
        ["/api/v1/mercado/dimensionamento"] = 24,
        ["/api/v1/mercado/financiamentos"] = 16,
        ["/api/v1/mercado/precos"] = 16,
    };

    private readonly ApiEmMemoria _api = new() { SegundosEntreConferenciasDaReferencia = 15 };

    public Task InitializeAsync() => _api.InitializeAsync();

    public async Task DisposeAsync()
    {
        await ((IAsyncLifetime)_api).DisposeAsync();
        await _api.DisposeAsync();
    }

    public static TheoryData<string> Rotas() => [.. TetoQuente.Keys];

    [Theory]
    [MemberData(nameof(Rotas))]
    public async Task A_rota_nao_passa_do_seu_orcamento_de_consultas(string rota)
    {
        await CenarioDosCenarios.SemearAsync(_api);
        var http = _api.ClienteDeRibeirao();

        var frio = await ConsultasAsync(http, rota);
        var quente = await ConsultasAsync(http, rota);
        saida.WriteLine($"ORCAMENTO {rota}: frio {frio}, quente {quente}");

        frio.Should().BeLessThanOrEqualTo(TetoFrio[rota], "o teto só desce (doc 54 §3.5)");
        quente.Should().BeLessThanOrEqualTo(TetoQuente[rota], "o teto só desce (doc 54 §3.5)");
    }

    private static async Task<int> ConsultasAsync(HttpClient http, string rota)
    {
        var resposta = await http.GetAsync(rota);
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return int.Parse(resposta.Headers.GetValues("X-Consultas-Ao-Banco").Single(), CultureInfo.InvariantCulture);
    }
}
