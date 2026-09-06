/* ==============================================================
   Objeto ..........: dbo.IV$P_NO_PROPRIEDADE_RURAL
   Tipo ............: VIEW
   Criado em .......: 2025-04-14 08:58:33
   Modificado em ...: 2025-04-14 08:58:33
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_NO_PROPRIEDADE_RURAL ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  NOME,  ENDERECO,  I_E,  CNPJ,  CEP,  CIDADE)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  LITERAL1,  LITERAL2,  LITERAL3,  LITERAL4,  LITERAL5,  LITERAL6 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 9618 