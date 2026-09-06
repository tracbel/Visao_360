/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IVF
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Acordo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Acordo] (
    [SeqAcordo] numeric(18,0) NOT NULL,
    [SeqTabela] numeric(18,0) NOT NULL,
    [Prazo] decimal(4,0) NOT NULL,
    [VlrProducao] decimal(15,2) NOT NULL,
    [SimulaProdOk] numeric(1,0) NULL,
    [PercPlus] decimal(4,2) NOT NULL,
    [VlrPlus] decimal(15,2) NOT NULL,
    [EmUso] numeric(1,0) NOT NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAcordo] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_Acordo__5150D53D] PRIMARY KEY CLUSTERED ([SeqAcordo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_AcordoPK] ON [dbo].[IVF_Acordo] ([SeqAcordo]);
GO
CREATE NONCLUSTERED INDEX [IVF_AcordoIF2] ON [dbo].[IVF_Acordo] ([Prazo]);
GO
CREATE NONCLUSTERED INDEX [IVF_AcordoIF3] ON [dbo].[IVF_Acordo] ([SeqTabela]);
GO
ALTER TABLE [dbo].[IVF_Acordo] ADD CONSTRAINT [FK__IVF_Acord__Prazo__67C004A0] FOREIGN KEY ([Prazo]) REFERENCES [dbo].[IVF_Prazo] ([Prazo]);
GO
ALTER TABLE [dbo].[IVF_Acordo] ADD CONSTRAINT [FK__IVF_Acord__Prazo__2DFD775F] FOREIGN KEY ([Prazo]) REFERENCES [dbo].[IVF_Prazo] ([Prazo]);
GO
ALTER TABLE [dbo].[IVF_Acordo] ADD CONSTRAINT [FK__IVF_Acord__SeqTa__2EF19B98] FOREIGN KEY ([SeqTabela]) REFERENCES [dbo].[IVF_Tabela] ([SeqTabela]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Agregado
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Agregado] (
    [SeqAgreg] numeric(18,0) NOT NULL,
    [SeqTpAgreg] decimal(4,0) NOT NULL,
    [SubTipoAgreg] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [SeqProposta] numeric(18,0) NULL,
    [SeqPessoaRec] numeric(8,0) NOT NULL,
    [Vlr] decimal(15,2) NOT NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_Agregado__5244F976] PRIMARY KEY CLUSTERED ([SeqAgreg])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_AgregadoPK] ON [dbo].[IVF_Agregado] ([SeqAgreg]);
GO
CREATE NONCLUSTERED INDEX [IVF_AgregadoIF2] ON [dbo].[IVF_Agregado] ([SeqTpAgreg]);
GO
CREATE NONCLUSTERED INDEX [IVF_AgregadoIF4] ON [dbo].[IVF_Agregado] ([SeqPessoaRec], [SeqTpAgreg]);
GO
CREATE NONCLUSTERED INDEX [IVF_AgregadoIF5] ON [dbo].[IVF_Agregado] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IVF_AGREGADOIF1] ON [dbo].[IVF_Agregado] ([Processo]);
GO
ALTER TABLE [dbo].[IVF_Agregado] ADD CONSTRAINT [FK__IVF_Agreg__SeqTp__2FE5BFD1] FOREIGN KEY ([SeqTpAgreg]) REFERENCES [dbo].[IVF_TipoAgregado] ([SeqTpAgreg]);
GO
ALTER TABLE [dbo].[IVF_Agregado] ADD CONSTRAINT [FK__IVF_Agregado__30D9E40A] FOREIGN KEY ([SeqTpAgreg], [SeqPessoaRec]) REFERENCES [dbo].[IVF_TpAgrPessoa] ([SeqTpAgreg], [SeqPessoa]);
GO
ALTER TABLE [dbo].[IVF_Agregado] ADD CONSTRAINT [FK__IVF_Agreg__Proce__31CE0843] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_FichaNegVeic] ([Processo]);
GO
ALTER TABLE [dbo].[IVF_Agregado] ADD CONSTRAINT [FK__IVF_Agregado__6A9C714B] FOREIGN KEY ([SeqTpAgreg], [SeqPessoaRec]) REFERENCES [dbo].[IVF_TpAgrPessoa] ([SeqTpAgreg], [SeqPessoa]);
GO
ALTER TABLE [dbo].[IVF_Agregado] ADD CONSTRAINT [FK__IVF_Agreg__Proce__6B909584] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_FichaNegVeic] ([Processo]);
GO
ALTER TABLE [dbo].[IVF_Agregado] ADD CONSTRAINT [FK__IVF_Agreg__SeqTp__69A84D12] FOREIGN KEY ([SeqTpAgreg]) REFERENCES [dbo].[IVF_TipoAgregado] ([SeqTpAgreg]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_AgregCC
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_AgregCC] (
    [SeqAgregCC] numeric(18,0) NOT NULL,
    [SeqAgreg] numeric(18,0) NOT NULL,
    [Real] numeric(1,0) NULL,
    [Vlr] decimal(15,2) NULL,
    [DtaLancto] datetime NULL,
    [DtaLanctoPlan] datetime NULL,
    [VlrPlan] decimal(15,2) NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_AgregCC__53391DAF] PRIMARY KEY CLUSTERED ([SeqAgregCC])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_AgregCCPK] ON [dbo].[IVF_AgregCC] ([SeqAgregCC]);
