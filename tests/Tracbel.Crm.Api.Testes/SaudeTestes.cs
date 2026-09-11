using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A prova de vida — a rota que o deploy consulta para decidir se a atualização deu certo.
/// </summary>
[Trait("Categoria", "Api")]
public sealed class SaudeTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    [Fact]
    public async Task A_prova_de_vida_do_banco_responde_sem_contexto_de_acesso()
    {
        // Sem cabeçalho nenhum: /saude fica fora do meio de campo, e é justamente isso que a
        // derrubava — o banco injetado lia um contexto de acesso que nunca era definido.
        var resposta = await api.CreateClient().GetAsync("/saude/banco");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK,
            "um 500 mudo aqui faria o deploy remoto dar a atualização como quebrada sem ela estar");

        var corpo = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;
        corpo.GetProperty("conectado").GetBoolean().Should().BeTrue();
        corpo.TryGetProperty("migracoesPendentes", out _).Should().BeTrue(
            "é por esta lista que o deploy confere que a migração na subida rodou");
    }
}
