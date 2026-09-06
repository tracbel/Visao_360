/* ==============================================================
   Objeto ..........: dbo.GE$PESSOAFONE_FULL
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOAFONE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

     CREATE VIEW GE$PESSOAFONE_FULL AS SELECT SEQPESFONE,        TIPOFONESEQPAR,        SEQPESSOA,        DDD,        NUMERO,        COMPLEMENTO,        OBS,        INDFONEPREF,        NROFONEPESSOA,        DTAULTSUCESSO,        DTAULTINSUCESSO,        INDEMUSO,        MOTIVOEMUSO,        DTAALTERACAO,        USUALTERACAO,        INDUSOMKT,        DTAINCLUSAO,        USUINCLUSAO,        INDWHATSAPP        ,NUMERO AS z_NUMERO FROM GE_PESSOAFONE  