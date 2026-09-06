/* ==============================================================
   Objeto ..........: dbo.ZZ_IV$NFSAIDA
   Tipo ............: VIEW
   Criado em .......: 2011-12-21 11:37:17
   Modificado em ...: 2017-01-25 08:44:05
   Linhas ..........: 28
   Escreve em tabela: nao
   Tabelas referidas: EXT_NFS, GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE view [dbo].[IV$NFSAIDA] 
(  IDNFSAIDA, ORIGEM, NROEMPRESA, NRONF, SERIENF, SERIE,
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
       NFS.SERIENF,
       (SELECT MIN(SEQPESSOA) FROM GE_PESSOA WHERE GE_PESSOA.NROCGCCPF = NFS.NROCGCCPF ),
       NFS.OPERACAO,
       NFS.FORMAPGTO,
       NFS.VENDEDOR,
       NFS.NROVENDEDOR,
       NFS.NROPEDIDO,
       NFS.DTAPEDIDO,
       NFS.DTAEMISSAONF,
       NFS.DTAALTERACAO,
       NFS.SITUACAO,
       NFS.USUARIO,
       OBS
FROM EXT_NFS NFS 
