using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O DIMENSIONAMENTO DA ADR PELA API (issue 259).
///
/// <para><b>O cenário, com as contas à mão:</b></para>
/// <list type="bullet">
/// <item>Município D1 (ADR, Norte, loja de Ribeirão, uma usina), em 2024: café 100 ha (80 colhidos, 160 t, R$ 1.000 mil);
/// soja 50 ha (150 t, R$ 300 mil); abacaxi 10 ha (500 MIL FRUTOS, R$ 50 mil). E o café Arábica 100 ha — o detalhado do
/// "Café Total", que NÃO pode somar. Em 2023, só o café: 80 ha, R$ 800 mil.</item>
/// <item>Município D2 (ADR, Noroeste, loja de Barretos), em 2024: café 200 ha (400 t, R$ 2.000 mil).</item>
/// <item>Município D3 (fora da ADR): café 1.000 ha, R$ 9.000 mil — só no mapa.</item>
/// <item>São Paulo em 2024: café 2.000 ha (R$ 20.000 mil), soja 500 ha (R$ 3.000 mil), abacaxi 100 ha (R$ 500 mil).</item>
/// </list>
/// <para>A ADR em 2024: área 160 + 200 = 360 ha; quantidade 310 + 400 = 710 t (o abacaxi fica fora); valor 1.350 + 2.000 =
/// R$ 3.350 mil — 14,26% dos R$ 23.500 mil do estado.</para>
/// <para>A carteira de Ribeirão: c1 (A, D1, contato há 10 dias), c2 (B, D1, há 100), c3 (A, D1, nunca), c4 (C, D3, há 50) e
/// c5 (sem classe, D2, há 200).</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class DimensionamentoDaAdrNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/mercado/dimensionamento";
    private const int Cafe = 40139, CafeArabica = 40140, Soja = 40124, Abacaxi = 40092;
    private const int D1 = 3598101, D2 = 3598102, D3 = 3598103;

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").Clone();
    }

    private async Task<HttpClient> SemearAsync()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var http = api.ClienteDeRibeirao();

        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        if (await db.Municipios.AnyAsync(m => m.CodigoIbge == D1)) return http;

        var agora = DateTime.UtcNow;
        var d1 = Municipio.Criar("Município Dim 1", "SP", D1);
        var d2 = Municipio.Criar("Município Dim 2", "SP", D2);
        var d3 = Municipio.Criar("Município Dim 3", "SP", D3);
        db.Municipios.AddRange(d1, d2, d3);
        await db.SaveChangesAsync();

        db.MunicipiosDaAreaDeAtuacao.AddRange(
            MunicipioDaAreaDeAtuacao.Registrar(d1.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 2, 100, agora),
            MunicipioDaAreaDeAtuacao.Registrar(d2.Id, true, RegiaoDaAreaDeAtuacao.Noroeste, 2, "Area de Atuação.xlsx", 3, 100, agora));
        db.UsinasDeEtanol.Add(UsinaDeEtanol.Registrar("12345678000190", "Usina de Teste", d1.Id, new DateOnly(2026, 9, 1), 100, 200, 100, agora));

        db.ProducoesAgricolasNosMunicipios.AddRange(
            ProducaoAgricolaNoMunicipio.Registrar(d1.Id, 2024, Cafe, "Café (em grão) Total", new(100m, 80m, 160m, 1_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(d1.Id, 2024, CafeArabica, "Café (em grão) Arábica", new(100m, 80m, 160m, 1_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(d1.Id, 2024, Soja, "Soja (em grão)", new(50m, 50m, 150m, 300m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(d1.Id, 2024, Abacaxi, "Abacaxi", new(10m, 10m, 500m, 50m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(d1.Id, 2023, Cafe, "Café (em grão) Total", new(80m, 80m, 120m, 800m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(d2.Id, 2024, Cafe, "Café (em grão) Total", new(200m, 200m, 400m, 2_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(d3.Id, 2024, Cafe, "Café (em grão) Total", new(1_000m, 1_000m, 2_000m, 9_000m), 100, agora));

        db.ProducoesAgricolasNosEstados.AddRange(
            ProducaoAgricolaNoEstado.Registrar(35, 2024, Cafe, "Café (em grão) Total", new(2_000m, 1_800m, 4_000m, 20_000m), 100, agora),
            ProducaoAgricolaNoEstado.Registrar(35, 2024, CafeArabica, "Café (em grão) Arábica", new(2_000m, 1_800m, 4_000m, 20_000m), 100, agora),
            ProducaoAgricolaNoEstado.Registrar(35, 2024, Soja, "Soja (em grão)", new(500m, 500m, 1_500m, 3_000m), 100, agora),
            ProducaoAgricolaNoEstado.Registrar(35, 2024, Abacaxi, "Abacaxi", new(100m, 100m, 5_000m, 500m), 100, agora));

        var linha = LinhaDeNegocio.Criar("MAQ_DIM", "Máquinas do dimensionamento");
        db.LinhasDeNegocio.Add(linha);
        await db.SaveChangesAsync();

        var carteira = Carteira.Criar(1, linha.Id, "DIM_RP", "Carteira do Dimensionamento", 100, 100);
        db.Carteiras.Add(carteira);

        (string Nome, ClasseDeCliente? Classe, Municipio Onde, int? DiasDesdeOContato)[] clientes =
        [
            ("Cliente Dim 1", ClasseDeCliente.A, d1, 10),
            ("Cliente Dim 2", ClasseDeCliente.B, d1, 100),
            ("Cliente Dim 3", ClasseDeCliente.A, d1, null),
            ("Cliente Dim 4", ClasseDeCliente.C, d3, 50),
            ("Cliente Dim 5", null, d2, 200)
        ];

        var criados = clientes.Select(c => Cliente.Criar(1, c.Nome, TipoDePessoa.Juridica, 100, 100)).ToList();
        db.Clientes.AddRange(criados);
        await db.SaveChangesAsync();

        foreach (var (cliente, (_, classe, onde, dias)) in criados.Zip(clientes))
        {
            if (classe is { } c) cliente.ApurarClasse(c, 1_000m, agora, 100);
            db.Enderecos.Add(Endereco.Criar(
                1, cliente.Id, TipoDeEndereco.Fiscal, "Rua do teste", MunicipioDoEndereco.Selecionado(onde.Id), "SP", 100, ehPrincipal: true));
            var vinculo = ClienteCarteira.Criar(cliente.Id, carteira.Id, ClasseDeCliente.C, 100);
            if (dias is { } d) vinculo.RegistrarInteracao(agora.AddDays(-d));
            db.ClienteCarteiras.Add(vinculo);
        }

        await db.SaveChangesAsync();
        return http;
    }

    [Fact]
    public async Task A_ADR_soma_os_municipios_e_o_estado_e_o_total_publicado_sem_contar_o_cafe_duas_vezes()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?anoBase=2024"));

        (dados.GetProperty("anoBase").GetInt16(), dados.GetProperty("anoAnterior").GetInt16()).Should().Be(((short)2024, (short)2023));
        dados.GetProperty("municipiosNoRecorte").GetInt32().Should().Be(2, "D3 não é da ADR");

        var totais = dados.GetProperty("totais");
        totais.GetProperty("area").GetProperty("atual").GetDecimal().Should().Be(360m, "160 + 200, sem o Arábica");
        totais.GetProperty("quantidade").GetProperty("atual").GetDecimal().Should().Be(710m, "os 500 mil frutos do abacaxi ficam fora");
        totais.GetProperty("valor").GetProperty("atual").GetDecimal().Should().Be(3_350m);
        totais.GetProperty("valor").GetProperty("anterior").GetDecimal().Should().Be(800m);
        totais.GetProperty("valor").GetProperty("variacaoPercentual").GetDecimal().Should().Be(318.8m, "3.350 ÷ 800 − 1");
        totais.GetProperty("produtividade").ValueKind.Should().Be(JsonValueKind.Null, "sem cultura não há produtividade");

        var noEstado = dados.GetProperty("tracbelNoEstado").GetProperty("valor");
        noEstado.GetProperty("todo").GetDecimal().Should().Be(23_500m, "o total publicado, sem o Arábica");
        noEstado.GetProperty("percentual").GetDecimal().Should().Be(14.26m);
    }

    [Fact]
    public async Task Com_a_cultura_vem_a_produtividade_pela_area_colhida_e_as_outras_ficam_sem_quantidade()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?anoBase=2024&cultura=CAFE"));

        dados.GetProperty("cultura").GetString().Should().Be("CAFE");
        var totais = dados.GetProperty("totais");
        totais.GetProperty("area").GetProperty("atual").GetDecimal().Should().Be(300m);
        totais.GetProperty("produtividade").GetDecimal().Should().Be(2m, "(160 + 400) t ÷ (80 + 200) ha colhidos");
        dados.GetProperty("tracbelNoEstado").GetProperty("area").GetProperty("percentual").GetDecimal().Should().Be(15m, "300 de 2.000 ha");

        var outras = dados.GetProperty("porCultura").EnumerateArray().Single(c => c.GetProperty("codigo").GetString() == "OUTRAS");
        outras.GetProperty("area").GetProperty("parte").GetDecimal().Should().Be(10m, "o abacaxi entra na área");
        outras.GetProperty("quantidade").GetProperty("parte").ValueKind.Should().Be(JsonValueKind.Null, "mil frutos não soma com tonelada");

        var d1 = dados.GetProperty("porLoja").EnumerateArray().Single(l => l.GetProperty("lojaCodigo").GetString() == ApiEmMemoria.FilialDeRibeirao);
        d1.GetProperty("tecnificacao").GetDecimal().Should().Be(100m, "160 t ÷ 80 ha = 2 t/ha, a mesma média do recorte");
    }

    [Fact]
    public async Task O_mapa_traz_o_estado_inteiro_com_a_ADR_em_foco()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?anoBase=2024"));

        var mapa = dados.GetProperty("mapa").EnumerateArray().ToDictionary(m => m.GetProperty("codigoIbge").GetInt32());
        mapa[D1].GetProperty("emFoco").GetBoolean().Should().BeTrue();
        mapa[D3].GetProperty("emFoco").GetBoolean().Should().BeFalse();
        mapa[D3].GetProperty("valor").GetDecimal().Should().Be(9_000m, "fora da ADR continua no mapa");
        mapa[D1].GetProperty("clientes").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task A_carteira_conta_as_faixas_acumuladas_e_o_AB_sem_contato()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?anoBase=2024"));

        var carteira = dados.GetProperty("carteira");
        var clientes = carteira.GetProperty("clientes");
        clientes.GetProperty("clientes").GetInt32().Should().Be(5);
        (carteira.GetProperty("naRegiao").GetInt32(), carteira.GetProperty("fora").GetInt32()).Should().Be((4, 1), "c4 mora em D3, fora da ADR");
        (clientes.GetProperty("a").GetInt32(), clientes.GetProperty("b").GetInt32(), clientes.GetProperty("c").GetInt32(),
            clientes.GetProperty("semClasse").GetInt32()).Should().Be((2, 1, 1, 1));

        var faixas = clientes.GetProperty("faixas");
        (faixas.GetProperty("ate30").GetInt32(), faixas.GetProperty("ate60").GetInt32(), faixas.GetProperty("ate90").GetInt32(),
            faixas.GetProperty("ate120").GetInt32(), faixas.GetProperty("sem120").GetInt32()).Should().Be((1, 2, 2, 3, 2));
        clientes.GetProperty("abSemContato").GetInt32().Should().Be(1, "c3 é A e nunca foi contatado; c2 é B e foi há 100 dias");
        carteira.GetProperty("potencialMedio").GetDecimal().Should().Be(3.25m, "(4×2 + 3×1 + 2×1) ÷ 4, sem o sem classe");

        var prioritario = dados.GetProperty("prioritarios").EnumerateArray().Single();
        prioritario.GetProperty("codigoIbge").GetInt32().Should().Be(D1, "D2 tem um cliente só");
        prioritario.GetProperty("coberturaAte90Percentual").GetDecimal().Should().Be(33.3m);
        prioritario.GetProperty("prioridade").GetDecimal().Should().Be(450m, "R$ 1.350 mil × 1/3 sem contato");

        var vendedor = dados.GetProperty("porVendedor").EnumerateArray().Single();
        (vendedor.GetProperty("naRegiao").GetInt32(), vendedor.GetProperty("fora").GetInt32()).Should().Be((4, 1));
    }

    [Fact]
    public async Task A_loja_recorta_o_territorio_e_a_carteira_e_mostra_o_peso_dela_na_ADR()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?anoBase=2024&lojaCodigo={ApiEmMemoria.FilialDeRibeirao}"));

        dados.GetProperty("municipiosNoRecorte").GetInt32().Should().Be(1);
        dados.GetProperty("carteira").GetProperty("clientes").GetProperty("clientes").GetInt32().Should().Be(3, "só os de D1");
        dados.GetProperty("carteira").GetProperty("fora").ValueKind.Should().Be(JsonValueKind.Null, "com loja não há fora da região");

        var peso = dados.GetProperty("representatividade").GetProperty("valor");
        peso.GetProperty("percentual").GetDecimal().Should().Be(40.3m, "1.350 de 3.350");
        dados.GetProperty("lacunas").EnumerateArray().Select(l => l.GetProperty("metrica").GetString()).Should().Contain("foraDaRegiao");
    }

    [Fact]
    public async Task A_usina_recorta_os_municipios_com_usina()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?anoBase=2024&usina=com"));

        dados.GetProperty("matriz").EnumerateArray().Select(m => m.GetProperty("codigoIbge").GetInt32()).Should().Equal(D1);
        dados.GetProperty("matriz")[0].GetProperty("usinas").GetInt32().Should().Be(1);
    }

    [Theory]
    [InlineData("anoBase=1990", "anoBase")]
    [InlineData("cultura=NAO_EXISTE", "cultura")]
    [InlineData("lojaCodigo=999999", "lojaCodigo")]
    [InlineData("classe=E", "classe")]
    [InlineData("usina=talvez", "usina")]
    public async Task Parametro_que_nao_vale_e_recusado_no_campo(string consulta, string campo)
    {
        var http = await SemearAsync();

        var resposta = await http.GetAsync($"{Rota}?{consulta}");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain(campo);
    }
}
