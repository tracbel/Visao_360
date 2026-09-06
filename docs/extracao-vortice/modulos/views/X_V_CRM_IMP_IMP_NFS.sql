/* ==============================================================
   Objeto ..........: dbo.X_V_CRM_IMP_IMP_NFS
   Tipo ............: VIEW
   Criado em .......: 2023-09-12 09:09:50
   Modificado em ...: 2023-10-04 13:10:02
   Linhas ..........: 121
   Escreve em tabela: nao
   Tabelas referidas: X_TOTVS_CRM_FATURAMENTO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

/*

  select top 100 * from 
  TOTVS.TMPRD.dbo.X_V_BI_FATURAMENTO_PECAS
  
  select top 100 * from X_TOTVS_BI_FATURAMENTO_POS_VENDAS_N
  
  ------------------------------------------------------------------------
  --pendencias
  	NULL  			as Formapgto,    		--E4_COND|varchar(25)------------------------------			condpgto 		tem na view
	''  			as CondicaoPgto,    	--E4_DESCRI|varchar(60)----------------------------			condpgto		tem na view
	''  			as Vendedor,    		--A3_NOME|varchar(60)------------------------------			nome_vendedor	tem na view
	''  as Dtapedido,    					--C5_EMISSAO|datetime------------------------------				????????????	
	''  as Nropedido,    					--C5_NUM|varchar(20)-------------------------------			nro_pedido		tem na view
	''  as Situacao,    					--N  = ??? Ou C = ???|char(1)----------------------			????????????
	''  as NCM,    							--B1_POSIPI|numeric(10,0)----------------------------------------------------	





*/
CREATE   VIEW  [X_V_CRM_IMP_IMP_NFS] AS
--VIEW CRIADA NO DIA 12/09/2023 -  PARA ATENDER AO NOVO PROCESSO DE INTEGRACAO DE FATURAMENTO DO POS VENDAS COM O CRM.

