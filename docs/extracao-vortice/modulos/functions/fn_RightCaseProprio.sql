/* ==============================================================
   Objeto ..........: dbo.fn_RightCaseProprio
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2011-12-19 11:48:36
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 25
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION dbo.fn_RightCaseProprio (@Nome VARCHAR(200))
    RETURNS Varchar(200) AS
BEGIN 
   DECLARE @strtemp VARCHAR(200)
   DECLARE @i INT 
   SET @strtemp = LOWER(@Nome) 
   SET @strtemp = UPPER(LEFT(@strtemp,1)) +
                  SUBSTRING(@strtemp,2,LEN(@strtemp))
   WHILE CHARINDEX(' ',@strtemp,1) > 0
       BEGIN
          SET @i = CHARINDEX(' ',@strtemp,1)
          SET @strtemp = LEFT(@strtemp,@i-1) + '|' + 
                         UPPER(SUBSTRING(@strtemp,@i + 1,1)) +
                         SUBSTRING(@strtemp,@i+2,LEN(@strtemp))
       END
   SET @strtemp = REPLACE(@strtemp,'|',' ')
   SET @strtemp = REPLACE(@strtemp,' Da ',' da ')
   SET @strtemp = REPLACE(@strtemp,' Das ',' das ')
   SET @strtemp = REPLACE(@strtemp,' Do ',' do ')
   SET @strtemp = REPLACE(@strtemp,' Dos ',' dos ')
   SET @strtemp = REPLACE(@strtemp,' De ',' de ')
   SET @Nome = @strtemp
   RETURN @Nome
END
