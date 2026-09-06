/* ==============================================================
   Objeto ..........: dbo.X_P_IMP_CRM_NF_POS_VENDA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-09-12 13:45:52
   Modificado em ...: 2023-10-11 15:14:56
   Linhas ..........: 302
   Escreve em tabela: SIM (INSERT, UPDATE, TRUNCATE)
   Alvos de escrita : IMP_NFSItem, IMP_NFS
   Tabelas referidas: IMP_NFS, IMP_NFSItem, X_TOTVS_CRM_FATURAMENTO, X_V_IMP_CRM_IMP_NF
   Outras refs .....: X_TOTVS_ATUA_CRM_FATURAMENTO, X_V_CRM_IMP_IMP_NFS, X_V_CRM_IMP_NFSItem
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

--###################################################################################################################
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
--###################################################################################################################

CREATE   PROCEDURE  [X_P_IMP_CRM_NF_POS_VENDA]
AS 

/*

--DESENVOLVIMENTO DA PROCEDURE RESPONSAVEL POR REALIZAR A IMPORTAÇÃO DAS NOTAS FISCAIS POS VENDAS EMITIDAS NO ERP TOTVS
--RESPONSAVEL 	- FELIPE AUGUSTO VIOLIN
--REQUISITANTE	- DIEGO MARQUES
--DATA DEPLOY - 20/09/2023 
--Exec [X_P_IMP_CRM_NF_POS_VENDA]

	--SELECT * INTO IMP_NFSItem_BKP_15_09_2023 FROM IMP_NFSItem;
	--SELECT * INTO IMP_NFSFROM_BKP_15_09_2023 FROM IMP_NFS;

	--SELECT * FROM IMP_NFSItem_BKP_15_09_2023;
	--SELECT * FROM IMP_NFSFROM_BKP_15_09_2023

	--TRUNCATE TABLE IMP_NFSItem;
	--TRUNCATE TABLE IMP_NFS;
	--SELECT * FROM IMP_NFSItem;
	--SELECT * FROM IMP_NFS;

*/

