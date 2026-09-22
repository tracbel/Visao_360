using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Integracoes;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// AS INTEGRAÇÕES CONFIGURÁVEIS (issue 136). Ler é <c>Integracao.Ler</c> (Gerência para cima); configurar, gravar a
/// credencial, testar, reagendar e "rodar agora" é <c>Integracao.Administrar</c> (o Administrador).
///
/// <para><b>A senha tem rota própria</b> (<c>PUT …/segredo</c>) e nunca volta em resposta nenhuma. Não existe
/// DELETE: retirar a credencial e desligar o monitoramento são ações registradas, e nada some.</para>
/// </summary>
public static class EndpointsDeAdministracaoDeIntegracoes
{
    private const string Base = "/api/v1/admin/integracoes";
    private const string Tag = "Integrações configuráveis (banco do CRM)";

    /// <summary>Registra as rotas.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearAdministracaoDeIntegracoes(this IEndpointRouteBuilder app)
    {
        app.MapGet(Base, async (ListarIntegracoes caso, CancellationToken ct) => (await caso.ExecutarAsync(ct)).Responder())
            .WithTags(Tag).WithName("ListarIntegracoes")
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("As conexões (com a origem da credencial, nunca a senha) e as rotinas do servidor, com a agenda.");

        app.MapGet($"{Base}/conexoes/{{codigo}}/verificacoes", async (string codigo, ListarHistoricoDeIntegracoes caso, CancellationToken ct) =>
                (await caso.VerificacoesAsync(codigo, ct)).Responder())
            .WithTags(Tag).WithName("ListarVerificacoesDaConexao")
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("Os últimos testes de uma conexão, com quem testou e o que respondeu.");

        app.MapGet($"{Base}/rotinas/{{codigo}}/execucoes", async (string codigo, ListarHistoricoDeIntegracoes caso, CancellationToken ct) =>
                (await caso.ExecucoesAsync(codigo, ct)).Responder())
            .WithTags(Tag).WithName("ListarExecucoesDaRotina")
            .ExigePermissao(Permissoes.IntegracaoLer)
            .WithSummary("As últimas execuções de uma rotina, com o motivo, quem pediu e o resultado.");

        app.MapPost($"{Base}/conexoes", async (NovaConexaoMonitorada corpo, AdministrarConexoes caso, CancellationToken ct) =>
                (await caso.CriarMonitoradaAsync(corpo, ct)).Responder(criada => Results.Created(Base, criada)))
            .WithTags(Tag).WithName("CriarConexaoMonitorada")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Cadastra uma API para ser monitorada: GET, status esperado e, se pedir, um segredo no cabeçalho.");

        app.MapPut($"{Base}/conexoes/{{codigo}}", async (string codigo, ConfiguracaoDeConexao corpo, AdministrarConexoes caso, CancellationToken ct) =>
                (await caso.ConfigurarAsync(codigo, corpo, ct)).Responder())
            .WithTags(Tag).WithName("ConfigurarConexao")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Troca o endereço, o banco, a visão e o usuário; na monitorada, o nome e o monitoramento. A senha vai à parte.");

        app.MapPut($"{Base}/conexoes/{{codigo}}/segredo", async (string codigo, SegredoDaConexao corpo, AdministrarConexoes caso, CancellationToken ct) =>
                (await caso.DefinirSegredoAsync(codigo, corpo, ct)).Responder())
            .WithTags(Tag).WithName("DefinirSegredoDaConexao")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Grava a senha, protegida no servidor. Nenhuma resposta a devolve.");

        app.MapPost($"{Base}/conexoes/{{codigo}}/segredo/remocao", async (string codigo, AdministrarConexoes caso, CancellationToken ct) =>
                (await caso.RemoverSegredoAsync(codigo, ct)).Responder())
            .WithTags(Tag).WithName("RemoverSegredoDaConexao")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Retira a credencial da tela; a conexão volta à variável de ambiente do servidor, se houver.");

        app.MapPost($"{Base}/conexoes/{{codigo}}/teste", async (string codigo, AdministrarConexoes caso, CancellationToken ct) =>
                (await caso.TestarAsync(codigo, ct)).Responder())
            .WithTags(Tag).WithName("TestarConexao")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Testa no servidor, só leitura, e guarda o resultado no histórico.");

        app.MapPost($"{Base}/conexoes/{{codigo}}/desativacao", async (string codigo, AdministrarConexoes caso, CancellationToken ct) =>
                (await caso.DesativarAsync(codigo, ct)).Responder())
            .WithTags(Tag).WithName("DesativarConexao")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Tira uma API monitorada do monitoramento, sem apagar.");

        app.MapPost($"{Base}/conexoes/{{codigo}}/reativacao", async (string codigo, AdministrarConexoes caso, CancellationToken ct) =>
                (await caso.ReativarAsync(codigo, ct)).Responder())
            .WithTags(Tag).WithName("ReativarConexao")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Volta a monitorar uma API.");

        app.MapPut($"{Base}/rotinas/{{codigo}}", async (string codigo, AgendaNaTela corpo, AdministrarRotinas caso, CancellationToken ct) =>
                (await caso.ReagendarAsync(codigo, corpo, ct)).Responder())
            .WithTags(Tag).WithName("ReagendarRotina")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Troca a agenda e liga ou desliga. O que ficou para trás da agenda antiga não é cobrado.");

        app.MapPost($"{Base}/rotinas/{{codigo}}/execucao", async (string codigo, AdministrarRotinas caso, CancellationToken ct) =>
                (await caso.PedirExecucaoAsync(codigo, ct)).Responder())
            .WithTags(Tag).WithName("PedirExecucaoDaRotina")
            .ExigePermissao(Permissoes.IntegracaoAdministrar)
            .WithSummary("Põe um \"rodar agora\" na fila; o orquestrador começa em até cinco minutos.");

        return app;
    }
}
