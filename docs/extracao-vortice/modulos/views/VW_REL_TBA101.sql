/* ==============================================================
   Objeto ..........: dbo.VW_REL_TBA101
   Tipo ............: VIEW
   Criado em .......: 2025-11-19 16:01:33
   Modificado em ...: 2026-04-06 15:12:55
   Linhas ..........: 157
   Escreve em tabela: nao
   Tabelas referidas: GE_EMPRESA, GE_PESSOA, GE_USUARIO, IMP_REL_TBA101, IV_ACAO, IV_HISTORICO, IV_PROCDADO, IV_PROCESSO, IV_RESULTADO
   Outras refs .....: IV_Q$ACOMP_VENDA_FINANC, IV_Q$VENDA_EQUIPAMENTO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

-- dbo.VW_REL_TBA101 fonte

CREATE VIEW VW_REL_TBA101 AS

/* =========================================================
   BLOCO 1 – QUERY PRINCIPAL
   ========================================================= */

SELECT DISTINCT
       XA.EMPRESA,
       XA.CEN,
       XA.JD_QUOTE,
       XA.TIPO_VENDA,
       XA.TIPO_DE_EQUIPAMENTO,
       XA.MARCA,
       XA.MODELO,
       XA.SEQPESSOA,
       XA.NOMERAZAO,
       XA.CNPJCPF,
       XA.LINHA_CREDITO,
       XA.INST_FINANCEIRA,
       XA.FINANC_CHASSI,
       XA.ULT_RESULTADO,
       XA.DTA_FATURAMENTO_D,
       XA.NF_N,
       XA.DTA_PED_D,
       XA.DTA_PRENCH_FORM_D,
       XA.VALOR_N,
       XA.DTA_ULT_ANDAMENTO_D,
       XA.ULT_HISTORICO_L,
       XA.PROCESSO_N,
       XA.CARTEIRA,
       XA.CIDADE,
       XA.DATA_PEDIDO_HIST_D,
       XA.CAMPANHA,
       XA.PROCESSODNA,
       XA.DTAPROVADO,
       XA.DATA_MARCADO_ENTREGUE,
       XA.STATUS
