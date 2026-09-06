/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IMP
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_EMAIL
   Criada em ..: 2018-03-20
   Alterada em : 2020-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_EMAIL] (
    [PESSOALINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PESSOALINK] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAILLINK] nvarchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [EMAIL] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [INDEMUSO] numeric(1,0) NULL,
    [INDPREFERENCIAL] numeric(1,0) NULL,
    [INDUSOMKT] numeric(1,0) NULL,
    [INDUSOPESSOAL] numeric(1,0) NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NOT NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDEMAIL] numeric(18,0) IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK__IMP_EMAI__B4CA2AEB75680080] PRIMARY KEY CLUSTERED ([IDEMAIL])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_EMAIL] ON [dbo].[IMP_EMAIL] ([EMAILLINK]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_Evento
   Criada em ..: 2017-02-03
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_Evento] (
    [IdEvento] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Processo] numeric(18,0) NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PessoaLinkAd] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [NroEmpresa] numeric(10,0) NULL,
    [Evento] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Resultado] numeric(6,0) NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaEvento] datetime NULL,
    [Valor] numeric(14,2) NULL,
    [Valor2] numeric(14,2) NULL,
    [Qtde] numeric(10,2) NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Detalhe] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InfoAdicional] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkDocto] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkSerie] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNroEmpresa] numeric(6,0) NULL,
    [LinkAdicional] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusImpSP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [StatusImpZ] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_Even__034EFC046674F3F3] PRIMARY KEY CLUSTERED ([IdEvento])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_EVENTO] ON [dbo].[IMP_Evento] ([DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE2IMP_Evento] ON [dbo].[IMP_Evento] ([StatusImpSP], [DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE3IMP_Evento] ON [dbo].[IMP_Evento] ([StatusImpZ], [DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE4IMP_EVENTO] ON [dbo].[IMP_Evento] ([StatusImpZ], [DtaGeracao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_FORMAPGTO
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_FORMAPGTO] (
    [IdFormaPgto] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Nronf] numeric(18,0) NOT NULL,
    [Serienf] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFormaPGTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESCFormaPGTO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [VLR] numeric(14,2) NOT NULL,
    [DtaVencto] datetime NOT NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_FORM__D6E14D4664EC38D9] PRIMARY KEY CLUSTERED ([IdFormaPgto])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_FORMAPGTO] ON [dbo].[IMP_FORMAPGTO] ([DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE2IMP_FORMAPGTO] ON [dbo].[IMP_FORMAPGTO] ([Origem], [NroEmpresa], [Nronf], [Serienf]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_LINKPESSOA
   Criada em ..: 2021-08-02
   Alterada em : 2021-08-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_LINKPESSOA] (
    [PESSOALINK] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CGCCPF] numeric(15,0) NULL,
    [NROCGCCPF] numeric(15,0) NULL,
    [DIGCGCCPF] numeric(5,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_LOGAPROVACAO
   Criada em ..: 2025-07-17
   Alterada em : 2025-07-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_LOGAPROVACAO] (
    [Seq] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [detalhe] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_LOGA__CA1E3C88B6220E47] PRIMARY KEY CLUSTERED ([Seq])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_NFS
   Criada em ..: 2017-02-03
   Alterada em : 2023-09-12
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_NFS] (
    [IdNFS] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [Nronf] numeric(18,0) NOT NULL,
    [Serienf] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresaVda] numeric(10,0) NULL,
    [NroCNPJCPF] numeric(13,0) NULL,
    [DigCNPJCPF] numeric(2,0) NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Segmento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoVenda] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CFOP] numeric(6,0) NULL,
    [CodOperacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Operacao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CanalVenda] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Setor] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Formapgto] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CondicaoPgto] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vendedor] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodVendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtapedido] datetime NULL,
    [Nropedido] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtaemissaonf] datetime NOT NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CodTransportador] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Transportador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracaoERP] datetime NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [WhereItem] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERCBASECOMISSAO] numeric(5,2) NULL,
    [IDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOTECARGA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAIIDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EVENTOCMPL] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOPEDIDO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDENTIFICADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROCPFVENDEDOR] numeric(11,0) NULL,
    [NROVOUCHER] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_NFS__0DD2EFAB913F832D] PRIMARY KEY CLUSTERED ([IdNFS])
);
GO
CREATE NONCLUSTERED INDEX [EXT_NFSIE4_0] ON [dbo].[IMP_NFS] ([Origem], [NroEmpresa], [Nronf], [Serienf]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_NFS] ON [dbo].[IMP_NFS] ([DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE3IMP_NFS] ON [dbo].[IMP_NFS] ([StatusIMP], [DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_NFS_0] ON [dbo].[IMP_NFS] ([IDNFSEXTERNO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_NFS_1] ON [dbo].[IMP_NFS] ([PAIIDNFSEXTERNO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_NFSCMPL
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_NFSCMPL] (
    [Idcmpl] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [Nronf] numeric(18,0) NOT NULL,
    [Serienf] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CMPL] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_NFSC__367F41070CDD0688] PRIMARY KEY CLUSTERED ([Idcmpl])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_NFSCMPL] ON [dbo].[IMP_NFSCMPL] ([DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE2IMP_NFSCMPL] ON [dbo].[IMP_NFSCMPL] ([Origem], [NroEmpresa], [Nronf], [Serienf]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_NFSItem
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_NFSItem] (
    [IdNFSItem] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [Nronf] numeric(18,0) NOT NULL,
    [Serienf] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroItem] numeric(6,0) NULL,
    [CodProduto] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NCM] numeric(10,0) NULL,
    [CodBarra] numeric(18,0) NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Marca] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFamilia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descproduto] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Qtde] numeric(10,3) NOT NULL,
    [VlrLiqItem] numeric(14,2) NOT NULL,
    [Vlrdescto] numeric(14,2) NULL,
    [VlrResult] numeric(14,2) NULL,
    [VlrCustoMkt] numeric(14,2) NULL,
    [VlrICM] numeric(14,2) NULL,
    [VlrImposto] numeric(14,2) NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Vendedor] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodVendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Identificador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [TIPO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOTECARGA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SETORITEM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRCUSTO] numeric(14,2) NULL,
    [IDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRICMSRETIDO] numeric(14,2) NULL,
    [VLRICMSSUBS] numeric(14,2) NULL,
    [VLRPIS] numeric(14,2) NULL,
    [VLRCOFINS] numeric(14,2) NULL,
    [NROCPFVENDEDOR] numeric(11,0) NULL,
    [VLRDESCTOVCHR] numeric(14,2) NULL,
    CONSTRAINT [PK__IMP_NFSI__E3AB6FE0D943DCFE] PRIMARY KEY CLUSTERED ([IdNFSItem])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_NFSITEM] ON [dbo].[IMP_NFSItem] ([DtaGeracao], [StatusIMP]);
