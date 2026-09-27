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
/// OS PARÂMETROS DO PLANEJAMENTO PELA API DE VERDADE (issue 256): a semente do protótipo sai marcada como a confirmar,
/// a vigência de uma data é a daquela data, a alteração gera trilha com o autor e quem não administra recebe 403.
///
/// <para>Como na classe dos parâmetros do potencial, Barretos (200) fica com o perfil padrão e Ribeirão (100) recebe o
/// de administrador; cada teste usa a sua data de início.</para>
/// </summary>
[Trait("Categoria", "ParametrosDoPotencial")]
public sealed class ParametrosDoPlanejamentoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Base = "/api/v1/admin/parametros-do-potencial/planejamento";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly string[] SazonalidadeDoPrototipo =
        ["6.52", "6.89", "8.56", "8.59", "8.87", "8.58", "7.84", "8.75", "9.15", "10.51", "7.44", "8.30"];

    private static DateOnly Hoje => ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow);

    private static string Iso(DateOnly data) => data.ToString("yyyy-MM-dd");

    private async Task<HttpClient> AdministradorAsync()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        return api.ClienteDeRibeirao();
    }

    private static object Planejamento(DateOnly vigenteDesde, string[]? meses = null, string potencial = "30") => new
    {
        vigenteDesde = Iso(vigenteDesde),
        sazonalidade = meses ?? SazonalidadeDoPrototipo,
        pesoDoPotencial = potencial,
        pesoDaCobertura = "20",
        pesoDoCredito = "15",
        pesoDaRentabilidade = "15",
        pesoDosClientes = "10",
        pesoDaRealizacao = "0",
        pesoDaPenetracao = "10",
        justificativa = "reunião do comercial"
    };

    private static object Share(string categoria, string percentual, DateOnly vigenteDesde) => new
    {
        categoriaDeMaquinaCodigo = categoria,
        percentual,
        vigenteDesde = Iso(vigenteDesde),
        justificativa = "meta da diretoria"
    };

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta, HttpStatusCode esperado)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(esperado, corpo);
        return JsonDocument.Parse(corpo).RootElement.Clone();
    }

    private static decimal? ShareDe(JsonElement vigentes, string categoria) =>
        vigentes.GetProperty("dados").GetProperty("shares").EnumerateArray()
            .Where(s => s.GetProperty("categoriaDeMaquinaCodigo").GetString() == categoria)
            .Select(s => (decimal?)s.GetProperty("percentual").GetDecimal())
            .SingleOrDefault();

    private static List<string> Pendencias(JsonElement vigentes) =>
        [.. vigentes.GetProperty("dados").GetProperty("pendencias").EnumerateArray().Select(p => p.GetString()!)];

    [Fact]
    public async Task A_semente_do_prototipo_vale_desde_27_09_e_sai_marcada_como_a_confirmar()
    {
        var leitura = await LerAsync(await api.ClienteDeBarretos().GetAsync($"{Base}?em=2026-09-27"), HttpStatusCode.OK);
        var dados = leitura.GetProperty("dados");

        dados.GetProperty("planejamento").GetProperty("sazonalidade").EnumerateArray().Select(m => m.GetDecimal())
            .Should().Equal(6.52m, 6.89m, 8.56m, 8.59m, 8.87m, 8.58m, 7.84m, 8.75m, 9.15m, 10.51m, 7.44m, 8.30m);
        dados.GetProperty("planejamento").GetProperty("pesos").GetProperty("potencial").GetDecimal().Should().Be(25m);
        dados.GetProperty("planejamento").GetProperty("vigencia").GetProperty("informadoPor").ValueKind.Should().Be(JsonValueKind.Null);

        // NA ORDEM DO CATÁLOGO: trator, plantadeira, colheitadeira, pulverizador… — a colhedora de cana por último.
        dados.GetProperty("shares").EnumerateArray().Select(s => s.GetProperty("categoriaDeMaquinaCodigo").GetString())
            .Should().Equal("TRATOR", "PLANTADEIRA", "COLHEITADEIRA", "PULVERIZADOR", "COLHEDORA_DE_CANA");
        ShareDe(leitura, "TRATOR").Should().Be(31m);

        var pendencias = Pendencias(leitura);
        pendencias.Should().Contain(p => p.Contains("ainda são os do protótipo"));
        pendencias.Should().Contain(p => p.Contains("O share-alvo de Trator,") && p.Contains("a confirmar"));
        pendencias.Should().Contain(p => p.Contains("Implemento e Agricultura de precisão: sem share-alvo"),
            "o protótipo não planejava essas duas, e o CRM não inventa share para elas");

        var antes = await LerAsync(await api.ClienteDeBarretos().GetAsync($"{Base}?em=2026-09-26"), HttpStatusCode.OK);
        antes.GetProperty("dados").GetProperty("planejamento").ValueKind.Should().Be(JsonValueKind.Null);
        Pendencias(antes).Should().Contain(p => p.Contains("Não há sazonalidade nem pesos do IOC vigentes"));
    }

    [Fact]
    public async Task Sem_permissao_le_e_recebe_403_ao_alterar()
    {
        var barretos = api.ClienteDeBarretos();
        await LerAsync(await barretos.GetAsync($"{Base}/historico"), HttpStatusCode.OK);

        var tentativas = new[]
        {
            await barretos.PostAsJsonAsync(Base, Planejamento(Hoje.AddDays(5)), Json),
            await barretos.PostAsJsonAsync($"{Base}/shares", Share("TRATOR", "35", Hoje.AddDays(5)), Json),
            await barretos.PostAsJsonAsync($"{Base}/2026-09-27/revogacao", new { motivo = "x" }, Json),
            await barretos.PostAsJsonAsync($"{Base}/shares/TRATOR/2026-09-27/revogacao", new { motivo = "x" }, Json)
        };

        foreach (var resposta in tentativas)
        {
            var corpo = await resposta.Content.ReadAsStringAsync();
            resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, corpo);
            corpo.Should().Contain("Falta a permissão");
        }
    }

    [Fact]
    public async Task O_share_de_uma_data_e_o_que_valia_nela_e_a_alteracao_gera_trilha_com_o_autor()
    {
        var http = await AdministradorAsync();
        var inicio = Hoje.AddDays(10);

        var criado = await LerAsync(await http.PostAsJsonAsync($"{Base}/shares", Share("COLHEITADEIRA", "40,5", inicio), Json), HttpStatusCode.Created);
        criado.GetProperty("categoriaDeMaquinaNome").GetString().Should().Be("Colheitadeira");
        criado.GetProperty("vigencia").GetProperty("informadoPor").GetString().Should().NotBeNullOrWhiteSpace();

        ShareDe(await LerAsync(await http.GetAsync($"{Base}?em={Iso(inicio.AddDays(-1))}"), HttpStatusCode.OK), "COLHEITADEIRA")
            .Should().Be(31m, "a vigência nova ainda não começou");
        var depois = await LerAsync(await http.GetAsync($"{Base}?em={Iso(inicio)}"), HttpStatusCode.OK);
        ShareDe(depois, "COLHEITADEIRA").Should().Be(40.5m);
        Pendencias(depois).Should().NotContain(p => p.Contains("Colheitadeira") && p.Contains("protótipo"),
            "a colheitadeira já tem share decidido por alguém");

        int id;
        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            id = (await db.SharesAlvo.SingleAsync(s => s.VigenteDesde == inicio && s.CategoriaDeMaquinaId == 3)).Id;
        }

        var trilha = await TrilhaAsync(nameof(ShareAlvoDaCategoria), id);
        trilha.Should().NotBeEmpty();
        trilha.Should().AllSatisfy(l => l.AlteradoPorId.Should().Be(100));
        trilha.Should().Contain(l => l.Campo == "Percentual" && l.ValorNovo == "40.5");

        var revogado = await LerAsync(
            await http.PostAsJsonAsync($"{Base}/shares/COLHEITADEIRA/{Iso(inicio)}/revogacao", new { motivo = "a diretoria voltou atrás" }, Json),
            HttpStatusCode.OK);
        revogado.GetProperty("vigencia").GetProperty("motivoDaRevogacao").GetString().Should().Be("a diretoria voltou atrás");
        ShareDe(await LerAsync(await http.GetAsync($"{Base}?em={Iso(inicio)}"), HttpStatusCode.OK), "COLHEITADEIRA")
            .Should().Be(31m, "revogada a vigência nova, volta a valer a anterior");
    }

    [Fact]
    public async Task Sazonalidade_e_pesos_novos_valem_da_data_em_diante_e_entram_na_trilha()
    {
        var http = await AdministradorAsync();
        var inicio = Hoje.AddDays(20);

        var criado = await LerAsync(await http.PostAsJsonAsync(Base, Planejamento(inicio), Json), HttpStatusCode.Created);
        criado.GetProperty("pesos").GetProperty("potencial").GetDecimal().Should().Be(30m);
        criado.GetProperty("pesos").GetProperty("realizacao").GetDecimal().Should().Be(0m, "peso zero tira o componente da conta");

        var depois = await LerAsync(await http.GetAsync($"{Base}?em={Iso(inicio)}"), HttpStatusCode.OK);
        depois.GetProperty("dados").GetProperty("planejamento").GetProperty("pesos").GetProperty("potencial").GetDecimal().Should().Be(30m);
        Pendencias(depois).Should().NotContain(p => p.Contains("ainda são os do protótipo"));

        int id;
        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            id = (await db.ParametrosDoPlanejamento.SingleAsync(p => p.VigenteDesde == inicio)).Id;
        }

        var trilha = await TrilhaAsync(nameof(ParametroDoPlanejamento), id);
        trilha.Should().Contain(l => l.Campo == "SazonalidadeOutubro" && l.ValorNovo == "10.51" && l.AlteradoPorId == 100);
        trilha.Should().Contain(l => l.Campo == "PesoDoPotencial" && l.ValorNovo == "30");

        var historico = await LerAsync(await http.GetAsync($"{Base}/historico"), HttpStatusCode.OK);
        historico.GetProperty("dados").GetProperty("planejamentos").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Sazonalidade_que_nao_soma_cem_e_recusada_no_campo_com_a_soma()
    {
        var http = await AdministradorAsync();
        var meses = SazonalidadeDoPrototipo.ToArray();
        meses[0] = "16.52";

        var recusa = await LerAsync(await http.PostAsJsonAsync(Base, Planejamento(Hoje.AddDays(40), meses), Json), HttpStatusCode.UnprocessableEntity);

        var erro = recusa.GetProperty("erros").EnumerateArray().Single();
        erro.GetProperty("campo").GetString().Should().Be("sazonalidade");
        erro.GetProperty("mensagem").GetString().Should().Contain("somam 110%");
    }

    [Fact]
    public async Task Mes_invalido_sai_no_mes_certo()
    {
        var http = await AdministradorAsync();
        var meses = SazonalidadeDoPrototipo.ToArray();
        meses[3] = "abc";

        var recusa = await LerAsync(await http.PostAsJsonAsync(Base, Planejamento(Hoje.AddDays(41), meses), Json), HttpStatusCode.UnprocessableEntity);

        recusa.GetProperty("erros").EnumerateArray().Select(e => e.GetProperty("campo").GetString())
            .Should().Contain("sazonalidade[3]", "o índice 3 é abril, e é a caixa de abril que o formulário marca");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("100,5")]
    public async Task Share_fora_de_zero_a_cem_e_recusado(string percentual)
    {
        var http = await AdministradorAsync();

        var recusa = await LerAsync(await http.PostAsJsonAsync($"{Base}/shares", Share("TRATOR", percentual, Hoje.AddDays(50)), Json), HttpStatusCode.UnprocessableEntity);

        recusa.GetProperty("erros").EnumerateArray().Should().Contain(e => e.GetProperty("campo").GetString() == "percentual");
    }

    [Fact]
    public async Task Categoria_que_nao_existe_e_recusada()
    {
        var http = await AdministradorAsync();

        var recusa = await LerAsync(await http.PostAsJsonAsync($"{Base}/shares", Share("TRATORR", "30", Hoje.AddDays(51)), Json), HttpStatusCode.UnprocessableEntity);

        recusa.GetProperty("erros").EnumerateArray().Should().Contain(e => e.GetProperty("campo").GetString() == "categoriaDeMaquinaCodigo");
    }

    private async Task<List<AlteracaoDeCampo>> TrilhaAsync(string entidade, int id)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await db.AlteracoesDeCampo
            .Where(a => a.Entidade == entidade && a.RegistroId == id)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }
}
