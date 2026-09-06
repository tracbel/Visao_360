/* ==============================================================
   Objeto ..........: dbo.PR_COL_MOVPESSOA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-27 20:22:14
   Modificado em ...: 2024-12-27 20:22:14
   Linhas ..........: 578
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : ge_cidade, GEP_IMPORT
   Tabelas referidas: GE_CIDADE, GEP_IMPORT
   Outras refs .....: fC5_ColDate, fC5_ColNumber, fC5_ColString
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MOVPESSOA] 
      @vspessoalink  varchar(50),
      @vspessoalinkorigem  varchar(50),
      @vsGrupo  varchar(30),
      @vsStatus  char(1),
      @vNVersao numeric(2),
      @vsNomerazao  varchar(60),
      @vsFantasia   varchar(60),
      @vsPalavraChave varchar(50),
      @vsSexo  char(1),
      @vsCidade  varchar(50),
      @vsUf  varchar(2),
      @vsBairro  varchar(50),
      @vsTipoLogradouro  varchar(15),
      @vsLogradouro  varchar(80),
      @vsNrologradouro  varchar(10),
      @vsCmpltologradouro  varchar(30),
      @vsCep  varchar(12),
      @vsPais  varchar(30),
      @vsFisicajuridica  char(1),
      @vnnrocgccpf  numeric(18),
      @vnDigcgccpf  numeric(6),
      @vsInscricaoRg  varchar(15),
      @vsUFEmissor varchar(2),
      @vsOrgaoEmissor varchar(10),
      @vsInscMunic varchar(15),
      @vsInscProdutor varchar(20),
      @vsCNAE varchar(15),
      @vdDtaNascfund  date,
      @vsEmail  varchar(70),
      @vsEstadocivil  varchar(20),
      @vsAtividade  varchar(30),
      @vsRendaFaturamento  varchar(30),
      @vsgrauinstrucao  varchar(30),
      @vnSeqPessoaEndCobr numeric(3),
      @vsFoneDDD1  varchar(5),
      @vnFonenro1  numeric(15),
      @vsFonecmpl1  varchar(12),
      @vsFoneDDD2  varchar(5),
      @vnFonenro2  numeric(15),
      @vsFonecmpl2  varchar(12),
      @vsFoneDDD3  varchar(5),
      @vnFonenro3  numeric(15),
      @vsFonecmpl3  varchar(12),
      @vsFaxDDD varchar(5),
      @vnFaxNro numeric(12),

      @vsHomePage varchar(80),
      @vsPorte varchar(30),
      @vsCodEquipe varchar(20),
      @vnNaoPossuiEmail numeric(1),
      @vnProblemaCredito numeric(1),
      @vsIndContribICMS char(1),
      @vsRefEndereco varchar(150),
      @vnLatitude numeric(14, 11),
      @vnLongitude numeric(14, 11),
      @vnSEQPESSOAPRC numeric(10),
      @vsSkype varchar(70),
      @vsSMSCODIGO varchar(8),
      @vsSMSCODIGODTA date,

      @vsOrigem  varchar(50),
      @vsUltOrigem  varchar(50),
      @vnTelefonema  integer,
      @vnCorrespondencia  integer,
      @vnRecebeemail  integer,
      @vnRecebesms  integer,
      @vnSeqpessoa  numeric,
      @vsUsuAlteracao  varchar(20),
      @vdUsuDtAlteracao  date

as

       Declare @vnCount                                              integer
       Declare @vbOk                                                 Bit
       Declare @vtTab                                                varchar(50)
       Declare @vsSepar                                              char(1)
       Declare @vnSeqcid                                             integer
      
      Declare @Origem         varchar(20)
      Declare @Processo    varchar(20)
      Declare @Banco       varchar(20)
      Declare @Dono        varchar(20)
      Declare @Tabela         varchar(20)
      Declare @Acao        char(1)
      Declare @Dtageracao     date
      Declare @Coluna         varchar(1000)
      Declare @Dado        varchar(1000)
      Declare @Colunaident    varchar(250)
      Declare @Dadoident      varchar(250)


/* Procedure que recebe dados como parametro e insere na tabela de Integração GEP_IMPORT para ser processada pelo SERVIDOR DE PROCESSOS
   Vortice
   */

begin

--- Definindo o separador padrao
    set @vsSepar = ';'
    set @vbOk =  0

