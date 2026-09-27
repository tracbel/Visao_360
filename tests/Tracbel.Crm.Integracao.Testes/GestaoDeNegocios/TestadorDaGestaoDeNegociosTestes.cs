using System.Net;
using System.Security.Authentication;
using System.Text;
using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Integracao.Conexoes;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.GestaoDeNegocios;

/// <summary>
/// O BOTÃO "TESTAR" DA API GESTÃO DE NEGÓCIOS (<see cref="TipoDeConexao.ApiComChave"/>): lê UMA linha do cadastro de
/// metas com a chave no Bearer e diz, sem a chave, o que houve — autenticou, chave recusada, ou o certificado que pede o
/// NOME do servidor. Tratador falso; endereço e chave inventados.
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class TestadorDaGestaoDeNegociosTestes
{
    private const string Chave = "chave-inventada-do-botao-testar";

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
        public List<string> Nomes { get; } = [];

        public HttpClient CreateClient(string name)
        {
            Nomes.Add(name);
            return new HttpClient(tratador, disposeHandler: false);
        }
    }

    private static ConexaoResolvida Gn(string endereco = "https://agro-sistemas-w.tracbel.com.br:5001") => new(
        ConexoesDoSistema.GestaoDeNegocios, TipoDeConexao.ApiComChave, OrigemDaCredencial.Tela, endereco, null, null, null, null, Chave, null, 200, null);

    [Fact]
    public async Task Com_a_chave_aceita_le_uma_linha_e_diz_so_quantas_metas_ha()
    {
        var tratador = new TratadorFalso(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"cadastro":"metas","total":1540,"pagina":1,"paginas":1540,"por_pagina":1,"linhas":[{"id":1,"consultor":"FULANO.DE.TAL"}]}""",
                Encoding.UTF8, "application/json")
        });
        var fabrica = new FabricaFalsa(tratador);

        var resultado = await new TestadorDeConexoes(fabrica).TestarAsync(Gn(), CancellationToken.None);

        resultado.Ok.Should().BeTrue(resultado.Resumo);
        resultado.Resumo.Should().Contain("1540 metas").And.NotContain("FULANO", "nada da linha volta para a tela");
        tratador.Pedidos.Single().RequestUri!.ToString()
            .Should().Be("https://agro-sistemas-w.tracbel.com.br:5001/api/v1/cadastros/metas?pagina=1&por_pagina=1");
        tratador.Pedidos.Single().Headers.Authorization!.ToString().Should().Be($"Bearer {Chave}");
        fabrica.Nomes.Should().OnlyContain(n => n == TestadorDeConexoes.NomeDoCliente, "o teste usa o cliente que não segue redirecionamento");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Found)]
    public async Task A_chave_recusada_e_dita_sem_a_chave(HttpStatusCode codigo)
    {
        var tratador = new TratadorFalso(_ => new HttpResponseMessage(codigo));

        var resultado = await new TestadorDeConexoes(new FabricaFalsa(tratador)).TestarAsync(Gn(), CancellationToken.None);

        resultado.Ok.Should().BeFalse();
        resultado.Resumo.Should().Contain("recusou a chave").And.NotContain(Chave);
    }

    [Fact]
    public async Task Pelo_ip_o_certificado_nao_confere_e_o_teste_manda_usar_o_nome()
    {
        var tratador = new TratadorFalso(_ => throw new HttpRequestException(
            "The SSL connection could not be established", new AuthenticationException("RemoteCertificateNameMismatch")));

        var resultado = await new TestadorDeConexoes(new FabricaFalsa(tratador)).TestarAsync(Gn("https://10.150.4.249:5001"), CancellationToken.None);

        resultado.Ok.Should().BeFalse();
        resultado.Resumo.Should().Contain("use o NOME").And.Contain("agro-sistemas-w.tracbel.com.br");
    }
}
