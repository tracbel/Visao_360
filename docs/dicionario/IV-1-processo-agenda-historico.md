# IV-1 - Nucleo BPM: processo, agenda e historico

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**37 tabelas · 393 colunas · 19.567.539 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IV_AgdLink`](#iv_agdlink) | nucleo | 11 | 4.534 |
| [`IV_AGDPLANACAO`](#iv_agdplanacao) | vazia | 7 | 0 |
| [`IV_AGDPLANTRG`](#iv_agdplantrg) | vazia | 42 | 0 |
| [`IV_AgdRec`](#iv_agdrec) | vazia | 9 | 0 |
| [`IV_AgdUsr`](#iv_agdusr) | vazia | 4 | 0 |
| [`IV_Agenda`](#iv_agenda) | nucleo | 40 | 931.989 |
| [`IV_AGENDACMPL`](#iv_agendacmpl) | vazia | 4 | 0 |
| [`IV_AgendaCtrl`](#iv_agendactrl) | nucleo | 6 | 963 |
| [`IV_AGENDAITEM`](#iv_agendaitem) | vazia | 7 | 0 |
| [`IV_AgendaLog`](#iv_agendalog) | isolada | 9 | 11.049.475 |
| [`IV_Agenda_bkp20250717`](#iv_agenda_bkp20250717) | lixo/backup | 40 | 1 |
| [`IV_AtdBloq`](#iv_atdbloq) | vazia | 10 | 0 |
| [`IV_AtivAgenda`](#iv_ativagenda) | vazia | 6 | 0 |
| [`IV_AtivProc`](#iv_ativproc) | vazia | 7 | 0 |
| [`IV_Ciencia`](#iv_ciencia) | isolada | 5 | 173.477 |
| [`IV_Distribui`](#iv_distribui) | nucleo | 3 | 609 |
| [`IV_HistInfo`](#iv_histinfo) | isolada | 2 | 17.250 |
| [`IV_HistLink`](#iv_histlink) | isolada | 5 | 645.850 |
| [`IV_Historico`](#iv_historico) | nucleo | 29 | 2.436.127 |
| [`IV_HistoricoNota`](#iv_historiconota) | isolada | 2 | 94.315 |
| [`IV_HISTORICOTAG`](#iv_historicotag) | vazia | 4 | 0 |
| [`IV_Interacao`](#iv_interacao) | nucleo | 4 | 720.525 |
| [`IV_ProcAcao`](#iv_procacao) | catalogo | 3 | 738 |
| [`IV_ProcAtiv`](#iv_procativ) | vazia | 6 | 0 |
| [`IV_ProcComent`](#iv_proccoment) | vazia | 3 | 0 |
| [`IV_ProcDado`](#iv_procdado) | nucleo | 24 | 1.516.214 |
| [`IV_ProcDocto`](#iv_procdocto) | isolada | 5 | 73.646 |
| [`IV_Processo`](#iv_processo) | nucleo | 22 | 1.174.932 |
| [`IV_Processo_bkp20250717`](#iv_processo_bkp20250717) | lixo/backup | 22 | 1 |
| [`IV_ProcLink`](#iv_proclink) | isolada | 7 | 602.150 |
| [`IV_PROCPESLINK`](#iv_procpeslink) | vazia | 2 | 0 |
| [`IV_ProcProduto`](#iv_procproduto) | isolada | 18 | 62.578 |
| [`IV_ProcProjeto`](#iv_procprojeto) | vazia | 4 | 0 |
| [`IV_ProcRef`](#iv_procref) | nucleo | 5 | 61.975 |
| [`IV_ProcRelacao`](#iv_procrelacao) | vazia | 6 | 0 |
| [`IV_PROCTAG`](#iv_proctag) | vazia | 4 | 0 |
| [`IV_ProcVinc`](#iv_procvinc) | isolada | 6 | 190 |

---

### IV_AgdLink

`classe: nucleo` · `11 colunas` · `4.534 linhas (snapshot 03/06/2026)` · `PK: SeqAgenda`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para relacionada a agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com agenda (`SeqAgenda`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 2 | `LinkOrigem` | `varchar(20)` | **nao** | - |  |
| 3 | `LinkTipo` | `varchar(10)` | **nao** | - |  |
| 4 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 5 | `LinkStr` | `varchar(40)` | sim | - |  |
| 6 | `CtrlN1` | `numeric(18,0)` | sim | - |  |
| 7 | `CtrlN2` | `numeric(18,0)` | sim | - |  |
| 8 | `CtrlStr1` | `varchar(40)` | sim | - |  |
| 9 | `CtrlDta1` | `datetime` | sim | - |  |
| 10 | `UltAtualizacao` | `datetime` | sim | - |  |
| 11 | `OBS` | `varchar(50)` | sim | - | texto livre |

---

### IV_AGDPLANACAO

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQAGDPLANACAO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQAGDPLANACAO` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `SEQAGDPLANTRG` | `numeric(8,0)` | **nao** | - | FK -> `IV_AGDPLANTRG.SEQAGDPLANTRG` |
| 3 | `ACAO` | `numeric(6,0)` | **nao** | - | acao (`IV_Acao.Acao`) |
| 4 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 5 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 6 | `DETALHEAGD` | `varchar(250)` | sim | - |  |
| 7 | `INDGRUPOTODO` | `numeric(1,0)` | **nao** | - |  |

