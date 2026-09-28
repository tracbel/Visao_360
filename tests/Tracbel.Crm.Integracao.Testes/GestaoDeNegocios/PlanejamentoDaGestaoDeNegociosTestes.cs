using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.GestaoDeNegocios;

/// <summary>
/// A LEITURA DO PLANEJAMENTO DA GN (28/09/2026) — o de-para de consultores, o forecast, a performance de consórcio e o
/// de-para das lojas. As amostras têm a forma conferida na API real em 28/09; nomes e números são inventados.
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class PlanejamentoDaGestaoDeNegociosTestes
{
    private const string Time = """
        {"cadastro":"de_para_consultores","gerado_em":"2026-09-28T01:30:00","total":2,"pagina":1,"paginas":1,"por_pagina":500,"linhas":[
          {"id":1,"consultor":"FULANO.DE.TAL","capitao":"GESTOR.NORTE","filial_numero":5,"inicio_vigencia":"2025-11-01","relatorio_consorcio":"FULANO"},
          {"id":2,"consultor":"BELTRANA.SILVA","capitao":"GESTOR.SUL","filial_numero":"01","inicio_vigencia":null}
        ]}
        """;

    private const string Forecast = """
        {"cadastro":"forecast","gerado_em":"2026-09-28T01:30:00","total":2,"pagina":1,"paginas":1,"por_pagina":500,"linhas":[
          {"id":10,"mes":"2026-09-01","gestor":"GESTOR.NORTE","linha":"TRATOR MÉDIO","forecast":5,"best_guess":6,"observacao":"cliente X deve fechar"},
          {"id":11,"mes":"2026-10-01","gestor":"GESTOR.SUL","linha":"PULVERIZADOR","forecast":null,"best_guess":2,"observacao":null}
        ]}
        """;

    private const string Consorcio = """
        {"painel":"performance-consorcio","gerado_em":"2026-09-28T01:30:00","idade_segundos":120,"total":3,"pagina":1,"paginas":1,"por_pagina":500,"linhas":[
          {"tipo":"Meta","mes_rotulo":"Ago/2026","filial":"ITUVERAVA","vendedor":"FULANO.DE.TAL","gestor":"GESTOR.NORTE","quantidade":3},
          {"tipo":"Realizado","mes_rotulo":"Set/2026","cota_grupo":"1000","cota":"1","filial":"ITUVERAVA","vendedor":"FULANO.DE.TAL",
           "gestor":"GESTOR.NORTE","contemplacao":"Lance","dt_alocacao":"2026-09-10","dt_contemplacao":null,"produto":"TRATOR 6M",
           "valor":250000.0,"valor_parcela":3100.5,"consorciado":"NOME DO CLIENTE"},
          {"tipo":"Realizado","mes_rotulo":"Set/2026","cota_grupo":1001,"cota":7,"filial":"DIGITAL","vendedor":"BELTRANA.SILVA",
           "gestor":"GESTOR.SUL","contemplacao":"Não Contemplado","dt_alocacao":"2026-09-02"}
        ]}
        """;

    private const string Filiais = """
        {"filiais":[{"numero":1,"nome":"RIBEIRAO PRETO","codigo_totvs":"010101"},{"numero":0,"nome":"DIGITAL","codigo_totvs":null}]}
        """;

    /// <summary>Responde cada rota com o seu corpo, e anota o que foi pedido.</summary>
    private sealed class TratadorPorRota(IReadOnlyDictionary<string, string> corpos, List<string> pedidos) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct)
        {
            pedidos.Add(pedido.RequestUri!.PathAndQuery);
            var corpo = corpos.First(c => pedido.RequestUri.AbsolutePath == c.Key).Value;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(corpo, Encoding.UTF8, "application/json") });
        }
    }

    private sealed class FabricaFalsa(HttpMessageHandler tratador) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(tratador, disposeHandler: false);
    }

    private static LeitorDoPlanejamentoDaGestaoDeNegocios Leitor(List<string> pedidos, string? consorcio = null) =>
        new(new ClienteDaGestaoDeNegocios(
            new FabricaFalsa(new TratadorPorRota(new Dictionary<string, string>
            {
                ["/api/v1/cadastros/de_para_consultores"] = Time,
                ["/api/v1/cadastros/forecast"] = Forecast,
                ["/api/v1/paineis/performance-consorcio"] = consorcio ?? Consorcio,
                ["/api/v1/filiais"] = Filiais
            }, pedidos)),
            Options.Create(new OpcoesDaGestaoDeNegocios { Base = "https://gn.exemplo.invalido:5001", Chave = "chave-inventada" }),
            esperaEntreTentativas: TimeSpan.Zero));

    private static List<JsonElement> Linhas(string json) => [.. JsonDocument.Parse(json).RootElement.EnumerateArray()];

    [Fact]
    public async Task As_quatro_rotas_viram_o_planejamento_com_tudo_como_texto()
    {
        var pedidos = new List<string>();

        var leitura = await Leitor(pedidos).LerAsync(CancellationToken.None);

        leitura.EhSucesso.Should().BeTrue(leitura.Erro);
        var p = leitura.Valor;
        p.Time.Should().Equal(
            new ConsultorNaGestao(1, "FULANO.DE.TAL", "GESTOR.NORTE", "5", "2025-11-01"),
            new ConsultorNaGestao(2, "BELTRANA.SILVA", "GESTOR.SUL", "01", null));
        p.Forecast.Should().Equal(
            new ForecastNaOrigem(10, "2026-09-01", "GESTOR.NORTE", "TRATOR MÉDIO", "5", "6"),
            new ForecastNaOrigem(11, "2026-10-01", "GESTOR.SUL", "PULVERIZADOR", null, "2"));
        p.Cotas.Should().HaveCount(2, "só as linhas \"Realizado\" são cotas vendidas");
        p.Cotas[0].Should().Be(new CotaNaOrigem("1000", "1", "Set/2026", "ITUVERAVA", "FULANO.DE.TAL", "GESTOR.NORTE", "Lance",
            "2026-09-10", null, "TRATOR 6M", "250000.0", "3100.5"));
        (p.Cotas[1].Grupo, p.Cotas[1].Cota).Should().Be(("1001", "7"), "grupo e cota numéricos viram o mesmo texto");
        p.MesesDaPerformance.Should().BeEquivalentTo([new DateOnly(2026, 8, 1), new DateOnly(2026, 9, 1)],
            "a janela é a de TODAS as linhas — a meta de agosto diz que agosto está coberto, mesmo sem cota vendida");
        p.Filiais.Should().Equal(new FilialDaGestao(1, "RIBEIRAO PRETO", "010101"), new FilialDaGestao(0, "DIGITAL", null));
        p.GeradaEmUtc.Should().NotBeNull("é a data do forecast");

        pedidos.Should().Contain(r => r.StartsWith("/api/v1/paineis/performance-consorcio?data_de=2000-01-01&data_ate=2100-12-31&pagina=1&", StringComparison.Ordinal),
            "o painel tem período padrão: a janela larga vai sempre, e a paginação entra depois dela com &");
        pedidos.Should().Contain("/api/v1/filiais", "o de-para das lojas não é paginado");
    }

    [Fact]
    public void Nem_a_observacao_do_gestor_nem_o_consorciado_sao_lidos()
    {
        var convertido = LeitorDoPlanejamentoDaGestaoDeNegocios.Converter(
            Linhas(JsonDocument.Parse(Time).RootElement.GetProperty("linhas").GetRawText()),
            Linhas(JsonDocument.Parse(Forecast).RootElement.GetProperty("linhas").GetRawText()),
            Linhas(JsonDocument.Parse(Consorcio).RootElement.GetProperty("linhas").GetRawText()),
            JsonDocument.Parse(Filiais).RootElement);

        convertido.EhSucesso.Should().BeTrue(convertido.Erro);
        JsonSerializer.Serialize(convertido.Valor).Should().NotContain("cliente X").And.NotContain("NOME DO CLIENTE",
            "texto livre do gestor e nome do consorciado não entram no CRM por aqui");
    }

    [Fact]
    public async Task Campo_obrigatorio_ausente_na_cota_falha_alto_com_os_nomes_e_sem_os_valores()
    {
        var pedidos = new List<string>();
        var semGrupo = Consorcio.Replace("\"cota_grupo\":\"1000\",", "", StringComparison.Ordinal);

        var leitura = await Leitor(pedidos, semGrupo).LerAsync(CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("linha 2").And.Contain("falta o campo \"cota_grupo\"").And.Contain("Campos recebidos: tipo, mes_rotulo, cota")
            .And.Contain("nada foi gravado");
        leitura.Erro.Should().NotContain("FULANO").And.NotContain("NOME DO CLIENTE", "o erro diz os nomes dos campos, nunca os valores");
    }

    [Fact]
    public void Id_do_de_para_que_nao_e_inteiro_recusa_a_leitura()
    {
        var convertido = LeitorDoPlanejamentoDaGestaoDeNegocios.Converter(
            Linhas("""[{"id":"a1","consultor":"A.B","capitao":"C.D"}]"""), [], [], JsonDocument.Parse(Filiais).RootElement);

        convertido.Erro.Should().Contain(LeitorDoPlanejamentoDaGestaoDeNegocios.RotaDoTime).And.Contain("o id não é um número inteiro");
    }

    [Fact]
    public void Filiais_sem_a_lista_recusam_a_leitura() =>
        LeitorDoPlanejamentoDaGestaoDeNegocios.Converter([], [], [], JsonDocument.Parse("""{"lojas":[]}""").RootElement)
            .Erro.Should().Contain("não trouxe a lista \"filiais\"");

    [Theory]
    [InlineData("Set/2026", 2026, 9)]
    [InlineData("set/2026", 2026, 9)]
    [InlineData(" Jan / 2027 ", 2027, 1)]
    [InlineData("Dez/2025", 2025, 12)]
    [InlineData("Fev/2026", 2026, 2)]
    public void O_mes_do_painel_se_le_em_portugues(string rotulo, int ano, int mes) =>
        LeitorDoPlanejamentoDaGestaoDeNegocios.Mes(rotulo).Should().Be(new DateOnly(ano, mes, 1));

    [Theory]
    [InlineData("Sep/2026")]
    [InlineData("09/2026")]
    [InlineData("Set/26")]
    [InlineData("Set")]
    [InlineData("")]
    [InlineData(null)]
    public void Mes_do_painel_que_nao_se_le_e_nulo(string? rotulo) =>
        LeitorDoPlanejamentoDaGestaoDeNegocios.Mes(rotulo).Should().BeNull();
}
