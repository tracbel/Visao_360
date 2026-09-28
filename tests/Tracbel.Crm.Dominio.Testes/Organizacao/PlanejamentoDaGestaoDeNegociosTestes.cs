using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O PLANEJAMENTO DA API GESTÃO DE NEGÓCIOS (28/09/2026): o gestor de cada consultor, a previsão da gerência e a cota de
/// consórcio vendida. Nomes inventados.
/// </summary>
public sealed class PlanejamentoDaGestaoDeNegociosTestes
{
    private static readonly LeituraDoPlanejamento Leitura =
        new(new DateTime(2026, 9, 28, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 28, 4, 30, 0, DateTimeKind.Utc));

    private static ForecastDaGerencia Previsao(int? forecast = 5, int? bestGuess = 6, DateOnly? mes = null, string linha = "TRATOR MÉDIO") =>
        ForecastDaGerencia.Registrar(1, 10, mes ?? new DateOnly(2026, 9, 1), "GESTOR.NORTE", linha, forecast, bestGuess, "h1", Leitura, 100);

    private static DadosDaCotaNaOrigem Cota(int empresa = 2, string hash = "h1", long? conta = 101) =>
        new(empresa, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 10), "Lance", null, "fulano.de.tal", conta, "GESTOR.NORTE",
            "TRATOR 6M", 250000m, 3100.50m, hash);

    // =============================================================================================
    // O gestor de cada consultor
    // =============================================================================================

    [Fact]
    public void O_gestor_do_consultor_e_o_da_vigencia_mais_recente_e_sem_vigencia_o_de_id_maior()
    {
        var gestores = GestorDoConsultor.GestorPorConsultor(
        [
            ("FULANO.DE.TAL", "GESTOR.NORTE", new DateOnly(2025, 11, 1), 1),
            ("fulano.de.tal", "GESTOR.SUL", new DateOnly(2026, 6, 1), 2),
            ("BELTRANA.SILVA", "GESTOR.NORTE", null, 3),
            ("BELTRANA.SILVA", "GESTOR.LESTE", null, 4),
            ("  ", "GESTOR.OESTE", null, 5)
        ]);

        gestores.Should().HaveCount(2, "consultor em branco não entra");
        gestores[MetaDeVenda.ChaveDaPessoa("FULANO.DE.TAL")].Should().Be("GESTOR.SUL", "a chave é a mesma da meta: caixa não importa");
        gestores[MetaDeVenda.ChaveDaPessoa("BELTRANA.SILVA")].Should().Be("GESTOR.LESTE");
    }

    [Fact]
    public void O_de_para_guarda_o_consultor_em_maiusculas_e_so_muda_quando_o_resumo_muda()
    {
        var linha = GestorDoConsultor.Registrar(1, 7, " fulano.de.tal ", "GESTOR.NORTE", 5, null, "h1", Leitura, 100);

        (linha.ConsultorNaOrigem, linha.ImportadoEm, linha.ImportadoPorId).Should().Be(("FULANO.DE.TAL", Leitura.LidaEm, 100L));
        linha.AtualizarDaOrigem("fulano.de.tal", "GESTOR.SUL", 5, null, "h1", Leitura).Should().BeFalse("mesmo resumo, nada muda");
        linha.GestorNaOrigem.Should().Be("GESTOR.NORTE");
        linha.AtualizarDaOrigem("fulano.de.tal", "GESTOR.SUL", 5, null, "h2", Leitura).Should().BeTrue();
        linha.GestorNaOrigem.Should().Be("GESTOR.SUL");
    }

    [Fact]
    public void O_de_para_sai_e_volta_sem_ser_apagado()
    {
        var linha = GestorDoConsultor.Registrar(1, 7, "A.B", "C.D", null, null, "h1", Leitura, 100);
        var sumiu = Leitura.LidaEm.AddDays(1);

        linha.Excluir(sumiu);
        linha.Excluir(sumiu.AddDays(1));
        linha.ExcluidoEm.Should().Be(sumiu, "a primeira saída é a que fica");
        linha.Reativar().Should().BeTrue();
        linha.ExcluidoEm.Should().BeNull();
        linha.Reativar().Should().BeFalse();
    }

    [Theory]
    [InlineData(0, "A.B", "C.D")]
    [InlineData(1, " ", "C.D")]
    [InlineData(1, "A.B", "")]
    public void De_para_sem_id_consultor_ou_gestor_e_recusado(int id, string consultor, string gestor)
    {
        var registrar = () => GestorDoConsultor.Registrar(1, id, consultor, gestor, null, null, "h1", Leitura, 100);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    // =============================================================================================
    // O forecast
    // =============================================================================================

    [Fact]
    public void O_forecast_codifica_a_linha_como_a_meta_e_guarda_o_nulo_como_nulo()
    {
        var previsao = Previsao(forecast: null, bestGuess: 2);

        previsao.CodigoDaLinha.Should().Be("TRATOR_MEDIO", "o mesmo código da meta e do ART, para o PO e o realizado casarem");
        (previsao.Forecast, previsao.BestGuess).Should().Be(((int?)null, (int?)2), "não informado não é zero");
        previsao.GeradaNaOrigemEm.Should().Be(Leitura.GeradaNaOrigemEm);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, ForecastDaGerencia.QuantidadeMaxima + 1)]
    public void Quantidade_fora_de_0_ao_maximo_e_recusada(int forecast, int bestGuess)
    {
        var registrar = () => Previsao(forecast, bestGuess);
        registrar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*0 a 10000*");
    }

    [Fact]
    public void A_competencia_do_forecast_e_o_dia_1()
    {
        var registrar = () => Previsao(mes: new DateOnly(2026, 9, 15));
        registrar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*2026-09-15*");
    }

    [Fact]
    public void O_forecast_so_muda_quando_o_resumo_muda()
    {
        var previsao = Previsao();
        var depois = Leitura with { LidaEm = Leitura.LidaEm.AddDays(1) };

        previsao.AtualizarDaOrigem(new DateOnly(2026, 9, 1), "GESTOR.NORTE", "TRATOR MÉDIO", 8, 6, "h1", depois).Should().BeFalse();
        previsao.Forecast.Should().Be(5);
        previsao.AtualizarDaOrigem(new DateOnly(2026, 9, 1), "GESTOR.NORTE", "TRATOR MÉDIO", 8, 6, "h2", depois).Should().BeTrue();
        (previsao.Forecast, previsao.LidaEm, previsao.ImportadoEm).Should().Be(((int?)8, depois.LidaEm, Leitura.LidaEm));
    }

    // =============================================================================================
    // A cota vendida
    // =============================================================================================

    [Fact]
    public void A_cota_muda_quando_o_resumo_a_filial_ou_a_conta_mudam()
    {
        var cota = CotaDeConsorcioVendida.Registrar(1, " 1000 ", "1", Cota(), Leitura, 100);

        (cota.Grupo, cota.ConsultorNaOrigem, cota.EmpresaId).Should().Be(("1000", "FULANO.DE.TAL", 2));
        CotaDeConsorcioVendida.Chave(" 1000", "1 ").Should().Be("1000/1");
        cota.AtualizarDaOrigem(Cota(), Leitura, 100).Should().BeFalse();
        cota.AtualizarDaOrigem(Cota(conta: null), Leitura, 100).Should().BeTrue("a conta do consultor foi desativada");
        cota.AtualizarDaOrigem(Cota(empresa: 1, conta: null), Leitura, 100).Should().BeTrue("a cota mudou de filial");
        cota.EmpresaId.Should().Be(1);
    }

    [Fact]
    public void Cota_com_valor_negativo_ou_sem_contemplacao_e_recusada()
    {
        var negativa = () => CotaDeConsorcioVendida.Registrar(1, "1000", "1", Cota() with { ValorDoBem = -1m }, Leitura, 100);
        var semContemplacao = () => CotaDeConsorcioVendida.Registrar(1, "1000", "1", Cota() with { Contemplacao = " " }, Leitura, 100);

        negativa.Should().Throw<RegraDeNegocioViolada>();
        semContemplacao.Should().Throw<RegraDeNegocioViolada>();
    }
}
