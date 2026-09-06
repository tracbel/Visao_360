# IV-3 - Catalogos e parametrizacao do BPM

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**56 tabelas · 657 colunas · 2.129.692 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IV_Acao`](#iv_acao) | catalogo | 37 | 979 |
| [`IV_ACAOANEXA`](#iv_acaoanexa) | vazia | 11 | 0 |
| [`IV_AcaoAtendente`](#iv_acaoatendente) | vazia | 6 | 0 |
| [`IV_AcaoAuto`](#iv_acaoauto) | nucleo | 31 | 5.942 |
| [`IV_AcaoAutoCtrl`](#iv_acaoautoctrl) | vazia | 6 | 0 |
| [`IV_AcaoCmpl`](#iv_acaocmpl) | vazia | 2 | 0 |
| [`IV_AcaoCtrl`](#iv_acaoctrl) | nucleo | 13 | 44 |
| [`IV_AcaoMon`](#iv_acaomon) | vazia | 9 | 0 |
| [`IV_AcaoRem`](#iv_acaorem) | nucleo | 9 | 266 |
| [`IV_ACAOURACENARIO`](#iv_acaouracenario) | vazia | 2 | 0 |
| [`IV_ClasseRes`](#iv_classeres) | nucleo | 5 | 40 |
| [`IV_CodPrcEmpr`](#iv_codprcempr) | nucleo | 2 | 17 |
| [`IV_CodProcComent`](#iv_codproccoment) | nucleo | 4 | 1 |
| [`IV_CodProcesso`](#iv_codprocesso) | catalogo | 35 | 62 |
| [`IV_Departamento`](#iv_departamento) | vazia | 3 | 0 |
| [`IV_DoctoApl`](#iv_doctoapl) | nucleo | 15 | 6 |
| [`IV_DoctoAplUso`](#iv_doctoapluso) | nucleo | 10 | 3 |
| [`IV_DoctoTipo`](#iv_doctotipo) | nucleo | 5 | 3 |
| [`IV_Evento`](#iv_evento) | isolada | 27 | 132 |
| [`IV_EventoAcao`](#iv_eventoacao) | vazia | 7 | 0 |
| [`IV_GlobalPar`](#iv_globalpar) | nucleo | 43 | 3.848 |
| [`IV_GlobalParCtrl`](#iv_globalparctrl) | catalogo | 54 | 48 |
| [`IV_GlobalParLista`](#iv_globalparlista) | nucleo | 4 | 197 |
| [`IV_Motivo`](#iv_motivo) | isolada | 4 | 43 |
| [`IV_PAREVTEXTRES`](#iv_parevtextres) | vazia | 7 | 0 |
| [`IV_ProcFase`](#iv_procfase) | nucleo | 5 | 273 |
| [`IV_ProcFaseMonit`](#iv_procfasemonit) | isolada | 9 | 655.144 |
| [`IV_ProcPersp`](#iv_procpersp) | nucleo | 4 | 67 |
| [`IV_ProcPerspMonit`](#iv_procperspmonit) | isolada | 6 | 126.145 |
| [`IV_ProcResultado`](#iv_procresultado) | nucleo | 5 | 2.153 |
| [`IV_ProcSt`](#iv_procst) | nucleo | 4 | 250 |
| [`IV_ProcStatMonit`](#iv_procstatmonit) | isolada | 6 | 850.022 |
| [`IV_ProcTpRel`](#iv_proctprel) | vazia | 5 | 0 |
| [`IV_Recurso`](#iv_recurso) | vazia | 5 | 0 |
| [`IV_RecUso`](#iv_recuso) | vazia | 7 | 0 |
| [`IV_ResClasse`](#iv_resclasse) | nucleo | 2 | 236 |
| [`IV_ResEvtOut`](#iv_resevtout) | vazia | 4 | 0 |
| [`IV_RESJOB`](#iv_resjob) | vazia | 2 | 0 |
| [`IV_ResMsgPapel`](#iv_resmsgpapel) | nucleo | 28 | 1.341 |
| [`IV_ResParam`](#iv_resparam) | vazia | 37 | 0 |
| [`IV_Resultado`](#iv_resultado) | nucleo | 63 | 4.201 |
| [`IV_ResultadoCmpl`](#iv_resultadocmpl) | nucleo | 2 | 3.110 |
| [`IV_ResultadoInstr`](#iv_resultadoinstr) | nucleo | 2 | 98 |
| [`IV_ResultadoReq`](#iv_resultadoreq) | vazia | 6 | 0 |
| [`IV_ResultadoWeb`](#iv_resultadoweb) | vazia | 10 | 0 |
| [`IV_ResVinc`](#iv_resvinc) | nucleo | 8 | 959 |
| [`IV_RetProcRegra`](#iv_retprocregra) | vazia | 7 | 0 |
| [`IV_SegPerfil`](#iv_segperfil) | vazia | 27 | 0 |
| [`IV_STATUS_DEPTO`](#iv_status_depto) | isolada | 5 | 473.844 |
| [`IV_TAGCAD`](#iv_tagcad) | vazia | 9 | 0 |
| [`IV_TIPOCONTEUDO`](#iv_tipoconteudo) | catalogo | 3 | 1 |
| [`IV_TpPgto`](#iv_tppgto) | vazia | 5 | 0 |
| [`IV_TxtPadConta`](#iv_txtpadconta) | vazia | 5 | 0 |
| [`IV_TxtPadrao`](#iv_txtpadrao) | catalogo | 11 | 212 |
| [`IV_TxtPadraoUso`](#iv_txtpadraouso) | nucleo | 4 | 5 |
| [`IV_Unidade`](#iv_unidade) | vazia | 10 | 0 |

---

### IV_Acao

`classe: catalogo` · `37 colunas` · `979 linhas (snapshot 03/06/2026)` · `PK: Acao`

**Funcao:** O motor BPM declarativo: 980 ações (378 ativas), 4.208 resultados, 5.958 regras de geração automática de agenda e 2.160 mapeamentos resultado→fase/status. Nenhum código: tudo é catálogo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Acao` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `SeqFormulario` | `numeric(18,0)` | sim | - | FK -> `IV_Formulario.SeqFormulario`; formulario (`IV_Formulario.SeqFormulario`) |
| 3 | `Pcte` | `varchar(4)` | sim | - |  |
| 4 | `DescReduzida` | `varchar(20)` | **nao** | - |  |
| 5 | `Descricao` | `varchar(40)` | **nao** | - | descricao do registro |
| 6 | `Sigla` | `varchar(5)` | sim | - |  |
| 7 | `Classe` | `char(1)` | **nao** | - |  |
| 8 | `PermiteExclusao` | `char(1)` | **nao** | - |  |
| 9 | `ExigeResposta` | `char(1)` | **nao** | - |  |
| 10 | `EmUso` | `char(1)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 11 | `Avulsa` | `char(1)` | sim | - |  |
| 12 | `ExigeProduto` | `char(1)` | sim | - |  |
| 13 | `ExigeVendedor` | `char(1)` | sim | - |  |
| 14 | `ExigeMotivo` | `char(1)` | sim | - |  |
| 15 | `ExigeDepto` | `char(1)` | sim | - |  |
| 16 | `ExigeDetalhe` | `char(1)` | sim | - |  |
| 17 | `ExigeFormulario` | `char(1)` | sim | - |  |
| 18 | `DescPesJuridica` | `numeric(1,0)` | sim | - |  |
| 19 | `PermiteReagendar` | `numeric(1,0)` | sim | - |  |
| 20 | `ExigePrazo` | `numeric(1,0)` | sim | - |  |
| 21 | `PrazoRealizacao` | `decimal(6,0)` | sim | - |  |
| 22 | `PrazoMaxInicio` | `decimal(4,0)` | sim | - |  |
| 23 | `PrazoMaxReag` | `numeric(8,0)` | sim | - |  |
| 24 | `TarefaCompromisso` | `char(1)` | sim | - |  |
| 25 | `PermTrocaTarComp` | `numeric(1,0)` | sim | - |  |
| 26 | `TempoMedio` | `decimal(4,0)` | sim | - |  |
| 27 | `QtdeMaxPessoa` | `decimal(2,0)` | sim | - |  |
| 28 | `QtdeMaxPessoaEmp` | `decimal(2,0)` | sim | - |  |
| 29 | `MinutoAntesPerm` | `decimal(6,0)` | sim | - |  |
| 30 | `Instrucao` | `varchar(250)` | sim | - |  |
| 31 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 32 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 33 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 34 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 35 | `DutUltAgenda` | `decimal(4,0)` | sim | - |  |
| 36 | `ExigeDtaLimite` | `numeric(1,0)` | sim | - |  |
| 37 | `QtdeMaxProcesso` | `numeric(2,0)` | sim | - |  |

**Referenciada por:** `IV_AcaoAtendente.Acao`, `IV_AcaoAuto.Acao`, `IV_AcaoCtrl.Acao`, `IV_AcaoMon.Acao`, `IV_AcaoRem.Acao`, `IV_Agenda.Acao`, `IV_CobrCrit.Acao`, `IV_Distribui.Acao`, `IV_ProcAcao.Acao`

---

### IV_ACAOANEXA

`classe: vazia` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de acao (tipo de tarefa), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQACAOANEXA` | `numeric(18,0)` | sim | - |  |
| 2 | `ACAO` | `numeric(6,0)` | **nao** | - | acao (`IV_Acao.Acao`) |
| 3 | `ACAOANEXA` | `numeric(6,0)` | **nao** | - |  |
| 4 | `DESCRICAO` | `varchar(100)` | **nao** | - | descricao do registro |
| 5 | `DESTINATARIO` | `numeric(18,0)` | **nao** | - |  |
| 6 | `QTDEDIAS` | `numeric(4,0)` | **nao** | - |  |
| 7 | `QTDEHU` | `numeric(8,2)` | sim | - |  |
| 8 | `INDINTERATIVO` | `numeric(1,0)` | sim | - |  |
| 9 | `INDOBRIGATORIO` | `numeric(1,0)` | sim | - |  |
| 10 | `INDATUALIZA` | `numeric(1,0)` | sim | - |  |
| 11 | `INDRECRIA` | `numeric(1,0)` | sim | - |  |

