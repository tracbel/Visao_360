using FluentAssertions;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// O PERÍODO DO SICOR da Gestão de Financiamentos (issue 261): os meses contados com as duas pontas, o mesmo período do ano
/// anterior e os últimos N meses que terminam no fim — a base de R3, R6 e R12.
/// </summary>
public sealed class PeriodoDoSicorTestes
{
    [Fact]
    public void Conta_os_meses_com_as_duas_pontas_mesmo_virando_o_ano()
    {
        new PeriodoDoSicor(new DateOnly(2025, 9, 1), new DateOnly(2026, 8, 1)).Meses.Should().Be(12);
        new PeriodoDoSicor(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 1)).Meses.Should().Be(1);
    }

    [Fact]
    public void O_ano_anterior_e_o_mesmo_intervalo_doze_meses_antes()
    {
        var anterior = new PeriodoDoSicor(new DateOnly(2026, 1, 1), new DateOnly(2026, 8, 1)).DoAnoAnterior();

        anterior.Should().Be(new PeriodoDoSicor(new DateOnly(2025, 1, 1), new DateOnly(2025, 8, 1)));
    }

    [Fact]
    public void Os_ultimos_meses_terminam_no_fim_do_periodo()
    {
        var periodo = new PeriodoDoSicor(new DateOnly(2025, 9, 1), new DateOnly(2026, 8, 1));

        periodo.UltimosMeses(3).Should().Be(new PeriodoDoSicor(new DateOnly(2026, 6, 1), new DateOnly(2026, 8, 1)));
        periodo.UltimosMeses(12).Should().Be(periodo);
    }
}
