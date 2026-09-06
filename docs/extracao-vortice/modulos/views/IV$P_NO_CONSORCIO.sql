/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_CONSORCIO
   Tipo ............: VIEW
   Criado em .......: 2025-11-28 15:17:27
   Modificado em ...: 2025-11-28 15:17:27
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_CONSORCIO ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO2 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9514 