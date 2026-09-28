using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Integracoes;
using Tracbel.Crm.Dominio.Comum;
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

        // O TEMPO DE RESPOSTA MEDIDO PELO PRÓPRIO SERVIDOR (issue 51). Não é caso de uso nem lê banco: a medida é da API,
        // em memória, e a procedência diz isso — e desde quando.
        app.MapGet("/api/v1/integracoes/desempenho", (MedidorDeDesempenho medidor, IRelogio relogio) =>
                Results.Ok(new ComProcedencia<DesempenhoDaApi>(
                    new DesempenhoDaApi(medidor.DesdeUtc, MedidorDeDesempenho.AmostrasPorRota, medidor.Resumo()),
                    new Procedencia(
                        Procedencias.SistemaProprio,
                        "medição da própria API, em memória, desde a última subida do serviço",
                        relogio.Agora,
                        medidor.DesdeUtc,
                        false,
                        null))))
            .WithTags("Integrações (administração)")
            .WithName("ObterDesempenhoDaApi")
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("O tempo de resposta de cada rota da API, medido pelo servidor: p50, p95 e máximo das chamadas recentes (issue 51).")
            .WithDescription(
                "Cada rota guarda as últimas mil chamadas, com o tempo do pipeline inteiro — contexto de acesso, permissão, caso de " +
                "uso, banco e trilha de auditoria. A medição recomeça a cada subida do serviço (toda publicação). 'grava' marca as " +
                "rotas que passam pela trilha na mesma transação — é o custo que o documento 45 §6.1 pediu para medir no servidor.");

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

        // A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS (28/09/2026): os números do CRM contra o gabarito da GN.
        app.MapGet("/api/v1/integracoes/conferencia-gn", async (Aplicacao.Relacionamento.ObterConferenciaComAGestao caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithTags("Integrações (administração)")
            .WithName("ObterConferenciaComAGestao")
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("A meta e o realizado de máquinas na API Gestão de Negócios e no CRM, por filial e mês, e cada máquina que não bate.")
            .WithDescription(
                "A última apuração da rotina 13 (Conferência com a Gestão de Negócios): a meta sem consórcio e o realizado — só a máquina " +
                "entregue, no mês da entrega, a mesma régua dos dois lados —, lá e aqui, pela filial do cabeçalho ou por todas; e as " +
                "divergências abertas do realizado, chassi a chassi, com o tipo e o que cada lado diz. Nenhum nome de cliente ou de CEN.");

        return app;
    }
}
