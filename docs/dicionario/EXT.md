# EXT - Espelho de dados do ERP (equipamentos, notas, OS, titulos)

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**46 tabelas · 846 colunas · 15.504.403 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`EXT_AGUARDENTREGA`](#ext_aguardentrega) | isolada | 3 | 50 |
| [`EXT_APROVACAO`](#ext_aprovacao) | isolada | 3 | 2.224 |
| [`EXT_CONDPAGTO`](#ext_condpagto) | isolada | 3 | 21 |
| [`EXT_EMAIL`](#ext_email) | vazia | 15 | 0 |
| [`EXT_FormaPgto`](#ext_formapgto) | vazia | 6 | 0 |
| [`EXT_NFS`](#ext_nfs) | nucleo | 39 | 390.755 |
| [`EXT_NFSCmpl`](#ext_nfscmpl) | vazia | 5 | 0 |
| [`EXT_NFSItem`](#ext_nfsitem) | nucleo | 27 | 1.300.126 |
| [`EXT_NFSOper`](#ext_nfsoper) | catalogo | 9 | 241 |
| [`EXT_NFSOperEmp`](#ext_nfsoperemp) | nucleo | 3 | 330 |
| [`EXT_OS`](#ext_os) | nucleo | 40 | 8.099 |
| [`EXT_OSITEM`](#ext_ositem) | isolada | 15 | 182.238 |
| [`EXT_OSSolic`](#ext_ossolic) | vazia | 6 | 0 |
| [`EXT_Pedido`](#ext_pedido) | vazia | 57 | 0 |
| [`EXT_PedidoItem`](#ext_pedidoitem) | vazia | 22 | 0 |
| [`EXT_Pessoa`](#ext_pessoa) | nucleo | 77 | 57.682 |
| [`EXT_PESSOACONTATO`](#ext_pessoacontato) | vazia | 29 | 0 |
| [`EXT_PESSOAFONE`](#ext_pessoafone) | vazia | 20 | 0 |
| [`EXT_Pessoa_bkpago22`](#ext_pessoa_bkpago22) | lixo/backup | 77 | 27.280 |
| [`EXT_Pot_Pecas`](#ext_pot_pecas) | isolada | 13 | 1.287 |
| [`EXT_Produto`](#ext_produto) | nucleo | 27 | 78.749 |
| [`EXT_Titulo`](#ext_titulo) | nucleo | 37 | 577.925 |
| [`EXT_TituloCmpl`](#ext_titulocmpl) | vazia | 2 | 0 |
| [`EXT_TituloMov`](#ext_titulomov) | nucleo | 9 | 1.134.039 |
| [`EXT_Titulo_bkp_11_04`](#ext_titulo_bkp_11_04) | lixo/backup | 37 | 108.061 |
| [`EXT_Titulo_BKP_22_03_2023`](#ext_titulo_bkp_22_03_2023) | lixo/backup | 37 | 4.635 |
| [`EXT_Titulo_BKP_22_03_2023_BAIXADOS`](#ext_titulo_bkp_22_03_2023_baixados) | lixo/backup | 37 | 16.705 |
| [`EXT_TIT_ACRESC`](#ext_tit_acresc) | isolada | 2 | 1.575 |
| [`EXT_Veic`](#ext_veic) | nucleo | 28 | 8.020 |
| [`EXT_VeicAgd`](#ext_veicagd) | vazia | 28 | 0 |
| [`EXT_VEICAGDERP`](#ext_veicagderp) | vazia | 12 | 0 |
| [`EXT_VeicAvalFoto`](#ext_veicavalfoto) | vazia | 3 | 0 |
| [`EXT_VeicAvalia`](#ext_veicavalia) | vazia | 31 | 0 |
| [`EXT_VeicFam`](#ext_veicfam) | catalogo | 6 | 68 |
| [`EXT_VEICFAMREF`](#ext_veicfamref) | isolada | 2 | 23 |
| [`EXT_VeicKM`](#ext_veickm) | nucleo | 11 | 17.378 |
| [`EXT_VeicMarca`](#ext_veicmarca) | catalogo | 5 | 5 |
| [`EXT_VeicMarcaEmp`](#ext_veicmarcaemp) | nucleo | 4 | 2 |
| [`EXT_VeicModelo`](#ext_veicmodelo) | nucleo | 5 | 4.431.168 |
| [`EXT_VeicModPlano`](#ext_veicmodplano) | nucleo | 2 | 3.571.235 |
| [`EXT_VeicPlanoMan`](#ext_veicplanoman) | nucleo | 4 | 3.571.257 |
| [`EXT_VeicPlanoMnFX`](#ext_veicplanomnfx) | vazia | 7 | 0 |
| [`EXT_VeicProp`](#ext_veicprop) | nucleo | 24 | 12.621 |
| [`EXT_VEICREF`](#ext_veicref) | isolada | 3 | 40 |
| [`EXT_VeicTipoMan`](#ext_veictipoman) | catalogo | 8 | 1 |
| [`EXT_Vendedor`](#ext_vendedor) | catalogo | 6 | 563 |

---

### EXT_AGUARDENTREGA

`classe: isolada` · `3 colunas` · `50 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Apesar do prefixo EXT_, NÃO vêm de ERP: são tabelas de controle escritas pelas procedures locais VTC_P_GERAAPROVACAO / VTC_P_GERAAGUARDENTREGA / VTC_P_GERACONDPAGTO, que rodam a cada 5 minutos e estão entre os poucos jobs vivos (30/08/2026). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** | - | numero do processo (`IV_Processo.Processo`) |
| 2 | `DTAGERACAO` | `datetime` | sim | - |  |
| 3 | `processodna` | `numeric(18,0)` | sim | - |  |

---

### EXT_APROVACAO

`classe: isolada` · `3 colunas` · `2.224 linhas (snapshot 03/06/2026)` · `PK: PROCESSO`

**Funcao:** Apesar do prefixo EXT_, NÃO vêm de ERP: são tabelas de controle escritas pelas procedures locais VTC_P_GERAAPROVACAO / VTC_P_GERAAGUARDENTREGA / VTC_P_GERACONDPAGTO, que rodam a cada 5 minutos e estão entre os poucos jobs vivos (30/08/2026). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `DTAGERACAO` | `datetime` | sim | - |  |
| 3 | `processodna` | `numeric(18,0)` | sim | - |  |

---

### EXT_CONDPAGTO

`classe: isolada` · `3 colunas` · `21 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Apesar do prefixo EXT_, NÃO vêm de ERP: são tabelas de controle escritas pelas procedures locais VTC_P_GERAAPROVACAO / VTC_P_GERAAGUARDENTREGA / VTC_P_GERACONDPAGTO, que rodam a cada 5 minutos e estão entre os poucos jobs vivos (30/08/2026). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** | - | numero do processo (`IV_Processo.Processo`) |
| 2 | `DTAGERACAO` | `datetime` | sim | - |  |
| 3 | `processodna` | `numeric(18,0)` | sim | - |  |

---

### EXT_EMAIL

`classe: vazia` · `15 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDEMAIL`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDEMAIL` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `PESSOALINKORIGEM` | `varchar(20)` | **nao** | - |  |
| 3 | `PESSOALINK` | `varchar(250)` | sim | - |  |
| 4 | `EMAILLINK` | `varchar(70)` | **nao** | - |  |
| 5 | `EMAIL` | `varchar(70)` | **nao** | - |  |
| 6 | `INDEMUSO` | `numeric(1,0)` | sim | - |  |
| 7 | `INDPREFERENCIAL` | `numeric(1,0)` | sim | - |  |
| 8 | `INDUSOMKT` | `numeric(1,0)` | sim | - |  |
| 9 | `INDUSOPESSOAL` | `numeric(1,0)` | sim | - |  |
| 10 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 11 | `OBS` | `varchar(50)` | sim | - | texto livre |
| 12 | `DTAGERACAO` | `datetime` | **nao** | - |  |
| 13 | `STATUSIMP` | `char(1)` | sim | - |  |
| 14 | `SEQPESSOA` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 15 | `SEQEMAIL` | `numeric(10,0)` | sim | - |  |

---

### EXT_FormaPgto

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdNFS, IdFormaPgto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de forma/condicao de pagamento, no modulo `EXT` (espelho de dados do ERP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdNFS` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `EXT_NFS.IdNFS` |
| 2 | `IdFormaPgto` | `numeric(6,0)` | **nao** | - | **PK** |
| 3 | `DescrFormaPgto` | `varchar(50)` | sim | - |  |
| 4 | `VLR` | `numeric(14,2)` | sim | - |  |
| 5 | `DtaVencto` | `datetime` | sim | - |  |
| 6 | `OBS` | `varchar(100)` | sim | - | texto livre |

---

### EXT_NFS

`classe: nucleo` · `39 colunas` · `390.755 linhas (snapshot 03/06/2026)` · `PK: IdNFS`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdNFS` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `NroNF` | `numeric(18,0)` | **nao** | - |  |
| 5 | `SerieNF` | `varchar(12)` | sim | - |  |
| 6 | `NroEmpresaVda` | `numeric(6,0)` | sim | - |  |
| 7 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 8 | `IdVeic` | `numeric(18,0)` | sim | - | FK -> `EXT_Veic.IdVeic`; equipamento (`EXT_Veic.IdVeic`) |
| 9 | `IdNFSOper` | `numeric(8,0)` | **nao** | - | FK -> `EXT_NFSOper.IdNFSOper` |
| 10 | `IdVendedor` | `numeric(18,0)` | sim | - | FK -> `EXT_Vendedor.IdVendedor` |
| 11 | `SeqDepto` | `numeric(4,0)` | sim | - | departamento |
| 12 | `CFOP` | `numeric(8,0)` | sim | - |  |
| 13 | `TipoVenda` | `varchar(30)` | sim | - |  |
| 14 | `CanalVenda` | `varchar(20)` | sim | - |  |
| 15 | `Setor` | `varchar(30)` | sim | - |  |
| 16 | `FormaPgto` | `varchar(25)` | sim | - |  |
| 17 | `CondicaoPgto` | `varchar(60)` | sim | - |  |
| 18 | `DtaPedido` | `datetime` | sim | - |  |
| 19 | `NroPedido` | `varchar(20)` | sim | - |  |
| 20 | `DtaEmissaoNF` | `datetime` | sim | - |  |
| 21 | `Situacao` | `char(1)` | sim | - |  |
| 22 | `CodTransportador` | `varchar(20)` | sim | - |  |
| 23 | `Transportador` | `varchar(40)` | sim | - |  |
| 24 | `Usuario` | `varchar(20)` | sim | - |  |
| 25 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 26 | `OBS` | `varchar(250)` | sim | - | texto livre |
| 27 | `DtaImport` | `datetime` | sim | - |  |
| 28 | `Processar` | `numeric(1,0)` | sim | - |  |
| 29 | `StatusDWH` | `numeric(1,0)` | sim | - |  |
| 30 | `IndEstorno` | `numeric(1,0)` | sim | - |  |
| 31 | `Segmento` | `varchar(30)` | sim | - |  |
| 32 | `PERCBASECOMISSAO` | `numeric(5,2)` | sim | - |  |
| 33 | `IDNFSEXTERNO` | `varchar(40)` | sim | - |  |
| 34 | `PAIIDNFSEXTERNO` | `varchar(40)` | sim | - |  |
| 35 | `TIPOPEDIDO` | `varchar(40)` | sim | - |  |
| 36 | `NROCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 37 | `DIGCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 38 | `IDENTIFICADO` | `varchar(20)` | sim | - |  |
| 39 | `NROVOUCHER` | `varchar(50)` | sim | - |  |

**Referenciada por:** `EXT_FormaPgto.IdNFS`, `EXT_NFSCmpl.IdNFS`, `EXT_NFSItem.IdNFS`

---

### EXT_NFSCmpl

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdNFS, Idcmpl`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdNFS` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `EXT_NFS.IdNFS` |
| 2 | `Idcmpl` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `CMPL` | `varchar(250)` | sim | - |  |
| 4 | `Dtaimport` | `datetime` | sim | - |  |
| 5 | `IDNFSEXTERNO` | `varchar(40)` | sim | - |  |

---

### EXT_NFSItem

`classe: nucleo` · `27 colunas` · `1.300.126 linhas (snapshot 03/06/2026)` · `PK: IdNFS, IdItem`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdNFS` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `EXT_NFS.IdNFS` |
| 2 | `IdItem` | `numeric(6,0)` | **nao** | - | **PK** |
| 3 | `IdProduto` | `numeric(18,0)` | sim | - | FK -> `EXT_Produto.IdProduto` |
| 4 | `IdVendedor` | `numeric(18,0)` | sim | - | FK -> `EXT_Vendedor.IdVendedor` |
| 5 | `SeqDepto` | `numeric(4,0)` | sim | - | departamento |
| 6 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 7 | `Qtde` | `numeric(10,3)` | sim | - |  |
| 8 | `VlrLiqItem` | `numeric(14,2)` | sim | - |  |
| 9 | `vlrUnitario` | `numeric(14,2)` | sim | - |  |
| 10 | `Vlrdescto` | `numeric(14,2)` | sim | - |  |
| 11 | `VlrResult` | `numeric(14,2)` | sim | - |  |
| 12 | `VlrCustoMkt` | `numeric(14,2)` | sim | - |  |
| 13 | `VlrICM` | `numeric(14,2)` | sim | - |  |
| 14 | `VlrImposto` | `numeric(14,2)` | sim | - |  |
| 15 | `OBS` | `varchar(250)` | sim | - | texto livre |
| 16 | `Situacao` | `char(1)` | sim | - |  |
| 17 | `IndEstorno` | `numeric(1,0)` | sim | - |  |
| 18 | `StatusDWH` | `numeric(1,0)` | sim | - |  |
| 19 | `DtaImport` | `datetime` | sim | - |  |
| 20 | `SETORITEM` | `varchar(30)` | sim | - |  |
| 21 | `VLRCUSTO` | `numeric(14,2)` | sim | - |  |
| 22 | `IDNFSEXTERNO` | `varchar(40)` | sim | - |  |
| 23 | `VLRICMSRETIDO` | `numeric(14,2)` | sim | - |  |
| 24 | `VLRICMSSUBS` | `numeric(14,2)` | sim | - |  |
| 25 | `VLRPIS` | `numeric(14,2)` | sim | - |  |
| 26 | `VLRCOFINS` | `numeric(14,2)` | sim | - |  |
| 27 | `VLRDESCTOVCHR` | `numeric(14,2)` | sim | - |  |

---

### EXT_NFSOper

`classe: catalogo` · `9 colunas` · `241 linhas (snapshot 03/06/2026)` · `PK: IdNFSOper`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdNFSOper` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `SeqDepto` | `numeric(4,0)` | sim | - | departamento |
| 3 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 4 | `CodOperacao` | `varchar(20)` | **nao** | - |  |
| 5 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 6 | `DescReduzida` | `varchar(12)` | sim | - |  |
| 7 | `EntradaSaida` | `char(1)` | **nao** | - |  |
| 8 | `IndEstorno` | `numeric(1,0)` | **nao** | - |  |
| 9 | `FormaLeitura` | `char(1)` | sim | - |  |

**Referenciada por:** `EXT_NFS.IdNFSOper`, `EXT_NFSOperEmp.IdNFSOper`

---

### EXT_NFSOperEmp

`classe: nucleo` · `3 colunas` · `330 linhas (snapshot 03/06/2026)` · `PK: NroEmpresa, IdNFSOper`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 2 | `IdNFSOper` | `numeric(8,0)` | **nao** | - | **PK**; FK -> `EXT_NFSOper.IdNFSOper` |
| 3 | `SeqDepto` | `numeric(4,0)` | sim | - | departamento |

---

### EXT_OS

`classe: nucleo` · `40 colunas` · `8.099 linhas (snapshot 03/06/2026)` · `PK: IDOS`

**Funcao:** Ordem de serviço (pós-venda/oficina). IMP_OS 46 col./287.868 linhas vs EXT_OS 40 col./8.099 — 280.214 nunca promovidas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDOS` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 4 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 5 | `Nroos` | `numeric(18,0)` | sim | - |  |
| 6 | `SerieOS` | `varchar(12)` | sim | - |  |
| 7 | `IdVeic` | `numeric(18,0)` | sim | - | FK -> `EXT_Veic.IdVeic`; equipamento (`EXT_Veic.IdVeic`) |
| 8 | `SeqDepto` | `numeric(4,0)` | sim | - | departamento |
| 9 | `Nrochassi` | `varchar(40)` | sim | - |  |
| 10 | `Placa` | `varchar(9)` | sim | - |  |
| 11 | `Combustivel` | `varchar(15)` | sim | - |  |
| 12 | `Codveiculo` | `varchar(20)` | sim | - |  |
| 13 | `Modelo` | `varchar(30)` | sim | - |  |
| 14 | `Corveiculo` | `varchar(20)` | sim | - |  |
| 15 | `Anofabricacao` | `numeric(4,0)` | sim | - |  |
| 16 | `AnoModelo` | `numeric(4,0)` | sim | - |  |
| 17 | `Dtavenda` | `datetime` | sim | - |  |
| 18 | `Consultor` | `varchar(30)` | sim | - |  |
| 19 | `TipoOS` | `varchar(30)` | sim | - |  |
| 20 | `Dtaabertura` | `datetime` | sim | - |  |
| 21 | `Dtaencerramento` | `datetime` | sim | - |  |
| 22 | `Dtafechamento` | `datetime` | sim | - |  |
| 23 | `VlrLiqPecas` | `numeric(14,2)` | sim | - |  |
| 24 | `VlrLiqServicos` | `numeric(14,2)` | sim | - |  |
| 25 | `Observacao` | `varchar(250)` | sim | - | texto livre |
| 26 | `Nrodn` | `varchar(10)` | sim | - |  |
| 27 | `Kilometragem` | `numeric(8,0)` | sim | - |  |
| 28 | `Usuario` | `varchar(20)` | sim | - |  |
| 29 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 30 | `Dtaimport` | `datetime` | sim | - |  |
| 31 | `Processar` | `numeric(1,0)` | sim | - |  |
| 32 | `StatusDWH` | `numeric(1,0)` | sim | - |  |
| 33 | `Marca` | `varchar(20)` | sim | - |  |
| 34 | `Familia` | `varchar(60)` | sim | - |  |
| 35 | `CodFamilia` | `varchar(30)` | sim | - |  |
| 36 | `IDOSEXTERNO` | `varchar(40)` | sim | - |  |
| 37 | `SITUACAO` | `char(1)` | sim | - |  |
| 38 | `CODTIPOOS` | `varchar(10)` | sim | - |  |
| 39 | `DTAALTERACAOERP` | `datetime` | sim | - |  |
| 40 | `TIPOSERVICO` | `varchar(30)` | sim | - |  |

**Referenciada por:** `EXT_OSSolic.IDOS`, `EXT_VeicKM.IDOS`

---

### EXT_OSITEM

`classe: isolada` · `15 colunas` · `182.238 linhas (snapshot 03/06/2026)` · `PK: IDOS, IDITEM`

**Funcao:** Ordem de serviço (pós-venda/oficina). IMP_OS 46 col./287.868 linhas vs EXT_OS 40 col./8.099 — 280.214 nunca promovidas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDOS` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IDITEM` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `TIPOITEM` | `char(1)` | sim | - |  |
| 4 | `PRODUTIVO` | `varchar(30)` | sim | - |  |
| 5 | `CODIGO` | `varchar(40)` | sim | - |  |
| 6 | `DESCRICAO` | `varchar(100)` | sim | - | descricao do registro |
| 7 | `QTDE` | `numeric(10,2)` | sim | - |  |
| 8 | `OBS` | `varchar(250)` | sim | - | texto livre |
| 9 | `DTAIMPORT` | `datetime` | sim | - |  |
| 10 | `STATUSITEM` | `char(1)` | sim | - |  |
| 11 | `VLRTOTITEM` | `numeric(14,2)` | sim | - |  |
| 12 | `VLRTOTITEMBRUTO` | `numeric(14,2)` | sim | - |  |
| 13 | `IDOSEXTERNO` | `varchar(40)` | sim | - |  |
| 14 | `MAODEOBRA` | `varchar(30)` | sim | - |  |
| 15 | `GRUPOITEM` | `varchar(20)` | sim | - |  |

---

### EXT_OSSolic

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDOS, Idsolic`

**Funcao:** Ordem de serviço (pós-venda/oficina). IMP_OS 46 col./287.868 linhas vs EXT_OS 40 col./8.099 — 280.214 nunca promovidas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDOS` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `EXT_OS.IDOS` |
| 2 | `Idsolic` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `Codigo` | `varchar(16)` | sim | - |  |
| 4 | `Descricao` | `varchar(250)` | sim | - | descricao do registro |
| 5 | `IDOSEXTERNO` | `varchar(40)` | sim | - |  |
| 6 | `DTAIMPORT` | `datetime` | sim | - |  |

---

### EXT_Pedido

`classe: vazia` · `57 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdPedido`

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
| 7 | `NroEmpresa` | `decimal(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 8 | `NroPedido` | `varchar(20)` | **nao** | - |  |
| 9 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 10 | `IdPessoa` | `numeric(18,0)` | sim | - | FK -> `EXT_Pessoa.IdPessoa` |
| 11 | `CodOperacao` | `varchar(12)` | sim | - |  |
| 12 | `Operacao` | `varchar(40)` | sim | - |  |
| 13 | `CodVendedor` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 14 | `Vendedor` | `varchar(40)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 15 | `CanalVenda` | `varchar(20)` | sim | - |  |
| 16 | `NroPedCliente` | `varchar(50)` | sim | - |  |
| 17 | `NroPedidoERP` | `varchar(30)` | sim | - |  |
| 18 | `FormaPgto` | `varchar(15)` | sim | - |  |
| 19 | `CondicaoPgto` | `varchar(60)` | sim | - |  |
| 20 | `AtivoReceptivo` | `char(1)` | sim | - |  |
| 21 | `DtaBaseFatura` | `datetime` | sim | - |  |
| 22 | `NroDocto` | `decimal(12,0)` | sim | - |  |
| 23 | `Serie` | `varchar(4)` | sim | - |  |
| 24 | `DtaPedido` | `datetime` | sim | - |  |
| 25 | `DtaValidade` | `datetime` | sim | - |  |
| 26 | `DtaFechamento` | `datetime` | sim | - |  |
| 27 | `DtaProxContato` | `datetime` | sim | - |  |
| 28 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 29 | `IndEstRservado` | `numeric(1,0)` | sim | - |  |
| 30 | `IndEnvioERP` | `numeric(1,0)` | sim | - |  |
| 31 | `DtaEnvioERP` | `datetime` | sim | - |  |
| 32 | `ObsNF` | `varchar(250)` | sim | - |  |
| 33 | `ObsInterna` | `varchar(250)` | sim | - |  |
| 34 | `ObsDesconto` | `varchar(200)` | sim | - |  |
| 35 | `ExigeAssinatura` | `varchar(1)` | sim | - |  |
| 36 | `AssinaturaDesc` | `varchar(20)` | sim | - |  |
| 37 | `AssinaturaDta` | `datetime` | sim | - |  |
| 38 | `EntradaSaida` | `varchar(1)` | sim | - |  |
| 39 | `MotivoVP` | `varchar(20)` | sim | - |  |
| 40 | `TipoEntrega` | `varchar(20)` | sim | - |  |
| 41 | `TipoFrete` | `varchar(20)` | sim | - |  |
| 42 | `CodTransportador` | `varchar(20)` | sim | - |  |
| 43 | `Transportador` | `varchar(40)` | sim | - |  |
| 44 | `PedidoLinkNro` | `numeric(18,0)` | sim | - |  |
| 45 | `PedidoLinkStr` | `varchar(20)` | sim | - |  |
| 46 | `USUARIO` | `varchar(20)` | sim | - |  |
| 47 | `DEPARTAMENTO` | `varchar(30)` | sim | - | departamento |
| 48 | `SEGMENTO` | `varchar(30)` | sim | - |  |
| 49 | `STATUSCMPL` | `varchar(40)` | sim | - |  |
| 50 | `PERCBASECOMISSAO` | `numeric(5,2)` | sim | - |  |
| 51 | `PEDIDOLINKORIGEM` | `varchar(20)` | sim | - |  |
| 52 | `PAILINKORIGEM` | `varchar(20)` | sim | - |  |
| 53 | `PAILINKNRO` | `numeric(18,0)` | sim | - |  |
| 54 | `PAILINKSTR` | `varchar(20)` | sim | - |  |
| 55 | `PROCESSO` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 56 | `VLRDESCADICIONAL` | `numeric(15,2)` | sim | - |  |
| 57 | `DTAALTERACAOERP` | `datetime` | sim | - |  |

**Referenciada por:** `EXT_PedidoItem.IdPedido`

---

### EXT_PedidoItem

`classe: vazia` · `22 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdPedido, SeqItem`

**Funcao:** Pedido de venda — a ÚNICA cadeia modelada para saída CRM→ERP e a única EXT_ com coluna PROCESSO. Todas com 0 linhas: nunca usada na Tracbel. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdPedido` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `EXT_Pedido.IdPedido` |
| 2 | `SeqItem` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `IdProduto` | `numeric(18,0)` | sim | - | FK -> `EXT_Produto.IdProduto` |
| 4 | `NroEmpresa` | `decimal(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 5 | `CodTabelaPreco` | `varchar(12)` | sim | - |  |
| 6 | `PrecoTabela` | `decimal(15,2)` | sim | - |  |
| 7 | `Qtde` | `decimal(10,2)` | sim | - |  |
| 8 | `QtdeAtendida` | `decimal(10,2)` | sim | - |  |
| 9 | `IndEstRservado` | `numeric(1,0)` | sim | - |  |
| 10 | `VlrUnitario` | `decimal(15,4)` | sim | - |  |
| 11 | `VlrDescUnitCml` | `decimal(10,3)` | sim | - |  |
| 12 | `VlrDescUnitAuto` | `decimal(10,3)` | sim | - |  |
| 13 | `VlrDescTotal` | `decimal(15,2)` | sim | - |  |
| 14 | `DescConciliado` | `numeric(1,0)` | sim | - |  |
| 15 | `VlrBaseICM` | `decimal(15,2)` | sim | - |  |
| 16 | `VlrICM` | `decimal(15,2)` | sim | - |  |
| 17 | `AliqICM` | `decimal(15,2)` | sim | - |  |
| 18 | `VlrICMSubs` | `decimal(15,2)` | sim | - |  |
| 19 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 20 | `MotivoVP` | `varchar(20)` | sim | - |  |
| 21 | `ObsItem` | `varchar(50)` | sim | - |  |
| 22 | `IndSugestao` | `char(2)` | sim | - |  |

---

### EXT_Pessoa

`classe: nucleo` · `77 colunas` · `57.682 linhas (snapshot 03/06/2026)` · `PK: IdPessoa`

**Funcao:** Espelho da pessoa no ERP (77 colunas, 58.171 linhas). Serve de 3º nível na resolução de identidade quando GE_PessoaLink não resolve. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdPessoa` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `PessoaLinkOrigem` | `varchar(20)` | **nao** | - |  |
| 4 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 5 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 6 | `Status` | `char(1)` | **nao** | - | status - validar dominio real por tabela |
| 7 | `NomeRazao` | `varchar(100)` | sim | - |  |
| 8 | `Fantasia` | `varchar(50)` | sim | - |  |
| 9 | `PalavraChave` | `varchar(50)` | sim | - |  |
| 10 | `FisicaJuridica` | `char(1)` | sim | - |  |
| 11 | `SEXO` | `char(1)` | sim | - |  |
| 12 | `Cidade` | `varchar(50)` | sim | - |  |
| 13 | `UF` | `varchar(2)` | sim | - |  |
| 14 | `PAIS` | `varchar(25)` | sim | - |  |
| 15 | `Bairro` | `varchar(50)` | sim | - |  |
| 16 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 17 | `Logradouro` | `varchar(80)` | sim | - |  |
| 18 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 19 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 20 | `CEP` | `varchar(12)` | sim | - |  |
| 21 | `CxPostal` | `varchar(7)` | sim | - |  |
| 22 | `RefEndereco` | `varchar(150)` | sim | - |  |
| 23 | `InscMunic` | `varchar(15)` | sim | - |  |
| 24 | `InscProdutor` | `varchar(20)` | sim | - |  |
| 25 | `CNAE` | `varchar(15)` | sim | - |  |
| 26 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 27 | `FoneNro1` | `varchar(30)` | sim | - |  |
| 28 | `FoneCmpl1` | `varchar(12)` | sim | - |  |
| 29 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 30 | `FoneNro2` | `varchar(30)` | sim | - |  |
| 31 | `FoneCmpl2` | `varchar(12)` | sim | - |  |
| 32 | `FoneDDD3` | `varchar(5)` | sim | - |  |
| 33 | `FoneNro3` | `varchar(30)` | sim | - |  |
| 34 | `FoneCmpl3` | `varchar(12)` | sim | - |  |
| 35 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 36 | `FaxNro` | `varchar(20)` | sim | - |  |
| 37 | `NroCNPJCPF` | `numeric(13,0)` | sim | - |  |
| 38 | `DigCNPJCPF` | `numeric(2,0)` | sim | - |  |
| 39 | `CNPJx` | `varchar(20)` | sim | - |  |
| 40 | `InscricaoRG` | `varchar(20)` | sim | - |  |
| 41 | `UFEmissor` | `varchar(2)` | sim | - |  |
| 42 | `OrgaoEmissor` | `varchar(10)` | sim | - |  |
| 43 | `DtaNASC` | `datetime` | sim | - |  |
| 44 | `Email` | `varchar(70)` | sim | - |  |
| 45 | `HomePage` | `varchar(80)` | sim | - |  |
| 46 | `EstadoCivil` | `varchar(20)` | sim | - |  |
| 47 | `Atividade` | `varchar(30)` | sim | - |  |
| 48 | `RendaFaturamento` | `varchar(30)` | sim | - |  |
| 49 | `GrauInstrucao` | `varchar(30)` | sim | - |  |
| 50 | `Grupo` | `varchar(30)` | sim | - |  |
| 51 | `Porte` | `varchar(30)` | sim | - |  |
| 52 | `CodRegiao` | `varchar(30)` | sim | - |  |
| 53 | `Regiao` | `varchar(40)` | sim | - |  |
| 54 | `CodRota` | `varchar(30)` | sim | - |  |
| 55 | `ROTA` | `varchar(40)` | sim | - |  |
| 56 | `PessoaLinkVincOrig` | `varchar(20)` | sim | - |  |
| 57 | `PessoaLinkVinc` | `varchar(30)` | sim | - |  |
| 58 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 59 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 60 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 61 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 62 | `CODVENDEDOR` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 63 | `NaoPossuiEmail` | `numeric(1,0)` | sim | - |  |
| 64 | `ProblemaCredito` | `numeric(1,0)` | sim | - |  |
| 65 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 66 | `DtaGeracao` | `datetime` | **nao** | - |  |
| 67 | `StatusIMP` | `char(1)` | sim | - |  |
| 68 | `DtaImport` | `datetime` | sim | - |  |
| 69 | `CLASSES` | `varchar(250)` | sim | - |  |
| 70 | `VLRLIMITECREDITO` | `numeric(14,2)` | sim | - |  |
| 71 | `SITUACAOCREDITO` | `varchar(30)` | sim | - |  |
| 72 | `VLRSALDOCREDITO` | `numeric(14,2)` | sim | - |  |
| 73 | `STATUSSINTEGRA` | `varchar(50)` | sim | - |  |
| 74 | `DADOADICIONAL1` | `varchar(50)` | sim | - |  |
| 75 | `DADOADICIONAL2` | `varchar(100)` | sim | - |  |
| 76 | `DEPTO` | `varchar(250)` | sim | - |  |
| 77 | `CARTEIRA` | `varchar(250)` | sim | - |  |

**Referenciada por:** `EXT_Pedido.IdPessoa`, `EXT_Titulo.IdPessoa`

---

### EXT_PESSOACONTATO

`classe: vazia` · `29 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDPESSOACTTO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IDPESSOACTTO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `PESSOALINKORIGEM` | `varchar(20)` | **nao** | - |  |
| 3 | `PESSOALINK` | `varchar(250)` | sim | - |  |
| 4 | `CONTATOLINK` | `varchar(50)` | **nao** | - |  |
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
| 19 | `PAPEL1` | `varchar(30)` | sim | - |  |
| 20 | `PAPEL2` | `varchar(30)` | sim | - |  |
| 21 | `PAPEL3` | `varchar(30)` | sim | - |  |
| 22 | `OBSERVACAO` | `varchar(250)` | sim | - | texto livre |
| 23 | `DTAGERACAO` | `datetime` | **nao** | - |  |
| 24 | `STATUSIMPSP` | `char(1)` | sim | - |  |
| 25 | `STATUSIMP` | `char(1)` | sim | - |  |
| 26 | `SEQPESSOA` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 27 | `SEQCONTATO` | `numeric(4,0)` | sim | - |  |
| 28 | `INDWHATSAPPF1` | `numeric(1,0)` | sim | - |  |
| 29 | `INDWHATSAPPF2` | `numeric(1,0)` | sim | - |  |

---

### EXT_PESSOAFONE

`classe: vazia` · `20 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IDPESSOAFONE`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

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
| 20 | `SEQPESFONE` | `numeric(18,0)` | sim | - |  |

---

### EXT_Pessoa_bkpago22

`classe: lixo/backup` · `77 colunas` · `27.280 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (77 colunas) para manter o documento legivel._

---

### EXT_Pot_Pecas

`classe: isolada` · `13 colunas` · `1.287 linhas (snapshot 03/06/2026)` · `PK: SEQ`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQ` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `JAN` | `numeric(14,2)` | sim | - |  |
| 3 | `FEV` | `numeric(14,2)` | sim | - |  |
| 4 | `MAR` | `numeric(14,2)` | sim | - |  |
| 5 | `ABR` | `numeric(14,2)` | sim | - |  |
| 6 | `MAI` | `numeric(14,2)` | sim | - |  |
| 7 | `JUN` | `numeric(14,2)` | sim | - |  |
| 8 | `JUL` | `numeric(14,2)` | sim | - |  |
| 9 | `AGO` | `numeric(14,2)` | sim | - |  |
| 10 | `SET` | `numeric(14,2)` | sim | - |  |
| 11 | `OUT` | `numeric(14,2)` | sim | - |  |
| 12 | `NOV` | `numeric(14,2)` | sim | - |  |
| 13 | `DEZ` | `numeric(14,2)` | sim | - |  |

---

### EXT_Produto

`classe: nucleo` · `27 colunas` · `78.749 linhas (snapshot 03/06/2026)` · `PK: IdProduto`

**Funcao:** Catálogos vindos do ERP. EXT_Produto 27 col./78.749 linhas (Protheus 35.027, SISDIA 27.859, 15.860 sem origem); EXT_Vendedor 6 col./563 (SISDIA 347, sem origem 188, Protheus 28); IMP_PRODUTO 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdProduto` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CodProduto` | `varchar(50)` | sim | - |  |
| 3 | `CodBarra` | `numeric(18,0)` | sim | - |  |
| 4 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 5 | `Tipo` | `char(1)` | sim | - |  |
| 6 | `Marca` | `varchar(30)` | sim | - |  |
| 7 | `CodFamilia` | `varchar(30)` | sim | - |  |
| 8 | `Familia` | `varchar(60)` | sim | - |  |
| 9 | `DescricaoCompleta` | `varchar(250)` | sim | - |  |
| 10 | `NCM` | `numeric(10,0)` | sim | - |  |
| 11 | `PrecoPublico` | `numeric(14,2)` | sim | - |  |
| 12 | `Preco1` | `numeric(14,2)` | sim | - |  |
| 13 | `Preco2` | `numeric(14,2)` | sim | - |  |
| 14 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 15 | `Dtaimport` | `datetime` | sim | - |  |
| 16 | `FORNECEDORPRINCIPAL` | `varchar(70)` | sim | - |  |
| 17 | `CATEGORIA1` | `varchar(50)` | sim | - |  |
| 18 | `CATEGORIA2` | `varchar(50)` | sim | - |  |
| 19 | `CATEGORIA3` | `varchar(50)` | sim | - |  |
| 20 | `CATEGORIA4` | `varchar(50)` | sim | - |  |
| 21 | `CATEGORIA5` | `varchar(50)` | sim | - |  |
| 22 | `CATEGORIA6` | `varchar(50)` | sim | - |  |
| 23 | `ORIGEM` | `varchar(20)` | sim | - | sistema de origem do dado |
| 24 | `CHAVEPRODUTOERP` | `varchar(50)` | sim | - |  |
| 25 | `QTDEESTOQUE` | `numeric(10,2)` | sim | - |  |
| 26 | `CUSTOESTOQUE` | `numeric(15,4)` | sim | - |  |
| 27 | `DTAESTOQUE` | `datetime` | sim | - |  |

**Referenciada por:** `EXT_NFSItem.IdProduto`, `EXT_PedidoItem.IdProduto`

---

### EXT_Titulo

`classe: nucleo` · `37 colunas` · `577.925 linhas (snapshot 03/06/2026)` · `PK: idTitulo`

**Funcao:** Contas a receber. IMP_Titulo (42 col., 892.131 linhas) é o staging; EXT_Titulo (37 col., 577.925) o canônico; EXT_TituloMov (9 col., 1.134.039) os movimentos; EXT_TituloCmpl o texto livre. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `idTitulo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 5 | `IdPessoa` | `numeric(18,0)` | sim | - | FK -> `EXT_Pessoa.IdPessoa` |
| 6 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 7 | `Especie` | `varchar(20)` | sim | - |  |
| 8 | `NroTitulo` | `varchar(30)` | sim | - |  |
| 9 | `NroDocto` | `varchar(30)` | sim | - |  |
| 10 | `TipoCobranca` | `varchar(20)` | sim | - |  |
| 11 | `IndAtivo` | `numeric(1,0)` | sim | - |  |
| 12 | `IndQuitado` | `numeric(1,0)` | sim | - |  |
| 13 | `IndCobrJuridica` | `numeric(1,0)` | sim | - |  |
| 14 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 15 | `NroBanco` | `numeric(6,0)` | sim | - |  |
| 16 | `LocalCobranca` | `varchar(25)` | sim | - |  |
| 17 | `NroTituloBanco` | `varchar(30)` | sim | - |  |
| 18 | `DtaEmissao` | `datetime` | sim | - |  |
| 19 | `DtaVenctoOrig` | `datetime` | sim | - |  |
| 20 | `DtaVencto` | `datetime` | sim | - |  |
| 21 | `VlrOriginal` | `numeric(14,2)` | sim | - |  |
| 22 | `VlrAcrescimo` | `numeric(14,2)` | sim | - |  |
| 23 | `VlrAbatimento` | `numeric(14,2)` | sim | - |  |
| 24 | `VlrPago` | `numeric(14,2)` | sim | - |  |
| 25 | `VlrAberto` | `numeric(14,2)` | sim | - |  |
| 26 | `VlrMov` | `numeric(14,2)` | sim | - |  |
| 27 | `Movimento` | `varchar(250)` | sim | - |  |
| 28 | `DtaUltPgto` | `datetime` | sim | - |  |
| 29 | `DtaQuitacao` | `datetime` | sim | - |  |
| 30 | `DtaUltAlteracao` | `datetime` | sim | - |  |
| 31 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 32 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 33 | `LinkStr` | `varchar(30)` | sim | - |  |
| 34 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 35 | `CHAVESTR1` | `varchar(250)` | sim | - |  |
| 36 | `CODBARRAS` | `varchar(250)` | sim | - |  |
| 37 | `dtaimport` | `datetime` | sim | - |  |

**Referenciada por:** `EXT_TituloCmpl.idTitulo`, `EXT_TituloMov.idTitulo`

---

### EXT_TituloCmpl

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: idTitulo`

**Funcao:** Contas a receber. IMP_Titulo (42 col., 892.131 linhas) é o staging; EXT_Titulo (37 col., 577.925) o canônico; EXT_TituloMov (9 col., 1.134.039) os movimentos; EXT_TituloCmpl o texto livre. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `idTitulo` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `EXT_Titulo.idTitulo` |
| 2 | `Complemento` | `text(2147483647)` | sim | - |  |

---

### EXT_TituloMov

`classe: nucleo` · `9 colunas` · `1.134.039 linhas (snapshot 03/06/2026)` · `PK: IdTituloMov`

**Funcao:** Contas a receber. IMP_Titulo (42 col., 892.131 linhas) é o staging; EXT_Titulo (37 col., 577.925) o canônico; EXT_TituloMov (9 col., 1.134.039) os movimentos; EXT_TituloCmpl o texto livre. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdTituloMov` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `idTitulo` | `numeric(18,0)` | sim | - | FK -> `EXT_Titulo.idTitulo` |
| 3 | `DtaMov` | `datetime` | sim | - |  |
| 4 | `VlrMov` | `numeric(14,2)` | sim | - |  |
| 5 | `Movimento` | `varchar(250)` | sim | - |  |
| 6 | `VlrMovCalc` | `numeric(14,2)` | sim | - |  |
| 7 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 8 | `DtaUltAlteracao` | `datetime` | sim | - |  |
| 9 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### EXT_Titulo_bkp_11_04

`classe: lixo/backup` · `37 colunas` · `108.061 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (37 colunas) para manter o documento legivel._

---

### EXT_Titulo_BKP_22_03_2023

`classe: lixo/backup` · `37 colunas` · `4.635 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (37 colunas) para manter o documento legivel._

---

### EXT_Titulo_BKP_22_03_2023_BAIXADOS

`classe: lixo/backup` · `37 colunas` · `16.705 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (37 colunas) para manter o documento legivel._

---

### EXT_TIT_ACRESC

`classe: isolada` · `2 colunas` · `1.575 linhas (snapshot 03/06/2026)` · `PK: LINKSTR`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `EXT` (espelho de dados do ERP).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `LINKSTR` | `int(10,0)` | **nao** | - | **PK** |
| 2 | `VALOR` | `numeric(13,2)` | sim | - |  |

---

### EXT_Veic

`classe: nucleo` · `28 colunas` · `8.020 linhas (snapshot 03/06/2026)` · `PK: IdVeic`

**Funcao:** Equipamento/máquina e frota do cliente. EXT_Veic 8.020 linhas; EXT_VeicModelo 4.431.168 (!); EXT_VeicPlanoMan 3.571.257; EXT_VeicModPlano 3.571.235; EXT_VeicProp 12.621 (histórico de propriedade); EXT_VeicKM 17.378 (leituras de horímetro/km). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVeic` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IdVeicMarca` | `numeric(4,0)` | **nao** | - | FK -> `EXT_VeicMarca.IdVeicMarca` |
| 3 | `IdVeicModelo` | `numeric(8,0)` | **nao** | - | FK -> `EXT_VeicModelo.IdVeicModelo` |
| 4 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 5 | `SeqPlanoMAN` | `numeric(18,0)` | sim | - | FK -> `EXT_VeicPlanoMan.SeqPlanoMAN` |
| 6 | `Descricao` | `varchar(40)` | **nao** | - | descricao do registro |
| 7 | `CorExterna` | `varchar(30)` | sim | - |  |
| 8 | `CorInterna` | `varchar(30)` | sim | - |  |
| 9 | `Chassi` | `varchar(30)` | sim | - |  |
| 10 | `ChassiRed` | `varchar(20)` | sim | - |  |
| 11 | `Placa` | `varchar(10)` | sim | - |  |
| 12 | `NroMOTOR` | `varchar(30)` | sim | - |  |
| 13 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 14 | `NroEmpresaServ` | `numeric(6,0)` | sim | - |  |
| 15 | `KMAtual` | `numeric(8,0)` | sim | - |  |
| 16 | `DtaKMAtual` | `datetime` | sim | - |  |
| 17 | `KMMedia` | `numeric(6,0)` | sim | - |  |
| 18 | `DtaKMMedia` | `datetime` | sim | - |  |
| 19 | `INDRECALCKM` | `numeric(1,0)` | sim | - |  |
| 20 | `INDAnaliseAgd` | `numeric(1,0)` | sim | - |  |
| 21 | `NroUltServico` | `numeric(2,0)` | sim | - |  |
| 22 | `AnoModelo` | `numeric(4,0)` | sim | - |  |
| 23 | `AnoFabric` | `numeric(4,0)` | sim | - |  |
| 24 | `Combustivel` | `varchar(30)` | sim | - |  |
| 25 | `INDALTManual` | `numeric(1,0)` | sim | - |  |
| 26 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 27 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 28 | `DTAPRIMVENDA` | `datetime` | sim | - |  |

**Referenciada por:** `EXT_NFS.IdVeic`, `EXT_OS.IdVeic`, `EXT_VeicAgd.IdVeic`, `EXT_VeicAvalia.IdVeic`, `EXT_VeicProp.IdVeic`

---

### EXT_VeicAgd

`classe: vazia` · `28 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqVeicAgd`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com processo (`Processo`), vinculo com agenda (`SeqAgenda`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqVeicAgd` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IdVeic` | `numeric(18,0)` | sim | - | FK -> `EXT_Veic.IdVeic`; equipamento (`EXT_Veic.IdVeic`) |
| 3 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `Placa` | `char(7)` | sim | - |  |
| 5 | `NroServico` | `numeric(2,0)` | sim | - |  |
| 6 | `KMAtual` | `numeric(8,0)` | **nao** | - |  |
| 7 | `DtaConvite` | `datetime` | sim | - |  |
| 8 | `SeqAgdConvite` | `numeric(18,0)` | sim | - |  |
| 9 | `DtaLembrete1` | `datetime` | sim | - |  |
| 10 | `SeqAgdLembrete1` | `numeric(18,0)` | sim | - |  |
| 11 | `DtaLembrete2` | `datetime` | sim | - |  |
| 12 | `SeqAgdLembrete2` | `numeric(18,0)` | sim | - |  |
| 13 | `DtaConfirmacao` | `datetime` | sim | - |  |
| 14 | `SeqAgdConfirmacao` | `numeric(18,0)` | sim | - |  |
| 15 | `NroTentativa` | `numeric(2,0)` | sim | - |  |
| 16 | `DtaAgenda` | `datetime` | sim | - |  |
| 17 | `SeqAgenda` | `numeric(18,0)` | sim | - | agenda (`IV_Agenda.SeqAgenda`) |
| 18 | `IndDtaAgendaConf` | `numeric(1,0)` | sim | - |  |
| 19 | `DtaRealizado` | `datetime` | sim | - |  |
| 20 | `StatusFinal` | `varchar(16)` | sim | - |  |
| 21 | `IndAgdAvulso` | `numeric(1,0)` | sim | - |  |
| 22 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 23 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 24 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 25 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 26 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 27 | `Gerador` | `char(1)` | sim | - |  |
| 28 | `OBS` | `varchar(50)` | sim | - | texto livre |

**Referenciada por:** `EXT_VEICAGDERP.SEQVEICAGD`

---

### EXT_VEICAGDERP

`classe: vazia` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQVEICAGDERP`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `EXT` (espelho de dados do ERP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQVEICAGDERP` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQVEICAGD` | `numeric(18,0)` | **nao** | - | FK -> `EXT_VeicAgd.SeqVeicAgd` |
| 3 | `TIPOAGD` | `varchar(12)` | **nao** | - |  |
| 4 | `INDEMABERTO` | `numeric(1,0)` | sim | - |  |
| 5 | `ERP` | `varchar(12)` | **nao** | - |  |
| 6 | `DTADE` | `datetime` | **nao** | - |  |
| 7 | `DTAATE` | `datetime` | sim | - |  |
| 8 | `ATENDENTE` | `varchar(20)` | sim | - |  |
| 9 | `CHAVETAB` | `varchar(50)` | sim | - |  |
| 10 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 11 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 12 | `OBS` | `varchar(50)` | sim | - | texto livre |

---

### EXT_VeicAvalFoto

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdAvalFoto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdAvalFoto` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IdAvalia` | `numeric(18,0)` | **nao** | - | FK -> `EXT_VeicAvalia.IdAvalia` |
| 3 | `URL` | `varchar(200)` | sim | - |  |

---

### EXT_VeicAvalia

`classe: vazia` · `31 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdAvalia`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`), vinculo com equipamento (`IdVeic`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdAvalia` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CNPJEmpresa` | `numeric(18,0)` | sim | - |  |
| 3 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 4 | `Placa` | `varchar(10)` | sim | - |  |
| 5 | `Chassi` | `varchar(30)` | sim | - |  |
| 6 | `Renavam` | `varchar(16)` | sim | - |  |
| 7 | `Marca` | `varchar(50)` | sim | - |  |
| 8 | `Modelo` | `varchar(50)` | sim | - |  |
| 9 | `Versao` | `varchar(50)` | sim | - |  |
| 10 | `NroPortas` | `numeric(2,0)` | sim | - |  |
| 11 | `CorExterna` | `varchar(30)` | sim | - |  |
| 12 | `KMAtual` | `numeric(8,0)` | sim | - |  |
| 13 | `AnoModelo` | `numeric(4,0)` | sim | - |  |
| 14 | `AnoFabric` | `numeric(4,0)` | sim | - |  |
| 15 | `Combustivel` | `varchar(30)` | sim | - |  |
| 16 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 17 | `DtaAvaliacao` | `datetime` | sim | - |  |
| 18 | `VlrAvaliado` | `numeric(14,2)` | sim | - |  |
| 19 | `VlrVenda` | `numeric(14,2)` | sim | - |  |
| 20 | `VlrCliente` | `numeric(14,2)` | sim | - |  |
| 21 | `VlrReparos` | `numeric(14,2)` | sim | - |  |
| 22 | `Classificacao` | `varchar(10)` | sim | - |  |
| 23 | `Finalidade` | `varchar(20)` | sim | - |  |
| 24 | `NomeCliente` | `varchar(50)` | sim | - |  |
| 25 | `EmailCliente` | `varchar(50)` | sim | - |  |
| 26 | `FoneCliente` | `varchar(50)` | sim | - |  |
| 27 | `CNPJCPFCliente` | `numeric(18,0)` | sim | - |  |
| 28 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 29 | `IdVeic` | `numeric(18,0)` | sim | - | FK -> `EXT_Veic.IdVeic`; equipamento (`EXT_Veic.IdVeic`) |
| 30 | `LinkAvaliacao` | `varchar(30)` | sim | - |  |
| 31 | `DtaAtualizacao` | `datetime` | sim | - |  |

**Referenciada por:** `EXT_VeicAvalFoto.IdAvalia`

---

### EXT_VeicFam

`classe: catalogo` · `6 colunas` · `68 linhas (snapshot 03/06/2026)` · `PK: IdVeicFamilia`

**Funcao:** - Não apurei o conteúdo de EXT_VEICREF (SEQPROPRIEDADE, CAMPOORIGEM, CAMPODEST — 40 linhas) e EXT_VEICFAMREF (23 linhas), que parecem ser a tabela de-para entre campos do veículo do ERP e propriedades do CRM (IV_Propriedade). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVeicFamilia` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `IdVeicMarca` | `numeric(4,0)` | **nao** | - | FK -> `EXT_VeicMarca.IdVeicMarca` |
| 3 | `Descricao` | `varchar(60)` | sim | - | descricao do registro |
| 4 | `INDHoraKM` | `char(1)` | **nao** | - |  |
| 5 | `TPVeicSeqPar` | `numeric(18,0)` | sim | - |  |
| 6 | `SeqPropriedade` | `numeric(4,0)` | sim | - | FK -> `IV_Propriedade.SeqPropriedade` |

**Referenciada por:** `EXT_VeicModelo.IdVeicFamilia`

---

### EXT_VEICFAMREF

`classe: isolada` · `2 colunas` · `23 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** - Não apurei o conteúdo de EXT_VEICREF (SEQPROPRIEDADE, CAMPOORIGEM, CAMPODEST — 40 linhas) e EXT_VEICFAMREF (23 linhas), que parecem ser a tabela de-para entre campos do veículo do ERP e propriedades do CRM (IV_Propriedade). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `FAMILIA` | `varchar(30)` | sim | - |  |
| 2 | `SEQPROPRIEDADE` | `int(10,0)` | sim | - |  |

---

### EXT_VeicKM

`classe: nucleo` · `11 colunas` · `17.378 linhas (snapshot 03/06/2026)` · `PK: IdVeic, KMAtual`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com equipamento (`IdVeic`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVeic` | `numeric(18,0)` | **nao** | - | **PK**; equipamento (`EXT_Veic.IdVeic`) |
| 2 | `KMAtual` | `numeric(8,0)` | **nao** | - | **PK** |
| 3 | `IdVeicProp` | `numeric(18,0)` | **nao** | - | FK -> `EXT_VeicProp.IdVeicProp` |
| 4 | `IDOS` | `numeric(18,0)` | sim | - | FK -> `EXT_OS.IDOS` |
| 5 | `SeqPlanoMnFx` | `numeric(18,0)` | sim | - | FK -> `EXT_VeicPlanoMnFX.SeqPlanoMnFx` |
| 6 | `DtaKMAtual` | `datetime` | sim | - |  |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `NroServico` | `numeric(2,0)` | sim | - |  |
| 9 | `OBS` | `varchar(50)` | sim | - | texto livre |
| 10 | `KMRevisao` | `numeric(8,0)` | sim | - |  |
| 11 | `INDKMINVALIDO` | `numeric(1,0)` | sim | - |  |

---

### EXT_VeicMarca

`classe: catalogo` · `5 colunas` · `5 linhas (snapshot 03/06/2026)` · `PK: IdVeicMarca`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVeicMarca` | `numeric(4,0)` | **nao** | - | **PK** |
| 2 | `Marca` | `varchar(20)` | **nao** | - |  |
| 3 | `NroEmpresaPadrao` | `numeric(6,0)` | sim | - |  |
| 4 | `SeqTipoManPadrao` | `decimal(6,0)` | sim | - | FK -> `EXT_VeicTipoMan.SeqTipoMan` |
| 5 | `SeqPlanoManMarca` | `numeric(18,0)` | sim | - | FK -> `EXT_VeicPlanoMan.SeqPlanoMAN` |

**Referenciada por:** `EXT_Veic.IdVeicMarca`, `EXT_VeicFam.IdVeicMarca`, `EXT_VeicMarcaEmp.IdVeicMarca`

---

### EXT_VeicMarcaEmp

`classe: nucleo` · `4 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SeqVeicMarcaEmp`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqVeicMarcaEmp` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IdVeicMarca` | `numeric(4,0)` | sim | - | FK -> `EXT_VeicMarca.IdVeicMarca` |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `NroEmpresaOrig` | `numeric(6,0)` | **nao** | - |  |

---

### EXT_VeicModelo

`classe: nucleo` · `5 colunas` · `4.431.168 linhas (snapshot 03/06/2026)` · `PK: IdVeicModelo`

**Funcao:** Frota do cliente e plano de manutenção — a maior base operacional de pós-venda (3.571.257 linhas de plano de manutenção), alimentando o agendamento preditivo de revisão. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVeicModelo` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `IdVeicFamilia` | `numeric(6,0)` | **nao** | - | FK -> `EXT_VeicFam.IdVeicFamilia` |
| 3 | `Modelo` | `varchar(50)` | sim | - |  |
| 4 | `Descricao` | `varchar(40)` | **nao** | - | descricao do registro |
| 5 | `Potencia` | `numeric(4,0)` | sim | - |  |

**Referenciada por:** `EXT_Veic.IdVeicModelo`, `EXT_VeicModPlano.IdVeicModelo`

---

### EXT_VeicModPlano

`classe: nucleo` · `2 colunas` · `3.571.235 linhas (snapshot 03/06/2026)` · `PK: IdVeicModelo, SeqPlanoMAN`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVeicModelo` | `numeric(8,0)` | **nao** | - | **PK**; FK -> `EXT_VeicModelo.IdVeicModelo` |
| 2 | `SeqPlanoMAN` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `EXT_VeicPlanoMan.SeqPlanoMAN` |

---

### EXT_VeicPlanoMan

`classe: nucleo` · `4 colunas` · `3.571.257 linhas (snapshot 03/06/2026)` · `PK: SeqPlanoMAN`

**Funcao:** Frota do cliente e plano de manutenção — a maior base operacional de pós-venda (3.571.257 linhas de plano de manutenção), alimentando o agendamento preditivo de revisão. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlanoMAN` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TPVeicSeqPar` | `numeric(18,0)` | **nao** | - |  |
| 3 | `SeqTipoMan` | `decimal(6,0)` | **nao** | - | FK -> `EXT_VeicTipoMan.SeqTipoMan` |
| 4 | `Descricao` | `varchar(30)` | **nao** | - | descricao do registro |

**Referenciada por:** `EXT_Veic.SeqPlanoMAN`, `EXT_VeicMarca.SeqPlanoManMarca`, `EXT_VeicModPlano.SeqPlanoMAN`, `EXT_VeicPlanoMnFX.SeqPlanoMAN`

---

### EXT_VeicPlanoMnFX

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPlanoMnFx`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlanoMnFx` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPlanoMAN` | `numeric(18,0)` | **nao** | - | FK -> `EXT_VeicPlanoMan.SeqPlanoMAN` |
| 3 | `NroServico` | `numeric(2,0)` | **nao** | - |  |
| 4 | `Descricao` | `varchar(20)` | **nao** | - | descricao do registro |
| 5 | `Faixa` | `numeric(18,0)` | **nao** | - |  |
| 6 | `QTDDiasUltMAN` | `numeric(4,0)` | sim | - |  |
| 7 | `Gerador` | `char(1)` | sim | - |  |

**Referenciada por:** `EXT_VeicKM.SeqPlanoMnFx`

---

### EXT_VeicProp

`classe: nucleo` · `24 colunas` · `12.621 linhas (snapshot 03/06/2026)` · `PK: IdVeicProp`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`), vinculo com equipamento (`IdVeic`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVeicProp` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IdVeic` | `numeric(18,0)` | **nao** | - | FK -> `EXT_Veic.IdVeic`; equipamento (`EXT_Veic.IdVeic`) |
| 3 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `IdVendedor` | `numeric(18,0)` | sim | - | FK -> `EXT_Vendedor.IdVendedor` |
| 5 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 6 | `Dtavenda` | `datetime` | sim | - |  |
| 7 | `DtaEntrega` | `datetime` | sim | - |  |
| 8 | `EstadoVenda` | `char(1)` | sim | - |  |
| 9 | `VlrVenda` | `numeric(14,2)` | sim | - |  |
| 10 | `TPVendaSeqPar` | `numeric(18,0)` | sim | - |  |
| 11 | `NroNF` | `numeric(18,0)` | sim | - |  |
| 12 | `SerieNF` | `varchar(12)` | sim | - |  |
| 13 | `Revenda` | `varchar(40)` | sim | - |  |
| 14 | `Financiador` | `varchar(40)` | sim | - |  |
| 15 | `Dtaprevquitacao` | `datetime` | sim | - |  |
| 16 | `KMCompra` | `numeric(8,0)` | sim | - |  |
| 17 | `CanalVenda` | `varchar(20)` | sim | - |  |
| 18 | `IndPropAtual` | `numeric(1,0)` | sim | - |  |
| 19 | `IndVendaAVISADA` | `numeric(1,0)` | sim | - |  |
| 20 | `IndRecusaAgd` | `numeric(1,0)` | sim | - |  |
| 21 | `Observacao` | `varchar(250)` | sim | - | texto livre |
| 22 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 23 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 24 | `IndConcessionaria` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `EXT_VeicKM.IdVeicProp`

---

### EXT_VEICREF

`classe: isolada` · `3 colunas` · `40 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** - Não apurei o conteúdo de EXT_VEICREF (SEQPROPRIEDADE, CAMPOORIGEM, CAMPODEST — 40 linhas) e EXT_VEICFAMREF (23 linhas), que parecem ser a tabela de-para entre campos do veículo do ERP e propriedades do CRM (IV_Propriedade). (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPROPRIEDADE` | `int(10,0)` | sim | - |  |
| 2 | `CAMPOORIGEM` | `varchar(250)` | sim | - |  |
| 3 | `CAMPODEST` | `varchar(250)` | sim | - |  |

---

### EXT_VeicTipoMan

`classe: catalogo` · `8 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SeqTipoMan`

**Funcao:** _(inferido)_ Pelo nome, e um catalogo de tipos relacionada a veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTipoMan` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `TipoManutencao` | `varchar(20)` | sim | - |  |
| 3 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 4 | `IndPadrao` | `numeric(1,0)` | sim | - |  |
| 5 | `DiaToleraServ` | `decimal(4,0)` | sim | - |  |
| 6 | `KmHrToleraServ` | `decimal(4,0)` | sim | - |  |
| 7 | `KmHrMinimoXDia` | `decimal(4,0)` | sim | - |  |
| 8 | `KmHrMaximoXDia` | `decimal(4,0)` | sim | - |  |

**Referenciada por:** `EXT_VeicMarca.SeqTipoManPadrao`, `EXT_VeicPlanoMan.SeqTipoMan`

---

### EXT_Vendedor

`classe: catalogo` · `6 colunas` · `563 linhas (snapshot 03/06/2026)` · `PK: IdVendedor`

**Funcao:** Catálogos vindos do ERP. EXT_Produto 27 col./78.749 linhas (Protheus 35.027, SISDIA 27.859, 15.860 sem origem); EXT_Vendedor 6 col./563 (SISDIA 347, sem origem 188, Protheus 28); IMP_PRODUTO 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdVendedor` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Nome` | `varchar(60)` | **nao** | - |  |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `CodVendedor` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 5 | `ORIGEM` | `varchar(20)` | sim | - | sistema de origem do dado |
| 6 | `NROCPF` | `numeric(11,0)` | sim | - |  |

**Referenciada por:** `EXT_NFS.IdVendedor`, `EXT_NFSItem.IdVendedor`, `EXT_VeicProp.IdVendedor`

---