---- Definindo os dados basicos da tabela destino.

   set @Processo = 'VORTICOCRM'
   set @Origem  = @vsOrigem 
   set @Banco = null
   set @Dono = null
   set @Tabela = 'GE_PESSOA'
   set @Dtageracao = getdate()

   set @Coluna =  'ORIGEM' + @vsSepar + 'ULTORIGEM' + @vsSepar
   set @Dado =  @vsOrigem + @vsSepar + @vsOrigem 

if isnull(@vsStatus, '@')<>'@'
begin
   set @Coluna  = 'STATUS' + @vsSepar
   set @Dado = @vsStatus
end

if isnull(@vnSeqpessoa, 0)<>0  
begin
   set @Colunaident = 'SEQPESSOA'
   set @Dadoident = cast(@vnSeqpessoa as varchar)
   set @Acao = 'A'
end
else
begin
   set @Acao = 'I'
   if @vnnrocgccpf is null  
   begin
      set @Colunaident = 'PESSOALINK' + @vsSepar + 'PESSOALINKORIGEM'
      set @Dadoident = @vspessoalink + @vsSepar
      set @Dadoident = @Dadoident + char(39) + @vsPessoalinkorigem + char(39)
   end
   else
   begin
      set @Colunaident = 'NROCGCCPF' + @vsSepar + 'DIGCGCCPF'
      set @Dadoident = cast ( @vnnrocgccpf as varchar) + @vsSepar + cast ( @vnDigcgccpf as varchar)
   end
end

   set @Coluna =  @Coluna + 'NOMERAZAO' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @vsNomerazao, @vsSepar )

if @vsFantasia is not null  
begin
   set @Coluna =  @Coluna + 'FANTASIA' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFantasia, @vsSepar )
end

if @vsPalavraChave is not null  
begin
   set @Coluna =  @Coluna + 'PALAVRACHAVE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsPalavraChave, @vsSepar )
end

if @vsFisicajuridica is not null  
begin
   set @Coluna =  @Coluna + 'FISICAJURIDICA' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFisicajuridica, @vsSepar )
end

if @vsSexo is not null  
begin
   set @Coluna =  @Coluna + 'SEXO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsSexo, @vsSepar )
end

set @vnSeqcid=0
if @vsCidade is not null
begin
   set @vnSeqcid = (SELECT isnull(max(SEQCIDADE), 0) FROM GE_CIDADE WHERE CIDADE = @vsCidade and UF = @vsUF)
   if isnull(@vnSeqCid, 0)=0  
   begin
      set @vnSeqcid = (SELECT isnull(max(SEQCIDADE), 0) FROM GE_CIDADE)
      set @vnSeqcid = isnull(@vnSeqcid, 0) + 1
      insert into ge_cidade ( SEQCIDADE, CIDADE, UF, EXGBAIRRO, EXGLOGRADOURO, DTAALTERACAO, USUALTERACAO )
      values ( @vnSeqcid , @vsCidade, isnull(@vsUF, '**'), 'N', 'N', GETDATE(), 'Import' )
   end
end

if @vnSeqcid <> 0
begin
   set @Coluna =  @Coluna + 'SEQCIDADE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeqcid, @vsSepar )
end

if @vscidade is not null  
begin
   set @Coluna =  @Coluna + 'CIDADE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCidade, @vsSepar )
end

if @vsUf is not null  
begin
   set @Coluna =  @Coluna + 'UF' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsUf, @vsSepar )
end

if @vsbairro is not null  
begin
   set @Coluna =  @Coluna + 'BAIRRO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsBairro, @vsSepar )
end

if @vnVersao <> 0
begin
   set @Coluna =  @Coluna + 'VERSAO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnVersao, @vsSepar )
end

if @vslogradouro is not null  
begin
   set @Coluna =  @Coluna + 'LOGRADOURO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsLogradouro, @vsSepar )
end

if @vsTipoLogradouro is not null  
begin
   set @Coluna =  @Coluna + 'TIPOLOGRADOURO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsTipoLogradouro, @vsSepar )
end

if @vsNrologradouro is not null  
begin
   set @Coluna =  @Coluna + 'NROLOGRADOURO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsNrologradouro, @vsSepar )
end

if @vscmpltologradouro is not null  
begin
   set @Coluna =  @Coluna + 'CMPLTOLOGRADOURO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vscmpltologradouro, @vsSepar )
end

if @vscep is not null  
begin
   set @Coluna =  @Coluna + 'CEP' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vscep, @vsSepar )
end

if @vsPais is not null  
begin
   set @Coluna =  @Coluna + 'PAIS' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vspais, @vsSepar )
