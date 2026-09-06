# IVS - Segmentacao, carteira e RFV

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**27 tabelas · 255 colunas · 335.407 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`ivs_callcenter`](#ivs_callcenter) | isolada | 2 | 3.475 |
| [`ivs_callcidades`](#ivs_callcidades) | isolada | 3 | 83 |
| [`IVS_CALL_FORAREGIAO`](#ivs_call_foraregiao) | isolada | 1 | 2.360 |
| [`IVS_CanalVenda`](#ivs_canalvenda) | vazia | 7 | 0 |
| [`IVS_CartCategoria`](#ivs_cartcategoria) | vazia | 2 | 0 |
| [`IVS_CartCid`](#ivs_cartcid) | nucleo | 4 | 670 |
| [`IVS_CartDepto`](#ivs_cartdepto) | nucleo | 4 | 205 |
| [`IVS_Carteira`](#ivs_carteira) | catalogo | 16 | 655 |
| [`IVS_Categoria`](#ivs_categoria) | vazia | 3 | 0 |
| [`IVS_Depto`](#ivs_depto) | catalogo | 29 | 29 |
| [`IVS_DeptoDePara`](#ivs_deptodepara) | vazia | 2 | 0 |
| [`IVS_DeptoEmpr`](#ivs_deptoempr) | nucleo | 5 | 13 |
| [`IVS_DEPTOPOT`](#ivs_deptopot) | nucleo | 8 | 126 |
| [`IVS_DeptoRes`](#ivs_deptores) | nucleo | 7 | 63 |
| [`IVS_NegCategoria`](#ivs_negcategoria) | vazia | 2 | 0 |
| [`IVS_Negocio`](#ivs_negocio) | vazia | 9 | 0 |
| [`IVS_Pes`](#ivs_pes) | nucleo | 30 | 138.584 |
| [`IVS_PES_MAQ_PECAS`](#ivs_pes_maq_pecas) | isolada | 6 | 1.365 |
| [`IVS_PES_NELSON`](#ivs_pes_nelson) | lixo/backup | 4 | 134.872 |
| [`IVS_PES_PNEUS`](#ivs_pes_pneus) | isolada | 6 | 1.313 |
| [`IVS_Pes_RAO_Pneus_02_03`](#ivs_pes_rao_pneus_02_03) | isolada | 28 | 18.261 |
| [`IVS_Regional`](#ivs_regional) | vazia | 8 | 0 |
| [`IVS_Segm`](#ivs_segm) | catalogo | 5 | 8 |
| [`IVS_TGLCLICLIENT`](#ivs_tglcliclient) | isolada | 25 | 16.654 |
| [`IVS_TGLCLIFIS`](#ivs_tglclifis) | isolada | 16 | 11.232 |
| [`IVS_TGLCLIJUR`](#ivs_tglclijur) | isolada | 10 | 5.438 |
| [`IVS_UsrMeta`](#ivs_usrmeta) | nucleo | 13 | 1 |

---

### ivs_callcenter

`classe: isolada` · `2 colunas` · `3.475 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `POTENCIAL` | `varchar(3)` | sim | - |  |

---

### ivs_callcidades

`classe: isolada` · `3 colunas` · `83 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `IVS` (segmentacao/carteira).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `cidade` | `varchar(50)` | sim | - |  |
| 2 | `cen` | `varchar(50)` | sim | - |  |
| 3 | `cenok` | `varchar(50)` | sim | - |  |

---

### IVS_CALL_FORAREGIAO

`classe: isolada` · `1 colunas` · `2.360 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `IVS` (segmentacao/carteira). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPESSOA` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |

---

### IVS_CanalVenda

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCanal`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de canal de venda, no modulo `IVS` (segmentacao/carteira). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCanal` | `decimal(2,0)` | **nao** | - | **PK** |
| 2 | `Canal` | `varchar(12)` | sim | - |  |
| 3 | `SeqCanalPreco` | `decimal(2,0)` | sim | - |  |
| 4 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 5 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 6 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IVP_TabPreco.SeqCanal`

---

### IVS_CartCategoria

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCARTEIRA, SeqCategoria`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IVS` (segmentacao/carteira). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCARTEIRA` | `decimal(6,0)` | **nao** | - | **PK**; carteira (`IVS_Carteira`) |
| 2 | `SeqCategoria` | `decimal(4,0)` | **nao** | - | **PK** |

---

### IVS_CartCid

`classe: nucleo` · `4 colunas` · `670 linhas (snapshot 03/06/2026)` · `PK: SeqCarteira, SeqCidade`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCarteira` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVS_Carteira.SeqCarteira`; carteira (`IVS_Carteira`) |
| 2 | `SeqCidade` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_Cidade.SeqCidade` |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IVS_CartDepto

`classe: nucleo` · `4 colunas` · `205 linhas (snapshot 03/06/2026)` · `PK: SeqCarteira, SeqDepto`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCarteira` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVS_Carteira.SeqCarteira`; carteira (`IVS_Carteira`) |
| 2 | `SeqDepto` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVS_Depto.SeqDepto`; departamento |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IVS_Carteira

`classe: catalogo` · `16 colunas` · `655 linhas (snapshot 03/06/2026)` · `PK: SeqCarteira`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCarteira` | `decimal(4,0)` | **nao** | - | **PK**; carteira (`IVS_Carteira`) |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 3 | `Carteira` | `varchar(15)` | sim | - |  |
| 4 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 5 | `SeqUsrResp` | `numeric(18,0)` | sim | - |  |
| 6 | `SeqUrSuperv` | `numeric(18,0)` | sim | - |  |
| 7 | `SeqVendedor` | `numeric(18,0)` | sim | - | vendedor (`IV_VENDEDOR.SEQVENDEDOR`) |
| 8 | `SeqCanal` | `decimal(2,0)` | sim | - |  |
| 9 | `SeqRegional` | `numeric(8,0)` | sim | - |  |
| 10 | `SeqGrupoPreco` | `decimal(6,0)` | sim | - |  |
| 11 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 12 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 13 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 14 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 15 | `AUTOSINCCID` | `numeric(1,0)` | sim | - |  |
| 16 | `AUTOSINCSTATUS` | `varchar(20)` | sim | - |  |

**Referenciada por:** `IVS_CartCid.SeqCarteira`, `IVS_CartDepto.SeqCarteira`

---

### IVS_Categoria

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCategoria`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de categoria / linha de negocio, no modulo `IVS` (segmentacao/carteira). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCategoria` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `Categoria` | `varchar(20)` | sim | - |  |
| 3 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |

---

### IVS_Depto

`classe: catalogo` · `29 colunas` · `29 linhas (snapshot 03/06/2026)` · `PK: SeqDepto`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDepto` | `decimal(4,0)` | **nao** | - | **PK**; departamento |
| 2 | `Depto` | `varchar(12)` | sim | - |  |
| 3 | `Descricao` | `varchar(30)` | sim | - | descricao do registro |
| 4 | `SeqUsrDirDepto` | `numeric(18,0)` | sim | - | FK -> `GE_Usuario.SeqUsuario` |
| 5 | `SeqSegm` | `decimal(8,0)` | **nao** | - | FK -> `IVS_Segm.SeqSegm` |
| 6 | `CtrlPorEmpresa` | `numeric(1,0)` | sim | - |  |
| 7 | `MultCarteira` | `numeric(1,0)` | sim | - |  |
| 8 | `CodProcesso` | `decimal(4,0)` | sim | - | FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 9 | `CicloA` | `decimal(3,0)` | sim | - |  |
| 10 | `CicloB` | `decimal(3,0)` | sim | - |  |
| 11 | `CicloC` | `decimal(3,0)` | sim | - |  |
| 12 | `CicloD` | `decimal(3,0)` | sim | - |  |
| 13 | `CicloN` | `decimal(3,0)` | sim | - |  |
| 14 | `CicloP` | `decimal(3,0)` | sim | - |  |
| 15 | `CicloAExt` | `decimal(3,0)` | sim | - |  |
| 16 | `CicloBExt` | `decimal(3,0)` | sim | - |  |
| 17 | `CicloCExt` | `decimal(3,0)` | sim | - |  |
| 18 | `CicloDExt` | `decimal(3,0)` | sim | - |  |
| 19 | `AcaoContato` | `numeric(8,0)` | sim | - |  |
| 20 | `AcaoExterna` | `numeric(8,0)` | sim | - |  |
| 21 | `SeqDeptoPreco` | `decimal(4,0)` | sim | - |  |
| 22 | `SeqNegocio` | `decimal(4,0)` | sim | - |  |
| 23 | `CICLOE` | `numeric(3,0)` | sim | - |  |
| 24 | `CICLOEEXT` | `numeric(3,0)` | sim | - |  |
| 25 | `INDUSAPOTZ` | `numeric(1,0)` | sim | - |  |
| 26 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 27 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 28 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 29 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IVP_TabPreco.SeqDepto`, `IVS_CartDepto.SeqDepto`, `IVS_DeptoDePara.SeqDepto`, `IVS_DeptoEmpr.SeqDepto`, `IVS_DeptoRes.SeqDepto`, `IVS_Pes.SeqDepto`, `IV_ProcDado.SeqDepto`

---

### IVS_DeptoDePara

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqDepto, Descricao`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para relacionada a departamento, no modulo `IVS` (segmentacao/carteira). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDepto` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVS_Depto.SeqDepto`; departamento |
| 2 | `Descricao` | `varchar(40)` | **nao** | - | **PK**; descricao do registro |

---

### IVS_DeptoEmpr

`classe: nucleo` · `5 colunas` · `13 linhas (snapshot 03/06/2026)` · `PK: SeqDepto, NroEmpresa`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDepto` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVS_Depto.SeqDepto`; departamento |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `SeqUsrGerDepto` | `numeric(18,0)` | sim | - | FK -> `GE_Usuario.SeqUsuario` |
| 4 | `SeqUsrDirDepto` | `numeric(18,0)` | sim | - | FK -> `GE_Usuario.SeqUsuario` |
| 5 | `SEQUSUARIO` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |

---

### IVS_DEPTOPOT

`classe: nucleo` · `8 colunas` · `126 linhas (snapshot 03/06/2026)` · `PK: SEQPOTENCIALDP`

**Funcao:** A segmentação que REALMENTE funciona: potencial do cliente POR DEPARTAMENTO, com SLA de cadência. 126 linhas. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPOTENCIALDP` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `SEQDEPTO` | `numeric(4,0)` | **nao** | - | departamento |
| 3 | `POTENCIAL` | `varchar(12)` | **nao** | - |  |
| 4 | `ORDEM` | `numeric(2,0)` | **nao** | - |  |
| 5 | `DIASCICLOCTTO` | `numeric(4,0)` | sim | - |  |
| 6 | `DIASCICLOVISITA` | `numeric(4,0)` | sim | - |  |
| 7 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 8 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |

**Referenciada por:** `IVS_Pes.SEQPOTENCIALDP`

---

### IVS_DeptoRes

`classe: nucleo` · `7 colunas` · `63 linhas (snapshot 03/06/2026)` · `PK: SeqDepto, Resultado`

**Funcao:** `DtaUltCtto` só é preenchido para resultados marcados em `IVS_DeptoRes` como "conta como contato". (fonte: `16-vortice-ponta-a-ponta-ciclo-de-vida-pessoa-processo-agenda-historico.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqDepto` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVS_Depto.SeqDepto`; departamento |
| 2 | `Resultado` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 3 | `IndContato` | `numeric(1,0)` | **nao** | - |  |
| 4 | `IndExterno` | `numeric(1,0)` | sim | - |  |
| 5 | `IndExtFormaCtto` | `numeric(1,0)` | sim | - |  |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IVS_NegCategoria

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqNegocio, SeqCategoria`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de categoria / linha de negocio, no modulo `IVS` (segmentacao/carteira). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqNegocio` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `SeqCategoria` | `decimal(4,0)` | **nao** | - | **PK** |

---

### IVS_Negocio

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqNegocio`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de categoria / linha de negocio, no modulo `IVS` (segmentacao/carteira). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqNegocio` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `Negocio` | `varchar(20)` | sim | - |  |
| 3 | `Sigla` | `varchar(4)` | sim | - |  |
| 4 | `Descricao` | `varchar(100)` | sim | - | descricao do registro |
| 5 | `SeqUsrResp` | `numeric(18,0)` | sim | - |  |
| 6 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 7 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 8 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 9 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IVS_Pes

`classe: nucleo` · `30 colunas` · `138.584 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqDepto, SeqPesDepto`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqDepto` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVS_Depto.SeqDepto`; departamento |
| 3 | `SeqPesDepto` | `numeric(10,0)` | **nao** | - | **PK** |
| 4 | `SeqCarteira` | `decimal(4,0)` | **nao** | - | carteira (`IVS_Carteira`) |
| 5 | `Ciclo` | `decimal(3,0)` | sim | - |  |
| 6 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 7 | `Situacao` | `varchar(2)` | sim | - |  |
| 8 | `Potencial` | `varchar(3)` | sim | - |  |
| 9 | `Perspectiva` | `decimal(2,0)` | sim | - |  |
| 10 | `DtaCalc` | `datetime` | sim | - |  |
| 11 | `Rec` | `decimal(1,0)` | sim | - |  |
| 12 | `Freq` | `numeric(1,0)` | sim | - |  |
| 13 | `Vlr` | `numeric(1,0)` | sim | - |  |
| 14 | `Pto` | `numeric(1,0)` | sim | - |  |
| 15 | `QtdTrans` | `numeric(18,0)` | sim | - |  |
| 16 | `ProcUltTrans` | `numeric(18,0)` | sim | - |  |
| 17 | `DtaIniTrans` | `datetime` | sim | - |  |
| 18 | `DtaUltTrans` | `datetime` | sim | - |  |
| 19 | `DtaUltCtto` | `datetime` | sim | - |  |
| 20 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 21 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 22 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 23 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 24 | `DtaUltCttoExt` | `datetime` | sim | - |  |
| 25 | `CicloExt` | `decimal(3,0)` | sim | - |  |
| 26 | `Score` | `decimal(2,0)` | sim | - |  |
| 27 | `Classe` | `varchar(30)` | sim | - |  |
| 28 | `Recalc` | `numeric(1,0)` | sim | - |  |
| 29 | `SEQPOTENCIALDP` | `numeric(6,0)` | sim | - | FK -> `IVS_DEPTOPOT.SEQPOTENCIALDP` |
| 30 | `INDTROCAPORCIDBLOQ` | `numeric(1,0)` | sim | - |  |

---

### IVS_PES_MAQ_PECAS

`classe: isolada` · `6 colunas` · `1.365 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IVS` (segmentacao/carteira). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `varchar(50)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `Carteira` | `varchar(50)` | sim | - |  |
| 3 | `Departamento` | `varchar(50)` | sim | - | departamento |
| 4 | `CONSULTOR` | `varchar(70)` | sim | - |  |
| 5 | `Seqcarteira` | `varchar(20)` | sim | - | carteira (`IVS_Carteira`) |
| 6 | `SeqDepto` | `varchar(20)` | sim | - | departamento |

---

### IVS_PES_NELSON

`classe: lixo/backup` · `4 colunas` · `134.872 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IVS` (segmentacao/carteira). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_NELSON`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (4 colunas) para manter o documento legivel._

---

### IVS_PES_PNEUS

`classe: isolada` · `6 colunas` · `1.313 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IVS` (segmentacao/carteira). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `varchar(50)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `Carteira` | `varchar(50)` | sim | - |  |
| 3 | `Departamento` | `varchar(50)` | sim | - | departamento |
| 4 | `CONSULTOR` | `varchar(50)` | sim | - |  |
| 5 | `Seqcarteira` | `varchar(20)` | sim | - | carteira (`IVS_Carteira`) |
| 6 | `SeqDepto` | `varchar(20)` | sim | - | departamento |

---

### IVS_Pes_RAO_Pneus_02_03

`classe: isolada` · `28 colunas` · `18.261 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IVS` (segmentacao/carteira). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqDepto` | `decimal(4,0)` | **nao** | - | departamento |
| 3 | `SeqPesDepto` | `numeric(10,0)` | **nao** | - |  |
| 4 | `SeqCarteira` | `decimal(4,0)` | **nao** | - | carteira (`IVS_Carteira`) |
| 5 | `Ciclo` | `decimal(3,0)` | sim | - |  |
| 6 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 7 | `Situacao` | `varchar(2)` | sim | - |  |
| 8 | `Potencial` | `varchar(3)` | sim | - |  |
| 9 | `Perspectiva` | `decimal(2,0)` | sim | - |  |
| 10 | `DtaCalc` | `datetime` | sim | - |  |
| 11 | `Rec` | `decimal(1,0)` | sim | - |  |
| 12 | `Freq` | `numeric(1,0)` | sim | - |  |
| 13 | `Vlr` | `numeric(1,0)` | sim | - |  |
| 14 | `Pto` | `numeric(1,0)` | sim | - |  |
| 15 | `QtdTrans` | `numeric(18,0)` | sim | - |  |
| 16 | `ProcUltTrans` | `numeric(18,0)` | sim | - |  |
| 17 | `DtaIniTrans` | `datetime` | sim | - |  |
| 18 | `DtaUltTrans` | `datetime` | sim | - |  |
| 19 | `DtaUltCtto` | `datetime` | sim | - |  |
| 20 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 21 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 22 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 23 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 24 | `DtaUltCttoExt` | `datetime` | sim | - |  |
| 25 | `CicloExt` | `decimal(3,0)` | sim | - |  |
| 26 | `Score` | `decimal(2,0)` | sim | - |  |
| 27 | `Classe` | `varchar(30)` | sim | - |  |
| 28 | `Recalc` | `numeric(1,0)` | sim | - |  |

---

### IVS_Regional

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRegional`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRegional` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `Regional` | `varchar(30)` | sim | - |  |
| 3 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 4 | `SeqUsrResp` | `numeric(18,0)` | sim | - |  |
| 5 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 6 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 7 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IVS_Segm

`classe: catalogo` · `5 colunas` · `8 linhas (snapshot 03/06/2026)` · `PK: SeqSegm`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqSegm` | `decimal(8,0)` | **nao** | - | **PK** |
| 2 | `SegmSigla` | `varchar(4)` | sim | - |  |
| 3 | `Segm` | `varchar(14)` | sim | - |  |
| 4 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 5 | `SeqUsrResp` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `IVS_Depto.SeqSegm`

---

### IVS_TGLCLICLIENT

`classe: isolada` · `25 colunas` · `16.654 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CLIENTNRO` | `int(10,0)` | sim | - |  |
| 2 | `CLIENTNOMSIGLA` | `varchar(15)` | sim | - |  |
| 3 | `CLIENTNOMRAZAO` | `varchar(60)` | sim | - |  |
| 4 | `CLIENTNOMRUA` | `varchar(60)` | sim | - |  |
| 5 | `CLIENTNROEND` | `int(10,0)` | sim | - |  |
| 6 | `CLIENTDESCOMEND` | `varchar(60)` | sim | - |  |
| 7 | `CLIENTNOMBAIRRO` | `varchar(60)` | sim | - |  |
| 8 | `CLIENTNOMCIDADE` | `varchar(60)` | sim | - |  |
| 9 | `CLIENTCODCEP` | `int(10,0)` | sim | - |  |
| 10 | `ESTADOCOD` | `varchar(2)` | sim | - |  |
| 11 | `CLIENTNOMPAIS` | `varchar(15)` | sim | - |  |
| 12 | `CLIENTNRODDD` | `int(10,0)` | sim | - |  |
| 13 | `CLIENTNROTEL` | `int(10,0)` | sim | - |  |
| 14 | `CLIENTNROALTDDD` | `int(10,0)` | sim | - |  |
| 15 | `CLIENTNROTELALT` | `varchar(20)` | sim | - |  |
| 16 | `CLIENTNROFAXDDD` | `int(10,0)` | sim | - |  |
| 17 | `CLIENTNROFAX` | `varchar(20)` | sim | - |  |
| 18 | `CLIENTDATCAD` | `varchar(10)` | sim | - |  |
| 19 | `GRUCLICOD` | `varchar(3)` | sim | - |  |
| 20 | `REGIAOCOD` | `varchar(3)` | sim | - |  |
| 21 | `CLIENTIDTFISJUR` | `char(1)` | sim | - |  |
| 22 | `FORNECNRO` | `int(10,0)` | sim | - |  |
| 23 | `DTIMPORT` | `datetime` | sim | - |  |
| 24 | `USUIMPORT` | `varchar(20)` | sim | - |  |
| 25 | `DTREFERENCIA` | `datetime` | sim | - |  |

---

### IVS_TGLCLIFIS

`classe: isolada` · `16 colunas` · `11.232 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CLIENTNRO` | `int(10,0)` | sim | - |  |
| 2 | `CLIFISNROCPF` | `numeric(11,0)` | sim | - |  |
| 3 | `CLIFISNROID` | `varchar(15)` | sim | - |  |
| 4 | `CLIFISNOMORGID` | `varchar(10)` | sim | - |  |
| 5 | `ATPEFICOD` | `varchar(3)` | sim | - |  |
| 6 | `CLIFISNOMEMPRESA` | `varchar(40)` | sim | - |  |
| 7 | `CLIFISNOMCARGO` | `varchar(30)` | sim | - |  |
| 8 | `FUNCAOCOD` | `varchar(3)` | sim | - |  |
| 9 | `CLIFISIDTSEXO` | `varchar(1)` | sim | - |  |
| 10 | `CLIFISDATNASC` | `varchar(15)` | sim | - |  |
| 11 | `RENMENCOD` | `char(1)` | sim | - |  |
| 12 | `CLIFISDESEMAIL` | `varchar(60)` | sim | - |  |
| 13 | `CLIFISDESINSEST` | `varchar(20)` | sim | - |  |
| 14 | `DTIMPORT` | `datetime` | sim | - |  |
| 15 | `USUIMPORT` | `varchar(20)` | sim | - |  |
| 16 | `DTREFERENCIA` | `datetime` | sim | - |  |

---

### IVS_TGLCLIJUR

`classe: isolada` · `10 colunas` · `5.438 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CLIENTNRO` | `int(10,0)` | sim | - |  |
| 2 | `CLIJURNROCGC` | `numeric(15,0)` | sim | - |  |
| 3 | `CLIJURNROINSEST` | `varchar(20)` | sim | - |  |
| 4 | `CLIJURNROINSMUN` | `varchar(20)` | sim | - |  |
| 5 | `CLIJURNROREGJUN` | `varchar(20)` | sim | - |  |
| 6 | `ATPEJUCOD` | `varchar(3)` | sim | - |  |
| 7 | `CLIJURDESEMAIL` | `varchar(60)` | sim | - |  |
| 8 | `DTIMPORT` | `datetime` | sim | - |  |
| 9 | `USUIMPORT` | `varchar(20)` | sim | - |  |
| 10 | `DTREFERENCIA` | `datetime` | sim | - |  |

---

### IVS_UsrMeta

`classe: nucleo` · `13 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SeqUsrMeta`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema, no modulo `IVS` (segmentacao/carteira). As colunas confirmam vinculo com usuario (`SeqUsuario`), escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsrMeta` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CodProcesso` | `decimal(4,0)` | sim | - | FK -> `IV_CodProcesso.CodProcesso`; TIPO de fluxo (41/50) - nao e o numero do processo |
| 3 | `SeqUsuario` | `numeric(18,0)` | sim | - | FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 4 | `DtaReferencia` | `datetime` | sim | - |  |
| 5 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 6 | `QtdProspec` | `decimal(4,0)` | sim | - |  |
| 7 | `QtdContato` | `decimal(4,0)` | sim | - |  |
| 8 | `QtdVenda` | `decimal(4,0)` | sim | - |  |
| 9 | `QtdeDemo` | `decimal(4,0)` | sim | - |  |
| 10 | `VlrVenda` | `decimal(15,2)` | sim | - |  |
| 11 | `Obs` | `varchar(100)` | sim | - | texto livre |
| 12 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 13 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---
