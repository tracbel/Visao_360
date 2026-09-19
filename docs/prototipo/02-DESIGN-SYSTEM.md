# 02 — Design System do protótipo

> Fonte única: `prototipo/referencia/assets/app.css` (6.790 linhas), carregado por
> `prototipo/referencia/index.html` como `<link rel="stylesheet" href="assets/app.css?v=map-r1">`.
> Este documento é um **levantamento**, não uma proposta de melhoria — todo token, toda classe e
> toda inconsistência abaixo existe exatamente como descrita no CSS original. Onde o próprio
> protótipo é inconsistente (cores repetidas com hex diferente, variável nunca declarada, 3
> implementações de drawer), isso é sinalizado explicitamente em vez de "corrigido" aqui.

---

## 1. Tokens declarados em `:root` (app.css:2-63)

### 1.1 Cores John Deere

| Token | Valor | Onde é usado |
|---|---|---|
| `--jd-green` | `#367C2B` | Cor primária de marca: sidebar ativo, botões primários, gradientes de header de tela (Clientes, Config, Performance, Pipeline), ícones ativos, foco de input |
| `--jd-green-dark` | `#1B5E20` | Hover de botão primário, texto de link/ação (`cell-link`, `.v360-link`), lado escuro dos gradientes de header, ponta final de vários gradientes |
| `--jd-green-darker` | `#0D3E12` | Início do gradiente do `.home-hero` |
| `--jd-yellow` | `#FFDE00` | Cor de destaque John Deere: logo da sidebar, `--jd-yellow` em ícones ativos da sidebar, badge "NOVA" no Pipeline/Ficha do Cliente/Funil, nó "fase atual" da timeline de oportunidade, brand-sub |
| `--jd-yellow-dark` | `#E5C700` | Ponta do gradiente do `.proto-banner` (banner não usado nas rotas atuais, mas declarado) |

### 1.2 Neutros (escala de cinza-esverdeado, 12 degraus)

| Token | Valor | Papel típico |
|---|---|---|
| `--n-950` | `#0F1613` | Fundo da sidebar (`--bg-sidebar`) |
| `--n-900` | `#1A2420` | Texto primário (`--text-primary`) |
| `--n-800` | `#2A3630` | (declarado; sem uso direto identificado fora de heranças de `--text-primary`) |
| `--n-700` | `#3D4B44` | idem |
| `--n-600` | `#566158` | Texto secundário (`--text-secondary`) |
| `--n-500` | `#7A857D` | Texto terciário (`--text-tertiary`) |
| `--n-400` | `#9DA69F` | idem, tons intermediários pontuais |
| `--n-300` | `#C4CBC6` | Borda forte (`--border-strong`), separador de breadcrumb |
| `--n-200` | `#E1E5E2` | Borda padrão (`--border`) |
| `--n-100` | `#F0F2F1` | Fundo de hover (`--bg-hover`), badges neutros, chips inativos |
| `--n-50` | `#F7F8F7` | Fundo da aplicação (`--bg-app`), cabeçalho de tabela, fundo zebra de hover de linha |
| `--white` | `#FFFFFF` | Superfícies (`--bg-surface`), texto sobre fundo escuro (`--text-inverse`) |

### 1.3 Estados (semáforo)

| Token | Valor | Token `-bg` | Valor `-bg` |
|---|---|---|---|
| `--success` | `#16A34A` | `--success-bg` | `#DCFCE7` |
| `--warning` | `#EA580C` | `--warning-bg` | `#FFEDD5` |
| `--danger` | `#DC2626` | `--danger-bg` | `#FEE2E2` |
| `--info` | `#2563EB` | `--info-bg` | `#DBEAFE` |

Usados em `.badge-success/-warning/-danger/-info`, `.kpi-delta.up/.down`, nota de escopo do Funil
(`--info`), badge de dot de notificação (`--danger`).

### 1.4 Semânticos (aliases sobre neutros)

| Token | Resolve para | Uso |
|---|---|---|
| `--bg-app` | `var(--n-50)` | `body`, fundo geral da página |
| `--bg-surface` | `var(--white)` | `.card`, `.kpi`, inputs |
| `--bg-elevated` | `var(--white)` | declarado; mesmo valor de `--bg-surface`, sem diferenciação visual no CSS atual |
| `--bg-sidebar` | `var(--n-950)` | `.sidebar` |
| `--bg-hover` | `var(--n-100)` | hover de `.nav-item`, `.btn-icon`, `.btn-ghost` |
| `--border` | `var(--n-200)` | borda padrão de cards/tabelas/inputs |
| `--border-strong` | `var(--n-300)` | borda de botão secundário, input |
| `--text-primary` | `var(--n-900)` | título, valor de KPI, texto de tabela |
| `--text-secondary` | `var(--n-600)` | subtítulo, rótulo de campo |
| `--text-tertiary` | `var(--n-500)` | hint, legenda, texto auxiliar |
| `--text-inverse` | `var(--white)` | declarado; sem uso direto localizado (telas com fundo escuro usam `#fff` literal em vez do token) |

### 1.5 Tipografia (famílias)

| Token | Valor | Uso |
|---|---|---|
| `--font-sans` | `'Inter', -apple-system, BlinkMacSystemFont, sans-serif` | fonte de corpo padrão (`body`) |
| `--font-mono` | `'JetBrains Mono', ui-monospace, monospace` | todo valor numérico "de identidade": chassi, CNPJ, CPF, valores monetários em tabela, IDs de oportunidade/OS, datas em tabela — mistura com `--font-sans` na mesma tela é deliberada (mono = "isto é um dado exato", sans = "isto é texto") |

