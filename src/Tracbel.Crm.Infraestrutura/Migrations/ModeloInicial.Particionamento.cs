using Microsoft.EntityFrameworkCore.Migrations;

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// PARTICIONAMENTO POR DATA — a parte da migração inicial que é escrita à mão.
    ///
    /// Fica num arquivo À PARTE, e não dentro do arquivo gerado pelo EF Core, por um motivo
    /// prático: regenerar a migração (<c>dotnet ef migrations remove</c> seguido de
    /// <c>add</c>) reescreve o arquivo gerado do zero e levaria este bloco junto. Aqui ele
    /// sobrevive, e o que precisa ser refeito na regeneração é UMA LINHA no fim do
    /// <c>Up(...)</c> do arquivo gerado:
    ///
    ///     ParticionarTabelasDeLogEAuditoria(migrationBuilder);
    ///
    /// POR QUE É SQL BRUTO: o EF Core não tem API de particionamento. O <c>CreateTable</c>
    /// gerado cria as quatro tabelas no filegroup padrão; este bloco cria a FUNÇÃO e o ESQUEMA
    /// de partição de cada uma e move a tabela para o esquema, recriando o índice
    /// CLUSTERIZADO (a chave primária) e todos os índices não clusterizados sobre ele — que é
    /// como o SQL Server particiona uma tabela. Nenhuma delas tem dado neste momento: a
    /// conversão é segura porque acontece na mesma migração que as criou.
    ///
    /// EDIÇÃO: particionamento de tabela deixou de ser recurso exclusivo da Enterprise no
    /// SQL Server 2016 SP1 — a edição STANDARD, que é a da instância de produção, suporta
    /// função de partição, esquema de partição, índice alinhado, <c>SWITCH</c> e
    /// <c>TRUNCATE ... WITH (PARTITIONS ...)</c>. Nada aqui depende de Enterprise.
    ///
    /// POR QUE PARTICIONAR ESTAS QUATRO — é a lição mais cara do Vórtice:
    ///   - 22 tabelas de log clonadas por assunto somam 43.673.105 linhas, 42% do banco, sem
    ///     NENHUMA política de retenção; existe um job de compactação cadastrado e nunca
    ///     agendado;
    ///   - 76 tabelas de staging permanentes guardam 19,4 milhões de linhas, entre elas
    ///     783.242 títulos (R$ 5,18 bi) presos desde maio de 2025.
    /// Com partição mensal, expurgar o que passou da retenção é
    /// <c>TRUNCATE TABLE ... WITH (PARTITIONS (n))</c> ou um <c>SWITCH</c> para tabela de
    /// descarte: instantâneo, sem travar a tabela viva e sem inchar o log de transação. Sem
    /// partição seria um <c>DELETE</c> de dezenas de milhões de linhas que ninguém tem coragem
    /// de rodar — e foi exatamente por isso que, no Vórtice, nunca se rodou.
    ///
    /// A CHAVE PRIMÁRIA das quatro inclui a coluna de data: o SQL Server exige que a coluna de
    /// particionamento faça parte da chave de todo índice ÚNICO da tabela particionada. É a
    /// única exceção à regra "a chave primária é a coluna Id", e está registrada como tal em
    /// <c>EsquemaENomenclaturaTestes.TabelasParticionadasPorData</c>.
    ///
    /// RETENÇÃO DECLARADA (documento 14, seção 10):
    ///   <c>auditoria.AlteracaoDeCampo</c>  18 meses disponíveis, arquivamento depois
    ///   <c>auditoria.EventoDeAcesso</c>    24 meses (exigência de LGPD)
    ///   <c>processo.RegraExecucao</c>      12 meses disponíveis, arquivamento depois
    ///   <c>integracao.Recepcao</c>         expurgo pela coluna <c>ExpurgarApos</c>
    ///
    /// JANELA CRIADA AQUI: do terceiro mês anterior ao décimo segundo mês seguinte. Os três
    /// meses para trás existem para a carga do dado histórico do Vórtice. Criar os limites
    /// seguintes é trabalho de job mensal (<c>ALTER PARTITION FUNCTION ... SPLIT RANGE</c>) —
    /// e, ao contrário do PostgreSQL, aqui NÃO existe risco de linha recusada se o job falhar:
    /// a função de partição do SQL Server cobre o domínio inteiro por construção, com uma
    /// partição à esquerda do primeiro limite e outra à direita do último. É o equivalente
    /// estrutural da partição de escape, sem precisar criá-la.
    /// </summary>
    public partial class ModeloInicial
    {
        private static readonly string[] TabelasParticionadas =
            ["AlteracaoDeCampo", "EventoDeAcesso", "RegraExecucao", "Recepcao"];

        /// <summary>
        /// Desfaz o particionamento — chamado no <c>Down(...)</c>, DEPOIS que as tabelas caem.
        ///
        /// Função e esquema de partição são objetos de banco independentes da tabela: o
        /// <c>DROP TABLE</c> não os leva junto. Sem esta limpeza, reaplicar a migração falharia
        /// em <c>CREATE PARTITION FUNCTION</c> ("já existe") — e a reversão só seria reversível
        /// uma vez, que é o mesmo que não ser reversível.
        /// </summary>
        private static void DesfazerParticionamento(MigrationBuilder migrationBuilder)
        {
            foreach (var tabela in TabelasParticionadas)
            {
                migrationBuilder.Sql(
                    $@"IF EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_Mensal_{tabela}')
    DROP PARTITION SCHEME [PS_Mensal_{tabela}];
IF EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_Mensal_{tabela}')
    DROP PARTITION FUNCTION [PF_Mensal_{tabela}];");
            }
        }

        /// <summary>Converte as quatro tabelas em particionadas por mês.</summary>
        private static void ParticionarTabelasDeLogEAuditoria(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET NOCOUNT ON;

DECLARE @alvos TABLE (Esquema sysname, Tabela sysname, Coluna sysname);
INSERT INTO @alvos (Esquema, Tabela, Coluna) VALUES
    (N'auditoria',  N'AlteracaoDeCampo', N'AlteradoEm'),
    (N'auditoria',  N'EventoDeAcesso',   N'OcorreuEm'),
    (N'processo',   N'RegraExecucao',    N'ExecutadoEm'),
    (N'integracao', N'Recepcao',         N'RecebidaEm');

-- Limites mensais: do terceiro mes anterior ao decimo segundo mes seguinte.
-- RANGE RIGHT: o limite pertence a particao da DIREITA, entao cada particao e exatamente
-- [primeiro instante do mes, primeiro instante do mes seguinte).
DECLARE @inicio datetime2(3) = DATEADD(month, -3, DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1));
DECLARE @limites nvarchar(max) = N'';
DECLARE @i int = 0;
WHILE @i <= 15
BEGIN
    SET @limites = @limites + CASE WHEN @i = 0 THEN N'' ELSE N', ' END
                 + N'N''' + CONVERT(nvarchar(23), DATEADD(month, @i, @inicio), 126) + N'''';
    SET @i = @i + 1;
END;

-- Todas as variaveis sao declaradas AQUI, fora do laco: no T-SQL o DECLARE tem escopo de lote,
-- e declarar dentro do WHILE so confunde quem le.
DECLARE @esquema sysname, @tabela sysname, @coluna sysname, @sql nvarchar(max);
DECLARE @pf sysname, @ps sysname, @pk sysname, @colunasPk nvarchar(max);
DECLARE @indice sysname, @comando nvarchar(max), @objeto int;
DECLARE @recriar TABLE (Ordem int IDENTITY(1,1), Comando nvarchar(max));

DECLARE alvo CURSOR LOCAL FAST_FORWARD FOR SELECT Esquema, Tabela, Coluna FROM @alvos;
OPEN alvo;
FETCH NEXT FROM alvo INTO @esquema, @tabela, @coluna;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @objeto = OBJECT_ID(QUOTENAME(@esquema) + N'.' + QUOTENAME(@tabela));
    SET @pf = N'PF_Mensal_' + @tabela;
    SET @ps = N'PS_Mensal_' + @tabela;

    SET @sql = N'CREATE PARTITION FUNCTION ' + QUOTENAME(@pf)
             + N' (datetime2(3)) AS RANGE RIGHT FOR VALUES (' + @limites + N');';
    EXEC sys.sp_executesql @sql;

    -- ALL TO [PRIMARY]: um filegroup so. Separar filegroup por particao e decisao de
    -- armazenamento da infra, nao do modelo — e a instancia de producao e compartilhada.
    SET @sql = N'CREATE PARTITION SCHEME ' + QUOTENAME(@ps)
             + N' AS PARTITION ' + QUOTENAME(@pf) + N' ALL TO ([PRIMARY]);';
    EXEC sys.sp_executesql @sql;

    -- Guarda a definicao dos indices NAO clusterizados antes de derruba-los: eles precisam ser
    -- recriados SOBRE o esquema de particao, ou ficariam desalinhados — e indice desalinhado
    -- impede SWITCH e TRUNCATE por particao, que sao justamente o motivo de particionar.
    DELETE FROM @recriar;

    INSERT INTO @recriar (Comando)
    SELECT N'CREATE ' + CASE WHEN i.is_unique = 1 THEN N'UNIQUE ' ELSE N'' END
         + N'NONCLUSTERED INDEX ' + QUOTENAME(i.name)
         + N' ON ' + QUOTENAME(@esquema) + N'.' + QUOTENAME(@tabela) + N' ('
         + STUFF((SELECT N', ' + QUOTENAME(c.name) + CASE WHEN ic.is_descending_key = 1 THEN N' DESC' ELSE N' ASC' END
                  FROM sys.index_columns ic
                  JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                  WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
                  ORDER BY ic.key_ordinal
                  FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'')
         + N')'
         + ISNULL(N' INCLUDE ('
             + STUFF((SELECT N', ' + QUOTENAME(c.name)
                      FROM sys.index_columns ic
                      JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                      WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
                      ORDER BY ic.index_column_id
                      FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'') + N')', N'')
         + ISNULL(N' WHERE ' + i.filter_definition, N'')
         + N' ON ' + QUOTENAME(@ps) + N'(' + QUOTENAME(@coluna) + N');'
    FROM sys.indexes i
    WHERE i.object_id = @objeto
      AND i.type_desc = N'NONCLUSTERED'
      AND i.is_primary_key = 0
      AND i.is_unique_constraint = 0;

    DECLARE derrubar CURSOR LOCAL FAST_FORWARD FOR
        SELECT i.name
        FROM sys.indexes i
        WHERE i.object_id = @objeto
          AND i.type_desc = N'NONCLUSTERED'
          AND i.is_primary_key = 0
          AND i.is_unique_constraint = 0;
    OPEN derrubar;
    FETCH NEXT FROM derrubar INTO @indice;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'DROP INDEX ' + QUOTENAME(@indice) + N' ON '
                 + QUOTENAME(@esquema) + N'.' + QUOTENAME(@tabela) + N';';
        EXEC sys.sp_executesql @sql;
        FETCH NEXT FROM derrubar INTO @indice;
    END;
    CLOSE derrubar; DEALLOCATE derrubar;

    -- A chave primaria e o indice CLUSTERIZADO: recria-la sobre o esquema de particao e o que
    -- efetivamente move a tabela. A chave ja inclui a coluna de data, porque o SQL Server
    -- exige a coluna de particionamento em todo indice unico da tabela particionada.
    SET @pk = (SELECT i.name FROM sys.indexes i WHERE i.object_id = @objeto AND i.is_primary_key = 1);

    SET @colunasPk =
        STUFF((SELECT N', ' + QUOTENAME(c.name)
                    + CASE WHEN ic.is_descending_key = 1 THEN N' DESC' ELSE N' ASC' END
               FROM sys.index_columns ic
               JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
               JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
               WHERE i.object_id = @objeto AND i.is_primary_key = 1
               ORDER BY ic.key_ordinal
               FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'');

    SET @sql = N'ALTER TABLE ' + QUOTENAME(@esquema) + N'.' + QUOTENAME(@tabela)
             + N' DROP CONSTRAINT ' + QUOTENAME(@pk) + N';';
    EXEC sys.sp_executesql @sql;

    SET @sql = N'ALTER TABLE ' + QUOTENAME(@esquema) + N'.' + QUOTENAME(@tabela)
             + N' ADD CONSTRAINT ' + QUOTENAME(@pk) + N' PRIMARY KEY CLUSTERED (' + @colunasPk + N')'
             + N' ON ' + QUOTENAME(@ps) + N'(' + QUOTENAME(@coluna) + N');';
    EXEC sys.sp_executesql @sql;

    DECLARE recriar CURSOR LOCAL FAST_FORWARD FOR SELECT Comando FROM @recriar ORDER BY Ordem;
    OPEN recriar;
    FETCH NEXT FROM recriar INTO @comando;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC sys.sp_executesql @comando;
        FETCH NEXT FROM recriar INTO @comando;
    END;
    CLOSE recriar; DEALLOCATE recriar;

    FETCH NEXT FROM alvo INTO @esquema, @tabela, @coluna;
END;

CLOSE alvo; DEALLOCATE alvo;
");

            // Comentário no próprio catálogo do banco: quem abrir a tabela pelo SSMS lê a
            // política de retenção sem precisar procurar documento nenhum.
            migrationBuilder.Sql(@"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
    @value = N'Particionada por mes em AlteradoEm. Retencao: 18 meses disponiveis, arquivamento depois.',
    @level0type = N'SCHEMA', @level0name = N'auditoria',
    @level1type = N'TABLE',  @level1name = N'AlteracaoDeCampo';

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
    @value = N'Particionada por mes em OcorreuEm. Retencao: 24 meses disponiveis (LGPD).',
    @level0type = N'SCHEMA', @level0name = N'auditoria',
    @level1type = N'TABLE',  @level1name = N'EventoDeAcesso';

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
    @value = N'Particionada por mes em ExecutadoEm. Retencao: 12 meses disponiveis, arquivamento depois.',
    @level0type = N'SCHEMA', @level0name = N'processo',
    @level1type = N'TABLE',  @level1name = N'RegraExecucao';

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
    @value = N'Particionada por mes em RecebidaEm. Area de pouso EFEMERA: expurgo por ExpurgarApos.',
    @level0type = N'SCHEMA', @level0name = N'integracao',
    @level1type = N'TABLE',  @level1name = N'Recepcao';
");
        }
    }
}
