/* ==============================================================
   Objeto ..........: dbo.X_V_BI_DESPESAS_VENDA_MAQUINAS
   Tipo ............: VIEW
   Criado em .......: 2023-02-13 09:11:40
   Modificado em ...: 2023-03-31 09:44:00
   Linhas ..........: 719
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_ACOMP_VEND_MAQUINAS, IV_Q_CONT_COMISSAO_22, IV_Q_GESTAO_CREDITO, IV_Q_GESTAO_CREDITO_AMS, IV_Q_GESTAO_CREDITO_IMP, IV_Q_GESTAO_PRODUTO___AMS, IV_Q_GESTAO_PRODUTO_IMPL, IV_Questionario, X_TOTVS_BI_FATURAMENTO_MAQUINAS
   Outras refs .....: fn_StripCharacters, X_CRM_BI_CONGLOMERADO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

/*

SELECT * from IV_Q$GESTAO_CREDITO 		WHERE Processo = 1136828;
SELECT FORMULARIO_NUMERO, * FROM IV_Q_GESTAO_CREDITO WHERE SeqQuestionario = 44349;
SELECT * FROM IV_Questionario WHERE SeqQuestionario = 44349;
SELECT * FROM dbo.IV_Formulario 

SELECT * FROM IV_QUESTIONARIO WHERE PROCESSO = 1136828;
SELECT FORMULARIO_NUMERO, * FROM IV_Q_ACOMP_VEND_MAQUINAS WHERE CHASSI = '1BM7200JENH003169';

SELECT * from openquery ( totvs, 'select * from X_V_COL_MODELO_CRM'ï¿½)

SELECT * FROM X_TOTVS_BI_FATURAMENTO_MAQUINAS



--SELECT * FROM GE$PESSOA gp 

--EXEC  [X_TOTVS_ATUALIZA_BI_FATURAMENTO_MAQUINAS];--
SELECT * FROM X_V_BI_DESPESAS_VENDA_MAQUINAS 
where NRO_PROCESSO_CRM in (1136828,1124015);
*/


CREATE   VIEW  [X_V_BI_DESPESAS_VENDA_MAQUINAS] AS
/*
	VIEW CRIADA NO DIA 13/02/2023 - POR FELIPE VIOLIN
	OBJETIVO: BUSCAR NO PROCESSO VENDA DE IMPLEMENTOS, AMS, E MAQUINAS AS DESPESAS DE VENDAS.
	
	O PROCESSO FOI DESENVOLVIDO COM CONSULTORIA DO DIEGO MARQUES E VANESSA.
	 
	OBS. ESSA VIEW DEPENDA DE EXECUÇÃO DA PROCEDURE X_TOTVS_ATUALIZA_BI_FATURAMENTO_MAQUINAS
	
	OBS_2. ESSA VIEW SERÁ UTILIZADA PELO BI ORÇADO X REALIZADO E GESTÃO DE VENDAS.
*/


--SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

