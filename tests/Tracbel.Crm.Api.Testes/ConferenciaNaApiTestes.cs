using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS POR HTTP (28/09/2026). Ribeirão (1) e Barretos (2): a meta e o realizado de agosto
/// lá e aqui, e três divergências abertas — duas de Ribeirão, uma de Barretos — e uma que deixou de ocorrer.
/// </summary>
[Trait("Categoria", "Conferencia")]
public sealed class ConferenciaNaApiTestes
{
    private const string Rota = "/api/v1/integracoes/conferencia-gn";

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static async Task<ApiEmMemoria> AppAsync(bool apurada = true)
    {
        var app = new ApiEmMemoria();
        await app.InitializeAsync();
        if (!apurada) return app;

        using var escopo = app.Services.CreateScope();
        await using var db = new CrmDbContext(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);
        var gn = Sistema.Criar(ConexoesDoSistema.GestaoDeNegocios, "Gestão de Negócios — API", "teste");
        db.Sistemas.Add(gn);
        await db.SaveChangesAsync();

        var agora = DateTime.UtcNow;
        var agosto = new DateOnly(2026, 8, 1);
        db.ConferenciasDaGestaoDeNegocios.AddRange(
            ConferenciaDaGestaoDeNegocios.Apurar(gn.Id, 1, ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, agosto, 10, 10, agora, 100),
            ConferenciaDaGestaoDeNegocios.Apurar(gn.Id, 1, ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, agosto, 8, 6, agora, 100),
            ConferenciaDaGestaoDeNegocios.Apurar(gn.Id, 2, ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, agosto, 3, 3, agora, 100));

        var encerrada = DivergenciaDeIntegracao.Registrar(1, gn.Id, TipoDeDivergencia.RealizadoEmOutroMes, "CHASSI00000000004", null, null, "outro mês", "a", "b", null, agora, 100);
        encerrada.MarcarQueDeixouDeOcorrer(agora, 100);
        db.DivergenciasDeIntegracao.AddRange(
            DivergenciaDeIntegracao.Registrar(1, gn.Id, TipoDeDivergencia.RealizadoPendenteNoArt, "CHASSI00000000001", null, null, "pendente", "pendente: COMPRADOR_AUSENTE_NO_CRM", "Ribeirão · 2026-08", null, agora, 100),
            DivergenciaDeIntegracao.Registrar(1, gn.Id, TipoDeDivergencia.RealizadoSoNaGestao, "CHASSI00000000002", null, null, "só na GN", null, "Ribeirão · 2026-08", null, agora, 100),
            DivergenciaDeIntegracao.Registrar(2, gn.Id, TipoDeDivergencia.RealizadoSoNaGestao, "CHASSI00000000003", null, null, "só na GN", null, "Barretos · 2026-08", null, agora, 100),
            encerrada);
        db.PontosDeSincronismo.Add(PontoDeSincronismo.Criar(gn.Id, ConferenciaDaGestaoDeNegocios.FluxoDaCarga, "2026-09-28T10:00:00.0000000Z"));
        await db.SaveChangesAsync();
        return app;
    }

    [Fact]
    public async Task A_gerencia_ve_a_conferencia_da_filial_com_as_divergencias_abertas()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota));

        dados.GetProperty("alcance").GetString().Should().Be("Filiais");
        var totais = dados.GetProperty("totais");
        (totais.GetProperty("metaNaGestao").GetInt32(), totais.GetProperty("metaNoCrm").GetInt32(),
                totais.GetProperty("realizadoNaGestao").GetInt32(), totais.GetProperty("realizadoNoCrm").GetInt32())
            .Should().Be((10, 10, 8, 6), "Barretos é de lá");
        var divergencias = dados.GetProperty("divergencias").EnumerateArray().ToList();
        divergencias.Select(d => d.GetProperty("chassi").GetString()).Should().BeEquivalentTo(["CHASSI00000000001", "CHASSI00000000002"],
            "a de Barretos é de lá, e a que deixou de ocorrer não aparece");
        divergencias.Single(d => d.GetProperty("chassi").GetString() == "CHASSI00000000001").GetProperty("rotulo").GetString()
            .Should().Be("Pendente na integração do ART");
        dados.GetProperty("porTipo").EnumerateArray().Single(t => t.GetProperty("tipo").GetString() == "RealizadoSoNaGestao")
            .GetProperty("quantidade").GetInt32().Should().Be(1);
        dados.ToString().Should().NotContain("cliente");
    }

    [Fact]
    public async Task Em_todas_as_filiais_a_diretoria_ve_a_empresa_inteira()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Diretoria);

        var dados = await DadosAsync(await app.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ContextoAcesso.CodigoDeTodasAsFiliais).GetAsync(Rota));

        dados.GetProperty("alcance").GetString().Should().Be("Organizacao");
        dados.GetProperty("totais").GetProperty("realizadoNaGestao").GetInt32().Should().Be(11);
        dados.GetProperty("porFilial").GetArrayLength().Should().Be(2);
        dados.GetProperty("divergencias").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Sem_apuracao_a_tela_diz_que_nao_foi_apurada_e_o_padrao_nao_ve()
    {
        await using var app = await AppAsync(apurada: false);
        (await app.ClienteDeRibeirao().GetAsync(Rota)).StatusCode.Should().Be(HttpStatusCode.Forbidden, "é tela de quem cuida dos números");

        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota));
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString()).Should().Contain("naoApurada");
    }
}
