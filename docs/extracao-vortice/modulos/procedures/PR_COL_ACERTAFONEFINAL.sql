/* ==============================================================
   Objeto ..........: dbo.PR_COL_ACERTAFONEFINAL
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-30 16:32:36
   Modificado em ...: 2024-12-30 16:38:29
   Linhas ..........: 89
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : ge_sequencia, GE_PessoaFone
   Tabelas referidas: GE_PESSOA, GE_PESSOA_ITA, GE_PESSOAFONE, GE_PESSOAFONE_ITA, ge_sequencia, MIG_GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[PR_COL_ACERTAFONEFINAL] as
DECLARE
  @SeqPesFone        numeric(18, 0),
  @TipoFoneSeqPar        numeric(18, 0),
  @SeqPessoa         numeric(10, 0),
  @DDD           varchar(5),
  @Numero          numeric(12, 0),
  @Complemento         varchar(20),
  @Obs           varchar(30),
  @IndFonePref         numeric(1, 0),
  @NroFonePessoa         numeric(1, 0),
  @DTAULTSUCESSO         datetime,
  @DTAULTINSUCESSO       datetime,
  @INDEMUSO        numeric(1, 0),
  @MOTIVOEMUSO         varchar(30),
  @DTAALTERACAO        datetime,
  @USUALTERACAO        varchar(20),
  @INDUSOMKT         numeric(1, 0),
  @DTAINCLUSAO         datetime,
  @USUINCLUSAO         varchar(20),
  @INDWHATSAPP         numeric(1, 0),
  @Ultseq             numeric(10,0),
  @Rank             numeric(10,0)

DECLARE CURPES
CURSOR
FOR
      SELECT  FITA.TipoFoneSeqPar, PES.SEQPESSOA, FITA.DDD, FITA.NUMERO, FITA.Complemento, FITA.OBS,  
              row_number () over ( partition by PES.seqpessoa order by FITA.NROFONEPESSOA, FITA.SEQPESFONE )  AS  RANK,
              FITA.DTAULTSUCESSO, FITA.DTAULTINSUCESSO, FITA.INDEMUSO, FITA.MOTIVOEMUSO, FITA.DTAALTERACAO, FITA.USUALTERACAO, FITA.INDUSOMKT, 
              FITA.DTAINCLUSAO, FITA.USUINCLUSAO, FITA.INDWHATSAPP,
              (SELECT MAX(NROFONEPESSOA) FROM GE_PESSOAFONE WHERE SEQPESSOA = PES.SEQPESSOA) AS ULTSEQ
      from MIG_GE_PESSOA MIG
      JOIN GE_PESSOA_ITA PITA ON PITA.SeqPessoa = MIG.SEQPESSOA
      JOIN GE_PESSOAFONE_ITA FITA ON FITA.SEQPESSOA = PITA.SEQPESSOA AND FITA.INDEMUSO=1
      JOIN GE_PESSOA PES ON PES.NROCGCCPF = PITA.NroCGCCPF
      WHERE  FITA.NUMERO NOT IN (SELECT NUMERO FROM GE_PESSOAFONE WHERE SEQPESSOA = PES.SEQPESSOA )
---        AND PES.SEQPESSOA = 1139

begin
  OPEN curPES
    FETCH NEXT FROM curPES INTO     @TipoFoneSeqPar, @SEQPESSOA, @DDD, @NUMERO, @Complemento, @OBS, @RANK,
              @DTAULTSUCESSO, @DTAULTINSUCESSO, @INDEMUSO, @MOTIVOEMUSO, @DTAALTERACAO, @USUALTERACAO, @INDUSOMKT, 
              @DTAINCLUSAO, @USUINCLUSAO, @INDWHATSAPP, @ULTSEQ

  WHILE (@@FETCH_STATUS = 0)
  BEGIN
            select @SeqPesFone = max(seqpesfone)  from GE_PESSOAFONE
            set @SeqPesFone = @SeqPesFone + 1
            update ge_sequencia set sequencia = @SeqPesFone where nometabela = 'GE_PESSOAFONE'

            set @NroFonePessoa = @Ultseq + @Rank

            INSERT INTO GE_PessoaFone (SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Complemento, Obs, 
            IndFonePref, NroFonePessoa, DTAULTSUCESSO, DTAULTINSUCESSO, INDEMUSO, MOTIVOEMUSO, DTAALTERACAO, 
            USUALTERACAO, INDUSOMKT, DTAINCLUSAO, USUINCLUSAO, INDWHATSAPP)
            values (   @SeqPesFone      ,
                      @TipoFoneSeqPar  ,
                      @SeqPessoa       ,
                      @DDD           ,
                      @Numero          ,
                      @Complemento     ,
                      @Obs           ,
                      @IndFonePref     ,
                      @NroFonePessoa   ,
                      @DTAULTSUCESSO   ,
                      @DTAULTINSUCESSO ,
                      @INDEMUSO        ,
                      @MOTIVOEMUSO     ,
                      @DTAALTERACAO    ,
                      @USUALTERACAO    ,
                      @INDUSOMKT       ,
                      @DTAINCLUSAO     ,
                      @USUINCLUSAO     ,
                      @INDWHATSAPP     
              )



    FETCH NEXT FROM curPES INTO     @TipoFoneSeqPar, @SEQPESSOA, @DDD, @NUMERO, @Complemento, @OBS, @RANK,
              @DTAULTSUCESSO, @DTAULTINSUCESSO, @INDEMUSO, @MOTIVOEMUSO, @DTAALTERACAO, @USUALTERACAO, @INDUSOMKT, 
              @DTAINCLUSAO, @USUINCLUSAO, @INDWHATSAPP, @ULTSEQ
  END

  CLOSE curPES
  DEALLOCATE curPES
  
end
