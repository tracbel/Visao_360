/* ==============================================================
   Objeto ..........: dbo.BI_EMPRESA
   Tipo ............: VIEW
   Criado em .......: 2015-08-03 12:29:00
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_EMPRESA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW BI_EMPRESA AS  SELECT EMP.NROEMPRESA,        EMP.NOMEREDUZIDO as EMPRESA,        MTZ.NOMEREDUZIDO AS MATRIZ  FROM GE_EMPRESA EMP    JOIN GE_EMPRESA MTZ ON MTZ.NROEMPRESA = EMP.MATRIZ    WHERE EMP.EMUSO = 1