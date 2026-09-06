/* ==============================================================
   Objeto ..........: dbo.PR_INC_OUT_PESSOA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2013-11-01 10:48:51
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 156
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : OUT_PESSOA
   Tabelas referidas: GE_PESSOA, GE_PESSOAVERSAO, OUT_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE PR_INC_OUT_PESSOA

AS

DECLARE 
           @SeqPessoa numeric(8,0),
           @NroCGCCPF decimal(13,0),
           @DigCGCCPF decimal(2,0),
           @PessoaLinkOrigem varchar(30),
           @Pessoalink varchar(30),
           @NomeRazao varchar(60),
           @Fantasia varchar(50),
           @Versao decimal(2,0),
           @Status varchar(1),
           @DtaAtivacao datetime,
           @FisicaJuridica varchar(1),
           @Sexo varchar(1),
           @Cidade varchar(50),
           @Uf varchar(2),
           @Pais varchar(25),
           @Bairro varchar(50),
           @TipoLogradouro varchar(15),
           @Logradouro varchar(80),
           @NroLogradouro varchar(10),
           @CmpltoLogradouro varchar(30),
           @Cep varchar(12),
           @CxPostal varchar(7),
		   @Latitude decimal(14,11),
		   @Longitude decimal(14,11),
           @FoneDDD1 varchar(5),
           @FoneNro1 decimal(12,0),
           @FoneCmpl1 varchar(12),
           @FoneDDD2 varchar(5),
           @FoneNro2 decimal(12,0),
           @FoneCmpl2 varchar(12),
           @FoneDDD3 varchar(5),
           @FoneNro3 decimal(12,0),
           @FoneCmpl3 varchar(12),
           @FaxDDD varchar(5),
           @FaxNro decimal(12,0),
           @InscricaoRG varchar(20),
           @UFEmissor varchar(2),
           @OrgaoEmissor varchar(10),
           @DtaNascFund datetime,
           @Email varchar(70),
           @HomePage varchar(80),
           @EstadoCivil varchar(20),
           @Atividade varchar(30),
           @RendaFaturamento varchar(30),
           @GrauInstrucao varchar(30),
           @Grupo varchar(30),
           @Porte varchar(30),
           @DtaInclusao datetime,
           @UsuInclusao varchar(20),
           @DtaAlteracao datetime,
           @UsuAlteracao varchar(20),
           @DtaInativacao datetime,
           @UsuInativacao varchar(20),
           @ObsInativacao varchar(50),
           @Telefonema numeric(1,0),
           @Correspondencia numeric(1,0),
           @RecebeEmail numeric(1,0),
           @NaoPossuiEmail numeric(1,0),
           @IndContribICMS varchar(1),
           @Origem varchar(20),
           @UltOrigem varchar(20),
           @Seq numeric
		   
DECLARE CURPES CURSOR FOR

SELECT DISTINCT 
     PES.SEQPESSOA,        PES.NROCGCCPF,         PES.DIGCGCCPF,        '' AS PESSOALINKORIGEM,
     '' AS PESSOALINK,     PES.NOMERAZAO,         PES.FANTASIA,         0 AS VERSAO,
     PES.STATUS,           PES.DTAATIVACAO,       PES.FISICAJURIDICA,   PES.SEXO,
     PES.CIDADE,           PES.UF,                PES.PAIS,             PES.BAIRRO,
     PES.TIPOLOGRADOURO,   PES.LOGRADOURO,        PES.NROLOGRADOURO,    PES.CMPLTOLOGRADOURO,
     PES.CEP,              PES.CXPOSTAL,          PES.LATITUDE,			PES.LONGITUDE,
	 PES.FONEDDD1,         PES.FONENRO1,
     PES.FONECMPL1,        PES.FONEDDD2,          PES.FONENRO2,         PES.FONECMPL2,
     PES.FONEDDD3,         PES.FONENRO3,          PES.FONECMPL3,        PES.FAXDDD,
     PES.FAXNRO,           PES.INSCRICAORG,       PES.UFEMISSOR,        PES.ORGAOEMISSOR,
     PES.DTANASCFUND,      PES.EMAIL,             PES.HOMEPAGE,         PES.ESTADOCIVIL,
     PES.ATIVIDADE,        PES.RENDAFATURAMENTO,  PES.GRAUINSTRUCAO,    PES.GRUPO,
     PES.PORTE,            PES.DTAINCLUSAO,       PES.USUINCLUSAO,      PES.DTAALTERACAO,
     PES.USUALTERACAO,     PES.DTAINATIVACAO,     PES.USUINATIVACAO,    PES.OBSINATIVACAO,
     PES.TELEFONEMA,       PES.CORRESPONDENCIA,   PES.RECEBEEMAIL,      PES.NAOPOSSUIEMAIL,
     PES.INDCONTRIBICMS,   PES.ORIGEM,            PES.ULTORIGEM
FROM GE_PESSOA PES  
	   JOIN GE_PESSOAVERSAO VER 
			ON VER.SEQPESSOA = PES.SEQPESSOA
			AND VER.VERSAO = ( SELECT MAX(VERSAO) FROM GE_PESSOAVERSAO WHERE SEQPESSOA = PES.SEQPESSOA)
	   
         
BEGIN 
	OPEN CURPES
	FETCH NEXT FROM CURPES INTO 	
	 @SeqPessoa,           @NroCGCCPF,          @DigCGCCPF,           @PessoaLinkOrigem,
     @Pessoalink,          @NomeRazao,          @Fantasia,            @Versao,
     @Status,              @DtaAtivacao,        @FisicaJuridica,      @Sexo,
     @Cidade,              @Uf,                 @Pais,                @Bairro,
     @TipoLogradouro,      @Logradouro,         @NroLogradouro,       @CmpltoLogradouro,
     @Cep,                 @CxPostal,           @Latitude,			  @Longitude,
	 @FoneDDD1,            @FoneNro1,
     @FoneCmpl1,           @FoneDDD2,           @FoneNro2,            @FoneCmpl2,
     @FoneDDD3,            @FoneNro3,           @FoneCmpl3,           @FaxDDD,
     @FaxNro,              @InscricaoRG,        @UFEmissor,           @OrgaoEmissor,
     @DtaNascFund,         @Email,              @HomePage,            @EstadoCivil,
     @Atividade,           @RendaFaturamento,   @GrauInstrucao,       @Grupo,
     @Porte,               @DtaInclusao,        @UsuInclusao,         @DtaAlteracao,
     @UsuAlteracao,        @DtaInativacao,      @UsuInativacao,       @ObsInativacao,
     @Telefonema,          @Correspondencia,    @RecebeEmail,         @NaoPossuiEmail,
     @IndContribICMS,      @Origem,             @UltOrigem 
	
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
	
	INSERT INTO dbo.OUT_PESSOA VALUES (
     @SeqPessoa,           @NroCGCCPF,          @DigCGCCPF,           @PessoaLinkOrigem,
     @Pessoalink,          @NomeRazao,          @Fantasia,            @Versao,
     @Status,              @DtaAtivacao,        @FisicaJuridica,      @Sexo,
     @Cidade,              @Uf,                 @Pais,                @Bairro,
     @TipoLogradouro,      @Logradouro,         @NroLogradouro,       @CmpltoLogradouro,
     @Cep,                 @CxPostal,           @Latitude,			  @Longitude,
	 @FoneDDD1,            @FoneNro1,
     @FoneCmpl1,           @FoneDDD2,           @FoneNro2,            @FoneCmpl2,
     @FoneDDD3,            @FoneNro3,           @FoneCmpl3,           @FaxDDD,
     @FaxNro,              @InscricaoRG,        @UFEmissor,           @OrgaoEmissor,
     @DtaNascFund,         @Email,              @HomePage,            @EstadoCivil,
     @Atividade,           @RendaFaturamento,   @GrauInstrucao,       @Grupo,
     @Porte,               @DtaInclusao,        @UsuInclusao,         @DtaAlteracao,
     @UsuAlteracao,        @DtaInativacao,      @UsuInativacao,       @ObsInativacao,
     @Telefonema,          @Correspondencia,    @RecebeEmail,         @NaoPossuiEmail,
     @IndContribICMS,      @Origem,             @UltOrigem 
	)
		
	FETCH NEXT FROM CURPES INTO 	
	 @SeqPessoa,           @NroCGCCPF,          @DigCGCCPF,           @PessoaLinkOrigem,
     @Pessoalink,          @NomeRazao,          @Fantasia,            @Versao,
     @Status,              @DtaAtivacao,        @FisicaJuridica,      @Sexo,
     @Cidade,              @Uf,                 @Pais,                @Bairro,
     @TipoLogradouro,      @Logradouro,         @NroLogradouro,       @CmpltoLogradouro,
     @Cep,                 @CxPostal,           @Latitude,			  @Longitude,
	 @FoneDDD1,            @FoneNro1,
     @FoneCmpl1,           @FoneDDD2,           @FoneNro2,            @FoneCmpl2,
     @FoneDDD3,            @FoneNro3,           @FoneCmpl3,           @FaxDDD,
     @FaxNro,              @InscricaoRG,        @UFEmissor,           @OrgaoEmissor,
     @DtaNascFund,         @Email,              @HomePage,            @EstadoCivil,
     @Atividade,           @RendaFaturamento,   @GrauInstrucao,       @Grupo,
     @Porte,               @DtaInclusao,        @UsuInclusao,         @DtaAlteracao,
     @UsuAlteracao,        @DtaInativacao,      @UsuInativacao,       @ObsInativacao,
     @Telefonema,          @Correspondencia,    @RecebeEmail,         @NaoPossuiEmail,
     @IndContribICMS,      @Origem,             @UltOrigem 			
	END   	
	CLOSE CURPES
	DEALLOCATE CURPES	  
END