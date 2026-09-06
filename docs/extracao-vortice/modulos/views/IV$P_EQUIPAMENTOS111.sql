/* ==============================================================
   Objeto ..........: dbo.IV$P_EQUIPAMENTOS111
   Tipo ............: VIEW
   Criado em .......: 2018-03-20 12:41:45
   Modificado em ...: 2018-03-20 12:41:45
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_EQUIPAMENTOS111 ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem,  DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  FAMILIA,  MODELO,  NOVO_USADO,  ANO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM,  DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO5,  CAMPO6 FROM IV_CLIENTEPROPR        WHERE SEQPROPRIEDADE = 6