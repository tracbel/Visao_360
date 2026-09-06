/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo EXT
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_AGUARDENTREGA
   Criada em ..: 2025-09-16
   Alterada em : 2025-09-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_AGUARDENTREGA] (
    [PROCESSO] numeric(18,0) NOT NULL,
    [DTAGERACAO] datetime NULL,
    [processodna] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_APROVACAO
   Criada em ..: 2025-06-27
   Alterada em : 2025-07-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_APROVACAO] (
    [PROCESSO] numeric(18,0) NOT NULL,
    [DTAGERACAO] datetime NULL,
    [processodna] numeric(18,0) NULL,
    CONSTRAINT [PK_EXT_APROVACAO] PRIMARY KEY CLUSTERED ([PROCESSO])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_CONDPAGTO
   Criada em ..: 2025-09-16
   Alterada em : 2025-09-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_CONDPAGTO] (
    [PROCESSO] numeric(18,0) NOT NULL,
    [DTAGERACAO] datetime NULL,
    [processodna] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_EMAIL
   Criada em ..: 2018-03-06
   Alterada em : 2020-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_EMAIL] (
    [IDEMAIL] numeric(18,0) NOT NULL,
    [PESSOALINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PESSOALINK] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAILLINK] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [EMAIL] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [INDEMUSO] numeric(1,0) NULL,
    [INDPREFERENCIAL] numeric(1,0) NULL,
    [INDUSOMKT] numeric(1,0) NULL,
    [INDUSOPESSOAL] numeric(1,0) NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NOT NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPESSOA] numeric(10,0) NULL,
    [SEQEMAIL] numeric(10,0) NULL,
    CONSTRAINT [PK__EXT_EMAI__B4CA2AEB2641D59D] PRIMARY KEY CLUSTERED ([IDEMAIL])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_EMAIL_0] ON [dbo].[EXT_EMAIL] ([EMAILLINK]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_EMAIL] ON [dbo].[EXT_EMAIL] ([SEQPESSOA]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_EMAIL_1] ON [dbo].[EXT_EMAIL] ([SEQEMAIL]);