---

### IV_AGDPLANTRG

`classe: vazia` · `42 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQAGDPLANTRG`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQAGDPLANTRG` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `EMUSO` | `numeric(1,0)` | **nao** | - | flag de registro/regra ativa (0 = desligada) |
| 3 | `DESCRICAO` | `varchar(80)` | **nao** | - | descricao do registro |
| 4 | `RESUMOREGRA` | `varchar(250)` | sim | - |  |
| 5 | `QTDEEXECUCOES` | `numeric(4,0)` | sim | - |  |
| 6 | `QTDDIASANTECIPADO` | `numeric(2,0)` | **nao** | - |  |
| 7 | `DTALIMITE` | `datetime` | sim | - |  |
| 8 | `HORAINICIAL` | `numeric(4,0)` | **nao** | - |  |
| 9 | `DURACAO` | `numeric(4,0)` | sim | - |  |
| 10 | `PRAZOMAXEXEC` | `numeric(4,0)` | **nao** | - |  |
| 11 | `TIPODIA` | `varchar(3)` | **nao** | - |  |
| 12 | `DIAESPECIAL` | `varchar(3)` | sim | - |  |
| 13 | `INDCOMPROMISSO` | `numeric(1,0)` | **nao** | - |  |
| 14 | `PROXIMODIA` | `numeric(1,0)` | sim | - |  |
| 15 | `PRIORIDADE` | `numeric(1,0)` | sim | - |  |
| 16 | `INDSABADOUTIL` | `numeric(1,0)` | **nao** | - |  |
| 17 | `INDDOMINGOUTIL` | `numeric(1,0)` | **nao** | - |  |
| 18 | `DIAMES01` | `numeric(2,0)` | sim | - |  |
| 19 | `DIAMES02` | `numeric(2,0)` | sim | - |  |
| 20 | `DIAMES03` | `numeric(2,0)` | sim | - |  |
| 21 | `DIAMES04` | `numeric(2,0)` | sim | - |  |
| 22 | `DIAMES05` | `numeric(2,0)` | sim | - |  |
| 23 | `DOMINGO` | `numeric(1,0)` | sim | - |  |
| 24 | `SEGUNDA` | `numeric(1,0)` | sim | - |  |
| 25 | `TERCA` | `numeric(1,0)` | sim | - |  |
| 26 | `QUARTA` | `numeric(1,0)` | sim | - |  |
| 27 | `QUINTA` | `numeric(1,0)` | sim | - |  |
| 28 | `SEXTA` | `numeric(1,0)` | sim | - |  |
| 29 | `SABADO` | `numeric(1,0)` | sim | - |  |
| 30 | `MJAN` | `numeric(1,0)` | sim | - |  |
| 31 | `MFEV` | `numeric(1,0)` | sim | - |  |
| 32 | `MMAR` | `numeric(1,0)` | sim | - |  |
| 33 | `MABR` | `numeric(1,0)` | sim | - |  |
| 34 | `MMAI` | `numeric(1,0)` | sim | - |  |
| 35 | `MJUN` | `numeric(1,0)` | sim | - |  |
| 36 | `MJUL` | `numeric(1,0)` | sim | - |  |
| 37 | `MAGO` | `numeric(1,0)` | sim | - |  |
| 38 | `MSET` | `numeric(1,0)` | sim | - |  |
| 39 | `MOUT` | `numeric(1,0)` | sim | - |  |
| 40 | `MNOV` | `numeric(1,0)` | sim | - |  |
| 41 | `MDEZ` | `numeric(1,0)` | sim | - |  |
| 42 | `ULTDTAGERADA` | `datetime` | **nao** | - |  |

**Referenciada por:** `IV_AGDPLANACAO.SEQAGDPLANTRG`

---

### IV_AgdRec

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAgenda, SeqAgdRec`

