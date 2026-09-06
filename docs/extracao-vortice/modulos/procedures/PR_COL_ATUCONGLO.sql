/* ==============================================================
   Objeto ..........: dbo.PR_COL_ATUCONGLO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 16:57:29
   Modificado em ...: 2024-12-30 17:28:01
   Linhas ..........: 139
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : ge_pessoa
   Tabelas referidas: ge_pessoa, ge_pessoa_ita, ge_pessoalink, mig_ge_pessoa
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_ATUCONGLO] 
AS


/*
-- autor: AMAURY 12/11/2024
   objetivo: Migrar CONGLOMERADO
   log de alterações: 12/11/2024 - criacao da rotina
*/	

	DECLARE 
	@SEQPESSOA		 numeric(10, 0) ,
	@SEQPRINCIPAL	 numeric(10, 0) 

	------------ primeiro
	Declare curPES
	CURSOR
	FOR
			select ---mp.seqpessoa as seqorigem, 
			       ---pit.nomerazao, 
				   ---pit.seqpessoaprc, 
				   ---pit2.seqpessoa as seqprincipal, 
				   ---pit2.nomerazao as razaoprincipal,
				   lk.seqpessoa as seqpessoa,
				   lk2.seqpessoa as seqpessoaprc
			from mig_ge_pessoa mp
			join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa  
			                      AND PIT.SEQPESSOA <> PIT.SEQPESSOAPRC
            join ge_pessoalink lk on lk.origem = 'INTEGRAÇÃONOROESTE'
			                     and lk.pessoalink = cast( pit.seqpessoa as varchar(10))
            join ge_pessoa_ita pit2 on pit2.seqpessoa = pit.seqpessoaprc
            join ge_pessoalink lk2 on lk2.origem = 'INTEGRAÇÃONOROESTE'
			                     and lk2.pessoalink = cast( pit2.seqpessoa as varchar(10))
			WHERE mp.SEQPESSOA  in ( 3302, 3153, 13239, 21292, 1501) 

	------------ segundo

	Declare curPES2
	CURSOR
	FOR
	   select --   mp.seqpessoa as seqorigem, 
		--	       pit.nomerazao, 
		--		   pit.seqpessoaprc, 
		--		   pit2.seqpessoa as seqprincipal, 
		--		   pit2.nomerazao as razaoprincipal,
				   lk.seqpessoa as seqpessoa,
				   lk2.seqpessoa as seqpessoaprc
			from mig_ge_pessoa mp
			join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa  
			                      AND PIT.SEQPESSOA <> PIT.SEQPESSOAPRC
            join ge_pessoa lk on lk.nrocgccpf = pit.nrocgccpf and lk.digcgccpf = pit.digcgccpf
            join ge_pessoa_ita pit2 on pit2.seqpessoa = pit.seqpessoaprc
            join ge_pessoalink lk2 on lk2.origem = 'INTEGRAÇÃONOROESTE'
			                     and lk2.pessoalink = cast( pit2.seqpessoa as varchar(10))
			WHERE mp.SEQPESSOA  in ( 3302, 3153, 13239, 21292, 1501) 

---------- terceiro
	Declare curPES3
	cursor
	for
			select 
				   lk.seqpessoa as seqpessoa,
				   pes.seqpessoa as seqpessoaprc
			from mig_ge_pessoa mp
			join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa  
			                      AND PIT.SEQPESSOA <> PIT.SEQPESSOAPRC
            join ge_pessoalink lk on lk.origem = 'INTEGRAÇÃONOROESTE'
			                     and lk.pessoalink = cast( pit.seqpessoa as varchar(10))
            join ge_pessoa_ita pit2 on pit2.seqpessoa = pit.seqpessoaprc
			join ge_pessoa pes on pes.nrocgccpf = pit2.nrocgccpf and pes.digcgccpf = pit2.digcgccpf
			WHERE mp.SEQPESSOA  in ( 3302, 3153, 13239, 21292, 1501) 

---------- quarto
	Declare curPES4
	cursor
	for
			select 
				    pes.seqpessoa as seqpessoa,
				   pes2.seqpessoa as seqpessoaprc
			from mig_ge_pessoa mp
			join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa  
			                      AND PIT.SEQPESSOA <> PIT.SEQPESSOAPRC
			join ge_pessoa pes on pes.nrocgccpf = pit.nrocgccpf and pes.digcgccpf = pit.digcgccpf
			join ge_pessoa_ita pit2 on pit2.seqpessoa = pit.seqpessoaprc
			join ge_pessoa pes2 on pes2.nrocgccpf = pit2.nrocgccpf and pes2.digcgccpf = pit2.digcgccpf
			WHERE 1=1
			and mp.SEQPESSOA  in ( 3302, 3153, 13239, 21292, 1501) 
----			  and mp.descricao = 'Ativo/Prospect CNPJ Existente'

begin
	OPEN curPES
		FETCH NEXT FROM curPES INTO 	@seqpessoa, @SeqPrincipal

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		update ge_pessoa set SEQPESSOAPRC = @SeqPrincipal where seqpessoa = @seqpessoa
		FETCH NEXT FROM curPES INTO 	@seqpessoa, @SeqPrincipal
	END

	CLOSE curPES
	DEALLOCATE curPES

	OPEN curPES2
		FETCH NEXT FROM curPES2 INTO 	@seqpessoa, @SeqPrincipal

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		update ge_pessoa set SEQPESSOAPRC = @SeqPrincipal where seqpessoa = @seqpessoa
		FETCH NEXT FROM curPES2 INTO 	@seqpessoa, @SeqPrincipal
	END

	CLOSE curPES2
	DEALLOCATE curPES2

	OPEN curPES3
		FETCH NEXT FROM curPES3 INTO 	@seqpessoa, @SeqPrincipal

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		update ge_pessoa set SEQPESSOAPRC = @SeqPrincipal where seqpessoa = @seqpessoa
		FETCH NEXT FROM curPES3 INTO 	@seqpessoa, @SeqPrincipal
	END

	CLOSE curPES3
	DEALLOCATE curPES3

	OPEN curPES4
		FETCH NEXT FROM curPES4 INTO 	@seqpessoa, @SeqPrincipal

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		update ge_pessoa set SEQPESSOAPRC = @SeqPrincipal where seqpessoa = @seqpessoa
		FETCH NEXT FROM curPES4 INTO 	@seqpessoa, @SeqPrincipal
	END

	CLOSE curPES4
	DEALLOCATE curPES4
End
