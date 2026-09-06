# IVT - Tabela de-para de integracao

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**1 tabelas · 8 colunas · 12 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IVT_DePara`](#ivt_depara) | isolada | 8 | 12 |

---

### IVT_DePara

`classe: isolada` · `8 colunas` · `12 linhas (snapshot 03/06/2026)` · `PK: SEQ`

**Funcao:** Arquitetura de integração em três camadas: 132 eventos externos configurados que traduzem fato do ERP em andamento/processo/ação; 23 tabelas de staging IMP_; 46 tabelas consolidadas EXT_; DE-PARA de códigos. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQ` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tipo` | `varchar(20)` | **nao** | - |  |
| 3 | `Nr1De` | `numeric(18,0)` | sim | - |  |
| 4 | `Nr1Para` | `numeric(18,0)` | **nao** | - |  |
| 5 | `Nr2PAra` | `numeric(18,0)` | sim | - |  |
| 6 | `Nr2De` | `numeric(18,0)` | sim | - |  |
| 7 | `Obs` | `varchar(40)` | sim | - | texto livre |
| 8 | `Ativo` | `numeric(1,0)` | sim | - |  |

---
