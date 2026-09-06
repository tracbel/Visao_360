/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo GE
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Alias
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Alias] (
    [SeqAlias] numeric(18,0) NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SEQ] numeric(18,0) NOT NULL,
    [Origem] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkStr] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ChaveCMPL] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_GE_Alias] PRIMARY KEY CLUSTERED ([SeqAlias])
);
GO
CREATE NONCLUSTERED INDEX [GE_ALIASIE1] ON [dbo].[GE_Alias] ([Tabela], [SEQ]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Aplicacao
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Aplicacao] (
    [SeqAplicacao] numeric(18,0) NOT NULL,
    [CodAplicacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Sistema] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Modulo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALIMITEUSO] datetime NULL,
    [TipoAcesso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RegistrarLog] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Especial1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Aplic__2EFA6BD5FFAAEED4] PRIMARY KEY CLUSTERED ([SeqAplicacao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_AplicacaoPK] ON [dbo].[GE_Aplicacao] ([SeqAplicacao]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_AplicacaoAK1] ON [dbo].[GE_Aplicacao] ([CodAplicacao]);
GO
CREATE NONCLUSTERED INDEX [GE_AplicacaoIF1] ON [dbo].[GE_Aplicacao] ([Sistema], [Modulo]);
GO
ALTER TABLE [dbo].[GE_Aplicacao] ADD CONSTRAINT [FK__GE_Aplicacao__243F02FB] FOREIGN KEY ([Sistema], [Modulo]) REFERENCES [dbo].[GE_Modulo] ([Sistema], [Modulo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_AppModulo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_AppModulo] (
    [Modulo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescRed] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_AppMo__D47F63DC6CCA87DE] PRIMARY KEY CLUSTERED ([Modulo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_AppModuloPK] ON [dbo].[GE_AppModulo] ([Modulo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_AtributoFixo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_AtributoFixo] (
    [Atributo] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Lista] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [UsaFisica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [UsaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [LinkExterno] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroAdic] numeric(18,0) NULL,
    [DadoAdic] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqAtrib] decimal(8,0) NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__GE_Atrib__682FA8127E52E449] PRIMARY KEY CLUSTERED ([Atributo], [Lista])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_AtributoFixoPK] ON [dbo].[GE_AtributoFixo] ([Atributo], [Lista]);
GO
CREATE NONCLUSTERED INDEX [GE_AtributoFixoIE1] ON [dbo].[GE_AtributoFixo] ([NroAdic], [Atributo]);
GO
CREATE NONCLUSTERED INDEX [GE_AtributoFixoIE2] ON [dbo].[GE_AtributoFixo] ([SeqAtrib]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Bairro
   Criada em ..: 2011-12-19
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Bairro] (
    [SeqCidade] decimal(6,0) NOT NULL,
    [SeqBairro] decimal(5,0) NOT NULL,
    [Bairro] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Bairr__CF9E91039F6526F9] PRIMARY KEY CLUSTERED ([SeqCidade], [SeqBairro])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_BairroPK] ON [dbo].[GE_Bairro] ([SeqCidade], [SeqBairro]);
GO
CREATE NONCLUSTERED INDEX [GE_BairroIF1] ON [dbo].[GE_Bairro] ([SeqCidade]);
GO
ALTER TABLE [dbo].[GE_Bairro] ADD CONSTRAINT [FK__GE_Bairro__SeqCi__3EB3CD6C] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GE_Cidade] ([SeqCidade]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CampoExig
   Criada em ..: 2012-04-13
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CampoExig] (
    [Form] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Campo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Tipo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPolSeg] decimal(6,0) NOT NULL,
    [Exige] numeric(1,0) NULL,
    [Dtaalteracao] datetime NULL,
    [Usualteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Campo__FCF5DAD01DC49350] PRIMARY KEY CLUSTERED ([Form], [Campo], [Tipo], [SeqPolSeg])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CampoExigPK] ON [dbo].[GE_CampoExig] ([Form], [Campo], [Tipo], [SeqPolSeg]);
GO
CREATE NONCLUSTERED INDEX [GE_CampoExigIF1] ON [dbo].[GE_CampoExig] ([SeqPolSeg]);
GO
ALTER TABLE [dbo].[GE_CampoExig] ADD CONSTRAINT [FK__GE_CampoE__SeqPo__6C64BE2C] FOREIGN KEY ([SeqPolSeg]) REFERENCES [dbo].[GE_PolSeg] ([SeqPolSeg]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CampoMemo
   Criada em ..: 2018-08-15
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CampoMemo] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [CodAplicacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Campo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Valor] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_GE_CampoMemo] PRIMARY KEY CLUSTERED ([SeqUsuario], [CodAplicacao], [Campo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CampoMemoPK] ON [dbo].[GE_CampoMemo] ([SeqUsuario], [CodAplicacao], [Campo]);
GO
CREATE NONCLUSTERED INDEX [GE_CampoMemoIF1] ON [dbo].[GE_CampoMemo] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[GE_CampoMemo] ADD CONSTRAINT [FK__GE_CampoM__SeqUs__6D58E265] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CampoPerm
   Criada em ..: 2018-08-15
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CampoPerm] (
    [CodAplicacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Campo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Permissao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK_GE_CampoPerm] PRIMARY KEY CLUSTERED ([CodAplicacao], [NroEmpresa], [Campo], [SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CampoPermPK] ON [dbo].[GE_CampoPerm] ([CodAplicacao], [NroEmpresa], [Campo], [SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [GE_CampoPermIF1] ON [dbo].[GE_CampoPerm] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[GE_CampoPerm] ADD CONSTRAINT [FK__GE_CampoP__SeqUs__6E4D069E] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CFinConj
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CFinConj] (
    [SeqCFinFis] numeric(18,0) NOT NULL,
    [SeqPessoaConj] numeric(10,0) NOT NULL,
    [OutraRendaVlr] numeric(14,2) NULL,
    [OutraRendaOrigem] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEmpresa] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCNPJ] numeric(13,0) NULL,
    [OcpCNPJDig] numeric(2,0) NULL,
    [OcpEndereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEnderecoNro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEnderecoCmpl] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFoneDDD] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFone] numeric(12,0) NULL,
    [OcpFoneCmpl] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpSeqCidade] numeric(18,0) NULL,
    [OcpEncarregado] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCidade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFuncao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpBairro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpRenda] numeric(14,2) NULL,
    [OcpCEP] numeric(8,0) NULL,
    [OcpDtaAdmis] datetime NULL,
    [OcpUF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [GE_CFINCONJIF2] ON [dbo].[GE_CFinConj] ([SeqPessoaConj]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CFinDocto
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CFinDocto] (
    [SeqCFinFis] numeric(18,0) NOT NULL,
    [DocReqSeqPar] numeric(18,0) NOT NULL,
    [SeqDocto] numeric(18,0) NULL,
    [SeqDocTp] numeric(4,0) NOT NULL,
    [CondicaoDocSeqPar] numeric(18,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [GE_CFINDOCTOIF1] ON [dbo].[GE_CFinDocto] ([SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [GE_CFINDOCTOIF4] ON [dbo].[GE_CFinDocto] ([SeqDocTp]);
GO
CREATE NONCLUSTERED INDEX [GE_CFINDOCTOIF3] ON [dbo].[GE_CFinDocto] ([DocReqSeqPar]);
GO
CREATE NONCLUSTERED INDEX [GE_CFINDOCTOIF5] ON [dbo].[GE_CFinDocto] ([CondicaoDocSeqPar]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CFinFis
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CFinFis] (
    [SeqCFinFis] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [DtaAtualizacao] datetime NULL,
    [DtaRenovacao] datetime NULL,
    [VlrLimCred] numeric(14,2) NULL,
    [CnfPessoa] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEmpresa] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCNPJ] numeric(13,0) NULL,
    [OcpCNPJDig] numeric(2,0) NULL,
    [OcpFoneDDD] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFone] numeric(12,0) NULL,
    [OcpFoneCmpl] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEndereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEnderecoNro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEnderecoCmpl] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpSeqCidade] numeric(18,0) NULL,
    [OcpEncarregado] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCidade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFuncao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpBairro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpRenda] numeric(14,2) NULL,
    [OcpCEP] numeric(8,0) NULL,
    [OcpDtaAdmis] datetime NULL,
    [OcpUF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpAnterior] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpAFoneDDD] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpAFone] numeric(12,0) NULL,
    [OcpADtaAdmis] datetime NULL,
    [OcpADtaDemis] datetime NULL,
    [OutraRendaVlr] numeric(14,2) NULL,
    [OutraRendaOrigem] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FinancTeve] numeric(1,0) NULL,
    [FinancVlrParcela] numeric(14,2) NULL,
    [FinancBanco] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FinancQuitado] numeric(1,0) NULL,
    [FinancBem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipDocExigido] char(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OutraInf] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfOcpTempo] numeric(4,1) NULL,
    [CnfOcpRenda] numeric(14,2) NULL,
    [CnfOcpSetor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfOcpResponsavel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfDta] datetime NULL,
    [CnfUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AprUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AprDta] datetime NULL,
    [AprObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [GE_CFINFISIE1] ON [dbo].[GE_CFinFis] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [GE_CFINFISIF1] ON [dbo].[GE_CFinFis] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CFinRPes
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CFinRPes] (
    [SeqRPes] numeric(18,0) NOT NULL,
    [SeqCFinFis] numeric(18,0) NOT NULL,
    [NroRef] numeric(2,0) NULL,
    [TipoRelacSeqPar] numeric(18,0) NULL,
    [NOME] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone1DDD] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone1] numeric(12,0) NULL,
    [Fone1Cmpl] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone2DDD] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone2] numeric(12,0) NULL,
    [Fone2Cmpl] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [GE_CFINRPESIF2] ON [dbo].[GE_CFinRPes] ([SeqCFinFis]);
GO
CREATE NONCLUSTERED INDEX [GE_CFINRPESIF1] ON [dbo].[GE_CFinRPes] ([TipoRelacSeqPar]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Cidade
   Criada em ..: 2017-01-22
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Cidade] (
    [SeqCidade] decimal(6,0) NOT NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Regiao] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Populacao] decimal(8,0) NULL,
    [DtaAniversario] datetime NULL,
    [CEPInicial] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEPFinal] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ddd] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFeriado1] datetime NULL,
    [DescFeriado1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFeriado2] datetime NULL,
    [DescFeriado2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExgBairro] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ExgLogradouro] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [INDDESCARTADDD] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_Cidad__95B1809438B79C47] PRIMARY KEY CLUSTERED ([SeqCidade])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CidadePK] ON [dbo].[GE_Cidade] ([SeqCidade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Cidade_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Cidade_BKPJUN] (
    [SeqCidade] decimal(6,0) NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Regiao] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Populacao] decimal(8,0) NULL,
    [DtaAniversario] datetime NULL,
    [CEPInicial] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEPFinal] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ddd] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFeriado1] datetime NULL,
    [DescFeriado1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFeriado2] datetime NULL,
    [DescFeriado2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExgBairro] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ExgLogradouro] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [INDDESCARTADDD] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Cidade_CRM
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Cidade_CRM] (
    [SeqCidade] decimal(6,0) NOT NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Regiao] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Populacao] decimal(8,0) NULL,
    [DtaAniversario] datetime NULL,
    [CEPInicial] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEPFinal] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ddd] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFeriado1] datetime NULL,
    [DescFeriado1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFeriado2] datetime NULL,
    [DescFeriado2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExgBairro] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ExgLogradouro] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_Cidad__95B18094FA87402A] PRIMARY KEY CLUSTERED ([SeqCidade])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CidadePK] ON [dbo].[GE_Cidade_CRM] ([SeqCidade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CIdadePref
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CIdadePref] (
    [SeqCidade] decimal(6,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Ordem] decimal(2,0) NULL,
    CONSTRAINT [PK__GE_CIdad__61688312DA4983A8] PRIMARY KEY CLUSTERED ([SeqCidade], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CIdadePrefPK] ON [dbo].[GE_CIdadePref] ([SeqCidade], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [GE_CIdadePrefIF1] ON [dbo].[GE_CIdadePref] ([SeqCidade]);
GO
ALTER TABLE [dbo].[GE_CIdadePref] ADD CONSTRAINT [FK__GE_CIdade__SeqCi__2903B818] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GE_Cidade_CRM] ([SeqCidade]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Col
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Col] (
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Coluna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TipoDado] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Instrucoes] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndExigido] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_Col__33CEEA481D191254] PRIMARY KEY CLUSTERED ([Tabela], [Coluna])
);
GO
CREATE NONCLUSTERED INDEX [GE_COLIF1] ON [dbo].[GE_Col] ([Tabela]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ColPK] ON [dbo].[GE_Col] ([Tabela], [Coluna]);
GO
ALTER TABLE [dbo].[GE_Col] ADD CONSTRAINT [FK__GE_Col__Tabela__29F7DC51] FOREIGN KEY ([Tabela]) REFERENCES [dbo].[GE_Tab] ([Tabela]);
GO
ALTER TABLE [dbo].[GE_Col] ADD CONSTRAINT [FK__GE_Col__Tabela__70354F10] FOREIGN KEY ([Tabela]) REFERENCES [dbo].[GE_Tab] ([Tabela]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ColPosition
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ColPosition] (
    [Objeto] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Coluna] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Posicao] decimal(4,0) NULL,
    [Tamanho] decimal(6,4) NULL,
    CONSTRAINT [PK__GE_ColPo__BDFC3E87BED0D603] PRIMARY KEY CLUSTERED ([Objeto], [SeqUsuario], [Coluna])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ColPositionPK] ON [dbo].[GE_ColPosition] ([Objeto], [SeqUsuario], [Coluna]);
GO
CREATE NONCLUSTERED INDEX [GE_ColPositionIE1] ON [dbo].[GE_ColPosition] ([Objeto], [SeqUsuario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ColRegra
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ColRegra] (
    [SeqColRegra] numeric(18,0) NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Coluna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPolSeg] numeric(6,0) NOT NULL,
    [Condicao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Regra] numeric(18,0) NULL,
    [SeqPerfil] numeric(6,0) NOT NULL,
    [CKS] numeric(18,0) NULL,
    CONSTRAINT [PK__GE_ColRe__42CA2BCC78C1559C] PRIMARY KEY CLUSTERED ([SeqColRegra])
);
GO
CREATE NONCLUSTERED INDEX [GE_COLREGRAAK1] ON [dbo].[GE_ColRegra] ([Tabela], [Coluna], [SeqPolSeg], [Condicao]);
GO
CREATE NONCLUSTERED INDEX [GE_COLREGRAIF1] ON [dbo].[GE_ColRegra] ([Coluna], [Tabela]);
GO
CREATE NONCLUSTERED INDEX [GE_COLREGRAIF2] ON [dbo].[GE_ColRegra] ([SeqPolSeg], [SeqPerfil]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ColRegraPK] ON [dbo].[GE_ColRegra] ([SeqColRegra]);
GO
ALTER TABLE [dbo].[GE_ColRegra] ADD CONSTRAINT [FK__GE_ColRegra__2AEC008A] FOREIGN KEY ([Tabela], [Coluna]) REFERENCES [dbo].[GE_Col] ([Tabela], [Coluna]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CONSHST
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CONSHST] (
    [SEQCONSHST] numeric(18,0) NOT NULL,
    [SEQCONSSQL] numeric(18,0) NOT NULL,
    [DETALHE] text COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CODUSUARIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTAREALIZACAO] datetime NOT NULL,
    CONSTRAINT [PK__GE_CONSH__CD6635BE40C6BDA9] PRIMARY KEY CLUSTERED ([SEQCONSHST])
);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_CONSHST] ON [dbo].[GE_CONSHST] ([SEQCONSSQL]);
GO
ALTER TABLE [dbo].[GE_CONSHST] ADD CONSTRAINT [FK__GE_CONSHS__SEQCO__110C32F6] FOREIGN KEY ([SEQCONSSQL]) REFERENCES [dbo].[GE_CONSSQL] ([SEQCONSSQL]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CONSSQL
   Criada em ..: 2016-07-12
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CONSSQL] (
    [SEQCONSSQL] numeric(18,0) NOT NULL,
    [SEQRELTEMPLATE] numeric(18,0) NULL,
    [NOME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ADVERTENCIA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUERY] text COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PREVIEW] numeric(1,0) NULL,
    [SESSAO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDEUSO] numeric(18,0) NULL,
    [INDRELSISTEMA] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_CONSS__CF288582D046B8C0] PRIMARY KEY CLUSTERED ([SEQCONSSQL])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVCONSAK11] ON [dbo].[GE_CONSSQL] ([NOME]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_CONSSQL] ON [dbo].[GE_CONSSQL] ([SEQRELTEMPLATE]);
