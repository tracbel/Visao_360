using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Potencial;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// OS PARÂMETROS DO POTENCIAL DE MERCADO — lidos por data e alterados por vigência (issue 71).
///
/// <para><b>Não existe PUT nem DELETE.</b> Mudar um parâmetro é registrar uma vigência nova (POST), e a
/// antiga continua valendo para as datas em que valia; o erro se desfaz revogando a vigência que ainda não
/// passou de hoje. É o que deixa o cálculo de uma data passada reproduzível.</para>
///
/// <para>A tela do administrador é a issue 77; estas rotas são o que ela vai usar.</para>
/// </summary>
public static class EndpointsDeParametrosDoPotencial
{
    private const string Base = "/api/v1/admin/parametros-do-potencial";

    /// <summary>Registra as rotas.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearParametrosDoPotencial(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup(Base)
            .WithTags("Parâmetros do potencial de mercado (banco do CRM)");

        grupo.MapGet("/", async (ObterParametrosDoPotencial caso, CancellationToken ct, string? em = null) =>
                (await caso.ExecutarAsync(em, ct)).Responder())
            .WithName("ObterParametrosDoPotencial")
            .ExigePermissao(Permissoes.ParametroDoPotencialLer)
            .WithSummary("Os parâmetros do potencial que valem numa data (padrão: hoje), com o que falta decidir.")
            .WithDescription(
                "Os gerais (janela, crédito, faixas, pesos, fator, limite da percepção), a regra de cada cultura e a " +
                "percepção do gestor por município. Uma data passada devolve o que valia naquela data. 'pendencias' diz, " +
                "em frase, o que está em aberto — o motor não usa valor padrão para isso.");

        grupo.MapGet("/opcoes", async (ListarOpcoesDosParametros caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ListarOpcoesDosParametros")
            .ExigePermissao(Permissoes.ParametroDoPotencialLer)
            .WithSummary("As listas de escolha dos formulários: os produtos da PAM (com a área na ADR) e os municípios da ADR (issue 77).");

        // O CATÁLOGO DE CULTURAS E CATEGORIAS (issue 165). É esta rota que tira as listas fixas de cultura
        // do front: a tela pede o catálogo e desenha o que vier.
        grupo.MapGet("/catalogo", async (ObterCatalogoDoMercado caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterCatalogoDoMercado")
            .ExigePermissao(Permissoes.ParametroDoPotencialLer)
            .WithSummary("As culturas e as categorias de máquina do catálogo (issue 165).")
            .WithDescription(
                "Cada cultura traz o segmento, a unidade comercial e o fator em quilos, a fonte do preço, a série de " +
                "custo e os produtos da PAM que a compõem — com a marca de quem entra na soma da lavoura. Categoria " +
                "sem produto do SICOR não é erro: o investimento do Banco Central não separa plantadeira nem pulverizador.");

        grupo.MapPost("/catalogo/culturas", async (NovaCultura corpo, CadastrarCultura caso, CancellationToken ct) =>
                (await caso.IncluirAsync(corpo, ct)).Responder())
            .WithName("IncluirCultura")
            .ExigePermissao(Permissoes.ParametroDoPotencialAdministrar)
            .WithSummary("Cadastra uma cultura nova — ela passa a aparecer nas telas sem publicação.");

        grupo.MapPut("/catalogo/culturas/{codigo}", async (string codigo, NovaCultura corpo, CadastrarCultura caso, CancellationToken ct) =>
                (await caso.AlterarAsync(codigo, corpo, ct)).Responder())
            .WithName("AlterarCultura")
            .ExigePermissao(Permissoes.ParametroDoPotencialAdministrar)
            .WithSummary("Altera uma cultura. O código é a identidade e não muda; desligar não apaga o histórico.");

        grupo.MapGet("/historico", async (ListarHistoricoDosParametrosDoPotencial caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ListarHistoricoDosParametrosDoPotencial")
            .ExigePermissao(Permissoes.ParametroDoPotencialLer)
            .WithSummary("Todas as vigências já registradas, inclusive as revogadas e as futuras, com autor e justificativa.");

        grupo.MapPost("/geral", async (NovoParametroDoPotencial corpo, InformarParametroDoPotencial caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(corpo, ct)).Responder(criado => Results.Created($"{Base}/historico", criado)))
            .WithName("InformarParametroDoPotencial")
            .ExigePermissao(Permissoes.ParametroDoPotencialAdministrar)
            .WithSummary("Registra uma vigência nova dos parâmetros gerais — o conjunto inteiro, a partir de hoje ou depois.");

        grupo.MapPost("/culturas", async (NovaRegraDePotencial corpo, InformarRegraDePotencial caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(corpo, ct)).Responder(criado => Results.Created($"{Base}/historico", criado)))
            .WithName("InformarRegraDePotencial")
            .ExigePermissao(Permissoes.ParametroDoPotencialAdministrar)
            .WithSummary("Registra uma vigência nova da regra de uma cultura: hectares por máquina, anos de renovação e modelo.");

        grupo.MapPost("/percepcoes", async (NovaPercepcaoDoGestor corpo, InformarPercepcaoDoGestor caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(corpo, ct)).Responder(criado => Results.Created($"{Base}/historico", criado)))
            .WithName("InformarPercepcaoDoGestor")
            .ExigePermissao(Permissoes.PercepcaoDoGestorInformar)
            .WithSummary("Registra a percepção do gestor sobre um município, dentro do limite dos parâmetros gerais.");

        grupo.MapPost("/geral/{vigenteDesde}/revogacao", async (
                string vigenteDesde, RevogacaoDeVigencia corpo, RevogarParametroDoPotencial caso, CancellationToken ct) =>
            (await caso.RevogarGeralAsync(vigenteDesde, corpo, ct)).Responder())
            .WithName("RevogarParametroGeralDoPotencial")
            .ExigePermissao(Permissoes.ParametroDoPotencialAdministrar)
            .WithSummary("Revoga a vigência dos parâmetros gerais que começa na data (aaaa-mm-dd) — só se ainda não passou de hoje.");

        grupo.MapPost("/culturas/{produtoCodigoIbge:int}/{vigenteDesde}/revogacao", async (
                int produtoCodigoIbge, string vigenteDesde, RevogacaoDeVigencia corpo, RevogarParametroDoPotencial caso, CancellationToken ct) =>
            (await caso.RevogarRegraAsync(produtoCodigoIbge, vigenteDesde, corpo, ct)).Responder())
            .WithName("RevogarRegraDePotencial")
            .ExigePermissao(Permissoes.ParametroDoPotencialAdministrar)
            .WithSummary("Revoga a vigência da regra de um produto que começa na data — só se ainda não passou de hoje.");

        grupo.MapPost("/percepcoes/{municipioCodigoIbge:int}/{vigenteDesde}/revogacao", async (
                int municipioCodigoIbge, string vigenteDesde, RevogacaoDeVigencia corpo, RevogarParametroDoPotencial caso, CancellationToken ct) =>
            (await caso.RevogarPercepcaoAsync(municipioCodigoIbge, vigenteDesde, corpo, ct)).Responder())
            .WithName("RevogarPercepcaoDoGestor")
            .ExigePermissao(Permissoes.PercepcaoDoGestorInformar)
            .WithSummary("Revoga a vigência da percepção de um município que começa na data — só se ainda não passou de hoje.");

        return app;
    }
}
