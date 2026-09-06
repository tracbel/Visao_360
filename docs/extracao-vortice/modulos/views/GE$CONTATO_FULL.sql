/* ==============================================================
   Objeto ..........: dbo.GE$CONTATO_FULL
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_CONTATO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW GE$CONTATO_FULL AS SELECT SEQPESSOA,        SEQCONTATO,        EMUSO,        TIPOCONTATO,        AREAATUACAO,        RG,        CPF,        DIGCPF,        SAUDACAO,        CONTATO,        FONEDDD1,        FONENRO1,        FONECMPL1,        FONEDDD2,        FONENRO2,        FONECMPL2,        FAXDDD,        FAXNRO,        SEXO,        ESTADOCIVIL,        DTANASCIMENTO,        NIVELDECISAO,        POSICIONAMENTO,        OBSPESSOAL,        ATRIBUTO1,        ATRIBUTO2,        ATRIBUTO3,        ATRIBDTA,        ATRIBNUM,        EMAIL,        SKYPE,        LINKWEB,        ULTORIGEM,        USUALTERACAO,        DTAALTERACAO,        OBSERVACAO,        INDWHATSAPPF1,        INDWHATSAPPF2,        INDWHATSAPPFX        , RG as z_RG        , CPF as z_CPF        , SAUDACAO as z_SAUDACAO        , CONTATO as z_CONTATO        , FONENRO1 as z_FONENRO1        , FONENRO2 as z_FONENRO2        , FAXNRO as z_FAXNRO        , DTANASCIMENTO as z_DTANASCIMENTO        , EMAIL AS z_EMAIL        , SKYPE AS z_SKYPE   FROM GE_CONTATO  