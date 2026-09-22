using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O CRÉDITO RURAL PELA API DE VERDADE (issue 157): a janela dita por extenso, e a Região ao lado de
/// São Paulo.
///
/// <para>A rota devolvia a Região sozinha, com um filtro "só a ADR" no navegador — e "R$ 900 mil na
/// Região" sem o estado ao lado pode ser metade do mercado paulista ou um vigésimo dele.</para>
///
/// <para>O parâmetro de carência semeado pela migração está <b>em aberto</b> (D-IM-03), e é isso que
/// este teste exercita: a resposta diz que a carência não foi decidida, em vez de descartar meses
/// por um palpite.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class CreditoRuralNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/territorio/credito";
    private const int Trator = 7080;

    private const int Franca = 3516200;
    private const int Cravinhos = 3513108;
    private const int Prudente = 3541406;

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.CreditosRuraisDeInvestimento.AnyAsync()) return;

        var agora = DateTime.UtcNow;
        var franca = Municipio.Criar("Franca", "SP", Franca);
        var cravinhos = Municipio.Criar("Cravinhos", "SP", Cravinhos);
        var prudente = Municipio.Criar("Presidente Prudente", "SP", Prudente);
        db.Municipios.AddRange(franca, cravinhos, prudente);
        await db.SaveChangesAsync();

        // Franca e Cravinhos são a Região; Presidente Prudente é São Paulo e não é Região.
        db.MunicipiosDaAreaDeAtuacao.AddRange(
            MunicipioDaAreaDeAtuacao.Registrar(franca.Id, true, RegiaoDaAreaDeAtuacao.Norte, null, "Area de Atuação.xlsx", 2, 100, agora),
            MunicipioDaAreaDeAtuacao.Registrar(cravinhos.Id, true, RegiaoDaAreaDeAtuacao.Norte, null, "Area de Atuação.xlsx", 3, 100, agora));

        // O último mês com dado é 08/2026. O código do Banco Central entra na chave única do SICOR,
        // e por isso cada município tem o seu.
        db.CreditosRuraisDeInvestimento.AddRange(
            Linha(franca.Id, 6482, 2026, 8, 600_000m),
            Linha(cravinhos.Id, 6483, 2026, 7, 300_000m),
            Linha(franca.Id, 6482, 2025, 8, 500_000m),
            Linha(prudente.Id, 6484, 2026, 8, 900_000m));

        db.ItensDoSicor.Add(ItemDoSicor.Registrar("PRODUTO", Trator, 0, "TRATORES", null, null, 100, agora));
        await db.SaveChangesAsync();
    }

    private static CreditoRuralDeInvestimento Linha(int municipioId, int bcb, short ano, byte mes, decimal valor) =>
        CreditoRuralDeInvestimento.Registrar(
            new CreditoRuralDeInvestimento.Chave(bcb, ano, mes, Trator, 154, 71, 431, 9, 1, 14),
            municipioId, valor, 0m, 100, DateTime.UtcNow);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    [Fact]
    public async Task A_resposta_diz_o_intervalo_exato_de_cada_janela()
    {
        // O CRITÉRIO DE ACEITE: a tela não recalcula a janela, e quem confere sabe que meses entraram.
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var janela = dados.GetProperty("janela");

        janela.GetProperty("ultimoMesComDado").GetString().Should().StartWith("2026-08");
        janela.GetProperty("mesesPorJanela").GetInt32().Should().Be(12, "12 contra 12, do parâmetro semeado");
        janela.GetProperty("inicio").GetString().Should().StartWith("2025-09");
        janela.GetProperty("fim").GetString().Should().StartWith("2026-08");
        janela.GetProperty("inicioAnterior").GetString().Should().StartWith("2024-09");
        janela.GetProperty("fimAnterior").GetString().Should().StartWith("2025-08");

        janela.GetProperty("carenciaDecidida").GetBoolean().Should().BeFalse(
            "D-IM-03 está em aberto, e a tela precisa dizer isso em vez de descartar meses por palpite");
        janela.GetProperty("mesesDeCarencia").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task A_regiao_e_sao_paulo_vem_lado_a_lado()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var regiao = dados.GetProperty("regiao");
        var estado = dados.GetProperty("saoPaulo");

        regiao.GetProperty("recorte").GetString().Should().Be("Região (ADR)");
        regiao.GetProperty("municipios").GetInt32().Should().Be(2);
        regiao.GetProperty("janelas").GetProperty("valor").GetDecimal().Should().Be(900_000m);
        regiao.GetProperty("janelas").GetProperty("valorAnterior").GetDecimal().Should().Be(500_000m);

        estado.GetProperty("recorte").GetString().Should().Be("São Paulo");
        estado.GetProperty("municipios").GetInt32().Should().Be(3, "Presidente Prudente é estado e não é Região");
        estado.GetProperty("janelas").GetProperty("valor").GetDecimal().Should().Be(1_800_000m);

        // É a conta que a tela mostra: a Região é metade do crédito de máquina de São Paulo.
        (regiao.GetProperty("janelas").GetProperty("valor").GetDecimal()
         / estado.GetProperty("janelas").GetProperty("valor").GetDecimal()).Should().Be(0.5m);
    }
}
