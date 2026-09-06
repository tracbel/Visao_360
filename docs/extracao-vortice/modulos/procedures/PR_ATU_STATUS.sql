/* ==============================================================
   Objeto ..........: dbo.PR_ATU_STATUS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2015-07-27 17:47:48
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 68
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : gep_import
   Tabelas referidas: GE_PESSOA, gep_import, IV_HISTORICO, IV_RESULTADO
   Outras refs .....: f_s_dado
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE procedure [dbo].[PR_ATU_STATUS] 
AS
	DECLARE 
	@nroempresa	varchar(2),
	@SeqPessoa	numeric(8,0),
	@UsuInclusao	varchar(20),
	@DtaInclusao	datetime,

	@origem 	varchar(20),
	@acao		varchar(1),
	@tabela		varchar(30),
	@separador	varchar(1),
	@colunaidentific 	varchar(100),
	@dadoidentificador	varchar(200),
	@campos		varchar(1000),
	@dados		varchar(1000),
	@dtageracao datetime,
	@conteudo	varchar(250),
	@processo   varchar(20)

	Declare curFORM
	
	CURSOR
	FOR
		SELECT HIS.SEQPESSOA
		FROM IV_HISTORICO HIS
		INNER JOIN GE_PESSOA PES ON PES.SEQPESSOA = HIS.SEQPESSOA AND PES.STATUS <> 'A'
		WHERE HIS.RESULTADO IN ( SELECT RESULTADO FROM IV_RESULTADO
								  WHERE upper(DESCRICAO) LIKE 'FATURAMENTO%REALIZADO%' )
		  AND HIS.DTAREALIZACAO > GETDATE()-2		

begin
	OPEN curFORM
		FETCH NEXT FROM curFORM INTO 	@seqpessoa

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		set @campos = 'SEQPESSOA|STATUS'
		set @processo 	= 'VORTICOCRM'
		set @origem = 'Import'
		set @acao	= 'I'
		set @tabela	= 'GE_PESSOA'
		set @separador	= '|'
		set @colunaidentific 	= 'SEQPESSOA'
		set @dadoidentificador	= cast ( @SeqPessoa as varchar(19) )
		set @dtageracao	= getdate()

		set @dados = ''
	
		set @conteudo	= cast ( @SeqPessoa as varchar(19) )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado( 'A' )
		set @dados  	= @dados + RTrim(@conteudo)
	

		insert into gep_import (processo, origem, acao, tabela, separador, coluna, dado, colunaidentific, dadoidentificador, dtageracao) 
			values (@processo, @origem, @acao, @tabela, @separador, @campos, @dados, @colunaidentific, @dadoidentificador, @dtageracao )
	
		FETCH NEXT FROM curFORM INTO 	@seqpessoa
						
	END

	CLOSE curFORM
	DEALLOCATE curFORM

End