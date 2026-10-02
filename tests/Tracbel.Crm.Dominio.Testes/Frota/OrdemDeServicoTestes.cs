using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Frota;

/// <summary>
/// A ORDEM DE SERVIÇO DO PROTHEUS (02/10/2026): a situação pela letra da capa (o D antigo é o L), a chave da filial e do
/// número, o espelho que não muda quando nada mudou — nem quando o casamento é o mesmo —, e os dias em aberto só para a
/// OS que ainda está na oficina.
/// </summary>
public sealed class OrdemDeServicoTestes
{
    private static readonly DateTime LidaEm = new(2026, 10, 2, 9, 0, 0, DateTimeKind.Utc);

    private static DadosDaOrdemDeServico Dados(
        SituacaoDaOrdemDeServico situacao = SituacaoDaOrdemDeServico.Aberta, long? clienteId = 10, long? equipamentoId = 20,
        decimal? horimetro = 1200m, string hash = "h1", string numero = "00001234", decimal pecas = 100.555m) =>
        new(1, numero, clienteId, equipamentoId, "1PY6155MCSS000001", "6155M", horimetro, situacao, "OFICINA",
            new DateOnly(2026, 8, 1), null, null, null, pecas, 50m, 3, 2, hash);

    [Theory]
    [InlineData("A", SituacaoDaOrdemDeServico.Aberta)]
    [InlineData("L", SituacaoDaOrdemDeServico.Liberada)]
    [InlineData("D", SituacaoDaOrdemDeServico.Liberada)]
    [InlineData(" f ", SituacaoDaOrdemDeServico.Fechada)]
    [InlineData("C", SituacaoDaOrdemDeServico.Cancelada)]
    public void A_situacao_vem_da_letra_da_capa_e_o_D_e_o_L_antigo(string letra, SituacaoDaOrdemDeServico esperada) =>
        OrdemDeServico.SituacaoPelaLetra(letra).Should().Be(esperada);

    [Theory]
    [InlineData("X")]
    [InlineData("")]
    [InlineData(null)]
    public void Letra_desconhecida_nao_vira_situacao(string? letra) =>
        OrdemDeServico.SituacaoPelaLetra(letra).Should().BeNull("a carga a conta como ilegível em vez de adivinhar");

    [Fact]
    public void A_chave_e_a_filial_e_o_numero_aparados() =>
        OrdemDeServico.ChaveDe(" 010101 ", "00001234 ").Should().Be("010101|00001234");

    [Fact]
    public void Registra_com_os_valores_arredondados_em_centavos()
    {
        var ordem = OrdemDeServico.Registrar(3, "010101|00001234", Dados(), LidaEm, 100);

        (ordem.SistemaId, ordem.ChaveNaOrigem, ordem.Numero, ordem.EmpresaId, ordem.ClienteId, ordem.EquipamentoId)
            .Should().Be((3, "010101|00001234", "00001234", 1, (long?)10, (long?)20));
        ordem.ValorDePecas.Should().Be(100.56m);
        ordem.ValorTotal.Should().Be(150.56m);
        ordem.EstaEmAberto.Should().BeTrue();
    }

    [Fact]
    public void Chave_numero_e_horimetro_fora_da_regra_sao_recusados()
    {
        var semChave = () => OrdemDeServico.Registrar(3, " ", Dados(), LidaEm, 100);
        var numeroLongo = () => OrdemDeServico.Registrar(3, "010101|1", Dados(numero: new string('9', 21)), LidaEm, 100);
        var horimetroNegativo = () => OrdemDeServico.Registrar(3, "010101|1", Dados(horimetro: -1m), LidaEm, 100);

        semChave.Should().Throw<RegraDeNegocioViolada>();
        numeroLongo.Should().Throw<RegraDeNegocioViolada>();
        horimetroNegativo.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_mesma_leitura_nao_muda_nada_e_o_cliente_casado_depois_muda()
    {
        var ordem = OrdemDeServico.Registrar(3, "010101|00001234", Dados(clienteId: null), LidaEm, 100);

        ordem.AtualizarDaOrigem(Dados(clienteId: null), LidaEm.AddDays(1), 100).Should().BeFalse();
        ordem.AlteradoEm.Should().BeNull();

        ordem.AtualizarDaOrigem(Dados(clienteId: 10), LidaEm.AddDays(2), 100).Should().BeTrue("o cliente cadastrado depois da OS a casa");
        ordem.ClienteId.Should().Be(10);
        ordem.AlteradoPorId.Should().Be(100);
    }

    [Fact]
    public void A_que_sumiu_e_voltou_e_reativada_na_mesma_linha()
    {
        var ordem = OrdemDeServico.Registrar(3, "010101|00001234", Dados(), LidaEm, 100);
        ordem.Excluir(100);

        ordem.Reativar(100).Should().BeTrue();
        ordem.EstaExcluido.Should().BeFalse();
        ordem.Reativar(100).Should().BeFalse("a que está ativa não muda");
    }

    [Fact]
    public void Os_dias_em_aberto_so_existem_para_a_OS_na_oficina()
    {
        var aberta = OrdemDeServico.Registrar(3, "010101|1", Dados(numero: "1"), LidaEm, 100);
        var fechada = OrdemDeServico.Registrar(3, "010101|2", Dados(numero: "2", situacao: SituacaoDaOrdemDeServico.Fechada), LidaEm, 100);

        aberta.DiasEmAberto(new DateOnly(2026, 9, 15)).Should().Be(45);
        fechada.DiasEmAberto(new DateOnly(2026, 9, 15)).Should().BeNull();
        fechada.EstaEmAberto.Should().BeFalse();
    }
}
