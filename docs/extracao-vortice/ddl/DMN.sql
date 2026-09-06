/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo DMN
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_Doc
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_Doc] (
    [SeqDocto] numeric(18,0) NOT NULL,
    [SeqDocTp] decimal(4,0) NULL,
    [SeqVrs] decimal(4,0) NOT NULL,
    [Descr] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Arq] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ext] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaBase] datetime NULL,
    [Validade] datetime NULL,
    [DtaUltVrs] datetime NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaCkIn] datetime NULL,
    [UsuCkIn] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaCkOut] datetime NULL,
    [UsuCkOut] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdeCons] decimal(6,0) NULL,
    [UltCons] datetime NULL,
    [Obs] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_Doc__E97A788829326739] PRIMARY KEY CLUSTERED ([SeqDocto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocPK] ON [dbo].[DMN_Doc] ([SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocIF1] ON [dbo].[DMN_Doc] ([SeqDocTp]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocArq
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocArq] (
    [SeqDocto] numeric(18,0) NOT NULL,
    [Bloco] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sala] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Endereco] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocA__E97A7888DE14F885] PRIMARY KEY CLUSTERED ([SeqDocto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocArqPK] ON [dbo].[DMN_DocArq] ([SeqDocto]);
GO
ALTER TABLE [dbo].[DMN_DocArq] ADD CONSTRAINT [FK__DMN_DocAr__SeqDo__14089B32] FOREIGN KEY ([SeqDocto]) REFERENCES [dbo].[DMN_Doc] ([SeqDocto]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocHst
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocHst] (
    [SeqDocto] numeric(18,0) NOT NULL,
    [SeqDH] decimal(6,0) NOT NULL,
    [DtaMovto] datetime NULL,
    [TipoMov] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descr] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocH__AED5C93A318CF887] PRIMARY KEY CLUSTERED ([SeqDocto], [SeqDH])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocHstPK] ON [dbo].[DMN_DocHst] ([SeqDocto], [SeqDH]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocHstIF1] ON [dbo].[DMN_DocHst] ([SeqDocto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocObs
   Criada em ..: 2011-12-20
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocObs] (
    [SeqDocto] numeric(18,0) NOT NULL,
    [Obs] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocO__E97A7888D2D8F196] PRIMARY KEY CLUSTERED ([SeqDocto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocObsPK] ON [dbo].[DMN_DocObs] ([SeqDocto]);
GO
ALTER TABLE [dbo].[DMN_DocObs] ADD CONSTRAINT [FK__DMN_DocOb__SeqDo__15F0E3A4] FOREIGN KEY ([SeqDocto]) REFERENCES [dbo].[DMN_Doc] ([SeqDocto]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocPes
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocPes] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqDocto] numeric(18,0) NOT NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocP__3DE3070415A7C5CC] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqDocto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocPesPK] ON [dbo].[DMN_DocPes] ([SeqPessoa], [SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocPesIF1] ON [dbo].[DMN_DocPes] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocPesIF2] ON [dbo].[DMN_DocPes] ([SeqDocto]);
GO
ALTER TABLE [dbo].[DMN_DocPes] ADD CONSTRAINT [FK__DMN_DocPe__SeqPe__1492AB17] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[DMN_DocPes] ADD CONSTRAINT [FK__DMN_DocPe__SeqPe__16E507DD] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[DMN_DocPes] ADD CONSTRAINT [FK__DMN_DocPe__SeqPe__5D227A9C] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocProj
   Criada em ..: 2011-12-20
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocProj] (
    [SeqProjeto] numeric(18,0) NOT NULL,
    [SeqDocto] numeric(18,0) NOT NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocP__5308B81253E3BBAA] PRIMARY KEY CLUSTERED ([SeqProjeto], [SeqDocto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocProjPK] ON [dbo].[DMN_DocProj] ([SeqProjeto], [SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocProjIF1] ON [dbo].[DMN_DocProj] ([SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocProjIF2] ON [dbo].[DMN_DocProj] ([SeqProjeto]);
GO
ALTER TABLE [dbo].[DMN_DocProj] ADD CONSTRAINT [FK__DMN_DocPr__SeqPr__5FFEE747] FOREIGN KEY ([SeqProjeto]) REFERENCES [dbo].[IV_Projeto] ([SeqProjeto]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[DMN_DocProj] ADD CONSTRAINT [FK__DMN_DocPr__SeqDo__18CD504F] FOREIGN KEY ([SeqDocto]) REFERENCES [dbo].[DMN_Doc] ([SeqDocto]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocProp
   Criada em ..: 2011-12-20
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocProp] (
    [SeqPropPessoa] numeric(18,0) NOT NULL,
    [SeqDocto] numeric(18,0) NOT NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocP__D2DA107CC086AAB1] PRIMARY KEY CLUSTERED ([SeqPropPessoa], [SeqDocto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocPropPK] ON [dbo].[DMN_DocProp] ([SeqPropPessoa], [SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocPropIF1] ON [dbo].[DMN_DocProp] ([SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocPropIF2] ON [dbo].[DMN_DocProp] ([SeqPropPessoa]);
GO
ALTER TABLE [dbo].[DMN_DocProp] ADD CONSTRAINT [FK__DMN_DocPr__SeqPr__61E72FB9] FOREIGN KEY ([SeqPropPessoa]) REFERENCES [dbo].[IV_ClientePropr] ([SeqPropPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[DMN_DocProp] ADD CONSTRAINT [FK__DMN_DocPr__SeqDo__1AB598C1] FOREIGN KEY ([SeqDocto]) REFERENCES [dbo].[DMN_Doc] ([SeqDocto]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocTp
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocTp] (
    [SeqDocTp] decimal(4,0) NOT NULL,
    [DocTp] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descr] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdDiaProcEncer] numeric(5,0) NULL,
    [QtdDiaValidade] decimal(4,0) NULL,
    [ExigArq] numeric(1,0) NULL,
    [ExigAutent] numeric(1,0) NULL,
    [ExigDtaBase] numeric(1,0) NULL,
    [ExigValidade] numeric(1,0) NULL,
    [Formato] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VincPessoa] numeric(1,0) NULL,
    [VincProcesso] numeric(1,0) NULL,
    [VincPropriedade] numeric(1,0) NULL,
    [VincProjeto] numeric(1,0) NULL,
    [UmPorPessoa] numeric(1,0) NULL,
    [UmPorProcesso] numeric(1,0) NULL,
    [UmPorPropriedade] numeric(1,0) NULL,
    [UmPorProjeto] numeric(1,0) NULL,
    [TamanhoMax] decimal(8,0) NULL,
    [TrocaPropriedade] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REFPESSOA] numeric(1,0) NULL,
    [VINCOS] numeric(1,0) NULL,
    [UMPOROS] numeric(1,0) NULL,
    [EXTENSOES] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocT__E97B64A7F5B63030] PRIMARY KEY CLUSTERED ([SeqDocTp])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocTpPK] ON [dbo].[DMN_DocTp] ([SeqDocTp]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DMN_DocVrs
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DMN_DocVrs] (
    [SeqDocto] numeric(18,0) NOT NULL,
    [SeqVrs] decimal(4,0) NOT NULL,
    [Arq] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ext] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__DMN_DocV__DBB20EF31CAEE4E2] PRIMARY KEY CLUSTERED ([SeqDocto], [SeqVrs])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [DMN_DocVrsPK] ON [dbo].[DMN_DocVrs] ([SeqDocto], [SeqVrs]);
GO
CREATE NONCLUSTERED INDEX [DMN_DocVrsIF1] ON [dbo].[DMN_DocVrs] ([SeqDocto]);
GO
ALTER TABLE [dbo].[DMN_DocVrs] ADD CONSTRAINT [FK__DMN_DocVr__SeqDo__1C9DE133] FOREIGN KEY ([SeqDocto]) REFERENCES [dbo].[DMN_Doc] ([SeqDocto]) ON DELETE CASCADE;
GO

