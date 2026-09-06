/* ==============================================================
   Objeto ..........: dbo.IV$TITULO
   Tipo ............: VIEW
   Criado em .......: 2017-09-11 11:49:35
   Modificado em ...: 2018-02-27 10:02:52
   Linhas ..........: 55
   Escreve em tabela: nao
   Tabelas referidas: EXT_TIT_ACRESC, EXT_TITULO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */




CREATE VIEW [dbo].[IV$TITULO] (         
IDTITULO,           
NROEMPRESA,         
NROEMPRESACOBR,         
SEQPESSOA,      
ESPECIE,   
NROTITULO,   
VLRORIGINAL, 
DTAEMISSAO,    
DTAVENCTO,       
QTDEDIA,  
QUITADO,  
NRODOCTO, 
LOCALCOBR, 
TIPOCOBR,
VLRABERTO, 
VLRACRESCIMO,  
VLRABATIMENTO,          
VLRPAGO,    
DTAPGTO,     
COBJURIDICA,     
ORIGEM,      
OBS ) AS 
  
SELECT TIT.IDTITULO AS SEQTITULO,
       TIT.NROEMPRESA AS NROEMPRESA,
       TIT.NROEMPRESA AS NROEMPRESACOBR,
       TIT.SEQPESSOA,
       TIT.ESPECIE,
       TIT.NROTITULO,
       TIT.VLRORIGINAL,
       TIT.DTAEMISSAO,
       TIT.DTAVENCTO AS DTAVENCTO,
       DATEDIFF(DAY, TIT.DTAVENCTO, GETDATE()) AS QTDEDIA,
       ISNULL(TIT.INDQUITADO, 0) AS QUITADO,
       TIT.NRODOCTO,
       TIT.LOCALCOBRANCA AS LOCALCOBR,
       TIT.TIPOCOBRANCA AS TIPOCOBR,
       ISNULL(TIT.VLRABERTO + ACR.VALOR, TIT.VlrAberto),
       ACR.VALOR,
       TIT.VLRABATIMENTO,
       TIT.VLRPAGO,
       TIT.DTAQUITACAO AS DTAPGTO,
       TIT.INDCOBRJURIDICA AS COBJURIDICA,
       TIT.ORIGEM,
       TIT.LINKSTR AS OBS
  FROM EXT_TITULO TIT
       LEFT JOIN EXT_TIT_ACRESC ACR ON CAST(ACR.LINKSTR AS VARCHAR(30)) = SUBSTRING(TIT.LINKSTR, 4, LEN(TIT.LINKSTR))
 WHERE TIT.INDATIVO = 1


