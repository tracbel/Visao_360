using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Comum;

/// <summary>
/// O ANO FISCAL DA TRACBEL — novembro a outubro, com o nome do ano em que termina (confirmado em 24/09/2026 e
/// tornado o período padrão das telas em 27/09/2026).
///
/// <para><b>Novembro e dezembro são os meses que pegam</b>: neles o ano fiscal vai à frente do civil, e uma
/// conta derivada do civil erra o ano inteiro. Cada borda tem o seu teste.</para>
/// </summary>
[Trait("Categoria", "Comum")]
public sealed class AnoFiscalTestes
{
    [Theory]
    [InlineData(2025, 10, 2025)]
    [InlineData(2025, 11, 2026)]
    [InlineData(2025, 12, 2026)]
    [InlineData(2026, 1, 2026)]
    [InlineData(2026, 8, 2026)]
    [InlineData(2026, 10, 2026)]
    [InlineData(2026, 11, 2027)]
    public void O_ano_fiscal_leva_o_nome_do_ano_em_que_termina(int ano, int mes, int esperado)
    {
        AnoFiscal.Do(new DateOnly(ano, mes, 15)).Should().Be(esperado);
    }

    [Fact]
    public void O_FY2026_vai_de_novembro_de_2025_a_outubro_de_2026()
    {
        var fy = AnoFiscal.Inteiro(2026);

        fy.Inicial.Should().Be(new DateOnly(2025, 11, 1));
        fy.Final.Should().Be(new DateOnly(2026, 10, 1));
        fy.Meses.Should().Be(12);
        AnoFiscal.Nome(2026).Should().Be("FY2026");
    }

    [Theory]
    // SETEMBRO DE 2026 (o dia da decisão): o último mês fechado é agosto, e o ano fiscal começou em nov/2025.
    [InlineData(2026, 9, "2025-11-01", "2026-08-01")]
    // NOVEMBRO: o ano fiscal que acabou de fechar, inteiro — outubro é a última competência dele.
    [InlineData(2026, 11, "2025-11-01", "2026-10-01")]
    // DEZEMBRO: o fiscal já virou, e o recorte é só novembro.
    [InlineData(2026, 12, "2026-11-01", "2026-11-01")]
    // JANEIRO: novembro e dezembro do ano civil anterior — e não "janeiro a dezembro".
    [InlineData(2027, 1, "2026-11-01", "2026-12-01")]
    public void O_ano_fiscal_ate_o_ultimo_mes_fechado_deixa_o_mes_em_curso_de_fora(
        int ano, int mes, string inicial, string final)
    {
        var janela = AnoFiscal.AteOUltimoMesFechado(new DateOnly(ano, mes, 1));

        janela.Inicial.Should().Be(DateOnly.ParseExact(inicial, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
        janela.Final.Should().Be(DateOnly.ParseExact(final, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void O_mesmo_trecho_do_ano_anterior_e_a_janela_inteira_doze_meses_para_tras()
    {
        // A COMPARAÇÃO DA DIRETORIA (27/09/2026): o FY2026 até agosto contra o FY2025 até agosto — e não os
        // dez meses imediatamente anteriores, que misturariam safras.
        var fytd = AnoFiscal.AteOUltimoMesFechado(new DateOnly(2026, 9, 1));
        var anterior = fytd.DoAnoAnterior();

        anterior.Inicial.Should().Be(new DateOnly(2024, 11, 1));
        anterior.Final.Should().Be(new DateOnly(2025, 8, 1));
        anterior.Meses.Should().Be(fytd.Meses, "os dois lados têm o mesmo número de meses");
        anterior.Texto.Should().Be("nov/2024 a ago/2025");
    }

    [Fact]
    public void As_22h_de_31_de_outubro_em_Sao_Paulo_ainda_e_outubro_e_o_ano_fiscal_nao_virou()
    {
        // 31/10/2026 às 22:00 em São Paulo é 01/11/2026 às 01:00 em UTC. Derivar o mês do UTC viraria o ano
        // fiscal três horas antes da hora.
        var agoraUtc = new DateTime(2026, 11, 1, 1, 0, 0, DateTimeKind.Utc);

        var mes = AnoFiscal.MesCorrenteEmSaoPaulo(agoraUtc);
        mes.Should().Be(new DateOnly(2026, 10, 1));

        var janela = AnoFiscal.AteOUltimoMesFechado(mes);
        janela.Inicial.Should().Be(new DateOnly(2025, 11, 1));
        janela.Final.Should().Be(new DateOnly(2026, 9, 1), "outubro ainda está em curso");
    }

    [Fact]
    public void A_meia_noite_de_1o_de_novembro_em_Sao_Paulo_o_ano_fiscal_que_fechou_vem_inteiro()
    {
        // 01/11/2026 às 00:00 em São Paulo = 03:00 UTC. O último mês fechado é outubro: o FY2026 inteiro.
        var mes = AnoFiscal.MesCorrenteEmSaoPaulo(new DateTime(2026, 11, 1, 3, 0, 0, DateTimeKind.Utc));

        mes.Should().Be(new DateOnly(2026, 11, 1));
        AnoFiscal.AteOUltimoMesFechado(mes).Should().Be(AnoFiscal.Inteiro(2026));
    }

    [Fact]
    public void A_janela_contem_qualquer_dia_do_mes_e_so_os_meses_dela()
    {
        var janela = new JanelaDeCompetencia(new DateOnly(2025, 11, 1), new DateOnly(2026, 8, 1));

        janela.Contem(new DateOnly(2025, 11, 30)).Should().BeTrue();
        janela.Contem(new DateOnly(2026, 8, 31)).Should().BeTrue("o último mês entra inteiro");
        janela.Contem(new DateOnly(2025, 10, 31)).Should().BeFalse();
        janela.Contem(new DateOnly(2026, 9, 1)).Should().BeFalse();
    }
}