GO
ALTER TABLE [dbo].[GE_CONSSQL] ADD CONSTRAINT [FK__GE_CONSSQ__SEQRE__1200572F] FOREIGN KEY ([SEQRELTEMPLATE]) REFERENCES [dbo].[GE_RELTEMPLATE] ([SEQRELTEMPLATE]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Consulta
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Consulta] (
    [Seqconsulta] numeric(6,0) NOT NULL,
    [Cod] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Seqconsultapai] numeric(6,0) NULL,
    [Tipo] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Consulta] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dtinclusao] datetime NOT NULL,
    [Instrucaosql] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Advertencia] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Banco] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usuario] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Senha] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Conexao] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Colunadado] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipografico] numeric(6,0) NULL,
    [Titulografico] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Titulorodape] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tituloesquerdo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Legeixox] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Colunatexto] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Colunalegenda] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Titulorel] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Arqqrp] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Qrpdireto] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Checksum] numeric(18,0) NULL,
    [DtaInicioUso] datetime NULL,
    [DtaUltUso] datetime NULL,
    [UltUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdeUso] numeric(18,0) NULL,
    CONSTRAINT [PK__GE_Consu__F75B1C4287E91EB2] PRIMARY KEY CLUSTERED ([Seqconsulta])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ConsultaPK] ON [dbo].[GE_Consulta] ([Seqconsulta]);
GO
CREATE NONCLUSTERED INDEX [GE_ConsultaIE1] ON [dbo].[GE_Consulta] ([Cod]);
GO
CREATE NONCLUSTERED INDEX [GE_ConsultaIF1] ON [dbo].[GE_Consulta] ([Seqconsultapai]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ConsultaVar
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ConsultaVar] (
    [Seqconsulta] numeric(6,0) NOT NULL,
    [Variavel] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Instrucao] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Padrao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Minimo] numeric(15,5) NULL,
    [Maximo] numeric(15,5) NULL,
    [Geralista] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipoin] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Retorno] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Consu__2735ED754700E5CB] PRIMARY KEY CLUSTERED ([Seqconsulta], [Variavel])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ConsultaVarPK] ON [dbo].[GE_ConsultaVar] ([Seqconsulta], [Variavel]);
GO
CREATE NONCLUSTERED INDEX [GE_ConsultaVarIF1] ON [dbo].[GE_ConsultaVar] ([Seqconsulta]);
GO
ALTER TABLE [dbo].[GE_ConsultaVar] ADD CONSTRAINT [FK__GE_Consul__Seqco__2DC86D35] FOREIGN KEY ([Seqconsulta]) REFERENCES [dbo].[GE_Consulta] ([Seqconsulta]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CONSVAR
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CONSVAR] (
    [SEQCONSVAR] numeric(18,0) NOT NULL,
    [SEQCONSSQL] numeric(18,0) NOT NULL,
    [VARIAVEL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ORDEM] numeric(18,0) NOT NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PADRAO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTRUCAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUERY] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPODADO] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TUDO] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_CONSV__9B8D36B00FB0B7B0] PRIMARY KEY CLUSTERED ([SEQCONSVAR])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XAK3GE_CONSVAR] ON [dbo].[GE_CONSVAR] ([SEQCONSSQL], [VARIAVEL]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_CONSVAR] ON [dbo].[GE_CONSVAR] ([SEQCONSSQL]);
GO
ALTER TABLE [dbo].[GE_CONSVAR] ADD CONSTRAINT [FK__GE_CONSVA__SEQCO__12F47B68] FOREIGN KEY ([SEQCONSSQL]) REFERENCES [dbo].[GE_CONSSQL] ([SEQCONSSQL]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CONSVARLST
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CONSVARLST] (
    [SEQCONSVARLST] numeric(18,0) NOT NULL,
    [SEQCONSVAR] numeric(18,0) NOT NULL,
    [VALOR] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_CONSV__35DCB034DB12FD09] PRIMARY KEY CLUSTERED ([SEQCONSVARLST])
);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_CONSVARLST] ON [dbo].[GE_CONSVARLST] ([SEQCONSVAR]);
GO
ALTER TABLE [dbo].[GE_CONSVARLST] ADD CONSTRAINT [FK__GE_CONSVA__SEQCO__13E89FA1] FOREIGN KEY ([SEQCONSVAR]) REFERENCES [dbo].[GE_CONSVAR] ([SEQCONSVAR]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Contato
   Criada em ..: 2011-12-19
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Contato] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [EmUso] numeric(1,0) NULL,
    [TipoContato] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AreaAtuacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Rg] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cpf] numeric(18,0) NULL,
    [DigCPF] decimal(2,0) NULL,
    [Saudacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Contato] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] decimal(12,0) NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] decimal(12,0) NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] decimal(12,0) NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EstadoCivil] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascimento] datetime NULL,
    [NivelDecisao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Posicionamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsPessoal] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AtribDta] datetime NULL,
    [AtribNum] numeric(18,0) NULL,
    [EMail] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkWeb] numeric(1,0) NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDWHATSAPPF1] numeric(1,0) NULL,
    [INDWHATSAPPF2] numeric(1,0) NULL,
    [INDWHATSAPPFX] numeric(1,0) NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    CONSTRAINT [PK__GE_Conta__3F7BFE1F99FB1C33] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqContato])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ContatoPK] ON [dbo].[GE_Contato] ([SeqPessoa], [SeqContato]);
GO
CREATE NONCLUSTERED INDEX [GE_ContatoIE1] ON [dbo].[GE_Contato] ([FoneNro1]);
GO
CREATE NONCLUSTERED INDEX [GE_ContatoIE2] ON [dbo].[GE_Contato] ([FoneNro2]);
GO
CREATE NONCLUSTERED INDEX [GE_ContatoIF1] ON [dbo].[GE_Contato] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [PK_GE_CONTATO] ON [dbo].[GE_Contato] ([SeqContato]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Contato_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Contato_BKPJUN] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [EmUso] numeric(1,0) NULL,
    [TipoContato] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AreaAtuacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Rg] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cpf] numeric(18,0) NULL,
    [DigCPF] decimal(2,0) NULL,
    [Saudacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Contato] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] decimal(12,0) NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] decimal(12,0) NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] decimal(12,0) NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EstadoCivil] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascimento] datetime NULL,
    [NivelDecisao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Posicionamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsPessoal] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AtribDta] datetime NULL,
    [AtribNum] numeric(18,0) NULL,
    [EMail] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkWeb] numeric(1,0) NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDWHATSAPPF1] numeric(1,0) NULL,
    [INDWHATSAPPF2] numeric(1,0) NULL,
    [INDWHATSAPPFX] numeric(1,0) NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Contato_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Contato_ITA] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [EmUso] numeric(1,0) NULL,
    [TipoContato] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AreaAtuacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Rg] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cpf] numeric(18,0) NULL,
    [DigCPF] decimal(2,0) NULL,
    [Saudacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Contato] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] decimal(12,0) NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] decimal(12,0) NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] decimal(12,0) NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EstadoCivil] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascimento] datetime NULL,
    [NivelDecisao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Posicionamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsPessoal] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AtribDta] datetime NULL,
    [AtribNum] numeric(18,0) NULL,
    [EMail] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkWeb] numeric(1,0) NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [Observacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDWHATSAPPF1] numeric(1,0) NULL,
    [INDWHATSAPPF2] numeric(1,0) NULL,
    [INDWHATSAPPFX] numeric(1,0) NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CONTATOAPP
   Criada em ..: 2019-02-05
   Alterada em : 2019-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CONTATOAPP] (
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [SEQCONTATO] numeric(4,0) NOT NULL,
    [NOME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [USUARIO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SENHA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDATIVO] numeric(1,0) NULL,
    [DTALOGINANT] datetime NULL,
    [DTALOGIN] datetime NULL,
    [FCMTOKEN] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENDPOINT] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AUTH] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [P256DH] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_CONTA__D54471E68C130075] PRIMARY KEY CLUSTERED ([SEQPESSOA], [SEQCONTATO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_CONTATOAPP_02] ON [dbo].[GE_CONTATOAPP] ([SEQCONTATO]);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_CONTATOAPP_01] ON [dbo].[GE_CONTATOAPP] ([SEQPESSOA]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ContatoEmail
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ContatoEmail] (
    [SeqEmail] numeric(10,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] numeric(4,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_ContatoEmail] ON [dbo].[GE_ContatoEmail] ([SeqEmail], [SeqPessoa], [SeqContato]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ContatoPapel
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ContatoPapel] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [Papel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Conta__22882B3AB9AB6E3F] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqContato], [Papel])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ContatoPapelPK] ON [dbo].[GE_ContatoPapel] ([SeqPessoa], [SeqContato], [Papel]);
GO
CREATE NONCLUSTERED INDEX [GE_ContatoPapelIF1] ON [dbo].[GE_ContatoPapel] ([SeqPessoa], [SeqContato]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ContatoPapel_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ContatoPapel_BKPJUN] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [Papel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ContatoPapel_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ContatoPapel_ITA] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [Papel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CTRL2
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CTRL2] (
    [K01] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DD] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DA1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_CTRL2__C41D9C18855898AB] PRIMARY KEY CLUSTERED ([K01])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CtrlE
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CtrlE] (
    [K01] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Sfan] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Srso] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Snrd] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nrcg] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dgcg] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dglb] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Datu] datetime NULL,
    CONSTRAINT [PK__GE_CtrlE__C41D9C18A6D75B17] PRIMARY KEY CLUSTERED ([K01])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CtrlEPK] ON [dbo].[GE_CtrlE] ([K01]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CtrlM
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CtrlM] (
    [K01] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dexp] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nusr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ctemp] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Datu] datetime NULL,
    [Dglb] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KDESC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEXPTMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEXPQTD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_CtrlM__C41D9C183308D05F] PRIMARY KEY CLUSTERED ([K01])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CtrlMPK] ON [dbo].[GE_CtrlM] ([K01]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CtrlME
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CtrlME] (
    [K01] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dexp] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nusr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Datu] datetime NULL,
    [Dglb] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_CtrlM__C41D9C18ADF6C1B8] PRIMARY KEY CLUSTERED ([K01])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_CtrlMEPK] ON [dbo].[GE_CtrlME] ([K01]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CTRLMU
   Criada em ..: 2019-02-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CTRLMU] (
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [K01] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DATU] datetime NULL,
    [INDATIVO] numeric(1,0) NULL,
    [INDTEMPORARIO] numeric(1,0) NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_CTRLMU_SEQUSUARIO] ON [dbo].[GE_CTRLMU] ([SEQUSUARIO]);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_CTRLMU_K01] ON [dbo].[GE_CTRLMU] ([K01]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_CTRLUSR
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_CTRLUSR] (
    [K01] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SPW] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SNVL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DGER] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATU] datetime NULL,
    CONSTRAINT [PK__GE_CTRLU__C41D9C1826DB7678] PRIMARY KEY CLUSTERED ([K01])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_DiaNaoUtil
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_DiaNaoUtil] (
    [Data] datetime NOT NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Motivo] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_DiaNa__77387D0A22948A4F] PRIMARY KEY CLUSTERED ([Data])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_DiaNaoUtilPK] ON [dbo].[GE_DiaNaoUtil] ([Data]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Email
   Criada em ..: 2014-09-15
   Alterada em : 2021-06-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Email] (
    [SeqEmail] numeric(10,0) NOT NULL,
    [eMail] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [IndPreferencial] numeric(1,0) NULL,
    [IndUsoMkt] numeric(1,0) NULL,
    [IndUsoProfissional] numeric(1,0) NULL,
    [IndUsoPessoal] numeric(1,0) NULL,
    [IndUsoFiscal] numeric(1,0) NULL,
    [IndEmUso] numeric(1,0) NULL,
    [Senha] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAEMAILATIVO] datetime NULL,
    CONSTRAINT [PK_GE_Email] PRIMARY KEY CLUSTERED ([SeqEmail])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_Email] ON [dbo].[GE_Email] ([SeqEmail]);
