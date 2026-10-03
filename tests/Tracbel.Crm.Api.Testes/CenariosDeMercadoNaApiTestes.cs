using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O CENÁRIO DE TESTE DOS CENÁRIOS DE MERCADO (issue 263), o mesmo da Demanda: três municípios da ADR com café — 150, 300 e
/// 450 ha —, uma regra do trator de 10 ha por máquina e 5 anos (demanda 3, 6 e 9) e o share-alvo da semente (31%). C1 e C2
/// são da filial de Ribeirão (1), C3 da de Barretos (2). O comprador de C3 recebeu um trator no primeiro dia deste ano
/// fiscal, um no do anterior e um no de dois anos atrás, e uma máquina sem categoria (que não conta).
/// </summary>
internal static class CenarioDosCenarios
{
    public const string Rota = "/api/v1/mercado/cenarios";
    public const int C1 = 3597001, C2 = 3597002, C3 = 3597003;
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const int Cafe = 40139;

    private static readonly (int Codigo, string Nome, decimal Hectares, int Filial)[] Municipios =
    [
        (C1, "Município de Teste C1", 150m, 1), (C2, "Município de Teste C2", 300m, 1), (C3, "Município de Teste C3", 450m, 2)
    ];

    public static int AnoFiscalCorrente => AnoFiscal.Do(AnoFiscal.MesCorrenteEmSaoPaulo(DateTime.UtcNow));

    private static DateOnly InicioDoAnoFiscal => AnoFiscal.Inteiro(AnoFiscalCorrente).Inicial;

    /// <summary>Semeia uma vez por banco (por classe de teste), com o administrador registrando a regra do potencial.</summary>
    public static async Task SemearAsync(ApiEmMemoria api)
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var http = api.ClienteDeRibeirao();

        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            if (await db.MunicipiosDaAreaDeAtuacao.AnyAsync()) return;

            var agora = DateTime.UtcNow;
            var municipios = Municipios.Select(m => Municipio.Criar(m.Nome, "SP", m.Codigo)).ToList();
            var foraDaAdr = Municipio.Criar("Município fora da ADR", "SP", 3597999);
            db.Municipios.AddRange([.. municipios, foraDaAdr]);
            await db.SaveChangesAsync();

