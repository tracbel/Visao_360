using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>O potencial de referência faz a conta do motor por município uma vez, e a mesma conta da apuração.</summary>
public sealed class PotencialDeReferenciaTestes
{
    private const int Cafe = 40139;

    private static PotencialDeReferencia Montar(decimal hectares)
    {
        var catalogo = new CatalogoDoMotor(
            [new RegraNoMotor("CAFE", "Café", "TRATOR", "Trator", 10m, 5m, true, [Cafe], null)],
            new Dictionary<int, IReadOnlyList<ChaveNoMotor>> { [Cafe] = [new ChaveNoMotor("TRATOR", "CAFE")] });
        var medidas = new Dictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio>
        {
            [(3598003, Cafe)] = new(hectares, hectares, hectares * 2, hectares * 10)
        };
        return new PotencialDeReferencia(
            2024, [], new Dictionary<int, CategoriaDaRegra>(), catalogo, new Dictionary<int, short> { [Cafe] = 2024 },
            medidas, new Dictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio>(),
            new Dictionary<int, ProducaoAgricolaDoMunicipio>(), []);
    }

    [Fact]
    public void O_parque_e_a_demanda_do_municipio_saem_do_motor_com_a_area_da_pam()
    {
        var doMunicipio = Montar(450m).DoMunicipio(3598003);

        doMunicipio.Sobreposto.Parque.Should().Be(45m, "450 ha ÷ 10 ha por máquina");
        doMunicipio.Sobreposto.DemandaAnual.Should().Be(9m, "o parque ÷ 5 anos");
        doMunicipio.Demanda.Should().ContainSingle(d => d.CategoriaCodigo == "TRATOR" && d.CulturaCodigo == "CAFE" && d.DemandaAnual == 9m);
    }

    [Fact]
    public void O_municipio_sem_pam_nao_tem_demanda_e_a_conta_e_guardada()
    {
        var potencial = Montar(450m);

        var semPam = potencial.DoMunicipio(3500000);
        semPam.Demanda.Should().BeEmpty("sigilo do IBGE não é lavoura zero: sem área não há parque");
        potencial.DoMunicipio(3500000).Should().BeSameAs(semPam, "a mesma conta não é refeita");
    }
}
