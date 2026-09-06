# JDE - Integracao JD Edwards

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**7 tabelas · 66 colunas · 0 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`JDE_CATEGORY`](#jde_category) | staging | 2 | 0 |
| [`JDE_EQUIPAMENTS`](#jde_equipaments) | staging | 18 | 0 |
| [`JDE_PURCHASE`](#jde_purchase) | staging | 18 | 0 |
| [`JDE_QUOTE`](#jde_quote) | staging | 19 | 0 |
| [`JDE_QUOTE_ITEM`](#jde_quote_item) | staging | 2 | 0 |
| [`JDE_SALES_PERSON`](#jde_sales_person) | staging | 5 | 0 |
| [`JDE_SUB_CATEGORY`](#jde_sub_category) | staging | 2 | 0 |

---

### JDE_CATEGORY

`classe: staging` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: categoryId`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `categoryId` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `categoryDescription` | `varchar(100)` | sim | - |  |

**Referenciada por:** `JDE_EQUIPAMENTS.categoryId`

---

### JDE_EQUIPAMENTS

`classe: staging` · `18 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: EQUIPAMENT_ID`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `EQUIPAMENT_ID` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `categoryId` | `numeric(18,0)` | sim | - | FK -> `JDE_CATEGORY.categoryId` |
| 3 | `subCategoryId` | `numeric(18,0)` | sim | - | FK -> `JDE_SUB_CATEGORY.subCategoryId` |
| 4 | `costPrice` | `decimal(10,4)` | sim | - |  |
| 5 | `listPrice` | `decimal(10,4)` | sim | - |  |
| 6 | `machineHours` | `decimal(10,4)` | sim | - |  |
| 7 | `makeName` | `varchar(100)` | sim | - |  |
| 8 | `modelName` | `varchar(250)` | sim | - |  |
| 9 | `serialNumber` | `varchar(100)` | sim | - |  |
| 10 | `equip_status` | `varchar(50)` | sim | - |  |
| 11 | `equip_year` | `numeric(18,0)` | sim | - |  |
| 12 | `storeLocation` | `varchar(200)` | sim | - |  |
| 13 | `totalAttachmentAccMa` | `decimal(10,4)` | sim | - |  |
| 14 | `totalAttachmentAccSe` | `decimal(10,4)` | sim | - |  |
| 15 | `totalDealerOptionMar` | `decimal(10,4)` | sim | - |  |
| 16 | `totalDealerOptionSel` | `decimal(10,4)` | sim | - |  |
| 17 | `totalEquipmentMargin` | `decimal(10,4)` | sim | - |  |
| 18 | `totalEquipmentSellin` | `decimal(10,4)` | sim | - |  |

**Referenciada por:** `JDE_QUOTE_ITEM.EQUIPAMENT_ID`

---

### JDE_PURCHASE

`classe: staging` · `18 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: poNumber`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `poNumber` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `orderDate` | `datetime` | sim | - |  |
| 3 | `signedOnDate` | `datetime` | sim | - |  |
| 4 | `transactionType` | `varchar(50)` | sim | - |  |
| 5 | `warrantyBeginsDate` | `datetime` | sim | - |  |
| 6 | `marketUse` | `varchar(200)` | sim | - |  |
| 7 | `ped_status` | `varchar(50)` | sim | - |  |
| 8 | `purchaserType` | `varchar(50)` | sim | - |  |
| 9 | `balanceDue` | `decimal(10,4)` | sim | - |  |
| 10 | `fieldCashWithOrder` | `decimal(10,4)` | sim | - |  |
| 11 | `fieldRentalApplied` | `decimal(10,4)` | sim | - |  |
| 12 | `balance` | `decimal(10,4)` | sim | - |  |
| 13 | `subTotal` | `decimal(10,4)` | sim | - |  |
| 14 | `totalCashPrice` | `decimal(10,4)` | sim | - |  |
| 15 | `totalTradeInAllowanc` | `decimal(10,4)` | sim | - |  |
| 16 | `totalTradePayOff` | `decimal(10,4)` | sim | - |  |
| 17 | `deliveredDate` | `datetime` | sim | - |  |
| 18 | `SeqQuestionario` | `numeric(18,0)` | sim | - | FK -> `IV_Questionario.SeqQuestionario`; questionario (`IV_Questionario.SeqQuestionario`) |

---

### JDE_QUOTE

`classe: staging` · `19 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: quoteId`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `quoteId` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `deereUserId` | `varchar(50)` | **nao** | - | FK -> `JDE_SALES_PERSON.deereUserId` |
| 3 | `balanceDue` | `decimal(10,4)` | sim | - |  |
| 4 | `creationDate` | `datetime` | sim | - |  |
| 5 | `expirationDate` | `datetime` | sim | - |  |
| 6 | `lastModifiedDate` | `datetime` | sim | - |  |
| 7 | `netCost` | `decimal(10,4)` | sim | - |  |
| 8 | `netProceeds` | `decimal(10,4)` | sim | - |  |
| 9 | `poNumber` | `numeric(18,0)` | sim | - |  |
| 10 | `quoteName` | `varchar(200)` | sim | - |  |
| 11 | `quoteStatus` | `varchar(50)` | sim | - |  |
| 12 | `quoteType` | `varchar(50)` | sim | - |  |
| 13 | `taxAmount` | `decimal(10,4)` | sim | - |  |
| 14 | `totalNetTradeValue` | `decimal(10,4)` | sim | - |  |
| 15 | `tradeDifference` | `decimal(10,4)` | sim | - |  |
| 16 | `downPayment` | `decimal(10,4)` | sim | - |  |
| 17 | `SEQPESSOA` | `numeric(18,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 18 | `agreementDate` | `datetime` | sim | - |  |
| 19 | `signDate` | `datetime` | sim | - |  |

**Referenciada por:** `JDE_QUOTE_ITEM.quoteId`

---

### JDE_QUOTE_ITEM

`classe: staging` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: quoteId, EQUIPAMENT_ID`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `quoteId` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `JDE_QUOTE.quoteId` |
| 2 | `EQUIPAMENT_ID` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `JDE_EQUIPAMENTS.EQUIPAMENT_ID` |

---

### JDE_SALES_PERSON

`classe: staging` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: deereUserId`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `deereUserId` | `varchar(50)` | **nao** | - | **PK** |
| 2 | `firstName` | `varchar(50)` | sim | - |  |
| 3 | `lastName` | `varchar(50)` | sim | - |  |
| 4 | `middleName` | `varchar(150)` | sim | - |  |
| 5 | `emailAddress` | `varchar(250)` | sim | - |  |

**Referenciada por:** `JDE_QUOTE.deereUserId`

---

### JDE_SUB_CATEGORY

`classe: staging` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: subCategoryId`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `subCategoryId` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `subCategoryDescripti` | `varchar(100)` | sim | - |  |

**Referenciada por:** `JDE_EQUIPAMENTS.subCategoryId`

---
