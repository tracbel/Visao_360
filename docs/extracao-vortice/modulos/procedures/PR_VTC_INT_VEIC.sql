/* ==============================================================
   Objeto ..........: dbo.PR_VTC_INT_VEIC
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-10-31 15:47:09
   Modificado em ...: 2025-10-31 15:47:09
   Linhas ..........: 104
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : EXT_VEIC, ext_veic, EXT_VEICPROP, ext_veicprop, imp_veiculo
   Tabelas referidas: EXT_VEIC, ext_veicprop, GE_PESSOALINK, imp_veiculo
   Outras refs .....: PRC_GET_SEQUENCIA_TABELA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create PROCEDURE PR_VTC_INT_VEIC @NROEMPRESA NUMERIC(10), @IDMARCA NUMERIC(10) , @IDMODELO NUMERIC(10) , @PESSOALINKORIGEM VARCHAR(20), @PESSOALINK VARCHAR(250),
                                        @NROCHASSI VARCHAR(30),@NROCHASSIRED VARCHAR(20),@PLACA VARCHAR(10),@COMBUSTIVEL VARCHAR(30),@COREXTERNA VARCHAR(30),@CORINTERNA VARCHAR(30),
		                                @ANOFABRICACAO NUMERIC(4),@ANOMODELO NUMERIC(4),@KMATUAL NUMERIC(8),@DTAKMATUAL DATETIME,@USUARIOALTERACAO VARCHAR(20),@DTAALTERACAO DATETIME,
										@DTAPRIMVENDA DATETIME, @CODMODELO VARCHAR(50), @MODELO VARCHAR(50), @IDVEICULO numeric(18), @ESTADOVENDA CHAR(1), @FINANCIADOR VARCHAR(40), 
										@CANALVENDA VARCHAR(20), @NRONF NUMERIC(18), @SERIENF VARCHAR(12), @DTAVENDA DATETIME, @VLRVENDA NUMERIC(14, 2), @OBSERVACAO VARCHAR(250), 
										@KMCOMPRA NUMERIC(8), @REVENDA VARCHAR(40), @CODIGO NUMERIC(10) OUTPUT, @CODIGOPES NUMERIC(10) OUTPUT, @CODIGOIMP char(1) OUTPUT
AS
/*			Rotina diaria para atualizar veiculo de integracao
abr/2025 - Amaury
*/
-------------------------------
----- inicio da execução da rotina
Declare 
	@vnSeqVeic   numeric(10),
	@vnSeqVeicEx numeric(10),
	@vnSeqVeicProp numeric(10),
	@vnSeqVeicProNew numeric(10),
	@vnSeqPessoa numeric(10),
	@vnSeqPesex  numeric(10),
	@vnSeqPesprop  numeric(10),
	@vbVeicExist char(1)

BEGIN

--- procura a pessoa buscando o seqpessoa baseado nos links 
	Set @vnSeqPessoa = (Select seqpessoa From  GE_PESSOALINK
						Where ORIGEM = @PESSOALINKORIGEM
							AND PESSOALINK = @PESSOALINK)

--- procura veiculo na ext_veic
	Select @vnSeqVeic = idveic,
	       @vnSeqPesex = SEQPESSOA
	From  EXT_VEIC 
	Where IDVEICMARCA = @IDMARCA
		and IDVEICMODELO = @IDMODELO
		and CHASSI = @NROCHASSI

--- verificando se pessoa existe
	IF @vnSeqPessoa is not null
	begin
	--- pega sequencia da propriedade
		EXEC dbo.PRC_GET_SEQUENCIA_TABELA
				@sNomeTabela = 'EXT_VEICPROP',
				@vnSeqNovo = @vnSeqVeicProNew OUTPUT,
				@sTabelaOrigem = 'EXT_VEICPROP',
				@sCampoOrigem = 'IDVEICPROP'

