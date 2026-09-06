using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// O <c>ALTER DATABASE ... COLLATE</c> da migração inicial, exercitado no único cenário em
/// que ele faz alguma coisa: um banco que NASCEU com a colação errada.
///
/// POR QUE ESTE ARQUIVO EXISTE — auditoria do documento 21, achado I-2, gravidade "impede".
/// O bloco de <c>ModeloInicial.ColacaoDoBanco.cs</c> é guardado por um <c>IF</c> que compara a
/// colação corrente com <c>Latin1_General_CI_AI</c>. No contêiner de desenvolvimento,
/// <c>MSSQL_COLLATION</c> fixa a colação do SERVIDOR e, por consequência, do <c>model</c>, em
/// <c>Latin1_General_CI_AI</c>; como todo banco novo nasce do <c>model</c>, a condição do
/// <c>IF</c> era SEMPRE FALSA e o <c>ALTER</c> nunca executava. O bloco podia estar quebrado —
/// sintaxe inválida, colação com nome errado, ordem errada — e todos os testes continuavam
/// verdes.
///
/// Em produção é o contrário: o banco nasce dentro da instância que hospeda o Vórtice, cuja
/// colação padrão é <c>SQL_Latin1_General_CP1_CI_AS</c> — *accent-SENSITIVE*. É lá que este
/// bloco é o único mecanismo que existe (documento 20, seções 4.3 e 4.4). Se ele falhar,
/// TODAS as colunas nascem com a colação errada, a busca por nome volta a se comportar como no
/// Vórtice, e <c>UX_CatalogoItem_Catalogo_Descricao</c> deixa de impedir 'Preço' ao lado de
/// 'Preco'. Nada avisa.
///
/// COMO O CENÁRIO É REPRODUZIDO, e por que não mexendo no <c>model</c>: a auditoria sugeriu
/// criar o banco a partir de um <c>model</c> com colação accent-sensitive. O estado que
/// importa é o mesmo — um banco cuja colação, no momento em que a migração começa, é
/// accent-sensitive — e ele se obtém com <c>CREATE DATABASE ... COLLATE</c>, sem alterar o
/// <c>model</c> da instância inteira. Alterar o <c>model</c> exigiria acesso exclusivo a um
/// banco de sistema compartilhado por todos os bancos da máquina e deixaria a instância suja
/// se o teste caísse no meio; o ganho seria nenhum, porque o <c>IF</c> da migração lê a
/// colação do banco corrente, não a origem dela.
///
/// COMO RODAR: igual aos demais deste diretório — suba o contêiner e rode <c>dotnet test</c>.
/// Sem servidor, o teste é IGNORADO com a razão dita em voz alta.
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class ColacaoNoContainerTestes
{
    /// <summary>Banco próprio deste teste, apagado e recriado a cada execução.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmColacaoTeste";

    /// <summary>
    /// A colação do Vórtice, confirmada na investigação de 02/09/2026: ignora caixa, mas
    /// DISTINGUE acento. É a colação padrão da instância onde o banco novo vai nascer.
    /// </summary>
    private const string ColacaoDoVortice = "SQL_Latin1_General_CP1_CI_AS";

    [FatoSeHouverSqlServer]
    public void A_migracao_corrige_a_colacao_do_banco_que_nasceu_com_a_colacao_do_Vortice()
    {
        RecriarBancoDeTesteCom(ColacaoDoVortice);

        ColacaoDoBanco().Should().Be(ColacaoDoVortice,
            "a premissa deste teste é um banco nascido com a colação da instância do Vórtice — " +
            "se ele já nascesse certo, o teste passaria sem exercitar nada, que é exatamente o " +
            "defeito I-2 que ele existe para não repetir");

        using (var contexto = CriarContexto())
        {
            contexto.Database.Migrate();
        }

        ColacaoDoBanco().Should().Be(CrmDbContext.ColacaoSemCaixaNemAcento,
            "o ALTER DATABASE ... COLLATE da migração inicial é o mecanismo que corrige a " +
            "colação herdada da instância (documento 20, seção 4.4). Sem ele, o banco de " +
            "produção nasceria accent-sensitive e ninguém saberia");

        // A ORDEM também é o que se prova aqui: o ALTER roda ANTES do primeiro CreateTable, de
        // modo que as colunas herdam a colação já corrigida. Catalogo.Nome é uma coluna que
        // NÃO declara colação própria — ela só pode ter herdado a do banco.
        ColacaoDaColuna("metadado", "Catalogo", "Nome").Should().Be(
            CrmDbContext.ColacaoSemCaixaNemAcento,
            "coluna sem colação declarada herda a do banco; se o ALTER rodasse depois do " +
            "CreateTable, esta coluna teria nascido com a colação do Vórtice e ficado assim");
    }

    [FatoSeHouverSqlServer]
    public void No_banco_nascido_errado_e_corrigido_a_unicidade_ja_ignora_acento()
    {
        // A prova de COMPORTAMENTO, não de metadado: não basta a propriedade do banco dizer
        // Latin1_General_CI_AI — o índice único precisa de fato recusar dois códigos que só
        // diferem por acento. Sob a colação do Vórtice, os dois entrariam.
        RecriarBancoDeTesteCom(ColacaoDoVortice);

        using var contexto = CriarContexto();
        contexto.Database.Migrate();

        var gravar = () => contexto.Database.ExecuteSqlRaw(@"
            INSERT INTO metadado.Catalogo (Codigo, Nome, PermiteItemNovo, EstaAtivo)
            VALUES ('SEGMENTO', N'Segmento', 1, 1);

            INSERT INTO metadado.Catalogo (Codigo, Nome, PermiteItemNovo, EstaAtivo)
            VALUES ('SÉGMENTO', N'Segmento com acento', 1, 1);");

        gravar.Should().Throw<SqlException>()
            .Which.Number.Should().BeOneOf([2601, 2627],
                "UX_Catalogo_Codigo é um índice sobre uma coluna SEM colação declarada: ela " +
                "herdou a do banco. Se a migração não tivesse corrigido a colação, 'SEGMENTO' " +
                "e 'SÉGMENTO' seriam códigos diferentes e os dois entrariam");
    }

    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// Apaga e recria o banco deste teste com a colação pedida — o que simula o
    /// <c>CREATE DATABASE</c> dentro de uma instância cuja colação padrão não é a nossa.
    /// </summary>
    private static void RecriarBancoDeTesteCom(string colacao)
    {
        // Sem isto, uma conexão ociosa do pool apontando para o banco impede o DROP.
        SqlConnection.ClearAllPools();

        ExecutarNoMaster($@"
            IF DB_ID(N'{NomeDoBancoDeTeste}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{NomeDoBancoDeTeste}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{NomeDoBancoDeTeste}];
            END;");

        ExecutarNoMaster($"CREATE DATABASE [{NomeDoBancoDeTeste}] COLLATE {colacao};");
    }

    /// <summary>A colação do banco de teste, lida de fora dele.</summary>
    private static string ColacaoDoBanco() => ConsultarTextoNoMaster(
        $"SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(N'{NomeDoBancoDeTeste}', 'Collation'));");

    /// <summary>A colação efetiva de uma coluna, lida dentro do banco de teste.</summary>
    private static string ColacaoDaColuna(string schema, string tabela, string coluna)
    {
        using var conexao = new SqlConnection(SemPool(NomeDoBancoDeTeste));
        conexao.Open();
        using var comando = conexao.CreateCommand();
        comando.CommandText = $@"
            SELECT c.collation_name
            FROM sys.columns c
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = N'{schema}' AND t.name = N'{tabela}' AND c.name = N'{coluna}';";
        return (string)comando.ExecuteScalar()!;
    }

    private static void ExecutarNoMaster(string sql)
    {
        using var conexao = AbrirMaster();
        using var comando = conexao.CreateCommand();
        comando.CommandText = sql;
        comando.CommandTimeout = 180;
        comando.ExecuteNonQuery();
    }

    private static string ConsultarTextoNoMaster(string sql)
    {
        using var conexao = AbrirMaster();
        using var comando = conexao.CreateCommand();
        comando.CommandText = sql;
        return (string)comando.ExecuteScalar()!;
    }

    private static SqlConnection AbrirMaster()
    {
        var conexao = new SqlConnection(SemPool("master"));
        conexao.Open();
        return conexao;
    }

    /// <summary>
    /// A conexão deste teste NUNCA usa o pool.
    ///
    /// POR QUÊ: <c>ALTER DATABASE ... COLLATE</c> muda o estado do banco por baixo de uma
    /// conexão viva. Quando o pool tenta reaproveitá-la, o servidor recusa o
    /// <c>sp_reset_connection</c> com o erro 4021 (<i>"Resetting the connection results in a
    /// different state than the initial login"</i>), e o teste falharia por um detalhe de pool,
    /// não pelo que ele mede. É o preço de exercitar de verdade o comando que só roda quando o
    /// banco nasceu com a colação errada; fora deste arquivo, nada muda.
    /// </summary>
    private static string SemPool(string banco) =>
        new SqlConnectionStringBuilder(ConexaoDeSqlServer.MontarPara(banco)) { Pooling = false }
            .ConnectionString;

    private static CrmDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlServer(
                SemPool(NomeDoBancoDeTeste),
                sql => sql
                    .MigrationsHistoryTable("__EFMigrationsHistory", "metadado")
                    .CommandTimeout(180))
            .Options;

        return new CrmDbContext(opcoes, new ProvedorDeSistema());
    }

    /// <summary>Contexto de acesso de sistema — este teste verifica colação, nunca segurança.</summary>
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
