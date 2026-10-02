using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Comercial;

/// <summary>
/// AS PEÇAS DO PROTHEUS (02/10/2026): a apuração do faturamento no dia 1 do mês e com pelo menos um item, e o orçamento que
/// espelha a origem — em aberto quando aberto ou parcialmente atendido, sem mudança quando nada mudou, reativado quando volta.
/// </summary>
public sealed class PecasDoProtheusTestes
{
    private static readonly DateTime Agora = new(2026, 10, 2, 9, 0, 0, DateTimeKind.Utc);

    private static DadosDoFaturamentoDePecas Faturamento(DateOnly? mes = null, int itens = 3) =>
        new(1, mes ?? new DateOnly(2026, 9, 1), 10, "BALCAO", "PECAS", "PECAS JD", "000123", "VENDEDOR FICTICIO",
            4.5m, 1234.567m, -10m, 50m, 1300m, itens);

    private static DadosDoOrcamentoDePecas Orcamento(string situacao = "Aberto", long? cliente = 10, string hash = "h1") =>
        new(1, "000777", cliente, situacao, "NO PRAZO", "RESERVADO", "BALCAO", "VENDA", new DateOnly(2026, 9, 20), new DateOnly(2026, 10, 20),
            null, "000123", "VENDEDOR FICTICIO", 5000m, 100m, 4, hash);

    [Fact]
    public void A_apuracao_guarda_o_mes_no_dia_1_e_arredonda_em_centavos()
    {
        var linha = FaturamentoDePecasNoMes.Apurar(3, Faturamento(), Agora, 100);

        (linha.EmpresaId, linha.Competencia, linha.ClienteId, linha.Setor, linha.Grupo).Should().Be((1, new DateOnly(2026, 9, 1), (long?)10, "BALCAO", "PECAS"));
        linha.ValorLiquido.Should().Be(1234.57m);
        linha.ValorDeDevolucoes.Should().Be(-10m);
    }

    [Fact]
    public void Mes_fora_do_dia_1_e_combinacao_sem_item_sao_recusados()
    {
        var foraDoDia1 = () => FaturamentoDePecasNoMes.Apurar(3, Faturamento(new DateOnly(2026, 9, 15)), Agora, 100);
        var semItem = () => FaturamentoDePecasNoMes.Apurar(3, Faturamento(itens: 0), Agora, 100);

        foraDoDia1.Should().Throw<RegraDeNegocioViolada>();
        semItem.Should().Throw<RegraDeNegocioViolada>();
    }

    [Theory]
    [InlineData("Aberto", true)]
    [InlineData("Parcialmente Atendido", true)]
    [InlineData("Encerrado", false)]
    [InlineData(null, false)]
    public void Em_aberto_e_o_aberto_e_o_parcialmente_atendido(string? situacao, bool emAberto) =>
        OrcamentoDePecas.EstaEmAbertoNa(situacao).Should().Be(emAberto);

    [Fact]
    public void O_orcamento_nao_muda_com_a_mesma_leitura_e_muda_com_o_cliente_casado_depois()
    {
        var orcamento = OrcamentoDePecas.Registrar(3, "010101|000777", Orcamento(cliente: null), Agora, 100);

        orcamento.AtualizarDaOrigem(Orcamento(cliente: null), Agora.AddDays(1), 100).Should().BeFalse();
        orcamento.AtualizarDaOrigem(Orcamento(cliente: 10), Agora.AddDays(2), 100).Should().BeTrue();
        orcamento.ClienteId.Should().Be(10);
        orcamento.EstaEmAberto.Should().BeTrue();
    }

    [Fact]
    public void O_orcamento_que_sumiu_e_voltou_e_reativado_e_o_sem_item_e_recusado()
    {
        var orcamento = OrcamentoDePecas.Registrar(3, "010101|000777", Orcamento(), Agora, 100);
        orcamento.Excluir(100);
        orcamento.Reativar(100).Should().BeTrue();
        orcamento.EstaExcluido.Should().BeFalse();

        var semItem = () => OrcamentoDePecas.Registrar(3, "010101|000778", Orcamento() with { Itens = 0 }, Agora, 100);
        semItem.Should().Throw<RegraDeNegocioViolada>();
    }
}
