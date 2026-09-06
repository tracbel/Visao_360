/* ==============================================================
   Objeto ..........: dbo.fva_GetProcessoTag
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_PROCTAG
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fva_GetProcessoTag (@pnProcesso numeric ) RETURNS VARCHAR(250) AS BEGIN      Declare @vsRetorno      varchar(250)      Declare @vsVirg    varchar(2)     Declare @vsTag    varchar(20)  		   		   	DECLARE c01 CURSOR FOR  			SELECT TAG  FROM IV_PROCTAG WHERE PROCESSO = @pnProcesso ORDER BY 1      Set @vsRetorno  = '' 	Set @vsVirg = ''		 	OPEN c01	 	FETCH NEXT FROM c01 	INTO   @vsTag 	 	WHILE @@FETCH_STATUS = 0 		BEGIN 			If   isnull(len(@vsRetorno) + len(@vsTag), 1) < 225  			Begin  				Set @vsRetorno = @vsRetorno  + @vsVirg + LTRIM(RTRIM(@vsTag)) 				Set @vsVirg = ', ' 			End 			FETCH NEXT FROM c01  INTO   @vsTag   		END 	CLOSE c01 	DEALLOCATE c01   RETURN @vsRetorno END  