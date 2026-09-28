using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// O FATURAMENTO NÃO DEPENDE DE NADA DO VÓRTICE (issue 19).
///
/// Até 24/09/2026 a carga do faturamento morava dentro da carga do Vórtice, só porque nasceu lá, e atualizar o número
/// da diretoria exigia reler o legado inteiro. Desde o PR 233 ela é código próprio: a contraparte vem da SD2 do
/// Protheus e o cliente vem do nosso cadastro. Este teste impede que um atalho futuro — um tipo do Vórtice "só para
/// reaproveitar" — traga a dependência de volta sem ninguém perceber.
/// </summary>
[Trait("Categoria", "Arquitetura")]
public class FaturamentoSemVorticeTestes
{
    private static readonly Assembly Carga = Assembly.Load("Tracbel.Crm.Carga");

    private static readonly Assembly Integracao = typeof(Tracbel.Crm.Integracao.Protheus.LeitorDeFaturamentoDoProtheus).Assembly;

    /// <summary>Tudo o que é do Vórtice: o leitor e os tipos da integração, e as cargas que o leem.</summary>
    private static readonly string[] Vortice =
    [
        "Tracbel.Crm.Integracao.Vortice",
        "Tracbel.Crm.Carga.CargaDoVortice",
        "Tracbel.Crm.Carga.CargaDeProcessoDoVortice",
        "Tracbel.Crm.Carga.CargaDeCarteirasDoVortice",
        "Tracbel.Crm.Carga.CargaDoFunilDoVortice",
        "Tracbel.Crm.Carga.CargaDasOportunidadesDoVortice"
    ];

    [Fact]
    public void A_carga_do_faturamento_nao_usa_tipo_nenhum_do_Vortice()
    {
        var tipos = Types.InAssembly(Carga).That().HaveNameMatching("Faturamento");

        tipos.GetTypes().Should().Contain(t => t.Name == "CargaDeFaturamentoDoProtheus",
            "a regra precisa estar olhando para a carga de verdade, e não para um conjunto vazio");

        var resultado = tipos.ShouldNot().HaveDependencyOnAny(Vortice).GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            "o faturamento vem do Protheus e do nosso cadastro. Violações: {0}",
            string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact]
    public void Os_leitores_do_Protheus_nao_usam_tipo_nenhum_do_Vortice()
    {
        var resultado = Types.InAssembly(Integracao)
            .That().ResideInNamespace("Tracbel.Crm.Integracao.Protheus")
            .ShouldNot().HaveDependencyOnAny(Vortice)
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            "o Protheus é lido sem passar pelo legado. Violações: {0}",
            string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact]
    public void A_regra_tem_dente_a_carga_das_carteiras_do_Vortice_seria_barrada()
    {
        // Prova de que o teste acima não passa por estar cego: a mesma regra, aplicada à carga que LÊ o Vórtice,
        // precisa falhar.
        var resultado = Types.InAssembly(Carga)
            .That().HaveName("CargaDeCarteirasDoVortice")
            .ShouldNot().HaveDependencyOnAny("Tracbel.Crm.Integracao.Vortice")
            .GetResult();

        resultado.IsSuccessful.Should().BeFalse("a carga das carteiras lê o Vórtice, e a regra precisa enxergar isso");
    }
}