GO
CREATE NONCLUSTERED INDEX [XIE1GE_Email] ON [dbo].[GE_Email] ([eMail]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_EMAILAGD
   Criada em ..: 2016-07-12
   Alterada em : 2024-12-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_EMAILAGD] (
    [SEQEMAILAGD] numeric(18,0) NOT NULL,
    [SEQEMAILTEMPLATE] numeric(18,0) NOT NULL,
    [SEQCONSSQL] numeric(18,0) NOT NULL,
    [RELANEXO] numeric(1,0) NULL,
    [NOME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CCO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERVALOEXEC] numeric(18,0) NULL,
    [PLANOEXEC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINICIO] datetime NULL,
    [FILTROVALOR] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_EMAIL__BA7D91569876B8A9] PRIMARY KEY CLUSTERED ([SEQEMAILAGD])
);
GO
CREATE NONCLUSTERED INDEX [XIE1GE_EMAILAGD] ON [dbo].[GE_EMAILAGD] ([DTAINICIO]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_EMAILAGD] ON [dbo].[GE_EMAILAGD] ([SEQEMAILTEMPLATE]);
GO
CREATE NONCLUSTERED INDEX [XIF2GE_EMAILAGD] ON [dbo].[GE_EMAILAGD] ([SEQCONSSQL]);
GO
ALTER TABLE [dbo].[GE_EMAILAGD] ADD CONSTRAINT [FK__GE_EMAILA__SEQEM__14DCC3DA] FOREIGN KEY ([SEQEMAILTEMPLATE]) REFERENCES [dbo].[GE_EMAILTEMPLATE] ([SEQEMAILTEMPLATE]);
GO
ALTER TABLE [dbo].[GE_EMAILAGD] ADD CONSTRAINT [FK__GE_EMAILA__SEQCO__15D0E813] FOREIGN KEY ([SEQCONSSQL]) REFERENCES [dbo].[GE_CONSSQL] ([SEQCONSSQL]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_EMAILINVALIDO
   Criada em ..: 2018-03-06
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_EMAILINVALIDO] (
    [EMAIL] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    CONSTRAINT [PK__GE_EMAIL__161CF725D4575611] PRIMARY KEY CLUSTERED ([EMAIL])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_EmailSpam
   Criada em ..: 2014-09-15
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_EmailSpam] (
    [SeqEmailSpam] numeric(18,0) NOT NULL,
    [eMail] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_GE_EmailSpam] PRIMARY KEY CLUSTERED ([SeqEmailSpam])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_EmailSpam] ON [dbo].[GE_EmailSpam] ([SeqEmailSpam]);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_EMAILSPAM_01] ON [dbo].[GE_EmailSpam] ([eMail]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_EMAILTEMPLATE
   Criada em ..: 2016-07-12
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_EMAILTEMPLATE] (
    [SEQEMAILTEMPLATE] numeric(18,0) NOT NULL,
    [TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NOME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ASSUNTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MENSAGEM] text COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_EMAIL__02F93BE9F4CAC59F] PRIMARY KEY CLUSTERED ([SEQEMAILTEMPLATE])
);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_EMAILTEMPLATE_01] ON [dbo].[GE_EMAILTEMPLATE] ([TIPO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Empresa
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Empresa] (
    [NroEmpresa] numeric(6,0) NOT NULL,
    [EmUso] numeric(1,0) NULL,
    [Fantasia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [RazaoSocial] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeReduzido] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Endereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EnderecoNRO] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cep] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cidade] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Estado] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscrEstadual] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscrMunicipal] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OutraInscricao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCGC] decimal(15,0) NULL,
    [DigCGC] decimal(2,0) NULL,
    [FoneNro] decimal(8,0) NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] decimal(8,0) NULL,
    [ArqLogo] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Matriz] decimal(3,0) NULL,
    [DtaAlteracao] datetime NULL,
    [email] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [homepage] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmpSeguranca] decimal(3,0) NULL,
    [Ddd] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fax] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sigla] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FusoHorario] decimal(2,0) NULL,
    [Regional] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Empre__4D90386E64151DBA] PRIMARY KEY CLUSTERED ([NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_EmpresaPK] ON [dbo].[GE_Empresa] ([NroEmpresa]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_EmpresaAK1] ON [dbo].[GE_Empresa] ([NomeReduzido]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FCadConj
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FCadConj] (
    [SeqFCadF] numeric(18,0) NOT NULL,
    [Nome] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [RG] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascimento] datetime NULL,
    [OcpTipo] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEmpresa] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCNPJ] decimal(13,0) NULL,
    [OcpCNPJDig] decimal(2,0) NULL,
    [OcpEndereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpSeqCidade] numeric(18,0) NULL,
    [OcpCidade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpBairro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpUF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCEP] decimal(8,0) NULL,
    [OcpFone] decimal(12,0) NULL,
    [OcpEncarregado] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFuncao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpRenda] decimal(15,2) NULL,
    [OcpDtaAdmis] datetime NULL,
    [OcpAnterior] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpAFone] decimal(11,0) NULL,
    [OcpADtaAdmis] datetime NULL,
    [OcpADtaDemis] datetime NULL,
    [Ocp2Origem] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Endereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2SeqCidade] numeric(18,0) NULL,
    [Ocp2Cidade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Bairro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2UF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2CEP] decimal(8,0) NULL,
    [Ocp2Fone] decimal(12,0) NULL,
    [Ocp2Encarregado] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Funcao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Renda] decimal(15,2) NULL,
    [Ocp2DtaAdmis] datetime NULL,
    [OcpContador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCtdFone] decimal(11,0) NULL,
    [CnfOcpSetor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfDta] datetime NULL,
    [CnfUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_FCadC__9EDAB5C24096A028] PRIMARY KEY CLUSTERED ([SeqFCadF])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FCadConjPK] ON [dbo].[GE_FCadConj] ([SeqFCadF]);
GO
ALTER TABLE [dbo].[GE_FCadConj] ADD CONSTRAINT [FK__GE_FCadCo__SeqFC__30A4D9E0] FOREIGN KEY ([SeqFCadF]) REFERENCES [dbo].[GE_FCadF] ([SeqFCadF]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FCadF
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FCadF] (
    [SeqFCadF] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [ObjFinanc] numeric(1,0) NULL,
    [ObjConsor] numeric(1,0) NULL,
    [ObjCredProp] numeric(1,0) NULL,
    [Avalista] numeric(1,0) NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [DtaAtualizacao] datetime NULL,
    [DtaRenovacao] datetime NULL,
    [VlrLimCred] decimal(15,2) NULL,
    [CnfPessoa] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpTipo] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpEmpresa] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCNPJ] decimal(13,0) NULL,
    [OcpCNPJDig] decimal(2,0) NULL,
    [OcpEndereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpSeqCidade] numeric(18,0) NULL,
    [OcpCidade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpBairro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCEP] decimal(8,0) NULL,
    [OcpUF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFone] decimal(12,0) NULL,
    [OcpEncarregado] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpFuncao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpRenda] decimal(15,2) NULL,
    [OcpDtaAdmis] datetime NULL,
    [OcpAnterior] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpAFone] decimal(11,0) NULL,
    [OcpADtaAdmis] datetime NULL,
    [OcpADtaDemis] datetime NULL,
    [Ocp2Origem] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Endereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2SeqCidade] numeric(18,0) NULL,
    [Ocp2Cidade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Bairro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2UF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2CEP] decimal(8,0) NULL,
    [Ocp2Fone] decimal(12,0) NULL,
    [Ocp2Encarregado] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Funcao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ocp2Renda] decimal(15,2) NULL,
    [Ocp2DtaAdmis] datetime NULL,
    [OcpContador] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OcpCtdFone] decimal(11,0) NULL,
    [MorTipo] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MorTempoRes] decimal(4,1) NULL,
    [MorTempoResAnt] decimal(4,1) NULL,
    [MorTempoResCid] decimal(4,1) NULL,
    [MorImob] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MorImobFone] decimal(11,0) NULL,
    [MorImobCtto] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MorImobAnt] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MorImobAFone] decimal(11,0) NULL,
    [MorImobACtto] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PI1Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PI1Matric] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PI1Vlr] decimal(15,2) NULL,
    [PI1Local] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PI2Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PI2Matric] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PI2Vlr] decimal(15,2) NULL,
    [PI2Local] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA1Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA1Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA1Modelo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA1Ano] decimal(4,0) NULL,
    [PA1Vlr] decimal(15,2) NULL,
    [PA1Placa] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA1Alienado] numeric(1,0) NULL,
    [PA1Qtde] decimal(4,0) NULL,
    [PA1Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA2Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pa2Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA2Modelo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA2Ano] decimal(4,0) NULL,
    [PA2Vlr] decimal(15,2) NULL,
    [PA2Placa] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PA2Alienado] numeric(1,0) NULL,
    [PA2Qtde] decimal(4,0) NULL,
    [PA2Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FinancTeve] numeric(1,0) NULL,
    [FinancBanco] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FinancQuitado] numeric(1,0) NULL,
    [FinancBem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OutraInf] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfOcpTempo] decimal(4,1) NULL,
    [CnfOcpRenda] decimal(15,2) NULL,
    [CnfOcpSetor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfOcpResponsavel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfDta] datetime NULL,
    [CnfUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AprUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AprDta] datetime NULL,
    [AprObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FinancVlrParcela] decimal(15,2) NULL,
    CONSTRAINT [PK__GE_FCadF__9EDAB5C2B60DAC3D] PRIMARY KEY CLUSTERED ([SeqFCadF])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FCadFPK] ON [dbo].[GE_FCadF] ([SeqFCadF]);
GO
CREATE NONCLUSTERED INDEX [GE_FCadFIE1] ON [dbo].[GE_FCadF] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [GE_FCadFIF1] ON [dbo].[GE_FCadF] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_FCadF] ADD CONSTRAINT [FK__GE_FCadF__SeqPes__77D670D8] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_FCadF] ADD CONSTRAINT [FK__GE_FCadF__SeqPes__3198FE19] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FCadInstCred
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FCadInstCred] (
    [SeqFCadIC] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [TipoFcad] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqFcad] numeric(18,0) NOT NULL,
    [InstCred] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PendCheque] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Protesto] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Acao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltRegistro] datetime NULL,
    [Pontual] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfDta] datetime NULL,
    [CnfUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_FCadI__466133F1030D41AA] PRIMARY KEY CLUSTERED ([SeqFCadIC])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FCadInstCredPK] ON [dbo].[GE_FCadInstCred] ([SeqFCadIC]);
GO
CREATE NONCLUSTERED INDEX [GE_FCadInstCredIE1] ON [dbo].[GE_FCadInstCred] ([SeqFcad], [TipoFcad]);
GO
CREATE NONCLUSTERED INDEX [GE_FCadInstCredIF1] ON [dbo].[GE_FCadInstCred] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_FCadInstCred] ADD CONSTRAINT [FK__GE_FCadIn__SeqPe__78CA9511] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FcadRBanc
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FcadRBanc] (
    [SeqRBanc] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [TipoFcad] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqFcad] numeric(18,0) NOT NULL,
    [NroRef] decimal(1,0) NOT NULL,
    [SeqRef] numeric(18,0) NULL,
    [Banco] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Agencia] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CtaCorr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAberturaCta] datetime NULL,
    [Fone] decimal(11,0) NULL,
    [Contato] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CartCred1] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CartCred2] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoaRef] numeric(10,0) NULL,
    [CnfDta] datetime NULL,
    [CnfUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_FcadR__5F8AF3E034BED1DE] PRIMARY KEY CLUSTERED ([SeqRBanc])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FcadRBancPK] ON [dbo].[GE_FcadRBanc] ([SeqRBanc]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FcadRBancAK1] ON [dbo].[GE_FcadRBanc] ([SeqFcad], [TipoFcad], [NroRef]);
GO
CREATE NONCLUSTERED INDEX [GE_FcadRBancIF1] ON [dbo].[GE_FcadRBanc] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_FcadRBanc] ADD CONSTRAINT [FK__GE_FcadRB__SeqPe__79BEB94A] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FcadRCom
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FcadRCom] (
    [SeqRCom] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [TipoFcad] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TipoRef] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqFcad] numeric(18,0) NOT NULL,
    [NroRef] decimal(1,0) NOT NULL,
    [Empresa] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone] decimal(11,0) NULL,
    [Contato] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoaRef] numeric(10,0) NULL,
    [CnfDta] datetime NULL,
    [CnfUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfContato] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfTempoCliente] decimal(4,1) NULL,
    [CnfPontual] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfTempoAtraso] decimal(4,0) NULL,
    [CnfDtaUltCompra] datetime NULL,
    [CnfVlrUltCompra] decimal(15,2) NULL,
    [CnfVlrMedio] decimal(15,2) NULL,
    [CnfObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_FcadR__0183399111DF8701] PRIMARY KEY CLUSTERED ([SeqRCom])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FcadRComPK] ON [dbo].[GE_FcadRCom] ([SeqRCom]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FcadRComAK1] ON [dbo].[GE_FcadRCom] ([SeqFcad], [TipoFcad], [NroRef]);
GO
CREATE NONCLUSTERED INDEX [GE_FcadRComIF1] ON [dbo].[GE_FcadRCom] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_FcadRCom] ADD CONSTRAINT [FK__GE_FcadRC__SeqPe__7AB2DD83] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_FcadRCom] ADD CONSTRAINT [FK__GE_FcadRC__SeqPe__34756AC4] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FCadRPes
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FCadRPes] (
    [SeqRPes] numeric(18,0) NOT NULL,
    [SeqFCadF] numeric(18,0) NOT NULL,
    [NroRef] decimal(1,0) NULL,
    [Nome] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GrauRelacionam] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [PercPart] decimal(6,2) NULL,
    [DtaEntradaSocio] datetime NULL,
    [Endereco] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqCidade] numeric(18,0) NULL,
    [Cidade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fone1] decimal(12,0) NULL,
    [Fone2] decimal(12,0) NULL,
    [Fone3] decimal(12,0) NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoaRef] numeric(10,0) NULL,
    [CnfDta] datetime NULL,
    [CnfUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfContato] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfGrauRelac] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CnfTempoRes] decimal(4,1) NULL,
    [CnfObs] varchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndAssinaEmpr] numeric(1,0) NULL,
    [EstadoCivil] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEP] decimal(8,0) NULL,
    [Renda] decimal(15,2) NULL,
    [ConjNome] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ConjNroCPF] decimal(13,0) NULL,
    [ConjDigCPF] decimal(2,0) NULL,
    [ConjDtaNascimento] datetime NULL,
    CONSTRAINT [PK__GE_FCadR__06800694243B0A95] PRIMARY KEY CLUSTERED ([SeqRPes])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FCadRPesPK] ON [dbo].[GE_FCadRPes] ([SeqRPes]);
GO
CREATE NONCLUSTERED INDEX [GE_FCadRPesIF1] ON [dbo].[GE_FCadRPes] ([SeqFCadF]);
GO
ALTER TABLE [dbo].[GE_FCadRPes] ADD CONSTRAINT [FK__GE_FCadRP__SeqFC__35698EFD] FOREIGN KEY ([SeqFCadF]) REFERENCES [dbo].[GE_FCadF] ([SeqFCadF]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FERIADO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FERIADO] (
    [SEQFERIADO] numeric(4,0) NOT NULL,
    [DATA] datetime NOT NULL,
    [INDFIXO] numeric(1,0) NULL,
    [DESCRICAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [TIPO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_FERIA__51C519F3A9649081] PRIMARY KEY CLUSTERED ([SEQFERIADO])
);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_FERIADO] ON [dbo].[GE_FERIADO] ([DATA]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Figura
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Figura] (
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Codigo] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Figura] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Arquivo] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoArquivo] decimal(4,0) NULL,
    [TamanhoArquivo] numeric(18,0) NULL,
    [FIGURABIN] image NULL,
    CONSTRAINT [PK__GE_Figur__2158CC57DFAB1BB2] PRIMARY KEY CLUSTERED ([Tipo], [Codigo], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_FiguraPK] ON [dbo].[GE_Figura] ([Tipo], [Codigo], [NroEmpresa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_FONEINVALIDO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_FONEINVALIDO] (
    [TELEFONE] numeric(11,0) NOT NULL,
    [ORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    CONSTRAINT [PK__GE_FONEI__D6F1694E28C52261] PRIMARY KEY CLUSTERED ([TELEFONE])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Grafico
   Criada em ..: 2013-12-20
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Grafico] (
    [SeqGraf] numeric(18,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Kn1] numeric(18,0) NOT NULL,
    [Kn2] numeric(18,0) NOT NULL,
    [Ks1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Titulo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TituloEsquerdo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TituloRodape] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] numeric(2,0) NULL,
    [Estilo] numeric(2,0) NULL,
    [True3D] numeric(1,0) NULL,
    [LabelAtivo] numeric(1,0) NULL,
    [LabelPerc] numeric(1,0) NULL,
    [LabelComLegenda] numeric(1,0) NULL,
    [QtdeMaxItens] numeric(2,0) NULL,
    [Grid] numeric(1,0) NULL,
    [ColLegenda] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ColDado] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ColCor] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LegendaPos] numeric(2,0) NULL,
    [YEscalaEstilo] numeric(2,0) NULL,
    [YEscalaMax] numeric(18,0) NULL,
    [YEscalaQtde] numeric(2,0) NULL,
    [XEscalaEstilo] numeric(2,0) NULL,
    [XEscalaMax] numeric(18,0) NULL,
    [XEscalaQtde] numeric(2,0) NULL
);
GO
CREATE NONCLUSTERED INDEX [GE_GRAFICOAK1] ON [dbo].[GE_Grafico] ([Origem], [Kn1], [Kn2], [Ks1]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Help
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Help] (
    [SeqAplicacao] numeric(18,0) NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Help] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Help__2EFA6BD51A3422BC] PRIMARY KEY CLUSTERED ([SeqAplicacao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_HelpPK] ON [dbo].[GE_Help] ([SeqAplicacao]);
GO
ALTER TABLE [dbo].[GE_Help] ADD CONSTRAINT [FK__GE_Help__SeqApli__365DB336] FOREIGN KEY ([SeqAplicacao]) REFERENCES [dbo].[GE_Aplicacao] ([SeqAplicacao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_IMPORTA_CART
   Criada em ..: 2018-02-02
   Alterada em : 2018-02-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_IMPORTA_CART] (
    [SeqPessoa] numeric(10,0) NULL,
    [SEQPESSOAPRC] numeric(10,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeRazao] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fantasia] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroLogradouro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cep] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [Atividade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RendaFaturamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] decimal(12,0) NULL,
    [Tipo1] numeric(1,0) NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] decimal(12,0) NULL,
    [Tipo2] numeric(1,0) NULL,
    [FoneDDD3] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro3] decimal(12,0) NULL,
    [Tipo3] numeric(1,0) NULL,
    [FoneDDD4] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro4] decimal(12,0) NULL,
    [Tipo4] numeric(1,0) NULL,
    [FoneDDD5] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro5] decimal(12,0) NULL,
    [Tipo5] numeric(1,0) NULL,
    [FoneDDD6] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro6] decimal(12,0) NULL,
    [Tipo6] numeric(1,0) NULL,
    [FoneDDD7] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro7] decimal(12,0) NULL,
    [Tipo7] numeric(1,0) NULL,
    [FoneDDD8] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro8] decimal(12,0) NULL,
    [Tipo8] numeric(1,0) NULL,
    [Email1] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Email2] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Email3] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCCNAE] varchar(180) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NJUR] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCNJUR] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEMRECEITA] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_IntCtrl
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_IntCtrl] (
    [Tipo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [StrK] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroK] numeric(18,0) NOT NULL,
    [Ind1] numeric(1,0) NULL,
    [Ind2] numeric(1,0) NULL,
    [Str1] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nro1] numeric(18,0) NULL,
    [Dta1] datetime NULL,
    [Obs] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_IntCt__5C8BB26F3E498CA7] PRIMARY KEY CLUSTERED ([Tipo], [NroEmpresa], [StrK], [NroK])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_IntCtrlPK] ON [dbo].[GE_IntCtrl] ([Tipo], [NroEmpresa], [StrK], [NroK]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LgTb
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LgTb] (
    [Tb] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Kn1] numeric(18,0) NOT NULL,
    [Kn2] numeric(18,0) NOT NULL,
    [Ks] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaLog] datetime NOT NULL,
    [Usr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodApl] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK__GE_LgTb__3765F65882301789] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LgTbIE1] ON [dbo].[GE_LgTb] ([Tb], [Kn1]);
