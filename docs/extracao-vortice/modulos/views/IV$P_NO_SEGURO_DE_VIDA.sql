/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_SEGURO_DE_VIDA
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 08:57:51
   Modificado em ...: 2025-04-14 08:57:51
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_SEGURO_DE_VIDA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MARCA,  SEGURADO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  SIMNAO1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9622 