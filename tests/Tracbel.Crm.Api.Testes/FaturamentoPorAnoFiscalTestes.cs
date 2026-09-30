using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O FATURAMENTO PELA NOTA DO PROTHEUS SEGUE O ANO FISCAL ESCOLHIDO (30/09/2026, #313) — a série do "Faturamento — 12
/// meses" no modo da nota e o "Top 5 clientes" da Visão 360.
///
/// <para>Decisão do Ricardo: o período muda todos os painéis que têm data. Antes, a série era sempre a dos doze meses até o
/// último carregado, e o ranking era o acumulado de sempre — trocar o ano na tela não mexia neles.</para>
/// </summary>
[Trait("Categoria", "PainelExecutivo")]
public sealed class FaturamentoPorAnoFiscalTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/relatorios/faturamento";

    // DOIS ANOS FISCAIS FECHADOS, longe de hoje: o FY2024 (nov/2023 a out/2024) e o FY2025 (nov/2024 a out/2025).
    private static readonly DateOnly OutubroDe2024 = new(2024, 10, 1);
    private static readonly DateOnly MarcoDe2025 = new(2025, 3, 1);
    private static readonly DateOnly OutubroDe2025 = new(2025, 10, 1);

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.Clientes.AnyAsync(c => c.NomeRazao == "Comprou no FY2024")) return;

        var doAnoPassado = Cliente.Criar(1, "Comprou no FY2024", TipoDePessoa.Juridica, 100, 100);
        var doAno = Cliente.Criar(1, "Comprou no FY2025", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.AddRange(doAnoPassado, doAno);
        await db.SaveChangesAsync();

        // O MAIOR NO ACUMULADO É O DO FY2024; no FY2025, só o outro comprou.
        db.FaturamentoDosClientes.AddRange(
            FaturamentoDoCliente.Criar(1, doAnoPassado.Id, OutubroDe2024, 9_000m, 1, 1, new QuebraDoFaturamento(9_000m, 0m, 0m, 0m)),
            FaturamentoDoCliente.Criar(1, doAno.Id, MarcoDe2025, 1_000m, 1, 1, new QuebraDoFaturamento(1_000m, 0m, 0m, 0m)),
            FaturamentoDoCliente.Criar(1, doAno.Id, OutubroDe2025, 500m, 1, 1, new QuebraDoFaturamento(500m, 0m, 0m, 0m)));
        await db.SaveChangesAsync();
    }

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static List<string?> Nomes(JsonElement dados) =>
        dados.GetProperty("topClientes").EnumerateArray().Select(c => c.GetProperty("nome").GetString()).ToList();

    [Fact]
    public async Task Com_o_ano_fiscal_a_serie_termina_em_outubro_dele_e_o_ranking_e_o_do_ano()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"{Rota}?anoFiscal=2025"));

        var serie = dados.GetProperty("serie").EnumerateArray().ToList();
        serie.Should().HaveCount(12);
        serie[0].GetProperty("competencia").GetString().Should().Be(AnoFiscal.Inteiro(2025).Inicial.ToString("yyyy-MM-dd"));
        serie[^1].GetProperty("competencia").GetString().Should().Be("2025-10-01");
        serie.Sum(m => m.GetProperty("valorLiquido").GetDecimal()).Should().Be(1_500m, "só as notas do FY2025");
        dados.GetProperty("ultimoMesEstaAberto").GetBoolean().Should().BeFalse("outubro de um ano fechado não está em curso");

        Nomes(dados).Should().Equal("Comprou no FY2025");
    }

    [Fact]
    public async Task Sem_o_ano_fiscal_fica_como_antes_os_doze_meses_ate_o_ultimo_carregado_e_o_acumulado()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        dados.GetProperty("serie").EnumerateArray().Last().GetProperty("competencia").GetString().Should().Be("2025-10-01");
        Nomes(dados).Should().Equal("Comprou no FY2024", "Comprou no FY2025");
    }
}
