using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A CALCULADORA DE MÁQUINAS POR HTTP (issue 161).
///
/// <para><b>A igualdade com o mapa é conferida na classe do mapa</b>
/// (<see cref="IndicadoresTerritoriaisTestes"/>), que é onde o cenário completo está semeado; aqui o
/// cenário é mínimo de propósito — um município e a lavoura dele —, para que cada recusa e cada borda
/// da rota apareçam sem depender de cliente, carteira ou faturamento.</para>
///
/// <para>A regra vigente é a semeada pela migração: 1 trator 3036N a cada 10 ha de café, <b>a
/// confirmar</b>, sem ciclo de renovação (D-P01).</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class CalculadoraDeMaquinasTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int RibeiraoPreto = 3543402;
    private const string Rota = "/api/v1/mercado/calculadora";

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private Task<HttpResponseMessage> SimularAsync(object corpo) =>
        api.ClienteDeRibeirao().PostAsJsonAsync(Rota, corpo);

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.ProducoesAgricolasNosMunicipios.AnyAsync()) return;

        var agora = DateTime.UtcNow;
        var ribeirao = Municipio.Criar("Ribeirão Preto", "SP", RibeiraoPreto);
        db.Municipios.Add(ribeirao);
        await db.SaveChangesAsync();

        // O CAFÉ EM TRÊS LINHAS, como o IBGE o publica. Só o "Total" entra na soma da cultura (catálogo
        // da issue 165): se Arábica e Canephora entrassem, a área do café seria 140 ha e não 70.
        db.ProducoesAgricolasNosMunicipios.AddRange(
            ProducaoAgricolaNoMunicipio.Registrar(ribeirao.Id, 2024, 40139, "Café (em grão) Total", new(70m, 60m, 200m, 900m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(ribeirao.Id, 2024, 40140, "Café (em grão) Arábica", new(50m, 40m, 150m, 700m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(ribeirao.Id, 2024, 40141, "Café (em grão) Canephora", new(20m, 20m, 50m, 200m), 100, agora));

        await db.SaveChangesAsync();
    }

    // =============================================================================================
    // O aceite: sem área informada, a calculadora é o motor
    // =============================================================================================

    [Fact]
    public async Task Sem_area_informada_a_calculadora_devolve_a_area_medida_do_municipio()
    {
        await SemearAsync();
        var simulado = await DadosAsync(await SimularAsync(new { municipioCodigoIbge = RibeiraoPreto.ToString() }));

        simulado.GetProperty("municipioNome").GetString().Should().Be("Ribeirão Preto");
        simulado.GetProperty("areaUtilHectares").GetDecimal().Should().Be(70m,
            "a área da cultura é a do produto que entra na soma — Arábica e Canephora não somam de novo");
        simulado.GetProperty("parqueDeMaquinas").GetDecimal().Should().Be(7m, "70 ha ÷ 10 ha por máquina");
        simulado.GetProperty("estimativa").GetBoolean().Should().BeTrue("a regra semeada é o exemplo do gerente, a confirmar");

        var cafe = simulado.GetProperty("culturas").EnumerateArray().Single(c => c.GetProperty("codigo").GetString() == "CAFE");
        cafe.GetProperty("areaMedidaHectares").GetDecimal().Should().Be(70m);
        cafe.GetProperty("anoDaArea").GetInt16().Should().Be(2024);
        cafe.GetProperty("areaInformadaHectares").ValueKind.Should().Be(JsonValueKind.Null);
        cafe.GetProperty("categoriaCodigo").GetString().Should().Be("TRATOR");
    }

    [Fact]
    public async Task A_area_informada_substitui_a_medida_e_a_medida_continua_visivel()
    {
        await SemearAsync();
        var simulado = await DadosAsync(await SimularAsync(new
        {
            municipioCodigoIbge = RibeiraoPreto.ToString(),
            areas = new[] { new { culturaCodigo = "CAFE", areaHectares = "500" } }
        }));

        simulado.GetProperty("parqueDeMaquinas").GetDecimal().Should().Be(50m, "500 ÷ 10");
        simulado.GetProperty("areaUtilHectares").GetDecimal().Should().Be(500m);

        var cafe = simulado.GetProperty("culturas").EnumerateArray().Single(c => c.GetProperty("codigo").GetString() == "CAFE");
        cafe.GetProperty("areaMedidaHectares").GetDecimal().Should().Be(70m,
            "quem simula precisa ver de quanto partiu, senão não sabe o tamanho do que mudou");
        cafe.GetProperty("areaInformadaHectares").GetDecimal().Should().Be(500m);
    }

    [Fact]
    public async Task Sem_municipio_a_conta_parte_do_zero_e_vale_so_o_que_foi_digitado()
    {
        await SemearAsync();
        var simulado = await DadosAsync(await SimularAsync(new
        {
            // SEM SEPARADOR DE MILHAR, como toda a API: "1.000" seria ambíguo entre mil e um.
            areas = new[] { new { culturaCodigo = "CAFE", areaHectares = "1234,5" } }
        }));

        simulado.GetProperty("municipioCodigoIbge").ValueKind.Should().Be(JsonValueKind.Null);
        simulado.GetProperty("parqueDeMaquinas").GetDecimal().Should().Be(123.45m, "1234,5 ÷ 10");
        simulado.GetProperty("culturas").EnumerateArray()
            .Single(c => c.GetProperty("codigo").GetString() == "CAFE")
            .GetProperty("areaMedidaHectares").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // =============================================================================================
    // Vazio com motivo
    // =============================================================================================

    [Fact]
    public async Task A_demanda_anual_nao_sai_e_a_frase_diz_por_que()
    {
        // A regra do café não informou o ciclo de renovação (D-P01): o parque sai, a demanda não, e a tela
        // recebe a frase em vez de um traço mudo.
        await SemearAsync();
        var simulado = await DadosAsync(await SimularAsync(new
        {
            areas = new[] { new { culturaCodigo = "CAFE", areaHectares = "100" } }
        }));

        simulado.GetProperty("parqueDeMaquinas").GetDecimal().Should().Be(10m);
        simulado.GetProperty("demandaAnualDeMaquinas").ValueKind.Should().Be(JsonValueKind.Null);
        simulado.GetProperty("motivoSemDemanda").GetString().Should().Be("SemCicloDeRenovacao");
        simulado.GetProperty("frase").GetString().Should().Contain("falta o ciclo de renovação de Café");
    }

    [Fact]
    public async Task Area_zero_e_resposta_e_a_resposta_e_zero()
    {
        await SemearAsync();
        var simulado = await DadosAsync(await SimularAsync(new
        {
            areas = new[] { new { culturaCodigo = "CAFE", areaHectares = "0" } }
        }));

        simulado.GetProperty("parqueDeMaquinas").GetDecimal().Should().Be(0m);
        simulado.GetProperty("motivoSemParque").GetString().Should().Be("Nenhum",
            "zero hectare é resposta — 'não se planta aqui' —, e não ausência de dado");
    }

    [Fact]
    public async Task A_resposta_diz_por_que_os_cenarios_nao_entram()
    {
        await SemearAsync();
        var simulado = await DadosAsync(await SimularAsync(new
        {
            areas = new[] { new { culturaCodigo = "CAFE", areaHectares = "100" } }
        }));

        simulado.GetProperty("sobreOsCenarios").GetString()
            .Should().Contain("D-P05", "a tela precisa dizer que falta decisão, e não deixar o campo mudo");
    }

    // =============================================================================================
    // As recusas
    // =============================================================================================

    [Fact]
    public async Task Cultura_sem_regra_e_recusa_com_o_nome_dela()
    {
        // Devolver uma linha zerada faria quem digitou "UVA" concluir que não há potencial de uva; o certo
        // é dizer que falta a regra, e onde cadastrá-la.
        await SemearAsync();
        var resposta = await SimularAsync(new
        {
            areas = new[] { new { culturaCodigo = "UVA", areaHectares = "2000" } }
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("UVA").And.Contain("Administrador");
    }

    [Fact]
    public async Task A_mesma_cultura_duas_vezes_e_recusada()
    {
        // Somar em silêncio esconderia um engano de digitação; a segunda linha sobrescrevendo a primeira,
        // também.
        await SemearAsync();
        var resposta = await SimularAsync(new
        {
            areas = new[]
            {
                new { culturaCodigo = "CAFE", areaHectares = "100" },
                new { culturaCodigo = "CAFE", areaHectares = "200" }
            }
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("duas vezes");
    }

    [Theory]
    [InlineData("-10")]
    [InlineData("30000000")]
    public async Task Area_negativa_ou_maior_que_o_estado_e_recusada(string area)
    {
        await SemearAsync();
        (await SimularAsync(new { areas = new[] { new { culturaCodigo = "CAFE", areaHectares = area } } }))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Municipio_que_nao_existe_no_catalogo_e_404()
    {
        await SemearAsync();
        (await SimularAsync(new { municipioCodigoIbge = "9999999" }))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Antes_da_vigencia_da_regra_nao_ha_o_que_calcular()
    {
        // A regra do café vale desde 13/09/2026 (issue 71). Antes disso não havia regra nenhuma — e a
        // calculadora diz isso, em vez de aplicar a regra de hoje a uma data em que ela não valia.
        await SemearAsync();
        var resposta = await SimularAsync(new
        {
            data = "2026-01-01",
            areas = new[] { new { culturaCodigo = "CAFE", areaHectares = "100" } }
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("regra de potencial vigente");
    }
}
