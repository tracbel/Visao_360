using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Carga;

/// <summary>Curta (a janela dos últimos dias) ou completa (os 36 meses).</summary>
internal enum ModoDaLeituraDoFaturamento
{
    /// <summary>O mês em que caem os últimos <see cref="AlcanceDaLeituraDoFaturamento.DiasDaJanelaCurta"/> dias de emissão.</summary>
    Curta,

    /// <summary>Os 36 meses — a conferência que pega o que a origem corrigiu lá atrás.</summary>
    Completa
}

/// <summary>
/// O ALCANCE DA LEITURA DO FATURAMENTO (documento 54, passo 6; decisão de 03/10/2026: pela emissão + semanal).
///
/// <para><b>Curta nos dias comuns, pela EMISSÃO.</b> A SD2 não tem data de alteração confirmada, e a leitura de produção daqui
/// está negada. Mas nota nova nasce com a emissão de hoje, e o cancelamento da NF-e tem prazo curto: relendo o mês em que caem
/// os últimos três dias, a rodada diária pega a venda nova e o cancelamento do dia. Os três dias cobrem a rodada que falhou e o
/// fim de semana.</para>
///
/// <para><b>Completa no domingo</b>, ou quando a última completa com sucesso tem sete dias ou mais (o domingo que falhou vira a
/// segunda), ou quando nunca houve uma, ou quando pedida (<c>--completa</c>). É ela que pega o que a origem corrigiu lá atrás, e
/// ela mede quanto corrigiu fora da janela curta — a confirmação de que a janela curta basta.</para>
/// </summary>
/// <param name="Modo">Curta ou completa.</param>
/// <param name="Desde">O primeiro dia lido — o dia 1 de um mês, para nenhum mês chegar pela metade.</param>
/// <param name="InicioDaJanelaCurta">Onde a janela curta começaria hoje — a fronteira da conferência da completa.</param>
/// <param name="Motivo">Por que este modo, para o relatório da carga.</param>
internal sealed record AlcanceDaLeituraDoFaturamento(
    ModoDaLeituraDoFaturamento Modo, DateOnly Desde, DateOnly InicioDaJanelaCurta, string Motivo)
{
    /// <summary>Quantos dias de emissão a leitura curta cobre.</summary>
    internal const int DiasDaJanelaCurta = 3;

    /// <summary>Depois de quantos dias sem completa a rodada é completa em qualquer dia da semana.</summary>
    internal const int DiasEntreCompletas = 7;

    /// <summary>Decide o alcance da rodada.</summary>
    /// <param name="agoraUtc">O instante da rodada.</param>
    /// <param name="ultimaCompletaUtc">O fim da última leitura completa com sucesso; nulo quando nunca houve.</param>
    /// <param name="completaPedida">Se a linha de comando pediu <c>--completa</c>.</param>
    internal static AlcanceDaLeituraDoFaturamento Decidir(DateTime agoraUtc, DateTime? ultimaCompletaUtc, bool completaPedida)
    {
        var hoje = ParametroComVigencia.HojeNoBrasil(agoraUtc);
        var tresDiasAtras = hoje.AddDays(-DiasDaJanelaCurta);
        var inicioDaCurta = new DateOnly(tresDiasAtras.Year, tresDiasAtras.Month, 1);

        var motivoDaCompleta =
            completaPedida ? "pedida na linha de comando (--completa)"
            : ultimaCompletaUtc is null ? "nunca houve leitura completa registrada"
            : hoje.DayOfWeek == DayOfWeek.Sunday ? "domingo, a conferência semanal"
            : agoraUtc - ultimaCompletaUtc.Value >= TimeSpan.FromDays(DiasEntreCompletas)
                ? $"a última completa tem {DiasEntreCompletas} dias ou mais"
                : null;

        return motivoDaCompleta is not null
            ? new(ModoDaLeituraDoFaturamento.Completa, CargaDeFaturamentoDoProtheus.InicioDaJanela(agoraUtc), inicioDaCurta, motivoDaCompleta)
            : new(ModoDaLeituraDoFaturamento.Curta, inicioDaCurta, inicioDaCurta,
                $"dia comum: o mês em que caem os últimos {DiasDaJanelaCurta} dias de emissão");
    }
}
