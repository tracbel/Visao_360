/* ==============================================================
   Objeto ..........: dbo.X_TOTVS_ATUA_CRM_FATURAMENTO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-09-29 17:22:33
   Modificado em ...: 2024-05-27 22:37:37
   Linhas ..........: 757
   Escreve em tabela: SIM (INSERT, DELETE)
   Alvos de escrita : X_TOTVS_CRM_FATURAMENTO
   Tabelas referidas: LOG_INTEGRACAO_FATURAMENTO_TOTVS, X_TOTVS_CRM_FATURAMENTO
   Outras refs .....: X_V_CRM_FATURAMENTO_MAQUINAS, X_V_CRM_FATURAMENTO_PECAS, X_V_CRM_FATURAMENTO_SERVICOS
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

/*
	EXEC [X_TOTVS_ATUA_CRM_FATURAMENTO]
	
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	select * from CRM.dbo.X_TOTVS_CRM_FATURAMENTO	
	
*/


CREATE   PROCEDURE  [dbo].[X_TOTVS_ATUA_CRM_FATURAMENTO]
AS 

BEGIN
	--TABELA PARA ARMAZENAMENTO DE LOGS
	IF NOT EXISTS(SELECT *FROM sys.tables WHERE name = 'LOG_INTEGRACAO_FATURAMENTO_TOTVS')
		BEGIN 
			CREATE TABLE CRM.dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS (
				ID int NOT NULL,
				Tabela varchar(250) NULL,
				MensagemErro nvarchar(4000) NULL,
				DataHora datetime NULL,
				CONSTRAINT LOG_INTEGRACAO_FATURAMENTO_TOTVS_PK PRIMARY KEY (ID)
	
			)			
		END

	--TABELA PARA ARMAZENAMENTO DOS DADOS
	IF  EXISTS (SELECT *FROM sys.tables WHERE name = 'X_TOTVS_CRM_FATURAMENTO')
		BEGIN
			
			--BUSCA NOVOS REGISTROS - ERP TOTVS FATURAMENTO DE PEÇAS TOTVS
			--variavel controle
			DECLARE @V_filtro DATE 
			DECLARE @V_i int			
			DECLARE @ErrorMessage NVARCHAR(4000)
			
			--Parametro de tempo para retroagir a base
			Set @V_i	= -1
			
			BEGIN TRY
			
				BEGIN TRANSACTION -- Inicia a transação
			
				SELECT @V_filtro = 	case when max(DATA_EMISSAO_NF) is null
										then '2021-07-31'
										else DATEADD(month, @V_i, max(DATA_EMISSAO_NF)) 
									end
				from CRM.dbo.X_TOTVS_CRM_FATURAMENTO where ORIGEM IN ('FAT_PECAS','DEV_PECAS')
				--SELECT @V_filtro
				
				--LIMPEZA REGISTROS 
				DELETE from X_TOTVS_CRM_FATURAMENTO WHERE DATA_EMISSAO_NF > @V_filtro and ORIGEM IN ('FAT_PECAS','DEV_PECAS')
				
				
				----------------------------------------------------------------------------------------------------------------	
				--BUSCA NOVOS REGISTROS - ERP TOTVS
				INSERT INTO CRM.dbo.X_TOTVS_CRM_FATURAMENTO		
				SELECT DISTINCT
				
					origem				AS ORIGEM,
					filial				AS FILIAL,
					data_emissao_nf		AS DATA_EMISSAO_NF,
					nro_nf				AS NRO_NF,
					nro_item			AS NRO_ITEM,
					serie				AS SERIE,
					COD_CLIENTE			AS COD_CLIENTE,
					seq_pessoa			AS CPF_CNPJ,
					XMARPEC				AS COD_MARCA,
					MARCA				AS DES_MARCA,
					id_vendedor			AS ID_VENDEDOR,
					cod_operacao 		AS COD_OPERACAO,
					nome_operacao		AS DES_OPERACAO,
					cfop				AS CFOP,
					cod_produto			AS PRODUTO,
					--------------------------------------------------------
					nome_produto		AS DESC_PRODUTO,
					--------------------------------------------------------
					cod_familia_produto AS COD_FAMILIA, 
					nome_familia_produto 
										AS DESC_FAMILIA,
					--------------------------------------------------------
					usuario				AS NOME_VENDEDOR,				--as 	varchar(42)
					DATA_PEDIDO			AS DATA_PEDIDO,					--as 	date
					STATUS				AS STATUS_NF,					--as 	varchar(1)
					NCM					AS NCM,							--as 	varchar(12)
					condpgto			AS CODICAO_PAGAMENTO,			--as 	varchar(5)
					desc_condpgto		AS DES_CODICAO_PAGAMENTO,		--AS	varchar(20)
					nro_pedido			AS NRO_PEDIDO,					--AS 	varchar(8)
					NULL				AS OBSERVACAO,
					--------------------------------------------------------										
					qtde				AS QUANTIDADE,
					vlr_desconto		AS VLR_UNITARIO,
					vlr_tabela			AS VALOR_TOTAL,
					vlr_desconto		AS VALOR_DESCONTO,
					vlr_liquido_item	AS VLR_LIQUIDO_ITEM,
					vlr_icm				AS VLR_ICMS,
					vlr_icm_subst		AS VLR_ICMS_SUBST,
					vlr_pis				AS VLR_PIS,
					vlr_cofins			AS VALOR_COFINS,
					custo_medio			AS VALOR_CUSTO_MEDIO,
					vlr_frete			AS VALOR_FRETE,
					margem_reais		AS VALOR_MARGEM,
					margem_perc			AS PERCENTUAL_MARGEM,
					DELETADO			as DELETADO
				
					
					--INTO X_TOTVS_BI_FATURAMENTO_POS_VENDAS_N 
					
				FROM 
					TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_PECAS
				WHERE 
					data_emissao_nf > @V_filtro

				COMMIT -- Confirma a transação se tudo foi executado com sucesso
			
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK		-- Reverte a transação em caso de erro

					SET @ErrorMessage = ERROR_MESSAGE()
					--PRINT 'Ocorreu um erro: ' + @ErrorMessage

					INSERT INTO CRM.dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS(Tabela, MensagemErro, DataHora)
					VALUES ('INCREMENTAL TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_PECAS','Ocorreu um erro: ' + @ErrorMessage, GETDATE())
					
			END CATCH
			
			----------------------------------------------------------------------------------------------------------------
			--BUSCA NOVOS REGISTROS - ERP TOTVS FATURAMENTO DE SERVICOS TOTVS

			BEGIN TRY
			
				BEGIN TRANSACTION -- Inicia a transação
								
				SELECT @V_filtro = case when max(DATA_EMISSAO_NF) is null
										then '2021-07-31'
										else DATEADD(month, @V_i, max(DATA_EMISSAO_NF)) 
									end 
				from CRM.dbo.X_TOTVS_CRM_FATURAMENTO where ORIGEM = 'FAT_SERVICOS'
				
				--LIMPEZA REGISTROS 
				DELETE from X_TOTVS_CRM_FATURAMENTO WHERE DATA_EMISSAO_NF > @V_filtro and ORIGEM = 'FAT_SERVICOS'				
				
				INSERT INTO CRM.dbo.X_TOTVS_CRM_FATURAMENTO
				SELECT DISTINCT
				
					origem				AS ORIGEM,
					filial				AS FILIAL,
					data_emissao_nf		AS DATA_EMISSAO_NF,
					nro_nf				AS NRO_NF,
					nro_item			AS NRO_ITEM,
					serie				AS SERIE,
					COD_CLIENTE			AS COD_CLIENTE,
					seq_pessoa			AS CPF_CNPJ,
					XMARPEC				AS COD_MARCA,
					MARCA				AS DES_MARCA,
					id_vendedor			AS ID_VENDEDOR,
					cod_operacao 		AS COD_OPERACAO,
					nome_operacao		AS DES_OPERACAO,
					cfop				AS CFOP,
					cod_produto			AS PRODUTO,
					--------------------------------------------------------
					nome_produto		AS DESC_PRODUTO,
					--------------------------------------------------------
					cod_familia_produto AS COD_FAMILIA, 
					nome_familia_produto 
										AS DESC_FAMILIA,
					--------------------------------------------------------
					usuario				AS NOME_VENDEDOR,				--as 	varchar(42)
					DATA_PEDIDO			AS DATA_PEDIDO,					--as 	date
					STATUS				AS STATUS_NF,					--as 	varchar(1)
					NCM					AS NCM,							--as 	varchar(12)
					condpgto			AS CODICAO_PAGAMENTO,			--as 	varchar(5)
					desc_condpgto		AS DES_CODICAO_PAGAMENTO,		--AS	varchar(20)
					nro_pedido			AS NRO_PEDIDO,					--AS 	varchar(8)
					rua					AS OBSERVACAO,
					--------------------------------------------------------										
					qtde				AS QUANTIDADE,
					vlr_desconto		AS VLR_UNITARIO,
					vlr_tabela			AS VALOR_TOTAL,
					vlr_desconto		AS VALOR_DESCONTO,
					vlr_liquido_item	AS VLR_LIQUIDO_ITEM,
					vlr_icm				AS VLR_ICMS,
					vlr_icm_subst		AS VLR_ICMS_SUBST,
					vlr_pis				AS VLR_PIS,
					vlr_cofins			AS VALOR_COFINS,
					custo_medio			AS VALOR_CUSTO_MEDIO,
					vlr_frete			AS VALOR_FRETE,
					margem_reais		AS VALOR_MARGEM,
					margem_perc			AS PERCENTUAL_MARGEM,
					DELETADO			as DELETADO
						
				FROM 
					TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_SERVICOS	
				WHERE 
					data_emissao_nf > @V_filtro
				and CONCAT(filial,'|',nro_nf,'|',serie,'|',COD_CLIENTE) not in (
																					'9|000000005|S  |008099008|0001',
																					'9|000000006|S  |033656729|0018',
																					'9|000000007|S  |033656729|0018',
																					'9|000000008|S  |033656729|0018'
																					
																					)	

				COMMIT -- Confirma a transação se tudo foi executado com sucesso
			
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK		-- Reverte a transação em caso de erro

					--DECLARE @ErrorMessage NVARCHAR(4000)
					SET @ErrorMessage = ERROR_MESSAGE()
					--PRINT 'Ocorreu um erro: ' + @ErrorMessage

					INSERT INTO CRM.dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS(Tabela, MensagemErro, DataHora)
					VALUES ('INCREMENTAL TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_SERVICOS','Ocorreu um erro: ' + @ErrorMessage, GETDATE())
					
			END CATCH
				
				
			----------------------------------------------------------------------------------------------------------------
			--BUSCA NOVOS REGISTROS - ERP TOTVS FATURAMENTO DE MAQUINAS TOTVS
			BEGIN TRY
			
				BEGIN TRANSACTION -- Inicia a transação					
			
				SELECT @V_filtro = case when max(DATA_EMISSAO_NF) is null
										then '2021-07-31'
										else DATEADD(month, @V_i, max(DATA_EMISSAO_NF)) 
									end
				from CRM.dbo.X_TOTVS_CRM_FATURAMENTO where ORIGEM IN ('FAT_MAQUINAS','DEV_MAQUINAS')
				
				--LIMPEZA REGISTROS 
				DELETE from X_TOTVS_CRM_FATURAMENTO WHERE DATA_EMISSAO_NF > @V_filtro and ORIGEM IN ('FAT_MAQUINAS','DEV_MAQUINAS')				
				
				INSERT INTO CRM.dbo.X_TOTVS_CRM_FATURAMENTO		
				SELECT 	DISTINCT
					CASE 
						WHEN origem IN ('01_FATURAMENTO_MAQUINAS','12_FATURAMENTO_CLIENTES_ESTRATEGICOS')
							THEN 'FAT_MAQUINAS'
						WHEN origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							THEN 'DEV_MAQUINAS'
							ELSE 'OUTROS'					
					END 					AS origem,
					filial					AS FILIAL,
					data_emissao_nf			AS DATA_EMISSAO_NF,
					nro_nf					AS NRO_NF,
					NRO_ITEM				AS NRO_ITEM,					
					serie					AS SERIE,
					COD_CLIENTE				AS COD_CLIENTE,					
					seq_pessoa				AS CPF_CNPJ,
					COD_MARCA				AS COD_MARCA,					
					CAST(DES_MARCA AS VARCHAR(30))
											AS DES_MARCA,					
					id_vendedor				AS ID_VENDEDOR,
					cod_operacao 			AS COD_OPERACAO,
					nome_operacao			AS DES_OPERACAO,
					cfop					AS CFOP,
					cod_produto				AS PRODUTO,
					--------------------------------------------------------
					left(nome_produto,50)	AS DESC_PRODUTO,
					--------------------------------------------------------
					cod_familia_produto 	AS COD_FAMILIA, 
					desc_modelo 			AS DESC_FAMILIA,
					--------------------------------------------------------
					nome_vendedor			AS NOME_VENDEDOR,				
					DATA_PEDIDO				AS DATA_PEDIDO,				
					CASE 
						WHEN origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							THEN 'D'
							ELSE STATUS					
					END						AS STATUS_NF,					
					NCM						AS NCM,							
					condpgto				AS CODICAO_PAGAMENTO,			
					desc_condpgto			AS DES_CODICAO_PAGAMENTO,		
					nro_pedido				AS NRO_PEDIDO,					
					''						AS OBSERVACAO,					
					--------------------------------------------------------
					Case 
						when qtde < 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(qtde)
							else qtde
					END						AS QUANTIDADE,
					Case 
						when vlr_unitario < 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_unitario)
							else vlr_unitario
					END						AS VLR_UNITARIO,
					Case 
						when (qtde * vlr_unitario)< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs((qtde * vlr_unitario))
							else (qtde * vlr_unitario)
					END						AS VALOR_TOTAL,
					Case 
						when vlr_desconto< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_desconto)
							else vlr_desconto
					END						AS VALOR_DESCONTO,
					Case 
						when vlr_liquido_item< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_liquido_item)
							else vlr_liquido_item
					END						AS VLR_LIQUIDO_ITEM,
					Case 
						when vlr_icm< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_icm)
							else vlr_icm
					END						AS VLR_ICMS,
					Case 
						when vlr_icm_subst< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_icm_subst)
							else vlr_icm_subst
					END						AS VLR_ICMS_SUBST,
					Case 
						when vlr_pis< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_pis)
							else vlr_pis
					END						AS VLR_PIS,
					Case 
						when vlr_cofins< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_cofins)
							else vlr_cofins
					END						AS VALOR_COFINS,
					Case 
						when custo_medio< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(custo_medio)
							else custo_medio
					END						AS VALOR_CUSTO_MEDIO,
					0						AS VALOR_FRETE,
					Case 
						when margem_reais< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then (margem_reais *-1)
							else margem_reais
					END						AS VALOR_MARGEM,
					Case 
						when margem_perc< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then (margem_perc *-1)
							else margem_perc
					END						AS PERCENTUAL_MARGEM,
					-------------------------------------------------------
					DELETADO
					-------------------------------------------------------
				FROM 
					TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_MAQUINAS 
				WHERE 
					data_emissao_nf > @V_filtro
				AND origem_fonte IN ('01_MAQUINAS','04_CLIENTES_ESTRATEGICOS')
				AND origem in (
								'01_FATURAMENTO_MAQUINAS',
								'02_DEVOLUÇÃO_MAQUINAS',
								'12_FATURAMENTO_CLIENTES_ESTRATEGICOS',
								'13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS'
				)
				
				COMMIT -- Confirma a transação se tudo foi executado com sucesso
			
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK		-- Reverte a transação em caso de erro

					--DECLARE @ErrorMessage NVARCHAR(4000)
					SET @ErrorMessage = ERROR_MESSAGE()
					--PRINT 'Ocorreu um erro: ' + @ErrorMessage

					INSERT INTO CRM.dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS(Tabela, MensagemErro, DataHora)
					VALUES ('INCREMENTAL TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_MAQUINAS','Ocorreu um erro: ' + @ErrorMessage, GETDATE())
					
			END CATCH
			
			----------------------------------------------------------------------------------------------------------------
		END
	ELSE 
		BEGIN
			
			--CRIACAO DA TABELA BASE.
			CREATE TABLE CRM.dbo.X_TOTVS_CRM_FATURAMENTO (
				ID int IDENTITY(1,1) NOT NULL,

				ORIGEM varchar(12) COLLATE Latin1_General_CI_AS NOT NULL,
				FILIAL int NULL,
				DATA_EMISSAO_NF date NULL,
				NRO_NF varchar(9) COLLATE Latin1_General_CI_AS NOT NULL,
				NRO_ITEM varchar(5) COLLATE Latin1_General_CI_AS NOT NULL,
				SERIE varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
				COD_CLIENTE varchar(14) COLLATE Latin1_General_CI_AS NULL,
				CPF_CNPJ varchar(14) COLLATE Latin1_General_CI_AS NULL,
				COD_MARCA varchar(6) COLLATE Latin1_General_CI_AS NULL,
				DES_MARCA varchar(30) COLLATE Latin1_General_CI_AS NULL,
				ID_VENDEDOR varchar(6) COLLATE Latin1_General_CI_AS NOT NULL,
				COD_OPERACAO varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
				DES_OPERACAO varchar(20) COLLATE Latin1_General_CI_AS NULL,
				CFOP varchar(5) COLLATE Latin1_General_CI_AS NULL,
				PRODUTO varchar(28) COLLATE Latin1_General_CI_AS NULL,
				--------------------------------------------------------
				DESC_PRODUTO varchar(50) COLLATE Latin1_General_CI_AS NULL,
				--------------------------------------------------------
				COD_FAMILIA varchar(10) COLLATE Latin1_General_CI_AS NULL,
				DESC_FAMILIA varchar(80) COLLATE Latin1_General_CI_AS NULL,
				--------------------------------------------------------
				NOME_VENDEDOR varchar(42) COLLATE Latin1_General_CI_AS NULL,
				DATA_PEDIDO	date NULL,
				STATUS_NF varchar(1) COLLATE Latin1_General_CI_AS NULL,
				NCM	varchar(12) COLLATE Latin1_General_CI_AS NULL,
				CODICAO_PAGAMENTO varchar(5) COLLATE Latin1_General_CI_AS NULL,
				DES_CODICAO_PAGAMENTO varchar(40) COLLATE Latin1_General_CI_AS NULL,
				NRO_PEDIDO varchar(8) COLLATE Latin1_General_CI_AS NULL,
				OBSERVACAO varchar(30) COLLATE Latin1_General_CI_AS NULL,
				--------------------------------------------------------
				QUANTIDADE float NOT NULL,
				VLR_UNITARIO float NOT NULL,
				VALOR_TOTAL float NOT NULL,
				VALOR_DESCONTO float NOT NULL,
				VLR_LIQUIDO_ITEM float NOT NULL,
				VLR_ICMS float NOT NULL,
				VLR_ICMS_SUBST float NOT NULL,
				VLR_PIS float NOT NULL,
				VALOR_COFINS float NOT NULL,
				VALOR_CUSTO_MEDIO float NOT NULL,
				VALOR_FRETE float NOT NULL,
				VALOR_MARGEM float NOT NULL,
				PERCENTUAL_MARGEM float NOT NULL,
				DELETADO varchar(1) COLLATE Latin1_General_CI_AS NULL
				CONSTRAINT X_TOTVS_CRM_FATURAMENTO_PK PRIMARY KEY (ID),
				CONSTRAINT X_TOTVS_CRM_FATURAMENTO_UN_CHAVE_ITEM UNIQUE (STATUS_NF,ORIGEM,FILIAL,NRO_NF,SERIE,COD_CLIENTE,COD_OPERACAO,PRODUTO,NRO_ITEM,DELETADO)
			)
				
			 CREATE NONCLUSTERED INDEX X_TOTVS_CRM_FATURAMENTO_ID_COD_CHAVE_ITEM ON dbo.X_TOTVS_CRM_FATURAMENTO (  ID ASC  , STATUS_NF ASC  , ORIGEM ASC  , FILIAL ASC  , NRO_NF ASC  , SERIE ASC  , COD_CLIENTE ASC  , COD_OPERACAO ASC  , PRODUTO ASC  , NRO_ITEM ASC  , DELETADO ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
			 CREATE NONCLUSTERED INDEX X_TOTVS_CRM_FATURAMENTO_ID_COD_CHAVE_NF ON dbo.X_TOTVS_CRM_FATURAMENTO (  ID ASC  , STATUS_NF ASC  , ORIGEM ASC  , FILIAL ASC  , NRO_NF ASC  , SERIE ASC  , COD_CLIENTE ASC  , DELETADO ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
			 CREATE NONCLUSTERED INDEX X_TOTVS_CRM_FATURAMENTO_ID_DATA ON dbo.X_TOTVS_CRM_FATURAMENTO (  ID ASC  , DATA_EMISSAO_NF ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
			 CREATE NONCLUSTERED INDEX X_TOTVS_CRM_FATURAMENTO_ID_IDX ON dbo.X_TOTVS_CRM_FATURAMENTO (  ID ASC  , ORIGEM ASC  , FILIAL ASC  , NRO_NF ASC  , COD_CLIENTE ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
			 CREATE NONCLUSTERED INDEX X_TOTVS_CRM_FATURAMENTO_ID_ORIGEM_DATA ON dbo.X_TOTVS_CRM_FATURAMENTO (  ID ASC  , ORIGEM ASC  , DATA_EMISSAO_NF ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
			 CREATE NONCLUSTERED INDEX X_TOTVS_CRM_FATURAMENTO_SUGES_01 ON dbo.X_TOTVS_CRM_FATURAMENTO (  NRO_NF ASC  , DATA_EMISSAO_NF ASC  )  
				 INCLUDE ( CFOP , COD_CLIENTE , COD_FAMILIA , COD_MARCA , COD_OPERACAO , CODICAO_PAGAMENTO , CPF_CNPJ , DATA_PEDIDO , DELETADO , DES_CODICAO_PAGAMENTO , DES_MARCA , DES_OPERACAO , DESC_FAMILIA , DESC_PRODUTO , FILIAL , ID_VENDEDOR , NCM , NOME_VENDEDOR , NRO_ITEM , NRO_PEDIDO , OBSERVACAO , ORIGEM , PERCENTUAL_MARGEM , PRODUTO , QUANTIDADE , SERIE , STATUS_NF , VALOR_COFINS , VALOR_CUSTO_MEDIO , VALOR_DESCONTO , VALOR_FRETE , VALOR_MARGEM , VALOR_TOTAL , VLR_ICMS , VLR_ICMS_SUBST , VLR_LIQUIDO_ITEM , VLR_PIS , VLR_UNITARIO ) 
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 			
		 
			 
			----------------------------------------------------------------------------------------------------------------
			--BUSCA FULL REGISTROS - ERP TOTVS FATURAMENTO DE PEÇAS TOTVS
			BEGIN TRY
			
				BEGIN TRANSACTION -- Inicia a transação	
							 
				INSERT INTO CRM.dbo.X_TOTVS_CRM_FATURAMENTO		
				SELECT DISTINCT
		
					origem				AS ORIGEM,
					filial				AS FILIAL,
					data_emissao_nf		AS DATA_EMISSAO_NF,
					nro_nf				AS NRO_NF,
					nro_item			AS NRO_ITEM,
					serie				AS SERIE,
					COD_CLIENTE			AS COD_CLIENTE,
					seq_pessoa			AS CPF_CNPJ,
					XMARPEC				AS COD_MARCA,
					MARCA				AS DES_MARCA,
					id_vendedor			AS ID_VENDEDOR,
					cod_operacao 		AS COD_OPERACAO,
					nome_operacao		AS DES_OPERACAO,
					cfop				AS CFOP,
					cod_produto			AS PRODUTO,
					--------------------------------------------------------
					nome_produto		AS DESC_PRODUTO,
					--------------------------------------------------------
					cod_familia_produto AS COD_FAMILIA, 
					nome_familia_produto 
										AS DESC_FAMILIA,
					--------------------------------------------------------
					usuario				AS NOME_VENDEDOR,				--as 	varchar(42)
					DATA_PEDIDO			AS DATA_PEDIDO,					--as 	date
					STATUS				AS STATUS_NF,					--as 	varchar(1)
					NCM					AS NCM,							--as 	varchar(12)
					condpgto			AS CODICAO_PAGAMENTO,			--as 	varchar(5)
					desc_condpgto		AS DES_CODICAO_PAGAMENTO,		--AS	varchar(20)
					nro_pedido			AS NRO_PEDIDO,					--AS 	varchar(8)
					NULL				AS OBSERVACAO,
					--------------------------------------------------------									
					qtde				AS QUANTIDADE,
					vlr_desconto		AS VLR_UNITARIO,
					vlr_tabela			AS VALOR_TOTAL,
					vlr_desconto		AS VALOR_DESCONTO,
					vlr_liquido_item	AS VLR_LIQUIDO_ITEM,
					vlr_icm				AS VLR_ICMS,
					vlr_icm_subst		AS VLR_ICMS_SUBST,
					vlr_pis				AS VLR_PIS,
					vlr_cofins			AS VALOR_COFINS,
					custo_medio			AS VALOR_CUSTO_MEDIO,
					vlr_frete			AS VALOR_FRETE,
					margem_reais		AS VALOR_MARGEM,
					margem_perc			AS PERCENTUAL_MARGEM,
					DELETADO			AS DELETADO
					--INTO X_TOTVS_BI_FATURAMENTO_POS_VENDAS_N 
					
				FROM 	
					TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_PECAS

				COMMIT -- Confirma a transação se tudo foi executado com sucesso
			
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK		-- Reverte a transação em caso de erro

					--DECLARE @ErrorMessage NVARCHAR(4000)
					SET @ErrorMessage = ERROR_MESSAGE()
					--PRINT 'Ocorreu um erro: ' + @ErrorMessage

					INSERT INTO CRM.dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS(Tabela, MensagemErro, DataHora)
					VALUES ('FULL TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_PECAS','Ocorreu um erro: ' + @ErrorMessage, GETDATE())
					
			END CATCH
	
			----------------------------------------------------------------------------------------------------------------
			--BUSCA FULL REGISTROS - ERP TOTVS FATURAMENTO DE SERVICOS TOTVS
			BEGIN TRY
			
				BEGIN TRANSACTION -- Inicia a transação	
					
				INSERT INTO CRM.dbo.X_TOTVS_CRM_FATURAMENTO
				SELECT DISTINCT
		
					origem				AS ORIGEM,
					filial				AS FILIAL,
					data_emissao_nf		AS DATA_EMISSAO_NF,
					nro_nf				AS NRO_NF,
					nro_item			AS NRO_ITEM,
					serie				AS SERIE,
					COD_CLIENTE			AS COD_CLIENTE,
					seq_pessoa			AS CPF_CNPJ,
					XMARPEC				AS COD_MARCA,
					MARCA				AS DES_MARCA,
					id_vendedor			AS ID_VENDEDOR,
					cod_operacao 		AS COD_OPERACAO,
					nome_operacao		AS DES_OPERACAO,
					cfop				AS CFOP,
					cod_produto			AS PRODUTO,
					--------------------------------------------------------
					nome_produto		AS DESC_PRODUTO,
					--------------------------------------------------------
					cod_familia_produto AS COD_FAMILIA, 
					nome_familia_produto 
										AS DESC_FAMILIA,
					--------------------------------------------------------
					usuario				AS NOME_VENDEDOR,				--as 	varchar(42)
					DATA_PEDIDO			AS DATA_PEDIDO,					--as 	date
					STATUS				AS STATUS_NF,					--as 	varchar(1)
					NCM					AS NCM,							--as 	varchar(12)
					condpgto			AS CODICAO_PAGAMENTO,			--as 	varchar(5)
					desc_condpgto		AS DES_CODICAO_PAGAMENTO,		--AS	varchar(20)
					nro_pedido			AS NRO_PEDIDO,					--AS 	varchar(8)
					rua					AS OBSERVACAO,
					--------------------------------------------------------
					qtde				AS QUANTIDADE,
					vlr_desconto		AS VLR_UNITARIO,
					vlr_tabela			AS VALOR_TOTAL,
					vlr_desconto		AS VALOR_DESCONTO,
					vlr_liquido_item	AS VLR_LIQUIDO_ITEM,
					vlr_icm				AS VLR_ICMS,
					vlr_icm_subst		AS VLR_ICMS_SUBST,
					vlr_pis				AS VLR_PIS,
					vlr_cofins			AS VALOR_COFINS,
					custo_medio			AS VALOR_CUSTO_MEDIO,
					vlr_frete			AS VALOR_FRETE,
					margem_reais		AS VALOR_MARGEM,
					margem_perc			AS PERCENTUAL_MARGEM,
					DELETADO			AS DELETADO
	
					--INTO X_TOTVS_BI_FATURAMENTO_POS_VENDAS_N
					
				FROM 
					TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_SERVICOS	
				where
				--erros bebedouro
					CONCAT(filial,'|',nro_nf,'|',serie,'|',COD_CLIENTE) not in (
																					'9|000000005|S  |008099008|0001',
																					'9|000000006|S  |033656729|0018',
																					'9|000000007|S  |033656729|0018',
																					'9|000000008|S  |033656729|0018'
																					
																					)	

				COMMIT -- Confirma a transação se tudo foi executado com sucesso
			
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK		-- Reverte a transação em caso de erro

					--DECLARE @ErrorMessage NVARCHAR(4000)
					SET @ErrorMessage = ERROR_MESSAGE()
					--PRINT 'Ocorreu um erro: ' + @ErrorMessage

					INSERT INTO CRM.dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS(Tabela, MensagemErro, DataHora)
					VALUES ('FULL TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_SERVICOS','Ocorreu um erro: ' + @ErrorMessage, GETDATE())
					
			END CATCH
		
			----------------------------------------------------------------------------------------------------------------
			--BUSCA FULL REGISTROS - ERP TOTVS FATURAMENTO DE MAQUINAS TOTVS
			BEGIN TRY
			
				BEGIN TRANSACTION -- Inicia a transação	
					
				INSERT INTO CRM.dbo.X_TOTVS_CRM_FATURAMENTO
				SELECT 	 DISTINCT
					CASE 
						WHEN origem IN ('01_FATURAMENTO_MAQUINAS','12_FATURAMENTO_CLIENTES_ESTRATEGICOS')
							THEN 'FAT_MAQUINAS'
						WHEN origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							THEN 'DEV_MAQUINAS'
							ELSE 'OUTROS'
						
					END 					AS origem,
					filial					AS FILIAL,
					data_emissao_nf			AS DATA_EMISSAO_NF,
					nro_nf					AS NRO_NF,
					NRO_ITEM				AS NRO_ITEM,					
					serie					AS SERIE,
					COD_CLIENTE				AS COD_CLIENTE,					
					seq_pessoa				AS CPF_CNPJ,
					COD_MARCA				AS COD_MARCA,					
					CAST(DES_MARCA AS VARCHAR(30))
											AS DES_MARCA,					
					id_vendedor				AS ID_VENDEDOR,
					cod_operacao 			AS COD_OPERACAO,
					nome_operacao			AS DES_OPERACAO,
					cfop					AS CFOP,
					cod_produto				AS PRODUTO,
					--------------------------------------------------------
					left(nome_produto,50)	AS DESC_PRODUTO,
					--------------------------------------------------------
					cod_familia_produto 	AS COD_FAMILIA, 
					desc_modelo 			AS DESC_FAMILIA,
					--------------------------------------------------------
					nome_vendedor			AS NOME_VENDEDOR,				
					DATA_PEDIDO				AS DATA_PEDIDO,				
					CASE 
						WHEN origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							THEN 'D'
							ELSE STATUS					
					END						AS STATUS_NF,					
					NCM						AS NCM,							
					condpgto				AS CODICAO_PAGAMENTO,			
					desc_condpgto			AS DES_CODICAO_PAGAMENTO,		
					nro_pedido				AS NRO_PEDIDO,					
					''						AS OBSERVACAO,					
					--------------------------------------------------------
					Case 
						when qtde < 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(qtde)
							else qtde
					END						AS QUANTIDADE,
					Case 
						when vlr_unitario < 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_unitario)
							else vlr_unitario
					END						AS VLR_UNITARIO,
					Case 
						when (qtde * vlr_unitario)< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs((qtde * vlr_unitario))
							else (qtde * vlr_unitario)
					END						AS VALOR_TOTAL,
					Case 
						when vlr_desconto< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_desconto)
							else vlr_desconto
					END						AS VALOR_DESCONTO,
					Case 
						when vlr_liquido_item< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_liquido_item)
							else vlr_liquido_item
					END						AS VLR_LIQUIDO_ITEM,
					Case 
						when vlr_icm< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_icm)
							else vlr_icm
					END						AS VLR_ICMS,
					Case 
						when vlr_icm_subst< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_icm_subst)
							else vlr_icm_subst
					END						AS VLR_ICMS_SUBST,
					Case 
						when vlr_pis< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_pis)
							else vlr_pis
					END						AS VLR_PIS,
					Case 
						when vlr_cofins< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(vlr_cofins)
							else vlr_cofins
					END						AS VALOR_COFINS,
					Case 
						when custo_medio< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then abs(custo_medio)
							else custo_medio
					END						AS VALOR_CUSTO_MEDIO,
					0						AS VALOR_FRETE,
					Case 
						when margem_reais< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then (margem_reais *-1)
							else margem_reais
					END						AS VALOR_MARGEM,
					Case 
						when margem_perc< 0 and origem IN ('02_DEVOLUÇÃO_MAQUINAS','13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS')
							then (margem_perc *-1)
							else margem_perc
					END						AS PERCENTUAL_MARGEM,
					-------------------------------------------------------
					DELETADO
					-------------------------------------------------------
				FROM 
					TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_MAQUINAS 
				WHERE 
					--data_emissao_nf >= '2023-10-01' AND
				 	origem_fonte IN ('01_MAQUINAS','04_CLIENTES_ESTRATEGICOS')
				AND origem in (
								'01_FATURAMENTO_MAQUINAS',
								'02_DEVOLUÇÃO_MAQUINAS',
								'12_FATURAMENTO_CLIENTES_ESTRATEGICOS',
								'13_DEVOLUÇÃO_CLIENTES_ESTRATEGICOS'
				)	

				COMMIT -- Confirma a transação se tudo foi executado com sucesso
			
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK		-- Reverte a transação em caso de erro

					--DECLARE @ErrorMessage NVARCHAR(4000)
					SET @ErrorMessage = ERROR_MESSAGE()
					--PRINT 'Ocorreu um erro: ' + @ErrorMessage

					INSERT INTO CRM.dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS(Tabela, MensagemErro, DataHora)
					VALUES ('FULL TOTVS.TMPRD.dbo.X_V_CRM_FATURAMENTO_MAQUINAS','Ocorreu um erro: ' + @ErrorMessage, GETDATE())
					
			END CATCH
		
					
		END
----------------------------------------------------------------------------------------------------------------------------		
END
