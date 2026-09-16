using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Integracoes;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// O registro das sincronizações, para a administração — só leitura (documento 35, seção 11).
///
/// <para>A sincronização em si não é disparada por aqui: ela roda no serviço do Windows
/// <c>TracbelCrmSincronizacaoArt</c>, no servidor, com operador e horário conhecidos.</para>
/// </summary>
public static class EndpointsDeSincronizacao
{
    /// <summary>Registra as rotas.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearSincronizacoes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/integracoes/sincronizacoes", async (
                ListarSincronizacoes caso, CancellationToken ct, int? execucoes = null) =>
            (await caso.ExecutarAsync(execucoes, ct)).Responder())
            .WithTags("Integrações (administração)")
            .WithName("ListarSincronizacoes")
            .WithSummary("Cada fluxo de sincronização com a última execução, o último sucesso e as execuções recentes.");

        return app;
    }
}
