using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O ENCERRAMENTO DO VÍNCULO CLIENTE × CARTEIRA — o que a sincronia das carteiras do Vórtice faz quando a origem deixa
/// de declarar um vínculo ou o cliente muda de carteira — e o ÚLTIMO CONTATO, que só anda para a frente.
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

    // O ÚLTIMO CONTATO SÓ ANDA PARA A FRENTE (decisão de 27/09/2026): a sincronia das carteiras do Vórtice e a gravação
    // da interação chegam ao mesmo campo, e o que vale é o máximo das duas — em qualquer ordem.

    [Fact]
    public void O_primeiro_contato_preenche_a_data_que_estava_vazia()
    {
        var vinculo = ClienteCarteira.Criar(10, 20, ClasseDeCliente.C, vinculadoPorId: 1, vinculadoEmUtc: Entrou);
        vinculo.UltimaInteracaoEm.Should().BeNull("vínculo novo nunca foi contatado");

        vinculo.RegistrarInteracao(Entrou.AddDays(3));

        vinculo.UltimaInteracaoEm.Should().Be(Entrou.AddDays(3));
    }

    [Fact]
    public void A_data_mais_nova_vence_e_a_mais_velha_nao_desfaz_nada()
    {
        var vinculo = ClienteCarteira.Criar(10, 20, ClasseDeCliente.C, vinculadoPorId: 1, vinculadoEmUtc: Entrou);
        var contato = Entrou.AddDays(10);
        vinculo.RegistrarInteracao(contato);

        vinculo.RegistrarInteracao(contato.AddDays(-5));
        vinculo.UltimaInteracaoEm.Should().Be(contato, "a data mais velha de uma fonte não apaga a mais nova da outra");

        vinculo.RegistrarInteracao(contato);
        vinculo.UltimaInteracaoEm.Should().Be(contato, "a mesma data não muda nada");

        vinculo.RegistrarInteracao(contato.AddHours(1));
        vinculo.UltimaInteracaoEm.Should().Be(contato.AddHours(1), "o contato mais novo avança a data");
    }

    [Fact]
    public void A_ordem_das_fontes_nao_importa_o_resultado_e_o_maximo()
    {
        var datas = new[] { Entrou.AddDays(7), Entrou.AddDays(2), Entrou.AddDays(30), Entrou.AddDays(15) };

        var emOrdem = ClienteCarteira.Criar(10, 20, ClasseDeCliente.C, vinculadoPorId: 1, vinculadoEmUtc: Entrou);
        var aoContrario = ClienteCarteira.Criar(10, 20, ClasseDeCliente.C, vinculadoPorId: 1, vinculadoEmUtc: Entrou);
        foreach (var data in datas) emOrdem.RegistrarInteracao(data);
        foreach (var data in Enumerable.Reverse(datas)) aoContrario.RegistrarInteracao(data);

        emOrdem.UltimaInteracaoEm.Should().Be(Entrou.AddDays(30));
        aoContrario.UltimaInteracaoEm.Should().Be(Entrou.AddDays(30));
    }
}
