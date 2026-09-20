# Visão 360 — os cinco cartões e os três mapas, com fonte, regra e conferência

> **Documento 36** · Versão 1.0 · 14/09/2026 — ajuste da Visão 360 e dos Indicadores Geográficos da ADR
> pelas duas referências recebidas (`visao360-mapas-adr-v8.png`, a organização dos três mapas, e
> `imagem (5).png`, as anotações em vermelho sobre os cinco cartões).
> Convenções: **[medido]** = obtido em 14/09/2026 por consulta somente leitura; **[regra]** = regra
> implementada; **[pendente]** = decisão ou dado que falta.
>
> **Este documento não tem nome de pessoa nem valor financeiro do ART.** Os nomes que aparecem nas
> capturas vêm do banco e ficam nas capturas, em `dados-locais/` (fora do Git). As anotações e os
> números das imagens de referência **não** foram copiados como dado: a pendência P-21 do documento 32
> (faturamento da empresa no repositório) vale também para este documento.

---

## 0. Resumo

1. **Os cinco cartões passaram a ter uma rota própria**, `GET /api/v1/relatorios/indicadores-executivos`,
   lida filial a filial e somada por **partição** — faturamento pela filial que emitiu, vínculo pela
   filial da carteira, cliente único pela filial de cadastro, meta e venda perdida pela filial dona.
   Nenhum número se soma duas vezes, e o percentual é refeito a partir do numerador e do denominador.
2. **Três números mudaram porque estavam errados na tela**, e agora batem, ao centavo e à unidade,
   com consulta SQL independente [medido]:

   | Cartão | Antes | Depois | Por que mudou |
   |---|---|---|---|
   | Faturamento em curso | R$ 8,3 M | **R$ 10,7 M** (R$ 10.652.619,09) | a nota sem cliente no CRM (R$ 2.396.023,67) ficava fora |
   | Previsão FY 2026 → **Meta e realizado · 2026** | "—, depende do faturamento" | **R$ 569,1 M realizado**; meta: nenhuma cadastrada; previsão: sem modelo | não havia previsão nem meta; "FY" sem calendário fiscal confirmado |
   | Clientes na carteira | 49.109 | **21.001** clientes únicos | 49.109 eram vínculos; um cliente em três carteiras contava três vezes |
   | Cobertura ativa (30 dias) → **Cobertura pela cadência** | 24% (11.974 de 49.109) | **66,6%** (14.503 de 21.774 elegíveis) | 30 dias fixos sobre todos os vínculos; agora a cadência declarada, a mesma regra do mapa |
   | Conhecimento de mercado | "—, emplacamento não integrado" | **165** vendas perdidas registradas, sem percentual | o CRM tem o formulário de venda perdida; participação de mercado continua sem dado |

3. **Os três mapas ficaram lado a lado, maiores e comparáveis.** O quadro é a ADR inteira e é o
   mesmo nos três, e o cursor sobre um município mostra o número dele nos três. A cobertura ganhou a
   escala vermelho→verde, com pendência e quantidade como alternativas. As vendas ganharam o período
   explícito ("12 meses fechados" ou "ano civil até o último mês fechado"), e o potencial ganhou o
   alternador Região total / Clientes / Não clientes, com o motivo de cada opção desligada. A ficha do
   município passou a trazer os **responsáveis reais das carteiras**, pelos vínculos.
4. **Achados que mudam a leitura da cobertura** [medido]: nenhum dos 179 tipos de atividade está
   marcado como visita; 30.488 das 122.002 interações são registros gerados pelo sistema e contam como
   contato; e a classe A/B/C/D do vínculo veio "C" por padrão em 47 mil dos 49 mil vínculos — por isso
   a regra usa a classe do cadastro do cliente (§3.4).
5. **Testes:** 484 testes .NET passando (9 novos da rota dos cartões e 1 novo do território); `tsc -b`
   sem erro; `oxlint` sem erro nos arquivos alterados. Um teste de domínio pré-existente, com data
   fixa, começou a falhar hoje e foi corrigido (§6).
6. **Não houve commit, publicação nem reescrita de histórico.** Nenhuma gravação em banco de origem nem
   no banco do servidor; o ART foi lido só com agregados.

---

## 1. As duas referências, orientação por orientação

As anotações em vermelho **não são elemento de tela**: são pedidos. Nenhum número, percentual ou nome das
imagens foi usado como dado.

### 1.1 `imagem (5).png` — os cinco cartões

