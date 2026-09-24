using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// OS QUATRO NÚMEROS DE DECISÃO (documento 50, §4.1).
///
/// <para>Estes testes existem porque, até aqui, os três números sem dado eram <c>valor={null}</c> escrito no
/// TypeScript, com o motivo repetido em dezesseis lugares de três arquivos — e nada disso tinha teste. A
/// ausência é uma AFIRMAÇÃO da tela, e afirmação sem teste é a que muda sozinha.</para>
/// </summary>
[Trait("Categoria", "Mercado")]
public sealed class NumerosDeDecisaoTestes
{
    private static DemandaDaCategoria Categoria(string nome, decimal demanda, decimal? preco = null) =>
        new(nome, demanda, preco);

    // -------------------------------------------------------------------------------------------------
    // O estado de hoje — é o que a tela mostra, e é o que estes testes travam
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void Com_o_banco_de_hoje_a_demanda_sai_e_os_outros_tres_dizem_o_que_falta()
    {
        // O recorte de hoje: o motor produz a demanda quando há ciclo, e não há nem vendas em unidades
        // (issue 69) nem preço de máquina (issue 70).
        var numeros = DecisaoDoMercado.Calcular(demandaAnual: 3457m, demandaAjustada: 2727m, vendasEmUnidades: null, porCategoria: []);

        numeros.DemandaAnual.Valor.Should().Be(3457m);
        numeros.MercadoAnual.Valor.Should().BeNull();
        numeros.MercadoAnual.Motivo.Should().Be(nameof(MotivoSemNumeroDeDecisao.SemPrecoDeMaquina));
        numeros.CapturaPercentual.Motivo.Should().Be(nameof(MotivoSemNumeroDeDecisao.SemVendasEmUnidades));
        numeros.Oportunidade.Motivo.Should().Be(nameof(MotivoSemNumeroDeDecisao.SemVendasEmUnidades));
    }

    [Fact]
    public void Sem_demanda_anual_nenhum_dos_quatro_inventa_numero()
    {
        var numeros = DecisaoDoMercado.Calcular(null, null, vendasEmUnidades: 120, porCategoria: [Categoria("Trator", 100m, 300_000m)]);

        numeros.DemandaAnual.Valor.Should().BeNull();
        numeros.MercadoAnual.Valor.Should().BeNull("sem demanda não há o que multiplicar pelo preço");
        numeros.CapturaPercentual.Valor.Should().BeNull("captura sem denominador não é zero: é sem base");
        numeros.Oportunidade.Valor.Should().BeNull();

        new[] { numeros.DemandaAnual.Motivo, numeros.MercadoAnual.Motivo, numeros.CapturaPercentual.Motivo, numeros.Oportunidade.Motivo }
            .Should().AllBe(nameof(MotivoSemNumeroDeDecisao.SemDemandaAnual));
    }

    // -------------------------------------------------------------------------------------------------
    // Mercado anual — a regra que o documento 50 §4.1 escreve em maiúsculas
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void O_mercado_anual_e_a_soma_por_categoria_e_nunca_a_demanda_total_vezes_um_preco()
    {
        // 10 colheitadeiras a R$ 1,2 mi + 100 tratores a R$ 300 mil = 12 mi + 30 mi.
        var numeros = DecisaoDoMercado.Calcular(
            demandaAnual: 110m, demandaAjustada: 110m, vendasEmUnidades: null,
            porCategoria: [Categoria("Colheitadeira", 10m, 1_200_000m), Categoria("Trator", 100m, 300_000m)]);

        numeros.MercadoAnual.Valor.Should().Be(42_000_000m);
        numeros.MercadoAnual.Parcial.Should().BeFalse();

        // A conta proibida daria 110 × 300.000 = 33 milhões, ou 110 × 1,2 mi = 132 milhões. Nenhuma das
        // duas é o mercado: elas misturam colhedora com trator compacto.
        numeros.MercadoAnual.Valor.Should().NotBe(33_000_000m).And.NotBe(132_000_000m);
    }

    [Fact]
    public void Categoria_sem_preco_nao_vira_zero_e_o_total_sai_marcado_como_parcial()
    {
        var numeros = DecisaoDoMercado.Calcular(
            demandaAnual: 110m, demandaAjustada: 110m, vendasEmUnidades: null,
            porCategoria: [Categoria("Trator", 100m, 300_000m), Categoria("Colheitadeira", 10m)]);

        numeros.MercadoAnual.Valor.Should().Be(30_000_000m, "só a categoria com preço entra na soma");
        numeros.MercadoAnual.Parcial.Should().BeTrue("somar em silêncio afirmaria que a colheitadeira não vale nada");
        numeros.MercadoAnual.CategoriasSemPreco.Should().Equal("Colheitadeira");
        numeros.MercadoAnual.Motivo.Should().Be(nameof(MotivoSemNumeroDeDecisao.Nenhum));
    }

