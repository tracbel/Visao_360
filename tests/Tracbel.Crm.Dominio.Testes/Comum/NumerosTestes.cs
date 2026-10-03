using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Comum;

/// <summary>A soma que ignora o que não foi divulgado, e o arredondamento que não inventa número.</summary>
public sealed class NumerosTestes
{
    [Fact]
    public void A_soma_ignora_o_nulo_e_sem_nenhum_valor_e_nula() =>
        (Numeros.SomaOuNulo([1.5m, null, 2m]), Numeros.SomaOuNulo([null, null])).Should().Be(((decimal?)3.5m, (decimal?)null));

    [Fact]
    public void O_arredondamento_tem_duas_casas_por_padrao_e_nulo_continua_nulo() =>
        (Numeros.Arredondar(2.345m), Numeros.Arredondar(2.345m, 1), Numeros.Arredondar(null))
            .Should().Be(((decimal?)2.34m, (decimal?)2.3m, (decimal?)null));
}
