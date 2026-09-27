using FluentAssertions;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Relacionamento;

/// <summary>
/// O MÊS CORRENTE DA META É O DE SÃO PAULO (revisão do PR #248). O relógio é UTC: das 21h do último dia do mês até a
/// meia-noite UTC, o UTC já virou o mês — e o período padrão (o ano fiscal até o último mês FECHADO) daria como fechado um
/// mês que ainda tem três horas pela frente.
/// </summary>
public sealed class MesCorrenteDaMetaTestes
{
    [Theory]
    [InlineData("2026-10-01T02:30:00Z", 2026, 9)]
    [InlineData("2026-10-01T03:00:00Z", 2026, 10)]
    [InlineData("2026-09-27T09:00:00Z", 2026, 9)]
    [InlineData("2027-01-01T01:00:00Z", 2026, 12)]
    public void O_mes_corrente_e_o_de_Sao_Paulo_e_nao_o_do_UTC(string instanteUtc, int ano, int mes)
    {
        var agora = DateTime.Parse(instanteUtc, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal);

        ObterMetaERealizado.MesCorrenteEmSaoPaulo(agora).Should().Be(new DateOnly(ano, mes, 1));
    }
}
