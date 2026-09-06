# CLARITY - Integracao de telefonia (BINA)

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**1 tabelas · 5 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`CLARITY_BINA`](#clarity_bina) | vazia | 5 | 0 |

---

### CLARITY_BINA

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia. Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `RAMAL` | `varchar(10)` | sim | - |  |
| 2 | `TELEFONE` | `varchar(20)` | sim | - |  |
| 3 | `CLIENTE` | `varchar(20)` | sim | - |  |
| 4 | `CODIGO_EXTERNO` | `varchar(22)` | sim | - |  |
| 5 | `TIPO_LIGACAO` | `varchar(1)` | sim | - |  |

---
