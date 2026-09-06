/* ==============================================================
   Objeto ..........: dbo.PR_COL_ATU_LINKS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-29 10:35:55
   Modificado em ...: 2024-12-29 10:35:55
   Linhas ..........: 35
   Escreve em tabela: SIM (DELETE)
   Alvos de escrita : ge_pessoalink
   Tabelas referidas: GE_PESSOALINK
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE  procedure [PR_COL_ATU_LINKS] 
AS

/*
-- autor: AMAURY 25/10/2024
   objetivo: Eliminar os links de outras origens que não serão mais usados pelo cliente.
   log de alterações: 25/10/2024 - Criação da rotina, aguardando cliente identificar quais links devem ser higienizados da base
*/
	DECLARE 
	@SeqPessoalink	numeric(12,0)

	Declare curFORM
	CURSOR
	FOR
			SELECT SEQPESSOALINK
			FROM GE_PESSOALINK
			WHERE ORIGEM NOT IN ( 'PES_JUNCAO', 'INTEGRAÇÃONOROESTE', 'Protheus' )

begin
	OPEN curFORM
		FETCH NEXT FROM curFORM INTO 	@SeqPessoalink

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		Delete from ge_pessoalink 
		where seqpessoalink = @SeqPessoalink

		FETCH NEXT FROM curFORM INTO 	@SeqPessoalink
						
	END

	CLOSE curFORM
	DEALLOCATE curFORM

End