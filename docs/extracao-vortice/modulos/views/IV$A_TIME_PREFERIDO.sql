/* ==============================================================
   Objeto ..........: dbo.IV$A_TIME_PREFERIDO
   Tipo ............: VIEW
   Criado em .......: 2018-03-28 15:38:23
   Modificado em ...: 2018-03-28 15:38:23
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_TIME_PREFERIDO( SEQPESSOA , TIME_PREFERIDO)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'Time Preferido'