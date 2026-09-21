using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Catalogos;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// As rotas de catálogo — o que alimenta os campos de seleção do front.
///
/// É O ENDPOINT QUE TORNA EXEQUÍVEL a regra mais repetida do documento 16: campo com catálogo
/// não aceita digitação livre. A tela só consegue oferecer apenas o que vale se tiver de onde
/// puxar a lista; sem isto, a alternativa real é o front embutir listas próprias — e listas
/// embutidas divergem.
/// </summary>
public static class EndpointsDeCatalogo
{
    /// <summary>Registra as rotas de catálogo.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearCatalogos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/catalogos")
            .WithTags("Catálogos (banco do CRM)");

        grupo.MapGet("/", async (ListarCatalogos caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(null, ct)).Responder())
            .WithName("ListarCatalogos")
            .ExigePermissao(Permissoes.CatalogoLer)
            .WithSummary("Todas as listas de seleção: catálogos de banco, modelos, filiais e domínios fechados.");

        grupo.MapGet("/{codigo}", async (string codigo, ListarCatalogos caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(codigo, ct))
                .Responder(lista => Results.Ok(lista with { Dados = lista.Dados })))
            .WithName("ObterCatalogo")
            .ExigePermissao(Permissoes.CatalogoLer)
            .WithSummary("Um catálogo pelo código. Ex.: ORIGEM_LEAD, MOTIVO_INATIVACAO, EMPRESA.");

        return app;
    }
}
