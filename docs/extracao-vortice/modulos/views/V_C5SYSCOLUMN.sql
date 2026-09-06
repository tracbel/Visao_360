/* ==============================================================
   Objeto ..........: dbo.V_C5SYSCOLUMN
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Outras refs .....: SYSCOLUMN
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

		            Create View V_C5SYSCOLUMN (  COLNOME, TBNOME, TBDONO, COLTIPODADO, COLTAMANHO, COLdecimal, COLNULL, COLROTULO, COLNOTA, COLNRO)  AS  SELECT NAME, TBNAME, TBCREATOR, COLTYPE, LENGTH, SCALE, NULLS, NULL, REMARKS, COLNO   FROM SYSCOLUMN   