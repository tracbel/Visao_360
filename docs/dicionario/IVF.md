# IVF - Financiamento, planos e propostas

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**20 tabelas · 240 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IVF_Acordo`](#ivf_acordo) | vazia | 10 | 0 |
| [`IVF_Agregado`](#ivf_agregado) | vazia | 8 | 0 |
| [`IVF_AgregCC`](#ivf_agregcc) | vazia | 8 | 0 |
| [`IVF_Financeira`](#ivf_financeira) | vazia | 23 | 0 |
| [`IVF_FinancEmpr`](#ivf_financempr) | vazia | 4 | 0 |
| [`IVF_FinancImpTx`](#ivf_financimptx) | vazia | 6 | 0 |
| [`IVF_Molicar`](#ivf_molicar) | vazia | 25 | 0 |
| [`IVF_Plano`](#ivf_plano) | vazia | 19 | 0 |
| [`IVF_PlanoIndic`](#ivf_planoindic) | vazia | 7 | 0 |
| [`IVF_Prazo`](#ivf_prazo) | vazia | 3 | 0 |
| [`IVF_Proposta`](#ivf_proposta) | vazia | 60 | 0 |
| [`IVF_PropostaHst`](#ivf_propostahst) | vazia | 7 | 0 |
| [`IVF_PropostaObs`](#ivf_propostaobs) | vazia | 6 | 0 |
| [`IVF_PropostaResult`](#ivf_propostaresult) | vazia | 5 | 0 |
| [`IVF_Tabela`](#ivf_tabela) | vazia | 24 | 0 |
| [`IVF_TabelaFiltro`](#ivf_tabelafiltro) | vazia | 4 | 0 |
| [`IVF_TabEmpr`](#ivf_tabempr) | vazia | 4 | 0 |
| [`IVF_TabIndice`](#ivf_tabindice) | vazia | 6 | 0 |
| [`IVF_TipoAgregado`](#ivf_tipoagregado) | vazia | 6 | 0 |
| [`IVF_TpAgrPessoa`](#ivf_tpagrpessoa) | vazia | 5 | 0 |

---

### IVF_Acordo

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAcordo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de acordo comercial/financeiro, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAcordo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqTabela` | `numeric(18,0)` | **nao** | - | FK -> `IVF_Tabela.SeqTabela` |
| 3 | `Prazo` | `decimal(4,0)` | **nao** | - | FK -> `IVF_Prazo.Prazo` |
| 4 | `VlrProducao` | `decimal(15,2)` | **nao** | - |  |
| 5 | `SimulaProdOk` | `numeric(1,0)` | sim | - |  |
| 6 | `PercPlus` | `decimal(4,2)` | **nao** | - |  |
| 7 | `VlrPlus` | `decimal(15,2)` | **nao** | - |  |
| 8 | `EmUso` | `numeric(1,0)` | **nao** | - | flag de registro/regra ativa (0 = desligada) |
| 9 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 10 | `TipoAcordo` | `varchar(12)` | sim | - |  |

---

### IVF_Agregado

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAgreg`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgreg` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqTpAgreg` | `decimal(4,0)` | **nao** | - | FK -> `IVF_TipoAgregado.SeqTpAgreg`; FK -> `IVF_TpAgrPessoa.SeqTpAgreg` |
| 3 | `SubTipoAgreg` | `varchar(10)` | sim | - |  |
| 4 | `Processo` | `numeric(18,0)` | sim | - | FK -> `IV_FichaNegVeic.Processo`; numero do processo (`IV_Processo.Processo`) |
| 5 | `SeqProposta` | `numeric(18,0)` | sim | - |  |
| 6 | `SeqPessoaRec` | `numeric(8,0)` | **nao** | - | FK -> `IVF_TpAgrPessoa.SeqPessoa` |
| 7 | `Vlr` | `decimal(15,2)` | **nao** | - |  |
| 8 | `Obs` | `varchar(100)` | sim | - | texto livre |

**Referenciada por:** `IVF_AgregCC.SeqAgreg`

---

### IVF_AgregCC

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAgregCC`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgregCC` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqAgreg` | `numeric(18,0)` | **nao** | - | FK -> `IVF_Agregado.SeqAgreg` |
| 3 | `Real` | `numeric(1,0)` | sim | - |  |
| 4 | `Vlr` | `decimal(15,2)` | sim | - |  |
| 5 | `DtaLancto` | `datetime` | sim | - |  |
| 6 | `DtaLanctoPlan` | `datetime` | sim | - |  |
| 7 | `VlrPlan` | `decimal(15,2)` | sim | - |  |
| 8 | `Obs` | `varchar(100)` | sim | - | texto livre |

