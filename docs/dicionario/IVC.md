# IVC - Call center: equipes, atendentes e ramais

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**6 tabelas · 57 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IVC_ATENDENTE`](#ivc_atendente) | vazia | 18 | 0 |
| [`IVC_ATENDENTELOG`](#ivc_atendentelog) | vazia | 6 | 0 |
| [`IVC_CHAMADALOG`](#ivc_chamadalog) | vazia | 22 | 0 |
| [`IVC_EQUIPE`](#ivc_equipe) | vazia | 3 | 0 |
| [`IVC_EQUIPEUSR`](#ivc_equipeusr) | vazia | 3 | 0 |
| [`IVC_RAMAL`](#ivc_ramal) | vazia | 5 | 0 |

---

### IVC_ATENDENTE

`classe: vazia` · `18 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQUSUARIO`

**Funcao:** Modelo de EQUIPE e de call center — schema completo, ZERO linhas em todas. Nunca implantado na Tracbel. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `STATUS` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 3 | `STATUSCMPL` | `varchar(30)` | sim | - |  |
| 4 | `STATUSCTI` | `varchar(30)` | sim | - |  |
| 5 | `SEQAGENDA` | `numeric(18,0)` | sim | - | FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 6 | `NROFONEEXTERNO` | `varchar(20)` | sim | - |  |
| 7 | `NRORAMAL` | `varchar(10)` | sim | - |  |
| 8 | `DTAINICIO` | `datetime` | sim | - |  |
| 9 | `DTAINICIOCONVERSA` | `datetime` | sim | - |  |
| 10 | `DTAFINALCONVERSA` | `datetime` | sim | - |  |
| 11 | `DTAULTATENDIMENTO` | `datetime` | sim | - |  |
| 12 | `DTAULTSTATUS` | `datetime` | sim | - |  |
| 13 | `DTALOGIN` | `datetime` | sim | - |  |
| 14 | `IDCHAMADACTI` | `varchar(20)` | sim | - |  |
| 15 | `SENTIDO` | `char(1)` | sim | - |  |
| 16 | `PROCESSO` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 17 | `STATUSNEXT` | `varchar(20)` | sim | - |  |
| 18 | `SILOORIGEM` | `numeric(8,0)` | sim | - |  |

---

### IVC_ATENDENTELOG

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQATENDENTELOG`

**Funcao:** Modelo de EQUIPE e de call center — schema completo, ZERO linhas em todas. Nunca implantado na Tracbel. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQATENDENTELOG` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQUSUARIO` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `DATA` | `datetime` | sim | - |  |
| 4 | `STATUS` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 5 | `STATUSCMPL` | `varchar(30)` | sim | - |  |
| 6 | `NRORAMAL` | `varchar(10)` | sim | - |  |

---

### IVC_CHAMADALOG

`classe: vazia` · `22 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCHAMADALOG`

**Funcao:** Modelo de EQUIPE e de call center — schema completo, ZERO linhas em todas. Nunca implantado na Tracbel. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCHAMADALOG` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `DTAINICIO` | `datetime` | sim | - |  |
| 3 | `DTAFINAL` | `datetime` | sim | - |  |
| 4 | `SEQUSUARIO` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 5 | `SEQAGENDA` | `numeric(18,0)` | sim | - | agenda (`IV_Agenda.SeqAgenda`) |
| 6 | `PROCESSO` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 7 | `NRORAMAL` | `varchar(10)` | sim | - |  |
| 8 | `NROFONEEXTERNO` | `varchar(20)` | sim | - |  |
| 9 | `SENTIDO` | `char(1)` | sim | - |  |
| 10 | `IDCHAMADACTI` | `varchar(20)` | sim | - |  |
| 11 | `ARQUIVOAUDIO` | `varchar(100)` | sim | - |  |
| 12 | `SILOORIGEM` | `numeric(8,0)` | sim | - |  |
| 13 | `DTAINICIOCONVERSA` | `datetime` | sim | - |  |
| 14 | `DTAFINALCONVERSA` | `datetime` | sim | - |  |
| 15 | `DURACAOTOTAL` | `numeric(6,1)` | sim | - |  |
| 16 | `DURACAOCONVERSA` | `numeric(6,1)` | sim | - |  |
| 17 | `RESULTADO` | `numeric(10,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |
| 18 | `OBS` | `varchar(100)` | sim | - | texto livre |
| 19 | `EVENTO` | `varchar(250)` | sim | - |  |
| 20 | `SEQPESSOA` | `numeric(18,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 21 | `DTAGERACAO` | `datetime` | sim | - |  |
| 22 | `INDLINKGERADO` | `numeric(1,0)` | sim | - |  |

---

### IVC_EQUIPE

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQEQUIPE`

**Funcao:** Modelo de EQUIPE e de call center — schema completo, ZERO linhas em todas. Nunca implantado na Tracbel. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQEQUIPE` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `EQUIPE` | `varchar(20)` | **nao** | - |  |
| 3 | `DESCRICAO` | `varchar(150)` | **nao** | - | descricao do registro |

**Referenciada por:** `IVC_EQUIPEUSR.SEQEQUIPE`

---

### IVC_EQUIPEUSR

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQUSUARIO, SEQEQUIPE`

**Funcao:** Modelo de EQUIPE e de call center — schema completo, ZERO linhas em todas. Nunca implantado na Tracbel. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `SEQEQUIPE` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `IVC_EQUIPE.SEQEQUIPE` |
| 3 | `PAPEL` | `varchar(3)` | sim | - |  |

---

### IVC_RAMAL

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDPABX, RAMAL`

**Funcao:** Modelo de EQUIPE e de call center — schema completo, ZERO linhas em todas. Nunca implantado na Tracbel. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDPABX` | `numeric(2,0)` | **nao** | - | **PK** |
| 2 | `RAMAL` | `varchar(10)` | **nao** | - | **PK** |
| 3 | `DATASTATUS` | `datetime` | sim | - |  |
| 4 | `STATUSCTI` | `varchar(30)` | sim | - |  |
| 5 | `STATUSDESC` | `varchar(50)` | sim | - |  |

---
