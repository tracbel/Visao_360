/* ==============================================================
   Objeto ..........: dbo.PR_COL_ATULATLONG
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 17:16:30
   Modificado em ...: 2024-12-28 17:16:30
   Linhas ..........: 56
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : ge_pessoa
   Tabelas referidas: ge_pessoa, ge_pessoa_ita, ge_pessoalink, mig_ge_pessoa
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create procedure [dbo].[PR_COL_ATULATLONG] 
AS	DECLARE 
	@seqpessoa	numeric(8,0),
	@Seqatual	numeric(8,0),
	@Latitude	numeric ( 14, 11),
	@Longitude	numeric ( 14, 11)
	
	Declare curFORM
	
	CURSOR
	FOR		select mp.seqpessoa, pit.latitude, pit.Longitude, pes.seqpessoa as seqatual
		from mig_ge_pessoa mp
		join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa
							 and pit.latitude is not null
        join ge_pessoalink pp on pp.pessoalink = cast( mp.seqpessoa as varchar(10))
		  and pp.Origem = 'INTEGRAÇÃONOROESTE'
		join ge_pessoa pes on pes.seqpessoa = pp.seqpessoa
---		where pes.seqpessoa = 106446

	Declare curFORM2
	
	CURSOR
	FOR		select mp.seqpessoa, pit.latitude, pit.Longitude, pes.seqpessoa as seqatual
		from mig_ge_pessoa mp
		join ge_pessoa_ita pit on pit.SeqPessoa = mp.seqpessoa
							 and pit.latitude is not null
		join ge_pessoa pes on pes.NROCGCCPF = pit.NROCGCCPF AND PES.DigCGCCPF = PIT.DigCGCCPF
---		where pes.seqpessoa = 106446

begin
	OPEN curFORM
		FETCH NEXT FROM curFORM INTO 	@seqpessoa, @Latitude, @Longitude, @Seqatual

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		update ge_pessoa set latitude = @Latitude, longitude = @Longitude where seqpessoa = @seqatual

		FETCH NEXT FROM curFORM INTO 	@seqpessoa, @Latitude, @Longitude, @Seqatual
	END

	CLOSE curFORM
	DEALLOCATE curFORM

	OPEN curFORM2
		FETCH NEXT FROM curFORM2 INTO 	@seqpessoa, @Latitude, @Longitude, @Seqatual

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		update ge_pessoa set latitude = @Latitude, longitude = @Longitude where seqpessoa = @seqatual

		FETCH NEXT FROM curFORM2 INTO 	@seqpessoa, @Latitude, @Longitude, @Seqatual
	END

	CLOSE curFORM2
	DEALLOCATE curFORM2
End