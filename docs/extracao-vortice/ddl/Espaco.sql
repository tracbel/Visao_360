/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo Espaco
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.Espaco_Tabelas
   Criada em ..: 2025-06-09
   Alterada em : 2025-06-09
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[Espaco_Tabelas] (
    [NomeTabela] nvarchar(128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NumLinhas] int NULL,
    [Reservado] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dados] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Indice] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Livre] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
);
GO

