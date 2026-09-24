# 51 — Fechamento da camada de dados da Visão Diretoria

> **Data:** 23/09/2026 · **Status:** levantamento e plano. **Nada implementado nesta entrega.**
> **Pedido:** fechar a camada de dados da aba **Mercado** para que as dez perguntas da diretoria sejam
> respondidas com **dado real** ou **estimativa explicitamente aprovada** — sem métrica decorativa e sem
> preencher vazio com aproximação não autorizada.
> **Regra que não muda:** parâmetro sem valor decidido → **resultado vazio com o motivo**, nunca um número
> de exemplo. É a R-27 do documento 46.

Marcação: **[M]** medido no código ou no dado; **[D]** declarado; **[P]** proposta a aprovar.

---

## 0. As três coisas que este documento diz

1. **Quatro das dez perguntas não fecham com trabalho de engenharia.** Elas dependem de **dado interno que
   não está no servidor** (#69, #70) e de **decisões que só o comercial e a diretoria tomam** (D-P01, D-P08).
   Nenhuma quantidade de código muda isso.
2. **O caminho mais curto até os quatro KPIs executivos não começa por dado — começa por um formulário.**
   O D-P01 é uma **decisão**, e a tela onde ela se registra já existe (Configurações › Comercial › Potencial
   de mercado, #71/#77). O que falta lá é uma coluna: a **categoria de máquina**, que a rota de cadastro
   ainda não aceita. Isso é código, é pequeno, e **não depende de decisão nenhuma**.
3. **Há uma parede de calendário que ninguém notou** [M]: o índice de momento de preço é **12 meses ÷ 12
   anteriores**, e a CONAB só está carregada **desde 09/2025**. A série só completa 24 meses em **09/2027**.
   A saída existe e está no banco desde 2010 — o **preço implícito da PAM** (#198). Sem ela, a pergunta 4
   fica esperando dois anos.

---

## 1. FASE A — os bloqueios de negócio, decisão por decisão

Registro oficial: `48-POTENCIAL-DE-MERCADO.md` §5, issue **#63**. São **catorze** decisões, não doze.

### 1.1 As sete que o pedido nomeia

| Decisão | Estado | Opções que existem | Impacto na tela | Quem decide |
|---|---|---|---|---|
| **D-P01** cultura × categoria/modelo × ha por máquina × anos de renovação | **PENDENTE** | café **10 ha (CRM, 3036N) × 20 ha (planilha)**; as demais culturas só na planilha; laranja perene e cana semiperene; "máquinas em geral" pede categorias além de trator | **Demanda anual** e **Mercado anual** vazios; o parque cobre só o café; cenários sem base; o teste de ouro não pode rodar | comercial, com a diretoria |
| **D-P06** modelo de referência do termo de troca | **PENDENTE** | 5080EN a R$ 300 mil fixo (planilha); 3036N no café (CRM); "ART preço de trator" (conversa) | aba **Termo de troca** vazia | comercial |
| **D-P08** fonte oficial das vendas Tracbel | **PENDENTE** | entregas John Deere por ano fiscal (planilha); faturamento do **Protheus** (#18/#19); pedidos da **API GN** (#12) | **Captura**, **Oportunidade** e o bloco **Performance** vazios | diretoria — é decisão de acesso |
| **D-P09** venda financiada × SICOR | **PENDENTE** | o SICOR **não identifica cliente nem revenda**; a recomendação é aceitar a comparação por município e mês como **aproximação**, nunca contrato a contrato | share do crédito | diretoria |
| **D-P10** competência dos dados | **PENDENTE** | último ano completo de cada fonte. **[M 20/09]** o rótulo da planilha está adiantado: o que ela chama de área 2025 é a **PAM de 2024**. O banco já guarda **três anos**, então a escolha **não pede nova carga** | o ano escrito em cada número | diretoria |
| **D-P11** fontes e licenças de preço | **RESOLVIDA, menos o CEPEA** | **[M 21/09]** a CONAB publica o preço recebido em SP como dado aberto; a Socicana é página pública sem restrição no `robots.txt`. **O CEPEA é CC BY-NC 4.0 — a cláusula NC proíbe uso comercial** | só o CEPEA fica fora da coleta | **jurídico** — deixou de ser questão técnica |
| **D-P12** preço usado no potencial em R$ | **PENDENTE** | não existe preço por máquina no modelo; a recomendação é preço de referência **por categoria** × demanda | **Mercado anual** e **Oportunidade** em R$ | comercial |

### 1.2 As outras sete, para o registro ficar completo

| Decisão | Estado | Observação |
|---|---|---|
| **D-P02** índice de momento de preço | **PARCIAL** | 12 ÷ 12 está **implementado e semeado**; falta só **o nome da faixa entre 1,00 e 1,20** |
| **D-P03** índice de crédito | **PARCIAL** | 70/30 está **implementado e semeado**; em aberto: o **mínimo de linhas** (coluna existe, nula) e **se o trator entra** — a nota da planilha o exclui por endogeneidade, e **o CRM hoje o inclui** |
| **D-P04** percepção do gestor | **TRATADA COMO DECIDIDA**, sem marcação formal | ±5 pontos percentuais, por município; está no código e semeado |
| **D-P05** pesos, limites e cenários | **DECIDIDA em 23/09** | pesos da planilha como primeira vigência, com selo de estimativa |
| **D-P07** rentabilidade: custo operacional ou total | **PENDENTE** | a CONAB publica série de soja, milho, amendoim e laranja; os **locais de SP** ainda não foram conferidos dentro dos arquivos |
| **D-P13** propriedades por tamanho × clientes | **PENDENTE** | área por cliente está **0% preenchida** no CRM |
| **D-P14** base de municípios e CEN | **PENDENTE** | 203 municípios confirmados; falta qual planilha de CEN vale (#48) |

### 1.3 Três inconsistências no próprio registro [M]

Elas não mudam nenhum número, e mudam a confiança de quem lê o registro para decidir:

1. **O doc 48 §5 abre com "Nenhuma foi tomada"** — na mesma tabela em que **D-P05 está marcada como
   decidida**. A frase é de antes de 23/09 e não foi atualizada.
2. **D-P02, D-P03 e D-P04 estão no código e semeadas como vigência inicial**, e a tabela de decisão não as
   reconhece como decididas. Hoje o sistema tem escolhas que o registro não registra — que é exatamente o
   que a #63 existe para impedir.
3. **A forma do fator diverge entre dois documentos.** O doc 49 §9.4 recomenda a forma **aditiva**
   (`1 + w₁·(I₁−1) + w₂·(Crédito−1) + π`), justificada por "cada parcela vira uma linha do tooltip". O doc
   48 §7.4 registra a forma **implementada**, multiplicativa no crédito:
   `limite[(1 + a·z_preço + d·z_percepção) × (1 + b·z_crédito)]`. **Nenhum documento revoga a recomendação
   aditiva.** A implementada é a do protótipo; quem decidiu trocar não está escrito.

**Ação proposta [P]:** um PR de registro que corrige a frase, marca D-P02/D-P03/D-P04 pelo que o código já
faz, e anota a escolha da forma multiplicativa com a justificativa. **Não muda uma linha de cálculo.**

---

## 2. A matriz

Legenda pedida: **🟩 VERDE** já existe e é confiável · **🟨 AMARELO** existe, falta decisão ou integração ·
**🟥 VERMELHO** ainda não existe.

> **Aviso sobre o anexo 49A** [M]: ele é de **22/09** e já está **desatualizado em cerca de vinte linhas** —
> foi escrito antes de #72, #73, #74, #159, #160 e #161 serem entregues. Ele mesmo marca a calculadora como
> "⛔", e ela está no ar. **A matriz abaixo é a medida de hoje**, e substitui o placar do 49A.

### 2.1 As dez perguntas

| # | Métrica / pergunta | Status hoje | Fonte | Issue | Bloqueio | Ação | Resultado na tela |
|---|---|---|---|---|---|---|---|
| 1 | **Quanto mercado existe?** (parque estrutural) | 🟨 | PAM 5457 + `RegraDePotencial` | #72 ✔ feito; #63 | **D-P01** — há **uma** regra registrada (café, 10 ha, "a confirmar"). O parque é o do café, não o da região | registrar as 6 culturas × categoria no formulário que já existe | "Parque" passa a cobrir a região inteira em vez de uma cultura |
| 2 | **Quanto renova por ano?** (Demanda anual) | 🟨 | derivado — `parque ÷ ciclo` | #72 ✔; #63 | **D-P01** — `AnosDeRenovacao` é **anulável e está nulo**; o motor devolve vazio com o motivo | mesma vigência da linha 1 | **KPI Demanda anual** sai do travessão |
| 3 | **Quanto vale em R$?** (Mercado anual) | 🟥 | interno — nota do Protheus, ART ou tabela John Deere | **#70**; D-P12 | **não existe preço de máquina no modelo** — nenhuma tabela, nenhum parâmetro | decidir a fonte (D-P12) e integrar por **categoria**, com mediana mensal | **KPI Mercado anual** e a Oportunidade em R$ |
| 4 | **Aquecido ou retraído?** (Momento) | 🟨 | CONAB + Socicana + PTAX + SICOR | #73 ✔, #74 ✔, T3.1 ✔ | **parede de calendário [M]:** o índice é **12 ÷ 12** e a CONAB está no banco **desde 09/2025** → 24 meses só em **09/2027**. Os pesos **já estão semeados** (vigência de 23/09), então o fator sai — o que falta é série | **#198** — o preço implícito da PAM é anual e municipal **desde 2010** | o fator deixa de depender de esperar dois anos |
| 5 | **Por quê?** (composição do fator) | 🟩 | as três parcelas por cultura | #74 ✔ | nenhum. Ressalva: os pesos são "medidos no protótipo, a confirmar" e carregam **selo de estimativa** | — | aba "Composição do fator", uma linha por cultura |
| 6 | **Quanto a Tracbel captura?** | 🟥 | interno — Protheus (#18/#19) ou API GN (#12) | **#69**; D-P08; #162 | **vendas em unidades não existem**. O CRM tem faturamento **em R$, sem modelo** | decidir a fonte (D-P08) e carregar un. por município × categoria × mês | **KPI Captura** e a aba Captura da Performance |
| 7 | **Quanto ainda não captura?** (Oportunidade) | 🟥 | derivado | **#69**, #70, #162 | o mesmo da linha 6, mais o R$ da linha 3 | depois da #69 | **KPI Oportunidade** e a aba "Não capturado" |
| 8 | **Onde estão os municípios com maior oportunidade?** | 🟨 | mapa de potencial | #72 ✔; #69 | o mapa **já ranqueia por potencial**; "oportunidade" = potencial − vendas, e as vendas em un. faltam | usar potencial como ranking enquanto a #69 não chega, **rotulado como potencial** | o mapa C já responde metade da pergunta hoje |
| 9 | **Quais culturas explicam?** | 🟩 estrutura · 🟨 oportunidade | PAM + catálogo | #151/#165 ✔, #160 ✔ | a decomposição por cultura existe no parque e no fator; na **oportunidade** depende da #69 | — | "Potencial por cultura" e "Composição do fator" |
| 10 | **Cenários conservador / moderado / otimista** | 🟨 | motor | #74 ✔, #161 ✔ | os cenários existem **na calculadora**; a **matriz do recorte** é a fase T5 e as bandas são **D-IM-05** | decidir D-IM-05 e implementar T5 | aba "Cenários" do Potencial estrutural |

### 2.2 As linhas de apoio, que a diretoria não vê e sem as quais nada se prova

| Métrica | Status | Issue | Bloqueio | Ação | Resultado |
|---|---|---|---|---|---|
| Parâmetros do motor com vigência | 🟨 | **#166** | **estrutura sem bloqueio**. Já são colunas: carência do SICOR, mínimo de linhas, pesos, limites do fator, bandas de porte. **Faltam colunas**: suavização do crédito, base do poder de compra, N safras da rentabilidade, bandas de cenário (D-IM-05), janela e tolerância da renovação (D-IM-10) | migration aditiva + formulário | toda decisão pendente vira **registrável**, e nenhuma ganha default escondido |
| Teste de ouro do motor | 🟥 | **#171** | os números são conhecidos — **30.317 de parque e 3.457 por ano** (doc 48 §3.2) —, mas "só poderão ser reproduzidos quando alguém registrar os parâmetros que os geraram" | escrever o teste com os parâmetros da planilha **como fixture**, sem depender do banco | o motor provado contra número conhecido |
| Procedência por indicador | 🟨 | #167 | o contrato já devolve `procedencias`; nem todo número tem o seu | completar a cobertura | "de onde veio, de quando, medido ou estimado" em todo número |
| Rodada do motor gravada | 🟥 | **#170 parte B** | não existe `RodadaDoMotor`; p95 abaixo de 500 ms não foi medido | tabela de resultado + medição no servidor | histórico do que foi mostrado, e desempenho provado |
| Resíduo fictício no pacote | 🟥 | **#169** | **nenhum.** `public/dados/mercado-pracas.json` tem 4 praças fictícias de MT, GO e BA e continua no pacote publicado | apagar arquivo, tipo e entrada do manifesto | nenhum número inventado sai da fábrica |
| Contratos da API | 🟨 | #75 | as rotas existem; falta OpenAPI com exemplos e a permissão por perfil | completar | 403 sem permissão; contrato documentado |
| Preço implícito da PAM | 🟥 | **#198** | **nenhum** — o dado está no banco desde 2010 | leitura derivada, **sem gravar coluna nova** | médias de 3 e 5 anos, e preço **por município**, que a CONAB não tem |
| CEPEA | 🟥 | #117 | **licença CC BY-NC** — uso comercial exige contrato | **não implementar** antes de D-P11 | fora da apresentação, e dito |
| Idade do parque | 🟥 | **#164** | D-IM-10 e o preenchimento de `frota.Equipamento`, **que não foi medido** | medir o preenchimento primeiro | transforma potencial em ação sobre a carteira |

### 2.3 Cinco fatos do código que mudam o tamanho do trabalho [M]

Medidos em 23/09/2026, com arquivo e linha. Eles não aparecem em documento nenhum e mudam a estimativa:

1. **`mercadoAnual`, `capturaTracbel` e `oportunidade` não existem em DTO nenhum do backend.** Na tela eles
   são `valor={null}` **fixo no TypeScript** (`KpisExecutivos.tsx:109,116,123`), e os três motivos são
   **constantes do front**, não vêm em `metricasSemDado`. Ou seja: quando a #69 e a #70 chegarem, **o
   contrato precisa de campos novos** — não é só ligar um fio.
2. **A `Demanda anual` já é calculada** (`MotorDoPotencial.cs:203`) e sai nula por um motivo de uma linha:
   a **única** regra semeada no banco é o café com `AnosDeRenovacao = null`
   (`ParametrosDoPotencialConfiguracao.cs:86-106`; é a única linha de `HasData` de `RegraDePotencial`).
   **Registrar a vigência do D-P01 acende o KPI sem tocar em uma linha de cálculo.**
3. **`frota.VendaDeMaquina` já existe, com `Quantidade`** (`VendaDeMaquina.cs:115`) — e está **desligada**:
   não tem município, nenhum repositório de território a lê, e por decisão **não guarda valor**
   (`:58-60`). A #69 tem onde aterrissar; falta a fonte, o município e a ligação.
4. **Os pesos do fator já estão semeados** (vigência Id=2 de 23/09, `ParametrosDoPotencialConfiguracao.cs:229-233`).
   `MotivoSemFator.SemPesos` **não dispara mais**. O que ainda sai nulo é o **porte**: as colunas
   `PorteMedioAPartirDe` e `PorteGrandeAPartirDe` existem desde 23/09 e estão **vazias**.
5. **Não existe schema `mercado`.** Tudo do domínio de mercado está em `organizacao`. A **D-IM-08 foi
   decidida na prática, por omissão** — e o registro diz que ela está aberta e recomenda `mercado`.

### 2.4 O placar

| | Perguntas da diretoria | Linhas de apoio |
|---|---|---|
| 🟩 VERDE | 1 (mais metade da 9) | 0 |
| 🟨 AMARELO | 5 | 4 |
| 🟥 VERMELHO | 3 | 5 |

**As três vermelhas são as perguntas 3, 6 e 7** — e as três são o mesmo bloqueio de fundo: **dado interno da
Tracbel que não está no servidor**. Nenhuma delas se resolve com engenharia sobre fonte pública.

---

## 3. A sequência [D]

**Ordem decidida pelo Ricardo em 23/09/2026:**

```
#63 → #69 → #70 → #162 → #166 → cenários → #170B / #171
```

O critério é o certo, e é o de impacto: **#63, #69 e #70 são a maior transformação da tela**. Hoje o topo diz

```
Demanda        —        Mercado anual  —
Captura        —        Oportunidade   —
```

e com os três resolvidos ele passa a dizer quatro números — e aí os mapas, a rentabilidade, o crédito, os
cenários e o Território deixam de ser dados soltos e viram **a explicação desses quatro números**. É a
diferença entre um painel com muita informação e uma ferramenta para decidir onde atacar.

### 3.1 A emenda: o #63 tem um pré-requisito de código, e ele é pequeno

**Hoje a decisão D-P01 não pode ser registrada nem que o comercial a tome nesta tarde** [M]:

> a rota de cadastro da regra **não aceita categoria de máquina** (D-IM-06; doc 48 §7.1) — regra sem
> categoria cai num grupo "Sem categoria declarada".

E a D-P01 é *cultura × **categoria** × ha/máquina × renovação*. Sem a coluna, a reunião produz uma decisão
que o sistema não sabe guardar, e ela vira ata em vez de vigência.

**Isso não é uma fase antes do #63 — é a primeira entrega DO #63**, e vem antes da reunião, não depois.
São dois campos que já existem na entidade (`RegraDePotencial.CulturaId` e `.CategoriaDeMaquinaId`, ambos
anuláveis desde a #165) e que faltam na rota e no formulário.

### 3.2 O que cada etapa acende

| Etapa | Tipo | O que acende na tela |
|---|---|---|
| **#63** (pré-requisito + D-P01) | código pequeno **+ decisão do comercial** | **Demanda anual**; o parque passa a cobrir a região em vez de só o café; o teste de ouro do #171 passa a poder rodar |
| **#69** (D-P08 + integração) | **decisão da diretoria** + dado interno | **Captura**, **Oportunidade em unidades**, e o bloco **Performance** inteiro |
| **#70** (D-P12 + integração) | **decisão** + dado interno | **Mercado anual em R$**, Oportunidade em R$, aba **Termo de troca** |
| **#162** | código | confiança **alta / média / baixa** em toda oportunidade; captura por ano civil, com o fiscal ao lado |
| **#166** | código + as decisões que faltam | os parâmetros nulos restantes viram registráveis; **o porte ganha nome** |
| **cenários** (T5, D-IM-05) | código + decisão | a matriz conservador / moderado / otimista do recorte |
| **#170B / #171** | código | a rodada gravada, o p95 medido e o motor provado contra **30.317 / 3.457** |

### 3.3 Onde cada um trava, em uma linha

- **#63** — comercial, com a diretoria. É o gargalo dos dois primeiros KPIs.
- **#69** — diretoria: é **decisão de acesso** antes de ser integração. Três candidatas: Protheus (#18/#19),
  API GN (#12), entregas John Deere.
- **#70** — comercial (D-P12) e a mesma fonte interna.
- **CEPEA** — jurídico. **Não bloqueia a apresentação** e não deve atrasá-la.

### 3.4 O que eu consigo fazer enquanto as decisões não saem

Três coisas, nenhuma dependendo de você, todas na ordem acima ou preparando-a:

1. **O pré-requisito do #63** — categoria e cultura na rota e no formulário da regra.
2. **#169** — apagar as quatro praças fictícias de MT, GO e BA que ainda saem no pacote publicado. Sem
   bloqueio nenhum, e é dado inventado em produção.
3. **#171** — o teste de ouro com os parâmetros da planilha **como fixture**, que não depende do banco nem
   da VPN: ele prova que o motor reproduz 30.317 de parque e 3.457 por ano.

---

## 4. O que não fazer

- **Não** completar categoria incompleta em silêncio. Se uma categoria ficar sem preço, o mercado anual dela
  sai vazio e **o total do município fica rotulado como parcial** — nunca somado como se estivesse completo.
- **Não** chamar captura de *market share*. Enquanto o denominador for demanda estimada, o nome é captura.
- **Não** chamar o sinal de baixa confiança de "venda perdida". Crédito do SICOR sem venda Tracbel
  correspondente é **indício**, e o SICOR não identifica revenda (D-P09).
- **Não** emendar a série anual da PAM na mensal da CONAB. Mesma família, não intercambiáveis.
- **Não** inventar custo municipal: a CONAB publica **localidade de referência**, e a tela diz qual é.
- **Não** dar default escondido a parâmetro nulo. Nulo → vazio com o motivo, e o parâmetro entra em
  `pendencias`.
- **Não** implementar o CEPEA antes da licença.
