# 54 — Desempenho e arquitetura: telas em até 2 segundos, código dividido por responsabilidade

> **Versão 1.1 — 03/10/2026.** Primeira das cinco frentes de organização pedidas pelo Ricardo em 03/10/2026, nesta ordem:
> **desempenho + arquitetura** → banco sem redundância → o que falta → material da diretoria.
> Diagnóstico de partida: `.omc/plans/diagnostico-da-organizacao-2026-10-03.md`.
> A 1.1 registra o que o plano 1 de 3 entregou (§7) e corrige a §3.2: a versão é a assinatura do próprio dado.

## 1. Decisões que este documento cumpre

| # | Decisão (Ricardo, 03/10/2026) |
|---|---|
| DA-1 | Desempenho e arquitetura são atacados **juntos**, a começar pela apuração do território, onde os dois problemas se cruzam. |
| DA-2 | **Meta: toda tela abre em até 2 segundos**, no p95 medido pelo servidor (Configurações › Integrações › Desempenho). |
| DA-3 | **Caminho B:** dividir a apuração por natureza do dado. O dado de referência é calculado uma vez e compartilhado; o dado do recorte é lido na hora, só o que a tela usa. Sem tabelas de resumo pré-calculadas, para não criar dado derivado no banco (a frente 4 pede banco sem redundância). |
| DA-4 | **Carga incremental:** a primeira carga traz tudo; depois cada rodada traz só o que mudou nos **últimos 3 dias**, com uma **conferência completa semanal**, de madrugada. |

## 2. O problema, medido

**A apuração do território.** `RepositorioDeIndicadoresTerritoriais.ApurarAsync` é um método único de **922 linhas** que faz pelo menos **38 consultas ao banco em sequência**. Ela lê tabelas inteiras: municípios, clientes, inativados, carteiras, vínculos, faturamento de três anos, notas sem cliente, vendas do ART, PAM de dois anos, estrutura agropecuária, totais do estado e parque conectado. Cada tela que precisa de um pedaço refaz tudo:

| Rota | Usa da apuração | Paga pela apuração inteira |
|---|---|---|
| Indicadores Geográficos | tudo | sim |
| Demanda e Previsão | potencial por município, e a existência de venda do ART | sim |
| Diagnóstico Comercial | potencial, cobertura e vendas | sim |
| Cenários de Mercado | potencial (via Demanda) | sim, **duas vezes** na gravação |
| Sugestão de bandas de porte | potencial | sim |

**Duas naturezas de dado misturadas no mesmo método:**

- **Referência — igual para todo mundo.** Muda só quando uma rotina carrega dado novo:
  - a área de atuação, os municípios e as lojas;
  - as regras e o catálogo do potencial, e a PAM;
  - **o motor do potencial inteiro** (parque e demanda por município, categoria e cultura, no ano e no anterior);
  - a estrutura agropecuária, os totais do estado e a vocação.
- **Recorte — de cada pessoa.** Passa pelo filtro de filial:
  - clientes e endereços, carteiras e cobertura;
  - faturamento (com e sem cliente);
  - vendas do ART.

**O cache não ajuda o suficiente.** O cache das leituras (30/09/2026) guarda por endereço, até 10 minutos, e é limpo inteiro sempre que **qualquer** rotina termina. As metas da Gestão de Negócios rodam de hora em hora, então o cache cai de hora em hora, mesmo quando a PAM (anual) não mudou.

**O navegador baixa tudo de uma vez.** É um pacote único de **1,5 MB de JavaScript mais 435 KB de CSS**, com as 27 telas importadas juntas, sem carregamento por tela.

**As rotinas releem tudo a cada rodada:**
- o faturamento regrava **36 meses**;
- as OS do Protheus regravam **3 anos**, as peças 3 anos e os orçamentos 24 meses;
- os processos do Vórtice releem desde 01/11/2023;
- as carteiras, o parque, as metas, o planejamento e o estoque releem o conjunto inteiro.

## 3. O desenho

### 3.1 A apuração vira leitores por natureza

