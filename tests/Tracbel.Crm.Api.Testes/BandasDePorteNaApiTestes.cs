using System.Net;
using System.Net.Http.Json;
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
/// AS BANDAS DE PORTE PELA API (issue 166). O critério foi decidido pelo Ricardo em 27/09/2026 — os tercis da
/// demanda anual dos municípios da ADR, um terço em cada porte —, e o que está sob prova é o caminho inteiro:
/// <list type="number">
///   <item>a sugestão sai da MESMA demanda que a tela mostra, e não grava nada;</item>
///   <item>a vigência guarda as duas bandas e o mínimo de linhas do SICOR, que o formulário não mandava;</item>
///   <item>registradas as bandas, o porte do recorte ganha nome — o do município típico, e não o da soma.</item>
/// </list>
///
/// <para><b>Classe própria, banco próprio</b>: a vigência dos parâmetros gerais registrada hoje muda a leitura
/// de hoje, e nas outras classes ela mexeria com o que os testes de lá conferem.</para>
/// </summary>
[Trait("Categoria", "ParametrosDoPotencial")]
public sealed class BandasDePorteNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Base = "/api/v1/admin/parametros-do-potencial";
    private const int Cafe = 40139;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Seis municípios fictícios da ADR, com café de 100 a 600 ha: seis demandas diferentes.</summary>
    private static readonly (int Codigo, string Nome, decimal Hectares)[] MunicipiosDaAdr =
    [
        (3599001, "Município de Teste A", 100m), (3599002, "Município de Teste B", 200m),
        (3599003, "Município de Teste C", 300m), (3599004, "Município de Teste D", 400m),
        (3599005, "Município de Teste E", 500m), (3599006, "Município de Teste F", 600m)
    ];

    private static DateOnly Hoje => ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow);

    private static string Iso(DateOnly data) => data.ToString("yyyy-MM-dd");

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta, HttpStatusCode esperado)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(esperado, corpo);
        return JsonDocument.Parse(corpo).RootElement.Clone();
    }

    /// <summary>O conjunto inteiro dos gerais, igual à semente de 23/09, com as bandas e o mínimo pedidos.</summary>
    private static object Gerais(DateOnly inicio, string? porteMedio = null, string? porteGrande = null, string? minimo = null) => new
    {
        vigenteDesde = Iso(inicio),
        mesesDaJanela = "12",
        pesoDosContratosNoCredito = "0,70",
        limiteDeRetracao = "1,00",
        limiteDeAquecimento = "1,20",
        limiteDeSuperaquecimento = "1,40",
        limiteDaPercepcao = "5",
        pesoDoIndicadorDePreco = "0,4",
        pesoDoIndicadorDeCredito = "0,5",
        pesoDoIndicadorComercial = "1",
        fatorMinimo = "0,4",
        fatorMaximo = "1,5",
        minimoDeLinhasNoCredito = minimo,
        porteMedioAPartirDe = porteMedio,
        porteGrandeAPartirDe = porteGrande,
        justificativa = "bandas de porte (exemplo de teste)"
    };

    /// <summary>
    /// A ADR, o café da PAM em cada município e uma regra do café COM anos de renovação a partir de hoje — a
    /// semente não tem anos, e sem eles a demanda anual não sai.
    /// </summary>
    private async Task<HttpClient> AdministradorComAdrAsync()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var http = api.ClienteDeRibeirao();

        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            if (await db.MunicipiosDaAreaDeAtuacao.AnyAsync()) return http;

            var agora = DateTime.UtcNow;
            var municipios = MunicipiosDaAdr.Select(m => Municipio.Criar(m.Nome, "SP", m.Codigo)).ToList();
            db.Municipios.AddRange(municipios);
            await db.SaveChangesAsync();

            foreach (var (municipio, (_, _, hectares)) in municipios.Zip(MunicipiosDaAdr))
            {
                db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                    municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 2, 100, agora));
                db.ProducoesAgricolasNosMunicipios.Add(ProducaoAgricolaNoMunicipio.Registrar(
                    municipio.Id, 2024, Cafe, "Café (em grão) Total", new(hectares, hectares, hectares * 2, hectares * 10), 100, agora));
            }

            await db.SaveChangesAsync();
        }

        await LerAsync(await http.PostAsJsonAsync($"{Base}/culturas", new
        {
            produtoCodigoIbge = Cafe.ToString(),
            hectaresPorMaquina = "10",
            anosDeRenovacao = "5",
            modeloDeReferencia = "3036N",
            situacao = "AConfirmar",
            vigenteDesde = Iso(Hoje),
            justificativa = "regra com ciclo, para a demanda anual sair",
            culturaCodigo = "CAFE",
            categoriaDeMaquinaCodigo = "TRATOR"
        }, Json), HttpStatusCode.Created);

        return http;
    }

    /// <summary>A demanda anual de cada município da ADR, como a tela a recebe.</summary>
    private static List<decimal> DemandasDaAdr(JsonElement indicadores) =>
    [
        .. indicadores.GetProperty("municipios").EnumerateArray()
            .Where(m => m.GetProperty("pertenceAAdr").GetBoolean())
            .Select(m => m.GetProperty("potencialEstrutural"))
            .Where(p => p.ValueKind == JsonValueKind.Object
                        && p.GetProperty("demandaAnualDeMaquinas").ValueKind == JsonValueKind.Number)
            .Select(p => p.GetProperty("demandaAnualDeMaquinas").GetDecimal())
    ];

    private static IEnumerable<string> Pendencias(JsonElement vigentes) =>
        vigentes.GetProperty("dados").GetProperty("pendencias").EnumerateArray().Select(p => p.GetString()!);

    [Fact]
    public async Task Os_tercis_saem_da_demanda_da_tela_e_registrados_dao_nome_ao_porte_do_recorte()
    {
        var http = await AdministradorComAdrAsync();

        var indicadores = (await LerAsync(await http.GetAsync("/api/v1/territorio/indicadores"), HttpStatusCode.OK))
            .GetProperty("dados").GetProperty("indicadores");
        var demandas = DemandasDaAdr(indicadores);
        demandas.Should().HaveCount(MunicipiosDaAdr.Length, "cada município da ADR tem café e a regra tem ciclo");

        var historicoAntes = (await LerAsync(await http.GetAsync($"{Base}/historico"), HttpStatusCode.OK))
            .GetProperty("dados").GetProperty("gerais").GetArrayLength();

        // 1. A SUGESTÃO É A CONTA DO DOMÍNIO SOBRE A MESMA DEMANDA DA TELA.
        var sugestao = (await LerAsync(await http.GetAsync($"{Base}/geral/porte-pelos-tercis"), HttpStatusCode.OK)).GetProperty("dados");
        var esperado = ParametroDoPotencial.BandasPelosTercis(demandas)!.Value;

        var medio = sugestao.GetProperty("porteMedioAPartirDe").GetDecimal();
        var grande = sugestao.GetProperty("porteGrandeAPartirDe").GetDecimal();
        medio.Should().Be(esperado.MedioAPartirDe);
        grande.Should().Be(esperado.GrandeAPartirDe);
        sugestao.GetProperty("municipiosDaAdr").GetInt32().Should().Be(MunicipiosDaAdr.Length);
        sugestao.GetProperty("municipiosNaConta").GetInt32().Should().Be(MunicipiosDaAdr.Length);
        sugestao.GetProperty("motivo").ValueKind.Should().Be(JsonValueKind.Null);
        sugestao.GetProperty("justificativa").GetString().Should()
            .Contain("Tercis").And.Contain("municípios da ADR").And.Contain("27/09/2026");
        sugestao.GetProperty("justificativa").GetString()!.Length.Should().BeLessThanOrEqualTo(400, "cabe na justificativa da vigência");

        (await LerAsync(await http.GetAsync($"{Base}/historico"), HttpStatusCode.OK))
            .GetProperty("dados").GetProperty("gerais").GetArrayLength().Should().Be(historicoAntes, "a sugestão não grava nada");

        // 2. ANTES DE REGISTRAR, o porte já tem nome, pelos tercis da ADR calculados na apuração (28/09/2026) — as
        // mesmas bandas da sugestão, e ditas como tercis, e não como registradas. Não é pendência.
        var bandasAntes = indicadores.GetProperty("momento").GetProperty("bandasDoPorte");
        bandasAntes.GetProperty("origem").GetString().Should().Be("TercisDaAdr");
        bandasAntes.GetProperty("medioAPartirDe").GetDecimal().Should().Be(medio);
        bandasAntes.GetProperty("grandeAPartirDe").GetDecimal().Should().Be(grande);
        bandasAntes.GetProperty("municipiosNaBase").GetInt32().Should().Be(MunicipiosDaAdr.Length);
        indicadores.GetProperty("momento").GetProperty("porte").GetString().Should().NotBeNull();
        Pendencias(await LerAsync(await http.GetAsync(Base), HttpStatusCode.OK))
            .Should().NotContain(p => p.Contains("bandas de porte"));

        // 3. REGISTRADAS A PARTIR DE HOJE, o recorte ganha o nome do município típico.
        await LerAsync(await http.PostAsJsonAsync($"{Base}/geral", Gerais(Hoje,
            medio.ToString(System.Globalization.CultureInfo.InvariantCulture),
            grande.ToString(System.Globalization.CultureInfo.InvariantCulture)), Json), HttpStatusCode.Created);

        var depois = (await LerAsync(await http.GetAsync("/api/v1/territorio/indicadores"), HttpStatusCode.OK))
            .GetProperty("dados").GetProperty("indicadores");
        var nomeEsperado = ParametroDoPotencial.Informar(
                new ParametroDoPotencial.Valores(12, 0.7m, 1m, 1.2m, 1.4m, null, 5m, 0.4m, 0.5m, 1m, 0.4m, 1.5m,
                    PorteMedioAPartirDe: medio, PorteGrandeAPartirDe: grande),
                Hoje, "referência do teste", 100, DateTime.UtcNow)
            .PorteDosMunicipios(demandas);

        nomeEsperado.Should().NotBeNull();
        depois.GetProperty("momento").GetProperty("porte").GetString().Should().Be(nomeEsperado,
            "o recorte é comparado pela demanda média por município, a mesma população de onde os cortes saíram");
        depois.GetProperty("momento").GetProperty("porte").GetString().Should().NotBe("Mercado grande",
            "a soma dos seis municípios passaria do corte de grande, e o nome não diria nada");
        depois.GetProperty("momento").GetProperty("bandasDoPorte").GetProperty("origem").GetString().Should().Be("Registradas");
    }

    [Fact]
    public async Task Com_filtro_de_regiao_os_tercis_continuam_os_da_ADR_inteira()
    {
        // O CORTE É DA ADR, e não do recorte: com a região Norte escolhida (onde estão os seis), os tercis são os mesmos;
        // com a Noroeste (nenhum), não há recorte a nomear.
        var http = await AdministradorComAdrAsync();

        var norte = (await LerAsync(await http.GetAsync("/api/v1/territorio/indicadores?regiao=Norte"), HttpStatusCode.OK))
            .GetProperty("dados").GetProperty("indicadores");
        var todos = (await LerAsync(await http.GetAsync("/api/v1/territorio/indicadores"), HttpStatusCode.OK))
            .GetProperty("dados").GetProperty("indicadores");

        norte.GetProperty("momento").GetProperty("bandasDoPorte").GetProperty("medioAPartirDe").GetDecimal()
            .Should().Be(todos.GetProperty("momento").GetProperty("bandasDoPorte").GetProperty("medioAPartirDe").GetDecimal());
    }

    [Fact]
    public async Task A_vigencia_guarda_as_bandas_e_o_minimo_de_linhas_que_o_formulario_nao_mandava()
    {
        var http = await AdministradorComAdrAsync();
        var inicio = Hoje.AddDays(120);

        var geral = await LerAsync(
            await http.PostAsJsonAsync($"{Base}/geral", Gerais(inicio, porteMedio: "12,5", porteGrande: "40", minimo: "20"), Json),
            HttpStatusCode.Created);

        geral.GetProperty("porteMedioAPartirDe").GetDecimal().Should().Be(12.5m, "vírgula decimal, como os outros campos");
        geral.GetProperty("porteGrandeAPartirDe").GetDecimal().Should().Be(40m);
        geral.GetProperty("minimoDeLinhasNoCredito").GetInt32().Should().Be(20,
            "o formulário não mandava o mínimo, e toda vigência nova registrada pela tela o apagava");

        var naData = await LerAsync(await http.GetAsync($"{Base}?em={Iso(inicio)}"), HttpStatusCode.OK);
        naData.GetProperty("dados").GetProperty("geral").GetProperty("porteGrandeAPartirDe").GetDecimal().Should().Be(40m);

        // AS BANDAS ENTRAM NA TRILHA, com o autor — o aceite da issue 166 pede formulário, trilha e pendências.
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        var id = (await db.ParametrosDoPotencial.SingleAsync(p => p.VigenteDesde == inicio)).Id;
        var trilha = await db.AlteracoesDeCampo
            .Where(a => a.Entidade == nameof(ParametroDoPotencial) && a.RegistroId == id)
            .ToListAsync();

        trilha.Select(a => a.Campo).Should().Contain(["PorteMedioAPartirDe", "PorteGrandeAPartirDe", "MinimoDeLinhasNoCredito"]);
        trilha.Should().AllSatisfy(a => a.AlteradoPorId.Should().Be(100, "a trilha diz quem decidiu"));
    }

    [Fact]
    public async Task As_bandas_pela_metade_invertidas_ou_com_mais_de_uma_casa_sao_recusadas_no_campo()
    {
        var http = await AdministradorComAdrAsync();

        async Task<string?[]> CamposRecusados(object corpo) =>
            [
                .. (await LerAsync(await http.PostAsJsonAsync($"{Base}/geral", corpo, Json), HttpStatusCode.UnprocessableEntity))
                    .GetProperty("erros").EnumerateArray().Select(e => e.GetProperty("campo").GetString())
            ];

        (await CamposRecusados(Gerais(Hoje.AddDays(121), porteMedio: "12")))
            .Should().Equal(["porteGrandeAPartirDe"], "a recusa cai no campo que ficou vazio");
        (await CamposRecusados(Gerais(Hoje.AddDays(122), porteGrande: "40")))
            .Should().Equal(["porteMedioAPartirDe"]);
        (await CamposRecusados(Gerais(Hoje.AddDays(123), porteMedio: "40", porteGrande: "12")))
            .Should().Equal(["porteGrandeAPartirDe"]);
        (await CamposRecusados(Gerais(Hoje.AddDays(124), porteMedio: "12,55", porteGrande: "40")))
            .Should().Equal(["porteMedioAPartirDe"], "a coluna guarda uma casa: o banco arredondaria em silêncio");
        (await CamposRecusados(Gerais(Hoje.AddDays(125), minimo: "2,5")))
            .Should().Equal(["minimoDeLinhasNoCredito"]);
    }

    [Fact]
    public async Task Quem_nao_administra_os_parametros_nao_pede_a_sugestao()
    {
        await AdministradorComAdrAsync();

        var resposta = await api.ClienteDeBarretos().GetAsync($"{Base}/geral/porte-pelos-tercis");
        var corpo = await resposta.Content.ReadAsStringAsync();

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, corpo);
        corpo.Should().Contain("Falta a permissão");
    }
}
