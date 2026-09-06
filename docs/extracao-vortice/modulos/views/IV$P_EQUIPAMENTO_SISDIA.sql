/* ==============================================================
   Objeto ..........: dbo.IV$P_EQUIPAMENTO_SISDIA
   Tipo ............: VIEW
   Criado em .......: 2018-03-23 14:38:36
   Modificado em ...: 2018-03-23 14:38:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_EQUIPAMENTO_SISDIA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem,  DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  COMBUSTIVEL,  ESTADO_DA_COMPRA,  FORMA_DE_COMPRA,  FORMA_DE_NEGOCIACAO,  ANO_MODELO,  ANO_FABRICACAO,  VALOR,  DATA_DA_VENDA,  DATA_PREV_QUITACAO,  REVENDA,  VENDEDOR,  FINANCIADOR,  USO_DO_VEICULO,  COR,  PLACA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM,  DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO3,  CAMPO4,  CAMPO5,  NUMERO1,  NUMERO2,  NUMERO3,  DATA1,  DATA2,  LITERAL1,  LITERAL2,  LITERAL3,  LITERAL4,  LITERAL5,  LITERAL6 FROM IV_CLIENTEPROPR        WHERE SEQPROPRIEDADE = 4