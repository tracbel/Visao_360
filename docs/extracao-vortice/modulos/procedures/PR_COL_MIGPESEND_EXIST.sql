/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGPESEND_EXIST
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-28 16:35:08
   Modificado em ...: 2024-12-28 16:35:08
   Linhas ..........: 178
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOA_ITA, GE_PESSOAEND, GE_PESSOAEND_ITA, MIG_GE_PESSOA
   Outras refs .....: PR_COL_MOVEND
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE  procedure [dbo].[PR_COL_MIGPESEND_EXIST] 
AS


/*
-- autor: AMAURY 13/11/2024
   objetivo: Migrar enderecos pessoas ativas com cnpj existentes
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
			       PES.SeqPessoa
				  ,PEND.SeqPessoaEnd + isnull( ( SELECT MAX(SEQPESSOAEND) AS QTD FROM GE_PESSOAEND WHERE SEQPESSOA = PES.SEQPESSOA ), 0) AS SEQPESSOAEND
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
		JOIN GE_PESSOA PES ON PES.NroCGCCPF = PI.NroCGCCPF
		JOIN GE_PESSOAEND_ITA PEND ON PEND.SeqPessoa = PI.SeqPessoa
		WHERE 1=1
		  AND MP.DESCRICAO = 'Ativo/Prospect CNPJ Existente'
		  AND NOT EXISTS (SELECT 1 FROM GE_PESSOAEND WHERE SEQPESSOA = PES.SEQPESSOA
		                                             AND TIPOENDERECO = PEND.TipoEndereco
													 AND Logradouro = PEND.Logradouro
													 AND CIDADE = PEND.CIDADE )
---		  AND ( PI.DtaInclusao >= PES.DtaInclusao or PI.DtaAlteracao >= PES.DtaAlteracao )
---		  and pi.SeqPessoa IN (869) --- (869)
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