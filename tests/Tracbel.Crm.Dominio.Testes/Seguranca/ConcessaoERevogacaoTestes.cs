using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Seguranca;

/// <summary>
/// A administração das concessões (issue 113): revogar guarda o histórico, ninguém concede o que não tem, e a
/// conta desativada volta com os perfis que ainda valem.
/// </summary>
public sealed class ConcessaoERevogacaoTestes
{
    private static readonly DateTime Agora = new(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

    private static ContextoAcesso Com(params (string Codigo, Profundidade Profundidade)[] permissoes) =>
        new(1, "quem concede", 1, new HashSet<int> { 1 }, new HashSet<long>(), new HashSet<long>(),
            permissoes.ToDictionary(p => p.Codigo, p => p.Profundidade));

    [Fact]
    public void Revogar_guarda_quem_quando_e_por_que_e_a_concessao_deixa_de_valer()
    {
        var concessao = UsuarioPerfil.Conceder(10, 4, "autorizado pela diretoria", 1, Agora.AddDays(-5));

        concessao.Revogar(2, "  saiu da empresa  ", Agora);

        concessao.Revogada.Should().BeTrue();
        concessao.RevogadaEm.Should().Be(Agora);
        concessao.RevogadaPorId.Should().Be(2);
        concessao.MotivoDaRevogacao.Should().Be("saiu da empresa");
        concessao.EstaVigente(Agora.AddSeconds(1)).Should().BeFalse();
        concessao.Justificativa.Should().Be("autorizado pela diretoria", "a concessão original continua registrada");
    }

    [Fact]
    public void Revogar_sem_motivo_ou_duas_vezes_e_recusado()
    {
        var concessao = UsuarioPerfil.Conceder(10, 4, "motivo", 1, Agora);

        var semMotivo = () => concessao.Revogar(2, " ", Agora);
        semMotivo.Should().Throw<RegraDeNegocioViolada>().WithMessage("*motivo*");

        concessao.Revogar(2, "motivo", Agora);
        var deNovo = () => concessao.Revogar(2, "outra vez", Agora);
        deNovo.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Quem_concede_so_da_o_que_tem_na_mesma_profundidade_ou_maior()
    {
        var administradorDeFilial = Com(
            (Permissoes.UsuarioAdministrar, Profundidade.Organizacao),
            (Permissoes.ClienteLer, Profundidade.EmpresaEAbaixo));

        RegraDeConcessao.FaltasParaConceder(administradorDeFilial, [(Permissoes.ClienteLer, Profundidade.EmpresaEAbaixo)])
            .Should().BeEmpty();

        RegraDeConcessao.FaltasParaConceder(administradorDeFilial,
            [
                (Permissoes.ClienteLer, Profundidade.Organizacao),
                (Permissoes.PerfilAdministrar, Profundidade.Organizacao)
            ])
            .Should().Equal(Permissoes.ClienteLer, Permissoes.PerfilAdministrar);
    }

    [Fact]
    public void O_administrador_semeado_pode_conceder_qualquer_perfil_semeado()
    {
        var administrador = Com([.. PerfisDeSistema.Todos.Single(p => p.Codigo == PerfisDeSistema.Administrador).Concedidas]);

        foreach (var perfil in PerfisDeSistema.Todos)
            RegraDeConcessao.FaltasParaConceder(administrador, perfil.Concedidas).Should().BeEmpty(perfil.Codigo);
    }

    [Fact]
    public void Reativar_devolve_a_conta_e_limpa_a_data_de_desativacao()
    {
        var usuario = Usuario.Criar(Guid.NewGuid(), "pessoa@exemplo.invalid", "Pessoa", "Pessoa",
            Email.Criar("pessoa@exemplo.invalid"), 1, criadoPorId: 1);

        usuario.Desativar(5);
        usuario.EstaAtivo.Should().BeFalse();

        usuario.Reativar(6);

        usuario.EstaAtivo.Should().BeTrue();
        usuario.DesativadoEm.Should().BeNull();
        usuario.AlteradoPorId.Should().Be(6);
    }
}
