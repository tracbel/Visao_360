/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_TRATOR_MANUAL_FRO
   Tipo ............: VIEW
   Criado em .......: 2025-01-07 16:19:25
   Modificado em ...: 2025-01-07 16:19:25
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_TRATOR_MANUAL_FRO ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  COMBUSTIVEL,  NOVO_USADO,  FORMA_DE_COMPRA,  MODELO,  ANO_MODELO,  ANO_FABRICACAO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO2,  CAMPO3,  CAMPO4,  NUMERO1,  NUMERO2,  NUMERO3 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9603 