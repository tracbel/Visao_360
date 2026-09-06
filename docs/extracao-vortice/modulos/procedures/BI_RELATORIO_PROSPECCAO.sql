/* ==============================================================
   Objeto ..........: dbo.BI_RELATORIO_PROSPECCAO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2021-01-18 12:25:12
   Modificado em ...: 2021-01-18 12:25:12
   Linhas ..........: 304
   Escreve em tabela: SIM (INSERT)
   Tabelas referidas: EXT_NFS, EXT_NFSItem, GE_Pessoa, IV_Acao, IV_CLASSERES, IV_HISTORICO, IV_PROCDADO, IV_RESCLASSE, IV_RESULTADO, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

/*
--Apagar procedure
DROP PROCEDURE dbo.BI_RELATORIO_PROSPECCAO;  
GO  

--Executar procedure 
Exec BI_RELATORIO_PROSPECCAO '2020-12-01', '2020-12-31',30;    

*/


------------------------------------------------------------------------------------------------------------------------------------------------------
CREATE Procedure [dbo].[BI_RELATORIO_PROSPECCAO] (@Inicio as Datetime, @Fim as Datetime, @Corte	as Int) 
as

------------------------------------------------------------------------------------------------------------------------------------------------------
/*			
12/11/2020

Projeto Relatório faturamento de peças
"
De:	Vanessa Kelly Alves Corneti/Administracao/Colorado Maquinas/Ribeirao Preto/Colorado
Para:	Felipe Augusto Violin/Servicos/Colorado Maquinas/Ribeirao Preto/Colorado@Colorado
Cc:	Lalesca Taborda/Colorado Maquinas/Ribeirao Preto/Colorado@Colorado
Data:	12/11/2020 14:33
Assunto:	Enc: RELATÓRIO FATURAMENTO - 

Felipe boa tarde!
Sem querer abusar da sua boa vontade mas ja abusando, no relatório de faturamentos de peças que vc desenvolveu não aparecem situações como abaixo.
Lalesca falou com o cliente, ele comprou, porém quando eu gero o relatório o cliente não está na carteira dela já que o cliente tornou-se ativo.
Podem acontecer diversos casos onde o contato aconteceu no mês anterior e o faturamento tb (tipo final de mês) e no do mês seguinte eu quando eu gero novamente os inativos esse cliente sai da carteira da Lalesca.

Consegue incluir este critério na seleção?
"
------------------------------------------------------------------------------------------------------------------------------------------------------
# Validações
--Tabela utiliada para validação do relatório gerado.
select 
	a.DtaEmissaoNF		as data_contato,
	a.DtaEmissaoNF		as data_contato_pos,
	a.DtaEmissaoNF 		as data_nf,
	a.NroEmpresa		as empresa,
	a.SeqPessoa,
	c.NomeRazao			as nome_cliente,
	a.NroNF				as nro_nf,
	sum(b.VlrLiqItem) 	as valor

FROM 	
			EXT_NFS a
left join 	EXT_NFSItem b on a.IdNFS 		= b.IdNFS
left join 	GE_Pessoa	c on a.SeqPessoa 	= c.SeqPessoa 
where 
	b.SeqDepto = 3
and a.SeqPessoa = 117
and	a.DtaEmissaoNF BETWEEN '2020-10-05 11:05:00.0' and  '2020-11-04 00:00:00.0'

GROUP by 
	a.DtaEmissaoNF,
	a.NroEmpresa,
	a.SeqPessoa,
	a.NroNF,
	c.NomeRazao
;

select * from IV_Resultado where RESULTADO in 	(1640,1641)  ;

*/

------------------------------------------------------------------------------------------------------------------------------------------------------
--Limpando tabelas temporarias se existirem.

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '#Tab01')
DROP TABLE #Tab01

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '#Tab02')
DROP TABLE #Tab02

IF  EXISTS (SELECT *FROM tempdb.sys.tables WHERE name = '#Tab03')
DROP TABLE #Tab03


