/* ==============================================================
   Objeto ..........: dbo.IV$A_COR_PREFERIDO
   Tipo ............: VIEW
   Criado em .......: 2013-07-10 15:01:42
   Modificado em ...: 2016-03-22 19:20:06
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_COR_PREFERIDO( SEQPESSOA , COR_PREFERIDO)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'Cor preferido'