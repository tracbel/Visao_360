/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo GEP
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_CidadeEmprDest
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_CidadeEmprDest] (
    [Destino] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqCidade] numeric(6,0) NOT NULL,
    CONSTRAINT [PK__GEP_CidadeEmprDe__3AAD6707] PRIMARY KEY CLUSTERED ([Destino], [NroEmpresa], [SeqCidade])
);
GO
CREATE NONCLUSTERED INDEX [IDX_GEP_USRCIDADEUSR] ON [dbo].[GEP_CidadeEmprDest] ([NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IDX_GEP_USRCIDADE] ON [dbo].[GEP_CidadeEmprDest] ([SeqCidade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_EMailSend
   Criada em ..: 2011-12-19
   Alterada em : 2021-06-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_EMailSend] (
    [Prioridade] decimal(2,0) NULL,
    [De] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Para] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cc] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bcc] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Assunto] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Anexado] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Mensagem] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Solicitante] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [EnderecoResposta] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [DtaEnviar] datetime NULL,
    [SEQEMAILSEND] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SEQCONTAEMAIL] numeric(18,0) NULL,
    [CONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUBCONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MSGERRO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] char(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVE] numeric(18,0) NULL,
    CONSTRAINT [PK__GEP_EMai__493CB600EEBA6035] PRIMARY KEY CLUSTERED ([SEQEMAILSEND])
);
GO
CREATE NONCLUSTERED INDEX [IDX_GEP_EMLSND_SEQCO] ON [dbo].[GEP_EMailSend] ([SEQCONTAEMAIL]);
GO
ALTER TABLE [dbo].[GEP_EMailSend] ADD CONSTRAINT [FK__GEP_EMail__SEQCO__0D917324] FOREIGN KEY ([SEQCONTAEMAIL]) REFERENCES [dbo].[GE_ParamLista] ([SeqParamLista]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_EMAILSENDREL
   Criada em ..: 2021-06-02
   Alterada em : 2021-06-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_EMAILSENDREL] (
    [SEQEMAILSENDREL] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SEQEMAILSEND] numeric(18,0) NOT NULL,
    [SEQCONSSQL] numeric(18,0) NOT NULL,
    [FILTROVALOR] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GEP_EMAI__5F46C7F3C51CBA05] PRIMARY KEY CLUSTERED ([SEQEMAILSENDREL])
);
GO
ALTER TABLE [dbo].[GEP_EMAILSENDREL] ADD CONSTRAINT [FK__GEP_EMAIL__SEQEM__413112BB] FOREIGN KEY ([SEQEMAILSEND]) REFERENCES [dbo].[GEP_EMailSend] ([SEQEMAILSEND]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[GEP_EMAILSENDREL] ADD CONSTRAINT [FK__GEP_EMAIL__SEQCO__422536F4] FOREIGN KEY ([SEQCONSSQL]) REFERENCES [dbo].[GE_CONSSQL] ([SEQCONSSQL]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_EMAILSENT
   Criada em ..: 2019-02-05
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_EMAILSENT] (
    [SEQEMAILSENT] numeric(18,0) NOT NULL,
    [DE] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BCC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ASSUNTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANEXADO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MENSAGEM] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SOLICITANTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROEMPRESA] numeric(6,0) NULL,
    [ENDERECORESPOSTA] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NULL,
    [STATUS] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MSGERRO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAENVIO] datetime NULL,
    [SEQCONTAEMAIL] numeric(18,0) NULL,
    [CONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUBCONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] char(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVE] numeric(18,0) NULL,
    CONSTRAINT [PK__GEP_EMAI__493CB61010EE67E7] PRIMARY KEY CLUSTERED ([SEQEMAILSENT])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EMAILSENT_PARA] ON [dbo].[GEP_EMAILSENT] ([PARA]);
