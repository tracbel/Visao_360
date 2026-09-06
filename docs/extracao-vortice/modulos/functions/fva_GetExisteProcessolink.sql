/* ==============================================================
   Objeto ..........: dbo.fva_GetExisteProcessolink
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_DOCTOTIPO, IV_ProcDocto, IV_PROCLINK, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fva_GetExisteProcessolink (@Processo numeric(18,0) ) RETURNS numeric AS BEGIN  	Declare @vnRetorno      numeric 	SELECT @vnRetorno = COUNT(1) 	FROM IV_PROCLINK PL 	JOIN IV_DOCTOTIPO DOC ON DOC.DOCTO = PL.LINKDOCTO 	WHERE PL.PROCESSO = @Processo 	 	if @vnRetorno > 0  	begin  		Return 1 	End 	 	SELECT @vnRetorno = COUNT(1) 	FROM IV_Questionario QU 	WHERE QU.PROCESSO = @Processo 	 	if @vnRetorno > 0  	begin  		Return 1 	End 	SELECT @vnRetorno = COUNT(1) 	FROM IV_ProcDocto  	WHERE PROCESSO = @Processo 	 	if @vnRetorno > 0  	begin  		Return 1 	End 	RETURN 0 END  