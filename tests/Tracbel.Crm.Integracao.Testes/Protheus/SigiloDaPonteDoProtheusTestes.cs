using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Protheus;

/// <summary>
/// NENHUMA CREDENCIAL DO PROTHEUS SAI NUMA MENSAGEM DE ERRO (issue [001] do backlog mestre).
///
/// <para>A ponte fala com um ERP de produção. Quando a rede falha, a mensagem da exceção vira texto que
/// a carga imprime e o serviço grava. Estes testes forçam a falha com uma exceção que cita o usuário, a
/// senha e um <c>password=</c> na URL — o pior caso — e conferem que nada disso chega a quem chamou,
/// nem na autenticação nem na leitura de página.</para>
///
/// <para><b>Usuário, senha e endereço aqui são inventados.</b> Nenhum sistema é chamado: o
/// <c>HttpClient</c> usa um tratador falso.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class SigiloDaPonteDoProtheusTestes
{
    private const string Usuario = "leitor_inventado";
    private const string Senha = "Inventad0Forte!";

    private static IOptions<OpcoesDoProtheus> Opcoes() => Options.Create(new OpcoesDoProtheus
    {
        Base = "http://erp.exemplo.invalido/rest",
        Usuario = Usuario,
        Senha = Senha,
        TempoLimiteSegundos = 5
    });

    private sealed class TratadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(responder(pedido));
    }

    private sealed class FabricaFalsa(HttpMessageHandler tratador) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(tratador, disposeHandler: false);
    }

    private static HttpRequestException FalhaQueCitaACredencial() =>
        new($"Falha ao conectar em http://erp.exemplo.invalido/rest/api/oauth2/v1/token" +
            $"?grant_type=password&username={Usuario}&password={Senha} (usuário {Usuario}, senha {Senha})");

    [Fact]
    public async Task A_falha_de_rede_na_autenticacao_nao_devolve_usuario_nem_senha()
    {
        var ponte = new PonteDoProtheus(
            new FabricaFalsa(new TratadorFalso(_ => throw FalhaQueCitaACredencial())), Opcoes());

        var resultado = await ponte.LerPaginaAsync("SD2", "D2_DOC", null, 1, null, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().NotContain(Senha).And.NotContain(Usuario);
        resultado.Erro.Should().Contain("***", "a mensagem continua dizendo o que falhou, sem a credencial");
    }

    [Fact]
    public async Task A_falha_de_rede_na_leitura_da_pagina_nao_devolve_usuario_nem_senha()
    {
        // A autenticação responde; a leitura da página falha nas três tentativas.
        var ponte = new PonteDoProtheus(new FabricaFalsa(new TratadorFalso(pedido =>
            pedido.Method == HttpMethod.Post
                ? new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"access_token":"token-inventado","expires_in":3600}""",
                        Encoding.UTF8, "application/json")
                }
                : throw FalhaQueCitaACredencial())), Opcoes());

        var resultado = await ponte.LerPaginaAsync("SD2", "D2_DOC", null, 1, null, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().NotContain(Senha).And.NotContain(Usuario);
        resultado.Erro.Should().Contain("password=***");
    }

    [Fact]
    public async Task A_senha_nao_vai_na_url_do_pedido_de_token()
    {
        // A documentação do fornecedor manda a senha na query string. A ponte manda no corpo — e este
        // teste impede que alguém "simplifique" de volta.
        Uri? enderecoDoToken = null;
        string? corpoDoToken = null;

        var ponte = new PonteDoProtheus(new FabricaFalsa(new TratadorFalso(pedido =>
        {
            if (pedido.Method == HttpMethod.Post)
            {
                enderecoDoToken = pedido.RequestUri;
                corpoDoToken = pedido.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        })), Opcoes());

        await ponte.LerPaginaAsync("SD2", "D2_DOC", null, 1, null, CancellationToken.None);

        enderecoDoToken.Should().NotBeNull();
        enderecoDoToken!.Query.Should().NotContain(Senha).And.NotContain("password");
        corpoDoToken.Should().Contain("password=");
    }
}