| # | Orientação | O que foi feito | Onde | Situação |
|---|---|---|---|---|
| A1 | Faturamento: ART ou Protheus — escolher e documentar | Fonte: **nota fiscal de saída do Protheus**. O ART registra venda de máquina, não nota, e não é somado (§3.1) | rota nova; cartão A | feito |
| A2 | Conciliar, não somar equivalentes | Protheus × ART comparados mês a mês: não são a mesma medida (razão de 0,75 a 4,4); nenhuma soma entre eles | §3.1 | feito |
| A3 | Período efetivo, escopo e hora da atualização | "set/2026 até a carga de 08/09, 15:46 · 13 filiais" no próprio cartão | cartão A | feito |
| A4 | Cancelamento e devolução pela regra confirmada | Não há regra confirmada (P-5): o cartão diz "devolução não abatida" | cartão A | **pendente P-5** |
| A5 | Consultar os registros que compõem | Composição filial a filial abaixo dos cartões; natureza da nota sem cliente e grupo de item na dica do cartão | `ComposicaoDosIndicadores` | feito |
| B1 | Previsão FY: ART ou arquivo de meta — verificar | ART: só vendas de máquina, sem meta. `organizacao.Meta`: **0 linhas**. Arquivos: nenhuma meta real — só a planilha-modelo de coleta, com exemplos fictícios, as constantes ilustrativas do protótipo e a tabela de metas do Vórtice com 1 linha (§3.2) | §3.2 | medido |
| B2 | Distinguir meta, previsão e realizado; corrigir o título | Título "Meta e realizado · ano"; realizado medido; meta "nenhuma cadastrada"; previsão "sem modelo aprovado" | cartão B | feito |
| B3 | Sem extrapolação; dependência específica | Nenhuma projeção; o motivo diz exatamente o que falta (meta cadastrada, modelo de previsão) | cartão B | feito |
| B4 | O ano segue o período escolhido, não fica em 2026 | Seletor "Ano de referência" (2020 até o ano corrente); a API recebe `ano` | cartão B | feito |
| C1 | Identidade, duplicidade, situação cadastral, vínculos | Clientes únicos × vínculos; situação (cliente/prospect/suspect); sem CPF/CNPJ; duplicidades medidas (§3.3) | cartão C | feito |
| C2 | Correções com originais preservados | **Nenhuma correção aplicada nesta rodada**: fundir cadastro duplicado exige regra que o negócio não confirmou. Os casos estão medidos (§3.3) | — | **pendente P-27** |
| C3 | Únicos × vínculos, sem dupla contagem, respeitando o escopo | Partição pela filial de cadastro: soma das 13 filiais = 21.001 = contagem distinta global | rota nova | feito |
| D1 | Quais atividades contam como visita | Medido: **nenhuma** (0 de 179 tipos marcados). O cartão diz "contato registrado, não visita" | §3.4 | **pendente P-2** |
| D2 | Carteira elegível, período, denominador; numerador e denominador rastreáveis | Elegível = vínculo em carteira comercial de linha com cadência; denominador e numerador no cartão e na composição | cartão D | feito |
| D3 | Sem 30 dias fixos; coerente com o mapa | Cadência declarada da linha pela classe ABC do cliente — a mesma regra, no mesmo código, do mapa de cobertura | §3.4 | feito |
| E1 | Investigar ART, venda perdida e dado externo | ART: vendas de máquina (sem mercado total); venda perdida: 165 formulários; emplacamento: não existe fonte | §3.5 | medido |
| E2 | Definir o que o cartão mede; sem percentual inventado | "Vendas perdidas registradas", com concorrente e período; "participação de mercado: sem dado" | cartão E | feito |
| E3 | Relacionar produto, modelo, município, período e concorrente | Concorrente, modelo e período no cartão e no painel "Conhecimento de mercado"; município pelo cliente da perda **não implementado** | §3.5 | **pendente P-29** |

### 1.2 `visao360-mapas-adr-v8.png` — os três mapas

