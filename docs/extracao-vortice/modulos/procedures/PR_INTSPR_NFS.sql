/* ==============================================================
   Objeto ..........: dbo.PR_INTSPR_NFS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2015-09-30 15:57:27
   Modificado em ...: 2016-08-23 13:48:13
   Linhas ..........: 194
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : ext_nfs, ext_nfsitem
   Tabelas referidas: EXT_NFS, EXT_NFSITEM
   Outras refs .....: fva_UnFormatCGCCPF
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_INTSPR_NFS]
AS
	DECLARE @nroempresa	varchar(2),
	@nronf 		integer,
	@serienf	varchar(8),
	@nrocgccpf	varchar(20),	
	@digcgccpf	varchar(2),
	@nrocgccpffmt	varchar(19),
	@departamento	varchar(30),
	@operacao	varchar(20),
	@formapagto	varchar(25),
	@vendedor	varchar(70),
	@nrovendedor	integer,
	@dtaemissaonf	integer,
	@ano		varchar(2),
	@mes		varchar(2),
	@dia		varchar(2),
	@idpessoa	integer,
	@natopecod	varchar(3),
	
	@idNFS		integer,
	@iditem		integer,
	@codproduto	varchar(40),
	@familia	varchar(30),
	@descproduto	varchar(100),
	@qtde		numeric(15, 2),
	@vlrunitario	numeric(15, 2),
	@vlrdescto	numeric(15, 2),
	@vlricm		numeric(15, 2),

	@conteudo	varchar(250),
	@qtdNfs		integer,
	@nwOrigem       varchar(20),
	@nwSegmento	varchar(30),
	@nwSituacao	varchar(20),
	@niNroEmpresa   integer,
	@nwNroEmpresa   varchar(2)
	Declare curNFS
	
	CURSOR
	FOR
	SELECT * FROM OPENQUERY(SPRESS, 
	'SELECT NFS.FILIALCOD AS NROEMPRESA,
	        NFS.CANFSANRO AS NRONF,
		NFS.SENFSACOD AS SERIENF,
		NFS.CANFSANROCGCCPF AS NROCJCCPF,
		ANEG.ARENEGDES AS DEPARTAMENTO,
		NFS.CANFSADESNATOPE AS OPERACAO,
		PAG.CONPAGDES AS FORMAPAGTO,
		FUNC.FUNCIONOM AS VENDEDOR,
		NFS.USUARICOD AS NROVENDEDOR,
		NFS.CANFSADATEMISSAO AS DTAEMISSAONF,
		SUBSTRING(NFS.CANFSADATEMISSAO FROM 1 FOR 4) AS ANO,
		SUBSTRING(NFS.CANFSADATEMISSAO FROM 5 FOR 2) AS MES,
		SUBSTRING(NFS.CANFSADATEMISSAO FROM 7 FOR 2) AS DIA,
		NFS.CLIENTNRO AS IDPESSOA,
		NFS.NATOPECOD,
		NFIT.ITNFSASEQ AS IDITEM,
		NFIT.ITNFSACOD AS CODPRODUTO,
		NFIT.ITNFSAIDTTIPO AS FAMILIA,
		NFIT.ITNFSADES AS DESCPRODUTO,
		NFIT.ITNFSAQTD AS QTDE,
		NFIT.ITNFSAVLRUNITARIO AS VLRUNITARIO,
		NFIT.ITNFSAVLRDESCONTO AS VLRDESCONTO,
		NFIT.ITNFSAVLRICMOP AS VLRICM
	FROM TNFCANFSA NFS
	INNER JOIN TGLARENEG ANEG ON ANEG.ARENEGCOD = NFS.ARENEGCOD
	INNER JOIN TGLCONPAG PAG ON PAG.CONPAGCOD = NFS.CONPAGCOD
	INNER JOIN TGLFUNCIO FUNC ON FUNC.FUNCIOCOD = NFS.USUARICOD
	INNER JOIN TNFITNFSA NFIT ON NFIT.FILIALCOD = NFS.FILIALCOD
				 AND NFIT.CANFSANRO = NFS.CANFSANRO
				 AND NFIT.SENFSACOD = NFS.SENFSACOD
	WHERE NFS.TIPOPECOD = 3
	  AND NFS.TABSITCOD = 23
	  AND SUBSTRING(NFS.CANFSADATEMISSAO FROM 1 FOR 4) >= EXTRACT( YEAR FROM CURRENT_DATE-90)
	  AND SUBSTRING(NFS.CANFSADATEMISSAO FROM 5 FOR 2) >= EXTRACT( MONTH FROM CURRENT_DATE-90)
	  AND SUBSTRING(NFS.CANFSADATEMISSAO FROM 7 FOR 2) >= EXTRACT( DAY FROM CURRENT_DATE-90)
	')
begin
	OPEN curNFS
		FETCH NEXT FROM curNFS INTO 	@nroempresa,
						@nronf,
						@serienf,
						@nrocgccpffmt,
						@departamento,
						@operacao,
						@formapagto,
						@vendedor,
						@nrovendedor,
						@dtaemissaonf,
						@ano,
						@mes,
						@dia,
						@idpessoa,
						@natopecod,
						@iditem,
						@codproduto,
						@familia,
						@descproduto,
						@qtde,
						@vlrunitario,
						@vlrdescto,
						@vlricm

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
	
		if charindex(RTRIM(@natopecod) + '#', '03#05#06#07#BF#VB#VN#VU#VE#VM#VG#VP#')>0
		BEGIN
			set @nwOrigem = 'COLORADO'
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
			SELECT @qtdNfs = COUNT(*) 
			FROM EXT_NFS
			WHERE NROEMPRESA = @nwNroEmpresa
			  AND NRONF = @nronf
			  AND SERIENF = @serienf
			
			if @qtdNfs=0
			begin
				set @conteudo = rTrim( dbo.fva_UnFormatCGCCPF( @nrocgccpffmt ) )
				set @digcgccpf = right(@conteudo, 2)
				set @conteudo = @conteudo + '#'
				set @nrocgccpf = replace(@conteudo, @digcgccpf + '#', '')

				set @conteudo = cast (@dtaemissaonf as varchar)
				set @conteudo = substring(@conteudo, 5,2) + '/' + right(@conteudo, 2) + '/' + left(@conteudo, 4)
			
				insert into ext_nfs (IDPESSOA, ORIGEM, PESSOALINK, PESSOALINKNRO, NROEMPRESA, NRONF, SERIENF, NROCGCCPF, DIGCGCCPF, SEGMENTO, DEPARTAMENTO, OPERACAO, FORMAPGTO, VENDEDOR, NROVENDEDOR, DTAEMISSAONF, SITUACAO, DTAIMPORT)
				VALUES (@idpessoa, @natopecod, @nwOrigem, @idpessoa, cast (@nwNroEmpresa as integer), @nronf, @serienf, 
					cast (@nrocgccpf as numeric), cast(@digcgccpf as integer), @nwSegmento, @departamento, @operacao, @formapagto, @vendedor, @nrovendedor, 
					cast (@conteudo as datetime), @nwSituacao, GETDATE())
			end
			SELECT @idNfs = idnfs
			FROM EXT_NFS
			WHERE NROEMPRESA = @nwNroEmpresa
			  AND NRONF = @nronf
			  AND SERIENF = @serienf

			SELECT @qtdNfs = COUNT(*) 
			FROM EXT_NFSITEM
			WHERE IDNFS = @idNFS
			  AND IDITEM = @idItem
			
			if @qtdNfs=0
			begin
				insert into ext_nfsitem (idnfs, IDITEM, CODPRODUTO, FAMILIA, DESCPRODUTO, QTDE,  VLRUNITRARIO, VLRDESCTO, SITUACAO, VLRICM, DTAIMPORT)
				VALUES (@idnfs, @iditem, @codproduto, @familia, @descproduto, 
					@qtde, @vlrunitario, @vlrdescto, @nwSituacao, @vlricm, GETDATE())
			end
		END
		FETCH NEXT FROM curNFS INTO 	@nroempresa,
						@nronf,
						@serienf,
						@nrocgccpffmt,
						@departamento,
						@operacao,
						@formapagto,
						@vendedor,
						@nrovendedor,
						@dtaemissaonf,
						@ano,
						@mes,
						@dia,
						@idpessoa,
						@natopecod,
						@iditem,
						@codproduto,
						@familia,
						@descproduto,
						@qtde,
						@vlrunitario,
						@vlrdescto,
						@vlricm
						
	END

	CLOSE curNFS
	DEALLOCATE curNFS

End
