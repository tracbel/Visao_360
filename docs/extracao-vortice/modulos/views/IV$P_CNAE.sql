/* ==============================================================
   Objeto ..........: dbo.IV$P_CNAE
   Tipo ............: VIEW
   Criado em .......: 2024-09-12 09:50:20
   Modificado em ...: 2024-09-12 09:50:20
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_CNAE ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  )   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR   FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9413 