# 54 — Desempenho e arquitetura: telas em até 2 segundos, código dividido por responsabilidade

> **Versão 1.5 — 03/10/2026.** Primeira das cinco frentes de organização pedidas pelo Ricardo em 03/10/2026, nesta ordem:
> **desempenho + arquitetura** → banco sem redundância → o que falta → material da diretoria.
> Diagnóstico de partida: `.omc/plans/diagnostico-da-organizacao-2026-10-03.md`.
> A 1.1 registra o que o plano 1 de 3 entregou (§7) e corrige a §3.2: a versão é a assinatura do próprio dado.
> A 1.2 registra a captura de produção depois do plano 1 (§7.1) e o que o plano 2 de 3 entregou (§7.2).
> A 1.3 registra a primeira rotina incremental, o faturamento (§3.3 e §7.3).
> A 1.4 registra a segunda, o pós-venda: ordens de serviço, faturamento de peças e orçamentos (§3.3 e §7.3).
> A 1.5 registra que as rotinas 3, 5, 6 e 8 continuam completas (§3.3) e a parte 1 do passo 7: as outras telas pesadas
> aquecidas na subida e com orçamento de consultas (§7.4).

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
| 3 CADASTRO_CLIENTES (SA1) | completa | **continua completa** (decisão de 03/10/2026, abaixo) | — |
| 4 FATURAMENTO_PROTHEUS | regrava 36 meses | **incremental pela emissão** (o mês dos últimos 3 dias) + completa no domingo — **entregue no plano 3** (§7.3) | a SD2 não tem data de alteração confirmada; decisão de 03/10/2026: a completa de domingo mede o que a curta não veria |
| 5 ART_VENDAS | serviço com marcador | **continua como está** (decisão de 03/10/2026, abaixo) | — |
| 6 CARTEIRAS_VORTICE | completa | **continua completa** (decisão de 03/10/2026, abaixo) | confirmado: `IVS_Pes.DtaAlteracao` e `IV_HISTORICO.UltAlteracao` servem |
| 7 PARQUE_PROTHEUS (VV1) | completa | incremental se houver data; senão **diária completa** | coluna de alteração da VV1 |
| 8 PROCESSOS_VORTICE | desde 11/2023 | **continua completa** (decisão de 03/10/2026, abaixo) | confirmado: `IV_HISTORICO.UltAlteracao` e `IV_PROCESSO.DtaAlteracao` servem |
| 9 METAS / PLANEJAMENTO GN | completa, de hora em hora | **continua completa** (volume pequeno), mas só avança a versão quando muda algo | se a API GN aceita filtro por data |
| 10 ESTOQUE GN | completa | **continua**: com janela de datas a API perde os pedidos | — |
| 11 TELEMETRIA (Operations Center) | última leitura por máquina | **continua** (já é só a última) | — |
| 12 CONFERÊNCIA GN | comparação | **continua**: é comparação inteira por natureza | — |
| 14 PARTIÇÃO DA AUDITORIA | manutenção | — | — |
| 15 PÓS-VENDA PROTHEUS (OS, peças) | regrava 3 anos | **incremental** + completa no domingo — **entregue no plano 3** (§7.3): OS pela abertura e pela mudança de situação, peças pelo mês curto, orçamentos pela data de alteração | a OS não tem `S_T_A_M_P_` (VO1, VO3, VO4); o orçamento tem `data_alteracao_orc` |

A confirmação é a primeira tarefa de cada rotina, e cada rotina é um PR à parte. **Nada muda de comportamento antes da confirmação.**

**As rotinas 3, 5, 6 e 8 continuam completas** (decisão do Ricardo, 03/10/2026). A carga incremental existe para tirar
tempo da rodada, e o que o código registra dessas leituras é de segundos:

- **processos do Vórtice** (funil, oportunidades, vendas perdidas e financiamentos): o funil em 2 a 3 s e os formulários
  em 1 s (medido em 27/09/2026);
- **carteiras**: a consulta mais pesada, a do último contato, em 1,3 s na primeira leitura e 0,7 s na segunda (27/09/2026);
- **ART**: a view inteira volta em segundos, e a carga só grava o que mudou;
- **cadastro de clientes (SA1)**: não há tempo registrado no código. São cerca de 33 mil clientes, e o tempo real sai do
  resumo da rotina em Configurações › Integrações.

