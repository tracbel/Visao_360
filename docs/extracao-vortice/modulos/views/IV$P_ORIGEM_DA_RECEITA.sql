/* ==============================================================
   Objeto ..........: dbo.IV$P_ORIGEM_DA_RECEITA
   Tipo ............: VIEW
   Criado em .......: 2025-06-09 09:39:16
   Modificado em ...: 2025-06-09 09:39:16
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_ORIGEM_DA_RECEITA ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  TIPO,  PESO,  AREA_COMPARTILHADA,  AREA_PRINCIPAL,  AREA_HA,  TERMINO_ARRENDAMENTO,  TIPO_DE_PROPRIEDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO3,  CAMPO6,  CAMPO7,  NUMERO4,  DATA1,  LITERAL2 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9400 