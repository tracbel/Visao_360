using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Mercado;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// AS ROTAS DE INTELIGÊNCIA DE MERCADO — hoje, a calculadora de máquinas (issue 161).
///
/// <para><b>A permissão é <c>Territorio.Ler</c>, e não uma nova.</b> A issue cita <c>Mercado.Ler</c>
/// (D-IM-11), que é <b>decisão em aberto</b>: criar a permissão agora deixaria a calculadora invisível
/// para todo mundo até alguém conceder o acesso perfil a perfil, e conceder por conta própria seria
/// alterar permissão de produção por suposição. A calculadora mora na tela de território, lê o mesmo
/// dado do território e nada além dele — enquanto D-IM-11 não sair, é a mesma porta.</para>
/// </summary>
public static class EndpointsDeMercado
{
    /// <summary>Registra a rota da calculadora de máquinas.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearMercado(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/mercado")
            .WithTags("Inteligência de mercado (banco do CRM)");

        grupo.MapPost("/calculadora", async (
                SimulacaoDeMaquinas entrada,
                SimularMaquinas caso,
                CancellationToken ct) =>
            (await caso.ExecutarAsync(entrada, ct)).Responder())
            .WithName("SimularMaquinas")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("Simula o parque de máquinas e a demanda anual de uma área.")
            .WithDescription(
                "É POST porque a entrada é um conjunto de áreas por cultura, não uma chave — mas NADA É " +
                "GRAVADO: simulação é pergunta, e uma resposta gravada viraria um número 'oficial' que " +
                "ninguém decidiu adotar.\n\n" +
                "Tudo é opcional, e cada ausência tem sentido: sem `municipioCodigoIbge`, a conta parte do " +
                "zero e vale só o que for digitado; SEM `areas`, ela devolve o número medido do município — " +
                "o mesmo que o mapa mostra, porque é a mesma função do domínio; sem `categoriaCodigo`, " +
                "entram todas as categorias de máquina; sem `data`, valem as regras de hoje.\n\n" +
                "A `data` escolhe a VIGÊNCIA (issue 71): simular com uma data passada usa a regra que valia " +
                "naquele dia.\n\n" +
                "Cultura sem regra de hectares por máquina é RECUSA com o nome dela, e não uma linha zerada: " +
                "quem digitou o código precisa saber que falta parâmetro, não que 'deu zero'.\n\n" +
                "O ajuste por cenário de mercado não entra: ele depende do fator de ciclo, cujos pesos são " +
                "decisão em aberto (D-P05). O campo `sobreOsCenarios` diz isso para a tela.");

        // O DIAGNÓSTICO COMERCIAL (issue 257) lê o mesmo dado do território, com o mesmo alcance — a mesma porta.
        grupo.MapGet("/diagnostico", async (
                ObterDiagnosticoComercial caso,
                CancellationToken ct,
                string? competenciaInicial = null,
                string? competenciaFinal = null,
                string? regiao = null,
                string? lojaCodigo = null,
                string? visao = null,
                string? categoria = null,
                string? responsavel = null) =>
            (await caso.ExecutarAsync(competenciaInicial, competenciaFinal, regiao, lojaCodigo, visao, categoria, responsavel, ct)).Responder())
            .WithName("ObterDiagnosticoComercial")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("O Índice de Oportunidade Comercial (IOC) de cada município da ADR, com a situação e o plano de ação.")
            .WithDescription(
                "Sete componentes de 0 a 1 — potencial, cobertura, crédito, rentabilidade, clientes, realização e " +
                "penetração —, ponderados pelos pesos vigentes (Configurações › Potencial de mercado). Componente sem dado " +
                "sai da conta e aparece em `componentesAusentes`: ausência de carga não é zero.\n\n" +
                "Padrão: os últimos 12 meses fechados e a categoria TRATOR (como no protótipo); `categoria=TODAS` soma as " +
                "categorias com demanda. Outro período é levado a um ano pela sazonalidade vigente.");

        // A DEMANDA E A PREVISÃO (issue 258) — as mesmas parcelas do motor, distribuídas pelo share-alvo e pela
        // sazonalidade. A mesma porta e o mesmo alcance do Diagnóstico.
        grupo.MapGet("/demanda", async (
                ObterDemandaEPrevisao caso,
                CancellationToken ct,
                string? regiao = null,
                string? lojaCodigo = null,
                string? visao = null,
                string? categoria = null) =>
            (await caso.ExecutarAsync(regiao, lojaCodigo, visao, categoria, ct)).Responder())
            .WithName("ObterDemandaEPrevisao")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("A demanda anual de máquinas da ADR, o que a Tracbel tem de entregar pelo share-alvo, e a previsão por mês e por loja.")
            .WithDescription(
                "O parque e a demanda de cada cultura em cada município são as parcelas do motor do potencial; a ajustada usa " +
                "o fator de ciclo (preço, crédito e percepção). O \"a entregar\" é a demanda × o share-alvo vigente da " +
                "categoria, e a previsão mensal a distribui pela sazonalidade vigente, de novembro a outubro (o ano fiscal).\n\n" +
                "Padrão: a categoria TRATOR; `categoria=TODAS` soma as categorias com demanda, cada uma pelo próprio share.");

        // A GESTÃO DE FINANCIAMENTOS (issue 261) — o SICOR de máquinas com o período, o produto e o programa escolhidos.
        grupo.MapGet("/financiamentos", async (
                ObterFinanciamentosDoSicor caso,
                CancellationToken ct,
                string? de = null,
                string? ate = null,
                string? produto = null,
                string? programa = null,
                string? recorte = null,
                string? lojaCodigo = null,
                string? usina = null) =>
            (await caso.ExecutarAsync(de, ate, produto, programa, recorte, lojaCodigo, usina, ct)).Responder())
            .WithName("ObterFinanciamentosDoSicor")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("O crédito de mecanização do SICOR por município e por loja, no período, produto e programa escolhidos.")
            .WithDescription(
                "Linhas e valor financiado dos produtos de máquina do SICOR (investimento), comparados com o MESMO período do ano " +
                "anterior. O índice é o do CRM (70% linhas, 30% valor) e a situação é a faixa dele — e não os cortes de ±5%/±20% do " +
                "protótipo. LINHA NÃO É CONTRATO: o Banco Central não publica quantidade de contrato.\n\n" +
                "A série vai do primeiro ao último mês do SICOR, para a tela agrupar por mês, trimestre, semestre ou ano. O momento " +
                "traz R3, R6 e R12 terminando no fim do período.\n\n" +
                "Padrão: os 12 meses que terminam no último mês do SICOR descontada a carência, os três produtos de máquina, todos os " +
                "programas, a Região Tracbel (`recorte=adr`). O share da Tracbel contra a concorrência é a issue 262, no painel do crédito.");

        return app;
    }
}
