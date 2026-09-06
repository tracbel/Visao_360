/* ==============================================================
   Objeto ..........: dbo.GE$PESSOAFONE_LGPD
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOAFONE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

      CREATE VIEW GE$PESSOAFONE_LGPD AS SELECT PESF.SEQPESFONE,        PESF.TIPOFONESEQPAR,        PESF.SEQPESSOA,        PESF.DDD,        CASE PES.FISICAJURIDICA WHEN 'F' THEN (  PESF.NUMERO % 10000 ) ELSE PESF.NUMERO END as NUMERO,         PESF.COMPLEMENTO,        PESF.OBS,        PESF.INDFONEPREF,        PESF.NROFONEPESSOA,        PESF.DTAULTSUCESSO,        PESF.DTAULTINSUCESSO,        PESF.INDEMUSO,        PESF.MOTIVOEMUSO,        PESF.DTAALTERACAO,        PESF.USUALTERACAO,        PESF.INDUSOMKT,        PESF.DTAINCLUSAO,        PESF.USUINCLUSAO,        PESF.INDWHATSAPP        ,PESF.NUMERO AS z_NUMERO   FROM GE_PESSOAFONE PESF JOIN GE_PESSOA PES ON PES.SEQPESSOA = PESF.SEQPESSOA   