SELECT	--TOP 30 
	distinct
	CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',DELETADO) AS COD_CHAVE_NF,
	NULL			as IdNFS,   			--AUTO INCREMENT |numeric(18,0)
	'Protheus'  	as Origem,  			--ORIGEM|varchar(20)
	FILIAL  		as NroEmpresa,    		--F2_FILIAL|numeric(10,0)
	cast(NRO_NF as numeric(18,0))  
					as Nronf,    			--F2_DOC|numeric(18,0)
	trim(cast(SERIE as varchar(20)))			
					as Serienf,    			--F2_SERIE|varchar(20)
	NULL  			as NroEmpresaVda,    	--Null|numeric(10,0)
	NULL  			as NroCNPJCPF,   		--Null|numeric(13,0)
	NULL  			as DigCNPJCPF,    		--Null|numeric(2,0)
	NULL  			as CNPJx,    			--Null|varchar(20)
	'Protheus'  	as PessoaLinkOrigem,    --PESSOALINKORIGEM|varchar(20)
	REPLACE(COD_CLIENTE, '|','')  
					as Pessoalink,    		--F2_CLIENTE|varchar(250)
	'AUTOMOVEIS'  	as Segmento,    		--SEGMENTO|varchar(30)
	Case 
		When ORIGEM in ('FAT_PECAS','DEV_PECAS')
			then 
				'MAQ-PEÇAS'
		When ORIGEM = 'FAT_SERVICOS'
			then 
				'MAQ-SERV'
		WHEN ORIGEM in ('FAT_MAQUINAS','DEV_MAQUINAS') 
			then 'MAQ-NOVOS'
			else 
				'OUTRO'	
	END				as Departamento,   		--DEPARTAMENTO|varchar(30)
	'Normal'  		as TipoVenda,    		--NORMAL|varchar(30)
	--NULL  			as CFOP,    			--F3_CFO|numeric(6,0) 
	CFOP  			as CFOP,    			--F3_CFO|numeric(6,0) 
	COD_OPERACAO	as CodOperacao,    		--D2_TES|varchar(20)
	--COD_OPERACAO  as CodOperacao,    		--D2_TES|varchar(20)
	ORIGEM  		as Operacao,    		--Null|varchar(30)
	--DES_OPERACAO  	as Operacao,    	--Null|varchar(30)
	'Normal'		as CanalVenda,    		--Null|varchar(20)
	NULL  			as Setor,   			--Null|varchar(30)
	CODICAO_PAGAMENTO
					as Formapgto,    		--E4_COND|varchar(25)
	DES_CODICAO_PAGAMENTO  			
					as CondicaoPgto,    	--E4_DESCRI|varchar(60)
	NOME_VENDEDOR  	as Vendedor,    		--A3_NOME|varchar(60)
	ID_VENDEDOR  	as CodVendedor,    		--C5_VEND1|varchar(20)
	cast(DATA_PEDIDO as datetime)  
					as Dtapedido,    		--C5_EMISSAO|datetime
	NRO_PEDIDO		as Nropedido,    		--C5_NUM|varchar(20)
	cast(DATA_EMISSAO_NF as datetime)  	
					as Dtaemissaonf,    	--F2_EMISSAO|datetime
	STATUS_NF		as Situacao,    		--N  = ??? Ou C = ??? Ou D = Para Devolucao|char(1)
	NULL  			as CodTransportador,    --Null|varchar(20)
	NULL  			as Transportador,    	--Null|varchar(40)
	NULL  			as Usuario,    			--Null|varchar(20)
	--OBSERVACAO		
	NULL			as Obs,    				--Null|varchar(250) (vou usar como id)
	NULL  			as DtaAlteracaoERP,    	--Null|datetime
	CAST(GETDATE() AS DATETIME)  
	--cast(DATA_EMISSAO_NF as datetime)
					as DtaGeracao,    		--???|datetime
	NULL  			as StatusIMP,    		--N = ??? Ou E = ???|char(1)
	NULL  			as DtaImport,    		--???|datetime
	NULL  			as WhereItem,    		--Null|varchar(60)
	NULL  			as PERCBASECOMISSAO,    --Null|numeric(5,2)
	NULL  			as IDNFSEXTERNO,    	--Null|varchar(40)
	NULL  			as LOTECARGA,    		--Null|varchar(50)
	NULL  			as PAIIDNFSEXTERNO,    	--Null|varchar(40)
	NULL  			as EVENTOCMPL,    		--Null|varchar(60)
	NULL  			as TIPOPEDIDO,    		--Null|varchar(40)
	NULL  			as IDENTIFICADO,    	--Null|varchar(20)
	NULL  			as NROCPFVENDEDOR,    	--Null|numeric(11,0)
	NULL  			as NROVOUCHER    		--Null|varchar(50)
	
FROM
	X_TOTVS_CRM_FATURAMENTO A with(nolock) 
INNER JOIN (
				SELECT 
					CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',DELETADO)	as MAX_COD_CHAVE_NF,
					max(ID) 		as MAX_ID
				from 
					X_TOTVS_CRM_FATURAMENTO with(nolock) 
				where 
					DATA_EMISSAO_NF > '2023-01-01' 
				--and cast(NRO_NF as numeric(18,0)) in (396239,23091, 54048, 13012, 63852)
				--and COD_CHAVE_NF = 'N|FAT_PECAS|9|000013012|1  |010647131|0001|'
				group by 
					CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',DELETADO)
)										B 	ON 	CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',DELETADO) 	= B.MAX_COD_CHAVE_NF
											AND A.ID 			= B.MAX_ID
where 
	DATA_EMISSAO_NF > '2023-01-01' 
and ORIGEM in ('DEV_MAQUINAS','FAT_SERVICOS','FAT_PECAS','FAT_MAQUINAS','DEV_PECAS') 


--AND cast(NRO_NF as numeric(18,0)) = 14930
