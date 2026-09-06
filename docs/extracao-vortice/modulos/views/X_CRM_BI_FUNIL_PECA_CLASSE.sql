/* ==============================================================
   Objeto ..........: dbo.X_CRM_BI_FUNIL_PECA_CLASSE
   Tipo ............: VIEW
   Criado em .......: 2023-04-24 09:33:48
   Modificado em ...: 2023-04-24 16:18:18
   Linhas ..........: 194
   Escreve em tabela: nao
   Tabelas referidas: IV_Acao, IV_CLASSERES, IV_HISTORICO, IV_PROCDADO, IV_RESCLASSE, IV_RESULTADO, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE   VIEW  [X_CRM_BI_FUNIL_PECA_CLASSE] AS

/*
VIEW CRIADA NO DIA 24/04/2023
BY - FELIPE VIOLIN

*/

--Temp_Funil:
--SELECT COUNT(*) FROM (

	Select 
		'PRIMEIRO_BLOCO'			as origem,
		A.SEQPESSOA					as cod_pessoa, 
		A.PROCESSO					as nro_processo,
		H.PROCESSODNA				AS dna,        
		F.CLASSE, 
		A.AcaoGeradora				AS cod_acao,
		G.DescReduzida				as desc_acao,
		D.RESULTADO 				as cod_resultado, 	-- COD_RESULTADO, 
		D.DESCRICAO 				as resultado, 		--RESULTADO, 
		A.RESULTADOCMPL				as compl_resultado, 
		A.DTAREALIZACAO				as data_historico, 
		A.CodUsuario				as cip, 
		C.CAMPANHA

FROM 
			IV_HISTORICO	A
LEFT JOIN 	IVS_PES			B	ON B.SEQPESSOA		= A.SEQPESSOA AND B.SEQDEPTO = 3
LEFT JOIN	IV_PROCDADO		C	ON C.PROCESSO		= A.PROCESSO
LEFT JOIN 	IV_RESULTADO 	D 	ON D.RESULTADO 		= A.RESULTADO
LEFT JOIN 	IV_RESCLASSE 	E 	ON E.RESULTADO 		= D.RESULTADO
LEFT JOIN 	IV_CLASSERES 	F 	ON F.SEQCLASSERES 	= E.SEQCLASSERES --AND F.CLASSE IN ('1-Cobertura Peças')
LEFT JOIN	IV_Acao			G 	ON G.Acao			= A.ACAOGERADORA
LEFT JOIN 	IV_ProcDado 	H	ON a.Processo 		= H.Processo 

WHERE 
		YEAR (DTAREALIZACAO) >='2020'
	AND F.CLASSE = ('1-Cobertura Peças')
	AND A.SEQHISTORICO = 
		(	SELECT 
				MAX(SeqHistorico) 
			FROM 
						IV_HISTORICO AA
			LEFT JOIN 	IV_RESCLASSE AB ON AA.RESULTADO 	=	AB.RESULTADO
			LEFT JOIN 	IV_CLASSERES AC ON AB.SEQCLASSERES	=	AC.SEQCLASSERES
			WHERE 
				AC.CLASSE = '1-Cobertura Peças'
				AND AA.PROCESSO=A.PROCESSO
		)
        AND D.RESULTADO<>1640
	        
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
UNION ALL 
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Select 
		'SEGUNDO_BLOCO'				as origem,
		A.SEQPESSOA					as cod_pessoa, 
		A.PROCESSO					as nro_processo, 
		H.PROCESSODNA				AS dna,
		F.CLASSE, 
		
		A.AcaoGeradora				AS cod_acao,
		G.DescReduzida				as desc_acao,
		
		D.RESULTADO 				as cod_resultado, 	-- COD_RESULTADO, 
		D.DESCRICAO 				as resultado, 		--RESULTADO, 
		A.RESULTADOCMPL				as compl_resultado, 
		A.DTAREALIZACAO				as data_historico, 
		A.CodUsuario				as cip, 
		C.CAMPANHA

FROM 
			IV_HISTORICO	A
LEFT JOIN 	IVS_PES			B	ON B.SEQPESSOA		= A.SEQPESSOA AND B.SEQDEPTO = 3
LEFT JOIN	IV_PROCDADO		C	ON C.PROCESSO		= A.PROCESSO
LEFT JOIN 	IV_RESULTADO 	D 	ON D.RESULTADO 		= A.RESULTADO
LEFT JOIN 	IV_RESCLASSE 	E 	ON E.RESULTADO 		= D.RESULTADO
LEFT JOIN 	IV_CLASSERES 	F 	ON F.SEQCLASSERES 	= E.SEQCLASSERES 
LEFT JOIN	IV_Acao			G 	ON G.Acao			= A.ACAOGERADORA
LEFT JOIN 	IV_ProcDado 	H	ON a.Processo 		= H.Processo 

WHERE 
		YEAR (DTAREALIZACAO) >='2020'
	AND F.CLASSE = ('1-Cobertura Peças')
	AND A.SEQHISTORICO = 
		(	SELECT 
				MAX(SeqHistorico) 
			FROM 
						IV_HISTORICO AA
			LEFT JOIN 	IV_RESCLASSE AB ON AA.RESULTADO 	=	AB.RESULTADO
			LEFT JOIN 	IV_CLASSERES AC ON AB.SEQCLASSERES	=	AC.SEQCLASSERES
			WHERE 
				AC.CLASSE = '1-Cobertura Peças'
				AND AA.PROCESSO=A.PROCESSO
		)

AND D.RESULTADO in (1640,1641) 

--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
UNION ALL 
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
--Funil - '3-Pedido Peças'
	
	Select 
		'TERCEIRO_BLOCO'			as origem,
		A.SEQPESSOA					as cod_pessoa, 
		A.PROCESSO					as nro_processo,
		H.PROCESSODNA				AS dna,
		--//F.CLASSE, 
		--//'2-Pedido Peças'		AS CLASSE,
        '2-Orçamento Peças'			AS CLASSE, 
		A.AcaoGeradora				AS cod_acao,
		G.DescReduzida				as desc_acao,
		
		D.RESULTADO 				as cod_resultado, -- COD_RESULTADO, 
		D.DESCRICAO 				as resultado, 		--RESULTADO, 
		A.RESULTADOCMPL				as compl_resultado, 
		A.DTAREALIZACAO				as data_historico, 
		A.CodUsuario				as cip, 
		C.CAMPANHA

FROM 
			IV_HISTORICO	A
LEFT JOIN 	IVS_PES			B	ON B.SEQPESSOA		= A.SEQPESSOA AND B.SEQDEPTO = 3
LEFT JOIN	IV_PROCDADO		C	ON C.PROCESSO		= A.PROCESSO
LEFT JOIN 	IV_RESULTADO 	D 	ON D.RESULTADO 		= A.RESULTADO
LEFT JOIN 	IV_RESCLASSE 	E 	ON E.RESULTADO 		= D.RESULTADO
LEFT JOIN 	IV_CLASSERES 	F 	ON F.SEQCLASSERES 	= E.SEQCLASSERES --AND F.CLASSE IN ('1-Cobertura Peças')
LEFT JOIN	IV_Acao			G 	ON G.Acao			= A.ACAOGERADORA
LEFT JOIN 	IV_ProcDado 	H	ON a.Processo 		= H.Processo 

WHERE 
		YEAR (DTAREALIZACAO) >='2020'
	AND F.CLASSE = ('3-Pedido Peças')
	AND A.SEQHISTORICO = 
		(	SELECT 
				MAX(SeqHistorico) 
			FROM 
						IV_HISTORICO AA
			LEFT JOIN 	IV_RESCLASSE AB ON AA.RESULTADO 	=	AB.RESULTADO
			LEFT JOIN 	IV_CLASSERES AC ON AB.SEQCLASSERES	=	AC.SEQCLASSERES
			WHERE 
				AC.CLASSE = '3-Pedido Peças'
				AND AA.PROCESSO=A.PROCESSO
		)

--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
UNION ALL 
--///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
--Funil - '4-Faturamento Peças'
Select 
		'QUARTO_BLOCO'				as origem,
		A.SEQPESSOA					as cod_pessoa, 
		A.PROCESSO					as nro_processo, 
		H.PROCESSODNA				AS dna,        
		--//F.CLASSE, 
		'3-Faturamento Peças'		AS CLASSE,
		A.AcaoGeradora				AS cod_acao,
		G.DescReduzida				as desc_acao,
		
		D.RESULTADO 				as cod_resultado, -- COD_RESULTADO, 
		D.DESCRICAO 				as resultado, 		--RESULTADO, 
		A.RESULTADOCMPL				as compl_resultado, 
		A.DTAREALIZACAO				as data_historico, 
		A.CodUsuario				as cip, 
		C.CAMPANHA

FROM 
			IV_HISTORICO	A
LEFT JOIN 	IVS_PES			B	ON B.SEQPESSOA		= A.SEQPESSOA AND B.SEQDEPTO = 3
LEFT JOIN	IV_PROCDADO		C	ON C.PROCESSO		= A.PROCESSO
LEFT JOIN 	IV_RESULTADO 	D 	ON D.RESULTADO 		= A.RESULTADO
LEFT JOIN 	IV_RESCLASSE 	E 	ON E.RESULTADO 		= D.RESULTADO
LEFT JOIN 	IV_CLASSERES 	F 	ON F.SEQCLASSERES 	= E.SEQCLASSERES --AND F.CLASSE IN ('1-Cobertura Peças')
LEFT JOIN	IV_Acao			G 	ON G.Acao			= A.ACAOGERADORA
LEFT JOIN 	IV_ProcDado 	H	ON a.Processo 		= H.Processo 

WHERE 
		YEAR (DTAREALIZACAO) >='2020'
	AND F.CLASSE = ('4-Faturamento Peças')
	AND A.SEQHISTORICO = 
		(	SELECT 
				MAX(SeqHistorico) 
			FROM 
						IV_HISTORICO AA
			LEFT JOIN 	IV_RESCLASSE AB ON AA.RESULTADO 	=	AB.RESULTADO
			LEFT JOIN 	IV_CLASSERES AC ON AB.SEQCLASSERES	=	AC.SEQCLASSERES
			WHERE 
				AC.CLASSE = '4-Faturamento Peças'
				AND AA.PROCESSO=A.PROCESSO
		)

--		)MAIN WHERE origem = 'QUARTO_BLOCO' ;	