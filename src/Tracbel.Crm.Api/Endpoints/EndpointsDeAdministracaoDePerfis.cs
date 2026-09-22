using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Seguranca;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// A ADMINISTRAÇÃO DE PERFIS (issue 113, parte 2b) — <c>Perfil.Administrar</c>, que só o Administrador tem.
///
/// <para><b>Os perfis do sistema são fixos</b> (decisão de 22/09/2026): a rota de edição os recusa com 409, e a
/// tela oferece duplicar. <b>Não existe DELETE</b>: desativar deixa de valer na hora, e nada some.</para>
/// </summary>
public static class EndpointsDeAdministracaoDePerfis
{
    private const string Base = "/api/v1/admin/perfis";
    private const string Tag = "Administração de perfis (banco do CRM)";

    /// <summary>Registra as rotas.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearAdministracaoDePerfis(this IEndpointRouteBuilder app)
    {
        app.MapGet($"{Base}/todos", async (ListarPerfisDaAdministracao caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithTags(Tag)
            .WithName("ListarPerfisDaAdministracao")
            .ExigePermissao(Permissoes.PerfilAdministrar)
            .WithSummary("Todos os perfis, inclusive os desativados, com as permissões, se é do sistema e quantas pessoas têm.");

        app.MapGet("/api/v1/admin/permissoes", (ListarCatalogoDePermissoes caso) => caso.Executar().Responder())
            .WithTags(Tag)
            .WithName("ListarCatalogoDePermissoes")
            .ExigePermissao(Permissoes.PerfilAdministrar)
            .WithSummary("O catálogo de permissões (em código), para o editor de perfil.");

        app.MapPost(Base, async (NovoPerfil corpo, AdministrarPerfil caso, CancellationToken ct) =>
                (await caso.CriarAsync(corpo, ct)).Responder(criado => Results.Created($"{Base}/todos", criado)))
            .WithTags(Tag)
            .WithName("CriarPerfil")
            .ExigePermissao(Permissoes.PerfilAdministrar)
            .WithSummary("Cria um perfil próprio — do zero ou duplicando outro (a tela manda as permissões).");

        app.MapPut($"{Base}/{{codigo}}", async (string codigo, EdicaoDePerfil corpo, AdministrarPerfil caso, CancellationToken ct) =>
                (await caso.EditarAsync(codigo, corpo, ct)).Responder())
            .WithTags(Tag)
            .WithName("EditarPerfil")
            .ExigePermissao(Permissoes.PerfilAdministrar)
            .WithSummary("Edita um perfil próprio: nome, descrição e o conjunto inteiro de permissões. Perfil do sistema é 409.");

        app.MapPost($"{Base}/{{codigo}}/desativacao", async (string codigo, AdministrarPerfil caso, CancellationToken ct) =>
                (await caso.DesativarAsync(codigo, ct)).Responder())
            .WithTags(Tag)
            .WithName("DesativarPerfil")
            .ExigePermissao(Permissoes.PerfilAdministrar)
            .WithSummary("Desativa um perfil próprio: deixa de valer na hora para quem tem.");

        app.MapPost($"{Base}/{{codigo}}/reativacao", async (string codigo, AdministrarPerfil caso, CancellationToken ct) =>
                (await caso.ReativarAsync(codigo, ct)).Responder())
            .WithTags(Tag)
            .WithName("ReativarPerfil")
            .ExigePermissao(Permissoes.PerfilAdministrar)
            .WithSummary("Reativa um perfil próprio desativado.");

        return app;
    }
}
