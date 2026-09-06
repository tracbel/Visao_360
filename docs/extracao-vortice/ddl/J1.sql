/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo J1
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.J1_PRODUTO
   Criada em ..: 2018-09-04
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[J1_PRODUTO] (
    [SEQPRODUTO] numeric(18,0) NOT NULL,
    [DESTINO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [USR] numeric(8,0) NOT NULL,
    [FAMILIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODPRODUTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO] numeric(15,2) NULL,
    [EMUSO] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__J1_PRODU__4C136A08F3FEAF76] PRIMARY KEY CLUSTERED ([SEQPRODUTO], [DESTINO], [USR])
);
GO
CREATE NONCLUSTERED INDEX [J1_PRODUTO_IDX1] ON [dbo].[J1_PRODUTO] ([DESTINO], [USR]) INCLUDE ([SEQPRODUTO], [FAMILIA], [MARCA], [CODPRODUTO], [DESCRICAO], [PRECO], [EMUSO]);
GO

