/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IVC
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVC_ATENDENTE
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVC_ATENDENTE] (
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSCMPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSCTI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQAGENDA] numeric(18,0) NULL,
    [NROFONEEXTERNO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRORAMAL] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINICIO] datetime NULL,
    [DTAINICIOCONVERSA] datetime NULL,
    [DTAFINALCONVERSA] datetime NULL,
    [DTAULTATENDIMENTO] datetime NULL,
    [DTAULTSTATUS] datetime NULL,
    [DTALOGIN] datetime NULL,
    [IDCHAMADACTI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SENTIDO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO] numeric(18,0) NULL,
    [STATUSNEXT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SILOORIGEM] numeric(8,0) NULL,
    CONSTRAINT [PK__IVC_ATEN__59D861DEE66318A2] PRIMARY KEY CLUSTERED ([SEQUSUARIO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IVC_ATENDENTE_0] ON [dbo].[IVC_ATENDENTE] ([SEQAGENDA]);
GO
ALTER TABLE [dbo].[IVC_ATENDENTE] ADD CONSTRAINT [FK__IVC_ATEND__SEQUS__04FC2D23] FOREIGN KEY ([SEQUSUARIO]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVC_ATENDENTE] ADD CONSTRAINT [FK__IVC_ATEND__SEQAG__05F0515C] FOREIGN KEY ([SEQAGENDA]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVC_ATENDENTELOG
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVC_ATENDENTELOG] (
    [SEQATENDENTELOG] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SEQUSUARIO] numeric(18,0) NULL,
    [DATA] datetime NULL,
    [STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSCMPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRORAMAL] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVC_ATEN__B1B74CEA5E3C3A97] PRIMARY KEY CLUSTERED ([SEQATENDENTELOG])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVC_CHAMADALOG
   Criada em ..: 2019-02-05
   Alterada em : 2021-06-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVC_CHAMADALOG] (
    [SEQCHAMADALOG] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [DTAINICIO] datetime NULL,
    [DTAFINAL] datetime NULL,
    [SEQUSUARIO] numeric(18,0) NULL,
    [SEQAGENDA] numeric(18,0) NULL,
    [PROCESSO] numeric(18,0) NULL,
    [NRORAMAL] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROFONEEXTERNO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SENTIDO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDCHAMADACTI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ARQUIVOAUDIO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SILOORIGEM] numeric(8,0) NULL,
    [DTAINICIOCONVERSA] datetime NULL,
    [DTAFINALCONVERSA] datetime NULL,
    [DURACAOTOTAL] numeric(6,1) NULL,
    [DURACAOCONVERSA] numeric(6,1) NULL,
    [RESULTADO] numeric(10,0) NULL,
    [OBS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EVENTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPESSOA] numeric(18,0) NULL,
    [DTAGERACAO] datetime NULL,
    [INDLINKGERADO] numeric(1,0) NULL,
    CONSTRAINT [PK__IVC_CHAM__3119230B68278C33] PRIMARY KEY CLUSTERED ([SEQCHAMADALOG])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVC_EQUIPE
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVC_EQUIPE] (
    [SEQEQUIPE] numeric(6,0) NOT NULL,
    [EQUIPE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESCRICAO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__IVC_EQUI__EFAE8DBD322D9315] PRIMARY KEY CLUSTERED ([SEQEQUIPE])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVC_EQUIPEUSR
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVC_EQUIPEUSR] (
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [SEQEQUIPE] numeric(6,0) NOT NULL,
    [PAPEL] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVC_EQUI__97228905BCC242F5] PRIMARY KEY CLUSTERED ([SEQUSUARIO], [SEQEQUIPE])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IVC_EQUIPEUSR] ON [dbo].[IVC_EQUIPEUSR] ([SEQUSUARIO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVC_EQUIPEUSR_0] ON [dbo].[IVC_EQUIPEUSR] ([SEQEQUIPE]);
GO
ALTER TABLE [dbo].[IVC_EQUIPEUSR] ADD CONSTRAINT [FK__IVC_EQUIP__SEQUS__06E47595] FOREIGN KEY ([SEQUSUARIO]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVC_EQUIPEUSR] ADD CONSTRAINT [FK__IVC_EQUIP__SEQEQ__07D899CE] FOREIGN KEY ([SEQEQUIPE]) REFERENCES [dbo].[IVC_EQUIPE] ([SEQEQUIPE]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVC_RAMAL
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVC_RAMAL] (
    [IDPABX] numeric(2,0) NOT NULL,
    [RAMAL] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DATASTATUS] datetime NULL,
    [STATUSCTI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSDESC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVC_RAMA__A341CFDB6E4AC604] PRIMARY KEY CLUSTERED ([IDPABX], [RAMAL])
);
GO

