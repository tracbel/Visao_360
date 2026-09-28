using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O FINANCIAMENTO DAS VENDAS NO SQL SERVER DE VERDADE (issue 262) — com a cadeia inteira de migrações: a carga cria,
/// exclui e reativa com a trilha, o CHECK do valor, a conferência da tabela em <c>sys.tables</c> que a simulação faz, e o
/// Down com trilha. Banco próprio (<c>TracbelCrmFinanciamentoTeste</c>).
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class FinanciamentoDaVendaNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmFinanciamentoTeste";

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
        var semente = Semear(db, forcarIdentificadores: false);
        db.Municipios.Add(Municipio.Criar("Franca", "SP", 3516200));
        db.SaveChanges();
        return semente;
    }

    private static FinanciamentoNoVortice Resposta(long processo, string? fase = "Recebimento", string linha = "MODER FROTA") =>
        new(processo, "IV_Q_VENDA_EQUIPAMENTO", processo * 10, new DateTime(2026, 3, 5), new DateTime(2026, 3, 1), 300_000m,
            "BANCO JOHN DEERE", linha, 11, "FRANCA", "SP", fase, null);

    private static async Task<RelatorioDosFinanciamentos> Sincronizar(
        SementeDasMetas semente, List<FinanciamentoNoVortice> leitura, DateTime quando, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));

        var resultado = await new CargaDosFinanciamentosDoVortice(
                Abrir, _ => Task.FromResult(Resultado<IReadOnlyList<FinanciamentoNoVortice>>.Ok(leitura)), semente.Operador, () => quando, _ => { })
            .ExecutarAsync(simular, false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private static List<FinanciamentoNoVortice> Leitura() => [.. Enumerable.Range(1, 10).Select(p => Resposta(p))];

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_sincronia_cria_revisa_exclui_e_reativa_com_a_trilha()
    {
        var semente = Recriar();

        (await Sincronizar(semente, Leitura(), Agora)).Valor(CargaDosFinanciamentosDoVortice.RotuloDeNovas).Should().Be(10);
        (await Sincronizar(semente, Leitura(), Agora.AddHours(1))).Valor(CargaDosFinanciamentosDoVortice.RotuloDeIguais).Should().Be(10);

        var mudou = Leitura();
        mudou[0] = Resposta(1, linha: "RECURSO PRÓPRIO");
        mudou[1] = Resposta(2, fase: "Cancelamento");
        var terceira = await Sincronizar(semente, mudou, Agora.AddHours(2));
        terceira.Valor(CargaDosFinanciamentosDoVortice.RotuloDeRevisadas).Should().Be(1);
        terceira.Valor(CargaDosFinanciamentosDoVortice.RotuloDeExcluidas).Should().Be(1);

        (await Sincronizar(semente, Leitura(), Agora.AddHours(3))).Valor(CargaDosFinanciamentosDoVortice.RotuloDeReativadas).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.FinanciamentosDaVenda.CountAsync()).Should().Be(10, "a volta é a mesma linha");
        (await db.FinanciamentosDaVenda.CountAsync(f => f.MunicipioId != null)).Should().Be(10);
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(FinanciamentoDaVenda)).ToListAsync();
        trilha.Count(a => a.Campo == nameof(FinanciamentoDaVenda.ContaNoCreditoRural)).Should().Be(2, "de crédito para recurso próprio, e de volta");
        trilha.Count(a => a.Campo == nameof(FinanciamentoDaVenda.ExcluidoEm)).Should().Be(2, "o cancelamento e a volta");
    }

    [FatoSeHouverSqlServer]
    public async Task O_banco_recusa_valor_acima_do_limite()
    {
        var semente = Recriar();
        await Sincronizar(semente, Leitura(), Agora);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var absurdo = () => db.Database.ExecuteSqlRawAsync("UPDATE organizacao.FinanciamentoDaVenda SET ValorFinanciado = 3455000000");
        await absurdo.Should().ThrowAsync<SqlException>().WithMessage("*CK_FinanciamentoDaVenda_Valor*");
    }

    [FatoSeHouverSqlServer]
    public async Task O_Down_da_migracao_desfaz_a_tabela_mesmo_com_trilha()
    {
        var semente = Recriar();
        await Sincronizar(semente, Leitura(), Agora);
        var mudou = Leitura();
        mudou[0] = Resposta(1, linha: "RECURSO PRÓPRIO");
        await Sincronizar(semente, mudou, Agora.AddHours(1));

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var migrador = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        var anterior = db.Database.GetMigrations().TakeWhile(m => !m.EndsWith("_FinanciamentoDaVenda", StringComparison.Ordinal)).Last();

        var desfazer = () => migrador.MigrateAsync(anterior);
        await desfazer.Should().NotThrowAsync("o Down apaga antes a trilha da entidade que ele remove");

        (await db.Database.SqlQueryRaw<int>(
                "SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'organizacao.FinanciamentoDaVenda')")
            .SingleAsync()).Should().Be(0);
        await migrador.MigrateAsync();
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var semente = Recriar();

        (await Sincronizar(semente, Leitura(), Agora, simular: true)).Valor(CargaDosFinanciamentosDoVortice.RotuloDeNovas).Should().Be(10);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.FinanciamentosDaVenda.CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }
}
