using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A RENTABILIDADE POR CULTURA PELA API DE VERDADE (issue 159).
///
/// <para>O cenário usa o número público do aceite da issue 67: o custo total do café em <b>Franca</b> na
/// safra 2025 é <b>R$ 29.279,94/ha</b>. A produtividade e o preço são semeados para que a margem seja
/// conferível à mão.</para>
///
/// <para>O catálogo vem semeado pela migração (issue 165) com as seis culturas; o que este teste
/// acrescenta é a <b>referência do custo</b> do café, que nasce em aberto (D-P07).</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class RentabilidadeNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/territorio/rentabilidade";

    /// <summary>O custo total por hectare do café em Franca, safra 2025 — aceite da issue 67.</summary>
    private const decimal CustoDoCafe = 29_279.94m;

    private const int CafeTotal = 40139;

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.ProducoesAgricolasNosEstados.AnyAsync(p => p.ProdutoCodigoIbge == CafeTotal)) return;

        var agora = DateTime.UtcNow;

        // A PAM de São Paulo: 90.000 t de café em 50.000 ha colhidos = 1.800 kg/ha.
        db.ProducoesAgricolasNosEstados.Add(ProducaoAgricolaNoEstado.Registrar(
            35, 2025, CafeTotal, "Café (em grão) Total", new(52_000m, 50_000m, 90_000m, 3_000_000m), 100, agora));

        // O preço da CONAB: dois meses de 2025, média R$ 33,00/kg.
        db.CotacoesDeProdutos.AddRange(
            CotacaoDeProduto.Registrar("CONAB", "11195", "SP", "PREÇO RECEBIDO P/ PRODUTOR", "CAFE",
                "ARÁBICA TIPO 6, BEBIDA DURA", "kg", new DateOnly(2025, 6, 1), 32m, 100, agora),
            CotacaoDeProduto.Registrar("CONAB", "11195", "SP", "PREÇO RECEBIDO P/ PRODUTOR", "CAFE",
                "ARÁBICA TIPO 6, BEBIDA DURA", "kg", new DateOnly(2025, 7, 1), 34m, 100, agora));

        // O custo da CONAB em Franca, com o número público do aceite da issue 67.
        var valores = new CustoDeProducao.Valores(
            9, 1_800m, "kg/ha", 15_000m, 500m, 9_000m, 300m, 24_000m, 800m, 5_279.94m, 175m, CustoDoCafe, 975m);

        db.CustosDeProducao.Add(CustoDeProducao.Registrar(
            "CAFÉ ARÁBICA", "Franca-SP-2025", "Franca", null, null, 2025, "saca de 60 kg", valores, 100, agora));

        await db.SaveChangesAsync();

        // A REFERÊNCIA DO CAFÉ (D-P07) entra PELA ROTA, e não por gravação direta: é assim que o
        // administrador a escolhe, e a trilha exige filial — gravar por fora não exercitaria o caminho.
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var resposta = await api.ClienteDeRibeirao().PutAsJsonAsync(
            "/api/v1/admin/parametros-do-potencial/catalogo/culturas/CAFE",
            new
            {
                nome = "Café",
                segmento = "Cafe",
                unidadeComercial = "saca de 60 kg",
                quilosPorUnidade = "60",
                fonteDoPreco = "CONAB",
                produtoDoPreco = "11195",
                serieDeCusto = "CAFÉ ARÁBICA",
                localDeReferenciaDoCusto = "Franca",
                camadaDeCustoDaMargem = "Total"
            });

        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
    }

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static JsonElement Cultura(JsonElement dados, string codigo) =>
        dados.EnumerateArray().Single(c => c.GetProperty("culturaCodigo").GetString() == codigo);

    [Fact]
    public async Task A_margem_do_cafe_sai_com_o_custo_publico_de_franca_e_as_tres_competencias()
    {
        // O CRITÉRIO DE ACEITE DA ISSUE: com Franca como referência, o custo total é R$ 29.279,94, e a
        // margem sai com as três competências para o tooltip.
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var cafe = Cultura(dados, "CAFE");

        cafe.GetProperty("custoPorHectare").GetDecimal().Should().Be(CustoDoCafe);
        cafe.GetProperty("localDoCusto").GetString().Should().Be("Franca");
        cafe.GetProperty("camadaDoCusto").GetString().Should().Be("Total");
        cafe.GetProperty("safraDoCusto").GetInt32().Should().Be(2025);

        cafe.GetProperty("produtividadeKgPorHa").GetDecimal().Should().Be(1_800m, "90.000 t em 50.000 ha colhidos");
        cafe.GetProperty("anoDaProdutividade").GetInt32().Should().Be(2025);
        cafe.GetProperty("precoMedioPorKg").GetDecimal().Should().Be(33m, "média de R$ 32 e R$ 34");
        cafe.GetProperty("mesesDePrecoNaMedia").GetInt32().Should().Be(2);

        cafe.GetProperty("receitaPorHectare").GetDecimal().Should().Be(59_400m, "1.800 kg/ha × R$ 33/kg");
        cafe.GetProperty("margemPorHectare").GetDecimal().Should().Be(59_400m - CustoDoCafe);

        cafe.GetProperty("motivo").GetString().Should().Be("Nenhum");
        cafe.GetProperty("fraseDoMotivo").GetString().Should().BeEmpty();
    }

    [Fact]
    public async Task A_margem_total_multiplica_a_area_COLHIDA()
    {
        // Não a plantada (achado C-07): em cultura perene a plantada inclui o cafezal novo que ainda
        // não produz, e multiplicar por ela inventaria receita que não existiu.
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var cafe = Cultura(dados, "CAFE");

        cafe.GetProperty("areaColhidaHectares").GetDecimal().Should().Be(50_000m, "a plantada é 52.000");
        cafe.GetProperty("margemTotal").GetDecimal().Should().Be((59_400m - CustoDoCafe) * 50_000m);
    }

    [Fact]
    public async Task Cultura_sem_referencia_escolhida_nao_tem_margem_e_a_tela_recebe_o_motivo()
    {
        // D-P07 EM ABERTO nas outras cinco culturas semeadas: a margem não sai, e o motivo é uma frase
        // que a tela mostra — não um traço mudo nem um local escolhido por conta própria.
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var soja = Cultura(dados, "SOJA");

        soja.GetProperty("margemPorHectare").ValueKind.Should().Be(JsonValueKind.Null);
        soja.GetProperty("motivo").GetString().Should().Be("SemLocalDeReferencia");
        soja.GetProperty("fraseDoMotivo").GetString().Should().Contain("local de referência");
    }
}