GO
CREATE NONCLUSTERED INDEX [XIE2IMP_NFSItem] ON [dbo].[IMP_NFSItem] ([Origem], [NroEmpresa], [Nronf], [Serienf]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_NFSITEM_1] ON [dbo].[IMP_NFSItem] ([IDNFSEXTERNO], [LOTECARGA]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_OS
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_OS] (
    [IDOS] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [Nroos] numeric(18,0) NOT NULL,
    [SerieOS] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCNPJCPF] numeric(13,0) NULL,
    [DigCNPJCPF] numeric(2,0) NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Segmento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nrochassi] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Placa] varchar(9) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Combustivel] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodModelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Modelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorExterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorInterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Anofabricacao] numeric(4,0) NULL,
    [AnoModelo] numeric(4,0) NULL,
    [Dtavenda] datetime NULL,
    [Consultor] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipoos] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtaabertura] datetime NULL,
    [Dtaencerramento] datetime NULL,
    [Dtafechamento] datetime NULL,
    [VlrLiqPecas] numeric(14,2) NULL,
    [VlrLiqServicos] numeric(14,2) NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroDN] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Kilometragem] numeric(8,0) NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [WhereItem] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFamilia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDOSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SITUACAO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CODTIPOOS] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EVENTOCMPL] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAOERP] datetime NULL,
    [TIPOSERVICO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_OS__B87C231DD3D702B4] PRIMARY KEY CLUSTERED ([IDOS])
);
GO
CREATE NONCLUSTERED INDEX [EXT_OSAK1_0] ON [dbo].[IMP_OS] ([Origem], [NroEmpresa], [Nroos]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_OS] ON [dbo].[IMP_OS] ([DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE3IMP_OS] ON [dbo].[IMP_OS] ([StatusIMP], [DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_OS_0] ON [dbo].[IMP_OS] ([IDOSEXTERNO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_OSITEM
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_OSITEM] (
    [Iditem] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [Nroos] numeric(18,0) NOT NULL,
    [SerieOS] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroItem] numeric(6,0) NULL,
    [Tipoitem] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GrupoItem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Produtivo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [StatusItem] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Codigo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Qtde] numeric(10,2) NULL,
    [VlrLiquido] numeric(14,2) NULL,
    [Vlrdescto] numeric(14,2) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [IDOSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MAODEOBRA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRTOTITEM] numeric(14,2) NULL,
    [VLRTOTITEMBRUTO] numeric(14,2) NULL,
    [IDOS] numeric(18,0) NULL,
    CONSTRAINT [PK__IMP_OSIT__5F0501CC5A65BDA8] PRIMARY KEY CLUSTERED ([Iditem])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_OSITEM] ON [dbo].[IMP_OSITEM] ([StatusIMP], [DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE2IMP_OSITEM] ON [dbo].[IMP_OSITEM] ([Origem], [NroEmpresa], [Nroos], [SerieOS]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_OSITEM_0] ON [dbo].[IMP_OSITEM] ([IDOSEXTERNO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_OSITEM_1] ON [dbo].[IMP_OSITEM] ([Origem], [IDOSEXTERNO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_OSSOLIC
   Criada em ..: 2017-02-03
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_OSSOLIC] (
    [Idsolic] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [Nroos] numeric(18,0) NOT NULL,
    [SerieOS] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Codigo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [IDOSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_OSSO__491C2E9CC527C210] PRIMARY KEY CLUSTERED ([Idsolic])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_OSSOLIC] ON [dbo].[IMP_OSSOLIC] ([DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIE2IMP_OSSOLIC] ON [dbo].[IMP_OSSOLIC] ([Origem], [NroEmpresa], [Nroos], [SerieOS]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_Pedido
   Criada em ..: 2017-02-03
   Alterada em : 2020-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_Pedido] (
    [IdPedido] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCNPJCPF] numeric(13,0) NULL,
    [DigCNPJCPF] numeric(2,0) NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Segmento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] decimal(10,0) NOT NULL,
    [NroPedido] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CodOperacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Operacao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodVendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vendedor] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CanalVenda] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroPedCliente] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroPedidoERP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaPgto] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CondicaoPgto] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AtivoReceptivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaBaseFatura] datetime NULL,
    [NroDocto] decimal(12,0) NULL,
    [Serie] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaPedido] datetime NULL,
    [DtaValidade] datetime NULL,
    [DtaFechamento] datetime NULL,
    [DtaProxContato] datetime NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [StatusCmpl] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndEstRservado] numeric(1,0) NULL,
    [IndEnvioERP] numeric(1,0) NULL,
    [DtaEnvioERP] datetime NULL,
    [DtaAlteracaoERP] datetime NULL,
    [ObsNF] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsInterna] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsDesconto] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeAssinatura] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssinaturaDesc] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssinaturaDta] datetime NULL,
    [EntradaSaida] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MotivoVP] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoEntrega] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoFrete] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodTransportador] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Transportador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PedidoLinkNro] numeric(18,0) NULL,
    [PedidoLinkStr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [PERCBASECOMISSAO] numeric(5,2) NULL,
    [PEDIDOLINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAILINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAILINKNRO] numeric(18,0) NULL,
    [PAILINKSTR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO] numeric(18,0) NULL,
    [LOTECARGA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EVENTOCMPL] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRDESCADICIONAL] numeric(15,2) NULL,
    CONSTRAINT [PK__IMP_Pedi__9D335DC302FA6484] PRIMARY KEY CLUSTERED ([IdPedido])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIMP_Pedido] ON [dbo].[IMP_Pedido] ([IdPedido]);
