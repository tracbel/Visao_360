# 42 — Classe do cliente e cadência de contato: análise e decisão

> **Versão 1.0 · 15/09/2026 · bloqueador da fase 5** do
> [plano executivo](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md).
> **Nada foi alterado.** Este documento mede, apresenta cenários e **não escolhe a regra de negócio**.
>
> Fontes: código em `src/`; dados do banco de arquivo `TracbelCrmArquivo20260915` (contêiner
> `tracbel-crm-ensaio`, consulta somente leitura), que preserva a base anterior à sanitização.

## 1. O problema, em uma frase

**A mesma pergunta — "que classe é este cliente e de quanto em quanto tempo ele deve ser visitado?" —
tem duas respostas diferentes no banco, e as telas não leem a mesma.**

| Tela | Classe que usa | Cadência que usa |
|---|---|---|
| **Cobertura de Carteira** | `ClienteCarteira.Classe` | `ClienteCarteira.DiasCicloContato` |
| **Performance de CEN**, **Visão 360 executiva**, **Indicadores Geográficos** | `Cliente.Classe` | `LinhaDeNegocio.DiasCicloClasse<A..D>` pela classe do cliente |

## 2. Classe

### 2.1 Onde existe

| Coluna | Tipo | Quem grava | Regra |
|---|---|---|---|
| `comercial.Cliente.Classe` + `ClasseApuradaEm` + `FaturamentoApurado` | `ClasseDeCliente` (A, B, C, D) | a carga de faturamento (`CargaDeProcessoDoVortice.Faturamento.cs:283-326`), via `Cliente.ApurarClasse` | **curva ABC do faturamento real**: A = primeiros 80% do faturamento, B = até 95%, C = até 100%, D = sem compra na janela |
| `comercial.ClienteCarteira.Classe` | `ClasseDeCliente` | a carga do Vórtice, ao criar o vínculo (`CargaDeProcessoDoVortice.cs:772`) | **campo `Potencial` do Vórtice**, saneado em `SaneamentoDeProcesso.cs:191-203`: `A`/`B`/`C`/`D` viram a letra; **qualquer outra coisa vira `C`** |

### 2.2 Quem lê

| Leitor | Coluna | Uso |
|---|---|---|
| `RepositorioDeCarteiras.cs:49`, `:147-170` | `ClienteCarteira.Classe` | filtro e ordenação da Cobertura |
| `ContratosDeRelacionamento.cs:330` | `ClienteCarteira.Classe` | a letra que a tela mostra |
| `RepositorioDoPainelDoCen.cs:129-148` | `Cliente.Classe` | cadência do ranking de CEN |
| `RepositorioDeIndicadoresExecutivos.cs:175-199` | `Cliente.Classe` | cobertura da Visão 360 executiva |
| `RepositorioDeIndicadoresTerritoriais.cs:131`, `:230` | `Cliente.Classe` | cobertura por município |
| `RepositorioDeFaturamento.cs:82-92` | `Cliente.Classe` | faturamento por classe |

### 2.3 Dados históricos (base anterior à sanitização)

**Distribuição — `ClienteCarteira.Classe`, 49.109 vínculos:**

| Classe | Vínculos | % |
|:-:|---:|---:|
| A | 5 | 0,01% |
| B | 45 | 0,09% |
| C | **49.050** | **99,88%** |
| D | 9 | 0,02% |

**Distribuição — `Cliente.Classe`, 23.945 clientes:**

| Classe | Clientes | % |
|:-:|---:|---:|
| A | 428 | 1,8% |
| B | 610 | 2,5% |
| C | 2.767 | 11,6% |
| D | 20.140 | 84,1% |

**As duas comparadas, vínculo a vínculo:**

| Classe no cliente ↓ / no vínculo → | A | B | C | D |
|---|---:|---:|---:|---:|
| **A** | 0 | 2 | 1.519 | 1 |
| **B** | 2 | 0 | 1.880 | 2 |
| **C** | 1 | 1 | **6.958** | 1 |
| **D** | 2 | 42 | 38.693 | **5** |

- **Concordam em 6.963 de 49.109 vínculos — 14,2%.** Divergem em 42.146 (85,8%).
- **Nenhum cliente classe A do faturamento aparece como A na carteira.**
- A concordância de 6.958 linhas é só coincidência: cliente "C" com vínculo "C", sendo que o "C" do
  vínculo é o valor padrão do saneamento.

