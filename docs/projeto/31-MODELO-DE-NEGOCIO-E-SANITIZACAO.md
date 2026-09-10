# O modelo de negócio da concessionária — e o que os dados precisam responder

> Documento 31 · Versão 1.0 · 10/09/2026
> Corrige a direção do documento 30, que media **contabilidade** e não **negócio**.
> Convenção: `[medido]` = valor obtido ao vivo contra a produção em 09 e 10/09/2026.
> Todo acesso foi **somente leitura**.

---

## O erro de direção, e por que ele importa

O documento 30 respondeu "quanto faturamos, por CFOP, por filial". Está certo e é insuficiente:
**não é assim que uma concessionária John Deere ganha dinheiro.**

Numa revenda de máquinas agrícolas:

1. **A máquina é o ponto de entrada, não o lucro.** Margem baixa, negociada contra concorrente,
   muitas vezes com financiamento da própria fábrica.
2. **O lucro está no pós-venda** — peça e serviço, com margem várias vezes maior, gerado ao longo
   de **dez anos ou mais** por cada máquina vendida.
3. **A máquina vendida vira base instalada**, e a base instalada é o ativo que produz receita
   recorrente. Perder o pós-venda de uma máquina é perder uma década de margem.
4. **O potencial do cliente não é o que ele comprou — é a área que ele planta.** Um produtor com
   1.500 hectares de cana tem uma necessidade previsível de máquinas, peças e horas de serviço,
   independentemente do que comprou até hoje.

**Medir faturamento passado responde "como foi".** O modelo de negócio pergunta **"quanto deste
cliente ainda não é nosso"** — e essa pergunta precisa de base instalada, horímetro e área
plantada. É exatamente o que a planilha `Tabelas Exemplo.xlsx` pede, e não é coincidência.

---

## 1. A primeira métrica de negócio que consegui medir

**Captura de pós-venda:** de quem comprou máquina, quantos voltam para peça e serviço.

`[medido]` clientes que compraram máquina até 31/12/2024, e o que gastaram em peça e serviço de
01/01/2025 em diante — **excluindo John Deere e empresas do grupo**:

| | |
|---|---:|
| Clientes que compraram máquina | **627** |
| Voltaram para pós-venda | **493 (79%)** |
| **Nunca voltaram** | **134 (21%)** |
| Máquina vendida | R$ 509,9 mi |
| Pós-venda gerado depois | R$ 158,7 mi |
| **Captura** | **31,1%** |

**Os 134 que nunca voltaram levaram R$ 66,2 milhões em máquina** e não deixaram um real de peça
ou serviço. Cada um deles é uma máquina em campo sendo mantida por outro — ou não sendo mantida.

**Não confie na abertura por filial desta métrica.** Atribuí o cliente à filial da primeira compra,
e o pós-venda pode ser faturado por outra praça. A captura global (31,1%) é sólida; a comparação
entre filiais, com o dado de hoje, **seria enganosa** — e uma comparação enganosa entre praças é
pior do que nenhuma, porque vira cobrança de meta.

---

## 2. Por que a base instalada é o item que falta

A captura de 31,1% é calculada sobre **faturamento de máquina**, que é uma aproximação ruim do que
deveria ser: **número de máquinas em campo, idade e horas trabalhadas**.

O certo é: *este cliente tem 12 máquinas nossas, com média de 4.800 horas; máquinas nessa faixa
consomem X de peça por ano; ele compra Y; a diferença é o que está indo para outro.*

Isso exige quatro dados que **não existem em nenhuma fonte que eu alcance hoje**:

| Dado | Onde deveria estar | Estado |
|---|---|---|
| Máquinas por cliente | ART (`bi_art_veiculos`) | ❌ sem rota |
| Horímetro e data de leitura | ART — origem Operation Center | ❌ sem rota |
| Ano e entrega técnica | ART | ❌ sem rota |
| Área plantada e cultura | ART | ❌ sem rota |

**O Protheus não substitui:** `[medido]` dos 38.369 chassis na `VV1010`, **apenas 3.550 (9%) têm
dono**, e `VV1_FABANO` (ano) e `VV1_KMS` (horímetro) estão **vazios em 100%**.

**Estado do acesso em 10/09/2026:** `aftracbel.tracbel.com.br` → `10.235.0.58`, porta 3306.
Continua **sem resposta nem a ping** desta estação, com a VPN ativa e comprovada — Protheus
(`10.100.6.243:1433`) e o servidor de aplicação (`10.150.4.249`) respondem no mesmo instante.
Se a liberação foi feita, ela **não alcançou esta estação**; é plausível que tenha sido concedida
ao servidor de aplicação, o que não me permite consultar de onde eu trabalho.

---

## 3. Sanitização — o que a base tem de podre

`[medido]` sobre **23.945 clientes** carregados:

| Achado | Quantidade | O que significa |
|---|---:|---|
| **Sem documento** | **8.422 (35%)** | Não cruzam com o ERP, não têm faturamento, não entram em nenhuma métrica de negócio |
| Com CNPJ | 10.637 | |
| Com CPF | 4.886 | Produtor pessoa física |
| **Raízes de CNPJ repetidas** | **326 raízes, 726 cadastros** | São 326 empresas aparecendo como 726 clientes |
| Documento idêntico repetido | 4 documentos, 8 cadastros | Duplicata pura |