Ficar completa também evita o risco que a §3.3 aponta: a rodada curta perder alteração antiga. A confirmação na origem
do Vórtice foi feita mesmo assim, pelo agente do Vórtice, só leitura (`.omc/research/confirmacao-vortice-incremental-2026-10-03.md`).
Nenhuma tabela tem gatilho nem `rowversion`: as datas são mantidas pela aplicação.

| Tabela | Coluna | Preenchida | O que ela diz |
|---|---|---|---|
| `IV_HISTORICO` | `UltAlteracao` | ~100% | acompanha a edição; 97,7% do retroativo de 2026 entra em até 3 dias |
| `IV_PROCESSO` | `DtaAlteracao` | 100% | 51% dos processos de 2026 mudam mais de 3 dias depois da inclusão: a inclusão não serve |
| `IVS_Pes` | `DtaAlteracao` | 99,2% | — |
| `IVS_Carteira`, `IVS_CartDepto`, `IV_VENDEDOR` | — | — | 655, 206 e 352 linhas: lidas inteiras de qualquer jeito |

Se um dia a rodada dessas rotinas pesar, o caminho incremental está confirmado.

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
6. **Rotinas incrementais**, uma por PR, na ordem do ganho: faturamento, pós-venda, processos do Vórtice, carteiras, clientes e ART. Faturamento e pós-venda entregues; as outras quatro continuam completas (§3.3).
7. **As outras rotas lentas** que a captura de produção mostrar (Visão 360, Performance de CEN, Cobertura…), com o mesmo padrão. Parte 1 entregue (§7.4): aquecimento e orçamento de consultas.

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

**A captura de produção depois da publicação** (03/10/2026, publicação das 14:01, tela Configurações › Integrações ›
Desempenho). Quase toda rota estava na primeira chamada depois da subida. Não houve captura "antes".

| Rota | p50 (ms) | p95 (ms) | Consultas por chamada |
|---|---|---|---|
| `territorio/indicadores` (1 chamada) | — | **10.629** | 102 |
| `indicadores-executivos` | 619 | 2.949 | 21 |
| `mercado/demanda` | — | 2.909 | 40 |
| `mercado/financiamentos` | — | 2.748 | 20 |
| `mercado/dimensionamento` | — | 2.602 | 24 |
| `funil-por-estagio` | 936 | 2.505 | — |
| `cen` | — | 2.461 | — |
| `faturamento` | — | 2.102 | — |
| `metas` | 189 | 1.752 | — |
| `vendas-perdidas` | 15 | 1.742 | — |

**O que ela mostra:** o pior é a primeira chamada depois de cada subida. Nela o EF compila cada consulta pela primeira
vez e os caches de referência estão vazios. Onde houve várias chamadas, o p50 já é baixo (funil 936 ms, metas 189 ms,
vendas perdidas 15 ms). Por isso o plano 2 ganhou duas peças que não estavam no desenho: o aquecimento na subida e a
estrutura agropecuária como terceiro assunto de referência.

### 7.2 Plano 2 de 3 — Indicadores e Diagnóstico: estrutura de referência, aquecimento e a apuração dividida (03/10/2026)

Entregue na branch `feat/desempenho-plano-2`. Nenhuma tabela nova e nenhuma migração. O que mudou:

- **A estrutura agropecuária é dado de referência.** O parque de tratores do Censo, as propriedades, o rebanho, as
  usinas, os totais do estado e da região e o ano de cada fonte formam o terceiro assunto do cache, ao lado do
  território e do potencial (`RepositorioDaEstruturaDeReferencia`). A vocação agrícola depende do recorte; ela é
  calculada a cada apuração sobre uma cópia, e a estrutura guardada não muda.
- **A assinatura de cada assunto é uma consulta só.** Eram cinco idas ao banco por assunto; agora são subconsultas numa
  consulta. Uma armadilha do EF ficou registrada no código: `Count()` sem predicado dentro da projeção vira uma ida
  própria ao banco, e por isso a assinatura usa `Count(_ => true)`.
