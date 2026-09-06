/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo LOG
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.LOG_INTEGRACAO_FATURAMENTO_TOTVS
   Criada em ..: 2023-11-22
   Alterada em : 2023-11-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[LOG_INTEGRACAO_FATURAMENTO_TOTVS] (
    [ID] int NOT NULL,
    [Tabela] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MensagemErro] nvarchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DataHora] datetime NULL,
    CONSTRAINT [LOG_INTEGRACAO_FATURAMENTO_TOTVS_PK] PRIMARY KEY CLUSTERED ([ID])
);
GO