| Orientação | O que foi feito | Situação |
|---|---|---|
| Três mapas lado a lado, responsivos | Grade de três colunas; uma coluna abaixo de 1.100 px | feito |
| Mapas maiores; enquadramento comparável | Quadro recortado na ADR, o mesmo nos três e estável ao filtrar região (viewBox 520 × 368) | feito |
| Cobertura verde→vermelho, unidade explícita, cobertura × pendência | Modos "% no prazo" (vermelho→verde), "% pendente" e "pendentes (qtd.)", cada um com a unidade escrita na legenda | feito |
| Elegíveis, visitados e pendentes visíveis | Linha de números no topo do mapa e no detalhe do município (elegíveis, no prazo, fora do prazo, nunca) | feito — "visitados" lido como "com contato no prazo" (P-2) |
| Vendas em azul; totais, detalhe e filtros conciliados | Linha de números no topo; total da ADR na tabela = total da tela = SQL (§4.2) | feito |
| FYTD × 12 meses; calendário fiscal | "12 meses fechados" (padrão) e "ano civil até o último mês fechado"; FYTD não oferecido até confirmar o calendário | **pendente P-4** |
| Máquina × pós-venda | Alternador Total / Máquina / Pós-venda; "Total = máquina + peça + serviço + outros" na nota | feito — composição de pós-venda provisória |
| Potencial em roxo; clientes × região total (e não clientes) | Alternador Região total / Clientes / Não clientes; os dois últimos desligados, com o motivo medido | feito |
| Potencial sem dupla contagem e com limites | Nota: a área é do município inteiro; somar clientes e não clientes a ela contaria a mesma área duas vezes | feito |
| Não copiar R$ 1,24 bi nem 21% | Nenhum valor da referência na tela | feito |
| Geometria real do IBGE; ADR em destaque | 645 polígonos da malha oficial; contorno verde nos 203 da ADR | feito |
| Fora da ADR × sem dado × zero | Cinza claro, hachurado e primeira faixa da escala, iguais nos três mapas | feito |
| Detalhe ao passar e ao selecionar | Cursor: contorno tracejado e o número do município nos três mapas; clique: ficha completa | feito |
| Legendas legíveis | Unidade em linha própria; faixas, sem dado, fora da ADR e contorno da ADR | feito |
| Validar Norte e Noroeste | Norte 83, Noroeste 120, juntos 203 — tela = API = SQL | feito |
| Responsáveis reais, sem nomes da imagem | Ficha do município: responsáveis das carteiras com vínculo ali, pelo cadastro do CRM | feito |
| Filtros: filial, município, CEN/gestor, período, SAM/KAM/Varejo, produto e modelo | Mantidos; município pela seleção no mapa e na tabela; CEN/gestor, SAM/KAM/Varejo, produto e modelo desligados com o motivo | feito — os desligados dependem de P-1, P-6 e P-7 |

---

## 2. Qual banco a aplicação acessou nesta validação [medido]

| Camada | O que foi usado |
|---|---|
| API | local, `http://localhost:5145`, ambiente `Development` |
| Banco | `TracbelCrmServidor` no contêiner local `tracbel-crm-ensaio` (SQL Server 2025, porta 14334) — **cópia `COPY_ONLY` do banco do servidor** feita em 14/09/2026 (documento 35, §3). Não é o banco do servidor: nenhuma leitura nem gravação foi feita lá nesta rodada |
| Faturamento | a carga do Protheus gravou a competência mais recente (09/2026) em 08/09/2026 18:46 UTC |
| Front | Vite local, `http://localhost:5199` |
| Identidade | a ponte provisória por cabeçalho (usuário de desenvolvimento, filial 010101); ela não autentica (dívida D-1 do doc 23 e **P-20** — o defeito D-11 do documento 32, que fora dele se cita por P-20 para não colidir com a decisão D-11 da semente) |
| ART | leitura só de agregados, pela credencial do `.env` da raiz (documento 35, §7); nenhum valor financeiro registrado aqui |
| Protheus | não consultado nesta rodada: o faturamento vem do banco do projeto, carregado da SD2 |

**Consequência:** os números deste documento valem para a cópia de 14/09/2026. No servidor os valores
serão os do banco de lá, e a tela nova só aparece lá depois de publicada (P-30).

---

## 3. Cartão a cartão

Todos os cartões saem da mesma rota; a tabela de cada um segue o caminho
**fonte → tabela → campo → transformação → indicador → componente**.

### 3.1 A — Faturamento em curso

| Etapa | Conteúdo |
|---|---|
| Fonte | Protheus, SD2 (nota fiscal de saída de venda), pela carga do projeto (documento 30) |
| Tabelas | `comercial.FaturamentoDoCliente` (nota com cliente no CRM) e `comercial.FaturamentoSemCliente` (sem cliente, por natureza da contraparte) |
| Campos | `Competencia`, `ValorLiquido`, `ValorEmMaquina`, `ValorEmPeca`, `ValorEmServico`, `ValorEmOutros`, `Notas`, `Natureza`, `CriadoEm`/`AlteradoEm` |
| Transformação [regra] | competência mais recente carregada até o mês corrente; soma com cliente + sem cliente; natureza Cliente não cadastrado, Sem documento e Indefinida = "contraparte sem cadastro"; hora da carga = maior `AlteradoEm`/`CriadoEm` da competência; no consolidado, filial em outra competência fica fora da soma e nomeada |
| Indicador | total emitido na competência, com a abertura com cliente × sem cliente e máquina × peça × serviço × outros |
| Componente | `PainelExecutivo.tsx` → `CincoIndicadores`, cartão A; composição em `ComposicaoDosIndicadores` |