- **O aquecimento na subida** (`AquecimentoDaApi`). Logo depois que a API sobe, ele:
  - calcula as três referências;
  - roda a apuração dos Indicadores uma vez, no período padrão, sob contexto de sistema;
  - deixa as consultas do EF compiladas antes do primeiro usuário.

  A falha vai para o log e não derruba nada: a primeira tela volta a pagar a conta inteira, como antes.
  `Aquecimento:Ligado = false` desliga (doc 23 §2.15).
- **A apuração virou composição.** O `ApurarAsync` só lê e compõe; a conta acontece em memória, sem banco. O
  `RepositorioDeIndicadoresTerritoriais` desceu de **2.032 para 221 linhas** e saiu da lista dos arquivos grandes:

  | Arquivo | Linhas | O que faz |
  |---|---|---|
  | `LeitoresDoRecorteTerritorial` | 323 | clientes, cobertura, faturamento com e sem cliente, vendas do ART, parque conectado — o que passa pelo filtro de filial |
  | `AcumulacaoDoRecorte` | 308 | cada cliente, vínculo, nota e venda num grupo (município de SP ou fora do mapa) |
  | `MontagemDosIndicadores` | 501 | os municípios do recorte por cima da referência, o ano anterior e as procedências |
  | `PotencialNaMontagem` | 136 | o potencial do recorte e a relevância contra São Paulo |
  | `AcumuladorDoRecorte` | 124 | o contador de cada grupo e os grupos fora do mapa |
  | `RepositorioDoHistoricoDoMunicipio` | 266 | a série de cada município, numa classe própria |
  | `CriterioDasVendasDoArt` / `CodigosDoIbge` | 80 / 98 | o critério de data do ART e os códigos do IBGE, cada um num lugar só |

- **O mesmo número antes e depois.** Durante o plano, a apuração antiga ficou copiada nos testes, e a nova foi comparada
  com ela campo a campo em onze consultas. Os casos: filial, empresa, região, loja, filial da venda, filial do cliente,
  categoria, CEN, mês em curso, ano anterior e dezoito meses. As onze bateram depois de cada passo, e a cópia saiu no
  fim. Nenhuma expectativa numérica dos testes existentes foi editada.
- **Os utilitários únicos** (§3.5):
  - `Numeros.SomaOuNulo` e `Numeros.Arredondar` no Domínio, e `LeituraDeCompetencia.Ler` (o mês `aaaa-mm`) na
    Aplicação, substituem doze cópias privadas espalhadas por oito arquivos;
  - no front, as três cópias da moeda compacta passaram a usar `formatarBRLCompacto`, de `dados/formatadores`;
  - duas regras de arquitetura reprovam cópia nova, uma no C# e uma no front.

**Consultas ao banco por chamada**, no teste de orçamento com o mesmo cenário de dados:

| Rota | Linha de base | Plano 1, quente | **Plano 2, quente** | Plano 1, frio | **Plano 2, frio** |
|---|---|---|---|---|---|
| Indicadores Geográficos | 73 | 58 | **43** | 84 | **72** |
| Demanda e Previsão | 81 | 29 | **29** | 55 | **47** |
| Diagnóstico Comercial | 77 | 62 | **47** | 88 | **76** |
| Cenários de Mercado | 87 | 35 | **35** | 61 | **53** |
| Dimensionamento ADR | 24 | 24 | 24 | 24 | 24 |
| Gestão de Financiamentos | 16 | 16 | 16 | 16 | 16 |
| Preço de Commodities | 16 | 16 | 16 | 16 | 16 |

- O frio desce em todas as rotas que usam a referência, porque a assinatura passou de cinco consultas para uma.
- O quente dos Indicadores e do Diagnóstico desce 15, porque a estrutura não é mais relida.
- Este cenário de teste não tem Censo nem rebanho, e metade das leituras da estrutura já era pulada. Com as fontes
  carregadas, como em produção, a contagem das leituras dá perto de 31 consultas a menos em cada uma das duas telas.
  É estimativa: a captura do "depois" é que mede.

**Ainda falta medir** a captura de produção depois desta publicação, com as telas pesadas abertas algumas vezes. É ela
que diz se a primeira chamada depois da subida desceu dos 10,6 s e se a meta de 2 s foi alcançada.

