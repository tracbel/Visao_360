/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo GEL
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEL_CEP
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEL_CEP] (
    [Cep] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TipoCEP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqCidade] decimal(6,0) NULL,
    [LogrFonetica] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TipoLograd] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cidade] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CidadeFonetica] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CepCidade] numeric(1,0) NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GEL_CEP__5091BB2E] PRIMARY KEY CLUSTERED ([Cep])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_CEPPK] ON [dbo].[GEL_CEP] ([Cep]);
GO
CREATE NONCLUSTERED INDEX [GEL_CEPIE1] ON [dbo].[GEL_CEP] ([LogrFonetica]);
GO
CREATE NONCLUSTERED INDEX [GEL_CEPIE2] ON [dbo].[GEL_CEP] ([CidadeFonetica]);
GO
CREATE NONCLUSTERED INDEX [GEL_CEPIF1] ON [dbo].[GEL_CEP] ([SeqCidade]);
GO
ALTER TABLE [dbo].[GEL_CEP] ADD CONSTRAINT [FK__GEL_CEP__SeqCida__32F736A6] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GEL_Cidade] ([SeqCidade]);
GO
ALTER TABLE [dbo].[GEL_CEP] ADD CONSTRAINT [FK__GEL_CEP__Cep__33EB5ADF] FOREIGN KEY ([Cep]) REFERENCES [dbo].[GEL_CepOrig] ([Cep]);
GO
ALTER TABLE [dbo].[GEL_CEP] ADD CONSTRAINT [FK__GEL_CEP__SeqCida__6CB9C3E7] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GEL_Cidade] ([SeqCidade]);
GO
ALTER TABLE [dbo].[GEL_CEP] ADD CONSTRAINT [FK__GEL_CEP__Cep__6DADE820] FOREIGN KEY ([Cep]) REFERENCES [dbo].[GEL_CepOrig] ([Cep]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEL_CepAlerta
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEL_CepAlerta] (
    [Cep] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Alerta] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GEL_CepAlerta__5185DF67] PRIMARY KEY CLUSTERED ([Cep])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_CepAlertaPK] ON [dbo].[GEL_CepAlerta] ([Cep]);
GO
ALTER TABLE [dbo].[GEL_CepAlerta] ADD CONSTRAINT [FK__GEL_CepAler__Cep__34DF7F18] FOREIGN KEY ([Cep]) REFERENCES [dbo].[GEL_CEP] ([Cep]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEL_CepOrig
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEL_CepOrig] (
    [Cep] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TipoCEP] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Complemento] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cidade] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqCidade] numeric(18,0) NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GEL_CepOrig__527A03A0] PRIMARY KEY CLUSTERED ([Cep])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_CepOrigPK] ON [dbo].[GEL_CepOrig] ([Cep]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEL_Cidade
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEL_Cidade] (
    [SeqCidade] decimal(6,0) NOT NULL,
    [Cidade] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Regiao] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Populacao] decimal(8,0) NULL,
    [DtaAniversario] datetime NULL,
    [DtaFeriado1] datetime NULL,
    [DescFeriado1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFeriado2] datetime NULL,
    [DescFeriado2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GEL_Cidade__536E27D9] PRIMARY KEY CLUSTERED ([SeqCidade])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_CidadePK] ON [dbo].[GEL_Cidade] ([SeqCidade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEL_Conv
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEL_Conv] (
    [SeqConv] numeric(6,0) NOT NULL,
    [ConvPara] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GEL_Conv__54624C12] PRIMARY KEY CLUSTERED ([SeqConv])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_ConvPK] ON [dbo].[GEL_Conv] ([SeqConv]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_ConvAK1] ON [dbo].[GEL_Conv] ([ConvPara]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEL_ConvDe
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEL_ConvDe] (
    [SeqConv] numeric(6,0) NOT NULL,
    [SeqConvDe] numeric(4,0) NOT NULL,
    [Nivel] decimal(1,0) NULL,
    [ConvDe] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GEL_ConvDe__5556704B] PRIMARY KEY CLUSTERED ([SeqConv], [SeqConvDe])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_ConvDePK] ON [dbo].[GEL_ConvDe] ([SeqConv], [SeqConvDe]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEL_ConvDeAK1] ON [dbo].[GEL_ConvDe] ([ConvDe]);
GO
CREATE NONCLUSTERED INDEX [GEL_ConvDeIF1] ON [dbo].[GEL_ConvDe] ([SeqConv]);
GO
ALTER TABLE [dbo].[GEL_ConvDe] ADD CONSTRAINT [FK__GEL_ConvD__SeqCo__35D3A351] FOREIGN KEY ([SeqConv]) REFERENCES [dbo].[GEL_Conv] ([SeqConv]) ON DELETE CASCADE;
GO

