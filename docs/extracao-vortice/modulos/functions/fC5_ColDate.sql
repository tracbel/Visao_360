/* ==============================================================
   Objeto ..........: dbo.fC5_ColDate
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2024-12-26 15:12:55
   Modificado em ...: 2024-12-26 15:12:55
   Linhas ..........: 14
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE    function [dbo].[fC5_ColDate](@vsData datetime, @vsSeparador varchar)
returns varchar(250) as
Begin
   
   Declare @vnCount int
   Declare @vsTemp varchar(250)
   if @vsData is null
      set @vsTemp = @vsSeparador+'null'
      else
      set @vsTemp = @vsSeparador+'convert(datetime,'+char(39)+rtrim(convert(char,@vsData,120))+char(39)+',120)'

   return(@vsTemp)
end
