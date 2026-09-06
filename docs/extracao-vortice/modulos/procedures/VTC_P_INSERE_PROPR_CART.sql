/* ==============================================================
   Objeto ..........: dbo.VTC_P_INSERE_PROPR_CART
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2018-02-06 09:37:32
   Modificado em ...: 2018-02-06 11:32:00
   Linhas ..........: 118
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : IV_ClientePropr, GE_Sequencia
   Tabelas referidas: GE_IMPORTA_CART, GE_Pessoa, GE_Sequencia, IV_ClientePropr
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[VTC_P_INSERE_PROPR_CART]
AS

DECLARE @vnSeqpessoa as numeric
DECLARE @vsOrigem    as varchar(80)
DECLARE @vsNJUR      as varchar(15)
DECLARE @vsDescJNUR  as varchar(80)
DECLARE @vsCNAE      as varchar(15)
DECLARE @vsDescCNAE  as varchar(180)

BEGIN

DECLARE C01 CURSOR FOR 


SELECT  CART.SeqPessoa, 
		CART.ORIGEMRECEITA,
		CART.NJUR,
		CART.DESCNJUR,
		CART.CNAE,
		CART.DESCCNAE
FROM GE_IMPORTA_CART CART
WHERE EXISTS (SELECT 1 
			  FROM GE_Pessoa PES
			  WHERE PES.SeqPessoa = CART.SeqPessoa)
 --AND CART.SeqPessoa IN (74898, 69838)


  OPEN C01
  
    	FETCH NEXT FROM C01
		INTO @vnSeqpessoa,
			 @vsOrigem   ,
			 @vsNJUR     ,
			 @vsDescJNUR ,
			 @vsCNAE     ,
			 @vsDescCNAE 

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

	If (LEN(@vsOrigem) > 0)
	Begin
		INSERT INTO IV_ClientePropr
		(SeqPropPessoa, SeqPessoa, SeqPropriedade, Referencia, Ativo, DtaInclusao, UsuInclusao)
		VALUES
		((SELECT MAX(SeqPropPessoa) +1 FROM IV_ClientePropr),
		@vnSeqpessoa, 9400, @vsOrigem, 'S', GETDATE(), 'VTCCONS')
	END

	If (LEN(@vsCNAE) > 0)
	Begin
		INSERT INTO IV_ClientePropr
		(SeqPropPessoa, SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, DtaInclusao, UsuInclusao)
		VALUES
		((SELECT MAX(SeqPropPessoa) +1 FROM IV_ClientePropr),
		  @vnSeqpessoa, 9413, @vsCNAE, 'S', @vsDescCNAE, GETDATE(), 'VTCCONS')
    End

	If (LEN(@vsNJUR) > 0)
	Begin
		INSERT INTO IV_ClientePropr
		(SeqPropPessoa, SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, DtaInclusao, UsuInclusao)
		VALUES
		((SELECT MAX(SeqPropPessoa) +1 FROM IV_ClientePropr),
		  @vnSeqpessoa, 9414, @vsNJUR, 'S', @vsDescJNUR, GETDATE(), 'VTCCONS')
    End

	UPDATE GE_Sequencia
	SET Sequencia = (SELECT MAX(SeqPropPessoa) FROM IV_ClientePropr)
	WHERE NomeTabela = 'IV_ClientePropr'
	
	FETCH NEXT FROM C01
		INTO 	@vnSeqpessoa,
		        @vsOrigem   ,
				@vsNJUR     ,
				@vsDescJNUR ,
				@vsCNAE     ,
				@vsDescCNAE 
				
	END                
    CLOSE C01
    DEALLOCATE C01    	

END
