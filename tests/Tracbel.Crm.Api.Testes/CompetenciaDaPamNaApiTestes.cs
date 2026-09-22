using System.Net;
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
/// A COMPETÊNCIA E A UNIDADE DE CADA MEDIDA DA PAM, pela API de verdade (issue 152).
///
/// <para><b>O cenário é o do dia em que a PAM nova entra incompleta:</b> a cana já tem 2025 e o café, cultura da regra,
/// ainda só tem 2024. Com o "maior ano da tabela" para tudo — a regra até aqui —, o café aparecia sem área e o mapa
/// de potencial ficava vazio. Cada cultura no seu ano, ele aparece com 2024, e a tela diz o ano.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class CompetenciaDaPamNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int RibeiraoPreto = 3543402;
    private const string Periodo = "/api/v1/territorio/indicadores?competenciaInicial=2026-01&competenciaFinal=2026-06";

    private async Task<JsonElement> IndicadoresAsync()
    {
        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

            if (!await db.MunicipiosDaAreaDeAtuacao.AnyAsync())
            {
                var agora = DateTime.UtcNow;
                var ribeirao = Municipio.Criar("Ribeirão Preto", "SP", RibeiraoPreto);
                db.Municipios.Add(ribeirao);
                await db.SaveChangesAsync();

                db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                    ribeirao.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 2, 100, agora));

                db.ProducoesAgricolasNosMunicipios.AddRange(
                    ProducaoAgricolaNoMunicipio.Registrar(ribeirao.Id, 2024, 40139, "Café (em grão) Total", new(70m, 60m, 200m, 900m), 100, agora),
                    ProducaoAgricolaNoMunicipio.Registrar(ribeirao.Id, 2025, 40106, "Cana-de-açúcar", new(1_000m, 990m, 80_000m, 5_100m), 100, agora));

                db.ProducoesAgricolasNosEstados.AddRange(
                    ProducaoAgricolaNoEstado.Registrar(35, 2024, 40139, "Café (em grão) Total", new(700m, 600m, 2_000m, 9_000m), 100, agora),
                    ProducaoAgricolaNoEstado.Registrar(35, 2025, 40106, "Cana-de-açúcar", new(9_300m, 9_200m, 800_000m, 51_000m), 100, agora));

                db.FrotasDeTratoresNosMunicipios.Add(FrotaDeTratoresNoMunicipio.Registrar(ribeirao.Id, 2017, 113521, "Total", 172, 450, 100, agora));

                await db.SaveChangesAsync();
            }
        }

        var resposta = await api.ClienteDeRibeirao().GetAsync(Periodo);
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").GetProperty("indicadores").Clone();
    }

    private static JsonElement Ribeirao(JsonElement indicadores) =>
        indicadores.GetProperty("municipios").EnumerateArray().Single(m => m.GetProperty("codigoIbge").GetInt32() == RibeiraoPreto);

    [Fact]
    public async Task A_cultura_da_regra_fica_no_ano_dela_quando_outra_cultura_ja_tem_o_ano_seguinte()
    {
        var indicadores = await IndicadoresAsync();
        var cafe = Ribeirao(indicadores).GetProperty("potencial")[0];

        cafe.GetProperty("produtoCodigoIbge").GetInt32().Should().Be(40139, "a regra semeada é a do café");
        cafe.GetProperty("ano").GetInt32().Should().Be(2024, "a cana tem 2025, o café ainda não");
        cafe.GetProperty("areaPlantadaHectares").GetDecimal().Should().Be(70m,
            "com o maior ano da tabela (2025) para tudo, o café viria sem área — e o mapa, vazio");
        cafe.GetProperty("maquinasTeoricas").GetDecimal().Should().Be(7m);

        var lavoura = Ribeirao(indicadores).GetProperty("producao");
        lavoura.GetProperty("ano").GetInt32().Should().Be(2025, "a lavoura soma um ano só, o mais recente — e diz qual");
        lavoura.GetProperty("culturasComArea").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task A_quantidade_vem_com_a_unidade_e_a_produtividade_sobre_a_area_colhida()
    {
        var cafe = Ribeirao(await IndicadoresAsync()).GetProperty("potencial")[0];

        cafe.GetProperty("quantidadeProduzida").GetDecimal().Should().Be(200m);
        cafe.GetProperty("unidadeDaQuantidade").GetString().Should().Be("toneladas");
        cafe.GetProperty("produtividade").GetDecimal().Should().Be(3.3333m, "200 t sobre 60 ha colhidos — e não sobre os 70 plantados");
        cafe.GetProperty("unidadeDaProdutividade").GetString().Should().Be("t/ha");
    }

    [Fact]
    public async Task O_estado_traz_a_mesma_cultura_no_mesmo_ano_e_o_ano_do_censo()
    {
        var indicadores = await IndicadoresAsync();

        var culturas = indicadores.GetProperty("culturasNoEstado").EnumerateArray().ToList();
        culturas.Should().ContainSingle("só o café tem regra");
        culturas[0].GetProperty("ano").GetInt32().Should().Be(2024, "o mesmo ano do café no município — a comparação não mistura anos");
        culturas[0].GetProperty("produtividade").GetDecimal().Should().Be(3.3333m, "2.000 t sobre 600 ha colhidos em SP");
        culturas[0].GetProperty("unidadeDaQuantidade").GetString().Should().Be("toneladas");

        var estado = indicadores.GetProperty("estado");
        estado.GetProperty("ano").GetInt32().Should().Be(2025, "o ano da área e do valor da lavoura");
        estado.GetProperty("anoDoCenso").GetInt32().Should().Be(2017, "os tratores do estado são do Censo, e agora dizem isso");
    }
}