---

### IVF_Financeira

`classe: vazia` · `23 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqFinanc`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFinanc` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 4 | `Financeira` | `varchar(40)` | sim | - |  |
| 5 | `AnoReferencia` | `char(1)` | sim | - |  |
| 6 | `VlrTCNovo` | `decimal(15,2)` | sim | - |  |
| 7 | `VlrTCUsado` | `decimal(15,2)` | sim | - |  |
| 8 | `VlrTCRetNovo` | `decimal(15,2)` | sim | - |  |
| 9 | `VlrTCRetUsado` | `decimal(15,2)` | sim | - |  |
| 10 | `VlrTCMinNovo` | `decimal(15,2)` | sim | - |  |
| 11 | `VlrTCMinUsado` | `decimal(15,2)` | sim | - |  |
| 12 | `RetMax` | `decimal(2,0)` | sim | - |  |
| 13 | `IndPref` | `decimal(1,0)` | sim | - |  |
| 14 | `IndPrefUsado` | `decimal(1,0)` | sim | - |  |
| 15 | `DevolveTCPJ` | `numeric(1,0)` | sim | - |  |
| 16 | `MinutoMaxResp` | `decimal(4,0)` | sim | - |  |
| 17 | `PercSegPrestam` | `decimal(6,4)` | sim | - |  |
| 18 | `VlrSegPrestam` | `decimal(8,2)` | sim | - |  |
| 19 | `IndSemSegPrest` | `numeric(1,0)` | sim | - |  |
| 20 | `Banco` | `varchar(12)` | sim | - |  |
| 21 | `IndUsaILA` | `numeric(1,0)` | sim | - |  |
| 22 | `IndSegPrestExt` | `numeric(1,0)` | sim | - |  |
| 23 | `TipoSegGarExt` | `char(1)` | sim | - |  |

**Referenciada por:** `IVF_FinancEmpr.SeqFinanc`, `IVF_PlanoIndic.SeqFinanc`, `IVF_Proposta.SeqFinanc`, `IVF_Tabela.SeqFinanc`, `IV_FichaNegVeic.SeqFinanc`

---

### IVF_FinancEmpr

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqFinanc, NroEmpresa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de empresa (multiempresa), no modulo `IVF` (financiamento). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFinanc` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IVF_Financeira.SeqFinanc` |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `IndUsaILARet` | `numeric(1,0)` | sim | - |  |
| 4 | `IndUsaILAAcor` | `numeric(1,0)` | sim | - |  |

---

### IVF_FinancImpTx

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqFinanc, LayOut, ColDestino`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFinanc` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `LayOut` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `ColDestino` | `varchar(30)` | **nao** | - | **PK** |
| 4 | `Coluna` | `numeric(4,0)` | sim | - |  |
| 5 | `Formula` | `varchar(250)` | sim | - |  |
| 6 | `Separador` | `char(1)` | sim | - |  |

---

### IVF_Molicar

`classe: vazia` · `25 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqMolicar`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqMolicar` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CodMolicar` | `varchar(10)` | sim | - |  |
| 3 | `CodMarca` | `decimal(3,0)` | sim | - |  |
| 4 | `Marca` | `varchar(20)` | sim | - |  |
| 5 | `CodModelo` | `decimal(4,0)` | sim | - |  |
| 6 | `Modelo` | `varchar(30)` | sim | - |  |
| 7 | `CodVersao` | `decimal(3,0)` | sim | - |  |
| 8 | `Versao` | `varchar(30)` | sim | - |  |
| 9 | `CodConfiguracao` | `decimal(2,0)` | sim | - |  |
| 10 | `Configuracao` | `varchar(30)` | sim | - |  |
| 11 | `CodCombustivel` | `decimal(1,0)` | sim | - |  |
| 12 | `Combustivel` | `varchar(10)` | sim | - |  |
| 13 | `Porte` | `varchar(3)` | sim | - |  |
| 14 | `CodCategoria` | `decimal(1,0)` | sim | - |  |
| 15 | `Categoria` | `varchar(20)` | sim | - |  |
| 16 | `Origem` | `varchar(3)` | sim | - | sistema de origem do dado |
| 17 | `Cambio` | `char(1)` | sim | - |  |
| 18 | `Portas` | `decimal(1,0)` | sim | - |  |
| 19 | `QtdePassageiros` | `decimal(2,0)` | sim | - |  |
| 20 | `Peso` | `decimal(8,2)` | sim | - |  |
| 21 | `Carga` | `decimal(8,2)` | sim | - |  |
| 22 | `Motor` | `decimal(5,0)` | sim | - |  |
| 23 | `CV` | `decimal(3,0)` | sim | - |  |
| 24 | `AnoInicioFabric` | `decimal(4,0)` | sim | - |  |
| 25 | `AnoFimFabric` | `decimal(4,0)` | sim | - |  |

---

### IVF_Plano

`classe: vazia` · `19 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPlano`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlano` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqTabela` | `numeric(18,0)` | **nao** | - | FK -> `IVF_Tabela.SeqTabela` |
| 3 | `Prazo` | `decimal(4,0)` | **nao** | - | FK -> `IVF_Prazo.Prazo` |
| 4 | `PercMinEntrada` | `decimal(4,2)` | **nao** | - |  |
| 5 | `Coeficiente` | `decimal(10,7)` | **nao** | - |  |
| 6 | `Taxa` | `decimal(6,4)` | **nao** | - |  |
| 7 | `EmUso` | `numeric(1,0)` | **nao** | - | flag de registro/regra ativa (0 = desligada) |
| 8 | `Coef01` | `decimal(10,7)` | sim | - |  |
| 9 | `Coef02` | `decimal(10,7)` | sim | - |  |
| 10 | `Coef03` | `decimal(10,7)` | sim | - |  |
| 11 | `Coef04` | `decimal(10,7)` | sim | - |  |
| 12 | `Coef05` | `decimal(10,7)` | sim | - |  |
| 13 | `Coef06` | `decimal(10,7)` | sim | - |  |
| 14 | `Coef07` | `decimal(10,7)` | sim | - |  |
| 15 | `Coef08` | `decimal(10,7)` | sim | - |  |
| 16 | `Coef09` | `decimal(10,7)` | sim | - |  |
| 17 | `Coef10` | `decimal(10,7)` | sim | - |  |
| 18 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 19 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

**Referenciada por:** `IVF_Proposta.SeqPlanoMelhor`, `IVF_Proposta.SeqPlano`

---

### IVF_PlanoIndic

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPlanoIndic`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlanoIndic` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Processo` | `numeric(18,0)` | **nao** | - | FK -> `IV_FichaNegVeic.Processo`; numero do processo (`IV_Processo.Processo`) |
| 3 | `TipoIndic` | `char(1)` | sim | - |  |
| 4 | `SeqFinanc` | `decimal(6,0)` | sim | - | FK -> `IVF_Financeira.SeqFinanc` |
| 5 | `SeqTabela` | `numeric(18,0)` | sim | - | FK -> `IVF_Tabela.SeqTabela` |
| 6 | `Obs` | `varchar(100)` | sim | - | texto livre |
| 7 | `CodUsuario` | `varchar(20)` | sim | - | login do usuario (varchar) |

