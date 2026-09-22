using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A ADMINISTRAÇÃO DE PERFIS pela API de verdade (issue 113, parte 2b). O que se prende: os perfis do sistema são
/// fixos; o perfil próprio nasce, muda e desliga, e quem o tem sente na hora; ninguém cria um perfil maior do que
/// o próprio acesso; tudo entra na trilha.
/// </summary>
public sealed class AdministracaoDePerfisNaApiTestes
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();

    private static async Task<JsonElement> ComStatusAsync(HttpResponseMessage resposta, HttpStatusCode esperado)
    {
        var texto = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(esperado, texto);
        return JsonDocument.Parse(texto).RootElement.Clone();
    }

    private static CrmDbContext Sistema(ApiEmMemoria app) =>
        new(app.Services.CreateScope().ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);

    private static async Task<ApiEmMemoria> ComAdministradorAsync()
    {
        var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        return app;
    }

    private static object Permissao(string codigo, string profundidade = "EmpresaEAbaixo") => new { codigo, profundidade };

    [Fact]
    public async Task So_quem_administra_perfis_ve_a_tela_de_perfis()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Diretoria);

        var resposta = await app.ClienteDeRibeirao().GetAsync("/api/v1/admin/perfis/todos");

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, "a Diretoria vê usuários, mas não administra perfis");
        (await CorpoAsync(resposta)).GetProperty("detail").GetString().Should().Contain(Permissoes.PerfilAdministrar);
    }

    [Fact]
    public async Task Os_perfis_do_sistema_aparecem_fixos_e_nao_se_editam()
    {
        await using var app = await ComAdministradorAsync();
        var admin = app.ClienteDeRibeirao();

        var perfis = (await ComStatusAsync(await admin.GetAsync("/api/v1/admin/perfis/todos"), HttpStatusCode.OK)).GetProperty("dados").EnumerateArray().ToList();
        perfis.Select(p => p.GetProperty("codigo").GetString()).Should().StartWith(PerfisDeSistema.Todos.Select(s => s.Codigo), "os do sistema primeiro, na ordem da semente");
        perfis.Should().OnlyContain(p => p.GetProperty("ehDoSistema").GetBoolean());
        perfis.Single(p => p.GetProperty("codigo").GetString() == PerfisDeSistema.Administrador).GetProperty("pessoasComOPerfil").GetInt32().Should().Be(1);

        var edicao = await admin.PutAsJsonAsync($"/api/v1/admin/perfis/{PerfisDeSistema.Gerencia}",
            new { nome = "Gerência", permissoes = new[] { Permissao(Permissoes.ClienteLer) } }, Json);
        edicao.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await CorpoAsync(edicao)).GetProperty("title").GetString().Should().Contain("Duplique");
    }

    [Fact]
    public async Task O_perfil_proprio_nasce_muda_e_desliga_e_quem_o_tem_sente_na_hora()
    {
        await using var app = await ComAdministradorAsync();
        var admin = app.ClienteDeRibeirao();

        // DUPLICANDO A DIRETORIA, sem a visão de todas as filiais.
        var criado = await ComStatusAsync(await admin.PostAsJsonAsync("/api/v1/admin/perfis", new
        {
            codigo = "diretoria_regional",
            nome = "Diretoria regional",
            descricao = "A diretoria, só da filial",
            permissoes = new[] { Permissao(Permissoes.IntegracaoLer), Permissao(Permissoes.UsuarioLer), Permissao(Permissoes.PercepcaoDoGestorInformar, "Organizacao") }
        }, Json), HttpStatusCode.Created);
        criado.GetProperty("codigo").GetString().Should().Be("DIRETORIA_REGIONAL", "o código fica em maiúsculas");
        criado.GetProperty("ehDoSistema").GetBoolean().Should().BeFalse();

        // APARECE NO FORMULÁRIO DE CONCESSÃO e vale para quem recebe.
        var formulario = (await ComStatusAsync(await admin.GetAsync("/api/v1/admin/perfis"), HttpStatusCode.OK)).GetProperty("dados").EnumerateArray();
        formulario.Single(p => p.GetProperty("codigo").GetString() == "DIRETORIA_REGIONAL").GetProperty("podeConceder").GetBoolean().Should().BeTrue();

        Guid barretos;
        await using (var db = Sistema(app)) barretos = await db.Usuarios.Where(u => u.Id == 200).Select(u => u.ChavePublica).SingleAsync();
        await ComStatusAsync(await admin.PostAsJsonAsync($"/api/v1/admin/usuarios/{barretos}/concessoes",
            new { perfilCodigo = "DIRETORIA_REGIONAL", justificativa = "diretor regional de Barretos" }, Json), HttpStatusCode.OK);
        (await app.ClienteDeBarretos().GetAsync("/api/v1/integracoes/sincronizacoes")).StatusCode.Should().Be(HttpStatusCode.OK);

        // TIRAR A PERMISSÃO DO PERFIL TIRA DE QUEM O TEM.
        var editado = await ComStatusAsync(await admin.PutAsJsonAsync("/api/v1/admin/perfis/DIRETORIA_REGIONAL", new
        {
            nome = "Diretoria regional",
            permissoes = new[] { Permissao(Permissoes.UsuarioLer) }
        }, Json), HttpStatusCode.OK);
        editado.GetProperty("permissoes").EnumerateArray().Select(p => p.GetProperty("codigo").GetString()).Should().Equal(Permissoes.UsuarioLer);
        (await app.ClienteDeBarretos().GetAsync("/api/v1/integracoes/sincronizacoes")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // DESATIVAR DESLIGA; REATIVAR RELIGA.
        await ComStatusAsync(await admin.PostAsync("/api/v1/admin/perfis/DIRETORIA_REGIONAL/desativacao", null), HttpStatusCode.OK);
        (await app.ClienteDeBarretos().GetAsync("/api/v1/admin/usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await ComStatusAsync(await admin.PostAsync("/api/v1/admin/perfis/DIRETORIA_REGIONAL/reativacao", null), HttpStatusCode.OK);
        (await app.ClienteDeBarretos().GetAsync("/api/v1/admin/usuarios")).StatusCode.Should().Be(HttpStatusCode.OK);

        // NA TRILHA, com o autor: a permissão que saiu do perfil.
        await using var banco = Sistema(app);
        (await banco.AlteracoesDeCampo.AsNoTracking()
                .Where(a => a.Entidade == nameof(PerfilPermissao) && a.Operacao == Dominio.Auditoria.OperacaoAuditada.Exclusao && a.AlteradoPorId == 100)
                .CountAsync())
            .Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("ADMINISTRADOR", "sistema")]
    [InlineData("x", "3 a 60")]
    [InlineData("1ABC", "3 a 60")]
    public async Task O_codigo_do_perfil_proprio_tem_formato_e_nao_colide_com_o_sistema(string codigo, string trecho)
    {
        await using var app = await ComAdministradorAsync();

        var resposta = await app.ClienteDeRibeirao().PostAsJsonAsync("/api/v1/admin/perfis",
            new { codigo, nome = "Qualquer", permissoes = Array.Empty<object>() }, Json);

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await CorpoAsync(resposta)).GetProperty("erros")[0].GetProperty("mensagem").GetString().Should().Contain(trecho);
    }

    [Fact]
    public async Task Ninguem_cria_um_perfil_maior_do_que_o_proprio_acesso()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await using (var db = Sistema(app))
        {
            db.Perfis.Add(Perfil.Criar("SO_PERFIS", "Só perfis").Conceder(Permissoes.PerfilAdministrar, Profundidade.Organizacao));
            await db.SaveChangesAsync();
        }
        await app.ConcederPerfilAsync(100, "SO_PERFIS");

        var resposta = await app.ClienteDeRibeirao().PostAsJsonAsync("/api/v1/admin/perfis", new
        {
            codigo = "EXCLUI_TUDO",
            nome = "Exclui tudo",
            permissoes = new[] { Permissao(Permissoes.ClienteExcluir, "Organizacao") }
        }, Json);

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CorpoAsync(resposta)).GetProperty("title").GetString().Should().Contain(Permissoes.ClienteExcluir);
    }
}
