# GEL - Logistica: CEP, cidades e conversoes

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**6 tabelas · 46 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`GEL_CEP`](#gel_cep) | vazia | 14 | 0 |
| [`GEL_CepAlerta`](#gel_cepalerta) | vazia | 4 | 0 |
| [`GEL_CepOrig`](#gel_ceporig) | vazia | 10 | 0 |
| [`GEL_Cidade`](#gel_cidade) | vazia | 12 | 0 |
| [`GEL_Conv`](#gel_conv) | vazia | 2 | 0 |
| [`GEL_ConvDe`](#gel_convde) | vazia | 4 | 0 |

---

### GEL_CEP

`classe: vazia` · `14 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Cep`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEL` (logistica/CEP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Cep` | `varchar(10)` | **nao** | - | **PK**; FK -> `GEL_CepOrig.Cep` |
| 2 | `TipoCEP` | `char(1)` | sim | - |  |
| 3 | `SeqCidade` | `decimal(6,0)` | sim | - | FK -> `GEL_Cidade.SeqCidade` |
| 4 | `LogrFonetica` | `varchar(150)` | **nao** | - |  |
| 5 | `TipoLograd` | `varchar(10)` | sim | - |  |
| 6 | `Logradouro` | `varchar(100)` | sim | - |  |
| 7 | `Cidade` | `varchar(100)` | sim | - |  |
| 8 | `CidadeFonetica` | `varchar(100)` | sim | - |  |
| 9 | `Bairro` | `varchar(100)` | sim | - |  |
| 10 | `Uf` | `varchar(2)` | sim | - |  |
| 11 | `Pais` | `varchar(25)` | sim | - |  |
| 12 | `CepCidade` | `numeric(1,0)` | sim | - |  |
| 13 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 14 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |

**Referenciada por:** `GEL_CepAlerta.Cep`

---

### GEL_CepAlerta

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Cep`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEL` (logistica/CEP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Cep` | `varchar(10)` | **nao** | - | **PK**; FK -> `GEL_CEP.Cep` |
| 2 | `Alerta` | `varchar(200)` | sim | - |  |
| 3 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |

---

### GEL_CepOrig

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Cep`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEL` (logistica/CEP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Cep` | `varchar(10)` | **nao** | - | **PK** |
| 2 | `TipoCEP` | `varchar(1)` | sim | - |  |
| 3 | `TipoLogradouro` | `varchar(30)` | sim | - |  |
| 4 | `Logradouro` | `varchar(100)` | sim | - |  |
| 5 | `Descricao` | `varchar(250)` | sim | - | descricao do registro |
| 6 | `Complemento` | `varchar(150)` | sim | - |  |
| 7 | `Bairro` | `varchar(100)` | sim | - |  |
| 8 | `Cidade` | `varchar(100)` | sim | - |  |
| 9 | `SeqCidade` | `numeric(18,0)` | sim | - |  |
| 10 | `Uf` | `varchar(2)` | sim | - |  |

**Referenciada por:** `GEL_CEP.Cep`

---

### GEL_Cidade

`classe: vazia` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCidade`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GEL` (logistica/CEP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCidade` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Cidade` | `varchar(100)` | sim | - |  |
| 3 | `Uf` | `varchar(2)` | **nao** | - |  |
| 4 | `Regiao` | `varchar(12)` | sim | - |  |
| 5 | `Populacao` | `decimal(8,0)` | sim | - |  |
| 6 | `DtaAniversario` | `datetime` | sim | - |  |
| 7 | `DtaFeriado1` | `datetime` | sim | - |  |
| 8 | `DescFeriado1` | `varchar(20)` | sim | - |  |
| 9 | `DtaFeriado2` | `datetime` | sim | - |  |
| 10 | `DescFeriado2` | `varchar(20)` | sim | - |  |
| 11 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 12 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |

**Referenciada por:** `GEL_CEP.SeqCidade`

---

### GEL_Conv

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqConv`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de conversao/de-para, no modulo `GEL` (logistica/CEP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqConv` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `ConvPara` | `varchar(10)` | **nao** | - |  |

**Referenciada por:** `GEL_ConvDe.SeqConv`

---

### GEL_ConvDe

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqConv, SeqConvDe`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqConv` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `GEL_Conv.SeqConv` |
| 2 | `SeqConvDe` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `Nivel` | `decimal(1,0)` | sim | - |  |
| 4 | `ConvDe` | `varchar(30)` | sim | - |  |

---
