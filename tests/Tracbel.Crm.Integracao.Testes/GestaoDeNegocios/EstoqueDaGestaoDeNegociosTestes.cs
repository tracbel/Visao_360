using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.GestaoDeNegocios;

/// <summary>
/// A LEITURA DO ESTOQUE E DA COBERTURA DA GN (28/09/2026). As amostras têm a forma conferida na API real em 28/09;
/// chassis, pedidos e o cliente são inventados.
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class EstoqueDaGestaoDeNegociosTestes
{
    private const string Estoque = """
        {"painel":"estoque-pedidos","gerado_em":"2026-09-28T11:02:13","idade_segundos":120,"total":2,"pagina":1,"paginas":1,"por_pagina":5000,"linhas":[
          {"qt":1,"chaint":"900001","chassis":"1PY6155MKSS000001","pedido":"4500000001","comar":"1234567","filial":"Ribeirão Preto",
           "sit_equipamento":"Estoque","grupo":"TRATOR 6000","descricao":"TR 6155M","config":"CAB 16X16","maquina_ams_implemento":"MÁQUINA",
           "novo_usado":"NOVO","ano":"2026/2026","dias_estoque":45.0,"cat_dias":"31 a 60 dias","dt_entrada":"2026-08-14","dt_fdd":null,
           "dt_prev_fat":null,"pago":"Sim","reservado":"Não","sit_fabrica":null,"vl_custo":650000.0,"vl_compra":700000.0,
           "cliente_atendimento":"NOME DO CLIENTE"},
          {"qt":1,"chaint":"900002","chassis":null,"pedido":"4500000002","comar":null,"filial":"Barretos","sit_equipamento":"PEDIDO",
           "grupo":"COLHEITADEIRA","descricao":"S 780","config":null,"maquina_ams_implemento":"MÁQUINA","novo_usado":"NOVO","ano":null,
           "dt_entrada":null,"dt_fdd":"2026-11-30","dt_prev_fat":null,"pago":"Não","reservado":"Sim","sit_fabrica":"Confirmado"}
        ]}
        """;

    private const string Cobertura = """
        {"como_e_calculado":"...","gerado_em":"2026-09-28T11:02:13","idade_segundos":744.3,"unidade":"meses de estoque",
         "por_mes":{"itens":[{"chave":"Ago/2026","meses":3.12,"vendas":152},{"chave":"Jul/2026","meses":3.59,"vendas":143}],"media":3.36},
         "por_grupo":{"itens":[{"chave":"AMS","meses":3.55,"vendas":512}],"media":3.55}}
        """;

    private const string Filiais = """
        {"filiais":[{"numero":1,"nome":"Ribeirão Preto","codigo_totvs":"010101"},{"numero":3,"nome":"Barretos","codigo_totvs":"010103"}]}
        """;

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

    private static LeitorDoEstoqueDaGestaoDeNegocios Leitor(List<string> pedidos, string? cobertura = null) =>
        new(new ClienteDaGestaoDeNegocios(
            new FabricaFalsa(new TratadorPorRota(new Dictionary<string, string>
            {
                ["/api/v1/paineis/estoque-pedidos"] = Estoque,
                ["/api/v1/cobertura"] = cobertura ?? Cobertura,
                ["/api/v1/filiais"] = Filiais
            }, pedidos)),
            Options.Create(new OpcoesDaGestaoDeNegocios { Base = "https://gn.exemplo.invalido:5001", Chave = "chave-inventada" }),
            esperaEntreTentativas: TimeSpan.Zero));

    [Fact]
    public async Task As_tres_rotas_viram_o_estoque_sem_custo_nem_cliente()
    {
        var pedidos = new List<string>();

        var leitura = await Leitor(pedidos).LerAsync(CancellationToken.None);

        leitura.EhSucesso.Should().BeTrue(leitura.Erro);
        var e = leitura.Valor;
        e.Equipamentos.Should().HaveCount(2);
        e.Equipamentos[0].Should().Be(new EquipamentoNaOrigem("900001", "1PY6155MKSS000001", "4500000001", "1234567", "Ribeirão Preto", "Estoque",
            "TRATOR 6000", "TR 6155M", "CAB 16X16", "MÁQUINA", "NOVO", "2026/2026", "2026-08-14", null, null, "Sim", "Não", null));
        e.Equipamentos[1].ChegadaPrevistaEm.Should().Be("2026-11-30");
        JsonSerializer.Serialize(e).Should().NotContain("NOME DO CLIENTE").And.NotContain("650000", "custo e cliente não entram no CRM");

        e.Cobertura.Should().Equal(
            new ItemDaCoberturaNaOrigem("MES", "Ago/2026", "3.12", "152"),
            new ItemDaCoberturaNaOrigem("MES", "Jul/2026", "3.59", "143"),
            new ItemDaCoberturaNaOrigem("GRUPO", "AMS", "3.55", "512"));
        e.Filiais.Should().HaveCount(2);
        e.GeradaEmUtc.Should().NotBeNull();
        e.CoberturaGeradaEmUtc.Should().NotBeNull();

        pedidos.Should().Contain(p => p.StartsWith("/api/v1/paineis/estoque-pedidos?pagina=1&", StringComparison.Ordinal),
            "o estoque vai SEM janela de datas: com ela, o painel perde os pedidos à fábrica");
        pedidos.Should().NotContain(p => p.Contains("data_de", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Cobertura_sem_a_lista_de_itens_falha_alto_com_os_nomes_dos_campos()
    {
        var leitura = await Leitor([], """{"gerado_em":"2026-09-28T11:02:13","por_mes":{"meses":[]},"por_grupo":{"itens":[]}}""").LerAsync(CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("O bloco \"por_mes\"").And.Contain("falta a lista \"itens\"").And.Contain("nada foi gravado");
    }

    [Fact]
    public void Linha_do_estoque_sem_campo_obrigatorio_recusa_a_leitura_e_nao_mostra_valor()
    {
        var linhas = JsonDocument.Parse("""[{"chaint":"1","filial":"Barretos","grupo":"AMS","descricao":"X","novo_usado":"NOVO","maquina_ams_implemento":"AMS","pago":"Sim","reservado":"Não","cliente_atendimento":"FULANO"}]""")
            .RootElement.EnumerateArray().ToList();

        var convertido = LeitorDoEstoqueDaGestaoDeNegocios.Converter(linhas, JsonDocument.Parse(Cobertura).RootElement, JsonDocument.Parse(Filiais).RootElement);

        convertido.Erro.Should().Contain("A linha 1").And.Contain("falta o campo \"sit_equipamento\"").And.NotContain("FULANO");
    }

    [Theory]
    [InlineData("Ago/2026", 2026, 8)]
    [InlineData("dez/2025", 2025, 12)]
    [InlineData(" Mar / 2026 ", 2026, 3)]
    public void O_mes_da_cobertura_se_le_em_portugues(string rotulo, int ano, int mes) =>
        LeitorDoEstoqueDaGestaoDeNegocios.Mes(rotulo).Should().Be(new DateOnly(ano, mes, 1));

    [Theory]
    [InlineData("Aug/2026")]
    [InlineData("2026-08")]
    [InlineData(null)]
    public void Mes_que_nao_se_le_e_nulo(string? rotulo) =>
        LeitorDoEstoqueDaGestaoDeNegocios.Mes(rotulo).Should().BeNull();
}
