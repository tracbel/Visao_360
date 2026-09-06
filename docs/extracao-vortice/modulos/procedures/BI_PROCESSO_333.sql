/* ==============================================================
   Objeto ..........: dbo.BI_PROCESSO_333
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2018-09-18 09:56:47
   Modificado em ...: 2019-01-18 09:48:11
   Linhas ..........: 209
   Escreve em tabela: SIM (UPDATE, DELETE)
   Tabelas referidas: GE_Empresa, GE_Pessoa, IV_HISTORICO, IV_PROCDADO, IV_Resultado
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE [dbo].[BI_PROCESSO_333]
AS

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '##ACUMULA')
DROP TABLE ##ACUMULA;

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '##IV_HISTORICO')
DROP TABLE ##IV_HISTORICO;

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '##ACAO63')
DROP TABLE ##ACAO63;

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '##ACAO149')
DROP TABLE ##ACAO149;

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '##ACAO150')
DROP TABLE ##ACAO150;

--IF  EXISTS (SELECT *FROM sysobjects WHERE name = 'BI_PROCESSO_333' AND xtype = 'P')

-----------------------------------------------------------------------------------------------------------------------

--CRIANDO TABELA TEMPORARIA PARA AJUSTES

SELECT * INTO ##IV_HISTORICO
FROM IV_HISTORICO;

UPDATE ##IV_HISTORICO
SET Processo = 404591
where Processo = 482633;

UPDATE ##IV_HISTORICO
SET Processo = 371150
where Processo = 486174;

UPDATE ##IV_HISTORICO
SET Processo = 515981
where Processo = 499430
and AcaoGeradora = 150;

--DELETE ##IV_HISTORICO
--WHERE PROCESSO = 398093 AND
--AcaoGeradora = 47 AND
--UsuAlteracao = 'VANESSA.BROCANELLI';

--DELETE ##IV_HISTORICO
--WHERE PROCESSO = 371150 AND
--AcaoGeradora = 47 
--AND AgendaOrigem = '409292';

--DELETE ##IV_HISTORICO
--WHERE PROCESSO = 457692 AND
--AcaoGeradora = 47 
--AND SeqHistorico = '624297';

--DELETE ##IV_HISTORICO
--WHERE PROCESSO = 409222 AND
--AcaoGeradora = 47 
--AND SeqHistorico = '631964';

--DELETE ##IV_HISTORICO
--WHERE PROCESSO = 371151 AND
--AcaoGeradora = 47 
--AND SeqHistorico in ('651463','651465');

--DELETE ##IV_HISTORICO
--WHERE PROCESSO = 530007 AND
--AcaoGeradora = 47 
--AND SeqHistorico in ('740164');

SELECT B.Descricao as Desc_Resultado, A.* INTO ##ACAO63
FROM ##IV_HISTORICO A INNER JOIN IV_Resultado B ON A.Resultado = B.Resultado
WHERE AcaoGeradora = 63;

SELECT B.Descricao as Desc_Resultado, A.* INTO ##ACAO149
FROM ##IV_HISTORICO A INNER JOIN IV_Resultado B ON A.Resultado = B.Resultado
WHERE AcaoGeradora = 149;

SELECT B.Descricao as Desc_Resultado, A.* INTO ##ACAO150
FROM ##IV_HISTORICO A INNER JOIN IV_Resultado B ON A.Resultado = B.Resultado
WHERE AcaoGeradora = 150;

-----------------------------------------------------------------------------------------------------------------------

SELECT	PRC.NroEmpresa								AS Cod_Filial,
		substring(BB.Fantasia,10,20)				AS Filial,
		PRC.SeqPessoa,
		AA.NomeRazao,
		TT.Codigo_Grupo								AS SeqPessoa_Conglomerado,
		TT.Grupo_Cliente							AS NomeRazao_Conglomerado,
		C.ProcessoDNA, 
		PRC.Processo,

			( SELECT MAX(A.Dtarealizacao) from ##IV_HISTORICO A
				INNER JOIN IV_PROCDADO B ON
					A.Processo = B.Processo
				WHERE B.ProcessoDNA = c.ProcessoDNA
					AND A.Resultado in (243,666)
				group by B.ProcessoDNA )			AS Dta_Entrega_Tecnica,

				(SELECT MAX (DTAREALIZACAO)
                    FROM ##IV_HISTORICO
                   WHERE Processo = PRC.Processo
                   AND AcaoGeradora = 63 )			AS Dta_Primeiro_Contato,
				    (SELECT MAX (DTAREALIZACAO)
                    FROM ##IV_HISTORICO
                   WHERE Processo = PRC.Processo
                   AND AcaoGeradora = 149 )			AS Dta_Segundo_Contato,
				    (SELECT MAX (DTAREALIZACAO)
                    FROM ##IV_HISTORICO
                   WHERE Processo = PRC.Processo
                   AND AcaoGeradora = 150 )			AS Dta_Terceiro_Contato
INTO ##ACUMULA
FROM ##IV_HISTORICO PRC 
		INNER JOIN IV_PROCDADO C ON 
			PRC.Processo = C.Processo
		INNER JOIN GE_Pessoa AA ON
			AA.SeqPessoa = PRC.SeqPessoa
		INNER JOIN GE_Empresa BB ON
			BB.NroEmpresa = PRC.NroEmpresa

		LEFT JOIN (  SELECT J.Grupo as Codigo_Grupo,LTRIM(A.NomeRazao) AS Grupo_Cliente,J.SeqPessoa as SeqPessoa,J.NomeRazao as NomeRazao
					 from GE_Pessoa A INNER JOIN 
							(SELECT SEQPESSOAPRC as Grupo,SeqPessoa,NomeRazao from GE_Pessoa ) J
							   ON A.SeqPessoa = J.Grupo ) TT ON
					              AA.SeqPessoa = TT.SeqPessoa

WHERE PRC.AcaoGeradora in (63,149,150)
AND
( SELECT MAX(A.Dtarealizacao) from ##IV_HISTORICO A
INNER JOIN IV_PROCDADO B ON
A.Processo = B.Processo
WHERE B.ProcessoDNA = c.ProcessoDNA
AND A.Resultado in (243)
group by B.ProcessoDNA ) IS NOT NULL


	GROUP BY 
		PRC.NroEmpresa,
		BB.Fantasia,
		PRC.SeqPessoa,
		AA.NomeRazao,
		C.ProcessoDNA,
		PRC.processo,
		C.Processo,
		TT.Codigo_Grupo,
		TT.Grupo_Cliente

ORDER BY 3;

-----------------------------------------------------------------------------------------------------------------------

	SELECT 
		Cod_Filial,
		Filial,
		A.SeqPessoa,
		NomeRazao,
		SeqPessoa_Conglomerado,
		NomeRazao_Conglomerado,
		ProcessoDNA,
		A.Processo,
		Dta_Entrega_Tecnica,
		Dta_Primeiro_Contato,
		DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Primeiro_Contato)						as 'Prazo_7_Dias_da_Entrega',
		case when Dta_Primeiro_Contato IS NULL THEN NULL
			 when DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Primeiro_Contato) <=7 then 1 else 0 end as Status_1,
		cast(B.Resultado as varchar) + ' - ' + B.Desc_Resultado						as Desc_Result_1,
		B.Detalhe																	as Detalhe_Result_1,
		Dta_Segundo_Contato,
		DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Segundo_Contato)						as 'Prazo_30_Dias_da_Entrega',
		case when Dta_Segundo_Contato IS NULL THEN NULL
			 when DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Segundo_Contato) <=30 then 1 else 0 end as Status_2,
		cast(C.Resultado as varchar) + ' - ' + C.Desc_Resultado						as Desc_Result_2,
		C.Detalhe																	as Detalhe_Result_2,
		Dta_Terceiro_Contato,
		DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Terceiro_Contato)						as 'Prazo_100_Dias_da_Entrega',
		case when Dta_Terceiro_Contato IS NULL THEN NULL					
			 when DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Terceiro_Contato) <=100 then 1 else 0 end as Status_3,
		cast(D.Resultado as varchar) + ' - ' + D.Desc_Resultado						as Desc_Result_3,
		D.Detalhe																	as Detalhe_Result_3,
        case when Dta_Primeiro_Contato IS NULL THEN 
			 DATEDIFF(DD,Dta_Entrega_Tecnica,getdate()) else 0 end as Prazo_Primeiro_Contato,
	    case when Dta_Segundo_Contato IS NULL THEN 
			 DATEDIFF(DD,Dta_Entrega_Tecnica,getdate()) else 0 end as Prazo_Segundo_Contato,
	    case when Dta_Terceiro_Contato IS NULL THEN 
			 DATEDIFF(DD,Dta_Entrega_Tecnica,getdate()) else 0 end as Prazo_Terceiro_Contato

	FROM ##ACUMULA	A LEFT JOIN ##ACAO63 B ON 
					A.Processo = B.Processo and
					A.Dta_Primeiro_Contato = B.DtaRealizacao

					LEFT JOIN  ##ACAO149 C ON 
					A.Processo = C.Processo and
					A.Dta_Segundo_Contato = C.DtaRealizacao

					LEFT JOIN  ##ACAO150 D ON 
					A.Processo = D.Processo and
					A.Dta_Terceiro_Contato = D.DtaRealizacao

	WHERE cast(Dta_Entrega_Tecnica as date) >= '2017-11-01'  --AND cast(Dta_Entrega_Tecnica as date) <= '2018-06-30' 
	AND (DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Primeiro_Contato) >= 0 OR DATEDIFF(DD,Dta_Entrega_Tecnica,Dta_Primeiro_Contato) IS NULL)
	AND B.Resultado not in (288,289)

	ORDER BY 9,8




