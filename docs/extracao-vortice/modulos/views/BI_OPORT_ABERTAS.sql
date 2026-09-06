/* ==============================================================
   Objeto ..........: dbo.BI_OPORT_ABERTAS
   Tipo ............: VIEW
   Criado em .......: 2023-03-01 11:45:42
   Modificado em ...: 2023-07-18 17:03:36
   Linhas ..........: 251
   Escreve em tabela: nao
   Tabelas referidas: ge_pessoa, iv_acao, iv_agenda, iv_historico, iv_procdado, iv_resultado
   Outras refs .....: fcnDiasUteis, fva_getdata
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE view [dbo].[BI_OPORT_ABERTAS] as
SELECT  XFI.Processo,
		XFI.Seqpessoa,
		XFI.Nomerazao,
		XFI.Acao,
		XFI.Resultado,
		XFI.Data,
		XFI.Conclusivo,
		XFI.AgendaAberta,
		XFI.Fase,
		XFI.DtaAgenda,
		XFI.Nrodias,
		XFI.Nrohoras,
		XFI.Nrominutos,
		XFI.Codacao,
		XFI.OrdemFase,
		XFI.TemEntrega,
		XFI.HorasIni,
		XFI.MinIni,
		XFI.Dias,
		XFI.HorasFim,
		XFI.MinFim,
		XFI.Minutos,
		XFI.MinutosDias,
		REPLICATE('0', 4 - LEN(CAST((XFI.Minutos / 60) AS VARCHAR(4)))) + CAST((XFI.Minutos / 60) AS VARCHAR(4)) -- Horas
		 + ':' +
		REPLICATE('0', 2 - LEN(CAST((XFI.Minutos % 60) AS VARCHAR(2)))) + CAST((XFI.Minutos % 60) AS VARCHAR(2)) -- Minutos
		AS TempoTotal,
		
		case when xfi.MesmoDia='N' 
		     then
				case when xfi.MinutosDias>1440 
					 then
				(REPLICATE('0', 4 - LEN(CAST((XFI.MinutosDias / 60) AS VARCHAR(4)))) + CAST((XFI.MinutosDias / 60) AS VARCHAR(4)) 
				/ 10)  
					 else 0 end 
		     else 0 end as TempoDias,
		--REPLICATE('0', 4 - LEN(CAST((XFI.Minutos / 60) AS VARCHAR(4)))) + CAST((XFI.Minutos / 60) AS VARCHAR(4)) 
  --      AS TempoHoras,
		case when xfi.MesmoDia='N' 
		     then XFI.Minutos / 60 
			 else xfi.HorasNoDia end AS TempoHoras,
	    case when xfi.MesmoDia='N'
---		     then REPLICATE('0', 3 - LEN(CAST((XFI.Minutos % 60) AS VARCHAR(3)))) + CAST((XFI.Minutos % 60) AS VARCHAR(3))
             then CAST((XFI.Minutos % 60) AS VARCHAR(4))
			 else xfi.MinutosNoDia end AS TempoMinutos
from (
select  ff.Processo,
		ff.Seqpessoa,
		ff.Nomerazao,
		ff.Acao,
		ff.Resultado,
		ff.Data,
		ff.Conclusivo,
		ff.AgendaAberta,
		ff.Fase,
		ff.DtaAgenda,
		ff.Nrodias,
		ff.Nrohoras,
		ff.Nrominutos,
		ff.Codacao,
		case ff.Fase when 'Comercial' then '1'
					 when 'Autorização Venda' then '2'
					 when 'Preparação Processo' then '3'
					 when 'Analise de Crédito' then '5'
					 when 'Estoque Produto' then '6'
					 when 'Preparação' then '7'
					 when 'Faturamento' then '8'
					 when 'Entrega' then '9'
					 when 'Preparação Usado' then '4' end 
					 + '-' + Fase 
					 as OrdemFase,
         (select DISTINCT 'S' from iv_historico where processo = ff.processo and acaogeradora = 525 ) as TemEntrega,
		 ff.HorasIni,
		 ff.MinIni,
		 ff.Dias,
		 ff.HorasFim,
		 ff.MinFim,
		 case when dbo.fcnDiasUteis (ff.dtaagenda, ff.data ) - 1<0 
		      then 0 
			  else dbo.fcnDiasUteis (ff.dtaagenda, ff.data ) - 1 end as UteisDias,
	     ( ( ff.HorasIni + ff.HorasFim ) * 60 ) +
		 ( ff.MinIni + ff.MinFim ) +
		 ( case when dbo.fcnDiasUteis (ff.dtaagenda, ff.data ) - 1<0 
		      then 0 
			  else dbo.fcnDiasUteis (ff.dtaagenda, ff.data ) - 1 end * 10 * 60 ) as MinutosDias,
		 case when dbo.fcnDiasUteis (ff.dtaagenda, ff.data ) - 1<0 
		      then 0 
			  else dbo.fcnDiasUteis (ff.dtaagenda, ff.data ) - 1 end as Uteis,
	     ( ( ff.HorasIni + ff.HorasFim ) * 60 ) +
		 ( ff.MinIni + ff.MinFim )  as Minutos,
		 ff.MesmoDia,
		 ff.HorasNoDia,
		 ff.MinutosNoDia
from (
select  xa.Processo, 
		xa.Seqpessoa,
		xa.Nomerazao,
		xa.Acao,
		xa.Resultado,
		xa.dtarealizacao as Data,
		case when xa.AgendaAberta='S' then Null Else xa.Conclusivo End as Conclusivo,
		xa.Agendaaberta,
		case xa.Codacao
		     when 527 then 'Comercial'
		     when 528 then 'Autorização Venda'
		     when 529 then 'Preparação Processo'
		     when 521 then 'Analise de Crédito'
		     when 522 then 'Estoque Produto'
		     when 523 then 'Preparação'
		     when 524 then 'Faturamento'
		     when 525 then 'Entrega'
		     when 546 then 'Preparação Usado'
			 end AS Fase,
		xa.codacao,
		xa.Dtaagenda,
		case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.Nrodias end as Nrodias,
	    case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.Nrohoras end as Nrohoras,
		case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.Nrominutos end as Nrominutos,
		case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.HorasIni end as HorasIni,
		case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.MinIni end as MinIni,
		case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.Dias end as Dias,
		case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.HorasFim end as HorasFim,
		case when xa.Dtaagenda=xa.DtaRealizacao then 0 else xa.MinFim end as MinFim,
		xa.HorasNoDia,
		xa.MinutosNoDia,
		xa.MesmoDia
from (
select	prd.Processo, 
		prd.SeqPessoa,
		pes.NomeRazao,
		aca.Descricao as Acao,
		res.descricao as Resultado,
		xhis.DtaRealizacao,
		case when res.CTRLCONCLUSAO=9 then 'S' else 'N' end as Conclusivo,
		(select distinct 'S' from iv_agenda where processo = prd.processo and realizada = 'N' and acao = xhis.AcaoGeradora )
		as AgendaAberta,
		case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end as Dtaagenda,
		xhis.AcaoGeradora as Codacao,
		DATEDIFF(day, case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
		              case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end) as Nrodias,
		DATEDIFF(hour, case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
		              case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end) as Nrohoras,
		DATEDIFF(MINUTE, case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
		              case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end) as Nrominutos,

---- novas colunas referente ao tempo

(DATEDIFF(second, case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
dbo.fva_getdata ( case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end  , '18:00' ) )/60)/60 as HorasIni,
( DATEDIFF(second, case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
dbo.fva_getdata ( case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end  , '18:00' ) )/60) % 60 MinIni,

CASE WHEN format((case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end), 'dd-MM-yyyy') =
          format((case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end), 'dd-MM-yyyy') THEN 0
     ELSE 
		DATEDIFF(day,case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
		case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end) + 1 - 2
     END as Dias,

(DATEDIFF(second,dbo.fva_getdata ( case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end, '08:00' ),
case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end)/60)/60 as HorasFim,
( DATEDIFF(second,dbo.fva_getdata ( case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end, '08:00' ), 
case when xhis.dtageracao is null then xhis.dtageracao2 else xhis.dtageracao end) / 60 ) % 60 MinFim,

case when CONVERT(VARCHAR(10), case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 103)  = 
          CONVERT(VARCHAR(10), xhis.DtaRealizacao, 103) 
	 then DATEDIFF(hour, case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
		              xhis.DtaRealizacao) end as HorasNoDia,
case when CONVERT(VARCHAR(10), case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 103)  = 
          CONVERT(VARCHAR(10), xhis.DtaRealizacao, 103) 
	 then DATEDIFF(Minute, case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 
		              xhis.DtaRealizacao) end as MinutosNoDia,
case when CONVERT(VARCHAR(10), case when xhis.dtaagenda is null then xhis.dtaagenda2 else xhis.dtaagenda end, 103)  = 
          CONVERT(VARCHAR(10), xhis.DtaRealizacao, 103) 
     then 'S' else 'N' end as MesmoDia
----

from iv_procdado prd
join ge_pessoa pes on pes.seqpessoa = prd.SeqPessoa
left join (select xxhh.processo, xxhh.acaogeradora, xxhh.resultado, xxhh.dtarealizacao , xxhh.seqhistorico, xxhh.agendaorigem,
           xxaa.dtageracao, xxaa.dtaagenda, xxbb.dtageracao as dtageracao2, xxbb.dtaagenda as dtaagenda2,
           row_number() over 
		   ( partition by xxhh.processo, xxhh.acaogeradora order by xxhh.dtarealizacao desc, xxhh.seqhistorico desc ) as rankhis
           from iv_historico xxhh
		   left join iv_agenda xxaa on xxaa.seqagenda = xxhh.agendaorigem
		   left join iv_agenda xxbb on xxbb.processo = xxhh.processo and xxbb.Acao = xxhh.AcaoGeradora
		   ) xhis on xhis.Processo = prd.Processo and xhis.rankhis=1
		         and xhis.AcaoGeradora in ( 527, 528, 529, 546, 521, 522, 523, 524, 525 )
left join iv_acao aca on aca.Acao = xhis.AcaoGeradora
left join iv_resultado res on res.resultado = xhis.resultado
where prd.codprocesso in ( 7, 37 )
---where prd.processo = 1160540
) xa

UNION

select	prd.Processo, 
		prd.SeqPessoa,
		pes.NomeRazao,
		aca.Descricao as Acao,
		Null as Resultado,
		Age.DtaAgenda,
		Null,
		'S',
		case age.acao
		     when 527 then 'Comercial'
		     when 528 then 'Autorização Venda'
		     when 529 then 'Preparação Processo'
		     when 521 then 'Analise de Crédito'
		     when 522 then 'Estoque Produto'
		     when 523 then 'Preparação'
		     when 524 then 'Faturamento'
		     when 525 then 'Entrega'
		     when 546 then 'Preparação Usado'
			 end AS Fase,
		age.acao,
		age.dtaagenda,
		DATEDIFF(day, age.dtaagenda, age.dtageracao) as Nrodias,
		DATEDIFF(hour, age.dtaagenda, age.dtageracao) as Nrohoras,
		DATEDIFF(minute, age.dtaagenda, age.dtageracao) as Nrominutos,
---- novas colunas referente ao tempo

(DATEDIFF(second, age.dtaagenda, 
dbo.fva_getdata ( age.dtaagenda  , '18:00' ) )/60)/60 as HorasIni,
( DATEDIFF(second, age.dtaagenda, 
dbo.fva_getdata ( age.dtaagenda  , '18:00' ) )/60) % 60 MinIni,
DATEDIFF(day,age.dtaagenda, 
age.dtageracao) as Dias,
(DATEDIFF(second,dbo.fva_getdata ( age.dtageracao, '08:00' ),
age.dtageracao)/60)/60 as HorasFim,
( DATEDIFF(second,dbo.fva_getdata ( age.dtageracao, '08:00' ), 
age.dtageracao) / 60 ) % 60 MinFim,
		null as HorasNoDia,
		null as MinutosNoDia,
		null as MesmoDia
----
from iv_procdado prd
join ge_pessoa pes on pes.seqpessoa = prd.SeqPessoa
join iv_agenda age on age.Processo = prd.Processo
                  and age.Realizada = 'N'
				  and not exists (select 1 from iv_historico 
				                  where processo = age.processo
				                    and AcaoGeradora = age.Acao )
join iv_acao aca on aca.Acao = age.Acao
                and aca.Acao in ( 527, 528, 529, 546, 521, 522, 523, 524, 525 )
where prd.codprocesso in ( 7, 37 )
---where prd.processo = 1160540
) ff
) XFI
