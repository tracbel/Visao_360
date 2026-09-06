# OUTROS - Tabelas sem prefixo (nome sem `_`)

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**16 tabelas · 96 colunas · 98.881 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`andre`](#andre) | lixo/backup | 1 | 3 |
| [`CONTAS`](#contas) | lixo/backup | 8 | 1.003 |
| [`CONTAS2`](#contas2) | lixo/backup | 7 | 1.003 |
| [`DUAL`](#dual) | lixo/backup | 1 | 1 |
| [`nfsaida$`](#nfsaida) | lixo/backup | 23 | 62.619 |
| [`SYSCONVERT1`](#sysconvert1) | lixo/backup | 2 | 3 |
| [`SYSCONVERT2`](#sysconvert2) | lixo/backup | 2 | 24 |
| [`SYSCONVERT3`](#sysconvert3) | lixo/backup | 2 | 14 |
| [`sysdiagrams`](#sysdiagrams) | lixo/backup | 5 | 0 |
| [`SYSDUMMY`](#sysdummy) | lixo/backup | 2 | 0 |
| [`TEMP`](#temp) | lixo/backup | 3 | 1.026 |
| [`teste`](#teste) | lixo/backup | 1 | 0 |
| [`teste333`](#teste333) | lixo/backup | 11 | 24.024 |
| [`testepiv`](#testepiv) | lixo/backup | 4 | 12 |
| [`usuarios$`](#usuarios) | lixo/backup | 2 | 158 |
| [`ww`](#ww) | lixo/backup | 22 | 8.991 |

---

### andre

`classe: lixo/backup` · `1 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Em .NET 8, isso é Polly com `WaitAndRetryAsync` e jitter. (fonte: `13-extensibilidade-e-automacao-no-dynamics-365-dataverse-power-.md`)

> **Nao usar em producao.** Motivo da classificacao: tabela pessoal (nome de usuario em minusculo) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (1 colunas) para manter o documento legivel._

---

### CONTAS

`classe: lixo/backup` · `8 colunas` · `1.003 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Varejo 85.483, Cliente 24.351, Diversos 3.233, Contas Chave 814, CLIENTES 766 (duplicata de 'Cliente'), Funcionário 461, Concessionária 345, Fornecedor 166, Ex-Funcionário 37, PROSPECT 33, Concorrente 16 — mistura tipo de relacionamento comercial, papel na cadeia e status, e contém 'Teste' 1 e 'string' 1. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

> **Nao usar em producao.** Motivo da classificacao: planilha importada solta (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (8 colunas) para manter o documento legivel._

---

### CONTAS2

`classe: lixo/backup` · `7 colunas` · `1.003 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de plano de contas.

> **Nao usar em producao.** Motivo da classificacao: planilha importada solta (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (7 colunas) para manter o documento legivel._

---

### DUAL

`classe: lixo/backup` · `1 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Tabela de compatibilidade Oracle (1 coluna, 1 linha). Usada pelos relatórios para o preâmbulo procedural: SELECT CASE ... (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

> **Nao usar em producao.** Motivo da classificacao: tabela tecnica de 1 linha (compat. Oracle DUAL) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (1 colunas) para manter o documento legivel._

---

### nfsaida$

`classe: lixo/backup` · `23 colunas` · `62.619 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de nota fiscal. As colunas confirmam escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: importacao de planilha Excel (sufixo `$`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (23 colunas) para manter o documento legivel._

---

### SYSCONVERT1

`classe: lixo/backup` · `2 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: tabela tecnica de conversao (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (2 colunas) para manter o documento legivel._

---

### SYSCONVERT2

`classe: lixo/backup` · `2 colunas` · `24 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: tabela tecnica de conversao (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (2 colunas) para manter o documento legivel._

---

### SYSCONVERT3

`classe: lixo/backup` · `2 colunas` · `14 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: tabela tecnica de conversao (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (2 colunas) para manter o documento legivel._

---

### sysdiagrams

`classe: lixo/backup` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: diagram_id`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: diagramas do SSMS (metadado da ferramenta) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### SYSDUMMY

`classe: lixo/backup` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: tabela tecnica dummy (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (2 colunas) para manter o documento legivel._

---

### TEMP

`classe: lixo/backup` · `3 colunas` · `1.026 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** E modelar a hierarquia como fato TEMPORAL (rep_manager { rep_id, manager_id, valid_from, valid_to }): isso resolve de vez o defeito 2.5 (aprovação foi para o líder antigo) e o 2.6 (troca de usuário quebra o relatório do gerente), porque a atribuição passa a ser resolvida contra a hierarquia vigente e é reprocessável. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

> **Nao usar em producao.** Motivo da classificacao: tabela temporaria (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (3 colunas) para manter o documento legivel._

---

### teste

`classe: lixo/backup` · `1 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Sobre RLS do SQL Server: DESCARTAR na v1 (duplica a regra em dois lugares e complica migration e testes), mas manter no radar como defesa em profundidade para o dia em que o Power BI/Excel acessar o banco direto — porque ai o filtro do EF Core nao existe. (fonte: `08-modelo-de-seguranca-do-salesforce-camadas-ordem-de-avaliacao.md`)

> **Nao usar em producao.** Motivo da classificacao: tabela de teste (`teste*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (1 colunas) para manter o documento legivel._

---

### teste333

`classe: lixo/backup` · `11 colunas` · `24.024 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: tabela de teste (`teste*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (11 colunas) para manter o documento legivel._

---

### testepiv

`classe: lixo/backup` · `4 colunas` · `12 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: tabela de teste (`teste*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (4 colunas) para manter o documento legivel._

---

### usuarios$

`classe: lixo/backup` · `2 colunas` · `158 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema.

> **Nao usar em producao.** Motivo da classificacao: importacao de planilha Excel (sufixo `$`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (2 colunas) para manter o documento legivel._

---

### ww

`classe: lixo/backup` · `22 colunas` · `8.991 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Big Objects via https://www.dataarchiva.com/guide/salesforce-big-objects/ e https://www.saasguru.co/big-objects-in-salesforce/ (índice composto de 5 campos, filtro esquerda-para-direita sem gaps, LIMIT obrigatório 10.000, Async SOQL aposentado no Summer '23) (fonte: `14-plataforma-metadados-ux-e-alm-no-salesforce-licoes-verificad.md`)

> **Nao usar em producao.** Motivo da classificacao: tabela de rascunho (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (22 colunas) para manter o documento legivel._

---