GO
CREATE NONCLUSTERED INDEX [IDX_EMAILSENT_DTAENVIO] ON [dbo].[GEP_EMAILSENT] ([DTAENVIO]);
GO
CREATE NONCLUSTERED INDEX [IDX_EMAILSENT_STATUS] ON [dbo].[GEP_EMAILSENT] ([STATUS]);
GO
CREATE NONCLUSTERED INDEX [IDX_EMAILSENT_ORIGEM] ON [dbo].[GEP_EMAILSENT] ([ORIGEM]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_EMAILSENTREL
   Criada em ..: 2021-06-02
   Alterada em : 2021-06-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_EMAILSENTREL] (
    [SEQEMAILSENTREL] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SEQEMAILSENT] numeric(18,0) NOT NULL,
    [SEQCONSSQL] numeric(18,0) NOT NULL,
    [FILTROVALOR] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GEP_EMAI__D39674F292C160CE] PRIMARY KEY CLUSTERED ([SEQEMAILSENTREL])
);
GO
ALTER TABLE [dbo].[GEP_EMAILSENTREL] ADD CONSTRAINT [FK__GEP_EMAIL__SEQEM__4501A39F] FOREIGN KEY ([SEQEMAILSENT]) REFERENCES [dbo].[GEP_EMAILSENT] ([SEQEMAILSENT]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.gep_excfrota
   Criada em ..: 2016-03-24
   Alterada em : 2016-03-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[gep_excfrota] (
    [cliente] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [cidade] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ano] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [marca] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [modelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [qtde] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [tipo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.gep_excfrotarep
   Criada em ..: 2016-03-24
   Alterada em : 2016-03-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[gep_excfrotarep] (
    [seqpessoa] int NULL,
    [nomerazao] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [cidade] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [propriedade] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [modelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [qtde] int NULL,
    [marca] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ano] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [notas] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [identificador] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [qtfrota] int NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_EXCFROTASEQ
   Criada em ..: 2016-03-24
   Alterada em : 2016-03-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_EXCFROTASEQ] (
    [SEQPROPPESSOA] int NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_EXCFROTASEQDEL
   Criada em ..: 2016-03-24
   Alterada em : 2016-03-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_EXCFROTASEQDEL] (
    [SEQPROPPESSOA] int NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.gep_excseqcar
   Criada em ..: 2016-03-24
   Alterada em : 2016-03-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[gep_excseqcar] (
    [seqpessoa] int NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.gep_excseqcarok
   Criada em ..: 2016-03-24
   Alterada em : 2016-03-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[gep_excseqcarok] (
    [seqpessoa] int NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_Fila
   Criada em ..: 2011-12-20
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_Fila] (
    [Servidor] decimal(3,0) NULL,
    [CodProcesso] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Aplicativo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaExecucao] datetime NULL,
    [ExeMinuto] decimal(8,2) NULL,
    [Prioridade] decimal(2,0) NULL,
    [Solicitante] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqProcesso] numeric(18,0) NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Argumento] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DNet] numeric(1,0) NULL,
    [PrioridadeThread] numeric(1,0) NULL,
    [ThreadNro] numeric(4,0) NULL,
    [SEQFILA] numeric(18,0) IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK__GEP_Fila__628287DAFA0DE699] PRIMARY KEY CLUSTERED ([SEQFILA])
);
GO
CREATE NONCLUSTERED INDEX [GEP_FilaIE1] ON [dbo].[GEP_Fila] ([DtaExecucao]);
GO
CREATE NONCLUSTERED INDEX [GEP_FilaIE2] ON [dbo].[GEP_Fila] ([CodProcesso], [LinkNro]);
GO
CREATE NONCLUSTERED INDEX [GEP_FilaIE3] ON [dbo].[GEP_Fila] ([CodProcesso], [LinkStr]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_Import
   Criada em ..: 2011-12-20
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_Import] (
    [Seq] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Processo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Acao] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Banco] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dono] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Separador] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Status] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Coluna] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dado] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ColunaIdentific] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DadoIdentificador] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [Prioridade] decimal(2,0) NULL,
    CONSTRAINT [PK__GEP_Import__5832DCF6] PRIMARY KEY CLUSTERED ([Seq])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_ImportPK] ON [dbo].[GEP_Import] ([Seq]);