**Conferência [medido]** — competência 09/2026, 13 filiais ativas:

| Parcela | API (soma das 13) | SQL independente |
|---|---:|---:|
| Com cliente no CRM | 8.256.595,42 | 8.256.595,42 |
| Sem cliente no CRM | 2.396.023,67 | 2.396.023,67 |
| **Total** | **10.652.619,09** | **10.652.619,09** |

Sem cliente, por natureza: cliente não cadastrado 2.057.129,41; fábrica 322.597,87; empresa do grupo
16.296,39. As filiais inativas (Guaíra, Ituverava, Monte Alto) não entram na visão por filial — a
diferença entre "13 ativas" e "todas as filiais" está no documento 32, §8.6.

**Por que não o ART.** A view `bi_art_veiculos` registra a venda de máquina com a data de faturamento,
mas o valor dela não é a nota do Protheus. Nos 12 meses fechados (09/2025 a 08/2026), a razão entre a
soma do ART e o valor de máquina das notas do Protheus foi de **0,75 a 4,4**: em 5 meses o ART ficou
abaixo, em 7 acima, e em dois meses passou de três vezes [medido, só a razão]. Os dois medem coisas
diferentes (venda registrada pelo comercial × nota emitida pela filial); somar contaria a mesma máquina
duas vezes, e escolher um no lugar do outro mudaria o que "faturamento" quer dizer. O ART fica como
fonte de venda de máquina por chassi, produto e comprador (documento 35, §7.7).

**Mês em curso.** Hoje (14/09) o mês de setembro está aberto e a carga mais recente é de 08/09: o
cartão diz "set/2026 até a carga de 08/09" e não compara com agosto cheio.

### 3.2 B — Meta e realizado

| Etapa | Conteúdo |
|---|---|
| Fonte | as mesmas notas do cartão A; meta de `organizacao.Meta` |
| Campos | `Competencia`, `ValorLiquido`; `Meta.Tipo`, `PeriodoInicio`, `PeriodoFim`, `Alvo`, `CarteiraId`, `UsuarioId`, `LinhaDeNegocioId`, `EstaAtiva` |
| Transformação [regra] | realizado = soma das notas do **ano civil** pedido; meta = soma das metas de faturamento **da filial** (sem carteira, usuário nem linha) com o período inteiro dentro do ano; meta de carteira/usuário/linha é contada e não somada; meta que cruza o ano é contada e não rateada; % só entre filiais com meta |
| Indicador | realizado do ano; meta quando existe; nunca previsão |
| Componente | cartão B e seletor "Ano de referência" |

**Conferência [medido]:** realizado 2026 (jan a set, setembro em curso) = R$ 569.092.026,32 na API e no
SQL. `organizacao.Meta` tem **0 linhas**, e o ART não traz meta. Os arquivos com "meta" no nome não são
meta real:

| Arquivo | O que é |
|---|---|
| `dados-locais/coleta/modelos/04-metas.xlsx` | a planilha-modelo de coleta (documento 34): colunas ano fiscal, início do ano fiscal, filial, linha, responsável, mês, tipo e alvo, **só com exemplos fictícios** — é o caminho para cadastrar a meta (P-26) e, de quebra, o calendário fiscal (P-4) |
| `prototipo/dados-seed/config-metas.json`, `meta-frequencia.json` (e as cópias em `public/dados/`) | constantes ilustrativas do protótipo (documento 17, §7); não são lidas pelos cartões |
| `docs/extracao-vortice/catalogos-bpm/IVS_UsrMeta.csv` | a tabela de metas por usuário do Vórtice: **1 linha** em produção (documento 17) |
| `docs/extracao-vortice/_raw/modulos-meta.csv`, `tabelas-meta.csv` | metadados do esquema do Vórtice, não metas |

**Por que o título mudou.** "Previsão FY 2026" prometia três coisas que não existem: previsão (nenhum
modelo aprovado), ano fiscal (calendário não confirmado, P-4) e o ano fixo. O cartão agora diz o que
mede e o que falta.

### 3.3 C — Clientes na carteira

