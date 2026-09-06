/* ==============================================================
   Objeto ..........: dbo.BI_DEPTO
   Tipo ............: VIEW
   Criado em .......: 2015-08-03 12:29:29
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IVS_DEPTO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW BI_DEPTO AS  SELECT D.SEQDEPTO, D.DEPTO, D.DESCRICAO AS DEPARTAMENTO   FROM IVS_DEPTO D