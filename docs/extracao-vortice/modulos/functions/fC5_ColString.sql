/* ==============================================================
   Objeto ..........: dbo.fC5_ColString
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2024-12-26 15:12:55
   Modificado em ...: 2024-12-26 15:12:55
   Linhas ..........: 17
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE                function [dbo].[fC5_ColString](@vsTemp varchar(250), @vsSeparador varchar(1))
returns varchar(250) as
Begin
-- funcao utilizada para gravar dados do tipo STRING na gep_import seguindo o padrao e tratamentos necessários   
   Declare @vnCount int

   set @vsTemp = replace(replace(rtrim(ltrim(@vsTemp)),';',''),char(39),'')
   set @vsTemp = replace(@vsTemp,char(34),'')
   if @vsTemp is null
      set @vsTemp = @vsSeparador+'null'
      else
      SET @vsTemp = @vsSeparador+char(39)+@vsTemp+char(39)

   return(@vsTemp)
end
