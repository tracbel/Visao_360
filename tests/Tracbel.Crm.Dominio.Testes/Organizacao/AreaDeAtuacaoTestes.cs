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
    // Área plantada e regra de potencial
    // =============================================================================================

    [Fact]
    public void Area_nao_disponivel_e_nula_e_diferente_de_zero()
    {
        var naoDisponivel = AreaPlantadaNoMunicipio.Registrar(1, 2024, 40139, "Café (em grão) Total", null, 1, Agora);
        var zero = AreaPlantadaNoMunicipio.Registrar(2, 2024, 40139, "Café (em grão) Total", 0m, 1, Agora);

        naoDisponivel.AreaPlantadaHectares.Should().BeNull();
        zero.AreaPlantadaHectares.Should().Be(0m);
    }

    [Fact]
    public void Reapurar_substitui_e_diz_se_mudou()
    {
        var area = AreaPlantadaNoMunicipio.Registrar(1, 2024, 40139, "Café (em grão) Total", 70m, 1, Agora);

        area.Reapurar("Café (em grão) Total", 70m, 1, Agora).Should().BeFalse();
        area.Reapurar("Café (em grão) Total", 90m, 2, Agora).Should().BeTrue();
        area.ImportadoPorId.Should().Be(2, "quem conferiu por último passa a responder pelo carimbo");
        area.AreaPlantadaHectares.Should().Be(90m);
    }

    [Fact]
    public void Area_negativa_nao_existe()
    {
        var registrar = () => AreaPlantadaNoMunicipio.Registrar(1, 2024, 40139, "Café", -1m, 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_regra_do_cafe_da_uma_maquina_a_cada_dez_hectares_e_nasce_a_confirmar()
    {
        var regra = RegraDePotencial.Informar(40139, "Café (em grão) Total", 10m, "3036N", "exemplo do comercial", new DateOnly(2026, 9, 13));

        regra.Situacao.Should().Be(SituacaoDaRegraDePotencial.AConfirmar);
        regra.MaquinasTeoricas(10000m).Should().Be(1000m);
        regra.MaquinasTeoricas(null).Should().BeNull("área não disponível não é área zero");
    }

    [Fact]
    public void Regra_sem_hectares_positivos_e_recusada()
    {
        var informar = () => RegraDePotencial.Informar(40139, "Café", 0m, "3036N", "exemplo", new DateOnly(2026, 9, 13));
        informar.Should().Throw<RegraDeNegocioViolada>();
    }
}