---

### IV_AcaoAtendente

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Acao, SeqUsuario`

**Funcao:** ainda IV_AcaoAtendente (limites Qtdemaxdia/Qtdemaxhora/Qtdealertadia/Qtdealertahora — 0 linhas) e a política ATD_AG0_70_QTLM (limite de linhas na agenda). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Acao` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Operador.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `Qtdemaxdia` | `numeric(5,0)` | sim | - |  |
| 4 | `Qtdemaxhora` | `numeric(5,0)` | sim | - |  |
| 5 | `Qtdealertadia` | `numeric(5,0)` | sim | - |  |
| 6 | `Qtdealertahora` | `numeric(5,0)` | sim | - |  |

---

### IV_AcaoAuto

`classe: nucleo` · `31 colunas` · `5.942 linhas (snapshot 03/06/2026)` · `PK: SeqAcaoAuto`

**Funcao:** O motor BPM declarativo: 980 ações (378 ativas), 4.208 resultados, 5.958 regras de geração automática de agenda e 2.160 mapeamentos resultado→fase/status. Nenhum código: tudo é catálogo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAcaoAuto` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Pcte` | `varchar(4)` | sim | - |  |
| 3 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 4 | `Resultado` | `decimal(6,0)` | **nao** | - | FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 5 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |
| 6 | `Acao` | `decimal(6,0)` | sim | - | FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 7 | `SeqUsuario` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 8 | `AssuntoCmpl` | `varchar(40)` | sim | - |  |
| 9 | `Operacao` | `varchar(2)` | sim | - |  |
| 10 | `UsaObjDyn` | `numeric(1,0)` | sim | - |  |
| 11 | `Exigida` | `char(1)` | **nao** | - |  |
| 12 | `GerarPara` | `char(1)` | sim | - |  |
| 13 | `FormaGeracao` | `char(1)` | sim | - |  |
| 14 | `QtdeDias` | `numeric(4,0)` | **nao** | - |  |
| 15 | `QtdeHU` | `decimal(8,2)` | sim | - |  |
| 16 | `Departamento` | `varchar(20)` | sim | - | departamento |
| 17 | `AtivoReceptivo` | `char(1)` | sim | - |  |
| 18 | `FormaPrimCont` | `varchar(30)` | sim | - |  |
| 19 | `Prioridade` | `decimal(1,0)` | sim | - |  |
| 20 | `SemConfirmacao` | `char(1)` | sim | - |  |
| 21 | `TipoAgendamento` | `char(1)` | sim | - |  |
| 22 | `Mesmoprocesso` | `char(1)` | sim | - |  |
| 23 | `SeqOrder` | `decimal(4,0)` | sim | - |  |
| 24 | `CHKSUM` | `varchar(50)` | sim | - |  |
| 25 | `QuebraDNA` | `numeric(1,0)` | sim | - |  |
| 26 | `EXIGESCOLHAATDGRPO` | `numeric(1,0)` | sim | - |  |
| 27 | `INDGERAPARAPESPRC` | `numeric(1,0)` | sim | - |  |
| 28 | `EMUSO` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 29 | `PROPAGAFORMTRAB` | `numeric(1,0)` | sim | - |  |
| 30 | `SOLICDATAAGD` | `numeric(1,0)` | sim | - |  |
| 31 | `HORAAGENDA` | `datetime` | sim | - |  |

---

### IV_AcaoAutoCtrl

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAcaoAutoCtrl`

**Funcao:** A regra de automação NÃO tem linguagem de condição: IV_AcaoAutoCtrl (a tabela de condições) está VAZIA e UsaObjDyn=0 em 100% das linhas preenchidas — o único discriminante real é (Resultado, NroEmpresa). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAcaoAutoCtrl` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `Tipo` | `varchar(10)` | sim | - |  |
| 3 | `CtrlNum` | `numeric(18,0)` | sim | - |  |
| 4 | `CtrlNum2` | `numeric(18,0)` | sim | - |  |
| 5 | `CtrlStr` | `varchar(40)` | sim | - |  |
| 6 | `SeqAcaoAuto` | `numeric(18,0)` | **nao** | - |  |

---

### IV_AcaoCmpl

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Acao, AssuntoCmpl`

**Funcao:** _(inferido)_ Pelo nome, e um complemento do registro pai relacionada a acao (tipo de tarefa), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Acao` | `numeric(6,0)` | **nao** | - | **PK**; acao (`IV_Acao.Acao`) |
| 2 | `AssuntoCmpl` | `varchar(40)` | **nao** | - | **PK** |

---

### IV_AcaoCtrl

`classe: nucleo` · `13 colunas` · `44 linhas (snapshot 03/06/2026)` · `PK: Acao, SeqCtrl`

**Funcao:** Os outros três primitivos de automação: remoção de tarefa (266), escalação por e-mail (44), round-robin (609). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Acao` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 2 | `SeqCtrl` | `decimal(3,0)` | **nao** | - | **PK** |
| 3 | `Base` | `char(1)` | sim | - |  |
| 4 | `SeqUsrBase` | `numeric(18,0)` | sim | - |  |
| 5 | `Minuto` | `decimal(8,0)` | sim | - |  |
| 6 | `Destinatario` | `numeric(18,0)` | sim | - |  |
| 7 | `Atitude` | `varchar(10)` | sim | - |  |
| 8 | `IntervaloExec` | `decimal(4,0)` | sim | - |  |
| 9 | `DtaProxExec` | `datetime` | sim | - |  |
| 10 | `TipoDest` | `varchar(5)` | sim | - |  |
| 11 | `Papel` | `varchar(25)` | sim | - |  |
| 12 | `SeqTxtPadrao` | `numeric(18,0)` | sim | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 13 | `Evento` | `varchar(30)` | sim | - |  |

---

### IV_AcaoMon

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Acao, SeqAcMon`

