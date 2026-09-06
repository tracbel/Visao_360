/* ==============================================================
   Objeto ..........: dbo.IV$A_PERSONA_JOHN_DEERE
   Tipo ............: VIEW
   Criado em .......: 2022-02-15 16:57:30
   Modificado em ...: 2022-02-15 16:57:30
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEATRIB
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$A_PERSONA_JOHN_DEERE( SEQPESSOA , PERSONA_JOHN_DEERE)  AS   	SELECT SEQPESSOA ,   LISTA  FROM IV_CLIENTEATRIB WHERE ATRIBUTO = 'Persona John Deere'