using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoPlanejamento;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O PLANEJAMENTO DA API GESTÃO DE NEGÓCIOS (28/09/2026) — o de-para de consultores, o forecast da gerência e as cotas de
/// consórcio vendidas. O que estes testes prendem:
/// <list type="bullet">
/// <item>a primeira rodada cria as três tabelas, casa a filial da cota pelo NOME (pelo de-para das lojas) e a conta do
/// consultor, deixa a Digital de fora e contada, e carimba os três frescores;</item>
/// <item>a segunda leitura igual grava zero; a revisão do forecast deixa a trilha como integração;</item>
/// <item>a cota que some DENTRO dos meses da leitura é excluída, e a de fora (o ano fiscal que virou) fica; a que vai para
/// a Digital sai da filial;</item>
/// <item>as travas: mais de 5% de formato ilegível numa rota, ou mais de 20% de remoção numa tabela, abortam tudo;</item>
/// <item>a simulação não grava nada, e o relatório não tem nome de ninguém.</item>
/// </list>
/// <para>SQLite em memória com o modelo de verdade, como as outras cargas.</para>
/// </summary>
public sealed class CargaDoPlanejamentoDaGestaoDeNegociosTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 28, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDasMetas _semente;

    public CargaDoPlanejamentoDaGestaoDeNegociosTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    private async Task<RelatorioDoPlanejamento> Sincronizar(
        LeituraDoPlanejamentoNaOrigem? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null)
    {
        var resultado = await Tentar(leitura, simular, aceitarRemocao, quando);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private Task<Resultado<RelatorioDoPlanejamento>> Tentar(
        LeituraDoPlanejamentoNaOrigem? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null) =>
        Sincronia(DaCarga, leitura ?? Leitura(), quando ?? Agora, _semente.Operador).ExecutarAsync(simular, aceitarRemocao, CancellationToken.None);

    private static int Na(RelatorioDoPlanejamento relatorio, string etapa, string rotulo) =>
        relatorio.Contagens.Where(c => c.Etapa == etapa && c.Rotulo == rotulo).Sum(c => c.Valor);

    [Fact]
    public async Task A_primeira_rodada_cria_as_tres_tabelas_e_casa_a_filial_pelo_nome_e_a_conta_do_consultor()
    {
        var relatorio = await Sincronizar();

        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoTime, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeNovas).Should().Be(3);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoTime, "gestores distintos").Should().Be(2);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoTime, "consultores em mais de uma linha (vale a vigência mais recente)").Should().Be(1);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoForecast, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeNovas).Should().Be(2);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoForecast, "  linhas sem forecast informado (nulo, e não zero)").Should().Be(1);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoConsorcio, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDasCotasLidas).Should().Be(3);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoConsorcio, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDasCotasSemFilial).Should().Be(1);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoConsorcio, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeNovas).Should().Be(2);
        relatorio.Contagens.Should().NotContain(c =>
                c.Rotulo.Contains(ConsultorComConta, StringComparison.OrdinalIgnoreCase) || c.Rotulo.Contains(GestorNorte, StringComparison.OrdinalIgnoreCase),
            "o relatório tem só contagens, nunca nome de consultor ou de gestor");

        await using var db = Sistema();
        var time = await db.GestoresDosConsultores.AsNoTracking().OrderBy(g => g.IdNaOrigem).ToListAsync();
        time.Select(g => g.GestorNaOrigem).Should().Equal(GestorNorte, GestorNorte, GestorSul);
        time[0].FilialNumero.Should().Be(5);
        time[2].VigenteDesde.Should().Be(new DateOnly(2026, 6, 1));
        time.Should().OnlyContain(g => g.ImportadoPorId == _semente.Operador && g.ImportadoEm == Agora && g.ExcluidoEm == null);
        GestorDoConsultor.GestorPorConsultor(time.Select(g => (g.ConsultorNaOrigem, g.GestorNaOrigem, g.VigenteDesde, g.IdNaOrigem)))
            [MetaDeVenda.ChaveDaPessoa(ConsultorSemConta)].Should().Be(GestorSul, "vale a linha de vigência mais recente");

        var forecast = await db.ForecastsDaGerencia.AsNoTracking().OrderBy(f => f.IdNaOrigem).ToListAsync();
        (forecast[0].Competencia, forecast[0].CodigoDaLinha, forecast[0].Forecast, forecast[0].BestGuess)
            .Should().Be((new DateOnly(2026, 9, 1), "TRATOR_MEDIO", (int?)5, (int?)6));
        (forecast[1].Forecast, forecast[1].BestGuess).Should().Be(((int?)null, (int?)2), "o gestor não informou o forecast: nulo, e não zero");
        forecast[0].GeradaNaOrigemEm.Should().Be(new DateTime(2026, 9, 28, 4, 30, 0, DateTimeKind.Utc));

        var cotas = await db.CotasDeConsorcioVendidas.AsNoTracking().OrderBy(c => c.Cota).ToListAsync();
        cotas.Select(c => CotaDeConsorcioVendida.Chave(c.Grupo, c.Cota)).Should().Equal("1000/1", "1000/2");
        (cotas[0].EmpresaId, cotas[0].Competencia, cotas[0].ConsultorUsuarioId).Should().Be((_semente.Ituverava, new DateOnly(2026, 9, 1), (long?)_semente.Consultor),
            "ITUVERAVA é 010105 pelo de-para das lojas, e o login da conta é o consultor em minúsculas");
        (cotas[1].EmpresaId, cotas[1].Competencia, cotas[1].ConsultorUsuarioId).Should().Be((_semente.RibeiraoPreto, new DateOnly(2026, 8, 1), (long?)null));
        (cotas[0].ValorDoBem, cotas[0].ValorDaParcela, cotas[0].AlocadaEm).Should().Be((250000.00m, 3100.50m, new DateOnly(2026, 9, 10)));

        var pontos = await db.PontosDeSincronismo.AsNoTracking().Select(p => p.Fluxo).ToListAsync();
        pontos.Should().BeEquivalentTo(GestorDoConsultor.FluxoDaCarga, ForecastDaGerencia.FluxoDaCarga, CotaDeConsorcioVendida.FluxoDaCarga);
    }

    [Fact]
    public async Task A_segunda_leitura_igual_grava_zero()
    {
        await Sincronizar();

        var segunda = await Sincronizar(quando: Agora.AddDays(1));

        segunda.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeNovas).Should().Be(0);
        segunda.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(0);
        segunda.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(0);
        segunda.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeIguais).Should().Be(7, "3 do de-para, 2 previsões e 2 cotas");

        await using var db = Sistema();
        (await db.ForecastsDaGerencia.AsNoTracking().Select(f => f.LidaEm).Distinct().ToListAsync()).Should().Equal(Agora);
        (await db.AlteracoesDeCampo.CountAsync()).Should().Be(0, "a inclusão pela integração não entra na trilha, e nada mudou");
    }

    [Fact]
    public async Task A_revisao_do_forecast_muda_a_linha_e_deixa_a_trilha_como_integracao()
    {
        await Sincronizar();

        var revisado = Forecast();
        revisado[0] = Previsao(1, forecast: "8");
        (await Sincronizar(Leitura(forecast: revisado), quando: Agora.AddDays(1)))
            .Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(1);

        await using var db = Sistema();
        var previsao = await db.ForecastsDaGerencia.AsNoTracking().SingleAsync(f => f.IdNaOrigem == 1);
        (previsao.Forecast, previsao.LidaEm, previsao.ImportadoEm).Should().Be(((int?)8, Agora.AddDays(1), Agora),
            "a importação fica com a data da primeira leitura; a leitura, com a da revisão");

        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(ForecastDaGerencia)).ToListAsync();
        trilha.Should().ContainSingle(a => a.Campo == nameof(ForecastDaGerencia.Forecast) && a.ValorAnterior == "5" && a.ValorNovo == "8");
        trilha.Should().OnlyContain(a => a.Origem == OrigemDaOperacao.Integracao && a.SistemaId != null);
    }

    [Fact]
    public async Task A_previsao_que_some_e_excluida_e_a_que_volta_e_reativada_na_mesma_linha()
    {
        await Sincronizar();
        long idDaLinha;
        await using (var db = Sistema()) idDaLinha = (await db.ForecastsDaGerencia.AsNoTracking().SingleAsync(f => f.IdNaOrigem == 2)).Id;

        (await Sincronizar(Leitura(forecast: [Previsao(1)]), quando: Agora.AddDays(1)))
            .Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1);
        await using (var db = Sistema())
            (await db.ForecastsDaGerencia.AsNoTracking().SingleAsync(f => f.IdNaOrigem == 2)).ExcluidoEm.Should().Be(Agora.AddDays(1), "excluída, nunca apagada");

        (await Sincronizar(quando: Agora.AddDays(2))).Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeReativadas).Should().Be(1);
        await using (var db = Sistema())
        {
            var voltou = await db.ForecastsDaGerencia.AsNoTracking().SingleAsync(f => f.IdNaOrigem == 2);
            (voltou.ExcluidoEm, voltou.Id).Should().Be(((DateTime?)null, idDaLinha), "a volta é a mesma linha, com a trilha junto");
            (await db.AlteracoesDeCampo.CountAsync(a => a.Entidade == nameof(ForecastDaGerencia) && a.Campo == "ExcluidoEm")).Should().Be(2);
        }
    }

    [Fact]
    public async Task A_cota_que_some_dentro_dos_meses_da_leitura_e_excluida()
    {
        await Sincronizar();

        var semA2 = Cotas().Where(c => c.Cota != "2").ToList();
        (await Sincronizar(Leitura(cotas: semA2), quando: Agora.AddDays(1)))
            .Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1);

        await using var db = Sistema();
        (await db.CotasDeConsorcioVendidas.IgnoreQueryFilters().AsNoTracking().SingleAsync(c => c.Cota == "2")).ExcluidoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task A_cota_de_um_mes_que_a_leitura_nao_cobre_fica_como_estava()
    {
        await Sincronizar();

        // O ANO FISCAL VIROU: o painel agora só traz novembro, e as cotas de agosto e setembro não vêm — não foram canceladas.
        var novembro = new HashSet<DateOnly> { new(2026, 11, 1) };
        var relatorio = await Sincronizar(Leitura(cotas: [], meses: novembro), quando: Agora.AddDays(40));

        relatorio.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(0);
        await using var db = Sistema();
        (await db.CotasDeConsorcioVendidas.CountAsync(c => c.ExcluidoEm == null)).Should().Be(2);
    }

    [Fact]
    public async Task A_cota_que_vai_para_a_digital_sai_da_filial()
    {
        await Sincronizar();

        var cotas = Cotas();
        cotas[0] = Cota("1000", "1", filial: "DIGITAL");
        var relatorio = await Sincronizar(Leitura(cotas: cotas), quando: Agora.AddDays(1));

        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoConsorcio, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDasCotasSemFilial).Should().Be(2);
        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoConsorcio, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1,
            "a cota não é mais de Ituverava, e não pode contar no realizado dela");
    }

    [Fact]
    public async Task Mais_de_5_por_cento_de_forecast_ilegivel_aborta_e_nao_grava_nada()
    {
        var forecast = Enumerable.Range(1, 20).Select(i => Previsao(i)).ToList();
        forecast[0] = Previsao(1, forecast: "cinco");
        forecast[1] = Previsao(2, bestGuess: "2.5");

        var resultado = await Tentar(Leitura(forecast: forecast));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("2 de 20").And.Contain(CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoForecast).And.Contain("ABORTADA")
            .And.Contain("frescor não foi carimbado");
        resultado.Erro.Should().NotContain(GestorNorte, "o gestor nunca sai na mensagem");

        await using var db = Sistema();
        (await db.GestoresDosConsultores.CountAsync()).Should().Be(0, "a rodada inteira é abortada, e não só a rota");
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Uma_linha_ilegivel_abaixo_da_trava_nao_apaga_a_que_ja_existia()
    {
        var forecast = Enumerable.Range(1, 40).Select(i => Previsao(i)).ToList();
        await Sincronizar(Leitura(forecast: forecast));

        forecast[0] = Previsao(1, mes: "setembro");
        var relatorio = await Sincronizar(Leitura(forecast: forecast), quando: Agora.AddDays(1));

        Na(relatorio, CargaDoPlanejamentoDaGestaoDeNegocios.EtapaDoForecast, CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeRecusadas).Should().Be(1);
        relatorio.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(0, "a linha veio — só não passou no saneamento");
        await using var db = Sistema();
        (await db.ForecastsDaGerencia.AsNoTracking().SingleAsync(f => f.IdNaOrigem == 1)).Competencia.Should().Be(new DateOnly(2026, 9, 1));
    }

    [Fact]
    public async Task Remocao_acima_de_20_por_cento_aborta_e_aceitar_remocao_passa()
    {
        var time = Enumerable.Range(1, 30).Select(i => new ConsultorNaGestao(i, $"CONSULTOR.{i}", GestorNorte, "5", null)).ToList();
        await Sincronizar(Leitura(time: time));

        // 10 DE 30 SOMEM (33%): leitura parcial.
        var parcial = Leitura(time: time.Take(20).ToList());
        var resultado = await Tentar(parcial, quando: Agora.AddDays(1));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("10 de 30").And.Contain("ABORTADA").And.Contain("--aceitar-remocao");
        await using (var db = Sistema())
            (await db.GestoresDosConsultores.CountAsync(g => g.ExcluidoEm == null)).Should().Be(30);

        var aceita = await Sincronizar(parcial, aceitarRemocao: true, quando: Agora.AddDays(2));
        aceita.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(10);
        aceita.Observacoes.Should().ContainSingle(o => o.Contains("--aceitar-remocao", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(19, 19, false)]
    [InlineData(20, 4, false)]
    [InlineData(20, 5, true)]
    [InlineData(100, 20, false)]
    [InlineData(100, 21, true)]
    public void A_trava_de_remocao_e_20_por_cento_a_partir_de_20_vigentes(int vigentes, int aExcluir, bool aborta) =>
        CargaDoPlanejamentoDaGestaoDeNegocios.RemocaoPassaDaTrava(vigentes, aExcluir).Should().Be(aborta);

    [Fact]
    public async Task A_simulacao_nao_grava_nada()
    {
        var relatorio = await Sincronizar(simular: true);

        relatorio.Simulada.Should().BeTrue();
        relatorio.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeNovas).Should().Be(7);

        await using var db = Sistema();
        (await db.GestoresDosConsultores.CountAsync()).Should().Be(0);
        (await db.ForecastsDaGerencia.CountAsync()).Should().Be(0);
        (await db.CotasDeConsorcioVendidas.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Tudo_vazio_nao_exclui_nada()
    {
        await Sincronizar();

        var resultado = await Tentar(Leitura(time: [], forecast: [], cotas: []), quando: Agora.AddDays(1));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("vieram vazios");
        await using var db = Sistema();
        (await db.GestoresDosConsultores.CountAsync(g => g.ExcluidoEm == null)).Should().Be(3);
    }
}
