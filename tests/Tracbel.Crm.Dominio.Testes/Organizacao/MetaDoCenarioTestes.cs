using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// A meta escolhida nos Cenários de mercado (issue 263): só o manual leva número digitado, e ele é a própria meta; mudar de
/// ideia altera a linha e carimba quem mudou; repetir a mesma escolha não é alteração.
/// </summary>
public sealed class MetaDoCenarioTestes
{
    private static MetaDoCenarioNoMunicipio Moderado(decimal meta = 2.79m) =>
        MetaDoCenarioNoMunicipio.Escolher(10, 1, 2026, CenarioDeMercado.Moderado, null, meta, 100);

    [Fact]
    public void O_cenario_grava_o_numero_do_mercado_de_hoje_e_quem_escolheu()
    {
        var meta = Moderado(2.7856m);

        meta.Cenario.Should().Be(CenarioDeMercado.Moderado);
        meta.MetaNaEscolha.Should().Be(2.79m, "duas casas, como a tela mostra");
        meta.ValorManual.Should().BeNull();
        meta.CriadoPorId.Should().Be(100);
        meta.AlteradoEm.Should().BeNull();
    }

    [Fact]
    public void O_manual_e_o_proprio_numero_digitado()
    {
        var meta = MetaDoCenarioNoMunicipio.Escolher(10, 1, 2026, CenarioDeMercado.Manual, 5m, 99m, 100);

        meta.ValorManual.Should().Be(5m);
        meta.MetaNaEscolha.Should().Be(5m, "no manual a meta do cenário não conta — vale o número de quem digitou");
    }

    [Fact]
    public void Mudar_de_ideia_altera_a_mesma_meta_e_repetir_nao_e_alteracao()
    {
        var meta = Moderado();

        meta.Alterar(CenarioDeMercado.Moderado, null, 2.79m, 200).Should().BeFalse("a mesma escolha não muda nada");
        meta.AlteradoPorId.Should().BeNull();

        meta.Alterar(CenarioDeMercado.Otimista, null, 2.93m, 200).Should().BeTrue();
        meta.Cenario.Should().Be(CenarioDeMercado.Otimista);
        meta.AlteradoPorId.Should().Be(200);
    }

    [Theory]
    [InlineData(CenarioDeMercado.Manual, null, "precisa do número")]
    [InlineData(CenarioDeMercado.Manual, -1, "não é negativa")]
    [InlineData(CenarioDeMercado.Manual, 100_001, "100 mil")]
    [InlineData(CenarioDeMercado.Moderado, 3, "Só o cenário manual")]
    public void A_combinacao_que_nao_vale_e_recusada(CenarioDeMercado cenario, int? valor, string motivo)
    {
        var escolher = () => MetaDoCenarioNoMunicipio.Escolher(10, 1, 2026, cenario, valor, 1m, 100);

        escolher.Should().Throw<RegraDeNegocioViolada>().WithMessage($"*{motivo}*");
    }

    [Theory]
    [InlineData(0, 1, 2026)]
    [InlineData(10, 0, 2026)]
    [InlineData(10, 1, 1999)]
    public void A_meta_e_de_um_municipio_uma_categoria_e_um_ano_fiscal(int municipio, int categoria, short ano)
    {
        var escolher = () => MetaDoCenarioNoMunicipio.Escolher(municipio, categoria, ano, CenarioDeMercado.Moderado, null, 1m, 100);

        escolher.Should().Throw<RegraDeNegocioViolada>();
    }
}
