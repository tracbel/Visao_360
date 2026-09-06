/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGCONTCLASSE
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 17:00:44
   Modificado em ...: 2024-12-28 17:10:06
   Linhas ..........: 144
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : ge_contatopapel
   Tabelas referidas: ge_contato, GE_Contato_ITA, ge_contatopapel, GE_CONTATOPAPEL_ITA, ge_pessoa, GE_PESSOA_ITA, GE_PESSOALINK, MIG_GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MIGCONTCLASSE] 
AS


/*
-- autor: AMAURY 15/11/2024
   objetivo: Migrar PAPEL DE contatos 
   log de alterações: 15/11/2024 - Criação da rotina, testando e finalizando criação da rotina
*/	

	DECLARE 
	@SEQPESSOA               decimal(10, 0) ,
	@SEQCONTATO              decimal(4, 0) ,
	@maxSEQCONTATO              decimal(4, 0) ,
	@PAPEL		             varchar(20) ,
	@DTAALTERACAO            datetime ,
	@USUALTEROU              varchar(20),
	@ORIGEM               	varchar(20),
	@CONTATO					VARCHAR(40)

	Declare curPES
	CURSOR
	FOR
	SELECT		LK.SEQPESSOA
		      ,CI.SeqContato
			  ,ISNULL((select max(cc.seqcontato) from ge_pessoa pp, ge_contato cc 
				                  where pp.seqpessoa = lk.seqpessoa
								    and cc.seqpessoa = pp.SeqPessoa
									and SUBSTRING (cc.contato, 1, 3 )<>'NO-'), 0) as MAXSeqcontato 
		      ,PPITA.PAPEL
		      ,PPITA.DtaAlteracao
		      ,PPITA.UsuAlterou
		  FROM MIG_GE_PESSOA MP
		  JOIN GE_Contato_ITA CI ON CI.SEQPESSOA = MP.SEQPESSOA
		  JOIN GE_CONTATOPAPEL_ITA PPITA ON PPITA.SeqPessoa = CI.SeqPessoa AND PPITA.SeqContato = CI.SeqContato
		  JOIN GE_PESSOALINK LK ON LK.Pessoalink = CAST( MP.SEQPESSOA AS VARCHAR) 
		                     AND LK.Origem = 'INTEGRAÇÃONOROESTE'
		  WHERE 1=1
		    AND MP.DESCRICAO in ( 'Ativo com CNPJ Inexistente', 'Prospect com CNPJ Inexistente', 'Prospect com CNPJ<>0 Inexistente', 'Prospect sem CNPJ' )
		    AND MP.SEQPESSOA NOT in ( 6976 )
		 order by lk.seqpessoa, ci.seqcontato

	Declare curPES2
	CURSOR
	FOR
			SELECT pes.SEQPESSOA
		      ,CI.SeqContato
			  ,ISNULL((select max(cc.seqcontato) from ge_pessoa pp, ge_contato cc 
				                  where pp.seqpessoa = PES.seqpessoa
								    and cc.seqpessoa = pp.SeqPessoa
									and SUBSTRING (cc.contato, 1, 3 )<>'NO-'), 0) as MAXSeqcontato  
		      ,PPITA.PAPEL
		      ,PPITA.DtaAlteracao
		      ,PPITA.UsuAlterou
		  FROM MIG_GE_PESSOA MP
		  JOIN GE_Contato_ITA CI ON CI.SEQPESSOA = MP.SEQPESSOA
		  JOIN GE_CONTATOPAPEL_ITA PPITA ON PPITA.SeqPessoa = CI.SeqPessoa AND PPITA.SeqContato = CI.SeqContato
		  JOIN GE_PESSOA_ITA PITA ON PITA.SEQPESSOA = MP.SEQPESSOA
		  JOIN GE_PESSOA PES ON PES.NROCGCCPF = PITA.NROCGCCPF AND PES.DIGCGCCPF = PITA.DIGCGCCPF
		  WHERE 1=1
		    AND MP.DESCRICAO in ( 'Ativo/Prospect CNPJ Existente' )
		    AND MP.SEQPESSOA NOT in ( 9999999 )
		 order by pes.seqpessoa, ci.seqcontato

begin

	OPEN curPES
		FETCH NEXT FROM curPES INTO 					@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@PAPEL,
														@DTAALTERACAO,
														@USUALTEROU


	WHILE (@@FETCH_STATUS = 0)
	BEGIN
			SET @seqcontato = @seqcontato + @maxseqcontato
			insert into ge_contatopapel ( seqpessoa, 
				                           seqcontato,
				                           papel,
				                           DTAALTERACAO,
				                           usualterou
				                           )
			values ( @SEQPESSOA,
				      @SEQCONTATO,
				      @PAPEL,
				      @DTAALTERACAO,
				      @UsuAlterou
				)

		FETCH NEXT FROM curPES INTO 					@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@PAPEL,
														@DTAALTERACAO,
														@USUALTEROU
						
	END

	CLOSE curPES
	DEALLOCATE curPES

	OPEN curPES2
		FETCH NEXT FROM curPES2 INTO 					@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@PAPEL,
														@DTAALTERACAO,
														@USUALTEROU


	WHILE (@@FETCH_STATUS = 0)
	BEGIN
			SET @seqcontato = @seqcontato + @maxseqcontato

			insert into ge_contatopapel ( seqpessoa, 
				                           seqcontato,
				                           papel,
				                           DTAALTERACAO,
				                           usualterou
				                           )
			values ( @SEQPESSOA,
				      @SEQCONTATO,
				      @PAPEL,
				      @DTAALTERACAO,
				      @UsuAlterou
				)


		FETCH NEXT FROM curPES2 INTO 					@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@PAPEL,
														@DTAALTERACAO,
														@USUALTEROU
						
	END

	CLOSE curPES2
	DEALLOCATE curPES2

End
