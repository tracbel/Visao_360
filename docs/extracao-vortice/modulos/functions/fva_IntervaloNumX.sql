/* ==============================================================
   Objeto ..........: dbo.fva_IntervaloNumX
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 create  FUNCTION   dbo.fva_IntervaloNumX (@Base Int, @Primeiro Int, @Segundo Int, @ret1 Varchar(100), @Ret2 Varchar(100)) RETURNS varchar(100) AS   BEGIN      DECLARE @Retorno varchar(100)    IF @Base >= @Primeiro and @Base <= @Segundo 	Set @Retorno = (@ret1)    ELSE         Set @Retorno = (@Ret2)      Return (@Retorno) END   