GO
CREATE NONCLUSTERED INDEX [GE_LgTbIE2] ON [dbo].[GE_LgTb] ([Tb], [Ks]);
GO
CREATE NONCLUSTERED INDEX [XIE3GE_LgTb] ON [dbo].[GE_LgTb] ([DtaLog]);
GO
CREATE NONCLUSTERED INDEX [GE_LGTBIE3] ON [dbo].[GE_LgTb] ([Tb], [DtaLog]) INCLUDE ([SEQLOGTB]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_CARTCRED
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_CARTCRED] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_C__3765F65855908242] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LGCRTCRIE1_4] ON [dbo].[GE_LOG_CARTCRED] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CRTCRIE2_4] ON [dbo].[GE_LOG_CARTCRED] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CRTCRIE4_4] ON [dbo].[GE_LOG_CARTCRED] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CRTCRIE3_4] ON [dbo].[GE_LOG_CARTCRED] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_CONFIG
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_CONFIG] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_C__3765F65876E8FAF6] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CFG_IE1_5] ON [dbo].[GE_LOG_CONFIG] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CFG_IE2_5] ON [dbo].[GE_LOG_CONFIG] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CFG_IE4_5] ON [dbo].[GE_LOG_CONFIG] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CFG_IE3_5] ON [dbo].[GE_LOG_CONFIG] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_CONTATO
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_CONTATO] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_C__3765F65893DA8C1D] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CTTOIE1_2] ON [dbo].[GE_LOG_CONTATO] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CTTOIE2_2] ON [dbo].[GE_LOG_CONTATO] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CTTOIE4_2] ON [dbo].[GE_LOG_CONTATO] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_CTTOIE3_2] ON [dbo].[GE_LOG_CONTATO] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_EXT
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_EXT] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_E__3765F658AA2F0107] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LG_EXTIE1_6] ON [dbo].[GE_LOG_EXT] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_EXTIE2_6] ON [dbo].[GE_LOG_EXT] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_EXTIE4_6] ON [dbo].[GE_LOG_EXT] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_EXTIE3_6] ON [dbo].[GE_LOG_EXT] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_HISTORICO
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_HISTORICO] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_H__3765F658D516AC5F] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LG_HSTIE1_0] ON [dbo].[GE_LOG_HISTORICO] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_HSTIE2_0] ON [dbo].[GE_LOG_HISTORICO] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_HSTIE4_0] ON [dbo].[GE_LOG_HISTORICO] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_HSTIE3_0] ON [dbo].[GE_LOG_HISTORICO] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_PESSOA
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_PESSOA] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_P__3765F658FF973A8C] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PESIE1_1] ON [dbo].[GE_LOG_PESSOA] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PESIE2_1] ON [dbo].[GE_LOG_PESSOA] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PESIE4_1] ON [dbo].[GE_LOG_PESSOA] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PESIE3_1] ON [dbo].[GE_LOG_PESSOA] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_PROCESSO
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_PROCESSO] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_P__3765F658B450BAF7] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PRC_IE1_3] ON [dbo].[GE_LOG_PROCESSO] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PRCIE2_3] ON [dbo].[GE_LOG_PROCESSO] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PRCIE4_3] ON [dbo].[GE_LOG_PROCESSO] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_PRCIE3_3] ON [dbo].[GE_LOG_PROCESSO] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LOG_TRANS
   Criada em ..: 2023-06-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LOG_TRANS] (
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [TB] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NOT NULL,
    [KN2] numeric(18,0) NOT NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALOG] datetime NOT NULL,
    [USR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODAPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LOG_T__3765F658708DDE74] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO
CREATE NONCLUSTERED INDEX [GE_LG_TRNSIE1_7] ON [dbo].[GE_LOG_TRANS] ([TB], [KN1]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_TRNSIE2_7] ON [dbo].[GE_LOG_TRANS] ([TB], [KS]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_TRNSIE4_7] ON [dbo].[GE_LOG_TRANS] ([DTALOG]);
GO
CREATE NONCLUSTERED INDEX [GE_LG_TRNSIE3_7] ON [dbo].[GE_LOG_TRANS] ([TB], [KN1], [KN2], [KS], [DTALOG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Log2
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Log2] (
    [NroEmpresa] numeric(6,0) NULL,
    [Sistema] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Modulo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Aplicacao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data] datetime NULL,
    [Estacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] numeric(1,0) NULL,
    [Resumo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Detalhe] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkTipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkSerie] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQLOG] numeric(18,0) IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK__GE_Log2__017D4447051D4F7B] PRIMARY KEY CLUSTERED ([SEQLOG])
);
GO
CREATE NONCLUSTERED INDEX [GE_Log2IE1] ON [dbo].[GE_Log2] ([LinkNro]);
GO
CREATE NONCLUSTERED INDEX [GE_Log2IE2] ON [dbo].[GE_Log2] ([Data]);
GO
CREATE NONCLUSTERED INDEX [GE_Log2IE3] ON [dbo].[GE_Log2] ([CodUsuario]);
GO
CREATE NONCLUSTERED INDEX [GE_Log2IE4] ON [dbo].[GE_Log2] ([Aplicacao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_LogAtividade
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_LogAtividade] (
    [SeqLogAtividade] numeric(18,0) NOT NULL,
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NULL,
    [KN2] numeric(18,0) NULL,
    [KS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaLog] datetime NOT NULL,
    [Host] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Resumo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Detalhe] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_LogAt__B5AEFE609CDB2E9A] PRIMARY KEY CLUSTERED ([SeqLogAtividade])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_LogAtividade] ON [dbo].[GE_LogAtividade] ([SeqLogAtividade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Membro
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Membro] (
    [Grupo] numeric(18,0) NOT NULL,
    [Usuario] numeric(18,0) NOT NULL,
    [INDATIVO] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_Membr__AE31A404F5379463] PRIMARY KEY CLUSTERED ([Grupo], [Usuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_MembroPK] ON [dbo].[GE_Membro] ([Grupo], [Usuario]);
GO
CREATE NONCLUSTERED INDEX [GE_MembroIF1] ON [dbo].[GE_Membro] ([Usuario]);
GO
CREATE NONCLUSTERED INDEX [GE_MembroIF2] ON [dbo].[GE_Membro] ([Grupo]);
GO
ALTER TABLE [dbo].[GE_Membro] ADD CONSTRAINT [FK__GE_Membro__Usuar__7D8F4A2E] FOREIGN KEY ([Usuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Membro_BKP20250520
   Criada em ..: 2025-05-20
   Alterada em : 2025-05-20
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Membro_BKP20250520] (
    [Grupo] numeric(18,0) NOT NULL,
    [Usuario] numeric(18,0) NOT NULL,
    [INDATIVO] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_MobileConfig
   Criada em ..: 2016-07-12
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_MobileConfig] (
    [SeqMobileConfig] numeric(10,0) NOT NULL,
    [Nome] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Acao] numeric(6,0) NULL,
    [Resultado] numeric(6,0) NULL,
    [SeqPropriedade] numeric(4,0) NULL,
    [SeqDocTp] numeric(4,0) NULL,
    [CompresFoto] numeric(2,0) NULL,
    [IntervaloSync] numeric(8,0) NULL,
    [ExigeFoto] numeric(1,0) NULL,
    [ExigeContato] numeric(1,0) NULL,
    [ExigeContatoFone] numeric(1,0) NULL,
    [ExigeContatoEmail] numeric(1,0) NULL,
    [ExigeDtaNasc] numeric(1,0) NULL,
    [GRLimiteRario] numeric(8,0) NULL,
    CONSTRAINT [PK__GE_Mobil__54575050328EE2A5] PRIMARY KEY CLUSTERED ([SeqMobileConfig])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_MobileConfig] ON [dbo].[GE_MobileConfig] ([SeqMobileConfig]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_MobileConfig] ON [dbo].[GE_MobileConfig] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [XIF2GE_MobileConfig] ON [dbo].[GE_MobileConfig] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [XIF3GE_MobileConfig] ON [dbo].[GE_MobileConfig] ([SeqPropriedade]);
GO
CREATE NONCLUSTERED INDEX [XIF4GE_MobileConfig] ON [dbo].[GE_MobileConfig] ([SeqDocTp]);
GO
ALTER TABLE [dbo].[GE_MobileConfig] ADD CONSTRAINT [FK__GE_Mobile__SeqPr__5C63608D] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_MOBILEPROP
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_MOBILEPROP] (
    [SEQMOBILECONFIG] numeric(10,0) NOT NULL,
    [SEQPROPRIEDADE] numeric(4,0) NOT NULL,
    CONSTRAINT [PK__GE_MOBIL__912F0C35374F234A] PRIMARY KEY CLUSTERED ([SEQMOBILECONFIG], [SEQPROPRIEDADE])
);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_MOBILEPRO_SEQMOBILECON] ON [dbo].[GE_MOBILEPROP] ([SEQMOBILECONFIG]);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_MOBILEPRO_SEQPROPRIEDA] ON [dbo].[GE_MOBILEPROP] ([SEQPROPRIEDADE]);
GO
ALTER TABLE [dbo].[GE_MOBILEPROP] ADD CONSTRAINT [FK__GE_MOBILE__SEQMO__4D76EE0F] FOREIGN KEY ([SEQMOBILECONFIG]) REFERENCES [dbo].[GE_MobileConfig] ([SeqMobileConfig]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[GE_MOBILEPROP] ADD CONSTRAINT [FK__GE_MOBILE__SEQPR__4E6B1248] FOREIGN KEY ([SEQPROPRIEDADE]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Mod
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Mod] (
    [Modulo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tpac] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vers] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Datu] datetime NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SISTEMAGE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODULOGE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Mod__D47F63DCD4B895ED] PRIMARY KEY CLUSTERED ([Modulo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ModPK] ON [dbo].[GE_Mod] ([Modulo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Modulo
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Modulo] (
    [Sistema] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Modulo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SiglaModulo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAcesso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Versao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Modul__E424BB092EAD87CC] PRIMARY KEY CLUSTERED ([Sistema], [Modulo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ModuloPK] ON [dbo].[GE_Modulo] ([Sistema], [Modulo]);
GO
CREATE NONCLUSTERED INDEX [GE_ModuloIF1] ON [dbo].[GE_Modulo] ([Sistema]);
GO
ALTER TABLE [dbo].[GE_Modulo] ADD CONSTRAINT [FK__GE_Modulo__Siste__7F7792A0] FOREIGN KEY ([Sistema]) REFERENCES [dbo].[GE_Sistema] ([Sistema]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ModuloPerm
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ModuloPerm] (
    [Sistema] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Modulo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Permissao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Modul__A5B7226E7DE5B431] PRIMARY KEY CLUSTERED ([Sistema], [Modulo], [NroEmpresa], [SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ModuloPermPK] ON [dbo].[GE_ModuloPerm] ([Sistema], [Modulo], [NroEmpresa], [SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [GE_ModuloPermIF1] ON [dbo].[GE_ModuloPerm] ([Sistema], [Modulo]);
GO
CREATE NONCLUSTERED INDEX [GE_ModuloPermIF2] ON [dbo].[GE_ModuloPerm] ([NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [GE_ModuloPermIF3] ON [dbo].[GE_ModuloPerm] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[GE_ModuloPerm] ADD CONSTRAINT [FK__GE_Modulo__SeqUs__0253FF4B] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[GE_ModuloPerm] ADD CONSTRAINT [FK__GE_ModuloPerm__3A2E441A] FOREIGN KEY ([Sistema], [Modulo]) REFERENCES [dbo].[GE_Modulo] ([Sistema], [Modulo]);
GO
ALTER TABLE [dbo].[GE_ModuloPerm] ADD CONSTRAINT [FK__GE_Modulo__NroEm__3B226853] FOREIGN KEY ([NroEmpresa]) REFERENCES [dbo].[GE_Empresa] ([NroEmpresa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ObjDinamico
   Criada em ..: 2011-12-19
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ObjDinamico] (
    [SeqObjDyn] numeric(18,0) NOT NULL,
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoRetorno] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MensInicial] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MensFalso] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MensVerdadeiro] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uso] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Conexao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BancoDados] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Senha] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DBVersion] numeric(18,0) NULL,
    [Comando] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsoJuncaoPessoa] numeric(1,0) NULL,
    [UsoPessoa] numeric(1,0) NULL,
    [UsoProcesso] numeric(1,0) NULL,
    [UsoAgenda] numeric(1,0) NULL,
    [UsoHistorico] numeric(1,0) NULL,
    [UsoAcao] numeric(1,0) NULL,
    [UsoResultado] numeric(1,0) NULL,
    [UsoReqResultado] numeric(1,0) NULL,
    [UsoResultadoCmpl] numeric(1,0) NULL,
    [UsoWorkFlow] numeric(1,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USOMSGCENTER] numeric(1,0) NULL,
    [ChkSum] numeric(18,0) NULL,
    [USOMSGCTRSMS] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_ObjDi__FC9B53DE94EEC70D] PRIMARY KEY CLUSTERED ([SeqObjDyn])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ObjDinamicoPK] ON [dbo].[GE_ObjDinamico] ([SeqObjDyn]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ObjDinAplic
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ObjDinAplic] (
    [SeqObjDyn] numeric(18,0) NOT NULL,
    [Aplicativo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Chave] numeric(18,0) NOT NULL,
    [Vf] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_ObjDi__6EC55BB7F001B240] PRIMARY KEY CLUSTERED ([SeqObjDyn], [Aplicativo], [Chave])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ObjDinAplicPK] ON [dbo].[GE_ObjDinAplic] ([SeqObjDyn], [Aplicativo], [Chave]);
