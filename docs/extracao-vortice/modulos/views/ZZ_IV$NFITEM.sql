/* ==============================================================
   Objeto ..........: dbo.ZZ_IV$NFITEM
   Tipo ............: VIEW
   Criado em .......: 2011-12-21 11:37:17
   Modificado em ...: 2017-01-25 08:43:55
   Linhas ..........: 23
   Escreve em tabela: nao
   Tabelas referidas: EXT_NFS, EXT_NFSITEM
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */




CREATE VIEW [dbo].[IV$NFITEM]
(idnfsaida, NRONF, SERIE, origem, nroempresa, iditem, codproduto, descproduto, qtdepedida, qtdeatendida, vlrunitario, vlrdescto, situacao, vlricms, obs, tipoitem)
AS
SELECT NFI.IDNFS, NFS.NRONF, NFS.SERIENF,
       NFS.ORIGEM,
       NFS.NROEMPRESA,
       NFI.IDITEM,
       NFI.CODPRODUTO,
       NFI.DESCPRODUTO,
       0,
       NFI.QTDE,
       NFI.VLRUNITRARIO,
       NFI.VLRDESCTO,
       NFI.SITUACAO,
       NFI.VLRICM,
       NFI.OBS,
       'ITEM'
FROM EXT_NFSITEM NFI , EXT_NFS NFS
WHERE NFS.IDNFS = NFI.IDNFS
