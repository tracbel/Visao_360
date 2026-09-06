/* ==============================================================
   Objeto ..........: dbo.BI_USUARIO
   Tipo ............: VIEW
   Criado em .......: 2015-08-03 12:29:15
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_USUARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW BI_USUARIO AS  SELECT USR.SEQUSUARIO    ,USR.CODUSUARIO AS USR_CODIGO    ,USR.NOMEREDUZIDO AS USR_NOME  FROM GE_USUARIO USR  WHERE USR.TIPOUSUARIO = 'U'