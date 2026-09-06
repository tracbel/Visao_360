/* ==============================================================
   Objeto ..........: dbo.VBI$CONTAFAM
   Tipo ............: VIEW
   Criado em .......: 2017-07-11 15:41:48
   Modificado em ...: 2017-07-11 15:41:48
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: VBI_CONTA, VBI_CONTAFAM, VBI_CONTAPAI
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW VBI$CONTAFAM AS   SELECT CTA.CODCONTA,         CAST(CTA.NROORDEM AS CHAR(2) ) + '-' + CTA.DESCRICAO AS CONTA,         CTA.ORIGEM,         CTA.CHAVEEXTERNA,         CTA.SINAL,         CTA.CORRGB,         CTA.NIVEL,          (SELECT CAST (P.NROORDEM AS CHAR(1) ) + '-' + P.DESCRICAO FROM VBI_CONTAPAI P WHERE P.SEQCONTA = F.SEQCONTA1) AS CONTA_1,          (SELECT CAST (P.NROORDEM AS CHAR(2)  ) + '-' + P.DESCRICAO FROM VBI_CONTAPAI P WHERE P.SEQCONTA = F.SEQCONTA2) AS CONTA_2,          (SELECT CAST (P.NROORDEM AS CHAR(2)  ) + '-' + P.DESCRICAO FROM VBI_CONTAPAI P WHERE P.SEQCONTA = F.SEQCONTA3) AS CONTA_3,          (SELECT CAST (P.NROORDEM AS CHAR(2)  ) + '-' + P.DESCRICAO FROM VBI_CONTAPAI P WHERE P.SEQCONTA = F.SEQCONTA4) AS CONTA_4,          (SELECT CAST (P.NROORDEM AS CHAR(2)  ) + '-' + P.DESCRICAO FROM VBI_CONTAPAI P WHERE P.SEQCONTA = F.SEQCONTA5) AS CONTA_5  FROM VBI_CONTA CTA   left JOIN VBI_CONTAFAM F ON F.SEQCONTA = CTA.SEQCONTA