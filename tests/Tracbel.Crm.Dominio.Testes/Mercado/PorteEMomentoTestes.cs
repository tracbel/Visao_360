using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// PORTE E MOMENTO — dois números, nunca um (documento 50, §4.2; fase T3).
///
/// <para>O que está sob prova aqui é a decisão que separa "não sei" de "é pequeno": sem banda
/// registrada (issue 166) o porte NÃO tem nome, e a tela mostra o número. O momento, esse já tem
/// faixas decididas e pode ser nomeado hoje.</para>
/// </summary>
public class PorteEMomentoTestes
{
    private static readonly DateTime Agora = new(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);

    private static ParametroDoPotencial Parametros(decimal? porteMedio = null, decimal? porteGrande = null) =>
        ParametroDoPotencial.Informar(
            new ParametroDoPotencial.Valores(
                MesesDaJanela: 12,
                PesoDosContratosNoCredito: 0.70m,
                LimiteDeRetracao: 1.00m,
                LimiteDeAquecimento: 1.20m,
                LimiteDeSuperaquecimento: 1.40m,
                NomeDaFaixaIntermediaria: null,
                LimiteDaPercepcao: 5m,
                PesoDoIndicadorDePreco: 0.40m,
                PesoDoIndicadorDeCredito: 0.50m,
                PesoDoIndicadorComercial: 1.00m,
                FatorMinimo: 0.40m,
                FatorMaximo: 1.50m,
                PorteMedioAPartirDe: porteMedio,
                PorteGrandeAPartirDe: porteGrande),
            ParametroComVigencia.HojeNoBrasil(Agora),
            "bandas de porte para o teste",
            100,
            Agora);

    // ---------------------------------------------------------------------------------------------
    // O porte não tem nome enquanto ninguém decidir os cortes (issue 166)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Sem_bandas_registradas_o_porte_nao_tem_nome_e_nulo_nao_e_pequeno()
    {
        var semBandas = Parametros();

        semBandas.PorteDe(3_457m).Should().BeNull(
            "nomear exige um corte, e um corte sem dono é parâmetro inventado — nulo aqui não quer dizer pequeno");
        semBandas.PorteDe(1m).Should().BeNull();
        semBandas.PorteDe(999_999m).Should().BeNull();
    }

    [Theory]
    [InlineData(200, "Mercado pequeno")]
    [InlineData(500, "Mercado médio")]
    [InlineData(1_999, "Mercado médio")]
    [InlineData(2_000, "Mercado grande")]
    [InlineData(3_457, "Mercado grande")]
    public void Com_bandas_registradas_o_porte_ganha_nome(int demanda, string esperado) =>
        Parametros(porteMedio: 500m, porteGrande: 2_000m).PorteDe(demanda).Should().Be(esperado);

    [Fact]
    public void A_banda_e_inclusiva_no_piso_da_classe()
    {
        var p = Parametros(porteMedio: 500m, porteGrande: 2_000m);

        p.PorteDe(499.9m).Should().Be("Mercado pequeno");
        p.PorteDe(500m).Should().Be("Mercado médio", "o corte marca o começo da classe, e não o fim da anterior");
    }

    [Fact]
    public void Sem_demanda_nao_ha_porte_para_nomear() =>
        Parametros(porteMedio: 500m, porteGrande: 2_000m).PorteDe(null).Should().BeNull();

    // ---------------------------------------------------------------------------------------------
    // As duas bandas andam juntas
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Uma_banda_sozinha_e_recusada()
    {
        var soMedia = () => Parametros(porteMedio: 500m);
        soMedia.Should().Throw<RegraDeNegocioViolada>()
            .WithMessage("*andam juntas*", "com só o médio não se sabe onde começa o grande");

        var soGrande = () => Parametros(porteGrande: 2_000m);
        soGrande.Should().Throw<RegraDeNegocioViolada>().WithMessage("*andam juntas*");
    }

    [Fact]
    public void A_banda_de_medio_precisa_ficar_abaixo_da_de_grande()
    {
        var invertida = () => Parametros(porteMedio: 2_000m, porteGrande: 500m);
        invertida.Should().Throw<RegraDeNegocioViolada>().WithMessage("*menor que a de mercado grande*");
    }

