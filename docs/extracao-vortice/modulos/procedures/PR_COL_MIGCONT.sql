/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGCONT
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 16:24:23
   Modificado em ...: 2024-12-28 16:55:33
   Linhas ..........: 559
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : GE_Contato
   Tabelas referidas: GE_Contato, GE_Contato_ITA, ge_pessoa, ge_pessoa_ita, GE_PESSOALINK, MIG_GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MIGCONT] 
AS


/*
-- autor: AMAURY 01/11/2024
   objetivo: Migrar contatos de pessoas ativas com cnpj da norte para noroeste
   log de alterações: 01/11/2024 - Criação da rotina, testando e finalizando criação da rotina
                      05/11/2024 - Teste de alguns exemplos integrando contatos especificos, mudando para nova origem IntegraçãoNoroeste
*/	

	DECLARE 
	@SEQPESSOA               decimal(10, 0) ,
	@SEQCONTATO              decimal(4, 0) ,
	@MAXSEQCONTATO              decimal(4, 0) ,
	@EMUSO                   decimal(1, 0) ,
	@TIPOCONTATO             varchar(30) ,
	@AREAATUACAO             varchar(20) ,
	@RG                      char(20) ,
	@CPF                     decimal(18, 0) ,
	@DIGCPF                  decimal(2, 0) ,
	@SAUDACAO                varchar(20) ,
	@CONTATO                 varchar(40) ,
	@FONEDDD1                varchar(5) ,
	@FONENRO1                decimal(12, 0) ,
	@FONECMPL1               varchar(12) ,
	@FONEDDD2                varchar(5) ,
	@FONENRO2                decimal(12, 0) ,
	@FONECMPL2               varchar(12) ,
	@FAXDDD                  varchar(5) ,
	@FAXNRO                  decimal(12, 0) ,
	@SEXO                    char(1) ,
	@ESTADOCIVIL             char(1) ,
	@DTANASCIMENTO           datetime ,
	@NIVELDECISAO            char(1) ,
	@POSICIONAMENTO          char(1) ,
	@OBSPESSOAL              varchar(250) ,
	@ATRIBUTO1               varchar(20) ,
	@ATRIBUTO2               varchar(20) ,
	@ATRIBUTO3               varchar(20) ,
	@ATRIBDTA                datetime ,
	@ATRIBNUM                decimal(18, 0) ,
	@EMAIL                   varchar(50) ,
	@LINKWEB                 decimal(1, 0) ,
	@ULTORIGEM               varchar(20) ,
	@USUALTERACAO            varchar(20) ,
	@DTAALTERACAO            datetime ,
	@OBSERVACAO              varchar(250) ,
	@SKYPE                   varchar(70) ,
	@INDWHATSAPPF1           decimal(1, 0) ,
	@INDWHATSAPPF2           decimal(1, 0) ,
	@INDWHATSAPPFX           decimal(1, 0) ,
	@USUINCLUSAO             varchar(20) ,
	@DTAINCLUSAO             datetime,
	@ORIGEM               varchar(20)

-------- primeiro

	Declare curPES
	CURSOR
	FOR
		SELECT LK.SEQPESSOA
		      ,CI.SeqContato
			  ,isnull((select max(cc.seqcontato) from ge_pessoa pp, ge_contato cc 
				                  where pp.seqpessoa = lk.seqpessoa
								    and cc.seqpessoa = pp.SeqPessoa), 0) as MAXSeqcontato 
		      ,CI.EmUso
		      ,CI.TipoContato
		      ,CI.AreaAtuacao
		      ,CI.Rg
		      ,CI.Cpf
		      ,CI.DigCPF
		      ,'NO-' + CI.Saudacao as Saudacao
		      ,'NO-' + CI.Contato as Contato
		      ,CI.FoneDDD1
		      ,CI.FoneNro1
		      ,CI.FoneCmpl1
		      ,CI.FoneDDD2
		      ,CI.FoneNro2
		      ,CI.FoneCmpl2
		      ,CI.FaxDDD
		      ,CI.FaxNro
		      ,CASE WHEN ISNULL(CI.Sexo, '@')='@' THEN '-' ELSE CI.SEXO END AS Sexo
		      ,CI.EstadoCivil
		      ,CI.DtaNascimento
		      ,CI.NivelDecisao
		      ,CI.Posicionamento
		      ,CI.ObsPessoal
		      ,CI.Atributo1
		      ,CI.Atributo2
		      ,CI.Atributo3
		      ,CI.AtribDta
		      ,CI.AtribNum
		      ,CI.EMail
		      ,CI.LinkWeb
		      ,CI.UltOrigem
		      ,CI.UsuAlteracao
		      ,CI.DtaAlteracao
		      ,CI.Observacao
		      ,CI.Skype
		      ,CI.INDWHATSAPPF1
		      ,CI.INDWHATSAPPF2
		      ,CI.INDWHATSAPPFX
		      ,CI.USUINCLUSAO
		      ,CI.DTAINCLUSAO
		      ,'INTEGRAÇÃONOROESTE' as ORIGEM
		  FROM MIG_GE_PESSOA MP
		  JOIN GE_Contato_ITA CI ON CI.SEQPESSOA = MP.SEQPESSOA
		  JOIN GE_PESSOALINK LK ON LK.Pessoalink = CAST( MP.SEQPESSOA AS VARCHAR) 
		                     AND LK.Origem = 'INTEGRAÇÃONOROESTE'
		  WHERE 1=1
		    AND MP.DESCRICAO in ( 'Ativo com CNPJ Inexistente', 'Prospect com CNPJ Inexistente', 'Prospect com CNPJ<>0 Inexistente', 'Prospect sem CNPJ' )
		    AND MP.SEQPESSOA in ( 33569 )   ---8511 )
