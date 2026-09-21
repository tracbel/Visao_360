using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// A ROTA DO ESCOPO EFETIVO — onde o usuário pode olhar e o que pode fazer (fase 3 do documento 41).
///
/// <para><b>Responde mesmo quando a filial pedida é recusada.</b> A tela guarda a última filial escolhida;
/// se ela deixar de ser permitida, toda chamada daria 403 — inclusive esta, que é a que diz quais filiais
/// são permitidas. Então o meio de campo de contexto monta esta rota, e só ela, pela filial de casa quando
/// a pedida é recusada, e a resposta diz qual foi recusada. A tela volta para a filial de casa sozinha.</para>
/// </summary>
public static class EndpointsDeAcesso
{
    /// <summary>O caminho da rota — o meio de campo de contexto o reconhece.</summary>
    public const string CaminhoDoEscopo = "/api/v1/acesso/escopo";

    /// <summary>A chave em <c>HttpContext.Items</c> onde o meio de campo deixa a filial recusada.</summary>
    public const string ChaveDaFilialRecusada = "Tracbel.FilialRecusada";

    /// <summary>Registra a rota.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearAcesso(this IEndpointRouteBuilder app)
    {
        app.MapGet(CaminhoDoEscopo, async (HttpContext http, ObterEscopoDeAcesso caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(http.Items[ChaveDaFilialRecusada] as string, ct)).Responder())
            .WithTags("Acesso (escopo efetivo)")
            .WithName("ObterEscopoDeAcesso")
            .WithSummary("As filiais que você pode escolher e o que pode fazer na filial atual.")
            .WithDescription(
                "A filial de casa e as filiais com perfil concedido (P-20); as permissões do perfil padrão somadas " +
                "às dos perfis concedidos e vigentes. Quando a filial do cabeçalho não é permitida, esta rota — e só " +
                "ela — responde pela filial de casa e informa qual foi recusada.")
            .SemPermissaoExigida("é o escopo do próprio usuário: exige identidade, e é o que a tela usa para saber o que mostrar");

        return app;
    }
}
