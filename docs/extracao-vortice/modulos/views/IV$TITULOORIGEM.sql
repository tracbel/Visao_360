/* ==============================================================
   Objeto ..........: dbo.IV$TITULOORIGEM
   Tipo ............: VIEW
   Criado em .......: 2017-09-11 11:51:24
   Modificado em ...: 2017-09-15 16:12:31
   Linhas ..........: 2
   Escreve em tabela: nao
   Tabelas referidas: GE_EMPRESA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[IV$TITULOORIGEM]     ( NROEMPRESA, ORIGEM  )  AS  SELECT DISTINCT NROEMPRESA, 'COLORADO'     FROM GE_EMPRESA
