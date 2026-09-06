/* ==============================================================
   Objeto ..........: dbo.f_IV_UltimoResHistProcesso
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_HISTORICO, IV_RESULTADO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE  FUNCTION  dbo.f_IV_UltimoResHistProcesso  	( 	 @pnProcesso decimal(15,0),  	 @pnValido Int 	)   RETURNS varchar(1025) AS   BEGIN     DECLARE @vsUltHistorico varchar(1025)    If @pnValido > 0   	begin          Set  @vsUltHistorico = (Select RES.DESCREDUZIDA ++ ' --> ' ++ HIS.DETALHE            FROM IV_HISTORICO HIS, IV_RESULTADO RES                WHERE HIS.RESULTADO = RES.RESULTADO                  AND HIS.SEQHISTORICO IN (SELECT MAX(H.SEQHISTORICO)                                           FROM IV_HISTORICO H, IV_RESULTADO R                                           WHERE H.PROCESSO = @pnProcesso                                             AND R.RESULTADO = H.RESULTADO                                             AND R.CTRLPRODUTIVO = 1 )) 	end      Else 	begin           Set  @vsUltHistorico = ( select RES.DESCREDUZIDA ++ ' --> ' ++ HIS.DETALHE            FROM IV_HISTORICO HIS, IV_RESULTADO RES                WHERE HIS.RESULTADO = RES.RESULTADO                  AND HIS.SEQHISTORICO IN (SELECT MAX(H.SEQHISTORICO)                                           FROM IV_HISTORICO H, IV_RESULTADO R                                           WHERE H.PROCESSO = @pnProcesso                                             AND R.RESULTADO = H.RESULTADO))  	end    return(@vsUltHistorico) END   