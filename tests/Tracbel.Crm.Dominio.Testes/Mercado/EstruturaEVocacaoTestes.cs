using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// A ÁREA DAS PROPRIEDADES, O TAMANHO MÉDIO E A VOCAÇÃO AGRÍCOLA (28/09/2026) — a utilização das terras do Censo
/// (SIDRA 6881) e os tercis da ADR, decididos pelo Ricardo.
/// </summary>
public class EstruturaEVocacaoTestes
{
    private static EstruturaDoMunicipio Estrutura(decimal? area, int? comArea, decimal? lavoura) =>
        new(2017, null, null, null, null, null, [], null, null, null, [], area, comArea, lavoura);

    [Fact]
    public void O_tamanho_medio_divide_pela_quantidade_COM_AREA_e_a_fatia_e_lavoura_sobre_o_total()
    {
        var e = Estrutura(45_210m, 240, 30_000m);

        e.TamanhoMedioHectares.Should().Be(188.4m, "45.210 ÷ 240");
        e.FatiaDeLavouraPercentual.Should().Be(66.4m, "30.000 ÷ 45.210");
    }

    [Fact]
    public void Sem_area_ou_sem_estabelecimentos_nao_ha_media_nem_fatia_e_sigilo_nao_vira_zero()
    {
        Estrutura(null, 240, 30_000m).TamanhoMedioHectares.Should().BeNull();
        Estrutura(45_210m, 0, 30_000m).TamanhoMedioHectares.Should().BeNull();
        Estrutura(45_210m, 240, null).FatiaDeLavouraPercentual.Should().BeNull();
    }

    [Fact]
    public void A_vocacao_corta_a_ADR_em_tres_tercos_pelos_tercis_da_fatia()
    {
        // Seis municípios, de 10% a 60%: tercis (PERCENTIL.INC) em 26,7% e 43,3%.
        var fatias = new Dictionary<int, decimal> { [1] = 10m, [2] = 20m, [3] = 30m, [4] = 40m, [5] = 50m, [6] = 60m };

        var vocacoes = VocacaoAgricola.PelosTercis(fatias);

        vocacoes.Values.Select(v => v.Classe).Should().Equal("Baixa", "Baixa", "Média", "Média", "Alta", "Alta");
        vocacoes[1].MediaAPartirDe.Should().Be(26.7m);
        vocacoes[1].AltaAPartirDe.Should().Be(43.3m);
        vocacoes[1].MunicipiosNaBase.Should().Be(6);
    }

    [Fact]
    public void Com_menos_de_tres_municipios_nao_ha_vocacao()
    {
        VocacaoAgricola.PelosTercis(new Dictionary<int, decimal> { [1] = 10m, [2] = 60m }).Should().BeEmpty();
    }

    [Fact]
    public void A_utilizacao_das_terras_recusa_area_negativa_e_guarda_o_sigilo_como_nulo()
    {
        var agora = DateTime.UtcNow;
        var linha = UtilizacaoDasTerrasNoMunicipio.Registrar(1, 2017, 110087, "Total", null, null, 1, agora);
        linha.AreaHectares.Should().BeNull();

        var negativa = () => UtilizacaoDasTerrasNoMunicipio.Registrar(1, 2017, 110087, "Total", 10, -1m, 1, agora);
        negativa.Should().Throw<RegraDeNegocioViolada>();

        linha.Reapurar("Total", 10, 100m, 1, agora).Should().BeTrue();
        linha.Reapurar("Total", 10, 100m, 1, agora).Should().BeFalse("a mesma leitura não muda nada");
    }
}
