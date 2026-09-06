/* ==============================================================
   Objeto ..........: dbo.X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS_N
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-09-29 16:45:31
   Modificado em ...: 2023-10-10 11:39:57
   Linhas ..........: 187
   Escreve em tabela: nao
   Tabelas referidas: X_TOTVS_CRM_FATURAMENTO
   Outras refs .....: BI_COBERTURA_PROSPECCAO, X_CRM_BI_CONGLOMERADO, X_V_COL_CLIENTE_CRM
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

/*

Exec [dbo].[X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS_N]
--DROP PROC [dbo].[X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS_N]

select ORIGEM from X_TOTVS_CRM_FATURAMENTO group by ORIGEM

**/


CREATE   PROCEDURE  [dbo].[X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS_N]
AS 
BEGIN


		--Limpando tabela temporarias se existirem.
		IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name LIKE  '%#Tab01%')
		DROP TABLE #Tab01
		
		
		--TEMP TABLE - CADASTRO CLIENTES TOVS VS CONGLOMERADO CRM.
		SELECT 
			TRIM(A.COD)+'|'+TRIM(A.LOJA) 	AS COD_CLIENTE,
			A.CPF_CNPJ						AS CPF_CNPJ,
			CASE 
				WHEN A.TIPO_PESSOA = 'JURIDICA' 
					THEN 'J'
					ELSE 'F'				
			END 						AS TIPO_PESSOA,
			A.NOME						AS NOME,
			B.seq_pessoa				AS SEQ_PESSOA,
			B.seq_conglo				AS SEQ_CONGLO
			
			Into #Tab01
			
		FROM 
				TOTVS.TMPRD.dbo.X_V_COL_CLIENTE_CRM A
		RIGHT JOIN 	X_CRM_BI_CONGLOMERADO			B 	ON 	CAST(CAST(B.cnpj_pessoa AS BIGINT) AS VARCHAR(15)) = 
															CAST(CAST(CPF_CNPJ AS BIGINT) AS VARCHAR(15)) COLLATE SQL_Latin1_General_CP1_CI_AS
		--SELECT * from #Tab01 WHERE SEQ_PESSOA = '84882';
		--SELECT * FROM GE$PESSOA WHERE SeqPessoa  = '84882';
	 	--SELECT * from X_CRM_BI_CONGLOMERADO where seq_pessoa = 84882
----------------------------------------------------------------------------------------------------------------------------
		--Limpando tabela temporarias se existirem.
		SET ANSI_WARNINGS OFF
		--SET ANSI_WARNINGS { ON | OFF }
		IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name LIKE  '%#Tab02%')
		DROP TABLE #Tab02
		
		SELECT 
			A.nro_processo			AS NRO_PROCESSO,
			A.classe 				AS CLASSE,
			A.resultado 			AS RESULTADO,
			A.oportunidade 			AS OPORTUNIDADE,
			A.seq_pessoa 			AS SEQ_PESSOA,
			--C.SEQ_CONGLO			AS SEQ_CONGLO,
			A.seq_conglo			AS SEQ_CONGLO,
			A.data_historico		AS DATA_CONTATO,
			
			CASE 
  				WHEN B.data_historico <= A.data_historico_final
  					THEN 
  						B.data_historico
  					ELSE 
  						A.data_historico_final
  			END						AS DATA_FINAL_CONTATO
			
			Into #Tab02
			 
		FROM 
					BI_COBERTURA_PROSPECCAO A 
		LEFT JOIN	BI_COBERTURA_PROSPECCAO B 	ON 	B.data_historico 	<= 	A.data_historico_final	
												AND B.data_historico	> 	A.data_historico
												AND B.seq_conglo 		= 	A.seq_conglo					
		--LEFT JOIN	#Tab01  				C 	ON 	A.seq_pessoa 		= 	C.SEQ_PESSOA
		
		
		--SELECT * from #Tab02 where SEQ_PESSOA = 84882 ORDER BY DATA_CONTATO;

