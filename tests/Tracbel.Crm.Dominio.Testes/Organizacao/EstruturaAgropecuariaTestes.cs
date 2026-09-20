using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// A ESTRUTURA AGROPECUÁRIA no domínio (issue 65): o que já existe no território para mecanizar —
/// tratores, propriedades, rebanho, área e usinas.
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class EstruturaAgropecuariaTestes
{
    private static readonly DateTime Agora = new(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Julho = new(2026, 7, 1);

    // =============================================================================================
    // Frota de tratores
    // =============================================================================================

    [Fact]
    public void Sigilo_e_nulo_e_e_diferente_de_zero()
    {
        // O IBGE oculta o número quando poucos estabelecimentos o compõem. Gravar isso como zero
        // diria que o município não tem trator — quando só não foi divulgado.
        var sigilosa = FrotaDeTratoresNoMunicipio.Registrar(
            1, 2017, 113523, "De 100 cv e mais", null, null, 1, Agora);
        var semNenhum = FrotaDeTratoresNoMunicipio.Registrar(
            2, 2017, 113523, "De 100 cv e mais", 0, 0, 1, Agora);

        sigilosa.Tratores.Should().BeNull();
        semNenhum.Tratores.Should().Be(0);
    }

    [Fact]
    public void As_duas_contagens_da_frota_ficam_na_mesma_linha()
    {
        var frota = FrotaDeTratoresNoMunicipio.Registrar(
            1, 2017, 113521, "Total", 172, 450, 1, Agora);

        frota.EstabelecimentosComTrator.Should().Be(172);
        frota.Tratores.Should().Be(450, "450 tratores em 172 estabelecimentos é a frota média do lugar");
    }

    [Fact]
    public void Reapurar_a_frota_substitui_e_diz_se_mudou()
    {
        var frota = FrotaDeTratoresNoMunicipio.Registrar(1, 2017, 113521, "Total", 172, 450, 1, Agora);

        frota.Reapurar("Total", 172, 450, 1, Agora).Should().BeFalse();
        frota.Reapurar("Total", 172, 460, 2, Agora).Should().BeTrue();
        frota.Tratores.Should().Be(460);
        frota.ImportadoPorId.Should().Be(2, "quem conferiu por último responde pelo carimbo");
    }

    [Fact]
    public void Contagem_negativa_nao_existe()
    {
        var registrar = () => FrotaDeTratoresNoMunicipio.Registrar(1, 2017, 113521, "Total", 1, -1, 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Theory]
    [InlineData((short)1919)]
    [InlineData((short)2101)]
    public void Ano_fora_do_intervalo_das_pesquisas_e_recusado(short ano)
    {
        var registrar = () => FrotaDeTratoresNoMunicipio.Registrar(1, ano, 113521, "Total", 1, 1, 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Categoria_sem_codigo_ou_sem_rotulo_e_recusada()
    {
        var semCodigo = () => FrotaDeTratoresNoMunicipio.Registrar(1, 2017, 0, "Total", 1, 1, 1, Agora);
        semCodigo.Should().Throw<RegraDeNegocioViolada>();

        var semRotulo = () => FrotaDeTratoresNoMunicipio.Registrar(1, 2017, 113521, "  ", 1, 1, 1, Agora);
        semRotulo.Should().Throw<RegraDeNegocioViolada>("o rótulo oficial é o que a tela mostra");
    }

    // =============================================================================================
    // Estabelecimentos por área e rebanho
    // =============================================================================================

    [Fact]
    public void A_faixa_de_area_guarda_o_codigo_e_o_rotulo_do_ibge()
    {
        var faixa = EstabelecimentosPorAreaNoMunicipio.Registrar(
            1, 2017, 111553, "De 20 a menos de 50 ha", 13, 1, Agora);

        faixa.GrupoDeAreaCodigoIbge.Should().Be(111553);
        faixa.GrupoDeAreaNome.Should().Be("De 20 a menos de 50 ha");
        faixa.Estabelecimentos.Should().Be(13);
    }

    [Fact]
    public void O_rebanho_e_anual_e_guarda_o_tipo()
    {
        // A PPM é anual: ela sabe de 2024 enquanto o Censo ainda fala de 2017. É por isso que o
        // rebanho tem tabela própria em vez de virar mais uma coluna do Censo.
        var rebanho = RebanhoNoMunicipio.Registrar(1, 2024, 2670, "Bovino", 3000, 1, Agora);

        rebanho.Ano.Should().Be(2024);
        rebanho.RebanhoCodigoIbge.Should().Be(2670);
        rebanho.Cabecas.Should().Be(3000);

        rebanho.Reapurar("Bovino", 3000, 1, Agora).Should().BeFalse();
        rebanho.Reapurar("Bovino", 3100, 1, Agora).Should().BeTrue();
    }

    // =============================================================================================
    // Área territorial
    // =============================================================================================

    [Fact]
    public void A_area_guarda_os_tres_decimais_que_o_ibge_publica()
    {
        // 1.521,202 km² é a área REAL da capital. Arredondar para inteiro perderia 202 mil m² por
        // município, e a soma do estado sairia errada em quilômetros quadrados.
        var area = AreaTerritorialDoMunicipio.Registrar(1, 2022, 1521.202m, 1, Agora);

        area.AreaKm2.Should().Be(1521.202m);
    }

    [Fact]
    public void Area_zero_ou_negativa_nao_existe()
    {
        // AO CONTRÁRIO DE UMA CONTAGEM, onde zero é medida legítima: um município sem área seria erro
        // de leitura, e é melhor recusar do que dividir por ele depois.
        var zero = () => AreaTerritorialDoMunicipio.Registrar(1, 2022, 0m, 1, Agora);
        zero.Should().Throw<RegraDeNegocioViolada>();

        var negativa = () => AreaTerritorialDoMunicipio.Registrar(1, 2022, -1m, 1, Agora);
        negativa.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Area_nao_disponivel_e_nula_e_nao_zero()
    {
        var area = AreaTerritorialDoMunicipio.Registrar(1, 2022, null, 1, Agora);
        area.AreaKm2.Should().BeNull();
    }

    // =============================================================================================
    // Usinas de etanol
    // =============================================================================================

    private static UsinaDeEtanol Usina(
        string cnpj = "12345678000199", int? anidro = 700, int? hidratado = 1300) =>
        UsinaDeEtanol.Registrar(cnpj, "USINA EXEMPLO S/A", 1, Julho, anidro, hidratado, 1, Agora);

    [Fact]
    public void A_usina_guarda_o_cnpj_do_estabelecimento_so_com_digitos()
    {
        // É o ESTABELECIMENTO, e não a empresa: um mesmo grupo tem várias usinas, cada uma no seu
        // município e com a sua capacidade.
        var usina = UsinaDeEtanol.Registrar(
            "12.345.678/0001-99", "USINA EXEMPLO S/A", 1, Julho, 700, 1300, 1, Agora);

        usina.Cnpj.Should().Be("12345678000199", "a pontuação sai, porque o CNPJ é identificador");
    }

    [Fact]
    public void Cnpj_fora_dos_catorze_digitos_e_recusado()
    {
        var curto = () => Usina("123");
        curto.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_capacidade_total_soma_as_duas_e_e_nula_so_quando_as_duas_faltam()
    {
        Usina(anidro: 700, hidratado: 1300).CapacidadeTotalM3Dia.Should().Be(2000);

        Usina(anidro: null, hidratado: 1200).CapacidadeTotalM3Dia.Should().Be(1200,
            "quem informou uma das duas tem capacidade conhecida — tratá-la como desconhecida a " +
            "esconderia do ranking");

        Usina(anidro: null, hidratado: null).CapacidadeTotalM3Dia.Should().BeNull();
    }

    [Fact]
    public void Usina_parada_tem_capacidade_zero_e_isso_nao_e_erro()
    {
        // MEDIDO em 07/2026: uma das 145 usinas paulistas estava com as duas zeradas — usina parada,
        // autorização viva. Recusá-la a apagaria do mapa.
        var parada = Usina(anidro: 0, hidratado: 0);

        parada.CapacidadeTotalM3Dia.Should().Be(0);
    }

    [Fact]
    public void Capacidade_negativa_e_recusada()
    {
        var registrar = () => Usina(anidro: -1);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Reapurar_a_usina_aceita_correcao_de_municipio_e_diz_se_mudou()
    {
        var usina = Usina();

        usina.Reapurar("USINA EXEMPLO S/A", 1, Julho, 700, 1300, 1, Agora).Should().BeFalse();

        // Uma usina não se muda de cidade, mas a ANP corrige grafia — e a correção precisa chegar.
        usina.Reapurar("USINA EXEMPLO S/A", 2, Julho, 700, 1300, 2, Agora).Should().BeTrue();
        usina.MunicipioId.Should().Be(2);
        usina.ImportadoPorId.Should().Be(2);
    }
}
