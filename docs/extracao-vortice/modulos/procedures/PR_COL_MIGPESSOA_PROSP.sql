/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGPESSOA_PROSP
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-27 20:23:49
   Modificado em ...: 2024-12-30 17:21:15
   Linhas ..........: 412
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA_ITA, MIG_GE_PESSOA
   Outras refs .....: PR_COL_MOVPESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MIGPESSOA_PROSP] 
AS


/*
-- autor: AMAURY 25/10/2024
   objetivo: Migrar pessoas prospects com cnpj da norte para noroeste
   log de alterações: 01/11/2024 - Criação da rotina testando e finalizando criação da rotina
*/	

	DECLARE 
	@SEQPESSOA		 numeric(10, 0) ,
	@SEQCIDADE		 decimal(6, 0) ,
	@SEQBAIRRO		 decimal(5, 0) ,
	@VERSAO			 decimal(2, 0) ,
	@STATUS			 char(1) ,
	@DTAATIVACAO		 datetime ,
	@NOMERAZAO		Varchar(100) ,
	@FANTASIA		Varchar(50) ,
	@PALAVRACHAVE		Varchar(50) ,
	@FISICAJURIDICA		Char(1) ,
	@SEXO			 char(1) ,
	@CIDADE			 varchar(50) ,
	@UF			 varchar(2) ,
	@PAIS			 varchar(25) ,
	@BAIRRO			 varchar(50) ,
	@TIPOLOGRADOURO		 varchar(15) ,
	@LOGRADOURO		 varchar(80) ,
	@NROLOGRADOURO		 varchar(10) ,
	@CMPLTOLOGRADOURO	 varchar(30) ,
	@CEP			 varchar(12) ,
	@CXPOSTAL		 varchar(7) ,
	@SEQPESSOAENDCOBR	 decimal(3, 0) ,
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
	@CNAE			 varchar(15) ,
	@DTANASCFUND		 datetime ,
	@ORIGEM			 varchar(20) ,
	@ULTORIGEM		 varchar(20) ,
	@EMAIL			 varchar(70) ,
	@HOMEPAGE		 varchar(80) ,
	@ESTADOCIVIL		 varchar(20) ,
	@ATIVIDADE		 varchar(30) ,
	@RENDAFATURAMENTO	 varchar(30) ,
	@GRAUINSTRUCAO		 varchar(30) ,
	@GRUPO			 varchar(30) ,
	@PORTE			 varchar(30) ,
	@DTAINCLUSAO		 datetime ,
	@USUINCLUSAO		 varchar(20) ,
	@DTAALTERACAO		 datetime ,
	@USUALTERACAO		 varchar(20) ,
	@DTAINATIVACAO		 datetime ,
	@USUINATIVACAO		 varchar(20) ,
	@OBSINATIVACAO		 varchar(50) ,
	@CODEQUIPE		 varchar(20) ,
	@TELEFONEMA		 numeric(1, 0) ,
	@CORRESPONDENCIA	 numeric(1, 0) ,
	@RECEBEEMAIL		 numeric(1, 0) ,
	@NAOPOSSUIEMAIL		 numeric(1, 0) ,
	@PROBLEMACREDITO	 numeric(1, 0) ,
	@INDCONTRIBICMS		 char(1) ,
	@REFENDERECO		 varchar(150) ,
	@LATITUDE		 decimal(14, 11) ,
	@LONGITUDE		 decimal(14, 11) ,
	@SEQREGIAO		 decimal(6, 0) ,
	@SEQROTA		 decimal(6, 0) ,
	@RECEBESMS		 numeric(1, 0) ,
	@SEQPESSOAPRC		 numeric(10, 0) ,
	@SKYPE			 varchar(70) ,
	@SMSCODIGO		 varchar(8) ,
	@SMSCODIGODTA		 datetime ,
	@PESSOALINK      varchar(250)

	Declare curPES
	CURSOR
	FOR
		SELECT
			   PI.SEQPESSOA
			  ,PI.SEQCIDADE
			  ,PI.SEQBAIRRO
			  ,PI.VERSAO
			  ,PI.STATUS
			  ,PI.DTAATIVACAO
			  ,PI.NOMERAZAO
			  ,PI.FANTASIA
			  ,PI.PALAVRACHAVE
			  ,PI.FISICAJURIDICA
			  ,PI.SEXO
			  ,PI.CIDADE
			  ,PI.UF
			  ,PI.PAIS
			  ,PI.BAIRRO
			  ,PI.TIPOLOGRADOURO
			  ,PI.LOGRADOURO
			  ,PI.NROLOGRADOURO
			  ,PI.CMPLTOLOGRADOURO
			  ,PI.CEP
			  ,PI.CXPOSTAL
			  ,PI.SEQPESSOAENDCOBR
			  ,PI.FONEDDD1
			  ,PI.FONENRO1
			  ,PI.FONECMPL1
			  ,PI.FONEDDD2
			  ,PI.FONENRO2
			  ,PI.FONECMPL2
			  ,PI.FONEDDD3
			  ,PI.FONENRO3
			  ,PI.FONECMPL3
			  ,PI.FAXDDD
			  ,PI.FAXNRO
			  ,PI.NROCGCCPF
			  ,PI.DIGCGCCPF
			  ,PI.INSCRICAORG
			  ,PI.UFEMISSOR
			  ,PI.ORGAOEMISSOR
			  ,PI.INSCMUNIC
			  ,PI.INSCPRODUTOR
			  ,PI.CNAE
			  ,PI.DTANASCFUND
			  ,'INTEGRAÇÃONOROESTE' AS ORIGEM
			  ,'INTEGRAÇÃONOROESTE' AS ULTORIGEM
			  ,PI.EMAIL
			  ,PI.HOMEPAGE
			  ,PI.ESTADOCIVIL
			  ,PI.ATIVIDADE
			  ,PI.RENDAFATURAMENTO
			  ,PI.GRAUINSTRUCAO
			  ,PI.GRUPO
			  ,PI.PORTE
			  ,PI.DTAINCLUSAO
			  ,PI.USUINCLUSAO
			  ,PI.DTAALTERACAO
			  ,PI.USUALTERACAO
			  ,PI.DTAINATIVACAO
			  ,PI.USUINATIVACAO
			  ,PI.OBSINATIVACAO
			  ,PI.CODEQUIPE
			  ,PI.TELEFONEMA
			  ,PI.CORRESPONDENCIA
			  ,PI.RECEBEEMAIL
			  ,PI.NAOPOSSUIEMAIL
			  ,PI.PROBLEMACREDITO
			  ,PI.INDCONTRIBICMS
			  ,PI.REFENDERECO
			  ,PI.LATITUDE
			  ,PI.LONGITUDE
			  ,PI.SEQREGIAO
			  ,PI.SEQROTA
			  ,PI.RECEBESMS
			  ,PI.SEQPESSOAPRC
			  ,PI.SKYPE
			  ,PI.SMSCODIGO
			  ,PI.SMSCODIGODTA
		FROM MIG_GE_PESSOA MP
		JOIN GE_PESSOA_ITA PI ON PI.SEQPESSOA = MP.SEQPESSOA
		WHERE 1=1
		  AND MP.DESCRICAO in ( 'Prospect com CNPJ Inexistente', 'Prospect com CNPJ<>0 Inexistente', 'Prospect sem CNPJ' )
		  AND MP.SEQPESSOA in ( 3302, 3153, 13239, 21292) ---- 10905, 24692, 65932 )

