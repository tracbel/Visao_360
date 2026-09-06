/* ==============================================================
   Objeto ..........: dbo.IV$P_CONTRATO_GFC
   Tipo ............: VIEW
   Criado em .......: 2024-09-12 09:50:38
   Modificado em ...: 2024-09-12 09:50:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_CONTRATO_GFC ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  HORIMETRO,  DATA_DE_ATUALIZACAO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  NUMERO1,  DATA1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 19 