| Etapa | Conteúdo |
|---|---|
| Fonte | CRM (carga do Vórtice e cadastros do projeto) |
| Tabelas | `comercial.Cliente`, `comercial.ClienteCarteira`, `organizacao.Carteira` |
| Campos | `Cliente.EmpresaId`, `Situacao`, `Documento`, `ExcluidoEm`; `ClienteCarteira.DesvinculadoEm`, `CarteiraId`; `Carteira.Natureza` |
| Transformação [regra] | cliente único = cliente não excluído com vínculo ativo em qualquer carteira, **contado na filial de cadastro**; vínculos e carteiras, na filial da carteira; "nas carteiras da filial" existe na API e é declarada não somável |
| Indicador | clientes únicos; situação cadastral; sem CPF/CNPJ; vínculos comerciais e carteiras comerciais |
| Componente | cartão C e composição |

**Conferência [medido]:**

| Medida | API (soma das 13) | SQL independente |
|---|---:|---:|
| Clientes únicos | 21.001 | 21.001 (distinto global, sem partição) |
| Cliente / Prospect / Suspect | 6.119 / 14.469 / 413 | 6.119 / 14.469 / 413 |
| Sem CPF/CNPJ | 7.391 | 7.391 |
| Vínculos (todas as naturezas) | 49.109 | 49.109 |
| Vínculos comerciais / carteiras comerciais | 48.959 / 131 | 48.959 / 131 |

**A dupla contagem que existia:** somar "clientes distintos nas carteiras" filial a filial daria mais de
40 mil, porque 26.908 vínculos ligam cliente de uma filial a carteira de outra. A partição pela filial de
cadastro fecha com a contagem distinta global.

**Sanitização — o que foi medido e não corrigido:**

- 4 documentos (CPF/CNPJ) aparecem em 8 cadastros ativos, **todos em filiais diferentes** — duplicidade
  entre filiais, invisível na visão de uma filial só;
- 7.391 dos 21.001 clientes em carteira não têm CPF/CNPJ e não se conferem com a SA1 do Protheus;
- 387 CNPJs que faturam existem no CRM só por nome (documento 32, P-25).

Nenhum desses casos foi alterado: fundir cadastro, escolher o sobrevivente ou completar documento exige
regra do negócio (P-27). Os registros originais do Vórtice e do Protheus seguem intocados.

### 3.4 D — Cobertura pela cadência

| Etapa | Conteúdo |
|---|---|
| Fonte | interações do CRM (carga do Vórtice) |
| Tabelas | `comercial.ClienteCarteira`, `organizacao.Carteira`, `organizacao.LinhaDeNegocio`, `comercial.Cliente`, `processo.TipoTarefa` |
| Campos | `ClienteCarteira.UltimaInteracaoEm`; `LinhaDeNegocio.DiasCicloClasseA..D`; `Cliente.Classe`; `TipoTarefa.ContaParaCobertura` |
| Transformação [regra] | vínculo ativo em carteira comercial; cadência da linha pela classe ABC **do cadastro do cliente** (sem classe = D); linha sem cadência sai do denominador; coberto = último contato dentro do prazo; pendente = fora do prazo + nunca contatado |
| Indicador | cobertos ÷ elegíveis; pendentes; vínculos sem cadência à parte |
| Componente | cartão D, rosca "Status da cobertura", alerta de nunca contatados; e o mapa de cobertura (mesma regra em `RepositorioDeIndicadoresTerritoriais`) |

Cadências declaradas [medido]: Venda de Máquinas e Implementos 180/180/180/360 dias (A/B/C/D);
Prospecção de Máquinas e Implementos 120/120/120/180; Venda de Peças e Venda de AMS 360; as demais 11
linhas sem cadência.

**Conferência [medido]:**

| Medida | API (soma das 13) | SQL independente (regra de alcance por filial) |
|---|---:|---:|
| Vínculos comerciais | 48.959 | 48.959 |
| Elegíveis | 21.774 | 21.774 |
| No prazo | 14.503 | 14.503 |
| Fora do prazo | 44 | 44 |
| Nunca contatados | 7.227 | 7.227 |
| Em linha sem cadência | 27.185 | 27.185 |

**Achados que mudam a leitura [medido]:**

1. **Nenhuma atividade conta como visita.** O campo `TipoTarefa.ContaParaCobertura` existe e está
   falso nos 179 tipos. A cobertura, então, é de **contato registrado**, e o cartão diz isso.
2. **Registro do sistema conta como contato.** 30.488 das 122.002 interações são de natureza Sistema
   (21.545 delas "Monitorar Cliente"). Com elas, 39.441 vínculos comerciais têm algum contato; só com
   as de natureza Ativa ou Receptiva, 24.318. A regra não foi trocada por suposição: é a decisão P-2,
   agora com o impacto medido.
