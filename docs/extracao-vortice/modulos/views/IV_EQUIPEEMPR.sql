/* ==============================================================
   Objeto ..........: dbo.IV_EQUIPEEMPR
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:34
   Modificado em ...: 2025-02-17 17:35:34
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_VENDEDOREMPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

  CREATE VIEW IV_EQUIPEEMPR (SEQEQUIPE, NROEMPRESA, DTAALTERACAO, USUALTEROU) AS   SELECT VE.SEQVENDEDOR, VE.NROEMPRESA, VE.DTAALTERACAO, VE.USUALTEROU FROM IV_VENDEDOREMPR VE 