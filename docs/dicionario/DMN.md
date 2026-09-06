# DMN - Gestao documental (Doc Manager)

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**9 tabelas · 73 colunas · 325.959 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`DMN_Doc`](#dmn_doc) | nucleo | 17 | 68.609 |
| [`DMN_DocArq`](#dmn_docarq) | vazia | 5 | 0 |
| [`DMN_DocHst`](#dmn_dochst) | isolada | 6 | 183.667 |
| [`DMN_DocObs`](#dmn_docobs) | vazia | 2 | 0 |
| [`DMN_DocPes`](#dmn_docpes) | nucleo | 4 | 73.576 |
| [`DMN_DocProj`](#dmn_docproj) | vazia | 4 | 0 |
| [`DMN_DocProp`](#dmn_docprop) | vazia | 4 | 0 |
| [`DMN_DocTp`](#dmn_doctp) | isolada | 24 | 107 |
| [`DMN_DocVrs`](#dmn_docvrs) | vazia | 7 | 0 |

---

### DMN_Doc

`classe: nucleo` · `17 colunas` · `68.609 linhas (snapshot 03/06/2026)` · `PK: SeqDocto`

**Funcao:** Gestão documental com 107 tipos de documento politicamente configurados, 68.609 documentos, check-in/check-out, versionamento, validade e transporte por FTP; 76.603 vínculos com processo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqDocTp` | `decimal(4,0)` | sim | - |  |
| 3 | `SeqVrs` | `decimal(4,0)` | **nao** | - |  |
| 4 | `Descr` | `varchar(100)` | sim | - |  |
| 5 | `Arq` | `varchar(250)` | sim | - |  |
| 6 | `Ext` | `varchar(5)` | sim | - |  |
| 7 | `DtaBase` | `datetime` | sim | - |  |
| 8 | `Validade` | `datetime` | sim | - |  |
| 9 | `DtaUltVrs` | `datetime` | sim | - |  |
| 10 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 11 | `DtaCkIn` | `datetime` | sim | - |  |
| 12 | `UsuCkIn` | `varchar(20)` | sim | - |  |
| 13 | `DtaCkOut` | `datetime` | sim | - |  |
| 14 | `UsuCkOut` | `varchar(20)` | sim | - |  |
| 15 | `QtdeCons` | `decimal(6,0)` | sim | - |  |
| 16 | `UltCons` | `datetime` | sim | - |  |
| 17 | `Obs` | `varchar(50)` | sim | - | texto livre |

**Referenciada por:** `DMN_DocArq.SeqDocto`, `DMN_DocObs.SeqDocto`, `DMN_DocProj.SeqDocto`, `DMN_DocProp.SeqDocto`, `DMN_DocVrs.SeqDocto`, `IV_ProjDocto.SeqDocto`

---

### DMN_DocArq

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqDocto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de arquivo binario anexado, no modulo `DMN` (gestao documental). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `DMN_Doc.SeqDocto`; documento (`DMN_Doc.SeqDocto`) |
| 2 | `Bloco` | `varchar(10)` | sim | - |  |
| 3 | `Sala` | `varchar(10)` | sim | - |  |
| 4 | `Endereco` | `varchar(20)` | sim | - |  |
| 5 | `Obs` | `varchar(50)` | sim | - | texto livre |

---

### DMN_DocHst

`classe: isolada` · `6 colunas` · `183.667 linhas (snapshot 03/06/2026)` · `PK: SeqDocto, SeqDH`

**Funcao:** Gestão documental. DMN_Doc 71.400 documentos, DMN_DocPes 76.520 vínculos com pessoa, DMN_DocHst 189.393 eventos. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; documento (`DMN_Doc.SeqDocto`) |
| 2 | `SeqDH` | `decimal(6,0)` | **nao** | - | **PK** |
| 3 | `DtaMovto` | `datetime` | sim | - |  |
| 4 | `TipoMov` | `varchar(10)` | sim | - |  |
| 5 | `Descr` | `varchar(100)` | sim | - |  |
| 6 | `Usuario` | `varchar(20)` | sim | - |  |

---

### DMN_DocObs

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqDocto`

**Funcao:** _(inferido)_ Pelo nome, e um bloco de observacoes, no modulo `DMN` (gestao documental). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `DMN_Doc.SeqDocto`; documento (`DMN_Doc.SeqDocto`) |
| 2 | `Obs` | `text(2147483647)` | sim | - | texto livre |

---

### DMN_DocPes

`classe: nucleo` · `4 colunas` · `73.576 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqDocto`

**Funcao:** Gestão documental. DMN_Doc 71.400 documentos, DMN_DocPes 76.520 vínculos com pessoa, DMN_DocHst 189.393 eventos. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; documento (`DMN_Doc.SeqDocto`) |
| 3 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 4 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |

---

### DMN_DocProj

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProjeto, SeqDocto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de projeto, no modulo `DMN` (gestao documental). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProjeto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Projeto.SeqProjeto`; projeto (`IV_Projeto`) |
| 2 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `DMN_Doc.SeqDocto`; documento (`DMN_Doc.SeqDocto`) |
| 3 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 4 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |

---

### DMN_DocProp

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPropPessoa, SeqDocto`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPropPessoa` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_ClientePropr.SeqPropPessoa` |
| 2 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `DMN_Doc.SeqDocto`; documento (`DMN_Doc.SeqDocto`) |
| 3 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 4 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |

---

### DMN_DocTp

`classe: isolada` · `24 colunas` · `107 linhas (snapshot 03/06/2026)` · `PK: SeqDocTp`

**Funcao:** Gestão documental com 107 tipos de documento politicamente configurados, 68.609 documentos, check-in/check-out, versionamento, validade e transporte por FTP; 76.603 vínculos com processo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDocTp` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `DocTp` | `varchar(15)` | **nao** | - |  |
| 3 | `Descr` | `varchar(50)` | sim | - |  |
| 4 | `QtdDiaProcEncer` | `numeric(5,0)` | sim | - |  |
| 5 | `QtdDiaValidade` | `decimal(4,0)` | sim | - |  |
| 6 | `ExigArq` | `numeric(1,0)` | sim | - |  |
| 7 | `ExigAutent` | `numeric(1,0)` | sim | - |  |
| 8 | `ExigDtaBase` | `numeric(1,0)` | sim | - |  |
| 9 | `ExigValidade` | `numeric(1,0)` | sim | - |  |
| 10 | `Formato` | `varchar(3)` | sim | - |  |
| 11 | `VincPessoa` | `numeric(1,0)` | sim | - |  |
| 12 | `VincProcesso` | `numeric(1,0)` | sim | - |  |
| 13 | `VincPropriedade` | `numeric(1,0)` | sim | - |  |
| 14 | `VincProjeto` | `numeric(1,0)` | sim | - |  |
| 15 | `UmPorPessoa` | `numeric(1,0)` | sim | - |  |
| 16 | `UmPorProcesso` | `numeric(1,0)` | sim | - |  |
| 17 | `UmPorPropriedade` | `numeric(1,0)` | sim | - |  |
| 18 | `UmPorProjeto` | `numeric(1,0)` | sim | - |  |
| 19 | `TamanhoMax` | `decimal(8,0)` | sim | - |  |
| 20 | `TrocaPropriedade` | `char(1)` | sim | - |  |
| 21 | `REFPESSOA` | `numeric(1,0)` | sim | - |  |
| 22 | `VINCOS` | `numeric(1,0)` | sim | - |  |
| 23 | `UMPOROS` | `numeric(1,0)` | sim | - |  |
| 24 | `EXTENSOES` | `varchar(80)` | sim | - |  |

---

### DMN_DocVrs

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqDocto, SeqVrs`

**Funcao:** Gestão documental com 107 tipos de documento politicamente configurados, 68.609 documentos, check-in/check-out, versionamento, validade e transporte por FTP; 76.603 vínculos com processo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `DMN_Doc.SeqDocto`; documento (`DMN_Doc.SeqDocto`) |
| 2 | `SeqVrs` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Arq` | `varchar(150)` | sim | - |  |
| 4 | `Ext` | `varchar(5)` | sim | - |  |
| 5 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 6 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 7 | `Obs` | `varchar(100)` | sim | - | texto livre |

---
