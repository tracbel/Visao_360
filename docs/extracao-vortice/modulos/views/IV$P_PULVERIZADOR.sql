/* ==============================================================
   Objeto ..........: dbo.IV$P_PULVERIZADOR
   Tipo ............: VIEW
   Criado em .......: 2026-01-13 08:04:28
   Modificado em ...: 2026-01-13 08:04:28
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_PULVERIZADOR ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  ANO,  QUANTIDADE,  CAP__TANQUE_SOLUCAO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO2,  CAMPO6,  CAMPO7,  NUMERO4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9410 