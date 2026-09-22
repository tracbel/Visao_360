using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
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
    public void A_cadeia_de_migracoes_cria_os_oito_schemas_e_as_cinquenta_e_cinco_tabelas()
    {
        using var contexto = CriarContexto();

        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        var porSchema = ConsultarContagemPorSchema(contexto);

        porSchema.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["organizacao"] = 23,
            ["seguranca"] = 4,
            ["comercial"] = 8,
            ["processo"] = 9,
            ["frota"] = 7,
            ["auditoria"] = 1,
            ["integracao"] = 13,
            ["metadado"] = 2
        }, "é a conta do documento 14, seção 2.1 — 67 tabelas de modelo em 8 schemas, no banco de " +
           "verdade. A migração inicial criava 80 em 10; a fase 1 do documento 41 removeu as 31 " +
           "que nunca receberam uma linha e esvaziou por completo os schemas 'documento' e " +
           "'relatorio'; a issue 64 acrescentou o total do estado, a 65 as cinco da estrutura " +
           "agropecuária a 66 as duas dos preços de mercado a 67 a dos custos de produção a 68 as duas do crédito rural do SICOR a 71 as duas dos parâmetros do potencial com vigência e a 154 o de-para de município por fonte. A cadeia inteira roda aqui, do zero: é o que prova que a remoção — e o " +
           "RENAME da tabela da PAM, que preserva a área plantada já carregada — também funcionam " +
           "em banco que nasce agora");

        porSchema.Values.Sum().Should().Be(67);
    }

    [FatoSeHouverSqlServer]
    public void A_regra_de_potencial_existe_depois_das_migracoes_e_repor_duas_vezes_nao_duplica()
    {
        // POR QUE ESTE TESTE EXISTE (issue 98). A regra é semente: nasce por `HasData` e a migração
        // de 13/09/2026 a inseriu. A sanitização de 15/09 a apagou junto com a configuração herdada
        // do Vórtice — e, como a migração já estava aplicada, o `HasData` não repõe nada. O servidor
        // ficou com 54.570 linhas de área plantada e nenhuma regra: o mapa C não calculava.
        //
        // A migração `ReporSementeDaRegraDePotencial` repõe com `IF NOT EXISTS`. Rodá-la de novo num
        // banco que já tem a linha não pode duplicar — é o que a segunda metade do teste prova.
        using var contexto = CriarContexto();

        contexto.Database.EnsureDeleted();
        contexto.Database.Migrate();

        var regras = contexto.RegrasDePotencial.Where(r => r.RevogadoEm == null).ToList();

        regras.Should().HaveCount(1,
            "sem regra vigente, o mapa C da Visão 360 mostra 'sem regra de potencial' e não calcula nada");
        regras[0].ProdutoCodigoIbge.Should().Be(40139, "é o café (em grão) total da classificação 782");
        regras[0].HectaresPorMaquina.Should().Be(10m);
        regras[0].ModeloDeReferencia.Should().Be("3036N");
        regras[0].Situacao.Should().Be(SituacaoDaRegraDePotencial.AConfirmar,
            "a regra veio de uma frase do gerente comercial e ninguém a confirmou — a tela diz isso");

        // ISSUE 71: a mesma linha virou a primeira vigência — a data de 13/09 preservada pelo RENAME de
        // InformadaEm, o texto de origem pelo RENAME de Origem, e sem autor, porque veio da migração.
        regras[0].VigenteDesde.Should().Be(new DateOnly(2026, 9, 13));
        regras[0].Justificativa.Should().StartWith("Exemplo do gerente comercial");
        regras[0].InformadoPorId.Should().BeNull();

        var geral = contexto.ParametrosDoPotencial.Single();
        geral.MesesDaJanela.Should().Be(12, "é o '12 contra 12' do texto de 21/09");
        geral.PesoDosContratosNoCredito.Should().Be(0.700m);
        geral.LimiteDaPercepcao.Should().Be(5.00m);
        geral.PesoDoIndicadorDePreco.Should().BeNull("os pesos estão em aberto (D-P05)");

        // A mesma reposição, de novo, num banco que já tem a linha — agora com as colunas da vigência.
        contexto.Database.ExecuteSql(
            $"""
             IF NOT EXISTS (SELECT 1 FROM organizacao.RegraDePotencial WHERE Id = 1)
             BEGIN
                 SET IDENTITY_INSERT organizacao.RegraDePotencial ON;
                 INSERT INTO organizacao.RegraDePotencial
                     (Id, ProdutoCodigoIbge, ProdutoNome, HectaresPorMaquina, ModeloDeReferencia,
                      Situacao, Justificativa, VigenteDesde, InformadoEm)
                 VALUES (1, 40139, N'Café (em grão) Total', 10.00, N'3036N', 'AConfirmar', N'reexecução', '2026-09-13', '2026-09-13');
                 SET IDENTITY_INSERT organizacao.RegraDePotencial OFF;
             END
             """);

        contexto.RegrasDePotencial.Count().Should().Be(1, "repor duas vezes não duplica");
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
    public void A_tabela_de_auditoria_esta_particionada_por_data()
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
            "auditoria.AlteracaoDeCampo"
        }, "é a tabela de log que o sistema de fato escreve, particionada por mês. [V] é a " +
           "lição das 22 tabelas de log do Vórtice, com 43,7 milhões de linhas e nenhuma " +
           "política de retenção, e das 76 de staging permanente. As outras três — " +
           "auditoria.EventoDeAcesso, integracao.Recepcao e processo.RegraExecucao — saíram " +
           "vazias na fase 1 (documento 41), e a fase 1 também derruba a função e o esquema de " +
           "partição de cada uma: partição órfã sobrevive ao DROP TABLE");

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
        var gravar = () =>
        {
            InserirNoCatalogoDeCultura(contexto, "SOJA", "Soja", 1);
            InserirNoCatalogoDeCultura(contexto, "SOJA2", "SOJA", 2);
        };

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
        var gravar = () =>
        {
            InserirNoCatalogoDeCultura(contexto, "SOJA", "Soja", 1);
            InserirNoCatalogoDeCultura(contexto, "SOJA_ACENTO", "Sója", 2);
        };

        gravar.Should().Throw<SqlException>()
            .Which.Number.Should().BeOneOf([2601, 2627],
                "'Soja' e 'Sója' são o MESMO rótulo sob Latin1_General_CI_AI, e o índice único " +
                "UX_CatalogoItem_Catalogo_Descricao recusa o segundo. Sob a colação do Vórtice " +
                "(accent-SENSITIVE) os dois entrariam, e o catálogo voltaria a ter duplicata " +
                "semântica — a classe de defeito medida em 30 mil linhas do legado");
    }

    /// <summary>
    /// Um <c>INSERT</c> no catálogo de sistema <c>CULTURA</c>, que a migração já semeou.
    ///
    /// <para><b>SQL parametrizado</b> (<c>ExecuteSql</c>, e não <c>ExecuteSqlRaw</c> com texto
    /// montado): o analisador EF1003, que chegou com o EF Core 10, recusa concatenação em SQL cru —
    /// e com razão. O parâmetro também resolve melhor o que este teste mede: o cliente manda texto
    /// como <c>nvarchar</c>, então o acento de "Sója" chega intacto sem depender do prefixo
    /// <c>N'...'</c> no literal.</para>
    /// </summary>
    private static int InserirNoCatalogoDeCultura(CrmDbContext contexto, string codigo, string descricao, int ordem) =>
        contexto.Database.ExecuteSql(
            $@"INSERT INTO metadado.CatalogoItem (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
               VALUES ({CatalogosDeSistema.Cultura}, {codigo}, {descricao}, {ordem}, 0, 1);");

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
