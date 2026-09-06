/* ==============================================================
   Objeto ..........: dbo.IV$DOITRES
   Tipo ............: VIEW
   Criado em .......: 2023-06-05 18:10:09
   Modificado em ...: 2023-06-05 18:10:09
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_USUARIO, IV_CLASSERES, IV_RESCLASSE, IV_RESULTADO
   Outras refs .....: GE$USUARIOPERM
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

  CREATE VIEW IV$DOITRES AS  SELECT DISTINCT 'DOIT' AS TIPO         ,ISNULL ( RES.ACAO, 0 ) AS ACAO         ,USR.CODUSUARIO         ,RES.RESULTADO         ,RES.DESCRICAO as DESCRICAO         ,CASE RES.CTRLDETALHE WHEN 1 THEN 1 WHEN 9 THEN 1 ELSE 0 END AS EXIGEDETALHE         ,CASE RES.CTRLCOMPLEMENTO WHEN 1 THEN 1 WHEN 9 THEN 1 ELSE 0 END AS TEMCOMPLEMENTO         ,1 as NROEMPRESA         ,3155 as SEQPESSOA  FROM GE$USUARIOPERM UPRM  JOIN IV_RESULTADO RES       ON RES.RESULTADO = UPRM.CHAVEAPLICACAO       AND RES.RESULTADO IN ( SELECT RCL.RESULTADO                             FROM IV_CLASSERES CLASR                             JOIN IV_RESCLASSE RCL ON CLASR.SEQCLASSERES = RCL.SEQCLASSERES                             WHERE CLASR.CLASSE = 'DOIT')  JOIN GE_USUARIO USR ON USR.SEQUSUARIO = UPRM.SEQUSUARIO AND USR.TIPOUSUARIO = 'U' AND USR.NIVEL > 0  WHERE UPRM.CODAPLICACAO = 'IV_RESULTADO'  AND UPRM.PERMISSAO >= '2'