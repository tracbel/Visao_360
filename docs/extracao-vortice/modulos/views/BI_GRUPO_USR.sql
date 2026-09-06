/* ==============================================================
   Objeto ..........: dbo.BI_GRUPO_USR
   Tipo ............: VIEW
   Criado em .......: 2015-08-03 12:32:33
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_MEMBRO, GE_USUARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW BI_GRUPO_USR AS  SELECT GRP.CODUSUARIO AS GRUPO_USUARIO,  MBR.USUARIO  FROM GE_USUARIO USR  JOIN GE_MEMBRO MBR ON MBR.USUARIO = USR.SEQUSUARIO  JOIN GE_USUARIO GRP     ON GRP.SEQUSUARIO = MBR.GRUPO     AND GRP.CODUSUARIO <> 'todos'  WHERE USR.NIVEL > 0