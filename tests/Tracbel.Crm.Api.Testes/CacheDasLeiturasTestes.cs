using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O CACHE DAS LEITURAS (30/09/2026) — a mesma resposta não é recalculada enquanto o dado não muda, e deixa de valer na
/// hora em que muda.
///
/// <para><b>Como o teste enxerga o cache:</b> grava direto no banco entre duas leituras, como as rotinas fazem em outro
/// processo. Se a segunda leitura não mostra o que foi gravado, ela veio do cache; quando a versão dos dados muda, ela
/// mostra.</para>
///
/// <para>Cada teste tem a SUA API, com o cache ligado — o <see cref="ApiEmMemoria"/> dos outros testes nasce com ele
/// desligado.</para>
/// </summary>
[Trait("Categoria", "Api")]
public sealed class CacheDasLeiturasTestes : IAsyncLifetime
{
    private const string Catalogo = "/api/v1/catalogos/ORIGEM_LEAD";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ApiEmMemoria _api = new() { MinutosDoCache = 10 };

    public Task InitializeAsync() => _api.InitializeAsync();

    public async Task DisposeAsync()
    {
        await ((IAsyncLifetime)_api).DisposeAsync();
        await _api.DisposeAsync();
    }

    private async Task<string> LerAsync(HttpClient http, string caminho = Catalogo)
    {
        var resposta = await http.GetAsync(caminho);
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        return await resposta.Content.ReadAsStringAsync();
    }