3. **A classe do vínculo não serve.** 47.050 dos 49.109 vínculos vieram com classe C, padrão do legado;
   a classe real é a da curva ABC, no cadastro do cliente — é a que a regra usa.
4. **Cliente de outra filial.** Na visão por filial, a classe de um cliente cadastrado em outra filial
   não está ao alcance e conta como D: 501 vínculos com classe A/B/C estão nessa situação. Medido pela
   regra global, seriam 14.465 no prazo e 82 fora; pela regra por filial, 14.503 e 44 (P-28).
5. **O cartão anterior** contava contato em 30 dias sobre todos os vínculos (24%). O prazo de 30 dias
   não existe em nenhuma cadência declarada.

### 3.5 E — Conhecimento de mercado

| Etapa | Conteúdo |
|---|---|
| Fonte | formulário de venda perdida do CRM (`IV_Q_VENDA_PERDIDA_FY25` na origem) |
| Tabela | `processo.VendaPerdida` |
| Campos | `ConcorrenteId`, `ModeloDoConcorrente`, `PrecoDoConcorrente`, `PrecoOfertado`, `Quantidade`, `OcorridaEm`, `RegistradaEm` |
| Transformação [regra] | contagem por filial; data da perda, ou do registro quando falta; concorrentes distintos pela união dos códigos do relatório de vendas perdidas, só quando as duas leituras estão completas |
| Indicador | vendas perdidas registradas, com concorrente, período; **sem percentual** |
| Componente | cartão E e o painel "Conhecimento de mercado" (para quem se perdeu) |

**Conferência [medido]:** 165 registradas (API = SQL), 163 com concorrente, 163 com modelo do
concorrente, 156 com os dois preços, 22 concorrentes, 7 tipos de equipamento, 234 máquinas, de
12/11/2025 a 03/09/2026. Processos marcados como perdidos: 872 — 707 sem formulário.

**O que não existe:** emplacamento, mercado total ou participação. O ART tem 4.144 vendas de máquina
de 2024 a 2026, mas só as da Tracbel; sozinho não diz o tamanho do mercado. Relacionar a perda ao
município é possível pelo cliente do formulário (162 de 165 têm cliente) e não foi implementado (P-29).

---

## 4. Os três mapas

### 4.1 O que mudou

| Item | Antes | Depois |
|---|---|---|
| Quadro | o estado inteiro; a ADR num canto | a ADR inteira, o mesmo quadro nos três mapas, estável ao filtrar a região |
| Cobertura | só pendência, em vermelho | "% no prazo" (vermelho→verde), "% pendente" e "pendentes (qtd.)"; unidade escrita na legenda |
| Números de cada mapa | só nos cartões do topo | uma linha de números no topo de cada mapa |
| Detalhe | título nativo do navegador | cursor: contorno tracejado e o número do município nos três mapas; clique: ficha |
| Período das vendas | campos de mês | "12 meses fechados" · "ano civil até o último mês fechado" · personalizado; aviso de FYTD |
| Potencial | Região total / Clientes (desligado) | Região total / Clientes / Não clientes, com o motivo medido |
| Responsáveis | as duas planilhas | as duas planilhas **e** os responsáveis das carteiras com vínculo no município |

A regra da cobertura no mapa é a do cartão D (§3.4), no mesmo repositório de sempre; os estados fora da
ADR, sem dado e zero continuam iguais nos três mapas.

### 4.2 Conferência dos mapas [medido]

Visão da filial 010101, 12 meses fechados (09/2025 a 08/2026), toda a ADR:

| Medida | Tela | API | SQL independente |
|---|---:|---:|---:|
| Municípios da ADR (polígonos com contorno) | 203 em cada mapa | 203 | 203 |
| Norte / Noroeste | 83 / 120 | 83 / 120 | 83 / 120 |
| Elegíveis / no prazo | 2.566 / 1.688 (65,8%) | 2.566 / 1.688 | 2.566 / 1.688 |
| Vendas | R$ 77,1 mi | 77.060.969 | 77.060.969,36 |
| Máquina / pós-venda | R$ 30,3 mi / R$ 45,6 mi | — | 30.295.187,97 / 45.609.206,23 |
| Vendas, ano civil (01 a 08/2026) | — | 45.346.861 | 45.346.860,86 |
| Potencial | 9.932 teóricos · 99.322 ha em 203 municípios | — | 9.932,2 · 99.322,0 ha · 203 |

