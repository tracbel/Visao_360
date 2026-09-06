/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo CLARITY
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.CLARITY_BINA
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[CLARITY_BINA] (
    [RAMAL] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TELEFONE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_EXTERNO] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_LIGACAO] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO
CREATE NONCLUSTERED INDEX [IDX_CLARITY_BINA_0] ON [dbo].[CLARITY_BINA] ([RAMAL]);
GO

