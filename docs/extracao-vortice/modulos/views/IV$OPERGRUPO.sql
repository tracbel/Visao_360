/* ==============================================================
   Objeto ..........: dbo.IV$OPERGRUPO
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_MEMBRO, GE_USUARIO, IV_OPERADOR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

    CREATE VIEW IV$OPERGRUPO    ( CODUSUARIO, SEQUSUARIO , NIVEL, NOME, GRUPO , FUNCAO ,      TIPOUSUARIO, IDENTIFICACAO , STATUS , JUSTIFDISPON, SUPERVISOR ) AS SELECT U.CODUSUARIO, U.SEQUSUARIO , NIVEL, NOME, M.GRUPO , O.FUNCAO ,       U.TIPOUSUARIO , U.IDENTIFICACAO   , O.STATUS   , O.JUSTIFDISPON, O.SUPERVISOR FROM GE_USUARIO U JOIN IV_OPERADOR O ON O.SEQUSUARIO = U.SEQUSUARIO  JOIN GE_MEMBRO M ON M.USUARIO = U.SEQUSUARIO  WHERE NIVEL > 0      AND EXISTS (SELECT 1 FROM GE_USUARIO G                  WHERE G.SEQUSUARIO = M.GRUPO                   AND G.NIVEL > 0  ) UNION SELECT U.CODUSUARIO, U.SEQUSUARIO , NIVEL, NOME,0, 'G',             U.TIPOUSUARIO  ,  U.IDENTIFICACAO , 'Grupo'  , 'Grupo', 'Grupo'   FROM GE_USUARIO U   WHERE  U.TIPOUSUARIO = 'G'      AND NIVEL > 0   