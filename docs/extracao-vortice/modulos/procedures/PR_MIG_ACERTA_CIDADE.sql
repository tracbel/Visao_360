/* ==============================================================
   Objeto ..........: dbo.PR_MIG_ACERTA_CIDADE
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2017-01-22 22:38:00
   Modificado em ...: 2017-01-22 22:38:00
   Linhas ..........: 50
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : GE_PESSOA
   Tabelas referidas: GE_CIDADE, GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[PR_MIG_ACERTA_CIDADE]
AS

DECLARE @vnSeqpessoa numeric
DECLARE @vnSeqCidade numeric
DECLARE @vsCidade as varchar(100)
DECLARE @vsUF as varchar(2)


BEGIN

DECLARE C01 CURSOR FOR 


SELECT SEQPESSOA, CIDADE, UF
  FROM GE_PESSOA PES
WHERE PES.CIDADE IS NOT NULL
  AND PES.Uf IS NOT NULL
  AND PES.SEQCIDADE IS NULL


  OPEN C01
  
    	FETCH NEXT FROM C01
		INTO 	@vnSeqPessoa,
		        @vsCidade,
				@vsUF
                                
                				
	WHILE @@FETCH_STATUS = 0
	BEGIN 
	
	SELECT @vnSeqCidade = SEQCIDADE FROM GE_CIDADE WHERE CIDADE = @vsCidade AND UF = @vsUF

	If (@vnSeqCidade >0)
	Begin
		UPDATE GE_PESSOA SET SEQCIDADE = @vnSeqCidade WHERE CIDADE = @vsCidade AND UF = @vsUF AND SEQPESSOA = @vnSeqPessoa
	End
	
	FETCH NEXT FROM C01
		INTO 	@vnSeqPessoa,
		        @vsCidade,
				@vsUF
	END                
    CLOSE C01
    DEALLOCATE C01   

     	

END	  