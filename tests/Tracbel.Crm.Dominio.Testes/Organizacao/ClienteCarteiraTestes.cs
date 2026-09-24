using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O ENCERRAMENTO DO VÍNCULO CLIENTE × CARTEIRA — o que a sincronia das carteiras do Vórtice faz quando a origem deixa
/// de declarar um vínculo ou o cliente muda de carteira.
/// </summary>
public sealed class ClienteCarteiraTestes
{
    private static readonly DateTime Entrou = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Desvincular_encerra_sem_apagar_e_guarda_a_primeira_data()
    {
        var vinculo = ClienteCarteira.Criar(10, 20, ClasseDeCliente.C, vinculadoPorId: 1, vinculadoEmUtc: Entrou);
        var saiu = Entrou.AddDays(20);

        vinculo.Desvincular(saiu);
        vinculo.Desvincular(saiu.AddDays(1));

        vinculo.DesvinculadoEm.Should().Be(saiu,
            "rodar a sincronia de novo não pode empurrar a data de saída: ela é quando o cliente saiu, não quando alguém conferiu");
        vinculo.VinculadoEm.Should().Be(Entrou);
        vinculo.DiasCicloContato.Should().BeNull("a cadência vem da linha de negócio (issue 53), não do vínculo");
    }
}
