using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// OS INDICADORES DE MERCADO (issue 73) — momento de preço, crédito e a margem na unidade do produtor.
///
/// <para>As faixas e o peso do crédito são os decididos no texto de 21/09/2026 (D-P02 e D-P03): janela de
/// 12 contra 12, faixas 1,0 / 1,2 / 1,4, e 70% contratos + 30% valor.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class IndicadoresDeMercadoTestes
{
    private static readonly DateTime Agora = new(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Os parâmetros vigentes, com as faixas decididas.</summary>
    private static ParametroDoPotencial Parametros() =>
        ParametroDoPotencial.Informar(
            new ParametroDoPotencial.Valores(
                MesesDaJanela: 12,
                PesoDosContratosNoCredito: 0.70m,
                LimiteDeRetracao: 1.00m,
                LimiteDeAquecimento: 1.20m,
                LimiteDeSuperaquecimento: 1.40m,
                NomeDaFaixaIntermediaria: null,
                LimiteDaPercepcao: 5m,
                PesoDoIndicadorDePreco: null,
                PesoDoIndicadorDeCredito: null,
                PesoDoIndicadorComercial: null,
                FatorMinimo: null,
                FatorMaximo: null),
            ParametroComVigencia.HojeNoBrasil(Agora),
            "faixas decididas no texto de 21/09/2026",
            100,
            Agora);

    private static List<decimal> Meses(decimal valor, int quantos = 12) => [.. Enumerable.Repeat(valor, quantos)];

    // ---------------------------------------------------------------------------------------------
    // Momento de preço
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void O_momento_de_preco_e_a_media_recente_sobre_a_anterior()
    {
        var momento = IndicadoresDeMercado.MomentoDePreco(Meses(30m), Meses(25m), 12, Parametros());

        momento.Indice.Should().Be(1.2m, "30 ÷ 25");
        momento.MediaRecente.Should().Be(30m);
        momento.MediaAnterior.Should().Be(25m);
        momento.Motivo.Should().Be("Nenhum");
    }

    // O preço da janela anterior é sempre 25, então o índice é `recente ÷ 25`. Os limites são os do
    // texto de 21/09/2026, e o LIMITE PERTENCE À FAIXA DE BAIXO: "> 1,2 aquecido" quer dizer que 1,20
    // exato ainda não é aquecido.
    [Theory]
    [InlineData(24, "Retraido")]      // 0,96
    [InlineData(25, "Intermediaria")] // 1,00 exato
    [InlineData(30, "Intermediaria")] // 1,20 exato
    [InlineData(32, "Aquecido")]      // 1,28
    [InlineData(35, "Aquecido")]      // 1,40 exato
    [InlineData(40, "Superaquecido")] // 1,60
    public void A_faixa_do_indice_segue_os_limites_decididos(int recente, string faixa)
    {
        IndicadoresDeMercado.MomentoDePreco(Meses(recente), Meses(25m), 12, Parametros())
            .Faixa.Should().Be(faixa);
    }

    [Fact]
    public void A_janela_de_12_meses_inclui_o_ultimo_mes()
    {
        // O ACEITE DA ISSUE. A planilha da laranja desloca a janela um mês e descarta o mais recente —
        // que é justamente onde a virada aparece. Aqui, o último mês manda no resultado: trocá-lo muda o
        // índice, e se ele fosse descartado o número seria o mesmo.
        List<decimal> comAltaNoUltimoMes = [.. Meses(25m, 11), 58m];
        var semAltaNoUltimoMes = Meses(25m);

        var com = IndicadoresDeMercado.MomentoDePreco(comAltaNoUltimoMes, Meses(25m), 12, Parametros());
        var sem = IndicadoresDeMercado.MomentoDePreco(semAltaNoUltimoMes, Meses(25m), 12, Parametros());

        com.Indice.Should().NotBe(sem.Indice, "descartar o mês mais recente esconderia a virada");
        com.Indice.Should().Be(1.11m, "(25×11 + 58) ÷ 12 ÷ 25");
    }

    [Fact]
    public void Serie_curta_nao_compara_janelas_de_tamanhos_diferentes()
    {
        // Comparar 12 meses com 7 mede o tamanho da janela, e não o preço.
        var curta = IndicadoresDeMercado.MomentoDePreco(Meses(30m, 7), Meses(25m), 12, Parametros());

        curta.Indice.Should().BeNull();
        curta.Faixa.Should().BeNull();
        curta.Motivo.Should().Be("SerieCurta");
        curta.MesesRecentes.Should().Be(7);
        curta.MediaRecente.Should().Be(30m, "o que existe continua visível, para a tela dizer quanto falta");

        IndicadoresDeMercado.Frase(curta.Motivo, mesesFaltando: 5).Should().Contain("faltam 5 meses");
    }

    [Fact]
    public void Base_zerada_nao_vira_indice_infinito()
    {
        var semBase = IndicadoresDeMercado.MomentoDePreco(Meses(30m), Meses(0m), 12, Parametros());

        semBase.Indice.Should().BeNull();
        semBase.Motivo.Should().Be("SemBaseDeComparacao");
        IndicadoresDeMercado.Frase(semBase.Motivo).Should().Contain("não há do que variar");
    }

    [Fact]
    public void Sem_parametro_vigente_o_indice_sai_e_a_faixa_nao()
    {
        // O número é conta; a faixa é decisão. Sem a decisão registrada, a tela mostra o número sem
        // rotulá-lo de "aquecido".
        var momento = IndicadoresDeMercado.MomentoDePreco(Meses(30m), Meses(25m), 12, parametro: null);

        momento.Indice.Should().Be(1.2m);
        momento.Faixa.Should().BeNull();
    }

    // ---------------------------------------------------------------------------------------------
    // Crédito
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void O_indice_de_credito_compoe_70_por_cento_de_quantidade_e_30_de_valor()
    {
        // 120 linhas contra 100 = 1,20; R$ 2,2 mi contra R$ 2,0 mi = 1,10.
        // 0,7 × 1,20 + 0,3 × 1,10 = 1,17.
        var credito = IndicadoresDeMercado.Credito(
            new JanelasDeCredito(120, 2_200_000m, 100, 2_000_000m), 0.70m, linhasMinimas: null, Parametros());

        credito.IndiceDeLinhas.Should().Be(1.20m);
        credito.IndiceDeValor.Should().Be(1.10m);
        credito.Indice.Should().Be(1.17m);
        credito.Faixa.Should().Be("Intermediaria");
    }

    [Fact]
    public void O_peso_da_quantidade_separa_quem_toma_credito_de_quanto_se_gasta()
    {
        // O DEFEITO QUE O PESO EVITA: o valor sobe com a inflação e com o preço da máquina, sem nenhum
        // produtor a mais ter financiado. Aqui a quantidade caiu e o valor disparou.
        var janelas = new JanelasDeCredito(80, 3_000_000m, 100, 2_000_000m);

        var comPeso = IndicadoresDeMercado.Credito(janelas, 0.70m, null, Parametros());
        var soValor = IndicadoresDeMercado.Credito(janelas, 0m, null, Parametros());

        comPeso.Indice.Should().Be(1.01m, "0,7 × 0,80 + 0,3 × 1,50");
        soValor.Indice.Should().Be(1.50m);
        comPeso.Indice.Should().BeLessThan(soValor.Indice!.Value,
            "contar a dispersão segura o número que o valor sozinho inflaria");
    }

    [Fact]
    public void O_valor_medio_por_linha_diz_se_cresceu_por_mais_gente_ou_por_operacao_maior()
    {
        // Mesma quantidade de linhas, valor 50% maior: não entrou mais gente, entrou operação maior.
        var credito = IndicadoresDeMercado.Credito(
            new JanelasDeCredito(100, 3_000_000m, 100, 2_000_000m), 0.70m, null, Parametros());

        credito.IndiceDeLinhas.Should().Be(1m);
        credito.ValorMedioPorLinha.Should().Be(30_000m);
        credito.ValorMedioAnterior.Should().Be(20_000m);
        credito.IndiceDoValorMedio.Should().Be(1.5m);
    }

    [Fact]
    public void Linha_do_SICOR_nao_e_contrato_e_o_contrato_devolve_o_nome_certo()
    {
        // O Banco Central NÃO publica quantidade de contrato: cada linha é a soma dos contratos de uma
        // combinação (LeitorDoSicor). Chamar de contrato faria a tela afirmar um número de produtores que
        // a fonte não dá — e é por isso que nem o contrato nem a frase falam em "contrato".
        var credito = IndicadoresDeMercado.Credito(
            new JanelasDeCredito(4, 800_000m, 2, 400_000m), 0.70m, linhasMinimas: 10, Parametros());

        credito.Linhas.Should().Be(4);
        IndicadoresDeMercado.Frase(credito.Motivo, linhas: credito.Linhas)
            .Should().Contain("linhas do SICOR").And.NotContain("contrato");
    }

    [Fact]
    public void Municipio_com_poucas_linhas_sai_marcado_e_nao_corrigido_em_silencio()
    {
        // O ACEITE DA ISSUE. De 2 linhas para 4 é "+100%", e nenhuma suavização faz esse número virar
        // informação: o que ele precisa é de contexto. O índice sai, marcado, com a contagem ao lado.
        var poucas = IndicadoresDeMercado.Credito(
            new JanelasDeCredito(4, 800_000m, 2, 400_000m), 0.70m, linhasMinimas: 10, Parametros());

        poucas.Indice.Should().Be(2m, "o número é o que é — a tela é que o qualifica");
        poucas.BasePequena.Should().BeTrue();
        poucas.Linhas.Should().Be(4);
    }

    [Fact]
    public void Sem_minimo_decidido_nada_e_marcado_e_a_contagem_continua_visivel()
    {
        // O mínimo é parâmetro em aberto (D-P03): inventar um limiar aqui seria escolher no código o que
        // conta como "poucas".
        var poucas = IndicadoresDeMercado.Credito(
            new JanelasDeCredito(4, 800_000m, 2, 400_000m), 0.70m, linhasMinimas: null, Parametros());

        poucas.BasePequena.Should().BeFalse();
        poucas.Linhas.Should().Be(4, "quem lê continua vendo o tamanho da base e julga");
    }

    [Fact]
    public void Municipio_sem_credito_na_janela_anterior_nao_gera_indice()
    {
        var semBase = IndicadoresDeMercado.Credito(
            new JanelasDeCredito(5, 900_000m, 0, 0m), 0.70m, linhasMinimas: 10, Parametros());

        semBase.Indice.Should().BeNull();
        semBase.Motivo.Should().Be("SemBaseDeComparacao");
        semBase.BasePequena.Should().BeTrue("a base pequena continua sendo dita, mesmo sem índice");
        semBase.ValorMedioPorLinha.Should().Be(180_000m, "o que existe continua visível");
        semBase.IndiceDoValorMedio.Should().BeNull("sem janela anterior não há variação");
    }

    // ---------------------------------------------------------------------------------------------
    // Margem por unidade
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void A_margem_por_unidade_e_a_margem_do_hectare_repartida_pelas_sacas_dele()
    {
        // 3.000 kg/ha ÷ 60 kg = 50 sacas por hectare; R$ 6.000/ha ÷ 50 = R$ 120 por saca.
        IndicadoresDeMercado.MargemPorUnidade(6_000m, 3_000m, 60m).Should().Be(120m);
    }

    [Fact]
    public void Margem_negativa_por_unidade_e_resultado_e_nao_erro()
    {
        IndicadoresDeMercado.MargemPorUnidade(-1_500m, 3_000m, 60m).Should().Be(-30m);
    }

    [Fact]
    public void Sem_colheita_nao_ha_unidade_vendida_e_a_conta_nao_existe()
    {
        IndicadoresDeMercado.MargemPorUnidade(6_000m, 0m, 60m).Should().BeNull();
        IndicadoresDeMercado.MargemPorUnidade(6_000m, null, 60m).Should().BeNull();
        IndicadoresDeMercado.MargemPorUnidade(null, 3_000m, 60m).Should().BeNull();
    }

    [Fact]
    public void A_frase_fica_vazia_quando_o_indicador_saiu_inteiro()
    {
        IndicadoresDeMercado.Frase("Nenhum").Should().BeEmpty();
    }
    // ---------------------------------------------------------------------------------------------------------
    // O MOMENTO ANUAL PELA PAM (decisão de 27/09/2026) — enquanto a série mensal não fecha as duas janelas.
    // ---------------------------------------------------------------------------------------------------------

    /// <summary>Um ano do preço implícito do café (40139), a partir de valor e quantidade como o IBGE publica.</summary>
    private static PrecoImplicitoNoAno AnoDoCafe(short ano, decimal valorMilReais, decimal toneladas) =>
        PrecoImplicitoDaPam.De(40139, ano, valorMilReais, toneladas);

    [Fact]
    public void O_anual_da_PAM_e_o_ultimo_ano_contra_o_anterior_e_diz_de_que_serie_saiu()
    {
        // 2024: R$ 20.000 mil ÷ 1.000 t = R$ 20.000/t; 2025: R$ 24.000 mil ÷ 1.000 t = R$ 24.000/t → 1,20.
        var indice = IndicadoresDeMercado.MomentoAnualDaPam(
            [AnoDoCafe(2023, 18_000m, 1_000m), AnoDoCafe(2025, 24_000m, 1_000m), AnoDoCafe(2024, 20_000m, 1_000m)],
            Parametros());

        indice.Indice.Should().Be(1.20m);
        indice.MediaRecente.Should().Be(24_000m);
        indice.MediaAnterior.Should().Be(20_000m);
        indice.Serie.Should().Be(nameof(SerieDoIndiceDePreco.AnualPam));
        indice.AnoRecente.Should().Be((short)2025);
        indice.Motivo.Should().Be(nameof(MotivoSemIndicador.Nenhum));
        indice.Faixa.Should().Be(Parametros().FaixaDe(1.20m).ToString(), "a faixa é a mesma régua do índice mensal");
    }

    [Fact]
    public void Ano_vizinho_faltando_nao_vira_indice_de_dois_anos()
    {
        var indice = IndicadoresDeMercado.MomentoAnualDaPam(
            [AnoDoCafe(2025, 24_000m, 1_000m), AnoDoCafe(2023, 18_000m, 1_000m)], Parametros());

        indice.Indice.Should().BeNull("2025 contra 2023 mediria dois anos com o rótulo de um");
        indice.Motivo.Should().Be(nameof(MotivoSemIndicador.SerieCurta));
        indice.AnoRecente.Should().Be((short)2025);
    }

    [Fact]
    public void Ano_sem_preco_e_ignorado_e_sem_ano_nenhum_a_fonte_falta()
    {
        // 2025 sob sigilo (sem valor): o mais recente com preço é 2024, contra 2023.
        var comSigilo = IndicadoresDeMercado.MomentoAnualDaPam(
            [PrecoImplicitoDaPam.De(40139, 2025, null, 1_000m), AnoDoCafe(2024, 20_000m, 1_000m), AnoDoCafe(2023, 16_000m, 1_000m)],
            Parametros());

        comSigilo.Indice.Should().Be(1.25m);
        comSigilo.AnoRecente.Should().Be((short)2024);

        var vazio = IndicadoresDeMercado.MomentoAnualDaPam([], Parametros());
        vazio.Indice.Should().BeNull();
        vazio.Motivo.Should().Be(nameof(MotivoSemIndicador.SemFonte));
    }

    [Fact]
    public void O_indice_mensal_continua_dizendo_que_e_mensal()
    {
        var indice = IndicadoresDeMercado.MomentoDePreco(Meses(12m), Meses(10m), 12, Parametros());

        indice.Serie.Should().Be(nameof(SerieDoIndiceDePreco.Mensal));
        indice.AnoRecente.Should().BeNull();
    }
}