**Funcao:** COUNT IV_Interacao=720.525 e IV_AgdRec=0 em produção; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 2 | `SeqAgdRec` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `TipoAgdRec` | `char(1)` | sim | - |  |
| 4 | `SeqRecurso` | `decimal(6,0)` | sim | - | FK -> `IV_Recurso.SeqRecurso` |
| 5 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 6 | `SeqUsuario` | `numeric(18,0)` | sim | - | FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 7 | `DtaAgenda` | `datetime` | **nao** | - |  |
| 8 | `DtaAgendaFinal` | `datetime` | sim | - |  |
| 9 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |

---

### IV_AgdUsr

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAgenda, SeqUsuario`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com agenda (`SeqAgenda`), vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `DtaUltGeracao` | `datetime` | sim | - |  |
| 4 | `DtaLeitura` | `datetime` | sim | - |  |

---

### IV_Agenda

`classe: nucleo` · `40 colunas` · `931.989 linhas (snapshot 03/06/2026)` · `PK: SeqAgenda`

**Funcao:** A TAREFA aberta/atribuída. 40 col, 931.989 linhas (930.126 realizadas / 35.724 pendentes). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `Contato` | `varchar(20)` | sim | - |  |
| 5 | `TipoAgendamento` | `char(1)` | sim | - |  |
| 6 | `TarefaCompromisso` | `char(1)` | sim | - |  |
| 7 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 8 | `CodProcesso` | `decimal(4,0)` | sim | - | TIPO de fluxo (41/50) - nao e o numero do processo |
| 9 | `Acao` | `decimal(6,0)` | sim | - | FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 10 | `Assunto` | `varchar(50)` | sim | - |  |
| 11 | `AssuntoCmpl` | `varchar(40)` | sim | - |  |
| 12 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 13 | `Classe` | `char(1)` | **nao** | - |  |
| 14 | `DtaAgenda` | `datetime` | **nao** | - |  |
| 15 | `DtaAgendaFinal` | `datetime` | sim | - |  |
| 16 | `DtaLimiteExecucao` | `datetime` | sim | - |  |
| 17 | `DtaAviso` | `datetime` | sim | - |  |
| 18 | `DtaAgendaOriginal` | `datetime` | sim | - |  |
| 19 | `DtaPrimContato` | `datetime` | sim | - |  |
| 20 | `Prioridade` | `decimal(1,0)` | **nao** | - |  |
| 21 | `Detalhe` | `varchar(250)` | sim | - |  |
| 22 | `TipoAcesso` | `char(1)` | sim | - |  |
| 23 | `HistoricoOrigem` | `numeric(18,0)` | sim | - |  |
| 24 | `UsuGerouAcao` | `varchar(20)` | sim | - |  |
| 25 | `DtaGeracao` | `datetime` | sim | - |  |
| 26 | `DtaGeracaoOrig` | `datetime` | sim | - |  |
| 27 | `TempoEstimado` | `decimal(6,0)` | sim | - |  |
| 28 | `DtaLeitura` | `datetime` | sim | - |  |
| 29 | `Realizada` | `char(1)` | sim | - |  |
| 30 | `DtaRealizacao` | `datetime` | sim | - | data de realizacao |
| 31 | `UltResultado` | `numeric(18,0)` | sim | - |  |
| 32 | `UltHistorico` | `numeric(18,0)` | sim | - |  |
| 33 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |
| 34 | `DtaUltResultado` | `datetime` | sim | - |  |
| 35 | `Departamento` | `varchar(12)` | sim | - | departamento |
| 36 | `HUDecorrido` | `decimal(8,2)` | sim | - |  |
| 37 | `Vendedor` | `varchar(20)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 38 | `RevisarHistorico` | `char(1)` | sim | - |  |
| 39 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 40 | `QtdLibBloq` | `decimal(4,0)` | sim | - |  |

**Referenciada por:** `IVC_ATENDENTE.SEQAGENDA`, `IV_AGENDACMPL.SEQAGENDA`, `IV_AgdLink.SeqAgenda`, `IV_AgdRec.SeqAgenda`, `IV_AgdUsr.SeqAgenda`, `IV_AgendaCtrl.SeqAgenda`, `IV_AtivAgenda.SeqAgenda`, `IV_CbrCobranca.SeqAgenda`, `IV_CobrCritAgd.SeqAgenda`, `IV_URADISPARO.SEQAGENDA`

