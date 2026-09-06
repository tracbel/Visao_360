# Protheus — como se lê, e onde está cada coisa

> Documento 28 · Versão 1.0 · 06/09/2026
> Escopo: o caminho de leitura do Protheus de produção, o mapa das tabelas que a operação usa e
> o que cada uma responde. **Todo acesso foi somente leitura** — nenhum `POST`, `PUT` ou
> `DELETE` foi emitido contra o ERP.
> Convenção: `[medido]` = valor obtido ao vivo contra a produção em 06/09/2026.

---

## Resumo em cinco linhas

A API REST de produção do Protheus é alcançável e as credenciais funcionam. Ela publica quase
nada — mas tem um endpoint não catalogado, `genericQuery`, que lê **qualquer tabela do
dicionário**. Com ele foram mapeadas as famílias `VV`, `SB`, `VQ`, `VJ` e `VO`, e medido o que
importa. **O achado que muda o projeto: o faturamento no Protheus não parou** — o que parou foi
a integração com o Vórtice.

---

## 1. O caminho de leitura

| Item | Valor `[medido]` |
|---|---|
| Base de produção | `http://10.100.10.98:5891/rest` — **HTTP puro**, não HTTPS |
| Ambiente | `envId: TMPRD` |
| Usuário | `Integracao.IA` (do `.env`, chave `TOTVS_API_USER_PROD`) |
| Token | `POST /api/oauth2/v1/token?grant_type=password&username=…&password=…`, validade **3600s** |
| Homologação (`10.100.10.252:5891`) | **Sem rota** desta estação |

**A senha viaja na query string** — é assim que a API do Protheus funciona. Toda mensagem de
erro precisa passar por um filtro que troque `password=…` por `***` antes de ir para tela ou
log, senão a credencial vaza no primeiro `catch`. O `scripts/protheus/_comum.ps1` faz isso.

### 1.1 O que a API publica — e o que não publica

| Rota | Resultado |
|---|---|
| `/api/framework/v1/users` | **200** — SCIM, usada pelo projeto de onboarding |
| `/api/framework/v1/companies`, `.../branches` | 404 |
| `/UONBMOD`, `/UONBSA3`, `/UONBVAI` | **404 em produção** — existem só em homologação |
| `/api/fat/*`, `/api/est/*`, `/api/com/*`, `/api/oficina/*` | 404 |
| **`/api/framework/v1/genericQuery`** | **funciona** |

**Como o `genericQuery` foi encontrado.** Ele não aparece em catálogo nenhum: o `GET /rest/`
devolve uma tela de login em HTML, não uma lista de serviços. Ele apareceu porque respondeu
**400** — parâmetro faltando — onde todo o resto respondia **404**. A diferença entre os dois
códigos foi a pista.

### 1.2 Como se usa

```
GET /api/framework/v1/genericQuery?tables=VV1&fields=VV1_FILIAL,VV1_CHASSI&pageSize=1000&page=1
→ { "items": [...], "hasNext": true, "remainingRecords": 37358, "total": 38358 }
```

Três armadilhas, todas medidas:

1. **`fields` é obrigatório e não aceita `*`.** Sem ele, 400; com asterisco, 500.
2. **Campo inexistente não dá erro — ele some da resposta em silêncio.** Um agrupamento por
   coluna ausente vira "(vazio)" para tudo, e o relatório sai errado sem ninguém perceber.
3. **A `SX3` lista campos que a tabela física não tem.** `VV1_DESMAR` e `VV1_ANOMOD` estão no
   dicionário e não voltam na consulta. Sempre conferir com uma amostra antes de confiar.

O dicionário é lido do próprio Protheus: **`SX2`** dá a lista de tabelas com descrição, **`SX3`**
dá os campos de cada uma. É o que evita chutar nome de campo numa tabela de 203 colunas.

---

## 2. O mapa das tabelas

### 2.1 `VV` — veículos e concessionária (36 tabelas)

| Tabela | O que é | Linhas `[medido]` |
|---|---|---:|
| **`VV1`** | **Cadastro de Veículos** — uma linha por chassi | **38.358** |
| `VV2` | Modelos de Veículos | 861 |
| `VVR` | Grupo de Modelos | 265 |
| `VVX` | Segmento do Modelo | 3 |
| `VVB` | Categorias de Veículos | 28 |
| `VV8` | Tipo de Veículos | 25 |
| `VV0` / `VVA` | Saídas de Veículos e seus itens — **é aqui que mora a venda** | 6.900 / 8.534 |
| `VVF` / `VVG` | Entradas de Veículos e seus itens | — |

Outras do grupo: `VVC` cores, `VVP` preços, `VVL` garantia do modelo, `VVM` acessórios,
`VVW` opcionais, `VVZ` equipamentos do veículo.

### 2.2 `SB` — produto e estoque (34 tabelas)

**Correção importante:** `SB1`, `SB2` e `SB3` **não são faturamento**.

| Tabela | O que é de verdade | Linhas `[medido]` |
|---|---|---:|
| `SB1` | **Descrição Genérica do Produto** — o cadastro, máquina e peça juntos | 517.855 |
| `SB2` | **Saldos Físico e Financeiro** — estoque, não venda | 88.990 |
| `SB3` | Demandas | — |
| **`SBM`** | **Grupo de Produto — é o que separa máquina de peça** | 273 |

O faturamento está em **`SD2`** (itens de nota fiscal de saída, 324.796 linhas) e **`SF2`**
(cabeçalho da nota, 109.137).

