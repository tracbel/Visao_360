using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Processo;

/// <summary>
/// A TAREFA DA ONDA 2 espelha a agenda do Vórtice (documento 52 §12). O que estes testes prendem:
/// <list type="bullet">
/// <item>relida igual não muda — nem pela data fora do milissegundo —; reprogramada e concluída na origem, acompanha;</item>
/// <item>concluída tem data, quem concluiu e desfecho (CK_Tarefa_Conclusao); a data é UTC; a prioridade vai de 1 a 5;</item>
/// <item>o duplo ponteiro: a interação de conclusão só vale para a concluída, e reabrir a tarefa a desliga;</item>
/// <item>a tarefa que volta à origem é restaurada.</item>
/// </list>
/// </summary>
public sealed class TarefaDaOrigemTestes
{
    private static readonly DateTime DoVortice = new DateTime(2024, 4, 2, 12, 0, 0, 7, DateTimeKind.Utc).AddTicks(3333);

    private static RetratoDaTarefaDaOrigem Pendente() => new(
        1, 50, 7, 3, " Visita de apresentação ", 5, OrigemDaAtribuicao.Manual, DoVortice, null, 3, SituacaoDaTarefa.Pendente,
        null, null, null, DoVortice);

    private static RetratoDaTarefaDaOrigem Concluida() =>
        Pendente() with { Situacao = SituacaoDaTarefa.Concluida, ConcluidaEm = DoVortice.AddDays(1), ConcluidaPorId = 5, ResultadoId = 8 };

    [Fact]
    public void Relida_igual_nao_muda_e_a_agenda_que_andou_na_origem_acompanha()
    {
        var tarefa = Tarefa.DaOrigem(Pendente(), criadoPorId: 1);
        tarefa.Assunto.Should().Be("Visita de apresentação");
        tarefa.CriadoEm.Ticks.Should().Be(new DateTime(2024, 4, 2, 12, 0, 0, 7, DateTimeKind.Utc).Ticks);

        tarefa.AtualizarDaOrigem(Pendente(), 1).Should().BeFalse("o datetime do Vórtice tem passo de 1/300 s; a coluna, milissegundo");

        tarefa.AtualizarDaOrigem(Pendente() with { AgendadaPara = DoVortice.AddDays(7) }, 1).Should().BeTrue("reprogramada na origem");
        tarefa.AtualizarDaOrigem(Concluida(), 1).Should().BeTrue("concluída na origem — sem recusar como o Concluir da tela");
        tarefa.Should().BeEquivalentTo(new { Situacao = SituacaoDaTarefa.Concluida, ConcluidaPorId = (long?)5, ResultadoId = (int?)8 },
            o => o.ExcludingMissingMembers());
        tarefa.AtualizarDaOrigem(Concluida(), 1).Should().BeFalse();
    }

    [Fact]
    public void Concluida_tem_data_quem_concluiu_e_desfecho_a_data_e_UTC_e_a_prioridade_vai_de_1_a_5()
    {
        FluentActions.Invoking(() => Tarefa.DaOrigem(Concluida() with { ResultadoId = null }, 1)).Should().Throw<RegraDeNegocioViolada>();
        FluentActions.Invoking(() => Tarefa.DaOrigem(Concluida() with { ConcluidaPorId = null }, 1)).Should().Throw<RegraDeNegocioViolada>();
        FluentActions.Invoking(() => Tarefa.DaOrigem(Pendente() with { AgendadaPara = DateTime.SpecifyKind(DoVortice, DateTimeKind.Unspecified) }, 1))
            .Should().Throw<RegraDeNegocioViolada>();
        FluentActions.Invoking(() => Tarefa.DaOrigem(Pendente() with { Prioridade = 0 }, 1)).Should().Throw<RegraDeNegocioViolada>();
        FluentActions.Invoking(() => Tarefa.DaOrigem(Pendente() with { Assunto = " " }, 1)).Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void O_duplo_ponteiro_so_liga_a_conclusao_da_concluida_e_reabrir_desliga()
    {
        var pendente = Tarefa.DaOrigem(Pendente(), 1);
        pendente.LigarInteracoes(10, 11, 1).Should().BeTrue();
        pendente.InteracaoOrigemId.Should().Be(10);
        pendente.InteracaoConclusaoId.Should().BeNull("pendente não tem quem a tenha concluído");

        var concluida = Tarefa.DaOrigem(Concluida(), 1);
        concluida.LigarInteracoes(10, 11, 1).Should().BeTrue();
        concluida.LigarInteracoes(10, 11, 1).Should().BeFalse("ligada igual não muda");
        concluida.InteracaoConclusaoId.Should().Be(11);

        concluida.AtualizarDaOrigem(Pendente(), 1).Should().BeTrue("a origem reabriu a tarefa");
        concluida.InteracaoConclusaoId.Should().BeNull();
        concluida.InteracaoOrigemId.Should().Be(10, "quem gerou a tarefa continua sendo quem gerou");
    }

    [Fact]
    public void A_tarefa_que_volta_a_origem_e_restaurada()
    {
        var tarefa = Tarefa.DaOrigem(Pendente(), 1);
        tarefa.Excluir(1);
        tarefa.RestaurarDaOrigem(1).Should().BeTrue();
        tarefa.RestaurarDaOrigem(1).Should().BeFalse();
    }
}
