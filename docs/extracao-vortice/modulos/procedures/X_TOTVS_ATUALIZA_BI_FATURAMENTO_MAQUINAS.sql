/* ==============================================================
   Objeto ..........: dbo.X_TOTVS_ATUALIZA_BI_FATURAMENTO_MAQUINAS
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2022-12-09 17:25:21
   Modificado em ...: 2023-11-29 17:42:23
   Linhas ..........: 120
   Escreve em tabela: SIM (INSERT, DELETE)
   Alvos de escrita : X_TOTVS_BI_FATURAMENTO_MAQUINAS
   Tabelas referidas: X_TOTVS_BI_FATURAMENTO_MAQUINAS
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

--PROCEDURE PARA ATUALIZAR BASE

CREATE   PROCEDURE  [X_TOTVS_ATUALIZA_BI_FATURAMENTO_MAQUINAS]
AS 
IF  EXISTS (SELECT *FROM sys.tables WHERE name = 'X_TOTVS_BI_FATURAMENTO_MAQUINAS')
BEGIN
	--variavel controle
	DECLARE @V_filtro DATE 
	DECLARE @V_i int
	
	--Parametro de tempo para retroagir a base
	Set @V_i	= -1
	
	SELECT @V_filtro = DATEADD(month, @V_i, max(data_emissao_nf)) from X_TOTVS_BI_FATURAMENTO_MAQUINAS
	
	--LIMPEZA REGISTROS 
	DELETE from X_TOTVS_BI_FATURAMENTO_MAQUINAS WHERE data_emissao_nf > @V_filtro 
	
	--BUSCA NOVOS REGISTROS - ERP TOTVS
	INSERT INTO CRM.dbo.X_TOTVS_BI_FATURAMENTO_MAQUINAS
	SELECT 
		origem,
		origem_fonte,
		tipo_nf,
		filial,
		nro_nf,
		cast(cast(seq_pessoa as bigint) as varchar(800)) as seq_pessoa,
		cliente,
		nome_operacao,
		data_emissao_nf,
		cliente_fj,
		REPLACE(cod_produto,'|','') AS cod_produto,
		nome_produto,
		vlr_liquido_item
		
		--INTO X_TOTVS_BI_FATURAMENTO_MAQUINAS  
		
	FROM
		
		(		
						SELECT 
							origem,
							origem_fonte,
							tipo_nf,
							filial, 
							nro_nf, 
							nome_operacao, 
							data_emissao_nf, 
							cliente_fj, 
							seq_pessoa, 
							cliente, 
							cod_produto, 
							nome_produto,
							vlr_liquido_item
							--,*
							
						from openquery ( totvs, 'select * from V_COL_FATURAMENTO ')
						where 
							origem_fonte in ('01_MAQUINAS','04_CLIENTES_ESTRATEGICOS')	
						AND data_emissao_nf > @V_filtro
		) MAIN
	WHERE data_emissao_nf > @V_filtro

END
ELSE 
BEGIN 
SELECT 
	origem,
	origem_fonte,
	tipo_nf,
	filial,
	nro_nf,
	cast(cast(seq_pessoa as bigint) as varchar(800)) as seq_pessoa,
	cliente,
	nome_operacao,
	data_emissao_nf,
	cliente_fj,
	REPLACE(cod_produto,'|','') AS cod_produto,
	nome_produto,
	vlr_liquido_item
	
	INTO X_TOTVS_BI_FATURAMENTO_MAQUINAS  
	
FROM
	
	(		
					SELECT 
						origem,
						origem_fonte,
						tipo_nf,
						filial, 
						nro_nf, 
						nome_operacao, 
						data_emissao_nf, 
						cliente_fj, 
						seq_pessoa, 
						cliente, 
						cod_produto, 
						nome_produto,
						vlr_liquido_item
						--,*
						
					from openquery ( totvs, 'select * from V_COL_FATURAMENTO ')
					where 
						origem_fonte in ('01_MAQUINAS','04_CLIENTES_ESTRATEGICOS')		
	) MAIN
	
 CREATE NONCLUSTERED INDEX X_TOTVS_BI_FATURAMENTO_MAQUINAS_cod_produto_IDX ON dbo.X_TOTVS_BI_FATURAMENTO_MAQUINAS (  cod_produto ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] 
 CREATE NONCLUSTERED INDEX X_TOTVS_BI_FATURAMENTO_MAQUINAS_filial_IDX ON dbo.X_TOTVS_BI_FATURAMENTO_MAQUINAS (  filial ASC  , nro_nf ASC  , seq_pessoa ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] 
 CREATE NONCLUSTERED INDEX X_TOTVS_BI_FATURAMENTO_MAQUINAS_seq_pessoa_IDX ON dbo.X_TOTVS_BI_FATURAMENTO_MAQUINAS (  seq_pessoa ASC  , cod_produto ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] 	
	
END

;