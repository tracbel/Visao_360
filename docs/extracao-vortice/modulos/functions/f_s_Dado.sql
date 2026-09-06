/* ==============================================================
   Objeto ..........: dbo.f_s_Dado
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2012-01-16 15:37:41
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 20
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE Function [dbo].[f_s_Dado](@conteudo varchar(1000) )
RETURNS varchar(1000)
AS
BEGIN
	Declare @Retorno varchar(1000)

	Set @Retorno = ''
	
	if @conteudo is not null 
	begin
		set @retorno = '| ' + char(39) + upper(rtrim(@conteudo)) + char(39)
	end
	return @Retorno
END
