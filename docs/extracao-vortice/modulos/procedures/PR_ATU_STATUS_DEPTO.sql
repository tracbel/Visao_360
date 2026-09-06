/* ==============================================================
   Objeto ..........: dbo.PR_ATU_STATUS_DEPTO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2017-01-03 09:25:24
   Modificado em ...: 2023-01-24 09:18:50
   Linhas ..........: 252
   Escreve em tabela: SIM (INSERT, UPDATE, DELETE)
   Alvos de escrita : IV_STATUS_DEPTO, gep_import
   Tabelas referidas: GE_PESSOA, IV_CLASSERES, iv_clientepropr, IV_GLOBALPAR, IV_HISTORICO, IV_RESCLASSE, IV_STATUS_DEPTO
   Outras refs .....: f_s_dado
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */




CREATE procedure [dbo].[PR_ATU_STATUS_DEPTO] 
AS
	DECLARE 
	@vnSeqPessoa	 numeric,
	@vsDepto		 varchar(20),
	@vsStatus		 varchar(10),
	@vdUltCompra	 datetime,
	@vdUltCompraNw   datetime,

	@vnMesEquip numeric,
	@vnMesMaq numeric,
	@vnMesPec   numeric,
	@vnMesServ  numeric,
	@vsStatusNew varchar(10),
	@vbGera		numeric,
	@vnQtde		integer,

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

	Declare C01
	
	CURSOR
	FOR

SELECT PES.SEQPESSOA, 
			 'Serviços' AS DEPTO, 
			 DEP.STATUS, 
			 MAX(DEP.ULTCOMPRA) AS ULTCOMPRA, 
			 MAX(HIST.DTAREALIZACAO) AS ULTCOMPRANEW
		FROM GE_PESSOA PES
			LEFT JOIN IV_STATUS_DEPTO DEP ON DEP.SEQPESSOA = PES.SEQPESSOA AND DEP.DEPTO = 'Serviços'
			LEFT JOIN IV_HISTORICO HIST
			LEFT JOIN IV_RESCLASSE RCL ON RCL.RESULTADO = HIST.RESULTADO
			LEFT JOIN IV_CLASSERES CRS ON CRS.SeqClasseRes = RCL.SeqClasseRes AND CRS.Classe = 'Status Serviços'
			ON HIST.SEQPESSOA = PES.SEQPESSOA
		WHERE 1 = 1
		--AND PES.SEQPESSOA = 111
		GROUP BY PES.SEQPESSOA,  
		DEP.STATUS

UNION

	SELECT PES.SEQPESSOA, 
			 'Peças' AS DEPTO, 
			 DEP.STATUS, 
			 MAX(DEP.ULTCOMPRA) AS ULTCOMPRA, 
			 MAX(HIST.DTAREALIZACAO) AS ULTCOMPRANEW
		FROM GE_PESSOA PES
			LEFT JOIN IV_STATUS_DEPTO DEP ON DEP.SEQPESSOA = PES.SEQPESSOA AND DEP.DEPTO = 'Peças'
			LEFT JOIN IV_HISTORICO HIST
			LEFT JOIN IV_RESCLASSE RCL ON RCL.RESULTADO = HIST.RESULTADO
			LEFT JOIN IV_CLASSERES CRS ON CRS.SeqClasseRes = RCL.SeqClasseRes AND CRS.Classe = 'Status Peças'
			ON HIST.SEQPESSOA = PES.SEQPESSOA
		WHERE 1 = 1
		--AND PES.SEQPESSOA = 111
		GROUP BY PES.SEQPESSOA,  
		DEP.STATUS

