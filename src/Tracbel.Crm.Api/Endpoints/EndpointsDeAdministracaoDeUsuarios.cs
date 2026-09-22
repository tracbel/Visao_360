using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Seguranca;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// A ADMINISTRAÇÃO DE USUÁRIOS E CONCESSÕES (issue 113) — o que substitui o <c>INSERT</c> à mão em
/// <c>seguranca.UsuarioPerfil</c>.
///
/// <para><b>Não existe DELETE.</b> Revogar marca a concessão (quem, quando, por quê) e desativar marca a conta;
/// nada some, e a trilha de auditoria registra cada mudança com o autor.</para>
///
/// <para><b>Ver</b> exige <c>Usuario.Ler</c> (o Administrador tem, com todo o catálogo); <b>agir</b> exige
/// <c>Usuario.Administrar</c>. A rota declara a permissão mínima, e o caso de uso confere o resto — o alcance
/// por filial, a própria conta e a escalada (<see cref="RegraDeConcessao"/>).</para>
/// </summary>
public static class EndpointsDeAdministracaoDeUsuarios
{
    private const string Base = "/api/v1/admin/usuarios";

    /// <summary>Registra as rotas.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearAdministracaoDeUsuarios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup(Base).WithTags("Administração de usuários e concessões (banco do CRM)");

        grupo.MapGet("/", async (
                ListarUsuariosDaAdministracao caso, CancellationToken ct,
                string? situacao = null, string? termo = null, int? pagina = null, int? tamanho = null) =>
            (await caso.ExecutarAsync(situacao, termo, pagina, tamanho, ct)).Responder())
            .WithName("ListarUsuariosDaAdministracao")
            .ExigePermissao(Permissoes.UsuarioLer)
            .WithSummary("Os usuários: ativos (padrão), aguardando liberação ou desativados, com busca e paginação.")
            .WithDescription(
                "Dentro do alcance: Usuario.Ler em Organização vê todos; na filial, só os da filial escolhida. A fila de " +
                "liberação (situacao=AguardandoLiberacao) é de quem tem Usuario.Administrar.");

        grupo.MapGet("/{chave:guid}", async (Guid chave, ObterUsuarioDaAdministracao caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithName("ObterUsuarioDaAdministracao")
            .ExigePermissao(Permissoes.UsuarioLer)
            .WithSummary("A conta e todas as concessões, inclusive as revogadas e as vencidas, com autor e justificativa.");

        grupo.MapPost("/{chave:guid}/liberacao", async (Guid chave, LiberacaoDeUsuario corpo, AdministrarUsuario caso, CancellationToken ct) =>
                (await caso.LiberarAsync(chave, corpo, ct)).Responder())
            .WithName("LiberarUsuario")
            .ExigePermissao(Permissoes.UsuarioAdministrar)
            .WithSummary("Libera a conta que nasceu no primeiro login, na filial de casa escolhida.");

        grupo.MapPost("/{chave:guid}/concessoes", async (Guid chave, NovaConcessao corpo, AdministrarUsuario caso, CancellationToken ct) =>
                (await caso.ConcederAsync(chave, corpo, ct)).Responder())
            .WithName("ConcederPerfil")
            .ExigePermissao(Permissoes.UsuarioAdministrar)
            .WithSummary("Concede um perfil, com filial e validade opcionais e justificativa obrigatória.")
            .WithDescription("Recusa conceder à própria conta e conceder um perfil com permissão que quem concede não tem.");

        grupo.MapPost("/{chave:guid}/concessoes/{concessaoId:long}/revogacao", async (
                Guid chave, long concessaoId, RevogacaoDeConcessao corpo, AdministrarUsuario caso, CancellationToken ct) =>
            (await caso.RevogarAsync(chave, concessaoId, corpo, ct)).Responder())
            .WithName("RevogarConcessao")
            .ExigePermissao(Permissoes.UsuarioAdministrar)
            .WithSummary("Revoga uma concessão, com motivo. A linha fica, marcada, para o histórico.");

        grupo.MapPost("/{chave:guid}/desativacao", async (Guid chave, AdministrarUsuario caso, CancellationToken ct) =>
                (await caso.DesativarAsync(chave, ct)).Responder())
            .WithName("DesativarUsuario")
            .ExigePermissao(Permissoes.UsuarioAdministrar)
            .WithSummary("Desativa a conta: ela deixa de entrar, e nada dela é apagado.");

        grupo.MapPost("/{chave:guid}/reativacao", async (Guid chave, AdministrarUsuario caso, CancellationToken ct) =>
                (await caso.ReativarAsync(chave, ct)).Responder())
            .WithName("ReativarUsuario")
            .ExigePermissao(Permissoes.UsuarioAdministrar)
            .WithSummary("Reativa a conta desativada; os perfis ainda vigentes voltam a valer.");

        app.MapGet("/api/v1/admin/perfis", async (ListarPerfisParaConceder caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithTags("Administração de usuários e concessões (banco do CRM)")
            .WithName("ListarPerfisParaConceder")
            .ExigePermissao(Permissoes.UsuarioAdministrar)
            .WithSummary("Os perfis ativos, com o que cada um dá e se quem pergunta pode concedê-lo.");

        return app;
    }
}
