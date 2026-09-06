/* ==============================================================
   Objeto ..........: dbo.PR_ACERTA_CARTEIRA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2017-12-08 11:00:50
   Modificado em ...: 2017-12-08 11:00:50
   Linhas ..........: 83
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : IVS_Pes
   Tabelas referidas: GE_PESSOA, IVS_Carteira, IVS_Depto, IVS_Pes
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE PROCEDURE [dbo].[PR_ACERTA_CARTEIRA]
AS

DECLARE @vnSeqpessoa numeric
DECLARE @vnSeqCarteira numeric
DECLARE @vnSeqCarteiraTRE numeric

BEGIN

DECLARE C01 CURSOR FOR 

SELECT  RPES.SeqPessoa, RPES.SeqCarteira, TPES.SeqCarteira
FROM IVS_Pes RPES
JOIN IVS_Depto RDEP
ON RDEP.SeqDepto = RPES.SeqDepto
JOIN IVS_Carteira RCART
ON RCART.SeqCarteira = RPES.SeqCarteira
JOIN CRM_setembro.dbo.IVS_Pes TPES
ON TPES.SeqPessoa = RPES.SeqPessoa
   AND TPES.SeqCarteira <> RPES.SeqCarteira
JOIN CRM_RESTORE.dbo.IVS_Depto TDEP
ON TDEP.SeqDepto = TPES.SeqDepto
JOIN IVS_Carteira TCART
ON TCART.SeqCarteira = TPES.SeqCarteira
JOIN GE_PESSOA PES
ON PES.SeqPessoa = RPES.SeqPessoa
WHERE RPES.SeqDepto = 2
  AND TPES.SeqDepto = 2

  OPEN C01
  
    	FETCH NEXT FROM C01
		INTO 	@vnSeqPessoa,
				@vnSeqCarteira,
		        @vnSeqCarteiraTRE
                				
	WHILE @@FETCH_STATUS = 0
	BEGIN 
	
	SELECT  @vnSeqPessoa = RPES.SeqPessoa, @vnSeqCarteiraTRE = RPES.SeqCarteira, @vnSeqCarteiraTRE = TPES.SeqCarteira
	FROM IVS_Pes RPES
	JOIN IVS_Depto RDEP
	ON RDEP.SeqDepto = RPES.SeqDepto
	JOIN IVS_Carteira RCART
	ON RCART.SeqCarteira = RPES.SeqCarteira
	JOIN CRM_setembro.dbo.IVS_Pes TPES
	ON TPES.SeqPessoa = RPES.SeqPessoa
		AND TPES.SeqCarteira <> RPES.SeqCarteira
	JOIN CRM_RESTORE.dbo.IVS_Depto TDEP
	ON TDEP.SeqDepto = TPES.SeqDepto
	JOIN IVS_Carteira TCART
	ON TCART.SeqCarteira = TPES.SeqCarteira
	JOIN GE_PESSOA PES
	ON PES.SeqPessoa = RPES.SeqPessoa
	WHERE RPES.SeqDepto = 2
	  AND TPES.SeqDepto = 2
	  AND RPES.SeqPessoa = 45135

		If (@vnSeqPessoa > 0)
		Begin
			UPDATE IVS_Pes 
			SET SeqCarteira = @vnSeqCarteiraTRE 
			WHERE SEQPESSOA = @vnSeqPessoa 
			AND SeqCarteira = @vnSeqCarteira 
			AND SeqDepto = 2
		End
	
	FETCH NEXT FROM C01
		INTO 	@vnSeqPessoa,
				@vnSeqCarteira,
		        @vnSeqCarteiraTRE
	END                
    CLOSE C01
    DEALLOCATE C01    	

END	  





