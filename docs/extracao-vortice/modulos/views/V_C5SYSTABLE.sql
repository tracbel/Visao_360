/* ==============================================================
   Objeto ..........: dbo.V_C5SYSTABLE
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Outras refs .....: SYSTABLE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

     Create View V_C5SYSTABLE ( TBNOME, TBDONO, QTDECOLUNA, TBTIPO, COLROTULO, COLNOTA)  AS  SELECT NAME , CREATOR, COLCOUNT, TYPE, NULL, REMARKS  FROM SYSTABLE   