/* ==============================================================
   Objeto ..........: dbo.f_IntervaloNumX
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2011-12-19 11:22:55
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 15
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */



--- Arquivo: M:\Glb\SqlServer\Pack\000_f_IntervaloNumX retorno string SQLServer.sql
--- Atualizado em: 14/12/2011  10:35
create  FUNCTION   f_IntervaloNumX (@Base Int, @Primeiro Int, @Segundo Int, @ret1 Varchar(100), @Ret2 Varchar(100))  
RETURNS varchar(100) AS  
BEGIN  
   DECLARE @Retorno varchar(100)
   IF @Base >= @Primeiro and @Base <= @Segundo
	Set @Retorno = (@ret1)
   ELSE
        Set @Retorno = (@Ret2)  
   Return (@Retorno)
END 