GO
CREATE NONCLUSTERED INDEX [GE_ObjDinAplicIF1] ON [dbo].[GE_ObjDinAplic] ([SeqObjDyn]);
GO
CREATE NONCLUSTERED INDEX [GE_ObjDinAplicIE1] ON [dbo].[GE_ObjDinAplic] ([Aplicativo], [Chave]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ParametroGlobal
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ParametroGlobal] (
    [Sistema] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Modulo] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] decimal(3,0) NOT NULL,
    [Parametro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Valor] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Criptografado] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtaalteracao] datetime NULL,
    [Usualteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Param__4F9DB9BBF21D75CC] PRIMARY KEY CLUSTERED ([Sistema], [Modulo], [NroEmpresa], [Parametro])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ParametroGloPK] ON [dbo].[GE_ParametroGlobal] ([Sistema], [Modulo], [NroEmpresa], [Parametro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_ParamLista
   Criada em ..: 2011-12-19
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_ParamLista] (
    [SeqParamLista] numeric(18,0) NOT NULL,
    [Parametro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [ListaStr] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ListaNro] numeric(18,0) NULL,
    [Str1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nro1] numeric(18,0) NULL,
    [Nro2] numeric(18,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STR2] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STR3] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STR4] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO3] numeric(18,0) NULL,
    [NRO4] numeric(18,0) NULL,
    [Ind1] numeric(1,0) NULL,
    [Ind2] numeric(1,0) NULL,
    [DTA1] datetime NULL,
    [DTA2] datetime NULL,
    [STR5] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IND3] numeric(1,0) NULL,
    [NRO5] numeric(18,0) NULL,
    [NRO6] numeric(18,0) NULL,
    [IND4] numeric(1,0) NULL,
    [STRL1] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Param__D7946655F521FA96] PRIMARY KEY CLUSTERED ([SeqParamLista])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_ParamListaPK] ON [dbo].[GE_ParamLista] ([SeqParamLista]);
GO
CREATE NONCLUSTERED INDEX [GE_ParamListaIE1] ON [dbo].[GE_ParamLista] ([Parametro], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [GE_ParamListaIE2] ON [dbo].[GE_ParamLista] ([Parametro], [ListaStr]);
GO
CREATE NONCLUSTERED INDEX [GE_ParamListaIE3] ON [dbo].[GE_ParamLista] ([Parametro], [ListaNro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Permissao
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Permissao] (
    [SeqAplicacao] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Executar] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Incluir] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Alterar] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Excluir] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RegistrarLog] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Verimpressao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Exportar] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Imprimir] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Permi__9FBEA4C320CBFE47] PRIMARY KEY CLUSTERED ([SeqAplicacao], [SeqUsuario], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PermissaoPK] ON [dbo].[GE_Permissao] ([SeqAplicacao], [SeqUsuario], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [GE_PermissaoIF1] ON [dbo].[GE_Permissao] ([NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [GE_PermissaoIF2] ON [dbo].[GE_Permissao] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [GE_PermissaoIF3] ON [dbo].[GE_Permissao] ([SeqAplicacao]);
GO
ALTER TABLE [dbo].[GE_Permissao] ADD CONSTRAINT [FK__GE_Permis__SeqUs__05306BF6] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[GE_Permissao] ADD CONSTRAINT [FK__GE_Permis__NroEm__3DFED4FE] FOREIGN KEY ([NroEmpresa]) REFERENCES [dbo].[GE_Empresa] ([NroEmpresa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PesDelVinc
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PesDelVinc] (
    [SeqPesDelVinc] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [KN1] numeric(18,0) NULL,
    [KN2] numeric(18,0) NULL,
    [KS1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KS2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PesDe__0068D8ADF30C0674] PRIMARY KEY CLUSTERED ([SeqPesDelVinc])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_PesDelVinc] ON [dbo].[GE_PesDelVinc] ([SeqPesDelVinc]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_PesDelVinc] ON [dbo].[GE_PesDelVinc] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_PesDelVinc] ADD CONSTRAINT [FK__GE_PesDel__SeqPe__232AE331] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_PessoaDel] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Pessoa
   Criada em ..: 2011-12-19
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Pessoa] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqCidade] decimal(6,0) NULL,
    [SeqBairro] decimal(5,0) NULL,
    [Versao] decimal(2,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAtivacao] datetime NULL,
    [NomeRazao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fantasia] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PalavraChave] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [SeqPessoaEndCobr] decimal(3,0) NULL,
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
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefonema] numeric(1,0) NULL,
    [Correspondencia] numeric(1,0) NULL,
    [RecebeEmail] numeric(1,0) NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [ProblemaCredito] numeric(1,0) NULL,
    [IndContribICMS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [SeqRegiao] decimal(6,0) NULL,
    [SeqRota] decimal(6,0) NULL,
    [RecebeSMS] numeric(1,0) NULL,
    [SEQPESSOAPRC] numeric(10,0) NOT NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGO] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGODTA] datetime NULL,
    CONSTRAINT [PK__GE_Pesso__A374A08CC9348B76] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE1] ON [dbo].[GE_Pessoa] ([NomeRazao]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE2] ON [dbo].[GE_Pessoa] ([Fantasia]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE3] ON [dbo].[GE_Pessoa] ([PalavraChave]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE4] ON [dbo].[GE_Pessoa] ([FoneNro1]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE5] ON [dbo].[GE_Pessoa] ([FoneNro2]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE6] ON [dbo].[GE_Pessoa] ([FoneNro3]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE7] ON [dbo].[GE_Pessoa] ([NroCGCCPF]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIF1] ON [dbo].[GE_Pessoa] ([SeqCidade], [SeqBairro]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIF2] ON [dbo].[GE_Pessoa] ([SeqCidade]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIF3] ON [dbo].[GE_Pessoa] ([SeqRegiao]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIF4] ON [dbo].[GE_Pessoa] ([SeqRota]);
GO
CREATE NONCLUSTERED INDEX [idxGE_PESSOAPRC] ON [dbo].[GE_Pessoa] ([SEQPESSOAPRC]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaPK] ON [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaIE8] ON [dbo].[GE_Pessoa] ([FaxNro]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__SeqRo__43B7AE54] FOREIGN KEY ([SeqRota]) REFERENCES [dbo].[GE_Rota] ([SeqRota]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__SeqRo__09F52113] FOREIGN KEY ([SeqRota]) REFERENCES [dbo].[GE_Rota] ([SeqRota]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__SeqCi__6E22E96C] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GE_Cidade] ([SeqCidade]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__SeqRe__1E662E14] FOREIGN KEY ([SeqRegiao]) REFERENCES [dbo].[GE_Regiao] ([SeqRegiao]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__SeqRe__0900FCDA] FOREIGN KEY ([SeqRegiao]) REFERENCES [dbo].[GE_Regiao] ([SeqRegiao]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__40DB41A9] FOREIGN KEY ([SeqCidade], [SeqBairro]) REFERENCES [dbo].[GE_Bairro] ([SeqCidade], [SeqBairro]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__SeqRe__42C38A1B] FOREIGN KEY ([SeqRegiao]) REFERENCES [dbo].[GE_Regiao] ([SeqRegiao]);
GO
ALTER TABLE [dbo].[GE_Pessoa] ADD CONSTRAINT [FK__GE_Pessoa__SeqRo__1C7DE5A2] FOREIGN KEY ([SeqRota]) REFERENCES [dbo].[GE_Rota] ([SeqRota]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Pessoa_BKP28052025
   Criada em ..: 2025-05-28
   Alterada em : 2025-05-28
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Pessoa_BKP28052025] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqCidade] decimal(6,0) NULL,
    [SeqBairro] decimal(5,0) NULL,
    [Versao] decimal(2,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAtivacao] datetime NULL,
    [NomeRazao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fantasia] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PalavraChave] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [SeqPessoaEndCobr] decimal(3,0) NULL,
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
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefonema] numeric(1,0) NULL,
    [Correspondencia] numeric(1,0) NULL,
    [RecebeEmail] numeric(1,0) NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [ProblemaCredito] numeric(1,0) NULL,
    [IndContribICMS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [SeqRegiao] decimal(6,0) NULL,
    [SeqRota] decimal(6,0) NULL,
    [RecebeSMS] numeric(1,0) NULL,
    [SEQPESSOAPRC] numeric(10,0) NOT NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGO] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGODTA] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Pessoa_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Pessoa_BKPJUN] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqCidade] decimal(6,0) NULL,
    [SeqBairro] decimal(5,0) NULL,
    [Versao] decimal(2,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAtivacao] datetime NULL,
    [NomeRazao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fantasia] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PalavraChave] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [SeqPessoaEndCobr] decimal(3,0) NULL,
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
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [CodEquipe] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefonema] numeric(1,0) NULL,
    [Correspondencia] numeric(1,0) NULL,
    [RecebeEmail] numeric(1,0) NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [ProblemaCredito] numeric(1,0) NULL,
    [IndContribICMS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [SeqRegiao] decimal(6,0) NULL,
    [SeqRota] decimal(6,0) NULL,
    [RecebeSMS] numeric(1,0) NULL,
    [SEQPESSOAPRC] numeric(10,0) NOT NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGO] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGODTA] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Pessoa_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Pessoa_ITA] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqCidade] decimal(6,0) NULL,
    [SeqBairro] decimal(5,0) NULL,
    [Versao] decimal(2,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAtivacao] datetime NULL,
    [NomeRazao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fantasia] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PalavraChave] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [SeqPessoaEndCobr] decimal(3,0) NULL,
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
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [CodEquipe] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefonema] numeric(1,0) NULL,
    [Correspondencia] numeric(1,0) NULL,
    [RecebeEmail] numeric(1,0) NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [ProblemaCredito] numeric(1,0) NULL,
    [IndContribICMS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [SeqRegiao] decimal(6,0) NULL,
    [SeqRota] decimal(6,0) NULL,
    [RecebeSMS] numeric(1,0) NULL,
    [SEQPESSOAPRC] numeric(10,0) NOT NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGO] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSCODIGODTA] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaAlerta
   Criada em ..: 2011-12-19
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaAlerta] (
    [SeqPessoaAlerta] numeric(10,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [Resumo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Advertencia] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaVigor] datetime NULL,
    [EmUso] numeric(1,0) NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_Pesso__6E9A1EA0FBB5C757] PRIMARY KEY CLUSTERED ([SeqPessoaAlerta])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaAlertaPK] ON [dbo].[GE_PessoaAlerta] ([SeqPessoaAlerta]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaAlertaIF1] ON [dbo].[GE_PessoaAlerta] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaAlt
   Criada em ..: 2015-09-14
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaAlt] (
    [SeqPessoaAlt] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqCidade] numeric(6,0) NULL,
    [SeqBairro] numeric(5,0) NULL,
    [Versao] numeric(2,0) NULL,
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
    [Latitude] numeric(14,11) NULL,
    [Longitude] numeric(14,11) NULL,
    [SeqPessoaEndCobr] numeric(3,0) NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] numeric(12,0) NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] numeric(12,0) NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD3] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro3] numeric(12,0) NULL,
    [FoneCmpl3] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] numeric(12,0) NULL,
    [NroCGCCPF] numeric(13,0) NULL,
    [DigCGCCPF] numeric(2,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Email] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HomePage] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EstadoCivil] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atividade] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RendaFaturamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GrauInstrucao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Porte] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInativacao] datetime NULL,
    [UsuInativacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsInativacao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefonema] numeric(1,0) NULL,
    [Correspondencia] numeric(1,0) NULL,
    [RecebeEmail] numeric(1,0) NULL,
    [RecebeSMS] numeric(1,0) NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [ProblemaCredito] numeric(1,0) NULL,
    [IndContribICMS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PessoaAlt__2F3BB45B] PRIMARY KEY CLUSTERED ([SeqPessoaAlt])
);
GO
CREATE NONCLUSTERED INDEX [XIE1GE_PessoaAlt] ON [dbo].[GE_PessoaAlt] ([DtaAlteracao]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_PessoaAlt] ON [dbo].[GE_PessoaAlt] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF2GE_PessoaAlt] ON [dbo].[GE_PessoaAlt] ([SeqCidade]);
GO
CREATE NONCLUSTERED INDEX [XIF3GE_PessoaAlt] ON [dbo].[GE_PessoaAlt] ([SeqCidade], [SeqBairro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.ge_pessoaativa
   Criada em ..: 2017-01-02
   Alterada em : 2017-01-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[ge_pessoaativa] (
    [seqpessoa] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PESSOAATIVAUSR
   Criada em ..: 2020-07-14
   Alterada em : 2020-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PESSOAATIVAUSR] (
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [CODUSUARIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPESSOA] numeric(10,0) NULL,
    [PESSOALINK] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAACESSO] datetime NULL,
    CONSTRAINT [PK__GE_PESSO__59D861DEA046F17D] PRIMARY KEY CLUSTERED ([SEQUSUARIO])
);
GO
ALTER TABLE [dbo].[GE_PESSOAATIVAUSR] ADD CONSTRAINT [FK__GE_PESSOA__SEQUS__1B4073FD] FOREIGN KEY ([SEQUSUARIO]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaClasse
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaClasse] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Classe] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dtaalteracao] datetime NULL,
    [Usualteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDENTORIGEM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Pesso__2DFF04D2E11C002F] PRIMARY KEY CLUSTERED ([SeqPessoa], [Classe])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaClassePK] ON [dbo].[GE_PessoaClasse] ([SeqPessoa], [Classe]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaClasseIE1] ON [dbo].[GE_PessoaClasse] ([Classe]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaClasseIF1] ON [dbo].[GE_PessoaClasse] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaDel
   Criada em ..: 2016-07-12
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaDel] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPessoaPRC] numeric(10,0) NULL,
    [SeqCidade] numeric(6,0) NULL,
    [SeqBairro] numeric(10,0) NULL,
    [Versao] numeric(2,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAtivacao] datetime NULL,
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
    [Latitude] numeric(14,11) NULL,
    [Longitude] numeric(14,11) NULL,
    [SeqPessoaEndCobr] numeric(3,0) NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] numeric(12,0) NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] numeric(12,0) NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD3] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro3] numeric(12,0) NULL,
    [FoneCmpl3] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxDDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaxNro] numeric(12,0) NULL,
    [NroCGCCPF] numeric(13,0) NULL,
    [DigCGCCPF] numeric(2,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UFEmissor] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrgaoEmissor] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascFund] datetime NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Email] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Skype] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Telefonema] numeric(1,0) NULL,
    [Correspondencia] numeric(1,0) NULL,
    [RecebeEmail] numeric(1,0) NULL,
    [RecebeSMS] numeric(1,0) NULL,
    [NaoPossuiEmail] numeric(1,0) NULL,
    [ProblemaCredito] numeric(1,0) NULL,
    [IndContribICMS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqRegiao] numeric(6,0) NULL,
    [SeqRota] numeric(6,0) NULL,
    [DtaDel] datetime NOT NULL,
    [UsrDel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_Pesso__A374A08C051EA217] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE1] ON [dbo].[GE_PessoaDel] ([NomeRazao]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE2] ON [dbo].[GE_PessoaDel] ([Fantasia]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE3] ON [dbo].[GE_PessoaDel] ([PalavraChave]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE4] ON [dbo].[GE_PessoaDel] ([FoneNro1]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE5] ON [dbo].[GE_PessoaDel] ([FoneNro2]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE6] ON [dbo].[GE_PessoaDel] ([FoneNro3]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE7] ON [dbo].[GE_PessoaDel] ([NroCGCCPF]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOAIE8] ON [dbo].[GE_PessoaDel] ([FaxNro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaDestino
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaDestino] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Destino] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [IndEnvia] numeric(1,0) NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_Pesso__B1D1DAE51B7F93C3] PRIMARY KEY CLUSTERED ([SeqPessoa], [Destino])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaDestinPK] ON [dbo].[GE_PessoaDestino] ([SeqPessoa], [Destino]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaDestinIF1] ON [dbo].[GE_PessoaDestino] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaDestinIF2] ON [dbo].[GE_PessoaDestino] ([Destino]);
GO
ALTER TABLE [dbo].[GE_PessoaDestino] ADD CONSTRAINT [FK__GE_Pessoa__Desti__0DC5B1F7] FOREIGN KEY ([Destino]) REFERENCES [dbo].[GEP_ParEnvia] ([Destino]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaEmail
   Criada em ..: 2018-08-15
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaEmail] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [EMail] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Preferencial] numeric(1,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_GE_PessoaEmail] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqContato])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaEmailPK] ON [dbo].[GE_PessoaEmail] ([SeqPessoa], [SeqContato]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaEmailIE1] ON [dbo].[GE_PessoaEmail] ([EMail]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaEmailIF1] ON [dbo].[GE_PessoaEmail] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_PessoaEmail] ADD CONSTRAINT [FK__GE_Pessoa__SeqPe__0EB9D630] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaEnd
   Criada em ..: 2011-12-19
   Alterada em : 2019-09-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaEnd] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPessoaEnd] decimal(3,0) NOT NULL,
    [SeqCidade] decimal(6,0) NULL,
    [TipoEndereco] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqBairro] decimal(5,0) NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroLogradouro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CxPostal] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoaEndCobr] decimal(3,0) NULL,
    [Cep] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqRegiao] decimal(6,0) NULL,
    [SeqRota] decimal(6,0) NULL,
    [CHAVEADICIONAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSCPRODUTOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Pesso__E57719780F636BA5] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqPessoaEnd])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaEndPK] ON [dbo].[GE_PessoaEnd] ([SeqPessoa], [SeqPessoaEnd]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaEndIF1] ON [dbo].[GE_PessoaEnd] ([SeqCidade], [SeqBairro]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaEndIF2] ON [dbo].[GE_PessoaEnd] ([SeqCidade]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaEndIF3] ON [dbo].[GE_PessoaEnd] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_PessoaEnd] ADD CONSTRAINT [FK__GE_PessoaEnd__497087AA] FOREIGN KEY ([SeqCidade], [SeqBairro]) REFERENCES [dbo].[GE_Bairro] ([SeqCidade], [SeqBairro]);
