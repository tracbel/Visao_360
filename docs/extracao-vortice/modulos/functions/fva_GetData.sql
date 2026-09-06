/* ==============================================================
   Objeto ..........: dbo.fva_GetData
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2023-03-24 11:06:59
   Modificado em ...: 2023-03-24 11:23:19
   Linhas ..........: 6
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION [dbo].[fva_GetData] (@Data datetime, @Hora varchar(5)) 
    RETURNS datetime AS  
BEGIN  
   RETURN convert ( datetime, convert(varchar(10), convert ( datetime, @Data, 120 ) ,120) + ' ' + @Hora, 120 )
END
