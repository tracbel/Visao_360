using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// A UNIDADE DE CADA PRODUTO DA PAM E A PRODUTIVIDADE (issue 152).
///
/// <para>O teste de ouro usa os números que o IBGE publica para São Paulo em 2024 (SIDRA 5457, conferidos em
/// 22/09/2026): quantidade produzida (variável 214), área colhida (216) e o rendimento médio que o próprio IBGE
/// calcula (112, em kg/ha). A produtividade do CRM, em t/ha, vezes mil, tem de bater com o rendimento ao quilo.</para>
/// </summary>
public sealed class UnidadesDaPamTestes
{
    [Theory]
    [InlineData(40139, 335_310, 190_255, 1_762)]         // café (em grão) total
    [InlineData(40106, 418_569_112, 5_398_676, 77_532)]  // cana-de-açúcar
    [InlineData(40124, 3_626_440, 1_355_595, 2_675)]     // soja
    [InlineData(40122, 3_657_604, 723_128, 5_058)]       // milho
    [InlineData(40151, 12_002_362, 354_562, 33_851)]     // laranja
    public void A_produtividade_bate_com_o_rendimento_medio_que_o_ibge_publica(int produto, int quantidade, int colhida, int rendimentoKgPorHa)
    {
        var produtividade = UnidadesDaPam.Produtividade(quantidade, colhida);

        UnidadesDaPam.DaQuantidade(produto, 2024).DaProdutividade.Should().Be("t/ha");
        decimal.Round(produtividade!.Value * 1000m, 0, MidpointRounding.AwayFromZero).Should().Be(rendimentoKgPorHa,
            "é quantidade sobre área COLHIDA, a regra da variável 112 — e não sobre a plantada, como fazia a planilha");
    }

    [Fact]
    public void Abacaxi_e_coco_sao_mil_frutos_em_qualquer_ano()
    {
        // Nota 6 da tabela 5457. O campo de unidade do SIDRA diz "Toneladas" para eles, porque é a unidade da variável.
        UnidadesDaPam.DaQuantidade(40092, 2024).Should().Be(new UnidadeDaQuantidade("mil frutos", "mil frutos/ha", false));
        UnidadesDaPam.DaQuantidade(40145, 1990).Nome.Should().Be("mil frutos");
    }

    [Fact]
    public void As_frutas_da_nota_2_so_viram_tonelada_em_2001()
    {
        UnidadesDaPam.DaQuantidade(40151, 2000).Nome.Should().Be("mil frutos", "laranja antes de 2001");
        UnidadesDaPam.DaQuantidade(40151, 2001).Nome.Should().Be("toneladas");
        UnidadesDaPam.DaQuantidade(40136, 2000).Should().Be(new UnidadeDaQuantidade("mil cachos", "mil cachos/ha", false), "banana antes de 2001");
        UnidadesDaPam.DaQuantidade(40136, 2001).EhMassa.Should().BeTrue();
    }

    [Fact]
    public void Grao_e_tonelada_em_qualquer_ano()
    {
        UnidadesDaPam.DaQuantidade(40124, 1990).Should().Be(new UnidadeDaQuantidade("toneladas", "t/ha", true));
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData(100, null)]
    [InlineData(100, 0)]
    public void Sem_quantidade_ou_sem_colheita_nao_ha_produtividade(int? quantidade, int? colhida) =>
        UnidadesDaPam.Produtividade(quantidade, colhida).Should().BeNull("dividir por área colhida zero não é produtividade zero");

    [Fact]
    public void Colheu_e_nao_produziu_e_produtividade_zero() =>
        UnidadesDaPam.Produtividade(0m, 50m).Should().Be(0m, "frustração total de safra é medida, não falta de dado");
}
