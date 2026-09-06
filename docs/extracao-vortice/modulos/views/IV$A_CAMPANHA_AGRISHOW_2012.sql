/* ==============================================================
   Objeto ..........: dbo.IV$A_CAMPANHA_AGRISHOW_2012
   Tipo ............: VIEW
   Criado em .......: 2013-03-11 09:38:13
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_CAMPANHA_AGRISHOW_2012( SEQPESSOA , CAMPANHA_AGRISHOW_2012)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'CAMPANHA AGRISHOW_2012'