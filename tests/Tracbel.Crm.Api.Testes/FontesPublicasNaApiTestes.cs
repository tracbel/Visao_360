using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O PAINEL DE FONTES PÚBLICAS E AS OPÇÕES DOS FORMULÁRIOS, pela API de verdade (issue 77).
///
/// <para>O banco é semeado com três situações: a PAM rodou agora (em dia), o dólar rodou há dois meses (atrasado)
/// e os preços da CONAB nunca rodaram (sem dado) — o caso do servidor em 21/09/2026. A ADR tem dois municípios,
/// e um terceiro fora dela prova que a cobertura só conta os da ADR.</para>
/// </summary>
[Trait("Categoria", "ParametrosDoPotencial")]
public sealed class FontesPublicasNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int Franca = 3516200;
    private const int Cravinhos = 3513108;
    private const int Uberaba = 3170107;

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.Municipios.AnyAsync(m => m.CodigoIbge == Franca)) return;

        var agora = DateTime.UtcNow;
        var franca = Municipio.Criar("Franca", "SP", Franca);
        var cravinhos = Municipio.Criar("Cravinhos", "SP", Cravinhos);
        var uberaba = Municipio.Criar("Uberaba", "MG", Uberaba);
        db.Municipios.AddRange(franca, cravinhos, uberaba);
        await db.SaveChangesAsync();

        db.MunicipiosDaAreaDeAtuacao.AddRange(
            MunicipioDaAreaDeAtuacao.Registrar(franca.Id, true, RegiaoDaAreaDeAtuacao.Norte, null, "Area de Atuação.xlsx", 2, 100, agora),
            MunicipioDaAreaDeAtuacao.Registrar(cravinhos.Id, true, RegiaoDaAreaDeAtuacao.Norte, null, "Area de Atuação.xlsx", 3, 100, agora));

        db.ProducoesAgricolasNosMunicipios.AddRange(
            ProducaoAgricolaNoMunicipio.Registrar(franca.Id, 2024, 40139, "Café (em grão) Total", new(30_000m, 29_000m, 50_000m, 900_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(uberaba.Id, 2024, 40124, "Soja (em grão)", new(90_000m, 90_000m, 300_000m, 600_000m), 100, agora));

        db.CotacoesDoDolar.AddRange(
            CotacaoDoDolar.Registrar(new DateOnly(2025, 9, 1), 5.40m, 100, agora),
            CotacaoDoDolar.Registrar(new DateOnly(2026, 8, 1), 5.10m, 100, agora));

        var ibge = Sistema.Criar("IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura");
        var bcb = Sistema.Criar("BCB", "Banco Central — SGS e SICOR", "REST público, somente leitura");
        db.Sistemas.AddRange(ibge, bcb);
        await db.SaveChangesAsync();

        var pam = PontoDeSincronismo.Criar(ibge.Id, "IBGE.PRODUCAO_AGRICOLA", "2024");
        pam.RegistrarRodada("2024", 120, 2, 1, agora);
        var ptax = PontoDeSincronismo.Criar(bcb.Id, "BCB.PTAX_MENSAL", "2026-08");
        ptax.RegistrarRodada("2026-08", 12, 12, 0, agora.AddDays(-60));
        db.PontosDeSincronismo.AddRange(pam, ptax);

        db.MensagensDescartadas.Add(MensagemDescartada.Criar(
            "IBGE.PRODUCAO_AGRICOLA", "{\"municipio\":\"9999999\"}", "Município 9999999 não está no catálogo do IBGE.", 1));

        await db.SaveChangesAsync();
    }

    private static JsonElement Fonte(JsonElement painel, string fluxo) =>
        painel.GetProperty("dados").GetProperty("fontes").EnumerateArray().Single(f => f.GetProperty("fluxo").GetString() == fluxo);

    [Fact]
    public async Task O_painel_mostra_cada_fonte_com_a_situacao_e_so_conta_a_cobertura_da_ADR()
    {
        await SemearAsync();

        // A SITUAÇÃO DAS FONTES É DA GERÊNCIA PARA CIMA (issue 134): o perfil padrão não a vê mais.
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var resposta = await api.ClienteDeRibeirao().GetAsync("/api/v1/integracoes/fontes-publicas");
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        var painel = JsonDocument.Parse(corpo).RootElement;

        painel.GetProperty("dados").GetProperty("fontes").GetArrayLength().Should().Be(FontesPublicas.Todas.Count,
            "toda fonte do catálogo aparece, inclusive as vazias — escondê-las esconderia o problema");
        painel.GetProperty("dados").GetProperty("municipiosDaAdr").GetInt32().Should().Be(2);

        var pam = Fonte(painel, "IBGE.PRODUCAO_AGRICOLA");
        pam.GetProperty("situacao").GetString().Should().Be("EmDia");
        pam.GetProperty("linhas").GetInt32().Should().Be(2);
        pam.GetProperty("periodoInicial").GetString().Should().Be("2024");
        pam.GetProperty("municipiosCobertos").GetInt32().Should().Be(1, "Uberaba tem dado, mas não é da ADR");
        pam.GetProperty("recusasPendentes").GetInt32().Should().Be(1);
        pam.GetProperty("exemplosDeRecusa")[0].GetString().Should().Contain("9999999");
        pam.GetProperty("rotina").GetString().Should().Be("TracbelCrmFontesPublicas");

        var ptax = Fonte(painel, "BCB.PTAX_MENSAL");
        ptax.GetProperty("situacao").GetString().Should().Be("Atrasada", "a última rodada foi há 60 dias");
        ptax.GetProperty("periodoInicial").GetString().Should().Be("2025-09");
        ptax.GetProperty("periodoFinal").GetString().Should().Be("2026-08");
        ptax.GetProperty("municipiosCobertos").ValueKind.Should().Be(JsonValueKind.Null, "o dólar não é por município");

        var conab = Fonte(painel, "CONAB.PRECO_RECEBIDO");
        conab.GetProperty("situacao").GetString().Should().Be("SemDado");
        conab.GetProperty("motivo").GetString().Should().Contain("nunca rodou");

        painel.GetProperty("dados").GetProperty("atrasadas").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        painel.GetProperty("dados").GetProperty("semDado").GetInt32().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task As_opcoes_trazem_os_produtos_da_PAM_pela_area_na_ADR_e_os_municipios_da_ADR()
    {
        await SemearAsync();

        var resposta = await api.ClienteDeRibeirao().GetAsync("/api/v1/admin/parametros-do-potencial/opcoes");
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        var dados = JsonDocument.Parse(corpo).RootElement.GetProperty("dados");

        var produtos = dados.GetProperty("produtos").EnumerateArray().ToList();
        produtos[0].GetProperty("codigoIbge").GetInt32().Should().Be(40139, "o café é o que mais se planta na ADR semeada");
        produtos[0].GetProperty("areaPlantadaNaAdrHectares").GetDecimal().Should().Be(30_000m);
        produtos.Single(p => p.GetProperty("codigoIbge").GetInt32() == 40124)
            .GetProperty("areaPlantadaNaAdrHectares").ValueKind.Should().Be(JsonValueKind.Null,
                "a soja semeada é de Uberaba, fora da ADR — o produto aparece, sem área na ADR");

        dados.GetProperty("municipios").EnumerateArray().Select(m => m.GetProperty("nome").GetString())
            .Should().Equal("Cravinhos", "Franca");
    }
}
