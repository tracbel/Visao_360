using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasOrdensDeServico;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasPecas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// AS PEÇAS NO SQL SERVER DE VERDADE (02/10/2026) — com a cadeia inteira de migrações: o <c>ExecuteDelete</c> da janela regravada
/// dentro da transação, os CHECKs, o índice único do orçamento, a conferência da tabela que a simulação faz e o Down com
/// trilha. Banco próprio (<c>TracbelCrmPecasTeste</c>).
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class PecasNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmPecasTeste";

    private static readonly DateTime Agora = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

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
        var semente = Semear(db, forcarIdentificadores: false);
        SemearAOficina(db, semente);
        return semente;
    }

    private static async Task<RelatorioDasPecas> Rodar(SementeDasMetas semente, LeituraDasPecas leitura, DateTime quando, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));
        var resultado = await CargaDePecas(Abrir, leitura, quando, semente.Operador).ExecutarAsync(simular, false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_janela_e_regravada_e_os_orcamentos_sincronizam_com_a_trilha()
    {
        var semente = Recriar();

        (await Rodar(semente, LeituraDePecas(Agora), Agora)).Valor(CargaDasPecasDoProtheus.RotuloDeCombinacoes).Should().Be(3);
        var orcamentos = OrcamentosPadrao();
        orcamentos[0] = orcamentos[0] with { Situacao = "Encerrado" };
        orcamentos[1] = orcamentos[1] with { Situacao = "Encerrado" };
        (await Rodar(semente, LeituraDePecas(Agora, null, orcamentos), Agora.AddHours(1))).Valor(CargaDasPecasDoProtheus.RotuloDeAtualizados).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.FaturamentosDePecasNoMes.IgnoreQueryFilters().CountAsync()).Should().Be(3, "a segunda rodada regravou a janela, sem duplicar");
        (await db.OrcamentosDePecas.IgnoreQueryFilters().CountAsync()).Should().Be(2);
        (await db.AlteracoesDeCampo.AsNoTracking().CountAsync(a => a.Entidade == nameof(OrcamentoDePecas) && a.Campo == nameof(OrcamentoDePecas.Situacao)))
            .Should().Be(1);
    }

    [FatoSeHouverSqlServer]
    public async Task O_banco_recusa_mes_fora_do_dia_1_combinacao_sem_item_e_orcamento_repetido()
    {
        var semente = Recriar();
        await Rodar(semente, LeituraDePecas(Agora), Agora);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var foraDoDia1 = () => db.Database.ExecuteSqlRawAsync("UPDATE comercial.FaturamentoDePecasNoMes SET Competencia = '2026-09-15'");
        var semItem = () => db.Database.ExecuteSqlRawAsync("UPDATE comercial.FaturamentoDePecasNoMes SET Itens = 0");
        var repetido = () => db.Database.ExecuteSqlRawAsync(
            "INSERT INTO comercial.OrcamentoDePecas (EmpresaId, SistemaId, ChaveNaOrigem, Numero, Situacao, OrcadoEm, ValorTotal, ValorDeDesconto, Itens, " +
            "HashDaOrigem, LidaEm, CriadoEm, CriadoPorId) SELECT EmpresaId, SistemaId, ChaveNaOrigem, Numero, Situacao, OrcadoEm, 0, 0, 1, 'x', " +
            "SYSUTCDATETIME(), SYSUTCDATETIME(), CriadoPorId FROM comercial.OrcamentoDePecas WHERE Numero = '000777'");

        await foraDoDia1.Should().ThrowAsync<SqlException>().WithMessage("*CK_FaturamentoDePecasNoMes_Competencia*");
        await semItem.Should().ThrowAsync<SqlException>().WithMessage("*CK_FaturamentoDePecasNoMes_Itens*");
        await repetido.Should().ThrowAsync<SqlException>().WithMessage("*UX_OrcamentoDePecas_Sistema_Chave*");
    }

    [FatoSeHouverSqlServer]
    public async Task O_Down_da_migracao_desfaz_as_pecas_mesmo_com_trilha()
    {
        var semente = Recriar();
        await Rodar(semente, LeituraDePecas(Agora), Agora);
        var orcamentos = OrcamentosPadrao();
        orcamentos[0] = orcamentos[0] with { Situacao = "Encerrado" };
        await Rodar(semente, LeituraDePecas(Agora, null, orcamentos), Agora.AddHours(1));

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var migrador = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        var desfazer = () => migrador.MigrateAsync("20261002145426_OrdensDeServicoDoProtheus");
        await desfazer.Should().NotThrowAsync("o Down apaga antes a trilha do que ele remove");

        var tabela = await db.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'comercial.OrcamentoDePecas')")
            .SingleAsync();
        tabela.Should().Be(0);
        await migrador.MigrateAsync();
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var semente = Recriar();

        (await Rodar(semente, LeituraDePecas(Agora), Agora, simular: true)).Valor(CargaDasPecasDoProtheus.RotuloDeCombinacoes).Should().Be(3);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.FaturamentosDePecasNoMes.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        (await db.OrcamentosDePecas.IgnoreQueryFilters().CountAsync()).Should().Be(0);
    }
}
