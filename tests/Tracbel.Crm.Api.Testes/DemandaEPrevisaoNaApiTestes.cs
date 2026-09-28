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
/// A DEMANDA E PREVISÃO PELA API (issue 258). Três municípios da ADR com café — 150, 300 e 450 ha — e uma regra do trator
/// de 10 ha por máquina e 5 anos de renovação: parque de 15, 30 e 45 (90 no total) e demanda de 3, 6 e 9 (18). O share-alvo
/// do trator é a semente do protótipo (31%) e a sazonalidade também — a previsão mensal tem de somar o ano.
///
/// <para><b>Classe própria, banco próprio</b>: a regra com ciclo mudaria a demanda que as outras classes conferem.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class DemandaEPrevisaoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/mercado/demanda";
    private const int Cafe = 40139;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly (int Codigo, string Nome, decimal Hectares)[] MunicipiosDaAdr =
    [
        (3598001, "Município de Teste D1", 150m), (3598002, "Município de Teste D2", 300m), (3598003, "Município de Teste D3", 450m)
    ];

    private static DateOnly Hoje => ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").Clone();
    }

    private async Task<HttpClient> ComDemandaAsync()
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

        var regra = await http.PostAsJsonAsync("/api/v1/admin/parametros-do-potencial/culturas", new
        {
            produtoCodigoIbge = Cafe.ToString(),
            hectaresPorMaquina = "10",
            anosDeRenovacao = "5",
            modeloDeReferencia = "3036N",
            situacao = "AConfirmar",
            vigenteDesde = Hoje.ToString("yyyy-MM-dd"),
            justificativa = "regra com ciclo, para a demanda anual sair",
            culturaCodigo = "CAFE",
            categoriaDeMaquinaCodigo = "TRATOR"
        }, Json);
        regra.StatusCode.Should().Be(HttpStatusCode.Created, await regra.Content.ReadAsStringAsync());

        return http;
    }

    [Fact]
    public async Task O_parque_a_demanda_e_o_a_entregar_saem_das_parcelas_do_motor()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        dados.GetProperty("categoria").GetString().Should().Be("TRATOR", "o padrão é o do protótipo");
        dados.GetProperty("shareAlvo").GetDecimal().Should().Be(31m);
        dados.GetProperty("shareDoPrototipo").GetBoolean().Should().BeTrue();

        var totais = dados.GetProperty("totais");
        totais.GetProperty("parque").GetDecimal().Should().Be(90m, "150 + 300 + 450 ha ÷ 10 ha por máquina");
        totais.GetProperty("demandaEstrutural").GetDecimal().Should().Be(18m, "o parque ÷ 5 anos");
        totais.GetProperty("aEntregar").GetDecimal().Should().Be(5.58m, "18 × 31%");
        totais.GetProperty("municipiosComDemanda").GetInt32().Should().Be(3);
        totais.GetProperty("culturasComRegra").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task A_cultura_mostra_os_parametros_que_a_propria_conta_usou()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        var cafe = dados.GetProperty("porCultura").EnumerateArray().Single(c => c.GetProperty("culturaCodigo").GetString() == "CAFE");
        cafe.GetProperty("hectaresPorMaquina").GetDecimal().Should().Be(10m, "área ÷ parque devolve a regra");
        cafe.GetProperty("anosDeRenovacao").GetDecimal().Should().Be(5m, "parque ÷ demanda devolve o ciclo");
        cafe.GetProperty("parque").GetDecimal().Should().Be(90m);
        cafe.GetProperty("demandaEstrutural").GetDecimal().Should().Be(18m);
    }

    [Fact]
    public async Task A_previsao_mensal_vai_de_novembro_a_outubro_e_soma_o_ano()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        var meses = dados.GetProperty("previsaoMensal").EnumerateArray().ToList();
        meses.Select(m => m.GetProperty("mes").GetInt32()).Should().Equal(11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        meses.Sum(m => m.GetProperty("fracao").GetDecimal()).Should().BeApproximately(1m, 0.001m, "a sazonalidade soma o ano inteiro");
        meses.Sum(m => m.GetProperty("aEntregar").GetDecimal()).Should().BeApproximately(5.58m, 0.05m, "os meses somam o a entregar do ano");

        // A ENTREGA POR LOJA SOMA O MESMO ANO — pela ajustada quando existe, e a soma dos meses fecha com o total da loja.
        foreach (var loja in dados.GetProperty("porLoja").EnumerateArray())
            loja.GetProperty("porMes").EnumerateArray().Sum(v => v.GetDecimal())
                .Should().BeApproximately(loja.GetProperty("aEntregarNoAno").GetDecimal(), 0.05m);
    }

    [Fact]
    public async Task A_matriz_ordena_pela_demanda_e_traz_a_demanda_de_cada_cultura()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        var municipios = dados.GetProperty("municipios").EnumerateArray().ToList();
        municipios.Select(m => m.GetProperty("codigoIbge").GetInt32()).Should().Equal(3598003, 3598002, 3598001);

        var maior = municipios[0];
        maior.GetProperty("parque").GetDecimal().Should().Be(45m);
        maior.GetProperty("demandaEstrutural").GetDecimal().Should().Be(9m);
        maior.GetProperty("porCultura").EnumerateArray().Single().GetProperty("demanda").GetDecimal().Should().Be(9m);
        maior.GetProperty("culturaPredominante").GetString().Should().NotBeNullOrEmpty();
        dados.GetProperty("culturas").EnumerateArray().Select(c => c.GetProperty("codigo").GetString()).Should().Contain("CAFE");
    }

    [Fact]
    public async Task Categoria_que_nao_existe_e_recusada_no_campo()
    {
        var http = await ComDemandaAsync();

        var resposta = await http.GetAsync($"{Rota}?categoria=NAO_EXISTE");

        // 422, COMO TODA RECUSA DE PARÂMETRO DA API: o pedido chegou certo, o valor é que não vale.
        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("categoria");
    }
}
