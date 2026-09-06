/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IVP
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVP_PedCritica
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVP_PedCritica] (
    [SeqPedCritica] numeric(18,0) NOT NULL,
    [SeqPedido] numeric(18,0) NULL,
    [CodCritica] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [DtaAnalise] datetime NULL,
    [UsuAnalise] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndLiberada] numeric(1,0) NULL,
    [ObsLiberacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVP_PedCritica__0DA5B666] PRIMARY KEY CLUSTERED ([SeqPedCritica])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IVP_PedCritica] ON [dbo].[IVP_PedCritica] ([SeqPedido]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVP_ProdImagem
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVP_ProdImagem] (
    [SeqProdImagem] numeric(18,0) NOT NULL,
    [SeqProduto] numeric(18,0) NOT NULL,
    [Caminho] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [XIF1IVP_ProdImagem] ON [dbo].[IVP_ProdImagem] ([SeqProduto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVP_TabPreco
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVP_TabPreco] (
    [SeqProdPreco] numeric(18,0) NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] decimal(6,0) NOT NULL,
    [SeqGrupoPreco] decimal(6,0) NOT NULL,
    [SeqCanal] decimal(2,0) NOT NULL,
    [SeqDepto] decimal(4,0) NOT NULL,
    [VigorDe] datetime NULL,
    CONSTRAINT [PK__IVP_TabPreco__08E10149] PRIMARY KEY CLUSTERED ([SeqProdPreco])
);
GO
CREATE NONCLUSTERED INDEX [IVP_ProdPrecoIF2] ON [dbo].[IVP_TabPreco] ([SeqGrupoPreco]);
GO
CREATE NONCLUSTERED INDEX [IVP_ProdPrecoIF3] ON [dbo].[IVP_TabPreco] ([SeqDepto]);
GO
CREATE NONCLUSTERED INDEX [XIF4IVP_TabPreco] ON [dbo].[IVP_TabPreco] ([SeqCanal]);
GO
ALTER TABLE [dbo].[IVP_TabPreco] ADD CONSTRAINT [FK__IVP_TabPr__SeqDe__0AC949BB] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]);
GO
ALTER TABLE [dbo].[IVP_TabPreco] ADD CONSTRAINT [FK__IVP_TabPr__SeqCa__0BBD6DF4] FOREIGN KEY ([SeqCanal]) REFERENCES [dbo].[IVS_CanalVenda] ([SeqCanal]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVP_Vendedor
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVP_Vendedor] (
    [SeqVendedor] numeric(18,0) NOT NULL,
    [EmUso] numeric(1,0) NULL,
    [Equipe] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodVendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nome] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmprPadrao] numeric(6,0) NULL,
    [SalarioFixo] decimal(14,2) NULL,
    [Comissao1] decimal(8,4) NULL,
    [Comissao2] decimal(8,4) NULL,
    [Obs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsuario] decimal(8,0) NULL,
    CONSTRAINT [PK__IVP_Vendedor__5A3B20F9] PRIMARY KEY CLUSTERED ([SeqVendedor])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVP_VendedorAK1] ON [dbo].[IVP_Vendedor] ([CodVendedor]);
GO
CREATE NONCLUSTERED INDEX [IVP_VendedorIE1] ON [dbo].[IVP_Vendedor] ([SeqUsuario]);
GO

