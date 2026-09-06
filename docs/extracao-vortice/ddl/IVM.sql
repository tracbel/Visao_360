/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IVM
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVM_Material
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVM_Material] (
    [SeqMaterial] numeric(18,0) NOT NULL,
    [CodMaterial] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [EmUso] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Preco1] decimal(15,2) NULL,
    [Preco2] decimal(15,2) NULL,
    [QtdePadrao] decimal(10,2) NULL,
    [QtdeFixa] numeric(1,0) NULL,
    [PrecoFixo] numeric(1,0) NULL,
    [Custeio] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CusteioFixo] numeric(1,0) NULL,
    [Atributo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVM_Material__636F8578] PRIMARY KEY CLUSTERED ([SeqMaterial])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVM_MaterialPK] ON [dbo].[IVM_Material] ([SeqMaterial]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVM_MatPessoa
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVM_MatPessoa] (
    [SeqMaterial] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Preco1] decimal(15,2) NULL,
    [Preco2] decimal(15,2) NULL,
    [QtdePadrao] decimal(10,2) NULL,
    [QtdeFixa] numeric(1,0) NULL,
    [PrecoFixo] numeric(1,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVM_MatPessoa__6463A9B1] PRIMARY KEY CLUSTERED ([SeqMaterial], [SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVM_MatPessoaPK] ON [dbo].[IVM_MatPessoa] ([SeqMaterial], [SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IVM_MatPessoaIF1] ON [dbo].[IVM_MatPessoa] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IVM_MatPessoaIF2] ON [dbo].[IVM_MatPessoa] ([SeqMaterial]);
GO
ALTER TABLE [dbo].[IVM_MatPessoa] ADD CONSTRAINT [FK__IVM_MatPe__SeqPe__44E0DCB7] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVM_MatPessoa] ADD CONSTRAINT [FK__IVM_MatPe__SeqMa__45D500F0] FOREIGN KEY ([SeqMaterial]) REFERENCES [dbo].[IVM_Material] ([SeqMaterial]);
GO
ALTER TABLE [dbo].[IVM_MatPessoa] ADD CONSTRAINT [FK__IVM_MatPe__SeqMa__7F978E31] FOREIGN KEY ([SeqMaterial]) REFERENCES [dbo].[IVM_Material] ([SeqMaterial]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVM_ProcMat
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVM_ProcMat] (
    [SeqProcMat] numeric(18,0) NOT NULL,
    [Processo] numeric(18,0) NOT NULL,
    [DtaAplicacao] datetime NULL,
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Responsavel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroDocto] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVM_ProcMat__6557CDEA] PRIMARY KEY CLUSTERED ([SeqProcMat])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVM_ProcMatPK] ON [dbo].[IVM_ProcMat] ([SeqProcMat]);
GO
CREATE NONCLUSTERED INDEX [IVM_ProcMatIF1] ON [dbo].[IVM_ProcMat] ([Processo]);
GO
ALTER TABLE [dbo].[IVM_ProcMat] ADD CONSTRAINT [FK__IVM_ProcM__Proce__46C92529] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_Processo] ([Processo]);
GO
ALTER TABLE [dbo].[IVM_ProcMat] ADD CONSTRAINT [FK__IVM_ProcM__Proce__008BB26A] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_Processo] ([Processo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVM_ProcMatItem
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVM_ProcMatItem] (
    [SeqMatItem] numeric(18,0) NOT NULL,
    [SeqMaterial] numeric(18,0) NOT NULL,
    [SeqProcMat] numeric(18,0) NOT NULL,
    [Qtde] decimal(10,2) NULL,
    [vlrUnitario] decimal(15,2) NULL,
    [DescTotal] decimal(15,2) NULL,
    [Obs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAplicou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Custeio] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAplicacao] datetime NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVM_ProcMatItem__664BF223] PRIMARY KEY CLUSTERED ([SeqMatItem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVM_ProcMatItemPK] ON [dbo].[IVM_ProcMatItem] ([SeqMatItem]);
GO
CREATE NONCLUSTERED INDEX [IVM_ProcMatItemIF1] ON [dbo].[IVM_ProcMatItem] ([SeqMaterial]);
GO
CREATE NONCLUSTERED INDEX [IVM_ProcMatItemIF2] ON [dbo].[IVM_ProcMatItem] ([SeqProcMat]);
GO
ALTER TABLE [dbo].[IVM_ProcMatItem] ADD CONSTRAINT [FK__IVM_ProcM__SeqPr__48B16D9B] FOREIGN KEY ([SeqProcMat]) REFERENCES [dbo].[IVM_ProcMat] ([SeqProcMat]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVM_ProcMatItem] ADD CONSTRAINT [FK__IVM_ProcM__SeqMa__47BD4962] FOREIGN KEY ([SeqMaterial]) REFERENCES [dbo].[IVM_Material] ([SeqMaterial]);
GO
ALTER TABLE [dbo].[IVM_ProcMatItem] ADD CONSTRAINT [FK__IVM_ProcM__SeqMa__017FD6A3] FOREIGN KEY ([SeqMaterial]) REFERENCES [dbo].[IVM_Material] ([SeqMaterial]);
GO