```
                 ┌─────────────── REFERÊNCIA (cache por assunto) ───────────────┐
                 │  Território     ADR, municípios, lojas                         │
                 │  Potencial      PAM + regras + catálogo → motor (parque e     │
                 │                 demanda por município × categoria × cultura,  │
                 │                 ano corrente e anterior)                      │
                 │  Estrutura      censo, rebanho, usinas, totais do estado,     │
                 │                 vocação                                       │
                 └───────────────────────────────────────────────────────────────┘
                 ┌─────────────── RECORTE (lido na hora, filtro de filial) ──────┐
                 │  Cobertura      clientes, endereços, carteiras, cadência      │
                 │  Vendas (R$)    faturamento com e sem cliente, duas janelas   │
                 │  Vendas (un.)   ART, por categoria, duas janelas              │
                 └───────────────────────────────────────────────────────────────┘
        Cada tela compõe só o que usa:
          Indicadores = tudo · Diagnóstico = Potencial + Cobertura + Vendas
          Demanda e Cenários = Território + Potencial (+ entregas do ART)
```

**Regras:**

1. **Uma porta por leitor, no domínio, com no máximo 4 membros.** É a regra que já existe; o repositório de 2 mil linhas sai do caminho.
2. **Nenhuma conta muda.** O motor, o fator de ciclo, as faixas e os critérios de grupo (fora do mapa, inativado, outra filial) continuam os mesmos. A conferência é **número igual antes e depois**, por teste.
3. **O "Território" e o "Potencial" não passam pelo filtro de filial** (já não passam hoje: área de atuação e PAM não têm dono). É por isso que podem ser compartilhados entre usuários com segurança.
4. **Cada leitor faz poucas consultas.** O que hoje é "ler tudo e somar em memória" continua em memória onde o volume é pequeno, mas cada leitor lê uma vez só o que precisa.

### 3.2 Cache por assunto, e não "limpa tudo"

- **Uma versão por assunto.** Cada assunto de referência tem a sua versão (território, PAM e regras, estrutura, preços e crédito). A versão é a **assinatura do próprio dado**: máximos de data e contagens nas tabelas do assunto, relidos a cada 15 s. Somam-se a ela as gravações feitas pela tela de Configurações do potencial. A rotina não precisa avisar ninguém, e a rodada que não grava nada não muda a versão.
- **Onde fica o cache.** Na memória da API (é um servidor só). Ele guarda o resultado do leitor, e não a resposta HTTP: a Demanda, os Cenários e o Diagnóstico aproveitam o Potencial que os Indicadores já calcularam.
- **Gravação pela tela.** Um parâmetro do potencial gravado na tela de Configurações avança a versão do assunto dele na hora. Quem grava a regra vê o número novo no mesmo instante.
- **O cache de 10 minutos por endereço continua** para o recorte, como hoje.

### 3.3 Carga incremental (DA-4)

- **Rodada normal.** Lê o que foi **criado ou alterado nos últimos 3 dias** na origem. Os 3 dias de sobreposição cobrem rodadas falhas, fins de semana e o atraso de espelhos como a GN (cerca de 21 h).
- **Conferência semanal** (domingo de madrugada). É a leitura completa de hoje, que pega o que a origem **apagou ou corrigiu lá atrás**: nota cancelada, OS reaberta, o Banco Central completando meses antigos, carteira desfeita.
- **Só vira incremental o que tem data de alteração confiável na origem.** Sem ela, a rodada de 3 dias perderia alteração, e a rotina continua completa.

| Rotina | Hoje | Proposta | A confirmar na origem |
|---|---|---|---|
| 1 FONTES_ANUAIS (IBGE, ANP) | anual, completa | **continua**: é anual e pequena | — |
| 2 PRECOS_MENSAIS (CONAB, Socicana, PTAX, SICOR) | mensal | **continua**, relendo só os meses recentes que a fonte revisa | janela de revisão do SICOR |
| 3 CADASTRO_CLIENTES (SA1) | completa | **incremental** + semanal | coluna de alteração da SA1 (`S_T_A_M_P_`/`I_N_S_D_T_`) |
| 4 FATURAMENTO_PROTHEUS | regrava 36 meses | **incremental pela emissão/alteração** + semanal | data de alteração/cancelamento da nota |
| 5 ART_VENDAS | serviço com marcador | **incremental** (já tem marcador) + semanal | se a view tem data de alteração |
| 6 CARTEIRAS_VORTICE | completa | **incremental** + semanal | datas de vínculo e de contato no Vórtice |
| 7 PARQUE_PROTHEUS (VV1) | completa | incremental se houver data; senão **diária completa** | coluna de alteração da VV1 |
| 8 PROCESSOS_VORTICE | desde 11/2023 | **incremental pelo histórico** + semanal | data do histórico (`IV_HISTORICO`) |
| 9 METAS / PLANEJAMENTO GN | completa, de hora em hora | **continua completa** (volume pequeno), mas só avança a versão quando muda algo | se a API GN aceita filtro por data |
| 10 ESTOQUE GN | completa | **continua**: com janela de datas a API perde os pedidos | — |
| 11 TELEMETRIA (Operations Center) | última leitura por máquina | **continua** (já é só a última) | — |
| 12 CONFERÊNCIA GN | comparação | **continua**: é comparação inteira por natureza | — |
| 14 PARTIÇÃO DA AUDITORIA | manutenção | — | — |
| 15 PÓS-VENDA PROTHEUS (OS, peças) | regrava 3 anos | **incremental** + semanal | data de alteração da OS e da nota de peça |

