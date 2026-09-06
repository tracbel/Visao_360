/* ==============================================================
   Objeto ..........: dbo.f_IntervaloNum
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2012-04-04 17:33:04
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE  FUNCTION  dbo.f_IntervaloNum (@Base Int, @Primeiro Int, @Segundo Int, @ret1 Int, @Ret2 Int)    RETURNS Int AS    BEGIN       DECLARE @Retorno int     IF @Base >= @Primeiro and @Base <= @Segundo  	Set @Retorno = (@ret1)     ELSE          Set @Retorno = (@Ret2)       Return (@Retorno)  END 