/*


*/

		 order by lk.seqpessoa, ci.seqcontato

-------- segundo

	Declare curPES2
	CURSOR
	FOR
		SELECT pes.SEQPESSOA
		      ,CI.SeqContato
			  ,isnull((select max(cc.seqcontato) from ge_pessoa pp, ge_contato cc 
				                  where pp.seqpessoa = pes.seqpessoa
								    and cc.seqpessoa = pp.SeqPessoa), 0) as MAXSeqcontato 
		      ,CI.EmUso
		      ,CI.TipoContato
		      ,CI.AreaAtuacao
		      ,CI.Rg
		      ,CI.Cpf
		      ,CI.DigCPF
		      ,'NO-' + CI.Saudacao as Saudacao
		      ,'NO-' + CI.Contato as Contato
		      ,CI.FoneDDD1
		      ,CI.FoneNro1
		      ,CI.FoneCmpl1
		      ,CI.FoneDDD2
		      ,CI.FoneNro2
		      ,CI.FoneCmpl2
		      ,CI.FaxDDD
		      ,CI.FaxNro
		      ,CASE WHEN ISNULL(CI.Sexo, '@')='@' THEN '-' ELSE CI.SEXO END AS Sexo
		      ,CI.EstadoCivil
		      ,CI.DtaNascimento
		      ,CI.NivelDecisao
		      ,CI.Posicionamento
		      ,CI.ObsPessoal
		      ,CI.Atributo1
		      ,CI.Atributo2
		      ,CI.Atributo3
		      ,CI.AtribDta
		      ,CI.AtribNum
		      ,CI.EMail
		      ,CI.LinkWeb
		      ,CI.UltOrigem
		      ,CI.UsuAlteracao
		      ,CI.DtaAlteracao
		      ,CI.Observacao
		      ,CI.Skype
		      ,CI.INDWHATSAPPF1
		      ,CI.INDWHATSAPPF2
		      ,CI.INDWHATSAPPFX
		      ,CI.USUINCLUSAO
		      ,CI.DTAINCLUSAO
		      ,'INTEGRAÇÃONOROESTE' as ORIGEM
		  FROM MIG_GE_PESSOA MP
		  JOIN GE_Contato_ITA CI ON CI.SEQPESSOA = MP.SEQPESSOA
		  join ge_pessoa_ita pita on pita.seqpessoa = mp.seqpessoa 
		  JOIN ge_pessoa pes ON pes.nrocgccpf = pita.NroCGCCPF and pes.digcgccpf = pita.digcgccpf
		  WHERE 1=1
		    AND MP.DESCRICAO in ( 'Ativo/Prospect CNPJ Existente' )
		    AND MP.SEQPESSOA in ( 9999999 )
		 order by pes.seqpessoa, ci.seqcontato

