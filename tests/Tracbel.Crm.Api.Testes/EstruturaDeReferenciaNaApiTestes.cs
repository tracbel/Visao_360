using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Cache;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A ESTRUTURA DE REFERÊNCIA (plano 2 do documento 54): o parque, as propriedades, o rebanho, a área, as usinas e os totais
/// do estado e da região, calculados uma vez por versão do assunto. O cenário é o dos Cenários de mercado (C1, C2 e C3 na
/// ADR) com a utilização das terras do Censo, para a vocação sair: lavoura de 20%, 50% e 80% da área dos estabelecimentos.
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class EstruturaDeReferenciaNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private async Task SemearAsync()
    {
        await CenarioDosCenarios.SemearAsync(api);

        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        if (await db.UtilizacoesDasTerrasNosMunicipios.AnyAsync()) return;

        var agora = DateTime.UtcNow;
        foreach (var (codigo, lavoura) in new[] { (CenarioDosCenarios.C1, 200m), (CenarioDosCenarios.C2, 500m), (CenarioDosCenarios.C3, 800m) })
        {
            var id = await db.Municipios.Where(m => m.CodigoIbge == codigo).Select(m => m.Id).SingleAsync();
            db.UtilizacoesDasTerrasNosMunicipios.AddRange(
                UtilizacaoDasTerrasNoMunicipio.Registrar(id, 2017, 110087, "Total", 10, 1_000m, 100, agora),
                UtilizacaoDasTerrasNoMunicipio.Registrar(id, 2017, 113470, "Lavouras - permanentes", 5, lavoura / 2, 100, agora),
                UtilizacaoDasTerrasNoMunicipio.Registrar(id, 2017, 113471, "Lavouras - temporárias", 5, lavoura / 2, 100, agora));
            db.FrotasDeTratoresNosMunicipios.Add(FrotaDeTratoresNoMunicipio.Registrar(id, 2017, 113521, "Total", 10, 30, 100, agora));
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task A_vocacao_sai_na_tela_e_a_estrutura_guardada_continua_sem_ela()
    {
        await SemearAsync();
        var http = api.ClienteDeRibeirao();

        var resposta = await http.GetAsync("/api/v1/territorio/indicadores");
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        var c3 = JsonDocument.Parse(corpo).RootElement.GetProperty("dados").GetProperty("indicadores").GetProperty("municipios")
            .EnumerateArray().Single(m => m.GetProperty("codigoIbge").GetInt32() == CenarioDosCenarios.C3);
        c3.GetProperty("estrutura").GetProperty("vocacao").GetProperty("classe").GetString().Should().Be("Alta");

        // A VOCAÇÃO É CALCULADA POR CIMA, a cada apuração: a estrutura guardada é de todas as telas e não pode ganhar o
        // recorte de uma delas.
        var guardada = await api.Services.GetRequiredService<IRepositorioDaEstruturaDeReferencia>().LerAsync(default);
        guardada.PorMunicipio[CenarioDosCenarios.C3].Vocacao.Should().BeNull();
    }

    [Fact]
    public async Task A_frota_reapurada_pela_carga_muda_a_assinatura_da_estrutura()
    {
        await SemearAsync();
        var assinatura = api.Services.GetRequiredService<IAssinaturaDosAssuntos>();
        var antes = await assinatura.LerAsync(AssuntoDeReferencia.Estrutura, default);

        // A CARGA GRAVA EM NOME DA CONTA DELA, com filial de casa: a trilha de auditoria não grava alteração sem filial.
        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, new ContaDaCarga());
            var frota = await db.FrotasDeTratoresNosMunicipios.OrderBy(f => f.Id).FirstAsync();
            frota.Reapurar("Total", 11, 31, 100, DateTime.UtcNow.AddMinutes(1));
            await db.SaveChangesAsync();
        }

        (await assinatura.LerAsync(AssuntoDeReferencia.Estrutura, default)).Should().NotBe(antes, "a carga regravou a frota de um município");
    }

    /// <summary>A conta com que a carga grava: o usuário de Ribeirão, com a filial de casa que a trilha exige.</summary>
    private sealed class ContaDaCarga : IProvedorContextoAcesso
    {
        public Dominio.Seguranca.ContextoAcesso Atual { get; } = new(
            usuarioId: 100,
            nomeExibicao: "carga",
            empresaId: 1,
            empresasVisiveis: new HashSet<int> { 1 },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Dominio.Seguranca.Profundidade>(StringComparer.Ordinal));
    }
}
