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
    /// A segunda leitura, com o cache de referência cheio — o medido depois de cada etapa do documento 54. Em consultas ao
    /// banco por chamada:
    ///
    /// <code>
    /// rota              linha de base (03/10)   plano 1 (cache de referência)   plano 2 (estrutura de referência)
    /// indicadores                73                       58                              43
    /// demanda                    81                       29                              29
    /// diagnóstico                77                       62                              47
    /// cenários                   87                       35                              35
    /// dimensionamento            24                       24                              24
    /// financiamentos             16                       16                              16
    /// preços                     16                       16                              16
    /// </code>
    ///
    /// <para>Neste cenário não há Censo nem rebanho, e metade das leituras da estrutura já era pulada: em produção, com as
    /// fontes carregadas, a estrutura de referência tira perto de 31 consultas dos Indicadores e do Diagnóstico.</para>
    /// </summary>
    private static readonly Dictionary<string, int> TetoQuente = new(StringComparer.Ordinal)
    {
        ["/api/v1/territorio/indicadores"] = 43,
        ["/api/v1/mercado/demanda"] = 29,
        ["/api/v1/mercado/diagnostico"] = 47,
        ["/api/v1/mercado/cenarios"] = 35,
        ["/api/v1/mercado/dimensionamento"] = 24,
        ["/api/v1/mercado/financiamentos"] = 16,
        ["/api/v1/mercado/precos"] = 16,
    };

    /// <summary>
    /// A primeira leitura, com o cache de referência vazio: a assinatura de cada assunto (uma consulta, plano 2) e a conta de
    /// cada leitor de referência, uma vez para todas as telas — o medido.
    /// </summary>
    private static readonly Dictionary<string, int> TetoFrio = new(StringComparer.Ordinal)
    {
        ["/api/v1/territorio/indicadores"] = 72,
        ["/api/v1/mercado/demanda"] = 47,
        ["/api/v1/mercado/diagnostico"] = 76,
        ["/api/v1/mercado/cenarios"] = 53,
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