---

### IVF_Prazo

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Prazo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Prazo` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(20)` | sim | - | descricao do registro |
| 3 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |

**Referenciada por:** `IVF_Acordo.Prazo`, `IVF_Plano.Prazo`

---

### IVF_Proposta

`classe: vazia` · `60 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProposta`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProposta` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `ProcessoFN` | `numeric(18,0)` | sim | - | FK -> `IV_FichaNegVeic.Processo` |
| 3 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 4 | `SequenciaEnvio` | `decimal(2,0)` | sim | - |  |
| 5 | `SeqFinanc` | `decimal(6,0)` | **nao** | - | FK -> `IVF_Financeira.SeqFinanc` |
| 6 | `SeqPlano` | `numeric(18,0)` | **nao** | - | FK -> `IVF_Plano.SeqPlano` |
| 7 | `Agente` | `varchar(20)` | sim | - |  |
| 8 | `Veiculo` | `varchar(50)` | sim | - |  |
| 9 | `CodModelo` | `varchar(30)` | sim | - |  |
| 10 | `AnoModelo` | `decimal(4,0)` | sim | - |  |
| 11 | `AnoFabric` | `decimal(4,0)` | sim | - |  |
| 12 | `VlrTotalBem` | `decimal(15,2)` | sim | - |  |
| 13 | `VlrFinanciado` | `decimal(15,2)` | sim | - |  |
| 14 | `VlrTC` | `decimal(15,2)` | sim | - |  |
| 15 | `VlrParcela` | `decimal(15,2)` | sim | - |  |
| 16 | `VlrRetorno` | `decimal(15,2)` | sim | - |  |
| 17 | `VlrRetornoTC` | `decimal(15,2)` | sim | - |  |
| 18 | `VlrPlus` | `decimal(15,2)` | sim | - |  |
| 19 | `VlrOutro` | `decimal(15,2)` | sim | - |  |
| 20 | `Coeficiente` | `decimal(10,7)` | sim | - |  |
| 21 | `Prazo` | `decimal(4,0)` | sim | - |  |
| 22 | `Retorno` | `decimal(2,0)` | sim | - |  |
| 23 | `DtaGeracao` | `datetime` | sim | - |  |
| 24 | `DtaEnvioMesa` | `datetime` | sim | - |  |
| 25 | `DtaEnvioFin` | `datetime` | sim | - |  |
| 26 | `DtaRespostaFin` | `datetime` | sim | - |  |
| 27 | `DtaConfirmacao` | `datetime` | sim | - |  |
| 28 | `DtaUltTransacao` | `datetime` | sim | - |  |
| 29 | `DtaRecebimento` | `datetime` | sim | - |  |
| 30 | `IndUltAnalise` | `numeric(1,0)` | sim | - |  |
| 31 | `IndRecebida` | `numeric(1,0)` | sim | - |  |
| 32 | `IndDefinida` | `numeric(1,0)` | sim | - |  |
| 33 | `IndAvaliadaMesa` | `numeric(1,0)` | sim | - |  |
| 34 | `IndAvaliadaFinanc` | `numeric(1,0)` | sim | - |  |
| 35 | `IndAprovada` | `numeric(1,0)` | sim | - |  |
| 36 | `IndRecusada` | `numeric(1,0)` | sim | - |  |
| 37 | `IndDescartada` | `numeric(1,0)` | sim | - |  |
| 38 | `IndVendaPerdida` | `numeric(1,0)` | sim | - |  |
| 39 | `IndUtilizada` | `numeric(1,0)` | sim | - |  |
| 40 | `IndReanaliseMesa` | `numeric(1,0)` | sim | - |  |
| 41 | `IndResgTempo` | `numeric(1,0)` | sim | - |  |
| 42 | `Situacao` | `decimal(2,0)` | **nao** | - |  |
| 43 | `MotivoRecusa` | `varchar(15)` | sim | - |  |
| 44 | `SolicDocAdicional` | `numeric(1,0)` | sim | - |  |
| 45 | `SolicAvalista` | `numeric(1,0)` | sim | - |  |
| 46 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 47 | `ObsInterna` | `varchar(100)` | sim | - |  |
| 48 | `SeqUsrAlocado` | `decimal(8,0)` | sim | - |  |
| 49 | `IndEmTrabalho` | `numeric(1,0)` | sim | - |  |
| 50 | `DtaEmTrabalho` | `datetime` | sim | - |  |
| 51 | `DtaProxAnalise` | `datetime` | sim | - |  |
| 52 | `IndIgnoraTempoFin` | `numeric(1,0)` | sim | - |  |
| 53 | `VlrSegPrest` | `decimal(15,2)` | sim | - |  |
| 54 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 55 | `VlrResultado` | `decimal(10,2)` | sim | - |  |
| 56 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 57 | `VlrPerda` | `decimal(8,2)` | sim | - |  |
| 58 | `SeqPlanoMelhor` | `numeric(18,0)` | sim | - | FK -> `IVF_Plano.SeqPlano` |
| 59 | `VlrSegGarExt` | `numeric(14,2)` | sim | - |  |
| 60 | `VlrPlusPerform` | `numeric(14,2)` | sim | - |  |

**Referenciada por:** `IVF_PropostaHst.SeqProposta`, `IVF_PropostaObs.SeqProposta`, `IVF_PropostaResult.SeqProposta`

---

### IVF_PropostaHst

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQPROPOSTAHST`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a financiamento, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProposta` | `numeric(18,0)` | **nao** | - | FK -> `IVF_Proposta.SeqProposta` |
| 2 | `Situacao` | `decimal(2,0)` | **nao** | - |  |
| 3 | `Detalhe` | `varchar(250)` | sim | - |  |
| 4 | `HU` | `decimal(8,2)` | sim | - |  |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `SEQPROPOSTAHST` | `numeric(18,0)` | **nao** | - | **PK** |

---

### IVF_PropostaObs

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProposta, SeqObs`

