# MIG - Tabelas de migracao

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**2 tabelas · 4 colunas · 28.089 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`mig_ge_pessoa`](#mig_ge_pessoa) | lixo/backup | 2 | 5 |
| [`mig_ge_pessoa_bkp`](#mig_ge_pessoa_bkp) | lixo/backup | 2 | 28.084 |

---

### mig_ge_pessoa

`classe: lixo/backup` · `2 colunas` · `5 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: tabela de migracao pontual (`mig*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (2 colunas) para manter o documento legivel._

---

### mig_ge_pessoa_bkp

`classe: lixo/backup` · `2 colunas` · `28.084 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (2 colunas) para manter o documento legivel._

---
