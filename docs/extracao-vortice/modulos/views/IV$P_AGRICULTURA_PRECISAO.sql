/* ==============================================================
   Objeto ..........: dbo.IV$P_AGRICULTURA_PRECISAO
   Tipo ............: VIEW
   Criado em .......: 2025-12-01 10:36:54
   Modificado em ...: 2025-12-01 10:36:54
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_AGRICULTURA_PRECISAO ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  MODELO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9407 