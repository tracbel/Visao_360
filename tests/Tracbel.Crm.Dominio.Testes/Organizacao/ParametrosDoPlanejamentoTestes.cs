using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// Os parâmetros do planejamento comercial (issue 256): a sazonalidade reparte a demanda do ano sem aumentá-la, os pesos
/// do IOC são normalizados pela soma e o share-alvo de uma categoria é maior que zero.
/// </summary>
public sealed class ParametrosDoPlanejamentoTestes
{
    /// <summary>27/09/2026, 15h em São Paulo.</summary>
    private static readonly DateTime Agora = new(2026, 9, 27, 18, 0, 0, DateTimeKind.Utc);

    private static readonly DateOnly Hoje = new(2026, 9, 27);

    /// <summary>A sazonalidade do protótipo da pasta 360 — soma 100,00.</summary>
    private static readonly decimal[] DoPrototipo = [6.52m, 6.89m, 8.56m, 8.59m, 8.87m, 8.58m, 7.84m, 8.75m, 9.15m, 10.51m, 7.44m, 8.30m];

    private static readonly PesosDoIoc PesosDoPrototipo = new(25, 20, 15, 15, 10, 5, 10);

    private static ParametroDoPlanejamento Informar(IReadOnlyList<decimal>? meses = null, PesosDoIoc? pesos = null) =>
        ParametroDoPlanejamento.Informar(
            new ParametroDoPlanejamento.Valores(meses ?? DoPrototipo, pesos ?? PesosDoPrototipo), Hoje, "protótipo", 100, Agora);

    [Fact]
    public void A_sazonalidade_do_prototipo_entra_inteira_e_na_ordem()
    {
        var parametro = Informar();

        parametro.Sazonalidade.Should().Equal(DoPrototipo);
        parametro.SazonalidadeOutubro.Should().Be(10.51m, "outubro é o pico do protótipo");
        parametro.Pesos.Should().Be(PesosDoPrototipo);
        parametro.InformadoPorId.Should().Be(100);
    }

    [Fact]
    public void A_fracao_do_mes_fecha_exatamente_no_ano_mesmo_com_a_folga_do_arredondamento()
    {
        // 100,04: dentro da folga. A previsão mensal divide pela soma, e os doze meses somam a demanda do ano inteira.
        var meses = DoPrototipo.ToArray();
        meses[0] += 0.04m;

        var parametro = Informar(meses);

        Enumerable.Range(1, 12).Sum(parametro.FracaoDoMes).Should().BeApproximately(1m, 0.0000001m);
        parametro.FracaoDoMes(10).Should().BeApproximately(10.51m / 100.04m, 0.0000001m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Mes_fora_do_ano_e_erro_de_quem_chama(int mes)
    {
        var fracao = () => Informar().FracaoDoMes(mes);
        fracao.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1.00)]
    [InlineData(-1.00)]
    [InlineData(0.06)]
    public void Sazonalidade_que_nao_soma_cem_e_recusada(double desvio)
    {
        var meses = DoPrototipo.ToArray();
        meses[5] += (decimal)desvio;

        var informar = () => Informar(meses);
        informar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*somam 100%*não a aumenta*");
    }

    [Fact]
    public void Sazonalidade_sem_os_doze_meses_e_recusada()
    {
        var informar = () => Informar(DoPrototipo[..11]);
        informar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*doze meses*");
    }

    [Fact]
    public void Mes_negativo_e_recusado_mesmo_quando_a_soma_fecha()
    {
        var meses = DoPrototipo.ToArray();
        meses[0] = -1m;
        meses[1] += 7.52m;

        var informar = () => Informar(meses);
        informar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*de 0% a 100%*");
    }

    [Fact]
    public void Peso_zero_tira_o_componente_mas_todos_zero_nao_medem_nada()
    {
        Informar(pesos: PesosDoPrototipo with { Realizacao = 0 }).PesoDaRealizacao.Should().Be(0);

        var nenhum = () => Informar(pesos: new PesosDoIoc(0, 0, 0, 0, 0, 0, 0));
        nenhum.Should().Throw<RegraDeNegocioViolada>().WithMessage("*Ao menos um peso*");
    }

    [Fact]
    public void Peso_negativo_e_recusado()
    {
        var informar = () => Informar(pesos: PesosDoPrototipo with { Credito = -5 });
        informar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*de 0 a 100*");
    }

    [Fact]
    public void O_passado_nao_se_reescreve()
    {
        var ontem = () => ParametroDoPlanejamento.Informar(
            new ParametroDoPlanejamento.Valores(DoPrototipo, PesosDoPrototipo), Hoje.AddDays(-1), "x", 100, Agora);

        ontem.Should().Throw<RegraDeNegocioViolada>().WithMessage("*já passou*");
    }

    [Fact]
    public void O_share_alvo_guarda_a_categoria_e_o_percentual()
    {
        var share = ShareAlvoDaCategoria.Informar(1, 31m, Hoje, "protótipo", 100, Agora);

        share.CategoriaDeMaquinaId.Should().Be(1);
        share.Percentual.Should().Be(31m);
        share.VigenteDesde.Should().Be(Hoje);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(100.01)]
    public void Share_alvo_fora_de_zero_a_cem_e_recusado(double percentual)
    {
        var informar = () => ShareAlvoDaCategoria.Informar(1, (decimal)percentual, Hoje, "x", 100, Agora);
        informar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*maior que 0%*");
    }

    [Fact]
    public void Share_alvo_sem_categoria_e_recusado()
    {
        var informar = () => ShareAlvoDaCategoria.Informar(0, 31m, Hoje, "x", 100, Agora);
        informar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*categoria de máquina*");
    }

    [Fact]
    public void Na_data_vale_a_vigencia_mais_recente_que_ja_comecou()
    {
        var hoje = ShareAlvoDaCategoria.Informar(1, 31m, Hoje, "x", 100, Agora);
        var mesQueVem = ShareAlvoDaCategoria.Informar(1, 35m, Hoje.AddMonths(1), "x", 100, Agora);

        ParametroComVigencia.VigenteEm([hoje, mesQueVem], Hoje.AddDays(10))!.Percentual.Should().Be(31m);
        ParametroComVigencia.VigenteEm([hoje, mesQueVem], Hoje.AddMonths(2))!.Percentual.Should().Be(35m);
    }
}
