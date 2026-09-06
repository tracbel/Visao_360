/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_AREA_TOTAL
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 09:03:09
   Modificado em ...: 2025-04-14 09:03:09
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_AREA_TOTAL ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  AREA_IRRIG_GOTEJO__H,  AREA_PECUARIA__HA,  AREA_GRAOS__HA,  AREA_ALGODAO__HA,  AREA_CANA__HA,  AREA_IRRGI_ASPERS__H,  QTDE_ARRENDADA__HA,  QTDE_PROPRIA__HA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  NUMERO1,  NUMERO2,  NUMERO3,  NUMERO4,  NUMERO5,  NUMERO6,  LITERAL5,  LITERAL9 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9512 