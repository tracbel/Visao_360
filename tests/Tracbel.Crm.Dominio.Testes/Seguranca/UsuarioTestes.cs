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
    private static Usuario DaCarga(string login = "pessoa.ficticia") => Usuario.Criar(
        identidadeExterna: Guid.NewGuid(),
        nomePrincipal: login + Usuario.SufixoSemEmail,
        nomeCompleto: "PESSOA FICTICIA",
        nomeExibicao: "Pessoa Ficticia",
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
            oid, "  pessoa.ficticia@tracbel.com.br ", Email.Criar("Pessoa.Ficticia@tracbel.com.br"), agora);

        usuario.IdentidadeExterna.Should().Be(oid);
        usuario.NomePrincipal.Should().Be("pessoa.ficticia@tracbel.com.br", "o espaço nas pontas não é parte do nome");
        usuario.Email.Endereco.Should().Be("pessoa.ficticia@tracbel.com.br");
        usuario.UltimoLoginEm.Should().Be(agora);
        usuario.AindaNaoEntrouPeloEntraId.Should().BeFalse(
            "é o sumiço do sufixo inventado que impede um segundo 'pessoa.ficticia' de herdar esta conta");
    }

    [Fact]
    public void Vincular_recusa_identificador_vazio()
    {
        var vincular = () => DaCarga().VincularAoEntraId(
            Guid.Empty, "pessoa.ficticia@tracbel.com.br", Email.Criar("pessoa.ficticia@tracbel.com.br"), DateTime.UtcNow);

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

    // ------------------------------------------------------------------ a conta nova do primeiro login

    private static readonly DateTime NoLogin = new(2026, 9, 21, 20, 0, 0, DateTimeKind.Utc);

    private static Usuario DoPrimeiroLogin(string? nome = "Pessoa Nova Ficticia") => Usuario.CriarNoPrimeiroLogin(
        Guid.NewGuid(), " pessoa.nova@tracbel.com.br ", nome, Email.Criar("pessoa.nova@tracbel.com.br"), 1, NoLogin);

    [Fact]
    public void A_conta_do_primeiro_login_nasce_aguardando_liberacao()
    {
        var usuario = DoPrimeiroLogin();

        usuario.AguardaLiberacao.Should().BeTrue("o grupo do Entra deixa entrar; o que ela enxerga, quem decide é o administrador");
        usuario.AguardandoLiberacaoDesde.Should().Be(NoLogin);
        usuario.EstaAtivo.Should().BeTrue("esperar liberação não é estar desativada: não há data de desativação a dar");
        usuario.NomePrincipal.Should().Be("pessoa.nova@tracbel.com.br");
        usuario.NomeExibicao.Should().Be("Pessoa Nova Ficticia");
        usuario.Natureza.Should().Be(NaturezaDoUsuario.Pessoa);
        usuario.AindaNaoEntrouPeloEntraId.Should().BeFalse("o nome principal já é o real");
    }

    [Fact]
    public void Sem_o_nome_no_token_vale_a_parte_antes_do_arroba()
    {
        DoPrimeiroLogin(nome: null).NomeCompleto.Should().Be("pessoa.nova");
    }

    [Fact]
    public void O_nome_longo_e_cortado_no_tamanho_das_colunas()
    {
        var usuario = DoPrimeiroLogin(nome: new string('a', 250));

        usuario.NomeCompleto.Should().HaveLength(200);
        usuario.NomeExibicao.Should().HaveLength(80);
    }

    [Fact]
    public void A_conta_do_primeiro_login_recusa_identificador_vazio()
    {
        var criar = () => Usuario.CriarNoPrimeiroLogin(
            Guid.Empty, "pessoa.nova@tracbel.com.br", null, Email.Criar("pessoa.nova@tracbel.com.br"), 1, NoLogin);

        criar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Liberar_escolhe_a_filial_e_encerra_a_espera()
    {
        var usuario = DoPrimeiroLogin();

        usuario.Liberar(filialId: 7, usuarioId: 42);

        usuario.AguardaLiberacao.Should().BeFalse();
        usuario.EmpresaId.Should().Be(7, "a filial provisória sai; vale a que o administrador escolheu");
        usuario.AlteradoPorId.Should().Be(42, "a trilha precisa dizer quem liberou");
    }

    [Fact]
    public void Liberar_duas_vezes_e_recusado()
    {
        var usuario = DoPrimeiroLogin();
        usuario.Liberar(7, 42);

        var deNovo = () => usuario.Liberar(3, 42);

        deNovo.Should().Throw<RegraDeNegocioViolada>("trocar a filial de quem já entra é outra operação, e não uma nova liberação");
    }

    [Fact]
    public void Conta_da_carga_nao_aguarda_liberacao()
    {
        DaCarga().AguardaLiberacao.Should().BeFalse("quem veio do Vórtice já era usuário; só a conta nova do primeiro login espera");
    }
}
