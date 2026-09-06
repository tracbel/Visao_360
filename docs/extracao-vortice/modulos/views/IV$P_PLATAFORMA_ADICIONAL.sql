/* ==============================================================
   Objeto ..........: dbo.IV$P_PLATAFORMA_ADICIONAL
   Tipo ............: VIEW
   Criado em .......: 2021-05-03 14:00:24
   Modificado em ...: 2021-05-03 14:00:24
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_PLATAFORMA_ADICIONAL ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem,  DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  MODELO,  ANO,  TAM_PLATAFORMA__L_P)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM,  DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO6,  NUMERO4 FROM IV_CLIENTEPROPR        WHERE SEQPROPRIEDADE = 9404