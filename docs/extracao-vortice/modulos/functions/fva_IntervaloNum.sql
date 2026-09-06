/* ==============================================================
   Objeto ..........: dbo.fva_IntervaloNum
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE  FUNCTION  dbo.fva_IntervaloNum (@Base Int, @Primeiro Int, @Segundo Int, @ret1 Int, @Ret2 Int)   RETURNS Int AS   BEGIN      DECLARE @Retorno int    IF @Base >= @Primeiro and @Base <= @Segundo 	Set @Retorno = (@ret1)    ELSE         Set @Retorno = (@Ret2)      Return (@Retorno) END   