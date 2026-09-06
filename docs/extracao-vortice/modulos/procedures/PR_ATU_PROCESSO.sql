/* ==============================================================
   Objeto ..........: dbo.PR_ATU_PROCESSO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-03-15 11:47:40
   Modificado em ...: 2023-03-15 11:47:40
   Linhas ..........: 77
   Escreve em tabela: SIM (INSERT)
   Tabelas referidas: IV_Processo
   Outras refs .....: IV_Processo_bkp20230314
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_ATU_PROCESSO] 
AS

	Declare 
		    @vnProcesso	numeric(8,0)

	Declare curPROC
	
	CURSOR
	FOR
		select prc.processo
		from iv_processo prc

begin
	OPEN curPROC
		FETCH NEXT FROM curPROC INTO 	@vnProcesso

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		
		insert into IV_Processo_bkp20230314 (Processo
			  ,Resumo
			  ,Descricao
			  ,UsuResponsavel
			  ,UsuInclusao
			  ,DtaInclusao
			  ,DtaAlteracao
			  ,UsuAlteracao
			  ,Realizado
			  ,DtaRealizacao
			  ,DtaFase
			  ,DtaStatus
			  ,Perspectiva
			  ,Fase
			  ,FaseOrdem
			  ,Status
			  ,StatusDesc
			  ,DtaPrevConclusao
			  ,Valor
			  ,Qtde
			  ,DtaPrevConcOrig
			  ,Prioridade) 
		SELECT [Processo]
			  ,[Resumo]
			  ,[Descricao]
			  ,[UsuResponsavel]
			  ,[UsuInclusao]
			  ,[DtaInclusao]
			  ,[DtaAlteracao]
			  ,[UsuAlteracao]
			  ,[Realizado]
			  ,[DtaRealizacao]
			  ,[DtaFase]
			  ,[DtaStatus]
			  ,[Perspectiva]
			  ,[Fase]
			  ,[FaseOrdem]
			  ,[Status]
			  ,[StatusDesc]
			  ,[DtaPrevConclusao]
			  ,[Valor]
			  ,[Qtde]
			  ,[DtaPrevConcOrig]
			  ,[Prioridade]
		  FROM [dbo].[IV_Processo]
          where Processo = @vnProcesso


		FETCH NEXT FROM curPROC INTO 	@vnProcesso
						
	END

	CLOSE curPROC
	DEALLOCATE curPROC

End
