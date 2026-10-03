using FluentAssertions;
using Tracbel.Crm.Carga;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O ALCANCE DA LEITURA DO FATURAMENTO (documento 54, passo 6): curta nos dias comuns — o mês em que caem os últimos três dias
/// de emissão —, completa no domingo, quando a última completa tem sete dias ou mais, quando nunca houve uma, ou quando pedida.
/// </summary>
public sealed class AlcanceDaLeituraDoFaturamentoTestes
{
    // 05:00 em São Paulo = 08:00 UTC, a hora da rotina.
    private static DateTime As5h(int ano, int mes, int dia) => new(ano, mes, dia, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void No_dia_comum_com_completa_recente_a_leitura_e_curta_e_comeca_no_mes_dos_ultimos_tres_dias()
    {
        // Sexta, 09/10/2026; a completa foi no domingo, 04/10.
        var alcance = AlcanceDaLeituraDoFaturamento.Decidir(As5h(2026, 10, 9), As5h(2026, 10, 4), completaPedida: false);

        (alcance.Modo, alcance.Desde, alcance.InicioDaJanelaCurta)
            .Should().Be((ModoDaLeituraDoFaturamento.Curta, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 1)));
    }

    [Fact]
    public void Nos_tres_primeiros_dias_do_mes_a_janela_curta_comeca_no_mes_anterior()
    {
        // Sexta, 02/10/2026: três dias atrás é 29/09 — a nota do fim de setembro cancelada hoje está dentro.
        var alcance = AlcanceDaLeituraDoFaturamento.Decidir(As5h(2026, 10, 2), As5h(2026, 9, 27), completaPedida: false);

        (alcance.Modo, alcance.Desde).Should().Be((ModoDaLeituraDoFaturamento.Curta, new DateOnly(2026, 9, 1)));
    }

    [Fact]
    public void No_domingo_a_leitura_e_completa_desde_36_meses()
    {
        var alcance = AlcanceDaLeituraDoFaturamento.Decidir(As5h(2026, 10, 11), As5h(2026, 10, 4), completaPedida: false);

        (alcance.Modo, alcance.Desde, alcance.InicioDaJanelaCurta)
            .Should().Be((ModoDaLeituraDoFaturamento.Completa, new DateOnly(2023, 10, 1), new DateOnly(2026, 10, 1)));
    }

    [Theory]
    [InlineData(null, "nunca houve")]
    [InlineData(7, "sete dias ou mais")]
    public void Sem_completa_recente_a_leitura_e_completa_mesmo_em_dia_comum(int? diasDesdeACompleta, string motivo)
    {
        var agora = As5h(2026, 10, 13); // terça
        var alcance = AlcanceDaLeituraDoFaturamento.Decidir(
            agora, diasDesdeACompleta is { } d ? agora.AddDays(-d) : null, completaPedida: false);

        alcance.Modo.Should().Be(ModoDaLeituraDoFaturamento.Completa, motivo);
    }

    [Fact]
    public void Seis_dias_depois_da_completa_o_dia_comum_ainda_e_curto()
    {
        var agora = As5h(2026, 10, 10); // sábado
        AlcanceDaLeituraDoFaturamento.Decidir(agora, agora.AddDays(-6), completaPedida: false)
            .Modo.Should().Be(ModoDaLeituraDoFaturamento.Curta);
    }

    [Fact]
    public void A_completa_pedida_vale_em_qualquer_dia()
    {
        AlcanceDaLeituraDoFaturamento.Decidir(As5h(2026, 10, 9), As5h(2026, 10, 8), completaPedida: true)
            .Modo.Should().Be(ModoDaLeituraDoFaturamento.Completa);
    }
}
