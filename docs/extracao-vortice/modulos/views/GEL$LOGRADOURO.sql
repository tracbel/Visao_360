/* ==============================================================
   Objeto ..........: dbo.GEL$LOGRADOURO
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GEL_CEP, GEL_CEPORIG
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE  VIEW GEL$LOGRADOURO (          CEP,         TIPO,         SEQCID,         LOGRFONETICA,         LOGRADOURO,         CIDADE,         CIDFONETICA,        BAIRRO,         UF,        PAIS,        CEPCIDADE,        COMPLEMENTO  )   AS   SELECT CEP.CEP,        CEP.TIPOLOGRAD AS TIPO,        CEP.SEQCIDADE,        CEP.LOGRFONETICA,        CEP.LOGRADOURO,        CEP.CIDADE,        CEP.CIDADEFONETICA,        CEP.BAIRRO,        CEP.UF,        CEP.PAIS,        CEP.CEPCIDADE,        ORI.COMPLEMENTO FROM GEL_CEP CEP      JOIN GEL_CEPORIG ORI ON ORI.CEP = CEP.CEP   