using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O PREÇO DE COMMODITIES PELA API (issue 260).
///
/// <para><b>O cenário:</b> a soja tem 13 meses da CONAB — 7 a R$ 2,00 e os 6 últimos a R$ 2,20 —, e por isso o R6 sai (2,20 ÷
/// 2,00 = 1,10) e o R12 não (precisa de 24 meses). A cana tem 24 meses de ATR mensal da Socicana — 12 a R$ 1,00 e 12 a R$
/// 1,10 —, mais o ACUMULADO da safra, que não pode entrar na série. O café não tem cotação nenhuma.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class PrecosDasCulturasNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/mercado/precos";

    private async Task<HttpClient> SemearAsync()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var http = api.ClienteDeRibeirao();

        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        if (await db.CotacoesDeProdutos.AnyAsync()) return http;

        var agora = DateTime.UtcNow;
        for (var i = 0; i < 13; i++)
            db.CotacoesDeProdutos.Add(CotacaoDeProduto.Registrar(
                "CONAB", "4744", "SP", "RECEBIDO PELO PRODUTOR", "SOJA", "EM GRÃOS", "kg", new DateOnly(2025, 8, 1).AddMonths(i), i < 7 ? 2.00m : 2.20m, 100, agora));

        for (var i = 0; i < 24; i++)
        {
            var mes = new DateOnly(2024, 9, 1).AddMonths(i);
            db.CotacoesDeProdutos.Add(CotacaoDeProduto.Registrar(
                "SOCICANA", "ATR", "SP", "MENSAL", "CANA DE AÇÚCAR", "KG DE ATR", "kg de ATR", mes, i < 12 ? 1.00m : 1.10m, 100, agora));
            db.CotacoesDeProdutos.Add(CotacaoDeProduto.Registrar(
                "SOCICANA", "ATR", "SP", "ACUMULADO DA SAFRA", "CANA DE AÇÚCAR", "KG DE ATR", "kg de ATR", mes, 5.00m, 100, agora));
        }

        await db.SaveChangesAsync();
        return http;
    }

    private static async Task<Dictionary<string, JsonElement>> CulturasAsync(HttpClient http)
    {
        var resposta = await http.GetAsync(Rota);
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").GetProperty("culturas").Clone()
            .EnumerateArray().ToDictionary(c => c.GetProperty("codigo").GetString()!);
    }

    private static JsonElement Horizonte(JsonElement cultura, int meses) =>
        cultura.GetProperty("horizontes").EnumerateArray().Single(h => h.GetProperty("meses").GetInt32() == meses).GetProperty("indice");

    [Fact]
    public async Task Cada_horizonte_e_a_media_dos_ultimos_meses_sobre_a_dos_anteriores()
    {
        var culturas = await CulturasAsync(await SemearAsync());

        var soja = culturas["SOJA"];
        soja.GetProperty("serie").GetArrayLength().Should().Be(13);
        soja.GetProperty("unidade").GetString().Should().Be("kg");
        soja.GetProperty("ultimoValor").GetDecimal().Should().Be(2.20m);

        Horizonte(soja, 1).GetProperty("indice").GetDecimal().Should().Be(1m, "o último mês contra o anterior, os dois a R$ 2,20");
        Horizonte(soja, 3).GetProperty("indice").GetDecimal().Should().Be(1m);
        var r6 = Horizonte(soja, 6);
        r6.GetProperty("indice").GetDecimal().Should().BeApproximately(1.10m, 0.0001m, "R$ 2,20 contra R$ 2,00");
        (r6.GetProperty("mediaRecente").GetDecimal(), r6.GetProperty("mediaAnterior").GetDecimal()).Should().Be((2.20m, 2.00m));
    }

    [Fact]
    public async Task O_R12_e_o_momento_dos_Indicadores_e_a_serie_curta_diz_o_motivo()
    {
        var culturas = await CulturasAsync(await SemearAsync());

        Horizonte(culturas["SOJA"], 12).GetProperty("motivo").GetString().Should().Be("SerieCurta", "13 meses não fecham 12 contra 12");

        var cana = culturas["CANA"];
        cana.GetProperty("serie").GetArrayLength().Should().Be(24, "o acumulado da safra não entra na série mensal");
        var r12 = Horizonte(cana, 12);
        r12.GetProperty("indice").GetDecimal().Should().BeApproximately(1.10m, 0.0001m);
        r12.GetProperty("faixa").GetString().Should().Be("Intermediaria");
        r12.GetProperty("serie").GetString().Should().Be("Mensal");
    }

    [Fact]
    public async Task Cultura_sem_cotacao_vem_sem_serie_e_com_o_motivo_em_cada_horizonte()
    {
        var culturas = await CulturasAsync(await SemearAsync());

        var cafe = culturas["CAFE"];
        cafe.GetProperty("serie").GetArrayLength().Should().Be(0);
        cafe.GetProperty("ultimoValor").ValueKind.Should().Be(JsonValueKind.Null);
        Horizonte(cafe, 3).GetProperty("indice").ValueKind.Should().Be(JsonValueKind.Null);
        Horizonte(cafe, 3).GetProperty("motivo").GetString().Should().Be("SerieCurta");
    }
}
