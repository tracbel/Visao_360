/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGPESSOA_EXIST
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-27 20:23:23
   Modificado em ...: 2024-12-27 20:23:23
   Linhas ..........: 514
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOA_ITA, MIG_GE_PESSOA
   Outras refs .....: PR_COL_MOVPESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MIGPESSOA_EXIST] 
AS


/*
-- autor: AMAURY 05/11/2024
   objetivo: Migrar pessoas ativas com cnpj existentes
   log de alterações: 05/11/2024 - criacao da rotina
*/	

	DECLARE 
	@SEQPESSOA		 numeric(10, 0) ,
	@DTAINCLUSAO		 datetime ,
	@NOMERAZAO		Varchar(100) ,
	@FANTASIA		Varchar(50) ,
	@CIDADE			 varchar(50) ,
	@UF			 varchar(2) ,
	@PAIS			 varchar(25) ,
	@BAIRRO			 varchar(50) ,
	@TIPOLOGRADOURO		 varchar(15) ,
	@LOGRADOURO		 varchar(80) ,
	@NROLOGRADOURO		 varchar(10) ,
	@CMPLTOLOGRADOURO	 varchar(30) ,
	@CEP			 varchar(12) ,
	@FONEDDD1		 varchar(5) ,
	@FONENRO1		 decimal(12, 0) ,
	@FONECMPL1		 varchar(20) ,
	@FONEDDD2		 varchar(5) ,
	@FONENRO2		 decimal(12, 0) ,
	@FONECMPL2		 varchar(20) ,
	@FONEDDD3		 varchar(5) ,
	@FONENRO3		 decimal(12, 0) ,
	@FONECMPL3		 varchar(20) ,
	@FAXDDD			 varchar(5) ,
	@FAXNRO			 decimal(12, 0) ,
	@NROCGCCPF		 decimal(13, 0) ,
	@DIGCGCCPF		 decimal(2, 0) ,
	@INSCRICAORG		 varchar(20) ,
	@UFEMISSOR		 varchar(2) ,
	@ORGAOEMISSOR		 varchar(10) ,
	@INSCMUNIC		 varchar(15) ,
	@INSCPRODUTOR		 varchar(20) ,
	@DTANASCFUND		 datetime ,
	@EMAIL			 varchar(70) ,
	@HOMEPAGE		 varchar(80) ,
	@ESTADOCIVIL		 varchar(20) ,
	@ATIVIDADE		 varchar(30) ,
	@RENDAFATURAMENTO	 varchar(30) ,
	@GRAUINSTRUCAO		 varchar(30) ,
	@GRUPO			 varchar(30) ,
	@PORTE			 varchar(30) ,
	@STATUS			 CHAR(1),	 

	@DTAINCLUSAO2		 datetime ,
	@NOMERAZAO2		 Varchar(100) ,
	@FANTASIA2		 Varchar(50) ,
	@CIDADE2	         varchar(50) ,
	@UF2			 varchar(2) ,
	@PAIS2			 varchar(25) ,
	@BAIRRO2		 varchar(50) ,
	@TIPOLOGRADOURO2	 varchar(15) ,
	@LOGRADOURO2		 varchar(80) ,
	@NROLOGRADOURO2		 varchar(10) ,
	@CMPLTOLOGRADOURO2	 varchar(30) ,
	@CEP2			 varchar(12) ,
	@FONEDDD12		 varchar(5) ,
	@FONENRO12		 decimal(12, 0) ,
	@FONECMPL12		 varchar(20) ,
	@FONEDDD22		 varchar(5) ,
	@FONENRO22		 decimal(12, 0) ,
	@FONECMPL22		 varchar(20) ,
	@FONEDDD32		 varchar(5) ,
	@FONENRO32		 decimal(12, 0) ,
	@FONECMPL32		 varchar(20) ,
	@FAXDDD2		 varchar(5) ,
	@FAXNRO2		 decimal(12, 0) ,
	@NROCGCCPF2		 decimal(13, 0) ,
	@DIGCGCCPF2		 decimal(2, 0) ,
	@INSCRICAORG2		 varchar(20) ,
	@UFEMISSOR2		 varchar(2) ,
	@ORGAOEMISSOR2		 varchar(10) ,
	@INSCMUNIC2		 varchar(15) ,
	@INSCPRODUTOR2		 varchar(20) ,
	@DTANASCFUND2		 datetime ,
	@EMAIL2			 varchar(70) ,
	@HOMEPAGE2		 varchar(80) ,
	@ESTADOCIVIL2		 varchar(20) ,
	@ATIVIDADE2		 varchar(30) ,
	@RENDAFATURAMENTO2	 varchar(30) ,
	@GRAUINSTRUCAO2		 varchar(30) ,
	@GRUPO2			 varchar(30) ,
	@PORTE2			 varchar(30) ,
	@STATUS2		 CHAR(1)


	Declare curPES
	CURSOR
	FOR
		SELECT 
			   PES.SEQPESSOA
			  ,PES.DtaInclusao
			  ,PI.DtaInclusao AS DTAINCLUSAO2
			  ,PES.NOMERAZAO
			  ,PI.NOMERAZAO AS NOMERAZAO2
			  ,PES.FANTASIA
			  ,PI.FANTASIA AS FANTASIA2
			  ,PES.CIDADE
			  ,PI.CIDADE AS CIDADE2
			  ,PES.UF
			  ,PI.UF AS UF2
			  ,PES.PAIS
			  ,PI.PAIS AS PAIS2
			  ,PES.BAIRRO
			  ,PI.BAIRRO AS BAIRRO2
			  ,PES.TIPOLOGRADOURO
			  ,PI.TipoLogradouro AS TIPOLOGRADOURO2
			  ,PES.LOGRADOURO
			  ,PI.Logradouro AS LOGRADOURO2
			  ,PES.NROLOGRADOURO
			  ,PI.NroLogradouro AS NROLOGRADOURO2
			  ,PES.CMPLTOLOGRADOURO
			  ,PI.CmpltoLogradouro AS CMPLTOLOGRADOURO2
			  ,PES.CEP
			  ,PI.CEP AS CEP2
			  ,PES.FONEDDD1
			  ,PI.FONEDDD1 AS FONEDDD12
			  ,PES.FONENRO1
			  ,PI.FONENRO1 AS FONENRO12
			  ,PES.FONECMPL1
			  ,PI.FONECMPL1 AS FONECMPL12
			  ,PES.FONEDDD2
			  ,PI.FONEDDD2 AS FONEDDD22
			  ,PES.FONENRO2
			  ,PI.FONENRO2 AS FONENRO22
			  ,PES.FONECMPL2
			  ,PI.FONECMPL2 AS FONECMPL22
			  ,PES.FONEDDD3
			  ,PI.FONEDDD3 AS FONEDDD32
			  ,PES.FONENRO3
			  ,PI.FONENRO3 AS FONENRO32
			  ,PES.FONECMPL3
			  ,PI.FONECMPL3 AS FONECMPL32
			  ,PES.FAXDDD
			  ,PI.FAXDDD AS FAXDDD2
			  ,PES.FAXNRO
			  ,PI.FAXNRO AS FAXNRO2
			  ,PES.NROCGCCPF
			  ,PI.NROCGCCPF AS NROCGCCPF2
			  ,PES.DIGCGCCPF
			  ,PI.DIGCGCCPF AS DIGCGCCPF2
			  ,PES.INSCRICAORG
			  ,PI.INSCRICAORG AS INSCRICAORG2
			  ,PES.UFEMISSOR
			  ,PI.UFEmissor AS UFEMISSOR2
			  ,PES.ORGAOEMISSOR
			  ,PI.OrgaoEmissor AS ORGAOEMISSOR2
			  ,PES.INSCMUNIC
			  ,PI.InscMunic AS INSCMUNIC2
			  ,PES.INSCPRODUTOR
			  ,PI.InscProdutor AS INSCPRODUTOR2
			  ,PES.DTANASCFUND
			  ,PI.DtaNascFund AS DTANASCFUND2
			  ,PES.EMAIL
			  ,PI.Email AS EMAIL2
			  ,PES.HOMEPAGE
			  ,PI.HomePage AS HOMEPAGE2
			  ,PES.ESTADOCIVIL
			  ,PI.EstadoCivil AS ESTADOCIVIL2
			  ,PES.ATIVIDADE
			  ,PI.ATIVIDADE AS ATIVIDADE2
			  ,PES.RENDAFATURAMENTO
			  ,PI.RendaFaturamento AS RENDAFATURAMENTO2
			  ,PES.GRAUINSTRUCAO
			  ,PI.GrauInstrucao AS GRAUINSTRUCAO2
			  ,PES.GRUPO
			  ,PI.GRUPO AS GRUPO2
			  ,PES.PORTE
			  ,PI.PORTE AS PORTE2
			  ,PES.Status
			  ,PI.Status AS STATUS2
		FROM MIG_GE_PESSOA MP
		JOIN GE_PESSOA_ITA PI ON PI.SEQPESSOA = MP.SEQPESSOA
		JOIN GE_PESSOA PES ON PES.NroCGCCPF = PI.NroCGCCPF
		WHERE 1=1
		  AND MP.DESCRICAO = 'Ativo/Prospect CNPJ Existente'
		  AND (
				PI.NomeRazao <> PES.NomeRazao OR
				PI.Fantasia <> PES.Fantasia OR
				( ISNULL(PES.CIDADE, '@')<>'@' AND PI.CIDADE <> PES.CIDADE ) OR
				( ISNULL(PES.UF, '@')<>'@' AND PI.UF <> PES.UF ) OR
				( ISNULL(PES.BAIRRO, '@')<>'@' AND PI.Bairro <> PES.Bairro ) OR
				( ISNULL(PES.TipoLogradouro, '@')<>'@' AND PI.TipoLogradouro <> PES.TipoLogradouro) OR
				( ISNULL(PES.Logradouro, '@')<>'@' AND PI.Logradouro <> PES.Logradouro ) OR
				( ISNULL(PES.NROLOGRADOURO, '@')<>'@' AND PI.NroLogradouro <> PES.NroLogradouro ) OR
				( ISNULL(PES.CMPLTOLOGRADOURO, '@')<>'@' AND PI.CmpltoLogradouro <> PES.CmpltoLogradouro ) OR
				( ISNULL(PES.CEP, '@')<>'@' AND PI.Cep <> PES.Cep ) OR 
				( ISNULL(PES.FONEDDD1, '@')<>'@' AND PI.FoneDDD1 <> PES.FoneDDD1 ) OR
				( ISNULL(PES.FONENRO1, 0)<>0 AND PI.FoneNro1 <> PES.FoneNro1 ) OR
				( ISNULL(PES.FONECMPL1, '@')<>'@' AND PI.FoneCmpl1 <> PES.FoneCmpl1 ) OR
				( ISNULL(PES.FONEDDD2, '@')<>'@' AND PI.FoneDDD2 <> PES.FoneDDD2 ) OR
				( ISNULL(PES.FONENRO2, 0)<>0 AND PI.FoneNro2 <> PES.FoneNro2 ) OR
				( ISNULL(PES.FONECMPL2, '@')<>'@' AND PI.FoneCmpl2 <> PES.FoneCmpl2 ) OR
				( ISNULL(PES.FONEDDD3, '@')<>'@' AND PI.FoneDDD3 <> PES.FoneDDD3 ) OR
				( ISNULL(PES.FONENRO3, 0)<>0 AND PI.FoneNro3 <> PES.FoneNro3 ) OR
				( ISNULL(PES.FONECMPL3, '@')<>'@' AND PI.FoneCmpl3 <> PES.FoneCmpl3 ) OR
				( ISNULL(PES.FAXDDD, '@')<>'@' AND PI.FaxDDD <> PES.FaxDDD ) OR
				( ISNULL(PES.FAXNRO, 0)<>0 AND PI.FaxNro <> PES.FaxNro ) OR
				( ISNULL(PES.NROCGCCPF, 0)<>0 AND PI.NroCGCCPF <> PES.NroCGCCPF ) OR
				( ISNULL(PES.DIGCGCCPF, 0)<>0 AND PI.DigCGCCPF <> PES.DigCGCCPF ) OR
				( ISNULL(PES.INSCRICAORG, '@')<>'@' AND PI.InscricaoRG <> PES.InscricaoRG ) OR
				( ISNULL(PES.UFEMISSOR, '@')<>'@' AND PI.UFEmissor <> PES.UFEmissor ) OR
				( ISNULL(PES.ORGAOEMISSOR, '@')<>'@' AND PI.OrgaoEmissor <> PES.OrgaoEmissor ) OR
				( ISNULL(PES.INSCMUNIC, '@')<>'@' AND PI.InscMunic <> PES.InscMunic ) OR
				( ISNULL(PES.INSCPRODUTOR, '@')<>'@' AND PI.InscProdutor <> PES.InscProdutor ) OR
				( PES.DTANASCFUND IS NOT NULL AND PI.DtaNascFund <> PES.DtaNascFund ) OR
				( ISNULL(PES.HOMEPAGE, '@')<>'@' AND PI.HOMEPAGE <> PES.HOMEPAGE ) OR
				( ISNULL(PES.EMAIL, '@')<>'@' AND PI.EMAIL <> PES.EMAIL ) OR
				( ISNULL(PES.ESTADOCIVIL, '@')<>'@' AND PI.EstadoCivil <> PES.EstadoCivil ) OR
				( ISNULL(PES.ATIVIDADE, '@')<>'@' AND PI.Atividade <> PES.Atividade ) OR
				( ISNULL(PES.RENDAFATURAMENTO, '@')<>'@' AND PI.RendaFaturamento <> PES.RendaFaturamento ) OR
				( ISNULL(PES.GRAUINSTRUCAO, '@')<>'@' AND PI.GrauInstrucao <> PES.GrauInstrucao ) OR
				( ISNULL(PES.GRUPO, '@')<>'@' AND PI.GRUPO <> PES.Grupo ) OR
				( ISNULL(PES.PORTE, '@')<>'@' AND PI.Porte <> PES.Porte )
		  )
