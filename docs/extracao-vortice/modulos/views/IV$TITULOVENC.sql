/* ==============================================================
   Objeto ..........: dbo.IV$TITULOVENC
   Tipo ............: VIEW
   Criado em .......: 2017-10-20 15:25:35
   Modificado em ...: 2017-12-27 11:15:44
   Linhas ..........: 23
   Escreve em tabela: nao
   Tabelas referidas: EXT_TITULO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */



CREATE VIEW [dbo].[IV$TITULOVENC]   (
        IDTITULO, 
        NROEMPRESACOBR,  
        SEQPESSOA,  
        QTDEDIA , 
        TIPOCOBR,
        VLRABERTO  ,
        ORIGEM,
        DTAVENCTO  ) AS 
SELECT TIT.IDTITULO
     , TIT.NROEMPRESA
     , TIT.SEQPESSOA
     , DATEDIFF(DAY, ISNULL(TIT.DTAVENCTO, TIT.DTAVENCTOORIG), GETDATE())
     , TIT.TIPOCOBRANCA
     , TIT.VlrAberto
     , TIT.ORIGEM
     , TIT.DTAVENCTO
  FROM EXT_TITULO TIT
WHERE TIT.INDQUITADO = 0
AND TIT.INDATIVO = 1
