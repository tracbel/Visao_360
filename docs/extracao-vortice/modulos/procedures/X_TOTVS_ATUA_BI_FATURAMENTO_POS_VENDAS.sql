/* ==============================================================
   Objeto ..........: dbo.X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-07-10 23:07:54
   Modificado em ...: 2023-09-29 16:57:08
   Linhas ..........: 286
   Escreve em tabela: SIM (INSERT, DELETE)
   Alvos de escrita : X_TOTVS_BI_FATURAMENTO_POS_VENDAS
   Tabelas referidas: X_TOTVS_BI_FATURAMENTO_POS_VENDAS
   Outras refs .....: X_V_BI_FATURAMENTO_PECAS, X_V_BI_FATURAMENTO_SERVICOS
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

/*

GRANT SELECT ON [X_V_BI_FATURAMENTO_PECAS] TO [crm];

Exec [dbo].[X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS]

--DROP PROC [dbo].[X_TOTVS_BI_FATURAMENTO_POS_VENDAS]

*/


CREATE   PROCEDURE  [dbo].[X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS]
AS 
BEGIN

	IF  EXISTS (SELECT *FROM sys.tables WHERE name = 'X_TOTVS_BI_FATURAMENTO_POS_VENDAS')
		BEGIN
			
			--variavel controle
			DECLARE @V_filtro DATE 
			DECLARE @V_i int
			
			--Parametro de tempo para retroagir a base
			Set @V_i	= -1
			
			SELECT @V_filtro = DATEADD(month, @V_i, max(DATA_EMISSAO_NF)) from CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS where ORIGEM IN ('FAT_PECAS','DEV_PECAS')
			
			--SELECT @V_filtro
			--LIMPEZA REGISTROS 
			DELETE from X_TOTVS_BI_FATURAMENTO_POS_VENDAS WHERE DATA_EMISSAO_NF >= @V_filtro and ORIGEM IN ('FAT_PECAS','DEV_PECAS')	
			
			
			----------------------------------------------------------------------------------------------------------------	
			--BUSCA NOVOS REGISTROS - ERP TOTVS
			INSERT INTO CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS		
			SELECT
				--TOP 10
				origem				AS ORIGEM,
				filial				AS FILIAL,
				data_emissao_nf		AS DATA_EMISSAO_NF,
				nro_nf				AS NRO_NF,
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
				cod_familia_produto AS COD_FAMILIA, 
				nome_familia_produto 
									AS DESC_FAMILIA,
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
				margem_perc			AS PERCENTUAL_MARGEM
				
				--INTO X_TOTVS_BI_FATURAMENTO_POS_VENDAS 
				
			FROM 
				TOTVS.TMPRD.dbo.X_V_BI_FATURAMENTO_PECAS
			WHERE 
				data_emissao_nf>= '2021-08-01'
			and	data_emissao_nf >= @V_filtro
				--data_emissao_nf >= '2023-08-05'
			----------------------------------------------------------------------------------------------------------------
			
			--variavel controle
			--DECLARE @V_filtro DATE 
			--DECLARE @V_i int
			
			--Parametro de tempo para retroagir a base
			--Set @V_i	= -2
			
			SELECT @V_filtro = DATEADD(month, @V_i, max(DATA_EMISSAO_NF)) from CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS where ORIGEM = 'FAT_SERVICOS'
			
			
			--LIMPEZA REGISTROS 
			DELETE from X_TOTVS_BI_FATURAMENTO_POS_VENDAS WHERE DATA_EMISSAO_NF >= @V_filtro and ORIGEM = 'FAT_SERVICOS'				
			
			
			
			INSERT INTO CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS
			SELECT 
				--TOP 10
				origem				AS ORIGEM,
				filial				AS FILIAL,
				data_emissao_nf		AS DATA_EMISSAO_NF,
				nro_nf				AS NRO_NF,
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
				cod_familia_produto AS COD_FAMILIA, 
				nome_familia_produto 
									AS DESC_FAMILIA,
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
				margem_perc			AS PERCENTUAL_MARGEM
				
			FROM 
				TOTVS.TMPRD.dbo.X_V_BI_FATURAMENTO_SERVICOS	
			WHERE 
				data_emissao_nf>= '2021-08-01'
			AND	data_emissao_nf >= @V_filtro
				 
			----------------------------------------------------------------------------------------------------------------			
		
		END
	ELSE 
		BEGIN
			
			--CRIACAO DA TABELA BASE.
			CREATE TABLE CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS (
				ID int IDENTITY(1,1) NOT NULL,
				ORIGEM varchar(12) COLLATE Latin1_General_CI_AS NOT NULL,
				FILIAL int NULL,
				DATA_EMISSAO_NF date NULL,
				NRO_NF varchar(9) COLLATE Latin1_General_CI_AS NOT NULL,
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
				COD_FAMILIA varchar(10) COLLATE Latin1_General_CI_AS NULL,
				DESC_FAMILIA varchar(80) COLLATE Latin1_General_CI_AS NULL,
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
				PERCENTUAL_MARGEM float NOT NULL
				CONSTRAINT PK__X_TOTVS___3214EC2781A7BF54 PRIMARY KEY (ID)
			)
			
			 CREATE NONCLUSTERED INDEX X_TOTVS_BI_FATURAMENTO_POS_VENDAS_ID_DATA ON dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS (  ID ASC  , DATA_EMISSAO_NF ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
			 CREATE NONCLUSTERED INDEX X_TOTVS_BI_FATURAMENTO_POS_VENDAS_ID_IDX ON dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS (  ID ASC  , ORIGEM ASC  , FILIAL ASC  , NRO_NF ASC  , COD_CLIENTE ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
			 CREATE NONCLUSTERED INDEX X_TOTVS_BI_FATURAMENTO_POS_VENDAS_ID_ORIGEM_DATA ON dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS (  ID ASC  , ORIGEM ASC  , DATA_EMISSAO_NF ASC  )  
				 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
				 ON [PRIMARY ] 
	
		 
			 
			----------------------------------------------------------------------------------------------------------------
			--BUSCA NOVOS REGISTROS - ERP TOTVS FATURAMENTO DE PEÇAS TOTVS
			INSERT INTO CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS		
			SELECT
				--TOP 10
				origem				AS ORIGEM,
				filial				AS FILIAL,
				data_emissao_nf		AS DATA_EMISSAO_NF,
				nro_nf				AS NRO_NF,
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
				cod_familia_produto AS COD_FAMILIA, 
				nome_familia_produto 
									AS DESC_FAMILIA,			
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
				margem_perc			AS PERCENTUAL_MARGEM
				
				--INTO X_TOTVS_BI_FATURAMENTO_POS_VENDAS 
				
			FROM 	
				TOTVS.TMPRD.dbo.X_V_BI_FATURAMENTO_PECAS
			WHERE 
				data_emissao_nf>= '2021-08-01'
	
			----------------------------------------------------------------------------------------------------------------
			--BUSCA NOVOS REGISTROS - ERP TOTVS FATURAMENTO DE PEÇAS TOTVS	
			INSERT INTO CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS
			SELECT 
				--TOP 10
				origem				AS ORIGEM,
				filial				AS FILIAL,
				data_emissao_nf		AS DATA_EMISSAO_NF,
				nro_nf				AS NRO_NF,
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
				cod_familia_produto AS COD_FAMILIA, 
				nome_familia_produto 
									AS DESC_FAMILIA,
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
				margem_perc			AS PERCENTUAL_MARGEM
				--INTO X_TOTVS_BI_FATURAMENTO_POS_VENDAS2
				
			FROM 
				TOTVS.TMPRD.dbo.X_V_BI_FATURAMENTO_SERVICOS	
			WHERE 
				data_emissao_nf>= '2021-08-01'
			----------------------------------------------------------------------------------------------------------------
--		select count(ID) from X_TOTVS_BI_FATURAMENTO_POS_VENDAS;
--		select * from X_TOTVS_BI_FATURAMENTO_POS_VENDAS WHERE ORIGEM = 'FAT_SERVICOS' ORDER BY DATA_EMISSAO_NF  DESC;
--		select count(ID) from X_TOTVS_BI_FATURAMENTO_POS_VENDAS WHERE ORIGEM = 'FAT_SERVICOS';
					
		END
----------------------------------------------------------------------------------------------------------------------------		
END


	
	
	
	
	
	