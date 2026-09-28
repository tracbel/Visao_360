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
/// O SHARE DA TRACBEL NO CRÉDITO DE MECANIZAÇÃO PELA API (issue 262, decisões de 28/09/2026): agregado por filial, na
/// janela do painel do crédito, com recurso próprio e consórcio fora do numerador e o município só indicativo.
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class ShareNoCreditoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/territorio/credito/share";
    private const int Trator = 7080;

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.FinanciamentosDaVenda.AnyAsync()) return;

        var agora = DateTime.UtcNow;
        var franca = Municipio.Criar("Franca", "SP", 3516200);
        var cravinhos = Municipio.Criar("Cravinhos", "SP", 3513108);
        var barretos = Municipio.Criar("Barretos", "SP", 3505500);
        var prudente = Municipio.Criar("Presidente Prudente", "SP", 3541406);
        db.Municipios.AddRange(franca, cravinhos, barretos, prudente);
        await db.SaveChangesAsync();

        // Franca e Cravinhos são de Ribeirão (1), Barretos da filial 2; Presidente Prudente não é da ADR.
        db.MunicipiosDaAreaDeAtuacao.AddRange(
            MunicipioDaAreaDeAtuacao.Registrar(franca.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 2, 100, agora),
            MunicipioDaAreaDeAtuacao.Registrar(cravinhos.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 3, 100, agora),
            MunicipioDaAreaDeAtuacao.Registrar(barretos.Id, true, RegiaoDaAreaDeAtuacao.Norte, 2, "Area de Atuação.xlsx", 4, 100, agora));

        // O SICOR vai até 08/2026: a janela é 09/2025 a 08/2026. A linha de 08/2025 fica fora dela.
        db.CreditosRuraisDeInvestimento.AddRange(
            Linha(franca.Id, 6482, 2026, 8, 1_000_000m),
            Linha(cravinhos.Id, 6483, 2026, 7, 200_000m),
            Linha(barretos.Id, 6484, 2026, 6, 800_000m),
            Linha(franca.Id, 6482, 2025, 8, 5_000_000m),
            Linha(prudente.Id, 6485, 2026, 8, 900_000m));

        db.FinanciamentosDaVenda.AddRange(
            Financiamento(1, franca.Id, 2026, 8, 300_000m, "MODER FROTA", "BANCO JOHN DEERE"),
            Financiamento(2, franca.Id, 2026, 2, 100_000m, "PRONAF", "SICREDI"),
            Financiamento(3, franca.Id, 2026, 3, 999_000m, "RECURSO PRÓPRIO", "NÃO SE APLICA"),
            Financiamento(4, cravinhos.Id, 2026, 7, 250_000m, "FINAME", "BRADESCO"),
            Financiamento(5, barretos.Id, 2026, 6, 200_000m, "CONSÓRCIO", "CONSÓRCIO JOHN DEERE"),
            Financiamento(6, barretos.Id, 2025, 8, 400_000m, "MODER FROTA", "BANCO JOHN DEERE"),
            Financiamento(7, prudente.Id, 2026, 5, 150_000m, "PRONAMP", "BANCO DO BRASIL"),
            Financiamento(8, null, 2026, 5, 90_000m, "PRONAMP", "BANCO DO BRASIL"));

        await db.SaveChangesAsync();
    }

    private static CreditoRuralDeInvestimento Linha(int municipioId, int bcb, short ano, byte mes, decimal valor) =>
        CreditoRuralDeInvestimento.Registrar(
            new CreditoRuralDeInvestimento.Chave(bcb, ano, mes, Trator, 154, 71, 431, 9, 1, 14),
            municipioId, valor, 0m, 100, DateTime.UtcNow);

    private static FinanciamentoDaVenda Financiamento(
        long processo, int? municipioId, int ano, int mes, decimal valor, string linha, string instituicao) =>
        FinanciamentoDaVenda.Registrar(processo,
            new FinanciamentoDaVenda.Dados("IV_Q_VENDA_EQUIPAMENTO", new DateOnly(ano, mes, 10), false, municipioId, valor,
                instituicao, linha, $"hash{processo}"),
            100, DateTime.UtcNow);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    [Fact]
    public async Task A_regiao_divide_o_credito_rural_da_tracbel_pelo_sicor_de_maquinas_na_janela()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        dados.GetProperty("inicio").GetString().Should().StartWith("2025-09");
        dados.GetProperty("fim").GetString().Should().StartWith("2026-08");

        // Tracbel na Região e na janela: 300 + 100 + 250 mil. Fora: recurso próprio (999 mil), consórcio (200 mil),
        // 08/2025 (fora da janela), Presidente Prudente (fora da ADR) e o sem município.
        var regiao = dados.GetProperty("regiao");
        regiao.GetProperty("valorTracbel").GetDecimal().Should().Be(650_000m);
        regiao.GetProperty("financiamentos").GetInt32().Should().Be(3);
        regiao.GetProperty("valorSicor").GetDecimal().Should().Be(2_000_000m);
        regiao.GetProperty("share").GetDecimal().Should().Be(0.325m);
        regiao.GetProperty("valorDaConcorrencia").GetDecimal().Should().Be(1_350_000m);

        var fora = dados.GetProperty("foraDaRegiao");
        fora.GetProperty("foraDaAdr").GetInt32().Should().Be(1);
        fora.GetProperty("valorForaDaAdr").GetDecimal().Should().Be(150_000m);
        fora.GetProperty("semMunicipio").GetInt32().Should().Be(1);
        fora.GetProperty("valorSemMunicipio").GetDecimal().Should().Be(90_000m);

        dados.GetProperty("procedencia").GetProperty("ressalva").GetString().Should().Contain("ESTIMATIVA");
    }

    [Fact]
    public async Task Por_filial_os_municipios_que_ela_responde_somados()
    {
        await SemearAsync();
        var filiais = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota))).GetProperty("porFilial").EnumerateArray().ToList();

        var ribeirao = filiais.Single(f => f.GetProperty("empresaId").GetInt32() == 1);
        ribeirao.GetProperty("municipios").GetInt32().Should().Be(2);
        ribeirao.GetProperty("share").GetProperty("valorTracbel").GetDecimal().Should().Be(650_000m);
        ribeirao.GetProperty("share").GetProperty("valorSicor").GetDecimal().Should().Be(1_200_000m);

        var barretos = filiais.Single(f => f.GetProperty("empresaId").GetInt32() == 2);
        barretos.GetProperty("share").GetProperty("valorTracbel").GetDecimal().Should().Be(0m, "consórcio não é crédito rural");
        barretos.GetProperty("share").GetProperty("share").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task O_municipio_e_indicativo_e_as_linhas_fora_do_share_aparecem_a_parte()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var cravinhos = dados.GetProperty("porMunicipio").EnumerateArray().Single(m => m.GetProperty("codigoIbge").GetInt32() == 3513108);
        cravinhos.GetProperty("acimaDoSicor").GetBoolean().Should().BeTrue("250 mil financiados contra 200 mil no SICOR");
        cravinhos.GetProperty("filial").GetString().Should().Contain("Ribeirão");

        var linhas = dados.GetProperty("porLinha").EnumerateArray()
            .ToDictionary(l => l.GetProperty("linha").GetString()!, l => l);
        linhas["RECURSO PRÓPRIO"].GetProperty("contaNoShare").GetBoolean().Should().BeFalse();
        linhas["CONSÓRCIO"].GetProperty("contaNoShare").GetBoolean().Should().BeFalse();
        linhas["MODER FROTA"].GetProperty("contaNoShare").GetBoolean().Should().BeTrue();
        linhas["MODER FROTA"].GetProperty("valor").GetDecimal().Should().Be(300_000m, "o de 08/2025 está fora da janela");

        var meses = dados.GetProperty("porMes").EnumerateArray().ToList();
        meses.Should().HaveCount(12);
        var agosto = meses.Single(m => m.GetProperty("mes").GetString()!.StartsWith("2026-08", StringComparison.Ordinal));
        agosto.GetProperty("valorTracbel").GetDecimal().Should().Be(300_000m);
        agosto.GetProperty("valorSicor").GetDecimal().Should().Be(1_000_000m);
    }
}
