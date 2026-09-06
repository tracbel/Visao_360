/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IVS
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_CALL_FORAREGIAO
   Criada em ..: 2015-06-26
   Alterada em : 2015-06-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_CALL_FORAREGIAO] (
    [SEQPESSOA] numeric(10,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.ivs_callcenter
   Criada em ..: 2015-06-25
   Alterada em : 2015-06-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[ivs_callcenter] (
    [SeqPessoa] numeric(10,0) NULL,
    [POTENCIAL] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.ivs_callcidades
   Criada em ..: 2015-06-25
   Alterada em : 2015-06-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[ivs_callcidades] (
    [cidade] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [cen] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [cenok] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_CanalVenda
   Criada em ..: 2014-09-15
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_CanalVenda] (
    [SeqCanal] decimal(2,0) NOT NULL,
    [Canal] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqCanalPreco] decimal(2,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_CanalVenda__5852D887] PRIMARY KEY CLUSTERED ([SeqCanal])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_CartCategoria
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_CartCategoria] (
    [SEQCARTEIRA] decimal(6,0) NOT NULL,
    [SeqCategoria] decimal(4,0) NOT NULL,
    CONSTRAINT [PK__IVS_CartCategori__5946FCC0] PRIMARY KEY CLUSTERED ([SEQCARTEIRA], [SeqCategoria])
);
GO
CREATE NONCLUSTERED INDEX [IVS_CartCategorIF1] ON [dbo].[IVS_CartCategoria] ([SEQCARTEIRA]);
GO
CREATE NONCLUSTERED INDEX [IVS_CartCategorIF2] ON [dbo].[IVS_CartCategoria] ([SeqCategoria]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_CartCid
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_CartCid] (
    [SeqCarteira] decimal(4,0) NOT NULL,
    [SeqCidade] decimal(6,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_CartCid__6740165C] PRIMARY KEY CLUSTERED ([SeqCarteira], [SeqCidade])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_CartCidPK] ON [dbo].[IVS_CartCid] ([SeqCarteira], [SeqCidade]);
GO
CREATE NONCLUSTERED INDEX [IVS_CartCidIF1] ON [dbo].[IVS_CartCid] ([SeqCidade]);
GO
CREATE NONCLUSTERED INDEX [IVS_CartCidIF2] ON [dbo].[IVS_CartCid] ([SeqCarteira]);
GO
ALTER TABLE [dbo].[IVS_CartCid] ADD CONSTRAINT [FK__IVS_CartC__SeqCa__4A99B60D] FOREIGN KEY ([SeqCarteira]) REFERENCES [dbo].[IVS_Carteira] ([SeqCarteira]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVS_CartCid] ADD CONSTRAINT [FK__IVS_CartC__SeqCi__71F37A50] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GE_Cidade] ([SeqCidade]);
GO
ALTER TABLE [dbo].[IVS_CartCid] ADD CONSTRAINT [FK__IVS_CartC__SeqCi__700B31DE] FOREIGN KEY ([SeqCidade]) REFERENCES [dbo].[GE_Cidade] ([SeqCidade]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_CartDepto
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_CartDepto] (
    [SeqCarteira] decimal(4,0) NOT NULL,
    [SeqDepto] decimal(4,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_CartDepto__68343A95] PRIMARY KEY CLUSTERED ([SeqCarteira], [SeqDepto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_CartDeptoPK] ON [dbo].[IVS_CartDepto] ([SeqCarteira], [SeqDepto]);
GO
CREATE NONCLUSTERED INDEX [IVS_CartDeptoIF1] ON [dbo].[IVS_CartDepto] ([SeqDepto]);
GO
CREATE NONCLUSTERED INDEX [IVS_CartDeptoIF2] ON [dbo].[IVS_CartDepto] ([SeqCarteira]);
GO
ALTER TABLE [dbo].[IVS_CartDepto] ADD CONSTRAINT [FK__IVS_CartD__SeqDe__4B8DDA46] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVS_CartDepto] ADD CONSTRAINT [FK__IVS_CartD__SeqCa__4C81FE7F] FOREIGN KEY ([SeqCarteira]) REFERENCES [dbo].[IVS_Carteira] ([SeqCarteira]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Carteira
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Carteira] (
    [SeqCarteira] decimal(4,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Carteira] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrResp] numeric(18,0) NULL,
    [SeqUrSuperv] numeric(18,0) NULL,
    [SeqVendedor] numeric(18,0) NULL,
    [SeqCanal] decimal(2,0) NULL,
    [SeqRegional] numeric(8,0) NULL,
    [SeqGrupoPreco] decimal(6,0) NULL,
    [DTAALTERACAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    [AUTOSINCCID] numeric(1,0) NULL,
    [AUTOSINCSTATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_Carteira__69285ECE] PRIMARY KEY CLUSTERED ([SeqCarteira])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_CarteiraPK] ON [dbo].[IVS_Carteira] ([SeqCarteira]);
GO
CREATE NONCLUSTERED INDEX [XIF4IVS_CARTEIRA] ON [dbo].[IVS_Carteira] ([SeqVendedor]);
GO
CREATE NONCLUSTERED INDEX [XIF1IVS_Carteira] ON [dbo].[IVS_Carteira] ([SeqCanal]);
GO
CREATE NONCLUSTERED INDEX [XIF3IVS_Carteira] ON [dbo].[IVS_Carteira] ([SeqRegional]);
GO
CREATE NONCLUSTERED INDEX [XIF5IVS_Carteira] ON [dbo].[IVS_Carteira] ([SeqGrupoPreco]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Categoria
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Categoria] (
    [SeqCategoria] decimal(4,0) NOT NULL,
    [Categoria] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_Categoria__548247A3] PRIMARY KEY CLUSTERED ([SeqCategoria])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Depto
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Depto] (
    [SeqDepto] decimal(4,0) NOT NULL,
    [Depto] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrDirDepto] numeric(18,0) NULL,
    [SeqSegm] decimal(8,0) NOT NULL,
    [CtrlPorEmpresa] numeric(1,0) NULL,
    [MultCarteira] numeric(1,0) NULL,
    [CodProcesso] decimal(4,0) NULL,
    [CicloA] decimal(3,0) NULL,
    [CicloB] decimal(3,0) NULL,
    [CicloC] decimal(3,0) NULL,
    [CicloD] decimal(3,0) NULL,
    [CicloN] decimal(3,0) NULL,
    [CicloP] decimal(3,0) NULL,
    [CicloAExt] decimal(3,0) NULL,
    [CicloBExt] decimal(3,0) NULL,
    [CicloCExt] decimal(3,0) NULL,
    [CicloDExt] decimal(3,0) NULL,
    [AcaoContato] numeric(8,0) NULL,
    [AcaoExterna] numeric(8,0) NULL,
    [SeqDeptoPreco] decimal(4,0) NULL,
    [SeqNegocio] decimal(4,0) NULL,
    [CICLOE] numeric(3,0) NULL,
    [CICLOEEXT] numeric(3,0) NULL,
    [INDUSAPOTZ] numeric(1,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_Depto__6A1C8307] PRIMARY KEY CLUSTERED ([SeqDepto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_DeptoPK] ON [dbo].[IVS_Depto] ([SeqDepto]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_DeptoAK1] ON [dbo].[IVS_Depto] ([Depto]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoIF1] ON [dbo].[IVS_Depto] ([CodProcesso]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoIF3] ON [dbo].[IVS_Depto] ([SeqSegm]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoIF4] ON [dbo].[IVS_Depto] ([SeqUsrDirDepto]);
GO
ALTER TABLE [dbo].[IVS_Depto] ADD CONSTRAINT [FK__IVS_Depto__CodPr__4D7622B8] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IVS_Depto] ADD CONSTRAINT [FK__IVS_Depto__SeqSe__4E6A46F1] FOREIGN KEY ([SeqSegm]) REFERENCES [dbo].[IVS_Segm] ([SeqSegm]);
GO
ALTER TABLE [dbo].[IVS_Depto] ADD CONSTRAINT [FK__IVS_Depto__SeqUs__4F5E6B2A] FOREIGN KEY ([SeqUsrDirDepto]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IVS_Depto] ADD CONSTRAINT [FK__IVS_Depto__SeqUs__0920F86B] FOREIGN KEY ([SeqUsrDirDepto]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IVS_Depto] ADD CONSTRAINT [FK__IVS_Depto__CodPr__0738AFF9] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IVS_Depto] ADD CONSTRAINT [FK__IVS_Depto__SeqSe__082CD432] FOREIGN KEY ([SeqSegm]) REFERENCES [dbo].[IVS_Segm] ([SeqSegm]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_DeptoDePara
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_DeptoDePara] (
    [SeqDepto] decimal(4,0) NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__IVS_DeptoDePara__6B10A740] PRIMARY KEY CLUSTERED ([SeqDepto], [Descricao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_DeptoDeParaPK] ON [dbo].[IVS_DeptoDePara] ([SeqDepto], [Descricao]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoDeParaIF2] ON [dbo].[IVS_DeptoDePara] ([SeqDepto]);
GO
ALTER TABLE [dbo].[IVS_DeptoDePara] ADD CONSTRAINT [FK__IVS_Depto__SeqDe__50528F63] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_DeptoEmpr
   Criada em ..: 2012-04-13
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_DeptoEmpr] (
    [SeqDepto] decimal(4,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqUsrGerDepto] numeric(18,0) NULL,
    [SeqUsrDirDepto] numeric(18,0) NULL,
    [SEQUSUARIO] numeric(18,0) NULL,
    CONSTRAINT [PK__IVS_DeptoEmpr__6C04CB79] PRIMARY KEY CLUSTERED ([SeqDepto], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_DeptoEmprPK] ON [dbo].[IVS_DeptoEmpr] ([SeqDepto], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoEmprIF1] ON [dbo].[IVS_DeptoEmpr] ([SeqUsrDirDepto]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoEmprIF2] ON [dbo].[IVS_DeptoEmpr] ([SeqUsrGerDepto]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoEmprIF3] ON [dbo].[IVS_DeptoEmpr] ([SeqDepto]);
GO
CREATE NONCLUSTERED INDEX [XIF4IVS_DEPTOEMPR] ON [dbo].[IVS_DeptoEmpr] ([SEQUSUARIO]);
GO
ALTER TABLE [dbo].[IVS_DeptoEmpr] ADD CONSTRAINT [FK__IVS_Depto__SeqUs__0B0940DD] FOREIGN KEY ([SeqUsrDirDepto]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IVS_DeptoEmpr] ADD CONSTRAINT [FK__IVS_Depto__SeqDe__532EFC0E] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVS_DeptoEmpr] ADD CONSTRAINT [FK__IVS_Depto__SeqUs__5146B39C] FOREIGN KEY ([SeqUsrDirDepto]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IVS_DeptoEmpr] ADD CONSTRAINT [FK__IVS_Depto__SeqUs__523AD7D5] FOREIGN KEY ([SeqUsrGerDepto]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IVS_DeptoEmpr] ADD CONSTRAINT [FK__IVS_Depto__SeqUs__0BFD6516] FOREIGN KEY ([SeqUsrGerDepto]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_DEPTOPOT
   Criada em ..: 2023-06-05
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_DEPTOPOT] (
    [SEQPOTENCIALDP] numeric(6,0) NOT NULL,
    [SEQDEPTO] numeric(4,0) NOT NULL,
    [POTENCIAL] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ORDEM] numeric(2,0) NOT NULL,
    [DIASCICLOCTTO] numeric(4,0) NULL,
    [DIASCICLOVISITA] numeric(4,0) NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    CONSTRAINT [PK__IVS_DEPT__2C683F3CC591CEBD] PRIMARY KEY CLUSTERED ([SEQPOTENCIALDP])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_DeptoRes
   Criada em ..: 2012-04-13
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_DeptoRes] (
    [SeqDepto] decimal(4,0) NOT NULL,
    [Resultado] decimal(6,0) NOT NULL,
    [IndContato] numeric(1,0) NOT NULL,
    [IndExterno] numeric(1,0) NULL,
    [IndExtFormaCtto] numeric(1,0) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IVS_DeptoRes__6CF8EFB2] PRIMARY KEY CLUSTERED ([SeqDepto], [Resultado])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_DeptoResPK] ON [dbo].[IVS_DeptoRes] ([SeqDepto], [Resultado]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoResIF1] ON [dbo].[IVS_DeptoRes] ([SeqDepto]);