### 7.3 Plano 3 de 3 — rotinas incrementais (03/10/2026)

**1. O faturamento** (`FATURAMENTO_PROTHEUS`), entregue na branch `feat/incremental-faturamento`.

A confirmação na origem, que o §3.3 exige antes de cada rotina, não fechou para a SD2:

- **A leitura de produção desta estação está negada.**
- **O extrator do BI não resolve.** Ele usa o `S_T_A_M_P_` para carga incremental na SB1 e na SF4, mas lê a SD2 e a SF2
  inteiras. Os blocos incrementais delas estão comentados, copiados da SB1.

O Ricardo decidiu, em 03/10/2026, ir **pela emissão, com a completa semanal medindo o que a curta não veria**:

- **Nos dias comuns**, a rotina relê só o mês em que caem os últimos três dias de emissão. Nos três primeiros dias do
  mês, ela começa no mês anterior. O que é anterior a essa janela fica como está.
- **No domingo**, ela relê os 36 meses. Também relê em qualquer dia quando a última completa com sucesso tem sete dias ou
  mais, quando nunca houve uma, ou com `--somente-faturamento --completa`. A completa que cai no meio fica como falha e
  não conta.
- **A curva ABC olha 36 meses nos dois modos.** Com a janela da leitura curta, ela rebaixaria a D o cliente que comprou
  em março e nada desde então.
- **A medida que confirma.** A completa conta o que corrigiu antes da janela curta, com e sem cliente: mês novo, mês com
  valor mudado e mês removido. Ela escreve o resultado no relatório, inclusive quando é zero.
- **O que espera até domingo, de propósito.** O cliente cadastrado no CRM durante a semana só herda os meses dele
  ANTERIORES à janela curta na completa. Até lá, esses meses ficam no faturamento sem cadastro, e a classe ABC dele não os
  conta. O total não muda, porque nada é contado duas vezes. O caso é raro: cliente novo costuma ter só venda recente,
  que a leitura curta já atribui. A conta da completa inclui essa mudança (um mês removido do sem cadastro e um novo no
  cliente).
- **Onde ver:** Configurações › Integrações, na mensagem da rotina e nos dois fluxos de sincronização,
  `PROTHEUS.FATURAMENTO` (curta) e `PROTHEUS.FATURAMENTO_COMPLETO`. A linha da rotina diz o modo, desde quando leu e, no
  domingo, "antes de mm/aaaa, N mês(es) corrigido(s) (R$ X) que a leitura curta não teria visto".

**Ainda falta medir:** o tempo da rodada curta contra o da completa, e o "corrigido antes da janela curta" dos primeiros
domingos, depois da publicação. Se o corrigido for grande e frequente, a janela curta aumenta.

**2. O pós-venda** (`POS_VENDA_PROTHEUS`, os dois modos), entregue na branch `feat/incremental-pos-venda`.

O alcance é o mesmo do faturamento. Ele virou genérico (`AlcanceDaLeitura`), e cada carga passa o início da janela inteira
dela. O que cada leitura curta pega:

| Dado | Leitura curta | O que ela não exclui |
|---|---|---|
| **Ordens de serviço** (sincronia) | as abertas desde o início curto, as ainda na oficina (A, L, D) e as **fechadas ou canceladas desde o início curto**. A VO1 não tem data de alteração; a data de fechamento faz o papel dela | a OS antiga que não veio: pode ter sido fechada com a rotina parada, e quem a resolve é a completa |
| **Faturamento de peças** (apuração por mês) | o mês curto, regravado inteiro; os meses anteriores ficam como estão | — |
| **Orçamentos de peças** (sincronia) | os orçados desde o início curto, os abertos e os **alterados desde o início curto** (a view tem `data_alteracao_orc`) | o orçamento antigo que não veio |

- **A conta da completa.** Ela conta as OS e os orçamentos corrigidos fora do alcance curto, e os meses-filial do
  faturamento de peças com valor diferente antes do início curto, com os reais.
- **A última completa de cada modo** é um ponto de sincronismo próprio: `PROTHEUS.ORDENS_DE_SERVICO_COMPLETO` e
  `PROTHEUS.FATURAMENTO_PECAS_COMPLETO`. Ele é gravado na mesma transação da carga, então a completa que cai no meio não
  conta.
