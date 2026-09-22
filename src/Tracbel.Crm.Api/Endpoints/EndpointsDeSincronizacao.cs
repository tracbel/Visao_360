using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Integracoes;
using Tracbel.Crm.Dominio.Seguranca;

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
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("Cada fluxo de sincronização com a última execução, o último sucesso e as execuções recentes.");

        app.MapGet("/api/v1/integracoes/fontes-publicas", async (ObterFontesPublicas caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithTags("Integrações (administração)")
            .WithName("ObterFontesPublicas")
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("O painel de fontes públicas do potencial: última atualização, período, cobertura, recusas e próxima execução (issue 77).")
            .WithDescription(
                "Uma linha por fluxo da carga (IBGE, ANP, CONAB, Socicana, Banco Central). A última atualização é a rodada que a " +
                "carga gravou em integracao.PontoDeSincronismo — rodada que falha não grava nada. A fonte fica Atrasada quando a " +
                "execução agendada passou (com 6 horas de margem) e ela não foi atualizada depois; SemDado quando a tabela está vazia.");

        app.MapGet("/api/v1/integracoes/cobertura-do-motor", async (ObterCoberturaDoMotor caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithTags("Integrações (administração)")
            .WithName("ObterCoberturaDoMotor")
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("A cobertura do dado que o motor de mercado vai usar, por cultura, mês e campo (issue 150).")
            .WithDescription(
                "Só contagens: municípios da ADR com a área de cada cultura divulgada (PAM), meses de cada série de preço, abas de custo " +
                "com custo total, meses do SICOR e municípios com crédito de máquina nas duas janelas, e o preenchimento do dado interno " +
                "(vendas de máquina, equipamentos, vendas perdidas, endereços). Nenhum valor em reais, nome de cliente ou de CEN. Cada item " +
                "vem Completo, Parcial ou Vazio, com a frase que diz o que falta e para quê. O dado interno passa pela fronteira de filial; " +
                "a resposta diz quantas filiais entraram na conta.");

        return app;
    }
}
