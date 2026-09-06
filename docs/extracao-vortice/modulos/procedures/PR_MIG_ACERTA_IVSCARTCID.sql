/* ==============================================================
   Objeto ..........: dbo.PR_MIG_ACERTA_IVSCARTCID
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2017-01-22 22:45:26
   Modificado em ...: 2017-01-22 22:45:26
   Linhas ..........: 46
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : IVS_CARTCID
   Tabelas referidas: GE_CIDADE, GE_CIDADE_CRM, IVS_CARTCID
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE PROCEDURE [dbo].[PR_MIG_ACERTA_IVSCARTCID]
AS

DECLARE @vnSeqCidade numeric
DECLARE @vnSeqCidadeNew numeric


BEGIN

DECLARE C01 CURSOR FOR 


SELECT DISTINCT CID.SeqCidade
  FROM IVS_CARTCID CCID
  JOIN GE_CIDADE_CRM CID ON CID.SEQCIDADE = CCID.SEQCIDADE


  OPEN C01
  
    	FETCH NEXT FROM C01
		INTO 	@vnSeqCidade
                                
                				
	WHILE @@FETCH_STATUS = 0
	BEGIN 
	
	SELECT @vnSeqCidadeNew = CID.SEQCIDADE 
	  FROM GE_CIDADE_CRM C
	     JOIN GE_CIDADE CID 
	        ON CID.CIDADE = C.CIDADE
	       AND CID.UF = C.UF
	WHERE C.SEQCIDADE = @vnSeqCidade

	If (@vnSeqCidadeNew >0)
	Begin
		UPDATE IVS_CARTCID SET SEQCIDADE = @vnSeqCidadeNew WHERE SEQCIDADE = @vnSeqCidade
	End
	
	FETCH NEXT FROM C01
		INTO 	@vnSeqCidade
	END                
    CLOSE C01
    DEALLOCATE C01    	

END	  