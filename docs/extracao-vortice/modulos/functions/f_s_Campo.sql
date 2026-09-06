/* ==============================================================
   Objeto ..........: dbo.f_s_Campo
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2012-01-16 16:21:36
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 19
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE Function [dbo].[f_s_Campo]( @conteudo varchar(250), @campo varchar(250) )
RETURNS varchar(1000)
AS
BEGIN
	Declare @Retorno varchar(1000)

	Set @Retorno = ''

	if @conteudo != ''
		set @retorno = '| ' + RTrim (@campo)

	return @Retorno
END
