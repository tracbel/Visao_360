/* ==============================================================
   Objeto ..........: dbo.GE$PESSOAEND_FULL
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOAEND
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

       CREATE VIEW GE$PESSOAEND_FULL AS SELECT SEQPESSOA,        SEQPESSOAEND,        TIPOENDERECO,        SEQCIDADE,        CIDADE,        UF,        SEQBAIRRO,        BAIRRO,        LOGRADOURO,        NROLOGRADOURO,        CMPLTOLOGRADOURO,        CEP,        DTAALTERACAO,        USUALTERACAO,        TIPOLOGRADOURO,        SEQPESSOAENDCOBR,        PAIS,        CXPOSTAL,        REFENDERECO,        LATITUDE,        LONGITUDE,        DESCRICAO,        SEQREGIAO,        SEQROTA,        CHAVEADICIONAL,        INSCPRODUTOR        , LOGRADOURO as z_LOGRADOURO        , NROLOGRADOURO as z_NROLOGRADOURO FROM GE_PESSOAEND   