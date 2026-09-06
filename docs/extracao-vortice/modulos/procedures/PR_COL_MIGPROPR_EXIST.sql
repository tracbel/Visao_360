/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGPROPR_EXIST
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 17:42:49
   Modificado em ...: 2024-12-28 17:42:49
   Linhas ..........: 303
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOA_ITA, IV_CLIENTEPROPR_ITA, MIG_GE_PESSOA
   Outras refs .....: PR_COL_MOVPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create procedure [dbo].[PR_COL_MIGPROPR_EXIST] 
AS


/*
-- autor: AMAURY 08/11/2024
   objetivo: Migrar propriedades pessoas existem na norte
   log de alterações: 01/11/2024 - Criação da rotina testando e finalizando criação da rotina
*/	

	DECLARE 
		@SEQPROPPESSOA       NUMERIC(18, 0),
	@SEQPESSOA           NUMERIC(10, 0),
	@SEQPROPRIEDADE      NUMERIC(4, 0),
	@REFERENCIA          VARCHAR(30),
	@IDENTIFICADOR       VARCHAR(30),
	@ATIVO               CHAR(1),
	@NOTAS               VARCHAR(250),
	@CAMPO1              VARCHAR(40),
	@CAMPO2              VARCHAR(40),
	@CAMPO3              VARCHAR(40),
	@CAMPO4              VARCHAR(40),
	@CAMPO5              VARCHAR(40),
	@CAMPO6              VARCHAR(40),
	@NUMERO1             DECIMAL(15, 2),
	@NUMERO2             DECIMAL(15, 2),
	@NUMERO3             DECIMAL(15, 2),
	@NUMERO4             DECIMAL(15, 2),
	@NUMERO5             DECIMAL(15, 2),
	@NUMERO6             DECIMAL(15, 2),
	@DATA1               DATETIME,
	@DATA2               DATETIME,
	@DATA3               DATETIME,
	@DATA4               DATETIME,
	@DATA5               DATETIME,
	@DATA6               DATETIME,
	@SIMNAO1             NUMERIC(1, 0),
	@SIMNAO2             NUMERIC(1, 0),
	@SIMNAO3             NUMERIC(1, 0),
	@SIMNAO4             NUMERIC(1, 0),
	@SIMNAO5             NUMERIC(1, 0),
	@SIMNAO6             NUMERIC(1, 0),
	@LITERAL1            VARCHAR(40),
	@LITERAL2            VARCHAR(40),
	@LITERAL3            VARCHAR(40),
	@LITERAL4            VARCHAR(40),
	@LITERAL5            VARCHAR(40),
	@LITERAL6            VARCHAR(40),
	@LITERAL7            VARCHAR(40),
	@LITERAL8            VARCHAR(40),
	@LITERAL9            VARCHAR(40),
	@LITERAL10           VARCHAR(40),
	@CODORIGEM           VARCHAR(20),
	@ULTORIGEM           VARCHAR(20),
	@DTAINCLUSAO         DATETIME,
	@USUINCLUSAO         VARCHAR(20),
	@DTAALTERACAO        DATETIME,
	@USUALTERACAO        VARCHAR(20),
	@CAMPO7              VARCHAR(40),
	@CAMPO8              VARCHAR(40),
	@CAMPO7SQL           NUMERIC(1, 0),
	@CAMPO8SQL           NUMERIC(1, 0)


	Declare curPES
	CURSOR
	FOR
	SELECT     PRP.SeqPropPessoa
			  ,PES.SEQPESSOA
			  ,case when PRP.SeqPropriedade<100 then PRP.SeqPropriedade + 9600 else PRP.SeqPropriedade + 100 end AS SEQPROPRIEDADE
			  ,PRP.Referencia
			  ,PRP.Identificador
			  ,PRP.Ativo
			  ,PRP.Notas
			  ,PRP.Campo1
			  ,PRP.Campo2
			  ,PRP.Campo3
			  ,PRP.Campo4
			  ,PRP.Campo5
			  ,PRP.Campo6
			  ,PRP.Numero1
			  ,PRP.Numero2
			  ,PRP.Numero3
			  ,PRP.Numero4
			  ,PRP.Numero5
			  ,PRP.Numero6
			  ,PRP.Data1
			  ,PRP.Data2
			  ,PRP.Data3
			  ,PRP.Data4
			  ,PRP.Data5
			  ,PRP.Data6
			  ,PRP.SimNao1
			  ,PRP.SimNao2
			  ,PRP.SimNao3
			  ,PRP.SimNao4
			  ,PRP.SimNao5
			  ,PRP.SimNao6
			  ,PRP.Literal1
			  ,PRP.Literal2
			  ,PRP.Literal3
			  ,PRP.Literal4
			  ,PRP.Literal5
			  ,PRP.Literal6
			  ,PRP.Literal7
			  ,PRP.Literal8
			  ,PRP.Literal9
			  ,PRP.Literal10
			  ,PRP.CodOrigem
			  ,PRP.UltOrigem
			  ,PRP.DtaInclusao
			  ,PRP.UsuInclusao
			  ,PRP.DtaAlteracao
			  ,PRP.UsuAlteracao
			  ,PRP.CAMPO7
			  ,PRP.CAMPO8
			  ,PRP.CAMPO7SQL
			  ,PRP.CAMPO8SQL
		FROM MIG_GE_PESSOA MP
		JOIN GE_PESSOA_ITA PI ON PI.SEQPESSOA = MP.SEQPESSOA
		JOIN IV_CLIENTEPROPR_ITA PRP ON PRP.SEQPESSOA = MP.seqpessoa
		                     AND PRP.Ativo='S'
		JOIN GE_PESSOA PES ON PES.NROCGCCPF = PI.NROCGCCPF
		WHERE 1=1
		  AND MP.DESCRICAO = 'Ativo/Prospect CNPJ Existente'
