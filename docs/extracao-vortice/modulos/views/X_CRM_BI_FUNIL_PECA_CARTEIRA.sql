/* ==============================================================
   Objeto ..........: dbo.X_CRM_BI_FUNIL_PECA_CARTEIRA
   Tipo ............: VIEW
   Criado em .......: 2023-04-24 21:44:48
   Modificado em ...: 2023-08-15 13:16:23
   Linhas ..........: 375
   Escreve em tabela: nao
   Tabelas referidas: GE_Empresa, GE_Pessoa, GE_Usuario, IVS_CARTEIRA, IVS_Depto, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

-- dbo.X_CRM_BI_FUNIL_PECA_CARTEIRA source

CREATE   VIEW  [X_CRM_BI_FUNIL_PECA_CARTEIRA] AS

/*
VIEW CRIADA NO DIA 24/04/2023
BY - FELIPE VIOLIN

*/

select
	a.departamento,
	a.Carteira_pessoa	as seq_carteira,
	Case 
		When a.Carteira_pessoa is null 
			then 'Sem Carteira'
			else 'Com Carteira' end 
						as carteira,						
	b.Carteira			as cod_carteira,
	b.Descricao			as nome_carteira,
	
	B.SeqUsrResp		as seq_cip,
	c.NomeReduzido		as nome_cip,
	
	D.Fantasia			as filial,
   	substring(D.Fantasia, 9, len(D.Fantasia)) 
						as empresa,
	
	a.status_pessoa,					
	a.cnpj_pessoa		as seq_pessoa,
	a.seq_pessoa		as cod_pessoa,
	a.nome_pessoa		as cliente,
	a.cidade_pessoa,
	a.tipo_pessoa		as tipo_pessoa,
	
	a.cnpj_conglo		as seq_conglo,
	a.seq_conglo		as cod_conglo,
	a.cidade_conglo,

	a.cliente_conglo	as grupo_conglo,
	--a.tipo_conglo,
	a.principal,
	a.prospeccao



