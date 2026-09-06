/* ==============================================================
   Objeto ..........: dbo.fiv_GetProdutoEstrutura
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_ESTRPRODUTO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fiv_GetProdutoEstrutura (@pnSeqEstrutura Numeric(18,0), @psSeparador varchar) RETURNS VARCHAR(250) AS BEGIN      Declare @vsRetorno      varchar(250)  	Declare @vsDescricao      varchar(50)      Declare @vnSeqEstrProd  varchar(250)  	Declare @vsSeparador	varchar(5) 	Declare @vnFetch		numeric(2)     		   	DECLARE c01 CURSOR FOR  			SELECT EST.DESCRICAO, EST.SEQESTRPRODPAI               FROM IV_ESTRPRODUTO EST               WHERE EST.SEQESTRPROD = @vnSeqEstrProd      Set @vsRetorno  = ''		 	Set @vnSeqEstrProd = @pnSeqEstrutura 	Set @vsSeparador = @psSeparador 	If len(@vsSeparador) < 1  	  Begin  		Set @vsSeparador = '/'  	  End 	 	OPEN c01 		FETCH NEXT FROM c01 INTO   @vsDescricao, @vnSeqEstrProd 	WHILE @@FETCH_STATUS = 0 		BEGIN 			If   isnull(len(@vsRetorno) + len(@vsDescricao + @vsSeparador), 1) < 250  			Begin  				Set @vsRetorno = @vsRetorno + @vsSeparador + @vsDescricao 			End 			CLOSE c01 			OPEN c01 			FETCH NEXT FROM c01 INTO   @vsDescricao, @vnSeqEstrProd   		END 	CLOSE c01 	DEALLOCATE c01   RETURN @vsRetorno END   