UNION

	SELECT PES.SEQPESSOA, 
			 'Máquinas' AS DEPTO, 
			 DEP.STATUS, 
			 MAX(DEP.ULTCOMPRA) AS ULTCOMPRA, 
			 MAX(HIST.DTAREALIZACAO) AS ULTCOMPRANEW
		FROM GE_PESSOA PES
			LEFT JOIN IV_STATUS_DEPTO DEP ON DEP.SEQPESSOA = PES.SEQPESSOA AND DEP.DEPTO = 'Máquinas'
			LEFT JOIN IV_HISTORICO HIST
			LEFT JOIN IV_RESCLASSE RCL ON RCL.RESULTADO = HIST.RESULTADO
			LEFT JOIN IV_CLASSERES CRS ON CRS.SeqClasseRes = RCL.SeqClasseRes AND CRS.Classe = 'Status Máquinas'
			ON HIST.SEQPESSOA = PES.SEQPESSOA
						
		WHERE 1 = 1
		--AND PES.SEQPESSOA = 111
		GROUP BY PES.SEQPESSOA,  
		DEP.STATUS

UNION

	SELECT PES.SEQPESSOA, 
			 'Equipamentos' AS DEPTO, 
			 DEP.STATUS, 
			 MAX(DEP.ULTCOMPRA) AS ULTCOMPRA, 
			 MAX(HIST.DTAREALIZACAO) AS ULTCOMPRANEW
		FROM GE_PESSOA PES
			LEFT JOIN IV_STATUS_DEPTO DEP ON DEP.SEQPESSOA = PES.SEQPESSOA AND DEP.DEPTO = 'Equipamentos'
			LEFT JOIN IV_HISTORICO HIST
			LEFT JOIN IV_RESCLASSE RCL ON RCL.RESULTADO = HIST.RESULTADO
			LEFT JOIN IV_CLASSERES CRS ON CRS.SeqClasseRes = RCL.SeqClasseRes AND CRS.Classe = 'Status Equipamentos'
			ON HIST.SEQPESSOA = PES.SEQPESSOA
						
		WHERE 1 = 1
		--AND PES.SEQPESSOA = 111
		GROUP BY PES.SEQPESSOA,  
		DEP.STATUS

UNION 

	SELECT DISTINCT PES.SEQPESSOA, 
			 'Deletar' AS DEPTO,
			 '' AS STATUS, 
			 GETDATE() AS ULTCOMPRA, 
			 GETDATE() AS ULTCOMPRANEW
		FROM IV_STATUS_DEPTO PES
		WHERE NOT EXISTS ( SELECT 1 FROM GE_PESSOA WHERE SEQPESSOA = PES.SEQPESSOA )
		ORDER BY 1, 5

