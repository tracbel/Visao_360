using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
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
public sealed class ObterCreditoRural(
    IRepositorioDeCreditoRural repositorio, IRepositorioDeParametrosDoPotencial parametros, IRelogio relogio)
{
    /// <summary>
    /// A janela quando ainda não há vigência de parâmetro no banco: 12 contra 12, o do texto-base, e
    /// carência não decidida. Nenhum mês é descartado enquanto ninguém decidir quantos.
    /// </summary>
    private const short MesesDaJanelaDoTextoBase = 12;

    /// <summary>Lê o painel.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelDeCreditoRural>>> ExecutarAsync(CancellationToken ct)
    {
        // A VIGÊNCIA É ESCOLHIDA AQUI, e não na consulta: qual parâmetro vale hoje é regra do domínio
        // (ParametroComVigencia.VigenteEm), e o repositório de crédito é uma consulta, não um juiz.
        var vigente = ParametroComVigencia.VigenteEm(
            await parametros.ListarGeraisAsync(ct), ParametroComVigencia.HojeNoBrasil(relogio.Agora));

        var painel = await repositorio.LerAsync(
            vigente?.MesesDaJanela ?? MesesDaJanelaDoTextoBase, vigente?.MesesDeCarenciaDoSicor, vigente, ct);

        return Resultado<ComProcedencia<PainelDeCreditoRural>>.Ok(
            ComProcedencia<PainelDeCreditoRural>.DoNossoBanco(
                painel,
                "organizacao.CreditoRuralDeInvestimento + organizacao.ItemDoSicor + organizacao.ParametroDoPotencial",
                relogio));
    }
}
