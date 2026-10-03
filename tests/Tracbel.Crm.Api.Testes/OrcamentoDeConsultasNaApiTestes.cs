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
/// <para>O cenário é o dos Cenários de mercado: três municípios da ADR com café, a regra do trator e as entregas do ART.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class OrcamentoDeConsultasNaApiTestes(ApiEmMemoria api, ITestOutputHelper saida) : IClassFixture<ApiEmMemoria>
{
    /// <summary>
    /// A primeira leitura, com o cache de referência vazio. A LINHA DE BASE DE 03/10/2026 (doc 54, antes de qualquer
    /// otimização), em consultas ao banco por chamada: indicadores 73, demanda 81, diagnóstico 77, cenários 87,
    /// dimensionamento 24, financiamentos 16, preços 16.
    /// </summary>
    private static readonly Dictionary<string, int> TetoFrio = new(StringComparer.Ordinal)
    {
        ["/api/v1/territorio/indicadores"] = 73,
        ["/api/v1/mercado/demanda"] = 81,
        ["/api/v1/mercado/diagnostico"] = 77,
        ["/api/v1/mercado/cenarios"] = 87,
        ["/api/v1/mercado/dimensionamento"] = 24,
        ["/api/v1/mercado/financiamentos"] = 16,
        ["/api/v1/mercado/precos"] = 16,
    };

    /// <summary>A segunda leitura da mesma rota, com o cache de referência já cheio.</summary>
    private static readonly Dictionary<string, int> TetoQuente = new(TetoFrio, StringComparer.Ordinal);

    public static TheoryData<string> Rotas() => [.. TetoFrio.Keys];

    [Theory]
    [MemberData(nameof(Rotas))]
    public async Task A_rota_nao_passa_do_seu_orcamento_de_consultas(string rota)
    {
        await CenarioDosCenarios.SemearAsync(api);
        var http = api.ClienteDeRibeirao();

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