**Cuidado com a leitura fácil dos 35%.** A conclusão óbvia — "8.422 cadastros sujos, complete o
documento" — **não sobrevive à medição.** Cruzei os 8.422 contra os 29.946 nomes com documento na
`SA1010` do Protheus, normalizando acento, pontuação e caixa: **casam apenas 698 (8,3%)**.

Os outros **7.724 não existem no ERP** — e isso é coerente, não é defeito: **quem nunca comprou
não tem cadastro no faturamento**. São prospects, contatos e clientes de carteira que nunca
fecharam. Um prospect sem CNPJ é uma lacuna comercial, não uma sujeira de dados.

**O problema caro é outro, e é menor e mais preciso:** os **387 CNPJs que faturam e existem no CRM
apenas por nome** — **R$ 110,8 milhões** que não casam só porque falta a chave. Esses são os que
valem correção, e a correção é barata porque os dois lados já existem.

> Registro de honestidade: a primeira versão deste documento recomendava "completar o documento a
> partir da SA1" como se resolvesse os 8.422. Resolve 698. Recomendar sem medir teria custado uma
> tarefa grande com um décimo do retorno prometido.

**As 326 raízes são a segunda camada.** Grupo econômico não existe no Protheus (`A1_GRPVEN` vazio
em 100%), então a única forma de agrupar é pela raiz do CNPJ. Sem isso, uma usina com quatro
cadastros aparece como quatro clientes médios em vez de um cliente grande — e a curva ABC erra.

### 3.1 A ordem de correção que eu recomendo

1. **Completar o documento a partir da SA1 do Protheus**, casando por nome normalizado. Resolve os
   387 que já sabemos existir dos dois lados.
2. **Agrupar por raiz de CNPJ** e preencher `Cliente.ClienteMatrizId`, que já existe no modelo.
3. **Só então** recalcular a curva ABC — antes disso ela mede o cadastro, não o cliente.

---

## 4. O que cada indicador da tela deve puxar

Seguindo o que você anotou no print, e com o que eu confirmei nas fontes:

| Indicador | Fonte que você indicou | Situação real |
|---|---|---|
| Faturamento em curso | ART ou Protheus | ✅ **Protheus SQL** — funciona hoje |
| Previsão FY 2026 | ART ou arquivo de meta | ⛔ `organizacao.Meta` vazia; precisa da meta do negócio |
| Clientes na carteira | Sanitização (CRM + Protheus) | ⚠️ funciona, mas **35% sem documento** contaminam |
| Cobertura ativa | Transacional de visitas (CRM) | ⚠️ funciona; a rosca usa 30/90 dias **inventados** — a cadência declarada é 180/120/360 |
| Conhecimento de mercado | ART + vendas perdidas + dados externos | ⛔ depende do ART |

**Dois dos cinco indicadores dependem do ART.** Um terceiro depende de uma decisão de negócio (a
meta). Os outros dois funcionam, mas medem sobre uma base com 35% de cadastro incompleto.

---

## 5. O que muda no modelo de dados

A planilha pede colunas que o modelo atual não tem. O que precisa entrar:

**No cliente** — todas de origem ART, e **todas de seleção, nunca texto livre**:
`AreaTotalHa`, `AreaAgriculturavelHa`, `AtividadePrincipal` (catálogo), `CultivoPrincipal`
(catálogo), `AreaCultivoPrincipalHa`, `CultivoSecundario`, `AreaCultivoSecundariaHa`,
`ProdutorRural` (sim/não).

**No equipamento** — hoje inexistentes: `HorimetroAtual`, `HorimetroLidoEm`,
`OrigemDaLeitura` (catálogo: Operation Center, Simova, Manual), `AnoDeFabricacao`,
`EntregaTecnicaEm`, `SerieDoProduto`, `Familia`, `LinhaDeProduto`.

**Do Protheus, já disponíveis e ainda não usados:** `A1_LC` (limite de crédito — R$ 123,7 milhões
concedidos a 10.760 clientes `[medido]`), `A1_RISCO`, `A1_VENCLC`.

**Por que catálogo e não texto:** "Cana de açúcar", "cana-de-açucar" e "CANA" são a mesma cultura
escrita de três jeitos, e qualquer soma por cultura vira três linhas. É a mesma razão pela qual a
base do Vórtice não serve para relatório — e repetir o erro na tabela nova seria imperdoável.

---

## 6. Pendências, em ordem de impacto

1. **Rota para o ART.** Bloqueia base instalada, horímetro e área plantada — ou seja, **bloqueia o
   modelo de negócio**. Tudo o mais é contorno.
2. **Completar documento de 8.422 clientes** e agrupar as 326 raízes de CNPJ.
3. **Meta FY 2026** — decisão do negócio, não de tecnologia.
4. **Cadência na rosca de cobertura** — trocar 30/90 pela cadência declarada.
5. **CFOP 5949** — R$ 38,1 mi aguardando o fiscal.
