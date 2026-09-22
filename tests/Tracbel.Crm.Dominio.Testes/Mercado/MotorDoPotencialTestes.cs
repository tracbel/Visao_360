using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// O MOTOR DO POTENCIAL ESTRUTURAL (issue 72) — parque de máquinas, demanda anual e relevância dentro de
/// São Paulo.
///
/// <para><b>O que estes testes conseguem provar, e o que não conseguem.</b> O critério de aceite da issue
/// é "com os parâmetros da planilha, o motor reproduz o protótipo município a município". Os parâmetros da
/// planilha <b>não estão no CRM</b>: quantos hectares por máquina e qual o ciclo de cada cultura é a
/// decisão D-P01, e a única regra registrada é o exemplo do gerente comercial ("1 trator 3036N a cada 10
/// hectares de café"), marcada como a confirmar.</para>
///
/// <para>O que dá para provar — e está provado aqui — é que <b>a conta do motor é a do protótipo</b>:
/// parque = área ÷ hectares por máquina, demanda = parque ÷ ciclo, somando as culturas. A única diferença
/// deliberada é o grupo de compartilhamento (issue 160), e <b>ele nasce desligado</b>: sem grupo
/// configurado, o motor devolve exatamente o número do protótipo. Quando os parâmetros da planilha forem
/// registrados, o aceite se confere trocando os números destes testes.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class MotorDoPotencialTestes
{
    private static CulturaNoRecorte Cultura(
        string codigo,
        decimal? area,
        decimal? haPorMaquina,
        decimal? ciclo = null,
        bool confirmada = true,
        string? grupo = null) =>
        new(codigo, codigo[..1] + codigo[1..].ToLowerInvariant(), area, haPorMaquina, ciclo, confirmada, grupo);

    // ---------------------------------------------------------------------------------------------
    // A conta do protótipo
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Sem_grupo_o_motor_faz_a_conta_do_prototipo_cultura_a_cultura()
    {
        // O PROTÓTIPO: parque = área plantada ÷ hectares por máquina, somando as culturas. Sem grupo de
        // compartilhamento configurado (D-IM-01 não saiu), é exatamente isto que o motor faz — é o que
        // torna a troca do cálculo do mapa C uma troca de implementação, e não de número.
        var potencial = MotorDoPotencial.Potencial(
        [
            Cultura("CANA", 120_000m, haPorMaquina: 600m, ciclo: 8m),
            Cultura("SOJA", 40_000m, haPorMaquina: 500m, ciclo: 10m),
            Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)
        ]);

        potencial.Parque.Should().Be(200m + 80m + 900m, "120.000÷600 + 40.000÷500 + 9.000÷10");
        potencial.AreaUtilHectares.Should().Be(169_000m, "sem grupo, a área útil é a soma das áreas");
        potencial.MotivoSemParque.Should().Be("Nenhum");
        potencial.Parcelas.Should().HaveCount(3);
    }

    [Fact]
    public void A_demanda_anual_e_o_parque_dividido_pelo_ciclo_de_renovacao()
    {
        var potencial = MotorDoPotencial.Potencial(
        [
            Cultura("CANA", 120_000m, haPorMaquina: 600m, ciclo: 8m),
            Cultura("SOJA", 40_000m, haPorMaquina: 500m, ciclo: 10m)
        ]);

        potencial.Parque.Should().Be(280m);
        potencial.DemandaAnual.Should().Be(25m + 8m, "200÷8 + 80÷10 — cada parcela com o ciclo dela");
        potencial.MotivoSemDemanda.Should().Be("Nenhum");
        potencial.CulturasSemCiclo.Should().BeEmpty();
    }

    [Fact]
    public void A_demanda_nao_sai_pela_metade_quando_falta_o_ciclo_de_uma_cultura()
    {
        // SOMAR SÓ AS QUE TÊM daria um total menor que o real, com cara de completo. O parque continua
        // saindo — ele não depende do ciclo —, e a tela diz de quem falta.
        var potencial = MotorDoPotencial.Potencial(
        [
            Cultura("CANA", 120_000m, haPorMaquina: 600m, ciclo: 8m),
            Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: null)
        ]);

        potencial.Parque.Should().Be(200m + 900m);
        potencial.DemandaAnual.Should().BeNull();
        potencial.MotivoSemDemanda.Should().Be("SemCicloDeRenovacao");
        potencial.CulturasSemCiclo.Should().Equal(["Cafe"]);
        MotorDoPotencial.Frase(potencial).Should().Contain("falta o ciclo de renovação de Cafe");
    }

    [Fact]
    public void O_grupo_de_compartilhamento_nao_conta_a_mesma_terra_duas_vezes()
    {
        // A soja e o milho safrinha dividem o talhão (achado C-02). No grupo, a área útil é a MAIOR.
        var comGrupo = MotorDoPotencial.Potencial(
        [
            Cultura("MILHO", 8_000m, haPorMaquina: 400m, ciclo: 10m, grupo: "GRAOS-TRATOR"),
            Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m, grupo: "GRAOS-TRATOR")
        ]);

        comGrupo.Parque.Should().Be(20m, "10.000 ÷ 500, a regra da soja, que é a dominante");
        comGrupo.AreaUtilHectares.Should().Be(10_000m);
        comGrupo.Parcelas.Should().ContainSingle();
        comGrupo.Parcelas[0].Compartilhada.Should().Equal(["Milho"]);
    }

    [Fact]
    public void Dois_grupos_e_uma_cultura_solta_somam_entre_si()
    {
        var potencial = MotorDoPotencial.Potencial(
        [
            Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m, grupo: "GRAOS"),
            Cultura("MILHO", 8_000m, haPorMaquina: 400m, ciclo: 10m, grupo: "GRAOS"),
            Cultura("CANA", 30_000m, haPorMaquina: 600m, ciclo: 8m, grupo: "CANA-AMENDOIM"),
            Cultura("AMENDOIM", 6_000m, haPorMaquina: 300m, ciclo: 8m, grupo: "CANA-AMENDOIM"),
            Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)
        ]);

        potencial.Parque.Should().Be(20m + 50m + 900m, "10.000÷500 + 30.000÷600 + 9.000÷10");
        potencial.Parcelas.Should().HaveCount(3, "dois grupos e uma cultura solta");
        potencial.Parcelas.Select(p => p.CulturaCodigo).Should().Equal(["CAFE", "CANA", "SOJA"],
            "a ordem é pelo código da dominante, e não pela ordem de quem chamou");
    }

    [Fact]
    public void A_ordem_da_entrada_nao_muda_o_resultado()
    {
        // O empate de área e a escolha da dominante não podem depender da ordem em que o banco devolveu.
        var culturas = new[]
        {
            Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m, grupo: "GRAOS"),
            Cultura("MILHO", 10_000m, haPorMaquina: 400m, ciclo: 10m, grupo: "GRAOS"),
            Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)
        };

        var direta = MotorDoPotencial.Potencial(culturas);
        var invertida = MotorDoPotencial.Potencial([.. culturas.Reverse()]);

        invertida.Should().BeEquivalentTo(direta);
        direta.Parcelas.Single(p => p.CulturaCodigo == "MILHO").Parque.Should().Be(25m,
            "empate de área fica com o primeiro código — MILHO antes de SOJA");
    }

    // ---------------------------------------------------------------------------------------------
    // Vazio com motivo
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Sem_area_divulgada_o_parque_e_vazio_com_motivo_e_nao_zero()
    {
        // Zero diria "não há potencial aqui"; o certo é "o IBGE não divulgou" — o município com poucos
        // produtores fica em sigilo.
        var potencial = MotorDoPotencial.Potencial([Cultura("CAFE", null, haPorMaquina: 10m, ciclo: 12m)]);

        potencial.Parque.Should().BeNull();
        potencial.AreaUtilHectares.Should().BeNull();
        potencial.MotivoSemParque.Should().Be("SemArea");
        MotorDoPotencial.Frase(potencial).Should().Contain("sigilo");
    }

    [Fact]
    public void Com_area_e_sem_regra_o_motivo_diz_que_falta_a_regra_e_nao_a_area()
    {
        // A ordem dos motivos importa: quem tem 9.000 ha de café e nenhuma regra precisa ouvir "falta a
        // regra", e não "falta área".
        var potencial = MotorDoPotencial.Potencial([Cultura("CAFE", 9_000m, haPorMaquina: null)]);

        potencial.Parque.Should().BeNull();
        potencial.MotivoSemParque.Should().Be("SemRegra");
        potencial.MotivoSemDemanda.Should().Be("SemRegra", "sem parque não há o que renovar");
        MotorDoPotencial.Frase(potencial).Should().Contain("nenhuma cultura daqui tem regra");
    }

    [Fact]
    public void Recorte_sem_cultura_nenhuma_nao_devolve_zero()
    {
        var potencial = MotorDoPotencial.Potencial([]);

        potencial.Parque.Should().BeNull();
        potencial.DemandaAnual.Should().BeNull();
        potencial.Parcelas.Should().BeEmpty();
        potencial.MotivoSemParque.Should().Be("SemArea");
    }

    // ---------------------------------------------------------------------------------------------
    // O selo de estimativa (D-P01)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void A_regra_a_confirmar_acende_o_selo_de_estimativa()
    {
        // É o estado de hoje: a única regra do CRM é o exemplo do gerente comercial, e ninguém a confirmou.
        var potencial = MotorDoPotencial.Potencial(
            [Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m, confirmada: false)]);

        potencial.Parque.Should().Be(900m, "o número sai — o selo avisa, não esconde");
        potencial.Estimativa.Should().BeTrue();
        MotorDoPotencial.Frase(potencial).Should().Contain("ainda não foi confirmada pelo comercial");
    }

    [Fact]
    public void Com_a_regra_confirmada_o_selo_apaga_e_a_frase_fica_vazia()
    {
        var potencial = MotorDoPotencial.Potencial(
            [Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m, confirmada: true)]);

        potencial.Estimativa.Should().BeFalse();
        MotorDoPotencial.Frase(potencial).Should().BeEmpty(
            "a tela não põe rodapé para dizer que está tudo certo");
    }

    [Fact]
    public void A_regra_a_confirmar_de_quem_nao_dimensionou_a_maquina_nao_acende_o_selo()
    {
        // Só a dominante do grupo dimensiona: a regra do milho não entrou na conta, logo não torna
        // estimativa um número que não a usou.
        var potencial = MotorDoPotencial.Potencial(
        [
            Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m, confirmada: true, grupo: "GRAOS"),
            Cultura("MILHO", 8_000m, haPorMaquina: 400m, ciclo: 10m, confirmada: false, grupo: "GRAOS")
        ]);

        potencial.Parque.Should().Be(20m);
        potencial.Estimativa.Should().BeFalse();
    }

    [Fact]
    public void Cultura_sem_area_com_regra_a_confirmar_nao_acende_o_selo()
    {
        // Ela não gerou parque nenhum: não há número dela para ressalvar.
        var potencial = MotorDoPotencial.Potencial(
        [
            Cultura("CANA", 120_000m, haPorMaquina: 600m, ciclo: 8m, confirmada: true),
            Cultura("SOJA", null, haPorMaquina: 500m, ciclo: 10m, confirmada: false)
        ]);

        potencial.Estimativa.Should().BeFalse();
    }

    // ---------------------------------------------------------------------------------------------
    // O recorte maior é a soma dos municípios
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void A_regiao_e_a_soma_dos_municipios_e_nao_o_motor_sobre_as_areas_somadas()
    {
        // O DEFEITO QUE ESTE TESTE IMPEDE: aplicar "a área útil é a maior" sobre a soja da região inteira
        // contra o milho da região inteira afirmaria que a soja de um município e o milho do vizinho são a
        // mesma terra. O compartilhamento acontece NO CHÃO — dentro do município.
        var ribeirao = MotorDoPotencial.Potencial(
        [
            Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m, grupo: "GRAOS"),
            Cultura("MILHO", 8_000m, haPorMaquina: 400m, ciclo: 10m, grupo: "GRAOS")
        ]);

        var franca = MotorDoPotencial.Potencial(
        [
            Cultura("SOJA", 2_000m, haPorMaquina: 500m, ciclo: 10m, grupo: "GRAOS"),
            Cultura("MILHO", 6_000m, haPorMaquina: 400m, ciclo: 10m, grupo: "GRAOS")
        ]);

        var regiao = MotorDoPotencial.Somar([ribeirao, franca]);

        regiao.Parque.Should().Be(20m + 15m, "10.000÷500 em Ribeirão e 6.000÷400 em Franca");
        regiao.Parque.Should().NotBe(24m, "24 seria 12.000÷500 — a soja das duas cidades como um talhão só");
        regiao.DemandaAnual.Should().Be(3.5m);
        regiao.AreaUtilHectares.Should().Be(16_000m);
    }

    [Fact]
    public void O_total_da_regiao_fecha_com_a_soma_da_coluna_da_tela()
    {
        // Quem conferir no papel soma a coluna e tem de chegar no mesmo número do cabeçalho.
        var municipios = new[]
        {
            MotorDoPotencial.Potencial([Cultura("CANA", 30_000m, haPorMaquina: 600m, ciclo: 8m)]),
            MotorDoPotencial.Potencial([Cultura("CANA", 12_000m, haPorMaquina: 600m, ciclo: 8m)]),
            MotorDoPotencial.Potencial([Cultura("CANA", null, haPorMaquina: 600m, ciclo: 8m)])
        };

        MotorDoPotencial.Somar(municipios).Parque.Should().Be(municipios.Sum(m => m.Parque ?? 0m));
    }

    [Fact]
    public void As_parcelas_da_regiao_se_juntam_por_cultura()
    {
        var regiao = MotorDoPotencial.Somar(
        [
            MotorDoPotencial.Potencial([Cultura("CANA", 30_000m, haPorMaquina: 600m, ciclo: 8m)]),
            MotorDoPotencial.Potencial(
            [
                Cultura("CANA", 12_000m, haPorMaquina: 600m, ciclo: 8m),
                Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)
            ])
        ]);

        regiao.Parcelas.Should().HaveCount(2);
        regiao.Parcelas.Single(p => p.CulturaCodigo == "CANA").Parque.Should().Be(70m, "50 + 20");
        regiao.Parcelas.Single(p => p.CulturaCodigo == "CANA").AreaUtilHectares.Should().Be(42_000m);
    }

    [Fact]
    public void Uma_cidade_sem_ciclo_deixa_a_demanda_da_regiao_vazia_com_o_nome_dela()
    {
        var regiao = MotorDoPotencial.Somar(
        [
            MotorDoPotencial.Potencial([Cultura("CANA", 30_000m, haPorMaquina: 600m, ciclo: 8m)]),
            MotorDoPotencial.Potencial([Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: null)])
        ]);

        regiao.Parque.Should().Be(50m + 900m);
        regiao.DemandaAnual.Should().BeNull();
        regiao.CulturasSemCiclo.Should().Equal(["Cafe"]);
    }

    [Fact]
    public void Na_regiao_sem_parque_em_lugar_nenhum_sem_regra_ganha_de_sem_area()
    {
        // Quem tem 9.000 ha e nenhuma regra precisa ouvir o que falta de verdade.
        var regiao = MotorDoPotencial.Somar(
        [
            MotorDoPotencial.Potencial([Cultura("CAFE", null, haPorMaquina: 10m)]),
            MotorDoPotencial.Potencial([Cultura("CAFE", 9_000m, haPorMaquina: null)])
        ]);

        regiao.Parque.Should().BeNull();
        regiao.MotivoSemParque.Should().Be("SemRegra");
    }

    [Fact]
    public void As_categorias_de_maquina_somam_maquina_e_nao_somam_terra()
    {
        // O MESMO TALHÃO recebe o trator e a colheitadeira: são duas máquinas, e é uma lavoura só. Somar
        // a área das duas categorias diria que o município tem o dobro da lavoura que tem.
        var trator = MotorDoPotencial.Potencial([Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m)]);
        var colheitadeira = MotorDoPotencial.Potencial([Cultura("SOJA", 10_000m, haPorMaquina: 2_000m, ciclo: 8m)]);

        var municipio = MotorDoPotencial.Sobrepor([trator, colheitadeira]);

        municipio.Parque.Should().Be(20m + 5m, "20 tratores e 5 colheitadeiras");
        municipio.AreaUtilHectares.Should().Be(10_000m, "a soja é a mesma — e não 20.000");
        municipio.DemandaAnual.Should().Be(2m + 0.625m);
    }

    [Fact]
    public void Sobrepor_e_somar_so_diferem_na_terra()
    {
        var a = MotorDoPotencial.Potencial([Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m)]);
        var b = MotorDoPotencial.Potencial([Cultura("SOJA", 10_000m, haPorMaquina: 2_000m, ciclo: 8m)]);

        MotorDoPotencial.Somar([a, b]).Parque.Should().Be(MotorDoPotencial.Sobrepor([a, b]).Parque);
        MotorDoPotencial.Somar([a, b]).AreaUtilHectares.Should().Be(20_000m, "municípios diferentes somam terra");
        MotorDoPotencial.Sobrepor([a, b]).AreaUtilHectares.Should().Be(10_000m, "categorias dividem a mesma terra");
    }

    [Fact]
    public void O_selo_de_estimativa_de_um_municipio_marca_a_regiao_inteira()
    {
        var regiao = MotorDoPotencial.Somar(
        [
            MotorDoPotencial.Potencial([Cultura("CANA", 30_000m, haPorMaquina: 600m, ciclo: 8m, confirmada: true)]),
            MotorDoPotencial.Potencial([Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m, confirmada: false)])
        ]);

        regiao.Estimativa.Should().BeTrue("o total carrega um número que veio de regra não confirmada");
    }

    // ---------------------------------------------------------------------------------------------
    // A calculadora (o aceite da issue 161)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void A_calculadora_com_a_area_medida_devolve_exatamente_o_numero_do_mapa()
    {
        // O CRITÉRIO DE ACEITE DA ISSUE 161, garantido por construção: a simulação não tem fórmula
        // própria — ela troca a área e chama a mesma função. Passando as áreas medidas, o resultado é o
        // mesmo objeto.
        var recorte = new[]
        {
            Cultura("CANA", 120_000m, haPorMaquina: 600m, ciclo: 8m),
            Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)
        };

        var doMapa = MotorDoPotencial.Potencial(recorte);
        var simulado = MotorDoPotencial.Simular(recorte, new Dictionary<string, decimal>
        {
            ["CANA"] = 120_000m,
            ["CAFE"] = 9_000m
        });

        simulado.Should().BeEquivalentTo(doMapa);
    }

    [Fact]
    public void A_calculadora_troca_so_a_cultura_informada_e_preserva_o_resto()
    {
        // "E se este município tivesse 20.000 ha de cana?" não pode apagar o café que ele tem.
        var recorte = new[]
        {
            Cultura("CANA", 120_000m, haPorMaquina: 600m, ciclo: 8m),
            Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)
        };

        var simulado = MotorDoPotencial.Simular(recorte, new Dictionary<string, decimal> { ["CANA"] = 20_000m });

        simulado.Parque.Should().Be(33m + 900m + (1m / 3m), "20.000÷600 + 9.000÷10");
        simulado.Parcelas.Single(p => p.CulturaCodigo == "CAFE").Parque.Should().Be(900m);
    }

    [Fact]
    public void A_calculadora_de_uma_cultura_que_ninguem_parametrizou_diz_que_falta_a_regra()
    {
        // Simular uma cultura sem regra não pode devolver máquina nenhuma em silêncio.
        var simulado = MotorDoPotencial.Simular(
            [Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)],
            new Dictionary<string, decimal> { ["UVA"] = 2_000m });

        simulado.Parque.Should().Be(900m, "só o café tem regra");
        simulado.Parcelas.Single(p => p.CulturaCodigo == "UVA").Motivo.Should().Be("SemRegra");
    }

    [Fact]
    public void A_calculadora_com_area_zero_zera_a_parcela_sem_derrubar_o_recorte()
    {
        // "E se a cana sumisse daqui?" é uma pergunta legítima — e a resposta é o resto do município.
        var simulado = MotorDoPotencial.Simular(
        [
            Cultura("CANA", 120_000m, haPorMaquina: 600m, ciclo: 8m),
            Cultura("CAFE", 9_000m, haPorMaquina: 10m, ciclo: 12m)
        ], new Dictionary<string, decimal> { ["CANA"] = 0m });

        simulado.Parque.Should().Be(900m);
        simulado.Parcelas.Single(p => p.CulturaCodigo == "CANA").Parque.Should().Be(0m);
        simulado.Parcelas.Single(p => p.CulturaCodigo == "CANA").Motivo.Should().Be("Nenhum",
            "zero hectare é resposta, e a resposta é zero máquina");
    }

    [Fact]
    public void Area_zero_da_zero_maquinas_e_area_nao_divulgada_nao_da_numero_nenhum()
    {
        // A DIFERENÇA QUE ESTE TESTE GUARDA: zero é o IBGE dizendo "não se planta café aqui" — e a
        // resposta certa é zero. Nulo é "não se sabe" — e aí não há resposta.
        var zero = MotorDoPotencial.Potencial([Cultura("CAFE", 0m, haPorMaquina: 10m, ciclo: 12m)]);
        var semDado = MotorDoPotencial.Potencial([Cultura("CAFE", null, haPorMaquina: 10m, ciclo: 12m)]);

        zero.Parque.Should().Be(0m);
        zero.DemandaAnual.Should().Be(0m);
        zero.AreaUtilHectares.Should().Be(0m);
        zero.MotivoSemParque.Should().Be("Nenhum");

        semDado.Parque.Should().BeNull();
        semDado.MotivoSemParque.Should().Be("SemArea");
    }

    [Fact]
    public void Quem_tem_zero_hectare_nao_aparece_como_quem_divide_a_terra()
    {
        var potencial = MotorDoPotencial.Potencial(
        [
            Cultura("SOJA", 10_000m, haPorMaquina: 500m, ciclo: 10m, grupo: "GRAOS"),
            Cultura("MILHO", 0m, haPorMaquina: 400m, ciclo: 10m, grupo: "GRAOS")
        ]);

        potencial.Parque.Should().Be(20m);
        potencial.Parcelas[0].Compartilhada.Should().BeEmpty(
            "'compartilhada com' é sobre terra, e zero hectare não é terra dividida");
    }

    // ---------------------------------------------------------------------------------------------
    // A relevância dentro de São Paulo
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void A_relevancia_e_a_fatia_do_que_o_IBGE_publica_para_o_estado()
    {
        // O número medido da aba "Relevância vs SP" (documento 48): a região tem 40,3% da área plantada
        // de São Paulo e 42,8% do valor da produção.
        var relevancia = MotorDoPotencial.Relevancia(
            new MedidasDaLavoura(403_000m, 400_000m, 1_200_000m, 8_560_000m),
            new MedidasDaLavoura(1_000_000m, 980_000m, 3_000_000m, 20_000_000m));

        relevancia.FatiaDaAreaPlantada.Should().Be(40.3m);
        relevancia.FatiaDoValor.Should().Be(42.8m);
        relevancia.FatiaDaQuantidade.Should().Be(40m);
        relevancia.FatiaDaAreaColhida!.Value.Should().BeApproximately(40.816m, 0.001m);
    }

    [Fact]
    public void A_razao_de_produtividade_compara_a_terra_daqui_com_a_media_do_estado()
    {
        // 3 t/ha aqui contra 2,5 t/ha no estado: 1,20 é "20% acima da média de São Paulo".
        var relevancia = MotorDoPotencial.Relevancia(
            new MedidasDaLavoura(1_000m, 1_000m, 3_000m, null),
            new MedidasDaLavoura(10_000m, 10_000m, 25_000m, null));

        relevancia.ProdutividadeDoRecorte.Should().Be(3m);
        relevancia.ProdutividadeNoEstado.Should().Be(2.5m);
        relevancia.RazaoDeProdutividade.Should().Be(1.2m);
    }

    [Fact]
    public void Denominador_ausente_ou_zerado_nao_vira_cem_por_cento()
    {
        // Dividir por zero não é "tudo": é "não dá para dizer". A fonte do estado pode não estar
        // carregada, e nesse caso a tela não mostra fatia nenhuma.
        var semEstado = MotorDoPotencial.Relevancia(
            new MedidasDaLavoura(403_000m, 400_000m, 1_200_000m, 8_560_000m),
            new MedidasDaLavoura(null, 0m, null, 0m));

        semEstado.FatiaDaAreaPlantada.Should().BeNull();
        semEstado.FatiaDaAreaColhida.Should().BeNull();
        semEstado.FatiaDoValor.Should().BeNull();
        semEstado.RazaoDeProdutividade.Should().BeNull();
    }

    [Fact]
    public void Numerador_zero_com_denominador_de_pe_e_zero_por_cento_e_isso_e_informacao()
    {
        var relevancia = MotorDoPotencial.Relevancia(
            new MedidasDaLavoura(0m, 0m, 0m, 0m),
            new MedidasDaLavoura(1_000_000m, 980_000m, 3_000_000m, 20_000_000m));

        relevancia.FatiaDaAreaPlantada.Should().Be(0m);
        relevancia.FatiaDoValor.Should().Be(0m);
    }
}
