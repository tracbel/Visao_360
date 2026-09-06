/* ==============================================================
   Objeto ..........: dbo.BI_AGENDA_PROSPECCAO
   Tipo ............: VIEW
   Criado em .......: 2023-07-10 09:10:24
   Modificado em ...: 2023-09-05 16:31:38
   Linhas ..........: 160
   Escreve em tabela: nao
   Tabelas referidas: GE_Empresa, GE_Pessoa, Ge_Usuario, IV_Acao, IV_Agenda
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

-- dbo.BI_AGENDA_PROSPECCAO source

create   view BI_AGENDA_PROSPECCAO AS

select 
	'PRIMEIRA' AS ORIGEM,
	a.SeqAgenda,
	a.UltResultado,
	a.Processo,
	CAST(a.DtaGeracaoOrig as DATE) 																					as 	DataCriacao,
	CAST(a.DtaUltResultado as DATE) 																				as 	DtaUltResultado,
	a.AssuntoCmpl,
	CAST(a.DtaAgenda as DATE)																						AS  DataAgenda,
	CASE 
		when month(CAST(a.DtaUltResultado as DATE)) > month(CAST(a.DtaGeracaoOrig as DATE)) then 1
		else 0
	END																												as is_ultresultado,
		

	e.Descricao																										as 	desc_acao,
	a.SeqPessoa																										as 	SeqPessoa,
	a.SeqUsuario																									as 	SeqUsuario,
	f.CodUsuario																									as  NomeUsuario,
	b.NomeRazao																										as 	Nome,
	a.Realizada																										as  Realizada,
    
	CAST(
		CASE 
			WHEN LEN(b.digCGCCPF) = 1 THEN CAST(b.NroCGCCPF as VARCHAR(12)) + '0' + CAST(b.DigCGCCPF as CHAR(2))  
			WHEN LEN(b.digCGCCPF) = 2 THEN CAST(b.NroCGCCPF as VARCHAR(12)) + CAST(b.DigCGCCPF as CHAR(2))
			ELSE CAST(b.NroCGCCPF as VARCHAR(12))          
		END as Varchar
    	)																											as  CPF_CNPJ,
    b.Cidade 																										as cidade_pessoa,
	  c.SeqUsuario 																									as desc_responsavel,
	  UPPER(TRIM(c.Nome))																							as Nome_Responsavel,
	  a.ResultadoCmpl 																								as ResultadoCompl,
	  d.Fantasia 																									as filial
	 -- a.*
	  
from IV_Agenda a 				with(nolock) 

LEFT JOIN GE_Pessoa 	b 		with(nolock)		ON b.SeqPessoa 	= a.SeqPessoa
LEFT JOIN Ge_Usuario 	c		with(nolock)		ON c.SeqUsuario = a.SeqUsuario
LEFT JOIN GE_Empresa	d		with(nolock)		ON d.NroEmpresa = a.NroEmpresa
LEFT JOIN IV_Acao 		e		with(nolock)		ON e.Acao  		= a.Acao
LEFT JOIN GE_Usuario 	f 		with(nolock)		ON f.SeqUsuario = a.SeqUsuario

WHERE 
	--a.DtaGeracaoOrig 	>= 	'2021-08-01'
	 a.DtaGeracaoOrig 	>= 	'2023-01-01'
AND  e.Descricao 	in 	('PROSPECTAR CLIENTE PEÇAS IM', 'PROSPECTAR CLIENTE CANAIS DIGITAIS', 'PROSPECTAR CLIENTE SERVIÇO IM')
	AND (
	
		a.ResultadoCmpl is null
		or 
				a.ResultadoCmpl = ''
			
	OR(
		a.ResultadoCmpl not in 
			(
				'Arrendamento',
				'Cliente no balcão',
				'Compra recente',
				'Conglomerado',
				'Contato recente',
				'Sem crédito',
				'Sem potencial'
			)
			
		)
	)			
				--AND a.SeqPessoa	= 73892
				--AND a.SeqPessoa	= 3847
				--AND a.SeqPessoa	= 86207
				--AND a.SeqPessoa	= 68516
				--and a.Processo = 1252241
	
	

---------------------------------------------------------------------------------------------------------------------------------------------------------------
UNION ALL 
-------------------------------------------------------------------------------------------------------------------------------------------
select
	'SEGUNDA' AS ORIGEM,
	a.SeqAgenda,
	a.UltResultado,
	a.Processo,
	CAST(a.DtaGeracaoOrig as DATE) 																					as 	DataCriacao,
	CAST(a.DtaUltResultado as DATE) 																				as 	DtaUltResultado,
	a.AssuntoCmpl,
	--DATEADD(DD, -DAY(DATEADD(M, 1, a.DtaGeracaoOrig)), DATEADD(M, 1, a.DtaGeracaoOrig)) AS UltimoDiaMes,
	CAST(a.DtaAgenda as DATE)																						as  DataAgenda,
	CASE 
		when month(CAST(a.DtaUltResultado as DATE)) > month(CAST(a.DtaGeracaoOrig as DATE)) then 1
		else 0
	END																												as  is_ultresultado,
	e.Descricao																										as 	desc_acao,
	a.SeqPessoa																										as 	SeqPessoa,
	a.SeqUsuario																									as  SeqUsuario,
	f.CodUsuario																									as  NomeUsuario,
	b.NomeRazao																										as 	Nome,
	a.Realizada																										as  Realizada,
    
	CAST(
		CASE 
			WHEN LEN(b.digCGCCPF) = 1 THEN CAST(b.NroCGCCPF as VARCHAR(12)) + '0' + CAST(b.DigCGCCPF as CHAR(2))  
			WHEN LEN(b.digCGCCPF) = 2 THEN CAST(b.NroCGCCPF as VARCHAR(12)) + CAST(b.DigCGCCPF as CHAR(2))
			ELSE CAST(b.NroCGCCPF as VARCHAR(12))          
		END as Varchar
    	)																											as  CPF_CNPJ,
    b.Cidade 																										as cidade_pessoa,
	  c.SeqUsuario 																									as desc_responsavel,
	  a.ResultadoCmpl 																								as ResultadoCompl,
	  UPPER(TRIM(c.Nome))																							as Nome_Responsavel,
	  d.Fantasia 																									as filial
	 -- a.*
	  
from IV_Agenda a 				with(nolock) 

LEFT JOIN GE_Pessoa 	b 		with(nolock)		ON b.SeqPessoa 	= a.SeqPessoa
LEFT JOIN Ge_Usuario 	c		with(nolock)		ON c.SeqUsuario = a.SeqUsuario
LEFT JOIN GE_Empresa	d		with(nolock)		ON d.NroEmpresa = a.NroEmpresa
LEFT JOIN IV_Acao 		e		with(nolock)		ON e.Acao  		= a.Acao
LEFT JOIN GE_Usuario 	f 		with(nolock)		ON f.SeqUsuario = a.SeqUsuario

WHERE 			
	a.DtaAgenda > DATEADD(DD, -DAY(DATEADD(M, 1, a.DtaGeracaoOrig)), DATEADD(M, 1, a.DtaGeracaoOrig))
	AND a.DtaGeracaoOrig 	>= 	'2023-01-01'
	AND e.Descricao 	in 	('PROSPECTAR CLIENTE PEÇAS IM', 'PROSPECTAR CLIENTE CANAIS DIGITAIS', 'PROSPECTAR CLIENTE SERVIÇO IM')
	AND (
	
		a.ResultadoCmpl is null
		or 
				a.ResultadoCmpl = ''
			
	OR(
		a.ResultadoCmpl not in 
			(
				'Arrendamento',
				'Cliente no balcão',
				'Compra recente',
				'Conglomerado',
				'Contato recente',
				'Sem crédito',
				'Sem potencial'
			)
			
		)
	)
	--AND a.SeqPessoa	= 73892
	--AND a.SeqPessoa	= 3847
	--AND a.SeqPessoa	= 86207
	--AND a.SeqPessoa	= 68516
	--and a.Processo = 1252241
	
	
	--select * from IV_Agenda where SeqPessoa = 8459 and DtaAgenda > '2023-08-01' and DtaAgenda < '2023-02-01'
	
	