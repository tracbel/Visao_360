using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// O PREÇO IMPLÍCITO DA PAM (issue 198).
///
/// <para>Ele é <c>valor da produção × 1000 ÷ quantidade produzida</c> — o preço médio recebido pelo
/// produtor naquele ano, por município, derivado de duas colunas que já estão no banco desde 2010.</para>
/// </summary>
[Trait("Categoria", "Mercado")]
public sealed class PrecoImplicitoDaPamTestes
{
    private const int Cafe = 40139;
    private const int Soja = 40441;
    private const int Abacaxi = 40092;
    private const int Banana = 40136;

    // -------------------------------------------------------------------------------------------------
    // A conta
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void O_preco_e_o_valor_em_mil_reais_vezes_mil_dividido_pela_quantidade()
    {
        // 900.000 mil reais = R$ 900 milhões; 50.000 toneladas → R$ 18.000 por tonelada.
        var preco = PrecoImplicitoDaPam.De(Cafe, 2024, valorDaProducaoMilReais: 900_000m, quantidadeProduzida: 50_000m);

        preco.PrecoPorUnidade.Should().Be(18_000m);
        preco.Motivo.Should().Be(nameof(MotivoSemPrecoImplicito.Nenhum));
        preco.Ano.Should().Be(2024);
    }

    [Fact]
    public void O_numerador_e_o_denominador_saem_junto_do_preco()
    {
        // Sem eles, ninguém confere a conta — e este número é derivado, não medido.
        var preco = PrecoImplicitoDaPam.De(Cafe, 2024, 900_000m, 50_000m);

        preco.ValorDaProducaoMilReais.Should().Be(900_000m);
        preco.QuantidadeProduzida.Should().Be(50_000m);
    }

    // -------------------------------------------------------------------------------------------------
    // A unidade — issue 152: não é tonelada em todos os produtos
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void A_unidade_vem_do_produto_e_do_ano_e_nao_e_tonelada_em_todos()
    {
        PrecoImplicitoDaPam.De(Soja, 2024, 1m, 1m).Unidade.Should().Be("toneladas");

        // Abacaxi é SEMPRE mil frutos (nota 6 do IBGE): "R$ por tonelada" aqui seria mentira.
        PrecoImplicitoDaPam.De(Abacaxi, 2024, 1m, 1m).Unidade.Should().NotBe("toneladas");

        // A banana só passa a tonelada em 2001 — antes disso é mil cachos.
        PrecoImplicitoDaPam.De(Banana, 1999, 1m, 1m).Unidade
            .Should().NotBe(PrecoImplicitoDaPam.De(Banana, 2010, 1m, 1m).Unidade);
    }

    [Fact]
    public void A_unidade_sai_mesmo_quando_o_preco_nao_sai()
    {
        // A tela precisa dizer "sem preço por tonelada em 2019", e não só "sem preço".
        PrecoImplicitoDaPam.De(Soja, 2019, null, 100m).Unidade.Should().Be("toneladas");
    }

    // -------------------------------------------------------------------------------------------------
    // Ausência com motivo — os três casos são fatos diferentes
    // -------------------------------------------------------------------------------------------------

    [Fact]
    public void Sem_valor_divulgado_nao_ha_preco_e_o_motivo_diz_que_pode_ser_sigilo()
    {
        var preco = PrecoImplicitoDaPam.De(Cafe, 2024, valorDaProducaoMilReais: null, quantidadeProduzida: 50_000m);

        preco.PrecoPorUnidade.Should().BeNull();
        preco.Motivo.Should().Be(nameof(MotivoSemPrecoImplicito.SemValorDaProducao));
        PrecoImplicitoDaPam.Frase(preco.Motivo).Should().Contain("Sigilo não é zero");
    }

    [Fact]
    public void Sem_quantidade_divulgada_nao_ha_denominador()
    {
        var preco = PrecoImplicitoDaPam.De(Cafe, 2024, 900_000m, quantidadeProduzida: null);

        preco.PrecoPorUnidade.Should().BeNull();
        preco.Motivo.Should().Be(nameof(MotivoSemPrecoImplicito.SemQuantidadeProduzida));
    }

    [Fact]
    public void Quantidade_zero_e_colheita_nenhuma_e_ainda_assim_nao_da_preco_zero()
    {
        // Zero é MEDIÇÃO — diferente de sigilo —, e mesmo assim dividir por ele não dá "preço zero".
        var preco = PrecoImplicitoDaPam.De(Cafe, 2024, 900_000m, quantidadeProduzida: 0m);

        preco.PrecoPorUnidade.Should().BeNull();
        preco.Motivo.Should().Be(nameof(MotivoSemPrecoImplicito.SemColheita));
        preco.QuantidadeProduzida.Should().Be(0m, "o zero medido continua visível: ele é informação");
    }

