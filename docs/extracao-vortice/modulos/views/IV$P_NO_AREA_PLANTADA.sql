/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_AREA_PLANTADA
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 09:03:28
   Modificado em ...: 2025-04-14 09:03:28
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_AREA_PLANTADA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO_CULTURA,  HECTARES,  CIDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  NUMERO1,  LITERAL1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9611 