### 2.3 `VQ` e `VJ` — pedidos (27 + 31 tabelas)

| Tabela | O que é | Linhas `[medido]` |
|---|---|---:|
| **`VQ0`** | **Pedido de Veículos** — 53 campos | **10.866** |
| **`VJR`** | **Complemento Pedido John Deere** — 20 campos | **11.477** |
| `VQ5` / `VQ6` | Agendamento JD / Log de integração JD | — |
| `VJM` / `VJJ` / `VJK` | Pedido de venda JD, cotação e itens da cotação | — |
| `VJU` | Relação modelo JD × Protheus | — |

`VQ0` tem chassi, modelo, data do pedido, data da venda, status e filial. `VJR` complementa com
status de fábrica, status de importação e datas FDD/ORSD.

### 2.4 `VO` — serviços e oficina (36 tabelas)

| Tabela | O que é | Linhas `[medido]` |
|---|---|---:|
| **`VO1`** | **Ordem de Serviço** — 102 campos, o centro do módulo | **17.988** |
| `VO0` | Controle de KM/Horímetro | — |
| `VO2` / `VO3` / `VO4` | Requisição: cabeçalho, peças, serviços | — |
| `VO5` | Veículos da Oficina | — |
| `VO6` / `VO7` | Tabela de Serviços e tempos | — |
| `VOM` | Relação Pós-Venda | — |
| `VOT` | Abastecimentos | — |

As demais são cadastro de apoio: `VOG` motoristas, `VOD` seções, `VON` box, `VOK` tipo de
serviço, `VOS` grupos de serviço, `VOP`/`VOU` campanha técnica.

---

## 3. Máquina ou peça — a resposta

A pergunta do negócio era "qual das SB é de máquina, porque às vezes mistura com peças". A
resposta é que **nenhuma delas separa** — quem separa é o **grupo do produto**, catálogo `SBM`:

| Grupo | Descrição |
|---|---|
| `VEIC` | **MÁQUINAS** |
| `1001` … `10xx` | **PEÇAS**, uma faixa por linha: colhedoras de cana, tratores, colheitadeira de grãos, plantadeiras, plataformas, gator, pulverizador, implementos JD… |
| `SRV` | SERVIÇOS |
| `MO_O` / `MO_T` | Mão de obra de oficina / de terceiro |
| `AMS` | A.M.S |
| `FR` | Ferramentas |
| `JD` | John Deere |

**E o grupo viaja na própria linha da nota:** `SD2.D2_GRUPO`. Não é preciso juntar com a `SB1`
para separar faturamento de máquina do de peça — o que torna a consulta muito mais barata.

---

## 4. O achado que muda o projeto

O CRM operou meses com a premissa **"o faturamento parou em 11/04/2025"**. Ela veio de
`X_TOTVS_CRM_FATURAMENTO`, a tabela que o **Vórtice recebe** do Protheus — e essa de fato para
naquela data.

Na **origem**, medido em 06/09/2026:

| Recorte | Itens de nota |
|---|---:|
| Emissão ≥ 11/04/2025 | 113.822 |
| Emissão ≥ 01/01/2026 | 54.158 |
| Emissão ≥ 01/09/2026 | **1.365** |

**O que morreu foi a integração Protheus → Vórtice, não o faturamento.** Há nota emitida esta
semana.

Isso derruba, de uma vez, quase todo cartão vazio do painel: faturamento do mês, série de doze
meses, top clientes, mix por linha e a curva ABC — que hoje é apurada sobre dado que termina em
abril de 2025 e passa a poder ser apurada sobre dado de hoje.

---

## 5. O que ainda não está resolvido

1. **`VV1_FILIAL` é `0101` em 100% das 38.358 linhas** `[medido]`. Ou a tabela é compartilhada
   entre filiais nesse nível, ou a segmentação por filial da frota sai de outro lugar — da venda
   (`VV0`/`VVA`), provavelmente. Confirmar antes de afirmar "frota da filial X".
2. **`VV1_SITVEI` e `VV1_STATUS` vêm vazios** na amostra. Sem eles não dá para separar estoque
   de máquina em poder do cliente, e "frota em campo" não pode ser afirmada.
3. **`VV1_TIPVEI` é `1` em 99% das linhas** e a `VV8` não traz descrição para esse código. O tipo
   não segmenta nada hoje.
4. **A `VVX` não preenche o nome da marca.** `JD` não vira "John Deere" por ela. O de-para de
   marca precisa de outra fonte, ou de uma lista mantida pelo negócio.
5. **Autorização para leitura em volume.** A conta `Integracao.IA` funciona, mas ler a `SD2`
   inteira são ~325 requisições contra um ERP de produção. Falta combinar com a TI qual janela e
   com que frequência.

---

## 6. Ferramentas

| Arquivo | O que faz |
|---|---|
| `scripts/protheus/_comum.ps1` | Autenticação com renovação automática do token, paginação, retentativa em 428/503 (só em GET) e ocultação da senha nas mensagens de erro |
| `scripts/protheus/01-frota-vv1.ps1` | Lê a `VV1` inteira e produz `docs/extracao-protheus/frota-vv1.md` |
| `scripts/protheus/02-faturamento-sd2.ps1` | Lê a `SD2` de um período e produz `docs/extracao-protheus/faturamento-sd2.md`, com a quebra máquina × peça |

Os `.ps1` são gravados em **UTF-8 com BOM**: o PowerShell 5.1 lê `.ps1` como ANSI e sem o BOM
qualquer acento quebra o parser.
