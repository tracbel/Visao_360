/* ==============================================================
   Objeto ..........: dbo.Trunc_Date
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2011-12-19 11:48:36
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 6
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION dbo.Trunc_Date (@Data datetime) 
    RETURNS datetime AS  
BEGIN  
   RETURN convert ( datetime  , convert ( char   , @Data , 101))
END
