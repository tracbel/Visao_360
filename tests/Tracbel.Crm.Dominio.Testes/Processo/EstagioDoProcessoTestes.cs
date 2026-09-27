using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Processo;

/// <summary>
/// A LINHA DO FUNIL acompanha a origem sem inventar mudança: relida igual, não muda — nem pela data do Vórtice fora do
/// milissegundo —; a chave (processo, estágio) não troca; e o tipo fora de 31/41/50 é recusado.
/// </summary>
public sealed class EstagioDoProcessoTestes
{
    private static readonly DateTime DoVortice = new DateTime(2024, 3, 1, 13, 0, 0, 3, DateTimeKind.Utc).AddTicks(3333);

    private static RetratoDoEstagio Retrato(EstagioDoFunil estagio = EstagioDoFunil.Pedido, long? dna = 10) => new(
        1, 11, 41, null, 5, 7, DoVortice, false, SituacaoDoProcesso.Aberto, null, estagio, DoVortice, null, dna, 3239, false);

    [Fact]
    public void Relida_igual_nao_muda_e_relida_diferente_muda()
    {
        var linha = EstagioDoProcesso.Registrar(Retrato());
        linha.HerdadoDoProcessoDna.Should().BeTrue();
        linha.AlcancadoEm.Ticks.Should().Be(new DateTime(2024, 3, 1, 13, 0, 0, 3, DateTimeKind.Utc).Ticks);

        linha.AtualizarDaOrigem(Retrato()).Should().BeFalse("o datetime do Vórtice tem passo de 1/300 s; a coluna, milissegundo");
        linha.AtualizarDaOrigem(Retrato() with { ClienteId = 99 }).Should().BeTrue();
        linha.ClienteId.Should().Be(99);

        linha.AtualizarDaOrigem(Retrato() with { ClienteId = 99, NumeroDoProcessoDnaNaOrigem = null }).Should().BeTrue();
        linha.HerdadoDoProcessoDna.Should().BeFalse();
    }

    [Fact]
    public void A_ligacao_ao_processo_da_onda_2_fica_fora_do_retrato_que_o_funil_compara()
    {
        var linha = EstagioDoProcesso.Registrar(Retrato());
        var antes = linha.Retrato;

        linha.LigarAoProcesso(42).Should().BeTrue();
        linha.LigarAoProcesso(42).Should().BeFalse("ligada igual não muda");
        linha.ProcessoId.Should().Be(42);
        linha.Retrato.Should().Be(antes, "o funil não enxerga a ligação — senão toda rodada do funil a desfaria");
        linha.AtualizarDaOrigem(Retrato()).Should().BeFalse();

        linha.LigarAoProcesso(null).Should().BeTrue("o processo saiu do CRM");
        linha.ProcessoId.Should().BeNull();
    }

    [Fact]
    public void A_chave_nao_troca_e_o_tipo_fora_do_funil_e_recusado()
    {
        var linha = EstagioDoProcesso.Registrar(Retrato());

        FluentActions.Invoking(() => linha.AtualizarDaOrigem(Retrato(EstagioDoFunil.Faturamento))).Should().Throw<RegraDeNegocioViolada>();
        FluentActions.Invoking(() => EstagioDoProcesso.Registrar(Retrato() with { TipoDeProcessoNaOrigem = 12 }))
            .Should().Throw<RegraDeNegocioViolada>();
    }
}
