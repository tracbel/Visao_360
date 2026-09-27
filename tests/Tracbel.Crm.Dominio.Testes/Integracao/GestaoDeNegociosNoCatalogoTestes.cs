using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Integracao;

/// <summary>
/// A API GESTÃO DE NEGÓCIOS NO CATÁLOGO DAS INTEGRAÇÕES (#138, 27/09/2026): a conexão 13 e a rotina 8, no FIM das listas —
/// a posição é o identificador semeado —, e o tipo novo <see cref="TipoDeConexao.ApiComChave"/>, que tem endereço https e
/// chave, e não tem usuário.
/// </summary>
public sealed class GestaoDeNegociosNoCatalogoTestes
{
    private static Conexao Gn() =>
        Conexao.DoCatalogo(ConexoesDoSistema.Todas.Single(c => c.Codigo == ConexoesDoSistema.GestaoDeNegocios));

    [Fact]
    public void A_conexao_da_gn_e_a_ultima_da_lista_e_por_isso_a_13()
    {
        var todas = ConexoesDoSistema.Todas;

        todas[^1].Codigo.Should().Be(ConexoesDoSistema.GestaoDeNegocios, "conexão nova entra no fim: a posição é o identificador");
        todas.Count.Should().Be(13);
        todas[^1].Tipo.Should().Be(TipoDeConexao.ApiComChave);
        todas[^1].Endereco.Should().BeNull("o endereço é digitado pela tela, pelo NOME do servidor (D-M1)");
    }

    [Fact]
    public void A_rotina_das_metas_e_a_ultima_nasce_desligada_e_exige_a_gn()
    {
        var todas = RotinasDoSistema.Todas;
        var metas = todas[^1];

        metas.Codigo.Should().Be(RotinasDoSistema.MetasGestaoDeNegocios);
        todas.Count.Should().Be(8, "é a rotina 8; a de processos do Vórtice vem depois, com o 9");
        metas.Modos.Should().Equal("--somente-metas-gn");
        metas.LigadaPorPadrao.Should().BeFalse("trazer dado novo para produção é decisão de quem administra");
        metas.ConexaoExigida.Should().Be(ConexoesDoSistema.GestaoDeNegocios);
        metas.AgendaPadrao.Should().Be(AgendaDaRotina.DiariaAs(new TimeOnly(6, 0)));
    }

    [Fact]
    public void A_api_com_chave_so_aceita_https_e_nao_guarda_usuario()
    {
        Conexao.ProblemasDoEndereco(TipoDeConexao.ApiComChave, "http://agro-sistemas-w.tracbel.com.br:5001", null, null, null, null)
            .Should().ContainSingle(p => p.Campo == "endereco", "a chave não vai em claro pela rede");
        Conexao.ProblemasDoEndereco(TipoDeConexao.ApiComChave, "agro-sistemas-w", null, null, null, null)
            .Should().ContainSingle(p => p.Campo == "endereco");
        Conexao.ProblemasDoEndereco(TipoDeConexao.ApiComChave, "https://agro-sistemas-w.tracbel.com.br:5001", null, null, null, null)
            .Should().BeEmpty();

        var gn = Gn();
        gn.Configurar("https://agro-sistemas-w.tracbel.com.br:5001", 443, "banco", "visao", "alguem", "X-Api-Key");

        gn.Usuario.Should().BeNull("a chave é a credencial; não há usuário");
        (gn.Porta, gn.Banco, gn.Objeto, gn.NomeDoCabecalho).Should().Be(((int?)null, (string?)null, (string?)null, (string?)null));
        gn.Endereco.Should().Be("https://agro-sistemas-w.tracbel.com.br:5001");
    }

    [Fact]
    public void A_api_com_chave_so_esta_configurada_com_endereco_e_chave()
    {
        var gn = Gn();
        gn.AceitaSegredo.Should().BeTrue();
        gn.EstaConfiguradaPelaTela.Should().BeFalse();

        gn.Configurar("https://agro-sistemas-w.tracbel.com.br:5001", null, null, null, null, null);
        gn.EstaConfiguradaPelaTela.Should().BeFalse("sem a chave a API manda para a tela de login");

        gn.DefinirSegredo([1, 2, 3], 100, new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc));
        gn.EstaConfiguradaPelaTela.Should().BeTrue();
    }

    [Fact]
    public void Endereco_http_puro_e_recusado_pelo_dominio_antes_de_gravar()
    {
        var gn = Gn();

        var configurar = () => gn.Configurar("http://10.150.4.249:5001", null, null, null, null, null);

        configurar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*https://*");
    }
}
