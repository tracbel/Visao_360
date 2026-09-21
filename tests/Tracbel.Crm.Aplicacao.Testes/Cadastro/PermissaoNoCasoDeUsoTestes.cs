using FluentAssertions;
using Tracbel.Crm.Aplicacao.Clientes;
using Tracbel.Crm.Aplicacao.Equipamentos;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Cadastro;

/// <summary>
/// A PERMISSÃO CONFERIDA NO PRÓPRIO CASO DE USO DE ESCRITA (fase 3 do documento 41; issue 46).
///
/// <para>A rota já recusa com 403; o caso de uso recusa de novo porque pode ser chamado por outro
/// caminho um dia. A conferência é a primeira coisa que ele faz — por isso os repositórios aqui podem ser
/// nulos: se o caso de uso chegasse a tocá-los sem a permissão, o teste estouraria.</para>
/// </summary>
[Trait("Categoria", "Autorizacao")]
public sealed class PermissaoNoCasoDeUsoTestes
{
    private sealed class Provedor(ContextoAcesso atual) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = atual;
    }

    /// <summary>Um usuário que só lê: o que sobra quando o perfil não dá escrita nenhuma.</summary>
    private static Provedor SoLeitura() => new(new ContextoAcesso(
        usuarioId: 1,
        nomeExibicao: "só leitura",
        empresaId: 1,
        empresasVisiveis: new HashSet<int> { 1 },
        subordinadosIds: new HashSet<long>(),
        equipesIds: new HashSet<long>(),
        profundidades: new Dictionary<string, Profundidade>
        {
            [Permissoes.ClienteLer] = Profundidade.EmpresaEAbaixo,
            [Permissoes.EquipamentoLer] = Profundidade.EmpresaEAbaixo
        }));

    [Fact]
    public async Task Criar_alterar_e_inativar_cliente_sem_a_permissao_sao_recusados_antes_de_tocar_o_banco()
    {
        var acesso = SoLeitura();

        var criar = await new CriarCliente(null!, null!, null!, acesso).ExecutarAsync(null!, CancellationToken.None);
        var alterar = await new AlterarCliente(null!, null!, null!, acesso).ExecutarAsync(Guid.NewGuid(), null!, CancellationToken.None);
        var inativar = await new InativarCliente(null!, null!, null!, acesso).ExecutarAsync(Guid.NewGuid(), null!, CancellationToken.None);

        foreach (var (resultado, permissao) in new[] { (criar, Permissoes.ClienteCriar), (alterar, Permissoes.ClienteEditar), (inativar, Permissoes.ClienteExcluir) })
        {
            resultado.Tipo.Should().Be(TipoDeFalha.SemPermissao);
            resultado.Erro.Should().Contain(permissao);
        }
    }

    [Fact]
    public async Task Criar_alterar_e_dar_baixa_em_equipamento_sem_a_permissao_sao_recusados()
    {
        var acesso = SoLeitura();

        var criar = await new CriarEquipamento(null!, null!, null!, null!, acesso).ExecutarAsync(null!, CancellationToken.None);
        var alterar = await new AlterarEquipamento(null!, null!, null!, null!, acesso).ExecutarAsync(Guid.NewGuid(), null!, CancellationToken.None);
        var baixar = await new InativarEquipamento(null!, null!, acesso).ExecutarAsync(Guid.NewGuid(), null!, CancellationToken.None);

        criar.Tipo.Should().Be(TipoDeFalha.SemPermissao);
        alterar.Tipo.Should().Be(TipoDeFalha.SemPermissao);
        baixar.Tipo.Should().Be(TipoDeFalha.SemPermissao);
        baixar.Erro.Should().Contain(Permissoes.EquipamentoExcluir);
    }
}
