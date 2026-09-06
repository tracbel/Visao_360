/* ==============================================================
   Objeto ..........: dbo.fIV_GetResultadoAcao
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_RESULTADO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fIV_GetResultadoAcao (@pnAcao Decimal(6,0) ) RETURNS VARCHAR(250) AS BEGIN      Declare @vsRetorno      varchar(250)      Declare @vsResultado    varchar(250)  		   	DECLARE c01 CURSOR FOR  			SELECT ltrim(rtrim(DESCREDUZIDA))              FROM IV_RESULTADO R 			WHERE R.ACAO = @pnACAO 			  AND R.EMUSO > 0 			ORDER BY R.ORDEM     Set @vsRetorno  = ''		 	OPEN c01	 	FETCH NEXT FROM c01 	INTO   @vsResultado 	 	WHILE @@FETCH_STATUS = 0 		BEGIN 			If   isnull(len(@vsRetorno) + len(@vsResultado), 1) < 240  			Begin  				Set @vsRetorno = @vsRetorno  + '[_]-' + @vsResultado + '  ' 			End 			FETCH NEXT FROM c01  INTO   @vsResultado   		END 	CLOSE c01 	DEALLOCATE c01   RETURN @vsRetorno END  