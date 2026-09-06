/* ==============================================================
   Objeto ..........: dbo.PR_ATU_FORMPRODTOTVS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2022-08-12 09:55:47
   Modificado em ...: 2022-08-12 11:02:28
   Linhas ..........: 63
   Escreve em tabela: SIM (INSERT, DELETE)
   Alvos de escrita : IV_GLOBALPAR, iv_globalpar
   Tabelas referidas: IV_GlobalPar
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_ATU_FORMPRODTOTVS]
AS
	DECLARE
		@xMarca			varchar(100),
		@xDescricao		varchar(100),
		@Xcod_modelo    varchar(100),
		@Xmodelo        varchar(100),
		@Xmodelo2       varchar(100),
		@Xcod_grupo_modelo    varchar(100),
		@Xgrupo_modelo  varchar(100),
		@Xcod_tipo      varchar(100),
		@XTipo          varchar(100)

	Declare curVEIC
	CURSOR
	FOR
		SELECT DISTINCT
		       LEFT(MARCA COLLATE SQL_Latin1_General_CP1_CI_AS, 100) AS MARCA,
---		       COD_MODELO,
			   LEFT( MODELO COLLATE SQL_Latin1_General_CP1_CI_AS, 100 ) AS MODELO,
			   --COD_GRUPO_MODELO,
			   --GRUPO_MODELO,
			   --COD_TIPO,
			   TIPO COLLATE SQL_Latin1_General_CP1_CI_AS
			   from openquery ( totvs, 'select * from X_V_COL_MODELO_CRM' )

	Declare curMARCA
	CURSOR
	FOR
		SELECT DISTINCT
		       LEFT(COD_MARCA COLLATE SQL_Latin1_General_CP1_CI_AS, 100) AS MARCA,
			   LEFT(MARCA COLLATE SQL_Latin1_General_CP1_CI_AS, 100) AS DESCRICAO
			   from openquery ( totvs, 'select * from X_V_COL_MODELO_CRM' ) XM
	    WHERE NOT EXISTS (SELECT 1 FROM IV_GlobalPar WHERE SeqGlbPar=16 AND TRIM(LITERAL1) = TRIM(XM.MARCA COLLATE SQL_Latin1_General_CP1_CI_AS) )

begin

    DELETE FROM IV_GLOBALPAR WHERE SeqGlbPar = 16
	OPEN curMARCA
	FETCH NEXT FROM curMARCA INTO @xMarca, @xDescricao
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		insert into iv_globalpar ( seqpar, seqglbpar, nroempresa, literal1, literal2, DtaAlteracao, usualteracao )
		values ( (select max(seqpar) from iv_globalpar) + 1, 16, 0, @xMarca, @xDescricao, GETDATE(), 'Import' )
		FETCH NEXT FROM curMARCA INTO @xMarca, @xDescricao
	END
	CLOSE curMARCA
	DEALLOCATE curMARCA


    DELETE FROM IV_GLOBALPAR WHERE SeqGlbPar = 15
	OPEN curVEIC
	FETCH NEXT FROM curVEIC INTO @xDescricao, @xModelo, @xTipo
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		insert into iv_globalpar ( seqpar, seqglbpar, nroempresa, literal1, literal2, literal3, DtaAlteracao, usualteracao )
		values ( (select max(seqpar) from iv_globalpar) + 1, 15, 0, @xDescricao, @xModelo, @xTipo, GETDATE(), 'Import' )
		FETCH NEXT FROM curVEIC INTO @xDescricao, @xModelo, @xTipo
	END
	CLOSE curVEIC
	DEALLOCATE curVEIC
end
