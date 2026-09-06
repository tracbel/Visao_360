/* ==============================================================
   Objeto ..........: dbo.toChar
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.toChar ( @pdData date , @psFormatFake varchar(20)) RETURNS Varchar(40) AS  	BEGIN 		Return convert(char,  @pdData, 103)       END    