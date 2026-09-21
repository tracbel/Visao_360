using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A PERMISSÃO NAS ROTAS (fase 3 do documento 41; issue 46) — o aceite, por HTTP, com a aplicação inteira.
///
/// <list type="bullet">
///   <item>toda rota declara a permissão que exige, ou escreve por que é aberta;</item>
///   <item>sem a permissão, <b>toda</b> rota da API devolve 403 — nenhuma escapa;</item>
///   <item>o perfil padrão não exclui cliente (Q-P2); com o perfil de exclusão concedido, exclui;</item>
///   <item>filial fora da de casa e das concedidas é 403 (P-20); concedida, entra;</item>
///   <item>a rota de escopo responde pela filial de casa quando a pedida é recusada.</item>
/// </list>
/// </summary>
[Trait("Categoria", "Autorizacao")]
public sealed class PermissoesNasRotasTestes(ApiEmMemoria api, ITestOutputHelper saida) : IClassFixture<ApiEmMemoria>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();

    private static CrmDbContext BancoDeSistema(ApiEmMemoria app)
    {
        var escopo = app.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        return new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
    }

    private static IEnumerable<RouteEndpoint> Rotas(ApiEmMemoria app) =>
        app.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>();

    private static string Descrever(RouteEndpoint rota) =>
        $"{string.Join(",", rota.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? ["*"])} {rota.RoutePattern.RawText}";

    // =============================================================================================
    // O portão: toda rota declara
    // =============================================================================================

    [Fact]
    public void Toda_rota_declara_a_permissao_que_exige_ou_escreve_por_que_e_aberta()
    {
        var semDeclaracao = Rotas(api)
            .Where(r => r.Metadata.GetMetadata<PermissaoExigida>() is null && r.Metadata.GetMetadata<RotaSemPermissao>() is null)
            .Select(Descrever)
            .ToList();

        semDeclaracao.Should().BeEmpty(
            "toda rota precisa de .ExigePermissao(Permissoes.X) ou de .SemPermissaoExigida(\"motivo\") — " +
            "rota esquecida é rota aberta, e descobrir isso em produção é tarde (fase 3 do documento 41)");
    }

    [Fact]
    public void Toda_rota_de_negocio_da_api_exige_permissao_e_so_o_escopo_e_aberto()
    {
        var abertas = Rotas(api)
            .Where(r => (r.RoutePattern.RawText ?? string.Empty).StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            .Where(r => r.Metadata.GetMetadata<PermissaoExigida>() is null)
            .Select(Descrever)
            .ToList();

        abertas.Should().BeEquivalentTo(["GET /api/v1/acesso/escopo"],
            "sob /api, a única rota sem permissão é a do escopo do próprio usuário");

        foreach (var exigida in Rotas(api).SelectMany(r => r.Metadata.GetOrderedMetadata<PermissaoExigida>()))
            Permissoes.Existe(exigida.Codigo).Should().BeTrue($"'{exigida.Codigo}' precisa existir no catálogo");
    }

    // =============================================================================================
    // Sem permissão, 403 em toda rota
    // =============================================================================================

    [Fact]
    public async Task Sem_permissao_nenhuma_toda_rota_da_api_devolve_403()
    {
        // UMA API PRÓPRIA, porque este teste esvazia o perfil padrão — e as outras deste arquivo precisam dele.
        await using var vazia = new ApiEmMemoria();
        await vazia.InitializeAsync();

        await using (var db = BancoDeSistema(vazia))
        {
            var padrao = await db.Perfis.SingleAsync(p => p.EhPadrao);
            await db.PerfisPermissoes.Where(p => p.PerfilId == padrao.Id).ExecuteDeleteAsync();
        }

        var http = vazia.ClienteDeRibeirao();
        var falhas = new List<string>();
        var conferidas = 0;

        foreach (var rota in Rotas(vazia).Where(r => r.Metadata.GetMetadata<PermissaoExigida>() is not null))
        {
            var metodo = rota.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.First();
            // O VALOR PRECISA CASAR COM A RESTRIÇÃO DA ROTA, senão o roteador devolve 404 antes de o portão de
            // permissão ser alcançado — e o teste mediria o roteador, não a permissão.
            var caminho = Regex.Replace(rota.RoutePattern.RawText!, @"\{(?<nome>[^}:]+)(?<tipo>:[^}]+)?\}", m =>
                m.Groups["tipo"].Value.Contains("guid") ? Guid.NewGuid().ToString()
                : m.Groups["tipo"].Value.Contains("long") || m.Groups["tipo"].Value.Contains("int") ? "1" : "X");

            using var pedido = new HttpRequestMessage(new HttpMethod(metodo), caminho);
            if (metodo is "POST" or "PUT" or "DELETE")
                pedido.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var resposta = await http.SendAsync(pedido);
            conferidas++;
            saida.WriteLine($"{metodo,-6} {caminho} → {(int)resposta.StatusCode}");

            if (resposta.StatusCode != HttpStatusCode.Forbidden) falhas.Add($"{metodo} {caminho} → {(int)resposta.StatusCode}");
        }

        conferidas.Should().BeGreaterThanOrEqualTo(38, "são 38 rotas de negócio, e nenhuma pode ficar de fora");
        falhas.Should().BeEmpty("sem permissão, nenhuma rota pode responder outra coisa que não 403");
    }

    [Fact]
    public async Task O_403_diz_qual_permissao_falta()
    {
        var chave = await CriarClienteEmRibeiraoAsync("Fazenda do 403 Explicado");

        var resposta = await api.ClienteDeRibeirao().SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/clientes/{chave}")
        {
            Content = JsonContent.Create(new { motivoCodigo = "DUPLICADO", versao = (string?)null }, options: Json)
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, "o perfil padrão é o mínimo, sem excluir (Q-P2)");
        var corpo = await CorpoAsync(resposta);
        corpo.GetProperty("type").GetString().Should().EndWith("sem-acesso");
        corpo.GetProperty("detail").GetString().Should().Contain(Permissoes.ClienteExcluir);
    }

    [Fact]
    public async Task Com_o_perfil_de_exclusao_concedido_a_exclusao_passa_da_permissao()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        await using (var db = BancoDeSistema(app))
        {
            var exclusao = await db.Perfis.SingleAsync(p => p.Codigo == PerfisDeSistema.ExclusaoDeCadastro);
            db.UsuariosPerfis.Add(UsuarioPerfil.Conceder(100, exclusao.Id, "teste — gerente da filial", 100, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        var criado = await app.ClienteDeRibeirao().PostAsJsonAsync("/api/v1/clientes", new { nomeRazao = "Fazenda Que Sai", tipoDePessoa = "Juridica" }, Json);
        var chave = (await CorpoAsync(criado)).GetProperty("chave").GetGuid();

        var resposta = await app.ClienteDeRibeirao().SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/clientes/{chave}")
        {
            Content = JsonContent.Create(new { motivoCodigo = "DUPLICADO", versao = (string?)null }, options: Json)
        });

        resposta.StatusCode.Should().NotBe(HttpStatusCode.Forbidden, "o perfil concedido soma ao padrão");
    }

    // =============================================================================================
    // P-20: a filial
    // =============================================================================================

    [Fact]
    public async Task Filial_fora_da_de_casa_e_das_concedidas_e_403_com_o_campo_da_filial()
    {
        var resposta = await api.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ApiEmMemoria.FilialDeBarretos).GetAsync("/api/v1/clientes");

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, "Barretos não é a filial de casa dele nem tem perfil concedido nela (P-20)");
        var corpo = await CorpoAsync(resposta);
        corpo.GetProperty("erros")[0].GetProperty("campo").GetString().Should().Be("X-Tracbel-Empresa");
    }

    [Fact]
    public async Task Com_um_perfil_concedido_na_filial_ela_passa_a_poder_ser_escolhida()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        await using (var db = BancoDeSistema(app))
        {
            var padrao = await db.Perfis.SingleAsync(p => p.EhPadrao);
            db.UsuariosPerfis.Add(UsuarioPerfil.Conceder(100, padrao.Id, "teste — cobre Barretos", 100, DateTime.UtcNow, empresaId: 2));
            await db.SaveChangesAsync();
        }

        var resposta = await app.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ApiEmMemoria.FilialDeBarretos).GetAsync("/api/v1/clientes");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // =============================================================================================
    // O escopo efetivo
    // =============================================================================================

    [Fact]
    public async Task O_escopo_responde_pela_filial_de_casa_quando_a_pedida_e_recusada()
    {
        var resposta = await api.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ApiEmMemoria.FilialDeBarretos).GetAsync("/api/v1/acesso/escopo");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK, "é a rota que a tela usa para sair da filial recusada");
        var dados = (await CorpoAsync(resposta)).GetProperty("dados");

        dados.GetProperty("filialPedidaRecusada").GetString().Should().Be(ApiEmMemoria.FilialDeBarretos);
        dados.GetProperty("filialAtual").GetProperty("codigo").GetString().Should().Be(ApiEmMemoria.FilialDeRibeirao);
        dados.GetProperty("filiaisPermitidas").EnumerateArray().Select(f => f.GetProperty("codigo").GetString())
            .Should().BeEquivalentTo([ApiEmMemoria.FilialDeRibeirao]);
    }

    [Fact]
    public async Task O_escopo_mostra_o_que_o_perfil_padrao_da_e_o_que_nao_da()
    {
        var dados = (await CorpoAsync(await api.ClienteDeRibeirao().GetAsync("/api/v1/acesso/escopo"))).GetProperty("dados");
        var codigos = dados.GetProperty("permissoes").EnumerateArray().Select(p => p.GetProperty("codigo").GetString()).ToList();

        codigos.Should().Contain([Permissoes.ClienteEditar, Permissoes.TerritorioLer]);
        codigos.Should().NotContain([Permissoes.ClienteExcluir, Permissoes.EmpresaAlcanceEntreFiliais, "Lead.Ler"]);
        dados.GetProperty("filialPedidaRecusada").ValueKind.Should().Be(JsonValueKind.Null);
    }

    private async Task<Guid> CriarClienteEmRibeiraoAsync(string nome)
    {
        var resposta = await api.ClienteDeRibeirao().PostAsJsonAsync("/api/v1/clientes", new { nomeRazao = nome, tipoDePessoa = "Juridica" }, Json);
        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await CorpoAsync(resposta)).GetProperty("chave").GetGuid();
    }
}
