# IMP - Staging de importacao do ERP

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**23 tabelas · 717 colunas · 1.620.093 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IMP_EMAIL`](#imp_email) | staging | 13 | 0 |
| [`IMP_Evento`](#imp_evento) | staging | 26 | 0 |
| [`IMP_FORMAPGTO`](#imp_formapgto) | staging | 12 | 0 |
| [`IMP_LINKPESSOA`](#imp_linkpessoa) | staging | 4 | 20.757 |
| [`IMP_LOGAPROVACAO`](#imp_logaprovacao) | staging | 2 | 0 |
| [`IMP_NFS`](#imp_nfs) | staging | 45 | 18.378 |
| [`IMP_NFSCMPL`](#imp_nfscmpl) | staging | 9 | 0 |
| [`IMP_NFSItem`](#imp_nfsitem) | staging | 40 | 67.404 |
| [`IMP_OS`](#imp_os) | staging | 46 | 287.868 |
| [`IMP_OSITEM`](#imp_ositem) | staging | 24 | 399.040 |
| [`IMP_OSSOLIC`](#imp_ossolic) | staging | 11 | 0 |
| [`IMP_Pedido`](#imp_pedido) | staging | 61 | 0 |
| [`IMP_PedidoItem`](#imp_pedidoitem) | staging | 30 | 0 |
| [`IMP_PESSOA`](#imp_pessoa) | staging | 76 | 0 |
| [`IMP_PESSOACONTATO`](#imp_pessoacontato) | staging | 27 | 0 |
| [`IMP_PESSOAFONE`](#imp_pessoafone) | staging | 19 | 0 |
| [`IMP_PRODUTO`](#imp_produto) | staging | 29 | 0 |
| [`IMP_REL_TBA101`](#imp_rel_tba101) | staging | 29 | 318 |
| [`IMP_REL_TBA101_PG2`](#imp_rel_tba101_pg2) | staging | 31 | 208 |
| [`IMP_Titulo`](#imp_titulo) | staging | 42 | 823.952 |
| [`IMP_Titulo_bkp_11_04`](#imp_titulo_bkp_11_04) | lixo/backup | 42 | 2.168 |
| [`IMP_VEICULO`](#imp_veiculo) | staging | 49 | 0 |
| [`IMP_VEICULO_INTEGRADO`](#imp_veiculo_integrado) | staging | 50 | 0 |

---

### IMP_EMAIL

`classe: staging` · `13 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDEMAIL`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `IMP` (staging de importacao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PESSOALINKORIGEM` | `varchar(20)` | **nao** | - |  |
| 2 | `PESSOALINK` | `varchar(250)` | sim | - |  |
| 3 | `EMAILLINK` | `nvarchar(70)` | **nao** | - |  |
| 4 | `EMAIL` | `varchar(70)` | **nao** | - |  |
| 5 | `INDEMUSO` | `numeric(1,0)` | sim | - |  |
| 6 | `INDPREFERENCIAL` | `numeric(1,0)` | sim | - |  |
| 7 | `INDUSOMKT` | `numeric(1,0)` | sim | - |  |
| 8 | `INDUSOPESSOAL` | `numeric(1,0)` | sim | - |  |
| 9 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 10 | `OBS` | `varchar(50)` | sim | - | texto livre |
| 11 | `DTAGERACAO` | `datetime` | **nao** | - |  |
| 12 | `STATUSIMP` | `char(1)` | sim | - |  |
| 13 | `IDEMAIL` | `numeric(18,0)` | **nao** | - | **PK** |

---

### IMP_Evento

`classe: staging` · `26 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdEvento`

