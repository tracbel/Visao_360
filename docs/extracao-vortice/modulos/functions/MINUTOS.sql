/* ==============================================================
   Objeto ..........: dbo.MINUTOS
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2015-01-29 14:52:58
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION [dbo].[MINUTOS](    @Data1 DateTime, @Data2 DateTime  )  RETURNS INT  begin  	DECLARE @MINUTOS INT  	DECLARE @HORAS INT  	DECLARE @DIAS INT  	IF DATEPART(DAY, @DATA1) <> DATEPART(DAY, @DATA2)  	BEGIN  		IF DATEPART(DAY, @DATA1) < DATEPART(DAY, @DATA2)  		BEGIN  			SET @DIAS = DATEPART(DAY, @DATA2) - DATEPART(DAY, @DATA1)  		END  	END  	ELSE  		SET @DIAS = 0  	SET @MINUTOS = ((DATEPART(HOUR, @DATA2) - DATEPART(HOUR, @DATA1))*60) + ((DATEPART(minutE, @DATA2) - DATEPART(minutE, @DATA1)))  	RETURN convert(Varchar(15),@DIAS * 1440) + @MINUTOS   end