GO
CREATE NONCLUSTERED INDEX [XIE1IMP_Pedido] ON [dbo].[IMP_Pedido] ([NroPedido]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_PedidoItem
   Criada em ..: 2017-02-03
   Alterada em : 2021-07-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_PedidoItem] (
    [IdPedidoItem] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [NroPedido] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroItem] numeric(6,0) NULL,
    [CodTabelaPreco] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodProduto] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Marca] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFamilia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descproduto] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PrecoTabela] decimal(15,2) NULL,
    [Qtde] decimal(10,2) NULL,
    [QtdeAtendida] decimal(10,2) NULL,
    [IndEstRservado] numeric(1,0) NULL,
    [VlrUnitario] decimal(15,4) NULL,
    [VlrDescUnitCml] decimal(10,3) NULL,
    [VlrDescUnitAuto] decimal(10,3) NULL,
    [VlrDescTotal] decimal(15,2) NULL,
    [DescConciliado] numeric(1,0) NULL,
    [VlrBaseICM] decimal(15,2) NULL,
    [VlrICM] decimal(15,2) NULL,
    [AliqICM] decimal(15,2) NULL,
    [VlrICMSubs] decimal(15,2) NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MotivoVP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsItem] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndSugestao] char(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [LOTECARGA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_Pedi__8E9C2D7E60D1F365] PRIMARY KEY CLUSTERED ([IdPedidoItem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIMP_PedidoItem] ON [dbo].[IMP_PedidoItem] ([IdPedidoItem]);
GO
CREATE NONCLUSTERED INDEX [XIE1IMP_PedidoItem] ON [dbo].[IMP_PedidoItem] ([NroPedido]);
GO
CREATE NONCLUSTERED INDEX [XIE2IMP_PEDIDOITEM] ON [dbo].[IMP_PedidoItem] ([NroPedido], [Origem], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [XIE3IMP_PEDIDOITEM] ON [dbo].[IMP_PedidoItem] ([NroPedido], [Origem], [NroEmpresa], [LOTECARGA]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_PESSOA
   Criada em ..: 2017-02-03
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_PESSOA] (
    [IdPessoa] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NomeRazao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fantasia] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PalavraChave] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEXO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAIS] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroLogradouro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEP] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CxPostal] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD3] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneCmpl3] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCNPJCPF] numeric(13,0) NULL,
    [DigCNPJCPF] numeric(2,0) NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNASC] datetime NULL,
    [Email] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HomePage] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EstadoCivil] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atividade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RendaFaturamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GrauInstrucao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Porte] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodRegiao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Regiao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodRota] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ROTA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PessoaLinkVincOrig] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PessoaLinkVinc] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [ProblemaCredito] numeric(1,0) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [CLASSES] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRLIMITECREDITO] numeric(14,2) NULL,
    [SITUACAOCREDITO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRSALDOCREDITO] numeric(14,2) NULL,
    [STATUSSINTEGRA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DADOADICIONAL1] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DADOADICIONAL2] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEPTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CARTEIRA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_PESS__7061465D093962E5] PRIMARY KEY CLUSTERED ([IdPessoa])
);
GO
CREATE NONCLUSTERED INDEX [EXT_PESSOAIE1_0] ON [dbo].[IMP_PESSOA] ([PessoaLinkOrigem], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [EXT_PESSOAIE3_0] ON [dbo].[IMP_PESSOA] ([PessoaLinkOrigem], [NroEmpresa], [Pessoalink]);
GO
CREATE NONCLUSTERED INDEX [EXT_PESSOAIE4_0] ON [dbo].[IMP_PESSOA] ([NroCNPJCPF]);
GO
CREATE NONCLUSTERED INDEX [IDX_IMP_PESSOA] ON [dbo].[IMP_PESSOA] ([StatusIMP], [DtaGeracao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_PESSOACONTATO
   Criada em ..: 2018-03-20
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_PESSOACONTATO] (
    [IDPESSOACTTO] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [PESSOALINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PESSOALINK] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONTATOLINK] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CONTATO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SAUDACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEXO] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DATANASCIMENTO] datetime NULL,
    [EMAIL] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOCONTATO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [AREAATUACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CPF] numeric(12,0) NULL,
    [FONEDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONENRO1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONEDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONENRO2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SKYPE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAPEL1] nvarchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAPEL2] nvarchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAPEL3] nvarchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBSERVACAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NOT NULL,
    [STATUSIMPSP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDWHATSAPPF1] numeric(1,0) NULL,
    [INDWHATSAPPF2] numeric(1,0) NULL,
    CONSTRAINT [PK__IMP_PESS__4F4354735DE1D552] PRIMARY KEY CLUSTERED ([IDPESSOACTTO])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_PESSOAFONE
   Criada em ..: 2019-12-19
   Alterada em : 2019-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_PESSOAFONE] (
    [IDPESSOAFONE] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [PESSOALINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PESSOALINK] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SEQPESSOA] numeric(10,0) NULL,
    [FONELINK] numeric(12,0) NULL,
    [OPERACAO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO] numeric(12,0) NOT NULL,
    [COMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOFONE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDUSOMKT] numeric(1,0) NULL,
    [INDFONEPREF] numeric(1,0) NULL,
    [INDEMUSO] numeric(1,0) NULL,
    [INDWHATSAPP] numeric(1,0) NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NOT NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    CONSTRAINT [PK__IMP_PESS__4EBB23887A143455] PRIMARY KEY CLUSTERED ([IDPESSOAFONE])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_PRODUTO
   Criada em ..: 2018-03-06
   Alterada em : 2021-07-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_PRODUTO] (
    [IDPRODUTO] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [OPERACAO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CODPRODUTO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TIPO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CODBARRA] numeric(18,0) NULL,
    [DESCRICAO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODFAMILIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FAMILIA] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAOCOMPLETA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NCM] numeric(10,0) NULL,
    [PRECOPUBLICO] numeric(14,2) NULL,
    [PRECO1] numeric(14,2) NULL,
    [PRECO2] numeric(14,2) NULL,
    [FORNECEDORPRINCIPAL] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA1] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA3] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA4] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA5] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA6] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVEPRODUTOERP] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NOT NULL,
    [EMUSO] numeric(1,0) NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDEESTOQUE] numeric(10,2) NULL,
    [CUSTOESTOQUE] numeric(15,4) NULL,
    [DTAESTOQUE] datetime NULL,
    CONSTRAINT [PK__IMP_PROD__ED0C5933C1DF0505] PRIMARY KEY CLUSTERED ([IDPRODUTO])
);
GO
CREATE NONCLUSTERED INDEX [EXT_PRODUTOAK1_0] ON [dbo].[IMP_PRODUTO] ([CODPRODUTO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_REL_TBA101
   Criada em ..: 2025-10-29
   Alterada em : 2026-05-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_REL_TBA101] (
    [codusuario] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMPRESA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEN] nvarchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [JD_QUOTE] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_VENDA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_EQUIPAMENTO] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO] nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPESSOA] int NULL,
    [NOMERAZAO] nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNPJCPF] nvarchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINHA_CREDITO] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INST_FINANCEIRA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FINANC_CHASSI] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ULT_RESULTADO] nvarchar(400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTA_FATURAMENTO_D] datetime NULL,
    [NF_N] nvarchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTA_PED_D] datetime NULL,
    [DTA_PRENCH_FORM_D] datetime NULL,
    [VALOR_N] decimal(18,2) NULL,
    [DTA_ULT_ANDAMENTO_D] datetime NULL,
    [ULT_HISTORICO_L] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO_N] int NULL,
    [CARTEIRA] nvarchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CIDADE] nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_PEDIDO_HIST_D] datetime NULL,
    [CAMPANHA] nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSODNA] int NULL,
    [DTAPROVADO] nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [IX_IMP_REL_TBA101_codusuario] ON [dbo].[IMP_REL_TBA101] ([codusuario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_REL_TBA101_PG2
   Criada em ..: 2026-05-27
   Alterada em : 2026-05-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_REL_TBA101_PG2] (
    [codusuario] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMPRESA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEN] nvarchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [JD_QUOTE] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMAR] decimal(8,0) NULL,
    [TIPO_VENDA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_EQUIPAMENTO] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO] nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPESSOA] int NULL,
    [NOMERAZAO] nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNPJCPF] nvarchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINHA_CREDITO] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INST_FINANCEIRA] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FINANC_CHASSI] nvarchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ULT_RESULTADO] nvarchar(400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTA_FATURAMENTO_D] datetime NULL,
    [NF_N] nvarchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTA_PED_D] datetime NULL,
    [DTA_PRENCH_FORM_D] datetime NULL,
    [VALOR_N] decimal(18,2) NULL,
    [DTA_ULT_ANDAMENTO_D] datetime NULL,
    [ULT_HISTORICO_L] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO_N] int NULL,
    [CARTEIRA] nvarchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CIDADE] nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_PEDIDO_HIST_D] datetime NULL,
    [CAMPANHA] nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSODNA] int NULL,
    [DTAPROVADO] nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_MARCADO_ENTREGUE] nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [IX_IMP_REL_TBA101_PG2_codusuario] ON [dbo].[IMP_REL_TBA101_PG2] ([codusuario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_Titulo
   Criada em ..: 2017-02-03
   Alterada em : 2021-08-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_Titulo] (
    [idTitulo] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [NroCNPJ] numeric(18,0) NULL,
    [DigCNPJ] numeric(2,0) NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Especie] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroTitulo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroDocto] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoCobranca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndAtivo] numeric(1,0) NULL,
    [IndQuitado] numeric(1,0) NULL,
    [IndCobrJuridica] numeric(1,0) NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroBanco] numeric(6,0) NULL,
    [LocalCobranca] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroTituloBanco] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaEmissao] datetime NULL,
    [DtaVenctoOrig] datetime NULL,
    [DtaVencto] datetime NULL,
    [VlrOriginal] numeric(14,2) NULL,
    [VlrAcrescimo] numeric(14,2) NULL,
    [VlrAbatimento] numeric(14,2) NULL,
    [VlrPago] numeric(14,2) NULL,
    [VlrMov] numeric(14,2) NULL,
    [Movimento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltPgto] datetime NULL,
    [DtaQuitacao] datetime NULL,
    [DtaUltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdPessoa] numeric(18,0) NULL,
    [CHAVESTR1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODBARRAS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [dtaimport] datetime NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IMP_Titu__A3113E5716634512] PRIMARY KEY CLUSTERED ([idTitulo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIMP_Titulo] ON [dbo].[IMP_Titulo] ([idTitulo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_Titulo_bkp_11_04
   Criada em ..: 2023-04-11
   Alterada em : 2023-04-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_Titulo_bkp_11_04] (
    [idTitulo] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NOT NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [NroCNPJ] numeric(18,0) NULL,
    [DigCNPJ] numeric(2,0) NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Especie] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroTitulo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroDocto] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoCobranca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndAtivo] numeric(1,0) NULL,
    [IndQuitado] numeric(1,0) NULL,
    [IndCobrJuridica] numeric(1,0) NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroBanco] numeric(6,0) NULL,
    [LocalCobranca] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroTituloBanco] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaEmissao] datetime NULL,
    [DtaVenctoOrig] datetime NULL,
    [DtaVencto] datetime NULL,
    [VlrOriginal] numeric(14,2) NULL,
    [VlrAcrescimo] numeric(14,2) NULL,
    [VlrAbatimento] numeric(14,2) NULL,
    [VlrPago] numeric(14,2) NULL,
    [VlrMov] numeric(14,2) NULL,
    [Movimento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltPgto] datetime NULL,
    [DtaQuitacao] datetime NULL,
    [DtaUltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdPessoa] numeric(18,0) NULL,
    [CHAVESTR1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODBARRAS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [dtaimport] datetime NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_VEICULO
   Criada em ..: 2017-02-03
   Alterada em : 2021-07-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_VEICULO] (
    [Idveiculo] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCNPJCPF] numeric(13,0) NULL,
    [DigCNPJCPF] numeric(2,0) NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroChassi] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroChassiRed] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoVeiculo] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Placa] varchar(9) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Combustivel] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFamilia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodModelo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Modelo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorExterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorInterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Potencia] numeric(4,0) NULL,
    [QtdeEixo] numeric(2,0) NULL,
    [EstadoVenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaPgto] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Financiador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CanalVenda] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nronf] numeric(18,0) NULL,
    [Serienf] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Anofabricacao] numeric(4,0) NULL,
    [AnoModelo] numeric(4,0) NULL,
    [Dtavenda] datetime NULL,
    [Dtaprevquitacao] datetime NULL,
    [VlrVenda] numeric(14,2) NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KMAtual] numeric(8,0) NULL,
    [DtaKMAtual] datetime NULL,
    [KMProxRevisao] numeric(8,0) NULL,
    [DtaProxRevisao] datetime NULL,
    [Revenda] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vendedor] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoUso] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuarioAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAPRIMVENDA] datetime NULL,
    [NROCPFVENDEDOR] numeric(11,0) NULL,
    CONSTRAINT [PK__IMP_VEIC__673A1D7153680069] PRIMARY KEY CLUSTERED ([Idveiculo])
);
GO
CREATE NONCLUSTERED INDEX [EXT_VEICULOAK1] ON [dbo].[IMP_VEICULO] ([Origem], [NroChassi]);
GO
CREATE NONCLUSTERED INDEX [XIE3IMP_VEICULO] ON [dbo].[IMP_VEICULO] ([StatusIMP], [DtaGeracao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IMP_VEICULO_INTEGRADO
   Criada em ..: 2025-10-31
   Alterada em : 2025-10-31
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IMP_VEICULO_INTEGRADO] (
    [Idveicseq] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [Idveiculo] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(10,0) NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCNPJCPF] numeric(13,0) NULL,
    [DigCNPJCPF] numeric(2,0) NULL,
    [CNPJx] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroChassi] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroChassiRed] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoVeiculo] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Placa] varchar(9) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Combustivel] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFamilia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodModelo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Modelo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorExterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorInterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Potencia] numeric(4,0) NULL,
    [QtdeEixo] numeric(2,0) NULL,
    [EstadoVenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaPgto] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Financiador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CanalVenda] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nronf] numeric(18,0) NULL,
    [Serienf] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Anofabricacao] numeric(4,0) NULL,
    [AnoModelo] numeric(4,0) NULL,
    [Dtavenda] datetime NULL,
    [Dtaprevquitacao] datetime NULL,
    [VlrVenda] numeric(14,2) NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KMAtual] numeric(8,0) NULL,
    [DtaKMAtual] datetime NULL,
    [KMProxRevisao] numeric(8,0) NULL,
    [DtaProxRevisao] datetime NULL,
    [Revenda] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vendedor] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoUso] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuarioAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [DtaGeracao] datetime NOT NULL,
    [StatusIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAPRIMVENDA] datetime NULL,
    [NROCPFVENDEDOR] numeric(11,0) NULL
);
GO

