/* ==============================================================
   Objeto ..........: dbo.VTC_P_INSERE_EMAIL_CART
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2018-02-05 15:20:48
   Modificado em ...: 2018-02-05 16:18:13
   Linhas ..........: 155
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : GE_Email, GE_Sequencia
   Tabelas referidas: GE_Email, GE_IMPORTA_CART, GE_Pessoa, GE_Sequencia
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[VTC_P_INSERE_EMAIL_CART]
AS

DECLARE @vnSeqpessoa    as numeric
DECLARE @vsEmail1       as varchar(70)
DECLARE @vsEmail2       as varchar(70)
DECLARE @vsEmail3       as varchar(70)
DECLARE @vnExisteEml    as numeric
DECLARE @vnExistePes    as numeric

BEGIN

DECLARE C01 CURSOR FOR 


SELECT  CART.SeqPessoa, 
		CART.Email1,
		CART.Email2,
		CART.Email3,
		0,
		0
FROM GE_IMPORTA_CART CART
WHERE EXISTS (SELECT 1 
			  FROM GE_Pessoa PES
			  WHERE PES.SeqPessoa = CART.SeqPessoa)
  --AND CART.SeqPessoa IN (258, 2925)


  OPEN C01
  
    	FETCH NEXT FROM C01
		INTO 	@vnSeqpessoa,
                @vsEmail1,                 
                @vsEmail2, 				
				@vsEmail3,      
				@vnExisteEml,
				@vnExistePes

	WHILE @@FETCH_STATUS = 0
	BEGIN 
	
--	SELECT  @vnSeqPessoa = CART.SeqPessoa, 
--			@vsFoneDDD1  = CART.FoneDDD1,
--			@vnFoneNro1  = CART.FoneNro1,
--			@vnTipo1     = CART.Tipo1,
--			@vsFoneDDD2  = CART.FoneDDD2,
--			@vnFoneNro2  = CART.FoneNro2,
--			@vnTipo2     = CART.Tipo2,
--			@vsFoneDDD3  = CART.FoneDDD3,
--			@vnFoneNro3  = CART.FoneNro3,
--			@vnTipo3     = CART.Tipo3,
--			@vsFoneDDD4  = CART.FoneDDD4,
--			@vnFoneNro4  = CART.FoneNro4,
--			@vnTipo4     = CART.Tipo4,
--			@vsFoneDDD5  = CART.FoneDDD5,
--			@vnFoneNro5  = CART.FoneNro5,
--			@vnTipo5     = CART.Tipo5,
--			@vsFoneDDD6  = CART.FoneDDD6,
--			@vnFoneNro6  = CART.FoneNro6,
--			@vnTipo6     = CART.Tipo6,
--			@vsFoneDDD7  = CART.FoneDDD7,
--			@vnFoneNro7  = CART.FoneNro7,
--			@vnTipo7     = CART.Tipo7,
--			@vsFoneDDD8  = CART.FoneDDD8,
--			@vnFoneNro8  = CART.FoneNro8,
--			@vnTipo8     = CART.Tipo8,
--			@vnExiste    = 0
--FROM GE_IMPORTA_CART CART
--WHERE EXISTS (SELECT 1 
--			  FROM GE_Pessoa PES
--			  WHERE PES.SeqPessoa = CART.SeqPessoa)
--  AND CART.SeqPessoa = 71371

	If (LEN(@vsEmail1) > 0)
	Begin
	  select @vnExisteEml = COUNT(*)
	  FROM GE_Email
	  WHERE eMail = @vsEmail1
	    AND SeqPessoa = @vnSeqpessoa
		
	  If (@vnExisteEml = 0)
		Begin
			INSERT INTO GE_Email
			(SeqEmail, eMail, SeqPessoa, IndEmUso, DtaAlteracao, UsuAlterou, Obs)
			VALUES
			((SELECT MAX(SEQEMAIL) + 1 FROM GE_Email),
			@vsEmail1, @vnSeqpessoa, 1, GETDATE(), 'VTCCONS', '1')  
	    End

	  SELECT @vnExistePes = COUNT(*)
	  FROM GE_Pessoa
	  WHERE SeqPessoa = @vnSeqpessoa
	    AND LEN(Email) > 0

	  IF(@vnExistePes = 0)
		BEGIN
			UPDATE GE_Email
			SET IndPreferencial = 1
			WHERE eMail = @vsEmail1
			  AND SeqPessoa = @vnSeqpessoa
		END
     END

	If (LEN(@vsEmail2) > 0)
	Begin
	  select @vnExisteEml = COUNT(*)
	  FROM GE_Email
	  WHERE eMail = @vsEmail2
	    AND SeqPessoa = @vnSeqpessoa
		
	  If (@vnExisteEml = 0)
		Begin
			INSERT INTO GE_Email
			(SeqEmail, eMail, SeqPessoa, IndEmUso, DtaAlteracao, UsuAlterou, Obs)
			VALUES
			((SELECT MAX(SEQEMAIL) + 1 FROM GE_Email),
			@vsEmail2, @vnSeqpessoa, 1, GETDATE(), 'VTCCONS', '2')  
	    End
	END

	If (LEN(@vsEmail3) > 0)
	Begin
	  select @vnExisteEml = COUNT(*)
	  FROM GE_Email
	  WHERE eMail = @vsEmail3
	    AND SeqPessoa = @vnSeqpessoa
		
	  If (@vnExisteEml = 0)
		Begin
			INSERT INTO GE_Email
			(SeqEmail, eMail, SeqPessoa, IndEmUso, DtaAlteracao, UsuAlterou, Obs)
			VALUES
			((SELECT MAX(SEQEMAIL) + 1 FROM GE_Email),
			@vsEmail3, @vnSeqpessoa, 1, GETDATE(), 'VTCCONS', '3')  
	    End
	END
	
	UPDATE GE_Sequencia
	SET Sequencia = (SELECT MAX(SEQEMAIL) FROM GE_Email)
	WHERE NomeTabela = 'GE_Email'

	FETCH NEXT FROM C01
		INTO 	@vnSeqpessoa,
		        @vsEmail1,   
				@vsEmail2, 	
				@vsEmail3,   
				@vnExisteEml,		    
				@vnExistePes

	END                
    CLOSE C01
    DEALLOCATE C01    	

END