end

if @vnNrocgccpf is not null  
begin
   set @Coluna =  @Coluna + 'NROCGCCPF' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnNrocgccpf, @vsSepar )
end

if @vnDigcgccpf is not null  
begin
   set @Coluna =  @Coluna + 'DIGCGCCPF' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnDigcgccpf, @vsSepar )
end

if @vsInscricaorg is not null  
begin
   set @Coluna =  @Coluna + 'INSCRICAORG' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsinscricaorg, @vsSepar )
end

if @vsUFEmissor is not null  
begin
   set @Coluna =  @Coluna + 'UFEMISSOR' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsUFEmissor, @vsSepar )
end

if @vsOrgaoEmissor is not null  
begin
   set @Coluna =  @Coluna + 'ORGAOEMISSOR' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsOrgaoEmissor, @vsSepar )
end

if @vsInscMunic is not null  
begin
   set @Coluna =  @Coluna + 'INSCMUNIC' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsInscMunic, @vsSepar )
end

if @vsInscProdutor is not null  
begin
   set @Coluna =  @Coluna + 'INSCPRODUTOR' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsInscProdutor, @vsSepar )
end

if @vsCNAE is not null  
begin
   set @Coluna =  @Coluna + 'CNAE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCNAE, @vsSepar )
end

if @vdDtanascfund is not null  
   begin
   set @Coluna =  @Coluna + 'DTANASCFUND' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @vdDtanascfund, @vsSepar )
end

if @vsEmail is not null  
begin
   set @Coluna =  @Coluna + 'EMAIL' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsEmail, @vsSepar )
end

if @vsEstadocivil is not null  
begin
   set @Coluna =  @Coluna + 'ESTADOCIVIL' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsEstadocivil, @vsSepar )
end

if @vsAtividade is not null  
begin
   set @Coluna =  @Coluna + 'ATIVIDADE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsAtividade, @vsSepar )
end

if @vsrendafaturamento is not null  
begin
   set @Coluna =  @Coluna + 'RENDAFATURAMENTO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsrendafaturamento, @vsSepar )
end

if @vsgrauinstrucao is not null  
begin
   set @Coluna =  @Coluna + 'GRAUINSTRUCAO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsgrauinstrucao, @vsSepar )
end

if @vnSeqPessoaEndCobr is not null  
begin
   set @Coluna =  @Coluna + 'SEQPESSOAENDCOBR' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnSeqPessoaEndCobr, @vsSepar )
end

if @vsFoneDDD1 is not null  
begin
   set @Coluna =  @Coluna + 'FONEDDD1' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFoneDDD1, @vsSepar )
end

if @vnFonenro1 is not null  
begin
   set @Coluna =  @Coluna + 'FONENRO1' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnFonenro1, @vsSepar )
end

if @vsFonecmpl1 is not null  
begin
   set @Coluna =  @Coluna + 'FONECMPL1' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFonecmpl1, @vsSepar )
end

if @vsFoneDDD2 is not null  
begin
   set @Coluna =  @Coluna + 'FONEDDD2' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFoneDDD2, @vsSepar )
end

if @vnFonenro2 is not null  
begin
   set @Coluna =  @Coluna + 'FONENRO2' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnFonenro2, @vsSepar )
end

if @vsFonecmpl2 is not null  
begin
   set @Coluna =  @Coluna + 'FONECMPL2' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFonecmpl2, @vsSepar )
end

if @vsFoneddd3 is not null  
begin
   set @Coluna =  @Coluna + 'FONEDDD3' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFoneddd3, @vsSepar )
end

if @vnFonenro3 is not null  
begin
   set @Coluna =  @Coluna + 'FONENRO3' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnFonenro3, @vsSepar )
end

if @vsFonecmpl3 is not null  
begin
   set @Coluna =  @Coluna + 'FONECMPL3' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFonecmpl3, @vsSepar )
end

if @vsFaxDDD is not null  
begin
   set @Coluna =  @Coluna + 'FAXDDD' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsFaxDDD, @vsSepar )
end

if @vnFaxNro is not null  
begin
   set @Coluna =  @Coluna + 'FAXNRO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnFaxNro, @vsSepar )
end

if @vsHomePage is not null  
begin
   set @Coluna =  @Coluna + 'HOMEPAGE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsHomePage, @vsSepar )
end

if @vsPorte is not null  
begin
   set @Coluna =  @Coluna + 'PORTE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsPorte, @vsSepar )
