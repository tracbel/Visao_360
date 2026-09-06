/* ==============================================================
   Objeto ..........: dbo.GE$EMAIL_FULL
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_EMAIL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

     CREATE VIEW GE$EMAIL_FULL  AS  SELECT SEQEMAIL,        EMAIL,        SEQPESSOA,        SEQUSUARIO,        INDPREFERENCIAL,        INDUSOMKT,        INDUSOPROFISSIONAL,        INDUSOPESSOAL,        INDUSOFISCAL,        INDEMUSO,        SENHA,        DTAALTERACAO,        USUALTEROU,        OBS,        MOTIVO,        DTAEMAILATIVO        , EMAIL as z_EMAIL   FROM GE_EMAIL  