GO
ALTER TABLE [dbo].[GE_PessoaEnd] ADD CONSTRAINT [FK__GE_Pessoa__SeqCi__795F91EE] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GE_Cidade] ([SeqCidade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaEnd_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaEnd_BKPJUN] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPessoaEnd] decimal(3,0) NOT NULL,
    [SeqCidade] decimal(6,0) NULL,
    [TipoEndereco] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqBairro] decimal(5,0) NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroLogradouro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CxPostal] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoaEndCobr] decimal(3,0) NULL,
    [Cep] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqRegiao] decimal(6,0) NULL,
    [SeqRota] decimal(6,0) NULL,
    [CHAVEADICIONAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSCPRODUTOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaEnd_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaEnd_ITA] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPessoaEnd] decimal(3,0) NOT NULL,
    [SeqCidade] decimal(6,0) NULL,
    [TipoEndereco] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqBairro] decimal(5,0) NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroLogradouro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CxPostal] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoaEndCobr] decimal(3,0) NULL,
    [Cep] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] decimal(14,11) NULL,
    [Longitude] decimal(14,11) NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqRegiao] decimal(6,0) NULL,
    [SeqRota] decimal(6,0) NULL,
    [CHAVEADICIONAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSCPRODUTOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaEndAlt
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaEndAlt] (
    [SeqPessoaEndAlt] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [SeqPessoaEnd] numeric(3,0) NULL,
    [SeqCidade] numeric(6,0) NULL,
    [SeqBairro] numeric(5,0) NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Logradouro] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroLogradouro] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltoLogradouro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CxPostal] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RefEndereco] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Latitude] numeric(14,11) NULL,
    [Longitude] numeric(14,11) NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoaEndCobr] numeric(3,0) NULL,
    [CEP] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAIS] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_PessoaEndAlt__3123FCCD] PRIMARY KEY CLUSTERED ([SeqPessoaEndAlt])
);
GO
CREATE NONCLUSTERED INDEX [XIE1GE_PessoaEndAlt] ON [dbo].[GE_PessoaEndAlt] ([DtaAlteracao]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_PessoaEndAlt] ON [dbo].[GE_PessoaEndAlt] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF2GE_PessoaEndAlt] ON [dbo].[GE_PessoaEndAlt] ([SeqPessoa], [SeqPessoaEnd]);
GO
CREATE NONCLUSTERED INDEX [XIF3GE_PessoaEndAlt] ON [dbo].[GE_PessoaEndAlt] ([SeqCidade]);
GO
CREATE NONCLUSTERED INDEX [XIF4GE_PessoaEndAlt] ON [dbo].[GE_PessoaEndAlt] ([SeqCidade], [SeqBairro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaEndOutroBc
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaEndOutroBc] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPessoaEnd] decimal(3,0) NOT NULL,
    [SeqEndOutroBc] numeric(18,0) NULL,
    CONSTRAINT [PK__GE_Pesso__E57719785504518C] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqPessoaEnd])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaEndOutPK] ON [dbo].[GE_PessoaEndOutroBc] ([SeqPessoa], [SeqPessoaEnd]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaEndOutAK1] ON [dbo].[GE_PessoaEndOutroBc] ([SeqPessoa], [SeqEndOutroBc]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaEndOutIF1] ON [dbo].[GE_PessoaEndOutroBc] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_PessoaEndOutroBc] ADD CONSTRAINT [FK__GE_Pessoa__SeqPe__128A6714] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaFis
   Criada em ..: 2011-12-19
   Alterada em : 2020-11-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaFis] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [NomePai] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeMae] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NatCidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NatUF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NatSeqCidade] numeric(6,0) NULL,
    [Nacionalidade] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaEmissaoRG] datetime NULL,
    [NatOcupacaoSeqPar] numeric(18,0) NULL,
    [AtividadeSeqPar] numeric(18,0) NULL,
    [NacionalSeqPar] numeric(18,0) NULL,
    [EstadoCivilSeqPar] numeric(18,0) NULL,
    [GrauInstrSeqPar] numeric(18,0) NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaNascimento] datetime NULL,
    [RGDtaEmissao] datetime NULL,
    [QtdDependente] decimal(2,0) NULL,
    [CNHNro] numeric(18,0) NULL,
    [CNHDtaValidade] datetime NULL,
    [RGOrgaoEmissor] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGUFEmissao] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CartTrabNro] numeric(18,0) NULL,
    [CartTrabSerie] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CartTrabUF] char(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdentProfSeqPar] numeric(18,0) NULL,
    [IdentProfNro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNHDtaEmis] datetime NULL,
    [CartTrabDtaEmis] datetime NULL,
    [IdentProfDtaEmis] datetime NULL,
    [CRNM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CRNMDTAEMISSAO] datetime NULL,
    [CRNMDTAVALIDADE] datetime NULL,
    CONSTRAINT [PK__GE_Pesso__A374A08C147780FC] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaFisPK] ON [dbo].[GE_PessoaFis] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFisIF2] ON [dbo].[GE_PessoaFis] ([NatSeqCidade]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFisIF3] ON [dbo].[GE_PessoaFis] ([NatOcupacaoSeqPar]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFisIF4] ON [dbo].[GE_PessoaFis] ([AtividadeSeqPar]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFisIF5] ON [dbo].[GE_PessoaFis] ([NacionalSeqPar]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFisIF6] ON [dbo].[GE_PessoaFis] ([EstadoCivilSeqPar]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFisIF7] ON [dbo].[GE_PessoaFis] ([GrauInstrSeqPar]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFisIF8] ON [dbo].[GE_PessoaFis] ([IdentProfSeqPar]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaFone
   Criada em ..: 2015-09-14
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaFone] (
    [SeqPesFone] numeric(18,0) NOT NULL,
    [TipoFoneSeqPar] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero] numeric(12,0) NOT NULL,
    [Complemento] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndFonePref] numeric(1,0) NULL,
    [NroFonePessoa] numeric(1,0) NULL,
    [DTAULTSUCESSO] datetime NULL,
    [DTAULTINSUCESSO] datetime NULL,
    [INDEMUSO] numeric(1,0) NULL,
    [MOTIVOEMUSO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDUSOMKT] numeric(1,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDWHATSAPP] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_PessoaFone__77B67547] PRIMARY KEY CLUSTERED ([SeqPesFone])
);
GO
CREATE NONCLUSTERED INDEX [XIE1GE_PessoaFone] ON [dbo].[GE_PessoaFone] ([Numero]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_PessoaFone] ON [dbo].[GE_PessoaFone] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF2GE_PessoaFone] ON [dbo].[GE_PessoaFone] ([TipoFoneSeqPar]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaFone_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaFone_BKPJUN] (
    [SeqPesFone] numeric(18,0) NOT NULL,
    [TipoFoneSeqPar] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero] numeric(12,0) NOT NULL,
    [Complemento] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndFonePref] numeric(1,0) NULL,
    [NroFonePessoa] numeric(1,0) NULL,
    [DTAULTSUCESSO] datetime NULL,
    [DTAULTINSUCESSO] datetime NULL,
    [INDEMUSO] numeric(1,0) NULL,
    [MOTIVOEMUSO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDUSOMKT] numeric(1,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDWHATSAPP] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaFone_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaFone_ITA] (
    [SeqPesFone] numeric(18,0) NOT NULL,
    [TipoFoneSeqPar] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DDD] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero] numeric(12,0) NOT NULL,
    [Complemento] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndFonePref] numeric(1,0) NULL,
    [NroFonePessoa] numeric(1,0) NULL,
    [DTAULTSUCESSO] datetime NULL,
    [DTAULTINSUCESSO] datetime NULL,
    [INDEMUSO] numeric(1,0) NULL,
    [MOTIVOEMUSO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDUSOMKT] numeric(1,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDWHATSAPP] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaFonema
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaFonema] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [Particula] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_PessoaFonema__2F30C763] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqContato], [Particula])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaFonemaPK] ON [dbo].[GE_PessoaFonema] ([SeqPessoa], [SeqContato], [Particula]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFonemaIE1] ON [dbo].[GE_PessoaFonema] ([Particula]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaFonemaIF1] ON [dbo].[GE_PessoaFonema] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaJur
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaJur] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [QtdFuncionario] decimal(4,0) NULL,
    [QtdFilial] decimal(4,0) NULL,
    [VlrFatMedioMes] decimal(15,2) NULL,
    [VlrCapitalSoc] decimal(15,2) NULL,
    [Clg1CNPJ] numeric(18,0) NULL,
    [Clg1RazaoSoc] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Clg1PercPart] decimal(6,2) NULL,
    [Clg1Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Clg1SeqPessoa] numeric(10,0) NULL,
    [Clg2CNPJ] numeric(18,0) NULL,
    [Clg2RazaoSoc] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Clg2PercPart] decimal(6,2) NULL,
    [Clg2Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Clg2SeqPessoa] numeric(10,0) NULL,
    CONSTRAINT [PK__GE_PessoaJur__3024EB9C] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaJurPK] ON [dbo].[GE_PessoaJur] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_PessoaJur] ADD CONSTRAINT [FK__GE_Pessoa__SeqPe__1566D3BF] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaLink
   Criada em ..: 2011-12-19
   Alterada em : 2020-11-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaLink] (
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [SEQPESSOALINK] numeric(18,0) IDENTITY(1,1) NOT NULL
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaLinkPK] ON [dbo].[GE_PessoaLink] ([Pessoalink], [Origem]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaLinkIF1] ON [dbo].[GE_PessoaLink] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_PESSOALINK_01] ON [dbo].[GE_PessoaLink] ([Origem]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_GE_PESSOALINK_U01] ON [dbo].[GE_PessoaLink] ([SEQPESSOALINK]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaLink_bkpago22
   Criada em ..: 2022-09-13
   Alterada em : 2022-09-13
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaLink_bkpago22] (
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [SEQPESSOALINK] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaLink_BKPJUN
   Criada em ..: 2024-12-29
   Alterada em : 2024-12-29
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaLink_BKPJUN] (
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [SEQPESSOALINK] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaLink_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaLink_ITA] (
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [SEQPESSOALINK] numeric(18,0) IDENTITY(1,1) NOT NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaLinkbkp
   Criada em ..: 2021-08-02
   Alterada em : 2021-08-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaLinkbkp] (
    [Pessoalink] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [SEQPESSOALINK] numeric(18,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaMural
   Criada em ..: 2012-05-02
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaMural] (
    [SeqPessoaMural] numeric(10,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Resumo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Nivel] decimal(1,0) NULL,
    [Detalhe] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndRemovivel] numeric(1,0) NULL,
    [DtaExpira] datetime NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [LinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PessoaMural__320D340E] PRIMARY KEY CLUSTERED ([SeqPessoaMural])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaMuralPK] ON [dbo].[GE_PessoaMural] ([SeqPessoaMural]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaMuralIE1] ON [dbo].[GE_PessoaMural] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaMuralIE2] ON [dbo].[GE_PessoaMural] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaMuralIE3] ON [dbo].[GE_PessoaMural] ([LinkOrigem], [LinkNro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaNomeFonema
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaNomeFonema] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqContato] decimal(4,0) NOT NULL,
    [NomeRazao] varchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomePuro] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PessoaNomeFon__33015847] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqContato])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaNomeFoPK] ON [dbo].[GE_PessoaNomeFonema] ([SeqPessoa], [SeqContato]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaNomeFoIE1] ON [dbo].[GE_PessoaNomeFonema] ([NomeRazao]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaNomeFoIF1] ON [dbo].[GE_PessoaNomeFonema] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [GE_PESSOANOMEFOIE2] ON [dbo].[GE_PessoaNomeFonema] ([NomePuro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaNota
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaNota] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Anotacao] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PessoaNota__33F57C80] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaNotaPK] ON [dbo].[GE_PessoaNota] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_PessoaNota] ADD CONSTRAINT [FK__GE_Pessoa__SeqPe__1843406A] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaPasw
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaPasw] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [eMail] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Senha] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PerguntaChave] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Resposta] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TrocarSenha] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OrigemCadastro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PessoaPasw__34E9A0B9] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaPaswPK] ON [dbo].[GE_PessoaPasw] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[GE_PessoaPasw] ADD CONSTRAINT [FK__GE_Pessoa__SeqPe__193764A3] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PESSOAREDESOCIAL
   Criada em ..: 2019-12-19
   Alterada em : 2019-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PESSOAREDESOCIAL] (
    [SEQPESREDESOCIAL] numeric(12,0) NOT NULL,
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [REDESOCIAL] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDENTIFICADOR] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [URL] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDEMUSO] numeric(1,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PESSO__C50C958CB5C52732] PRIMARY KEY CLUSTERED ([SEQPESREDESOCIAL])
);
GO
ALTER TABLE [dbo].[GE_PESSOAREDESOCIAL] ADD CONSTRAINT [FK__GE_PESSOA__SEQPE__6BC66B05] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaRelacao
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaRelacao] (
    [TipoRelacionamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPrincipal] numeric(10,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Obs] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Link] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__GE_PessoaRelacao__35DDC4F2] PRIMARY KEY CLUSTERED ([TipoRelacionamento], [SeqPrincipal], [SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaRelacaPK] ON [dbo].[GE_PessoaRelacao] ([TipoRelacionamento], [SeqPrincipal], [SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaRelacaIE1] ON [dbo].[GE_PessoaRelacao] ([Link], [LinkOrigem]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaRelacaIE2] ON [dbo].[GE_PessoaRelacao] ([SeqPrincipal]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaRelacaIE3] ON [dbo].[GE_PessoaRelacao] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaRelacao_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaRelacao_BKPJUN] (
    [TipoRelacionamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPrincipal] numeric(10,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Obs] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Link] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaRelacao_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaRelacao_ITA] (
    [TipoRelacionamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPrincipal] numeric(10,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Obs] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Link] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaSimilar
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaSimilar] (
    [SeqPessoa1] numeric(10,0) NOT NULL,
    [SeqPessoa2] numeric(10,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [Probabilidade] decimal(3,0) NULL,
    [ObsGeracao] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Revisado] numeric(1,0) NULL,
    [DtaRevisao] datetime NULL,
    [Duplicado] numeric(1,0) NULL,
    [Parecer] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuParecer] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuEmTrabalho] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaEmTrabalho] datetime NULL,
    [SeqPessoaFica] numeric(10,0) NULL,
    [ObsFica] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PessoaSimilar__36D1E92B] PRIMARY KEY CLUSTERED ([SeqPessoa1], [SeqPessoa2])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaSimilaPK] ON [dbo].[GE_PessoaSimilar] ([SeqPessoa1], [SeqPessoa2]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaSimilaIE2] ON [dbo].[GE_PessoaSimilar] ([Revisado]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaSimilaIE3] ON [dbo].[GE_PessoaSimilar] ([Probabilidade]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaSimilaIF1] ON [dbo].[GE_PessoaSimilar] ([SeqPessoa1]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaSimilaIF2] ON [dbo].[GE_PessoaSimilar] ([SeqPessoa2]);
