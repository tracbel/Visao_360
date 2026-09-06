/* ==============================================================
   Objeto ..........: dbo.fva_DateTimeFromParts
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:39
   Modificado em ...: 2025-02-17 17:35:39
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fva_DateTimeFromParts (     @Year INT,     @Month INT,     @Day INT,     @Hour INT,     @Minute INT,     @Second INT,     @Millisecond INT ) RETURNS DATETIME AS BEGIN     DECLARE @Result DATETIME     SET @Result = DATEADD(YEAR, @Year - 1900, 0)     SET @Result = DATEADD(MONTH, @Month - 1, @Result)     SET @Result = DATEADD(DAY, @Day - 1, @Result)     SET @Result = DATEADD(HOUR, @Hour, @Result)     SET @Result = DATEADD(MINUTE, @Minute, @Result)     SET @Result = DATEADD(SECOND, @Second, @Result)     SET @Result = DATEADD(MILLISECOND, @Millisecond, @Result)     RETURN @Result END   