    // ---------------------------------------------------------------------------------------------
    // Os cortes pelos tercis dos municípios da ADR (decisão do Ricardo de 27/09/2026)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Os_tercis_sao_os_da_planilha_e_deixam_um_terco_dos_municipios_em_cada_porte()
    {
        // PERCENTIL.INC({1..9}; 1/3) = 3,667 e PERCENTIL.INC({1..9}; 2/3) = 6,333 — arredondados a uma casa.
        var demandas = new decimal[] { 9, 1, 8, 2, 7, 3, 6, 4, 5 };

        var bandas = ParametroDoPotencial.BandasPelosTercis(demandas);

        bandas.Should().Be((3.7m, 6.3m), "a ordem de entrada não importa, e a interpolação é a do Excel");

        var p = Parametros(bandas!.Value.MedioAPartirDe, bandas.Value.GrandeAPartirDe);
        demandas.GroupBy(d => p.PorteDe(d)!).ToDictionary(g => g.Key, g => g.Count())
            .Should().BeEquivalentTo(new Dictionary<string, int>
            {
                ["Mercado pequeno"] = 3, ["Mercado médio"] = 3, ["Mercado grande"] = 3
            });
    }

    [Fact]
    public void Sem_como_cortar_nao_ha_banda()
    {
        ParametroDoPotencial.BandasPelosTercis([10m, 20m]).Should().BeNull("o tercil pede pelo menos três municípios");
        ParametroDoPotencial.BandasPelosTercis([0m, 0m, 0m, 0m, 5m, 9m])
            .Should().BeNull("com um terço sem demanda o médio começaria em zero, e zero não é banda");
        ParametroDoPotencial.BandasPelosTercis([7m, 7m, 7m, 7m]).Should().BeNull("todos iguais: o corte não separa ninguém");
    }

    [Fact]
    public void O_porte_de_um_recorte_e_o_do_municipio_tipico_e_nao_o_da_soma()
    {
        var p = Parametros(porteMedio: 10m, porteGrande: 30m);

        p.PorteDosMunicipios([5m, 20m, 35m]).Should().Be("Mercado médio", "a soma (60) passaria do corte de grande; a média (20) não");
        p.PorteDosMunicipios([40m]).Should().Be("Mercado grande", "com um município só, o recorte é ele");
        p.PorteDosMunicipios([]).Should().BeNull("sem município com demanda não há porte para nomear");
        Parametros().PorteDosMunicipios([5m, 20m]).Should().BeNull("sem bandas registradas não há nome");
    }

    // ---------------------------------------------------------------------------------------------
    // A faixa do momento usa a mesma régua dos índices
    // ---------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(0.88, "Retraído")]
    [InlineData(0.99, "Retraído")]
    [InlineData(1.00, "Normal")]
    [InlineData(1.19, "Normal")]
    [InlineData(1.20, "Aquecido")]
    [InlineData(1.39, "Aquecido")]
    [InlineData(1.40, "Superaquecido")]
    public void A_faixa_do_momento_segue_as_fronteiras_registradas(double fator, string esperado) =>
        LeituraDoMercado.FaixaDoFator((decimal)fator, Parametros()).Should().Be(esperado);

    [Fact]
    public void Sem_fator_ou_sem_parametro_nao_ha_faixa()
    {
        LeituraDoMercado.FaixaDoFator(null, Parametros()).Should().BeNull();
        LeituraDoMercado.FaixaDoFator(1.10m, null).Should().BeNull();
    }

    [Fact]
    public void O_nome_da_faixa_intermediaria_e_respeitado_quando_alguem_o_registra()
    {
        var comNome = ParametroDoPotencial.Informar(
            new ParametroDoPotencial.Valores(
                MesesDaJanela: 12, PesoDosContratosNoCredito: 0.70m,
                LimiteDeRetracao: 1.00m, LimiteDeAquecimento: 1.20m, LimiteDeSuperaquecimento: 1.40m,
                NomeDaFaixaIntermediaria: "Morno", LimiteDaPercepcao: 5m,
                PesoDoIndicadorDePreco: 0.40m, PesoDoIndicadorDeCredito: 0.50m, PesoDoIndicadorComercial: 1.00m,
                FatorMinimo: 0.40m, FatorMaximo: 1.50m),
            ParametroComVigencia.HojeNoBrasil(Agora), "faixa do meio nomeada", 100, Agora);

        LeituraDoMercado.FaixaDoFator(1.10m, comNome).Should().Be("Morno");
    }

    // ---------------------------------------------------------------------------------------------
    // A frase junta os dois, e não inventa a metade que falta
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Com_porte_e_momento_a_frase_junta_os_dois() =>
        LeituraDoMercado.Frase("Mercado grande", "Retraído").Should().Be("Mercado grande, agora retraído.");

    [Fact]
    public void Sem_porte_a_frase_fala_so_do_momento() =>
        LeituraDoMercado.Frase(null, "Retraído").Should().Be("Mercado retraído.");

    [Fact]
    public void Sem_momento_a_frase_fala_so_do_porte() =>
        LeituraDoMercado.Frase("Mercado grande", null).Should().Be("Mercado grande.");

    [Fact]
    public void Sem_os_dois_a_frase_e_vazia_e_a_tela_mostra_os_numeros() =>
        LeituraDoMercado.Frase(null, null).Should().BeEmpty();
}
