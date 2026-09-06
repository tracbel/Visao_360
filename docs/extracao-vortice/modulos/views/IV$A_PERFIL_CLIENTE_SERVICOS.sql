/* ==============================================================
   Objeto ..........: dbo.IV$A_PERFIL_CLIENTE_SERVICOS
   Tipo ............: VIEW
   Criado em .......: 2013-03-11 09:38:07
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_PERFIL_CLIENTE_SERVICOS( SEQPESSOA , PERFIL_CLIENTE_SERVICOS)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'PERFIL CLIENTE SERVIÇOS'