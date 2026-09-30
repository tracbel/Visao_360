using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// A ROTINA MENSAL DA PARTIÇÃO DA AUDITORIA CONTRA UM SQL SERVER DE VERDADE (issue 268).
///
/// <para><b>POR QUE NO CONTÊINER:</b> tudo o que pode quebrar aqui é do motor — <c>SPLIT</c> e <c>MERGE</c> de função de
/// partição, <c>TRUNCATE ... WITH (PARTITIONS)</c>, <c>$PARTITION</c>, <c>IDENTITY_INSERT</c> dentro de
/// <c>sp_executesql</c>. Nenhum dublê exercita isso.</para>
///
/// <para><b>O CENÁRIO.</b> O banco nasce pelas migrações, hoje: a função da trilha vai do terceiro mês anterior ao décimo
/// segundo seguinte. A rotina roda como se fosse vinte meses depois — o corte de 18 meses passa a cobrir as primeiras
/// partições. Na trilha há linhas antes do corte (de cadastro e de permissão) e depois dele.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class ParticaoDaAuditoriaNoContainerTestes
{
    /// <summary>Banco próprio deste teste, apagado e recriado a cada execução.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmParticaoDaAuditoriaTeste";

    [FatoSeHouverSqlServer]
    public async Task Tira_so_as_particoes_vencidas_mantem_a_permissao_abre_os_meses_e_rodar_de_novo_nao_muda_nada()
    {
        await using var banco = CriarContexto();
        await banco.Database.EnsureDeletedAsync();
        await banco.Database.MigrateAsync();

        var hoje = DateTime.UtcNow;
        var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);
        var primeiroLimite = inicioDoMes.AddMonths(-3);

        // Vinte meses depois do mês em que a migração criou a função: o corte (18 meses antes do mês corrente) fica
        // em primeiroLimite + 2 meses — as partições até lá estão vencidas.
        var agora = inicioDoMes.AddMonths(20).AddDays(14);
        var corte = new DateTime(agora.Year, agora.Month, 1).AddMonths(-ManutencaoDasParticoes.MesesDeRetencaoDaTrilha);

        // As chaves estrangeiras da trilha saem do caminho: o teste mede a partição, e não o cadastro de filial e usuário.
        await banco.Database.ExecuteSqlRawAsync("""
            DECLARE @sql nvarchar(max) = (
                SELECT STRING_AGG(CAST(N'ALTER TABLE [auditoria].[AlteracaoDeCampo] NOCHECK CONSTRAINT ' + QUOTENAME(name) + N';' AS nvarchar(max)), N' ')
                FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'auditoria.AlteracaoDeCampo'));
            EXEC sys.sp_executesql @sql;
            """);

        var antesDoCorte = corte.AddDays(-20);
        var bemAntes = primeiroLimite.AddDays(-40);
        var depoisDoCorte = corte.AddDays(1);

        await InserirAsync(banco, bemAntes, "Cliente", "Situacao");
        await InserirAsync(banco, antesDoCorte, "Cliente", "NomeRazao");
        await InserirAsync(banco, antesDoCorte, "Equipamento", "Chassi");
        await InserirAsync(banco, antesDoCorte, nameof(UsuarioPerfil), "ExpiraEm");
        await InserirAsync(banco, depoisDoCorte, "Cliente", "Situacao");
        await InserirAsync(banco, inicioDoMes.AddMonths(19), "Cliente", "Situacao");
        var permissaoId = await ScalarAsync(banco, $"SELECT MAX(Id) FROM auditoria.AlteracaoDeCampo WHERE Entidade = '{nameof(UsuarioPerfil)}'");

        var limitesAntes = await LimitesAsync(banco, "PF_Mensal_AlteracaoDeCampo");
        limitesAntes.Should().HaveCount(16, "a migração cria a janela do terceiro mês anterior ao décimo segundo seguinte");

        var manutencao = new ManutencaoDasParticoes(banco);

        // A SIMULAÇÃO CONTA E NÃO MUDA NADA.
        var simulada = await manutencao.ExecutarAsync(agora, simular: true, CancellationToken.None);
        simulada.LinhasExpurgadas.Should().Be(3, "saem as três de cadastro anteriores ao corte");
        simulada.MudancasDePermissaoMantidas.Should().Be(1);
        (await ContarAsync(banco)).Should().Be(6, "a simulação não esvazia nada");
        (await LimitesAsync(banco, "PF_Mensal_AlteracaoDeCampo")).Should().Equal(limitesAntes, "a simulação não parte nem une nada");

        // A RODADA DE VERDADE.
        var feita = await manutencao.ExecutarAsync(agora, simular: false, CancellationToken.None);

        feita.LinhasExpurgadas.Should().Be(3);
        feita.MudancasDePermissaoMantidas.Should().Be(1);
        feita.ParticoesExpurgadas.Should().BeGreaterThan(0);

        (await ContarAsync(banco)).Should().Be(3, "fica a mudança de permissão e as duas linhas depois do corte");
        (await ScalarAsync(banco, $"SELECT COUNT(*) FROM auditoria.AlteracaoDeCampo WHERE AlteradoEm < '{corte:yyyy-MM-dd}' AND Entidade <> '{nameof(UsuarioPerfil)}'"))
            .Should().Be(0, "nada de cadastro anterior ao corte sobrevive");
        (await ScalarAsync(banco, $"SELECT COUNT(*) FROM auditoria.AlteracaoDeCampo WHERE AlteradoEm >= '{corte:yyyy-MM-dd}'"))
            .Should().Be(2, "nenhuma linha com menos de 18 meses é apagada");
        (await ScalarAsync(banco, $"SELECT COUNT(*) FROM auditoria.AlteracaoDeCampo WHERE Id = {permissaoId}"))
            .Should().Be(1, "a mudança de permissão é permanente, e volta com o mesmo identificador");

        var limites = await LimitesAsync(banco, "PF_Mensal_AlteracaoDeCampo");
        limites.Should().NotContain(l => l < corte, "os limites antigos foram unidos");
        limites.Should().Contain(corte, "o limite do corte fica: é ele que separa o que vale do que já passou");
        limites[^1].Should().Be(new DateTime(agora.Year, agora.Month, 1).AddMonths(ManutencaoDasParticoes.MesesDeFolga + 1),
            "os próximos três meses têm partição própria");

        // TODA FUNÇÃO MENSAL DO BANCO GANHA OS MESES — hoje só a da trilha: a fase 1 removeu as outras três tabelas.
        foreach (var funcao in await manutencao.LerFuncoesMensaisAsync(CancellationToken.None))
            (await LimitesAsync(banco, funcao))[^1].Should().Be(limites[^1], $"{funcao} também ganha os meses");

        // RODAR DUAS VEZES NÃO MUDA NADA.
        var denovo = await manutencao.ExecutarAsync(agora, simular: false, CancellationToken.None);

        denovo.MesesAbertos.Should().Be(0);
        denovo.ParticoesExpurgadas.Should().Be(0);
        denovo.LimitesUnidos.Should().Be(0);
        (await ContarAsync(banco)).Should().Be(3);
        (await LimitesAsync(banco, "PF_Mensal_AlteracaoDeCampo")).Should().Equal(limites);
    }

    [FatoSeHouverSqlServer]
    public void As_entidades_de_permissao_sao_as_do_schema_de_seguranca()
    {
        using var banco = CriarContexto();

        new ManutencaoDasParticoes(banco).EntidadesDePermissao()
            .Should().Contain([nameof(Usuario), nameof(Perfil), nameof(UsuarioPerfil)])
            .And.NotContain("Cliente");
    }

    private static Task InserirAsync(CrmDbContext banco, DateTime quando, string entidade, string campo) =>
        banco.Database.ExecuteSqlAsync($"""
            INSERT INTO auditoria.AlteracaoDeCampo (AlteradoEm, EmpresaId, Entidade, RegistroId, Campo, ValorAnterior, ValorNovo,
                                                    AlteradoPorId, Origem, Operacao)
            VALUES ({quando}, 1, {entidade}, 1, {campo}, N'antes', N'depois', 1, 'Usuario', 'Alteracao');
            """);

    private static async Task<long> ContarAsync(CrmDbContext banco) =>
        await ScalarAsync(banco, "SELECT COUNT(*) FROM auditoria.AlteracaoDeCampo");

    private static async Task<long> ScalarAsync(CrmDbContext banco, string sql)
    {
        var conexao = banco.Database.GetDbConnection();
        if (conexao.State != System.Data.ConnectionState.Open) await conexao.OpenAsync();
        await using var comando = conexao.CreateCommand();
        comando.CommandText = sql;
        return Convert.ToInt64(await comando.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<List<DateTime>> LimitesAsync(CrmDbContext banco, string funcao)
    {
        var conexao = banco.Database.GetDbConnection();
        if (conexao.State != System.Data.ConnectionState.Open) await conexao.OpenAsync();
        await using var comando = conexao.CreateCommand();
        comando.CommandText = $"""
            SELECT CAST(prv.value AS datetime2(3)) FROM sys.partition_range_values prv
            JOIN sys.partition_functions pf ON pf.function_id = prv.function_id
            WHERE pf.name = N'{funcao}' ORDER BY prv.boundary_id;
            """;
        var limites = new List<DateTime>();
        await using var leitor = await comando.ExecuteReaderAsync();
        while (await leitor.ReadAsync()) limites.Add(leitor.GetDateTime(0));
        return limites;
    }

    private static CrmDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlServer(
                ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
            .Options;

        return new CrmDbContext(opcoes, new ProvedorDeSistemaDaParticao());
    }

    private sealed class ProvedorDeSistemaDaParticao : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = new(
            usuarioId: 0,
            nomeExibicao: "sistema",
            empresaId: 0,
            empresasVisiveis: new HashSet<int>(),
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(),
            ehServicoDeSistema: true);
    }
}