/*
------------------------------------------------------------------------------------------------------------------------------------------------------
--Teste definiçao veriaveis
Declare @Inicio	as Date
Declare @Fim	as Date
Declare @Corte	as Int

Set @Inicio	= '2020-12-19'
Set @Fim	= '2021-12-18'
Set @Corte	= 30
*/
------------------------------------------------------------------------------------------------------------------------------------------------------
--Select para geração da relação de processos que vão ser buscados no relatório, base de processos que são prospectados para o departamento de peças.
Select 
		A.SEQPESSOA					as cod_pessoa, 
		A.PROCESSO					as nro_processo, 
		I.SEQPESSOAPRC				as cod_conglomerado,
		--H.PROCESSODNA				AS dna,
		F.CLASSE, 
		--A.AcaoGeradora			as cod_acao,
		--G.DescReduzida			as desc_acao,
		--D.RESULTADO 				as cod_resultado, 	-- COD_RESULTADO, 
		D.DESCRICAO 				as resultado, 		--RESULTADO, 
		--A.RESULTADOCMPL			as compl_resultado, 
		cast(A.DTAREALIZACAO as date)				
									as data_historico, 
		--(A.DTAREALIZACAO +30)		as data_historico_teste, 
		--DATEADD(DD, DATEDIFF(DD, 0, A.DTAREALIZACAO +30), 0) as data_historico_final,	
		cast(DATEADD(DD, DATEDIFF(DD, 0, A.DTAREALIZACAO + @Corte), 0)as date) as data_historico_final,	
		A.CodUsuario				as cip--, 
		--C.CAMPANHA
	Into #Tab01
	
FROM 
			IV_HISTORICO	A
LEFT JOIN 	IVS_PES			B	ON B.SEQPESSOA		= A.SEQPESSOA AND B.SEQDEPTO = 3
LEFT JOIN	IV_PROCDADO		C	ON C.PROCESSO		= A.PROCESSO
LEFT JOIN 	IV_RESULTADO 	D 	ON D.RESULTADO 		= A.RESULTADO
LEFT JOIN 	IV_RESCLASSE 	E 	ON E.RESULTADO 		= D.RESULTADO
LEFT JOIN 	IV_CLASSERES 	F 	ON F.SEQCLASSERES 	= E.SEQCLASSERES 
LEFT JOIN	IV_Acao			G 	ON G.Acao			= A.ACAOGERADORA
LEFT JOIN 	IV_ProcDado 	H	ON a.Processo 		= H.Processo 
LEFT JOIN	GE_Pessoa		I	ON A.SeqPessoa 		= I.SeqPessoa 

WHERE 
		YEAR (DTAREALIZACAO) >='2020'
	AND F.CLASSE in 		(
								'1-Cobertura Peças'--,
							--	'1-Cobertura Pneus'
							)
	AND A.SEQHISTORICO = 
		(	SELECT 
				MAX(SeqHistorico) 
			FROM 
						IV_HISTORICO AA
			LEFT JOIN 	IV_RESCLASSE AB ON AA.RESULTADO 	=	AB.RESULTADO
			LEFT JOIN 	IV_CLASSERES AC ON AB.SEQCLASSERES	=	AC.SEQCLASSERES
			WHERE 
				AC.CLASSE in	(	
								'1-Cobertura Peças'--,
								--'1-Cobertura Pneus'
								)
				AND AA.PROCESSO=A.PROCESSO
		)

AND D.RESULTADO in 	(1640,1641)  
--AND A.DTAREALIZACAO BETWEEN '2020-10-01' AND '2020-10-31'
AND cast(A.DTAREALIZACAO as date) BETWEEN @Inicio AND @Fim
ORDER BY 
cast(A.DTAREALIZACAO as date)


------------------------------------------------------------------------------------------------------------------------------------------------------
--Extração dos campos necessarios para o relatório
select 
	ROW_NUMBER() OVER (ORDER BY data_historico) row_num,
	cod_pessoa,
	cod_conglomerado,
	data_historico,
	data_historico_final

	Into #Tab02

from #Tab01
order by
	--cod_pessoa,
	data_historico

------------------------------------------------------------------------------------------------------------------------------------------------------	
--Excluindo tabela temporaria 1, que não sera mais utilizada
drop table #Tab01	

------------------------------------------------------------------------------------------------------------------------------------------------------
--Criação da tabela base para reber o resultado do loop
select 
	--row_number()		as linha,
	cast(a.DtaEmissaoNF as date) 		
						as data_contato,
	cast(a.DtaEmissaoNF as date)
						as data_contato_pos,
	cast(a.DtaEmissaoNF as date) 		
						as data_nf,
	a.NroEmpresa		as empresa,
	a.SeqPessoa,
	c.SEQPESSOAPRC		as cod_conglomerado,
	c.NomeRazao			as nome_cliente,
	a.NroNF				as nro_nf,
	--c.SeqCarteira		as seq_cateira,
	sum(b.VlrLiqItem) 	as valor--,
	--a.*

	Into #Tab03
