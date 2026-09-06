/* ==============================================================
   Objeto ..........: dbo.IV$A_PROJETO_CULTIVAR
   Tipo ............: VIEW
   Criado em .......: 2020-09-07 16:00:54
   Modificado em ...: 2020-09-07 16:00:54
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_PROJETO_CULTIVAR( SEQPESSOA , PROJETO_CULTIVAR)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'Projeto Cultivar'