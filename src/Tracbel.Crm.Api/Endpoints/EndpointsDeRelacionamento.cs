using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Relacionamento;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// As rotas de LEITURA do relacionamento — processo, tarefa, interação e cobertura.
///
/// <para>Todas são somente leitura nesta etapa, e é uma decisão, não uma etapa faltando: a
/// escrita de processo e de tarefa carrega o motor de regras (mudar de fase dispara automação),
/// e ele entra junto com a tela que o exercita. O que existe aqui é o que as telas precisam para
/// mostrar número real em vez de exemplo.</para>
///
/// <para>Cada endpoint faz três coisas e nada mais: recebe, delega ao caso de uso e traduz o
/// <c>Resultado</c> em código HTTP. Nenhuma decisão de negócio mora neste arquivo.</para>
/// </summary>
public static class EndpointsDeRelacionamento
{
    /// <summary>Registra as rotas de processo.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearProcessos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/processos")
            .WithTags("Processos (banco do CRM)");

        grupo.MapGet("/", async (
                ListarProcessos caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                string? termo = null,
                string? situacao = null,
                Guid? clienteChave = null,
                string? faseCodigo = null,
                string? tipoProcessoCodigo = null,
                string? ordenarPor = null,
                bool descendente = false,
                bool incluirEncerrados = false) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, termo, situacao, clienteChave, faseCodigo, tipoProcessoCodigo,
                ordenarPor, descendente, incluirEncerrados, ct)).Responder())
            .WithName("ListarProcessos")
            .WithSummary("Lista processos e oportunidades, com fase, valor e tempo de fase.");

        grupo.MapGet("/{chave:guid}", async (Guid chave, ObterProcesso caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithName("ObterProcesso")
            .WithSummary("Traz a ficha de um processo pela chave pública.");

        return app;
    }

    /// <summary>Registra as rotas de tarefa.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearTarefas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/tarefas")
            .WithTags("Tarefas (banco do CRM)");

        grupo.MapGet("/", async (
                ListarTarefas caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                bool minhas = false,
                string? situacao = null,
                Guid? clienteChave = null,
                DateOnly? de = null,
                DateOnly? ate = null,
                bool somenteAtrasadas = false,
                string? ordenarPor = null,
                bool descendente = false) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, minhas, situacao, clienteChave, de, ate, somenteAtrasadas,
                ordenarPor, descendente, ct)).Responder())
            .WithName("ListarTarefas")
            .WithSummary("Lista tarefas da agenda, com prazo e atraso calculados.");

        return app;
    }

    /// <summary>Registra as rotas de interação.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearInteracoes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/interacoes")
            .WithTags("Interações (banco do CRM)");

        grupo.MapGet("/", async (
                ListarInteracoes caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                Guid? clienteChave = null,
                string? natureza = null,
                DateOnly? de = null,
                DateOnly? ate = null) =>
            (await caso.ExecutarAsync(pagina, tamanho, clienteChave, natureza, de, ate, ct)).Responder())
            .WithName("ListarInteracoes")
            .WithSummary("Lista a linha do tempo de contatos, filtrada por cliente.");

        return app;
    }

    /// <summary>Registra as rotas de cobertura de carteira.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearCobertura(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/cobertura")
            .WithTags("Cobertura de carteira (banco do CRM)");

        grupo.MapGet("/", async (
                ListarCobertura caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                string? classe = null,
                int? diasSemContato = null,
                bool somenteSemContato = false,
                string? ordenarPor = null,
                bool descendente = false) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, classe, diasSemContato, somenteSemContato,
                ordenarPor, descendente, ct)).Responder())
            .WithName("ListarCobertura")
            .WithSummary("A carteira cliente a cliente, com a data do último contato e o atraso de ciclo.");

        return app;
    }

    /// <summary>
    /// Registra as rotas de relatório — os agregados que as telas consomem.
    ///
    /// <para><b>Tudo aqui é calculado no banco.</b> A tela recebe dez linhas de funil, não 45 mil
    /// processos para somar no navegador. E quando o dado não sustenta a métrica, a resposta traz
    /// <c>metricasSemDado</c> com o motivo medido, em vez de um número construído sobre nada.</para>
    /// </summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearRelatorios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/relatorios")
            .WithTags("Relatórios (agregados calculados no banco)");

        grupo.MapGet("/funil", async (ObterFunil caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterFunil")
            .WithSummary("O funil por fase: quantos processos e quanto valor, com a cobertura do valor.");

        grupo.MapGet("/perdas", async (ObterPerdas caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterPerdas")
            .WithSummary("As perdas por motivo.");

        grupo.MapGet("/vendas-perdidas", async (ObterVendasPerdidas caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterVendasPerdidas")
            .WithSummary(
                "As vendas perdidas registradas no formulário: por motivo, para qual concorrente " +
                "e a que distância de preço.");

        grupo.MapGet("/faturamento", async (ObterFaturamento caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterFaturamento")
            .WithSummary(
                "O faturamento lido da SD2 do Protheus: série dos últimos doze meses e os cinco " +
                "maiores clientes, com a competência mais recente sempre junto do número.");

        grupo.MapGet("/cen", async (
                ObterPainelDoCen caso, CancellationToken ct, Guid? responsavel = null) =>
                (await caso.ExecutarAsync(responsavel, ct)).Responder())
            .WithName("ObterPainelDoCen")
            .WithSummary(
                "O painel de um CEN: cobertura por classe A/B/C/D contra a cadência declarada, " +
                "processos ganhos e perdidos, e o faturamento da carteira. Sem `responsavel`, " +
                "devolve o consolidado.");

        grupo.MapGet("/agenda", async (ObterPainelDaAgenda caso, CancellationToken ct, bool minhas = false) =>
                (await caso.ExecutarAsync(minhas, ct)).Responder())
            .WithName("ObterPainelDaAgenda")
            .WithSummary("O painel do CEN: pendentes, atrasadas, hoje, próximos sete dias.");

        grupo.MapGet("/cobertura", async (ObterResumoDeCobertura caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterResumoDeCobertura")
            .WithSummary("A cobertura por carteira: clientes, contatados em 30 e 90 dias, nunca contatados.");

        return app;
    }
}
