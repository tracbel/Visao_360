/* ==============================================================
   Objeto ..........: dbo.f_GetColumnData
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2011-12-19 11:48:36
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 46
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION dbo.f_GetColumnData (@nome_coluna VARCHAR(200), @coluna Varchar(1000), @Dado Varchar(2000), @Separador Char (1))
    RETURNS Varchar(1000) AS
BEGIN 
   DECLARE @strColuna VARCHAR(1000) 
   DECLARE @strNomeColuna Varchar(200)
   DECLARE @i INT 
   DECLARE @nQtdeVirg INT
   Declare @nTam INt
   Declare @nTamCol INt
   SET @strNomeColuna = LOWER(@nome_coluna) 
   SET @strColuna = LOWER(@Coluna) + @Separador
   Set @dado = @dado + @Separador
   Set @nTam = Len (@strColuna)
   Set @nTamCol = Len(@strNomeColuna)
   Set @i = CHARINDEX(@strNomeColuna,@strColuna,1) 
   if  @i = 0
       return ''
 
   Set @nQtdeVirg = 0
   WHILE @i > 0 
       BEGIN
          If  substring(@strColuna ,@i, 1 ) = @Separador
              begin
                 Set @nQtdeVirg = @nQtdeVirg + 1
              end
          Set @i = @i - 1
       END
    Set @i = 0
    WHILE @nQtdeVirg > 0
        BEGIN
          Set @i = @i + 1
 	  If  substring(@Dado ,@i, 1 ) = @Separador  
 	      begin
                 Set @nQtdeVirg = @nQtdeVirg - 1
              end 
        END
    Set @nTam = @i
    Set @nTam = @nTam + 1
    WHILE substring(@Dado ,@nTam, 1 ) != @Separador  
        BEGIN
          Set @nTam = @nTam + 1
        END 
    Set @i = @i+1
    Return replace (substring(@Dado ,@i, @nTam - @i ), char(39), '') 
END
