/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo JDE
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.JDE_CATEGORY
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[JDE_CATEGORY] (
    [categoryId] numeric(18,0) NOT NULL,
    [categoryDescription] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__JDE_CATE__23CAF1D8ABCDE4C3] PRIMARY KEY CLUSTERED ([categoryId])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.JDE_EQUIPAMENTS
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[JDE_EQUIPAMENTS] (
    [EQUIPAMENT_ID] numeric(18,0) NOT NULL,
    [categoryId] numeric(18,0) NULL,
    [subCategoryId] numeric(18,0) NULL,
    [costPrice] decimal(10,4) NULL,
    [listPrice] decimal(10,4) NULL,
    [machineHours] decimal(10,4) NULL,
    [makeName] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [modelName] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [serialNumber] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [equip_status] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [equip_year] numeric(18,0) NULL,
    [storeLocation] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [totalAttachmentAccMa] decimal(10,4) NULL,
    [totalAttachmentAccSe] decimal(10,4) NULL,
    [totalDealerOptionMar] decimal(10,4) NULL,
    [totalDealerOptionSel] decimal(10,4) NULL,
    [totalEquipmentMargin] decimal(10,4) NULL,
    [totalEquipmentSellin] decimal(10,4) NULL,
    CONSTRAINT [PK__JDE_EQUI__31F503419C3E557D] PRIMARY KEY CLUSTERED ([EQUIPAMENT_ID])
);
GO
CREATE NONCLUSTERED INDEX [JDE_EQUIPAMENTS_FKIn] ON [dbo].[JDE_EQUIPAMENTS] ([subCategoryId]);
GO
CREATE NONCLUSTERED INDEX [JDE_EQUIPAMENTS_FKI2] ON [dbo].[JDE_EQUIPAMENTS] ([categoryId]);
GO
ALTER TABLE [dbo].[JDE_EQUIPAMENTS] ADD CONSTRAINT [FK__JDE_EQUIP__categ__391A2450] FOREIGN KEY ([categoryId]) REFERENCES [dbo].[JDE_CATEGORY] ([categoryId]);
GO
ALTER TABLE [dbo].[JDE_EQUIPAMENTS] ADD CONSTRAINT [FK__JDE_EQUIP__subCa__3A0E4889] FOREIGN KEY ([subCategoryId]) REFERENCES [dbo].[JDE_SUB_CATEGORY] ([subCategoryId]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.JDE_PURCHASE
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[JDE_PURCHASE] (
    [poNumber] numeric(18,0) NOT NULL,
    [orderDate] datetime NULL,
    [signedOnDate] datetime NULL,
    [transactionType] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [warrantyBeginsDate] datetime NULL,
    [marketUse] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ped_status] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [purchaserType] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [balanceDue] decimal(10,4) NULL,
    [fieldCashWithOrder] decimal(10,4) NULL,
    [fieldRentalApplied] decimal(10,4) NULL,
    [balance] decimal(10,4) NULL,
    [subTotal] decimal(10,4) NULL,
    [totalCashPrice] decimal(10,4) NULL,
    [totalTradeInAllowanc] decimal(10,4) NULL,
    [totalTradePayOff] decimal(10,4) NULL,
    [deliveredDate] datetime NULL,
    [SeqQuestionario] numeric(18,0) NULL,
    CONSTRAINT [PK__JDE_PURC__F78207A166D6DB35] PRIMARY KEY CLUSTERED ([poNumber])
);
GO
CREATE NONCLUSTERED INDEX [XIF1JDE_PURCHASE] ON [dbo].[JDE_PURCHASE] ([SeqQuestionario]);
GO
ALTER TABLE [dbo].[JDE_PURCHASE] ADD CONSTRAINT [FK__JDE_PURCH__SeqQu__3DDED96D] FOREIGN KEY ([SeqQuestionario]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.JDE_QUOTE
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[JDE_QUOTE] (
    [quoteId] numeric(18,0) NOT NULL,
    [deereUserId] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [balanceDue] decimal(10,4) NULL,
    [creationDate] datetime NULL,
    [expirationDate] datetime NULL,
    [lastModifiedDate] datetime NULL,
    [netCost] decimal(10,4) NULL,
    [netProceeds] decimal(10,4) NULL,
    [poNumber] numeric(18,0) NULL,
    [quoteName] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [quoteStatus] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [quoteType] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [taxAmount] decimal(10,4) NULL,
    [totalNetTradeValue] decimal(10,4) NULL,
    [tradeDifference] decimal(10,4) NULL,
    [downPayment] decimal(10,4) NULL,
    [SEQPESSOA] numeric(18,0) NULL,
    [agreementDate] datetime NULL,
    [signDate] datetime NULL,
    CONSTRAINT [PK__JDE_QUOT__C7709243A2901C8E] PRIMARY KEY CLUSTERED ([quoteId])
);
GO
CREATE NONCLUSTERED INDEX [JDE_COTACAO_FKIndex1] ON [dbo].[JDE_QUOTE] ([deereUserId]);
GO
ALTER TABLE [dbo].[JDE_QUOTE] ADD CONSTRAINT [FK__JDE_QUOTE__deere__3B026CC2] FOREIGN KEY ([deereUserId]) REFERENCES [dbo].[JDE_SALES_PERSON] ([deereUserId]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.JDE_QUOTE_ITEM
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[JDE_QUOTE_ITEM] (
    [quoteId] numeric(18,0) NOT NULL,
    [EQUIPAMENT_ID] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__JDE_QUOT__C46FC2772347354E] PRIMARY KEY CLUSTERED ([quoteId], [EQUIPAMENT_ID])
);
GO
CREATE NONCLUSTERED INDEX [JDE_QUOTE_ITEM_FKInd] ON [dbo].[JDE_QUOTE_ITEM] ([quoteId]);
GO
CREATE NONCLUSTERED INDEX [XIF2JDE_QUOTE_ITEM] ON [dbo].[JDE_QUOTE_ITEM] ([EQUIPAMENT_ID]);
GO
ALTER TABLE [dbo].[JDE_QUOTE_ITEM] ADD CONSTRAINT [FK__JDE_QUOTE__quote__3BF690FB] FOREIGN KEY ([quoteId]) REFERENCES [dbo].[JDE_QUOTE] ([quoteId]);
GO
ALTER TABLE [dbo].[JDE_QUOTE_ITEM] ADD CONSTRAINT [FK__JDE_QUOTE__EQUIP__3CEAB534] FOREIGN KEY ([EQUIPAMENT_ID]) REFERENCES [dbo].[JDE_EQUIPAMENTS] ([EQUIPAMENT_ID]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.JDE_SALES_PERSON
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[JDE_SALES_PERSON] (
    [deereUserId] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [firstName] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [lastName] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [middleName] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [emailAddress] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__JDE_SALE__E94D99DEB811F2B1] PRIMARY KEY CLUSTERED ([deereUserId])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.JDE_SUB_CATEGORY
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[JDE_SUB_CATEGORY] (
    [subCategoryId] numeric(18,0) NOT NULL,
    [subCategoryDescripti] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__JDE_SUB___F8206469F6090D6B] PRIMARY KEY CLUSTERED ([subCategoryId])
);
GO