end

if @vsCodEquipe is not null  
begin
   set @Coluna =  @Coluna + 'CODEQUIPE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCodEquipe, @vsSepar )
end

if @vnNaoPossuiEmail is not null  
begin
   set @Coluna =  @Coluna + 'NAOPOSSUIEMAIL' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnNaoPossuiEmail, @vsSepar )
end

if @vnProblemaCredito is not null  
begin
   set @Coluna =  @Coluna + 'PROBLEMACREDITO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnProblemaCredito, @vsSepar )
end

if @vsIndContribICMS is not null  
begin
   set @Coluna =  @Coluna + 'INDCONTRIBICMS' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsIndContribICMS, @vsSepar )
end

if @vsRefEndereco is not null  
begin
   set @Coluna =  @Coluna + 'REFENDERECO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsRefEndereco, @vsSepar )
end

if @vnLatitude is not null  
begin
   set @Coluna =  @Coluna + 'LATITUDE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnLatitude, @vsSepar )
end

if @vnLongitude is not null  
begin
   set @Coluna =  @Coluna + 'LONGITUDE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnLongitude, @vsSepar )
end

if @vnSEQPESSOAPRC is not null  
begin
   set @Coluna =  @Coluna + 'SEQPESSOAPRC' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnSEQPESSOAPRC, @vsSepar )
end

if @vsSkype is not null  
begin
   set @Coluna =  @Coluna + 'SKYPE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsSkype, @vsSepar )
end

if @vsSMSCODIGO is not null  
begin
   set @Coluna =  @Coluna + 'SMSCODIGO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsSMSCODIGO, @vsSepar )
end

if @vsSMSCODIGODTA is not null  
begin
   set @Coluna =  @Coluna + 'SMSCODIGONOTA' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsSMSCODIGODTA, @vsSepar )
end

if @vsGrupo is not null  
begin
   set @Coluna =  @Coluna + 'GRUPO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsGrupo, @vsSepar )
end

if @vsPessoalink is not null  
begin
   set @Coluna =  @Coluna + 'PESSOALINK' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsPessoalink, @vsSepar )
end

if @vsPessoalinkorigem is not null  
begin
   set @Coluna =  @Coluna + 'PESSOALINKORIGEM' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsPessoalinkorigem, @vsSepar )
end

if @vsUltOrigem is not null  
begin
   set @Coluna =  @Coluna + 'ULTORIGEM' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsUltOrigem, @vsSepar )
end

if @vnTelefonema is not null 
begin
   set @Coluna =  @Coluna +  'TELEFONEMA' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnTelefonema, @vsSepar )
end

if @vnCorrespondencia is not null  
begin
   set @Coluna =  @Coluna +  'CORRESPONDENCIA' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnCorrespondencia, @vsSepar )
end

if @vnRecebeemail is not null  
begin
   set @Coluna =  @Coluna + 'RECEBEEMAIL' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnRecebeemail, @vsSepar )
end

if @vnRecebesms is not null  
begin
   set @Coluna =  @Coluna + 'RECEBESMS' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnRecebesms, @vsSepar )
end

if @vnSeqpessoa is not null  
begin
   set @Coluna =  @Coluna + 'SEQPESSOA' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnSeqpessoa, @vsSepar )
end

if @vsUsuAlteracao is null  
begin
	set @Coluna =  @Coluna + 'USUINCLUSAO' + @vsSepar
	set @Dado =  @Dado + dbo.fC5_ColString  ( 'VTCCONS', @vsSepar )
	set @Coluna =  @Coluna + 'DTAINCLUSAO' + @vsSepar
	set @Dado =  @Dado + dbo.fC5_ColDate ( GETDATE(), @vsSepar )
end

if @vsUsuAlteracao is not null  
begin
   set @Coluna =  @Coluna + 'USUALTERACAO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsUsuAlteracao, @vsSepar )
   set @Coluna =  @Coluna + 'DTAALTERACAO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( GETDATE(), @vsSepar )
END

  INSERT INTO GEP_IMPORT(
         PROCESSO,
         ORIGEM,
         ACAO,
         TABELA,
         SEPARADOR,
         COLUNA,
         Dado,
         colunaidentific,
         Dadoidentificador,
         Dtageracao,
         prioridade)
  VALUES (
         @Processo,
         @Origem,
         @Acao,
         @Tabela,
         @vsSepar,
         @coluna,
         @dado,
         @Colunaident,
         @Dadoident,
         GETDATE(),
         '0')
end
