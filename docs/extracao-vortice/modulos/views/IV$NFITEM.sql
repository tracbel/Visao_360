/* ==============================================================
   Objeto ..........: dbo.IV$NFITEM
   Tipo ............: VIEW
   Criado em .......: 2021-08-17 15:29:21
   Modificado em ...: 2021-08-17 15:29:21
   Linhas ..........: 20
   Escreve em tabela: nao
   Tabelas referidas: EXT_NFS, EXT_NFSITEM, EXT_Produto
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[IV$NFITEM]
(idnfsaida, origem, nroempresa, iditem, codproduto, descproduto, qtdepedida, qtdeatendida, vlrunitario, vlrdescto, situacao, vlricms, obs, tipoitem)
AS
SELECT NFI.IDNFS,
       NFS.ORIGEM,
       NFS.NROEMPRESA,
       NFI.IDITEM,
	   (SELECT CODPRODUTO FROM EXT_Produto WHERE IdProduto = NFI.IdProduto) AS CODPRODUTO,
	   (SELECT DESCRICAO FROM EXT_Produto WHERE IdProduto = NFI.IdProduto) AS CODPRODUTO,
       0,
       NFI.QTDE,
       NFI.VLRUNITARIO,
       NFI.VLRDESCTO,
       NFI.SITUACAO,
       NFI.VLRICM,
       NFI.OBS,
       'ITEM'
FROM EXT_NFSITEM NFI , EXT_NFS NFS
WHERE NFS.IDNFS = NFI.IDNFS
