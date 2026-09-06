/* ==============================================================
   Objeto ..........: dbo.IV$A_BENEFICIO
   Tipo ............: VIEW
   Criado em .......: 2020-09-07 16:42:37
   Modificado em ...: 2020-09-07 16:42:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_BENEFICIO( SEQPESSOA , BENEFICIO)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'Benefício'