GO
CREATE NONCLUSTERED INDEX [IVF_AgregCCIF1] ON [dbo].[IVF_AgregCC] ([SeqAgreg]);
GO
CREATE NONCLUSTERED INDEX [XIF1IVF_AgregCC] ON [dbo].[IVF_AgregCC] ([SeqAgreg]);
GO
ALTER TABLE [dbo].[IVF_AgregCC] ADD CONSTRAINT [FK__IVF_Agreg__SeqAg__32C22C7C] FOREIGN KEY ([SeqAgreg]) REFERENCES [dbo].[IVF_Agregado] ([SeqAgreg]);
GO
ALTER TABLE [dbo].[IVF_AgregCC] ADD CONSTRAINT [FK__IVF_Agreg__SeqAg__6C84B9BD] FOREIGN KEY ([SeqAgreg]) REFERENCES [dbo].[IVF_Agregado] ([SeqAgreg]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Financeira
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Financeira] (
    [SeqFinanc] decimal(6,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [EmUso] numeric(1,0) NULL,
    [Financeira] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AnoReferencia] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VlrTCNovo] decimal(15,2) NULL,
    [VlrTCUsado] decimal(15,2) NULL,
    [VlrTCRetNovo] decimal(15,2) NULL,
    [VlrTCRetUsado] decimal(15,2) NULL,
    [VlrTCMinNovo] decimal(15,2) NULL,
    [VlrTCMinUsado] decimal(15,2) NULL,
    [RetMax] decimal(2,0) NULL,
    [IndPref] decimal(1,0) NULL,
    [IndPrefUsado] decimal(1,0) NULL,
    [DevolveTCPJ] numeric(1,0) NULL,
    [MinutoMaxResp] decimal(4,0) NULL,
    [PercSegPrestam] decimal(6,4) NULL,
    [VlrSegPrestam] decimal(8,2) NULL,
    [IndSemSegPrest] numeric(1,0) NULL,
    [Banco] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndUsaILA] numeric(1,0) NULL,
    [IndSegPrestExt] numeric(1,0) NULL,
    [TipoSegGarExt] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_Financeira__542D41E8] PRIMARY KEY CLUSTERED ([SeqFinanc])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_FinanceiraPK] ON [dbo].[IVF_Financeira] ([SeqFinanc]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_FinanceiraAK1] ON [dbo].[IVF_Financeira] ([Financeira]);
