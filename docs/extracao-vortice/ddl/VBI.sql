/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo VBI
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.VBI_CONTA
   Criada em ..: 2017-07-11
   Alterada em : 2017-07-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[VBI_CONTA] (
    [SEQCONTA] numeric(18,0) NOT NULL,
    [SEQCONTAPAI] numeric(18,0) NOT NULL,
    [NIVEL] numeric(2,0) NOT NULL,
    [NROORDEM] numeric(4,0) NOT NULL,
    [DESCRICAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CODCONTA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CORRGB] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CHAVEEXTERNA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVEEXTOBS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL] numeric(1,0) NOT NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__VBI_CONT__4CE55003E921DAE8] PRIMARY KEY CLUSTERED ([SEQCONTA])
);
GO
CREATE NONCLUSTERED INDEX [IDX_VBI_CONTAGERENC_0] ON [dbo].[VBI_CONTA] ([SEQCONTAPAI]);
GO
ALTER TABLE [dbo].[VBI_CONTA] ADD CONSTRAINT [FK__VBI_CONTA__SEQCO__56564CD3] FOREIGN KEY ([SEQCONTAPAI]) REFERENCES [dbo].[VBI_CONTAPAI] ([SEQCONTA]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.VBI_CONTAFAM
   Criada em ..: 2017-07-11
   Alterada em : 2017-07-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[VBI_CONTAFAM] (
    [SEQCONTA] numeric(18,0) NOT NULL,
    [SEQCONTA1] numeric(18,0) NULL,
    [SEQCONTA2] numeric(18,0) NULL,
    [SEQCONTA3] numeric(18,0) NULL,
    [SEQCONTA4] numeric(18,0) NULL,
    [SEQCONTA5] numeric(18,0) NULL,
    CONSTRAINT [PK__VBI_CONT__4CE550036E06FD51] PRIMARY KEY CLUSTERED ([SEQCONTA])
);
GO
ALTER TABLE [dbo].[VBI_CONTAFAM] ADD CONSTRAINT [FK__VBI_CONTA__SEQCO__574A710C] FOREIGN KEY ([SEQCONTA]) REFERENCES [dbo].[VBI_CONTA] ([SEQCONTA]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.VBI_CONTAPAI
   Criada em ..: 2017-07-11
   Alterada em : 2017-07-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[VBI_CONTAPAI] (
    [SEQCONTA] numeric(18,0) NOT NULL,
    [GRUPOGESTAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NIVEL] numeric(2,0) NOT NULL,
    [NROORDEM] numeric(4,0) NOT NULL,
    [DESCRICAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CODCONTA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [IDENTIFICADOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQCONTAPAI] numeric(18,0) NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CORRGB] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CORRGBLETRA] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NEGRITO] numeric(1,0) NULL,
    [FONTEESTILO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ALINHAMENTO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAMANHO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__VBI_CONT__4CE5500321E13E98] PRIMARY KEY CLUSTERED ([SEQCONTA])
);
GO
CREATE NONCLUSTERED INDEX [IDX_VBI_CONTAPAI_1] ON [dbo].[VBI_CONTAPAI] ([GRUPOGESTAO]);
GO
CREATE NONCLUSTERED INDEX [IDX_VBI_CONTAPAIC_2] ON [dbo].[VBI_CONTAPAI] ([SEQCONTAPAI]);
GO
CREATE NONCLUSTERED INDEX [VBI_CONTAPIX1] ON [dbo].[VBI_CONTAPAI] ([NROORDEM]);
GO
CREATE NONCLUSTERED INDEX [VBI_CONTAPIX2] ON [dbo].[VBI_CONTAPAI] ([CODCONTA]);
GO