GO
ALTER TABLE [dbo].[GE_PessoaSimilar] ADD CONSTRAINT [FK__GE_Pessoa__SeqPe__1C13D14E] FOREIGN KEY ([SeqPessoa1]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaUnidade
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaUnidade] (
    [SeqPessoa] numeric(8,0) NOT NULL,
    [SeqUnidade] numeric(4,0) NOT NULL,
    [Nivel] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_PessoaUnidade__37C60D64] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaUnidadPK] ON [dbo].[GE_PessoaUnidade] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PessoaVersao
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PessoaVersao] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Versao] decimal(2,0) NOT NULL,
    [NomeRazao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Uf] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Bairro] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EnderecoCompleto] varchar(120) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cep] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Pais] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCGCCPF] decimal(13,0) NULL,
    [DigCGCCPF] decimal(2,0) NULL,
    [InscricaoRG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscMunic] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InscProdutor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNAE] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAtualizacao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAlteracao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Justificativa] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PessoaVersao__38BA319D] PRIMARY KEY CLUSTERED ([SeqPessoa], [Versao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PessoaVersaoPK] ON [dbo].[GE_PessoaVersao] ([SeqPessoa], [Versao]);
GO
CREATE NONCLUSTERED INDEX [GE_PessoaVersaoIF1] ON [dbo].[GE_PessoaVersao] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PolSeg
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PolSeg] (
    [SeqPolSeg] decimal(6,0) NOT NULL,
    [Politica] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PolSeg__3AA27A0F] PRIMARY KEY CLUSTERED ([SeqPolSeg])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PolSegPK] ON [dbo].[GE_PolSeg] ([SeqPolSeg]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PolSegAces
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PolSegAces] (
    [SeqPolSeg] decimal(6,0) NOT NULL,
    [Modulo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DiaSem] numeric(1,0) NOT NULL,
    [HINI1] decimal(4,0) NULL,
    [HFIM1] decimal(4,0) NULL,
    [HINI2] decimal(4,0) NULL,
    [HFIM2] decimal(4,0) NULL,
    CONSTRAINT [PK__GE_PolSegAces__3B969E48] PRIMARY KEY CLUSTERED ([SeqPolSeg], [Modulo], [DiaSem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PolSegAcesPK] ON [dbo].[GE_PolSegAces] ([SeqPolSeg], [Modulo], [DiaSem]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegAcesIF2] ON [dbo].[GE_PolSegAces] ([SeqPolSeg]);
GO
ALTER TABLE [dbo].[GE_PolSegAces] ADD CONSTRAINT [FK__GE_PolSeg__SeqPo__1FE46232] FOREIGN KEY ([SeqPolSeg]) REFERENCES [dbo].[GE_PolSeg] ([SeqPolSeg]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PolSegCtrl
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PolSegCtrl] (
    [Seq] numeric(18,0) NOT NULL,
    [Modulo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Item] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPolSeg] decimal(6,0) NOT NULL,
    [Str] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PolSegCtrl__3C8AC281] PRIMARY KEY CLUSTERED ([Seq])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PolSegCtrlPK] ON [dbo].[GE_PolSegCtrl] ([Seq]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegCtrlIE1] ON [dbo].[GE_PolSegCtrl] ([Modulo], [Item], [SeqPolSeg]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegCtrlIF1] ON [dbo].[GE_PolSegCtrl] ([Item], [Modulo]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegCtrlIF2] ON [dbo].[GE_PolSegCtrl] ([SeqPolSeg]);
GO
ALTER TABLE [dbo].[GE_PolSegCtrl] ADD CONSTRAINT [FK__GE_PolSeg__SeqPo__21CCAAA4] FOREIGN KEY ([SeqPolSeg]) REFERENCES [dbo].[GE_PolSeg] ([SeqPolSeg]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PolSegItem
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PolSegItem] (
    [Modulo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Item] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescRed] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SubTipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ordem] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_PolSegItem__3D7EE6BA] PRIMARY KEY CLUSTERED ([Modulo], [Item])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PolSegItemPK] ON [dbo].[GE_PolSegItem] ([Modulo], [Item]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegItemIF1] ON [dbo].[GE_PolSegItem] ([Modulo]);
GO
ALTER TABLE [dbo].[GE_PolSegItem] ADD CONSTRAINT [FK__GE_PolSeg__Modul__5C835C1E] FOREIGN KEY ([Modulo]) REFERENCES [dbo].[GE_AppModulo] ([Modulo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PolSegItLst
   Criada em ..: 2013-03-27
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PolSegItLst] (
    [Modulo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Item] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Lista] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Nro] numeric(18,0) NULL,
    [Str] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CTRLATUALIZACAO] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_PolSegItLst__3E730AF3] PRIMARY KEY CLUSTERED ([Modulo], [Item], [Lista])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PolSegItLstPK] ON [dbo].[GE_PolSegItLst] ([Modulo], [Item], [Lista]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegItLstIF1] ON [dbo].[GE_PolSegItLst] ([Item], [Modulo]);
GO
ALTER TABLE [dbo].[GE_PolSegItLst] ADD CONSTRAINT [FK__GE_PolSegItLst__23B4F316] FOREIGN KEY ([Modulo], [Item]) REFERENCES [dbo].[GE_PolSegItem] ([Modulo], [Item]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PolSegParams
   Criada em ..: 2012-03-28
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PolSegParams] (
    [SeqPolSeg] decimal(6,0) NOT NULL,
    [Param1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Param2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Param3] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Str] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__GE_PolSegParams__3F672F2C] PRIMARY KEY CLUSTERED ([SeqPolSeg], [Param1], [Param2], [Param3])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PolSegParamsPK] ON [dbo].[GE_PolSegParams] ([SeqPolSeg], [Param1], [Param2], [Param3]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegParamsIF1] ON [dbo].[GE_PolSegParams] ([SeqPolSeg]);
GO
ALTER TABLE [dbo].[GE_PolSegParams] ADD CONSTRAINT [FK__GE_PolSeg__SeqPo__24A9174F] FOREIGN KEY ([SeqPolSeg]) REFERENCES [dbo].[GE_PolSeg] ([SeqPolSeg]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_POLSEGPERM
   Criada em ..: 2012-01-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_POLSEGPERM] (
    [CODAPLICACAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CHAVEAPLICACAO] numeric(18,0) NOT NULL,
    [NROEMPRESA] numeric(6,0) NOT NULL,
    [SEQPOLSEG] decimal(6,0) NOT NULL,
    [PERMISSAO] decimal(4,0) NULL,
    CONSTRAINT [PK__GE_POLSEGPERM__405B5365] PRIMARY KEY CLUSTERED ([CODAPLICACAO], [CHAVEAPLICACAO], [NROEMPRESA], [SEQPOLSEG])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_PolSegPermPK] ON [dbo].[GE_POLSEGPERM] ([CODAPLICACAO], [CHAVEAPLICACAO], [NROEMPRESA], [SEQPOLSEG]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegPermIE1] ON [dbo].[GE_POLSEGPERM] ([CODAPLICACAO], [CHAVEAPLICACAO], [NROEMPRESA]);
GO
CREATE NONCLUSTERED INDEX [GE_PolSegPermIF2] ON [dbo].[GE_POLSEGPERM] ([SEQPOLSEG]);
GO
ALTER TABLE [dbo].[GE_POLSEGPERM] ADD CONSTRAINT [FK__GE_POLSEG__SEQPO__259D3B88] FOREIGN KEY ([SEQPOLSEG]) REFERENCES [dbo].[GE_PolSeg] ([SeqPolSeg]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_PROCESSOWEB
   Criada em ..: 2022-05-16
   Alterada em : 2022-05-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_PROCESSOWEB] (
    [PROCESSO] numeric(18,0) NOT NULL,
    [DEPTOPESSOAPROC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQUSUARIOAPPRESP] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [DTASOLICUSUEXT] datetime NULL,
    [INDAGUARDUSUEXT] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_PROCE__C8342852E37CDFAC] PRIMARY KEY CLUSTERED ([PROCESSO])
);
GO
ALTER TABLE [dbo].[GE_PROCESSOWEB] ADD CONSTRAINT [FK__GE_PROCES__PROCE__5B7AF4A3] FOREIGN KEY ([PROCESSO]) REFERENCES [dbo].[IV_ProcDado] ([Processo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_QVCons
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_QVCons] (
    [SeqCons] numeric(18,0) NOT NULL,
    [Cod] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nome] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Adv] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InstrSql] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Qrp] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Prvw] numeric(1,0) NULL,
    [FtTipo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FtTam] decimal(2,0) NULL,
    [Tit] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sessao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GTipo] decimal(8,0) NULL,
    [GTit] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GRdp] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GEsq] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GLeg] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GCol] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dono] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndSqlChg] numeric(1,0) NULL,
    [Style] numeric(1,0) NULL,
    [ChkS1] numeric(18,0) NULL,
    [ChkS2] numeric(18,0) NULL,
    [UltAtualizacao] datetime NULL,
    [DtaInicioUso] datetime NULL,
    [DtaUltimoUso] datetime NULL,
    [UltUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdeUso] decimal(8,0) NULL,
    CONSTRAINT [PK__GE_QVCons__414F779E] PRIMARY KEY CLUSTERED ([SeqCons])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVConsPK] ON [dbo].[GE_QVCons] ([SeqCons]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVConsAK1] ON [dbo].[GE_QVCons] ([Cod]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_QVConsCol
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_QVConsCol] (
    [SeqCons] numeric(18,0) NOT NULL,
    [Pos] decimal(2,0) NOT NULL,
    [ColName] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Coluna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TpDado] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cor] numeric(18,0) NULL,
    [Formato] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vars] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_QVConsCol__42439BD7] PRIMARY KEY CLUSTERED ([SeqCons], [Pos])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVConsColPK] ON [dbo].[GE_QVConsCol] ([SeqCons], [Pos]);
GO
CREATE NONCLUSTERED INDEX [GE_QVConsColIF1] ON [dbo].[GE_QVConsCol] ([SeqCons]);
GO
ALTER TABLE [dbo].[GE_QVConsCol] ADD CONSTRAINT [FK__GE_QVCons__SeqCo__26915FC1] FOREIGN KEY ([SeqCons]) REFERENCES [dbo].[GE_QVCons] ([SeqCons]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_QVConsHst
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_QVConsHst] (
    [SeqCons] numeric(18,0) NOT NULL,
    [SeqHst] decimal(4,0) NOT NULL,
    [Detalhe] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dta] datetime NULL,
    CONSTRAINT [PK__GE_QVConsHst__4337C010] PRIMARY KEY CLUSTERED ([SeqCons], [SeqHst])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVConsHstPK] ON [dbo].[GE_QVConsHst] ([SeqCons], [SeqHst]);
GO
CREATE NONCLUSTERED INDEX [GE_QVConsHstIF1] ON [dbo].[GE_QVConsHst] ([SeqCons]);
GO
ALTER TABLE [dbo].[GE_QVConsHst] ADD CONSTRAINT [FK__GE_QVCons__SeqCo__278583FA] FOREIGN KEY ([SeqCons]) REFERENCES [dbo].[GE_QVCons] ([SeqCons]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_QVConsVar
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_QVConsVar] (
    [SeqCons] numeric(18,0) NOT NULL,
    [Var] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Ord] decimal(2,0) NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Padrao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DepAnt] numeric(1,0) NULL,
    [Instrucao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InstrSql] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Lista] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SQLCmd] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Externa] numeric(1,0) NULL,
    [TpDado] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tudo] numeric(1,0) NULL,
    [LimLow] decimal(15,2) NULL,
    [CorLow] numeric(18,0) NULL,
    [LimHig] decimal(15,2) NULL,
    [CorHig] numeric(18,0) NULL,
    CONSTRAINT [PK__GE_QVConsVar__442BE449] PRIMARY KEY CLUSTERED ([SeqCons], [Var])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVConsVarPK] ON [dbo].[GE_QVConsVar] ([SeqCons], [Var]);
GO
CREATE NONCLUSTERED INDEX [GE_QVConsVarIF1] ON [dbo].[GE_QVConsVar] ([SeqCons]);
GO
ALTER TABLE [dbo].[GE_QVConsVar] ADD CONSTRAINT [FK__GE_QVCons__SeqCo__2879A833] FOREIGN KEY ([SeqCons]) REFERENCES [dbo].[GE_QVCons] ([SeqCons]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_QVPasta
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_QVPasta] (
    [SeqPasta] decimal(6,0) NOT NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CodPasta] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPastaPai] decimal(6,0) NULL,
    CONSTRAINT [PK__GE_QVPasta__45200882] PRIMARY KEY CLUSTERED ([SeqPasta])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVPastaPK] ON [dbo].[GE_QVPasta] ([SeqPasta]);
GO
CREATE NONCLUSTERED INDEX [GE_QVPastaIF1] ON [dbo].[GE_QVPasta] ([SeqPastaPai]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_QVPastaCons
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_QVPastaCons] (
    [SeqPasta] decimal(6,0) NOT NULL,
    [SeqCons] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__GE_QVPastaCons__46142CBB] PRIMARY KEY CLUSTERED ([SeqPasta], [SeqCons])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_QVPastaConsPK] ON [dbo].[GE_QVPastaCons] ([SeqPasta], [SeqCons]);
