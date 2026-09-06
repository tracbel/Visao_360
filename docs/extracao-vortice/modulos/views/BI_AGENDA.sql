/* ==============================================================
   Objeto ..........: dbo.BI_AGENDA
   Tipo ............: VIEW
   Criado em .......: 2015-08-25 12:45:06
   Modificado em ...: 2023-03-15 09:39:38
   Linhas ..........: 50
   Escreve em tabela: nao
   Tabelas referidas: GE_USUARIO, IV_ACAO, IV_AGENDA, IV_CodProcesso
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[BI_AGENDA] AS   
SELECT AGD.SEQPESSOA,
       AGD.SEQAGENDA AS AGD_SEQAGENDA,
       AGD.ASSUNTO AS AGD_ASSUNTO,
       AGD.DTAAGENDA AS AGD_DATA_AGENDA,
       AGD.SEQUSUARIO AS SEQUSUARIO,
       AGD.DETALHE AS AGD_DETALHE,
       CAST(YEAR(AGD.DtaAgenda) AS VARCHAR(4)) + '-' +
       RIGHT('0' + CAST(MONTH(AGD.DtaAgenda) AS VARCHAR(2)), 2) + '-' +
       CAST(DAY(AGD.DtaAgenda) AS VARCHAR(2)) AS AGD_DATA_AGENDA_X,
       CONVERT(VARCHAR(12),
               CASE
                 --WHEN AGD.DTAAGENDA < GETDATE() THEN
                 -- DATEADD(DD, -DAY(GETDATE() - 6) + 1, GETDATE() - 6)
                 --WHEN AGD.DTAAGENDA > (GETDATE() + 60) THEN
                 -- DATEADD(DD, -DAY(GETDATE() - 90) + 1, GETDATE() - 90)
                 --ELSE
                 -- DATEADD(DD, -DAY(GETDATE()) + 1, GETDATE())
				 WHEN AGD.DtaAgenda > GETDATE()
				      THEN DATEFROMPARTS ( year(GETDATE()), MONTH(DATEADD (MM , +1 , GETDATE() )) , 1 ) 
				 WHEN DATEDIFF(DD, AGD.DtaAgenda, GETDATE()) > 60
					  THEN DATEFROMPARTS ( year(GETDATE()), MONTH(DATEADD (MM , -1 , GETDATE() )) , 1 ) 
				 ELSE DATEFROMPARTS ( year(GETDATE()), MONTH( GETDATE() ) , 1 ) 
               END,
               103) AS AGD_SEMANA,
       CASE
         WHEN AGD.CLASSE = 'P' THEN
          'Visita'
         WHEN AGD.CLASSE = 'T' THEN
          'Telefone'
         else
          'Outro'
       end AS AGD_CLASSE,
	   AGD.PROCESSO AS AGD_PROCESSO,
	   (SELECT CODUSUARIO FROM GE_USUARIO WHERE SEQUSUARIO = AGD.SEQUSUARIO) AS AGD_CODUSUARIO,
	   isnull( AGD.CodProcesso, 0) AS AGD_CODPROCESSO,
	   isnull( (SELECT DESCRICAO FROM IV_CodProcesso WHERE AGD.CodProcesso = CodProcesso) , '<em branco>' )
	   AS AGD_DESCPROCESSO
  FROM IV_AGENDA AGD
  ----JOIN IVS_PES PESC
  ----  ON PESC.SEQPESSOA = AGD.SEQPESSOA
  ---- AND PESC.SEQDEPTO = 2
  ----JOIN IVS_CARTEIRA CART
  ----  ON CART.SEQCARTEIRA = PESC.SEQCARTEIRA
  ---- AND CART.SEQUSRRESP = AGD.SEQUSUARIO
 WHERE AGD.REALIZADA = 'N'
   AND AGD.ACAO > 0
   AND EXISTS (SELECT 1 FROM IV_ACAO WHERE ACAO = AGD.ACAO
                                        AND EMUSO = 'S');
