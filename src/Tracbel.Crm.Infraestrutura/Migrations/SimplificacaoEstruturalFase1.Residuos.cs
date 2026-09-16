using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// O QUE O EF CORE NÃO SABE REMOVER — a parte da fase 1 escrita à mão.
    ///
    /// Fica num arquivo À PARTE, pelo mesmo motivo de
    /// <c>ModeloInicial.Particionamento.cs</c>: regenerar a migração reescreve o arquivo gerado
    /// do zero e levaria este bloco junto. O que a regeneração precisa refazer são DUAS LINHAS
    /// no arquivo gerado — <c>RemoverResiduosDaSimplificacao(migrationBuilder);</c> no fim do
    /// <c>Up(...)</c> e <c>RefazerResiduosDaSimplificacao(migrationBuilder);</c> no fim do
    /// <c>Down(...)</c>.
    ///
    /// SÃO TRÊS RESÍDUOS, e nenhum deles é tabela do modelo:
    ///
    /// 1. <b>Partição órfã.</b> Três das quatro tabelas particionadas por mês saem nesta fase:
    ///    <c>auditoria.EventoDeAcesso</c>, <c>processo.RegraExecucao</c> e
    ///    <c>integracao.Recepcao</c>. Função e esquema de partição são objetos independentes da
    ///    tabela — o <c>DROP TABLE</c> não os leva junto, e ficariam no banco para sempre,
    ///    apontando para nada. Fica só a partição de <c>auditoria.AlteracaoDeCampo</c>, que é a
    ///    tabela de log que o sistema de fato escreve.
    ///
    /// 2. <b>Schema vazio.</b> <c>documento</c> e <c>relatorio</c> perdem todas as suas tabelas.
    ///    Schema vazio não custa espaço, mas mente: quem abre o banco lê dez assuntos e encontra
    ///    oito. O <c>Down</c> os recria — o próprio EF já emite o <c>EnsureSchema</c> deles.
    ///
    /// 3. <b>Histórico de migração órfão.</b> <c>dbo.__EFMigrationsHistory</c> nasceu de uma
    ///    execução antiga, antes de o histórico ser configurado em <c>metadado</c>
    ///    (<c>FabricaDeContextoEmTempoDeDesenho</c>). Está vazia, está fora do modelo do EF e
    ///    nenhum caminho da aplicação a lê. A REMOÇÃO É CONDICIONAL: se alguém tiver gravado
    ///    alguma linha nela desde a conferência, a tabela FICA e a migração segue sem erro —
    ///    apagar histórico de migração por engano é o tipo de estrago que não se desfaz.
    ///
    /// Documento 41, fase 1.
    /// </summary>
    public partial class SimplificacaoEstruturalFase1
    {
        /// <summary>As três partições mensais que saem, e a coluna de data de cada uma.</summary>
        private static readonly (string Esquema, string Tabela, string Coluna)[] ParticoesQueSaem =
        [
            ("auditoria",  "EventoDeAcesso", "OcorreuEm"),
            ("processo",   "RegraExecucao",  "ExecutadoEm"),
            ("integracao", "Recepcao",       "RecebidaEm")
        ];

        /// <summary>Os schemas que ficam sem nenhuma tabela depois das remoções.</summary>
        private static readonly string[] SchemasQueEsvaziam = ["documento", "relatorio"];

        /// <summary>Limpa o que o <c>DropTable</c> deixa para trás. Chamado no fim do <c>Up</c>.</summary>
        private static void RemoverResiduosDaSimplificacao(MigrationBuilder migrationBuilder)
        {
            foreach (var (_, tabela, _) in ParticoesQueSaem)
                migrationBuilder.Sql(
                    $@"IF EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_Mensal_{tabela}')
    DROP PARTITION SCHEME [PS_Mensal_{tabela}];
IF EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_Mensal_{tabela}')
    DROP PARTITION FUNCTION [PF_Mensal_{tabela}];");

            // O schema só cai se estiver REALMENTE vazio. A conferência olha sys.objects, e não
            // só tabela: view, procedure ou tipo esquecido ali dentro faz o DROP SCHEMA falhar, e
            // uma migração que falha no meio é pior do que um schema a mais.
            foreach (var schema in SchemasQueEsvaziam)
                migrationBuilder.Sql(
                    $@"IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'{schema}')
   AND NOT EXISTS (SELECT 1 FROM sys.objects o
                   JOIN sys.schemas s ON s.schema_id = o.schema_id
                   WHERE s.name = N'{schema}')
    EXEC(N'DROP SCHEMA [{schema}]');");

            // A tabela órfã, e SÓ se continuar vazia.
            //
            // O SELECT VAI DENTRO DE UM EXEC de propósito: o SQL Server resolve os nomes do lote
            // inteiro ANTES de executar, inclusive os do ramo que o IF não vai tomar. Num banco
            // onde a órfã não existe — todo banco nascido depois que o histórico foi configurado
            // em `metadado`, e é o caso dos bancos dos testes — um `SELECT ... FROM
            // [dbo].[__EFMigrationsHistory]` escrito direto aqui derrubaria a migração com
            // "Invalid object name" sem nunca chegar a rodar. Só `OBJECT_ID`, que recebe o nome
            // como TEXTO, pode ficar no lote de fora.
            migrationBuilder.Sql(
                @"IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
    EXEC(N'IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory])
    DROP TABLE [dbo].[__EFMigrationsHistory];');");
        }

        /// <summary>
        /// Devolve o banco ao estado anterior. Chamado no FIM do <c>Down</c>, depois que o EF já
        /// recriou as tabelas e os índices — o particionamento recria a chave primária
        /// clusterizada e os índices não clusterizados SOBRE o esquema de partição, e por isso
        /// precisa que eles já existam.
        /// </summary>
        private static void RefazerResiduosDaSimplificacao(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId]    nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );");

            // Mesma técnica de ModeloInicial.Particionamento: cria função e esquema de partição e
            // recria a chave primária clusterizada sobre o esquema, que é o que move a tabela.
            // A janela é recalculada a partir de hoje, como na migração inicial — as três tabelas
            // estão vazias neste ponto, então não há linha para cair fora da janela.
            var alvos = string.Join(",\n    ", ParticoesQueSaem.Select(a =>
                $"(N'{a.Esquema}', N'{a.Tabela}', N'{a.Coluna}')"));

            migrationBuilder.Sql($@"
SET NOCOUNT ON;

DECLARE @alvos TABLE (Esquema sysname, Tabela sysname, Coluna sysname);
INSERT INTO @alvos (Esquema, Tabela, Coluna) VALUES
    {alvos};

DECLARE @inicio datetime2(3) = DATEADD(month, -3, DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1));
DECLARE @limites nvarchar(max) = N'';
DECLARE @i int = 0;
WHILE @i <= 15
BEGIN
    SET @limites = @limites + CASE WHEN @i = 0 THEN N'' ELSE N', ' END
                 + N'N''' + CONVERT(nvarchar(23), DATEADD(month, @i, @inicio), 126) + N'''';
    SET @i = @i + 1;
END;

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

    SET @sql = N'CREATE PARTITION SCHEME ' + QUOTENAME(@ps)
             + N' AS PARTITION ' + QUOTENAME(@pf) + N' ALL TO ([PRIMARY]);';
    EXEC sys.sp_executesql @sql;

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

            migrationBuilder.Sql(@"
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
