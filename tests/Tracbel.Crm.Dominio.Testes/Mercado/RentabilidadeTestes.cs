using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// RECEITA, CUSTO E MARGEM POR HECTARE (issue 159) — a conta que junta preço, custo e produtividade,
/// que até aqui viviam em três lugares separados da tela.
///
/// <para>Os números são os públicos: o custo total do café em Franca na safra 2025 é
/// <b>R$ 29.279,94/ha</b>, que é o aceite da issue 67 e foi lido da série histórica da CONAB.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class RentabilidadeTestes
{
    /// <summary>O custo total por hectare do café em Franca, safra 2025 — aceite da issue 67.</summary>
    private const decimal CustoDoCafeEmFranca = 29_279.94m;

    [Fact]
    public void A_receita_por_hectare_e_produtividade_vezes_preco()
    {
        // A produtividade vem em quilos por hectare COLHIDO (regra da issue 152) e o preço em R$/kg,
        // que é como a CONAB publica. 1.800 kg/ha de café a R$ 33,89/kg.
        Rentabilidade.ReceitaPorHectare(1_800m, 33.89m).Should().Be(61_002m);

        Rentabilidade.ReceitaPorHectare(null, 33.89m).Should().BeNull("sem produtividade não há receita");
        Rentabilidade.ReceitaPorHectare(1_800m, null).Should().BeNull("sem preço não há receita");
    }

    [Fact]
    public void A_margem_do_cafe_em_franca_sai_com_o_custo_publico_da_conab()
    {
        // O CRITÉRIO DE ACEITE DA ISSUE: com Franca como referência, o custo total é R$ 29.279,94/ha.
        var receita = Rentabilidade.ReceitaPorHectare(1_800m, 33.89m);
        var margem = Rentabilidade.MargemPorHectare(receita, CustoDoCafeEmFranca);

        margem.Should().Be(61_002m - CustoDoCafeEmFranca);
        margem.Should().BeApproximately(31_722.06m, 0.01m);
    }

    [Fact]
    public void Margem_negativa_e_resultado_e_nao_erro()
    {
        // Safra em que o preço não cobre o custo existe. Esconder isso seria mentir sobre o mercado.
        var receita = Rentabilidade.ReceitaPorHectare(900m, 10m);

        Rentabilidade.MargemPorHectare(receita, CustoDoCafeEmFranca).Should().BeNegative();
    }

    [Fact]
    public void A_cana_e_vendida_pelo_acucar_que_carrega_e_nao_pelo_peso()
    {
        // O produtor de cana recebe pelo ATR: kg de ATR por tonelada × preço do kg de ATR × t/ha.
        // Usar o preço da tonelada de cana daria outro número — é o erro do protótipo (C-06).
        var receita = Rentabilidade.ReceitaDaCanaPorHectare(toneladasPorHa: 80m, atrKgPorTonelada: 140m, precoDoKgDeAtr: 0.87m);

        receita.Should().Be(80m * 140m * 0.87m);
        receita.Should().BeApproximately(9_744m, 0.01m);

        Rentabilidade.ReceitaDaCanaPorHectare(80m, null, 0.87m).Should().BeNull("sem ATR da safra não há receita");
    }

    [Fact]
    public void A_margem_total_multiplica_a_area_COLHIDA_e_nao_a_plantada()
    {
        // O DEFEITO QUE ESTE TESTE IMPEDE (achado C-07): o protótipo multiplica pela área plantada. Em
        // cultura perene a plantada inclui o pomar novo que ainda não produz — e a receita sai inventada.
        Rentabilidade.MargemTotal(1_000m, areaColhidaHa: 250m).Should().Be(250_000m);
        Rentabilidade.MargemTotal(1_000m, null).Should().BeNull();
    }

    [Fact]
    public void A_razao_de_rentabilidade_recusa_custo_zero()
    {
        // Custo zero é erro de leitura, e um índice infinito contaminaria toda média que o usasse.
        Rentabilidade.RazaoDeRentabilidade(60_000m, 30_000m).Should().Be(2m);
        Rentabilidade.RazaoDeRentabilidade(60_000m, 0m).Should().BeNull();
        Rentabilidade.RazaoDeRentabilidade(60_000m, null).Should().BeNull();
    }

    [Fact]
    public void O_indice_precisa_das_N_safras_completas()
    {
        // Sem as N anteriores, o índice não sai: uma média de duas safras onde se pediu cinco é outro
        // indicador, com outro nome. E sem o parâmetro N decidido, também não sai.
        var anteriores = new[] { 1.8m, 2.2m };

        Rentabilidade.IndiceDeRentabilidade(2m, anteriores, safrasDaMedia: 2).Should().Be(1m);
        Rentabilidade.IndiceDeRentabilidade(2m, anteriores, safrasDaMedia: 3).Should().BeNull("faltam safras");
        Rentabilidade.IndiceDeRentabilidade(2m, anteriores, safrasDaMedia: null).Should().BeNull("N não decidido");
    }

    // =============================================================================================
    // O vazio com motivo — a regra que atravessa a Inteligência de Mercado
    // =============================================================================================

    [Fact]
    public void Sem_a_referencia_escolhida_a_margem_diz_isso_antes_de_qualquer_outra_falta()
    {
        // D-P07 EM ABERTO. Quem não escolheu o local precisa ouvir isso primeiro — "sem preço" seria
        // consequência, e mandaria a pessoa procurar o problema no lugar errado.
        var motivo = Rentabilidade.PorQueNaoSaiu(
            temLocalDeReferencia: false, temCamada: false, temCusto: false,
            custoNaCamada: null, produtividade: null, preco: null);

        motivo.Should().Be(MotivoSemMargem.SemLocalDeReferencia);
        Rentabilidade.Frase(motivo, "café").Should().Contain("local de referência");
    }

    [Fact]
    public void A_safra_que_parou_no_operacional_nao_tem_margem_sobre_o_total()
    {
        // As abas antigas de laranja e duas de cana param no operacional (issue 67). Custo total
        // ausente NÃO É ZERO: a margem não sai, e a tela diz por quê.
        var motivo = Rentabilidade.PorQueNaoSaiu(
            temLocalDeReferencia: true, temCamada: true, temCusto: true,
            custoNaCamada: null, produtividade: 1_800m, preco: 33.89m);

        motivo.Should().Be(MotivoSemMargem.SemCustoTotalNaSafra);
        Rentabilidade.Frase(motivo, "laranja").Should().Contain("operacional");
    }

    [Fact]
    public void Area_colhida_zero_aparece_como_falta_de_produtividade()
    {
        // Produtividade é quantidade ÷ área colhida: colhida zero não dá divisão, e o que chega aqui é
        // nulo. A frase manda a pessoa olhar a PAM, que é onde o dado falta.
        var motivo = Rentabilidade.PorQueNaoSaiu(
            temLocalDeReferencia: true, temCamada: true, temCusto: true,
            custoNaCamada: CustoDoCafeEmFranca, produtividade: null, preco: 33.89m);

        motivo.Should().Be(MotivoSemMargem.SemProdutividade);
        Rentabilidade.Frase(motivo, "café").Should().Contain("área colhida");
    }

    [Fact]
    public void Preco_faltando_no_mes_da_safra_tem_frase_propria()
    {
        var motivo = Rentabilidade.PorQueNaoSaiu(
            temLocalDeReferencia: true, temCamada: true, temCusto: true,
            custoNaCamada: CustoDoCafeEmFranca, produtividade: 1_800m, preco: null);

        motivo.Should().Be(MotivoSemMargem.SemPreco);
        Rentabilidade.Frase(motivo, "amendoim").Should().Contain("Sem preço de amendoim");
    }

    [Fact]
    public void Com_tudo_no_lugar_nao_ha_motivo_e_a_frase_e_vazia()
    {
        var motivo = Rentabilidade.PorQueNaoSaiu(true, true, true, CustoDoCafeEmFranca, 1_800m, 33.89m);

        motivo.Should().Be(MotivoSemMargem.Nenhum);
        Rentabilidade.Frase(motivo, "café").Should().BeEmpty();
    }
}