GO
CREATE NONCLUSTERED INDEX [IVF_FinanceiraIF1] ON [dbo].[IVF_Financeira] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IVF_Financeira] ADD CONSTRAINT [FK__IVF_Finan__SeqPe__33B650B5] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IVF_Financeira] ADD CONSTRAINT [FK__IVF_Finan__SeqPe__6D78DDF6] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_FinancEmpr
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_FinancEmpr] (
    [SeqFinanc] decimal(6,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [IndUsaILARet] numeric(1,0) NULL,
    [IndUsaILAAcor] numeric(1,0) NULL,
    CONSTRAINT [PK__IVF_FinancEmpr__55216621] PRIMARY KEY CLUSTERED ([SeqFinanc], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_FinancEmprPK] ON [dbo].[IVF_FinancEmpr] ([SeqFinanc], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IVF_FinancEmprIF1] ON [dbo].[IVF_FinancEmpr] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IVF_FinancEmpr] ADD CONSTRAINT [FK__IVF_Finan__SeqFi__34AA74EE] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_FinancImpTx
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_FinancImpTx] (
    [SeqFinanc] numeric(6,0) NOT NULL,
    [LayOut] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ColDestino] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Coluna] numeric(4,0) NULL,
    [Formula] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Separador] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_FinancImpTx__3D89D3B2] PRIMARY KEY CLUSTERED ([SeqFinanc], [LayOut], [ColDestino])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IVF_FinancImpTx] ON [dbo].[IVF_FinancImpTx] ([SeqFinanc]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Molicar
   Criada em ..: 2013-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Molicar] (
    [SeqMolicar] numeric(18,0) NOT NULL,
    [CodMolicar] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodMarca] decimal(3,0) NULL,
    [Marca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodModelo] decimal(4,0) NULL,
    [Modelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodVersao] decimal(3,0) NULL,
    [Versao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodConfiguracao] decimal(2,0) NULL,
    [Configuracao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodCombustivel] decimal(1,0) NULL,
    [Combustivel] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Porte] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodCategoria] decimal(1,0) NULL,
    [Categoria] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cambio] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Portas] decimal(1,0) NULL,
    [QtdePassageiros] decimal(2,0) NULL,
    [Peso] decimal(8,2) NULL,
    [Carga] decimal(8,2) NULL,
    [Motor] decimal(5,0) NULL,
    [CV] decimal(3,0) NULL,
    [AnoInicioFabric] decimal(4,0) NULL,
    [AnoFimFabric] decimal(4,0) NULL,
    CONSTRAINT [PK__IVF_Molicar__56158A5A] PRIMARY KEY CLUSTERED ([SeqMolicar])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_MolicarPK] ON [dbo].[IVF_Molicar] ([SeqMolicar]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_MolicarAK1] ON [dbo].[IVF_Molicar] ([CodMolicar]);
GO
CREATE NONCLUSTERED INDEX [IVF_MolicarIE1] ON [dbo].[IVF_Molicar] ([Marca]);
GO
CREATE NONCLUSTERED INDEX [IVF_MolicarIE2] ON [dbo].[IVF_Molicar] ([Modelo]);
GO
CREATE NONCLUSTERED INDEX [IVF_MolicarIE3] ON [dbo].[IVF_Molicar] ([CodModelo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Plano
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Plano] (
    [SeqPlano] numeric(18,0) NOT NULL,
    [SeqTabela] numeric(18,0) NOT NULL,
    [Prazo] decimal(4,0) NOT NULL,
    [PercMinEntrada] decimal(4,2) NOT NULL,
    [Coeficiente] decimal(10,7) NOT NULL,
    [Taxa] decimal(6,4) NOT NULL,
    [EmUso] numeric(1,0) NOT NULL,
    [Coef01] decimal(10,7) NULL,
    [Coef02] decimal(10,7) NULL,
    [Coef03] decimal(10,7) NULL,
    [Coef04] decimal(10,7) NULL,
    [Coef05] decimal(10,7) NULL,
    [Coef06] decimal(10,7) NULL,
    [Coef07] decimal(10,7) NULL,
    [Coef08] decimal(10,7) NULL,
    [Coef09] decimal(10,7) NULL,
    [Coef10] decimal(10,7) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IVF_Plano__5709AE93] PRIMARY KEY CLUSTERED ([SeqPlano])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_PlanoPK] ON [dbo].[IVF_Plano] ([SeqPlano]);