begin

	OPEN curPES
		FETCH NEXT FROM curPES INTO 			@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@EMUSO,
														@TIPOCONTATO,
														@AREAATUACAO,
														@RG,
														@CPF,
														@DIGCPF,
														@SAUDACAO,
														@CONTATO,
														@FONEDDD1,
														@FONENRO1,
														@FONECMPL1,
														@FONEDDD2,
														@FONENRO2,
														@FONECMPL2,
														@FAXDDD,
														@FAXNRO,
														@SEXO,
														@ESTADOCIVIL,
														@DTANASCIMENTO,
														@NIVELDECISAO,
														@POSICIONAMENTO,
														@OBSPESSOAL,
														@ATRIBUTO1,
														@ATRIBUTO2,
														@ATRIBUTO3,
														@ATRIBDTA,
														@ATRIBNUM,
														@EMAIL,
														@LINKWEB,
														@ULTORIGEM,
														@USUALTERACAO,
														@DTAALTERACAO,
														@OBSERVACAO,
														@SKYPE,
														@INDWHATSAPPF1,
														@INDWHATSAPPF2,
														@INDWHATSAPPFX,
														@USUINCLUSAO,
														@DTAINCLUSAO,
														@ORIGEM


	WHILE (@@FETCH_STATUS = 0)
	BEGIN

				set @SEQCONTATO = @MAXSEQCONTATO + @SEQCONTATO

					INSERT INTO dbo.GE_Contato
							   (SeqPessoa
							   ,SeqContato
							   ,EmUso
							   ,TipoContato
							   ,AreaAtuacao
							   ,Rg
							   ,Cpf
							   ,DigCPF
							   ,Saudacao
							   ,Contato
							   ,FoneDDD1
							   ,FoneNro1
							   ,FoneCmpl1
							   ,FoneDDD2
							   ,FoneNro2
							   ,FoneCmpl2
							   ,FaxDDD
							   ,FaxNro
							   ,Sexo
							   ,EstadoCivil
							   ,DtaNascimento
							   ,NivelDecisao
							   ,Posicionamento
							   ,ObsPessoal
							   ,Atributo1
							   ,Atributo2
							   ,Atributo3
							   ,AtribDta
							   ,AtribNum
							   ,EMail
							   ,LinkWeb
							   ,UltOrigem
							   ,UsuAlteracao
							   ,DtaAlteracao
							   ,Observacao
							   ,Skype
							   ,INDWHATSAPPF1
							   ,INDWHATSAPPF2
							   ,INDWHATSAPPFX
							   ,USUINCLUSAO
							   ,DTAINCLUSAO)
						 VALUES
							   (	   @SEQPESSOA
						  ,@SEQCONTATO
						  ,@EMUSO
						  ,@TIPOCONTATO
						  ,@AREAATUACAO
						  ,@RG
						  ,@CPF
						  ,@DIGCPF
						  ,@SAUDACAO
						  ,@CONTATO
						  ,@FONEDDD1
						  ,@FONENRO1
						  ,@FONECMPL1
						  ,@FONEDDD2
						  ,@FONENRO2
						  ,@FONECMPL2
						  ,@FAXDDD
						  ,@FAXNRO
						  ,@SEXO
						  ,@ESTADOCIVIL
						  ,@DTANASCIMENTO
						  ,@NIVELDECISAO
						  ,@POSICIONAMENTO
						  ,@OBSPESSOAL
						  ,@ATRIBUTO1
						  ,@ATRIBUTO2
						  ,@ATRIBUTO3
						  ,@ATRIBDTA
						  ,@ATRIBNUM
						  ,@EMAIL
						  ,@LINKWEB
						  ,@ULTORIGEM
						  ,@USUALTERACAO
						  ,@DTAALTERACAO
						  ,@OBSERVACAO
						  ,@SKYPE
						  ,@INDWHATSAPPF1
						  ,@INDWHATSAPPF2
						  ,@INDWHATSAPPFX
						  ,@USUINCLUSAO
						  ,@DTAINCLUSAO
						)
	


		FETCH NEXT FROM curPES INTO 			@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@EMUSO,
														@TIPOCONTATO,
														@AREAATUACAO,
														@RG,
														@CPF,
														@DIGCPF,
														@SAUDACAO,
														@CONTATO,
														@FONEDDD1,
														@FONENRO1,
														@FONECMPL1,
														@FONEDDD2,
														@FONENRO2,
														@FONECMPL2,
														@FAXDDD,
														@FAXNRO,
														@SEXO,
														@ESTADOCIVIL,
														@DTANASCIMENTO,
														@NIVELDECISAO,
														@POSICIONAMENTO,
														@OBSPESSOAL,
														@ATRIBUTO1,
														@ATRIBUTO2,
														@ATRIBUTO3,
														@ATRIBDTA,
														@ATRIBNUM,
														@EMAIL,
														@LINKWEB,
														@ULTORIGEM,
														@USUALTERACAO,
														@DTAALTERACAO,
														@OBSERVACAO,
														@SKYPE,
														@INDWHATSAPPF1,
														@INDWHATSAPPF2,
														@INDWHATSAPPFX,
														@USUINCLUSAO,
														@DTAINCLUSAO,
														@ORIGEM
						
	END

	CLOSE curPES
	DEALLOCATE curPES