SELECT 
	'PROCESSO_MAQUINAS'		AS ORIGEM,
	CASE
		WHEN A.NOTA_FISCAL	IS NULL 
			THEN 0
			ELSE A.NOTA_FISCAL
	END 					as NRO_NF1,
	CASE
		WHEN A.NF_REFATURAMENTO IS NULL 
			THEN 0
			ELSE A.NF_REFATURAMENTO
	END  					as NF_DE_REFATURAMENTO,
	B.SeqQuestionario 		as SEQQUESTIONARIO,
	UPPER(TRIM(A.FORMA_PAGAMENTO)) 		
							as TIPO_RECURSO,
	E.CHASSIS_CRM			as CHASSIS_CRM,									
	  case 
	  	when len(D.digCGCCPF) =1 
	  		then 
	  			CAST(D.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(D.DigCGCCPF AS CHAR(2))  
	  	when len(D.digCGCCPF) =2 
	  		then 
	  			CAST(D.NroCGCCPF AS VARCHAR(12)) + CAST(D.DigCGCCPF AS CHAR(2))
	  		else  
	  			CAST(D.NroCGCCPF AS VARCHAR(12)) 
	  end  					as CGCCPF_CRM,
	D.SeqPessoa 			as SEQ_PESSOA_CRM,
	B.Processo				as NRO_PROCESSO_CRM,
	A.VALOR_TOTAL	 		as VLR_TOTAL_CRM,
	A.VALOR_FINANCIADO 		as VALOR_FINANCIADO_CRM,
	CASE 
		WHEN A.VALOR_USADO IS NULL 
			THEN 0
			ELSE A.VALOR_USADO
	END						as VALOR_USADO_CRM,
	
	CASE 
		WHEN A.TAXA_FLAT IS NOT NULL 
			THEN 'SIM'
			ELSE 'NÃO'
	END						as TAXA_FLAT_CRM,
	case 
		when
			TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) is not null
	  	and 
	      	TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) <=6
	  		then
	      		TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) 
	  		else
	      	0 
	end						AS TAXA_FLAT_PERCENTUAL_CRM,
	ROUND(
		(case 
			when
				TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) is not null
		  	and 
		      	TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) <=6
		  		then
		      		TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) 
		  		else
		      	0 
		end /100 )	* A.VALOR_FINANCIADO		,2)		 	
							AS VLR_TAXA_FLAT_CRM,
		
	UPPER(TRIM(A.TIPO_BONIFICACAO_DES))	
							AS BONIFICACAO_CRM,
	A.BONIFICACAO_DESCONTO  	
							AS VLR_BONIFICACAO_CRM,							
	
	UPPER(TRIM(A.DETALHES_DESCONTO_IN))
							AS DESC_INCONDICIONAL,
	CASE 
		WHEN A.VALOR_DESCONTO_INCON IS NULL
			THEN 0
			ELSE A.VALOR_DESCONTO_INCON
	END 	
							AS VLR_DESC_INCONDICIONAL,							

	F.VRL_LIQ_INCENTIVO 	as VLR_INCENTIVO_LIQ_CRM, 						
	F.VLR_BRUTO_INCENTIVO 	as VLR_INCENTIVO_BRU_CRM, 						
	E.TP_FRETE_CRM		 	as TP_FRETE_CRM,								
	E.VLR_FRETE_CRM  		as VLR_FRETE_CRM,								
	E.TRANSPORTADORA_CRM 	as TRANSPORTADORA_CRM,							
	UPPER(TRIM(A.INSTITUICAO_FIINSTIT)) 	
							AS BANCO_CRM,
	CASE 
		WHEN UPPER(TRIM(A.INSTITUICAO_FIINSTIT)) = 'BANCO JOHN DEERE'						
			THEN (A.VALOR_FINANCIADO*0.01)
			ELSE 0
	END						as FUNDO_RESERVA_CRM,
	
	
	CASE 
		when G.VLR_COMISSAO_CRM	is null 
			then 0
			else G.VLR_COMISSAO_CRM
	END 					as VLR_COMISSAO_CRM,							
	Case 
		when G.STATUS_COMISSAO is null 	
			then 'NÃO_CALCULADO'
			else G.STATUS_COMISSAO
	END						as STATUS_COMISSAO

FROM

 			dbo.IV_Q_GESTAO_CREDITO  AS A 		--TABF
LEFT  JOIN	dbo.IV_Questionario 	AS B 		ON B.SeqQuestionario 	= A.SEQQUESTIONARIO --QST																						
LEFT JOIN	dbo.IV_Formulario 		AS C 		ON C.SeqFormulario 		= B.SeqFormulario 	--FRM
LEFT JOIN	dbo.GE_Pessoa 			AS D 		ON D.SeqPessoa 			= B.SeqPessoa		--TBASE
--------------------------------------------------------------------------------------------------------
LEFT JOIN 	(
			--------------------------------------------------------------------------------------------
				SELECT 					
					TBASE.SEQPESSOA			as SEQ_PESSOA,
					QST.PROCESSO			as NRO_PROCESSO,
					QST.SEQQUESTIONARIO		as SEQ_QUESTIONARIO,
					TABF.FORMULARIO_NUMERO 	as NRO_FORMULARIO,
					TABF.CHASSI				as CHASSIS_CRM,
					CASE WHEN TABF.VALOR_FRETE IS NULL
						THEN 0
						ELSE TABF.VALOR_FRETE
					END 				as VLR_FRETE_CRM,
					CASE WHEN TABF.TRANSPORTADORA IS NULL OR TRIM(TABF.TRANSPORTADORA) = ''
						THEN 'NÃO INFORMADO'
						ELSE TABF.TRANSPORTADORA
					END 				as TRANSPORTADORA_CRM,	
					CASE 
						WHEN (TABF.TRANSPORTE_ENTREGA1 IS NULL OR TABF.TRANSPORTE_ENTREGA1 = '') AND (TABF.TRANSPORTE_ENTREGA IS NULL OR TABF.TRANSPORTE_ENTREGA = '')
							THEN 'NÃO INFORMADO'
						WHEN TABF.TRANSPORTE_ENTREGA1 IS NULL AND TABF.TRANSPORTE_ENTREGA IS NOT NULL
							THEN UPPER(TRIM(TABF.TRANSPORTE_ENTREGA))
							ELSE UPPER(TRIM(TABF.TRANSPORTE_ENTREGA1))
					END 				as TP_FRETE_CRM
				FROM 
						IV_Q_ACOMP_VEND_MAQUINAS	TABF  	
				JOIN 	IV_QUESTIONARIO 			QST 	ON QST.SEQQUESTIONARIO 	= TABF.SEQQUESTIONARIO  	
				JOIN 	IV_FORMULARIO 				FRM 	ON  FRM.SEQFORMULARIO 	= QST.SEQFORMULARIO  	
				JOIN 	GE_PESSOA 					TBASE 	ON TBASE.SEQPESSOA 		= QST.SEQPESSOA
----------------------------------------------------------------------------------------------------------				
) AS E 											ON  E.NRO_FORMULARIO 	= A.FORMULARIO_NUMERO 
												AND E.NRO_PROCESSO		= B.Processo
												AND E.SEQ_PESSOA		= D.SeqPessoa 
