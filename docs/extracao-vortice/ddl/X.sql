/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo X
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_T_IMP_CRM_TITULO
   Criada em ..: 2023-03-17
   Alterada em : 2023-03-20
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_T_IMP_CRM_TITULO] (
    [Origem] varchar(8) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NULL,
    [PessoaLinkOrigem] varchar(8) COLLATE Latin1_General_CI_AS NOT NULL,
    [Pessoalink] varchar(13) COLLATE Latin1_General_CI_AS NOT NULL,
    [SeqPessoa] int NOT NULL,
    [NroCNPJ] int NULL,
    [DigCNPJ] int NULL,
    [Departamento] int NULL,
    [Especie] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroTitulo] varchar(8000) COLLATE Latin1_General_CI_AS NULL,
    [NroDocto] int NULL,
    [TipoCobranca] varchar(5) COLLATE Latin1_General_CI_AS NOT NULL,
    [IndAtivo] int NOT NULL,
    [IndQuitado] int NULL,
    [IndCobrJuridica] int NULL,
    [Status] varchar(1) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroBanco] int NULL,
    [LocalCobranca] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroTituloBanco] varchar(6) COLLATE Latin1_General_CI_AS NOT NULL,
    [DtaEmissao] datetime2(7) NULL,
    [DtaVenctoOrig] datetime2(7) NULL,
    [DtaVencto] datetime2(7) NULL,
    [VlrOriginal] float NOT NULL,
    [VlrAcrescimo] float NOT NULL,
    [VlrAbatimento] int NOT NULL,
    [VlrPago] float NOT NULL,
    [VlrMov] int NULL,
    [Movimento] int NULL,
    [DtaUltPgto] datetime2(7) NULL,
    [DtaQuitacao] datetime2(7) NULL,
    [DtaUltAlteracao] datetime2(7) NULL,
    [UsuAlteracao] int NULL,
    [LinkNro] int NULL,
    [LinkStr] varchar(8000) COLLATE Latin1_General_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdPessoa] int NULL,
    [CHAVESTR1] int NULL,
    [DATA_INCLUSAO] varchar(30) COLLATE Latin1_General_CI_AS NULL,
    [DATA_ALTERACAO] datetime NULL,
    [X_INTEGRADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [X_DATA_INTEGRACAO] datetime NULL
);
GO
CREATE NONCLUSTERED INDEX [X_T_IMP_CRM_TITULO_Obs_IDX] ON [dbo].[X_T_IMP_CRM_TITULO] ([Obs]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_T_IMP_CRM_TITULO_bkp_11_04
   Criada em ..: 2023-04-11
   Alterada em : 2023-04-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_T_IMP_CRM_TITULO_bkp_11_04] (
    [Origem] varchar(8) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NULL,
    [PessoaLinkOrigem] varchar(8) COLLATE Latin1_General_CI_AS NOT NULL,
    [Pessoalink] varchar(13) COLLATE Latin1_General_CI_AS NOT NULL,
    [SeqPessoa] int NOT NULL,
    [NroCNPJ] int NULL,
    [DigCNPJ] int NULL,
    [Departamento] int NULL,
    [Especie] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroTitulo] varchar(8000) COLLATE Latin1_General_CI_AS NULL,
    [NroDocto] int NULL,
    [TipoCobranca] varchar(5) COLLATE Latin1_General_CI_AS NOT NULL,
    [IndAtivo] int NOT NULL,
    [IndQuitado] int NULL,
    [IndCobrJuridica] int NULL,
    [Status] varchar(1) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroBanco] int NULL,
    [LocalCobranca] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [NroTituloBanco] varchar(6) COLLATE Latin1_General_CI_AS NOT NULL,
    [DtaEmissao] datetime2(7) NULL,
    [DtaVenctoOrig] datetime2(7) NULL,
    [DtaVencto] datetime2(7) NULL,
    [VlrOriginal] float NOT NULL,
    [VlrAcrescimo] float NOT NULL,
    [VlrAbatimento] int NOT NULL,
    [VlrPago] float NOT NULL,
    [VlrMov] int NULL,
    [Movimento] int NULL,
    [DtaUltPgto] datetime2(7) NULL,
    [DtaQuitacao] datetime2(7) NULL,
    [DtaUltAlteracao] datetime2(7) NULL,
    [UsuAlteracao] int NULL,
    [LinkNro] int NULL,
    [LinkStr] varchar(8000) COLLATE Latin1_General_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdPessoa] int NULL,
    [CHAVESTR1] int NULL,
    [DATA_INCLUSAO] varchar(30) COLLATE Latin1_General_CI_AS NULL,
    [DATA_ALTERACAO] datetime NULL,
    [X_INTEGRADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [X_DATA_INTEGRACAO] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_T_IMP_CRM_VEICULO
   Criada em ..: 2024-03-20
   Alterada em : 2024-03-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_T_IMP_CRM_VEICULO] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE Latin1_General_CI_AS NULL,
    [VV1_CHAINT] varchar(6) COLLATE Latin1_General_CI_AS NOT NULL,
    [DATA_INCLUSAO] date NULL,
    [DATA_ALTERACAO] datetime2(7) NULL,
    [X_INTEGRADO] varchar(20) COLLATE Latin1_General_CI_AS NOT NULL,
    [X_DATA_INTEGRACAO] date NULL,
    CONSTRAINT [X_T_IMP_CRM_VEICULO_PK] PRIMARY KEY CLUSTERED ([ID]),
    CONSTRAINT [X_T_IMP_CRM_VEICULO_UNIQUE] UNIQUE NONCLUSTERED ([VV1_CHAINT])
);
GO
CREATE NONCLUSTERED INDEX [X_T_IMP_CRM_VEICULO_ID_CHAINT] ON [dbo].[X_T_IMP_CRM_VEICULO] ([ID], [VV1_CHAINT]);
GO
CREATE NONCLUSTERED INDEX [X_T_IMP_CRM_VEICULO_ID_CHAINT_X_INTEGRADO] ON [dbo].[X_T_IMP_CRM_VEICULO] ([ID], [VV1_CHAINT], [X_INTEGRADO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_TOTVS_BI_FATURAMENTO_MAQUINAS
   Criada em ..: 2022-12-09
   Alterada em : 2022-12-09
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_TOTVS_BI_FATURAMENTO_MAQUINAS] (
    [origem] varchar(36) COLLATE Latin1_General_CI_AS NULL,
    [origem_fonte] varchar(24) COLLATE Latin1_General_CI_AS NOT NULL,
    [tipo_nf] varchar(12) COLLATE Latin1_General_CI_AS NULL,
    [filial] int NULL,
    [nro_nf] varchar(9) COLLATE Latin1_General_CI_AS NOT NULL,
    [seq_pessoa] varchar(800) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [cliente] varchar(40) COLLATE Latin1_General_CI_AS NULL,
    [nome_operacao] varchar(20) COLLATE Latin1_General_CI_AS NULL,
    [data_emissao_nf] date NULL,
    [cliente_fj] varchar(1) COLLATE Latin1_General_CI_AS NULL,
    [cod_produto] varchar(8000) COLLATE Latin1_General_CI_AS NULL,
    [nome_produto] varchar(60) COLLATE Latin1_General_CI_AS NULL,
    [vlr_liquido_item] float NOT NULL
);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_BI_FATURAMENTO_MAQUINAS_cod_produto_IDX] ON [dbo].[X_TOTVS_BI_FATURAMENTO_MAQUINAS] ([cod_produto]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_BI_FATURAMENTO_MAQUINAS_filial_IDX] ON [dbo].[X_TOTVS_BI_FATURAMENTO_MAQUINAS] ([filial], [nro_nf], [seq_pessoa]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_BI_FATURAMENTO_MAQUINAS_seq_pessoa_IDX] ON [dbo].[X_TOTVS_BI_FATURAMENTO_MAQUINAS] ([seq_pessoa], [cod_produto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_TOTVS_BI_FATURAMENTO_POS_VENDAS
   Criada em ..: 2023-09-05
   Alterada em : 2023-09-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_TOTVS_BI_FATURAMENTO_POS_VENDAS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ORIGEM] varchar(12) COLLATE Latin1_General_CI_AS NOT NULL,
    [FILIAL] int NULL,
    [DATA_EMISSAO_NF] date NULL,
    [NRO_NF] varchar(9) COLLATE Latin1_General_CI_AS NOT NULL,
    [SERIE] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [COD_CLIENTE] varchar(14) COLLATE Latin1_General_CI_AS NULL,
    [CPF_CNPJ] varchar(14) COLLATE Latin1_General_CI_AS NULL,
    [COD_MARCA] varchar(6) COLLATE Latin1_General_CI_AS NULL,
    [DES_MARCA] varchar(30) COLLATE Latin1_General_CI_AS NULL,
    [ID_VENDEDOR] varchar(6) COLLATE Latin1_General_CI_AS NOT NULL,
    [COD_OPERACAO] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [DES_OPERACAO] varchar(20) COLLATE Latin1_General_CI_AS NULL,
    [CFOP] varchar(5) COLLATE Latin1_General_CI_AS NULL,
    [PRODUTO] varchar(28) COLLATE Latin1_General_CI_AS NULL,
    [COD_FAMILIA] varchar(10) COLLATE Latin1_General_CI_AS NULL,
    [DESC_FAMILIA] varchar(80) COLLATE Latin1_General_CI_AS NULL,
    [QUANTIDADE] float NOT NULL,
    [VLR_UNITARIO] float NOT NULL,
    [VALOR_TOTAL] float NOT NULL,
    [VALOR_DESCONTO] float NOT NULL,
    [VLR_LIQUIDO_ITEM] float NOT NULL,
    [VLR_ICMS] float NOT NULL,
    [VLR_ICMS_SUBST] float NOT NULL,
    [VLR_PIS] float NOT NULL,
    [VALOR_COFINS] float NOT NULL,
    [VALOR_CUSTO_MEDIO] float NOT NULL,
    [VALOR_FRETE] float NOT NULL,
    [VALOR_MARGEM] float NOT NULL,
    [PERCENTUAL_MARGEM] float NOT NULL,
    CONSTRAINT [PK__X_TOTVS___3214EC2781A7BF54] PRIMARY KEY CLUSTERED ([ID])
);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_BI_FATURAMENTO_POS_VENDAS_ID_DATA] ON [dbo].[X_TOTVS_BI_FATURAMENTO_POS_VENDAS] ([ID], [DATA_EMISSAO_NF]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_BI_FATURAMENTO_POS_VENDAS_ID_IDX] ON [dbo].[X_TOTVS_BI_FATURAMENTO_POS_VENDAS] ([ID], [ORIGEM], [FILIAL], [NRO_NF], [COD_CLIENTE]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_BI_FATURAMENTO_POS_VENDAS_ID_ORIGEM_DATA] ON [dbo].[X_TOTVS_BI_FATURAMENTO_POS_VENDAS] ([ID], [ORIGEM], [DATA_EMISSAO_NF]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_TOTVS_CRM_FATURAMENTO
   Criada em ..: 2023-11-21
   Alterada em : 2024-05-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_TOTVS_CRM_FATURAMENTO] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ORIGEM] varchar(12) COLLATE Latin1_General_CI_AS NOT NULL,
    [FILIAL] int NULL,
    [DATA_EMISSAO_NF] date NULL,
    [NRO_NF] varchar(9) COLLATE Latin1_General_CI_AS NOT NULL,
    [NRO_ITEM] varchar(5) COLLATE Latin1_General_CI_AS NOT NULL,
    [SERIE] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [COD_CLIENTE] varchar(14) COLLATE Latin1_General_CI_AS NULL,
    [CPF_CNPJ] varchar(14) COLLATE Latin1_General_CI_AS NULL,
    [COD_MARCA] varchar(6) COLLATE Latin1_General_CI_AS NULL,
    [DES_MARCA] varchar(30) COLLATE Latin1_General_CI_AS NULL,
    [ID_VENDEDOR] varchar(6) COLLATE Latin1_General_CI_AS NOT NULL,
    [COD_OPERACAO] varchar(3) COLLATE Latin1_General_CI_AS NOT NULL,
    [DES_OPERACAO] varchar(20) COLLATE Latin1_General_CI_AS NULL,
    [CFOP] varchar(5) COLLATE Latin1_General_CI_AS NULL,
    [PRODUTO] varchar(28) COLLATE Latin1_General_CI_AS NULL,
    [DESC_PRODUTO] varchar(50) COLLATE Latin1_General_CI_AS NULL,
    [COD_FAMILIA] varchar(10) COLLATE Latin1_General_CI_AS NULL,
    [DESC_FAMILIA] varchar(80) COLLATE Latin1_General_CI_AS NULL,
    [NOME_VENDEDOR] varchar(42) COLLATE Latin1_General_CI_AS NULL,
    [DATA_PEDIDO] date NULL,
    [STATUS_NF] varchar(1) COLLATE Latin1_General_CI_AS NULL,
    [NCM] varchar(12) COLLATE Latin1_General_CI_AS NULL,
    [CODICAO_PAGAMENTO] varchar(5) COLLATE Latin1_General_CI_AS NULL,
    [DES_CODICAO_PAGAMENTO] varchar(40) COLLATE Latin1_General_CI_AS NULL,
    [NRO_PEDIDO] varchar(8) COLLATE Latin1_General_CI_AS NULL,
    [OBSERVACAO] varchar(30) COLLATE Latin1_General_CI_AS NULL,
    [QUANTIDADE] float NOT NULL,
    [VLR_UNITARIO] float NOT NULL,
    [VALOR_TOTAL] float NOT NULL,
    [VALOR_DESCONTO] float NOT NULL,
    [VLR_LIQUIDO_ITEM] float NOT NULL,
    [VLR_ICMS] float NOT NULL,
    [VLR_ICMS_SUBST] float NOT NULL,
    [VLR_PIS] float NOT NULL,
    [VALOR_COFINS] float NOT NULL,
    [VALOR_CUSTO_MEDIO] float NOT NULL,
    [VALOR_FRETE] float NOT NULL,
    [VALOR_MARGEM] float NOT NULL,
    [PERCENTUAL_MARGEM] float NOT NULL,
    [DELETADO] varchar(1) COLLATE Latin1_General_CI_AS NULL,
    CONSTRAINT [X_TOTVS_CRM_FATURAMENTO_PK] PRIMARY KEY CLUSTERED ([ID]),
    CONSTRAINT [X_TOTVS_CRM_FATURAMENTO_UN_CHAVE_ITEM] UNIQUE NONCLUSTERED ([STATUS_NF], [ORIGEM], [FILIAL], [NRO_NF], [SERIE], [COD_CLIENTE], [COD_OPERACAO], [PRODUTO], [NRO_ITEM], [DELETADO])
);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_CRM_FATURAMENTO_ID_COD_CHAVE_ITEM] ON [dbo].[X_TOTVS_CRM_FATURAMENTO] ([ID], [STATUS_NF], [ORIGEM], [FILIAL], [NRO_NF], [SERIE], [COD_CLIENTE], [COD_OPERACAO], [PRODUTO], [NRO_ITEM], [DELETADO]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_CRM_FATURAMENTO_ID_COD_CHAVE_NF] ON [dbo].[X_TOTVS_CRM_FATURAMENTO] ([ID], [STATUS_NF], [ORIGEM], [FILIAL], [NRO_NF], [SERIE], [COD_CLIENTE], [DELETADO]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_CRM_FATURAMENTO_ID_DATA] ON [dbo].[X_TOTVS_CRM_FATURAMENTO] ([ID], [DATA_EMISSAO_NF]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_CRM_FATURAMENTO_ID_IDX] ON [dbo].[X_TOTVS_CRM_FATURAMENTO] ([ID], [ORIGEM], [FILIAL], [NRO_NF], [COD_CLIENTE]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_CRM_FATURAMENTO_ID_ORIGEM_DATA] ON [dbo].[X_TOTVS_CRM_FATURAMENTO] ([ID], [ORIGEM], [DATA_EMISSAO_NF]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_CRM_FATURAMENTO_SUGES_01] ON [dbo].[X_TOTVS_CRM_FATURAMENTO] ([NRO_NF], [DATA_EMISSAO_NF]) INCLUDE ([CFOP], [COD_CLIENTE], [COD_FAMILIA], [COD_MARCA], [COD_OPERACAO], [CODICAO_PAGAMENTO], [CPF_CNPJ], [DATA_PEDIDO], [DELETADO], [DES_CODICAO_PAGAMENTO], [DES_MARCA], [DES_OPERACAO], [DESC_FAMILIA], [DESC_PRODUTO], [FILIAL], [ID_VENDEDOR], [NCM], [NOME_VENDEDOR], [NRO_ITEM], [NRO_PEDIDO], [OBSERVACAO], [ORIGEM], [PERCENTUAL_MARGEM], [PRODUTO], [QUANTIDADE], [SERIE], [STATUS_NF], [VALOR_COFINS], [VALOR_CUSTO_MEDIO], [VALOR_DESCONTO], [VALOR_FRETE], [VALOR_MARGEM], [VALOR_TOTAL], [VLR_ICMS], [VLR_ICMS_SUBST], [VLR_LIQUIDO_ITEM], [VLR_PIS], [VLR_UNITARIO]);
GO
CREATE NONCLUSTERED INDEX [X_TOTVS_CRM_FATURAMENTO_SUGES_02] ON [dbo].[X_TOTVS_CRM_FATURAMENTO] ([ORIGEM], [DATA_EMISSAO_NF], [DELETADO]) INCLUDE ([FILIAL], [NRO_NF], [NRO_ITEM], [SERIE], [COD_CLIENTE], [CPF_CNPJ], [COD_MARCA], [DES_MARCA], [ID_VENDEDOR], [COD_OPERACAO], [DES_OPERACAO], [CFOP], [PRODUTO], [DESC_PRODUTO], [COD_FAMILIA], [DESC_FAMILIA], [NOME_VENDEDOR], [DATA_PEDIDO], [STATUS_NF], [NCM], [CODICAO_PAGAMENTO], [DES_CODICAO_PAGAMENTO], [NRO_PEDIDO], [OBSERVACAO], [QUANTIDADE], [VLR_UNITARIO], [VALOR_TOTAL], [VALOR_DESCONTO], [VLR_LIQUIDO_ITEM], [VLR_ICMS], [VLR_ICMS_SUBST], [VLR_PIS], [VALOR_COFINS], [VALOR_CUSTO_MEDIO], [VALOR_FRETE], [VALOR_MARGEM], [PERCENTUAL_MARGEM]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_V_IMP_CRM_IMP_NF
   Criada em ..: 2023-09-12
   Alterada em : 2023-09-12
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_V_IMP_CRM_IMP_NF] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [COD_CHAVE_ITEM] varchar(150) COLLATE Latin1_General_CI_AS NOT NULL,
    [COD_CHAVE_NF] varchar(100) COLLATE Latin1_General_CI_AS NOT NULL,
    [ORIGEM] varchar(12) COLLATE Latin1_General_CI_AS NOT NULL,
    [X_INTEGRADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [X_DATA_INTEGRACAO] datetime NULL,
    CONSTRAINT [X_V_IMP_CRM_IMP_NF_PK] PRIMARY KEY CLUSTERED ([ID])
);
GO
CREATE NONCLUSTERED INDEX [X_V_IMP_CRM_IMP_NF_ID_CHAVE_ITEM] ON [dbo].[X_V_IMP_CRM_IMP_NF] ([ID], [COD_CHAVE_ITEM]);
GO
CREATE NONCLUSTERED INDEX [X_V_IMP_CRM_IMP_NF_ID_INTEGRADO_CHAVE_ITEM] ON [dbo].[X_V_IMP_CRM_IMP_NF] ([ID], [X_INTEGRADO], [COD_CHAVE_ITEM]);
GO
CREATE NONCLUSTERED INDEX [X_V_IMP_CRM_IMP_NF_SQL_ITEM] ON [dbo].[X_V_IMP_CRM_IMP_NF] ([ID], [COD_CHAVE_ITEM], [ORIGEM], [X_INTEGRADO]);
GO
CREATE NONCLUSTERED INDEX [X_V_IMP_CRM_IMP_NF_SQL_NF] ON [dbo].[X_V_IMP_CRM_IMP_NF] ([ID], [COD_CHAVE_NF], [ORIGEM], [X_INTEGRADO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.X_V_IMP_CRM_IMP_NF_BKP_18_09_2023
   Criada em ..: 2023-09-18
   Alterada em : 2023-09-18
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[X_V_IMP_CRM_IMP_NF_BKP_18_09_2023] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [COD_CHAVE_ITEM] varchar(150) COLLATE Latin1_General_CI_AS NOT NULL,
    [COD_CHAVE_NF] varchar(100) COLLATE Latin1_General_CI_AS NOT NULL,
    [ORIGEM] varchar(12) COLLATE Latin1_General_CI_AS NOT NULL,
    [X_INTEGRADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [X_DATA_INTEGRACAO] datetime NULL
);
GO

