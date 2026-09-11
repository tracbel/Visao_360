using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Seguranca;

/// <summary>
/// O vínculo da conta com o Microsoft Entra ID — a decisão que entrega uma carteira a uma pessoa.
/// </summary>
public sealed class UsuarioTestes
{
    private static Usuario DaCarga(string login = "abner.costa") => Usuario.Criar(
        identidadeExterna: Guid.NewGuid(),
        nomePrincipal: login + Usuario.SufixoSemEmail,
        nomeCompleto: "ABNER COSTA",
        nomeExibicao: "Abner Costa",
        email: Email.Criar(login + Usuario.SufixoSemEmail),
        empresaId: 1,
        criadoPorId: 1);

    [Fact]
    public void Conta_criada_pela_carga_ainda_nao_entrou_pelo_Entra()
    {
        DaCarga().AindaNaoEntrouPeloEntraId.Should().BeTrue();
    }

    [Fact]
    public void Vincular_grava_o_identificador_e_troca_o_nome_inventado_pelo_real()
    {
        var usuario = DaCarga();
        var oid = Guid.NewGuid();
        var agora = new DateTime(2026, 9, 10, 14, 0, 0, DateTimeKind.Utc);

        usuario.VincularAoEntraId(
            oid, "  abner.costa@tracbel.com.br ", Email.Criar("Abner.Costa@tracbel.com.br"), agora);

        usuario.IdentidadeExterna.Should().Be(oid);
        usuario.NomePrincipal.Should().Be("abner.costa@tracbel.com.br", "o espaço nas pontas não é parte do nome");
        usuario.Email.Endereco.Should().Be("abner.costa@tracbel.com.br");
        usuario.UltimoLoginEm.Should().Be(agora);
        usuario.AindaNaoEntrouPeloEntraId.Should().BeFalse(
            "é o sumiço do sufixo inventado que impede um segundo 'abner.costa' de herdar esta conta");
    }

    [Fact]
    public void Vincular_recusa_identificador_vazio()
    {
        var vincular = () => DaCarga().VincularAoEntraId(
            Guid.Empty, "abner.costa@tracbel.com.br", Email.Criar("abner.costa@tracbel.com.br"), DateTime.UtcNow);

        vincular.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Registrar_acesso_nao_grava_duas_vezes_na_mesma_hora()
    {
        var usuario = DaCarga();
        var primeiro = new DateTime(2026, 9, 10, 14, 0, 0, DateTimeKind.Utc);

        usuario.RegistrarAcesso(primeiro).Should().BeTrue("não havia acesso registrado");
        usuario.RegistrarAcesso(primeiro.AddMinutes(40)).Should().BeFalse(
            "a Visão 360 dispara dezenas de chamadas por tela; gravar em cada uma viraria disputa de bloqueio");
        usuario.RegistrarAcesso(primeiro.AddMinutes(61)).Should().BeTrue();

        usuario.UltimoLoginEm.Should().Be(primeiro.AddMinutes(61));
    }
}