**Funcao:** No catálogo de jobs: FID_INTEGRACAO ('Importa dados do PDV para a IMP_EVENTO'), FIDELIDADE_SENDEMAIL ('Envia e-mails para pessoas que têm pontos para resgatar'), MELIUZ ('Obtém as transações efetuadas pela Méliuz'). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdEvento` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 4 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 5 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 6 | `PessoaLinkAd` | `varchar(60)` | sim | - |  |
| 7 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 8 | `NroEmpresa` | `numeric(10,0)` | sim | - | multiempresa - filial/empresa |
| 9 | `Evento` | `varchar(40)` | sim | - |  |
| 10 | `Resultado` | `numeric(6,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |
| 11 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |
| 12 | `DtaEvento` | `datetime` | sim | - |  |
| 13 | `Valor` | `numeric(14,2)` | sim | - |  |
| 14 | `Valor2` | `numeric(14,2)` | sim | - |  |
| 15 | `Qtde` | `numeric(10,2)` | sim | - |  |
| 16 | `CodUsuario` | `varchar(20)` | sim | - | login do usuario (varchar) |
| 17 | `Detalhe` | `varchar(4000)` | sim | - |  |
| 18 | `InfoAdicional` | `varchar(2000)` | sim | - |  |
| 19 | `LinkDocto` | `varchar(10)` | sim | - | chave de vinculo com documento do ERP |
| 20 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 21 | `LinkSerie` | `varchar(250)` | sim | - | chave de vinculo com documento do ERP |
| 22 | `LinkNroEmpresa` | `numeric(6,0)` | sim | - |  |
| 23 | `LinkAdicional` | `varchar(150)` | sim | - |  |
| 24 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 25 | `StatusImpSP` | `char(1)` | sim | - |  |
| 26 | `StatusImpZ` | `char(1)` | sim | - |  |

---

### IMP_FORMAPGTO

`classe: staging` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdFormaPgto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de forma/condicao de pagamento, no modulo `IMP` (staging de importacao). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdFormaPgto` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `Nronf` | `numeric(18,0)` | **nao** | - |  |
| 5 | `Serienf` | `varchar(12)` | sim | - |  |
| 6 | `CodFormaPGTO` | `varchar(30)` | **nao** | - |  |
| 7 | `DESCFormaPGTO` | `varchar(50)` | **nao** | - |  |
| 8 | `VLR` | `numeric(14,2)` | **nao** | - |  |
| 9 | `DtaVencto` | `datetime` | **nao** | - |  |
| 10 | `Obs` | `varchar(100)` | sim | - | texto livre |
| 11 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 12 | `StatusIMP` | `char(1)` | sim | - |  |

---

### IMP_LINKPESSOA

`classe: staging` · `4 colunas` · `20.757 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IMP` (staging de importacao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PESSOALINK` | `varchar(250)` | sim | - |  |
| 2 | `CGCCPF` | `numeric(15,0)` | sim | - |  |
| 3 | `NROCGCCPF` | `numeric(15,0)` | sim | - |  |
| 4 | `DIGCGCCPF` | `numeric(5,0)` | sim | - |  |

---

### IMP_LOGAPROVACAO

`classe: staging` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Seq`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de acao (tipo de tarefa), no modulo `IMP` (staging de importacao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `detalhe` | `varchar(1000)` | sim | - |  |

---

### IMP_NFS

`classe: staging` · `45 colunas` · `18.378 linhas (snapshot 03/06/2026)` · `PK: IdNFS`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdNFS` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `Nronf` | `numeric(18,0)` | **nao** | - |  |
| 5 | `Serienf` | `varchar(20)` | sim | - |  |
| 6 | `NroEmpresaVda` | `numeric(10,0)` | sim | - |  |
| 7 | `NroCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 8 | `DigCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 9 | `CNPJx` | `varchar(20)` | sim | - |  |
| 10 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 11 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 12 | `Segmento` | `varchar(30)` | sim | - |  |
| 13 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 14 | `TipoVenda` | `varchar(30)` | sim | - |  |
| 15 | `CFOP` | `numeric(6,0)` | sim | - |  |
| 16 | `CodOperacao` | `varchar(20)` | sim | - |  |
| 17 | `Operacao` | `varchar(30)` | sim | - |  |
| 18 | `CanalVenda` | `varchar(20)` | sim | - |  |
| 19 | `Setor` | `varchar(30)` | sim | - |  |
| 20 | `Formapgto` | `varchar(25)` | sim | - |  |
| 21 | `CondicaoPgto` | `varchar(60)` | sim | - |  |
| 22 | `Vendedor` | `varchar(60)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 23 | `CodVendedor` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 24 | `Dtapedido` | `datetime` | sim | - |  |
| 25 | `Nropedido` | `varchar(20)` | sim | - |  |
| 26 | `Dtaemissaonf` | `datetime` | **nao** | - |  |
| 27 | `Situacao` | `char(1)` | **nao** | - |  |
| 28 | `CodTransportador` | `varchar(20)` | sim | - |  |
| 29 | `Transportador` | `varchar(40)` | sim | - |  |
| 30 | `Usuario` | `varchar(20)` | sim | - |  |
| 31 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 32 | `DtaAlteracaoERP` | `datetime` | sim | - |  |
| 33 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 34 | `StatusIMP` | `char(1)` | sim | - |  |
| 35 | `DtaImport` | `datetime` | sim | - |  |
| 36 | `WhereItem` | `varchar(60)` | sim | - |  |
| 37 | `PERCBASECOMISSAO` | `numeric(5,2)` | sim | - |  |
| 38 | `IDNFSEXTERNO` | `varchar(40)` | sim | - |  |
| 39 | `LOTECARGA` | `varchar(50)` | sim | - |  |
| 40 | `PAIIDNFSEXTERNO` | `varchar(40)` | sim | - |  |
| 41 | `EVENTOCMPL` | `varchar(60)` | sim | - |  |
| 42 | `TIPOPEDIDO` | `varchar(40)` | sim | - |  |
| 43 | `IDENTIFICADO` | `varchar(20)` | sim | - |  |
| 44 | `NROCPFVENDEDOR` | `numeric(11,0)` | sim | - |  |
| 45 | `NROVOUCHER` | `varchar(50)` | sim | - |  |

---

### IMP_NFSCMPL

`classe: staging` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Idcmpl`

**Funcao:** _(inferido)_ Pelo nome, e um complemento do registro pai relacionada a nota fiscal, no modulo `IMP` (staging de importacao). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Idcmpl` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `Nronf` | `numeric(18,0)` | **nao** | - |  |
| 5 | `Serienf` | `varchar(12)` | sim | - |  |
| 6 | `CMPL` | `varchar(250)` | sim | - |  |
| 7 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 8 | `StatusIMP` | `char(1)` | sim | - |  |
| 9 | `IDNFSEXTERNO` | `varchar(40)` | sim | - |  |

---

### IMP_NFSItem

`classe: staging` · `40 colunas` · `67.404 linhas (snapshot 03/06/2026)` · `PK: IdNFSItem`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdNFSItem` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `Nronf` | `numeric(18,0)` | **nao** | - |  |
| 5 | `Serienf` | `varchar(20)` | sim | - |  |
| 6 | `NroItem` | `numeric(6,0)` | sim | - |  |
| 7 | `CodProduto` | `varchar(40)` | **nao** | - |  |
| 8 | `NCM` | `numeric(10,0)` | sim | - |  |
| 9 | `CodBarra` | `numeric(18,0)` | sim | - |  |
| 10 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 11 | `Marca` | `varchar(30)` | sim | - |  |
| 12 | `CodFamilia` | `varchar(30)` | sim | - |  |
| 13 | `Familia` | `varchar(60)` | sim | - |  |
| 14 | `Descproduto` | `varchar(100)` | **nao** | - |  |
| 15 | `Qtde` | `numeric(10,3)` | **nao** | - |  |
| 16 | `VlrLiqItem` | `numeric(14,2)` | **nao** | - |  |
| 17 | `Vlrdescto` | `numeric(14,2)` | sim | - |  |
| 18 | `VlrResult` | `numeric(14,2)` | sim | - |  |
| 19 | `VlrCustoMkt` | `numeric(14,2)` | sim | - |  |
| 20 | `VlrICM` | `numeric(14,2)` | sim | - |  |
| 21 | `VlrImposto` | `numeric(14,2)` | sim | - |  |
| 22 | `Situacao` | `char(1)` | **nao** | - |  |
| 23 | `Vendedor` | `varchar(60)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 24 | `CodVendedor` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 25 | `Identificador` | `varchar(40)` | sim | - |  |
| 26 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 27 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 28 | `StatusIMP` | `char(1)` | sim | - |  |
| 29 | `DtaImport` | `datetime` | sim | - |  |
| 30 | `TIPO` | `char(1)` | sim | - |  |
| 31 | `LOTECARGA` | `varchar(50)` | sim | - |  |
| 32 | `SETORITEM` | `varchar(30)` | sim | - |  |
| 33 | `VLRCUSTO` | `numeric(14,2)` | sim | - |  |
| 34 | `IDNFSEXTERNO` | `varchar(40)` | sim | - |  |
| 35 | `VLRICMSRETIDO` | `numeric(14,2)` | sim | - |  |
| 36 | `VLRICMSSUBS` | `numeric(14,2)` | sim | - |  |
| 37 | `VLRPIS` | `numeric(14,2)` | sim | - |  |
| 38 | `VLRCOFINS` | `numeric(14,2)` | sim | - |  |
| 39 | `NROCPFVENDEDOR` | `numeric(11,0)` | sim | - |  |
| 40 | `VLRDESCTOVCHR` | `numeric(14,2)` | sim | - |  |

---

### IMP_OS

`classe: staging` · `46 colunas` · `287.868 linhas (snapshot 03/06/2026)` · `PK: IDOS`

**Funcao:** Ordem de serviço (pós-venda/oficina). IMP_OS 46 col./287.868 linhas vs EXT_OS 40 col./8.099 — 280.214 nunca promovidas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDOS` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `Nroos` | `numeric(18,0)` | **nao** | - |  |
| 5 | `SerieOS` | `varchar(12)` | sim | - |  |
| 6 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 7 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 8 | `NroCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 9 | `DigCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 10 | `CNPJx` | `varchar(20)` | sim | - |  |
| 11 | `Segmento` | `varchar(30)` | sim | - |  |
| 12 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 13 | `Nrochassi` | `varchar(40)` | sim | - |  |
| 14 | `Placa` | `varchar(9)` | sim | - |  |
| 15 | `Combustivel` | `varchar(30)` | sim | - |  |
| 16 | `CodModelo` | `varchar(30)` | sim | - |  |
| 17 | `Modelo` | `varchar(30)` | sim | - |  |
| 18 | `CorExterna` | `varchar(30)` | sim | - |  |
| 19 | `CorInterna` | `varchar(30)` | sim | - |  |
| 20 | `Anofabricacao` | `numeric(4,0)` | sim | - |  |
| 21 | `AnoModelo` | `numeric(4,0)` | sim | - |  |
| 22 | `Dtavenda` | `datetime` | sim | - |  |
| 23 | `Consultor` | `varchar(30)` | sim | - |  |
| 24 | `Tipoos` | `varchar(30)` | sim | - |  |
| 25 | `Dtaabertura` | `datetime` | sim | - |  |
| 26 | `Dtaencerramento` | `datetime` | sim | - |  |
| 27 | `Dtafechamento` | `datetime` | sim | - |  |
| 28 | `VlrLiqPecas` | `numeric(14,2)` | sim | - |  |
| 29 | `VlrLiqServicos` | `numeric(14,2)` | sim | - |  |
| 30 | `Observacao` | `varchar(250)` | sim | - | texto livre |
| 31 | `NroDN` | `varchar(10)` | sim | - |  |
| 32 | `Kilometragem` | `numeric(8,0)` | sim | - |  |
| 33 | `Usuario` | `varchar(20)` | sim | - |  |
| 34 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 35 | `StatusIMP` | `char(1)` | sim | - |  |
| 36 | `DtaImport` | `datetime` | sim | - |  |
| 37 | `WhereItem` | `varchar(60)` | sim | - |  |
| 38 | `Marca` | `varchar(20)` | sim | - |  |
| 39 | `Familia` | `varchar(60)` | sim | - |  |
| 40 | `CodFamilia` | `varchar(30)` | sim | - |  |
| 41 | `IDOSEXTERNO` | `varchar(40)` | sim | - |  |
| 42 | `SITUACAO` | `char(1)` | **nao** | - |  |
| 43 | `CODTIPOOS` | `varchar(10)` | sim | - |  |
| 44 | `EVENTOCMPL` | `varchar(60)` | sim | - |  |
| 45 | `DTAALTERACAOERP` | `datetime` | sim | - |  |
| 46 | `TIPOSERVICO` | `varchar(30)` | sim | - |  |

---

### IMP_OSITEM

`classe: staging` · `24 colunas` · `399.040 linhas (snapshot 03/06/2026)` · `PK: Iditem`

**Funcao:** Ordem de serviço (pós-venda/oficina). IMP_OS 46 col./287.868 linhas vs EXT_OS 40 col./8.099 — 280.214 nunca promovidas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Iditem` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `Nroos` | `numeric(18,0)` | **nao** | - |  |
| 5 | `SerieOS` | `varchar(12)` | sim | - |  |
| 6 | `NroItem` | `numeric(6,0)` | sim | - |  |
| 7 | `Tipoitem` | `char(1)` | sim | - |  |
| 8 | `GrupoItem` | `varchar(20)` | sim | - |  |
| 9 | `Produtivo` | `varchar(30)` | sim | - |  |
| 10 | `StatusItem` | `char(1)` | sim | - |  |
| 11 | `Codigo` | `varchar(40)` | sim | - |  |
| 12 | `Descricao` | `varchar(100)` | sim | - | descricao do registro |
| 13 | `Qtde` | `numeric(10,2)` | sim | - |  |
| 14 | `VlrLiquido` | `numeric(14,2)` | sim | - |  |
| 15 | `Vlrdescto` | `numeric(14,2)` | sim | - |  |
| 16 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 17 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 18 | `StatusIMP` | `char(1)` | sim | - |  |
| 19 | `DtaImport` | `datetime` | sim | - |  |
| 20 | `IDOSEXTERNO` | `varchar(40)` | sim | - |  |
| 21 | `MAODEOBRA` | `varchar(30)` | sim | - |  |
| 22 | `VLRTOTITEM` | `numeric(14,2)` | sim | - |  |
| 23 | `VLRTOTITEMBRUTO` | `numeric(14,2)` | sim | - |  |
| 24 | `IDOS` | `numeric(18,0)` | sim | - |  |

---

### IMP_OSSOLIC

`classe: staging` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Idsolic`

**Funcao:** Ordem de serviço (pós-venda/oficina). IMP_OS 46 col./287.868 linhas vs EXT_OS 40 col./8.099 — 280.214 nunca promovidas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Idsolic` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `Nroos` | `numeric(18,0)` | **nao** | - |  |
| 5 | `SerieOS` | `varchar(12)` | sim | - |  |
| 6 | `Codigo` | `varchar(20)` | sim | - |  |
| 7 | `Descricao` | `varchar(250)` | sim | - | descricao do registro |
| 8 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 9 | `StatusIMP` | `char(1)` | sim | - |  |
| 10 | `DtaImport` | `datetime` | sim | - |  |
| 11 | `IDOSEXTERNO` | `varchar(40)` | sim | - |  |

---

### IMP_Pedido

`classe: staging` · `61 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdPedido`

**Funcao:** Pedido de venda — a ÚNICA cadeia modelada para saída CRM→ERP e a única EXT_ com coluna PROCESSO. Todas com 0 linhas: nunca usada na Tracbel. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdPedido` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 4 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 5 | `NroCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 6 | `DigCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 7 | `CNPJx` | `varchar(20)` | sim | - |  |
| 8 | `Segmento` | `varchar(30)` | sim | - |  |
| 9 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 10 | `NroEmpresa` | `decimal(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 11 | `NroPedido` | `varchar(20)` | **nao** | - |  |
| 12 | `CodOperacao` | `varchar(20)` | sim | - |  |
| 13 | `Operacao` | `varchar(40)` | sim | - |  |
| 14 | `CodVendedor` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 15 | `Vendedor` | `varchar(40)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 16 | `CanalVenda` | `varchar(20)` | sim | - |  |
| 17 | `NroPedCliente` | `varchar(50)` | sim | - |  |
| 18 | `NroPedidoERP` | `varchar(30)` | sim | - |  |
| 19 | `FormaPgto` | `varchar(15)` | sim | - |  |
| 20 | `CondicaoPgto` | `varchar(60)` | sim | - |  |
| 21 | `AtivoReceptivo` | `char(1)` | sim | - |  |
| 22 | `DtaBaseFatura` | `datetime` | sim | - |  |
| 23 | `NroDocto` | `decimal(12,0)` | sim | - |  |
| 24 | `Serie` | `varchar(4)` | sim | - |  |
| 25 | `DtaPedido` | `datetime` | sim | - |  |
| 26 | `DtaValidade` | `datetime` | sim | - |  |
| 27 | `DtaFechamento` | `datetime` | sim | - |  |
| 28 | `DtaProxContato` | `datetime` | sim | - |  |
| 29 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 30 | `StatusCmpl` | `varchar(40)` | sim | - |  |
| 31 | `IndEstRservado` | `numeric(1,0)` | sim | - |  |
| 32 | `IndEnvioERP` | `numeric(1,0)` | sim | - |  |
| 33 | `DtaEnvioERP` | `datetime` | sim | - |  |
| 34 | `DtaAlteracaoERP` | `datetime` | sim | - |  |
| 35 | `ObsNF` | `varchar(250)` | sim | - |  |
| 36 | `ObsInterna` | `varchar(250)` | sim | - |  |
| 37 | `ObsDesconto` | `varchar(200)` | sim | - |  |
| 38 | `ExigeAssinatura` | `varchar(1)` | sim | - |  |
| 39 | `AssinaturaDesc` | `varchar(20)` | sim | - |  |
| 40 | `AssinaturaDta` | `datetime` | sim | - |  |
| 41 | `EntradaSaida` | `varchar(1)` | sim | - |  |
| 42 | `MotivoVP` | `varchar(40)` | sim | - |  |
| 43 | `TipoEntrega` | `varchar(20)` | sim | - |  |
| 44 | `TipoFrete` | `varchar(20)` | sim | - |  |
| 45 | `CodTransportador` | `varchar(20)` | sim | - |  |
| 46 | `Transportador` | `varchar(40)` | sim | - |  |
| 47 | `PedidoLinkNro` | `numeric(18,0)` | sim | - |  |
| 48 | `PedidoLinkStr` | `varchar(20)` | sim | - |  |
| 49 | `Usuario` | `varchar(20)` | sim | - |  |
| 50 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 51 | `StatusIMP` | `char(1)` | sim | - |  |
| 52 | `DtaImport` | `datetime` | sim | - |  |
| 53 | `PERCBASECOMISSAO` | `numeric(5,2)` | sim | - |  |
| 54 | `PEDIDOLINKORIGEM` | `varchar(20)` | sim | - |  |
| 55 | `PAILINKORIGEM` | `varchar(20)` | sim | - |  |
| 56 | `PAILINKNRO` | `numeric(18,0)` | sim | - |  |
| 57 | `PAILINKSTR` | `varchar(20)` | sim | - |  |
| 58 | `PROCESSO` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 59 | `LOTECARGA` | `varchar(50)` | sim | - |  |
| 60 | `EVENTOCMPL` | `varchar(60)` | sim | - |  |
| 61 | `VLRDESCADICIONAL` | `numeric(15,2)` | sim | - |  |

---

### IMP_PedidoItem

`classe: staging` · `30 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdPedidoItem`

**Funcao:** Pedido de venda — a ÚNICA cadeia modelada para saída CRM→ERP e a única EXT_ com coluna PROCESSO. Todas com 0 linhas: nunca usada na Tracbel. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdPedidoItem` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `NroPedido` | `varchar(20)` | **nao** | - |  |
| 5 | `NroItem` | `numeric(6,0)` | sim | - |  |
| 6 | `CodTabelaPreco` | `varchar(12)` | sim | - |  |
| 7 | `CodProduto` | `varchar(40)` | **nao** | - |  |
| 8 | `Marca` | `varchar(30)` | sim | - |  |
| 9 | `CodFamilia` | `varchar(30)` | sim | - |  |
| 10 | `Familia` | `varchar(60)` | sim | - |  |
| 11 | `Descproduto` | `varchar(100)` | **nao** | - |  |
| 12 | `PrecoTabela` | `decimal(15,2)` | sim | - |  |
| 13 | `Qtde` | `decimal(10,2)` | sim | - |  |
| 14 | `QtdeAtendida` | `decimal(10,2)` | sim | - |  |
| 15 | `IndEstRservado` | `numeric(1,0)` | sim | - |  |
| 16 | `VlrUnitario` | `decimal(15,4)` | sim | - |  |
| 17 | `VlrDescUnitCml` | `decimal(10,3)` | sim | - |  |
| 18 | `VlrDescUnitAuto` | `decimal(10,3)` | sim | - |  |
| 19 | `VlrDescTotal` | `decimal(15,2)` | sim | - |  |
| 20 | `DescConciliado` | `numeric(1,0)` | sim | - |  |
| 21 | `VlrBaseICM` | `decimal(15,2)` | sim | - |  |
| 22 | `VlrICM` | `decimal(15,2)` | sim | - |  |
| 23 | `AliqICM` | `decimal(15,2)` | sim | - |  |
| 24 | `VlrICMSubs` | `decimal(15,2)` | sim | - |  |
| 25 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 26 | `MotivoVP` | `varchar(20)` | sim | - |  |
| 27 | `ObsItem` | `varchar(50)` | sim | - |  |
| 28 | `IndSugestao` | `char(2)` | sim | - |  |
| 29 | `TIPO` | `char(1)` | **nao** | - |  |
| 30 | `LOTECARGA` | `varchar(50)` | sim | - |  |

---

### IMP_PESSOA

`classe: staging` · `76 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IMP` (staging de importacao). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdPessoa` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 3 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 4 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 5 | `Status` | `char(1)` | **nao** | - | status - validar dominio real por tabela |
| 6 | `NomeRazao` | `varchar(100)` | sim | - |  |
| 7 | `Fantasia` | `varchar(50)` | sim | - |  |
| 8 | `PalavraChave` | `varchar(50)` | sim | - |  |
| 9 | `FisicaJuridica` | `char(1)` | sim | - |  |
| 10 | `SEXO` | `char(1)` | sim | - |  |
| 11 | `Cidade` | `varchar(50)` | sim | - |  |
| 12 | `UF` | `varchar(2)` | sim | - |  |
| 13 | `PAIS` | `varchar(25)` | sim | - |  |
| 14 | `Bairro` | `varchar(50)` | sim | - |  |
| 15 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 16 | `Logradouro` | `varchar(80)` | sim | - |  |
| 17 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 18 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 19 | `CEP` | `varchar(12)` | sim | - |  |
| 20 | `CxPostal` | `varchar(7)` | sim | - |  |
| 21 | `RefEndereco` | `varchar(150)` | sim | - |  |
| 22 | `InscMunic` | `varchar(15)` | sim | - |  |
| 23 | `InscProdutor` | `varchar(20)` | sim | - |  |
| 24 | `CNAE` | `varchar(15)` | sim | - |  |
| 25 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 26 | `FoneNro1` | `varchar(30)` | sim | - |  |
| 27 | `FoneCmpl1` | `varchar(12)` | sim | - |  |
| 28 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 29 | `FoneNro2` | `varchar(30)` | sim | - |  |
| 30 | `FoneCmpl2` | `varchar(12)` | sim | - |  |
| 31 | `FoneDDD3` | `varchar(5)` | sim | - |  |
| 32 | `FoneNro3` | `varchar(30)` | sim | - |  |
| 33 | `FoneCmpl3` | `varchar(12)` | sim | - |  |
| 34 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 35 | `FaxNro` | `varchar(20)` | sim | - |  |
| 36 | `NroCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 37 | `DigCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 38 | `CNPJx` | `varchar(20)` | sim | - |  |
| 39 | `InscricaoRG` | `varchar(20)` | sim | - |  |
| 40 | `UFEmissor` | `varchar(2)` | sim | - |  |
| 41 | `OrgaoEmissor` | `varchar(10)` | sim | - |  |
| 42 | `DtaNASC` | `datetime` | sim | - |  |
| 43 | `Email` | `varchar(70)` | sim | - |  |
| 44 | `HomePage` | `varchar(80)` | sim | - |  |
| 45 | `EstadoCivil` | `varchar(20)` | sim | - |  |
| 46 | `Atividade` | `varchar(30)` | sim | - |  |
| 47 | `RendaFaturamento` | `varchar(30)` | sim | - |  |
| 48 | `GrauInstrucao` | `varchar(30)` | sim | - |  |
| 49 | `Grupo` | `varchar(30)` | sim | - |  |
| 50 | `Porte` | `varchar(30)` | sim | - |  |
| 51 | `CodRegiao` | `varchar(30)` | sim | - |  |
| 52 | `Regiao` | `varchar(40)` | sim | - |  |
| 53 | `CodRota` | `varchar(30)` | sim | - |  |
| 54 | `ROTA` | `varchar(40)` | sim | - |  |
| 55 | `PessoaLinkVincOrig` | `varchar(20)` | sim | - |  |
| 56 | `PessoaLinkVinc` | `varchar(30)` | sim | - |  |
| 57 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 58 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 59 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 60 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 61 | `CODVENDEDOR` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 62 | `NaoPossuiEmail` | `numeric(1,0)` | sim | - |  |
| 63 | `ProblemaCredito` | `numeric(1,0)` | sim | - |  |
| 64 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 65 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 66 | `StatusIMP` | `char(1)` | sim | - |  |
| 67 | `DtaImport` | `datetime` | sim | - |  |
| 68 | `CLASSES` | `varchar(250)` | sim | - |  |
| 69 | `VLRLIMITECREDITO` | `numeric(14,2)` | sim | - |  |
| 70 | `SITUACAOCREDITO` | `varchar(30)` | sim | - |  |
| 71 | `VLRSALDOCREDITO` | `numeric(14,2)` | sim | - |  |
| 72 | `STATUSSINTEGRA` | `varchar(50)` | sim | - |  |
| 73 | `DADOADICIONAL1` | `varchar(50)` | sim | - |  |
| 74 | `DADOADICIONAL2` | `varchar(100)` | sim | - |  |
| 75 | `DEPTO` | `varchar(250)` | sim | - |  |
| 76 | `CARTEIRA` | `varchar(250)` | sim | - |  |

---

### IMP_PESSOACONTATO

`classe: staging` · `27 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDPESSOACTTO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IMP` (staging de importacao).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDPESSOACTTO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `PESSOALINKORIGEM` | `varchar(20)` | **nao** | - |  |
| 3 | `PESSOALINK` | `varchar(250)` | sim | - |  |
| 4 | `CONTATOLINK` | `nvarchar(50)` | **nao** | - |  |
| 5 | `CONTATO` | `varchar(40)` | **nao** | - |  |
| 6 | `SAUDACAO` | `varchar(20)` | sim | - |  |
| 7 | `SEXO` | `varchar(1)` | **nao** | - |  |
| 8 | `DATANASCIMENTO` | `datetime` | sim | - |  |
| 9 | `EMAIL` | `varchar(50)` | sim | - |  |
| 10 | `TIPOCONTATO` | `varchar(30)` | **nao** | - |  |
| 11 | `AREAATUACAO` | `varchar(20)` | sim | - |  |
| 12 | `RG` | `varchar(20)` | sim | - |  |
| 13 | `CPF` | `numeric(12,0)` | sim | - |  |
| 14 | `FONEDDD1` | `varchar(5)` | sim | - |  |
| 15 | `FONENRO1` | `varchar(12)` | sim | - |  |
| 16 | `FONEDDD2` | `varchar(5)` | sim | - |  |
| 17 | `FONENRO2` | `varchar(12)` | sim | - |  |
| 18 | `SKYPE` | `varchar(30)` | sim | - |  |
| 19 | `PAPEL1` | `nvarchar(30)` | sim | - |  |
| 20 | `PAPEL2` | `nvarchar(30)` | sim | - |  |
| 21 | `PAPEL3` | `nvarchar(30)` | sim | - |  |
| 22 | `OBSERVACAO` | `varchar(250)` | sim | - | texto livre |
| 23 | `DTAGERACAO` | `datetime` | **nao** | - |  |
| 24 | `STATUSIMPSP` | `char(1)` | sim | - |  |
| 25 | `STATUSIMP` | `char(1)` | sim | - |  |
| 26 | `INDWHATSAPPF1` | `numeric(1,0)` | sim | - |  |
| 27 | `INDWHATSAPPF2` | `numeric(1,0)` | sim | - |  |

---

### IMP_PESSOAFONE

`classe: staging` · `19 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDPESSOAFONE`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IMP` (staging de importacao). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDPESSOAFONE` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `PESSOALINKORIGEM` | `varchar(20)` | **nao** | - |  |
| 3 | `PESSOALINK` | `varchar(250)` | **nao** | - |  |
| 4 | `SEQPESSOA` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 5 | `FONELINK` | `numeric(12,0)` | sim | - |  |
| 6 | `OPERACAO` | `char(1)` | sim | - |  |
| 7 | `DDD` | `varchar(5)` | sim | - |  |
| 8 | `NUMERO` | `numeric(12,0)` | **nao** | - |  |
| 9 | `COMPLEMENTO` | `varchar(20)` | sim | - |  |
| 10 | `OBS` | `varchar(30)` | sim | - | texto livre |
| 11 | `TIPOFONE` | `varchar(20)` | sim | - |  |
| 12 | `INDUSOMKT` | `numeric(1,0)` | sim | - |  |
| 13 | `INDFONEPREF` | `numeric(1,0)` | sim | - |  |
| 14 | `INDEMUSO` | `numeric(1,0)` | sim | - |  |
| 15 | `INDWHATSAPP` | `numeric(1,0)` | sim | - |  |
| 16 | `STATUSIMP` | `char(1)` | sim | - |  |
| 17 | `DTAGERACAO` | `datetime` | **nao** | - |  |
| 18 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 19 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IMP_PRODUTO

`classe: staging` · `29 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDPRODUTO`

**Funcao:** Catálogos vindos do ERP. EXT_Produto 27 col./78.749 linhas (Protheus 35.027, SISDIA 27.859, 15.860 sem origem); EXT_Vendedor 6 col./563 (SISDIA 347, sem origem 188, Protheus 28); IMP_PRODUTO 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDPRODUTO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `OPERACAO` | `char(1)` | **nao** | - |  |
| 3 | `CODPRODUTO` | `varchar(50)` | sim | - |  |
| 4 | `ORIGEM` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 5 | `TIPO` | `char(1)` | **nao** | - |  |
| 6 | `CODBARRA` | `numeric(18,0)` | sim | - |  |
| 7 | `DESCRICAO` | `varchar(50)` | sim | - | descricao do registro |
| 8 | `MARCA` | `varchar(30)` | sim | - |  |
| 9 | `CODFAMILIA` | `varchar(30)` | sim | - |  |
| 10 | `FAMILIA` | `varchar(60)` | sim | - |  |
| 11 | `DESCRICAOCOMPLETA` | `varchar(250)` | sim | - |  |
| 12 | `NCM` | `numeric(10,0)` | sim | - |  |
| 13 | `PRECOPUBLICO` | `numeric(14,2)` | sim | - |  |
| 14 | `PRECO1` | `numeric(14,2)` | sim | - |  |
| 15 | `PRECO2` | `numeric(14,2)` | sim | - |  |
| 16 | `FORNECEDORPRINCIPAL` | `varchar(70)` | sim | - |  |
| 17 | `CATEGORIA1` | `varchar(50)` | sim | - |  |
| 18 | `CATEGORIA2` | `varchar(50)` | sim | - |  |
| 19 | `CATEGORIA3` | `varchar(50)` | sim | - |  |
| 20 | `CATEGORIA4` | `varchar(50)` | sim | - |  |
| 21 | `CATEGORIA5` | `varchar(50)` | sim | - |  |
| 22 | `CATEGORIA6` | `varchar(50)` | sim | - |  |
| 23 | `CHAVEPRODUTOERP` | `varchar(50)` | sim | - |  |
| 24 | `DTAGERACAO` | `datetime` | **nao** | - |  |
| 25 | `EMUSO` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 26 | `STATUSIMP` | `char(1)` | sim | - |  |
| 27 | `QTDEESTOQUE` | `numeric(10,2)` | sim | - |  |
| 28 | `CUSTOESTOQUE` | `numeric(15,4)` | sim | - |  |
| 29 | `DTAESTOQUE` | `datetime` | sim | - |  |

---

### IMP_REL_TBA101

`classe: staging` · `29 colunas` · `318 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Staging de relatório (Análise da Carteira de Pedidos FY25), discriminado por codusuario. PG2 acumula sem DELETE: 1.558 linhas para muito menos processos (V:AMAURY 208 linhas / 13 processos). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `codusuario` | `nvarchar(50)` | sim | - | login do usuario (varchar) |
| 2 | `EMPRESA` | `nvarchar(120)` | sim | - |  |
| 3 | `CEN` | `nvarchar(60)` | sim | - |  |
| 4 | `JD_QUOTE` | `nvarchar(120)` | sim | - |  |
| 5 | `TIPO_VENDA` | `nvarchar(120)` | sim | - |  |
| 6 | `TIPO_DE_EQUIPAMENTO` | `nvarchar(120)` | sim | - |  |
| 7 | `MARCA` | `nvarchar(120)` | sim | - |  |
| 8 | `MODELO` | `nvarchar(200)` | sim | - |  |
| 9 | `SEQPESSOA` | `int(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 10 | `NOMERAZAO` | `nvarchar(200)` | sim | - |  |
| 11 | `CNPJCPF` | `nvarchar(32)` | sim | - |  |
| 12 | `LINHA_CREDITO` | `nvarchar(120)` | sim | - |  |
| 13 | `INST_FINANCEIRA` | `nvarchar(120)` | sim | - |  |
| 14 | `FINANC_CHASSI` | `nvarchar(120)` | sim | - |  |
| 15 | `ULT_RESULTADO` | `nvarchar(400)` | sim | - |  |
| 16 | `DTA_FATURAMENTO_D` | `datetime` | sim | - |  |
| 17 | `NF_N` | `nvarchar(60)` | sim | - |  |
| 18 | `DTA_PED_D` | `datetime` | sim | - |  |
| 19 | `DTA_PRENCH_FORM_D` | `datetime` | sim | - |  |
| 20 | `VALOR_N` | `decimal(18,2)` | sim | - |  |
| 21 | `DTA_ULT_ANDAMENTO_D` | `datetime` | sim | - |  |
| 22 | `ULT_HISTORICO_L` | `nvarchar(max)` | sim | - |  |
| 23 | `PROCESSO_N` | `int(10,0)` | sim | - |  |
| 24 | `CARTEIRA` | `nvarchar(60)` | sim | - |  |
| 25 | `CIDADE` | `nvarchar(150)` | sim | - |  |
| 26 | `DATA_PEDIDO_HIST_D` | `datetime` | sim | - |  |
| 27 | `CAMPANHA` | `nvarchar(150)` | sim | - |  |
| 28 | `PROCESSODNA` | `int(10,0)` | sim | - |  |
| 29 | `DTAPROVADO` | `nvarchar(10)` | sim | - |  |

---

### IMP_REL_TBA101_PG2

`classe: staging` · `31 colunas` · `208 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Staging de relatório (Análise da Carteira de Pedidos FY25), discriminado por codusuario. PG2 acumula sem DELETE: 1.558 linhas para muito menos processos (V:AMAURY 208 linhas / 13 processos). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `codusuario` | `nvarchar(50)` | sim | - | login do usuario (varchar) |
| 2 | `EMPRESA` | `nvarchar(120)` | sim | - |  |
| 3 | `CEN` | `nvarchar(60)` | sim | - |  |
| 4 | `JD_QUOTE` | `nvarchar(120)` | sim | - |  |
| 5 | `COMAR` | `decimal(8,0)` | sim | - |  |
| 6 | `TIPO_VENDA` | `nvarchar(120)` | sim | - |  |
| 7 | `TIPO_DE_EQUIPAMENTO` | `nvarchar(120)` | sim | - |  |
| 8 | `MARCA` | `nvarchar(120)` | sim | - |  |
| 9 | `MODELO` | `nvarchar(200)` | sim | - |  |
| 10 | `SEQPESSOA` | `int(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 11 | `NOMERAZAO` | `nvarchar(200)` | sim | - |  |
| 12 | `CNPJCPF` | `nvarchar(32)` | sim | - |  |
| 13 | `LINHA_CREDITO` | `nvarchar(120)` | sim | - |  |
| 14 | `INST_FINANCEIRA` | `nvarchar(120)` | sim | - |  |
| 15 | `FINANC_CHASSI` | `nvarchar(120)` | sim | - |  |
| 16 | `ULT_RESULTADO` | `nvarchar(400)` | sim | - |  |
| 17 | `DTA_FATURAMENTO_D` | `datetime` | sim | - |  |
| 18 | `NF_N` | `nvarchar(60)` | sim | - |  |
| 19 | `DTA_PED_D` | `datetime` | sim | - |  |
| 20 | `DTA_PRENCH_FORM_D` | `datetime` | sim | - |  |
| 21 | `VALOR_N` | `decimal(18,2)` | sim | - |  |
| 22 | `DTA_ULT_ANDAMENTO_D` | `datetime` | sim | - |  |
| 23 | `ULT_HISTORICO_L` | `nvarchar(max)` | sim | - |  |
| 24 | `PROCESSO_N` | `int(10,0)` | sim | - |  |
| 25 | `CARTEIRA` | `nvarchar(60)` | sim | - |  |
| 26 | `CIDADE` | `nvarchar(150)` | sim | - |  |
| 27 | `DATA_PEDIDO_HIST_D` | `datetime` | sim | - |  |
| 28 | `CAMPANHA` | `nvarchar(150)` | sim | - |  |
| 29 | `PROCESSODNA` | `int(10,0)` | sim | - |  |
| 30 | `DTAPROVADO` | `nvarchar(10)` | sim | - |  |
| 31 | `DATA_MARCADO_ENTREGUE` | `nvarchar(10)` | sim | - |  |

---

### IMP_Titulo

`classe: staging` · `42 colunas` · `823.952 linhas (snapshot 03/06/2026)` · `PK: idTitulo`

**Funcao:** Contas a receber. IMP_Titulo (42 col., 892.131 linhas) é o staging; EXT_Titulo (37 col., 577.925) o canônico; EXT_TituloMov (9 col., 1.134.039) os movimentos; EXT_TituloCmpl o texto livre. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `idTitulo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `PessoaLinkOrigem` | `varchar(20)` | sim | - |  |
| 5 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 6 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 7 | `NroCNPJ` | `numeric(18,0)` | sim | - |  |
| 8 | `DigCNPJ` | `numeric(2,0)` | sim | - |  |
| 9 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 10 | `Especie` | `varchar(20)` | sim | - |  |
| 11 | `NroTitulo` | `varchar(30)` | sim | - |  |
| 12 | `NroDocto` | `varchar(30)` | sim | - |  |
| 13 | `TipoCobranca` | `varchar(20)` | sim | - |  |
| 14 | `IndAtivo` | `numeric(1,0)` | sim | - |  |
| 15 | `IndQuitado` | `numeric(1,0)` | sim | - |  |
| 16 | `IndCobrJuridica` | `numeric(1,0)` | sim | - |  |
| 17 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 18 | `NroBanco` | `numeric(6,0)` | sim | - |  |
| 19 | `LocalCobranca` | `varchar(25)` | sim | - |  |
| 20 | `NroTituloBanco` | `varchar(30)` | sim | - |  |
| 21 | `DtaEmissao` | `datetime` | sim | - |  |
| 22 | `DtaVenctoOrig` | `datetime` | sim | - |  |
| 23 | `DtaVencto` | `datetime` | sim | - |  |
| 24 | `VlrOriginal` | `numeric(14,2)` | sim | - |  |
| 25 | `VlrAcrescimo` | `numeric(14,2)` | sim | - |  |
| 26 | `VlrAbatimento` | `numeric(14,2)` | sim | - |  |
| 27 | `VlrPago` | `numeric(14,2)` | sim | - |  |
| 28 | `VlrMov` | `numeric(14,2)` | sim | - |  |
| 29 | `Movimento` | `varchar(250)` | sim | - |  |
| 30 | `DtaUltPgto` | `datetime` | sim | - |  |
| 31 | `DtaQuitacao` | `datetime` | sim | - |  |
| 32 | `DtaUltAlteracao` | `datetime` | sim | - |  |
| 33 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 34 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 35 | `LinkStr` | `varchar(30)` | sim | - |  |
| 36 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 37 | `IdPessoa` | `numeric(18,0)` | sim | - |  |
| 38 | `CHAVESTR1` | `varchar(250)` | sim | - |  |
| 39 | `STATUSIMP` | `char(1)` | sim | - |  |
| 40 | `CODBARRAS` | `varchar(250)` | sim | - |  |
| 41 | `dtaimport` | `datetime` | sim | - |  |
| 42 | `CNPJx` | `varchar(20)` | sim | - |  |

---

### IMP_Titulo_bkp_11_04

`classe: lixo/backup` · `42 colunas` · `2.168 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `IMP` (staging de importacao). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (42 colunas) para manter o documento legivel._

---

### IMP_VEICULO

`classe: staging` · `49 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Idveiculo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `IMP` (staging de importacao). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Idveiculo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(10,0)` | sim | - | multiempresa - filial/empresa |
| 4 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 5 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 6 | `NroCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 7 | `DigCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 8 | `CNPJx` | `varchar(20)` | sim | - |  |
| 9 | `NroChassi` | `varchar(40)` | **nao** | - |  |
| 10 | `NroChassiRed` | `varchar(20)` | sim | - |  |
| 11 | `TipoVeiculo` | `varchar(5)` | sim | - |  |
| 12 | `Placa` | `varchar(9)` | sim | - |  |
| 13 | `Combustivel` | `varchar(30)` | sim | - |  |
| 14 | `Marca` | `varchar(20)` | sim | - |  |
| 15 | `CodFamilia` | `varchar(30)` | sim | - |  |
| 16 | `Familia` | `varchar(60)` | sim | - |  |
| 17 | `CodModelo` | `varchar(50)` | sim | - |  |
| 18 | `Modelo` | `varchar(50)` | sim | - |  |
| 19 | `CorExterna` | `varchar(30)` | sim | - |  |
| 20 | `CorInterna` | `varchar(30)` | sim | - |  |
| 21 | `Potencia` | `numeric(4,0)` | sim | - |  |
| 22 | `QtdeEixo` | `numeric(2,0)` | sim | - |  |
| 23 | `EstadoVenda` | `char(1)` | sim | - |  |
| 24 | `FormaPgto` | `char(1)` | sim | - |  |
| 25 | `Financiador` | `varchar(40)` | sim | - |  |
| 26 | `CanalVenda` | `varchar(20)` | sim | - |  |
| 27 | `Nronf` | `numeric(18,0)` | sim | - |  |
| 28 | `Serienf` | `varchar(12)` | sim | - |  |
| 29 | `Anofabricacao` | `numeric(4,0)` | sim | - |  |
| 30 | `AnoModelo` | `numeric(4,0)` | sim | - |  |
| 31 | `Dtavenda` | `datetime` | sim | - |  |
| 32 | `Dtaprevquitacao` | `datetime` | sim | - |  |
| 33 | `VlrVenda` | `numeric(14,2)` | sim | - |  |
| 34 | `Observacao` | `varchar(250)` | sim | - | texto livre |
| 35 | `KMAtual` | `numeric(8,0)` | sim | - |  |
| 36 | `DtaKMAtual` | `datetime` | sim | - |  |
| 37 | `KMProxRevisao` | `numeric(8,0)` | sim | - |  |
| 38 | `DtaProxRevisao` | `datetime` | sim | - |  |
| 39 | `Revenda` | `varchar(40)` | sim | - |  |
| 40 | `Vendedor` | `varchar(60)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 41 | `TipoUso` | `varchar(40)` | sim | - |  |
| 42 | `UsuarioAlteracao` | `varchar(20)` | sim | - |  |
| 43 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 44 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 45 | `StatusIMP` | `char(1)` | sim | - |  |
| 46 | `DtaImport` | `datetime` | sim | - |  |
| 47 | `CODVENDEDOR` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 48 | `DTAPRIMVENDA` | `datetime` | sim | - |  |
| 49 | `NROCPFVENDEDOR` | `numeric(11,0)` | sim | - |  |

---

### IMP_VEICULO_INTEGRADO

`classe: staging` · `50 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `IMP` (staging de importacao). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Idveicseq` | `numeric(18,0)` | **nao** | - |  |
| 2 | `Idveiculo` | `numeric(18,0)` | **nao** | - |  |
| 3 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 4 | `NroEmpresa` | `numeric(10,0)` | sim | - | multiempresa - filial/empresa |
| 5 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 6 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 7 | `NroCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 8 | `DigCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 9 | `CNPJx` | `varchar(20)` | sim | - |  |
| 10 | `NroChassi` | `varchar(40)` | **nao** | - |  |
| 11 | `NroChassiRed` | `varchar(20)` | sim | - |  |
| 12 | `TipoVeiculo` | `varchar(5)` | sim | - |  |
| 13 | `Placa` | `varchar(9)` | sim | - |  |
| 14 | `Combustivel` | `varchar(30)` | sim | - |  |
| 15 | `Marca` | `varchar(20)` | sim | - |  |
| 16 | `CodFamilia` | `varchar(30)` | sim | - |  |
| 17 | `Familia` | `varchar(60)` | sim | - |  |
| 18 | `CodModelo` | `varchar(50)` | sim | - |  |
| 19 | `Modelo` | `varchar(50)` | sim | - |  |
| 20 | `CorExterna` | `varchar(30)` | sim | - |  |
| 21 | `CorInterna` | `varchar(30)` | sim | - |  |
| 22 | `Potencia` | `numeric(4,0)` | sim | - |  |
| 23 | `QtdeEixo` | `numeric(2,0)` | sim | - |  |
| 24 | `EstadoVenda` | `char(1)` | sim | - |  |
| 25 | `FormaPgto` | `char(1)` | sim | - |  |
| 26 | `Financiador` | `varchar(40)` | sim | - |  |
| 27 | `CanalVenda` | `varchar(20)` | sim | - |  |
| 28 | `Nronf` | `numeric(18,0)` | sim | - |  |
| 29 | `Serienf` | `varchar(12)` | sim | - |  |
| 30 | `Anofabricacao` | `numeric(4,0)` | sim | - |  |
| 31 | `AnoModelo` | `numeric(4,0)` | sim | - |  |
| 32 | `Dtavenda` | `datetime` | sim | - |  |
| 33 | `Dtaprevquitacao` | `datetime` | sim | - |  |
| 34 | `VlrVenda` | `numeric(14,2)` | sim | - |  |
| 35 | `Observacao` | `varchar(250)` | sim | - | texto livre |
| 36 | `KMAtual` | `numeric(8,0)` | sim | - |  |
| 37 | `DtaKMAtual` | `datetime` | sim | - |  |
| 38 | `KMProxRevisao` | `numeric(8,0)` | sim | - |  |
| 39 | `DtaProxRevisao` | `datetime` | sim | - |  |
| 40 | `Revenda` | `varchar(40)` | sim | - |  |
| 41 | `Vendedor` | `varchar(60)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 42 | `TipoUso` | `varchar(40)` | sim | - |  |
| 43 | `UsuarioAlteracao` | `varchar(20)` | sim | - |  |
| 44 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 45 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 46 | `StatusIMP` | `char(1)` | sim | - |  |
| 47 | `DtaImport` | `datetime` | sim | - |  |
| 48 | `CODVENDEDOR` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 49 | `DTAPRIMVENDA` | `datetime` | sim | - |  |
| 50 | `NROCPFVENDEDOR` | `numeric(11,0)` | sim | - |  |

---
