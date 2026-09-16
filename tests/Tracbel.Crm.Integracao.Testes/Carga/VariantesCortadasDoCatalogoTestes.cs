using FluentAssertions;
using Tracbel.Crm.Integracao.Carga;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Carga;

/// <summary>
/// AS GRAFIAS CORTADAS QUE O CATÁLOGO RECONHECIDO JÁ PROVA, sem chamar o IBGE (documento 32, seção 4.6).
///
/// <para>É o que a carga do cadastro consulta a cada recarga para não desfazer uma correção. Os
/// municípios de nome inventado têm código inventado: a regra não depende de quais são.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class VariantesCortadasDoCatalogoTestes
{
    [Fact]
    public void Linha_cortada_em_vinte_com_um_unico_oficial_da_uf_e_variante_desse_oficial()
    {
        var variantes = SaneamentoDeTerritorio.VariantesCortadasDoCatalogo(
        [
            new MunicipioDoCatalogo(1, "São José do Rio Preto", "SP", 3549805),
            new MunicipioDoCatalogo(2, "SAO JOSE DO RIO PRET", "SP", null)
        ]);

        variantes.Should().ContainSingle();
        variantes[0].MunicipioId.Should().Be(2);
        variantes[0].Oficial.Codigo.Should().Be(3549805);
    }

    [Fact]
    public void Prefixo_de_dois_oficiais_nao_e_variante_de_nenhum()
    {
        var variantes = SaneamentoDeTerritorio.VariantesCortadasDoCatalogo(
        [
            new MunicipioDoCatalogo(1, "Bom Jesus dos Perdoes Novo", "SP", 3500001),
            new MunicipioDoCatalogo(2, "Bom Jesus dos Perdoes Velho", "SP", 3500002),
            new MunicipioDoCatalogo(3, "BOM JESUS DOS PERDOE", "SP", null)
        ]);

        variantes.Should().BeEmpty("ambiguidade não se resolve por aproximação");
    }

    [Theory]
    [InlineData("SAO JOSE DO RIO PRE", "SP")]
    [InlineData("SAO JOSE DO RIO PRET", "MG")]
    public void Nome_sem_a_marca_de_truncamento_ou_de_outra_uf_nao_e_variante(string nome, string uf)
    {
        SaneamentoDeTerritorio.VariantesCortadasDoCatalogo(
        [
            new MunicipioDoCatalogo(1, "São José do Rio Preto", "SP", 3549805),
            new MunicipioDoCatalogo(2, nome, uf, null)
        ]).Should().BeEmpty();
    }

    [Fact]
    public void Catalogo_nunca_reconhecido_nao_tem_variante()
    {
        SaneamentoDeTerritorio.VariantesCortadasDoCatalogo(
        [
            new MunicipioDoCatalogo(1, "SAO JOSE DO RIO PRETO", "SP", null),
            new MunicipioDoCatalogo(2, "SAO JOSE DO RIO PRET", "SP", null)
        ]).Should().BeEmpty("sem código IBGE no catálogo não há município oficial para apontar");
    }
}
