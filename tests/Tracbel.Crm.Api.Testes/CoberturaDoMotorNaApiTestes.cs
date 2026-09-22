using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A COBERTURA DOS DADOS DO MOTOR, pela API de verdade (issue 150).
///
/// <para>O banco é semeado com um caso de cada situação: o café com um município da ADR sob sigilo (parcial), a
/// soja divulgada nos dois (completa) e o milho só fora da ADR (não aparece); uma série da CONAB com um mês
/// faltando; uma aba de custo que parou no operacional; crédito de máquina em Franca nas duas janelas e em
/// Cravinhos só na última; e o dado interno de duas filiais — que prova que cada filial conta só o seu.</para>
/// </summary>
[Trait("Categoria", "ParametrosDoPotencial")]
public sealed class CoberturaDoMotorNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
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

        // A PAM: café com Cravinhos sob sigilo; soja divulgada nos dois (zero é medida); milho só em Uberaba.
        db.ProducoesAgricolasNosMunicipios.AddRange(
            ProducaoAgricolaNoMunicipio.Registrar(franca.Id, 2023, 40139, "Café (em grão) Total", new(29_000m, 28_000m, 48_000m, 800_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(franca.Id, 2024, 40139, "Café (em grão) Total", new(30_000m, 29_000m, 50_000m, 900_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(cravinhos.Id, 2024, 40139, "Café (em grão) Total", new(null, null, null, null), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(franca.Id, 2024, 40124, "Soja (em grão)", new(0m, 0m, 0m, 0m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(cravinhos.Id, 2024, 40124, "Soja (em grão)", new(500m, 500m, 1_500m, 3_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(uberaba.Id, 2024, 40122, "Milho (em grão)", new(90_000m, 90_000m, 300_000m, 600_000m), 100, agora));

        // A CONAB sem novembro na soja; a Socicana com o mensal e o acumulado do mesmo kg de ATR.
        db.CotacoesDeProdutos.AddRange(
            CotacaoDeProduto.Registrar("CONAB", "4744", "SP", "RECEBIDO PELO PRODUTOR", "SOJA", "EM GRÃOS", "kg", new DateOnly(2025, 9, 1), 2.10m, 100, agora),
            CotacaoDeProduto.Registrar("CONAB", "4744", "SP", "RECEBIDO PELO PRODUTOR", "SOJA", "EM GRÃOS", "kg", new DateOnly(2025, 10, 1), 2.12m, 100, agora),
            CotacaoDeProduto.Registrar("CONAB", "4744", "SP", "RECEBIDO PELO PRODUTOR", "SOJA", "EM GRÃOS", "kg", new DateOnly(2025, 12, 1), 2.15m, 100, agora),
            CotacaoDeProduto.Registrar("SOCICANA", "ATR", "SP", "MENSAL", "CANA DE AÇÚCAR", "KG DE ATR", "kg de ATR", new DateOnly(2026, 7, 1), 0.85m, 100, agora),
            CotacaoDeProduto.Registrar("SOCICANA", "ATR", "SP", "MENSAL", "CANA DE AÇÚCAR", "KG DE ATR", "kg de ATR", new DateOnly(2026, 8, 1), 0.87m, 100, agora),
            CotacaoDeProduto.Registrar("SOCICANA", "ATR", "SP", "ACUMULADO DA SAFRA", "CANA DE AÇÚCAR", "KG DE ATR", "kg de ATR", new DateOnly(2026, 8, 1), 0.86m, 100, agora));

        // O custo da cana: Piracicaba chega ao total; Penápolis 2016 parou no operacional.
        var comTotal = new CustoDeProducao.Valores(9, 77987.01m, "kg/ha", 10330.59m, 132.24702m, 2663.69m, 34.52645m, 12994.28m, 166.77347m,
            908.92m, 11.76423m, 13903.20m, 178.5377m);
        db.CustosDeProducao.AddRange(
            CustoDeProducao.Registrar("CANA DE AÇÚCAR", "Piracicaba-SP-2024", "Piracicaba", null, null, 2024, "t", comTotal, 100, agora),
            CustoDeProducao.Registrar("CANA DE AÇÚCAR", "Penapolis-SP-2016", "Penápolis", null, null, 2016, "t",
                comTotal with { RendaDeFatoresHa = null, RendaDeFatoresUnidade = null, CustoTotalHa = null, CustoTotalUnidade = null }, 100, agora));

        // O SICOR: trator em Franca nas duas janelas (08/2025 e 08/2026), em Cravinhos só em 07/2026, e em Uberaba,
        // que não é da ADR. O último mês com dado é 08/2026.
        db.CreditosRuraisDeInvestimento.AddRange(
            CreditoRuralDeInvestimento.Registrar(new(6482, 2025, 8, 7080, 154, 71, 431, 9, 1, 14), franca.Id, 500_000m, 0m, 100, agora),
            CreditoRuralDeInvestimento.Registrar(new(6482, 2026, 8, 7080, 154, 71, 431, 9, 1, 14), franca.Id, 600_000m, 0m, 100, agora),
            CreditoRuralDeInvestimento.Registrar(new(7001, 2026, 7, 7080, 154, 71, 431, 9, 1, 14), cravinhos.Id, 300_000m, 0m, 100, agora),
            CreditoRuralDeInvestimento.Registrar(new(9999, 2026, 8, 7080, 154, 71, 431, 9, 1, 14), uberaba.Id, 900_000m, 0m, 100, agora));

        // O dado interno de Ribeirão (filial 1): um cliente em Franca e outro com cidade que a carga não casou;
        // duas vendas perdidas, uma completa e uma sem nada. Barretos (filial 2) tem uma venda perdida só dela.
        var noMapa = Cliente.Criar(1, "Cliente no mapa", TipoDePessoa.Juridica, 100, 100);
        var foraDoMapa = Cliente.Criar(1, "Cliente fora do mapa", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.AddRange(noMapa, foraDoMapa);
        var motivo = MotivoDePerda.Criar("PRECO_COBERTURA", "Preço", CategoriaDeMotivoDePerda.Preco);
        db.MotivosDePerda.Add(motivo);
        await db.SaveChangesAsync();

        db.Enderecos.AddRange(
            Endereco.Criar(1, noMapa.Id, TipoDeEndereco.Fiscal, "Rua do teste", MunicipioDoEndereco.Selecionado(franca.Id), "SP", 100, ehPrincipal: true),
            Endereco.Criar(1, foraDoMapa.Id, TipoDeEndereco.Fiscal, "Rua do teste", MunicipioDoEndereco.NaoIdentificadoNaCarga("CIDADE SEM CADASTRO"), "SP", 100, ehPrincipal: true));

        db.VendasPerdidas.AddRange(
            VendaPerdida.Criar(1, agora.AddDays(-30), motivo.Id, clienteId: noMapa.Id, modeloOfertado: "8R 340", precoOfertado: 1_000m),
            VendaPerdida.Criar(1, agora.AddDays(-10), motivo.Id),
            VendaPerdida.Criar(2, agora.AddDays(-5), motivo.Id, modeloOfertado: "6M 150"));

        await db.SaveChangesAsync();
    }

    private static JsonElement Grupo(JsonElement dados, string codigo) =>
        dados.GetProperty("grupos").EnumerateArray().Single(g => g.GetProperty("codigo").GetString() == codigo);

    private static JsonElement Item(JsonElement grupo, string codigo) =>
        grupo.GetProperty("itens").EnumerateArray().Single(i => i.GetProperty("codigo").GetString() == codigo);

    private async Task<JsonElement> LerComoAsync(HttpClient cliente)
    {
        var resposta = await cliente.GetAsync("/api/v1/integracoes/cobertura-do-motor");
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados");
    }

    [Fact]
    public async Task A_producao_agricola_so_lista_o_que_se_planta_na_ADR_e_o_sigilo_fica_parcial()
    {
        await SemearAsync();
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var dados = await LerComoAsync(api.ClienteDeRibeirao());
        dados.GetProperty("municipiosDaAdr").GetInt32().Should().Be(2);

        var pam = Grupo(dados, "PAM");
        pam.GetProperty("fonte").GetString().Should().Contain("2024");
        pam.GetProperty("itens").EnumerateArray().Select(i => i.GetProperty("codigo").GetString())
            .Should().Equal(["PAM.40139", "PAM.40124"], "o café tem mais área na ADR que a soja; o milho só se planta em Uberaba");

        var cafe = Item(pam, "PAM.40139");
        cafe.GetProperty("situacao").GetString().Should().Be("Parcial");
        cafe.GetProperty("cobertos").GetInt32().Should().Be(1, "Cravinhos veio sob sigilo — nulo não é divulgado");
        cafe.GetProperty("percentual").GetDecimal().Should().Be(50m);
        cafe.GetProperty("periodoInicial").GetString().Should().Be("2023");
        cafe.GetProperty("periodoFinal").GetString().Should().Be("2024");
        cafe.GetProperty("motivo").GetString().Should().Be("1 de 2 municípios da ADR com o dado — falta 1 para o potencial estrutural desta cultura.");

        var soja = Item(pam, "PAM.40124");
        soja.GetProperty("situacao").GetString().Should().Be("Completa", "zero em Franca é medida, não falta de dado");
        soja.GetProperty("detalhe").GetString().Should().Contain("planta-se em 1")
            .And.Contain("quantidade em 2 (toneladas)", "a quantidade volta com a unidade da PAM (issue 152)");
    }

    [Fact]
    public async Task Preco_custo_e_credito_mostram_o_mes_que_falta_a_aba_sem_total_e_a_base_do_indice()
    {
        await SemearAsync();
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var dados = await LerComoAsync(api.ClienteDeRibeirao());

        var precos = Grupo(dados, "PRECOS");
        var soja = Item(precos, "PRECO.CONAB.4744.RECEBIDO PELO PRODUTOR");
        soja.GetProperty("total").GetInt32().Should().Be(4, "de 09/2025 a 12/2025");
        soja.GetProperty("cobertos").GetInt32().Should().Be(3);
        soja.GetProperty("detalhe").GetString().Should().Contain("faltam 11/2025").And.Contain("24 meses");
        precos.GetProperty("itens")[0].GetProperty("codigo").GetString().Should().Be(soja.GetProperty("codigo").GetString(),
            "o que falta vem primeiro");
        Item(precos, "PRECO.SOCICANA.ATR.MENSAL").GetProperty("nome").GetString().Should().Be("Cana de açúcar — kg de ATR (mensal)",
            "duas séries com o mesmo produto e classificação só se distinguem pelo nível, e ATR continua sigla");

        var custo = Item(Grupo(dados, "CUSTOS"), "CUSTO.CANA DE AÇÚCAR");
        custo.GetProperty("nome").GetString().Should().Be("Cana de açúcar");
        custo.GetProperty("total").GetInt32().Should().Be(2);
        custo.GetProperty("cobertos").GetInt32().Should().Be(1, "Penápolis 2016 parou no custo operacional");
        custo.GetProperty("periodoInicial").GetString().Should().Be("2016");
        custo.GetProperty("detalhe").GetString().Should().StartWith("2 locais de SP");

        var credito = Grupo(dados, "CREDITO");
        var meses = Item(credito, "CREDITO.MESES");
        meses.GetProperty("total").GetInt32().Should().Be(13, "de 08/2025 a 08/2026");
        meses.GetProperty("cobertos").GetInt32().Should().Be(3);
        var municipios = Item(credito, "CREDITO.MUNICIPIOS");
        municipios.GetProperty("cobertos").GetInt32().Should().Be(1, "só Franca tem linha de máquina nas duas janelas");
        municipios.GetProperty("detalhe").GetString().Should().Contain("só na última janela: 1").And.Contain("em nenhuma: 0");
    }

    [Fact]
    public async Task O_dado_interno_e_contagem_sem_valor_e_cada_filial_conta_so_o_seu()
    {
        await SemearAsync();
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        await api.ConcederPerfilAsync(200, PerfisDeSistema.Gerencia);

        var ribeirao = await LerComoAsync(api.ClienteDeRibeirao());
        var perdidas = Grupo(ribeirao, "VENDAS_PERDIDAS");
        perdidas.GetProperty("ehInterno").GetBoolean().Should().BeTrue();
        Item(perdidas, "INTERNO.PERDA.PRECO").GetProperty("total").GetInt32().Should().Be(2, "a de Barretos não entra na conta de Ribeirão");
        Item(perdidas, "INTERNO.PERDA.PRECO").GetProperty("cobertos").GetInt32().Should().Be(1);
        Item(perdidas, "INTERNO.PERDA.MUNICIPIO").GetProperty("cobertos").GetInt32().Should().Be(1);
        ribeirao.GetProperty("filiaisNoAlcance").GetInt32().Should().BeGreaterThan(0);

        var enderecos = Grupo(ribeirao, "ENDERECOS");
        Item(enderecos, "INTERNO.ENDERECO.MUNICIPIO").GetProperty("situacao").GetString().Should().Be("Parcial");
        Item(enderecos, "INTERNO.ENDERECO.AREA_E_CULTURA").GetProperty("situacao").GetString().Should().Be("Vazia");

        var vendas = Item(Grupo(ribeirao, "VENDAS_DE_MAQUINA"), "INTERNO.VENDA.MUNICIPIO");
        vendas.GetProperty("situacao").GetString().Should().Be("Vazia");
        vendas.GetProperty("motivo").GetString().Should().StartWith("Nenhum registro no banco");

        var barretos = await LerComoAsync(api.ClienteDeBarretos());
        Item(Grupo(barretos, "VENDAS_PERDIDAS"), "INTERNO.PERDA.MODELO").GetProperty("total").GetInt32().Should().Be(1);

        // NENHUM VALOR EM REAIS SAI DAQUI: nem o preço ofertado, nem o valor financiado, nem o da produção.
        var texto = ribeirao.GetRawText();
        texto.Should().NotContain("1000").And.NotContain("600000").And.NotContain("900000")
            .And.NotContain("Cliente no mapa");
    }

    [Fact]
    public async Task O_perfil_padrao_nao_le_a_cobertura()
    {
        // UMA API SÓ PARA ESTE CASO: os outros testes desta classe concedem a Gerência ao usuário de Ribeirão.
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        var resposta = await app.ClienteDeRibeirao().GetAsync("/api/v1/integracoes/cobertura-do-motor");

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain(Permissoes.IntegracaoLer);
    }
}
