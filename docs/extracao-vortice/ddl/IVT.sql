/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IVT
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVT_DePara
   Criada em ..: 2014-12-23
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVT_DePara] (
    [SEQ] numeric(18,0) NOT NULL,
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Nr1De] numeric(18,0) NULL,
    [Nr1Para] numeric(18,0) NOT NULL,
    [Nr2PAra] numeric(18,0) NULL,
    [Nr2De] numeric(18,0) NULL,
    [Obs] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ativo] numeric(1,0) NULL,
    CONSTRAINT [PK__IVT_DePara__41F98314] PRIMARY KEY CLUSTERED ([SEQ])
);
GO

