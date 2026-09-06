/* ==============================================================
   Objeto ..........: dbo.IV$P_OPERATIONS_CENTER
   Tipo ............: VIEW
   Criado em .......: 2026-06-09 09:33:34
   Modificado em ...: 2026-06-09 09:33:34
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_OPERATIONS_CENTER ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  NOME)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  LITERAL1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 17 