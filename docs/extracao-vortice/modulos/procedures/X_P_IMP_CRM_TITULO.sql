/* ==============================================================
   Objeto ..........: dbo.X_P_IMP_CRM_TITULO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-03-20 10:45:56
   Modificado em ...: 2023-03-21 08:10:14
   Linhas ..........: 135
   Escreve em tabela: SIM (INSERT, UPDATE, DELETE)
   Alvos de escrita : X_T_IMP_CRM_TITULO
   Tabelas referidas: IMP_Titulo, X_T_IMP_CRM_TITULO
   Outras refs .....: X_V_IMP_CRM_TITULO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

--###################################################################################################################
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
--###################################################################################################################
CREATE   PROCEDURE  [X_P_IMP_CRM_TITULO]
AS 
--DESENVOLVIMENTO DA PROCEDURE RESPONSAVEL POR REALIZAR A IMPORTAÇÃO DOS TITULOS DO CONTAS A RECEBER QUE FORAM EMITIDOS NO ERP TOTVS
--RESPONSAVEL 	- FELIPE AUGUSTO VIOLIN
--REQUISITANTE	- DIEGO MARQUES
--DATA DEPLOY - 20/03/2023 

--BACKUP BASE 
--SELECT * INTO X_T_IMP_CRM_TITULO_BKP_2 from X_T_IMP_CRM_TITULO

BEGIN

	--1º PASSO - Tabela 01 - Base Importação
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Base_Importacao%')
	BEGIN
	   DROP TABLE #Base_Importacao
	END
	
	--2º PASSO - abrir transação
	--BEGIN TRANSACTION; 
	--------------------------------------------------------------------------
	--3º PASSO - COMPOSIÇÃO BASE DE TITULOS A SEREM ATUALIZADOS E IMPORTADOS
	SELECT
		* 
		INTO #Base_Importacao
	from 
	(
	--TITULOS ALTERADOS
		SELECT 
			--B.* 
			'ATUALIZAR' AS TIPO,
			A.Obs					AS CHAVE,
			B.DATA_INCLUSAO			AS DATA_INCLUSAO_CRM,
			B.DATA_ALTERACAO		AS DATA_ALTERACAO_CRM,
			A.DATA_INCLUSAO			AS DATA_INCLUSAO_TOTVS ,
			A.DATA_ALTERACAO		AS DATA_ALTERACAO_TOTVS  
		FROM 
				TOTVS.TMPRD.dbo.X_V_IMP_CRM_TITULO 	A 
		INNER JOIN 	X_T_IMP_CRM_TITULO 				B 	ON 	A.Obs 				=	B.Obs COLLATE SQL_Latin1_General_CP1_CI_AS
														AND A.DATA_ALTERACAO	<>	B.DATA_ALTERACAO
		UNION ALL 
	
	--TITULOS INCLUIDOS	 
		SELECT 
			--B.*
			'INCLUIR' AS TIPO, 
			A.Obs,
			''						AS DATA_INCLUSAO_CRM,
			''						AS DATA_ALTERACAO_CRM,
			A.DATA_INCLUSAO			AS DATA_INCLUSAO_TOTVS,
			A.DATA_ALTERACAO		AS DATA_ALTERACAO_TOTVS 
			--B.Obs,
			--B.DATA_INCLUSAO,
			--B.DATA_ALTERACAO  
		FROM 
				TOTVS.TMPRD.dbo.X_V_IMP_CRM_TITULO 	A 
		WHERE a.Obs NOT IN (SELECT Obs COLLATE SQL_Latin1_General_CP1_CI_AS from X_T_IMP_CRM_TITULO)  
	) Base
	
	
	/*
	--EXIBIR LISTA DE TITULOS QUE VÃO SER INTEGRADOS COM BASE NO 3º PASSO
	select *, LEN(CHAVE) from #Base_Importacao
	--where TIPO = 'INCLUIR';
	 
	*/
	
	
	--------------------------------------------------------------------------
	--4º PASSO - EXCLUSÃO DOS REGISTROS QUE VÃO SER ATUALIZADOS NA BASE.
	DELETE FROM X_T_IMP_CRM_TITULO 
	WHERE OBS IN (SELECT CHAVE COLLATE SQL_Latin1_General_CP1_CI_AS FROM #Base_Importacao WHERE TIPO = 'ATUALIZAR')
	
	
	--------------------------------------------------------------------------
	--5º PASSO - IMPORTAÇÃO - TOTVS TO CRM BASE TRANSITORIA INTEGRACAO (INCLUSAO / ALTERAÇÃO)
	
	INSERT INTO CRM.dbo.X_T_IMP_CRM_TITULO
			(	Origem, NroEmpresa, PessoaLinkOrigem, Pessoalink, SeqPessoa, NroCNPJ, DigCNPJ, Departamento, Especie, NroTitulo, NroDocto, TipoCobranca, IndAtivo, IndQuitado, IndCobrJuridica, Status, NroBanco, LocalCobranca, NroTituloBanco, DtaEmissao, DtaVenctoOrig, DtaVencto, VlrOriginal, VlrAcrescimo, VlrAbatimento, VlrPago, VlrMov, Movimento, DtaUltPgto, DtaQuitacao, DtaUltAlteracao, UsuAlteracao, LinkNro, LinkStr, Obs, IdPessoa, CHAVESTR1,DATA_INCLUSAO, DATA_ALTERACAO, X_INTEGRADO, X_DATA_INTEGRACAO)
	SELECT 		Origem, NroEmpresa, PessoaLinkOrigem, Pessoalink, SeqPessoa, NroCNPJ, DigCNPJ, Departamento, Especie, NroTitulo, NroDocto, TipoCobranca, IndAtivo, IndQuitado, IndCobrJuridica, Status, NroBanco, LocalCobranca, NroTituloBanco, DtaEmissao, DtaVenctoOrig, DtaVencto, VlrOriginal, VlrAcrescimo, VlrAbatimento, VlrPago, VlrMov, Movimento, 
				CASE WHEN DtaUltPgto = '1900-01-01 00:00:00.000' THEN NULL ELSE DtaUltPgto END 				as DtaUltPgto, 
				CASE WHEN DtaQuitacao = '1900-01-01 00:00:00.000' THEN NULL ELSE DtaQuitacao END 			as DtaQuitacao, 
				CASE WHEN DtaUltAlteracao = '1900-01-01 00:00:00.000' THEN NULL ELSE DtaUltAlteracao END 	as DtaUltAlteracao, 
				UsuAlteracao, LinkNro, LinkStr, Obs, IdPessoa, CHAVESTR1,
				DATA_INCLUSAO, 
				CASE WHEN DATA_ALTERACAO = '1900-01-01 00:00:00.000' THEN NULL ELSE DATA_ALTERACAO END 		as DATA_ALTERACAO, 
				X_INTEGRADO, X_DATA_INTEGRACAO
	FROM 
		TOTVS.TMPRD.dbo.X_V_IMP_CRM_TITULO
	WHERE 
		Obs IN (SELECT CHAVE  FROM #Base_Importacao)
	
	
		
	--6º PASSO - Limpeza tabela que não vai mais ser utilizada
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Base_Importacao%')
	BEGIN
	   DROP TABLE #Base_Importacao
	END	
	
	--------------------------------------------------------------------------
	
	--7º PASSO - INSERCAO DOS REGISTROS NA TABELA DE INTEGRACAO
	INSERT INTO CRM.dbo.IMP_Titulo
			(	Origem, NroEmpresa, PessoaLinkOrigem, Pessoalink, SeqPessoa, NroCNPJ, DigCNPJ, Departamento, Especie, NroTitulo, NroDocto, TipoCobranca, IndAtivo, IndQuitado, IndCobrJuridica, Status, NroBanco, LocalCobranca, NroTituloBanco, DtaEmissao, DtaVenctoOrig, DtaVencto, VlrOriginal, VlrAcrescimo, VlrAbatimento, VlrPago, VlrMov, Movimento, DtaUltPgto, DtaQuitacao, DtaUltAlteracao, UsuAlteracao, LinkNro, LinkStr, Obs, IdPessoa, CHAVESTR1,dtaimport)
	SELECT 		Origem, NroEmpresa, PessoaLinkOrigem, Pessoalink, SeqPessoa, NroCNPJ, DigCNPJ, Departamento, Especie, NroTitulo, NroDocto, TipoCobranca, IndAtivo, IndQuitado, IndCobrJuridica, Status, NroBanco, LocalCobranca, NroTituloBanco, DtaEmissao, DtaVenctoOrig, DtaVencto, VlrOriginal, VlrAcrescimo, VlrAbatimento, VlrPago, VlrMov, Movimento, 
				CASE WHEN DtaUltPgto = '1900-01-01 00:00:00.000' THEN NULL ELSE DtaUltPgto END 				AS DtaUltPgto, 
				CASE WHEN DtaQuitacao = '1900-01-01 00:00:00.000' THEN NULL ELSE DtaQuitacao END 			AS DtaQuitacao, 
				CASE WHEN DtaUltAlteracao = '1900-01-01 00:00:00.000' THEN NULL ELSE DtaUltAlteracao END 	AS DtaUltAlteracao, 
				UsuAlteracao, LinkNro, LinkStr, Obs, IdPessoa, CHAVESTR1, GETDATE() as dtaimport 
	FROM 
		CRM.dbo.X_T_IMP_CRM_TITULO
	WHERE 
		X_INTEGRADO = 'NAO'
	
		
	--8º PASSO - ATUALIZAO DO STATUS DO TITULOS NA TABELA TRANSITORIA	
	UPDATE CRM.dbo.X_T_IMP_CRM_TITULO SET X_INTEGRADO = 'INTEGRADO',  X_DATA_INTEGRACAO = GETDATE()
	WHERE 
		X_INTEGRADO = 'NAO'
	
	--9º PASSO - COMITAR AS ALTERAÇÕES
	--COMMIT
	
END


--------------------------------------------------------------------------	
	--LISTAGEM -- DOS TITULOS QUE FORAM INTEGRADOS.
	--SELECT * FROM CRM.dbo.Imp_titulo_bkp_16_03_2023;
	--SELECT * FROM CRM.dbo.IMP_Titulo_BKP_20_03_2023;
	--SELECT * FROM CRM.dbo.IMP_Titulo