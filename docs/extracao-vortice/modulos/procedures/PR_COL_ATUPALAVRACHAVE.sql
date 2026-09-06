/* ==============================================================
   Objeto ..........: dbo.PR_COL_ATUPALAVRACHAVE
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 17:22:51
   Modificado em ...: 2024-12-28 17:22:51
   Linhas ..........: 71
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : ge_pessoa
   Tabelas referidas: GE_PESSOA, ge_pessoa_ita, ge_pessoalink, mig_ge_pessoa
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_ATUPALAVRACHAVE] 
AS


/*
-- autor: AMAURY 12/11/2024
   objetivo: Migrar CONGLOMERADO
   log de alterações: 12/11/2024 - criacao da rotina
*/	

	DECLARE 
	@SEQPESSOA		 numeric(10, 0) ,
	@Palavrachave	 varchar(50)

	Declare curPES
	CURSOR
	FOR
			select 
				   lk.seqpessoa as seqpessoa,
				   pit.PalavraChave as PalavrachaveNova
			from mig_ge_pessoa mp
			join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa  
			                      AND ISNULL( PIT.PALAVRACHAVE, '@') <> '@'
            join ge_pessoalink lk on lk.origem = 'INTEGRAÇÃONOROESTE'
			                     and lk.pessoalink = cast( pit.seqpessoa as varchar(10))
	        JOIN GE_PESSOA PES ON PES.SEQPESSOA = LK.SEQPESSOA
			where trim(isnull(pes.PalavraChave, '@')) <> trim(pit.PalavraChave)


	Declare curPES2
	CURSOR
	FOR
			select 
				   pes.seqpessoa as seqpessoa,
				   pit.PalavraChave as Palavrachave
			from mig_ge_pessoa mp
			join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa  
			                      AND ISNULL( PIT.PALAVRACHAVE, '@') <> '@'
	        JOIN GE_PESSOA PES ON pes.nrocgccpf = pit.nrocgccpf and pes.digcgccpf = pit.digcgccpf
			where 1=1
			 and trim(isnull(pes.PalavraChave, '@')) <> trim(pit.PalavraChave)

begin
	OPEN curPES
		FETCH NEXT FROM curPES INTO 	@seqpessoa, @Palavrachave

	WHILE (@@FETCH_STATUS = 0)
	BEGIN

		update ge_pessoa set PALAVRACHAVE = @Palavrachave where seqpessoa = @seqpessoa

		FETCH NEXT FROM curPES INTO 	@seqpessoa, @Palavrachave
	END

	CLOSE curPES
	DEALLOCATE curPES

	OPEN curPES2
		FETCH NEXT FROM curPES2 INTO 	@seqpessoa, @Palavrachave

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		update ge_pessoa set PALAVRACHAVE = @Palavrachave where seqpessoa = @seqpessoa

		FETCH NEXT FROM curPES2 INTO 	@seqpessoa, @Palavrachave
	END

	CLOSE curPES2
	DEALLOCATE curPES2
End
