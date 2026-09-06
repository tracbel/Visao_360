/* ==============================================================
   Objeto ..........: dbo.IV$NFSAIDA
   Tipo ............: VIEW
   Criado em .......: 2021-08-17 15:27:36
   Modificado em ...: 2021-08-17 15:27:36
   Linhas ..........: 26
   Escreve em tabela: nao
   Tabelas referidas: EXT_NFS, EXT_NFSOper, EXT_Vendedor
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE view [dbo].[IV$NFSAIDA]
( IDNFSAIDA, ORIGEM, NROEMPRESA, NRONF, SERIENF,
SEQPESSOA, OPERACAO, FORMAPGTO, VENDEDOR,
NROVENDEDOR, NROPEDIDO, DTAPEDIDO, DTAEMISSAONF,
DTAALTERACAO, SITUACAO, USUARIO,
OBS)
AS
SELECT NFS.IDNFS AS IDNFSSAIDA,
NFS.ORIGEM,
NFS.NROEMPRESA,
NFS.NRONF,
NFS.SERIENF,
NFS.SeqPessoa,
(SELECT DESCREDUZIDA FROM EXT_NFSOper WHERE IdNFSOper = NFS.IdNFSOper ) AS OPERACAO,
NFS.FORMAPGTO,
(SELECT NOME FROM EXT_Vendedor WHERE IdVendedor = NFS.IdVendedor ) AS VENDEDOR,
NFS.IdVendedor AS NROVENDEDOR,
NFS.NROPEDIDO,
NFS.DTAPEDIDO,
NFS.DTAEMISSAONF,
NFS.DTAALTERACAO,
NFS.SITUACAO,
NFS.USUARIO,
OBS
FROM EXT_NFS NFS 
