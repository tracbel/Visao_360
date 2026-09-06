/* ==============================================================
   Objeto ..........: dbo.V_C5SYSCONSTRAINT
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

	 CREATE VIEW V_C5SYSCONSTRAINT    ( DONO, TBNOME, TBREF , CONSTRNAME,  CONSTRTYPE, TABID, UPDRULE, DELRULE  ) AS   SELECT UPPER(USER_NAME(TABS.uid)) AS DONO, UPPER(TABS.NAME) AS TBNOME,     UPPER(TREF.NAME) AS TBREF,     FKEY.NAME AS CONSTRNAME,     FKEY.TYPE, TABS.ID AS TABID, UPDATE_REFERENTIAL_ACTION_DESC AS UPRULE ,     DELETE_REFERENTIAL_ACTION_DESC AS DELRULE   FROM  sys.FOREIGN_KEYS FKEY    JOIN  SYSOBJECTS TABS ON TABS.ID = FKEY.PARENT_OBJECT_ID       JOIN  SYSOBJECTS TREF ON TREF.ID = FKEY.REFERENCED_OBJECT_ID  