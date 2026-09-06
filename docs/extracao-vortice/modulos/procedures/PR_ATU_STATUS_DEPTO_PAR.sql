/* ==============================================================
   Objeto ..........: dbo.PR_ATU_STATUS_DEPTO_PAR
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-02-08 10:02:55
   Modificado em ...: 2023-02-08 10:02:55
   Linhas ..........: 205
   Escreve em tabela: SIM (INSERT, UPDATE, DELETE)
   Alvos de escrita : IV_STATUS_DEPTO, gep_import
   Tabelas referidas: EXT_NFS, EXT_Veic, EXT_VeicFam, EXT_VeicModelo, GE_PESSOA, iv_clientepropr, IV_GLOBALPAR, IV_STATUS_DEPTO, IVS_DEPTO
   Outras refs .....: f_s_dado
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_ATU_STATUS_DEPTO_PAR] 
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

		SELECT NFS.SeqPessoa,
			   DEP.STATUS,
			   CASE WHEN DDP.Depto = 'MAQ-SERV'  THEN 'Serviços'
					WHEN DDP.Depto = 'MAQ-PEÇAS' THEN 'Peças'
					WHEN DDP.Depto = 'MAQ-NOVOS' THEN 'Máquinas' END AS DEPTO,
			   MAX(DEP.ULTCOMPRA) AS ULTCOMPRA,
			   MAX(NFS.DtaEmissaoNF) AS ULTCOMPRANEW
		FROM EXT_NFS NFS
		JOIN IVS_DEPTO DDP ON DDP.SeqDepto = NFS.SeqDepto
		LEFT JOIN EXT_Veic VVV ON VVV.IdVeic = NFS.IdVeic
		LEFT JOIN EXT_VeicModelo EMOD ON EMOD.IdVeicModelo = VVV.IdVeicModelo
		LEFT JOIN EXT_VeicFam EFAM ON EFAM.IdVeicFamilia = EMOD.IdVeicFamilia
		LEFT JOIN IV_GLOBALPAR PPP ON PPP.Campo1 = LEFT ( EFAM.DESCRICAO, 20 )
		LEFT JOIN IV_STATUS_DEPTO DEP ON DEP.SEQPESSOA = NFS.SEQPESSOA AND 
					DEP.DEPTO = CASE WHEN DDP.Depto = 'MAQ-SERV'  THEN 'Serviços'
									 WHEN DDP.Depto = 'MAQ-PEÇAS' THEN 'Peças'
									 WHEN DDP.Depto = 'MAQ-NOVOS' THEN 'Máquinas' END
		WHERE NFS.IDVEIC IS NOT NULL
		GROUP BY NFS.SeqPessoa, 
				 DEP.STATUS,
				 CASE WHEN DDP.Depto = 'MAQ-SERV'  THEN 'Serviços'
					WHEN DDP.Depto = 'MAQ-PEÇAS' THEN 'Peças'
					WHEN DDP.Depto = 'MAQ-NOVOS' THEN 'Máquinas' END

	UNION 

	SELECT DISTINCT PES.SEQPESSOA, 
			 '' AS STATUS, 
			 'Deletar' AS DEPTO,
			 GETDATE() AS ULTCOMPRA, 
			 GETDATE() AS ULTCOMPRANEW
		FROM IV_STATUS_DEPTO PES
		WHERE NOT EXISTS ( SELECT 1 FROM GE_PESSOA WHERE SEQPESSOA = PES.SEQPESSOA )
		ORDER BY 1, 5

Begin
	OPEN C01
		FETCH NEXT FROM C01 INTO 	
		@vnSeqPessoa,
		@vsStatus,
		@vsDepto,
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

--IF @vnSeqPessoa=921
--BEGIN
--	PRINT @vsStatus;
--	PRINT @vsStatusNew;
--	PRINT @vdUltCompra;
--	PRINT @vdUltCompraNw;
--	print @vsDepto
--END

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
		@vsStatus,
		@vsDepto,
		@vdUltCompra,
		@vdUltCompraNw
						
	END

	CLOSE C01
	DEALLOCATE C01

End
