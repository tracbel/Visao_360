/* ==============================================================
   Objeto ..........: dbo.X_CRM_BI_CONGLOMERADO
   Tipo ............: VIEW
   Criado em .......: 2022-12-09 08:42:24
   Modificado em ...: 2023-07-10 22:00:42
   Linhas ..........: 62
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

-- dbo.X_CRM_BI_CONGLOMERADO source

CREATE   VIEW  [X_CRM_BI_CONGLOMERADO] AS

/*
VIEW CRIADA NO DIA 09/12/2022
BY - FELIPE VIOLIN

*/

select main.* from (
SELECT 

CASE 
 	WHEN LEN(a.DigCGCCPF) = 1 
 		THEN 
 			CAST(a.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(a.DigCGCCPF AS CHAR(2)) 
 	WHEN LEN(a.DigCGCCPF) = 2 
 		THEN 
     		CAST(a.NroCGCCPF AS VARCHAR(12)) + CAST(a.DigCGCCPF AS CHAR(2)) 
     	ELSE
 			CAST(a.NroCGCCPF AS VARCHAR(12)) 
 END 
 						as cnpj_pessoa,
 	a.SeqPessoa			as seq_pessoa,
	a.Status			as status_pessoa,
	a.NomeRazao			as nome_pessoa,
	a.FisicaJuridica	as tipo_pessoa,
	
CASE 
 	WHEN LEN(b.DigCGCCPF) = 1 
 		THEN 
 			CAST(b.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(b.DigCGCCPF AS CHAR(2)) 
 	WHEN LEN(b.DigCGCCPF) = 2 
 		THEN 
     		CAST(b.NroCGCCPF AS VARCHAR(12)) + CAST(b.DigCGCCPF AS CHAR(2)) 
     	ELSE
 			CAST(b.NroCGCCPF AS VARCHAR(12)) 
END 
 						as cnpj_conglo,
 	--b.DigCGCCPF,
 	--b.NroCGCCPF,
 						
	b.SeqPessoa			as seq_conglo,
	b.Status			as status_conglo,
	b.NomeRazao			as cliente_conglo,
	b.FisicaJuridica	as tipo_conglo,
	
CASE 
	WHEN a.SeqPessoa	=	b.SeqPessoa	 
		THEN 'S'
		ELSE 'N' 
end 					as principal

FROM 
			GE_Pessoa 	a
INNER JOIN	GE_Pessoa	b on a.SEQPESSOAPRC = b.SeqPessoa 

) main 
--WHERE 
--	main.cnpj_pessoa is not null 
--and main.cnpj_pessoa = '8006559000300';