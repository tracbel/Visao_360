using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// A PERCEPÇÃO DE CAMPO POR CULTURA (decisão do Ricardo em 27/09/2026): a nota da planilha, de −2 a +2, na escala da
/// D-P04 — cada ponto vale 2,5 pontos percentuais —, somada ao ajuste do gestor sobre o município.
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class PercepcaoDaCulturaTestes
{
    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private static PercepcaoDaCultura Nota(decimal nota) =>
        PercepcaoDaCultura.Informar(1, nota, new DateOnly(2026, 9, 27), "planilha, aba Administrador", 100, Agora);

    [Theory]
    [InlineData(1, 2.5)]
    [InlineData(-1, -2.5)]
    [InlineData(-2, -5)]
    [InlineData(-1.5, -3.75)]
    [InlineData(0, 0)]
    public void A_nota_vira_ponto_percentual_na_escala_da_D_P04(decimal nota, decimal percentual) =>
        Nota(nota).Percentual.Should().Be(percentual, "±2 é ±5%, o teto da D-P04 — e não os ±40% da conta da planilha");

    [Theory]
    [InlineData(2.01)]
    [InlineData(-3)]
    public void Nota_fora_da_regua_da_planilha_e_recusada(decimal nota)
    {
        var fora = () => Nota(nota);
        fora.Should().Throw<RegraDeNegocioViolada>().WithMessage("*−2 a +2*");
    }

    [Fact]
    public void A_percepcao_que_entra_no_fator_e_a_da_cultura_mais_a_do_municipio()
    {
        var recorte = new IndicadoresDoRecorte(
            new Dictionary<string, IndiceDeMomento>(), null, PercepcaoDoGestor: 2m, UltimoMesDePreco: null,
            PercepcaoPorCultura: new Dictionary<string, decimal> { ["CAFE"] = 2.5m, ["AMENDOIM"] = -5m });

        recorte.PercepcaoDaCulturaNoRecorte("CAFE").Should().Be(4.5m);
        recorte.PercepcaoDaCulturaNoRecorte("AMENDOIM").Should().Be(-3m);
        recorte.PercepcaoDaCulturaNoRecorte("SOJA").Should().Be(2m, "sem nota de campo, fica só o ajuste do município");
    }

    [Fact]
    public void Sem_nenhuma_das_duas_a_percepcao_e_nula_e_nao_zero_declarado()
    {
        var recorte = new IndicadoresDoRecorte(new Dictionary<string, IndiceDeMomento>(), null, null, null);

        recorte.PercepcaoDaCulturaNoRecorte("CAFE").Should().BeNull();
    }
}
