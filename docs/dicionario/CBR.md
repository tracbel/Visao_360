# CBR - Cobranca (contas do cliente)

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**1 tabelas · 7 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`CBR_CLIENTECONTA`](#cbr_clienteconta) | vazia | 7 | 0 |

---

### CBR_CLIENTECONTA

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCBRCLIENTECONTA`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de cobranca, no modulo `CBR` (cobranca). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCBRCLIENTECONTA` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IDENTIFICADOR` | `numeric(18,0)` | **nao** | - |  |
| 3 | `CPF` | `numeric(18,0)` | sim | - |  |
| 4 | `STATUS` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 5 | `DIASATRASO` | `numeric(6,0)` | sim | - |  |
| 6 | `SALDOATUAL` | `numeric(14,2)` | sim | - |  |
| 7 | `DTAGERACAO` | `datetime` | sim | - |  |

---
