/* ==============================================================
   Objeto ..........: dbo.BI_FROTA
   Tipo ............: VIEW
   Criado em .......: 2015-08-03 12:40:53
   Modificado em ...: 2023-10-05 15:31:46
   Linhas ..........: 28
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_CLIENTEPROPR, IV_GLOBALPAR, IV_PROPRIEDADE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[BI_FROTA] AS 
SELECT FRT.SEQPESSOA,
       PRT.PROPRIEDADE AS FROTA_TIPO,
       FRT.REFERENCIA AS FROTA_MARCA,
       FRT.CAMPO2 AS FROTA_MODELO,
       GPAR.CAMPO2 AS FROTA_POTENCIA,
       FRT.CAMPO6 AS FROTA_ANO,
       ROUND(ISNULL(FRT.NUMERO1, 1), 0) AS FROTA_QTDE,
       FRT.DTAALTERACAO AS FRT_DATA_ALTERACAO,
       CASE
         WHEN (GETDATE() - FRT.DTAALTERACAO) > 380 THEN
          'Não'
         else
          'Sim'
       end as FRT_DADO_ATUAL,
	   (SELECT SEQPESSOAPRC FROM GE_PESSOA WHERE SEQPESSOA = FRT.SEQPESSOA) AS SEQPRINC,
	   PRT.Propriedade,
	   FRT.IDENTIFICADOR AS FROTA_CHASSI
  FROM IV_CLIENTEPROPR FRT
  JOIN IV_PROPRIEDADE PRT
    ON PRT.SEQPROPRIEDADE = FRT.SEQPROPRIEDADE
  LEFT JOIN IV_GLOBALPAR GPAR
    ON GPAR.CAMPO1 = FRT.REFERENCIA
   AND GPAR.LITERAL1 = FRT.CAMPO2
   AND GPAR.SimNao1 = 1
 WHERE FRT.SEQPROPRIEDADE IN
       (9402, 9403, 9404, 9405, 9407, 9408, 9409, 9410, 9411, 16)
   AND FRT.ATIVO = 'S'