            foreach (var (municipio, (_, _, hectares, filial)) in municipios.Zip(Municipios))
            {
                db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                    municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, filial, "Area de Atuação.xlsx", 2, 100, agora));
                db.ProducoesAgricolasNosMunicipios.Add(ProducaoAgricolaNoMunicipio.Registrar(
                    municipio.Id, 2024, Cafe, "Café (em grão) Total", new(hectares, hectares, hectares * 2, hectares * 10), 100, agora));
            }

            db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                foraDaAdr.Id, false, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 9, 100, agora));
            await db.SaveChangesAsync();
            await SemearEntregasAsync(db, municipios[2], agora);
        }

        var regra = await http.PostAsJsonAsync("/api/v1/admin/parametros-do-potencial/culturas", new
        {
            produtoCodigoIbge = Cafe.ToString(),
            hectaresPorMaquina = "10",
            anosDeRenovacao = "5",
            modeloDeReferencia = "3036N",
            situacao = "AConfirmar",
            vigenteDesde = ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            justificativa = "regra com ciclo, para a demanda anual sair",
            culturaCodigo = "CAFE",
            categoriaDeMaquinaCodigo = "TRATOR"
        }, Json);
        regra.StatusCode.Should().Be(HttpStatusCode.Created, await regra.Content.ReadAsStringAsync());
    }

    private static async Task SemearEntregasAsync(CrmDbContext db, Municipio c3, DateTime agora)
    {
        var art = Dominio.Integracao.Sistema.Criar("ART_CENARIOS", "ART de teste dos cenários", "View somente leitura");
        db.Sistemas.Add(art);
        var tratorMedio = LinhaDeProduto.Criar("TRATOR_MEDIO", "Trator médio", null, PorteDeMaquina.Medio);
        db.LinhasDeProduto.Add(tratorMedio);
        var comprador = Cliente.Criar(1, "Comprador de C3", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.Add(comprador);
        await db.SaveChangesAsync();

        db.Enderecos.Add(Endereco.Criar(1, comprador.Id, TipoDeEndereco.Fiscal, "Rua do teste",
            MunicipioDoEndereco.Selecionado(c3.Id), "SP", 100, ehPrincipal: true));

        Equipamento Maquina(string chassi, int? linha) =>
            Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(chassi), OrigemDoEquipamento.Art, 100, null, linha);
        var maquinas = new[]
        {
            Maquina("1CENARIO000000001", tratorMedio.Id), Maquina("1CENARIO000000002", tratorMedio.Id),
            Maquina("1CENARIO000000003", tratorMedio.Id), Maquina("1CENARIO000000004", null)
        };
        db.Equipamentos.AddRange(maquinas);
        await db.SaveChangesAsync();

        void Entregar(int posicao, DateOnly entregueEm) =>
            db.VendasDeMaquina.Add(VendaDeMaquina.Registrar(
                art.Id, $"ART-CENARIO-{posicao}", maquinas[posicao].Id, comprador.Id,
                new DadosDaVendaNaOrigem(
                    1, null, entregueEm.AddDays(-20), entregueEm.AddDays(-10), entregueEm, agora,
                    null, null, null, "Varejo", false, false, 1, "TRATOR MEDIO", "PRODUTO DE TESTE",
                    "Agro Teste", "Unidade de teste", null, $"hash-cenario-{posicao}", null),
                agora, 100));

        Entregar(0, InicioDoAnoFiscal);
        Entregar(1, InicioDoAnoFiscal.AddYears(-1));
        Entregar(2, InicioDoAnoFiscal.AddYears(-2));
        Entregar(3, InicioDoAnoFiscal);
        await db.SaveChangesAsync();
    }

    public static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").Clone();
    }

    /// <summary>O corpo da gravação — sem o envelope `dados`, que é o da leitura com procedência.</summary>
    public static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.Clone();
    }

    public static JsonElement LinhaDo(JsonElement dados, int codigo) =>
        dados.GetProperty("municipios").EnumerateArray().Single(m => m.GetProperty("codigoIbge").GetInt32() == codigo);

    public static Task<HttpResponseMessage> EscolherAsync(HttpClient http, int codigo, object corpo) =>
        http.PutAsJsonAsync($"{Rota}/{codigo}", corpo, Json);
}

