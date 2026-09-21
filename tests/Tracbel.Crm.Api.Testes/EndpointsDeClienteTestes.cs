using Tracbel.Crm.Dominio.Seguranca;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O CONTRATO DA API, por HTTP de verdade: rota, código de status e formato de erro.
///
/// O que estes testes protegem é o que o front vai consumir. Um caso de uso pode estar perfeito e
/// a rota devolver 500 mesmo assim — foi exatamente o que aconteceu com o corpo do <c>DELETE</c>,
/// que o ASP.NET Core não infere e que nenhum teste de caso de uso pegaria.
/// </summary>
[Trait("Categoria", "Api")]
public sealed class EndpointsDeClienteTestes(ApiEmMemoria api, ITestOutputHelper saida)
    : IClassFixture<ApiEmMemoria>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static object ClienteValido(string nome, string? documento = null) => new
    {
        nomeRazao = nome,
        tipoDePessoa = "Juridica",
        documento,
        situacao = "Prospect",
        origemCodigo = "INDICACAO"
    };

    private static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();

    // ------------------------------------------------------------------ o caminho feliz

    [Fact]
    public async Task Criar_devolve_201_com_Location_e_a_chave_publica()
    {
        var http = api.ClienteDeRibeirao();

        var resposta = await http.PostAsJsonAsync(
            "/api/v1/clientes", ClienteValido("Fazenda do 201 Ltda"), Json);

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);

        var corpo = await CorpoAsync(resposta);
        var chave = corpo.GetProperty("chave").GetGuid();

        // O Location aponta para a CHAVE PÚBLICA, nunca para o identificador interno: quem
        // enumera /clientes/1, /clientes/2 conta quantos clientes a Tracbel tem.
        resposta.Headers.Location!.ToString().Should().Be($"/api/v1/clientes/{chave}");

        saida.WriteLine($"201 Created → Location: {resposta.Headers.Location}");
    }

    [Fact]
    public async Task Listar_devolve_200_com_a_pagina_e_a_procedencia()
    {
        var http = api.ClienteDeRibeirao();
        await http.PostAsJsonAsync("/api/v1/clientes", ClienteValido("Fazenda da Listagem Ltda"), Json);

        var resposta = await http.GetAsync("/api/v1/clientes?tamanho=5&ordenarPor=Nome");
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        var corpo = await CorpoAsync(resposta);

        corpo.GetProperty("dados").GetProperty("total").GetInt32().Should().BeGreaterThan(0);

        // TODA LEITURA CARREGA DE ONDE VEIO. Vale para o nosso banco e para a ponte do legado, no
        // mesmo formato — é o que permite o front marcar a origem na tela sem saber por qual rota
        // o dado chegou.
        var procedencia = corpo.GetProperty("procedencia");
        procedencia.GetProperty("sistema").GetString().Should().Be("CRM Tracbel");
        procedencia.GetProperty("objeto").GetString().Should().Be("comercial.Cliente");
        procedencia.GetProperty("lidoEmUtc").GetDateTime().Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));

        saida.WriteLine(procedencia.ToString());
    }

    [Fact]
    public async Task O_ciclo_completo_criar_obter_alterar_e_inativar_funciona_por_HTTP()
    {
        // INATIVAR EXIGE O PERFIL DE EXCLUSÃO desde a fase 3: o padrão é o mínimo, sem excluir (Q-P2).
        await api.ConcederPerfilAsync(100, PerfisDeSistema.ExclusaoDeCadastro);
        var http = api.ClienteDeRibeirao();

        var criacao = await http.PostAsJsonAsync(
            "/api/v1/clientes", ClienteValido("Fazenda do Ciclo Ltda", "11.222.333/0001-81"), Json);
        criacao.StatusCode.Should().Be(HttpStatusCode.Created);
        var chave = (await CorpoAsync(criacao)).GetProperty("chave").GetGuid();

        var leitura = await http.GetAsync($"/api/v1/clientes/{chave}");
        leitura.StatusCode.Should().Be(HttpStatusCode.OK);
        (await CorpoAsync(leitura)).GetProperty("dados").GetProperty("nomeRazao").GetString()
            .Should().Be("Fazenda do Ciclo Ltda");

        var alteracao = await http.PutAsJsonAsync($"/api/v1/clientes/{chave}", new
        {
            nomeRazao = "Fazenda do Ciclo Agropecuária Ltda",
            tipoDePessoa = "Juridica",
            documento = "11.222.333/0001-81",
            situacao = "Cliente"
        }, Json);
        alteracao.StatusCode.Should().Be(HttpStatusCode.OK);
        (await CorpoAsync(alteracao)).GetProperty("situacao").GetString().Should().Be("Cliente");

        // DELETE COM CORPO: o motivo é obrigatório e vem de catálogo.
        var inativacao = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/clientes/{chave}")
        {
            Content = JsonContent.Create(new { motivoCodigo = "DUPLICADO" }, options: Json)
        };
        var baixa = await http.SendAsync(inativacao);

        baixa.StatusCode.Should().Be(HttpStatusCode.OK);
        var corpoDaBaixa = await CorpoAsync(baixa);
        corpoDaBaixa.GetProperty("estaInativo").GetBoolean().Should().BeTrue();
        corpoDaBaixa.GetProperty("motivoInativacaoCodigo").GetString().Should().Be("DUPLICADO");

        // NADA FOI APAGADO: continua respondendo no GET, marcado como inativo.
        var depois = await http.GetAsync($"/api/v1/clientes/{chave}");
        depois.StatusCode.Should().Be(HttpStatusCode.OK);
        (await CorpoAsync(depois)).GetProperty("dados").GetProperty("estaInativo").GetBoolean()
            .Should().BeTrue();

        saida.WriteLine("criar → obter → alterar → inativar: todos com o status esperado");
    }

    // ------------------------------------------------------------------ o formato de erro

    [Fact]
    public async Task Entrada_invalida_devolve_422_em_problem_json_com_a_lista_de_erros_por_campo()
    {
        var http = api.ClienteDeRibeirao();

        var resposta = await http.PostAsJsonAsync("/api/v1/clientes", new
        {
            nomeRazao = "   ",
            tipoDePessoa = "Juridica",
            documento = "529.982.247-25",
            origemCodigo = "VEIO_DO_NADA"
        }, Json);

        // 422 E NÃO 400: o 400 diz "não entendi a requisição"; aqui ela foi entendida
        // perfeitamente e o CONTEÚDO é que não passa. A distinção importa para quem depura.
        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        resposta.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var corpo = await CorpoAsync(resposta);
        corpo.GetProperty("type").GetString().Should().EndWith("/validacao");
        corpo.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();

        var erros = corpo.GetProperty("erros").EnumerateArray().ToList();
        erros.Select(e => e.GetProperty("campo").GetString())
            .Should().BeEquivalentTo(["nomeRazao", "documento", "origemCodigo"]);

        // Cada erro diz o que corrigir E o que chegou. [V] "An error has occurred" é o que a API
        // do legado devolve para tudo.
        erros.Should().AllSatisfy(e =>
        {
            e.GetProperty("mensagem").GetString().Should().NotBeNullOrWhiteSpace();
            e.TryGetProperty("valorRecebido", out _).Should().BeTrue();
        });

        saida.WriteLine(corpo.ToString());
    }

    [Fact]
    public async Task Registro_que_nao_existe_devolve_404_em_problem_json()
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{Guid.NewGuid()}");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await CorpoAsync(resposta)).GetProperty("type").GetString().Should().EndWith("/nao-encontrado");
    }

    [Fact]
    public async Task Documento_repetido_devolve_409_com_o_campo_apontado()
    {
        var http = api.ClienteDeRibeirao();

        await http.PostAsJsonAsync("/api/v1/clientes", ClienteValido("Fazenda 409 Primeira", "04.252.011/0001-10"), Json);
        var segunda = await http.PostAsJsonAsync("/api/v1/clientes", ClienteValido("Fazenda 409 Segunda", "04252011000110"), Json);

        segunda.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var corpo = await CorpoAsync(segunda);
        corpo.GetProperty("type").GetString().Should().EndWith("/conflito");
        corpo.GetProperty("erros")[0].GetProperty("campo").GetString().Should().Be("documento");
        corpo.GetProperty("erros")[0].GetProperty("mensagem").GetString()
            .Should().Contain("Fazenda 409 Primeira", "a recusa diz QUEM já usa o documento");

        saida.WriteLine(corpo.GetProperty("erros")[0].ToString());
    }

    // ------------------------------------------------------------------ o contexto de acesso

    [Fact]
    public async Task Requisicao_com_usuario_desconhecido_e_recusada_no_MESMO_formato_de_erro()
    {
        var http = api.ClienteComo("ninguem@tracbel.com.br", ApiEmMemoria.FilialDeRibeirao);

        var resposta = await http.GetAsync("/api/v1/clientes");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var corpo = await CorpoAsync(resposta);

        // O MESMO FORMATO do resto da API. Um formato próprio aqui obrigaria o front a ter dois
        // tratamentos de erro.
        corpo.GetProperty("erros")[0].GetProperty("campo").GetString().Should().Be("X-Tracbel-Usuario");

        saida.WriteLine(corpo.ToString());
    }

    [Fact]
    public async Task Requisicao_com_filial_que_nao_existe_e_recusada_apontando_o_cabecalho()
    {
        var http = api.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, "999999");

        var resposta = await http.GetAsync("/api/v1/clientes");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await CorpoAsync(resposta)).GetProperty("erros")[0].GetProperty("campo").GetString()
            .Should().Be("X-Tracbel-Empresa");
    }

    // ------------------------------------------------------------------ catálogos

    [Fact]
    public async Task Os_catalogos_respondem_e_o_codigo_inexistente_devolve_404()
    {
        var http = api.ClienteDeRibeirao();

        var lista = await http.GetAsync("/api/v1/catalogos/ORIGEM_LEAD");
        lista.StatusCode.Should().Be(HttpStatusCode.OK);

        var corpo = await CorpoAsync(lista);
        corpo.GetProperty("dados")[0].GetProperty("codigo").GetString().Should().Be("ORIGEM_LEAD");
        corpo.GetProperty("dados")[0].GetProperty("itens").GetArrayLength().Should().BeGreaterThan(0);
        corpo.GetProperty("procedencia").GetProperty("sistema").GetString().Should().Be("CRM Tracbel");

        var inexistente = await http.GetAsync("/api/v1/catalogos/NAO_EXISTE");
        inexistente.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ------------------------------------------------------------------ a ponte do legado

    [Fact]
    public async Task A_ponte_do_legado_devolve_503_e_o_CADASTRO_CONTINUA_DE_PE()
    {
        // Nenhum teste configura Vortice__Conexao, então a ponte está "não configurada" — que é o
        // mesmo caminho de resposta da VPN fora do ar. É o requisito central: uma dependência
        // externa caída NÃO pode derrubar o cadastro.
        var http = api.ClienteDeRibeirao();

        foreach (var rota in new[]
                 {
                     "/api/v1/legado/saude",
                     "/api/v1/legado/clientes?termo=santa",
                     "/api/v1/legado/clientes/26192/parque"
                 })
        {
            var resposta = await http.GetAsync(rota);

            resposta.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable, $"em {rota}");

            var corpo = await CorpoAsync(resposta);
            corpo.GetProperty("type").GetString().Should().EndWith("/dependencia-indisponivel");
            corpo.GetProperty("title").GetString().Should().Contain("continua funcionando");

            saida.WriteLine($"{rota} → 503");
        }

        // E o resto responde normalmente, na mesma instância, no mesmo instante.
        (await http.GetAsync("/api/v1/clientes")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await http.GetAsync("/api/v1/catalogos")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await http.GetAsync("/api/v1/equipamentos")).StatusCode.Should().Be(HttpStatusCode.OK);

        saida.WriteLine("clientes, catálogos e equipamentos: 200 com a ponte caída");
    }
}
