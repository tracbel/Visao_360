/* ==============================================================
   Objeto ..........: dbo.PR_INT_OS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2012-05-30 10:30:12
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 263
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : ext_os, ext_OSitem
   Tabelas referidas: EXT_OS, EXT_OSITEM
   Outras refs .....: fva_UnFormatCGCCPF
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_INT_OS] 
AS
	DECLARE @nroempresa	varchar(2),                     
	@nroos 		integer,				
	@nrocgccpf	varchar(20),				
	@digcgccpf	varchar(2),				
	@nrocgcfmt	varchar(19),				
	@nrocpffmt	varchar(19),				
	@consultor	varchar(20),				
	@codconsultor	integer,
	@tipo1   	integer,
	@tipo2		varchar(2),
	@tipoos		varchar(10),
	@status		varchar(10),
	@fisicajuridica varchar(1),
	@negocio	integer,
	@dtaabertura	varchar(10),				
	@dtafechamento	varchar(10),				
	@dtaaberturafmt	integer,				
	@dtafechamentofmt	integer,				
	@idpessoa	integer,				
	@kilometragem	integer,				
								
	@idOS		integer,
	@idos2		integer,				
	@idItem		integer,				
	@produtivo	varchar(100),					
	@codigo		varchar(40),					
	@descricao	varchar(100),					
	@qtde		numeric(15, 2),				
	@valorbruto	numeric(15, 2),				
	@valor		numeric(15, 2),				
								
	@conteudo	varchar(250),				
	@qtdOs		integer,				
	@nwOrigem       varchar(20),
	@nwSegmento	varchar(30),
	@nwSituacao	varchar(20),
	@niNroEmpresa   integer,
	@nwNroEmpresa   varchar(2),
	@obs		varchar(200),
	@usuario	varchar(20)
	
Declare curOS
	CURSOR
	FOR
	SELECT * FROM OPENQUERY(SPRESS, 
	'SELECT OS.FILIALCOD AS NROEMPRESA,
			OS.ORDGERNROOS AS NROOS,
			OS.ORDGERIDTATENDOS AS TIPO1,
			OS.ORDGERIDTCARAC AS TIPO2,
			OS.ORDGERDATABEROS AS DTAABERTURA,
			OS.ORDGERDATFECHAOS AS DTAFECHAMENTO,
			OS.ORDGERNROKILOMETRO AS KILOMETRAGEM,
			(SELECT CLIFISNROCPF FROM TGLCLIFIS CLF 
				INNER JOIN TGLCLIENT CLI ON CLI.CLIENTNRO = CLF.CLIENTNRO
									     AND CLI.CLIENTNRO = OS.CLIENTNRO) AS CPF,
			(SELECT CLIJURNROCGC FROM TGLCLIJUR CLJ
				INNER JOIN TGLCLIENT CLI ON CLI.CLIENTNRO = CLJ.CLIENTNRO
									     AND CLI.CLIENTNRO = OS.CLIENTNRO) AS CGC,
			FUN.FUNCIONOM AS CONSULTOR, 
			FUN.FUNCIOCOD AS CODCONSULTOR,
			ITE.ORDSERIDTSTOS AS STATUS,
			ITE.SERVICCOD AS CODIGO,
			ITE.ORDSERQTDRMO AS QTDE,
			ITE.ORDSERVLRPREVENDA AS VALORBRUTO,
			ITE.ORDSERVLRLIQSERV AS VALOR,
			SEV.SERVICDES AS DESCRICAO,
			TCLI.CLIENTIDTFISJUR AS FISICAJURIDICA,
			OS.ORDGERIDTSTOS AS TIPOOS,
			OS.ARENEGCOD AS NEGOCIO,
			OS.CLIENTNRO AS IDPESSOA,
			TFU.FUNCIONOM AS PRODUTIVO
	FROM T04ORDGER OS
	INNER JOIN TGLFUNCIO FUN ON FUN.FUNCIOCOD = OS.FUNCIOCOD
	INNER JOIN T04ORDSER ITE ON ITE.FILIALCOD = OS.FILIALCOD
							AND ITE.ORDGERNROOS = OS.ORDGERNROOS
	INNER JOIN T04SERVIC SEV ON SEV.SERVICCOD = ITE.SERVICCOD
	INNER JOIN TGLCLIENT TCLI ON TCLI.CLIENTNRO = OS.CLIENTNRO
	INNER JOIN TGLFUNCIO TFU ON TFU.FUNCIOCOD = 
(SELECT FIRST 1 REG.FUNCIOCOD FROM T04REGCDT REG 
                              WHERE REG.FILIALCOD = ITE.FILIALCOD
							    AND REG.ORDGERNROOS = ITE.ORDGERNROOS
							  ORDER BY REG.REGCDTDAT, REG.REGCDTHORINICIAL)
	WHERE OS.ORDGERIDTSTOS = 3
	  AND  OS.ORDGERDATABEROS >= (EXTRACT (YEAR FROM CURRENT_DATE-30) * 10000 +
EXTRACT (MONTH FROM CURRENT_DATE-30) * 100 +
EXTRACT (DAY FROM CURRENT_DATE-30))
	')


begin
	set @idos2 = 0

	OPEN curOS
		FETCH NEXT FROM curOS INTO 	@nroempresa,
						@nroos,
						@tipo1,
						@tipo2,
						@dtaaberturafmt,
						@dtafechamentofmt,
						@kilometragem,
						@nrocpffmt,
						@nrocgcfmt,
						@consultor,
						@codconsultor,
						@status,
						@codigo,
						@qtde,
						@valorbruto,
						@valor,
						@descricao,
						@fisicajuridica,
						@tipoos,
						@negocio,
						@idpessoa,
						@produtivo

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
	
		if @tipoos=3 and charindex(cast(@tipo1	as varchar)+'#', '1#3#5#')>0 and charindex(cast(@negocio as varchar)+'#', '20#30#40#50#')>0 
		BEGIN
			set @nwOrigem = 'COLORADO'
			set @usuario = 'VTCCONS'
			set @nwSituacao = 'NORMAL'
			set @nwSegmento = 'MÁQUINAS'
			set @niNroempresa = cast ( @nroempresa as integer )
			set @nwNroEmpresa = '01'
			if @niNroempresa=2 or @niNroEmpresa=11
				set @nwNroEmpresa = '02'
			if @niNroEmpresa=4 or @niNroEmpresa=8
				set @nwNroEmpresa = '03'
			if @niNroEmpresa=5
				set @nwNroEmpresa = '04'
			if @niNroEmpresa=9
				set @nwNroEmpresa = '05'
			if @niNroEmpresa=10 or @niNroEmpresa=3
				set @nwNroEmpresa = '06'
			SELECT @qtdOS = COUNT(*) 
			FROM EXT_OS
			WHERE NROEMPRESA = @nwNroEmpresa
			  AND NROOS = @nroOS

			if @qtdos>0
			begin
				SELECT @idos = IDOS
				FROM EXT_OS
				WHERE NROEMPRESA = @nwNroEmpresa
				  AND NROOS = @nroOS
				if @idos2<>@idos
				begin
					set @iditem=0
					set @idos2 = @idos
				end
			end

			if @qtdOS=0
			begin
				SELECT @IDOS = ISNULL(MAX(IDOS), 1) FROM EXT_OS

				SET @IDOS = @IDOS + 1
				SET @iditem = 0
				set @conteudo = ''
				if rtrim(@nrocgcfmt)<>'' 
					set @conteudo = @nrocgcfmt
				if rtrim(@nrocpffmt)<>'' 
					set @conteudo = @nrocpffmt
				set @conteudo = rTrim( dbo.fva_UnFormatCGCCPF( @conteudo ) )
				set @digcgccpf = right(@conteudo, 2)
				set @conteudo = @conteudo + '#'
				set @nrocgccpf = replace(@conteudo, @digcgccpf + '#', '')

				set @conteudo = cast (@dtaaberturafmt as varchar(10) )
				set @dtaabertura = substring(@conteudo, 5,2) + '/' + right(@conteudo, 2) + '/' + left(@conteudo, 4)

				if @dtafechamentofmt<>0
				begin
					set @conteudo = cast (@dtafechamentofmt as varchar)
					set @dtafechamento = substring(@conteudo, 5,2) + '/' + right(@conteudo, 2) + '/' + left(@conteudo, 4)
				end
				
				set @tipoos = substring('Externo   Interno   G.Fabrica G.Interna R.Externa R.Interna Seguradora', @tipo1 * 10 - 9, 10)
				
				set @conteudo=(case @tipo2  when '1' then 'Demonstração'
											when '2' then 'Revisão'
											when '3' then 'Departamento Revenda'
											when '4' then 'Garantia / Cortesia'
											when '5' then 'Revisão Gratuita Veic.Usado'
											when '6' then 'Box Rápido Motorcraft'
											when '7' then 'Express Service'
											when 'C' then 'Atendimento Concessionária'
											when 'F' then 'Atendimento Frotista'
											when 'I' then 'Atendimento com Retenção INSS'
											when 'N' then 'Agregar valor a novos'
											when 'P' then 'Atendimento Premium'
											when 'R' then 'Atendimento Revisão'
											when 'S' then 'Agregar valor semi-novos'
								end)
				
				set @obs = @tipoos + ' - ' + @conteudo
				
				if @dtafechamentofmt<>0
				begin
					insert into ext_os (IDOS, IDPESSOA, ORIGEM, PESSOALINK, PESSOALINKNRO, NROEMPRESA, NROOS, NROCGCCPF, 
									DIGCGCCPF, SEGMENTO, CONSULTOR, TIPOOS, DTAABERTURA, DTAFECHAMENTO, KILOMETRAGEM, OBSERVACAO, DTAIMPORT, USUARIO, DTAALTERACAO)
					VALUES (@idos, @idpessoa, @nworigem, @nwOrigem, @idpessoa, cast (@nwNroEmpresa as integer), @nroos, 
						cast (@nrocgccpf as numeric), cast(@digcgccpf as integer), @nwSegmento, @consultor, @tipoos, convert (datetime, @dtaabertura), convert(datetime, @dtafechamento) ,
						@kilometragem, @obs, GETDATE(), @usuario, GETDATE() )
				end
				if @dtafechamentofmt=0
				begin
					insert into ext_os (IDOS, IDPESSOA, ORIGEM, PESSOALINK, PESSOALINKNRO, NROEMPRESA, NROOS, NROCGCCPF, 
									DIGCGCCPF, SEGMENTO, CONSULTOR, TIPOOS, DTAABERTURA, KILOMETRAGEM, OBSERVACAO, DTAIMPORT, USUARIO, DTAALTERACAO)
					VALUES (@idos, @idpessoa, @nworigem, @nwOrigem, @idpessoa, cast (@nwNroEmpresa as integer), @nroos, 
						cast (@nrocgccpf as numeric), cast(@digcgccpf as integer), @nwSegmento, @consultor, @tipoos, convert (datetime, @dtaabertura), 
						@kilometragem, @obs, GETDATE(), @usuario, GETDATE() )
				end
			end
				
			SET @iditem = @iditem + 1

			SELECT @qtdOS = COUNT(*) 
			FROM EXT_OSITEM
			WHERE IDOS = @idOS
			  AND IDITEM = @idItem
			
			if @qtdOS=0
			begin
				insert into ext_OSitem (idOS, IDITEM, TIPOITEM, PRODUTIVO, STATUSITEM, CODIGO, DESCRICAO, QTDE,  VALORBRUTO, VALOR, DTAIMPORT)
				VALUES (@idos, @iditem, 'S', @produtivo, @status, @codigo, @descricao, 
					@qtde, @valorbruto, @valor, GETDATE())
			end
		END
		FETCH NEXT FROM curOS INTO 	@nroempresa,
						@nroos,
						@tipo1,
						@tipo2,
						@dtaaberturafmt,
						@dtafechamentofmt,
						@kilometragem,
						@nrocpffmt,
						@nrocgcfmt,
						@consultor,
						@codconsultor,
						@status,
						@codigo,
						@qtde,
						@valorbruto,
						@valor,
						@descricao,
						@fisicajuridica,
						@tipoos,
						@negocio,
						@idpessoa,
						@produtivo
						
	END

	CLOSE curOS
	DEALLOCATE curOS

End