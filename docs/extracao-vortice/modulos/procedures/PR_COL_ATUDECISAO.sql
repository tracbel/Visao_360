/* ==============================================================
   Objeto ..........: dbo.PR_COL_ATUDECISAO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 16:58:33
   Modificado em ...: 2024-12-28 16:58:33
   Linhas ..........: 57
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : GE_CONTATO
   Tabelas referidas: GE_CONTATO, GE_CONTATO_ITA, ge_pessoa, ge_pessoa_ita
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_ATUDECISAO] 
AS


/*
-- autor: AMAURY 13/11/2024
   objetivo: Migrar DECISAO CONTATO
   log de alterações: 13/11/2024 - criacao da rotina
*/	

	DECLARE 
	@SEQPESSOA		 numeric(10, 0) ,
	@SEQPESSOA2		 numeric(10, 0) ,
	@NOMERAZAO		 Varchar(100) ,
	@CONTATO         VARCHAR(40),
	@NIVELDECISAO    CHAR(1) 

	Declare curPES
	CURSOR
	FOR
		select aa.seqpessoa, pes.seqpessoa as seqpessoa2, pes.nomerazao, CTT.CONTATO, CTT.NivelDecisao
			from ge_pessoa_ita aa
			join ge_pessoa pes on pes.nrocgccpf = aa.nrocgccpf
			JOIN GE_CONTATO_ITA CTT ON CTT.SEQPESSOA = AA.SEQPESSOA
			JOIN GE_CONTATO CTTOK ON CTTOK.SeqPessoa = PES.SeqPessoa
			                       AND CTTOK.CONTATO = CTT.CONTATO
---			where aa.seqpessoa = 869
begin

	OPEN curPES
		FETCH NEXT FROM curPES INTO 		
													 @SEQPESSOA
													,@SEQPESSOA2
													,@NOMERAZAO
													,@CONTATO
													,@NIVELDECISAO


	WHILE (@@FETCH_STATUS = 0)
	BEGIN
			UPDATE GE_CONTATO SET NIVELDECISAO = @NIVELDECISAO WHERE SEQPESSOA = @SEQPESSOA2
			                                                     AND CONTATO = @CONTATO

		FETCH NEXT FROM curPES INTO 		
													 @SEQPESSOA
													,@SEQPESSOA2
													,@NOMERAZAO
													,@CONTATO
													,@NIVELDECISAO
						
	END

	CLOSE curPES
	DEALLOCATE curPES

End