### 1.6 Sombras

| Token | Valor | Uso |
|---|---|---|
| `--shadow-sm` | `0 1px 2px rgba(15, 22, 19, 0.04)` | `.card`, `.kpi` (repouso) |
| `--shadow` | `0 2px 8px rgba(15, 22, 19, 0.06), 0 1px 2px rgba(15, 22, 19, 0.04)` | hover de `.screen-tile`, `.seg-btn.active` |
| `--shadow-lg` | `0 8px 24px rgba(15, 22, 19, 0.08), 0 2px 6px rgba(15, 22, 19, 0.04)` | declarado; drawers/modais usam sombras ad-hoc mais fortes (`-20px 0 60px rgba(15,23,42,.2)` no drawer da Agenda, `0 8px 24px rgba(0,0,0,.12)` no dropdown de CEN) em vez deste token — inconsistência: sombras de elementos flutuantes não usam a escala de `:root` |

### 1.7 Raio de borda

| Token | Valor | Uso |
|---|---|---|
| `--radius-sm` | `4px` | badge, elementos pequenos |
| `--radius` | `6px` (11 usos) | botão, input, card pequeno — o mais usado |
| `--radius-lg` | `10px` (5 usos) | `.card`, `.kpi`, painéis |
| `--radius-xl` | `14px` | `.home-hero` |

**Divergência real:** a maioria das telas construídas *depois* da Visão 360/Home (Ficha do Cliente,
Equipamento, Oportunidade, Cobertura, Pipeline, Clientes, Config, Performance, Nova Oportunidade,
Pós-vendas) **não usa os tokens `--radius*`** — usa valores literais em px espalhados pelo arquivo:
`4,5,6,7,8,10,11,12,14,16,20,24px` e `999px`/`50%` para pflags e avatares. Ou seja, a escala de raio
declarada em `:root` cobre só a primeira leva de componentes (shell, cards genéricos, KPI, tabela,
badge, tabs, filtro, funil).

---

## 2. Tokens usados no CSS mas **nunca declarados** (defeito real do protótipo)

Estes cinco identificadores aparecem em `var(--nome)` em várias telas mas **não existem** em nenhum
bloco `:root` do arquivo (há só um único `:root`, nas linhas 2-63, listado acima). Quando uma
variável CSS é referenciada e não está definida (e não há fallback, ex. `var(--x, red)`), a
declaração inteira em que ela aparece é tratada como inválida pelo navegador e ele usa o valor
herdado/inicial da propriedade — ou seja, essas bordas/fundos **não aparecem como o autor pretendia**.

| Token fantasma | Ocorrências | Onde aparece (telas) | Valor mais provável pela intenção do contexto |
|---|---:|---|---|
| `--border-primary` | 69 | Ficha do Cliente, Ficha do Equipamento, Ficha de Oportunidade, Cobertura de Carteira, Pipeline, Config, Performance, Pós-vendas | equivalente a `--border` (`#E1E5E2`) |
| `--bg-primary` | 27 | idem | equivalente a `--bg-surface` (`#FFFFFF`) |
| `--bg-secondary` | 4 | Pipeline (`.pipeline-search`, `.card-prob`), Config | equivalente a `--n-100` (`#F0F2F1`) |
| `--surface-2` | 10 | Cobertura (dashboard antigo), Agenda (`.view-toggle`, `.calendar-header`) | equivalente a `--n-50`/`--n-100` |
| `--surface-3` | 3 | Cobertura (`.widget-menu:hover`, `.motivo-bar-track`) | equivalente a `--n-100` |

**Recomendação para o port:** ao reconstruir em React, declare esses cinco tokens no tema (ou faça
find-and-replace pelos equivalentes reais acima) — não copie o `var(--border-primary)` literal sem
declará-lo, ou herdará o mesmo bug visual silencioso do protótipo.

---

## 3. Escala tipográfica real (medida, não a nominal)

`font-size` aparece com estes valores exatos no arquivo (todos em `px`), em ordem crescente:

```
9  9.5  10  10.5  11  11.5  12  12.5  13  13.5  14  15  16  17  18  20  22  24  26  28  32  40
```

Não é uma escala limpa (ex. tipo 1.25×) — é o resultado de cada tela ter sido construída
separadamente com ajustes finos de meio pixel (`9.5`, `10.5`, `11.5`, `12.5`, `13.5`). Agrupando por
papel:

| Papel | Tamanhos observados | Exemplos de classe |
|---|---|---|
| Micro-rótulo (uppercase, letter-spacing) | 9–11px | `.kpi-label`, `th`, `.filter-field label`, `.novaop-label`, `.pk-label` |
| Corpo / dado de tabela | 12–13.5px | `td`, `.form-field input`, `.cliente-sub` |
| Corpo padrão do app | 14px | `body` (base), `.info-value`, `.form-field` (drawer) |
| Subtítulo / rótulo de seção | 14–15px | `.card-title`, `.agenda-grupo-title`, `.drawer-modelo` |
| Título de página | 15–18px | `.empty-state h3`, `.drawer-title` |
| Valor de KPI padrão | 18–24px | `.kpi-value`, `.v360-kpi-valor`, `.pv-kpi-valor` |
| Título de tela / hero | 20–26px | `.page-title` (20), `.clientes-title`/`.config-title`/`.perf-title` (22), `.cliente-nome` (24), `.opp-title` (26), `.v360-title` (26) |
| Número "hero" (poucos lugares) | 28–40px | `.frota-linha-count` não chega a isso; `.pv-nps-num` (32), texto central de gauge Chart.js desenhado via canvas (`32px` no plugin `afterDraw`) |

