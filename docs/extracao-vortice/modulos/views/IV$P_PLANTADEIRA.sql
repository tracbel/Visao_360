/* ==============================================================
   Objeto ..........: dbo.IV$P_PLANTADEIRA
   Tipo ............: VIEW
   Criado em .......: 2026-01-13 08:07:44
   Modificado em ...: 2026-01-13 08:07:44
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_PLANTADEIRA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  MODELO,  ANO,  QUANTIDADE,  N__DE_LINHAS)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO6,  CAMPO7,  NUMERO4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9405 