GO
ALTER TABLE [dbo].[EXT_EMAIL] ADD CONSTRAINT [FK__EXT_EMAIL__SEQPE__360978D2] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_EMAIL] ADD CONSTRAINT [FK__EXT_EMAIL__SEQPE__1018DA14] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_EMAIL] ADD CONSTRAINT [FK__EXT_EMAIL__SEQPE__3F92E30C] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_FormaPgto
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_FormaPgto] (
    [IdNFS] numeric(18,0) NOT NULL,
    [IdFormaPgto] numeric(6,0) NOT NULL,
    [DescrFormaPgto] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR] numeric(14,2) NULL,
    [DtaVencto] datetime NULL,
    [OBS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_Form__70BCFB7F457DBACC] PRIMARY KEY CLUSTERED ([IdNFS], [IdFormaPgto])
);
GO
CREATE NONCLUSTERED INDEX [EXT_FORMAPGTOIF1] ON [dbo].[EXT_FormaPgto] ([IdNFS]);
GO
ALTER TABLE [dbo].[EXT_FormaPgto] ADD CONSTRAINT [FK__EXT_Forma__IdNFS__50136398] FOREIGN KEY ([IdNFS]) REFERENCES [dbo].[EXT_NFS] ([IdNFS]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_NFS
   Criada em ..: 2017-02-03
   Alterada em : 2023-10-04
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_NFS] (
    [IdNFS] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [NroNF] numeric(18,0) NOT NULL,
    [SerieNF] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresaVda] numeric(6,0) NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [IdVeic] numeric(18,0) NULL,
    [IdNFSOper] numeric(8,0) NOT NULL,
    [IdVendedor] numeric(18,0) NULL,
    [SeqDepto] numeric(4,0) NULL,
    [CFOP] numeric(8,0) NULL,
    [TipoVenda] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CanalVenda] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Setor] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaPgto] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CondicaoPgto] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaPedido] datetime NULL,
    [NroPedido] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaEmissaoNF] datetime NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodTransportador] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Transportador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImport] datetime NULL,
    [Processar] numeric(1,0) NULL,
    [StatusDWH] numeric(1,0) NULL,
    [IndEstorno] numeric(1,0) NULL,
    [Segmento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERCBASECOMISSAO] numeric(5,2) NULL,
    [IDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAIIDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOPEDIDO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROCNPJCPF] numeric(13,0) NULL,
    [DIGCNPJCPF] numeric(2,0) NULL,
    [IDENTIFICADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROVOUCHER] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_NFS__0DD2EFAB5EEAC422] PRIMARY KEY CLUSTERED ([IdNFS])
);
GO
CREATE NONCLUSTERED INDEX [EXT_NFSIE4] ON [dbo].[EXT_NFS] ([Origem], [NroEmpresa], [NroNF]);
GO
CREATE NONCLUSTERED INDEX [XIE2EXT_NFS] ON [dbo].[EXT_NFS] ([Processar], [StatusDWH]);
GO
CREATE NONCLUSTERED INDEX [XIE3EXT_NFS] ON [dbo].[EXT_NFS] ([DtaEmissaoNF]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_NFS] ON [dbo].[EXT_NFS] ([IdVeic]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_NFS_0] ON [dbo].[EXT_NFS] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_NFS_1] ON [dbo].[EXT_NFS] ([IdNFSOper]);
GO
CREATE NONCLUSTERED INDEX [XIF4EXT_NFS] ON [dbo].[EXT_NFS] ([IdVendedor]);
GO
CREATE NONCLUSTERED INDEX [XIF5EXT_NFS] ON [dbo].[EXT_NFS] ([SeqDepto]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_NFS_2] ON [dbo].[EXT_NFS] ([IDNFSEXTERNO]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_NFS_3] ON [dbo].[EXT_NFS] ([PAIIDNFSEXTERNO]);
GO
CREATE NONCLUSTERED INDEX [EXT_NFS_IdNFS_IDX_Empresa_NRO_NF_Serie] ON [dbo].[EXT_NFS] ([IdNFS], [NroEmpresa], [NroNF], [SerieNF]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__IdNFSOp__52EFD043] FOREIGN KEY ([IdNFSOper]) REFERENCES [dbo].[EXT_NFSOper] ([IdNFSOper]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__IdVende__53E3F47C] FOREIGN KEY ([IdVendedor]) REFERENCES [dbo].[EXT_Vendedor] ([IdVendedor]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__SeqPess__00A18C5A] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__IdNFSOp__0195B093] FOREIGN KEY ([IdNFSOper]) REFERENCES [dbo].[EXT_NFSOper] ([IdNFSOper]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__IdVende__0289D4CC] FOREIGN KEY ([IdVendedor]) REFERENCES [dbo].[EXT_Vendedor] ([IdVendedor]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__IdVeic__510787D1] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__SeqPess__51FBAC0A] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_NFS] ADD CONSTRAINT [FK__EXT_NFS__IdVeic__7FAD6821] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_NFSCmpl
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_NFSCmpl] (
    [IdNFS] numeric(18,0) NOT NULL,
    [Idcmpl] numeric(4,0) NOT NULL,
    [CMPL] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtaimport] datetime NULL,
    [IDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_NFSC__6EB51BBB2FCB34E2] PRIMARY KEY CLUSTERED ([IdNFS], [Idcmpl])
);
GO
CREATE NONCLUSTERED INDEX [EXT_NFSCMPLIF1] ON [dbo].[EXT_NFSCmpl] ([IdNFS]);
GO
ALTER TABLE [dbo].[EXT_NFSCmpl] ADD CONSTRAINT [FK__EXT_NFSCm__IdNFS__55CC3CEE] FOREIGN KEY ([IdNFS]) REFERENCES [dbo].[EXT_NFS] ([IdNFS]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_NFSItem
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_NFSItem] (
    [IdNFS] numeric(18,0) NOT NULL,
    [IdItem] numeric(6,0) NOT NULL,
    [IdProduto] numeric(18,0) NULL,
    [IdVendedor] numeric(18,0) NULL,
    [SeqDepto] numeric(4,0) NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Qtde] numeric(10,3) NULL,
    [VlrLiqItem] numeric(14,2) NULL,
    [vlrUnitario] numeric(14,2) NULL,
    [Vlrdescto] numeric(14,2) NULL,
    [VlrResult] numeric(14,2) NULL,
    [VlrCustoMkt] numeric(14,2) NULL,
    [VlrICM] numeric(14,2) NULL,
    [VlrImposto] numeric(14,2) NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndEstorno] numeric(1,0) NULL,
    [StatusDWH] numeric(1,0) NULL,
    [DtaImport] datetime NULL,
    [SETORITEM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRCUSTO] numeric(14,2) NULL,
    [IDNFSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRICMSRETIDO] numeric(14,2) NULL,
    [VLRICMSSUBS] numeric(14,2) NULL,
    [VLRPIS] numeric(14,2) NULL,
    [VLRCOFINS] numeric(14,2) NULL,
    [VLRDESCTOVCHR] numeric(14,2) NULL,
    CONSTRAINT [PK__EXT_NFSI__38CC6B8D9301B2E0] PRIMARY KEY CLUSTERED ([IdNFS], [IdItem])
);
GO
CREATE NONCLUSTERED INDEX [EXT_NFSITEMIF1] ON [dbo].[EXT_NFSItem] ([IdNFS]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_NFSItem] ON [dbo].[EXT_NFSItem] ([IdProduto]);
GO
CREATE NONCLUSTERED INDEX [XIF3EXT_NFSItem] ON [dbo].[EXT_NFSItem] ([IdVendedor]);
GO
CREATE NONCLUSTERED INDEX [XIF4EXT_NFSItem] ON [dbo].[EXT_NFSItem] ([SeqDepto]);
GO
ALTER TABLE [dbo].[EXT_NFSItem] ADD CONSTRAINT [FK__EXT_NFSIt__IdVen__58A8A999] FOREIGN KEY ([IdVendedor]) REFERENCES [dbo].[EXT_Vendedor] ([IdVendedor]);
GO
ALTER TABLE [dbo].[EXT_NFSItem] ADD CONSTRAINT [FK__EXT_NFSIt__IdPro__065A65B0] FOREIGN KEY ([IdProduto]) REFERENCES [dbo].[EXT_Produto] ([IdProduto]);
GO
ALTER TABLE [dbo].[EXT_NFSItem] ADD CONSTRAINT [FK__EXT_NFSIt__IdVen__074E89E9] FOREIGN KEY ([IdVendedor]) REFERENCES [dbo].[EXT_Vendedor] ([IdVendedor]);
GO
ALTER TABLE [dbo].[EXT_NFSItem] ADD CONSTRAINT [FK__EXT_NFSIt__IdNFS__56C06127] FOREIGN KEY ([IdNFS]) REFERENCES [dbo].[EXT_NFS] ([IdNFS]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[EXT_NFSItem] ADD CONSTRAINT [FK__EXT_NFSIt__IdPro__57B48560] FOREIGN KEY ([IdProduto]) REFERENCES [dbo].[EXT_Produto] ([IdProduto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_NFSOper
   Criada em ..: 2017-01-02
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_NFSOper] (
    [IdNFSOper] numeric(8,0) NOT NULL,
    [SeqDepto] numeric(4,0) NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CodOperacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescReduzida] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EntradaSaida] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [IndEstorno] numeric(1,0) NOT NULL,
    [FormaLeitura] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_NFSO__F97DD5D906F158BE] PRIMARY KEY CLUSTERED ([IdNFSOper])
);
GO
CREATE NONCLUSTERED INDEX [XIE1EXT_NFSOper] ON [dbo].[EXT_NFSOper] ([CodOperacao]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_NFSOper] ON [dbo].[EXT_NFSOper] ([SeqDepto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_NFSOperEmp
   Criada em ..: 2017-01-02
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_NFSOperEmp] (
    [NroEmpresa] numeric(6,0) NOT NULL,
    [IdNFSOper] numeric(8,0) NOT NULL,
    [SeqDepto] numeric(4,0) NULL,
    CONSTRAINT [PK__EXT_NFSO__C207E533738F740F] PRIMARY KEY CLUSTERED ([NroEmpresa], [IdNFSOper])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_NFSOPEMPR] ON [dbo].[EXT_NFSOperEmp] ([IdNFSOper]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_NFSOPEMPR_0] ON [dbo].[EXT_NFSOperEmp] ([SeqDepto]);
GO
ALTER TABLE [dbo].[EXT_NFSOperEmp] ADD CONSTRAINT [FK__EXT_NFSOp__IdNFS__2EDC8CFF] FOREIGN KEY ([IdNFSOper]) REFERENCES [dbo].[EXT_NFSOper] ([IdNFSOper]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_OS
   Criada em ..: 2017-02-03
   Alterada em : 2021-08-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_OS] (
    [IDOS] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Nroos] numeric(18,0) NULL,
    [SerieOS] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdVeic] numeric(18,0) NULL,
    [SeqDepto] numeric(4,0) NULL,
    [Nrochassi] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Placa] varchar(9) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Combustivel] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Codveiculo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Modelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Corveiculo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Anofabricacao] numeric(4,0) NULL,
    [AnoModelo] numeric(4,0) NULL,
    [Dtavenda] datetime NULL,
    [Consultor] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoOS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtaabertura] datetime NULL,
    [Dtaencerramento] datetime NULL,
    [Dtafechamento] datetime NULL,
    [VlrLiqPecas] numeric(14,2) NULL,
    [VlrLiqServicos] numeric(14,2) NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nrodn] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Kilometragem] numeric(8,0) NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [Dtaimport] datetime NULL,
    [Processar] numeric(1,0) NULL,
    [StatusDWH] numeric(1,0) NULL,
    [Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFamilia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDOSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SITUACAO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODTIPOOS] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAOERP] datetime NULL,
    [TIPOSERVICO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_OS__B87C231D41A98A61] PRIMARY KEY CLUSTERED ([IDOS])
);
GO
CREATE NONCLUSTERED INDEX [EXT_OSAK1] ON [dbo].[EXT_OS] ([Origem], [NroEmpresa], [Nroos]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_OS] ON [dbo].[EXT_OS] ([IdVeic]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_OS_SEQP] ON [dbo].[EXT_OS] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF3EXT_OS] ON [dbo].[EXT_OS] ([SeqDepto]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_OS_0] ON [dbo].[EXT_OS] ([IDOSEXTERNO]);
GO
ALTER TABLE [dbo].[EXT_OS] ADD CONSTRAINT [FK__EXT_OS__IdVeic__5D6D5EB6] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]);
GO
ALTER TABLE [dbo].[EXT_OS] ADD CONSTRAINT [FK__EXT_OS__SeqPesso__5E6182EF] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[EXT_OS] ADD CONSTRAINT [FK__EXT_OS__IdVeic__0C133F06] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_OSITEM
   Criada em ..: 2021-08-02
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_OSITEM] (
    [IDOS] numeric(18,0) NOT NULL,
    [IDITEM] numeric(18,0) NOT NULL,
    [TIPOITEM] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTIVO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE] numeric(10,2) NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAIMPORT] datetime NULL,
    [STATUSITEM] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLRTOTITEM] numeric(14,2) NULL,
    [VLRTOTITEMBRUTO] numeric(14,2) NULL,
    [IDOSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MAODEOBRA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GRUPOITEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_OSIT__4463D2A83E06D9B4] PRIMARY KEY CLUSTERED ([IDOS], [IDITEM])
);
GO
CREATE NONCLUSTERED INDEX [EXT_OSITEMIF1] ON [dbo].[EXT_OSITEM] ([IDOS]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_OSITEM] ON [dbo].[EXT_OSITEM] ([IDOSEXTERNO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_OSSolic
   Criada em ..: 2017-02-03
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_OSSolic] (
    [IDOS] numeric(18,0) NOT NULL,
    [Idsolic] numeric(18,0) NOT NULL,
    [Codigo] varchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDOSEXTERNO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAIMPORT] datetime NULL,
    CONSTRAINT [PK__EXT_OSSo__6CEDE1F4BAB11998] PRIMARY KEY CLUSTERED ([IDOS], [Idsolic])
);
GO
CREATE NONCLUSTERED INDEX [EXT_OSSOLICIF1] ON [dbo].[EXT_OSSolic] ([IDOS]);
GO
ALTER TABLE [dbo].[EXT_OSSolic] ADD CONSTRAINT [FK__EXT_OSSoli__IDOS__613DEF9A] FOREIGN KEY ([IDOS]) REFERENCES [dbo].[EXT_OS] ([IDOS]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Pedido
   Criada em ..: 2017-02-03
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Pedido] (
    [IdPedido] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCNPJCPF] numeric(13,0) NULL,
    [DigCNPJCPF] numeric(2,0) NULL,
    [NroEmpresa] decimal(6,0) NOT NULL,
    [NroPedido] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [IdPessoa] numeric(18,0) NULL,
    [CodOperacao] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [IndEstRservado] numeric(1,0) NULL,
    [IndEnvioERP] numeric(1,0) NULL,
    [DtaEnvioERP] datetime NULL,
    [ObsNF] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsInterna] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsDesconto] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeAssinatura] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssinaturaDesc] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssinaturaDta] datetime NULL,
    [EntradaSaida] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MotivoVP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoEntrega] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoFrete] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodTransportador] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Transportador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PedidoLinkNro] numeric(18,0) NULL,
    [PedidoLinkStr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USUARIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEPARTAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEGMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSCMPL] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERCBASECOMISSAO] numeric(5,2) NULL,
    [PEDIDOLINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAILINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAILINKNRO] numeric(18,0) NULL,
    [PAILINKSTR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO] numeric(18,0) NULL,
    [VLRDESCADICIONAL] numeric(15,2) NULL,
    [DTAALTERACAOERP] datetime NULL,
    CONSTRAINT [PK__EXT_Pedi__9D335DC34DE7BBA1] PRIMARY KEY CLUSTERED ([IdPedido])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_Pedido] ON [dbo].[EXT_Pedido] ([IdPedido]);
