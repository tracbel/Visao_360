/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo CBR
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.CBR_CLIENTECONTA
   Criada em ..: 2020-11-16
   Alterada em : 2020-11-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[CBR_CLIENTECONTA] (
    [SEQCBRCLIENTECONTA] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [IDENTIFICADOR] numeric(18,0) NOT NULL,
    [CPF] numeric(18,0) NULL,
    [STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DIASATRASO] numeric(6,0) NULL,
    [SALDOATUAL] numeric(14,2) NULL,
    [DTAGERACAO] datetime NULL,
    CONSTRAINT [PK__CBR_CLIE__29303D1F283A142C] PRIMARY KEY CLUSTERED ([SEQCBRCLIENTECONTA])
);
GO
CREATE NONCLUSTERED INDEX [IDX_CBR_CLIENTECONTA_CPF] ON [dbo].[CBR_CLIENTECONTA] ([CPF]);
GO

