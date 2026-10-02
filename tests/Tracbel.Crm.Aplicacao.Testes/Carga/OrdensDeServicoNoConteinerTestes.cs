using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasOrdensDeServico;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// AS ORDENS DE SERVIÇO NO SQL SERVER DE VERDADE (02/10/2026) — com a cadeia inteira de migrações. O que só o motor de
/// verdade prova: os CHECKs da OS, o índice único por filial e número, as chaves estrangeiras para cliente e máquina, a
/// conferência da tabela em <c>sys.tables</c> que a simulação faz, a rotina 15 semeada desligada e o Down com trilha e
/// execução da rotina. Banco próprio (<c>TracbelCrmOrdensTeste</c>).
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class OrdensDeServicoNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmOrdensTeste";

    private static readonly DateTime Agora = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    private static (SementeDasMetas Semente, long ClienteId, long MaquinaId) Recriar()
    {
        SqlConnection.ClearAllPools();
        using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        var semente = Semear(db, forcarIdentificadores: false);
        var (cliente, maquina) = SemearAOficina(db, semente);
        return (semente, cliente, maquina);
    }

    private static async Task<RelatorioDasOrdensDeServico> Sincronizar(
        SementeDasMetas semente, LeituraDasOrdensDeServico leitura, DateTime quando, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));

        var resultado = await Sincronia(Abrir, leitura, quando, semente.Operador).ExecutarAsync(simular, false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_sincronia_cria_atualiza_exclui_e_reativa_com_a_trilha()
    {
        var (semente, clienteId, maquinaId) = Recriar();

        (await Sincronizar(semente, Leitura(Agora), Agora)).Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeNovas).Should().Be(2);
        (await Sincronizar(semente, Leitura(Agora), Agora.AddHours(1))).Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeIguais).Should().Be(2);

        var mudou = ItensPadrao().Where(i => i.NumeroOs != "00000002").ToList();
        mudou[0] = mudou[0] with { StatusDaCapa = "L" };
        mudou[1] = mudou[1] with { StatusDaCapa = "L" };
        var terceira = await Sincronizar(semente, Leitura(Agora, mudou), Agora.AddHours(2));
        terceira.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeAtualizadas).Should().Be(1);
        terceira.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeExcluidas).Should().Be(1);

        (await Sincronizar(semente, Leitura(Agora), Agora.AddHours(3))).Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeReativadas).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var ordens = await db.OrdensDeServico.IgnoreQueryFilters().AsNoTracking().OrderBy(o => o.ChaveNaOrigem).ToListAsync();
        ordens.Should().HaveCount(2, "a volta é a mesma linha");
        (ordens[0].ClienteId, ordens[0].EquipamentoId, ordens[0].ValorDePecas, ordens[0].ValorDeServicos)
            .Should().Be(((long?)clienteId, (long?)maquinaId, 270m, 285m));

        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(OrdemDeServico)).ToListAsync();
        trilha.Count(a => a.Campo == nameof(OrdemDeServico.Situacao)).Should().Be(2, "de liberada e de volta a aberta");
        trilha.Count(a => a.Campo == "ExcluidoEm").Should().Be(2, "a que sumiu e a volta");

        var rotina = await db.Rotinas.AsNoTracking().SingleAsync(r => r.Id == 15);
        (rotina.Codigo, rotina.EstaLigada).Should().Be(("POS_VENDA_PROTHEUS", false));
    }

    [FatoSeHouverSqlServer]
    public async Task O_banco_recusa_situacao_desconhecida_horimetro_negativo_e_OS_repetida()
    {
        var (semente, _, _) = Recriar();
        await Sincronizar(semente, Leitura(Agora), Agora);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var situacao = () => db.Database.ExecuteSqlRawAsync("UPDATE frota.OrdemDeServico SET Situacao = 'Perdida'");
        var horimetro = () => db.Database.ExecuteSqlRawAsync("UPDATE frota.OrdemDeServico SET Horimetro = -1");
        var repetida = () => db.Database.ExecuteSqlRawAsync(
            "INSERT INTO frota.OrdemDeServico (EmpresaId, SistemaId, ChaveNaOrigem, Numero, Situacao, AbertaEm, ValorDePecas, ValorDeServicos, " +
            "ItensDePeca, ItensDeServico, HashDaOrigem, LidaEm, CriadoEm, CriadoPorId) " +
            "SELECT EmpresaId, SistemaId, ChaveNaOrigem, Numero, Situacao, AbertaEm, 0, 0, 0, 0, 'x', SYSUTCDATETIME(), SYSUTCDATETIME(), CriadoPorId " +
            "FROM frota.OrdemDeServico WHERE Numero = '00000001'");

        await situacao.Should().ThrowAsync<SqlException>().WithMessage("*CK_OrdemDeServico_Situacao*");
        await horimetro.Should().ThrowAsync<SqlException>().WithMessage("*CK_OrdemDeServico_Horimetro*");
        await repetida.Should().ThrowAsync<SqlException>().WithMessage("*UX_OrdemDeServico_Sistema_Chave*");
    }

    [FatoSeHouverSqlServer]
    public async Task O_Down_da_migracao_desfaz_a_frente_mesmo_com_trilha_e_execucao_da_rotina()
    {
        var (semente, _, _) = Recriar();
        await Sincronizar(semente, Leitura(Agora), Agora);
        var mudou = ItensPadrao();
        mudou[0] = mudou[0] with { StatusDaCapa = "L" };
        mudou[1] = mudou[1] with { StatusDaCapa = "L" };
        await Sincronizar(semente, Leitura(Agora, mudou), Agora.AddHours(1));

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO integracao.ExecucaoDeRotina (RotinaId, IniciadaEm, TerminadaEm, Motivo, Resultado, Maquina, Mensagem, CodigoDeSaida, PedidaPorId) " +
            "VALUES (15, SYSUTCDATETIME(), SYSUTCDATETIME(), 'Agenda', 'Falha', N'teste', N'teste do Down', 3, NULL)");

        var migrador = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        var desfazer = () => migrador.MigrateAsync("20261001144958_RotinaDasMetasDeHoraEmHora");
        await desfazer.Should().NotThrowAsync("o Down apaga antes o que referencia o que ele remove");

        (await db.Rotinas.AsNoTracking().AnyAsync(r => r.Id == 15)).Should().BeFalse();
        await migrador.MigrateAsync();
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var (semente, _, _) = Recriar();

        (await Sincronizar(semente, Leitura(Agora), Agora, simular: true)).Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeNovas).Should().Be(2);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.OrdensDeServico.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync(p => p.Fluxo == OrdemDeServico.FluxoDaCarga)).Should().Be(0);
    }
}
