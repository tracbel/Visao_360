/* ==============================================================
   Objeto ..........: dbo.fiv_GetGrupoUsr
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_MEMBRO, GE_USUARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION  dbo.fiv_GetGrupoUsr  (@pnSeqUsuario  numeric(18,0)) RETURNS VARCHAR(250) AS BEGIN Declare @vsRetorno   varchar(225) Declare @vsHifen     varchar(3) Declare @vsGrupo     varchar(20) DECLARE c01 CURSOR FOR  			SELECT GRP.CODUSUARIO 			FROM GE_MEMBRO MBR  			JOIN GE_USUARIO GRP ON GRP.SEQUSUARIO = MBR.GRUPO 			WHERE MBR.USUARIO = @pnSeqUsuario 			  AND GRP.CODUSUARIO != 'todos' 			ORDER BY 1  Set @vsRetorno = '' Set @vsHifen = '' Set @vsGrupo = '' OPEN c01 		FETCH NEXT FROM c01 INTO   @vsGrupo 	WHILE @@FETCH_STATUS = 0 		BEGIN 			If   isnull(len(@vsRetorno) + len(@vsGrupo), 1) < 220  			Begin  				Set @vsRetorno = @vsRetorno + @vsHifen + @vsGrupo   			End 			Set @vsHifen = ', ' 			FETCH NEXT FROM c01 INTO     @vsGrupo  		END 	CLOSE c01 	DEALLOCATE c01   RETURN @vsRetorno END  