/// <summary>
/// OS CENÁRIOS DE MERCADO PELA API (issue 263) — a leitura e a gravação pelo administrador, que planeja todos os municípios.
///
/// <para><b>Classe própria, banco próprio</b>: a regra com ciclo e as metas gravadas mudariam o que as outras classes conferem.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class CenariosDeMercadoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private async Task<HttpClient> ComCenariosAsync()
    {
        await CenarioDosCenarios.SemearAsync(api);
        return api.ClienteDeRibeirao();
    }

    [Fact]
    public async Task O_potencial_a_meta_estrutural_e_os_cenarios_sao_os_da_demanda_com_menos_e_mais_cinco_por_cento()
    {
        var http = await ComCenariosAsync();
        var dados = await CenarioDosCenarios.DadosAsync(await http.GetAsync(CenarioDosCenarios.Rota));
        var demanda = await CenarioDosCenarios.DadosAsync(await http.GetAsync("/api/v1/mercado/demanda"));

        dados.GetProperty("categoria").GetString().Should().Be("TRATOR", "o padrão é o do protótipo");
        dados.GetProperty("anoFiscal").GetInt32().Should().Be(CenarioDosCenarios.AnoFiscalCorrente);
        dados.GetProperty("situacaoDoAno").GetString().Should().Be("Corrente");
        dados.GetProperty("shareAlvo").GetDecimal().Should().Be(31m);
        dados.GetProperty("anosFiscais").EnumerateArray().Select(a => a.GetInt32()).First()
            .Should().Be(CenarioDosCenarios.AnoFiscalCorrente + 1, "o próximo ano fiscal já pode ser planejado");

        var c3 = CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C3);
        c3.GetProperty("potencial").GetDecimal().Should().Be(9m, "450 ha ÷ 10 ha ÷ 5 anos");
        c3.GetProperty("metaEstrutural").GetDecimal().Should().Be(2.79m, "9 × 31%");

        // O MODERADO É O "A ENTREGAR" DA DEMANDA — o ajustado quando o fator tem dado, o estrutural quando não tem.
        var naDemanda = demanda.GetProperty("municipios").EnumerateArray().Single(m => m.GetProperty("codigoIbge").GetInt32() == CenarioDosCenarios.C3);
        var esperado = naDemanda.GetProperty("aEntregarAjustada").ValueKind == JsonValueKind.Number
            ? naDemanda.GetProperty("aEntregarAjustada").GetDecimal()
            : naDemanda.GetProperty("aEntregar").GetDecimal();
        var moderado = c3.GetProperty("moderado").GetDecimal();
        moderado.Should().Be(decimal.Round(esperado, 2));
        c3.GetProperty("conservador").GetDecimal().Should().Be(decimal.Round(moderado * 0.95m, 2));
        c3.GetProperty("otimista").GetDecimal().Should().Be(decimal.Round(moderado * 1.05m, 2));
        c3.GetProperty("aEntregar").GetDecimal().Should().Be(moderado, "sem escolha gravada, vale o moderado");
        c3.GetProperty("gravavel").GetBoolean().Should().BeTrue("o administrador planeja todos os municípios");
        dados.GetProperty("podeGravar").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task O_realizado_e_a_entrega_do_art_no_ano_fiscal_e_a_media_e_dos_quatro_anos_fechados()
    {
        var http = await ComCenariosAsync();
        var dados = await CenarioDosCenarios.DadosAsync(await http.GetAsync(CenarioDosCenarios.Rota));

        var c3 = CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C3);
        c3.GetProperty("realizadoNoAno").GetInt32().Should().Be(1, "a máquina sem categoria não conta — não se sabe se é trator");
        c3.GetProperty("realizadoNoAnoAnterior").GetInt32().Should().Be(1);
        c3.GetProperty("mediaDosAnosAnteriores").GetDecimal().Should().Be(0.5m, "(0 + 0 + 1 + 1) ÷ 4");
        c3.GetProperty("clientes").GetInt32().Should().Be(1);
        c3.GetProperty("realizadoNoIntervalo").GetInt32().Should().Be(1, "o intervalo padrão é o ano fiscal até hoje");
        c3.GetProperty("shareEstrutural").GetDecimal().Should().Be(11.1m, "1 ÷ 9");

        var totais = dados.GetProperty("totais");
        totais.GetProperty("realizadoNoAno").GetInt32().Should().Be(1);
        totais.GetProperty("realizacaoSobrePotencial").GetDecimal().Should().Be(5.6m, "1 ÷ 18");
        totais.GetProperty("metaEstrutural").GetDecimal().Should().Be(5.58m, "18 × 31%");
        dados.GetProperty("anosDaMedia").EnumerateArray().Select(a => a.GetInt32()).Should().Equal(
            Enumerable.Range(CenarioDosCenarios.AnoFiscalCorrente - 4, 4));
    }

    [Fact]
    public async Task A_recomendacao_compara_o_moderado_com_o_ano_fiscal_anterior_inteiro()
    {
        var http = await ComCenariosAsync();
        var dados = await CenarioDosCenarios.DadosAsync(await http.GetAsync(CenarioDosCenarios.Rota));

        // C3: o moderado (perto de 2,8) passa de 1,5 × o realizado anterior (1) — a rampa sugere 1,3.
        var c3 = CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C3);
        c3.GetProperty("recomendacao").GetString().Should().Be("Gradual");
        c3.GetProperty("metaGradual").GetDecimal().Should().Be(1.3m);
        c3.GetProperty("acimaDoRealizado").GetDecimal()
            .Should().Be(decimal.Round((c3.GetProperty("moderado").GetDecimal() / 1m - 1) * 100m, 0));

        // C1: sem histórico, mas a meta (perto de 0,9) é pequena — atingível.
        CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C1).GetProperty("recomendacao").GetString().Should().Be("Atingivel");
    }

    [Fact]
    public async Task A_escolha_grava_o_numero_do_cenario_hoje_com_autor_e_trilha_e_muda_a_mesma_linha()
    {
        var http = await ComCenariosAsync();
        var antes = CenarioDosCenarios.LinhaDo(await CenarioDosCenarios.DadosAsync(await http.GetAsync(CenarioDosCenarios.Rota)), CenarioDosCenarios.C2);

        var otimista = await CenarioDosCenarios.EscolherAsync(http, CenarioDosCenarios.C2, new { cenario = "otimista" });
        var escolha = await CenarioDosCenarios.CorpoAsync(otimista);
        escolha.GetProperty("cenario").GetString().Should().Be("Otimista");
        escolha.GetProperty("metaCombinada").GetDecimal().Should().Be(antes.GetProperty("otimista").GetDecimal(), "o número é calculado no servidor");

        // O MANUAL: 5 máquinas, mais perto do otimista que do moderado.
        var manual = await CenarioDosCenarios.EscolherAsync(http, CenarioDosCenarios.C2, new { cenario = "Manual", valorManual = "5" });
        (await CenarioDosCenarios.CorpoAsync(manual)).GetProperty("metaCombinada").GetDecimal().Should().Be(5m);

        var dados = await CenarioDosCenarios.DadosAsync(await http.GetAsync(CenarioDosCenarios.Rota));
        var c2 = CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C2);
        c2.GetProperty("escolha").GetProperty("cenario").GetString().Should().Be("Manual");
        c2.GetProperty("escolha").GetProperty("gravadaPor").GetString().Should().NotBeNullOrEmpty();
        c2.GetProperty("aEntregar").GetDecimal().Should().Be(5m);
        c2.GetProperty("enquadramento").GetString().Should().Be("Otimista");
        dados.GetProperty("totais").GetProperty("municipiosComMeta").GetInt32().Should().Be(1, "mudar de ideia altera a mesma meta");

        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        (await db.MetasDosCenarios.CountAsync()).Should().Be(1);
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(MetaDoCenarioNoMunicipio)).ToListAsync();
        trilha.Should().Contain(a => a.Campo == nameof(MetaDoCenarioNoMunicipio.MetaNaEscolha) && a.Operacao == OperacaoAuditada.Alteracao && a.AlteradoPorId == 100,
            "a trilha guarda de quanto para quanto a meta mudou, e quem mudou");
    }

    [Theory]
    [InlineData("{\"cenario\":\"Manual\"}", "valorManual")]
    [InlineData("{\"cenario\":\"Moderado\",\"valorManual\":\"3\"}", "valorManual")]
    [InlineData("{\"cenario\":\"Pessimista\"}", "cenario")]
    [InlineData("{\"cenario\":\"2\"}", "cenario")]
    [InlineData("{\"cenario\":\"Manual\",\"valorManual\":\"-1\"}", "valorManual")]
    [InlineData("{\"cenario\":\"Moderado\",\"categoria\":\"COLHEITADEIRA\"}", "categoria")]
    public async Task Escolha_que_nao_vale_e_recusada_no_campo(string corpo, string campo)
    {
        var http = await ComCenariosAsync();

        var resposta = await http.PutAsync($"{CenarioDosCenarios.Rota}/{CenarioDosCenarios.C1}",
            new StringContent(corpo, System.Text.Encoding.UTF8, "application/json"));

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain($"\"{campo}\"");
    }

    [Fact]
    public async Task O_ano_fiscal_que_ja_fechou_e_so_leitura()
    {
        var http = await ComCenariosAsync();
        var anterior = CenarioDosCenarios.AnoFiscalCorrente - 1;

        var dados = await CenarioDosCenarios.DadosAsync(await http.GetAsync($"{CenarioDosCenarios.Rota}?anoFiscal={anterior}"));
        dados.GetProperty("situacaoDoAno").GetString().Should().Be("Fechado");
        dados.GetProperty("podeGravar").GetBoolean().Should().BeFalse();
        dados.GetProperty("porQueNaoGrava").GetString().Should().Contain("fechou");
        CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C3).GetProperty("realizadoNoAno").GetInt32().Should().Be(1);

        var resposta = await CenarioDosCenarios.EscolherAsync(http, CenarioDosCenarios.C1, new { cenario = "Moderado", anoFiscal = anterior.ToString() });
        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("\"anoFiscal\"");
    }

    [Fact]
    public async Task Municipio_fora_da_adr_e_recusado_e_o_que_nao_existe_nao_e_encontrado()
    {
        var http = await ComCenariosAsync();

        (await CenarioDosCenarios.EscolherAsync(http, 3597999, new { cenario = "Manual", valorManual = "1" }))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await CenarioDosCenarios.EscolherAsync(http, 3590000, new { cenario = "Manual", valorManual = "1" }))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("categoria=PULVERIZADOR", "categoria")]
    [InlineData("anoFiscal=2019", "anoFiscal")]
    [InlineData("de=2019-01", "de")]
    [InlineData("de=junho", "de")]
    public async Task Parametro_que_nao_vale_e_recusado_no_campo(string consulta, string campo)
    {
        var http = await ComCenariosAsync();

        var resposta = await http.GetAsync($"{CenarioDosCenarios.Rota}?{consulta}");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain($"\"{campo}\"");
    }
}

