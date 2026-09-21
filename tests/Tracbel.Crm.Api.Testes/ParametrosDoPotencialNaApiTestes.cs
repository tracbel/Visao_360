using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// OS PARÂMETROS DO POTENCIAL PELA API DE VERDADE (issue 71). O aceite da issue, escrito como teste:
/// <list type="number">
///   <item>parâmetro alterado gera trilha com o autor;</item>
///   <item>o cálculo de uma data passada usa o parâmetro vigente naquela data;</item>
///   <item>usuário sem permissão recebe 403.</item>
/// </list>
///
/// <para><b>Os dois usuários têm papéis fixos nesta classe</b>, porque o banco em memória é um só para todos
/// os testes dela: Barretos (200) fica com o perfil padrão e prova o 403; Ribeirão (100) recebe o perfil de
/// administrador e registra. Cada teste usa a sua data de início, para um não ocupar a do outro.</para>
/// </summary>
[Trait("Categoria", "ParametrosDoPotencial")]
public sealed class ParametrosDoPotencialNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Base = "/api/v1/admin/parametros-do-potencial";
    private const int Cafe = 40139;
    private const int Franca = 3516200;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static DateOnly Hoje => ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow);

    private static string Iso(DateOnly data) => data.ToString("yyyy-MM-dd");

    /// <summary>O café na PAM (a regra divide a área dele) e um município para a percepção.</summary>
    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.Municipios.AnyAsync(m => m.CodigoIbge == Franca)) return;

        var franca = Municipio.Criar("Franca", "SP", Franca);
        db.Municipios.Add(franca);
        await db.SaveChangesAsync();

        db.ProducoesAgricolasNosMunicipios.Add(ProducaoAgricolaNoMunicipio.Registrar(
            franca.Id, 2024, Cafe, "Café (em grão) Total", new(30_000m, 29_000m, 50_000m, 900_000m), 100, DateTime.UtcNow));
        await db.SaveChangesAsync();
    }

    private async Task<HttpClient> AdministradorAsync()
    {
        await SemearAsync();
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        return api.ClienteDeRibeirao();
    }

    private static object RegraDoCafe(decimal hectares, DateOnly vigenteDesde, string? anos = null) => new
    {
        produtoCodigoIbge = Cafe.ToString(),
        hectaresPorMaquina = hectares.ToString(System.Globalization.CultureInfo.InvariantCulture),
        anosDeRenovacao = anos,
        modeloDeReferencia = "3036N",
        situacao = "AConfirmar",
        vigenteDesde = Iso(vigenteDesde),
        justificativa = "planilha 360, aba Administrador"
    };

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta, HttpStatusCode esperado)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(esperado, corpo);
        return JsonDocument.Parse(corpo).RootElement.Clone();
    }

    private static decimal? HectaresDoCafe(JsonElement vigentes) =>
        vigentes.GetProperty("dados").GetProperty("culturas").EnumerateArray()
            .Where(c => c.GetProperty("produtoCodigoIbge").GetInt32() == Cafe)
            .Select(c => (decimal?)c.GetProperty("hectaresPorMaquina").GetDecimal())
            .SingleOrDefault();

    [Fact]
    public async Task Sem_permissao_le_os_parametros_e_recebe_403_ao_alterar()
    {
        await SemearAsync();
        var barretos = api.ClienteDeBarretos();

        // O PADRÃO LÊ: todo número do potencial sai com o parâmetro que o gerou.
        var leitura = await LerAsync(await barretos.GetAsync(Base), HttpStatusCode.OK);
        leitura.GetProperty("dados").GetProperty("geral").GetProperty("mesesDaJanela").GetInt32().Should().Be(12);

        var tentativas = new[]
        {
            await barretos.PostAsJsonAsync($"{Base}/culturas", RegraDoCafe(20m, Hoje.AddDays(5)), Json),
            await barretos.PostAsJsonAsync($"{Base}/geral", new { vigenteDesde = Iso(Hoje.AddDays(5)) }, Json),
            await barretos.PostAsJsonAsync($"{Base}/percepcoes", new { municipioCodigoIbge = Franca.ToString(), percentual = "2" }, Json),
            await barretos.PostAsJsonAsync($"{Base}/culturas/{Cafe}/2026-09-13/revogacao", new { motivo = "x" }, Json)
        };

        foreach (var resposta in tentativas)
        {
            var corpo = await resposta.Content.ReadAsStringAsync();
            resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, corpo);
            corpo.Should().Contain("sem-acesso").And.Contain("Falta a permissão");
        }
    }

    [Fact]
    public async Task O_calculo_de_uma_data_usa_o_parametro_vigente_naquela_data()
    {
        var http = await AdministradorAsync();
        var amanha = Hoje.AddDays(1);

        // A semente: o café a 10 ha desde 13/09/2026.
        HectaresDoCafe(await LerAsync(await http.GetAsync($"{Base}?em={Iso(Hoje)}"), HttpStatusCode.OK)).Should().Be(10m);

        await LerAsync(await http.PostAsJsonAsync($"{Base}/culturas", RegraDoCafe(20m, amanha, anos: "10"), Json), HttpStatusCode.Created);

        HectaresDoCafe(await LerAsync(await http.GetAsync($"{Base}?em={Iso(Hoje)}"), HttpStatusCode.OK))
            .Should().Be(10m, "a vigência nova só começa amanhã: o cálculo de hoje não muda");
        HectaresDoCafe(await LerAsync(await http.GetAsync($"{Base}?em={Iso(amanha)}"), HttpStatusCode.OK))
            .Should().Be(20m, "a partir de amanhã vale a vigência nova");

        var antesDaSemente = await LerAsync(await http.GetAsync($"{Base}?em=2026-09-12"), HttpStatusCode.OK);
        HectaresDoCafe(antesDaSemente).Should().BeNull("antes de 13/09/2026 não havia regra do café");
        antesDaSemente.GetProperty("dados").GetProperty("geral").ValueKind.Should().Be(JsonValueKind.Null,
            "os parâmetros gerais valem desde 21/09/2026");
        antesDaSemente.GetProperty("dados").GetProperty("pendencias").EnumerateArray().Select(p => p.GetString())
            .Should().Contain(p => p!.Contains("Não há parâmetros gerais vigentes"));
    }

    [Fact]
    public async Task Parametro_alterado_gera_trilha_com_o_autor_e_a_revogacao_tambem()
    {
        var http = await AdministradorAsync();
        var inicio = Hoje.AddDays(30);

        await LerAsync(await http.PostAsJsonAsync($"{Base}/culturas", RegraDoCafe(25m, inicio), Json), HttpStatusCode.Created);

        int regraId;
        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            regraId = (await db.RegrasDePotencial.SingleAsync(r => r.VigenteDesde == inicio && r.ProdutoCodigoIbge == Cafe)).Id;
        }

        var inclusao = await TrilhaAsync(regraId);
        inclusao.Should().NotBeEmpty("uma pessoa registrou o parâmetro: a vigência nasce na trilha");
        inclusao.Should().AllSatisfy(l =>
        {
            l.AlteradoPorId.Should().Be(100, "a trilha diz quem decidiu");
            l.Origem.Should().Be(OrigemDaOperacao.Usuario);
            l.Operacao.Should().Be(OperacaoAuditada.Inclusao);
        });
        inclusao.Should().Contain(l => l.Campo == "HectaresPorMaquina" && l.ValorNovo == "25");
        inclusao.Should().Contain(l => l.Campo == "VigenteDesde" && l.ValorNovo == Iso(inicio));
        inclusao.Should().Contain(l => l.Campo == "Justificativa" && l.ValorNovo == "planilha 360, aba Administrador");

        var revogada = await LerAsync(
            await http.PostAsJsonAsync($"{Base}/culturas/{Cafe}/{Iso(inicio)}/revogacao", new { motivo = "a planilha estava errada" }, Json),
            HttpStatusCode.OK);
        revogada.GetProperty("vigencia").GetProperty("motivoDaRevogacao").GetString().Should().Be("a planilha estava errada");
        revogada.GetProperty("vigencia").GetProperty("revogadoPor").GetString().Should().NotBeNullOrWhiteSpace();

        var revogacao = (await TrilhaAsync(regraId)).Skip(inclusao.Count).ToList();
        revogacao.Should().Contain(l => l.Campo == "MotivoDaRevogacao" && l.ValorNovo == "a planilha estava errada" && l.AlteradoPorId == 100);
        revogacao.Should().Contain(l => l.Campo == "RevogadoEm" && l.ValorAnterior == null && l.ValorNovo != null);

        // A DATA FICA LIVRE: revogar libera o início para a vigência que corrige o erro.
        await LerAsync(await http.PostAsJsonAsync($"{Base}/culturas", RegraDoCafe(22m, inicio), Json), HttpStatusCode.Created);
    }

    [Fact]
    public async Task O_passado_nao_se_reescreve_nem_se_revoga()
    {
        var http = await AdministradorAsync();

        var passada = await LerAsync(
            await http.PostAsJsonAsync($"{Base}/culturas", RegraDoCafe(20m, Hoje.AddDays(-1)), Json), HttpStatusCode.UnprocessableEntity);
        passada.GetProperty("erros").EnumerateArray().Should().Contain(e => e.GetProperty("campo").GetString() == "vigenteDesde");

        // A semente do café vale desde 13/09/2026 e já foi usada em dias que passaram.
        var revogarSemente = await LerAsync(
            await http.PostAsJsonAsync($"{Base}/culturas/{Cafe}/2026-09-13/revogacao", new { motivo = "tarde demais" }, Json),
            HttpStatusCode.Conflict);
        revogarSemente.GetProperty("title").GetString().Should().Contain("vigência nova a partir de hoje");
    }

    [Fact]
    public async Task A_mesma_data_nao_recebe_duas_vigencias_de_pe()
    {
        var http = await AdministradorAsync();
        var inicio = Hoje.AddDays(60);

        await LerAsync(await http.PostAsJsonAsync($"{Base}/culturas", RegraDoCafe(18m, inicio), Json), HttpStatusCode.Created);
        var repetida = await LerAsync(await http.PostAsJsonAsync($"{Base}/culturas", RegraDoCafe(19m, inicio), Json), HttpStatusCode.Conflict);

        repetida.GetProperty("erros").EnumerateArray().Single().GetProperty("mensagem").GetString().Should().Contain("revogue-a antes");
    }

    [Fact]
    public async Task Produto_fora_da_PAM_e_numero_invalido_sao_recusados_campo_a_campo()
    {
        var http = await AdministradorAsync();

        var recusa = await LerAsync(await http.PostAsJsonAsync($"{Base}/culturas", new
        {
            produtoCodigoIbge = "99999",
            hectaresPorMaquina = "vinte",
            modeloDeReferencia = "3036N",
            situacao = "Talvez",
            vigenteDesde = "01/10/2026",
            justificativa = "x"
        }, Json), HttpStatusCode.UnprocessableEntity);

        recusa.GetProperty("erros").EnumerateArray().Select(e => e.GetProperty("campo").GetString())
            .Should().BeEquivalentTo(["produtoCodigoIbge", "hectaresPorMaquina", "situacao", "vigenteDesde"]);
    }

    [Fact]
    public async Task Os_parametros_gerais_aceitam_virgula_e_a_percepcao_respeita_o_limite_vigente()
    {
        var http = await AdministradorAsync();
        var inicio = Hoje.AddDays(90);

        var geral = await LerAsync(await http.PostAsJsonAsync($"{Base}/geral", new
        {
            vigenteDesde = Iso(inicio),
            mesesDaJanela = "12",
            pesoDosContratosNoCredito = "0,70",
            limiteDeRetracao = "1,00",
            limiteDeAquecimento = "1,20",
            limiteDeSuperaquecimento = "1,40",
            nomeDaFaixaIntermediaria = "Anual",
            limiteDaPercepcao = "3",
            pesoDoIndicadorDePreco = "0,4",
            pesoDoIndicadorDeCredito = "0,5",
            pesoDoIndicadorComercial = "1",
            fatorMinimo = "0,4",
            fatorMaximo = "1,5",
            justificativa = "decisão da diretoria (exemplo de teste)"
        }, Json), HttpStatusCode.Created);
        geral.GetProperty("pesoDoValorNoCredito").GetDecimal().Should().Be(0.30m, "70% contratos, 30% valor");

        // O LIMITE É O DA VIGÊNCIA NA DATA DE INÍCIO: ±5 hoje, ±3 a partir da nova.
        var hoje = await http.PostAsJsonAsync($"{Base}/percepcoes", new
        {
            municipioCodigoIbge = Franca.ToString(), percentual = "-4,5", vigenteDesde = Iso(Hoje.AddDays(2)), justificativa = "geada"
        }, Json);
        var percepcao = await LerAsync(hoje, HttpStatusCode.Created);
        percepcao.GetProperty("municipioNome").GetString().Should().Be("Franca");

        var depois = await LerAsync(await http.PostAsJsonAsync($"{Base}/percepcoes", new
        {
            municipioCodigoIbge = Franca.ToString(), percentual = "-4,5", vigenteDesde = Iso(inicio), justificativa = "geada"
        }, Json), HttpStatusCode.UnprocessableEntity);
        depois.GetProperty("erros").EnumerateArray().Single().GetProperty("campo").GetString().Should().Be("percentual");

        var vigentes = await LerAsync(await http.GetAsync($"{Base}?em={Iso(inicio)}"), HttpStatusCode.OK);
        vigentes.GetProperty("dados").GetProperty("geral").GetProperty("nomeDaFaixaIntermediaria").GetString().Should().Be("Anual");
        vigentes.GetProperty("dados").GetProperty("percepcoes").EnumerateArray().Single().GetProperty("percentual").GetDecimal().Should().Be(-4.5m);
        vigentes.GetProperty("dados").GetProperty("pendencias").EnumerateArray().Select(p => p.GetString())
            .Should().NotContain(p => p!.Contains("pesos dos três indicadores"), "nesta vigência os pesos foram decididos");

        var historico = await LerAsync(await http.GetAsync($"{Base}/historico"), HttpStatusCode.OK);
        historico.GetProperty("dados").GetProperty("gerais").GetArrayLength().Should().BeGreaterThanOrEqualTo(2, "a semente e a nova");
    }

    private async Task<List<AlteracaoDeCampo>> TrilhaAsync(int regraId)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await db.AlteracoesDeCampo
            .Where(a => a.Entidade == nameof(RegraDePotencial) && a.RegistroId == regraId)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }
}
