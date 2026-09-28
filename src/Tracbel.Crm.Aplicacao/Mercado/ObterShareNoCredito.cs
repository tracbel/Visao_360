using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>
/// O SHARE DA TRACBEL NO CRÉDITO DE MECANIZAÇÃO (issue 262, decisões do Ricardo em 28/09/2026): o valor que a Tracbel
/// financiou em crédito rural, dos formulários da venda do Vórtice, dividido pelo crédito de máquinas do SICOR, por filial e
/// na Região, na janela do painel do crédito.
///
/// <para><b>A janela é a do crédito</b>: o mesmo parâmetro com vigência (meses e carência) que o painel do SICOR usa, para os
/// dois painéis falarem dos mesmos meses.</para>
/// </summary>
public sealed class ObterShareNoCredito(
    IRepositorioDoShareNoCredito repositorio, IRepositorioDeParametrosDoPotencial parametros, IRelogio relogio)
{
    private const short MesesDaJanelaDoTextoBase = 12;

    /// <summary>Lê o share.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<ShareNoCreditoDeMecanizacao>>> ExecutarAsync(CancellationToken ct)
    {
        var vigente = ParametroComVigencia.VigenteEm(
            await parametros.ListarGeraisAsync(ct), ParametroComVigencia.HojeNoBrasil(relogio.Agora));

        var share = await repositorio.LerAsync(
            vigente?.MesesDaJanela ?? MesesDaJanelaDoTextoBase, vigente?.MesesDeCarenciaDoSicor, ct);

        var procedencia = new ProcedenciaDoIndicador(
            "Vórtice (formulários da venda) ÷ BCB/SICOR",
            "Financiamento das vendas × crédito rural de investimento",
            "organizacao.FinanciamentoDaVenda ÷ InvestMunicipioProduto",
            "Valor financiado em crédito rural ÷ valor contratado em trator, máquinas e implementos e colheitadeiras",
            share.Inicio is { } inicio && share.Fim is { } fim ? $"{inicio:MM/yyyy} a {fim:MM/yyyy}" : null,
            relogio.Agora.ToUniversalTime(),
            "ESTIMATIVA. O numerador é o pedido de venda, no município do cadastro do cliente; o SICOR registra a cédula, no " +
            "município do empreendimento, e não identifica revenda. Por filial e em 12 meses as diferenças se diluem; por " +
            "município não, e o município em que a Tracbel passa do SICOR vem marcado. Entram todas as linhas menos recurso " +
            "próprio e consórcio (decisão de 28/09/2026).");

        return Resultado<ComProcedencia<ShareNoCreditoDeMecanizacao>>.Ok(
            ComProcedencia<ShareNoCreditoDeMecanizacao>.DoNossoBanco(
                share with { Procedencia = procedencia },
                "organizacao.FinanciamentoDaVenda + organizacao.CreditoRuralDeInvestimento + organizacao.MunicipioDaAreaDeAtuacao",
                relogio));
    }
}
