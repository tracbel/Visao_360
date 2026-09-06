/* ==============================================================
   Objeto ..........: dbo.X_V_CRM_IMP_NFSItem
   Tipo ............: VIEW
   Criado em .......: 2023-09-12 08:51:56
   Modificado em ...: 2023-10-04 13:11:59
   Linhas ..........: 99
   Escreve em tabela: nao
   Tabelas referidas: X_TOTVS_CRM_FATURAMENTO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

/*

  select top 100
  	--ORIGEM
  	* 
  from CRM.dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS_N
  where DATA_EMISSAO_NF > '2023-08-01' and ORIGEM not in ('DEV_PECAS')
  --group by ORIGEM
  ;

  select top 100 * from 
  TOTVS.TMPRD.dbo.X_V_BI_FATURAMENTO_PECAS where nro_nf = '000048351'



*/
CREATE   VIEW  [X_V_CRM_IMP_NFSItem] AS
	--VIEW CRIADA NO DIA 12/09/2023 -  PARA ATENDER AO NOVO PROCESSO DE INTEGRACAO DE FATURAMENTO DO POS VENDAS COM O CRM.

	SELECT	--TOP 10
		CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',COD_OPERACAO,'|',PRODUTO,'|', NRO_ITEM,'|',DELETADO)
											AS COD_CHAVE_ITEM,
		NULL  								as IdNFSItem,   	--|numeric(18,0)
		'Protheus'  						as Origem,    		--ORIGEM|varchar(20)
		FILIAL  							as NroEmpresa, 		--D2_FILIAL|numeric(10,0)
		cast(NRO_NF as numeric(18,0))   	as Nronf,    		--D2_DOC|numeric(18,0)
		trim(cast(SERIE as varchar(20)))  	as Serienf,    		--D2_SERIE|varchar(20)
		NULL  								as NroItem,    		--Null|numeric(6,0)
		--LEFT(PRODUTO,LEN(PRODUTO) -1) 	as CodProduto,  	--D2_COD|varchar(40)
		PRODUTO								as CodProduto,  	--D2_COD|varchar(40)
		NCM  								as NCM,    --B1_POSIPI|numeric(10,0)
		NULL								as CodBarra,    	--Null|numeric(18,0)
		Case 
			When ORIGEM in ('FAT_PECAS','DEV_PECAS')
				then 'MAQ-PEÇAS'
			When ORIGEM = 'FAT_SERVICOS'
				then 'MAQ-SERV'
			When ORIGEM in ('FAT_MAQUINAS','DEV_MAQUINAS') 
				then 'MAQ-NOVOS'
				else 'OUTRO'	
		END	  								as Departamento,	--MAQ-PEÇAS ou MAQ-NOVOS ou MAQ-SERV |varchar(30)
		COD_MARCA  							as Marca,    		--BM_CODMAR|varchar(30)
		NULL  								as CodFamilia,    	--Null|varchar(30)
		NULL  								as Familia,    		--Null|varchar(60)
		DESC_PRODUTO  						as Descproduto,    --B1_DESC|varchar(100)----------------------------------------------------------------
		QUANTIDADE  						as Qtde,    		--D2_QUANT|numeric(10,3)
		VLR_LIQUIDO_ITEM  					as VlrLiqItem,    	--D2_TOTAL|numeric(14,2)
		VALOR_DESCONTO  					as Vlrdescto,    	--D2_DESC|numeric(14,2) 
		NULL  								as VlrResult,    	--Null|numeric(14,2)
		NULL  								as VlrCustoMkt,    	--Null|numeric(14,2)
		VLR_ICMS  							as VlrICM,   		--D2_VALICM|numeric(14,2)
		NULL  								as VlrImposto,    	--Null|numeric(14,2)
		STATUS_NF  							as Situacao,    	--''  as Situacao,    --N  = ??? Ou C = ??? Ou D = Para Devolucao|char(1)
		NOME_VENDEDOR  						as Vendedor,    	--A3_NOME|varchar(60)
		ID_VENDEDOR  						as CodVendedor,    	--C5_VEND1|varchar(20)
		NULL  								as Identificador,   --Null|varchar(40)
		--NULL
		--OBSERVACAO
		CASE 
			WHEN ORIGEM = 'FAT_SERVICOS' AND OBSERVACAO IS NOT NULL AND TRIM(OBSERVACAO) <> ''
				THEN 
					CONCAT('#CHASSI:',TRIM(OBSERVACAO)) 
			WHEN ORIGEM in ('FAT_MAQUINAS','DEV_MAQUINAS') AND 	TRIM(PRODUTO) <> ''	AND PRODUTO IS NOT NULL
				THEN  
					CONCAT('#CHASSI:',TRIM(PRODUTO))
				ELSE
					OBSERVACAO
		END									as Obs,    			--Null|varchar(250)
		--cast(DATA_EMISSAO_NF as datetime)  	
		CAST(GETDATE() AS DATETIME)			as DtaGeracao,    	--Null|datetime
		NULL  								as StatusIMP,    	--AUTOMATICO MANDAR NULO|char(1)
		NULL  								as DtaImport,    	--AUTOMATICO MANDAR NULO|datetime
		NULL  								as TIPO,    		--Null|char(1)
		NULL  								as LOTECARGA,    	--Null|varchar(50)
		NULL  								as SETORITEM,    	--Null|varchar(30)
		NULL  								as VLRCUSTO,    	--Null|numeric(14,2)
		NULL  								as IDNFSEXTERNO,    --Null|varchar(40)
		NULL  								as VLRICMSRETIDO,   --Null|numeric(14,2)
		VLR_ICMS_SUBST  					as VLRICMSSUBS,    	--Null|numeric(14,2)
		VLR_PIS  							as VLRPIS,    		--Null|numeric(14,2)
		VALOR_COFINS  						as VLRCOFINS,    	--Null|numeric(14,2)
		NULL  								as NROCPFVENDEDOR,  --Null|numeric(11,0)
		NULL  								as VLRDESCTOVCHR    --Null|numeric(14,2)
	
	
	FROM
		X_TOTVS_CRM_FATURAMENTO  with(nolock)
	where 
		DATA_EMISSAO_NF > '2023-01-01' 
	and ORIGEM in (
					'DEV_MAQUINAS',
					'FAT_SERVICOS',
					'FAT_PECAS',
					'FAT_MAQUINAS',
					'DEV_PECAS'
					) 

	--and cast(NRO_NF as numeric(18,0)) = 14930
