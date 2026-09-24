using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDeCarteirasDoVortice;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A SINCRONIA DAS CARTEIRAS NO SQL SERVER DE VERDADE — o contêiner de desenvolvimento, com a cadeia inteira de
/// migrações aplicada.
///
/// <para><b>O que só o motor de verdade prova:</b> o índice único FILTRADO de <c>comercial.ClienteCarteira</c>
/// (<c>[DesvinculadoEm] IS NULL</c>) — é ele que deixa o cliente que volta à carteira ganhar uma linha nova ao lado
/// da encerrada; a tradução das consultas com lista de identificadores (<c>OPENJSON</c>); as colações e os CHECKs da
/// trilha e do de-para; e a identidade das tabelas, que no SQLite o teste fixa à mão.</para>
///
/// <para><b>Como rodar:</b> suba o contêiner (<c>./scripts/banco/subir-banco.ps1</c>) e rode <c>dotnet test</c>. Sem
/// servidor, o teste é IGNORADO com a razão dita em voz alta — nunca passa em silêncio fingindo que rodou.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class CarteirasDoVorticeNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmCarteirasTeste";

    private static readonly DateTime Agora = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    private static Semente Recriar()
    {
        SqlConnection.ClearAllPools();
        using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        return Semear(db, forcarIdentificadores: false);
    }

    private static async Task<RelatorioDaSincroniaDeCarteiras> Sincronizar(
        Semente semente, LeituraDasCarteirasDoVortice leitura, DateTime quando, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.DeParaDeFiliais.Values.ToHashSet()));

        var resultado = await Sincronia(Abrir, leitura, quando, semente).ExecutarAsync(simular, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_sincronia_cria_mantem_encerra_e_reabre_sem_tropecar_no_indice_filtrado()
    {
        var semente = Recriar();

        var primeira = await Sincronizar(semente, Leitura(), Agora);
        primeira.Valor(CargaDeCarteirasDoVortice.RotuloDeCarteirasCriadas).Should().Be(5);
        primeira.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosCriados).Should().Be(5);
        primeira.Valor(CargaDeCarteirasDoVortice.RotuloDeDonosCriados).Should().Be(3);

        var segunda = await Sincronizar(semente, Leitura(), Agora.AddDays(1));
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosCriados).Should().Be(0, "sem mudança na origem, nada é gravado");
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosEncerrados).Should().Be(0);
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosMantidos).Should().Be(5);

        // O CLIENTE 1 SAI DA CARTEIRA E VOLTA: a volta é uma linha nova ao lado da encerrada — o índice único vale só
        // para o vínculo vigente, e é o motor que prova isso.
        var semO1 = Vinculos().Where(v => v.Chave != "1/18").ToList();
        (await Sincronizar(semente, Leitura(vinculos: semO1), Agora.AddDays(2)))
            .Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosEncerrados).Should().Be(1);
        (await Sincronizar(semente, Leitura(), Agora.AddDays(3)))
            .Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosCriados).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var cliente1 = ClienteId(db, CpfDoCliente1);
        var linhas = await db.ClienteCarteiras.AsNoTracking().Where(v => v.ClienteId == cliente1).OrderBy(v => v.Id).ToListAsync();
        linhas.Should().HaveCount(2);
        linhas[0].DesvinculadoEm.Should().Be(Agora.AddDays(2));
        linhas[1].DesvinculadoEm.Should().BeNull();

        (await db.RegistrosDeOrigem.CountAsync(r => r.Fluxo == CargaDeCarteirasDoVortice.Fluxo)).Should().Be(11);
        (await db.Usuarios.CountAsync(u => u.AguardandoLiberacaoDesde != null)).Should().Be(3);
        (await db.LinhasDeNegocio.SingleAsync()).Codigo.Should().Be("MAQ_NOVOS");
    }

    [FatoSeHouverSqlServer]
    public async Task A_migracao_reativa_Guaira_Ituverava_e_Monte_Alto_e_nao_mexe_em_mais_nada()
    {
        // O BANCO COMO O SERVIDOR ESTAVA: migrado até a rotina das carteiras, com as três filiais inativas — e a
        // Colorado, que continua inativa.
        SqlConnection.ClearAllPools();
        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
        {
            await db.Database.EnsureDeletedAsync();
            await db.GetService<IMigrator>().MigrateAsync("RotinaDasCarteirasDoVortice");
            await db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO organizacao.Empresa (ChavePublica, Codigo, Nome, Caminho, Nivel, EstaAtiva, CriadoEm) VALUES
                (NEWID(), '010101', N'Tracbel Agro — Ribeirão Preto', '/', 0, 1, SYSUTCDATETIME()),
                (NEWID(), '010104', N'Tracbel Agro — Guaíra', '/', 0, 0, SYSUTCDATETIME()),
                (NEWID(), '010105', N'Tracbel Agro — Ituverava', '/', 0, 0, SYSUTCDATETIME()),
                (NEWID(), '010110', N'Tracbel Agro — Monte Alto', '/', 0, 0, SYSUTCDATETIME()),
                (NEWID(), 'CFRA_COLORADO_6', N'Colorado Desativado', '/', 0, 0, SYSUTCDATETIME());");
        }

        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
        {
            await db.Database.MigrateAsync();

            var ativas = await db.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Codigo, e => e.EstaAtiva);
            ativas.Should().Equal(new Dictionary<string, bool>
            {
                ["010101"] = true,
                ["010104"] = true,
                ["010105"] = true,
                ["010110"] = true,
                ["CFRA_COLORADO_6"] = false
            }, "a filial 06 do Vórtice é a Colorado, que não existe no Protheus: ela não é reativada nem inventada");

            // E A PRÓXIMA SUBIDA DA API não refaz nada: a migração já está no histórico.
            (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        }
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var semente = Recriar();

        var relatorio = await Sincronizar(semente, Leitura(), Agora, simular: true);
        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeCasados).Should().Be(5);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.Carteiras.CountAsync()).Should().Be(0);
        (await db.ClienteCarteiras.CountAsync()).Should().Be(0);
        (await db.RegistrosDeOrigem.CountAsync()).Should().Be(0);
        (await db.Usuarios.CountAsync()).Should().Be(2);
    }
}

