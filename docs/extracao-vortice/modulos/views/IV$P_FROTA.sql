/* ==============================================================
   Objeto ..........: dbo.IV$P_FROTA
   Tipo ............: VIEW
   Criado em .......: 2025-08-12 12:56:02
   Modificado em ...: 2025-08-12 12:56:02
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_FROTA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  )   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR   FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 20 