/* ==============================================================
   Objeto ..........: dbo.PR_VTC_INTVEICULO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-10-31 15:48:08
   Modificado em ...: 2025-10-31 15:49:03
   Linhas ..........: 320
   Escreve em tabela: SIM (INSERT, UPDATE, DELETE)
   Alvos de escrita : imp_veiculo_integrado, imp_veiculo, IMP_VEICULO
   Tabelas referidas: EXT_VEICREF, IMP_VEICULO, imp_veiculo_integrado
   Outras refs .....: PR_VTC_INT_VEIC, PR_VTC_INTFAMILIA, PR_VTC_INTMARCA, PR_VTC_INTMODELO, PR_VTC_INTPROPRIEDADE, VTC_P_HISTORICO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE PR_VTC_INTVEICULO
AS
/*			Rotina diaria para atualizar veiculo de integracao
abr/2025 - Amaury
*/
-------------------------------
----- inicio da execução da rotina
DECLARE
	@IDVEICULO			numeric(18, 0),
	@ORIGEM				varchar(20),
	@NROEMPRESA			numeric(10, 0),
	@PESSOALINKORIGEM		varchar(20),
	@PESSOALINK			varchar(250),
	@NROCNPJCPF			numeric(13, 0),
	@DIGCNPJCPF			numeric(2, 0),
	@CNPJX				varchar(20),
	@NROCHASSI			varchar(40),
	@NROCHASSIRED			varchar(20),
	@TIPOVEICULO			varchar(5),
	@PLACA				varchar(9),
	@COMBUSTIVEL			varchar(30),
	@MARCA				varchar(20),
	@CODFAMILIA			varchar(20),
	@FAMILIA			varchar(30),
	@CODMODELO			varchar(50),
	@MODELO				varchar(50),
	@COREXTERNA			varchar(30),
	@CORINTERNA			varchar(30),
	@POTENCIA			numeric(4, 0),
	@QTDEEIXO			numeric(2, 0),
	@ESTADOVENDA			char(1),
	@FORMAPGTO			char(1),
	@FINANCIADOR			varchar(40),
	@CANALVENDA			varchar(20),
	@NRONF				numeric(18, 0),
	@SERIENF			varchar(12),
	@ANOFABRICACAO			numeric(4, 0),
	@ANOMODELO			numeric(4, 0),
	@DTAVENDA			datetime 	,
	@DTAPREVQUITACAO		datetime 	,
	@VLRVENDA			numeric(14, 2),
	@OBSERVACAO			varchar(250),
	@KMATUAL			numeric(8, 0),
	@DTAKMATUAL			datetime 	,
	@KMPROXREVISAO			numeric(8, 0),
	@DTAPROXREVISAO			datetime 	,
	@REVENDA			varchar(40),
	@VENDEDOR			varchar(60),
	@TIPOUSO			varchar(40),
	@USUARIOALTERACAO		varchar(20),
	@DTAALTERACAO			datetime 	,
	@DTAGERACAO			datetime 	,
	@STATUSIMP			char(1),
	@DTAIMPORT			datetime 	,
	@CODVENDEDOR			varchar(20),
	@DTAPRIMVENDA			datetime 	,
	@NROCPFVENDEDOR			numeric(11, 0),
	@IDMARCA            numeric(10),
	@IDFAMILIA            numeric(10),
	@IDMODELO            numeric(10),

	@NROEMPRESAPADRAO  integer,
	@SEQPESSOA			numeric(10),
	@SEQPROPRIEDADE		integer,
	@IDVEICULODEL       NUMERIC(18),
	@DTHOJE				DATETIME,
	@DETALHE		    VARCHAR(250),
	@DETALHE1		    VARCHAR(250),
	@DETALHE2		    VARCHAR(250),
	@DETALHE3		    VARCHAR(250),
	@QTPROPR			INTEGER


DECLARE CURVEIC CURSOR FOR
	SELECT IDVEICULO
      ,ORIGEM
      ,NROEMPRESA
      ,PESSOALINKORIGEM
      ,PESSOALINK
      ,NROCNPJCPF
      ,DIGCNPJCPF
      ,CNPJX
      ,NROCHASSI
      ,NROCHASSIRED
      ,TIPOVEICULO
      ,PLACA
      ,COMBUSTIVEL
      ,MARCA
      ,CODFAMILIA
      ,FAMILIA
      ,CODMODELO
      ,MODELO
      ,COREXTERNA
      ,CORINTERNA
      ,POTENCIA
      ,QTDEEIXO
      ,ESTADOVENDA
      ,FORMAPGTO
      ,FINANCIADOR
      ,CANALVENDA
      ,NRONF
      ,SERIENF
      ,ANOFABRICACAO
      ,ANOMODELO
      ,DTAVENDA
      ,DTAPREVQUITACAO
      ,VLRVENDA
      ,OBSERVACAO
      ,KMATUAL
      ,DTAKMATUAL
      ,KMPROXREVISAO
      ,DTAPROXREVISAO
      ,REVENDA
      ,VENDEDOR
      ,TIPOUSO
      ,USUARIOALTERACAO
      ,DTAALTERACAO
      ,DTAGERACAO
      ,STATUSIMP
      ,DTAIMPORT
      ,CODVENDEDOR
      ,DTAPRIMVENDA
      ,NROCPFVENDEDOR
  FROM [dbo].[IMP_VEICULO]
  where statusimp is null


