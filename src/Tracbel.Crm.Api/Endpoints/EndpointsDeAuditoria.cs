using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Auditoria;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// A TRILHA DE AUDITORIA (issue 135) — só leitura, com <c>Auditoria.Ler</c> (Diretoria e Administrador).
///
/// <para>A trilha nasce do <c>SaveChanges</c>, na mesma transação do dado (documento 41, fase 2); nenhuma rota a
/// grava nem a apaga. A leitura respeita a fronteira de filial: "Todas as filiais" no seletor mostra todas.</para>
/// </summary>
public static class EndpointsDeAuditoria
{
    private const string Base = "/api/v1/admin/auditoria";
    private const string Tag = "Trilha de auditoria (banco do CRM)";

    /// <summary>Registra as rotas.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearAuditoria(this IEndpointRouteBuilder app)
    {
        app.MapGet(Base, async (
                ConsultarTrilhaDeAuditoria caso, CancellationToken ct,
                string? de = null, string? ate = null, string? entidade = null, long? registro = null, string? autor = null,
                string? origem = null, string? operacao = null, int? pagina = null, int? tamanho = null) =>
            (await caso.ExecutarAsync(de, ate, entidade, registro, autor, origem, operacao, pagina, tamanho, ct)).Responder())
            .WithTags(Tag)
            .WithName("ConsultarTrilhaDeAuditoria")
            .ExigePermissao(Permissoes.AuditoriaLer)
            .WithSummary("Quem mudou o quê, quando, de quanto para quanto — um evento por gravação, do mais recente para trás.")
            .WithDescription(
                "Período em dias de São Paulo (de e ate em aaaa-mm-dd, inclusos; sem período, os últimos 30 dias; até um ano). " +
                "Filtros: entidade, registro, autor (trecho do nome ou do e-mail), origem e operacao. Dentro da fronteira de filial.");

        app.MapGet($"{Base}/entidades", (ListarEntidadesAuditadas caso) => caso.Executar().Responder())
            .WithTags(Tag)
            .WithName("ListarEntidadesAuditadas")
            .ExigePermissao(Permissoes.AuditoriaLer)
            .WithSummary("As entidades que a trilha registra, com o nome de cada uma, para o filtro da tela.");

        return app;
    }
}
