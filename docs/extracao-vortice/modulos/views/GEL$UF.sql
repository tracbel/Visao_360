/* ==============================================================
   Objeto ..........: dbo.GEL$UF
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GEL_CEP
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

   CREATE  VIEW GEL$UF ( UF)   AS   SELECT DISTINCT CEP.UF FROM GEL_CEP CEP   