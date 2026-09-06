/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo OUT
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.OUT_Contato
   Criada em ..: 2018-08-15
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[OUT_Contato] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] numeric(4,0) NOT NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkWeb] numeric(1,0) NULL,
    [Email] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascimento] datetime NULL,
    [EstadoCivil] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEXO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] numeric(12,0) NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] numeric(12,0) NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] numeric(12,0) NULL,
    [Contato] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Saudacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DigCPF] numeric(2,0) NULL,
    [CPF] numeric(18,0) NULL,
    [RG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AreaAtuacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoContato] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_OUT_Contato] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqContato])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.OUT_Log
   Criada em ..: 2014-08-12
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[OUT_Log] (
    [SeqLog] numeric(18,0) NOT NULL,
    [Tabela] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Chave1N] numeric(18,0) NULL,
    [Chave2N] numeric(18,0) NULL,
    [Chave1C] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TpOper] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaIns] datetime NULL,
    [UsrIns] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsIns] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OutOk] numeric(1,0) NULL,
    [OutDta] datetime NULL,
    CONSTRAINT [PK_OUT_Log] PRIMARY KEY CLUSTERED ([SeqLog])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.OUT_Pessoa
   Criada em ..: 2018-08-15
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[OUT_Pessoa] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [PessoaLinkOrigem] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeRazao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fantasia] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Versao] decimal(2,0) NULL,
    [Status] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAtivacao] datetime NULL,
    [FisicaJuridica] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sexo] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroLogradouro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cep] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CxPostal] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] decimal(12,0) NULL,
    [FoneCmpl1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] decimal(12,0) NULL,
    [FoneCmpl2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD3] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro3] decimal(12,0) NULL,
    [FoneCmpl3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] decimal(12,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Email] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HomePage] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EstadoCivil] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atividade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RendaFaturamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GrauInstrucao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Porte] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInativacao] datetime NULL,
    [UsuInativacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsInativacao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefonema] numeric(1,0) NULL,
    [Correspondencia] numeric(1,0) NULL,
    [RecebeEmail] numeric(1,0) NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [IndContribICMS] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_OUT_Pessoa] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.OUT_PessoaLink
   Criada em ..: 2013-11-01
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[OUT_PessoaLink] (
    [Pessoalink] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [PessoaLinkNro] numeric(18,0) NULL,
    CONSTRAINT [PK_OUT_PessoaLink] PRIMARY KEY CLUSTERED ([Pessoalink], [Origem])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.OUT_PessoaRelacao
   Criada em ..: 2018-08-15
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[OUT_PessoaRelacao] (
    [SeqPrincipal] numeric(8,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [TipoRelacionamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Link] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_OUT_PessoaRelacao] PRIMARY KEY CLUSTERED ([SeqPrincipal], [SeqPessoa], [TipoRelacionamento])
);
GO

