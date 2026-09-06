/* ==============================================================
   Objeto ..........: dbo.IV$P_LOCACAO
   Tipo ............: VIEW
   Criado em .......: 2017-12-08 09:37:54
   Modificado em ...: 2017-12-08 09:37:54
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_LOCACAO ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem,  DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MARCA,  FAMILIA,  MODELO,  VALOR_MENSAL,  VALOR_TOTAL,  INICIO_CONTRATO,  RENOVACAO,  TERMINO_CONTRATO,  MARCA8,  FAMILIA9,  MODELO10,  N__CONTRATO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM,  DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO3,  NUMERO1,  NUMERO2,  DATA1,  DATA2,  DATA3,  LITERAL1,  LITERAL2,  LITERAL3,  LITERAL4 FROM IV_CLIENTEPROPR        WHERE SEQPROPRIEDADE = 5