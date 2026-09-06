/* ==============================================================
   Objeto ..........: dbo.BI_PESSOAOP
   Tipo ............: VIEW
   Criado em .......: 2016-10-04 20:08:09
   Modificado em ...: 2023-12-21 08:45:46
   Linhas ..........: 136
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_CLIENTEPROPR, IVS_CARTEIRA, IVS_DEPTO, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[BI_PESSOAOP] AS 

SELECT PES.SEQPESSOA,
       PES.NOMERAZAO AS CLIENTE,
       PES.CIDADE,
       PES.UF AS ESTADO,
       PES.ATIVIDADE,
       PES.FISICAJURIDICA AS FJ,
       PES.FoneDDD1 + ' - ' + convert(char, PES.FoneNro1) AS FONE1,
       PES.FoneDDD2 + ' - ' + convert(char, PES.FoneNro2) AS FONE2,
       PES.FoneDDD3 + ' - ' + convert(char, PES.FoneNro3) AS FONE3,
       PES.Email AS EMAIL,
	   PES.NROCGCCPF,
	   PES.DIGCGCCPF,
       PES.LATITUDE,
       PES.Telefonema		AS RECEBE_TELEFONEMA,
	   PES.Correspondencia  AS RECEBE_CORRESPONDENCIA,
	   PES.RecebeEmail		AS  RECEBE_EMAIL,
	   PES.RecebeSMS		AS  RECEBE_SMS,
	   JPES.SEQPESSOA					 							AS SEQ_CONGLOMERADO,
	   JPES.NomeRazao					 							AS NOME_CONGLOMERADO,
	   CAST(JPES.SEQPESSOA AS VARCHAR) + ' - ' + JPES.NomeRazao  	AS CONGLOMERADO,
       ISNULL(( SELECT COUNT(1) AS QT
         FROM IV_CLIENTEPROPR FRT
         WHERE FRT.SEQPESSOA = PES.SEQPESSOA
		 AND FRT.SEQPROPRIEDADE IN
          (9402, 9403, 9404, 9405, 9407, 9408, 9409, 9410, 9411, 9413)
		   AND FRT.ATIVO = 'S' 
        ), 0) AS POSSUI_FROTA,   

	   PES.Grupo				as GRUPO,
	   PES.DtaNascFund			as DTANASCFUND,

       PES.LONGITUDE,
       REPLACE(PES.LATITUDE, ',', '.') + ',' +
       REPLACE(PES.LONGITUDE, ',', '.') AS LOCATION,
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
       END AS PES_VINCULO,
	   SEQPESSOAPRC,
	   CASE WHEN (SELECT COUNT(1) AS QT
				FROM IVS_PES IVP, IVS_DEPTO DPT, IVS_CARTEIRA CRT
				   WHERE IVP.SEQPESSOA = PES.SEQPESSOA
				     AND IVP.SeqDepto=2
				     AND DPT.SEQDEPTO = IVP.SEQDEPTO
					 AND CRT.SeqCarteira = IVP.SeqCarteira)>0 
			 THEN   'carteira_ok'
			 ELSE   'carteira_nao_ok'
			 END AS VALIDACAO_CARTEIRA
  FROM GE_PESSOA PES
  LEFT JOIN (SELECT SEQPESSOA, NomeRazao from GE_Pessoa ) JPES
            ON JPES.SEQPESSOA = PES.SEQPESSOAPRC
 where 1 = 1
 --AND EXISTS (SELECT 1
 --         FROM IV_PROCDADO PDD
 --         JOIN IV_PROCESSO PRC
 --           ON PRC.PROCESSO = PDD.PROCESSO
 --        WHERE PDD.CODPROCESSO IN (7,8)
 --          AND PRC.FASEORDEM <= 60
 --          AND PRC.REALIZADO = 0
 --          AND PDD.SeqPessoa = PES.SeqPessoa)



