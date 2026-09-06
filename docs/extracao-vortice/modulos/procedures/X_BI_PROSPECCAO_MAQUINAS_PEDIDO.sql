/* ==============================================================
   Objeto ..........: dbo.X_BI_PROSPECCAO_MAQUINAS_PEDIDO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-03-10 10:36:49
   Modificado em ...: 2023-03-10 10:36:49
   Linhas ..........: 146
   Escreve em tabela: nao
   Tabelas referidas: GE_EMPRESA, GE_PESSOA, IV_ACAO, IV_CLASSERES, IV_HISTORICO, IV_PROCDADO, IV_PROCESSO, IV_RESCLASSE, IV_RESULTADO, IVS_CARTEIRA, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE   Procedure [dbo].[X_BI_PROSPECCAO_MAQUINAS_PEDIDO]  
as


----------------------------------------------------------------------------------------------------------
--Tabela 01
IF  EXISTS (SELECT * FROM tempdb.sys.tables WHERE name like '#Tab01%')
BEGIN
   DROP TABLE #Tab01
END



                    SELECT 
                    	aax.PROCESSO			AS PROCESSO, 
                    	MAX(aax.SeqHistorico)	AS SeqHistorico
                    	
                    Into #Tab01
            
                    FROM        IV_HISTORICO aax
                    LEFT JOIN 	IV_RESCLASSE abx         ON aax.RESULTADO 		=	abx.RESULTADO
                    LEFT JOIN 	IV_CLASSERES acx         ON abx.SEQCLASSERES	=	acx.SEQCLASSERES
            
                    WHERE 	
                    	acx.SeqClasseRes = 40
                    GROUP BY
                    	aax.PROCESSO
                    
                    Create nonclustered index idx on #Tab01	 (PROCESSO)	
                    --SELECT * FROM #Tab01	
----------------------------------------------------------------------------------------------------------
--Tabela 02
IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name like '#Tab02%')
BEGIN
   DROP TABLE #Tab02
END 

		            SELECT  
		            	kx.SEQPESSOAPRC,
		            	MAX(CAST(ax.DTAREALIZACAO as DATE))	AS DTAREALIZACAO
		            	
		            Into #Tab02	
		            
		            FROM        	IV_HISTORICO	ax
		            LEFT JOIN 		IVS_PES			bx	ON bx.SEQPESSOA		= ax.SEQPESSOA 
		            LEFT JOIN 		IV_RESULTADO 	dx 	ON dx.RESULTADO 	= ax.RESULTADO
		            LEFT JOIN 		IV_RESCLASSE 	ex 	ON ex.RESULTADO 	= dx.RESULTADO
		            LEFT JOIN 		IV_CLASSERES 	fx 	ON fx.SEQCLASSERES 	= ex.SEQCLASSERES
		            LEFT JOIN 		GE_PESSOA 		kx 	ON kx.SeqPessoa 	= ax.SeqPessoa
					INNER JOIN 		#Tab01 			aax ON aax.PROCESSO		= ax.PROCESSO
		            
		            
		            WHERE 
		            	CAST(ax.DTAREALIZACAO as DATE) 	>= '2022-01-01'
		            AND ax.SEQHISTORICO 				= aax.SeqHistorico
		            AND fx.SeqClasseRes 				= 40
		            AND bx.SeqCarteira 					IS NOT NULL
		            --AND ax.SEQHISTORICO = 
		            GROUP BY 
		            	kx.SEQPESSOAPRC
		           
		            Create nonclustered index idx on #Tab02	 (SEQPESSOAPRC)
            
            		DROP TABLE #Tab01
            		
					--SELECT * FROM #Tab02
					
----------------------------------------------------------------------------------------------------------
--Tabela 03					
IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name like '#Tab03%')
BEGIN
   DROP TABLE #Tab03
END 					
				
    SELECT --DISTINCT
        'PEDIDO'						as classe,
        b.SeqCarteira 					as carteira,
        j.Descricao						as desc_carteira,
        h.Fantasia						as filial,
        c.PROCESSODNA					as dna,
        a.PROCESSO						as processo, 
        CAST(a.DTAREALIZACAO as DATE)	as data_funil,
 		kx.DTAREALIZACAO				as data_prosp,
        a.AcaoGeradora					as acao,
        g.DescReduzida					as desc_acao,
        d.RESULTADO 					as resultado,
        d.DESCRICAO 					as desc_resultado, 
        a.RESULTADOCMPL					as compl_resultado, 
        a.CodUsuario					as atendente, 
        k.SEQPESSOA						as pessoa,
        k.SEQPESSOAPRC					as conglomerado,
        c.CAMPANHA						as campanha,
        i.Fase							as fase,
        i.Status						as status
        
        Into #Tab03

    FROM        	IV_HISTORICO	a
    LEFT JOIN 		IVS_PES			b	ON b.SEQPESSOA		= a.SEQPESSOA 		    AND b.SEQDEPTO = 2
    LEFT JOIN		IV_PROCDADO		c	ON c.PROCESSO		= a.PROCESSO
    LEFT JOIN 		IV_RESULTADO 	d 	ON d.RESULTADO 		= a.RESULTADO
    LEFT JOIN 		IV_RESCLASSE 	e 	ON e.RESULTADO 		= d.RESULTADO
    LEFT JOIN 		IV_CLASSERES 	f 	ON f.SEQCLASSERES 	= e.SEQCLASSERES
    LEFT JOIN		IV_ACAO			g 	ON g.Acao			= a.ACAOGERADORA
    LEFT JOIN   	GE_EMPRESA      h   ON h.NroEmpresa     = a.NroEmpresa
    LEFT JOIN     	IV_PROCESSO		i	ON i.Processo		= a.PROCESSO
    LEFT JOIN		IVS_CARTEIRA	j	ON j.SeqCarteira	= b.SeqCarteira
    LEFT JOIN 		GE_PESSOA 		k 	ON k.SeqPessoa 	 	= a.SeqPessoa
    INNER JOIN		#Tab02 			kx 	ON kx.SEQPESSOAPRC 	= k.SEQPESSOAPRC

    WHERE CAST(a.DTAREALIZACAO as DATE) >= '2022-01-01'
    AND b.SeqCarteira IS NOT NULL
    AND f.SeqClasseRes = 12 
	
    DROP TABLE #Tab02
    
 ----------------------------------------------------------------------------------------------------------
 --SQL RETURNING TO BI
 
    SELECT 

	    main.classe,
	    main.carteira,
	    main.desc_carteira,
	    main.filial,
	    main.dna,
	    main.processo,
	    main.data_funil,
	    main.acao,
	    main.desc_acao,
	    main.resultado,
	    main.desc_resultado,
	    main.compl_resultado,
	    main.atendente,
	    main.pessoa,
	    main.conglomerado,
	    main.campanha,
	    main.fase,
	    main.status

	FROM 
	    #Tab03 main
	WHERE 
		main.data_prosp <= main.data_funil
	
	;