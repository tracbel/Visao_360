/* ==============================================================
   Objeto ..........: dbo.fva_IntervaloDataB
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2012-11-26 11:45:24
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION dbo.fva_IntervaloDataB (@pdDataBase datetime, @pdDataInicial datetime, @pdDataFinal datetime)       RETURNS Int AS    BEGIN      DECLARE @vnTemp Int     Set @vnTemp = 0       IF  @pdDataBase  >= @pdDataInicial and @pdDataBase <= @pdDataFinal          Set @vnTemp = 1  Return (@vnTemp)  END