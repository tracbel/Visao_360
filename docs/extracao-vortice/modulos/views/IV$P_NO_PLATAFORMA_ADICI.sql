/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_PLATAFORMA_ADICI
   Tipo ............: VIEW
   Criado em .......: 2025-11-28 15:20:59
   Modificado em ...: 2025-11-28 15:20:59
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_PLATAFORMA_ADICI ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  ANO,  QUANTIDADE,  TAM_PLATAFORMA__L_P)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO6,  NUMERO1,  NUMERO4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9504 