----------------------------------------------------------------------------------------------------------
LEFT JOIN (		
				SELECT 
					CHASSI_ID_INCENTIVO,
					SUM(VLR_BRUTO_INCENTIVO) AS VLR_BRUTO_INCENTIVO,
					sum(VRL_LIQ_INCENTIVO)	 AS VRL_LIQ_INCENTIVO
				
				from openquery ( totvs, 'select * from X_V_BI_ESTOQUE_INCENTIVO')
				group by 
					CHASSI_ID_INCENTIVO
) AS F 											ON F.CHASSI_ID_INCENTIVO COLLATE Latin1_General_BIN = E.CHASSIS_CRM											
----------------------------------------------------------------------------------------------------------										
LEFT JOIN (

				SELECT 
					QST.PROCESSO					as NRO_PROCESSO,
					TABF.FORMULARIO_NUMERO			as NRO_FORMULARIO,
					TBASE.SEQPESSOA					as SEQ_PESSOA,
				    (
				    	Case when TABF.VALOR_COMISSAO_CEN is null then 0 else TABF.VALOR_COMISSAO_CEN end 		+ 
						Case when TABF.VALOR_PREMIO_ESPECIA is null then 0 else TABF.VALOR_PREMIO_ESPECIA end 	+ 
						Case when TABF.COMISSAO_AMS_ADICION is null then 0 else	TABF.COMISSAO_AMS_ADICION end 	+ 
						Case when TABF.COMISSAO_ADICIONAL_E is null then 0 else TABF.COMISSAO_ADICIONAL_E end
					) 								as VLR_COMISSAO_CRM,
					Case 
						When TABF.VALOR_COMISSAO_CEN is not null 
							then 'CALCULADO' 
							else 'NÃO_CALCULADO'
					End						as STATUS_COMISSAO	
					
				 
				 FROM 
						IV_Q_CONT_COMISSAO_22 	TABF  	
				JOIN 	IV_QUESTIONARIO 		QST 		ON QST.SEQQUESTIONARIO 	=  TABF.SEQQUESTIONARIO  	
				JOIN 	IV_FORMULARIO 			FRM 		ON FRM.SEQFORMULARIO 	= QST.SEQFORMULARIO  	
				JOIN 	GE_PESSOA 				TBASE 		ON TBASE.SEQPESSOA 		= QST.SEQPESSOA

)	G 											ON  G.NRO_FORMULARIO 	= A.FORMULARIO_NUMERO 
												AND G.NRO_PROCESSO		= B.Processo
												AND G.SEQ_PESSOA		= D.SeqPessoa 


WHERE
	A.FATURAMENTO_REALIZAD IS NOT NULL
