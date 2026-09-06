/* ==============================================================
   Objeto ..........: dbo.fva_QryDiaUtil
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE FUNCTION dbo.fva_QryDiaUtil  ( @pdData Date, @pdSabadoUtil int )     RETURNS INT AS BEGIN 	DECLARE @vsT VARCHAR(10) 	 	BEGIN 		SET @vsT = CONVERT(VARCHAR(5), @pdData, 103) 		IF  datepart(weekday,@pdData)  = 1 			BEGIN 				RETURN 0 			END 	    IF datepart(weekday,@pdData) = 7 And (Not @pdSabadoUtil > 0) 			BEGIN 				RETURN 0 			END 		IF @vsT = '01/01'  			BEGIN 				RETURN 0 			END 		IF @vsT = '25/12'  			BEGIN 				RETURN 0 			END 		IF @vsT = '01/05'  			BEGIN 				RETURN 0  			END 		IF @vsT = '07/09'  			BEGIN 				RETURN 0 			END 		IF @vsT = '12/10'  			BEGIN 				RETURN 0 			END				 		IF @vsT = '02/11'  			BEGIN 				RETURN 0 			END				 	END 	 	RETURN 1 END	  