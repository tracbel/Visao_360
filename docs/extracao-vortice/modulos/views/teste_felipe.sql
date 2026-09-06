/* ==============================================================
   Objeto ..........: dbo.teste_felipe
   Tipo ............: VIEW
   Criado em .......: 2019-06-27 15:21:41
   Modificado em ...: 2019-06-27 15:21:41
   Linhas ..........: 141
   Escreve em tabela: nao
   Tabelas referidas: IV_CLASSERES, IV_GLOBALPAR, IV_GLOBALPARCTRL, IV_HISTORICO, IV_PROCDADO, IV_PROCESSO, IV_PROCPRODUTO, IV_RESCLASSE, IV_RESULTADO
   Outras refs .....: IV_Q$ACOMPANH_VENDA_JDE, IV_Q$ACOMPANHAM_VENDA_IMP
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW  teste_felipe as

SELECT PRPP.PROCESSO,
       CASE GR.PARAMETRO 
        WHEN 'Implementos' THEN 'Implementos' 
		WHEN 'Turf' THEN 'Equipamento'
		WHEN 'Plantadeira' THEN 'Equipamento'
        WHEN 'Implementos' THEN 'Equipamento'
        WHEN 'Agricultura precisão' THEN 'Equipamento'
        WHEN 'Colheitadeira grãos' THEN 'Equipamento'
        WHEN 'Pulverizador' THEN 'Equipamento'
        WHEN 'Colhedora de cana' THEN 'Equipamento'
        WHEN 'Plataforma adicional' THEN 'Equipamento'
        WHEN 'Tratores' THEN 'Equipamento'
        WHEN 'Feno e forragem' THEN 'Equipamento'
       ELSE NULL END PROD_GRUPO,
       CASE GR.PARAMETRO 
         WHEN 'Implementos' THEN PRD.LITERAL2 
       ELSE GR.PARAMETRO END AS PROD_TIPO,
       PRD.CAMPO1 AS PROD_MARCA,
       ISNULL(PRD.LITERAL1, PRPP.CODPRODUTO) AS PROD_MODELO,             
       PRPP.QTDE AS PROD_QTDE,
       PRPP.VALOR AS PROD_VALOR,
       'Não' AS PROD_VENDIDO
  FROM IV_PROCPRODUTO PRPP
  LEFT JOIN IV_GLOBALPAR PRD ON PRD.SEQPAR = PRPP.SEQPRODUTO
  LEFT JOIN IV_GLOBALPARCTRL GR ON GR.SEQGLBPAR = PRD.SEQGLBPAR
 WHERE PROCESSO IN (SELECT PDD.PROCESSO
                      FROM IV_PROCDADO PDD
                      JOIN IV_PROCESSO PRC ON PRC.PROCESSO = PDD.PROCESSO
                     WHERE 1 = 1
                       AND ((PDD.CODPROCESSO IN ( 7, 8 ) AND PRC.FASEORDEM < 18 )
                        OR (PRC.STATUS IN ('DESISTIU DA COMPRA', 'VENDA PERDIDA')
 --                       	 //Somente produtos com o cancelamento antes do pedido realizado
                            AND (PRC.FASEORDEM < 18))
	     				OR (PDD.CODPROCESSO = 9400))
--                       //AND PRC.REALIZADO = 0                       
                       )            
--//   AND NOT EXISTS ( SELECT 1 FROM  IV_Q$ACOMPANH_VENDA_JDE WHERE PROCESSO = PRPP.PROCESSO )
--//   AND NOT EXISTS ( SELECT 1 FROM  IV_Q$ACOMPANHAM_VENDA_IMP WHERE PROCESSO = PRPP.PROCESSO )
   
UNION

SELECT ACV.PROCESSO,
       'Equipamento' AS GRUPO,
       ACV.TIPO_EQUIP1 AS VDA_TIPO_MAQUINA,
       ACV.MARCA_1 AS VDA_MARCA,
       ACV.MODEL_EQUIP1 AS VDA_MODELO,
       1 as PRPP_QTDE,
       ACV.VLR_TOTAL1 AS VDA_VALOR,
       'Sim' AS PROD_VENDIDO
FROM IV_Q$ACOMPANH_VENDA_JDE ACV
JOIN IV_Processo PRC
ON PRC.Processo = ACV.Processo
--//Somente produtos com o cancelamento após do pedido realizado
WHERE PRC.PROCESSO IN (SELECT PDD.PROCESSO
						FROM IV_PROCDADO PDD
						JOIN IV_PROCESSO PRC ON PRC.PROCESSO = PDD.PROCESSO
						WHERE (PDD.CODPROCESSO = 7
							AND PRC.FASEORDEM >= 18)
						  OR (PRC.STATUS IN ('DESISTIU DA COMPRA', 'VENDA PERDIDA')
								AND PRC.FaseOrdem > 18)
                        )

--//REMOVIDA NO DIA 14/03/2019 POR NÃO TRAZER OS PRODUTOS "NORMAIS"
--//(PRC.STATUS IN ('DESISTIU DA COMPRA', 'VENDA PERDIDA')
--//	    AND (PRC.FaseOrdem > 18))

--//  WHERE PROCESSO IN (SELECT PDD.PROCESSO
--//                       FROM IV_PROCDADO PDD
--//                       JOIN IV_PROCESSO PRC ON PRC.PROCESSO = PDD.PROCESSO
--//                      WHERE PDD.CODPROCESSO = 7
--//                        AND PRC.FASEORDEM >= 18
--//                        AND ( PRC.FASEORDEM <= 50 OR PRC.FASEORDEM > 60 )
--//                       )
                       
UNION

SELECT ACV.PROCESSO,
       'Implementos' AS GRUPO,
       ACV.TIP_IMPLEMENTO AS VDA_TIPO_MAQUINA,
       ACV.MARCA_IMPLEMENTO AS VDA_MARCA,
       ACV.MODELO_IMPLEMENTO AS VDA_MODELO,
       1 as PRPP_QTDE,
       ACV.VALOR_TOTAL_IMPLEM AS VDA_VALOR,
       'Sim' AS PROD_VENDIDO
FROM IV_Q$ACOMPANHAM_VENDA_IMP ACV
JOIN IV_Processo PRC
ON PRC.Processo = ACV.Processo
--//Somente produtos com o cancelamento após do pedido realizado
WHERE PRC.PROCESSO IN (SELECT PDD.PROCESSO
						FROM IV_PROCDADO PDD
						JOIN IV_PROCESSO PRC ON PRC.PROCESSO = PDD.PROCESSO
						WHERE (PDD.CODPROCESSO = 8
							AND PRC.FASEORDEM >= 18)
						  OR (PRC.STATUS IN ('DESISTIU DA COMPRA', 'VENDA PERDIDA')
								AND PRC.FaseOrdem > 18)
                                )

--//REMOVIDA NO DIA 14/03/2019 POR NÃO TRAZER OS PRODUTOS "NORMAIS"
--//(PRC.STATUS IN ('DESISTIU DA COMPRA', 'VENDA PERDIDA')
--//	    AND (PRC.FaseOrdem > 18))

--//  WHERE PROCESSO IN (SELECT PDD.PROCESSO
--//                       FROM IV_PROCDADO PDD
--//                       JOIN IV_PROCESSO PRC ON PRC.PROCESSO = PDD.PROCESSO
--//                      WHERE PDD.CODPROCESSO = 8
--//                        AND PRC.FASEORDEM >= 18
--//                        AND ( PRC.FASEORDEM <= 50 OR PRC.FASEORDEM > 60 )
--//                       )            

UNION

SELECT HIST.PROCESSO
     , 'Equipamento'
     , 'Tratores'
     , 'John Deere'
     , 'Modelo Padrão'
     , 1
     , 0
     , 'Não'
FROM IV_HISTORICO HIST
  JOIN IV_PROCDADO PDD ON PDD.PROCESSO = HIST.PROCESSO
  JOIN IV_RESULTADO RES ON RES.RESULTADO = HIST.RESULTADO
  JOIN IV_RESCLASSE RCL ON RCL.RESULTADO = RES.RESULTADO
  JOIN IV_CLASSERES CRES ON CRES.SEQCLASSERES = RCL.SEQCLASSERES
	AND CRES.CLASSE IN ( '1-Cobertura' )
WHERE YEAR(HIST.DTAREALIZACAO) >= YEAR(2015)
  AND HIST.SEQHISTORICO = ( SELECT MAX(H.SEQHISTORICO)
			FROM IV_HISTORICO H
			  JOIN  IV_RESCLASSE RCL ON RCL.RESULTADO = H.RESULTADO
			  JOIN IV_CLASSERES CRES ON CRES.SEQCLASSERES = RCL.SEQCLASSERES
				AND CRES.CLASSE IN ( '1-Cobertura' )               
			WHERE H.PROCESSO = HIST.PROCESSO ) 
  AND NOT EXISTS ( SELECT 1 FROM IV_PROCPRODUTO WHERE PROCESSO = HIST.PROCESSO )
  AND NOT EXISTS ( SELECT 1 FROM  IV_Q$ACOMPANH_VENDA_JDE WHERE PROCESSO = HIST.PROCESSO )
  AND NOT EXISTS ( SELECT 1 FROM  IV_Q$ACOMPANHAM_VENDA_IMP WHERE PROCESSO = HIST.PROCESSO )  

  ;

--and PROCESSO=457651			   ;