/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_JOHN_DEERE
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 09:00:07
   Modificado em ...: 2025-04-14 09:00:07
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_JOHN_DEERE ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  COMBUSTIVERL,  NOVO_USADO,  FORMA_DE_COMPRA,  ANO_MODELO,  ANO_FABRICACAO,  VALOR_COMPRA,  DATA_VENDA,  REVENDA,  VENDEDOR,  COR,  FROTA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO3,  CAMPO4,  NUMERO1,  NUMERO2,  NUMERO3,  DATA1,  LITERAL1,  LITERAL2,  LITERAL5,  LITERAL6 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9614 