**Conclusão de medição:** `ClienteCarteira.Classe` **não carrega informação** — 99,88% é o valor padrão
gravado quando a origem estava em branco. O documento de domínio já registrava a origem do problema:
`IVS_Pes.Potencial` em branco em 134.757 de 139.072 vínculos, e `IVS_Pes.Classe` 100% em branco
(`ClienteCarteira.cs:8-30`).

## 3. Cadência

### 3.1 Onde existe

| Coluna | Quem grava | Preenchimento medido |
|---|---|---|
| `organizacao.LinhaDeNegocio.DiasCicloClasseA..D` | carga do Vórtice (`LinhaDeNegocio.DeclararCadencia`) | **4 de 14 linhas** têm cadência |
| `comercial.ClienteCarteira.DiasCicloContato` | carga do Vórtice, do campo `Ciclo` do vínculo | **1 de 49.109 vínculos** |

**Cadência declarada por linha de negócio:**

| Linha | A | B | C | D | Carteiras |
|---|---:|---:|---:|---:|---:|
| `MAQ_NOVOS` | 180 | 180 | 180 | 360 | 72 |
| `MAQ_PECAS` | 360 | 360 | 360 | 360 | 29 |
| `MAQ_AMS` | 360 | 360 | 360 | 360 | 5 |
| `PROSP_MAQ` | 120 | 120 | 120 | 180 | 1 |
| `DSI_PUK`, `DSI`, `MAQ_PNEUS`, `DADOS_CADAST`, `PECAS_EXTERN`, `MAQ_SERV`, `PROSP_PECAS`, `VENDAS_DIGIT`, `IRRIGACAO`, `NAO_INFORMADA` | — | — | — | — | 35 |

107 das 142 carteiras (75%) estão em linha com cadência declarada; 35 não.

### 3.2 O efeito prático na Cobertura

A Cobertura calcula "atrasado" assim (`ContratosDeRelacionamento.cs:333-337`): se
`DiasCicloContato` for nulo, **não há resposta** — a tela não diz se o cliente está em dia.

Como a coluna está preenchida em **um** vínculo, a Cobertura hoje:

1. mostra praticamente todo cliente como classe **C** (99,88%);
2. não consegue dizer quem está atrasado em 49.108 dos 49.109 vínculos.

As outras telas, que usam `Cliente.Classe` × cadência da linha, respondem — para 107 das 142 carteiras.

### 3.3 Dado adicional medido

| Coluna de `ClienteCarteira` | Preenchida em |
|---|---|
| `UltimaInteracaoEm` | 39.589 de 49.109 (80,6%) |
| `DiasCicloContato` | 1 |
| `PotencialAnual` | **0** |

## 4. Cenários

### Cenário A — a classe pertence ao Cliente

Uma classe por cliente, apurada do faturamento total.

| | |
|---|---|
| **Modelo** | `Cliente.Classe` + `ClasseApuradaEm`; `ClienteCarteira.Classe` sai |
| **Cadência** | `LinhaDeNegocio.DiasCicloClasse<A..D>` aplicada à classe do cliente |
| **A favor** | é o que o dado sustenta hoje; uma fonte só; reproduzível (dois auditores chegam ao mesmo resultado); não exige dado novo |
| **Contra** | um cliente que compra muita peça e nenhuma máquina é "A" também na carteira de máquinas — a cobertura de máquinas trataria como prioritário quem talvez não seja |
| **Efeito nas telas** | a Cobertura passa a classificar como as demais: 85,8% dos vínculos mudam de letra, e a maioria dos clientes vira D (84%) |
| **Custo** | baixo — remover uma coluna e trocar a leitura da Cobertura |

### Cenário B — a classe pertence à carteira (linha de negócio)

Uma classe por cliente **por linha**, como o desenho original previa.

| | |
|---|---|
| **Modelo** | `ClienteCarteira.Classe` volta a ter significado e passa a ser apurada, não digitada |
| **Como apurar** | exige faturamento **por linha de negócio**. O que existe é a quebra por **máquina, peça, serviço e outros** (`FaturamentoDoCliente`), que cobre `MAQ_NOVOS`, `MAQ_PECAS` e `MAQ_SERV`, mas não `AMS`, `PNEUS`, `DSI`, `PUK`, `IRRIGACAO` |
| **A favor** | responde melhor "quem é prioritário nesta linha"; preserva a intenção do modelo unificado |
| **Contra** | precisa de um de-para linha ↔ quebra que hoje não existe para 10 das 14 linhas; mantém duas colunas de classe (a do cliente para a visão executiva e a do vínculo para a cobertura), com risco de divergirem de novo |
| **Custo** | médio-alto — de-para, job de apuração por linha, e a decisão do que fazer com linha sem quebra correspondente |