A confirmação é a primeira tarefa de cada rotina, e cada rotina é um PR à parte. **Nada muda de comportamento antes da confirmação.**

### 3.4 O navegador baixa só a tela aberta

- Cada tela vira um pedaço carregado sob demanda (`React.lazy`), com o esqueleto de carregamento que as telas já têm.
- O pacote inicial fica com o shell (menu, cabeçalho, login) e as bibliotecas comuns.
- O harness visual e as amostras continuam fora do pacote de produção; o `visual:conferir-pacote` já confere isso.

### 3.5 A vigilância, para não piorar de novo

| O que | Como |
|---|---|
| **Orçamento de consultas por rota** | Um contador de comandos SQL por requisição. Aparece ao lado do p95 em Configurações › Integrações › Desempenho, e um teste da API prende o máximo de cada rota pesada. |
| **Tempo antes e depois** | O p95 de produção, pela captura da tela de Desempenho antes e depois de cada entrega, e o número de consultas medido nos testes. |
| **Tamanho de arquivo** | Teste de arquitetura: arquivo novo com no máximo 600 linhas (C# e TS). Os que passam disso hoje (29 em C# e 26 no front) ficam numa lista que **só pode diminuir**. |
| **Arquitetura do front** | Teste de arquitetura: tela não importa outra tela; componente não importa tela; `dev/` não é importado pelo código de produção; peças comuns (filtros, formatos de número, ordenação) moram em `componentes/comum`. |
| **Duplicação no C#** | `SomaOuNulo`, `Arredondar`, leitura de mês `aaaa-mm` e repasse de falha viram utilitários únicos na Aplicação. |

## 4. Ordem de execução (cada passo é um PR pequeno, com teste e medida)

1. **Medir.** O contador de consultas por rota, mostrado no Desempenho, e o teste que registra a linha de base das rotas pesadas. Mais a captura de produção da tela de Desempenho, pedida ao Ricardo.
2. **Potencial e Território como leitores de referência com cache por assunto.** Demanda, Cenários, Diagnóstico e Sugestão de porte passam a usá-los. Número igual antes e depois.
3. **Indicadores Geográficos** compondo os leitores. O `ApurarAsync` deixa de existir.
4. **Front:** carregamento por tela e as regras de arquitetura do front.
5. **Regras de vigilância:** tamanho de arquivo, utilitários únicos e orçamento de consultas.
6. **Rotinas incrementais**, uma por PR, na ordem do ganho: faturamento, pós-venda, processos do Vórtice, carteiras, clientes e ART.
7. **As outras rotas lentas** que a captura de produção mostrar (Visão 360, Performance de CEN, Cobertura…), com o mesmo padrão.

## 5. O que não muda

- Nenhum número de tela, nenhuma regra de negócio e nenhuma decisão já tomada.
- A segurança: o filtro global de filial continua valendo em todo dado do recorte, e o cache compartilhado só guarda dado que já não tem dono de filial.
- O banco: nenhuma tabela nova nesta frente (DA-3).

## 6. Riscos e como cada um é contido

| Risco | Contenção |
|---|---|
| O cache mostrar número velho depois de uma carga | A versão é a assinatura do próprio dado (máximos e contagens, relidos a cada 15 s), e a gravação pela tela entra na hora. Testes gravam e leem em seguida: a PAM gravada por fora muda a assinatura, e a regra trocada pela tela chega à Demanda na leitura seguinte. |
| A carga de 3 dias perder alteração antiga | Só vira incremental a rotina com data de alteração confirmada, e a conferência semanal relê tudo. |
| Dividir a apuração mudar um número | Teste de "número igual" antes e depois de cada passo, com o mesmo cenário de dados dos testes de hoje. |
| Memória da API | O dado de referência é pequeno (645 municípios × poucas culturas e categorias); o recorte não fica em cache além do de hoje. |
| O teste instável conhecido do cache das leituras ("non-concurrent collections" com a conexão SQLite compartilhada em `ApiEmMemoria`) | Corrigido no passo que mexe no cache (passo 2), antes de qualquer regra nova em cima dele. |

> **A PAM do ano anterior** (`ComDemandaDoAnoAnterior`, #329) é parte do leitor de Potencial: a mesma conta, um ano para
> trás. Ela vem sempre junto, porque é uma leitura a mais por versão, e evita guardar duas versões do mesmo assunto.

## 7. Andamento

### 7.1 Plano 1 de 3 — medir, leitores de referência, front sob demanda e vigilância (03/10/2026)

Entregue na branch `feat/desempenho-e-arquitetura`. O que mudou:

- **O contador de consultas.** Toda resposta da API diz quantas vezes foi ao banco (cabeçalho `X-Consultas-Ao-Banco`), e a tela de Desempenho mostra o p95 e o máximo de consultas por rota, ao lado do tempo.
- **O cache de referência.** O território e o potencial são calculados uma vez por versão do assunto e compartilhados entre telas e usuários. A conta de cada município no motor também é feita uma vez e guardada.
- **A Demanda, os Cenários e a Sugestão de porte** deixaram de passar pela apuração inteira: leem só o território e o potencial de referência, mais o que é delas.
- **A apuração dos Indicadores** usa os dois leitores no lugar das leituras que fazia por conta própria.
- **O front baixa só a tela aberta.** Se a aba ficou aberta durante uma publicação e o pedaço sumiu, a página recarrega uma vez sozinha.
- **O teste instável do cache das leituras** foi corrigido: o vigia da versão não roda mais nos testes, porque disputava a conexão SQLite com a criação do banco.
- **A revisão final corrigiu três pontos, cada um com um teste que falhava antes:**
  - a conta que falhava depois que a tela tinha sido fechada ficava guardada; agora a leitura seguinte recalcula;
  - a assinatura do território não via o município que a carga reconhece no IBGE (código e nome mudam na mesma linha). Agora ela soma também os códigos e o tamanho dos nomes;
  - com o armazenamento do navegador bloqueado (navegação anônima), nenhuma tela abria.

**Consultas ao banco por chamada**, medidas pelo teste de orçamento com o mesmo cenário de dados:

| Rota | Antes (linha de base) | Depois, frio | Depois, quente |
|---|---|---|---|
| Indicadores Geográficos | 73 | 84 | **58** |
| Demanda e Previsão | 81 | 55 | **29** |
| Diagnóstico Comercial | 77 | 88 | **62** |
| Cenários de Mercado | 87 | 61 | **35** |
| Dimensionamento ADR | 24 | 24 | 24 |
| Gestão de Financiamentos | 16 | 16 | 16 |
| Preço de Commodities | 16 | 16 | 16 |

- **Frio** é a primeira chamada depois que a API sobe ou que o dado de referência muda. Ela paga a assinatura de cada assunto (5 consultas por assunto) e a conta do leitor, uma vez para todas as telas.
- **Quente** é a chamada com o cache cheio, que é o caso comum. Depois de 15 s sem chamada, a seguinte relê só as assinaturas (+5 por assunto), sem refazer a conta.
- Indicadores e Diagnóstico ainda passam pela apuração do recorte inteira: são o plano 2.

**O pacote do navegador:**

| | Antes | Depois |
|---|---|---|
| Pacote inicial (`index`) | 1.566 KB | **517 KB** |
| Pedaços | 1 | 104 (o maior de tela é o dos Indicadores, 275 KB) |

**A vigilância que entrou** (cada uma é um teste que reprova):

- o orçamento de consultas das sete rotas pesadas, em que o teto só desce;
- as regras de arquitetura do front: nada de `src/dev` em produção, componente não importa tela, tela não importa tela (hoje sem nenhuma exceção);
- o tamanho de arquivo: até 600 linhas. Os grandes de hoje, 29 em C# e 26 no front, estão listados e só diminuem.

**Ainda falta medir** o p95 de produção depois da publicação, pela captura da tela de Desempenho.

### 7.2 Próximos passos

- **Plano 2 — Indicadores e Diagnóstico.** Leitores do recorte (cobertura, faturamento, vendas do ART), o fim do `ApurarAsync` e a divisão do `RepositorioDeIndicadoresTerritoriais` (2.032 linhas). Entram também os utilitários únicos da §3.5 (`SomaOuNulo`, `Arredondar`, mês `aaaa-mm`) e as peças comuns do front em `componentes/comum`.
- **Plano 3 — rotinas incrementais** (§3.3): uma por PR, cada uma depois de confirmar na origem a data de alteração.
- **Passo 7 — as outras rotas lentas** que a captura de produção mostrar.
