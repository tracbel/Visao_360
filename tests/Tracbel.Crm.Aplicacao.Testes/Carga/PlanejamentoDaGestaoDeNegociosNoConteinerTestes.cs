using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoPlanejamento;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O PLANEJAMENTO DA GESTÃO DE NEGÓCIOS NO SQL SERVER DE VERDADE (28/09/2026) — o contêiner, com a cadeia inteira de
/// migrações aplicada (até <c>PlanejamentoDaGestaoDeNegocios</c>).
///
/// <para><b>O que só o motor de verdade prova:</b> os CHECKs das três tabelas (a competência no dia 1, as quantidades do
/// forecast, os valores da cota), os índices únicos por id e por grupo e cota, a conferência da tabela que a simulação faz em
/// <c>sys.tables</c>, a trilha gravada na mesma transação, e o Down com trilha e chave externa das três.</para>
///
/// <para><b>Banco próprio</b> (<c>TracbelCrmPlanejamentoTeste</c>): um banco por teste de contêiner.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class PlanejamentoDaGestaoDeNegociosNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmPlanejamentoTeste";

    private static readonly DateTime Agora = new(2026, 9, 28, 9, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    private static SementeDasMetas Recriar()
    {
        SqlConnection.ClearAllPools();
        using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        return Semear(db, forcarIdentificadores: false);
    }

    private static async Task<RelatorioDoPlanejamento> Sincronizar(
        SementeDasMetas semente, LeituraDoPlanejamentoNaOrigem leitura, DateTime quando, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));

        var resultado = await Sincronia(Abrir, leitura, quando, semente.Operador).ExecutarAsync(simular, false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_sincronia_cria_revisa_exclui_e_reativa_com_a_trilha()
    {
        var semente = Recriar();

        (await Sincronizar(semente, Leitura(), Agora)).Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeNovas).Should().Be(7);
        (await Sincronizar(semente, Leitura(), Agora.AddDays(1))).Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeIguais)
            .Should().Be(7, "a segunda leitura igual grava zero");

        var revisado = Forecast();
        revisado[0] = Previsao(1, forecast: "9");
        var semA2 = Cotas().Where(c => c.Cota != "2").ToList();
        var terceira = await Sincronizar(semente, Leitura(forecast: revisado, cotas: semA2), Agora.AddDays(2));
        terceira.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(1);
        terceira.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1);

        (await Sincronizar(semente, Leitura(), Agora.AddDays(3))).Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeReativadas).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.CotasDeConsorcioVendidas.IgnoreQueryFilters().CountAsync()).Should().Be(2, "a volta é a mesma linha");
        (await db.CotasDeConsorcioVendidas.CountAsync(c => c.ExcluidoEm == null)).Should().Be(2);
        (await db.ForecastsDaGerencia.SingleAsync(f => f.IdNaOrigem == 1)).Forecast.Should().Be(5, "a última leitura trouxe o 5 de volta");

        var trilha = await db.AlteracoesDeCampo.AsNoTracking().ToListAsync();
        trilha.Count(a => a.Entidade == nameof(ForecastDaGerencia) && a.Campo == nameof(ForecastDaGerencia.Forecast)).Should().Be(2, "de 5 para 9, e de 9 para 5");
        trilha.Count(a => a.Entidade == nameof(CotaDeConsorcioVendida) && a.Campo == "ExcluidoEm").Should().Be(2, "a saída e a volta");
    }

    [FatoSeHouverSqlServer]
    public async Task O_banco_recusa_competencia_fora_do_dia_1_e_forecast_negativo()
    {
        var semente = Recriar();
        await Sincronizar(semente, Leitura(), Agora);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var mes = () => db.Database.ExecuteSqlRawAsync("UPDATE organizacao.ForecastDaGerencia SET Competencia = '2026-09-15' WHERE IdNaOrigem = 1");
        var negativo = () => db.Database.ExecuteSqlRawAsync("UPDATE organizacao.ForecastDaGerencia SET BestGuess = -1 WHERE IdNaOrigem = 1");
        var cota = () => db.Database.ExecuteSqlRawAsync("UPDATE organizacao.CotaDeConsorcioVendida SET Competencia = '2026-09-02'");

        await mes.Should().ThrowAsync<SqlException>().WithMessage("*CK_ForecastDaGerencia_Competencia*");
        await negativo.Should().ThrowAsync<SqlException>().WithMessage("*CK_ForecastDaGerencia_Quantidades*");
        await cota.Should().ThrowAsync<SqlException>().WithMessage("*CK_CotaDeConsorcioVendida_Competencia*");
    }

    [FatoSeHouverSqlServer]
    public async Task O_Down_da_migracao_desfaz_a_frente_mesmo_com_trilha()
    {
        var semente = Recriar();
        await Sincronizar(semente, Leitura(), Agora);
        var revisado = Forecast();
        revisado[0] = Previsao(1, forecast: "9");
        await Sincronizar(semente, Leitura(forecast: revisado), Agora.AddDays(1));

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.AlteracoesDeCampo.CountAsync(a => a.Entidade == nameof(ForecastDaGerencia))).Should().BeGreaterThan(0);

        var migrador = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        var desfazer = () => migrador.MigrateAsync("20260928022745_TelemetriaDoOperationsCenter");
        await desfazer.Should().NotThrowAsync("o Down apaga antes a trilha das três, que o CHECK recriado recusaria");

        (await db.Database.SqlQueryRaw<int>(
                "SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'organizacao.ForecastDaGerencia')")
            .SingleAsync()).Should().Be(0);
        await migrador.MigrateAsync();
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var semente = Recriar();

        var relatorio = await Sincronizar(semente, Leitura(), Agora, simular: true);
        relatorio.Valor(CargaDoPlanejamentoDaGestaoDeNegocios.RotuloDeNovas).Should().Be(7);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.ForecastsDaGerencia.CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }
}