--AND B.Processo in (1136828,1124015)
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
UNION 
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
--PROCESSO IMPLEMENTO
SELECT 
	'PROCESSO_IMPLEMENTOS'		AS ORIGEM,
	CASE
		WHEN A.NOTA_FISCAL	IS NULL 
			THEN 0
			ELSE A.NOTA_FISCAL
	END 					as NRO_NF1,
	CASE
		WHEN A.NF_REFATURAMENTO IS NULL 
			THEN 0
			ELSE A.NF_REFATURAMENTO
	END  					as NF_DE_REFATURAMENTO,
	B.SeqQuestionario 		as SEQQUESTIONARIO,
	UPPER(TRIM(A.FORMA_PAGAMENTO)) 		
							as TIPO_RECURSO,
	E.CHASSIS_CRM			as CHASSIS_CRM,									
	  case 
	  	when len(D.digCGCCPF) =1 
	  		then 
	  			CAST(D.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(D.DigCGCCPF AS CHAR(2))  
	  	when len(D.digCGCCPF) =2 
	  		then 
	  			CAST(D.NroCGCCPF AS VARCHAR(12)) + CAST(D.DigCGCCPF AS CHAR(2))
	  		else  
	  			CAST(D.NroCGCCPF AS VARCHAR(12)) 
	  end  					as CGCCPF_CRM,
	D.SeqPessoa 			as SEQ_PESSOA_CRM,
	B.Processo				as NRO_PROCESSO_CRM,
	A.VALOR_TOTAL	 		as VLR_TOTAL_CRM,
	A.VALOR_FINANCIADO 		as VALOR_FINANCIADO_CRM,
	CASE 
		WHEN A.VALOR_USADO IS NULL 
			THEN 0
			ELSE A.VALOR_USADO
	END						as VALOR_USADO_CRM,
	
	CASE 
		WHEN A.TAXA_FLAT IS NOT NULL AND TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) >0
			THEN 'SIM'
			ELSE 'NAO'
	END						as TAXA_FLAT_CRM,
	case 
		when
			TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) is not null
	  	and 
	      	TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) <=6
	  		then
	      		TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) 
	  		else
	      	0 
	end						AS TAXA_FLAT_PERCENTUAL_CRM,
	ROUND(
		(case 
			when
				TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) is not null
		  	and 
		      	TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) <=6
		  		then
		      		TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) 
		  		else
		      	0 
		end /100 )	* A.VALOR_FINANCIADO		,2)		 	
							AS VLR_TAXA_FLAT_CRM,
		
	UPPER(TRIM(A.TIPO_BONIFICACAO_DES))	
							AS BONIFICACAO_CRM,
	A.BONIFICACAO_DESCONTO  	
							AS VLR_BONIFICACAO_CRM,							
	
	UPPER(TRIM(A.DETALHES_DESCONTO_IN))
							AS DESC_INCONDICIONAL,
	CASE 
		WHEN A.VALOR_DESCONTO_INCON IS NULL 
			THEN 0
			ELSE A.VALOR_DESCONTO_INCON
	END 	
							AS VLR_DESC_INCONDICIONAL,							
	Case 
		when F.VRL_LIQ_INCENTIVO is null
			then 0
			else F.VRL_LIQ_INCENTIVO
	end						as VLR_INCENTIVO_LIQ_CRM, 
	Case 
		when F.VLR_BRUTO_INCENTIVO is null
			then 0
			else F.VLR_BRUTO_INCENTIVO
	END						as VLR_INCENTIVO_BRU_CRM, 						
	E.TP_FRETE_CRM		 	as TP_FRETE_CRM,								
	E.VLR_FRETE_CRM  		as VLR_FRETE_CRM,								
	E.TRANSPORTADORA_CRM 	as TRANSPORTADORA_CRM,							
	UPPER(TRIM(A.INSTITUICAO_FIINSTIT)) 	
							AS BANCO_CRM,
	
	CASE 
		WHEN UPPER(TRIM(A.INSTITUICAO_FIINSTIT)) = 'BANCO JOHN DEERE'						
			THEN (A.VALOR_FINANCIADO*0.01)
			ELSE 0
	END						as FUNDO_RESERVA_CRM,
	CASE 
		when G.VLR_COMISSAO_CRM	is null 
			then 0
			else G.VLR_COMISSAO_CRM
	end 					as VLR_COMISSAO_CRM,							
	Case 
		when G.STATUS_COMISSAO is null 	
			then 'NÃO_CALCULADO'
			else G.STATUS_COMISSAO
	END						as STATUS_COMISSAO

FROM
			dbo.IV_Q_GESTAO_CREDITO_IMP 	AS A 													--TABF	
INNER JOIN	dbo.IV_Questionario 			AS B 		ON B.SeqQuestionario 	= A.SEQQUESTIONARIO --QST
INNER JOIN	dbo.IV_Formulario 				AS C 		ON C.SeqFormulario 		= B.SeqFormulario 	--FRM
INNER JOIN	dbo.GE_Pessoa 					AS D 		ON D.SeqPessoa 			= B.SeqPessoa		--TBASE
---------------------------------------------------------------------------------------------------------------------------	

