# LOG - Log de integracao de faturamento

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**1 tabelas · 4 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`LOG_INTEGRACAO_FATURAMENTO_TOTVS`](#log_integracao_faturamento_totvs) | vazia | 4 | 0 |

---

### LOG_INTEGRACAO_FATURAMENTO_TOTVS

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: ID`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a acao (tipo de tarefa). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ID` | `int(10,0)` | **nao** | - | **PK** |
| 2 | `Tabela` | `varchar(250)` | sim | - |  |
| 3 | `MensagemErro` | `nvarchar(4000)` | sim | - |  |
| 4 | `DataHora` | `datetime` | sim | - |  |

---
