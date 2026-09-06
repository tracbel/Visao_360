/* ==============================================================
   Objeto ..........: dbo.X_P_EXE_INTEGRACOES_TOTVS_CRM
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2023-10-13 09:04:44
   Modificado em ...: 2023-10-13 15:39:47
   Linhas ..........: 16
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Outras refs .....: X_P_IMP_CRM_NF_POS_VENDA, X_P_IMP_CRM_TITULO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE   PROCEDURE  [X_P_EXE_INTEGRACOES_TOTVS_CRM]
AS 

BEGIN
	--##########################################################################################################
	--1º PASSO - Executar Procedure TITULOS TOTVS - CRM:
	EXEC X_P_IMP_CRM_TITULO

	--##########################################################################################################
	--2º PASSO - Executar Procedure FATURAMENTO MAQUINAS E POS VENDAS TOTVS - CRM:
	EXEC X_P_IMP_CRM_NF_POS_VENDA
	
	--##########################################################################################################
END
----------------------------------------------------------------------------------------------------------------------------