**Escala de peso:** só `400/500/600/700` são usados (`font-weight`), com `600`/`700` dominando
títulos e valores e `500` em rótulos e corpo de botão.

---

## 4. Espaçamento (gap/padding/margin)

Não existe token de espaçamento em `:root` — todo `padding`/`gap`/`margin` é literal. Os valores em
px mais frequentes no arquivo (contagem de ocorrências entre parênteses) são:
**8 (103), 6 (96), 4 (89), 12 (89), 2 (71), 10 (71), 16 (67), 20 (46), 14 (42), 24 (25)**, com usos
pontuais em 1, 3, 5, 7, 9, 18, 32, 60, 64px. Na prática o protótipo segue informalmente uma escala de
**passo 2px entre 2–24px**, com 8/12/16/20/24 como os "múltiplos de 4" preferidos para
padding de card/seção e 4/6/8 para espaçamento interno fino (ícone-texto, badge).

---

## 5. Shell da aplicação (layout de página inteira)

```html
<div class="app-shell">          <!-- grid-template-columns: 248px 1fr; min-height:100vh -->
  <aside class="sidebar">        <!-- bg var(--bg-sidebar); position sticky; top:0; height:100vh -->
    <div class="sidebar-brand">…</div>
    <nav class="sidebar-nav">
      <div class="nav-section">
        <span class="nav-section-title">EXECUTIVO</span>
        <a class="nav-item [active]" data-route="/">…</a>
      </div>
      <!-- 4 seções: Executivo, Comercial, Relatórios, Sistema -->
    </nav>
    <div class="sidebar-footer"><div class="user-chip">…</div></div>
  </aside>
  <main class="main">
    <header class="topbar">      <!-- height:60px; sticky; top:0 -->
      <div class="breadcrumb" id="breadcrumb">…</div>
      <div class="topbar-actions">
        <div class="search">…</div>              <!-- input + <kbd>Ctrl K</kbd>, decorativo -->
        <button class="btn-icon">…</button>       <!-- notificações, com .badge-dot -->
        <button class="btn-icon">…</button>       <!-- sincronização -->
      </div>
    </header>
    <div class="content" id="content">…</div>     <!-- max-width:1400px; padding:24px 32px 48px -->
  </main>
</div>
```

`.nav-item.active` usa `box-shadow: inset 2px 0 0 var(--jd-green)` (barra verde à esquerda) +
`background: rgba(54,124,43,.18)` + ícone SVG some para `--jd-yellow`. Ver `docs/prototipo/01-ESPEC-UI.md`
para o conteúdo exato de cada seção do menu, breadcrumb e topbar (busca/notificação/sync são
decorativos — sem handler real, conforme apurado tela a tela).

---

## 6. Catálogo de componentes CSS por família

Cada entrada: o que é, onde aparece, HTML mínimo para reproduzir. Classes utilitárias repetidas
(`.card`, badges, botões) valem para o app inteiro; famílias com prefixo (`opp-`, `v360-`, `pv-`,
`carteira-`, `pipeline-`, `clientes-`, `config-`, `perf-`, `novaop-`) são **específicas de uma tela**
e não devem ser reaproveitadas fora dela sem revisão — o protótipo não as tratou como sistema
compartilhado, e sim recriou padrões parecidos (KPI, badge, tabela) com nomes de classe próprios em
cada tela nova. Isto é o achado mais importante da auditoria de CSS: **não existe um único
"design system" de componentes — existem ~9 variações paralelas do mesmo punhado de padrões**
(cartão com cabeçalho, grade de KPI, badge de status, tabela com cabeçalho sticky, pílula de filtro).

### 6.1 `.card` — cartão genérico (o mais reutilizado de fato)

Usado em: Home, Funil, Cobertura Regional (como `.widget-card`, que estende `.card`), Agenda
(drawer), Pós-vendas. Não usado nas telas "novas" (Cliente/Equipamento/Oportunidade/Carteira/
Pipeline/Clientes/Config/Performance), que criaram `.ficha-bloco`, `.carteira-tabela-wrap`,
`.pipeline-coluna`, `.config-card`, `.perf-card` em vez de reaproveitar `.card`.

```html
<div class="card">
  <div class="card-header">
    <div>
      <div class="card-title">Título</div>
      <div class="card-subtitle">Subtítulo opcional</div>
    </div>
    <!-- ação opcional à direita: botão, segmented control -->
  </div>
  <div class="card-body">…</div>
</div>
```
`border:1px solid var(--border); border-radius:var(--radius-lg); box-shadow:var(--shadow-sm)`.

### 6.2 Cartão-equivalente por tela (variações paralelas de 6.1)

