# IVM - Materiais vinculados a processo

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**4 tabelas · 49 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IVM_Material`](#ivm_material) | vazia | 15 | 0 |
| [`IVM_MatPessoa`](#ivm_matpessoa) | vazia | 9 | 0 |
| [`IVM_ProcMat`](#ivm_procmat) | vazia | 11 | 0 |
| [`IVM_ProcMatItem`](#ivm_procmatitem) | vazia | 14 | 0 |

---

### IVM_Material

`classe: vazia` · `15 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqMaterial`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de material, no modulo `IVM` (materiais). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqMaterial` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CodMaterial` | `varchar(30)` | **nao** | - |  |
| 3 | `EmUso` | `varchar(18)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 4 | `Descricao` | `varchar(40)` | **nao** | - | descricao do registro |
| 5 | `Preco1` | `decimal(15,2)` | sim | - |  |
| 6 | `Preco2` | `decimal(15,2)` | sim | - |  |
| 7 | `QtdePadrao` | `decimal(10,2)` | sim | - |  |
| 8 | `QtdeFixa` | `numeric(1,0)` | sim | - |  |
| 9 | `PrecoFixo` | `numeric(1,0)` | sim | - |  |
| 10 | `Custeio` | `varchar(15)` | sim | - |  |
| 11 | `CusteioFixo` | `numeric(1,0)` | sim | - |  |
| 12 | `Atributo1` | `varchar(20)` | sim | - |  |
| 13 | `Atributo2` | `varchar(20)` | sim | - |  |
| 14 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 15 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |

**Referenciada por:** `IVM_MatPessoa.SeqMaterial`, `IVM_ProcMatItem.SeqMaterial`

---

### IVM_MatPessoa

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqMaterial, SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IVM` (materiais). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqMaterial` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IVM_Material.SeqMaterial` |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Preco1` | `decimal(15,2)` | sim | - |  |
| 4 | `Preco2` | `decimal(15,2)` | sim | - |  |
| 5 | `QtdePadrao` | `decimal(10,2)` | sim | - |  |
| 6 | `QtdeFixa` | `numeric(1,0)` | sim | - |  |
| 7 | `PrecoFixo` | `numeric(1,0)` | sim | - |  |
| 8 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 9 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IVM_ProcMat

`classe: vazia` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProcMat`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `IVM` (materiais). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProcMat` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Processo` | `numeric(18,0)` | **nao** | - | FK -> `IV_Processo.Processo`; numero do processo (`IV_Processo.Processo`) |
| 3 | `DtaAplicacao` | `datetime` | sim | - |  |
| 4 | `Tipo` | `varchar(20)` | sim | - |  |
| 5 | `Responsavel` | `varchar(20)` | sim | - |  |
| 6 | `NroDocto` | `varchar(15)` | sim | - |  |
| 7 | `Obs` | `varchar(200)` | sim | - | texto livre |
| 8 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 9 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 10 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 11 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IVM_ProcMatItem.SeqProcMat`

---

### IVM_ProcMatItem

`classe: vazia` · `14 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqMatItem`

**Funcao:** _(inferido)_ Pelo nome, e a filha 1:N de itens relacionada a processo/oportunidade do BPM, no modulo `IVM` (materiais). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqMatItem` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqMaterial` | `numeric(18,0)` | **nao** | - | FK -> `IVM_Material.SeqMaterial` |
| 3 | `SeqProcMat` | `numeric(18,0)` | **nao** | - | FK -> `IVM_ProcMat.SeqProcMat` |
| 4 | `Qtde` | `decimal(10,2)` | sim | - |  |
| 5 | `vlrUnitario` | `decimal(15,2)` | sim | - |  |
| 6 | `DescTotal` | `decimal(15,2)` | sim | - |  |
| 7 | `Obs` | `varchar(200)` | sim | - | texto livre |
| 8 | `UsuAplicou` | `varchar(20)` | sim | - |  |
| 9 | `Custeio` | `varchar(15)` | sim | - |  |
| 10 | `DtaAplicacao` | `datetime` | sim | - |  |
| 11 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 12 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 13 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 14 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---
