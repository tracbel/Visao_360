/* ==============================================================
   Objeto ..........: dbo.GE$PESSOAEND_LGPD
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOAEND
   Outras refs .....: fva_StrMaskLGPD
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

        CREATE VIEW GE$PESSOAEND_LGPD AS SELECT PEND.SEQPESSOA,        PEND.SEQPESSOAEND,        PEND.TIPOENDERECO,        PEND.SEQCIDADE,        PEND.CIDADE,        PEND.UF,        PEND.SEQBAIRRO,        PEND.BAIRRO,        CASE PES.FISICAJURIDICA WHEN 'F'  THEN dbo.fva_StrMaskLGPD (PEND.LOGRADOURO, 8,6 )  ELSE PEND.LOGRADOURO END AS LOGRADOURO,        CASE PES.FISICAJURIDICA WHEN 'F'  THEN dbo.fva_StrMaskLGPD (PEND.NROLOGRADOURO, 0,2 ) ELSE PEND.NROLOGRADOURO END AS NROLOGRADOURO,         PEND.CMPLTOLOGRADOURO,        PEND.CEP,        PEND.DTAALTERACAO,        PEND.USUALTERACAO,        PEND.TIPOLOGRADOURO,        PEND.SEQPESSOAENDCOBR,        PEND.PAIS,        PEND.CXPOSTAL,        PEND.REFENDERECO,        PEND.LATITUDE,        PEND.LONGITUDE,        PEND.DESCRICAO,        PEND.SEQREGIAO,        PEND.SEQROTA,        PEND.CHAVEADICIONAL,        PEND.INSCPRODUTOR        , PEND.LOGRADOURO as z_LOGRADOURO        , PEND.NROLOGRADOURO as z_NROLOGRADOURO FROM GE_PESSOAEND PEND JOIN GE_PESSOA PES ON PES.SEQPESSOA = PEND.SEQPESSOA  