/* ==============================================================
   Objeto ..........: dbo.PR_IV_COBERTURA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2015-03-04 12:20:45
   Modificado em ...: 2019-04-03 12:38:24
   Linhas ..........: 69
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : IVS_PES
   Tabelas referidas: IV_HISTORICO, IVS_DEPTORES, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE PROCEDURE [dbo].[PR_IV_COBERTURA]
AS

DECLARE @vnSeqpessoa numeric
DECLARE @vdMaxCtto datetime
DECLARE @vdMaxVisita datetime
DECLARE @vdDtaUltCtto datetime
DECLARE @vdDtaUltCttoExt datetime

BEGIN

DECLARE C01 CURSOR FOR 


SELECT HIST.SEQPESSOA
   , MAX(CASE WHEN DPRES.INDCONTATO = 1 THEN HIST.DTAREALIZACAO ELSE NULL END) AS MAXCTTO 
   , MAX(PESC.DTAULTCTTO) AS DTAULTCTTO
   , MAX(CASE WHEN DPRES.INDEXTERNO = 1 THEN HIST.DTAREALIZACAO WHEN DPRES.INDEXTFORMACTTO = 1 AND HIST.FORMAPRIMCONT = 'Externo' THEN HIST.DTAREALIZACAO ELSE NULL END) AS MAXEXT   
   , MAX(PESC.DTAULTCTTOEXT) AS DTAULTCTTOEXT   
  FROM IV_HISTORICO HIST
	JOIN IVS_DEPTORES DPRES 
	    ON DPRES.RESULTADO = HIST.RESULTADO 
	   AND DPRES.SEQDEPTO = 2
	JOIN IVS_PES PESC 
	   ON PESC.SEQPESSOA = HIST.SEQPESSOA 
	  AND PESC.SEQDEPTO = 2
WHERE 1 = 1
  AND HIST.DTAREALIZACAO >= Cast('2018/01/01' as datetime) --GETDATE() - 365
  AND (( DPRES.INDCONTATO >0 OR DPRES.INDEXTERNO > 0 ) OR ( DPRES.INDEXTFORMACTTO > 0 AND HIST.FORMAPRIMCONT = 'Externo' ))
  --AND HIST.SEQPESSOA = 2993
GROUP BY HIST.SEQPESSOA
ORDER BY HIST.SEQPESSOA 


  OPEN C01
  
    	FETCH NEXT FROM C01
		INTO 	@vnSeqpessoa,
				@vdMaxCtto,
				@vdDtaUltCtto,
				@vdMaxVisita,
				@vdDtaUltCttoExt
                                
                				
	WHILE @@FETCH_STATUS = 0
	BEGIN 
	
	IF @vdMaxCtto > ISNULL(@vdDtaUltCtto, GETDATE() - 999 )
	BEGIN
		UPDATE IVS_PES SET DTAULTCTTO = @vdMaxCtto WHERE SEQPESSOA = @vnSeqpessoa
	END
	
	IF @vdMaxVisita > ISNULL(@vdDtaUltCttoExt, GETDATE() - 999 )
	BEGIN
		UPDATE IVS_PES SET DTAULTCTTOEXT = @vdMaxVisita WHERE SEQPESSOA = @vnSeqpessoa		
	END
	
	FETCH NEXT FROM C01
		INTO 	@vnSeqpessoa,
				@vdMaxCtto,
				@vdDtaUltCtto,
				@vdMaxVisita,
				@vdDtaUltCttoExt 	
	END                
    CLOSE C01
    DEALLOCATE C01    	

END	  