GO
CREATE NONCLUSTERED INDEX [XIE1EXT_Pedido] ON [dbo].[EXT_Pedido] ([Pessoalink], [PessoaLinkOrigem]);
GO
CREATE NONCLUSTERED INDEX [XIE2EXT_Pedido] ON [dbo].[EXT_Pedido] ([NroCNPJCPF]);
GO
CREATE NONCLUSTERED INDEX [XIE3EXT_Pedido] ON [dbo].[EXT_Pedido] ([NroPedido]);
GO
CREATE NONCLUSTERED INDEX [XIE4EXT_Pedido] ON [dbo].[EXT_Pedido] ([DtaPedido]);
GO
CREATE NONCLUSTERED INDEX [XIE5EXT_Pedido] ON [dbo].[EXT_Pedido] ([NroPedido], [NroEmpresa], [Origem]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_Pedido] ON [dbo].[EXT_Pedido] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_Pedido] ON [dbo].[EXT_Pedido] ([IdPessoa]);
GO
ALTER TABLE [dbo].[EXT_Pedido] ADD CONSTRAINT [FK__EXT_Pedid__IdPes__11CC185C] FOREIGN KEY ([IdPessoa]) REFERENCES [dbo].[EXT_Pessoa] ([IdPessoa]);
GO
ALTER TABLE [dbo].[EXT_Pedido] ADD CONSTRAINT [FK__EXT_Pedid__SeqPe__623213D3] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_Pedido] ADD CONSTRAINT [FK__EXT_Pedid__IdPes__6326380C] FOREIGN KEY ([IdPessoa]) REFERENCES [dbo].[EXT_Pessoa] ([IdPessoa]);
GO
ALTER TABLE [dbo].[EXT_Pedido] ADD CONSTRAINT [FK__EXT_Pedid__SeqPe__10D7F423] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_PedidoItem
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_PedidoItem] (
    [IdPedido] numeric(18,0) NOT NULL,
    [SeqItem] numeric(18,0) NOT NULL,
    [IdProduto] numeric(18,0) NULL,
    [NroEmpresa] decimal(6,0) NOT NULL,
    [CodTabelaPreco] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    CONSTRAINT [PK__EXT_Pedi__BFC6373614BD8AC7] PRIMARY KEY CLUSTERED ([IdPedido], [SeqItem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_PedidoItem] ON [dbo].[EXT_PedidoItem] ([IdPedido], [SeqItem]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_PedidoItem] ON [dbo].[EXT_PedidoItem] ([IdProduto]);
GO
CREATE NONCLUSTERED INDEX [XIF3EXT_PedidoItem] ON [dbo].[EXT_PedidoItem] ([IdPedido]);
GO
ALTER TABLE [dbo].[EXT_PedidoItem] ADD CONSTRAINT [FK__EXT_Pedid__IdPro__12C03C95] FOREIGN KEY ([IdProduto]) REFERENCES [dbo].[EXT_Produto] ([IdProduto]);
GO
ALTER TABLE [dbo].[EXT_PedidoItem] ADD CONSTRAINT [FK__EXT_Pedid__IdPed__13B460CE] FOREIGN KEY ([IdPedido]) REFERENCES [dbo].[EXT_Pedido] ([IdPedido]);
GO
ALTER TABLE [dbo].[EXT_PedidoItem] ADD CONSTRAINT [FK__EXT_Pedid__IdPro__641A5C45] FOREIGN KEY ([IdProduto]) REFERENCES [dbo].[EXT_Produto] ([IdProduto]);
GO
ALTER TABLE [dbo].[EXT_PedidoItem] ADD CONSTRAINT [FK__EXT_Pedid__IdPed__650E807E] FOREIGN KEY ([IdPedido]) REFERENCES [dbo].[EXT_Pedido] ([IdPedido]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Pessoa
   Criada em ..: 2017-02-03
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Pessoa] (
    [IdPessoa] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
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
    CONSTRAINT [PK__EXT_Pess__7061465DBCF5E474] PRIMARY KEY CLUSTERED ([IdPessoa])
);
GO
CREATE NONCLUSTERED INDEX [EXT_PESSOAIE3_0x] ON [dbo].[EXT_Pessoa] ([PessoaLinkOrigem], [Pessoalink]);
GO
CREATE NONCLUSTERED INDEX [EXT_PESSOAIE4_0x] ON [dbo].[EXT_Pessoa] ([NroCNPJCPF]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_PESSOA] ON [dbo].[EXT_Pessoa] ([StatusIMP], [DtaGeracao]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_Pessoa] ON [dbo].[EXT_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_Pessoa] ADD CONSTRAINT [FK__EXT_Pesso__SeqPe__14A88507] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_Pessoa] ADD CONSTRAINT [FK__EXT_Pesso__SeqPe__6602A4B7] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Pessoa_bkpago22
   Criada em ..: 2022-08-11
   Alterada em : 2022-08-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Pessoa_bkpago22] (
    [IdPessoa] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [PessoaLinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
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
    [CodEquipe] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [CARTEIRA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_PESSOACONTATO
   Criada em ..: 2018-03-06
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_PESSOACONTATO] (
    [IDPESSOACTTO] numeric(18,0) NOT NULL,
    [PESSOALINKORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PESSOALINK] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONTATOLINK] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
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
    [PAPEL1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAPEL2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAPEL3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBSERVACAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NOT NULL,
    [STATUSIMPSP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSIMP] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPESSOA] numeric(10,0) NULL,
    [SEQCONTATO] numeric(4,0) NULL,
    [INDWHATSAPPF1] numeric(1,0) NULL,
    [INDWHATSAPPF2] numeric(1,0) NULL,
    CONSTRAINT [PK__EXT_PESS__4F435473B021655B] PRIMARY KEY CLUSTERED ([IDPESSOACTTO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_PESSOACONTATO] ON [dbo].[EXT_PESSOACONTATO] ([SEQPESSOA]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_PESSOACONTATO_0] ON [dbo].[EXT_PESSOACONTATO] ([SEQCONTATO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_PESSOAFONE
   Criada em ..: 2019-12-19
   Alterada em : 2019-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_PESSOAFONE] (
    [IDPESSOAFONE] numeric(18,0) NOT NULL,
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
    [SEQPESFONE] numeric(18,0) NULL,
    CONSTRAINT [PK__EXT_PESS__4EBB238807B17403] PRIMARY KEY CLUSTERED ([IDPESSOAFONE])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Pot_Pecas
   Criada em ..: 2018-09-10
   Alterada em : 2018-09-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Pot_Pecas] (
    [SEQ] numeric(10,0) NOT NULL,
    [JAN] numeric(14,2) NULL,
    [FEV] numeric(14,2) NULL,
    [MAR] numeric(14,2) NULL,
    [ABR] numeric(14,2) NULL,
    [MAI] numeric(14,2) NULL,
    [JUN] numeric(14,2) NULL,
    [JUL] numeric(14,2) NULL,
    [AGO] numeric(14,2) NULL,
    [SET] numeric(14,2) NULL,
    [OUT] numeric(14,2) NULL,
    [NOV] numeric(14,2) NULL,
    [DEZ] numeric(14,2) NULL,
    CONSTRAINT [PK_EXT_Pot_Pecas] PRIMARY KEY CLUSTERED ([SEQ])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Produto
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Produto] (
    [IdProduto] numeric(18,0) NOT NULL,
    [CodProduto] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodBarra] numeric(18,0) NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Marca] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodFamilia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescricaoCompleta] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NCM] numeric(10,0) NULL,
    [PrecoPublico] numeric(14,2) NULL,
    [Preco1] numeric(14,2) NULL,
    [Preco2] numeric(14,2) NULL,
    [EmUso] numeric(1,0) NULL,
    [Dtaimport] datetime NULL,
    [FORNECEDORPRINCIPAL] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA1] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA3] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA4] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA5] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CATEGORIA6] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVEPRODUTOERP] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDEESTOQUE] numeric(10,2) NULL,
    [CUSTOESTOQUE] numeric(15,4) NULL,
    [DTAESTOQUE] datetime NULL,
    CONSTRAINT [PK__EXT_Prod__2E883C23F4043BB4] PRIMARY KEY CLUSTERED ([IdProduto])
);
GO
CREATE NONCLUSTERED INDEX [EXT_PRODUTOAK1] ON [dbo].[EXT_Produto] ([CodProduto]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_PRODUTO] ON [dbo].[EXT_Produto] ([CodProduto], [ORIGEM], [Tipo]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_PRODUTO_ERP] ON [dbo].[EXT_Produto] ([CHAVEPRODUTOERP]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_TIT_ACRESC
   Criada em ..: 2018-09-10
   Alterada em : 2018-09-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_TIT_ACRESC] (
    [LINKSTR] int NOT NULL,
    [VALOR] numeric(13,2) NULL,
    CONSTRAINT [PK_EXT_TIT_ACRESC] PRIMARY KEY CLUSTERED ([LINKSTR])
);
GO
CREATE NONCLUSTERED INDEX [EXT_TIT_ACRESC_01] ON [dbo].[EXT_TIT_ACRESC] ([LINKSTR]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Titulo
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Titulo] (
    [idTitulo] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [IdPessoa] numeric(18,0) NULL,
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
    [VlrAberto] numeric(14,2) NULL,
    [VlrMov] numeric(14,2) NULL,
    [Movimento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltPgto] datetime NULL,
    [DtaQuitacao] datetime NULL,
    [DtaUltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVESTR1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODBARRAS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [dtaimport] datetime NULL,
    CONSTRAINT [PK__EXT_Titu__A3113E575E097FF1] PRIMARY KEY CLUSTERED ([idTitulo])
);
GO
CREATE NONCLUSTERED INDEX [EXT_TITULOIE1] ON [dbo].[EXT_Titulo] ([LinkNro]);
GO
CREATE NONCLUSTERED INDEX [EXT_TITULOIE2] ON [dbo].[EXT_Titulo] ([LinkStr]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_TITULO] ON [dbo].[EXT_Titulo] ([IdPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIE5EXT_Titulo] ON [dbo].[EXT_Titulo] ([NroTitulo]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_Titulo] ON [dbo].[EXT_Titulo] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_TITULO_01] ON [dbo].[EXT_Titulo] ([IndAtivo], [Status]);
GO
ALTER TABLE [dbo].[EXT_Titulo] ADD CONSTRAINT [FK__EXT_Titul__SeqPe__66F6C8F0] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[EXT_Titulo] ADD CONSTRAINT [FK__EXT_Titul__IdPes__67EAED29] FOREIGN KEY ([IdPessoa]) REFERENCES [dbo].[EXT_Pessoa] ([IdPessoa]);
GO
ALTER TABLE [dbo].[EXT_Titulo] ADD CONSTRAINT [FK__EXT_Titul__IdPes__1690CD79] FOREIGN KEY ([IdPessoa]) REFERENCES [dbo].[EXT_Pessoa] ([IdPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Titulo_bkp_11_04
   Criada em ..: 2023-04-11
   Alterada em : 2023-04-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Titulo_bkp_11_04] (
    [idTitulo] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [IdPessoa] numeric(18,0) NULL,
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
    [VlrAberto] numeric(14,2) NULL,
    [VlrMov] numeric(14,2) NULL,
    [Movimento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltPgto] datetime NULL,
    [DtaQuitacao] datetime NULL,
    [DtaUltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVESTR1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODBARRAS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [dtaimport] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Titulo_BKP_22_03_2023
   Criada em ..: 2023-03-22
   Alterada em : 2023-03-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Titulo_BKP_22_03_2023] (
    [idTitulo] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [IdPessoa] numeric(18,0) NULL,
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
    [VlrAberto] numeric(14,2) NULL,
    [VlrMov] numeric(14,2) NULL,
    [Movimento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltPgto] datetime NULL,
    [DtaQuitacao] datetime NULL,
    [DtaUltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVESTR1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODBARRAS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [dtaimport] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Titulo_BKP_22_03_2023_BAIXADOS
   Criada em ..: 2023-03-22
   Alterada em : 2023-03-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Titulo_BKP_22_03_2023_BAIXADOS] (
    [idTitulo] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [IdPessoa] numeric(18,0) NULL,
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
    [VlrAberto] numeric(14,2) NULL,
    [VlrMov] numeric(14,2) NULL,
    [Movimento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltPgto] datetime NULL,
    [DtaQuitacao] datetime NULL,
    [DtaUltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVESTR1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODBARRAS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [dtaimport] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_TituloCmpl
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_TituloCmpl] (
    [idTitulo] numeric(18,0) NOT NULL,
    [Complemento] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_Titu__A3113E57EC29C706] PRIMARY KEY CLUSTERED ([idTitulo])
);
GO
ALTER TABLE [dbo].[EXT_TituloCmpl] ADD CONSTRAINT [FK__EXT_Titul__idTit__68DF1162] FOREIGN KEY ([idTitulo]) REFERENCES [dbo].[EXT_Titulo] ([idTitulo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_TituloMov
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_TituloMov] (
    [IdTituloMov] numeric(18,0) NOT NULL,
    [idTitulo] numeric(18,0) NULL,
    [DtaMov] datetime NULL,
    [VlrMov] numeric(14,2) NULL,
    [Movimento] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VlrMovCalc] numeric(14,2) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_Titu__1174875FAEF83FC2] PRIMARY KEY CLUSTERED ([IdTituloMov])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_TituloMov] ON [dbo].[EXT_TituloMov] ([IdTituloMov]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_TituloMov] ON [dbo].[EXT_TituloMov] ([idTitulo]);
GO
ALTER TABLE [dbo].[EXT_TituloMov] ADD CONSTRAINT [FK__EXT_Titul__idTit__69D3359B] FOREIGN KEY ([idTitulo]) REFERENCES [dbo].[EXT_Titulo] ([idTitulo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Veic
   Criada em ..: 2017-02-03
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Veic] (
    [IdVeic] numeric(18,0) NOT NULL,
    [IdVeicMarca] numeric(4,0) NOT NULL,
    [IdVeicModelo] numeric(8,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [SeqPlanoMAN] numeric(18,0) NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CorExterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorInterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Chassi] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ChassiRed] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Placa] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroMOTOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [NroEmpresaServ] numeric(6,0) NULL,
    [KMAtual] numeric(8,0) NULL,
    [DtaKMAtual] datetime NULL,
    [KMMedia] numeric(6,0) NULL,
    [DtaKMMedia] datetime NULL,
    [INDRECALCKM] numeric(1,0) NULL,
    [INDAnaliseAgd] numeric(1,0) NULL,
    [NroUltServico] numeric(2,0) NULL,
    [AnoModelo] numeric(4,0) NULL,
    [AnoFabric] numeric(4,0) NULL,
    [Combustivel] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDALTManual] numeric(1,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAPRIMVENDA] datetime NULL,
    CONSTRAINT [PK__EXT_Veic__A8BACEFE00A0ED18] PRIMARY KEY CLUSTERED ([IdVeic])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEIC_CHASSI] ON [dbo].[EXT_Veic] ([Chassi]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEIC_PLACA] ON [dbo].[EXT_Veic] ([Placa]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEIC_CHSC] ON [dbo].[EXT_Veic] ([ChassiRed]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEIC_MOD] ON [dbo].[EXT_Veic] ([IdVeicModelo]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEIC] ON [dbo].[EXT_Veic] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICPLMAN] ON [dbo].[EXT_Veic] ([SeqPlanoMAN]);
