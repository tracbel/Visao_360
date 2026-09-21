using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>O CRÉDITO RURAL DO SICOR (issue 68) — a linha e o catálogo.</summary>
public sealed class CreditoRuralTestes
{
    private static readonly DateTime Agora = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

    private static readonly CreditoRuralDeInvestimento.Chave Trator =
        new(11790, 2025, 2, 7080, 154, 71, 431, 9, 1, 14);

    [Fact]
    public void Registra_a_linha_com_a_chave_inteira()
    {
        var c = CreditoRuralDeInvestimento.Registrar(Trator, 10, 1_000_000m, 0m, 1, Agora);

        c.ChaveNatural.Should().Be(Trator);
        c.Valor.Should().Be(1_000_000m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Valor_zero_ou_negativo_nao_e_credito(decimal valor)
    {
        var registrar = () => CreditoRuralDeInvestimento.Registrar(Trator, 10, valor, 0m, 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Mes_fora_de_1_a_12_nao_entra()
    {
        var registrar = () => CreditoRuralDeInvestimento.Registrar(Trator with { Mes = 13 }, 10, 1m, 0m, 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Registro_atrasado_do_Banco_Central_revisa_o_valor_e_o_mesmo_valor_nao_mexe_no_carimbo()
    {
        var c = CreditoRuralDeInvestimento.Registrar(Trator, 10, 1_000_000m, 0m, 1, Agora);

        c.Revisar(1_000_000m, 0m, 2, Agora.AddMonths(1)).Should().BeFalse();
        c.ImportadoEm.Should().Be(Agora);

        c.Revisar(1_180_000m, 0m, 2, Agora.AddMonths(1)).Should().BeTrue();
        c.Valor.Should().Be(1_180_000m);
        c.ImportadoPorId.Should().Be(2);
    }

    [Fact]
    public void O_catalogo_so_aceita_as_quatro_tabelas()
    {
        var item = ItemDoSicor.Registrar("PRODUTO", 7080, 0, "TRATOR", new DateOnly(2012, 5, 14), null, 1, Agora);
        item.Descricao.Should().Be("TRATOR");

        var desconhecida = () => ItemDoSicor.Registrar("MODALIDADE", 14, 0, "x", null, null, 1, Agora);
        desconhecida.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Catalogo_so_muda_quando_a_descricao_ou_a_vigencia_mudam()
    {
        var item = ItemDoSicor.Registrar("PROGRAMA", 154, 0, "MODERFROTA", null, null, 1, Agora);

        item.Revisar("MODERFROTA", null, null, 2, Agora.AddDays(1)).Should().BeFalse();
        item.Revisar("MODERFROTA", null, new DateOnly(2027, 6, 30), 2, Agora.AddDays(1)).Should().BeTrue();
    }
}
