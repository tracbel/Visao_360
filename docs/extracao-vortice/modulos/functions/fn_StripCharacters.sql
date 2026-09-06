/* ==============================================================
   Objeto ..........: dbo.fn_StripCharacters
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2019-06-07 16:17:47
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 16
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE FUNCTION [dbo].[fn_StripCharacters]
(
    @String NVARCHAR(MAX), 
    @MatchExpression VARCHAR(255)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    SET @MatchExpression =  '%['+@MatchExpression+']%'

    WHILE PatIndex(@MatchExpression, @String) > 0
        SET @String = Stuff(@String, PatIndex(@MatchExpression, @String), 1, '')

    RETURN @String

END