**Funcao:** _(inferido)_ Pelo nome, e um bloco de observacoes relacionada a financiamento, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProposta` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IVF_Proposta.SeqProposta` |
| 2 | `SeqObs` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 4 | `Nivel` | `numeric(2,0)` | sim | - |  |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IVF_PropostaResult

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProposta, SeqPropResult`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProposta` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IVF_Proposta.SeqProposta` |
| 2 | `SeqPropResult` | `decimal(2,0)` | **nao** | - | **PK** |
| 3 | `Tipo` | `varchar(10)` | sim | - |  |
| 4 | `SubTipo` | `varchar(12)` | sim | - |  |
| 5 | `Vlr` | `decimal(15,2)` | sim | - |  |

---

### IVF_Tabela

`classe: vazia` · `24 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTabela`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTabela` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tabela` | `varchar(30)` | sim | - |  |
| 3 | `SeqFinanc` | `decimal(6,0)` | **nao** | - | FK -> `IVF_Financeira.SeqFinanc` |
| 4 | `UsaPJ` | `numeric(1,0)` | sim | - |  |
| 5 | `UsaPF` | `numeric(1,0)` | sim | - |  |
| 6 | `TipoPlano` | `varchar(4)` | **nao** | - |  |
| 7 | `NovoUsado` | `char(1)` | **nao** | - |  |
| 8 | `AnoDe` | `decimal(4,0)` | **nao** | - |  |
| 9 | `AnoA` | `decimal(4,0)` | **nao** | - |  |
| 10 | `IndPlanoEspecial` | `numeric(1,0)` | sim | - |  |
| 11 | `IndFiltro` | `numeric(1,0)` | sim | - |  |
| 12 | `VigorDe` | `datetime` | **nao** | - |  |
| 13 | `VigorA` | `datetime` | **nao** | - |  |
| 14 | `VlrTC` | `decimal(15,2)` | sim | - |  |
| 15 | `VlrTCRet` | `decimal(15,2)` | sim | - |  |
| 16 | `VlrTCMin` | `decimal(15,2)` | sim | - |  |
| 17 | `RetMax` | `decimal(2,0)` | sim | - |  |
| 18 | `RestrEmpr` | `numeric(1,0)` | sim | - |  |
| 19 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 20 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 21 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 22 | `Alerta` | `varchar(100)` | sim | - |  |
| 23 | `IndUsoRestrito` | `numeric(1,0)` | sim | - |  |
| 24 | `CodTabela` | `varchar(30)` | sim | - |  |

**Referenciada por:** `IVF_Acordo.SeqTabela`, `IVF_Plano.SeqTabela`, `IVF_PlanoIndic.SeqTabela`, `IVF_TabEmpr.SeqTabela`, `IVF_TabelaFiltro.SeqTabela`

---

### IVF_TabelaFiltro

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTabela, SeqFiltro`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTabela` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IVF_Tabela.SeqTabela` |
| 2 | `SeqFiltro` | `decimal(6,0)` | **nao** | - | **PK** |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IVF_TabEmpr

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTabela, NroEmpresa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de empresa (multiempresa), no modulo `IVF` (financiamento). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTabela` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IVF_Tabela.SeqTabela` |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IVF_TabIndice

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: TipoIndice, VigorDe, VigorA`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `TipoIndice` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `VigorDe` | `datetime` | **nao** | - | **PK** |
| 3 | `VigorA` | `datetime` | **nao** | - | **PK** |
| 4 | `Indice` | `decimal(10,7)` | sim | - |  |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IVF_TipoAgregado

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTpAgreg`

