/* ==============================================================
   Objeto ..........: dbo.fva_GetHora
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2023-03-24 10:58:12
   Modificado em ...: 2023-03-24 11:30:47
   Linhas ..........: 9
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION [dbo].[fva_GetHora] (@Data datetime, @Data2 datetime) 
    RETURNS datetime AS  
BEGIN  
   RETURN convert ( datetime, 
          convert(varchar(10), convert ( datetime, @Data, 120 ) ,120) + ' ' +
          right( '00' + cast ( datepart( hour, @Data  ) as varchar), 2) + ':' +
          right( '00' + cast ( datepart( Minute, @Data ) as varchar), 2 ), 120 )
END
