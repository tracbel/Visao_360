using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Seguranca;

/// <summary>
/// O perfil próprio (issue 113, parte 2b): o editor da tela manda o conjunto final de permissões, e o perfil
/// passa a ser exatamente aquilo — inclusive com menos, ou com profundidade menor.
/// </summary>
public sealed class PerfilProprioTestes
{
    [Fact]
    public void Definir_permissoes_poe_o_perfil_exatamente_no_conjunto_pedido()
    {
        var perfil = Perfil.Criar("VENDAS_NORTE", "Vendas norte")
            .Conceder(Permissoes.ClienteLer, Profundidade.Organizacao)
            .Conceder(Permissoes.ClienteExcluir, Profundidade.EmpresaEAbaixo);

        perfil.DefinirPermissoes([
            (Permissoes.ClienteLer, Profundidade.EmpresaEAbaixo),
            (Permissoes.EquipamentoLer, Profundidade.EmpresaEAbaixo)
        ]);

        perfil.Permissoes.ToDictionary(p => p.CodigoPermissao, p => p.Profundidade).Should().Equal(new Dictionary<string, Profundidade>
        {
            [Permissoes.ClienteLer] = Profundidade.EmpresaEAbaixo,
            [Permissoes.EquipamentoLer] = Profundidade.EmpresaEAbaixo
        }, "Excluir saiu, e Ler desceu de Organização para a filial");
    }

    [Fact]
    public void Definir_permissoes_recusa_codigo_fora_do_catalogo_repetido_ou_com_nenhum()
    {
        var perfil = Perfil.Criar("X", "X");

        ((Action)(() => perfil.DefinirPermissoes([("Lead.Ler", Profundidade.Organizacao)]))).Should().Throw<RegraDeNegocioViolada>().WithMessage("*catálogo*");
        ((Action)(() => perfil.DefinirPermissoes([(Permissoes.ClienteLer, Profundidade.Organizacao), (Permissoes.ClienteLer, Profundidade.Empresa)])))
            .Should().Throw<RegraDeNegocioViolada>().WithMessage("*mais de uma vez*");
        ((Action)(() => perfil.DefinirPermissoes([(Permissoes.ClienteLer, Profundidade.Nenhum)]))).Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Renomear_e_reativar()
    {
        var perfil = Perfil.Criar("X", "Antigo", "descrição antiga");

        perfil.Renomear("  Novo nome  ", "  ");
        perfil.Desativar();
        perfil.Reativar();

        perfil.Nome.Should().Be("Novo nome");
        perfil.Descricao.Should().BeNull();
        perfil.EstaAtivo.Should().BeTrue();
        ((Action)(() => perfil.Renomear(" ", null))).Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void O_sistema_conhece_os_proprios_perfis_pelo_codigo()
    {
        PerfisDeSistema.EhDoSistema(PerfisDeSistema.Diretoria).Should().BeTrue();
        PerfisDeSistema.EhDoSistema("administrador").Should().BeTrue("a caixa não importa");
        PerfisDeSistema.EhDoSistema("DIRETORIA_TRACBEL").Should().BeFalse();
    }
}
