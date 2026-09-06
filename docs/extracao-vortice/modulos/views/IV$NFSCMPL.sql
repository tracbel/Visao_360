/* ==============================================================
   Objeto ..........: dbo.IV$NFSCMPL
   Tipo ............: VIEW
   Criado em .......: 2021-08-17 15:30:17
   Modificado em ...: 2021-08-17 15:30:17
   Linhas ..........: 8
   Escreve em tabela: nao
   Tabelas referidas: EXT_NFS, EXT_NFSCMPL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

Create  view [dbo].[IV$NFSCMPL]  (  IDNFSAIDA, ORIGEM, NROEMPRESA, IDCMPL, COMPLEMENTO)
AS  
SELECT NFS.IDNFS, NFS.ORIGEM,
       NFS.NROEMPRESA, CMPL.IDCMPL,
       CMPL.CMPL
FROM EXT_NFS NFS, EXT_NFSCMPL CMPL
WHERE NFS.IDNFS = CMPL.IDNFS;