GO
CREATE NONCLUSTERED INDEX [XIF4EXT_Veic] ON [dbo].[EXT_Veic] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__IdVeic__196D3A24] FOREIGN KEY ([IdVeicModelo]) REFERENCES [dbo].[EXT_VeicModelo] ([IdVeicModelo]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__IdVeic__6AC759D4] FOREIGN KEY ([IdVeicModelo]) REFERENCES [dbo].[EXT_VeicModelo] ([IdVeicModelo]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__SeqPes__6BBB7E0D] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__SeqPes__1A615E5D] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__SeqPla__1B558296] FOREIGN KEY ([SeqPlanoMAN]) REFERENCES [dbo].[EXT_VeicPlanoMan] ([SeqPlanoMAN]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__IdVeic__1C49A6CF] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__SeqPla__6CAFA246] FOREIGN KEY ([SeqPlanoMAN]) REFERENCES [dbo].[EXT_VeicPlanoMan] ([SeqPlanoMAN]);
GO
ALTER TABLE [dbo].[EXT_Veic] ADD CONSTRAINT [FK__EXT_Veic__IdVeic__6DA3C67F] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicAgd
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicAgd] (
    [SeqVeicAgd] numeric(18,0) NOT NULL,
    [IdVeic] numeric(18,0) NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [Placa] char(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroServico] numeric(2,0) NULL,
    [KMAtual] numeric(8,0) NOT NULL,
    [DtaConvite] datetime NULL,
    [SeqAgdConvite] numeric(18,0) NULL,
    [DtaLembrete1] datetime NULL,
    [SeqAgdLembrete1] numeric(18,0) NULL,
    [DtaLembrete2] datetime NULL,
    [SeqAgdLembrete2] numeric(18,0) NULL,
    [DtaConfirmacao] datetime NULL,
    [SeqAgdConfirmacao] numeric(18,0) NULL,
    [NroTentativa] numeric(2,0) NULL,
    [DtaAgenda] datetime NULL,
    [SeqAgenda] numeric(18,0) NULL,
    [IndDtaAgendaConf] numeric(1,0) NULL,
    [DtaRealizado] datetime NULL,
    [StatusFinal] varchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndAgdAvulso] numeric(1,0) NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [Gerador] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_Veic__0D4FECDB34BA173A] PRIMARY KEY CLUSTERED ([SeqVeicAgd])
);
GO
CREATE NONCLUSTERED INDEX [XIE1EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [XIE2EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([SeqAgdConvite]);
GO
CREATE NONCLUSTERED INDEX [XIE3EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([SeqAgdLembrete1]);
GO
CREATE NONCLUSTERED INDEX [XIE4EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([SeqAgdLembrete2]);
GO
CREATE NONCLUSTERED INDEX [XIE5EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([SeqAgdConfirmacao]);
GO
CREATE NONCLUSTERED INDEX [XIE6EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([SeqAgenda]);
GO
CREATE NONCLUSTERED INDEX [XIE7EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([Placa]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICAGD] ON [dbo].[EXT_VeicAgd] ([IdVeic]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_VeicAgd] ON [dbo].[EXT_VeicAgd] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_VeicAgd] ADD CONSTRAINT [FK__EXT_VeicA__SeqPe__1E31EF41] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[EXT_VeicAgd] ADD CONSTRAINT [FK__EXT_VeicA__IdVei__6E97EAB8] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[EXT_VeicAgd] ADD CONSTRAINT [FK__EXT_VeicA__SeqPe__6F8C0EF1] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VEICAGDERP
   Criada em ..: 2017-04-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VEICAGDERP] (
    [SEQVEICAGDERP] numeric(18,0) NOT NULL,
    [SEQVEICAGD] numeric(18,0) NOT NULL,
    [TIPOAGD] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [INDEMABERTO] numeric(1,0) NULL,
    [ERP] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTADE] datetime NOT NULL,
    [DTAATE] datetime NULL,
    [ATENDENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVETAB] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_VEIC__D461931EC0A35977] PRIMARY KEY CLUSTERED ([SEQVEICAGDERP])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICAGDERP] ON [dbo].[EXT_VEICAGDERP] ([SEQVEICAGD]);
GO
ALTER TABLE [dbo].[EXT_VEICAGDERP] ADD CONSTRAINT [FK__EXT_VEICA__SEQVE__58739F6F] FOREIGN KEY ([SEQVEICAGD]) REFERENCES [dbo].[EXT_VeicAgd] ([SeqVeicAgd]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicAvalFoto
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicAvalFoto] (
    [IdAvalFoto] numeric(18,0) NOT NULL,
    [IdAvalia] numeric(18,0) NOT NULL,
    [URL] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_Veic__B913BC4CE941EA95] PRIMARY KEY CLUSTERED ([IdAvalFoto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_VeicAvalFoto] ON [dbo].[EXT_VeicAvalFoto] ([IdAvalFoto]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_VeicAvalFoto] ON [dbo].[EXT_VeicAvalFoto] ([IdAvalia]);
GO
ALTER TABLE [dbo].[EXT_VeicAvalFoto] ADD CONSTRAINT [FK__EXT_VeicA__IdAva__7080332A] FOREIGN KEY ([IdAvalia]) REFERENCES [dbo].[EXT_VeicAvalia] ([IdAvalia]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicAvalia
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicAvalia] (
    [IdAvalia] numeric(18,0) NOT NULL,
    [CNPJEmpresa] numeric(18,0) NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Placa] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Chassi] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Renavam] varchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Marca] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Modelo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Versao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroPortas] numeric(2,0) NULL,
    [CorExterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KMAtual] numeric(8,0) NULL,
    [AnoModelo] numeric(4,0) NULL,
    [AnoFabric] numeric(4,0) NULL,
    [Combustivel] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAvaliacao] datetime NULL,
    [VlrAvaliado] numeric(14,2) NULL,
    [VlrVenda] numeric(14,2) NULL,
    [VlrCliente] numeric(14,2) NULL,
    [VlrReparos] numeric(14,2) NULL,
    [Classificacao] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Finalidade] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeCliente] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmailCliente] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneCliente] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNPJCPFCliente] numeric(18,0) NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [IdVeic] numeric(18,0) NULL,
    [LinkAvaliacao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAtualizacao] datetime NULL,
    CONSTRAINT [PK__EXT_Veic__46BBF4846EBC693D] PRIMARY KEY CLUSTERED ([IdAvalia])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_VeicAvalia] ON [dbo].[EXT_VeicAvalia] ([IdAvalia]);
