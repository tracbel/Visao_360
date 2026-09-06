/* ==============================================================
   Objeto ..........: dbo.TESTE_proc
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2012-04-10 15:44:03
   Modificado em ...: 2016-03-22 19:20:04
   Linhas ..........: 10
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : TESTE_acesso
   Tabelas referidas: TESTE_acesso
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create Procedure [dbo].[TESTE_proc] ( @Soma Numeric,@Seq Numeric = 0 OUTPUT ) 
As 
Begin
	SET @SEQ = ( select count(*) from openquery(SPRESS, 'select * from TNFTRNFSA') )
    If @Seq > 0  
    Begin
		INSERT INTO TESTE_acesso (QTDE) VALUES ( @Seq + @Soma )
    End
End
