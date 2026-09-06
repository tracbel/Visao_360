/* ==============================================================
   Objeto ..........: dbo.PR_INT_PROPRNFS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2022-03-23 18:50:06
   Modificado em ...: 2022-03-23 18:56:20
   Linhas ..........: 148
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : GE_Sequencia, gep_import
   Tabelas referidas: EXT_NFS, EXT_Veic, EXT_VeicFam, EXT_VeicMarca, EXT_VeicModelo, GE_PARAMLISTA, gep_import, IV_CLIENTEPROPR
   Outras refs .....: f_s_campo, f_s_dado
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_INT_PROPRNFS]
AS
	DECLARE
		@idVeic			integer,
		@identificador  varchar(50),
		@SeqPessoa  	integer,
		@idNFS			integer,
		@Familia    	varchar(50),
		@Tipo			varchar(10),
		@Modelo     	varchar(50),
		@Ano			integer,
		@Marca			varchar(50),
		@SeqPropriedade integer,
		@SeqPropPessoa  integer,
		@campos varchar(1000),
		@dados varchar(1000),
		@conteudo varchar(250),
		@Ativo          char(1)
	Declare curNFS
	CURSOR
	FOR
		SELECT XA.IDVEIC,
			   XA.IDENTIFICADOR,
			   XA.SEQPESSOA,
			   XA.FAMILIA,
			   XA.TIPO,
			   XA.MODELO,
			   XA.ANO,
			   XA.MARCA,
			   XA.SEQPROPRIEDADE,
			   XA.IDNFS
		FROM (
		SELECT     DISTINCT 
				   EVC.IdVeic,
				   EVC.Chassi AS IDENTIFICADOR,
				   NFS.SeqPessoa,
				   NFS.IDNFS,
				   EVF.Descricao AS FAMILIA,       
				   PL.LISTASTR AS TIPO,
				   EVM.MODELO AS MODELO,
				   EVC.AnoModelo AS ANO,
				   EVMAR.MARCA AS MARCA,
				   EVF.SeqPropriedade,
				   ROW_NUMBER() over( partition by EVC.Chassi order by NFS.IDNFS desc ) as RankNFS
			FROM EXT_NFS NFS
			JOIN EXT_Veic EVC ON EVC.IdVeic = NFS.IdVeic
			JOIN EXT_VeicMarca EVMAR ON EVMAR.IdVeicMarca = EVC.IdVeicMarca
			JOIN EXT_VeicModelo EVM ON EVM.IdVeicModelo = EVC.IdVeicModelo
			JOIN EXT_VeicFam EVF ON EVF.IdVeicFamilia = EVM.IdVeicFamilia
			LEFT JOIN GE_PARAMLISTA PL ON PL.SeqParamLista = EVF.TPVeicSeqPar
									  AND PL.PARAMETRO = 'ERPVEIC_TIPOVEIC' 
									  And PL.NROEMPRESA = 0                      
			WHERE NOT EXISTS ( SELECT 1 
								FROM IV_CLIENTEPROPR CPRP
								WHERE CPRP.SeqPessoa = NFS.SeqPessoa
								AND CPRP.IDENTIFICADOR = EVC.CHASSI )
			  AND ISNULL(EVC.Chassi, '@')<>'@'
			  AND ISNULL(EVF.SEQPROPRIEDADE, 0) <> 0
		) XA
		WHERE XA.RankNFS=1
begin

	OPEN curNFS
		FETCH NEXT FROM curNFS INTO @idVeic,	
									@identificador,
							        @SeqPessoa,  	
                                    @Familia,    	
                                    @Tipo,			
                                    @Modelo,     	
                                    @Ano,			
                                    @Marca,			
                                    @SeqPropriedade,
                                    @idNFS
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
	    set @Ativo = 'S'

		set @campos = 'SEQPESSOA '
		set @dados = LTrim (cast( @SeqPessoa as varchar(20) )) 
		set @conteudo = ''

		set @campos = @campos + '| SEQPROPRIEDADE '
		set @dados = @dados + '| ' + LTrim (cast( @SeqPropriedade as varchar(20) )) 

		--select @SeqPropPessoa = sequencia + 1
		--from ge_sequencia
		--where NomeTabela = 'IV_CLIENTEPROPR'

		--update GE_Sequencia
		--set Sequencia = @SeqPropPessoa
		--where NomeTabela = 'IV_CLIENTEPROPR'

		--set @campos = @campos + '| SEQPROPPESSOA '
		--set @dados = @dados + '| ' + LTrim (cast( @SeqPropPessoa as varchar(20) )) 

		set @conteudo = dbo.f_s_dado(@Ativo)             
		set @dados  = @dados + RTrim(@conteudo)
		set @campos = @campos + dbo.f_s_campo(@conteudo, 'ATIVO')

		set @conteudo = dbo.f_s_dado(@identificador)             
		set @dados  = @dados + RTrim(@conteudo)
		set @campos = @campos + dbo.f_s_campo(@conteudo, 'IDENTIFICADOR')
		
		set @conteudo = dbo.f_s_dado(@marca)             
		set @dados  = @dados + RTrim(@conteudo)
		set @campos = @campos + dbo.f_s_campo(@conteudo, 'REFERENCIA')
		
		set @conteudo = dbo.f_s_dado(@modelo)             
		set @dados  = @dados + RTrim(@conteudo)
		set @campos = @campos + dbo.f_s_campo(@conteudo, 'CAMPO2')
		
		set @conteudo = dbo.f_s_dado('NF: ' + cast(@idNFS as varchar) )                      
		set @dados  = @dados + RTrim(@conteudo)
		set @campos = @campos + dbo.f_s_campo(@conteudo, 'NOTAS')

		if @SeqPropriedade in ( 16, 9402 )
		begin
			set @conteudo = dbo.f_s_dado(@familia)             
			set @dados  = @dados + RTrim(@conteudo)
			set @campos = @campos + dbo.f_s_campo(@conteudo, 'CAMPO1')			
		end
		
		if @SeqPropriedade in ( 16, 9402, 9403, 9404, 9405, 9410, 9411 )
		begin
			set @conteudo = dbo.f_s_dado(@ano)             
			set @dados  = @dados + RTrim(@conteudo)
			set @campos = @campos + dbo.f_s_campo(@conteudo, 'CAMPO6')			
		end
		
		insert into gep_import (status, processo, origem, acao, tabela, separador, coluna, dado, colunaidentific, dadoidentificador, dtageracao)
		values (NULL, 'VORTICOCRM', 'Protheus', 'I', 'GE_PESSOAPROPR', '|', @campos, @dados,
		'SEQPESSOA, IDENTIFICADOR', cast( @SeqPessoa as varchar(20) ) + ', ' + @identificador,
		getdate())
		FETCH NEXT FROM curNFS INTO @idVeic,	
									@identificador,
							        @SeqPessoa,  	
                                    @Familia,    	
                                    @Tipo,			
                                    @Modelo,     	
                                    @Ano,			
                                    @Marca,			
                                    @SeqPropriedade,
                                    @idNFS
	END
	CLOSE curNFS
	DEALLOCATE curNFS
end
