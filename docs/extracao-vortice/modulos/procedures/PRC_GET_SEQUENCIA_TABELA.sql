/* ==============================================================
   Objeto ..........: dbo.PRC_GET_SEQUENCIA_TABELA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-06-27 10:08:29
   Modificado em ...: 2025-06-27 10:08:29
   Linhas ..........: 47
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : GE_SEQUENCIA
   Tabelas referidas: GE_SEQUENCIA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create PROCEDURE [dbo].[PRC_GET_SEQUENCIA_TABELA]
    @sNomeTabela NVARCHAR(100),         -- Nome da tabela no controle GE_SEQUENCIA
    @vnSeqNovo INT OUTPUT,              -- Variável de saída com a nova sequência
    @sTabelaOrigem NVARCHAR(100) = NULL, -- Nome da tabela real para fazer o MAX()
    @sCampoOrigem NVARCHAR(100) = NULL   -- Nome do campo para aplicar o MAX()
AS
BEGIN
    SET NOCOUNT ON;

    -- Tenta buscar a sequência da tabela de controle
    SET @vnSeqNovo = (
        SELECT SEQUENCIA
        FROM GE_SEQUENCIA
        WHERE NOMETABELA = @sNomeTabela
    )

    IF @vnSeqNovo IS NULL
    BEGIN
        DECLARE @sql NVARCHAR(MAX), @paramDef NVARCHAR(200)

        -- Monta SQL dinâmico para pegar o MAX do campo da tabela origem
        SET @sql = N'SELECT @maxID = MAX([' + @sCampoOrigem + ']) FROM [' + @sTabelaOrigem + ']'
        SET @paramDef = N'@maxID INT OUTPUT'

        DECLARE @maxID INT
        EXEC sp_executesql @sql, @paramDef, @maxID = @maxID OUTPUT

        IF @maxID IS NULL
            SET @vnSeqNovo = 1
        ELSE
            SET @vnSeqNovo = @maxID + 1

        -- Insere o novo valor na tabela de controle
        INSERT INTO GE_SEQUENCIA (NOMETABELA, SEQUENCIA)
        VALUES (@sNomeTabela, @vnSeqNovo)
    END
    ELSE
    BEGIN
        SET @vnSeqNovo = @vnSeqNovo + 1

        UPDATE GE_SEQUENCIA
        SET SEQUENCIA = @vnSeqNovo
        WHERE NOMETABELA = @sNomeTabela
    END
END

