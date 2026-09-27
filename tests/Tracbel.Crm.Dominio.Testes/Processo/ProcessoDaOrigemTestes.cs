using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Processo;

/// <summary>
/// O PROCESSO DA ONDA 2 acompanha o Vórtice sem inventar mudança (documento 52 §12). O que estes testes prendem:
/// <list type="bullet">
/// <item>relido igual não muda — nem pela data do Vórtice fora do milissegundo —, e relido diferente muda e carimba;</item>
/// <item>o número é a identidade: outro número é outro processo;</item>
/// <item>as regras do banco valem para a origem: encerrado tem data, perdido tem motivo, título não é vazio;</item>
/// <item>a abertura na origem vira a criação, e o processo que volta à origem é restaurado.</item>
/// </list>
/// </summary>
public sealed class ProcessoDaOrigemTestes
{
    private static readonly DateTime DoVortice = new DateTime(2024, 3, 1, 13, 0, 0, 3, DateTimeKind.Utc).AddTicks(3333);

    private static RetratoDoProcessoDaOrigem Retrato(SituacaoDoProcesso situacao = SituacaoDoProcesso.Aberto) => new(
        123456, 1, 2, 7, 18, "  Trator 6M para a safra  ", 3, DoVortice, situacao, DoVortice, null, null, null,
        Dinheiro.Criar(480_000m), 1m, new DateOnly(2024, 6, 30), new DateOnly(2024, 5, 31), 5, DoVortice);

    [Fact]
    public void Relido_igual_nao_muda_e_relido_diferente_muda_e_carimba()
    {
        var processo = Dominio.Processo.Processo.DaOrigem(Retrato(), criadoPorId: 1);

        processo.Numero.Should().Be(123456, "o número é o do Vórtice");
        processo.Titulo.Should().Be("Trator 6M para a safra");
        processo.CriadoEm.Ticks.Should().Be(new DateTime(2024, 3, 1, 13, 0, 0, 3, DateTimeKind.Utc).Ticks, "a abertura na origem vira a criação");
        processo.Descricao.Should().BeNull("a descrição do Vórtice não entra (decisão P1)");

        processo.AtualizarDaOrigem(Retrato(), 1).Should().BeFalse("o datetime do Vórtice tem passo de 1/300 s; a coluna, milissegundo");
        processo.AlteradoEm.Should().BeNull();

        processo.AtualizarDaOrigem(Retrato() with { ProprietarioId = 9, CarteiraId = null }, 1).Should().BeTrue();
        processo.ProprietarioId.Should().Be(9);
        processo.CarteiraId.Should().BeNull();
        processo.AlteradoEm.Should().NotBeNull();
    }

    [Fact]
    public void O_numero_nao_troca()
    {
        var processo = Dominio.Processo.Processo.DaOrigem(Retrato(), 1);
        FluentActions.Invoking(() => processo.AtualizarDaOrigem(Retrato() with { Numero = 999 }, 1)).Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Encerrado_tem_data_perdido_tem_motivo_e_o_titulo_nao_e_vazio()
    {
        FluentActions.Invoking(() => Dominio.Processo.Processo.DaOrigem(Retrato(SituacaoDoProcesso.Ganho), 1))
            .Should().Throw<RegraDeNegocioViolada>("CK_Processo_Encerramento");
        FluentActions.Invoking(() => Dominio.Processo.Processo.DaOrigem(Retrato(SituacaoDoProcesso.Perdido) with { ConcluidoEm = DoVortice }, 1))
            .Should().Throw<RegraDeNegocioViolada>("CK_Processo_MotivoDePerda");
        FluentActions.Invoking(() => Dominio.Processo.Processo.DaOrigem(Retrato() with { Titulo = "   " }, 1))
            .Should().Throw<RegraDeNegocioViolada>();

        var perdido = Dominio.Processo.Processo.DaOrigem(
            Retrato(SituacaoDoProcesso.Perdido) with { ConcluidoEm = DoVortice, MotivoDePerdaId = 4, ConcorrenteId = 11 }, 1);
        perdido.Should().BeEquivalentTo(new { MotivoDePerdaId = (int?)4, ConcorrenteId = (int?)11, Situacao = SituacaoDoProcesso.Perdido },
            o => o.ExcludingMissingMembers());

        // O PROCESSO QUE A ORIGEM REABRIU perde a data de encerramento junto.
        perdido.AtualizarDaOrigem(Retrato(), 1).Should().BeTrue();
        perdido.ConcluidoEm.Should().BeNull();
        perdido.MotivoDePerdaId.Should().BeNull();
    }

    [Fact]
    public void O_processo_que_volta_a_origem_e_restaurado()
    {
        var processo = Dominio.Processo.Processo.DaOrigem(Retrato(), 1);
        processo.RestaurarDaOrigem(1).Should().BeFalse("não estava excluído");

        processo.Excluir(1);
        processo.RestaurarDaOrigem(1).Should().BeTrue();
        processo.EstaExcluido.Should().BeFalse();
    }
}
