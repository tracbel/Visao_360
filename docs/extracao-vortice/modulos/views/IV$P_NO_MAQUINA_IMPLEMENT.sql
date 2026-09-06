/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_MAQUINA_IMPLEMENT
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 08:59:48
   Modificado em ...: 2025-04-14 08:59:48
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_MAQUINA_IMPLEMENT ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  MARCA,  MODELO,  CODMODELO,  NOVO_USADO,  SEGURADO,  ANO_FABRIC,  HORIMETRO,  FROTA)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO3,  CAMPO4,  CAMPO5,  SIMNAO1,  NUMERO1,  NUMERO2,  LITERAL1 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9619 