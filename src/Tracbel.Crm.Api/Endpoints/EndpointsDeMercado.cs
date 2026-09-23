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

        return app;
    }
}