---	NAO USSAR	  AND ( PI.DtaInclusao >= PES.DtaInclusao or PI.DtaAlteracao >= PES.DtaAlteracao )
----		  and pi.SeqPessoa IN (27472)
begin

	OPEN curPES
		FETCH NEXT FROM curPES INTO 		
													 @SEQPESSOA
													,@DTAINCLUSAO
													,@DTAINCLUSAO2
													,@NOMERAZAO
													,@NOMERAZAO2
													,@FANTASIA
													,@FANTASIA2
													,@CIDADE
													,@CIDADE2
													,@UF
													,@UF2
													,@PAIS
													,@PAIS2
													,@BAIRRO
													,@BAIRRO2
													,@TIPOLOGRADOURO
													,@TIPOLOGRADOURO2
													,@LOGRADOURO
													,@LOGRADOURO2
													,@NROLOGRADOURO
													,@NROLOGRADOURO2
													,@CMPLTOLOGRADOURO
													,@CMPLTOLOGRADOURO2
													,@CEP
													,@CEP2
													,@FONEDDD1
													,@FONEDDD12
													,@FONENRO1
													,@FONENRO12
													,@FONECMPL1
													,@FONECMPL12
													,@FONEDDD2
													,@FONEDDD22
													,@FONENRO2
													,@FONENRO22
													,@FONECMPL2
													,@FONECMPL22
													,@FONEDDD3
													,@FONEDDD32
													,@FONENRO3
													,@FONENRO32
													,@FONECMPL3
													,@FONECMPL32
													,@FAXDDD
													,@FAXDDD2
													,@FAXNRO
													,@FAXNRO2
													,@NROCGCCPF
													,@NROCGCCPF2
													,@DIGCGCCPF
													,@DIGCGCCPF2
													,@INSCRICAORG
													,@INSCRICAORG2
													,@UFEMISSOR
													,@UFEMISSOR2
													,@ORGAOEMISSOR
													,@ORGAOEMISSOR2
													,@INSCMUNIC
													,@INSCMUNIC2
													,@INSCPRODUTOR
													,@INSCPRODUTOR2
													,@DTANASCFUND
													,@DTANASCFUND2
													,@EMAIL
													,@EMAIL2
													,@HOMEPAGE
													,@HOMEPAGE2
													,@ESTADOCIVIL
													,@ESTADOCIVIL2
													,@ATIVIDADE
													,@ATIVIDADE2
													,@RENDAFATURAMENTO
													,@RENDAFATURAMENTO2
													,@GRAUINSTRUCAO
													,@GRAUINSTRUCAO2
													,@GRUPO
													,@GRUPO2
													,@PORTE
													,@PORTE2
													,@STATUS
													,@STATUS2


	WHILE (@@FETCH_STATUS = 0)
	BEGIN

