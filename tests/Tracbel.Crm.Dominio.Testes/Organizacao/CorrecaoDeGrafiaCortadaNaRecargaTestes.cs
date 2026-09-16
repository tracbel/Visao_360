using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// A CORREÇÃO DE GRAFIA CORTADA DIANTE DE UMA RECARGA DA ORIGEM (documento 32, seção 4.6).
///
/// <para>A origem traz de novo a linha cortada do catálogo. A correção só fica se a evidência que a
/// sustentou é a mesma; qualquer diferença devolve o endereço à origem para ser conferido de novo.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class CorrecaoDeGrafiaCortadaNaRecargaTestes
{
    private const int GrafiaCortada = 9450;
    private const int Oficial = 9451;
    private const decimal Latitude = -20.1234567m;
    private const decimal Longitude = -50.7654321m;

    private static Endereco Corrigido(decimal? latitude = Latitude, decimal? longitude = Longitude)
    {
        var endereco = Endereco.Criar(
            1, 10, TipoDeEndereco.Fiscal, "Rua do teste", MunicipioDoEndereco.Selecionado(GrafiaCortada), "SP", 1,
            ehPrincipal: true, latitude: latitude, longitude: longitude);

        endereco.ReapontarMunicipioDoCatalogo(Oficial, 1);
        return endereco;
    }

    [Fact]
    public void A_mesma_evidencia_mantem_a_correcao()
    {
        Corrigido().CorrecaoDeGrafiaCortadaSeMantem(Oficial, "SP", Latitude, Longitude).Should().BeTrue();
    }

    [Theory]
    [InlineData(-20.12345671, -50.76543209)]
    [InlineData(-20.12345675, -50.76543215)]
    [InlineData(-20.12345679, -50.76543219)]
    public void Coordenada_com_mais_casas_do_que_o_banco_guarda_nao_conta_como_mudanca(double latitude, double longitude)
    {
        // O gravado pode ter vindo arredondado (para cima ou para o par) ou truncado na sétima casa.
        Corrigido().CorrecaoDeGrafiaCortadaSeMantem(Oficial, "sp", Math.Round((decimal)latitude, 8), Math.Round((decimal)longitude, 8))
            .Should().BeTrue("o banco guarda sete casas, e a oitava não é evidência nova");
    }

    [Fact]
    public void Uma_unidade_inteira_na_setima_casa_ja_e_outra_coordenada()
    {
        Corrigido().CorrecaoDeGrafiaCortadaSeMantem(Oficial, "SP", Latitude + 0.0000001m, Longitude).Should().BeFalse();
    }

    [Fact]
    public void Sem_coordenada_dos_dois_lados_mantem()
    {
        Corrigido(null, null).CorrecaoDeGrafiaCortadaSeMantem(Oficial, "SP", null, null).Should().BeTrue();
    }

    [Theory]
    [InlineData("MG", -20.1234567, -50.7654321)]
    [InlineData("SP", -20.2, -50.7654321)]
    [InlineData("SP", -20.1234567, -50.8)]
    public void Uf_ou_coordenada_diferente_desfaz_a_correcao(string uf, double latitude, double longitude)
    {
        Corrigido().CorrecaoDeGrafiaCortadaSeMantem(Oficial, uf, (decimal)latitude, (decimal)longitude)
            .Should().BeFalse("evidência nova precisa ser conferida de novo");
    }

    [Fact]
    public void Coordenada_que_passou_a_faltar_desfaz_a_correcao()
    {
        Corrigido().CorrecaoDeGrafiaCortadaSeMantem(Oficial, "SP", null, null).Should().BeFalse();
    }

    [Fact]
    public void Endereco_que_nao_esta_no_oficial_daquela_grafia_nao_tem_correcao_a_manter()
    {
        var naGrafia = Endereco.Criar(
            1, 10, TipoDeEndereco.Fiscal, "Rua do teste", MunicipioDoEndereco.Selecionado(GrafiaCortada), "SP", 1);

        naGrafia.CorrecaoDeGrafiaCortadaSeMantem(Oficial, "SP", null, null)
            .Should().BeFalse("o endereço em conflito continua na grafia da origem, sem associação forçada");

        Corrigido().CorrecaoDeGrafiaCortadaSeMantem(9999, "SP", Latitude, Longitude).Should().BeFalse();
    }
}
