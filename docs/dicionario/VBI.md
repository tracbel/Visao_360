# VBI - Plano de contas para BI

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**3 tabelas · 38 colunas · 2.251 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`VBI_CONTA`](#vbi_conta) | nucleo | 14 | 1.004 |
| [`VBI_CONTAFAM`](#vbi_contafam) | nucleo | 6 | 1.004 |
| [`VBI_CONTAPAI`](#vbi_contapai) | nucleo | 18 | 243 |

---

### VBI_CONTA

`classe: nucleo` · `14 colunas` · `1.004 linhas (snapshot 03/06/2026)` · `PK: SEQCONTA`

**Funcao:** Camada de relatórios: 134 consultas SQL editáveis em produção com layout .qrp, gráfico e telemetria de uso; 411 views no banco, das quais 32 são BI_* de funil/carteira/cobertura; e a Estrutura Gerencial de Análise (EGA) para DRE gerencial. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONTA` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQCONTAPAI` | `numeric(18,0)` | **nao** | - | FK -> `VBI_CONTAPAI.SEQCONTA` |
| 3 | `NIVEL` | `numeric(2,0)` | **nao** | - |  |
| 4 | `NROORDEM` | `numeric(4,0)` | **nao** | - |  |
| 5 | `DESCRICAO` | `varchar(40)` | **nao** | - | descricao do registro |
| 6 | `CODCONTA` | `varchar(20)` | **nao** | - |  |
| 7 | `CORRGB` | `varchar(20)` | sim | - |  |
| 8 | `ORIGEM` | `varchar(6)` | **nao** | - | sistema de origem do dado |
| 9 | `CHAVEEXTERNA` | `varchar(30)` | sim | - |  |
| 10 | `CHAVEEXTOBS` | `varchar(100)` | sim | - |  |
| 11 | `SINAL` | `numeric(1,0)` | **nao** | - |  |
| 12 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 13 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 14 | `OBS` | `varchar(250)` | sim | - | texto livre |

**Referenciada por:** `VBI_CONTAFAM.SEQCONTA`

---

### VBI_CONTAFAM

`classe: nucleo` · `6 colunas` · `1.004 linhas (snapshot 03/06/2026)` · `PK: SEQCONTA`

**Funcao:** VBI_CONTA/VBI_CONTAFAM/VBI_CONTAPAI com parâmetros arquivocontasERP e arquivoestruturaDRE; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONTA` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `VBI_CONTA.SEQCONTA` |
| 2 | `SEQCONTA1` | `numeric(18,0)` | sim | - |  |
| 3 | `SEQCONTA2` | `numeric(18,0)` | sim | - |  |
| 4 | `SEQCONTA3` | `numeric(18,0)` | sim | - |  |
| 5 | `SEQCONTA4` | `numeric(18,0)` | sim | - |  |
| 6 | `SEQCONTA5` | `numeric(18,0)` | sim | - |  |

---

### VBI_CONTAPAI

`classe: nucleo` · `18 colunas` · `243 linhas (snapshot 03/06/2026)` · `PK: SEQCONTA`

**Funcao:** VBI_CONTA/VBI_CONTAFAM/VBI_CONTAPAI com parâmetros arquivocontasERP e arquivoestruturaDRE; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONTA` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `GRUPOGESTAO` | `varchar(20)` | **nao** | - |  |
| 3 | `NIVEL` | `numeric(2,0)` | **nao** | - |  |
| 4 | `NROORDEM` | `numeric(4,0)` | **nao** | - |  |
| 5 | `DESCRICAO` | `varchar(40)` | **nao** | - | descricao do registro |
| 6 | `CODCONTA` | `varchar(20)` | **nao** | - |  |
| 7 | `IDENTIFICADOR` | `varchar(20)` | sim | - |  |
| 8 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 9 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 10 | `SEQCONTAPAI` | `numeric(18,0)` | sim | - |  |
| 11 | `OBS` | `varchar(250)` | sim | - | texto livre |
| 12 | `TIPO` | `char(1)` | sim | - |  |
| 13 | `CORRGB` | `varchar(12)` | sim | - |  |
| 14 | `CORRGBLETRA` | `varchar(12)` | sim | - |  |
| 15 | `NEGRITO` | `numeric(1,0)` | sim | - |  |
| 16 | `FONTEESTILO` | `char(1)` | sim | - |  |
| 17 | `ALINHAMENTO` | `char(1)` | sim | - |  |
| 18 | `TAMANHO` | `char(1)` | sim | - |  |

**Referenciada por:** `VBI_CONTA.SEQCONTAPAI`

---
