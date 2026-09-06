/* ==============================================================
   Objeto ..........: dbo.PR_VTC_INTFAMILIA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-10-31 15:45:47
   Modificado em ...: 2025-10-31 15:53:49
   Linhas ..........: 108
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : GE_SEQUENCIA, IV_GLOBALPAR, EXT_VEICFAMREF, ext_veicfam, EXT_VEICFAM
   Tabelas referidas: EXT_VEICFAM, EXT_VEICFAMREF, GE_SEQUENCIA, IV_GLOBALPAR, IV_PROPRIEDADE
   Outras refs .....: PRC_GET_SEQUENCIA_TABELA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[PR_VTC_INTFAMILIA] @IDMARCA NUMERIC(10), @FAMILIA VARCHAR(30) , @CODIGO NUMERIC(10) OUTPUT, @CODIGOPROPR INTEGER OUTPUT
AS
/*			Rotina diaria para atualizar veiculo de integracao
abr/2025 - Amaury
*/
-------------------------------
----- inicio da execução da rotina
DECLARE @vnSeqFamilia NUMERIC(10)
DECLARE @vnSeqPropr NUMERIC(10)
DECLARE @vnSeqProprDP NUMERIC(10)
DECLARE @vnSeqParPropr NUMERIC(10)
DECLARE @vsFamilia VARCHAR(30)
DECLARE @vnSeqPar NUMERIC(10)


BEGIN
	-- 1) Verifica na tabela IV_GLOBALPAR
    SELECT @vnSeqParPropr = PP.SEQPROPRIEDADE, @vnSeqpar = SEQPAR
	FROM IV_GLOBALPAR GP
	JOIN IV_PROPRIEDADE PP ON PP.PROPRIEDADE = GP.CAMPO2
	WHERE GP.SeqGlbPar = 32
	  AND GP.CAMPO1 = @FAMILIA

	IF @vnSeqPar IS NULL
	BEGIN
		-- Obter próximo SEQPAR
		BEGIN TRANSACTION 
			SELECT @vnSeqpar = SEQUENCIA
			FROM GE_SEQUENCIA
			WHERE NOMETABELA = 'IV_GLOBALPAR'
			
			SET @vnSeqpar = @vnSeqpar + 1
			
			UPDATE GE_SEQUENCIA SET SEQUENCIA = @vnSeqpar
			WHERE NOMETABELA = 'IV_GLOBALPAR'
		COMMIT TRANSACTION
		
		-- Inserir novo parâmetro
		INSERT INTO IV_GLOBALPAR (SEQPAR, SEQGLBPAR, NROEMPRESA, CAMPO1, DTAALTERACAO, USUALTERACAO)
		VALUES (@vnSeqpar, 32, 0, @FAMILIA, GETDATE(), 'VTCCONS')

		SET @CODIGOPROPR = 999999
		RETURN
	END
	ELSE
	IF @vnSeqParPropr IS NULL OR @vnSeqParPropr = 0
	BEGIN
		SET @CODIGOPROPR = 999999
		RETURN
	END

	-- 2) Verifica na tabela EXT_VEICFAMREF
	SELECT @vnSeqProprDP = SEQPROPRIEDADE, @vsFamilia = FAMILIA
	FROM EXT_VEICFAMREF
	WHERE FAMILIA = @FAMILIA

	IF @vsFamilia IS NULL OR @vsFamilia=''
	BEGIN
		-- Inserção condicional na EXT_VEICFAMREF
		IF @vnSeqProprDP IS NULL or @vnSeqProprDP = 0
		BEGIN
			INSERT INTO EXT_VEICFAMREF (SEQPROPRIEDADE, FAMILIA)
			VALUES (@vnSeqParPropr, @FAMILIA)
			
			SET @vnSeqProprDP = @vnSeqParPropr
		END
		ELSE
		BEGIN
			SET @CODIGOPROPR = 999998
			RETURN
		END
	END


	-- 3) Verifica na tabela EXT_VEICFAM
	SELECT @vnSeqFamilia = IDVEICFAMILIA, @vnSeqPropr = SEQPROPRIEDADE
	FROM EXT_VEICFAM
	WHERE DESCRICAO = @FAMILIA AND IDVEICMARCA = @IDMARCA

	if ( @vnSeqPropr=0 or @vnSeqPropr is null ) and @vnSeqProprDP > 0
	begin
		update ext_veicfam set seqpropriedade = @vnSeqProprDP where DESCRICAO = @FAMILIA AND IDVEICMARCA = @IDMARCA
		set @vnSeqPropr = @vnSeqProprDP
	end

	IF @vnSeqFamilia IS NULL OR @vnSeqFamilia=0
	BEGIN
	--- pega sequencia
		EXEC dbo.PRC_GET_SEQUENCIA_TABELA
			 @sNomeTabela = 'EXT_VEICFAM',
			 @vnSeqNovo = @vnSeqFamilia OUTPUT,
			 @sTabelaOrigem = 'EXT_VEICFAM',
			 @sCampoOrigem = 'IDVEICFAMILIA'

	--- insere familia
		insert into EXT_VEICFAM ( IDVEICFAMILIA, IDVEICMARCA, DESCRICAO, INDHORAKM, SEQPROPRIEDADE ) 
		values (@vnSeqFamilia, @IDMARCA, @FAMILIA, 'N', @CODIGOPROPR )

		SET @CODIGOPROPR = @vnSeqParPropr
		SET @CODIGO = @vnSeqFamilia
	END
	ELSE
	BEGIN
		SET @CODIGOPROPR = @vnSeqPropr
		SET @CODIGO = @vnSeqFamilia
	END
END
