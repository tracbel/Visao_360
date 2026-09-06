# IVP - Produtos, tabela de preco e criticas de pedido

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**4 tabelas · 31 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IVP_PedCritica`](#ivp_pedcritica) | vazia | 9 | 0 |
| [`IVP_ProdImagem`](#ivp_prodimagem) | vazia | 4 | 0 |
| [`IVP_TabPreco`](#ivp_tabpreco) | vazia | 7 | 0 |
| [`IVP_Vendedor`](#ivp_vendedor) | vazia | 11 | 0 |

---

### IVP_PedCritica

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPedCritica`

**Funcao:** _(inferido)_ Pelo nome, e um criterio configuravel, no modulo `IVP` (produtos/precos). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPedCritica` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPedido` | `numeric(18,0)` | sim | - |  |
| 3 | `CodCritica` | `varchar(20)` | sim | - |  |
| 4 | `Descricao` | `varchar(150)` | sim | - | descricao do registro |
| 5 | `DtaGeracao` | `datetime` | sim | - |  |
| 6 | `DtaAnalise` | `datetime` | sim | - |  |
| 7 | `UsuAnalise` | `varchar(20)` | sim | - |  |
| 8 | `IndLiberada` | `numeric(1,0)` | sim | - |  |
| 9 | `ObsLiberacao` | `varchar(250)` | sim | - |  |

---

### IVP_ProdImagem

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de produto, no modulo `IVP` (produtos/precos). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProdImagem` | `numeric(18,0)` | **nao** | - |  |
| 2 | `SeqProduto` | `numeric(18,0)` | **nao** | - |  |
| 3 | `Caminho` | `varchar(200)` | sim | - |  |
| 4 | `Obs` | `varchar(100)` | sim | - | texto livre |

---

### IVP_TabPreco

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProdPreco`

**Funcao:** IVP_TabPreco.Tabela+SeqCanal+SeqDepto+VigorDe; (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProdPreco` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tabela` | `varchar(30)` | sim | - |  |
| 3 | `NroEmpresa` | `decimal(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `SeqGrupoPreco` | `decimal(6,0)` | **nao** | - |  |
| 5 | `SeqCanal` | `decimal(2,0)` | **nao** | - | FK -> `IVS_CanalVenda.SeqCanal` |
| 6 | `SeqDepto` | `decimal(4,0)` | **nao** | - | FK -> `IVS_Depto.SeqDepto`; departamento |
| 7 | `VigorDe` | `datetime` | sim | - |  |

---

### IVP_Vendedor

`classe: vazia` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqVendedor`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de vendedor/CEN, no modulo `IVP` (produtos/precos). As colunas confirmam vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqVendedor` | `numeric(18,0)` | **nao** | - | **PK**; vendedor (`IV_VENDEDOR.SEQVENDEDOR`) |
| 2 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 3 | `Equipe` | `varchar(20)` | sim | - |  |
| 4 | `CodVendedor` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 5 | `Nome` | `varchar(40)` | sim | - |  |
| 6 | `NroEmprPadrao` | `numeric(6,0)` | sim | - |  |
| 7 | `SalarioFixo` | `decimal(14,2)` | sim | - |  |
| 8 | `Comissao1` | `decimal(8,4)` | sim | - |  |
| 9 | `Comissao2` | `decimal(8,4)` | sim | - |  |
| 10 | `Obs` | `varchar(200)` | sim | - | texto livre |
| 11 | `SeqUsuario` | `decimal(8,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |

---
