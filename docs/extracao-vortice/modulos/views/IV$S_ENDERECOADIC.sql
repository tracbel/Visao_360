/* ==============================================================
   Objeto ..........: dbo.IV$S_ENDERECOADIC
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOAEND
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$S_ENDERECOADIC (SEQPESSOA,                 TIPOENDERECO,      SEQCIDADE,           CIDADE,              UF,      SEQBAIRRO,           BAIRRO,              TIPOLOGRADOURO,      LOGRADOURO,          NROLOGRADOURO,       CMPLTOLOGRADOURO,      CXPOSTAL,      CEP) AS Select        SEQPESSOA,             CASE TIPOENDERECO        WHEN 'E' THEN 'ENTREGA'       WHEN 'C' THEN 'COMERCIAL'       WHEN 'R' THEN 'CORRESPONDÊNCIA'         ELSE 'OUTRO'      END AS TIPOENDERECO,      SEQCIDADE,           CIDADE,              UF,      SEQBAIRRO,           BAIRRO,              TIPOLOGRADOURO,      LOGRADOURO,          NROLOGRADOURO,       CMPLTOLOGRADOURO,      CXPOSTAL,    CEP From   GE_PESSOAEND    