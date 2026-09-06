/* ==============================================================
   Objeto ..........: dbo.fIV_GetStatusNaData
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2014-10-06 08:40:44
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_PROCSTATMONIT
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create function [dbo].[fIV_GetStatusNaData](@pnProcesso numeric(18,0),@pdDta datetime)   returns varchar(30)   as  begin  declare @vsStatus varchar(30)   declare @vsData datetime  declare C01 cursor for   SELECT PM.DTAMONIT, PM.STATUS    FROM IV_PROCSTATMONIT PM   WHERE PM.PROCESSO = @pnProcesso   AND PM.DTAMONIT <= @pdDta   ORDER BY 1     begin   Set @vsStatus = NULL  OPEN C01     FETCH NEXT FROM C01 into @vsData, @vsStatus  WHILE @@FETCH_STATUS = 0   BEGIN   Set @vsStatus = @vsStatus   FETCH NEXT FROM C01 into @vsData, @vsStatus    END    CLOSE C01   DEALLOCATE C01    RETURN @vsStatus  END  end  