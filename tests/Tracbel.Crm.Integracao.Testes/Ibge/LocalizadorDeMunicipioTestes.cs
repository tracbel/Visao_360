using FluentAssertions;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Ibge;

/// <summary>
/// EM QUE MUNICÍPIO O PONTO CAI (telemetria, 28/09/2026): o contorno que o contém, o buraco fora, e nulo fora da malha —
/// nunca o vizinho mais perto.
/// </summary>
public sealed class LocalizadorDeMunicipioTestes
{
    private static (double Lon, double Lat)[] Quadrado(double oeste, double sul, double leste, double norte) =>
        [(oeste, sul), (leste, sul), (leste, norte), (oeste, norte), (oeste, sul)];

    private static LocalizadorDeMunicipio Malha() => new(new Dictionary<int, PoligonoMunicipal>
    {
        // Um "Ribeirão" com um buraco no meio, e um "Sertãozinho" colado a oeste.
        [3543402] = PoligonoMunicipal.DeAneis([[Quadrado(-48.0, -21.5, -47.5, -21.0), Quadrado(-47.8, -21.3, -47.7, -21.2)]]),
        [3551702] = PoligonoMunicipal.DeAneis([[Quadrado(-48.5, -21.5, -48.0, -21.0)]])
    });

    [Fact]
    public void O_ponto_cai_no_municipio_que_o_contem()
    {
        var malha = Malha();

        malha.Municipios.Should().Be(2);
        malha.CodigoIbgeDe(-47.6, -21.1).Should().Be(3543402);
        malha.CodigoIbgeDe(-48.3, -21.4).Should().Be(3551702);
    }

    [Fact]
    public void O_ponto_no_buraco_ou_fora_da_malha_nao_tem_municipio()
    {
        var malha = Malha();

        malha.CodigoIbgeDe(-47.75, -21.25).Should().BeNull("o buraco não é do município");
        malha.CodigoIbgeDe(-43.2, -22.9).Should().BeNull("fora da malha é fora de São Paulo, e não o vizinho mais perto");
    }

    [Fact]
    public void A_caixa_do_contorno_nao_muda_a_resposta_de_quem_esta_dentro()
    {
        // O PONTO DENTRO DA CAIXA E FORA DO CONTORNO (um triângulo) continua fora: a caixa só poupa a conta.
        var triangulo = PoligonoMunicipal.DeAneis([[[(0.0, 0.0), (10.0, 0.0), (0.0, 10.0), (0.0, 0.0)]]]);

        triangulo.Contem(1, 1).Should().BeTrue();
        triangulo.Contem(9, 9).Should().BeFalse("dentro da caixa, fora do triângulo");
        triangulo.Contem(11, 1).Should().BeFalse("fora da caixa");
    }
}
