/* ==============================================================
   Objeto ..........: dbo.fva_SoNro
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE  FUNCTION dbo.fva_SoNro (@PALAVRA VARCHAR (1000)) RETURNS VARCHAR (1000) AS  BEGIN DECLARE  @RESULTADO VARCHAR (1000),   @LETRA VARCHAR(1),  @QTD_PALAVRA INTEGER,  @CONT INTEGER SET @CONT = 0  SET @QTD_PALAVRA = LEN(@PALAVRA) SET @RESULTADO = '' WHILE @CONT < @QTD_PALAVRA   BEGIN    SET @CONT = @CONT + 1    SET @LETRA = SUBSTRING(@PALAVRA,@CONT,1)   IF @LETRA  >= '0' AND @LETRA <= '9'     BEGIN     SET @RESULTADO =  @RESULTADO +  @LETRA     END  END  RETURN @RESULTADO  END  