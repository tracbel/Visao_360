/* ==============================================================
   Objeto ..........: dbo.fIV_GetProdutoProcesso
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2011-12-19 11:48:38
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 31
   Escreve em tabela: nao
   Tabelas referidas: IV_PROCPRODUTO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION [dbo].[fIV_GetProdutoProcesso] (@pnProcesso Numeric(18,0))
RETURNS VARCHAR(250)
AS
BEGIN 
    Declare @vsRetorno      varchar(250) 
    Declare @vsResultado    varchar(250) 
 
		  
	DECLARE c01 CURSOR FOR 
			SELECT ltrim(rtrim(PP.CODPRODUTO))
            FROM IV_PROCPRODUTO PP
			WHERE PP.PROCESSO = @pnPROCESSO
			ORDER BY PP.CODPRODUTO
    Set @vsRetorno  = ''		
	OPEN c01
	
	FETCH NEXT FROM c01 	INTO   @vsResultado 
	
	WHILE @@FETCH_STATUS = 0
		BEGIN
			If   isnull(len(@vsRetorno) + len(@vsResultado), 1) < 245 
			Begin 
				Set @vsRetorno = @vsRetorno + @vsResultado + '  '
			End
			FETCH NEXT FROM c01  INTO   @vsResultado  
		END
	CLOSE c01
	DEALLOCATE c01
  RETURN @vsRetorno
END 
