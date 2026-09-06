/* ==============================================================
   Objeto ..........: dbo.BI_ORIGEM_RECEITA
   Tipo ............: VIEW
   Criado em .......: 2018-03-13 11:19:19
   Modificado em ...: 2025-07-31 11:32:19
   Linhas ..........: 27
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[BI_ORIGEM_RECEITA] AS 
SELECT CLT.SEQPESSOA,
       CLT.REFERENCIA AS ORIGEM_TIPO,
       CLT.CAMPO5 AS ORIGE_TIPO_AREA,
       CLT.CAMPO1 AS ORIGEM_SUB_TIPO,
       CLT.CAMPO3 AS ORIGEM_PESO,
	   --- mudancça efetuada dez/23 devido áreas mais antigas não estarem identificadas a pedido da vanessa
    --   CASE WHEN ISNULL(CLT.CAMPO7, 'NÃO')='NÃO' THEN 0 ELSE ROUND(CLT.NUMERO4, 0) END AS ORIGEM_HA,
	   --CASE WHEN CLT.CAMPO7='SIM' AND ISNULL(CLT.CAMPO6, 'NÃO')='NÃO' THEN ROUND(CLT.NUMERO4, 0) ELSE 0 END AS ORIGEM_HA_PRINCIPAL,

       CASE WHEN ISNULL(CLT.CAMPO7, 'SIM')='NÃO' THEN 0 ELSE ROUND(CLT.NUMERO4, 0) END AS ORIGEM_HA,
	   CASE WHEN ISNULL(CLT.CAMPO7, 'SIM')='SIM' AND ISNULL(CLT.CAMPO6, 'NÃO')='NÃO' THEN ROUND(CLT.NUMERO4, 0) ELSE 0 END AS ORIGEM_HA_PRINCIPAL,
       CLT.DTAALTERACAO AS ORIGEM_DATA_ALTERACAO,
       CASE
         WHEN (GETDATE() - CLT.DTAALTERACAO) > 380 THEN
          'Não'
         else
          'Sim'
       end as ORIGEM_DADO_ATUAL,
	   CLT.CAMPO6 AS ORIGEM_AREA_COMPART,
	   ROUND(CLT.NUMERO4, 0)  AS ORIGEM_AREAHA,
	   CLT.LITERAL2 AS ORIGEM_TIPOPROPRIEDADE,
	   CLT.CAMPO7 AS ORIGEM_AREAPRINC,
	   (SELECT SEQPESSOAPRC FROM GE_PESSOA WHERE SEQPESSOA = CLT.SEQPESSOA) AS SEQPRINC
  FROM IV_CLIENTEPROPR CLT
 WHERE CLT.SEQPROPRIEDADE in (9400, 9500)
