/* ==============================================================
   Objeto ..........: dbo.X_BI_PROSPECCAO_MAQUINAS_BASE_FATURAMENTO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-03-10 11:31:52
   Modificado em ...: 2023-03-10 11:31:52
   Linhas ..........: 132
   Escreve em tabela: nao
   Tabelas referidas: GE_Empresa, GE_PESSOA, IV_Acao, IV_CLASSERES, IV_HISTORICO, IV_PROCDADO, IV_RESCLASSE, IV_RESULTADO, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

--Executar procedure 
--Exec X_BI_PROSPECCAO_MAQUINAS_BASE_FATURAMENTO;


CREATE   Procedure [dbo].[X_BI_PROSPECCAO_MAQUINAS_BASE_FATURAMENTO]  
as


----------------------------------------------------------------------------------------------------------
--Tabela 01
IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab01_3%')
BEGIN
   DROP TABLE #Tab01_3
END



                    SELECT 
                    	aax.PROCESSO			AS PROCESSO, 
                    	MAX(aax.SeqHistorico)	AS SeqHistorico
                    	
                    Into #Tab01_3
            
                    FROM        IV_HISTORICO aax
                    LEFT JOIN 	IV_RESCLASSE abx         ON aax.RESULTADO 		=	abx.RESULTADO
                    LEFT JOIN 	IV_CLASSERES acx         ON abx.SEQCLASSERES	=	acx.SEQCLASSERES
            
                    WHERE 	
                    	acx.SeqClasseRes = 40
                    GROUP BY
                    	aax.PROCESSO
                    
                    Create nonclustered index idx on #Tab01_3	 (PROCESSO)	
                    --SELECT * FROM #Tab01	
----------------------------------------------------------------------------------------------------------
--Tabela 02
IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name like '#Tab02_3%')
BEGIN
   DROP TABLE #Tab02_3
END 

		            SELECT  
		            	kx.SEQPESSOAPRC,
		            	MAX(CAST(ax.DTAREALIZACAO as DATE))	AS DTAREALIZACAO
		            	
		            Into #Tab02_3	
		            
		            FROM        	IV_HISTORICO	ax
		            LEFT JOIN 		IVS_PES			bx	ON bx.SEQPESSOA		= ax.SEQPESSOA 
		            LEFT JOIN 		IV_RESULTADO 	dx 	ON dx.RESULTADO 	= ax.RESULTADO
		            LEFT JOIN 		IV_RESCLASSE 	ex 	ON ex.RESULTADO 	= dx.RESULTADO
		            LEFT JOIN 		IV_CLASSERES 	fx 	ON fx.SEQCLASSERES 	= ex.SEQCLASSERES
		            LEFT JOIN 		GE_PESSOA 		kx 	ON kx.SeqPessoa 	= ax.SeqPessoa
					INNER JOIN 		#Tab01_3 		aax ON aax.PROCESSO		= ax.PROCESSO
		            
		            
		            WHERE 
		            	CAST(ax.DTAREALIZACAO as DATE) 	>= '2022-01-01'
		            AND ax.SEQHISTORICO 				= aax.SeqHistorico
		            AND fx.SeqClasseRes 				= 40
		            AND bx.SeqCarteira 					IS NOT NULL
		            --AND ax.SEQHISTORICO = 
		            GROUP BY 
		            	kx.SEQPESSOAPRC
		           
		            Create nonclustered index idx on #Tab02_3	 (SEQPESSOAPRC)
            
            		DROP TABLE #Tab01_3
            		
					--SELECT * FROM #Tab02
					
----------------------------------------------------------------------------------------------------------
--Tabela 03					
IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name like '#Tab03_3%')
BEGIN
   DROP TABLE #Tab03_3
END 					
				
SELECT 
	k.Fantasia											as f_filial,
	a.PROCESSO					                    	as f_processo,
    c.PROCESSODNA										as f_dna,   
    CAST(a.DTAREALIZACAO as DATE)                   	as f_data_faturamento, 
    a.ACAOGERADORA										as f_acao,
    g.Descricao											as f_desc_acao,
    a.RESULTADO											as f_resultado,
	d.DESCRICAO 				                    	as f_desc_resultado,
    a.SeqPessoa 										as f_pessoa,
    i.NomeRazao 										as f_nome_pessoa,
    i.NroCGCCPF 										as f_cgccpf_pessoa,
    i.Cidade 											as f_cidade_pessoa,
    i.SEQPESSOAPRC 										as f_conglomerado,
    j.NomeRazao 										as f_nome_conglomerado,
    j.NroCGCCPF 		 								as f_cgccpf_conglomerado,
    j.Cidade 											as f_cidade_conglomerado,
    a.Vendedor											as f_cen,
    a.CodUsuario				                    	as f_atendente,
    f.CLASSE											as f_classe,
    c.Campanha											as f_campanha
    
    Into #Tab03_3
    
FROM        IV_HISTORICO	a
LEFT JOIN 	IVS_PES			b	ON b.SEQPESSOA		= a.SEQPESSOA		AND b.SEQDEPTO = 2
LEFT JOIN	IV_PROCDADO		c	ON c.PROCESSO		= a.PROCESSO
LEFT JOIN 	IV_RESULTADO 	d 	ON d.RESULTADO 		= a.RESULTADO
LEFT JOIN 	IV_RESCLASSE 	e 	ON e.RESULTADO 		= d.RESULTADO
LEFT JOIN 	IV_CLASSERES 	f 	ON f.SEQCLASSERES 	= e.SEQCLASSERES
LEFT JOIN	IV_Acao			g 	ON g.Acao			= a.ACAOGERADORA		  
LEFT JOIN	GE_Pessoa		i	ON i.SeqPessoa		= a.SeqPessoa 		 
LEFT JOIN 	GE_Pessoa 	 	j	ON j.SeqPessoa		= i.SEQPESSOAPRC	
LEFT JOIN   GE_Empresa		k	ON k.NroEmpresa		= c.NroEmpresa
INNER JOIN	#Tab02_3 		kx 	ON kx.SEQPESSOAPRC 	= i.SEQPESSOAPRC

WHERE 
	CAST(a.DTAREALIZACAO as DATE) >= '2022-01-01' 
AND f.SeqClasseRes = 11

	
    DROP TABLE #Tab02_3
    
 ----------------------------------------------------------------------------------------------------------
 --SQL RETURNING TO BI
 
    SELECT 

	*

	FROM 
	    #Tab03_3 main
	
	;