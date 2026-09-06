/* ==============================================================
   Objeto ..........: dbo.VTC_P_INSERE_FONE_CART
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2018-02-05 10:20:28
   Modificado em ...: 2018-02-06 11:31:26
   Linhas ..........: 316
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : GE_PessoaFone, GE_Sequencia
   Tabelas referidas: GE_IMPORTA_CART, GE_Pessoa, GE_PessoaFone, GE_Sequencia
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[VTC_P_INSERE_FONE_CART]
AS

DECLARE @vnSeqpessoa numeric
DECLARE @vsFoneDDD1  as varchar(5)
DECLARE @vnFoneNro1  as numeric
DECLARE @vnTipo1     as numeric
DECLARE @vsFoneDDD2  as varchar(5)
DECLARE @vnFoneNro2  as numeric
DECLARE @vnTipo2     as numeric
DECLARE @vsFoneDDD3  as varchar(5)
DECLARE @vnFoneNro3  as numeric
DECLARE @vnTipo3     as numeric
DECLARE @vsFoneDDD4  as varchar(5)
DECLARE @vnFoneNro4  as numeric
DECLARE @vnTipo4     as numeric
DECLARE @vsFoneDDD5  as varchar(5)
DECLARE @vnFoneNro5  as numeric
DECLARE @vnTipo5     as numeric
DECLARE @vsFoneDDD6  as varchar(5)
DECLARE @vnFoneNro6  as numeric
DECLARE @vnTipo6     as numeric
DECLARE @vsFoneDDD7  as varchar(5)
DECLARE @vnFoneNro7  as numeric
DECLARE @vnTipo7     as numeric
DECLARE @vsFoneDDD8  as varchar(5)
DECLARE @vnFoneNro8  as numeric
DECLARE @vnTipo8     as numeric
DECLARE @vnExiste    as numeric

BEGIN

DECLARE C01 CURSOR FOR 


SELECT  CART.SeqPessoa, 
		CART.FoneDDD1,
		CART.FoneNro1,
		CART.Tipo1,
		CART.FoneDDD2,
		CART.FoneNro2,
		CART.Tipo2,
		CART.FoneDDD3,
		CART.FoneNro3,
		CART.Tipo3,
		CART.FoneDDD4,
		CART.FoneNro4,
		CART.Tipo4,
		CART.FoneDDD5,
		CART.FoneNro5,
		CART.Tipo5,
		CART.FoneDDD6,
		CART.FoneNro6,
		CART.Tipo6,
		CART.FoneDDD7,
		CART.FoneNro7,
		CART.Tipo7,
		CART.FoneDDD8,
		CART.FoneNro8,
		CART.Tipo8,
		0
FROM GE_IMPORTA_CART CART
WHERE EXISTS (SELECT 1 
			  FROM GE_Pessoa PES
			  WHERE PES.SeqPessoa = CART.SeqPessoa)
  --AND CART.SeqPessoa IN (71376, 69838)


  OPEN C01
  
    	FETCH NEXT FROM C01
		INTO 	@vnSeqpessoa,
                @vsFoneDDD1,                 
                @vnFoneNro1, 				
				@vnTipo1,    
				@vsFoneDDD2, 
				@vnFoneNro2, 
				@vnTipo2,    
				@vsFoneDDD3, 
				@vnFoneNro3, 
				@vnTipo3,    
				@vsFoneDDD4, 
				@vnFoneNro4, 
				@vnTipo4,    
				@vsFoneDDD5, 
				@vnFoneNro5, 
				@vnTipo5,    
				@vsFoneDDD6, 
				@vnFoneNro6, 
				@vnTipo6,    
				@vsFoneDDD7, 
				@vnFoneNro7, 
				@vnTipo7,    
				@vsFoneDDD8, 
				@vnFoneNro8, 
				@vnTipo8,    
				@vnExiste

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

	If (@vnFoneNro1 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro1
	    AND SeqPessoa = @vnSeqpessoa
		
		If (@vnExiste = 0)
		Begin
			INSERT INTO GE_PessoaFone
			(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
			VALUES
			((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
			(SELECT CASE WHEN @vnTipo1 = 2 THEN 69 WHEN @vnTipo1 IN (3,8) THEN 71 ELSE NULL END), 
			@vnSeqpessoa, @vsFoneDDD1, @vnFoneNro1, '1')  
		End
	END

	If (@vnFoneNro2 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro2
	    AND SeqPessoa = @vnSeqpessoa

		If (@vnExiste = 0)
		BEGIN
		INSERT INTO GE_PessoaFone
		(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
		VALUES
		((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
		 (SELECT CASE WHEN @vnTipo2 = 2 THEN 69 WHEN @vnTipo2 IN (3,8) THEN 71 ELSE NULL END), 
		  @vnSeqpessoa, @vsFoneDDD2, @vnFoneNro2, '2')
		END
    End

	If (@vnFoneNro3 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro3
	    AND SeqPessoa = @vnSeqpessoa

		If (@vnExiste = 0)
		BEGIN
		INSERT INTO GE_PessoaFone
		(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
		VALUES
		((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
		 (SELECT CASE WHEN @vnTipo3 = 2 THEN 69 WHEN @vnTipo3 IN (3,8) THEN 71 ELSE NULL END), 
		  @vnSeqpessoa, @vsFoneDDD3, @vnFoneNro3, '3')
		END
    End

	If (@vnFoneNro4 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro4
	    AND SeqPessoa = @vnSeqpessoa

		If (@vnExiste = 0)
		BEGIN
		INSERT INTO GE_PessoaFone
		(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
		VALUES
		((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
		 (SELECT CASE WHEN @vnTipo4 = 2 THEN 69 WHEN @vnTipo4 IN (3,8) THEN 71 ELSE NULL END), 
		  @vnSeqpessoa, @vsFoneDDD4, @vnFoneNro4, '4')
		END
    End

	If (@vnFoneNro5 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro5
	    AND SeqPessoa = @vnSeqpessoa

		If (@vnExiste = 0)
		BEGIN
		INSERT INTO GE_PessoaFone
		(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
		VALUES
		((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
		 (SELECT CASE WHEN @vnTipo5 = 2 THEN 69 WHEN @vnTipo5 IN (3,8) THEN 71 ELSE NULL END), 
		  @vnSeqpessoa, @vsFoneDDD5, @vnFoneNro5, '5')
		END
    End

	If (@vnFoneNro6 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro6
	    AND SeqPessoa = @vnSeqpessoa

		If (@vnExiste = 0)
		BEGIN
		INSERT INTO GE_PessoaFone
		(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
		VALUES
		((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
		 (SELECT CASE WHEN @vnTipo6 = 2 THEN 69 WHEN @vnTipo6 IN (3,8) THEN 71 ELSE NULL END), 
		  @vnSeqpessoa, @vsFoneDDD6, @vnFoneNro6, '6')
		END
    End

	If (@vnFoneNro7 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro7
	    AND SeqPessoa = @vnSeqpessoa

		If (@vnExiste = 0)
		BEGIN
		INSERT INTO GE_PessoaFone
		(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
		VALUES
		((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
		 (SELECT CASE WHEN @vnTipo7 = 2 THEN 69 WHEN @vnTipo7 IN (3,8) THEN 71 ELSE NULL END), 
		  @vnSeqpessoa, @vsFoneDDD7, @vnFoneNro7, '7')
		END
    End

	If (@vnFoneNro8 > 0)
	Begin
	  select @vnExiste = COUNT(*)
	  FROM GE_PessoaFone
	  WHERE Numero = @vnFoneNro8
	    AND SeqPessoa = @vnSeqpessoa

		If (@vnExiste = 0)
		BEGIN
		INSERT INTO GE_PessoaFone
		(SeqPesFone, TipoFoneSeqPar, SeqPessoa, DDD, Numero, Obs)
		VALUES
		((SELECT MAX(SEQPESFONE) + 1 FROM GE_PessoaFone), 
		 (SELECT CASE WHEN @vnTipo8 = 2 THEN 69 WHEN @vnTipo8 IN (3,8) THEN 71 ELSE NULL END), 
		  @vnSeqpessoa, @vsFoneDDD8, @vnFoneNro8, '8')
		END
    End

	UPDATE GE_Sequencia
	SET Sequencia = (SELECT MAX(SeqPesFone) FROM GE_PessoaFone)
	WHERE NomeTabela = 'GE_PessoaFone'
	
	FETCH NEXT FROM C01
		INTO 	@vnSeqpessoa,
		        @vsFoneDDD1,
				@vnFoneNro1,
				@vnTipo1,   
				@vsFoneDDD2,
				@vnFoneNro2,
				@vnTipo2,   
				@vsFoneDDD3,
				@vnFoneNro3,
				@vnTipo3,   
				@vsFoneDDD4,
				@vnFoneNro4,
				@vnTipo4,   
				@vsFoneDDD5,
				@vnFoneNro5,
				@vnTipo5,   
				@vsFoneDDD6,
				@vnFoneNro6,
				@vnTipo6,   
				@vsFoneDDD7,
				@vnFoneNro7,
				@vnTipo7,   
				@vsFoneDDD8,
				@vnFoneNro8,
				@vnTipo8,
				@vnExiste
				    
	
	END                
    CLOSE C01
    DEALLOCATE C01    	

END