**Funcao:** _(inferido)_ Pelo nome, e um monitoramento relacionada a acao (tipo de tarefa), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Acao` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 2 | `SeqAcMon` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 4 | `TpMon` | `varchar(3)` | **nao** | - |  |
| 5 | `ResProd` | `char(1)` | sim | - |  |
| 6 | `ResImprod` | `char(1)` | sim | - |  |
| 7 | `ResEncer` | `char(1)` | sim | - |  |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_AcaoRem

`classe: nucleo` · `9 colunas` · `266 linhas (snapshot 03/06/2026)` · `PK: SeqAcaoRem`

**Funcao:** Os outros três primitivos de automação: remoção de tarefa (266), escalação por e-mail (44), round-robin (609). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAcaoRem` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Pcte` | `varchar(4)` | **nao** | - |  |
| 3 | `Resultado` | `decimal(6,0)` | sim | - | FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 4 | `Acao` | `decimal(6,0)` | sim | - | FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 5 | `NaoTrabalhada` | `numeric(1,0)` | sim | - |  |
| 6 | `Interativo` | `numeric(1,0)` | sim | - |  |
| 7 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 8 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 9 | `INDREMPES` | `numeric(1,0)` | sim | - |  |

---

### IV_ACAOURACENARIO

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: ACAO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de acao (tipo de tarefa), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ACAO` | `numeric(6,0)` | **nao** | - | **PK**; acao (`IV_Acao.Acao`) |
| 2 | `SEQURACENARIO` | `numeric(18,0)` | **nao** | - |  |

---

### IV_ClasseRes

`classe: nucleo` · `5 colunas` · `40 linhas (snapshot 03/06/2026)` · `PK: SeqClasseRes`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de resultado (desfecho de um andamento), no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqClasseRes` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `Classe` | `varchar(20)` | **nao** | - |  |
| 3 | `Cor` | `numeric(18,0)` | sim | - |  |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IV_ResClasse.SeqClasseRes`

---

### IV_CodPrcEmpr

`classe: nucleo` · `2 colunas` · `17 linhas (snapshot 03/06/2026)` · `PK: NroEmpresa, CodProcesso`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de empresa (multiempresa), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `GE_Empresa.NroEmpresa`; multiempresa - filial/empresa |
| 2 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |

---

### IV_CodProcComent

`classe: nucleo` · `4 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: CodProcesso, SeqProcComent`

**Funcao:** GE_PessoaNota, IV_ProcComent e IV_CodProcComent (comentário por modelo de processo) existem mas estão zerados. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `SeqProcComent` | `decimal(2,0)` | **nao** | - | **PK** |
| 3 | `ProcComent` | `varchar(20)` | **nao** | - |  |
| 4 | `Cor` | `numeric(18,0)` | sim | - |  |

---

### IV_CodProcesso

`classe: catalogo` · `35 colunas` · `62 linhas (snapshot 03/06/2026)` · `PK: CodProcesso`

**Funcao:** Catálogo dos TIPOS DE FLUXO (BPM). 35 col, 62 linhas, 58 EmUso=1. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `Pcte` | `varchar(4)` | sim | - |  |
| 3 | `Descricao` | `varchar(30)` | sim | - | descricao do registro |
| 4 | `DescrRed` | `varchar(15)` | sim | - |  |
| 5 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 6 | `TipoProcesso` | `varchar(10)` | sim | - |  |
| 7 | `UsaPerspectiva` | `numeric(1,0)` | sim | - |  |
| 8 | `UsaPercConclusao` | `numeric(1,0)` | sim | - |  |
| 9 | `UsaValor` | `numeric(1,0)` | sim | - |  |
| 10 | `UsaMaterial` | `numeric(1,0)` | sim | - |  |
| 11 | `UsaStatus` | `numeric(1,0)` | sim | - |  |
| 12 | `UsaStatusDes` | `numeric(1,0)` | sim | - |  |
| 13 | `UsaResumo` | `numeric(1,0)` | sim | - |  |
| 14 | `UsaFichCad` | `numeric(1,0)` | sim | - |  |
| 15 | `DescValor` | `varchar(20)` | sim | - |  |
| 16 | `DescPersp` | `varchar(20)` | sim | - |  |
| 17 | `UsaQtde` | `numeric(1,0)` | sim | - |  |
| 18 | `DescQtde` | `varchar(20)` | sim | - |  |
| 19 | `UsaProduto` | `numeric(1,0)` | sim | - |  |
| 20 | `ProdFamilia` | `varchar(250)` | sim | - |  |
| 21 | `CorLinha` | `numeric(18,0)` | sim | - |  |
| 22 | `AcaoProsp` | `decimal(6,0)` | sim | - |  |
| 23 | `AcaoAcomp` | `decimal(6,0)` | sim | - |  |
| 24 | `AcaoVenda` | `decimal(6,0)` | sim | - |  |
| 25 | `FaseEnvioERP` | `decimal(2,0)` | sim | - |  |
| 26 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 27 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 28 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 29 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 30 | `CriaFichaNeg` | `numeric(1,0)` | sim | - |  |
| 31 | `ResProrrConcl` | `numeric(18,0)` | sim | - |  |
| 32 | `QTDPRODUTO` | `numeric(4,0)` | sim | - |  |
| 33 | `QTDPRODUTOUND` | `numeric(4,0)` | sim | - |  |
| 34 | `INDFASEBASEACAO` | `numeric(1,0)` | sim | - |  |
| 35 | `INDUSABOARD` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `IVS_Depto.CodProcesso`, `IVS_UsrMeta.CodProcesso`, `IV_CodPrcEmpr.CodProcesso`, `IV_CodProcComent.CodProcesso`, `IV_ProcAcao.CodProcesso`, `IV_ProcFase.CodProcesso`, `IV_ProcPersp.CodProcesso`, `IV_ProcResultado.CodProcesso`, `IV_ProcSt.CodProcesso`

---

### IV_Departamento

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Departamento`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de departamento, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Departamento` | `varchar(12)` | **nao** | - | **PK**; departamento |
| 2 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 3 | `SeqDepartamento` | `int(10,0)` | sim | - |  |

---

### IV_DoctoApl

`classe: nucleo` · `15 colunas` · `6 linhas (snapshot 03/06/2026)` · `PK: SeqDoctoApl`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de documento, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDoctoApl` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Docto` | `varchar(10)` | **nao** | - | FK -> `IV_DoctoTipo.Docto` |
| 3 | `Atividade` | `varchar(40)` | sim | - |  |
| 4 | `TipoAplicativo` | `varchar(5)` | sim | - |  |
| 5 | `LocalAplicativo` | `varchar(50)` | sim | - |  |
| 6 | `Aplicativo` | `varchar(100)` | sim | - |  |
| 7 | `ParamInicial` | `varchar(200)` | sim | - |  |
| 8 | `Param` | `varchar(200)` | sim | - |  |
| 9 | `ParamFinal` | `varchar(200)` | sim | - |  |
| 10 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 11 | `Sessao` | `varchar(20)` | sim | - |  |
| 12 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 13 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 14 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 15 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IV_DoctoAplUso.SeqDoctoApl`

---

### IV_DoctoAplUso

`classe: nucleo` · `10 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: SeqDoctoAplUso`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de documento, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDoctoAplUso` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqDoctoApl` | `numeric(18,0)` | sim | - | FK -> `IV_DoctoApl.SeqDoctoApl` |
| 3 | `Uso` | `varchar(30)` | sim | - |  |
| 4 | `Referencia` | `varchar(30)` | sim | - |  |
| 5 | `Chave` | `numeric(18,0)` | sim | - |  |
| 6 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 7 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 8 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_DoctoTipo

`classe: nucleo` · `5 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: Docto`

**Funcao:** _(inferido)_ Pelo nome, e um catalogo de tipos relacionada a documento, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Docto` | `varchar(10)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 3 | `IndIncLinkManual` | `numeric(1,0)` | sim | - |  |
| 4 | `IndVerLink` | `numeric(1,0)` | sim | - |  |
| 5 | `SeqDoctoTipo` | `numeric(4,0)` | sim | - |  |

**Referenciada por:** `IV_DoctoApl.Docto`

---

### IV_Evento

`classe: isolada` · `27 colunas` · `132 linhas (snapshot 03/06/2026)` · `PK: SeqEvento`

**Funcao:** Arquitetura de integração em três camadas: 132 eventos externos configurados que traduzem fato do ERP em andamento/processo/ação; 23 tabelas de staging IMP_; 46 tabelas consolidadas EXT_; DE-PARA de códigos. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqEvento` | `decimal(8,0)` | **nao** | - | **PK** |
| 2 | `Pcte` | `varchar(4)` | sim | - |  |
| 3 | `Evento` | `varchar(200)` | sim | - |  |
| 4 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 5 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 6 | `NroEmpresaDestino` | `numeric(6,0)` | sim | - |  |
| 7 | `ResultAtivo` | `numeric(6,0)` | sim | - |  |
| 8 | `GeraAndSempre` | `numeric(1,0)` | sim | - |  |
| 9 | `ResultRec` | `numeric(6,0)` | sim | - |  |
| 10 | `ResultRecPE` | `numeric(6,0)` | sim | - |  |
| 11 | `GeraRecSempre` | `numeric(1,0)` | sim | - |  |
| 12 | `AtuHistProcesso` | `numeric(1,0)` | sim | - |  |
| 13 | `Descricao` | `varchar(200)` | sim | - | descricao do registro |
| 14 | `DetalheAuto` | `varchar(200)` | sim | - |  |
| 15 | `ProcedureAdicional` | `varchar(200)` | sim | - |  |
| 16 | `ProcessoUnico` | `numeric(1,0)` | sim | - |  |
| 17 | `GeraAcao` | `numeric(1,0)` | sim | - |  |
| 18 | `ExcluiProcesso` | `varchar(3)` | sim | - |  |
| 19 | `ObsTec` | `varchar(250)` | sim | - |  |
| 20 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 21 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 22 | `DtaUltUso` | `datetime` | sim | - |  |
| 23 | `DescAcaoOutroProc` | `numeric(1,0)` | sim | - |  |
| 24 | `RESULTRECHSTE` | `numeric(6,0)` | sim | - |  |
| 25 | `RESULTRECHSTNE` | `numeric(6,0)` | sim | - |  |
| 26 | `DESCACAOOUTRAEMPR` | `numeric(1,0)` | sim | - |  |
| 27 | `EMUSO` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |

