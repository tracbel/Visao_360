using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Territorio;

/// <summary>
/// O CRÉDITO RURAL DE INVESTIMENTO EM SÃO PAULO — o SICOR do Banco Central (issue 68).
///
/// <para>O texto-base: "a nível município vai trazer todos os financiamentos contratados, o nome do
/// produto"; "pegar os últimos 12 meses e dividir pelos meses anteriores". Esta rota entrega as duas
/// janelas lado a lado, por produto e por município; o índice ponderado e as faixas ("mercado quente,
/// morno ou frio") são da issue 73.</para>
///
/// <para>É dado público, o mesmo para todas as filiais — não passa pela fronteira de multiempresa.</para>
/// </summary>
public sealed class ObterCreditoRural(IRepositorioDeCreditoRural repositorio, IRelogio relogio)
{
    /// <summary>Lê o painel.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelDeCreditoRural>>> ExecutarAsync(CancellationToken ct) =>
        Resultado<ComProcedencia<PainelDeCreditoRural>>.Ok(
            ComProcedencia<PainelDeCreditoRural>.DoNossoBanco(
                await repositorio.LerAsync(ct), "organizacao.CreditoRuralDeInvestimento + organizacao.ItemDoSicor", relogio));
}
