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
/// O PREÇO DA MÁQUINA NA ROTA DE PREÇOS (issue 70, D-P12): a outra metade do termo de troca, ao lado das séries da CONAB e
/// da Socicana. Uma série por categoria, na ordem do catálogo, com a mediana, o menor, o maior e quantas notas.
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class PrecoDeMaquinaNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/territorio/precos";

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    [Fact]
    public async Task Sem_a_rotina_rodada_nao_ha_serie_de_maquina_e_com_ela_cada_categoria_vem_na_ordem_do_catalogo()
    {
        (await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota))).GetProperty("maquinas").GetArrayLength()
            .Should().Be(0, "a rotina PRECOS_DE_MAQUINA ainda não rodou");

        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

            var agora = DateTime.UtcNow;
            var trator = await db.CategoriasDeMaquina.SingleAsync(c => c.Codigo == "TRATOR");
            var colheitadeira = await db.CategoriasDeMaquina.SingleAsync(c => c.Codigo == "COLHEITADEIRA");

            db.PrecosDeMaquina.AddRange(
                PrecoDeMaquinaNoMes.Registrar(colheitadeira.Id, new DateOnly(2026, 3, 1), 3_000_000m, 3_000_000m, 3_000_000m, 1,
                    PrecoDeMaquinaNoMes.FonteDaNota, 100, agora),
                PrecoDeMaquinaNoMes.Registrar(trator.Id, new DateOnly(2026, 4, 1), 450_000m, 450_000m, 450_000m, 1,
                    PrecoDeMaquinaNoMes.FonteDaNota, 100, agora),
                PrecoDeMaquinaNoMes.Registrar(trator.Id, new DateOnly(2026, 3, 1), 500_000m, 400_000m, 600_000m, 2,
                    PrecoDeMaquinaNoMes.FonteDaNota, 100, agora));
            await db.SaveChangesAsync();
        }

        var maquinas = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota))).GetProperty("maquinas").EnumerateArray().ToList();

        maquinas.Select(m => m.GetProperty("categoriaCodigo").GetString())
            .Should().Equal(["TRATOR", "COLHEITADEIRA"], "a ordem de exibição do catálogo");

        var meses = maquinas[0].GetProperty("meses").EnumerateArray().ToList();
        meses.Select(m => m.GetProperty("mes").GetString()).Should().Equal(["2026-03-01", "2026-04-01"], "do mais antigo ao mais recente");
        meses[0].GetProperty("mediana").GetDecimal().Should().Be(500_000m);
        meses[0].GetProperty("notas").GetInt32().Should().Be(2);

        maquinas[0].GetProperty("procedencia").GetProperty("competencia").GetString().Should().Be("03/2026 a 04/2026");
    }
}
