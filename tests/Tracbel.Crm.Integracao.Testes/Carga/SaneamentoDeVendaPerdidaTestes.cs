using FluentAssertions;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Saneamento;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Carga;

/// <summary>
/// A QUANTIDADE DA VENDA PERDIDA: o '9999999999' da origem estourava o inteiro e derrubava a rodada inteira por uma
/// resposta. Acima de mil máquinas é digitação — entra como não declarada, com a correção anotada.
/// </summary>
public sealed class SaneamentoDeVendaPerdidaTestes
{
    [Theory]
    [InlineData(9_999_999_999)]
    [InlineData(1_001)]
    public void A_quantidade_absurda_vira_uma_maquina_com_o_motivo_anotado(double bruta)
    {
        var correcoes = new List<CorrecaoAplicada>();

        SaneamentoDeVendaPerdida.Quantidade((decimal)bruta, correcoes).Should().Be(1);
        correcoes.Should().ContainSingle().Which.Motivo.Should().Contain("digitação");
    }

    [Fact]
    public void A_quantidade_plausivel_entra_como_veio_e_o_vazio_vira_uma()
    {
        var correcoes = new List<CorrecaoAplicada>();

        SaneamentoDeVendaPerdida.Quantidade(SaneamentoDeVendaPerdida.MaiorQuantidadePlausivel, correcoes).Should().Be(1_000);
        SaneamentoDeVendaPerdida.Quantidade(3.7m, correcoes).Should().Be(3);
        correcoes.Should().BeEmpty();

        SaneamentoDeVendaPerdida.Quantidade(null, correcoes).Should().Be(1);
        correcoes.Should().ContainSingle();
    }
}
