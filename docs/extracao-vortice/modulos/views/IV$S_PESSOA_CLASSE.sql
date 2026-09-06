/* ==============================================================
   Objeto ..........: dbo.IV$S_PESSOA_CLASSE
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOACLASSE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$S_PESSOA_CLASSE AS SELECT SEQPESSOA, PC.CLASSE AS CLASSE FROM GE_PESSOACLASSE PC   