using System.Globalization;
using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// O FATOR DE CICLO E OS TRÊS CENÁRIOS (issue 74).
///
/// <para>Os pesos são os medidos no protótipo e registrados como primeira vigência em 23/09/2026 (D-P05):
/// <c>a = 0,4</c> no preço e na rentabilidade, <c>b = 0,5</c> no crédito, limites de 0,4 a 1,5. O peso da
/// percepção é <b>1,0</b>, e não o 0,4 do protótipo: lá a percepção ia de −2 a +2 e entrava dividida por
/// 2, o que dava ±40% de efeito; a decisão D-P04 trocou a escala para ±5 pontos percentuais justamente
/// para tirar aqueles ±40%. Com 1,0, o rótulo "−5% a +5%" é literal.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class FatorDeCicloTestes
{
    private static readonly DateTime Agora = new(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);

    private static ParametroDoPotencial Parametros(
        decimal? peso = 0.4m, decimal? credito = 0.5m, decimal? comercial = 1.0m,
        decimal? minimo = 0.4m, decimal? maximo = 1.5m) =>
        ParametroDoPotencial.Informar(
            new ParametroDoPotencial.Valores(
                MesesDaJanela: 12,
                PesoDosContratosNoCredito: 0.70m,
                LimiteDeRetracao: 1.00m,
                LimiteDeAquecimento: 1.20m,
                LimiteDeSuperaquecimento: 1.40m,
                NomeDaFaixaIntermediaria: null,
                LimiteDaPercepcao: 5m,
                PesoDoIndicadorDePreco: peso,
                PesoDoIndicadorDeCredito: credito,
                PesoDoIndicadorComercial: comercial,
                FatorMinimo: minimo,
                FatorMaximo: maximo),
            ParametroComVigencia.HojeNoBrasil(Agora),
            "pesos medidos no protótipo, a confirmar (D-P05, 23/09/2026)",
            100,
            Agora);

    // ---------------------------------------------------------------------------------------------
    // Neutralidade — o aceite da issue
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Sem_indice_nenhum_o_fator_e_um_e_a_demanda_e_a_estrutural()
    {
        // O ACEITE DA ISSUE: "sem índices, o fator é 1". Indicador ausente vale DESVIO ZERO, e não
        // "fator indeterminado" — quem não tem preço carregado deve ver a demanda estrutural, não a
        // demanda sumindo.
        var ajustado = FatorDeCiclo.Ajustar(3_457m, null, null, null, Parametros());

        ajustado.Fator.Fator.Should().Be(1m);
        ajustado.Fator.IndicadoresUsados.Should().Be(0);
        ajustado.DemandaAjustada.Should().Be(3_457m);
        ajustado.VariacaoPercentual.Should().Be(0m);
        FatorDeCiclo.Frase(ajustado.Fator).Should().Contain("o fator é neutro");
    }

    [Fact]
    public void Indice_exatamente_neutro_tambem_da_fator_um()
    {
        FatorDeCiclo.Fator(1m, 1m, 0m, Parametros()).Fator.Should().Be(1m);
    }

    // ---------------------------------------------------------------------------------------------
    // A conta
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void O_fator_e_o_produtor_vezes_o_credito()
    {
        // (1 + 0,4 × 0,25 + 1,0 × 0,00) × (1 + 0,5 × 0,10) = 1,10 × 1,05 = 1,155
        var fator = FatorDeCiclo.Fator(1.25m, 1.10m, 0m, Parametros());

        fator.ParcelaDePreco.Should().Be(0.100m);
        fator.ParcelaDeCredito.Should().Be(0.050m);
        fator.Fator.Should().Be(1.155m);
        fator.IndicadoresUsados.Should().Be(3);
    }

    [Fact]
    public void A_percepcao_de_mais_cinco_por_cento_move_o_fator_em_cinco_por_cento()
    {
        // O PESO DA PERCEPÇÃO É 1,0 PARA O RÓTULO SER VERDADE. A decisão D-P04 diz "−5% a +5%"; com o
        // 0,4 do protótipo — calibrado para uma escala de −2 a +2 — o mesmo +5 moveria só 2%.
        var comPercepcao = FatorDeCiclo.Fator(1m, 1m, 5m, Parametros());
        var semPercepcao = FatorDeCiclo.Fator(1m, 1m, 0m, Parametros());

        comPercepcao.Fator.Should().Be(1.05m);
        semPercepcao.Fator.Should().Be(1m);

        FatorDeCiclo.Fator(1m, 1m, 5m, Parametros(comercial: 0.4m)).Fator
            .Should().Be(1.02m, "com o peso do protótipo, os mesmos +5% moveriam 2%");
    }

    [Fact]
    public void A_percepcao_negativa_puxa_para_baixo()
    {
        FatorDeCiclo.Fator(1m, 1m, -5m, Parametros()).Fator.Should().Be(0.95m);
    }

    [Fact]
    public void O_credito_multiplica_porque_sem_ele_nem_a_melhor_safra_vira_maquina()
    {
        // Preço ótimo e crédito no chão: o produto segura o fator, que é o ponto da forma multiplicativa.
        // (1 + 0,4 × 0,50) × (1 + 0,5 × (−0,50)) = 1,20 × 0,75 = 0,90
        FatorDeCiclo.Fator(1.50m, 0.50m, 0m, Parametros()).Fator.Should().Be(0.90m);
    }

    // ---------------------------------------------------------------------------------------------
    // Os limites
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void O_limite_corta_e_a_frase_diz_qual_era_o_numero_antes()
    {
        // (1 + 0,4 × 2,00) × (1 + 0,5 × 2,00) = 1,80 × 2,00 = 3,60, cortado em 1,50.
        var fator = FatorDeCiclo.Fator(3m, 3m, 0m, Parametros());

        fator.FatorSemLimite.Should().Be(3.60m);
        fator.Fator.Should().Be(1.5m);
        fator.CortadoPeloLimite.Should().BeTrue();
        FatorDeCiclo.Frase(fator).Should().Contain("3,60").And.Contain("1,50");
    }

    [Fact]
    public void A_frase_usa_virgula_mesmo_num_servidor_de_outra_cultura()
    {
        // O DEFEITO QUE ESTE TESTE IMPEDE, e que o CI pegou antes de mim: a interpolação sem cultura
        // escreve "3.60" no runner (cultura invariante) e "3,60" na estação (pt-BR) — o mesmo código,
        // dois textos. Quem lê a frase é o comercial brasileiro, e a vírgula é parte do texto, não
        // preferência do processo.
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var fator = FatorDeCiclo.Fator(3m, 3m, 0m, Parametros());

            FatorDeCiclo.Frase(fator).Should().Contain("3,60").And.Contain("1,50")
                .And.NotContain("3.60");
        }
        finally
        {
            CultureInfo.CurrentCulture = anterior;
        }
    }

    [Fact]
    public void Sem_limites_registrados_o_fator_sai_sem_corte()
    {
        var fator = FatorDeCiclo.Fator(3m, 3m, 0m, Parametros(minimo: null, maximo: null));

        fator.Fator.Should().Be(3.60m);
        fator.CortadoPeloLimite.Should().BeFalse();
    }

    // ---------------------------------------------------------------------------------------------
    // Vazio com motivo
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Sem_os_pesos_decididos_nao_ha_fator_e_a_demanda_sai_sem_ajuste()
    {
        // PESO É DECISÃO, NÃO CONTA (D-P05). Sem ele, a tela mostra a demanda estrutural sozinha, em vez
        // de uma ajustada por pesos que ninguém escolheu.
        var ajustado = FatorDeCiclo.Ajustar(3_457m, 1.25m, 1.10m, 2m, Parametros(peso: null));

        ajustado.Fator.Fator.Should().BeNull();
        ajustado.Fator.Motivo.Should().Be("SemPesos");
        ajustado.DemandaAjustada.Should().BeNull();
        ajustado.DemandaEstrutural.Should().Be(3_457m, "a estrutural continua visível");
        ajustado.Frase.Should().Contain("não foram decididos");
    }

    [Fact]
    public void Sem_parametro_vigente_na_data_nao_ha_fator()
    {
        var ajustado = FatorDeCiclo.Ajustar(3_457m, 1.25m, 1.10m, 2m, parametro: null);

        ajustado.Fator.Motivo.Should().Be("SemParametroVigente");
        ajustado.Frase.Should().Contain("não há parâmetro do potencial vigente");
        ajustado.Cenarios.Should().HaveCount(3).And.OnlyContain(c => c.Fator == null);
    }

    [Fact]
    public void Com_um_indicador_so_a_frase_diz_quantos_entraram()
    {
        var fator = FatorDeCiclo.Fator(1.25m, null, null, Parametros());

        fator.IndicadoresUsados.Should().Be(1);
        FatorDeCiclo.Frase(fator).Should().Contain("1 de 3 indicadores");
    }

    [Fact]
    public void O_selo_de_estimativa_atravessa_o_fator()
    {
        FatorDeCiclo.Frase(FatorDeCiclo.Fator(1.25m, 1.1m, 0m, Parametros(), estimativa: true))
            .Should().Contain("medidos no protótipo, ainda a confirmar");
    }

    // ---------------------------------------------------------------------------------------------
    // Os três cenários
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Os_cenarios_saem_em_ordem_e_o_moderado_e_o_fator_calculado()
    {
        // O ACEITE DA ISSUE: conservador ≤ moderado ≤ otimista.
        var ajustado = FatorDeCiclo.Ajustar(3_457m, 1.28m, 1.10m, 0m, Parametros());

        ajustado.Cenarios.Select(c => c.Nome).Should().Equal(["Conservador", "Moderado", "Otimista"]);
        ajustado.Cenarios[1].Fator.Should().Be(ajustado.Fator.Fator);
        ajustado.Cenarios[0].Fator.Should().BeLessThanOrEqualTo(ajustado.Cenarios[1].Fator!.Value);
        ajustado.Cenarios[2].Fator.Should().BeGreaterThanOrEqualTo(ajustado.Cenarios[1].Fator!.Value);
    }

    [Fact]
    public void O_cenario_leva_CADA_indice_a_borda_da_faixa_em_que_ele_ja_esta()
    {
        // 1,28 está em "aquecido" (1,20 a 1,40) e 1,00 está em "intermediária" (1,00 a 1,20): o
        // conservador lê 1,20 e 1,00, o otimista lê 1,40 e 1,20. Os DOIS se movem — não é variação
        // inventada, é o intervalo que a própria classificação usa.
        var ajustado = FatorDeCiclo.Ajustar(1_000m, 1.28m, 1m, 0m, Parametros());

        ajustado.Cenarios[0].Fator.Should().Be(FatorDeCiclo.Fator(1.20m, 1.00m, 0m, Parametros()).Fator);
        ajustado.Cenarios[2].Fator.Should().Be(FatorDeCiclo.Fator(1.40m, 1.20m, 0m, Parametros()).Fator);
    }

    [Fact]
    public void A_borda_aberta_fica_onde_esta_e_o_cenario_coincide_com_o_moderado()
    {
        // A faixa de superaquecimento não tem teto; empurrar o índice para o infinito seria inventar um
        // cenário que a classificação não descreve. Com os DOIS índices lá em cima, o otimista não tem
        // para onde ir e coincide com o moderado.
        var ajustado = FatorDeCiclo.Ajustar(1_000m, 1.80m, 1.80m, 0m, Parametros(maximo: 10m));

        ajustado.Cenarios[2].Fator.Should().Be(ajustado.Cenarios[1].Fator);
        ajustado.Cenarios[0].Fator.Should().BeLessThan(ajustado.Cenarios[1].Fator!.Value,
            "para baixo a faixa tem piso: 1,40");
    }

    [Fact]
    public void A_percepcao_do_gestor_entra_com_o_mesmo_desvio_nos_tres_cenarios()
    {
        // ELA NÃO É FAIXA DE MERCADO, é a opinião de uma pessoa sobre aquele município: um cenário que a
        // mexesse estaria simulando o gestor mudando de ideia, e não o mercado mudando. Por isso os três
        // cenários são, exatamente, o fator nas bordas COM a mesma percepção.
        var ajustado = FatorDeCiclo.Ajustar(1_000m, 1.28m, 1m, 5m, Parametros());

        ajustado.Cenarios[0].Fator.Should().Be(FatorDeCiclo.Fator(1.20m, 1.00m, 5m, Parametros()).Fator);
        ajustado.Cenarios[1].Fator.Should().Be(FatorDeCiclo.Fator(1.28m, 1.00m, 5m, Parametros()).Fator);
        ajustado.Cenarios[2].Fator.Should().Be(FatorDeCiclo.Fator(1.40m, 1.20m, 5m, Parametros()).Fator);
    }

    [Fact]
    public void O_efeito_da_percepcao_e_amplificado_pelo_credito_e_isso_e_a_forma_multiplicativa()
    {
        // O DESVIO da percepção é o mesmo sempre (+0,05); o EFEITO dele no fator final é multiplicado
        // pelo termo de crédito, porque ele mora dentro do parêntese do produtor. Com crédito neutro,
        // +5% move 5%; com crédito 20% acima, move 5,5%. É consequência da forma do protótipo, e é
        // defensável: o otimismo do gestor vale mais quando há crédito para financiá-lo.
        var comCreditoNeutro =
            FatorDeCiclo.Fator(1m, 1m, 5m, Parametros()).Fator!.Value
            - FatorDeCiclo.Fator(1m, 1m, 0m, Parametros()).Fator!.Value;

        var comCreditoAquecido =
            FatorDeCiclo.Fator(1m, 1.20m, 5m, Parametros()).Fator!.Value
            - FatorDeCiclo.Fator(1m, 1.20m, 0m, Parametros()).Fator!.Value;

        comCreditoNeutro.Should().Be(0.05m);
        comCreditoAquecido.Should().Be(0.055m);
    }

    // ---------------------------------------------------------------------------------------------
    // O número medido do protótipo
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void A_queda_medida_no_prototipo_sai_do_fator_que_a_planilha_usou()
    {
        // MEDIDO NA PASTA 360 e registrado na issue 74: com os valores da planilha, a demanda da região
        // cai de 3.457 para 2.727 máquinas por ano — 21% a menos. Isso é um fator de 0,7888.
        //
        // O QUE ESTE TESTE PRENDE, E O QUE ELE NÃO PODE PRENDER: ele prende a APLICAÇÃO do fator — a
        // demanda ajustada é estrutural × fator, e a variação é a que o documento mede. Ele NÃO
        // reproduz o fator a partir dos índices da planilha, porque o fator dela inclui o TERMO DE
        // TROCA, que o CRM não tem (precisa do preço de máquina, issue 70). Quando a #70 entrar, este
        // teste ganha os índices e reproduz o 0,7888 de ponta a ponta.
        const decimal estrutural = 3_457m;
        const decimal doProtótipo = 2_727m;

        var fatorDaPlanilha = doProtótipo / estrutural;
        fatorDaPlanilha.Should().BeApproximately(0.7888m, 0.0001m);

        // O mesmo fator, montado pelos indicadores que o CRM TEM: preço em 0,60 e crédito em 0,95.
        // (1 + 0,4 × (−0,40)) × (1 + 0,5 × (−0,05)) = 0,84 × 0,975 = 0,819
        var equivalente = FatorDeCiclo.Ajustar(estrutural, 0.60m, 0.95m, 0m, Parametros());

        equivalente.Fator.Fator.Should().Be(0.81900m);
        equivalente.DemandaAjustada.Should().Be(2_831.283m);
        equivalente.VariacaoPercentual!.Value.Should().BeApproximately(-18.1m, 0.1m);
    }
}