--- testa se veic não existe na tabela ext_veic
		if @vnSeqVeic is null
		begin
		--- pega sequencia
			EXEC dbo.PRC_GET_SEQUENCIA_TABELA
				 @sNomeTabela = 'EXT_VEIC',
				 @vnSeqNovo = @vnSeqVeic OUTPUT,
				 @sTabelaOrigem = 'EXT_VEIC',
				 @sCampoOrigem = 'IDVEIC'

		--- insere ext_veic
			insert into EXT_VEIC ( IDVEIC, IDVEICMARCA, IDVEICMODELO, NROEMPRESA, SEQPESSOA, CHASSI, CHASSIRED, PLACA, COMBUSTIVEL, COREXTERNA, CORINTERNA,
									ANOFABRIC, ANOMODELO, KMATUAL, DTAKMATUAL, USUALTERACAO, DTAALTERACAO, DTAPRIMVENDA, DESCRICAO )
			values ( @vnSeqVeic, @IDMARCA, @IDMODELO, @NROEMPRESA, @vnSeqPessoa, @NROCHASSI ,@NROCHASSIRED ,@PLACA ,@COMBUSTIVEL ,@COREXTERNA ,@CORINTERNA ,
						@ANOFABRICACAO ,@ANOMODELO ,@KMATUAL ,@DTAKMATUAL ,@USUARIOALTERACAO ,@DTAALTERACAO ,@DTAPRIMVENDA, LEFT(@CODMODELO + '-' + @MODELO, 40) )
		end
		else
		begin
			--- atualizando dados da ext_veic
			update ext_veic set seqpessoa = @vnSeqPessoa, nroempresa = @NROEMPRESA, kmatual= @KMATUAL, dtakmatual = @DTAKMATUAL, 
								dtaalteracao = @DTAALTERACAO, usualteracao = @USUARIOALTERACAO
			where IDVEIC = @vnSeqVeic
		end

		--- identifica se o veiculo esta como propriedade
		Select @vnSeqVeicProp = idveicprop, @vnSeqPesprop = seqpessoa 
		From  ext_veicprop 
		WHERE IDVEIC = @vnSeqVeic AND INDPROPATUAL = 1 

		---- incluindo novo dono na ext_veicprop
		INSERT INTO EXT_VEICPROP ( IDVEICPROP, IDVEIC, SEQPESSOA, NROEMPRESA, ESTADOVENDA, FINANCIADOR, CANALVENDA, NRONF, SERIENF, DTAVENDA, VLRVENDA, OBSERVAcaO, KMCOMPRA,
									REVENDA, USUALTERACAO, DTAALTERACAO, INDPROPATUAL)
		VALUES ( @vnSeqVeicProNew, @vnSeqVeic, @vnSeqPessoa, @NROEMPRESA, @ESTADOVENDA, @FINANCIADOR, @CANALVENDA, @NRONF, @SERIENF, @DTAVENDA, @VLRVENDA, @OBSERVACAO, @KMCOMPRA,
					@REVENDA, @USUARIOALTERACAO, @DTAALTERACAO, 1 )

		--- desabilitando propriedade antiga
		update ext_veicprop set INDPROPATUAL = 0, USUALTERACAO = @USUARIOALTERACAO, DTAALTERACAO = @DTAALTERACAO
			WHERE IDVEICPROP = @vnSeqVeicProp

	--- atualiza imp_veiculo setando os dados de integração
		update imp_veiculo set statusimp = 'S' where IDVEICULO = @IDVEICULO
			
	--- atualiza imp_veiculo setando a data de importacao
		update imp_veiculo set dtaimport = getdate() where IDVEICULO = @IDVEICULO

		Set @CODIGOIMP = 'S'

	end ----  pessoa
	else
	begin
	--- atualiza imp_veiculo setando com erro os dados de integração porque nao encontrou a pessoa
		update imp_veiculo set statusimp = 'E' where IDVEICULO = @IDVEICULO
		Set @CODIGOIMP = 'N'
	end
	set @CODIGO = @vnSeqVeic
	SET @CODIGOPES = @vnSeqPessoa
END	