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
/// O DIAGNÓSTICO COMERCIAL PELA API (issue 257). Três municípios da ADR com café e uma regra do trator com ciclo:
/// demanda de 3, 6 e 9 tratores por ano. Sem ART, sem carteira, sem SICOR e sem preço, só o potencial e os clientes
/// têm dado — e o índice sai deles, com os outros componentes declarados ausentes, e não zerados.
///
/// <para><b>Classe própria, banco próprio</b>: a regra com ciclo mudaria a demanda que as outras classes conferem.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class DiagnosticoComercialNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/mercado/diagnostico";
    private const int Cafe = 40139;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly (int Codigo, string Nome, decimal Hectares)[] MunicipiosDaAdr =
    [
        (3597001, "Município de Teste P", 150m), (3597002, "Município de Teste Q", 300m), (3597003, "Município de Teste R", 450m)
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

            // O SICOR DE UM MUNICÍPIO SÓ (R): seis linhas de trator nos últimos 12 meses e três nos 12 anteriores, com o
            // mesmo valor por linha — o crédito dobrou. Os outros dois municípios ficam sem linha nenhuma.
            var ultimoMes = new DateOnly(Hoje.Year, Hoje.Month, 1).AddMonths(-3);
            var meses = Enumerable.Range(0, 6).Select(i => ultimoMes.AddMonths(-i))
                .Concat(Enumerable.Range(0, 3).Select(i => ultimoMes.AddMonths(-12 - 2 * i)));
            foreach (var mes in meses)
                db.CreditosRuraisDeInvestimento.Add(CreditoRuralDeInvestimento.Registrar(
                    new CreditoRuralDeInvestimento.Chave(9003, (short)mes.Year, (byte)mes.Month, 7080, 154, 71, 431, 9, 1, 14),
                    municipios[2].Id, 100_000m, 0m, 100, agora));

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

    private static JsonElement Linha(JsonElement dados, int codigo) =>
        dados.GetProperty("municipios").EnumerateArray().Single(m => m.GetProperty("codigoIbge").GetInt32() == codigo);

    [Fact]
    public async Task O_ioc_sai_dos_componentes_com_dado_e_ordena_os_municipios()
    {
        var http = await ComDemandaAsync();

        var dados = await DadosAsync(await http.GetAsync(Rota));

        dados.GetProperty("categoria").GetString().Should().Be("TRATOR", "o padrão é o do protótipo");
        dados.GetProperty("fracaoDoAnoNoPeriodo").GetDecimal().Should().Be(1m, "doze meses fechados são um ano inteiro");
        dados.GetProperty("pesosDoPrototipo").GetBoolean().Should().BeTrue();
        // SEM ART, CARTEIRA NEM PREÇO, SÓ POTENCIAL (25) E CLIENTES (10) TÊM DADO — e o crédito (15), em R.
        var linhas = dados.GetProperty("municipios").EnumerateArray().ToList();
        linhas.Select(l => l.GetProperty("codigoIbge").GetInt32()).Should().Equal(3597003, 3597002, 3597001);

        var maior = Linha(dados, 3597003);
        maior.GetProperty("ioc").GetDecimal().Should().Be(100m);
        maior.GetProperty("classe").GetString().Should().Be("Maxima");
        maior.GetProperty("demandaEstrutural").GetDecimal().Should().Be(9m);
        maior.GetProperty("culturaPrincipal").GetString().Should().Be("Café");
        maior.GetProperty("metaDePlanejamento").GetDecimal().Should().BeGreaterThan(0m, "a demanda ajustada × o share-alvo de 31% do trator");

        // O CRÉDITO É O DO MUNICÍPIO: R dobrou as linhas e o valor, e o componente de crédito vai ao topo; Q, sem linha
        // nenhuma no SICOR, fica sem índice — e não herda o de R.
        maior.GetProperty("indiceDeCredito").GetDecimal().Should().BeGreaterThan(1.4m);
        maior.GetProperty("componentes").GetProperty("credito").GetDecimal().Should().Be(1m);
        Linha(dados, 3597002).GetProperty("indiceDeCredito").ValueKind.Should().Be(JsonValueKind.Null);

        // A RÉGUA É A DEMANDA AJUSTADA DE R (o percentil 90 dos três), e P e Q não têm crédito nem ajuste: o potencial
        // deles é a demanda estrutural sobre ela.
        var regua = maior.GetProperty("demandaAjustada").GetDecimal();
        dados.GetProperty("percentil90").GetDecimal().Should().Be(regua);
        decimal Esperado(decimal demanda) => decimal.Round(100m * (25m * Math.Min(1m, demanda / regua) + 10m) / 35m, 1);
        Linha(dados, 3597002).GetProperty("ioc").GetDecimal().Should().BeApproximately(Esperado(6m), 0.1m);
        Linha(dados, 3597001).GetProperty("ioc").GetDecimal().Should().BeApproximately(Esperado(3m), 0.1m);

        var resumo = dados.GetProperty("resumo");
        resumo.GetProperty("maxima").GetInt32().Should().Be(1);
        resumo.GetProperty("total").GetInt32().Should().Be(3);
        resumo.GetProperty("semIndice").GetInt32().Should().Be(0);

        // OS TOTAIS DOS CARTÕES (28/09/2026): a demanda é a soma dos três municípios; sem ART, as vendas e a penetração
        // ficam vazias — e não zero.
        resumo.GetProperty("demandaEstrutural").GetDecimal().Should().Be(18m, "3 + 6 + 9 tratores por ano");
        resumo.GetProperty("municipiosComDemanda").GetInt32().Should().Be(3);
        resumo.GetProperty("metaDePlanejamento").GetDecimal().Should().BeGreaterThan(0m);
        resumo.GetProperty("vendidasNoPeriodo").ValueKind.Should().Be(JsonValueKind.Null);
        resumo.GetProperty("penetracao").ValueKind.Should().Be(JsonValueKind.Null);

        // A LINHA TRAZ O CÓDIGO DA LOJA (o filtro recebe o código) e os clientes em partes, mesmo zerados.
        maior.TryGetProperty("lojaCodigo", out _).Should().BeTrue();
        maior.GetProperty("clientesPorClasse").GetProperty("semClasse").GetInt32().Should().Be(0);
        maior.GetProperty("clientesEmCarteira").GetInt32().Should().Be(0);
        maior.GetProperty("clientesQueCompraram").GetInt32().Should().Be(0);

        // O CEN / GESTOR DA MAQUETE (02/10/2026): a resposta traz os CENs que o filtro oferece, e cada município o CEN da
        // carteira com mais vínculos nele — sem carteira, nenhum, e não um nome inventado.
        dados.GetProperty("responsaveis").ValueKind.Should().Be(JsonValueKind.Array);
        maior.GetProperty("responsavel").ValueKind.Should().Be(JsonValueKind.Null, "nenhuma carteira tem vínculo nos municípios do cenário");
    }

    [Fact]
    public async Task O_que_nao_tem_dado_sai_da_conta_e_e_dito_e_nao_vira_zero()
    {
        var http = await ComDemandaAsync();

        var dados = await DadosAsync(await http.GetAsync(Rota));
        var linha = Linha(dados, 3597002);

        linha.GetProperty("vendidasNoPeriodo").ValueKind.Should().Be(JsonValueKind.Null, "sem ART, não há venda — nem zero");
        linha.GetProperty("componentes").GetProperty("realizacao").ValueKind.Should().Be(JsonValueKind.Null);
        linha.GetProperty("componentes").GetProperty("cobertura").ValueKind.Should().Be(JsonValueKind.Null);
        linha.GetProperty("componentes").GetProperty("credito").ValueKind.Should().Be(JsonValueKind.Null);

        var ausentes = linha.GetProperty("componentesAusentes").EnumerateArray().Select(a => a.GetString()!).ToList();
        ausentes.Should().Contain(a => a.StartsWith("realização") && a.Contains("ART"));
        ausentes.Should().Contain(a => a.StartsWith("cobertura"));
        ausentes.Should().Contain(a => a.StartsWith("crédito"));
        ausentes.Should().Contain(a => a.StartsWith("rentabilidade"));

        var lacunas = dados.GetProperty("lacunas").EnumerateArray().Select(l => l.GetProperty("metrica").GetString()).ToList();
        lacunas.Should().Contain("vendasEmUnidades").And.Contain("pesosDoIoc").And.Contain("cobertura");
    }

    [Fact]
    public async Task Categoria_sem_regra_nao_tem_demanda_e_o_indice_sai_vazio_com_o_motivo()
    {
        var http = await ComDemandaAsync();

        var dados = await DadosAsync(await http.GetAsync($"{Rota}?categoria=PULVERIZADOR"));

        dados.GetProperty("categoriaNome").GetString().Should().Be("Pulverizador");
        var linha = Linha(dados, 3597003);
        linha.GetProperty("demandaEstrutural").ValueKind.Should().Be(JsonValueKind.Null);
        linha.GetProperty("componentesAusentes").EnumerateArray().Select(a => a.GetString())
            .Should().Contain(a => a!.StartsWith("potencial"));
        dados.GetProperty("categorias").EnumerateArray().Select(c => c.GetProperty("codigo").GetString())
            .Should().Equal(["TRATOR"], "o seletor oferece só as categorias com demanda na ADR");
    }

    [Fact]
    public async Task Parametro_que_nao_vale_e_recusado_no_campo()
    {
        var http = await ComDemandaAsync();

        var resposta = await http.GetAsync($"{Rota}?categoria=TRATORR&competenciaFinal=2026-13&responsavel=999999");

        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity, corpo);
        corpo.Should().Contain("\"categoria\"").And.Contain("\"competenciaFinal\"").And.Contain("\"responsavel\"",
            "o CEN do filtro é um responsável de carteira ao alcance — e não qualquer número");
    }

    [Fact]
    public async Task Periodo_de_outro_tamanho_e_levado_a_um_ano_pela_sazonalidade()
    {
        var http = await ComDemandaAsync();
        var ultimo = new DateOnly(Hoje.Year, Hoje.Month, 1).AddMonths(-1);

        var dados = await DadosAsync(await http.GetAsync(
            $"{Rota}?competenciaInicial={ultimo.AddMonths(-5):yyyy-MM}&competenciaFinal={ultimo:yyyy-MM}"));

        // SEIS MESES: a fração é a soma da sazonalidade desses meses, e não 0,5.
        var sazonalidade = new[] { 6.52m, 6.89m, 8.56m, 8.59m, 8.87m, 8.58m, 7.84m, 8.75m, 9.15m, 10.51m, 7.44m, 8.30m };
        var esperado = Enumerable.Range(0, 6).Sum(i => sazonalidade[ultimo.AddMonths(-i).Month - 1]) / 100m;
        dados.GetProperty("fracaoDoAnoNoPeriodo").GetDecimal().Should().Be(decimal.Round(esperado, 4));
        dados.GetProperty("lacunas").EnumerateArray().Select(l => l.GetProperty("metrica").GetString()).Should().Contain("periodo");
    }
}
