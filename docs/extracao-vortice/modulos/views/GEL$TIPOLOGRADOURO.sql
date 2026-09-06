/* ==============================================================
   Objeto ..........: dbo.GEL$TIPOLOGRADOURO
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_TIPOLOGRADOURO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE  VIEW GEL$TIPOLOGRADOURO (TIPOLOGRADOURO, DESCRICAO, SUBSTITUICOES) AS  SELECT DISTINCT TL.TIPOLOGRADOURO,         TL.DESCRICAO,         TL.SUBSTITUICOES FROM GE_TIPOLOGRADOURO TL  