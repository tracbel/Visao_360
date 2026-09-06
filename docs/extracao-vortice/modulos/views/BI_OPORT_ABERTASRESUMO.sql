/* ==============================================================
   Objeto ..........: dbo.BI_OPORT_ABERTASRESUMO
   Tipo ............: VIEW
   Criado em .......: 2023-03-08 10:14:19
   Modificado em ...: 2023-07-18 17:03:54
   Linhas ..........: 442
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Outras refs .....: BI_OPORT_ABERTAS
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE view [dbo].[BI_OPORT_ABERTASRESUMO] AS
SELECT xa.Processo,
      xa.Seqpessoa,
      max( isnull(xa.Dias1, 0) )	as Dias1,
	  max( isnull(xa.Horas1, 0) )	as Horas1,
	  max( isnull(xa.Min1, 0) )		as Min1,
	  max( isnull(xa.Dias2, 0) )	as Dias2,
	  max( isnull(xa.Horas2, 0) )	as Horas2,
	  max( isnull(xa.Min2, 0) )		as Min2,
	  max( isnull(xa.Dias3, 0) )	as Dias3,
	  max( isnull(xa.Horas3, 0) )	as Horas3,
	  max( isnull(xa.Min3, 0) )		as Min3,
	  max( isnull(xa.Dias4, 0) )	as Dias4,
	  max( isnull(xa.Horas4, 0) )	as Horas4,
	  max( isnull(xa.Min4, 0) )		as Min4,
	  max( isnull(xa.Dias5, 0) )	as Dias5,
	  max( isnull(xa.Horas5, 0) )	as Horas5,
	  max( isnull(xa.Min5, 0) )		as Min5,
	  max( isnull(xa.Dias6, 0) )	as Dias6,
	  max( isnull(xa.Horas6, 0) )	as Horas6,
	  max( isnull(xa.Min6, 0) )		as Min6,
	  max( isnull(xa.Dias7, 0) )	as Dias7,
	  max( isnull(xa.Horas7, 0) )	as Horas7,
	  max( isnull(xa.Min7, 0) )		as Min7,
	  max( isnull(xa.Dias8, 0) )	as Dias8,
	  max( isnull(xa.Horas8, 0) )	as Horas8,
	  max( isnull(xa.Min8, 0) )		as Min8,
	  max( isnull(xa.Dias9, 0) )	as Dias9,
	  max( isnull(xa.Horas9, 0) )	as Horas9,
	  max( isnull(xa.Min9, 0 ) )	as Min9,
	  max( isnull(AgdAberta9,'N') ) as AgdAberta9,
	  max( isnull(AgdAberta8,'N') ) as AgdAberta8,
	  max( isnull(AgdAberta7,'N') ) as AgdAberta7,
	  max( isnull(AgdAberta6,'N') ) as AgdAberta6,
	  max( isnull(AgdAberta5,'N') ) as AgdAberta5,
	  max( isnull(AgdAberta4,'N') ) as AgdAberta4,
	  max( isnull(AgdAberta3,'N') ) as AgdAberta3,
	  max( isnull(AgdAberta2,'N') ) as AgdAberta2,
	  max( isnull(AgdAberta1,'N') ) as AgdAberta1,
	  max( isnull(TemEntrega,'N') ) as TemEntrega
FROM (
SELECT Processo
      ,Seqpessoa
      , (TempoDias) as Dias1
	  , (TempoHoras) as Horas1
	  , (  TempoMinutos ) as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , isnull(AgendaAberta,'N') as AgdAberta1
	  , Null as AgdAberta2
	  , Null as AgdAberta3
	  , Null as AgdAberta4
	  , Null as AgdAberta5
	  , Null as AgdAberta6
	  , Null as AgdAberta7
	  , Null as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '1-Comercial'

UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
      , (TempoDias) as Dias2
	  , (TempoHoras) as Horas2
	  , (  TempoMinutos ) as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , Null as AgdAberta1
	  , isnull(AgendaAberta,'N') as AgdAberta2
	  , Null as AgdAberta3
	  , Null as AgdAberta4
	  , Null as AgdAberta5
	  , Null as AgdAberta6
	  , Null as AgdAberta7
	  , Null as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '2-Autorização Venda'

UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
      , (TempoDias) as Dias3
	  , (TempoHoras) as Horas3
	  , (  TempoMinutos ) as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , Null as AgdAberta1
	  , Null as AgdAberta2
	  , isnull(AgendaAberta,'N') as AgdAberta3
	  , Null as AgdAberta4
	  , Null as AgdAberta5
	  , Null as AgdAberta6
	  , Null as AgdAberta7
	  , Null as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '3-Preparação Processo'

UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
      , (TempoDias) as Dias4
	  , (TempoHoras) as Horas4
	  , (  TempoMinutos ) as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , Null as AgdAberta1
	  , Null as AgdAberta2
	  , Null as AgdAberta3
	  , isnull(AgendaAberta,'N') as AgdAberta4
	  , Null as AgdAberta5
	  , Null as AgdAberta6
	  , Null as AgdAberta7
	  , Null as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '4-Preparação Usado'
UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
      , (TempoDias) as Dias5
	  , (TempoHoras) as Horas5
	  , (  TempoMinutos ) as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , Null as AgdAberta1
	  , Null as AgdAberta2
	  , Null as AgdAberta3
	  , Null as AgdAberta4
	  , isnull(AgendaAberta,'N') as AgdAberta5
	  , Null as AgdAberta6
	  , Null as AgdAberta7
	  , Null as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '5-Analise de Crédito'
UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
      , (TempoDias) as Dias6
	  , (TempoHoras) as Horas6
	  , (  TempoMinutos ) as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , Null as AgdAberta1
	  , Null as AgdAberta2
	  , Null as AgdAberta3
	  , Null as AgdAberta4
	  , Null as AgdAberta5
	  , isnull(AgendaAberta,'N') as AgdAberta6
	  , Null as AgdAberta7
	  , Null as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '6-Estoque Produto'
UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
      , (TempoDias) as Dias7
	  , (TempoHoras) as Horas7
	  , (  TempoMinutos ) as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , Null as AgdAberta1
	  , Null as AgdAberta2
	  , Null as AgdAberta3
	  , Null as AgdAberta4
	  , Null as AgdAberta5
	  , Null as AgdAberta6
	  , isnull(AgendaAberta,'N') as AgdAberta7
	  , Null as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '7-Preparação'
UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
      , (TempoDias) as Dias8
	  , (TempoHoras) as Horas8
	  , (  TempoMinutos ) as Min8
	  , Null as Dias9
	  , Null as Horas9
	  , Null as Min9
	  , Null as AgdAberta1
	  , Null as AgdAberta2
	  , Null as AgdAberta3
	  , Null as AgdAberta4
	  , Null as AgdAberta5
	  , Null as AgdAberta6
	  , Null as AgdAberta7
	  , isnull(AgendaAberta,'N') as AgdAberta8
	  , Null as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '8-Faturamento'
UNION ALL

SELECT Processo
      ,Seqpessoa
	  , Null as Dias1
	  , Null as Horas1
	  , Null as Min1
	  , Null as Dias2
	  , Null as Horas2
	  , Null as Min2
	  , Null as Dias3
	  , Null as Horas3
	  , Null as Min3
	  , Null as Dias4
	  , Null as Horas4
	  , Null as Min4
	  , Null as Dias5
	  , Null as Horas5
	  , Null as Min5
	  , Null as Dias6
	  , Null as Horas6
	  , Null as Min6
	  , Null as Dias7
	  , Null as Horas7
	  , Null as Min7
	  , Null as Dias8
	  , Null as Horas8
	  , Null as Min8
      , (TempoDias) as Dias9
	  , (TempoHoras) as Horas9
	  , (  TempoMinutos ) as Min9
	  , Null as AgdAberta1
	  , Null as AgdAberta2
	  , Null as AgdAberta3
	  , Null as AgdAberta4
	  , Null as AgdAberta5
	  , Null as AgdAberta6
	  , Null as AgdAberta7
	  , Null as AgdAberta8
	  , isnull(AgendaAberta,'N') as AgdAberta9
	  , TemEntrega
FROM BI_OPORT_ABERTAS 
where 1 = 1
  and OrdemFase = '9-Entrega'

) XA
group by xa.Processo, xa.seqpessoa
