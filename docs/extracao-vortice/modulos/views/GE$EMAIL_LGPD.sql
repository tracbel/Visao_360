/* ==============================================================
   Objeto ..........: dbo.GE$EMAIL_LGPD
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_EMAIL, GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

    CREATE VIEW GE$EMAIL_LGPD  AS  SELECT EML.SEQEMAIL,        CASE FISICAJURIDICA WHEN 'F' THEN ( SUBSTRING(EML.EMAIL , 0, 6) + REPLICATE ('*', LEN(EML.EMAIL) - 6 ) + ( CASE WHEN LEN(EML.EMAIL) > 12 THEN SUBSTRING(EML.EMAIL, LEN(EML.EMAIL)-4 , 5) ELSE '' END ) ) ELSE EML.EMAIL END AS EMAIL,        EML.SEQPESSOA,        EML.SEQUSUARIO,        EML.INDPREFERENCIAL,        EML.INDUSOMKT,        EML.INDUSOPROFISSIONAL,        EML.INDUSOPESSOAL,        EML.INDUSOFISCAL,        EML.INDEMUSO,        EML.SENHA,        EML.DTAALTERACAO,        EML.USUALTEROU,        EML.OBS,        EML.MOTIVO,        EML.DTAEMAILATIVO        , EML.EMAIL as z_EMAIL FROM GE_EMAIL EML  JOIN GE_PESSOA PES ON PES.SEQPESSOA = EML.SEQPESSOA  