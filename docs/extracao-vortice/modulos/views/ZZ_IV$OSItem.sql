/* ==============================================================
   Objeto ..........: dbo.ZZ_IV$OSItem
   Tipo ............: VIEW
   Criado em .......: 2012-05-31 16:07:43
   Modificado em ...: 2017-01-25 12:07:31
   Linhas ..........: 11
   Escreve em tabela: nao
   Tabelas referidas: EXT_OS, EXT_OSITEM
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE  VIEW [dbo].[IV$OSItem]  ( IDOS, NROEMPRESA, NroOS  ,
   SeqItem , TipoItem , Produtivo ,
  StatusItem ,  Codigo  , Descricao ,  Qtde  ,Valor )
as 
SELECT OS.NROOS AS IDOS, OS.NROEMPRESA, OS.NROOS   AS NROOS, 
       IT.IDITEM, IT.TIPOITEM , IT.PRODUTIVO,
       IT.STATUSITEM, IT.CODIGO , IT.DESCRICAO , 
        IT.QTDE , 
        (IT.QTDE * IT.VALOR ) valor
  FROM EXT_OS OS, EXT_OSITEM IT
 WHERE OS.IDOS = IT.IDOS