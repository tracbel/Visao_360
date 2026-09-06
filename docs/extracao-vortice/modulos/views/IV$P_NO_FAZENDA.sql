/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_FAZENDA
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 09:01:47
   Modificado em ...: 2025-04-14 09:01:47
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_FAZENDA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  NOME_DA_FAZENDA,  CNPJ,  MUNICIPIO,  I_E,  AREA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  LITERAL1,  LITERAL2,  LITERAL3,  LITERAL4,  LITERAL5 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9613 