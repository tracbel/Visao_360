/* ==============================================================
   Objeto ..........: dbo.fva_FormatMoney
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

  CREATE FUNCTION dbo.fva_FormatMoney ( @pnNum decimal ) RETURNS Varchar(40) AS  	BEGIN 		 	Declare  @vsRet Varchar ( 40)  	 		Set @vsRet = FORMAT ( @pnNum, 'C2', 'pt')  		Return REPLACE( @vsRet, 'R$ ', '' )      END   