Begin
	OPEN C01
		FETCH NEXT FROM C01 INTO 	
		@vnSeqPessoa,
		@vsDepto,
		@vsStatus,
		@vdUltCompra,
		@vdUltCompraNw

		Select @vnMesMaq = Floor(Numero1) From IV_GLOBALPAR Where SeqGlbPar = 6
		Select @vnMesPec   = Floor(Numero2) From IV_GLOBALPAR Where SeqGlbPar = 6
		Select @vnMesServ  = Floor(Numero3) From IV_GLOBALPAR Where SeqGlbPar = 6
		Select @vnMesEquip = Floor(Numero4) From IV_GLOBALPAR Where SeqGlbPar = 6

	WHILE (@@FETCH_STATUS = 0)
	BEGIN

	    Set @vbGera = Null
		Set @vsStatusNew = Null
		Set @vnQtde = Null

	   
		If @vsDepto = 'Peças' And DateDiff(Month,@vdUltCompraNw,getdate()) <= @vnMesPec
		   Set @vsStatusNew = 'Ativo'
	    Else if @vsDepto = 'Serviços' And DateDiff(Month,@vdUltCompraNw,getdate()) <= @vnMesServ
		   Set @vsStatusNew = 'Ativo'
	    Else if @vsDepto = 'Equipamentos' And DateDiff(Month,@vdUltCompraNw,getdate()) <= @vnMesEquip
		   Set @vsStatusNew = 'Ativo'
	    Else if @vsDepto = 'Máquinas' And DateDiff(Month,@vdUltCompraNw,getdate()) <= @vnMesMaq
			 Begin
				Select @vnQtde = count(1) 
					FROM iv_clientepropr 
				 WHERE seqpessoa = @vnSeqPessoa 
				 And referencia = 'John Deere'
				 And Ativo = 'S'			
				If @vnQtde > 0				
				  Set @vsStatusNew = 'Ativo'
				Else 
				  Set @vsStatusNew = 'Prospect'
		     End 
		Else 
		   Set @vsStatusNew = 'Prospect'

		If IsNull(@vsStatus, '@')<>'@'
		  Begin
		       If @vdUltCompra < @vdUltCompraNw  Or @vsStatus <> @vsStatusNew
			     Begin
		            UPDATE IV_STATUS_DEPTO SET 
			   	         ULTCOMPRA = @vdUltCompraNw
			   		   , STATUS = @vsStatusNew 
			   		   , ULTINTEGRACAO = getdate()
			   	    WHERE SEQPESSOA = @vnSeqPessoa AND DEPTO = @vsDepto
			     End
		  End

		Else
		  Begin
		   INSERT INTO IV_STATUS_DEPTO (SEQPESSOA, DEPTO, STATUS, ULTCOMPRA, ULTINTEGRACAO) 
		      VALUES ( @vnSeqPessoa, @vsDepto, @vsStatusNew, @vdUltCompraNw, getdate() )
		  End


	    If @vsStatus = 'Ativo' And @vsStatusNew = 'Prospect'
		  Begin
		   Set @vbGera = 1
		  End

		if @vsDepto = 'Deletar'
		  Begin
		    Delete from IV_STATUS_DEPTO Where SEQPESSOA = @vnSeqPessoa
		  End

		If @vbGera = 1
		  Begin
			set @campos = 'NROEMPRESA|SEQPESSOA|CODUSUARIO|ORIGEM|EVENTO|RESULTADOCMPL|DTAREALIZACAO|DETALHE'
			set @processo 	= 'VORTICOCRM'
			set @origem = 'Import'
			set @acao	= 'I'
			set @tabela	= 'IV_HISTORICO'
			set @separador	= '|'
			set @colunaidentific 	= 'SEQPESSOA'
			set @dadoidentificador	= cast ( @vnSeqPessoa as varchar(19) )
			set @dtageracao	= getdate()

			set @dados = ''
	
			set @dados  	= @dados + cast ( '1|' as varchar(19) )

			set @conteudo	= cast ( @vnSeqPessoa as varchar(19) )
			set @dados  	= @dados + RTrim(@conteudo)

			set @conteudo	= dbo.f_s_dado( 'VTCCONS' )
			set @dados  	= @dados + RTrim(@conteudo)

			set @conteudo	= dbo.f_s_dado( 'IMPORT' )
			set @dados  	= @dados + RTrim(@conteudo)

			set @conteudo	= dbo.f_s_dado( 'MUDOUSTATUS' )
			set @dados  	= @dados + RTrim(@conteudo)		

			set @conteudo	= dbo.f_s_dado( @vsDepto )
			set @dados  	= @dados + RTrim(@conteudo)

			set @conteudo	= dbo.f_s_dado( cast (convert(varchar(8), getdate(), 112) as varchar(10) ) + ' 00:00:00' )
			set @dados  	= @dados + RTrim(@conteudo)

			set @conteudo	= dbo.f_s_dado( 'Status do cliente no departamento ' + @vsDepto + ' mudou de Ativo para Prospect' )
			set @dados  	= @dados + RTrim(@conteudo)		

--			Insert Into gep_import (processo, origem, acao, tabela, separador, coluna, dado, colunaidentific, dadoidentificador, dtageracao) 
	--			Values (@processo, @origem, @acao, @tabela, @separador, @campos, @dados, @colunaidentific, @dadoidentificador, @dtageracao )
	      End

		FETCH NEXT FROM C01 INTO
		@vnSeqPessoa,
		@vsDepto,
		@vsStatus,
		@vdUltCompra,
		@vdUltCompraNw
						
	END

	CLOSE C01
	DEALLOCATE C01

End





