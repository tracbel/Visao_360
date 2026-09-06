/* ==============================================================
   Objeto ..........: dbo.PR_ATUALIZA_TITULO_INATIVO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2017-11-23 12:51:05
   Modificado em ...: 2019-10-30 15:49:17
   Linhas ..........: 13
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : EXT_Titulo
   Tabelas referidas: EXT_Titulo
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_ATUALIZA_TITULO_INATIVO] ( @LinkStr Varchar(40), @Retorno Varchar(40) ='' OUTPUT )
AS

Begin
	If Len(@LinkStr) > 0
	Begin
		Update EXT_Titulo Set IndAtivo = 0, Status = 'Excluido_COL' Where LinkStr = @LinkStr
	End

	Set @Retorno = (SELECT 'Título ' + NROTITULO + ' inativado'
					  FROM EXT_TITULO
					 WHERE LINKSTR = @LinkStr)
End