/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_FENO_E_FORRAGEM
   Tipo ............: VIEW
   Criado em .......: 2025-11-28 15:17:56
   Modificado em ...: 2025-11-28 15:17:56
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_FENO_E_FORRAGEM ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  ANO,  QUANTIDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO6,  NUMERO1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9508 