BEGIN
	--##########################################################################################################
	--0º PASSO - Executar Procedure DB LINK_Nova - Atualizar o Repositorio na tabela X_TOTVS_BI_FATURAMENTO_POS_VENDAS_N
	Exec [dbo].[X_TOTVS_ATUA_CRM_FATURAMENTO]
	
	--##########################################################################################################
	--1º PASSO - Verificar a existencia da tabela intermediaria para controle da integracao
	IF NOT EXISTS (SELECT *FROM sys.tables WHERE name = 'X_V_IMP_CRM_IMP_NF')
	BEGIN
		CREATE TABLE CRM.dbo.X_V_IMP_CRM_IMP_NF (
			ID int IDENTITY(1,1) NOT NULL,
			COD_CHAVE_ITEM varchar(150) COLLATE Latin1_General_CI_AS NOT NULL,
			COD_CHAVE_NF varchar(100) COLLATE Latin1_General_CI_AS NOT NULL,
			ORIGEM varchar(12) COLLATE Latin1_General_CI_AS NOT NULL,
			X_INTEGRADO varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
			X_DATA_INTEGRACAO datetime NULL
			
			CONSTRAINT X_V_IMP_CRM_IMP_NF_PK PRIMARY KEY (ID)
		)
		
	 CREATE NONCLUSTERED INDEX X_V_IMP_CRM_IMP_NF_ID_CHAVE_ITEM ON dbo.X_V_IMP_CRM_IMP_NF (  ID ASC  , COD_CHAVE_ITEM ASC  )  
		 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
		 ON [PRIMARY ] 
	 CREATE NONCLUSTERED INDEX X_V_IMP_CRM_IMP_NF_ID_INTEGRADO_CHAVE_ITEM ON dbo.X_V_IMP_CRM_IMP_NF (  ID ASC  , X_INTEGRADO ASC  , COD_CHAVE_ITEM ASC  )  
		 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
		 ON [PRIMARY ] 
	 CREATE NONCLUSTERED INDEX X_V_IMP_CRM_IMP_NF_SQL_ITEM ON dbo.X_V_IMP_CRM_IMP_NF (  ID ASC  , COD_CHAVE_ITEM ASC  , ORIGEM ASC  , X_INTEGRADO ASC  )  
		 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
		 ON [PRIMARY ] 
	 CREATE NONCLUSTERED INDEX X_V_IMP_CRM_IMP_NF_SQL_NF ON dbo.X_V_IMP_CRM_IMP_NF (  ID ASC  , COD_CHAVE_NF ASC  , ORIGEM ASC  , X_INTEGRADO ASC  )  
		 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
		 ON [PRIMARY ] 
		END		
	
	--##########################################################################################################
	--2º PASSO - Tabela 01 - Base Importação
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Base_Importacao%')
	BEGIN
	   DROP TABLE #Base_Importacao
	END
	
	--COMPOSIÇÃO BASE DE NOTAS A SEREM IMPORTADAS
	
		SELECT 
			'INCLUIR' 				AS TIPO, 
			CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',COD_OPERACAO,'|',PRODUTO,'|', NRO_ITEM,'|',DELETADO)
									AS COD_CHAVE_ITEM,
			CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',DELETADO)
									AS COD_CHAVE_NF,
			ORIGEM
			
		INTO #Base_Importacao
			
		FROM 
				X_TOTVS_CRM_FATURAMENTO 	A with (nolock)
		WHERE
			A.DATA_EMISSAO_NF >= '2023-01-01'
		AND	CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',COD_OPERACAO,'|',PRODUTO,'|', NRO_ITEM,'|',DELETADO) 
		NOT IN (SELECT COD_CHAVE_ITEM from CRM.dbo.X_V_IMP_CRM_IMP_NF with (nolock))  
	
	--####Validacao
	/*
		SELECT *  FROM #Base_Importacao
		select count 
	*/
	
	--##########################################################################################################
	--3º PASSO - Teste se a tabela #Base_Importacao possui registros, se nao possui finaliza a procedure
	IF  (SELECT count(*) FROM #Base_Importacao) = 0
	BEGIN
	   RETURN 
	END	
		
	--##########################################################################################################
	--4º PASSO - IMPORTAÇÃO - X_TOTVS_BI_FATURAMENTO_POS_VENDAS_N TO CRM BASE TRANSITORIA INTEGRACAO (INCLUSAO)
	INSERT INTO CRM.dbo.X_V_IMP_CRM_IMP_NF (COD_CHAVE_ITEM, COD_CHAVE_NF, ORIGEM, X_INTEGRADO, X_DATA_INTEGRACAO)
	SELECT 
		CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',COD_OPERACAO,'|',PRODUTO,'|', NRO_ITEM,'|',DELETADO)
								AS COD_CHAVE_ITEM,
		CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',DELETADO)						
								AS COD_CHAVE_NF,
		A.ORIGEM,
		'NAO'					AS 	X_INTEGRADO,
		cast(NULL as datetime) X_DATA_INTEGRACAO 
		
	FROM 
		X_TOTVS_CRM_FATURAMENTO	A	with (nolock)
	WHERE 
		A.DATA_EMISSAO_NF >= '2023-01-01'
	AND CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',COD_OPERACAO,'|',PRODUTO,'|', NRO_ITEM,'|',DELETADO) 
		IN (SELECT COD_CHAVE_ITEM  FROM #Base_Importacao)
		
	--##########################################################################################################
	--5º PASSO - Limpeza tabela que não vai mais ser utilizada
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Base_Importacao%')
	BEGIN
	   DROP TABLE #Base_Importacao
	END	
	
	--##########################################################################################################
	--6º PASSO - INSERINDO VALORES TABELA (IMP_NFS)

	--INSERT INTO CRM.dbo.IMP_NFS2 
	INSERT INTO CRM.dbo.IMP_NFS
	(Origem, NroEmpresa, Nronf, Serienf, NroEmpresaVda, NroCNPJCPF, DigCNPJCPF, CNPJx, PessoaLinkOrigem, Pessoalink, Segmento, Departamento, TipoVenda, CFOP, CodOperacao, Operacao, CanalVenda, Setor, Formapgto, CondicaoPgto, Vendedor, CodVendedor, Dtapedido, Nropedido, Dtaemissaonf, Situacao, CodTransportador, Transportador, Usuario, Obs, DtaAlteracaoERP, DtaGeracao, StatusIMP, DtaImport, WhereItem, PERCBASECOMISSAO, IDNFSEXTERNO, LOTECARGA, PAIIDNFSEXTERNO, EVENTOCMPL, TIPOPEDIDO, IDENTIFICADO, NROCPFVENDEDOR, NROVOUCHER)	
	SELECT 
		--COD_CHAVE_NF,
		--IdNFS, 
		Origem, 
		NroEmpresa, 
		Nronf, 
		Serienf, 
		NroEmpresaVda, 
		NroCNPJCPF, 
		DigCNPJCPF, 
		CNPJx, 
		PessoaLinkOrigem, 
		Pessoalink, 
		Segmento, 
		Departamento, 
		TipoVenda, 
		CFOP, 
		CodOperacao, 
		Operacao, 
		CanalVenda, 
		Setor, 
		Formapgto, 
		CondicaoPgto, 
		Vendedor, 
		CodVendedor, 
		Dtapedido, 
		Nropedido, 
		Dtaemissaonf, 
		Situacao, 
		CodTransportador, 
		Transportador, 
		Usuario, 
		Obs, 
		DtaAlteracaoERP, 
		DtaGeracao, 
		StatusIMP, 
		DtaImport, 
		WhereItem, 
		PERCBASECOMISSAO, 
		IDNFSEXTERNO, 
		LOTECARGA, 
		PAIIDNFSEXTERNO, 
		EVENTOCMPL, 
		TIPOPEDIDO, 
		IDENTIFICADO, 
		NROCPFVENDEDOR, 
		NROVOUCHER
	FROM 		
		CRM.dbo.X_V_CRM_IMP_IMP_NFS
	WHERE 
		COD_CHAVE_NF IN (	SELECT DISTINCT 
								COD_CHAVE_NF 
							FROM 
								X_V_IMP_CRM_IMP_NF 
							WHERE 
								Origem in 	(	'FAT_PECAS','DEV_PECAS','FAT_SERVICOS'
											,'FAT_MAQUINAS','DEV_MAQUINAS'
											)
							AND X_INTEGRADO = 'NAO'
						)	
/*	and	Departamento ='MAQ-NOVOS' 
	and Dtaemissaonf > '2023-09-01'
	and Nronf in (
					--192328,  	
					--54262,	
					192878,	
					23104,	
					--193687,	
					23284	--	
				  )
*/

	
	--	SELECT * FROM IMP_NFS;
	--	SELECT COUNT(*) FROM IMP_NFS;		
	
	--##########################################################################################################
	--7º PASSO - INSERINDO VALORES TABELA (IMP_NFSItem)
	--INSERT INTO CRM.dbo.IMP_NFSItem2
	INSERT INTO CRM.dbo.IMP_NFSItem
	(Origem, NroEmpresa, Nronf, Serienf, NroItem, CodProduto, NCM, CodBarra, Departamento, Marca, CodFamilia, Familia, Descproduto, Qtde, VlrLiqItem, Vlrdescto, VlrResult, VlrCustoMkt, VlrICM, VlrImposto, Situacao, Vendedor, CodVendedor, Identificador, Obs, DtaGeracao, StatusIMP, DtaImport, TIPO, LOTECARGA, SETORITEM, VLRCUSTO, IDNFSEXTERNO, VLRICMSRETIDO, VLRICMSSUBS, VLRPIS, VLRCOFINS, NROCPFVENDEDOR, VLRDESCTOVCHR)
	SELECT 
		--COD_CHAVE_ITEM, 
		--A.IdNFSItem, 
		A.Origem, 
		A.NroEmpresa, 
		A.Nronf, 
		A.Serienf, 
		A.NroItem, 
		A.CodProduto, 
		A.NCM, 
		A.CodBarra, 
		A.Departamento, 
		A.Marca, 
		A.CodFamilia, 
		A.Familia, 
		A.Descproduto, 
		A.Qtde, 
		A.VlrLiqItem, 
		A.Vlrdescto, 
		A.VlrResult, 
		A.VlrCustoMkt, 
		A.VlrICM, 
		A.VlrImposto, 
		A.Situacao, 
		A.Vendedor, 
		A.CodVendedor, 
		A.Identificador, 
		A.Obs, 
		A.DtaGeracao, 
		A.StatusIMP, 
		A.DtaImport, 
		A.TIPO, 
		A.LOTECARGA, 
		A.SETORITEM, 
		A.VLRCUSTO, 
		A.IDNFSEXTERNO, 
		A.VLRICMSRETIDO, 
		A.VLRICMSSUBS, 
		A.VLRPIS, 
		A.VLRCOFINS, 
		A.NROCPFVENDEDOR, 
		A.VLRDESCTOVCHR
	FROM 
		CRM.dbo.X_V_CRM_IMP_NFSItem A
	WHERE 
		COD_CHAVE_ITEM IN 	(	SELECT 
									COD_CHAVE_ITEM 
								FROM 
									X_V_IMP_CRM_IMP_NF 
								WHERE 
									Origem in 	('FAT_PECAS','DEV_PECAS','FAT_SERVICOS'
												,'FAT_MAQUINAS','DEV_MAQUINAS'
												)
								AND X_INTEGRADO = 'NAO'
							)
	/*and	Departamento ='MAQ-NOVOS' 
	--and Dtaemissaonf > '2023-09-01'
	and Nronf in (
					--192328,  	
					--54262,	
					192878,	
					23104,	
					--193687,	
					23284	--	
				  )
	*/

	--	SELECT * FROM IMP_NFSItem;
	--	SELECT COUNT(*) FROM IMP_NFSItem;
	
	--##########################################################################################################
	--8º PASSO - ATUALIZAO DO STATUS DO TITULOS NA TABELA TRANSITORIA
	
	UPDATE CRM.dbo.X_V_IMP_CRM_IMP_NF 
		SET X_INTEGRADO = 'INTEGRADO',  X_DATA_INTEGRACAO = CAST( GETDATE() AS datetime)
	WHERE 
		X_INTEGRADO = 'NAO'	
	AND Origem in 	('FAT_PECAS','DEV_PECAS','FAT_SERVICOS','FAT_MAQUINAS','DEV_MAQUINAS')	
		
	--SELECT  * FROM 	X_V_IMP_CRM_IMP_NF WHERE CAST(X_DATA_INTEGRACAO AS DATE) <> '2023-10-03' 
	--------------------------------------------------------	
	
END
----------------------------------------------------------------------------------------------------------------------------



