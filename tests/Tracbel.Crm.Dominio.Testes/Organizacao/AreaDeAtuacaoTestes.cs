using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O território da Tracbel Agro no domínio (documento 32): o município oficial, a ADR, quem as
/// planilhas dizem que responde por cada cidade, a área plantada e a regra de potencial.
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class AreaDeAtuacaoTestes
{
    private static readonly DateTime Agora = new(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);

    // =============================================================================================
    // Município no IBGE
    // =============================================================================================

    [Fact]
    public void Reconhecer_no_ibge_troca_o_nome_cortado_pelo_oficial_e_guarda_o_codigo()
    {
        var municipio = Municipio.Criar("SANTA CRUZ DA ESPERA", "SP");

        municipio.ReconhecerNoIbge(3546256, "Santa Cruz da Esperança").Should().BeTrue();

        municipio.CodigoIbge.Should().Be(3546256);
        municipio.Nome.Should().Be("Santa Cruz da Esperança");
    }

    [Fact]
    public void Reconhecer_de_novo_com_o_mesmo_codigo_nao_muda_nada()
    {
        var municipio = Municipio.Criar("Cajuru", "SP", 3509601);

        municipio.ReconhecerNoIbge(3509601, "Cajuru").Should().BeFalse();
    }

    [Fact]
    public void Codigo_ja_reconhecido_nao_troca_por_outro()
    {
        var municipio = Municipio.Criar("Cajuru", "SP", 3509601);

        var trocar = () => municipio.ReconhecerNoIbge(3543402, "Ribeirão Preto");

        trocar.Should().Throw<RegraDeNegocioViolada>("trocaria o município de todo endereço que aponta para a linha");
    }

    [Theory]
    [InlineData(999999)]
    [InlineData(6000000)]
    public void Codigo_fora_dos_sete_digitos_do_ibge_e_recusado(int codigo)
    {
        var reconhecer = () => Municipio.Criar("X", "SP").ReconhecerNoIbge(codigo, "X");
        reconhecer.Should().Throw<RegraDeNegocioViolada>();
    }

    // =============================================================================================
    // Município da área de atuação
    // =============================================================================================

    [Fact]
    public void A_linha_1_da_planilha_e_o_cabecalho_e_nao_tem_municipio()
    {
        var registrar = () => MunicipioDaAreaDeAtuacao.Registrar(
            1, true, RegiaoDaAreaDeAtuacao.Norte, 902, "Area de Atuação.xlsx", 1, 1, Agora);

        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Conferir_diz_se_mudou_e_reabre_o_municipio_que_tinha_saido()
    {
        var linha = MunicipioDaAreaDeAtuacao.Registrar(
            1, true, RegiaoDaAreaDeAtuacao.Norte, 902, "Area de Atuação.xlsx", 5, 1, Agora);

        linha.Conferir(true, RegiaoDaAreaDeAtuacao.Norte, 902, "Area de Atuação.xlsx", 7, 1, Agora)
            .Should().BeFalse("mudar de linha na planilha não muda o que ela afirma");

        linha.Encerrar(Agora);
        linha.EncerradoEm.Should().Be(Agora);

        linha.Conferir(true, RegiaoDaAreaDeAtuacao.Norte, 902, "Area de Atuação.xlsx", 7, 2, Agora.AddDays(1))
            .Should().BeTrue();
        linha.EncerradoEm.Should().BeNull();
        linha.ImportadoPorId.Should().Be(2, "a conferência de outra pessoa troca o dono do carimbo");
    }

    // =============================================================================================
    // Responsável pelo município
    // =============================================================================================

    [Fact]
    public void Identificado_sem_usuario_e_recusado()
    {
        var registrar = () => ResponsavelPeloMunicipio.Registrar(
            1, PapelNoMunicipio.Cen, FonteDoResponsavel.PlanilhaCenEGestorPorMunicipio, "FULANO.DETAL",
            SituacaoDoResponsavel.UsuarioIdentificado, null, "00204", "CEN e Gestor por Municipio.xlsx", 3, 1, Agora);

        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Vaga_a_contratar_com_usuario_e_recusada()
    {
        var registrar = () => ResponsavelPeloMunicipio.Registrar(
            1, PapelNoMunicipio.Cen, FonteDoResponsavel.PlanilhaAreaDeAtuacao, "A Contratar 3",
            SituacaoDoResponsavel.VagaAContratar, 10, null, "Area de Atuação.xlsx", 3, 1, Agora);

        registrar.Should().Throw<RegraDeNegocioViolada>("inventaria quem ocupa uma vaga em aberto");
    }

    [Fact]
    public void A_mesma_afirmacao_e_reconhecida_mesmo_com_espaco_sobrando_na_celula()
    {
        var responsavel = ResponsavelPeloMunicipio.Registrar(
            1, PapelNoMunicipio.Gestor, FonteDoResponsavel.PlanilhaCenEGestorPorMunicipio, "Fulano de Tal",
            SituacaoDoResponsavel.UsuarioIdentificado, 10, "00204", "CEN e Gestor por Municipio.xlsx", 3, 1, Agora);

        responsavel.AfirmaOMesmo(" Fulano de Tal ", SituacaoDoResponsavel.UsuarioIdentificado, 10).Should().BeTrue();
        responsavel.AfirmaOMesmo("Fulano de Tal", SituacaoDoResponsavel.NaoIdentificado, null).Should().BeFalse();
    }

    // =============================================================================================
    // Produção agrícola (PAM) e regra de potencial
    // =============================================================================================

    /// <summary>As quatro medidas de um produto que existe e foi todo divulgado.</summary>
    private static MedidasDaProducaoAgricola Medidas(
        decimal? plantada = 70m, decimal? colhida = 68m, decimal? quantidade = 200m, decimal? valor = 1500m) =>
        new(plantada, colhida, quantidade, valor);

    [Fact]
    public void Medida_nao_disponivel_e_nula_e_diferente_de_zero()
    {
        var naoDisponivel = ProducaoAgricolaNoMunicipio.Registrar(
            1, 2024, 40139, "Café (em grão) Total", Medidas(plantada: null), 1, Agora);
        var zero = ProducaoAgricolaNoMunicipio.Registrar(
            2, 2024, 40139, "Café (em grão) Total", Medidas(plantada: 0m), 1, Agora);

        naoDisponivel.AreaPlantadaHectares.Should().BeNull();
        zero.AreaPlantadaHectares.Should().Be(0m);
    }

    [Fact]
    public void As_quatro_medidas_ficam_na_mesma_linha()
    {
        var producao = ProducaoAgricolaNoMunicipio.Registrar(
            1, 2025, 40106, "Cana-de-açúcar", new(71500m, 71000m, 5_720_000m, 1_200_000m), 1, Agora);

        producao.AreaPlantadaHectares.Should().Be(71500m);
        producao.AreaColhidaHectares.Should().Be(71000m, "plantada e colhida são medidas diferentes (issue 83)");
        producao.QuantidadeProduzida.Should().Be(5_720_000m);
        producao.ValorDaProducaoMilReais.Should().Be(1_200_000m, "o IBGE publica o valor em MIL reais");
    }

    [Fact]
    public void Reapurar_substitui_e_diz_se_mudou()
    {
        var producao = ProducaoAgricolaNoMunicipio.Registrar(
            1, 2024, 40139, "Café (em grão) Total", Medidas(), 1, Agora);

        producao.Reapurar("Café (em grão) Total", Medidas(), 1, Agora).Should().BeFalse();
        producao.Reapurar("Café (em grão) Total", Medidas(plantada: 90m), 2, Agora).Should().BeTrue();
        producao.ImportadoPorId.Should().Be(2, "quem conferiu por último passa a responder pelo carimbo");
        producao.AreaPlantadaHectares.Should().Be(90m);

        producao.Reapurar("Café (em grão) Total", Medidas(plantada: 90m, valor: 1600m), 2, Agora)
            .Should().BeTrue("mudar só o valor da produção também é mudança");
    }

    [Fact]
    public void Medida_negativa_nao_existe()
    {
        var registrar = () => ProducaoAgricolaNoMunicipio.Registrar(
            1, 2024, 40139, "Café", Medidas(plantada: -1m), 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();

        var comValorNegativo = () => ProducaoAgricolaNoMunicipio.Registrar(
            1, 2024, 40139, "Café", Medidas(valor: -1m), 1, Agora);
        comValorNegativo.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void O_total_do_estado_exige_uma_uf_que_existe()
    {
        var saoPaulo = ProducaoAgricolaNoEstado.Registrar(
            35, 2025, 40106, "Cana-de-açúcar", Medidas(plantada: 5_430_681m), 1, Agora);

        saoPaulo.EstadoCodigoIbge.Should().Be(35);
        saoPaulo.AreaPlantadaHectares.Should().Be(5_430_681m);

        var inventada = () => ProducaoAgricolaNoEstado.Registrar(
            99, 2025, 40106, "Cana-de-açúcar", Medidas(), 1, Agora);
        inventada.Should().Throw<RegraDeNegocioViolada>("o código de UF do IBGE vai de 11 a 53");
    }
    // =============================================================================================
    // O milho por safra (issue 156)
    // =============================================================================================

    [Fact]
    public void O_milho_por_safra_guarda_as_tres_medidas_da_839()
    {
        // A 839 NÃO PUBLICA VALOR DA PRODUÇÃO: ele existe só na 5457, para o milho inteiro. A
        // medida vem no mesmo registro das outras cargas, e o valor é ignorado de propósito.
        var safrinha = ProducaoDeMilhoPorSafraNoMunicipio.Registrar(
            1, 2024, 114254, "Milho (em grão) - 2ª safra", Medidas(plantada: 12_000m, colhida: 11_800m, quantidade: 70_000m), 1, Agora);

        safrinha.SafraCodigoIbge.Should().Be(114254);
        safrinha.SafraNome.Should().Be("Milho (em grão) - 2ª safra");
        safrinha.AreaPlantadaHectares.Should().Be(12_000m);
        safrinha.AreaColhidaHectares.Should().Be(11_800m);
        safrinha.QuantidadeProduzida.Should().Be(70_000m);
    }

    [Fact]
    public void O_milho_por_safra_recusa_ano_e_safra_que_nao_existem()
    {
        var anoAntesDaPam = () => ProducaoDeMilhoPorSafraNoMunicipio.Registrar(
            1, 1970, 114253, "Milho (em grão) - 1ª safra", Medidas(), 1, Agora);
        var semCodigo = () => ProducaoDeMilhoPorSafraNoMunicipio.Registrar(
            1, 2024, 0, "Milho (em grão) - 1ª safra", Medidas(), 1, Agora);
        var semRotulo = () => ProducaoDeMilhoPorSafraNoMunicipio.Registrar(
            1, 2024, 114253, "  ", Medidas(), 1, Agora);

        anoAntesDaPam.Should().Throw<RegraDeNegocioViolada>();
        semCodigo.Should().Throw<RegraDeNegocioViolada>();
        semRotulo.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Reapurar_o_milho_por_safra_diz_se_a_leitura_mudou()
    {
        var safra = ProducaoDeMilhoPorSafraNoMunicipio.Registrar(
            1, 2024, 114253, "Milho (em grão) - 1ª safra", Medidas(plantada: 5_000m), 1, Agora);

        safra.Reapurar("Milho (em grão) - 1ª safra", Medidas(plantada: 5_000m), 2, Agora)
            .Should().BeFalse("o IBGE republicou o mesmo número");

        safra.Reapurar("Milho (em grão) - 1ª safra", Medidas(plantada: 5_100m), 2, Agora).Should().BeTrue();
        safra.AreaPlantadaHectares.Should().Be(5_100m);
        safra.ImportadoPorId.Should().Be(2);
    }
}