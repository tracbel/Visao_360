/* ==============================================================
   Objeto ..........: dbo.ZZ_IV$OSSolicitacao
   Tipo ............: VIEW
   Criado em .......: 2012-05-31 16:06:45
   Modificado em ...: 2017-01-25 12:07:36
   Linhas ..........: 9
   Escreve em tabela: nao
   Tabelas referidas: EXT_OS, EXT_OSSOLIC
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW IV$OSSolicitacao ( IDOS, NROEMPRESA,
       NROOS, SeqSolicitacao, Codigo, Descricao )
AS
SELECT OS.IDOS , OS.NROEMPRESA, OS.NROOS      AS NROOS,
       SOL.IDSOLIC    AS SEQSOLICITACAO,
       SOL.CODIGO    AS CODIGO,
       SOL.DESCRICAO  AS DESCRICAO
  FROM EXT_OS OS, EXT_OSSOLIC SOL
WHERE OS.IDOS = SOL.IDOS