GO
CREATE NONCLUSTERED INDEX [IVF_PlanoIE1] ON [dbo].[IVF_Plano] ([EmUso]);
GO
CREATE NONCLUSTERED INDEX [IVF_PlanoIF2] ON [dbo].[IVF_Plano] ([Prazo]);
GO
CREATE NONCLUSTERED INDEX [IVF_PlanoIF3] ON [dbo].[IVF_Plano] ([SeqTabela]);
GO
ALTER TABLE [dbo].[IVF_Plano] ADD CONSTRAINT [FK__IVF_Plano__Prazo__359E9927] FOREIGN KEY ([Prazo]) REFERENCES [dbo].[IVF_Prazo] ([Prazo]);
GO
ALTER TABLE [dbo].[IVF_Plano] ADD CONSTRAINT [FK__IVF_Plano__SeqTa__3692BD60] FOREIGN KEY ([SeqTabela]) REFERENCES [dbo].[IVF_Tabela] ([SeqTabela]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVF_Plano] ADD CONSTRAINT [FK__IVF_Plano__Prazo__6F612668] FOREIGN KEY ([Prazo]) REFERENCES [dbo].[IVF_Prazo] ([Prazo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_PlanoIndic
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_PlanoIndic] (
    [SeqPlanoIndic] numeric(18,0) NOT NULL,
    [Processo] numeric(18,0) NOT NULL,
    [TipoIndic] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqFinanc] decimal(6,0) NULL,
    [SeqTabela] numeric(18,0) NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_PlanoIndic__57FDD2CC] PRIMARY KEY CLUSTERED ([SeqPlanoIndic])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_PlanoIndicPK] ON [dbo].[IVF_PlanoIndic] ([SeqPlanoIndic]);
GO
CREATE NONCLUSTERED INDEX [IVF_PlanoIndicIF1] ON [dbo].[IVF_PlanoIndic] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IVF_PlanoIndicIF3] ON [dbo].[IVF_PlanoIndic] ([SeqFinanc]);
GO
CREATE NONCLUSTERED INDEX [IVF_PlanoIndicIF4] ON [dbo].[IVF_PlanoIndic] ([SeqTabela]);
GO
ALTER TABLE [dbo].[IVF_PlanoIndic] ADD CONSTRAINT [FK__IVF_Plano__Proce__3786E199] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_FichaNegVeic] ([Processo]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IVF_PlanoIndic] ADD CONSTRAINT [FK__IVF_Plano__SeqFi__387B05D2] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IVF_PlanoIndic] ADD CONSTRAINT [FK__IVF_Plano__SeqFi__723D9313] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IVF_PlanoIndic] ADD CONSTRAINT [FK__IVF_Plano__SeqTa__7331B74C] FOREIGN KEY ([SeqTabela]) REFERENCES [dbo].[IVF_Tabela] ([SeqTabela]);
GO
ALTER TABLE [dbo].[IVF_PlanoIndic] ADD CONSTRAINT [FK__IVF_Plano__SeqTa__396F2A0B] FOREIGN KEY ([SeqTabela]) REFERENCES [dbo].[IVF_Tabela] ([SeqTabela]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Prazo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Prazo] (
    [Prazo] decimal(4,0) NOT NULL,
    [Descricao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    CONSTRAINT [PK__IVF_Prazo__58F1F705] PRIMARY KEY CLUSTERED ([Prazo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_PrazoPK] ON [dbo].[IVF_Prazo] ([Prazo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Proposta
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Proposta] (
    [SeqProposta] numeric(18,0) NOT NULL,
    [ProcessoFN] numeric(18,0) NULL,
    [Processo] numeric(18,0) NULL,
    [SequenciaEnvio] decimal(2,0) NULL,
    [SeqFinanc] decimal(6,0) NOT NULL,
    [SeqPlano] numeric(18,0) NOT NULL,
    [Agente] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Veiculo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodModelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AnoModelo] decimal(4,0) NULL,
    [AnoFabric] decimal(4,0) NULL,
    [VlrTotalBem] decimal(15,2) NULL,
    [VlrFinanciado] decimal(15,2) NULL,
    [VlrTC] decimal(15,2) NULL,
    [VlrParcela] decimal(15,2) NULL,
    [VlrRetorno] decimal(15,2) NULL,
    [VlrRetornoTC] decimal(15,2) NULL,
    [VlrPlus] decimal(15,2) NULL,
    [VlrOutro] decimal(15,2) NULL,
    [Coeficiente] decimal(10,7) NULL,
    [Prazo] decimal(4,0) NULL,
    [Retorno] decimal(2,0) NULL,
    [DtaGeracao] datetime NULL,
    [DtaEnvioMesa] datetime NULL,
    [DtaEnvioFin] datetime NULL,
    [DtaRespostaFin] datetime NULL,
    [DtaConfirmacao] datetime NULL,
    [DtaUltTransacao] datetime NULL,
    [DtaRecebimento] datetime NULL,
    [IndUltAnalise] numeric(1,0) NULL,
    [IndRecebida] numeric(1,0) NULL,
    [IndDefinida] numeric(1,0) NULL,
    [IndAvaliadaMesa] numeric(1,0) NULL,
    [IndAvaliadaFinanc] numeric(1,0) NULL,
    [IndAprovada] numeric(1,0) NULL,
    [IndRecusada] numeric(1,0) NULL,
    [IndDescartada] numeric(1,0) NULL,
    [IndVendaPerdida] numeric(1,0) NULL,
    [IndUtilizada] numeric(1,0) NULL,
    [IndReanaliseMesa] numeric(1,0) NULL,
    [IndResgTempo] numeric(1,0) NULL,
    [Situacao] decimal(2,0) NOT NULL,
    [MotivoRecusa] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SolicDocAdicional] numeric(1,0) NULL,
    [SolicAvalista] numeric(1,0) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsInterna] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrAlocado] decimal(8,0) NULL,
    [IndEmTrabalho] numeric(1,0) NULL,
    [DtaEmTrabalho] datetime NULL,
    [DtaProxAnalise] datetime NULL,
    [IndIgnoraTempoFin] numeric(1,0) NULL,
    [VlrSegPrest] decimal(15,2) NULL,
    [DtaAlteracao] datetime NULL,
    [VlrResultado] decimal(10,2) NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VlrPerda] decimal(8,2) NULL,
    [SeqPlanoMelhor] numeric(18,0) NULL,
    [VlrSegGarExt] numeric(14,2) NULL,
    [VlrPlusPerform] numeric(14,2) NULL,
    CONSTRAINT [PK__IVF_Proposta__59E61B3E] PRIMARY KEY CLUSTERED ([SeqProposta])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_PropostaPK] ON [dbo].[IVF_Proposta] ([SeqProposta]);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaIE1] ON [dbo].[IVF_Proposta] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaIE2] ON [dbo].[IVF_Proposta] ([Situacao]);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaIF2] ON [dbo].[IVF_Proposta] ([SeqPlano]);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaIF3] ON [dbo].[IVF_Proposta] ([SeqFinanc]);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaIF4] ON [dbo].[IVF_Proposta] ([ProcessoFN]);
