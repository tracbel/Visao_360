/* ==============================================================
   Objeto ..........: dbo.PR_ATULINK_PESSOA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2021-08-02 20:41:01
   Modificado em ...: 2021-08-02 20:41:01
   Linhas ..........: 37
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : IMP_LINKPESSOA, ge_pessoalink
   Tabelas referidas: GE_PESSOA, ge_pessoalink, IMP_LINKPESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create PROCEDURE [dbo].[PR_ATULINK_PESSOA]
AS
DECLARE 
           @SeqPessoa numeric(22,0),
           @Seq numeric,
		   @Pessoalink varchar(250),
		   @Origem varchar(20)
		   
DECLARE CURPES CURSOR FOR

SELECT 'Protheus' AS ORIGEM,
       XA.PESSOALINK,
	   PES.SEQPESSOA
FROM IMP_LINKPESSOA XA
JOIN GE_PESSOA PES ON PES.NROCGCCPF = XA.NROCGCCPF

BEGIN 
    
	--update IMP_LINKPESSOA
	--set nrocgccpf = left ( cgccpf, len(cgccpf) - 2 ),
	--digcgccpf = RIGHT ( cgccpf, 2 )

	OPEN CURPES
	FETCH NEXT FROM CURPES INTO @Origem, @Pessoalink, @SeqPessoa
	
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		insert into ge_pessoalink ( pessoalink, origem, seqpessoa, dtageracao )
		values ( @Pessoalink, @Origem, @SeqPessoa, getdate() )
				
	FETCH NEXT FROM CURPES INTO 	
		@Origem, @Pessoalink, @SeqPessoa
	END   	
	CLOSE CURPES
	DEALLOCATE CURPES	 
END
