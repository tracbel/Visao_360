using FluentAssertions;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Seguranca;

/// <summary>
/// A SEMENTE DOS PERFIS — Diretoria e Gerência (issue 134, 22/09/2026) e as linhas que a migração grava.
///
/// <para>O que se prende aqui: cada perfil dá o que a matriz aprovada diz, e os identificadores das linhas
/// já gravadas no banco não mudam — mudar um deles faria a migração apagar e recriar linhas em vez de só
/// acrescentar ou retirar a que mudou.</para>
/// </summary>
public sealed class PerfisDeSistemaTestes
{
    private static PerfisDeSistema.Semente Perfil(string codigo) => PerfisDeSistema.Todos.Single(p => p.Codigo == codigo);

    private static Dictionary<string, Profundidade> Concedidas(string codigo) =>
        Perfil(codigo).Concedidas.ToDictionary(p => p.Codigo, p => p.Profundidade);

    [Fact]
    public void Gerencia_informa_a_percepcao_e_ve_as_integracoes_mas_nao_todas_as_filiais()
    {
        var gerencia = Concedidas(PerfisDeSistema.Gerencia);

        gerencia.Should().ContainKey(Permissoes.PercepcaoDoGestorInformar);
        gerencia.Should().ContainKey(Permissoes.IntegracaoLer);
        gerencia.Should().NotContainKey(Permissoes.EmpresaAlcanceEntreFiliais);
        gerencia.Should().NotContainKey(Permissoes.UsuarioAdministrar);
        gerencia.Should().NotContainKey(Permissoes.PerfilAdministrar);
    }

    [Fact]
    public void Diretoria_e_a_gerencia_mais_todas_as_filiais()
    {
        var diretoria = Concedidas(PerfisDeSistema.Diretoria);

        diretoria.Keys.Should().Contain(Concedidas(PerfisDeSistema.Gerencia).Keys);
        diretoria[Permissoes.EmpresaAlcanceEntreFiliais].Should().Be(Profundidade.Organizacao, "é o que libera \"Todas as filiais\"");
        diretoria.Should().NotContainKey(Permissoes.UsuarioAdministrar, "liberar conta e conceder perfil é do super admin");
    }

    [Fact]
    public void O_padrao_nao_ve_mais_as_integracoes()
    {
        Concedidas(PerfisDeSistema.Padrao).Should().NotContainKey(Permissoes.IntegracaoLer,
            "a situação das integrações é da Configuração, que o usuário comum não vê (issue 134)");
    }

    [Fact]
    public void As_linhas_do_padrao_ja_gravadas_mantem_o_identificador()
    {
        // Os identificadores de 21/09/2026, antes de a leitura das integrações sair. A 111 era ela.
        var esperadas = new Dictionary<int, string>
        {
            [101] = Permissoes.ClienteLer, [102] = Permissoes.EquipamentoLer, [103] = Permissoes.CatalogoLer,
            [104] = Permissoes.ProcessoLer, [105] = Permissoes.TarefaLer, [106] = Permissoes.InteracaoLer,
            [107] = Permissoes.CoberturaLer, [108] = Permissoes.RelatorioLer, [109] = Permissoes.FaturamentoLer,
            [110] = Permissoes.TerritorioLer, [112] = Permissoes.LegadoLer, [113] = Permissoes.ClienteCriar,
            [114] = Permissoes.ClienteEditar, [115] = Permissoes.EquipamentoCriar, [116] = Permissoes.EquipamentoEditar,
            [117] = Permissoes.ParametroDoPotencialLer
        };

        PerfisDeSistema.LinhasDaSemente()
            .Where(l => l.PerfilId == 1)
            .ToDictionary(l => l.Id, l => l.Codigo)
            .Should().Equal(esperadas);
    }

    [Fact]
    public void Nenhuma_linha_da_semente_tem_profundidade_nenhuma_e_os_identificadores_nao_se_repetem()
    {
        var linhas = PerfisDeSistema.LinhasDaSemente().ToList();

        linhas.Should().OnlyContain(l => l.Profundidade != Profundidade.Nenhum, "Nenhum só marca o lugar de uma permissão retirada");
        linhas.Select(l => l.Id).Should().OnlyHaveUniqueItems();
        linhas.Should().OnlyContain(l => l.Id / 100 == l.PerfilId, "a faixa de cada perfil é 100 × perfil");
    }
}
