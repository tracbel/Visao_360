/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_ESTANCIA
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 09:02:00
   Modificado em ...: 2025-04-14 09:02:00
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_ESTANCIA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  ENDERECO,  MUNICIPIO,  CNPJ,  I_E)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  LITERAL1,  LITERAL2,  LITERAL3,  LITERAL4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9617 