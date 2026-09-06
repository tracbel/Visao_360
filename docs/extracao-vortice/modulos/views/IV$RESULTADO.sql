/* ==============================================================
   Objeto ..........: dbo.IV$RESULTADO
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_ACAO, IV_CODPROCESSO, IV_PROCRESULTADO, IV_RESULTADO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$RESULTADO (resultado, acao, descricao, descreduzida, ordem, descricaofull, descredfull, acaodescricao, acaodescreduzida, emuso, temcomplemento, ctrlcomplemento, codprocesso, tipoprocesso) AS SELECT RES.RESULTADO,        ACA.ACAO,        RES.DESCRICAO,        RES.DESCREDUZIDA,        RES.ORDEM,        ISNULL(ACA.DESCRICAO, ' (rec)' ) ++ ' - ' ++ RES.DESCRICAO AS DESCRICAOFULL,        ISNULL(ACA.DESCREDUZIDA, ' (rec)' ) ++ ' - ' ++ RES.DESCREDUZIDA AS DESCREDFULL,        ISNULL(ACA.DESCRICAO, ' (rec)' ) AS ACAODESCRICAO,        ISNULL(ACA.DESCREDUZIDA, ' (rec)' ) AS DESCREDUZIDAACAO,        RES.EMUSO,        (CASE RES.CTRLCOMPLEMENTO WHEN 1 THEN 'S' WHEN 9 THEN 'S' ELSE 'N' END ) as TEMCOMPLEMENTO,        RES.CTRLCOMPLEMENTO,        PRES.CODPROCESSO,        CPRS.DESCRRED AS TIPOPROCESSO FROM IV_RESULTADO RES      LEFT JOIN IV_ACAO ACA ON ACA.ACAO = RES.ACAO      LEFT JOIN IV_PROCRESULTADO PRES       JOIN IV_CODPROCESSO CPRS ON CPRS.CODPROCESSO = PRES.CODPROCESSO           ON PRES.RESULTADO = RES.RESULTADO   