---

### IV_EventoAcao

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqEventoAcao`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de acao (tipo de tarefa), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqEventoAcao` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqEvento` | `numeric(8,0)` | **nao** | - |  |
| 3 | `ACAO` | `numeric(6,0)` | **nao** | - | acao (`IV_Acao.Acao`) |
| 4 | `AttdeHaAndamento` | `char(1)` | sim | - |  |
| 5 | `ResHaAndamento` | `numeric(6,0)` | sim | - |  |
| 6 | `AttdeNaoHaAndamento` | `char(1)` | sim | - |  |
| 7 | `ResNaoHaAndamento` | `numeric(6,0)` | sim | - |  |

---

### IV_GlobalPar

`classe: nucleo` · `43 colunas` · `3.848 linhas (snapshot 03/06/2026)` · `PK: SeqPar`

**Funcao:** EAV de listas/parâmetros globais (marcas, modelos, famílias, faixas de potência). Base das 48 views IV$PG_*, filtradas por SEQGLBPAR. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqGlbPar` | `numeric(4,0)` | **nao** | - | FK -> `IV_GlobalParCtrl.SeqGlbPar` |
| 3 | `NroEmpresa` | `varchar(18)` | sim | - | multiempresa - filial/empresa |
| 4 | `Campo1` | `varchar(20)` | sim | - |  |
| 5 | `Campo2` | `varchar(20)` | sim | - |  |
| 6 | `Campo3` | `varchar(20)` | sim | - |  |
| 7 | `Campo4` | `varchar(20)` | sim | - |  |
| 8 | `Campo5` | `varchar(20)` | sim | - |  |
| 9 | `Campo6` | `varchar(20)` | sim | - |  |
| 10 | `Numero1` | `decimal(15,2)` | sim | - |  |
| 11 | `Numero2` | `decimal(15,2)` | sim | - |  |
| 12 | `Numero3` | `decimal(15,2)` | sim | - |  |
| 13 | `Numero4` | `decimal(15,2)` | sim | - |  |
| 14 | `Numero5` | `decimal(15,2)` | sim | - |  |
| 15 | `Numero6` | `decimal(15,2)` | sim | - |  |
| 16 | `Data1` | `datetime` | sim | - |  |
| 17 | `Data2` | `datetime` | sim | - |  |
| 18 | `Data3` | `datetime` | sim | - |  |
| 19 | `Data4` | `datetime` | sim | - |  |
| 20 | `Data5` | `datetime` | sim | - |  |
| 21 | `Data6` | `datetime` | sim | - |  |
| 22 | `Literal1` | `varchar(100)` | sim | - |  |
| 23 | `Literal2` | `varchar(100)` | sim | - |  |
| 24 | `Literal3` | `varchar(100)` | sim | - |  |
| 25 | `Literal4` | `varchar(100)` | sim | - |  |
| 26 | `Literal5` | `varchar(100)` | sim | - |  |
| 27 | `Literal6` | `varchar(100)` | sim | - |  |
| 28 | `Literal7` | `varchar(100)` | sim | - |  |
| 29 | `Literal8` | `varchar(100)` | sim | - |  |
| 30 | `Literal9` | `varchar(100)` | sim | - |  |
| 31 | `Literal10` | `varchar(100)` | sim | - |  |
| 32 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 33 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 34 | `SimNao1` | `numeric(1,0)` | sim | - |  |
| 35 | `SimNao2` | `varchar(30)` | sim | - |  |
| 36 | `SimNao3` | `varchar(30)` | sim | - |  |
| 37 | `SimNao4` | `varchar(30)` | sim | - |  |
| 38 | `SimNao5` | `varchar(30)` | sim | - |  |
| 39 | `SimNao6` | `varchar(30)` | sim | - |  |
| 40 | `SimNao7` | `varchar(30)` | sim | - |  |
| 41 | `SimNao8` | `varchar(30)` | sim | - |  |
| 42 | `SimNao9` | `varchar(30)` | sim | - |  |
| 43 | `SimNao10` | `varchar(30)` | sim | - |  |

---

### IV_GlobalParCtrl

`classe: catalogo` · `54 colunas` · `48 linhas (snapshot 03/06/2026)` · `PK: SeqGlbPar`

**Funcao:** EAV de listas/parâmetros globais (marcas, modelos, famílias, faixas de potência). Base das 48 views IV$PG_*, filtradas por SEQGLBPAR. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqGlbPar` | `numeric(4,0)` | **nao** | - | **PK** |
| 2 | `Pcte` | `varchar(4)` | sim | - |  |
| 3 | `Parametro` | `varchar(20)` | **nao** | - |  |
| 4 | `PorEmpresa` | `numeric(1,0)` | **nao** | - |  |
| 5 | `Unico` | `numeric(1,0)` | **nao** | - |  |
| 6 | `CampoListar` | `varchar(15)` | sim | - |  |
| 7 | `Instrucao` | `varchar(250)` | sim | - |  |
| 8 | `Campo1` | `varchar(20)` | sim | - |  |
| 9 | `Campo2` | `varchar(20)` | sim | - |  |
| 10 | `Campo3` | `varchar(20)` | sim | - |  |
| 11 | `Campo4` | `varchar(20)` | sim | - |  |
| 12 | `Campo5` | `varchar(20)` | sim | - |  |
| 13 | `Campo6` | `varchar(20)` | sim | - |  |
| 14 | `Numero1` | `varchar(20)` | sim | - |  |
| 15 | `Numero2` | `varchar(20)` | sim | - |  |
| 16 | `Numero3` | `varchar(20)` | sim | - |  |
| 17 | `Numero4` | `varchar(20)` | sim | - |  |
| 18 | `Numero5` | `varchar(20)` | sim | - |  |
| 19 | `Numero6` | `varchar(20)` | sim | - |  |
| 20 | `Data1` | `varchar(20)` | sim | - |  |
| 21 | `Data2` | `varchar(20)` | sim | - |  |
| 22 | `Data3` | `varchar(20)` | sim | - |  |
| 23 | `Data4` | `varchar(20)` | sim | - |  |
| 24 | `Data5` | `varchar(20)` | sim | - |  |
| 25 | `Data6` | `varchar(20)` | sim | - |  |
| 26 | `Literal1` | `varchar(20)` | sim | - |  |
| 27 | `Literal2` | `varchar(20)` | sim | - |  |
| 28 | `Literal3` | `varchar(20)` | sim | - |  |
| 29 | `Literal4` | `varchar(20)` | sim | - |  |
| 30 | `Literal5` | `varchar(20)` | sim | - |  |
| 31 | `Literal6` | `varchar(20)` | sim | - |  |
| 32 | `Literal7` | `varchar(20)` | sim | - |  |
| 33 | `Literal8` | `varchar(20)` | sim | - |  |
| 34 | `Literal9` | `varchar(20)` | sim | - |  |
| 35 | `Literal10` | `varchar(20)` | sim | - |  |
| 36 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 37 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 38 | `CampoListar2` | `varchar(15)` | sim | - |  |
| 39 | `SimNao1` | `varchar(30)` | sim | - |  |
| 40 | `SimNao2` | `varchar(30)` | sim | - |  |
| 41 | `SimNao3` | `varchar(30)` | sim | - |  |
| 42 | `SimNao4` | `varchar(30)` | sim | - |  |
| 43 | `SimNao5` | `varchar(30)` | sim | - |  |
| 44 | `SimNao6` | `varchar(30)` | sim | - |  |
| 45 | `SimNao7` | `varchar(30)` | sim | - |  |
| 46 | `SimNao8` | `varchar(30)` | sim | - |  |
| 47 | `SimNao9` | `varchar(30)` | sim | - |  |
| 48 | `SimNao10` | `varchar(30)` | sim | - |  |
| 49 | `Campo1Sql` | `smallint(5,0)` | sim | - |  |
| 50 | `Campo2Sql` | `smallint(5,0)` | sim | - |  |
| 51 | `Campo3Sql` | `smallint(5,0)` | sim | - |  |
| 52 | `Campo4Sql` | `smallint(5,0)` | sim | - |  |
| 53 | `Campo5Sql` | `smallint(5,0)` | sim | - |  |
| 54 | `Campo6Sql` | `smallint(5,0)` | sim | - |  |

**Referenciada por:** `IV_GlobalPar.SeqGlbPar`, `IV_GlobalParLista.SeqGlbPar`

---

### IV_GlobalParLista

`classe: nucleo` · `4 colunas` · `197 linhas (snapshot 03/06/2026)` · `PK: SeqGlbPar, Campo, SeqPropriLista`

**Funcao:** _(inferido)_ Pelo nome, e uma lista de opcoes de dominio relacionada a parametrizacao, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqGlbPar` | `numeric(4,0)` | **nao** | - | **PK**; FK -> `IV_GlobalParCtrl.SeqGlbPar` |
| 2 | `Campo` | `varchar(2)` | **nao** | - | **PK** |
| 3 | `SeqPropriLista` | `decimal(4,0)` | **nao** | - | **PK** |
| 4 | `Lista` | `varchar(30)` | sim | - |  |