From (

select 
	departamento,
	cnpj_pessoa,
	DigCGCCPF,
	NroCGCCPF,
	status_pessoa,
	seq_pessoa,
	nome_pessoa,
	cidade_pessoa,
	tipo_pessoa,
	cnpj_conglo,
	seq_conglo,
	status_conglo,
	cliente_conglo,
	cidade_conglo,
	tipo_conglo,
	
	case 
		when Carteira_pessoa is not null 
			then 'S'
			else principal end 
							as principal,
	--Carteira_pessoa,
	--Carteira_Conglomerado,
	case 	
		when Carteira_pessoa is null 
			then Carteira_Conglomerado
			else Carteira_pessoa end 
							as Carteira_pessoa,
							
case when 
(select COUNT(cc.seqpessoa)  from IVS_PES cc where cc.seqcarteira in(165,204) and (cc.seqpessoa = seq_conglo or cc.seqpessoa = seq_pessoa))	>0 
	then  'Ativo'
	else  'Inativo' end	as prospeccao
	
from (


--consulta banco de dados para cadastro / carteira do conglomerado
select 
	'Peças'				as departamento,
	 CASE 
 	WHEN LEN(a.DigCGCCPF) = 1 THEN 
 		CAST(a.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(a.DigCGCCPF AS CHAR(2)) 
 	WHEN LEN(a.DigCGCCPF) = 2 THEN 
     	CAST(a.NroCGCCPF AS VARCHAR(12)) + CAST(a.DigCGCCPF AS CHAR(2)) ELSE
 		CAST(a.NroCGCCPF AS VARCHAR(12)) END 
 						as cnpj_pessoa,
	a.SeqPessoa			as seq_pessoa,
	a.Status			as status_pessoa,
	a.NomeRazao			as nome_pessoa,
	a.FisicaJuridica	as tipo_pessoa,
	a.cidade			as cidade_pessoa,

	--a.SEQPESSOAPRC,
	
		 CASE 
 	WHEN LEN(b.DigCGCCPF) = 1 THEN 
 		CAST(b.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(b.DigCGCCPF AS CHAR(2)) 
 	WHEN LEN(b.DigCGCCPF) = 2 THEN 
     	CAST(b.NroCGCCPF AS VARCHAR(12)) + CAST(b.DigCGCCPF AS CHAR(2)) ELSE
 		CAST(b.NroCGCCPF AS VARCHAR(12)) END 
 						as cnpj_conglo,
 	b.DigCGCCPF,
 	b.NroCGCCPF,
 						
	b.SeqPessoa			as seq_conglo,
	b.Status			as status_conglo,
	b.NomeRazao			as cliente_conglo,
	b.FisicaJuridica	as tipo_conglo,
	b.cidade			as cidade_conglo,
	Case 
		when a.SeqPessoa	=	b.SeqPessoa	 
			then 'S'
			else 'N' end 	
						as principal,
(
	select 
		AA.SeqCarteira
	from IVS_PES AA
	LEFT	JOIN IVS_CARTEIRA 	BB ON AA.SeqCarteira 	= BB.SeqCarteira
	INNER 	JOIN IVS_Depto		CC ON AA.SeqDepto		= CC.SeqDepto
	where 
		AA.SeqDepto IN (3)
		--and BB.Carteira not in('RAO_PEÇAS_01','RAO_PEÇAS_05')
		and AA.SeqPessoa=a.SeqPessoa	
) 			as Carteira_pessoa,									
(
	select 
		AA.SeqCarteira
	from IVS_PES AA
	LEFT	JOIN IVS_CARTEIRA 	BB ON AA.SeqCarteira 	= BB.SeqCarteira
	INNER 	JOIN IVS_Depto		CC ON AA.SeqDepto		= CC.SeqDepto
	where 
		AA.SeqDepto IN (3)
		--and BB.Carteira not in('RAO_PEÇAS_01','RAO_PEÇAS_05')
		and AA.SeqPessoa=b.SeqPessoa
) 			as Carteira_Conglomerado						
						
FROM 
			GE_Pessoa 	a
INNER JOIN	GE_Pessoa	b on a.SEQPESSOAPRC = b.SeqPessoa 
--WHERE b.SeqPessoa	= 70960

) Cadastro
) a
LEFT join 	IVS_CARTEIRA 	b on a.Carteira_pessoa 	= b.SeqCarteira
--LEFT join IVS_PES			c on b.SeqCarteira		= c.SeqCarteira and 
LEFT JOIN 	GE_Usuario		c ON b.SeqUsrResp		= c.SeqUsuario
LEFT JOIN 	GE_Empresa		D ON B.NroEmpresa		= D.NroEmpresa

where 
	a.Carteira_pessoa is not null 
/*
	and (select 
		case when 
			cc.seqpessoa is null  	then 'Inativo'
									else 'Ativo' end from IVS_PES cc where cc.seqcarteira=165 and cc.seqpessoa = a.seq_conglo)= 'Inativo'
*/
		--a.Carteira_pessoa is not NULL 
		
and 
/*
case when 
(select cc.seqpessoa  from IVS_PES cc where cc.seqcarteira=165 and cc.seqpessoa = a.seq_conglo)	IS NOT NULL 
	then  'Ativo'
	else  'Inativo' end	 
*/
a.prospeccao
		= 'Inativo'

--seq_conglo = 70960
--status_pessoa = 'A'

--B.Descricao	= 'RAO_PEÇAS_02'
--Carteira_pessoa = 108
--and principal='S'
/*
order by 
	--cnpj_pessoa
	a.cliente_conglo
*/	
	
UNION ALL 

select 
	a.departamento,
	a.Carteira_pessoa	as seq_carteira,
	Case 
		When a.Carteira_pessoa is null 
			then 'Sem Carteira'
			else 'Com Carteira' end 
						as carteira,						
	b.Carteira			as cod_carteira,
	b.Descricao			as nome_carteira,
	
	B.SeqUsrResp		as seq_cip,
	c.NomeReduzido		as nome_cip,
	
	D.Fantasia			as filial,
   	substring(D.Fantasia, 9, len(D.Fantasia)) 
						as empresa,
	
	a.status_pessoa,					
	a.cnpj_pessoa		as seq_pessoa,
	a.seq_pessoa		as cod_pessoa,
	a.nome_pessoa		as cliente,
	a.cidade_pessoa,
	a.tipo_pessoa		as tipo_pessoa,
	
	a.cnpj_conglo		as seq_conglo,
	a.seq_conglo		as cod_conglo,
	a.cidade_conglo,

	a.cliente_conglo	as grupo_conglo,
	--a.tipo_conglo,
	a.principal,
	a.prospeccao



From (

select 
	departamento,
	cnpj_pessoa,
	DigCGCCPF,
	NroCGCCPF,
	status_pessoa,
	seq_pessoa,
	nome_pessoa,
	cidade_pessoa,
	tipo_pessoa,
	cnpj_conglo,
	seq_conglo,
	status_conglo,
	cliente_conglo,
	cidade_conglo,
	tipo_conglo,
	
	case 
		when Carteira_pessoa is not null 
			then 'S'
			else principal end 
							as principal,
	--Carteira_pessoa,
	--Carteira_Conglomerado,
	case 	
		when Carteira_pessoa is null 
			then Carteira_Conglomerado
			else Carteira_pessoa end 
							as Carteira_pessoa,
							
case when 
(select COUNT(cc.seqpessoa)  from IVS_PES cc where cc.seqcarteira in (165,204) and (cc.seqpessoa = seq_conglo or cc.seqpessoa = seq_pessoa))	>0 
	then  'Ativo'
	else  'Inativo' end	as prospeccao
	
from (


--consulta banco de dados para cadastro / carteira do conglomerado
select
	'Prospecção'		as departamento,
	 CASE 
 	WHEN LEN(a.DigCGCCPF) = 1 THEN 
 		CAST(a.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(a.DigCGCCPF AS CHAR(2)) 
 	WHEN LEN(a.DigCGCCPF) = 2 THEN 
     	CAST(a.NroCGCCPF AS VARCHAR(12)) + CAST(a.DigCGCCPF AS CHAR(2)) ELSE
 		CAST(a.NroCGCCPF AS VARCHAR(12)) END 
 						as cnpj_pessoa,
	a.SeqPessoa			as seq_pessoa,
	a.Status			as status_pessoa,
	a.NomeRazao			as nome_pessoa,
	a.FisicaJuridica	as tipo_pessoa,
	a.cidade			as cidade_pessoa,

	--a.SEQPESSOAPRC,
	
		 CASE 
 	WHEN LEN(b.DigCGCCPF) = 1 THEN 
 		CAST(b.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(b.DigCGCCPF AS CHAR(2)) 
 	WHEN LEN(b.DigCGCCPF) = 2 THEN 
     	CAST(b.NroCGCCPF AS VARCHAR(12)) + CAST(b.DigCGCCPF AS CHAR(2)) ELSE
 		CAST(b.NroCGCCPF AS VARCHAR(12)) END 
 						as cnpj_conglo,
 	b.DigCGCCPF,
 	b.NroCGCCPF,
 						
	b.SeqPessoa			as seq_conglo,
	b.Status			as status_conglo,
	b.NomeRazao			as cliente_conglo,
	b.FisicaJuridica	as tipo_conglo,
	b.cidade			as cidade_conglo,
	Case 
		when a.SeqPessoa	=	b.SeqPessoa	 
			then 'S'
			else 'N' end 	
						as principal,
(
	select 
		AA.SeqCarteira
	from IVS_PES AA
	LEFT	JOIN IVS_CARTEIRA 	BB ON AA.SeqCarteira 	= BB.SeqCarteira
	INNER 	JOIN IVS_Depto		CC ON AA.SeqDepto		= CC.SeqDepto
	where 
		AA.SeqDepto IN (21)
		and BB.Carteira like ('PROSP_PEÇAS%')
		and AA.SeqPessoa=a.SeqPessoa	
) 			as Carteira_pessoa,									
(
	select 
		AA.SeqCarteira
	from IVS_PES AA
	LEFT	JOIN IVS_CARTEIRA 	BB ON AA.SeqCarteira 	= BB.SeqCarteira
	INNER 	JOIN IVS_Depto		CC ON AA.SeqDepto		= CC.SeqDepto
	where 
		AA.SeqDepto IN (21)
		and BB.Carteira like ('PROSP_PEÇAS%')
		and AA.SeqPessoa=b.SeqPessoa
) 			as Carteira_Conglomerado						
						
FROM 
			GE_Pessoa 	a
INNER JOIN	GE_Pessoa	b on a.SEQPESSOAPRC = b.SeqPessoa 
--WHERE b.SeqPessoa	= 70960

) Cadastro
) a
LEFT join 	IVS_CARTEIRA 	b on a.Carteira_pessoa 	= b.SeqCarteira
--LEFT join IVS_PES			c on b.SeqCarteira		= c.SeqCarteira and 
LEFT JOIN 	GE_Usuario		c ON b.SeqUsrResp		= c.SeqUsuario
LEFT JOIN 	GE_Empresa		D ON B.NroEmpresa		= D.NroEmpresa

where 
	a.Carteira_pessoa is not null 
/*
	and (select 
		case when 
			cc.seqpessoa is null  	then 'Inativo'
									else 'Ativo' end from IVS_PES cc where cc.seqcarteira=165 and cc.seqpessoa = a.seq_conglo)= 'Inativo'
*/
		--a.Carteira_pessoa is not NULL 
		
and 
/*
case when 
(select cc.seqpessoa  from IVS_PES cc where cc.seqcarteira=165 and cc.seqpessoa = a.seq_conglo)	IS NOT NULL 
	then  'Ativo'
	else  'Inativo' end	 
*/
a.prospeccao
		= 'Ativo'

--seq_conglo = 70960
--status_pessoa = 'A'

--B.Descricao	= 'RAO_PEÇAS_02'
--Carteira_pessoa = 108
--and principal='S'

/*		
order by 
	--cnpj_pessoa
	a.cliente_conglo
	;
*/;