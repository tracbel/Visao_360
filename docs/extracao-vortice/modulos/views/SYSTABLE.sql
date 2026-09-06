/* ==============================================================
   Objeto ..........: dbo.SYSTABLE
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: SYSCONVERT1
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

   create view SYSTABLE(CREATOR, NAME, COLCOUNT, TYPE, 	REMARKS, PERCENTFREE, ID, 	DTACRIACAO)        as          select USER_NAME(T.uid), T.name,  	      (select count(*) from dbo.syscolumns C where C.id = T.id),    	      S.OUTSTR,'',100 , T.id, T.CRDATE        from   dbo.sysobjects T, SYSCONVERT1 S         where  T.type != 'S' and T.type = S.INSTR and  	      (USER_NAME(T.uid) != 'dbo' or T.name NOT IN  	       ('SYSCONVERT1', 'SYSCONVERT2', 'SYSCONVERT3', 'SYSDUMMY'))   