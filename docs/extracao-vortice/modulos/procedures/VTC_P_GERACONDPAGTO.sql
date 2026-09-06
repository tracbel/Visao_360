/* ==============================================================
   Objeto ..........: dbo.VTC_P_GERACONDPAGTO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-09-11 17:56:28
   Modificado em ...: 2026-03-12 08:56:31
   Linhas ..........: 264
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : GEP_IMPORT, GEP_IMPORTAPROVACAO, EXT_CONDPAGTO
   Tabelas referidas: EXT_CONDPAGTO, GE_USUARIO, GEP_IMPORT, GEP_IMPORTAPROVACAO, IV_ACAO, IV_HISTORICO, IV_OPERADOR, IV_PROCDADO
   Outras refs .....: IV_Q$PEDIDO_KAM, PRC_GET_SEQUENCIA_TABELA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[VTC_P_GERACONDPAGTO] 
AS
BEGIN
    SET NOCOUNT ON;

    -- Declaração da tabela temporária para armazenar os dados intermediários
    DECLARE @Processo NVARCHAR(50), @SeqPessoa INT, @Quantidade NUMERIC(10,2), @i INT, @Resumo varchar(100), @vnProcesso NUMERIC(18, 0);
	DECLARE @VALOR NUMERIC(18, 2), @CODUSUARIO varchar(20), @PROCESSOPAI NUMERIC(18, 0), @PROCESSODNA NUMERIC(18, 0), @CODPROCESSO NUMERIC(4, 0);
	DECLARE @HISTORICOORIGEM NUMERIC(18, 0), @VENDEDOR varchar(20), @FORMAPRIMCONT varchar(20), @ATIVORECEPTIVO char(1) , @MOTIVO varchar(40) ;
	DECLARE @ORIGEM VARCHAR(20), @ULTRESULTADO INT, @DTAULTRESULTADO DATETIME, @DTAGERACAO DATETIME, @USUGERACAO VARCHAR (20), @NROEMPRESA INT, @VS VARCHAR(2);
	DECLARE @DTAPRIMRESULTADO DATETIME, @DETALHE varchar(1000), @USUINCLUSAO varchar(20),  @SEQUSUARIO INT, @vnSeqagenda NUMERIC(18, 0);
	DECLARE @TIPO VARCHAR(20), @MARCA VARCHAR(20), @MODELO VARCHAR(20), @GERENTE VARCHAR(20), @SEQGERENTE INT
	DECLARE @ASSUNTO VARCHAR(50)

    -- Cursor para iterar sobre os registros da IV_HISTORICO com resultado 3657 e dentro do período
    DECLARE CURHIS CURSOR FOR
    SELECT H.PROCESSO, 
		   H.SEQPESSOA, 
		   H.NROEMPRESA,
		   V.QUANTIDADE,
		   ---isnull(V.VENDA_VALOR_TOTAL, 0 ) as VENDA_VALOR_TOTAL,
		   V.USUINCLUSAO,
		   PPD.PROCESSOPAI,
		   PPD.PROCESSODNA,
		   PPD.CODPROCESSO,
		   H.SEQHISTORICO AS HISTORICOORIGEM,
		   H.VENDEDOR,
		   H.FORMAPRIMCONT,
		   PPD.ATIVORECEPTIVO,
		   PPD.MOTIVO,
		   PPD.ORIGEM,
		   H.RESULTADO AS ULTRESULTADO,
		   H.DTAREALIZACAO AS DTAULTRESULTADO,
		   GETDATE() AS DTAGERACAO,
		   'VTCCONS' AS USUGERACAO,
		   PPD.DTAPRIMRESULTADO,
		   H.SEQUSUARIO,
		   ---V.VENDA_TIPO_EQUIPAMEN AS TIPO,
		   ---V.VENDA_MARCA_EQUIPAME AS MARCA,
		   V.MODELO_DO_PRODUTO AS MODELO,
		   ATD.Gerente,
		   USU.SEQUSUARIO AS SEQGERENTE
    FROM IV_HISTORICO H
---	JOIN IV_AGENDA AGE ON AGE.PROCESSO = H.PROCESSO AND AGE.REALIZADA = 'N'
    JOIN IV_Q$PEDIDO_KAM V ON H.PROCESSO = V.PROCESSO AND V.QUANTIDADE>0
	JOIN IV_PROCDADO PPD ON PPD.PROCESSO = H.PROCESSO
	LEFT JOIN IV_OPERADOR ATD ON ATD.SeqUsuario = H.SeqUsuario
	LEFT JOIN GE_USUARIO USU ON USU.CODUSUARIO = ATD.Gerente
    WHERE 1=1
---- executado até 04/12/2025      AND H.RESULTADO = 3657  --- Resultado Venda Direta KAM , da açao 768 Vender Equipamentos Novos
---- abaixo modificado a pedido do cliente para o resultado abaixo.
      AND H.RESULTADO = 3686  --- Novo Resultado Alocado
	  AND NOT EXISTS ( SELECT 1 FROM EXT_CONDPAGTO WHERE PROCESSODNA = ppd.PROCESSODNA  )
---- executado até 04/12/2025      	  	  AND H.DTAINCLUSAO > DATEADD(HOUR, -2,  '2025-07-24 07:00:00' )
	  	  AND H.DTAINCLUSAO > DATEADD(HOUR, -2,  '2025-12-05 07:00:00' )
		  ;

    set @RESUMO = 'Vender veiculo'
	SET @VS = ';'
	
	OPEN CURHIS;
    FETCH NEXT FROM CURHIS INTO  @Processo, @SeqPessoa, @NROEMPRESA, @Quantidade, 
		  ---@VALOR, 
		  @USUINCLUSAO, @PROCESSOPAI, 
	      @PROCESSODNA, @CODPROCESSO , @HISTORICOORIGEM, @VENDEDOR, @FORMAPRIMCONT, @ATIVORECEPTIVO, @MOTIVO, @ORIGEM, 
		  @ULTRESULTADO, @DTAULTRESULTADO, @DTAGERACAO, @USUGERACAO, @DTAPRIMRESULTADO, @SEQUSUARIO,
		  ---@TIPO, @MARCA , 
		  @MODELO , @GERENTE, @SEQGERENTE

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @i = 1;
        WHILE @i <= @Quantidade
        BEGIN
		--- pega sequencia PROCESSO
		    if @Quantidade=1 
			begin
				set @vnProcesso = @PROCESSO
			end
			else
			begin
				EXEC dbo.PRC_GET_SEQUENCIA_TABELA
					 @sNomeTabela = 'IV_PROCESSO',
					 @vnSeqNovo = @vnProcesso OUTPUT,
					 @sTabelaOrigem = 'IV_PROCESSO',
					 @sCampoOrigem = 'PROCESSO'
			end
		    set @DETALHE = 'Pedido de Venda (' +   cast ( @i as varchar ) + ' de ' + cast ( cast(@Quantidade as int) as varchar ) + ') '
			set @DETALHE = @DETALHE + char(13) + char(10) 
			set @DETALHE = @DETALHE + '  Modelo: ' + isnull(@MODELO, ' ' )
		
		    if @Quantidade>1 
			begin
		            -- Inserir primeira linha (IV_PROCESSO E IV_PROCDADO)
		            INSERT INTO GEP_IMPORT ( PROCESSO, ORIGEM, ACAO, TABELA, SEPARADOR, STATUS, COLUNAIDENTIFIC, DADOIDENTIFICADOR, DTAGERACAO, COLUNA, DADO )
		            VALUES (
		                'CRM', 
						'CRMTRAC', 
						'I', 
						'IV_PROCESSO', 
						';', 
						NULL,
		                'PROCESSO', 
						cast ( @vnProcesso as varchar),
						GETDATE(),
						'PROCESSO; DESCRICAO; USUINCLUSAO; DTAINCLUSAO; QTDE; USURESPONSAVEL; DTAALTERACAO; USUALTERACAO; REALIZADO; DTAFASE; FASE; STATUS;'+
						'PRIORIDADE',
						cast ( @vnProcesso as varchar) + @VS + @RESUMO + @VS + @USUINCLUSAO + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
						CAST( @Quantidade AS VARCHAR) + @VS + isnull(@GERENTE, '') + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
						@USUINCLUSAO + @VS + '0' + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
						'Apresentação' + @VS + 'EM ABERTO' + @VS + '2'
		            );
		
					---- FAZENDO BACKUP DO PROCESSO DA INSERCAO NA GEP_IMPORT
		            INSERT INTO GEP_IMPORTAPROVACAO ( PROCESSO, ORIGEM, ACAO, TABELA, SEPARADOR, STATUS, COLUNAIDENTIFIC, DADOIDENTIFICADOR, DTAGERACAO, COLUNA, DADO )
		            VALUES (
		                'CRM', 
						'CRMTRAC', 
						'I', 
						'IV_PROCESSO', 
						';', 
						NULL,
		                'PROCESSO', 
						cast ( @vnProcesso as varchar),
						GETDATE(),
						'PROCESSO; DESCRICAO; USUINCLUSAO; DTAINCLUSAO; QTDE; USURESPONSAVEL; DTAALTERACAO; USUALTERACAO; REALIZADO; DTAFASE; FASE; STATUS;'+
						'PRIORIDADE',
						cast ( @vnProcesso as varchar) + @VS + @RESUMO + @VS + @USUINCLUSAO + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
						CAST( @Quantidade AS VARCHAR) + @VS + isnull(@GERENTE, '') + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
						@USUINCLUSAO + @VS + '0' + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
						'Apresentação' + @VS + 'EM ABERTO' + @VS + '2'
		            );
		
					---- FAZENDO INSERCAO DA PROCDADO NA GEP_IMPORT
		            INSERT INTO GEP_IMPORT ( PROCESSO, ORIGEM, ACAO, TABELA, SEPARADOR, STATUS, COLUNAIDENTIFIC, DADOIDENTIFICADOR, DTAGERACAO, COLUNA, DADO )
		            VALUES (
		                'CRM', 
						'CRMTRAC', 
						'I', 
						'IV_PROCDADO', 
						';', 
						NULL,
						'PROCESSO',
						cast ( @vnProcesso as varchar),
						GETDATE(),
						'PROCESSO; NROEMPRESA; PROCESSOPAI; PROCESSODNA; CODPROCESSO; SEQPESSOA; VENDEDOR; FORMAPRIMCONT; ATIVORECEPTIVO; MOTIVO; ORIGEM; ULTRESULTADO; DTAULTRESULTADO; DTAGERACAO; USUGERACAO; DTAPRIMRESULTADO ',
						cast ( @vnProcesso as varchar) + @VS +cast ( @NROEMPRESA as varchar) + @VS +cast ( @PROCESSOPAI as varchar) + @VS +
						cast ( @PROCESSODNA as varchar) + @VS + cast ( @CODPROCESSO as varchar) + @VS + cast ( @SEQPESSOA as varchar) + @VS + 
						@VENDEDOR + @VS + ISNULL(@FORMAPRIMCONT, '') + @VS + ISNULL(@ATIVORECEPTIVO, '') + @VS + ISNULL(@MOTIVO, '') + @VS + @ORIGEM + 
						@VS + cast ( @ULTRESULTADO as varchar) + @VS +  
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAULTRESULTADO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS +
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS +
						@USUGERACAO + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAPRIMRESULTADO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' 
		            );
		
					---- FAZENDO BACKUP DO PROCDADO DA INSERCAO NA GEP_IMPORT
		            INSERT INTO GEP_IMPORTAPROVACAO ( PROCESSO, ORIGEM, ACAO, TABELA, SEPARADOR, STATUS, COLUNAIDENTIFIC, DADOIDENTIFICADOR, DTAGERACAO, COLUNA, DADO )
		            VALUES (
		                'CRM', 
						'CRMTRAC', 
						'I', 
						'IV_PROCDADO', 
						';', 
						NULL,
		                'PROCESSO', 
						cast ( @vnProcesso as varchar),
						GETDATE(),
						'PROCESSO; NROEMPRESA; PROCESSOPAI; PROCESSODNA; CODPROCESSO; SEQPESSOA; VENDEDOR; FORMAPRIMCONT; ATIVORECEPTIVO; MOTIVO; ORIGEM; ULTRESULTADO; DTAULTRESULTADO; DTAGERACAO; USUGERACAO; DTAPRIMRESULTADO ',
						cast ( @vnProcesso as varchar) + @VS +cast ( @NROEMPRESA as varchar) + @VS +cast ( @PROCESSOPAI as varchar) + @VS +
						cast ( @PROCESSODNA as varchar) + @VS + cast ( @CODPROCESSO as varchar) + @VS + cast ( @SEQPESSOA as varchar) + @VS + 
						@VENDEDOR + @VS + ISNULL(@FORMAPRIMCONT, '') + @VS + ISNULL(@ATIVORECEPTIVO, '') + @VS + ISNULL(@MOTIVO, '') + @VS + @ORIGEM + 
						@VS + cast ( @ULTRESULTADO as varchar) + @VS +  
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAULTRESULTADO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS +
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS +
						@USUGERACAO + @VS + 
						'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAPRIMRESULTADO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' 
		            );
			end
            -- Inserir segunda linha (IV_AGENDA)

		--- pega sequencia AGENDA
			EXEC dbo.PRC_GET_SEQUENCIA_TABELA
				 @sNomeTabela = 'IV_AGENDA',
				 @vnSeqNovo = @vnSeqagenda OUTPUT,
				 @sTabelaOrigem = 'IV_AGENDA',
				 @sCampoOrigem = 'SEQAGENDA'

		   SELECT @ASSUNTO = DESCRICAO FROM IV_ACAO WHERE ACAO = 823
		   set @ASSUNTO = isnull (@ASSUNTO, '')

            INSERT INTO GEP_IMPORT ( PROCESSO, ORIGEM, ACAO, TABELA, SEPARADOR, STATUS, COLUNAIDENTIFIC, DADOIDENTIFICADOR, DTAGERACAO, COLUNA, DADO )
            VALUES (
                'CRM', 
				'CRMTRAC', 
				'I', 
				'IV_AGENDA', 
				@VS, 
				NULL,
                'SEQAGENDA', 
				CAST ( @vnSeqagenda as varchar),
				GETDATE(),
				'SEQAGENDA;SEQUSUARIO; SEQPESSOA; TIPOAGENDAMENTO; TAREFACOMPROMISSO; NROEMPRESA; CODPROCESSO; ACAO; ASSUNTO; ASSUNTOCMPL; CLASSE; DTAAGENDA; DTAAGENDAORIGINAL; PRIORIDADE; DETALHE; HISTORICOORIGEM; USUGEROUACAO; DTAGERACAO; REALIZADA; VENDEDOR; PROCESSO; DTAGERACAOORIG; STATUS',
				CAST ( @vnSeqagenda as varchar) + @VS + 
				CAST ( isnull(@SEQGERENTE, @SEQUSUARIO) as varchar) + @VS + CAST ( @SeqPessoa AS VARCHAR) + @VS + 'A' + @VS + 'T' + @VS + 
				CAST ( @NROEMPRESA AS varchar)  + @VS + CAST( @CODPROCESSO AS VARCHAR) + @VS + '823' + @VS + @ASSUNTO + @VS + 
				'Pedido KAM' + @VS + 'X' + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( GETDATE(), 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( GETDATE(), 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
				'2' + @VS + @DETALHE + @VS + 
				CAST (@HISTORICOORIGEM AS varchar) + @VS + @USUGERACAO + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 'N' + @VS + 
				isnull(@VENDEDOR, '') + @VS + CAST (@vnProcesso AS varchar) + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 'C'
            );

			---- FAZENDO BACKUP DA AGENDA DA INSERCAO NA GEP_IMPORT
            INSERT INTO GEP_IMPORTAPROVACAO ( PROCESSO, ORIGEM, ACAO, TABELA, SEPARADOR, STATUS, COLUNAIDENTIFIC, DADOIDENTIFICADOR, DTAGERACAO, COLUNA, DADO )
            VALUES (
                'CRM', 
				'CRMTRAC', 
				'I', 
				'IV_AGENDA', 
				@VS, 
				NULL,
                'SEQAGENDA', 
				CAST ( @vnSeqagenda as varchar),
				GETDATE(),
				'SEQAGENDA;SEQUSUARIO; SEQPESSOA; TIPOAGENDAMENTO; TAREFACOMPROMISSO; NROEMPRESA; CODPROCESSO; ACAO; ASSUNTO; ASSUNTOCMPL; CLASSE; DTAAGENDA; DTAAGENDAORIGINAL; PRIORIDADE; DETALHE; HISTORICOORIGEM; USUGEROUACAO; DTAGERACAO; REALIZADA; VENDEDOR; PROCESSO; DTAGERACAOORIG; STATUS',
				CAST ( @vnSeqagenda as varchar) + @VS + 
				CAST ( isnull(@SEQGERENTE, @SEQUSUARIO) as varchar) + @VS + CAST ( @SeqPessoa AS VARCHAR) + @VS + 'A' + @VS + 'T' + @VS + 
				CAST ( @NROEMPRESA AS varchar)  + @VS + CAST( @CODPROCESSO AS VARCHAR) + @VS + '823' + @VS + @ASSUNTO + @VS + 
				'Pedido KAM' + @VS + 'X' + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( GETDATE(), 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( GETDATE(), 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 
				'2' + @VS + @DETALHE + @VS + 
				CAST (@HISTORICOORIGEM AS varchar) + @VS + @USUGERACAO + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 'N' + @VS + 
				isnull(@VENDEDOR, '') + @VS + CAST (@vnProcesso AS varchar) + @VS + 
				'CONVERT( DATETIME, ' + CHAR(39) + format ( @DTAGERACAO, 'dd-MM-yyyy HH:mm:ss') + CHAR(39)  + ', 103)' + @VS + 'C'
            );
            SET @i = @i + 1;
        END
		INSERT INTO EXT_CONDPAGTO ( PROCESSO, PROCESSODNA, DTAGERACAO ) VALUES ( @vnProcesso,  @PROCESSODNA, GETDATE() )

		FETCH NEXT FROM CURHIS INTO  @Processo, @SeqPessoa, @NROEMPRESA, @Quantidade, 
		  ---@VALOR, 
		  @USUINCLUSAO, @PROCESSOPAI, 
	      @PROCESSODNA, @CODPROCESSO , @HISTORICOORIGEM, @VENDEDOR, @FORMAPRIMCONT, @ATIVORECEPTIVO, @MOTIVO, @ORIGEM, 
		  @ULTRESULTADO, @DTAULTRESULTADO, @DTAGERACAO, @USUGERACAO, @DTAPRIMRESULTADO, @SEQUSUARIO,
		  ---@TIPO, @MARCA , 
		  @MODELO , @GERENTE, @SEQGERENTE
    END

    CLOSE CURHIS;
    DEALLOCATE CURHIS;
END;