LEFT JOIN 	(
			---------------------------------------------------------------------------------------------------------------------------	
				SELECT 					
					TBASE.SEQPESSOA				as SEQ_PESSOA,
					QST.PROCESSO				as NRO_PROCESSO,
					QST.SEQQUESTIONARIO			as SEQ_QUESTIONARIO,
					TABF.FORMULARIO_NFORMULAR 	as NRO_FORMULARIO,
					TABF.CHASSI					as CHASSIS_CRM,
					--CASE WHEN TABF.VALOR_FRETE IS NULL
					--	THEN 0
					--	ELSE TABF.VALOR_FRETE
					--END 				
					0							
												as VLR_FRETE_CRM,
					--CASE WHEN TABF.TRANSPORTADORA IS NULL OR TRIM(TABF.TRANSPORTADORA) = ''
					--	THEN 'NÃO INFORMADO'
					--	ELSE TABF.TRANSPORTADORA
					--END 				
					''							
												as TRANSPORTADORA_CRM,	
					--CASE 
					--	WHEN (TABF.TRANSPORTE_ENTREGA1 IS NULL OR TABF.TRANSPORTE_ENTREGA1 = '') AND (TABF.TRANSPORTE_ENTREGA IS NULL OR TABF.TRANSPORTE_ENTREGA = '')
					--		THEN 'NÃO INFORMADO'
					--	WHEN TABF.TRANSPORTE_ENTREGA1 IS NULL AND TABF.TRANSPORTE_ENTREGA IS NOT NULL
					--		THEN UPPER(TRIM(TABF.TRANSPORTE_ENTREGA))
					--		ELSE UPPER(TRIM(TABF.TRANSPORTE_ENTREGA1))
					--END 				
					''							as TP_FRETE_CRM
				FROM 
					dbo.IV_Q_GESTAO_PRODUTO_IMPL	TABF  	
				JOIN 	IV_QUESTIONARIO 			QST 	ON QST.SEQQUESTIONARIO 	= TABF.SEQQUESTIONARIO  	
				JOIN 	IV_FORMULARIO 				FRM 	ON  FRM.SEQFORMULARIO 	= QST.SEQFORMULARIO  	
				JOIN 	GE_PESSOA 					TBASE 	ON TBASE.SEQPESSOA 		= QST.SEQPESSOA
---------------------------------------------------------------------------------------------------------------------------					
) AS E 											ON  E.NRO_FORMULARIO 	= A.FORMULARIO_NUMERO 
												AND E.NRO_PROCESSO		= B.Processo
												AND E.SEQ_PESSOA		= D.SeqPessoa 
											
---------------------------------------------------------------------------------------------------------------------------												
LEFT JOIN (		
				SELECT 
					CHASSI_ID_INCENTIVO,
					SUM(VLR_BRUTO_INCENTIVO) AS VLR_BRUTO_INCENTIVO,
					sum(VRL_LIQ_INCENTIVO)	 AS VRL_LIQ_INCENTIVO
				
				from openquery ( totvs, 'select * from X_V_BI_ESTOQUE_INCENTIVO')
				group by 
					CHASSI_ID_INCENTIVO
) AS F 											ON F.CHASSI_ID_INCENTIVO COLLATE Latin1_General_BIN = E.CHASSIS_CRM		

																	
---------------------------------------------------------------------------------------------------------------------------										
LEFT JOIN (
				SELECT 
					QST.PROCESSO					as NRO_PROCESSO,
					TABF.FORMULARIO_NUMERO			as NRO_FORMULARIO,
					TBASE.SEQPESSOA					as SEQ_PESSOA,
				    (
				    	Case when TABF.VALOR_COMISSAO_CEN is null then 0 else TABF.VALOR_COMISSAO_CEN end 		+ 
						Case when TABF.VALOR_PREMIO_ESPECIA is null then 0 else TABF.VALOR_PREMIO_ESPECIA end 	+ 
						Case when TABF.COMISSAO_AMS_ADICION is null then 0 else	TABF.COMISSAO_AMS_ADICION end 	+ 
						Case when TABF.COMISSAO_ADICIONAL_E is null then 0 else TABF.COMISSAO_ADICIONAL_E end
					) 								as VLR_COMISSAO_CRM,
					Case 
						When TABF.VALOR_COMISSAO_CEN is not null 
							then 'CALCULADO' 
							else 'NÃO_CALCULADO'
					End						as STATUS_COMISSAO	
					
				 
				 FROM 
						IV_Q_CONT_COMISSAO_22 	TABF  	
				JOIN 	IV_QUESTIONARIO 		QST 		ON QST.SEQQUESTIONARIO 	=  TABF.SEQQUESTIONARIO  	
				JOIN 	IV_FORMULARIO 			FRM 		ON FRM.SEQFORMULARIO 	= QST.SEQFORMULARIO  	
				JOIN 	GE_PESSOA 				TBASE 		ON TBASE.SEQPESSOA 		= QST.SEQPESSOA

)	G 											ON  G.NRO_FORMULARIO 	= A.FORMULARIO_NUMERO
												AND G.NRO_PROCESSO		= B.Processo
												AND G.SEQ_PESSOA		= D.SeqPessoa 
---------------------------------------------------------------------------------------------------------------------------													


WHERE 
	A.FATURAMENTO_REALIZAD IS NOT NULL
--AND	B.Processo = 1148351
 