**Funcao:** _(inferido)_ Pelo nome, e um catalogo de tipos, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTpAgreg` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `Agregado` | `varchar(20)` | **nao** | - |  |
| 3 | `Grupo` | `varchar(20)` | **nao** | - |  |
| 4 | `EmUso` | `numeric(1,0)` | **nao** | - | flag de registro/regra ativa (0 = desligada) |
| 5 | `PermEmbutir` | `numeric(1,0)` | sim | - |  |
| 6 | `PermValorZero` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `IVF_Agregado.SeqTpAgreg`, `IVF_TpAgrPessoa.SeqTpAgreg`

---

### IVF_TpAgrPessoa

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTpAgreg, SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, e um agrupamento relacionada a pessoa (cliente/prospect/contato), no modulo `IVF` (financiamento). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTpAgreg` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IVF_TipoAgregado.SeqTpAgreg` |
| 2 | `SeqPessoa` | `numeric(8,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `BaseFechamento` | `char(1)` | sim | - |  |
| 4 | `DiasPgto` | `decimal(2,0)` | sim | - |  |
| 5 | `Obs` | `varchar(100)` | sim | - | texto livre |

**Referenciada por:** `IVF_Agregado.SeqPessoaRec`, `IVF_Agregado.SeqTpAgreg`

---
