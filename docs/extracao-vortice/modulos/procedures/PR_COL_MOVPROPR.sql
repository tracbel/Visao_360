/* ==============================================================
   Objeto ..........: dbo.PR_COL_MOVPROPR
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 17:40:38
   Modificado em ...: 2024-12-28 17:40:38
   Linhas ..........: 429
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : ge_sequencia, GEP_IMPORT
   Tabelas referidas: GEP_IMPORT
   Outras refs .....: fC5_ColDate, fC5_ColNumber, fC5_ColString
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create procedure [dbo].[PR_COL_MOVPROPR] 
	@VNSEQPROPPESSOA       NUMERIC(18, 0),
	@VNSEQPESSOA           NUMERIC(10, 0),
	@VNSEQPROPRIEDADE      NUMERIC(4, 0),
	@VSREFERENCIA          VARCHAR(30),
	@VSIDENTIFICADOR       VARCHAR(30),
	@VSATIVO               CHAR(1),
	@VSNOTAS               VARCHAR(250),
	@VSCAMPO1              VARCHAR(40),
	@VSCAMPO2              VARCHAR(40),
	@VSCAMPO3              VARCHAR(40),
	@VSCAMPO4              VARCHAR(40),
	@VSCAMPO5              VARCHAR(40),
	@VSCAMPO6              VARCHAR(40),
	@VNNUMERO1             DECIMAL(15, 2),
	@VNNUMERO2             DECIMAL(15, 2),
	@VNNUMERO3             DECIMAL(15, 2),
	@VNNUMERO4             DECIMAL(15, 2),
	@VNNUMERO5             DECIMAL(15, 2),
	@VNNUMERO6             DECIMAL(15, 2),
	@VDDATA1               DATETIME,
	@VDDATA2               DATETIME,
	@VDDATA3               DATETIME,
	@VDDATA4               DATETIME,
	@VDDATA5               DATETIME,
	@VDDATA6               DATETIME,
	@VNSIMNAO1             NUMERIC(1, 0),
	@VNSIMNAO2             NUMERIC(1, 0),
	@VNSIMNAO3             NUMERIC(1, 0),
	@VNSIMNAO4             NUMERIC(1, 0),
	@VNSIMNAO5             NUMERIC(1, 0),
	@VNSIMNAO6             NUMERIC(1, 0),
	@VSLITERAL1            VARCHAR(40),
	@VSLITERAL2            VARCHAR(40),
	@VSLITERAL3            VARCHAR(40),
	@VSLITERAL4            VARCHAR(40),
	@VSLITERAL5            VARCHAR(40),
	@VSLITERAL6            VARCHAR(40),
	@VSLITERAL7            VARCHAR(40),
	@VSLITERAL8            VARCHAR(40),
	@VSLITERAL9            VARCHAR(40),
	@VSLITERAL10           VARCHAR(40),
	@VSCODORIGEM           VARCHAR(20),
	@VSULTORIGEM           VARCHAR(20),
	@VDDTAINCLUSAO         DATETIME,
	@VSUSUINCLUSAO         VARCHAR(20),
	@VDDTAALTERACAO        DATETIME,
	@VSUSUALTERACAO        VARCHAR(20),
	@VSCAMPO7              VARCHAR(40),
	@VSCAMPO8              VARCHAR(40),
	@VDCAMPO7SQL           NUMERIC(1, 0),
	@VDCAMPO8SQL           NUMERIC(1, 0),
	@VSORIGEM			   VARCHAR(20),
	@VSSTATUS              VARCHAR(1)


as

       Declare @vnCount                                              integer
       Declare @vnSeq                                                integer
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
	SET @VSSTATUS = ''

---- Definindo os dados basicos da tabela destino.

   set @Processo = 'VORTICOCRM'
   set @Origem  = @vsOrigem 
   set @Banco = null
   set @Dono = null
   set @Tabela = 'IV_CLIENTEPROPR'
   set @Acao = 'I'
   set @Dtageracao = getdate()
   set @Coluna  = 'REFERENCIA' + @vsSepar
   set @Dado = @VSREFERENCIA

if isnull(@vnSeqpessoa, 0)<>0  
begin
   set @Colunaident = 'SEQPESSOA'
   set @Dadoident = cast(@vnSeqpessoa as varchar)
end

   --set @vnSeq = (SELECT isnull(SEQUENCIA, 0) FROM ge_sequencia where UPPER(nometabela) = 'IV_CLIENTEPROPR')
   --SET @vnSeq = @vnSeq + 1
   --update ge_sequencia set sequencia = @vnSeq where UPPER(nometabela) = 'IV_CLIENTEPROPR'
   --set @Coluna =  @Coluna + 'SEQPROPPESSOA' + @vsSepar
   --set @Dado =  @Dado + dbo.fC5_ColNumber( @vnSeq, @vsSepar )

if @VNSEQPROPRIEDADE <> 0
begin
   set @Coluna =  @Coluna + 'SEQPROPRIEDADE' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNSEQPROPRIEDADE, @vsSepar )
end


if @VSIDENTIFICADOR is not null  
begin
   set @Coluna =  @Coluna + 'IDENTIFICADOR' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSIDENTIFICADOR, @vsSepar )
end 

if @VSATIVO is not null  
begin
   set @Coluna =  @Coluna + 'ATIVO' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSATIVO, @vsSepar )
end 

if @VSNOTAS is not null  
begin
   set @Coluna =  @Coluna + 'NOTAS' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSNOTAS, @vsSepar )
end 

if @VSCAMPO1 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO1' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO1, @vsSepar )
end 

if @VSCAMPO2 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO2' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO2, @vsSepar )
end 

if @VSCAMPO3 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO3' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO3, @vsSepar )
end 

if @VSCAMPO4 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO4' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO4, @vsSepar )
end 

if @VSCAMPO5 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO5' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO5, @vsSepar )
end 

if @VSCAMPO6 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO6' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO6, @vsSepar )
end 

if @VNNUMERO1 <> 0
begin
   set @Coluna =  @Coluna + 'NUMERO1' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNNUMERO1, @vsSepar )
end

if @VNNUMERO2 <> 0
begin
   set @Coluna =  @Coluna + 'NUMERO2' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNNUMERO2, @vsSepar )
end

if @VNNUMERO3 <> 0
begin
   set @Coluna =  @Coluna + 'NUMERO3' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNNUMERO3, @vsSepar )
end

if @VNNUMERO4 <> 0
begin
   set @Coluna =  @Coluna + 'NUMERO4' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNNUMERO4, @vsSepar )
end

if @VNNUMERO5 <> 0
begin
   set @Coluna =  @Coluna + 'NUMERO5' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNNUMERO5, @vsSepar )
end

if @VNNUMERO6 <> 0
begin
   set @Coluna =  @Coluna + 'NUMERO6' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNNUMERO6, @vsSepar )
end

if @VDDATA1 is not null  
   begin
   set @Coluna =  @Coluna + 'DATA1' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDATA1, @vsSepar )
end

if @VDDATA2 is not null  
   begin
   set @Coluna =  @Coluna + 'DATA2' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDATA2, @vsSepar )
end

if @VDDATA3 is not null  
   begin
   set @Coluna =  @Coluna + 'DATA3' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDATA3, @vsSepar )
end

if @VDDATA4 is not null  
   begin
   set @Coluna =  @Coluna + 'DATA4' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDATA4, @vsSepar )
end

if @VDDATA5 is not null  
   begin
   set @Coluna =  @Coluna + 'DATA5' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDATA5, @vsSepar )
end

if @VDDATA6 is not null  
   begin
   set @Coluna =  @Coluna + 'DATA6' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDATA6, @vsSepar )
end

if @VNSIMNAO1 <> 0
begin
   set @Coluna =  @Coluna + 'SIMNAO1' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNSIMNAO1, @vsSepar )
end

if @VNSIMNAO2 <> 0
begin
   set @Coluna =  @Coluna + 'SIMNAO2' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNSIMNAO2, @vsSepar )
end

if @VNSIMNAO3 <> 0
begin
   set @Coluna =  @Coluna + 'SIMNAO3' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNSIMNAO3, @vsSepar )
end

if @VNSIMNAO4 <> 0
begin
   set @Coluna =  @Coluna + 'SIMNAO4' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNSIMNAO4, @vsSepar )
end

if @VNSIMNAO5 <> 0
begin
   set @Coluna =  @Coluna + 'SIMNAO5' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNSIMNAO5, @vsSepar )
end

if @VNSIMNAO6 <> 0
begin
   set @Coluna =  @Coluna + 'SIMNAO6' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VNSIMNAO6, @vsSepar )
end

if @VSLITERAL1 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL1' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL1, @vsSepar )
end 

if @VSLITERAL2 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL2' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL2, @vsSepar )
end 

if @VSLITERAL3 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL3' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL3, @vsSepar )
end 

if @VSLITERAL4 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL4' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL4, @vsSepar )
end 

if @VSLITERAL5 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL5' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL5, @vsSepar )
end 

if @VSLITERAL6 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL6' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL6, @vsSepar )
end 

if @VSLITERAL7 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL7' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL7, @vsSepar )
end 

if @VSLITERAL8 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL8' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL8, @vsSepar )
end 

if @VSLITERAL9 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL9' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL9, @vsSepar )
end 

if @VSLITERAL10 is not null  
begin
   set @Coluna =  @Coluna + 'LITERAL10' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSLITERAL10, @vsSepar )
end 

if @VSCODORIGEM is not null  
begin
   set @Coluna =  @Coluna + 'CODORIGEM' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCODORIGEM, @vsSepar )
end 

if @VSULTORIGEM is not null  
begin
   set @Coluna =  @Coluna + 'ULTORIGEM' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSULTORIGEM, @vsSepar )
end 

if @VDDTAINCLUSAO is not null  
   begin
   set @Coluna =  @Coluna + 'DTAINCLUSAO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDTAINCLUSAO, @vsSepar )
end

if @VSUSUINCLUSAO is not null  
begin
   set @Coluna =  @Coluna + 'USUINCLUSAO' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSUSUINCLUSAO, @vsSepar )
end 

if @VDDTAALTERACAO is not null  
   begin
   set @Coluna =  @Coluna + 'DTAALTERACAO' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColDate ( @VDDTAALTERACAO, @vsSepar )
end

if @VSUSUALTERACAO is not null  
begin
   set @Coluna =  @Coluna + 'USUALTERACAO' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSUSUALTERACAO, @vsSepar )
end 

if @VSCAMPO7 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO7' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO7, @vsSepar )
end 

if @VSCAMPO8 is not null  
begin
   set @Coluna =  @Coluna + 'CAMPO8' + @vsSepar
   set @Dado = @Dado + dbo.fC5_ColString  ( @VSCAMPO8, @vsSepar )
end 

if @VDCAMPO7SQL <> 0
begin
   set @Coluna =  @Coluna + 'CAMPO7SQL' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VDCAMPO7SQL, @vsSepar )
end

if @VDCAMPO8SQL <> 0
begin
   set @Coluna =  @Coluna + 'CAMPO8SQL' + @vsSepar
   set @Dado =  @Dado + dbo.fC5_ColNumber( @VDCAMPO8SQL, @vsSepar )
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
         'I',
         @Tabela,
         @vsSepar,
         @coluna,
         @dado,
         @Colunaident,
         @Dadoident,
         GETDATE(),
         '0')
end
