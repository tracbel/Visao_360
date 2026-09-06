/* ==============================================================
   Objeto ..........: dbo.GE$POLSEGPERM
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_POLSEGPERM, GE_USUARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW GE$POLSEGPERM (  CODAPLICACAO, CHAVEAPLICACAO, NROEMPRESA, SEQPOLSEG,          SEQUSUARIO)   AS   SELECT PSEG.CODAPLICACAO, PSEG.CHAVEAPLICACAO, PSEG.NROEMPRESA, PSEG.SEQPOLSEG, USR.SEQUSUARIO    FROM GE_POLSEGPERM PSEG    JOIN GE_USUARIO USR ON USR.SEQPOLSEG = PSEG.SEQPOLSEG  UNION SELECT PSEG.CODAPLICACAO, PSEG.CHAVEAPLICACAO, PSEG.NROEMPRESA, PSEG.SEQPOLSEG, USR.SEQUSUARIO    FROM GE_POLSEGPERM PSEG    JOIN GE_USUARIO USR ON (USR.SEQPOLSEG IS NULL OR USR.SEQPOLSEG = 0) AND USR.TIPOUSUARIO = 'U' WHERE PSEG.SEQPOLSEG = 0  