| Classe raiz | Tela | Header próprio | Observação |
|---|---|---|---|
| `.ficha-bloco` + `.ficha-bloco-header`/`.ficha-bloco-titulo` (uppercase) | Ficha do Cliente/Equipamento | sim | `renderFichaBloco(id, titulo, conteudo, spacious)` é o único helper de card reutilizado *dentro* de uma tela |
| `.config-card` + `.cc-header`/`.cc-body` | Configurações | sim | título `<h3>` dentro do header, não `<div class="card-title">` |
| `.perf-card` + `.perf-card-header` | Performance de CEN | sim | idem, `<h3>` |
| `.opp-side-card` + `.opp-side-title` | Ficha de Oportunidade (coluna lateral) | sim (uppercase, borda inferior) | |
| `.pv-card-*` (herda `.card`) | Pós-vendas | reaproveita `.card-header`/`.card-title` | única tela "nova" que reaproveitou o `.card` genérico |
| `.v360-card` + `.v360-card-header`/`.v360-card-title` | Visão 360 | sim | 3 tamanhos via classe vazia `.v360-card-md`/`.v360-card-lg` (regra `/* placeholder */`, **sem efeito real** — o tamanho vem do `grid-template-columns` do pai, não da classe) |

### 6.3 KPI — grade de indicadores (7 implementações paralelas)

Padrão comum: rótulo pequeno maiúsculo + valor grande + hint pequeno. Nomes de classe raiz por tela:
`.kpi` (genérico: Home/Funil/Cobertura Regional/Agenda), `.carteira-kpi`, `.pipeline-kpi`/`.kpi`+`.pk-*`
(Performance usa `.pk-label`/`.pk-value`/`.pk-hint`, prefixo diferente do restante do KPI!),
`.clientes-kpi`, `.v360-kpi-card`+`.v360-kpi-*`, `.pv-kpi`+`.pv-kpi-*`, `.opp-kpi`+`.opp-kpi-*`.

```html
<!-- forma genérica (.kpi) -->
<div class="kpi-grid">
  <div class="kpi [kpi-highlight|kpi-danger|kpi-sm]">
    <span class="kpi-label">Rótulo</span>
    <span class="kpi-value">24 <span style="font-size:14px;color:var(--text-tertiary)">/ 30</span></span>
    <span class="kpi-delta up">↑ 7%</span>            <!-- opcional -->
    <span class="kpi-hint">Texto de apoio</span>
  </div>
</div>
```
Variantes de tom: `.kpi-highlight` (gradiente verde escuro, texto branco, hint amarelo),
`.kpi-danger` (gradiente vermelho claro), `.kpi-good`/`.kpi-warn` (borda + fundo tingido, usado em
`.carteira-kpi`/`.clientes-kpi`/`.pipeline-kpi`).

### 6.4 Tabela

Base genérica (`table`/`th`/`td` sem classe, `.table-wrap`, `.table-actions`, `.table-footer`,
`.pagination`) usada em Home/Funil. Cada tela "nova" tem sua própria classe de tabela com o mesmo
padrão de cabeçalho maiúsculo cinza + zebra de hover, mas nomes próprios:
`.carteira-tabela`, `.clientes-tabela` (cabeçalho `sticky`, `th-sortable` com cursor pointer),
`.config-tabela`, `.perf-tabela`, `.pv-table`, `.opp-itens-tabela`.

```html
<div class="table-wrap">
  <table>
    <thead><tr><th>Coluna</th><th class="num">Valor</th></tr></thead>
    <tbody>
      <tr><td>…</td><td class="num">…</td></tr>
    </tbody>
  </table>
  <div class="table-footer">
    <span>Mostrando 10 de 464 registros</span>
    <div class="pagination"><button class="btn btn-ghost btn-sm" disabled>← Anterior</button>…</div>
  </div>
</div>
```
`td.mono`/`.pv-mono`/`.carteira-tabela .mono` aplicam `font-family:var(--font-mono)` a células de
identidade (chassi, CNPJ, ID).

### 6.5 Badge / chip de status (a família mais fragmentada)

Genérico: `.badge` + modificador de cor (`-success/-warning/-danger/-info/-neutral/-green`).
Cada tela nova recriou o mesmo conceito com paleta e nome próprios — **11 vocabulários de badge
diferentes coexistem**:

| Família | Estados | Onde |
|---|---|---|
| `.badge` + `.badge-{success,warning,danger,info,neutral,green}` | genérico | Home, Funil (fase), Ficha do Cliente (status "Ativo") |
| `.badge-classe` + `.classe-a/.classe-b` | Classe A/B (só 2, sem C/D) | Cabeçalho da Ficha do Cliente |
| `.badge-classe-A/-B/-C/-D` (4 classes soltas, não têm classe base comum) | Classe A–D | Tabela de Cobertura |
| `.classe-badge` + `.classe-a/b/c/d` | Classe A–D (quadrado 18×18, iniciais) | Card do Kanban Pipeline |
| `.nci-value.classe-a/b/c/d` | Classe A–D (só cor de texto) | Info do cliente em Nova Oportunidade |
| `.status-pill` + `.status-dot` (cor inline via `style`) | em_dia/aviso/atraso/critico/nunca | Tabela de Clientes |
| `STATUS_LABELS`/`.leg-dot` (cor inline) | os mesmos 5 status de cobertura | Legenda do mapa de Cobertura |
| `.pv-status` + `.pv-status-{red,amber,green,blue,gray}` | status de OS/PMP | Pós-vendas |
| `.pv-badge` + `.pv-badge-{red,amber,green,blue,neutral}` | contagens/alertas | Pós-vendas KPIs |
| `.badge-status` + `.ok/.warn/.danger/.muted` | status de integração | Configurações |
| `.role-pill` + `.role-{adminti,diretorcomercial,gerenteregional,cen,admincomercial}` | papel de usuário | Configurações |
| `.opp-status-badge` + `.ganho/.perdido/.cancelado` | status macro da oportunidade | Header da Ficha de Oportunidade |
| `.opp-desc-pill` + `.aprovado/.dentro_alcada/.pendente/.rejeitado` | aprovação de item | Itens da proposta |
| `.opp-fase-aprov-marker` + `.pendente/.aprovada/.rejeitada/.nao_iniciada` | aprovação por fase | Timeline da oportunidade |
| `.card-resultado` + `.ganho/.perdido` | resultado do card | Kanban Pipeline |
| `.tarefa-classe` + `.classe-a/.classe-b` | classe do cliente na tarefa | Cards da Agenda |
| `.pv-pmp-badge` + `.pv-pmp-crit-{red,amber,info}` | criticidade de PMP | Pós-vendas |

