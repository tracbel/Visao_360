/* ==============================================================
   Objeto ..........: dbo.SYSCOLUMN
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: SYSCONVERT2, SYSCONVERT3
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

		      						   CREATE VIEW SYSCOLUMN					       	    (TBCREATOR, NAME, TBNAME, COLNO, COLTYPE, LENGTH,  	     NULLS, UPdatetimeS, REMARKS, SCALE)        AS        SELECT      USER_NAME(B.uid),      A.name,      B.name,      A.colid,      C.SYSTYPE,     A.length,      D.OUTVAL,      'Y',      '',      2     FROM      dbo.syscolumns A INNER JOIN      dbo.sysobjects B ON A.id = B.id INNER JOIN      SYSCONVERT2 C ON A.type = C.SQSTYPE LEFT JOIN      SYSCONVERT3 D ON A.status = D.INVAL WHERE 1 = 1      AND  (USER_NAME(B.uid)  != 'dbo' OR B.name NOT IN ('SYSCONVERT1', 'SYSCONVERT2', 'SYSCONVERT3', 'SYSDUMMY'))  