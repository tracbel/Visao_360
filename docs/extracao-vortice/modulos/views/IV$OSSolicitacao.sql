/* ==============================================================
   Objeto ..........: dbo.IV$OSSolicitacao
   Tipo ............: VIEW
   Criado em .......: 2021-08-18 18:25:16
   Modificado em ...: 2021-08-18 18:25:16
   Linhas ..........: 10
   Escreve em tabela: nao
   Tabelas referidas: EXT_OS, EXT_OSSOLIC
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[IV$OSSolicitacao] ( IDOS, NROEMPRESA,
       NROOS, SeqSolicitacao, Codigo, Descricao )
AS
SELECT OS.IDOS , OS.NROEMPRESA, OS.NROOS      AS NROOS,
       SOL.IDSOLIC    AS SEQSOLICITACAO,
       SOL.CODIGO    AS CODIGO,
       SOL.DESCRICAO  AS DESCRICAO
  FROM EXT_OS OS, EXT_OSSOLIC SOL
WHERE OS.IDOS = SOL.IDOS ;
