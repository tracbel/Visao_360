/* ==============================================================
   Objeto ..........: dbo.fcnDiasUteis
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2015-02-27 16:25:09
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 40
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION fcnDiasUteis (@DataInicial datetime, @DataFinal datetime)
RETURNS int
AS
BEGIN

-- Cria variaveis
DECLARE @Retorno int, @DiasAUX int, @DataAUX datetime

-- Seta primeiro dia da semana em domingo
--SET DATEFIRST 7

-- Seta dias úteis como 0
SET @Retorno = DateDiff(d, @DataInicial, @DataFinal)

-- Seta data auxiliar para cálculos
SET @DataAUX = @DataFinal

-- Contabiliza dias úteis
WHILE @DataInicial <= @DataAUX
BEGIN
-- Se for final de semana, desconsidera do total
IF DATEPART(dw, @DataAUX) in (1, 7)
BEGIN
SET @Retorno = @Retorno - 1
END

-- Subtrai um da data auxiliar
SET @DataAUX = @DataAUX - 1
END

-- Corrige caso data inicial seja um FDS
IF DATEPART(dw, @DataInicial) in (1, 7)
BEGIN
SET @Retorno = @Retorno + 1
END


RETURN(@RETORNO)

END 