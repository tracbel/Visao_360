using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    private static readonly decimal[] HectaresDoAnoAnterior = [120m, 300m, 300m];

    private static DateOnly Hoje => ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow);

    /// <summary>O ano fiscal corrente e o primeiro dia dele — é nele que a entrega deste ano cai, qualquer que seja o dia do teste.</summary>
    private static int AnoFiscalCorrente => AnoFiscal.Do(AnoFiscal.MesCorrenteEmSaoPaulo(DateTime.UtcNow));

    private static DateOnly InicioDoAnoFiscal => AnoFiscal.Inteiro(AnoFiscalCorrente).Inicial;

    /// <summary>
    /// AS ENTREGAS DO ART (02/10/2026): um comprador com endereço principal em D3, e três vendas — uma entregue no primeiro
    /// dia deste ano fiscal, uma no mesmo dia do ano fiscal anterior, e uma de máquina sem linha de produto (sem categoria),
    /// que não entra: não se sabe se é trator.
    /// </summary>
    private static async Task SemearEntregasAsync(CrmDbContext db, Municipio d3, DateTime agora)
    {
        var art = Dominio.Integracao.Sistema.Criar("ART_DEMANDA", "ART de teste da demanda", "View somente leitura");
        db.Sistemas.Add(art);
        var tratorMedio = LinhaDeProduto.Criar("TRATOR_MEDIO", "Trator médio", null, PorteDeMaquina.Medio);
        db.LinhasDeProduto.Add(tratorMedio);
        var comprador = Cliente.Criar(1, "Comprador de D3", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.Add(comprador);
        await db.SaveChangesAsync();

        db.Enderecos.Add(Endereco.Criar(1, comprador.Id, TipoDeEndereco.Fiscal, "Rua do teste",
            MunicipioDoEndereco.Selecionado(d3.Id), "SP", 100, ehPrincipal: true));

        Equipamento Maquina(string chassi, int? linha) =>
            Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(chassi), OrigemDoEquipamento.Art, 100, null, linha);
        var maquinas = new[] { Maquina("1DEMANDA000000001", tratorMedio.Id), Maquina("1DEMANDA000000002", tratorMedio.Id), Maquina("1DEMANDA000000003", null) };
        db.Equipamentos.AddRange(maquinas);
        await db.SaveChangesAsync();

        void Entregar(int posicao, DateOnly entregueEm) =>
            db.VendasDeMaquina.Add(VendaDeMaquina.Registrar(
                art.Id, $"ART-DEMANDA-{posicao}", maquinas[posicao].Id, comprador.Id,
                new DadosDaVendaNaOrigem(
                    1, null, entregueEm.AddDays(-20), entregueEm.AddDays(-10), entregueEm, agora,
                    null, null, null, "Varejo", false, false, 1, "TRATOR MEDIO", "PRODUTO DE TESTE",
                    "Agro Teste", "Unidade de teste", null, $"hash-demanda-{posicao}", null),
                agora, 100));

        Entregar(0, InicioDoAnoFiscal);
        Entregar(1, InicioDoAnoFiscal.AddYears(-1));
        Entregar(2, InicioDoAnoFiscal);
        await db.SaveChangesAsync();
    }

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

            foreach (var ((municipio, (_, _, hectares)), antes) in municipios.Zip(MunicipiosDaAdr).Zip(HectaresDoAnoAnterior))
            {
                db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                    municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 2, 100, agora));
                db.ProducoesAgricolasNosMunicipios.Add(ProducaoAgricolaNoMunicipio.Registrar(
                    municipio.Id, 2024, Cafe, "Café (em grão) Total", new(hectares, hectares, hectares * 2, hectares * 10), 100, agora));
                // O ANO ANTERIOR DA PAM (02/10/2026): 120, 300 e 300 ha — D1 cresceu 25%, D2 ficou igual e D3 cresceu 50%.
                db.ProducoesAgricolasNosMunicipios.Add(ProducaoAgricolaNoMunicipio.Registrar(
                    municipio.Id, 2023, Cafe, "Café (em grão) Total", new(antes, antes, antes * 2, antes * 10), 100, agora));
            }

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

    // =============================================================================================
    // A maquete de 02/10/2026: o ano anterior da PAM, as entregas do ART, o ano fiscal e a cultura
    // =============================================================================================

    [Fact]
    public async Task O_ano_anterior_e_a_mesma_conta_com_a_area_da_pam_de_um_ano_antes()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        var totais = dados.GetProperty("totais");
        totais.GetProperty("parqueAnoAnterior").GetDecimal().Should().Be(72m, "120 + 300 + 300 ha ÷ 10 ha por máquina");
        totais.GetProperty("demandaEstruturalAnoAnterior").GetDecimal().Should().Be(14.4m, "o parque ÷ 5 anos");
        totais.GetProperty("culturasComAumentoDeArea").EnumerateArray().Should().ContainSingle("a área do café cresceu de 720 para 900 ha");
        dados.GetProperty("anoDaAreaAnterior").GetInt32().Should().Be(2023);

        var cafe = dados.GetProperty("porCultura").EnumerateArray().Single(c => c.GetProperty("culturaCodigo").GetString() == "CAFE");
        (cafe.GetProperty("areaAnoAnterior").GetDecimal(), cafe.GetProperty("demandaAnoAnterior").GetDecimal()).Should().Be((720m, 14.4m));

        decimal Variacao(int codigo) => dados.GetProperty("municipios").EnumerateArray()
            .Single(m => m.GetProperty("codigoIbge").GetInt32() == codigo).GetProperty("variacaoAnoAnterior").GetDecimal();
        (Variacao(3598001), Variacao(3598002), Variacao(3598003)).Should().Be((25m, 0m, 50m));
    }

    [Fact]
    public async Task A_entrega_realizada_e_a_do_art_pela_data_da_entrega_no_ano_fiscal_e_no_mesmo_trecho_do_anterior()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        dados.GetProperty("anoFiscal").GetInt32().Should().Be(AnoFiscalCorrente);
        dados.GetProperty("anosFiscais").EnumerateArray().Select(a => a.GetInt32()).Should().Equal(
            AnoFiscalCorrente, AnoFiscalCorrente - 1, AnoFiscalCorrente - 2, AnoFiscalCorrente - 3);

        // NOVEMBRO TEM A ENTREGA DO PRIMEIRO DIA DO ANO: a máquina sem categoria não entra — não se sabe se é trator.
        var novembro = dados.GetProperty("previsaoMensal").EnumerateArray().First();
        novembro.GetProperty("entregues").GetInt32().Should().Be(1);

        var totais = dados.GetProperty("totais");
        totais.GetProperty("entreguesNoPeriodo").GetInt32().Should().Be(1);
        totais.GetProperty("entreguesNoPeriodoAnterior").GetInt32().Should().Be(1, "a entrega do mesmo dia do ano fiscal anterior");
        dados.GetProperty("lacunas").EnumerateArray().Select(l => l.GetProperty("metrica").GetString())
            .Should().Contain(["entregaRealizada", "anoAnterior"]);
    }

    [Fact]
    public async Task O_ano_fiscal_anterior_mostra_as_entregas_dele_e_todos_os_meses_fechados()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?anoFiscal={AnoFiscalCorrente - 1}"));

        dados.GetProperty("anoFiscal").GetInt32().Should().Be(AnoFiscalCorrente - 1);
        dados.GetProperty("totais").GetProperty("entreguesNoPeriodo").GetInt32().Should().Be(1);
        dados.GetProperty("totais").GetProperty("entreguesNoPeriodoAnterior").GetInt32().Should().Be(0);
        dados.GetProperty("previsaoMensal").EnumerateArray().Should().OnlyContain(m => m.GetProperty("entregues").ValueKind == JsonValueKind.Number,
            "o ano fiscal passado está fechado: todo mês tem a entrega contada, mesmo zero");
    }

    [Fact]
    public async Task A_cultura_recorta_a_demanda_e_deixa_a_entrega_de_fora_com_o_motivo()
    {
        var http = await ComDemandaAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?cultura=cafe"));

        dados.GetProperty("cultura").GetString().Should().Be("CAFE");
        dados.GetProperty("culturasDoFiltro").EnumerateArray().Select(c => c.GetProperty("codigo").GetString()).Should().Contain("CAFE");
        dados.GetProperty("totais").GetProperty("demandaEstrutural").GetDecimal().Should().Be(18m);
        dados.GetProperty("totais").GetProperty("entreguesNoPeriodo").ValueKind.Should().Be(JsonValueKind.Null, "a máquina não diz a cultura");
        dados.GetProperty("lacunas").EnumerateArray().Select(l => l.GetProperty("metrica").GetString()).Should().Contain("entregaPorCultura");
    }

    [Fact]
    public async Task O_potencial_e_o_mesmo_para_as_duas_filiais_e_a_entrega_e_so_a_do_alcance_de_cada_uma()
    {
        var ribeirao = await ComDemandaAsync();
        var barretos = api.ClienteDeBarretos();

        var deRibeirao = await DadosAsync(await ribeirao.GetAsync(Rota));
        var deBarretos = await DadosAsync(await barretos.GetAsync(Rota));

        // O POTENCIAL É DADO DE REFERÊNCIA (documento 54): a área de atuação, a PAM e a regra não têm dono de filial.
        deBarretos.GetProperty("totais").GetProperty("demandaEstrutural").GetDecimal()
            .Should().Be(deRibeirao.GetProperty("totais").GetProperty("demandaEstrutural").GetDecimal(), "o potencial é dado de referência");
        deRibeirao.GetProperty("totais").GetProperty("entreguesNoPeriodo").GetInt32().Should().Be(1);
        deBarretos.GetProperty("totais").GetProperty("entreguesNoPeriodo").ValueKind.Should().NotBe(JsonValueKind.Number,
            "a venda do ART é da filial de Ribeirão, fora do alcance de Barretos");
    }

    [Theory]
    [InlineData("anoFiscal=2019", "anoFiscal")]
    [InlineData("anoFiscal=abc", "anoFiscal")]
    [InlineData("cultura=SOJA", "cultura")]
    public async Task Ano_fiscal_fora_do_filtro_e_cultura_sem_demanda_sao_recusados_no_campo(string consulta, string campo)
    {
        var http = await ComDemandaAsync();

        var resposta = await http.GetAsync($"{Rota}?{consulta}");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain($"\"{campo}\"");
    }
}
