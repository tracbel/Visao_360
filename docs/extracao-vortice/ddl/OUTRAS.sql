/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo OUTRAS
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.andre
   Criada em ..: 2019-08-15
   Alterada em : 2019-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[andre] (
    [seq] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.CONTAS
   Criada em ..: 2017-07-21
   Alterada em : 2017-07-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[CONTAS] (
    [codconta1] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo1] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [codconta2] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo2] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [codconta3] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo3] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ContaReduzida] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescrContaReduzida] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [CONTASX1] ON [dbo].[CONTAS] ([codconta1]);
GO
CREATE NONCLUSTERED INDEX [CONTASX3] ON [dbo].[CONTAS] ([codconta3]);
GO
CREATE NONCLUSTERED INDEX [CONTASX2] ON [dbo].[CONTAS] ([codconta2]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.CONTAS2
   Criada em ..: 2017-07-21
   Alterada em : 2017-07-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[CONTAS2] (
    [SEQ] int IDENTITY(1,1) NOT NULL,
    [CONTAREDUZIDA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODCONTA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQCONTA] numeric(18,0) NULL,
    [SEQCONTAPAI] numeric(18,0) NULL,
    [CODCONTAPAI] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORDEM] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.DUAL
   Criada em ..: 2011-12-19
   Alterada em : 2011-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[DUAL] (
    [N] decimal(20,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.nfsaida$
   Criada em ..: 2012-01-06
   Alterada em : 2012-01-06
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[nfsaida$] (
    [ORIGEM] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROEMPRESA] float NULL,
    [NUMERO] float NULL,
    [SERIE] float NULL,
    [PESSOALINKNRO] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESSOALINK] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROCGCCPF] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DIGCGCCPF] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEGMENTO] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEPARTAMENTO] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROCHASSI] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OPERACAO] float NULL,
    [TIPOVENDA] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [formapagto] float NULL,
    [VENDEDOR] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROVENDEDOR] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTPEDIDO] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROPEDIDO] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTEMISSAO] datetime NULL,
    [COD#SIT#] float NULL,
    [USUARIO] float NULL,
    [dtaalteracao] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [obs] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.SYSCONVERT1
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[SYSCONVERT1] (
    [INSTR] char(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTSTR] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.SYSCONVERT2
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[SYSCONVERT2] (
    [SQSTYPE] tinyint NULL,
    [SYSTYPE] char(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.SYSCONVERT3
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[SYSCONVERT3] (
    [INVAL] smallint NULL,
    [OUTVAL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.sysdiagrams
   Criada em ..: 2013-03-27
   Alterada em : 2017-01-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[sysdiagrams] (
    [name] sysname COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [principal_id] int NOT NULL,
    [diagram_id] int IDENTITY(1,1) NOT NULL,
    [version] int NULL,
    [definition] varbinary(max) NULL,
    CONSTRAINT [PK__sysdiagrams__68143F04] PRIMARY KEY CLUSTERED ([diagram_id]),
    CONSTRAINT [UK_principal_name] UNIQUE NONCLUSTERED ([principal_id], [name])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.SYSDUMMY
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[SYSDUMMY] (
    [a] char(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [b] smallint NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.TEMP
   Criada em ..: 2016-09-24
   Alterada em : 2016-09-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[TEMP] (
    [CPF] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CARTEIRA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQCARTEIRA] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.teste
   Criada em ..: 2019-05-14
   Alterada em : 2019-05-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[teste] (
    [CHASSI_DO_EQUIPAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.teste333
   Criada em ..: 2024-12-30
   Alterada em : 2024-12-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[teste333] (
    [SeqPessoa] numeric(10,0) NULL,
    [SeqPesFone] numeric(18,0) NULL,
    [DDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero] numeric(12,0) NULL,
    [IndFonePref] numeric(1,0) NULL,
    [NroFonePessoa] numeric(1,0) NULL,
    [Complemento] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Foni] int NULL,
    [Seqf] numeric(1,0) NULL,
    [DDDF] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [rank] int NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.testepiv
   Criada em ..: 2023-03-03
   Alterada em : 2023-03-03
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[testepiv] (
    [ano] int NULL,
    [produto] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [valor] decimal(10,2) NULL,
    [grupo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.usuarios$
   Criada em ..: 2011-12-20
   Alterada em : 2011-12-20
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[usuarios$] (
    [Código Spress] float NULL,
    [Funcionário] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.ww
   Criada em ..: 2017-05-30
   Alterada em : 2017-05-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[ww] (
    [Nome] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sexo] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Est] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Endereco] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nro] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Complemento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEP] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone_1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone_2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CPF_CNPJ] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROCGCCPF] numeric(15,0) NULL,
    [DIGCGCCPF] numeric(2,0) NULL,
    [e_mail] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atividade] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Notas] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

