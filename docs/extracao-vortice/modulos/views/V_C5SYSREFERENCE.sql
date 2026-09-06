/* ==============================================================
   Objeto ..........: dbo.V_C5SYSREFERENCE
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Outras refs .....: SYSTABLE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

		            Create View V_C5SYSREFERENCE (  TBNOME, TBREF )  AS   SELECT TP.NAME , TF.NAME FROM SYSTABLE TP, SYSTABLE TF,  SYSREFERENCES REF WHERE TP.ID = REF.RKEYID   AND TF.ID = REF.FKEYID   