--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
UNION 
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
--PROCESSO AMS
SELECT 

	ORIGEM,
	NRO_NF1,
	NF_DE_REFATURAMENTO,
	SEQQUESTIONARIO,
	TIPO_RECURSO,
	CHASSIS_CRM,
	CGCCPF_CRM,
	SEQ_PESSOA_CRM,
	NRO_PROCESSO_CRM,
	--VLR_TOTAL_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_TOTAL_CRM) 			as VLR_TOTAL_CRM,
	--VALOR_FINANCIADO_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VALOR_FINANCIADO_CRM) 	as VALOR_FINANCIADO_CRM,
	--VALOR_USADO_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VALOR_USADO_CRM) 			as VALOR_USADO_CRM,
	TAXA_FLAT_CRM,
	TAXA_FLAT_PERCENTUAL_CRM,
	--VLR_TAXA_FLAT_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_TAXA_FLAT_CRM) 		as VLR_TAXA_FLAT_CRM,
	BONIFICACAO_CRM,
	--VLR_BONIFICACAO_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_BONIFICACAO_CRM) 		as VLR_BONIFICACAO_CRM,
	DESC_INCONDICIONAL,
	--VLR_DESC_INCONDICIONAL,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_DESC_INCONDICIONAL) 	as VLR_DESC_INCONDICIONAL,
	--VLR_INCENTIVO_LIQ_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_INCENTIVO_LIQ_CRM) 	as VLR_INCENTIVO_LIQ_CRM,
	--VLR_INCENTIVO_BRU_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_INCENTIVO_BRU_CRM) 	as VLR_INCENTIVO_BRU_CRM,
	TP_FRETE_CRM,
	--VLR_FRETE_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_FRETE_CRM) 			as VLR_FRETE_CRM,
	TRANSPORTADORA_CRM,
	BANCO_CRM,
	--FUNDO_RESERVA_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * FUNDO_RESERVA_CRM) 		as FUNDO_RESERVA_CRM,
	--VLR_COMISSAO_CRM,
	((vlr_liquido_item / VLR_TOTAL_CRM) * VLR_COMISSAO_CRM) 		as VLR_COMISSAO_CRM,
	STATUS_COMISSAO--,
	--vlr_liquido_item

