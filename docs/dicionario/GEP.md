# GEP - Jobs, agendador, e-mail e sincronizacao

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**36 tabelas · 352 colunas · 5.985.987 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`GEP_CidadeEmprDest`](#gep_cidadeemprdest) | vazia | 3 | 0 |
| [`GEP_EMailSend`](#gep_emailsend) | vazia | 21 | 0 |
| [`GEP_EMAILSENDREL`](#gep_emailsendrel) | vazia | 4 | 0 |
| [`GEP_EMAILSENT`](#gep_emailsent) | nucleo | 20 | 571.866 |
| [`GEP_EMAILSENTREL`](#gep_emailsentrel) | vazia | 4 | 0 |
| [`gep_excfrota`](#gep_excfrota) | isolada | 7 | 1.180 |
| [`gep_excfrotarep`](#gep_excfrotarep) | isolada | 11 | 425 |
| [`GEP_EXCFROTASEQ`](#gep_excfrotaseq) | isolada | 1 | 1.128 |
| [`GEP_EXCFROTASEQDEL`](#gep_excfrotaseqdel) | isolada | 1 | 1.082 |
| [`gep_excseqcar`](#gep_excseqcar) | isolada | 1 | 3.143 |
| [`gep_excseqcarok`](#gep_excseqcarok) | isolada | 1 | 1.668 |
| [`GEP_Fila`](#gep_fila) | isolada | 18 | 4 |
| [`GEP_Import`](#gep_import) | isolada | 15 | 344 |
| [`GEP_ImportAprovacao`](#gep_importaprovacao) | isolada | 15 | 4.284 |
| [`GEP_ImportErro`](#gep_importerro) | vazia | 4 | 0 |
| [`GEP_ImportTry`](#gep_importtry) | isolada | 4 | 1.870 |
| [`GEP_Import_bkpjun`](#gep_import_bkpjun) | lixo/backup | 15 | 502.009 |
| [`GEP_IMPORT_GUI`](#gep_import_gui) | isolada | 15 | 52 |
| [`GEP_JOBAGD`](#gep_jobagd) | nucleo | 18 | 25 |
| [`GEP_JobAgdExecLog`](#gep_jobagdexeclog) | isolada | 13 | 981.319 |
| [`GEP_JOBCAD`](#gep_jobcad) | catalogo | 11 | 71 |
| [`GEP_JOBFILA`](#gep_jobfila) | nucleo | 16 | 23 |
| [`GEP_JOBFILALOG`](#gep_jobfilalog) | isolada | 5 | 14.038 |
| [`GEP_JobMonitor`](#gep_jobmonitor) | isolada | 10 | 1 |
| [`GEP_JOBNOTIFICAR`](#gep_jobnotificar) | vazia | 14 | 0 |
| [`GEP_Notificar`](#gep_notificar) | vazia | 8 | 0 |
| [`GEP_PABX`](#gep_pabx) | vazia | 6 | 0 |
| [`GEP_ParEnvia`](#gep_parenvia) | catalogo | 7 | 3 |
| [`GEP_ParRecebe`](#gep_parrecebe) | nucleo | 12 | 14 |
| [`GEP_ParRecEnvia`](#gep_parrecenvia) | vazia | 2 | 0 |
| [`GEP_ProcControle`](#gep_proccontrole) | vazia | 5 | 0 |
| [`GEP_Processo`](#gep_processo) | nucleo | 20 | 8 |
| [`GEP_ProcImport`](#gep_procimport) | vazia | 9 | 0 |
| [`GEP_SyncUsrSat`](#gep_syncusrsat) | isolada | 15 | 3.901.359 |
| [`Gep_UsrPabx`](#gep_usrpabx) | vazia | 5 | 0 |
| [`GEP_UsrSat`](#gep_usrsat) | nucleo | 16 | 71 |

---

### GEP_CidadeEmprDest

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Destino, NroEmpresa, SeqCidade`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEP` (jobs e integracao). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Destino` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `SeqCidade` | `numeric(6,0)` | **nao** | - | **PK** |

---

### GEP_EMailSend

`classe: vazia` · `21 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQEMAILSEND`

**Funcao:** Motor de notificação declarativo: 1.341 regras 'resultado X → mensagem para papel/usuário Y pelo canal Z com template W', com fila assíncrona (job EMAIL_SEND a cada 3 min) e 585.938 envios registrados. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Prioridade` | `decimal(2,0)` | sim | - |  |
| 2 | `De` | `varchar(150)` | sim | - |  |
| 3 | `Para` | `varchar(250)` | sim | - |  |
| 4 | `Cc` | `varchar(250)` | sim | - |  |
| 5 | `Bcc` | `varchar(250)` | sim | - |  |
| 6 | `Assunto` | `varchar(250)` | sim | - |  |
| 7 | `Anexado` | `varchar(250)` | sim | - |  |
| 8 | `Mensagem` | `text(2147483647)` | sim | - |  |
| 9 | `Solicitante` | `varchar(30)` | sim | - |  |
| 10 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 11 | `EnderecoResposta` | `varchar(150)` | sim | - |  |
| 12 | `Status` | `varchar(1)` | sim | - | status - validar dominio real por tabela |
| 13 | `DtaGeracao` | `datetime` | sim | - |  |
| 14 | `DtaEnviar` | `datetime` | sim | - |  |
| 15 | `SEQEMAILSEND` | `numeric(18,0)` | **nao** | - | **PK** |
| 16 | `SEQCONTAEMAIL` | `numeric(18,0)` | sim | - | FK -> `GE_ParamLista.SeqParamLista` |
| 17 | `CONTEXTO` | `varchar(250)` | sim | - |  |
| 18 | `SUBCONTEXTO` | `varchar(250)` | sim | - |  |
| 19 | `MSGERRO` | `varchar(250)` | sim | - |  |
| 20 | `ORIGEM` | `char(3)` | sim | - | sistema de origem do dado |
| 21 | `CHAVE` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `GEP_EMAILSENDREL.SEQEMAILSEND`

---

### GEP_EMAILSENDREL

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQEMAILSENDREL`

**Funcao:** GEP_EMAILSENDREL e GEP_EMAILSENTREL existem com 0 linhas (entrega de relatório nunca usada). (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQEMAILSENDREL` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQEMAILSEND` | `numeric(18,0)` | **nao** | - | FK -> `GEP_EMailSend.SEQEMAILSEND` |
| 3 | `SEQCONSSQL` | `numeric(18,0)` | **nao** | - | FK -> `GE_CONSSQL.SEQCONSSQL` |
| 4 | `FILTROVALOR` | `varchar(250)` | sim | - |  |

---

### GEP_EMAILSENT

`classe: nucleo` · `20 colunas` · `571.866 linhas (snapshot 03/06/2026)` · `PK: SEQEMAILSENT`

**Funcao:** Motor de notificação declarativo: 1.341 regras 'resultado X → mensagem para papel/usuário Y pelo canal Z com template W', com fila assíncrona (job EMAIL_SEND a cada 3 min) e 585.938 envios registrados. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQEMAILSENT` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `DE` | `varchar(150)` | sim | - |  |
| 3 | `PARA` | `varchar(250)` | sim | - |  |
| 4 | `CC` | `varchar(250)` | sim | - |  |
| 5 | `BCC` | `varchar(250)` | sim | - |  |
| 6 | `ASSUNTO` | `varchar(250)` | sim | - |  |
| 7 | `ANEXADO` | `varchar(250)` | sim | - |  |
| 8 | `MENSAGEM` | `text(2147483647)` | sim | - |  |
| 9 | `SOLICITANTE` | `varchar(30)` | sim | - |  |
| 10 | `NROEMPRESA` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 11 | `ENDERECORESPOSTA` | `varchar(150)` | sim | - |  |
| 12 | `DTAGERACAO` | `datetime` | sim | - |  |
| 13 | `STATUS` | `varchar(1)` | sim | - | status - validar dominio real por tabela |
| 14 | `MSGERRO` | `varchar(250)` | sim | - |  |
| 15 | `DTAENVIO` | `datetime` | sim | - |  |
| 16 | `SEQCONTAEMAIL` | `numeric(18,0)` | sim | - |  |
| 17 | `CONTEXTO` | `varchar(250)` | sim | - |  |
| 18 | `SUBCONTEXTO` | `varchar(250)` | sim | - |  |
| 19 | `ORIGEM` | `char(3)` | sim | - | sistema de origem do dado |
| 20 | `CHAVE` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `GEP_EMAILSENTREL.SEQEMAILSENT`

---

### GEP_EMAILSENTREL

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQEMAILSENTREL`

**Funcao:** GEP_EMAILSENDREL e GEP_EMAILSENTREL existem com 0 linhas (entrega de relatório nunca usada). (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQEMAILSENTREL` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQEMAILSENT` | `numeric(18,0)` | **nao** | - | FK -> `GEP_EMAILSENT.SEQEMAILSENT` |
| 3 | `SEQCONSSQL` | `numeric(18,0)` | **nao** | - |  |
| 4 | `FILTROVALOR` | `varchar(250)` | sim | - |  |

---

### gep_excfrota

`classe: isolada` · `7 colunas` · `1.180 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEP` (jobs e integracao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `cliente` | `varchar(60)` | sim | - |  |
| 2 | `cidade` | `varchar(40)` | sim | - |  |
| 3 | `ano` | `varchar(10)` | sim | - |  |
| 4 | `marca` | `varchar(30)` | sim | - |  |
| 5 | `modelo` | `varchar(30)` | sim | - |  |
| 6 | `qtde` | `varchar(10)` | sim | - |  |
| 7 | `tipo` | `varchar(30)` | sim | - |  |

---

### gep_excfrotarep

`classe: isolada` · `11 colunas` · `425 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEP` (jobs e integracao). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `seqpessoa` | `int(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `nomerazao` | `varchar(60)` | sim | - |  |
| 3 | `cidade` | `varchar(40)` | sim | - |  |
| 4 | `propriedade` | `varchar(40)` | sim | - |  |
| 5 | `modelo` | `varchar(30)` | sim | - |  |
| 6 | `qtde` | `int(10,0)` | sim | - |  |
| 7 | `marca` | `varchar(40)` | sim | - |  |
| 8 | `ano` | `varchar(10)` | sim | - |  |
| 9 | `notas` | `varchar(250)` | sim | - |  |
| 10 | `identificador` | `varchar(30)` | sim | - |  |
| 11 | `qtfrota` | `int(10,0)` | sim | - |  |

---

### GEP_EXCFROTASEQ

`classe: isolada` · `1 colunas` · `1.128 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEP` (jobs e integracao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPROPPESSOA` | `int(10,0)` | sim | - |  |

---

### GEP_EXCFROTASEQDEL

`classe: isolada` · `1 colunas` · `1.082 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEP` (jobs e integracao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPROPPESSOA` | `int(10,0)` | sim | - |  |

---

### gep_excseqcar

`classe: isolada` · `1 colunas` · `3.143 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `seqpessoa` | `int(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |

---

### gep_excseqcarok

`classe: isolada` · `1 colunas` · `1.668 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `seqpessoa` | `int(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |

---

### GEP_Fila

`classe: isolada` · `18 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SEQFILA`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de fila de processamento, no modulo `GEP` (jobs e integracao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Servidor` | `decimal(3,0)` | sim | - |  |
| 2 | `CodProcesso` | `varchar(30)` | sim | - | TIPO de fluxo (41/50) - nao e o numero do processo |
| 3 | `Tipo` | `varchar(10)` | sim | - |  |
| 4 | `Aplicativo` | `varchar(20)` | sim | - |  |
| 5 | `DtaExecucao` | `datetime` | sim | - |  |
| 6 | `ExeMinuto` | `decimal(8,2)` | sim | - |  |
| 7 | `Prioridade` | `decimal(2,0)` | sim | - |  |
| 8 | `Solicitante` | `varchar(20)` | sim | - |  |
| 9 | `SeqProcesso` | `numeric(18,0)` | sim | - |  |
| 10 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 11 | `Obs` | `varchar(150)` | sim | - | texto livre |
| 12 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 13 | `LinkStr` | `varchar(20)` | sim | - |  |
| 14 | `Argumento` | `varchar(200)` | sim | - |  |
| 15 | `DNet` | `numeric(1,0)` | sim | - |  |
| 16 | `PrioridadeThread` | `numeric(1,0)` | sim | - |  |
| 17 | `ThreadNro` | `numeric(4,0)` | sim | - |  |
| 18 | `SEQFILA` | `numeric(18,0)` | **nao** | - | **PK** |

---

### GEP_Import

`classe: isolada` · `15 colunas` · `344 linhas (snapshot 03/06/2026)` · `PK: Seq`

**Funcao:** Fila genérica de importação linha-a-linha (formato EAV: lista de colunas + lista de dados separadas por um caractere). Usada por RD Station e MOBILELITE. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Processo` | `varchar(20)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 3 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 4 | `Acao` | `varchar(1)` | sim | - | acao (`IV_Acao.Acao`) |
| 5 | `Banco` | `varchar(20)` | sim | - |  |
| 6 | `Dono` | `varchar(20)` | sim | - |  |
| 7 | `Tabela` | `varchar(30)` | **nao** | - |  |
| 8 | `Separador` | `varchar(1)` | **nao** | - |  |
| 9 | `Status` | `varchar(1)` | sim | - | status - validar dominio real por tabela |
| 10 | `Coluna` | `varchar(1000)` | **nao** | - |  |
| 11 | `Dado` | `varchar(2000)` | **nao** | - |  |
| 12 | `ColunaIdentific` | `varchar(100)` | sim | - |  |
| 13 | `DadoIdentificador` | `varchar(200)` | sim | - |  |
| 14 | `DtaGeracao` | `datetime` | sim | - |  |
| 15 | `Prioridade` | `decimal(2,0)` | sim | - |  |

---

### GEP_ImportAprovacao

`classe: isolada` · `15 colunas` · `4.284 linhas (snapshot 03/06/2026)` · `PK: Seq`

**Funcao:** Fila genérica de importação linha-a-linha (formato EAV: lista de colunas + lista de dados separadas por um caractere). Usada por RD Station e MOBILELITE. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Processo` | `varchar(20)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 3 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 4 | `Acao` | `varchar(1)` | sim | - | acao (`IV_Acao.Acao`) |
| 5 | `Banco` | `varchar(20)` | sim | - |  |
| 6 | `Dono` | `varchar(20)` | sim | - |  |
| 7 | `Tabela` | `varchar(30)` | **nao** | - |  |
| 8 | `Separador` | `varchar(1)` | **nao** | - |  |
| 9 | `Status` | `varchar(1)` | sim | - | status - validar dominio real por tabela |
| 10 | `Coluna` | `varchar(1000)` | **nao** | - |  |
| 11 | `Dado` | `varchar(2000)` | **nao** | - |  |
| 12 | `ColunaIdentific` | `varchar(100)` | sim | - |  |
| 13 | `DadoIdentificador` | `varchar(200)` | sim | - |  |
| 14 | `DtaGeracao` | `datetime` | sim | - |  |
| 15 | `Prioridade` | `decimal(2,0)` | sim | - |  |

---

### GEP_ImportErro

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Fila genérica de importação linha-a-linha (formato EAV: lista de colunas + lista de dados separadas por um caractere). Usada por RD Station e MOBILELITE. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - |  |
| 2 | `Servidor` | `decimal(2,0)` | sim | - |  |
| 3 | `Erro` | `varchar(250)` | sim | - |  |
| 4 | `DtaGeracao` | `datetime` | sim | - |  |

---

### GEP_ImportTry

`classe: isolada` · `4 colunas` · `1.870 linhas (snapshot 03/06/2026)` · `PK: Seq, Origem`

**Funcao:** Fila genérica de importação linha-a-linha (formato EAV: lista de colunas + lista de dados separadas por um caractere). Usada por RD Station e MOBILELITE. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | **PK**; sistema de origem do dado |
| 3 | `DtaLimiteTentativa` | `datetime` | sim | - |  |
| 4 | `Razao` | `varchar(50)` | sim | - |  |

---

### GEP_Import_bkpjun

`classe: lixo/backup` · `15 colunas` · `502.009 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de importacao de dados, no modulo `GEP` (jobs e integracao). As colunas confirmam vinculo com processo (`Processo`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (15 colunas) para manter o documento legivel._

---

### GEP_IMPORT_GUI

`classe: isolada` · `15 colunas` · `52 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de importacao de dados, no modulo `GEP` (jobs e integracao). As colunas confirmam vinculo com processo (`Processo`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - |  |
| 2 | `Processo` | `varchar(20)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 3 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 4 | `Acao` | `varchar(1)` | sim | - | acao (`IV_Acao.Acao`) |
| 5 | `Banco` | `varchar(20)` | sim | - |  |
| 6 | `Dono` | `varchar(20)` | sim | - |  |
| 7 | `Tabela` | `varchar(30)` | **nao** | - |  |
| 8 | `Separador` | `varchar(1)` | **nao** | - |  |
| 9 | `Status` | `varchar(1)` | sim | - | status - validar dominio real por tabela |
| 10 | `Coluna` | `varchar(1000)` | **nao** | - |  |
| 11 | `Dado` | `varchar(2000)` | **nao** | - |  |
| 12 | `ColunaIdentific` | `varchar(100)` | sim | - |  |
| 13 | `DadoIdentificador` | `varchar(200)` | sim | - |  |
| 14 | `DtaGeracao` | `datetime` | sim | - |  |
| 15 | `Prioridade` | `decimal(2,0)` | sim | - |  |

---

### GEP_JOBAGD

`classe: nucleo` · `18 colunas` · `25 linhas (snapshot 03/06/2026)` · `PK: SeqJOBAgd`

**Funcao:** Agendador do produto: 71 tipos de job catalogados (25+ integrações externas prontas, 4 stored procedures, execução de .EXE/.LNK), 25 agendados na Tracbel. É a espinha dorsal de toda a integração e da comunicação assíncrona. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqJOBAgd` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tipo` | `varchar(20)` | sim | - |  |
| 3 | `NroZUMBI` | `numeric(2,0)` | sim | - |  |
| 4 | `DtaInicio` | `datetime` | sim | - |  |
| 5 | `Codigo` | `varchar(30)` | sim | - |  |
| 6 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 7 | `Aplicacao` | `varchar(100)` | sim | - |  |
| 8 | `Prioridade` | `numeric(1,0)` | sim | - |  |
| 9 | `THREADEXCLUSIVA` | `numeric(1,0)` | sim | - |  |
| 10 | `Argumento` | `varchar(250)` | sim | - |  |
| 11 | `IntervaloExec` | `numeric(4,0)` | sim | - |  |
| 12 | `UltimaExecucao` | `datetime` | sim | - |  |
| 13 | `NroTENTATIVA` | `numeric(2,0)` | sim | - |  |
| 14 | `IntervaloTENTATIVA` | `numeric(4,0)` | sim | - |  |
| 15 | `DiasVIdALOG` | `numeric(4,0)` | sim | - |  |
| 16 | `PlanoExec` | `varchar(250)` | sim | - |  |
| 17 | `ExecucaoExclusiva` | `numeric(1,0)` | sim | - |  |
| 18 | `SEQJOB` | `numeric(18,0)` | sim | - | FK -> `GEP_JOBCAD.SeqJOB` |

**Referenciada por:** `GEP_JOBFILA.SeqJobAgd`

---

### GEP_JobAgdExecLog

`classe: isolada` · `13 colunas` · `981.319 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Agendador do produto: 71 tipos de job catalogados (25+ integrações externas prontas, 4 stored procedures, execução de .EXE/.LNK), 25 agendados na Tracbel. É a espinha dorsal de toda a integração e da comunicação assíncrona. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `DtaExecucao` | `datetime` | sim | - |  |
| 2 | `Codigo` | `varchar(30)` | sim | - |  |
| 3 | `TipoLog` | `char(1)` | sim | - |  |
| 4 | `TipoErro` | `varchar(20)` | sim | - |  |
| 5 | `Argumento` | `varchar(250)` | sim | - |  |
| 6 | `SeqJobAgd` | `numeric(18,0)` | sim | - |  |
| 7 | `Descricao` | `text(2147483647)` | sim | - | descricao do registro |
| 8 | `NROTENTATIVA` | `numeric(2,0)` | sim | - |  |
| 9 | `DTAFINALEXECUCAO` | `datetime` | sim | - |  |
| 10 | `LINKSTR` | `varchar(30)` | sim | - |  |
| 11 | `LINKNRO2` | `numeric(18,0)` | sim | - |  |
| 12 | `LINKNRO1` | `numeric(18,0)` | sim | - |  |
| 13 | `PRIORIDADE` | `numeric(2,0)` | sim | - |  |

---

### GEP_JOBCAD

`classe: catalogo` · `11 colunas` · `71 linhas (snapshot 03/06/2026)` · `PK: SeqJOB`

**Funcao:** Agendador do produto: 71 tipos de job catalogados (25+ integrações externas prontas, 4 stored procedures, execução de .EXE/.LNK), 25 agendados na Tracbel. É a espinha dorsal de toda a integração e da comunicação assíncrona. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqJOB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Codigo` | `varchar(30)` | sim | - |  |
| 3 | `Tipo` | `char(1)` | sim | - |  |
| 4 | `LICENCA` | `varchar(10)` | sim | - |  |
| 5 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 6 | `Argumento` | `varchar(150)` | sim | - |  |
| 7 | `ESTIMULOFILA` | `numeric(1,0)` | sim | - |  |
| 8 | `TIPOJOB` | `varchar(15)` | sim | - |  |
| 9 | `OBJETIVO` | `varchar(250)` | sim | - |  |
| 10 | `RESTRICOES` | `varchar(250)` | sim | - |  |
| 11 | `PRIORIDADEFILA` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `GEP_JOBAGD.SEQJOB`, `IV_RESJOB.SEQJOB`

---

### GEP_JOBFILA

`classe: nucleo` · `16 colunas` · `23 linhas (snapshot 03/06/2026)` · `PK: SeqJOBFILA`

**Funcao:** Agendador de jobs do Vórtice. Catálogo (71) × instância agendada (25) × fila sob demanda (23) × log (981.319) × monitor × matriz de notificação. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqJOBFILA` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `NroZUMBI` | `numeric(2,0)` | sim | - |  |
| 3 | `DtaExecucao` | `datetime` | sim | - |  |
| 4 | `Prioridade` | `numeric(2,0)` | sim | - |  |
| 5 | `Codigo` | `varchar(30)` | sim | - |  |
| 6 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 7 | `LinkStr` | `varchar(30)` | sim | - |  |
| 8 | `LINKNro1` | `numeric(18,0)` | sim | - |  |
| 9 | `LINKNro2` | `numeric(18,0)` | sim | - |  |
| 10 | `Argumento` | `varchar(150)` | sim | - |  |
| 11 | `SeqUSRSolicitaNTE` | `numeric(18,0)` | sim | - |  |
| 12 | `DtaEXECORIGINAL` | `datetime` | sim | - |  |
| 13 | `NroTENTATIVA` | `numeric(2,0)` | sim | - |  |
| 14 | `FILAWAIT` | `numeric(1,0)` | sim | - |  |
| 15 | `SeqJobAgd` | `numeric(18,0)` | sim | - | FK -> `GEP_JOBAGD.SeqJOBAgd` |
| 16 | `INDAVISOINDIVIDUALSM` | `numeric(1,0)` | sim | - |  |

---

### GEP_JOBFILALOG

`classe: isolada` · `5 colunas` · `14.038 linhas (snapshot 03/06/2026)` · `PK: SeqJOBFILA`

**Funcao:** Agendador de jobs do Vórtice. Catálogo (71) × instância agendada (25) × fila sob demanda (23) × log (981.319) × monitor × matriz de notificação. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqJOBFILA` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `DtaExecucao` | `datetime` | sim | - |  |
| 3 | `CodERRO` | `numeric(18,0)` | sim | - |  |
| 4 | `Descricao` | `varchar(250)` | sim | - | descricao do registro |
| 5 | `DtaMORTE` | `datetime` | sim | - |  |

---

### GEP_JobMonitor

`classe: isolada` · `10 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: JobId`

**Funcao:** Agendador de jobs do Vórtice. Catálogo (71) × instância agendada (25) × fila sob demanda (23) × log (981.319) × monitor × matriz de notificação. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `JobId` | `varchar(50)` | **nao** | - | **PK** |
| 2 | `Par` | `varchar(250)` | sim | - |  |
| 3 | `Inicio` | `datetime` | sim | - |  |
| 4 | `UltOk` | `datetime` | sim | - |  |
| 5 | `Fim` | `datetime` | sim | - |  |
| 6 | `LogNormal` | `varchar(1000)` | sim | - |  |
| 7 | `LogAdv` | `varchar(1000)` | sim | - |  |
| 8 | `LogErro` | `varchar(1000)` | sim | - |  |
| 9 | `TipoLog` | `char(1)` | sim | - |  |
| 10 | `Retorno` | `varchar(250)` | sim | - |  |

---

### GEP_JOBNOTIFICAR

`classe: vazia` · `14 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqJOBAgd, SeqNotificar`

**Funcao:** Agendador de jobs do Vórtice. Catálogo (71) × instância agendada (25) × fila sob demanda (23) × log (981.319) × monitor × matriz de notificação. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqJOBAgd` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqNotificar` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `Email` | `varchar(100)` | sim | - |  |
| 4 | `SeqUsuario` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 5 | `IndENVEMAIL` | `numeric(1,0)` | sim | - |  |
| 6 | `IndENVSMS` | `numeric(1,0)` | sim | - |  |
| 7 | `IndENVCRM` | `numeric(1,0)` | sim | - |  |
| 8 | `IndENVREDESOC` | `numeric(1,0)` | sim | - |  |
| 9 | `IndEXECUCAO` | `numeric(1,0)` | sim | - |  |
| 10 | `IndERRO` | `numeric(1,0)` | sim | - |  |
| 11 | `AssuntoAdicional` | `varchar(250)` | sim | - |  |
| 12 | `IndADVERTENCIA` | `numeric(1,0)` | sim | - |  |
| 13 | `IndGERARESULTADO` | `numeric(1,0)` | sim | - |  |
| 14 | `Resultado` | `numeric(6,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |

---

### GEP_Notificar

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProcesso, SeqNotificar`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de notificacao, no modulo `GEP` (jobs e integracao). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProcesso` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GEP_Processo.SeqProcesso` |
| 2 | `SeqNotificar` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `CodUsuario` | `varchar(12)` | sim | - | login do usuario (varchar) |
| 4 | `Email` | `varchar(100)` | sim | - |  |
| 5 | `InterVISION` | `char(1)` | sim | - |  |
| 6 | `Assunto` | `varchar(40)` | sim | - |  |
| 7 | `PrioridadeErro` | `decimal(1,0)` | sim | - |  |
| 8 | `PrioridadeExec` | `decimal(1,0)` | sim | - |  |

---

### GEP_PABX

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPabx`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia (PABX), no modulo `GEP` (jobs e integracao). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPabx` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `DtaGeracao` | `datetime` | sim | - |  |
| 3 | `Tipo` | `char(1)` | **nao** | - |  |
| 4 | `Ramal` | `varchar(10)` | sim | - |  |
| 5 | `Telefone` | `varchar(20)` | sim | - |  |
| 6 | `DtaProc` | `datetime` | sim | - |  |

---

### GEP_ParEnvia

`classe: catalogo` · `7 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: Destino`

**Funcao:** Registro e parametrização de sistemas de origem (14) e destino (3) da integração, com flags de comportamento do merge por origem. GEP_ParRecEnvia (matriz origem×destino) está vazia. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Destino` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `SessaoCnx` | `varchar(20)` | sim | - |  |
| 3 | `IndEnvPessoa` | `numeric(1,0)` | sim | - |  |
| 4 | `IndEnvProspect` | `numeric(1,0)` | sim | - |  |
| 5 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 6 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |
| 7 | `INDENVTODAS` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `GEP_ParRecEnvia.Destino`, `GE_PessoaDestino.Destino`

---

### GEP_ParRecebe

`classe: nucleo` · `12 colunas` · `14 linhas (snapshot 03/06/2026)` · `PK: Origem`

**Funcao:** Registro e parametrização de sistemas de origem (14) e destino (3) da integração, com flags de comportamento do merge por origem. GEP_ParRecEnvia (matriz origem×destino) está vazia. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Origem` | `varchar(20)` | **nao** | - | **PK**; sistema de origem do dado |
| 2 | `IndAlteraPessoa` | `numeric(1,0)` | sim | - |  |
| 3 | `IndIdentPesCNPJ` | `numeric(1,0)` | sim | - |  |
| 4 | `IndSobrepoeFone` | `numeric(1,0)` | sim | - |  |
| 5 | `IndEnviaOut` | `numeric(1,0)` | sim | - |  |
| 6 | `DestinoOut` | `varchar(20)` | sim | - |  |
| 7 | `QtdDiasRemove` | `numeric(4,0)` | sim | - |  |
| 8 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 9 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |
| 10 | `INDALTERAPESSOAEXT` | `char(1)` | sim | - |  |
| 11 | `INDMANTEMORDEMFONES` | `numeric(1,0)` | sim | - |  |
| 12 | `INDMANTEMATIVO` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `GEP_ParRecEnvia.Origem`

---

### GEP_ParRecEnvia

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Origem, Destino`

**Funcao:** Registro e parametrização de sistemas de origem (14) e destino (3) da integração, com flags de comportamento do merge por origem. GEP_ParRecEnvia (matriz origem×destino) está vazia. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Origem` | `varchar(20)` | **nao** | - | **PK**; FK -> `GEP_ParRecebe.Origem`; sistema de origem do dado |
| 2 | `Destino` | `varchar(20)` | **nao** | - | **PK**; FK -> `GEP_ParEnvia.Destino` |

---

### GEP_ProcControle

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Servidor`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de controle/condicoes de uso relacionada a processo/oportunidade do BPM, no modulo `GEP` (jobs e integracao). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Servidor` | `decimal(3,0)` | **nao** | - | **PK** |
| 2 | `DtaUltProc` | `datetime` | sim | - |  |
| 3 | `DtaInicio` | `datetime` | sim | - |  |
| 4 | `DtaFim` | `datetime` | sim | - |  |
| 5 | `EmExecucao` | `numeric(1,0)` | sim | - |  |

---

### GEP_Processo

`classe: nucleo` · `20 colunas` · `8 linhas (snapshot 03/06/2026)` · `PK: SeqProcesso`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `GEP` (jobs e integracao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProcesso` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 3 | `SistemaOrigem` | `varchar(20)` | sim | - |  |
| 4 | `Conexao` | `varchar(20)` | sim | - |  |
| 5 | `CodProcesso` | `varchar(20)` | sim | - | TIPO de fluxo (41/50) - nao e o numero do processo |
| 6 | `Prioridade` | `decimal(2,0)` | sim | - |  |
| 7 | `Aplicativo` | `varchar(20)` | sim | - |  |
| 8 | `Tipo` | `varchar(10)` | sim | - |  |
| 9 | `ExeDta` | `datetime` | sim | - |  |
| 10 | `ExeDiaSemana` | `varchar(3)` | sim | - |  |
| 11 | `ExeDiaMes` | `decimal(2,0)` | sim | - |  |
| 12 | `ExeHora` | `datetime` | sim | - |  |
| 13 | `Servidor` | `decimal(3,0)` | sim | - |  |
| 14 | `ExeMinuto` | `decimal(4,2)` | sim | - |  |
| 15 | `UltimaExecucao` | `datetime` | sim | - |  |
| 16 | `Argumento` | `varchar(200)` | sim | - |  |
| 17 | `SuspendeAposExec` | `decimal(1,0)` | sim | - |  |
| 18 | `DNet` | `numeric(1,0)` | sim | - |  |
| 19 | `PrioridadeThread` | `numeric(1,0)` | sim | - |  |
| 20 | `ThreadNro` | `numeric(4,0)` | sim | - |  |

**Referenciada por:** `GEP_Notificar.SeqProcesso`

---

### GEP_ProcImport

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Fila genérica de importação linha-a-linha (formato EAV: lista de colunas + lista de dados separadas por um caractere). Usada por RD Station e MOBILELITE. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 2 | `ACAO` | `char(1)` | sim | - | acao (`IV_Acao.Acao`) |
| 3 | `TAG` | `char(1)` | sim | - |  |
| 4 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 5 | `Tabela` | `varchar(30)` | sim | - |  |
| 6 | `DtaProcessar` | `datetime` | sim | - |  |
| 7 | `Prioridade` | `numeric(1,0)` | sim | - |  |
| 8 | `Chave` | `varchar(20)` | sim | - |  |
| 9 | `DADO` | `varchar(2000)` | sim | - |  |

---

### GEP_SyncUsrSat

`classe: isolada` · `15 colunas` · `3.901.359 linhas (snapshot 03/06/2026)` · `PK: SEQSYNCUSRSAT`

**Funcao:** Outbox de replicação para satélites móveis (Vórtico Mobile Lite). 3.901.359 linhas para 65 usuários ativos; ~60 tabelas replicadas com operação I/A/E por registro por usuário. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Destino` | `varchar(20)` | **nao** | - |  |
| 2 | `Usr` | `numeric(8,0)` | **nao** | - |  |
| 3 | `Tabela` | `varchar(30)` | **nao** | - |  |
| 4 | `IndProcessado` | `numeric(1,0)` | sim | - |  |
| 5 | `IndProcessoWait` | `numeric(1,0)` | sim | - |  |
| 6 | `DtaGeracao` | `datetime` | sim | - |  |
| 7 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 8 | `KN2` | `numeric(18,0)` | sim | - |  |
| 9 | `KS1` | `varchar(50)` | **nao** | - |  |
| 10 | `KS2` | `varchar(50)` | sim | - |  |
| 11 | `Operacao` | `char(1)` | sim | - |  |
| 12 | `NivelSeguranca` | `decimal(3,0)` | sim | - |  |
| 13 | `DtaColeta` | `datetime` | sim | - |  |
| 14 | `SEQSYNCUSRSAT` | `numeric(18,0)` | **nao** | - | **PK** |
| 15 | `FCMTOKEN` | `varchar(250)` | sim | - |  |

---

### Gep_UsrPabx

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqUsrPabx`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema, no modulo `GEP` (jobs e integracao). As colunas confirmam vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsrPabx` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `PabxLogin` | `varchar(50)` | sim | - |  |
| 4 | `PabxSenha` | `varchar(50)` | sim | - |  |
| 5 | `Campanha` | `varchar(50)` | sim | - |  |

---

### GEP_UsrSat

`classe: nucleo` · `16 colunas` · `71 linhas (snapshot 03/06/2026)` · `PK: Destino, Usr`

**Funcao:** Outbox de replicação para satélites móveis (Vórtico Mobile Lite). 3.901.359 linhas para 65 usuários ativos; ~60 tabelas replicadas com operação I/A/E por registro por usuário. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Destino` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `Usr` | `numeric(8,0)` | **nao** | - | **PK** |
| 3 | `IDSatelite` | `varchar(40)` | sim | - |  |
| 4 | `DtaInicioRec` | `datetime` | sim | - |  |
| 5 | `DtaUltimoRec` | `datetime` | sim | - |  |
| 6 | `DtaInicioEnvio` | `datetime` | sim | - |  |
| 7 | `DtaUltimoEnvio` | `datetime` | sim | - |  |
| 8 | `IndSateliteLivre` | `numeric(1,0)` | sim | - |  |
| 9 | `SeqSyncUsrSat` | `numeric(18,0)` | sim | - |  |
| 10 | `UltServicoEnviado` | `varchar(50)` | sim | - |  |
| 11 | `SeqLogUsr` | `numeric(18,0)` | sim | - |  |
| 12 | `SeqUsrLog` | `numeric(18,0)` | sim | - |  |
| 13 | `SQL` | `varchar(2000)` | sim | - |  |
| 14 | `VersaoSO` | `varchar(50)` | sim | - |  |
| 15 | `VersaoApp` | `varchar(50)` | sim | - |  |
| 16 | `SeqMobileConfig` | `numeric(10,0)` | sim | - | FK -> `GE_MobileConfig.SeqMobileConfig` |

---
