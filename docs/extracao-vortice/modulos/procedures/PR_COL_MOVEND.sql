/* ==============================================================
   Objeto ..........: dbo.PR_COL_MOVEND
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 16:34:39
   Modificado em ...: 2024-12-28 16:34:39
   Linhas ..........: 273
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : ge_cidade, GEP_IMPORT
   Tabelas referidas: GE_CIDADE, GEP_IMPORT
   Outras refs .....: fC5_ColDate, fC5_ColNumber, fC5_ColString
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MOVEND] 
	@VNSeqPessoa 	numeric(10, 0)  ,
	@VNSeqPessoaEnd 	decimal(3, 0)  ,
	@VNSeqCidade 	decimal(6, 0) ,
	@VSTipoEndereco 	char(1)  ,
	@VSCidade 	varchar(50) ,
	@VSUf 		varchar(2) ,
	@VNSeqBairro 	decimal(5, 0) ,
	@VSBairro 	varchar(50) ,
	@VSTipoLogradouro varchar(15) ,
	@VSLogradouro 	varchar(80) ,
	@VSNroLogradouro 	varchar(10) ,
	@VSCmpltoLogradouro varchar(30) ,
	@VSCxPostal 	varchar(7) ,
	@VNSeqPessoaEndCobr decimal(3, 0) ,
	@VSCep 		varchar(12) ,
	@VSPais 		varchar(25) ,
	@VDDtaAlteracao 	datetime  ,
	@VSUsuAlteracao 	varchar(20)  ,
	@VSRefEndereco 	varchar(150) ,
	@VNLatitude 	decimal(14, 11) ,
	@VNLongitude 	decimal(14, 11) ,
	@VSDescricao 	varchar(100) ,
	@VNSeqRegiao 	decimal(6, 0) ,
	@VNSeqRota 	decimal(6, 0) ,
	@VSCHAVEADICIONAL varchar(30) ,
	@VSINSCPRODUTOR 	varchar(20),
	@VSORIGEM   VARCHAR(20)

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
   set @Tabela = 'GE_PESSOAEND'
   set @Dtageracao = getdate()

   set @Coluna =  'ORIGEM' + @vsSepar
   set @Dado =  @vsOrigem 

if isnull(@vnSeqpessoa, 0)<>0  
begin
   set @Colunaident = 'SEQPESSOA'
   set @Dadoident = cast(@vnSeqpessoa as varchar)
   set @Acao = 'A'
end

if @vdDtaAlteracao is not null  
   begin
   set @Coluna =  @Coluna + 'DtaAlteracao' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @vdDtaAlteracao, @vsSepar )
end

if @vsUsuAlteracao is not null  
begin
   set @Coluna =  @Coluna + 'UsuAlteracao' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsUsuAlteracao, @vsSepar )
end

if @vsRefEndereco is not null  
begin
   set @Coluna =  @Coluna + 'RefEndereco' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsRefEndereco, @vsSepar )
end

---if @vnSeqPessoaEnd <> 0
---begin
   set @Coluna =  @Coluna + 'SeqPessoaEnd' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeqPessoaEnd, @vsSepar )
---end

set @vnSeqcid=0
if @vsCidade is not null and @VSUf is not null
begin
   set @vnSeqcid = (SELECT isnull(max(SEQCIDADE), 0) FROM GE_CIDADE WHERE CIDADE = @vsCidade and UF = @vsUF)
   if isnull(@vnSeqCid, 0)=0  
   begin
      set @vnSeqcid = (SELECT isnull(max(SEQCIDADE), 0) FROM GE_CIDADE)
      set @vnSeqcid = isnull(@vnSeqcid, 0) + 1
      insert into ge_cidade ( SEQCIDADE, CIDADE, UF, EXGBAIRRO, EXGLOGRADOURO, DTAALTERACAO, USUALTERACAO )
      values ( @vnSeqcid , @vsCidade, @vsUF, 'N', 'N', GETDATE(), 'Import' )
   end
end

   set @Coluna =  @Coluna + 'SeqCidade' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeqcid, @vsSepar )

--if @vnSeqBairro <> 0
--begin
--   set @Coluna =  @Coluna + 'SeqBairro' + @vsSepar
--   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeqBairro, @vsSepar )
--end

if @vnSeqPessoaEndCobr <> 0
begin
   set @Coluna =  @Coluna + 'SeqPessoaEndCobr' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeqPessoaEndCobr, @vsSepar )
end

if @vsCep is not null  
begin
   set @Coluna =  @Coluna + 'Cep' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCep, @vsSepar )
end

if @vsPais is not null  
begin
   set @Coluna =  @Coluna + 'Pais' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsPais, @vsSepar )
end

if @vsTipoEndereco is not null  
begin
   set @Coluna =  @Coluna + 'TipoEndereco' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsTipoEndereco, @vsSepar )
end

if @vsCidade is not null  
begin
   set @Coluna =  @Coluna + 'Cidade' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCidade, @vsSepar )
end

if @vsUf is not null  
begin
   set @Coluna =  @Coluna + 'Uf' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsUf, @vsSepar )
end

if @vsBairro is not null  
begin
   set @Coluna =  @Coluna + 'Bairro' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsBairro, @vsSepar )
end

if @vsTipoLogradouro is not null  
begin
   set @Coluna =  @Coluna + 'TipoLogradouro' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsTipoLogradouro, @vsSepar )
end

if @vsLogradouro is not null  
begin
   set @Coluna =  @Coluna + 'Logradouro' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsLogradouro, @vsSepar )
end

if @vsNroLogradouro is not null  
begin
   set @Coluna =  @Coluna + 'NroLogradouro' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsNroLogradouro, @vsSepar )
end

if @vsCmpltoLogradouro is not null  
begin
   set @Coluna =  @Coluna + 'CmpltoLogradouro' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCmpltoLogradouro, @vsSepar )
end

if @vsCxPostal is not null  
begin
   set @Coluna =  @Coluna + 'CxPostal' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCxPostal, @vsSepar )
end


if @vnLongitude <> 0
begin
   set @Coluna =  @Coluna + 'Longitude' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnLongitude, @vsSepar )
end

if @vnLatitude <> 0
begin
   set @Coluna =  @Coluna + 'Latitude' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnLatitude, @vsSepar )
end

if @vsDescricao is not null  
begin
   set @Coluna =  @Coluna + 'Descricao' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsDescricao, @vsSepar )
end

if @vnSeqRegiao <> 0
begin
   set @Coluna =  @Coluna + 'SeqRegiao' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeqRegiao, @vsSepar )
end

if @vnSeqRota <> 0
begin
   set @Coluna =  @Coluna + 'SeqRota' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeqRota, @vsSepar )
end

if @vsCHAVEADICIONAL is not null  
begin
   set @Coluna =  @Coluna + 'CHAVEADICIONAL' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsCHAVEADICIONAL, @vsSepar )
end

if @vsINSCPRODUTOR is not null  
begin
   set @Coluna =  @Coluna + 'INSCPRODUTOR' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColString  ( @vsINSCPRODUTOR, @vsSepar )
end

if @vnSeqpessoa is not null  
begin
   set @Coluna =  @Coluna + 'SEQPESSOA' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber  ( @vnSeqpessoa, @vsSepar )
end


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