### Cenário C — classificação calculada por contexto

Nenhuma classe gravada: a letra é calculada na leitura, com parâmetros (janela e recorte).

| | |
|---|---|
| **Modelo** | `Faturamento` é a única fonte; a classe some das tabelas |
| **A favor** | divergência deixa de ser possível; muda a janela e todo o sistema muda junto |
| **Contra** | a classe deixa de ter "fotografia": comparar hoje com o que se via em março exige recomputar com a janela de março; consultas ficam mais caras (curva ABC exige ordenar o faturamento de todos os clientes da filial) |
| **Custo** | médio — cálculo em consulta, índice por filial e competência; volume atual (23.945 clientes, 37.867 linhas de faturamento) é pequeno, mas cresce |

## 5. O que o código e os dados representam hoje

| Evidência | Aponta para |
|---|---|
| `Cliente.Classe` é apurada do faturamento e lida por 4 dos 6 leitores | **Cenário A** |
| `ClienteCarteira.Classe` existe, mas 99,88% é valor padrão — nunca carregou classe por linha | Cenário B **pretendido**, nunca realizado |
| A cadência por classe está em `LinhaDeNegocio`, não no vínculo | **Cenário A** (classe do cliente × cadência da linha) |
| `ClienteCarteira.DiasCicloContato` (1 linha) e `PotencialAnual` (0) | resquício de importação, não regra |
| A quebra de faturamento é por tipo de produto, não por linha de negócio | Cenário B **não é calculável** hoje para 10 das 14 linhas |

**Leitura técnica:** o sistema *implementa* o cenário A e *carrega a carcaça* do cenário B. O cenário C
é tecnicamente viável no volume atual e é o único que elimina a divergência por construção, ao custo de
perder a fotografia datada.

## 6. Pendência de negócio

> **Esta decisão é do comercial e da diretoria. Não há evidência suficiente nos dados para decidir
> entre A e B — porque o dado que sustentaria B nunca existiu.**

Perguntas objetivas para a decisão:

1. **A classe é do cliente ou da linha?** Um cliente pode ser A em máquinas e D em peças — e isso muda
   a rotina de alguém?
2. **Qual a janela de apuração?** Hoje a carga usou o faturamento acumulado disponível; a janela
   precisa ser declarada (12, 24, 36 meses?) e revista quando?
3. **Quem define a cadência das 10 linhas sem cadência?** Sem ela, a cobertura dessas carteiras não
   responde "atrasado".
4. **Cliente sem compra na janela é D?** É o que a regra atual faz (84% da base). O comercial confirma?
5. **A classe pode ser corrigida à mão?** Hoje não pode — é sempre apurada. Se puder, é preciso uma
   coluna de "classe declarada" com autor e motivo, e a regra de precedência.

## 7. Impacto no plano executivo

- A **fase 5** (cliente, contato, endereço, carteira) depende desta decisão: é ela que remove — ou
  mantém — `ClienteCarteira.Classe` e `DiasCicloContato`.
- **Se a resposta for A:** a fase 5 segue como está no documento 40 (duas colunas saem), e a Cobertura
  passa a ler `Cliente.Classe` e a cadência da linha.
- **Se for B:** a fase 5 mantém `ClienteCarteira.Classe`, **mas** ela passa a ser apurada por um job
  com de-para linha ↔ quebra, e entra uma fase nova para esse de-para; `Cliente.Classe` continua para a
  visão executiva, com a regra de precedência escrita.
- **Se for C:** a fase 5 remove as duas colunas de classe, e a apuração vira consulta — com medição de
  desempenho antes de valer nas telas.
- **Enquanto não houver decisão**, a recomendação técnica é preparar a fase 5 no cenário A (é o que o
  código já faz e o que o dado sustenta) e deixar a mudança para B ou C isolada num passo próprio.

## 8. Efeito visível quando a unificação acontecer

Qualquer que seja o cenário, a Cobertura vai mudar de número. No conjunto anterior à sanitização,
**85,8% dos vínculos trocariam de letra** ao passar a usar a classe do cliente. Isso precisa ser
comunicado antes de a tela mudar — e é o motivo de o plano executivo pedir uma comparação "antes e
depois" no ambiente de ensaio (risco R2).
