/* ==============================================================
   Objeto ..........: dbo.fn_ContaHoras
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2015-02-27 12:46:53
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 39
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE function fn_ContaHoras (
	@dt_inicio smalldatetime,
	@dt_final smalldatetime
)

returns varchar(max)
as

begin
DECLARE @fimdesemana INT
DECLARE @i INT

SELECT @fimdesemana = 0, @i = 0 
WHILE (DATEDIFF(day, @dt_inicio, @dt_final) > @i) begin
SELECT @i = @i + 1
IF (DATEPART(WEEKDAY, DATEADD(day, @i, @dt_inicio)) in (1,7)) 
	BEGIN
	SELECT @fimdesemana = @fimdesemana + 1
	END
END

	declare @dia numeric(30),@hora numeric(30), @minuto numeric(30)

	SELECT @dia =    DATEDIFF(d, @dt_inicio, @dt_final) 
	SELECT @hora =    DATEDIFF(hour, @dt_inicio, @dt_final) 
	select @minuto =  DATEDIFF(MINUTE, @dt_inicio, @dt_final) 
	
	if @dia > 0 
	begin
		SET @fimdesemana = (@fimdesemana * 8)
		SET @hora = @hora - (@dia * 24) - @fimdesemana
		SET @dia = @dia * 10


	end
--	return @hora+@dia
return 
convert(varchar(max),(@hora +@dia)) +':'+convert(varchar(max),(@minuto %60))
end