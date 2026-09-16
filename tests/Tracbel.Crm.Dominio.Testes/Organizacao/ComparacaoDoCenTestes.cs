using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// COMO AS DUAS PLANILHAS NOMEIAM O CEN (documento 32, seção 4.3). A comparação é rótulo: não funde e
/// não escolhe fonte. Os nomes aqui são inventados.
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class ComparacaoDoCenTestes
{
    [Fact]
    public void O_mesmo_usuario_identificado_nas_duas_fontes_e_o_mesmo_nome()
    {
        ResponsavelPeloMunicipio.CompararCen("FULANO.DETAL", 10, "Fulano Detal Junior", 10)
            .Should().Be(ComparacaoDoCen.MesmoNome);
    }

    [Theory]
    [InlineData("FULANO.DE.TAL", "Fulano de Tal", ComparacaoDoCen.MesmoNome)]
    [InlineData("Fulano", "Fulano de Tal", ComparacaoDoCen.ProvavelMesmaPessoa)]
    [InlineData("Fulano Silva", "Fulano Souza", ComparacaoDoCen.ProvavelMesmaPessoa)]
    [InlineData("A Contratar 3", "Beltrano de Tal", ComparacaoDoCen.NomesDiferentes)]
    [InlineData("Ciclano Souza", "Beltrano Souza", ComparacaoDoCen.NomesDiferentes)]
    public void A_comparacao_separa_grafia_de_pessoa_sem_decidir_por_ninguem(string numa, string outra, ComparacaoDoCen esperada)
    {
        ResponsavelPeloMunicipio.CompararCen(numa, null, outra, null).Should().Be(esperada);
    }
}
