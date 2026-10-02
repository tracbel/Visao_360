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
                string? categoria = null,
                string? anoFiscal = null,
                string? cultura = null) =>
            (await caso.ExecutarAsync(regiao, lojaCodigo, visao, categoria, ct, anoFiscal, cultura)).Responder())
            .WithName("ObterDemandaEPrevisao")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("A demanda anual de máquinas da ADR, o que a Tracbel tem de entregar pelo share-alvo, e a previsão por mês e por loja.")
            .WithDescription(
                "O parque e a demanda de cada cultura em cada município são as parcelas do motor do potencial; a ajustada usa " +
                "o fator de ciclo (preço, crédito e percepção). O \"a entregar\" é a demanda × o share-alvo vigente da " +
                "categoria, e a previsão mensal a distribui pela sazonalidade vigente, de novembro a outubro (o ano fiscal).\n\n" +
                "Padrão: a categoria TRATOR; `categoria=TODAS` soma as categorias com demanda, cada uma pelo próprio share.\n\n" +
                "Desde 02/10/2026: `anoFiscal` (padrão, o corrente) escolhe o ano das ENTREGAS do ART, pela data da entrega, mês a " +
                "mês e no mesmo trecho do ano anterior; `cultura` recorta a demanda (e deixa a entrega de fora, porque a máquina " +
                "não diz a cultura); e cada número traz a mesma conta com a área do ano anterior da PAM.");

        // O DIMENSIONAMENTO DA ADR (issue 259) — a PAM do estado inteiro e a carteira, com o mesmo alcance do Diagnóstico.
        grupo.MapGet("/dimensionamento", async (
                ObterDimensionamentoDaAdr caso,
                CancellationToken ct,
                string? anoBase = null,
                string? cultura = null,
                string? regiao = null,
                string? lojaCodigo = null,
                string? visao = null,
                string? responsavel = null,
                string? classe = null,
                string? usina = null) =>
            (await caso.ExecutarAsync(anoBase, cultura, regiao, lojaCodigo, visao, responsavel, classe, usina, ct)).Responder())
            .WithName("ObterDimensionamentoDaAdr")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("O tamanho da ADR dentro de São Paulo, o perfil de cada loja, a matriz municipal e a carteira que cobre o território.")
            .WithDescription(
                "Área plantada, quantidade e valor da produção são os da PAM do IBGE (tabela 5457), no ano-base e no anterior: o " +
                "recorte é a soma dos municípios, e São Paulo é o total que o IBGE publica para o estado — sem o total carregado, a " +
                "fatia sai vazia com o motivo. A cultura de cada produto vem do catálogo; o resto vai para `OUTRAS`. A quantidade " +
                "soma só o que é publicado em toneladas.\n\n" +
                "A carteira é a dos Indicadores (vínculo em carteira comercial, município do endereço principal, classe da curva ABC), " +
                "em faixas ACUMULADAS de dias desde o último contato (< 30, 60, 90 e 120). Com região, loja ou usina, ela conta só os " +
                "clientes dos municípios do recorte; com o CEN, o território é o dos municípios onde ele tem cliente.\n\n" +
                "Padrão: o ano mais recente da PAM, todas as culturas, a ADR inteira.");

        // O PREÇO DE COMMODITIES (issue 260) — os quatro horizontes do momento de preço de cada cultura e a série.
        grupo.MapGet("/precos", async (ObterPrecosDasCulturas caso, CancellationToken ct) => (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterPrecosDasCulturas")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("A variação do preço de cada cultura no mês, em R3, R6 e R12, com as duas médias e a série histórica.")
            .WithDescription(
                "Cada horizonte é a média dos últimos N meses sobre a dos N anteriores, com as duas janelas cheias — a conta do " +
                "momento de preço do CRM. O R12 é o PRÓPRIO momento dos Indicadores, inclusive o preço anual da PAM onde a série " +
                "mensal ainda não fecha 24 meses (`serie = AnualPam`). As faixas são as do CRM, e não os cortes do protótipo.\n\n" +
                "A série de cada cultura é a do índice quando ela declara uma (a cana pelo ATR mensal da Socicana), e senão a do " +
                "preço (CONAB), na unidade em que a fonte publica.");
        return app;
    }
}