GO
CREATE NONCLUSTERED INDEX [GEP_ImportIE1] ON [dbo].[GEP_Import] ([Status], [Prioridade], [DtaGeracao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_Import_bkpjun
   Criada em ..: 2024-12-27
   Alterada em : 2024-12-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_Import_bkpjun] (
    [Seq] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Processo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Acao] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Banco] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dono] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Separador] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Status] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Coluna] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dado] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ColunaIdentific] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DadoIdentificador] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [Prioridade] decimal(2,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_IMPORT_GUI
   Criada em ..: 2025-02-25
   Alterada em : 2025-02-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_IMPORT_GUI] (
    [Seq] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Processo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Acao] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Banco] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dono] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Separador] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Status] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Coluna] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dado] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ColunaIdentific] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DadoIdentificador] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [Prioridade] decimal(2,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ImportAprovacao
   Criada em ..: 2025-07-15
   Alterada em : 2025-07-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ImportAprovacao] (
    [Seq] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Processo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Acao] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Banco] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dono] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Separador] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Status] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Coluna] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dado] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ColunaIdentific] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DadoIdentificador] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [Prioridade] decimal(2,0) NULL,
    CONSTRAINT [PK__GEP_Impo__CA1E3C887E7F7B16] PRIMARY KEY CLUSTERED ([Seq])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ImportErro
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ImportErro] (
    [Seq] numeric(18,0) NOT NULL,
    [Servidor] decimal(2,0) NULL,
    [Erro] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL
);
GO
CREATE NONCLUSTERED INDEX [GEP_ImportErroIE1] ON [dbo].[GEP_ImportErro] ([Seq]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ImportTry
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ImportTry] (
    [Seq] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaLimiteTentativa] datetime NULL,
    [Razao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GEP_ImportTry__5927012F] PRIMARY KEY CLUSTERED ([Seq], [Origem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_ImportTryPK] ON [dbo].[GEP_ImportTry] ([Seq], [Origem]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_JOBAGD
   Criada em ..: 2014-09-15
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_JOBAGD] (
    [SeqJOBAgd] numeric(18,0) NOT NULL,
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroZUMBI] numeric(2,0) NULL,
    [DtaInicio] datetime NULL,
    [Codigo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Aplicacao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Prioridade] numeric(1,0) NULL,
    [THREADEXCLUSIVA] numeric(1,0) NULL,
    [Argumento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IntervaloExec] numeric(4,0) NULL,
    [UltimaExecucao] datetime NULL,
    [NroTENTATIVA] numeric(2,0) NULL,
    [IntervaloTENTATIVA] numeric(4,0) NULL,
    [DiasVIdALOG] numeric(4,0) NULL,
    [PlanoExec] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExecucaoExclusiva] numeric(1,0) NULL,
    [SEQJOB] numeric(18,0) NULL,
    CONSTRAINT [PK__GEP_JOBAGD__22EAEC0F] PRIMARY KEY CLUSTERED ([SeqJOBAgd])
);
GO
CREATE NONCLUSTERED INDEX [GEP_JOBAGDIE1] ON [dbo].[GEP_JOBAGD] ([Codigo]);
GO
CREATE NONCLUSTERED INDEX [IDX_GEP_JOBAGD] ON [dbo].[GEP_JOBAGD] ([SEQJOB]);
GO
CREATE NONCLUSTERED INDEX [IDX_GEP_JOBAGD_01] ON [dbo].[GEP_JOBAGD] ([Codigo], [Tipo]);
GO
ALTER TABLE [dbo].[GEP_JOBAGD] ADD CONSTRAINT [FK__GEP_JOBAG__SEQJO__5967C3A8] FOREIGN KEY ([SEQJOB]) REFERENCES [dbo].[GEP_JOBCAD] ([SeqJOB]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_JobAgdExecLog
   Criada em ..: 2015-09-14
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_JobAgdExecLog] (
    [DtaExecucao] datetime NULL,
    [Codigo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLog] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoErro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Argumento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqJobAgd] numeric(18,0) NULL,
    [Descricao] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROTENTATIVA] numeric(2,0) NULL,
    [DTAFINALEXECUCAO] datetime NULL,
    [LINKSTR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINKNRO2] numeric(18,0) NULL,
    [LINKNRO1] numeric(18,0) NULL,
    [PRIORIDADE] numeric(2,0) NULL
);
GO
CREATE NONCLUSTERED INDEX [XIE1GEP_JobAgdExecLo] ON [dbo].[GEP_JobAgdExecLog] ([DtaExecucao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_JOBCAD
   Criada em ..: 2014-09-15
   Alterada em : 2020-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_JOBCAD] (
    [SeqJOB] numeric(18,0) NOT NULL,
    [Codigo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LICENCA] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Argumento] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESTIMULOFILA] numeric(1,0) NULL,
    [TIPOJOB] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBJETIVO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RESTRICOES] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRIORIDADEFILA] numeric(1,0) NULL,
    CONSTRAINT [PK__GEP_JOBCAD__23DF1048] PRIMARY KEY CLUSTERED ([SeqJOB])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_JOBFILA
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_JOBFILA] (
    [SeqJOBFILA] numeric(18,0) NOT NULL,
    [NroZUMBI] numeric(2,0) NULL,
    [DtaExecucao] datetime NULL,
    [Prioridade] numeric(2,0) NULL,
    [Codigo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINKNro1] numeric(18,0) NULL,
    [LINKNro2] numeric(18,0) NULL,
    [Argumento] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUSRSolicitaNTE] numeric(18,0) NULL,
    [DtaEXECORIGINAL] datetime NULL,
    [NroTENTATIVA] numeric(2,0) NULL,
    [FILAWAIT] numeric(1,0) NULL,
    [SeqJobAgd] numeric(18,0) NULL,
    [INDAVISOINDIVIDUALSM] numeric(1,0) NULL,
    CONSTRAINT [PK__GEP_JOBFILA__24D33481] PRIMARY KEY CLUSTERED ([SeqJOBFILA])
);
GO
CREATE NONCLUSTERED INDEX [GEP_JOBFILAIE2] ON [dbo].[GEP_JOBFILA] ([DtaExecucao]);
GO
ALTER TABLE [dbo].[GEP_JOBFILA] ADD CONSTRAINT [FK__GEP_JOBFI__SeqJo__0F8DFED8] FOREIGN KEY ([SeqJobAgd]) REFERENCES [dbo].[GEP_JOBAGD] ([SeqJOBAgd]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_JOBFILALOG
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_JOBFILALOG] (
    [SeqJOBFILA] numeric(18,0) NOT NULL,
    [DtaExecucao] datetime NULL,
    [CodERRO] numeric(18,0) NULL,
    [Descricao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaMORTE] datetime NULL,
    CONSTRAINT [PK__GEP_JOBFILALOG__25C758BA] PRIMARY KEY CLUSTERED ([SeqJOBFILA])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_JobMonitor
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_JobMonitor] (
    [JobId] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Par] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Inicio] datetime NULL,
    [UltOk] datetime NULL,
    [Fim] datetime NULL,
    [LogNormal] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LogAdv] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LogErro] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLog] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Retorno] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GEP_JobMonitor__7C7B2A64] PRIMARY KEY CLUSTERED ([JobId])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_JOBNOTIFICAR
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_JOBNOTIFICAR] (
    [SeqJOBAgd] numeric(18,0) NOT NULL,
    [SeqNotificar] numeric(4,0) NOT NULL,
    [Email] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [IndENVEMAIL] numeric(1,0) NULL,
    [IndENVSMS] numeric(1,0) NULL,
    [IndENVCRM] numeric(1,0) NULL,
    [IndENVREDESOC] numeric(1,0) NULL,
    [IndEXECUCAO] numeric(1,0) NULL,
    [IndERRO] numeric(1,0) NULL,
    [AssuntoAdicional] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndADVERTENCIA] numeric(1,0) NULL,
    [IndGERARESULTADO] numeric(1,0) NULL,
    [Resultado] numeric(6,0) NULL,
    CONSTRAINT [PK__GEP_JOBNOTIFICAR__27AFA12C] PRIMARY KEY CLUSTERED ([SeqJOBAgd], [SeqNotificar])
);
GO
CREATE NONCLUSTERED INDEX [XIF2GEP_JOBNOTIFICAR] ON [dbo].[GEP_JOBNOTIFICAR] ([Resultado]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_Notificar
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_Notificar] (
    [SeqProcesso] numeric(18,0) NOT NULL,
    [SeqNotificar] decimal(4,0) NOT NULL,
    [CodUsuario] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Email] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InterVISION] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Assunto] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PrioridadeErro] decimal(1,0) NULL,
    [PrioridadeExec] decimal(1,0) NULL,
    CONSTRAINT [PK__GEP_Notificar__5A1B2568] PRIMARY KEY CLUSTERED ([SeqProcesso], [SeqNotificar])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_NotificarPK] ON [dbo].[GEP_Notificar] ([SeqProcesso], [SeqNotificar]);