---

### IV_Motivo

`classe: isolada` · `4 colunas` · `43 linhas (snapshot 03/06/2026)` · `PK: Motivo`

**Funcao:** origem/motivo do lead, catálogo saudável e em uso (Instagram Anúncio, RD Tallos, Google anúncio, Landing Page, John Deere, MF Rural...) (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Motivo` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(40)` | **nao** | - | descricao do registro |
| 3 | `SeqMotivo` | `numeric(18,0)` | sim | - |  |
| 4 | `Link` | `varchar(20)` | sim | - |  |

---

### IV_PAREVTEXTRES

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQPAREVTRES`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de resultado (desfecho de um andamento), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPAREVTRES` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TABELA` | `varchar(30)` | **nao** | - |  |
| 3 | `EVENTO` | `varchar(30)` | **nao** | - |  |
| 4 | `TIPOPRODUTO` | `varchar(250)` | sim | - |  |
| 5 | `TIPOOUTRO` | `varchar(250)` | sim | - |  |
| 6 | `DETALHEADIC` | `varchar(250)` | sim | - |  |
| 7 | `RESULTADO` | `numeric(6,0)` | **nao** | - | resultado (`IV_Resultado.Resultado`) |

---

### IV_ProcFase

`classe: nucleo` · `5 colunas` · `273 linhas (snapshot 03/06/2026)` · `PK: CodProcesso, Fase`

**Funcao:** Não tem FK para IV_ProcFase/IV_ProcSt — os nomes de fase/status são strings livres (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `Fase` | `varchar(20)` | **nao** | - | **PK**; fase do fluxo (`IV_ProcFase`) |
| 3 | `FaseOrdem` | `decimal(2,0)` | sim | - | fase do fluxo (`IV_ProcFase`) |
| 4 | `Padrao` | `numeric(1,0)` | sim | - |  |
| 5 | `Marco` | `varchar(20)` | sim | - |  |

---

### IV_ProcFaseMonit

`classe: isolada` · `9 colunas` · `655.144 linhas (snapshot 03/06/2026)` · `PK: Processo, SeqFase`

**Funcao:** Instrumentação de SLA do BPM: 1,66 milhão de linhas de série temporal medindo quanto tempo cada processo passou em cada fase, status e perspectiva, com horas úteis (HU) separadas de horas corridas (HN). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `SeqFase` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `FASE` | `varchar(20)` | **nao** | - | fase do fluxo (`IV_ProcFase`) |
| 4 | `FaseOrdem` | `numeric(2,0)` | sim | - | fase do fluxo (`IV_ProcFase`) |
| 5 | `DtaInicio` | `datetime` | sim | - |  |
| 6 | `DtaFim` | `datetime` | sim | - |  |
| 7 | `UltHistorico` | `numeric(18,0)` | sim | - |  |
| 8 | `HU` | `numeric(6,2)` | sim | - |  |
| 9 | `HN` | `numeric(6,2)` | sim | - |  |

---

### IV_ProcPersp

`classe: nucleo` · `4 colunas` · `67 linhas (snapshot 03/06/2026)` · `PK: CodProcesso, PerspOrdem`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `PerspOrdem` | `decimal(2,0)` | **nao** | - | **PK** |
| 3 | `Perspectiva` | `varchar(20)` | **nao** | - |  |
| 4 | `QtdDiaPro` | `decimal(6,2)` | sim | - |  |

---

### IV_ProcPerspMonit

`classe: isolada` · `6 colunas` · `126.145 linhas (snapshot 03/06/2026)` · `PK: Processo, DtaMonit`

**Funcao:** Instrumentação de SLA do BPM: 1,66 milhão de linhas de série temporal medindo quanto tempo cada processo passou em cada fase, status e perspectiva, com horas úteis (HU) separadas de horas corridas (HN). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `DtaMonit` | `datetime` | **nao** | - | **PK** |
| 3 | `Perspectiva` | `decimal(3,0)` | sim | - |  |
| 4 | `HU` | `decimal(6,2)` | sim | - |  |
| 5 | `HN` | `decimal(6,2)` | sim | - |  |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_ProcResultado

`classe: nucleo` · `5 colunas` · `2.153 linhas (snapshot 03/06/2026)` · `PK: CodProcesso, Resultado`

**Funcao:** O motor BPM declarativo: 980 ações (378 ativas), 4.208 resultados, 5.958 regras de geração automática de agenda e 2.160 mapeamentos resultado→fase/status. Nenhum código: tudo é catálogo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 3 | `Fase` | `varchar(20)` | sim | - | fase do fluxo (`IV_ProcFase`) |
| 4 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 5 | `FaseSeguinte` | `varchar(20)` | sim | - |  |

---

### IV_ProcSt

`classe: nucleo` · `4 colunas` · `250 linhas (snapshot 03/06/2026)` · `PK: CodProcesso, Status`

**Funcao:** Não tem FK para IV_ProcFase/IV_ProcSt — os nomes de fase/status são strings livres (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `Status` | `varchar(20)` | **nao** | - | **PK**; status - validar dominio real por tabela |
| 3 | `StatusOrdem` | `decimal(2,0)` | sim | - |  |
| 4 | `Padrao` | `numeric(1,0)` | sim | - |  |

---

### IV_ProcStatMonit

`classe: isolada` · `6 colunas` · `850.022 linhas (snapshot 03/06/2026)` · `PK: Processo, DtaMonit`

**Funcao:** Instrumentação de SLA do BPM: 1,66 milhão de linhas de série temporal medindo quanto tempo cada processo passou em cada fase, status e perspectiva, com horas úteis (HU) separadas de horas corridas (HN). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `DtaMonit` | `datetime` | **nao** | - | **PK** |
| 3 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 4 | `HU` | `decimal(6,2)` | sim | - |  |
| 5 | `HN` | `decimal(6,2)` | sim | - |  |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_ProcTpRel

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTpRel`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTpRel` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `DescReduzida` | `varchar(20)` | sim | - |  |
| 3 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IV_ProcRelacao.SeqTpRel`

---

### IV_Recurso

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRecurso`

**Funcao:** Tela IVS1REC01 'Recursos' + IV_Recurso (0 linhas: SeqRecurso, Tipo, Descricao, DescRed, EmUso) e IV_RecUso (uso do recurso no tempo); (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRecurso` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Tipo` | `varchar(20)` | sim | - |  |
| 3 | `Descricao` | `varchar(60)` | sim | - | descricao do registro |
| 4 | `DescRed` | `varchar(30)` | sim | - |  |
| 5 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |

**Referenciada por:** `IV_AgdRec.SeqRecurso`, `IV_RecUso.SeqRecurso`

---

### IV_RecUso

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRecurso, SeqrecUso`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRecurso` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Recurso.SeqRecurso` |
| 2 | `SeqrecUso` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `DtaUso` | `datetime` | sim | - |  |
| 4 | `CodUsuario` | `varchar(20)` | sim | - | login do usuario (varchar) |
| 5 | `SeqAgenda` | `numeric(18,0)` | sim | - | agenda (`IV_Agenda.SeqAgenda`) |
| 6 | `SeqHistorico` | `numeric(18,0)` | sim | - | historico (`IV_Historico.SeqHistorico`) |
| 7 | `Obs` | `varchar(200)` | sim | - | texto livre |

---

### IV_ResClasse

`classe: nucleo` · `2 colunas` · `236 linhas (snapshot 03/06/2026)` · `PK: Resultado, SeqClasseRes`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de segmentacao, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 2 | `SeqClasseRes` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_ClasseRes.SeqClasseRes` |

---

### IV_ResEvtOut

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Resultado, SeqEvt`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de evento, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 2 | `SeqEvt` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Destino` | `varchar(20)` | sim | - |  |
| 4 | `Evento` | `varchar(40)` | sim | - |  |

---

### IV_RESJOB

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: RESULTADO, SEQJOB`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de job/sincronizacao, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `RESULTADO` | `numeric(6,0)` | **nao** | - | **PK**; resultado (`IV_Resultado.Resultado`) |
| 2 | `SEQJOB` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GEP_JOBCAD.SeqJOB` |

---

### IV_ResMsgPapel

`classe: nucleo` · `28 colunas` · `1.341 linhas (snapshot 03/06/2026)` · `PK: SeqResMsgPapel`

**Funcao:** Motor de notificação declarativo: 1.341 regras 'resultado X → mensagem para papel/usuário Y pelo canal Z com template W', com fila assíncrona (job EMAIL_SEND a cada 3 min) e 585.938 envios registrados. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqResMsgPapel` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Resultado` | `decimal(6,0)` | **nao** | - | FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 3 | `AtitudeUsr` | `char(1)` | sim | - |  |
| 4 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 5 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |
| 6 | `Grupo` | `varchar(10)` | sim | - |  |
| 7 | `Ordem` | `decimal(4,0)` | sim | - |  |
| 8 | `TipoDest` | `varchar(5)` | sim | - |  |
| 9 | `Papel` | `varchar(25)` | sim | - |  |
| 10 | `Canal` | `varchar(3)` | **nao** | - |  |
| 11 | `SeqTxtPadrao` | `numeric(18,0)` | sim | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 12 | `Template` | `varchar(250)` | sim | - |  |
| 13 | `Remetente` | `varchar(60)` | sim | - |  |
| 14 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 15 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 16 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 17 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 18 | `PesFormulario` | `numeric(18,0)` | sim | - | FK -> `IV_Formulario.SeqFormulario` |
| 19 | `PesResultado` | `numeric(6,0)` | sim | - |  |
| 20 | `QTDEDIAS` | `numeric(4,0)` | sim | - |  |
| 21 | `QTDEHU` | `numeric(8,2)` | sim | - |  |
| 22 | `SEQCONTAENVIO` | `numeric(18,0)` | sim | - | FK -> `GE_ParamLista.SeqParamLista` |
| 23 | `TIPOPREFENVIO` | `varchar(10)` | sim | - |  |
| 24 | `INDENVIAQLQERHORA` | `numeric(1,0)` | sim | - |  |
| 25 | `EMUSO` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 26 | `PRIORIDADE` | `numeric(1,0)` | sim | - |  |
| 27 | `DESCRICAOMENSAGEM` | `varchar(40)` | sim | - |  |
| 28 | `OBJETIVOMSG` | `varchar(25)` | sim | - |  |

---

### IV_ResParam

`classe: vazia` · `37 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Resultado`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de parametrizacao, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 2 | `Advertencia` | `varchar(200)` | sim | - |  |
| 3 | `CmpltTxtPadrao` | `varchar(250)` | sim | - |  |
| 4 | `AssuntoCmpl` | `varchar(40)` | sim | - |  |
| 5 | `AssuntoEmail` | `varchar(40)` | sim | - |  |
| 6 | `PermEMail` | `numeric(1,0)` | sim | - |  |
| 7 | `PermReagendar` | `numeric(1,0)` | sim | - |  |
| 8 | `ReagendarPara` | `decimal(6,0)` | sim | - |  |
| 9 | `HoraReagenda` | `decimal(6,2)` | sim | - |  |
| 10 | `HoraProrrAgda` | `decimal(6,2)` | sim | - |  |
| 11 | `HoraProrrLimite` | `decimal(6,2)` | sim | - |  |
| 12 | `PermProrrogar` | `numeric(1,0)` | sim | - |  |
| 13 | `PermIncManual` | `numeric(1,0)` | sim | - |  |
| 14 | `ExigPessoaAtiva` | `numeric(1,0)` | sim | - |  |
| 15 | `ExigDuracao` | `numeric(1,0)` | sim | - |  |
| 16 | `ExigClienteAtivo` | `numeric(1,0)` | sim | - |  |
| 17 | `ExigDetalhe` | `numeric(1,0)` | **nao** | - |  |
| 18 | `DescValor` | `varchar(20)` | sim | - |  |
| 19 | `ExigValor` | `numeric(1,0)` | sim | - |  |
| 20 | `DescQtde` | `varchar(20)` | sim | - |  |
| 21 | `ExigQtde` | `numeric(1,0)` | sim | - |  |
| 22 | `CtrlProcProj` | `numeric(1,0)` | sim | - |  |
| 23 | `CtrlProcProd` | `numeric(1,0)` | sim | - |  |
| 24 | `CtrlProcResp` | `numeric(1,0)` | sim | - |  |
| 25 | `CtrlProcVend` | `numeric(1,0)` | sim | - |  |
| 26 | `CtrlProcVlr` | `numeric(1,0)` | sim | - |  |
| 27 | `CtrlProcQtd` | `numeric(1,0)` | sim | - |  |
| 28 | `CtrlProcDtPrvConc` | `numeric(1,0)` | sim | - |  |
| 29 | `CtrlProcDepto` | `numeric(1,0)` | sim | - |  |
| 30 | `CtrlProcCampanha` | `numeric(1,0)` | sim | - |  |
| 31 | `CtrlProcStatus` | `numeric(1,0)` | sim | - |  |
| 32 | `CtrlProcDetalhe` | `numeric(1,0)` | sim | - |  |
| 33 | `CtrlProcPersp` | `numeric(1,0)` | sim | - |  |
| 34 | `CtrlProcCanal` | `numeric(1,0)` | sim | - |  |
| 35 | `CtrlRespProcesso` | `numeric(1,0)` | sim | - |  |
| 36 | `TpoMaxReagenda` | `decimal(10,4)` | sim | - |  |
| 37 | `QtdMaxUso` | `decimal(2,0)` | sim | - |  |

---

### IV_Resultado

`classe: nucleo` · `63 colunas` · `4.201 linhas (snapshot 03/06/2026)` · `PK: Resultado`

**Funcao:** O motor BPM declarativo: 980 ações (378 ativas), 4.208 resultados, 5.958 regras de geração automática de agenda e 2.160 mapeamentos resultado→fase/status. Nenhum código: tudo é catálogo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Pcte` | `varchar(4)` | sim | - |  |
| 3 | `Acao` | `decimal(6,0)` | sim | - | acao (`IV_Acao.Acao`) |
| 4 | `DescReduzida` | `varchar(20)` | **nao** | - |  |
| 5 | `Descricao` | `varchar(40)` | **nao** | - | descricao do registro |
| 6 | `CMPLTTXTPADRAO` | `varchar(250)` | sim | - |  |
| 7 | `ADVERTENCIA` | `varchar(200)` | sim | - |  |
| 8 | `REAGENDARPARA` | `numeric(6,0)` | sim | - |  |
| 9 | `Seqformulario` | `numeric(18,0)` | sim | - | formulario (`IV_Formulario.SeqFormulario`) |
| 10 | `Ordem` | `decimal(3,0)` | sim | - |  |
| 11 | `DESCVALOR` | `varchar(20)` | sim | - |  |
| 12 | `DESCQTDE` | `varchar(20)` | sim | - |  |
| 13 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 14 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 15 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 16 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 17 | `QtdeMaxPorAcao` | `decimal(2,0)` | sim | - |  |
| 18 | `ASSUNTOCMPL` | `varchar(40)` | sim | - |  |
| 19 | `ASSUNTOEMAIL` | `varchar(40)` | sim | - |  |
| 20 | `PubWeb` | `numeric(1,0)` | sim | - |  |
| 21 | `HORAPRORRLIMITE` | `numeric(6,2)` | sim | - |  |
| 22 | `HORAPRORRAGDA` | `numeric(6,2)` | sim | - |  |
| 23 | `HORAREAGENDA` | `numeric(6,2)` | sim | - |  |
| 24 | `CanalPadrao` | `varchar(20)` | sim | - |  |
| 25 | `SOLICSTATDESC` | `numeric(1,0)` | sim | - |  |
| 26 | `VidaUtil` | `numeric(4,0)` | sim | - |  |
| 27 | `MINUTILREAGENDA` | `numeric(8,2)` | sim | - |  |
| 28 | `CTRLINTERATIVO` | `numeric(1,0)` | sim | - |  |
| 29 | `CTRLCONCLUSAO` | `numeric(1,0)` | sim | - |  |
| 30 | `CTRLDETALHE` | `numeric(1,0)` | sim | - |  |
| 31 | `CTRLFORMACONTATO` | `numeric(1,0)` | sim | - |  |
| 32 | `CTRLREAGENDA` | `numeric(1,0)` | sim | - |  |
| 33 | `CTRLCONFIRMAREAG` | `numeric(1,0)` | sim | - |  |
| 34 | `CTRLREAGENDASILO` | `numeric(1,0)` | sim | - |  |
| 35 | `CTRLMUDARATDREAG` | `numeric(1,0)` | sim | - |  |
| 36 | `CTRLFORMULARIO` | `numeric(1,0)` | sim | - |  |
| 37 | `CTRLDURACAO` | `numeric(1,0)` | sim | - |  |
| 38 | `CTRLQTDE` | `numeric(1,0)` | sim | - |  |
| 39 | `CTRLVALOR` | `numeric(1,0)` | sim | - |  |
| 40 | `CTRLPRODUTIVO` | `numeric(1,0)` | sim | - |  |
| 41 | `CTRLPRODUTO` | `numeric(1,0)` | sim | - |  |
| 42 | `CTRLVENDEDOR` | `numeric(1,0)` | sim | - |  |
| 43 | `CTRLMOTIVO` | `numeric(1,0)` | sim | - |  |
| 44 | `CTRLDEPARTAMENTO` | `numeric(1,0)` | sim | - |  |
| 45 | `CTRLCAMPANHA` | `numeric(1,0)` | sim | - |  |
| 46 | `CTRLCONTATOPF` | `numeric(1,0)` | sim | - |  |
| 47 | `CTRLCONTATOPJ` | `numeric(1,0)` | sim | - |  |
| 48 | `CTRLCLIENTEATIVO` | `numeric(1,0)` | sim | - |  |
| 49 | `CTRLDETALHEPROC` | `numeric(1,0)` | sim | - |  |
| 50 | `CTRLDESCRSTATUSPROC` | `numeric(1,0)` | sim | - |  |
| 51 | `CTRLSTATUSPROC` | `numeric(1,0)` | sim | - |  |
| 52 | `CTRLRESUMOPROC` | `numeric(1,0)` | sim | - |  |
| 53 | `CTRLPERSPECTIVAPROC` | `numeric(1,0)` | sim | - |  |
| 54 | `CTRLVALORPROC` | `numeric(1,0)` | sim | - |  |
| 55 | `CTRLDTAENCERRAPROC` | `numeric(1,0)` | sim | - |  |
| 56 | `CTRLPROJETO` | `numeric(1,0)` | sim | - |  |
| 57 | `CTRLIMAGEM` | `numeric(1,0)` | sim | - |  |
| 58 | `CTRLCTIDISPONIVEL` | `numeric(1,0)` | sim | - |  |
| 59 | `CTRLCOMPLEMENTO` | `numeric(1,0)` | sim | - |  |
| 60 | `CTRLCMPLSQL` | `numeric(1,0)` | sim | - |  |
| 61 | `CTRLAGENDACONF` | `numeric(1,0)` | sim | - |  |
| 62 | `CTRLRESPPROC` | `numeric(1,0)` | sim | - |  |
| 63 | `EMUSO` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |

**Referenciada por:** `IVS_DeptoRes.Resultado`, `IV_AcaoAuto.Resultado`, `IV_AcaoRem.Resultado`, `IV_CobrCrit.Resultado`, `IV_Conhecimento.Resultado`, `IV_Historico.Resultado`, `IV_ProcResultado.Resultado`, `IV_ResClasse.Resultado`, `IV_ResEvtOut.Resultado`, `IV_ResMsgPapel.Resultado`, `IV_ResParam.Resultado`, `IV_ResVinc.Resultado`, `IV_ResultadoCmpl.Resultado`, `IV_ResultadoInstr.Resultado`, `IV_ResultadoReq.ResDep`, `IV_ResultadoWeb.Resultado`

---

### IV_ResultadoCmpl

`classe: nucleo` · `2 colunas` · `3.110 linhas (snapshot 03/06/2026)` · `PK: Resultado, ResultadoCmpl`

**Funcao:** complementos válidos. ⚠️ não é validado na escrita: o conector do RD Station grava 'TALLOS Chat' direto em IV_Historico.ResultadoCmpl sem passar por aqui (defeito 2.10). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 2 | `ResultadoCmpl` | `varchar(150)` | **nao** | - | **PK**; resultado complementar (texto livre; ver defeito 2.10) |

---

### IV_ResultadoInstr

`classe: nucleo` · `2 colunas` · `98 linhas (snapshot 03/06/2026)` · `PK: Resultado`

**Funcao:** instrução em text para o usuário. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 2 | `Instrucao` | `text(2147483647)` | sim | - |  |

---

### IV_ResultadoReq

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Resultado, CodProcesso, Acao, ResDep`

**Funcao:** condições em IV_AcaoAutoCtrl (vazia) e IV_ResultadoReq (vazia) (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; resultado (`IV_Resultado.Resultado`) |
| 2 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_ProcAcao.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 3 | `Acao` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_ProcAcao.Acao`; acao (`IV_Acao.Acao`) |
| 4 | `ResDep` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado` |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_ResultadoWeb

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: EventoWeb`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de resultado (desfecho de um andamento), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `EventoWeb` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 3 | `Resultado` | `decimal(6,0)` | **nao** | - | FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 4 | `DescricaoWeb` | `varchar(60)` | sim | - |  |
| 5 | `GrupoWebCRM` | `varchar(30)` | sim | - |  |
| 6 | `WebVars` | `text(2147483647)` | sim | - |  |
| 7 | `ExigeAnexo` | `numeric(1,0)` | sim | - |  |
| 8 | `InstrucaoWeb` | `varchar(250)` | sim | - |  |
| 9 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 10 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_ResVinc

`classe: nucleo` · `8 colunas` · `959 linhas (snapshot 03/06/2026)` · `PK: Resultado, SeqVinc`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 2 | `SeqVinc` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Vinculo` | `varchar(5)` | **nao** | - |  |
| 4 | `CodProcesso` | `decimal(4,0)` | **nao** | - | TIPO de fluxo (41/50) - nao e o numero do processo |
| 5 | `NroVinc` | `numeric(18,0)` | sim | - |  |
| 6 | `CodVinc` | `varchar(30)` | sim | - |  |
| 7 | `GrupoVinc` | `varchar(10)` | sim | - |  |
| 8 | `Exigido` | `numeric(1,0)` | sim | - |  |

---

### IV_RetProcRegra

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRetProcRegra`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRetProcRegra` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqTxtPadrao` | `numeric(18,0)` | sim | - |  |
| 3 | `OrdemProc` | `numeric(2,0)` | sim | - |  |
| 4 | `TAGS` | `varchar(40)` | sim | - |  |
| 5 | `Resultado` | `numeric(6,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |
| 6 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |
| 7 | `Detalhe` | `varchar(100)` | sim | - |  |

---

### IV_SegPerfil

`classe: vazia` · `27 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPerfil`

**Funcao:** Permissões operacionais por usuário (939 linhas × 51 colunas de flags, 370 combinações distintas) e o modelo de PERFIL que resolveria isso — nunca ativado (0 linhas em ambas). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPerfil` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `EVendedor` | `char(1)` | sim | - |  |
| 3 | `IncHistorico` | `char(1)` | sim | - |  |
| 4 | `IncHistRetr` | `char(1)` | sim | - |  |
| 5 | `QtdDiasRetr` | `decimal(4,0)` | sim | - |  |
| 6 | `IncHistAgConcl` | `char(1)` | sim | - |  |
| 7 | `AltHistorico` | `char(1)` | sim | - |  |
| 8 | `ExcHistorico` | `char(1)` | sim | - |  |
| 9 | `IncAgenda` | `char(1)` | sim | - |  |
| 10 | `AltAgenda` | `char(1)` | sim | - |  |
| 11 | `ExcAgenda` | `char(1)` | sim | - |  |
| 12 | `VerAgenda` | `char(1)` | sim | - |  |
| 13 | `ConcAgenda` | `char(1)` | sim | - |  |
| 14 | `AltOperAgenda` | `char(1)` | sim | - |  |
| 15 | `AltOperAgdReag` | `char(1)` | sim | - |  |
| 16 | `ConcAgeVendor` | `char(1)` | sim | - |  |
| 17 | `ReativarAgenda` | `char(1)` | sim | - |  |
| 18 | `AnalisarHistorico` | `char(1)` | sim | - |  |
| 19 | `AbreAnalise` | `char(1)` | sim | - |  |
| 20 | `Alteragrupo` | `char(1)` | sim | - |  |
| 21 | `AlteraCodImport` | `char(1)` | sim | - |  |
| 22 | `AlteraRegiao` | `char(1)` | sim | - |  |
| 23 | `AlteraVendedor` | `char(1)` | sim | - |  |
| 24 | `AlteraClienteAtivo` | `char(1)` | sim | - |  |
| 25 | `AlteraStatus` | `char(1)` | sim | - |  |
| 26 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 27 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IV_Atendente.SeqPerfil`

---

### IV_STATUS_DEPTO

`classe: isolada` · `5 colunas` · `473.844 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** - IV_STATUS_DEPTO (473.844 linhas) e IV_Distribui: classifiquei pela estrutura mas não confirmei o papel operacional exato de nenhuma das duas com dado real. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPESSOA` | `numeric(18,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `DEPTO` | `varchar(20)` | sim | - |  |
| 3 | `STATUS` | `varchar(10)` | sim | - | status - validar dominio real por tabela |
| 4 | `ULTCOMPRA` | `datetime` | sim | - |  |
| 5 | `ULTINTEGRACAO` | `datetime` | sim | - |  |

---

### IV_TAGCAD

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Lista do que existe no produto e está VAZIO na Tracbel (verificado por COUNT): IV_TAGCAD (0 — TAG, COR, CORNUM, QTDUSOHST, DTAULTUSOHST, QTDUSOPRC, DTAULTUSOPRC) com IV_PROCTAG e IV_HISTORICOTAG e quatro itens de política dedicados (ATD_TAG_01_HSTI, ATD_TAG_01_PRCI, ATD_TAG_02_HSTE, ATD_TAG_02_PRCE); (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `TAG` | `varchar(20)` | sim | - |  |
| 2 | `COR` | `varchar(6)` | sim | - |  |
| 3 | `DTAGERACAO` | `datetime` | sim | - |  |
| 4 | `USUGERACAO` | `varchar(20)` | sim | - |  |
| 5 | `CORNUM` | `numeric(18,0)` | sim | - |  |
| 6 | `QTDUSOHST` | `numeric(18,0)` | sim | - |  |
| 7 | `DTAULTUSOHST` | `datetime` | sim | - |  |
| 8 | `QTDUSOPRC` | `numeric(18,0)` | sim | - |  |
| 9 | `DTAULTUSOPRC` | `datetime` | sim | - |  |

---

### IV_TIPOCONTEUDO

`classe: catalogo` · `3 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQCONTEUDO`

**Funcao:** _(inferido)_ Pelo nome, e um catalogo de tipos, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONTEUDO` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `TIPOCAMPANHA` | `varchar(20)` | sim | - |  |
| 3 | `DESCRICAO` | `varchar(40)` | sim | - | descricao do registro |

**Referenciada por:** `IV_OPTEMAIL.SEQCONTEUDO`, `IV_OPTFONE.SEQCONTEUDO`

---

### IV_TpPgto

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTpPgto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de forma/condicao de pagamento, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTpPgto` | `decimal(2,0)` | **nao** | - | **PK** |
| 2 | `TipoPgto` | `varchar(15)` | sim | - |  |
| 3 | `VistaPrazo` | `char(1)` | sim | - |  |
| 4 | `TpCadastro` | `varchar(2)` | sim | - |  |
| 5 | `LinkExterno` | `varchar(20)` | sim | - |  |

---

### IV_TxtPadConta

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: NroEmpresa, SeqTxtPadrao, SeqParamLista`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de plano de contas, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 2 | `SeqTxtPadrao` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `SeqParamLista` | `numeric(18,0)` | **nao** | - | **PK** |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_TxtPadrao

`classe: catalogo` · `11 colunas` · `212 linhas (snapshot 03/06/2026)` · `PK: SeqTxtPadrao`

**Funcao:** Já os TEXTOS PADRÃO são muito usados: IV_TxtPadrao = 212 (Aplicacao, Descricao, AssPadrao, Texto, HoraAGUARDARET, IDEXTERNO), telas IVS1TXP00/IVS1TXP10 'Textos padrões (Templates)' e IVS1TXP02 (conta SMS associada), IV_TxtPadraoUso, IV_TxtPadConta, parâmetro UsaTxtPadrao; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTxtPadrao` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Aplicacao` | `varchar(20)` | sim | - |  |
| 3 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 4 | `AssPadrao` | `varchar(150)` | sim | - |  |
| 5 | `Texto` | `text(2147483647)` | sim | - |  |
| 6 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 7 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 8 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 9 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 10 | `HoraAGUARDARET` | `numeric(6,1)` | sim | - |  |
| 11 | `IDEXTERNO` | `varchar(300)` | sim | - |  |

**Referenciada por:** `IV_AcaoCtrl.SeqTxtPadrao`, `IV_AgendaCtrl.SEQTXTPADRAO`, `IV_CbrCriterio.SeqTxt1CobSMS`, `IV_CbrCriterio.SeqTxt1CobeMail`, `IV_CbrCriterio.SeqTxt2CobSMS`, `IV_CbrCriterio.SeqTxt2CobeMail`, `IV_ResMsgPapel.SeqTxtPadrao`, `IV_TxtPadraoUso.SeqTxtPadrao`, `IV_WHATSAPP.SEQTXTPADRAO`

---

### IV_TxtPadraoUso

`classe: nucleo` · `4 colunas` · `5 linhas (snapshot 03/06/2026)` · `PK: CodAplicacao, ChaveAplicacao, SeqTxtPadrao`

**Funcao:** _(inferido)_ Pelo nome, e um texto/modelo padrao, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodAplicacao` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `ChaveAplicacao` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `SeqTxtPadrao` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 4 | `Direto` | `numeric(1,0)` | sim | - |  |

---

### IV_Unidade

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de unidade de medida, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUnidade` | `numeric(4,0)` | **nao** | - |  |
| 2 | `Gerente` | `varchar(18)` | sim | - |  |
| 3 | `NroEmpresa` | `decimal(3,0)` | sim | - | multiempresa - filial/empresa |
| 4 | `Unidade` | `varchar(15)` | sim | - |  |
| 5 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 6 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 7 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 8 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 9 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 10 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---