BEGIN
	OPEN CURVEIC
	FETCH NEXT FROM CURVEIC into 	@IDVEICULO		,
									@ORIGEM			,
									@NROEMPRESA		,
									@PESSOALINKORIGEM	,
									@PESSOALINK		,
									@NROCNPJCPF		,
									@DIGCNPJCPF		,
									@CNPJX			,
									@NROCHASSI		,
									@NROCHASSIRED		,
									@TIPOVEICULO		,
									@PLACA			,
									@COMBUSTIVEL		,
									@MARCA			,
									@CODFAMILIA		,
									@FAMILIA		,
									@CODMODELO		,
									@MODELO			,
									@COREXTERNA		,
									@CORINTERNA		,
									@POTENCIA		,
									@QTDEEIXO		,
									@ESTADOVENDA		,
									@FORMAPGTO		,
									@FINANCIADOR		,
									@CANALVENDA		,
									@NRONF			,
									@SERIENF		,
									@ANOFABRICACAO		,
									@ANOMODELO		,
									@DTAVENDA		,
									@DTAPREVQUITACAO	,
									@VLRVENDA		,
									@OBSERVACAO		,
									@KMATUAL		,
									@DTAKMATUAL		,
									@KMPROXREVISAO		,
									@DTAPROXREVISAO		,
									@REVENDA		,
									@VENDEDOR		,
									@TIPOUSO		,
									@USUARIOALTERACAO	,
									@DTAALTERACAO		,
									@DTAGERACAO		,
									@STATUSIMP		,
									@DTAIMPORT		,
									@CODVENDEDOR		,
									@DTAPRIMVENDA		,
									@NROCPFVENDEDOR		

	WHILE (@@FETCH_STATUS = 0)
	BEGIN		
		
		set @NROEMPRESAPADRAO = 1
		SET @IDVEICULODEL = @IDVEICULO
		SET @DTHOJE = GETDATE()
		SET @DETALHE1 = 'ERRO NA INTEGRAÇÃO, FAMILIA NÃO POSSUI PROPRIEDADE ASSOCIADA - ATUALIZE OS PARAMETROS DINAMICOS'
		SET @DETALHE2= 'ERRO NA INTEGRAÇÃO, PROPRIEDADE NÃO FOI CRIADO UM DE-PARA DOS CAMPOS (EXT_VEICREF) AVISE SUPORTE'
		SET @DETALHE3= 'ERRO NA INTEGRAÇÃO, PESSOA NÃO ENCONTRADA PARA O VEICULO'
		exec [dbo].[PR_VTC_INTMARCA] @NROEMPRESAPADRAO, @MARCA, @CODIGO = @IDMARCA OUTPUT

		exec [dbo].[PR_VTC_INTFAMILIA] @IDMARCA, @FAMILIA, @CODIGO = @IDFAMILIA OUTPUT, @CODIGOPROPR = @SEQPROPRIEDADE OUTPUT

		SELECT @QTPROPR = COUNT(1) FROM EXT_VEICREF WHERE SEQPROPRIEDADE = @SEQPROPRIEDADE
		if @SEQPROPRIEDADE<999997 AND @QTPROPR>0
		BEGIN
			exec [dbo].[PR_VTC_INTMODELO] @IDFAMILIA, @CODMODELO, @MODELO, @CODIGO = @IDMODELO OUTPUT

			exec [dbo].[PR_VTC_INT_VEIC] @NROEMPRESAPADRAO, @IDMARCA, @IDMODELO, @PESSOALINKORIGEM, @PESSOALINK,@NROCHASSI,@NROCHASSIRED,@PLACA,@COMBUSTIVEL,@COREXTERNA,@CORINTERNA,
											@ANOFABRICACAO,@ANOMODELO,@KMATUAL,@DTAKMATUAL,@USUARIOALTERACAO,@DTAALTERACAO,@DTAPRIMVENDA, @CODMODELO, @MODELO, 
											@IDVEICULO, @ESTADOVENDA, @FINANCIADOR, @CANALVENDA, @NRONF, @SERIENF, @DTAVENDA, @VLRVENDA, @OBSERVACAO, @KMATUAL, @REVENDA, 
											@CODIGO = @IDVEICULO OUTPUT, @CODIGOPES = @SEQPESSOA OUTPUT, @CODIGOIMP = @STATUSIMP OUTPUT
		
			IF @SEQPESSOA IS NOT NULL AND @SEQPROPRIEDADE IS NOT NULL
			BEGIN
				EXEC [dbo].[PR_VTC_INTPROPRIEDADE] @SEQPESSOA, @SEQPROPRIEDADE, @MARCA, @NROCHASSI, @FAMILIA, @MODELO, @ANOFABRICACAO, @DTAALTERACAO, @USUARIOALTERACAO
			END
			if @STATUSIMP='S'
			begin
				insert into imp_veiculo_integrado select * from imp_veiculo where idveiculo = @IDVEICULODEL
				----delete from imp_veiculo where idveiculo = @IDVEICULODEL
			end
			else
			begin
				SET @DETALHE = 'IDVEIC= ' + CAST(@IDVEICULO AS VARCHAR) + @DETALHE3
				EXEC VTC_P_HISTORICO 
						'PROTHEUS',           --- origem
						';',				  --- separador
						'VEIC_ERR_PROPR',     --- evento
						'MASTER',             --- codusuario
						@DETALHE,             --- detalhe
						@DTHOJE,              --- dtrealizacao
						@NROEMPRESAPADRAO,    ---- nroempresa
						@PESSOALINK,          --- pessoalink
						@PESSOALINKORIGEM,    --- pessoalinkorigem
						NULL,                 --- pessoalinkant
						NULL,				  --- pessoalinkorigemant
						NULL,				  --- processo
						NULL,				  --- seqpessoa
						NULL 				  --- resultado
			end
		END
		ELSE
		BEGIN
			UPDATE IMP_VEICULO SET StatusIMP='E' WHERE IDVEICULO = @IDVEICULO
			SET @DETALHE = 'IDVEIC= ' + CAST(@IDVEICULO AS VARCHAR)
			IF @SEQPROPRIEDADE=999999
			BEGIN
				SET @DETALHE = @DETALHE + ' ' + @DETALHE1
			END
			ELSE
			IF @SEQPROPRIEDADE=999998
			BEGIN
				SET @DETALHE = @DETALHE + ' ' + @DETALHE2
			END
			ELSE
			IF @SEQPROPRIEDADE=999997
			BEGIN
				SET @DETALHE = @DETALHE + ' ' + @DETALHE3
			END

			EXEC VTC_P_HISTORICO 
					'PROTHEUS',           --- origem
					';',				  --- separador
					'VEIC_ERR_PROPR',     --- evento
					'MASTER',             --- codusuario
					@DETALHE,             --- detalhe
					@DTHOJE,              --- dtrealizacao
					@NROEMPRESAPADRAO,    ---- nroempresa
					@PESSOALINK,          --- pessoalink
					@PESSOALINKORIGEM,    --- pessoalinkorigem
					NULL,                 --- pessoalinkant
					NULL,				  --- pessoalinkorigemant
					NULL,				  --- processo
					NULL,				  --- seqpessoa
					NULL 				  --- resultado
		END

	FETCH NEXT FROM CURVEIC into 	@IDVEICULO		,
									@ORIGEM			,
									@NROEMPRESA		,
									@PESSOALINKORIGEM	,
									@PESSOALINK		,
									@NROCNPJCPF		,
									@DIGCNPJCPF		,
									@CNPJX			,
									@NROCHASSI		,
									@NROCHASSIRED		,
									@TIPOVEICULO		,
									@PLACA			,
									@COMBUSTIVEL		,
									@MARCA			,
									@CODFAMILIA		,
									@FAMILIA		,
									@CODMODELO		,
									@MODELO			,
									@COREXTERNA		,
									@CORINTERNA		,
									@POTENCIA		,
									@QTDEEIXO		,
									@ESTADOVENDA		,
									@FORMAPGTO		,
									@FINANCIADOR		,
									@CANALVENDA		,
									@NRONF			,
									@SERIENF		,
									@ANOFABRICACAO		,
									@ANOMODELO		,
									@DTAVENDA		,
									@DTAPREVQUITACAO	,
									@VLRVENDA		,
									@OBSERVACAO		,
									@KMATUAL		,
									@DTAKMATUAL		,
									@KMPROXREVISAO		,
									@DTAPROXREVISAO		,
									@REVENDA		,
									@VENDEDOR		,
									@TIPOUSO		,
									@USUARIOALTERACAO	,
									@DTAALTERACAO		,
									@DTAGERACAO		,
									@STATUSIMP		,
									@DTAIMPORT		,
									@CODVENDEDOR		,
									@DTAPRIMVENDA		,
									@NROCPFVENDEDOR		
	END
	CLOSE CURVEIC;
	DEALLOCATE CURVEIC;
END	