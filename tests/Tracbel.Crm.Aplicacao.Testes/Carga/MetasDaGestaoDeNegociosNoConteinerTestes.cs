using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// AS METAS DA GESTÃO DE NEGÓCIOS NO SQL SERVER DE VERDADE — o contêiner de desenvolvimento, com a cadeia inteira de
/// migrações aplicada (até <c>MetasDaGestaoDeNegocios</c>).
///
/// <para><b>O que só o motor de verdade prova:</b> os CHECKs novos (a competência no dia 1, a quantidade, a origem), o
/// índice único por id da origem, a conferência da tabela que a simulação faz em <c>sys.tables</c>, a conexão 13 com o
/// tipo novo aceito pelo <c>CK_Conexao_Tipo</c>, e a trilha gravada na mesma transação.</para>
///
/// <para><b>Banco próprio</b> (<c>TracbelCrmMetasTeste</c>): testes de contêiner de outras frentes rodam no mesmo servidor, e
/// um banco por teste é o que os deixa não se atrapalhar.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class MetasDaGestaoDeNegociosNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmMetasTeste";

    private static readonly DateTime Agora = new(2026, 9, 27, 9, 0, 0, DateTimeKind.Utc);

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

    private static async Task<RelatorioDasMetasDaGestaoDeNegocios> Sincronizar(
        SementeDasMetas semente, IReadOnlyList<MetaNaOrigem> cadastro, DateTime quando, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));

        var resultado = await Sincronia(Abrir, cadastro, quando, semente.Operador).ExecutarAsync(simular, false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_sincronia_cria_revisa_exclui_e_reativa_com_a_trilha()
    {
        var semente = Recriar();

        (await Sincronizar(semente, CadastroDeMetas(), Agora)).Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeNovas).Should().Be(5);
        (await Sincronizar(semente, CadastroDeMetas(), Agora.AddDays(1))).Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeIguais)
            .Should().Be(5, "a segunda leitura igual grava zero");

        var revisado = CadastroDeMetas().Where(l => l.Id != 3).ToList();
        revisado[0] = Linha(1, quantidade: "6");
        var terceira = await Sincronizar(semente, revisado, Agora.AddDays(2));
        terceira.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(1);
        terceira.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1);

        (await Sincronizar(semente, CadastroDeMetas(), Agora.AddDays(3))).Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeReativadas).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.MetasDeVenda.CountAsync()).Should().Be(5, "a volta é a mesma linha");
        (await db.MetasDeVenda.CountAsync(m => m.ExcluidoEm == null)).Should().Be(5);
        (await db.MetasDeVenda.SingleAsync(m => m.IdNaOrigem == 1)).Quantidade.Should().Be(2, "a última leitura trouxe o 2 de volta");

        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(MetaDeVenda)).ToListAsync();
        trilha.Count(a => a.Campo == nameof(MetaDeVenda.Quantidade)).Should().Be(2, "de 2 para 6, e de 6 para 2");
        trilha.Count(a => a.Campo == "ExcluidoEm").Should().Be(2, "a saída e a volta");

        var gn = await db.Conexoes.AsNoTracking().SingleAsync(c => c.Id == 13);
        gn.Tipo.Should().Be(Dominio.Integracao.TipoDeConexao.ApiComChave, "o CK_Conexao_Tipo aceita o tipo novo");
    }

    [FatoSeHouverSqlServer]
    public async Task O_banco_recusa_competencia_fora_do_dia_1()
    {
        var semente = Recriar();
        await Sincronizar(semente, CadastroDeMetas(), Agora);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var atualizar = () => db.Database.ExecuteSqlRawAsync("UPDATE organizacao.MetaDeVenda SET Competencia = '2025-11-15' WHERE IdNaOrigem = 1");

        await atualizar.Should().ThrowAsync<SqlException>().WithMessage("*CK_MetaDeVenda_Competencia*");
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var semente = Recriar();

        var relatorio = await Sincronizar(semente, CadastroDeMetas(), Agora, simular: true);
        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeNovas).Should().Be(5);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.MetasDeVenda.CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }
}