----				IF ISNULL( @NOMERAZAO2, '@')<>'@' OR @NOMERAZAO=@NOMERAZAO2 SET @NOMERAZAO2 = Null
				IF ISNULL( @FANTASIA2, 		'@')='@' OR @FANTASIA2 = @FANTASIA 							set @FANTASIA2		=null
				IF ISNULL( @CIDADE2, 		'@')='@' OR @CIDADE2 	= @CIDADE							set @CIDADE2		=null
				IF ISNULL( @UF2, 		'@')='@' OR @UF2 	= @UF										set @UF2			=null
				IF ISNULL( @PAIS2, 		'@')='@' OR @PAIS2	= @PAIS										set @PAIS2			=null
				IF ISNULL( @BAIRRO2, 		'@')='@' OR @BAIRRO2	= @BAIRRO							set @BAIRRO2		=null
				IF ISNULL( @TIPOLOGRADOURO2, 	'@')='@' OR @TIPOLOGRADOURO2 	= @TIPOLOGRADOURO		set @TIPOLOGRADOURO2	=null
				IF ISNULL( @LOGRADOURO2, 	'@')='@' OR @LOGRADOURO2	= @LOGRADOURO					set @LOGRADOURO2	=null
				IF ISNULL( @NROLOGRADOURO2, 	'@')='@' OR @NROLOGRADOURO2	= @NROLOGRADOURO or @NROLOGRADOURO2='0'	set @NROLOGRADOURO2	=null
				IF ISNULL( @CMPLTOLOGRADOURO2, 	'@')='@' OR @CMPLTOLOGRADOURO2	= @CMPLTOLOGRADOURO OR @CMPLTOLOGRADOURO2='0'	set @CMPLTOLOGRADOURO2	=null
				IF ISNULL( @CEP2, 		'@')='@' OR @CEP2	= @CEP	OR @CEP2='0'						set @CEP2		=null
				IF ISNULL( @FONEDDD12, 		'@')='@' OR @FONEDDD12	= @FONEDDD1	OR @FONEDDD12='0'		set @FONEDDD12		=null
				IF ISNULL( @FONENRO12, 0)=0 OR @FONENRO12 = @FONENRO1	OR @FONENRO12=0					set @FONENRO12		=null
				IF ISNULL( @FONECMPL12, 	'@')='@' OR @FONECMPL12	= @FONECMPL1 						set @FONECMPL12		=null
				IF ISNULL( @FONEDDD22, 		'@')='@' OR @FONEDDD22	= @FONEDDD2	OR @FONEDDD22='0'		set @FONEDDD22		=null
				IF ISNULL( @FONENRO22, 0)=0 OR @FONENRO22 = @FONENRO2	OR @FONENRO22=0					set @FONENRO22		=null
				IF ISNULL( @FONECMPL22, 	'@')='@' OR @FONECMPL22	= @FONECMPL2 						set @FONECMPL22		=null
				IF ISNULL( @FONEDDD32, 		'@')='@' OR @FONEDDD32	= @FONEDDD3	OR @FONEDDD32='0'		set @FONEDDD32		=null
				IF ISNULL( @FONENRO32, 0)=0 OR @FONENRO32 = @FONENRO3	OR @FONENRO3=0					set @FONENRO32		=null
				IF ISNULL( @FONECMPL32, 	'@')='@' OR @FONECMPL32	= @FONECMPL3						set @FONECMPL32		=null
				IF ISNULL( @FAXDDD2, 		'@')='@' OR @FAXDDD2	= @FAXDDD OR @FAXDDD2='0'			set @FAXDDD2		=null
				IF ISNULL( @FAXNRO2, 0)=0 OR @FAXNRO2 = @FAXNRO	OR @FAXNRO2=0							set @FAXNRO2		=null
				IF ISNULL( @NROCGCCPF2, 0)=0 OR @NROCGCCPF2 = @NROCGCCPF OR @NROCGCCPF=0				set @NROCGCCPF2		=null
				IF ISNULL( @DIGCGCCPF2, 0)=0 OR @DIGCGCCPF2 = @DIGCGCCPF OR @DIGCGCCPF=0				set @DIGCGCCPF2		=null
				IF ISNULL( @INSCRICAORG2, 	'@')='@' OR @INSCRICAORG2	= @INSCRICAORG					set @INSCRICAORG2	=null
				IF ISNULL( @UFEMISSOR2, 	'@')='@' OR @UFEMISSOR2	= @UFEMISSOR						set @UFEMISSOR2		=null
				IF ISNULL( @ORGAOEMISSOR2, 	'@')='@' OR @ORGAOEMISSOR2	= @ORGAOEMISSOR	OR @ORGAOEMISSOR2=''	set @ORGAOEMISSOR2	=null
				IF ISNULL( @INSCMUNIC2, 	'@')='@' OR @INSCMUNIC2	= @INSCMUNIC OR @INSCMUNIC2=''		set @INSCMUNIC2		=null
				IF ISNULL( @INSCPRODUTOR2, 	'@')='@' OR @INSCPRODUTOR2	= @INSCPRODUTOR OR @INSCPRODUTOR2=''		set @INSCPRODUTOR2	=null
				IF @DTANASCFUND2 IS NULL OR @DTANASCFUND2 = @DTANASCFUND 								set @DTANASCFUND2	=null
				IF ISNULL( @EMAIL2, 		'@')='@' OR @EMAIL2		= @EMAIL							set @EMAIL2			=null
				IF ISNULL( @HOMEPAGE2, 		'@')='@' OR @HOMEPAGE2	= @HOMEPAGE							set @HOMEPAGE2		=null
				IF ISNULL( @ESTADOCIVIL2, 	'@')='@' OR @ESTADOCIVIL2	= @ESTADOCIVIL					set @ESTADOCIVIL2	=null
				IF ISNULL( @ATIVIDADE2, 	'@')='@' OR @ATIVIDADE2	= @ATIVIDADE OR @ATIVIDADE2='.' OR @ATIVIDADE2=''			set @ATIVIDADE2		=null
				IF ISNULL( @RENDAFATURAMENTO2, 	'@')='@' OR @RENDAFATURAMENTO2	= @RENDAFATURAMENTO	OR @RENDAFATURAMENTO2='.' OR @RENDAFATURAMENTO2='' set @RENDAFATURAMENTO2	=null
				IF ISNULL( @GRAUINSTRUCAO2, 	'@')='@' OR @GRAUINSTRUCAO2	= @GRAUINSTRUCAO OR @GRAUINSTRUCAO2='.' OR @GRAUINSTRUCAO2=''	set @GRAUINSTRUCAO2	=null
				IF ISNULL( @GRUPO2, 		'@')='@' OR @GRUPO2		= @GRUPO OR @GRUPO2='.' OR @GRUPO2=''		set @GRUPO2		=null
				IF ISNULL( @PORTE2, 		'@')='@' OR @PORTE2		= @PORTE OR @PORTE2='.' OR @PORTE2=''		set @PORTE2     =null
				IF ISNULL( @STATUS2, 		'@')='@' OR @STATUS2		= @STATUS						set @STATUS2     =null

				execute dbo.PR_COL_MOVPESSOA
						   null 
						  ,null
						  ,@GRUPO2
						  ,@STATUS2
						  ,null
						  ,@NOMERAZAO2
						  ,@FANTASIA2
						  ,null
						  ,null
						  ,@CIDADE2
						  ,@UF2
						  ,@BAIRRO2
						  ,@TIPOLOGRADOURO2
						  ,@LOGRADOURO2
						  ,@NROLOGRADOURO2
						  ,@CMPLTOLOGRADOURO2
						  ,@CEP2
						  ,@PAIS2
						  ,null
						  ,@NROCGCCPF2
						  ,@DIGCGCCPF2
						  ,@INSCRICAORG2
						  ,@UFEMISSOR2
						  ,@ORGAOEMISSOR2
						  ,@INSCMUNIC2
						  ,@INSCPRODUTOR2
						  ,null
						  ,@DTANASCFUND2
						  ,@EMAIL2
						  ,@ESTADOCIVIL2
						  ,@ATIVIDADE2
						  ,@RENDAFATURAMENTO2
						  ,@GRAUINSTRUCAO2
						  ,null
						  ,@FONEDDD12
						  ,@FONENRO12
						  ,@FONECMPL12
						  ,@FONEDDD22
						  ,@FONENRO22
						  ,@FONECMPL22
						  ,@FONEDDD32
						  ,@FONENRO32
						  ,@FONECMPL32
						  ,@FAXDDD2
						  ,@FAXNRO2
						  ,@HOMEPAGE2
						  ,@PORTE2
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,'INTEGRAÇÃONOROESTE'
						  ,null
						  ,null
						  ,null
						  ,null
						  ,null
						  ,@SEQPESSOA
						  ,'INTEGRAÇÃONOROESTE'
						  ,null

		FETCH NEXT FROM curPES INTO 		
													 @SEQPESSOA
													,@DTAINCLUSAO
													,@DTAINCLUSAO2
													,@NOMERAZAO
													,@NOMERAZAO2
													,@FANTASIA
													,@FANTASIA2
													,@CIDADE
													,@CIDADE2
													,@UF
													,@UF2
													,@PAIS
													,@PAIS2
													,@BAIRRO
													,@BAIRRO2
													,@TIPOLOGRADOURO
													,@TIPOLOGRADOURO2
													,@LOGRADOURO
													,@LOGRADOURO2
													,@NROLOGRADOURO
													,@NROLOGRADOURO2
													,@CMPLTOLOGRADOURO
													,@CMPLTOLOGRADOURO2
													,@CEP
													,@CEP2
													,@FONEDDD1
													,@FONEDDD12
													,@FONENRO1
													,@FONENRO12
													,@FONECMPL1
													,@FONECMPL12
													,@FONEDDD2
													,@FONEDDD22
													,@FONENRO2
													,@FONENRO22
													,@FONECMPL2
													,@FONECMPL22
													,@FONEDDD3
													,@FONEDDD32
													,@FONENRO3
													,@FONENRO32
													,@FONECMPL3
													,@FONECMPL32
													,@FAXDDD
													,@FAXDDD2
													,@FAXNRO
													,@FAXNRO2
													,@NROCGCCPF
													,@NROCGCCPF2
													,@DIGCGCCPF
													,@DIGCGCCPF2
													,@INSCRICAORG
													,@INSCRICAORG2
													,@UFEMISSOR
													,@UFEMISSOR2
													,@ORGAOEMISSOR
													,@ORGAOEMISSOR2
													,@INSCMUNIC
													,@INSCMUNIC2
													,@INSCPRODUTOR
													,@INSCPRODUTOR2
													,@DTANASCFUND
													,@DTANASCFUND2
													,@EMAIL
													,@EMAIL2
													,@HOMEPAGE
													,@HOMEPAGE2
													,@ESTADOCIVIL
													,@ESTADOCIVIL2
													,@ATIVIDADE
													,@ATIVIDADE2
													,@RENDAFATURAMENTO
													,@RENDAFATURAMENTO2
													,@GRAUINSTRUCAO
													,@GRAUINSTRUCAO2
													,@GRUPO
													,@GRUPO2
													,@PORTE
													,@PORTE2
													,@STATUS
													,@STATUS2
						
	END

	CLOSE curPES
	DEALLOCATE curPES

End