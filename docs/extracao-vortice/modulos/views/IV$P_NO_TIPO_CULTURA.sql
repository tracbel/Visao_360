/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_TIPO_CULTURA
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 08:57:02
   Modificado em ...: 2025-04-14 08:57:02
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_TIPO_CULTURA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  PESO_CULTURA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9610 