GO
CREATE NONCLUSTERED INDEX [IVS_DeptoResIF2] ON [dbo].[IVS_DeptoRes] ([Resultado]);
GO
ALTER TABLE [dbo].[IVS_DeptoRes] ADD CONSTRAINT [FK__IVS_Depto__SeqDe__54232047] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVS_DeptoRes] ADD CONSTRAINT [FK__IVS_Depto__Resul__55174480] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_NegCategoria
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_NegCategoria] (
    [SeqNegocio] decimal(4,0) NOT NULL,
    [SeqCategoria] decimal(4,0) NOT NULL,
    CONSTRAINT [PK__IVS_NegCategoria__55766BDC] PRIMARY KEY CLUSTERED ([SeqNegocio], [SeqCategoria])
);
GO
CREATE NONCLUSTERED INDEX [IVS_NegCategoriIF1] ON [dbo].[IVS_NegCategoria] ([SeqNegocio]);
GO
CREATE NONCLUSTERED INDEX [XIF2IVS_NegCategoria] ON [dbo].[IVS_NegCategoria] ([SeqCategoria]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Negocio
   Criada em ..: 2014-09-15
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Negocio] (
    [SeqNegocio] decimal(4,0) NOT NULL,
    [Negocio] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sigla] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrResp] numeric(18,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_Negocio__566A9015] PRIMARY KEY CLUSTERED ([SeqNegocio])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Pes
   Criada em ..: 2017-05-29
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Pes] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqDepto] decimal(4,0) NOT NULL,
    [SeqPesDepto] numeric(10,0) NOT NULL,
    [SeqCarteira] decimal(4,0) NOT NULL,
    [Ciclo] decimal(3,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Situacao] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Potencial] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Perspectiva] decimal(2,0) NULL,
    [DtaCalc] datetime NULL,
    [Rec] decimal(1,0) NULL,
    [Freq] numeric(1,0) NULL,
    [Vlr] numeric(1,0) NULL,
    [Pto] numeric(1,0) NULL,
    [QtdTrans] numeric(18,0) NULL,
    [ProcUltTrans] numeric(18,0) NULL,
    [DtaIniTrans] datetime NULL,
    [DtaUltTrans] datetime NULL,
    [DtaUltCtto] datetime NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltCttoExt] datetime NULL,
    [CicloExt] decimal(3,0) NULL,
    [Score] decimal(2,0) NULL,
    [Classe] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Recalc] numeric(1,0) NULL,
    [SEQPOTENCIALDP] numeric(6,0) NULL,
    [INDTROCAPORCIDBLOQ] numeric(1,0) NULL,
    CONSTRAINT [PK__IVS_Pes__1295DCF666F2779D] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqDepto], [SeqPesDepto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [UNQ_IVS_PESCRT] ON [dbo].[IVS_Pes] ([SeqPessoa], [SeqCarteira]);
GO
ALTER TABLE [dbo].[IVS_Pes] ADD CONSTRAINT [FK__IVS_Pes__SeqDept__0AFF1F3C] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]);
GO
ALTER TABLE [dbo].[IVS_Pes] ADD CONSTRAINT [FK__IVS_Pes__SEQPOTE__5EE17133] FOREIGN KEY ([SEQPOTENCIALDP]) REFERENCES [dbo].[IVS_DEPTOPOT] ([SEQPOTENCIALDP]);
GO
ALTER TABLE [dbo].[IVS_Pes] ADD CONSTRAINT [FK__IVS_Pes__SeqPess__0BF34375] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IVS_Pes] ADD CONSTRAINT [FK__IVS_Pes__SeqPess__0CE767AE] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_PES_MAQ_PECAS
   Criada em ..: 2020-03-05
   Alterada em : 2020-03-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_PES_MAQ_PECAS] (
    [SeqPessoa] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Carteira] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Departamento] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSULTOR] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Seqcarteira] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqDepto] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_PES_NELSON
   Criada em ..: 2025-08-07
   Alterada em : 2025-08-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_PES_NELSON] (
    [SEQPESSOA] int NULL,
    [seqcarteira] int NULL,
    [dtaultctto] datetime NULL,
    [dtaultcttoext] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_PES_PNEUS
   Criada em ..: 2020-03-05
   Alterada em : 2020-03-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_PES_PNEUS] (
    [SeqPessoa] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Carteira] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Departamento] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSULTOR] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Seqcarteira] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqDepto] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Pes_RAO_Pneus_02_03
   Criada em ..: 2019-05-10
   Alterada em : 2019-05-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Pes_RAO_Pneus_02_03] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqDepto] decimal(4,0) NOT NULL,
    [SeqPesDepto] numeric(10,0) NOT NULL,
    [SeqCarteira] decimal(4,0) NOT NULL,
    [Ciclo] decimal(3,0) NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Situacao] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Potencial] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Perspectiva] decimal(2,0) NULL,
    [DtaCalc] datetime NULL,
    [Rec] decimal(1,0) NULL,
    [Freq] numeric(1,0) NULL,
    [Vlr] numeric(1,0) NULL,
    [Pto] numeric(1,0) NULL,
    [QtdTrans] numeric(18,0) NULL,
    [ProcUltTrans] numeric(18,0) NULL,
    [DtaIniTrans] datetime NULL,
    [DtaUltTrans] datetime NULL,
    [DtaUltCtto] datetime NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltCttoExt] datetime NULL,
    [CicloExt] decimal(3,0) NULL,
    [Score] decimal(2,0) NULL,
    [Classe] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Recalc] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Regional
   Criada em ..: 2014-09-15
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Regional] (
    [SeqRegional] numeric(8,0) NOT NULL,
    [Regional] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrResp] numeric(18,0) NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVS_Regional__575EB44E] PRIMARY KEY CLUSTERED ([SeqRegional])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_Segm
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_Segm] (
    [SeqSegm] decimal(8,0) NOT NULL,
    [SegmSigla] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Segm] varchar(14) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrResp] numeric(18,0) NULL,
    CONSTRAINT [PK__IVS_Segm__6EE13824] PRIMARY KEY CLUSTERED ([SeqSegm])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_SegmPK] ON [dbo].[IVS_Segm] ([SeqSegm]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_TGLCLICLIENT
   Criada em ..: 2015-06-26
   Alterada em : 2015-07-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_TGLCLICLIENT] (
    [CLIENTNRO] int NULL,
    [CLIENTNOMSIGLA] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNOMRAZAO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNOMRUA] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNROEND] int NULL,
    [CLIENTDESCOMEND] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNOMBAIRRO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNOMCIDADE] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTCODCEP] int NULL,
    [ESTADOCOD] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNOMPAIS] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNRODDD] int NULL,
    [CLIENTNROTEL] int NULL,
    [CLIENTNROALTDDD] int NULL,
    [CLIENTNROTELALT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTNROFAXDDD] int NULL,
    [CLIENTNROFAX] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTDATCAD] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GRUCLICOD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REGIAOCOD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTIDTFISJUR] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORNECNRO] int NULL,
    [DTIMPORT] datetime NULL,
    [USUIMPORT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTREFERENCIA] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_TGLCLIFIS
   Criada em ..: 2015-06-26
   Alterada em : 2015-07-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_TGLCLIFIS] (
    [CLIENTNRO] int NULL,
    [CLIFISNROCPF] numeric(11,0) NULL,
    [CLIFISNROID] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIFISNOMORGID] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATPEFICOD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIFISNOMEMPRESA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIFISNOMCARGO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FUNCAOCOD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIFISIDTSEXO] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIFISDATNASC] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RENMENCOD] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIFISDESEMAIL] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIFISDESINSEST] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTIMPORT] datetime NULL,
    [USUIMPORT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTREFERENCIA] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_TGLCLIJUR
   Criada em ..: 2015-06-26
   Alterada em : 2015-07-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_TGLCLIJUR] (
    [CLIENTNRO] int NULL,
    [CLIJURNROCGC] numeric(15,0) NULL,
    [CLIJURNROINSEST] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIJURNROINSMUN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIJURNROREGJUN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATPEJUCOD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIJURDESEMAIL] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTIMPORT] datetime NULL,
    [USUIMPORT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTREFERENCIA] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVS_UsrMeta
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVS_UsrMeta] (
    [SeqUsrMeta] numeric(18,0) NOT NULL,
    [CodProcesso] decimal(4,0) NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [DtaReferencia] datetime NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [QtdProspec] decimal(4,0) NULL,
    [QtdContato] decimal(4,0) NULL,
    [QtdVenda] decimal(4,0) NULL,
    [QtdeDemo] decimal(4,0) NULL,
    [VlrVenda] decimal(15,2) NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IVS_UsrMeta__6FD55C5D] PRIMARY KEY CLUSTERED ([SeqUsrMeta])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_UsrMetaPK] ON [dbo].[IVS_UsrMeta] ([SeqUsrMeta]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVS_UsrMetaAK1] ON [dbo].[IVS_UsrMeta] ([CodProcesso], [SeqUsuario], [DtaReferencia], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IVS_UsrMetaIE1] ON [dbo].[IVS_UsrMeta] ([SeqUsuario], [DtaReferencia]);
GO
CREATE NONCLUSTERED INDEX [IVS_UsrMetaIF1] ON [dbo].[IVS_UsrMeta] ([CodProcesso]);
GO
CREATE NONCLUSTERED INDEX [IVS_UsrMetaIF2] ON [dbo].[IVS_UsrMeta] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IVS_UsrMeta] ADD CONSTRAINT [FK__IVS_UsrMe__CodPr__58E7D564] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVS_UsrMeta] ADD CONSTRAINT [FK__IVS_UsrMe__SeqUs__59DBF99D] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

