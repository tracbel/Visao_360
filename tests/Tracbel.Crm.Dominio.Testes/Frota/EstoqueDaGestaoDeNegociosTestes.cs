using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Frota;

/// <summary>
/// O ESTOQUE E A COBERTURA DA API GESTÃO DE NEGÓCIOS (28/09/2026): a rotina 12 no catálogo, a máquina do estoque e a
/// cobertura em meses.
/// </summary>
public sealed class EstoqueDaGestaoDeNegociosTestes
{
    private static readonly DateTime Agora = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    private static DadosDoEquipamentoEmEstoque Dados(int empresa = 2, string hash = "h1", string situacao = "Estoque", bool reservado = false) =>
        new(empresa, "1PY6155MKSS012345", "4500012345", "CO1234567", situacao, "TRATOR 6000", "TR 6155M", "CAB 16X16", "MÁQUINA", false,
            "2026/2026", new DateOnly(2026, 6, 1), null, null, true, reservado, null, hash);

    [Fact]
    public void A_rotina_do_estoque_e_a_12a_de_hora_em_hora_nasce_desligada_e_exige_a_gn()
    {
        var todas = RotinasDoSistema.Todas;
        var estoque = todas[11];

        estoque.Codigo.Should().Be(RotinasDoSistema.EstoqueGestaoDeNegocios, "rotina nova entra no fim: a posição é o identificador");
        estoque.Modos.Should().Equal("--somente-estoque-gn");
        estoque.LigadaPorPadrao.Should().BeFalse("trazer dado novo para produção é decisão de quem administra");
        estoque.ConexaoExigida.Should().Be(ConexoesDoSistema.GestaoDeNegocios);
        estoque.AgendaPadrao.Should().Be(AgendaDaRotina.ACada(60), "a reserva e a situação mudam durante o dia");
    }

    [Fact]
    public void A_maquina_so_muda_quando_o_resumo_ou_a_filial_mudam()
    {
        var maquina = EquipamentoEmEstoque.Registrar(1, " 123456 ", Dados(), Agora, 100);

        (maquina.Chaint, maquina.EmpresaId, maquina.EhPedidoDeFabrica).Should().Be(("123456", 2, false));
        maquina.AtualizarDaOrigem(Dados(reservado: true), Agora.AddHours(1), 100).Should().BeFalse("mesmo resumo, nada muda");
        maquina.Reservado.Should().BeFalse();
        maquina.AtualizarDaOrigem(Dados(reservado: true, hash: "h2"), Agora.AddHours(1), 100).Should().BeTrue();
        maquina.Reservado.Should().BeTrue();
        maquina.AtualizarDaOrigem(Dados(empresa: 1, reservado: true, hash: "h2"), Agora.AddHours(2), 100).Should().BeTrue("mudou de filial");
        maquina.EmpresaId.Should().Be(1);
    }

    [Theory]
    [InlineData("PEDIDO", true)]
    [InlineData("pedido", true)]
    [InlineData("Estoque", false)]
    [InlineData("Remessa", false)]
    public void O_pedido_a_fabrica_e_a_situacao_PEDIDO(string situacao, bool pedido) =>
        EquipamentoEmEstoque.EhPedido(situacao).Should().Be(pedido);

    [Fact]
    public void Maquina_sem_chassi_interno_ou_sem_grupo_e_recusada()
    {
        var semChaint = () => EquipamentoEmEstoque.Registrar(1, " ", Dados(), Agora, 100);
        var semGrupo = () => EquipamentoEmEstoque.Registrar(1, "1", Dados() with { Grupo = "" }, Agora, 100);

        semChaint.Should().Throw<RegraDeNegocioViolada>();
        semGrupo.Should().Throw<RegraDeNegocioViolada>().WithMessage("*o grupo*");
    }

    [Fact]
    public void A_cobertura_por_mes_guarda_o_mes_no_dia_1_e_a_por_grupo_nao_guarda_mes()
    {
        var mes = CoberturaDoEstoque.Registrar(1, CoberturaDoEstoque.RecortePorMes, "2026-08", new DateOnly(2026, 8, 1), 3.123m, 152, "h", Agora, null, 100);
        var grupo = CoberturaDoEstoque.Registrar(1, CoberturaDoEstoque.RecortePorGrupo, "AMS", new DateOnly(2026, 8, 1), 3.55m, 512, "h", Agora, null, 100);

        (mes.Competencia, mes.MesesDeEstoque).Should().Be((new DateOnly(2026, 8, 1), 3.12m));
        grupo.Competencia.Should().BeNull("o recorte por grupo é o período inteiro");
        CoberturaDoEstoque.ChaveDoMes(new DateOnly(2026, 8, 1)).Should().Be("2026-08");

        var foraDoDia1 = () => CoberturaDoEstoque.Registrar(1, CoberturaDoEstoque.RecortePorMes, "2026-08", new DateOnly(2026, 8, 15), 1, 1, "h", Agora, null, 100);
        var negativa = () => CoberturaDoEstoque.Registrar(1, CoberturaDoEstoque.RecortePorGrupo, "AMS", null, -1, 1, "h", Agora, null, 100);
        foraDoDia1.Should().Throw<RegraDeNegocioViolada>();
        negativa.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_cobertura_sai_e_volta_sem_ser_apagada()
    {
        var item = CoberturaDoEstoque.Registrar(1, CoberturaDoEstoque.RecortePorGrupo, "AMS", null, 3.55m, 512, "h", Agora, null, 100);

        item.AtualizarDaOrigem(3.6m, 520, "h", Agora, null).Should().BeFalse();
        item.Excluir(Agora);
        item.ExcluidoEm.Should().Be(Agora);
        item.Reativar().Should().BeTrue();
        item.ExcluidoEm.Should().BeNull();
    }
}
