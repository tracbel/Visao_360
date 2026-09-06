/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGPESEND
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 16:36:49
   Modificado em ...: 2024-12-28 16:36:49
   Linhas ..........: 174
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOA_ITA, GE_PESSOAEND, GE_PESSOAEND_ITA, GE_PESSOALINK, MIG_GE_PESSOA
   Outras refs .....: PR_COL_MOVEND
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MIGPESEND] 
AS


/*
-- autor: AMAURY 13/11/2024
   objetivo: Migrar enderecos das pessoas com exceção das ativas com cnpj existentes
   log de alterações: 13/11/2024 - criacao da rotina
*/	

	DECLARE 
	@SeqPessoa 	numeric(10, 0)  ,
	@SeqPessoaEnd 	decimal(3, 0)  ,
	@SeqCidade 	decimal(6, 0) ,
	@TipoEndereco 	char(1)  ,
	@Cidade 	varchar(50) ,
	@Uf 		varchar(2) ,
	@SeqBairro 	decimal(5, 0) ,
	@Bairro 	varchar(50) ,
	@TipoLogradouro varchar(15) ,
	@Logradouro 	varchar(80) ,
	@NroLogradouro 	varchar(10) ,
	@CmpltoLogradouro varchar(30) ,
	@CxPostal 	varchar(7) ,
	@SeqPessoaEndCobr decimal(3, 0) ,
	@Cep 		varchar(12) ,
	@Pais 		varchar(25) ,
	@DtaAlteracao 	datetime  ,
	@UsuAlteracao 	varchar(20)  ,
	@RefEndereco 	varchar(150) ,
	@Latitude 	decimal(14, 11) ,
	@Longitude 	decimal(14, 11) ,
	@Descricao 	varchar(100) ,
	@SeqRegiao 	decimal(6, 0) ,
	@SeqRota 	decimal(6, 0) ,
	@CHAVEADICIONAL varchar(30) ,
	@INSCPRODUTOR 	varchar(20)

	Declare curPES
	CURSOR
	FOR
		SELECT 
			       LK.SeqPessoa
				  ,isnull(PEND.SeqPessoaEnd, 0) + isnull(( SELECT MAX(SEQPESSOAEND) AS QTD FROM GE_PESSOAEND WHERE SEQPESSOA = LK.SEQPESSOA ), 0) AS SEQPESSOAEND
				  ,PEND.SeqCidade
				  ,PEND.TipoEndereco
				  ,PEND.Cidade
				  ,PEND.Uf
				  ,PEND.SeqBairro
				  ,PEND.Bairro
				  ,PEND.TipoLogradouro
				  ,PEND.Logradouro
				  ,PEND.NroLogradouro
				  ,PEND.CmpltoLogradouro
				  ,PEND.CxPostal
				  ,PEND.SeqPessoaEndCobr
				  ,PEND.Cep
				  ,PEND.Pais
				  ,PEND.DtaAlteracao
				  ,PEND.UsuAlteracao
				  ,PEND.RefEndereco
				  ,PEND.Latitude
				  ,PEND.Longitude
				  ,PEND.Descricao
				  ,PEND.SeqRegiao
				  ,PEND.SeqRota
				  ,PEND.CHAVEADICIONAL
				  ,PEND.INSCPRODUTOR
	  FROM MIG_GE_PESSOA MP
		JOIN GE_PESSOA_ITA PI ON PI.SEQPESSOA = MP.SEQPESSOA
		JOIN GE_PESSOAEND_ITA PEND ON PEND.SeqPessoa = PI.SeqPessoa
		JOIN GE_PESSOALINK LK ON LK.Pessoalink = CAST( PI.SEQPESSOA AS VARCHAR) 
		                     AND LK.Origem = 'INTEGRAÇÃONOROESTE'
        JOIN GE_PESSOA PES ON PES.SEQPESSOA = LK.SEQPESSOA
		WHERE 1=1
		  AND MP.DESCRICAO  in ( 'Ativo com CNPJ Inexistente', 'Prospect com CNPJ Inexistente', 'Prospect com CNPJ<>0 Inexistente', 'Prospect sem CNPJ' )
begin

	OPEN curPES
		FETCH NEXT FROM curPES INTO 		
									@SeqPessoa,
									@SeqPessoaEnd,
									@SeqCidade,
									@TipoEndereco,
									@Cidade,
									@Uf,
									@SeqBairro,
									@Bairro,
									@TipoLogradouro,
									@Logradouro,
									@NroLogradouro,
									@CmpltoLogradouro,
									@CxPostal,
									@SeqPessoaEndCobr,
									@Cep,
									@Pais,
									@DtaAlteracao,
									@UsuAlteracao,
									@RefEndereco,
									@Latitude,
									@Longitude,
									@Descricao,
									@SeqRegiao,
									@SeqRota,
									@CHAVEADICIONAL,
									@INSCPRODUTOR


	WHILE (@@FETCH_STATUS = 0)
	BEGIN

				EXECUTE [dbo].[PR_COL_MOVEND]
									@SeqPessoa,
									@SeqPessoaEnd,
									@SeqCidade,
									@TipoEndereco,
									@Cidade,
									@Uf,
									@SeqBairro,
									@Bairro,
									@TipoLogradouro,
									@Logradouro,
									@NroLogradouro,
									@CmpltoLogradouro,
									@CxPostal,
									@SeqPessoaEndCobr,
									@Cep,
									@Pais,
									@DtaAlteracao,
									@UsuAlteracao,
									@RefEndereco,
									@Latitude,
									@Longitude,
									@Descricao,
									@SeqRegiao,
									@SeqRota,
									@CHAVEADICIONAL,
									@INSCPRODUTOR,
									'INTEGRAÇÃONOROESTE'

		FETCH NEXT FROM curPES INTO 		
									@SeqPessoa,
									@SeqPessoaEnd,
									@SeqCidade,
									@TipoEndereco,
									@Cidade,
									@Uf,
									@SeqBairro,
									@Bairro,
									@TipoLogradouro,
									@Logradouro,
									@NroLogradouro,
									@CmpltoLogradouro,
									@CxPostal,
									@SeqPessoaEndCobr,
									@Cep,
									@Pais,
									@DtaAlteracao,
									@UsuAlteracao,
									@RefEndereco,
									@Latitude,
									@Longitude,
									@Descricao,
									@SeqRegiao,
									@SeqRota,
									@CHAVEADICIONAL,
									@INSCPRODUTOR
					
	END

	CLOSE curPES
	DEALLOCATE curPES

End