------------- segundo cursor
	OPEN curPES2
		FETCH NEXT FROM curPES2 INTO 			@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@EMUSO,
														@TIPOCONTATO,
														@AREAATUACAO,
														@RG,
														@CPF,
														@DIGCPF,
														@SAUDACAO,
														@CONTATO,
														@FONEDDD1,
														@FONENRO1,
														@FONECMPL1,
														@FONEDDD2,
														@FONENRO2,
														@FONECMPL2,
														@FAXDDD,
														@FAXNRO,
														@SEXO,
														@ESTADOCIVIL,
														@DTANASCIMENTO,
														@NIVELDECISAO,
														@POSICIONAMENTO,
														@OBSPESSOAL,
														@ATRIBUTO1,
														@ATRIBUTO2,
														@ATRIBUTO3,
														@ATRIBDTA,
														@ATRIBNUM,
														@EMAIL,
														@LINKWEB,
														@ULTORIGEM,
														@USUALTERACAO,
														@DTAALTERACAO,
														@OBSERVACAO,
														@SKYPE,
														@INDWHATSAPPF1,
														@INDWHATSAPPF2,
														@INDWHATSAPPFX,
														@USUINCLUSAO,
														@DTAINCLUSAO,
														@ORIGEM


	WHILE (@@FETCH_STATUS = 0)
	BEGIN

				set @SEQCONTATO = @MAXSEQCONTATO + @SEQCONTATO

				INSERT INTO dbo.GE_Contato
							   (SeqPessoa
							   ,SeqContato
							   ,EmUso
							   ,TipoContato
							   ,AreaAtuacao
							   ,Rg
							   ,Cpf
							   ,DigCPF
							   ,Saudacao
							   ,Contato
							   ,FoneDDD1
							   ,FoneNro1
							   ,FoneCmpl1
							   ,FoneDDD2
							   ,FoneNro2
							   ,FoneCmpl2
							   ,FaxDDD
							   ,FaxNro
							   ,Sexo
							   ,EstadoCivil
							   ,DtaNascimento
							   ,NivelDecisao
							   ,Posicionamento
							   ,ObsPessoal
							   ,Atributo1
							   ,Atributo2
							   ,Atributo3
							   ,AtribDta
							   ,AtribNum
							   ,EMail
							   ,LinkWeb
							   ,UltOrigem
							   ,UsuAlteracao
							   ,DtaAlteracao
							   ,Observacao
							   ,Skype
							   ,INDWHATSAPPF1
							   ,INDWHATSAPPF2
							   ,INDWHATSAPPFX
							   ,USUINCLUSAO
							   ,DTAINCLUSAO)
						 VALUES
							   (	   @SEQPESSOA
						  ,@SEQCONTATO
						  ,@EMUSO
						  ,@TIPOCONTATO
						  ,@AREAATUACAO
						  ,@RG
						  ,@CPF
						  ,@DIGCPF
						  ,@SAUDACAO
						  ,@CONTATO
						  ,@FONEDDD1
						  ,@FONENRO1
						  ,@FONECMPL1
						  ,@FONEDDD2
						  ,@FONENRO2
						  ,@FONECMPL2
						  ,@FAXDDD
						  ,@FAXNRO
						  ,@SEXO
						  ,@ESTADOCIVIL
						  ,@DTANASCIMENTO
						  ,@NIVELDECISAO
						  ,@POSICIONAMENTO
						  ,@OBSPESSOAL
						  ,@ATRIBUTO1
						  ,@ATRIBUTO2
						  ,@ATRIBUTO3
						  ,@ATRIBDTA
						  ,@ATRIBNUM
						  ,@EMAIL
						  ,@LINKWEB
						  ,@ULTORIGEM
						  ,@USUALTERACAO
						  ,@DTAALTERACAO
						  ,@OBSERVACAO
						  ,@SKYPE
						  ,@INDWHATSAPPF1
						  ,@INDWHATSAPPF2
						  ,@INDWHATSAPPFX
						  ,@USUINCLUSAO
						  ,@DTAINCLUSAO
						)


		FETCH NEXT FROM curPES2 INTO 			@SEQPESSOA,
														@SEQCONTATO,
														@MAXSEQCONTATO,
														@EMUSO,
														@TIPOCONTATO,
														@AREAATUACAO,
														@RG,
														@CPF,
														@DIGCPF,
														@SAUDACAO,
														@CONTATO,
														@FONEDDD1,
														@FONENRO1,
														@FONECMPL1,
														@FONEDDD2,
														@FONENRO2,
														@FONECMPL2,
														@FAXDDD,
														@FAXNRO,
														@SEXO,
														@ESTADOCIVIL,
														@DTANASCIMENTO,
														@NIVELDECISAO,
														@POSICIONAMENTO,
														@OBSPESSOAL,
														@ATRIBUTO1,
														@ATRIBUTO2,
														@ATRIBUTO3,
														@ATRIBDTA,
														@ATRIBNUM,
														@EMAIL,
														@LINKWEB,
														@ULTORIGEM,
														@USUALTERACAO,
														@DTAALTERACAO,
														@OBSERVACAO,
														@SKYPE,
														@INDWHATSAPPF1,
														@INDWHATSAPPF2,
														@INDWHATSAPPFX,
														@USUINCLUSAO,
														@DTAINCLUSAO,
														@ORIGEM
						
	END

	CLOSE curPES2
	DEALLOCATE curPES2

End