/// <summary>
/// Marca um teste que só faz sentido com um SQL Server de verdade respondendo — o mesmo critério do
/// <c>FatoSeHouverSqlServer</c> dos testes de arquitetura: sem servidor, IGNORADO com a razão escrita; nunca falha
/// por falta de infraestrutura, e nunca passa em silêncio.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FatoSeHouverSqlServerAttribute : FactAttribute
{
    /// <summary>Decide, uma vez por execução, se há servidor respondendo.</summary>
    public FatoSeHouverSqlServerAttribute()
    {
        if (SqlServerDoConteiner.Indisponivel is { } motivo) Skip = motivo;
    }
}

/// <summary>
/// COMO CHEGAR AO SQL SERVER DE DESENVOLVIMENTO, na ordem dos testes de arquitetura: a variável de ambiente que os
/// scripts exportam, depois <c>infra/.env</c>, depois o padrão.
/// </summary>
internal static class SqlServerDoConteiner
{
    private static readonly Lazy<string?> Diagnostico = new(Verificar);

    /// <summary>Nulo quando há servidor respondendo; a razão do salto quando não há.</summary>
    public static string? Indisponivel => Diagnostico.Value;

    /// <summary>A cadeia de conexão para um banco específico.</summary>
    public static string MontarPara(string banco) =>
        new SqlConnectionStringBuilder(Base()) { InitialCatalog = banco, Pooling = false }.ConnectionString;

    private static string Base()
    {
        var doAmbiente = Environment.GetEnvironmentVariable("ConnectionStrings__Crm");
        if (!string.IsNullOrWhiteSpace(doAmbiente)) return doAmbiente;

        var config = LerArquivoDeAmbiente();
        var usuario = config.GetValueOrDefault("DB_USUARIO", "TracbelCrm");
        var banco = config.GetValueOrDefault("DB_NOME", "TracbelCrm");
        var porta = config.GetValueOrDefault("DB_PORTA", "1433");
        var senha = config.GetValueOrDefault("DB_SENHA_APLICACAO", "");

        return $"Server=localhost,{porta};Database={banco};User Id={usuario};Password={senha};TrustServerCertificate=True";
    }

    private static Dictionary<string, string> LerArquivoDeAmbiente()
    {
        var config = new Dictionary<string, string>(StringComparer.Ordinal);

        var atual = new DirectoryInfo(AppContext.BaseDirectory);
        while (atual is not null && !atual.EnumerateFiles("*.sln").Any()) atual = atual.Parent;
        if (atual is null) return config;

        var caminho = Path.Combine(atual.FullName, "infra", ".env");
        if (!File.Exists(caminho)) return config;

        foreach (var linha in File.ReadAllLines(caminho))
        {
            var texto = linha.Trim();
            if (texto.Length == 0 || texto.StartsWith('#')) continue;

            var separador = texto.IndexOf('=', StringComparison.Ordinal);
            if (separador <= 0) continue;

            config[texto[..separador].Trim()] = texto[(separador + 1)..].Trim();
        }

        return config;
    }

    private static string? Verificar()
    {
        try
        {
            using var conexao = new SqlConnection(new SqlConnectionStringBuilder(Base()) { InitialCatalog = "master", ConnectTimeout = 5 }.ConnectionString);
            conexao.Open();
            return null;
        }
        catch (Exception falha) when (falha is SqlException or InvalidOperationException or ArgumentException)
        {
            return "Sem SQL Server de desenvolvimento respondendo (suba o contêiner com ./scripts/banco/subir-banco.ps1; " +
                   $"sem infra/.env no worktree, copie-o): {falha.GetType().Name}.";
        }
    }
}
