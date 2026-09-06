/* ==============================================================
   Objeto ..........: dbo.ZZ_IV$NFSCMPL
   Tipo ............: VIEW
   Criado em .......: 2011-12-21 11:37:17
   Modificado em ...: 2017-01-25 08:44:08
   Linhas ..........: 9
   Escreve em tabela: nao
   Tabelas referidas: EXT_NFS, EXT_NFSCMPL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE view [dbo].[IV$NFSCMPL]  (  IDNFSAIDA, NRONF, SERIE, ORIGEM, NROEMPRESA, IDCMPL, COMPLEMENTO)
AS  
SELECT NFS.IDNFS, NFS.NRONF, NFS.SERIENF, NFS.ORIGEM,
       NFS.NROEMPRESA, CMPL.IDCMPL,
       CMPL.CMPL
FROM EXT_NFS NFS, EXT_NFSCMPL CMPL
WHERE NFS.IDNFS = CMPL.IDNFS
