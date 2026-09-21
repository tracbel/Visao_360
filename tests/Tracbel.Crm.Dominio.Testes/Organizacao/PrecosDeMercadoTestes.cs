using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// OS PREÇOS DE MERCADO (issue 66) — a série que só cresce e só muda com registro.
/// </summary>
public sealed class PrecosDeMercadoTestes
{
    private static readonly DateTime Agora = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

    private static CotacaoDeProduto Soja(decimal valor = 2.16m) =>
        CotacaoDeProduto.Registrar(
            "CONAB", "4744", "SP", "RECEBIDO PELO PRODUTOR", "SOJA", "EM GRÃOS", "kg",
            new DateOnly(2026, 8, 17), valor, importadoPorId: 1, Agora);

    [Fact]
    public void O_mes_e_gravado_no_dia_1() =>
        Soja().Mes.Should().Be(new DateOnly(2026, 8, 1));

    [Theory]
    [InlineData(0)]
    [InlineData(-2.16)]
    public void Preco_zero_ou_negativo_e_falta_de_dado_e_nao_entra(decimal valor)
    {
        var registrar = () => Soja(valor);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Revisar_com_o_mesmo_valor_nao_muda_nada_nem_o_carimbo()
    {
        var soja = Soja();
        var depois = Agora.AddMonths(1);

        soja.Revisar("SOJA", "EM GRÃOS", 2.16m, importadoPorId: 2, depois).Should().BeFalse();

        soja.ImportadoEm.Should().Be(Agora, "recarimbar o que não mudou é o erro que encheu o log do Vórtice");
        soja.ImportadoPorId.Should().Be(1);
    }

    [Fact]
    public void Revisao_da_fonte_muda_o_valor_e_o_carimbo()
    {
        var soja = Soja();
        var depois = Agora.AddMonths(1);

        soja.Revisar("SOJA", "EM GRÃOS", 2.21m, importadoPorId: 2, depois).Should().BeTrue();

        soja.ValorEmReais.Should().Be(2.21m);
        soja.ImportadoEm.Should().Be(depois);
        soja.ImportadoPorId.Should().Be(2);
    }

    [Fact]
    public void Correcao_de_grafia_da_fonte_acompanha_sem_contar_como_revisao_de_preco()
    {
        var soja = Soja();

        soja.Revisar("SOJA", "EM GRAOS", 2.16m, importadoPorId: 2, Agora.AddDays(1)).Should().BeFalse();

        soja.Classificacao.Should().Be("EM GRAOS");
    }

    [Fact]
    public void Cotacao_sem_fonte_ou_sem_produto_nao_entra()
    {
        var semFonte = () => CotacaoDeProduto.Registrar(
            " ", "4744", "SP", "RECEBIDO", "SOJA", "", "kg", new DateOnly(2026, 8, 1), 2m, 1, Agora);
        var semProduto = () => CotacaoDeProduto.Registrar(
            "CONAB", "4744", "SP", "RECEBIDO", "", "", "kg", new DateOnly(2026, 8, 1), 2m, 1, Agora);

        semFonte.Should().Throw<RegraDeNegocioViolada>().WithMessage("*fonte*");
        semProduto.Should().Throw<RegraDeNegocioViolada>().WithMessage("*produto*");
    }

    [Fact]
    public void O_dolar_do_mes_so_muda_quando_o_Banco_Central_revisa()
    {
        var dolar = CotacaoDoDolar.Registrar(new DateOnly(2026, 8, 31), 5.1532m, 1, Agora);

        dolar.Mes.Should().Be(new DateOnly(2026, 8, 1));
        dolar.Revisar(5.1532m, 2, Agora.AddDays(1)).Should().BeFalse();
        dolar.Revisar(5.1600m, 2, Agora.AddDays(2)).Should().BeTrue();
        dolar.ReaisPorDolar.Should().Be(5.16m);
    }

    [Fact]
    public void Dolar_zero_nao_entra()
    {
        var registrar = () => CotacaoDoDolar.Registrar(new DateOnly(2026, 8, 1), 0m, 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }
}
