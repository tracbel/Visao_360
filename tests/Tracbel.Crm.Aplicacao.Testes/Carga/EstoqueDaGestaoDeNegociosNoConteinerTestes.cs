using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoEstoque;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O ESTOQUE E A COBERTURA NO SQL SERVER DE VERDADE (28/09/2026) — com a cadeia inteira de migrações. O que só o motor de
/// verdade prova: os CHECKs da cobertura, o índice único por chassi interno, a conferência da tabela em <c>sys.tables</c>
/// que a simulação faz, a rotina 12 semeada e o Down com trilha e execução da rotina. Banco próprio
/// (<c>TracbelCrmEstoqueTeste</c>).
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class EstoqueDaGestaoDeNegociosNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmEstoqueTeste";

    private static readonly DateTime Agora = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

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

    private static async Task<RelatorioDoEstoque> Sincronizar(SementeDasMetas semente, LeituraDoEstoqueNaOrigem leitura, DateTime quando, bool simular = false)
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

        (await Sincronizar(semente, Leitura(), Agora)).Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeNovas).Should().Be(8);
        (await Sincronizar(semente, Leitura(), Agora.AddHours(1))).Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeIguais).Should().Be(8);

        var mudou = Estoque().Where(m => m.Chaint != "100002").ToList();
        mudou[0] = Maquina("100001", reservado: "Sim");
        var terceira = await Sincronizar(semente, Leitura(mudou), Agora.AddHours(2));
        terceira.Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(1);
        terceira.Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1);

        (await Sincronizar(semente, Leitura(), Agora.AddHours(3))).Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeReativadas).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.EquipamentosEmEstoque.IgnoreQueryFilters().CountAsync()).Should().Be(4, "a volta é a mesma linha");
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(EquipamentoEmEstoque)).ToListAsync();
        trilha.Count(a => a.Campo == nameof(EquipamentoEmEstoque.Reservado)).Should().Be(2, "de não para sim, e de volta");
        trilha.Count(a => a.Campo == "ExcluidoEm").Should().Be(2, "a venda e a volta");
        (await db.Rotinas.SingleAsync(r => r.Id == 12)).Codigo.Should().Be("ESTOQUE_GESTAO_NEGOCIOS");
    }

    [FatoSeHouverSqlServer]
    public async Task O_banco_recusa_cobertura_por_mes_sem_mes_e_meses_negativos()
    {
        var semente = Recriar();
        await Sincronizar(semente, Leitura(), Agora);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var semMes = () => db.Database.ExecuteSqlRawAsync("UPDATE frota.CoberturaDoEstoque SET Competencia = NULL WHERE Recorte = 'MES'");
        var negativa = () => db.Database.ExecuteSqlRawAsync("UPDATE frota.CoberturaDoEstoque SET MesesDeEstoque = -1");

        await semMes.Should().ThrowAsync<SqlException>().WithMessage("*CK_CoberturaDoEstoque_Competencia*");
        await negativa.Should().ThrowAsync<SqlException>().WithMessage("*CK_CoberturaDoEstoque_Valores*");
    }

    [FatoSeHouverSqlServer]
    public async Task O_Down_da_migracao_desfaz_a_frente_mesmo_com_trilha_e_execucao_da_rotina()
    {
        var semente = Recriar();
        await Sincronizar(semente, Leitura(), Agora);
        var mudou = Estoque();
        mudou[0] = Maquina("100001", reservado: "Sim");
        await Sincronizar(semente, Leitura(mudou), Agora.AddHours(1));

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO integracao.ExecucaoDeRotina (RotinaId, IniciadaEm, TerminadaEm, Motivo, Resultado, Maquina, Mensagem, CodigoDeSaida, PedidaPorId) " +
            "VALUES (12, SYSUTCDATETIME(), SYSUTCDATETIME(), 'Agenda', 'Falha', N'teste', N'teste do Down', 3, NULL)");

        var migrador = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        var desfazer = () => migrador.MigrateAsync("20260928022745_TelemetriaDoOperationsCenter");
        await desfazer.Should().NotThrowAsync("o Down apaga antes o que referencia o que ele remove");

        (await db.Rotinas.AsNoTracking().AnyAsync(r => r.Id == 12)).Should().BeFalse();
        await migrador.MigrateAsync();
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var semente = Recriar();

        (await Sincronizar(semente, Leitura(), Agora, simular: true)).Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeNovas).Should().Be(8);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.EquipamentosEmEstoque.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }
}
