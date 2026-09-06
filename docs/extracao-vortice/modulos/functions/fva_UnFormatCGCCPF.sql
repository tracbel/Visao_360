/* ==============================================================
   Objeto ..........: dbo.fva_UnFormatCGCCPF
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2012-05-24 16:00:34
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 12
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION   [dbo].[fva_UnFormatCGCCPF] (@pnNroCGCCPF Varchar(30) )
RETURNS varchar(30) AS
BEGIN
	DECLARE @vsCGC varchar(30)
	DECLARE @Retorno varchar(30)

	Set @vsCGC = REPLACE (REPLACE (REPLACE (@pnNroCGCCPF, '.', ''), '-', ''), '/', '')

	Set @Retorno = @vsCGC

	Return (@Retorno)
END