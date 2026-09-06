/* ==============================================================
   Objeto ..........: dbo.fva_RemoveCRLF
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

  CREATE FUNCTION dbo.fva_RemoveCRLF ( @psString Varchar(8000) ) RETURNS Varchar (8000) AS  	BEGIN  		Return REPLACE(REPLACE(@psString , CHAR(13), ''), CHAR(10), ' ')     END   