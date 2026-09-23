# 50 — A tela da Inteligência de Mercado: estrutura e desenho

> **Data:** 23/09/2026 · **Status:** especificação **aprovada com correções** em 23/09/2026. Nada implementado.
> **Escopo:** **Visão Diretoria** (issue #76). O CEN usa a mesma tela com um recorte menor, mas **a Visão do
> CEN é a #78 e não entra nesta etapa** — nem regra, nem permissão, nem UX própria. Componentes e rotas
> nascem reutilizáveis para que a #78 os aproveite depois; é só isso que esta etapa faz por ela.
> **Detalha e substitui** a §10.3 do documento 49 ("A tela: o que fica, o que evolui e o que é novo").
> **Regra que continua valendo:** os quatro cartões de mapa ficam **juntos e lado a lado**; nenhum número
> sai da tela.

Marcação: **[M]** medido no código; **[D]** declarado pelo Ricardo; **[P]** proposta a aprovar.

---

## 0. O problema, em uma frase

A tela não tem **hierarquia**. Hierarquia se faz com **tamanho, espaço e contraste**, e hoje tudo tem o
mesmo tamanho, o mesmo espaçamento e o mesmo peso: 40 números na ficha do município e 12 blocos na página
gritam no mesmo volume. Não é excesso de dado — é ausência de ordem.

**Medido** [M]: `IndicadoresGeograficos.tsx` tem **1.340 linhas** (eram 1.149 quando a #170 foi aberta);
a ficha usa `grid-template-columns: repeat(auto-fit, minmax(240px, 1fr))`, que numa tela larga produz
**cinco a seis colunas de 240 px** — o pior arranjo possível para pares rótulo/valor, e a causa dos rótulos
quebrados em quatro linhas.

---

## 1. Vocabulário: captura não é market share

**Regra desta tela e do contrato da API** [D], alinhada com a #162:

> Enquanto o denominador for **demanda estimada pelo motor**, o número se chama **Captura Tracbel**.
> **Share** só pode aparecer quando existir um denominador **apropriado e comprovável** — o total de
> máquinas efetivamente vendidas no município por todos os fabricantes. Nenhuma fonte aberta dá isso hoje.

```
Captura Tracbel = vendas Tracbel (un) ÷ demanda anual estimada (un)
Oportunidade    = max(0, demanda ajustada − vendas)          (regra da #162)
```

Nenhum rótulo, tooltip, título de aba ou campo de contrato usa a palavra *share* para este número.

---

## 2. Perguntas que esta tela responde

**Critério de aceite da especificação** [D]. A coluna "hoje" diz o que o CRM responde **agora**.

| # | Pergunta | Onde responde | Hoje |
|---|---|---|---|
| 1 | Qual é o tamanho do mercado? | KPI *Mercado anual* + *Demanda anual* | **parcial** — em máquinas depende de **D-P01**; em R$ depende de **#70** |
| 2 | Quanto ele demanda por ano? | KPI *Demanda anual* · bloco Potencial | **depende de D-P01** |
| 3 | Qual a relevância deste município ante a região e SP? | selo de comparação em cada indicador (§7) | **parcial** — a fatia de SP existe (#72); a **fatia da Região** ainda não é exposta por município |
| 4 | O mercado está melhor ou pior que antes? | faixa + índice em *Momento do mercado* | **sim** (#73) |
| 5 | Por quê? | as quatro setas do fator, com tooltip de fórmula e fonte | **sim** (#74) |
| 6 | Quanto a Tracbel vende? | bloco *Performance Tracbel* | **sim em R$** (faturamento); **não em unidades** (#69) |
| 7 | Qual é a nossa **captura**? | KPI *Captura Tracbel* · bloco *Performance* | **não** — precisa de #69 |
| 8 | Quanto ainda não capturamos? | KPI *Oportunidade* | **não** — #69 e, para o R$, #70 |
| 9 | Quais culturas estão puxando o mercado? | *Momento do mercado*, uma linha por cultura | **sim** |
| 10 | O crédito está aumentando ou diminuindo? | aba *Crédito* | **sim** (#73) |
| 11 | O produtor ganhou ou perdeu poder de compra? | aba *Termo de troca* | **não** — precisa de **#70** |
| 12 | Qual cenário considerar no planejamento? | matriz de cenários | **estrutura sim** (#74); os números dependem de 1 e 2 |

**Leitura honesta:** cinco das doze são respondidas hoje; quatro dependem de **#69** e **#70**; uma da
decisão **D-P01**. O desenho prevê o lugar de todas; enquanto o dado não chega, o lugar mostra **vazio com
o motivo**, nunca um número inventado.

---

## 3. A estrutura: duas abas, um recorte

```
┌─────────────────────────────────────────────────────────────────┐
│  filtros · período · visão                     [Cafelândia ✕]   │  ← compartilhado
├─────────────────────────────────────────────────────────────────┤
│  ▸ MERCADO            Território                                │  ← Mercado é o padrão
└─────────────────────────────────────────────────────────────────┘
```

**Os filtros e o município escolhido ficam ACIMA das abas** [P]. É o que faz as duas abas serem leituras do
*mesmo recorte*: escolher um município no mapa e trocar de aba mantém os números daquele município. É o que
a #163 pede, e a escolha vive na URL (`?municipio=<código IBGE>`) para poder ser compartilhada.

**`Mercado` é a aba padrão** [D], porque o público é a diretoria.

---

## 4. A aba **Mercado** — cinco blocos

```
┌─ O MERCADO DA REGIÃO ───────────────────────────────────────────────────────┐
│  DEMANDA ANUAL      MERCADO ANUAL     CAPTURA TRACBEL   OPORTUNIDADE        │
│  3.457 máq/ano      R$ 2,1 bi         14,8%             R$ 1,8 bi           │
│  42,3% de SP        38,1% de SP       3,2 p.p. acima    85% do mercado      │
│                                       da Região                             │
│                                                                             │
│  Demanda 42,3% de SP        Momento: RETRAÍDO  ↓ 0,88                       │
│  ↓ Commodity   ↓ Crédito   → Custo   ↑ Percepção            ⓘ               │
├─ VISÃO GEOGRÁFICA ──────────────────────────────────────────────────────────┤
│  ┌─────────────────────────┬─────────────────────────┐                      │
│  │ Cobertura de carteira   │ Vendas realizadas       │                      │
│  ├─────────────────────────┼─────────────────────────┤                      │
│  │ Potencial               │ Estrutura agropecuária  │                      │
│  └─────────────────────────┴─────────────────────────┘                      │
├─ POTENCIAL ESTRUTURAL ──────────────────────────── [Simular cenário] ───────┤
│  [ Parque ] [ Demanda anual ] [ Cenários ]                                  │
├─ MOMENTO DO MERCADO ────────────────────────────────────────────────────────┤
│  [ Rentabilidade ] [ Crédito ] [ Termo de troca ] [ Percepção comercial ]   │
├─ PERFORMANCE TRACBEL ───────────────────────────────────────────────────────┤
│  [ Captura ] [ Vendas ] [ Não capturado ]                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.1 Os quatro KPIs executivos [D]

| KPI | Conta | Depende de |
|---|---|---|
| **Demanda anual** | soma, por cultura × categoria, de `área ÷ hectares por máquina ÷ ciclo de renovação` | **D-P01** |
| **Mercado anual** | **soma, por categoria/modelo, de `demanda da categoria × preço de referência daquela categoria`** | **#70** |
| **Captura Tracbel** | vendas Tracbel (un) ÷ demanda anual (un) | **#69** |
| **Oportunidade** | `max(0, demanda ajustada − vendas)` | #69 e, em R$, #70 |

**D-P01 não é só o ciclo de renovação** [D]. Conforme a #63, ela fixa quatro coisas juntas: **cultura ×
categoria/modelo de máquina × hectares por máquina × anos de renovação**. Sem as quatro não há demanda — e
é por isso que ela bloqueia os dois primeiros KPIs, não apenas um.

**Mercado anual nunca é `demanda total × um preço genérico`** [D]. O trator de R$ 300 mil da planilha vale
para a categoria dele; aplicá-lo à demanda inteira mistura colhedora com trator compacto. O valor sai da
**agregação por categoria/modelo**, cada uma com o preço de referência correspondente da **#70** — o que
torna a #70 dependência de valor, e não de conveniência.

**Enquanto faltarem, o cartão mostra o motivo e o que o destrava** — *"falta o ciclo de renovação e os
hectares por máquina (D-P01, issue #63)"* —, com link para a tela do Administrador. Nunca um número de
exemplo.

**Caminho curto** [P]: decidido o **D-P01**, sai a *Demanda anual*; entregue a **#69**, saem *Captura* e
*Oportunidade* **em unidades**. Só o *Mercado anual* em R$ depende da #70.

### 4.2 Porte e momento são **dois** números, nunca um [D]

- **Porte estrutural** — o tamanho do mercado: demanda anual e a comparação com a Região e SP. Muda devagar.
- **Momento** — o fator de ciclo (#74) e sua faixa. Muda todo mês.

Misturá-los esconderia a leitura que a diretoria precisa: *"estruturalmente grande, mas agora retraído"* é
uma decisão diferente de *"pequeno e aquecido"*.

**O porte não recebe rótulo enquanto não houver bandas configuradas** [D]. Chamar um município de "mercado
grande" exige um corte — e um corte sem dono é parâmetro inventado, o que a R-27 do doc 46 proíbe. Até lá o
porte se expressa **pelo número e pela comparação** (`3.457 máq/ano · 42,3% de SP`), sem adjetivo. As
**bandas de porte estrutural entram na #166** como parâmetro administrável com vigência, ao lado dos
demais; enquanto forem nulas, o rótulo não aparece e o parâmetro consta em `pendencias`.

O **momento**, esse já tem faixas decididas (#73/#74) e pode ser nomeado: retraído, normal, aquecido,
superaquecido.

### 4.3 O "por quê", em quatro setas

```
↓ Commodity   ↓ Crédito   → Custo   ↑ Percepção        ⓘ
```

Cada seta é uma **parcela do fator** (#74), com `InfoTooltip`: fórmula, fonte, competência e o valor da
parcela. **Cor nunca é o único sinal** — a direção da seta carrega o significado.

### 4.4 Visão geográfica — os quatro mapas ficam aqui [D]

Os quatro cartões continuam **juntos, lado a lado, na grade 2×2**, dentro da aba Mercado. É a parte da tela
que funciona; mandar a diretoria trocar de aba para vê-la seria perder o que ela tem de melhor.

Eles ganham **organização e dados melhores** (as evoluções da §10.3 do doc 49), sem mudar o desenho do
cartão.

### 4.5 Potencial estrutural

Abas internas: **Parque** · **Demanda anual** · **Cenários**. A calculadora é **ação secundária** no canto
do cartão — `Simular cenário` —, e abre em painel lateral. O painel manda, não a ferramenta.

**A matriz de cenários** [D], no lugar de três números soltos:

```
                    CONSERVADOR    MODERADO    OTIMISTA
  Demanda anual          18            22          27
  Mercado anual      R$ 14 mi      R$ 18 mi    R$ 22 mi
  Captura atual          14%           14%         14%
  Oportunidade       R$ 12 mi      R$ 15 mi    R$ 19 mi
```

O **moderado** vem destacado — é o fator calculado; os outros dois levam cada índice à borda da faixa em
que ele já está (#74). *Mercado anual*, *Captura* e *Oportunidade* dependem de #69/#70 e, até lá, mostram o
motivo **numa linha só**, sem repetir a mesma frase quatro vezes.

### 4.6 Momento do mercado

Abas internas: **Rentabilidade** · **Crédito** · **Termo de troca** · **Percepção comercial**. Uma linha por
cultura, com índice, faixa e variação. *Termo de troca* nasce com o motivo no lugar do número, até a **#70**.

### 4.7 Performance Tracbel

Abas internas: **Captura** · **Vendas** · **Não capturado**. Hoje só *Vendas* tem dado (faturamento em R$);
as outras duas mostram o motivo e a issue que as destrava (#69). A classificação de confiança da
oportunidade — alta, média, baixa — é a da #162 e aparece ao lado de cada linha.

---

## 5. A aba **Território** — operacional

Concentra o trabalho de campo [D]: responsáveis, carteira, cobertura, cadência, clientes, a tabela
operacional de municípios e o planejamento territorial. **Não duplica a experiência do Mercado**: onde um
dado já aparece resumido lá, aqui ele aparece na forma operacional — a lista, o nome, a data.

---

## 6. A ficha do município — resposta, porquê, evidência

```
┌──────────────────────────────────────────────────────────────────────┐
│  Cafelândia                      Noroeste · Catanduva      [Fechar]  │
├──────────────────────────────────────────────────────────────────────┤
│   DEMANDA ANUAL      MERCADO ANUAL     CAPTURA        OPORTUNIDADE   │
│   22 máquinas        R$ 18,4 mi        14,8%          R$ 15,7 mi     │
│   +18% cenário atual 0,9% da região    3,2 p.p. acima 85% do mercado │
│                      0,2% de SP        da Região                     │
│                                                                      │
│   Momento: AQUECIDO ↑ 1,24                                           │
│   ↑ Commodity   ↑ Crédito   → Custo   ↑ Percepção            ⓘ       │
├──────────────────────────────────────────────────────────────────────┤
│  LAVOURA                          ESTRUTURA                          │
│  Área plantada     37.226 ha      Tratores            422            │
│                    4,2% da região                     1,1% da região │
│                    0,8% de SP                         0,3% de SP     │
│  Valor da produção R$ 468,6 mi    Propriedades    162 de 253         │
│  Produtividade     1,63 t/ha      Rebanho          12.400            │
│                    10% acima de SP                    0,4% de SP     │
├──────────────────────────────────────────────────────────────────────┤
│  ▸ Fontes e competências    ▸ Cobertura e carteira                   │
└──────────────────────────────────────────────────────────────────────┘
```

**Três camadas** [D]:

1. **Executiva — sempre aberta.** Os quatro números de decisão com sua comparação, o momento e as setas.
2. **Intermediária — aberta.** Lavoura e estrutura, com todas as medidas de hoje e a comparação ao lado de
   cada uma. **Duas colunas, nunca cinco.**
3. **Evidência — recolhida.** Fontes, competências, notas metodológicas (sigilo do IBGE, Censo de 2017, ANP
   só etanol) e o bloco operacional de cobertura e carteira.

**Nada sai.** As 40 medidas de hoje continuam, redistribuídas entre as camadas 2 e 3.

---

## 7. Comparação com a Região e com SP: duas regras, não uma [D]

| Tipo de grandeza | Como compara | Exemplo |
|---|---|---|
| **Somável** — área, quantidade, valor da produção, parque, propriedades, rebanho, crédito, demanda, vendas em unidades | **fatia**: % da Região · % de SP | `37.226 ha · 4,2% da região · 0,8% de SP` |
| **Razão** — produtividade, preço, rentabilidade, **captura**, índices, fator | **diferença contra a referência**, em % ou em **pontos percentuais** | `1,63 t/ha · 10% acima de SP` · `Captura 14,8% · 3,2 p.p. acima da Região` |

**Uma razão nunca leva "fatia de SP"** [D]. Somar produtividades de municípios não dá a produtividade do
estado, e uma captura de 14,8% não é "uma fatia" de coisa alguma — dizer `0,8% de SP` sobre uma razão é um
número sem significado. Para razões, a referência é o **valor da Região ou de SP**, e o que se mostra é a
**distância até ele**: percentual quando a grandeza é contínua (produtividade, preço, rentabilidade),
**pontos percentuais** quando a própria grandeza já é um percentual (captura, índices).

**Lacuna a fechar** [M]: a API entrega hoje `RelevanciaNoEstado` — só a fatia de **São Paulo** (#72). Faltam
a fatia da **Região** por município e as referências para as razões. O denominador existe no mesmo cálculo;
falta expô-lo. É escopo da **#163** com a **#75**.

---

## 8. Regras de densidade e componentes

| Regra | Hoje [M] | Passa a ser [P] |
|---|---|---|
| Colunas de rótulo/valor | `auto-fit, minmax(240px)` → 5–6 colunas | **teto de 3 colunas**, mínimo 280 px |
| Rótulo longo | quebra em 4 linhas | rótulo curto; o detalhe no tooltip |
| Valor ausente | prosa dentro do `<dd>` | `ValorAusente`: traço + ⓘ com o motivo |
| Explicações | parágrafo cinza fixo de até 8 linhas | `InfoTooltip` |
| Ritmo vertical | tudo 16 px | 8 / 16 / 24 / 32 por nível |
| Hierarquia | mesma fonte e peso | **tamanho e espaço**; cor nunca sozinha |
| Números | `cad-mono` (tabular) | mantém |

**O tooltip é o componente da casa** [D]: `componentes/InfoTooltip.tsx`, que já tem `role="tooltip"` e
`aria-describedby`. **Nada de `title` cru** — há **23 usos em 14 arquivos** [M], quatro deles em
`IndicadoresGeograficos.tsx`; a troca é escopo da **#167**, junto do `Procedencia.tsx` que ela pede.

**Progressive disclosure com limite** [D]: camadas executiva **e** intermediária abertas; só fonte,
competência e nota metodológica ficam recolhidas. A diretoria não clica dez vezes.

---

## 9. Componentização

Sem isto o resto não se sustenta [M]: 1.340 linhas num arquivo.

```
telas/IndicadoresGeograficos.tsx        → casca: filtros, chip do recorte, abas
componentes/mercado/AbaDeMercado.tsx
componentes/mercado/KpisExecutivos.tsx
componentes/mercado/PorteEMomento.tsx
componentes/mercado/VisaoGeografica.tsx   (a grade 2×2, intacta)
componentes/mercado/BlocoDePotencial.tsx  (Parque · Demanda · Cenários)
componentes/mercado/MatrizDeCenarios.tsx
componentes/mercado/BlocoDoMomento.tsx    (Rentabilidade · Crédito · Troca · Percepção)
componentes/mercado/PerformanceTracbel.tsx
componentes/territorio/AbaDeTerritorio.tsx
componentes/comum/Comparacao.tsx          (a regra da §7, nas duas formas)
componentes/comum/ValorAusente.tsx        (o traço + ⓘ com motivo)
```

`Comparacao` e `ValorAusente` são os dois que aparecem dezenas de vezes — fazê-los uma vez é o que garante
que a regra valha em todo lugar.

---

## 10. O CEN não entra nesta etapa [D]

A **#78** continua sendo a Visão do CEN. Esta etapa **não** implementa recorte por carteira, permissão por
município nem UX própria do CEN. O que ela faz por ela é indireto e barato: componentes e rotas que aceitam
o recorte como parâmetro, para que a #78 os reaproveite em vez de clonar a tela.

---

## 11. O que **não** muda

- Nenhum número, tabela ou painel é removido.
- Os quatro cartões de mapa: mesmo desenho, mesma grade 2×2, mesmas abas internas, cursor sincronizado,
  legendas, clique que abre a ficha.
- A paleta, os tokens e as classes `cad-*` do design system.
- A tabela de municípios e os painéis de preço, custo e crédito — mudam de lugar, não de conteúdo.

---

## 12. O que esta tela precisa que ainda não existe

| O que falta | Tipo | Destrava |
|---|---|---|
| **D-P01** (#63) — cultura × categoria/modelo × ha por máquina × ciclo de renovação | **decisão** | Demanda anual · Mercado anual · cenários |
| **#69** — vendas Tracbel por município em unidades | dado interno | Captura · Oportunidade · Performance |
| **#70** — preço de referência por categoria/modelo | dado interno | Mercado anual em R$ · termo de troca |
| **Bandas de porte estrutural** (#166) | parâmetro com vigência | o rótulo de porte |
| Fatia da **Região** e referências das razões (#163/#75) | tarefa desta tela | as comparações da §7 |

**Ordem recomendada** [P]: D-P01 primeiro (é uma decisão, sai num formulário), depois #69 (dá três dos
quatro KPIs em unidades), depois #70 (converte o mercado para R$).

---

## 13. Sequência de execução [D]

| Fase | O que entra | Issues | Depende |
|---|---|---|---|
| **T0** | **Componentização pura.** Quebrar `IndicadoresGeograficos.tsx`, acrescentar testes de componente, **preservar pixel e estrutura**. Nenhuma aba nova, nenhuma mudança de posição. | **#170 parte A**, prova pela **#171** | — |
| **T1** | Casca: filtros e chip acima das abas, **Mercado** padrão e **Território** secundário. Grade 2×2 preservada dentro de Mercado. | #76 | **T0 verde** |
| **T2** | Recorte por município, comparação Região/SP (§7), `Comparacao`, `ValorAusente`, procedência por indicador, `InfoTooltip` no lugar de `title` e da prosa fixa. | **#163**, **#167**, #168, #75 | T1 |
| **T3** | Os quatro KPIs, Porte × Momento e as parcelas do fator. Valor inexistente continua inexistente **com motivo**; nunca mock. | #76, #74, #166 | T2; D-P01 para o número sair |
| **T4** | A ficha em três camadas. | #76 | T2 |
| **T5** | Matriz de cenários e `Simular cenário` como ação secundária. | #74, #161 | T3 |
| **T6** | Performance Tracbel, com **Captura** enquanto o denominador for demanda estimada. | **#162**, #69 | **#69 pronta** |

**T0 vem antes de qualquer reorganização visual** [D]. Mover blocos num arquivo de 1.340 linhas sem rede é
o jeito mais fácil de perder um número sem ninguém notar; a #171 é a rede.

**T0 não fecha a #170.** A #170 tem duas metades: **parte A**, a tela em componentes, e **parte B**, a
`RodadaDoMotor` gravada, com p95 abaixo de 500 ms medido no servidor. T0 entrega a parte A; a parte B segue
aberta na mesma issue.

---

## 14. Como o T0 se prova

Não existe suíte de regressão visual no CI [M] — isso é a **#43** (P2, marco M9), e esta etapa não a
antecipa. O que existe e será usado:

1. **Regressão estrutural, versionada e no CI** — teste de componente (Vitest + testing-library, no padrão
   `*.teste.tsx` do projeto) que renderiza a tela com a API simulada e afirma **a presença e a ordem** de
   todos os blocos. É o que pega "sumiu um painel" e "trocou a ordem".
2. **Regressão de pixel, local** — `scripts/prototipo/` já tem **Playwright + pixelmatch**, e
   `capturar-territorio.mjs` é o capturador desta tela. Captura antes × depois, e o resultado é o
   percentual de pixels diferentes. A saída tem número real da empresa e fica em `dados-locais/`, **fora do
   Git**.
