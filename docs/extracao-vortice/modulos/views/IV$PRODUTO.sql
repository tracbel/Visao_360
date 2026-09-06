/* ==============================================================
   Objeto ..........: dbo.IV$PRODUTO
   Tipo ............: VIEW
   Criado em .......: 2016-04-13 09:38:13
   Modificado em ...: 2020-07-15 08:18:26
   Linhas ..........: 29
   Escreve em tabela: nao
   Tabelas referidas: IV_GlobalPar, IV_GlobalParCtrl
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE VIEW [dbo].[IV$PRODUTO]
AS
SELECT        0 AS NROEMPRESA,
	  GP.SeqPar AS SEQPRODUTO,
	  SUBSTRING(CTR.Parametro, 0, 5) + '-' + SUBSTRING(GP.Campo1, 0, 6) + '-' + ISNULL(GP.Literal1, ' ') + '-' + ISNULL(GP.Literal2, ' ') AS CODPRODUTO, 
      CTR.Parametro + ' ' + GP.Campo1 + ' ' + GP.Literal1 + ' ' + ISNULL(GP.Literal2, ' ') AS DESCRICAO,
	  CTR.Parametro AS FAMILIA,
	  GP.Campo1 AS MARCA,
	  GP.Literal1 AS MODELO,
	  0.00 AS PRECO1,
	  'S' AS EMUSO,
	  '' AS CATEGORIA, 
      '' AS NEGOCIO,
	  '' AS CODFAMILIA,
	  '' AS NOVO,
	  '' AS USADO,
	  '' AS CHASSI,
	  '' AS ESTRUTURA,
	  '' AS OBS



FROM            dbo.IV_GlobalPar AS GP
INNER JOIN      dbo.IV_GlobalParCtrl AS CTR ON CTR.SeqGlbPar = GP.SeqGlbPar
WHERE        (GP.SeqGlbPar IN (9401, 9405, 9407, 9408, 9409, 9411, 9406, 9404, 9410, 9412, 3, 7))
AND (GP.Campo1 IS NOT NULL) AND (GP.SimNao1 = 1)

