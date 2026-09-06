/* ==============================================================
   Objeto ..........: dbo.fC5_ColNumber
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2024-12-26 15:12:55
   Modificado em ...: 2024-12-26 15:12:55
   Linhas ..........: 17
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


create       function [dbo].[fC5_ColNumber](@vsTemp varchar(250),@vsSeparador varchar(1))
returns varchar(250) as
Begin
   
   Declare @vnCount int

   set @vsTemp = replace(replace(rtrim(ltrim(@vsTemp)),';',''),char(39),'')
   if @vsTemp is null
      set @vsTemp = @vsSeparador+'null'
      else
      SET @vsTemp = @vsSeparador+@vsTemp

   return(@vsTemp)
end

