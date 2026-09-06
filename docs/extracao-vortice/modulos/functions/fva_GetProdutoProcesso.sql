/* ==============================================================
   Objeto ..........: dbo.fva_GetProdutoProcesso
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_PROCPRODUTO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fva_GetProdutoProcesso (@pnProcesso Numeric ) RETURNS VARCHAR(400) AS BEGIN      Declare @vsRetorno      varchar(400)      Declare @vsResultado    varchar(400)    		   	DECLARE c01 CURSOR FOR  			SELECT ltrim(rtrim(PP.CODPRODUTO))             FROM IV_PROCPRODUTO PP 			WHERE PP.PROCESSO = @pnPROCESSO 			ORDER BY PP.CODPRODUTO     Set @vsRetorno  = ''		 	OPEN c01 	 	FETCH NEXT FROM c01 	INTO   @vsResultado  	 	WHILE @@FETCH_STATUS = 0 		BEGIN 			If   isnull(len(@vsRetorno) + len(@vsResultado), 1) < 245  			Begin  				Set @vsRetorno = @vsRetorno + @vsResultado + '  ' 			End 			FETCH NEXT FROM c01  INTO   @vsResultado   		END 	CLOSE c01 	DEALLOCATE c01   RETURN @vsRetorno END    