from
(

	SELECT 
		'PROCESSO_AMS'			AS ORIGEM,
		CASE
			WHEN A.NOTA_FISCAL	IS NULL 
				THEN 0
				ELSE A.NOTA_FISCAL
		END 					as NRO_NF1,
		CASE
			WHEN A.NF_REFATURAMENTO IS NULL 
				THEN 0
				ELSE A.NF_REFATURAMENTO
		END  					as NF_DE_REFATURAMENTO,
		B.SeqQuestionario 		as SEQQUESTIONARIO,
		UPPER(TRIM(A.FORMA_PAGAMENTO)) 		
								as TIPO_RECURSO,
		E.CHASSIS_CRM			as CHASSIS_CRM,													
		CON.cnpj_pessoa			as CGCCPF_CRM,
		D.SeqPessoa 			as SEQ_PESSOA_CRM,
		B.Processo				as NRO_PROCESSO_CRM,
		A.VALOR_TOTAL	 		as VLR_TOTAL_CRM,
		A.VALOR_FINANCIADO 		as VALOR_FINANCIADO_CRM,
		CASE 
			WHEN A.VALOR_USADO IS NULL 
				THEN 0
				ELSE A.VALOR_USADO
		END						as VALOR_USADO_CRM,
		
		CASE 
			WHEN A.TAXA_FLAT IS NOT NULL AND TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) >0
				THEN 'SIM'
				ELSE 'NAO'
		END						as TAXA_FLAT_CRM,
		case 
			when
				TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) is not null
		  	and 
		      	TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) <=6
		  		then
		      		TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) 
		  		else
		      	0 
		end						AS TAXA_FLAT_PERCENTUAL_CRM,
		ROUND(
			(case 
				when
					TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) is not null
			  	and 
			      	TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) <=6
			  		then
			      		TRY_CAST( REPLACE( dbo.fn_StripCharacters(A.TAXA_FLAT, '^0-9-,-.'),',','.') AS DECIMAL(10, 2)) 
			  		else
			      	0 
			end /100 )	* A.VALOR_FINANCIADO		,2)		 	
								AS VLR_TAXA_FLAT_CRM,
			
		UPPER(TRIM(A.TIPO_BONIFICACAO_DES))	
								AS BONIFICACAO_CRM,
		A.BONIFICACAO_DESCONTO  	
								AS VLR_BONIFICACAO_CRM,							
		
		UPPER(TRIM(A.DETALHES_DESCONTO_IN))
								AS DESC_INCONDICIONAL,
		CASE 
			WHEN A.VALOR_DESCONTO_INCON IS NULL 
				THEN 0
				ELSE A.VALOR_DESCONTO_INCON
		END 	
								AS VLR_DESC_INCONDICIONAL,							
		Case 
			when F.VRL_LIQ_INCENTIVO is null
				then 0
				else F.VRL_LIQ_INCENTIVO
		end						as VLR_INCENTIVO_LIQ_CRM, 
		Case 
			when F.VLR_BRUTO_INCENTIVO is null
				then 0
				else F.VLR_BRUTO_INCENTIVO
		END						as VLR_INCENTIVO_BRU_CRM, 						
		E.TP_FRETE_CRM		 	as TP_FRETE_CRM,								
		E.VLR_FRETE_CRM  		as VLR_FRETE_CRM,								
		E.TRANSPORTADORA_CRM 	as TRANSPORTADORA_CRM,							
		UPPER(TRIM(A.INSTITUICAO_FIINSTIT)) 	
								AS BANCO_CRM,
		
		CASE 
			WHEN UPPER(TRIM(A.INSTITUICAO_FIINSTIT)) = 'BANCO JOHN DEERE'						
				THEN (A.VALOR_FINANCIADO*0.01)
				ELSE 0
		END						as FUNDO_RESERVA_CRM,
		CASE 
			when G.VLR_COMISSAO_CRM	is null 
				then 0
				else G.VLR_COMISSAO_CRM
		end 					as VLR_COMISSAO_CRM,							
		Case 
			when G.STATUS_COMISSAO is null 	
				then 'NÃO_CALCULADO'
				else G.STATUS_COMISSAO
		END						as STATUS_COMISSAO,
		
		H.vlr_liquido_item
	
	
	FROM
				dbo.IV_Q_GESTAO_CREDITO_AMS AS A WITH (NOLOCK)													--TABF	
	INNER JOIN	dbo.IV_Questionario 		AS B WITH (NOLOCK)		ON B.SeqQuestionario 	= A.SEQQUESTIONARIO --QST
	INNER JOIN	dbo.IV_Formulario 			AS C WITH (NOLOCK)		ON C.SeqFormulario 		= B.SeqFormulario 	--FRM
	INNER JOIN	dbo.GE_Pessoa 				AS D WITH (NOLOCK)		ON D.SeqPessoa 			= B.SeqPessoa		--TBASE
	---------------------------------------------------------------------------------------------------------------------------------------	
	LEFT JOIN 	(
	---------------------------------------------------------------------------------------------------------------------------------------	
					SELECT 					
						TBASE.SEQPESSOA				as SEQ_PESSOA,
						QST.PROCESSO				as NRO_PROCESSO,
						QST.SEQQUESTIONARIO			as SEQ_QUESTIONARIO,
						TABF.FORMULARIO_NUMERO 		as NRO_FORMULARIO,
						CHASSI.CHASSI				as CHASSIS_CRM,
						0							as VLR_FRETE_CRM,
						''							as TRANSPORTADORA_CRM,				
						''							as TP_FRETE_CRM
					FROM 
						dbo.IV_Q_GESTAO_PRODUTO___AMS 	TABF	WITH (NOLOCK) 	
					JOIN 	IV_QUESTIONARIO 			QST		WITH (NOLOCK)	ON QST.SEQQUESTIONARIO 	= TABF.SEQQUESTIONARIO  	
					JOIN 	IV_FORMULARIO				FRM 	WITH (NOLOCK)	ON FRM.SEQFORMULARIO 	= QST.SEQFORMULARIO  	
					JOIN 	GE_PESSOA					TBASE 	WITH (NOLOCK)	ON TBASE.SEQPESSOA 		= QST.SEQPESSOA
					JOIN (
							SELECT 
								SEQQUESTIONARIO,
								CHASSI
							FROM 
								(SELECT SEQQUESTIONARIO, CHASSI_AMS___1, CHASSI_AMS___2, CHASSI_AMS___3,CHASSI_AMS___4,CHASSI_AMS___5,CHASSI_AMS___6,CHASSI_AMS___7,CHASSI_AMS___8,CHASSI_AMS___9,CHASSI_AMS___10 
								From dbo.IV_Q_GESTAO_PRODUTO___AMS)	TABF 
							UNPIVOT
								( CHASSI FOR CHASSI_TP IN 
									(CHASSI_AMS___1, CHASSI_AMS___2, CHASSI_AMS___3,CHASSI_AMS___4,CHASSI_AMS___5,CHASSI_AMS___6,CHASSI_AMS___7,CHASSI_AMS___8,CHASSI_AMS___9,CHASSI_AMS___10)
								)AS unpvt
	
							WHERE 
								CHASSI <> ''			
					)	CHASSI									ON 	QST.SEQQUESTIONARIO = CHASSI.SEQQUESTIONARIO
	---------------------------------------------------------------------------------------------------------------------------					
	) AS E 											ON  E.NRO_FORMULARIO 	= A.FORMULARIO_NUMERO 
													AND E.NRO_PROCESSO		= B.Processo
													AND E.SEQ_PESSOA		= D.SeqPessoa 											
	---------------------------------------------------------------------------------------------------------------------------	
											
	LEFT JOIN (		
					SELECT 
						CHASSI_ID_INCENTIVO,
						SUM(VLR_BRUTO_INCENTIVO) AS VLR_BRUTO_INCENTIVO,
						sum(VRL_LIQ_INCENTIVO)	 AS VRL_LIQ_INCENTIVO
					
					from openquery ( totvs, 'select * from X_V_BI_ESTOQUE_INCENTIVO')
					group by 
						CHASSI_ID_INCENTIVO
	) AS F 											ON F.CHASSI_ID_INCENTIVO COLLATE Latin1_General_BIN = E.CHASSIS_CRM		
	
																		
	---------------------------------------------------------------------------------------------------------------------------										
	
	LEFT JOIN (
					SELECT 
						QST.PROCESSO					as NRO_PROCESSO,
						TABF.FORMULARIO_NUMERO			as NRO_FORMULARIO,
						TBASE.SEQPESSOA					as SEQ_PESSOA,
					    (
					    	Case when TABF.VALOR_COMISSAO_CEN is null then 0 else TABF.VALOR_COMISSAO_CEN end 		+ 
							Case when TABF.VALOR_PREMIO_ESPECIA is null then 0 else TABF.VALOR_PREMIO_ESPECIA end 	+ 
							Case when TABF.COMISSAO_AMS_ADICION is null then 0 else	TABF.COMISSAO_AMS_ADICION end 	+ 
							Case when TABF.COMISSAO_ADICIONAL_E is null then 0 else TABF.COMISSAO_ADICIONAL_E end
						) 								as VLR_COMISSAO_CRM,
						Case 
							When TABF.VALOR_COMISSAO_CEN is not null 
								then 'CALCULADO' 
								else 'NÃO_CALCULADO'
						End						as STATUS_COMISSAO	
						
					 
					 FROM 
							IV_Q_CONT_COMISSAO_22 	TABF	WITH (NOLOCK)   	
					JOIN 	IV_QUESTIONARIO 		QST 	WITH (NOLOCK) 		ON QST.SEQQUESTIONARIO 	=  TABF.SEQQUESTIONARIO  	
					JOIN 	IV_FORMULARIO 			FRM 	WITH (NOLOCK) 		ON FRM.SEQFORMULARIO 	= QST.SEQFORMULARIO  	
					JOIN 	GE_PESSOA 				TBASE 	WITH (NOLOCK) 		ON TBASE.SEQPESSOA 		= QST.SEQPESSOA
	
	)	G 																		ON  G.NRO_FORMULARIO 	= A.FORMULARIO_NUMERO
																				AND G.NRO_PROCESSO		= B.Processo
																				AND G.SEQ_PESSOA		= D.SeqPessoa 
	---------------------------------------------------------------------------------------------------------------------------	
	inner join X_CRM_BI_CONGLOMERADO AS CON WITH (NOLOCK)						ON	D.SeqPessoa			= CON.seq_pessoa
																				AND	D.FisicaJuridica	= CON.tipo_pessoa
	---------------------------------------------------------------------------------------------------------------------------											
	left join (
					SELECT 
						FAT.filial, 
						cast(FAT.nro_nf as decimal(10,2)) as nro_nf, 
						FAT.seq_pessoa, 
						FAT.cliente_fj,
						FAT.cliente, 
						CON.cnpj_conglo, 
						CON.tipo_conglo,
						CON.cliente_conglo,
						FAT.nome_operacao, 
						FAT.data_emissao_nf, 
						 
						FAT.cod_produto, 
						FAT.nome_produto, 
						FAT.vlr_liquido_item
						
					FROM		X_TOTVS_BI_FATURAMENTO_MAQUINAS AS FAT WITH (NOLOCK)
					LEFT JOIN 	X_CRM_BI_CONGLOMERADO 			AS CON WITH (NOLOCK)	ON	FAT.seq_pessoa	= CON.cnpj_pessoa COLLATE SQL_Latin1_General_CP1_CI_AS
																						AND	FAT.cliente_fj	= CON.tipo_pessoa COLLATE SQL_Latin1_General_CP1_CI_AS		
					WHERE 
						FAT.nro_nf IN 	(SELECT MAX(nro_nf) FROM X_TOTVS_BI_FATURAMENTO_MAQUINAS B WHERE FAT.seq_pessoa = B.seq_pessoa AND FAT.cod_produto=B.cod_produto AND FAT.cliente_fj = B.cliente_fj)		
					and tipo_nf <> 'NF_Devolução'
	)	H											ON	H.cnpj_conglo	= 	CON.cnpj_conglo
													AND H.tipo_conglo	=	CON.tipo_conglo
													AND H.cod_produto 	=	E.CHASSIS_CRM COLLATE SQL_Latin1_General_CP1_CI_AS
	
	WHERE 
		A.FATURAMENTO_REALIZAD IS NOT NULL
	AND CHASSIS_CRM IS NOT NULL	
	/*AND		B.Processo IN ( 
						1154798,
						1154295,
						1120912,
						1154295,
						1151307
	
	)
	*/


) main


