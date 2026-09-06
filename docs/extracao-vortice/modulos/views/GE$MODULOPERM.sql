/* ==============================================================
   Objeto ..........: dbo.GE$MODULOPERM
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_MEMBRO, GE_MODULOPERM, GE_USUARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW GE$MODULOPERM (  SISTEMA, MODULO, NROEMPRESA, SEQUSUARIO)   AS   SELECT DISTINCT MODP.SISTEMA, MODP.MODULO, MODP.NROEMPRESA,    CASE  	WHEN USR.TIPOUSUARIO = 'U' THEN MODP.SEQUSUARIO   ELSE     MEM.USUARIO   END AS SEQUSUARIO             FROM GE_MODULOPERM MODP   JOIN GE_USUARIO USR ON USR.SEQUSUARIO = MODP.SEQUSUARIO   LEFT JOIN GE_MEMBRO MEM ON MEM.GRUPO = MODP.SEQUSUARIO WHERE MODP.PERMISSAO = 'S'  