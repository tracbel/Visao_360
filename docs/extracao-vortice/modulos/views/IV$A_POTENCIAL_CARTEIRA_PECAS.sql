/* ==============================================================
   Objeto ..........: dbo.IV$A_POTENCIAL_CARTEIRA_PECAS
   Tipo ............: VIEW
   Criado em .......: 2020-07-29 15:43:54
   Modificado em ...: 2020-07-29 15:43:54
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_POTENCIAL_CARTEIRA_PECAS( SEQPESSOA , POTENCIAL_CARTEIRA_PECAS)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'Potencial Carteira Peças'