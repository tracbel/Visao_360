using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// AS FAIXAS DE DIAS DESDE O ÚLTIMO CONTATO do Dimensionamento (issue 259): acumuladas, com o dia N dentro dos N dias, e o
/// "A/B em risco" contando só A e B sem contato em 120 dias.
/// </summary>
public sealed class CarteiraDoDimensionamentoTestes
{
    private static readonly DateTime Agora = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void As_faixas_sao_acumuladas_e_o_dia_exato_fica_dentro()
    {
        var contagem = ClientesDaCarteira.Contar(
        [
            (ClasseDeCliente.A, Agora.AddDays(-30)),
            (ClasseDeCliente.B, Agora.AddDays(-60)),
            (ClasseDeCliente.C, Agora.AddDays(-90)),
            (ClasseDeCliente.D, Agora.AddDays(-120)),
            (null, Agora.AddDays(-121))
        ], Agora);

        contagem.Faixas.Should().Be(new FaixasDoUltimoContato(Ate30: 1, Ate60: 2, Ate90: 3, Ate120: 4, Sem120: 1));
        (contagem.A, contagem.B, contagem.C, contagem.D, contagem.SemClasse).Should().Be((1, 1, 1, 1, 1));
        contagem.UltimoContatoEm.Should().Be(Agora.AddDays(-30));
    }

    [Fact]
    public void AB_sem_contato_conta_so_A_e_B_sem_contato_em_120_dias()
    {
        var contagem = ClientesDaCarteira.Contar(
        [
            (ClasseDeCliente.A, null),
            (ClasseDeCliente.B, Agora.AddDays(-200)),
            (ClasseDeCliente.B, Agora.AddDays(-10)),
            (ClasseDeCliente.C, null),
            (null, null)
        ], Agora);

        contagem.ABSemContato.Should().Be(2, "o C e o sem classe sem contato não são A/B em risco");
        contagem.Faixas.Sem120.Should().Be(4);
    }

    [Fact]
    public void Carteira_vazia_nao_tem_ultimo_contato()
    {
        ClientesDaCarteira.Contar([], Agora).Should().Be(ClientesDaCarteira.Nenhum);
    }
}
