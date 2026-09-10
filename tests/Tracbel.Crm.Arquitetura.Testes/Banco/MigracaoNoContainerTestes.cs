using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Cria o banco DE VERDADE e roda a migração inicial contra o container.
///
/// POR QUE ESTE TESTE EXISTE, se todos os outros deste diretório já verificam o modelo:
/// porque o modelo do EF Core e o banco são duas coisas diferentes, e a diferença só aparece
/// quando o comando chega ao servidor. Coisas que SÓ este teste pega:
///   - restrição de verificação com sintaxe inválida no SQL Server;
///   - filtro de índice com nome de coluna errado, ou com construção que o SQL Server recusa
///     em índice filtrado (OR, função);
///   - colação que o servidor não conhece;
///   - identificador acima de 128 caracteres;
///   - o SQL bruto do particionamento, que nenhum teste de modelo enxerga;
///   - o comportamento de unicidade sem caixa nem acento, e o de concorrência otimista, que
///     só existem quando o motor executa.
///
/// COMO RODAR: suba o container (<c>./scripts/banco/subir-banco.ps1</c>) e rode
/// <c>dotnet test</c>. Sem container, o teste é IGNORADO com a razão dita em voz alta — nunca
/// falha por falta de infraestrutura, e nunca passa em silêncio fingindo que rodou.
///
/// PARA EXCLUIR NO CI: <c>dotnet test --filter "Categoria!=BancoReal"</c>.
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class MigracaoNoContainerTestes
{
    /// <summary>
    /// Banco PRÓPRIO deste teste, apagado e recriado a cada execução. Nunca o de
    /// desenvolvimento: rodar o teste não pode custar o dado com que alguém está trabalhando.
    /// </summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmMigracaoTeste";

    [FatoSeHouverSqlServer]
    public void A_migracao_inicial_cria_os_dez_schemas_e_as_sessenta_e_oito_tabelas()
    {
        using var contexto = CriarContexto();

        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        var porSchema = ConsultarContagemPorSchema(contexto);

        porSchema.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["organizacao"] = 8,
            ["seguranca"] = 8,
            ["comercial"] = 11,
            ["processo"] = 14,
            ["frota"] = 5,
            ["documento"] = 2,
            ["auditoria"] = 3,
            ["integracao"] = 6,
            ["metadado"] = 8,
            ["relatorio"] = 3
        }, "é a conta do documento 14, seção 2.1 — 68 tabelas em 10 schemas, no banco de " +
           "verdade: as 63 do documento 17, mais as duas de município do documento 26, mais " +
           "processo.VendaPerdida, comercial.FaturamentoDoCliente e " +
           "comercial.FaturamentoSemCliente (documento 31)");

        porSchema.Values.Sum().Should().Be(68);
    }

    [FatoSeHouverSqlServer]
    public void Nenhuma_tabela_do_modelo_nasce_no_schema_dbo()
    {
        using var contexto = CriarContexto();
        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        // A tabela de controle de migração do EF Core é configurada para o schema metadado
        // justamente por causa desta regra (documento 14, seção 2). Se alguém tirar essa
        // configuração, ela reaparece em dbo e este teste falha.
        var emDbo = ConsultarTextos(contexto, @"
            SELECT t.name
            FROM sys.tables t
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = 'dbo'");

        emDbo.Should().BeEmpty(
            "nada do modelo mora em dbo (documento 14, seção 2) — é literalmente o schema onde " +
            "as 767 tabelas do Vórtice se amontoam. Encontrado: {0}",
            string.Join(", ", emDbo));
    }

    [FatoSeHouverSqlServer]
    public void As_quatro_tabelas_de_log_e_auditoria_estao_particionadas_por_data()
    {
        using var contexto = CriarContexto();
        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        // Uma tabela está particionada quando o índice CLUSTERIZADO dela mora num esquema de
        // partição, e não num filegroup. O SQL bruto da migração é a única coisa que faz isso
        // — nenhum teste de modelo consegue verificar.
        var particionadas = ConsultarTextos(contexto, @"
            SELECT s.name + '.' + t.name
            FROM sys.tables t
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            JOIN sys.indexes i ON i.object_id = t.object_id AND i.index_id IN (0, 1)
            JOIN sys.data_spaces ds ON ds.data_space_id = i.data_space_id
            WHERE ds.type = 'PS'
            ORDER BY 1");

        particionadas.Should().BeEquivalentTo(new[]
        {
            "auditoria.AlteracaoDeCampo",
            "auditoria.EventoDeAcesso",
            "integracao.Recepcao",
            "processo.RegraExecucao"
        }, "são as quatro tabelas de log e de área de pouso, particionadas por mês. [V] é a " +
           "lição das 22 tabelas de log do Vórtice, com 43,7 milhões de linhas e nenhuma " +
           "política de retenção, e das 76 de staging permanente");

        foreach (var tabela in particionadas)
        {
            var partes = tabela.Split('.');

            var quantidade = ConsultarInteiro(contexto, $@"
                SELECT COUNT(*)
                FROM sys.partitions p
                JOIN sys.tables t ON t.object_id = p.object_id
                JOIN sys.schemas s ON s.schema_id = t.schema_id
                WHERE s.name = '{partes[0]}' AND t.name = '{partes[1]}' AND p.index_id IN (0, 1)");

            quantidade.Should().BeGreaterThan(12, $"{tabela} precisa de uma partição por mês");

            // Ao contrário do PostgreSQL, aqui a partição de escape não precisa ser criada:
            // a função de partição do SQL Server cobre o domínio INTEIRO por construção — há
            // sempre uma partição à esquerda do primeiro limite e outra à direita do último.
            // Nenhuma linha é recusada por cair fora da faixa, nem se o job mensal falhar.
            var todosOsIndicesAlinhados = ConsultarInteiro(contexto, $@"
                SELECT COUNT(*)
                FROM sys.indexes i
                JOIN sys.tables t ON t.object_id = i.object_id
                JOIN sys.schemas s ON s.schema_id = t.schema_id
                JOIN sys.data_spaces ds ON ds.data_space_id = i.data_space_id
                WHERE s.name = '{partes[0]}' AND t.name = '{partes[1]}'
                  AND i.index_id > 0 AND ds.type <> 'PS'");

            todosOsIndicesAlinhados.Should().Be(0,
                $"todo índice de {tabela} precisa estar ALINHADO com a partição — índice " +
                "desalinhado impede SWITCH e TRUNCATE por partição, que são o motivo de " +
                "particionar");
        }
    }

    [FatoSeHouverSqlServer]
    public void Toda_coluna_de_data_e_hora_do_banco_e_datetime2_de_milissegundos()
    {
        using var contexto = CriarContexto();
        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        // [V] o Vórtice usa `datetime`, com precisão irregular de ~3,33 ms e faixa curta.
        // Aqui, no banco de verdade, nenhuma coluna pode ser datetime nem smalldatetime.
        var foraDoPadrao = ConsultarTextos(contexto, @"
            SELECT s.name + '.' + t.name + '.' + c.name + ' (' + ty.name + ')'
            FROM sys.columns c
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            WHERE ty.name IN ('datetime', 'smalldatetime')
            ORDER BY 1");

        foraDoPadrao.Should().BeEmpty(
            "toda coluna de data e hora é datetime2(3). Fora do padrão: {0}",
            string.Join(", ", foraDoPadrao));
    }

    [FatoSeHouverSqlServer]
    public void A_restricao_de_dominio_recusa_o_valor_invalido_no_banco()
    {
        using var contexto = CriarContexto();
        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        // O catálogo CULTURA não é criado aqui: ele nasce com a migração, porque é um dos oito
        // catálogos de SISTEMA cujo identificador o próprio esquema cita (ver
        // Dominio.Metadado.CatalogosDeSistema). Criá-lo de novo aqui faria este teste passar
        // pelo motivo errado — a violação seria de UX_Catalogo_Codigo, não da unicidade de
        // rótulo que ele diz medir.
        //
        // [V] O Vórtice tem ZERO restrições de verificação em 767 tabelas, e é por isso que
        // FINALIZADO (18.416 linhas) convive com FINALIZADA (11.563) e 437.694 processos estão
        // com o status em branco. Este teste prova que, aqui, o banco recusa — não a tela.
        var gravar = () => contexto.Database.ExecuteSqlRaw(
            InserirNoCatalogoDeCultura("SOJA", "Soja", 1) +
            InserirNoCatalogoDeCultura("SOJA2", "SOJA", 2));

        // 'Soja' e 'SOJA' são o MESMO rótulo sob a colação sem caixa: o índice único do
        // catálogo recusa o segundo. É a garantia estrutural que mata a classe de defeito
        // FINALIZADO/FINALIZADA.
        gravar.Should().Throw<SqlException>()
            .Which.Number.Should().BeOneOf([2601, 2627],
                "dois rótulos que só diferem em CAIXA são o MESMO item de catálogo: é o índice " +
                "único sobre a coluna com colação Latin1_General_CI_AI que recusa o segundo. " +
                "Sem isso, volta a ser possível ter 'PRECO' e 'Preco' na mesma lista — que é " +
                "como FINALIZADO e FINALIZADA convivem no Vórtice");
    }

    [FatoSeHouverSqlServer]
    public void A_restricao_de_dominio_recusa_o_rotulo_que_so_difere_por_acento()
    {
        using var contexto = CriarContexto();
        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        // POR QUE ESTE TESTE É SEPARADO DO DE CAIXA, e por que ele é o que importa: a colação
        // do Vórtice (SQL_Latin1_General_CP1_CI_AS) TAMBÉM ignora caixa — ela recusaria
        // 'Soja'/'SOJA' exatamente igual. A propriedade que a decisão 20, seção 4.3, comprou
        // ao trocar de colação é o AI: ignorar ACENTO, que é o que faz "Jose" encontrar "José"
        // e o que impede 'Preço' de conviver com 'Preco' no mesmo catálogo.
        //
        // Até a auditoria do documento 21 (achado I-2) essa propriedade não era exercitada em
        // lugar nenhum do conjunto de testes: o teste de caixa passaria igual sob a colação
        // errada. Este aqui não passa.
        var gravar = () => contexto.Database.ExecuteSqlRaw(
            InserirNoCatalogoDeCultura("SOJA", "Soja", 1) +
            InserirNoCatalogoDeCultura("SOJA_ACENTO", "Sója", 2));

        gravar.Should().Throw<SqlException>()
            .Which.Number.Should().BeOneOf([2601, 2627],
                "'Soja' e 'Sója' são o MESMO rótulo sob Latin1_General_CI_AI, e o índice único " +
                "UX_CatalogoItem_Catalogo_Descricao recusa o segundo. Sob a colação do Vórtice " +
                "(accent-SENSITIVE) os dois entrariam, e o catálogo voltaria a ter duplicata " +
                "semântica — a classe de defeito medida em 30 mil linhas do legado");
    }

    /// <summary>
    /// Um <c>INSERT</c> no catálogo de sistema <c>CULTURA</c>, que a migração já semeou.
    /// A descrição vai como literal <c>nvarchar</c> (<c>N'...'</c>) porque é onde o acento
    /// deste teste mora — sem o <c>N</c>, o servidor converteria para a página de código e o
    /// teste mediria outra coisa.
    /// </summary>
    private static string InserirNoCatalogoDeCultura(string codigo, string descricao, int ordem) => $@"
        INSERT INTO metadado.CatalogoItem (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
        VALUES ({CatalogosDeSistema.Cultura}, '{codigo}', N'{descricao}', {ordem}, 0, 1);
";

    [FatoSeHouverSqlServer]
    public void A_concorrencia_otimista_recusa_a_segunda_gravacao_em_cima_da_primeira()
    {
        using var contexto = CriarContexto();
        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        // Uma filial e um usuário, gravados por fora do EF para não depender de construtor de
        // domínio. O usuário é quem herda EntidadeBase — logo, quem tem a coluna Versao.
        contexto.Database.ExecuteSqlRaw(@"
            SET IDENTITY_INSERT organizacao.Empresa ON;
            INSERT INTO organizacao.Empresa (Id, Codigo, Nome, Caminho, Nivel, EstaAtiva, CriadoEm)
            VALUES (1, 'MATRIZ', 'Tracbel Agro', '/1/', 0, 1, SYSUTCDATETIME());
            SET IDENTITY_INSERT organizacao.Empresa OFF;

            SET IDENTITY_INSERT seguranca.Usuario ON;
            INSERT INTO seguranca.Usuario
                (Id, EmpresaId, IdentidadeExterna, NomePrincipal, NomeCompleto, NomeExibicao,
                 Email, EstaAtivo, CriadoEm, CriadoPorId)
            VALUES (1, 1, NEWID(), 'ana@tracbel.com.br', 'Ana Souza', 'Ana',
                    'ana@tracbel.com.br', 1, SYSUTCDATETIME(), 0);
            SET IDENTITY_INSERT seguranca.Usuario OFF;");

        // Duas pessoas leem a MESMA linha, cada uma no seu contexto.
        using var primeira = CriarContexto();
        using var segunda = CriarContexto();

        var visaoDaPrimeira = primeira.Usuarios.Single(u => u.Id == 1);
        var visaoDaSegunda = segunda.Usuarios.Single(u => u.Id == 1);

        visaoDaPrimeira.Versao.Should().NotBeNull(
            "a coluna rowversion é preenchida pelo banco e vem no SELECT");

        // A primeira salva. O rowversion da linha muda no banco.
        primeira.Entry(visaoDaPrimeira).Property(nameof(Dominio.Comum.EntidadeBase.AlteradoEm))
            .CurrentValue = DateTime.UtcNow;
        primeira.SaveChanges();

        // A segunda tenta salvar por cima, ainda com o rowversion antigo em mãos.
        segunda.Entry(visaoDaSegunda).Property(nameof(Dominio.Comum.EntidadeBase.AlteradoEm))
            .CurrentValue = DateTime.UtcNow.AddSeconds(1);

        var gravarPorCima = () => segunda.SaveChanges();

        gravarPorCima.Should().Throw<DbUpdateConcurrencyException>(
            "quem salva em cima do trabalho de outra pessoa recebe erro, nunca a sobrescrita " +
            "silenciosa (documento 14, seção 5.1). É o rowversion que o banco incrementa a " +
            "cada gravação que torna isso possível");
    }

    // -----------------------------------------------------------------------------------------

    private static CrmDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlServer(
                ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste),
                sql => sql
                    .MigrationsHistoryTable("__EFMigrationsHistory", "metadado")
                    .CommandTimeout(180))
            .Options;

        return new CrmDbContext(opcoes, new ProvedorDeSistema());
    }

    private static Dictionary<string, int> ConsultarContagemPorSchema(CrmDbContext contexto)
    {
        // Conta a tabela LÓGICA: a tabela particionada continua sendo UMA linha em sys.tables
        // (a partição é física, não é conceito de negócio), e a tabela de controle de migração
        // do EF Core fica de fora.
        AbrirSePreciso(contexto);
        using var comando = contexto.Database.GetDbConnection().CreateCommand();
        comando.CommandText = @"
            SELECT s.name, COUNT(*)
            FROM sys.tables t
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE t.name <> '__EFMigrationsHistory'
              AND s.name NOT IN ('dbo', 'sys', 'guest', 'INFORMATION_SCHEMA')
            GROUP BY s.name";

        var resultado = new Dictionary<string, int>();
        using var leitor = comando.ExecuteReader();
        while (leitor.Read()) resultado[leitor.GetString(0)] = leitor.GetInt32(1);
        return resultado;
    }

    private static List<string> ConsultarTextos(CrmDbContext contexto, string sql)
    {
        AbrirSePreciso(contexto);
        using var comando = contexto.Database.GetDbConnection().CreateCommand();
        comando.CommandText = sql;

        var resultado = new List<string>();
        using var leitor = comando.ExecuteReader();
        while (leitor.Read()) resultado.Add(leitor.GetString(0));
        return resultado;
    }

    private static int ConsultarInteiro(CrmDbContext contexto, string sql)
    {
        AbrirSePreciso(contexto);
        using var comando = contexto.Database.GetDbConnection().CreateCommand();
        comando.CommandText = sql;
        return Convert.ToInt32(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void AbrirSePreciso(CrmDbContext contexto)
    {
        var conexao = contexto.Database.GetDbConnection();
        if (conexao.State != System.Data.ConnectionState.Open) conexao.Open();
    }

    /// <summary>Contexto de acesso de sistema — o teste verifica esquema, nunca segurança por linha.</summary>
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

/// <summary>
/// Marca um teste que só faz sentido com um SQL Server de verdade respondendo.
///
/// Sem servidor, o teste é IGNORADO com a razão escrita — nunca falha por falta de
/// infraestrutura e nunca passa em silêncio fingindo que rodou. É a diferença entre "o CI não
/// tem Docker" e "o banco está quebrado", que precisa ser legível no relatório do CI.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FatoSeHouverSqlServerAttribute : FactAttribute
{
    /// <summary>Decide, uma única vez por execução, se há servidor respondendo.</summary>
    public FatoSeHouverSqlServerAttribute()
    {
        if (ConexaoDeSqlServer.Indisponivel is { } motivo)
            Skip = motivo;
    }
}

/// <summary>
/// Descobre COMO chegar ao SQL Server de desenvolvimento, na ordem: a variável de ambiente
/// que os scripts exportam, depois <c>infra/.env</c>, depois o padrão.
/// </summary>
internal static class ConexaoDeSqlServer
{
    private static readonly Lazy<string?> Diagnostico = new(Verificar);

    /// <summary>Nulo quando há servidor respondendo; a razão do salto quando não há.</summary>
    public static string? Indisponivel => Diagnostico.Value;

    /// <summary>Monta a string de conexão para um banco específico.</summary>
    public static string MontarPara(string banco)
    {
        var construtor = new SqlConnectionStringBuilder(LerConexaoBase()) { InitialCatalog = banco };
        return construtor.ConnectionString;
    }

    private static string LerConexaoBase()
    {
        var doAmbiente = Environment.GetEnvironmentVariable("ConnectionStrings__Crm");
        if (!string.IsNullOrWhiteSpace(doAmbiente)) return doAmbiente;

        var config = LerArquivoDeAmbiente();
        var usuario = config.GetValueOrDefault("DB_USUARIO", "TracbelCrm");
        var banco = config.GetValueOrDefault("DB_NOME", "TracbelCrm");
        var porta = config.GetValueOrDefault("DB_PORTA", "1433");
        var senha = config.GetValueOrDefault("DB_SENHA_APLICACAO", "");

        return $"Server=localhost,{porta};Database={banco};User Id={usuario};Password={senha};" +
               "TrustServerCertificate=True";
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
            // Conecta ao banco de sistema: o banco do teste ainda nem existe.
            var construtor = new SqlConnectionStringBuilder(LerConexaoBase())
            {
                InitialCatalog = "master",
                ConnectTimeout = 5
            };

            using var conexao = new SqlConnection(construtor.ConnectionString);
            conexao.Open();
            return null;
        }
        catch (Exception ex)
        {
            return "Nenhum SQL Server respondendo. Suba o container com " +
                   "'./scripts/banco/subir-banco.ps1' e rode de novo. " +
                   $"Motivo: {ex.GetType().Name}: {ex.Message}";
        }
    }
}
