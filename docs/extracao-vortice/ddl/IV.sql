/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo IV
   Gerado em 2026-09-02 23:03 a partir dos catalogos do SQL Server (somente leitura).
   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Acao
   Criada em ..: 2011-12-19
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Acao] (
    [Acao] decimal(6,0) NOT NULL,
    [SeqFormulario] numeric(18,0) NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescReduzida] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Sigla] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Classe] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PermiteExclusao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ExigeResposta] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [EmUso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Avulsa] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeProduto] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeVendedor] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeMotivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeDepto] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeDetalhe] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeFormulario] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescPesJuridica] numeric(1,0) NULL,
    [PermiteReagendar] numeric(1,0) NULL,
    [ExigePrazo] numeric(1,0) NULL,
    [PrazoRealizacao] decimal(6,0) NULL,
    [PrazoMaxInicio] decimal(4,0) NULL,
    [PrazoMaxReag] numeric(8,0) NULL,
    [TarefaCompromisso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PermTrocaTarComp] numeric(1,0) NULL,
    [TempoMedio] decimal(4,0) NULL,
    [QtdeMaxPessoa] decimal(2,0) NULL,
    [QtdeMaxPessoaEmp] decimal(2,0) NULL,
    [MinutoAntesPerm] decimal(6,0) NULL,
    [Instrucao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [DutUltAgenda] decimal(4,0) NULL,
    [ExigeDtaLimite] numeric(1,0) NULL,
    [QtdeMaxProcesso] numeric(2,0) NULL,
    CONSTRAINT [PK__IV_Acao__61BC4730] PRIMARY KEY CLUSTERED ([Acao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AcaoPK] ON [dbo].[IV_Acao] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoIF1] ON [dbo].[IV_Acao] ([SeqFormulario]);
GO
ALTER TABLE [dbo].[IV_Acao] ADD CONSTRAINT [FK__IV_Acao__SeqForm__39A43435] FOREIGN KEY ([SeqFormulario]) REFERENCES [dbo].[IV_Formulario] ([SeqFormulario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ACAOANEXA
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ACAOANEXA] (
    [SEQACAOANEXA] numeric(18,0) NULL,
    [ACAO] numeric(6,0) NOT NULL,
    [ACAOANEXA] numeric(6,0) NOT NULL,
    [DESCRICAO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESTINATARIO] numeric(18,0) NOT NULL,
    [QTDEDIAS] numeric(4,0) NOT NULL,
    [QTDEHU] numeric(8,2) NULL,
    [INDINTERATIVO] numeric(1,0) NULL,
    [INDOBRIGATORIO] numeric(1,0) NULL,
    [INDATUALIZA] numeric(1,0) NULL,
    [INDRECRIA] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AcaoAtendente
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AcaoAtendente] (
    [Acao] decimal(6,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Qtdemaxdia] numeric(5,0) NULL,
    [Qtdemaxhora] numeric(5,0) NULL,
    [Qtdealertadia] numeric(5,0) NULL,
    [Qtdealertahora] numeric(5,0) NULL,
    CONSTRAINT [PK__IV_AcaoAtendente__62B06B69] PRIMARY KEY CLUSTERED ([Acao], [SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AcaoAtendentPK] ON [dbo].[IV_AcaoAtendente] ([Acao], [SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoAtendentIF1] ON [dbo].[IV_AcaoAtendente] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoAtendentIF2] ON [dbo].[IV_AcaoAtendente] ([Acao]);
GO
ALTER TABLE [dbo].[IV_AcaoAtendente] ADD CONSTRAINT [FK__IV_AcaoAt__SeqUs__3A98586E] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[IV_Operador] ([SeqUsuario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AcaoAtendente] ADD CONSTRAINT [FK__IV_AcaoAte__Acao__3B8C7CA7] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AcaoAuto
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AcaoAuto] (
    [SeqAcaoAuto] numeric(18,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Resultado] decimal(6,0) NOT NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Acao] decimal(6,0) NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [AssuntoCmpl] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Operacao] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsaObjDyn] numeric(1,0) NULL,
    [Exigida] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [GerarPara] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaGeracao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdeDias] numeric(4,0) NOT NULL,
    [QtdeHU] decimal(8,2) NULL,
    [Departamento] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AtivoReceptivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaPrimCont] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Prioridade] decimal(1,0) NULL,
    [SemConfirmacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAgendamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Mesmoprocesso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqOrder] decimal(4,0) NULL,
    [CHKSUM] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QuebraDNA] numeric(1,0) NULL,
    [EXIGESCOLHAATDGRPO] numeric(1,0) NULL,
    [INDGERAPARAPESPRC] numeric(1,0) NULL,
    [EMUSO] numeric(1,0) NULL,
    [PROPAGAFORMTRAB] numeric(1,0) NULL,
    [SOLICDATAAGD] numeric(1,0) NULL,
    [HORAAGENDA] datetime NULL,
    CONSTRAINT [PK__IV_AcaoAuto__63A48FA2] PRIMARY KEY CLUSTERED ([SeqAcaoAuto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AcaoAutoPK] ON [dbo].[IV_AcaoAuto] ([SeqAcaoAuto]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoAutoIF2] ON [dbo].[IV_AcaoAuto] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoAutoIF3] ON [dbo].[IV_AcaoAuto] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_AcaoAuto] ADD CONSTRAINT [FK__IV_AcaoAut__Acao__3C80A0E0] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AcaoAuto] ADD CONSTRAINT [FK__IV_AcaoAu__Resul__3D74C519] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AcaoAutoCtrl
   Criada em ..: 2013-09-23
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AcaoAutoCtrl] (
    [SeqAcaoAutoCtrl] numeric(8,0) NOT NULL,
    [Tipo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CtrlNum] numeric(18,0) NULL,
    [CtrlNum2] numeric(18,0) NULL,
    [CtrlStr] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqAcaoAuto] numeric(18,0) NOT NULL,
    CONSTRAINT [PK_IV_AcaoAutoCtrl] PRIMARY KEY CLUSTERED ([SeqAcaoAutoCtrl])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AcaoCmpl
   Criada em ..: 2017-04-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AcaoCmpl] (
    [Acao] numeric(6,0) NOT NULL,
    [AssuntoCmpl] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__IV_AcaoC__9FB531FB4D741C1F] PRIMARY KEY CLUSTERED ([Acao], [AssuntoCmpl])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_AcaoCmpl] ON [dbo].[IV_AcaoCmpl] ([Acao], [AssuntoCmpl]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_AcaoCmpl] ON [dbo].[IV_AcaoCmpl] ([Acao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AcaoCtrl
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AcaoCtrl] (
    [Acao] decimal(6,0) NOT NULL,
    [SeqCtrl] decimal(3,0) NOT NULL,
    [Base] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrBase] numeric(18,0) NULL,
    [Minuto] decimal(8,0) NULL,
    [Destinatario] numeric(18,0) NULL,
    [Atitude] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IntervaloExec] decimal(4,0) NULL,
    [DtaProxExec] datetime NULL,
    [TipoDest] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Papel] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqTxtPadrao] numeric(18,0) NULL,
    [Evento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_AcaoCtrl__6498B3DB] PRIMARY KEY CLUSTERED ([Acao], [SeqCtrl])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AcaoCtrlPK] ON [dbo].[IV_AcaoCtrl] ([Acao], [SeqCtrl]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoCtrlIF1] ON [dbo].[IV_AcaoCtrl] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_AcaoCtrl] ON [dbo].[IV_AcaoCtrl] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_AcaoCtrl] ADD CONSTRAINT [FK__IV_AcaoCtr__Acao__3E68E952] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AcaoCtrl] ADD CONSTRAINT [FK__IV_AcaoCt__SeqTx__54D74D5E] FOREIGN KEY ([SeqTxtPadrao]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AcaoMon
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AcaoMon] (
    [Acao] decimal(6,0) NOT NULL,
    [SeqAcMon] decimal(4,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [TpMon] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ResProd] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ResImprod] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ResEncer] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_AcaoMon__658CD814] PRIMARY KEY CLUSTERED ([Acao], [SeqAcMon])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AcaoMonPK] ON [dbo].[IV_AcaoMon] ([Acao], [SeqAcMon]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoMonIF1] ON [dbo].[IV_AcaoMon] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoMonIF2] ON [dbo].[IV_AcaoMon] ([Acao]);
GO
ALTER TABLE [dbo].[IV_AcaoMon] ADD CONSTRAINT [FK__IV_AcaoMo__SeqUs__791F9ACC] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IV_AcaoMon] ADD CONSTRAINT [FK__IV_AcaoMo__SeqUs__3F5D0D8B] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IV_AcaoMon] ADD CONSTRAINT [FK__IV_AcaoMon__Acao__405131C4] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AcaoRem
   Criada em ..: 2011-12-19
   Alterada em : 2019-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AcaoRem] (
    [SeqAcaoRem] numeric(18,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Resultado] decimal(6,0) NULL,
    [Acao] decimal(6,0) NULL,
    [NaoTrabalhada] numeric(1,0) NULL,
    [Interativo] numeric(1,0) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [INDREMPES] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_AcaoRem__6680FC4D] PRIMARY KEY CLUSTERED ([SeqAcaoRem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AcaoRemPK] ON [dbo].[IV_AcaoRem] ([SeqAcaoRem]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoRemIF1] ON [dbo].[IV_AcaoRem] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_AcaoRemIF2] ON [dbo].[IV_AcaoRem] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_AcaoRem] ADD CONSTRAINT [FK__IV_AcaoRem__Acao__414555FD] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AcaoRem] ADD CONSTRAINT [FK__IV_AcaoRe__Resul__42397A36] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ACAOURACENARIO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ACAOURACENARIO] (
    [ACAO] numeric(6,0) NOT NULL,
    [SEQURACENARIO] numeric(18,0) IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK__IV_ACAOU__06FE15783A5F3C83] PRIMARY KEY CLUSTERED ([ACAO])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AgdLink
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AgdLink] (
    [SeqAgenda] numeric(18,0) NOT NULL,
    [LinkOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [LinkTipo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkStr] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CtrlN1] numeric(18,0) NULL,
    [CtrlN2] numeric(18,0) NULL,
    [CtrlStr1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CtrlDta1] datetime NULL,
    [UltAtualizacao] datetime NULL,
    [OBS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_AgdLi__2C94937421555E39] PRIMARY KEY CLUSTERED ([SeqAgenda])
);
GO
CREATE NONCLUSTERED INDEX [IV_AGDLINKIE1] ON [dbo].[IV_AgdLink] ([LinkOrigem], [LinkTipo], [LinkNro]);
GO
CREATE NONCLUSTERED INDEX [IV_AGDLINKIE2] ON [dbo].[IV_AgdLink] ([LinkOrigem], [LinkTipo], [LinkStr]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AgdLinkPK] ON [dbo].[IV_AgdLink] ([SeqAgenda]);
GO
ALTER TABLE [dbo].[IV_AgdLink] ADD CONSTRAINT [FK__IV_AgdLin__SeqAg__415A6496] FOREIGN KEY ([SeqAgenda]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AGDPLANACAO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AGDPLANACAO] (
    [SEQAGDPLANACAO] numeric(8,0) NOT NULL,
    [SEQAGDPLANTRG] numeric(8,0) NOT NULL,
    [ACAO] numeric(6,0) NOT NULL,
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [DETALHEAGD] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDGRUPOTODO] numeric(1,0) NOT NULL,
    CONSTRAINT [PK__IV_AGDPL__58AFBC85D6C69B82] PRIMARY KEY CLUSTERED ([SEQAGDPLANACAO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_AGDPLANACAO_1] ON [dbo].[IV_AGDPLANACAO] ([SEQAGDPLANTRG]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_AGDPLANACAO_ACAO] ON [dbo].[IV_AGDPLANACAO] ([ACAO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_AGDPLANACAO_SEQUSUARIO] ON [dbo].[IV_AGDPLANACAO] ([SEQUSUARIO]);
GO
ALTER TABLE [dbo].[IV_AGDPLANACAO] ADD CONSTRAINT [FK__IV_AGDPLA__SEQAG__29398D99] FOREIGN KEY ([SEQAGDPLANTRG]) REFERENCES [dbo].[IV_AGDPLANTRG] ([SEQAGDPLANTRG]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AGDPLANTRG
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AGDPLANTRG] (
    [SEQAGDPLANTRG] numeric(8,0) NOT NULL,
    [EMUSO] numeric(1,0) NOT NULL,
    [DESCRICAO] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [RESUMOREGRA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDEEXECUCOES] numeric(4,0) NULL,
    [QTDDIASANTECIPADO] numeric(2,0) NOT NULL,
    [DTALIMITE] datetime NULL,
    [HORAINICIAL] numeric(4,0) NOT NULL,
    [DURACAO] numeric(4,0) NULL,
    [PRAZOMAXEXEC] numeric(4,0) NOT NULL,
    [TIPODIA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DIAESPECIAL] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDCOMPROMISSO] numeric(1,0) NOT NULL,
    [PROXIMODIA] numeric(1,0) NULL,
    [PRIORIDADE] numeric(1,0) NULL,
    [INDSABADOUTIL] numeric(1,0) NOT NULL,
    [INDDOMINGOUTIL] numeric(1,0) NOT NULL,
    [DIAMES01] numeric(2,0) NULL,
    [DIAMES02] numeric(2,0) NULL,
    [DIAMES03] numeric(2,0) NULL,
    [DIAMES04] numeric(2,0) NULL,
    [DIAMES05] numeric(2,0) NULL,
    [DOMINGO] numeric(1,0) NULL,
    [SEGUNDA] numeric(1,0) NULL,
    [TERCA] numeric(1,0) NULL,
    [QUARTA] numeric(1,0) NULL,
    [QUINTA] numeric(1,0) NULL,
    [SEXTA] numeric(1,0) NULL,
    [SABADO] numeric(1,0) NULL,
    [MJAN] numeric(1,0) NULL,
    [MFEV] numeric(1,0) NULL,
    [MMAR] numeric(1,0) NULL,
    [MABR] numeric(1,0) NULL,
    [MMAI] numeric(1,0) NULL,
    [MJUN] numeric(1,0) NULL,
    [MJUL] numeric(1,0) NULL,
    [MAGO] numeric(1,0) NULL,
    [MSET] numeric(1,0) NULL,
    [MOUT] numeric(1,0) NULL,
    [MNOV] numeric(1,0) NULL,
    [MDEZ] numeric(1,0) NULL,
    [ULTDTAGERADA] datetime NOT NULL,
    CONSTRAINT [PK__IV_AGDPL__AF488020D367C2D4] PRIMARY KEY CLUSTERED ([SEQAGDPLANTRG])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AgdRec
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AgdRec] (
    [SeqAgenda] numeric(18,0) NOT NULL,
    [SeqAgdRec] decimal(4,0) NOT NULL,
    [TipoAgdRec] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqRecurso] decimal(6,0) NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [DtaAgenda] datetime NOT NULL,
    [DtaAgendaFinal] datetime NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_AgdRec__686944BF] PRIMARY KEY CLUSTERED ([SeqAgenda], [SeqAgdRec])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AgdRecPK] ON [dbo].[IV_AgdRec] ([SeqAgenda], [SeqAgdRec]);
GO
CREATE NONCLUSTERED INDEX [IV_AgdRecIF1] ON [dbo].[IV_AgdRec] ([SeqAgenda]);
GO
CREATE NONCLUSTERED INDEX [IV_AgdRecIF2] ON [dbo].[IV_AgdRec] ([SeqRecurso]);
GO
CREATE NONCLUSTERED INDEX [IV_AgdRecIF3] ON [dbo].[IV_AgdRec] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_AgdRecIF4] ON [dbo].[IV_AgdRec] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IV_AgdRec] ADD CONSTRAINT [FK__IV_AgdRec__SeqAg__4421C2A8] FOREIGN KEY ([SeqAgenda]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AgdRec] ADD CONSTRAINT [FK__IV_AgdRec__SeqRe__4515E6E1] FOREIGN KEY ([SeqRecurso]) REFERENCES [dbo].[IV_Recurso] ([SeqRecurso]);
GO
ALTER TABLE [dbo].[IV_AgdRec] ADD CONSTRAINT [FK__IV_AgdRec__SeqPe__460A0B1A] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AgdRec] ADD CONSTRAINT [FK__IV_AgdRec__SeqUs__46FE2F53] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AgdRec] ADD CONSTRAINT [FK__IV_AgdRec__SeqRe__7ED87422] FOREIGN KEY ([SeqRecurso]) REFERENCES [dbo].[IV_Recurso] ([SeqRecurso]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AgdUsr
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AgdUsr] (
    [SeqAgenda] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [DtaUltGeracao] datetime NULL,
    [DtaLeitura] datetime NULL,
    CONSTRAINT [PK__IV_AgdUsr__695D68F8] PRIMARY KEY CLUSTERED ([SeqAgenda], [SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AgdUsrPK] ON [dbo].[IV_AgdUsr] ([SeqAgenda], [SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_AgdUsrIF1] ON [dbo].[IV_AgdUsr] ([SeqAgenda]);
GO
ALTER TABLE [dbo].[IV_AgdUsr] ADD CONSTRAINT [FK__IV_AgdUsr__SeqAg__47F2538C] FOREIGN KEY ([SeqAgenda]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Agenda
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Agenda] (
    [SeqAgenda] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [Contato] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAgendamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TarefaCompromisso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [CodProcesso] decimal(4,0) NULL,
    [Acao] decimal(6,0) NULL,
    [Assunto] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssuntoCmpl] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Classe] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAgenda] datetime NOT NULL,
    [DtaAgendaFinal] datetime NULL,
    [DtaLimiteExecucao] datetime NULL,
    [DtaAviso] datetime NULL,
    [DtaAgendaOriginal] datetime NULL,
    [DtaPrimContato] datetime NULL,
    [Prioridade] decimal(1,0) NOT NULL,
    [Detalhe] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAcesso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HistoricoOrigem] numeric(18,0) NULL,
    [UsuGerouAcao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [DtaGeracaoOrig] datetime NULL,
    [TempoEstimado] decimal(6,0) NULL,
    [DtaLeitura] datetime NULL,
    [Realizada] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaRealizacao] datetime NULL,
    [UltResultado] numeric(18,0) NULL,
    [UltHistorico] numeric(18,0) NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltResultado] datetime NULL,
    [Departamento] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HUDecorrido] decimal(8,2) NULL,
    [Vendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RevisarHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [QtdLibBloq] decimal(4,0) NULL,
    CONSTRAINT [PK__IV_Agenda__6A518D31] PRIMARY KEY CLUSTERED ([SeqAgenda])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AgendaPK] ON [dbo].[IV_Agenda] ([SeqAgenda]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE10] ON [dbo].[IV_Agenda] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE11] ON [dbo].[IV_Agenda] ([SeqUsuario], [Realizada]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE12] ON [dbo].[IV_Agenda] ([SeqUsuario], [HistoricoOrigem]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE13] ON [dbo].[IV_Agenda] ([UltHistorico]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE2] ON [dbo].[IV_Agenda] ([DtaAgenda], [Realizada]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE7] ON [dbo].[IV_Agenda] ([DtaUltResultado], [UltResultado]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE8] ON [dbo].[IV_Agenda] ([Realizada]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIE9] ON [dbo].[IV_Agenda] ([HistoricoOrigem]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIF1] ON [dbo].[IV_Agenda] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIF2] ON [dbo].[IV_Agenda] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaIF3] ON [dbo].[IV_Agenda] ([Acao]);
GO
ALTER TABLE [dbo].[IV_Agenda] ADD CONSTRAINT [FK__IV_Agenda__SeqPe__039D293F] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IV_Agenda] ADD CONSTRAINT [FK__IV_Agenda__Acao__04914D78] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]);
GO
ALTER TABLE [dbo].[IV_Agenda] ADD CONSTRAINT [FK__IV_Agenda__SeqPe__49DA9BFE] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IV_Agenda] ADD CONSTRAINT [FK__IV_Agenda__Acao__4ACEC037] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Agenda_bkp20250717
   Criada em ..: 2025-07-17
   Alterada em : 2025-07-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Agenda_bkp20250717] (
    [SeqAgenda] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [Contato] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAgendamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TarefaCompromisso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [CodProcesso] decimal(4,0) NULL,
    [Acao] decimal(6,0) NULL,
    [Assunto] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssuntoCmpl] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Classe] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaAgenda] datetime NOT NULL,
    [DtaAgendaFinal] datetime NULL,
    [DtaLimiteExecucao] datetime NULL,
    [DtaAviso] datetime NULL,
    [DtaAgendaOriginal] datetime NULL,
    [DtaPrimContato] datetime NULL,
    [Prioridade] decimal(1,0) NOT NULL,
    [Detalhe] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAcesso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HistoricoOrigem] numeric(18,0) NULL,
    [UsuGerouAcao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [DtaGeracaoOrig] datetime NULL,
    [TempoEstimado] decimal(6,0) NULL,
    [DtaLeitura] datetime NULL,
    [Realizada] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaRealizacao] datetime NULL,
    [UltResultado] numeric(18,0) NULL,
    [UltHistorico] numeric(18,0) NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltResultado] datetime NULL,
    [Departamento] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HUDecorrido] decimal(8,2) NULL,
    [Vendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RevisarHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [QtdLibBloq] decimal(4,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AGENDACMPL
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AGENDACMPL] (
    [SEQAGENDA] numeric(18,0) NOT NULL,
    [DDDPREFERENCIAL] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONEPREFERENCIAL] numeric(12,0) NULL,
    [SILOORIGEM] numeric(8,0) NULL,
    CONSTRAINT [PK__IV_AGEND__60998463755A308E] PRIMARY KEY CLUSTERED ([SEQAGENDA])
);
GO
ALTER TABLE [dbo].[IV_AGENDACMPL] ADD CONSTRAINT [FK__IV_AGENDA__SEQAG__08CCBE07] FOREIGN KEY ([SEQAGENDA]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AgendaCtrl
   Criada em ..: 2020-08-25
   Alterada em : 2021-06-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AgendaCtrl] (
    [SeqAgenda] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Base] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Atitude] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqAgendaGerada] numeric(18,0) NULL,
    [SEQTXTPADRAO] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_AgendaCtrl__6B45B16A] PRIMARY KEY CLUSTERED ([SeqAgenda], [SeqUsuario], [Base], [Atitude])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AgendaCtrlPK] ON [dbo].[IV_AgendaCtrl] ([SeqAgenda], [SeqUsuario], [Base], [Atitude]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaCtrlIF1] ON [dbo].[IV_AgendaCtrl] ([SeqAgenda]);
GO
CREATE NONCLUSTERED INDEX [IV_AgendaCtrlIF2] ON [dbo].[IV_AgendaCtrl] ([SeqAgendaGerada]);
GO
ALTER TABLE [dbo].[IV_AgendaCtrl] ADD CONSTRAINT [FK__IV_Agenda__SEQTX__389BCCBA] FOREIGN KEY ([SEQTXTPADRAO]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_AgendaCtrl] ADD CONSTRAINT [FK__IV_Agenda__SeqAg__4BC2E470] FOREIGN KEY ([SeqAgenda]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AGENDAITEM
   Criada em ..: 2025-02-17
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AGENDAITEM] (
    [SEQAGDITEM] numeric(18,0) NOT NULL,
    [SEQAGENDA] numeric(18,0) NOT NULL,
    [ORDEM] numeric(4,0) NOT NULL,
    [ITEM] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_AGEND__CD4A95D12209C44C] PRIMARY KEY CLUSTERED ([SEQAGDITEM])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AgendaLog
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AgendaLog] (
    [Kn1] numeric(18,0) NOT NULL,
    [Tb] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Kn2] numeric(18,0) NOT NULL,
    [Ks] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaLog] datetime NOT NULL,
    [Usr] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodApl] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQLOGTB] numeric(18,0) IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK__IV_Agend__3765F6589FEDCFFC] PRIMARY KEY CLUSTERED ([SEQLOGTB])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AtdBloq
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AtdBloq] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [TpBloq] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaBloq] datetime NOT NULL,
    [DtaBloqueio] datetime NULL,
    [CausaBloqueio] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsBloq] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaLiberacao] datetime NULL,
    [SeqUsuLiberacao] decimal(8,0) NULL,
    [UsuLiberacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsDesbloq] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_AtdBloq__6D2DF9DC] PRIMARY KEY CLUSTERED ([SeqUsuario], [TpBloq], [DtaBloq])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AtdBloqPK] ON [dbo].[IV_AtdBloq] ([SeqUsuario], [TpBloq], [DtaBloq]);
GO
CREATE NONCLUSTERED INDEX [IV_AtdBloqIF1] ON [dbo].[IV_AtdBloq] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IV_AtdBloq] ADD CONSTRAINT [FK__IV_AtdBlo__SeqUs__4E9F511B] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Atendente
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Atendente] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [SeqPerfil] decimal(4,0) NOT NULL,
    CONSTRAINT [PK__IV_Atendente__6E221E15] PRIMARY KEY CLUSTERED ([SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AtendentePK] ON [dbo].[IV_Atendente] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_AtendenteIF1] ON [dbo].[IV_Atendente] ([SeqPerfil]);
GO
ALTER TABLE [dbo].[IV_Atendente] ADD CONSTRAINT [FK__IV_Atende__SeqPe__4F937554] FOREIGN KEY ([SeqPerfil]) REFERENCES [dbo].[IV_SegPerfil] ([SeqPerfil]);
GO
ALTER TABLE [dbo].[IV_Atendente] ADD CONSTRAINT [FK__IV_Atende__SeqPe__09560295] FOREIGN KEY ([SeqPerfil]) REFERENCES [dbo].[IV_SegPerfil] ([SeqPerfil]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Ativ
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Ativ] (
    [SeqPlanoAtiv] numeric(18,0) NOT NULL,
    [Procedimento] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Recurso] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TempoEstimado] decimal(6,0) NULL,
    [TempoReal] decimal(6,0) NULL,
    [TempoRealTot] decimal(6,0) NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [DtaPlanejada] datetime NULL,
    [DtaConfirmada] datetime NULL,
    [DtaUltAndamento] datetime NULL,
    [Realizada] numeric(1,0) NULL,
    [StatInc] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RecursoCliente] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoFaturamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Ativ__6F16424E] PRIMARY KEY CLUSTERED ([SeqPlanoAtiv])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AtivPK] ON [dbo].[IV_Ativ] ([SeqPlanoAtiv]);
GO
ALTER TABLE [dbo].[IV_Ativ] ADD CONSTRAINT [FK__IV_Ativ__SeqPlan__5087998D] FOREIGN KEY ([SeqPlanoAtiv]) REFERENCES [dbo].[IV_PlanoAtiv] ([SeqPlanoAtiv]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AtivAgenda
   Criada em ..: 2017-04-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AtivAgenda] (
    [SeqPlanoAtiv] numeric(18,0) NOT NULL,
    [SeqAgenda] numeric(18,0) NOT NULL,
    [DtaDeOriginal] datetime NULL,
    [DtaAOriginal] datetime NULL,
    [TempoEstimado] numeric(6,0) NULL,
    [StatReal] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_AtivA__42B4D58B5D8DD158] PRIMARY KEY CLUSTERED ([SeqPlanoAtiv], [SeqAgenda])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_AtivAgenda] ON [dbo].[IV_AtivAgenda] ([SeqPlanoAtiv]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_AtivAgenda] ON [dbo].[IV_AtivAgenda] ([SeqAgenda]);
GO
ALTER TABLE [dbo].[IV_AtivAgenda] ADD CONSTRAINT [FK__IV_AtivAg__SeqPl__68AA0738] FOREIGN KEY ([SeqPlanoAtiv]) REFERENCES [dbo].[IV_Ativ] ([SeqPlanoAtiv]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_AtivAgenda] ADD CONSTRAINT [FK__IV_AtivAg__SeqAg__699E2B71] FOREIGN KEY ([SeqAgenda]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ATIVGRUPO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ATIVGRUPO] (
    [SEQATIVGRUPO] numeric(6,0) NOT NULL,
    [GRUPO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SEQATIVMODELO] numeric(6,0) NOT NULL,
    [ORDEM] numeric(4,0) NOT NULL,
    CONSTRAINT [PK__IV_ATIVG__A97F33C50BC109A5] PRIMARY KEY CLUSTERED ([SEQATIVGRUPO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_ATIVGRUPO_0] ON [dbo].[IV_ATIVGRUPO] ([SEQATIVMODELO]);
GO
ALTER TABLE [dbo].[IV_ATIVGRUPO] ADD CONSTRAINT [FK__IV_ATIVGR__SEQAT__71E958AF] FOREIGN KEY ([SEQATIVMODELO]) REFERENCES [dbo].[IV_ATIVMODELO] ([SEQATIVMODELO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ATIVMODELO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ATIVMODELO] (
    [SEQATIVMODELO] numeric(6,0) NOT NULL,
    [MODELOPROJETO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ORDEM] numeric(4,0) NOT NULL,
    CONSTRAINT [PK__IV_ATIVM__70974A8ED080B561] PRIMARY KEY CLUSTERED ([SEQATIVMODELO])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ATIVPADRAO
   Criada em ..: 2019-02-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ATIVPADRAO] (
    [SEQATIVPADRAO] numeric(18,0) NOT NULL,
    [SEQATIVGRUPO] numeric(6,0) NOT NULL,
    [DESCRICAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TEMPOESTIMADO] numeric(6,0) NULL,
    [RECURSO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORDEM] numeric(4,0) NOT NULL,
    CONSTRAINT [PK__IV_ATIVP__6A5E4F39F700BE89] PRIMARY KEY CLUSTERED ([SEQATIVPADRAO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_ATIVPADRAOX] ON [dbo].[IV_ATIVPADRAO] ([SEQATIVGRUPO]);
GO
ALTER TABLE [dbo].[IV_ATIVPADRAO] ADD CONSTRAINT [FK__IV_ATIVPA__SEQAT__72DD7CE8] FOREIGN KEY ([SEQATIVGRUPO]) REFERENCES [dbo].[IV_ATIVGRUPO] ([SEQATIVGRUPO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AtivProc
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AtivProc] (
    [SeqPlanoAtiv] numeric(18,0) NOT NULL,
    [Processo] numeric(18,0) NOT NULL,
    [TempoEstimado] decimal(6,0) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Realizada] numeric(1,0) NULL,
    [StatReal] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndFatBonif] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_AtivProc__72E6D332] PRIMARY KEY CLUSTERED ([SeqPlanoAtiv], [Processo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AtivProcPK] ON [dbo].[IV_AtivProc] ([SeqPlanoAtiv], [Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_AtivProcIF2] ON [dbo].[IV_AtivProc] ([SeqPlanoAtiv]);
GO
ALTER TABLE [dbo].[IV_AtivProc] ADD CONSTRAINT [FK__IV_AtivPr__SeqPl__54582A71] FOREIGN KEY ([SeqPlanoAtiv]) REFERENCES [dbo].[IV_Ativ] ([SeqPlanoAtiv]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_AtribLista
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_AtribLista] (
    [Atributo] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Lista] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__IV_AtribLista__73DAF76B] PRIMARY KEY CLUSTERED ([Atributo], [Lista])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AtribListaPK] ON [dbo].[IV_AtribLista] ([Atributo], [Lista]);
GO
CREATE NONCLUSTERED INDEX [IV_AtribListaIF1] ON [dbo].[IV_AtribLista] ([Atributo]);
GO
ALTER TABLE [dbo].[IV_AtribLista] ADD CONSTRAINT [FK__IV_AtribL__Atrib__554C4EAA] FOREIGN KEY ([Atributo]) REFERENCES [dbo].[IV_Atributo] ([Atributo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Atributo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Atributo] (
    [Atributo] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [UsaFeminino] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [UsaMasculino] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [UsaPessoaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [UsaContato] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Principal] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TipoDado] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Obrigatorio] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Validade] decimal(3,0) NULL,
    [Ordem] decimal(3,0) NULL,
    [SeqAtributo] numeric(6,0) NULL,
    CONSTRAINT [PK__IV_Atributo__74CF1BA4] PRIMARY KEY CLUSTERED ([Atributo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_AtributoPK] ON [dbo].[IV_Atributo] ([Atributo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_BaseInformacao
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_BaseInformacao] (
    [SeqConh] decimal(6,0) NOT NULL,
    [FonteTipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FonteTamanho] decimal(2,0) NULL,
    [FonteCor] numeric(18,0) NULL,
    [FundoCor] numeric(18,0) NULL,
    [Conteudo] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_BaseInformaca__75C33FDD] PRIMARY KEY CLUSTERED ([SeqConh])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_BaseInformacPK] ON [dbo].[IV_BaseInformacao] ([SeqConh]);
GO
ALTER TABLE [dbo].[IV_BaseInformacao] ADD CONSTRAINT [FK__IV_BaseIn__SeqCo__564072E3] FOREIGN KEY ([SeqConh]) REFERENCES [dbo].[IV_Conhecimento] ([SeqConh]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_BonusCC
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_BonusCC] (
    [SeqBonusCC] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [DtaMovto] datetime NULL,
    [Descricao] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VlrMovto] numeric(14,2) NULL,
    [LinkDocto] numeric(18,0) NULL,
    [LinkTipo] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLcto] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_BonusCC__163AFC67] PRIMARY KEY CLUSTERED ([SeqBonusCC])
);
GO
CREATE NONCLUSTERED INDEX [XIE1IV_BonusCC] ON [dbo].[IV_BonusCC] ([LinkDocto]);
GO
CREATE NONCLUSTERED INDEX [XIE2IV_BonusCC] ON [dbo].[IV_BonusCC] ([SeqPessoa], [DtaMovto]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_BonusCC] ON [dbo].[IV_BonusCC] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Campanha
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Campanha] (
    [SeqCampanha] decimal(6,0) NOT NULL,
    [Campanha] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Detalhe] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInicio] datetime NULL,
    [DtaFim] datetime NULL,
    [EmUso] numeric(1,0) NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [ProvedorSmsPL] numeric(18,0) NULL,
    [Resultado] numeric(6,0) NULL,
    [PrioridadeMail2Easy] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DetHistorico] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRIORIDADEALLIN] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRIORIDADEEMAIL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRIORIDADESMS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRIORIDADECORREIO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQTXTPADRAO] numeric(18,0) NULL,
    [INDINTERATIVO] numeric(1,0) NULL,
    [CODCAMPANHA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQCONTEUDO] numeric(6,0) NULL,
    [SEQFORMULARIO] numeric(18,0) NULL,
    [SEQEMAILTEMPLATE] numeric(18,0) NULL,
    [PROVEDOREMAILPL] numeric(18,0) NULL,
    [EMAILHORAINI] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAILHORAFIM] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSHORAINI] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMSHORAFIM] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDMSG] numeric(18,0) NULL,
    [IDULTUPLOAD] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Campanha__76B76416] PRIMARY KEY CLUSTERED ([SeqCampanha])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CampanhaPK] ON [dbo].[IV_Campanha] ([SeqCampanha]);
GO
CREATE NONCLUSTERED INDEX [XIF5IV_Campanha] ON [dbo].[IV_Campanha] ([ProvedorSmsPL]);
GO
CREATE NONCLUSTERED INDEX [XIF7IV_Campanha] ON [dbo].[IV_Campanha] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CAMPANHA] ON [dbo].[IV_Campanha] ([SEQCONTEUDO]);
GO
CREATE NONCLUSTERED INDEX [XIF10IV_CAMPANHA] ON [dbo].[IV_Campanha] ([ProvedorSmsPL]);
GO
CREATE NONCLUSTERED INDEX [XIF11IV_CAMPANHA] ON [dbo].[IV_Campanha] ([PROVEDOREMAILPL]);
GO
CREATE NONCLUSTERED INDEX [XIF6IV_CAMPANHA] ON [dbo].[IV_Campanha] ([SEQFORMULARIO]);
GO
CREATE NONCLUSTERED INDEX [XIF8IV_CAMPANHA] ON [dbo].[IV_Campanha] ([SEQEMAILTEMPLATE]);
GO
CREATE NONCLUSTERED INDEX [XIF9IV_CAMPANHA] ON [dbo].[IV_Campanha] ([Resultado]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CampPesMsg
   Criada em ..: 2016-07-12
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CampPesMsg] (
    [SeqCampPessoa] numeric(18,0) NOT NULL,
    [Canal] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CanalDestino] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Mensagem] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaEnviar] datetime NULL,
    [DtaEnvio] datetime NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [StatusMsg] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaLeitura] datetime NULL,
    [DtaClique] datetime NULL,
    [IndSpam] numeric(1,0) NULL,
    [IndDescadastro] numeric(1,0) NULL,
    [IndEntregue] numeric(1,0) NULL,
    [IndEnviado] numeric(1,0) NULL,
    [IndLido] numeric(1,0) NULL,
    [IndClique] numeric(1,0) NULL,
    [IndErro] numeric(1,0) NULL,
    [PROCESSO] numeric(18,0) NULL,
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [SEQCAMPANHA] numeric(6,0) NOT NULL,
    [SEQCONTATO] numeric(6,0) NOT NULL,
    [DTAGERACAO] datetime NULL,
    CONSTRAINT [PK__IV_CampP__FFE14B9970C3BC30] PRIMARY KEY CLUSTERED ([SeqCampPessoa], [Canal])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKRIV_CampPesMsg] ON [dbo].[IV_CampPesMsg] ([SeqCampPessoa], [Canal]);
GO
CREATE NONCLUSTERED INDEX [XIF1RIV_CampPesMsg] ON [dbo].[IV_CampPesMsg] ([SeqCampPessoa]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVCAMPPESMSG_STATUS] ON [dbo].[IV_CampPesMsg] ([Status]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPPESMSG_SEQPESSOA] ON [dbo].[IV_CampPesMsg] ([SEQPESSOA]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPPESMSG_SEQCAMPANHA] ON [dbo].[IV_CampPesMsg] ([SEQCAMPANHA]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPPESMSG_3] ON [dbo].[IV_CampPesMsg] ([SEQCAMPANHA], [DtaEnvio]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPPESMSG_4] ON [dbo].[IV_CampPesMsg] ([SEQCAMPANHA], [DtaEnviar]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPPESMSG_5] ON [dbo].[IV_CampPesMsg] ([Canal], [SEQCAMPANHA]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVCAMPPESMSG_02] ON [dbo].[IV_CampPesMsg] ([SEQCAMPANHA], [Canal]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVCAMPPESMSG_04] ON [dbo].[IV_CampPesMsg] ([SEQCAMPANHA], [Canal], [DtaEnviar]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVCAMPPESMSG_05] ON [dbo].[IV_CampPesMsg] ([Canal]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVCAMPPESMSG_06] ON [dbo].[IV_CampPesMsg] ([Canal], [DtaEnviar]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVCAMPPESMSG_07] ON [dbo].[IV_CampPesMsg] ([SEQCAMPANHA], [Canal], [CanalDestino]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVCAMPPESMSG_08] ON [dbo].[IV_CampPesMsg] ([SEQCAMPANHA], [CanalDestino]);
GO
ALTER TABLE [dbo].[IV_CampPesMsg] ADD CONSTRAINT [FK__IV_CampPe__SEQPE__12562841] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_CampPesMsg] ADD CONSTRAINT [FK__IV_CampPe__SeqCa__4E154136] FOREIGN KEY ([SeqCampPessoa]) REFERENCES [dbo].[IV_CampPessoa] ([SeqCampPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CampPessoa
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CampPessoa] (
    [SeqCampPessoa] numeric(18,0) NOT NULL,
    [SeqCampanha] numeric(6,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [SeqContato] numeric(6,0) NULL,
    [DtaGeracao] datetime NULL,
    CONSTRAINT [PK__IV_CampP__8F5B70755B021E6B] PRIMARY KEY CLUSTERED ([SeqCampPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKRIV_CampPessoa] ON [dbo].[IV_CampPessoa] ([SeqCampPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF1RIV_CampPessoa] ON [dbo].[IV_CampPessoa] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF2RIV_CampPessoa] ON [dbo].[IV_CampPessoa] ([SeqCampanha]);
GO
CREATE NONCLUSTERED INDEX [XIF3RIV_CampPessoa] ON [dbo].[IV_CampPessoa] ([SeqPessoa], [SeqContato]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CAMPSELECAO
   Criada em ..: 2017-08-01
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CAMPSELECAO] (
    [SEQCAMPANHA] numeric(6,0) NOT NULL,
    [SEQSELECAO] numeric(18,0) NOT NULL,
    [USAPESSOA] numeric(1,0) NULL,
    [USACONTATO] numeric(1,0) NULL,
    [USAPESSOAGE] numeric(1,0) NULL,
    [USAPESSOAPES] numeric(1,0) NULL,
    [PAPEIS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NULL,
    [DTAULTSTATUS] datetime NULL,
    [USATODOSCANAIS] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_CAMPS__916D557236BA1B79] PRIMARY KEY CLUSTERED ([SEQCAMPANHA], [SEQSELECAO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPSELECAO] ON [dbo].[IV_CAMPSELECAO] ([SEQCAMPANHA]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPSELECAO_0] ON [dbo].[IV_CAMPSELECAO] ([SEQSELECAO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CampVoucher
   Criada em ..: 2016-07-12
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CampVoucher] (
    [SeqCampPessoa] numeric(18,0) NOT NULL,
    [DtaUso] datetime NULL,
    [NroVoucher] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndUso] numeric(1,0) NULL,
    [ChaveBusca] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [SEQCAMPANHA] numeric(6,0) NOT NULL,
    [SEQCONTATO] numeric(6,0) NOT NULL,
    [DTAGERACAO] datetime NULL,
    CONSTRAINT [PK__IV_CampV__8F5B70752306DCE0] PRIMARY KEY CLUSTERED ([SeqCampPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKRIV_CampVoucher] ON [dbo].[IV_CampVoucher] ([SeqCampPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIE1RIV_CampVoucher] ON [dbo].[IV_CampVoucher] ([ChaveBusca]);
GO
CREATE NONCLUSTERED INDEX [XIE1IV_CAMPVOUCHER] ON [dbo].[IV_CampVoucher] ([ChaveBusca]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPVOUCHER_SEQPESSOA] ON [dbo].[IV_CampVoucher] ([SEQPESSOA]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CAMPVOUCHER_SEQCAMPANHA] ON [dbo].[IV_CampVoucher] ([SEQCAMPANHA]);
GO
ALTER TABLE [dbo].[IV_CampVoucher] ADD CONSTRAINT [FK__IV_CampVo__SEQPE__143E70B3] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_CampVoucher] ADD CONSTRAINT [FK__IV_CampVo__SeqCa__50F1ADE1] FOREIGN KEY ([SeqCampPessoa]) REFERENCES [dbo].[IV_CampPessoa] ([SeqCampPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartContratante
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartContratante] (
    [SeqContratante] numeric(10,0) NOT NULL,
    [Contratante] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DescContratante] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqCartProd] numeric(6,0) NOT NULL,
    [SeqPessoaGrupo] numeric(10,0) NOT NULL,
    [SeqPessoaCtr] numeric(10,0) NOT NULL,
    [NroConta] numeric(18,0) NULL,
    [IDOrigemCml] numeric(18,0) NULL,
    [NroEstabelecimento] numeric(18,0) NULL,
    [IndUsoEmProposta] numeric(1,0) NULL,
    [IndProdutoPadrao] numeric(1,0) NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [ChkSum] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_CartContratan__4436D141] PRIMARY KEY CLUSTERED ([SeqContratante])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XAK1IV_CartContratan] ON [dbo].[IV_CartContratante] ([IDOrigemCml]);
GO
CREATE NONCLUSTERED INDEX [XIE2IV_CartContratan] ON [dbo].[IV_CartContratante] ([NroConta]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CartContratan] ON [dbo].[IV_CartContratante] ([SeqPessoaGrupo]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CartContratan] ON [dbo].[IV_CartContratante] ([SeqPessoaCtr]);
GO
CREATE NONCLUSTERED INDEX [XIF3IV_CartContratan] ON [dbo].[IV_CartContratante] ([SeqCartProd]);
GO
ALTER TABLE [dbo].[IV_CartContratante] ADD CONSTRAINT [FK__IV_CartCo__SeqCa__5DF6A344] FOREIGN KEY ([SeqCartProd]) REFERENCES [dbo].[IV_CartProduto] ([SeqCartProd]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartCred
   Criada em ..: 2015-09-14
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartCred] (
    [SeqCartCred] numeric(18,0) NOT NULL,
    [SeqCartTitular] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [NroCartao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCartaoX] varchar(24) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCartao6] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Validade] numeric(4,0) NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqCFinFis] numeric(18,0) NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NomeImpresso] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoRelacSeqPar] numeric(18,0) NULL,
    [Nome] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sexo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD1] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro1] numeric(12,0) NULL,
    [FoneCmpl1] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneDDD2] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FoneNro2] numeric(12,0) NULL,
    [FoneCmpl2] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroCPF] numeric(13,0) NULL,
    [DigCPF] numeric(2,0) NULL,
    [RGNro] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGOrgaoEmissor] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGUFEmissao] char(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGDtaEmissao] datetime NULL,
    [DtaNascimento] datetime NULL,
    [DtaImpressao] datetime NULL,
    [DtaUltImpressao] datetime NULL,
    [NacionalSeqPar] numeric(18,0) NULL,
    [EstadoCivilSeqPar] numeric(18,0) NULL,
    [ChkSum] numeric(18,0) NULL,
    [CVV] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Trilha2Cmpl] varchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    CONSTRAINT [PK__IV_CartCred__461F19B3] PRIMARY KEY CLUSTERED ([SeqCartCred])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XAK1IV_CartCred] ON [dbo].[IV_CartCred] ([NroCartao6]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTCREDNRO] ON [dbo].[IV_CartCred] ([NroCartao]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTCREDIF1] ON [dbo].[IV_CartCred] ([SeqCFinFis]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTCREDIF3] ON [dbo].[IV_CartCred] ([TipoRelacSeqPar]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTCREDIF4] ON [dbo].[IV_CartCred] ([NacionalSeqPar]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTCREDIF5] ON [dbo].[IV_CartCred] ([EstadoCivilSeqPar]);
GO
CREATE NONCLUSTERED INDEX [XIF6IV_CartCred] ON [dbo].[IV_CartCred] ([SeqCartTitular]);
GO
CREATE NONCLUSTERED INDEX [XIF7IV_CartCred] ON [dbo].[IV_CartCred] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IV_CartCred] ADD CONSTRAINT [FK__IV_CartCr__SeqCa__61C73428] FOREIGN KEY ([SeqCartTitular]) REFERENCES [dbo].[IV_CartTitular] ([SeqCartTitular]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartLoteCartao
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartLoteCartao] (
    [SeqCartLote] numeric(18,0) NOT NULL,
    [SeqCartCred] numeric(18,0) NOT NULL,
    [IndImpressaoOk] numeric(1,0) NULL,
    [Via] numeric(2,0) NULL,
    [ChkSum] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_CartLoteCarta__48076225] PRIMARY KEY CLUSTERED ([SeqCartLote], [SeqCartCred])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CartLoteCarta] ON [dbo].[IV_CartLoteCartao] ([SeqCartLote]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CartLoteCarta] ON [dbo].[IV_CartLoteCartao] ([SeqCartCred]);
GO
ALTER TABLE [dbo].[IV_CartLoteCartao] ADD CONSTRAINT [FK__IV_CartLo__SeqCa__64A3A0D3] FOREIGN KEY ([SeqCartLote]) REFERENCES [dbo].[IV_CartLoteImp] ([SeqCartLote]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_CartLoteCartao] ADD CONSTRAINT [FK__IV_CartLo__SeqCa__6597C50C] FOREIGN KEY ([SeqCartCred]) REFERENCES [dbo].[IV_CartCred] ([SeqCartCred]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartLoteImp
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartLoteImp] (
    [SeqCartLote] numeric(18,0) NOT NULL,
    [SeqContratante] numeric(10,0) NOT NULL,
    [DescLote] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoLote] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Situacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaImpressao] datetime NULL,
    [UsuImpressao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CartLoteImp__49EFAA97] PRIMARY KEY CLUSTERED ([SeqCartLote])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CartLoteImp] ON [dbo].[IV_CartLoteImp] ([SeqContratante]);
GO
ALTER TABLE [dbo].[IV_CartLoteImp] ADD CONSTRAINT [FK__IV_CartLo__SeqCo__668BE945] FOREIGN KEY ([SeqContratante]) REFERENCES [dbo].[IV_CartContratante] ([SeqContratante]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartMarca
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartMarca] (
    [SeqCartMarca] numeric(4,0) NOT NULL,
    [Marca] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [ChkSum] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_CartMarca__4BD7F309] PRIMARY KEY CLUSTERED ([SeqCartMarca])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CARTPESORIGEM
   Criada em ..: 2023-06-05
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CARTPESORIGEM] (
    [SEQCARTPESORIGEM] numeric(18,0) NOT NULL,
    [SEQPROPOSTA] numeric(18,0) NOT NULL,
    [ORIGEM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    CONSTRAINT [PK__IV_CARTP__B239D4516A04BC08] PRIMARY KEY CLUSTERED ([SEQCARTPESORIGEM])
);
GO
ALTER TABLE [dbo].[IV_CARTPESORIGEM] ADD CONSTRAINT [FK__IV_CARTPE__SEQPR__61BDDDDE] FOREIGN KEY ([SEQPROPOSTA]) REFERENCES [dbo].[IV_CartProposta] ([SeqProposta]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CARTPESSOA
   Criada em ..: 2020-11-16
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CARTPESSOA] (
    [SEQPROPOSTA] numeric(18,0) NOT NULL,
    [NOMEMAE] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOMEPAI] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGINSCRICAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGUFEMISSOR] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGORGAOEMISSOR] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RGDTAEMISSAO] datetime NULL,
    [CNHNRO] numeric(18,0) NULL,
    [CNHDTAVALIDADE] datetime NULL,
    [CNHDTAEMISSAO] datetime NULL,
    [CARTTRABNRO] numeric(18,0) NULL,
    [CARTTRABSERIE] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CARTTRABUF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CARTTRABDTAEMISSAO] datetime NULL,
    [IDENTPROFNRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IDENTPROFSEQPAR] numeric(18,0) NULL,
    [IDENTPROFDTAEMISSAO] datetime NULL,
    [CEP] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQCIDADE] numeric(18,0) NULL,
    [BAIRRO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOGRADOURO] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOGRADOURONRO] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOGRADOUROCMPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONECELULAR] numeric(18,0) NULL,
    [FONERESIDENCIAL] numeric(18,0) NULL,
    [NACIONALSEQPAR] numeric(18,0) NULL,
    [NATUF] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NATSEQCIDADE] numeric(18,0) NULL,
    [OCPRENDA] numeric(15,2) NULL,
    [OUTRARENDAVLR] numeric(15,2) NULL,
    [OCPFISICAJURIDICA] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NATOCUPACAOSEQPAR] numeric(18,0) NULL,
    [ATIVIDADESEQPAR] numeric(18,0) NULL,
    [ESTADOCIVILSEQPAR] numeric(18,0) NULL,
    [GRAUINSTRSEQPAR] numeric(18,0) NULL,
    [DTANASCIMENTO] datetime NULL,
    [SEXO] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAIL] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDDEPENDENTE] numeric(2,0) NULL,
    [CPF] numeric(11,0) NULL,
    [NOME] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FOTO] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDPROCESSADO] numeric(1,0) NULL,
    [TIPODOCTOEXIGIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENTIDADEPROPOSTA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONERECADO] numeric(18,0) NULL,
    [CRNM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CRNMDTAEMISSAO] datetime NULL,
    [CRNMDTAVALIDADE] datetime NULL,
    [ABORDAGEMTIPO] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ABORDAGEMLOCAL] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CERTIFACEAPPKEY] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CARTP__9EBEC2B128B2E058] PRIMARY KEY CLUSTERED ([SEQPROPOSTA])
);
GO
ALTER TABLE [dbo].[IV_CARTPESSOA] ADD CONSTRAINT [FK__IV_CARTPE__SEQPR__6697A194] FOREIGN KEY ([SEQPROPOSTA]) REFERENCES [dbo].[IV_CartProposta] ([SeqProposta]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartProduto
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartProduto] (
    [SeqCartProd] numeric(6,0) NOT NULL,
    [SeqCartMarca] numeric(4,0) NOT NULL,
    [DescProduto] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndAltDadoAprovacao] numeric(1,0) NULL,
    [QtdeMaxCartAdicional] numeric(2,0) NULL,
    [ModeloImpressao] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [ChkSum] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_CartProduto__4DC03B7B] PRIMARY KEY CLUSTERED ([SeqCartProd])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CartProduto] ON [dbo].[IV_CartProduto] ([SeqCartMarca]);
GO
ALTER TABLE [dbo].[IV_CartProduto] ADD CONSTRAINT [FK__IV_CartPr__SeqCa__67800D7E] FOREIGN KEY ([SeqCartMarca]) REFERENCES [dbo].[IV_CartMarca] ([SeqCartMarca]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartProposta
   Criada em ..: 2015-09-14
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartProposta] (
    [SeqProposta] numeric(18,0) NOT NULL,
    [SeqCFinFis] numeric(18,0) NOT NULL,
    [SeqContratante] numeric(10,0) NOT NULL,
    [PercLojIndicSeqPar] numeric(18,0) NULL,
    [PercLojObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SituacaoSeqPar] numeric(18,0) NULL,
    [OrigemSeqPar] numeric(18,0) NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Processo] numeric(18,0) NULL,
    [TipoEndCorresp] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuPromotor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndRecusaCliente] numeric(1,0) NULL,
    [MotRecusaCliSeqPar] numeric(18,0) NULL,
    [DtaEnvioFin] datetime NULL,
    [DtaRespostaFin] datetime NULL,
    [MotivoRecusa] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdProposta] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RetObs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RetStatus] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RETPROPOBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RETPROPSTATUS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTARESPOSTAPROP] datetime NULL,
    [INDRETPROCESSADO] numeric(1,0) NULL,
    [INDRETPROPPROCESSADO] numeric(1,0) NULL,
    [NEUROTECH_LOG_ID] numeric(11,0) NULL,
    CONSTRAINT [PK__IV_CartProposta__4FA883ED] PRIMARY KEY CLUSTERED ([SeqProposta])
);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTAIE1] ON [dbo].[IV_CartProposta] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTAIE2] ON [dbo].[IV_CartProposta] ([IdProposta]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTAIE4] ON [dbo].[IV_CartProposta] ([DtaEnvioFin]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTAIF1] ON [dbo].[IV_CartProposta] ([SeqCFinFis]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTAIF2] ON [dbo].[IV_CartProposta] ([PercLojIndicSeqPar]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTAIF4] ON [dbo].[IV_CartProposta] ([SituacaoSeqPar]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTAIF5] ON [dbo].[IV_CartProposta] ([MotRecusaCliSeqPar]);
GO
CREATE NONCLUSTERED INDEX [XIF7IV_CartProposta] ON [dbo].[IV_CartProposta] ([SeqContratante]);
GO
CREATE NONCLUSTERED INDEX [XIF8IV_CartProposta] ON [dbo].[IV_CartProposta] ([OrigemSeqPar]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_CARTPROPOSTA] ON [dbo].[IV_CartProposta] ([NEUROTECH_LOG_ID]);
GO
CREATE NONCLUSTERED INDEX [IV_CARTPROPOSTA_IDX10] ON [dbo].[IV_CartProposta] ([Processo], [NEUROTECH_LOG_ID]);
GO
ALTER TABLE [dbo].[IV_CartProposta] ADD CONSTRAINT [FK__IV_CartPr__SeqCo__6C44C29B] FOREIGN KEY ([SeqContratante]) REFERENCES [dbo].[IV_CartContratante] ([SeqContratante]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CartTitular
   Criada em ..: 2015-09-14
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CartTitular] (
    [SeqCartTitular] numeric(18,0) NOT NULL,
    [SeqContratante] numeric(10,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqProposta] numeric(18,0) NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [NroConta] numeric(18,0) NULL,
    [DiaVencto] numeric(2,0) NULL,
    [VlrLimiteAprovado] numeric(14,2) NULL,
    [Validade] numeric(4,0) NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CartTitular__5190CC5F] PRIMARY KEY CLUSTERED ([SeqCartTitular])
);
GO
CREATE NONCLUSTERED INDEX [XIE1IV_CartTitular] ON [dbo].[IV_CartTitular] ([NroConta]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CartTitular] ON [dbo].[IV_CartTitular] ([SeqProposta]);
GO
CREATE NONCLUSTERED INDEX [XIF3IV_CartTitular] ON [dbo].[IV_CartTitular] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF4IV_CartTitular] ON [dbo].[IV_CartTitular] ([SeqContratante]);
GO
ALTER TABLE [dbo].[IV_CartTitular] ADD CONSTRAINT [FK__IV_CartTi__SeqPr__6E2D0B0D] FOREIGN KEY ([SeqProposta]) REFERENCES [dbo].[IV_CartProposta] ([SeqProposta]);
GO
ALTER TABLE [dbo].[IV_CartTitular] ADD CONSTRAINT [FK__IV_CartTi__SeqCo__7015537F] FOREIGN KEY ([SeqContratante]) REFERENCES [dbo].[IV_CartContratante] ([SeqContratante]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrCobranca
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrCobranca] (
    [SeqCbrCobranca] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqAgenda] numeric(18,0) NULL,
    [SeqCbrCriterio] numeric(6,0) NULL,
    [Lote] numeric(18,0) NOT NULL,
    [Processo] numeric(18,0) NULL,
    [Dta1Cob] datetime NULL,
    [Dta2Cob] datetime NULL,
    CONSTRAINT [PK__IV_CbrCo__02685989C26828CE] PRIMARY KEY CLUSTERED ([SeqCbrCobranca])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrCobranca] ON [dbo].[IV_CbrCobranca] ([SeqCbrCobranca]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CbrCobranca] ON [dbo].[IV_CbrCobranca] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CbrCobranca] ON [dbo].[IV_CbrCobranca] ([SeqAgenda]);
GO
CREATE NONCLUSTERED INDEX [XIF3IV_CbrCobranca] ON [dbo].[IV_CbrCobranca] ([SeqCbrCriterio]);
GO
CREATE NONCLUSTERED INDEX [XIF4IV_CbrCobranca] ON [dbo].[IV_CbrCobranca] ([Lote]);
GO
ALTER TABLE [dbo].[IV_CbrCobranca] ADD CONSTRAINT [FK__IV_CbrCob__SeqCb__7252A1AC] FOREIGN KEY ([SeqCbrCriterio]) REFERENCES [dbo].[IV_CbrCriterio] ([SeqCbrCriterio]);
GO
ALTER TABLE [dbo].[IV_CbrCobranca] ADD CONSTRAINT [FK__IV_CbrCobr__Lote__7346C5E5] FOREIGN KEY ([Lote]) REFERENCES [dbo].[IV_CbrCobrancaLote] ([Lote]);
GO
ALTER TABLE [dbo].[IV_CbrCobranca] ADD CONSTRAINT [FK__IV_CbrCob__SeqAg__715E7D73] FOREIGN KEY ([SeqAgenda]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CBRCOBRANCAHST
   Criada em ..: 2017-09-22
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CBRCOBRANCAHST] (
    [SEQCBRCOBRHST] numeric(18,0) NOT NULL,
    [SEQCBRCOBRANCA] numeric(18,0) NOT NULL,
    [SEQCBRCRITMSG] numeric(8,0) NULL,
    [DATA] datetime NULL,
    [OBS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CBRCO__636E780E623C9AFA] PRIMARY KEY CLUSTERED ([SEQCBRCOBRHST])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CBRCOBRANCAHS] ON [dbo].[IV_CBRCOBRANCAHST] ([SEQCBRCOBRANCA]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CBRCOBRANCAHS] ON [dbo].[IV_CBRCOBRANCAHST] ([SEQCBRCRITMSG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrCobrancaLote
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrCobrancaLote] (
    [Lote] numeric(18,0) NOT NULL,
    [DtaGeracao] datetime NULL,
    [SeqCbrCriterio] numeric(6,0) NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CbrCo__4E4659D22F91BDB9] PRIMARY KEY CLUSTERED ([Lote])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrCobrancaLot] ON [dbo].[IV_CbrCobrancaLote] ([Lote]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CbrCobrancaLo] ON [dbo].[IV_CbrCobrancaLote] ([SeqCbrCriterio]);
GO
ALTER TABLE [dbo].[IV_CbrCobrancaLote] ADD CONSTRAINT [FK__IV_CbrCob__SeqCb__743AEA1E] FOREIGN KEY ([SeqCbrCriterio]) REFERENCES [dbo].[IV_CbrCriterio] ([SeqCbrCriterio]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrCobrancaMon
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrCobrancaMon] (
    [SeqCbrCobrancaMonit] numeric(18,0) NOT NULL,
    [SeqCbrCobranca] numeric(18,0) NOT NULL,
    [DtaAnalise] datetime NOT NULL,
    [VlrAnterior] numeric(14,2) NULL,
    [VlrNovo] numeric(14,2) NULL,
    [VlrBaixado] numeric(14,2) NULL,
    [VlrAlterado] numeric(14,2) NULL,
    [QtdAnterior] numeric(4,0) NULL,
    [QtdTitNovo] numeric(4,0) NULL,
    [QtdTitBaixado] numeric(4,0) NULL,
    [QtdTitAlterado] numeric(4,0) NULL,
    CONSTRAINT [PK__IV_CbrCo__784E175F8E773C55] PRIMARY KEY CLUSTERED ([SeqCbrCobrancaMonit])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrCobrancaMon] ON [dbo].[IV_CbrCobrancaMon] ([SeqCbrCobrancaMonit]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CbrCobrancaMo] ON [dbo].[IV_CbrCobrancaMon] ([SeqCbrCobranca]);
GO
ALTER TABLE [dbo].[IV_CbrCobrancaMon] ADD CONSTRAINT [FK__IV_CbrCob__SeqCb__752F0E57] FOREIGN KEY ([SeqCbrCobranca]) REFERENCES [dbo].[IV_CbrCobranca] ([SeqCbrCobranca]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrCobrancaTit
   Criada em ..: 2016-07-12
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrCobrancaTit] (
    [SeqCbrCobranca] numeric(18,0) NOT NULL,
    [idTitulo] numeric(18,0) NOT NULL,
    [MotivoSaida] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndQuitado] numeric(1,0) NULL,
    [DtaInclusao] datetime NULL,
    [DtaSaida] datetime NULL,
    [VlrOriginal] numeric(14,2) NULL,
    [VlrPago] numeric(14,2) NULL,
    [VlrAberto] numeric(14,2) NULL,
    [DtaVencto] datetime NULL,
    CONSTRAINT [PK__IV_CbrCo__68594A6CB7EFF64E] PRIMARY KEY CLUSTERED ([SeqCbrCobranca], [idTitulo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrCobrancaTit] ON [dbo].[IV_CbrCobrancaTit] ([SeqCbrCobranca], [idTitulo]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CbrCobrancaTi] ON [dbo].[IV_CbrCobrancaTit] ([idTitulo]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CbrCobrancaTi] ON [dbo].[IV_CbrCobrancaTit] ([SeqCbrCobranca]);
GO
ALTER TABLE [dbo].[IV_CbrCobrancaTit] ADD CONSTRAINT [FK__IV_CbrCob__SeqCb__771756C9] FOREIGN KEY ([SeqCbrCobranca]) REFERENCES [dbo].[IV_CbrCobranca] ([SeqCbrCobranca]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrCobrancaTitLog
   Criada em ..: 2016-07-12
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrCobrancaTitLog] (
    [SeqCbrCobLog] numeric(18,0) NOT NULL,
    [SeqCbrCobranca] numeric(18,0) NULL,
    [idTitulo] numeric(18,0) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaLog] datetime NULL,
    CONSTRAINT [PK__IV_CbrCo__A97CC778AC6220C4] PRIMARY KEY CLUSTERED ([SeqCbrCobLog])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrCobrTitLog] ON [dbo].[IV_CbrCobrancaTitLog] ([SeqCbrCobLog]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CbrCobrTitLog] ON [dbo].[IV_CbrCobrancaTitLog] ([SeqCbrCobranca], [idTitulo]);
GO
ALTER TABLE [dbo].[IV_CbrCobrancaTitLog] ADD CONSTRAINT [FK__IV_CbrCobrancaTi__780B7B02] FOREIGN KEY ([SeqCbrCobranca], [idTitulo]) REFERENCES [dbo].[IV_CbrCobrancaTit] ([SeqCbrCobranca], [idTitulo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrCriterio
   Criada em ..: 2016-07-12
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrCriterio] (
    [SeqCbrCriterio] numeric(6,0) NOT NULL,
    [Criterio] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmprAgenda] numeric(6,0) NULL,
    [VlrPrior0] numeric(14,2) NULL,
    [VlrPrior1] numeric(14,2) NULL,
    [SeqTxt1CobeMail] numeric(18,0) NULL,
    [Dias1Cob] numeric(2,0) NULL,
    [SeqTxt1CobSMS] numeric(18,0) NULL,
    [SeqTxt2CobeMail] numeric(18,0) NULL,
    [Dias2Cob] numeric(2,0) NULL,
    [SeqTxt2CobSMS] numeric(18,0) NULL,
    [DiasGerarAgenda] numeric(2,0) NULL,
    [IndExecAutomatica] numeric(1,0) NULL,
    [IndAceitaNovoTitulo] numeric(1,0) NULL,
    [Acao] numeric(6,0) NULL,
    [ResultadoBaixa] numeric(6,0) NULL,
    [PapelDestino] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrDestino] numeric(18,0) NULL,
    [RELACDESTINO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDAVISOINDIVIDUALSM] numeric(1,0) NULL,
    [resultadosaida] numeric(6,0) NULL,
    [VLRMINIMO] numeric(14,2) NULL,
    [INDAGDAVULSA] numeric(1,0) NULL,
    [INDAVISOINDIVIDSMS] numeric(1,0) NULL,
    [INDGERAAVISODIAUTL] numeric(1,0) NULL,
    [EMAILREMETENTE] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CbrCr__F0D33A2E1102801B] PRIMARY KEY CLUSTERED ([SeqCbrCriterio])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([SeqCbrCriterio]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([SeqTxt1CobeMail]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([SeqTxt1CobSMS]);
GO
CREATE NONCLUSTERED INDEX [XIF3IV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([SeqTxt2CobeMail]);
GO
CREATE NONCLUSTERED INDEX [XIF4IV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([SeqTxt2CobSMS]);
GO
CREATE NONCLUSTERED INDEX [XIF5IV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [XIF6IV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([ResultadoBaixa]);
GO
CREATE NONCLUSTERED INDEX [XIF7IV_CbrCriterio] ON [dbo].[IV_CbrCriterio] ([SeqUsrDestino]);
GO
ALTER TABLE [dbo].[IV_CbrCriterio] ADD CONSTRAINT [FK__IV_CbrCri__SeqTx__7AE7E7AD] FOREIGN KEY ([SeqTxt2CobeMail]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_CbrCriterio] ADD CONSTRAINT [FK__IV_CbrCri__SeqTx__7BDC0BE6] FOREIGN KEY ([SeqTxt2CobSMS]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_CbrCriterio] ADD CONSTRAINT [FK__IV_CbrCri__SeqTx__78FF9F3B] FOREIGN KEY ([SeqTxt1CobeMail]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_CbrCriterio] ADD CONSTRAINT [FK__IV_CbrCri__SeqTx__79F3C374] FOREIGN KEY ([SeqTxt1CobSMS]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_CbrCriterio] ADD CONSTRAINT [FK__IV_CbrCri__SeqUs__7EB87891] FOREIGN KEY ([SeqUsrDestino]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrCriterioDef
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrCriterioDef] (
    [SeqCbrCritDef] numeric(18,0) NOT NULL,
    [SeqCbrCriterio] numeric(6,0) NOT NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [Departamento] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Especie] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoCobranca] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndCobrJuridica] numeric(1,0) NULL,
    [LocalCobranca] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VlrAberto] numeric(14,2) NULL,
    [DiasAbertoDe] numeric(2,0) NULL,
    [DiasAbertoAte] numeric(2,0) NULL,
    [AtividadePessoa] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FisicaJuridica] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [diasvencidode] numeric(4,0) NULL,
    [diasvencidoate] numeric(4,0) NULL,
    CONSTRAINT [PK__IV_CbrCr__35BD5EE05FB48836] PRIMARY KEY CLUSTERED ([SeqCbrCritDef])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrCriterioDef] ON [dbo].[IV_CbrCriterioDef] ([SeqCbrCritDef]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CbrCriterioDe] ON [dbo].[IV_CbrCriterioDef] ([SeqCbrCriterio]);
GO
ALTER TABLE [dbo].[IV_CbrCriterioDef] ADD CONSTRAINT [FK__IV_CbrCri__SeqCb__7FAC9CCA] FOREIGN KEY ([SeqCbrCriterio]) REFERENCES [dbo].[IV_CbrCriterio] ([SeqCbrCriterio]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CBRCRITERIOMSG
   Criada em ..: 2017-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CBRCRITERIOMSG] (
    [SEQCBRCRITMSG] numeric(8,0) NOT NULL,
    [SEQCBRCRITERIO] numeric(6,0) NULL,
    [DIAS1COB] numeric(2,0) NULL,
    [INDENVIOALTERNATIVO] numeric(1,0) NULL,
    [SEQTXTSMS] numeric(18,0) NULL,
    [SEQTXTEMAIL] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_CBRCR__61C3CFEBC1839AFA] PRIMARY KEY CLUSTERED ([SEQCBRCRITMSG])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CBRCRITERIOMS] ON [dbo].[IV_CBRCRITERIOMSG] ([SEQCBRCRITERIO]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_CBRCRITERIOMS] ON [dbo].[IV_CBRCRITERIOMSG] ([SEQTXTSMS]);
GO
CREATE NONCLUSTERED INDEX [XIF3IV_CBRCRITERIOMS] ON [dbo].[IV_CBRCRITERIOMSG] ([SEQTXTEMAIL]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CBRCRITMON
   Criada em ..: 2019-12-19
   Alterada em : 2020-11-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CBRCRITMON] (
    [SEQCBRCRITERIOMON] numeric(18,0) NOT NULL,
    [SEQCBRCRITERIO] numeric(6,0) NOT NULL,
    [DTAANALISE] datetime NOT NULL,
    [DTAULTANALISE] datetime NULL,
    [VLRNOVO] numeric(14,2) NULL,
    [VLRBAIXADO] numeric(14,2) NULL,
    [VLRALTERADO] numeric(14,2) NULL,
    [VLRMIGRADO] numeric(14,2) NULL,
    [QTDTITNOVO] numeric(6,0) NULL,
    [QTDTITBAIXADO] numeric(6,0) NULL,
    [QTDTITMIGRADO] numeric(6,0) NULL,
    [QTDPESNOVO] numeric(6,0) NULL,
    [QTDPESBAIXADO] numeric(6,0) NULL,
    [QTDPESALTERADO] numeric(6,0) NULL,
    [QTDPESMIGRADO] numeric(6,0) NULL,
    [USUANALISE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDTITALTERADO] numeric(6,0) NULL,
    CONSTRAINT [PK__IV_CBRCR__5FA7721D9974E1CB] PRIMARY KEY CLUSTERED ([SEQCBRCRITERIOMON])
);
GO
ALTER TABLE [dbo].[IV_CBRCRITMON] ADD CONSTRAINT [FK__IV_CBRCRI__SEQCB__6CBA8F3E] FOREIGN KEY ([SEQCBRCRITERIO]) REFERENCES [dbo].[IV_CbrCriterio] ([SeqCbrCriterio]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CbrTitulo
   Criada em ..: 2016-07-12
   Alterada em : 2018-10-06
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CbrTitulo] (
    [idTitulo] numeric(18,0) NOT NULL,
    [IndNaoCobravel] numeric(1,0) NULL,
    [Motivo] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_CbrTi__A3113E5757869AA3] PRIMARY KEY CLUSTERED ([idTitulo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_CbrTitulo] ON [dbo].[IV_CbrTitulo] ([idTitulo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CBRTITULOHST
   Criada em ..: 2019-05-24
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CBRTITULOHST] (
    [SEQCBRTITHST] numeric(18,0) NOT NULL,
    [IDTITULO] numeric(18,0) NULL,
    [DATA] date NULL,
    [TIPOHST] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [IDX_IV_CBRTITULOHST] PRIMARY KEY CLUSTERED ([SEQCBRTITHST])
);
GO
CREATE NONCLUSTERED INDEX [XIE1IV_CBRTITULOHST] ON [dbo].[IV_CBRTITULOHST] ([IDTITULO], [TIPOHST]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_CBRTITULOHST] ON [dbo].[IV_CBRTITULOHST] ([IDTITULO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CFinDocAceito
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CFinDocAceito] (
    [DocReqSeqPar] numeric(18,0) NOT NULL,
    [SeqDocTp] numeric(4,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CFinDocAceito__7DB96760] PRIMARY KEY CLUSTERED ([DocReqSeqPar], [SeqDocTp])
);
GO
CREATE NONCLUSTERED INDEX [IV_CFIndOCACEITIF2] ON [dbo].[IV_CFinDocAceito] ([SeqDocTp]);
GO
CREATE NONCLUSTERED INDEX [IV_CFINDOCACEITIF1] ON [dbo].[IV_CFinDocAceito] ([DocReqSeqPar]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Ciencia
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Ciencia] (
    [SeqHistorico] numeric(18,0) NOT NULL,
    [SeqCiencia] decimal(4,0) NOT NULL,
    [DtaLeitura] datetime NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Ciencia__77AB884F] PRIMARY KEY CLUSTERED ([SeqHistorico], [SeqCiencia])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CienciaPK] ON [dbo].[IV_Ciencia] ([SeqHistorico], [SeqCiencia]);
GO
CREATE NONCLUSTERED INDEX [IV_CienciaIF1] ON [dbo].[IV_Ciencia] ([SeqHistorico]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ClasseRes
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ClasseRes] (
    [SeqClasseRes] decimal(4,0) NOT NULL,
    [Classe] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Cor] numeric(18,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ClasseRes__789FAC88] PRIMARY KEY CLUSTERED ([SeqClasseRes])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ClasseResPK] ON [dbo].[IV_ClasseRes] ([SeqClasseRes]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ClienteAtrib
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ClienteAtrib] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Atributo] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Lista] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAtualizacao] datetime NULL,
    [DtaValidade] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ClienteAtrib__7993D0C1] PRIMARY KEY CLUSTERED ([SeqPessoa], [Atributo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ClienteAtribPK] ON [dbo].[IV_ClienteAtrib] ([SeqPessoa], [Atributo]);
GO
CREATE NONCLUSTERED INDEX [IV_ClienteAtribIF1] ON [dbo].[IV_ClienteAtrib] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ClientePropCmpl
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ClientePropCmpl] (
    [SeqPropPessoa] numeric(18,0) NOT NULL,
    [Complemento] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ClientePropCm__7A87F4FA] PRIMARY KEY CLUSTERED ([SeqPropPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ClientePropCPK] ON [dbo].[IV_ClientePropCmpl] ([SeqPropPessoa]);
GO
ALTER TABLE [dbo].[IV_ClientePropCmpl] ADD CONSTRAINT [FK__IV_Client__SeqPr__51B0C7F0] FOREIGN KEY ([SeqPropPessoa]) REFERENCES [dbo].[IV_ClientePropr] ([SeqPropPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ClientePropr
   Criada em ..: 2011-12-19
   Alterada em : 2019-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ClientePropr] (
    [SeqPropPessoa] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Referencia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Identificador] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ativo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Notas] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero1] decimal(15,2) NULL,
    [Numero2] decimal(15,2) NULL,
    [Numero3] decimal(15,2) NULL,
    [Numero4] decimal(15,2) NULL,
    [Numero5] decimal(15,2) NULL,
    [Numero6] decimal(15,2) NULL,
    [Data1] datetime NULL,
    [Data2] datetime NULL,
    [Data3] datetime NULL,
    [Data4] datetime NULL,
    [Data5] datetime NULL,
    [Data6] datetime NULL,
    [SimNao1] numeric(1,0) NULL,
    [SimNao2] numeric(1,0) NULL,
    [SimNao3] numeric(1,0) NULL,
    [SimNao4] numeric(1,0) NULL,
    [SimNao5] numeric(1,0) NULL,
    [SimNao6] numeric(1,0) NULL,
    [Literal1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO8] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7SQL] numeric(1,0) NULL,
    [CAMPO8SQL] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_ClientePropr__7B7C1933] PRIMARY KEY CLUSTERED ([SeqPropPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ClienteProprPK] ON [dbo].[IV_ClientePropr] ([SeqPropPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_ClienteProprIE1] ON [dbo].[IV_ClientePropr] ([Referencia]);
GO
CREATE NONCLUSTERED INDEX [IV_ClienteProprIE2] ON [dbo].[IV_ClientePropr] ([Identificador]);
GO
CREATE NONCLUSTERED INDEX [IV_ClienteProprIF1] ON [dbo].[IV_ClientePropr] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_ClienteProprIF2] ON [dbo].[IV_ClientePropr] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[IV_ClientePropr] ADD CONSTRAINT [FK__IV_Client__SeqPe__585DC57F] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ClientePropr] ADD CONSTRAINT [FK__IV_Client__SeqPr__5B052800] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[IV_ClientePropr] ADD CONSTRAINT [FK__IV_Client__SeqPr__53991062] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[IV_ClientePropr] ADD CONSTRAINT [FK__IV_Client__SeqPr__14C7B541] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ClientePropr_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ClientePropr_BKPJUN] (
    [SeqPropPessoa] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Referencia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Identificador] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ativo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Notas] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero1] decimal(15,2) NULL,
    [Numero2] decimal(15,2) NULL,
    [Numero3] decimal(15,2) NULL,
    [Numero4] decimal(15,2) NULL,
    [Numero5] decimal(15,2) NULL,
    [Numero6] decimal(15,2) NULL,
    [Data1] datetime NULL,
    [Data2] datetime NULL,
    [Data3] datetime NULL,
    [Data4] datetime NULL,
    [Data5] datetime NULL,
    [Data6] datetime NULL,
    [SimNao1] numeric(1,0) NULL,
    [SimNao2] numeric(1,0) NULL,
    [SimNao3] numeric(1,0) NULL,
    [SimNao4] numeric(1,0) NULL,
    [SimNao5] numeric(1,0) NULL,
    [SimNao6] numeric(1,0) NULL,
    [Literal1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO8] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7SQL] numeric(1,0) NULL,
    [CAMPO8SQL] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ClientePropr_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ClientePropr_ITA] (
    [SeqPropPessoa] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Referencia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Identificador] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ativo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Notas] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero1] decimal(15,2) NULL,
    [Numero2] decimal(15,2) NULL,
    [Numero3] decimal(15,2) NULL,
    [Numero4] decimal(15,2) NULL,
    [Numero5] decimal(15,2) NULL,
    [Numero6] decimal(15,2) NULL,
    [Data1] datetime NULL,
    [Data2] datetime NULL,
    [Data3] datetime NULL,
    [Data4] datetime NULL,
    [Data5] datetime NULL,
    [Data6] datetime NULL,
    [SimNao1] numeric(1,0) NULL,
    [SimNao2] numeric(1,0) NULL,
    [SimNao3] numeric(1,0) NULL,
    [SimNao4] numeric(1,0) NULL,
    [SimNao5] numeric(1,0) NULL,
    [SimNao6] numeric(1,0) NULL,
    [Literal1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltOrigem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO8] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7SQL] numeric(1,0) NULL,
    [CAMPO8SQL] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CobrCrit
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CobrCrit] (
    [Seq] decimal(4,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Acao] decimal(6,0) NULL,
    [Resultado] decimal(6,0) NULL,
    [Descricao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [VlrPr0] decimal(15,2) NULL,
    [VlrPr1] decimal(15,2) NULL,
    [Conexao] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CobrCrit__7C703D6C] PRIMARY KEY CLUSTERED ([Seq])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CobrCritPK] ON [dbo].[IV_CobrCrit] ([Seq]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrCritIF1] ON [dbo].[IV_CobrCrit] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrCritIF2] ON [dbo].[IV_CobrCrit] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrCritIF3] ON [dbo].[IV_CobrCrit] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_CobrCrit] ADD CONSTRAINT [FK__IV_CobrCr__Resul__17A421EC] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_CobrCrit] ADD CONSTRAINT [FK__IV_CobrCr__SeqUs__15BBD97A] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IV_CobrCrit] ADD CONSTRAINT [FK__IV_CobrCri__Acao__16AFFDB3] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]);
GO
ALTER TABLE [dbo].[IV_CobrCrit] ADD CONSTRAINT [FK__IV_CobrCr__SeqUs__5BF94C39] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IV_CobrCrit] ADD CONSTRAINT [FK__IV_CobrCri__Acao__5CED7072] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]);
GO
ALTER TABLE [dbo].[IV_CobrCrit] ADD CONSTRAINT [FK__IV_CobrCr__Resul__5DE194AB] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CobrCritAgd
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CobrCritAgd] (
    [SeqAgenda] numeric(18,0) NOT NULL,
    [Seq] decimal(4,0) NULL,
    [SeqCobCrit] numeric(18,0) NULL,
    [VlrAberto] decimal(15,2) NULL,
    [QtdTitulo] decimal(4,0) NULL,
    CONSTRAINT [PK__IV_CobrCritAgd__7D6461A5] PRIMARY KEY CLUSTERED ([SeqAgenda])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CobrCritAgdPK] ON [dbo].[IV_CobrCritAgd] ([SeqAgenda]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrCritAgdIF1] ON [dbo].[IV_CobrCritAgd] ([SeqCobCrit]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrCritAgdIF3] ON [dbo].[IV_CobrCritAgd] ([Seq]);
GO
ALTER TABLE [dbo].[IV_CobrCritAgd] ADD CONSTRAINT [FK__IV_CobrCr__SeqCo__5ED5B8E4] FOREIGN KEY ([SeqCobCrit]) REFERENCES [dbo].[IV_CobrCritMon] ([SeqCobCrit]);
GO
ALTER TABLE [dbo].[IV_CobrCritAgd] ADD CONSTRAINT [FK__IV_CobrCr__SeqAg__5FC9DD1D] FOREIGN KEY ([SeqAgenda]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_CobrCritAgd] ADD CONSTRAINT [FK__IV_CobrCrit__Seq__60BE0156] FOREIGN KEY ([Seq]) REFERENCES [dbo].[IV_CobrCrit] ([Seq]);
GO
ALTER TABLE [dbo].[IV_CobrCritAgd] ADD CONSTRAINT [FK__IV_CobrCr__SeqCo__18984625] FOREIGN KEY ([SeqCobCrit]) REFERENCES [dbo].[IV_CobrCritMon] ([SeqCobCrit]);
GO
ALTER TABLE [dbo].[IV_CobrCritAgd] ADD CONSTRAINT [FK__IV_CobrCrit__Seq__1A808E97] FOREIGN KEY ([Seq]) REFERENCES [dbo].[IV_CobrCrit] ([Seq]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CobrCritDef
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CobrCritDef] (
    [SeqParam] decimal(4,0) NOT NULL,
    [Seq] decimal(4,0) NOT NULL,
    [NroEmpresaOrigem] numeric(6,0) NULL,
    [Origem] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Especie] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoCobr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroDias] decimal(3,0) NULL,
    [NroDiasAte] decimal(4,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CobrCritDef__7E5885DE] PRIMARY KEY CLUSTERED ([SeqParam], [Seq])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CobrCritDefPK] ON [dbo].[IV_CobrCritDef] ([SeqParam], [Seq]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrCritDefIF1] ON [dbo].[IV_CobrCritDef] ([Seq]);
GO
ALTER TABLE [dbo].[IV_CobrCritDef] ADD CONSTRAINT [FK__IV_CobrCrit__Seq__61B2258F] FOREIGN KEY ([Seq]) REFERENCES [dbo].[IV_CobrCrit] ([Seq]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CobrCritMon
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CobrCritMon] (
    [SeqCobCrit] numeric(18,0) NOT NULL,
    [Seq] decimal(4,0) NOT NULL,
    [DtaAnalise] datetime NOT NULL,
    [DtaUltAnalise] datetime NULL,
    [VlrNovo] decimal(15,2) NULL,
    [VlrBaixado] decimal(15,2) NULL,
    [VlrAlterado] decimal(15,2) NULL,
    [VlrMigrado] decimal(15,2) NULL,
    [QtdTitNovo] decimal(6,0) NULL,
    [QtdTitBaixado] decimal(6,0) NULL,
    [QtdTitAlterado] decimal(6,0) NULL,
    [QtdTitMIgrado] decimal(6,0) NULL,
    [QtdPesNovo] decimal(6,0) NULL,
    [QtdPesBaixado] decimal(6,0) NULL,
    [QtdPesAlterado] decimal(6,0) NULL,
    [QtdPesMIgrado] decimal(6,0) NULL,
    [UsuAnalise] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CobrCritMon__7F4CAA17] PRIMARY KEY CLUSTERED ([SeqCobCrit])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CobrCritMonPK] ON [dbo].[IV_CobrCritMon] ([SeqCobCrit]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrCritMonIF1] ON [dbo].[IV_CobrCritMon] ([Seq]);
GO
ALTER TABLE [dbo].[IV_CobrCritMon] ADD CONSTRAINT [FK__IV_CobrCrit__Seq__62A649C8] FOREIGN KEY ([Seq]) REFERENCES [dbo].[IV_CobrCrit] ([Seq]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CobrTit
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CobrTit] (
    [IdTitulo] numeric(18,0) NOT NULL,
    [Seq] decimal(4,0) NOT NULL,
    [Processo] numeric(18,0) NULL,
    [DtaVencto] datetime NULL,
    [VlrAberto] decimal(15,2) NULL,
    [Quitado] numeric(1,0) NULL,
    [NroEmpresa] decimal(6,0) NULL,
    [Titulo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAnalise] datetime NOT NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_CobrTit__0040CE50] PRIMARY KEY CLUSTERED ([IdTitulo], [Seq])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CobrTitPK] ON [dbo].[IV_CobrTit] ([IdTitulo], [Seq]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrTitIE1] ON [dbo].[IV_CobrTit] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_CobrTitIF1] ON [dbo].[IV_CobrTit] ([Seq]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CodPrcEmpr
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CodPrcEmpr] (
    [NroEmpresa] numeric(6,0) NOT NULL,
    [CodProcesso] decimal(4,0) NOT NULL,
    CONSTRAINT [PK__IV_CodPrcEmpr__0134F289] PRIMARY KEY CLUSTERED ([NroEmpresa], [CodProcesso])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CodPrcEmprPK] ON [dbo].[IV_CodPrcEmpr] ([NroEmpresa], [CodProcesso]);
GO
CREATE NONCLUSTERED INDEX [IV_CodPrcEmprIF1] ON [dbo].[IV_CodPrcEmpr] ([NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IV_CodPrcEmprIF2] ON [dbo].[IV_CodPrcEmpr] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_CodPrcEmpr] ADD CONSTRAINT [FK__IV_CodPrc__NroEm__1E511F7B] FOREIGN KEY ([NroEmpresa]) REFERENCES [dbo].[GE_Empresa] ([NroEmpresa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_CodPrcEmpr] ADD CONSTRAINT [FK__IV_CodPrc__CodPr__6582B673] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CodProcComent
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CodProcComent] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [SeqProcComent] decimal(2,0) NOT NULL,
    [ProcComent] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Cor] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_CodProcComent__022916C2] PRIMARY KEY CLUSTERED ([CodProcesso], [SeqProcComent])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CodProcComenPK] ON [dbo].[IV_CodProcComent] ([CodProcesso], [SeqProcComent]);
GO
CREATE NONCLUSTERED INDEX [IV_CodProcComenIF1] ON [dbo].[IV_CodProcComent] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_CodProcComent] ADD CONSTRAINT [FK__IV_CodPro__CodPr__6676DAAC] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CodProcesso
   Criada em ..: 2011-12-19
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CodProcesso] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescrRed] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    [TipoProcesso] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsaPerspectiva] numeric(1,0) NULL,
    [UsaPercConclusao] numeric(1,0) NULL,
    [UsaValor] numeric(1,0) NULL,
    [UsaMaterial] numeric(1,0) NULL,
    [UsaStatus] numeric(1,0) NULL,
    [UsaStatusDes] numeric(1,0) NULL,
    [UsaResumo] numeric(1,0) NULL,
    [UsaFichCad] numeric(1,0) NULL,
    [DescValor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescPersp] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsaQtde] numeric(1,0) NULL,
    [DescQtde] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsaProduto] numeric(1,0) NULL,
    [ProdFamilia] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorLinha] numeric(18,0) NULL,
    [AcaoProsp] decimal(6,0) NULL,
    [AcaoAcomp] decimal(6,0) NULL,
    [AcaoVenda] decimal(6,0) NULL,
    [FaseEnvioERP] decimal(2,0) NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [CriaFichaNeg] numeric(1,0) NULL,
    [ResProrrConcl] numeric(18,0) NULL,
    [QTDPRODUTO] numeric(4,0) NULL,
    [QTDPRODUTOUND] numeric(4,0) NULL,
    [INDFASEBASEACAO] numeric(1,0) NULL,
    [INDUSABOARD] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_CodProcesso__031D3AFB] PRIMARY KEY CLUSTERED ([CodProcesso])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CodProcessoPK] ON [dbo].[IV_CodProcesso] ([CodProcesso]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ConhecFonema
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ConhecFonema] (
    [SeqConh] decimal(6,0) NOT NULL,
    [Particula] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__IV_ConhecFonema__04115F34] PRIMARY KEY CLUSTERED ([SeqConh], [Particula])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ConhecFonemaPK] ON [dbo].[IV_ConhecFonema] ([SeqConh], [Particula]);
GO
CREATE NONCLUSTERED INDEX [IV_ConhecFonemaIF1] ON [dbo].[IV_ConhecFonema] ([SeqConh]);
GO
ALTER TABLE [dbo].[IV_ConhecFonema] ADD CONSTRAINT [FK__IV_Conhec__SeqCo__676AFEE5] FOREIGN KEY ([SeqConh]) REFERENCES [dbo].[IV_Conhecimento] ([SeqConh]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Conhecimento
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Conhecimento] (
    [SeqConh] decimal(6,0) NOT NULL,
    [SeqConhPai] decimal(6,0) NULL,
    [Descricao] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Conteudo] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atalho] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdeAcesso] numeric(18,0) NULL,
    [DtaUltAcesso] datetime NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoArquivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AtalhoPcte] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PermiteAnexar] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PalavraChave] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Versao] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Dtainclusao] datetime NULL,
    [Usuinclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Resultado] decimal(6,0) NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Conhecimento__0505836D] PRIMARY KEY CLUSTERED ([SeqConh])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ConhecimentoPK] ON [dbo].[IV_Conhecimento] ([SeqConh]);
GO
CREATE NONCLUSTERED INDEX [IV_ConhecimentoIE1] ON [dbo].[IV_Conhecimento] ([SeqConhPai]);
GO
CREATE NONCLUSTERED INDEX [IV_ConhecimentoIF1] ON [dbo].[IV_Conhecimento] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_Conhecimento] ADD CONSTRAINT [FK__IV_Conhec__Resul__2221B05F] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_Conhecimento] ADD CONSTRAINT [FK__IV_Conhec__Resul__685F231E] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ConhecLeitura
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ConhecLeitura] (
    [SeqConh] decimal(6,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Lido] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaUltAcesso] datetime NULL,
    [QtdeAcesso] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_ConhecLeitura__05F9A7A6] PRIMARY KEY CLUSTERED ([SeqConh], [SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ConhecLeiturPK] ON [dbo].[IV_ConhecLeitura] ([SeqConh], [SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_ConhecLeiturIF1] ON [dbo].[IV_ConhecLeitura] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_ConhecLeiturIF2] ON [dbo].[IV_ConhecLeitura] ([SeqConh]);
GO
ALTER TABLE [dbo].[IV_ConhecLeitura] ADD CONSTRAINT [FK__IV_Conhec__SeqCo__6A476B90] FOREIGN KEY ([SeqConh]) REFERENCES [dbo].[IV_Conhecimento] ([SeqConh]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ConhecLeitura] ADD CONSTRAINT [FK__IV_Conhec__SeqUs__69534757] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_CustoMidia
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_CustoMidia] (
    [SeqCustoCamp] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Campanha] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaBase] datetime NULL,
    [MesBase] datetime NULL,
    [Obs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Valor] decimal(15,2) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_CustoMidia__06EDCBDF] PRIMARY KEY CLUSTERED ([SeqCustoCamp])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_CustoMidiaPK] ON [dbo].[IV_CustoMidia] ([SeqCustoCamp]);
GO
CREATE NONCLUSTERED INDEX [IV_CustoMidiaIE1] ON [dbo].[IV_CustoMidia] ([Campanha], [DtaBase]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Departamento
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Departamento] (
    [Departamento] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqDepartamento] int NULL,
    CONSTRAINT [PK__IV_Departamento__07E1F018] PRIMARY KEY CLUSTERED ([Departamento])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_DepartamentoPK] ON [dbo].[IV_Departamento] ([Departamento]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Distribui
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Distribui] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [Acao] decimal(6,0) NOT NULL,
    [UltimoUsuario] decimal(6,0) NULL,
    CONSTRAINT [PK__IV_Distribui__08D61451] PRIMARY KEY CLUSTERED ([SeqUsuario], [Acao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_DistribuiPK] ON [dbo].[IV_Distribui] ([SeqUsuario], [Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_DistribuiIF1] ON [dbo].[IV_Distribui] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_DistribuiIF2] ON [dbo].[IV_Distribui] ([Acao]);
GO
ALTER TABLE [dbo].[IV_Distribui] ADD CONSTRAINT [FK__IV_Distri__SeqUs__6B3B8FC9] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_Distribui] ADD CONSTRAINT [FK__IV_Distrib__Acao__6C2FB402] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_DoctoApl
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_DoctoApl] (
    [SeqDoctoApl] numeric(18,0) NOT NULL,
    [Docto] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Atividade] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoAplicativo] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LocalAplicativo] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Aplicativo] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ParamInicial] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Param] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ParamFinal] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    [Sessao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_DoctoApl__09CA388A] PRIMARY KEY CLUSTERED ([SeqDoctoApl])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_DoctoAplPK] ON [dbo].[IV_DoctoApl] ([SeqDoctoApl]);
GO
CREATE NONCLUSTERED INDEX [IV_DoctoAplIF1] ON [dbo].[IV_DoctoApl] ([Docto]);
GO
ALTER TABLE [dbo].[IV_DoctoApl] ADD CONSTRAINT [FK__IV_DoctoA__Docto__6D23D83B] FOREIGN KEY ([Docto]) REFERENCES [dbo].[IV_DoctoTipo] ([Docto]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_DoctoAplUso
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_DoctoAplUso] (
    [SeqDoctoAplUso] numeric(18,0) NOT NULL,
    [SeqDoctoApl] numeric(18,0) NULL,
    [Uso] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Referencia] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Chave] numeric(18,0) NULL,
    [EmUso] numeric(1,0) NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_DoctoAplUso__0ABE5CC3] PRIMARY KEY CLUSTERED ([SeqDoctoAplUso])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_DoctoAplUsoPK] ON [dbo].[IV_DoctoAplUso] ([SeqDoctoAplUso]);
GO
CREATE NONCLUSTERED INDEX [IV_DoctoAplUsoIF1] ON [dbo].[IV_DoctoAplUso] ([SeqDoctoApl]);
GO
ALTER TABLE [dbo].[IV_DoctoAplUso] ADD CONSTRAINT [FK__IV_DoctoA__SeqDo__6E17FC74] FOREIGN KEY ([SeqDoctoApl]) REFERENCES [dbo].[IV_DoctoApl] ([SeqDoctoApl]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_DoctoTipo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_DoctoTipo] (
    [Docto] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndIncLinkManual] numeric(1,0) NULL,
    [IndVerLink] numeric(1,0) NULL,
    [SeqDoctoTipo] numeric(4,0) NULL,
    CONSTRAINT [PK__IV_DoctoTipo__0BB280FC] PRIMARY KEY CLUSTERED ([Docto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_DoctoTipoPK] ON [dbo].[IV_DoctoTipo] ([Docto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_eMail
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_eMail] (
    [SeqEmail] numeric(18,0) NOT NULL,
    [SeqHistorico] numeric(18,0) NULL,
    [SeqPessoa] numeric(8,0) NULL,
    [De] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Para] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cc] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cco] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Assunto] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Detalhe] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Arquivo] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Enviado] numeric(1,0) NULL,
    [FormaEnvio] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [DtaEnvio] datetime NULL,
    CONSTRAINT [PK__IV_eMail__0CA6A535] PRIMARY KEY CLUSTERED ([SeqEmail])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_eMailPK] ON [dbo].[IV_eMail] ([SeqEmail]);
GO
CREATE NONCLUSTERED INDEX [IV_eMailIF1] ON [dbo].[IV_eMail] ([SeqHistorico]);
GO
CREATE NONCLUSTERED INDEX [IV_eMailIF2] ON [dbo].[IV_eMail] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IV_eMail] ADD CONSTRAINT [FK__IV_eMail__SeqHis__6F0C20AD] FOREIGN KEY ([SeqHistorico]) REFERENCES [dbo].[IV_Historico] ([SeqHistorico]);
GO
ALTER TABLE [dbo].[IV_eMail] ADD CONSTRAINT [FK__IV_eMail__SeqHis__28CEADEE] FOREIGN KEY ([SeqHistorico]) REFERENCES [dbo].[IV_Historico] ([SeqHistorico]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_EMAILESTATISTICA
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_EMAILESTATISTICA] (
    [SEQEMAILESTATISTICA] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SEQCONTAEMAIL] numeric(18,0) NULL,
    [CONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUBCONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINKSTR] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DE] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ASSUNTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAENVIO] datetime NULL,
    [DTALEITURA] datetime NULL,
    [DTACLIQUE] datetime NULL,
    [INDSPAM] numeric(1,0) NULL,
    [INDDESCADASTRO] numeric(1,0) NULL,
    [INDENTREGUE] numeric(1,0) NULL,
    [INDENVIADO] numeric(1,0) NULL,
    [INDLIDO] numeric(1,0) NULL,
    [INDCLIQUE] numeric(1,0) NULL,
    [INDERRO] numeric(1,0) NULL,
    [DTAGERACAO] datetime NULL,
    [DTAULTATUALIZACAO] datetime NULL,
    CONSTRAINT [PK__IV_EMAIL__2AE5C942FDA2751A] PRIMARY KEY CLUSTERED ([SEQEMAILESTATISTICA])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IVEMAILESTATISTICA_0] ON [dbo].[IV_EMAILESTATISTICA] ([CONTEXTO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVEMAILESTATISTICA_1] ON [dbo].[IV_EMAILESTATISTICA] ([CONTEXTO], [SUBCONTEXTO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVEMAILESTATISTICA_2] ON [dbo].[IV_EMAILESTATISTICA] ([DTAENVIO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IVEMAILESTATISTICA_3] ON [dbo].[IV_EMAILESTATISTICA] ([LINKSTR]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ESTRPRODUTO
   Criada em ..: 2020-07-14
   Alterada em : 2020-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ESTRPRODUTO] (
    [SEQESTRPROD] numeric(10,0) NOT NULL,
    [NIVEL] numeric(1,0) NOT NULL,
    [DESCRICAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SEQESTRPRODPAI] numeric(10,0) NULL,
    [TIPOESTRUTURA] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DESCCOMPLETA] varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ESTRP__6EF3AC1E6310D093] PRIMARY KEY CLUSTERED ([SEQESTRPROD])
);
GO
ALTER TABLE [dbo].[IV_ESTRPRODUTO] ADD CONSTRAINT [FK__IV_ESTRPR__SEQES__176FE319] FOREIGN KEY ([SEQESTRPRODPAI]) REFERENCES [dbo].[IV_ESTRPRODUTO] ([SEQESTRPROD]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Evento
   Criada em ..: 2011-12-19
   Alterada em : 2019-09-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Evento] (
    [SeqEvento] decimal(8,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Evento] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [NroEmpresaDestino] numeric(6,0) NULL,
    [ResultAtivo] numeric(6,0) NULL,
    [GeraAndSempre] numeric(1,0) NULL,
    [ResultRec] numeric(6,0) NULL,
    [ResultRecPE] numeric(6,0) NULL,
    [GeraRecSempre] numeric(1,0) NULL,
    [AtuHistProcesso] numeric(1,0) NULL,
    [Descricao] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DetalheAuto] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ProcedureAdicional] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ProcessoUnico] numeric(1,0) NULL,
    [GeraAcao] numeric(1,0) NULL,
    [ExcluiProcesso] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsTec] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [DtaUltUso] datetime NULL,
    [DescAcaoOutroProc] numeric(1,0) NULL,
    [RESULTRECHSTE] numeric(6,0) NULL,
    [RESULTRECHSTNE] numeric(6,0) NULL,
    [DESCACAOOUTRAEMPR] numeric(1,0) NULL,
    [EMUSO] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_Evento__0E8EEDA7] PRIMARY KEY CLUSTERED ([SeqEvento])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_EventoPK] ON [dbo].[IV_Evento] ([SeqEvento]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_EventoAK1] ON [dbo].[IV_Evento] ([NroEmpresa], [Origem], [Evento]);
GO
CREATE NONCLUSTERED INDEX [IV_EventoIE1] ON [dbo].[IV_Evento] ([ResultAtivo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_EventoAcao
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_EventoAcao] (
    [SeqEventoAcao] numeric(18,0) NOT NULL,
    [SeqEvento] numeric(8,0) NOT NULL,
    [ACAO] numeric(6,0) NOT NULL,
    [AttdeHaAndamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ResHaAndamento] numeric(6,0) NULL,
    [AttdeNaoHaAndamento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ResNaoHaAndamento] numeric(6,0) NULL,
    CONSTRAINT [PK__IV_Event__E8B08E982DB60A6A] PRIMARY KEY CLUSTERED ([SeqEventoAcao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_EventoAcao] ON [dbo].[IV_EventoAcao] ([SeqEventoAcao]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_EventoAcao] ON [dbo].[IV_EventoAcao] ([SeqEvento]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_EventoAcao] ON [dbo].[IV_EventoAcao] ([ACAO]);
GO
CREATE NONCLUSTERED INDEX [XIF3IV_EventoAcao] ON [dbo].[IV_EventoAcao] ([ResHaAndamento]);
GO
CREATE NONCLUSTERED INDEX [XIF4IV_EventoAcao] ON [dbo].[IV_EventoAcao] ([ResNaoHaAndamento]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_FichaNegVeic
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_FichaNegVeic] (
    [Processo] numeric(18,0) NOT NULL,
    [Filial] decimal(6,0) NULL,
    [PedVenda] decimal(10,0) NULL,
    [DtaFaturamento] datetime NULL,
    [SeqPlanoMelhor] numeric(18,0) NULL,
    [Avaliacao] decimal(10,0) NULL,
    [TipoVeiculo] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoPgto] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NovoUsado] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ApresentDinamica] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ApresentEstatica] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MarcaInteresse] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FamiliaInteresse] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ModeloInteresse] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IntAnoFabr] decimal(4,0) NULL,
    [IntAnoMod] decimal(4,0) NULL,
    [MarcaEscolhida] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ModeloEscolhido] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodModEscolhido] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VlrInteresseDe] decimal(15,2) NULL,
    [VlrInteresseAte] decimal(15,2) NULL,
    [Cor1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Cor2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CorInterna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroPortas] decimal(1,0) NULL,
    [VlrTabela] decimal(15,2) NULL,
    [VlrProposto] decimal(15,2) NULL,
    [VlrEntrada] decimal(15,2) NULL,
    [VlrPrestacao] decimal(15,2) NULL,
    [VlrSeguro] decimal(15,2) NULL,
    [VlrGarExtendida] decimal(15,2) NULL,
    [VlrTC] decimal(15,2) NULL,
    [QtdePrestacao] decimal(4,0) NULL,
    [UsadoMarca] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoModelo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoCor] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoAnoFabr] decimal(4,0) NULL,
    [UsadoAnoMod] decimal(4,0) NULL,
    [UsadoNroPortas] decimal(1,0) NULL,
    [UsadoVlrPre] decimal(15,2) NULL,
    [UsadoVlrAvalCct] decimal(15,2) NULL,
    [UsadoObs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [VlrCusto] decimal(15,2) NULL,
    [VlrMargem] decimal(15,2) NULL,
    [Financeira] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqFinanc] decimal(6,0) NULL,
    [Placa] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Chassi] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Negociacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Opcionais] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ObsNegociacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoChassi] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoPlaca] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndVeicUsadoTroca] numeric(1,0) NULL,
    [UsadoCombustivel] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoVlrTroca] decimal(15,2) NULL,
    [Combustivel] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoRenavam] varchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsadoNroMotor] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADOKM] numeric(8,0) NULL,
    [TIPOVENDA] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADOMARCA2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADOMODELO2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADORENAVAM2] varchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADOPLACA2] varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADOKM2] numeric(8,0) NULL,
    [USADOANOFABR2] numeric(4,0) NULL,
    [USADOANOMOD2] numeric(4,0) NULL,
    [USADOCOR2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADONROPORTAS2] numeric(1,0) NULL,
    [USADOVLRPRE2] numeric(14,2) NULL,
    [USADOVLRAVALCCT2] numeric(14,2) NULL,
    [USADOVLRTROCA2] numeric(14,2) NULL,
    [USADOOBS2] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADOCOMBUSTIVEL2] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADOCHASSI2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADONROMOTOR2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_FichaNegVeic__0F8311E0] PRIMARY KEY CLUSTERED ([Processo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_FichaNegVeicPK] ON [dbo].[IV_FichaNegVeic] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_FichaNegVeicIE1] ON [dbo].[IV_FichaNegVeic] ([Filial], [PedVenda]);
GO
CREATE NONCLUSTERED INDEX [IV_FichaNegVeicIE2] ON [dbo].[IV_FichaNegVeic] ([Filial], [Avaliacao]);
GO
CREATE NONCLUSTERED INDEX [IV_FichaNegVeicIE3] ON [dbo].[IV_FichaNegVeic] ([SeqPlanoMelhor]);
GO
CREATE NONCLUSTERED INDEX [IV_FichaNegVeicIF1] ON [dbo].[IV_FichaNegVeic] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IV_FichaNegVeic] ADD CONSTRAINT [FK__IV_FichaN__SeqFi__2AB6F660] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO
ALTER TABLE [dbo].[IV_FichaNegVeic] ADD CONSTRAINT [FK__IV_FichaN__SeqFi__70F4691F] FOREIGN KEY ([SeqFinanc]) REFERENCES [dbo].[IVF_Financeira] ([SeqFinanc]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_FoneCtrl
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_FoneCtrl] (
    [FoneNro] decimal(12,0) NOT NULL,
    [FoneDDD] decimal(2,0) NOT NULL,
    [Bloqueado] numeric(1,0) NULL,
    [Status] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaBloqueio] datetime NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_FoneCtrl__10773619] PRIMARY KEY CLUSTERED ([FoneNro], [FoneDDD])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_FoneCtrlPK] ON [dbo].[IV_FoneCtrl] ([FoneNro], [FoneDDD]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_FoneCtrl2
   Criada em ..: 2013-03-24
   Alterada em : 2014-09-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_FoneCtrl2] (
    [FoneNro] decimal(12,0) NOT NULL,
    [FoneDDD] decimal(2,0) NOT NULL,
    [Bloqueado] numeric(1,0) NULL,
    [Status] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaBloqueio] datetime NULL,
    [DtaAlteracao] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_FoneCtrlHst
   Criada em ..: 2013-03-24
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_FoneCtrlHst] (
    [FoneNro] decimal(12,0) NOT NULL,
    [FoneDDD] decimal(2,0) NOT NULL,
    [SeqHst] decimal(4,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [Operacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_FoneCtrlHst__116B5A52] PRIMARY KEY CLUSTERED ([FoneNro], [FoneDDD], [SeqHst])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_FoneCtrlHstPK] ON [dbo].[IV_FoneCtrlHst] ([FoneNro], [FoneDDD], [SeqHst]);
GO
CREATE NONCLUSTERED INDEX [IV_FoneCtrlHstIF1] ON [dbo].[IV_FoneCtrlHst] ([FoneNro], [FoneDDD]);
GO
ALTER TABLE [dbo].[IV_FoneCtrlHst] ADD CONSTRAINT [FK__IV_FoneCtrlHst__71E88D58] FOREIGN KEY ([FoneNro], [FoneDDD]) REFERENCES [dbo].[IV_FoneCtrl] ([FoneNro], [FoneDDD]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Formulario
   Criada em ..: 2011-12-19
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Formulario] (
    [SeqFormulario] numeric(18,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Objetivo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Script] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PrimeiraQuestao] decimal(2,0) NULL,
    [EmUso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Layout] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsaObs] numeric(1,0) NULL,
    [QuestaoGuia] decimal(4,0) NULL,
    [Restricao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndUsoPessoa] numeric(1,0) NULL,
    [INDUSOPROJETO] numeric(1,0) NULL,
    [INDUSOPROPRIEDADE] numeric(1,0) NULL,
    [INDUMPORPESSOA] numeric(1,0) NULL,
    [INDUSAASSINATURA] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_Formulario__39237A9A] PRIMARY KEY CLUSTERED ([SeqFormulario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_FormularioPK] ON [dbo].[IV_Formulario] ([SeqFormulario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_GlobalPar
   Criada em ..: 2011-12-19
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_GlobalPar] (
    [SeqPar] numeric(18,0) NOT NULL,
    [SeqGlbPar] numeric(4,0) NOT NULL,
    [NroEmpresa] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero1] decimal(15,2) NULL,
    [Numero2] decimal(15,2) NULL,
    [Numero3] decimal(15,2) NULL,
    [Numero4] decimal(15,2) NULL,
    [Numero5] decimal(15,2) NULL,
    [Numero6] decimal(15,2) NULL,
    [Data1] datetime NULL,
    [Data2] datetime NULL,
    [Data3] datetime NULL,
    [Data4] datetime NULL,
    [Data5] datetime NULL,
    [Data6] datetime NULL,
    [Literal1] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao1] numeric(1,0) NULL,
    [SimNao2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao6] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao7] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao8] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao9] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao10] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_GlobalPar__125F7E8B] PRIMARY KEY CLUSTERED ([SeqPar])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_GlobalParPK] ON [dbo].[IV_GlobalPar] ([SeqPar]);
GO
CREATE NONCLUSTERED INDEX [IV_GlobalParIF1] ON [dbo].[IV_GlobalPar] ([SeqGlbPar]);
GO
CREATE NONCLUSTERED INDEX [IV_GLOBALPAR_IDX1] ON [dbo].[IV_GlobalPar] ([SimNao1], [SeqGlbPar], [Campo1]) INCLUDE ([SeqPar]);
GO
ALTER TABLE [dbo].[IV_GlobalPar] ADD CONSTRAINT [FK__IV_Global__SeqGl__72DCB191] FOREIGN KEY ([SeqGlbPar]) REFERENCES [dbo].[IV_GlobalParCtrl] ([SeqGlbPar]);
GO
ALTER TABLE [dbo].[IV_GlobalPar] ADD CONSTRAINT [FK__IV_Global__SeqGl__2C9F3ED2] FOREIGN KEY ([SeqGlbPar]) REFERENCES [dbo].[IV_GlobalParCtrl] ([SeqGlbPar]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_GlobalParCtrl
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_GlobalParCtrl] (
    [SeqGlbPar] numeric(4,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Parametro] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PorEmpresa] numeric(1,0) NOT NULL,
    [Unico] numeric(1,0) NOT NULL,
    [CampoListar] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Instrucao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CampoListar2] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao6] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao7] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao8] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao9] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao10] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1Sql] smallint NULL,
    [Campo2Sql] smallint NULL,
    [Campo3Sql] smallint NULL,
    [Campo4Sql] smallint NULL,
    [Campo5Sql] smallint NULL,
    [Campo6Sql] smallint NULL,
    CONSTRAINT [PK__IV_GlobalParCtrl__1353A2C4] PRIMARY KEY CLUSTERED ([SeqGlbPar])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_GlobalParCtrPK] ON [dbo].[IV_GlobalParCtrl] ([SeqGlbPar]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_GlobalParLista
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_GlobalParLista] (
    [SeqGlbPar] numeric(4,0) NOT NULL,
    [Campo] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPropriLista] decimal(4,0) NOT NULL,
    [Lista] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_GlobalParList__1447C6FD] PRIMARY KEY CLUSTERED ([SeqGlbPar], [Campo], [SeqPropriLista])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_GlobalParLisPK] ON [dbo].[IV_GlobalParLista] ([SeqGlbPar], [Campo], [SeqPropriLista]);
GO
CREATE NONCLUSTERED INDEX [IV_GlobalParLisIF1] ON [dbo].[IV_GlobalParLista] ([SeqGlbPar]);
GO
ALTER TABLE [dbo].[IV_GlobalParLista] ADD CONSTRAINT [FK__IV_Global__SeqGl__73D0D5CA] FOREIGN KEY ([SeqGlbPar]) REFERENCES [dbo].[IV_GlobalParCtrl] ([SeqGlbPar]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_HistInfo
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_HistInfo] (
    [SeqHistorico] numeric(18,0) NOT NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_HistInfo__6A7188C2] PRIMARY KEY CLUSTERED ([SeqHistorico])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XIF1IV_HistInfo] ON [dbo].[IV_HistInfo] ([SeqHistorico]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_HistLink
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_HistLink] (
    [SeqHistorico] numeric(18,0) NOT NULL,
    [LinkSerie] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkDocto] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkNroEmpresa] numeric(6,0) NULL,
    CONSTRAINT [PK__IV_HistLink__153BEB36] PRIMARY KEY CLUSTERED ([SeqHistorico])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_HistLinkPK] ON [dbo].[IV_HistLink] ([SeqHistorico]);
GO
CREATE NONCLUSTERED INDEX [IV_HistLinkIE1] ON [dbo].[IV_HistLink] ([LinkSerie]);
GO
CREATE NONCLUSTERED INDEX [IV_HistLinkIE2] ON [dbo].[IV_HistLink] ([LinkNro]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Historico
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Historico] (
    [SeqHistorico] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Contato] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [SeqUsuario] int NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AcaoGeradora] numeric(6,0) NULL,
    [Departamento] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Resultado] decimal(6,0) NOT NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AgendaOrigem] int NULL,
    [DtaRealizacao] datetime NOT NULL,
    [Detalhe] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Natureza] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Vendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaPrimCont] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] numeric(18,0) NULL,
    [CodProcesso] decimal(4,0) NULL,
    [Valor] decimal(15,2) NULL,
    [Qtde] numeric(17,2) NULL,
    [Duracao] decimal(4,0) NULL,
    [TemCiencia] numeric(1,0) NULL,
    [Latitude] numeric(14,11) NULL,
    [Longitude] numeric(14,11) NULL,
    [SEQPESSOACTTO] numeric(10,0) NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    CONSTRAINT [PK__IV_Historico__16300F6F] PRIMARY KEY CLUSTERED ([SeqHistorico])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_HistoricoPK] ON [dbo].[IV_Historico] ([SeqHistorico]);
GO
CREATE NONCLUSTERED INDEX [IV_HistoricoIE2] ON [dbo].[IV_Historico] ([DtaRealizacao]);
GO
CREATE NONCLUSTERED INDEX [IV_HistoricoIE5] ON [dbo].[IV_Historico] ([AgendaOrigem]);
GO
CREATE NONCLUSTERED INDEX [IV_HistoricoIE6] ON [dbo].[IV_Historico] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_HistoricoIF1] ON [dbo].[IV_Historico] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_HistoricoIF2] ON [dbo].[IV_Historico] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [IV_HISTORICOIE7] ON [dbo].[IV_Historico] ([SeqPessoa], [DtaRealizacao]);
GO
ALTER TABLE [dbo].[IV_Historico] ADD CONSTRAINT [FK__IV_Histor__Resul__76AD4275] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_Historico] ADD CONSTRAINT [FK__IV_Histor__Resul__306FCFB6] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_Historico] ADD CONSTRAINT [FK__IV_Histor__SeqPe__5769A146] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_HistoricoNota
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_HistoricoNota] (
    [SeqHistorico] numeric(18,0) NOT NULL,
    [Nota] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_HistoricoNota__172433A8] PRIMARY KEY CLUSTERED ([SeqHistorico])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_HistoricoNotPK] ON [dbo].[IV_HistoricoNota] ([SeqHistorico]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_HISTORICOTAG
   Criada em ..: 2023-06-05
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_HISTORICOTAG] (
    [SEQHISTORICO] numeric(18,0) NOT NULL,
    [TAG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTAGERACAO] datetime NULL,
    [USUGERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_HISTO__BBA2F64BB6EF4044] PRIMARY KEY CLUSTERED ([SEQHISTORICO], [TAG])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_HISTORICO] ON [dbo].[IV_HISTORICOTAG] ([TAG]);
GO
ALTER TABLE [dbo].[IV_HISTORICOTAG] ADD CONSTRAINT [FK__IV_HISTOR__SEQHI__5B10E04F] FOREIGN KEY ([SEQHISTORICO]) REFERENCES [dbo].[IV_Historico] ([SeqHistorico]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Interacao
   Criada em ..: 2015-09-14
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Interacao] (
    [SeqHistorico] numeric(18,0) NOT NULL,
    [DtaAgenda] datetime NULL,
    [DtaAgendaFinal] datetime NULL,
    [DtaLimiteExecucao] datetime NULL,
    CONSTRAINT [PK__IV_Interacao__135E8FBC] PRIMARY KEY CLUSTERED ([SeqHistorico])
);
GO
CREATE NONCLUSTERED INDEX [XIE1IV_Interacao] ON [dbo].[IV_Interacao] ([DtaAgenda]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XIF1IV_Interacao] ON [dbo].[IV_Interacao] ([SeqHistorico]);
GO
ALTER TABLE [dbo].[IV_Interacao] ADD CONSTRAINT [FK__IV_Intera__SeqHi__1452B3F5] FOREIGN KEY ([SeqHistorico]) REFERENCES [dbo].[IV_Historico] ([SeqHistorico]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_LEADFACEBOOK
   Criada em ..: 2021-06-02
   Alterada em : 2021-06-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_LEADFACEBOOK] (
    [SEQLEADFACEBOOK] numeric(18,0) NOT NULL,
    [CAMPANHA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANUNCIO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTALEAD] datetime NULL,
    [PLATAFORMA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAIL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CPF] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENDERECO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CIDADE] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESTADO] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEP] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTANASCIMENTO] datetime NULL,
    [SEXO] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESTADOCIVIL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROFISSAO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FONECOMERCIAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAILCOMERCIAL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOMEEMPRESA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LEADID] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_LEADF__33741ABA26549B4D] PRIMARY KEY CLUSTERED ([SEQLEADFACEBOOK])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_LEADFACEITEM
   Criada em ..: 2021-06-02
   Alterada em : 2021-06-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_LEADFACEITEM] (
    [SEQLEADFACEBOOK] numeric(18,0) NOT NULL,
    [SEQLEADITEM] numeric(2,0) NOT NULL,
    [PERGUNTA] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPODADO] varchar(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RESPOSTATEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RESPOSTADATA] datetime NULL,
    CONSTRAINT [PK__IV_LEADF__ED6B74DA9F666308] PRIMARY KEY CLUSTERED ([SEQLEADFACEBOOK], [SEQLEADITEM])
);
GO
ALTER TABLE [dbo].[IV_LEADFACEITEM] ADD CONSTRAINT [FK__IV_LEADFA__SEQLE__3D6081D7] FOREIGN KEY ([SEQLEADFACEBOOK]) REFERENCES [dbo].[IV_LEADFACEBOOK] ([SEQLEADFACEBOOK]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ListSQL
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ListSQL] (
    [Tipo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqMain] numeric(18,0) NOT NULL,
    [SeqSub] numeric(18,0) NOT NULL,
    [Sql] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Sessao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ListSQL__181857E1] PRIMARY KEY CLUSTERED ([Tipo], [SeqMain], [SeqSub])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ListSQLPK] ON [dbo].[IV_ListSQL] ([Tipo], [SeqMain], [SeqSub]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Motivo
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Motivo] (
    [Motivo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqMotivo] numeric(18,0) NULL,
    [Link] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Motivo__190C7C1A] PRIMARY KEY CLUSTERED ([Motivo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_MotivoPK] ON [dbo].[IV_Motivo] ([Motivo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ObjFlow
   Criada em ..: 2017-09-11
   Alterada em : 2024-06-13
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ObjFlow] (
    [CodModelo] decimal(8,0) NOT NULL,
    [ModoSimples] numeric(1,0) NOT NULL,
    [TipoObj] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ChaveObj] numeric(18,0) NOT NULL,
    [PosLeft] numeric(18,0) NULL,
    [PosTop] numeric(18,0) NULL,
    [IndCor] decimal(10,0) NULL,
    [Dados] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IndMudanca] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_ObjFl__B2AEFB8BAB419CF8] PRIMARY KEY CLUSTERED ([CodModelo], [ModoSimples], [TipoObj], [ChaveObj])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_ObjFlow] ON [dbo].[IV_ObjFlow] ([CodModelo], [ModoSimples], [TipoObj], [ChaveObj]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ObjVenda
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ObjVenda] (
    [SeqObjVenda] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Seguimento] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Familia] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsuario] decimal(8,0) NULL,
    [QtdeContato] decimal(4,0) NULL,
    [QtdeUnidVendida] decimal(4,0) NULL,
    [VlrVenda] decimal(15,2) NULL,
    [QtdeTestDrive] decimal(4,0) NULL,
    [DtaReferencia] datetime NULL,
    [Obs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_ObjVenda__1A00A053] PRIMARY KEY CLUSTERED ([SeqObjVenda])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ObjVendaPK] ON [dbo].[IV_ObjVenda] ([SeqObjVenda]);
GO
CREATE NONCLUSTERED INDEX [IV_ObjVendaIE1] ON [dbo].[IV_ObjVenda] ([SeqUsuario], [Familia], [DtaReferencia]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_OcrmAgd
   Criada em ..: 2013-11-16
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_OcrmAgd] (
    [SeqOcrmAgd] numeric(18,0) NOT NULL,
    [SeqOcrmDest] numeric(18,0) NOT NULL,
    [SeqAgenda] numeric(18,0) NOT NULL,
    [DtaEnvio] datetime NULL,
    [IndEnviado] numeric(1,0) NULL,
    [NroEnvio] numeric(1,0) NULL,
    [eMail] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSMSG] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK_IV_OcrmAgd] PRIMARY KEY CLUSTERED ([SeqOcrmAgd])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_OcrmDest
   Criada em ..: 2013-11-16
   Alterada em : 2018-08-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_OcrmDest] (
    [SeqOcrmDest] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [Acao] numeric(10,0) NULL,
    [HUEnvio1] numeric(4,1) NULL,
    [HUEnvio2] numeric(4,1) NULL,
    [HUEnvio3] numeric(4,1) NULL,
    [HUEnvio4] numeric(4,1) NULL,
    [IndEnvPessoa] numeric(1,0) NULL,
    [IndEnvContato] numeric(1,0) NULL,
    [IndEnvHistProc] numeric(1,0) NULL,
    [IndEnvHistDNA] numeric(1,0) NULL,
    [IndEnvProcesso] numeric(1,0) NULL,
    CONSTRAINT [PK_IV_OcrmDest] PRIMARY KEY CLUSTERED ([SeqOcrmDest])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_OpBloq
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_OpBloq] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [DtaInicial] datetime NOT NULL,
    [DtaFinal] datetime NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsrDesvio] numeric(18,0) NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_OpBloq__1AF4C48C] PRIMARY KEY CLUSTERED ([SeqUsuario], [DtaInicial])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_OpBloqPK] ON [dbo].[IV_OpBloq] ([SeqUsuario], [DtaInicial]);
GO
CREATE NONCLUSTERED INDEX [IV_OpBloqIF1] ON [dbo].[IV_OpBloq] ([SeqUsuario]);
GO
ALTER TABLE [dbo].[IV_OpBloq] ADD CONSTRAINT [FK__IV_OpBloq__SeqUs__78958AE7] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[IV_Operador] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Operador
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Operador] (
    [SeqUsuario] numeric(18,0) NOT NULL,
    [SeqUnidade] numeric(4,0) NULL,
    [Funcao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Supervisor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [JustifDispon] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Gerente] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HoraInicial] datetime NULL,
    [HoraFinal] datetime NULL,
    [DispDomingo] numeric(1,0) NULL,
    [DispSegunda] numeric(1,0) NULL,
    [DispTerca] numeric(1,0) NULL,
    [DispQuarta] numeric(1,0) NULL,
    [DispQuinta] numeric(1,0) NULL,
    [DispSexta] numeric(1,0) NULL,
    [DispSabado] numeric(1,0) NULL,
    [EVendedor] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IncHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IncHistRetr] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdDiasRetr] decimal(4,0) NULL,
    [IncHistAgConcl] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExcHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IncAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExcAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VerAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ConcAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltOperAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltOperAgdReag] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ConcAgeVendor] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ReativarAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AnalisarHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AbreAnalise] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Alteragrupo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraCodImport] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraRegiao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraVendedor] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraClienteAtivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraStatus] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DupClicAgenda] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaEnvioEmail] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EnderecoSMTP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Departamento] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [eMail] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMailAssinatura] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdHistAdicAgenda] decimal(1,0) NULL,
    [PesAtendSeq] numeric(18,0) NULL,
    [PesAtendEnv] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PesAtendHora] datetime NULL,
    CONSTRAINT [PK__IV_Operador__1BE8E8C5] PRIMARY KEY CLUSTERED ([SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_OperadorPK] ON [dbo].[IV_Operador] ([SeqUsuario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_OPTEMAIL
   Criada em ..: 2017-08-01
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_OPTEMAIL] (
    [SEQCONTEUDO] numeric(6,0) NOT NULL,
    [EMAIL] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [MOTIVO] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTIN] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_OPTEM__49FE5E1E86897F66] PRIMARY KEY CLUSTERED ([SEQCONTEUDO], [EMAIL])
);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_OPTEMAIL] ON [dbo].[IV_OPTEMAIL] ([SEQCONTEUDO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_OPTEMAIL] ON [dbo].[IV_OPTEMAIL] ([EMAIL]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_OPTEMAIL01] ON [dbo].[IV_OPTEMAIL] ([EMAIL], [OUTIN]);
GO
ALTER TABLE [dbo].[IV_OPTEMAIL] ADD CONSTRAINT [FK__IV_OPTEMA__SEQCO__2D0A1E7D] FOREIGN KEY ([SEQCONTEUDO]) REFERENCES [dbo].[IV_TIPOCONTEUDO] ([SEQCONTEUDO]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_OPTFONE
   Criada em ..: 2017-08-01
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_OPTFONE] (
    [SEQCONTEUDO] numeric(6,0) NOT NULL,
    [FONENUMERO] numeric(18,0) NOT NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [MOTIVO] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTIN] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_OPTFO__C4D3B2D12729D2AB] PRIMARY KEY CLUSTERED ([SEQCONTEUDO], [FONENUMERO])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_OPTFONE] ON [dbo].[IV_OPTFONE] ([SEQCONTEUDO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_OPTFONE] ON [dbo].[IV_OPTFONE] ([FONENUMERO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_OPTFONE_02] ON [dbo].[IV_OPTFONE] ([SEQCONTEUDO], [OUTIN], [FONENUMERO]);
GO
ALTER TABLE [dbo].[IV_OPTFONE] ADD CONSTRAINT [FK__IV_OPTFON__SEQCO__2DFE42B6] FOREIGN KEY ([SEQCONTEUDO]) REFERENCES [dbo].[IV_TIPOCONTEUDO] ([SEQCONTEUDO]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_OS
   Criada em ..: 2019-02-05
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_OS] (
    [SEQOS] numeric(8,0) NOT NULL,
    [VERSAO] numeric(2,0) NOT NULL,
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [SEQPROJETO] numeric(18,0) NULL,
    [TIPOOS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTAFINALIZACAO] datetime NULL,
    [DTAFINALPREV] datetime NULL,
    [OBJETIVO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTAAPROVACAO] datetime NULL,
    [USUAPROVOU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSDESC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOVALOR] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [VLRORCADO] numeric(12,2) NULL,
    [VLRDESCONTO] numeric(12,2) NULL,
    [TIPOFATURAMENTO] char(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTAPREVFATURAR] datetime NULL,
    [OBS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_OS__C83207F528A23F61] PRIMARY KEY CLUSTERED ([SEQOS])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_OS] ON [dbo].[IV_OS] ([SEQPROJETO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_OS_0] ON [dbo].[IV_OS] ([SEQPESSOA]);
GO
ALTER TABLE [dbo].[IV_OS] ADD CONSTRAINT [FK__IV_OS__SEQPROJET__6C307F59] FOREIGN KEY ([SEQPROJETO]) REFERENCES [dbo].[IV_Projeto] ([SeqProjeto]);
GO
ALTER TABLE [dbo].[IV_OS] ADD CONSTRAINT [FK__IV_OS__SEQPESSOA__6D24A392] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PAREVTEXTRES
   Criada em ..: 2021-08-02
   Alterada em : 2021-08-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PAREVTEXTRES] (
    [SEQPAREVTRES] numeric(18,0) NOT NULL,
    [TABELA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [EVENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TIPOPRODUTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOOUTRO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DETALHEADIC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RESULTADO] numeric(6,0) NOT NULL,
    CONSTRAINT [PK__IV_PAREV__447A2B4B2363CA50] PRIMARY KEY CLUSTERED ([SEQPAREVTRES])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_PAREVTEXTRES] ON [dbo].[IV_PAREVTEXTRES] ([RESULTADO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Pcte
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Pcte] (
    [NroPcte] decimal(2,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descr] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Pcte__1DD13137] PRIMARY KEY CLUSTERED ([NroPcte])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PctePK] ON [dbo].[IV_Pcte] ([NroPcte]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PcteAK1] ON [dbo].[IV_Pcte] ([Pcte]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PcteAtrFx
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PcteAtrFx] (
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Atributo] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Lista] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__IV_PcteAtrFx__1EC55570] PRIMARY KEY CLUSTERED ([Pcte], [Atributo], [Lista])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PcteAtrFxPK] ON [dbo].[IV_PcteAtrFx] ([Pcte], [Atributo], [Lista]);
GO
CREATE NONCLUSTERED INDEX [IV_PcteAtrFxIF1] ON [dbo].[IV_PcteAtrFx] ([Atributo], [Lista]);
GO
ALTER TABLE [dbo].[IV_PcteAtrFx] ADD CONSTRAINT [FK__IV_PcteAtrFx__3440609A] FOREIGN KEY ([Atributo], [Lista]) REFERENCES [dbo].[GE_AtributoFixo] ([Atributo], [Lista]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Pessoa
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Pessoa] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [LinkWeb] numeric(1,0) NULL,
    [NomeReduzido] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DirEspecial] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_Pessoa__1FB979A9] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PessoaPK] ON [dbo].[IV_Pessoa] ([SeqPessoa]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PessoaAK1] ON [dbo].[IV_Pessoa] ([NomeReduzido]);
GO
ALTER TABLE [dbo].[IV_Pessoa] ADD CONSTRAINT [FK__IV_Pessoa__SeqPe__7B71F792] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PessoaStat
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PessoaStat] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [DtaUltCalcHst] datetime NULL,
    [DtaHstBase] datetime NULL,
    CONSTRAINT [PK__IV_PessoaStat__697D6489] PRIMARY KEY CLUSTERED ([SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XIF1IV_PessoaStat] ON [dbo].[IV_PessoaStat] ([SeqPessoa]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PlanoAtiv
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PlanoAtiv] (
    [SeqPlanoAtiv] numeric(18,0) NOT NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nro] decimal(10,0) NULL,
    [Cod] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] decimal(1,0) NULL,
    [NroOrdem] decimal(6,0) NULL,
    [SeqGrupo] numeric(18,0) NULL,
    [Analitica] decimal(1,0) NULL,
    [DtaNula] datetime NULL,
    CONSTRAINT [PK__IV_PlanoAtiv__21A1C21B] PRIMARY KEY CLUSTERED ([SeqPlanoAtiv])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PlanoAtivPK] ON [dbo].[IV_PlanoAtiv] ([SeqPlanoAtiv]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PlanoAtivAK1] ON [dbo].[IV_PlanoAtiv] ([Tipo], [Nro], [Cod]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcAcao
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcAcao] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [Acao] decimal(6,0) NOT NULL,
    [QtdeLimite] decimal(2,0) NULL,
    CONSTRAINT [PK__IV_ProcAcao__238A0A8D] PRIMARY KEY CLUSTERED ([CodProcesso], [Acao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcAcaoPK] ON [dbo].[IV_ProcAcao] ([CodProcesso], [Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcAcaoIF1] ON [dbo].[IV_ProcAcao] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcAcaoIF2] ON [dbo].[IV_ProcAcao] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_ProcAcao] ADD CONSTRAINT [FK__IV_ProcAca__Acao__7D5A4004] FOREIGN KEY ([Acao]) REFERENCES [dbo].[IV_Acao] ([Acao]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ProcAcao] ADD CONSTRAINT [FK__IV_ProcAc__CodPr__7E4E643D] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcAtiv
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcAtiv] (
    [Processo] numeric(18,0) NOT NULL,
    [StatRealAtividade] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [StatRealProcesso] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaFinalizacao] datetime NULL,
    [IndFechado] numeric(1,0) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcAtiv__247E2EC6] PRIMARY KEY CLUSTERED ([Processo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcAtivPK] ON [dbo].[IV_ProcAtiv] ([Processo]);
GO
ALTER TABLE [dbo].[IV_ProcAtiv] ADD CONSTRAINT [FK__IV_ProcAt__Proce__7F428876] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_ProcDado] ([Processo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcComent
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcComent] (
    [Processo] numeric(18,0) NOT NULL,
    [SeqProcComent] decimal(2,0) NOT NULL,
    [Comentario] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcComent__257252FF] PRIMARY KEY CLUSTERED ([Processo], [SeqProcComent])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcComentPK] ON [dbo].[IV_ProcComent] ([Processo], [SeqProcComent]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcComentIF1] ON [dbo].[IV_ProcComent] ([Processo]);
GO
ALTER TABLE [dbo].[IV_ProcComent] ADD CONSTRAINT [FK__IV_ProcCo__Proce__0036ACAF] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_Processo] ([Processo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcDado
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcDado] (
    [Processo] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [ProcessoPai] numeric(18,0) NULL,
    [ProcessoDNA] numeric(18,0) NULL,
    [CodProcesso] decimal(4,0) NULL,
    [PessoaDepto] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqDepto] decimal(4,0) NULL,
    [SeqProjeto] numeric(18,0) NULL,
    [SeqPessoa] decimal(10,0) NULL,
    [HistoricoOrigem] numeric(18,0) NULL,
    [Vendedor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FormaPrimCont] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AtivoReceptivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Motivo] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campanha] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Origem] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UltResultado] int NULL,
    [UltHistorico] numeric(18,0) NULL,
    [DtaUltResultado] datetime NULL,
    [HUDecorrido] decimal(8,2) NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaPrimResultado] datetime NULL,
    [DtaGeracao] datetime NULL,
    [UsuGeracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcDado__26667738] PRIMARY KEY CLUSTERED ([Processo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcDadoPK] ON [dbo].[IV_ProcDado] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDadoIE1] ON [dbo].[IV_ProcDado] ([ProcessoPai]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDadoIE2] ON [dbo].[IV_ProcDado] ([ProcessoDNA]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDadoIE3] ON [dbo].[IV_ProcDado] ([HistoricoOrigem]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDadoIE4] ON [dbo].[IV_ProcDado] ([UltHistorico]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDadoIE5] ON [dbo].[IV_ProcDado] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDadoIF1] ON [dbo].[IV_ProcDado] ([SeqDepto]);
GO
ALTER TABLE [dbo].[IV_ProcDado] ADD CONSTRAINT [FK__IV_ProcDa__SeqDe__012AD0E8] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]);
GO
ALTER TABLE [dbo].[IV_ProcDado] ADD CONSTRAINT [FK__IV_ProcDa__SeqDe__3AED5E29] FOREIGN KEY ([SeqDepto]) REFERENCES [dbo].[IVS_Depto] ([SeqDepto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcDocto
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcDocto] (
    [Processo] numeric(18,0) NOT NULL,
    [SeqDocto] numeric(18,0) NOT NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcDocto__275A9B71] PRIMARY KEY CLUSTERED ([Processo], [SeqDocto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcDoctoPK] ON [dbo].[IV_ProcDocto] ([Processo], [SeqDocto]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDoctoIF2] ON [dbo].[IV_ProcDocto] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcDoctoIF3] ON [dbo].[IV_ProcDocto] ([SeqDocto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Processo
   Criada em ..: 2011-12-19
   Alterada em : 2025-07-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Processo] (
    [Processo] numeric(18,0) NOT NULL,
    [Resumo] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuResponsavel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Realizado] decimal(1,0) NULL,
    [DtaRealizacao] datetime NULL,
    [DtaFase] datetime NULL,
    [DtaStatus] datetime NULL,
    [Perspectiva] decimal(3,0) NULL,
    [Fase] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaseOrdem] decimal(2,0) NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [StatusDesc] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaPrevConclusao] datetime NULL,
    [Valor] decimal(15,2) NULL,
    [Qtde] decimal(10,2) NULL,
    [DtaPrevConcOrig] datetime NULL,
    [Prioridade] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_Processo__284EBFAA] PRIMARY KEY CLUSTERED ([Processo])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcessoPK] ON [dbo].[IV_Processo] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcessoIE3] ON [dbo].[IV_Processo] ([DtaFase]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcessoIE4] ON [dbo].[IV_Processo] ([DtaStatus]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcessoIE5] ON [dbo].[IV_Processo] ([DtaRealizacao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Processo_bkp20250717
   Criada em ..: 2025-07-17
   Alterada em : 2025-07-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Processo_bkp20250717] (
    [Processo] numeric(18,0) NOT NULL,
    [Resumo] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuResponsavel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Realizado] decimal(1,0) NULL,
    [DtaRealizacao] datetime NULL,
    [DtaFase] datetime NULL,
    [DtaStatus] datetime NULL,
    [Perspectiva] decimal(3,0) NULL,
    [Fase] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaseOrdem] decimal(2,0) NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [StatusDesc] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaPrevConclusao] datetime NULL,
    [Valor] decimal(15,2) NULL,
    [Qtde] decimal(10,2) NULL,
    [DtaPrevConcOrig] datetime NULL,
    [Prioridade] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcFase
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcFase] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [Fase] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [FaseOrdem] decimal(2,0) NULL,
    [Padrao] numeric(1,0) NULL,
    [Marco] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcFase__2942E3E3] PRIMARY KEY CLUSTERED ([CodProcesso], [Fase])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcFasePK] ON [dbo].[IV_ProcFase] ([CodProcesso], [Fase]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcFaseIF1] ON [dbo].[IV_ProcFase] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_ProcFase] ADD CONSTRAINT [FK__IV_ProcFa__CodPr__04073D93] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcFaseMonit
   Criada em ..: 2016-07-12
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcFaseMonit] (
    [Processo] numeric(18,0) NOT NULL,
    [SeqFase] decimal(4,0) NOT NULL,
    [FASE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [FaseOrdem] numeric(2,0) NULL,
    [DtaInicio] datetime NULL,
    [DtaFim] datetime NULL,
    [UltHistorico] numeric(18,0) NULL,
    [HU] numeric(6,2) NULL,
    [HN] numeric(6,2) NULL,
    CONSTRAINT [PK__IV_ProcF__318467F5627E6ACC] PRIMARY KEY CLUSTERED ([Processo], [SeqFase])
);
GO
CREATE NONCLUSTERED INDEX [IV_PROCFASEMONIIF1] ON [dbo].[IV_ProcFaseMonit] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [XIE1IV_ProcFaseMonit] ON [dbo].[IV_ProcFaseMonit] ([Processo], [FASE]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcLink
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcLink] (
    [Processo] decimal(15,0) NOT NULL,
    [LinkSerie] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkDocto] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkNroEmpresa] numeric(6,0) NULL,
    [SEQPROCLINK] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [DTAALTERACAO] datetime NULL,
    CONSTRAINT [PK__IV_ProcL__DF64D5AE6076202E] PRIMARY KEY CLUSTERED ([SEQPROCLINK])
);
GO
CREATE NONCLUSTERED INDEX [IV_PROCLINK_IDX3] ON [dbo].[IV_ProcLink] ([LinkDocto], [Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcLinkIE1] ON [dbo].[IV_ProcLink] ([LinkSerie]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcLinkIE2] ON [dbo].[IV_ProcLink] ([LinkNro]);
GO
CREATE NONCLUSTERED INDEX [IV_PROCLINKIE1x] ON [dbo].[IV_ProcLink] ([LinkSerie]);
GO
CREATE NONCLUSTERED INDEX [IV_PROCLINKIE2x] ON [dbo].[IV_ProcLink] ([LinkNro]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_ProcLinkx] ON [dbo].[IV_ProcLink] ([Processo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcPersp
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcPersp] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [PerspOrdem] decimal(2,0) NOT NULL,
    [Perspectiva] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [QtdDiaPro] decimal(6,2) NULL,
    CONSTRAINT [PK__IV_ProcPersp__2C1F508E] PRIMARY KEY CLUSTERED ([CodProcesso], [PerspOrdem])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcPerspPK] ON [dbo].[IV_ProcPersp] ([CodProcesso], [PerspOrdem]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcPerspIF1] ON [dbo].[IV_ProcPersp] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_ProcPersp] ADD CONSTRAINT [FK__IV_ProcPe__CodPr__05EF8605] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_ProcPersp] ADD CONSTRAINT [FK__IV_ProcPe__CodPr__3FB21346] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcPerspMonit
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcPerspMonit] (
    [Processo] numeric(18,0) NOT NULL,
    [DtaMonit] datetime NOT NULL,
    [Perspectiva] decimal(3,0) NULL,
    [HU] decimal(6,2) NULL,
    [HN] decimal(6,2) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcPerspMoni__2D1374C7] PRIMARY KEY CLUSTERED ([Processo], [DtaMonit])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcPerspMonPK] ON [dbo].[IV_ProcPerspMonit] ([Processo], [DtaMonit]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcPerspMonIF1] ON [dbo].[IV_ProcPerspMonit] ([Processo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PROCPESLINK
   Criada em ..: 2020-11-16
   Alterada em : 2020-11-16
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PROCPESLINK] (
    [PROCESSO] numeric(18,0) NOT NULL,
    [SEQPESSOALINK] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__IV_PROCP__5FF079A979EB57B0] PRIMARY KEY CLUSTERED ([PROCESSO], [SEQPESSOALINK])
);
GO
ALTER TABLE [dbo].[IV_PROCPESLINK] ADD CONSTRAINT [FK__IV_PROCPE__PROCE__69740E3F] FOREIGN KEY ([PROCESSO]) REFERENCES [dbo].[IV_ProcDado] ([Processo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcProduto
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcProduto] (
    [Processo] numeric(18,0) NOT NULL,
    [SeqProcProduto] decimal(4,0) NOT NULL,
    [SeqProduto] numeric(18,0) NULL,
    [CodProduto] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Valor] decimal(15,2) NULL,
    [Qtde] decimal(10,2) NULL,
    [Obs] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPOPGTO] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCONTO] numeric(14,2) NULL,
    [STATUS] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO01] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO02] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO03] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO04] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO05] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO06] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO07] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATRIBUTO08] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcProduto__2EFBBD39] PRIMARY KEY CLUSTERED ([Processo], [SeqProcProduto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcProdutoPK] ON [dbo].[IV_ProcProduto] ([Processo], [SeqProcProduto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcProjeto
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcProjeto] (
    [Processo] numeric(18,0) NOT NULL,
    [SeqProjeto] numeric(18,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcProjeto__36F1E4BC] PRIMARY KEY CLUSTERED ([Processo], [SeqProjeto])
);
GO
CREATE NONCLUSTERED INDEX [IV_PROCPROJETOIF1] ON [dbo].[IV_ProcProjeto] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_PROCPROJETOIF2] ON [dbo].[IV_ProcProjeto] ([SeqProjeto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcRef
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcRef] (
    [Processo] numeric(18,0) NOT NULL,
    [Tipo] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Referencia] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DtaGeracao] datetime NULL,
    [UsuGeracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcRef__2FEFE172] PRIMARY KEY CLUSTERED ([Processo], [Tipo], [Referencia])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcRefPK] ON [dbo].[IV_ProcRef] ([Processo], [Tipo], [Referencia]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcRefIE1] ON [dbo].[IV_ProcRef] ([Tipo], [Referencia]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcRefIF1] ON [dbo].[IV_ProcRef] ([Processo]);
GO
ALTER TABLE [dbo].[IV_ProcRef] ADD CONSTRAINT [FK__IV_ProcRe__Proce__07D7CE77] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_ProcDado] ([Processo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcRelacao
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcRelacao] (
    [ProcBase] numeric(18,0) NOT NULL,
    [Processo] numeric(18,0) NOT NULL,
    [SeqTpRel] decimal(4,0) NOT NULL,
    [Obs] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcRelacao__30E405AB] PRIMARY KEY CLUSTERED ([ProcBase], [Processo], [SeqTpRel])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcRelacaoPK] ON [dbo].[IV_ProcRelacao] ([ProcBase], [Processo], [SeqTpRel]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcRelacaoIF1] ON [dbo].[IV_ProcRelacao] ([SeqTpRel]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcRelacaoIF2] ON [dbo].[IV_ProcRelacao] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcRelacaoIF3] ON [dbo].[IV_ProcRelacao] ([ProcBase]);
GO
ALTER TABLE [dbo].[IV_ProcRelacao] ADD CONSTRAINT [FK__IV_ProcRe__SeqTp__08CBF2B0] FOREIGN KEY ([SeqTpRel]) REFERENCES [dbo].[IV_ProcTpRel] ([SeqTpRel]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ProcRelacao] ADD CONSTRAINT [FK__IV_ProcRe__Proce__09C016E9] FOREIGN KEY ([Processo]) REFERENCES [dbo].[IV_Processo] ([Processo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcResultado
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcResultado] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [Resultado] decimal(6,0) NOT NULL,
    [Fase] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FaseSeguinte] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcResultado__31D829E4] PRIMARY KEY CLUSTERED ([CodProcesso], [Resultado])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcResultadPK] ON [dbo].[IV_ProcResultado] ([CodProcesso], [Resultado]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcResultadIF1] ON [dbo].[IV_ProcResultado] ([CodProcesso], [Fase]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcResultadIF2] ON [dbo].[IV_ProcResultado] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcResultadIF3] ON [dbo].[IV_ProcResultado] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_ProcResultado] ADD CONSTRAINT [FK__IV_ProcRe__Resul__0C9C8394] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ProcResultado] ADD CONSTRAINT [FK__IV_ProcRe__CodPr__0D90A7CD] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcSt
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcSt] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [StatusOrdem] decimal(2,0) NULL,
    [Padrao] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_ProcSt__32CC4E1D] PRIMARY KEY CLUSTERED ([CodProcesso], [Status])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcStPK] ON [dbo].[IV_ProcSt] ([CodProcesso], [Status]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcStIF1] ON [dbo].[IV_ProcSt] ([CodProcesso]);
GO
ALTER TABLE [dbo].[IV_ProcSt] ADD CONSTRAINT [FK__IV_ProcSt__CodPr__0E84CC06] FOREIGN KEY ([CodProcesso]) REFERENCES [dbo].[IV_CodProcesso] ([CodProcesso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcStatMonit
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcStatMonit] (
    [Processo] numeric(18,0) NOT NULL,
    [DtaMonit] datetime NOT NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HU] decimal(6,2) NULL,
    [HN] decimal(6,2) NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcStatMonit__33C07256] PRIMARY KEY CLUSTERED ([Processo], [DtaMonit])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcStatMoniPK] ON [dbo].[IV_ProcStatMonit] ([Processo], [DtaMonit]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcStatMoniIF1] ON [dbo].[IV_ProcStatMonit] ([Processo]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PROCTAG
   Criada em ..: 2023-06-05
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PROCTAG] (
    [PROCESSO] numeric(18,0) NOT NULL,
    [TAG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [DTAGERACAO] datetime NULL,
    [USUGERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_PROCT__747141519E2D82F4] PRIMARY KEY CLUSTERED ([PROCESSO], [TAG])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PROCTAG] ON [dbo].[IV_PROCTAG] ([TAG]);
GO
ALTER TABLE [dbo].[IV_PROCTAG] ADD CONSTRAINT [FK__IV_PROCTA__PROCE__5C050488] FOREIGN KEY ([PROCESSO]) REFERENCES [dbo].[IV_ProcDado] ([Processo]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcTpRel
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcTpRel] (
    [SeqTpRel] decimal(4,0) NOT NULL,
    [DescReduzida] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcTpRel__34B4968F] PRIMARY KEY CLUSTERED ([SeqTpRel])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcTpRelPK] ON [dbo].[IV_ProcTpRel] ([SeqTpRel]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProcVinc
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProcVinc] (
    [CodProcesso] decimal(4,0) NOT NULL,
    [SeqVinc] decimal(4,0) NOT NULL,
    [Vinculo] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Exigido] numeric(1,0) NOT NULL,
    [NroVinc] numeric(18,0) NULL,
    [CodVinc] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProcVinc__35A8BAC8] PRIMARY KEY CLUSTERED ([CodProcesso], [SeqVinc])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProcVincPK] ON [dbo].[IV_ProcVinc] ([CodProcesso], [SeqVinc]);
GO
CREATE NONCLUSTERED INDEX [IV_ProcVincIF1] ON [dbo].[IV_ProcVinc] ([CodProcesso]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Produto
   Criada em ..: 2011-12-19
   Alterada em : 2021-07-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Produto] (
    [SeqProduto] numeric(18,0) NOT NULL,
    [CodProduto] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [CodProdutoFora] numeric(18,0) NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Especificacao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Preco1] decimal(15,2) NULL,
    [Preco2] decimal(15,2) NULL,
    [Familia] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Atributo5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Fator1] decimal(8,4) NULL,
    [Fator2] decimal(8,4) NULL,
    [Fator3] decimal(8,4) NULL,
    [EmUso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [URLDetalhes] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVEPRODUTOERP] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQESTRPROD] numeric(10,0) NULL,
    CONSTRAINT [PK__IV_Produto__369CDF01] PRIMARY KEY CLUSTERED ([SeqProduto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProdutoPK] ON [dbo].[IV_Produto] ([SeqProduto]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProdutoAK1] ON [dbo].[IV_Produto] ([CodProduto]);
GO
CREATE NONCLUSTERED INDEX [IV_ProdutoIE1] ON [dbo].[IV_Produto] ([Descricao]);
GO
CREATE NONCLUSTERED INDEX [IV_ProdutoIE2] ON [dbo].[IV_Produto] ([CodProdutoFora]);
GO
ALTER TABLE [dbo].[IV_Produto] ADD CONSTRAINT [FK__IV_Produt__SEQES__18640752] FOREIGN KEY ([SEQESTRPROD]) REFERENCES [dbo].[IV_ESTRPRODUTO] ([SEQESTRPROD]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProjColec
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProjColec] (
    [SeqProjColec] numeric(18,0) NOT NULL,
    [Descricao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProjColec__3791033A] PRIMARY KEY CLUSTERED ([SeqProjColec])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProjColecPK] ON [dbo].[IV_ProjColec] ([SeqProjColec]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProjDocto
   Criada em ..: 2016-07-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProjDocto] (
    [SeqProjeto] numeric(18,0) NOT NULL,
    [SeqDocto] numeric(18,0) NOT NULL,
    [DtaInclusao] datetime NULL,
    [UsuIncluiu] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProjD__5308B8129D3248AE] PRIMARY KEY CLUSTERED ([SeqProjeto], [SeqDocto])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_ProjDocto] ON [dbo].[IV_ProjDocto] ([SeqProjeto]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_ProjDocto] ON [dbo].[IV_ProjDocto] ([SeqDocto]);
GO
ALTER TABLE [dbo].[IV_ProjDocto] ADD CONSTRAINT [FK__IV_ProjDo__SeqPr__66C1BEC6] FOREIGN KEY ([SeqProjeto]) REFERENCES [dbo].[IV_Projeto] ([SeqProjeto]);
GO
ALTER TABLE [dbo].[IV_ProjDocto] ADD CONSTRAINT [FK__IV_ProjDo__SeqDo__67B5E2FF] FOREIGN KEY ([SeqDocto]) REFERENCES [dbo].[DMN_Doc] ([SeqDocto]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ProjDocto] ADD CONSTRAINT [FK__IV_ProjDo__SeqPr__19A178F7] FOREIGN KEY ([SeqProjeto]) REFERENCES [dbo].[IV_Projeto] ([SeqProjeto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProjEquipe
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProjEquipe] (
    [SeqProjeto] numeric(18,0) NOT NULL,
    [SeqUsuario] numeric(18,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProjEquipe__38852773] PRIMARY KEY CLUSTERED ([SeqProjeto], [SeqUsuario])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProjEquipePK] ON [dbo].[IV_ProjEquipe] ([SeqProjeto], [SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_ProjEquipeIF1] ON [dbo].[IV_ProjEquipe] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_ProjEquipeIF2] ON [dbo].[IV_ProjEquipe] ([SeqProjeto]);
GO
ALTER TABLE [dbo].[IV_ProjEquipe] ADD CONSTRAINT [FK__IV_ProjEq__SeqPr__12555CEA] FOREIGN KEY ([SeqProjeto]) REFERENCES [dbo].[IV_Projeto] ([SeqProjeto]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ProjEquipe] ADD CONSTRAINT [FK__IV_ProjEq__SeqUs__116138B1] FOREIGN KEY ([SeqUsuario]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Projeto
   Criada em ..: 2011-12-19
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Projeto] (
    [SeqProjeto] numeric(18,0) NOT NULL,
    [SeqProjColec] numeric(18,0) NULL,
    [Projeto] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Responsavel] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaIniPrev] datetime NULL,
    [DtaFinPrev] datetime NULL,
    [DtaInicial] datetime NULL,
    [DtaFinal] datetime NULL,
    [DtaInsProcDe] datetime NULL,
    [DtaInsProcA] datetime NULL,
    [QtdHoraEst] decimal(6,0) NULL,
    [VlrVenda] decimal(15,2) NULL,
    [CustoEst] decimal(15,2) NULL,
    [PubWeb] numeric(1,0) NULL,
    [Status] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Projeto__39794BAC] PRIMARY KEY CLUSTERED ([SeqProjeto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProjetoPK] ON [dbo].[IV_Projeto] ([SeqProjeto]);
GO
CREATE NONCLUSTERED INDEX [IV_ProjetoIF1] ON [dbo].[IV_Projeto] ([SeqProjColec]);
GO
ALTER TABLE [dbo].[IV_Projeto] ADD CONSTRAINT [FK__IV_Projet__SeqPr__13498123] FOREIGN KEY ([SeqProjColec]) REFERENCES [dbo].[IV_ProjColec] ([SeqProjColec]);
GO
ALTER TABLE [dbo].[IV_Projeto] ADD CONSTRAINT [FK__IV_Projet__SeqPr__4D0C0E64] FOREIGN KEY ([SeqProjColec]) REFERENCES [dbo].[IV_ProjColec] ([SeqProjColec]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ProjPessoa
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ProjPessoa] (
    [SeqPessoa] numeric(10,0) NOT NULL,
    [SeqProjeto] numeric(18,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ProjPessoa__3A6D6FE5] PRIMARY KEY CLUSTERED ([SeqPessoa], [SeqProjeto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ProjPessoaPK] ON [dbo].[IV_ProjPessoa] ([SeqPessoa], [SeqProjeto]);
GO
CREATE NONCLUSTERED INDEX [IV_ProjPessoaIF1] ON [dbo].[IV_ProjPessoa] ([SeqProjeto]);
GO
CREATE NONCLUSTERED INDEX [IV_ProjPessoaIF2] ON [dbo].[IV_ProjPessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IV_ProjPessoa] ADD CONSTRAINT [FK__IV_ProjPe__SeqPr__143DA55C] FOREIGN KEY ([SeqProjeto]) REFERENCES [dbo].[IV_Projeto] ([SeqProjeto]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ProjPessoa] ADD CONSTRAINT [FK__IV_ProjPe__SeqPe__1531C995] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Propriedade
   Criada em ..: 2011-12-19
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Propriedade] (
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Propriedade] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] decimal(1,0) NULL,
    [NivelIdentificador] decimal(1,0) NULL,
    [CampoListar] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MostraRef] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usacomplemento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoRefVinculado] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UmPorPessoa] numeric(1,0) NULL,
    [ReferenciaSQL] numeric(1,0) NULL,
    [Campo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1Sql] numeric(1,0) NULL,
    [Campo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2Sql] numeric(1,0) NULL,
    [Campo3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3Sql] numeric(1,0) NULL,
    [Campo4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4Sql] numeric(1,0) NULL,
    [Campo5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5Sql] numeric(1,0) NULL,
    [Campo6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6Sql] numeric(1,0) NULL,
    [Numero1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao6] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ReferenciaDesc] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal1Sql] numeric(1,0) NULL,
    [Literal2Sql] numeric(1,0) NULL,
    [Literal3Sql] numeric(1,0) NULL,
    [Literal4Sql] numeric(1,0) NULL,
    [Literal5Sql] numeric(1,0) NULL,
    [Literal6Sql] numeric(1,0) NULL,
    [Literal7Sql] numeric(1,0) NULL,
    [Literal8Sql] numeric(1,0) NULL,
    [Literal9Sql] numeric(1,0) NULL,
    [Literal10Sql] numeric(1,0) NULL,
    [CAMPO7] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO8] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7SQL] numeric(1,0) NULL,
    [CAMPO8SQL] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_Propriedade__3B61941E] PRIMARY KEY CLUSTERED ([SeqPropriedade])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PropriedadePK] ON [dbo].[IV_Propriedade] ([SeqPropriedade]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PropriedadeAK1] ON [dbo].[IV_Propriedade] ([Propriedade]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Propriedade_BKPJUN
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Propriedade_BKPJUN] (
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Propriedade] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] decimal(1,0) NULL,
    [NivelIdentificador] decimal(1,0) NULL,
    [CampoListar] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MostraRef] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usacomplemento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoRefVinculado] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UmPorPessoa] numeric(1,0) NULL,
    [ReferenciaSQL] numeric(1,0) NULL,
    [Campo1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1Sql] numeric(1,0) NULL,
    [Campo2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2Sql] numeric(1,0) NULL,
    [Campo3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3Sql] numeric(1,0) NULL,
    [Campo4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4Sql] numeric(1,0) NULL,
    [Campo5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5Sql] numeric(1,0) NULL,
    [Campo6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6Sql] numeric(1,0) NULL,
    [Numero1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao6] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ReferenciaDesc] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal1Sql] numeric(1,0) NULL,
    [Literal2Sql] numeric(1,0) NULL,
    [Literal3Sql] numeric(1,0) NULL,
    [Literal4Sql] numeric(1,0) NULL,
    [Literal5Sql] numeric(1,0) NULL,
    [Literal6Sql] numeric(1,0) NULL,
    [Literal7Sql] numeric(1,0) NULL,
    [Literal8Sql] numeric(1,0) NULL,
    [Literal9Sql] numeric(1,0) NULL,
    [Literal10Sql] numeric(1,0) NULL,
    [CAMPO7] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO8] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7SQL] numeric(1,0) NULL,
    [CAMPO8SQL] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Propriedade_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Propriedade_ITA] (
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Propriedade] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nivel] decimal(1,0) NULL,
    [NivelIdentificador] decimal(1,0) NULL,
    [CampoListar] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MostraRef] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usacomplemento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoRefVinculado] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UmPorPessoa] numeric(1,0) NULL,
    [ReferenciaSQL] numeric(1,0) NULL,
    [Campo1] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo1Sql] numeric(1,0) NULL,
    [Campo2] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo2Sql] numeric(1,0) NULL,
    [Campo3] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo3Sql] numeric(1,0) NULL,
    [Campo4] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo4Sql] numeric(1,0) NULL,
    [Campo5] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo5Sql] numeric(1,0) NULL,
    [Campo6] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campo6Sql] numeric(1,0) NULL,
    [Numero1] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero2] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero3] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero4] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero5] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Numero6] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data1] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data2] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data3] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data4] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data5] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Data6] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SimNao6] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal1] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal2] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal3] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal4] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal5] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal6] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal7] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal8] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal9] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal10] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ReferenciaDesc] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Literal1Sql] numeric(1,0) NULL,
    [Literal2Sql] numeric(1,0) NULL,
    [Literal3Sql] numeric(1,0) NULL,
    [Literal4Sql] numeric(1,0) NULL,
    [Literal5Sql] numeric(1,0) NULL,
    [Literal6Sql] numeric(1,0) NULL,
    [Literal7Sql] numeric(1,0) NULL,
    [Literal8Sql] numeric(1,0) NULL,
    [Literal9Sql] numeric(1,0) NULL,
    [Literal10Sql] numeric(1,0) NULL,
    [CAMPO7] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO8] varchar(22) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPO7SQL] numeric(1,0) NULL,
    [CAMPO8SQL] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PropriLista
   Criada em ..: 2011-12-19
   Alterada em : 2019-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PropriLista] (
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Campo] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPropriLista] decimal(4,0) NOT NULL,
    [Lista] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ordem] smallint NULL,
    CONSTRAINT [PK__IV_PropriLista__3C55B857] PRIMARY KEY CLUSTERED ([SeqPropriedade], [Campo], [SeqPropriLista])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PropriListaPK] ON [dbo].[IV_PropriLista] ([SeqPropriedade], [Campo], [SeqPropriLista]);
GO
CREATE NONCLUSTERED INDEX [IV_PropriListaIF1] ON [dbo].[IV_PropriLista] ([SeqPropriedade]);
GO
ALTER TABLE [dbo].[IV_PropriLista] ADD CONSTRAINT [FK__IV_Propri__SeqPr__1625EDCE] FOREIGN KEY ([SeqPropriedade]) REFERENCES [dbo].[IV_Propriedade] ([SeqPropriedade]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PropriLista_ITA
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PropriLista_ITA] (
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Campo] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPropriLista] decimal(4,0) NOT NULL,
    [Lista] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ordem] smallint NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PropriListaLk
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PropriListaLk] (
    [SeqPropriedade] numeric(4,0) NOT NULL,
    [Campo] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqPropriLista] decimal(4,0) NOT NULL,
    [SeqPropListaLk] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Similar] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_PropriListaLk__3D49DC90] PRIMARY KEY CLUSTERED ([SeqPropriedade], [Campo], [SeqPropriLista], [SeqPropListaLk])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_PropriListaLPK] ON [dbo].[IV_PropriListaLk] ([SeqPropriedade], [Campo], [SeqPropriLista], [SeqPropListaLk]);
GO
CREATE NONCLUSTERED INDEX [IV_PropriListaLIE1] ON [dbo].[IV_PropriListaLk] ([SeqPropriedade], [Campo], [Similar]);
GO
CREATE NONCLUSTERED INDEX [IV_PropriListaLIF1] ON [dbo].[IV_PropriListaLk] ([SeqPropriedade], [Campo], [SeqPropriLista]);
GO
ALTER TABLE [dbo].[IV_PropriListaLk] ADD CONSTRAINT [FK__IV_PropriListaLk__171A1207] FOREIGN KEY ([SeqPropriedade], [Campo], [SeqPropriLista]) REFERENCES [dbo].[IV_PropriLista] ([SeqPropriedade], [Campo], [SeqPropriLista]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_PUSH
   Criada em ..: 2019-09-02
   Alterada em : 2019-09-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_PUSH] (
    [SEQPUSH] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [APLICATIVO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ASSUNTO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MENSAGEM] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SEQPESSOA] numeric(10,0) NULL,
    [PROCESSO] numeric(18,0) NULL,
    [SEQHISTORICO] numeric(18,0) NULL,
    [SEQCONTATO] numeric(4,0) NULL,
    [DTAGERACAO] datetime NULL,
    [DTAENVIO] datetime NULL,
    [CONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUBCONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAENVIAR] datetime NULL,
    [STATUS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSMSG] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_PUSH__6928302B9978D833] PRIMARY KEY CLUSTERED ([SEQPUSH])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PUSH_02] ON [dbo].[IV_PUSH] ([SEQHISTORICO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PUSH_03] ON [dbo].[IV_PUSH] ([SEQPESSOA]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PUSH_01] ON [dbo].[IV_PUSH] ([STATUS]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PUSH_04] ON [dbo].[IV_PUSH] ([PROCESSO]);
GO
ALTER TABLE [dbo].[IV_PUSH] ADD CONSTRAINT [FK__IV_PUSH__SEQPESS__3C1757E3] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IV_PUSH] ADD CONSTRAINT [FK__IV_PUSH__PROCESS__3D0B7C1C] FOREIGN KEY ([PROCESSO]) REFERENCES [dbo].[IV_ProcDado] ([Processo]);
GO
ALTER TABLE [dbo].[IV_PUSH] ADD CONSTRAINT [FK__IV_PUSH__SEQHIST__3B2333AA] FOREIGN KEY ([SEQHISTORICO]) REFERENCES [dbo].[IV_Historico] ([SeqHistorico]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ABERTURA_OS_REVISAO
   Criada em ..: 2025-02-14
   Alterada em : 2025-02-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ABERTURA_OS_REVISAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_ABERTURA] datetime NULL,
    [NUMERO_OS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ABE__7484C68CDD6E13FE] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ABERTURA_OS_REVISAO] ADD CONSTRAINT [FK__IV_Q_ABER__SEQQU__672C9E71] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMP_VEND_MAQUINAS
   Criada em ..: 2022-03-31
   Alterada em : 2023-08-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMP_VEND_MAQUINAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_PESO_TRASEIRO__] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KG_PESO_TRASEIRO__1_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSICAO_PESO_TRASEIR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_PESO_TRAS2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KG_PESO_TRASEIRO__2_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSICAO_PESO_TRASEI2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_PESO_DIANTEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KG_PESO_DIANTEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTRO_DIANTEIRO__] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTRO_TRASEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_TRASEIRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_DIANTEIRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_VCR_100] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_VCR_300] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MEDIDA_BITOLA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSICAO_BITOLA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AMS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEPTOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RADIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATIVACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MONITOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VOLANTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONECTIVIDADCONECTIV] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONFIGURACOES_AMS] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMACAO_PREPARACA] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CREEPER_CR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TROCA_DE_EIXO_3_METR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TROCAPNEUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESOS_CONFIGURACAO_F] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTALACAO_CABINE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTALACAO_VCRINSTAL] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COTACAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FDD] datetime NULL,
    [TROCA_DE_ASSENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PREVISAO_ENTREGA_PRE] datetime NULL,
    [DATA_ENTREGA] datetime NULL,
    [TRANSPORTE_ENTREGA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSPORTADORA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FRETE] decimal(14,2) NULL,
    [PRODUTO_NOVO_OU_USAD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_NUMERO1] decimal(14,0) NULL,
    [DESCONTO_INCONDICION] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSPORTE_ENTREGA1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MEU_PRIMEIRO_JD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ESPERADA_PARA_E] datetime NULL,
    [DATA_DE_USO_PARA_SAZ] datetime NULL,
    [JD_QUOTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_CONTRATO_GFC] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACO__7484C68C5C8F3886] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMP_VEND_MAQUINAS] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__5132705A] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMP_VENDA_DIRETA
   Criada em ..: 2015-10-02
   Alterada em : 2019-04-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMP_VENDA_DIRETA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CLIENTE_SAM] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q002_COLHEDORAS] decimal(1,0) NULL,
    [Q002_COLHEITADEIRAS] decimal(1,0) NULL,
    [Q002_PULVERIZADORES] decimal(1,0) NULL,
    [Q002_TRATORES] decimal(1,0) NULL,
    [MODELO_DO_EQUIPAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__PEDIDO] decimal(14,0) NULL,
    [VALOR_DA_VENDA] decimal(14,2) NULL,
    [FRETE] decimal(14,2) NULL,
    [ICMS] decimal(14,2) NULL,
    [PIS___COFINS] decimal(14,2) NULL,
    [BASE_DE_CALCULO] decimal(14,2) NULL,
    [__COMISSAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMISSAO_PROVISIONAD] decimal(14,2) NULL,
    [VALOR_FINAL_COMISSAO] decimal(14,2) NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__NF_COMISSAO_VEND_] decimal(14,0) NULL,
    [DATA_MARCADO_ENTREGU] datetime NULL,
    [TIPO_CLIENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACOMP_VENDA__5FA9E18C] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMP_VENDA_DIRETA] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__609E05C5] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMP_VENDA_DIRETAJD
   Criada em ..: 2017-01-19
   Alterada em : 2023-07-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMP_VENDA_DIRETAJD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_DE_CLIENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [C1__JOHN_DEERE_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODEL_EQUIP1] varchar(51) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_PEDIDO] decimal(14,0) NULL,
    [__COMISSAO_FABRICA] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMISSAO_PROVISIONAD] decimal(14,2) NULL,
    [VALOR_COMISSAO_RECEB] decimal(14,2) NULL,
    [NF_COMISSAO_COLORADO] decimal(14,0) NULL,
    [CHASSI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_FABRICA_CLIENTE] decimal(14,0) NULL,
    [VALOR_VENDA_CLIENTE] decimal(14,2) NULL,
    [DATA_ENTREGA] datetime NULL,
    [ITENS_BONIFICADOS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_COM_AMS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AMS_DE_FABRICA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO_DO_AMS] varchar(51) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_PESO_TRAS] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESO_TRASEIRO___KG] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESO_TRASEIRO___POSI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANT_PESO_TRAS2] decimal(14,0) NULL,
    [PESO_TRASEIRO__2____] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESO_TRASEIRO__2_POS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_PESO_DIAN] decimal(14,0) NULL,
    [RODADO_TRASEIRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_DIANTEIRO] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_VCR_100] decimal(14,0) NULL,
    [QUANTIDADE_VCR_300] decimal(14,0) NULL,
    [CHASSI_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [_CHASSI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_DE_EQUIPA] decimal(14,0) NULL,
    [_CHASSI_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [_CHASSI__] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [__CHASSI_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHA_SSI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI11] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI12] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI13] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI14] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI15] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_PEDIDO] datetime NULL,
    [COTACAO] decimal(14,0) NULL,
    [NOTA_FISCAL] decimal(14,0) NULL,
    [ITENS_BONIF_PECAS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_BONIF_PECAS] decimal(14,2) NULL,
    [DESCR_PECAS_BONIF] varchar(400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BONIFICACAO_SERVICOS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_BONIF_SERVICOS] decimal(14,2) NULL,
    [DESCR_SERV_BONIFICAD] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BONIFICACAO_AMS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_BONIFICACAO_AMS] decimal(14,2) NULL,
    [DESCRICAO_AMS_BONIF] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTA_FISCAL_1] decimal(14,0) NULL,
    [NOTA_FISCAL_2] decimal(14,0) NULL,
    [NOTA_FISCAL_3] decimal(14,0) NULL,
    [NOTA_FISCAL_4] decimal(14,0) NULL,
    [NOTA_FISCAL_5] decimal(14,0) NULL,
    [NOTA_FISCAL_6] decimal(14,0) NULL,
    [NOTA_FISCAL_7] decimal(14,0) NULL,
    [NOTA_FISCAL_8] decimal(14,0) NULL,
    [NOTA_FISCAL_9] decimal(14,0) NULL,
    [NOTA_FISCAL_10] decimal(14,0) NULL,
    [NF_DESC_INCONDI_SERV] decimal(14,0) NULL,
    [NF_DESC_INCONDI_PECA] decimal(14,0) NULL,
    [NF_DESC_INCOND_AMS] decimal(14,0) NULL,
    [DATA_ENTREGA1] datetime NULL,
    [DATA_ENTREGA2] datetime NULL,
    [DATA_ENTREGA3] datetime NULL,
    [DATA_ENTREGA4] datetime NULL,
    [DATA_ENTREGA5] datetime NULL,
    [DATA_ENTREGA6] datetime NULL,
    [DATA_ENTREGA7] datetime NULL,
    [DATA_ENTREGA8] datetime NULL,
    [DATA_ENTREGA9] datetime NULL,
    [VALIDACAO_GESTOR] datetime NULL,
    [VLR_CHASSI_1] decimal(14,2) NULL,
    [VLR_CHASSI_2] decimal(14,2) NULL,
    [VLR_CHASSI_3] decimal(14,2) NULL,
    [VLR_CHASSI_4] decimal(14,2) NULL,
    [VLR_CHASSI5] decimal(14,2) NULL,
    [VLR_CHASSI6] decimal(14,2) NULL,
    [NUMERO_COTACAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERTO_COTA_2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_COTA_3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_COTA_4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_COTA_5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_FORMULARIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_DE_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOVALOR_BONIFICACA] decimal(14,2) NULL,
    [TIPO_BTIPO_ONIFICACA] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACO__7484C68C8DC3FA9F] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMP_VENDA_DIRETAJD] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__6681C7A4] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMP_VENDA_FINANC
   Criada em ..: 2025-04-24
   Alterada em : 2026-07-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMP_VENDA_FINANC] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VENDA_FINANC_CHASSI] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_FINANC_NRO_NF] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_FINANC_DATAFAT] datetime NULL,
    [VENDA_FINANC_DATAENT] datetime NULL,
    [VENDA_FINANC_FILIAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_PROPOSTA] decimal(20,0) NULL,
    [DATA_PROPOSTA] datetime NULL,
    [COMAR] decimal(8,0) NULL,
    [DATA_PREVISAO_FAT] datetime NULL,
    [COMAR_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACO__7484C68CDE4E2645] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMP_VENDA_FINANC] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__7BBDA703] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMP_VENDA_LOCACAO
   Criada em ..: 2017-11-09
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMP_VENDA_LOCACAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_DE_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DO_EQUIPAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_TORRE] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_RODADO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BITOLA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACESSORIOS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IMPLEMENTOS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EASY_MANAGER___COMPU] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL_DA_NEGOC] decimal(14,2) NULL,
    [DATA_INICIO_DA_LOCAC] datetime NULL,
    [DATA_TERMINO_LOCACAO] datetime NULL,
    [QUANTIDADE_DE_EQUIPA] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_ACO__7484C68C71293718] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMP_VENDA_LOCACAO] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__70D538E5] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMP_VENDA_MANITOU
   Criada em ..: 2017-06-05
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMP_VENDA_MANITOU] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_EQUIP_MANITOU] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_M] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIPM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N_PEDIDO_VDM] decimal(14,0) NULL,
    [TIPO_TORRE] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_RODADO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BITOLA] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACESSORIOS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IMPLEMENTOS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EASY_MANAGER] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTRAS_CONFIG] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AGREGA_DESAGREG] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTA] decimal(14,2) NULL,
    [VALOR_SINAL] decimal(14,2) NULL,
    [DATA_SINAL] datetime NULL,
    [VALOR_FINANC] decimal(14,2) NULL,
    [TIPO_PGTO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_PGTO] datetime NULL,
    [DATA_ATUALIZADA] datetime NULL,
    [COD_FINAME] decimal(14,0) NULL,
    [TX_FLAT] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAXA_FLT_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INST_FINANC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N_AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_AGENC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_GER_AGENC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEL_AGENC] decimal(14,0) NULL,
    [CONTATO_AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INF_FINANC] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_EQUIP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [APROV_CRED] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__DO_PAC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAC_EMITIDO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N_CONTRATO] decimal(14,0) NULL,
    [PREVISAO_DE_FATURAME] datetime NULL,
    [DATA_PED_VD] datetime NULL,
    [DATA_PROC_BC] datetime NULL,
    [N__NF] decimal(14,0) NULL,
    [DATA_FAT] datetime NULL,
    [NF_REFAT] decimal(14,0) NULL,
    [OBS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODEL_EQUIP1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACO__7484C68C99FC29B0] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMP_VENDA_MANITOU] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__49F075EE] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMP_VENDA_USADO
   Criada em ..: 2017-09-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMP_VENDA_USADO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_EQUIP_MANITOU] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_M] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIPM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N_PEDIDO_VDM] decimal(14,0) NULL,
    [TIPO_TORRE] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BITOLA] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACESSORIOS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IMPLEMENTOS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EASY_MANAGER] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTRAS_CONFIG] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AGREGA_DESAGREG] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTA] decimal(14,2) NULL,
    [VALOR_SINAL] decimal(14,2) NULL,
    [DATA_SINAL] datetime NULL,
    [VALOR_FINANC] decimal(14,2) NULL,
    [TIPO_PGTO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_PGTO] datetime NULL,
    [DATA_ATUALIZADA] datetime NULL,
    [COD_FINAME] decimal(14,0) NULL,
    [TX_FLAT] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAXA_FLT_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INST_FINANC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N_AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_AGENC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_GER_AGENC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEL_AGENC] decimal(14,0) NULL,
    [CONTATO_AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INF_FINANC] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_EQUIP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [APROV_CRED] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__DO_PAC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAC_EMITIDO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N_CONTRATO] decimal(14,0) NULL,
    [PREVISAO_DE_FATURAME] datetime NULL,
    [DATA_PED_VD] datetime NULL,
    [DATA_PROC_BC] datetime NULL,
    [N__NF] decimal(14,0) NULL,
    [DATA_FAT] datetime NULL,
    [NF_REFAT] decimal(14,0) NULL,
    [QTDE_VCR] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DO_EQUIPAMENT] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACO__7484C68CFDE148D1] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMP_VENDA_USADO] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__1B007CDB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMPAN_COMPRA_JDE
   Criada em ..: 2014-12-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMPAN_COMPRA_JDE] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TP_EQUIP2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCAJD2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MDL_EQUIP2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QT2] decimal(14,0) NULL,
    [FDD2] datetime NULL,
    [QTD_PES_TRS2] decimal(14,0) NULL,
    [QTD_PES_TRS_OPC22] decimal(14,0) NULL,
    [QTD_PES_DIAN2] decimal(14,0) NULL,
    [RDD_TRS_JD2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RDD_DIAN_JD2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTD_VCR_100_JD2] decimal(14,0) NULL,
    [QTDE_VCR_300_JD2] decimal(14,0) NULL,
    [KG_TRAS1_JD2] decimal(14,2) NULL,
    [KG_TRS2_JD2] decimal(14,2) NULL,
    [KG_DIANT_JD2] decimal(14,2) NULL,
    [COMPLEMENTO_OBS] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACOMPAN_COM__18F76D81] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMPAN_COMPRA_JDE] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__19EB91BA] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMPAN_VEND_CONCESS
   Criada em ..: 2021-01-25
   Alterada em : 2021-01-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMPAN_VEND_CONCESS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [FORMA_PAGAMENTO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AMS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONFIGURACOES] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTA_FISCAL] decimal(14,0) NULL,
    [MODELO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACO__7484C68CEBC40BD3] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMPAN_VEND_CONCESS] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__7B92BE7A] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMPANH_VENDA_JDE
   Criada em ..: 2014-12-22
   Alterada em : 2022-02-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMPANH_VENDA_JDE] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MEU_1_JD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIP1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODEL_EQUIP1] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_PED_VEND1] decimal(14,0) NULL,
    [POSICAO_TRAS1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSICAO_PESO_TRAS2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTR_LIQ_TRAS1] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTR_DIANT_LIQ1] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_TRAS1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_DIANT1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTD_VRC_1001] decimal(14,0) NULL,
    [QTD_VCR_3001] decimal(14,0) NULL,
    [AMS1] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AMS_DE_FABRICA1] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO_AMS1] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTRAS_CONFIG1] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_TOTAL1] decimal(14,2) NULL,
    [VLR_SINAL1] decimal(14,2) NULL,
    [DTA_SINAL1] datetime NULL,
    [VLR_FINAN1] decimal(14,2) NULL,
    [TP_PGTO1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTITUI_FINANCEIRA1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_AGENC1] decimal(14,0) NULL,
    [NOME_AGENC1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_GERENT1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEL_AGENCIA1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAIL_AGENCIA1] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INF_PARA_FINANC1] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_CHASSI1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROB_APROV_CRED1] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_PAC1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_CONTR1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PREV_FATURAM1] datetime NULL,
    [NRO_NF1] decimal(14,0) NULL,
    [COD_FIN_MDA_CONS1] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_PESO_TRAS1] decimal(14,2) NULL,
    [KG] decimal(14,2) NULL,
    [QDTE_PESO_TRAS2] decimal(14,2) NULL,
    [KG_TRAS_2] decimal(14,2) NULL,
    [QTDE_PESO_DIAN1] decimal(14,2) NULL,
    [KG_PESO_DIAN1] decimal(14,2) NULL,
    [INFORMACOES_AGREGA__] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_COMBINADA_CLIEN] datetime NULL,
    [EXPECTATIVA_DATA_CLI] datetime NULL,
    [DATA_ATUALIZADA] datetime NULL,
    [ORIGEM_FATURAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FATURAMENTO_REALIZ] datetime NULL,
    [DATA_PEDIDO_DE_VENDA] datetime NULL,
    [PAC_EMITIDO] datetime NULL,
    [BITOLA] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_MDA] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAXA_FLAT] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAXA_FLAT__] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_DE_REFATURAMENTO] decimal(14,0) NULL,
    [DATA_INICIO_BANCO] datetime NULL,
    [CALCULO_DA_BITOLA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q058_RADIO_450MHZ] decimal(1,0) NULL,
    [Q058_KIT_AL207195] decimal(1,0) NULL,
    [VALIDACAO_GESTOR] datetime NULL,
    [VALOR_DO_USADO] decimal(14,2) NULL,
    [VALOR_DA_AVALIACAO_U] decimal(14,2) NULL,
    [HAVERA_USADO_NA_NEGO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONFIRMACAO_DE_DADOS] datetime NULL,
    [BONIFICACAO_DESCONTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_BONIFICACAO_DE] decimal(14,2) NULL,
    [TIPO_DO_FRETE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DO_FRETE] decimal(14,2) NULL,
    [NOME_TRANSPORTADORA] varchar(45) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL_EMBUTIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q058_MONITOR_GEN4_42] decimal(1,0) NULL,
    [Q058_MONITOR_GEN4_46] decimal(1,0) NULL,
    [Q058_ATIVACAO_RTK_RE] decimal(1,0) NULL,
    [Q058_ATIVACAO_SF3_RE] decimal(1,0) NULL,
    [Q058_RADIO_450_MHZ] decimal(1,0) NULL,
    [Q058_RADIO_900_MHZ] decimal(1,0) NULL,
    [Q058_ATU_300] decimal(1,0) NULL,
    [Q058_RECEPTOR_SF6000] decimal(1,0) NULL,
    [Q058_MONITOR_GS3_263] decimal(1,0) NULL,
    [Q058_SF_600] decimal(1,0) NULL,
    [Q058_VOLANTE_200] decimal(1,0) NULL,
    [Q058_CHICOTE_PF906] decimal(1,0) NULL,
    [Q058_SUPORTEPF903] decimal(1,0) NULL,
    [Q058_CHICOTE_A] decimal(1,0) NULL,
    [Q058_ROTACAO] decimal(1,0) NULL,
    [Q058_AUTOTRAV_SF3] decimal(1,0) NULL,
    [Q058_READY] decimal(1,0) NULL,
    [Q058_RADIO_900MHZ] decimal(1,0) NULL,
    [MONITOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEPTOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RADIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VOLANTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATIVACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONECTIVIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CREEPER] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TROCA_DE_EIXO_3_METR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TROCA_DE_RODADO_E_PN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESOS_CONFIGURACAO_F] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTALACAO_DE_CABINE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TROCA_DE_ASSENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTALACAO_DE_VCR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FDD] decimal(14,0) NULL,
    [COTACAO] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_ACOMPANH_VE__3C40A9BE] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMPANH_VENDA_JDE] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__3D34CDF7] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMPANHA_COMPRA_IMP
   Criada em ..: 2015-02-27
   Alterada em : 2021-06-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMPANHA_COMPRA_IMP] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TP_EQUIP2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCAJD2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MDL_EQUIP2] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QT2] decimal(14,0) NULL,
    [FDD2] datetime NULL,
    [QTD_PES_TRS2] decimal(14,0) NULL,
    [KG_TRAS1_JD2] decimal(14,2) NULL,
    [QTD_PES_TRS_OPC22] decimal(14,0) NULL,
    [KG_TRS2_JD2] decimal(14,2) NULL,
    [QTD_PES_DIAN2] decimal(14,0) NULL,
    [KG_DIANT_JD2] decimal(14,2) NULL,
    [RDD_TRS_JD2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RDD_DIAN_JD2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTD_VCR_100_JD2] decimal(14,0) NULL,
    [QTDE_VCR_300_JD2] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_ACOMPANHA_C__41C478EA] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMPANHA_COMPRA_IMP] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__42B89D23] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMPANHAM_VENDA_IMP
   Criada em ..: 2013-10-08
   Alterada em : 2021-08-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMPANHAM_VENDA_IMP] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_IMPLEMENTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL_IMPLEM] decimal(14,2) NULL,
    [TIPO_PAGTO_IMPL] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUM_PED_FABRICA] decimal(14,0) NULL,
    [PROBAB_APR_CREDITO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_PREV_FATURAM] datetime NULL,
    [MARCA_IMPLEMENTO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_SINAL_IMPL] decimal(14,2) NULL,
    [VALOR_FINAN_IMPL] decimal(14,2) NULL,
    [NUM_PED_VENDA] decimal(14,0) NULL,
    [NUM_CHASSI_IMPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INST_FINANC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_REC_PROP] decimal(14,2) NULL,
    [EMAIL_CONT] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCR_DET_DIMENS] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_NOTAFISCAL] decimal(14,0) NULL,
    [TIP_IMPLEMENTO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_DO_PAC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AGENCIA_BANC] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_GER_BANCO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEL_AGENCIA_BANCO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COD_FINAME_MDA_CONSO] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DO_SINAL] datetime NULL,
    [INFORMACOES_PARA_O_F] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_CONTRATO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFO_AGREGA_DESAGREG] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_COMBINADA_CLIEN] datetime NULL,
    [DTA_PEDIDO] datetime NULL,
    [BONIFICACAO_DESCONTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_BONIFICACAO_DE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [USADO_NA_NEGOCIACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_IMPLEMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL_EMBUTIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIG_FATUR_IMPL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOCAL_DE_FATURAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACOMPANHAM___5C6D822E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMPANHAM_VENDA_IMP] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__5D61A667] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ACOMPANHAMENTO_VENDA
   Criada em ..: 2012-01-02
   Alterada em : 2023-12-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ACOMPANHAMENTO_VENDA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_D_EQUIPAME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR] decimal(14,2) NULL,
    [TIPO_DE_FINANCIAME] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_DO_GERENTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TELEFONE_DA_AGENCI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROPOSTA_ENTREGUE_] datetime NULL,
    [PROPOSTA_PROTOCOLA] datetime NULL,
    [PROCESSO_MONTADO] datetime NULL,
    [PROCESSO_EM_ANALIS] datetime NULL,
    [PROCESSO_APROVADO] datetime NULL,
    [PROCESSO_NAO_APROV] datetime NULL,
    [CONTRATO_EMITIDO] datetime NULL,
    [CONTRATO_REGISTRAD] datetime NULL,
    [PAC_EMITIDO] datetime NULL,
    [FATURAMENTO_REALIZ] datetime NULL,
    [N__PEDIDO_NA_FABRI] decimal(14,0) NULL,
    [TIPO_DE_MAQUINA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROB_APROVACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTA_PREVFAT] datetime NULL,
    [ORIGEM_FATURAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_SINAL] decimal(14,2) NULL,
    [VLR_FINANCIADO] decimal(14,2) NULL,
    [NRO_PEDIDO] decimal(14,0) NULL,
    [NRO_CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INST_FINANCEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_REC_PROPRIO] decimal(14,2) NULL,
    [NRO_CONTRATO] decimal(14,0) NULL,
    [NRO_PAC] decimal(14,0) NULL,
    [EMAIL_CONTATO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESC_IMPLEMENTO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_NF] decimal(14,0) NULL,
    [NRO_AGENCIA] decimal(14,0) NULL,
    [TIPO_IMPLEMENTO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTD_PESO_DIANTEIRO_] decimal(14,0) NULL,
    [QTD_PESO_TRASEIRO_] decimal(14,0) NULL,
    [LASTRO_LIQUIDO_DIANT] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTRO_LIQUIDO_TRAS_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_DIANT__] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_TRAS__] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTRAS_CONFIGURACOES] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMACOES_PARA_O_F] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENTROU_USADO_NA_NEGO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSSUI_AGREGADOS] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSSUI_AMS_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PFA] datetime NULL,
    [NRO_COTACAO_FABRICA] decimal(14,0) NULL,
    [FDD] datetime NULL,
    [DATA_PEDIDO_DE_VENDA] datetime NULL,
    [HOUVE_ENTREGA_ANTECI] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PEDIDO_TIRADO_PARA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ACOMPANHAME__7953D99F] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ACOMPANHAMENTO_VENDA] ADD CONSTRAINT [FK__IV_Q_ACOM__SEQQU__7A47FDD8] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ADM_FINANCEIRO
   Criada em ..: 2023-12-28
   Alterada em : 2024-05-09
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ADM_FINANCEIRO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CNPJ_CPF_DO_COMPRADO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_PROTOCOLO_NO_BA] datetime NULL,
    [DATA_AUTORIZACAO_FAT] datetime NULL,
    [DATA_FATURAMENTO] datetime NULL,
    [NRO_NF] decimal(14,0) NULL,
    [DATA_LIBERACAO_DO_RE] datetime NULL,
    [VALOR_RECEBIDO] decimal(14,2) NULL,
    [DATA_DA_PROPOSTA] datetime NULL,
    [PFA___PREVISAO_DE_FA] datetime NULL,
    [NUMERO_DA_PROPOSTA] decimal(14,0) NULL,
    [LOJA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ADM__7484C68C84BA3103] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ADM_FINANCEIRO] ADD CONSTRAINT [FK__IV_Q_ADM___SEQQU__1D9DBDBB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFE_VENDA_MQ_IM_USAD
   Criada em ..: 2013-03-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFE_VENDA_MQ_IM_USAD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [Q001_ZERO] decimal(1,0) NULL,
    [Q001_UM] decimal(1,0) NULL,
    [Q001_DOIS] decimal(1,0) NULL,
    [Q001_TRES] decimal(1,0) NULL,
    [Q001_QUATRO] decimal(1,0) NULL,
    [Q001_CINCO] decimal(1,0) NULL,
    [Q001_SEIS] decimal(1,0) NULL,
    [Q001_SETE] decimal(1,0) NULL,
    [Q001_OITO] decimal(1,0) NULL,
    [Q001_NOVE] decimal(1,0) NULL,
    [Q001_DEZ] decimal(1,0) NULL,
    [Q002_INSATISFEITO] decimal(1,0) NULL,
    [Q002_INDIFERENTE] decimal(1,0) NULL,
    [Q002_SATISFEITO] decimal(1,0) NULL,
    [RECOM_COLORADO_JD_VE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q006_INSATISFEITO] decimal(1,0) NULL,
    [Q006_INDIFERENTE] decimal(1,0) NULL,
    [Q006_SATISFEITO] decimal(1,0) NULL,
    [Q007_ESTRUTURA] decimal(1,0) NULL,
    [Q007_AREA_VENDAS] decimal(1,0) NULL,
    [Q007_AREA_PECAS] decimal(1,0) NULL,
    [Q007_AREA_SERVICO] decimal(1,0) NULL,
    [Q007_ORGANIZACAO] decimal(1,0) NULL,
    [Q007_ESPACO_FISICO] decimal(1,0) NULL,
    [PESQUISA_MQ_IMP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE_VENDA_M__4E89772B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFE_VENDA_MQ_IM_USAD] ADD CONSTRAINT [FK__IV_Q_AFE___SEQQU__4F7D9B64] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_CSC
   Criada em ..: 2021-03-02
   Alterada em : 2021-03-03
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_CSC] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VOCE_RECEBEU_EM_SEU_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VOCE_CHEGOU_A_ABRIR_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SE_SIM__DE_1_A_5_QUE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VOCE_RECEBEU_ALGUM_T] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EM_UMA_ESCALA_DE_MUI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAIS_INFORMACOES_SA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DE_UMA_NOTA_DE_0_A_1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C8AFD2C81] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_CSC] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__1B0B69D3] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_DE_PECAS
   Criada em ..: 2020-06-18
   Alterada em : 2020-06-18
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_DE_PECAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_ATENDIMEN] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CUMPRIMENTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUGESTAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTA_NPS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTO_DISP] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUGESTAO_PECAS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C5D398166] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_DE_PECAS] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__77F737C0] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_IMPLEMENTO
   Criada em ..: 2020-08-27
   Alterada em : 2020-08-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_IMPLEMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_ATENDIMEN] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [A_AREA_COMERCIAL_ATE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMBINADO_CUMPRIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIENTOU_EQUIPAMENTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSULTAR_CONTATO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO_FINANCEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRAZO_ENTREGA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENTREGA_REALIZAD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FICOU_SATISFEITO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UTILIZOU_PRODUTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESEMPENHO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NPS_0_10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68CEA35B86D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_IMPLEMENTO] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__5849823D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_MAQ_1_CONT
   Criada em ..: 2020-07-23
   Alterada em : 2020-07-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_MAQ_1_CONT] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ESTA_SATISFEITO_COM_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DURANTE_NEGOCIACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMBINADO_CUMPRIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UTILIZOU_DEMONSTRACA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FINANCEIRO_ADEQUADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRAZO_CUMPRIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [A_ENTREGA_TECNICA_FO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NPS_0_A_10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FICOU_SATISFEITO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_AVIALIACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO_FINANCEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C28A09B5A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_MAQ_1_CONT] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__312FB51C] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_MAQ_1_WEB
   Criada em ..: 2020-07-23
   Alterada em : 2020-07-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_MAQ_1_WEB] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_VENDA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATENDEU_NEGOCIACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FOI_CUMPRIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROGRAMA_DEMONSTRACA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO_FINANCEIRO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRAZO_ENTREGA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENTREGA_TECNICA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NPS_MAQUINA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C7DE1275E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_MAQ_1_WEB] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__3CA167C8] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_MAQ_2_CONT
   Criada em ..: 2020-07-23
   Alterada em : 2020-07-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_MAQ_2_CONT] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [UTILIZA_PRODUTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONTATO_CONSULTOR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_QUEM_FALAR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECISOU_ATENDIMENTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EM_UMA_ESCALA_DE_0_A] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BEM_ATENDIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFEITO_DESEMPENH] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C578C6B12] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_MAQ_2_CONT] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__21ED718C] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_MAQ_2_WEB
   Criada em ..: 2020-07-23
   Alterada em : 2020-07-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_MAQ_2_WEB] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [UTILIZA_PRODUTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONTATO_CONSULTOR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESEMPENHO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TENHA_NECESSIDADE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NPS_0_A_10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C79D5A2E0] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_MAQ_2_WEB] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__44428990] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_MAQ_3_CONT
   Criada em ..: 2020-07-23
   Alterada em : 2020-07-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_MAQ_3_CONT] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ENTROU_CONTATO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSULTOR_CONTATO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NECESSIDADE_PRODUTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NPS_0_A_10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_ESCOLHA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_DESEMPENH] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_DSIPONIBI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DISPONILIDADE_ESTOQU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_TEMPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_RESPEITO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROBLEMA_N_RESOLVIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MELHOR_EXPERIENCIA] varchar(90) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C6F2431B8] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_MAQ_3_CONT] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__298E9354] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_MAQ_3_WEB
   Criada em ..: 2020-07-24
   Alterada em : 2020-07-24
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_MAQ_3_WEB] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [RECEBEU_CONTATO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_CONSULTOR_DE_VENDA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NECESSIDADE_PRODUTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NPS_0_A_10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESCOLHA_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MELHORIA_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DISPONIBILIDADE_ESTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SOLUCAO_TEMPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_RESPEITO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROBLEMA_RESOLVIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MELHRO_EXPERIENCIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68CD3F5037C] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_MAQ_3_WEB] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__4BE3AB58] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_PECAS
   Criada em ..: 2012-03-29
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_PECAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [Q001_ZERO] decimal(1,0) NULL,
    [Q001_UM] decimal(1,0) NULL,
    [Q001_DOIS] decimal(1,0) NULL,
    [Q001_TRES] decimal(1,0) NULL,
    [Q001_QUATRO] decimal(1,0) NULL,
    [Q001_CINCO] decimal(1,0) NULL,
    [Q002_INSATISFEITO] decimal(1,0) NULL,
    [Q002_INDIFERENTE] decimal(1,0) NULL,
    [Q002_SATISFEITO] decimal(1,0) NULL,
    [Q003_INSATISFEITO] decimal(1,0) NULL,
    [Q003_INDIFERENTE] decimal(1,0) NULL,
    [Q003_SATISFEITO] decimal(1,0) NULL,
    [Q004_INSATISFEITO] decimal(1,0) NULL,
    [Q004_INDIFERENTE] decimal(1,0) NULL,
    [Q004_SATISFEITO] decimal(1,0) NULL,
    [RECOM_DEPTO_PECAS_CO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q006_INSATISFEITO] decimal(1,0) NULL,
    [Q006_INDIFERENTE] decimal(1,0) NULL,
    [Q006_SATISFEITO] decimal(1,0) NULL,
    [Q007_ESTRUTURA] decimal(1,0) NULL,
    [Q007_AREA_DE_VENDAS] decimal(1,0) NULL,
    [Q007_AREA_DE_PECAS] decimal(1,0) NULL,
    [Q007_AREA_DE_SERVICO] decimal(1,0) NULL,
    [Q007_ORGANIZACAO] decimal(1,0) NULL,
    [Q007_ESPACO_FISICO] decimal(1,0) NULL,
    [Q001_SEIS] decimal(1,0) NULL,
    [Q001_SETE] decimal(1,0) NULL,
    [Q001_OITO] decimal(1,0) NULL,
    [Q001_NOVE] decimal(1,0) NULL,
    [Q001_DEZ] decimal(1,0) NULL,
    [FINAL_DA_PESQUISA___] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PECA_INDISPONIVEL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFERICAO_PE__66CB1510] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_PECAS] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__67BF3949] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_POS_SOLUCAO
   Criada em ..: 2012-03-30
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_POS_SOLUCAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [Q001_INSATISFEITO] decimal(1,0) NULL,
    [Q001_INDIFERENTE] decimal(1,0) NULL,
    [Q001_SATISFEITO] decimal(1,0) NULL,
    CONSTRAINT [PK__IV_Q_AFERICAO_PO__0737E4A2] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_POS_SOLUCAO] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__082C08DB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_SERV_WEB
   Criada em ..: 2020-05-13
   Alterada em : 2020-05-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_SERV_WEB] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [OS_PRAZOS_AGENDADOS_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AGENDAMENTO_ENTREGA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXPECTATIVA_TECNICO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SERV_CORRETAMENTE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q005_1] decimal(1,0) NULL,
    [Q005_2] decimal(1,0) NULL,
    [Q005_3] decimal(1,0) NULL,
    [Q005_4] decimal(1,0) NULL,
    [Q005_5] decimal(1,0) NULL,
    [Q005_6] decimal(1,0) NULL,
    [Q005_7] decimal(1,0) NULL,
    [Q005_8] decimal(1,0) NULL,
    [Q005_9] decimal(1,0) NULL,
    [Q005_] decimal(1,0) NULL,
    [NOTANPS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEVE_RETORNO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C3A144200] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_SERV_WEB] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__578A682E] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_SERVICOS
   Criada em ..: 2012-03-29
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_SERVICOS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [Q001_INSATISFEITO] decimal(1,0) NULL,
    [Q001_INDIFERENTE] decimal(1,0) NULL,
    [Q001_SATISFEITO] decimal(1,0) NULL,
    [Q004_0] decimal(1,0) NULL,
    [Q004_1] decimal(1,0) NULL,
    [Q004_2] decimal(1,0) NULL,
    [Q004_3] decimal(1,0) NULL,
    [Q004_4] decimal(1,0) NULL,
    [Q004_5] decimal(1,0) NULL,
    [RECOMEND_DEPTO_SERVI] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q004_6] decimal(1,0) NULL,
    [Q004_7] decimal(1,0) NULL,
    [Q004_8] decimal(1,0) NULL,
    [Q004_9] decimal(1,0) NULL,
    [Q004_10] decimal(1,0) NULL,
    CONSTRAINT [PK__IV_Q_AFERICAO_SE__5B596264] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_SERVICOS] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__5C4D869D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_SUPORTE_INT
   Criada em ..: 2018-01-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_SUPORTE_INT] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [INFORME_SATISFACAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68CE7B925E5] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_SUPORTE_INT] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__6069C6F2] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_VENDA_MAQ
   Criada em ..: 2016-08-12
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_VENDA_MAQ] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_PERFORMAN] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_CUSTO_OPE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_ATENDIMEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_SETOR_HA_OPORTU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECOMENDACAO_COLORAD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROBLEMA_NRESOLVIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SR_NOS_INDICA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_COLABORADOR_DE_] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENTREGA_TECNICA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFE__7484C68C18465410] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_VENDA_MAQ] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__2F5BAFEC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AFERICAO_VENDA_MQ_IM
   Criada em ..: 2012-03-29
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AFERICAO_VENDA_MQ_IM] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [Q001_ZERO] decimal(1,0) NULL,
    [Q001_UM] decimal(1,0) NULL,
    [Q001_DOIS] decimal(1,0) NULL,
    [Q001_TRES] decimal(1,0) NULL,
    [Q001_QUATRO] decimal(1,0) NULL,
    [Q001_CINCO] decimal(1,0) NULL,
    [Q002_INSATISFEITO] decimal(1,0) NULL,
    [Q002_INDIFERENTE] decimal(1,0) NULL,
    [Q002_SATISFEITO] decimal(1,0) NULL,
    [Q003_INSATISFEITO] decimal(1,0) NULL,
    [Q003_INDIFERENTE] decimal(1,0) NULL,
    [Q003_SATISFEITO] decimal(1,0) NULL,
    [Q004_INSATISFEITO] decimal(1,0) NULL,
    [Q004_INDIFERENTE] decimal(1,0) NULL,
    [Q004_SATISFEITO] decimal(1,0) NULL,
    [RECOM_COLORADO_JD_VE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q006_INSATISFEITO] decimal(1,0) NULL,
    [Q006_INDIFERENTE] decimal(1,0) NULL,
    [Q006_SATISFEITO] decimal(1,0) NULL,
    [Q007_ESTRUTURA] decimal(1,0) NULL,
    [Q007_AREA_VENDAS] decimal(1,0) NULL,
    [Q007_AREA_PECAS] decimal(1,0) NULL,
    [Q007_AREA_SERVICO] decimal(1,0) NULL,
    [Q007_ORGANIZACAO] decimal(1,0) NULL,
    [Q007_ESPACO_FISICO] decimal(1,0) NULL,
    [Q001_SEIS] decimal(1,0) NULL,
    [Q001_SETE] decimal(1,0) NULL,
    [Q001_OITO] decimal(1,0) NULL,
    [Q001_NOVE] decimal(1,0) NULL,
    [Q001_DEZ] decimal(1,0) NULL,
    [PESQUISA_MQ_IMP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AFERICAO_VE__723CC7BC] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AFERICAO_VENDA_MQ_IM] ADD CONSTRAINT [FK__IV_Q_AFER__SEQQU__7330EBF5] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AGUARDAR_PECAS
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AGUARDAR_PECAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [QUAL_TIPO_DE_COMPRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AGU__7484C68C173E1A2A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AGUARDAR_PECAS] ADD CONSTRAINT [FK__IV_Q_AGUA__SEQQU__11F700E5] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ALTERADO_PAGAMENTO
   Criada em ..: 2025-08-07
   Alterada em : 2025-08-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ALTERADO_PAGAMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [FORMA_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTITUICAO_FINANCEI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FINANCIADO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ALT__7484C68C62FE1B79] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ALTERADO_PAGAMENTO] ADD CONSTRAINT [FK__IV_Q_ALTE__SEQQU__151364B2] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_APRESENT_EQUIPAMENTO
   Criada em ..: 2014-12-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_APRESENT_EQUIPAMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__IV_Q_APRESENT_EQ__1CC7FE65] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_APRESENT_EQUIPAMENTO] ADD CONSTRAINT [FK__IV_Q_APRE__SEQQU__1DBC229E] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_APRESENT_IMPLEMENTO
   Criada em ..: 2013-10-08
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_APRESENT_IMPLEMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [Q001_EQUIP] decimal(1,0) NULL,
    [Q001_EVENTOS] decimal(1,0) NULL,
    [Q001_FOLD] decimal(1,0) NULL,
    [Q001_VIS_CL_RE] decimal(1,0) NULL,
    [QUAL_EV] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_CLI] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_APRESENT_IM__02932B16] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_APRESENT_IMPLEMENTO] ADD CONSTRAINT [FK__IV_Q_APRE__SEQQU__03874F4F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_APRESENTACAO
   Criada em ..: 2014-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_APRESENTACAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TABELA_PADRAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q002_EVENT] decimal(1,0) NULL,
    [Q002_FOLDE] decimal(1,0) NULL,
    [Q002_NO_EQUIPAMENT] decimal(1,0) NULL,
    [Q002_VISITA_REF2] decimal(1,0) NULL,
    [QUAL_O_EVENTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_O_CLIENTE] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_APRESENTACA__292DD54A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_APRESENTACAO] ADD CONSTRAINT [FK__IV_Q_APRE__SEQQU__2A21F983] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_APRESENTACAO_JDE
   Criada em ..: 2012-01-10
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_APRESENTACAO_JDE] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TABELA_PADRAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q002_EVENT] decimal(1,0) NULL,
    [Q002_FOLDE] decimal(1,0) NULL,
    [Q002_NO_EQUIPAMENT] decimal(1,0) NULL,
    [Q002_VISITA_REF2] decimal(1,0) NULL,
    [QUAL_O_EVENTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_O_CLIENTE] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_APRESENTACA__4F2895A9] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_APRESENTACAO_JDE] ADD CONSTRAINT [FK__IV_Q_APRE__SEQQU__501CB9E2] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_APROVACAO_TCSM
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_APROVACAO_TCSM] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_RETORNO_TCSM] datetime NULL,
    CONSTRAINT [PK__IV_Q_APR__7484C68C5ABE00E1] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_APROVACAO_TCSM] ADD CONSTRAINT [FK__IV_Q_APRO__SEQQU__0A55DF1D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ATUALIZACAO_PUK
   Criada em ..: 2026-02-10
   Alterada em : 2026-02-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ATUALIZACAO_PUK] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CHASSI_DA_MAQUINA_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_ATUALIZACAO_DO] decimal(14,2) NULL,
    [CHASSI_DO_RECEPTOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_MONITOR] decimal(14,2) NULL,
    [CHASSI_DO_MONITOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_MODEM] decimal(14,2) NULL,
    [CHASSI_MODEM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [FORMA_DE_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ATU__7484C68C9488C9B3] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ATUALIZACAO_PUK] ADD CONSTRAINT [FK__IV_Q_ATUA__SEQQU__0A2BC1EB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AVAL_COLH_CANA_USADA
   Criada em ..: 2013-03-21
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AVAL_COLH_CANA_USADA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ESTADO_GERAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LATARIA_E_PINTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR___FUNCIONAMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR___VAZAMENTO_DE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR_VAZ_COMBUSTIVE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR_VAZ_AGUA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELETRICA___BAT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELET_PARTIDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAINEL_DE_INSTRUMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSMISSAO___FUNCIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSMISSAO___VAZAME] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SISTEMA_HIDRAULICO__] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SISTEMA_DE_DIRECAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ROLOS_DIVISORES] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ROLOS_PICADORES] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ELEVADOR___CONDICOES] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ELEVADOR___TALISCAS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXTRATOR_PRIMARIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXTRATOR_SECUNDARIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DA_MAQUINA___] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CABINE_DO_OPERADOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTAL_GERAL_ATRIBUID] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MATERIAL_RODANTE___S] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MATERIAL_RODANTE___L] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MATERIAL_RODANTE___R] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MAT_ROD_ROL_INFERIOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS_GERAIS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_PEDIDO_PELO_CL] decimal(14,2) NULL,
    [VALOR_APURADO_NO_MER] decimal(14,2) NULL,
    [VALOR_ATRIBUIDO_PARA] decimal(14,2) NULL,
    [VALIDADE_DA_AVALIACA] datetime NULL,
    CONSTRAINT [PK__IV_Q_AVAL_COLH_C__2A170C8B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AVAL_COLH_CANA_USADA] ADD CONSTRAINT [FK__IV_Q_AVAL__SEQQU__2B0B30C4] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AVAL_TRATORES_USADOS
   Criada em ..: 2013-03-21
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AVAL_TRATORES_USADOS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ESTADO_GERAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR___FUNCIONAMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR___VAZAMENTO_DE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR_VAZ_COMBUSTIVE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR_VAZ_AGUA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELETRICA___BAT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELET_PARTIDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELETR_FUNCIONA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSMISSAO___FUNCIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSMISSAO___VAZAME] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMBREAGEM___FUNCIONA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SISTEMA_DE_DIRECAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAINEL_DE_INSTRUMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRACAO_DIANTEIRA_AUX] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EIXO_DIANT_BUCH_TERM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EIXO_TRASEIRO___DIFE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EIXO_TRASEIRO___FREI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EIXO_TRASEIRO___VAZA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HIDRAULICO___FUNCION] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HIDRAULICO___VAZAMEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HIDRAULICO___LEVANTA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TOMADA_DE_FORCA___FU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VCR___QUANTIDADE_E_S] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BARRA_DE_TRACAO___SI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PLATAFORMA_DE_OPERAC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSSUI_IMPLEMENTO_IN] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTAL_GERAL_ATRIBUID] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIANTEIRO_DIREI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_DIR_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_DIR_MEDIDA] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_DIR_VIDA_U] decimal(14,0) NULL,
    [PNEU_DIAN_DIR_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIANTEIRO_ESQUE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_ESQ_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_ESQ_MEDIDA] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_ESQ_VIDA_U] decimal(14,0) NULL,
    [PNEU_DIAN_ESQ_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRASEIRO_DIREIT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_DIR_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_DIR_MEDIDA] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_DIR_VIDA_U] decimal(14,0) NULL,
    [PNEU_TRAS_DIR_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRASEITO_ESQUER] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_ESQ_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_ESQ_MEDIDA] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_ESQ_VIDA_U] decimal(14,0) NULL,
    [PNEU_TRAS_ESQ_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBSERVACOES_GERAIS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_PEDIDO_PELO_CL] decimal(14,2) NULL,
    [VALOR_APURADO_NO_MER] decimal(14,2) NULL,
    [VALOR_ATRIBUIDO_PARA] decimal(14,2) NULL,
    [DATA_DE_VALIDADE_DA_] datetime NULL,
    CONSTRAINT [PK__IV_Q_AVAL_TRATOR__2275EAC3] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AVAL_TRATORES_USADOS] ADD CONSTRAINT [FK__IV_Q_AVAL__SEQQU__236A0EFC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AVAL_USADO_ENTRADA
   Criada em ..: 2022-04-12
   Alterada em : 2023-03-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AVAL_USADO_ENTRADA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_USADO] decimal(14,2) NULL,
    [VALOR_AVALIACAO_USAD] decimal(14,2) NULL,
    [RETIRADO_DO_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_VALIDACAO_GEST] decimal(14,2) NULL,
    [ENTRADA_DO_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_DO_USADO] datetime NULL,
    [ANO_USADO] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_AVA__7484C68CAEF1DC4D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AVAL_USADO_ENTRADA] ADD CONSTRAINT [FK__IV_Q_AVAL__SEQQU__130033B7] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AVALIA_COLHEIT_USADA
   Criada em ..: 2013-03-21
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AVALIA_COLHEIT_USADA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ESTADO_GERAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LATARIA_E_PINTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR_FUNCIONAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR___VAZAMENTOS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELETRICA_FUNCI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELETRICA___BAT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PAINEL_DE_INSTRUMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSMISSAO___FUNCIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PLATAF_CORTE_GRAOS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PLATAF_CORTE_GRA_SER] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PLATAF_CORTE_MILHO] decimal(14,0) NULL,
    [PLATAF_CORTE_GRAOS_E] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PLATAF_CORTE_MILHO_S] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SIT_PLAT_CORTE_MILHO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SISTEMA_DE_DEBULHA__] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SACA_PALHAS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PENEIRAS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CORREIAS_E_REDUTORES] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PICADOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DISTRIBUIDOR_DE_PALH] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GRANELEIRO___SITUACA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEM_FIM_DO_GRANELEIR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TUBO_DE_DESCARGA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PLATAFORMA_DE_OPERAC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTAL_GERAL_ATRIBUID] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MED_PNEU_DIAN_DIREIT] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MED_PNEU_DIAN_ESQUER] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MED_PNEU_TRAS_DIREIT] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MED_PNEU_TRAS_ESQUER] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBSERVACOES_GERAIS] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_PEDIDO_PELO_CL] decimal(14,2) NULL,
    [VALOR_APURADO_NO_MER] decimal(14,2) NULL,
    [VALOR_ATRIBUIDO_PARA] decimal(14,2) NULL,
    [DATA_DE_VALIDADE_DA_] datetime NULL,
    [PNEU_DIANT_DIR_TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_DIR_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_DIR_VIDA_U] decimal(14,0) NULL,
    [PNEU_DIAN_DIR_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_ESQ_TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_ESQ_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_DIAN_ESQ_VIDA_U] decimal(14,0) NULL,
    [PNEU_DIAN_ESQ_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_DIR_TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_DIR_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_DIR_VIDA_U] decimal(14,0) NULL,
    [PNEU_TRAS_DIR_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_ESQ_TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_ESQ_MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEU_TRAS_ESQ_VIDA_U] decimal(14,0) NULL,
    [PNEU_TRAS_ESQ_AVARIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AVALIA_COLH__26467BA7] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AVALIA_COLHEIT_USADA] ADD CONSTRAINT [FK__IV_Q_AVAL__SEQQU__273A9FE0] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AVALIA_IMPLEM_USADO
   Criada em ..: 2019-11-21
   Alterada em : 2019-12-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AVALIA_IMPLEM_USADO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA_IMP_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_IMP_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_IMP_USADO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_IMP_USADO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_IMP_USADO] decimal(14,2) NULL,
    [MOTOR_IMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BOMBA_HIDRAULICA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENGATE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BARRA_DE_TRACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [APOIO_DE_ENGATE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTD_DISCO] decimal(14,0) NULL,
    [ESTADO_DOS_DISCOS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_DE_HASTES] decimal(14,0) NULL,
    [ESTADO_HASTES] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CALCOS_DE_PROFUNDIDA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MANCAIS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MANGUEIRAS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CARACOL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTD_BICOS] decimal(14,0) NULL,
    [SUPORTE_FILTROS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REGULADORES] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TURBINA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CABOS_CONTROLE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BARRAS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESTADO_TANQUE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESTEIRAS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CORREIAS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECOLHEDOR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MONITORES] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PINTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEUS_DIANTEIRO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESTADO_DOS_PNEUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEUS_TRASEIRO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RETIRADA_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FINAL_AVA] decimal(14,0) NULL,
    [MARCA_PNEUS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_IMP_DETALHADO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AVA__7484C68C1AC092C9] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AVALIA_IMPLEM_USADO] ADD CONSTRAINT [FK__IV_Q_AVAL__SEQQU__6054B859] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AVALIA_USADO_ENTRADA
   Criada em ..: 2013-03-20
   Alterada em : 2021-03-06
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AVALIA_USADO_ENTRADA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_DO_EQUIPAMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ENDERECO_PARA_A_AVAL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXPECTATIVA_DE_VALOR] decimal(14,2) NULL,
    [CHASSI_DO_EQUIPAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_EQUIPAMENTO] decimal(14,0) NULL,
    [VALOR_DA_AVALIACAO] decimal(14,2) NULL,
    [HORIMETRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__SERIE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTOR___FUNCIONAMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VAZAMENTOS___AGUA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VAZAMENTO___OLEO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VAZAMENTO___COMBUSTI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELETRICA___BAT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTE_ELETRICA___PAR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEUS_DIANTEIRO_DIRE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEUS_DIANTEIRO_ESQU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEUS_TRASEIRO_DIREI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PNEUS_TRASEIRO_ESQUE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FINAL_DE_AVALI] decimal(14,2) NULL,
    [COM_RETIRADA_DO_USAD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RETIRADA_DO_USADO1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMO_SERA_REALIZADO_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_CABINE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESO_DIANTEIRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PESO_TRASEIRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMBREAGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TERCEIRO_PONTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRANSMISSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SISTEMA_INDUSTRIAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ELEVADOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOJA_ENTRADA_DO_USAD] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_AVALIA_USAD__656CDC83] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AVALIA_USADO_ENTRADA] ADD CONSTRAINT [FK__IV_Q_AVAL__SEQQU__666100BC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_AVALIACAO_AMS_USADO
   Criada em ..: 2021-02-07
   Alterada em : 2021-02-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_AVALIACAO_AMS_USADO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR] decimal(14,2) NULL,
    [VALOR_FINAL_DEFINIDO] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_AVA__7484C68C3EC8C808] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_AVALIACAO_AMS_USADO] ADD CONSTRAINT [FK__IV_Q_AVAL__SEQQU__06104CED] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CADASTROS_LISTAS
   Criada em ..: 2012-01-02
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CADASTROS_LISTAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ORIGEM_DA_RENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_CADASTROS_L__00F4FB67] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CADASTROS_LISTAS] ADD CONSTRAINT [FK__IV_Q_CADA__SEQQU__01E91FA0] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CANCEL_RENOVACAO_SEG
   Criada em ..: 2025-05-07
   Alterada em : 2025-05-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CANCEL_RENOVACAO_SEG] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CANCEL_RENOV_SEGURO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CANCEL_RENOV_PRECO] decimal(14,2) NULL,
    [CANCEL_SEGUR_MOTIVO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CANCEL_RENOV_SEG_APO] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_CAN__7484C68CB11CA168] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CANCEL_RENOVACAO_SEG] ADD CONSTRAINT [FK__IV_Q_CANC__SEQQU__0AFFEA93] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CANCELAMENTO_SEGURO
   Criada em ..: 2024-12-26
   Alterada em : 2024-12-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CANCELAMENTO_SEGURO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SEGURADORA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO] decimal(14,2) NULL,
    [MOTIVO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [APOLICE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_CAN__7484C68C37302ACA] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CANCELAMENTO_SEGURO] ADD CONSTRAINT [FK__IV_Q_CANC__SEQQU__5E0D488B] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CANHOTO_DIGITAL
   Criada em ..: 2025-06-13
   Alterada em : 2025-06-13
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CANHOTO_DIGITAL] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CANHOTO_NRO_NF] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CANHOTO_RESP_RETIRAD] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CANHOTO_DOC_RESP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CANHOTO_DATA_RETIRAD] datetime NULL,
    CONSTRAINT [PK__IV_Q_CAN__7484C68C95BD098A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CANHOTO_DIGITAL] ADD CONSTRAINT [FK__IV_Q_CANH__SEQQU__1824DB87] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CHASSI_ENTREGA_FISIC
   Criada em ..: 2024-11-27
   Alterada em : 2024-11-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CHASSI_ENTREGA_FISIC] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_CHA__7484C68CEAD9926A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CHASSI_ENTREGA_FISIC] ADD CONSTRAINT [FK__IV_Q_CHAS__SEQQU__1E27CDA0] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CHASSI_EQUIPAMENTO
   Criada em ..: 2024-05-09
   Alterada em : 2024-05-09
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CHASSI_EQUIPAMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOJA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_CHA__7484C68C8546023A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CHASSI_EQUIPAMENTO] ADD CONSTRAINT [FK__IV_Q_CHAS__SEQQU__6ADD33C4] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CHASSI_PMP
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CHASSI_PMP] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_PMP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUM_SOLUCAO_DTAC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_CHA__7484C68CDEAAA16D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CHASSI_PMP] ADD CONSTRAINT [FK__IV_Q_CHAS__SEQQU__199822AD] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CHEGADA_IMPLEMENTO
   Criada em ..: 2013-10-08
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CHEGADA_IMPLEMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DTA_CHEGADA_IMPL] datetime NULL,
    CONSTRAINT [PK__IV_Q_CHEGADA_IMP__2C8964E2] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CHEGADA_IMPLEMENTO] ADD CONSTRAINT [FK__IV_Q_CHEG__SEQQU__2D7D891B] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COM_INTERESSE_FUTURO
   Criada em ..: 2026-03-31
   Alterada em : 2026-03-31
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COM_INTERESSE_FUTURO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [INTERESSE_F_TIPO_NEG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERESSE_F_TIPO_EQ] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERESSE_F_MARCA_EQ] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERESSE_F_MODELO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERESSE_F_COMPRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERESSE_F_NIVEL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERESSE_F_MOTIVO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INTERESSE_F_VALOR] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_COM__7484C68C917BC6FF] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COM_INTERESSE_FUTURO] ADD CONSTRAINT [FK__IV_Q_COM___SEQQU__4A113CD6] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMISSAO
   Criada em ..: 2015-02-19
   Alterada em : 2021-07-20
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMISSAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR_DO_EQUIPAMENTO] decimal(14,2) NULL,
    [DESCONTO_INCONDICION] decimal(14,2) NULL,
    [VALOR_DO_EQUIP__USAD] decimal(14,2) NULL,
    [VALOR_BASE_COMISSAO] decimal(14,2) NULL,
    [__COMISSAO__RH_] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAO] decimal(14,2) NULL,
    [COMISSAO_PAGA___RH_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINHA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_COMISSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERC_COMISSAO] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACELERADOR___RISCO__] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACELERADOR___MARGEM_] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q013_TREINAMENTO] decimal(1,0) NULL,
    [Q013_AGRICULTURA_DE_] decimal(1,0) NULL,
    [Q013_VENDA_1__TRATOR] decimal(1,0) NULL,
    [Q013_CONTRATOS_DE_PE] decimal(1,0) NULL,
    [VENDA_DE_PACOTE_OU_P] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UTILIZACAO_DE_MARGEM] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DE_VENDA_SEM_I] decimal(14,2) NULL,
    [ESPECIALISTA_NA_VEND] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESPECIALISTA_VENDA_A] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPANHA_PROMOCIONAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_TOTAL_COMISSAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSSUI_ACEL_PROD_PAC] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL_EMBUTIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_COMISSAO_ESPECIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_DESCONTO_INCO] decimal(14,2) NULL,
    [COMISSAO_DESCONTO_IN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARGEM_LUCRO_OPE] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_COMISSAO_CEN] decimal(14,2) NULL,
    [VLR_COMISSAO_ADC_ESP] decimal(14,2) NULL,
    [VLR_COMISSAO_ADC_CEN] decimal(14,2) NULL,
    [VALOR_COMISSAO_ESP] decimal(14,2) NULL,
    [VALOR_COMISSAO_CEN] decimal(14,2) NULL,
    [MARGEM_LUCRO_OPERACI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_COMISSAO__2EB1A476] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMISSAO] ADD CONSTRAINT [FK__IV_Q_COMI__SEQQU__2FA5C8AF] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMISSAO_AMS
   Criada em ..: 2017-09-08
   Alterada em : 2021-05-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMISSAO_AMS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR_DO_AMS] decimal(14,2) NULL,
    [CEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESPECIALISTA_AMS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [__COMISSAO_CEN] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAO_CEN] decimal(14,2) NULL,
    [__COMISSAO_ESPECIALI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALORCOMISSAO_ESPECI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMISSAO_PAGA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FINAL_DA_COMIS] decimal(14,2) NULL,
    [OBSERVACAO_GERAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_COMISSAO_CEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_COMISSAO_ESPECIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VLR_TOTAL_COMISSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL_EMBUTIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMISSAO__ESPECIAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMISSAO__CEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALORCOMISSAO_CEN] decimal(14,2) NULL,
    [VALORCOMISSAO_ESPEC] decimal(14,2) NULL,
    [MARGEM_LUCRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_COM__7484C68CFA423269] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMISSAO_AMS] ADD CONSTRAINT [FK__IV_Q_COMI__SEQQU__0140AAD8] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMISSAO_CONTACHAVE
   Criada em ..: 2020-03-19
   Alterada em : 2020-08-06
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMISSAO_CONTACHAVE] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR_VENDA] decimal(14,2) NULL,
    [CEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [Q004_TRATORES_LINHA5] decimal(1,0) NULL,
    [Q004_TRATORES_LINHA6] decimal(1,0) NULL,
    [Q004_TRATORES_LINHA7] decimal(1,0) NULL,
    [Q004_TRATORES_LINHA8] decimal(1,0) NULL,
    [Q004_COLHEDORAS] decimal(1,0) NULL,
    [Q004_COLHEITADEIRAS_] decimal(1,0) NULL,
    [Q004_GREEN_SYSTEM] decimal(1,0) NULL,
    [Q004_PLANTADEIRAS] decimal(1,0) NULL,
    [Q004_PULVERIZADORES] decimal(1,0) NULL,
    [VALOR_COMISSAO] decimal(14,2) NULL,
    [PREMIO_PAGO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EQUIPAMENTO_AMS_FULL] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESPECIALISTA_VENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PREMIO_ESPECIALISTA] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_COM__7484C68C2E3F6B41] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMISSAO_CONTACHAVE] ADD CONSTRAINT [FK__IV_Q_COMI__SEQQU__0956CDEC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMISSAO_SERV_AMS
   Criada em ..: 2017-12-21
   Alterada em : 2020-06-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMISSAO_SERV_AMS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_DE_SERVICO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DO_SERVICO] decimal(14,2) NULL,
    [ESPECIALISTA_AMS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [__COMISSAO_CEN] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [__COMISSAO_ESPECIALI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CEN] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAO_ESPEC] decimal(14,2) NULL,
    [VALOR_COMISSAO_CEN] decimal(14,2) NULL,
    [CEN_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAO_ESP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAOCEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [_COMISSAO_CEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [_COMISSAO_ESPEC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMISSAO_PAGA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_COM__7484C68C15584266] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMISSAO_SERV_AMS] ADD CONSTRAINT [FK__IV_Q_COMI__SEQQU__58C8A52A] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMISSAO_USADO
   Criada em ..: 2017-09-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMISSAO_USADO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR_DO_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDICACAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [__COMISSAO__RH_] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [R__COMISSAO__RH_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_COM__7484C68CFDF57907] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMISSAO_USADO] ADD CONSTRAINT [FK__IV_Q_COMI__SEQQU__135F5B13] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMISSAO_VD_LOCACAO
   Criada em ..: 2017-11-09
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMISSAO_VD_LOCACAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [QUANTIDADE_DE_EQUIPA] decimal(14,0) NULL,
    [TIPO_DE_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DO_EQUIPAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_MODELO_EQUIPAMEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__SERIE] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_INICIO_LOCACAO] datetime NULL,
    [DATA_TERMINO_LOCACAO] datetime NULL,
    [VALOR_MENSAL_DO_CONT] decimal(14,2) NULL,
    [VALOR_TOTAL_DO_CONTR] decimal(14,2) NULL,
    [DATA_RENOVACAO] datetime NULL,
    CONSTRAINT [PK__IV_Q_COM__7484C68C9984F8C7] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMISSAO_VD_LOCACAO] ADD CONSTRAINT [FK__IV_Q_COMI__SEQQU__16FAE1CD] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMPETID_NEGOC_IMPL
   Criada em ..: 2013-10-08
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMPETID_NEGOC_IMPL] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA_IMPL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA_CONC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOD_IMPLEMENTO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_IMPL] decimal(14,0) NULL,
    [PRECO_CONCORR] decimal(14,2) NULL,
    [COND_PGTO_CONCORR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_PGTO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMPETID_NEGOC] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_COMPETID_NE__7AF2094E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMPETID_NEGOC_IMPL] ADD CONSTRAINT [FK__IV_Q_COMP__SEQQU__7BE62D87] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMPETIDORES_NA_NEG
   Criada em ..: 2014-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMPETIDORES_NA_NEG] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_COMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [PRECO_CONCORRENTE] decimal(14,2) NULL,
    [CONDICOES_PAGTO_CO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONDICOES_PAGTO_JO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXISTE_COMP] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_COMPETIDORE__218CB382] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMPETIDORES_NA_NEG] ADD CONSTRAINT [FK__IV_Q_COMP__SEQQU__2280D7BB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COMPETIDORES_NA_NEGO
   Criada em ..: 2012-01-02
   Alterada em : 2024-01-31
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COMPETIDORES_NA_NEGO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_COMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [PRECO_CONCORRENTE] decimal(14,2) NULL,
    [CONDICOES_PAGTO_CO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONDICOES_PAGTO_JO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXISTE_COMP] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_COMPETIDORE__758348BB] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COMPETIDORES_NA_NEGO] ADD CONSTRAINT [FK__IV_Q_COMP__SEQQU__76776CF4] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CONDICOES_DE_VENDAS
   Criada em ..: 2025-08-07
   Alterada em : 2025-08-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CONDICOES_DE_VENDAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [COND_TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_MARCA_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_MODELO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_QUANTIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_CONFIGURACAO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_PRECO] decimal(14,2) NULL,
    [COND_BONIFICACAO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_COND_PAGTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COND_COMISSAO] decimal(14,2) NULL,
    [COND_PRAZO_ENTREGA] datetime NULL,
    CONSTRAINT [PK__IV_Q_CON__7484C68C9514B28B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CONDICOES_DE_VENDAS] ADD CONSTRAINT [FK__IV_Q_COND__SEQQU__0D7242EA] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_CONT_COMISSAO_22
   Criada em ..: 2022-04-05
   Alterada em : 2022-04-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_CONT_COMISSAO_22] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR_EQUIPAMENTO_IM] decimal(14,2) NULL,
    [VALOR_AMS_ADICIONAL] decimal(14,2) NULL,
    [__COMISSAO__RH_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAO___RH] decimal(14,2) NULL,
    [COMISSAO_PAGA_RH] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINHA_DE_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HOUVE_PARTICIPACAO_V] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ESPECIALISTA_PARTICI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSSUI_ACELERADOR___] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAO_ESPEC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARGEM_LUCRO_OPERACI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_COMISSAO_CEN] decimal(14,2) NULL,
    [COMISSAO_ADICIONAL_E] decimal(14,2) NULL,
    [COMISSAO_AMS_ADICION] decimal(14,2) NULL,
    [VALOR_PREMIO_ESPECIA] decimal(14,2) NULL,
    [MARGEM_LUCROOPERACI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_CON__7484C68C7E231B2A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_CONT_COMISSAO_22] ADD CONSTRAINT [FK__IV_Q_CONT__SEQQU__7D10F298] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_COTA_CONSORCIO
   Criada em ..: 2025-07-10
   Alterada em : 2025-07-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_COTA_CONSORCIO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CONSORCIO_NOME_CLIEN] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSORCIO_LOJA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSORCIO_CEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSORCIO_DATA] datetime NULL,
    [CONSORCIO_GRUPO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSORCIO_COTA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSORCIO_VALOR] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_COT__7484C68C55368A7F] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_COTA_CONSORCIO] ADD CONSTRAINT [FK__IV_Q_COTA__SEQQU__430F398C] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMO_EQUIP_JD
   Criada em ..: 2026-02-26
   Alterada em : 2026-02-26
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMO_EQUIP_JD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DEMO_JD_MODELO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_CLIENTE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_USO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_ESP_CSC] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_CHASSI] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_VALOR_DEMO] decimal(14,2) NULL,
    [DEMO_JD_QTDE_NEGOC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_OBJ_DEMO] varchar(70) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_ENDERECO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_DATA] datetime NULL,
    [DEMO_JD_RESPONSAVEL] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_CONTATO_RESP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_EMAIL_RESP] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_OPERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_IMPLEMENTO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_HORIMET_INIC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_HORIM_FINAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_DIESEL_GASTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_NIVEL_TANQUE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_AREA_TRAB] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMO_JD_DEPOIMENTO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_DEM__7484C68CB585539E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMO_EQUIP_JD] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__26C80099] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMO_IMPLEMENTO
   Criada em ..: 2015-01-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMO_IMPLEMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_DEMO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__SERIE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MUNICIPIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMONSTRADOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODAGEM_DIANTEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODAGEM_TRASEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTREAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_SERVICO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_DO_IMPLEMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DO_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GRUPO_CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCHA_UTILIZADA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RPM_DO_MOTOR] decimal(14,0) NULL,
    [VELOCIDADE_KM_H] decimal(14,0) NULL,
    [PROFUNDIDADE_DE_CO] decimal(14,0) NULL,
    [HORAS_TRABALHADAS] decimal(14,0) NULL,
    [HORA_INICIAL] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORA_FINAL] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DIAS_TRABALHADOS] decimal(14,0) NULL,
    [OPERADOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LITROS_CONSUMIDOS] decimal(14,0) NULL,
    [TONELADAS_TRABALHA] decimal(14,0) NULL,
    [HECTARES_TRABALHAD] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_DEMO_IMPLEM__7949B7FE] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMO_IMPLEMENTO] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__7A3DDC37] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMO_MAQUINAS
   Criada em ..: 2020-05-04
   Alterada em : 2021-03-04
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMO_MAQUINAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ENDERECO_ENTREGA] varchar(600) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OPERACAO_DEMO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_IDEAL] datetime NULL,
    [APROVAR_GERENTE] datetime NULL,
    [CULTURAS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AREA_TRABALHADA] decimal(14,0) NULL,
    [POTENCIAL_DE_COMPRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_DEMONSTRACAO] decimal(14,0) NULL,
    [RESULTADAO_DEMONSTRA] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIPA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_AGENDADA] datetime NULL,
    [DATA_REALIZADA] datetime NULL,
    [TIPO_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_NOTA] datetime NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_EQUIPAMENTO] decimal(14,2) NULL,
    [MARCA1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_DEM__7484C68C97DE9164] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMO_MAQUINAS] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__4B249149] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMO_TRATOR
   Criada em ..: 2014-12-19
   Alterada em : 2024-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMO_TRATOR] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_DEMO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__SERIE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MUNICIPIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMONSTRADOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODAGEM_DIANTEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODAGEM_TRASEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTREAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_SERVICO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_DO_IMPLEMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DO_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GRUPO_CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCHA_UTILIZADA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RPM_DO_MOTOR] decimal(14,0) NULL,
    [VELOCIDADE_KM_H] decimal(14,0) NULL,
    [PROFUNDIDADE_DE_CO] decimal(14,0) NULL,
    [HORAS_TRABALHADAS] decimal(14,0) NULL,
    [HORA_INICIAL] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORA_FINAL] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DIAS_TRABALHADOS] decimal(14,0) NULL,
    [OPERADOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LITROS_CONSUMIDOS] decimal(14,0) NULL,
    [TONELADAS_TRABALHA] decimal(14,0) NULL,
    [HECTARES_TRABALHAD] decimal(14,0) NULL,
    [TIPO_DE_MAQUINA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_DEMO_TRATOR__255D4466] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMO_TRATOR] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__2651689F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMONSTR_COLHEITAD
   Criada em ..: 2013-03-20
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMONSTR_COLHEITAD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_PLANTADEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_PLANTADEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_SERIE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMONSTRADOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_PLANTIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_TRATOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONFIG_TRATOR] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_DEMONSTR_CO__5DCBBABB] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMONSTR_COLHEITAD] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__5EBFDEF4] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMONSTRACAO_JD
   Criada em ..: 2023-08-31
   Alterada em : 2026-03-18
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMONSTRACAO_JD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CONCESSIONARIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOJA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DEMONSTRACAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FAZENDA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DEMONSTRACAO] datetime NULL,
    [RESPONSAVEL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CELULAR_RESPONSAVEL] decimal(14,0) NULL,
    [E_MAIL_CLIENTE] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CNPJ] decimal(14,0) NULL,
    [OPERACAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IMPLEMENTO_AGRICOLA] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO_INICIAL] decimal(14,0) NULL,
    [HORIMETRO_FINAL] decimal(14,0) NULL,
    [DIESEL_GASTO] decimal(14,0) NULL,
    [NIVEL_DO_TANQUE] decimal(14,0) NULL,
    [AREA_TRABALHADA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEPOIMENTO_DO_CLIENT] varchar(999) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CELULAR_DO_RESPONSAV] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETROINICIAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETROFINAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NIVEL_TANQUE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OPERACAO1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SERA_NECESSARIO_ENTR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CLIENTE_JOHN_DEERE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSSUI_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ITEM_TECNOLOGIA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DEMONSTRACAO] decimal(14,2) NULL,
    [QTD_PRODUTOS_NEG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBJETIVO_DEMO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_DEM__7484C68C41C443AE] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMONSTRACAO_JD] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__32CDE4CB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMONSTRACAO_LOG
   Criada em ..: 2026-02-04
   Alterada em : 2026-02-04
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMONSTRACAO_LOG] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR_FRETE__ENTREGA] decimal(14,2) NULL,
    [VALOR_FRETE_RETORNO] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_DEM__7484C68C72C0A913] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMONSTRACAO_LOG] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__7DC5EB06] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMONSTRACAO_NF
   Criada em ..: 2026-01-19
   Alterada em : 2026-01-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMONSTRACAO_NF] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_NF] decimal(20,0) NULL,
    CONSTRAINT [PK__IV_Q_DEM__7484C68CC98C5EC3] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMONSTRACAO_NF] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__6F77CBAF] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEMONSTRACAO_TRATOR
   Criada em ..: 2012-01-09
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEMONSTRACAO_TRATOR] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_DEMO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__SERIE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MUNICIPIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMONSTRADOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODAGEM_DIANTEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODAGEM_TRASEIRA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTREAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_SERVICO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_DO_IMPLEMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DO_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GRUPO_CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCHA_UTILIZADA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RPM_DO_MOTOR] decimal(14,0) NULL,
    [VELOCIDADE_KM_H] decimal(14,0) NULL,
    [PROFUNDIDADE_DE_CO] decimal(14,0) NULL,
    [HORAS_TRABALHADAS] decimal(14,0) NULL,
    [DIAS_TRABALHADOS] decimal(14,0) NULL,
    [OPERADOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LITROS_CONSUMIDOS] decimal(14,0) NULL,
    [TONELADAS_TRABALHA] decimal(14,0) NULL,
    [HECTARES_TRABALHAD] decimal(14,0) NULL,
    [HORA_INICIAL] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORA_FINAL] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_DEMONSTRACA__29F710FA] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEMONSTRACAO_TRATOR] ADD CONSTRAINT [FK__IV_Q_DEMO__SEQQU__2AEB3533] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEVOLUCAO_PECA
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEVOLUCAO_PECA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_NF] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_DEV__7484C68CAEE6BD47] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEVOLUCAO_PECA] ADD CONSTRAINT [FK__IV_Q_DEVO__SEQQU__21394475] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DEVOLUCAO_PUK
   Criada em ..: 2025-08-27
   Alterada em : 2025-08-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DEVOLUCAO_PUK] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NF_DEVOLUCAO] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_DEV__7484C68CA8ADF160] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DEVOLUCAO_PUK] ADD CONSTRAINT [FK__IV_Q_DEVO__SEQQU__44C277D4] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_DOC_ANALISE_CREDITO
   Criada em ..: 2016-08-08
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_DOC_ANALISE_CREDITO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_DE_CLIENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q002_COPIA_ULTIMO] decimal(1,0) NULL,
    [Q002_D_R_E_DEMONS] decimal(1,0) NULL,
    [Q002_FATUR] decimal(1,0) NULL,
    [Q002_CONT_SOCIAL] decimal(1,0) NULL,
    [Q002_COMPROV_END] decimal(1,0) NULL,
    [Q002_TELEFONE] decimal(1,0) NULL,
    [Q002_EMAIL7] decimal(1,0) NULL,
    [Q002_TRES_REFERENCIA] decimal(1,0) NULL,
    [Q002_REF_BANC] decimal(1,0) NULL,
    [Q003_DECA3] decimal(1,0) NULL,
    [Q003_IR3] decimal(1,0) NULL,
    [Q003_CPF3] decimal(1,0) NULL,
    [Q003_RG3] decimal(1,0) NULL,
    [Q003_COPIA_COMPROVA3] decimal(1,0) NULL,
    [Q003_FONE3] decimal(1,0) NULL,
    [Q003_EMAIL3] decimal(1,0) NULL,
    [Q003_TRES_COM3] decimal(1,0) NULL,
    [Q003_TRES_BANC3] decimal(1,0) NULL,
    [Q004_CPF4] decimal(1,0) NULL,
    [Q004_RG4] decimal(1,0) NULL,
    [Q004_COPIA_COMPROVAN] decimal(1,0) NULL,
    [Q004_COPIA_DO_HOLERI] decimal(1,0) NULL,
    [Q004_FONE4] decimal(1,0) NULL,
    [Q004_EMAIL4] decimal(1,0) NULL,
    [Q004_TRES_COM4] decimal(1,0) NULL,
    [Q004_TRES_BANC4] decimal(1,0) NULL,
    CONSTRAINT [PK__IV_Q_DOC__7484C68C9635851D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_DOC_ANALISE_CREDITO] ADD CONSTRAINT [FK__IV_Q_DOC___SEQQU__27BA8E24] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_EVENTOS_AFERICAO
   Criada em ..: 2019-04-05
   Alterada em : 2023-04-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_EVENTOS_AFERICAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [QUAL_EVENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_SR_A___GOSTOU_DO_N] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMO_O_SR_A___SOUBE_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEM_ALGUMA_SUGESTAO_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_BASE_NO_ULTIMO_E] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DE_MODO_GERAL_A_FEIR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEM_UMA_VISITA_AGEND] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESEJA_AGENDAR_UMA_V] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [A_DATA_E_DURACAO_DO_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_HORARIO_FOI_CONVEN] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DE_FORMA_GERAL__COMO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_EVE__7484C68C6734B103] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_EVENTOS_AFERICAO] ADD CONSTRAINT [FK__IV_Q_EVEN__SEQQU__6CEF9968] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_EXP_FLUXO_MODELER
   Criada em ..: 2024-07-25
   Alterada em : 2024-07-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_EXP_FLUXO_MODELER] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO] decimal(14,0) NULL,
    [NOME] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_EXP__7484C68CED4D834C] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_EXP_FLUXO_MODELER] ADD CONSTRAINT [FK__IV_Q_EXP___SEQQU__77430AA9] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_FORA_SERVICO_PMP
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_FORA_SERVICO_PMP] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DO_EVENTO] datetime NULL,
    [DESCRICAO_DANO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAUSA_DANO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_FOR__7484C68C749D19C0] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_FORA_SERVICO_PMP] ADD CONSTRAINT [FK__IV_Q_FORA__SEQQU__28DA663D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_FORM_TREINO
   Criada em ..: 2018-01-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_FORM_TREINO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__IV_Q_FOR__7484C68C574B5ABD] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_FORM_TREINO] ADD CONSTRAINT [FK__IV_Q_FORM__SEQQU__643A57D6] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_GAR_DATA_SERVICO
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_GAR_DATA_SERVICO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [INICIO_SERVICO] datetime NULL,
    [TERMINO_SERVICO] datetime NULL,
    CONSTRAINT [PK__IV_Q_GAR__7484C68CE2F4E24D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_GAR_DATA_SERVICO] ADD CONSTRAINT [FK__IV_Q_GAR___SEQQU__307B8805] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_GAR_FAB_SOL_PECA
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_GAR_FAB_SOL_PECA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_OS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_GAR__7484C68CB8714A26] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_GAR_FAB_SOL_PECA] ADD CONSTRAINT [FK__IV_Q_GAR___SEQQU__381CA9CD] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_GESTAO_CREDITO
   Criada em ..: 2022-04-01
   Alterada em : 2023-05-31
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_GESTAO_CREDITO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [VALOR_DO_SINAL] decimal(14,2) NULL,
    [DATA_SINAL] datetime NULL,
    [VALOR_FINANCIADO] decimal(14,2) NULL,
    [VALOR_USADO] decimal(14,2) NULL,
    [VALOR_AVALIACAO_USAD] decimal(14,2) NULL,
    [BONIFICACAO_DESCONTO] decimal(14,2) NULL,
    [SINAL_EMSINAL_EMBUTI] decimal(14,2) NULL,
    [FORMA_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_MDA] decimal(14,0) NULL,
    [INSTITUICAO_FIINSTIT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FATURAMENTO_REALIZAD] datetime NULL,
    [NOTA_FISCAL] decimal(14,0) NULL,
    [INSTITUICAO_FINANCEI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANALISE_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GESTOR___TELEFONE___] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_ANALISE_BC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ENVIO_DATA_ENVI] datetime NULL,
    [ANALISE_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_PROCESSO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROBABILIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_BC_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANALISE_BC_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ENVIO__3_] datetime NULL,
    [DATA_ENVIO__2_] datetime NULL,
    [DATA_RESPOSTA__1_] datetime NULL,
    [DATA_RESPOSTA__2_] datetime NULL,
    [DADATA_RESPOSTATA_RE] datetime NULL,
    [NF_REFATURAMENTO] decimal(14,0) NULL,
    [PROCESSO_STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_DESCONTO_INCONDIC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAXA_FLAT] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_FINAME] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOCAL_FATURAMENTO] varchar(21) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_BONIFICACAO_DES] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DESCONTO_INCON] decimal(14,2) NULL,
    [DETALHES_DESCONTO_IN] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TAXTAXA_FLAT_FLAT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_ANALISE_BANCO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_IMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_AMS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRATOR_NUMERO] decimal(14,0) NULL,
    [INF_FINANCEIRO] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMACAO_DE_FATURA] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_CANCELADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_MDA1] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_DO_EQUIP] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MEU_PRIMEIRO_JD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_CONTRATO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_CONTRATOR_GFC] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_GES__7484C68CA6636464] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_GESTAO_CREDITO] ADD CONSTRAINT [FK__IV_Q_GEST__SEQQU__6074B3EA] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_GESTAO_CREDITO_AMS
   Criada em ..: 2022-04-20
   Alterada em : 2022-12-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_GESTAO_CREDITO_AMS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TAXTAXA_FLAT_FLAT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_ANALISE_BANCO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_DESCONTO_INCONDIC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [VALOR_DO_SINAL] decimal(14,2) NULL,
    [DATA_SINAL] datetime NULL,
    [RECEBIMENTO_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FINANCIADO] decimal(14,2) NULL,
    [VALOR_USADO] decimal(14,2) NULL,
    [VALOR_AVALIACAO_USAD] decimal(14,2) NULL,
    [BONIFICACAO_DESCONTO] decimal(14,2) NULL,
    [TIPO_BONIFICACAO_DES] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DESCONTO_INCON] decimal(14,2) NULL,
    [DETALHES_DESCONTO_IN] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL_EMSINAL_EMBUTI] decimal(14,2) NULL,
    [FORMA_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_FINAME] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INF_FINANCEIRO] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_MDA] decimal(14,0) NULL,
    [NUMERO_AMS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_IMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTITUICAO_FIINSTIT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO_STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_REFATURAMENTO] decimal(14,0) NULL,
    [DADATA_RESPOSTATA_RE] datetime NULL,
    [DATA_RESPOSTA__1_] datetime NULL,
    [DATA_RESPOSTA__2_] datetime NULL,
    [TAXA_FLAT] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANALISE_BC_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ENVIO__2_] datetime NULL,
    [DATA_ENVIO__3_] datetime NULL,
    [LOCAL_FATURAMENTO] varchar(21) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FATURAMENTO_REALIZAD] datetime NULL,
    [NOTA_FISCAL] decimal(14,0) NULL,
    [INSTITUICAO_FINANCEI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANALISE_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_BC_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GESTOR___TELEFONE___] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_ANALISE_BC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ENVIO_DATA_ENVI] datetime NULL,
    [ANALISE_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_PROCESSO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROBABILIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMACAO_DE_FATURA] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_CANCELADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_MDA1] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_DO_EQUIP] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_CONTRATO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_GES__7484C68C35BFE7D2] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_GESTAO_CREDITO_AMS] ADD CONSTRAINT [FK__IV_Q_GEST__SEQQU__4868202F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_GESTAO_CREDITO_IMP
   Criada em ..: 2022-04-15
   Alterada em : 2022-12-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_GESTAO_CREDITO_IMP] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TAXTAXA_FLAT_FLAT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_ANALISE_BANCO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_DESCONTO_INCONDIC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [VALOR_DO_SINAL] decimal(14,2) NULL,
    [DATA_SINAL] datetime NULL,
    [RECEBIMENTO_USADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FINANCIADO] decimal(14,2) NULL,
    [VALOR_USADO] decimal(14,2) NULL,
    [VALOR_AVALIACAO_USAD] decimal(14,2) NULL,
    [BONIFICACAO_DESCONTO] decimal(14,2) NULL,
    [TIPO_BONIFICACAO_DES] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DESCONTO_INCON] decimal(14,2) NULL,
    [DETALHES_DESCONTO_IN] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL_EMSINAL_EMBUTI] decimal(14,2) NULL,
    [FORMA_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_FINAME] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_MDA] decimal(14,0) NULL,
    [NUMERO_AMS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_IMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTITUICAO_FIINSTIT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO_STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_REFATURAMENTO] decimal(14,0) NULL,
    [DADATA_RESPOSTATA_RE] datetime NULL,
    [DATA_RESPOSTA__1_] datetime NULL,
    [DATA_RESPOSTA__2_] datetime NULL,
    [TAXA_FLAT] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANALISE_BC_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ENVIO__2_] datetime NULL,
    [DATA_ENVIO__3_] datetime NULL,
    [LOCAL_FATURAMENTO] varchar(21) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FATURAMENTO_REALIZAD] datetime NULL,
    [NOTA_FISCAL] decimal(14,0) NULL,
    [INSTITUICAO_FINANCEI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANALISE_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_BC_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GESTOR___TELEFONE___] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_ANALISE_BC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ENVIO_DATA_ENVI] datetime NULL,
    [ANALISE_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_PROCESSO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROBABILIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INF_FINANCEIRO] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMACAO_DE_FATURA] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_CANCELADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_MDA1] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_DO_EQUIP] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_CONTRATO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_GES__7484C68C7AF6636D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_GESTAO_CREDITO_IMP] ADD CONSTRAINT [FK__IV_Q_GEST__SEQQU__29E3990F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_GESTAO_PRODUTO___AMS
   Criada em ..: 2022-04-14
   Alterada em : 2023-03-28
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_GESTAO_PRODUTO___AMS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ITENS_AMS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COM_INSTALACAO_AMS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ITENS_A_AGREGAR] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ITENS_A_DESAGREGAR] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORDEM_SERVICO_AGREGA] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_ITENS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___1] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___6] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___7] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___8] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___9] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS___10] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMAR] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTO_NOVO_OU_USAD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMULARIO_NUMERO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO] decimal(14,0) NULL,
    [DESCONTO_INCONDICION] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ESPERADA_PARA_E] datetime NULL,
    [DATA_DE_USO_PARA_SAZ] datetime NULL,
    [PREVISAO_ENTREGA_PRE] datetime NULL,
    [DATA_ENTREGA] datetime NULL,
    [TRANSPORTADORA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FRETE] decimal(14,2) NULL,
    [TRANSPORTE_ENTREGA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_5] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_6] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_7] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_8] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_9] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_AMS_10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_GES__7484C68C0262FC07] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_GESTAO_PRODUTO___AMS] ADD CONSTRAINT [FK__IV_Q_GEST__SEQQU__1D7DC22A] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_GESTAO_PRODUTO_IMPL
   Criada em ..: 2022-04-14
   Alterada em : 2023-03-28
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_GESTAO_PRODUTO_IMPL] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [FORMULARIO_NFORMULAR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_IMPLEMENTO] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO_DETALHADA_] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COR_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_FABRICACAO] decimal(14,0) NULL,
    [INFORMACAO_PREPARACA] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTO_NOVO_OU_USAD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO1] decimal(14,0) NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_DO_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCONTO_INCONDICION] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ESPERADA_PARA_E] datetime NULL,
    [DATA_DE_USO_PARA_SAZ] datetime NULL,
    [PREVISAO_ENTREGA_PRE] datetime NULL,
    [DATA_ENTREGA] datetime NULL,
    [TRANSPORTADORA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FRETE] decimal(14,2) NULL,
    [TRANSPORTE_ENTREGA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_IMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_GES__7484C68C5F053E9F] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_GESTAO_PRODUTO_IMPL] ADD CONSTRAINT [FK__IV_Q_GEST__SEQQU__18B90D0D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_HORIMETRO_AGREGA
   Criada em ..: 2024-09-19
   Alterada em : 2024-09-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_HORIMETRO_AGREGA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [HORIMETRO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_HOR__7484C68C00825C56] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_HORIMETRO_AGREGA] ADD CONSTRAINT [FK__IV_Q_HORI__SEQQU__7FA34680] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_INCENTIVO
   Criada em ..: 2015-09-09
   Alterada em : 2019-08-02
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_INCENTIVO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PERC_INCENTIVO] decimal(14,0) NULL,
    [NRO_COTACAO_ALCADA] decimal(14,0) NULL,
    [BASE_CALCULO] decimal(14,2) NULL,
    [TOTAL_BASE_ALCADA] decimal(14,2) NULL,
    [MARCAR_ENTREGUE] datetime NULL,
    [MARCAR_VENDIDO] datetime NULL,
    [OUTROS_INCENTIVOS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_COTACAO] decimal(14,0) NULL,
    [BASE_OUTROS] decimal(14,2) NULL,
    [CAMPANHA1] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERC_CMP1] decimal(14,0) NULL,
    [CAMPANHA2] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERC_CMP2] decimal(14,0) NULL,
    [CAMPANHA3] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERC_CMP3] decimal(14,0) NULL,
    [CAMPANHA1JD] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CMP_1JD] decimal(14,0) NULL,
    [TOTAL_BRUTO] decimal(14,2) NULL,
    [DATA_ENVIO_XML] datetime NULL,
    [NRO_NF] decimal(14,0) NULL,
    [DATA_EMISSAO] datetime NULL,
    [TOTAL_LIQUIDO] decimal(14,2) NULL,
    [COMPRAR_VENDIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_NF_ALCADA] decimal(14,0) NULL,
    [NRO_PROC_ALCADA] decimal(14,0) NULL,
    [DTA_NF_ALCADA] datetime NULL,
    [TIPO_CAMPANHA_1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_CAMPANHA_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_CAMPANHA_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_CAMPANHA_DE_INC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_CAMPANHA_DE_INC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_INCENTIVO_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_INCENTIVO_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_INCENTIVO_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_INCENTIVO_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_INCENTIVO_4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_INCENTIVO_4] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [__CAMPANHA_4] decimal(14,0) NULL,
    [_CAMPANHA1] decimal(14,2) NULL,
    [_CAMPANHA2] decimal(14,2) NULL,
    [_CAMPANHA3] decimal(14,2) NULL,
    [_CAMPANHA4] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_INCENTIVO__1E112859] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_INCENTIVO] ADD CONSTRAINT [FK__IV_Q_INCE__SEQQU__1F054C92] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_INTERESSE_FUTURO_PRO
   Criada em ..: 2023-12-04
   Alterada em : 2023-12-04
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_INTERESSE_FUTURO_PRO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AREA_HA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LANCES] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_INT__7484C68C94F54456] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_INTERESSE_FUTURO_PRO] ADD CONSTRAINT [FK__IV_Q_INTE__SEQQU__0E5B7A2B] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_INTERESSE_PROJETO_IR
   Criada em ..: 2023-12-04
   Alterada em : 2024-01-28
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_INTERESSE_PROJETO_IR] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CULTURA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AREA_HA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LANCES] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROBABILIDADE_FECHAM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR] decimal(14,2) NULL,
    [DATA_FATURAMENTO] datetime NULL,
    [VENDEDOR_INDICACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_INT__7484C68CCBCD7666] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_INTERESSE_PROJETO_IR] ADD CONSTRAINT [FK__IV_Q_INTE__SEQQU__0A8AE947] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_LIBERAR_DEMONSTRACAO
   Criada em ..: 2026-01-19
   Alterada em : 2026-01-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_LIBERAR_DEMONSTRACAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [LOJA_PRODUTO] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSIS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_IDA] datetime NULL,
    [DATA_VOLTA] datetime NULL,
    CONSTRAINT [PK__IV_Q_LIB__7484C68CD03965E5] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_LIBERAR_DEMONSTRACAO] ADD CONSTRAINT [FK__IV_Q_LIBE__SEQQU__67D6A9E7] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_LICENCAS_PUK
   Criada em ..: 2026-02-12
   Alterada em : 2026-02-12
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_LICENCAS_PUK] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [CHASSI_MAQUINA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LICENCA_RECEPTOR] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_RECEPTOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_RECEPTOR] decimal(14,2) NULL,
    [CODIGO_RECEPTOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LICENCA_MONITOR] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_MONITOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_MONITOR] decimal(14,2) NULL,
    [CODIGO_MONITOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [FORMA_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_LIC__7484C68C23B5DEE4] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_LICENCAS_PUK] ADD CONSTRAINT [FK__IV_Q_LICE__SEQQU__1D3E965F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_LOCACAO_COMISSAO
   Criada em ..: 2017-11-09
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_LOCACAO_COMISSAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [QUANTIDADE_DE_EQUIPA] decimal(14,0) NULL,
    [TIPO_DE_EQUIPAMENTOS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DO_EQUIPAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_MODELO_EQUIPAMEN] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__SERIE] varchar(400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_INICIO_LOCACAO] datetime NULL,
    [DATA_TERMINO_LOCACAO] datetime NULL,
    [VALOR_MENSAL_DO_CONT] decimal(14,2) NULL,
    [DATA_RENOVACAO] datetime NULL,
    [VALOR_TOTAL_DO_CONTR] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_LOC__7484C68C7CB40800] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_LOCACAO_COMISSAO] ADD CONSTRAINT [FK__IV_Q_LOCA__SEQQU__2DDE4725] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_OFERECE_RENOV_SEGURO
   Criada em ..: 2024-12-27
   Alterada em : 2024-12-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_OFERECE_RENOV_SEGURO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SEGURADORA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CORRETORA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [APOLICE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VIGENCIA_INICIO] datetime NULL,
    [VIGENCIA_TERMINO] datetime NULL,
    [PREMIO_LIQUIDO] decimal(14,2) NULL,
    [QDE_PARCELAS] decimal(14,0) NULL,
    [VALOR_PARCELA] decimal(14,2) NULL,
    [DATA_VENCTO_PRIMEIRA] datetime NULL,
    [PART_PORCENT] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUM_PARCELA] decimal(14,0) NULL,
    [VALOR_PAGO] decimal(14,2) NULL,
    [HOUVE_SINISTRO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_SINISTRO_UM] datetime NULL,
    [DATA_SINISTRO_DOIS] datetime NULL,
    CONSTRAINT [PK__IV_Q_OFE__7484C68C03B3BB41] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_OFERECE_RENOV_SEGURO] ADD CONSTRAINT [FK__IV_Q_OFER__SEQQU__6D4F8C1B] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ORIGEM_DA_RENDA
   Criada em ..: 2012-01-02
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ORIGEM_DA_RENDA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ORIGEM_D_RENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ORIGEM_DA_R__0A7E65A1] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ORIGEM_DA_RENDA] ADD CONSTRAINT [FK__IV_Q_ORIG__SEQQU__0B7289DA] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_OS_ABERTA
   Criada em ..: 2024-09-19
   Alterada em : 2024-09-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_OS_ABERTA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_DA_OS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ABERTURA_OS] datetime NULL,
    CONSTRAINT [PK__IV_Q_OS___7484C68C73CFC500] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_OS_ABERTA] ADD CONSTRAINT [FK__IV_Q_OS_A__SEQQU__07446848] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_OS_CORTESIA
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_OS_CORTESIA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_OS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ABERTURA_OS] datetime NULL,
    [TIPO_GARANTIA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_OS___7484C68C6DAADA40] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_OS_CORTESIA] ADD CONSTRAINT [FK__IV_Q_OS_C__SEQQU__3FBDCB95] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_OS_GARANTIA
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_OS_GARANTIA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_OS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_ABERTURA_OS] datetime NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_GARANTIA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_OS___7484C68CA7116901] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_OS_GARANTIA] ADD CONSTRAINT [FK__IV_Q_OS_G__SEQQU__475EED5D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_OS_REVISAO_ENTREGA
   Criada em ..: 2024-09-19
   Alterada em : 2024-09-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_OS_REVISAO_ENTREGA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_OS] decimal(14,0) NULL,
    [NUMERO_DO_CHASSI] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO] decimal(14,2) NULL,
    [TIPO_REVISAO_ENTREGA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_OS___7484C68CF9D1FC10] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_OS_REVISAO_ENTREGA] ADD CONSTRAINT [FK__IV_Q_OS_R__SEQQU__0EE58A10] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PECAS_AFERICAO
   Criada em ..: 2016-08-17
   Alterada em : 2020-11-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PECAS_AFERICAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFACAO_EM_RELACA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_SETOR_HA_OPORTU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO_AS_ULTIMA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECOMENDA_A_COLORADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_SR__INDICA_UM_AMIG] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_COLABORADOR_SE_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECOMENDA] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATISFACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACORDADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUGESTAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTA_NPS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_PRODUTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSATISFACAO_COM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTO_N_ESTOQUE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DE_0_10_PRECO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO_ACIMA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PECAS_ORIGINAIS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PECAS_ORIGINAIS_PAGA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_SUA_INSATISFACA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SABE_OS_VALORES_DA_C] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TRATA_SE_DE_PRODUTOS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PEC__7484C68C119265A3] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PECAS_AFERICAO] ADD CONSTRAINT [FK__IV_Q_PECA__SEQQU__3ACD6298] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PEDIDO_GC
   Criada em ..: 2025-10-01
   Alterada em : 2025-10-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PEDIDO_GC] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [GC_TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_MODELO_EQUIPAMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_PRECO_UNITARIO] decimal(14,2) NULL,
    [GC_QUANTIDADE] decimal(10,0) NULL,
    [GC_PRECO_TOTAL] decimal(14,2) NULL,
    [GC_ENTREGA] datetime NULL,
    [GC_BONIFICACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_ITENS_BONIFICADOS] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_COMISSAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_TECNOLOGIA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_ITENS_TECNOLOGIA] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PED__7484C68C006CDDCA] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PEDIDO_GC] ADD CONSTRAINT [FK__IV_Q_PEDI__SEQQU__7DFAF530] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PEDIDO_KAM
   Criada em ..: 2025-09-03
   Alterada em : 2026-04-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PEDIDO_KAM] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_DO_PRODUTO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO] decimal(14,2) NULL,
    [COTACAO_MODELO] decimal(15,0) NULL,
    [CNPJ_CPF] decimal(18,0) NULL,
    [INSCRICAO_ESTADUAL] decimal(9,0) NULL,
    [CONDICAO_DE_PAGAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRAZO_DE_ENTREGA] datetime NULL,
    [COMISSAO] decimal(10,0) NULL,
    [SINAL] decimal(14,2) NULL,
    [_SINAL] decimal(10,0) NULL,
    [DATA_VENCIMENTO_SINA] datetime NULL,
    [SALDO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BANCO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BONIFICACAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_TEST_CONC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAIL_TEST_CONC] varchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CPF_TEST_CONC] decimal(18,0) NULL,
    [NOME_PROC_CLIENTE] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAIL_PROC_CLIENTE] varchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CPF_PROC_CLIENTE] decimal(18,0) NULL,
    [NOME_TEST_CLIENTE] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EMAIL_TEST_CLIENTE] varchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CPF_TEST_CLIENTE] decimal(18,0) NULL,
    [TIPO_VENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PED__7484C68C4A7B601E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PEDIDO_KAM] ADD CONSTRAINT [FK__IV_Q_PEDI__SEQQU__4D57BDD5] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PEDIDO_SAM
   Criada em ..: 2025-08-15
   Alterada em : 2026-04-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PEDIDO_SAM] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MODELO_DO_PRODUTO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO] decimal(14,2) NULL,
    [COTACAO_MODELO] decimal(15,0) NULL,
    [CNPJ_CPF] decimal(18,0) NULL,
    [INSCRICAO_ESTADUAL] decimal(10,0) NULL,
    [CONDICAO_DE_PAGAMENT] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRAZO_DE_ENTREGA] datetime NULL,
    [COMISSAO] decimal(10,0) NULL,
    [SINAL] decimal(14,2) NULL,
    [_SINAL] decimal(10,0) NULL,
    [DATA_VENCIMENTO_SINA] datetime NULL,
    [SALDO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BANCO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [BONIFICACAO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_VENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PED__7484C68C99C97191] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PEDIDO_SAM] ADD CONSTRAINT [FK__IV_Q_PEDI__SEQQU__348C100B] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PERCEPCAO_JD
   Criada em ..: 2025-06-05
   Alterada em : 2026-02-10
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PERCEPCAO_JD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NOTA_DEMONSTRACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POSSUI_EQP_JD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMONSTROU_INTERESSE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEMONSTRACAO_NEGOC] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_02] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ERGONOMIA_ATRATIVA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERFORMANCE_ATRATIVA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSUMO_EXPECTATIVA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AGRICULTURA_VALOR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_OPERATION] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERCEPCAO_GERAL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXPECTATIVA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSUMO_EXPEC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO_FINAL] decimal(10,0) NULL,
    CONSTRAINT [PK__IV_Q_PER__7484C68C30F528C7] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PERCEPCAO_JD] ADD CONSTRAINT [FK__IV_Q_PERC__SEQQU__7A9478A0] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PESQUISA_NPS
   Criada em ..: 2025-06-13
   Alterada em : 2025-06-13
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PESQUISA_NPS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PESQUISA_NPS_0_A_10] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PES__7484C68C02E1C1AD] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PESQUISA_NPS] ADD CONSTRAINT [FK__IV_Q_PESQ__SEQQU__1BF56C6B] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PESQUISA_TI
   Criada em ..: 2018-03-26
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PESQUISA_TI] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_COM_ATEND] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATENDIMENTO_NO_PRAZO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTA_DE_ATENDIMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PES__7484C68C5CB12F8B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PESQUISA_TI] ADD CONSTRAINT [FK__IV_Q_PESQ__SEQQU__548DFFF2] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PREMIO_DEMO
   Criada em ..: 2021-04-29
   Alterada em : 2021-04-29
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PREMIO_DEMO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_PREMIO] decimal(14,2) NULL,
    [COMISSAO_PAGA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PRE__7484C68CC8848625] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PREMIO_DEMO] ADD CONSTRAINT [FK__IV_Q_PREM__SEQQU__2E1E3E47] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PREVISAO_RECEBIMENTO
   Criada em ..: 2014-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PREVISAO_RECEBIMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_DA_PREVISAO_DE_] datetime NULL,
    CONSTRAINT [PK__IV_Q_PREVISAO_RE__349F87F6] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PREVISAO_RECEBIMENTO] ADD CONSTRAINT [FK__IV_Q_PREV__SEQQU__3593AC2F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PRODUTO_RD
   Criada em ..: 2026-03-25
   Alterada em : 2026-07-13
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PRODUTO_RD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PRODUTO_RD] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CAMPANHA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTO_TEMPO_DESEJA_] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO_MIDIA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONDICAO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMA_DE_CONTATO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COTA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERFIL_DE_COMPRA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PRO__7484C68C7D0BE65B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PRODUTO_RD] ADD CONSTRAINT [FK__IV_Q_PROD__SEQQU__417BF6D5] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PROPOSTA_COMERCIAL
   Criada em ..: 2024-01-29
   Alterada em : 2024-01-29
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PROPOSTA_COMERCIAL] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_DE_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DO_EQUIPAMENT] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [VALOR_SINAL] decimal(14,2) NULL,
    [VALOR_FINANCIADO] decimal(14,2) NULL,
    [VALOR_RECURSO_PROP] decimal(14,2) NULL,
    [TIPO_DE_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PRO__7484C68C07D1F8F1] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PROPOSTA_COMERCIAL] ADD CONSTRAINT [FK__IV_Q_PROP__SEQQU__2FBC6DF6] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_PROSPECCAO_SERV__JD
   Criada em ..: 2019-08-08
   Alterada em : 2019-08-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_PROSPECCAO_SERV__JD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [ORCAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONDICAO_DE_PAGAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORDEM_DE_SERVICO] decimal(14,0) NULL,
    [VALOR] decimal(14,2) NULL,
    [SERVICO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_PRO__7484C68C4739D21C] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_PROSPECCAO_SERV__JD] ADD CONSTRAINT [FK__IV_Q_PROS__SEQQU__1E86F4FC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_QUALIDADE_PECAS
   Criada em ..: 2015-11-06
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_QUALIDADE_PECAS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_ATEND] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDICARIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECISA_FAZER] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_SR__INDICA_UM_AMIG] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_QUALIDADE_P__07B7D2E6] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_QUALIDADE_PECAS] ADD CONSTRAINT [FK__IV_Q_QUAL__SEQQU__08ABF71F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_QUALIDADE_SERVICOS
   Criada em ..: 2015-11-06
   Alterada em : 2020-05-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_QUALIDADE_SERVICOS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_ATEND] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDICARIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECISA_FAZER] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TESTE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_QUALIDADE_S__1605F23D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_QUALIDADE_SERVICOS] ADD CONSTRAINT [FK__IV_Q_QUAL__SEQQU__16FA1676] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_QUALIDADE_VENDAMAQ
   Criada em ..: 2015-11-06
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_QUALIDADE_VENDAMAQ] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFEITO_ATEND] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDICARIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECISA_FAZER] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_QUALIDADE_V__12356159] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_QUALIDADE_VENDAMAQ] ADD CONSTRAINT [FK__IV_Q_QUAL__SEQQU__13298592] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_RECEB_FINAN_IMPLEM
   Criada em ..: 2013-10-09
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_RECEB_FINAN_IMPLEM] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_REC_FIN_IMPLE] datetime NULL,
    CONSTRAINT [PK__IV_Q_RECEB_FINAN__3AD78439] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_RECEB_FINAN_IMPLEM] ADD CONSTRAINT [FK__IV_Q_RECE__SEQQU__3BCBA872] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_RECEBIMENTO_A_PRAZO
   Criada em ..: 2025-04-11
   Alterada em : 2025-04-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_RECEBIMENTO_A_PRAZO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [RECEBIMENTO_NOME_CLI] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_CPF_CNPJ] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_CIDADE] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_ENDERECO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_NUMERO] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_BAIRRO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_CEP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_AVALISTA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_CPF_AVAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_RG_AVALI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_END_AVAL] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_FORMA_PA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_EMAIL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_EMAIL_2] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_MARGEM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEBIMENTO_TEMPO_ES] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_REC__7484C68CAF02BE21] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_RECEBIMENTO_A_PRAZO] ADD CONSTRAINT [FK__IV_Q_RECE__SEQQU__2BA1C44F] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_RECEBIMENTO_COMISSAO
   Criada em ..: 2024-02-05
   Alterada em : 2024-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_RECEBIMENTO_COMISSAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NRO_NF] decimal(14,0) NULL,
    [VLR_COMISSAO] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_REC__7484C68C30AC8DE3] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_RECEBIMENTO_COMISSAO] ADD CONSTRAINT [FK__IV_Q_RECE__SEQQU__3A39FC69] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_RECEBIMENTO_FINANC
   Criada em ..: 2012-08-16
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_RECEBIMENTO_FINANC] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PREV_RECBTO] datetime NULL,
    CONSTRAINT [PK__IV_Q_RECEBIMENTO__0055DCE9] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_RECEBIMENTO_FINANC] ADD CONSTRAINT [FK__IV_Q_RECE__SEQQU__014A0122] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_RESPONSAVEL_TECNICO
   Criada em ..: 2012-05-31
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_RESPONSAVEL_TECNICO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [RESPONSAVEL_TECNICO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_RESPONSAVEL__6E372CAE] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_RESPONSAVEL_TECNICO] ADD CONSTRAINT [FK__IV_Q_RESP__SEQQU__6F2B50E7] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_RESULTADO_DEMO
   Criada em ..: 2021-02-14
   Alterada em : 2021-02-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_RESULTADO_DEMO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFACAO_DO_CLIENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMENTARIO_DO_CLIENT] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMAS_DE_MEDICAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSUMO_DIESEL] decimal(14,0) NULL,
    [CONSUMO_ESPECIFICO] decimal(14,0) NULL,
    [CAPACIDADE_OPERACION] decimal(14,0) NULL,
    [AREA_TRABALHADA] decimal(14,0) NULL,
    [HORIMETRO_INICIAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO_FINAL] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TOTAL_HORAS_TRABALHA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AREA_TOTAL_TRABALHAD] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RENDIMENTO_OPERACION] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMBUSTIVEL_CONSUMID] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MEDIA_CONSUMO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_RES__7484C68CD3D71EC9] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_RESULTADO_DEMO] ADD CONSTRAINT [FK__IV_Q_RESU__SEQQU__09E0DDD1] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_RETORNADO_JD
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_RETORNADO_JD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_RETORNO] datetime NULL,
    CONSTRAINT [PK__IV_Q_RET__7484C68C6F80221C] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_RETORNADO_JD] ADD CONSTRAINT [FK__IV_Q_RETO__SEQQU__4F000F25] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_REV_DATA_SERVICO
   Criada em ..: 2025-01-30
   Alterada em : 2025-03-12
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_REV_DATA_SERVICO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_INICIO_SERVICO] datetime NULL,
    [DATA_TERMINO_SERVICO] datetime NULL,
    CONSTRAINT [PK__IV_Q_REV__7484C68C1A96AE21] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_REV_DATA_SERVICO] ADD CONSTRAINT [FK__IV_Q_REV___SEQQU__50493919] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_REVISAO_100H
   Criada em ..: 2025-03-21
   Alterada em : 2025-03-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_REVISAO_100H] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_INICIO_REV_100H] datetime NULL,
    [DATA_TERM_REV_100H] datetime NULL,
    CONSTRAINT [PK__IV_Q_REV__7484C68CCE82B06A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_REVISAO_100H] ADD CONSTRAINT [FK__IV_Q_REVI__SEQQU__604A96B8] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_REVISAO_1100_1150H
   Criada em ..: 2025-03-21
   Alterada em : 2025-03-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_REVISAO_1100_1150H] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_INICIO_REV_1100] datetime NULL,
    [DATA_TERM_REV_1100] datetime NULL,
    CONSTRAINT [PK__IV_Q_REV__7484C68C703FA8DC] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_REVISAO_1100_1150H] ADD CONSTRAINT [FK__IV_Q_REVI__SEQQU__7AFE8CF4] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_REVISAO_1500H
   Criada em ..: 2025-03-21
   Alterada em : 2025-03-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_REVISAO_1500H] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_INICIO_REV_1500] datetime NULL,
    [DATA_TERM_REV_1500] datetime NULL,
    CONSTRAINT [PK__IV_Q_REV__7484C68C1DB11742] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_REVISAO_1500H] ADD CONSTRAINT [FK__IV_Q_REVI__SEQQU__029FAEBC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_REVISAO_450_600H
   Criada em ..: 2025-03-21
   Alterada em : 2025-03-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_REVISAO_450_600H] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_INICIO_REV_600H] datetime NULL,
    [DATA_TERM_REV_600H] datetime NULL,
    CONSTRAINT [PK__IV_Q_REV__7484C68C0A2C386D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_REVISAO_450_600H] ADD CONSTRAINT [FK__IV_Q_REVI__SEQQU__6BBC4964] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_REVISAO_800H
   Criada em ..: 2025-03-21
   Alterada em : 2025-03-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_REVISAO_800H] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_INICIO_REV_800H] datetime NULL,
    [DATA_TERM_REV_800H] datetime NULL,
    CONSTRAINT [PK__IV_Q_REV__7484C68C34E08508] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_REVISAO_800H] ADD CONSTRAINT [FK__IV_Q_REVI__SEQQU__735D6B2C] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_REVISAO_FIM_GARANTIA
   Criada em ..: 2025-03-21
   Alterada em : 2025-03-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_REVISAO_FIM_GARANTIA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA_INICIO_REV_FIMG] datetime NULL,
    [DATA_TERM_REV_FIMG] datetime NULL,
    CONSTRAINT [PK__IV_Q_REV__7484C68C66AD7DBC] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_REVISAO_FIM_GARANTIA] ADD CONSTRAINT [FK__IV_Q_REVI__SEQQU__0A40D084] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_ROMANEIO_DEV_PECA
   Criada em ..: 2024-09-11
   Alterada em : 2024-09-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_ROMANEIO_DEV_PECA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_OS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DEVOLUCAO] datetime NULL,
    [DEVOLVIDO_PARA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_ROM__7484C68CC1F44F7D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_ROMANEIO_DEV_PECA] ADD CONSTRAINT [FK__IV_Q_ROMA__SEQQU__56A130ED] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_SEPARACAO_PEDIDO
   Criada em ..: 2025-09-08
   Alterada em : 2025-09-08
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_SEPARACAO_PEDIDO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [GC_PEDIDO_PARA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_MARCA_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_MODELO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GC_QTDE_TOTAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UNIDADE_1] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_1] decimal(10,0) NULL,
    [UNIDADE_2] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_2] decimal(10,0) NULL,
    [UNIDADE_3] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_3] decimal(10,0) NULL,
    [UNIDADE_4] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_4] decimal(10,0) NULL,
    [UNIDADE_5] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_5] decimal(10,0) NULL,
    [UNIDADE_6] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_6] decimal(10,0) NULL,
    CONSTRAINT [PK__IV_Q_SEP__7484C68C6D3EC677] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_SEPARACAO_PEDIDO] ADD CONSTRAINT [FK__IV_Q_SEPA__SEQQU__55ED03D6] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_SERVICO_EXTERNOS_JD
   Criada em ..: 2019-08-16
   Alterada em : 2020-08-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_SERVICO_EXTERNOS_JD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FROTA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONDICAO_DE_PAGAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DEFEITO_APRESENTADO] varchar(400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LOCALIZACAO_DO_EQUIP] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONTATO_DO_OPERADOR] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_SER__7484C68CE57BD022] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_SERVICO_EXTERNOS_JD] ADD CONSTRAINT [FK__IV_Q_SERV__SEQQU__328DEDA9] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_SERVICOS_AFERICAO
   Criada em ..: 2016-08-17
   Alterada em : 2020-05-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_SERVICOS_AFERICAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SATISFACAO_EM_RELACA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_SETOR_HA_OPORTU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SATIS_EM_RELACA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECOMENDACAO_A_COLOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SER_REALIZADOS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AQUILO_QUE_FOI_ACORD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATEND_TEC_EXPECTATIV] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOTA_SATISFACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_SR__INDICA_UM_AMIG] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUAL_COLABORADOR_DE_] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUESTAO_TEC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRAZO_AGENDADOS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ACORDADO_CUMPRIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TECNICO_ATENDEU] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SERV_EXECUTADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEVE_RETORNAR] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUE_NOTA_O_A__SR__A_] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_SER__7484C68C202CE128] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_SERVICOS_AFERICAO] ADD CONSTRAINT [FK__IV_Q_SERV__SEQQU__3E9DF37C] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_SOLICITACAO_TCAT
   Criada em ..: 2024-11-25
   Alterada em : 2024-11-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_SOLICITACAO_TCAT] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TCAT_DATA_SINISTRO] datetime NULL,
    [TCAT_TRANSPORTADORA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_CTRE] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_MOTORISTA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_CNH_MOTORISTA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_PLACA_CAVALO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_PLACA_CARRETA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_NF_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_DATA_NF] datetime NULL,
    [TCAT_VALOR_NF] decimal(14,2) NULL,
    [TCAT_ORIGEM] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_DESTINO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_EQUIPAMENTO] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_ANO_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_CHASSIS_EQUIP] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_OCORRENCIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_CAUSA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_ORIGEM_AVARIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_DESCRICAO] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TCAT_VALORES_PECAS] decimal(14,2) NULL,
    [TCAT_VALORES_OBRA] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_SOL__7484C68C17D63FB6] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_SOLICITACAO_TCAT] ADD CONSTRAINT [FK__IV_Q_SOLI__SEQQU__1A573CBC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_TESTE_PRIMEIRO_JD
   Criada em ..: 2024-08-20
   Alterada em : 2024-08-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_TESTE_PRIMEIRO_JD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TST_1JD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TESTE] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_TES__7484C68CED2CA4D0] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_TESTE_PRIMEIRO_JD] ADD CONSTRAINT [FK__IV_Q_TEST__SEQQU__7B139B8D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_TESTE1
   Criada em ..: 2022-08-05
   Alterada em : 2022-08-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_TESTE1] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EQPTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_TES__7484C68CB004914B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_TESTE1] ADD CONSTRAINT [FK__IV_Q_TEST__SEQQU__0B2A07C5] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_TESTE2
   Criada em ..: 2022-08-05
   Alterada em : 2022-08-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_TESTE2] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VALOR] decimal(14,2) NULL,
    [FORM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_3] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_TES__7484C68C6E2C2085] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_TESTE2] ADD CONSTRAINT [FK__IV_Q_TEST__SEQQU__1C5493C7] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_TICKET_DSI
   Criada em ..: 2025-01-21
   Alterada em : 2025-01-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_TICKET_DSI] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NOME_DO_CLIENTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORGANIZACAO_OPERATIO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TITULO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RESUMO_DO_PROBLEMA] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRODUTO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONTATO_DO_CLIENTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_TIC__7484C68C263DB53A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_TICKET_DSI] ADD CONSTRAINT [FK__IV_Q_TICK__SEQQU__4012D150] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA
   Criada em ..: 2023-12-28
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_DO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DO_EQUIPAMENT] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [NRO_PEDIDO_DE_VENDA] decimal(14,0) NULL,
    [DATA_PEDIDO_DE_VENDA] datetime NULL,
    [PEDIDO_TIRADO_PARA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM_DO_FATURAMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FINANCIADO] decimal(14,2) NULL,
    [VALOR_RECURSO_PROPRI] decimal(14,2) NULL,
    [INSTITUICAO_FINANCEI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_DO_GERENTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LINHA_DE_CREDITO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_DA_COTACAO] decimal(14,0) NULL,
    [CODIGO_DO_MODELO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRIMEIRO_JOHN_DEERE] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_COMPARTILHADA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSULTOR_1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONSULTOR_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_VENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FORMA_DE_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C22A3CCC7] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__15FC9BF3] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_AMS
   Criada em ..: 2017-03-24
   Alterada em : 2020-10-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_AMS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_PEDIDO_AMS] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DA_VENDA] decimal(14,2) NULL,
    [FORMA_DE_PAGAMENTO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Q004_RECEPTOR_0908] decimal(1,0) NULL,
    [Q004_VOLANTE_0520] decimal(1,0) NULL,
    [Q004_CHICOTE_90687] decimal(1,0) NULL,
    [Q004_SUPORTE_90385] decimal(1,0) NULL,
    [Q004_CHICOTE_ISO] decimal(1,0) NULL,
    [Q004_ANTI_PF90889] decimal(1,0) NULL,
    [Q004_AUTOTRAC_049] decimal(1,0) NULL,
    [Q004_SF3_READY] decimal(1,0) NULL,
    [Q004_RADIO_900_MHZ] decimal(1,0) NULL,
    [Q004_450MHZ] decimal(1,0) NULL,
    [Q004_KIT_AL207195] decimal(1,0) NULL,
    [Q004_MONITOR_GS3] decimal(1,0) NULL,
    [Q004_ANTI_ROTACAO] decimal(1,0) NULL,
    [Q004_ATIVACAO_RTK] decimal(1,0) NULL,
    [Q004_RTK_7307PC] decimal(1,0) NULL,
    [COM_INSTALACAO_DO_AM] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMAR_AGREGA] varchar(400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMAR_DESAGREGA_A] varchar(400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ITENS_DO_AMS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_NOTA_FISCAL] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DO_PEDIDO] datetime NULL,
    [TIPO_DE_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTITUICAO_FINANCEI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NRO_DA_AGENCIA] decimal(14,0) NULL,
    [NOME_DA_AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NOME_DO_GERENTE_DO_B] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_FINANCIADO] decimal(14,2) NULL,
    [TELEFONE_DA_AGENCIA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [E_MAIL_DE_CONTATO_DA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INFORMACOES_PARA_O_F] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PREVISAO_DO_FATURAME] datetime NULL,
    [VALOR_DO_SINAL] decimal(14,2) NULL,
    [INFORMAR_COMAR] varchar(90) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_APROVACAO] datetime NULL,
    [HAVERA_BONIFICACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_BONIFICACAO] decimal(14,2) NULL,
    [CODIGO_FINAME] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SERA_DESMEMBRADO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCONTO_INCODICIONA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NR_NOTA_FISCAL_DESCO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUMERO_OS_AGREGA_DES] decimal(14,0) NULL,
    [CHASSI_DESCONTO_INCO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FAT_CENTRO_DE_CUSTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_REFATURAMENTO_AMS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_DE_ITENS] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS_2] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS_3] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS_4] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS_5] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS_6] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS_7] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASI_DO_ITEM_AMS_8] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS_9] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI_DO_ITEM_AMS10] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OS_ADICIONAIS] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCONTO_INCODICION] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SINAL_EMBUTIDO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ID_MONITOR_1] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ID_MONITOR_2] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ID_CONTROLADORA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CA2B6C2E7] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_AMS] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__4B19A451] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_CONSORCIO
   Criada em ..: 2024-02-06
   Alterada em : 2024-02-06
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_CONSORCIO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PRODUTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [VALOR_ENTRADA] decimal(14,2) NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [GRUPO] decimal(14,0) NULL,
    [COTA] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C2A2F1F98] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_CONSORCIO] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__41DB1E31] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_DIRETA
   Criada em ..: 2025-05-19
   Alterada em : 2025-05-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_DIRETA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VD_TIRADO_PARA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_NRO_COTACAO] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_NRO_PEDIDO_VENDA] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_DATA_PEDIDO_VEND] datetime NULL,
    [VD_TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_MARCA_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_MODELO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_VALOR_TOTAL] decimal(14,2) NULL,
    [VD_BONIFICACAO] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_COMISSAO] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VD_EMAIL_NF] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C221694F1] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_DIRETA] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__4620B061] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_DSI
   Criada em ..: 2025-01-21
   Alterada em : 2025-01-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_DSI] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PEDIDO_PARA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_SERVICO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_SERVICO] decimal(14,2) NULL,
    [VALOR_DESLOCAM] decimal(14,2) NULL,
    [DATA_EXEC_SERVICO] datetime NULL,
    [SITUACAO_CADASTRAL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_EQUIPAMENTO] decimal(14,0) NULL,
    [CONDICAO_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSIS_EQUIP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CD4DF4BF7] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_DSI] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__3871AF88] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_EQUIPAMENTO
   Criada em ..: 2025-05-27
   Alterada em : 2026-01-30
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_EQUIPAMENTO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VENDA_COMPRADOR] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_JDQUOTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_TIPO_VENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_DATA_PEDIDO] datetime NULL,
    [VENDA_TIPO_EQUIPAMEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_MARCA_EQUIPAME] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_MODELO_EQUIPAM] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_QTDE_EQUIPAMEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_LASTRO_DIANT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_LASTRO_TRASEIR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_RODADO_DIANT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_RODADO_TRASEIR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_QTDE_VCR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_QTDE_VCR_DLX] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_MEDIDA_BITOLA] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_CREEPER] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_INST_CABINE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_AMS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEPTOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_AMS_ATIVACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_AMS_MONITOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_AMS_VOLANTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_AMS_RADIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDAS_AMS_CONECT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_AMS_CONFIG_ADC] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_VALOR_TOTAL] decimal(14,2) NULL,
    [VENDA_VALOR_SINAL] decimal(14,2) NULL,
    [VENDA_DATA_SINAL] datetime NULL,
    [VENDA_VALOR_FINANC] decimal(14,2) NULL,
    [VENDA_FORMA_PAGTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_INST_FINANC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_LINHA_CREDITO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_CONTATO_BANCO] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_COMPARTILHADA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_CONSULTOR_1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_CONSULTOR_2] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_PRIMEIRO_JD] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_MARGEM_NEGOC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_COTACAO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_FDD] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_CHASSI] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_INFO_PREP] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_ENTREG_PREP] datetime NULL,
    [VENDA_DATA_ENTREGA] datetime NULL,
    [VENDA_DESCONTO_INC] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_TRANSPORTADORA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VENDA_VALOR_FRETE] decimal(14,2) NULL,
    [DESCONTO_INCONDICION] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C1A8326F3] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_EQUIPAMENTO] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__60D4A69D] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_MAQUINA_FY25
   Criada em ..: 2025-02-03
   Alterada em : 2025-02-03
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_MAQUINA_FY25] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PEDIDO_TIRADO_PARA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [JD_QUOTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_EQUIPAMENTOS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_PESO_DIANTEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KG_PESO_DIANTEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_PESO_TRASEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [KG_PESO_TRASEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTRO_DIANTEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LASTRO_TRASEIRO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_DIANTEIRO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RODADO_TRASEIRO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_VCR_PADRAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_VCR_DELUXE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MEDIDA_BITOLA] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CREEPER] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTALACAO_CABINE_CA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AMS] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RECEPTOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ATIVACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MONITOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VOLANTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [RADIO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONECTIVIDADE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONFIGURACAO_ADICION] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_TOTAL] decimal(14,2) NULL,
    [VALOR_SINAL] decimal(14,2) NULL,
    [DATA_SINAL] datetime NULL,
    [VALOR_FINANCIADO] decimal(14,2) NULL,
    [FORMA_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INSTITUICAO_FINANCEI] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONTATO_BANCO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CONFIGURACAO_AMS] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COTACAO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [FDD] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHASSI] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PREPARACAO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PREVISAO_ENTREGA_PRE] datetime NULL,
    [DATA_ENTREGA] datetime NULL,
    [DESCONTO_INCONDICION] decimal(14,2) NULL,
    [TRANSPORTADORA] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DO_FRETE] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CFA2D061B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_MAQUINA_FY25] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__59D2A353] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA
   Criada em ..: 2014-12-19
   Alterada em : 2022-06-07
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA_VP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_VP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DA_VENDA] datetime NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [PRECO_CONCORRENTE] decimal(14,2) NULL,
    [PRECO_JOHN_DEERE] decimal(14,2) NULL,
    [MODELO_JOHN_DEER] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DE_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTICIPAMOS_DA_NEGO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HAVIA_MONITORAMENTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VENDA_PERDI__2CFE662E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__2DF28A67] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_FY25
   Criada em ..: 2025-07-11
   Alterada em : 2025-07-11
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_FY25] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VP_TIPO_EQUIP_CONCOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_MARCA_EQUIP_CONCO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_MODELO_EQUIP_CONC] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_COD_MODELO_CONCOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_QUANTIDADE] decimal(8,0) NULL,
    [VP_REVENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_DATA_VP] datetime NULL,
    [VP_PRECO_CONCORRENTE] decimal(14,2) NULL,
    [VP_PRECO_JD] decimal(14,2) NULL,
    [VP_MODELO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PILOTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_MOTIVO_VP] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CBF6A716B] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_FY25] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__58FE7AAB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_IMPL
   Criada em ..: 2015-05-28
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_IMPL] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA_VP] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_VP] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DA_VENDA] datetime NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [PRECO_CONCORRENTE] decimal(14,2) NULL,
    [PRECO_JOHN_DEERE] decimal(14,2) NULL,
    [MODELO_JOHN_DEER] varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VENDA_PERDI__1C5DEA11] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_IMPL] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__1D520E4A] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_IMPLEM
   Criada em ..: 2013-10-08
   Alterada em : 2020-04-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_IMPLEM] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA_VC_IMPL] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA_CONC_IMPL] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTA_DA_VENDA] datetime NULL,
    [QUANTIDADE_IMPLEMENT] decimal(14,0) NULL,
    [PRECO_CONC] decimal(14,2) NULL,
    [PRECO_JD] decimal(14,2) NULL,
    [MODELO_JD_IMPL] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO_VD_IMP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PART_NEGOCIACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HAVIA_MONITORAMENTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_DO_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [T_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VENDA_PERDI__24E8431A] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_IMPLEM] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__25DC6753] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_JDE
   Criada em ..: 2012-01-10
   Alterada em : 2024-01-31
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_JDE] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DA_VENDA] datetime NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [PRECO_CONCORRENTE] decimal(14,2) NULL,
    [PRECO_JOHN_DEERE] decimal(14,2) NULL,
    [MODELO_JOHN_DEER] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VENDA_PERDI__4B5804C5] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_JDE] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__4C4C28FE] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_MANITO
   Criada em ..: 2017-06-05
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_MANITO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PART_NEG] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA_GANH] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOD_EQUIP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DT_VENDA] datetime NULL,
    [QUANTIDAD] decimal(14,0) NULL,
    [PRECO_CONC] decimal(14,2) NULL,
    [PRECO_COL_EQ] decimal(14,2) NULL,
    [MODELO_COLORADO_EQUI] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO_EQ] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CD189A8B9] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_MANITO] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__461FE50A] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_MAQIMP
   Criada em ..: 2024-01-29
   Alterada em : 2024-09-13
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_MAQIMP] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_VENDA_VP] datetime NULL,
    [QUANTIDADE_VP] decimal(14,0) NULL,
    [PRECO_CONCORRENTE_VP] decimal(14,2) NULL,
    [PRECO_JOHN_DEERE_VP] decimal(14,2) NULL,
    [MODELO_JOHN_DEERE_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO_VP] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIP_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COD_MODE_VP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C78EAFA76] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_MAQIMP] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__281B4C2E] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_PROD
   Criada em ..: 2022-04-07
   Alterada em : 2023-07-20
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_PROD] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_DA_VENDA] datetime NULL,
    [QUANTIDADE] decimal(14,0) NULL,
    [MODELO_JOHN_DEERE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO_JOHN_DEERE] decimal(14,2) NULL,
    [PRECO_CONCORRENTE] decimal(14,2) NULL,
    [MOTIVA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TIPO_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PARTICIPAMOS_DA_NEGO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_IMPLEMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO_IMPLEMENTO] decimal(14,2) NULL,
    [HAVIA_HAACAO_DE_MONI] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CE1C15D20] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_PROD] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__04B21460] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_SEGURO
   Criada em ..: 2023-05-22
   Alterada em : 2023-05-25
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_SEGURO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [DATA] datetime NULL,
    [EQUIPAMENTOS_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OUTROS_EQUIPAMENTOS] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEGURADORA_CONCORREN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO_CONCORRENTE] decimal(14,2) NULL,
    [CONDICAO_COMERCIAL_C] decimal(14,2) NULL,
    [SEGURADORA_COLORADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO_COLORADO] decimal(14,2) NULL,
    [CONDICAO_COMERCIAL] decimal(14,2) NULL,
    [MOTIVO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO_CONCORRENTE1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO_COLORADO1] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C7FF77614] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_SEGURO] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__442D7AF7] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PERDIDA_TESTE
   Criada em ..: 2024-09-13
   Alterada em : 2024-09-13
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PERDIDA_TESTE] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODIGO_DO_MODELO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_EQUIP] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REVENDA_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_VENDA] datetime NULL,
    [PRECO_CONC] decimal(14,2) NULL,
    [PRECO_JD] decimal(14,2) NULL,
    [MODELO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO_VENDA_PERDIDA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C86468E0E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PERDIDA_TESTE] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__780224B8] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PNEUS
   Criada em ..: 2025-12-12
   Alterada em : 2025-12-12
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PNEUS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NRO_NF] decimal(10,0) NULL,
    [MEDIDA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] decimal(10,0) NULL,
    [FORMA_DE_PAGAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VALOR_DE_VENDA] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CEDE5BE58] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PNEUS] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__55B7F9AC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_PRECISION_UP
   Criada em ..: 2025-01-21
   Alterada em : 2025-01-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_PRECISION_UP] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [PUK_TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PUK_MARCA_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PUK_MODELO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PUK_QUANTIDADE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PUK_FORMA_PAGTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PUK_ITENS_AGREGA] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PUK_INSTALACAO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CC4234AB6] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_PRECISION_UP] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__218E4A30] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_SEMINOVO
   Criada em ..: 2025-07-01
   Alterada em : 2025-07-01
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_SEMINOVO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SMNV_TIPO_VENDA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMNV_TIPO_EQUIPAMENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMNV_MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMNV_MODELO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMNV_POTENCIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMNV_ANO_FABRICACAO] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMNV_HORAS_KMS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SMNV_VALOR_AVALIADO] decimal(14,2) NULL,
    [SMNV_CHASSI] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C6F5A5D7F] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_SEMINOVO] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__27671F17] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_SERVICOS_AMS
   Criada em ..: 2017-12-21
   Alterada em : 2019-12-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_SERVICOS_AMS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [NUMERO_PEDIDO] decimal(14,0) NULL,
    [VALOR_DA_VENDA] decimal(14,2) NULL,
    [FORMA_DE_PAGAMENTO] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SERVICO_VENDIDO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO_DO_SERVICO] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NF_SERVICO] decimal(14,0) NULL,
    [NF_PECAS] decimal(14,0) NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C0A86A9A5] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_SERVICOS_AMS] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__51278362] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDA_VP_PNEUS
   Criada em ..: 2025-12-12
   Alterada em : 2025-12-12
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDA_VP_PNEUS] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [MEDIDA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE] decimal(10,0) NULL,
    [VALOR_DE_VENDA] decimal(14,2) NULL,
    [FORNECEDOR] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C4D83BCB9] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDA_VP_PNEUS] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__51E768C8] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDAPERDIDA_SEGURO
   Criada em ..: 2024-12-27
   Alterada em : 2024-12-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDAPERDIDA_SEGURO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SEGURADORA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO] decimal(14,2) NULL,
    [MOTIVO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68C44039E87] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDAPERDIDA_SEGURO] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__78C13EC7] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VENDER_RENOVACAO_SEG
   Criada em ..: 2024-12-27
   Alterada em : 2024-12-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VENDER_RENOVACAO_SEG] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SEGURADORA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CORRETORA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [APOLICE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VIGENCIA_INICIO] datetime NULL,
    [VIGENCIA_TERMINO] datetime NULL,
    [PREMIO_LIQUIDO] decimal(14,2) NULL,
    [QDE_PARCELAS] decimal(14,0) NULL,
    [VALOR_PARCELA] decimal(14,2) NULL,
    [DATA_VENCTO_PRIMEIRA] datetime NULL,
    [PART_PORCENT] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NUM_PARCELA] decimal(14,0) NULL,
    [VALOR_PAGO] decimal(14,2) NULL,
    [HOUVE_SINISTRO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_SINISTRO_UM] datetime NULL,
    [DATA_SINISTRO_DOIS] datetime NULL,
    CONSTRAINT [PK__IV_Q_VEN__7484C68CCE506EEC] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VENDER_RENOVACAO_SEG] ADD CONSTRAINT [FK__IV_Q_VEND__SEQQU__65AE6A53] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VISITA_DSI
   Criada em ..: 2025-01-21
   Alterada em : 2025-01-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VISITA_DSI] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [QTDE_MQNS_CLIENTE] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [POTENCIAL_HECTARES] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDE_PROD_TEC] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VIS__7484C68C1BF74F76] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VISITA_DSI] ADD CONSTRAINT [FK__IV_Q_VISI__SEQQU__1DBDB94C] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VISITA_EXP_CLIENTE
   Criada em ..: 2018-08-13
   Alterada em : 2018-08-21
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VISITA_EXP_CLIENTE] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [FICOU_ALGUMA_DUVIDA_] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_EQUIPAMENTO_ESTA_F] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TEM_ALGUM_PROBLEMA_N] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_SR_SABE_COM_QUEM_P] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ALGO_FOI_PROMETIDO_N] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DE_0_A_10_QUAL_NOTA_] decimal(14,0) NULL,
    [O_QUE_O_SR__A__ESPER] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [O_QUE_A_COLORADO_MAQ] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COMO_ESTA_NOSSO_ATEN] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SERVICOS] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VIS__7484C68CF04ECD0F] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VISITA_EXP_CLIENTE] ADD CONSTRAINT [FK__IV_Q_VISI__SEQQU__6C658983] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VP_COLHEDORA
   Criada em ..: 2025-09-09
   Alterada em : 2025-09-09
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VP_COLHEDORA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VP_CH_TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CH_MARCA_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CH_MODELO_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CH_QTDE] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CH_RODADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CH_TECNOLOGIA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_ITENS_TECNOLOGIA] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CH_PRECO_CONC] decimal(14,2) NULL,
    [VP_CH_MODELO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CH_PRECO_JD] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_VP___7484C68CDA147D5D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VP_COLHEDORA] ADD CONSTRAINT [FK__IV_Q_VP_C__SEQQU__5AB1B8F3] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VP_COLHEITADEIRA
   Criada em ..: 2025-05-19
   Alterada em : 2025-05-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VP_COLHEITADEIRA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VP_CA_TIPO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_MARCA_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_MODELO_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_QTDE_CONC] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_PLATAFORMA_CON] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_RODADO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_TECNOLOGIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_ITENS_TECNOLOG] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_PRECO_CONC] decimal(14,2) NULL,
    [VP_CA_MODELO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_CA_PRECO_JD] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_VP___7484C68C8787214E] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VP_COLHEITADEIRA] ADD CONSTRAINT [FK__IV_Q_VP_C__SEQQU__2F3D4B09] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VP_PLANTADEIRA
   Criada em ..: 2025-05-19
   Alterada em : 2025-05-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VP_PLANTADEIRA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VP_PL_TIPO_EQUIP_CON] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_MARCA_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_MODELO_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_QTDE_LINHAS] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_CAIXA_SEMENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_DESLIGAMENTO] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_MARCADOR_LINHA] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_DOSADOR_MEC] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_MONITOR_PLANT] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_PRECO_CONC] decimal(14,2) NULL,
    [VP_PL_MODELO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PL_PRECO_JD] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_VP___7484C68CBCE880C7] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VP_PLANTADEIRA] ADD CONSTRAINT [FK__IV_Q_VP_P__SEQQU__3E7F8E99] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VP_PULVERIZADOR
   Criada em ..: 2025-07-14
   Alterada em : 2025-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VP_PULVERIZADOR] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VP_PV_TIPO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PV_MARCA_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PV_MODELO_CONC] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PV_QTDE_CONC] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PV_TECNOLOGIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PV_ITENS_TECNO] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PV_PRECO_CONC] decimal(14,2) NULL,
    [VP_PV_MODELO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PV_PRECO_JD] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_VP___7484C68C6C6BB4BF] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VP_PULVERIZADOR] ADD CONSTRAINT [FK__IV_Q_VP_P__SEQQU__6193C0AC] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VP_RENOVACAO_SEGURO
   Criada em ..: 2024-12-27
   Alterada em : 2024-12-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VP_RENOVACAO_SEGURO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [SEGURADORA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRECO] decimal(14,2) NULL,
    [MOTIVO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VP___7484C68C2FAA6443] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VP_RENOVACAO_SEGURO] ADD CONSTRAINT [FK__IV_Q_VP_R__SEQQU__7C91CFAB] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VP_SEM_PARTICIPACAO
   Criada em ..: 2025-08-27
   Alterada em : 2025-08-27
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VP_SEM_PARTICIPACAO] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [TIPO_CONCORRENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA_CONCORRENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_CONCORRENTE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COD_MODELO_CONCORREN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QUANTIDADE_VP] decimal(8,0) NULL,
    [REVENDA_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_VP] datetime NULL,
    [PRECO_VP] decimal(14,2) NULL,
    [PRECO_JD_VP] decimal(14,2) NULL,
    [MODELO_JD_VP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MOTIVO_VP] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Q_VP___7484C68C228C7F49] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VP_SEM_PARTICIPACAO] ADD CONSTRAINT [FK__IV_Q_VP_S__SEQQU__3D21560C] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Q_VP_TRATOR
   Criada em ..: 2025-07-14
   Alterada em : 2025-07-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Q_VP_TRATOR] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [VP_TIPO_EQUIPAMENTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_MARCA_CONCORRENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_MODELO_CONCORRENT] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_QUANTIDADE_EQUIP] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_VERSAO_EQUIP] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_TRANSMISSAO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_TECNOLOGIA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_ITEM_TECNOLOGIA] varchar(350) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PRECO_CONCORRENTE] decimal(14,2) NULL,
    [VP_MODELO_JD] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VP_PRECO_JD] decimal(14,2) NULL,
    CONSTRAINT [PK__IV_Q_VP___7484C68C6D22992C] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_Q_VP_TRATOR] ADD CONSTRAINT [FK__IV_Q_VP_T__SEQQU__5DC32FC8] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Questao
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Questao] (
    [SeqFormulario] numeric(18,0) NOT NULL,
    [Questao] decimal(4,0) NOT NULL,
    [Descricao] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NroReferencia] varchar(8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescReduzida] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Nomecoluna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TipoDado] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [NumMinimo] decimal(10,2) NULL,
    [NumMaximo] decimal(10,2) NULL,
    [Peso] decimal(6,2) NULL,
    [ProximaQuestao] numeric(18,0) NULL,
    [Script] varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Tamanho] numeric(4,0) NULL,
    [Prefixoresposta] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Permitepeso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    [ExigeResposta] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QTDEDECIMAL] numeric(2,0) NULL,
    CONSTRAINT [PK__IV_Questao__13BCEBC1] PRIMARY KEY CLUSTERED ([SeqFormulario], [Questao])
);
GO
CREATE NONCLUSTERED INDEX [IV_QuestaoIF1] ON [dbo].[IV_Questao] ([SeqFormulario]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_QuestaoPK] ON [dbo].[IV_Questao] ([SeqFormulario], [Questao]);
GO
ALTER TABLE [dbo].[IV_Questao] ADD CONSTRAINT [FK__IV_Questa__SeqFo__20ACD28B] FOREIGN KEY ([SeqFormulario]) REFERENCES [dbo].[IV_Formulario] ([SeqFormulario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_QuestaoLista
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_QuestaoLista] (
    [SeqFormulario] numeric(18,0) NOT NULL,
    [Questao] decimal(4,0) NOT NULL,
    [Lista] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ProximaQuestao] decimal(2,0) NULL,
    [Ordem] numeric(4,0) NULL,
    [Nomecoluna] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Peso] decimal(6,2) NULL,
    [EmUso] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_QuestaoLista__15A53433] PRIMARY KEY CLUSTERED ([SeqFormulario], [Questao], [Lista])
);
GO
CREATE NONCLUSTERED INDEX [IV_QuestaoListaIF1] ON [dbo].[IV_QuestaoLista] ([SeqFormulario], [Questao]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_QuestaoListaPK] ON [dbo].[IV_QuestaoLista] ([SeqFormulario], [Questao], [Lista]);
GO
ALTER TABLE [dbo].[IV_QuestaoLista] ADD CONSTRAINT [FK__IV_QuestaoLista__21A0F6C4] FOREIGN KEY ([SeqFormulario], [Questao]) REFERENCES [dbo].[IV_Questao] ([SeqFormulario], [Questao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Questionario
   Criada em ..: 2011-12-19
   Alterada em : 2026-03-31
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Questionario] (
    [SeqQuestionario] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(8,0) NOT NULL,
    [SeqFormulario] numeric(18,0) NOT NULL,
    [SeqHistorico] float NULL,
    [SeqAgenda] numeric(18,0) NULL,
    [DtaRealizacao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [Departamento] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Obs] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Resultado] decimal(6,0) NULL,
    [LinkDocto] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkNro] numeric(18,0) NULL,
    [LinkSerie] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Processo] decimal(15,0) NULL,
    [SubProcesso] decimal(6,0) NULL,
    [SEQPROJETO] numeric(18,0) NULL,
    CONSTRAINT [PK__IV_Questionario__178D7CA5] PRIMARY KEY CLUSTERED ([SeqQuestionario])
);
GO
CREATE NONCLUSTERED INDEX [IV_QuestionarioIF1] ON [dbo].[IV_Questionario] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_QuestionarioIF2] ON [dbo].[IV_Questionario] ([SeqFormulario]);
GO
CREATE NONCLUSTERED INDEX [IV_QuestionarioIE1] ON [dbo].[IV_Questionario] ([DtaRealizacao]);
GO
CREATE NONCLUSTERED INDEX [IV_QuestionarioIE2] ON [dbo].[IV_Questionario] ([SeqHistorico]);
GO
CREATE NONCLUSTERED INDEX [IV_QuestionarioIE3] ON [dbo].[IV_Questionario] ([Processo]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_QuestionarioPK] ON [dbo].[IV_Questionario] ([SeqQuestionario]);
GO
CREATE NONCLUSTERED INDEX [IV_QUESTIONARIE3] ON [dbo].[IV_Questionario] ([Processo]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_QUESTIONARIO] ON [dbo].[IV_Questionario] ([SEQPROJETO]);
GO
ALTER TABLE [dbo].[IV_Questionario] ADD CONSTRAINT [FK__IV_Questi__SeqFo__54AD302C] FOREIGN KEY ([SeqFormulario]) REFERENCES [dbo].[IV_Formulario] ([SeqFormulario]);
GO
ALTER TABLE [dbo].[IV_Questionario] ADD CONSTRAINT [FK__IV_Questi__SEQPR__79D4933A] FOREIGN KEY ([SEQPROJETO]) REFERENCES [dbo].[IV_Projeto] ([SeqProjeto]);
GO
ALTER TABLE [dbo].[IV_Questionario] ADD CONSTRAINT [FK__IV_Questi__SeqFo__558158D4] FOREIGN KEY ([SeqFormulario]) REFERENCES [dbo].[IV_Formulario] ([SeqFormulario]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_Questionario] ADD CONSTRAINT [FK__IV_Questi__SeqFo__1AEAA2EB] FOREIGN KEY ([SeqFormulario]) REFERENCES [dbo].[IV_Formulario] ([SeqFormulario]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Recurso
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Recurso] (
    [SeqRecurso] decimal(6,0) NOT NULL,
    [Tipo] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DescRed] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_Recurso__3E3E00C9] PRIMARY KEY CLUSTERED ([SeqRecurso])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_RecursoPK] ON [dbo].[IV_Recurso] ([SeqRecurso]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_RecUso
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_RecUso] (
    [SeqRecurso] decimal(6,0) NOT NULL,
    [SeqrecUso] numeric(18,0) NOT NULL,
    [DtaUso] datetime NULL,
    [CodUsuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqAgenda] numeric(18,0) NULL,
    [SeqHistorico] numeric(18,0) NULL,
    [Obs] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_RecUso__3F322502] PRIMARY KEY CLUSTERED ([SeqRecurso], [SeqrecUso])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_RecUsoPK] ON [dbo].[IV_RecUso] ([SeqRecurso], [SeqrecUso]);
GO
CREATE NONCLUSTERED INDEX [IV_RecUsoIF1] ON [dbo].[IV_RecUso] ([SeqRecurso]);
GO
ALTER TABLE [dbo].[IV_RecUso] ADD CONSTRAINT [FK__IV_RecUso__SeqRe__1BDEC724] FOREIGN KEY ([SeqRecurso]) REFERENCES [dbo].[IV_Recurso] ([SeqRecurso]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResClasse
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResClasse] (
    [Resultado] decimal(6,0) NOT NULL,
    [SeqClasseRes] decimal(4,0) NOT NULL,
    CONSTRAINT [PK__IV_ResClasse__4026493B] PRIMARY KEY CLUSTERED ([Resultado], [SeqClasseRes])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResClassePK] ON [dbo].[IV_ResClasse] ([Resultado], [SeqClasseRes]);
GO
CREATE NONCLUSTERED INDEX [IV_ResClasseIF1] ON [dbo].[IV_ResClasse] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [IV_ResClasseIF2] ON [dbo].[IV_ResClasse] ([SeqClasseRes]);
GO
ALTER TABLE [dbo].[IV_ResClasse] ADD CONSTRAINT [FK__IV_ResCla__Resul__1CD2EB5D] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResClasse] ADD CONSTRAINT [FK__IV_ResCla__SeqCl__1DC70F96] FOREIGN KEY ([SeqClasseRes]) REFERENCES [dbo].[IV_ClasseRes] ([SeqClasseRes]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ResClasse] ADD CONSTRAINT [FK__IV_ResCla__Resul__5695789E] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResEvtOut
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResEvtOut] (
    [Resultado] decimal(6,0) NOT NULL,
    [SeqEvt] decimal(4,0) NOT NULL,
    [Destino] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Evento] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ResEvtOut__411A6D74] PRIMARY KEY CLUSTERED ([Resultado], [SeqEvt])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResEvtOutPK] ON [dbo].[IV_ResEvtOut] ([Resultado], [SeqEvt]);
GO
CREATE NONCLUSTERED INDEX [IV_ResEvtOutIF1] ON [dbo].[IV_ResEvtOut] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResEvtOut] ADD CONSTRAINT [FK__IV_ResEvt__Resul__1EBB33CF] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_RESJOB
   Criada em ..: 2017-04-18
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_RESJOB] (
    [RESULTADO] numeric(6,0) NOT NULL,
    [SEQJOB] numeric(18,0) NOT NULL,
    CONSTRAINT [PK__IV_RESJO__DC200B82A662C84C] PRIMARY KEY CLUSTERED ([RESULTADO], [SEQJOB])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_RESJOB] ON [dbo].[IV_RESJOB] ([RESULTADO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_RESJOB_0] ON [dbo].[IV_RESJOB] ([SEQJOB]);
GO
ALTER TABLE [dbo].[IV_RESJOB] ADD CONSTRAINT [FK__IV_RESJOB__SEQJO__5D38548C] FOREIGN KEY ([SEQJOB]) REFERENCES [dbo].[GEP_JOBCAD] ([SeqJOB]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResMsgPapel
   Criada em ..: 2011-12-19
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResMsgPapel] (
    [SeqResMsgPapel] decimal(6,0) NOT NULL,
    [Resultado] decimal(6,0) NOT NULL,
    [AtitudeUsr] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ordem] decimal(4,0) NULL,
    [TipoDest] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Papel] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Canal] varchar(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [SeqTxtPadrao] numeric(18,0) NULL,
    [Template] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Remetente] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PesFormulario] numeric(18,0) NULL,
    [PesResultado] numeric(6,0) NULL,
    [QTDEDIAS] numeric(4,0) NULL,
    [QTDEHU] numeric(8,2) NULL,
    [SEQCONTAENVIO] numeric(18,0) NULL,
    [TIPOPREFENVIO] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDENVIAQLQERHORA] numeric(1,0) NULL,
    [EMUSO] numeric(1,0) NULL,
    [PRIORIDADE] numeric(1,0) NULL,
    [DESCRICAOMENSAGEM] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBJETIVOMSG] varchar(25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ResMsgPapel__420E91AD] PRIMARY KEY CLUSTERED ([SeqResMsgPapel])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResMsgPapelPK] ON [dbo].[IV_ResMsgPapel] ([SeqResMsgPapel]);
GO
CREATE NONCLUSTERED INDEX [IV_ResMsgPapelIF1] ON [dbo].[IV_ResMsgPapel] ([SeqTxtPadrao]);
GO
CREATE NONCLUSTERED INDEX [IV_ResMsgPapelIF2] ON [dbo].[IV_ResMsgPapel] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [XIF3IV_ResMsgPapel] ON [dbo].[IV_ResMsgPapel] ([PesFormulario]);
GO
CREATE NONCLUSTERED INDEX [XIF4IV_ResMsgPapel] ON [dbo].[IV_ResMsgPapel] ([PesResultado]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_RESMSGPAPEL_01] ON [dbo].[IV_ResMsgPapel] ([SEQCONTAENVIO]);
GO
ALTER TABLE [dbo].[IV_ResMsgPapel] ADD CONSTRAINT [FK__IV_ResMsg__SeqTx__5971E549] FOREIGN KEY ([SeqTxtPadrao]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_ResMsgPapel] ADD CONSTRAINT [FK__IV_ResMsg__Resul__5A660982] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResMsgPapel] ADD CONSTRAINT [FK__IV_ResMsg__SeqTx__1FAF5808] FOREIGN KEY ([SeqTxtPadrao]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_ResMsgPapel] ADD CONSTRAINT [FK__IV_ResMsg__SEQCO__42053B63] FOREIGN KEY ([SEQCONTAENVIO]) REFERENCES [dbo].[GE_ParamLista] ([SeqParamLista]);
GO
ALTER TABLE [dbo].[IV_ResMsgPapel] ADD CONSTRAINT [FK__IV_ResMsg__PesFo__5E4BA8FF] FOREIGN KEY ([PesFormulario]) REFERENCES [dbo].[IV_Formulario] ([SeqFormulario]);
GO
ALTER TABLE [dbo].[IV_ResMsgPapel] ADD CONSTRAINT [FK__IV_ResMsg__Resul__20A37C41] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResParam
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResParam] (
    [Resultado] decimal(6,0) NOT NULL,
    [Advertencia] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmpltTxtPadrao] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssuntoCmpl] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssuntoEmail] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PermEMail] numeric(1,0) NULL,
    [PermReagendar] numeric(1,0) NULL,
    [ReagendarPara] decimal(6,0) NULL,
    [HoraReagenda] decimal(6,2) NULL,
    [HoraProrrAgda] decimal(6,2) NULL,
    [HoraProrrLimite] decimal(6,2) NULL,
    [PermProrrogar] numeric(1,0) NULL,
    [PermIncManual] numeric(1,0) NULL,
    [ExigPessoaAtiva] numeric(1,0) NULL,
    [ExigDuracao] numeric(1,0) NULL,
    [ExigClienteAtivo] numeric(1,0) NULL,
    [ExigDetalhe] numeric(1,0) NOT NULL,
    [DescValor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigValor] numeric(1,0) NULL,
    [DescQtde] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigQtde] numeric(1,0) NULL,
    [CtrlProcProj] numeric(1,0) NULL,
    [CtrlProcProd] numeric(1,0) NULL,
    [CtrlProcResp] numeric(1,0) NULL,
    [CtrlProcVend] numeric(1,0) NULL,
    [CtrlProcVlr] numeric(1,0) NULL,
    [CtrlProcQtd] numeric(1,0) NULL,
    [CtrlProcDtPrvConc] numeric(1,0) NULL,
    [CtrlProcDepto] numeric(1,0) NULL,
    [CtrlProcCampanha] numeric(1,0) NULL,
    [CtrlProcStatus] numeric(1,0) NULL,
    [CtrlProcDetalhe] numeric(1,0) NULL,
    [CtrlProcPersp] numeric(1,0) NULL,
    [CtrlProcCanal] numeric(1,0) NULL,
    [CtrlRespProcesso] numeric(1,0) NULL,
    [TpoMaxReagenda] decimal(10,4) NULL,
    [QtdMaxUso] decimal(2,0) NULL,
    CONSTRAINT [PK__IV_ResParam__4302B5E6] PRIMARY KEY CLUSTERED ([Resultado])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResParamPK] ON [dbo].[IV_ResParam] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResParam] ADD CONSTRAINT [FK__IV_ResPar__Resul__2197A07A] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Resultado
   Criada em ..: 2011-12-19
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Resultado] (
    [Resultado] decimal(6,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Acao] decimal(6,0) NULL,
    [DescReduzida] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CMPLTTXTPADRAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ADVERTENCIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REAGENDARPARA] numeric(6,0) NULL,
    [Seqformulario] numeric(18,0) NULL,
    [Ordem] decimal(3,0) NULL,
    [DESCVALOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCQTDE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [QtdeMaxPorAcao] decimal(2,0) NULL,
    [ASSUNTOCMPL] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ASSUNTOEMAIL] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PubWeb] numeric(1,0) NULL,
    [HORAPRORRLIMITE] numeric(6,2) NULL,
    [HORAPRORRAGDA] numeric(6,2) NULL,
    [HORAREAGENDA] numeric(6,2) NULL,
    [CanalPadrao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SOLICSTATDESC] numeric(1,0) NULL,
    [VidaUtil] numeric(4,0) NULL,
    [MINUTILREAGENDA] numeric(8,2) NULL,
    [CTRLINTERATIVO] numeric(1,0) NULL,
    [CTRLCONCLUSAO] numeric(1,0) NULL,
    [CTRLDETALHE] numeric(1,0) NULL,
    [CTRLFORMACONTATO] numeric(1,0) NULL,
    [CTRLREAGENDA] numeric(1,0) NULL,
    [CTRLCONFIRMAREAG] numeric(1,0) NULL,
    [CTRLREAGENDASILO] numeric(1,0) NULL,
    [CTRLMUDARATDREAG] numeric(1,0) NULL,
    [CTRLFORMULARIO] numeric(1,0) NULL,
    [CTRLDURACAO] numeric(1,0) NULL,
    [CTRLQTDE] numeric(1,0) NULL,
    [CTRLVALOR] numeric(1,0) NULL,
    [CTRLPRODUTIVO] numeric(1,0) NULL,
    [CTRLPRODUTO] numeric(1,0) NULL,
    [CTRLVENDEDOR] numeric(1,0) NULL,
    [CTRLMOTIVO] numeric(1,0) NULL,
    [CTRLDEPARTAMENTO] numeric(1,0) NULL,
    [CTRLCAMPANHA] numeric(1,0) NULL,
    [CTRLCONTATOPF] numeric(1,0) NULL,
    [CTRLCONTATOPJ] numeric(1,0) NULL,
    [CTRLCLIENTEATIVO] numeric(1,0) NULL,
    [CTRLDETALHEPROC] numeric(1,0) NULL,
    [CTRLDESCRSTATUSPROC] numeric(1,0) NULL,
    [CTRLSTATUSPROC] numeric(1,0) NULL,
    [CTRLRESUMOPROC] numeric(1,0) NULL,
    [CTRLPERSPECTIVAPROC] numeric(1,0) NULL,
    [CTRLVALORPROC] numeric(1,0) NULL,
    [CTRLDTAENCERRAPROC] numeric(1,0) NULL,
    [CTRLPROJETO] numeric(1,0) NULL,
    [CTRLIMAGEM] numeric(1,0) NULL,
    [CTRLCTIDISPONIVEL] numeric(1,0) NULL,
    [CTRLCOMPLEMENTO] numeric(1,0) NULL,
    [CTRLCMPLSQL] numeric(1,0) NULL,
    [CTRLAGENDACONF] numeric(1,0) NULL,
    [CTRLRESPPROC] numeric(1,0) NULL,
    [EMUSO] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_Resultado__43F6DA1F] PRIMARY KEY CLUSTERED ([Resultado])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResultadoPK] ON [dbo].[IV_Resultado] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [IV_ResultadoIE1] ON [dbo].[IV_Resultado] ([DescReduzida]);
GO
CREATE NONCLUSTERED INDEX [IV_ResultadoIF2] ON [dbo].[IV_Resultado] ([Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_RESULTADOIE2] ON [dbo].[IV_Resultado] ([EMUSO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_RESULTADO_3110
   Criada em ..: 2023-06-05
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_RESULTADO_3110] (
    [Resultado] decimal(6,0) NOT NULL,
    [Pcte] varchar(4) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Acao] decimal(6,0) NULL,
    [DescReduzida] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CMPLTTXTPADRAO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGEDETALHE] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Conclusivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ProximoPasso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NEGOCIO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INCLUSAOPROSPECT] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsaObjDyn] numeric(1,0) NULL,
    [Ativo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Receptivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Grupo] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ForcaConclusao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IdentSituacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ADVERTENCIA] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [REAGENDARPARA] numeric(6,0) NULL,
    [EXIGEPRODUTO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGEVENDEDOR] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGEMOTIVO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGEDEPTO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGECAMPANHA] numeric(1,0) NULL,
    [ExigAgendaConf] numeric(1,0) NULL,
    [PERMITEREAGENDAR] numeric(1,0) NULL,
    [PERMITEPRORROGAR] numeric(1,0) NULL,
    [TemComplemento] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CmplSql] numeric(1,0) NULL,
    [EmUso] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PERMITEEMAIL] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGECLIENTEATIVO] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Seqformulario] numeric(18,0) NULL,
    [ExigeFormulario] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Campanha] varchar(12) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ordem] decimal(3,0) NULL,
    [DESCVALOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGEVALOR] numeric(1,0) NULL,
    [DESCQTDE] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGEQTDE] numeric(1,0) NULL,
    [SOLICITASTATUSPROC] numeric(1,0) NULL,
    [SOLICITAFORMACTTO] numeric(1,0) NULL,
    [SOLICITADESCSTAT] numeric(1,0) NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [QtdeMaxPorAcao] decimal(2,0) NULL,
    [SOLICITAPERSPECT] numeric(1,0) NULL,
    [ASSUNTOCMPL] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EXIGEDURACAO] numeric(1,0) NULL,
    [PERMITEINCMANUAL] numeric(1,0) NULL,
    [ASSUNTOEMAIL] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PubWeb] numeric(1,0) NULL,
    [SOLICITARESPPROC] numeric(1,0) NULL,
    [SOLICITADURACAO] numeric(1,0) NULL,
    [SOLICITADETPROC] numeric(1,0) NULL,
    [HORAPRORRLIMITE] numeric(6,2) NULL,
    [HORAPRORRAGDA] numeric(6,2) NULL,
    [HORAREAGENDA] numeric(6,2) NULL,
    [SOLICITAVLRPROC] numeric(1,0) NULL,
    [SOLPROJETO] numeric(1,0) NULL,
    [CanalPadrao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SOLICSTATDESC] numeric(1,0) NULL,
    [ExigContatoPJ] numeric(1,0) NULL,
    [ExigContatoPF] numeric(1,0) NULL,
    [VidaUtil] numeric(4,0) NULL,
    [SOLICDTAENCPROC] numeric(1,0) NULL,
    [SOLICITACONFREAGEN] numeric(1,0) NULL,
    [PERMITEMUDARATDREAG] numeric(1,0) NULL,
    [INDREAGSILORIG] numeric(1,0) NULL,
    [INDCTIDISPONIVEL] numeric(1,0) NULL,
    [MINUTILREAGENDA] numeric(8,2) NULL,
    [INDAPPEXIGEFOTO] numeric(1,0) NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResultadoCmpl
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResultadoCmpl] (
    [Resultado] decimal(6,0) NOT NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    CONSTRAINT [PK__IV_ResultadoCmpl__44EAFE58] PRIMARY KEY CLUSTERED ([Resultado], [ResultadoCmpl])
);
GO
CREATE NONCLUSTERED INDEX [IV_ResultadoCmpIF1] ON [dbo].[IV_ResultadoCmpl] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResultadoCmpl] ADD CONSTRAINT [FK__IV_Result__Resul__237FE8EC] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResultadoInstr
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResultadoInstr] (
    [Resultado] decimal(6,0) NOT NULL,
    [Instrucao] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_ResultadoInst__45DF2291] PRIMARY KEY CLUSTERED ([Resultado])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResultadoInsPK] ON [dbo].[IV_ResultadoInstr] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResultadoInstr] ADD CONSTRAINT [FK__IV_Result__Resul__24740D25] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResultadoReq
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResultadoReq] (
    [Resultado] decimal(6,0) NOT NULL,
    [CodProcesso] decimal(4,0) NOT NULL,
    [Acao] decimal(6,0) NOT NULL,
    [ResDep] decimal(6,0) NOT NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_ResultadoReq__46D346CA] PRIMARY KEY CLUSTERED ([Resultado], [CodProcesso], [Acao], [ResDep])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResultadoReqPK] ON [dbo].[IV_ResultadoReq] ([Resultado], [CodProcesso], [Acao], [ResDep]);
GO
CREATE NONCLUSTERED INDEX [IV_ResultadoReqIF1] ON [dbo].[IV_ResultadoReq] ([ResDep]);
GO
CREATE NONCLUSTERED INDEX [IV_ResultadoReqIF2] ON [dbo].[IV_ResultadoReq] ([CodProcesso], [Acao]);
GO
CREATE NONCLUSTERED INDEX [IV_ResultadoReqIF3] ON [dbo].[IV_ResultadoReq] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResultadoReq] ADD CONSTRAINT [FK__IV_Result__ResDe__2568315E] FOREIGN KEY ([ResDep]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_ResultadoReq] ADD CONSTRAINT [FK__IV_ResultadoReq__265C5597] FOREIGN KEY ([CodProcesso], [Acao]) REFERENCES [dbo].[IV_ProcAcao] ([CodProcesso], [Acao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResultadoWeb
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResultadoWeb] (
    [EventoWeb] decimal(6,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [Resultado] decimal(6,0) NOT NULL,
    [DescricaoWeb] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GrupoWebCRM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [WebVars] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExigeAnexo] numeric(1,0) NULL,
    [InstrucaoWeb] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    CONSTRAINT [PK__IV_ResultadoWeb__47C76B03] PRIMARY KEY CLUSTERED ([EventoWeb])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResultadoWebPK] ON [dbo].[IV_ResultadoWeb] ([EventoWeb]);
GO
CREATE NONCLUSTERED INDEX [IV_ResultadoWebIF1] ON [dbo].[IV_ResultadoWeb] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResultadoWeb] ADD CONSTRAINT [FK__IV_Result__Resul__28449E09] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_ResVinc
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_ResVinc] (
    [Resultado] decimal(6,0) NOT NULL,
    [SeqVinc] decimal(4,0) NOT NULL,
    [Vinculo] varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CodProcesso] decimal(4,0) NOT NULL,
    [NroVinc] numeric(18,0) NULL,
    [CodVinc] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [GrupoVinc] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Exigido] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_ResVinc__48BB8F3C] PRIMARY KEY CLUSTERED ([Resultado], [SeqVinc])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_ResVincPK] ON [dbo].[IV_ResVinc] ([Resultado], [SeqVinc]);
GO
CREATE NONCLUSTERED INDEX [IV_ResVincIF1] ON [dbo].[IV_ResVinc] ([Resultado]);
GO
ALTER TABLE [dbo].[IV_ResVinc] ADD CONSTRAINT [FK__IV_ResVin__Resul__2938C242] FOREIGN KEY ([Resultado]) REFERENCES [dbo].[IV_Resultado] ([Resultado]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_RetProcRegra
   Criada em ..: 2014-09-15
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_RetProcRegra] (
    [SeqRetProcRegra] numeric(18,0) NOT NULL,
    [SeqTxtPadrao] numeric(18,0) NULL,
    [OrdemProc] numeric(2,0) NULL,
    [TAGS] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Resultado] numeric(6,0) NULL,
    [ResultadoCmpl] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Detalhe] varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_RetProcRegra__37E608F5] PRIMARY KEY CLUSTERED ([SeqRetProcRegra])
);
GO
CREATE NONCLUSTERED INDEX [IV_RETPROCREGRAIF2] ON [dbo].[IV_RetProcRegra] ([Resultado]);
GO
CREATE NONCLUSTERED INDEX [IV_RETPROCREGRAIF1] ON [dbo].[IV_RetProcRegra] ([SeqTxtPadrao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_SegPerfil
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_SegPerfil] (
    [SeqPerfil] decimal(4,0) NOT NULL,
    [EVendedor] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IncHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IncHistRetr] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [QtdDiasRetr] decimal(4,0) NULL,
    [IncHistAgConcl] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExcHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [IncAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ExcAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VerAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ConcAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltOperAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AltOperAgdReag] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ConcAgeVendor] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ReativarAgenda] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AnalisarHistorico] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AbreAnalise] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Alteragrupo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraCodImport] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraRegiao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraVendedor] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraClienteAtivo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AlteraStatus] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_SegPerfil__49AFB375] PRIMARY KEY CLUSTERED ([SeqPerfil])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_SegPerfilPK] ON [dbo].[IV_SegPerfil] ([SeqPerfil]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Selecao
   Criada em ..: 2011-12-19
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Selecao] (
    [SeqSelecao] numeric(18,0) NOT NULL,
    [Selecao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Descricao] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    [Dinamica] numeric(1,0) NULL,
    [Tipo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [QtdeClientes] decimal(7,0) NULL,
    [DtaAlteracao] datetime NULL,
    [Construtor] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Usuario] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [UsuGeracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_Selecao__4AA3D7AE] PRIMARY KEY CLUSTERED ([SeqSelecao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_SelecaoPK] ON [dbo].[IV_Selecao] ([SeqSelecao]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_SELECAO_0] ON [dbo].[IV_Selecao] ([EmUso]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_SELECAO_1] ON [dbo].[IV_Selecao] ([EmUso], [DtaAlteracao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_SelecaoColList
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_SelecaoColList] (
    [TbNome] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ColNome] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [InstrSql] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_SelecaoColLis__4B97FBE7] PRIMARY KEY CLUSTERED ([TbNome], [ColNome])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_SelecaoColLiPK] ON [dbo].[IV_SelecaoColList] ([TbNome], [ColNome]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_SelecaoCriterio
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_SelecaoCriterio] (
    [SeqSelecao] numeric(18,0) NOT NULL,
    [SeqCriterio] decimal(4,0) NOT NULL,
    [Operacao] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Ordem] decimal(4,0) NULL,
    [SeqSelecaoFoco] numeric(18,0) NULL,
    [DescCriterio] varchar(60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [InstrSQL] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_SelecaoCriter__4C8C2020] PRIMARY KEY CLUSTERED ([SeqSelecao], [SeqCriterio])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_SelecaoCritePK] ON [dbo].[IV_SelecaoCriterio] ([SeqSelecao], [SeqCriterio]);
GO
CREATE NONCLUSTERED INDEX [IV_SelecaoCriteIF1] ON [dbo].[IV_SelecaoCriterio] ([SeqSelecao]);
GO
ALTER TABLE [dbo].[IV_SelecaoCriterio] ADD CONSTRAINT [FK__IV_Seleca__SeqSe__2A2CE67B] FOREIGN KEY ([SeqSelecao]) REFERENCES [dbo].[IV_Selecao] ([SeqSelecao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_SelecaoPessoa
   Criada em ..: 2011-12-19
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_SelecaoPessoa] (
    [SeqSelecao] numeric(18,0) NOT NULL,
    [SeqPessoa] numeric(10,0) NOT NULL,
    [Usado] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_SelecaoPessoa__4D804459] PRIMARY KEY CLUSTERED ([SeqSelecao], [SeqPessoa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_SelecaoPessoPK] ON [dbo].[IV_SelecaoPessoa] ([SeqSelecao], [SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_SelecaoPessoIF1] ON [dbo].[IV_SelecaoPessoa] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_SelecaoPessoIF2] ON [dbo].[IV_SelecaoPessoa] ([SeqSelecao]);
GO
ALTER TABLE [dbo].[IV_SelecaoPessoa] ADD CONSTRAINT [FK__IV_Seleca__SeqPe__2B210AB4] FOREIGN KEY ([SeqPessoa]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[IV_SelecaoPessoa] ADD CONSTRAINT [FK__IV_Seleca__SeqSe__2C152EED] FOREIGN KEY ([SeqSelecao]) REFERENCES [dbo].[IV_Selecao] ([SeqSelecao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_SELPROMOPRD
   Criada em ..: 2017-08-01
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_SELPROMOPRD] (
    [SEQSELPROMO] numeric(18,0) NOT NULL,
    [IDPRODUTO] numeric(10,0) NOT NULL,
    [PERDESCONTO] numeric(4,2) NULL,
    [VLRUNITARIO] numeric(14,2) NULL,
    [QTDLIMITE] numeric(10,2) NULL,
    CONSTRAINT [PK__IV_SELPR__0E6FBA8C2032FD77] PRIMARY KEY CLUSTERED ([SEQSELPROMO], [IDPRODUTO])
);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_SELPROMOPRD] ON [dbo].[IV_SELPROMOPRD] ([SEQSELPROMO]);
GO
CREATE NONCLUSTERED INDEX [XIF2IV_SELPROMOPRD] ON [dbo].[IV_SELPROMOPRD] ([IDPRODUTO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_SMS
   Criada em ..: 2014-09-15
   Alterada em : 2019-12-19
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_SMS] (
    [SeqSMS] numeric(18,0) NOT NULL,
    [Origem] char(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Chave] numeric(18,0) NULL,
    [Destino] numeric(16,0) NULL,
    [SeqPessoa] numeric(10,0) NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [SeqTxtPadrao] numeric(18,0) NULL,
    [Mensagem] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaGeracao] datetime NULL,
    [DtaEnvio] datetime NULL,
    [NroEmpresa] numeric(6,0) NULL,
    [IndRetProcessado] numeric(1,0) NULL,
    [ContaSeqPar] numeric(18,0) NULL,
    [Processo] numeric(18,0) NULL,
    [Resultado] numeric(6,0) NULL,
    [SeqContaSMS] numeric(18,0) NULL,
    [CONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUBCONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [INDENVIAQLQERHORA] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_SMS__38DA2D2E] PRIMARY KEY CLUSTERED ([SeqSMS])
);
GO
CREATE NONCLUSTERED INDEX [IV_SMSIE1] ON [dbo].[IV_SMS] ([Destino], [IndRetProcessado]);
GO
CREATE NONCLUSTERED INDEX [IV_SMSIE2] ON [dbo].[IV_SMS] ([DtaEnvio]);
GO
CREATE NONCLUSTERED INDEX [IV_SMSIF2] ON [dbo].[IV_SMS] ([ContaSeqPar]);
GO
CREATE NONCLUSTERED INDEX [IV_SMSIF3] ON [dbo].[IV_SMS] ([SeqPessoa]);
GO
CREATE NONCLUSTERED INDEX [IV_SMSIF4] ON [dbo].[IV_SMS] ([SeqUsuario]);
GO
CREATE NONCLUSTERED INDEX [IV_SMSIF1] ON [dbo].[IV_SMS] ([SeqTxtPadrao]);
GO
CREATE NONCLUSTERED INDEX [XIE3IV_SMS] ON [dbo].[IV_SMS] ([Chave], [Origem]);
GO
CREATE NONCLUSTERED INDEX [XIF5IV_SMS] ON [dbo].[IV_SMS] ([SeqContaSMS]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_SMSLog
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_SMSLog] (
    [SeqSMS] numeric(18,0) NOT NULL,
    [SeqLog] numeric(4,0) NOT NULL,
    [DtaLog] datetime NULL,
    [Status] char(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_SMSLog__39CE5167] PRIMARY KEY CLUSTERED ([SeqSMS], [SeqLog])
);
GO
CREATE NONCLUSTERED INDEX [IV_SMSLOGIF1] ON [dbo].[IV_SMSLog] ([SeqSMS]);
GO
CREATE NONCLUSTERED INDEX [XIE1IV_SMSLog] ON [dbo].[IV_SMSLog] ([DtaLog]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_STATUS_DEPTO
   Criada em ..: 2017-01-03
   Alterada em : 2023-03-23
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_STATUS_DEPTO] (
    [SEQPESSOA] numeric(18,0) NULL,
    [DEPTO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ULTCOMPRA] datetime NULL,
    [ULTINTEGRACAO] datetime NULL
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_STATUS_DEPTO1] ON [dbo].[IV_STATUS_DEPTO] ([SEQPESSOA], [DEPTO], [STATUS]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TAGCAD
   Criada em ..: 2023-06-05
   Alterada em : 2023-06-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TAGCAD] (
    [TAG] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [COR] varchar(6) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAGERACAO] datetime NULL,
    [USUGERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CORNUM] numeric(18,0) NULL,
    [QTDUSOHST] numeric(18,0) NULL,
    [DTAULTUSOHST] datetime NULL,
    [QTDUSOPRC] numeric(18,0) NULL,
    [DTAULTUSOPRC] datetime NULL
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_TAGCAD] ON [dbo].[IV_TAGCAD] ([TAG]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TC_PESSOA
   Criada em ..: 2017-11-09
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TC_PESSOA] (
    [SEQQUESTIONARIO] numeric(18,0) NOT NULL,
    [QUANTIDADE_DE_EQUIPA] decimal(14,0) NULL,
    [TIPO_DE_EQUIPAMENTO] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MARCA] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MODELO_DO_EQUIPAMENT] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ANO_MODELO_EQUIPAMEN] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [N__SERIE] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HORIMETRO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DATA_INICIO_LOCACAO] datetime NULL,
    [DATA_TERMINO_LOCACAO] datetime NULL,
    [VALOR_MENSAL_DO_CONT] decimal(14,2) NULL,
    [VALOR_TOTAL_DO_CONTR] decimal(14,2) NULL,
    [DATA_RENOVACAO] datetime NULL,
    CONSTRAINT [PK__IV_TC_PE__7484C68C261FD11D] PRIMARY KEY NONCLUSTERED ([SEQQUESTIONARIO])
);
GO
ALTER TABLE [dbo].[IV_TC_PESSOA] ADD CONSTRAINT [FK__IV_TC_PES__SEQQU__1ACB72B1] FOREIGN KEY ([SEQQUESTIONARIO]) REFERENCES [dbo].[IV_Questionario] ([SeqQuestionario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TEMPLATEPROJ
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TEMPLATEPROJ] (
    [ARQUIVOTEMPLATE] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [MENSSAGEM] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SEQPROJETO] numeric(18,0) NOT NULL,
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTEROU] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_TEMPL__B5066C547D067CEC] PRIMARY KEY CLUSTERED ([ARQUIVOTEMPLATE])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TIPOCONTEUDO
   Criada em ..: 2017-08-01
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TIPOCONTEUDO] (
    [SEQCONTEUDO] numeric(6,0) NOT NULL,
    [TIPOCAMPANHA] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRICAO] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_TIPOC__089F916C3C16609A] PRIMARY KEY CLUSTERED ([SEQCONTEUDO])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TpPgto
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TpPgto] (
    [SeqTpPgto] decimal(2,0) NOT NULL,
    [TipoPgto] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [VistaPrazo] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [TpCadastro] varchar(2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [LinkExterno] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_TpPgto__4E746892] PRIMARY KEY CLUSTERED ([SeqTpPgto])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_TpPgtoPK] ON [dbo].[IV_TpPgto] ([SeqTpPgto]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_TpPgtoAK1] ON [dbo].[IV_TpPgto] ([TipoPgto]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TxtPadConta
   Criada em ..: 2014-09-15
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TxtPadConta] (
    [NroEmpresa] numeric(6,0) NOT NULL,
    [SeqTxtPadrao] numeric(18,0) NOT NULL,
    [SeqParamLista] numeric(18,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_TxtPadConta__3AC275A0] PRIMARY KEY CLUSTERED ([NroEmpresa], [SeqTxtPadrao], [SeqParamLista])
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TxtPadrao
   Criada em ..: 2011-12-19
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TxtPadrao] (
    [SeqTxtPadrao] numeric(18,0) NOT NULL,
    [Aplicacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [AssPadrao] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Texto] text COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [HoraAGUARDARET] numeric(6,1) NULL,
    [IDEXTERNO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_TxtPadrao__4F688CCB] PRIMARY KEY CLUSTERED ([SeqTxtPadrao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_TxtPadraoPK] ON [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_TxtPadraoUso
   Criada em ..: 2011-12-19
   Alterada em : 2018-08-14
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_TxtPadraoUso] (
    [CodAplicacao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ChaveAplicacao] numeric(18,0) NOT NULL,
    [SeqTxtPadrao] numeric(18,0) NOT NULL,
    [Direto] numeric(1,0) NULL,
    CONSTRAINT [PK__IV_TxtPadraoUso__505CB104] PRIMARY KEY CLUSTERED ([CodAplicacao], [ChaveAplicacao], [SeqTxtPadrao])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_TxtPadraoUsoPK] ON [dbo].[IV_TxtPadraoUso] ([CodAplicacao], [ChaveAplicacao], [SeqTxtPadrao]);
GO
CREATE NONCLUSTERED INDEX [IV_TxtPadraoUsoIF1] ON [dbo].[IV_TxtPadraoUso] ([SeqTxtPadrao]);
GO
ALTER TABLE [dbo].[IV_TxtPadraoUso] ADD CONSTRAINT [FK__IV_TxtPad__SeqTx__2D095326] FOREIGN KEY ([SeqTxtPadrao]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_Unidade
   Criada em ..: 2011-12-19
   Alterada em : 2014-09-15
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_Unidade] (
    [SeqUnidade] numeric(4,0) NOT NULL,
    [Gerente] varchar(18) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresa] decimal(3,0) NULL,
    [Unidade] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [Descricao] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EmUso] numeric(1,0) NULL,
    [UsuInclusao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaInclusao] datetime NULL,
    [UsuAlteracao] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DtaAlteracao] datetime NULL
);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_URADISPARO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_URADISPARO] (
    [SEQURADISPARO] numeric(18,0) NOT NULL,
    [SEQAGENDA] numeric(18,0) NULL,
    [SEQURACENARIO] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [DESTINO] numeric(12,0) NULL,
    [DTAGERACAO] datetime NULL,
    [BULKID] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MESSAGEID] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAENVIO] datetime NULL,
    [STATUS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRIORIDADE] numeric(2,0) NULL,
    [DTAENVIAR] datetime NULL,
    [STATUSDESC] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_URADI__C93BF9A6483FD5FE] PRIMARY KEY CLUSTERED ([SEQURADISPARO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_URADISPARO_0] ON [dbo].[IV_URADISPARO] ([SEQAGENDA]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_URADISPARO_1] ON [dbo].[IV_URADISPARO] ([SEQURACENARIO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_URADISPARO_2] ON [dbo].[IV_URADISPARO] ([STATUS], [PRIORIDADE], [DTAENVIAR]);
GO
ALTER TABLE [dbo].[IV_URADISPARO] ADD CONSTRAINT [FK__IV_URADIS__SEQAG__1DC7DAED] FOREIGN KEY ([SEQAGENDA]) REFERENCES [dbo].[IV_Agenda] ([SeqAgenda]);
GO
ALTER TABLE [dbo].[IV_URADISPARO] ADD CONSTRAINT [FK__IV_URADIS__SEQUR__1EBBFF26] FOREIGN KEY ([SEQURACENARIO]) REFERENCES [dbo].[GE_URACENARIO] ([SEQURACENARIO]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_URARELATORIO
   Criada em ..: 2019-02-05
   Alterada em : 2019-02-05
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_URARELATORIO] (
    [SEQURARELATORIO] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SEQURADISPARO] numeric(18,0) NOT NULL,
    [SENDAT] datetime NULL,
    [DONEAT] datetime NULL,
    [DURACAO] numeric(6,0) NULL,
    [DTMFCODES] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [URLAUDIOFILE] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PRICEPERMESSAGE] numeric(14,2) NULL,
    [STATUS_GROUPNAME] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_NAME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUS_DESCRIPTION] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ERROR_GROUPNAME] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ERROR_NAME] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ERROR_DESCRIPTION] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESTINO] numeric(14,0) NULL,
    [REMETENTE] numeric(14,0) NULL,
    CONSTRAINT [PK__IV_URARE__22E93AB6257B2C26] PRIMARY KEY CLUSTERED ([SEQURARELATORIO])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_URARELATORIO_0] ON [dbo].[IV_URARELATORIO] ([SEQURADISPARO]);
GO
ALTER TABLE [dbo].[IV_URARELATORIO] ADD CONSTRAINT [FK__IV_URAREL__SEQUR__1FB0235F] FOREIGN KEY ([SEQURADISPARO]) REFERENCES [dbo].[IV_URADISPARO] ([SEQURADISPARO]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_USRPUSH
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_USRPUSH] (
    [SEQPUSH] numeric(18,0) IDENTITY(1,1) NOT NULL,
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [APLICATIVO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [ASSUNTO] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [MENSAGEM] varchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [PROCESSO] numeric(18,0) NULL,
    [SEQHISTORICO] numeric(18,0) NULL,
    [DTAGERACAO] datetime NULL,
    [DTAENVIO] datetime NULL,
    [CONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SUBCONTEXTO] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAENVIAR] datetime NULL,
    [STATUS] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [STATUSMSG] varchar(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [ORIGEM] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_USRPU__6928302B3ABBC564] PRIMARY KEY CLUSTERED ([SEQPUSH])
);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PUSH_2] ON [dbo].[IV_USRPUSH] ([SEQHISTORICO]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PUSH_1] ON [dbo].[IV_USRPUSH] ([STATUS]);
GO
CREATE NONCLUSTERED INDEX [IDX_IV_PUSH_4] ON [dbo].[IV_USRPUSH] ([PROCESSO]);
GO
ALTER TABLE [dbo].[IV_USRPUSH] ADD CONSTRAINT [FK__IV_USRPUS__SEQUS__794B4EAC] FOREIGN KEY ([SEQUSUARIO]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_USRSTATUS
   Criada em ..: 2025-02-17
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_USRSTATUS] (
    [SEQUSUARIO] numeric(18,0) NOT NULL,
    [DTAATUALIZACAO] datetime NULL,
    [SEQPESSOAATIVA] numeric(10,0) NULL,
    [LINKERPPESSOAATIVA] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROEMPRESACNX] numeric(6,0) NULL,
    [RAZAOSOCIALATIVA] varchar(80) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [OBS] varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_USRST__59D861DE2E2E2BD4] PRIMARY KEY CLUSTERED ([SEQUSUARIO])
);
GO
ALTER TABLE [dbo].[IV_USRSTATUS] ADD CONSTRAINT [FK__IV_USRSTA__SEQUS__6FC1E472] FOREIGN KEY ([SEQUSUARIO]) REFERENCES [dbo].[GE_Usuario] ([SeqUsuario]) ON DELETE CASCADE;
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_VENDEDOR
   Criada em ..: 2011-12-19
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_VENDEDOR] (
    [SEQVENDEDOR] numeric(18,0) NOT NULL,
    [SeqUnidade] numeric(4,0) NULL,
    [CODVENDEDOR] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [VENDEDOR] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CODVENDEDORFORA] numeric(18,0) NULL,
    [CODVENDEDORFORAX] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [SeqUsuario] numeric(18,0) NULL,
    [SeqUsuarioLider] numeric(18,0) NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NroEmpresaBKP] numeric(6,0) NULL,
    [EMUSO] numeric(1,0) NULL,
    [NROEMPRPADRAO] numeric(6,0) NULL,
    [SALARIOFIXO] numeric(14,2) NULL,
    [COMISSAO1] numeric(8,4) NULL,
    [COMISSAO2] numeric(8,4) NULL,
    [OBS] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [EQUIPE] varchar(40) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    CONSTRAINT [PK__IV_Equipe__0D9AC96E] PRIMARY KEY CLUSTERED ([SEQVENDEDOR])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_EquipePK] ON [dbo].[IV_VENDEDOR] ([SEQVENDEDOR]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IV_EquipeAK1] ON [dbo].[IV_VENDEDOR] ([CODVENDEDOR]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_VENDEDOREMPR
   Criada em ..: 2014-09-15
   Alterada em : 2025-02-17
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_VENDEDOREMPR] (
    [SEQVENDEDOR] numeric(18,0) NOT NULL,
    [NroEmpresa] numeric(6,0) NOT NULL,
    [DtaAlteracao] datetime NULL,
    [UsuAlterou] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_EquipeEmpr__35FDC083] PRIMARY KEY CLUSTERED ([SEQVENDEDOR], [NroEmpresa])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [XPKIV_EquipeEmpr] ON [dbo].[IV_VENDEDOREMPR] ([SEQVENDEDOR], [NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [IV_EQUIPEEMPRIE1] ON [dbo].[IV_VENDEDOREMPR] ([NroEmpresa]);
GO
CREATE NONCLUSTERED INDEX [XIF1IV_EquipeEmpr] ON [dbo].[IV_VENDEDOREMPR] ([SEQVENDEDOR]);
GO

/* ---------------------------------------------------------------
   Tabela .....: dbo.IV_WHATSAPP
   Criada em ..: 2025-04-22
   Alterada em : 2025-04-22
   --------------------------------------------------------------- */
CREATE TABLE [dbo].[IV_WHATSAPP] (
    [SEQWHATSAPP] numeric(18,0) NOT NULL,
    [ORIGEM] char(3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [CHAVE] numeric(18,0) NULL,
    [DESTINO] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [PROCESSO] numeric(18,0) NULL,
    [SEQPESSOA] numeric(10,0) NOT NULL,
    [SEQTXTPADRAO] numeric(18,0) NOT NULL,
    [MENSAGEM] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [NROEMPRESA] numeric(6,0) NULL,
    [SEQHISTORICO] numeric(6,0) NULL,
    [CONTAWAPSEQPAR] numeric(18,0) NULL,
    [INDENVIAQLQERHORA] numeric(1,0) NULL,
    [DTAENVIO] datetime NULL,
    [INDENVIADO] numeric(1,0) NULL,
    [INDENVIOOK] numeric(1,0) NULL,
    [CODSTATUSENVIO] varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DESCRSTATUSENVIO] varchar(300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAINCLUSAO] datetime NULL,
    [USUINCLUSAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    [DTAALTERACAO] datetime NULL,
    [USUALTERACAO] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CONSTRAINT [PK__IV_WHATS__6092F5DCA4D692F4] PRIMARY KEY CLUSTERED ([SEQWHATSAPP])
);
GO
ALTER TABLE [dbo].[IV_WHATSAPP] ADD CONSTRAINT [FK__IV_WHATSA__CONTA__73286102] FOREIGN KEY ([CONTAWAPSEQPAR]) REFERENCES [dbo].[GE_ParamLista] ([SeqParamLista]);
GO
ALTER TABLE [dbo].[IV_WHATSAPP] ADD CONSTRAINT [FK__IV_WHATSA__SEQPE__741C853B] FOREIGN KEY ([SEQPESSOA]) REFERENCES [dbo].[GE_Pessoa] ([SeqPessoa]);
GO
ALTER TABLE [dbo].[IV_WHATSAPP] ADD CONSTRAINT [FK__IV_WHATSA__SEQTX__7510A974] FOREIGN KEY ([SEQTXTPADRAO]) REFERENCES [dbo].[IV_TxtPadrao] ([SeqTxtPadrao]);
GO

