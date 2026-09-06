/* ==============================================================
   Objeto ..........: dbo.IV$OPERADOR
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_MEMBRO, GE_USUARIO, IV_OPERADOR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

                         CREATE  VIEW IV$OPERADOR ( CODUSUARIO, SEQUSUARIO, NIVEL, NOME, GRUPO, FUNCAO  , IDENTIFICACAO, RECEBECIENCIA , TIPOUSUARIO)    AS   SELECT U.CODUSUARIO, U.SEQUSUARIO, U.NIVEL, U.NOME, M.GRUPO, O.FUNCAO , U.IDENTIFICACAO, U.RECEBECIENCIA, U.TIPOUSUARIO FROM GE_USUARIO U JOIN IV_OPERADOR O ON O.SEQUSUARIO = U.SEQUSUARIO JOIN GE_MEMBRO M ON M.USUARIO =  U.SEQUSUARIO      AND M.GRUPO != (SELECT SEQUSUARIO FROM GE_USUARIO WHERE CODUSUARIO =  'todos') WHERE NIVEL >  0    AND EXISTS (SELECT 1 FROM GE_USUARIO G                  WHERE G.SEQUSUARIO = M.GRUPO                   AND G.NIVEL > 0  )  