GO
CREATE NONCLUSTERED INDEX [XIF4IVF_Proposta] ON [dbo].[IVF_Proposta] ([SeqPlanoMelhor]);
GO
ALTER TABLE [dbo].[IVF_Proposta] ADD CONSTRAINT [FK__IVF_Propo__Proce__760E23F7] FOREIGN KEY ([ProcessoFN]) REFERENCES [dbo].[IV_FichaNegVeic] ([Processo]);
GO
ALTER TABLE [dbo].[IVF_Proposta] ADD CONSTRAINT [FK__IVF_Propo__SeqPl__44D5EFBF] FOREIGN KEY ([SeqPlanoMelhor]) REFERENCES [dbo].[IVF_Plano] ([SeqPlano]) ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[IVF_Proposta] ADD CONSTRAINT [FK__IVF_Propo__Proce__3C4B96B6] FOREIGN KEY ([ProcessoFN]) REFERENCES [dbo].[IV_FichaNegVeic] ([Processo]);
GO
ALTER TABLE [dbo].[IVF_Proposta] ADD CONSTRAINT [FK__IVF_Propo__SeqPl__3A634E44] FOREIGN KEY ([SeqPlano]) REFERENCES [dbo].[IVF_Plano] ([SeqPlano]);
GO
ALTER TABLE [dbo].[IVF_Proposta] ADD CONSTRAINT [FK__IVF_Propo__SeqFi__3B57727D] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IVF_Proposta] ADD CONSTRAINT [FK__IVF_Propo__SeqPl__7425DB85] FOREIGN KEY ([SeqPlano]) REFERENCES [dbo].[IVF_Plano] ([SeqPlano]);
GO
ALTER TABLE [dbo].[IVF_Proposta] ADD CONSTRAINT [FK__IVF_Propo__SeqFi__7519FFBE] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_PropostaHst
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_PropostaHst] (
    [SeqProposta] numeric(18,0) NOT NULL,
    [Situacao] decimal(2,0) NOT NULL,
    [Detalhe] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HU] decimal(8,2) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [SEQPROPOSTAHST] numeric(18,0) IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK__IVF_Prop__FEE0FE1D7A9BB2E6] PRIMARY KEY CLUSTERED ([SEQPROPOSTAHST])
);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaHstIF1] ON [dbo].[IVF_PropostaHst] ([SeqProposta]);
GO
ALTER TABLE [dbo].[IVF_PropostaHst] ADD CONSTRAINT [FK__IVF_Propo__SeqPr__3D3FBAEF] FOREIGN KEY ([SeqProposta]) REFERENCES [dbo].[IVF_Proposta] ([SeqProposta]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_PropostaObs
   Criada em ..: 2012-03-28
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_PropostaObs] (
    [SeqProposta] numeric(18,0) NOT NULL,
    [SeqObs] numeric(4,0) NOT NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] numeric(2,0) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IVF_PropostaObs__5BCE63B0] PRIMARY KEY CLUSTERED ([SeqProposta], [SeqObs])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_PropostaObsPK] ON [dbo].[IVF_PropostaObs] ([SeqProposta], [SeqObs]);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaObsIF1] ON [dbo].[IVF_PropostaObs] ([SeqProposta]);
