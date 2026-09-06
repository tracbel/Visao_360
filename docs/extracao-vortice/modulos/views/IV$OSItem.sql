/* ==============================================================
   Objeto ..........: dbo.IV$OSItem
   Tipo ............: VIEW
   Criado em .......: 2021-08-18 18:24:25
   Modificado em ...: 2021-08-18 18:24:25
   Linhas ..........: 13
   Escreve em tabela: nao
   Tabelas referidas: EXT_OS, EXT_OSITEM
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[IV$OSItem]  ( IDOS, NROEMPRESA, NroOS  ,
       SeqItem , TipoItem , Produtivo ,
       StatusItem ,  Codigo  , Descricao ,  
       Qtde  ,Valor )
as 
SELECT OS.IDOS, OS.NROEMPRESA, OS.NROOS   AS NROOS, 
       IT.IDITEM, IT.TIPOITEM , IT.PRODUTIVO,
       IT.STATUSITEM, IT.CODIGO , IT.DESCRICAO , 
       IT.QTDE , 
       (IT.VLRTOTITEM ) as valor
  FROM EXT_OS OS, EXT_OSITEM IT
 WHERE OS.IDOS = IT.IDOS ;