FROM (

    /* =====================================================
       SUBQUERY XA – BASE PRINCIPAL
       ===================================================== */

    SELECT DISTINCT
           EMP.NOMEREDUZIDO                             AS EMPRESA,
           HIST.VENDEDOR                                AS CEN,
           VND.VENDA_JDQUOTE                           AS JD_QUOTE,
           VND.VENDA_TIPO_VENDA                        AS TIPO_VENDA,
           VND.VENDA_TIPO_EQUIPAMEN                    AS TIPO_DE_EQUIPAMENTO,
           VND.VENDA_MARCA_EQUIPAME                    AS MARCA,
           VND.VENDA_MODELO_EQUIPAM                    AS MODELO,
           PES.SEQPESSOA                                AS SEQPESSOA,
           CASE
             WHEN ISNULL(VND.VENDA_COMPRADOR,'') = ''
             THEN PES.NOMERAZAO
             ELSE VND.VENDA_COMPRADOR
           END                                          AS NOMERAZAO,
           ISNULL(CAST(PES.NROCGCCPF AS VARCHAR),'') +
           ISNULL(CAST(PES.DIGCGCCPF AS VARCHAR),'')   AS CNPJCPF,
           VND.VENDA_LINHA_CREDITO                     AS LINHA_CREDITO,
           VND.VENDA_INST_FINANC                       AS INST_FINANCEIRA,
           ADM.VENDA_FINANC_CHASSI                     AS FINANC_CHASSI,
           ACA.Descricao + ' - ' + RES.Descricao       AS ULT_RESULTADO,
           ADM.VENDA_FINANC_DATAFAT                    AS DTA_FATURAMENTO_D,
           ADM.VENDA_FINANC_NRO_NF                     AS NF_N,
           VND.VENDA_DATA_PEDIDO                       AS DTA_PED_D,
           VND.DTAREALIZACAO                           AS DTA_PRENCH_FORM_D,
           VND.VENDA_VALOR_TOTAL                       AS VALOR_N,
           PDD.DTAULTRESULTADO                         AS DTA_ULT_ANDAMENTO_D,
           HIST.DETALHE                                AS ULT_HISTORICO_L,
           PRC.PROCESSO                                AS PROCESSO_N,
           RESP.CODUSUARIO                             AS CARTEIRA,
           PES.CIDADE + ' - ' + PES.UF                 AS CIDADE,
           (
             SELECT MAX(H2.DTAREALIZACAO)
             FROM IV_HISTORICO H2
             WHERE H2.PROCESSO  = PRC.PROCESSO
               AND H2.RESULTADO = 3231
           )                                           AS DATA_PEDIDO_HIST_D,
           PDD.CAMPANHA                                AS CAMPANHA,
           PDD.PROCESSODNA                             AS PROCESSODNA,
           (
             SELECT FORMAT(MAX(H3.DTAREALIZACAO),'dd/MM/yyyy')
             FROM IV_PROCDADO PD3
             JOIN IV_HISTORICO H3 ON H3.PROCESSO = PD3.PROCESSO
             WHERE PD3.PROCESSODNA = PDD.PROCESSODNA
               AND H3.RESULTADO = 3239
           )                                           AS DTAPROVADO,
           CONVERT(VARCHAR(10), ADM.VENDA_FINANC_DATAENT,103)
                                                       AS DATA_MARCADO_ENTREGUE,
           PRC.STATUS                                  AS STATUS
    FROM IV_PROCESSO PRC
    JOIN IV_PROCDADO  PDD  ON PDD.PROCESSO     = PRC.PROCESSO
    JOIN GE_EMPRESA   EMP  ON EMP.NROEMPRESA   = PDD.NROEMPRESA
    JOIN GE_PESSOA    PES  ON PES.SEQPESSOA    = PDD.SEQPESSOA
    JOIN IV_Q$VENDA_EQUIPAMENTO VND ON VND.PROCESSO = PRC.PROCESSO
    LEFT JOIN IV_Q$ACOMP_VENDA_FINANC ADM ON ADM.PROCESSO = PRC.PROCESSO
    OUTER APPLY (
        SELECT TOP 1 H.VENDEDOR, H.DETALHE, H.RESULTADO, H.ACAOGERADORA
        FROM IV_HISTORICO H
        WHERE H.PROCESSO = PRC.PROCESSO
        ORDER BY H.DTAREALIZACAO DESC
    ) HIST
    LEFT JOIN IV_RESULTADO RES ON RES.RESULTADO = HIST.RESULTADO
    LEFT JOIN IV_ACAO ACA ON ACA.ACAO = RES.ACAO
    LEFT JOIN GE_USUARIO RESP ON RESP.CODUSUARIO = HIST.VENDEDOR
    WHERE VND.VENDA_DATA_PEDIDO >= CONVERT(DATE,'01/01/2026',103)
      AND VND.VENDA_DATA_PEDIDO <= CONVERT(DATE,'31/03/2026',103)
      AND PRC.STATUS NOT IN
          ('VENDA PERDIDA','DESISTIU DA COMPRA','CANCELADO',
           'PEDIDO NÃO APROVADO','CREDITO NAO APROVADO','DEVOLVIDO', 'FATURADO')
      AND PES.STATUS <> 'F'

) XA

/* =========================================================
   BLOCO 2 – UNION (DADOS JÁ GERADOS)
   ========================================================= */

UNION ALL

SELECT
      X1.EMPRESA,
      X1.CEN,
      X1.JD_QUOTE,
      X1.TIPO_VENDA,
      X1.TIPO_DE_EQUIPAMENTO,
      X1.MARCA,
      X1.MODELO,
      X1.SEQPESSOA,
      X1.NOMERAZAO,
      X1.CNPJCPF,
      X1.LINHA_CREDITO,
      X1.INST_FINANCEIRA,
      X1.FINANC_CHASSI,
      X1.ULT_RESULTADO,
      X1.DTA_FATURAMENTO_D,
      X1.NF_N,
      X1.DTA_PED_D,
      X1.DTA_PRENCH_FORM_D,
      X1.VALOR_N,
      X1.DTA_ULT_ANDAMENTO_D,
      X1.ULT_HISTORICO_L,
      X1.PROCESSO_N,
      X1.CARTEIRA,
      X1.CIDADE,
      X1.DATA_PEDIDO_HIST_D,
      X1.CAMPANHA,
      X1.PROCESSODNA,
      X1.DTAPROVADO,
      CONVERT(VARCHAR(10), X2.VENDA_FINANC_DATAENT,103),
      NULL AS STATUS
FROM IMP_REL_TBA101 X1
LEFT JOIN IV_Q$ACOMP_VENDA_FINANC X2
       ON X2.PROCESSO = X1.PROCESSO_N;