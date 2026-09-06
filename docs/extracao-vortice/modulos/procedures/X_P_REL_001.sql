/* ==============================================================
   Objeto ..........: dbo.X_P_REL_001
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-11-22 15:27:48
   Modificado em ...: 2023-11-22 17:07:28
   Linhas ..........: 392
   Escreve em tabela: nao
   Tabelas referidas: X_TOTVS_CRM_FATURAMENTO
   Outras refs .....: X_CRM_BI_CONGLOMERADO, X_V_COL_CRM_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE   Procedure [dbo].[X_P_REL_001] (
	@datai as VARCHAR(32), @dataf as VARCHAR(32), @filiali as VARCHAR(32), @filialf as VARCHAR(32)
	) as

/*
	--variavel controle
	DECLARE @datai as VARCHAR(32)
	DECLARE @dataf as VARCHAR(32)
	DECLARE @filiali as VARCHAR(32)
	DECLARE @filialf as VARCHAR(32)
	
	SET @datai 		= '01/01/2022'
	SET @dataf 		= '31/10/2023'
	SET @filiali	= '01'
	SET @filialf 	= '12'
*/
	
	DECLARE @vdatai as VARCHAR(32)
	DECLARE @vdataf as VARCHAR(32)
	
	--AJUSTE FORMATO DATA
	SELECT @vdatai = CONVERT(VARCHAR(10),CONVERT(date, @datai, 105),23)
	SELECT @vdataf = CONVERT(VARCHAR(10),CONVERT(date, @dataf, 105),23)
	
	--select @vdatai, @vdataf
	
	--#########################################################################################--
	---------------------------------------------------------------------------------------------
	--#########################################################################################--

	--Tabela 01
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab01%')
	BEGIN
	   DROP TABLE #Tab01
	END

	--extracao base de clientes e fornecedores 
	SELECT 
		TABELA,
		COD_CLIENTE
		INTO #Tab01
	FROM (
		SELECT 
			CASE 
				WHEN ORIGEM in 	('FAT_PECAS','FAT_SERVICOS','FAT_MAQUINAS')
					THEN 'SA1'
					ELSE 'SA2'
			END			AS TABELA,
			
				
			COD_CLIENTE
		
		FROM 
			X_TOTVS_CRM_FATURAMENTO WITH (NOLOCK)
		WHERE 
			ORIGEM in 	(	'FAT_PECAS',
							'DEV_PECAS',
							'FAT_SERVICOS',
							'FAT_MAQUINAS',
							'DEV_MAQUINAS'
						)
		AND DATA_EMISSAO_NF BETWEEN @vdatai  AND @vdataf
		AND FILIAL			BETWEEN @filiali  AND @filialf
		AND DELETADO <>'*'
	) MAIN 
	
	GROUP BY 
		TABELA,
		COD_CLIENTE

	--#########################################################################################--
	---------------------------------------------------------------------------------------------
	--#########################################################################################--
	--Tabela 02
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab02%')
	BEGIN
	   DROP TABLE #Tab02
	END

	SELECT 
		TABELA,
		COD,
		CPF_CNPJ,
		NOME,
		
		CASE 
			WHEN TIPO = 'F' 
				THEN 'F' 
			WHEN TIPO = 'J' 
				THEN 'J' 
				ELSE 'O' 
		END 							AS TIPO_PESSOA
		
		INTO #Tab02
	FROM 
		TOTVS.TMPRD.dbo.X_V_COL_CRM_PESSOA
	WHERE 
		CONCAT(TABELA,'|',COD COLLATE Latin1_General_BIN) IN 
								(SELECT CONCAT(TABELA,'|',COD_CLIENTE COLLATE Latin1_General_BIN) FROM #Tab01)
	
	--LIMPEZA TABELAS NÃO MAIS UTILIZADAS
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab01%')
	BEGIN
	   DROP TABLE #Tab01
	END							
	--#########################################################################################--
	---------------------------------------------------------------------------------------------
	--#########################################################################################--
	--Tabela 03
	IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab03%')
	BEGIN
	   DROP TABLE #Tab03
	END							
								
		SELECT 
			--ID, 
			A.ORIGEM, 
			A.FILIAL, 
			A.DATA_EMISSAO_NF, 
			A.NRO_NF, 
			A.NRO_ITEM, 
			A.SERIE, 
			A.COD_CLIENTE, 
			A.CPF_CNPJ, 
			A.COD_MARCA, 
			A.DES_MARCA, 
			A.ID_VENDEDOR, 
			A.COD_OPERACAO, 
			A.DES_OPERACAO, 
			A.CFOP, 
			A.PRODUTO, 
			A.DESC_PRODUTO, 
			A.COD_FAMILIA, 
			A.DESC_FAMILIA, 
			A.NOME_VENDEDOR, 
			A.DATA_PEDIDO, 
			A.STATUS_NF, 
			A.NCM, 
			A.CODICAO_PAGAMENTO, 
			A.DES_CODICAO_PAGAMENTO, 
			A.NRO_PEDIDO, 
			A.OBSERVACAO, 
			A.QUANTIDADE, 
			A.VLR_UNITARIO, 
			A.VALOR_TOTAL, 
			A.VALOR_DESCONTO, 
			A.VLR_LIQUIDO_ITEM, 
			A.VLR_ICMS, 
			A.VLR_ICMS_SUBST, 
			A.VLR_PIS, 
			A.VALOR_COFINS, 
			A.VALOR_CUSTO_MEDIO, 
			A.VALOR_FRETE, 
			A.VALOR_MARGEM, 
			A.PERCENTUAL_MARGEM,
			CASE 
				WHEN ORIGEM in 	('FAT_PECAS','FAT_SERVICOS','FAT_MAQUINAS')
					THEN 'SA1'
					ELSE 'SA2'
			END			AS TABELA			
			
			
			INTO #Tab03
			--A.DELETADO
		FROM 
					X_TOTVS_CRM_FATURAMENTO A WITH (NOLOCK)
		
		WHERE 
		ORIGEM in 	(	'FAT_PECAS',
						'DEV_PECAS',
						'FAT_SERVICOS',
						'FAT_MAQUINAS',
						'DEV_MAQUINAS'
					)
		AND DATA_EMISSAO_NF BETWEEN @vdatai  AND @vdataf
		AND FILIAL			BETWEEN @filiali  AND @filialf
		AND DELETADO <>'*'
		
		
		--SELECT * FROM #Tab03
		
	--#########################################################################################--
	---------------------------------------------------------------------------------------------
	--#########################################################################################--
	
	--RETORNO PROCEDURE
	SELECT 
			--ID, 
			A.ORIGEM, 
			A.FILIAL, 
			A.DATA_EMISSAO_NF, 
			A.NRO_NF, 
			A.NRO_ITEM, 
			A.SERIE, 
			A.COD_CLIENTE, 
			A.CPF_CNPJ, 
			B.NOME,
			B.TIPO_PESSOA,
			C.seq_pessoa 		AS SEQ_PESSOA,
			C.seq_conglo		AS SEQ_CONGLOMERADO,
			C.cnpj_conglo		AS CPF_CNPJ_CONGLOMERADO,
			C.cliente_conglo	AS NOME_CONGLOMERADO,
			A.COD_MARCA, 
			A.DES_MARCA, 
			A.ID_VENDEDOR, 
			A.COD_OPERACAO, 
			A.DES_OPERACAO, 
			A.CFOP, 
			A.PRODUTO, 
			A.DESC_PRODUTO, 
			A.COD_FAMILIA, 
			A.DESC_FAMILIA, 
			A.NOME_VENDEDOR, 
			A.DATA_PEDIDO, 
			A.STATUS_NF, 
			A.NCM, 
			A.CODICAO_PAGAMENTO, 
			A.DES_CODICAO_PAGAMENTO, 
			A.NRO_PEDIDO, 
			A.OBSERVACAO, 
			A.QUANTIDADE, 
			A.VLR_UNITARIO, 
			A.VALOR_TOTAL, 
			A.VALOR_DESCONTO, 
			A.VLR_LIQUIDO_ITEM, 
			A.VLR_ICMS, 
			A.VLR_ICMS_SUBST, 
			A.VLR_PIS, 
			A.VALOR_COFINS, 
			A.VALOR_CUSTO_MEDIO, 
			A.VALOR_FRETE, 
			A.VALOR_MARGEM, 
			A.PERCENTUAL_MARGEM--,
			--A.TABELA			
			
		FROM 
					#Tab03 A
					
		LEFT JOIN 	#Tab02 B 	ON 	A.COD_CLIENTE 	= B.COD 	COLLATE Latin1_General_BIN
								AND A.TABELA		= B.TABELA	COLLATE Latin1_General_BIN
		LEFT JOIN 	X_CRM_BI_CONGLOMERADO	C
								ON	CAST(TRIM(A.CPF_CNPJ) AS BIGINT)
													= CAST(TRIM(C.cnpj_pessoa) AS BIGINT) 	--COLLATE Latin1_General_BIN
								AND B.TIPO_PESSOA	= C.tipo_pessoa							COLLATE Latin1_General_BIN
								AND C.cnpj_pessoa is not null
								
		WHERE 
			B.NOME IS NOT NULL
			
		--select 
		
		UNION ALL
		
		SELECT 
				MAIN.ORIGEM, 
				MAIN.FILIAL, 
				MAIN.DATA_EMISSAO_NF, 
				MAIN.NRO_NF, 
				MAIN.NRO_ITEM, 
				MAIN.SERIE, 
				MAIN.COD_CLIENTE, 
				MAIN.CPF_CNPJ, 			
				B.NOME,
				B.TIPO_PESSOA,
				C.seq_pessoa 		AS SEQ_PESSOA,
				C.seq_conglo		AS SEQ_CONGLOMERADO,
				C.cnpj_conglo		AS CPF_CNPJ_CONGLOMERADO,
				C.cliente_conglo	AS NOME_CONGLOMERADO,
				MAIN.COD_MARCA, 
				MAIN.DES_MARCA, 
				MAIN.ID_VENDEDOR, 
				MAIN.COD_OPERACAO, 
				MAIN.DES_OPERACAO, 
				MAIN.CFOP, 
				MAIN.PRODUTO, 
				MAIN.DESC_PRODUTO, 
				MAIN.COD_FAMILIA, 
				MAIN.DESC_FAMILIA, 
				MAIN.NOME_VENDEDOR, 
				MAIN.DATA_PEDIDO, 
				MAIN.STATUS_NF, 
				MAIN.NCM, 
				MAIN.CODICAO_PAGAMENTO, 
				MAIN.DES_CODICAO_PAGAMENTO, 
				MAIN.NRO_PEDIDO, 
				MAIN.OBSERVACAO, 
				MAIN.QUANTIDADE, 
				MAIN.VLR_UNITARIO, 
				MAIN.VALOR_TOTAL, 
				MAIN.VALOR_DESCONTO, 
				MAIN.VLR_LIQUIDO_ITEM, 
				MAIN.VLR_ICMS, 
				MAIN.VLR_ICMS_SUBST, 
				MAIN.VLR_PIS, 
				MAIN.VALOR_COFINS, 
				MAIN.VALOR_CUSTO_MEDIO, 
				MAIN.VALOR_FRETE, 
				MAIN.VALOR_MARGEM, 
				MAIN.PERCENTUAL_MARGEM--,
				--MAIN.TABELA	
		FROM 
			(
			SELECT 
				--ID, 
				A.ORIGEM, 
				A.FILIAL, 
				A.DATA_EMISSAO_NF, 
				A.NRO_NF, 
				A.NRO_ITEM, 
				A.SERIE, 
				A.COD_CLIENTE, 
				A.CPF_CNPJ, 
				B.NOME,
				B.TIPO_PESSOA,
				C.seq_pessoa 		AS SEQ_PESSOA,
				C.seq_conglo		AS SEQ_CONGLOMERADO,
				C.cnpj_conglo		AS CPF_CNPJ_CONGLOMERADO,
				C.cliente_conglo	AS NOME_CONGLOMERADO,
				A.COD_MARCA, 
				A.DES_MARCA, 
				A.ID_VENDEDOR, 
				A.COD_OPERACAO, 
				A.DES_OPERACAO, 
				A.CFOP, 
				A.PRODUTO, 
				A.DESC_PRODUTO, 
				A.COD_FAMILIA, 
				A.DESC_FAMILIA, 
				A.NOME_VENDEDOR, 
				A.DATA_PEDIDO, 
				A.STATUS_NF, 
				A.NCM, 
				A.CODICAO_PAGAMENTO, 
				A.DES_CODICAO_PAGAMENTO, 
				A.NRO_PEDIDO, 
				A.OBSERVACAO, 
				A.QUANTIDADE, 
				A.VLR_UNITARIO, 
				A.VALOR_TOTAL, 
				A.VALOR_DESCONTO, 
				A.VLR_LIQUIDO_ITEM, 
				A.VLR_ICMS, 
				A.VLR_ICMS_SUBST, 
				A.VLR_PIS, 
				A.VALOR_COFINS, 
				A.VALOR_CUSTO_MEDIO, 
				A.VALOR_FRETE, 
				A.VALOR_MARGEM, 
				A.PERCENTUAL_MARGEM--,
				--A.TABELA			
				
			FROM 
						#Tab03 A			
			LEFT JOIN 	#Tab02 B 	ON 	A.COD_CLIENTE 	= B.COD 	COLLATE Latin1_General_BIN
									AND A.TABELA		= B.TABELA	COLLATE Latin1_General_BIN
			LEFT JOIN 	X_CRM_BI_CONGLOMERADO	C
						ON	CAST(TRIM(A.CPF_CNPJ) AS BIGINT)		
											= CAST(TRIM(C.cnpj_pessoa) AS BIGINT) 	--COLLATE Latin1_General_BIN
						AND B.TIPO_PESSOA	= C.tipo_pessoa							COLLATE Latin1_General_BIN
						AND C.cnpj_pessoa is not null
			WHERE 
				NOME IS NULL
		)			MAIN 
		LEFT JOIN 	#Tab02 B  	ON 	MAIN.COD_CLIENTE = B.COD COLLATE Latin1_General_BIN
								AND B.TABELA = 'SA1'
		LEFT JOIN 	X_CRM_BI_CONGLOMERADO	C
								--ON	MAIN.CPF_CNPJ	= C.cnpj_pessoa	COLLATE Latin1_General_BIN
								ON 	CAST(TRIM(MAIN.CPF_CNPJ) AS BIGINT)
													= CAST(TRIM(C.cnpj_pessoa) AS BIGINT)
								AND B.TIPO_PESSOA	= C.tipo_pessoa COLLATE Latin1_General_BIN
								AND C.cnpj_pessoa is not null
		--WHERE 
		--	B.NOME IS NULL 
		
		----------------------------------------------------------------------------------------------
		--LIMPEZA TABELAS
		IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab01%')
		BEGIN
		   DROP TABLE #Tab01
		END	

		IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab02%')
		BEGIN
		   DROP TABLE #Tab02
		END		
		
		IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab03%')
		BEGIN
		   DROP TABLE #Tab03
		END	
		----------------------------------------------------------------------------------------------
