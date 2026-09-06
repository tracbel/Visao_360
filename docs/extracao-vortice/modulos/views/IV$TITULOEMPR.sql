/* ==============================================================
   Objeto ..........: dbo.IV$TITULOEMPR
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: EXT_TITULO, GE_EMPRESA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

    CREATE VIEW IV$TITULOEMPR    ( EMPRESA, NROEMPRESA ) AS  SELECT EMP.NOMEREDUZIDO, EMP.NROEMPRESA    FROM GE_EMPRESA EMP    WHERE EXISTS ( SELECT 1 FROM EXT_TITULO TIT WHERE TIT.NROEMPRESA = EMP.NROEMPRESA)  