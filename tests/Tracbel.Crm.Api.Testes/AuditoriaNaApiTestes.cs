using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A TRILHA DE AUDITORIA NA TELA (issue 135), pela API de verdade. O que se prende: a concessão de perfil e a
/// liberação de conta aparecem com autor, justificativa e nomes no lugar de identificadores; a fronteira de filial
/// vale; os filtros e a paginação são por evento; quem não tem <c>Auditoria.Ler</c> recebe 403.
/// </summary>
public sealed class AuditoriaNaApiTestes
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private const string Base = "/api/v1/admin/auditoria";
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

    private static async Task<Guid> ChaveAsync(ApiEmMemoria app, long usuarioId)
    {
        await using var db = Sistema(app);
        return await db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.ChavePublica).SingleAsync();
    }

    private static HttpClient EmTodasAsFiliais(ApiEmMemoria app) =>
        app.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ContextoAcesso.CodigoDeTodasAsFiliais);

    private static List<JsonElement> Itens(JsonElement corpo) => corpo.GetProperty("dados").GetProperty("itens").EnumerateArray().ToList();

    private static string? Depois(JsonElement evento, string campo) =>
        evento.GetProperty("campos").EnumerateArray().Single(c => c.GetProperty("campo").GetString() == campo).GetProperty("depois").GetString();

    /// <summary>O administrador em Ribeirão concede a Gerência a Barretos e libera uma conta nova em Barretos.</summary>
    private static async Task<ApiEmMemoria> ComConcessaoELiberacaoAsync()
    {
        var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        await using (var db = Sistema(app))
        {
            db.Usuarios.Add(Usuario.CriarNoPrimeiroLogin(Guid.NewGuid(), Pendente, "Pessoa Nova", Email.Criar(Pendente), 1, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        var admin = app.ClienteDeRibeirao();
        var barretos = await ChaveAsync(app, 200);
        await OkAsync(await admin.PostAsJsonAsync($"/api/v1/admin/usuarios/{barretos}/concessoes",
            new { perfilCodigo = PerfisDeSistema.Gerencia, justificativa = "gerente de Barretos desde setembro" }, Json));

        var fila = Itens(await OkAsync(await admin.GetAsync("/api/v1/admin/usuarios?situacao=AguardandoLiberacao")));
        var pendente = fila.Single().GetProperty("chave").GetGuid();
        await OkAsync(await admin.PostAsJsonAsync($"/api/v1/admin/usuarios/{pendente}/liberacao", new { filialCodigo = ApiEmMemoria.FilialDeBarretos }, Json));

        return app;
    }

    [Fact]
    public async Task Quem_nao_tem_auditoria_ler_recebe_403_e_a_gerencia_tambem()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        var comum = await app.ClienteDeRibeirao().GetAsync(Base);
        comum.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CorpoAsync(comum)).GetProperty("detail").GetString().Should().Contain(Permissoes.AuditoriaLer);

        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        (await app.ClienteDeRibeirao().GetAsync(Base)).StatusCode.Should().Be(HttpStatusCode.Forbidden, "a Gerência não lê a trilha");
        (await app.ClienteDeRibeirao().GetAsync($"{Base}/entidades")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_concessao_e_a_liberacao_aparecem_com_autor_justificativa_e_nomes()
    {
        await using var app = await ComConcessaoELiberacaoAsync();

        var eventos = Itens(await OkAsync(await EmTodasAsFiliais(app).GetAsync(Base)));

        var concessao = eventos.Single(e => e.GetProperty("entidade").GetString() == nameof(UsuarioPerfil));
        concessao.GetProperty("entidadeRotulo").GetString().Should().Be("Concessão de perfil");
        concessao.GetProperty("operacao").GetString().Should().Be("Inclusao");
        concessao.GetProperty("origem").GetString().Should().Be("Usuario");
        concessao.GetProperty("autor").GetString().Should().Be(ApiEmMemoria.UsuarioDeRibeirao);
        concessao.GetProperty("registro").GetString().Should().Be($"Gerência para {ApiEmMemoria.UsuarioDeBarretos}");
        Depois(concessao, nameof(UsuarioPerfil.Justificativa)).Should().Be("gerente de Barretos desde setembro");
        Depois(concessao, nameof(UsuarioPerfil.PerfilId)).Should().Be("Gerência", "o identificador vira o nome");
        Depois(concessao, nameof(UsuarioPerfil.UsuarioId)).Should().Be(ApiEmMemoria.UsuarioDeBarretos);
        concessao.GetProperty("campos").EnumerateArray().Single(c => c.GetProperty("campo").GetString() == nameof(UsuarioPerfil.Justificativa))
            .GetProperty("rotulo").GetString().Should().Be("Justificativa");

        var liberacao = eventos.Single(e => e.GetProperty("entidade").GetString() == nameof(Usuario));
        liberacao.GetProperty("registro").GetString().Should().Be("Pessoa Nova");
        liberacao.GetProperty("operacao").GetString().Should().Be("Alteracao");
        liberacao.GetProperty("filialCodigo").GetString().Should().Be(ApiEmMemoria.FilialDeBarretos, "a conta foi liberada em Barretos");
        Depois(liberacao, nameof(Usuario.EmpresaId)).Should().Be($"{ApiEmMemoria.FilialDeBarretos} · Tracbel Agro — Barretos");
        var espera = liberacao.GetProperty("campos").EnumerateArray().Single(c => c.GetProperty("campo").GetString() == nameof(Usuario.AguardandoLiberacaoDesde));
        espera.GetProperty("antes").GetString().Should().NotBeNullOrEmpty();
        espera.GetProperty("depois").ValueKind.Should().Be(JsonValueKind.Null, "liberar é deixar de esperar");
    }

    [Fact]
    public async Task A_trilha_respeita_a_fronteira_de_filial()
    {
        await using var app = await ComConcessaoELiberacaoAsync();

        // EM RIBEIRÃO, só o que é de Ribeirão: a concessão sem filial fica na filial de casa de quem concedeu, e a
        // conta liberada em Barretos é de Barretos.
        var emRibeirao = Itens(await OkAsync(await app.ClienteDeRibeirao().GetAsync(Base)));
        emRibeirao.Select(e => e.GetProperty("entidade").GetString()).Should().Equal(nameof(UsuarioPerfil));

        var emTodas = Itens(await OkAsync(await EmTodasAsFiliais(app).GetAsync(Base)));
        emTodas.Select(e => e.GetProperty("entidade").GetString()).Should().BeEquivalentTo([nameof(UsuarioPerfil), nameof(Usuario)]);
    }

    [Fact]
    public async Task Os_filtros_e_a_paginacao_sao_por_evento()
    {
        await using var app = await ComConcessaoELiberacaoAsync();
        var todas = EmTodasAsFiliais(app);

        Itens(await OkAsync(await todas.GetAsync($"{Base}?entidade=usuarioperfil"))).Should().ContainSingle("a caixa da entidade não importa");
        Itens(await OkAsync(await todas.GetAsync($"{Base}?operacao=Inclusao"))).Select(e => e.GetProperty("entidade").GetString()).Should().Equal(nameof(UsuarioPerfil));
        Itens(await OkAsync(await todas.GetAsync($"{Base}?autor=ribeirao"))).Should().HaveCount(2);
        Itens(await OkAsync(await todas.GetAsync($"{Base}?autor=ninguem"))).Should().BeEmpty();
        Itens(await OkAsync(await todas.GetAsync($"{Base}?origem=Integracao"))).Should().BeEmpty();

        var primeira = (await OkAsync(await todas.GetAsync($"{Base}?tamanho=1"))).GetProperty("dados");
        primeira.GetProperty("total").GetInt32().Should().Be(2, "a concessão grava vários campos e conta como um evento");
        primeira.GetProperty("itens").GetArrayLength().Should().Be(1);
        primeira.GetProperty("temProxima").GetBoolean().Should().BeTrue();
        var segunda = Itens(await OkAsync(await todas.GetAsync($"{Base}?tamanho=1&pagina=2")));
        segunda.Single().GetProperty("entidade").GetString().Should().NotBe(primeira.GetProperty("itens")[0].GetProperty("entidade").GetString());

        // O PERÍODO É EM DIAS: ontem não tem nada; hoje, os dois.
        var ontem = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3)).AddDays(-1).ToString("yyyy-MM-dd");
        Itens(await OkAsync(await todas.GetAsync($"{Base}?de={ontem}&ate={ontem}"))).Should().BeEmpty();
    }

    [Theory]
    [InlineData("entidade=Inexistente", "entidade")]
    [InlineData("de=2026-09-22&ate=2026-09-01", "de")]
    [InlineData("de=2024-01-01&ate=2026-09-22", "de")]
    [InlineData("de=22/09/2026", "de")]
    [InlineData("origem=Planilha", "origem")]
    public async Task O_filtro_invalido_volta_no_campo(string consulta, string campo)
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);

        var resposta = await app.ClienteDeRibeirao().GetAsync($"{Base}?{consulta}");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await CorpoAsync(resposta)).GetProperty("erros")[0].GetProperty("campo").GetString().Should().Be(campo);
    }

    [Fact]
    public async Task A_permissao_tirada_de_um_perfil_se_descreve_pelo_que_a_trilha_guardou()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var admin = app.ClienteDeRibeirao();

        (await admin.PostAsJsonAsync("/api/v1/admin/perfis", new
        {
            codigo = "LEITURA_DE_INTEGRACOES", nome = "Leitura de integrações",
            permissoes = new[] { new { codigo = Permissoes.IntegracaoLer, profundidade = "EmpresaEAbaixo" }, new { codigo = Permissoes.UsuarioLer, profundidade = "EmpresaEAbaixo" } }
        }, Json)).StatusCode.Should().Be(HttpStatusCode.Created);
        await OkAsync(await admin.PutAsJsonAsync("/api/v1/admin/perfis/LEITURA_DE_INTEGRACOES", new
        {
            nome = "Leitura de integrações",
            permissoes = new[] { new { codigo = Permissoes.UsuarioLer, profundidade = "EmpresaEAbaixo" } }
        }, Json));

        var exclusao = Itens(await OkAsync(await admin.GetAsync($"{Base}?entidade=PerfilPermissao&operacao=Exclusao"))).Single();

        exclusao.GetProperty("registro").GetString().Should().Be($"{Permissoes.IntegracaoLer} em Leitura de integrações",
            "a linha foi apagada, e a descrição sai do que a exclusão gravou");
        exclusao.GetProperty("campos").EnumerateArray().Single(c => c.GetProperty("campo").GetString() == "Profundidade")
            .GetProperty("antes").GetString().Should().Be("a filial escolhida e as que estão abaixo dela");
    }

    [Fact]
    public async Task A_diretoria_le_a_trilha_e_ve_as_entidades_do_filtro()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Diretoria);

        (await EmTodasAsFiliais(app).GetAsync(Base)).StatusCode.Should().Be(HttpStatusCode.OK);

        var entidades = (await OkAsync(await app.ClienteDeRibeirao().GetAsync($"{Base}/entidades"))).GetProperty("dados").EnumerateArray().ToList();
        entidades.Should().Contain(e => e.GetProperty("codigo").GetString() == nameof(UsuarioPerfil) && e.GetProperty("rotulo").GetString() == "Concessão de perfil");
        entidades.Select(e => e.GetProperty("rotulo").GetString()).Should().BeInAscendingOrder(StringComparer.Create(new System.Globalization.CultureInfo("pt-BR"), true));
    }
}