GO
CREATE NONCLUSTERED INDEX [GEP_NotificarIF1] ON [dbo].[GEP_Notificar] ([SeqProcesso]);
GO
ALTER TABLE [dbo].[GEP_Notificar] ADD CONSTRAINT [FK__GEP_Notif__SeqPr__36C7C78A] FOREIGN KEY ([SeqProcesso]) REFERENCES [dbo].[GEP_Processo] ([SeqProcesso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_PABX
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_PABX] (
    [SeqPabx] numeric(18,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Ramal] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefone] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaProc] datetime NULL,
    CONSTRAINT [PK__GEP_PABX__EE3F544DD0CB2DB5] PRIMARY KEY CLUSTERED ([SeqPabx])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGEP_PABX] ON [dbo].[GEP_PABX] ([SeqPabx]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ParEnvia
   Criada em ..: 2013-03-24
   Alterada em : 2020-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ParEnvia] (
    [Destino] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SessaoCnx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndEnvPessoa] numeric(1,0) NULL,
    [IndEnvProspect] numeric(1,0) NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [INDENVTODAS] numeric(1,0) NULL,
    CONSTRAINT [PK__GEP_ParEnvia__5B0F49A1] PRIMARY KEY CLUSTERED ([Destino])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_ParEnviaPK] ON [dbo].[GEP_ParEnvia] ([Destino]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ParRecebe
   Criada em ..: 2013-03-24
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ParRecebe] (
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [IndAlteraPessoa] numeric(1,0) NULL,
    [IndIdentPesCNPJ] numeric(1,0) NULL,
    [IndSobrepoeFone] numeric(1,0) NULL,
    [IndEnviaOut] numeric(1,0) NULL,
    [DestinoOut] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdDiasRemove] numeric(4,0) NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [INDALTERAPESSOAEXT] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDMANTEMORDEMFONES] numeric(1,0) NULL,
    [INDMANTEMATIVO] numeric(1,0) NULL,
    CONSTRAINT [PK__GEP_ParRecebe__5C036DDA] PRIMARY KEY CLUSTERED ([Origem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_ParRecebePK] ON [dbo].[GEP_ParRecebe] ([Origem]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ParRecEnvia
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ParRecEnvia] (
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Destino] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GEP_ParRecEnvia__5CF79213] PRIMARY KEY CLUSTERED ([Origem], [Destino])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_ParRecEnviaPK] ON [dbo].[GEP_ParRecEnvia] ([Origem], [Destino]);
GO
CREATE NONCLUSTERED INDEX [GEP_ParRecEnviaIF1] ON [dbo].[GEP_ParRecEnvia] ([Origem]);
GO
CREATE NONCLUSTERED INDEX [GEP_ParRecEnviaIF2] ON [dbo].[GEP_ParRecEnvia] ([Destino]);
GO
ALTER TABLE [dbo].[GEP_ParRecEnvia] ADD CONSTRAINT [FK__GEP_ParRe__Orige__37BBEBC3] FOREIGN KEY ([Origem]) REFERENCES [dbo].[GEP_ParRecebe] ([Origem]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[GEP_ParRecEnvia] ADD CONSTRAINT [FK__GEP_ParRe__Desti__38B00FFC] FOREIGN KEY ([Destino]) REFERENCES [dbo].[GEP_ParEnvia] ([Destino]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ProcControle
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ProcControle] (
    [Servidor] decimal(3,0) NOT NULL,
    [DtaUltProc] datetime NULL,
    [DtaInicio] datetime NULL,
    [DtaFim] datetime NULL,
    [EmExecucao] numeric(1,0) NULL,
    CONSTRAINT [PK__GEP_ProcControle__5DEBB64C] PRIMARY KEY CLUSTERED ([Servidor])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_ProcControlPK] ON [dbo].[GEP_ProcControle] ([Servidor]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_Processo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_Processo] (
    [SeqProcesso] numeric(18,0) NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SistemaOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Conexao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodProcesso] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Prioridade] decimal(2,0) NULL,
    [Aplicativo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExeDta] datetime NULL,
    [ExeDiaSemana] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExeDiaMes] decimal(2,0) NULL,
    [ExeHora] datetime NULL,
    [Servidor] decimal(3,0) NULL,
    [ExeMinuto] decimal(4,2) NULL,
    [UltimaExecucao] datetime NULL,
    [Argumento] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SuspendeAposExec] decimal(1,0) NULL,
    [DNet] numeric(1,0) NULL,
    [PrioridadeThread] numeric(1,0) NULL,
    [ThreadNro] numeric(4,0) NULL,
    CONSTRAINT [PK__GEP_Processo__5EDFDA85] PRIMARY KEY CLUSTERED ([SeqProcesso])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_ProcessoPK] ON [dbo].[GEP_Processo] ([SeqProcesso]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_ProcImport
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_ProcImport] (
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACAO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAG] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaProcessar] datetime NULL,
    [Prioridade] numeric(1,0) NULL,
    [Chave] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DADO] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [GEP_PROCIMPORTIE1] ON [dbo].[GEP_ProcImport] ([Status], [DtaProcessar], [Prioridade]);