Cada família usa **hexadecimais próprios para o mesmo conceito de cor** — ex. "crítico"/vermelho
aparece como `#DC2626` (badge-danger), `#7F1D1D` (status critico de cobertura, mais escuro —
propositalmente para diferenciar de "atraso"), `#991B1B` (pv-status-red), `#B91C1C` (badge-visita).
Ao portar, vale consolidar essa paleta de estado num único enum, mas isso é decisão de
implementação — o levantamento aqui é só constatar que hoje são valores redigitados, não um token
compartilhado.

### 6.6 Botões

`.btn` base + `.btn-primary` (verde sólido) / `.btn-secondary` (branco, borda) / `.btn-ghost`
(transparente) / `.btn-sm`. Telas novas voltaram a criar variantes próprias: `.btn-icon`/
`.btn-icon-large`/`.btn-icon-sm`/`.btn-icon-inline`/`.btn-linha`/`.btn-acao`/`.btn-acao-primary`
(Cobertura), `.btn-config-primary`/`.btn-config-inline`/`.btn-config-cancelar`/`.btn-config-salvar`
(Config), `.btn-cancelar`/`.btn-salvar` (Nova Oportunidade, cores fixas em vez de var()),
`.opp-btn`/`.opp-btn.primary`/`.opp-btn.danger` (Oportunidade, sobre fundo verde escuro do header).

### 6.7 Filtros / chips de filtro

`.filter-bar` (grid) + `.filter-field` (genérico, usado por Funil/Cobertura Regional/Agenda) versus
`.carteira-filtros`/`.filtro-grupo`/`.filtro-chips` (Cobertura de Carteira, com `--chip-color` inline
por chip) versus `.clientes-filtros`/`.filtro-grupo` (Clientes, reaproveita o nome `.filtro-grupo` da
Cobertura mas com HTML diferente) versus `.pipeline-filtros` (Pipeline, só `<select>`, sem chip).
`.chip`/`.chip.active` é a classe de pílula clicável comum a Cobertura e Clientes, mas cada uma
define suas próprias variantes de cor ativa (`.chip[data-val="critico"].chip-active` vs
`.chip.chip-status-em_dia.active`).

### 6.8 Tabs

4 implementações: `.tabs`/`.tab`/`.tab.active`/`.tab-content.active` (genérico, usado na Ficha do
Cliente e do Equipamento e na Oportunidade — mesma classe, telas diferentes, consistente);
`.config-abas`/`.config-aba.active` + `.aba-lock` (cadeado nas abas Comercial/TI — ver doc 01);
`.view-toggle`/`.view-tab.active` (Agenda, Lista×Semana); `.persona-tabs`/`.persona-tab.active`
(Pipeline) e `.perf-switcher`/`.perf-persona.active` (Performance) — funcionalmente iguais
(3 opções CEN/Regional/Nacional) mas com nomes e paddings diferentes.

### 6.9 Modal

Uma implementação única, reaproveitada para dois conteúdos: `.modal-ficha-indispo-overlay` (overlay
fixo, `display:flex;align-items:center;justify-content:center`) + `.modal-ficha-indispo` (caixa
branca 520px) + `.mfi-header`/`.mfi-icon`/`.mfi-title`/`.mfi-sub`/`.mfi-close`/`.mfi-body`/
`.mfi-actions`. Usada por `mostrarModalFichaIndispo()` (aviso de ficha não disponível) e
`confirmarLimparSessao()` (confirmação de limpeza) — ver doc 01 para o texto exato de cada um.

```html
<div class="modal-ficha-indispo-overlay">
  <div class="modal-ficha-indispo">
    <div class="mfi-header">
      <div class="mfi-icon">…</div>
      <div><div class="mfi-title">…</div><div class="mfi-sub">…</div></div>
      <button class="mfi-close">×</button>
    </div>
    <div class="mfi-body"><p>…</p></div>
    <div class="mfi-actions"><button class="btn-cancelar">Fechar</button></div>
  </div>
</div>
```

### 6.10 Drawer (3 implementações não compartilhadas — divergência real)

| Implementação | Classes | Largura | Mecânica de abrir/fechar |
|---|---|---|---|
| Agenda (registro de contato) | `.drawer` (fixed inset:0) > `.drawer-backdrop` + `.drawer-panel` (slide-in `@keyframes drawer-in`) | 640px (max 96vw) | `style.display='block'/'none'` via `openTarefaDrawer()`/`closeTarefaDrawer()`; Esc e clique no backdrop fecham |
| Cobertura de Carteira | `.carteira-drawer` + modificador `.drawer-open` > `.drawer-backdrop`(opacity)+`.drawer-panel`(`translateX`) | 480px | classe `drawer-open` alternada; transição CSS em vez de `@keyframes` |
| Pipeline (detalhe de oportunidade) | `.pipeline-drawer` + modificador `.open` > `.drawer-overlay`+`.drawer-panel` | 440px | classe `open`; `pointer-events` alternado no container |

