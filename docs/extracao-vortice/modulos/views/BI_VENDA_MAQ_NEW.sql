/* ==============================================================
   Objeto ..........: dbo.BI_VENDA_MAQ_NEW
   Tipo ............: VIEW
   Criado em .......: 2022-10-07 11:40:22
   Modificado em ...: 2022-10-07 12:06:40
   Linhas ..........: 36
   Escreve em tabela: nao
   Tabelas referidas: IV_PROCDADO, IV_PROCESSO
   Outras refs .....: IV_Q$ACOMPANH_VENDA_JDE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW BI_VENDA_MAQ_NEW  AS
SELECT ACV.processo
    ,ACV.TP_PGTO1 AS VDA_TIPO_FINANCIAMENTO
    ,ACV.INSTITUI_FINANCEIRA1 AS VDA_INST_FINANCEIRA
    ,ACV.VLR_TOTAL1 AS VDA_VALOR
    ,ACV.MODEL_EQUIP1 AS VDA_MODELO
    ,ACV.MARCA_1 AS VDA_MARCA
    ,ACV.TIPO_EQUIP1 AS VDA_TIPO_MAQUINA
    ,case when ACV.PROB_APROV_CRED1 = 'Alta' then '1-Alta'
          when ACV.PROB_APROV_CRED1 = 'Média' then '2-Média'
            else '3-Baixa'
     end AS VDA_PROB_APROVACAO
    ,CASE WHEN ISNULL(ACV.PAC_EMITIDO, GETDATE()) = GETDATE() THEN 'NÃO' ELSE 'SIM' END AS VDA_PAC_EMITIDO
    ,ACV.pac_emitido AS VDA_DATA_PAC
    ,CASE WHEN ISNULL(ACV.FATURAMENTO_REALIZ, GETDATE()) = GETDATE() THEN 'NÃO' ELSE 'SIM' END AS VDA_FATURAMENTO_REALIZADO
    ,ACV.faturamento_realiz AS VDA_DATA_FATURAMENTO
    ,ACV.PREV_FATURAM1 AS VDA_DATA_PREVISTA_FATURAMENTO
    ,RIGHT( CAST ( YEAR( ACV.PREV_FATURAM1 ) AS VARCHAR(4) ), 2)  + '/' + 
     RIGHT( '0' + CAST ( MONTH( ACV.PREV_FATURAM1 ) AS VARCHAR(2)), 2) AS VDA_MES_PREVISTO_FATURAMENTO
    ,ACV.origem_faturamento AS VDA_ORIGEM_FATURAMENTO
    ,DATEADD(DD, DATEDIFF(DD, 0, ACV.pac_emitido - ACV.data_pedido_de_venda ), 0) AS VDA_DIAS_PEDIDO_PAC
	,DATEDIFF(day, ACV.faturamento_realiz, ACV.data_pedido_de_venda ) AS VDA_DIAS_PEDIDO_FATURAMENTO
	,DATEADD(DD, DATEDIFF(DD, 0, ACV.faturamento_realiz - ACV.pac_emitido ), 0) AS VDA_DIAS_PAC_FATURAMENTO
	,ACV.data_pedido_de_venda
FROM IV_Q$ACOMPANH_VENDA_JDE ACV
WHERE PROCESSO IN (
SELECT   PDD.PROCESSO
  FROM IV_PROCDADO PDD
  JOIN IV_PROCESSO PRC
    ON PRC.PROCESSO = PDD.PROCESSO
 WHERE PDD.CODPROCESSO IN(  7)
   AND PRC.FASEORDEM < 35
   AND PRC.REALIZADO = 0
)

