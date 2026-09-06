/* ==============================================================
   Objeto ..........: dbo.PR_COL_ACERTAFONE
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 17:26:03
   Modificado em ...: 2024-12-28 17:39:15
   Linhas ..........: 79
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : ge_pessoafone, ge_pessoa
   Tabelas referidas: GE_PESSOA, GE_PESSOA_BKPJUN, GE_PESSOA_ITA, ge_pessoafone, ge_pessoafone_bkpjun, GE_PESSOAfone_ITA, MIG_GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[PR_COL_ACERTAFONE] as
DECLARE
  @SeqPessoa numeric(10, 0) ,
  @SeqPesFone numeric(18, 0),
  @DDD varchar(5) ,
  @Numero numeric(12, 0) ,
  @IndFonePref numeric(1, 0) ,
  @NroFonePessoa numeric(1, 0) ,
  @Complemento varchar(20),
  @Foni integer,
  @Seqf numeric(1, 0) ,
  @DDDF varchar(5),
  @Rank numeric(10, 0) 


DECLARE CURPES
CURSOR
FOR
      select xa.seqpessoa, xa.seqpesfone, xa.ddd, xa.numero, xa.indfonepref, xa.nrofonepessoa,
       xa.complemento, xa.foni, xa.seqf, xa.dddf, 
     row_number () over ( partition by xa.seqpessoa order by isnull(xa.foni, 9), isnull(xa.seqf, 9), xa.seqpesfone ) as rank
      from (
      select pfon.SEQPESSOA, pfon.SEQPESFONE, pfon.DDD, pfon.NUMERO, pfon.INDFONEPREF, pfon.NROFONEPESSOA, PFON.Complemento,
      (SELECT distinct 1 as item FROM GE_PESSOA_BKPJUN WHERE SEQPESSOA = PES.SEQPESSOA AND FONENRO1 = PFON.Numero and fonenro2 <> pfon.Numero and fonenro3 <> pfon.numero
     union
     SELECT distinct 2 as item FROM GE_PESSOA_BKPJUN WHERE SEQPESSOA =  PES.SEQPESSOA AND FONENRO2 = PFON.Numero and fonenro2 <> pfon.Numero and fonenro3 <> pfon.numero
     union
     SELECT distinct 3 as item FROM GE_PESSOA_BKPJUN WHERE SEQPESSOA =  PES.SEQPESSOA AND FONENRO3 = PFON.Numero and fonenro2 <> pfon.Numero and fonenro3 <> pfon.numero
    )  AS FONI,
      (select max(nrofonepessoa) from ge_pessoafone_bkpjun where seqpessoa = PES.SEQPESSOA and numero = pfon.numero and ddd = pfon.DDD) as SEQF,
      (SELECT max(DDD) FROM GE_PESSOAfone_ITA WHERE seqpessoa =  MIG.SEQPESSOA and numero = pfon.numero AND DDD IS NOT NULL ) AS DDDF
      from MIG_GE_PESSOA MIG
      JOIN GE_PESSOA_ITA PITA ON PITA.SeqPessoa = MIG.SEQPESSOA
      JOIN GE_PESSOA PES ON PES.NROCGCCPF = PITA.NroCGCCPF
      JOIN ge_pessoafone pfon ON PFON.SeqPessoa = PES.SeqPessoa
      where 1=1
---         AND MIG.DESCRICAO =  'Ativo/Prospect CNPJ Existente'
---        and pfon.seqpessoa = 96506
      ) xa

begin
  OPEN curPES
    FETCH NEXT FROM curPES INTO     @SeqPessoa,   @SeqPesFone,   @DDD,   @Numero,   @IndFonePref,   @NroFonePessoa,   @Complemento,
                  @Foni,   @Seqf,   @DDDF, @Rank


  WHILE (@@FETCH_STATUS = 0)
  BEGIN

      if isnull(@dddf, '0' ) <> '0' 
      begin
        update ge_pessoafone set ddd = ISNULL(@dddf, '') where seqpessoa = @seqpessoa and seqpesfone = @seqpesfone
      end

      update ge_pessoafone set nrofonepessoa = @rank where seqpessoa = @seqpessoa and seqpesfone = @seqpesfone

    if @rank=1
    begin
    update ge_pessoa set fonenro1 = @Numero, foneddd1 = ISNULL(@DDD,''), FoneCmpl1=ISNULL(@Complemento,'') where seqpessoa = @seqpessoa
    end
    if @rank=2
    begin
    update ge_pessoa set fonenro2 = @Numero, foneddd2 = ISNULL(@DDD,''), FoneCmpl2=ISNULL(@Complemento,'') where seqpessoa = @seqpessoa
    end
    if @rank=3
    begin
    update ge_pessoa set fonenro3 = @Numero, foneddd3 = ISNULL(@DDD,''), FoneCmpl3=ISNULL(@Complemento,'') where seqpessoa = @seqpessoa
    end


    FETCH NEXT FROM curPES INTO   @SeqPessoa,   @SeqPesFone,   @DDD,   @Numero,   @IndFonePref,   @NroFonePessoa,   @Complemento,
                  @Foni,   @Seqf,   @DDDF, @Rank
  END

  CLOSE curPES
  DEALLOCATE curPES
  
end
