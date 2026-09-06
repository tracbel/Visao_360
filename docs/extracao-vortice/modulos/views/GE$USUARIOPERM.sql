/* ==============================================================
   Objeto ..........: dbo.GE$USUARIOPERM
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_MEMBRO, GE_USUARIO, GE_USUARIOPERM
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW GE$USUARIOPERM (  CODAPLICACAO, CHAVEAPLICACAO, NROEMPRESA, PERMISSAO,          SEQUSUARIO)   AS   SELECT USRP.CODAPLICACAO, USRP.CHAVEAPLICACAO, USRP.NROEMPRESA, USRP.PERMISSAO,   CASE WHEN USR.TIPOUSUARIO = 'U' THEN USRP.SEQUSUARIO             ELSE     MEM.USUARIO   END AS SEQUSUARIO FROM GE_USUARIOPERM USRP LEFT JOIN GE_USUARIO USR ON USR.SEQUSUARIO = USRP.SEQUSUARIO LEFT JOIN GE_MEMBRO MEM ON MEM.GRUPO = USRP.SEQUSUARIO   