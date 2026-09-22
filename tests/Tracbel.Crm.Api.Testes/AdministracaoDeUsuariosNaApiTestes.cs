using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A ADMINISTRAÇÃO DE USUÁRIOS E CONCESSÕES pela API de verdade (issue 113). O que se prende aqui: liberar,
/// conceder, revogar, desativar e reativar valem na hora (o escopo da pessoa muda), ficam na trilha com o
/// autor, e as três regras não cedem — ninguém age sobre a própria conta, ninguém dá mais do que tem, e a
/// gerência só vê a própria filial.
///
/// <para>Cada teste sobe a sua API: as concessões mudam o estado, e um teste não pode herdar o do outro.</para>
/// </summary>
public sealed class AdministracaoDeUsuariosNaApiTestes
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private const string Base = "/api/v1/admin/usuarios";
    private const string Pendente = "pessoa.nova@exemplo.invalid";

    private static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();

    private static async Task<JsonElement> OkAsync(HttpResponseMessage resposta)
    {
        var texto = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, texto);
        return JsonDocument.Parse(texto).RootElement.Clone();
    }

    private static CrmDbContext Sistema(ApiEmMemoria app) =>
        new(app.Services.CreateScope().ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);

    private static async Task<ApiEmMemoria> ComAdministradorEmRibeiraoAsync()
    {
        var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        return app;
    }

    private static async Task<Guid> ChaveAsync(ApiEmMemoria app, long usuarioId)
    {
        await using var db = Sistema(app);
        return await db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.ChavePublica).SingleAsync();
    }

    private static Task<HttpResponseMessage> ConcederAsync(HttpClient http, Guid chave, string perfil, string? filial = null, string justificativa = "autorizado pela diretoria em 22/09/2026") =>
        http.PostAsJsonAsync($"{Base}/{chave}/concessoes", new { perfilCodigo = perfil, filialCodigo = filial, justificativa }, Json);

    [Fact]
    public async Task O_usuario_comum_nao_ve_a_administracao()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        var resposta = await app.ClienteDeRibeirao().GetAsync(Base);

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CorpoAsync(resposta)).GetProperty("detail").GetString().Should().Contain(Permissoes.UsuarioLer);
    }

    [Fact]
    public async Task Conceder_vale_na_hora_fica_na_trilha_e_revogar_tira_sem_apagar_o_historico()
    {
        await using var app = await ComAdministradorEmRibeiraoAsync();
        var admin = app.ClienteDeRibeirao();
        var barretos = await ChaveAsync(app, 200);

        (await app.ClienteDeBarretos().GetAsync("/api/v1/integracoes/sincronizacoes")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var concedida = await OkAsync(await ConcederAsync(admin, barretos, PerfisDeSistema.Gerencia));
        var concessao = concedida.GetProperty("concessoes").EnumerateArray().Single();
        concessao.GetProperty("perfilCodigo").GetString().Should().Be(PerfisDeSistema.Gerencia);
        concessao.GetProperty("vigente").GetBoolean().Should().BeTrue();
        concessao.GetProperty("concedidaPor").GetString().Should().Be(ApiEmMemoria.UsuarioDeRibeirao);

        // VALE NA HORA: a próxima requisição da pessoa já tem a permissão.
        (await app.ClienteDeBarretos().GetAsync("/api/v1/integracoes/sincronizacoes")).StatusCode.Should().Be(HttpStatusCode.OK);

        var id = concessao.GetProperty("id").GetInt64();
        var semMotivo = await admin.PostAsJsonAsync($"{Base}/{barretos}/concessoes/{id}/revogacao", new { motivo = " " }, Json);
        semMotivo.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var revogada = await OkAsync(await admin.PostAsJsonAsync($"{Base}/{barretos}/concessoes/{id}/revogacao", new { motivo = "mudou de função" }, Json));
        var historico = revogada.GetProperty("concessoes").EnumerateArray().Single();
        historico.GetProperty("vigente").GetBoolean().Should().BeFalse();
        historico.GetProperty("motivoDaRevogacao").GetString().Should().Be("mudou de função");
        historico.GetProperty("justificativa").GetString().Should().Be("autorizado pela diretoria em 22/09/2026", "a concessão continua registrada");

        (await app.ClienteDeBarretos().GetAsync("/api/v1/integracoes/sincronizacoes")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // A REVOGADA NÃO IMPEDE UMA CONCESSÃO NOVA — nem na mesma filial.
        (await ConcederAsync(admin, barretos, PerfisDeSistema.Gerencia, ApiEmMemoria.FilialDeBarretos)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ConcederAsync(admin, barretos, PerfisDeSistema.Gerencia, ApiEmMemoria.FilialDeBarretos)).StatusCode.Should().Be(HttpStatusCode.Conflict,
            "a mesma concessão vigente não se repete");

        await using var db = Sistema(app);
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(UsuarioPerfil)).ToListAsync();
        trilha.Should().Contain(a => a.Campo == nameof(UsuarioPerfil.Justificativa) && a.AlteradoPorId == 100 && a.Origem == OrigemDaOperacao.Usuario);
        trilha.Should().Contain(a => a.Campo == nameof(UsuarioPerfil.MotivoDaRevogacao) && a.ValorNovo == "mudou de função" && a.AlteradoPorId == 100);
    }

    [Fact]
    public async Task Ninguem_concede_a_si_mesmo_nem_desativa_a_propria_conta()
    {
        await using var app = await ComAdministradorEmRibeiraoAsync();
        var admin = app.ClienteDeRibeirao();
        var propria = await ChaveAsync(app, 100);

        var concessao = await ConcederAsync(admin, propria, PerfisDeSistema.ExclusaoDeCadastro);
        concessao.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await CorpoAsync(concessao)).GetProperty("title").GetString().Should().Contain("própria conta");

        (await admin.PostAsync($"{Base}/{propria}/desativacao", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Ninguem_da_um_acesso_maior_do_que_o_proprio()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        // UM PERFIL QUE ADMINISTRA USUÁRIOS E MAIS NADA, concedido a Barretos.
        await using (var db = Sistema(app))
        {
            db.Perfis.Add(Perfil.Criar("ADMIN_DE_CONTAS", "Administra contas")
                .Conceder(Permissoes.UsuarioAdministrar, Profundidade.Organizacao)
                .Conceder(Permissoes.UsuarioLer, Profundidade.Organizacao));
            await db.SaveChangesAsync();
        }
        await app.ConcederPerfilAsync(200, "ADMIN_DE_CONTAS");

        var ribeirao = await ChaveAsync(app, 100);
        var resposta = await ConcederAsync(app.ClienteDeBarretos(), ribeirao, PerfisDeSistema.Administrador);

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, "quem só administra contas não pode dar o perfil Administrador");
        var titulo = (await CorpoAsync(resposta)).GetProperty("title").GetString();
        titulo.Should().Contain(Permissoes.PerfilAdministrar).And.Contain("maior do que o próprio");

        var perfis = (await OkAsync(await app.ClienteDeBarretos().GetAsync("/api/v1/admin/perfis"))).GetProperty("dados").EnumerateArray().ToList();
        perfis.Single(p => p.GetProperty("codigo").GetString() == PerfisDeSistema.Administrador).GetProperty("podeConceder").GetBoolean().Should().BeFalse();
        perfis.Single(p => p.GetProperty("codigo").GetString() == PerfisDeSistema.Padrao).GetProperty("podeConceder").GetBoolean().Should().BeFalse("o padrão não se concede");
    }

    [Fact]
    public async Task A_conta_que_espera_aparece_na_fila_e_e_liberada_na_filial_escolhida()
    {
        await using var app = await ComAdministradorEmRibeiraoAsync();
        await using (var db = Sistema(app))
        {
            db.Usuarios.Add(Usuario.CriarNoPrimeiroLogin(Guid.NewGuid(), Pendente, "Pessoa Nova", Email.Criar(Pendente), 1, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        var admin = app.ClienteDeRibeirao();
        var fila = (await OkAsync(await admin.GetAsync($"{Base}?situacao=AguardandoLiberacao"))).GetProperty("dados");
        var item = fila.GetProperty("itens").EnumerateArray().Single();
        item.GetProperty("nomePrincipal").GetString().Should().Be(Pendente);
        var chave = item.GetProperty("chave").GetGuid();

        (await admin.PostAsJsonAsync($"{Base}/{chave}/concessoes", new { perfilCodigo = PerfisDeSistema.Gerencia, justificativa = "x" }, Json))
            .StatusCode.Should().Be(HttpStatusCode.Conflict, "primeiro se libera, depois se concede");

        var liberada = await OkAsync(await admin.PostAsJsonAsync($"{Base}/{chave}/liberacao", new { filialCodigo = ApiEmMemoria.FilialDeBarretos }, Json));
        liberada.GetProperty("usuario").GetProperty("filialCodigo").GetString().Should().Be(ApiEmMemoria.FilialDeBarretos);
        liberada.GetProperty("usuario").GetProperty("aguardandoLiberacaoDesde").ValueKind.Should().Be(JsonValueKind.Null);

        (await OkAsync(await admin.GetAsync($"{Base}?situacao=AguardandoLiberacao"))).GetProperty("dados").GetProperty("itens").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task A_gerencia_ve_so_os_usuarios_da_filial_e_nao_ve_a_fila_nem_age()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        var gerencia = app.ClienteDeRibeirao();

        var lista = (await OkAsync(await gerencia.GetAsync(Base))).GetProperty("dados").GetProperty("itens").EnumerateArray()
            .Select(u => u.GetProperty("nomePrincipal").GetString()).ToList();
        lista.Should().Equal(ApiEmMemoria.UsuarioDeRibeirao);

        (await gerencia.GetAsync($"{Base}/{await ChaveAsync(app, 200)}")).StatusCode.Should().Be(HttpStatusCode.NotFound, "Barretos está fora do alcance");
        (await gerencia.GetAsync($"{Base}?situacao=AguardandoLiberacao")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await ConcederAsync(gerencia, await ChaveAsync(app, 200), PerfisDeSistema.ExclusaoDeCadastro)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Desativar_tira_o_acesso_e_reativar_devolve()
    {
        await using var app = await ComAdministradorEmRibeiraoAsync();
        var admin = app.ClienteDeRibeirao();
        var barretos = await ChaveAsync(app, 200);

        var desativada = await OkAsync(await admin.PostAsync($"{Base}/{barretos}/desativacao", null));
        desativada.GetProperty("usuario").GetProperty("estaAtivo").GetBoolean().Should().BeFalse();
        (await app.ClienteDeBarretos().GetAsync("/api/v1/acesso/escopo")).IsSuccessStatusCode.Should().BeFalse("a conta desativada não entra");

        var lista = (await OkAsync(await admin.GetAsync($"{Base}?situacao=Desativada"))).GetProperty("dados").GetProperty("itens").EnumerateArray()
            .Select(u => u.GetProperty("nomePrincipal").GetString());
        lista.Should().Contain(ApiEmMemoria.UsuarioDeBarretos);

        (await OkAsync(await admin.PostAsync($"{Base}/{barretos}/reativacao", null))).GetProperty("usuario").GetProperty("estaAtivo").GetBoolean().Should().BeTrue();
        (await app.ClienteDeBarretos().GetAsync("/api/v1/acesso/escopo")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
