/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_TRATOR_SISDIA
   Tipo ............: VIEW
   Criado em .......: 2025-01-27 09:44:14
   Modificado em ...: 2025-01-27 09:44:14
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_TRATOR_SISDIA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  COMBUSTIVEL,  NOVO_USADO,  FORMA_DE_COMPRA,  ANO_MODELO,  ANO_FABRICACAO,  VALOR_COMPRA,  DATA_VENDA,  REVENDA,  VENDEDOR,  COR,  FROTA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO3,  CAMPO4,  NUMERO1,  NUMERO2,  NUMERO3,  DATA1,  LITERAL1,  LITERAL2,  LITERAL5,  LITERAL6 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9604 