---

### IV_AGENDACMPL

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQAGENDA`

**Funcao:** a coluna IV_AGENDACMPL.SILOORIGEM registra de qual silo veio a agenda. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQAGENDA` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 2 | `DDDPREFERENCIAL` | `varchar(5)` | sim | - |  |
| 3 | `FONEPREFERENCIAL` | `numeric(12,0)` | sim | - |  |
| 4 | `SILOORIGEM` | `numeric(8,0)` | sim | - |  |

---

### IV_AgendaCtrl

`classe: nucleo` · `6 colunas` · `963 linhas (snapshot 03/06/2026)` · `PK: SeqAgenda, SeqUsuario, Base, Atitude`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de controle/condicoes de uso relacionada a agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com agenda (`SeqAgenda`), vinculo com usuario (`SeqUsuario`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `Base` | `char(1)` | **nao** | - | **PK** |
| 4 | `Atitude` | `varchar(10)` | **nao** | - | **PK** |
| 5 | `SeqAgendaGerada` | `numeric(18,0)` | sim | - |  |
| 6 | `SEQTXTPADRAO` | `numeric(18,0)` | sim | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |

---

### IV_AGENDAITEM

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQAGDITEM`

**Funcao:** (2) CHECKLIST dentro da tarefa (IV_AGENDAITEM) — barato e muito pedido em campo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQAGDITEM` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQAGENDA` | `numeric(18,0)` | **nao** | - | agenda (`IV_Agenda.SeqAgenda`) |
| 3 | `ORDEM` | `numeric(4,0)` | **nao** | - |  |
| 4 | `ITEM` | `varchar(250)` | sim | - |  |
| 5 | `STATUS` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 6 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_AgendaLog

`classe: isolada` · `9 colunas` · `11.049.475 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria da agenda — a maior tabela do módulo. 9 col, 11.049.475 linhas (46% de todas as linhas IV_). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Kn1` | `numeric(18,0)` | **nao** | - |  |
| 2 | `Tb` | `varchar(12)` | sim | - |  |
| 3 | `Kn2` | `numeric(18,0)` | **nao** | - |  |
| 4 | `Ks` | `varchar(40)` | **nao** | - |  |
| 5 | `DtaLog` | `datetime` | **nao** | - |  |
| 6 | `Usr` | `varchar(20)` | sim | - |  |
| 7 | `CodApl` | `varchar(30)` | sim | - |  |
| 8 | `Obs` | `varchar(1000)` | sim | - | texto livre |
| 9 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |

---

### IV_Agenda_bkp20250717

`classe: lixo/backup` · `40 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com processo (`Processo`), vinculo com agenda (`SeqAgenda`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (40 colunas) para manter o documento legivel._

---

### IV_AtdBloq

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario, TpBloq, DtaBloq`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de atendente, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `TpBloq` | `varchar(6)` | **nao** | - | **PK** |
| 3 | `DtaBloq` | `datetime` | **nao** | - | **PK** |
| 4 | `DtaBloqueio` | `datetime` | sim | - |  |
| 5 | `CausaBloqueio` | `varchar(20)` | sim | - |  |
| 6 | `ObsBloq` | `varchar(250)` | sim | - |  |
| 7 | `DtaLiberacao` | `datetime` | sim | - |  |
| 8 | `SeqUsuLiberacao` | `decimal(8,0)` | sim | - |  |
| 9 | `UsuLiberacao` | `varchar(20)` | sim | - |  |
| 10 | `ObsDesbloq` | `varchar(250)` | sim | - |  |

---

### IV_AtivAgenda

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPlanoAtiv, SeqAgenda`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com agenda (`SeqAgenda`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlanoAtiv` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Ativ.SeqPlanoAtiv` |
| 2 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 3 | `DtaDeOriginal` | `datetime` | sim | - |  |
| 4 | `DtaAOriginal` | `datetime` | sim | - |  |
| 5 | `TempoEstimado` | `numeric(6,0)` | sim | - |  |
| 6 | `StatReal` | `varchar(20)` | sim | - |  |

---

### IV_AtivProc

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPlanoAtiv, Processo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlanoAtiv` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Ativ.SeqPlanoAtiv` |
| 2 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 3 | `TempoEstimado` | `decimal(6,0)` | sim | - |  |
| 4 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 5 | `Realizada` | `numeric(1,0)` | sim | - |  |
| 6 | `StatReal` | `varchar(20)` | sim | - |  |
| 7 | `IndFatBonif` | `numeric(1,0)` | sim | - |  |

---

### IV_Ciencia

`classe: isolada` · `5 colunas` · `173.477 linhas (snapshot 03/06/2026)` · `PK: SeqHistorico, SeqCiencia`

**Funcao:** confirmação de leitura de um histórico por outro usuário (SeqHistorico, SeqCiencia, DtaLeitura, CodUsuario, Obs) — bom mecanismo de 'dar ciência' (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqHistorico` | `numeric(18,0)` | **nao** | - | **PK**; historico (`IV_Historico.SeqHistorico`) |
| 2 | `SeqCiencia` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `DtaLeitura` | `datetime` | sim | - |  |
| 4 | `CodUsuario` | `varchar(20)` | sim | - | login do usuario (varchar) |
| 5 | `Obs` | `varchar(200)` | sim | - | texto livre |

---

### IV_Distribui

`classe: nucleo` · `3 colunas` · `609 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario, Acao`

**Funcao:** Os outros três primitivos de automação: remoção de tarefa (266), escalação por e-mail (44), round-robin (609). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `Acao` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 3 | `UltimoUsuario` | `decimal(6,0)` | sim | - |  |

---

### IV_HistInfo

`classe: isolada` · `2 colunas` · `17.250 linhas (snapshot 03/06/2026)` · `PK: SeqHistorico`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de historico de interacoes, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com historico (`SeqHistorico`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqHistorico` | `numeric(18,0)` | **nao** | - | **PK**; historico (`IV_Historico.SeqHistorico`) |
| 2 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |

---

### IV_HistLink

`classe: isolada` · `5 colunas` · `645.850 linhas (snapshot 03/06/2026)` · `PK: SeqHistorico`

**Funcao:** Camada de identidade e reconciliação: 32.831 vínculos externos de pessoa, 602.150 de processo e 645.850 de histórico com sistemas de origem; 124.222 pares de pessoas candidatas a duplicidade. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqHistorico` | `numeric(18,0)` | **nao** | - | **PK**; historico (`IV_Historico.SeqHistorico`) |
| 2 | `LinkSerie` | `varchar(250)` | sim | - | chave de vinculo com documento do ERP |
| 3 | `LinkDocto` | `varchar(20)` | sim | - | chave de vinculo com documento do ERP |
| 4 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 5 | `LinkNroEmpresa` | `numeric(6,0)` | sim | - |  |

---

### IV_Historico

`classe: nucleo` · `29 colunas` · `2.436.127 linhas (snapshot 03/06/2026)` · `PK: SeqHistorico`

**Funcao:** O FATO imutável — cada andamento dado. 29 col, 2.436.127 linhas. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqHistorico` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Contato` | `varchar(20)` | sim | - |  |
| 4 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 5 | `SeqUsuario` | `int(10,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 6 | `CodUsuario` | `varchar(20)` | sim | - | login do usuario (varchar) |
| 7 | `AcaoGeradora` | `numeric(6,0)` | sim | - |  |
| 8 | `Departamento` | `varchar(18)` | sim | - | departamento |
| 9 | `Resultado` | `decimal(6,0)` | **nao** | - | FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 10 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |
| 11 | `AgendaOrigem` | `int(10,0)` | sim | - |  |
| 12 | `DtaRealizacao` | `datetime` | **nao** | - | data de realizacao |
| 13 | `Detalhe` | `varchar(4000)` | sim | - |  |
| 14 | `Natureza` | `char(1)` | sim | - |  |
| 15 | `Vendedor` | `varchar(20)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 16 | `UltAlteracao` | `datetime` | sim | - |  |
| 17 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 18 | `FormaPrimCont` | `varchar(30)` | sim | - |  |
| 19 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 20 | `CodProcesso` | `decimal(4,0)` | sim | - | TIPO de fluxo (41/50) - nao e o numero do processo |
| 21 | `Valor` | `decimal(15,2)` | sim | - |  |
| 22 | `Qtde` | `numeric(17,2)` | sim | - |  |
| 23 | `Duracao` | `decimal(4,0)` | sim | - |  |
| 24 | `TemCiencia` | `numeric(1,0)` | sim | - |  |
| 25 | `Latitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 26 | `Longitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 27 | `SEQPESSOACTTO` | `numeric(10,0)` | sim | - |  |
| 28 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 29 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |

**Referenciada por:** `IV_HISTORICOTAG.SEQHISTORICO`, `IV_Interacao.SeqHistorico`, `IV_PUSH.SEQHISTORICO`, `IV_eMail.SeqHistorico`

---

### IV_HistoricoNota

`classe: isolada` · `2 colunas` · `94.315 linhas (snapshot 03/06/2026)` · `PK: SeqHistorico`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de historico de interacoes, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com historico (`SeqHistorico`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqHistorico` | `numeric(18,0)` | **nao** | - | **PK**; historico (`IV_Historico.SeqHistorico`) |
| 2 | `Nota` | `text(2147483647)` | sim | - |  |

---

### IV_HISTORICOTAG

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQHISTORICO, TAG`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de historico de interacoes, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com historico (`SeqHistorico`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQHISTORICO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Historico.SeqHistorico`; historico (`IV_Historico.SeqHistorico`) |
| 2 | `TAG` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `DTAGERACAO` | `datetime` | sim | - |  |
| 4 | `USUGERACAO` | `varchar(20)` | sim | - |  |

---

### IV_Interacao

`classe: nucleo` · `4 colunas` · `720.525 linhas (snapshot 03/06/2026)` · `PK: SeqHistorico`

**Funcao:** / **Histórico** / `IV_Historico` (+ `IV_Interacao`) / `crm.Atividade` / Imutável, com natureza ativo/receptivo, duração de verdade, geo, e sem truncamento silencioso / (fonte: `16-vortice-ponta-a-ponta-ciclo-de-vida-pessoa-processo-agenda-historico.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqHistorico` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Historico.SeqHistorico`; historico (`IV_Historico.SeqHistorico`) |
| 2 | `DtaAgenda` | `datetime` | sim | - |  |
| 3 | `DtaAgendaFinal` | `datetime` | sim | - |  |
| 4 | `DtaLimiteExecucao` | `datetime` | sim | - |  |

---

### IV_ProcAcao

`classe: catalogo` · `3 colunas` · `738 linhas (snapshot 03/06/2026)` · `PK: CodProcesso, Acao`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `Acao` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 3 | `QtdeLimite` | `decimal(2,0)` | sim | - |  |

**Referenciada por:** `IV_ResultadoReq.Acao`, `IV_ResultadoReq.CodProcesso`

---

### IV_ProcAtiv

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Processo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_ProcDado.Processo`; numero do processo (`IV_Processo.Processo`) |
| 2 | `StatRealAtividade` | `varchar(20)` | sim | - |  |
| 3 | `StatRealProcesso` | `varchar(20)` | sim | - |  |
| 4 | `DtaFinalizacao` | `datetime` | sim | - |  |
| 5 | `IndFechado` | `numeric(1,0)` | sim | - |  |
| 6 | `Obs` | `varchar(250)` | sim | - | texto livre |

---

### IV_ProcComent

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Processo, SeqProcComent`

**Funcao:** GE_PessoaNota, IV_ProcComent e IV_CodProcComent (comentário por modelo de processo) existem mas estão zerados. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Processo.Processo`; numero do processo (`IV_Processo.Processo`) |
| 2 | `SeqProcComent` | `decimal(2,0)` | **nao** | - | **PK** |
| 3 | `Comentario` | `text(2147483647)` | sim | - |  |

---

### IV_ProcDado

`classe: nucleo` · `24 colunas` · `1.516.214 linhas (snapshot 03/06/2026)` · `PK: Processo`

**Funcao:** O CONTEXTO do processo (cliente, tipo de fluxo, vendedor, origem, linhagem). 24 col, 1.516.214 linhas (1.532.133 live). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 3 | `ProcessoPai` | `numeric(18,0)` | sim | - |  |
| 4 | `ProcessoDNA` | `numeric(18,0)` | sim | - |  |
| 5 | `CodProcesso` | `decimal(4,0)` | sim | - | TIPO de fluxo (41/50) - nao e o numero do processo |
| 6 | `PessoaDepto` | `varchar(20)` | sim | - |  |
| 7 | `SeqDepto` | `decimal(4,0)` | sim | - | FK -> `IVS_Depto.SeqDepto`; departamento |
| 8 | `SeqProjeto` | `numeric(18,0)` | sim | - | projeto (`IV_Projeto`) |
| 9 | `SeqPessoa` | `decimal(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 10 | `HistoricoOrigem` | `numeric(18,0)` | sim | - |  |
| 11 | `Vendedor` | `varchar(20)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 12 | `FormaPrimCont` | `varchar(12)` | sim | - |  |
| 13 | `AtivoReceptivo` | `char(1)` | sim | - |  |
| 14 | `Motivo` | `varchar(40)` | sim | - |  |
| 15 | `Campanha` | `varchar(60)` | sim | - |  |
| 16 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 17 | `UltResultado` | `int(10,0)` | sim | - |  |
| 18 | `UltHistorico` | `numeric(18,0)` | sim | - |  |
| 19 | `DtaUltResultado` | `datetime` | sim | - |  |
| 20 | `HUDecorrido` | `decimal(8,2)` | sim | - |  |
| 21 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |
| 22 | `DtaPrimResultado` | `datetime` | sim | - |  |
| 23 | `DtaGeracao` | `datetime` | sim | - |  |
| 24 | `UsuGeracao` | `varchar(20)` | sim | - |  |

**Referenciada por:** `GE_PROCESSOWEB.PROCESSO`, `IV_PROCPESLINK.PROCESSO`, `IV_PROCTAG.PROCESSO`, `IV_PUSH.PROCESSO`, `IV_ProcAtiv.Processo`, `IV_ProcRef.Processo`

---

### IV_ProcDocto

`classe: isolada` · `5 colunas` · `73.646 linhas (snapshot 03/06/2026)` · `PK: Processo, SeqDocto`

**Funcao:** Gestão documental com 107 tipos de documento politicamente configurados, 68.609 documentos, check-in/check-out, versionamento, validade e transporte por FTP; 76.603 vínculos com processo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; documento (`DMN_Doc.SeqDocto`) |
| 3 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 4 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 5 | `OBS` | `varchar(50)` | sim | - | texto livre |

---

### IV_Processo

`classe: nucleo` · `22 colunas` · `1.174.932 linhas (snapshot 03/06/2026)` · `PK: Processo`

**Funcao:** O CASO/oportunidade — guarda apenas o ESTADO. 22 colunas, 1.174.932 linhas (1.190.252 live). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Resumo` | `varchar(100)` | sim | - |  |
| 3 | `Descricao` | `varchar(4000)` | sim | - | descricao do registro |
| 4 | `UsuResponsavel` | `varchar(20)` | sim | - |  |
| 5 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 6 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 9 | `Realizado` | `decimal(1,0)` | sim | - |  |
| 10 | `DtaRealizacao` | `datetime` | sim | - | data de realizacao |
| 11 | `DtaFase` | `datetime` | sim | - |  |
| 12 | `DtaStatus` | `datetime` | sim | - |  |
| 13 | `Perspectiva` | `decimal(3,0)` | sim | - |  |
| 14 | `Fase` | `varchar(20)` | sim | - | fase do fluxo (`IV_ProcFase`) |
| 15 | `FaseOrdem` | `decimal(2,0)` | sim | - | fase do fluxo (`IV_ProcFase`) |
| 16 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 17 | `StatusDesc` | `varchar(150)` | sim | - |  |
| 18 | `DtaPrevConclusao` | `datetime` | sim | - |  |
| 19 | `Valor` | `decimal(15,2)` | sim | - |  |
| 20 | `Qtde` | `decimal(10,2)` | sim | - |  |
| 21 | `DtaPrevConcOrig` | `datetime` | sim | - |  |
| 22 | `Prioridade` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `IVM_ProcMat.Processo`, `IV_ProcComent.Processo`, `IV_ProcRelacao.Processo`

---

### IV_Processo_bkp20250717

`classe: lixo/backup` · `22 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (22 colunas) para manter o documento legivel._

---

### IV_ProcLink

`classe: isolada` · `7 colunas` · `602.150 linhas (snapshot 03/06/2026)` · `PK: SEQPROCLINK`

**Funcao:** Camada de identidade e reconciliação: 32.831 vínculos externos de pessoa, 602.150 de processo e 645.850 de histórico com sistemas de origem; 124.222 pares de pessoas candidatas a duplicidade. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `decimal(15,0)` | **nao** | - | numero do processo (`IV_Processo.Processo`) |
| 2 | `LinkSerie` | `varchar(250)` | sim | - | chave de vinculo com documento do ERP |
| 3 | `LinkDocto` | `varchar(20)` | sim | - | chave de vinculo com documento do ERP |
| 4 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 5 | `LinkNroEmpresa` | `numeric(6,0)` | sim | - |  |
| 6 | `SEQPROCLINK` | `numeric(18,0)` | **nao** | - | **PK** |
| 7 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_PROCPESLINK

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: PROCESSO, SEQPESSOALINK`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para relacionada a processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_ProcDado.Processo`; numero do processo (`IV_Processo.Processo`) |
| 2 | `SEQPESSOALINK` | `numeric(18,0)` | **nao** | - | **PK** |

---

### IV_ProcProduto

`classe: isolada` · `18 colunas` · `62.578 linhas (snapshot 03/06/2026)` · `PK: Processo, SeqProcProduto`

**Funcao:** EVITAR ATRIBUTO01..08 sem rótulo em IV_ProcProduto — é dado que ninguém consegue interpretar 3 anos depois. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `SeqProcProduto` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `SeqProduto` | `numeric(18,0)` | sim | - |  |
| 4 | `CodProduto` | `varchar(50)` | sim | - |  |
| 5 | `Valor` | `decimal(15,2)` | sim | - |  |
| 6 | `Qtde` | `decimal(10,2)` | sim | - |  |
| 7 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 8 | `TIPOPGTO` | `varchar(15)` | sim | - |  |
| 9 | `DESCONTO` | `numeric(14,2)` | sim | - |  |
| 10 | `STATUS` | `varchar(12)` | sim | - | status - validar dominio real por tabela |
| 11 | `ATRIBUTO01` | `varchar(40)` | sim | - |  |
| 12 | `ATRIBUTO02` | `varchar(40)` | sim | - |  |
| 13 | `ATRIBUTO03` | `varchar(40)` | sim | - |  |
| 14 | `ATRIBUTO04` | `varchar(40)` | sim | - |  |
| 15 | `ATRIBUTO05` | `varchar(40)` | sim | - |  |
| 16 | `ATRIBUTO06` | `varchar(40)` | sim | - |  |
| 17 | `ATRIBUTO07` | `varchar(40)` | sim | - |  |
| 18 | `ATRIBUTO08` | `varchar(40)` | sim | - |  |

---

### IV_ProcProjeto

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Processo, SeqProjeto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `SeqProjeto` | `numeric(18,0)` | **nao** | - | **PK**; projeto (`IV_Projeto`) |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_ProcRef

`classe: nucleo` · `5 colunas` · `61.975 linhas (snapshot 03/06/2026)` · `PK: Processo, Tipo, Referencia`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_ProcDado.Processo`; numero do processo (`IV_Processo.Processo`) |
| 2 | `Tipo` | `varchar(15)` | **nao** | - | **PK** |
| 3 | `Referencia` | `varchar(40)` | **nao** | - | **PK** |
| 4 | `DtaGeracao` | `datetime` | sim | - |  |
| 5 | `UsuGeracao` | `varchar(20)` | sim | - |  |

---

### IV_ProcRelacao

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: ProcBase, Processo, SeqTpRel`

**Funcao:** _(inferido)_ Pelo nome, e um relacionamento relacionada a processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ProcBase` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Processo.Processo`; numero do processo (`IV_Processo.Processo`) |
| 3 | `SeqTpRel` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_ProcTpRel.SeqTpRel` |
| 4 | `Obs` | `varchar(100)` | sim | - | texto livre |
| 5 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_PROCTAG

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: PROCESSO, TAG`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_ProcDado.Processo`; numero do processo (`IV_Processo.Processo`) |
| 2 | `TAG` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `DTAGERACAO` | `datetime` | sim | - |  |
| 4 | `USUGERACAO` | `varchar(20)` | sim | - |  |

---

### IV_ProcVinc

`classe: isolada` · `6 colunas` · `190 linhas (snapshot 03/06/2026)` · `PK: CodProcesso, SeqVinc`

**Funcao:** - IV_ProcVinc (CodProcesso, SeqVinc) + Vinculo varchar(5), Exigido, NroVinc, CodVinc. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodProcesso` | `decimal(4,0)` | **nao** | - | **PK**; TIPO de fluxo (41/50) - nao e o numero do processo |
| 2 | `SeqVinc` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Vinculo` | `varchar(5)` | **nao** | - |  |
| 4 | `Exigido` | `numeric(1,0)` | **nao** | - |  |
| 5 | `NroVinc` | `numeric(18,0)` | sim | - |  |
| 6 | `CodVinc` | `varchar(30)` | sim | - |  |

---
