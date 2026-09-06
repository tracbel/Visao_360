/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_IMPLEMENTO
   Tipo ............: VIEW
   Criado em .......: 2025-11-28 15:18:21
   Modificado em ...: 2025-11-28 15:18:21
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_IMPLEMENTO ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  ANO,  QUANTIDADE,  MODELO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO6,  NUMERO1,  LITERAL1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9506 