using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A CONFERÊNCIA NO SQL SERVER DE VERDADE (28/09/2026) — o CHECK dos tipos novos de divergência, os da tabela da
/// conferência, a rotina 13 semeada e o Down com divergência e execução da rotina. Banco próprio (<c>TracbelCrmConferenciaTeste</c>).
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class ConferenciaDaGestaoDeNegociosNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmConferenciaTeste";

    private static readonly DateTime Agora = new(2026, 9, 28, 10, 15, 0, DateTimeKind.Utc);

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

    private static LeituraDaConferenciaNaOrigem Gabarito() => new(
        [new RealizadoNaGestao("1PY6155MCSS000011", "Ribeirão Preto", "Ago/2026", "1"), new RealizadoNaGestao("1PY6155MCSS000012", "Ituverava", "Ago/2026", "1")],
        [new MetaNaGestao("Ribeirão Preto", "Ago/2026", "5")],
        [new FilialDaGestao(1, "Ribeirão Preto", "010101"), new FilialDaGestao(5, "Ituverava", "010105")],
        null);

    private static async Task Apurar(SementeDasMetas semente)
    {
        var opcoes = Opcoes();
        var resultado = await new CargaDaConferenciaDaGestaoDeNegocios(
                () => new CrmDbContext(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais)),
                _ => Task.FromResult(Resultado<LeituraDaConferenciaNaOrigem>.Ok(Gabarito())), semente.Operador, () => Agora, _ => { })
            .ExecutarAsync(false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_conferencia_grava_os_tipos_novos_e_o_resumo()
    {
        var semente = Recriar();
        await Apurar(semente);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.DivergenciasDeIntegracao.CountAsync(d => d.Tipo == TipoDeDivergencia.RealizadoSoNaGestao)).Should().Be(2, "o CHECK aceita o tipo novo");
        (await db.ConferenciasDaGestaoDeNegocios.CountAsync()).Should().Be(3);
        (await db.Rotinas.SingleAsync(r => r.Id == 13)).Codigo.Should().Be("CONFERENCIA_GESTAO_NEGOCIOS");

        var negativo = () => db.Database.ExecuteSqlRawAsync("UPDATE integracao.ConferenciaDaGestaoDeNegocios SET NaGestao = -1");
        await negativo.Should().ThrowAsync<SqlException>().WithMessage("*CK_ConferenciaDaGestaoDeNegocios_Valores*");
    }

    [FatoSeHouverSqlServer]
    public async Task O_Down_desfaz_a_frente_mesmo_com_divergencia_e_execucao_da_rotina()
    {
        var semente = Recriar();
        await Apurar(semente);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO integracao.ExecucaoDeRotina (RotinaId, IniciadaEm, TerminadaEm, Motivo, Resultado, Maquina, Mensagem, CodigoDeSaida, PedidaPorId) " +
            "VALUES (13, SYSUTCDATETIME(), SYSUTCDATETIME(), 'Agenda', 'Falha', N'teste', N'teste do Down', 3, NULL)");

        var migrador = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        var desfazer = () => migrador.MigrateAsync("20260928154329_EstoqueDaGestaoDeNegocios");
        await desfazer.Should().NotThrowAsync("o Down apaga antes as divergências dos tipos novos e a execução da rotina 13");

        (await db.Rotinas.AsNoTracking().AnyAsync(r => r.Id == 13)).Should().BeFalse();
        await migrador.MigrateAsync();
    }
}
