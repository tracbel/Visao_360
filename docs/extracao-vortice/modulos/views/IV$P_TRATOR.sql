/* ==============================================================
   Objeto ..........: dbo.IV$P_TRATOR
   Tipo ............: VIEW
   Criado em .......: 2026-01-13 08:00:44
   Modificado em ...: 2026-01-13 08:00:44
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_TRATOR ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  FAMILIA,  MODELO,  FAIXA_DE_POTENCIA,  ANO,  QUANTIDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO3,  CAMPO6,  CAMPO7 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9402 