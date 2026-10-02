using System.Net;
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
/// A GESTÃO DE FINANCIAMENTOS PELA API (issue 261).
///
/// <para><b>O cenário, com as contas à mão</b> (último mês do SICOR: 08/2026; o mais antigo: 01/2024):</para>
/// <list type="bullet">
/// <item>F1 (ADR, Norte, loja de Ribeirão): trator em 08/2026 em duas linhas de R$ 100 mil, 03/2026 R$ 50 mil, colheitadeira
/// em 05/2026 R$ 400 mil; no ano anterior, 08/2025 R$ 100 mil e 03/2025 R$ 50 mil; e 01/2024 R$ 10 mil.</item>
/// <item>F2 (ADR, Noroeste, loja de Barretos, uma usina): trator pelo programa 155 em 07/2026 R$ 300 mil e 07/2025 R$ 200 mil.</item>
/// <item>F3 (fora da ADR): trator em 08/2026, R$ 900 mil.</item>
/// </list>
/// <para>O período padrão é 09/2025 a 08/2026 contra 09/2024 a 08/2025. A Região: 5 linhas e R$ 950 mil contra 3 linhas e
/// R$ 350 mil — índice 0,7 × 5/3 + 0,3 × 950/350 = 1,981, superaquecido. São Paulo no período: 6 linhas e R$ 1,85 mi.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class FinanciamentosDoSicorNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/mercado/financiamentos";
    private const int Trator = 7080, Colheitadeira = 2700;
    private const int F1 = 3598201, F2 = 3598202, F3 = 3598203;

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").Clone();
    }

    private static CreditoRuralDeInvestimento Linha(int municipioId, int bcb, short ano, byte mes, decimal valor, int produto = Trator, int programa = 154, int modalidade = 14) =>
        CreditoRuralDeInvestimento.Registrar(
            new CreditoRuralDeInvestimento.Chave(bcb, ano, mes, produto, programa, 71, 431, 9, 1, modalidade),
            municipioId, valor, 0m, 100, DateTime.UtcNow);

    private async Task<HttpClient> SemearAsync()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var http = api.ClienteDeRibeirao();

        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        if (await db.Municipios.AnyAsync(m => m.CodigoIbge == F1)) return http;

        var agora = DateTime.UtcNow;
        var f1 = Municipio.Criar("Município Fin 1", "SP", F1);
        var f2 = Municipio.Criar("Município Fin 2", "SP", F2);
        var f3 = Municipio.Criar("Município Fin 3", "SP", F3);
        db.Municipios.AddRange(f1, f2, f3);
        await db.SaveChangesAsync();

        db.MunicipiosDaAreaDeAtuacao.AddRange(
            MunicipioDaAreaDeAtuacao.Registrar(f1.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 2, 100, agora),
            MunicipioDaAreaDeAtuacao.Registrar(f2.Id, true, RegiaoDaAreaDeAtuacao.Noroeste, 2, "Area de Atuação.xlsx", 3, 100, agora));
        db.UsinasDeEtanol.Add(UsinaDeEtanol.Registrar("12345678000271", "Usina Fin", f2.Id, new DateOnly(2026, 9, 1), 100, 100, 100, agora));

        db.CreditosRuraisDeInvestimento.AddRange(
            Linha(f1.Id, 7001, 2026, 8, 100_000m),
            Linha(f1.Id, 7001, 2026, 8, 100_000m, modalidade: 15),
            Linha(f1.Id, 7001, 2026, 3, 50_000m),
            Linha(f1.Id, 7001, 2026, 5, 400_000m, produto: Colheitadeira),
            Linha(f1.Id, 7001, 2025, 8, 100_000m),
            Linha(f1.Id, 7001, 2025, 3, 50_000m),
            Linha(f1.Id, 7001, 2024, 1, 10_000m),
            Linha(f2.Id, 7002, 2026, 7, 300_000m, programa: 155),
            Linha(f2.Id, 7002, 2025, 7, 200_000m, programa: 155),
            Linha(f3.Id, 7003, 2026, 8, 900_000m));

        db.ItensDoSicor.AddRange(
            ItemDoSicor.Registrar("PRODUTO", Trator, 0, "TRATORES", null, null, 100, agora),
            ItemDoSicor.Registrar("PROGRAMA", 155, 0, "PRONAF", null, null, 100, agora));
        await db.SaveChangesAsync();
        return http;
    }

    [Fact]
    public async Task O_padrao_e_a_regiao_nos_12_meses_contra_o_mesmo_periodo_do_ano_anterior()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        (dados.GetProperty("de").GetString(), dados.GetProperty("ate").GetString()).Should().Be(("2025-09-01", "2026-08-01"));
        (dados.GetProperty("anteriorDe").GetString(), dados.GetProperty("anteriorAte").GetString()).Should().Be(("2024-09-01", "2025-08-01"));
        dados.GetProperty("carenciaDecidida").GetBoolean().Should().BeFalse();

        var totais = dados.GetProperty("totais");
        (totais.GetProperty("linhas").GetInt32(), totais.GetProperty("valor").GetDecimal()).Should().Be((5, 950_000m));
        (totais.GetProperty("linhasAnteriores").GetInt32(), totais.GetProperty("valorAnterior").GetDecimal()).Should().Be((3, 350_000m));
        totais.GetProperty("indice").GetProperty("indice").GetDecimal().Should().BeApproximately(1.981m, 0.001m);
        totais.GetProperty("indice").GetProperty("faixa").GetString().Should().Be("Superaquecido");

        var noEstado = dados.GetProperty("noEstado");
        (noEstado.GetProperty("linhasDoEstado").GetInt32(), noEstado.GetProperty("valorDoEstado").GetDecimal()).Should().Be((6, 1_850_000m));
        noEstado.GetProperty("fatiaDoValor").GetDecimal().Should().Be(51.35m);
    }

    [Fact]
    public async Task O_momento_traz_R3_R6_e_R12_terminando_no_fim_do_periodo()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        var momento = dados.GetProperty("momento").EnumerateArray().ToDictionary(m => m.GetProperty("meses").GetInt32());
        var r3 = momento[3].GetProperty("janelas");
        (r3.GetProperty("linhas").GetInt32(), r3.GetProperty("valor").GetDecimal()).Should().Be((3, 500_000m), "06 a 08/2026");
        (r3.GetProperty("linhasAnteriores").GetInt32(), r3.GetProperty("valorAnterior").GetDecimal()).Should().Be((2, 300_000m));
        momento[3].GetProperty("indice").GetProperty("indice").GetDecimal().Should().BeApproximately(1.55m, 0.001m, "0,7 × 1,5 + 0,3 × 1,667");
        momento[12].GetProperty("indice").GetProperty("indice").GetDecimal().Should().BeApproximately(1.981m, 0.001m, "o R12 é o próprio período");

        // A SÉRIE VAI DO PRIMEIRO MÊS DO SICOR, para a granularidade: 01/2024 está lá.
        var serie = dados.GetProperty("serie").EnumerateArray().ToList();
        serie[0].GetProperty("mes").GetString().Should().Be("2024-01-01");
        serie.Sum(m => m.GetProperty("linhas").GetInt32()).Should().Be(9, "as 9 linhas da Região, F3 fora");
    }

    [Fact]
    public async Task Cada_municipio_tem_o_indice_a_faixa_e_a_fatia_no_estado()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync(Rota));

        var municipios = dados.GetProperty("municipios").EnumerateArray().ToDictionary(m => m.GetProperty("codigoIbge").GetInt32());
        municipios.Keys.Should().BeEquivalentTo([F1, F2], "F3 não é da Região");
        municipios[F1].GetProperty("indice").GetProperty("faixa").GetString().Should().Be("Superaquecido", "0,7 × 2 + 0,3 × 4,33 = 2,7");
        municipios[F2].GetProperty("indice").GetProperty("faixa").GetString().Should().Be("Intermediaria", "0,7 × 1 + 0,3 × 1,5 = 1,15");
        municipios[F1].GetProperty("fatiaNoEstado").GetDecimal().Should().BeApproximately(35.135m, 0.001m, "650 mil de 1,85 mi");

        var situacoes = dados.GetProperty("situacoes");
        (situacoes.GetProperty("superaquecidos").GetInt32(), situacoes.GetProperty("intermediarios").GetInt32()).Should().Be((1, 1));

        var lojas = dados.GetProperty("porLoja").EnumerateArray().Select(l => l.GetProperty("lojaCodigo").GetString()).ToList();
        lojas.Should().BeEquivalentTo([ApiEmMemoria.FilialDeRibeirao, ApiEmMemoria.FilialDeBarretos]);
    }

    [Fact]
    public async Task O_produto_o_programa_e_o_recorte_vao_ao_banco()
    {
        var http = await SemearAsync();

        var colheitadeira = await DadosAsync(await http.GetAsync($"{Rota}?produto={Colheitadeira}"));
        colheitadeira.GetProperty("totais").GetProperty("valor").GetDecimal().Should().Be(400_000m);
        colheitadeira.GetProperty("situacoes").GetProperty("semBase").GetInt32().Should().Be(1, "sem colheitadeira no ano anterior");

        var pronaf = await DadosAsync(await http.GetAsync($"{Rota}?programa=155"));
        pronaf.GetProperty("totais").GetProperty("valor").GetDecimal().Should().Be(300_000m);
        pronaf.GetProperty("programas").EnumerateArray().Should().Contain(p => p.GetProperty("nome").GetString() == "PRONAF");

        var estado = await DadosAsync(await http.GetAsync($"{Rota}?recorte=sp"));
        estado.GetProperty("totais").GetProperty("valor").GetDecimal().Should().Be(1_850_000m);
        estado.GetProperty("noEstado").ValueKind.Should().Be(JsonValueKind.Null, "o recorte já é o estado");

        var fora = await DadosAsync(await http.GetAsync($"{Rota}?recorte=fora"));
        fora.GetProperty("municipios").EnumerateArray().Single().GetProperty("codigoIbge").GetInt32().Should().Be(F3);

        var comUsina = await DadosAsync(await http.GetAsync($"{Rota}?usina=com"));
        comUsina.GetProperty("municipios").EnumerateArray().Single().GetProperty("codigoIbge").GetInt32().Should().Be(F2);

        var loja = await DadosAsync(await http.GetAsync($"{Rota}?lojaCodigo={ApiEmMemoria.FilialDeRibeirao}"));
        loja.GetProperty("municipios").EnumerateArray().Single().GetProperty("codigoIbge").GetInt32().Should().Be(F1);
    }

    [Fact]
    public async Task O_periodo_escolhido_compara_com_o_mesmo_periodo_do_ano_anterior()
    {
        var http = await SemearAsync();
        var dados = await DadosAsync(await http.GetAsync($"{Rota}?de=2026-01&ate=2026-08"));

        dados.GetProperty("meses").GetInt32().Should().Be(8);
        (dados.GetProperty("anteriorDe").GetString(), dados.GetProperty("anteriorAte").GetString()).Should().Be(("2025-01-01", "2025-08-01"));
        dados.GetProperty("totais").GetProperty("linhasAnteriores").GetInt32().Should().Be(3);
        dados.GetProperty("lacunas").EnumerateArray().Select(l => l.GetProperty("metrica").GetString()).Should().Contain("periodoCurto");
    }

    [Theory]
    [InlineData("produto=9999", "produto")]
    [InlineData("programa=1", "programa")]
    [InlineData("recorte=marte", "recorte")]
    [InlineData("de=2026-13", "de")]
    [InlineData("ate=2030-01", "ate")]
    [InlineData("lojaCodigo=999999", "lojaCodigo")]
    [InlineData("usina=talvez", "usina")]
    public async Task Parametro_que_nao_vale_e_recusado_no_campo(string consulta, string campo)
    {
        var http = await SemearAsync();

        var resposta = await http.GetAsync($"{Rota}?{consulta}");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain(campo);
    }
}
