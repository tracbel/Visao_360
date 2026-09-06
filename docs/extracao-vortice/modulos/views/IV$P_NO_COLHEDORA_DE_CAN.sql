/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_COLHEDORA_DE_CAN
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 08:48:58
   Modificado em ...: 2025-04-14 08:48:58
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_COLHEDORA_DE_CAN ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  ANO,  QUANTIDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO2,  CAMPO6,  NUMERO1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9511 