FROM 	
			EXT_NFS a
left join 	EXT_NFSItem b on a.IdNFS 		= b.IdNFS
left join 	GE_Pessoa	c on a.SeqPessoa 	= c.SeqPessoa 

where 
	b.SeqDepto = 99999
and a.SeqPessoa in (select cod_pessoa from #Tab02 )	
and	a.DtaEmissaoNF  BETWEEN (SELECT min(data_historico) from #Tab02) and  (SELECT max(data_historico) from #Tab02)

GROUP by 
	a.DtaEmissaoNF,
	a.NroEmpresa,
	a.SeqPessoa,
	C.SEQPESSOAPRC,
	a.NroNF,
	c.NomeRazao--,
	--c.SeqCarteira
	
------------------------------------------------------------------------------------------------------------------------------------------------------
--Definição de variaveis para controle do loop

--variavel controle
DECLARE @I INT 
select @I = min(row_num) FROM  #Tab02

--variavel fim
DECLARE @T INT
select @T = max(row_num) FROM  #Tab02	
--select @T = 2

------------------------------------------------------------------------------------------------------------------------------------------------------
--inicio loop
  WHILE @I <=@T 
  BEGIN
	 
	 INSERT INTO #Tab03 --VALUES (	@I, @I,@I,@I,@I,@I)		  
	 Select
	 	--(SELECT (data_historico) from #Tab02 where row_num= @I )
	 	(SELECT cast((data_historico) as date) from #Tab02 where row_num= @I )
	 
	 						as data_contato,
	 	--(SELECT (data_historico_final) from #Tab02 where row_num= @I)
	 	(SELECT cast((data_historico_final) as date) from #Tab02 where row_num= @I)
	 						as data_contato_pos,
		--a.DtaEmissaoNF 		as data_nf,
		cast(a.DtaEmissaoNF as date)		
							as data_nf,
		a.NroEmpresa		as empresa,
		a.SeqPessoa,
		c.SEQPESSOAPRC,
		c.NomeRazao			as nome_cliente,
		a.NroNF				as nro_nf,
		sum(b.VlrLiqItem) 	as valor

	FROM 	
				EXT_NFS a
	left join 	EXT_NFSItem b on a.IdNFS 		= b.IdNFS
	left join 	GE_Pessoa	c on a.SeqPessoa 	= c.SeqPessoa 
	
	where 
		b.SeqDepto = 3
	--and a.SeqPessoa = (select cod_pessoa from #Tab02 where row_num= @I)	
	and c.SEQPESSOAPRC = (select cod_conglomerado from #Tab02 where row_num= @I)	
	and	a.DtaEmissaoNF 
	BETWEEN 
			(SELECT (data_historico) from #Tab02 where row_num= @I ) 
	and		(SELECT (data_historico_final) from #Tab02 where row_num= @I)
	
	GROUP by 
		a.DtaEmissaoNF,
		a.NroEmpresa,
		a.SeqPessoa,
		c.SEQPESSOAPRC,
		a.NroNF,
		c.NomeRazao
	  
	SET @I += 1								
  END


------------------------------------------------------------------------------------------------------------------------------------------------------
--Retornando resultados calculos  
--  select * From #Tab03

SELECT  
	CONVERT(DATETIME, data_contato,103)		as Dt_contato,
	--cast(data_contato as date) 			as Dt_contato,
	CONVERT(DATETIME, data_contato_pos,103)	as Dt_prazo,
	--cast(data_contato_pos as date) 		as Dt_prazo,
	CONVERT(DATETIME, data_nf,103)			as Dt_nf,
	--cast(data_nf as date) 				as Dt_nf,
	empresa,
	SeqPessoa,
	cod_conglomerado,
	nome_cliente,
	nro_nf,
	valor

From #Tab03 
order by 
empresa,
SeqPessoa,
nro_nf

------------------------------------------------------------------------------------------------------------------------------------------------------
--Excluir tabelas temporarias
drop table #Tab02
drop table #Tab03

