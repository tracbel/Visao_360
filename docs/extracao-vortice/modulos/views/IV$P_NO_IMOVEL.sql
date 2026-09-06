/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_IMOVEL
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 09:01:00
   Modificado em ...: 2025-04-14 09:01:00
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_IMOVEL ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MARCA,  SEGURADO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  SIMNAO1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9621 