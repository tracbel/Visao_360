/* ==============================================================
   Objeto ..........: dbo.fIV_GetStatusOrdNaData
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2014-10-06 08:38:27
   Modificado em ...: 2022-03-06 17:11:25
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_PROCDADO, IV_PROCST, IV_PROCSTATMONIT
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create function [dbo].[fIV_GetStatusOrdNaData](@pnProcesso numeric(18,0),@pdDta datetime)   returns int  as  begin  declare @vnStatusOrd int  declare @vsData datetime  declare C01 cursor for   SELECT PM.DTAMONIT, PST.STATUSORDEM    FROM IV_PROCSTATMONIT PM   JOIN IV_PROCDADO PDD ON PDD.PROCESSO = PM.PROCESSO   JOIN IV_PROCST PST ON PST.CODPROCESSO = PDD.CODPROCESSO AND PST.STATUS = PM.STATUS    WHERE PM.PROCESSO = @pnProcesso   AND PM.DTAMONIT <= @pdDta   ORDER BY 1     begin   OPEN C01     FETCH NEXT FROM C01 into @vsData, @vnStatusOrd  WHILE @@FETCH_STATUS = 0   BEGIN   Set @vnStatusOrd = @vnStatusOrd   FETCH NEXT FROM C01 into @vsData, @vnStatusOrd    END    CLOSE C01   DEALLOCATE C01   RETURN @vnStatusOrd  END  end  