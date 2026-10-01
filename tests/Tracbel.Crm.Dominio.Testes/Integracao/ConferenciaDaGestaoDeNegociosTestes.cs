using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Integracao;

/// <summary>A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS (28/09/2026): a rotina 13 no catálogo e a linha apurada.</summary>
public sealed class ConferenciaDaGestaoDeNegociosTestes
{
    private static readonly DateTime Agora = new(2026, 9, 28, 10, 15, 0, DateTimeKind.Utc);

    [Fact]
    public void A_rotina_da_conferencia_e_a_13a_diaria_as_07_15_nasce_desligada_e_exige_a_gn()
    {
        var conferencia = RotinasDoSistema.Todas[12];

        conferencia.Codigo.Should().Be(RotinasDoSistema.ConferenciaGestaoDeNegocios, "rotina nova entra no fim: a posição é o identificador");
        conferencia.Modos.Should().Equal("--somente-conferencia-gn");
        conferencia.LigadaPorPadrao.Should().BeFalse();
        conferencia.ConexaoExigida.Should().Be(ConexoesDoSistema.GestaoDeNegocios);
        conferencia.AgendaPadrao.Should().Be(AgendaDaRotina.DiariaAs(new TimeOnly(7, 15)));
        RotinasDoSistema.Obter(RotinasDoSistema.MetasGestaoDeNegocios)!.AgendaPadrao.Should().Be(AgendaDaRotina.ACada(60),
            "compara com a meta que a rotina 9 trouxe há menos de uma hora");
    }

    [Fact]
    public void A_linha_apurada_guarda_os_dois_lados_e_a_diferenca_e_o_crm_menos_a_gn()
    {
        var linha = ConferenciaDaGestaoDeNegocios.Apurar(1, 2, ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, new DateOnly(2026, 8, 1), 12, 10, Agora, 100);

        (linha.NaGestao, linha.NoCrm, linha.Diferenca).Should().Be((12, 10, -2));
    }

    [Fact]
    public void Indicador_desconhecido_mes_fora_do_dia_1_ou_numero_negativo_sao_recusados()
    {
        var indicador = () => ConferenciaDaGestaoDeNegocios.Apurar(1, 2, "FATURAMENTO", new DateOnly(2026, 8, 1), 1, 1, Agora, 100);
        var dia = () => ConferenciaDaGestaoDeNegocios.Apurar(1, 2, ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, new DateOnly(2026, 8, 2), 1, 1, Agora, 100);
        var negativo = () => ConferenciaDaGestaoDeNegocios.Apurar(1, 2, ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, new DateOnly(2026, 8, 1), -1, 1, Agora, 100);

        indicador.Should().Throw<RegraDeNegocioViolada>();
        dia.Should().Throw<RegraDeNegocioViolada>();
        negativo.Should().Throw<RegraDeNegocioViolada>();
    }
}
