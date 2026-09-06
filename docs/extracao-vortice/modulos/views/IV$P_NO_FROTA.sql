/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_FROTA
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 09:01:13
   Modificado em ...: 2025-04-14 09:01:13
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_FROTA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  ANO,  POTENCIA,  QUANTIDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  LITERAL1,  LITERAL2,  LITERAL3,  LITERAL4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9612 