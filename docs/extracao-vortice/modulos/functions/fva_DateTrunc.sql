/* ==============================================================
   Objeto ..........: dbo.fva_DateTrunc
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fva_DateTrunc (@Data datetime)      RETURNS datetime AS   BEGIN      RETURN cast(floor(cast(getdate()  as float)) as datetime) END  