---		  AND MP.SEQPESSOA in ( 869 )

begin

	OPEN curPES
		FETCH NEXT FROM curPES INTO 			  @SEQPROPPESSOA,
												  @SEQPESSOA,
												  @SEQPROPRIEDADE,
												  @REFERENCIA,
												  @IDENTIFICADOR,
												  @ATIVO,
												  @NOTAS,
												  @CAMPO1,
												  @CAMPO2,
												  @CAMPO3,
												  @CAMPO4,
												  @CAMPO5,
												  @CAMPO6,
												  @NUMERO1,
												  @NUMERO2,
												  @NUMERO3,
												  @NUMERO4,
												  @NUMERO5,
												  @NUMERO6,
												  @DATA1,
												  @DATA2,
												  @DATA3,
												  @DATA4,
												  @DATA5,
												  @DATA6,
												  @SIMNAO1,
												  @SIMNAO2,
												  @SIMNAO3,
												  @SIMNAO4,
												  @SIMNAO5,
												  @SIMNAO6,
												  @LITERAL1,
												  @LITERAL2,
												  @LITERAL3,
												  @LITERAL4,
												  @LITERAL5,
												  @LITERAL6,
												  @LITERAL7,
												  @LITERAL8,
												  @LITERAL9,
												  @LITERAL10,
												  @CODORIGEM,
												  @ULTORIGEM,
												  @DTAINCLUSAO,
												  @USUINCLUSAO,
												  @DTAALTERACAO,
												  @USUALTERACAO,
												  @CAMPO7,
												  @CAMPO8,
												  @CAMPO7SQL,
												  @CAMPO8SQL

	WHILE (@@FETCH_STATUS = 0)
	BEGIN

				execute dbo.PR_COL_MOVPROPR
						   @SEQPROPPESSOA,
							@SEQPESSOA,
							@SEQPROPRIEDADE,
							@REFERENCIA,
							@IDENTIFICADOR,
							@ATIVO,
							@NOTAS,
							@CAMPO1,
							@CAMPO2,
							@CAMPO3,
							@CAMPO4,
							@CAMPO5,
							@CAMPO6,
							@NUMERO1,
							@NUMERO2,
							@NUMERO3,
							@NUMERO4,
							@NUMERO5,
							@NUMERO6,
							@DATA1,
							@DATA2,
							@DATA3,
							@DATA4,
							@DATA5,
							@DATA6,
							@SIMNAO1,
							@SIMNAO2,
							@SIMNAO3,
							@SIMNAO4,
							@SIMNAO5,
							@SIMNAO6,
							@LITERAL1,
							@LITERAL2,
							@LITERAL3,
							@LITERAL4,
							@LITERAL5,
							@LITERAL6,
							@LITERAL7,
							@LITERAL8,
							@LITERAL9,
							@LITERAL10,
							@CODORIGEM,
							@ULTORIGEM,
							@DTAINCLUSAO,
							@USUINCLUSAO,
							@DTAALTERACAO,
							@USUALTERACAO,
							@CAMPO7,
							@CAMPO8,
							@CAMPO7SQL,
							@CAMPO8SQL,
						    'INTEGRAÇÃONOROESTE',
						    Null


		FETCH NEXT FROM curPES INTO 			  @SEQPROPPESSOA,
												  @SEQPESSOA,
												  @SEQPROPRIEDADE,
												  @REFERENCIA,
												  @IDENTIFICADOR,
												  @ATIVO,
												  @NOTAS,
												  @CAMPO1,
												  @CAMPO2,
												  @CAMPO3,
												  @CAMPO4,
												  @CAMPO5,
												  @CAMPO6,
												  @NUMERO1,
												  @NUMERO2,
												  @NUMERO3,
												  @NUMERO4,
												  @NUMERO5,
												  @NUMERO6,
												  @DATA1,
												  @DATA2,
												  @DATA3,
												  @DATA4,
												  @DATA5,
												  @DATA6,
												  @SIMNAO1,
												  @SIMNAO2,
												  @SIMNAO3,
												  @SIMNAO4,
												  @SIMNAO5,
												  @SIMNAO6,
												  @LITERAL1,
												  @LITERAL2,
												  @LITERAL3,
												  @LITERAL4,
												  @LITERAL5,
												  @LITERAL6,
												  @LITERAL7,
												  @LITERAL8,
												  @LITERAL9,
												  @LITERAL10,
												  @CODORIGEM,
												  @ULTORIGEM,
												  @DTAINCLUSAO,
												  @USUINCLUSAO,
												  @DTAALTERACAO,
												  @USUALTERACAO,
												  @CAMPO7,
												  @CAMPO8,
												  @CAMPO7SQL,
												  @CAMPO8SQL
						
	END

	CLOSE curPES
	DEALLOCATE curPES

End



