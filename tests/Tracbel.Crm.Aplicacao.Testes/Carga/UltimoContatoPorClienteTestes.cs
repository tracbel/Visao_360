using FluentAssertions;
using Tracbel.Crm.Carga;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A AGREGAÇÃO DO ÚLTIMO CONTATO POR CLIENTE DO CRM (decisão de 27/09/2026: qualquer contato, como a BI do Vórtice) —
/// sem banco: quem chega ao cliente, qual data vale, e a data do futuro que não pode travar o vínculo.
/// </summary>
public sealed class UltimoContatoPorClienteTestes
{
    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private static UltimoContatoDosClientes Agregar(
        IEnumerable<(long SeqPessoa, long ClienteId)> casados, Dictionary<long, DateTime> contatos) =>
        CargaDeCarteirasDoVortice.UltimoContatoPorCliente(casados, contatos, Agora);

    [Fact]
    public void Varias_pessoas_do_Vortice_no_mesmo_cliente_vale_a_mais_recente()
    {
        var resultado = Agregar(
            [(1, 100), (2, 100), (3, 100), (4, 200)],
            new Dictionary<long, DateTime>
            {
                [1] = Agora.AddDays(-40), [2] = Agora.AddDays(-3), [3] = Agora.AddDays(-400), [4] = Agora.AddDays(-10)
            });

        resultado.PorCliente.Should().Equal(new Dictionary<long, DateTime> { [100] = Agora.AddDays(-3), [200] = Agora.AddDays(-10) });
        resultado.ClientesComMaisDeUmaPessoa.Should().Be(1);
    }

    [Fact]
    public void So_chega_ao_cliente_a_pessoa_cujo_vinculo_casou_e_quem_nao_tem_contato_fica_de_fora()
    {
        var resultado = Agregar(
            [(1, 100), (2, 200)],
            new Dictionary<long, DateTime> { [1] = Agora.AddDays(-5), [9] = Agora.AddDays(-1) });

        resultado.PorCliente.Keys.Should().Equal([100L],
            "a pessoa 9 tem contato mas não casou; a 2 casou mas nunca foi contatada — cliente sem data é 'nunca'");
        resultado.ClientesComMaisDeUmaPessoa.Should().Be(0);
    }

    [Fact]
    public void A_data_mais_de_um_dia_no_futuro_e_recusada_e_contada()
    {
        var resultado = Agregar(
            [(1, 100), (2, 200)],
            new Dictionary<long, DateTime> { [1] = Agora.AddDays(1).AddMinutes(1), [2] = new(2062, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        resultado.PorCliente.Should().BeEmpty("com a regra de só avançar, uma data futura travaria o vínculo para sempre");
        resultado.PessoasComDataNoFuturo.Should().Be(2);
    }

    [Fact]
    public void A_data_futura_de_uma_pessoa_nao_esconde_o_contato_real_de_outra_do_mesmo_cliente()
    {
        var resultado = Agregar(
            [(1, 100), (2, 100)],
            new Dictionary<long, DateTime> { [1] = Agora.AddDays(30), [2] = Agora.AddDays(-2) });

        resultado.PorCliente[100].Should().Be(Agora.AddDays(-2));
        resultado.PessoasComDataNoFuturo.Should().Be(1);
    }

    [Fact]
    public void Ate_um_dia_a_frente_ainda_vale_pela_folga_de_relogio_e_fuso()
    {
        var resultado = Agregar([(1, 100)], new Dictionary<long, DateTime> { [1] = Agora.AddHours(20) });

        resultado.PorCliente[100].Should().Be(Agora.AddHours(20));
        resultado.PessoasComDataNoFuturo.Should().Be(0);
    }

    [Fact]
    public void A_data_vai_ao_milissegundo_da_coluna_para_nao_avancar_sozinha_a_cada_rodada()
    {
        // O datetime do Vórtice guarda 1/300 s: 12:00:00,0066667. A coluna do CRM é datetime2(3).
        var doVortice = Agora.AddDays(-1).AddTicks(66_667);

        var resultado = Agregar([(1, 100)], new Dictionary<long, DateTime> { [1] = doVortice });

        resultado.PorCliente[100].Should().Be(Agora.AddDays(-1).AddMilliseconds(6));
        resultado.PorCliente[100].Kind.Should().Be(DateTimeKind.Utc);
    }
}
