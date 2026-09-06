/* ==============================================================
   Objeto ..........: dbo.VBI$CONTA
   Tipo ............: VIEW
   Criado em .......: 2017-07-12 09:40:40
   Modificado em ...: 2017-07-12 09:40:40
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: VBI_CONTA, VBI_CONTAPAI
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE  VIEW VBI$CONTA AS  SELECT CPAI.GRUPOGESTAO,         CPAI.CODCONTA,         CPAI.IDENTIFICADOR,         CPAI.SEQCONTA,         CPAI.SEQCONTAPAI,         CPAI.NROORDEM,         CPAI.DESCRICAO,         CPAI.NIVEL,         1 AS SINTETICA,         CPAI.TIPO,         CPAI.CORRGB,         '' AS ORIGEM,         '' AS CHAVEEXTERNA,         0 AS SINAL,         OBS,         isnull(CPAI.CODCONTA, ' ' ) + ' - ' + isnull(CPAI.DESCRICAO, ' ' )  as DESCRICAOFULL,         null AS CHAVEEXTOBS  FROM VBI_CONTAPAI CPAI  UNION  SELECT CPAI.GRUPOGESTAO,         CTB.CODCONTA,          NULL AS IDENTIFICADOR,         CTB.SEQCONTA,         CTB.SEQCONTAPAI,         CTB.NROORDEM,         CTB.DESCRICAO,         CTB.NIVEL,         0 AS SINTETICA,         'A' AS TIPO,         CTB.CORRGB,         CTB.ORIGEM,         CTB.CHAVEEXTERNA,         CTB.SINAL,         CTB.OBS,         isnull(CTB.CODCONTA, ' ') + ' - ' + isnull(CTB.DESCRICAO, ' ') +             CASE WHEN CTB.SINAL < 0 THEN '(-)'  else '' END +          ' (' + isnull(CTB.ORIGEM, '')  + ') ' + isnull(CTB.CHAVEEXTERNA, '') + ' ' + isnull(CTB.CHAVEEXTOBS, '')  as DESCRICAOFULL,          CTB.CHAVEEXTOBS  FROM VBI_CONTA CTB  JOIN VBI_CONTAPAI CPAI ON CPAI.SEQCONTA = CTB.SEQCONTAPAI