As três resolvem o mesmo problema (painel lateral deslizante) com nomes de classe, larguras e
mecanismo de toggle diferentes — nenhuma reaproveita a outra. Também **cada uma redeclara**
`.drawer-header`/`.drawer-title`/`.drawer-footer` com estilos ligeiramente distintos (ver linhas
1092-1153 vs 3245-3251 vs 3589-3611 do app.css).

### 6.11 Timeline / stepper

3 formas visuais distintas para "sequência de eventos":
- **Vertical genérica** `.timeline`/`.timeline-item`/`.timeline-marker`/`.timeline-body` — histórico
  de interações da Ficha do Cliente (marcador circular verde com ícone, linha vertical à esquerda).
- **Vertical de revisão** `.revisoes-timeline`/`.revisao-item`/`.revisao-marker`/`.revisao-body` —
  visualmente quase idêntica à anterior mas com classes próprias (Ficha do Equipamento).
- **Horizontal (stepper de fases)** `.opp-timeline`/`.opp-fases`/`.opp-fase`/`.opp-fase-node`/
  `.opp-fase-connector` — as 8 fases do processo comercial na Ficha de Oportunidade, nó circular
  numerado, conector colorido conforme concluído/atual/futuro, com `@keyframes pulseOpp` piscando no
  nó "atual" e `.opp-fase-aprov-marker` pendurado abaixo do nó quando a fase tem aprovação.

### 6.12 Kanban (Pipeline de Vendas)

`.pipeline-board` (grid 6 colunas) > `.pipeline-coluna` (`.final` no último) > `.coluna-header`
(`.coluna-title`+`.coluna-count`+`.coluna-total`) > `.coluna-cards` (`.drop-target` durante
drag-over) > N × `.pipeline-card` (modificadores `.classe-a`/`.travada`/`.ganho`/`.perdido`/
`.dragging`/`.criada-agora`) > `.card-alerta`/`.card-resultado`/`.card-titulo`/`.card-cliente`/
`.classe-badge`/`.card-meta`/`.card-valor`/`.card-rodape`/`.card-cen`/`.card-prob`/
`.card-lock-hint`/`.card-motivo`. Ver doc 01 para a mecânica exata de drag-and-drop.

### 6.13 Mapa (Leaflet — Cobertura de Carteira)

Todo o namespace `.leaflet-*` é override de estilo padrão do Leaflet 1.9.4 (fundo do container,
atribuição, controle de zoom). O pin custom é um `L.divIcon` com `.leaflet-cluster-pin` >
`.leaflet-cluster-halo` (círculo pulsante, `@keyframes pinPulse`, cor via `--pin-color` inline) +
`.leaflet-cluster-body` (número, cor de fundo = `--pin-color`). Popup customizado via
`.leaflet-popup-tracbel` > `.pop-cluster` (`.pop-title`/`.pop-sub`/`.pop-breakdown`/`.pop-row`/
`.pop-dot`/`.pop-btn`). Tooltip customizado via `.leaflet-tooltip-tracbel`. Nota de rodapé fixa
`.mapa-fonte-aviso` avisando que os tiles são OpenStreetMap (placeholder) e que produção deveria
avaliar Mapbox/MapTiler.

### 6.14 Formulário

Genérico: `.form-row`/`.form-field`/`.form-grid` (`.cols-3`/`.cols-4`) com `label`+`input/select/
textarea`, foco com `box-shadow:0 0 0 3px rgba(54,124,43,.12)`. Nova Oportunidade tem sua própria
skin completa (`.novaop-input`, `.novaop-field`, `.novaop-label` uppercase, `.novaop-row-2`/`-row-3`,
`.novaop-modo-item` como radio-pill, `.novaop-valor-total` como display read-only). Toggle switch
(`.switch`/`.slider`) só aparece em Configurações › Notificações. `<kbd>` estilizado para atalhos de
teclado (topbar e Config › Atalhos).

### 6.15 Estado vazio e toasts

`.empty-state` (ícone + título + texto, usado em Placeholder e Agenda) e duas famílias de toast:
`.novaop-toast` (borda esquerda colorida, ícone circular, `translateX` de entrada — sucesso ao criar
oportunidade e ao limpar sessão) e `.config-toast` (canto inferior direito, `translateY`, variante
`.toast-warn`).

### 6.16 Dica (tooltip) — **não existe no protótipo**

Nenhuma classe de tooltip no `app.css`: onde o protótipo explica um número, ele usa o `title=` do
navegador. O CRM herdou o hábito — **27 `title=` em 15 arquivos** em 19/09/2026 — e o `title` não
aparece para quem navega por teclado nem para quem usa toque, some sozinho e corta texto longo.

Por isso a dica é o **primeiro componente do CRM sem correspondente no protótipo**:
`src/componentes/InfoTooltip.tsx`, com as classes `.dica`, `.dica-gatilho` e `.dica-balao` no
`design-system.css` (issue 031). Ela abre por ponteiro, foco e toque, fecha com `Esc` e liga o balão
ao gatilho por `aria-describedby`. Os `title=` existentes continuam onde estão; trocá-los é trabalho
das issues de UX que mexem em cada tela (027, 028, 029), porque cada troca muda o desenho de uma tela
que é comparada com o protótipo.

---

## 7. Dependências externas (exatas, de `index.html`)

```html
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
<link rel="stylesheet" href="assets/app.css?v=map-r1">
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
      integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=" crossorigin="">
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
        integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=" crossorigin=""></script>
<script src="assets/app.js?v=map-r1"></script>
```