- **O resumo da rotina** começa pelo alcance: "leitura curta desde dd/mm/aaaa", ou "leitura COMPLETA desde …, N corrigidas
  fora do alcance curto".
- **A janela regravada vem do alcance, e não do "desde" da leitura.** Com a leitura que viesse inteira numa rodada curta,
  a apuração das peças apagaria os 36 meses.
- **O leitor das OS** saiu da lista dos arquivos grandes: as regras de valor do BI foram para arquivo próprio.
- **O que esperar do ganho.** As consultas das views comparam datas convertidas, como já faziam, e o SQL Server do
  Protheus provavelmente continua varrendo as views nas duas leituras. O que cai com certeza é o volume que atravessa a
  rede e o trabalho do CRM. O tempo da rodada curta contra o da completa, em Integrações, é que mede o ganho.

**As outras quatro rotinas** (processos e carteiras do Vórtice, cadastro de clientes e ART) **continuam completas**, por
decisão de 03/10/2026: já leem em segundos (§3.3). Com isso o passo 6 está encerrado.

### 7.4 Passo 7, parte 1 — as outras telas pesadas aquecidas e com orçamento (03/10/2026)

Entregue na branch `feat/passo-7-telas-lentas`. Nenhuma tabela nova, nenhuma migração e nenhum número de tela mudado.

**O que a medida mostrou antes de mexer.** As seis rotas de relatório que a captura de produção mostrou entre 1,7 s e
2,9 s (§7.1) vão pouco ao banco, e o mesmo tanto na primeira chamada e na segunda. Nenhuma faz consulta dentro de laço.
Onde houve várias chamadas, o p50 já era baixo (funil 936 ms, metas 189 ms, vendas perdidas 15 ms). O peso delas é a
primeira chamada depois da subida, quando o EF compila cada consulta. É o mesmo diagnóstico dos Indicadores, e o mesmo
remédio do plano 2.

**O que mudou:**

- **O aquecimento roda as seis telas.** Além das três referências e dos Indicadores, o `AquecimentoDaApi` roda, uma vez
  cada, no padrão da tela e num banco em contexto de sistema, o caso de uso de:
  - Visão 360;
  - funil por estágio;
  - painel do CEN;
  - faturamento;
  - metas;
  - vendas perdidas.
- **A meta confere `Meta.Ler` no caso de uso**, e o contexto de sistema passa: serviço de sistema alcança toda permissão
  na organização. Nenhum contexto novo foi criado.
- **Cada tela é isolada.** A que lança ou é recusada vai para o log com aviso, e as outras rodam. O log da subida diz o
  tempo de cada tela: `API aquecida em … ms: referências e Indicadores … ms, Visão 360 … ms, …`.
- **O orçamento de consultas ganhou as seis rotas**, com o teto igual ao medido, que só desce.

**Consultas ao banco por chamada**, no teste de orçamento, com o cenário de dados dos Cenários de Mercado:

| Rota | Frio | Quente |
|---|---|---|
| Visão 360 (`relatorios/indicadores-executivos`) | 19 | 19 |
| Funil por estágio | 11 | 11 |
| Painel do CEN | 10 | 10 |
| Faturamento | 8 | 8 |
| Metas | 15 | 15 |
| Vendas perdidas | 12 | 12 |

Frio e quente dão o mesmo número porque essas rotas não usam o cache de referência.

**Ainda falta medir:** a captura de produção depois desta publicação, com as telas abertas algumas vezes, e o log `API
aquecida em …` da subida. Se uma rota ainda passar de 2 s com o aquecimento feito, o problema é a consulta dela, e não a
compilação. Ela vira a parte 2 do passo 7, com o plano de execução da consulta no SQL Server.

### 7.5 Próximos passos

- **A captura do "depois"** dos planos 2 e 4 entra nas §7.2 e §7.4. As rotas que ainda passarem de 2 s viram a parte 2
  do passo 7.
- **O resumo das rotinas do faturamento e do pós-venda** num dia comum e no primeiro domingo entra na §7.3 como a medida
  do ganho.
- **As peças comuns do front em `componentes/comum`**, além da moeda compacta, ficaram fora do plano 2 e continuam na
  fila.