    [Fact]
    public void Nenhuma_categoria_com_preco_e_ausencia_e_nao_total_parcial_de_zero()
    {
        var numeros = DecisaoDoMercado.Calcular(
            demandaAnual: 110m, demandaAjustada: 110m, vendasEmUnidades: null,
            porCategoria: [Categoria("Trator", 100m), Categoria("Colheitadeira", 10m)]);

        numeros.MercadoAnual.Valor.Should().BeNull("zero afirmaria que o mercado não vale nada");
        numeros.MercadoAnual.Parcial.Should().BeFalse("não há parte nenhuma apurada para ser parcial");
        numeros.MercadoAnual.CategoriasSemPreco.Should().Equal("Colheitadeira", "Trator");
    }

    [Fact]
    public void A_lista_das_categorias_sem_preco_nao_depende_da_ordem_de_quem_chamou()
    {
        var umaOrdem = DecisaoDoMercado.Calcular(10m, 10m, null, [Categoria("Trator", 5m), Categoria("Colheitadeira", 5m)]);
        var outraOrdem = DecisaoDoMercado.Calcular(10m, 10m, null, [Categoria("Colheitadeira", 5m), Categoria("Trator", 5m)]);

        umaOrdem.MercadoAnual.CategoriasSemPreco.Should().Equal(outraOrdem.MercadoAnual.CategoriasSemPreco);
    }

    // -------------------------------------------------------------------------------------------------
    // Captura e oportunidade — as regras das issues 162 e 69
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void A_captura_e_unidades_sobre_demanda_nos_dois_lados_em_maquinas()
    {
        var numeros = DecisaoDoMercado.Calcular(demandaAnual: 1000m, demandaAjustada: 900m, vendasEmUnidades: 184, porCategoria: []);

        numeros.CapturaPercentual.Valor.Should().Be(18.4m);
    }

    [Fact]
    public void A_oportunidade_sai_da_demanda_AJUSTADA_e_nao_da_estrutural()
    {
        // O que sobra é no mercado de hoje, e não no de um ano médio: 900 − 184, e não 1000 − 184.
        var numeros = DecisaoDoMercado.Calcular(demandaAnual: 1000m, demandaAjustada: 900m, vendasEmUnidades: 184, porCategoria: []);

        numeros.Oportunidade.Valor.Should().Be(716m);
    }

    [Fact]
    public void Vender_mais_que_a_demanda_estimada_da_oportunidade_zero_e_nao_negativa()
    {
        // Não é oportunidade negativa: é a estimativa tendo ficado curta (issue 162).
        var numeros = DecisaoDoMercado.Calcular(demandaAnual: 100m, demandaAjustada: 80m, vendasEmUnidades: 120, porCategoria: []);

        numeros.Oportunidade.Valor.Should().Be(0m);
        numeros.CapturaPercentual.Valor.Should().Be(120m, "a captura passa de 100% e isso é a informação, não um erro a esconder");
    }

    [Fact]
    public void Vender_zero_maquina_e_captura_zero_por_cento_e_isso_e_informacao()
    {
        // Zero VENDIDO é medição; fonte ausente é outra coisa, e o teste de cima cobre essa.
        var numeros = DecisaoDoMercado.Calcular(demandaAnual: 1000m, demandaAjustada: 1000m, vendasEmUnidades: 0, porCategoria: []);

        numeros.CapturaPercentual.Valor.Should().Be(0m);
        numeros.Oportunidade.Valor.Should().Be(1000m);
    }

    [Fact]
    public void Demanda_zerada_nao_vira_captura_infinita()
    {
        var numeros = DecisaoDoMercado.Calcular(demandaAnual: 0m, demandaAjustada: 0m, vendasEmUnidades: 5, porCategoria: []);

        numeros.CapturaPercentual.Valor.Should().BeNull();
        numeros.CapturaPercentual.Motivo.Should().Be(nameof(MotivoSemNumeroDeDecisao.SemDemandaAnual));
    }

    // -------------------------------------------------------------------------------------------------
    // A frase — uma redação só para a página e para a ficha
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void Cada_motivo_diz_a_issue_que_o_destrava()
    {
        DecisaoDoMercado.Frase(nameof(MotivoSemNumeroDeDecisao.SemDemandaAnual), "a oportunidade")
            .Should().Contain("D-P01").And.Contain("issue 63").And.Contain("a oportunidade");

        DecisaoDoMercado.Frase(nameof(MotivoSemNumeroDeDecisao.SemVendasEmUnidades), "a captura")
            .Should().Contain("issue 69").And.Contain("D-P08")
            .And.Contain("não serve de numerador", "é o engano que a frase existe para desfazer");

        DecisaoDoMercado.Frase(nameof(MotivoSemNumeroDeDecisao.SemPrecoDeMaquina), "o mercado anual")
            .Should().Contain("issue 70").And.Contain("preço genérico");
    }

    [Fact]
    public void Motivo_desconhecido_nao_devolve_frase_vazia()
    {
        DecisaoDoMercado.Frase("AlgoQueNinguemPreviu", "a captura").Should().NotBeNullOrWhiteSpace();
    }
}
