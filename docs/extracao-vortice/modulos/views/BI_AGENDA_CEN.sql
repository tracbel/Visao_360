/* ==============================================================
   Objeto ..........: dbo.BI_AGENDA_CEN
   Tipo ............: VIEW
   Criado em .......: 2015-08-07 11:01:19
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 27
   Escreve em tabela: nao
   Tabelas referidas: IV_AGENDA, IVS_CARTEIRA, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE VIEW BI_AGENDA_CEN AS
SELECT AGD.SEQPESSOA
       ,AGD.SEQAGENDA AS AGDCEN_SEQAGENDA
       ,AGD.ASSUNTO AS AGDCEN_ASSUNTO
       ,AGD.DTAAGENDA AS AGDCEN_DATA_AGENDA
       ,CASE WHEN ( GETDATE() ) - (AGD.DTAAGENDA)  > 10 THEN (GETDATE() - 10)
	     WHEN (AGD.DTAAGENDA) -  ( GETDATE() )  > 45 THEN (GETDATE() + 45)
	     ELSE (AGD.DTAAGENDA)
          END AS AGDCEN_DATA_BASESEM
       ,AGD.DETALHE AS AGDCEN_DETALHE
       ,CASE WHEN AGD.CLASSE = 'P' THEN 'Visita'
              WHEN AGD.CLASSE = 'T' THEN 'Telefone'
         else 'Outro'
           end AS AGDCEN_CLASSE
      , CASE  WHEN ( GETDATE() ) - (AGD.DTAAGENDA) < 0 THEN 'Futura'
	      WHEN  ( GETDATE() ) - (AGD.DTAAGENDA) > 15 THEN '> 15 dias'
	      WHEN  ( GETDATE() ) - (AGD.DTAAGENDA) > 7 THEN '7 a 15 dias'
	      WHEN  ( GETDATE() ) - (AGD.DTAAGENDA) > 0 THEN '1 a 7 dias'
	  ELSE 'Em dia'
	END AS AGDCEN_PONTUALIDADE
     ,( GETDATE() ) -  (AGD.DTAAGENDA) AS AGDCEN_DIAS_ATRZ
FROM IV_AGENDA AGD
JOIN IVS_PES PESC ON PESC.SEQPESSOA = AGD.SEQPESSOA AND PESC.SEQDEPTO = 2
JOIN IVS_CARTEIRA CART ON CART.SEQCARTEIRA = PESC.SEQCARTEIRA AND CART.SEQUSRRESP = AGD.SEQUSUARIO
WHERE AGD.REALIZADA = 'N'
  AND AGD.ACAO > 0