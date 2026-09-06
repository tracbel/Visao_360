/* ==============================================================
   Objeto ..........: dbo.PR_PROPRIEDADE
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2017-04-07 11:15:39
   Modificado em ...: 2018-03-20 13:13:34
   Linhas ..........: 414
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : gep_import
   Tabelas referidas: EXT_VEIC, EXT_VEICFAM, EXT_VEICMARCA, EXT_VEICMODELO, EXT_VEICPROP, gep_import, IV_CLIENTEPROPR
   Outras refs .....: f_s_dado, f_s_dado_N
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE procedure [dbo].[PR_PROPRIEDADE] 
AS
	DECLARE 
	@vnIdVeic		 numeric,
	@vsMarca		 varchar(20),
	@vnSeqPropriedade   numeric,
	@vsFamilia		 varchar(30),
	@vsModelo varchar(40),
	@vsAnoModelo as varchar(4),
	@vsEstadoVenda as varchar(5),
	@vsAtivo CHAR(1),
	@vsChassi	 varchar(30),
	@vsNotas	varchar(60),
	@vnIndPropAtual  int,
	@vnSeqPessoa	 numeric,
	@vnQtde int,

	@processo	varchar(20),
	@origem 	varchar(20),
	@acao		varchar(1),
	@tabela		varchar(30),
	@separador	varchar(1),
	@colunaidentific 	varchar(100),
	@dadoidentificador	varchar(200),
	@campos		varchar(1000),
	@dados		varchar(1000),
	@dtageracao datetime,
	@conteudo	varchar(250)


	Declare C01
	
	CURSOR
	FOR

SELECT PROP.IDVEIC
     , MRK.MARCA
     , FAM.SEQPROPRIEDADE
     , FAM.DESCRICAO AS FAMILIA
     , MDL.DESCRICAO AS MODELO
	 , VEIC.ANOMODELO
     , VEIC.CHASSI
     , PROP.INDPROPATUAL
	 , PROP.ESTADOVENDA
	 , PROP.NRONF
     , PROP.SEQPESSOA     
  FROM EXT_VEICPROP PROP
     JOIN EXT_VEIC VEIC ON VEIC.IDVEIC = PROP.IDVEIC
     JOIN EXT_VEICMODELO MDL ON MDL.IDVEICMODELO = VEIC.IDVEICMODELO
     JOIN EXT_VEICFAM FAM ON FAM.IDVEICFAMILIA = MDL.IDVEICFAMILIA
     JOIN EXT_VEICMARCA MRK ON MRK.IDVEICMARCA = FAM.IDVEICMARCA
WHERE 1 = 1
--AND VEIC.Chassi = 'PCSRB9C592728'
AND NOT EXISTS ( SELECT 1 
		  FROM IV_CLIENTEPROPR CPR
		 WHERE CPR.SEQPROPRIEDADE = FAM.SEQPROPRIEDADE
		 AND CPR.IDENTIFICADOR = VEIC.CHASSI
		 AND CPR.ATIVO = CASE PROP.INDPROPATUAL WHEN 1 THEN 'S' ELSE 'N' END )

Begin
	OPEN C01
		FETCH NEXT FROM C01 INTO 	
		@vnIdVeic,
		@vsMarca,
		@vnSeqPropriedade,
		@vsFamilia,
		@vsModelo,
		@vsAnoModelo,
		@vsChassi,
		@vnIndPropAtual,
		@vsEstadoVenda,
		@vsNotas,
		@vnSeqPessoa

	WHILE (@@FETCH_STATUS = 0)
	BEGIN

		If @vnIndPropAtual > 0 
			Set @vsAtivo = 'S'
		Else
			Set @vsAtivo = 'N'
		
		
		set @campos = 'SEQPESSOA|SEQPROPRIEDADE|ATIVO|REFERENCIA|IDENTIFICADOR|DTAINCLUSAO|USUINCLUSAO|CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3|NOTAS'
		set @processo 	= 'VORTICOCRM'
		set @origem = 'Import'
		set @acao	= 'I'
		set @tabela	= 'GE_PESSOAPROPR'
		set @separador	= '|'
		set @colunaidentific 	= 'SEQPESSOA'
		set @dadoidentificador	= cast ( @vnSeqPessoa as varchar(19) )
		set @dtageracao	= getdate()

		set @dados = ''
	
		set @conteudo	= cast ( @vnSeqPessoa as varchar(19) )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado( cast ( @vnSeqPropriedade as varchar(19) )  )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado_N( isnull(@vsAtivo, ' ') )
		set @dados  	= @dados + RTrim(@conteudo)
	
		set @conteudo	= dbo.f_s_dado_N( isnull ( @vsMarca, ' ' ) )
		set @dados  	= @dados + @conteudo
	
		set @conteudo	= dbo.f_s_dado_N( isnull (@vsChassi, ' ' ) )
		set @dados  	= @dados + @conteudo

		set @conteudo = dbo.f_s_dado( cast (convert(varchar(8), getdate(), 112) as varchar(10) ) + ' 00:00:00' )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado( 'PR_PROPRIEDADE' )
		set @dados  	= @dados + RTrim(@conteudo)

	    set @vsEstadoVenda = case @vsEstadoVenda when 'N' then 'Novo' else 'Usado' end

		--Tratores
		if @vnSeqpropriedade = 9402
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsAnoModelo )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
			
		end

		--Implementos
		if @vnSeqpropriedade = 9406
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end

		--Colhedora de cana
		if @vnSeqpropriedade = 9411
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsAnoModelo )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end

		--Colheitadeira Grãos
		if @vnSeqpropriedade = 9403
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsAnoModelo )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end


		--Plantadeira
		if @vnSeqpropriedade = 9405
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsAnoModelo )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end

		--Plataforma Adicional
		if @vnSeqpropriedade = 9404
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsAnoModelo )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end

		--Pulverizador
		if @vnSeqpropriedade = 9410
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsAnoModelo )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end

		--Turf
		if @vnSeqpropriedade = 9409
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ISNULL(@vsAnoModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end

		--Agricultura de precisão
		if @vnSeqpropriedade = 9407
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ISNULL(@vsAnoModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
		end

		--Equipamentos
		if @vnSeqpropriedade = 16
		begin
		--'CAMPO2|LITERAL1|CAMPO1|NUMERO1|CAMPO6|CAMPO3'
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsModelo, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @vsFamilia, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( '1' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsAnoModelo )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( @vsEstadoVenda )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( 'Número da NF: ' + @vsNotas, ' ') )
			set @dados  	= @dados + @conteudo
			
		end

		
		insert into gep_import (processo, origem, acao, tabela, separador, coluna, dado, colunaidentific, dadoidentificador, dtageracao) 
			values (@processo, @origem, @acao, @tabela, @separador, @campos, @dados, @colunaidentific, @dadoidentificador, @dtageracao )

		FETCH NEXT FROM C01 INTO
		@vnIdVeic,
		@vsMarca,
		@vnSeqPropriedade,
		@vsFamilia,
		@vsModelo,
		@vsAnoModelo,
		@vsChassi,
		@vnIndPropAtual,
		@vsEstadoVenda,
		@vsNotas,
		@vnSeqPessoa
						
	END

	CLOSE C01
	DEALLOCATE C01

End








