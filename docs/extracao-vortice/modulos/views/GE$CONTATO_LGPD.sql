/* ==============================================================
   Objeto ..........: dbo.GE$CONTATO_LGPD
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_CONTATO
   Outras refs .....: fva_DateTimeFromParts, fva_StrMaskLGPD
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW GE$CONTATO_LGPD AS  SELECT SEQPESSOA,        SEQCONTATO,        EMUSO,        TIPOCONTATO,        AREAATUACAO,        (RG % 10000) as RG,        (CPF % 1000) as CPF,        DIGCPF,        dbo.fva_StrMaskLGPD (SAUDACAO, 4,3 ) AS SAUDACAO,        dbo.fva_StrMaskLGPD (CONTATO, 5,6 ) AS CONTATO,        FONEDDD1,        (FONENRO1 % 10000) as FONENRO1,        FONECMPL1,        FONEDDD2,        (FONENRO2 % 10000)  as FONENRO2,        FONECMPL2,        FAXDDD,        (FAXNRO % 10000)  as FAXNRO,        SEXO,        ESTADOCIVIL,        dbo.fva_DateTimeFromParts ( 1804, MONTH(DTANASCIMENTO), DAY(DTANASCIMENTO), 0, 0, 0, 0 )  AS DTANASCIMENTO,        NIVELDECISAO,        POSICIONAMENTO,        OBSPESSOAL,        ATRIBUTO1,        ATRIBUTO2,        ATRIBUTO3,        ATRIBDTA,        ATRIBNUM,        dbo.fva_StrMaskLGPD (EMAIL, 4,5 )  AS EMAIL,        dbo.fva_StrMaskLGPD (SKYPE, 3,3 )   AS SKYPE,         LINKWEB,        ULTORIGEM,        USUALTERACAO,        DTAALTERACAO,        OBSERVACAO,        INDWHATSAPPF1,        INDWHATSAPPF2,        INDWHATSAPPFX        , RG as z_RG        , CPF as z_CPF        , SAUDACAO as z_SAUDACAO        , CONTATO as z_CONTATO        , FONENRO1 as z_FONENRO1        , FONENRO2 as z_FONENRO2        , FAXNRO as z_FAXNRO        , DTANASCIMENTO as z_DTANASCIMENTO        , EMAIL AS z_EMAIL        , SKYPE AS z_SKYPE   FROM GE_CONTATO  