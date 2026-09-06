/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_TRATOR
   Tipo ............: VIEW
   Criado em .......: 2025-11-28 15:21:53
   Modificado em ...: 2025-11-28 15:21:53
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_TRATOR ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  FAIXA_DE_POTENCIA,  ANO,  QUANTIDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO2,  CAMPO3,  CAMPO6,  NUMERO1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9502 