begin
	OPEN curPES
		FETCH NEXT FROM curPES INTO 			@SEQPESSOA
													  ,@SEQCIDADE
													  ,@SEQBAIRRO
													  ,@VERSAO
													  ,@STATUS
													  ,@DTAATIVACAO
													  ,@NOMERAZAO
													  ,@FANTASIA
													  ,@PALAVRACHAVE
													  ,@FISICAJURIDICA
													  ,@SEXO
													  ,@CIDADE
													  ,@UF
													  ,@PAIS
													  ,@BAIRRO
													  ,@TIPOLOGRADOURO
													  ,@LOGRADOURO
													  ,@NROLOGRADOURO
													  ,@CMPLTOLOGRADOURO
													  ,@CEP
													  ,@CXPOSTAL
													  ,@SEQPESSOAENDCOBR
													  ,@FONEDDD1
													  ,@FONENRO1
													  ,@FONECMPL1
													  ,@FONEDDD2
													  ,@FONENRO2
													  ,@FONECMPL2
													  ,@FONEDDD3
													  ,@FONENRO3
													  ,@FONECMPL3
													  ,@FAXDDD
													  ,@FAXNRO
													  ,@NROCGCCPF
													  ,@DIGCGCCPF
													  ,@INSCRICAORG
													  ,@UFEMISSOR
													  ,@ORGAOEMISSOR
													  ,@INSCMUNIC
													  ,@INSCPRODUTOR
													  ,@CNAE
													  ,@DTANASCFUND
													  ,@ORIGEM
													  ,@ULTORIGEM
													  ,@EMAIL
													  ,@HOMEPAGE
													  ,@ESTADOCIVIL
													  ,@ATIVIDADE
													  ,@RENDAFATURAMENTO
													  ,@GRAUINSTRUCAO
													  ,@GRUPO
													  ,@PORTE
													  ,@DTAINCLUSAO
													  ,@USUINCLUSAO
													  ,@DTAALTERACAO
													  ,@USUALTERACAO
													  ,@DTAINATIVACAO
													  ,@USUINATIVACAO
													  ,@OBSINATIVACAO
													  ,@CODEQUIPE
													  ,@TELEFONEMA
													  ,@CORRESPONDENCIA
													  ,@RECEBEEMAIL
													  ,@NAOPOSSUIEMAIL
													  ,@PROBLEMACREDITO
													  ,@INDCONTRIBICMS
													  ,@REFENDERECO
													  ,@LATITUDE
													  ,@LONGITUDE
													  ,@SEQREGIAO
													  ,@SEQROTA
													  ,@RECEBESMS
													  ,@SEQPESSOAPRC
													  ,@SKYPE
													  ,@SMSCODIGO
													  ,@SMSCODIGODTA

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
	            Set @PESSOALINK = CAST ( @SEQPESSOA AS VARCHAR)

				execute dbo.PR_COL_MOVPESSOA
						   @PESSOALINK 
						  ,@ORIGEM
						  ,@GRUPO
						  ,@STATUS
						  ,@VERSAO
						  ,@NOMERAZAO
						  ,@FANTASIA
						  ,@PALAVRACHAVE
						  ,@SEXO
						  ,@CIDADE
						  ,@UF
						  ,@BAIRRO
						  ,@TIPOLOGRADOURO
						  ,@LOGRADOURO
						  ,@NROLOGRADOURO
						  ,@CMPLTOLOGRADOURO
						  ,@CEP
						  ,@PAIS
						  ,@FISICAJURIDICA
						  ,@NROCGCCPF
						  ,@DIGCGCCPF
						  ,@INSCRICAORG
						  ,@UFEMISSOR
						  ,@ORGAOEMISSOR
						  ,@INSCMUNIC
						  ,@INSCPRODUTOR
						  ,@CNAE
						  ,@DTANASCFUND
						  ,@EMAIL
						  ,@ESTADOCIVIL
						  ,@ATIVIDADE
						  ,@RENDAFATURAMENTO
						  ,@GRAUINSTRUCAO
						  ,@SEQPESSOAENDCOBR
						  ,@FONEDDD1
						  ,@FONENRO1
						  ,@FONECMPL1
						  ,@FONEDDD2
						  ,@FONENRO2
						  ,@FONECMPL2
						  ,@FONEDDD3
						  ,@FONENRO3
						  ,@FONECMPL3
						  ,@FAXDDD
						  ,@FAXNRO
						  ,@HOMEPAGE
						  ,@PORTE
						  ,@CODEQUIPE
						  ,@NAOPOSSUIEMAIL
						  ,@PROBLEMACREDITO
						  ,@INDCONTRIBICMS
						  ,@REFENDERECO
						  ,@LATITUDE
						  ,@LONGITUDE
						  ,@SEQPESSOAPRC
						  ,@SKYPE
						  ,@SMSCODIGO
						  ,@SMSCODIGODTA
						  ,@ORIGEM
						  ,@ULTORIGEM
						  ,@TELEFONEMA
						  ,@CORRESPONDENCIA
						  ,@RECEBEEMAIL
						  ,@RECEBESMS
						  ,0
						  ,@USUALTERACAO
						  ,@DTAALTERACAO


		FETCH NEXT FROM curPES INTO 			@SEQPESSOA
													  ,@SEQCIDADE
													  ,@SEQBAIRRO
													  ,@VERSAO
													  ,@STATUS
													  ,@DTAATIVACAO
													  ,@NOMERAZAO
													  ,@FANTASIA
													  ,@PALAVRACHAVE
													  ,@FISICAJURIDICA
													  ,@SEXO
													  ,@CIDADE
													  ,@UF
													  ,@PAIS
													  ,@BAIRRO
													  ,@TIPOLOGRADOURO
													  ,@LOGRADOURO
													  ,@NROLOGRADOURO
													  ,@CMPLTOLOGRADOURO
													  ,@CEP
													  ,@CXPOSTAL
													  ,@SEQPESSOAENDCOBR
													  ,@FONEDDD1
													  ,@FONENRO1
													  ,@FONECMPL1
													  ,@FONEDDD2
													  ,@FONENRO2
													  ,@FONECMPL2
													  ,@FONEDDD3
													  ,@FONENRO3
													  ,@FONECMPL3
													  ,@FAXDDD
													  ,@FAXNRO
													  ,@NROCGCCPF
													  ,@DIGCGCCPF
													  ,@INSCRICAORG
													  ,@UFEMISSOR
													  ,@ORGAOEMISSOR
													  ,@INSCMUNIC
													  ,@INSCPRODUTOR
													  ,@CNAE
													  ,@DTANASCFUND
													  ,@ORIGEM
													  ,@ULTORIGEM
													  ,@EMAIL
													  ,@HOMEPAGE
													  ,@ESTADOCIVIL
													  ,@ATIVIDADE
													  ,@RENDAFATURAMENTO
													  ,@GRAUINSTRUCAO
													  ,@GRUPO
													  ,@PORTE
													  ,@DTAINCLUSAO
													  ,@USUINCLUSAO
													  ,@DTAALTERACAO
													  ,@USUALTERACAO
													  ,@DTAINATIVACAO
													  ,@USUINATIVACAO
													  ,@OBSINATIVACAO
													  ,@CODEQUIPE
													  ,@TELEFONEMA
													  ,@CORRESPONDENCIA
													  ,@RECEBEEMAIL
													  ,@NAOPOSSUIEMAIL
													  ,@PROBLEMACREDITO
													  ,@INDCONTRIBICMS
													  ,@REFENDERECO
													  ,@LATITUDE
													  ,@LONGITUDE
													  ,@SEQREGIAO
													  ,@SEQROTA
													  ,@RECEBESMS
													  ,@SEQPESSOAPRC
													  ,@SKYPE
													  ,@SMSCODIGO
													  ,@SMSCODIGODTA
						
	END

	CLOSE curPES
	DEALLOCATE curPES

End