Mudar o período mexe só nas vendas: com o ano civil, elegíveis, no prazo e potencial ficaram iguais.

### 4.3 Responsáveis pelas carteiras

Para cada município, a API devolve `responsaveisPelasCarteiras`: o responsável cadastrado de cada
carteira comercial com cliente de endereço ali, com o número de vínculos e de carteiras, ordenado por
vínculos. O nome sai do cadastro de usuário, pelo mesmo filtro de alcance. É uma terceira fonte, ao lado
das duas planilhas, e nenhuma substitui a outra; o filtro "CEN / gestor" continua desligado até a decisão
de qual fonte vale (P-1).

---

## 5. Estados de tela e perfis

Os estados que o banco real não produz sob demanda foram simulados **no navegador**, interceptando a
resposta da API (`scripts/prototipo/capturar-estados-visao360.mjs`) — nada foi alterado em banco.

| Estado | Resultado na tela |
|---|---|
| Carregando | "Carregando os cinco indicadores das filiais…" — nenhum zero |
| Sem permissão (403 em todas as filiais) | "Sem permissão para esta consulta" com o motivo e "Tentar de novo"; "0 de 13 filiais responderam"; os cinco cartões com "—" e o motivo |
| Falha parcial (3 filiais com 500) | "10 de 13 filiais responderam" em destaque; os totais são das 10; a composição nomeia as que falharam; a contagem de concorrentes some, para não misturar 10 com 13 |
| Sem dado (13 filiais vazias) | "—" onde não há base (faturamento, meta, cobertura), 0 onde zero é valor (clientes, vendas perdidas); período "sem registro" |
| Mapas sem permissão (403) | bloco "Sem permissão para esta consulta" |

**Perfis.** A visão da empresa inteira, com e sem a permissão `Empresa.AlcanceEntreFiliais`, continua
coberta pelos testes de API com perfis fictícios (documento 32, §8.5.5). Nenhuma permissão foi concedida
a usuário real. Com a ponte por cabeçalho, a filial escolhida não é conferida contra o usuário
(**P-20**): o consolidado das filiais depende dela até o Entra ID (P-8).

---

## 6. Testes e resultados

| Conjunto | Resultado |
|---|---|
| `Tracbel.Crm.Api.Testes` | **82 passando** — 9 novos em `IndicadoresExecutivosTestes` (nota sem cliente por natureza e parcelas fechando o total; filial sem faturamento declara a lacuna; ano civil e meta só da filial dentro do ano; ano anterior não muda o mês do cartão; clientes únicos somáveis e "nas carteiras" não somável; cobertura pela cadência e lacunas de visita; vendas perdidas por filial sem percentual; ano fora do intervalo com 422) e 1 novo em `IndicadoresTerritoriaisTestes` (responsáveis das carteiras pelos vínculos) |
| `Tracbel.Crm.Dominio.Testes` | **183 passando** — `Concessao_temporaria_expira` usava a data fixa 30/08/2026 + 15 dias e passou a falhar em 14/09/2026 às 12h UTC, porque a concessão recusa expiração no passado; o teste agora usa o relógio, com a mesma intenção |
| `Tracbel.Crm.Aplicacao.Testes` | 56 passando |
| `Tracbel.Crm.Integracao.Testes` | 101 passando |
| `Tracbel.Crm.Arquitetura.Testes` | 62 passando |
| **Total .NET** | **484 passando, 0 falha** |
| Front | `tsc -b` sem erro; `oxlint` sem erro (os avisos restantes são de arquivos não alterados) |
| Conferência API × SQL × tela | §3 e §4.2, todas iguais |
| Console do navegador | 0 erro nas capturas da Visão 360, dos mapas e dos estados |

---

## 7. Capturas

Em `dados-locais/capturas/` (fora do Git; têm número e nome reais):

| Arquivo | O que mostra |
|---|---|
| `antes-visao360-1-geral.png`, `antes-visao360-2-cartoes.png` | a Visão 360 antes |
| `depois-visao360-1-geral.png`, `depois-visao360-2-cartoes.png` | a Visão 360 depois |
| `antes-mapas-1..5-*.png` | os mapas antes |
| `depois-mapas-1-geral.png`, `-2-mapas.png` | a tela e os três mapas depois |
| `depois-mapas-3-foco-ribeirao-preto.png` | o foco compartilhado nos três mapas |
| `depois-mapas-4-detalhe-ribeirao-preto.png` | a ficha, com os responsáveis das carteiras |
| `depois-mapas-5-mapas-quantidade-posvenda.png` | pendentes em quantidade e vendas de pós-venda |
| `depois-mapas-6-regiao-norte.png`, `-6-regiao-noroeste.png` | as duas regiões, no mesmo quadro |
| `depois-mapas-7-ano-civil.png` | o período "ano civil até o último mês fechado" |
| `depois-mapas-8-tabela-fora-do-mapa-e-total.png` | os grupos fora do mapa e o total |
| `estado-1-carregando.png` a `estado-5-mapas-sem-permissao.png` | os estados do §5 |

