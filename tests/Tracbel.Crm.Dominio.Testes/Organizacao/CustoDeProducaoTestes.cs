using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>O CUSTO DE PRODUÇÃO (issue 67) — o que a leitura não pode deixar passar.</summary>
public sealed class CustoDeProducaoTestes
{
    private static readonly DateTime Agora = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

    private static CustoDeProducao.Valores Piracicaba(decimal? total = 13903.20m, decimal operacional = 12994.28m) =>
        new(9, 77987.01m, "kg/ha", 10330.59m, 132.24702m, 2663.69m, 34.52645m, operacional, 166.77347m,
            908.92m, 11.76423m, total, 178.5377m);

    private static CustoDeProducao Registrar(CustoDeProducao.Valores v) =>
        CustoDeProducao.Registrar("CANA DE AÇÚCAR", "Piracicaba-SP-2025", "Piracicaba", null, 10, 2025, "t", v, 1, Agora);

    [Fact]
    public void Registra_as_cinco_camadas()
    {
        var c = Registrar(Piracicaba());

        c.CustoTotalHa.Should().Be(13903.20m);
        c.CustoOperacionalHa.Should().Be(12994.28m);
        c.Safra.Should().Be((short)2025);
    }

    [Fact]
    public void Total_menor_que_o_operacional_e_leitura_da_coluna_errada()
    {
        var registrar = () => Registrar(Piracicaba(total: 900m));
        registrar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*coluna errada*");
    }

    [Fact]
    public void Custo_operacional_zero_e_aba_vazia()
    {
        var registrar = () => Registrar(Piracicaba(operacional: 0m));
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Aba_que_para_no_operacional_entra_sem_total() =>
        Registrar(Piracicaba(total: null) with { RendaDeFatoresHa = null, RendaDeFatoresUnidade = null, CustoTotalUnidade = null })
            .CustoTotalHa.Should().BeNull();

    [Fact]
    public void Revisar_com_os_mesmos_valores_nao_muda_nem_o_carimbo()
    {
        var c = Registrar(Piracicaba());

        c.Revisar(10, Piracicaba(), 2, Agora.AddMonths(1)).Should().BeFalse();
        c.ImportadoEm.Should().Be(Agora);
    }

    [Fact]
    public void Revisao_da_CONAB_muda_o_valor_e_o_carimbo()
    {
        var c = Registrar(Piracicaba());
        var depois = Agora.AddMonths(1);

        c.Revisar(10, Piracicaba(total: 14000m), 2, depois).Should().BeTrue();

        c.CustoTotalHa.Should().Be(14000m);
        c.ImportadoEm.Should().Be(depois);
    }

    [Fact]
    public void Revisao_invalida_nao_altera_nada()
    {
        var c = Registrar(Piracicaba());

        var revisar = () => c.Revisar(10, Piracicaba(total: 1m), 2, Agora.AddDays(1));

        revisar.Should().Throw<RegraDeNegocioViolada>();
        c.CustoTotalHa.Should().Be(13903.20m, "a validação vem antes da gravação");
    }
}
