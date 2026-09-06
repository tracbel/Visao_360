/* ==============================================================
   Objeto ..........: dbo.BI_COBERTURA_PROSPECCAO_MAQUINAS
   Tipo ............: VIEW
   Criado em .......: 2024-01-04 14:59:41
   Modificado em ...: 2024-01-04 16:42:02
   Linhas ..........: 220
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Acao, IV_CLASSERES, IV_HISTORICO, IV_PROCDADO, IV_RESCLASSE, IV_RESULTADO, IVS_Depto, IVS_PES
   Outras refs .....: X_CRM_BI_CONGLOMERADO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE   VIEW BI_COBERTURA_PROSPECCAO_MAQUINAS AS
		SELECT 
			j.count_pessoa,
			j.count_seminteresse,
			j.count_interessefuturo,
			--j.max_SeqHistorico,
			CASE 
				when month(CAST(c.DtaUltResultado as DATE)) > month(CAST(a.DTAREALIZACAO as DATE)) then 1
				else 0
			END																										as is_ultresultado,
			year(CAST(a.DTAREALIZACAO as DATE)) 																	as ano,
			CONVERT(char(2), CAST(a.DTAREALIZACAO as DATE), 101)													as mes,
			a.SEQHISTORICO,
			
			 -- month with 2 digits
		
			--xx.SeqPessoa,
			CAST(a.SEQPESSOA as INT) 																				as seq_pessoa,
		    CAST(
		    	CASE 
					WHEN LEN(i.digCGCCPF) = 1 
						THEN CAST(i.NroCGCCPF as VARCHAR(12)) + '0' + CAST(i.DigCGCCPF as CHAR(2))  
					WHEN LEN(i.digCGCCPF) = 2 
						THEN CAST(i.NroCGCCPF as VARCHAR(12)) + CAST(i.DigCGCCPF as CHAR(2))
						ELSE CAST(i.NroCGCCPF as VARCHAR(12))          
	        END 
		    as VARCHAR)																								as cpf_cnpj_pessoa,
		    CAST(a.PROCESSO as INT) 																				as nro_processo,
		    UPPER(CAST(f.CLASSE as VARCHAR))																		as classe,
		    UPPER(CAST(d.DESCRICAO as VARCHAR)) 																	as resultado,
		    d.RESULTADO 																							as nro_resultado,
		    --c.DtaUltResultado,
		    UPPER(CAST(d.DescReduzida as VARCHAR))																	as oportunidade,
		    CAST(a.NroEmpresa as INT) 																				as empresa,
		    CAST(a.DTAREALIZACAO as DATE)																			as data_historico,
		    CAST(DATEADD(DD, DATEDIFF(DD, 0, a.DTAREALIZACAO + 30), 0) as DATE)										as data_historico_final,
		    c.DtaUltResultado,
		    UPPER(CAST(a.CodUsuario as VARCHAR)) 																	as cip,
		    UPPER(CAST(c.campanha as VARCHAR)) 																		as campanha,
		    UPPER(CAST(g.DescReduzida as VARCHAR))																	as acao,
		    CONGLO.seq_conglo
		   -- *
		
	FROM        IV_HISTORICO	    a WITH (NOLOCK)
	LEFT JOIN 	IVS_PES			    b WITH (NOLOCK)			ON 	b.SEQPESSOA		= a.SEQPESSOA
															AND b.SEQDEPTO 		IN 	
															(	SELECT 
																	SeqDepto 
																FROM 
																	IVS_Depto WITH (NOLOCK)
																WHERE 
																	Descricao in ('Prospecção de Maquinas e Imple','Prospecção Pós Venda') 
															) --FILTRO PELO NOME DO DEPARTAMENTO VINCULADO AO PROCESSO
	LEFT JOIN	IV_PROCDADO		    c WITH (NOLOCK)			ON 	c.PROCESSO		= a.PROCESSO
	LEFT JOIN 	IV_RESULTADO 	    d WITH (NOLOCK)			ON 	d.RESULTADO 	= a.RESULTADO
	LEFT JOIN 	IV_RESCLASSE 	    e WITH (NOLOCK)			ON 	e.RESULTADO 	= d.RESULTADO
	LEFT JOIN 	IV_CLASSERES 	    f WITH (NOLOCK)			ON 	f.SEQCLASSERES 	= e.SEQCLASSERES 
	LEFT JOIN	IV_Acao			    g WITH (NOLOCK)			ON 	g.Acao			= a.ACAOGERADORA
	LEFT JOIN 	IV_ProcDado 	    h WITH (NOLOCK)			ON 	h.Processo 		= a.Processo 
	LEFT JOIN	GE_Pessoa		    i WITH (NOLOCK)			ON 	i.SeqPessoa 	= a.SeqPessoa 
	--JOIN   IV_Agenda 			k WITH(NOLOCK)			ON  k.SeqPessoa 	= i.SeqPessoa
	---------------------------------------------------------------------------------------------------------------------------------------------
	INNER JOIN (
				SELECT 
					aa.SEQPESSOA											as SeqPessoa,
					year(CAST(aa.DTAREALIZACAO as DATE)) 					as ano,
					CONVERT(char(2), CAST(aa.DTAREALIZACAO as DATE), 101) 	as mes,
					COUNT(CONVERT(VARCHAR,aa.SEQPESSOA) + ' | ' + CONVERT(VARCHAR, year(aa.DTAREALIZACAO)) + ' | ' + CONVERT(char(2), aa.DTAREALIZACAO)) as count_pessoa,
					COUNT(	CASE
								WHEN ae.Descricao = 'SEM INTERESSE DE COMPRA' THEN 1
							END
					)														as count_seminteresse,
					COUNT(	CASE
								WHEN ae.Descricao = 'COM INTERESSE FUTURO' THEN 1
							END
					)														as count_interessefuturo,
	        		MAX(aa.SeqHistorico) 				 					as max_SeqHistorico
	            FROM            IV_HISTORICO aa WITH (NOLOCK)
	            LEFT JOIN 	    IV_RESCLASSE ab WITH (NOLOCK)   on aa.RESULTADO 	=	ab.RESULTADO
	            LEFT JOIN 	    IV_CLASSERES ac WITH (NOLOCK)   on ab.SEQCLASSERES	=	ac.SEQCLASSERES
	            LEFT JOIN		IV_Acao		 ad	WITH (NOLOCK)	on ad.Acao			= 	aa.ACAOGERADORA
	            LEFT JOIN 		IV_RESULTADO ae WITH (NOLOCK)	ON ae.RESULTADO 	= 	aa.RESULTADO
	
	            WHERE 

	            	ac.Classe in (
					            	'4-Faturamento',
									'3-Pedido',
									'1-Cobertura',
									'2-Oportunidade'
								)
	            AND aa.RESULTADO in (
										1586,
										1590,
										1663,
										2141,
										2143,
										2176,
										2140,
										2142
			    					)
	            AND ad.DescReduzida in	(			
	            
	    						'PROSP CLI MAQ IM',
	    						'VENDER PRODUTO'
	    					)
	    					
	    		AND ad.EmUso = 'S'
	    		
	    		AND aa.CodUsuario in ('JEFERSON.FAVERO', 'LEONARDO.FERNANDES')
	    		
		    	AND TRIM(ae.DESCRICAO) not in (
							'Negociação em andamento',
							'Pedido realizado sem usado',
							'Contato sem sucesso',
							'Solicitado cotação de compra',
							'Desistiu da compra',
							'Venda pedida',
							'Processo cancelado',
							'Solicitado demonstração'
	   					
	   					) 
	    		
	  		
	    		GROUP by 
	    			aa.SEQPESSOA, 
	    			year(CAST(aa.DTAREALIZACAO as DATE)),
	    			CONVERT(char(2), CAST(aa.DTAREALIZACAO as DATE), 101)	
	) 			j 	on 		j.SeqPessoa 		= a.SEQPESSOA
					and 	j.ano				= year(CAST(a.DTAREALIZACAO as DATE))
					and 	j.mes				= CONVERT(char(2), CAST(a.DTAREALIZACAO as DATE), 101)
					--AND 	j.max_SeqHistorico	= a.SEQHISTORICO
	---------------------------------------------------------------------------------------------------------------------------------------------
	LEFT JOIN X_CRM_BI_CONGLOMERADO CONGLO ON 	a.SEQPESSOA  = CONGLO.seq_pessoa 			
	---------------------------------------------------------------------------------------------------------------------------------------------	
	
	WHERE 
				CAST(a.DTAREALIZACAO as DATE) >= '2024-01-01'  --AND CAST(a.DTAREALIZACAO as DATE) <= '2023-12-31'
		--and 	CAST(a.DTAREALIZACAO as DATE) <= '2023-05-31'
		AND f.Classe in 	(										--CLASSES DO RESULTADO UTILIZADA COMO BASE DO FUNIL
								'4-Faturamento',
								'3-Pedido',
								'1-Cobertura',
								'2-Oportunidade'
							) 	
		AND d.EMUSO = 1 											--VERIFICA SE O STATUS DA RESULTADO ESTA ATIVO (S=SIM, N=N O)						
	    AND g.DescReduzida in	(										--ACOES QUE VAO SER UTILIZADAS COMO BASE DO FUNIL
	    						'PROSP CLI MAQ IM',
	    						'VENDER PRODUTO'
	    					)
	    					
	   	AND (
	   		 	j.count_pessoa = 1
	   			OR 	(
	   					d.DESCRICAO = 'SEM INTERESSE DE COMPRA'
	   					and j.count_pessoa >= 1 
	   					and j.max_SeqHistorico = a.SEQHISTORICO
	   					and j.count_seminteresse = j.count_pessoa
	   				)																						-- SE TIVER SÓ SEM INTERESSE, TRAZ O ULTIMO
	   			OR 	(
	   					d.DESCRICAO <> 'SEM INTERESSE DE COMPRA'
	   					and j.count_pessoa >= 2
	   					and j.count_seminteresse >= 1 and j.count_seminteresse < j.count_pessoa	
	   					and j.count_interessefuturo = 0														-- SE TIVER SEM INTERESSE E MAIS ALGUM QUE NAO SEJA INTERESSE FUTURO
	   				)
	   			OR  (
	   					j.count_pessoa >= 2
	   					and j.count_seminteresse >= 1 and j.count_seminteresse < j.count_pessoa	
	   					and j.count_interessefuturo >= 1 and j.count_interessefuturo < j.count_pessoa
	   					and j.max_SeqHistorico = a.SEQHISTORICO												-- SE TIVER SEM INTERESSE E COM INTERESSE FUTURO						-- 
	   				)
	   			OR 	(
	   					j.count_pessoa >= 2
	   					and j.count_seminteresse = 0
	   					and j.max_SeqHistorico = a.SEQHISTORICO
	   					AND j.count_interessefuturo = 0														-- SE NAO TIVER SEM INTERESSE E NEM INTERESSE FUTURO
	   				)
	   			OR 	(
	   					d.DESCRICAO <> 'COM INTERESSE FUTURO'
	   					and j.count_pessoa >= 2
	   					and j.count_interessefuturo >= 1 and j.count_interessefuturo < j.count_pessoa
	   				)
	   			OR  (
	   					j.count_pessoa >= 2
	   					and CAST(a.SEQPESSOA as INT) = 82409
	   				)
	   			
	   		)
	   		
	    AND d.RESULTADO in (
								1586,
								1590,
								1663,
								2141,
								2143,
								2176,
								2140,
								2142
	    					)
	    AND g.EmUso = 'S'											--VERIFICA SE O STATUS DA ACAO ESTA ATIVO (S=SIM, N=N O)
	    
	    
	    AND a.CodUsuario in ('JEFERSON.FAVERO', 'LEONARDO.FERNANDES')
	    
	   -- AND d.DESCRICAO = 'COM INTERESSE DE COMPRA'
	    --AND f.CLASSE = '2-OPORTUNIDADE'
    	AND TRIM(d.DESCRICAO) not in (
							'Negociação em andamento',
							'Pedido realizado sem usado',
							'Contato sem sucesso',
							'Solicitado cotação de compra',
							'Desistiu da compra',
							'Venda pedida',
							'Processo cancelado',
							'Solicitado demonstração'
	   					
	   					) 
	   					
	   					
