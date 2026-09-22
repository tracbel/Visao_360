using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O DE-PARA ENTRE A FONTE E O MUNICÍPIO DO CATÁLOGO (issue 154): o par nasce completo e só muda pela mão de
/// quem decidiu. Um par pela metade — sem fonte, sem chave, sem texto — apontaria crédito, produção ou custo
/// para o município errado sem ninguém conseguir conferir depois.
/// </summary>
public sealed class CorrespondenciaDeMunicipioTestes
{
    private const long Carga = 100;
    private static readonly DateTime Agora = new(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

    private static CorrespondenciaDeMunicipio Par(string chave = "11790", string texto = "SAO JOSE DO RIO PRETO") =>
        CorrespondenciaDeMunicipio.Registrar(
            "BCB.SICOR_INVESTIMENTO", chave, texto, 3549, FormaDaCorrespondencia.NomeUnicoNaUf, Carga, Agora);

    [Fact]
    public void O_par_guarda_a_chave_o_texto_a_forma_e_quem_casou()
    {
        var par = Par();

        par.Fonte.Should().Be("BCB.SICOR_INVESTIMENTO");
        par.ChaveNaFonte.Should().Be("11790");
        par.TextoNaFonte.Should().Be("SAO JOSE DO RIO PRETO", "é o que alguém procura quando confere o par");
        par.MunicipioId.Should().Be(3549);
        par.Forma.Should().Be(FormaDaCorrespondencia.NomeUnicoNaUf);
        par.CasadaEm.Should().Be(Agora);
        par.CasadaPorId.Should().Be(Carga);
    }

    [Theory]
    [InlineData("", "11790", "CIDADE", 3549)]
    [InlineData("BCB.SICOR_INVESTIMENTO", "   ", "CIDADE", 3549)]
    [InlineData("BCB.SICOR_INVESTIMENTO", "11790", "", 3549)]
    [InlineData("BCB.SICOR_INVESTIMENTO", "11790", "CIDADE", 0)]
    public void O_par_pela_metade_e_recusado(string fonte, string chave, string texto, int municipioId)
    {
        var registrar = () => CorrespondenciaDeMunicipio.Registrar(
            fonte, chave, texto, municipioId, FormaDaCorrespondencia.NomeUnicoNaUf, Carga, Agora);

        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_forma_desconhecida_e_recusada()
    {
        var registrar = () => CorrespondenciaDeMunicipio.Registrar(
            "ANP.USINA_DE_ETANOL", "PIRACICABA", "Piracicaba", 3538, (FormaDaCorrespondencia)9, Carga, Agora);

        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void O_texto_longo_demais_da_fonte_e_encurtado_e_nao_derruba_a_carga()
    {
        var par = CorrespondenciaDeMunicipio.Registrar(
            "CONAB.CUSTO_DE_PRODUCAO", "LOCAL", new string('A', 200), 3549,
            FormaDaCorrespondencia.NomeUnicoNaUf, Carga, Agora);

        par.TextoNaFonte.Should().HaveLength(CorrespondenciaDeMunicipio.TamanhoDoTexto);
    }

    [Fact]
    public void Reapontar_para_outro_municipio_muda_o_par_e_diz_que_mudou()
    {
        var par = Par();

        par.Reapontar(3550, "SÃO JOSÉ DO RIO PRETO", FormaDaCorrespondencia.Manual, 42, Agora.AddDays(1))
            .Should().BeTrue();

        par.MunicipioId.Should().Be(3550);
        par.TextoNaFonte.Should().Be("SÃO JOSÉ DO RIO PRETO");
        par.Forma.Should().Be(FormaDaCorrespondencia.Manual);
        par.CasadaPorId.Should().Be(42, "quem reaponta é quem decidiu, não a carga");
        par.ChaveNaFonte.Should().Be("11790", "a chave é a identidade do par — mudar a chave seria outro par");
    }

    [Fact]
    public void Reapontar_para_o_mesmo_diz_que_nao_mudou_e_nao_enche_a_trilha()
    {
        var par = Par();

        par.Reapontar(3549, "SAO JOSE DO RIO PRETO", FormaDaCorrespondencia.NomeUnicoNaUf, Carga, Agora.AddDays(1))
            .Should().BeFalse();
    }

    [Fact]
    public void Reapontar_para_municipio_que_nao_existe_e_recusado()
    {
        var par = Par();

        var reapontar = () => par.Reapontar(0, "CIDADE", FormaDaCorrespondencia.Manual, 42, Agora);

        reapontar.Should().Throw<RegraDeNegocioViolada>();
    }
}
