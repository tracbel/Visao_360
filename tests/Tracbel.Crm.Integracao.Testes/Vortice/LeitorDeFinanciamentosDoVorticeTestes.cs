using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// A LEITURA DOS FINANCIAMENTOS NO VÓRTICE (issue 262): um por processo, do formulário mais novo, e uma consulta que só
/// lê — sem pessoa e sem escrita.
/// </summary>
public sealed class LeitorDeFinanciamentosDoVorticeTestes
{
    private static FinanciamentoNoVortice Resposta(long processo, string formulario, long questionario, DateTime? preenchido = null, decimal valor = 100_000m) =>
        new(processo, formulario, questionario, preenchido ?? new DateTime(2025, 1, 1), new DateTime(2025, 1, 1), valor,
            "SICREDI", "PRONAF", 1, "FRANCA", "SP", "Recebimento", null);

    [Fact]
    public void Um_por_processo_vale_o_formulario_mais_novo()
    {
        // O ACOMPANHAMENTO É CÓPIA DO JDE, e o VENDA_EQUIPAMENTO é o formulário corrente.
        var escolhidos = LeitorDeFinanciamentosDoVortice.UmPorProcesso(
        [
            Resposta(1, "IV_Q_ACOMPANHAMENTO_VENDA", 10),
            Resposta(1, "IV_Q_ACOMPANH_VENDA_JDE", 11),
            Resposta(2, "IV_Q_GESTAO_CREDITO", 20),
            Resposta(2, "IV_Q_VENDA_EQUIPAMENTO", 21),
            Resposta(3, "IV_Q_VENDA", 30),
            Resposta(0, "IV_Q_VENDA", 40)
        ]);

        escolhidos.Should().HaveCount(3, "o processo zero não é processo");
        escolhidos.Single(e => e.Processo == 1).Formulario.Should().Be("IV_Q_ACOMPANH_VENDA_JDE");
        escolhidos.Single(e => e.Processo == 2).Formulario.Should().Be("IV_Q_VENDA_EQUIPAMENTO");
    }

    [Fact]
    public void No_mesmo_formulario_vale_a_resposta_preenchida_por_ultimo()
    {
        var escolhido = LeitorDeFinanciamentosDoVortice.UmPorProcesso(
        [
            Resposta(5, "IV_Q_GESTAO_CREDITO", 50, new DateTime(2023, 1, 1), 10m),
            Resposta(5, "IV_Q_GESTAO_CREDITO", 51, new DateTime(2023, 6, 1), 20m)
        ]).Single();

        escolhido.Valor.Should().Be(20m);
    }

    [Fact]
    public void A_consulta_le_os_cinco_formularios_so_com_select_e_nolock()
    {
        var consulta = LeitorDeFinanciamentosDoVortice.Consulta;

        foreach (var formulario in FinanciamentoDaVenda.PrioridadeDoFormulario)
            consulta.Should().Contain($"FROM {formulario} v WITH (NOLOCK)");

        LeitorDeFinanciamentosDoVortice.Formularios.Select(f => f.Tabela)
            .Should().BeEquivalentTo(FinanciamentoDaVenda.PrioridadeDoFormulario, "a prioridade cita cada formulário lido");

        consulta.Should().NotContainAny("INSERT", "UPDATE ", "DELETE", "EXEC");
        consulta.Should().NotContainAny("NomeRazao", "NroCGCCPF", "Contato", "Email", "Detalhe");
        consulta.Split("JOIN").Skip(1).Should().OnlyContain(j => j.Contains("WITH (NOLOCK)"));
    }
}
