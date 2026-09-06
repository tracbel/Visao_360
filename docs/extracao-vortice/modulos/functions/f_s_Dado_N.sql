/* ==============================================================
   Objeto ..........: dbo.f_s_Dado_N
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2013-02-08 11:30:47
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 15
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create Function f_s_Dado_N(@conteudo varchar(1000) )
RETURNS varchar(1000)
AS
BEGIN
	Declare @Retorno varchar(1000)

	Set @Retorno = ''
	
	if @conteudo is not null 
	begin
		set @retorno = '| ' + char(39) + rtrim(@conteudo) + char(39)
	end
	return @Retorno
END