- **Inter**: pesos 400/500/600/700, único font-family de UI.
- **JetBrains Mono**: pesos 400/500, só para dados "de identidade" (ver §1.5/§6.4).
- **Chart.js 4.4.1**: via CDN, build UMD minificado. Usado só em 2 telas (Visão 360, aba Pós-vendas
  da Ficha do Cliente) — ver §8.1. Todas as outras "visualizações" do protótipo (Funil, gauges de
  Cobertura Regional, barras de Cobertura Regional/Performance) são **SVG desenhado à mão em
  JavaScript**, não Chart.js — ver §8.2.
- **Leaflet 1.9.4**: via CDN com SRI (`integrity`), CSS + JS. Único uso: mapa da tela Cobertura de
  Carteira (`/cobertura`). Tile provider: OpenStreetMap padrão (`{s}.tile.openstreetmap.org`),
  marcado no próprio protótipo como placeholder (§6.13).

---

## 8. Gráficos — catálogo completo (opções exatas de configuração)

### 8.1 Chart.js 4.4.1 — gráfico por gráfico

Todos criados com `responsive:false` (ou `true`+`maintainAspectRatio:false` nos 2 casos indicados),
sem redimensionamento reativo — o `<canvas>` tem `width`/`height` fixos no HTML.

| # | Canvas id | Tela | `type` | Datasets / cores | Options relevantes |
|---|---|---|---|---|---|
| 1 | `v360GaugeConhec` | Visão 360 | `doughnut` (usado como gauge) | 1 dataset: `[vendemos, perdaRegistrada, semConhecimento]`, `backgroundColor:['#367C2B','#FFDE00','#E5E7EB']`, `borderWidth:0` | `rotation:-90`, `circumference:180` (meio-círculo), `cutout:'72%'`, `legend:false`, `tooltip:true`; plugin custom `gaugeCenterText` (`afterDraw`) escreve `${conhecimento}%` (font 700 32px Inter, cor condicional verde/laranja/vermelho por faixa) + rótulo "conhecimento" (500 11px, cinza) no centro do semicírculo |
| 2 | `v360DonutCobertura` | Visão 360 | `doughnut` | 5 valores (em_dia/aviso/atraso/critico/nunca), `backgroundColor:['#367C2B','#B45309','#DC2626','#7F1D1D','#9CA3AF']`, `borderWidth:2`, `borderColor:'#FFFFFF'` | `cutout:'62%'`, `legend:false`, `tooltip:true`; plugin `donutCenterCobertura` escreve `${coberturaPct}%` (700 22px) + "ativa" (500 10px) no centro |
| 3 | `v360LinhaFat` | Visão 360 | `line`, 3 datasets | **Realizado**: `borderColor:'#367C2B'`, `backgroundColor:'rgba(54,124,43,.10)'`, `tension:.35`, `fill:true`, `borderWidth:2.5`, `pointRadius:3`. **Previsto**: `borderColor:'#0EA5E9'`, `borderDash:[5,4]`, `tension:.3`, `fill:false`, `borderWidth:2`, `pointRadius:2`. **Meta**: `borderColor:'#9CA3AF'`, `borderDash:[2,3]`, `tension:.2`, `fill:false`, `borderWidth:1.5`, `pointRadius:0` | `interaction:{mode:'index',intersect:false}`, `legend:false` (legenda é feita em HTML fora do canvas, `.v360-legenda-inline`), `tooltip.callbacks.label` formata `"R$ X.XM"`; eixo Y: `beginAtZero:false`, grid `rgba(0,0,0,.05)`, ticks `"R$ {v}M"`; eixo X sem grid |
| 4 | `v360MixLinha` | Visão 360 | `doughnut` | 5 linhas de produto, cores de `MIX_LINHAS` (`#367C2B,#1B5E20,#4A9040,#7CB342,#FFDE00`), `borderWidth:2`, `borderColor:'#FFFFFF'` | `cutout:'55%'`, `legend:false` (feita em HTML), tooltip mostra `"{label}: {pct}%"` |
| 5 | `v360Perdidas` | Visão 360 | `bar`, `indexAxis:'y'` (barra horizontal) | 1 dataset "Negócios perdidos", `backgroundColor` = array de cores por motivo (`VENDAS_PERDIDAS_MOTIVOS[].cor`), `borderRadius:4`, `borderWidth:0` | `legend:false`; tooltip customizado retorna array `["{qtd} negócios","Valor: {compacto}"]`; eixo X `beginAtZero:true`; eixo Y sem grid |
| 6 | `pv-fat-12m` | Pós-vendas (aba da Ficha do Cliente) | `bar` empilhado, 2 datasets | **Peças**: `backgroundColor:'#367C2B'`. **Serviços**: `backgroundColor:'#FFDE00'`. Ambos `borderRadius:4` | `responsive:true,maintainAspectRatio:false`; `legend:false` (legenda em HTML `.pv-legend-inline`); tooltip `"{label}: R$ {v}k"`; **ambos os eixos `stacked:true`**; eixo Y ticks `"R$ {v}k"` |
| 7 | `pv-mix-canal` | Pós-vendas | `doughnut` | 3 canais (balcão/oficina/campo), `backgroundColor:['#367C2B','#7CB342','#C5E1A5']`, `borderWidth:0` | `responsive:true,maintainAspectRatio:false`; `cutout:'62%'`; **legenda real do Chart.js** (não HTML) `position:'right'` com `generateLabels` customizado (`"{label}  {pct}%"`); tooltip `"{label}: {pct}%"` |

