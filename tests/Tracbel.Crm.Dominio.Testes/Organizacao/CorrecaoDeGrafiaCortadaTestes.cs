using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// As duas operações de domínio da correção de grafia cortada (documento 32, seção 4.6): o
/// endereço troca SÓ o ponteiro de município, e a chave de origem passa a apontar para a linha
/// oficial.
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class CorrecaoDeGrafiaCortadaTestes
{
    private static Endereco EnderecoNoCatalogo(int municipioId) =>
        Endereco.Criar(1, 10, TipoDeEndereco.Fiscal, "Rua do teste", MunicipioDoEndereco.Selecionado(municipioId), "SP", 1,
            ehPrincipal: true, latitude: -20.8m, longitude: -49.3m);

    [Fact]
    public void Reapontar_troca_so_o_municipio_e_preserva_o_resto_do_endereco()
    {
        var endereco = EnderecoNoCatalogo(9490);

        endereco.ReapontarMunicipioDoCatalogo(9491, 7).Should().BeTrue();

        endereco.MunicipioId.Should().Be(9491);
        endereco.Municipio.Should().BeNull();
        endereco.Logradouro.Should().Be("Rua do teste");
        endereco.Uf.Should().Be("SP");
        endereco.Latitude.Should().Be(-20.8m);
    }

    [Fact]
    public void Reapontar_para_o_mesmo_municipio_nao_muda_nada() =>
        EnderecoNoCatalogo(9491).ReapontarMunicipioDoCatalogo(9491, 7).Should().BeFalse();

    [Fact]
    public void Endereco_com_o_texto_do_legado_nao_e_corrigido_por_reapontamento()
    {
        var endereco = Endereco.Criar(1, 10, TipoDeEndereco.Fiscal, "Rua do teste",
            MunicipioDoEndereco.NaoIdentificadoNaCarga("PALMEIRA DOESTE"), "SP", 1);

        var reapontar = () => endereco.ReapontarMunicipioDoCatalogo(9271, 7);

        reapontar.Should().Throw<RegraDeNegocioViolada>("texto não prova a qual município o endereço pertence");
    }

    [Fact]
    public void A_chave_de_origem_passa_a_apontar_para_a_linha_oficial()
    {
        var chave = ChaveExterna.Criar(1, "Municipio", 9490, "8123");
        var quando = new DateTime(2026, 9, 13, 15, 0, 0, DateTimeKind.Utc);

        chave.ReapontarPara(9491, quando);

        chave.RegistroId.Should().Be(9491);
        chave.ChaveOrigem.Should().Be("8123", "a chave da origem é o que se preserva");
        chave.SincronizadoEm.Should().Be(quando);
    }
}
