/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo mig
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.mig_ge_pessoa
   Criada em ..: 2024-12-27
   Alterada em : 2024-12-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[mig_ge_pessoa] (
    [seqpessoa] decimal(12,0) NULL,
    [descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.mig_ge_pessoa_bkp
   Criada em ..: 2024-12-30
   Alterada em : 2024-12-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[mig_ge_pessoa_bkp] (
    [seqpessoa] decimal(12,0) NULL,
    [descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