GO
CREATE NONCLUSTERED INDEX [XIE1EXT_VeicAvalia] ON [dbo].[EXT_VeicAvalia] ([Placa]);
GO
CREATE NONCLUSTERED INDEX [XIE2EXT_VeicAvalia] ON [dbo].[EXT_VeicAvalia] ([Chassi]);
GO
CREATE NONCLUSTERED INDEX [XIE3EXT_VeicAvalia] ON [dbo].[EXT_VeicAvalia] ([Modelo], [Marca]);
GO
CREATE NONCLUSTERED INDEX [XIE4EXT_VeicAvalia] ON [dbo].[EXT_VeicAvalia] ([DtaAvaliacao]);
GO
CREATE NONCLUSTERED INDEX [XIE5EXT_VeicAvalia] ON [dbo].[EXT_VeicAvalia] ([LinkAvaliacao]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_VeicAvalia] ON [dbo].[EXT_VeicAvalia] ([IdVeic]);
GO
ALTER TABLE [dbo].[EXT_VeicAvalia] ADD CONSTRAINT [FK__EXT_VeicA__IdVei__71745763] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]);
GO
ALTER TABLE [dbo].[EXT_VeicAvalia] ADD CONSTRAINT [FK__EXT_VeicA__IdVei__201A37B3] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicFam
   Criada em ..: 2017-01-17
   Alterada em : 2021-07-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicFam] (
    [IdVeicFamilia] numeric(6,0) NOT NULL,
    [IdVeicMarca] numeric(4,0) NOT NULL,
    [Descricao] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDHoraKM] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TPVeicSeqPar] numeric(18,0) NULL,
    [SeqPropriedade] numeric(4,0) NULL,
    CONSTRAINT [PK__EXT_Veic__545E036ED9D44969] PRIMARY KEY CLUSTERED ([IdVeicFamilia])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICFAM] ON [dbo].[EXT_VeicFam] ([IdVeicMarca]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_VeicFam] ON [dbo].[EXT_VeicFam] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__75E406C5] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__4DB619DA] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__6EACF951] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__6FA11D8A] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__6FC1191B] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__7248800B] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__72687B9C] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__733CA444] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__735C9FD5] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__70B53D54] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__6B117297] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__210E5BEC] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__22028025] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__4CC1F5A1] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__6A1D4E5E] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__74BAD862] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__IdVei__74EFE28C] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicFam] ADD CONSTRAINT [FK__EXT_VeicF__SeqPr__75AEFC9B] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VEICFAMREF
   Criada em ..: 2025-10-29
   Alterada em : 2025-10-29
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VEICFAMREF] (
    [FAMILIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPROPRIEDADE] int NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicKM
   Criada em ..: 2017-02-03
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicKM] (
    [IdVeic] numeric(18,0) NOT NULL,
    [KMAtual] numeric(8,0) NOT NULL,
    [IdVeicProp] numeric(18,0) NOT NULL,
    [IDOS] numeric(18,0) NULL,
    [SeqPlanoMnFx] numeric(18,0) NULL,
    [DtaKMAtual] datetime NULL,
    [DtaAlteracao] datetime NULL,
    [NroServico] numeric(2,0) NULL,
    [OBS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KMRevisao] numeric(8,0) NULL,
    [INDKMINVALIDO] numeric(1,0) NULL,
    CONSTRAINT [PK__EXT_Veic__9A698ECD58A0C5FB] PRIMARY KEY CLUSTERED ([IdVeic], [KMAtual])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICKM] ON [dbo].[EXT_VeicKM] ([IdVeic]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICKM_0] ON [dbo].[EXT_VeicKM] ([IdVeicProp]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_VeicKM] ON [dbo].[EXT_VeicKM] ([IDOS]);
