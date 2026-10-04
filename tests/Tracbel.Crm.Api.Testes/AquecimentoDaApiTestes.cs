using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Persistencia.Diagnostico;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O AQUECIMENTO NA SUBIDA (plano 2 do documento 54): depois de cada publicação, a primeira tela pagava as três contas de
/// referência e a compilação de cada consulta do EF — 10,6 s nos Indicadores, na captura de produção de 03/10/2026. O
/// aquecimento faz isso antes do primeiro usuário, e falhar nele não derruba a API.
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class AquecimentoDaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private sealed class TerritorioQueFalha : IRepositorioDoTerritorioDeReferencia
    {
        public Task<TerritorioDeReferencia> LerAsync(CancellationToken ct) => throw new InvalidOperationException("banco fora do ar");
    }

    private AquecimentoDaApi ComOTerritorioQueFalha(IConfiguration configuracao) => new(
        api.Services.GetRequiredService<IServiceScopeFactory>(),
        new TerritorioQueFalha(),
        api.Services.GetRequiredService<IRepositorioDoPotencialDeReferencia>(),
        api.Services.GetRequiredService<IRepositorioDaEstruturaDeReferencia>(),
        configuracao,
        api.Services.GetRequiredService<IRelogio>(),
        NullLogger<AquecimentoDaApi>.Instance);

    [Fact]
    public async Task Depois_do_aquecimento_as_tres_referencias_ja_estao_guardadas()
    {
        await CenarioDosCenarios.SemearAsync(api);
        var aquecimento = ActivatorUtilities.CreateInstance<AquecimentoDaApi>(api.Services);

        await aquecimento.AquecerAsync(default);

        var hoje = ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow);
        using var medicao = ContadorDeConsultas.Iniciar();
        await api.Services.GetRequiredService<IRepositorioDoTerritorioDeReferencia>().LerAsync(default);
        await api.Services.GetRequiredService<IRepositorioDoPotencialDeReferencia>().LerAsync(hoje, default);
        await api.Services.GetRequiredService<IRepositorioDaEstruturaDeReferencia>().LerAsync(default);
        medicao.Consultas.Should().Be(3, "só as três assinaturas: as contas já estavam guardadas");
    }

    [Fact]
    public async Task O_aquecimento_roda_as_telas_pesadas_da_captura()
    {
        await CenarioDosCenarios.SemearAsync(api);
        var aquecimento = ActivatorUtilities.CreateInstance<AquecimentoDaApi>(api.Services);

        var telas = await aquecimento.AquecerAsync(default);

        telas.Select(t => t.Tela).Should().Equal(
            "Indicadores", "Visão 360", "Funil por estágio", "Painel do CEN", "Faturamento", "Metas", "Vendas perdidas");
        telas.Should().OnlyContain(t => t.Falha == null, "cada tela roda o caso de uso dela, no padrão da tela, sem recusa");
    }

    [Fact]
    public async Task Uma_tela_que_falha_nao_impede_as_outras()
    {
        var aquecimento = ActivatorUtilities.CreateInstance<AquecimentoDaApi>(api.Services);
        var sistema = Tracbel.Crm.Infraestrutura.Identidade.ProvedorDeContextoDeSistema.Instancia;

        var telas = await aquecimento.AquecerTelasAsync(
        [
            new TelaParaAquecer("Primeira", sistema, (_, _) => Task.FromResult<string?>(null)),
            new TelaParaAquecer("Quebra", sistema, (_, _) => throw new InvalidOperationException("banco fora do ar")),
            new TelaParaAquecer("Recusa", sistema, (_, _) => Task.FromResult<string?>("sem permissão")),
            new TelaParaAquecer("Última", sistema, (_, _) => Task.FromResult<string?>(null))
        ], default);

        telas.Select(t => (t.Tela, t.Falha)).Should().Equal(
            ("Primeira", (string?)null), ("Quebra", "banco fora do ar"), ("Recusa", "sem permissão"), ("Última", (string?)null));
    }

    [Fact]
    public async Task O_aquecimento_que_falha_nao_derruba_a_api()
    {
        var aquecimento = ComOTerritorioQueFalha(new ConfigurationBuilder().Build());

        await aquecimento.StartAsync(default);
        var execucao = () => aquecimento.ExecuteTask!;

        await execucao.Should().NotThrowAsync("a falha vai para o log, e a primeira tela só paga a conta inteira");
    }

    [Fact]
    public async Task Com_o_aquecimento_desligado_nada_e_lido()
    {
        var aquecimento = ComOTerritorioQueFalha(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Aquecimento:Ligado"] = "false" })
            .Build());

        using var medicao = ContadorDeConsultas.Iniciar();
        await aquecimento.StartAsync(default);
        await aquecimento.ExecuteTask!;

        medicao.Consultas.Should().Be(0);
    }
}