    /// <summary>Um item novo no catálogo, gravado direto no banco — como a rotina grava, sem passar pela API.</summary>
    private async Task GravarNoBancoAsync(string codigo)
    {
        using var escopo = _api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        db.CatalogoItens.Add(CatalogoItem.Criar(CatalogosDeSistema.OrigemDeLead, codigo, $"Item {codigo}", 90));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task A_segunda_leitura_vem_do_cache_ate_uma_rotina_gravar()
    {
        var http = _api.ClienteDeRibeirao();
        (await LerAsync(http)).Should().Contain("INDICACAO").And.NotContain("FEIRA");

        await GravarNoBancoAsync("FEIRA");
        (await LerAsync(http)).Should().NotContain("FEIRA", "a resposta guardada vale enquanto nenhuma rotina gravou");

        // O VIGIA PERCEBE QUE UMA ROTINA TERMINOU e muda a versão — aqui, direto, sem esperar os 15 segundos dele.
        _api.Services.GetRequiredService<VersaoDosDados>().AtualizarDoBanco("uma rotina terminou");
        (await LerAsync(http)).Should().Contain("FEIRA", "com a versão nova, a leitura vai ao banco");
    }

    [Fact]
    public async Task Uma_gravacao_pela_api_limpa_o_cache_na_hora()
    {
        var http = _api.ClienteDeRibeirao();
        await LerAsync(http);
        await GravarNoBancoAsync("FEIRA");

        var criacao = await http.PostAsJsonAsync("/api/v1/clientes", new
        {
            nomeRazao = "Fazenda do Cache Ltda",
            tipoDePessoa = "Juridica",
            situacao = "Prospect",
            origemCodigo = "INDICACAO"
        }, Json);
        criacao.StatusCode.Should().Be(HttpStatusCode.Created);

        (await LerAsync(http)).Should().Contain("FEIRA", "quem grava pela tela vê o resultado na leitura seguinte");
    }

    [Fact]
    public async Task A_gravacao_recusada_nao_limpa_o_cache()
    {
        var http = _api.ClienteDeRibeirao();
        await LerAsync(http);
        await GravarNoBancoAsync("FEIRA");

        var recusada = await http.PostAsJsonAsync("/api/v1/clientes", new { nomeRazao = "   ", tipoDePessoa = "Juridica" }, Json);
        ((int)recusada.StatusCode).Should().BeGreaterThanOrEqualTo(400);

        (await LerAsync(http)).Should().NotContain("FEIRA", "nada foi gravado, e o que estava guardado continua valendo");
    }

    [Fact]
    public async Task A_resposta_guardada_de_um_usuario_nunca_e_servida_a_outro()
    {
        var ribeirao = _api.ClienteDeRibeirao();
        await LerAsync(ribeirao);
        await GravarNoBancoAsync("FEIRA");

        (await LerAsync(_api.ClienteDeBarretos())).Should().Contain("FEIRA", "outro usuário e outra filial têm a sua chave");
        (await LerAsync(ribeirao)).Should().NotContain("FEIRA", "e a de Ribeirão continua a dela");
    }

    [Fact]
    public async Task Outra_consulta_na_mesma_rota_e_outra_chave()
    {
        var http = _api.ClienteDeRibeirao();
        await LerAsync(http);
        await GravarNoBancoAsync("FEIRA");

        // A CONSULTA INTEIRA ENTRA NA CHAVE, e não só o caminho: o filtro trocado na tela nunca recebe a resposta do
        // filtro anterior.
        (await LerAsync(http, $"{Catalogo}?qualquer=1")).Should().Contain("FEIRA");
    }

    [Fact]
    public async Task O_vigia_so_muda_a_marca_quando_uma_rotina_termina_ou_o_art_traz_venda()
    {
        using var escopo = _api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        var agora = DateTime.UtcNow;

        var sistema = Sistema.Criar("ART", "ART — vendas de máquina", "teste");
        db.Sistemas.Add(sistema);
        await db.SaveChangesAsync();
        var antes = await VigiaDaVersaoDosDados.LerMarcaAsync(db, default);

        // A RODADA DO ART QUE NÃO MUDOU NADA NÃO MEXE NA MARCA.
        var semMudanca = ExecucaoDeSincronizacao.Iniciar(sistema.Id, "ART.VENDA_DE_MAQUINA", "SERVIDOR", agora.AddMinutes(-3));
        semMudanca.Concluir(1, 500, 0, 0, 0, "nada novo", agora.AddMinutes(-2));
        db.ExecucoesDeSincronizacao.Add(semMudanca);
        await db.SaveChangesAsync();
        (await VigiaDaVersaoDosDados.LerMarcaAsync(db, default)).Should().Be(antes);

        var comVenda = ExecucaoDeSincronizacao.Iniciar(sistema.Id, "ART.VENDA_DE_MAQUINA", "SERVIDOR", agora.AddMinutes(-1));
        comVenda.Concluir(1, 500, 2, 1, 0, "2 incluídas, 1 atualizada", agora);
        db.ExecucoesDeSincronizacao.Add(comVenda);
        await db.SaveChangesAsync();
        var depoisDoArt = await VigiaDaVersaoDosDados.LerMarcaAsync(db, default);
        depoisDoArt.Should().NotBe(antes);

        var rotina = await db.Rotinas.FirstAsync();
        rotina.IniciarExecucao(agora);
        rotina.EncerrarExecucao(ResultadoDaExecucao.Sucesso, null, agora.AddSeconds(30));
        await db.SaveChangesAsync();
        (await VigiaDaVersaoDosDados.LerMarcaAsync(db, default)).Should().NotBe(depoisDoArt);
    }

    [Theory]
    [InlineData("GET", "/api/v1/relatorios/indicadores-executivos", true)]
    [InlineData("GET", "/api/v1/territorio/indicadores", true)]
    [InlineData("GET", "/api/v1/clientes", true)]
    [InlineData("GET", "/api/v1/integracoes/conferencia-gn", true)]
    [InlineData("POST", "/api/v1/clientes", false)]
    [InlineData("GET", "/api/v1/integracoes/desempenho", false)]
    [InlineData("GET", "/api/v1/integracoes/sincronizacoes", false)]
    [InlineData("GET", "/api/v1/acesso/escopo", false)]
    [InlineData("GET", "/api/v1/admin/perfis", false)]
    [InlineData("GET", "/api/v1/relatoriosx", false)]
    public void So_as_leituras_de_relatorio_e_de_cadastro_entram_no_cache(string metodo, string caminho, bool entra)
    {
        var http = new DefaultHttpContext();
        http.Request.Method = metodo;
        http.Request.Path = caminho;

        PoliticaDeCacheDasLeituras.EhCacheavel(http.Request).Should().Be(entra);
    }
}