GO
CREATE NONCLUSTERED INDEX [XIF3EXT_VeicKM] ON [dbo].[EXT_VeicKM] ([SeqPlanoMnFx]);
GO
ALTER TABLE [dbo].[EXT_VeicKM] ADD CONSTRAINT [FK__EXT_VeicK__SeqPl__76390C80] FOREIGN KEY ([SeqPlanoMnFx]) REFERENCES [dbo].[EXT_VeicPlanoMnFX] ([SeqPlanoMnFx]);
GO
ALTER TABLE [dbo].[EXT_VeicKM] ADD CONSTRAINT [FK__EXT_VeicKM__IDOS__23EAC897] FOREIGN KEY ([IDOS]) REFERENCES [dbo].[EXT_OS] ([IDOS]);
GO
ALTER TABLE [dbo].[EXT_VeicKM] ADD CONSTRAINT [FK__EXT_VeicK__SeqPl__24DEECD0] FOREIGN KEY ([SeqPlanoMnFx]) REFERENCES [dbo].[EXT_VeicPlanoMnFX] ([SeqPlanoMnFx]);
GO
ALTER TABLE [dbo].[EXT_VeicKM] ADD CONSTRAINT [FK__EXT_VeicK__IdVei__7450C40E] FOREIGN KEY ([IdVeicProp]) REFERENCES [dbo].[EXT_VeicProp] ([IdVeicProp]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[EXT_VeicKM] ADD CONSTRAINT [FK__EXT_VeicKM__IDOS__7544E847] FOREIGN KEY ([IDOS]) REFERENCES [dbo].[EXT_OS] ([IDOS]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicMarca
   Criada em ..: 2017-01-17
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicMarca] (
    [IdVeicMarca] numeric(4,0) NOT NULL,
    [Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresaPadrao] numeric(6,0) NULL,
    [SeqTipoManPadrao] decimal(6,0) NULL,
    [SeqPlanoManMarca] numeric(18,0) NULL,
    CONSTRAINT [PK__EXT_Veic__0A2E7064CD4646F6] PRIMARY KEY CLUSTERED ([IdVeicMarca])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VMARCA] ON [dbo].[EXT_VeicMarca] ([Marca]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_VeicMarca] ON [dbo].[EXT_VeicMarca] ([SeqTipoManPadrao]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_VeicMarca] ON [dbo].[EXT_VeicMarca] ([SeqPlanoManMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__770D3528] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__772D30B9] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__6EE2037B] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__25D31109] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__7371AE6E] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__797F8D7F] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__79B497A9] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__5186AABE] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqPl__527ACEF7] FOREIGN KEY ([SeqPlanoManMarca]) REFERENCES [dbo].[EXT_VeicPlanoMan] ([SeqPlanoMAN]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[EXT_VeicMarca] ADD CONSTRAINT [FK__EXT_VeicM__SeqTi__7485CE38] FOREIGN KEY ([SeqTipoManPadrao]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicMarcaEmp
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicMarcaEmp] (
    [SeqVeicMarcaEmp] numeric(18,0) NOT NULL,
    [IdVeicMarca] numeric(4,0) NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [NroEmpresaOrig] numeric(6,0) NOT NULL,
    CONSTRAINT [PK__EXT_Veic__BA864ED09A16FCAA] PRIMARY KEY CLUSTERED ([SeqVeicMarcaEmp])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_VeicMarcaEmp] ON [dbo].[EXT_VeicMarcaEmp] ([SeqVeicMarcaEmp]);
GO
CREATE NONCLUSTERED INDEX [XIE1EXT_VeicMarcaEmp] ON [dbo].[EXT_VeicMarcaEmp] ([IdVeicMarca], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [XIE2EXT_VeicMarcaEmp] ON [dbo].[EXT_VeicMarcaEmp] ([IdVeicMarca], [NroEmpresaOrig]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_VeicMarcaEmp] ON [dbo].[EXT_VeicMarcaEmp] ([IdVeicMarca]);
GO
ALTER TABLE [dbo].[EXT_VeicMarcaEmp] ADD CONSTRAINT [FK__EXT_VeicM__IdVei__7915792B] FOREIGN KEY ([IdVeicMarca]) REFERENCES [dbo].[EXT_VeicMarca] ([IdVeicMarca]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicModelo
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicModelo] (
    [IdVeicModelo] numeric(8,0) NOT NULL,
    [IdVeicFamilia] numeric(6,0) NOT NULL,
    [Modelo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Potencia] numeric(4,0) NULL,
    CONSTRAINT [PK__EXT_Veic__B0D718B76029CE0C] PRIMARY KEY CLUSTERED ([IdVeicModelo])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICMODFAM] ON [dbo].[EXT_VeicModelo] ([IdVeicFamilia]);
GO
ALTER TABLE [dbo].[EXT_VeicModelo] ADD CONSTRAINT [FK__EXT_VeicM__IdVei__7A099D64] FOREIGN KEY ([IdVeicFamilia]) REFERENCES [dbo].[EXT_VeicFam] ([IdVeicFamilia]);
GO
ALTER TABLE [dbo].[EXT_VeicModelo] ADD CONSTRAINT [FK__EXT_VeicM__IdVei__28AF7DB4] FOREIGN KEY ([IdVeicFamilia]) REFERENCES [dbo].[EXT_VeicFam] ([IdVeicFamilia]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicModPlano
   Criada em ..: 2017-02-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicModPlano] (
    [IdVeicModelo] numeric(8,0) NOT NULL,
    [SeqPlanoMAN] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__EXT_Veic__E34788C6B6D17E3B] PRIMARY KEY CLUSTERED ([IdVeicModelo], [SeqPlanoMAN])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_VeicModPlano] ON [dbo].[EXT_VeicModPlano] ([IdVeicModelo], [SeqPlanoMAN]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_VeicModPlano] ON [dbo].[EXT_VeicModPlano] ([IdVeicModelo]);
GO
CREATE NONCLUSTERED INDEX [XIF2EXT_VeicModPlano] ON [dbo].[EXT_VeicModPlano] ([SeqPlanoMAN]);
GO
ALTER TABLE [dbo].[EXT_VeicModPlano] ADD CONSTRAINT [FK__EXT_VeicM__IdVei__29A3A1ED] FOREIGN KEY ([IdVeicModelo]) REFERENCES [dbo].[EXT_VeicModelo] ([IdVeicModelo]);
GO
ALTER TABLE [dbo].[EXT_VeicModPlano] ADD CONSTRAINT [FK__EXT_VeicM__SeqPl__2A97C626] FOREIGN KEY ([SeqPlanoMAN]) REFERENCES [dbo].[EXT_VeicPlanoMan] ([SeqPlanoMAN]);
GO
ALTER TABLE [dbo].[EXT_VeicModPlano] ADD CONSTRAINT [FK__EXT_VeicM__IdVei__7AFDC19D] FOREIGN KEY ([IdVeicModelo]) REFERENCES [dbo].[EXT_VeicModelo] ([IdVeicModelo]);
GO
ALTER TABLE [dbo].[EXT_VeicModPlano] ADD CONSTRAINT [FK__EXT_VeicM__SeqPl__7BF1E5D6] FOREIGN KEY ([SeqPlanoMAN]) REFERENCES [dbo].[EXT_VeicPlanoMan] ([SeqPlanoMAN]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicPlanoMan
   Criada em ..: 2017-01-17
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicPlanoMan] (
    [SeqPlanoMAN] numeric(18,0) NOT NULL,
    [TPVeicSeqPar] numeric(18,0) NOT NULL,
    [SeqTipoMan] decimal(6,0) NOT NULL,
    [Descricao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__EXT_Veic__39090714B3A2A698] PRIMARY KEY CLUSTERED ([SeqPlanoMAN])
);
GO
CREATE NONCLUSTERED INDEX [XIE1EXT_VeicPlanoMan] ON [dbo].[EXT_VeicPlanoMan] ([TPVeicSeqPar]);
GO
CREATE NONCLUSTERED INDEX [XIF1EXT_VeicPlanoMan] ON [dbo].[EXT_VeicPlanoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__2B8BEA5F] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__7CC60E7E] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__7CE60A0F] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__792A87C4] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__7A3EA78E] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__7F3866D5] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__7F6D70FF] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__573F8414] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMan] ADD CONSTRAINT [FK__EXT_VeicP__SeqTi__749ADCD1] FOREIGN KEY ([SeqTipoMan]) REFERENCES [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicPlanoMnFX
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicPlanoMnFX] (
    [SeqPlanoMnFx] numeric(18,0) NOT NULL,
    [SeqPlanoMAN] numeric(18,0) NOT NULL,
    [NroServico] numeric(2,0) NOT NULL,
    [Descricao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Faixa] numeric(18,0) NOT NULL,
    [QTDDiasUltMAN] numeric(4,0) NULL,
    [Gerador] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__EXT_Veic__ED46B9513C44B360] PRIMARY KEY CLUSTERED ([SeqPlanoMnFx]),
    CONSTRAINT [UQ__EXT_Veic__5C2BD78EBE49BB2B] UNIQUE NONCLUSTERED ([SeqPlanoMAN], [NroServico]),
    CONSTRAINT [UQ__EXT_Veic__5C2BD78E5350DF58] UNIQUE NONCLUSTERED ([SeqPlanoMAN], [NroServico])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XAK1EXT_VeicPlanoMnF] ON [dbo].[EXT_VeicPlanoMnFX] ([SeqPlanoMAN], [NroServico]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_PLMANFX] ON [dbo].[EXT_VeicPlanoMnFX] ([SeqPlanoMAN]);