GO
CREATE NONCLUSTERED INDEX [GEP_PROCIMPORTIE2] ON [dbo].[GEP_ProcImport] ([Chave]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_SyncUsrSat
   Criada em ..: 2017-01-22
   Alterada em : 2026-03-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_SyncUsrSat] (
    [Destino] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Usr] numeric(8,0) NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [IndProcessado] numeric(1,0) NULL,
    [IndProcessoWait] numeric(1,0) NULL,
    [DtaGeracao] datetime NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NULL,
    [KS1] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KS2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Operacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NivelSeguranca] decimal(3,0) NULL,
    [DtaColeta] datetime NULL,
    [SEQSYNCUSRSAT] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [FCMTOKEN] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GEP_Sync__7412625419799C95] PRIMARY KEY CLUSTERED ([SEQSYNCUSRSAT])
);
GO
CREATE NONCLUSTERED INDEX [GEP_SyncUsrSatIE1] ON [dbo].[GEP_SyncUsrSat] ([Destino], [Usr], [Tabela], [IndProcessado], [DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [IDX_GEP_SYNC_IE4] ON [dbo].[GEP_SyncUsrSat] ([Destino], [Usr], [Tabela], [IndProcessado], [IndProcessoWait], [Operacao]);
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190205-095930] ON [dbo].[GEP_SyncUsrSat] ([Destino], [Usr], [IndProcessado]);
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190205-100126] ON [dbo].[GEP_SyncUsrSat] ([Destino], [Usr], [Tabela], [KN1], [KN2], [Operacao]);
GO
CREATE NONCLUSTERED INDEX [IDX_GEPSYNCUSRSAT_1] ON [dbo].[GEP_SyncUsrSat] ([Destino], [Usr], [Tabela], [Operacao]) INCLUDE ([KN1], [KS1], [KS2], [SEQSYNCUSRSAT]);
GO
CREATE NONCLUSTERED INDEX [GEP_SYNCUSRSATIE4] ON [dbo].[GEP_SyncUsrSat] ([Usr], [IndProcessado], [Operacao], [Destino]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [gep_syncusrsat_dedup] ON [dbo].[GEP_SyncUsrSat] ([Usr], [Tabela], [KN1], [KN2], [KS1], [KS2]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.Gep_UsrPabx
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[Gep_UsrPabx] (
    [SeqUsrPabx] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [PabxLogin] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PabxSenha] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campanha] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__Gep_UsrP__6B10D4A5041BEB07] PRIMARY KEY CLUSTERED ([SeqUsrPabx])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGep_UsrPabx] ON [dbo].[Gep_UsrPabx] ([SeqUsrPabx]);