GO
ALTER TABLE [dbo].[IVF_PropostaObs] ADD CONSTRAINT [FK__IVF_Propo__SeqPr__3E33DF28] FOREIGN KEY ([SeqProposta]) REFERENCES [dbo].[IVF_Proposta] ([SeqProposta]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_PropostaResult
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_PropostaResult] (
    [SeqProposta] numeric(18,0) NOT NULL,
    [SeqPropResult] decimal(2,0) NOT NULL,
    [Tipo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SubTipo] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vlr] decimal(15,2) NULL,
    CONSTRAINT [PK__IVF_PropostaResu__5CC287E9] PRIMARY KEY CLUSTERED ([SeqProposta], [SeqPropResult])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_PropostaResPK] ON [dbo].[IVF_PropostaResult] ([SeqProposta], [SeqPropResult]);
GO
CREATE NONCLUSTERED INDEX [IVF_PropostaResIF1] ON [dbo].[IVF_PropostaResult] ([SeqProposta]);
GO
ALTER TABLE [dbo].[IVF_PropostaResult] ADD CONSTRAINT [FK__IVF_Propo__SeqPr__3F280361] FOREIGN KEY ([SeqProposta]) REFERENCES [dbo].[IVF_Proposta] ([SeqProposta]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_Tabela
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_Tabela] (
    [SeqTabela] numeric(18,0) NOT NULL,
    [Tabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqFinanc] decimal(6,0) NOT NULL,
    [UsaPJ] numeric(1,0) NULL,
    [UsaPF] numeric(1,0) NULL,
    [TipoPlano] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NovoUsado] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [AnoDe] decimal(4,0) NOT NULL,
    [AnoA] decimal(4,0) NOT NULL,
    [IndPlanoEspecial] numeric(1,0) NULL,
    [IndFiltro] numeric(1,0) NULL,
    [VigorDe] datetime NOT NULL,
    [VigorA] datetime NOT NULL,
    [VlrTC] decimal(15,2) NULL,
    [VlrTCRet] decimal(15,2) NULL,
    [VlrTCMin] decimal(15,2) NULL,
    [RetMax] decimal(2,0) NULL,
    [RestrEmpr] numeric(1,0) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Alerta] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndUsoRestrito] numeric(1,0) NULL,
    [CodTabela] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_Tabela__5DB6AC22] PRIMARY KEY CLUSTERED ([SeqTabela])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_TabelaPK] ON [dbo].[IVF_Tabela] ([SeqTabela]);
GO
CREATE NONCLUSTERED INDEX [IVF_TabelaIF1] ON [dbo].[IVF_Tabela] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IVF_Tabela] ADD CONSTRAINT [FK__IVF_Tabel__SeqFi__79DEB4DB] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IVF_Tabela] ADD CONSTRAINT [FK__IVF_Tabel__SeqFi__401C279A] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_TabelaFiltro
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_TabelaFiltro] (
    [SeqTabela] numeric(18,0) NOT NULL,
    [SeqFiltro] decimal(6,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_TabelaFiltro__5EAAD05B] PRIMARY KEY CLUSTERED ([SeqTabela], [SeqFiltro])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_TabelaFiltrPK] ON [dbo].[IVF_TabelaFiltro] ([SeqTabela], [SeqFiltro]);
