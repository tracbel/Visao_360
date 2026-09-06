/* ==============================================================
   Objeto ..........: dbo.IV$P_COLHEITADEIRA_GRAOS
   Tipo ............: VIEW
   Criado em .......: 2026-01-13 08:11:00
   Modificado em ...: 2026-01-13 08:11:00
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_COLHEITADEIRA_GRAOS ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MODELO,  SEPARADOR,  TIPO_DA_PLATAFORMA,  ANO,  QUANTIDADE,  TAM_PLATAFORMA__L_P)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO2,  CAMPO3,  CAMPO4,  CAMPO6,  CAMPO7,  NUMERO4 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9403 