using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// A REGRA DE CADA ITEM DO PAINEL DE COBERTURA (issue 150): completa é 100%, vazia é zero, o resto é parcial —
/// sem limite inventado — e a frase diz o que falta e para quê.
/// </summary>
public sealed class CoberturaDoMotorTestes
{
    private static ItemDeCobertura Item(int total, int cobertos) =>
        new("PAM.40106", "Cana-de-açúcar", "municípios da ADR", total, cobertos, "o potencial estrutural desta cultura");

    [Fact]
    public void Todos_com_o_dado_e_completa()
    {
        var item = Item(203, 203);

        item.Situacao.Should().Be(SituacaoDaCobertura.Completa);
        item.Percentual.Should().Be(100m);
        item.Motivo.Should().Be("203 de 203 municípios da ADR com o dado.");
    }

    [Fact]
    public void Parte_com_o_dado_e_parcial_e_a_frase_diz_quantos_faltam_e_para_que()
    {
        var item = Item(203, 190);

        item.Situacao.Should().Be(SituacaoDaCobertura.Parcial);
        item.Percentual.Should().Be(93.6m);
        item.Motivo.Should().Be("190 de 203 municípios da ADR com o dado — faltam 13 para o potencial estrutural desta cultura.");
    }

    [Fact]
    public void Nenhum_com_o_dado_e_vazia()
    {
        var item = Item(1_234, 0);

        item.Situacao.Should().Be(SituacaoDaCobertura.Vazia);
        item.Percentual.Should().Be(0m);
        item.Motivo.Should().Be("0 de 1.234 municípios da ADR com o dado — sem base para o potencial estrutural desta cultura.",
            "o milhar sai com ponto, como a tela escreve");
    }

    [Fact]
    public void Sem_registro_nenhum_e_vazia_e_nao_tem_percentual()
    {
        var item = Item(0, 0);

        item.Situacao.Should().Be(SituacaoDaCobertura.Vazia);
        item.Percentual.Should().BeNull("zero de zero não é 0% nem 100%: não há o que medir");
        item.Motivo.Should().Be("Nenhum registro no banco — sem base para o potencial estrutural desta cultura.");
    }

    [Fact]
    public void Cobertos_acima_do_total_nao_passa_de_100_por_cento()
    {
        // Não deveria acontecer; se a consulta contar a mais, o painel não pode dizer 110%.
        Item(10, 11).Percentual.Should().Be(100m);
        Item(10, 11).Situacao.Should().Be(SituacaoDaCobertura.Completa);
    }

    [Fact]
    public void O_grupo_conta_os_itens_incompletos()
    {
        var grupo = new GrupoDeCobertura("PAM", "Produção agrícola por cultura", "IBGE", false,
            [Item(2, 2), Item(2, 1), Item(0, 0)]);

        grupo.Incompletos.Should().Be(2);
    }
}