Guarda de montagem única: gráficos 6 e 7 verificam `el.__mounted` antes de instanciar (evita
recriar o Chart.js se a aba for re-renderizada); os 5 primeiros (Visão 360) não têm essa guarda —
cada chamada de `mountVisao360()` cria instâncias novas sem destruir as antigas explicitamente
(`chart.destroy()` não é chamado em nenhum dos dois grupos).

### 8.2 SVG desenhado à mão (sem biblioteca) — "gráfico por gráfico"

Estes são construídos concatenando strings de elementos SVG (`<path>`, `<rect>`, `<text>`) e
atribuindo a `svg.innerHTML`, não usam Chart.js:

| Elemento | Função em app.js | Tela | Mecânica |
|---|---|---|---|
| Funil | `mountFunil()` → `draw()` (app.js:569-690) | Funil de Vendas (`#funil-svg`, viewBox `0 640 440`) | Cada estágio é um `<path>` trapezoidal; largura proporcional ao valor da métrica ativa (`qty`/`value`/`opps`) entre um piso de 60px e teto de 480px — **não força ordem decrescente**, uma fase pode ficar "mais larga" que a anterior se o valor absoluto for maior. Cor por estágio vem de `FUNIL_DATA.stages[].color`. Rótulo dentro da faixa se ela tiver mais de 55px de largura (cor do texto branca, ou verde-escura se o fundo for amarelo/laranja); senão rótulo à direita da faixa. Métrica trocável por 3 botões `.seg-btn` (Quantidade/Valor R$/Oportunidades) que rechamam `draw()`. Legenda lateral (`#funil-legend`) mostra, por estágio, o valor e a conversão % em relação ao estágio anterior (`▲` se >100%, verde se ≥70%, laranja se ≥40%, vermelho abaixo) |
| Gauge (genérico) | `drawGauge(svgId, value, options)` (app.js:960-1022) | Cobertura Regional (2 instâncias: `#gauge-cobertura`, `#gauge-gpe`) | Semicírculo 180°→360°, raio 100, 4 segmentos de cor por faixa (parametrizável via `options.segments`; cobertura usa vermelho/laranja/amarelo/verde nos cortes 40/70/80/100, GPE usa a **paleta invertida** — verde/laranja/vermelho/vermelho-escuro nos cortes 30/60/80/100, porque para "vendas perdidas" menos é melhor); ticks a cada 20%; ponteiro (`<line>`+`<circle>`) apontando para o valor; valor central em texto grande |
| Barra por regional (cobertura) | `drawBarCobertura()` (app.js:1025-1066) | Cobertura Regional (`#bar-cobertura`, viewBox `640×260`) | Grid horizontal a cada 25%, linha tracejada verde na meta (80%), barras coloridas por limiar (verde ≥ meta, laranja ≥70%, vermelho abaixo), rótulo de % acima da barra, sigla+nome da regional abaixo |
| Barra vendas perdidas | `drawBarVendasPerdidas()` (app.js:1069-1105) | Cobertura Regional (`#bar-vendas-perdidas`, viewBox `400×260`) | Barras roxas (`#7C3AED`), ordenadas decrescente por quantidade, grid em 4 níveis (0/25/50/75/100% do máximo) |
| Pins do mapa | `renderPins()` (app.js:3763-3869) | Cobertura de Carteira | **Não é SVG solto** — é HTML (`L.divIcon`) injetado como marcador Leaflet; ver §6.13. Citado aqui para deixar explícito que não há um 5º gráfico SVG "de mapa": o mapa é 100% Leaflet + tiles, os "pins" são divs coloridos por status pior do grupo de clientes daquela cidade. |

**Nenhuma das telas de Performance de CEN, Config ou Ficha de Oportunidade usa `<svg>`/`<canvas>`
para gráfico de série temporal fora do que está listado acima** — os elementos "gráfico" adicionais
que essas telas possam ter (barra de progresso de meta, barra de horas de operação do equipamento)
são `<div>`s com `width` percentual controlado via `style` inline, não gráficos de biblioteca (ex.:
`.horas-bar-fill`/`.garantia-bar-fill`/`.pk-meta-fill`/`.credit-bar-used` — todas "barras de
progresso" CSS puras, não charts).

---

## 9. Resumo para quem vai portar

1. **Não existe 1 design system, existem ~9** (um por tela "nova"). Ao portar para componentes
   React reais, a primeira decisão de arquitetura é *unificar* KPI/badge/tabela/tab/drawer num único
   conjunto — o protótipo não faz isso, e replicar a fragmentação seria herdar dívida técnica de
   propósito.
2. **5 variáveis CSS usadas e nunca declaradas** (`--border-primary`, `--bg-primary`,
   `--bg-secondary`, `--surface-2`, `--surface-3`) — declare-as ou troque pelos tokens reais
   equivalentes (§2).
3. **3 implementações de drawer, 3 de timeline, 11 vocabulários de badge de status** — todos
   documentados acima com a localização exata; a escolha de qual vira o "componente oficial" é
   decisão de design, não deste levantamento.
4. **Só 2 telas usam Chart.js** (Visão 360, aba Pós-vendas); todo o resto de "gráfico" no protótipo
   é SVG à mão ou barra de progresso CSS — ao portar, decidir se tudo migra para uma única lib de
   gráficos (Chart.js, recharts, visx…) é decisão de implementação.
