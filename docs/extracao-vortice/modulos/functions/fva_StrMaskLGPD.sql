/* ==============================================================
   Objeto ..........: dbo.fva_StrMaskLGPD
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

    CREATE FUNCTION dbo.fva_StrMaskLGPD(@psTexto VARCHAR(250), @pnInicio INT, @pnFinal INT) RETURNS VARCHAR(250) AS BEGIN   DECLARE @vsRet VARCHAR(250)    DECLARE @nPos INT    DECLARE @nFim INT       IF LEN(@psTexto) < 3 OR @psTexto IS NULL     BEGIN         RETURN @psTexto     END         SET @vsRet = LEFT(@psTexto, @pnInicio)    SET @nPos = @pnInicio + 1    SET @nFim = LEN(@psTexto) - @pnFinal    WHILE @nPos <= @nFim   BEGIN     IF SUBSTRING(@psTexto, @nPos, 1) = ' '       SET @vsRet = @vsRet + ' '      ELSE       SET @vsRet = @vsRet + '*'      SET @nPos = @nPos + 1    END     SET @vsRet = @vsRet + SUBSTRING(@psTexto, @nPos, 200)   RETURN @vsRet  END   