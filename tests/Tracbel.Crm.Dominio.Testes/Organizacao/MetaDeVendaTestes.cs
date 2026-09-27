using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// A META DE VENDA DA API GESTÃO DE NEGÓCIOS (#138): o mês no dia 1, a quantidade não negativa, a releitura igual que
/// não muda nada, a revisão que muda, e a meta que some e volta na MESMA linha. E a matriz de visibilidade da D-M5.
/// </summary>
public sealed class MetaDeVendaTestes
{
    private static readonly LeituraDaMeta Leitura = new(new DateTime(2026, 9, 27, 9, 0, 0, DateTimeKind.Utc), null, null);

    private static DadosDaMetaNaOrigem Dados(int quantidade = 3, DateOnly? competencia = null, string hash = "h1", long? consultor = 100) => new(
        1, competencia ?? new DateOnly(2025, 11, 1), "TRATOR MÉDIO", "TRATOR_MEDIO", 2, "FULANO.DE.TAL", consultor, false,
        OrigemDaMeta.Campanha, quantidade, 450_000m, 0.12m, hash);

    [Fact]
    public void A_meta_nasce_com_o_retrato_da_origem_e_a_leitura_que_o_trouxe()
    {
        var meta = MetaDeVenda.Registrar(7, 1540, Dados(), Leitura, 1);

        (meta.SistemaId, meta.IdNaOrigem, meta.EmpresaId).Should().Be((7, 1540, 1));
        meta.Competencia.Should().Be(new DateOnly(2025, 11, 1));
        meta.Quantidade.Should().Be(3);
        meta.ImportadaEm.Should().Be(Leitura.LidaEm);
        meta.LidaEm.Should().Be(Leitura.LidaEm);
        meta.AtualizadaPelaOrigemEm.Should().BeNull();
        meta.EstaExcluido.Should().BeFalse();
    }

    [Fact]
    public void A_competencia_e_o_mes_no_dia_1()
    {
        var registrar = () => MetaDeVenda.Registrar(7, 1, Dados(competencia: new DateOnly(2025, 11, 15)), Leitura, 1);

        registrar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*dia 1*");
    }

    [Fact]
    public void A_meta_nao_e_negativa()
    {
        var registrar = () => MetaDeVenda.Registrar(7, 1, Dados(quantidade: -1), Leitura, 1);

        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_releitura_igual_nao_muda_nada_e_a_revisao_muda()
    {
        var meta = MetaDeVenda.Registrar(7, 1, Dados(), Leitura, 1);
        var depois = Leitura with { LidaEm = Leitura.LidaEm.AddDays(1) };

        meta.AtualizarDaOrigem(Dados(), depois, 1).Should().BeFalse("a segunda leitura igual grava zero");
        meta.LidaEm.Should().Be(Leitura.LidaEm);

        meta.AtualizarDaOrigem(Dados(quantidade: 5, hash: "h2"), depois, 1).Should().BeTrue();
        meta.Quantidade.Should().Be(5);
        meta.AtualizadaPelaOrigemEm.Should().Be(depois.LidaEm);
        meta.LidaEm.Should().Be(depois.LidaEm);
    }

    [Fact]
    public void A_conta_do_consultor_que_passa_a_existir_muda_a_meta_sem_a_origem_mudar()
    {
        var meta = MetaDeVenda.Registrar(7, 1, Dados(consultor: null), Leitura, 1);

        meta.AtualizarDaOrigem(Dados(consultor: 100), Leitura, 1).Should().BeTrue("a pessoa ganhou conta no CRM depois da primeira leitura");
        meta.ConsultorUsuarioId.Should().Be(100);
    }

    [Fact]
    public void A_meta_que_some_e_excluida_e_a_que_volta_e_reativada_na_mesma_linha()
    {
        var meta = MetaDeVenda.Registrar(7, 1, Dados(), Leitura, 1);

        meta.Excluir(1);
        meta.EstaExcluido.Should().BeTrue();

        meta.Reativar(1).Should().BeTrue();
        meta.EstaExcluido.Should().BeFalse();
        meta.Reativar(1).Should().BeFalse("reativar a ativa não muda nada");
    }

    [Fact]
    public void A_matriz_de_visibilidade_da_meta_e_a_da_D_M5_nas_linhas_118_428_604_e_706()
    {
        var linhas = PerfisDeSistema.LinhasDaSemente().Where(l => l.Codigo == Permissoes.MetaLer).ToList();

        linhas.Select(l => (l.Id, l.Profundidade)).Should().BeEquivalentTo(
        [
            (118, Profundidade.Proprios),
            (428, Profundidade.Organizacao),
            (604, Profundidade.EmpresaEAbaixo),
            (706, Profundidade.EmpresaEAbaixo)
        ], "o padrão vê a própria meta, a gerência e a diretoria a filial, o administrador tudo; o gestor comercial fica sem");
    }

    [Theory]
    [InlineData("FULANO.DE.TAL", "FULANO.DE.TAL")]
    [InlineData("fulano.de.tal", "FULANO.DE.TAL")]
    [InlineData("Fulano de Tal", "FULANO.DE.TAL")]
    [InlineData("  FULANO-DE  TAL ", "FULANO.DE.TAL")]
    [InlineData("joão.conceição", "JOAO.CONCEICAO")]
    [InlineData("José - Antônio", "JOSE.ANTONIO")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void A_chave_da_pessoa_tira_acento_e_troca_espaco_e_hifen_por_ponto(string? texto, string chave) =>
        MetaDeVenda.ChaveDaPessoa(texto).Should().Be(chave, "o consultor da GN, o vendedor do ART e o login passam pela mesma chave");
}
