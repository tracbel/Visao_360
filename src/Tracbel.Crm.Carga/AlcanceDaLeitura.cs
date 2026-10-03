using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Carga;

/// <summary>Curta (a janela dos últimos dias) ou completa (a janela inteira da carga).</summary>
internal enum ModoDaLeitura
{
    /// <summary>Desde o mês em que caem os últimos <see cref="AlcanceDaLeitura.DiasDaJanelaCurta"/> dias.</summary>
    Curta,

    /// <summary>A janela inteira — a conferência que pega o que a origem corrigiu lá atrás.</summary>
    Completa
}

/// <summary>
/// O ALCANCE DA LEITURA DE UMA CARGA INCREMENTAL (documento 54, passo 6; decisão de 03/10/2026: pela data + semanal).
///
/// <para><b>Curta nos dias comuns.</b> A origem não tem data de alteração confirmada (a SD2 e as tabelas da OS não têm
/// <c>S_T_A_M_P_</c>), e a leitura de produção daqui está negada. Mas o que muda está nos últimos dias: a nota emitida, a OS
/// aberta ou fechada, o orçamento alterado. Relendo desde o mês em que caem os últimos três dias, a rodada diária pega isso; os
/// três dias cobrem a rodada que falhou e o fim de semana.</para>
///
/// <para><b>Completa no domingo</b>, ou quando a última completa com sucesso tem sete dias ou mais (o domingo que falhou vira a
/// segunda), ou quando nunca houve uma, ou quando pedida (<c>--completa</c>). É ela que pega o que a origem corrigiu lá atrás, e
/// ela mede quanto corrigiu fora da janela curta — a confirmação de que a janela curta basta.</para>
/// </summary>
/// <param name="Modo">Curta ou completa.</param>
/// <param name="Desde">O primeiro dia lido — o dia 1 de um mês, para nenhum mês chegar pela metade.</param>
/// <param name="InicioDaJanelaCurta">Onde a janela curta começaria hoje — a fronteira da conferência da completa.</param>
/// <param name="Motivo">Por que este modo, para o relatório da carga.</param>
internal sealed record AlcanceDaLeitura(ModoDaLeitura Modo, DateOnly Desde, DateOnly InicioDaJanelaCurta, string Motivo)
{
    /// <summary>Quantos dias a leitura curta cobre.</summary>
    internal const int DiasDaJanelaCurta = 3;

    /// <summary>Depois de quantos dias sem completa a rodada é completa em qualquer dia da semana.</summary>
    internal const int DiasEntreCompletas = 7;

    /// <summary>Decide o alcance da rodada.</summary>
    /// <param name="agoraUtc">O instante da rodada.</param>
    /// <param name="ultimaCompletaUtc">O fim da última leitura completa com sucesso; nulo quando nunca houve.</param>
    /// <param name="completaPedida">Se a linha de comando pediu <c>--completa</c>.</param>
    /// <param name="inicioDaCompleta">O primeiro dia da janela inteira desta carga.</param>
    internal static AlcanceDaLeitura Decidir(DateTime agoraUtc, DateTime? ultimaCompletaUtc, bool completaPedida, DateOnly inicioDaCompleta)
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
            ? new(ModoDaLeitura.Completa, inicioDaCompleta, inicioDaCurta, motivoDaCompleta)
            : new(ModoDaLeitura.Curta, inicioDaCurta, inicioDaCurta,
                $"dia comum: desde o mês em que caem os últimos {DiasDaJanelaCurta} dias");
    }
}
