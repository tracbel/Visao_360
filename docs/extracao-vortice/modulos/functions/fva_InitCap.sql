/* ==============================================================
   Objeto ..........: dbo.fva_InitCap
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

  CREATE FUNCTION dbo.fva_InitCap (@Nome VARCHAR(400))      RETURNS Varchar(400) AS BEGIN     DECLARE @strtemp VARCHAR(200)    DECLARE @i INT     SET @strtemp = LOWER(@Nome)     SET @strtemp = UPPER(LEFT(@strtemp,1)) +                   SUBSTRING(@strtemp,2,LEN(@strtemp))    WHILE CHARINDEX(' ',@strtemp,1) > 0        BEGIN           SET @i = CHARINDEX(' ',@strtemp,1)           SET @strtemp = LEFT(@strtemp,@i-1) + '|' +                           UPPER(SUBSTRING(@strtemp,@i + 1,1)) +                          SUBSTRING(@strtemp,@i+2,LEN(@strtemp))        END    SET @strtemp = REPLACE(@strtemp,'|',' ')    SET @strtemp = REPLACE(@strtemp,' Da ',' da ')    SET @strtemp = REPLACE(@strtemp,' Das ',' das ')    SET @strtemp = REPLACE(@strtemp,' Do ',' do ')    SET @strtemp = REPLACE(@strtemp,' Dos ',' dos ')    SET @strtemp = REPLACE(@strtemp,' De ',' de ')    SET @strtemp = REPLACE(@strtemp,' E ',' e ')    SET @Nome = @strtemp    RETURN @Nome END  