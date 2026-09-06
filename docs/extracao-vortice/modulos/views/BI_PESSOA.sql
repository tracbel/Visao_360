/* ==============================================================
   Objeto ..........: dbo.BI_PESSOA
   Tipo ............: VIEW
   Criado em .......: 2015-09-15 12:31:34
   Modificado em ...: 2018-12-05 10:26:05
   Linhas ..........: 132
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */







CREATE VIEW [dbo].[BI_PESSOA] AS
SELECT PES.SEQPESSOA,
       PES.NOMERAZAO AS CLIENTE,
       PES.CIDADE,
       PES.UF AS ESTADO,
       PES.NROCGCCPF,
       PES.DIGCGCCPF,
       PES.ATIVIDADE,
       PES.FISICAJURIDICA AS FJ,
       PES.FoneDDD1 + ' - ' + convert(char, PES.FoneNro1) AS FONE1,
	   PES.FoneDDD2 + ' - ' + convert(char, PES.FoneNro2) AS FONE2,
	   PES.FoneDDD3 + ' - ' + convert(char, PES.FoneNro3) AS FONE3,
       PES.Email AS EMAIL,
       PES.LATITUDE,
       PES.LONGITUDE,
       REPLACE(PES.LATITUDE, ',', '.') + ',' +
       REPLACE(PES.LONGITUDE, ',', '.') AS LOCATION,
       ISNULL(
       ( SELECT COUNT(*)
         FROM IV_CLIENTEPROPR FRT
         WHERE FRT.SEQPESSOA = PES.SEQPESSOA
		 AND FRT.SEQPROPRIEDADE IN
          (9402, 9403, 9404, 9405, 9407, 9408, 9409, 9410, 9411, 9413)
		   AND FRT.ATIVO = 'S' 
        ), 0) AS POSSUI_FROTA,      
       CASE
         WHEN (SELECT COUNT(*)
                 FROM IV_CLIENTEPROPR CPR
                WHERE CPR.SEQPROPRIEDADE = 9400
                  AND CPR.SEQPESSOA = PES.SEQPESSOA) > 0 THEN
          'Sim'
         else
          'Não'
       end as PES_ORIGEM_OK,
       CASE
         WHEN (SELECT COUNT(*)
                 FROM IV_CLIENTEPROPR CPR
                WHERE CPR.SEQPROPRIEDADE = 9402
                  AND CPR.SEQPESSOA = PES.SEQPESSOA) > 0 THEN
          'Sim'
         else
          'Não'
       end as PES_TRATOR_OK,
       ISNULL((SELECT SUM(NUMERO1)
                FROM IV_CLIENTEPROPR CPR
               WHERE CPR.SEQPROPRIEDADE = 9402
                 AND CPR.SEQPESSOA = PES.SEQPESSOA),
              0) as PES_TRATOR_QTDE,
       CASE
         WHEN (SELECT COUNT(*)
                 FROM IV_CLIENTEPROPR CPR
                WHERE CPR.SEQPROPRIEDADE = 9403
                  AND CPR.SEQPESSOA = PES.SEQPESSOA) > 0 THEN
          'Sim'
         else
          'Não'
       end as PES_COLHE_GRAO_OK,
       ISNULL((SELECT SUM(NUMERO1)
                FROM IV_CLIENTEPROPR CPR
               WHERE CPR.SEQPROPRIEDADE = 9403
                 AND CPR.SEQPESSOA = PES.SEQPESSOA),
              0) as PES_COLHE_GRAO_QTDE,
       CASE
         WHEN (SELECT COUNT(*)
                 FROM IV_CLIENTEPROPR CPR
                WHERE CPR.SEQPROPRIEDADE = 9411
                  AND CPR.SEQPESSOA = PES.SEQPESSOA) > 0 THEN
          'Sim'
         else
          'Não'
       end as PES_COLHE_CANA_OK,
       ISNULL((SELECT SUM(NUMERO1)
                FROM IV_CLIENTEPROPR CPR
               WHERE CPR.SEQPROPRIEDADE = 9411
                 AND CPR.SEQPESSOA = PES.SEQPESSOA),
              0) as PES_COLHE_CANA_QTDE,
       CASE
         WHEN (SELECT COUNT(*)
                 FROM IV_CLIENTEPROPR CPR
                WHERE CPR.SEQPROPRIEDADE = 9405
                  AND CPR.SEQPESSOA = PES.SEQPESSOA) > 0 THEN
          'Sim'
         else
          'Não'
       end as PES_PLANTADERIA_OK,
       ISNULL((SELECT SUM(NUMERO1)
                FROM IV_CLIENTEPROPR CPR
               WHERE CPR.SEQPROPRIEDADE = 9405
                 AND CPR.SEQPESSOA = PES.SEQPESSOA),
              0) as PES_PLANTADERIA_QTDE,
       PES.DTAINCLUSAO AS PES_DATA_INCLUSAO,
       PES.USUINCLUSAO AS PES_CADASTRADO_POR,
       PES.STATUS PES_STATUS,
       CASE
         WHEN PES.SEQPESSOAPRC = PES.SEQPESSOA THEN
          'Principal'
         ELSE
          'Relacionado'
       END AS PES_VENCULO,

		B.Codigo_Grupo					 							AS SEQ_CONGLOMERADO,
		B.Grupo_Cliente					 							AS NOME_CONGLOMERADO,
		CAST(B.Codigo_Grupo AS VARCHAR) + ' - ' + B.Grupo_Cliente  	AS CONGLOMERADO,

	   PES.Grupo				as GRUPO,
	   PES.DtaNascFund			as DTANASCFUND,

	   case when PES.Telefonema			= 1 then 'sim' else 'não' end	as RECEBE_TELEFONEMA,
	   case when PES.Correspondencia	= 1 then 'sim' else 'não' end	as RECEBE_CORRESPONDENCIA,
	   case when PES.RecebeEmail		= 1 then 'sim' else 'não' end	as RECEBE_EMAIL,
	   case when PES.RecebeSMS			= 1 then 'sim' else 'não' end	as RECEBE_SMS

  FROM GE_PESSOA PES

    INNER JOIN (SELECT J.Grupo as Codigo_Grupo,LTRIM(A.NomeRazao) AS Grupo_Cliente,J.SeqPessoa as SeqPessoa,J.NomeRazao as NomeRazao
	 from GE_Pessoa A INNER JOIN 
	 (SELECT SEQPESSOAPRC as Grupo,SeqPessoa,NomeRazao from GE_Pessoa ) J
     ON A.SeqPessoa = J.Grupo) B ON PES.SeqPessoa = B.SEQPESSOA
	 
where 1 = 1





