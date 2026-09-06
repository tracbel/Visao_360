/* ==============================================================
   Objeto ..........: dbo.IV$P_EQUIPAMENTOS_MANITOU
   Tipo ............: VIEW
   Criado em .......: 2025-01-08 11:28:22
   Modificado em ...: 2025-01-08 11:28:22
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_EQUIPAMENTOS_MANITOU ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  FAMILIA,  MODELO,  NOVO_USADO,  ANO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO3,  CAMPO6 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 16 