----------------------------------------------------------------------------------------------------------------------------
		--Limpando tabela temporarias se existirem.
		IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name LIKE  '%#Tab03%')
		DROP TABLE #Tab03
		
		SELECT 
			C.NRO_PROCESSO,
			C.CLASSE, 
			C.RESULTADO,
			C.OPORTUNIDADE,
			C.SEQ_PESSOA,
			C.SEQ_CONGLO,
			C.DATA_CONTATO							AS DT_DATA_CONTATO,
			C.DATA_FINAL_CONTATO					AS DT_FINAL_CONTATO,
			D.DATA_EMISSAO_NF 						AS DT_PRIMEIRA_NF,
			A.ID,
			A.ORIGEM,
			A.FILIAL,
			Case 
				when D.DATA_EMISSAO_NF is null 
					then 'SEM FATURAMENTO'
				WHEN D.DATA_EMISSAO_NF = A.DATA_EMISSAO_NF
					THEN 'PRIMEIRA NOTA FISCAL'
					ELSE 'SEGUNDA NOTA FISCAL'
			END 									AS PRIMEIRA_NF, 
			A.NRO_NF,
			A.SERIE,
			A.DATA_EMISSAO_NF,		
			A.COD_CLIENTE							AS COD_CLIENTE_TOTVS,
			A.DES_MARCA,
			A.ID_VENDEDOR,
			CONCAT(A.COD_OPERACAO,' - ',A.DES_OPERACAO) 
													AS OPERACAO,
			A.CFOP,
			A.PRODUTO,
			CONCAT(TRIM(A.COD_FAMILIA),' - ',TRIM(DESC_FAMILIA)) 
													AS FAMILIA,
			A.QUANTIDADE,
			A.VALOR_TOTAL,
			A.VALOR_DESCONTO,
			A.VLR_LIQUIDO_ITEM
			
			Into #Tab03
		FROM 
					
					X_TOTVS_CRM_FATURAMENTO 	A
		-----------------------------------------------------------------------------------------------------------			
		LEFT JOIN	#Tab01  							B 	ON 	A.COD_CLIENTE 		= 	B.COD_CLIENTE
		-----------------------------------------------------------------------------------------------------------	
		RIGHT JOIN 	#Tab02								C   ON 	A.DATA_EMISSAO_NF 	>= 	C.DATA_CONTATO
															AND	A.DATA_EMISSAO_NF 	< 	C.DATA_FINAL_CONTATO
															AND B.SEQ_CONGLO 		= 	C.SEQ_CONGLO
		-----------------------------------------------------------------------------------------------------------	
		LEFT JOIN (
		-----------------------------------------------------------------------------------------------------------
	 		SELECT 
				C.NRO_PROCESSO,
				C.SEQ_PESSOA, 
				C.SEQ_CONGLO,
				min(DATA_EMISSAO_NF) as DATA_EMISSAO_NF
				--A.*
			FROM 
						
						X_TOTVS_CRM_FATURAMENTO 	A	 
			LEFT JOIN	#Tab01  							B 	ON 	A.COD_CLIENTE 		= 	B.COD_CLIENTE
			RIGHT JOIN 	#Tab02								C   ON 	A.DATA_EMISSAO_NF 	>= 	C.DATA_CONTATO
																AND	A.DATA_EMISSAO_NF 	< 	C.DATA_FINAL_CONTATO
																AND B.SEQ_CONGLO 		= 	C.SEQ_CONGLO
			WHERE 
				A.ORIGEM IN ('FAT_PECAS','DEV_PECAS','FAT_SERVICOS')
			AND A.DELETADO <>'*'
			GROUP BY 
				C.NRO_PROCESSO,
				C.SEQ_PESSOA, 
				C.SEQ_CONGLO
		-----------------------------------------------------------------------------------------------------------		
		)													D	ON 	D.NRO_PROCESSO		=	C.NRO_PROCESSO
																AND D.SEQ_PESSOA		=	C.SEQ_PESSOA
																AND D.SEQ_CONGLO		=	C.SEQ_CONGLO
															
		WHERE 
			A.ORIGEM IN ('FAT_PECAS','DEV_PECAS','FAT_SERVICOS')
		AND A.DELETADO <>'*' 

		set ANSI_NULLS ON 
		SET ANSI_WARNINGS ON 
 		--------------------------------------------------------------------------------------------------------------------
 		--RETURN RESULTADOS
 		SELECT * FROM #Tab03 --WHERE NRO_PROCESSO in('1239835','1245503')
 		
 		--SELECT * FROM #Tab03 WHERE NRO_PROCESSO in('1239835','1245503') and NRO_NF ='000022849'
 		
 		--------------------------------------------------------------------------------------------------------------------
  		--Limpando tabela temporarias se existirem.		
 		DROP TABLE #Tab01
 		DROP TABLE #Tab02
		DROP TABLE #Tab03
		
----------------------------------------------------------------------------------------------------------------------------		
END
		

	
	
	
	
	
	