GO
ALTER TABLE [dbo].[EXT_VeicPlanoMnFX] ADD CONSTRAINT [FK__EXT_VeicP__SeqPl__7DDA2E48] FOREIGN KEY ([SeqPlanoMAN]) REFERENCES [dbo].[EXT_VeicPlanoMan] ([SeqPlanoMAN]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicProp
   Criada em ..: 2017-02-03
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicProp] (
    [IdVeicProp] numeric(18,0) NOT NULL,
    [IdVeic] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [IdVendedor] numeric(18,0) NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Dtavenda] datetime NULL,
    [DtaEntrega] datetime NULL,
    [EstadoVenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VlrVenda] numeric(14,2) NULL,
    [TPVendaSeqPar] numeric(18,0) NULL,
    [NroNF] numeric(18,0) NULL,
    [SerieNF] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Revenda] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Financiador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtaprevquitacao] datetime NULL,
    [KMCompra] numeric(8,0) NULL,
    [CanalVenda] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndPropAtual] numeric(1,0) NULL,
    [IndVendaAVISADA] numeric(1,0) NULL,
    [IndRecusaAgd] numeric(1,0) NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndConcessionaria] numeric(1,0) NULL,
    CONSTRAINT [PK__EXT_Veic__B440E9300166C7A2] PRIMARY KEY CLUSTERED ([IdVeicProp])
);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VeicPrpNF] ON [dbo].[EXT_VeicProp] ([NroNF], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICPROP] ON [dbo].[EXT_VeicProp] ([IdVeic]);
GO
CREATE NONCLUSTERED INDEX [IDX_EXT_VEICPROP_0] ON [dbo].[EXT_VeicProp] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF3EXT_VeicProp] ON [dbo].[EXT_VeicProp] ([IdVendedor]);
GO
ALTER TABLE [dbo].[EXT_VeicProp] ADD CONSTRAINT [FK__EXT_VeicP__IdVen__2F5C7B43] FOREIGN KEY ([IdVendedor]) REFERENCES [dbo].[EXT_Vendedor] ([IdVendedor]);
GO
ALTER TABLE [dbo].[EXT_VeicProp] ADD CONSTRAINT [FK__EXT_VeicP__IdVen__00B69AF3] FOREIGN KEY ([IdVendedor]) REFERENCES [dbo].[EXT_Vendedor] ([IdVendedor]);
GO
ALTER TABLE [dbo].[EXT_VeicProp] ADD CONSTRAINT [FK__EXT_VeicP__IdVei__7ECE5281] FOREIGN KEY ([IdVeic]) REFERENCES [dbo].[EXT_Veic] ([IdVeic]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[EXT_VeicProp] ADD CONSTRAINT [FK__EXT_VeicP__SeqPe__7FC276BA] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VEICREF
   Criada em ..: 2025-10-29
   Alterada em : 2025-10-29
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VEICREF] (
    [SEQPROPRIEDADE] int NULL,
    [CAMPOORIGEM] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPODEST] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_VeicTipoMan
   Criada em ..: 2017-01-17
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_VeicTipoMan] (
    [SeqTipoMan] decimal(6,0) NOT NULL,
    [TipoManutencao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndPadrao] numeric(1,0) NULL,
    [DiaToleraServ] decimal(4,0) NULL,
    [KmHrToleraServ] decimal(4,0) NULL,
    [KmHrMinimoXDia] decimal(4,0) NULL,
    [KmHrMaximoXDia] decimal(4,0) NULL,
    CONSTRAINT [PK__EXT_Veic__4CD003F38FDDC4D5] PRIMARY KEY CLUSTERED ([SeqTipoMan])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_VeicTipoMan] ON [dbo].[EXT_VeicTipoMan] ([SeqTipoMan]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.EXT_Vendedor
   Criada em ..: 2017-02-03
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[EXT_Vendedor] (
    [IdVendedor] numeric(18,0) NOT NULL,
    [Nome] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [CodVendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROCPF] numeric(11,0) NULL,
    CONSTRAINT [PK__EXT_Vend__16D6C7CABC45CEAF] PRIMARY KEY CLUSTERED ([IdVendedor])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKEXT_Vendedor] ON [dbo].[EXT_Vendedor] ([IdVendedor]);
GO
CREATE NONCLUSTERED INDEX [XIE1EXT_Vendedor] ON [dbo].[EXT_Vendedor] ([CodVendedor]);
GO
CREATE NONCLUSTERED INDEX [XIE2EXT_Vendedor] ON [dbo].[EXT_Vendedor] ([Nome]);
GO