GO
CREATE NONCLUSTERED INDEX [XIF1Gep_UsrPabx] ON [dbo].[Gep_UsrPabx] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[Gep_UsrPabx] ADD CONSTRAINT [FK__Gep_UsrPa__SeqUs__40BB4618] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GEP_UsrSat
   Criada em ..: 2012-05-02
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GEP_UsrSat] (
    [Destino] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Usr] numeric(8,0) NOT NULL,
    [IDSatelite] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInicioRec] datetime NULL,
    [DtaUltimoRec] datetime NULL,
    [DtaInicioEnvio] datetime NULL,
    [DtaUltimoEnvio] datetime NULL,
    [IndSateliteLivre] numeric(1,0) NULL,
    [SeqSyncUsrSat] numeric(18,0) NULL,
    [UltServicoEnviado] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqLogUsr] numeric(18,0) NULL,
    [SeqUsrLog] numeric(18,0) NULL,
    [SQL] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VersaoSO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VersaoApp] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqMobileConfig] numeric(10,0) NULL,
    CONSTRAINT [PK__GEP_UsrSat__60C822F7] PRIMARY KEY CLUSTERED ([Destino], [Usr])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GEP_UsrSatPK] ON [dbo].[GEP_UsrSat] ([Destino], [Usr]);
GO
CREATE NONCLUSTERED INDEX [XIF1GEP_UsrSat] ON [dbo].[GEP_UsrSat] ([SeqMobileConfig]);
GO
ALTER TABLE [dbo].[GEP_UsrSat] ADD CONSTRAINT [FK__GEP_UsrSa__SeqMo__6033F171] FOREIGN KEY ([SeqMobileConfig]) REFERENCES [dbo].[GE_MobileConfig] ([SeqMobileConfig]);
GO