Scripts: `scripts/prototipo/capturar-visao360.mjs`, `capturar-territorio.mjs` (atualizado para os novos
controles, o foco, as duas regiões e o ano civil) e `capturar-estados-visao360.mjs`.

---

## 8. Pendências objetivas

| # | Pendência | Impacto hoje | Quem resolve |
|---|---|---|---|
| P-2 | Quais tipos de atividade contam como visita (marcar `ContaParaCobertura`) e se registro do sistema conta | a cobertura é de contato registrado; com só contato humano, 24.318 vínculos têm contato, e não 39.441 | gerente comercial |
| P-4 | Calendário fiscal (início do ano) | cartões em ano civil; mapas sem FYTD | diretoria |
| P-5 | Regra de devolução e cancelamento | nada é abatido | fiscal/controladoria |
| P-26 | Cadastrar as metas de faturamento (por filial e, se houver, por carteira), preenchendo a planilha-modelo `04-metas.xlsx` | cartão B sem meta | comercial/diretoria |
| P-27 | Regra de sanitização: 4 documentos em 8 cadastros de filiais diferentes; 7.391 clientes em carteira sem CPF/CNPJ; 387 CNPJs só por nome (P-25) | contagem única depende do cadastro; nada foi fundido nem completado | comercial (cadastro) + TI |
| P-28 | Classe de cliente de outra filial na visão por filial | 501 vínculos medidos como D (diferença de 38 no prazo no consolidado) | TI + diretoria (junto com P-10/P-20) |
| P-29 | Fonte de mercado (emplacamento) e venda perdida por município | sem participação; perda sem município | comercial + TI |
| P-11 | Trazer o ART para o banco do projeto (sincronização agendada, leituras adicionais) | venda de máquina por chassi e comprador fora do CRM | dono do ART + infraestrutura |
| P-8 / **P-20** | O consolidado lê filial a filial pelo cabeçalho, que não é conferido contra o usuário | a leitura depende da ponte provisória | TI + diretoria |
| P-30 | Publicar os cartões e os mapas novos | o servidor mostra a versão anterior | Ricardo (publicação) |
| P-21 | Este documento e o 32 trazem faturamento e contagem da empresa | decidir antes do push | Ricardo |

---

## 9. Arquivos alterados (sem commit)

| Camada | Arquivos |
|---|---|
| Domínio | `Portas/PortasDeIndicadoresExecutivos.cs` (novo); `Portas/PortasDeIndicadoresTerritoriais.cs` (`ResponsavelPelaCarteira`) |
| Aplicação | `Relacionamento/ObterIndicadoresExecutivos.cs` (novo) |
| Infraestrutura | `Repositorios/RepositorioDeIndicadoresExecutivos.cs` (novo); `Repositorios/RepositorioDeIndicadoresTerritoriais.cs` (responsáveis das carteiras) |
| API | `Program.cs`; `Endpoints/EndpointsDeRelacionamento.cs` (rota nova) |
| Testes | `Api.Testes/IndicadoresExecutivosTestes.cs` (novo); `Api.Testes/IndicadoresTerritoriaisTestes.cs`; `Dominio.Testes/Seguranca/AutorizadorTestes.cs` |
| Front | `tipos/painelExecutivo.ts` (novo); `tipos/territorio.ts`; `dados/api/consolidado.ts`; `componentes/painel360/PainelExecutivo.tsx`; `estilos/painel-executivo.css` (novo); `telas/Visao360.tsx`; `telas/IndicadoresGeograficos.tsx`; `componentes/territorio/MapaDeMunicipios.tsx`, `escalas.ts`, `projecao.ts`, `DetalheDoMunicipio.tsx`; `estilos/territorio.css` |
| Scripts | `scripts/prototipo/capturar-visao360.mjs` (novo), `capturar-territorio.mjs`, `capturar-estados-visao360.mjs` (novo) |
| Documentos | este; documento 32 (versão 1.4); documento 35 (versão 1.3) |

Continuam também sem commit as alterações da rodada anterior de 14/09/2026 (documento 35, §9).
