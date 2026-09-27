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
    // A base da captura — máquina contra máquina da mesma categoria (D-P01, 27/09/2026)
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void A_captura_conta_so_as_maquinas_das_categorias_que_tem_demanda()
    {
        // UM RECORTE EM MINIATURA: regra só de trator, e o ART trazendo trator, colheitadeira e uma
        // plataforma de corte, que está em linha sem categoria e por isso não aparece na quebra.
        var conta = BaseDaCaptura.Montar(
            unidadesVendidas: 10,
            vendidasPorCategoria: [("TRATOR", 7), ("COLHEITADEIRA", 2)],
            categoriasComDemanda: [("TRATOR", "Trator")])!;

        conta.Unidades.Should().Be(7, "só o trator tem demanda do outro lado da conta");
        conta.UnidadesForaDaConta.Should().Be(3, "as duas colheitadeiras e a plataforma sem categoria");
        conta.Categorias.Should().Equal("Trator");

        var numeros = DecisaoDoMercado.Calcular(demandaAnual: 35m, demandaAjustada: 35m, vendasEmUnidades: conta.Unidades, porCategoria: []);

        numeros.CapturaPercentual.Valor.Should().Be(20m,
            "7 tratores sobre a demanda de 35 tratores — as 10 máquinas dariam 28,6%, e seria colheitadeira contra demanda de trator");
    }

    [Fact]
    public void Sem_a_fonte_das_unidades_nao_ha_base_e_a_captura_continua_dizendo_que_falta_a_fonte()
    {
        BaseDaCaptura.Montar(null, [], [("TRATOR", "Trator")]).Should().BeNull("ausência de carga não é venda zero");
    }

    [Fact]
    public void Sem_categoria_com_demanda_nenhuma_venda_entra_na_conta_e_a_frase_diz_por_que()
    {
        var conta = BaseDaCaptura.Montar(4, [("TRATOR", 2)], [])!;

        conta.Unidades.Should().Be(0);
        conta.UnidadesForaDaConta.Should().Be(4);
        conta.Categorias.Should().BeEmpty();
        conta.Frase.Should().Contain("Nenhuma categoria tem demanda estimada").And.Contain("4 máquinas vendidas");
    }

    [Fact]
    public void A_frase_escreve_a_conta_e_o_que_ficou_de_fora_com_o_milhar_brasileiro()
    {
        var conta = BaseDaCaptura.Montar(1_300, [("TRATOR", 1_032), ("COLHEITADEIRA", 200)], [("TRATOR", "Trator")])!;

        conta.Frase.Should().StartWith(
            "A conta deste recorte: 1.032 máquinas vendidas da categoria Trator ÷ a demanda anual estimada da mesma categoria.");
        conta.Frase.Should().Contain("268 máquinas ficam de fora").And.Contain("regra de potencial");
    }

    [Fact]
    public void Categoria_com_demanda_e_sem_venda_entra_na_conta_com_zero_e_nada_fica_de_fora()
    {
        // Zero vendido é medida: a colheitadeira tem demanda e a Tracbel não vendeu nenhuma.
        var conta = BaseDaCaptura.Montar(5, [("TRATOR", 5)], [("TRATOR", "Trator"), ("COLHEITADEIRA", "Colheitadeira")])!;

        conta.Unidades.Should().Be(5);
        conta.UnidadesForaDaConta.Should().Be(0);
        conta.Categorias.Should().Equal("Colheitadeira", "Trator");
        conta.Frase.Should().Contain("das categorias Colheitadeira e Trator").And.Contain("das mesmas categorias");
        conta.Frase.Should().NotContain("de fora");
    }

    [Fact]
    public void Uma_maquina_so_fica_no_singular()
    {
        var conta = BaseDaCaptura.Montar(2, [("TRATOR", 1)], [("TRATOR", "Trator")])!;

        conta.Frase.Should().Contain("1 máquina vendida da categoria Trator").And.Contain("1 máquina fica de fora");
    }

    [Fact]
    public void A_lista_das_categorias_da_conta_nao_depende_da_ordem_de_quem_chamou()
    {
        var umaOrdem = BaseDaCaptura.Montar(3, [], [("TRATOR", "Trator"), ("COLHEITADEIRA", "Colheitadeira")])!;
        var outraOrdem = BaseDaCaptura.Montar(3, [], [("COLHEITADEIRA", "Colheitadeira"), ("TRATOR", "Trator")])!;

        umaOrdem.Categorias.Should().Equal(outraOrdem.Categorias);
        umaOrdem.Frase.Should().Be(outraOrdem.Frase);
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

    // -------------------------------------------------------------------------------------------------
    // O mesmo trecho do ano anterior (decisão de 27/09/2026)
    // -------------------------------------------------------------------------------------------------

    private static readonly (string, string)[] SoTrator = [("TRATOR", "Trator")];

    [Fact]
    public void A_captura_de_antes_usa_a_mesma_demanda_e_so_as_categorias_com_demanda()
    {
        // HOJE: 7 tratores sobre 35 de demanda = 20%. ANTES: 5 tratores e 2 colheitadeiras — só os tratores
        // entram, contra a MESMA demanda: 5 ÷ 35.
        var baseAtual = BaseDaCaptura.Montar(7, [("TRATOR", 7)], SoTrator);
        var atual = DecisaoDoMercado.Calcular(35m, 40m, baseAtual!.Unidades, []);
        var baseAnterior = BaseDaCaptura.Montar(7, [("TRATOR", 5), ("COLHEITADEIRA", 2)], SoTrator);

        var comparacao = DecisaoDoMercado.CompararComOAnoAnterior(atual, 35m, 40m, baseAnterior, null);

        comparacao.CapturaPercentual.Valor.Should().Be(5m / 35m * 100m);
        comparacao.Oportunidade.Valor.Should().Be(35m, "40 de demanda ajustada menos os 5 tratores de antes");
        comparacao.BaseDaCaptura!.UnidadesForaDaConta.Should().Be(2, "a colheitadeira não tem demanda do outro lado");
    }

    [Fact]
    public void Demanda_e_mercado_anual_nao_tem_ano_anterior_porque_sao_estruturais()
    {
        // O MESMO NÚMERO DOS DOIS LADOS daria uma variação de 0% — uma estabilidade que ninguém mediu.
        var atual = DecisaoDoMercado.Calcular(35m, 40m, 7, [Categoria("Trator", 35m, 300_000m)]);

        var comparacao = DecisaoDoMercado.CompararComOAnoAnterior(
            atual, 35m, 40m, BaseDaCaptura.Montar(7, [("TRATOR", 7)], SoTrator), null);

        comparacao.DemandaAnual.Valor.Should().BeNull();
        comparacao.DemandaAnual.Motivo.Should().Be(nameof(MotivoSemComparacao.NumeroEstrutural));
        comparacao.DemandaAnual.Frase.Should().Contain("estrutural").And.Contain("PAM");
        comparacao.MercadoAnual.Motivo.Should().Be(nameof(MotivoSemComparacao.NumeroEstrutural));
    }

    [Fact]
    public void Sem_o_numero_do_periodo_nao_ha_variacao_e_a_frase_diz_por_que()
    {
        var atual = DecisaoDoMercado.Calcular(null, null, 7, []);

        var comparacao = DecisaoDoMercado.CompararComOAnoAnterior(
            atual, null, null, BaseDaCaptura.Montar(3, [("TRATOR", 3)], SoTrator), null);

        comparacao.CapturaPercentual.Motivo.Should().Be(nameof(MotivoSemComparacao.SemNumeroNoPeriodo));
        comparacao.MercadoAnual.Motivo.Should().Be(nameof(MotivoSemComparacao.SemNumeroNoPeriodo));
    }

    [Fact]
    public void Sem_as_vendas_do_ano_anterior_a_frase_e_a_da_cobertura_da_fonte()
    {
        var atual = DecisaoDoMercado.Calcular(35m, 40m, 7, []);
        const string cobertura = "A primeira venda que o ART trouxe é de jan/2024.";

        var comparacao = DecisaoDoMercado.CompararComOAnoAnterior(atual, 35m, 40m, null, cobertura);

        comparacao.CapturaPercentual.Valor.Should().BeNull("ausência de carga não é venda zero");
        comparacao.CapturaPercentual.Motivo.Should().Be(nameof(MotivoSemComparacao.AnoAnteriorSemVendas));
        comparacao.CapturaPercentual.Frase.Should().Be(cobertura);
        comparacao.Oportunidade.Frase.Should().Be(cobertura);
    }
}
