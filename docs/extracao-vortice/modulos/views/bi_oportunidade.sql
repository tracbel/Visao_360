/* ==============================================================
   Objeto ..........: dbo.bi_oportunidade
   Tipo ............: VIEW
   Criado em .......: 2015-10-13 18:29:54
   Modificado em ...: 2023-03-17 10:19:08
   Linhas ..........: 51
   Escreve em tabela: nao
   Tabelas referidas: IV_AGENDA, IV_PROCDADO, IV_PROCESSO, IV_PROCPERSP
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE view [dbo].[bi_oportunidade] as
SELECT PDD.SEQPESSOA,
       PDD.NROEMPRESA,
       PDD.PROCESSO,
       PDD.CAMPANHA AS ON_CAMPANHA,
       CASE
         WHEN PDD.CODPROCESSO = 9400 THEN
          '1-Monitoramento'
         ELSE
          case
         WHEN PRC.FASEORDEM <= 15 THEN
          '2-Negociação'
         WHEN PRC.FASEORDEM <= 25 THEN
          '3-Montagem Pedido'
         WHEN PRC.FASEORDEM <= 30 THEN
          '4-Aguardando Banco'
         WHEN PRC.FASEORDEM <= 40 THEN
          '5-Faturamento'
         WHEN PRC.FASEORDEM <= 60 THEN
          '6-Entrega'
         WHEN PRC.FASEORDEM <= 80 THEN
          '7-Pos-venda'
         else
          '9-Encerrado'
       end END AS ON_ETAPA,
---       PRC.FASE AS ON_FASE,
	   CASE WHEN (SELECT COUNT(1) FROM IV_AGENDA 
	              WHERE REALIZADA = 'N' 
		          AND PROCESSO = PDD.PROCESSO
		          AND ACAO IN ( 527, 528, 529, 521, 522, 523, 524, 525, 546 ) )>0 
				  AND PRC.FASE IN ( 'Concluido', 'Pós Venda' )
		    THEN 'Em Transição'
			ELSE PRC.FASE END AS ON_FASE,
       PRC.FASEORDEM AS ON_FASE_ORDEM,
       PRC.STATUS AS ON_STATUS,
       DATEADD(DD, DATEDIFF(DD, 0, PRC.DTAINCLUSAO), 0) AS ON_DATA_INICIO,
       DATEADD(DD, DATEDIFF(DD, 0, PRC.DTAREALIZACAO), 0) AS ON_DATA_REALIZADO,
       DATEADD(DD, DATEDIFF(DD, 0, PDD.DtaUltResultado), 0) AS ON_DATA_ULT_RESULTADO,
       PRC.VALOR AS ON_VALOR,
       PRC.PERSPECTIVA AS ON_PERSP_PERCT,
       PESP.PERSPECTIVA AS ON_PERSPECTIVA,
       PRC.USURESPONSAVEL AS ON_CEN
  FROM IV_PROCDADO PDD
  JOIN IV_PROCESSO PRC ON PRC.PROCESSO = PDD.PROCESSO
  LEFT JOIN IV_PROCPERSP PESP ON PESP.CODPROCESSO = PDD.CODPROCESSO
                             AND PESP.PERSPORDEM = PRC.PERSPECTIVA
 WHERE PDD.CODPROCESSO IN ( 7, 9400, 37, 31 )
   AND ( PRC.FASEORDEM <= 60 or PRC.FASEORDEM = 90 or PRC.FaseOrdem = 80 )
   AND PRC.REALIZADO = 0
   AND EXISTS (SELECT 1 FROM IV_AGENDA WHERE REALIZADA = 'N' AND PROCESSO = PDD.PROCESSO)
