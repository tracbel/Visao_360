/* ==============================================================
   Objeto ..........: dbo.bi_faturamentos
   Tipo ............: VIEW
   Criado em .......: 2019-10-28 13:27:47
   Modificado em ...: 2019-10-28 13:27:47
   Linhas ..........: 26
   Escreve em tabela: nao
   Tabelas referidas: IV_CLASSERES, IV_HISTORICO, IV_PROCDADO, IV_RESCLASSE, IV_RESULTADO, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW bi_faturamentos as                    				 				
SELECT HIST.SEQPESSOA
     , HIST.PROCESSO
     , PDD.ProcessoDNA
     , PDD.ProcessoPai
     , CRES.CLASSE
     , RES.RESULTADO 		AS COD_RESULTADO
     , RES.DESCRICAO 		AS RESULTADO
     , HIST.RESULTADOCMPL     
     , HIST.DTAREALIZACAO 	AS DTAREALIZACAO
     , HIST.VENDEDOR
     , PDD.CAMPANHA
FROM IV_HISTORICO HIST
  JOIN IV_PROCDADO PDD ON PDD.PROCESSO = HIST.PROCESSO
  JOIN IV_RESULTADO RES ON RES.RESULTADO = HIST.RESULTADO
  JOIN IV_RESCLASSE RCL ON RCL.RESULTADO = RES.RESULTADO
  JOIN IV_CLASSERES CRES ON CRES.SEQCLASSERES = RCL.SEQCLASSERES
	AND CRES.CLASSE IN ( '4-Faturamento' )
  JOIN IVS_PES PES ON PES.SEQPESSOA = HIST.SEQPESSOA AND PES.SEQDEPTO = 2  
WHERE YEAR(HIST.DTAREALIZACAO) >= YEAR('2015')
  AND HIST.SEQHISTORICO = ( SELECT MAX(H.SEQHISTORICO)
			FROM IV_HISTORICO H
			  JOIN  IV_RESCLASSE RCL ON RCL.RESULTADO = H.RESULTADO
			  JOIN IV_CLASSERES CRES ON CRES.SEQCLASSERES = RCL.SEQCLASSERES
				AND CRES.CLASSE IN ( '4-Faturamento' )                  
			WHERE H.PROCESSO = HIST.PROCESSO )  