using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.GestaoDeNegocios;

/// <summary>
/// O CLIENTE DA API GESTÃO DE NEGÓCIOS, contra um tratador falso — nenhuma chamada sai do processo.
///
/// <para>O que se prende: a chave vai no cabeçalho <c>Authorization: Bearer</c> e nunca na URL nem numa mensagem; a
/// leitura inteira só volta se FECHAR (total estável, linhas somando o total); a chave recusada — 401, 403, o
/// redirecionamento para a tela de login ou a própria tela de login em HTML — é dita como chave recusada, e não como
/// "página vazia"; o erro de certificado manda usar o nome, e nada é desligado.</para>
///
/// <para><b>Endereço e chave são inventados.</b></para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class ClienteDaGestaoDeNegociosTestes
{
    private const string Chave = "chave-inventada-para-o-teste-0123456789";
    private const string Base = "https://gn.exemplo.invalido:5001";
    private const string Rota = "/api/v1/cadastros/metas";

    private sealed class TratadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Pedidos { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct)
        {
            Pedidos.Add(pedido);
            return Task.FromResult(responder(pedido));
        }
    }

    private sealed class FabricaFalsa(HttpMessageHandler tratador) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(tratador, disposeHandler: false);
    }

    private static ClienteDaGestaoDeNegocios Cliente(TratadorFalso tratador, string @base = Base, int porPagina = 2) =>
        new(new FabricaFalsa(tratador),
            Options.Create(new OpcoesDaGestaoDeNegocios { Base = @base, Chave = Chave, TamanhoDaPagina = porPagina, TempoLimiteSegundos = 5 }),
            esperaEntreTentativas: TimeSpan.Zero);

    private static HttpResponseMessage Json(string corpo, HttpStatusCode codigo = HttpStatusCode.OK) =>
        new(codigo) { Content = new StringContent(corpo, Encoding.UTF8, "application/json") };

    private static string Pagina(int pagina, int paginas, int total, params int[] ids) =>
        $$"""
        {"cadastro":"metas","tipo":"cadastro","rotulo":"Metas","gerado_em":"2026-09-27T01:30:00","total":{{total}},
         "pagina":{{pagina}},"paginas":{{paginas}},"por_pagina":2,
         "linhas":[{{string.Join(",", ids.Select(i => $$"""{"id":{{i}}}"""))}}]}
        """;

    private static int PaginaPedida(HttpRequestMessage pedido) =>
        int.Parse(pedido.RequestUri!.Query.Split('&')[0].Split('=')[1], System.Globalization.CultureInfo.InvariantCulture);

    [Fact]
    public async Task Le_todas_as_paginas_com_a_chave_no_cabecalho_e_nunca_na_url()
    {
        var tratador = new TratadorFalso(pedido => PaginaPedida(pedido) switch
        {
            1 => Json(Pagina(1, 3, 5, 1, 2)),
            2 => Json(Pagina(2, 3, 5, 3, 4)),
            _ => Json(Pagina(3, 3, 5, 5))
        });

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeTrue(leitura.Erro);
        leitura.Valor.Linhas.Select(l => l.GetProperty("id").GetInt32()).Should().Equal(1, 2, 3, 4, 5);
        leitura.Valor.Total.Should().Be(5);
        leitura.Valor.Paginas.Should().Be(3);
        leitura.Valor.IdadeSegundos.Should().BeNull("o cadastro de metas não traz idade_segundos");
        leitura.Valor.GeradaEmUtc.Should().Be(new DateTime(2026, 9, 27, 4, 30, 0, DateTimeKind.Utc), "sem fuso, vale São Paulo (UTC−3)");

        tratador.Pedidos.Should().HaveCount(3);
        tratador.Pedidos.Should().AllSatisfy(p =>
        {
            p.Method.Should().Be(HttpMethod.Get, "o cliente só lê");
            p.Headers.Authorization!.Scheme.Should().Be("Bearer");
            p.Headers.Authorization.Parameter.Should().Be(Chave);
            p.RequestUri!.ToString().Should().NotContain(Chave).And.StartWith($"{Base}{Rota}?pagina=");
            p.RequestUri.Query.Should().Contain("por_pagina=2");
        });
    }

    [Fact]
    public async Task O_total_que_muda_no_meio_da_leitura_derruba_a_leitura_inteira()
    {
        var tratador = new TratadorFalso(pedido => PaginaPedida(pedido) == 1 ? Json(Pagina(1, 2, 4, 1, 2)) : Json(Pagina(2, 3, 5, 3, 4)));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("mudou durante a leitura").And.Contain("nada foi gravado");
    }

    [Theory]
    [InlineData(1_000_000, 2)]
    [InlineData(4, 500_000)]
    public async Task Total_ou_paginas_acima_do_teto_derrubam_a_leitura_sem_pedir_a_segunda_pagina(int total, int paginas)
    {
        var tratador = new TratadorFalso(_ => Json(Pagina(1, paginas, total, 1, 2)));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("fora do teto").And.Contain("nada foi gravado");
        tratador.Pedidos.Should().ContainSingle("o envelope absurdo não vira uma fila de páginas");
    }

    [Fact]
    public async Task Linhas_que_nao_somam_o_total_nao_sao_devolvidas()
    {
        var tratador = new TratadorFalso(pedido => PaginaPedida(pedido) == 1 ? Json(Pagina(1, 2, 4, 1, 2)) : Json(Pagina(2, 2, 4, 3)));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("não fecha").And.Contain("declarou 4").And.Contain("entregou 3");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Found)]
    public async Task A_chave_recusada_e_dita_como_chave_recusada_sem_citar_a_chave(HttpStatusCode codigo)
    {
        var tratador = new TratadorFalso(_ => new HttpResponseMessage(codigo) { Headers = { Location = new Uri("/entrar", UriKind.Relative) } });

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("recusou a chave").And.NotContain(Chave);
        tratador.Pedidos.Should().HaveCount(1, "chave recusada não se repete");
    }

    [Fact]
    public async Task A_tela_de_login_em_html_nao_vira_pagina_vazia()
    {
        var tratador = new TratadorFalso(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html>Redirecting... /entrar</html>", Encoding.UTF8, "text/html")
        });

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("tela de login").And.Contain("não JSON");
    }

    [Fact]
    public async Task O_erro_de_certificado_manda_usar_o_nome_e_nao_se_repete()
    {
        var tratador = new TratadorFalso(_ => throw new HttpRequestException(
            "The SSL connection could not be established", new AuthenticationException("RemoteCertificateNameMismatch")));

        var leitura = await Cliente(tratador, "https://10.0.0.1:5001").LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("use o NOME").And.Contain(ClienteDaGestaoDeNegocios.EnderecoPeloNome).And.Contain("não é desligada");
        tratador.Pedidos.Should().HaveCount(1);
    }

    [Fact]
    public async Task A_falha_de_rede_que_cita_a_chave_sai_mascarada()
    {
        var tratador = new TratadorFalso(_ => throw new HttpRequestException($"Falha ao conectar (Authorization: Bearer {Chave})"));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().NotContain(Chave).And.Contain("***");
        tratador.Pedidos.Should().HaveCount(3, "falha de rede é intermitente: três tentativas");
    }

    [Fact]
    public async Task Endereco_http_puro_e_recusado_sem_chamar_nada()
    {
        var tratador = new TratadorFalso(_ => Json(Pagina(1, 1, 0)));

        var leitura = await Cliente(tratador, "http://gn.exemplo.invalido:5001").LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("https://");
        tratador.Pedidos.Should().BeEmpty("a chave não vai em claro pela rede");
    }

    [Fact]
    public async Task Envelope_sem_os_campos_de_paginacao_falha_alto()
    {
        var tratador = new TratadorFalso(_ => Json("""{"cadastro":"metas","dados":[{"id":1}]}"""));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("não tem o envelope esperado").And.Contain("total").And.Contain("linhas");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // A #41: o intermitente se repete e se recupera; o definitivo não se repete; o tempo esgotado e o cancelamento são
    // coisas diferentes; e o envelope aceita número vindo como texto, que é o que "tipos trocados" pede.
    // -----------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task O_503_se_repete_e_a_leitura_se_recupera_na_terceira_tentativa()
    {
        var chamadas = 0;
        var tratador = new TratadorFalso(_ => ++chamadas < 3
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : Json(Pagina(1, 1, 2, 1, 2)));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeTrue(leitura.Erro);
        leitura.Valor.Linhas.Should().HaveCount(2);
        tratador.Pedidos.Should().HaveCount(3, "duas respostas 503 e a terceira, que entregou");
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task O_intermitente_que_nao_passa_esgota_as_tres_tentativas_e_diz_o_codigo(HttpStatusCode codigo)
    {
        var tratador = new TratadorFalso(_ => new HttpResponseMessage(codigo));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain($"HTTP {(int)codigo}").And.NotContain(Chave);
        tratador.Pedidos.Should().HaveCount(3);
    }

    [Fact]
    public async Task O_404_nao_se_repete_a_rota_nao_vai_aparecer_na_segunda_tentativa()
    {
        var tratador = new TratadorFalso(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("HTTP 404").And.Contain(Rota);
        tratador.Pedidos.Should().ContainSingle();
    }

    [Fact]
    public async Task O_tempo_esgotado_se_repete_e_depois_diz_o_limite()
    {
        // O TEMPO ESGOTADO DO HttpClient é um TaskCanceledException com o token de quem chamou INTACTO — é assim que ele
        // se distingue do cancelamento de verdade.
        var tratador = new TratadorFalso(_ => throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout"));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("não respondeu").And.Contain("em 5s");
        tratador.Pedidos.Should().HaveCount(3);
    }

    [Fact]
    public async Task O_cancelamento_de_quem_chamou_sobe_e_nao_vira_indisponivel()
    {
        // CANCELAR NÃO É FALHA DA API: quem cancelou (o orquestrador parando, o serviço desligando) recebe o cancelamento,
        // e nenhuma tentativa a mais é feita.
        using var cancelamento = new CancellationTokenSource();
        var tratador = new TratadorFalso(_ =>
        {
            cancelamento.Cancel();
            throw new OperationCanceledException(cancelamento.Token);
        });

        var ler = () => Cliente(tratador).LerTudoAsync<JsonElement>(Rota, cancelamento.Token);

        await ler.Should().ThrowAsync<OperationCanceledException>();
        tratador.Pedidos.Should().ContainSingle();
    }

    [Fact]
    public async Task Numero_que_chega_como_texto_no_envelope_e_lido_e_nao_zerado()
    {
        // TIPOS TROCADOS: o mesmo campo vem como número num painel e como texto noutro (medido no doc 46 §5). O envelope
        // lê os dois; o que não for número de jeito nenhum continua falhando alto (Envelope_sem_os_campos…).
        var tratador = new TratadorFalso(_ => Json(
            """{"cadastro":"metas","total":"2","pagina":"1","paginas":"1","por_pagina":"2","linhas":[{"id":"1"},{"id":2}]}"""));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeTrue(leitura.Erro);
        leitura.Valor.Total.Should().Be(2);
        leitura.Valor.Linhas.Select(l => l.GetProperty("id").ValueKind).Should().Equal(JsonValueKind.String, JsonValueKind.Number);
    }

    [Theory]
    [InlineData("null", null)]
    [InlineData("75600.4", 75600)]
    public async Task A_idade_do_espelho_e_guardada_como_veio_e_nula_quando_nao_vem(string idade, int? esperada)
    {
        var tratador = new TratadorFalso(_ => Json(
            $$"""{"cadastro":"funil","tipo":"painel","total":1,"pagina":1,"paginas":1,"idade_segundos":{{idade}},"linhas":[{"id":1}]}"""));

        var leitura = await Cliente(tratador).LerTudoAsync<JsonElement>(Rota, CancellationToken.None);

        leitura.EhSucesso.Should().BeTrue(leitura.Erro);
        leitura.Valor.IdadeSegundos.Should().Be(esperada);
        leitura.Valor.GeradaEmUtc.Should().BeNull("sem gerado_em, o carimbo fica vazio — e a leitura continua valendo");
    }

    [Theory]
    [InlineData("2026-09-27T01:30:00-03:00", "2026-09-27T04:30:00")]
    [InlineData("2026-09-27T04:30:00Z", "2026-09-27T04:30:00")]
    [InlineData("2026-09-27 01:30:00", "2026-09-27T04:30:00")]
    public void O_gerado_em_vira_utc_com_o_fuso_que_tiver_ou_o_de_sao_paulo(string bruto, string esperado)
    {
        var elemento = JsonDocument.Parse($"\"{bruto}\"").RootElement;

        ClienteDaGestaoDeNegocios.InstanteUtc(elemento).Should()
            .Be(DateTime.SpecifyKind(DateTime.Parse(esperado, System.Globalization.CultureInfo.InvariantCulture), DateTimeKind.Utc));
        ClienteDaGestaoDeNegocios.InstanteUtc(JsonDocument.Parse("\"ontem\"").RootElement).Should().BeNull();
    }
}
