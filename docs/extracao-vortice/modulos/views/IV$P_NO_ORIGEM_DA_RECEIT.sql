/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_ORIGEM_DA_RECEIT
   Tipo ............: VIEW
   Criado em .......: 2025-12-04 17:30:50
   Modificado em ...: 2025-12-04 17:30:50
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_ORIGEM_DA_RECEIT ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  PESO,  TIPO_DE_AREA,  AREA_HA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO3,  CAMPO5,  NUMERO4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9500 