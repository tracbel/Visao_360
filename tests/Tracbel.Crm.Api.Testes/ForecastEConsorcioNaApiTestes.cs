using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O FORECAST DA GERÊNCIA E O REALIZADO DE CONSÓRCIO POR HTTP (28/09/2026), com a aplicação inteira de pé.
///
/// <para>Sobre o cenário das metas (<see cref="MetasNaApiTestes.SemearAsync"/>): o de-para põe o CEN de Ribeirão e o de
/// Barretos no time do Norte, e o consultor sem conta no do Sul; o forecast de nov/2025 tem o Norte com os dois números e o
/// Sul só com o best guess, e dez/2025 tem o Norte. As cotas de consórcio, em jan/2026: duas em Ribeirão (uma do CEN, uma
/// sem conta), uma em Barretos e uma excluída.</para>
///
/// <para>O que se prende: o forecast é da gerência (o padrão leva 403); o PO e o realizado do gestor são os do time dele,
/// pela filial escolhida — e, em "Todas as filiais", da organização; o nulo do forecast fica nulo; o realizado de consórcio
/// entra no cartão da meta por filial e por pessoa, e é nulo enquanto não foi lido.</para>
/// </summary>
[Trait("Categoria", "Metas")]
public sealed class ForecastEConsorcioNaApiTestes
{
    private const string Rota = "/api/v1/relatorios/forecast";
    private const string RotaDasMetas = "/api/v1/relatorios/metas?competenciaInicial=2025-11&competenciaFinal=2026-08";

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static IEnumerable<string?> Lacunas(JsonElement dados) =>
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString());

    private static CrmDbContext Banco(ApiEmMemoria app, IServiceScope escopo) =>
        new(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);

    private static async Task<ApiEmMemoria> AppAsync(bool comPlanejamento = true)
    {
        var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await MetasNaApiTestes.SemearAsync(app);
        if (comPlanejamento) await SemearPlanejamentoAsync(app);
        return app;
    }

    private static async Task SemearPlanejamentoAsync(ApiEmMemoria app)
    {
        using var escopo = app.Services.CreateScope();
        await using var db = Banco(app, escopo);
        var gn = await db.Sistemas.SingleAsync(s => s.Codigo == ConexoesDoSistema.GestaoDeNegocios);
        var leitura = new LeituraDoPlanejamento(new DateTime(2026, 9, 28, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 28, 4, 30, 0, DateTimeKind.Utc));

        db.GestoresDosConsultores.AddRange(
            GestorDoConsultor.Registrar(gn.Id, 1, "CEN.RIBEIRAOPRETO", "GESTOR.NORTE", 1, null, "g1", leitura, 100),
            GestorDoConsultor.Registrar(gn.Id, 2, "OUTRO.SEM.CONTA", "GESTOR.SUL", 1, null, "g2", leitura, 100),
            GestorDoConsultor.Registrar(gn.Id, 3, "CEN.BARRETOS", "GESTOR.NORTE", 3, null, "g3", leitura, 100),
            GestorDoConsultor.Registrar(gn.Id, 4, "OUTRO.DE.BARRETOS", "GESTOR.OESTE", 3, null, "g4", leitura, 100));

        // OUT/2026 É O DIA 1º DE 01/10/2026: o Oeste (só Barretos) já digitou o forecast, ninguém digitou o best guess, e o
        // Norte tem PG em Ribeirão (a meta de out/2026 do CEN) sem forecast.
        db.ForecastsDaGerencia.AddRange(
            ForecastDaGerencia.Registrar(gn.Id, 1, new DateOnly(2025, 11, 1), "GESTOR.NORTE", "TRATOR MÉDIO", 6, 5, "f1", leitura, 100),
            ForecastDaGerencia.Registrar(gn.Id, 2, new DateOnly(2025, 11, 1), "GESTOR.SUL", "TRATOR MÉDIO", null, 1, "f2", leitura, 100),
            ForecastDaGerencia.Registrar(gn.Id, 3, new DateOnly(2025, 12, 1), "GESTOR.NORTE", "TRATOR MÉDIO", 4, 4, "f3", leitura, 100),
            ForecastDaGerencia.Registrar(gn.Id, 4, new DateOnly(2026, 10, 1), "GESTOR.OESTE", "TRATOR MÉDIO", 7, null, "f4", leitura, 100));

        DadosDaCotaNaOrigem Cota(int empresa, string consultor, long? conta) => new(
            empresa, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 12), "Lance", null, consultor, conta, "GESTOR.NORTE", "TRATOR",
            250_000m, 3_100m, $"c-{empresa}-{consultor}");

        var excluida = CotaDeConsorcioVendida.Registrar(gn.Id, "1000", "9", Cota(1, "CEN.RIBEIRAOPRETO", 100), leitura, 100);
        excluida.Excluir(100);
        db.CotasDeConsorcioVendidas.AddRange(
            CotaDeConsorcioVendida.Registrar(gn.Id, "1000", "1", Cota(1, "CEN.RIBEIRAOPRETO", 100), leitura, 100),
            CotaDeConsorcioVendida.Registrar(gn.Id, "1000", "2", Cota(1, "OUTRO.SEM.CONTA", null), leitura, 100),
            CotaDeConsorcioVendida.Registrar(gn.Id, "1000", "3", Cota(2, "CEN.BARRETOS", 200), leitura, 100),
            excluida);

        db.PontosDeSincronismo.AddRange(
            PontoDeSincronismo.Criar(gn.Id, ForecastDaGerencia.FluxoDaCarga, "2026-09-28T04:30:00.0000000Z"),
            PontoDeSincronismo.Criar(gn.Id, CotaDeConsorcioVendida.FluxoDaCarga, "2026-09-28T04:30:00.0000000Z"));
        await db.SaveChangesAsync();
    }

    private static Dictionary<string, JsonElement> Gestores(JsonElement dados) =>
        dados.GetProperty("gestores").EnumerateArray().ToDictionary(g => g.GetProperty("gestor").GetString()!);

    private static JsonElement Linha(JsonElement gestorOuTotal, string codigo) =>
        (gestorOuTotal.ValueKind == JsonValueKind.Array ? gestorOuTotal : gestorOuTotal.GetProperty("linhas"))
        .EnumerateArray().Single(l => l.GetProperty("codigo").GetString() == codigo);

    private static int? Numero(JsonElement linha, string campo) =>
        linha.GetProperty(campo).ValueKind == JsonValueKind.Null ? null : linha.GetProperty(campo).GetInt32();

    // =============================================================================================
    // O forecast
    // =============================================================================================

    [Fact]
    public async Task A_gerencia_ve_cada_gestor_com_o_po_e_o_realizado_do_time_na_filial()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota + "?competencia=2025-11"));

        dados.GetProperty("competencia").GetString().Should().Be("2025-11-01");
        dados.GetProperty("alcance").GetString().Should().Be("Filiais");
        dados.GetProperty("mesesDisponiveis").EnumerateArray().Select(m => m.GetString()).Should().Equal("2025-11-01", "2025-12-01", "2026-10-01");

        var gestores = Gestores(dados);
        gestores.Keys.Should().Equal("GESTOR.NORTE", "GESTOR.SUL");
        gestores["GESTOR.NORTE"].GetProperty("consultores").GetInt32().Should().Be(2, "o CEN de Ribeirão e o de Barretos");

        var norte = Linha(gestores["GESTOR.NORTE"], "TRATOR_MEDIO");
        (Numero(norte, "meta"), Numero(norte, "forecast"), Numero(norte, "bestGuess"), Numero(norte, "realizado"))
            .Should().Be(((int?)3, (int?)6, (int?)5, (int?)1), "o PO e o realizado são só de Ribeirão; a meta e a venda de Barretos são de lá");

        var sul = Linha(gestores["GESTOR.SUL"], "TRATOR_MEDIO");
        (Numero(sul, "meta"), Numero(sul, "forecast"), Numero(sul, "bestGuess"), Numero(sul, "realizado"))
            .Should().Be(((int?)0, (int?)null, (int?)1, (int?)0), "o Sul não informou o forecast: nulo, e não zero");

        var total = Linha(dados.GetProperty("total"), "TRATOR_MEDIO");
        (Numero(total, "forecast"), Numero(total, "bestGuess")).Should().Be(((int?)6, (int?)6), "o nulo não soma; o best guess é 5 + 1");
        dados.GetProperty("vendasSemGestor").GetInt32().Should().Be(0);
        dados.GetProperty("lidoEm").ValueKind.Should().NotBe(JsonValueKind.Null);

        Lacunas(dados).Should().Contain(["alcanceDasFiliais", "observacao"]).And.NotContain("forecastNaoLido");
    }

    [Fact]
    public async Task Em_todas_as_filiais_a_diretoria_ve_o_time_inteiro()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Diretoria);

        var dados = await DadosAsync(await app.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ContextoAcesso.CodigoDeTodasAsFiliais)
            .GetAsync(Rota + "?competencia=2025-11"));

        dados.GetProperty("alcance").GetString().Should().Be("Organizacao");
        var norte = Linha(Gestores(dados)["GESTOR.NORTE"], "TRATOR_MEDIO");
        (Numero(norte, "meta"), Numero(norte, "realizado")).Should().Be(((int?)7, (int?)2), "3 + 4 de meta, e as vendas de Ribeirão e de Barretos");
        Lacunas(dados).Should().NotContain("alcanceDasFiliais");
    }

    [Fact]
    public async Task Na_filial_o_gestor_de_outra_filial_fica_fora_e_e_contado()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota + "?competencia=2026-10"));

        // O OESTE SÓ TEM BARRETOS: o forecast dele não entra na tela de Ribeirão, nem no total — em 01/10/2026, a filial de
        // Ribeirão via gestores de Franca com PG zero ao lado.
        var gestores = Gestores(dados);
        gestores.Keys.Should().Equal(["GESTOR.NORTE"], "o Norte tem a meta de out/2026 em Ribeirão; o Oeste é só de Barretos");
        var norte = Linha(gestores["GESTOR.NORTE"], "TRATOR_MEDIO");
        (Numero(norte, "meta"), Numero(norte, "forecast"), Numero(norte, "bestGuess")).Should().Be(((int?)9, (int?)null, (int?)null));
        Numero(Linha(dados.GetProperty("total"), "TRATOR_MEDIO"), "forecast").Should().BeNull("o forecast do Oeste ficou fora");

        Lacunas(dados).Should().Contain(["gestoresForaDoRecorte", "gestoresSemForecast", "semBestGuessNoMes", "alcanceDasFiliais"]);
        var fora = dados.GetProperty("metricasSemDado").EnumerateArray().Single(m => m.GetProperty("metrica").GetString() == "gestoresForaDoRecorte");
        fora.GetProperty("motivo").GetString().Should().StartWith("1 gestor(es) com forecast em out/2026").And.Contain("Todas as filiais");
    }

    [Fact]
    public async Task Em_todas_as_filiais_o_gestor_de_outra_filial_entra_com_o_forecast()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Diretoria);

        var dados = await DadosAsync(await app.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ContextoAcesso.CodigoDeTodasAsFiliais)
            .GetAsync(Rota + "?competencia=2026-10"));

        Gestores(dados).Keys.Should().Equal("GESTOR.NORTE", "GESTOR.OESTE");
        Numero(Linha(dados.GetProperty("total"), "TRATOR_MEDIO"), "forecast").Should().Be(7);
        Lacunas(dados).Should().NotContain(["gestoresForaDoRecorte", "alcanceDasFiliais"]).And.Contain("semBestGuessNoMes");
    }

    [Fact]
    public async Task Dados_atualizados_e_a_hora_da_leitura_da_gn_e_nao_a_da_consulta()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var resposta = await app.ClienteDeRibeirao().GetAsync(Rota + "?competencia=2025-11");
        var raiz = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;
        var procedencia = raiz.GetProperty("procedencia");

        procedencia.GetProperty("lidoEmUtc").GetDateTime().Should().Be(raiz.GetProperty("dados").GetProperty("lidoEm").GetDateTime(),
            "em 01/10/2026 a tela disse 11:32 com o forecast lido às 06:00");
        procedencia.GetProperty("estaDesatualizado").GetBoolean().Should().BeFalse("a leitura acabou de acontecer");
    }

    [Fact]
    public void Leitura_com_mais_de_tres_horas_avisa_com_a_hora_de_sao_paulo()
    {
        var lido = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);

        var fresca = ObterForecastDaGerencia.ProcedenciaDaLeitura(lido, lido.AddHours(2));
        (fresca.LidoEmUtc, fresca.EstaDesatualizado, fresca.Aviso).Should().Be((lido, false, (string?)null));

        var velha = ObterForecastDaGerencia.ProcedenciaDaLeitura(lido, lido.AddHours(5));
        velha.EstaDesatualizado.Should().BeTrue();
        velha.Aviso.Should().Contain("01/10/2026 às 06:00").And.Contain("há mais de 3 horas");

        var agora = new DateTime(2026, 10, 1, 14, 0, 0, DateTimeKind.Utc);
        var nunca = ObterForecastDaGerencia.ProcedenciaDaLeitura(null, agora);
        (nunca.LidoEmUtc, nunca.EstaDesatualizado).Should().Be((agora, false), "sem leitura, a lacuna forecastNaoLido diz o resto");
    }

    [Fact]
    public async Task Sem_leitura_o_forecast_diz_que_nao_foi_lido_e_as_vendas_ficam_sem_gestor()
    {
        await using var app = await AppAsync(comPlanejamento: false);
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota + "?competencia=2025-11"));

        dados.GetProperty("gestores").GetArrayLength().Should().Be(0, "sem o de-para, ninguém tem gestor");
        dados.GetProperty("vendasSemGestor").GetInt32().Should().Be(1, "a venda de nov/2025 em Ribeirão entra só no total");
        dados.GetProperty("lidoEm").ValueKind.Should().Be(JsonValueKind.Null);
        Lacunas(dados).Should().Contain(["forecastNaoLido", "vendasSemGestor"]);
    }

    [Fact]
    public async Task O_padrao_nao_ve_o_forecast_da_gerencia()
    {
        await using var app = await AppAsync();

        (await app.ClienteDeRibeirao().GetAsync(Rota)).StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "quem só vê a própria meta não vê a previsão dos gestores");
    }

    [Fact]
    public async Task Mes_que_nao_vale_e_recusado_com_o_campo_nomeado()
    {
        await using var app = await AppAsync(comPlanejamento: false);
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var resposta = await app.ClienteDeRibeirao().GetAsync(Rota + "?competencia=2025-13");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("erros")
            .EnumerateArray().Select(e => e.GetProperty("campo").GetString()).Should().Contain("competencia");
    }

    [Theory]
    [InlineData(new[] { "2026-08-01", "2026-09-01", "2026-10-01" }, "2026-09-01", "2026-09-01")]
    [InlineData(new[] { "2026-07-01", "2026-08-01" }, "2026-09-01", "2026-08-01")]
    [InlineData(new[] { "2026-10-01", "2026-11-01" }, "2026-09-01", "2026-11-01")]
    [InlineData(new string[0], "2026-09-01", "2026-09-01")]
    public void O_mes_padrao_e_o_corrente_senao_o_ultimo_ate_ele_senao_o_mais_recente(string[] meses, string corrente, string esperado) =>
        ObterForecastDaGerencia.MesPadrao([.. meses.Select(DateOnly.Parse)], DateOnly.Parse(corrente)).Should().Be(DateOnly.Parse(esperado));

    // =============================================================================================
    // O realizado de consórcio no cartão da meta
    // =============================================================================================

    [Fact]
    public async Task O_realizado_de_consorcio_entra_na_meta_pela_filial_e_pela_pessoa()
    {
        await using var app = await AppAsync();

        // O PADRÃO: só a cota que a carga casou com a conta dele.
        var proprio = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(RotaDasMetas));
        proprio.GetProperty("totais").GetProperty("realizadoConsorcio").GetInt32().Should().Be(1);

        // A GERÊNCIA: as duas de Ribeirão, com e sem conta; a de Barretos é de lá, e a excluída não conta.
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        var filial = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(RotaDasMetas));
        filial.GetProperty("totais").GetProperty("realizadoConsorcio").GetInt32().Should().Be(2);
        filial.GetProperty("porMes").EnumerateArray()
            .Single(m => m.GetProperty("competencia").GetString() == "2026-01-01").GetProperty("realizadoConsorcio").GetInt32().Should().Be(2);

        var consorcio = filial.GetProperty("metricasSemDado").EnumerateArray().Single(m => m.GetProperty("metrica").GetString() == "consorcio");
        consorcio.GetProperty("motivo").GetString().Should().Contain("2 cota(s) vendida(s) de uma meta de 5").And.Contain("Digital");
    }

    [Fact]
    public async Task Sem_leitura_o_realizado_de_consorcio_e_nulo_e_nao_zero()
    {
        await using var app = await AppAsync(comPlanejamento: false);

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(RotaDasMetas));

        dados.GetProperty("totais").GetProperty("realizadoConsorcio").ValueKind.Should().Be(JsonValueKind.Null);
        dados.GetProperty("metricasSemDado").EnumerateArray().Single(m => m.GetProperty("metrica").GetString() == "consorcio")
            .GetProperty("motivo").GetString().Should().Contain("ainda não foi lido");
    }
}
