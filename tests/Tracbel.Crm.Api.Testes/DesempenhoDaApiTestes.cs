using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O TEMPO DE RESPOSTA MEDIDO PELA PRÓPRIA API (issue 51): cada chamada entra na janela da rota dela, o p95 é uma chamada
/// que aconteceu, a janela esquece o que ficou velho, e só quem lê as integrações vê a medida.
/// </summary>
[Trait("Categoria", "Integracoes")]
public sealed class DesempenhoDaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/integracoes/desempenho";

    [Fact]
    public void O_percentil_e_o_posto_mais_proximo_e_nao_um_tempo_inventado()
    {
        var cem = Enumerable.Range(1, 100).Select(i => (double)i).ToList();

        MedidorDeDesempenho.Percentil(cem, 50).Should().Be(50);
        MedidorDeDesempenho.Percentil(cem, 95).Should().Be(95);
        MedidorDeDesempenho.Percentil([7.5], 95).Should().Be(7.5);
        MedidorDeDesempenho.Percentil([], 95).Should().Be(0);
    }

    [Fact]
    public void A_janela_guarda_as_ultimas_mil_e_conta_todas_as_chamadas_e_os_erros()
    {
        var medidor = new MedidorDeDesempenho();

        // QUINHENTAS LENTAS E DEPOIS MIL RÁPIDAS: as lentas saem da janela, e o p95 é o de agora — não o de ontem.
        for (var i = 0; i < 500; i++) medidor.Registrar("POST", "/api/v1/clientes", 1000, 201);
        for (var i = 0; i < MedidorDeDesempenho.AmostrasPorRota; i++) medidor.Registrar("POST", "/api/v1/clientes", 12, i % 100 == 0 ? 500 : 201);

        var rota = medidor.Resumo().Single();
        rota.Chamadas.Should().Be(1500);
        rota.Amostras.Should().Be(MedidorDeDesempenho.AmostrasPorRota);
        rota.P95.Should().Be(12);
        rota.Maximo.Should().Be(12, "as quinhentas lentas já saíram da janela");
        rota.Erros.Should().Be(10);
        rota.Grava.Should().BeTrue("POST passa pela trilha de auditoria");
    }

    [Fact]
    public async Task Sem_a_permissao_de_ler_as_integracoes_a_medida_e_recusada()
    {
        var resposta = await api.ClienteDeBarretos().GetAsync(Rota);

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Cada_chamada_entra_na_rota_dela_com_o_modelo_do_endereco_e_a_gravacao_marcada()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        var http = api.ClienteDeRibeirao();

        for (var i = 0; i < 3; i++)
            (await http.GetAsync("/api/v1/admin/parametros-do-potencial")).StatusCode.Should().Be(HttpStatusCode.OK);

        // UMA GRAVAÇÃO RECUSADA TAMBÉM É MEDIDA: o usuário esperou por ela do mesmo jeito.
        await http.PostAsJsonAsync("/api/v1/clientes", new { });

        var corpo = await http.GetStringAsync(Rota);
        var dados = JsonDocument.Parse(corpo).RootElement.GetProperty("dados");
        var rotas = dados.GetProperty("rotas").EnumerateArray().ToList();

        var leitura = rotas.Single(r => r.GetProperty("rota").GetString() == "/api/v1/admin/parametros-do-potencial");
        leitura.GetProperty("metodo").GetString().Should().Be("GET");
        leitura.GetProperty("chamadas").GetInt64().Should().BeGreaterThanOrEqualTo(3);
        leitura.GetProperty("p95").GetDouble().Should().BeGreaterThanOrEqualTo(leitura.GetProperty("p50").GetDouble());
        leitura.GetProperty("grava").GetBoolean().Should().BeFalse();

        rotas.Should().Contain(r => r.GetProperty("rota").GetString() == "/api/v1/clientes"
                                    && r.GetProperty("metodo").GetString() == "POST"
                                    && r.GetProperty("grava").GetBoolean());

        dados.GetProperty("amostrasPorRota").GetInt32().Should().Be(MedidorDeDesempenho.AmostrasPorRota);
        JsonDocument.Parse(corpo).RootElement.GetProperty("procedencia").GetProperty("objeto").GetString()
            .Should().Contain("em memória");
    }

    // =============================================================================================
    // As consultas ao banco de cada chamada (documento 54 §3.5)
    // =============================================================================================

    [Fact]
    public void O_medidor_guarda_as_consultas_de_cada_chamada_e_resume_o_p95_e_o_maximo()
    {
        var medidor = new MedidorDeDesempenho();
        for (var i = 1; i <= 100; i++) medidor.Registrar("GET", "/api/v1/mercado/demanda", 50, 200, consultas: i);

        var rota = medidor.Resumo().Single();
        rota.ConsultasP95.Should().Be(95, "o percentil por posto mais próximo, como o do tempo");
        rota.ConsultasMaximo.Should().Be(100);
    }

    [Fact]
    public async Task Cada_resposta_da_api_diz_quantas_vezes_foi_ao_banco_e_o_medidor_guarda_o_numero()
    {
        var http = api.ClienteDeRibeirao();

        var resposta = await http.GetAsync("/api/v1/catalogos/ORIGEM_LEAD");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var consultas = int.Parse(resposta.Headers.GetValues("X-Consultas-Ao-Banco").Single(), System.Globalization.CultureInfo.InvariantCulture);
        consultas.Should().BeGreaterThan(0, "a rota lê o catálogo no banco");
        api.Services.GetRequiredService<MedidorDeDesempenho>().Resumo()
            .Single(r => r.Metodo == "GET" && r.Rota == "/api/v1/catalogos/{codigo}").ConsultasMaximo.Should().BeGreaterThanOrEqualTo(consultas);
    }
}
