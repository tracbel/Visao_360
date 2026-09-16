using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Integracao;

/// <summary>O registro de cada ciclo do serviço de sincronização (documento 35, seção 11).</summary>
public sealed class ExecucaoDeSincronizacaoTestes
{
    private static readonly DateTime Inicio = new(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Comeca_em_andamento_e_termina_com_as_contagens_do_sucesso()
    {
        var execucao = ExecucaoDeSincronizacao.Iniciar(1, "ART.VENDA_DE_MAQUINA", "SERVIDOR", Inicio);
        execucao.Resultado.Should().Be(ResultadoDaExecucao.EmAndamento);
        execucao.TerminadaEm.Should().BeNull();

        execucao.Concluir(2, 4144, 10, 3, 2036, "resumo", Inicio.AddMinutes(4));

        execucao.Resultado.Should().Be(ResultadoDaExecucao.Sucesso);
        execucao.Tentativas.Should().Be(2);
        execucao.RegistrosLidos.Should().Be(4144);
        execucao.Incluidos.Should().Be(10);
        execucao.Atualizados.Should().Be(3);
        execucao.Pendentes.Should().Be(2036);
        execucao.TerminadaEm.Should().Be(Inicio.AddMinutes(4));
    }

    [Fact]
    public void A_falha_guarda_o_motivo_cortado_no_tamanho_da_coluna()
    {
        var execucao = ExecucaoDeSincronizacao.Iniciar(1, "ART.VENDA_DE_MAQUINA", "SERVIDOR", Inicio);
        execucao.Falhar(3, new string('x', 5000), Inicio.AddMinutes(1));

        execucao.Resultado.Should().Be(ResultadoDaExecucao.Falha);
        execucao.Mensagem!.Length.Should().Be(1000);
        execucao.Mensagem.Should().EndWith("...");
    }

    [Fact]
    public void A_execucao_ignorada_nao_conta_tentativa_e_o_nome_da_maquina_cabe_na_coluna()
    {
        var execucao = ExecucaoDeSincronizacao.Iniciar(1, "ART.VENDA_DE_MAQUINA", new string('M', 90), Inicio);
        execucao.Ignorar("outra em andamento", Inicio);

        execucao.Resultado.Should().Be(ResultadoDaExecucao.Ignorada);
        execucao.Tentativas.Should().Be(0);
        execucao.Maquina.Length.Should().Be(60);
    }
}
