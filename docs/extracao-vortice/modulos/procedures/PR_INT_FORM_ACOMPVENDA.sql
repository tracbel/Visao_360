/* ==============================================================
   Objeto ..........: dbo.PR_INT_FORM_ACOMPVENDA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2013-02-07 17:41:46
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 206
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : gep_import
   Tabelas referidas: gep_import, IV_CLIENTEPROPR, IV_GLOBALPAR, IV_HISTORICO
   Outras refs .....: f_s_dado, f_s_dado_N, IV_Q$ACOMPANH_VENDA_JDE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE procedure [dbo].[PR_INT_FORM_ACOMPVENDA] 
AS
	DECLARE 
	@nroempresa	varchar(2),
	@SeqPessoa	numeric(8,0),
	@TipoI	varchar(30),
	@NroNF	varchar(20),
	@UsuInclusao	varchar(20),
	@DtaInclusao	datetime,
	@Processo	varchar(20),
	@NroProcesso numeric(8, 0),
	@tipo_de_maquina varchar(30),
	@modelo_d_equipame varchar(30),
	@marca_equipamento varchar(30),
	@nro_chassi varchar(30),
	@tipo_Implemento	varchar(30),
	@desc_implemento varchar(50),
	@nro_nf	varchar(250),
	@seqpropriedade numeric(8, 0),
	@DtaRealizacao datetime,
	@potencia varchar(30),

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

	Declare curFORM
	
	CURSOR
	FOR
		SELECT *
		FROM (
		SELECT AV.SEQPESSOA
		  , AV.PROCESSO
		  , AV.TIPO_EQUIP1 
		  , AV.MODEL_EQUIP1 
		  , AV.MARCA_1 
		  , AV.NRO_CHASSI1 
		  , AV.NRO_NF1 
		  , 'VTCCONS' AS USUINCLUSAO
		  , GETDATE() AS DTAINCLUSAO
		  , HIS.NROEMPRESA,
			CASE AV.TIPO_EQUIP1
			WHEN 'Agricultura precisão' THEN 9407
			WHEN 'Colhedora de cana'	THEN 9411
			WHEN 'Colheitadeira grãos'	THEN 9403
			--WHEN 'Implementos'			THEN 9406
			WHEN 'Plantadeira'			THEN 9405
			WHEN 'Pulverizador'			THEN 9410
			WHEN 'Tratores'				THEN 9402
			ELSE 0
			END AS SEQPROPRIEDADE
		--  , AV.DESC_IMPLEMENTO
		, HIS.DTAREALIZACAO
		, GP.CAMPO2 AS POTENCIA
		FROM IV_Q$ACOMPANH_VENDA_JDE AV
		INNER JOIN IV_HISTORICO HIS ON HIS.PROCESSO = AV.PROCESSO
		LEFT JOIN IV_GLOBALPAR GP ON GP.SEQGLBPAR = 9401 AND GP.CAMPO1 = 'John Deere' AND GP.LITERAL1 = AV.MODEL_EQUIP1
		WHERE HIS.DTAREALIZACAO >= GETDATE()-1
			AND HIS.RESULTADO = 212
		) XA
		WHERE XA.SEQPROPRIEDADE <>0
		AND NOT EXISTS (SELECT 1 FROM IV_CLIENTEPROPR WHERE IDENTIFICADOR = XA.NRO_CHASSI1 OR ULTORIGEM = XA.PROCESSO)

begin
	OPEN curFORM
		FETCH NEXT FROM curFORM INTO 	@seqpessoa,
						@nroprocesso,
						@tipo_de_maquina,
						@modelo_d_equipame,
						@marca_equipamento,
						@nro_chassi,
						--@tipo_implemento,
						@nro_nf,
						@usuinclusao,
						@dtainclusao,
						@nroempresa,
						@seqpropriedade,
						--@desc_implemento,
						@dtarealizacao,
						@potencia

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		set @campos = 'SEQPESSOA|SEQPROPRIEDADE|ATIVO|REFERENCIA|IDENTIFICADOR|DTAINCLUSAO|USUINCLUSAO|CAMPO2|LITERAL1|CAMPO1|NOTAS|NUMERO1|CAMPO6|CAMPO3|ULTORIGEM'
		set @processo 	= 'VORTICOCRM'
		set @origem = 'Import'
		set @acao	= 'I'
		set @tabela	= 'GE_PESSOAPROPR'
		set @separador	= '|'
		set @colunaidentific 	= 'SEQPESSOA'
		set @dadoidentificador	= cast ( @SeqPessoa as varchar(19) )
		set @dtageracao	= getdate()

		set @dados = ''
	
		set @conteudo	= cast ( @SeqPessoa as varchar(19) )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado( cast ( @SeqPropriedade as varchar(19) )  )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado( 'S' )
		set @dados  	= @dados + RTrim(@conteudo)
	
		set @conteudo	= dbo.f_s_dado_N( isnull ( @marca_equipamento, ' ' ) )
		set @dados  	= @dados + @conteudo
	
		set @conteudo	= dbo.f_s_dado_N( isnull (@nro_chassi, ' ' ) )
		set @dados  	= @dados + @conteudo

		set @conteudo = dbo.f_s_dado( cast (convert(varchar(8), getdate(), 112) as varchar(10) ) + ' 00:00:00' )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado( @usuinclusao )
		set @dados  	= @dados + RTrim(@conteudo)

		if @seqpropriedade = 9406
		begin
			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @modelo_d_equipame, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @tipo_implemento, ' ') )
			set @dados  	= @dados + @conteudo

			set @nro_nf = @nro_nf + ' Descrição..: ' + isnull(@desc_implemento, ' ')
		end
		else
		begin
			set @conteudo	= dbo.f_s_dado_N( ISNULL ( @modelo_d_equipame, ' ') )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo

			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo
		end
		
		set @conteudo	= dbo.f_s_dado( 'Número da NF: ' + isnull(@nro_nf, ' ') )
		set @dados  	= @dados + RTrim(@conteudo)

		set @conteudo	= dbo.f_s_dado( '1' )
		set @dados  	= @dados + RTrim(@conteudo)

		if @seqpropriedade <> 9406 and @seqpropriedade <> 9407
		begin
			set @conteudo	= dbo.f_s_dado( cast ( year(@dtarealizacao) as varchar(4) ) )
			set @dados  	= @dados + RTrim(@conteudo)
		end
		else
		begin
			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo
		end

		if @seqpropriedade = 9402
		begin
			set @conteudo	= dbo.f_s_dado( isnull ( @potencia, ' ') )
			set @dados  	= @dados + @conteudo
		end
		else
		begin
			set @conteudo	= dbo.f_s_dado( ' ' )
			set @dados  	= @dados + @conteudo
		end

		set @conteudo	= dbo.f_s_dado( cast (@nroprocesso as varchar(8)) )
		set @dados  	= @dados + @conteudo

		insert into gep_import (processo, origem, acao, tabela, separador, coluna, dado, colunaidentific, dadoidentificador, dtageracao) 
			values (@processo, @origem, @acao, @tabela, @separador, @campos, @dados, @colunaidentific, @dadoidentificador, @dtageracao )
	
		FETCH NEXT FROM curFORM INTO 	@seqpessoa,
						@nroprocesso,
						@tipo_de_maquina,
						@modelo_d_equipame,
						@marca_equipamento,
						@nro_chassi,
						--@tipo_implemento,
						@nro_nf,
						@usuinclusao,
						@dtainclusao,
						@nroempresa,
						@seqpropriedade,
						--@desc_implemento,
						@dtarealizacao,
						@potencia
						
	END

	CLOSE curFORM
	DEALLOCATE curFORM

End