/// <summary>O CEN VÊ E NÃO GRAVA (decisão do Ricardo de 02/10/2026): o perfil padrão não tem <c>Planejamento.Gravar</c>.</summary>
[Trait("Categoria", "Territorio")]
public sealed class CenariosDeMercadoSemPermissaoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    [Fact]
    public async Task O_cen_le_os_cenarios_mas_a_gravacao_e_recusada()
    {
        await CenarioDosCenarios.SemearAsync(api);
        var http = api.ClienteDeBarretos();

        var dados = await CenarioDosCenarios.DadosAsync(await http.GetAsync(CenarioDosCenarios.Rota));
        dados.GetProperty("podeGravar").GetBoolean().Should().BeFalse();
        dados.GetProperty("porQueNaoGrava").GetString().Should().Contain(Permissoes.PlanejamentoGravar);
        dados.GetProperty("municipios").EnumerateArray().Should().OnlyContain(m => !m.GetProperty("gravavel").GetBoolean());

        var resposta = await CenarioDosCenarios.EscolherAsync(http, CenarioDosCenarios.C3, new { cenario = "Moderado" });
        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

/// <summary>A GERÊNCIA PLANEJA AS FILIAIS DELA: a de Barretos grava em C3 (Barretos) e não em C1 (Ribeirão).</summary>
[Trait("Categoria", "Territorio")]
public sealed class CenariosDeMercadoDaGerenciaNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    [Fact]
    public async Task A_gerencia_grava_nos_municipios_das_filiais_ao_alcance_e_so_neles()
    {
        await CenarioDosCenarios.SemearAsync(api);
        await api.ConcederPerfilAsync(200, PerfisDeSistema.Gerencia);
        var http = api.ClienteDeBarretos();

        var dados = await CenarioDosCenarios.DadosAsync(await http.GetAsync(CenarioDosCenarios.Rota));
        dados.GetProperty("podeGravar").GetBoolean().Should().BeTrue();
        CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C3).GetProperty("gravavel").GetBoolean().Should().BeTrue();
        CenarioDosCenarios.LinhaDo(dados, CenarioDosCenarios.C1).GetProperty("gravavel").GetBoolean().Should().BeFalse();

        (await CenarioDosCenarios.EscolherAsync(http, CenarioDosCenarios.C3, new { cenario = "Conservador" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var deRibeirao = await CenarioDosCenarios.EscolherAsync(http, CenarioDosCenarios.C1, new { cenario = "Conservador" });
        deRibeirao.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await deRibeirao.Content.ReadAsStringAsync()).Should().Contain("fora do seu alcance");
    }
}
