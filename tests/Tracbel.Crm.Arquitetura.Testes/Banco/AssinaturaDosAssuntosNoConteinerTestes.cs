using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Cache;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// A ASSINATURA DOS ASSUNTOS NO SQL SERVER DE VERDADE (plano 2 do documento 54): os testes da API rodam em SQLite, e a
/// consulta única — subconsultas escalares, LEN, CAST para bigint — precisa traduzir e rodar no banco do servidor.
/// </summary>
public sealed class AssinaturaDosAssuntosNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmAssinaturaTeste";

    [FatoSeHouverSqlServer]
    public async Task Cada_assunto_le_a_assinatura_numa_consulta_que_o_sql_server_executa()
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlServer(
                ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
            .Options;
        await using var db = new CrmDbContext(opcoes, new ProvedorDeSistema());

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        foreach (var assunto in Enum.GetValues<AssuntoDeReferencia>())
            (await AssinaturaDosAssuntos.LerDoBancoAsync(db, assunto, default))
                .Should().NotBeNullOrEmpty($"a semente cria as categorias de máquina, e a assinatura de {assunto} sai delas");

        await db.Database.EnsureDeletedAsync();
    }

    /// <summary>Contexto de sistema — o teste lê o esquema, nunca dado por filial.</summary>
    private sealed class ProvedorDeSistema : IProvedorContextoAcesso
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
