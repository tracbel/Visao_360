/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_COLHEITADEIRA_GR
   Tipo ............: VIEW
   Criado em .......: 2025-11-28 15:16:00
   Modificado em ...: 2025-11-28 15:16:00
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_COLHEITADEIRA_GR ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  SEPARADOR,  TIPO_DA_PLATAFORMA,  ANO,  QUANTIDADE,  TAM_PLATAFORMA__L_P)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO2,  CAMPO3,  CAMPO4,  CAMPO6,  NUMERO1,  NUMERO4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9503 