    [Fact]
    public void Os_tres_motivos_tem_frases_diferentes()
    {
        var frases = new[]
        {
            PrecoImplicitoDaPam.Frase(nameof(MotivoSemPrecoImplicito.SemValorDaProducao)),
            PrecoImplicitoDaPam.Frase(nameof(MotivoSemPrecoImplicito.SemQuantidadeProduzida)),
            PrecoImplicitoDaPam.Frase(nameof(MotivoSemPrecoImplicito.SemColheita)),
        };

        frases.Should().OnlyHaveUniqueItems().And.AllSatisfy(f => f.Should().NotBeNullOrWhiteSpace());
        PrecoImplicitoDaPam.Frase("AlgoQueNinguemPreviu").Should().NotBeNullOrWhiteSpace();
    }

    // -------------------------------------------------------------------------------------------------
    // As médias plurianuais — a armadilha desta issue
    // -------------------------------------------------------------------------------------------------

    private static PrecoImplicitoNoAno Ano(short ano, decimal? preco) =>
        preco is null
            ? PrecoImplicitoDaPam.De(Soja, ano, null, 100m)
            : PrecoImplicitoDaPam.De(Soja, ano, preco.Value * 100m / 1_000m, 100m);

    [Fact]
    public void A_media_de_tres_anos_sai_quando_os_tres_anos_tem_preco()
    {
        var serie = new[] { Ano(2022, 100m), Ano(2023, 200m), Ano(2024, 300m) };

        var media = PrecoImplicitoDaPam.Media(serie, 3);

        media.Preco.Should().Be(200m);
        media.AnosFaltando.Should().BeEmpty();
    }

    [Fact]
    public void Faltando_um_ano_a_media_NAO_sai_e_a_tela_diz_qual_faltou()
    {
        // O DEFEITO QUE ISTO IMPEDE: com 2022 ausente, a média de 2023 e 2024 daria 250 e seria
        // apresentada como "média de 3 anos". Uma média de dois anos com cara de três é mais enganosa que
        // ausência nenhuma, porque parece completa.
        var serie = new[] { Ano(2022, null), Ano(2023, 200m), Ano(2024, 300m) };

        var media = PrecoImplicitoDaPam.Media(serie, 3);

        media.Preco.Should().BeNull();
        media.AnosFaltando.Should().Equal((short)2022);
    }

    [Fact]
    public void Ano_ausente_da_serie_conta_como_faltando_igual_a_ano_sem_preco()
    {
        // A série nem traz 2020 e 2021: a janela de cinco anos não fecha, e os dois são nomeados.
        var serie = new[] { Ano(2022, 100m), Ano(2023, 200m), Ano(2024, 300m) };

        var media = PrecoImplicitoDaPam.Media(serie, 5);

        media.Preco.Should().BeNull();
        media.AnosFaltando.Should().Equal((short)2020, (short)2021);
    }

    [Fact]
    public void A_janela_termina_no_ano_mais_recente_da_serie_e_ignora_o_que_vem_antes()
    {
        // Sete anos na série, janela de três: valem 2022, 2023 e 2024 — e 2019 não entra nem estraga.
        var serie = new[]
        {
            Ano(2018, 10m), Ano(2019, null), Ano(2020, 30m),
            Ano(2021, 40m), Ano(2022, 100m), Ano(2023, 200m), Ano(2024, 300m),
        };

        PrecoImplicitoDaPam.Media(serie, 3).Preco.Should().Be(200m);
    }

    [Fact]
    public void A_ordem_da_serie_nao_muda_a_media()
    {
        var crescente = new[] { Ano(2022, 100m), Ano(2023, 200m), Ano(2024, 300m) };
        var embaralhada = new[] { Ano(2024, 300m), Ano(2022, 100m), Ano(2023, 200m) };

        PrecoImplicitoDaPam.Media(embaralhada, 3).Preco.Should().Be(PrecoImplicitoDaPam.Media(crescente, 3).Preco);
    }

    [Fact]
    public void Serie_vazia_nao_devolve_zero()
    {
        var media = PrecoImplicitoDaPam.Media([], 3);

        media.Preco.Should().BeNull();
        media.AnosFaltando.Should().BeEmpty("não há série de onde dizer quais anos faltam");
    }

    [Fact]
    public void Janela_de_zero_ou_negativa_e_erro_de_programacao_e_nao_resultado_vazio()
    {
        var acao = () => PrecoImplicitoDaPam.Media([Ano(2024, 100m)], 0);

        acao.Should().Throw<ArgumentOutOfRangeException>();
    }
}
