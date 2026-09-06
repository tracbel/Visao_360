# X_* - Integracao TOTVS (staging e materializacoes)

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**8 tabelas · 185 colunas · 3.044.200 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`X_TOTVS_BI_FATURAMENTO_MAQUINAS`](#x_totvs_bi_faturamento_maquinas) | staging | 13 | 4.612 |
| [`X_TOTVS_BI_FATURAMENTO_POS_VENDAS`](#x_totvs_bi_faturamento_pos_vendas) | staging | 30 | 407.100 |
| [`X_TOTVS_CRM_FATURAMENTO`](#x_totvs_crm_faturamento) | staging | 41 | 809.821 |
| [`X_T_IMP_CRM_TITULO`](#x_t_imp_crm_titulo) | staging | 41 | 462.391 |
| [`X_T_IMP_CRM_TITULO_bkp_11_04`](#x_t_imp_crm_titulo_bkp_11_04) | lixo/backup | 41 | 121.381 |
| [`X_T_IMP_CRM_VEICULO`](#x_t_imp_crm_veiculo) | staging | 7 | 7.279 |
| [`X_V_IMP_CRM_IMP_NF`](#x_v_imp_crm_imp_nf) | staging | 6 | 815.854 |
| [`X_V_IMP_CRM_IMP_NF_BKP_18_09_2023`](#x_v_imp_crm_imp_nf_bkp_18_09_2023) | lixo/backup | 6 | 415.762 |

---

### X_TOTVS_BI_FATURAMENTO_MAQUINAS

`classe: staging` · `13 colunas` · `4.612 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `X` (integracao TOTVS).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `origem` | `varchar(36)` | sim | - | sistema de origem do dado |
| 2 | `origem_fonte` | `varchar(24)` | **nao** | - |  |
| 3 | `tipo_nf` | `varchar(12)` | sim | - |  |
| 4 | `filial` | `int(10,0)` | sim | - |  |
| 5 | `nro_nf` | `varchar(9)` | **nao** | - |  |
| 6 | `seq_pessoa` | `varchar(800)` | sim | - |  |
| 7 | `cliente` | `varchar(40)` | sim | - |  |
| 8 | `nome_operacao` | `varchar(20)` | sim | - |  |
| 9 | `data_emissao_nf` | `date` | sim | - |  |
| 10 | `cliente_fj` | `varchar(1)` | sim | - |  |
| 11 | `cod_produto` | `varchar(8000)` | sim | - |  |
| 12 | `nome_produto` | `varchar(60)` | sim | - |  |
| 13 | `vlr_liquido_item` | `float(53,0)` | **nao** | - |  |

---

### X_TOTVS_BI_FATURAMENTO_POS_VENDAS

`classe: staging` · `30 colunas` · `407.100 linhas (snapshot 03/06/2026)` · `PK: ID`

**Funcao:** Camada analítica MATERIALIZADA do ERP TOTVS (407.100 / 809.821 / 462.391 linhas). É o contraexemplo BOM: carga por procedure idempotente com janela. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ID` | `int(10,0)` | **nao** | - | **PK** |
| 2 | `ORIGEM` | `varchar(12)` | **nao** | - | sistema de origem do dado |
| 3 | `FILIAL` | `int(10,0)` | sim | - |  |
| 4 | `DATA_EMISSAO_NF` | `date` | sim | - |  |
| 5 | `NRO_NF` | `varchar(9)` | **nao** | - |  |
| 6 | `SERIE` | `varchar(3)` | **nao** | - |  |
| 7 | `COD_CLIENTE` | `varchar(14)` | sim | - |  |
| 8 | `CPF_CNPJ` | `varchar(14)` | sim | - |  |
| 9 | `COD_MARCA` | `varchar(6)` | sim | - |  |
| 10 | `DES_MARCA` | `varchar(30)` | sim | - |  |
| 11 | `ID_VENDEDOR` | `varchar(6)` | **nao** | - |  |
| 12 | `COD_OPERACAO` | `varchar(3)` | **nao** | - |  |
| 13 | `DES_OPERACAO` | `varchar(20)` | sim | - |  |
| 14 | `CFOP` | `varchar(5)` | sim | - |  |
| 15 | `PRODUTO` | `varchar(28)` | sim | - |  |
| 16 | `COD_FAMILIA` | `varchar(10)` | sim | - |  |
| 17 | `DESC_FAMILIA` | `varchar(80)` | sim | - |  |
| 18 | `QUANTIDADE` | `float(53,0)` | **nao** | - |  |
| 19 | `VLR_UNITARIO` | `float(53,0)` | **nao** | - |  |
| 20 | `VALOR_TOTAL` | `float(53,0)` | **nao** | - |  |
| 21 | `VALOR_DESCONTO` | `float(53,0)` | **nao** | - |  |
| 22 | `VLR_LIQUIDO_ITEM` | `float(53,0)` | **nao** | - |  |
| 23 | `VLR_ICMS` | `float(53,0)` | **nao** | - |  |
| 24 | `VLR_ICMS_SUBST` | `float(53,0)` | **nao** | - |  |
| 25 | `VLR_PIS` | `float(53,0)` | **nao** | - |  |
| 26 | `VALOR_COFINS` | `float(53,0)` | **nao** | - |  |
| 27 | `VALOR_CUSTO_MEDIO` | `float(53,0)` | **nao** | - |  |
| 28 | `VALOR_FRETE` | `float(53,0)` | **nao** | - |  |
| 29 | `VALOR_MARGEM` | `float(53,0)` | **nao** | - |  |
| 30 | `PERCENTUAL_MARGEM` | `float(53,0)` | **nao** | - |  |

---

### X_TOTVS_CRM_FATURAMENTO

`classe: staging` · `41 colunas` · `809.821 linhas (snapshot 03/06/2026)` · `PK: ID`

**Funcao:** Tabela de POUSO (landing) do faturamento vindo do Protheus via linked server. 41 colunas, 809.821 linhas, segmentada em 5 streams pela coluna ORIGEM. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ID` | `int(10,0)` | **nao** | - | **PK** |
| 2 | `ORIGEM` | `varchar(12)` | **nao** | - | sistema de origem do dado |
| 3 | `FILIAL` | `int(10,0)` | sim | - |  |
| 4 | `DATA_EMISSAO_NF` | `date` | sim | - |  |
| 5 | `NRO_NF` | `varchar(9)` | **nao** | - |  |
| 6 | `NRO_ITEM` | `varchar(5)` | **nao** | - |  |
| 7 | `SERIE` | `varchar(3)` | **nao** | - |  |
| 8 | `COD_CLIENTE` | `varchar(14)` | sim | - |  |
| 9 | `CPF_CNPJ` | `varchar(14)` | sim | - |  |
| 10 | `COD_MARCA` | `varchar(6)` | sim | - |  |
| 11 | `DES_MARCA` | `varchar(30)` | sim | - |  |
| 12 | `ID_VENDEDOR` | `varchar(6)` | **nao** | - |  |
| 13 | `COD_OPERACAO` | `varchar(3)` | **nao** | - |  |
| 14 | `DES_OPERACAO` | `varchar(20)` | sim | - |  |
| 15 | `CFOP` | `varchar(5)` | sim | - |  |
| 16 | `PRODUTO` | `varchar(28)` | sim | - |  |
| 17 | `DESC_PRODUTO` | `varchar(50)` | sim | - |  |
| 18 | `COD_FAMILIA` | `varchar(10)` | sim | - |  |
| 19 | `DESC_FAMILIA` | `varchar(80)` | sim | - |  |
| 20 | `NOME_VENDEDOR` | `varchar(42)` | sim | - |  |
| 21 | `DATA_PEDIDO` | `date` | sim | - |  |
| 22 | `STATUS_NF` | `varchar(1)` | sim | - |  |
| 23 | `NCM` | `varchar(12)` | sim | - |  |
| 24 | `CODICAO_PAGAMENTO` | `varchar(5)` | sim | - |  |
| 25 | `DES_CODICAO_PAGAMENTO` | `varchar(40)` | sim | - |  |
| 26 | `NRO_PEDIDO` | `varchar(8)` | sim | - |  |
| 27 | `OBSERVACAO` | `varchar(30)` | sim | - | texto livre |
| 28 | `QUANTIDADE` | `float(53,0)` | **nao** | - |  |
| 29 | `VLR_UNITARIO` | `float(53,0)` | **nao** | - |  |
| 30 | `VALOR_TOTAL` | `float(53,0)` | **nao** | - |  |
| 31 | `VALOR_DESCONTO` | `float(53,0)` | **nao** | - |  |
| 32 | `VLR_LIQUIDO_ITEM` | `float(53,0)` | **nao** | - |  |
| 33 | `VLR_ICMS` | `float(53,0)` | **nao** | - |  |
| 34 | `VLR_ICMS_SUBST` | `float(53,0)` | **nao** | - |  |
| 35 | `VLR_PIS` | `float(53,0)` | **nao** | - |  |
| 36 | `VALOR_COFINS` | `float(53,0)` | **nao** | - |  |
| 37 | `VALOR_CUSTO_MEDIO` | `float(53,0)` | **nao** | - |  |
| 38 | `VALOR_FRETE` | `float(53,0)` | **nao** | - |  |
| 39 | `VALOR_MARGEM` | `float(53,0)` | **nao** | - |  |
| 40 | `PERCENTUAL_MARGEM` | `float(53,0)` | **nao** | - |  |
| 41 | `DELETADO` | `varchar(1)` | sim | - |  |

---

### X_T_IMP_CRM_TITULO

`classe: staging` · `41 colunas` · `462.391 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Tabela de ESTADO (último valor conhecido) dos títulos do Protheus, usada para calcular o delta INCLUIR/ATUALIZAR. 41 col., 462.391 linhas no snapshot / 479.271 medidas, 100% INTEGRADO até 13/07/2026 — o lado Tracbel está vivo. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Origem` | `varchar(8)` | **nao** | - | sistema de origem do dado |
| 2 | `NroEmpresa` | `numeric(10,0)` | sim | - | multiempresa - filial/empresa |
| 3 | `PessoaLinkOrigem` | `varchar(8)` | **nao** | - |  |
| 4 | `Pessoalink` | `varchar(13)` | **nao** | - |  |
| 5 | `SeqPessoa` | `int(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 6 | `NroCNPJ` | `int(10,0)` | sim | - |  |
| 7 | `DigCNPJ` | `int(10,0)` | sim | - |  |
| 8 | `Departamento` | `int(10,0)` | sim | - | departamento |
| 9 | `Especie` | `varchar(3)` | **nao** | - |  |
| 10 | `NroTitulo` | `varchar(8000)` | sim | - |  |
| 11 | `NroDocto` | `int(10,0)` | sim | - |  |
| 12 | `TipoCobranca` | `varchar(5)` | **nao** | - |  |
| 13 | `IndAtivo` | `int(10,0)` | **nao** | - |  |
| 14 | `IndQuitado` | `int(10,0)` | sim | - |  |
| 15 | `IndCobrJuridica` | `int(10,0)` | sim | - |  |
| 16 | `Status` | `varchar(1)` | **nao** | - | status - validar dominio real por tabela |
| 17 | `NroBanco` | `int(10,0)` | sim | - |  |
| 18 | `LocalCobranca` | `varchar(3)` | **nao** | - |  |
| 19 | `NroTituloBanco` | `varchar(6)` | **nao** | - |  |
| 20 | `DtaEmissao` | `datetime2` | sim | - |  |
| 21 | `DtaVenctoOrig` | `datetime2` | sim | - |  |
| 22 | `DtaVencto` | `datetime2` | sim | - |  |
| 23 | `VlrOriginal` | `float(53,0)` | **nao** | - |  |
| 24 | `VlrAcrescimo` | `float(53,0)` | **nao** | - |  |
| 25 | `VlrAbatimento` | `int(10,0)` | **nao** | - |  |
| 26 | `VlrPago` | `float(53,0)` | **nao** | - |  |
| 27 | `VlrMov` | `int(10,0)` | sim | - |  |
| 28 | `Movimento` | `int(10,0)` | sim | - |  |
| 29 | `DtaUltPgto` | `datetime2` | sim | - |  |
| 30 | `DtaQuitacao` | `datetime2` | sim | - |  |
| 31 | `DtaUltAlteracao` | `datetime2` | sim | - |  |
| 32 | `UsuAlteracao` | `int(10,0)` | sim | - | auditoria de alteracao (usuario) |
| 33 | `LinkNro` | `int(10,0)` | sim | - | chave de vinculo com documento do ERP |
| 34 | `LinkStr` | `varchar(8000)` | sim | - |  |
| 35 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 36 | `IdPessoa` | `int(10,0)` | sim | - |  |
| 37 | `CHAVESTR1` | `int(10,0)` | sim | - |  |
| 38 | `DATA_INCLUSAO` | `varchar(30)` | sim | - |  |
| 39 | `DATA_ALTERACAO` | `datetime` | sim | - |  |
| 40 | `X_INTEGRADO` | `varchar(20)` | sim | - |  |
| 41 | `X_DATA_INTEGRACAO` | `datetime` | sim | - |  |

---

### X_T_IMP_CRM_TITULO_bkp_11_04

`classe: lixo/backup` · `41 colunas` · `121.381 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `X` (integracao TOTVS). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (41 colunas) para manter o documento legivel._

---

### X_T_IMP_CRM_VEICULO

`classe: staging` · `7 colunas` · `7.279 linhas (snapshot 03/06/2026)` · `PK: ID`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `X` (integracao TOTVS).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ID` | `int(10,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 3 | `VV1_CHAINT` | `varchar(6)` | **nao** | - |  |
| 4 | `DATA_INCLUSAO` | `date` | sim | - |  |
| 5 | `DATA_ALTERACAO` | `datetime2` | sim | - |  |
| 6 | `X_INTEGRADO` | `varchar(20)` | **nao** | - |  |
| 7 | `X_DATA_INTEGRACAO` | `date` | sim | - |  |

---

### X_V_IMP_CRM_IMP_NF

`classe: staging` · `6 colunas` · `815.854 linhas (snapshot 03/06/2026)` · `PK: ID`

**Funcao:** LEDGER DE IDEMPOTÊNCIA do faturamento (apesar do nome começar com X_V_, é TABELA, criada dinamicamente pela procedure). 815.854 linhas, todas X_INTEGRADO='INTEGRADO'. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ID` | `int(10,0)` | **nao** | - | **PK** |
| 2 | `COD_CHAVE_ITEM` | `varchar(150)` | **nao** | - |  |
| 3 | `COD_CHAVE_NF` | `varchar(100)` | **nao** | - |  |
| 4 | `ORIGEM` | `varchar(12)` | **nao** | - | sistema de origem do dado |
| 5 | `X_INTEGRADO` | `varchar(20)` | sim | - |  |
| 6 | `X_DATA_INTEGRACAO` | `datetime` | sim | - |  |

---

### X_V_IMP_CRM_IMP_NF_BKP_18_09_2023

`classe: lixo/backup` · `6 colunas` · `415.762 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Backups esquecidos em produção: X_V_IMP_CRM_IMP_NF_BKP_18_09_2023 (415.762) e X_T_IMP_CRM_TITULO_bkp_11_04 (121.381). (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (6 colunas) para manter o documento legivel._

---