GO
CREATE NONCLUSTERED INDEX [GE_QVPastaConsIF1] ON [dbo].[GE_QVPastaCons] ([SeqCons]);
GO
CREATE NONCLUSTERED INDEX [GE_QVPastaConsIF2] ON [dbo].[GE_QVPastaCons] ([SeqPasta]);
GO
ALTER TABLE [dbo].[GE_QVPastaCons] ADD CONSTRAINT [FK__GE_QVPast__SeqCo__2A61F0A5] FOREIGN KEY ([SeqCons]) REFERENCES [dbo].[GE_QVCons] ([SeqCons]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[GE_QVPastaCons] ADD CONSTRAINT [FK__GE_QVPast__SeqPa__2B5614DE] FOREIGN KEY ([SeqPasta]) REFERENCES [dbo].[GE_QVPasta] ([SeqPasta]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_QVRegra
   Criada em ..: 2013-12-20
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_QVRegra] (
    [SeqRegra] numeric(18,0) NOT NULL,
    [SeqCons] numeric(18,0) NOT NULL,
    [Regra] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ordem] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Condicao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Acao1] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Acao2] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_QVRegra__35099C4A] PRIMARY KEY CLUSTERED ([SeqRegra])
);
GO
CREATE NONCLUSTERED INDEX [GE_QVREGRAIF1] ON [dbo].[GE_QVRegra] ([SeqCons]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Regiao
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Regiao] (
    [SeqRegiao] decimal(6,0) NOT NULL,
    [Regiao] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Regiao__470850F4] PRIMARY KEY CLUSTERED ([SeqRegiao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_RegiaoPK] ON [dbo].[GE_Regiao] ([SeqRegiao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_RelPasta
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_RelPasta] (
    [SeqPasta] numeric(18,0) NOT NULL,
    [Nome] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPastaPai] numeric(18,0) NULL,
    CONSTRAINT [PK__GE_RelPa__61E733949022AB94] PRIMARY KEY CLUSTERED ([SeqPasta])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_RelPasta] ON [dbo].[GE_RelPasta] ([SeqPasta]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_RelPasta] ON [dbo].[GE_RelPasta] ([SeqPastaPai]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_RelPastaCons
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_RelPastaCons] (
    [SeqPasta] numeric(18,0) NOT NULL,
    [SEQCONSSQL] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__GE_RelPa__5D15BBCC19653F4A] PRIMARY KEY CLUSTERED ([SeqPasta], [SEQCONSSQL])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_RelPastaCons] ON [dbo].[GE_RelPastaCons] ([SeqPasta], [SEQCONSSQL]);
GO
CREATE NONCLUSTERED INDEX [XIF1GE_RelPastaCons] ON [dbo].[GE_RelPastaCons] ([SeqPasta]);
GO
CREATE NONCLUSTERED INDEX [XIF2GE_RelPastaCons] ON [dbo].[GE_RelPastaCons] ([SEQCONSSQL]);
GO
ALTER TABLE [dbo].[GE_RelPastaCons] ADD CONSTRAINT [FK__GE_RelPas__SeqPa__46741F6E] FOREIGN KEY ([SeqPasta]) REFERENCES [dbo].[GE_RelPasta] ([SeqPasta]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[GE_RelPastaCons] ADD CONSTRAINT [FK__GE_RelPas__SEQCO__476843A7] FOREIGN KEY ([SEQCONSSQL]) REFERENCES [dbo].[GE_CONSSQL] ([SEQCONSSQL]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_RELTEMPLATE
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_RELTEMPLATE] (
    [SEQRELTEMPLATE] numeric(18,0) NOT NULL,
    [TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NOME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ARQUIVONOME] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEMPLATE] text COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_RELTE__01DB06A4F78198D5] PRIMARY KEY CLUSTERED ([SEQRELTEMPLATE])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Rota
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Rota] (
    [SeqRota] decimal(6,0) NOT NULL,
    [Rota] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [LinkStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Rota__47FC752D] PRIMARY KEY CLUSTERED ([SeqRota])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_RotaPK] ON [dbo].[GE_Rota] ([SeqRota]);
GO
CREATE NONCLUSTERED INDEX [GE_RotaIE1] ON [dbo].[GE_Rota] ([Rota]);
GO
CREATE NONCLUSTERED INDEX [GE_RotaIE2] ON [dbo].[GE_Rota] ([LinkStr]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Sequencia
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Sequencia] (
    [NomeTabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Sequencia] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__GE_Sequencia__48F09966] PRIMARY KEY CLUSTERED ([NomeTabela])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_SequenciaPK] ON [dbo].[GE_Sequencia] ([NomeTabela]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Sistema
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Sistema] (
    [Sistema] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SiglaSistema] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__GE_Sistema__49E4BD9F] PRIMARY KEY CLUSTERED ([Sistema])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_SistemaPK] ON [dbo].[GE_Sistema] ([Sistema]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_SyncParam
   Criada em ..: 2017-09-11
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_SyncParam] (
    [Origem] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ChaveN1] numeric(18,0) NOT NULL,
    [ChaveN2] numeric(18,0) NOT NULL,
    [ChaveStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ChaveLocalN1] numeric(18,0) NULL,
    [ChaveLocalN2] numeric(18,0) NULL,
    [ChaveLocalStr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [IndMudanca] numeric(1,0) NULL,
    CONSTRAINT [PK__GE_SyncP__BF490468AAD36354] PRIMARY KEY CLUSTERED ([Origem], [Tabela], [ChaveN1], [ChaveN2], [ChaveStr])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKGE_SyncParam] ON [dbo].[GE_SyncParam] ([Origem], [Tabela], [ChaveN1], [ChaveN2], [ChaveStr]);
GO
CREATE NONCLUSTERED INDEX [XIE1GE_SyncParam] ON [dbo].[GE_SyncParam] ([Tabela], [ChaveLocalN1]);
GO
CREATE NONCLUSTERED INDEX [XIE2GE_SyncParam] ON [dbo].[GE_SyncParam] ([Tabela], [ChaveLocalStr]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Tab
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Tab] (
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_Tab__7212AA8A] PRIMARY KEY CLUSTERED ([Tabela])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_TabPK] ON [dbo].[GE_Tab] ([Tabela]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_TabRegra
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_TabRegra] (
    [SeqTabRegra] numeric(18,0) NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPolSeg] numeric(6,0) NOT NULL,
    [Condicao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Regra] numeric(18,0) NULL,
    CONSTRAINT [PK__GE_TabRegra__73FAF2FC] PRIMARY KEY CLUSTERED ([SeqTabRegra])
);
GO
CREATE NONCLUSTERED INDEX [GE_TABREGRAAK1] ON [dbo].[GE_TabRegra] ([Tabela], [SeqPolSeg], [Condicao]);
GO
CREATE NONCLUSTERED INDEX [GE_TABREGRAIF2] ON [dbo].[GE_TabRegra] ([SeqPolSeg]);
GO
CREATE NONCLUSTERED INDEX [GE_TABREGRAIF1] ON [dbo].[GE_TabRegra] ([Tabela]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_TabRegraPK] ON [dbo].[GE_TabRegra] ([SeqTabRegra]);
GO
ALTER TABLE [dbo].[GE_TabRegra] ADD CONSTRAINT [FK__GE_TabReg__Tabel__2C4A3917] FOREIGN KEY ([Tabela]) REFERENCES [dbo].[GE_Tab] ([Tabela]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_TempLong
   Criada em ..: 2014-12-23
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_TempLong] (
    [kn] numeric(18,0) NOT NULL,
    [ks] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Dado] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_TempLong__499AA4DC] PRIMARY KEY CLUSTERED ([kn], [ks])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_TipoLogradouro
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_TipoLogradouro] (
    [TipoLogradouro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Substituicoes] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_TipoLogradour__4AD8E1D8] PRIMARY KEY CLUSTERED ([TipoLogradouro])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_TipoLogradouPK] ON [dbo].[GE_TipoLogradouro] ([TipoLogradouro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_URACENARIO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_URACENARIO] (
    [SEQURACENARIO] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [IDCENARIO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REMETENTE] numeric(12,0) NULL,
    [MINPERIODO] numeric(4,0) NULL,
    [MAXPERIODO] numeric(4,0) NULL,
    [MAXCOUNT] numeric(1,0) NULL,
    [NOMEVIEW] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARAMETROS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SCRIPT] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTIFYURL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTIFYCONTENTTYPE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_URACE__167BABA18F8CD502] PRIMARY KEY CLUSTERED ([SEQURACENARIO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_URACENARIO_0] ON [dbo].[GE_URACENARIO] ([NOME]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_UsrAcSp
   Criada em ..: 2016-07-12
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_UsrAcSp] (
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [KS1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Str1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAtribuido] datetime NULL,
    [DtaLimite] datetime NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_UsrAc__72BB6B13079F1A46] PRIMARY KEY CLUSTERED ([Tipo], [SeqUsuario], [KS1])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsrAcSpPK] ON [dbo].[GE_UsrAcSp] ([Tipo], [SeqUsuario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_UsrCtrl
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_UsrCtrl] (
    [Seq] numeric(18,0) NOT NULL,
    [Uc] numeric(18,0) NULL,
    [Sm] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Mq] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cx] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dc] datetime NULL,
    [Up] datetime NULL,
    CONSTRAINT [PK__GE_UsrCtrl__4BCD0611] PRIMARY KEY CLUSTERED ([Seq])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsrCtrlPK] ON [dbo].[GE_UsrCtrl] ([Seq]);
GO
CREATE NONCLUSTERED INDEX [GE_UsrCtrlIE1] ON [dbo].[GE_UsrCtrl] ([Sm], [Uc]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_UsrParam
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_UsrParam] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Parametro] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Valor] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Criptografado] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtaalteracao] datetime NULL,
    [Usualteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_UsrParam__4CC12A4A] PRIMARY KEY CLUSTERED ([SeqUsuario], [NroEmpresa], [Parametro])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsrParamPK] ON [dbo].[GE_UsrParam] ([SeqUsuario], [NroEmpresa], [Parametro]);
GO
CREATE NONCLUSTERED INDEX [GE_UsrParamIF1] ON [dbo].[GE_UsrParam] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[GE_UsrParam] ADD CONSTRAINT [FK__GE_UsrPar__SeqUs__2E328189] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Usuario
   Criada em ..: 2011-12-19
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Usuario] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Nome] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeReduzido] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [Senha] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LoginId] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoUsuario] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [RegistrarLog] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALIMITEUSO] datetime NULL,
    [Nivel] decimal(1,0) NULL,
    [Assinatura] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodUsuarioExt] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroUsuarioExt] numeric(18,0) NULL,
    [RecebeCiencia] numeric(1,0) NULL,
    [Senha3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Loginexpirando] numeric(1,0) NULL,
    [Qtdeloginrestante] numeric(18,0) NULL,
    [Ulttrocasenha] datetime NULL,
    [Identificacao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPolSeg] decimal(6,0) NULL,
    [CelularTrab] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDSILO] numeric(1,0) NULL,
    [ChkSum] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOSILO] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQCONTATO] numeric(4,0) NULL,
    [INDUSREXT] numeric(1,0) NULL,
    [LOGIN] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAILTRAB] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTALOGIN] datetime NULL,
    [DTALOGINANT] datetime NULL,
    CONSTRAINT [PK__GE_Usuario__4DB54E83] PRIMARY KEY CLUSTERED ([SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsuarioPK] ON [dbo].[GE_Usuario] ([SeqUsuario]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsuarioAK1] ON [dbo].[GE_Usuario] ([CodUsuario]);
GO
CREATE NONCLUSTERED INDEX [GE_UsuarioIF2] ON [dbo].[GE_Usuario] ([SeqPolSeg]);
GO
ALTER TABLE [dbo].[GE_Usuario] ADD CONSTRAINT [FK__GE_Usuari__SeqPo__2F26A5C2] FOREIGN KEY ([SeqPolSeg]) REFERENCES [dbo].[GE_PolSeg] ([SeqPolSeg]);
GO
ALTER TABLE [dbo].[GE_Usuario] ADD CONSTRAINT [FK__GE_Usuari__SeqPo__68E93303] FOREIGN KEY ([SeqPolSeg]) REFERENCES [dbo].[GE_PolSeg] ([SeqPolSeg]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_Usuario_BKP20250520
   Criada em ..: 2025-05-20
   Alterada em : 2025-05-20
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_Usuario_BKP20250520] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Nome] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeReduzido] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [Senha] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LoginId] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoUsuario] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [RegistrarLog] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTALIMITEUSO] datetime NULL,
    [Nivel] decimal(1,0) NULL,
    [Assinatura] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodUsuarioExt] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroUsuarioExt] numeric(18,0) NULL,
    [RecebeCiencia] numeric(1,0) NULL,
    [Senha3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Loginexpirando] numeric(1,0) NULL,
    [Qtdeloginrestante] numeric(18,0) NULL,
    [Ulttrocasenha] datetime NULL,
    [Identificacao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqPolSeg] decimal(6,0) NULL,
    [CelularTrab] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDSILO] numeric(1,0) NULL,
    [ChkSum] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOSILO] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQCONTATO] numeric(4,0) NULL,
    [INDUSREXT] numeric(1,0) NULL,
    [LOGIN] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAILTRAB] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTALOGIN] datetime NULL,
    [DTALOGINANT] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_USUARIOCMPL
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_USUARIOCMPL] (
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [G_TOKEN] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [G_ULTSINC] datetime NULL,
    [G_STATUSSINC] numeric(1,0) NULL,
    [G_IDCALENDARIO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_USUAR__59D861DEEA3D37F6] PRIMARY KEY CLUSTERED ([SEQUSUARIO])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_UsuarioLink
   Criada em ..: 2011-12-19
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_UsuarioLink] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [SeqUsrLink] decimal(6,0) NOT NULL,
    [NroUsuarioExt] numeric(18,0) NULL,
    [CodUsuarioExt] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_UsuarioLink__4EA972BC] PRIMARY KEY CLUSTERED ([SeqUsuario], [SeqUsrLink])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsuarioLinkPK] ON [dbo].[GE_UsuarioLink] ([SeqUsuario], [SeqUsrLink]);
GO
CREATE NONCLUSTERED INDEX [GE_UsuarioLinkIF1] ON [dbo].[GE_UsuarioLink] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[GE_UsuarioLink] ADD CONSTRAINT [FK__GE_Usuari__SeqUs__301AC9FB] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_UsuarioPerm
   Criada em ..: 2011-12-19
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_UsuarioPerm] (
    [CodAplicacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ChaveAplicacao] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Permissao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__GE_UsuarioPerm__4F9D96F5] PRIMARY KEY CLUSTERED ([CodAplicacao], [ChaveAplicacao], [SeqUsuario], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsuarioPermPK] ON [dbo].[GE_UsuarioPerm] ([CodAplicacao], [ChaveAplicacao], [SeqUsuario], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [GE_UsuarioPermIE1] ON [dbo].[GE_UsuarioPerm] ([ChaveAplicacao], [CodAplicacao]);
GO
CREATE NONCLUSTERED INDEX [GE_UsuarioPermIF1] ON [dbo].[GE_UsuarioPerm] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [GE_USUARIOPERMIE2] ON [dbo].[GE_UsuarioPerm] ([CodAplicacao], [NroEmpresa], [Permissao], [SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IDX_GE_USUARIOPERM_01] ON [dbo].[GE_UsuarioPerm] ([Permissao], [CodAplicacao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_UsuarioPerm_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_UsuarioPerm_BKPJUN] (
    [CodAplicacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ChaveAplicacao] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Permissao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.GE_UsuarioSenhaMem
   Criada em ..: 2018-08-15
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[GE_UsuarioSenhaMem] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [DtaTroca] datetime NOT NULL,
    [Senha] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_GE_UsuarioSenhaMem] PRIMARY KEY CLUSTERED ([SeqUsuario], [DtaTroca])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [GE_UsuarioSenhaPK] ON [dbo].[GE_UsuarioSenhaMem] ([SeqUsuario], [DtaTroca]);
GO
CREATE NONCLUSTERED INDEX [GE_UsuarioSenhaIF1] ON [dbo].[GE_UsuarioSenhaMem] ([SeqUsuario]);
GO