GO
CREATE NONCLUSTERED INDEX [IVF_TabelaFiltrIE1] ON [dbo].[IVF_TabelaFiltro] ([SeqFiltro]);
GO
CREATE NONCLUSTERED INDEX [IVF_TabelaFiltrIF1] ON [dbo].[IVF_TabelaFiltro] ([SeqTabela]);
GO
ALTER TABLE [dbo].[IVF_TabelaFiltro] ADD CONSTRAINT [FK__IVF_Tabel__SeqTa__41104BD3] FOREIGN KEY ([SeqTabela]) REFERENCES [dbo].[IVF_Tabela] ([SeqTabela]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_TabEmpr
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_TabEmpr] (
    [SeqTabela] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IVF_TabEmpr__5F9EF494] PRIMARY KEY CLUSTERED ([SeqTabela], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_TabEmprPK] ON [dbo].[IVF_TabEmpr] ([SeqTabela], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IVF_TabEmprIF1] ON [dbo].[IVF_TabEmpr] ([SeqTabela]);
GO
ALTER TABLE [dbo].[IVF_TabEmpr] ADD CONSTRAINT [FK__IVF_TabEm__SeqTa__4204700C] FOREIGN KEY ([SeqTabela]) REFERENCES [dbo].[IVF_Tabela] ([SeqTabela]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_TabIndice
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_TabIndice] (
    [TipoIndice] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [VigorDe] datetime NOT NULL,
    [VigorA] datetime NOT NULL,
    [Indice] decimal(10,7) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IVF_TabIndice__609318CD] PRIMARY KEY CLUSTERED ([TipoIndice], [VigorDe], [VigorA])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_TabIndicePK] ON [dbo].[IVF_TabIndice] ([TipoIndice], [VigorDe], [VigorA]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_TipoAgregado
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_TipoAgregado] (
    [SeqTpAgreg] decimal(4,0) NOT NULL,
    [Agregado] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Grupo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [EmUso] numeric(1,0) NOT NULL,
    [PermEmbutir] numeric(1,0) NULL,
    [PermValorZero] numeric(1,0) NULL,
    CONSTRAINT [PK__IVF_TipoAgregado__61873D06] PRIMARY KEY CLUSTERED ([SeqTpAgreg])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_TipoAgregadPK] ON [dbo].[IVF_TipoAgregado] ([SeqTpAgreg]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_TipoAgregadAK1] ON [dbo].[IVF_TipoAgregado] ([Agregado]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IVF_TpAgrPessoa
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IVF_TpAgrPessoa] (
    [SeqTpAgreg] decimal(4,0) NOT NULL,
    [SeqPessoa] numeric(8,0) NOT NULL,
    [BaseFechamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DiasPgto] decimal(2,0) NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IVF_TpAgrPessoa__627B613F] PRIMARY KEY CLUSTERED ([SeqTpAgreg], [SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IVF_TpAgrPessoaPK] ON [dbo].[IVF_TpAgrPessoa] ([SeqTpAgreg], [SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IVF_TpAgrPessoaIF1] ON [dbo].[IVF_TpAgrPessoa] ([SeqTpAgreg]);
GO
CREATE NONCLUSTERED INDEX [IVF_TpAgrPessoaIF2] ON [dbo].[IVF_TpAgrPessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IVF_TpAgrPessoa] ADD CONSTRAINT [FK__IVF_TpAgr__SeqTp__7CBB2186] FOREIGN KEY ([SeqTpAgreg]) REFERENCES [dbo].[IVF_TipoAgregado] ([SeqTpAgreg]);
GO
ALTER TABLE [dbo].[IVF_TpAgrPessoa] ADD CONSTRAINT [FK__IVF_TpAgr__SeqTp__42F89445] FOREIGN KEY ([SeqTpAgreg]) REFERENCES [dbo].[IVF_TipoAgregado] ([SeqTpAgreg]);
GO

