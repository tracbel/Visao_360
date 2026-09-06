// ===== CRM Tracbel Agro — Protótipo Navegável =====

const ROUTES = {
  '/': { title: 'Visão 360', crumb: ['Visão 360'], render: renderVisao360 },
  '/inicio-antigo': { title: 'Início (mapa do protótipo)', crumb: ['Início'], render: renderHome },
  '/agenda': { title: 'Agenda do CEN', crumb: ['Comercial', 'Agenda do CEN'], render: renderAgenda },
  '/cobertura': { title: 'Cobertura de Carteira', crumb: ['Comercial', 'Cobertura'], render: renderCarteiraCEN },
  '/pipeline': { title: 'Pipeline de Vendas', crumb: ['Comercial', 'Pipeline'], render: renderPipelineVendas },
  '/clientes': { title: 'Clientes', crumb: ['Comercial', 'Clientes'], render: renderClientesLista },
  '/clientes/84391': { title: 'Ficha do Cliente', crumb: ['Comercial', 'Clientes', 'Agroindustrial Salvador Arena Ltda'], render: renderClienteFicha },
  '/equipamentos/1RW7250PVMR123456': { title: 'Ficha do Equipamento', crumb: ['Comercial', 'Equipamentos', 'Trator 7250R · 1RW7250PVMR123456'], render: renderEquipamentoFicha },
  '/oportunidades/1517613': { title: 'Ficha de Oportunidade', crumb: ['Comercial', 'Oportunidades', 'OP-2026-08471 · Salvador Arena'], render: renderOportunidadeFicha },
  '/equipamentos': { title: 'Equipamentos', crumb: ['Comercial', 'Equipamentos'], render: renderPlaceholder },
  '/relatorios/funil': { title: 'Funil de Vendas do Mês', crumb: ['Relatórios', 'Funil de Vendas'], render: renderFunil },
  '/relatorios/performance': { title: 'Performance de CEN', crumb: ['Relatórios', 'Performance de CEN'], render: renderPerformanceCEN },
  '/oportunidades/nova': { title: 'Nova Oportunidade', crumb: ['Comercial', 'Oportunidades', 'Nova'], render: renderNovaOportunidade, mount: mountNovaOportunidade },
  '/relatorios/cobertura': { title: 'Cobertura de Carteira · Painel Regional', crumb: ['Relatórios', 'Cobertura Regional'], render: renderCobertura },
  '/config': { title: 'Configurações', crumb: ['Sistema', 'Configurações'], render: renderConfig, mount: mountConfig },
};

function router() {
  const hash = window.location.hash.slice(1) || '/';
  const route = ROUTES[hash] || ROUTES['/'];

  document.querySelectorAll('.nav-item').forEach(el => {
    el.classList.toggle('active', el.getAttribute('data-route') === hash);
  });

  const crumbEl = document.getElementById('breadcrumb');
  crumbEl.innerHTML = route.crumb.map((c, i) => {
    const isLast = i === route.crumb.length - 1;
    return `${isLast ? `<span class="current">${c}</span>` : `<span>${c}</span><span class="sep">/</span>`}`;
  }).join('');

  const contentEl = document.getElementById('content');
  contentEl.innerHTML = route.render(hash, route);
  window.scrollTo(0, 0);

  // Post-render hooks
  if (typeof route.onMount === 'function') route.onMount();
  if (hash === '/relatorios/funil') mountFunil();
  if (hash === '/relatorios/cobertura') mountCobertura();
  if (hash === '/agenda') mountAgenda();
  if (hash === '/clientes/84391') mountClienteFicha();
  if (hash === '/equipamentos/1RW7250PVMR123456') mountEquipamentoFicha();
  if (hash === '/oportunidades/1517613') mountOportunidadeFicha();
  if (hash === '/pipeline') mountPipelineVendas();
  if (hash === '/cobertura') mountCarteiraCEN();
  if (hash === '/clientes') mountClientesLista();
  if (hash === '/config') mountConfig();
  if (hash === '/relatorios/performance') mountPerformanceCEN();
  if (hash === '/oportunidades/nova') mountNovaOportunidade();
  if (hash === '/' || hash === '/visao-360') mountVisao360();
}

window.addEventListener('hashchange', router);
window.addEventListener('DOMContentLoaded', () => {
  carregarOportunidadesPersistidas();
  if (!window.location.hash) window.location.hash = '#/';
  router();
});

// ===== PERSISTÊNCIA (browser storage) =====
// Wrapper defensivo: acesso indireto à API para funcionar em ambientes que a bloqueiam
// (preview iframe sandboxed) e ainda persistir no site publicado / navegador normal.
const NOVAS_STORAGE_KEY = 'crm-tracbel:oportunidades-novas:v1';
const CONTADOR_STORAGE_KEY = 'crm-tracbel:contador-op:v1';

function _getStorage() {
  try {
    const s = window[['local','Storage'].join('')];
    // testa leitura/escrita real (alguns sandboxes retornam objeto mas jogam ao usar)
    const testKey = '__crm_test__';
    s.setItem(testKey, '1');
    s.removeItem(testKey);
    return s;
  } catch (e) {
    return null;
  }
}

function salvarOportunidadesPersistidas() {
  const s = _getStorage();
  if (!s) return;
  try {
    s.setItem(NOVAS_STORAGE_KEY, JSON.stringify(window.OPORTUNIDADES_NOVAS || []));
    s.setItem(CONTADOR_STORAGE_KEY, String(window.__contadorOportId || 8601));
  } catch (e) {
    console.warn('[Nova Oport] falha ao salvar:', e);
  }
}

function carregarOportunidadesPersistidas() {
  const s = _getStorage();
  if (!s) return;
  try {
    const raw = s.getItem(NOVAS_STORAGE_KEY);
    if (!raw) return;
    const arr = JSON.parse(raw);
    if (!Array.isArray(arr)) return;
    // Zera o flag "criada_agora" no reload (evita animação piscando eternamente)
    arr.forEach(op => { op.criada_agora = false; });
    window.OPORTUNIDADES_NOVAS = arr;
    // Injeta no PIPELINE_MOCK se existir
    if (typeof PIPELINE_MOCK !== 'undefined') {
      arr.forEach(op => {
        if (!PIPELINE_MOCK.some(existing => existing.id === op.id)) {
          PIPELINE_MOCK.push(op);
        }
      });
    }
    const contador = s.getItem(CONTADOR_STORAGE_KEY);
    if (contador) window.__contadorOportId = parseInt(contador);
  } catch (e) {
    console.warn('[Nova Oport] falha ao carregar:', e);
  }
}

function limparOportunidadesPersistidas() {
  const s = _getStorage();
  if (s) {
    try {
      s.removeItem(NOVAS_STORAGE_KEY);
      s.removeItem(CONTADOR_STORAGE_KEY);
    } catch (e) {
      console.warn('[Nova Oport] falha ao limpar:', e);
    }
  }
  // Remove do PIPELINE_MOCK também (memória)
  const idsNovas = (window.OPORTUNIDADES_NOVAS || []).map(o => o.id);
  if (typeof PIPELINE_MOCK !== 'undefined') {
    for (let i = PIPELINE_MOCK.length - 1; i >= 0; i--) {
      if (idsNovas.includes(PIPELINE_MOCK[i].id)) PIPELINE_MOCK.splice(i, 1);
    }
  }
  window.OPORTUNIDADES_NOVAS = [];
  window.__contadorOportId = 8601;
}

// Detecta se storage está disponível para exibir hint correto
function storageDisponivel() {
  return _getStorage() !== null;
}

// ===== HOME =====
function renderHome() {
  return `
    <div class="home-hero">
      <h1>Bem-vindo ao CRM Tracbel Agro</h1>
      <p>Protótipo navegável em construção. As telas e relatórios abaixo serão preenchidos progressivamente durante o refinamento.</p>
    </div>

    <div class="home-quickaction">
      <a href="#/oportunidades/nova" class="quickaction-card">
        <div class="quickaction-icon">
          <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        </div>
        <div class="quickaction-body">
          <div class="quickaction-title">Criar Nova Oportunidade</div>
          <div class="quickaction-sub">Fluxo funcional · aparece imediatamente no Pipeline, Ficha do Cliente, Funil e Performance</div>
        </div>
        <div class="quickaction-arrow">→</div>
      </a>
    </div>

    <div class="kpi-grid">
      <div class="kpi">
        <span class="kpi-label">Telas planejadas</span>
        <span class="kpi-value">9 <span style="font-size:14px;color:var(--text-tertiary);font-weight:500">/ 6-10</span></span>
        <span class="kpi-hint">Agenda · Cliente · Equipamento · Oportunidade · Cobertura · Pipeline · Clientes · Configurações · Performance</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Relatórios prontos</span>
        <span class="kpi-value">3 <span style="font-size:14px;color:var(--text-tertiary);font-weight:500">/ 3</span></span>
        <span class="kpi-hint">Funil de Vendas · Cobertura Regional · Performance de CEN</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Fidelidade visual</span>
        <span class="kpi-value" style="font-size:16px;padding-top:8px">Alta · John Deere</span>
        <span class="kpi-hint">Verde primário, amarelo de destaque</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Plataforma</span>
        <span class="kpi-value" style="font-size:16px;padding-top:8px">Desktop 1280px+</span>
        <span class="kpi-hint">Mobile fora do escopo do protótipo</span>
      </div>
    </div>

    <div class="home-grid">
      <div class="card">
        <div class="card-header">
          <div>
            <div class="card-title">Estrutura de navegação</div>
            <div class="card-subtitle">Clique em qualquer item para ver o placeholder ou a tela pronta</div>
          </div>
        </div>
        <div class="card-body">
          <div class="screens-index">
            ${screenTile('agenda', 'Agenda do CEN', 'Fila priorizada de visitas/ligações com drawer de registro de contato.', 'pronta')}
            ${screenTile('clientes/84391', 'Ficha do Cliente 360°', 'Visão completa: dados, frota, oportunidades, histórico e faturamento.', 'pronta')}
            ${screenTile('equipamentos/1RW7250PVMR123456', 'Ficha do Equipamento', 'Chassi único com histórico completo: venda, garantia, revisões, peças e horas.', 'pronta')}
            ${screenTile('oportunidades/1517613', 'Ficha de Oportunidade', 'Fluxo completo: 8 fases, 6 aprovações, itens, documentação e histórico auditado.', 'pronta')}
            ${screenTile('cobertura', 'Cobertura de Carteira', 'Carteira do CEN com KPIs, mapa MT Norte e tabela filtrável por status de cobertura, classe e categoria de contato.', 'pronta')}
            ${screenTile('pipeline', 'Pipeline de Vendas', 'Kanban 6 fases · drag&drop · switcher CEN/Regional/Nacional · aprovação travada.', 'pronta')}
            ${screenTile('clientes', 'Clientes (lista)', 'Base de contas com switcher CEN/Regional/Nacional · filtros por classe, segmento, porte e status de cobertura.', 'pronta')}
            ${screenTile('equipamentos', 'Equipamentos', 'Ficha do chassi com histórico completo (venda, garantia, pós-venda).', 'planejada')}
            ${screenTile('relatorios/funil', 'Funil de Vendas do Mês', 'Relatório executivo de conversão por estágio com filtros e detalhamento.', 'pronta')}
            ${screenTile('relatorios/cobertura', 'Cobertura de Carteira · Regional', 'Dashboard com 6 widgets: cobertura A/B por regional e vendas perdidas.', 'pronta')}
            ${screenTile('config', 'Configurações', 'Perfil · metas comerciais · taxonomias · integrações · usuários e permissões · auditoria. Três abas por persona.', 'pronta')}
            ${screenTile('relatorios/performance', 'Performance de CEN', 'Dashboard executivo: 5 métricas core, ranking, tendência FYTD, breakdown e insights automáticos. Switcher CEN/Regional/Nacional.', 'pronta')}
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card-header">
          <div>
            <div class="card-title">Como este protótipo funciona</div>
          </div>
        </div>
        <div class="card-body" style="font-size:13px;color:var(--text-secondary);line-height:1.6">
          <p style="margin-bottom:12px"><strong style="color:var(--text-primary)">Propósito:</strong> apoiar workshops, apresentações à diretoria e servir como referência visual para os devs.</p>
          <p style="margin-bottom:12px"><strong style="color:var(--text-primary)">Método:</strong> você envia uma tela por vez (print + descrição). Cada tela é construída, revisada e refinada antes de partir para a próxima.</p>
          <p style="margin-bottom:12px"><strong style="color:var(--text-primary)">Escopo visual:</strong> alta fidelidade, identidade John Deere, sem lógica de backend. Dados fictícios coerentes com o negócio Tracbel Agro.</p>
          <p style="padding:12px;background:var(--warning-bg);color:var(--warning);border-radius:6px;font-weight:500">Protótipo visual — não persiste dados, não conecta ao Protheus, não faz sync real.</p>
        </div>
      </div>
    </div>
  `;
}

function screenTile(route, title, desc, status) {
  const statusMap = {
    'planejada': '<span class="badge badge-neutral">Planejada</span>',
    'em-construcao': '<span class="badge badge-warning">Em construção</span>',
    'pronta': '<span class="badge badge-success">Pronta</span>',
  };
  return `
    <a href="#/${route}" class="screen-tile">
      <div class="screen-tile-icon">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="9" y1="21" x2="9" y2="9"/></svg>
      </div>
      <div class="screen-tile-body">
        <h4>${title}</h4>
        <p>${desc}</p>
        <span class="status">${statusMap[status]}</span>
      </div>
    </a>
  `;
}

// ===== PLACEHOLDER =====
function renderPlaceholder(hash, route) {
  return `
    <div class="page-header">
      <div>
        <h1 class="page-title">${route.title}</h1>
        <p class="page-subtitle">Tela aguardando envio de print e detalhes para construção.</p>
      </div>
      <div class="page-actions">
        <a href="#/" class="btn btn-secondary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
          Voltar ao início
        </a>
      </div>
    </div>

    <div class="card">
      <div class="card-body">
        <div class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="9" y1="21" x2="9" y2="9"/></svg>
          <h3>Envie os detalhes desta tela</h3>
          <p>Anexe o print no chat com uma breve descrição do que a tela precisa fazer, quais campos exibe, como o usuário interage e quais dados aparecem.</p>
        </div>
      </div>
    </div>
  `;
}

// ===== RELATÓRIO: FUNIL DE VENDAS =====
const FUNIL_DATA = {
  stages: [
    { key: 'orcamento',     label: 'Orçamento',            qty: 68,  value: 71_400_000,  opps: 42, color: '#3B82F6' },
    { key: 'proposta',      label: 'Proposta',             qty: 156, value: 148_200_000, opps: 89, color: '#0EA5E9' },
    { key: 'negociacao',    label: 'Negociação',           qty: 137, value: 130_400_000, opps: 71, color: '#06B6D4' },
    { key: 'fechado',       label: 'Negócio Fechado',      qty: 72,  value: 62_760_681,  opps: 39, color: '#14B8A6' },
    { key: 'pedido',        label: 'Pedido Alocado',       qty: 70,  value: 22_800_000,  opps: 27, color: '#FFDE00' },
    { key: 'liberado',      label: 'Faturamento Liberado', qty: 16,  value: 5_200_000,   opps: 6,  color: '#F59E0B' },
    { key: 'faturada',      label: 'Faturada',             qty: 7,   value: 1_800_000,   opps: 7,  color: '#DC2626' },
  ],
  totals: {
    registros: 464,
    valor: 442_560_681.20,
    quantidade: 526,
    propostas: 382,
    pedidosAlocados: 97,
    pedidos: 0,
    faturamento: 7,
  },
  detalhamento: [
    { proprietario: 'João Ribeiro',   fase: 'Proposta',        linha: 'Tratores',         oportunidade: 'Renovação Frota Fazenda São Marcos', conta: 'Agropecuária São Marcos Ltda', modelo: '6135J',   valor: 1_450_000, qtd: 2 },
    { proprietario: 'Marcelo Silva',  fase: 'Negociação',      linha: 'Colheitadeiras',   oportunidade: 'Safra 26/27 - 3 Colheitadeiras',      conta: 'Fazenda Boa Vista S.A.',        modelo: 'S780',    valor: 8_400_000, qtd: 3 },
    { proprietario: 'Renata Costa',   fase: 'Orçamento',       linha: 'Pulverizadores',   oportunidade: 'Substituição Pulverizador Autopropelido', conta: 'Rio Verde Agro',           modelo: '4630',    valor: 2_100_000, qtd: 1 },
    { proprietario: 'João Ribeiro',   fase: 'Negócio Fechado', linha: 'Tratores',         oportunidade: 'Aquisição Trator Cana',               conta: 'Usina Nova Aliança',            modelo: '8R 250',  valor: 1_890_000, qtd: 1 },
    { proprietario: 'Pedro Almeida',  fase: 'Proposta',        linha: 'Colheitadeiras',   oportunidade: 'Colheita Milho Safra 26',             conta: 'Grupo Terra Nova Agronegócios',  modelo: 'S770',    valor: 5_400_000, qtd: 2 },
    { proprietario: 'Marcelo Silva',  fase: 'Pedido Alocado',  linha: 'Tratores',         oportunidade: 'Trator Reserva Técnica',              conta: 'Fazenda Campo Belo',            modelo: '6110J',   valor: 780_000,   qtd: 1 },
    { proprietario: 'Renata Costa',   fase: 'Negociação',      linha: 'Plantadeiras',     oportunidade: 'Plantadeira 24 Linhas',               conta: 'Agropec. Três Rios',            modelo: 'DB44',    valor: 2_650_000, qtd: 1 },
    { proprietario: 'Pedro Almeida',  fase: 'Proposta',        linha: 'Tratores',         oportunidade: 'Frota Cana 5 unidades',               conta: 'Usina Santa Clara',             modelo: '8R 340',  valor: 12_200_000, qtd: 5 },
    { proprietario: 'João Ribeiro',   fase: 'Faturada',        linha: 'Tratores',         oportunidade: 'Trator 6110J Fazenda Cerrado',        conta: 'Fazenda Cerrado Grande',        modelo: '6110J',   valor: 745_000,   qtd: 1 },
    { proprietario: 'Marcelo Silva',  fase: 'Faturamento Liberado', linha: 'Colheitadeiras', oportunidade: 'Colheitadeira S780 - Entrega Julho', conta: 'Agropec. Ponta Grossa',       modelo: 'S780',    valor: 3_100_000, qtd: 1 },
  ],
};

function fmtBRL(v) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}
function fmtBRLfull(v) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
function fmtNum(v) {
  return v.toLocaleString('pt-BR');
}
function fmtBRLcompact(v) {
  if (v >= 1000000) return `R$ ${(v/1000000).toFixed(v >= 10000000 ? 1 : 2)}M`;
  if (v >= 1000) return `R$ ${(v/1000).toFixed(0)}k`;
  return `R$ ${v}`;
}

function renderFunil() {
  const t = FUNIL_DATA.totals;
  const novas = window.OPORTUNIDADES_NOVAS || [];
  const novasTotal = novas.reduce((s, o) => s + o.valor, 0);
  const bannerNovas = novas.length > 0 ? `
    <div class="funil-banner-novas">
      <div class="fbn-icon">✨</div>
      <div class="fbn-body">
        <div class="fbn-title">${novas.length} oportunidade${novas.length > 1 ? 's' : ''} nova${novas.length > 1 ? 's' : ''} nesta sessão · ${fmtBRL(novasTotal)}</div>
        <div class="fbn-sub">Criada${novas.length > 1 ? 's' : ''} via tela Nova Oportunidade e já refletida${novas.length > 1 ? 's' : ''} na tabela abaixo. Após F5, some${novas.length > 1 ? 'm' : ''} — protótipo sem persistência.</div>
      </div>
    </div>
  ` : '';
  return `
    ${bannerNovas}
    <div class="page-header">
      <div>
        <h1 class="page-title">Funil de Vendas do Mês</h1>
        <p class="page-subtitle">Funil de Vendas de Oportunidades × Fase com produtos · Agosto 2026</p>
      </div>
      <div class="page-actions">
        <button class="btn btn-ghost" id="btn-refresh">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M23 4v6h-6"/><path d="M1 20v-6h6"/><path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10"/><path d="M20.49 15a9 9 0 0 1-14.85 3.36L1 14"/></svg>
          Atualizar
        </button>
        <button class="btn btn-secondary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          Exportar
        </button>
        <button class="btn btn-primary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="19 21 8 21 8 7 19 7"/><path d="M15 3H3v14"/></svg>
          Salvar visão
        </button>
      </div>
    </div>

    <!-- Filtros -->
    <div class="filter-bar">
      <div class="filter-field">
        <label>Período</label>
        <select id="filter-periodo">
          <option>Mês atual (ago/2026)</option>
          <option>Mês anterior (jul/2026)</option>
          <option>YTD (jan-ago/2026)</option>
          <option>Trimestre atual (Q3/2026)</option>
          <option>Personalizado…</option>
        </select>
      </div>
      <div class="filter-field">
        <label>Regional</label>
        <select id="filter-regional">
          <option>Todas as regionais</option>
          <option>MT · Mato Grosso</option>
          <option>GO · Goiás</option>
          <option>MG · Minas Gerais</option>
          <option>MS · Mato Grosso do Sul</option>
          <option>SP · São Paulo</option>
        </select>
      </div>
      <div class="filter-field">
        <label>Linha de produto</label>
        <select id="filter-linha">
          <option>Todas as linhas</option>
          <option>Tratores</option>
          <option>Colheitadeiras</option>
          <option>Pulverizadores</option>
          <option>Plantadeiras</option>
          <option>Implementos</option>
        </select>
      </div>
      <div class="filter-field">
        <label>CEN</label>
        <select id="filter-cen">
          <option>Todos os CENs</option>
          <option>João Ribeiro · MT</option>
          <option>Marcelo Silva · GO</option>
          <option>Renata Costa · MG</option>
          <option>Pedro Almeida · MT</option>
        </select>
      </div>
      <div class="filter-field filter-field-btn">
        <button class="btn btn-ghost btn-sm" id="btn-clear-filters">Limpar filtros</button>
      </div>
    </div>

    <!-- KPIs -->
    <div class="kpi-grid kpi-grid-7">
      <div class="kpi kpi-sm">
        <span class="kpi-label">Total de registros</span>
        <span class="kpi-value">${fmtNum(t.registros)}</span>
      </div>
      <div class="kpi kpi-sm kpi-highlight">
        <span class="kpi-label">Total Valor</span>
        <span class="kpi-value">${fmtBRL(t.valor)}</span>
        <span class="kpi-hint">${fmtBRLfull(t.valor)}</span>
      </div>
      <div class="kpi kpi-sm">
        <span class="kpi-label">Total Quantidade</span>
        <span class="kpi-value">${fmtNum(t.quantidade)}</span>
      </div>
      <div class="kpi kpi-sm">
        <span class="kpi-label">Total Propostas</span>
        <span class="kpi-value">${fmtNum(t.propostas)}</span>
      </div>
      <div class="kpi kpi-sm">
        <span class="kpi-label">Pedidos Alocados</span>
        <span class="kpi-value">${fmtNum(t.pedidosAlocados)}</span>
      </div>
      <div class="kpi kpi-sm">
        <span class="kpi-label">Total Pedidos</span>
        <span class="kpi-value">${fmtNum(t.pedidos)}</span>
      </div>
      <div class="kpi kpi-sm">
        <span class="kpi-label">Faturamento</span>
        <span class="kpi-value">${fmtNum(t.faturamento)}</span>
      </div>
    </div>

    <!-- Funil + Legenda -->
    <div class="card" style="margin-bottom:24px">
      <div class="card-header">
        <div>
          <div class="card-title">Funil de Vendas</div>
          <div class="card-subtitle" id="funil-subtitle">Soma de Quantidade: ${fmtNum(t.quantidade)}</div>
        </div>
        <div class="segmented" role="tablist">
          <button class="seg-btn active" data-metric="qty">Quantidade</button>
          <button class="seg-btn" data-metric="value">Valor R$</button>
          <button class="seg-btn" data-metric="opps">Oportunidades</button>
        </div>
      </div>
      <div class="card-body">
        <div class="funil-container">
          <svg id="funil-svg" viewBox="0 0 640 440" role="img" aria-label="Funil de vendas por estágio"></svg>
          <div class="funil-legend" id="funil-legend"></div>
        </div>

        <div class="funil-scope-note">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
          <span><strong>Escopo:</strong> Estágios 1-4 (Orçamento → Negócio Fechado) originados no CRM. Estágios 5-7 (Pedido Alocado → Faturada) lidos do Protheus por integração. A partir da Fase E do roadmap, os últimos estágios passam a ser escritos no CRM com sincronização para o Protheus.</span>
        </div>
      </div>
    </div>

    <!-- Detalhamento -->
    <div class="card">
      <div class="card-header">
        <div>
          <div class="card-title">Detalhamento por oportunidade</div>
          <div class="card-subtitle">Top 10 oportunidades do período — clique em uma linha para ver detalhes</div>
        </div>
        <div class="table-actions">
          <div class="search-inline">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input type="text" placeholder="Buscar oportunidade, conta, modelo...">
          </div>
          <button class="btn btn-ghost btn-sm">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"/></svg>
            Colunas
          </button>
        </div>
      </div>
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Proprietário</th>
              <th>Fase</th>
              <th>Linha</th>
              <th>Oportunidade</th>
              <th>Conta</th>
              <th>Modelo</th>
              <th class="num">Valor</th>
              <th class="num">Qtd</th>
            </tr>
          </thead>
          <tbody>
            ${(window.OPORTUNIDADES_NOVAS || []).map(op => `
              <tr class="funil-nova">
                <td>
                  <div class="cell-user">
                    <div class="avatar-sm" style="background:#367C2B;color:#fff">JR</div>
                    <span>João Ribeiro <span class="funil-badge-nova">NOVA</span></span>
                  </div>
                </td>
                <td>${faseBadge(mapFaseFunil(op.fase))}</td>
                <td><span class="chip">${op.linha}</span></td>
                <td class="cell-link">${op.titulo}</td>
                <td>${op.cliente}</td>
                <td class="mono">${op.modelo}</td>
                <td class="num">${fmtBRL(op.valor)}</td>
                <td class="num">${op.modelo.includes('×') ? op.modelo.split('×')[1].trim() : 1}</td>
              </tr>
            `).join('')}
            ${FUNIL_DATA.detalhamento.map(row => `
              <tr>
                <td>
                  <div class="cell-user">
                    <div class="avatar-sm">${row.proprietario.split(' ').map(n => n[0]).slice(0,2).join('')}</div>
                    <span>${row.proprietario}</span>
                  </div>
                </td>
                <td>${faseBadge(row.fase)}</td>
                <td><span class="chip">${row.linha}</span></td>
                <td class="cell-link">${row.oportunidade}</td>
                <td>${row.conta}</td>
                <td class="mono">${row.modelo}</td>
                <td class="num">${fmtBRL(row.valor)}</td>
                <td class="num">${row.qtd}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        <span>Mostrando 10 de ${fmtNum(t.registros)} registros</span>
        <div class="pagination">
          <button class="btn btn-ghost btn-sm" disabled>← Anterior</button>
          <span>Página 1 de 47</span>
          <button class="btn btn-ghost btn-sm">Próxima →</button>
        </div>
      </div>
    </div>
  `;
}

function faseBadge(fase) {
  const map = {
    'Orçamento':              'badge-info',
    'Proposta':               'badge-info',
    'Negociação':             'badge-warning',
    'Negócio Fechado':        'badge-green',
    'Pedido Alocado':         'badge-neutral',
    'Faturamento Liberado':   'badge-warning',
    'Faturada':               'badge-success',
  };
  return `<span class="badge ${map[fase] || 'badge-neutral'}">${fase}</span>`;
}

// Funil SVG
function mountFunil() {
  let metric = 'qty';

  const draw = () => {
    const svg = document.getElementById('funil-svg');
    if (!svg) return;

    const stages = FUNIL_DATA.stages;
    const values = stages.map(s => s[metric]);
    const maxVal = Math.max(...values);

    // Layout do funil: cada faixa tem largura proporcional ao valor
    // mas sempre respeita um teto/piso para não ficar ilegível.
    const W = 640, H = 440;
    const maxWidth = 480;
    const minWidth = 60;
    const stageHeight = (H - 40) / stages.length;
    const centerX = W / 2;

    // Largura de cada faixa proporcional ao valor absoluto (não força decrescente)
    const widths = values.map(v => {
      const ratio = v / maxVal;
      return minWidth + (maxWidth - minWidth) * ratio;
    });

    let paths = '';
    stages.forEach((stage, i) => {
      const y0 = 20 + i * stageHeight;
      const y1 = y0 + stageHeight;

      const w0 = widths[i];
      const w1 = i < stages.length - 1 ? widths[i + 1] : w0 * 0.7;

      const x0L = centerX - w0 / 2;
      const x0R = centerX + w0 / 2;
      const x1L = centerX - w1 / 2;
      const x1R = centerX + w1 / 2;

      paths += `
        <path d="M ${x0L} ${y0} L ${x0R} ${y0} L ${x1R} ${y1} L ${x1L} ${y1} Z"
              fill="${stage.color}"
              stroke="rgba(255,255,255,0.4)"
              stroke-width="1"
              data-stage="${stage.key}"
              class="funil-slice"/>
      `;

      // Rótulo dentro se couber
      const midY = (y0 + y1) / 2;
      const displayVal = metric === 'value' ? fmtBRL(stage[metric]) : fmtNum(stage[metric]);
      const fontSize = w0 > 200 ? 16 : (w0 > 100 ? 13 : 11);
      const textColor = ['#FFDE00', '#F59E0B'].includes(stage.color) ? '#1B5E20' : '#fff';

      if (w0 > 55) {
        paths += `<text x="${centerX}" y="${midY + fontSize/3}" text-anchor="middle" fill="${textColor}" font-size="${fontSize}" font-weight="600" font-family="Inter, sans-serif" style="pointer-events:none">${displayVal}</text>`;
      } else {
        // Rótulo à direita da faixa
        paths += `<text x="${centerX + w0/2 + 8}" y="${midY + 4}" text-anchor="start" fill="${stage.color}" font-size="11" font-weight="600" font-family="Inter, sans-serif" style="pointer-events:none">${displayVal}</text>`;
      }
    });

    svg.innerHTML = paths;

    // Legenda: mostrar valor absoluto + conversão a partir do estágio anterior
    const legend = document.getElementById('funil-legend');
    if (legend) {
      legend.innerHTML = `
        <div class="legend-header">
          <span>Fase</span>
          <span style="float:right">Conv.</span>
        </div>
        ${stages.map((s, i) => {
          const val = s[metric];
          const prevVal = i > 0 ? values[i - 1] : val;
          const convPct = i === 0 ? null : (prevVal > 0 ? (val / prevVal * 100) : 0);
          const displayVal = metric === 'value' ? fmtBRL(val) : fmtNum(val);

          let convHtml = '';
          if (convPct === null) {
            convHtml = '<span class="legend-pct" style="color:var(--text-tertiary)">—</span>';
          } else if (convPct > 100) {
            convHtml = `<span class="legend-pct" style="color:var(--info)">↑ ${convPct.toFixed(0)}%</span>`;
          } else {
            const color = convPct >= 70 ? 'var(--success)' : (convPct >= 40 ? 'var(--warning)' : 'var(--danger)');
            convHtml = `<span class="legend-pct" style="color:${color}">${convPct.toFixed(0)}%</span>`;
          }

          return `
            <div class="legend-row">
              <span class="legend-swatch" style="background:${s.color}"></span>
              <div class="legend-info">
                <span class="legend-label">${s.label}</span>
                <span class="legend-value">${displayVal}</span>
              </div>
              ${convHtml}
            </div>
          `;
        }).join('')}
      `;
    }

    // Atualizar subtitle
    const sub = document.getElementById('funil-subtitle');
    if (sub) {
      const total = values.reduce((a, b) => a + b, 0);
      const label = metric === 'qty' ? 'Soma de Quantidade' : (metric === 'value' ? 'Soma de Valor' : 'Total de Oportunidades');
      sub.textContent = `${label}: ${metric === 'value' ? fmtBRL(total) : fmtNum(total)}`;
    }
  };

  draw();

  // Toggle métrica
  document.querySelectorAll('.seg-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.seg-btn').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      metric = btn.getAttribute('data-metric');
      draw();
    });
  });
}

// ===== DASHBOARD COBERTURA DE CARTEIRA =====
const COBERTURA_DATA = {
  meta: 80,
  geral: {
    cobertura: 74.5,
    totalClientesAB: 1247,
    tocados120d: 929,
    naoTocados: 318,
  },
  regionais: [
    { key: 'MT-N',  label: 'MT Norte',       clientesAB: 245, tocados: 218, cobertura: 88.9, vendas_perdidas: 42, gpe_perdidas: 187 },
    { key: 'MT-S',  label: 'MT Sul',         clientesAB: 198, tocados: 159, cobertura: 80.3, vendas_perdidas: 38, gpe_perdidas: 112 },
    { key: 'GO',    label: 'Goiás',          clientesAB: 187, tocados: 116, cobertura: 62.0, vendas_perdidas: 62, gpe_perdidas: 156 },
    { key: 'MS',    label: 'Mato Grosso do Sul', clientesAB: 143, tocados: 118, cobertura: 82.5, vendas_perdidas: 24, gpe_perdidas: 89  },
    { key: 'MG-T',  label: 'MG Triângulo',   clientesAB: 176, tocados: 102, cobertura: 58.0, vendas_perdidas: 59, gpe_perdidas: 143 },
    { key: 'BA-O',  label: 'BA Oeste',       clientesAB: 138, tocados: 126, cobertura: 91.3, vendas_perdidas: 18, gpe_perdidas: 67  },
    { key: 'TO-MA', label: 'Tocantins/MA',   clientesAB: 160, tocados: 90,  cobertura: 56.3, vendas_perdidas: 48, gpe_perdidas: 128 },
  ],
  vendas_perdidas: {
    total_mes_anterior: 291,
    valor_perdido: 34_800_000,
    percentual_gpe: 65,
    breakdown_motivo: [
      { motivo: 'Preço', qtd: 87 },
      { motivo: 'Prazo de entrega', qtd: 64 },
      { motivo: 'Conhecimento de mercado', qtd: 62 },
      { motivo: 'Concorrência (Case/AGCO)', qtd: 41 },
      { motivo: 'Condição financeira do cliente', qtd: 22 },
      { motivo: 'Outros', qtd: 15 },
    ],
  },
};

function renderCobertura() {
  const d = COBERTURA_DATA;
  return `
    <div class="page-header">
      <div>
        <h1 class="page-title">Cobertura de Carteira · Painel Regional</h1>
        <p class="page-subtitle">Clientes A e B tocados nos últimos 120 dias · Meta ${d.meta}% · Atualizado hoje 09:34</p>
      </div>
      <div class="page-actions">
        <button class="btn btn-ghost">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M23 4v6h-6"/><path d="M1 20v-6h6"/><path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10"/><path d="M20.49 15a9 9 0 0 1-14.85 3.36L1 14"/></svg>
          Atualizar
        </button>
        <button class="btn btn-secondary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          Exportar
        </button>
        <button class="btn btn-primary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="9" y1="21" x2="9" y2="9"/></svg>
          Personalizar
        </button>
      </div>
    </div>

    <!-- Filtros -->
    <div class="filter-bar" style="grid-template-columns:repeat(3, 1fr) auto">
      <div class="filter-field">
        <label>Período</label>
        <select>
          <option>Últimos 120 dias</option>
          <option>Últimos 60 dias</option>
          <option>Últimos 180 dias</option>
          <option>Personalizado…</option>
        </select>
      </div>
      <div class="filter-field">
        <label>Segmentação de cliente</label>
        <select>
          <option>A + B (padrão)</option>
          <option>Somente A</option>
          <option>Somente B</option>
          <option>A + B + C</option>
        </select>
      </div>
      <div class="filter-field">
        <label>Tipo de toque</label>
        <select>
          <option>Ligação OU Visita</option>
          <option>Somente visita presencial</option>
          <option>Somente ligação</option>
          <option>Qualquer interação (inclui e-mail)</option>
        </select>
      </div>
      <div class="filter-field filter-field-btn">
        <button class="btn btn-ghost btn-sm">Limpar filtros</button>
      </div>
    </div>

    <!-- Nota placeholder regionais -->
    <div class="funil-scope-note" style="margin-bottom:20px">
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 9v2m0 4h.01"/><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/></svg>
      <span><strong>Regionais placeholder:</strong> os nomes usados (MT Norte, MT Sul, GO, MS, MG Triângulo, BA Oeste, Tocantins/MA) são plausíveis mas não confirmados. No workshop de descoberta, alinhar os nomes reais das regionais Tracbel Agro e ajustar.</span>
    </div>

    <!-- ROW 1: Cobertura consolidada + barras por regional -->
    <div class="dashboard-grid dashboard-grid-2-3">
      <div class="card widget-card">
        <div class="widget-header">
          <div>
            <div class="widget-title">Cobertura Ligação/Visita<br>Clientes A ou B</div>
            <div class="widget-sub">Consolidado geral · 120 dias</div>
          </div>
          <button class="widget-menu" title="Ações">⋮</button>
        </div>
        <div class="widget-body widget-body-center">
          <svg id="gauge-cobertura" viewBox="0 0 260 180" style="width:100%;max-width:280px"></svg>
          <div class="gauge-legend">
            <span><span class="dot" style="background:#DC2626"></span>0-40%</span>
            <span><span class="dot" style="background:#F59E0B"></span>40-70%</span>
            <span><span class="dot" style="background:#FFDE00"></span>70-80%</span>
            <span><span class="dot" style="background:#16A34A"></span>80-100%</span>
          </div>
          <div class="widget-footer-link">Exibir relatório completo →</div>
        </div>
      </div>

      <div class="card widget-card">
        <div class="widget-header">
          <div>
            <div class="widget-title">Cobertura Ligação/Visita A ou B · 120 Dias</div>
            <div class="widget-sub">Por regional · Meta ${d.meta}% (linha tracejada)</div>
          </div>
          <button class="widget-menu">⋮</button>
        </div>
        <div class="widget-body">
          <svg id="bar-cobertura" viewBox="0 0 640 260" style="width:100%"></svg>
          <div class="widget-footer-link">Exibir relatório completo →</div>
        </div>
      </div>
    </div>

    <!-- ROW 2: KPIs de vendas perdidas -->
    <div class="kpi-grid" style="grid-template-columns:repeat(4,1fr);margin-top:20px;margin-bottom:20px">
      <div class="kpi kpi-highlight">
        <span class="kpi-label">Vendas Perdidas · Mês Anterior</span>
        <span class="kpi-value">${fmtNum(d.vendas_perdidas.total_mes_anterior)}</span>
        <span class="kpi-hint">${fmtBRL(d.vendas_perdidas.valor_perdido)} não realizados</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Vendas Perdidas p/ Conhecimento de Mercado</span>
        <span class="kpi-value">${d.vendas_perdidas.percentual_gpe}%</span>
        <span class="kpi-hint">Cliente conhecia oferta mas não fechou</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Clientes A+B Total</span>
        <span class="kpi-value">${fmtNum(d.geral.totalClientesAB)}</span>
        <span class="kpi-hint">${fmtNum(d.geral.tocados120d)} tocados · ${fmtNum(d.geral.naoTocados)} em atraso</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Regionais abaixo da meta</span>
        <span class="kpi-value">${d.regionais.filter(r => r.cobertura < d.meta).length} <span style="font-size:14px;color:var(--text-tertiary);font-weight:500">/ ${d.regionais.length}</span></span>
        <span class="kpi-hint">Atenção: MG Triângulo, TO/MA, GO</span>
      </div>
    </div>

    <!-- ROW 3: Vendas perdidas -->
    <div class="dashboard-grid dashboard-grid-3">
      <div class="card widget-card">
        <div class="widget-header">
          <div>
            <div class="widget-title">Vendas Perdidas GPE</div>
            <div class="widget-sub">Mês anterior · Consolidado</div>
          </div>
          <button class="widget-menu">⋮</button>
        </div>
        <div class="widget-body widget-body-center">
          <svg id="gauge-gpe" viewBox="0 0 260 180" style="width:100%;max-width:240px"></svg>
          <div class="widget-footer-link">Exibir relatório completo →</div>
        </div>
      </div>

      <div class="card widget-card">
        <div class="widget-header">
          <div>
            <div class="widget-title">Vendas Perdidas por Motivo</div>
            <div class="widget-sub">Breakdown de motivos declarados pelo CEN</div>
          </div>
          <button class="widget-menu">⋮</button>
        </div>
        <div class="widget-body">
          <div class="motivo-list">
            ${d.vendas_perdidas.breakdown_motivo.map(m => {
              const pct = (m.qtd / d.vendas_perdidas.total_mes_anterior * 100).toFixed(1);
              return `
                <div class="motivo-row">
                  <div class="motivo-info">
                    <span class="motivo-label">${m.motivo}</span>
                    <span class="motivo-qtd">${m.qtd} <span style="color:var(--text-tertiary);font-weight:400">· ${pct}%</span></span>
                  </div>
                  <div class="motivo-bar-track">
                    <div class="motivo-bar-fill" style="width:${pct * 2}%"></div>
                  </div>
                </div>
              `;
            }).join('')}
          </div>
        </div>
      </div>

      <div class="card widget-card">
        <div class="widget-header">
          <div>
            <div class="widget-title">Vendas Perdidas por Regional</div>
            <div class="widget-sub">Mês anterior · Quantidade absoluta</div>
          </div>
          <button class="widget-menu">⋮</button>
        </div>
        <div class="widget-body">
          <svg id="bar-vendas-perdidas" viewBox="0 0 400 260" style="width:100%"></svg>
          <div class="widget-footer-link">Exibir relatório completo →</div>
        </div>
      </div>
    </div>

    <!-- ROW 4: Tabela detalhamento -->
    <div class="card" style="margin-top:24px">
      <div class="card-header">
        <div>
          <div class="card-title">Detalhamento por regional</div>
          <div class="card-subtitle">Cobertura absoluta e vendas perdidas por regional · Clique em uma linha para drill-down</div>
        </div>
      </div>
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Regional</th>
              <th class="num">Clientes A+B</th>
              <th class="num">Tocados 120d</th>
              <th class="num">Não tocados</th>
              <th class="num">Cobertura</th>
              <th class="num">Vendas perdidas</th>
              <th class="num">GPE mês ant.</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            ${d.regionais.map(r => {
              const naoTocados = r.clientesAB - r.tocados;
              const statusBadge = r.cobertura >= d.meta ? 'badge-success' : (r.cobertura >= 70 ? 'badge-warning' : 'badge-danger');
              const statusLabel = r.cobertura >= d.meta ? 'Na meta' : (r.cobertura >= 70 ? 'Atenção' : 'Crítico');
              return `
                <tr>
                  <td><strong>${r.label}</strong></td>
                  <td class="num">${fmtNum(r.clientesAB)}</td>
                  <td class="num">${fmtNum(r.tocados)}</td>
                  <td class="num" style="color:${naoTocados > 50 ? 'var(--danger)' : 'var(--text-primary)'}">${fmtNum(naoTocados)}</td>
                  <td class="num"><strong>${r.cobertura.toFixed(1)}%</strong></td>
                  <td class="num">${fmtNum(r.vendas_perdidas)}</td>
                  <td class="num">${fmtNum(r.gpe_perdidas)}</td>
                  <td><span class="badge ${statusBadge}">${statusLabel}</span></td>
                </tr>
              `;
            }).join('')}
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        <span>7 regionais · Meta ${d.meta}% cobertura A+B em 120 dias</span>
      </div>
    </div>
  `;
}

// Gauge SVG helper
function drawGauge(svgId, value, options = {}) {
  const svg = document.getElementById(svgId);
  if (!svg) return;

  const cx = 130, cy = 140, r = 100;
  const startAngle = 180, endAngle = 360; // Semicírculo superior
  const segments = options.segments || [
    { max: 40,  color: '#DC2626' },
    { max: 70,  color: '#F59E0B' },
    { max: 80,  color: '#FFDE00' },
    { max: 100, color: '#16A34A' },
  ];

  function polarToCartesian(angle) {
    const rad = (angle - 90) * Math.PI / 180;
    return { x: cx + r * Math.cos(rad), y: cy + r * Math.sin(rad) };
  }

  function arcPath(fromDeg, toDeg, radius) {
    const start = polarToCartesian(fromDeg);
    const end = polarToCartesian(toDeg);
    const largeArc = toDeg - fromDeg > 180 ? 1 : 0;
    return `M ${start.x} ${start.y} A ${radius} ${radius} 0 ${largeArc} 1 ${end.x} ${end.y}`;
  }

  let content = '';
  let prev = 0;
  segments.forEach(seg => {
    const fromDeg = startAngle + (endAngle - startAngle) * (prev / 100);
    const toDeg = startAngle + (endAngle - startAngle) * (seg.max / 100);
    content += `<path d="${arcPath(fromDeg, toDeg, r)}" stroke="${seg.color}" stroke-width="18" fill="none" stroke-linecap="butt"/>`;
    prev = seg.max;
  });

  // Ticks
  [0, 20, 40, 60, 80, 100].forEach(v => {
    const deg = startAngle + (endAngle - startAngle) * (v / 100);
    const pIn = polarToCartesian(deg);
    const pOutRad = r - 30;
    const rad = (deg - 90) * Math.PI / 180;
    const tx = cx + pOutRad * Math.cos(rad);
    const ty = cy + pOutRad * Math.sin(rad);
    content += `<text x="${tx}" y="${ty}" text-anchor="middle" dominant-baseline="middle" fill="var(--text-tertiary)" font-size="10" font-family="Inter, sans-serif">${v}%</text>`;
  });

  // Ponteiro
  const needleAngle = startAngle + (endAngle - startAngle) * (value / 100);
  const rad = (needleAngle - 90) * Math.PI / 180;
  const nx = cx + (r - 8) * Math.cos(rad);
  const ny = cy + (r - 8) * Math.sin(rad);
  content += `
    <line x1="${cx}" y1="${cy}" x2="${nx}" y2="${ny}" stroke="var(--text-primary)" stroke-width="3" stroke-linecap="round"/>
    <circle cx="${cx}" cy="${cy}" r="8" fill="var(--text-primary)"/>
    <circle cx="${cx}" cy="${cy}" r="4" fill="var(--jd-yellow)"/>
  `;

  // Valor central
  content += `
    <text x="${cx}" y="${cy + 40}" text-anchor="middle" fill="var(--text-primary)" font-size="28" font-weight="700" font-family="Inter, sans-serif">${value.toFixed(1)}%</text>
  `;

  svg.innerHTML = content;
}

// Bar chart por regional
function drawBarCobertura() {
  const svg = document.getElementById('bar-cobertura');
  if (!svg) return;

  const data = COBERTURA_DATA.regionais;
  const W = 640, H = 260;
  const padding = { top: 20, right: 20, bottom: 40, left: 40 };
  const chartW = W - padding.left - padding.right;
  const chartH = H - padding.top - padding.bottom;

  const barWidth = chartW / data.length * 0.7;
  const gap = chartW / data.length * 0.3;

  let content = '';

  // Grid horizontal
  [0, 25, 50, 75, 100].forEach(v => {
    const y = padding.top + chartH - (chartH * v / 100);
    content += `<line x1="${padding.left}" y1="${y}" x2="${W - padding.right}" y2="${y}" stroke="var(--border)" stroke-width="0.5" stroke-dasharray="${v === 0 ? '0' : '2 3'}"/>`;
    content += `<text x="${padding.left - 8}" y="${y + 3}" text-anchor="end" fill="var(--text-tertiary)" font-size="10" font-family="Inter, sans-serif">${v}%</text>`;
  });

  // Linha da meta
  const metaY = padding.top + chartH - (chartH * COBERTURA_DATA.meta / 100);
  content += `<line x1="${padding.left}" y1="${metaY}" x2="${W - padding.right}" y2="${metaY}" stroke="var(--jd-green)" stroke-width="1.5" stroke-dasharray="4 3"/>`;
  content += `<text x="${W - padding.right}" y="${metaY - 4}" text-anchor="end" fill="var(--jd-green-dark)" font-size="10" font-weight="600" font-family="Inter, sans-serif">Meta ${COBERTURA_DATA.meta}%</text>`;

  // Barras
  data.forEach((d, i) => {
    const x = padding.left + (chartW / data.length) * i + gap / 2;
    const barH = chartH * d.cobertura / 100;
    const y = padding.top + chartH - barH;
    const color = d.cobertura >= COBERTURA_DATA.meta ? '#16A34A' : (d.cobertura >= 70 ? '#F59E0B' : '#DC2626');

    content += `<rect x="${x}" y="${y}" width="${barWidth}" height="${barH}" fill="${color}" rx="3" opacity="0.9"/>`;
    content += `<text x="${x + barWidth/2}" y="${y - 6}" text-anchor="middle" fill="var(--text-primary)" font-size="11" font-weight="600" font-family="Inter, sans-serif">${d.cobertura.toFixed(1)}%</text>`;
    content += `<text x="${x + barWidth/2}" y="${padding.top + chartH + 16}" text-anchor="middle" fill="var(--text-secondary)" font-size="10" font-family="Inter, sans-serif">${d.key}</text>`;
    content += `<text x="${x + barWidth/2}" y="${padding.top + chartH + 30}" text-anchor="middle" fill="var(--text-tertiary)" font-size="9" font-family="Inter, sans-serif">${d.label}</text>`;
  });

  svg.innerHTML = content;
}

// Bar chart vendas perdidas por regional
function drawBarVendasPerdidas() {
  const svg = document.getElementById('bar-vendas-perdidas');
  if (!svg) return;

  const data = COBERTURA_DATA.regionais.slice().sort((a,b) => b.vendas_perdidas - a.vendas_perdidas);
  const W = 400, H = 260;
  const padding = { top: 15, right: 20, bottom: 40, left: 40 };
  const chartW = W - padding.left - padding.right;
  const chartH = H - padding.top - padding.bottom;

  const maxVal = Math.max(...data.map(d => d.vendas_perdidas));
  const barWidth = chartW / data.length * 0.7;
  const gap = chartW / data.length * 0.3;

  let content = '';

  // Grid
  [0, 0.25, 0.5, 0.75, 1].forEach(f => {
    const y = padding.top + chartH - (chartH * f);
    const v = Math.round(maxVal * f);
    content += `<line x1="${padding.left}" y1="${y}" x2="${W - padding.right}" y2="${y}" stroke="var(--border)" stroke-width="0.5" stroke-dasharray="${f === 0 ? '0' : '2 3'}"/>`;
    content += `<text x="${padding.left - 8}" y="${y + 3}" text-anchor="end" fill="var(--text-tertiary)" font-size="10" font-family="Inter, sans-serif">${v}</text>`;
  });

  // Barras
  data.forEach((d, i) => {
    const x = padding.left + (chartW / data.length) * i + gap / 2;
    const barH = chartH * d.vendas_perdidas / maxVal;
    const y = padding.top + chartH - barH;

    content += `<rect x="${x}" y="${y}" width="${barWidth}" height="${barH}" fill="#7C3AED" rx="3" opacity="0.85"/>`;
    content += `<text x="${x + barWidth/2}" y="${y - 4}" text-anchor="middle" fill="var(--text-primary)" font-size="10" font-weight="600" font-family="Inter, sans-serif">${d.vendas_perdidas}</text>`;
    content += `<text x="${x + barWidth/2}" y="${padding.top + chartH + 16}" text-anchor="middle" fill="var(--text-secondary)" font-size="9" font-family="Inter, sans-serif">${d.key}</text>`;
  });

  svg.innerHTML = content;
}

function mountCobertura() {
  drawGauge('gauge-cobertura', COBERTURA_DATA.geral.cobertura);
  drawGauge('gauge-gpe', COBERTURA_DATA.vendas_perdidas.percentual_gpe, {
    segments: [
      { max: 30,  color: '#16A34A' }, // Baixa perda = bom
      { max: 60,  color: '#F59E0B' },
      { max: 80,  color: '#DC2626' },
      { max: 100, color: '#991B1B' },
    ]
  });
  drawBarCobertura();
  drawBarVendasPerdidas();
}

// ===== AGENDA DO CEN =====
// Hoje (contexto do protótipo): 25/08/2026 (terça)
const AGENDA_HOJE = new Date('2026-08-25T09:00:00-03:00');

const AGENDA_DATA = {
  cen_atual: { user: 'jose.rufino', nome: 'José Rufino', regional: 'MT Norte', avatar: 'JR' },
  cens_disponiveis: [
    { user: 'jose.rufino', nome: 'José Rufino', regional: 'MT Norte' },
    { user: 'matheus.augusto', nome: 'Matheus Augusto', regional: 'MT Sul' },
    { user: 'vanessa.alves', nome: 'Vanessa Alves', regional: 'Goiás' },
    { user: 'silmara.ferreira', nome: 'Silmara Ferreira', regional: 'MG Triângulo' },
  ],
  tarefas: [
    // ATRASADAS (13)
    { id: 1, data: '2026-04-09T09:51', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Alcides Herminio Sanfelice e Outros', cliente_id: 74133, cidade: 'Sorriso/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2025-11-14', obs: 'Cliente sinalizou interesse em pulverizador autopropelido — retornar até dezembro.' },
    { id: 2, data: '2026-05-08T09:14', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Antonio Luís Bianchi', cliente_id: 85859, cidade: 'Lucas do Rio Verde/MT', classe: 'A', prioridade: 0, origem: 'manual', agendado_por: 'silmara.ferreira', ultima_interacao: '2026-02-10', obs: 'Comprou 2 tratores 7J em 2024. Renovação da linha em 2026.' },
    { id: 3, data: '2026-07-21T16:54', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Alvaro Tadeu Arantes Nogueira e Outros', cliente_id: 3375, cidade: 'Sinop/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-03-22', obs: 'Frota mista (JD + concorrente). Ver oportunidade de conversão.' },
    { id: 4, data: '2026-07-30T08:14', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Antonio Carlos Fernades', cliente_id: 61579, cidade: 'Sorriso/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-03-30', obs: '' },
    { id: 5, data: '2026-07-30T08:14', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Antonio Donizeti Vencel', cliente_id: 2265, cidade: 'Sorriso/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-03-30', obs: '' },
    { id: 6, data: '2026-08-06T10:59', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Agropecuaria HBC Irmãos de Santi Ltda', cliente_id: 39773, cidade: 'Nova Mutum/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-06', obs: 'Cliente estratégico. Falta agendar visita técnica com pós-vendas.' },
    { id: 7, data: '2026-08-08T15:33', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Abel Natal Scandolara', cliente_id: 27879, cidade: 'Sorriso/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-08', obs: '' },
    { id: 8, data: '2026-08-11T11:06', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Adilson Callix', cliente_id: 26276, cidade: 'Sinop/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-11', obs: '' },
    { id: 9, data: '2026-08-19T14:00', tipo: 'visitar', titulo: 'Visita comercial · Angelo Caressato', cliente: 'Angelo Caressato Anibal', cliente_id: 19916, cidade: 'Sorriso/MT', classe: 'A', prioridade: 0, origem: 'manual', agendado_por: 'vanessa.alves', ultima_interacao: '2026-08-01', obs: 'Cliente pediu proposta formal de 2x colheitadeira S780. Gerente reagendou.' },
    { id: 10, data: '2026-08-21T16:54', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Alvaro Tadeu Arantes Nogueira e Outros', cliente_id: 3375, cidade: 'Sinop/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-21', obs: '' },
    { id: 11, data: '2026-08-22T09:49', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Agroindustrial Salvador Arena Ltda', cliente_id: 84391, cidade: 'Lucas do Rio Verde/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-22', obs: 'Grande frota. Atenção prioritária.' },

    // HOJE (25/08/2026) - 4 tarefas
    { id: 20, data: '2026-08-25T08:30', tipo: 'ligar', titulo: 'Retornar ligação · José Antônio', cliente: 'Fazenda Três Rios Ltda', cliente_id: 45001, cidade: 'Sinop/MT', classe: 'A', prioridade: 0, origem: 'manual', agendado_por: 'jose.rufino', ultima_interacao: '2026-08-24', obs: 'Cliente ligou ontem pedindo cotação de kit de plantio direto. Retornar antes das 10h.' },
    { id: 21, data: '2026-08-25T10:30', tipo: 'visitar', titulo: 'Visita técnica · Cerâmica São Marcos', cliente: 'Cerâmica São Marcos Agroindustrial', cliente_id: 61234, cidade: 'Sorriso/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-05-25', obs: 'Cliente parou de responder desde maio. Levar catálogo linha 6J.' },
    { id: 22, data: '2026-08-25T14:00', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Agropecuária Boa Vista S/A', cliente_id: 78901, cidade: 'Nova Mutum/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-05-01', obs: '' },
    { id: 23, data: '2026-08-25T16:30', tipo: 'proposta', titulo: 'Fechar proposta · Trator 6135J', cliente: 'Sítio Aliança Ltda ME', cliente_id: 12456, cidade: 'Sorriso/MT', classe: 'B', prioridade: 0, origem: 'manual', agendado_por: 'jose.rufino', ultima_interacao: '2026-08-22', obs: 'Cliente aprovou proposta verbalmente. Faltam assinaturas e envio ao Protheus.' },

    // AMANHÃ (26/08) - 3 tarefas
    { id: 30, data: '2026-08-26T08:00', tipo: 'visitar', titulo: 'Visita · Fazenda Nova Aurora', cliente: 'Fazenda Nova Aurora Ltda', cliente_id: 32178, cidade: 'Sorriso/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-26', obs: 'Rota do dia: passar antes de ir ao próximo cliente.' },
    { id: 31, data: '2026-08-26T11:00', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Agro Serra Real Ltda', cliente_id: 87654, cidade: 'Sinop/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-26', obs: '' },
    { id: 32, data: '2026-08-26T15:00', tipo: 'ligar', titulo: 'Ligar · Emerson Sanches', cliente: 'Emerson Sanches Produções Rurais', cliente_id: 44521, cidade: 'Lucas do Rio Verde/MT', classe: 'A', prioridade: 0, origem: 'manual', agendado_por: 'matheus.augusto', ultima_interacao: '2026-08-20', obs: 'Gerente pediu para retomar contato. Cliente sumiu.' },

    // ESTA SEMANA (27-30/08) - 5 tarefas
    { id: 40, data: '2026-08-27T09:00', tipo: 'visitar', titulo: 'Visita · Fazenda Progresso', cliente: 'Fazenda Progresso Agropecuária', cliente_id: 55632, cidade: 'Sinop/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-27' },
    { id: 41, data: '2026-08-27T14:00', tipo: 'proposta', titulo: 'Apresentar proposta · Plantadeira DB44', cliente: 'Sítio Santa Fé Ltda', cliente_id: 33221, cidade: 'Nova Mutum/MT', classe: 'B', prioridade: 0, origem: 'manual', agendado_por: 'jose.rufino', ultima_interacao: '2026-08-15' },
    { id: 42, data: '2026-08-28T10:00', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'JCV Agronegócios S/A', cliente_id: 66112, cidade: 'Sorriso/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-28' },
    { id: 43, data: '2026-08-28T16:00', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Rondon Agro Ltda', cliente_id: 77891, cidade: 'Sinop/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-04-28' },
    { id: 44, data: '2026-08-29T09:00', tipo: 'ligar', titulo: 'Ligar · Retomada pós-venda', cliente: 'Fazenda Cristalina Ltda', cliente_id: 88223, cidade: 'Sorriso/MT', classe: 'A', prioridade: 1, origem: 'manual', agendado_por: 'vanessa.alves', ultima_interacao: '2026-08-10' },

    // FUTURAS (setembro+) - 4 tarefas
    { id: 50, data: '2026-09-02T09:00', tipo: 'visitar', titulo: 'Visita · Cooperativa Coamo', cliente: 'Coamo Agroindustrial Cooperativa', cliente_id: 99123, cidade: 'Nova Mutum/MT', classe: 'A', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-05-02' },
    { id: 51, data: '2026-09-05T10:00', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Estância Cristalina Ltda', cliente_id: 44567, cidade: 'Sinop/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-05-05' },
    { id: 52, data: '2026-09-10T14:00', tipo: 'proposta', titulo: 'Follow-up proposta · Colheitadeira S770', cliente: 'Grupo Bom Futuro', cliente_id: 55144, cidade: 'Lucas do Rio Verde/MT', classe: 'A', prioridade: 0, origem: 'manual', agendado_por: 'jose.rufino', ultima_interacao: '2026-08-20' },
    { id: 53, data: '2026-09-15T09:00', tipo: 'monitorar', titulo: 'Monitorar Cliente (FY25)', cliente: 'Agropecuária Bela Vista', cliente_id: 22334, cidade: 'Sorriso/MT', classe: 'B', prioridade: 2, origem: 'auto', agendado_por: 'sistema.abc', ultima_interacao: '2026-05-15' },
  ],
};

const TIPO_META = {
  monitorar: { label: 'Monitoramento', icon: 'eye', color: '#0284C7', bg: '#E0F2FE' },
  visitar:   { label: 'Visita',        icon: 'nav',  color: '#16A34A', bg: '#DCFCE7' },
  ligar:     { label: 'Ligação',       icon: 'phone',color: '#7C3AED', bg: '#EDE9FE' },
  proposta:  { label: 'Proposta',      icon: 'file', color: '#EA580C', bg: '#FFEDD5' },
};

function classifyByDate(dataISO) {
  const d = new Date(dataISO);
  const hoje = new Date(AGENDA_HOJE);
  hoje.setHours(0,0,0,0);
  const amanha = new Date(hoje); amanha.setDate(amanha.getDate() + 1);
  const domSeg = new Date(hoje);
  const diaSemana = hoje.getDay(); // 0=dom, 1=seg, ..., 2=ter (que é hoje)
  const domingoQueVem = new Date(hoje); domingoQueVem.setDate(hoje.getDate() + (7 - diaSemana));
  const t = new Date(d); t.setHours(0,0,0,0);
  if (t < hoje) return 'atrasada';
  if (t.getTime() === hoje.getTime()) return 'hoje';
  if (t.getTime() === amanha.getTime()) return 'amanha';
  if (t < domingoQueVem) return 'semana';
  return 'futura';
}

function diasAtraso(dataISO) {
  const d = new Date(dataISO); d.setHours(0,0,0,0);
  const hoje = new Date(AGENDA_HOJE); hoje.setHours(0,0,0,0);
  return Math.floor((hoje - d) / (1000 * 60 * 60 * 24));
}

function fmtDataHora(iso) {
  const d = new Date(iso);
  const dias = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];
  const meses = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
  return `${dias[d.getDay()]} · ${d.getDate().toString().padStart(2,'0')}/${meses[d.getMonth()]} · ${d.getHours().toString().padStart(2,'0')}:${d.getMinutes().toString().padStart(2,'0')}`;
}

function iconSvg(name) {
  const icons = {
    eye:   `<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>`,
    nav:   `<path d="M3 11l19-9-9 19-2-8-8-2z"/>`,
    phone: `<path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/>`,
    file:  `<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/>`,
  };
  return `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${icons[name] || ''}</svg>`;
}

function renderAgenda() {
  const d = AGENDA_DATA;
  const cen = d.cen_atual;
  const tarefas = d.tarefas.slice().sort((a,b) => new Date(a.data) - new Date(b.data));

  const grupos = {
    atrasada: tarefas.filter(t => classifyByDate(t.data) === 'atrasada'),
    hoje:     tarefas.filter(t => classifyByDate(t.data) === 'hoje'),
    amanha:   tarefas.filter(t => classifyByDate(t.data) === 'amanha'),
    semana:   tarefas.filter(t => classifyByDate(t.data) === 'semana'),
    futura:   tarefas.filter(t => classifyByDate(t.data) === 'futura'),
  };

  const total = tarefas.length;

  return `
    <div class="page-header">
      <div>
        <h1 class="page-title">Agenda do CEN</h1>
        <p class="page-subtitle">
          <strong>${cen.nome}</strong> · Regional ${cen.regional} · Terça, 25 de agosto de 2026
        </p>
      </div>
      <div class="page-actions">
        <div class="view-toggle" data-view="lista">
          <button class="view-tab active" data-view="lista">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
            Lista
          </button>
          <button class="view-tab" data-view="semana">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
            Semana
          </button>
        </div>
        <button class="btn btn-primary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nova tarefa
        </button>
      </div>
    </div>

    <!-- KPIs da agenda -->
    <div class="kpi-grid" style="grid-template-columns:repeat(5,1fr);margin-bottom:20px">
      <div class="kpi ${grupos.atrasada.length > 0 ? 'kpi-danger' : ''}">
        <span class="kpi-label">Atrasadas</span>
        <span class="kpi-value">${grupos.atrasada.length}</span>
        <span class="kpi-hint">${grupos.atrasada.filter(t => t.classe === 'A').length} de clientes A</span>
      </div>
      <div class="kpi kpi-highlight">
        <span class="kpi-label">Para hoje</span>
        <span class="kpi-value">${grupos.hoje.length}</span>
        <span class="kpi-hint">${grupos.hoje.filter(t => t.prioridade === 0).length} de alta prioridade</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Amanhã</span>
        <span class="kpi-value">${grupos.amanha.length}</span>
        <span class="kpi-hint">Planeje sua rota</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Esta semana</span>
        <span class="kpi-value">${grupos.semana.length}</span>
        <span class="kpi-hint">Qua a sáb (27-30/ago)</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Total ativas</span>
        <span class="kpi-value">${total}</span>
        <span class="kpi-hint">${d.tarefas.filter(t => t.origem === 'auto').length} auto · ${d.tarefas.filter(t => t.origem === 'manual').length} manuais</span>
      </div>
    </div>

    <!-- Filtros -->
    <div class="filter-bar" style="grid-template-columns:repeat(4,1fr) auto">
      <div class="filter-field">
        <label>CEN</label>
        <select>
          ${d.cens_disponiveis.map(c => `<option ${c.user === cen.user ? 'selected' : ''}>${c.nome} · ${c.regional}</option>`).join('')}
        </select>
      </div>
      <div class="filter-field">
        <label>Tipo de tarefa</label>
        <select>
          <option>Todos os tipos</option>
          <option>Monitoramento (auto)</option>
          <option>Visita</option>
          <option>Ligação</option>
          <option>Proposta</option>
        </select>
      </div>
      <div class="filter-field">
        <label>Classe de cliente</label>
        <select>
          <option>Todas</option>
          <option>Somente A</option>
          <option>Somente B</option>
          <option>A e B</option>
        </select>
      </div>
      <div class="filter-field">
        <label>Prioridade</label>
        <select>
          <option>Todas</option>
          <option>Alta (0)</option>
          <option>Média (1)</option>
          <option>Baixa (2)</option>
        </select>
      </div>
      <div class="filter-field filter-field-btn">
        <button class="btn btn-ghost btn-sm">Limpar filtros</button>
      </div>
    </div>

    <!-- Lista agrupada -->
    <div id="agenda-lista-view" class="agenda-view">
      ${renderAgendaGrupo('atrasada', grupos.atrasada, 'Atrasadas', 'atenção — resolver antes de qualquer nova visita', 'danger')}
      ${renderAgendaGrupo('hoje', grupos.hoje, 'Hoje · terça, 25/08', 'sua rota do dia', 'highlight')}
      ${renderAgendaGrupo('amanha', grupos.amanha, 'Amanhã · quarta, 26/08', 'para planejar', 'default')}
      ${renderAgendaGrupo('semana', grupos.semana, 'Esta semana · qua-sáb', '27 a 30 de agosto', 'default')}
      ${renderAgendaGrupo('futura', grupos.futura, 'Futuras · setembro+', 'planejamento de longo prazo', 'default')}
    </div>

    <!-- Semana view (calendário estilo Outlook — placeholder minimalista) -->
    <div id="agenda-semana-view" class="agenda-view" style="display:none">
      ${renderAgendaSemana(tarefas)}
    </div>

    <!-- Drawer de detalhes / conclusão -->
    <div id="agenda-drawer" class="drawer" style="display:none">
      <div class="drawer-backdrop"></div>
      <div class="drawer-panel"></div>
    </div>
  `;
}

function renderAgendaGrupo(key, itens, titulo, subtitulo, tone) {
  if (itens.length === 0) {
    return `
      <div class="agenda-grupo agenda-grupo-empty">
        <div class="agenda-grupo-header">
          <div>
            <div class="agenda-grupo-title">${titulo} <span class="count-badge">0</span></div>
            <div class="agenda-grupo-sub">${subtitulo}</div>
          </div>
        </div>
        <div class="empty-state">Nenhuma tarefa neste grupo.</div>
      </div>
    `;
  }

  return `
    <div class="agenda-grupo agenda-grupo-${tone}">
      <div class="agenda-grupo-header">
        <div>
          <div class="agenda-grupo-title">${titulo} <span class="count-badge count-badge-${tone}">${itens.length}</span></div>
          <div class="agenda-grupo-sub">${subtitulo}</div>
        </div>
      </div>
      <div class="agenda-tarefas">
        ${itens.map(t => renderTarefaCard(t, key)).join('')}
      </div>
    </div>
  `;
}

function renderTarefaCard(t, grupo) {
  const meta = TIPO_META[t.tipo];
  const atraso = grupo === 'atrasada' ? diasAtraso(t.data) : 0;
  const prioridadeLabel = t.prioridade === 0 ? { label: 'Alta', color: 'danger' } : (t.prioridade === 1 ? { label: 'Média', color: 'warning' } : { label: 'Baixa', color: 'muted' });

  return `
    <div class="tarefa-card ${grupo === 'atrasada' ? 'tarefa-card-atrasada' : ''}" data-id="${t.id}" onclick="openTarefaDrawer(${t.id})">
      <div class="tarefa-tipo" style="background:${meta.bg};color:${meta.color}" title="${meta.label}">
        ${iconSvg(meta.icon)}
      </div>
      <div class="tarefa-body">
        <div class="tarefa-linha1">
          <span class="tarefa-titulo">${t.titulo}</span>
          <span class="tarefa-classe classe-${t.classe.toLowerCase()}">Classe ${t.classe}</span>
          <span class="tarefa-origem origem-${t.origem}" title="${t.origem === 'auto' ? 'Criada automaticamente pela regra ABC' : `Criada por ${t.agendado_por}`}">
            ${t.origem === 'auto' ? '⚙ auto' : '👤 manual'}
          </span>
          ${t.prioridade === 0 ? '<span class="tarefa-priority-flag">● Alta prioridade</span>' : ''}
        </div>
        <div class="tarefa-cliente">
          <strong>${t.cliente}</strong>
          <span class="tarefa-cidade">${t.cidade}</span>
          <span class="tarefa-cid-id">#${t.cliente_id}</span>
        </div>
        <div class="tarefa-linha3">
          <span class="tarefa-data">${fmtDataHora(t.data)}</span>
          ${atraso > 0 ? `<span class="tarefa-atraso">${atraso} dias em atraso</span>` : ''}
          <span class="tarefa-ultima">Última interação: ${t.ultima_interacao ? new Date(t.ultima_interacao).toLocaleDateString('pt-BR') : '—'}</span>
        </div>
        ${t.obs ? `<div class="tarefa-obs">"${t.obs}"</div>` : ''}
      </div>
      <div class="tarefa-actions">
        <button class="btn-icon" title="Concluir" onclick="event.stopPropagation();openTarefaDrawer(${t.id}, 'concluir')">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
        </button>
        <button class="btn-icon" title="Reagendar" onclick="event.stopPropagation()">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12a9 9 0 1 1-3-6.7L21 8"/><polyline points="21 3 21 8 16 8"/></svg>
        </button>
        <button class="btn-icon" title="Mais opções" onclick="event.stopPropagation()">
          <circle cx="12" cy="12" r="1" fill="currentColor"/><circle cx="12" cy="5" r="1" fill="currentColor"/><circle cx="12" cy="19" r="1" fill="currentColor"/>
        </button>
      </div>
    </div>
  `;
}

function renderAgendaSemana(tarefas) {
  // Semana de 24/08 (dom) a 30/08 (sáb)
  const dias = [
    { data: '2026-08-24', label: 'Dom', dia: '24' },
    { data: '2026-08-25', label: 'Ter', dia: '25', hoje: true }, // ajuste manual: 25/08/2026 é terça
    { data: '2026-08-26', label: 'Qua', dia: '26' },
    { data: '2026-08-27', label: 'Qui', dia: '27' },
    { data: '2026-08-28', label: 'Sex', dia: '28' },
    { data: '2026-08-29', label: 'Sáb', dia: '29' },
    { data: '2026-08-30', label: 'Dom', dia: '30' },
  ];

  // 25/08/2026 é terça-feira mesmo → semana começa em domingo 23/08
  const diasCorreto = [
    { data: '2026-08-23', label: 'Dom', dia: '23' },
    { data: '2026-08-24', label: 'Seg', dia: '24' },
    { data: '2026-08-25', label: 'Ter', dia: '25', hoje: true },
    { data: '2026-08-26', label: 'Qua', dia: '26' },
    { data: '2026-08-27', label: 'Qui', dia: '27' },
    { data: '2026-08-28', label: 'Sex', dia: '28' },
    { data: '2026-08-29', label: 'Sáb', dia: '29' },
  ];

  const horas = [];
  for (let h = 7; h <= 18; h++) horas.push(h);

  const tarefasPorDia = {};
  diasCorreto.forEach(d => {
    tarefasPorDia[d.data] = tarefas.filter(t => t.data.startsWith(d.data));
  });

  return `
    <div class="calendar-week">
      <div class="calendar-header">
        <div class="calendar-hour-col"></div>
        ${diasCorreto.map(d => `
          <div class="calendar-day-header ${d.hoje ? 'hoje' : ''}">
            <div class="cal-dia-label">${d.label}</div>
            <div class="cal-dia-num">${d.dia}</div>
          </div>
        `).join('')}
      </div>
      <div class="calendar-body">
        ${horas.map(h => `
          <div class="calendar-row">
            <div class="calendar-hour-col">${h.toString().padStart(2,'0')}:00</div>
            ${diasCorreto.map(d => {
              const tarefasHora = (tarefasPorDia[d.data] || []).filter(t => {
                const th = new Date(t.data).getHours();
                return th === h;
              });
              return `
                <div class="calendar-cell ${d.hoje ? 'hoje' : ''}">
                  ${tarefasHora.map(t => {
                    const meta = TIPO_META[t.tipo];
                    return `<div class="cal-tarefa" style="background:${meta.bg};border-left:3px solid ${meta.color}" onclick="openTarefaDrawer(${t.id})" title="${t.titulo}">
                      <div class="cal-tarefa-hora">${new Date(t.data).getHours().toString().padStart(2,'0')}:${new Date(t.data).getMinutes().toString().padStart(2,'0')}</div>
                      <div class="cal-tarefa-titulo">${t.titulo}</div>
                      <div class="cal-tarefa-cliente">${t.cliente}</div>
                    </div>`;
                  }).join('')}
                </div>
              `;
            }).join('')}
          </div>
        `).join('')}
      </div>
    </div>
  `;
}

function openTarefaDrawer(id, modo = 'ver') {
  const tarefa = AGENDA_DATA.tarefas.find(t => t.id === id);
  if (!tarefa) return;
  const meta = TIPO_META[tarefa.tipo];
  const drawer = document.getElementById('agenda-drawer');
  const panel = drawer.querySelector('.drawer-panel');
  drawer.style.display = 'block';

  panel.innerHTML = `
    <div class="drawer-header">
      <div>
        <div class="drawer-eyebrow" style="color:${meta.color}">${meta.label} · #${id}</div>
        <div class="drawer-title">${tarefa.titulo}</div>
      </div>
      <button class="btn-icon-large" onclick="closeTarefaDrawer()">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
      </button>
    </div>
    <div class="drawer-body">
      <div class="drawer-info-grid">
        <div class="info-block">
          <div class="info-label">Cliente</div>
          <div class="info-value"><strong>${tarefa.cliente}</strong> · #${tarefa.cliente_id}</div>
          <div class="info-sub">${tarefa.cidade} · Classe ${tarefa.classe}</div>
        </div>
        <div class="info-block">
          <div class="info-label">Data agendada</div>
          <div class="info-value">${fmtDataHora(tarefa.data)}</div>
          <div class="info-sub">${classifyByDate(tarefa.data) === 'atrasada' ? `<span style="color:var(--danger);font-weight:600">${diasAtraso(tarefa.data)} dias em atraso</span>` : 'No prazo'}</div>
        </div>
        <div class="info-block">
          <div class="info-label">Origem</div>
          <div class="info-value">${tarefa.origem === 'auto' ? '⚙ Automática (regra ABC · 120d)' : `👤 Manual · ${tarefa.agendado_por}`}</div>
        </div>
        <div class="info-block">
          <div class="info-label">Última interação</div>
          <div class="info-value">${tarefa.ultima_interacao ? new Date(tarefa.ultima_interacao).toLocaleDateString('pt-BR') : '—'}</div>
        </div>
      </div>

      ${tarefa.obs ? `
        <div class="info-block info-block-full">
          <div class="info-label">Observações</div>
          <div class="info-value info-obs">"${tarefa.obs}"</div>
        </div>
      ` : ''}

      <div class="drawer-section">
        <div class="drawer-section-title">Registrar contato</div>
        <div class="form-grid">
          <div class="form-field">
            <label>Data e hora do contato</label>
            <input type="datetime-local" value="2026-08-25T${new Date().getHours().toString().padStart(2,'0')}:00">
          </div>
          <div class="form-field">
            <label>Tipo de contato</label>
            <select>
              <option>Visita presencial</option>
              <option>Ligação telefônica</option>
              <option>WhatsApp / Mensagem</option>
              <option>E-mail</option>
              <option>Reunião virtual</option>
            </select>
          </div>
          <div class="form-field">
            <label>Resultado</label>
            <select>
              <option>Cliente contactado — sem interesse imediato</option>
              <option>Cliente contactado — gerou oportunidade</option>
              <option>Cliente contactado — reagendar visita</option>
              <option>Não conseguiu contato — tentar novamente</option>
              <option>Cliente ausente / viagem</option>
              <option>Cliente perdeu interesse na linha</option>
            </select>
          </div>
          <div class="form-field">
            <label>Próximo passo</label>
            <select>
              <option>Nenhuma ação — encerrar</option>
              <option>Reagendar contato para +30 dias</option>
              <option>Reagendar contato para +60 dias</option>
              <option>Criar oportunidade no funil</option>
              <option>Escalar para gerente comercial</option>
            </select>
          </div>
          <div class="form-field form-field-full">
            <label>Observações do contato</label>
            <textarea rows="3" placeholder="Ex: Cliente informou que colheita se estende até setembro. Retornar em outubro com proposta de 2 tratores 6J."></textarea>
          </div>
        </div>
      </div>
    </div>
    <div class="drawer-footer">
      <button class="btn btn-ghost" onclick="closeTarefaDrawer()">Cancelar</button>
      <button class="btn btn-secondary" onclick="closeTarefaDrawer();location.hash='#/clientes/84391'">Salvar e ver cliente</button>
      <button class="btn btn-primary">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
        Concluir tarefa
      </button>
    </div>
  `;

  document.body.style.overflow = 'hidden';
}

function closeTarefaDrawer() {
  const drawer = document.getElementById('agenda-drawer');
  if (drawer) drawer.style.display = 'none';
  document.body.style.overflow = '';
}

function mountAgenda() {
  // View toggle
  document.querySelectorAll('.view-tab').forEach(tab => {
    tab.addEventListener('click', () => {
      const view = tab.dataset.view;
      document.querySelectorAll('.view-tab').forEach(t => t.classList.toggle('active', t === tab));
      document.getElementById('agenda-lista-view').style.display = view === 'lista' ? 'flex' : 'none';
      document.getElementById('agenda-semana-view').style.display = view === 'semana' ? 'block' : 'none';
    });
  });

  // Fechar drawer com Esc
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeTarefaDrawer();
  });

  // Fechar drawer clicando no backdrop
  const drawer = document.getElementById('agenda-drawer');
  if (drawer) {
    drawer.querySelector('.drawer-backdrop').addEventListener('click', closeTarefaDrawer);
  }
}

// ===== FICHA DO CLIENTE 360° =====
const CLIENTE_DATA = {
  id: 84391,
  razao_social: 'Agroindustrial Salvador Arena Ltda',
  nome_fantasia: 'Fazenda Santa Fé',
  cnpj: '11.234.567/0001-89',
  ie: '13.456.789-0',
  ie_uf: 'MT',
  data_fundacao: '2003-04-12',
  tipo_pessoa: 'juridica',
  classe: 'A',
  status: 'Ativo',
  bloqueio: null,
  regional: 'MT Norte',
  cen_dono: { user: 'jose.rufino', nome: 'José Rufino', avatar: 'JR' },
  cen_pos_venda: { user: 'ricardo.moretti', nome: 'Ricardo Moretti', avatar: 'RM' },
  origem_inclusao: 'CRM-Manual · 15/03/2020',
  origem_alteracao: 'Protheus-Sync · 22/08/2026 14:31',

  endereco_principal: {
    logradouro: 'Rodovia MT-235, KM 87',
    numero: 'S/N',
    complemento: 'Zona Rural',
    bairro: 'Gleba Santa Fé',
    cidade: 'Lucas do Rio Verde',
    uf: 'MT',
    cep: '78455-000',
    pais: 'Brasil',
    latitude: -13.0489,
    longitude: -55.9142,
  },
  enderecos_adicionais: [
    { tipo: 'Fazenda Boa Vista', cidade: 'Sorriso/MT', hectares: 2450, cultura: 'Soja + Milho safrinha' },
    { tipo: 'Fazenda São Luiz', cidade: 'Nova Mutum/MT', hectares: 1820, cultura: 'Soja + Algodão' },
    { tipo: 'Escritório administrativo', cidade: 'Lucas do Rio Verde/MT', hectares: 0, cultura: '—' },
  ],

  preferencias_contato: {
    telefonema: true, correspondencia: false, email: true, sms: false, whatsapp: true,
  },
  consentimento_lgpd: { ativo: true, data: '2024-01-15', versao: 'v2.1' },

  contatos: [
    { nome: 'Salvador Arena Jr.', cargo: 'Proprietário / Diretor', celular: '(65) 99988-4455', email: 'salvador@arena.agr.br', decisor: true, aniversario: '1975-08-14' },
    { nome: 'Marina Arena Silva', cargo: 'Gerente Administrativa', celular: '(65) 99987-1122', email: 'marina@arena.agr.br', decisor: true, aniversario: '1982-03-22' },
    { nome: 'Carlos Eduardo Ribeiro', cargo: 'Gerente de Frota', celular: '(65) 99976-3344', email: 'carlos.frota@arena.agr.br', decisor: false, aniversario: '1978-11-08' },
    { nome: 'José Aparecido', cargo: 'Comprador Técnico', celular: '(65) 99965-8877', email: 'compras@arena.agr.br', decisor: false, aniversario: null },
  ],

  telefones: [
    { tipo: 'Fixo', numero: '(65) 3549-8800' },
    { tipo: 'Comercial', numero: '(65) 3549-8801' },
    { tipo: 'Fax', numero: '(65) 3549-8899' },
  ],
  emails: ['salvador@arena.agr.br', 'financeiro@arena.agr.br', 'compras@arena.agr.br'],
  home_page: 'www.agroarena.com.br',

  segmentacao: {
    grupo: 'Grandes Produtores MT',
    atividade: 'Agricultura extensiva · Soja/Milho/Algodão',
    rota: 'Rota 12 · Lucas-Sorriso-Nova Mutum',
    faturamento_declarado: 'R$ 80 a 120 milhões / ano',
    porte: 'GRANDE',
    hectares_totais: 4270,
    culturas: ['Soja', 'Milho safrinha', 'Algodão'],
  },

  frota: [
    { chassi: '1RW7250PVMR123456', modelo: 'Trator 7250R', linha: 'Tratores', ano: 2023, aquisicao: '2023-05-10', horas: 2840, garantia: 'Ativa · até 05/2028', status: 'Operacional' },
    { chassi: '1RW7250PVMR123457', modelo: 'Trator 7250R', linha: 'Tratores', ano: 2023, aquisicao: '2023-05-10', horas: 2650, garantia: 'Ativa · até 05/2028', status: 'Operacional' },
    { chassi: '1H0S780PXMR234567', modelo: 'Colheitadeira S780', linha: 'Colheitadeiras', ano: 2024, aquisicao: '2024-03-15', horas: 1240, garantia: 'Ativa · até 03/2027', status: 'Operacional' },
    { chassi: '1H0S780PXMR234568', modelo: 'Colheitadeira S780', linha: 'Colheitadeiras', ano: 2024, aquisicao: '2024-03-15', horas: 1180, garantia: 'Ativa · até 03/2027', status: 'Operacional' },
    { chassi: '1HDB44PXVMR345678', modelo: 'Plantadeira DB44', linha: 'Plantadeiras', ano: 2022, aquisicao: '2022-08-20', horas: 3520, garantia: 'Expirada · 08/2024', status: 'Manutenção agendada' },
    { chassi: '1H0S770PXMR456789', modelo: 'Colheitadeira S770', linha: 'Colheitadeiras', ano: 2020, aquisicao: '2020-04-05', horas: 5840, garantia: 'Expirada · 04/2023', status: 'Operacional' },
    { chassi: '1H4630PVXMR567890', modelo: 'Pulverizador 4630', linha: 'Pulverizadores', ano: 2021, aquisicao: '2021-06-18', horas: 4230, garantia: 'Expirada · 06/2024', status: 'Operacional' },
  ],

  oportunidades: [
    { id: 1517613, titulo: 'Renovação 2x Trator 7J → 8R', linha: 'Tratores', valor: 3_200_000, fase: 'Negociação', prob: 60, previsao: '2026-10-15', cen: 'José Rufino' },
    { id: 1517614, titulo: 'Kit plantio direto DB44 + acessórios', linha: 'Plantadeiras', valor: 890_000, fase: 'Proposta', prob: 40, previsao: '2026-09-30', cen: 'José Rufino' },
    { id: 1517615, titulo: 'Pulverizador autopropelido 4730', linha: 'Pulverizadores', valor: 1_450_000, fase: 'Orçamento', prob: 20, previsao: '2026-11-20', cen: 'José Rufino' },
  ],

  interacoes: [
    { data: '2026-08-22T10:15', tipo: 'ligacao', autor: 'José Rufino', assunto: 'Follow-up proposta 8R', resultado: 'Cliente confirmou análise até 30/08', obs: 'Marina pediu revisão do desconto financeiro. Passei para o gerente aprovar.' },
    { data: '2026-08-15T14:00', tipo: 'visita', autor: 'José Rufino', assunto: 'Apresentação linha 8R 250 e 8R 310', resultado: 'Interesse confirmado', obs: 'Salvador quer trocar 2x 7250R por 2x 8R 250. Frota mais nova reduz consumo diesel. Enviei catálogo técnico.' },
    { data: '2026-08-01T09:30', tipo: 'visita', autor: 'Ricardo Moretti', assunto: 'Manutenção preventiva DB44', resultado: 'Peças pedidas', obs: 'Cliente reclamou de barulho na transmissão. Técnico agendado 08/08. Peças no orçamento 1587091.' },
    { data: '2026-07-20T16:45', tipo: 'email', autor: 'sistema', assunto: 'Newsletter linha 6M', resultado: 'Enviado', obs: 'Aberto em 21/07 · Sem cliques' },
    { data: '2026-06-14T11:00', tipo: 'visita', autor: 'José Rufino', assunto: 'Monitoramento Cliente FY25', resultado: 'Cliente contactado', obs: 'Colheita safra 2026 iniciando em setembro. Frota atual atendendo bem.' },
  ],

  financeiro: {
    faturamento_ytd_2026: 4_320_000,
    faturamento_2025: 6_180_000,
    faturamento_2024: 8_450_000,
    limite_credito: 2_500_000,
    limite_usado: 890_000,
    limite_disponivel: 1_610_000,
    inadimplencia: 0,
    dias_atraso_max: 0,
    ultima_fatura: '2026-08-05',
  },

  alteracoes_pendentes: [
    { id: 782, campo: 'Limite de crédito', valor_atual: 'R$ 2.500.000,00', valor_solicitado: 'R$ 3.500.000,00', solicitante: 'José Rufino', data: '2026-08-20', motivo: 'Cliente pediu aumento para viabilizar 2x 8R 250. Faturamento 2024 e histórico zero atraso justificam.', status: 'Aguardando aprovação · Diretor Comercial' },
  ],
};

function renderClienteFicha() {
  const c = CLIENTE_DATA;

  return `
    <div class="cliente-header">
      <div class="cliente-header-left">
        <div class="cliente-avatar">${c.razao_social.substring(0,2).toUpperCase()}</div>
        <div class="cliente-header-info">
          <div class="cliente-eyebrow">
            <span class="badge badge-classe classe-a">Classe A</span>
            <span class="badge badge-success">${c.status}</span>
            <span class="cliente-cnpj">CNPJ ${c.cnpj}</span>
            <span class="cliente-id-code">#${c.id}</span>
          </div>
          <h1 class="cliente-nome">${c.razao_social}</h1>
          <div class="cliente-sub">
            ${c.nome_fantasia} · ${c.endereco_principal.cidade}/${c.endereco_principal.uf} · Regional ${c.regional}
          </div>
        </div>
      </div>
      <div class="cliente-header-actions">
        <button class="btn btn-ghost">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
          Ligar
        </button>
        <button class="btn btn-ghost">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
          E-mail
        </button>
        <button class="btn btn-secondary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 11l19-9-9 19-2-8-8-2z"/></svg>
          Nova visita
        </button>
        <button class="btn btn-primary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nova oportunidade
        </button>
      </div>
    </div>

    <!-- KPIs resumo -->
    <div class="kpi-grid" style="grid-template-columns:repeat(5,1fr);margin-bottom:24px">
      <div class="kpi">
        <span class="kpi-label">Faturamento YTD 2026</span>
        <span class="kpi-value">${fmtBRL(c.financeiro.faturamento_ytd_2026)}</span>
        <span class="kpi-hint">2025: ${fmtBRL(c.financeiro.faturamento_2025)}</span>
      </div>
      ${(() => {
        const novasClienteSA = (window.OPORTUNIDADES_NOVAS || []).filter(op => op.cliente_id === 84391);
        const totalPipe = c.oportunidades.reduce((s,o) => s+o.valor, 0) + novasClienteSA.reduce((s,o) => s+o.valor, 0);
        const totalCount = c.oportunidades.length + novasClienteSA.length;
        const hintExtra = novasClienteSA.length > 0 ? ` <span style=\"color:var(--jd-green);font-weight:600\">(+${novasClienteSA.length} nova${novasClienteSA.length > 1 ? 's' : ''})</span>` : '';
        return `
          <div class="kpi kpi-highlight">
            <span class="kpi-label">Pipeline aberto</span>
            <span class="kpi-value">${fmtBRL(totalPipe)}</span>
            <span class="kpi-hint">${totalCount} oportunidades${hintExtra}</span>
          </div>
        `;
      })()}
      <div class="kpi">
        <span class="kpi-label">Frota John Deere</span>
        <span class="kpi-value">${c.frota.length}</span>
        <span class="kpi-hint">${c.frota.filter(f => f.garantia.startsWith('Ativa')).length} em garantia · ${c.frota.filter(f => f.garantia.startsWith('Expirada')).length} fora</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Área total</span>
        <span class="kpi-value">${fmtNum(c.segmentacao.hectares_totais)} ha</span>
        <span class="kpi-hint">${c.enderecos_adicionais.filter(e => e.hectares > 0).length} fazendas</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Crédito disponível</span>
        <span class="kpi-value">${fmtBRL(c.financeiro.limite_disponivel)}</span>
        <span class="kpi-hint">de ${fmtBRL(c.financeiro.limite_credito)} · 0 inadimplência</span>
      </div>
    </div>

    ${c.alteracoes_pendentes.length > 0 ? `
      <div class="alteracao-pendente">
        <div class="alteracao-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
        </div>
        <div class="alteracao-body">
          <div class="alteracao-titulo">Alteração cadastral pendente · #${c.alteracoes_pendentes[0].id}</div>
          <div class="alteracao-detalhes">
            <strong>${c.alteracoes_pendentes[0].campo}:</strong>
            <span class="valor-de">${c.alteracoes_pendentes[0].valor_atual}</span>
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 12h14M13 5l7 7-7 7"/></svg>
            <span class="valor-para">${c.alteracoes_pendentes[0].valor_solicitado}</span>
          </div>
          <div class="alteracao-motivo">"${c.alteracoes_pendentes[0].motivo}"</div>
          <div class="alteracao-meta">Solicitado por ${c.alteracoes_pendentes[0].solicitante} em ${new Date(c.alteracoes_pendentes[0].data).toLocaleDateString('pt-BR')} · <strong>${c.alteracoes_pendentes[0].status}</strong></div>
        </div>
        <div class="alteracao-actions">
          <button class="btn btn-ghost btn-sm">Ver histórico</button>
          <button class="btn btn-secondary btn-sm">Cancelar solicitação</button>
        </div>
      </div>
    ` : ''}

    <!-- Tabs -->
    <div class="tabs">
      <button class="tab active" data-tab="visao">Visão geral</button>
      <button class="tab" data-tab="frota">Frota (${c.frota.length})</button>
      <button class="tab" data-tab="pos-vendas">Pós-vendas <span class="tab-badge tab-badge-red">${(typeof POSVENDAS_CLIENTE_84391 !== 'undefined' ? POSVENDAS_CLIENTE_84391.alertas_criticos.filter(a => a.cor === 'red').length : 0)}</span></button>
      <button class="tab" data-tab="oportunidades">Oportunidades (${c.oportunidades.length + (window.OPORTUNIDADES_NOVAS || []).filter(op => op.cliente_id === 84391).length})</button>
      <button class="tab" data-tab="historico">Histórico (${c.interacoes.length})</button>
      <button class="tab" data-tab="financeiro">Financeiro</button>
    </div>

    <!-- TAB: Visão geral -->
    <div class="tab-content active" data-tab-content="visao">
      <div class="ficha-grid">
        <!-- Coluna principal -->
        <div class="ficha-col-main">
          ${renderFichaBloco('identificacao', 'Identificação', `
            <div class="ficha-fields">
              <div class="ff"><label>Razão social</label><span>${c.razao_social}</span></div>
              <div class="ff"><label>Nome fantasia</label><span>${c.nome_fantasia}</span></div>
              <div class="ff"><label>CNPJ</label><span class="mono">${c.cnpj}</span></div>
              <div class="ff"><label>Inscrição estadual</label><span class="mono">${c.ie} <span class="muted">(${c.ie_uf})</span></span></div>
              <div class="ff"><label>Data de fundação</label><span>${new Date(c.data_fundacao).toLocaleDateString('pt-BR')}</span></div>
              <div class="ff"><label>Tipo</label><span>Pessoa Jurídica</span></div>
            </div>
          `)}

          ${renderFichaBloco('endereco', 'Endereço principal', `
            <div class="ficha-endereco">
              <div class="ff-full">
                <label>Logradouro</label>
                <span>${c.endereco_principal.logradouro}, ${c.endereco_principal.numero} · ${c.endereco_principal.complemento}</span>
              </div>
              <div class="ficha-fields">
                <div class="ff"><label>Bairro</label><span>${c.endereco_principal.bairro}</span></div>
                <div class="ff"><label>CEP</label><span class="mono">${c.endereco_principal.cep}</span></div>
                <div class="ff"><label>Cidade / UF</label><span>${c.endereco_principal.cidade} / ${c.endereco_principal.uf}</span></div>
                <div class="ff"><label>País</label><span>${c.endereco_principal.pais}</span></div>
                <div class="ff"><label>Coordenadas</label><span class="mono">${c.endereco_principal.latitude}, ${c.endereco_principal.longitude}</span></div>
                <div class="ff"><label>Rota comercial</label><span>${c.segmentacao.rota}</span></div>
              </div>
              <div class="ficha-map-placeholder">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                <span>Abrir no mapa · ${c.endereco_principal.cidade}/${c.endereco_principal.uf}</span>
              </div>
            </div>
          `)}

          ${renderFichaBloco('enderecos-adicionais', 'Endereços adicionais · Fazendas', `
            <div class="fazendas-list">
              ${c.enderecos_adicionais.map(e => `
                <div class="fazenda-row">
                  <div class="fazenda-icone">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 9h6M9 13h6M9 17h6"/></svg>
                  </div>
                  <div class="fazenda-body">
                    <div class="fazenda-nome">${e.tipo}</div>
                    <div class="fazenda-sub">${e.cidade}${e.hectares > 0 ? ` · ${fmtNum(e.hectares)} ha` : ''} · ${e.cultura}</div>
                  </div>
                </div>
              `).join('')}
            </div>
          `, true)}

          ${renderFichaBloco('contatos', 'Contatos', `
            <div class="contatos-list">
              ${c.contatos.map(ct => `
                <div class="contato-card">
                  <div class="contato-avatar">${ct.nome.split(' ').slice(0,2).map(p => p[0]).join('')}</div>
                  <div class="contato-body">
                    <div class="contato-linha1">
                      <strong>${ct.nome}</strong>
                      ${ct.decisor ? '<span class="badge-decisor">★ Decisor</span>' : ''}
                    </div>
                    <div class="contato-cargo">${ct.cargo}</div>
                    <div class="contato-canais">
                      <span class="contato-canal"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></svg>${ct.celular}</span>
                      <span class="contato-canal"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>${ct.email}</span>
                      ${ct.aniversario ? `<span class="contato-aniv">🎂 ${new Date(ct.aniversario).toLocaleDateString('pt-BR', {day:'2-digit', month:'2-digit'})}</span>` : ''}
                    </div>
                  </div>
                </div>
              `).join('')}
            </div>
          `, true)}
        </div>

        <!-- Coluna lateral -->
        <div class="ficha-col-side">
          ${renderFichaBloco('carteira', 'Vínculo à carteira', `
            <div class="carteira-info">
              <div class="carteira-cen">
                <div class="cen-avatar">${c.cen_dono.avatar}</div>
                <div>
                  <div class="cen-nome">${c.cen_dono.nome}</div>
                  <div class="cen-role">CEN Comercial · ${c.regional}</div>
                </div>
              </div>
              <div class="carteira-cen">
                <div class="cen-avatar cen-avatar-secondary">${c.cen_pos_venda.avatar}</div>
                <div>
                  <div class="cen-nome">${c.cen_pos_venda.nome}</div>
                  <div class="cen-role">Pós-venda / Assistência</div>
                </div>
              </div>
            </div>
          `)}

          ${renderFichaBloco('segmentacao', 'Segmentação comercial', `
            <div class="ficha-fields ficha-fields-col">
              <div class="ff"><label>Classe ABC</label><span><strong style="color:var(--jd-green)">A · Alto valor</strong></span></div>
              <div class="ff"><label>Grupo</label><span>${c.segmentacao.grupo}</span></div>
              <div class="ff"><label>Atividade</label><span>${c.segmentacao.atividade}</span></div>
              <div class="ff"><label>Porte</label><span>${c.segmentacao.porte}</span></div>
              <div class="ff"><label>Faturamento declarado</label><span>${c.segmentacao.faturamento_declarado}</span></div>
              <div class="ff"><label>Área plantada</label><span>${fmtNum(c.segmentacao.hectares_totais)} ha</span></div>
              <div class="ff"><label>Culturas</label>
                <div class="cultura-tags">
                  ${c.segmentacao.culturas.map(cu => `<span class="cultura-tag">${cu}</span>`).join('')}
                </div>
              </div>
            </div>
          `)}

          ${renderFichaBloco('preferencias', 'Preferências de contato · LGPD', `
            <div class="prefs-list">
              ${Object.entries(c.preferencias_contato).map(([canal, ativo]) => `
                <label class="pref-item ${ativo ? 'ativo' : 'inativo'}">
                  <span class="pref-check">${ativo ? '✓' : '✗'}</span>
                  <span class="pref-label">${canal.charAt(0).toUpperCase() + canal.slice(1)}</span>
                </label>
              `).join('')}
            </div>
            <div class="lgpd-nota">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              Consentimento LGPD ativo desde ${new Date(c.consentimento_lgpd.data).toLocaleDateString('pt-BR')} · ${c.consentimento_lgpd.versao}
            </div>
          `)}

          ${renderFichaBloco('auditoria', 'Auditoria', `
            <div class="audit-list">
              <div class="audit-row">
                <span class="audit-label">Origem inclusão</span>
                <span class="audit-value mono">${c.origem_inclusao}</span>
              </div>
              <div class="audit-row">
                <span class="audit-label">Última alteração</span>
                <span class="audit-value mono">${c.origem_alteracao}</span>
              </div>
              <div class="audit-row">
                <span class="audit-label">ID Vortice</span>
                <span class="audit-value mono">#${c.id}</span>
              </div>
            </div>
            <button class="btn btn-ghost btn-sm" style="margin-top:12px">Solicitar alteração cadastral</button>
          `)}
        </div>
      </div>
    </div>

    <!-- TAB: Frota -->
    <div class="tab-content" data-tab-content="frota">
      <div class="frota-summary">
        ${['Tratores', 'Colheitadeiras', 'Plantadeiras', 'Pulverizadores'].map(linha => {
          const items = c.frota.filter(f => f.linha === linha);
          if (items.length === 0) return '';
          return `
            <div class="frota-summary-card">
              <div class="frota-linha-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="8" width="18" height="10" rx="2"/><circle cx="8" cy="18" r="3"/><circle cx="17" cy="18" r="3"/></svg></div>
              <div>
                <div class="frota-linha-count">${items.length}</div>
                <div class="frota-linha-label">${linha}</div>
              </div>
            </div>
          `;
        }).join('')}
      </div>
      <div class="card" style="margin-top:16px">
        <div class="table-wrap">
          <table>
            <thead>
              <tr><th>Chassi</th><th>Modelo</th><th>Linha</th><th class="num">Ano</th><th class="num">Horas</th><th>Garantia</th><th>Status</th><th></th></tr>
            </thead>
            <tbody>
              ${c.frota.map(f => `
                <tr>
                  <td class="mono">${f.chassi}</td>
                  <td><strong>${f.modelo}</strong></td>
                  <td>${f.linha}</td>
                  <td class="num">${f.ano}</td>
                  <td class="num">${fmtNum(f.horas)}</td>
                  <td>${f.garantia.startsWith('Ativa') ? `<span class="badge badge-success">${f.garantia}</span>` : `<span class="badge badge-muted">${f.garantia}</span>`}</td>
                  <td>${f.status === 'Manutenção agendada' ? `<span class="badge badge-warning">${f.status}</span>` : `<span class="badge badge-info">${f.status}</span>`}</td>
                  <td><a href="#/equipamentos/${f.chassi}" class="btn-icon btn-icon-inline" title="Ver ficha do equipamento" style="text-decoration:none;display:inline-flex;align-items:center;justify-content:center;" onclick="if('${f.chassi}' !== '1RW7250PVMR123456'){event.preventDefault();alert('Ficha detalhada disponível só para o chassi 1RW7250PVMR123456 no protótipo')}">→</a></td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- TAB: Pós-vendas -->
    ${typeof renderPosVendasTab === 'function' ? renderPosVendasTab() : ''}

    <!-- TAB: Oportunidades -->
    <div class="tab-content" data-tab-content="oportunidades">
      <div class="oport-list">
        ${(window.OPORTUNIDADES_NOVAS || []).filter(op => op.cliente_id === 84391).map(op => `
          <div class="oport-card oport-nova" onclick="mostrarModalFichaIndispo('${op.id}', '${op.titulo.replace(/'/g, "\\'")}', true)" style="cursor:pointer">
            <div class="oport-body">
              <div class="oport-linha1">
                <strong>${op.titulo}</strong>
                <span class="badge badge-info">${op.linha}</span>
                <span class="oport-id mono">${op.id}</span>
                <span class="badge-nova-inline">NOVA</span>
              </div>
              <div class="oport-meta">
                <span>CEN: João Ribeiro</span>
                <span>Previsão: ${new Date(op.previsao).toLocaleDateString('pt-BR')}</span>
                <span>Modelo: ${op.modelo}</span>
              </div>
            </div>
            <div class="oport-fase">
              <div class="oport-fase-label">${PIPELINE_FASES.find(f => f.id === op.fase).label}</div>
              <div class="oport-prob">${op.probabilidade}% probabilidade</div>
              <div class="oport-prob-bar">
                <div class="oport-prob-fill" style="width:${op.probabilidade}%;background:${op.probabilidade >= 60 ? 'var(--jd-green)' : (op.probabilidade >= 30 ? '#F59E0B' : '#94A3B8')}"></div>
              </div>
            </div>
            <div class="oport-valor">
              <div class="oport-valor-label">Valor</div>
              <div class="oport-valor-num">${fmtBRL(op.valor)}</div>
              <div class="oport-valor-ponderado">Ponderado: ${fmtBRL(op.valor * op.probabilidade / 100)}</div>
            </div>
          </div>
        `).join('')}
        ${c.oportunidades.map(o => `
          <div class="oport-card" ${o.id === 1517613 ? `style="cursor:pointer" onclick="window.location.hash='#/oportunidades/${o.id}'"` : `onclick="mostrarModalFichaIndispo('${o.id}', '${(o.titulo||'').replace(/'/g,"\\'")}', false)" style="cursor:pointer"`}>
            <div class="oport-body">
              <div class="oport-linha1">
                <strong>${o.titulo}</strong>
                <span class="badge badge-info">${o.linha}</span>
                <span class="oport-id">#${o.id}</span>
              </div>
              <div class="oport-meta">
                <span>CEN: ${o.cen}</span>
                <span>Previsão: ${new Date(o.previsao).toLocaleDateString('pt-BR')}</span>
              </div>
            </div>
            <div class="oport-fase">
              <div class="oport-fase-label">${o.fase}</div>
              <div class="oport-prob">${o.prob}% probabilidade</div>
              <div class="oport-prob-bar">
                <div class="oport-prob-fill" style="width:${o.prob}%;background:${o.prob >= 60 ? 'var(--jd-green)' : (o.prob >= 30 ? '#F59E0B' : '#94A3B8')}"></div>
              </div>
            </div>
            <div class="oport-valor">
              <div class="oport-valor-label">Valor</div>
              <div class="oport-valor-num">${fmtBRL(o.valor)}</div>
              <div class="oport-valor-ponderado">Ponderado: ${fmtBRL(o.valor * o.prob / 100)}</div>
            </div>
          </div>
        `).join('')}
      </div>
    </div>

    <!-- TAB: Histórico -->
    <div class="tab-content" data-tab-content="historico">
      <div class="timeline">
        ${c.interacoes.map(i => {
          const meta = { visita: {icon:'📍', color:'#16A34A', label:'Visita'}, ligacao: {icon:'📞', color:'#7C3AED', label:'Ligação'}, email: {icon:'✉', color:'#0284C7', label:'E-mail'}, whatsapp: {icon:'💬', color:'#059669', label:'WhatsApp'} }[i.tipo] || {icon:'●', color:'#64748B', label:i.tipo};
          return `
            <div class="timeline-item">
              <div class="timeline-marker" style="background:${meta.color}">${meta.icon}</div>
              <div class="timeline-body">
                <div class="timeline-linha1">
                  <strong>${i.assunto}</strong>
                  <span class="timeline-tipo" style="color:${meta.color}">${meta.label}</span>
                </div>
                <div class="timeline-meta">
                  ${fmtDataHora(i.data)} · ${i.autor} · <strong>${i.resultado}</strong>
                </div>
                ${i.obs ? `<div class="timeline-obs">${i.obs}</div>` : ''}
              </div>
            </div>
          `;
        }).join('')}
      </div>
    </div>

    <!-- TAB: Financeiro -->
    <div class="tab-content" data-tab-content="financeiro">
      <div class="kpi-grid" style="grid-template-columns:repeat(4,1fr);margin-bottom:20px">
        <div class="kpi">
          <span class="kpi-label">Faturamento 2024</span>
          <span class="kpi-value">${fmtBRL(c.financeiro.faturamento_2024)}</span>
        </div>
        <div class="kpi">
          <span class="kpi-label">Faturamento 2025</span>
          <span class="kpi-value">${fmtBRL(c.financeiro.faturamento_2025)}</span>
        </div>
        <div class="kpi kpi-highlight">
          <span class="kpi-label">Faturamento YTD 2026</span>
          <span class="kpi-value">${fmtBRL(c.financeiro.faturamento_ytd_2026)}</span>
          <span class="kpi-hint">Projeção anual: ${fmtBRL(c.financeiro.faturamento_ytd_2026 * 12/8)}</span>
        </div>
        <div class="kpi">
          <span class="kpi-label">Dias em atraso · máx.</span>
          <span class="kpi-value" style="color:var(--jd-green)">${c.financeiro.dias_atraso_max}</span>
          <span class="kpi-hint">Zero inadimplência histórica</span>
        </div>
      </div>
      <div class="card">
        <div class="card-header">
          <div>
            <div class="card-title">Limite de crédito</div>
            <div class="card-subtitle">Utilização do limite aprovado</div>
          </div>
        </div>
        <div style="padding:24px">
          <div class="credit-bar-wrap">
            <div class="credit-bar-track">
              <div class="credit-bar-used" style="width:${(c.financeiro.limite_usado/c.financeiro.limite_credito*100).toFixed(1)}%"></div>
            </div>
            <div class="credit-bar-legend">
              <span><strong>${fmtBRL(c.financeiro.limite_usado)}</strong> utilizados</span>
              <span><strong>${fmtBRL(c.financeiro.limite_disponivel)}</strong> disponíveis</span>
              <span>Limite total: <strong>${fmtBRL(c.financeiro.limite_credito)}</strong></span>
            </div>
          </div>
          <div class="funil-scope-note" style="margin-top:20px">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 9v2m0 4h.01"/><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/></svg>
            <span>Dados financeiros vêm do Protheus (SA1 + SE1) por sync noturno. Última atualização: 25/08/2026 03:14. Alterações de limite requerem workflow de aprovação.</span>
          </div>
        </div>
      </div>
    </div>
  `;
}

function renderFichaBloco(id, titulo, conteudo, spacious = false) {
  return `
    <div class="ficha-bloco ${spacious ? 'ficha-bloco-full' : ''}" data-bloco="${id}">
      <div class="ficha-bloco-header">
        <div class="ficha-bloco-titulo">${titulo}</div>
        <button class="btn-icon-sm" title="Editar bloco" onclick="alert('Modo edição de bloco (protótipo — vira formulário inline no MVP)')">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
        </button>
      </div>
      <div class="ficha-bloco-body">
        ${conteudo}
      </div>
    </div>
  `;
}

function mountClienteFicha() {
  document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
      const target = tab.dataset.tab;
      document.querySelectorAll('.tab').forEach(t => t.classList.toggle('active', t === tab));
      document.querySelectorAll('.tab-content').forEach(c => c.classList.toggle('active', c.dataset.tabContent === target));
      // Mount lazy dos gráficos da aba pós-vendas (Chart.js precisa do canvas visível)
      if (target === 'pos-vendas' && typeof mountPosVendasCharts === 'function') {
        setTimeout(mountPosVendasCharts, 50);
      }
    });
  });
}

// ===== FICHA DO EQUIPAMENTO =====
const EQUIPAMENTO_DATA = {
  chassi: '1RW7250PVMR123456',
  numero_serie: '7250R-2023-BR-4487',
  modelo: 'Trator 7250R',
  linha: 'Tratores 7J · Alta potência',
  marca: 'John Deere',
  potencia: '250 CV',
  potencia_faixa: 'n) >= 250 CV',
  ano_modelo: 2023,
  ano_fabricacao: 2023,
  cor: 'Verde/Amarelo padrão JD',
  transmissao: 'e23 · 23 velocidades',
  cabine: 'CommandView III · Ar condicionado',
  tracao: '4x4 MFWD',
  pneus_diant: '380/85R30',
  pneus_tras: '520/85R42',
  peso_operacional: '11.850 kg',
  tanque_diesel: '620 L',

  status: 'Operacional',
  cliente: { id: 84391, razao: 'Agroindustrial Salvador Arena Ltda', classe: 'A' },
  localizacao: 'Fazenda Boa Vista · Sorriso/MT',
  fazenda_id: 'fbv-01',
  operador_principal: 'Jailson Ferreira',

  aquisicao: {
    tipo: 'Venda direta · Financiamento BNDES',
    data_venda: '2023-05-10',
    data_entrega: '2023-05-24',
    vendedor: 'José Rufino',
    valor_faturado: 780_000,
    valor_lista: 895_000,
    desconto: 115_000,
    pedido_protheus: 'PV-2023-04891',
    nota_fiscal: 'NF-e 001.847.559',
    concessionaria: 'Tracbel Lucas do Rio Verde',
  },

  garantia: {
    tipo: 'Fábrica + Estendida PowerGard',
    inicio: '2023-05-24',
    fim: '2028-05-24',
    horas_limite: 8000,
    horas_atual: 2840,
    percentual_uso: 35.5,
    cobertura: 'Motor, transmissão, sistemas hidráulicos, elétricos e AutoTrac',
    exclusoes: 'Pneus, filtros, óleos, itens de manutenção regular',
    contrato: 'PGD-2023-4487',
  },

  horas_operacao: {
    atual: 2840,
    media_diaria_30d: 8.2,
    media_diaria_90d: 6.7,
    ultima_leitura: '2026-08-24T18:30',
    fonte: 'JD Operations Center · Sync 4x/dia',
    proxima_revisao_horas: 3000,
    horas_para_proxima_revisao: 160,
    proxima_revisao_previsao: '2026-09-14',
  },

  historico_horas: [
    { data: '2026-02', horas: 1980 }, { data: '2026-03', horas: 2120 },
    { data: '2026-04', horas: 2280 }, { data: '2026-05', horas: 2410 },
    { data: '2026-06', horas: 2550 }, { data: '2026-07', horas: 2700 },
    { data: '2026-08', horas: 2840 },
  ],

  revisoes: [
    { id: 8, data: '2026-05-18', horas: 2450, tipo: 'Preventiva 2500h', tecnico: 'Ronaldo Pires', duracao_h: 6, custo: 4850, os: 'OS-2026-01887', status: 'Concluída', obs: 'Troca de óleo hidráulico, filtros, verificação sistema AutoTrac. Sem anomalia.' },
    { id: 7, data: '2025-11-12', horas: 1820, tipo: 'Preventiva 1500h', tecnico: 'Ronaldo Pires', duracao_h: 4, custo: 2340, os: 'OS-2025-04521', status: 'Concluída', obs: 'Troca óleo motor + filtros. Cliente relatou pequena vibração no cardan — verificado, dentro do normal.' },
    { id: 6, data: '2025-04-08', horas: 890, tipo: 'Preventiva 750h', tecnico: 'André Kobayashi', duracao_h: 3, custo: 1180, os: 'OS-2025-01443', status: 'Concluída', obs: 'Primeira revisão pós-safra. Tudo dentro do padrão fábrica.' },
    { id: 5, data: '2024-08-22', horas: 420, tipo: 'Preventiva 250h', tecnico: 'André Kobayashi', duracao_h: 2, custo: 890, os: 'OS-2024-03102', status: 'Concluída', obs: 'Revisão obrigatória de garantia. Assinada por Salvador Jr.' },
    { id: 4, data: '2024-03-15', horas: 180, tipo: 'Preventiva 100h', tecnico: 'André Kobayashi', duracao_h: 2, custo: 620, os: 'OS-2024-00891', status: 'Concluída', obs: 'Ajuste inicial, obrigatória de garantia.' },
  ],

  chamados_abertos: [
    { id: 91245, tipo: 'Preventiva 3000h', prioridade: 'Média', abertura: '2026-08-15', previsao: '2026-09-14', tecnico_agendado: 'Ronaldo Pires', descricao: 'Revisão programada 3000h — próxima janela: 14/09/2026 (10 dias antes do plantio começar).' },
  ],

  pecas: [
    { data: '2026-05-18', codigo: 'RE504914', descricao: 'Filtro de óleo motor', qtd: 1, valor: 187, os: 'OS-2026-01887' },
    { data: '2026-05-18', codigo: 'RE522688', descricao: 'Filtro hidráulico', qtd: 2, valor: 445, os: 'OS-2026-01887' },
    { data: '2026-05-18', codigo: 'AT445962', descricao: 'Óleo Torq-Gard 15W40 (balde 20L)', qtd: 1, valor: 890, os: 'OS-2026-01887' },
    { data: '2025-11-12', codigo: 'RE504914', descricao: 'Filtro de óleo motor', qtd: 1, valor: 178, os: 'OS-2025-04521' },
    { data: '2025-11-12', codigo: 'AT445962', descricao: 'Óleo Torq-Gard 15W40 (balde 20L)', qtd: 1, valor: 845, os: 'OS-2025-04521' },
    { data: '2025-04-08', codigo: 'RE504914', descricao: 'Filtro de óleo motor', qtd: 1, valor: 172, os: 'OS-2025-01443' },
  ],

  telemetria: {
    ultimo_sync: '2026-08-24T18:30',
    consumo_medio_lh: 21.4,
    consumo_ideal_lh: 19.8,
    eficiencia: 92.5,
    horas_ociosas_pct: 8.2,
    velocidade_media_trabalho: 8.4,
    autotrac_uso_pct: 87,
    alertas_ativos: 0,
    codigos_falha_30d: 2,
    codigos_falha_historico: 14,
  },
};

function renderEquipamentoFicha() {
  const e = EQUIPAMENTO_DATA;
  const garantiaAtiva = new Date(e.garantia.fim) > new Date();
  const diasParaExpirar = Math.round((new Date(e.garantia.fim) - new Date()) / 86400000);

  return `
    <div class="equip-header">
      <div class="equip-header-left">
        <div class="equip-icon-hero">
          <svg viewBox="0 0 100 60" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="20" y="20" width="60" height="25" rx="3" fill="currentColor" fill-opacity="0.15"/>
            <rect x="35" y="10" width="30" height="15" rx="2" fill="currentColor" fill-opacity="0.25"/>
            <circle cx="30" cy="50" r="8" fill="currentColor" fill-opacity="0.3"/>
            <circle cx="70" cy="50" r="10" fill="currentColor" fill-opacity="0.3"/>
            <line x1="20" y1="30" x2="80" y2="30"/>
          </svg>
        </div>
        <div class="equip-header-info">
          <div class="equip-eyebrow">
            <span class="badge badge-jd">John Deere · ${e.linha}</span>
            <span class="badge badge-success">${e.status}</span>
            <span class="equip-chassi mono">CHASSI ${e.chassi}</span>
          </div>
          <h1 class="cliente-nome">${e.modelo}</h1>
          <div class="cliente-sub">
            ${e.potencia} · ${e.ano_modelo} · N/S ${e.numero_serie} ·
            <a href="#/clientes/${e.cliente.id}" class="cliente-link">${e.cliente.razao}</a>
          </div>
        </div>
      </div>
      <div class="cliente-header-actions">
        <button class="btn btn-ghost">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg>
          Copiar chassi
        </button>
        <button class="btn btn-ghost">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
          Exportar histórico
        </button>
        <button class="btn btn-secondary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
          Ver no JD Connect
        </button>
        <button class="btn btn-primary">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Abrir chamado
        </button>
      </div>
    </div>

    <!-- KPIs -->
    <div class="kpi-grid" style="grid-template-columns:repeat(5,1fr);margin-bottom:24px">
      <div class="kpi">
        <span class="kpi-label">Horas de operação</span>
        <span class="kpi-value">${fmtNum(e.horas_operacao.atual)}<span style="font-size:14px;color:var(--text-tertiary);font-weight:500"> h</span></span>
        <span class="kpi-hint">Última leitura: ${fmtDataHora(e.horas_operacao.ultima_leitura)}</span>
      </div>
      <div class="kpi kpi-highlight">
        <span class="kpi-label">Garantia</span>
        <span class="kpi-value">${diasParaExpirar > 365 ? Math.round(diasParaExpirar/365) + ' anos' : diasParaExpirar + ' dias'}</span>
        <span class="kpi-hint">Ativa até ${new Date(e.garantia.fim).toLocaleDateString('pt-BR')}</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Uso da garantia</span>
        <span class="kpi-value">${e.garantia.percentual_uso.toFixed(1)}%</span>
        <span class="kpi-hint">${fmtNum(e.garantia.horas_atual)}h de ${fmtNum(e.garantia.horas_limite)}h</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Próxima revisão</span>
        <span class="kpi-value" style="color:#F59E0B">${e.horas_operacao.horas_para_proxima_revisao}<span style="font-size:14px;color:var(--text-tertiary);font-weight:500"> h</span></span>
        <span class="kpi-hint">Previsto ${new Date(e.horas_operacao.proxima_revisao_previsao).toLocaleDateString('pt-BR')} · ${e.horas_operacao.proxima_revisao_horas}h</span>
      </div>
      <div class="kpi">
        <span class="kpi-label">Eficiência (telemetria)</span>
        <span class="kpi-value" style="color:var(--jd-green)">${e.telemetria.eficiencia}%</span>
        <span class="kpi-hint">Consumo: ${e.telemetria.consumo_medio_lh} L/h · ideal ${e.telemetria.consumo_ideal_lh}</span>
      </div>
    </div>

    <!-- Alertas -->
    ${e.chamados_abertos.length > 0 ? `
      <div class="alteracao-pendente" style="background:linear-gradient(90deg,rgba(2,132,199,.06),transparent);border-color:rgba(2,132,199,.3);border-left-color:#0284C7">
        <div class="alteracao-icon" style="color:#075985">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>
        </div>
        <div class="alteracao-body">
          <div class="alteracao-titulo">Chamado aberto · Preventiva 3000h · #${e.chamados_abertos[0].id}</div>
          <div class="alteracao-detalhes">
            <span>Prioridade <strong>${e.chamados_abertos[0].prioridade}</strong> · Técnico agendado: <strong>${e.chamados_abertos[0].tecnico_agendado}</strong> · Previsão: <strong>${new Date(e.chamados_abertos[0].previsao).toLocaleDateString('pt-BR')}</strong></span>
          </div>
          <div class="alteracao-motivo">${e.chamados_abertos[0].descricao}</div>
        </div>
        <div class="alteracao-actions">
          <button class="btn btn-ghost btn-sm">Ver chamado</button>
          <button class="btn btn-secondary btn-sm">Reagendar</button>
        </div>
      </div>
    ` : ''}

    <!-- Tabs -->
    <div class="tabs">
      <button class="tab active" data-tab="visao">Visão geral</button>
      <button class="tab" data-tab="revisoes">Revisões (${e.revisoes.length})</button>
      <button class="tab" data-tab="pecas">Peças aplicadas (${e.pecas.length})</button>
      <button class="tab" data-tab="telemetria">Telemetria</button>
      <button class="tab" data-tab="comercial">Comercial</button>
    </div>

    <!-- Visão geral -->
    <div class="tab-content active" data-tab-content="visao">
      <div class="ficha-grid">
        <div class="ficha-col-main">
          ${renderFichaBloco('identificacao', 'Identificação do equipamento', `
            <div class="ficha-fields">
              <div class="ff"><label>Chassi (VIN)</label><span class="mono">${e.chassi}</span></div>
              <div class="ff"><label>Número de série</label><span class="mono">${e.numero_serie}</span></div>
              <div class="ff"><label>Marca / Modelo</label><span><strong>${e.marca}</strong> · ${e.modelo}</span></div>
              <div class="ff"><label>Linha</label><span>${e.linha}</span></div>
              <div class="ff"><label>Potência</label><span>${e.potencia} <span class="muted">(${e.potencia_faixa})</span></span></div>
              <div class="ff"><label>Ano modelo / fabricação</label><span>${e.ano_modelo} / ${e.ano_fabricacao}</span></div>
              <div class="ff"><label>Transmissão</label><span>${e.transmissao}</span></div>
              <div class="ff"><label>Tração</label><span>${e.tracao}</span></div>
            </div>
          `)}

          ${renderFichaBloco('tecnicas', 'Especificações técnicas', `
            <div class="ficha-fields">
              <div class="ff"><label>Cabine</label><span>${e.cabine}</span></div>
              <div class="ff"><label>Peso operacional</label><span>${e.peso_operacional}</span></div>
              <div class="ff"><label>Tanque de diesel</label><span>${e.tanque_diesel}</span></div>
              <div class="ff"><label>Cor</label><span>${e.cor}</span></div>
              <div class="ff"><label>Pneus dianteiros</label><span class="mono">${e.pneus_diant}</span></div>
              <div class="ff"><label>Pneus traseiros</label><span class="mono">${e.pneus_tras}</span></div>
            </div>
          `)}

          ${renderFichaBloco('horas', 'Evolução de horas operacionais', `
            <div class="horas-chart-wrap">
              <div class="horas-chart">
                ${(() => {
                  const max = Math.max(...e.historico_horas.map(h => h.horas));
                  return e.historico_horas.map(h => {
                    const pct = (h.horas / max * 100).toFixed(0);
                    const [y, m] = h.data.split('-');
                    const mesLabel = ['jan','fev','mar','abr','mai','jun','jul','ago','set','out','nov','dez'][parseInt(m)-1];
                    return `
                      <div class="horas-bar-col">
                        <div class="horas-bar-val">${fmtNum(h.horas)}</div>
                        <div class="horas-bar" style="height:${pct}%"><div class="horas-bar-fill"></div></div>
                        <div class="horas-bar-label">${mesLabel}/${y.substring(2)}</div>
                      </div>
                    `;
                  }).join('');
                })()}
              </div>
              <div class="horas-stats">
                <div class="hs-item"><span class="hs-label">Média diária (30d)</span><span class="hs-value">${e.horas_operacao.media_diaria_30d} h</span></div>
                <div class="hs-item"><span class="hs-label">Média diária (90d)</span><span class="hs-value">${e.horas_operacao.media_diaria_90d} h</span></div>
                <div class="hs-item"><span class="hs-label">Ganho mensal médio</span><span class="hs-value">+${Math.round((e.historico_horas[e.historico_horas.length-1].horas - e.historico_horas[0].horas) / (e.historico_horas.length-1))} h</span></div>
                <div class="hs-item"><span class="hs-label">Fonte</span><span class="hs-value" style="font-size:11px">${e.horas_operacao.fonte}</span></div>
              </div>
            </div>
          `, true)}
        </div>

        <div class="ficha-col-side">
          ${renderFichaBloco('cliente', 'Cliente atual', `
            <a href="#/clientes/${e.cliente.id}" class="cliente-link-card">
              <div class="cliente-avatar" style="width:44px;height:44px;font-size:16px;border-radius:10px">AG</div>
              <div>
                <div class="cen-nome">${e.cliente.razao}</div>
                <div class="cen-role">Classe ${e.cliente.classe} · #${e.cliente.id}</div>
              </div>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-left:auto"><polyline points="9 18 15 12 9 6"/></svg>
            </a>
            <div style="border-top:1px dashed var(--border-primary);margin:12px 0;padding-top:12px">
              <div class="audit-row">
                <span class="audit-label">Localização atual</span>
                <span class="audit-value">${e.localizacao}</span>
              </div>
              <div class="audit-row">
                <span class="audit-label">Operador principal</span>
                <span class="audit-value">${e.operador_principal}</span>
              </div>
            </div>
          `)}

          ${renderFichaBloco('garantia', 'Garantia PowerGard', `
            <div class="garantia-status">
              <div class="garantia-badge garantia-ativa">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/><polyline points="9 12 11 14 15 10"/></svg>
                <span>Ativa</span>
              </div>
              <div class="garantia-linha">
                <span>Duração</span>
                <strong>${new Date(e.garantia.inicio).toLocaleDateString('pt-BR')} → ${new Date(e.garantia.fim).toLocaleDateString('pt-BR')}</strong>
              </div>
              <div class="garantia-linha">
                <span>Uso</span>
                <strong>${e.garantia.percentual_uso.toFixed(1)}% (${fmtNum(e.garantia.horas_atual)}/${fmtNum(e.garantia.horas_limite)}h)</strong>
              </div>
              <div class="garantia-bar-track">
                <div class="garantia-bar-fill" style="width:${e.garantia.percentual_uso}%"></div>
              </div>
              <div class="garantia-info">
                <div><label>Tipo</label><span>${e.garantia.tipo}</span></div>
                <div><label>Contrato</label><span class="mono">${e.garantia.contrato}</span></div>
                <div><label>Cobre</label><span style="font-size:12px">${e.garantia.cobertura}</span></div>
                <div><label>Não cobre</label><span style="font-size:12px;color:var(--text-tertiary)">${e.garantia.exclusoes}</span></div>
              </div>
            </div>
          `)}

          ${renderFichaBloco('auditoria', 'Auditoria', `
            <div class="audit-list">
              <div class="audit-row">
                <span class="audit-label">Origem inclusão</span>
                <span class="audit-value mono">Protheus-Sync · 24/05/2023</span>
              </div>
              <div class="audit-row">
                <span class="audit-label">Última atualização</span>
                <span class="audit-value mono">JD Sync · 24/08/2026 18:30</span>
              </div>
              <div class="audit-row">
                <span class="audit-label">Pedido Protheus</span>
                <span class="audit-value mono">${e.aquisicao.pedido_protheus}</span>
              </div>
              <div class="audit-row">
                <span class="audit-label">Nota fiscal</span>
                <span class="audit-value mono">${e.aquisicao.nota_fiscal}</span>
              </div>
            </div>
          `)}
        </div>
      </div>
    </div>

    <!-- Revisões -->
    <div class="tab-content" data-tab-content="revisoes">
      <div class="revisoes-timeline">
        ${e.revisoes.map((r, i) => `
          <div class="revisao-item">
            <div class="revisao-marker">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>
            </div>
            <div class="revisao-body">
              <div class="revisao-header">
                <div>
                  <div class="revisao-titulo">${r.tipo}</div>
                  <div class="revisao-meta">${new Date(r.data).toLocaleDateString('pt-BR')} · ${fmtNum(r.horas)}h · ${r.tecnico} · ${r.duracao_h}h de execução</div>
                </div>
                <div class="revisao-actions">
                  <span class="revisao-os mono">${r.os}</span>
                  <span class="revisao-custo">${fmtBRL(r.custo)}</span>
                </div>
              </div>
              <div class="revisao-obs">${r.obs}</div>
            </div>
          </div>
        `).join('')}
      </div>
      <div class="funil-scope-note" style="margin-top:20px">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
        <span>Revisões vêm do módulo TOTVS Protheus SIGAOFI (Oficina). O CRM só exibe; alterações são feitas no Protheus.</span>
      </div>
    </div>

    <!-- Peças aplicadas -->
    <div class="tab-content" data-tab-content="pecas">
      <div class="card">
        <div class="card-header">
          <div>
            <div class="card-title">Peças aplicadas · ${e.pecas.length} itens</div>
            <div class="card-subtitle">Total gasto em peças: ${fmtBRL(e.pecas.reduce((s,p) => s + p.valor*p.qtd, 0))}</div>
          </div>
          <button class="btn btn-ghost btn-sm">Exportar CSV</button>
        </div>
        <div class="table-wrap">
          <table>
            <thead>
              <tr><th>Data</th><th>Código</th><th>Descrição</th><th class="num">Qtd</th><th class="num">Valor unit.</th><th class="num">Total</th><th>OS</th></tr>
            </thead>
            <tbody>
              ${e.pecas.map(p => `
                <tr>
                  <td>${new Date(p.data).toLocaleDateString('pt-BR')}</td>
                  <td class="mono">${p.codigo}</td>
                  <td>${p.descricao}</td>
                  <td class="num">${p.qtd}</td>
                  <td class="num">${fmtBRL(p.valor)}</td>
                  <td class="num"><strong>${fmtBRL(p.valor * p.qtd)}</strong></td>
                  <td class="mono">${p.os}</td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Telemetria -->
    <div class="tab-content" data-tab-content="telemetria">
      <div class="kpi-grid" style="grid-template-columns:repeat(4,1fr);margin-bottom:20px">
        <div class="kpi">
          <span class="kpi-label">Consumo médio</span>
          <span class="kpi-value">${e.telemetria.consumo_medio_lh} <span style="font-size:14px;color:var(--text-tertiary);font-weight:500">L/h</span></span>
          <span class="kpi-hint">Ideal para modelo: ${e.telemetria.consumo_ideal_lh} L/h · <span style="color:#F59E0B">+${((e.telemetria.consumo_medio_lh/e.telemetria.consumo_ideal_lh - 1)*100).toFixed(1)}%</span></span>
        </div>
        <div class="kpi">
          <span class="kpi-label">Horas ociosas</span>
          <span class="kpi-value">${e.telemetria.horas_ociosas_pct}%</span>
          <span class="kpi-hint">Motor ligado sem trabalho</span>
        </div>
        <div class="kpi kpi-highlight">
          <span class="kpi-label">Uso do AutoTrac</span>
          <span class="kpi-value">${e.telemetria.autotrac_uso_pct}%</span>
          <span class="kpi-hint">Piloto automático GPS</span>
        </div>
        <div class="kpi">
          <span class="kpi-label">Códigos de falha (30d)</span>
          <span class="kpi-value" style="color:${e.telemetria.codigos_falha_30d > 5 ? '#DC2626' : 'var(--jd-green)'}">${e.telemetria.codigos_falha_30d}</span>
          <span class="kpi-hint">Histórico: ${e.telemetria.codigos_falha_historico} · ${e.telemetria.alertas_ativos} ativos</span>
        </div>
      </div>
      <div class="card">
        <div class="card-header">
          <div>
            <div class="card-title">Telemetria John Deere Operations Center</div>
            <div class="card-subtitle">Última sincronização: ${fmtDataHora(e.telemetria.ultimo_sync)}</div>
          </div>
          <button class="btn btn-secondary btn-sm">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width:14px;height:14px;margin-right:4px"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>
            Abrir no JD Connect
          </button>
        </div>
        <div style="padding:24px;text-align:center;color:var(--text-tertiary);border-top:1px solid var(--border-primary)">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" style="margin-bottom:12px;opacity:.5"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M3 12h18M12 3v18"/></svg>
          <div>Mapas de operação, curvas de consumo por talhão e log de códigos de falha ficam no portal JD.</div>
          <div style="font-size:12px;margin-top:8px">CRM mostra só as métricas agregadas.</div>
        </div>
      </div>
    </div>

    <!-- Comercial -->
    <div class="tab-content" data-tab-content="comercial">
      <div class="ficha-grid">
        <div class="ficha-col-main">
          ${renderFichaBloco('aquisicao', 'Aquisição', `
            <div class="ficha-fields">
              <div class="ff"><label>Tipo de operação</label><span>${e.aquisicao.tipo}</span></div>
              <div class="ff"><label>Concessionária</label><span>${e.aquisicao.concessionaria}</span></div>
              <div class="ff"><label>Data da venda</label><span>${new Date(e.aquisicao.data_venda).toLocaleDateString('pt-BR')}</span></div>
              <div class="ff"><label>Data de entrega</label><span>${new Date(e.aquisicao.data_entrega).toLocaleDateString('pt-BR')}</span></div>
              <div class="ff"><label>Vendedor (CEN)</label><span>${e.aquisicao.vendedor}</span></div>
              <div class="ff"><label>Cliente atual</label><span>${e.cliente.razao}</span></div>
              <div class="ff"><label>Pedido Protheus</label><span class="mono">${e.aquisicao.pedido_protheus}</span></div>
              <div class="ff"><label>Nota fiscal</label><span class="mono">${e.aquisicao.nota_fiscal}</span></div>
            </div>
          `)}
        </div>
        <div class="ficha-col-side">
          ${renderFichaBloco('valores', 'Valores da venda', `
            <div style="display:flex;flex-direction:column;gap:12px">
              <div class="valor-row">
                <span>Valor de lista</span>
                <strong>${fmtBRL(e.aquisicao.valor_lista)}</strong>
              </div>
              <div class="valor-row" style="color:#DC2626">
                <span>Desconto</span>
                <strong>−${fmtBRL(e.aquisicao.desconto)}</strong>
              </div>
              <div class="valor-row" style="padding-top:12px;border-top:1px dashed var(--border-primary);font-size:16px">
                <span><strong>Valor faturado</strong></span>
                <strong style="color:var(--jd-green-dark);font-family:'JetBrains Mono',monospace">${fmtBRL(e.aquisicao.valor_faturado)}</strong>
              </div>
              <div style="font-size:11px;color:var(--text-tertiary);margin-top:4px">
                Desconto de ${(e.aquisicao.desconto/e.aquisicao.valor_lista*100).toFixed(1)}%
              </div>
            </div>
          `)}
        </div>
      </div>
    </div>
  `;
}

function mountEquipamentoFicha() {
  document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
      const target = tab.dataset.tab;
      document.querySelectorAll('.tab').forEach(t => t.classList.toggle('active', t === tab));
      document.querySelectorAll('.tab-content').forEach(c => c.classList.toggle('active', c.dataset.tabContent === target));
    });
  });
}
// Rascunho de dataset da oportunidade — depois é colado no app.js

const OPORTUNIDADE_DATA = {
  id: 1517613,
  numero: 'OP-2026-08471',
  titulo: 'Renovação Frota Cana · 2× Trator 8R 250',
  cliente: {
    id: 84391,
    razao: 'Agroindustrial Salvador Arena Ltda',
    cnpj: '12.345.678/0001-90',
    classe: 'A',
    cidade: 'Lucas do Rio Verde/MT',
  },
  cen: { nome: 'José Rufino', regional: 'MT Norte', unidade: 'Tracbel Lucas do Rio Verde' },
  linha: 'Tratores',
  origem: 'Cliente cadastrado SAP',
  criada_em: '2026-07-28',
  atualizada_em: '2026-08-22T10:15',
  previsao_fechamento: '2026-10-15',
  probabilidade: 60,
  fase_atual: 'negocio_fechado', // Projeto → Orçamento → Proposta → Negócio Fechado → Pedido Alocado → ...
  macro_status: 'aberto', // aberto | fechado_ganho | fechado_perdido | cancelado

  valor_total: 3_200_000,
  desconto_total: 285_000, // 8.9%
  valor_lista: 3_485_000,

  itens: [
    {
      id: 1,
      codigo: '8R250-2026',
      descricao: 'Trator John Deere 8R 250 · Cabine CommandView III · e23 · 4×4 MFWD',
      qtd: 2,
      valor_unitario_lista: 1_720_000,
      valor_unitario: 1_580_000,
      desconto_pct: 8.14,
      aprovacao_desconto: 'aprovado', // pendente | aprovado | rejeitado | dentro_alcada
      aprovador: 'Marcos Vinicius Alves · Gerente Regional MT',
      aprovado_em: '2026-08-18T14:30',
      alcada_limite_pct: 10.0,
      total: 3_160_000,
    },
    {
      id: 2,
      codigo: 'SVC-ENT-4H',
      descricao: 'Serviço · Entrega técnica + treinamento operador (4h por equipamento)',
      qtd: 2,
      valor_unitario_lista: 4_500,
      valor_unitario: 3_500,
      desconto_pct: 22.22,
      aprovacao_desconto: 'aprovado',
      aprovador: 'Marcos Vinicius Alves · Gerente Regional MT',
      aprovado_em: '2026-08-18T14:30',
      alcada_limite_pct: 15.0,
      total: 7_000,
    },
    {
      id: 3,
      codigo: 'PGD-EST-5A',
      descricao: 'Garantia estendida PowerGard · 5 anos ou 8.000h (por equipamento)',
      qtd: 2,
      valor_unitario_lista: 18_000,
      valor_unitario: 16_500,
      desconto_pct: 8.33,
      aprovacao_desconto: 'dentro_alcada',
      alcada_limite_pct: 10.0,
      total: 33_000,
    },
  ],

  timeline_fases: [
    { key: 'projeto',      label: 'Projeto',              inicio: '2026-07-28', fim: '2026-07-30', dias: 2, status: 'concluida' },
    { key: 'orcamento',    label: 'Orçamento',            inicio: '2026-07-30', fim: '2026-08-05', dias: 6, status: 'concluida' },
    { key: 'proposta',     label: 'Proposta',             inicio: '2026-08-05', fim: '2026-08-18', dias: 13, status: 'concluida' },
    { key: 'negocio_fechado', label: 'Negócio Fechado',   inicio: '2026-08-18', fim: null, dias: 7, status: 'atual', bloqueio: 'Aguardando aprovação de valor de proposta' },
    { key: 'pedido_alocado',  label: 'Pedido Alocado',    inicio: null, fim: null, status: 'futura', aprovacao: 'Aprovação Comercial/Pedido' },
    { key: 'processo_liberado', label: 'Processo Liberado', inicio: null, fim: null, status: 'futura', aprovacao: 'Aprovação Doc. Faturamento' },
    { key: 'faturamento_liberado', label: 'Faturamento Liberado', inicio: null, fim: null, status: 'futura', aprovacao: 'Aprovação Financeiro' },
    { key: 'faturado',     label: 'Faturado',             inicio: null, fim: null, status: 'futura' },
  ],

  aprovacoes: [
    {
      id: 'AP-2026-04521',
      tipo: 'Aprovação de Desconto (item)',
      escopo: 'Item de linha #1 · Trator 8R 250 · Desconto 8.14%',
      solicitante: 'José Rufino',
      solicitado_em: '2026-08-15T09:22',
      alcada_regra: 'Descontos ≤ 5% aprovação automática · 5% a 10% Gerente Regional · > 10% Diretor Comercial',
      aprovador_atual: 'Marcos Vinicius Alves · Gerente Regional MT',
      status: 'aprovada',
      decidido_em: '2026-08-18T14:30',
      comentario: 'Aprovado. Cliente Classe A, histórico zero atraso, volume 2 unidades justifica.',
    },
    {
      id: 'AP-2026-04522',
      tipo: 'Aprovação de Condição de Pagamento',
      escopo: 'Proposta · Financiamento BNDES Finame 7 anos + entrada 20%',
      solicitante: 'José Rufino',
      solicitado_em: '2026-08-16T11:05',
      alcada_regra: 'Prazo > 60 meses ou entrada < 25%: Diretor Financeiro',
      aprovador_atual: 'Carla Menezes · Diretora Financeira',
      status: 'aprovada',
      decidido_em: '2026-08-19T10:12',
      comentario: 'Aprovado com condicionante: cliente deve manter limite disponível ≥ R$ 1.5M até assinatura.',
    },
    {
      id: 'AP-2026-04523',
      tipo: 'Aprovação de Valor de Proposta',
      escopo: 'Proposta consolidada · R$ 3.200.000 · Desconto total 8.2%',
      solicitante: 'José Rufino',
      solicitado_em: '2026-08-22T08:40',
      alcada_regra: 'Propostas ≥ R$ 3M: Diretor Comercial',
      aprovador_atual: 'Ricardo Meneghini · Diretor Comercial',
      status: 'pendente',
      sla_horas: 48,
      sla_decorridas: 55,
      comentario: null,
    },
    {
      id: 'AP-2026-04524',
      tipo: 'Aprovação Comercial/Pedido',
      escopo: 'Emissão do pedido após negócio fechado',
      status: 'nao_iniciada',
      alcada_regra: 'Gerente Regional após anexo de proposta assinada + calculadora',
      previsto_em_fase: 'pedido_alocado',
    },
    {
      id: 'AP-2026-04525',
      tipo: 'Aprovação Doc. Faturamento',
      escopo: 'Validação de AF, comprovante de depósito e contrato bancário',
      status: 'nao_iniciada',
      alcada_regra: 'Coordenador de Faturamento',
      previsto_em_fase: 'processo_liberado',
    },
    {
      id: 'AP-2026-04526',
      tipo: 'Aprovação Financeiro',
      escopo: 'Liberação final para emissão de NF-e',
      status: 'nao_iniciada',
      alcada_regra: 'Diretor Financeiro',
      previsto_em_fase: 'faturamento_liberado',
    },
  ],

  documentacao: {
    proposta: [
      { nome: 'Proposta comercial assinada', obrigatorio: true, status: 'aguardando_cliente', quem: 'José Rufino', envio_em: '2026-08-22', prazo: '2026-08-30' },
      { nome: 'Ordem de Compra do cliente', obrigatorio: false, status: 'nao_aplicavel' },
      { nome: 'Calculadora de precificação completa', obrigatorio: true, status: 'anexado', anexo: 'CALC-2026-08471.xlsx', anexado_em: '2026-08-18', anexado_por: 'José Rufino' },
      { nome: 'SPE (Solicitação de Precificação Especial)', obrigatorio: false, status: 'nao_aplicavel' },
      { nome: 'Ficha cadastral atualizada do cliente', obrigatorio: true, status: 'anexado', anexo: 'FC-84391-2026.pdf', anexado_em: '2026-08-05', anexado_por: 'Marina Costa · Admin' },
    ],
    faturamento: [
      { nome: 'AF (Autorização de Faturamento)', obrigatorio: true, status: 'nao_iniciado' },
      { nome: 'Comprovante de depósito em conta', obrigatorio: true, status: 'nao_iniciado' },
      { nome: 'Contrato assinado do banco (BNDES/CDCI)', obrigatorio: true, status: 'nao_iniciado' },
      { nome: 'Cópia dos boletos (se CDCI)', obrigatorio: false, status: 'nao_aplicavel' },
    ],
  },

  historico: [
    { data: '2026-08-22T10:15', autor: 'José Rufino · CEN', tipo: 'aprovacao', acao: 'Enviou "Aprovação de Valor de Proposta" para Ricardo Meneghini', descricao: 'Proposta consolidada R$ 3.200.000 · desconto 8.2%.' },
    { data: '2026-08-22T08:40', autor: 'Sistema', tipo: 'fase', acao: 'Fase alterada: Proposta → Negócio Fechado', descricao: 'Cliente confirmou intenção via e-mail (Marina Costa). CEN acionou avanço manual.' },
    { data: '2026-08-19T10:12', autor: 'Carla Menezes · Diretora Financeira', tipo: 'aprovacao', acao: 'Aprovou "Aprovação de Condição de Pagamento"', descricao: 'Aprovado com condicionante de manter limite disponível.' },
    { data: '2026-08-18T14:30', autor: 'Marcos Vinicius Alves · Gerente Regional MT', tipo: 'aprovacao', acao: 'Aprovou "Aprovação de Desconto (item)"', descricao: 'Item #1 · 8R 250 · desconto 8.14% aprovado.' },
    { data: '2026-08-16T11:05', autor: 'José Rufino · CEN', tipo: 'aprovacao', acao: 'Enviou "Aprovação de Condição de Pagamento" para Carla Menezes', descricao: 'BNDES Finame 7 anos + entrada 20%.' },
    { data: '2026-08-15T09:22', autor: 'José Rufino · CEN', tipo: 'aprovacao', acao: 'Enviou "Aprovação de Desconto (item)" para Marcos Vinicius', descricao: 'Item de linha excedeu a alçada automática.' },
    { data: '2026-08-05T15:20', autor: 'José Rufino · CEN', tipo: 'fase', acao: 'Fase alterada: Orçamento → Proposta', descricao: 'Proposta comercial gerada no Protheus (PROP-2026-08471).' },
    { data: '2026-07-30T09:00', autor: 'José Rufino · CEN', tipo: 'fase', acao: 'Fase alterada: Projeto → Orçamento', descricao: 'Especificação técnica assinada pelo cliente.' },
    { data: '2026-07-28T14:00', autor: 'José Rufino · CEN', tipo: 'criacao', acao: 'Oportunidade criada', descricao: 'Origem: visita técnica presencial · Necessidade: renovação de 2× 7250R por 2× 8R 250.' },
  ],

  acessos: [
    { papel: 'CEN (dono)', pessoa: 'José Rufino', permissao: 'Editar até fase Negócio Fechado · Somente anexar após' },
    { papel: 'Admin Comercial', pessoa: 'Marina Costa', permissao: 'Editar documentação em qualquer fase' },
    { papel: 'Gerente Regional', pessoa: 'Marcos Vinicius Alves', permissao: 'Aprovar itens · Editar valores em exceção' },
    { papel: 'Diretor Comercial', pessoa: 'Ricardo Meneghini', permissao: 'Aprovar propostas ≥ R$ 3M · Vetar oportunidade' },
    { papel: 'Diretor Financeiro', pessoa: 'Carla Menezes', permissao: 'Aprovar condição de pagamento · Liberação final' },
  ],
};

// ===== FIM DATASET =====

// ===== FICHA DA OPORTUNIDADE =====

function iconOppSvg(name) {
  const paths = {
    check: '<polyline points="20 6 9 17 4 12"/>',
    clock: '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>',
    x: '<line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>',
    file: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/>',
    upload: '<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/>',
    shield: '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>',
    alert: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>',
    money: '<line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>',
    plus: '<line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>',
    edit: '<path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>',
    tag: '<path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/><line x1="7" y1="7" x2="7.01" y2="7"/>',
    users: '<path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>',
    truck: '<rect x="1" y="3" width="15" height="13"/><polygon points="16 8 20 8 23 11 23 16 16 16 16 8"/><circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/>',
    play: '<polygon points="5 3 19 12 5 21 5 3"/>',
  };
  return `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${paths[name] || ''}</svg>`;
}

function macroStatusBadge(status) {
  const map = {
    aberto: { label: 'Em andamento · Aberto', cls: '' },
    fechado_ganho: { label: 'Fechado · Ganho', cls: 'ganho' },
    fechado_perdido: { label: 'Fechado · Perdido', cls: 'perdido' },
    cancelado: { label: 'Cancelado', cls: 'cancelado' },
  };
  const m = map[status] || map.aberto;
  return `<span class="opp-status-badge ${m.cls}">${m.label}</span>`;
}

function renderOportunidadeTimeline(fases) {
  const faseAprovMap = {
    proposta: { tipo: 'Aprovação de Valor', key: 'valor_proposta' },
    negocio_fechado: { tipo: 'Aprovação de Valor', key: 'valor_proposta' },
    pedido_alocado: { tipo: 'Aprov. Comercial', key: 'comercial_pedido' },
    processo_liberado: { tipo: 'Aprov. Doc. Fatur.', key: 'doc_faturamento' },
    faturamento_liberado: { tipo: 'Aprov. Financeiro', key: 'financeiro' },
  };
  const aprovStatusByKey = {
    valor_proposta: OPORTUNIDADE_DATA.aprovacoes.find(a => a.tipo.includes('Valor de Proposta'))?.status || 'nao_iniciada',
    comercial_pedido: OPORTUNIDADE_DATA.aprovacoes.find(a => a.tipo.includes('Comercial/Pedido'))?.status || 'nao_iniciada',
    doc_faturamento: OPORTUNIDADE_DATA.aprovacoes.find(a => a.tipo.includes('Doc. Faturamento'))?.status || 'nao_iniciada',
    financeiro: OPORTUNIDADE_DATA.aprovacoes.find(a => a.tipo === 'Aprovação Financeiro')?.status || 'nao_iniciada',
  };

  return `
    <div class="opp-timeline">
      <div class="opp-timeline-header">
        <div class="opp-timeline-title">Timeline do processo comercial · 8 fases</div>
        <div class="opp-timeline-legend">
          <span><span class="opp-legend-dot concluida"></span>Concluída</span>
          <span><span class="opp-legend-dot atual"></span>Atual</span>
          <span><span class="opp-legend-dot futura"></span>Futura</span>
          <span><span class="opp-legend-dot bloqueada"></span>Aprovação pendente</span>
        </div>
      </div>
      <div class="opp-fases">
        ${fases.map((f, i) => {
          const nodeContent = f.status === 'concluida'
            ? iconOppSvg('check')
            : (f.status === 'atual' ? String(i+1) : String(i+1));
          const aprovInfo = faseAprovMap[f.key];
          let marker = '';
          if (aprovInfo) {
            const st = aprovStatusByKey[aprovInfo.key];
            marker = `<span class="opp-fase-aprov-marker ${st}">${iconOppSvg('shield')}${aprovInfo.tipo}</span>`;
          }
          let meta = '';
          if (f.inicio && f.fim) {
            meta = `<div class="opp-fase-meta">${f.dias}d</div>`;
          } else if (f.inicio && f.status === 'atual') {
            meta = `<div class="opp-fase-meta">há ${f.dias}d</div>`;
          }
          return `
            <div class="opp-fase ${f.status}">
              <div class="opp-fase-connector"></div>
              <div class="opp-fase-node">${nodeContent}</div>
              <div class="opp-fase-label">${f.label}</div>
              ${meta}
              ${marker}
            </div>
          `;
        }).join('')}
      </div>
    </div>
  `;
}

function renderOportunidadeVisao(o) {
  const proximaAprov = o.aprovacoes.find(a => a.status === 'pendente');
  const nextAction = proximaAprov
    ? `Aprovação pendente: <strong>${proximaAprov.tipo}</strong> com ${proximaAprov.aprovador_atual}. SLA ${proximaAprov.sla_decorridas}h de ${proximaAprov.sla_horas}h — <strong>vencido há ${proximaAprov.sla_decorridas - proximaAprov.sla_horas}h</strong>. Trava avanço para Pedido Alocado.`
    : 'Aguardando ação do CEN';

  return `
    <div class="opp-next-action" style="margin-bottom:20px">
      <div class="opp-next-action-icon">${iconOppSvg('alert')}</div>
      <div class="opp-next-action-body">
        <div class="opp-next-action-title">Próxima ação</div>
        <div class="opp-next-action-text">${nextAction}</div>
        <div class="opp-next-action-hint">Contato: Ricardo Meneghini · Diretor Comercial · Ramal 5012</div>
      </div>
      <button class="opp-btn primary" style="background:#3B82F6;color:#fff;border-color:#2563EB">Cobrar aprovação</button>
    </div>

    <div class="opp-tab-body">
      <div>
        ${renderFichaBloco('opp-dados', 'IDENTIFICAÇÃO DA OPORTUNIDADE', `
          <div class="ficha-fields">
            <div class="ff"><label>Número</label><span class="mono">${o.numero}</span></div>
            <div class="ff"><label>ID interno</label><span class="mono">#${o.id}</span></div>
            <div class="ff"><label>Título</label><span>${o.titulo}</span></div>
            <div class="ff"><label>Linha de produto</label><span>${o.linha}</span></div>
            <div class="ff"><label>Origem</label><span>${o.origem}</span></div>
            <div class="ff"><label>Criada em</label><span>${o.criada_em}</span></div>
            <div class="ff"><label>Última atualização</label><span>${fmtDataHora(o.atualizada_em)}</span></div>
            <div class="ff"><label>Previsão fechamento</label><span>${o.previsao_fechamento}</span></div>
          </div>
        `)}

        ${renderFichaBloco('opp-cliente', 'CLIENTE', `
          <div class="ficha-fields">
            <div class="ff"><label>Razão social</label><span><a href="#/clientes/${o.cliente.id}" style="color:#367C2B;font-weight:600">${o.cliente.razao}</a></span></div>
            <div class="ff"><label>CNPJ</label><span class="mono">${o.cliente.cnpj}</span></div>
            <div class="ff"><label>Classe</label><span><span class="badge badge-success">Classe ${o.cliente.classe}</span></span></div>
            <div class="ff"><label>Cidade</label><span>${o.cliente.cidade}</span></div>
          </div>
        `)}

        ${renderFichaBloco('opp-financ', 'RESUMO FINANCEIRO', `
          <div class="ficha-fields">
            <div class="ff"><label>Valor de lista</label><span class="mono">${fmtBRL(o.valor_lista)}</span></div>
            <div class="ff"><label>Desconto total</label><span class="mono" style="color:#DC2626">−${fmtBRL(o.desconto_total)} (${((o.desconto_total/o.valor_lista)*100).toFixed(1)}%)</span></div>
            <div class="ff"><label>Valor faturável</label><span class="mono" style="color:#367C2B;font-weight:700;font-size:16px">${fmtBRL(o.valor_total)}</span></div>
            <div class="ff"><label>Condição de pagamento</label><span>BNDES Finame 7 anos · entrada 20%</span></div>
            <div class="ff"><label>Probabilidade</label><span>${o.probabilidade}%</span></div>
          </div>
        `)}
      </div>

      <div>
        <div class="opp-side-card">
          <div class="opp-side-title">CEN Responsável</div>
          <div style="display:flex;gap:10px;align-items:center;margin-bottom:10px">
            <div style="width:40px;height:40px;border-radius:50%;background:#367C2B;color:#fff;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:14px">JR</div>
            <div>
              <div style="font-size:14px;font-weight:600;color:var(--text-primary)">${o.cen.nome}</div>
              <div style="font-size:11.5px;color:var(--text-secondary)">${o.cen.regional} · ${o.cen.unidade}</div>
            </div>
          </div>
        </div>

        <div class="opp-side-card">
          <div class="opp-side-title">Controle de acesso</div>
          ${o.acessos.map(a => `
            <div class="opp-acesso-item">
              <div class="opp-acesso-papel">${a.papel}</div>
              <div class="opp-acesso-pessoa">${a.pessoa}</div>
              <div class="opp-acesso-perm">${a.permissao}</div>
            </div>
          `).join('')}
        </div>

        <div class="opp-side-card">
          <div class="opp-side-title">Auditoria</div>
          <div class="opp-side-row"><dt>Criada por</dt><dd>José Rufino</dd></div>
          <div class="opp-side-row"><dt>Pedido Protheus</dt><dd class="mono">PROP-2026-08471</dd></div>
          <div class="opp-side-row"><dt>Sync Protheus</dt><dd>—</dd></div>
          <div class="opp-side-row"><dt>Origem inclusão</dt><dd>Manual · CEN</dd></div>
        </div>
      </div>
    </div>
  `;
}

function renderOportunidadeItens(o) {
  return `
    <div class="card" style="margin-bottom:16px">
      <div class="card-header">
        <div>
          <div class="card-title">Itens da proposta · ${o.itens.length} itens</div>
          <div class="card-subtitle">Descontos por item passam por aprovação automática quando excedem alçada. Aprovações refletem no total.</div>
        </div>
        <div style="display:flex;gap:8px">
          <button class="opp-btn" style="background:#fff;color:var(--text-primary);border-color:var(--border-primary)">${iconOppSvg('upload')} Importar do Protheus</button>
          <button class="opp-btn" style="background:#367C2B;color:#fff;border-color:#1B5E20">${iconOppSvg('plus')} Adicionar item</button>
        </div>
      </div>
      <div class="card-body" style="padding:0">
        <table class="opp-itens-tabela">
          <thead>
            <tr>
              <th style="width:110px">Código</th>
              <th>Descrição</th>
              <th style="text-align:center;width:60px">Qtd</th>
              <th style="text-align:right;width:130px">Valor lista</th>
              <th style="text-align:right;width:130px">Valor unitário</th>
              <th style="text-align:center;width:110px">Desconto</th>
              <th style="text-align:right;width:130px">Total</th>
            </tr>
          </thead>
          <tbody>
            ${o.itens.map(it => `
              <tr>
                <td><span class="opp-item-codigo">${it.codigo}</span></td>
                <td>
                  <div class="opp-item-desc">${it.descricao}</div>
                  ${it.aprovacao_desconto === 'aprovado' ? `<div class="opp-item-aprov-detalhe">Aprovado por ${it.aprovador} em ${fmtDataHora(it.aprovado_em)}</div>` : ''}
                  ${it.aprovacao_desconto === 'dentro_alcada' ? `<div class="opp-item-aprov-detalhe">Dentro da alçada automática (≤ ${it.alcada_limite_pct}%)</div>` : ''}
                </td>
                <td style="text-align:center">${it.qtd}</td>
                <td class="opp-item-num dim">${fmtBRL(it.valor_unitario_lista)}</td>
                <td class="opp-item-num strong">${fmtBRL(it.valor_unitario)}</td>
                <td style="text-align:center">
                  <span class="opp-desc-pill ${it.aprovacao_desconto}">${it.desconto_pct.toFixed(2)}%</span>
                </td>
                <td class="opp-item-num strong">${fmtBRL(it.total)}</td>
              </tr>
            `).join('')}
            <tr class="opp-itens-total">
              <td colspan="3"></td>
              <td class="opp-item-num dim" style="font-size:13px">${fmtBRL(o.valor_lista)}</td>
              <td colspan="2" style="text-align:right;font-size:12px;color:var(--text-secondary)">Desconto total <strong style="color:#DC2626;font-family:'JetBrains Mono',monospace">−${fmtBRL(o.desconto_total)}</strong></td>
              <td class="opp-item-num strong" style="font-size:16px;color:#367C2B">${fmtBRL(o.valor_total)}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="opp-next-action" style="background:#FFFBEB;border-color:#FCD34D">
      <div class="opp-next-action-icon" style="background:#F59E0B">${iconOppSvg('alert')}</div>
      <div class="opp-next-action-body">
        <div class="opp-next-action-title" style="color:#B45309">Regras de alçada aplicadas</div>
        <div class="opp-next-action-text" style="color:#78350F">Descontos ≤ 5% aprovação automática · 5% a 10% Gerente Regional · &gt; 10% Diretor Comercial · &gt; 15% Diretor + Comitê.</div>
      </div>
    </div>
  `;
}

function renderOportunidadeAprovacoes(o) {
  const iconMap = { pendente: 'clock', aprovada: 'check', rejeitada: 'x', nao_iniciada: 'clock' };
  const labelMap = { pendente: 'Pendente', aprovada: 'Aprovada', rejeitada: 'Rejeitada', nao_iniciada: 'Não iniciada' };

  return `
    <div style="margin-bottom:16px;padding:14px 16px;background:var(--n-50);border-radius:8px;font-size:12.5px;color:var(--text-secondary);line-height:1.5">
      <strong style="color:var(--text-primary)">3 canais de aprovação nesta oportunidade:</strong>
      (a) <strong>Item de linha</strong> — automática quando desconto excede alçada;
      (b) <strong>Botão "Enviar para aprovação" na Proposta</strong> — Condição de Pagamento e Valor de Proposta;
      (c) <strong>Botão "Enviar para aprovação" na Oportunidade</strong> — Comercial/Pedido, Doc. Faturamento e Financeiro.
    </div>

    ${o.aprovacoes.map(a => `
      <div class="opp-aprov-card ${a.status}">
        <div class="opp-aprov-icon ${a.status}">${iconOppSvg(iconMap[a.status])}</div>
        <div class="opp-aprov-body">
          <div class="opp-aprov-title-row">
            <div>
              <div class="opp-aprov-title">${a.tipo}</div>
              <div class="opp-aprov-escopo">${a.escopo}</div>
            </div>
            <div style="display:flex;flex-direction:column;gap:4px;align-items:flex-end">
              <span class="opp-aprov-id">${a.id}</span>
              ${a.status === 'pendente' && a.sla_decorridas > a.sla_horas
                ? `<span class="opp-sla-badge late">SLA +${a.sla_decorridas - a.sla_horas}h</span>`
                : (a.status === 'pendente' ? `<span class="opp-sla-badge warn">SLA ${a.sla_decorridas}/${a.sla_horas}h</span>` : '')}
              ${a.status === 'aprovada' ? `<span class="badge badge-success">${labelMap[a.status]}</span>` : ''}
              ${a.status === 'nao_iniciada' ? `<span class="badge badge-neutral">${labelMap[a.status]}</span>` : ''}
            </div>
          </div>
          ${a.solicitante ? `
            <div class="opp-aprov-grid">
              <div><dt>Solicitante</dt><dd>${a.solicitante}</dd></div>
              <div><dt>Solicitado em</dt><dd>${fmtDataHora(a.solicitado_em)}</dd></div>
              <div><dt>Aprovador</dt><dd>${a.aprovador_atual || '—'}</dd></div>
              <div><dt>Decidido em</dt><dd>${a.decidido_em ? fmtDataHora(a.decidido_em) : '—'}</dd></div>
            </div>
          ` : ''}
          ${a.previsto_em_fase ? `
            <div class="opp-aprov-grid">
              <div><dt>Aprovador previsto</dt><dd>${a.alcada_regra}</dd></div>
              <div><dt>Fase esperada</dt><dd>${a.previsto_em_fase.replace('_',' ')}</dd></div>
            </div>
          ` : ''}
          <div class="opp-aprov-regra">${a.alcada_regra}</div>
          ${a.comentario ? `<div class="opp-aprov-coment">${iconOppSvg('check')} <strong>Comentário do aprovador:</strong> ${a.comentario}</div>` : ''}
          ${a.status === 'pendente' ? `
            <div class="opp-aprov-actions">
              <button class="approve">${iconOppSvg('check')} Aprovar</button>
              <button class="reject">${iconOppSvg('x')} Rejeitar</button>
              <button class="comment">${iconOppSvg('edit')} Comentar</button>
              <button class="comment" style="margin-left:auto">Escalar para Diretor</button>
            </div>
          ` : ''}
        </div>
      </div>
    `).join('')}
  `;
}

function renderOportunidadeDocumentacao(o) {
  const iconMap = { anexado: 'check', aguardando_cliente: 'clock', nao_iniciado: 'file', nao_aplicavel: 'file' };
  const renderDoc = (d) => `
    <div class="opp-doc-item ${d.status}">
      <div class="opp-doc-icon">${iconOppSvg(iconMap[d.status])}</div>
      <div class="opp-doc-body">
        <div class="opp-doc-nome">${d.nome}${d.obrigatorio ? '<span class="req">*</span>' : ''}</div>
        <div class="opp-doc-meta">
          ${d.status === 'anexado' ? `${d.anexo} · anexado por ${d.anexado_por} em ${d.anexado_em}` : ''}
          ${d.status === 'aguardando_cliente' ? `Enviado ao cliente em ${d.envio_em} · prazo ${d.prazo}` : ''}
          ${d.status === 'nao_iniciado' ? 'Ainda não anexado' : ''}
          ${d.status === 'nao_aplicavel' ? 'Não aplicável a esta oportunidade' : ''}
        </div>
      </div>
      <div class="opp-doc-actions">
        ${d.status === 'anexado' ? `<button>Baixar</button><button>Substituir</button>` : ''}
        ${d.status === 'aguardando_cliente' ? `<button>Reenviar</button><button class="primary">Anexar</button>` : ''}
        ${d.status === 'nao_iniciado' ? `<button class="primary">${iconOppSvg('upload')} Anexar</button>` : ''}
        ${d.status === 'nao_aplicavel' ? `<button>Marcar como aplicável</button>` : ''}
      </div>
    </div>
  `;

  return `
    <div style="margin-bottom:16px;padding:14px 16px;background:#EFF6FF;border:1px solid #BFDBFE;border-radius:8px;font-size:12.5px;color:#1E3A8A;line-height:1.5">
      <strong>Regra do processo:</strong> Documentação da <strong>Proposta</strong> obrigatória antes da Aprovação Comercial/Pedido (fase Pedido Alocado). Documentação de <strong>Faturamento</strong> obrigatória antes da Aprovação Doc. Faturamento (fase Processo Liberado).
    </div>

    <div class="opp-doc-group-title">Documentação da Proposta · antes de Pedido Alocado</div>
    ${o.documentacao.proposta.map(renderDoc).join('')}

    <div class="opp-doc-group-title">Documentação de Faturamento · antes de Faturamento Liberado</div>
    ${o.documentacao.faturamento.map(renderDoc).join('')}

    <div class="opp-doc-group-title" style="border:none;margin-top:20px;color:var(--text-tertiary);font-weight:500;text-transform:none;letter-spacing:0;font-size:11.5px">
      <span class="req" style="color:#DC2626;font-weight:700">*</span> Documento obrigatório para avançar a fase.
    </div>
  `;
}

function renderOportunidadeHistorico(o) {
  const iconMap = { aprovacao: 'shield', fase: 'play', criacao: 'plus', doc: 'file' };
  return `
    <div class="card">
      <div class="card-header">
        <div>
          <div class="card-title">Histórico da oportunidade · ${o.historico.length} eventos</div>
          <div class="card-subtitle">Log auditável de mudanças de fase, aprovações, anexos e comentários. Ordenado do mais recente ao mais antigo.</div>
        </div>
        <button class="opp-btn" style="background:#fff;color:var(--text-primary);border-color:var(--border-primary)">Exportar CSV</button>
      </div>
      <div class="card-body">
        ${o.historico.map(h => `
          <div class="opp-hist-item">
            <div class="opp-hist-icon ${h.tipo}">${iconOppSvg(iconMap[h.tipo])}</div>
            <div class="opp-hist-body">
              <div class="opp-hist-header">
                <div class="opp-hist-acao">${h.acao}</div>
                <div class="opp-hist-data">${fmtDataHora(h.data)}</div>
              </div>
              <div class="opp-hist-autor">${h.autor}</div>
              <div class="opp-hist-desc">${h.descricao}</div>
            </div>
          </div>
        `).join('')}
      </div>
    </div>
  `;
}

function renderOportunidadeFicha() {
  const o = OPORTUNIDADE_DATA;
  const faseAtual = o.timeline_fases.find(f => f.status === 'atual');
  const pctConcluidas = (o.timeline_fases.filter(f => f.status === 'concluida').length / o.timeline_fases.length) * 100;

  return `
    <div class="opp-header">
      <div class="opp-header-top">
        <div style="flex:1;min-width:0">
          <div class="opp-eyebrow">
            <span class="opp-eyebrow-pill">${o.numero}</span>
            <span>#${o.id}</span>
            <span>·</span>
            <span>${o.linha}</span>
            <span>·</span>
            <span>CEN ${o.cen.nome}</span>
          </div>
          <div class="opp-title">${o.titulo}</div>
          <div class="opp-subline">
            <a href="#/clientes/${o.cliente.id}">${o.cliente.razao}</a>
            <span class="opp-subline-sep">·</span>
            <span>${o.cliente.cidade}</span>
            <span class="opp-subline-sep">·</span>
            <span>Classe ${o.cliente.classe}</span>
            <span class="opp-subline-sep">·</span>
            <span>Previsão fechamento: <strong>${o.previsao_fechamento}</strong></span>
          </div>
        </div>
        <div style="display:flex;flex-direction:column;gap:8px;align-items:flex-end">
          ${macroStatusBadge(o.macro_status)}
          <span style="font-size:11px;color:rgba(255,255,255,0.8);font-family:'JetBrains Mono',monospace">Fase: ${faseAtual.label}</span>
        </div>
      </div>
      <div class="opp-actions">
        <button class="opp-btn">${iconOppSvg('edit')} Editar</button>
        <button class="opp-btn">${iconOppSvg('file')} Gerar proposta PDF</button>
        <button class="opp-btn">${iconOppSvg('shield')} Enviar para aprovação</button>
        <button class="opp-btn danger">${iconOppSvg('x')} Marcar como perdida</button>
        <button class="opp-btn primary" style="margin-left:auto">${iconOppSvg('play')} Avançar para Pedido Alocado</button>
      </div>
    </div>

    ${renderOportunidadeTimeline(o.timeline_fases)}

    <div class="opp-kpi-grid">
      <div class="opp-kpi">
        <div class="opp-kpi-label">Valor faturável</div>
        <div class="opp-kpi-value" style="color:#367C2B">${fmtBRL(o.valor_total).replace('R$ ','R$ ')}</div>
        <div class="opp-kpi-hint">Lista R$ ${fmtNum(o.valor_lista)} · desc ${((o.desconto_total/o.valor_lista)*100).toFixed(1)}%</div>
      </div>
      <div class="opp-kpi">
        <div class="opp-kpi-label">Probabilidade</div>
        <div class="opp-kpi-value">${o.probabilidade}<span class="unit">%</span></div>
        <div class="opp-kpi-hint">Ponderado: ${fmtBRL(o.valor_total * o.probabilidade / 100)}</div>
      </div>
      <div class="opp-kpi warn">
        <div class="opp-kpi-label">SLA aprovação</div>
        <div class="opp-kpi-value">+7<span class="unit">h vencidas</span></div>
        <div class="opp-kpi-hint">Diretor Comercial há 55h · SLA 48h</div>
      </div>
      <div class="opp-kpi">
        <div class="opp-kpi-label">Dias para prev. fechamento</div>
        <div class="opp-kpi-value">51<span class="unit">d</span></div>
        <div class="opp-kpi-hint">15/out/2026 · Safra iniciando 20/set</div>
      </div>
      <div class="opp-next-action" style="margin:0">
        <div class="opp-next-action-icon">${iconOppSvg('alert')}</div>
        <div class="opp-next-action-body">
          <div class="opp-next-action-title">Bloqueio ativo</div>
          <div class="opp-next-action-text">${faseAtual.bloqueio || 'Sem bloqueios'}</div>
          <div class="opp-next-action-hint">Aprovação AP-2026-04523 pendente</div>
        </div>
      </div>
    </div>

    <div class="tabs">
      <button class="tab active" data-tab="visao">Visão geral</button>
      <button class="tab" data-tab="itens">Itens da proposta (${o.itens.length})</button>
      <button class="tab" data-tab="aprovacoes">Aprovações (${o.aprovacoes.length})</button>
      <button class="tab" data-tab="documentacao">Documentação</button>
      <button class="tab" data-tab="historico">Histórico (${o.historico.length})</button>
    </div>

    <div class="tab-content active" data-tab-content="visao">${renderOportunidadeVisao(o)}</div>
    <div class="tab-content" data-tab-content="itens">${renderOportunidadeItens(o)}</div>
    <div class="tab-content" data-tab-content="aprovacoes">${renderOportunidadeAprovacoes(o)}</div>
    <div class="tab-content" data-tab-content="documentacao">${renderOportunidadeDocumentacao(o)}</div>
    <div class="tab-content" data-tab-content="historico">${renderOportunidadeHistorico(o)}</div>
  `;
}

function mountOportunidadeFicha() {
  const tabs = document.querySelectorAll('.tabs .tab');
  const contents = document.querySelectorAll('.tab-content[data-tab-content]');
  tabs.forEach(tab => {
    tab.addEventListener('click', () => {
      const key = tab.getAttribute('data-tab');
      tabs.forEach(t => t.classList.toggle('active', t === tab));
      contents.forEach(c => c.classList.toggle('active', c.getAttribute('data-tab-content') === key));
    });
  });
}

// ===== COBERTURA DE CARTEIRA DO CEN =====

// Data "hoje" do protótipo: terça 25/ago/2026
const HOJE_CARTEIRA = new Date(2026, 7, 25);

// Meta de frequência por classe (dias entre interações válidas)
const META_FREQ = { A: 30, B: 60, C: 90, D: 120 };

// Categorias de interação
const CAT_INTERACAO = {
  visita: { label: 'Visita presencial', cor: '#367C2B', icone: 'M12 21s-8-4.5-8-11a8 8 0 0 1 16 0c0 6.5-8 11-8 11z M12 10a2 2 0 1 0 0-4 2 2 0 0 0 0 4z' },
  ligacao: { label: 'Ligação', cor: '#2563EB', icone: 'M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z' },
  whatsapp: { label: 'WhatsApp', cor: '#25D366', icone: 'M12 2a10 10 0 0 0-8.94 14.42L2 22l5.7-1.5A10 10 0 1 0 12 2z' },
  email: { label: 'E-mail', cor: '#7C3AED', icone: 'M4 4h16v16H4z M4 4l8 7 8-7' },
  remota: { label: 'Reunião remota', cor: '#0891B2', icone: 'M23 7l-7 5 7 5V7z M1 5h15v14H1z' }
};

// José Rufino · CEN MT Norte · atende Lucas do Rio Verde, Sorriso, Sinop, Nova Mutum, Sapezal, Campo Novo dos Parecis
// 24 clientes distribuídos por status realista
const CARTEIRA_CEN = [
  // === CRÍTICO (>150% da meta) ===
  { id: 84391, razao: 'Agroindustrial Salvador Arena Ltda', apelido: 'Salvador Arena', classe: 'A', cidade: 'Lucas do Rio Verde', uf: 'MT', lat: -13.05, lng: -55.91, exige_visita: true, ult_int: { data: '2026-08-22', cat: 'ligacao', autor: 'José Rufino' }, fat_12m: 4820000, oportunidades: 1, oportunidades_valor: 3200000, obs: 'Oportunidade #1517613 em Neg. Fechado · limite pendente' },
  { id: 84402, razao: 'Fazenda Boa Vista Agropecuária S/A', apelido: 'Boa Vista', classe: 'A', cidade: 'Sorriso', uf: 'MT', lat: -12.54, lng: -55.72, exige_visita: true, ult_int: { data: '2026-05-14', cat: 'visita', autor: 'José Rufino' }, fat_12m: 2680000, oportunidades: 0, obs: 'Sem interação há 103 dias · meta 30d (Classe A visita obrigatória)' },
  { id: 84418, razao: 'Grupo SLC Bocaiúva Fazenda Parceiro', apelido: 'SLC Bocaiúva', classe: 'A', cidade: 'Sapezal', uf: 'MT', lat: -13.54, lng: -58.81, exige_visita: true, ult_int: { data: '2026-05-30', cat: 'visita', autor: 'José Rufino' }, fat_12m: 6120000, oportunidades: 2, oportunidades_valor: 1850000, obs: 'Frota mista JD/Case · atenção ao competidor' },
  { id: 84425, razao: 'Agropecuária Três Marias Ltda', apelido: 'Três Marias', classe: 'B', cidade: 'Nova Mutum', uf: 'MT', lat: -13.83, lng: -56.08, exige_visita: false, ult_int: { data: '2026-04-18', cat: 'ligacao', autor: 'José Rufino' }, fat_12m: 890000, oportunidades: 0, obs: 'Aceita atendimento remoto · 129 dias sem contato' },

  // === ATRASO (100-150% da meta) ===
  { id: 84430, razao: 'Fazenda Santa Clara Grãos Ltda', apelido: 'Santa Clara', classe: 'A', cidade: 'Sinop', uf: 'MT', lat: -11.86, lng: -55.51, exige_visita: true, ult_int: { data: '2026-07-14', cat: 'visita', autor: 'José Rufino' }, fat_12m: 1980000, oportunidades: 1, oportunidades_valor: 720000, obs: 'Revisão 500h pendente na frota' },
  { id: 84437, razao: 'Grupo Bom Futuro Sinop', apelido: 'Bom Futuro', classe: 'A', cidade: 'Sinop', uf: 'MT', lat: -11.87, lng: -55.50, exige_visita: true, ult_int: { data: '2026-07-19', cat: 'visita', autor: 'José Rufino' }, fat_12m: 3420000, oportunidades: 1, oportunidades_valor: 2100000, obs: 'Cliente estratégico · 37 dias sem visita' },
  { id: 84445, razao: 'Agropecuária Vale Verde Ltda', apelido: 'Vale Verde', classe: 'B', cidade: 'Campo Novo dos Parecis', uf: 'MT', lat: -13.67, lng: -57.89, exige_visita: false, ult_int: { data: '2026-06-30', cat: 'whatsapp', autor: 'José Rufino' }, fat_12m: 640000, oportunidades: 0, obs: 'Prefere WhatsApp · próxima renovação em Q1/27' },
  { id: 84448, razao: 'Fazenda Nova Esperança S/A', apelido: 'Nova Esperança', classe: 'B', cidade: 'Lucas do Rio Verde', uf: 'MT', lat: -13.06, lng: -55.89, exige_visita: false, ult_int: { data: '2026-06-22', cat: 'ligacao', autor: 'Marina Costa · Admin' }, fat_12m: 512000, oportunidades: 0, obs: 'Último contato foi Admin · CEN não visitou há 89 dias' },

  // === AVISO (60-100% da meta) ===
  { id: 84452, razao: 'Sementes do Cerrado Ltda', apelido: 'Sementes do Cerrado', classe: 'A', cidade: 'Sorriso', uf: 'MT', lat: -12.55, lng: -55.71, exige_visita: true, ult_int: { data: '2026-08-02', cat: 'visita', autor: 'José Rufino' }, fat_12m: 1420000, oportunidades: 0, obs: '23 dias desde última visita' },
  { id: 84458, razao: 'Agroindustrial Bragagnolo Ltda', apelido: 'Bragagnolo', classe: 'A', cidade: 'Lucas do Rio Verde', uf: 'MT', lat: -13.04, lng: -55.92, exige_visita: true, ult_int: { data: '2026-08-05', cat: 'visita', autor: 'José Rufino' }, fat_12m: 2140000, oportunidades: 2, oportunidades_valor: 1450000, obs: '20 dias · dentro do prazo mas monitorar' },
  { id: 84461, razao: 'Grupo Amaggi Fazenda Tanguro', apelido: 'Amaggi Tanguro', classe: 'A', cidade: 'Sapezal', uf: 'MT', lat: -13.55, lng: -58.82, exige_visita: true, ult_int: { data: '2026-08-08', cat: 'visita', autor: 'José Rufino' }, fat_12m: 5820000, oportunidades: 1, oportunidades_valor: 4200000, obs: 'Cliente TOP 3 da região' },
  { id: 84467, razao: 'Fazenda Rio Verde Grãos Ltda', apelido: 'Rio Verde Grãos', classe: 'B', cidade: 'Lucas do Rio Verde', uf: 'MT', lat: -13.07, lng: -55.90, exige_visita: false, ult_int: { data: '2026-07-08', cat: 'ligacao', autor: 'José Rufino' }, fat_12m: 780000, oportunidades: 0, obs: '48 dias · dentro da meta B (60d)' },
  { id: 84472, razao: 'Agropecuária Bela Vista Ltda', apelido: 'Bela Vista', classe: 'C', cidade: 'Sinop', uf: 'MT', lat: -11.85, lng: -55.52, exige_visita: false, ult_int: { data: '2026-06-17', cat: 'email', autor: 'José Rufino' }, fat_12m: 240000, oportunidades: 0, obs: '69 dias · meta C 90d' },

  // === EM DIA (≤60% da meta) ===
  { id: 84478, razao: 'Fazenda Passo Fundo Sementes', apelido: 'Passo Fundo', classe: 'A', cidade: 'Nova Mutum', uf: 'MT', lat: -13.82, lng: -56.09, exige_visita: true, ult_int: { data: '2026-08-18', cat: 'visita', autor: 'José Rufino' }, fat_12m: 1680000, oportunidades: 1, oportunidades_valor: 890000, obs: '7 dias · visita recente' },
  { id: 84483, razao: 'Agroindustrial Cachoeira Ltda', apelido: 'Cachoeira', classe: 'A', cidade: 'Sorriso', uf: 'MT', lat: -12.56, lng: -55.70, exige_visita: true, ult_int: { data: '2026-08-15', cat: 'visita', autor: 'José Rufino' }, fat_12m: 1890000, oportunidades: 0, obs: '10 dias · em dia' },
  { id: 84489, razao: 'Grupo SLC Agrícola Sinop', apelido: 'SLC Sinop', classe: 'A', cidade: 'Sinop', uf: 'MT', lat: -11.88, lng: -55.49, exige_visita: true, ult_int: { data: '2026-08-20', cat: 'visita', autor: 'José Rufino' }, fat_12m: 4210000, oportunidades: 3, oportunidades_valor: 2650000, obs: 'Conta estratégica · 3 oportunidades ativas' },
  { id: 84495, razao: 'Fazenda Aliança Grãos Ltda', apelido: 'Aliança', classe: 'B', cidade: 'Lucas do Rio Verde', uf: 'MT', lat: -13.03, lng: -55.93, exige_visita: false, ult_int: { data: '2026-08-11', cat: 'whatsapp', autor: 'José Rufino' }, fat_12m: 520000, oportunidades: 0, obs: '14 dias' },
  { id: 84501, razao: 'Agropecuária São Lucas Ltda', apelido: 'São Lucas', classe: 'B', cidade: 'Sorriso', uf: 'MT', lat: -12.53, lng: -55.73, exige_visita: false, ult_int: { data: '2026-08-13', cat: 'remota', autor: 'José Rufino' }, fat_12m: 610000, oportunidades: 0, obs: 'Reunião mensal via Teams' },
  { id: 84508, razao: 'Fazenda Cerrado Verde Ltda', apelido: 'Cerrado Verde', classe: 'C', cidade: 'Campo Novo dos Parecis', uf: 'MT', lat: -13.68, lng: -57.90, exige_visita: false, ult_int: { data: '2026-08-04', cat: 'ligacao', autor: 'José Rufino' }, fat_12m: 190000, oportunidades: 0, obs: '21 dias · meta C 90d' },
  { id: 84512, razao: 'Sitio Boa Sorte Ltda ME', apelido: 'Boa Sorte', classe: 'C', cidade: 'Nova Mutum', uf: 'MT', lat: -13.84, lng: -56.07, exige_visita: false, ult_int: { data: '2026-07-28', cat: 'whatsapp', autor: 'José Rufino' }, fat_12m: 128000, oportunidades: 0, obs: '28 dias' },

  // === NUNCA VISITADO (prospects) ===
  { id: 84518, razao: 'Grupo Wehrmann Fazenda Nova Divisa', apelido: 'Wehrmann', classe: 'A', cidade: 'Campo Novo dos Parecis', uf: 'MT', lat: -13.66, lng: -57.88, exige_visita: true, ult_int: null, fat_12m: 0, oportunidades: 0, obs: 'Prospect indicado por Amaggi · nunca contatado' },
  { id: 84522, razao: 'Fazenda Terra Nova Agropecuária', apelido: 'Terra Nova', classe: 'B', cidade: 'Sinop', uf: 'MT', lat: -11.90, lng: -55.53, exige_visita: false, ult_int: null, fat_12m: 0, oportunidades: 0, obs: 'Lead frio · CNPJ ativo Protheus mas zero histórico' },
  { id: 84527, razao: 'Agropecuária Nova Aurora Ltda ME', apelido: 'Nova Aurora', classe: 'D', cidade: 'Lucas do Rio Verde', uf: 'MT', lat: -13.08, lng: -55.88, exige_visita: false, ult_int: { data: '2026-04-02', cat: 'email', autor: 'Marina Costa · Admin' }, fat_12m: 0, oportunidades: 0, obs: 'Cadastro reativado · sem faturamento em 12m' }
];

function diasDesde(dataISO) {
  if (!dataISO) return null;
  const d = new Date(dataISO);
  return Math.floor((HOJE_CARTEIRA - d) / (1000 * 60 * 60 * 24));
}

// Retorna: em_dia (<60% meta) · aviso (60-100%) · atraso (100-150%) · critico (>150%) · nunca
function statusCobertura(cliente) {
  if (!cliente.ult_int) return 'nunca';
  const dias = diasDesde(cliente.ult_int.data);
  const meta = META_FREQ[cliente.classe];
  // Se exige visita presencial e última interação NÃO foi visita, penaliza para status baseado em zerado
  const zerou = cliente.exige_visita ? cliente.ult_int.cat === 'visita' : true;
  const dias_efetivos = zerou ? dias : Math.max(dias, meta + 1); // se não zerou, cai automaticamente para atraso
  const pct = dias_efetivos / meta;
  if (pct <= 0.6) return 'em_dia';
  if (pct <= 1.0) return 'aviso';
  if (pct <= 1.5) return 'atraso';
  return 'critico';
}

const STATUS_LABELS = {
  em_dia: { label: 'Em dia', cor: '#367C2B', bg: 'rgba(54,124,43,.10)' },
  aviso: { label: 'Aviso', cor: '#B45309', bg: 'rgba(180,83,9,.10)' },
  atraso: { label: 'Atraso', cor: '#DC2626', bg: 'rgba(220,38,38,.10)' },
  critico: { label: 'Crítico', cor: '#7F1D1D', bg: 'rgba(127,29,29,.15)' },
  nunca: { label: 'Nunca contatado', cor: '#6B7280', bg: 'rgba(107,114,128,.10)' }
};

function renderCarteiraCEN() {
  const cens = [
    { id: 'jose_rufino', nome: 'José Rufino', regiao: 'MT Norte · Lucas do Rio Verde', clientes: CARTEIRA_CEN.length },
    { id: 'ana_paula', nome: 'Ana Paula Martins', regiao: 'MT Sul · Rondonópolis', clientes: 19 },
    { id: 'ricardo_lima', nome: 'Ricardo Lima', regiao: 'MT Oeste · Cáceres', clientes: 22 },
    { id: 'fernanda_leite', nome: 'Fernanda Leite', regiao: 'MT Leste · Barra do Garças', clientes: 17 }
  ];

  const total = CARTEIRA_CEN.length;
  const por_status = { em_dia: 0, aviso: 0, atraso: 0, critico: 0, nunca: 0 };
  CARTEIRA_CEN.forEach(c => { por_status[statusCobertura(c)]++; });
  const em_dia_pct = ((por_status.em_dia / total) * 100).toFixed(0);
  const criticos = por_status.critico + por_status.atraso;
  const fat_total = CARTEIRA_CEN.reduce((s, c) => s + c.fat_12m, 0);
  const opp_valor = CARTEIRA_CEN.reduce((s, c) => s + (c.oportunidades_valor || 0), 0);
  const nunca = por_status.nunca;

  return `
    <div class="carteira-page">

      <!-- Header + switcher -->
      <div class="carteira-header">
        <div class="carteira-header-left">
          <h1 class="page-title" style="margin:0">Cobertura de Carteira</h1>
          <div class="carteira-header-sub">
            Painel operacional do CEN · meta de frequência por classe (A 30d · B 60d · C 90d · D 120d) · status calculado contra a meta
          </div>
        </div>
        <div class="carteira-header-right">
          <div class="carteira-context">
            <span class="ctx-label">Ver carteira de</span>
            <div class="ctx-switcher" id="carteiraCenSwitcher">
              <span class="ctx-selected">
                <span class="ctx-avatar">JR</span>
                <span class="ctx-name">José Rufino</span>
                <span class="ctx-region">MT Norte</span>
                <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><polyline points="6 9 12 15 18 9"/></svg>
              </span>
              <div class="ctx-dropdown" id="ctxDropdown" style="display:none">
                ${cens.map(c => `
                  <div class="ctx-item ${c.id === 'jose_rufino' ? 'active' : ''}" data-cen="${c.id}">
                    <span class="ctx-avatar">${c.nome.split(' ').map(n => n[0]).slice(0,2).join('')}</span>
                    <div>
                      <div class="ctx-name">${c.nome}</div>
                      <div class="ctx-region">${c.regiao} · ${c.clientes} clientes</div>
                    </div>
                  </div>
                `).join('')}
              </div>
            </div>
          </div>
          <div class="carteira-role-badge">
            <span class="role-dot"></span>
            Você é <strong>Gerente Regional</strong> · pode trocar de CEN
          </div>
        </div>
      </div>

      <!-- KPIs -->
      <div class="carteira-kpis">
        <div class="carteira-kpi">
          <span class="kpi-label">Clientes na carteira</span>
          <span class="kpi-value">${total}</span>
          <span class="kpi-hint">Ativos no Protheus</span>
        </div>
        <div class="carteira-kpi kpi-good">
          <span class="kpi-label">Em dia</span>
          <span class="kpi-value">${por_status.em_dia} <span style="font-size:14px;color:var(--text-tertiary);font-weight:500">/ ${total}</span></span>
          <span class="kpi-hint">${em_dia_pct}% da carteira</span>
        </div>
        <div class="carteira-kpi kpi-warn">
          <span class="kpi-label">Atraso + Crítico</span>
          <span class="kpi-value">${criticos}</span>
          <span class="kpi-hint">Exige ação nesta semana</span>
        </div>
        <div class="carteira-kpi">
          <span class="kpi-label">Nunca contatado</span>
          <span class="kpi-value">${nunca}</span>
          <span class="kpi-hint">Prospects sem 1º contato</span>
        </div>
        <div class="carteira-kpi">
          <span class="kpi-label">Faturamento 12m</span>
          <span class="kpi-value" style="font-family:'JetBrains Mono',monospace;color:var(--jd-green)">${fmtBRLcompact(fat_total)}</span>
          <span class="kpi-hint">Toda a carteira</span>
        </div>
        <div class="carteira-kpi">
          <span class="kpi-label">Oport. abertas</span>
          <span class="kpi-value" style="font-family:'JetBrains Mono',monospace">${fmtBRLcompact(opp_valor)}</span>
          <span class="kpi-hint">${CARTEIRA_CEN.filter(c => c.oportunidades > 0).length} clientes com pipeline</span>
        </div>
      </div>

      <!-- Corpo: mapa (35%) + tabela (65%) -->
      <div class="carteira-body">

        <!-- Mapa MT Norte -->
        <div class="carteira-mapa-wrap">
          <div class="carteira-panel-header">
            <div>
              <div class="carteira-panel-title">Mapa MT Norte</div>
              <div class="carteira-panel-sub">Pins por status · clique para filtrar tabela</div>
            </div>
            <button class="btn-icon-sm" title="Expandir">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 3 21 3 21 9"/><polyline points="9 21 3 21 3 15"/><line x1="21" y1="3" x2="14" y2="10"/><line x1="3" y1="21" x2="10" y2="14"/></svg>
            </button>
          </div>
          <div class="carteira-mapa" id="carteiraMapa">
            <div id="carteiraMapaLeaflet" style="width:100%;height:100%;border-radius:8px;overflow:hidden;"></div>
          </div>

          <!-- Legenda -->
          <div class="carteira-legenda">
            ${Object.entries(STATUS_LABELS).map(([k, v]) => `
              <div class="leg-item">
                <span class="leg-dot" style="background:${v.cor}"></span>
                <span>${v.label}</span>
                <span class="leg-count">${por_status[k]}</span>
              </div>
            `).join('')}
          </div>

          <!-- Nota sobre origem do mapa (transparência prototípica) -->
          <div class="mapa-fonte-aviso">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>
            </svg>
            <span><b>Prototípico:</b> tiles OpenStreetMap. Em produção, avaliar Mapbox/MapTiler (custo por load) e geocodificação das fazendas.</span>
          </div>
        </div>

        <!-- Tabela filtrável -->
        <div class="carteira-tabela-wrap">
          <div class="carteira-panel-header">
            <div>
              <div class="carteira-panel-title">Carteira detalhada</div>
              <div class="carteira-panel-sub" id="carteiraSub">${total} clientes · ordenados por dias desde última interação</div>
            </div>
            <div class="carteira-actions">
              <button class="btn-secondary btn-sm" id="btnExportCarteira">
                <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
                Exportar CSV
              </button>
            </div>
          </div>

          <!-- Filtros -->
          <div class="carteira-filtros">
            <div class="filtro-grupo">
              <span class="filtro-label">Status</span>
              <div class="filtro-chips" data-filtro="status">
                <button class="chip chip-active" data-val="todos">Todos <span class="chip-count">${total}</span></button>
                <button class="chip" data-val="critico" style="--chip-color:#7F1D1D">Crítico <span class="chip-count">${por_status.critico}</span></button>
                <button class="chip" data-val="atraso" style="--chip-color:#DC2626">Atraso <span class="chip-count">${por_status.atraso}</span></button>
                <button class="chip" data-val="aviso" style="--chip-color:#B45309">Aviso <span class="chip-count">${por_status.aviso}</span></button>
                <button class="chip" data-val="em_dia" style="--chip-color:#367C2B">Em dia <span class="chip-count">${por_status.em_dia}</span></button>
                <button class="chip" data-val="nunca" style="--chip-color:#6B7280">Nunca <span class="chip-count">${por_status.nunca}</span></button>
              </div>
            </div>
            <div class="filtro-grupo">
              <span class="filtro-label">Classe</span>
              <div class="filtro-chips" data-filtro="classe">
                <button class="chip chip-active" data-val="todos">Todas</button>
                <button class="chip" data-val="A">A</button>
                <button class="chip" data-val="B">B</button>
                <button class="chip" data-val="C">C</button>
                <button class="chip" data-val="D">D</button>
              </div>
            </div>
            <div class="filtro-grupo">
              <span class="filtro-label">Categoria de contato</span>
              <div class="filtro-chips" data-filtro="cat">
                <button class="chip chip-active" data-val="todos">Todas</button>
                <button class="chip" data-val="visita">Visita</button>
                <button class="chip" data-val="ligacao">Ligação</button>
                <button class="chip" data-val="whatsapp">WhatsApp</button>
                <button class="chip" data-val="email">E-mail</button>
                <button class="chip" data-val="remota">Remota</button>
              </div>
            </div>
            <div class="filtro-grupo">
              <span class="filtro-label">Exige visita presencial</span>
              <div class="filtro-chips" data-filtro="exige">
                <button class="chip chip-active" data-val="todos">Todos</button>
                <button class="chip" data-val="sim">Sim</button>
                <button class="chip" data-val="nao">Não</button>
              </div>
            </div>
          </div>

          <!-- Tabela -->
          <div class="carteira-tabela">
            <table>
              <thead>
                <tr>
                  <th style="width:32%">Cliente</th>
                  <th>Classe</th>
                  <th>Cidade</th>
                  <th>Última interação</th>
                  <th class="num">Dias</th>
                  <th>Status</th>
                  <th class="num">Fat. 12m</th>
                  <th>Oport.</th>
                  <th class="acoes-th" style="width:80px"></th>
                </tr>
              </thead>
              <tbody id="carteiraTbody">
                <!-- linhas geradas por mount -->
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Drawer registrar contato -->
      <div class="carteira-drawer" id="carteiraDrawer" style="display:none">
        <div class="drawer-backdrop"></div>
        <div class="drawer-panel">
          <div class="drawer-header">
            <div>
              <div class="drawer-title" id="drawerTitle">Registrar contato</div>
              <div class="drawer-sub" id="drawerSub">—</div>
            </div>
            <button class="btn-icon-sm" id="drawerClose">
              <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="drawer-body">
            <div class="ff">
              <label>Categoria da interação</label>
              <div class="cat-picker" id="catPicker">
                ${Object.entries(CAT_INTERACAO).map(([k, v]) => `
                  <button class="cat-btn" data-cat="${k}" style="--cat-color:${v.cor}">
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><path d="${v.icone}"/></svg>
                    ${v.label}
                  </button>
                `).join('')}
              </div>
            </div>
            <div class="ff" style="margin-top:16px">
              <label>Anotação (opcional)</label>
              <textarea class="drawer-textarea" placeholder="Ex: cliente confirmou interesse em revisão da frota" rows="4"></textarea>
            </div>
            <div class="ff" style="margin-top:16px">
              <label>Próxima ação sugerida</label>
              <select class="drawer-select">
                <option>— Sem próxima ação —</option>
                <option>Agendar visita presencial</option>
                <option>Enviar proposta comercial</option>
                <option>Solicitar orçamento de peças</option>
                <option>Follow-up em 15 dias</option>
              </select>
            </div>
          </div>
          <div class="drawer-footer">
            <button class="btn-secondary" id="drawerCancel">Cancelar</button>
            <button class="btn-primary" id="drawerSave">Registrar contato</button>
          </div>
        </div>
      </div>

    </div>
  `;
}

// ============ MOUNT ============
const CARTEIRA_STATE = {
  filtros: { status: 'todos', classe: 'todos', cat: 'todos', exige: 'todos' },
  cidade: null // filtro adicional via mapa
};

function filtrarCarteira() {
  const f = CARTEIRA_STATE.filtros;
  return CARTEIRA_CEN.filter(c => {
    const s = statusCobertura(c);
    if (f.status !== 'todos' && s !== f.status) return false;
    if (f.classe !== 'todos' && c.classe !== f.classe) return false;
    if (f.cat !== 'todos') {
      if (!c.ult_int || c.ult_int.cat !== f.cat) return false;
    }
    if (f.exige !== 'todos') {
      if (f.exige === 'sim' && !c.exige_visita) return false;
      if (f.exige === 'nao' && c.exige_visita) return false;
    }
    if (CARTEIRA_STATE.cidade && c.cidade !== CARTEIRA_STATE.cidade) return false;
    return true;
  }).sort((a, b) => {
    // ordena por urgência: crítico > atraso > aviso > nunca > em_dia · dentro do grupo, mais dias primeiro
    const ordem = { critico: 0, atraso: 1, aviso: 2, nunca: 3, em_dia: 4 };
    const sa = statusCobertura(a), sb = statusCobertura(b);
    if (ordem[sa] !== ordem[sb]) return ordem[sa] - ordem[sb];
    const da = a.ult_int ? diasDesde(a.ult_int.data) : 9999;
    const db = b.ult_int ? diasDesde(b.ult_int.data) : 9999;
    return db - da;
  });
}

// Coordenadas reais (lat/lng) das cidades da regional MT Norte
const CIDADE_LATLNG = {
  'Sinop':                  { lat: -11.8642, lng: -55.5028 },
  'Sorriso':                { lat: -12.5453, lng: -55.7211 },
  'Lucas do Rio Verde':     { lat: -13.0489, lng: -55.9142 },
  'Nova Mutum':             { lat: -13.8300, lng: -56.0800 },
  'Campo Novo dos Parecis': { lat: -13.6742, lng: -57.8925 },
  'Sapezal':                { lat: -12.9906, lng: -58.7647 },
};

// Estado do mapa (referências para redraw sem reinicializar Leaflet)
let __carteiraMap = null;
let __carteiraMarkersLayer = null;

function renderPins() {
  const el = document.getElementById('carteiraMapaLeaflet');
  if (!el || typeof L === 'undefined') return;

  // Inicializa mapa apenas uma vez
  if (!__carteiraMap) {
    // Centro aproximado da regional MT Norte
    __carteiraMap = L.map(el, {
      center: [-12.8, -57.0],
      zoom: 7,
      zoomControl: true,
      scrollWheelZoom: false, // evita capturar scroll da página inadvertidamente
      attributionControl: true,
    });
    // Tiles OpenStreetMap (gratýto, sem chave). Atenção: fair-use em produção.
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 18,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
    }).addTo(__carteiraMap);
    __carteiraMarkersLayer = L.layerGroup().addTo(__carteiraMap);

    // Habilita scroll zoom ao clicar no mapa (padrão Google Maps embed)
    __carteiraMap.on('click', () => __carteiraMap.scrollWheelZoom.enable());
    __carteiraMap.on('mouseout', () => __carteiraMap.scrollWheelZoom.disable());
  }

  // Agrupa clientes por cidade
  const grupos = {};
  CARTEIRA_CEN.forEach(c => {
    const s = statusCobertura(c);
    const coords = CIDADE_LATLNG[c.cidade];
    if (!coords) return;
    if (!grupos[c.cidade]) grupos[c.cidade] = { total: 0, statuses: {}, coords };
    grupos[c.cidade].total++;
    grupos[c.cidade].statuses[s] = (grupos[c.cidade].statuses[s] || 0) + 1;
  });

  // Redesenha marcadores
  __carteiraMarkersLayer.clearLayers();
  const ordem = ['critico', 'atraso', 'aviso', 'nunca', 'em_dia'];

  Object.entries(grupos).forEach(([cidade, g]) => {
    const pior = ordem.find(s => g.statuses[s]) || 'em_dia';
    const cor = STATUS_LABELS[pior].cor;
    const raio = 18 + Math.min(g.total * 1.5, 12);
    const isAtivo = CARTEIRA_STATE.cidade === cidade;

    // "Divisa" da bolha exterior + número central. divIcon permite HTML/CSS livre.
    const html = `
      <div class="leaflet-cluster-pin ${isAtivo ? 'is-active' : ''}" style="--pin-color:${cor}">
        <div class="leaflet-cluster-halo"></div>
        <div class="leaflet-cluster-body" style="width:${raio*2}px;height:${raio*2}px;line-height:${raio*2}px;">
          ${g.total}
        </div>
      </div>
    `;
    const icon = L.divIcon({
      className: 'leaflet-cluster-icon-wrap',
      html,
      iconSize: [raio*2 + 20, raio*2 + 20],
      iconAnchor: [raio + 10, raio + 10],
    });

    // Breakdown para popup
    const breakdown = ordem
      .filter(s => g.statuses[s])
      .map(s => `<div class="pop-row"><span class="pop-dot" style="background:${STATUS_LABELS[s].cor}"></span>${STATUS_LABELS[s].label}<b>${g.statuses[s]}</b></div>`)
      .join('');

    const marker = L.marker([g.coords.lat, g.coords.lng], { icon })
      .bindTooltip(`<strong>${cidade}</strong> · ${g.total} clientes`, {
        direction: 'top',
        offset: [0, -raio],
        className: 'leaflet-tooltip-tracbel',
      })
      .bindPopup(`
        <div class="pop-cluster">
          <div class="pop-title">${cidade}</div>
          <div class="pop-sub">${g.total} clientes na carteira</div>
          <div class="pop-breakdown">${breakdown}</div>
          <button class="pop-btn" data-cidade="${cidade.replace(/"/g, '&quot;')}">${isAtivo ? 'Limpar filtro' : 'Filtrar tabela por ' + cidade}</button>
        </div>
      `, { className: 'leaflet-popup-tracbel', maxWidth: 260, closeButton: false });

    marker.on('click', () => {
      // O click do marker abre popup automático via Leaflet. Handler no botão do popup:
      setTimeout(() => {
        const btn = document.querySelector('.leaflet-popup-content .pop-btn[data-cidade]');
        if (btn && !btn.__wired) {
          btn.__wired = true;
          btn.addEventListener('click', () => {
            const c = btn.getAttribute('data-cidade');
            CARTEIRA_STATE.cidade = CARTEIRA_STATE.cidade === c ? null : c;
            __carteiraMap.closePopup();
            renderPins();
            renderCarteiraTabela();
          });
        }
      }, 30);
    });

    marker.addTo(__carteiraMarkersLayer);
  });

  // Força recomputo do tamanho (se o container estava oculto durante init)
  setTimeout(() => __carteiraMap.invalidateSize(), 60);
}

function renderCarteiraTabela() {
  const tbody = document.getElementById('carteiraTbody');
  const sub = document.getElementById('carteiraSub');
  if (!tbody) return;
  const clientes = filtrarCarteira();
  const total_original = CARTEIRA_CEN.length;
  const filtroCidade = CARTEIRA_STATE.cidade ? ` · filtrado por <strong style="color:var(--jd-green-dark)">${CARTEIRA_STATE.cidade}</strong> <button class="link-clear" onclick="window.limparCidadeFiltro()">limpar</button>` : '';
  sub.innerHTML = `${clientes.length} de ${total_original} clientes · ordenados por urgência${filtroCidade}`;

  if (clientes.length === 0) {
    tbody.innerHTML = `<tr><td colspan="9" style="text-align:center;padding:40px;color:var(--text-tertiary)">Nenhum cliente com esses filtros.</td></tr>`;
    return;
  }

  tbody.innerHTML = clientes.map(c => {
    const s = statusCobertura(c);
    const sl = STATUS_LABELS[s];
    const dias = c.ult_int ? diasDesde(c.ult_int.data) : null;
    const meta = META_FREQ[c.classe];
    const cat = c.ult_int ? CAT_INTERACAO[c.ult_int.cat] : null;
    const opp = c.oportunidades > 0 ? `<a href="#/oportunidades/1517613" class="opp-link" title="Ver oportunidades">${c.oportunidades} · ${fmtBRLcompact(c.oportunidades_valor)}</a>` : '<span style="color:var(--text-tertiary)">—</span>';
    const jaClienteLink = c.id === 84391 ? `<a href="#/clientes/${c.id}" style="color:var(--jd-green-dark);font-weight:600">${c.razao}</a>` : `<span style="font-weight:600">${c.razao}</span>`;
    return `
      <tr>
        <td>
          <div>${jaClienteLink}</div>
          ${c.exige_visita ? '<div class="carteira-cliente-sub"><span class="badge-visita">Visita obrigatória</span></div>' : ''}
          <div class="carteira-cliente-obs">${c.obs}</div>
        </td>
        <td><span class="badge badge-classe-${c.classe}">${c.classe}</span></td>
        <td style="font-size:13px">${c.cidade}</td>
        <td>
          ${c.ult_int ? `
            <div style="display:flex;align-items:center;gap:6px">
              <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="${cat.cor}" stroke-width="2"><path d="${cat.icone}"/></svg>
              <span style="font-size:13px">${cat.label}</span>
            </div>
            <div style="font-size:11px;color:var(--text-tertiary);margin-top:2px;font-family:'JetBrains Mono',monospace">${new Date(c.ult_int.data).toLocaleDateString('pt-BR')}</div>
          ` : `<span style="color:var(--text-tertiary);font-size:13px">— Nunca —</span>`}
        </td>
        <td class="num" style="font-family:'JetBrains Mono',monospace;white-space:nowrap">
          ${dias !== null ? `<strong>${dias}d</strong> <span style="color:var(--text-tertiary);font-size:11px">/ ${meta}d</span>` : '—'}
        </td>
        <td>
          <span class="status-pill" style="color:${sl.cor};background:${sl.bg}">
            <span class="status-dot" style="background:${sl.cor}"></span>
            ${sl.label}
          </span>
        </td>
        <td class="num" style="font-family:'JetBrains Mono',monospace;font-size:12px">${c.fat_12m > 0 ? fmtBRLcompact(c.fat_12m) : '<span style="color:var(--text-tertiary)">—</span>'}</td>
        <td>${opp}</td>
        <td class="acoes-cel">
          <button class="btn-acao btn-acao-primary" data-acao="registrar" data-cliente="${c.id}" title="Registrar contato hoje">
            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.2"><polyline points="20 6 9 17 4 12"/></svg>
          </button>
          <button class="btn-acao" data-acao="agendar" data-cliente="${c.id}" title="Agendar visita/ligação">
            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
          </button>
        </td>
      </tr>
    `;
  }).join('');

  // handlers
  tbody.querySelectorAll('button[data-acao]').forEach(btn => {
    btn.addEventListener('click', () => {
      const acao = btn.getAttribute('data-acao');
      const id = parseInt(btn.getAttribute('data-cliente'));
      const cli = CARTEIRA_CEN.find(x => x.id === id);
      if (acao === 'registrar') abrirDrawerRegistrar(cli);
      if (acao === 'agendar') {
        window.location.hash = '#/agenda';
        setTimeout(() => alert(`Agenda aberta · agendamento para "${cli.apelido}" (protótipo: no MVP abriria o formulário pré-preenchido)`), 300);
      }
    });
  });
}

function abrirDrawerRegistrar(cliente) {
  const drawer = document.getElementById('carteiraDrawer');
  document.getElementById('drawerTitle').textContent = `Registrar contato · ${cliente.apelido}`;
  document.getElementById('drawerSub').textContent = `${cliente.razao} · Classe ${cliente.classe}${cliente.exige_visita ? ' · Visita presencial obrigatória' : ''}`;
  // Se exige visita, destaca visualmente a opção "visita" e alerta se outra for escolhida
  document.querySelectorAll('#catPicker .cat-btn').forEach(b => {
    b.classList.remove('cat-active');
    if (cliente.exige_visita && b.getAttribute('data-cat') !== 'visita') {
      b.classList.add('cat-disabled');
      b.title = 'Cliente exige visita presencial · outras categorias não zeram cobertura';
    } else {
      b.classList.remove('cat-disabled');
      b.title = '';
    }
  });
  drawer.style.display = 'block';
  requestAnimationFrame(() => drawer.classList.add('drawer-open'));
}

function fecharDrawer() {
  const drawer = document.getElementById('carteiraDrawer');
  drawer.classList.remove('drawer-open');
  setTimeout(() => drawer.style.display = 'none', 200);
}

function mountCarteiraCEN() {
  // Reset do mapa (o div antigo já foi descartado pela SPA)
  __carteiraMap = null;
  __carteiraMarkersLayer = null;

  // Filtros chips
  document.querySelectorAll('.carteira-filtros .filtro-chips').forEach(grupo => {
    const key = grupo.getAttribute('data-filtro');
    grupo.querySelectorAll('.chip').forEach(chip => {
      chip.addEventListener('click', () => {
        grupo.querySelectorAll('.chip').forEach(c => c.classList.remove('chip-active'));
        chip.classList.add('chip-active');
        CARTEIRA_STATE.filtros[key] = chip.getAttribute('data-val');
        renderCarteiraTabela();
      });
    });
  });

  // Switcher de CEN
  const sw = document.getElementById('carteiraCenSwitcher');
  const dd = document.getElementById('ctxDropdown');
  if (sw) {
    sw.querySelector('.ctx-selected').addEventListener('click', (e) => {
      e.stopPropagation();
      dd.style.display = dd.style.display === 'none' ? 'block' : 'none';
    });
    document.addEventListener('click', () => { if (dd) dd.style.display = 'none'; });
    dd.querySelectorAll('.ctx-item').forEach(item => {
      item.addEventListener('click', () => {
        const cen = item.getAttribute('data-cen');
        if (cen !== 'jose_rufino') {
          alert(`No protótipo, apenas a carteira do José Rufino está populada. No MVP, esse switcher recarregaria a carteira do CEN selecionado.`);
        }
        dd.style.display = 'none';
      });
    });
  }

  // Drawer
  document.getElementById('drawerClose')?.addEventListener('click', fecharDrawer);
  document.getElementById('drawerCancel')?.addEventListener('click', fecharDrawer);
  document.querySelector('.drawer-backdrop')?.addEventListener('click', fecharDrawer);
  document.getElementById('drawerSave')?.addEventListener('click', () => {
    alert('Contato registrado (protótipo · sem persistência). No MVP, gravaria em tabela CRM_INTERACOES vinculada ao CNPJ.');
    fecharDrawer();
  });
  document.querySelectorAll('#catPicker .cat-btn').forEach(b => {
    b.addEventListener('click', () => {
      if (b.classList.contains('cat-disabled')) {
        if (!confirm('Este cliente exige visita presencial · a interação não zerará o contador de cobertura. Registrar mesmo assim?')) return;
      }
      document.querySelectorAll('#catPicker .cat-btn').forEach(x => x.classList.remove('cat-selected'));
      b.classList.add('cat-selected');
    });
  });

  // Exportar
  document.getElementById('btnExportCarteira')?.addEventListener('click', () => {
    alert('Exportação CSV disparada (protótipo). No MVP, geraria arquivo com os filtros atuais aplicados.');
  });

  // Botão limpar cidade (via window para funcionar no innerHTML)
  window.limparCidadeFiltro = () => {
    CARTEIRA_STATE.cidade = null;
    renderPins();
    renderCarteiraTabela();
  };

  renderPins();
  renderCarteiraTabela();
}

// ===== PIPELINE DE VENDAS (KANBAN) =====

// 6 fases consolidadas do funil comercial
const PIPELINE_FASES = [
  { id: 'qualificacao', label: 'Qualificação',  hint: 'Descoberta e fit inicial' },
  { id: 'diagnostico',  label: 'Diagnóstico',   hint: 'Levantamento técnico e financeiro' },
  { id: 'proposta',     label: 'Proposta',      hint: 'Preparo, envio e apresentação' },
  { id: 'negociacao',   label: 'Negociação',    hint: 'Contraprop., desconto e condições' },
  { id: 'fechamento',   label: 'Fechamento',    hint: 'Aprovações finais e formalização' },
  { id: 'ganho_perdido',label: 'Ganho / Perdido', hint: 'Concluídas no período' }
];

// Todos os CENs da MT Norte (com o Rufino também)
const CENS = {
  'joao':     { nome: 'João Ribeiro',   sigla: 'JR', regional: 'MT Norte', cor: '#367C2B' },
  'rufino':   { nome: 'José Rufino',    sigla: 'JR', regional: 'MT Norte', cor: '#4A7BA8' },
  'ana':      { nome: 'Ana Paula',      sigla: 'AP', regional: 'MT Norte', cor: '#B87333' },
  'ricardo':  { nome: 'Ricardo Alves',  sigla: 'RA', regional: 'MT Norte', cor: '#7A5EA5' },
  'fernanda': { nome: 'Fernanda Melo',  sigla: 'FM', regional: 'MT Norte', cor: '#C74B4B' },
  'marcelo':  { nome: 'Marcelo Silva',  sigla: 'MS', regional: 'MT Sul',   cor: '#2F6C8E' },
  'renata':   { nome: 'Renata Costa',   sigla: 'RC', regional: 'GO',       cor: '#5A8F3E' },
  'pedro':    { nome: 'Pedro Almeida',  sigla: 'PA', regional: 'BA',       cor: '#8A6A2E' }
};

// Linhas de produto com ícones
const LINHA_ICON = {
  'Tratores':       '🚜',
  'Colheitadeiras': '🌾',
  'Pulverizadores': '💧',
  'Plantadeiras':   '🌱',
  'Peças & Serviços':'🔧',
  'Implementos':    '⚙️'
};

// 34 oportunidades espalhadas em 6 fases
const PIPELINE_MOCK = [
  // === QUALIFICAÇÃO (6) ===
  { id: 'OP-2026-08501', titulo: 'Renovação frota compacta',        cliente: 'Grupo Amaggi Fazenda Tanguro',    classe: 'A', cidade: 'Sapezal',          cen: 'joao',    linha: 'Tratores',        modelo: '6135J × 3',    valor:  4200000, probabilidade: 15, previsao: '2026-11-30', dias_fase:  3, fase: 'qualificacao' },
  { id: 'OP-2026-08502', titulo: 'Reposição peças safra 26/27',     cliente: 'SLC Agrícola Sinop',              classe: 'A', cidade: 'Sinop',            cen: 'joao',    linha: 'Peças & Serviços',modelo: 'Contrato anual', valor:  680000, probabilidade: 20, previsao: '2026-10-15', dias_fase:  5, fase: 'qualificacao' },
  { id: 'OP-2026-08503', titulo: 'Prospect pulverizador autoprop.', cliente: 'Fazenda Nova Esperança S/A',       classe: 'B', cidade: 'Lucas do Rio Verde', cen: 'rufino', linha: 'Pulverizadores', modelo: '4630',         valor:  2100000, probabilidade: 10, previsao: '2026-12-20', dias_fase:  8, fase: 'qualificacao' },
  { id: 'OP-2026-08504', titulo: 'Renovação plantadeira 24L',       cliente: 'Agropecuária Vale Verde Ltda',    classe: 'B', cidade: 'Campo Novo dos Parecis', cen: 'rufino', linha: 'Plantadeiras', modelo: 'DB44',       valor:  2650000, probabilidade: 15, previsao: '2027-01-15', dias_fase: 12, fase: 'qualificacao' },
  { id: 'OP-2026-08505', titulo: 'Substituição colheitadeira',      cliente: 'Fazenda Rio Verde Grãos Ltda',    classe: 'B', cidade: 'Lucas do Rio Verde', cen: 'ana',    linha: 'Colheitadeiras', modelo: 'S770',         valor:  3200000, probabilidade: 15, previsao: '2026-11-10', dias_fase:  2, fase: 'qualificacao' },
  { id: 'OP-2026-08506', titulo: 'Trator compacto pecuária',        cliente: 'Sítio Boa Sorte Ltda ME',         classe: 'C', cidade: 'Nova Mutum',       cen: 'ricardo', linha: 'Tratores',        modelo: '5075E',        valor:   320000, probabilidade: 20, previsao: '2026-10-30', dias_fase:  6, fase: 'qualificacao' },

  // === DIAGNÓSTICO (5) ===
  { id: 'OP-2026-08487', titulo: 'Frota Cana 5 unidades',           cliente: 'Usina Santa Clara',                classe: 'A', cidade: 'Sorriso',          cen: 'joao',    linha: 'Tratores',        modelo: '8R 340 × 5',   valor: 12200000, probabilidade: 35, previsao: '2026-12-10', dias_fase:  9, fase: 'diagnostico' },
  { id: 'OP-2026-08488', titulo: 'Safra 26/27 - 3 Colheitadeiras',  cliente: 'Fazenda Boa Vista Agropecuária S/A', classe: 'A', cidade: 'Sorriso',        cen: 'rufino', linha: 'Colheitadeiras', modelo: 'S780 × 3',     valor:  8400000, probabilidade: 40, previsao: '2026-11-25', dias_fase: 11, fase: 'diagnostico' },
  { id: 'OP-2026-08489', titulo: 'Pulverizador substituição',      cliente: 'Agroindustrial Bragagnolo Ltda',   classe: 'A', cidade: 'Lucas do Rio Verde', cen: 'ana',    linha: 'Pulverizadores', modelo: '4630',         valor:  1450000, probabilidade: 30, previsao: '2026-10-25', dias_fase:  4, fase: 'diagnostico' },
  { id: 'OP-2026-08490', titulo: 'Plantadeira safrinha',            cliente: 'Fazenda Cerrado Verde Ltda',       classe: 'C', cidade: 'Campo Novo dos Parecis', cen: 'fernanda', linha: 'Plantadeiras', modelo: 'DB40',    valor:  1800000, probabilidade: 30, previsao: '2026-11-15', dias_fase:  7, fase: 'diagnostico' },
  { id: 'OP-2026-08491', titulo: 'Implemento arado',                cliente: 'Fazenda Aliança Grãos Ltda',       classe: 'B', cidade: 'Lucas do Rio Verde', cen: 'ricardo', linha: 'Implementos', modelo: '2730',        valor:   420000, probabilidade: 35, previsao: '2026-10-20', dias_fase: 15, fase: 'diagnostico' },

  // === PROPOSTA (7) ===
  { id: 'OP-2026-08471', titulo: 'Colheita Milho Safra 26',         cliente: 'Grupo Terra Nova Agronegócios',    classe: 'A', cidade: 'Sinop',            cen: 'joao',    linha: 'Colheitadeiras', modelo: 'S770 × 2',     valor:  5400000, probabilidade: 55, previsao: '2026-10-15', dias_fase: 12, fase: 'proposta', ficha_id: 1517613 },
  { id: 'OP-2026-08472', titulo: 'Renovação Frota Fazenda São Marcos', cliente: 'Agropecuária São Marcos Ltda',  classe: 'B', cidade: 'Sorriso',          cen: 'rufino', linha: 'Tratores',        modelo: '6135J × 2',    valor:  1450000, probabilidade: 55, previsao: '2026-09-30', dias_fase:  8, fase: 'proposta' },
  { id: 'OP-2026-08473', titulo: 'Trator + implemento pacote',      cliente: 'Grupo SLC Bocaiúva Fazenda Parceiro', classe: 'A', cidade: 'Sapezal',        cen: 'ana',    linha: 'Tratores',        modelo: '7230J + arado',valor:  1850000, probabilidade: 60, previsao: '2026-10-05', dias_fase:  6, fase: 'proposta', aprovacao_travada: { por: 'Diretor Comercial', motivo: 'Desconto 12% acima de política 10%', dias: 3 } },
  { id: 'OP-2026-08474', titulo: 'Pulverizador reposição',          cliente: 'Grupo Bom Futuro Sinop',            classe: 'A', cidade: 'Sinop',            cen: 'ana',    linha: 'Pulverizadores', modelo: '4630',         valor:  2100000, probabilidade: 50, previsao: '2026-11-20', dias_fase:  9, fase: 'proposta' },
  { id: 'OP-2026-08475', titulo: 'Trator Cana Reserva',             cliente: 'Fazenda Santa Clara Grãos Ltda',   classe: 'A', cidade: 'Sinop',            cen: 'ricardo', linha: 'Tratores',        modelo: '8R 250',       valor:   720000, probabilidade: 60, previsao: '2026-09-25', dias_fase:  3, fase: 'proposta' },
  { id: 'OP-2026-08476', titulo: 'Colheitadeira usada seminova',    cliente: 'Sementes do Cerrado Ltda',         classe: 'A', cidade: 'Sorriso',          cen: 'fernanda', linha: 'Colheitadeiras', modelo: 'S680 seminova', valor: 2200000, probabilidade: 55, previsao: '2026-10-30', dias_fase:  5, fase: 'proposta' },
  { id: 'OP-2026-08477', titulo: 'Plantadeira 30L nova',            cliente: 'Agropecuária Três Rios',           classe: 'B', cidade: 'Sinop',            cen: 'renata', linha: 'Plantadeiras', modelo: 'DB50',         valor:  2650000, probabilidade: 50, previsao: '2026-11-05', dias_fase:  7, fase: 'proposta' },

  // === NEGOCIAÇÃO (6) ===
  { id: 'OP-2026-08461', titulo: 'Renovação Frota Fazenda São Marcos - Fase 2', cliente: 'Agropecuária São Marcos Ltda', classe: 'B', cidade: 'Sorriso', cen: 'joao',    linha: 'Tratores', modelo: '6135J × 2', valor: 1620000, probabilidade: 70, previsao: '2026-09-15', dias_fase: 14, fase: 'negociacao' },
  { id: 'OP-2026-08462', titulo: 'Colheita Milho - Contraproposta',  cliente: 'Fazenda Passo Fundo Sementes',     classe: 'A', cidade: 'Nova Mutum',       cen: 'joao',    linha: 'Colheitadeiras', modelo: 'S780',         valor:  3100000, probabilidade: 75, previsao: '2026-09-20', dias_fase: 22, fase: 'negociacao' },
  { id: 'OP-2026-08463', titulo: 'Plantadeira 24 Linhas',            cliente: 'Agropec. Três Rios',                classe: 'B', cidade: 'Sinop',           cen: 'rufino', linha: 'Plantadeiras', modelo: 'DB44',        valor:  2650000, probabilidade: 70, previsao: '2026-09-25', dias_fase: 18, fase: 'negociacao', aprovacao_travada: { por: 'Gerente Regional', motivo: 'Prazo pagto 180d fora de política', dias: 5 } },
  { id: 'OP-2026-08464', titulo: 'Safra 26/27 - Contraprop. juros',  cliente: 'Fazenda Boa Vista S.A.',            classe: 'A', cidade: 'Sorriso',         cen: 'ana',    linha: 'Colheitadeiras', modelo: 'S780 × 3',    valor:  8400000, probabilidade: 65, previsao: '2026-10-10', dias_fase: 25, fase: 'negociacao' },
  { id: 'OP-2026-08465', titulo: 'Pulverizador - ajuste condições', cliente: 'Rio Verde Agro',                    classe: 'B', cidade: 'Lucas do Rio Verde', cen: 'marcelo', linha: 'Pulverizadores', modelo: '4630',        valor:  2100000, probabilidade: 70, previsao: '2026-09-30', dias_fase: 11, fase: 'negociacao' },
  { id: 'OP-2026-08466', titulo: 'Trator + garantia estendida',      cliente: 'Fazenda Campo Belo',                classe: 'B', cidade: 'Nova Mutum',      cen: 'fernanda', linha: 'Tratores',        modelo: '6110J',       valor:   890000, probabilidade: 75, previsao: '2026-09-18', dias_fase:  9, fase: 'negociacao' },

  // === FECHAMENTO (5) ===
  { id: 'OP-2026-08441', titulo: 'Trator 6110J Fazenda Cerrado',     cliente: 'Fazenda Cerrado Grande',            classe: 'B', cidade: 'Lucas do Rio Verde', cen: 'joao',   linha: 'Tratores',        modelo: '6110J',       valor:   745000, probabilidade: 85, previsao: '2026-09-05', dias_fase:  4, fase: 'fechamento' },
  { id: 'OP-2026-08442', titulo: 'Trator Reserva Técnica',           cliente: 'Fazenda Campo Belo',                 classe: 'B', cidade: 'Nova Mutum',       cen: 'rufino', linha: 'Tratores',        modelo: '6110J',       valor:   780000, probabilidade: 90, previsao: '2026-09-01', dias_fase:  2, fase: 'fechamento' },
  { id: 'OP-2026-08443', titulo: 'Aquisição Trator Cana',            cliente: 'Usina Nova Aliança',                 classe: 'A', cidade: 'Sinop',            cen: 'ana',    linha: 'Tratores',        modelo: '8R 250',      valor:  1890000, probabilidade: 90, previsao: '2026-09-10', dias_fase:  6, fase: 'fechamento', aprovacao_travada: { por: 'Financeiro', motivo: 'Análise de crédito pendente', dias: 2 } },
  { id: 'OP-2026-08444', titulo: 'Colheitadeira S780 - Entrega Julho', cliente: 'Agropec. Ponta Grossa',            classe: 'A', cidade: 'Sinop',            cen: 'pedro',  linha: 'Colheitadeiras', modelo: 'S780',        valor:  3100000, probabilidade: 90, previsao: '2026-09-08', dias_fase:  3, fase: 'fechamento' },
  { id: 'OP-2026-08445', titulo: 'Contrato peças anual',             cliente: 'Grupo Amaggi Fazenda Tanguro',       classe: 'A', cidade: 'Sapezal',         cen: 'joao',   linha: 'Peças & Serviços', modelo: 'Contrato',    valor:   520000, probabilidade: 85, previsao: '2026-09-12', dias_fase:  5, fase: 'fechamento' },

  // === GANHO / PERDIDO (5) - fechadas nos últimos 30 dias ===
  { id: 'OP-2026-08421', titulo: 'Trator 6110J - GANHO',             cliente: 'Fazenda Cerrado Grande',             classe: 'B', cidade: 'Lucas do Rio Verde', cen: 'joao',  linha: 'Tratores',        modelo: '6110J',       valor:   745000, probabilidade: 100, previsao: '2026-08-15', dias_fase:  10, fase: 'ganho_perdido', resultado: 'ganho', fechado_em: '2026-08-15' },
  { id: 'OP-2026-08422', titulo: 'Colheitadeira S780 - GANHO',       cliente: 'Agropec. Ponta Grossa',              classe: 'A', cidade: 'Sinop',            cen: 'pedro', linha: 'Colheitadeiras', modelo: 'S780',        valor:  3100000, probabilidade: 100, previsao: '2026-08-10', dias_fase:  8, fase: 'ganho_perdido', resultado: 'ganho', fechado_em: '2026-08-10' },
  { id: 'OP-2026-08423', titulo: 'Grupo SLC Agrícola - GANHO',       cliente: 'Grupo SLC Agrícola Sinop',           classe: 'A', cidade: 'Sinop',            cen: 'rufino', linha: 'Tratores',        modelo: '8R 340 × 3',  valor:  6800000, probabilidade: 100, previsao: '2026-08-20', dias_fase:  12, fase: 'ganho_perdido', resultado: 'ganho', fechado_em: '2026-08-20' },
  { id: 'OP-2026-08424', titulo: 'Fazenda Boa Vista - PERDIDO',      cliente: 'Fazenda Boa Vista Agropecuária S/A', classe: 'A', cidade: 'Sorriso',           cen: 'ana',   linha: 'Colheitadeiras', modelo: 'S780',        valor:  2800000, probabilidade:   0, previsao: '2026-08-05', dias_fase:  15, fase: 'ganho_perdido', resultado: 'perdido', motivo_perda: 'Concorrente Case ofereceu 15% menor', fechado_em: '2026-08-05' },
  { id: 'OP-2026-08425', titulo: 'Prospect Wehrmann - PERDIDO',      cliente: 'Grupo Wehrmann Fazenda Nova Divisa', classe: 'A', cidade: 'Campo Novo dos Parecis', cen: 'fernanda', linha: 'Tratores', modelo: '8R 250',      valor:  1800000, probabilidade:   0, previsao: '2026-08-01', dias_fase:  20, fase: 'ganho_perdido', resultado: 'perdido', motivo_perda: 'Cliente adiou investimento para 2027', fechado_em: '2026-08-01' }
];

// Estado da tela
const PIPELINE_STATE = {
  persona: 'cen',   // cen | regional | nacional
  cen: 'joao',      // usado quando persona=cen
  regional: 'MT Norte', // usado quando persona=regional
  filtro_linha: 'todas',
  filtro_valor_min: 0,
  busca: '',
  card_selecionado: null,
  dragging: null
};

// Retorna as oportunidades visíveis dado o estado
function filtrarPipeline() {
  return PIPELINE_MOCK.filter(op => {
    // Persona
    if (PIPELINE_STATE.persona === 'cen' && op.cen !== PIPELINE_STATE.cen) return false;
    if (PIPELINE_STATE.persona === 'regional' && CENS[op.cen].regional !== PIPELINE_STATE.regional) return false;
    // (nacional = todas)

    // Linha
    if (PIPELINE_STATE.filtro_linha !== 'todas' && op.linha !== PIPELINE_STATE.filtro_linha) return false;

    // Valor mínimo
    if (op.valor < PIPELINE_STATE.filtro_valor_min) return false;

    // Busca livre
    if (PIPELINE_STATE.busca) {
      const q = PIPELINE_STATE.busca.toLowerCase();
      const hay = `${op.titulo} ${op.cliente} ${op.cidade} ${op.modelo} ${op.id}`.toLowerCase();
      if (!hay.includes(q)) return false;
    }
    return true;
  });
}

// KPIs derivados
function calcularPipelineKPIs(ops) {
  const abertas = ops.filter(o => o.fase !== 'ganho_perdido');
  const ganhas  = ops.filter(o => o.resultado === 'ganho');
  const perdidas= ops.filter(o => o.resultado === 'perdido');

  const valorTotal = abertas.reduce((s,o) => s + o.valor, 0);
  const valorPonderado = abertas.reduce((s,o) => s + (o.valor * o.probabilidade / 100), 0);
  const ganhoMTD = ganhas.reduce((s,o) => s + o.valor, 0);
  const ticketMedio = abertas.length > 0 ? valorTotal / abertas.length : 0;
  const totalFechadas = ganhas.length + perdidas.length;
  const taxaConversao = totalFechadas > 0 ? (ganhas.length / totalFechadas * 100) : 0;
  const travadas = abertas.filter(o => o.aprovacao_travada).length;

  return { totalAbertas: abertas.length, valorTotal, valorPonderado, ganhoMTD, ticketMedio, taxaConversao, travadas };
}

function renderPipelineVendas() {
  return `
    <div class="pipeline-header">
      <div class="pipeline-header-top">
        <div style="flex:1;min-width:0">
          <div class="pipeline-title">Pipeline de Vendas</div>
          <div class="pipeline-subtitle">Kanban de oportunidades por fase — arraste para avançar, clique para detalhar</div>
        </div>
        <a href="#/oportunidades/nova" class="btn-nova-oport">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nova Oportunidade
        </a>
        <div class="pipeline-persona-switcher">
          <div class="persona-tabs">
            <button class="persona-tab active" data-persona="cen">Meu pipeline</button>
            <button class="persona-tab" data-persona="regional">Regional MT Norte</button>
            <button class="persona-tab" data-persona="nacional">Nacional</button>
          </div>
          <div class="persona-badge" id="personaBadge">
            <span class="persona-dot"></span>
            <strong>Você é Gerente Regional</strong>
            <span class="persona-hint">· vê todo o time MT Norte</span>
          </div>
        </div>
      </div>
    </div>

    <div class="pipeline-kpis" id="pipelineKpis">
      <!-- KPIs renderizados por mount() -->
    </div>

    <div class="pipeline-toolbar">
      <div class="pipeline-search">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
        <input type="text" id="pipelineBusca" placeholder="Buscar oportunidade, cliente, ID, modelo...">
      </div>
      <div class="pipeline-filtros">
        <label>Linha:</label>
        <select id="pipelineFiltroLinha">
          <option value="todas">Todas</option>
          <option>Tratores</option>
          <option>Colheitadeiras</option>
          <option>Pulverizadores</option>
          <option>Plantadeiras</option>
          <option>Peças & Serviços</option>
          <option>Implementos</option>
        </select>
        <label>Valor mín:</label>
        <select id="pipelineFiltroValor">
          <option value="0">Sem filtro</option>
          <option value="500000">R$ 500k+</option>
          <option value="1000000">R$ 1M+</option>
          <option value="3000000">R$ 3M+</option>
          <option value="5000000">R$ 5M+</option>
        </select>
        <button class="btn-secondary" id="pipelineReset">Limpar</button>
      </div>
    </div>

    <div class="pipeline-board" id="pipelineBoard">
      <!-- Colunas renderizadas por mount() -->
    </div>

    <div class="pipeline-drawer" id="pipelineDrawer">
      <div class="drawer-overlay" data-close-drawer></div>
      <div class="drawer-panel" id="pipelineDrawerPanel">
        <!-- Conteúdo renderizado dinamicamente -->
      </div>
    </div>
  `;
}

function mountPipelineVendas() {
  renderPipelineTudo();

  // Switcher persona
  document.querySelectorAll('.persona-tab').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.persona-tab').forEach(b => b.classList.toggle('active', b === btn));
      PIPELINE_STATE.persona = btn.dataset.persona;
      atualizarPersonaBadge();
      renderPipelineTudo();
    });
  });

  // Busca
  document.getElementById('pipelineBusca').addEventListener('input', (e) => {
    PIPELINE_STATE.busca = e.target.value;
    renderPipelineTudo();
  });

  // Filtro linha
  document.getElementById('pipelineFiltroLinha').addEventListener('change', (e) => {
    PIPELINE_STATE.filtro_linha = e.target.value;
    renderPipelineTudo();
  });

  // Filtro valor
  document.getElementById('pipelineFiltroValor').addEventListener('change', (e) => {
    PIPELINE_STATE.filtro_valor_min = parseInt(e.target.value);
    renderPipelineTudo();
  });

  // Reset
  document.getElementById('pipelineReset').addEventListener('click', () => {
    PIPELINE_STATE.busca = '';
    PIPELINE_STATE.filtro_linha = 'todas';
    PIPELINE_STATE.filtro_valor_min = 0;
    document.getElementById('pipelineBusca').value = '';
    document.getElementById('pipelineFiltroLinha').value = 'todas';
    document.getElementById('pipelineFiltroValor').value = '0';
    renderPipelineTudo();
  });

  // Fechar drawer
  document.querySelectorAll('[data-close-drawer]').forEach(el => {
    el.addEventListener('click', () => fecharDrawerPipeline());
  });

  atualizarPersonaBadge();
}

function atualizarPersonaBadge() {
  const badge = document.getElementById('personaBadge');
  if (!badge) return;
  const p = PIPELINE_STATE.persona;
  if (p === 'cen') {
    badge.innerHTML = `<span class="persona-dot"></span><strong>Você é João Ribeiro</strong><span class="persona-hint">· CEN MT Norte · vê só suas oportunidades</span>`;
  } else if (p === 'regional') {
    badge.innerHTML = `<span class="persona-dot"></span><strong>Modo Gerente Regional MT Norte</strong><span class="persona-hint">· 5 CENs · pipeline consolidado</span>`;
  } else {
    badge.innerHTML = `<span class="persona-dot warn"></span><strong>Modo Diretor Comercial</strong><span class="persona-hint">· visão nacional · exige RBAC real</span>`;
  }
}

function renderPipelineTudo() {
  const ops = filtrarPipeline();
  renderPipelineKPIs(ops);
  renderPipelineBoard(ops);
}

function renderPipelineKPIs(ops) {
  const k = calcularPipelineKPIs(ops);
  const el = document.getElementById('pipelineKpis');
  if (!el) return;
  el.innerHTML = `
    <div class="pipeline-kpi">
      <div class="kpi-label">Oport. abertas</div>
      <div class="kpi-value">${k.totalAbertas}</div>
      <div class="kpi-hint">${k.travadas > 0 ? `<span style="color:#B87333">${k.travadas} em aprovação</span>` : 'Nenhuma travada'}</div>
    </div>
    <div class="pipeline-kpi">
      <div class="kpi-label">Pipeline total</div>
      <div class="kpi-value">${fmtBRLcompact(k.valorTotal)}</div>
      <div class="kpi-hint">Soma bruta das abertas</div>
    </div>
    <div class="pipeline-kpi accent">
      <div class="kpi-label">Pipeline ponderado</div>
      <div class="kpi-value">${fmtBRLcompact(k.valorPonderado)}</div>
      <div class="kpi-hint">Valor × probabilidade</div>
    </div>
    <div class="pipeline-kpi good">
      <div class="kpi-label">Ganho MTD</div>
      <div class="kpi-value">${fmtBRLcompact(k.ganhoMTD)}</div>
      <div class="kpi-hint">Fechado nos últimos 30d</div>
    </div>
    <div class="pipeline-kpi">
      <div class="kpi-label">Ticket médio</div>
      <div class="kpi-value">${fmtBRLcompact(k.ticketMedio)}</div>
      <div class="kpi-hint">Nas ${k.totalAbertas} abertas</div>
    </div>
    <div class="pipeline-kpi">
      <div class="kpi-label">Taxa de conversão</div>
      <div class="kpi-value">${k.taxaConversao.toFixed(0)}%</div>
      <div class="kpi-hint">Ganhas ÷ fechadas 30d</div>
    </div>
  `;
}

function renderPipelineBoard(ops) {
  const board = document.getElementById('pipelineBoard');
  if (!board) return;

  board.innerHTML = PIPELINE_FASES.map(fase => {
    const opsFase = ops.filter(o => o.fase === fase.id);
    const valorFase = opsFase.reduce((s,o) => s + o.valor, 0);
    const isFinal = fase.id === 'ganho_perdido';
    return `
      <div class="pipeline-coluna ${isFinal ? 'final' : ''}" data-fase="${fase.id}">
        <div class="coluna-header">
          <div class="coluna-title">
            <span>${fase.label}</span>
            <span class="coluna-count">${opsFase.length}</span>
          </div>
          <div class="coluna-total">${fmtBRLcompact(valorFase)}</div>
        </div>
        <div class="coluna-cards" data-fase-drop="${fase.id}">
          ${opsFase.map(op => renderPipelineCard(op)).join('')}
          ${opsFase.length === 0 ? '<div class="coluna-empty">— Sem oportunidades —</div>' : ''}
        </div>
      </div>
    `;
  }).join('');

  // Wire drag events
  document.querySelectorAll('.pipeline-card').forEach(card => {
    card.addEventListener('dragstart', handleDragStart);
    card.addEventListener('dragend', handleDragEnd);
    card.addEventListener('click', (e) => {
      if (card.classList.contains('dragging')) return;
      abrirDrawerPipeline(card.dataset.opId);
    });
  });
  document.querySelectorAll('.coluna-cards').forEach(zone => {
    zone.addEventListener('dragover', handleDragOver);
    zone.addEventListener('dragleave', handleDragLeave);
    zone.addEventListener('drop', handleDrop);
  });
}

function renderPipelineCard(op) {
  const cen = CENS[op.cen];
  const travada = op.aprovacao_travada;
  const isGanho = op.resultado === 'ganho';
  const isPerdido = op.resultado === 'perdido';
  const draggable = !travada && op.fase !== 'ganho_perdido';

  let cardClass = 'pipeline-card';
  if (travada) cardClass += ' travada';
  if (isGanho) cardClass += ' ganho';
  if (isPerdido) cardClass += ' perdido';
  if (op.classe === 'A') cardClass += ' classe-a';
  if (op.criada_agora) cardClass += ' criada-agora';

  return `
    <div class="${cardClass}" data-op-id="${op.id}" ${draggable ? 'draggable="true"' : ''}>
      ${op.criada_agora ? '<div class="card-badge-nova">NOVA · nesta sessão</div>' : ''}
      ${travada ? `
        <div class="card-alerta">
          <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M12 9v4"/><path d="M12 17h.01"/><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/></svg>
          <span>Aguardando <strong>${travada.por}</strong> · ${travada.dias}d</span>
        </div>
      ` : ''}
      ${isGanho ? `<div class="card-resultado ganho">✓ GANHO</div>` : ''}
      ${isPerdido ? `<div class="card-resultado perdido">✗ PERDIDO</div>` : ''}
      <div class="card-titulo">${op.titulo}</div>
      <div class="card-cliente">
        <span class="classe-badge classe-${op.classe.toLowerCase()}">${op.classe}</span>
        <span class="cliente-nome">${op.cliente}</span>
      </div>
      <div class="card-meta">
        <span>${LINHA_ICON[op.linha] || '📦'} ${op.linha}</span>
        <span class="card-cidade">${op.cidade}</span>
      </div>
      <div class="card-valor">${fmtBRLcompact(op.valor)}</div>
      <div class="card-rodape">
        <div class="card-cen" title="${cen.nome} · ${cen.regional}">
          <span class="cen-avatar" style="background:${cen.cor}">${cen.sigla}</span>
          <span class="cen-nome">${cen.nome.split(' ')[0]}</span>
        </div>
        <div class="card-prob" title="Probabilidade">${op.probabilidade}%</div>
      </div>
      ${!draggable && op.fase !== 'ganho_perdido' ? '<div class="card-lock-hint">🔒 Não pode arrastar até aprovação</div>' : ''}
      ${isPerdido && op.motivo_perda ? `<div class="card-motivo">${op.motivo_perda}</div>` : ''}
    </div>
  `;
}

// === Drag & Drop ===
function handleDragStart(e) {
  PIPELINE_STATE.dragging = e.target.dataset.opId;
  e.target.classList.add('dragging');
  e.dataTransfer.effectAllowed = 'move';
  e.dataTransfer.setData('text/plain', e.target.dataset.opId);
}
function handleDragEnd(e) {
  e.target.classList.remove('dragging');
  document.querySelectorAll('.coluna-cards').forEach(c => c.classList.remove('drop-target'));
  PIPELINE_STATE.dragging = null;
}
function handleDragOver(e) {
  e.preventDefault();
  e.currentTarget.classList.add('drop-target');
  e.dataTransfer.dropEffect = 'move';
}
function handleDragLeave(e) {
  e.currentTarget.classList.remove('drop-target');
}
function handleDrop(e) {
  e.preventDefault();
  e.currentTarget.classList.remove('drop-target');
  const opId = e.dataTransfer.getData('text/plain');
  const novaFase = e.currentTarget.dataset.faseDrop;
  const op = PIPELINE_MOCK.find(o => o.id === opId);
  if (!op || !novaFase || op.fase === novaFase) return;
  if (op.aprovacao_travada) return; // sanity

  // Se estiver indo para ganho_perdido, perguntar resultado
  if (novaFase === 'ganho_perdido') {
    const resultado = confirm(`Marcar "${op.titulo}" como GANHO?\n\nOK = Ganho\nCancelar = Perdido`);
    op.resultado = resultado ? 'ganho' : 'perdido';
    op.probabilidade = resultado ? 100 : 0;
    op.fechado_em = '2026-08-25';
    if (!resultado) {
      op.motivo_perda = prompt('Motivo da perda:') || 'Sem motivo registrado';
    }
  }

  op.fase = novaFase;
  op.dias_fase = 0;
  renderPipelineTudo();
}

// === Drawer ===
function abrirDrawerPipeline(opId) {
  const op = PIPELINE_MOCK.find(o => o.id === opId);
  if (!op) return;
  PIPELINE_STATE.card_selecionado = opId;

  const cen = CENS[op.cen];
  const travada = op.aprovacao_travada;
  const faseAtual = PIPELINE_FASES.find(f => f.id === op.fase);
  const proximaFase = PIPELINE_FASES[PIPELINE_FASES.findIndex(f => f.id === op.fase) + 1];

  const panel = document.getElementById('pipelineDrawerPanel');
  panel.innerHTML = `
    <div class="drawer-header">
      <div style="flex:1;min-width:0">
        <div class="drawer-eyebrow">${op.id} · ${op.linha}</div>
        <div class="drawer-title">${op.titulo}</div>
        <div class="drawer-subtitle">
          <span class="classe-badge classe-${op.classe.toLowerCase()}" style="vertical-align:middle">${op.classe}</span>
          <a href="#/clientes/${op.ficha_id || 84391}" style="color:var(--jd-green-dark);font-weight:500">${op.cliente}</a> · ${op.cidade}
        </div>
      </div>
      <button class="drawer-close" data-close-drawer>✕</button>
    </div>

    ${travada ? `
      <div class="drawer-alerta">
        <div class="alerta-icon">⚠️</div>
        <div>
          <div style="font-weight:600;color:#B87333">Aguardando aprovação</div>
          <div style="font-size:12px;color:var(--text-secondary);margin-top:2px">
            <strong>${travada.por}</strong> há ${travada.dias} dias · ${travada.motivo}
          </div>
        </div>
      </div>
    ` : ''}

    <div class="drawer-kpi-grid">
      <div class="drawer-kpi">
        <div class="drawer-kpi-label">Valor</div>
        <div class="drawer-kpi-value">${fmtBRLcompact(op.valor)}</div>
      </div>
      <div class="drawer-kpi">
        <div class="drawer-kpi-label">Probabilidade</div>
        <div class="drawer-kpi-value">${op.probabilidade}%</div>
      </div>
      <div class="drawer-kpi">
        <div class="drawer-kpi-label">Previsão fech.</div>
        <div class="drawer-kpi-value" style="font-size:15px">${op.previsao.split('-').reverse().join('/')}</div>
      </div>
      <div class="drawer-kpi">
        <div class="drawer-kpi-label">Dias na fase</div>
        <div class="drawer-kpi-value">${op.dias_fase}<span style="font-size:12px;color:var(--text-tertiary);margin-left:4px">d</span></div>
      </div>
    </div>

    <div class="drawer-section">
      <div class="drawer-section-title">Modelo / configuração</div>
      <div class="drawer-modelo">${LINHA_ICON[op.linha] || '📦'} ${op.modelo}</div>
    </div>

    <div class="drawer-section">
      <div class="drawer-section-title">CEN responsável</div>
      <div class="drawer-cen">
        <span class="cen-avatar" style="background:${cen.cor};width:32px;height:32px;font-size:13px">${cen.sigla}</span>
        <div>
          <div style="font-weight:600">${cen.nome}</div>
          <div style="font-size:11px;color:var(--text-tertiary)">Regional ${cen.regional}</div>
        </div>
      </div>
    </div>

    <div class="drawer-section">
      <div class="drawer-section-title">Fase atual</div>
      <div class="drawer-fase-atual">
        <div class="fase-badge">${faseAtual.label}</div>
        <div style="font-size:12px;color:var(--text-secondary);margin-top:4px">${faseAtual.hint}</div>
      </div>
    </div>

    ${op.resultado === 'perdido' ? `
      <div class="drawer-section">
        <div class="drawer-section-title">Motivo da perda</div>
        <div style="padding:10px 12px;background:#FEF2F2;border:1px solid #FEE2E2;border-radius:6px;font-size:13px;color:#991B1B">${op.motivo_perda}</div>
      </div>
    ` : ''}

    <div class="drawer-actions">
      ${op.ficha_id ? `<a href="#/oportunidades/${op.ficha_id}" class="drawer-btn primary">Abrir ficha completa</a>` : ''}
      <button class="drawer-btn">Registrar contato</button>
      <button class="drawer-btn">Adicionar nota</button>
      ${proximaFase && !travada && op.fase !== 'ganho_perdido' ? `
        <button class="drawer-btn success" onclick="avancarFasePipeline('${op.id}')">
          Avançar para ${proximaFase.label} →
        </button>
      ` : ''}
      ${travada ? `<button class="drawer-btn warn">Ver detalhes da aprovação</button>` : ''}
      ${op.fase !== 'ganho_perdido' ? `<button class="drawer-btn danger">Marcar como perdida</button>` : ''}
    </div>
  `;

  document.getElementById('pipelineDrawer').classList.add('open');
}

function fecharDrawerPipeline() {
  document.getElementById('pipelineDrawer').classList.remove('open');
  PIPELINE_STATE.card_selecionado = null;
}

window.avancarFasePipeline = function(opId) {
  const op = PIPELINE_MOCK.find(o => o.id === opId);
  if (!op) return;
  const idx = PIPELINE_FASES.findIndex(f => f.id === op.fase);
  const proxima = PIPELINE_FASES[idx + 1];
  if (!proxima) return;
  op.fase = proxima.id;
  op.dias_fase = 0;
  op.probabilidade = Math.min(100, op.probabilidade + 15);
  fecharDrawerPipeline();
  renderPipelineTudo();
};

// ===== CLIENTES (LISTA) =====

// Atributos comerciais que enriquecem CARTEIRA_CEN (já existente)
// e clientes de outras regionais para modo Regional/Nacional.

const CLIENTES_EXTRA = {
  // Atributos adicionais para os 24 clientes do CARTEIRA_CEN (indexado por id)
  84391: { cnpj: '18.245.339/0001-42', segmento: 'Grãos', porte: 'Grande',    n_equipamentos: 12, cen: 'joao',    ult_compra: '2025-11-14' },
  84402: { cnpj: '14.567.821/0001-08', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 24, cen: 'rufino',  ult_compra: '2024-08-22' },
  84418: { cnpj: '22.851.774/0001-91', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 31, cen: 'ana',     ult_compra: '2025-03-11' },
  84425: { cnpj: '09.472.318/0001-55', segmento: 'Grãos', porte: 'Médio',     n_equipamentos:  4, cen: 'ricardo', ult_compra: '2024-04-05' },
  84430: { cnpj: '11.234.567/0001-89', segmento: 'Grãos', porte: 'Grande',    n_equipamentos:  8, cen: 'ricardo', ult_compra: '2025-06-30' },
  84437: { cnpj: '27.881.443/0001-17', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 18, cen: 'ana',     ult_compra: '2025-09-19' },
  84445: { cnpj: '15.442.998/0001-33', segmento: 'Grãos', porte: 'Médio',     n_equipamentos:  3, cen: 'rufino',  ult_compra: '2024-11-08' },
  84448: { cnpj: '19.556.101/0001-70', segmento: 'Grãos', porte: 'Médio',     n_equipamentos:  2, cen: 'rufino',  ult_compra: '2023-12-15' },
  84452: { cnpj: '31.775.802/0001-64', segmento: 'Grãos', porte: 'Grande',    n_equipamentos:  6, cen: 'fernanda',ult_compra: '2025-04-22' },
  84458: { cnpj: '08.123.456/0001-27', segmento: 'Agroindústria', porte: 'Grande', n_equipamentos: 9, cen: 'ana', ult_compra: '2025-07-30' },
  84461: { cnpj: '02.994.181/0001-88', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 45, cen: 'joao',    ult_compra: '2025-10-14' },
  84467: { cnpj: '17.663.220/0001-45', segmento: 'Grãos', porte: 'Médio',     n_equipamentos:  4, cen: 'ana',     ult_compra: '2024-09-11' },
  84472: { cnpj: '25.114.789/0001-52', segmento: 'Pecuária', porte: 'Pequeno', n_equipamentos: 1, cen: 'ricardo', ult_compra: '2023-05-18' },
  84478: { cnpj: '13.885.223/0001-04', segmento: 'Grãos', porte: 'Grande',    n_equipamentos:  7, cen: 'joao',    ult_compra: '2025-08-25' },
  84483: { cnpj: '29.447.116/0001-89', segmento: 'Agroindústria', porte: 'Grande', n_equipamentos: 8, cen: 'fernanda', ult_compra: '2025-05-13' },
  84489: { cnpj: '00.732.981/0001-72', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 27, cen: 'joao',    ult_compra: '2025-08-12' },
  84495: { cnpj: '16.994.552/0001-38', segmento: 'Grãos', porte: 'Médio',     n_equipamentos:  3, cen: 'ricardo', ult_compra: '2024-10-19' },
  84501: { cnpj: '21.336.774/0001-16', segmento: 'Grãos', porte: 'Médio',     n_equipamentos:  4, cen: 'rufino',  ult_compra: '2025-01-08' },
  84508: { cnpj: '32.114.855/0001-93', segmento: 'Grãos', porte: 'Pequeno',   n_equipamentos:  2, cen: 'fernanda',ult_compra: '2023-08-20' },
  84512: { cnpj: '18.223.664/0001-15', segmento: 'Pecuária', porte: 'Pequeno', n_equipamentos: 1, cen: 'ricardo', ult_compra: '2024-02-14' },
  84518: { cnpj: '04.556.882/0001-49', segmento: 'Grãos', porte: 'Corporate', n_equipamentos:  0, cen: 'fernanda',ult_compra: null },
  84523: { cnpj: '28.771.033/0001-61', segmento: 'Grãos', porte: 'Médio',     n_equipamentos:  0, cen: 'rufino',  ult_compra: null },
  84528: { cnpj: '10.884.552/0001-77', segmento: 'Cana',  porte: 'Grande',    n_equipamentos:  5, cen: 'joao',    ult_compra: '2025-02-08' },
  84532: { cnpj: '23.667.114/0001-08', segmento: 'Cana',  porte: 'Corporate', n_equipamentos: 14, cen: 'ana',     ult_compra: '2024-12-03' }
};

// Clientes de outras regionais (para modo Regional/Nacional)
const CLIENTES_OUTRAS = [
  // MT Sul (CEN Marcelo Silva)
  { id: 85101, razao: 'Rio Verde Agro',                          apelido: 'Rio Verde',         classe: 'B', cidade: 'Rondonópolis',      uf: 'MT', cnpj: '19.884.556/0001-02', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 5, cen: 'marcelo', ult_compra: '2025-06-12', ult_int: { data: '2026-08-01', cat: 'ligacao', autor: 'Marcelo Silva' }, fat_12m: 780000,  oportunidades: 1, oportunidades_valor: 2100000, exige_visita: false, obs: 'Contra-proposta em análise' },
  { id: 85105, razao: 'Fazenda Campo Belo',                       apelido: 'Campo Belo',        classe: 'B', cidade: 'Rondonópolis',      uf: 'MT', cnpj: '15.223.998/0001-11', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 3, cen: 'marcelo', ult_compra: '2025-04-08', ult_int: { data: '2026-08-14', cat: 'visita', autor: 'Marcelo Silva' }, fat_12m: 620000,  oportunidades: 2, oportunidades_valor: 1670000, exige_visita: false, obs: 'Trator + garantia estendida em fechamento' },
  { id: 85110, razao: 'Agropec. Ponta Grossa',                    apelido: 'Ponta Grossa',      classe: 'A', cidade: 'Primavera do Leste', uf: 'MT', cnpj: '08.774.223/0001-56', segmento: 'Grãos', porte: 'Grande',    n_equipamentos: 12, cen: 'pedro',   ult_compra: '2025-07-20', ult_int: { data: '2026-08-05', cat: 'visita', autor: 'Pedro Almeida' }, fat_12m: 3120000, oportunidades: 1, oportunidades_valor: 3100000, exige_visita: true, obs: 'Cliente TOP MT Sul' },
  { id: 85115, razao: 'Fazenda Cerrado Grande',                   apelido: 'Cerrado Grande',    classe: 'B', cidade: 'Primavera do Leste', uf: 'MT', cnpj: '30.114.665/0001-83', segmento: 'Grãos', porte: 'Grande',    n_equipamentos: 6, cen: 'joao',    ult_compra: '2025-08-15', ult_int: { data: '2026-08-15', cat: 'visita', autor: 'João Ribeiro' },  fat_12m: 1980000, oportunidades: 1, oportunidades_valor: 745000, exige_visita: false, obs: 'Trator 6110J em fechamento' },

  // MT Sul restante
  { id: 85120, razao: 'Grupo Terra Nova Agronegócios',            apelido: 'Terra Nova',        classe: 'A', cidade: 'Sinop',              uf: 'MT', cnpj: '11.556.889/0001-40', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 22, cen: 'joao',    ult_compra: '2025-09-30', ult_int: { data: '2026-08-19', cat: 'visita', autor: 'João Ribeiro' },  fat_12m: 4890000, oportunidades: 1, oportunidades_valor: 5400000, exige_visita: true, obs: 'Renegociação safra 26/27' },
  { id: 85125, razao: 'Agropecuária São Marcos Ltda',             apelido: 'São Marcos',        classe: 'B', cidade: 'Sorriso',            uf: 'MT', cnpj: '18.774.663/0001-27', segmento: 'Grãos', porte: 'Grande',    n_equipamentos: 7, cen: 'rufino',  ult_compra: '2025-06-05', ult_int: { data: '2026-08-16', cat: 'visita', autor: 'José Rufino' },  fat_12m: 1450000, oportunidades: 2, oportunidades_valor: 3070000, exige_visita: false, obs: 'Renovação frota fase 2' },
  { id: 85130, razao: 'Fazenda Boa Vista S.A.',                   apelido: 'Boa Vista MT-Sul',  classe: 'A', cidade: 'Sorriso',            uf: 'MT', cnpj: '02.669.335/0001-91', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 19, cen: 'ana',     ult_compra: '2025-05-25', ult_int: { data: '2026-08-10', cat: 'visita', autor: 'Ana Paula' },     fat_12m: 5230000, oportunidades: 1, oportunidades_valor: 8400000, exige_visita: true, obs: 'Contraproposta juros safra' },
  { id: 85135, razao: 'Agropec. Três Rios',                       apelido: 'Três Rios',         classe: 'B', cidade: 'Sinop',              uf: 'MT', cnpj: '25.115.884/0001-09', segmento: 'Grãos', porte: 'Grande',    n_equipamentos: 4, cen: 'renata',  ult_compra: '2024-10-14', ult_int: { data: '2026-08-11', cat: 'email', autor: 'Renata Costa' },  fat_12m:  980000, oportunidades: 1, oportunidades_valor: 2650000, exige_visita: false, obs: 'Plantadeira 30L em proposta' },
  { id: 85140, razao: 'Usina Nova Aliança',                       apelido: 'Nova Aliança',      classe: 'A', cidade: 'Sinop',              uf: 'MT', cnpj: '13.884.552/0001-16', segmento: 'Cana',  porte: 'Corporate', n_equipamentos: 16, cen: 'ana',     ult_compra: '2025-08-08', ult_int: { data: '2026-08-08', cat: 'visita', autor: 'Ana Paula' },     fat_12m: 2340000, oportunidades: 1, oportunidades_valor: 1890000, exige_visita: true, obs: 'Trator cana em análise crédito' },

  // GO (CEN Renata Costa)
  { id: 86201, razao: 'Fazenda Serra Dourada',                    apelido: 'Serra Dourada',     classe: 'A', cidade: 'Rio Verde',           uf: 'GO', cnpj: '16.223.998/0001-72', segmento: 'Grãos', porte: 'Grande',    n_equipamentos: 11, cen: 'renata',  ult_compra: '2025-04-20', ult_int: { data: '2026-08-04', cat: 'visita', autor: 'Renata Costa' }, fat_12m: 2890000, oportunidades: 0, exige_visita: true, obs: 'Cliente há 8 anos' },
  { id: 86205, razao: 'Agropecuária Vale do Ipê',                 apelido: 'Vale do Ipê',       classe: 'B', cidade: 'Cristalina',          uf: 'GO', cnpj: '09.445.881/0001-25', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 5, cen: 'renata',  ult_compra: '2024-11-30', ult_int: { data: '2026-07-19', cat: 'ligacao', autor: 'Renata Costa' }, fat_12m:  780000, oportunidades: 0, exige_visita: false, obs: 'Aguardando safra' },
  { id: 86210, razao: 'Grupo Cerrado Central',                    apelido: 'Cerrado Central',   classe: 'A', cidade: 'Rio Verde',           uf: 'GO', cnpj: '27.116.884/0001-58', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 28, cen: 'renata',  ult_compra: '2025-09-10', ult_int: { data: '2026-08-18', cat: 'visita', autor: 'Renata Costa' }, fat_12m: 6780000, oportunidades: 2, oportunidades_valor: 4200000, exige_visita: true, obs: 'TOP 5 GO' },
  { id: 86215, razao: 'Fazenda Santa Rita',                       apelido: 'Santa Rita',        classe: 'B', cidade: 'Jataí',               uf: 'GO', cnpj: '11.667.229/0001-40', segmento: 'Pecuária', porte: 'Médio', n_equipamentos: 4, cen: 'renata',  ult_compra: '2024-06-15', ult_int: { data: '2026-06-22', cat: 'whatsapp', autor: 'Renata Costa' }, fat_12m:  520000, oportunidades: 0, exige_visita: false, obs: 'Prefere WhatsApp · rebanho 8k cabeças' },
  { id: 86220, razao: 'Agroindústria Cristalina Ltda',            apelido: 'Cristalina AGI',    classe: 'A', cidade: 'Cristalina',          uf: 'GO', cnpj: '19.882.114/0001-93', segmento: 'Agroindústria', porte: 'Grande', n_equipamentos: 8, cen: 'renata', ult_compra: '2025-03-05', ult_int: { data: '2026-08-01', cat: 'visita', autor: 'Renata Costa' }, fat_12m: 3120000, oportunidades: 0, exige_visita: true, obs: 'Beneficiamento grãos' },
  { id: 86225, razao: 'Sitio Verde Vale ME',                      apelido: 'Verde Vale',        classe: 'C', cidade: 'Jataí',               uf: 'GO', cnpj: '05.114.556/0001-07', segmento: 'Pecuária', porte: 'Pequeno', n_equipamentos: 1, cen: 'renata', ult_compra: '2023-11-08', ult_int: { data: '2026-05-12', cat: 'email', autor: 'Renata Costa' }, fat_12m:  145000, oportunidades: 0, exige_visita: false, obs: 'Cliente pequeno · pouca frequência' },

  // BA (CEN Pedro Almeida)
  { id: 87301, razao: 'Fazenda Barreiras Norte',                  apelido: 'Barreiras Norte',   classe: 'A', cidade: 'Barreiras',           uf: 'BA', cnpj: '20.884.556/0001-49', segmento: 'Grãos', porte: 'Grande',    n_equipamentos: 14, cen: 'pedro',   ult_compra: '2025-06-25', ult_int: { data: '2026-08-12', cat: 'visita', autor: 'Pedro Almeida' }, fat_12m: 3450000, oportunidades: 0, exige_visita: true, obs: 'Cliente MATOPIBA' },
  { id: 87305, razao: 'Agropecuária Oeste Ltda',                  apelido: 'Oeste',             classe: 'B', cidade: 'Luís Eduardo Magalhães', uf: 'BA', cnpj: '14.223.667/0001-83', segmento: 'Grãos', porte: 'Grande', n_equipamentos: 6, cen: 'pedro',   ult_compra: '2025-01-30', ult_int: { data: '2026-07-25', cat: 'ligacao', autor: 'Pedro Almeida' }, fat_12m: 1230000, oportunidades: 0, exige_visita: false, obs: 'Trocou 2 pulverizadores' },
  { id: 87310, razao: 'Grupo Sertão Fértil',                      apelido: 'Sertão Fértil',     classe: 'A', cidade: 'Formosa do Rio Preto', uf: 'BA', cnpj: '22.114.885/0001-56', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 32, cen: 'pedro', ult_compra: '2025-08-01', ult_int: { data: '2026-08-01', cat: 'visita', autor: 'Pedro Almeida' }, fat_12m: 5670000, oportunidades: 1, oportunidades_valor: 4800000, exige_visita: true, obs: 'TOP 3 BA' },
  { id: 87315, razao: 'Fazenda Chapadão',                         apelido: 'Chapadão',          classe: 'B', cidade: 'São Desidério',        uf: 'BA', cnpj: '07.556.229/0001-70', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 5, cen: 'pedro',   ult_compra: '2024-09-18', ult_int: { data: '2026-07-05', cat: 'whatsapp', autor: 'Pedro Almeida' }, fat_12m:  890000, oportunidades: 0, exige_visita: false, obs: 'Renovação em fase 4' },

  // MT Norte adicional (fictício, para reforçar densidade regional)
  { id: 84545, razao: 'Fazenda Girassol Ltda',                    apelido: 'Girassol',          classe: 'C', cidade: 'Sinop',              uf: 'MT', cnpj: '31.664.225/0001-88', segmento: 'Grãos', porte: 'Pequeno',   n_equipamentos: 1, cen: 'fernanda',ult_compra: '2023-07-14', ult_int: { data: '2026-07-30', cat: 'ligacao', autor: 'Fernanda Melo' }, fat_12m:  180000, oportunidades: 0, exige_visita: false, obs: 'Cliente C · atendimento remoto' },
  { id: 84551, razao: 'Agropec. Vale do Teles',                   apelido: 'Vale do Teles',     classe: 'B', cidade: 'Sinop',              uf: 'MT', cnpj: '18.881.116/0001-27', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 3, cen: 'joao',    ult_compra: '2024-12-08', ult_int: { data: '2026-08-12', cat: 'visita', autor: 'João Ribeiro' }, fat_12m:  680000, oportunidades: 0, exige_visita: false, obs: '13 dias · em dia' },
  { id: 84557, razao: 'Fazenda Novo Horizonte',                   apelido: 'Novo Horizonte',    classe: 'B', cidade: 'Lucas do Rio Verde', uf: 'MT', cnpj: '15.442.885/0001-16', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 4, cen: 'rufino',  ult_compra: '2025-02-22', ult_int: { data: '2026-07-15', cat: 'ligacao', autor: 'José Rufino' }, fat_12m:  590000, oportunidades: 1, oportunidades_valor: 780000, exige_visita: false, obs: 'Trator reserva em análise' },
  { id: 84563, razao: 'Agroindústria Parecis',                    apelido: 'Parecis AGI',       classe: 'A', cidade: 'Campo Novo dos Parecis', uf: 'MT', cnpj: '27.115.882/0001-93', segmento: 'Agroindústria', porte: 'Grande', n_equipamentos: 6, cen: 'ricardo', ult_compra: '2025-05-30', ult_int: { data: '2026-08-14', cat: 'visita', autor: 'Ricardo Alves' }, fat_12m: 1980000, oportunidades: 0, exige_visita: true, obs: 'Beneficiamento soja' },
  { id: 84569, razao: 'Fazenda Ouro Verde',                       apelido: 'Ouro Verde',        classe: 'B', cidade: 'Sorriso',            uf: 'MT', cnpj: '11.885.663/0001-05', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 3, cen: 'fernanda',ult_compra: '2024-08-14', ult_int: { data: '2026-06-28', cat: 'ligacao', autor: 'Fernanda Melo' }, fat_12m:  510000, oportunidades: 0, exige_visita: false, obs: '58 dias · dentro meta B' },
  { id: 84575, razao: 'Grupo Fazendas Reunidas MT',               apelido: 'Faz. Reunidas MT',  classe: 'A', cidade: 'Sinop',              uf: 'MT', cnpj: '05.114.667/0001-30', segmento: 'Grãos', porte: 'Corporate', n_equipamentos: 38, cen: 'joao',    ult_compra: '2025-10-05', ult_int: { data: '2026-08-22', cat: 'visita', autor: 'João Ribeiro' }, fat_12m: 8890000, oportunidades: 2, oportunidades_valor: 12200000, exige_visita: true, obs: 'TOP 1 MT Norte · frota 100% JD' },
  { id: 84581, razao: 'Fazenda Barranca Alta',                    apelido: 'Barranca Alta',     classe: 'C', cidade: 'Nova Mutum',        uf: 'MT', cnpj: '19.223.554/0001-14', segmento: 'Pecuária', porte: 'Pequeno', n_equipamentos: 1, cen: 'ricardo', ult_compra: '2023-04-08', ult_int: { data: '2026-05-15', cat: 'email', autor: 'Ricardo Alves' }, fat_12m:   95000, oportunidades: 0, exige_visita: false, obs: 'Rebanho 3k · frota mínima' },
  { id: 84587, razao: 'Agropec. Cabeceira do Rio',                apelido: 'Cabeceira',         classe: 'B', cidade: 'Sapezal',           uf: 'MT', cnpj: '22.881.664/0001-79', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 4, cen: 'fernanda',ult_compra: '2024-11-25', ult_int: { data: '2026-08-05', cat: 'whatsapp', autor: 'Fernanda Melo' }, fat_12m:  640000, oportunidades: 0, exige_visita: false, obs: '20 dias · dentro meta' },
  { id: 84593, razao: 'Sitio Alto Bela Vista',                    apelido: 'Alto Bela Vista',   classe: 'D', cidade: 'Nova Mutum',        uf: 'MT', cnpj: '30.114.885/0001-62', segmento: 'Pecuária', porte: 'Pequeno', n_equipamentos: 0, cen: 'ricardo', ult_compra: null,          ult_int: { data: '2026-04-08', cat: 'email', autor: 'Ricardo Alves' }, fat_12m:    0, oportunidades: 0, exige_visita: false, obs: 'Prospect · CNPJ ativo' },
  { id: 84599, razao: 'Fazenda Riacho Doce',                      apelido: 'Riacho Doce',       classe: 'B', cidade: 'Sorriso',           uf: 'MT', cnpj: '16.885.221/0001-97', segmento: 'Grãos', porte: 'Médio',     n_equipamentos: 5, cen: 'ana',     ult_compra: '2025-01-12', ult_int: { data: '2026-08-15', cat: 'visita', autor: 'Ana Paula' }, fat_12m:  780000, oportunidades: 0, exige_visita: false, obs: '10 dias · em dia' }
];

// Constrói lista unificada de clientes (CARTEIRA_CEN + CLIENTES_OUTRAS)
function buildClientesUnificado() {
  const doMTNorte = CARTEIRA_CEN.map(c => {
    const extra = CLIENTES_EXTRA[c.id] || {};
    return {
      id: c.id,
      razao: c.razao,
      apelido: c.apelido,
      cnpj: extra.cnpj || '—',
      classe: c.classe,
      cidade: c.cidade,
      uf: c.uf,
      segmento: extra.segmento || 'Grãos',
      porte: extra.porte || 'Médio',
      n_equipamentos: extra.n_equipamentos !== undefined ? extra.n_equipamentos : 0,
      cen: extra.cen || 'joao',
      fat_12m: c.fat_12m,
      oportunidades: c.oportunidades || 0,
      oportunidades_valor: c.oportunidades_valor || 0,
      ult_compra: extra.ult_compra,
      ult_int: c.ult_int,
      exige_visita: c.exige_visita,
      obs: c.obs
    };
  });
  return doMTNorte.concat(CLIENTES_OUTRAS);
}

// ============ Estado da tela ============
const CLIENTES_STATE = {
  persona: 'cen',    // cen | regional | nacional
  cen: 'joao',
  regional: 'MT Norte',
  filtros: {
    classe:   'todas',    // todas | A | B | C | D
    cidade:   'todas',
    status:   'todos',    // todos | em_dia | aviso | atraso | critico | nunca
    segmento: 'todos',    // todos | Grãos | Cana | Pecuária | Agroindústria
    porte:    'todos'     // todos | Pequeno | Médio | Grande | Corporate
  },
  busca: '',
  ordem: { campo: 'fat_12m', dir: 'desc' }
};

// Delega para statusCobertura já existente (mesma lógica usada em Cobertura de Carteira)
function calcularStatusCliente(cli) {
  return statusCobertura(cli);
}

function filtrarClientes() {
  const todos = buildClientesUnificado();
  return todos.filter(c => {
    // Persona
    if (CLIENTES_STATE.persona === 'cen' && c.cen !== CLIENTES_STATE.cen) return false;
    if (CLIENTES_STATE.persona === 'regional' && CENS[c.cen].regional !== CLIENTES_STATE.regional) return false;
    // (nacional = todos)

    const f = CLIENTES_STATE.filtros;
    if (f.classe !== 'todas' && c.classe !== f.classe) return false;
    if (f.cidade !== 'todas' && c.cidade !== f.cidade) return false;
    if (f.segmento !== 'todos' && c.segmento !== f.segmento) return false;
    if (f.porte !== 'todos' && c.porte !== f.porte) return false;
    if (f.status !== 'todos') {
      const s = calcularStatusCliente(c);
      if (s !== f.status) return false;
    }

    if (CLIENTES_STATE.busca) {
      const q = CLIENTES_STATE.busca.toLowerCase();
      const hay = `${c.razao} ${c.apelido} ${c.cnpj} ${c.cidade} ${c.id}`.toLowerCase();
      if (!hay.includes(q)) return false;
    }
    return true;
  });
}

function ordenarClientes(lista) {
  const { campo, dir } = CLIENTES_STATE.ordem;
  const mult = dir === 'asc' ? 1 : -1;
  return lista.sort((a, b) => {
    let va = a[campo], vb = b[campo];
    if (campo === 'ult_compra') {
      va = a.ult_compra || '1900-01-01';
      vb = b.ult_compra || '1900-01-01';
    }
    if (typeof va === 'string') return va.localeCompare(vb) * mult;
    return ((va || 0) - (vb || 0)) * mult;
  });
}

function calcularClientesKPIs(clientes) {
  const total = clientes.length;
  const ativos = clientes.filter(c => c.fat_12m > 0).length;
  const inativos = total - ativos;
  const fatTotal = clientes.reduce((s, c) => s + (c.fat_12m || 0), 0);
  const oppTotal = clientes.reduce((s, c) => s + (c.oportunidades_valor || 0), 0);
  const classeA = clientes.filter(c => c.classe === 'A').length;
  const frotaJD = clientes.reduce((s, c) => s + (c.n_equipamentos || 0), 0);
  return { total, ativos, inativos, fatTotal, oppTotal, classeA, frotaJD };
}

function renderClientesLista() {
  return `
    <div class="clientes-header">
      <div class="clientes-header-top">
        <div style="flex:1;min-width:0">
          <div class="clientes-title">Clientes</div>
          <div class="clientes-subtitle">Base de contas comerciais · perfil, frota e oportunidades</div>
        </div>
        <div class="clientes-persona-switcher">
          <div class="persona-tabs">
            <button class="persona-tab active" data-c-persona="cen">Minha carteira</button>
            <button class="persona-tab" data-c-persona="regional">Regional MT Norte</button>
            <button class="persona-tab" data-c-persona="nacional">Nacional</button>
          </div>
          <div class="persona-badge" id="cPersonaBadge">
            <span class="persona-dot"></span><strong>Você é João Ribeiro</strong><span class="persona-hint">· CEN MT Norte</span>
          </div>
        </div>
      </div>
    </div>

    <div class="clientes-kpis" id="clientesKpis"></div>

    <div class="clientes-toolbar">
      <div class="clientes-search">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
        <input type="text" id="clientesBusca" placeholder="Buscar razão, CNPJ, apelido, cidade, ID...">
      </div>
      <div class="clientes-toolbar-actions">
        <button class="btn-secondary" id="clientesLimpar">Limpar filtros</button>
        <button class="btn-secondary" id="clientesExportar">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          Exportar CSV
        </button>
      </div>
    </div>

    <div class="clientes-filtros" id="clientesFiltros">
      <!-- chips renderizados por mount -->
    </div>

    <div class="clientes-tabela-wrap">
      <table class="clientes-tabela">
        <thead>
          <tr>
            <th class="th-sortable" data-sort="razao">Cliente</th>
            <th>CNPJ</th>
            <th data-sort="cidade" class="th-sortable">Cidade</th>
            <th data-sort="classe" class="th-sortable" style="text-align:center">Classe</th>
            <th data-sort="segmento" class="th-sortable">Segmento</th>
            <th data-sort="porte" class="th-sortable">Porte</th>
            <th data-sort="n_equipamentos" class="th-sortable num" style="text-align:right">Frota JD</th>
            <th data-sort="fat_12m" class="th-sortable num" style="text-align:right">Fat. 12m</th>
            <th data-sort="oportunidades_valor" class="th-sortable num" style="text-align:right">Oport.</th>
            <th data-sort="ult_compra" class="th-sortable">Últ. compra</th>
            <th>Cobertura</th>
            <th style="width:60px"></th>
          </tr>
        </thead>
        <tbody id="clientesTbody">
          <!-- linhas geradas por mount -->
        </tbody>
      </table>
      <div id="clientesEmpty" class="clientes-empty" style="display:none">
        <div class="empty-icon">📭</div>
        <div class="empty-title">Nenhum cliente encontrado</div>
        <div class="empty-hint">Ajuste os filtros ou a busca</div>
      </div>
    </div>
  `;
}

function mountClientesLista() {
  renderClientesFiltrosChips();
  renderClientesTudo();
  atualizarCPersonaBadge();

  // Switcher
  document.querySelectorAll('[data-c-persona]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-c-persona]').forEach(b => b.classList.toggle('active', b === btn));
      CLIENTES_STATE.persona = btn.dataset.cPersona;
      atualizarCPersonaBadge();
      renderClientesFiltrosChips();  // re-renderiza cidades (base muda)
      renderClientesTudo();
    });
  });

  // Busca
  document.getElementById('clientesBusca').addEventListener('input', (e) => {
    CLIENTES_STATE.busca = e.target.value;
    renderClientesTudo();
  });

  // Limpar
  document.getElementById('clientesLimpar').addEventListener('click', () => {
    Object.keys(CLIENTES_STATE.filtros).forEach(k => {
      CLIENTES_STATE.filtros[k] = k === 'classe' || k === 'cidade' ? 'todas' : 'todos';
    });
    CLIENTES_STATE.busca = '';
    document.getElementById('clientesBusca').value = '';
    renderClientesFiltrosChips();
    renderClientesTudo();
  });

  // Exportar (fake)
  document.getElementById('clientesExportar').addEventListener('click', () => {
    const lista = filtrarClientes();
    alert(`Exportar ${lista.length} clientes selecionados como CSV\n\n(Demo · em produção precisa RBAC + trilha de auditoria de export)`);
  });

  // Sort
  document.querySelectorAll('.th-sortable').forEach(th => {
    th.addEventListener('click', () => {
      const campo = th.dataset.sort;
      if (CLIENTES_STATE.ordem.campo === campo) {
        CLIENTES_STATE.ordem.dir = CLIENTES_STATE.ordem.dir === 'asc' ? 'desc' : 'asc';
      } else {
        CLIENTES_STATE.ordem.campo = campo;
        CLIENTES_STATE.ordem.dir = 'desc';
      }
      renderClientesTudo();
    });
  });
}

function atualizarCPersonaBadge() {
  const badge = document.getElementById('cPersonaBadge');
  if (!badge) return;
  const p = CLIENTES_STATE.persona;
  if (p === 'cen')       badge.innerHTML = `<span class="persona-dot"></span><strong>Você é João Ribeiro</strong><span class="persona-hint">· CEN MT Norte · vê só seus clientes</span>`;
  else if (p === 'regional') badge.innerHTML = `<span class="persona-dot"></span><strong>Modo Gerente Regional MT Norte</strong><span class="persona-hint">· 5 CENs · base consolidada</span>`;
  else                   badge.innerHTML = `<span class="persona-dot warn"></span><strong>Modo Diretor Comercial</strong><span class="persona-hint">· visão nacional · exige RBAC real</span>`;
}

function renderClientesFiltrosChips() {
  const el = document.getElementById('clientesFiltros');
  if (!el) return;

  // Cidades disponíveis no escopo atual
  const escopo = buildClientesUnificado().filter(c => {
    if (CLIENTES_STATE.persona === 'cen' && c.cen !== CLIENTES_STATE.cen) return false;
    if (CLIENTES_STATE.persona === 'regional' && CENS[c.cen].regional !== CLIENTES_STATE.regional) return false;
    return true;
  });
  const cidades = [...new Set(escopo.map(c => c.cidade))].sort();

  const f = CLIENTES_STATE.filtros;
  el.innerHTML = `
    <div class="filtro-grupo">
      <label>Classe:</label>
      ${['todas','A','B','C','D'].map(v => `
        <button class="chip ${f.classe === v ? 'active' : ''}" data-filtro="classe" data-valor="${v}">${v === 'todas' ? 'Todas' : v}</button>
      `).join('')}
    </div>
    <div class="filtro-grupo">
      <label>Status cobertura:</label>
      ${[['todos','Todos'],['em_dia','Em dia'],['aviso','Aviso'],['atraso','Atraso'],['critico','Crítico'],['nunca','Nunca']].map(([v,l]) => `
        <button class="chip chip-status-${v} ${f.status === v ? 'active' : ''}" data-filtro="status" data-valor="${v}">${l}</button>
      `).join('')}
    </div>
    <div class="filtro-grupo">
      <label>Segmento:</label>
      ${['todos','Grãos','Cana','Pecuária','Agroindústria'].map(v => `
        <button class="chip ${f.segmento === v ? 'active' : ''}" data-filtro="segmento" data-valor="${v}">${v === 'todos' ? 'Todos' : v}</button>
      `).join('')}
    </div>
    <div class="filtro-grupo">
      <label>Porte:</label>
      ${['todos','Pequeno','Médio','Grande','Corporate'].map(v => `
        <button class="chip ${f.porte === v ? 'active' : ''}" data-filtro="porte" data-valor="${v}">${v === 'todos' ? 'Todos' : v}</button>
      `).join('')}
    </div>
    <div class="filtro-grupo">
      <label>Cidade:</label>
      <select id="clientesFiltroCidade" class="chip-select">
        <option value="todas">Todas (${cidades.length})</option>
        ${cidades.map(c => `<option value="${c}" ${f.cidade === c ? 'selected' : ''}>${c}</option>`).join('')}
      </select>
    </div>
  `;

  // Wire chips
  el.querySelectorAll('.chip').forEach(chip => {
    chip.addEventListener('click', () => {
      const filtro = chip.dataset.filtro;
      const valor = chip.dataset.valor;
      CLIENTES_STATE.filtros[filtro] = valor;
      renderClientesFiltrosChips();
      renderClientesTudo();
    });
  });
  document.getElementById('clientesFiltroCidade').addEventListener('change', (e) => {
    CLIENTES_STATE.filtros.cidade = e.target.value;
    renderClientesTudo();
  });
}

function renderClientesTudo() {
  const lista = ordenarClientes(filtrarClientes());
  renderClientesKPIs(lista);
  renderClientesTabela(lista);
}

function renderClientesKPIs(lista) {
  const k = calcularClientesKPIs(lista);
  const el = document.getElementById('clientesKpis');
  if (!el) return;
  el.innerHTML = `
    <div class="clientes-kpi">
      <div class="kpi-label">Clientes na visão</div>
      <div class="kpi-value">${k.total}</div>
      <div class="kpi-hint">${k.ativos} ativos · ${k.inativos} inativos</div>
    </div>
    <div class="clientes-kpi accent">
      <div class="kpi-label">Classe A</div>
      <div class="kpi-value">${k.classeA}</div>
      <div class="kpi-hint">${k.total > 0 ? Math.round(k.classeA/k.total*100) : 0}% da visão</div>
    </div>
    <div class="clientes-kpi">
      <div class="kpi-label">Fat. 12m consolidado</div>
      <div class="kpi-value">${fmtBRLcompact(k.fatTotal)}</div>
      <div class="kpi-hint">Soma da base filtrada</div>
    </div>
    <div class="clientes-kpi good">
      <div class="kpi-label">Pipeline aberto</div>
      <div class="kpi-value">${fmtBRLcompact(k.oppTotal)}</div>
      <div class="kpi-hint">Oport. em andamento</div>
    </div>
    <div class="clientes-kpi">
      <div class="kpi-label">Frota JD instalada</div>
      <div class="kpi-value">${k.frotaJD}</div>
      <div class="kpi-hint">Equipamentos ativos no Protheus</div>
    </div>
  `;
}

function statusPill(s) {
  const map = {
    em_dia:  { l: 'Em dia',  c: '#22C55E' },
    aviso:   { l: 'Aviso',   c: '#F59E0B' },
    atraso:  { l: 'Atraso',  c: '#EF4444' },
    critico: { l: 'Crítico', c: '#991B1B' },
    nunca:   { l: 'Nunca contatado', c: '#94A3B8' }
  };
  const cfg = map[s] || map.nunca;
  return `<span class="status-pill" style="background:${cfg.c}20;color:${cfg.c};border-color:${cfg.c}40">
    <span class="status-dot" style="background:${cfg.c}"></span>${cfg.l}
  </span>`;
}

function renderClientesTabela(lista) {
  const tbody = document.getElementById('clientesTbody');
  const empty = document.getElementById('clientesEmpty');
  if (!tbody) return;

  if (lista.length === 0) {
    tbody.innerHTML = '';
    empty.style.display = 'flex';
    return;
  }
  empty.style.display = 'none';

  tbody.innerHTML = lista.map(c => {
    const status = calcularStatusCliente(c);
    const cen = CENS[c.cen];
    const ultCompra = c.ult_compra
      ? new Date(c.ult_compra).toLocaleDateString('pt-BR')
      : '<span class="muted">Sem compras</span>';
    const linkFicha = c.id === 84391 ? `#/clientes/84391` : null;

    return `
      <tr data-cli-id="${c.id}" class="cliente-row">
        <td class="cel-cliente">
          <div class="cli-razao">
            ${linkFicha ? `<a href="${linkFicha}">${c.razao}</a>` : c.razao}
          </div>
          <div class="cli-id">#${c.id} · ${c.apelido}</div>
        </td>
        <td class="cel-cnpj">${c.cnpj}</td>
        <td>${c.cidade}<span class="muted">/${c.uf}</span></td>
        <td style="text-align:center"><span class="classe-badge classe-${c.classe.toLowerCase()}">${c.classe}</span></td>
        <td>${c.segmento}</td>
        <td>${c.porte}</td>
        <td class="num">${c.n_equipamentos === 0 ? '<span class="muted">—</span>' : c.n_equipamentos}</td>
        <td class="num">${c.fat_12m > 0 ? fmtBRLcompact(c.fat_12m) : '<span class="muted">—</span>'}</td>
        <td class="num">${c.oportunidades > 0
            ? `<div class="opp-cell"><strong>${c.oportunidades}</strong> · ${fmtBRLcompact(c.oportunidades_valor || 0)}</div>`
            : '<span class="muted">—</span>'}</td>
        <td>${ultCompra}</td>
        <td>${statusPill(status)}</td>
        <td class="cel-acoes">
          <button class="btn-linha" title="Abrir ficha" onclick="location.hash='${linkFicha || '#/clientes/84391'}'">
            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 18l6-6-6-6"/></svg>
          </button>
        </td>
      </tr>
    `;
  }).join('');
}

// ===== CONFIGURAÇÕES =====

const CONFIG_STATE = {
  aba: 'preferencias',      // preferencias | comercial | ti
  secao: 'perfil'           // depende da aba
};

// ===== Estruturas de dados =====
const CONFIG_METAS = {
  frequencia_visita: { A: 30, B: 60, C: 90, D: 120 },
  frequencia_ligacao: { A: 15, B: 30, C: 45, D: 60 },
  sla_aprovacao_diretor: 48,        // horas
  sla_aprovacao_gerente: 24,
  sla_aprovacao_financeiro: 12,
  desconto_ate_gerente: 5,          // %
  desconto_ate_diretor: 10,
  desconto_ate_presidente: 20
};

const CONFIG_MOTIVOS_PERDA = [
  { id: 1, motivo: 'Preço acima do concorrente',      ativo: true,  usos_90d: 27 },
  { id: 2, motivo: 'Prazo de entrega longo',          ativo: true,  usos_90d: 12 },
  { id: 3, motivo: 'Cliente adiou para safra seguinte', ativo: true, usos_90d: 34 },
  { id: 4, motivo: 'Concorrente Case IH',             ativo: true,  usos_90d: 18 },
  { id: 5, motivo: 'Concorrente New Holland',         ativo: true,  usos_90d: 9 },
  { id: 6, motivo: 'Cliente não obteve financiamento', ativo: true, usos_90d: 21 },
  { id: 7, motivo: 'Especificação técnica não atendida', ativo: true, usos_90d: 4 },
  { id: 8, motivo: 'Outro',                            ativo: true, usos_90d: 8 }
];

const CONFIG_CATEGORIAS_INTERACAO = [
  { id: 1, nome: 'Visita presencial',   vale_cobertura: true,  ativo: true },
  { id: 2, nome: 'Ligação',             vale_cobertura: true,  ativo: true },
  { id: 3, nome: 'WhatsApp',            vale_cobertura: true,  ativo: true },
  { id: 4, nome: 'E-mail',              vale_cobertura: false, ativo: true },
  { id: 5, nome: 'Reunião remota',      vale_cobertura: true,  ativo: true },
  { id: 6, nome: 'Feira/Evento',        vale_cobertura: false, ativo: true }
];

const CONFIG_INTEGRACOES = [
  {
    nome: 'TOTVS Protheus',
    tipo: 'ERP',
    icone: '🏭',
    status: 'online',
    ambiente: 'PRODUÇÃO · ORACLE',
    endpoint: 'https://protheus-app.tracbel.local:8181',
    last_sync: '2026-08-25 18:32',
    frequencia: 'A cada 15 min',
    donos: 'TI · Hugo Rocha',
    notas: 'Sync de clientes SA1, oportunidades SC5/SC6, faturamento SD2'
  },
  {
    nome: 'Microsoft Entra ID (AD)',
    tipo: 'Identidade',
    icone: '🔐',
    status: 'online',
    ambiente: 'Tenant tracbelagro.onmicrosoft.com',
    endpoint: 'graph.microsoft.com/v1.0',
    last_sync: '2026-08-25 18:30',
    frequencia: 'Login em tempo real · SCIM diário 03:00',
    donos: 'TI · Hugo Rocha',
    notas: 'SSO federado · MFA obrigatório para roles Gerente+'
  },
  {
    nome: 'Microsoft 365 (E-mail e Calendário)',
    tipo: 'Produtividade',
    icone: '📧',
    status: 'online',
    ambiente: 'Exchange Online',
    endpoint: 'graph.microsoft.com/v1.0/me/messages',
    last_sync: 'Em tempo real (webhook)',
    frequencia: 'Push',
    donos: 'TI · Hugo Rocha',
    notas: 'Registra e-mails com clientes automaticamente via subject-line regex'
  },
  {
    nome: 'John Deere Operations Center',
    tipo: 'Telemetria',
    icone: '🚜',
    status: 'pendente',
    ambiente: 'API JD MyJohnDeere',
    endpoint: 'partnerapi.deere.com/platform/organizations',
    last_sync: null,
    frequencia: 'Diário 04:00',
    donos: 'TI · Hugo Rocha · aguardando contrato JD',
    notas: 'Habilitará horas trabalhadas por chassi e alertas de manutenção'
  },
  {
    nome: 'GLPI · Service Desk',
    tipo: 'Suporte',
    icone: '🎫',
    status: 'online',
    ambiente: 'GLPI 10 · MySQL',
    endpoint: 'glpi.tracbel.local/apirest.php',
    last_sync: '2026-08-25 18:20',
    frequencia: 'A cada 30 min',
    donos: 'TI · Hugo Rocha',
    notas: 'Recebe abertura de chamado de erro do CRM automaticamente'
  },
  {
    nome: 'WhatsApp Business API',
    tipo: 'Comunicação',
    icone: '💬',
    status: 'degradado',
    ambiente: 'Meta Cloud API',
    endpoint: 'graph.facebook.com/v18.0',
    last_sync: '2026-08-25 15:40',
    frequencia: 'Push',
    donos: 'Marketing + TI',
    notas: 'Rate limit alcançado 15:40 · 2h de espera para reset'
  },
  {
    nome: 'Vortice Rev/CRM legado',
    tipo: 'Histórico',
    icone: '📦',
    status: 'somente_leitura',
    ambiente: 'Base MSSQL congelada',
    endpoint: 'sqlvortice.tracbel.local',
    last_sync: '2025-06-30 (congelado)',
    frequencia: 'N/A',
    donos: 'TI · Hugo Rocha',
    notas: 'Somente-leitura para histórico anterior ao CRM novo'
  }
];

const CONFIG_USUARIOS = [
  { id: 1, nome: 'Hugo Rocha',       email: 'hugo.rocha@tracbel.com.br',    role: 'Admin TI',           regional: '—',        ultimo_login: '2026-08-25 18:30' },
  { id: 2, nome: 'Rafael Menezes',   email: 'rafael.menezes@tracbel.com.br', role: 'Diretor Comercial',  regional: 'Nacional', ultimo_login: '2026-08-25 17:45' },
  { id: 3, nome: 'Cláudia Batista',  email: 'claudia.batista@tracbel.com.br', role: 'Gerente Regional',   regional: 'MT Norte', ultimo_login: '2026-08-25 18:12' },
  { id: 4, nome: 'João Ribeiro',     email: 'joao.ribeiro@tracbel.com.br',   role: 'CEN',                regional: 'MT Norte', ultimo_login: '2026-08-25 18:35' },
  { id: 5, nome: 'José Rufino',      email: 'jose.rufino@tracbel.com.br',    role: 'CEN',                regional: 'MT Norte', ultimo_login: '2026-08-25 16:22' },
  { id: 6, nome: 'Ana Paula Silveira', email: 'ana.silveira@tracbel.com.br', role: 'CEN',                regional: 'MT Norte', ultimo_login: '2026-08-25 12:08' },
  { id: 7, nome: 'Ricardo Alves',    email: 'ricardo.alves@tracbel.com.br',  role: 'CEN',                regional: 'MT Norte', ultimo_login: '2026-08-24 19:00' },
  { id: 8, nome: 'Fernanda Melo',    email: 'fernanda.melo@tracbel.com.br',  role: 'CEN',                regional: 'MT Norte', ultimo_login: '2026-08-25 09:45' },
  { id: 9, nome: 'Marcelo Silva',    email: 'marcelo.silva@tracbel.com.br',  role: 'CEN',                regional: 'MT Sul',   ultimo_login: '2026-08-25 17:20' },
  { id: 10, nome: 'Renata Costa',    email: 'renata.costa@tracbel.com.br',   role: 'CEN',                regional: 'GO',       ultimo_login: '2026-08-25 15:55' },
  { id: 11, nome: 'Pedro Almeida',   email: 'pedro.almeida@tracbel.com.br',  role: 'CEN',                regional: 'BA',       ultimo_login: '2026-08-25 14:30' },
  { id: 12, nome: 'Marina Costa',    email: 'marina.costa@tracbel.com.br',   role: 'Admin Comercial',    regional: 'MT Norte', ultimo_login: '2026-08-25 18:00' }
];

const CONFIG_ROLES = [
  { role: 'Admin TI',           usuarios: 1,  descricao: 'Acesso total · configura integrações, permissões, taxonomias e ambiente' },
  { role: 'Diretor Comercial',  usuarios: 1,  descricao: 'Vê pipeline nacional · aprova descontos acima de 10% · define metas comerciais' },
  { role: 'Gerente Regional',   usuarios: 1,  descricao: 'Vê pipeline da regional · aprova descontos até 10% · redistribui carteiras' },
  { role: 'CEN',                usuarios: 8,  descricao: 'Vê apenas própria carteira e pipeline · registra contatos e oportunidades' },
  { role: 'Admin Comercial',    usuarios: 1,  descricao: 'Suporte administrativo · edita cadastros e taxonomias sem aprovar descontos' }
];

// ============ Render principal ============
function renderConfig() {
  return `
    <div class="config-header">
      <div>
        <div class="config-title">Configurações</div>
        <div class="config-subtitle">Administração do CRM · integrações · usuários e políticas comerciais</div>
      </div>
      <div class="config-user-chip">
        <div class="cu-avatar" style="background:#367C2B">HR</div>
        <div>
          <div class="cu-nome">Hugo Rocha</div>
          <div class="cu-role">Admin TI · Tracbel Agro</div>
        </div>
      </div>
    </div>

    <div class="config-abas">
      <button class="config-aba active" data-config-aba="preferencias">
        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
        Minhas preferências
      </button>
      <button class="config-aba" data-config-aba="comercial">
        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
        Comercial
        <span class="aba-lock" title="Gerente Comercial ou superior">🔒</span>
      </button>
      <button class="config-aba" data-config-aba="ti">
        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="3" width="20" height="14" rx="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>
        TI e Integrações
        <span class="aba-lock" title="Admin TI">🔒</span>
      </button>
    </div>

    <div class="config-body">
      <aside class="config-sidebar">
        <div id="configMenu"></div>
      </aside>
      <section class="config-content" id="configContent">
        <!-- Renderizado por mount -->
      </section>
    </div>
  `;
}

function mountConfig() {
  renderConfigMenu();
  renderConfigSecao();

  document.querySelectorAll('[data-config-aba]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-config-aba]').forEach(b => b.classList.toggle('active', b === btn));
      CONFIG_STATE.aba = btn.dataset.configAba;
      // Ajusta seção default por aba
      const defaults = { preferencias: 'perfil', comercial: 'metas', ti: 'integracoes' };
      CONFIG_STATE.secao = defaults[CONFIG_STATE.aba];
      renderConfigMenu();
      renderConfigSecao();
    });
  });
}

function renderConfigMenu() {
  const menus = {
    preferencias: [
      { id: 'perfil',        icon: '👤', label: 'Perfil e conta' },
      { id: 'notificacoes',  icon: '🔔', label: 'Notificações' },
      { id: 'atalhos',       icon: '⌨️', label: 'Atalhos e produtividade' }
    ],
    comercial: [
      { id: 'metas',         icon: '🎯', label: 'Metas e SLA' },
      { id: 'aprovacoes',    icon: '✅', label: 'Políticas de aprovação' },
      { id: 'taxonomias',    icon: '🏷️', label: 'Taxonomias' }
    ],
    ti: [
      { id: 'integracoes',   icon: '🔌', label: 'Integrações' },
      { id: 'usuarios',      icon: '👥', label: 'Usuários' },
      { id: 'permissoes',    icon: '🛡️', label: 'Permissões e roles' },
      { id: 'auditoria',     icon: '📋', label: 'Auditoria e logs' }
    ]
  };

  const menu = menus[CONFIG_STATE.aba];
  const el = document.getElementById('configMenu');
  el.innerHTML = menu.map(item => `
    <button class="config-menu-item ${CONFIG_STATE.secao === item.id ? 'active' : ''}" data-secao="${item.id}">
      <span class="cmi-icon">${item.icon}</span>
      <span>${item.label}</span>
    </button>
  `).join('');

  el.querySelectorAll('[data-secao]').forEach(btn => {
    btn.addEventListener('click', () => {
      CONFIG_STATE.secao = btn.dataset.secao;
      renderConfigMenu();
      renderConfigSecao();
    });
  });
}

function renderConfigSecao() {
  const s = CONFIG_STATE.secao;
  const renderers = {
    perfil: renderSecaoPerfil,
    notificacoes: renderSecaoNotificacoes,
    atalhos: renderSecaoAtalhos,
    metas: renderSecaoMetas,
    aprovacoes: renderSecaoAprovacoes,
    taxonomias: renderSecaoTaxonomias,
    integracoes: renderSecaoIntegracoes,
    usuarios: renderSecaoUsuarios,
    permissoes: renderSecaoPermissoes,
    auditoria: renderSecaoAuditoria
  };
  const el = document.getElementById('configContent');
  el.innerHTML = (renderers[s] || (() => '<div>Seção em construção</div>'))();

  // Wire de salvar (toast)
  el.querySelectorAll('.btn-config-salvar').forEach(btn => {
    btn.addEventListener('click', () => configToast('Alterações salvas · em produção grava no Protheus/AD'));
  });
  el.querySelectorAll('.btn-config-cancelar').forEach(btn => {
    btn.addEventListener('click', () => configToast('Alterações descartadas', 'warn'));
  });
}

// ============ Seções: MINHAS PREFERÊNCIAS ============
function renderSecaoPerfil() {
  return `
    ${cardConfig('Perfil e conta', `
      <div class="form-grid">
        <div class="form-field">
          <label>Nome completo</label>
          <input type="text" value="Hugo Rocha" disabled>
          <span class="field-hint">Sincronizado com AD · não editável aqui</span>
        </div>
        <div class="form-field">
          <label>E-mail corporativo</label>
          <input type="text" value="hugo.rocha@tracbel.com.br" disabled>
          <span class="field-hint">Login SSO Microsoft</span>
        </div>
        <div class="form-field">
          <label>Cargo</label>
          <input type="text" value="Gerente de TI" disabled>
        </div>
        <div class="form-field">
          <label>Regional</label>
          <input type="text" value="Nacional (TI)" disabled>
        </div>
        <div class="form-field">
          <label>Telefone celular <span class="pill-editavel">editável</span></label>
          <input type="text" value="(66) 99887-4321">
        </div>
        <div class="form-field">
          <label>Foto de perfil</label>
          <div class="foto-perfil-row">
            <div class="foto-atual" style="background:#367C2B">HR</div>
            <button class="btn-config-inline">Trocar</button>
          </div>
        </div>
      </div>
    `)}

    ${cardConfig('Sessão e segurança', `
      <div class="config-list">
        <div class="config-list-row">
          <div>
            <div class="clr-title">Autenticação em duas etapas</div>
            <div class="clr-desc">Obrigatório pela política de TI · via Microsoft Authenticator</div>
          </div>
          <span class="badge-status ok">Ativa</span>
        </div>
        <div class="config-list-row">
          <div>
            <div class="clr-title">Sessão ativa em outros dispositivos</div>
            <div class="clr-desc">2 sessões · Windows Chrome e iOS Edge</div>
          </div>
          <button class="btn-config-inline danger">Encerrar todas</button>
        </div>
        <div class="config-list-row">
          <div>
            <div class="clr-title">Último login</div>
            <div class="clr-desc">Hoje, 18:30 · Chrome 128 · IP interno 10.42.18.221</div>
          </div>
        </div>
      </div>
    `)}

    ${footerAcoes()}
  `;
}

function renderSecaoNotificacoes() {
  return `
    ${cardConfig('Canais preferidos', `
      <div class="config-list">
        ${toggleRow('Notificações no navegador', 'Alertas de aprovação, tarefa vencendo e menções', true)}
        ${toggleRow('E-mail para itens críticos', 'Aprovações travadas > 24h, oportunidades acima de R$ 3M', true)}
        ${toggleRow('Resumo diário por e-mail', 'Enviado às 07:00 com pipeline, tarefas e alertas', false)}
        ${toggleRow('WhatsApp para aprovações', 'Requer número validado com Meta Business', false)}
      </div>
    `)}

    ${cardConfig('Regras específicas', `
      <div class="form-grid">
        <div class="form-field">
          <label>Alertar quando oportunidade fica parada por</label>
          <select><option>7 dias</option><option selected>14 dias</option><option>30 dias</option></select>
        </div>
        <div class="form-field">
          <label>Cliente sem contato há mais de</label>
          <select><option>Padrão da classe</option><option selected>7 dias além da meta</option><option>15 dias além da meta</option></select>
        </div>
        <div class="form-field">
          <label>Silenciar notificações</label>
          <select><option selected>Fim de semana e feriados</option><option>Nunca</option><option>Após 19h</option></select>
        </div>
      </div>
    `)}

    ${footerAcoes()}
  `;
}

function renderSecaoAtalhos() {
  const atalhos = [
    ['Abrir busca global', 'Ctrl+K'],
    ['Nova oportunidade', 'Ctrl+Shift+O'],
    ['Registrar visita', 'Ctrl+Shift+V'],
    ['Ir para Agenda', 'G A'],
    ['Ir para Pipeline', 'G P'],
    ['Ir para Clientes', 'G C']
  ];
  return `
    ${cardConfig('Atalhos de teclado', `
      <div class="atalhos-grid">
        ${atalhos.map(([acao, teclas]) => `
          <div class="atalho-row">
            <span class="atalho-acao">${acao}</span>
            <span class="atalho-teclas">${teclas.split(' ').map(t => `<kbd>${t}</kbd>`).join(' ')}</span>
          </div>
        `).join('')}
      </div>
    `)}

    ${cardConfig('Assinatura de e-mail (registro de contato)', `
      <div class="form-field">
        <label>Rodapé automático em contatos enviados pelo CRM</label>
        <textarea rows="4">Hugo Rocha
Gerente de TI · Tracbel Agro
hugo.rocha@tracbel.com.br · (66) 99887-4321
João Deere Brasil · concessionária MT/GO/BA</textarea>
      </div>
    `)}

    ${footerAcoes()}
  `;
}

// ============ Seções: COMERCIAL ============
function renderSecaoMetas() {
  const m = CONFIG_METAS;
  return `
    ${cardConfig('Frequência de visita presencial (dias · meta máxima)', `
      <div class="form-grid cols-4">
        ${['A','B','C','D'].map(c => `
          <div class="form-field">
            <label>Classe <strong>${c}</strong></label>
            <div class="input-suffix">
              <input type="number" value="${m.frequencia_visita[c]}">
              <span>dias</span>
            </div>
          </div>
        `).join('')}
      </div>
      <div class="config-hint">
        Cliente Classe A sem visita há mais que 30 dias vira "atraso" no dashboard de cobertura.
      </div>
    `)}

    ${cardConfig('Frequência de contato remoto (ligação/WhatsApp/e-mail)', `
      <div class="form-grid cols-4">
        ${['A','B','C','D'].map(c => `
          <div class="form-field">
            <label>Classe <strong>${c}</strong></label>
            <div class="input-suffix">
              <input type="number" value="${m.frequencia_ligacao[c]}">
              <span>dias</span>
            </div>
          </div>
        `).join('')}
      </div>
    `)}

    ${cardConfig('SLA de aprovações (horas)', `
      <div class="form-grid cols-3">
        <div class="form-field">
          <label>Gerente Regional</label>
          <div class="input-suffix"><input type="number" value="${m.sla_aprovacao_gerente}"><span>h</span></div>
          <span class="field-hint">Depois disso escala para Diretor</span>
        </div>
        <div class="form-field">
          <label>Diretor Comercial</label>
          <div class="input-suffix"><input type="number" value="${m.sla_aprovacao_diretor}"><span>h</span></div>
          <span class="field-hint">Depois vira alerta vermelho</span>
        </div>
        <div class="form-field">
          <label>Financeiro (limite de crédito)</label>
          <div class="input-suffix"><input type="number" value="${m.sla_aprovacao_financeiro}"><span>h</span></div>
        </div>
      </div>
    `)}

    ${footerAcoes()}
  `;
}

function renderSecaoAprovacoes() {
  const m = CONFIG_METAS;
  return `
    ${cardConfig('Alçadas de desconto', `
      <div class="alcadas-visual">
        <div class="alcada-linha">
          <div class="alcada-role">CEN</div>
          <div class="alcada-range" style="width:${m.desconto_ate_gerente / m.desconto_ate_presidente * 100}%">
            <span>Até ${m.desconto_ate_gerente}%</span>
          </div>
        </div>
        <div class="alcada-linha">
          <div class="alcada-role">Gerente Regional</div>
          <div class="alcada-range warn" style="width:${m.desconto_ate_diretor / m.desconto_ate_presidente * 100}%">
            <span>Até ${m.desconto_ate_diretor}%</span>
          </div>
        </div>
        <div class="alcada-linha">
          <div class="alcada-role">Diretor Comercial</div>
          <div class="alcada-range danger" style="width:${m.desconto_ate_presidente / m.desconto_ate_presidente * 100}%">
            <span>Até ${m.desconto_ate_presidente}%</span>
          </div>
        </div>
        <div class="alcada-linha">
          <div class="alcada-role">Presidência</div>
          <div class="alcada-range dark">
            <span>Acima de ${m.desconto_ate_presidente}% · caso a caso</span>
          </div>
        </div>
      </div>

      <div class="form-grid cols-3" style="margin-top:20px">
        <div class="form-field">
          <label>Desconto até (Gerente aprova)</label>
          <div class="input-suffix"><input type="number" value="${m.desconto_ate_gerente}"><span>%</span></div>
        </div>
        <div class="form-field">
          <label>Desconto até (Diretor aprova)</label>
          <div class="input-suffix"><input type="number" value="${m.desconto_ate_diretor}"><span>%</span></div>
        </div>
        <div class="form-field">
          <label>Desconto até (Presidência aprova)</label>
          <div class="input-suffix"><input type="number" value="${m.desconto_ate_presidente}"><span>%</span></div>
        </div>
      </div>
    `)}

    ${cardConfig('Fluxo automático', `
      <div class="config-list">
        ${toggleRow('Escalar automaticamente após vencer SLA', 'Após 24h no Gerente sem resposta, envia ao Diretor', true)}
        ${toggleRow('Bloquear geração de proposta acima da alçada', 'Impede CEN de gerar PDF sem aprovação prévia', true)}
        ${toggleRow('Exigir justificativa obrigatória em descontos > 10%', 'Campo de texto livre com mínimo 30 caracteres', true)}
        ${toggleRow('Notificar Financeiro em oportunidades > R$ 5M', 'Antecipa análise de limite de crédito', false)}
      </div>
    `)}

    ${footerAcoes()}
  `;
}

function renderSecaoTaxonomias() {
  return `
    ${cardConfig('Motivos de perda de oportunidade', `
      <table class="config-tabela">
        <thead>
          <tr><th>#</th><th>Motivo</th><th style="text-align:right">Uso (90d)</th><th>Status</th><th></th></tr>
        </thead>
        <tbody>
          ${CONFIG_MOTIVOS_PERDA.map(m => `
            <tr>
              <td class="mono">${m.id}</td>
              <td>${m.motivo}</td>
              <td style="text-align:right" class="mono">${m.usos_90d}</td>
              <td>${m.ativo ? '<span class="badge-status ok">Ativo</span>' : '<span class="badge-status muted">Inativo</span>'}</td>
              <td><button class="btn-config-inline">Editar</button></td>
            </tr>
          `).join('')}
        </tbody>
      </table>
      <div style="margin-top:12px; display:flex; justify-content:space-between; align-items:center">
        <button class="btn-config-primary">+ Adicionar motivo</button>
        <span class="config-hint">Motivos aparecem como dropdown obrigatório ao marcar oportunidade como perdida.</span>
      </div>
    `)}

    ${cardConfig('Categorias de interação (contato com cliente)', `
      <table class="config-tabela">
        <thead>
          <tr><th>Categoria</th><th>Zera contador de cobertura</th><th>Status</th><th></th></tr>
        </thead>
        <tbody>
          ${CONFIG_CATEGORIAS_INTERACAO.map(c => `
            <tr>
              <td>${c.nome}</td>
              <td>${c.vale_cobertura ? '<span class="badge-status ok">Sim</span>' : '<span class="badge-status muted">Não</span>'}</td>
              <td>${c.ativo ? '<span class="badge-status ok">Ativo</span>' : '<span class="badge-status muted">Inativo</span>'}</td>
              <td><button class="btn-config-inline">Editar</button></td>
            </tr>
          `).join('')}
        </tbody>
      </table>
      <div class="config-hint" style="margin-top:12px">
        Cliente Classe A "exige visita" ignora categorias marcadas como "não zera contador" na análise de cobertura.
      </div>
    `)}

    ${footerAcoes()}
  `;
}

// ============ Seções: TI ============
function renderSecaoIntegracoes() {
  const statusPill = {
    online:          { l: 'Online',           cor: '#22C55E' },
    pendente:        { l: 'Pendente',         cor: '#F59E0B' },
    degradado:       { l: 'Degradado',        cor: '#EF4444' },
    somente_leitura: { l: 'Somente-leitura',  cor: '#94A3B8' },
    offline:         { l: 'Offline',          cor: '#991B1B' }
  };

  return `
    ${cardConfig('Sistemas conectados', `
      <div class="integracoes-lista">
        ${CONFIG_INTEGRACOES.map(i => {
          const s = statusPill[i.status] || statusPill.offline;
          return `
            <div class="integracao-item">
              <div class="int-icon">${i.icone}</div>
              <div class="int-info">
                <div class="int-header">
                  <strong>${i.nome}</strong>
                  <span class="int-tipo">${i.tipo}</span>
                  <span class="int-status" style="background:${s.cor}20;color:${s.cor};border-color:${s.cor}40">
                    <span class="int-dot" style="background:${s.cor}"></span>${s.l}
                  </span>
                </div>
                <div class="int-meta">
                  <div><span>Ambiente:</span> <span class="mono">${i.ambiente}</span></div>
                  <div><span>Endpoint:</span> <span class="mono">${i.endpoint}</span></div>
                  <div><span>Sync:</span> ${i.last_sync ? `<span class="mono">${i.last_sync}</span> · ${i.frequencia}` : `<span class="muted">Nunca sincronizado</span>`}</div>
                  <div><span>Responsável:</span> ${i.donos}</div>
                </div>
                <div class="int-notas">${i.notas}</div>
              </div>
              <div class="int-acoes">
                <button class="btn-config-inline">Testar</button>
                <button class="btn-config-inline">Logs</button>
              </div>
            </div>
          `;
        }).join('')}
      </div>
      <div style="margin-top:16px">
        <button class="btn-config-primary">+ Conectar novo sistema</button>
      </div>
    `)}

    ${footerAcoes()}
  `;
}

function renderSecaoUsuarios() {
  return `
    ${cardConfig('Usuários ativos', `
      <div class="config-filtros-inline">
        <input type="text" placeholder="Buscar por nome, e-mail...">
        <select>
          <option>Todos os roles</option>
          <option>Admin TI</option>
          <option>Diretor Comercial</option>
          <option>Gerente Regional</option>
          <option>CEN</option>
        </select>
        <select>
          <option>Todas as regionais</option>
          <option>MT Norte</option>
          <option>MT Sul</option>
          <option>GO</option>
          <option>BA</option>
        </select>
        <button class="btn-config-primary" style="margin-left:auto">+ Convidar usuário</button>
      </div>

      <table class="config-tabela">
        <thead>
          <tr><th>Nome</th><th>E-mail</th><th>Role</th><th>Regional</th><th>Último login</th><th></th></tr>
        </thead>
        <tbody>
          ${CONFIG_USUARIOS.map(u => `
            <tr>
              <td>${u.nome}</td>
              <td class="mono">${u.email}</td>
              <td><span class="role-pill role-${u.role.toLowerCase().replace(/[^a-z]/g,'')}">${u.role}</span></td>
              <td>${u.regional}</td>
              <td class="mono muted">${u.ultimo_login}</td>
              <td><button class="btn-config-inline">Editar</button></td>
            </tr>
          `).join('')}
        </tbody>
      </table>
      <div class="config-hint" style="margin-top:12px">
        Provisionamento automático via SCIM diário 03:00 (Microsoft Entra ID → CRM).
        Usuários criados manualmente aqui são flag "manual · exceção" na auditoria.
      </div>
    `)}
  `;
}

function renderSecaoPermissoes() {
  return `
    ${cardConfig('Roles e alçadas', `
      <table class="config-tabela">
        <thead>
          <tr><th>Role</th><th style="text-align:center">Usuários</th><th>Descrição</th><th></th></tr>
        </thead>
        <tbody>
          ${CONFIG_ROLES.map(r => `
            <tr>
              <td><span class="role-pill role-${r.role.toLowerCase().replace(/[^a-z]/g,'')}">${r.role}</span></td>
              <td style="text-align:center" class="mono">${r.usuarios}</td>
              <td>${r.descricao}</td>
              <td><button class="btn-config-inline">Ver permissões</button></td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `)}

    ${cardConfig('Matriz de permissões (resumo)', `
      <table class="config-tabela">
        <thead>
          <tr>
            <th>Ação</th>
            <th style="text-align:center">CEN</th>
            <th style="text-align:center">Gerente</th>
            <th style="text-align:center">Diretor</th>
            <th style="text-align:center">Admin TI</th>
          </tr>
        </thead>
        <tbody>
          ${[
            ['Ver própria carteira', '✓', '✓', '✓', '✓'],
            ['Ver carteira da regional', '—', '✓', '✓', '✓'],
            ['Ver carteira nacional', '—', '—', '✓', '✓'],
            ['Criar oportunidade', '✓', '✓', '✓', '—'],
            ['Aprovar desconto até 5%', '✓', '✓', '✓', '—'],
            ['Aprovar desconto até 10%', '—', '✓', '✓', '—'],
            ['Aprovar desconto até 20%', '—', '—', '✓', '—'],
            ['Reatribuir carteira', '—', '✓', '✓', '—'],
            ['Configurar integrações', '—', '—', '—', '✓'],
            ['Editar taxonomias', '—', '—', '✓', '✓']
          ].map(([acao, ...cells]) => `
            <tr>
              <td>${acao}</td>
              ${cells.map(c => `<td style="text-align:center; ${c === '✓' ? 'color:#22C55E; font-weight:700' : 'color:#94A3B8'}">${c}</td>`).join('')}
            </tr>
          `).join('')}
        </tbody>
      </table>
      <div class="config-hint" style="margin-top:12px">
        Permissões efetivas são calculadas na hora do login e cacheadas na sessão. Alterações aplicam no próximo login.
      </div>
    `)}
  `;
}

function renderSecaoAuditoria() {
  const eventos = [
    { data: '2026-08-25 18:32', usuario: 'Marina Costa · Admin Comercial', acao: 'Alterou motivo de perda #4 "Concorrente Case IH"', ip: '10.42.18.203' },
    { data: '2026-08-25 17:45', usuario: 'Rafael Menezes · Diretor',       acao: 'Aprovou desconto 12% · oportunidade OP-2026-08473',    ip: '10.42.18.7' },
    { data: '2026-08-25 15:40', usuario: 'Sistema · WhatsApp API',         acao: 'Rate limit atingido · integração marcada degradada',   ip: 'external' },
    { data: '2026-08-25 14:22', usuario: 'Hugo Rocha · Admin TI',          acao: 'Atualizou SLA aprovação Diretor de 36h para 48h',      ip: '10.42.18.221' },
    { data: '2026-08-25 11:08', usuario: 'Cláudia Batista · Gerente MT Norte', acao: 'Reatribuiu 3 clientes do CEN Ricardo para Fernanda', ip: '10.42.18.145' },
    { data: '2026-08-25 09:15', usuario: 'Sistema · Protheus sync',        acao: 'Sync noturno completo · 42 clientes atualizados',      ip: 'internal' },
    { data: '2026-08-24 19:40', usuario: 'João Ribeiro · CEN MT Norte',    acao: 'Exportou 24 clientes como CSV',                        ip: '10.42.18.98' },
    { data: '2026-08-24 16:22', usuario: 'Rafael Menezes · Diretor',       acao: 'Alterou desconto máximo Presidência de 25% para 20%',  ip: '10.42.18.7' }
  ];
  return `
    ${cardConfig('Eventos recentes', `
      <div class="config-filtros-inline">
        <input type="text" placeholder="Buscar por usuário, ação, IP...">
        <select>
          <option>Últimos 7 dias</option><option selected>Últimas 24h</option><option>Últimas 4h</option>
        </select>
        <select>
          <option>Todas as ações</option><option>Configuração</option><option>Aprovação</option><option>Integração</option><option>Exportação</option>
        </select>
        <button class="btn-config-inline" style="margin-left:auto">Exportar log</button>
      </div>

      <table class="config-tabela">
        <thead>
          <tr><th>Timestamp</th><th>Usuário</th><th>Ação</th><th>IP</th></tr>
        </thead>
        <tbody>
          ${eventos.map(e => `
            <tr>
              <td class="mono muted">${e.data}</td>
              <td>${e.usuario}</td>
              <td>${e.acao}</td>
              <td class="mono muted">${e.ip}</td>
            </tr>
          `).join('')}
        </tbody>
      </table>
      <div class="config-hint" style="margin-top:12px">
        Retenção de logs: 12 meses on-line · 24 meses arquivo. Exportação requer justificativa e é registrada.
      </div>
    `)}
  `;
}

// ============ Helpers ============
function cardConfig(titulo, conteudo) {
  return `
    <div class="config-card">
      <div class="cc-header">
        <h3>${titulo}</h3>
      </div>
      <div class="cc-body">
        ${conteudo}
      </div>
    </div>
  `;
}

function toggleRow(titulo, desc, ativo) {
  return `
    <div class="config-list-row">
      <div>
        <div class="clr-title">${titulo}</div>
        <div class="clr-desc">${desc}</div>
      </div>
      <label class="switch">
        <input type="checkbox" ${ativo ? 'checked' : ''}>
        <span class="slider"></span>
      </label>
    </div>
  `;
}

function footerAcoes() {
  return `
    <div class="config-footer-acoes">
      <button class="btn-config-cancelar">Cancelar</button>
      <button class="btn-config-salvar">Salvar alterações</button>
    </div>
  `;
}

function configToast(msg, tipo = 'ok') {
  const t = document.createElement('div');
  t.className = `config-toast toast-${tipo}`;
  t.textContent = msg;
  document.body.appendChild(t);
  setTimeout(() => t.classList.add('show'), 10);
  setTimeout(() => { t.classList.remove('show'); setTimeout(() => t.remove(), 300); }, 2800);
}

// ===== PERFORMANCE DE CEN =====
// Ano fiscal Tracbel: novembro a outubro. FY2026 = nov/2025 a out/2026.
// Hoje é 25/ago/2026 → FYTD abrange nov/25, dez/25, jan/26...ago/26 (ago parcial)

const PERF_STATE = {
  persona: 'cen',       // cen | regional | nacional (matriz de permissão)
  cenSelected: 'joao',  // usado no modo cen
  regional: 'MT Norte', // usado no modo regional/nacional filtrando
  metricaBreakdown: 'vendas_fyd'  // qual métrica é ranqueada nas barras horizontais
};

// FYTD = ano fiscal Tracbel novembro-outubro. Meses no mock:
const FYTD_MESES = [
  { key: '2025-11', label: 'Nov/25', dias_uteis: 20 },
  { key: '2025-12', label: 'Dez/25', dias_uteis: 18 },
  { key: '2026-01', label: 'Jan/26', dias_uteis: 21 },
  { key: '2026-02', label: 'Fev/26', dias_uteis: 19 },
  { key: '2026-03', label: 'Mar/26', dias_uteis: 22 },
  { key: '2026-04', label: 'Abr/26', dias_uteis: 20 },
  { key: '2026-05', label: 'Mai/26', dias_uteis: 21 },
  { key: '2026-06', label: 'Jun/26', dias_uteis: 20 },
  { key: '2026-07', label: 'Jul/26', dias_uteis: 22 },
  { key: '2026-08', label: 'Ago/26', dias_uteis: 17, parcial: true }  // hoje 25/08, 17 dias úteis passados
];

// Base de CENs Tracbel — 8 vendedores, 4 regionais
const PERF_CENS = [
  { id: 'joao',     nome: 'João Ribeiro',    regional: 'MT Norte', foto: 'JR', avatar: '#367C2B',
    meta_mes: 4200000, meta_fytd: 42000000, admissao: '2019-03-01' },
  { id: 'rufino',   nome: 'José Rufino',     regional: 'MT Norte', foto: 'JS', avatar: '#1B5E20',
    meta_mes: 4500000, meta_fytd: 45000000, admissao: '2016-08-15' },
  { id: 'ana',      nome: 'Ana Paula Silveira', regional: 'MT Norte', foto: 'AP', avatar: '#7C3AED',
    meta_mes: 4000000, meta_fytd: 40000000, admissao: '2021-05-10' },
  { id: 'ricardo',  nome: 'Ricardo Alves',   regional: 'MT Norte', foto: 'RA', avatar: '#0EA5E9',
    meta_mes: 3800000, meta_fytd: 38000000, admissao: '2018-11-22' },
  { id: 'fernanda', nome: 'Fernanda Melo',   regional: 'MT Norte', foto: 'FM', avatar: '#EC4899',
    meta_mes: 4200000, meta_fytd: 42000000, admissao: '2023-02-01' },  // mais nova
  { id: 'marcelo',  nome: 'Marcelo Silva',   regional: 'MT Sul',   foto: 'MS', avatar: '#F59E0B',
    meta_mes: 4500000, meta_fytd: 45000000, admissao: '2017-06-15' },
  { id: 'renata',   nome: 'Renata Costa',    regional: 'GO',       foto: 'RC', avatar: '#EF4444',
    meta_mes: 4300000, meta_fytd: 43000000, admissao: '2020-09-01' },
  { id: 'pedro',    nome: 'Pedro Almeida',   regional: 'BA',       foto: 'PA', avatar: '#0D9488',
    meta_mes: 4100000, meta_fytd: 41000000, admissao: '2019-12-05' }
];

// Perfil de performance de cada CEN por mês. Números plausíveis para uma concessionária JD.
// Métricas: vendas_fechadas (R$), pipeline_criado (R$), oport_ganhas, oport_perdidas, visitas, ticket_medio
function gerarSerieCEN(cenId, seed) {
  // Padrões distintos por CEN para tornar comparação realista:
  const PERFIL = {
    joao:     { base_vendas: 3600000, var: 0.25, trend: +0.03, cob: 82, conv: 0.32, ticket_base: 850000 },  // consistente, alta cobertura
    rufino:   { base_vendas: 4400000, var: 0.30, trend: +0.01, cob: 78, conv: 0.35, ticket_base: 1100000 }, // top performer, ticket alto
    ana:      { base_vendas: 2900000, var: 0.35, trend: +0.08, cob: 71, conv: 0.28, ticket_base: 780000 },  // crescendo, cob média
    ricardo:  { base_vendas: 3200000, var: 0.20, trend: -0.02, cob: 88, conv: 0.30, ticket_base: 720000 },  // estável mas caindo
    fernanda: { base_vendas: 2100000, var: 0.45, trend: +0.12, cob: 65, conv: 0.22, ticket_base: 620000 },  // júnior, subindo forte
    marcelo:  { base_vendas: 4000000, var: 0.22, trend: +0.02, cob: 79, conv: 0.33, ticket_base: 950000 },
    renata:   { base_vendas: 3800000, var: 0.28, trend: +0.04, cob: 74, conv: 0.31, ticket_base: 890000 },
    pedro:    { base_vendas: 3400000, var: 0.32, trend: -0.01, cob: 69, conv: 0.26, ticket_base: 780000 }
  };
  const p = PERFIL[cenId];
  const rng = mulberry32(seed);

  return FYTD_MESES.map((mes, i) => {
    const trend = 1 + (p.trend * i);
    const noise = 1 + ((rng() - 0.5) * p.var);
    const parcial = mes.parcial ? (mes.dias_uteis / 22) : 1;
    const vendas = Math.round(p.base_vendas * trend * noise * parcial);
    const oport_ganhas = Math.max(1, Math.round(vendas / p.ticket_base));
    const oport_totais = Math.round(oport_ganhas / p.conv);
    const oport_perdidas = oport_totais - oport_ganhas;
    const pipeline_criado = Math.round(vendas * (1.5 + rng() * 0.8));
    const visitas = Math.round((15 + rng() * 8) * parcial);
    const ticket_medio = Math.round(vendas / oport_ganhas);
    // Cobertura evolui suavemente
    const cob_variacao = ((rng() - 0.4) * 8);
    const cobertura = Math.max(45, Math.min(95, Math.round(p.cob + cob_variacao + i * 0.5)));
    return {
      mes: mes.key,
      label: mes.label,
      parcial: mes.parcial === true,
      vendas,
      pipeline_criado,
      oport_ganhas,
      oport_perdidas,
      oport_totais,
      conversao: oport_totais > 0 ? oport_ganhas / oport_totais : 0,
      visitas,
      ticket_medio,
      cobertura
    };
  });
}

function mulberry32(a) {
  return function() {
    let t = a += 0x6D2B79F5;
    t = Math.imul(t ^ t >>> 15, t | 1);
    t ^= t + Math.imul(t ^ t >>> 7, t | 61);
    return ((t ^ t >>> 14) >>> 0) / 4294967296;
  }
}

// Gera séries para todos os CENs
const PERF_SERIES = {};
PERF_CENS.forEach((c, i) => { PERF_SERIES[c.id] = gerarSerieCEN(c.id, (i + 1) * 12345); });

function calcularPerfCEN(cenId) {
  const serie = PERF_SERIES[cenId];
  const cen = PERF_CENS.find(c => c.id === cenId);
  const mesAtual = serie[serie.length - 1];
  const fytd_vendas = serie.reduce((s, m) => s + m.vendas, 0);
  const fytd_pipe = serie.reduce((s, m) => s + m.pipeline_criado, 0);
  const fytd_ganhas = serie.reduce((s, m) => s + m.oport_ganhas, 0);
  const fytd_totais = serie.reduce((s, m) => s + m.oport_totais, 0);
  const fytd_visitas = serie.reduce((s, m) => s + m.visitas, 0);
  const cob_atual = mesAtual.cobertura;
  const conv_fytd = fytd_totais > 0 ? fytd_ganhas / fytd_totais : 0;
  const ticket_fytd = fytd_ganhas > 0 ? fytd_vendas / fytd_ganhas : 0;

  // Pipeline aberto = pipeline criado - vendas ganhas (aproximação)
  let pipe_aberto = fytd_pipe - fytd_vendas;

  // Soma oportunidades criadas nesta sessão (só do CEN correto)
  const novasDoCEN = (window.OPORTUNIDADES_NOVAS || []).filter(op => op.cen === cen.id);
  const bumpNovas = novasDoCEN.reduce((s, op) => s + op.valor, 0);
  pipe_aberto += bumpNovas;

  return {
    cen,
    serie,
    mes_atual: mesAtual,
    mes_vendas: mesAtual.vendas,
    mes_meta: cen.meta_mes,
    mes_atingimento: mesAtual.vendas / cen.meta_mes,
    fytd_vendas,
    fytd_meta: cen.meta_fytd,
    fytd_atingimento: fytd_vendas / cen.meta_fytd,
    pipeline_aberto: pipe_aberto,
    conversao_fytd: conv_fytd,
    cobertura: cob_atual,
    ticket_medio_fytd: ticket_fytd,
    fytd_visitas,
    fytd_ganhas,
    fytd_totais
  };
}

function calcularPerfGrupo(cenIds) {
  const detalhes = cenIds.map(id => calcularPerfCEN(id));
  const soma = (fn) => detalhes.reduce((s, d) => s + fn(d), 0);
  const mes_vendas = soma(d => d.mes_vendas);
  const mes_meta = soma(d => d.mes_meta);
  const fytd_vendas = soma(d => d.fytd_vendas);
  const fytd_meta = soma(d => d.fytd_meta);
  const pipeline = soma(d => d.pipeline_aberto);
  const fytd_ganhas = soma(d => d.fytd_ganhas);
  const fytd_totais = soma(d => d.fytd_totais);
  const conversao = fytd_totais > 0 ? fytd_ganhas / fytd_totais : 0;
  const ticket = fytd_ganhas > 0 ? fytd_vendas / fytd_ganhas : 0;
  const cob = detalhes.reduce((s, d) => s + d.cobertura, 0) / detalhes.length;
  return {
    detalhes,
    mes_vendas, mes_meta, mes_atingimento: mes_vendas / mes_meta,
    fytd_vendas, fytd_meta, fytd_atingimento: fytd_vendas / fytd_meta,
    pipeline_aberto: pipeline,
    conversao_fytd: conversao,
    cobertura: cob,
    ticket_medio_fytd: ticket,
    n_cens: detalhes.length
  };
}

// ============ Render principal ============
function renderPerformanceCEN() {
  return `
    <div class="perf-header">
      <div class="perf-header-info">
        <div class="perf-title">Performance de CEN</div>
        <div class="perf-subtitle">Agosto/2026 (parcial · 17 dias úteis) · FYTD nov/2025 → ago/2026</div>
      </div>
      <div class="perf-switcher">
        <button class="perf-persona active" data-persona="cen">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="7" r="4"/><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/></svg>
          Minha performance
        </button>
        <button class="perf-persona" data-persona="regional">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          Regional MT Norte
        </button>
        <button class="perf-persona" data-persona="nacional">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10 15 15 0 0 1-4-10 15 15 0 0 1 4-10z"/></svg>
          Nacional
          <span class="perf-warn-badge">Diretor</span>
        </button>
      </div>
    </div>

    <div id="perfContext" class="perf-context-bar"></div>

    <div id="perfKpis" class="perf-kpi-grid"></div>

    <div class="perf-row-2col">
      <div class="perf-card">
        <div class="perf-card-header">
          <h3 id="perfChartTitle">Evolução mensal · FYTD</h3>
          <div class="perf-card-hint">Vendas fechadas · linha por CEN</div>
        </div>
        <div class="perf-chart-container">
          <canvas id="perfChartTrend"></canvas>
        </div>
      </div>

      <div class="perf-card">
        <div class="perf-card-header">
          <h3>Ranking por métrica</h3>
          <select id="perfMetricaBreakdown" class="perf-select">
            <option value="vendas_fyd">Vendas FYTD (R$)</option>
            <option value="atingimento">% Atingimento FYTD</option>
            <option value="pipeline">Pipeline aberto (R$)</option>
            <option value="conversao">Conversão %</option>
            <option value="cobertura">Cobertura de carteira %</option>
            <option value="ticket">Ticket médio</option>
          </select>
        </div>
        <div class="perf-chart-container">
          <canvas id="perfChartBreakdown"></canvas>
        </div>
      </div>
    </div>

    <div class="perf-card">
      <div class="perf-card-header">
        <h3 id="perfRankingTitle">Ranking detalhado FYTD</h3>
        <div class="perf-card-hint">Clique no cabeçalho para ordenar</div>
      </div>
      <div id="perfRankingTable"></div>
    </div>

    <div class="perf-card">
      <div class="perf-card-header">
        <h3>Insights automáticos</h3>
        <div class="perf-card-hint">Gerados a partir dos dados FYTD · precisam validação humana</div>
      </div>
      <div id="perfInsights" class="perf-insights-list"></div>
    </div>
  `;
}

function mountPerformanceCEN() {
  // Ajusta persona default se o usuário logado é gerente/diretor
  atualizarPerfPersonaBadge();
  renderPerfContext();
  renderPerfKpis();
  renderPerfChartTrend();
  renderPerfChartBreakdown();
  renderPerfRanking();
  renderPerfInsights();

  document.querySelectorAll('[data-persona]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('[data-persona]').forEach(b => b.classList.toggle('active', b === btn));
      PERF_STATE.persona = btn.dataset.persona;
      renderPerfContext();
      renderPerfKpis();
      renderPerfChartTrend();
      renderPerfChartBreakdown();
      renderPerfRanking();
      renderPerfInsights();
    });
  });

  document.getElementById('perfMetricaBreakdown').addEventListener('change', (e) => {
    PERF_STATE.metricaBreakdown = e.target.value;
    renderPerfChartBreakdown();
  });
}

function atualizarPerfPersonaBadge() {
  // No modo cen, mostra o CEN logado (João)
}

// Contextualiza a persona ativa
function renderPerfContext() {
  const el = document.getElementById('perfContext');
  const modos = {
    cen: `
      <span class="perf-context-icon" style="background:#367C2B">JR</span>
      <div>
        <div><strong>João Ribeiro</strong> · CEN · Regional MT Norte</div>
        <div class="perf-context-sub">Admissão 03/2019 · Meta agosto R$ 4.200.000 · Meta FYTD R$ 42.000.000</div>
      </div>
    `,
    regional: `
      <span class="perf-context-icon" style="background:#F59E0B">CB</span>
      <div>
        <div><strong>Cláudia Batista</strong> · Gerente Regional MT Norte · 5 CENs</div>
        <div class="perf-context-sub">João Ribeiro · José Rufino · Ana Paula · Ricardo · Fernanda</div>
      </div>
    `,
    nacional: `
      <span class="perf-context-icon" style="background:#EF4444">RM</span>
      <div>
        <div><strong>Rafael Menezes</strong> · Diretor Comercial · 4 regionais · 8 CENs</div>
        <div class="perf-context-sub">MT Norte (5) · MT Sul (1) · GO (1) · BA (1) — cobertura Brasil Central</div>
      </div>
    `
  };
  el.innerHTML = modos[PERF_STATE.persona];
  el.className = 'perf-context-bar' + (PERF_STATE.persona === 'nacional' ? ' warn' : '');
}

function perfCENsAtivos() {
  if (PERF_STATE.persona === 'cen') return ['joao'];
  if (PERF_STATE.persona === 'regional') return PERF_CENS.filter(c => c.regional === 'MT Norte').map(c => c.id);
  return PERF_CENS.map(c => c.id);
}

function renderPerfKpis() {
  const ids = perfCENsAtivos();
  const grupo = ids.length === 1 ? calcularPerfCEN(ids[0]) : calcularPerfGrupo(ids);
  const isCEN = ids.length === 1;

  const el = document.getElementById('perfKpis');

  const kpi = (label, valor, hint, metaProgresso) => `
    <div class="perf-kpi">
      <div class="pk-label">${label}</div>
      <div class="pk-value">${valor}</div>
      <div class="pk-hint">${hint}</div>
      ${metaProgresso !== undefined ? `
        <div class="pk-meta-bar">
          <div class="pk-meta-fill ${metaProgresso >= 1 ? 'atingido' : metaProgresso >= 0.85 ? 'quase' : metaProgresso >= 0.7 ? 'aviso' : 'baixo'}" style="width:${Math.min(100, metaProgresso * 100)}%"></div>
        </div>
        <div class="pk-meta-txt">${(metaProgresso * 100).toFixed(0)}% da meta</div>
      ` : ''}
    </div>
  `;

  el.innerHTML = `
    ${kpi('Vendas mês (Ago/26)', fmtBRLcompact(grupo.mes_vendas),
      isCEN ? 'Parcial · 17 dias úteis' : `${ids.length} CENs · média ${fmtBRLcompact(grupo.mes_vendas / ids.length)}`,
      grupo.mes_atingimento)}
    ${kpi('Vendas FYTD', fmtBRLcompact(grupo.fytd_vendas),
      isCEN ? '10 meses (nov/25 → ago/26)' : `Meta ${fmtBRLcompact(grupo.fytd_meta)}`,
      grupo.fytd_atingimento)}
    ${kpi('Pipeline aberto', fmtBRLcompact(grupo.pipeline_aberto),
      'Oport. em andamento no funil')}
    ${kpi('Conversão FYTD', (grupo.conversao_fytd * 100).toFixed(1) + '%',
      isCEN ? `${grupo.fytd_ganhas || 0} ganhas de ${grupo.fytd_totais || 0}` : 'Média ponderada')}
    ${kpi('Cobertura de carteira', Math.round(grupo.cobertura) + '%',
      isCEN ? 'Frequência conforme classe' : `Média ${ids.length} CENs`)}
  `;
}

function renderPerfChartTrend() {
  const canvas = document.getElementById('perfChartTrend');
  if (window.__perfChartTrend) window.__perfChartTrend.destroy();
  if (!canvas) return;

  const ids = perfCENsAtivos();
  const labels = FYTD_MESES.map(m => m.label);
  const colors = ['#367C2B', '#1B5E20', '#7C3AED', '#0EA5E9', '#EC4899', '#F59E0B', '#EF4444', '#0D9488'];

  const datasets = ids.map((id, i) => {
    const cen = PERF_CENS.find(c => c.id === id);
    const serie = PERF_SERIES[id];
    return {
      label: cen.nome.split(' ')[0] + ' ' + (cen.nome.split(' ')[1] || '').charAt(0) + '.',
      data: serie.map(m => m.vendas),
      borderColor: cen.avatar,
      backgroundColor: cen.avatar + '20',
      tension: 0.3,
      borderWidth: 2,
      pointRadius: 3,
      pointHoverRadius: 5
    };
  });

  window.__perfChartTrend = new Chart(canvas, {
    type: 'line',
    data: { labels, datasets },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: ids.length > 1,
          position: 'bottom',
          labels: { boxWidth: 10, boxHeight: 10, padding: 10, font: { size: 11 } }
        },
        tooltip: {
          callbacks: {
            label: (ctx) => `${ctx.dataset.label}: ${fmtBRLcompact(ctx.parsed.y)}`
          }
        }
      },
      scales: {
        y: {
          ticks: { callback: (v) => fmtBRLcompact(v), font: { size: 10 } },
          grid: { color: '#F1F5F9' }
        },
        x: {
          ticks: { font: { size: 10 } },
          grid: { display: false }
        }
      }
    }
  });
}

function renderPerfChartBreakdown() {
  const canvas = document.getElementById('perfChartBreakdown');
  if (window.__perfChartBreakdown) window.__perfChartBreakdown.destroy();
  if (!canvas) return;

  const ids = perfCENsAtivos();
  const dados = ids.map(id => {
    const p = calcularPerfCEN(id);
    let val, fmt;
    switch (PERF_STATE.metricaBreakdown) {
      case 'vendas_fyd': val = p.fytd_vendas; fmt = fmtBRLcompact(val); break;
      case 'atingimento': val = p.fytd_atingimento * 100; fmt = val.toFixed(0) + '%'; break;
      case 'pipeline': val = p.pipeline_aberto; fmt = fmtBRLcompact(val); break;
      case 'conversao': val = p.conversao_fytd * 100; fmt = val.toFixed(1) + '%'; break;
      case 'cobertura': val = p.cobertura; fmt = val + '%'; break;
      case 'ticket': val = p.ticket_medio_fytd; fmt = fmtBRLcompact(val); break;
    }
    return { nome: p.cen.nome, val, fmt, color: p.cen.avatar };
  });

  dados.sort((a, b) => b.val - a.val);

  window.__perfChartBreakdown = new Chart(canvas, {
    type: 'bar',
    data: {
      labels: dados.map(d => d.nome),
      datasets: [{
        data: dados.map(d => d.val),
        backgroundColor: dados.map(d => d.color),
        borderRadius: 4,
        borderSkipped: false
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      indexAxis: 'y',
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (ctx) => dados[ctx.dataIndex].fmt
          }
        },
        datalabels: false
      },
      scales: {
        x: {
          ticks: {
            callback: (v) => {
              if (PERF_STATE.metricaBreakdown === 'vendas_fyd' || PERF_STATE.metricaBreakdown === 'pipeline' || PERF_STATE.metricaBreakdown === 'ticket') return fmtBRLcompact(v);
              if (PERF_STATE.metricaBreakdown === 'atingimento' || PERF_STATE.metricaBreakdown === 'conversao' || PERF_STATE.metricaBreakdown === 'cobertura') return v + '%';
              return v;
            },
            font: { size: 10 }
          },
          grid: { color: '#F1F5F9' }
        },
        y: {
          ticks: { font: { size: 11 } },
          grid: { display: false }
        }
      }
    }
  });
}

function renderPerfRanking() {
  const ids = perfCENsAtivos();
  const linhas = ids.map(id => calcularPerfCEN(id))
    .sort((a, b) => b.fytd_vendas - a.fytd_vendas);

  const html = `
    <table class="perf-tabela">
      <thead>
        <tr>
          <th>#</th>
          <th>CEN</th>
          <th>Regional</th>
          <th style="text-align:right">Vendas Ago/26</th>
          <th style="text-align:right">% Meta mês</th>
          <th style="text-align:right">Vendas FYTD</th>
          <th style="text-align:right">% Meta FYTD</th>
          <th style="text-align:right">Pipeline</th>
          <th style="text-align:right">Conversão</th>
          <th style="text-align:right">Ticket médio</th>
          <th style="text-align:right">Cobertura</th>
        </tr>
      </thead>
      <tbody>
        ${linhas.map((p, i) => {
          const rank = i + 1;
          const rankCls = rank === 1 ? 'rank-1' : rank === 2 ? 'rank-2' : rank === 3 ? 'rank-3' : '';
          const clsMesMeta = classAtingimento(p.mes_atingimento);
          const clsFytdMeta = classAtingimento(p.fytd_atingimento);
          const clsCob = p.cobertura >= 80 ? 'val-ok' : p.cobertura >= 65 ? 'val-warn' : 'val-danger';
          return `
            <tr>
              <td><span class="perf-rank ${rankCls}">${rank}</span></td>
              <td>
                <div class="perf-cen-cell">
                  <div class="perf-cen-avatar" style="background:${p.cen.avatar}">${p.cen.foto}</div>
                  <div>
                    <div class="perf-cen-nome">${p.cen.nome}</div>
                    <div class="perf-cen-sub">desde ${new Date(p.cen.admissao).getFullYear()}</div>
                  </div>
                </div>
              </td>
              <td>${p.cen.regional}</td>
              <td class="mono num">${fmtBRLcompact(p.mes_vendas)}</td>
              <td class="mono ${clsMesMeta}">${(p.mes_atingimento * 100).toFixed(0)}%</td>
              <td class="mono num"><strong>${fmtBRLcompact(p.fytd_vendas)}</strong></td>
              <td class="mono ${clsFytdMeta}">${(p.fytd_atingimento * 100).toFixed(0)}%</td>
              <td class="mono num">${fmtBRLcompact(p.pipeline_aberto)}</td>
              <td class="mono num">${(p.conversao_fytd * 100).toFixed(1)}%</td>
              <td class="mono num">${fmtBRLcompact(p.ticket_medio_fytd)}</td>
              <td class="mono ${clsCob}">${p.cobertura}%</td>
            </tr>
          `;
        }).join('')}
      </tbody>
    </table>
  `;
  document.getElementById('perfRankingTable').innerHTML = html;
}

function classAtingimento(pct) {
  if (pct >= 1) return 'val-ok';
  if (pct >= 0.85) return 'val-quase';
  if (pct >= 0.7) return 'val-warn';
  return 'val-danger';
}

function renderPerfInsights() {
  const ids = perfCENsAtivos();
  const detalhes = ids.map(id => calcularPerfCEN(id));
  const insights = [];

  // Insight 1: quem está fora da meta mês
  const fora = detalhes.filter(d => d.mes_atingimento < 0.7);
  if (fora.length > 0 && ids.length > 1) {
    insights.push({
      tipo: 'danger',
      icon: '⚠️',
      texto: `<strong>${fora.length} CEN${fora.length > 1 ? 's' : ''} abaixo de 70% da meta em Ago/26</strong>: ${fora.map(d => d.cen.nome.split(' ')[0]).join(', ')}. Considere revisão de pipeline e reforço de fechamento.`
    });
  }

  // Insight 2: quem está no top
  if (ids.length > 1) {
    const top = detalhes.sort((a, b) => b.fytd_atingimento - a.fytd_atingimento)[0];
    insights.push({
      tipo: 'ok',
      icon: '🏆',
      texto: `<strong>${top.cen.nome}</strong> lidera em atingimento FYTD com ${(top.fytd_atingimento * 100).toFixed(0)}% da meta. Considere replicar padrão de cobertura (${top.cobertura}%) e ritmo de visitas nas reuniões da regional.`
    });
  }

  // Insight 3: cobertura baixa correlaciona com atingimento?
  const cobBaixa = detalhes.filter(d => d.cobertura < 70);
  const foraMeta = detalhes.filter(d => d.fytd_atingimento < 0.85);
  const intersect = cobBaixa.filter(c => foraMeta.some(f => f.cen.id === c.cen.id));
  if (intersect.length > 0 && ids.length > 1) {
    insights.push({
      tipo: 'warn',
      icon: '📊',
      texto: `<strong>${intersect.length} CEN${intersect.length > 1 ? 's' : ''} com cobertura de carteira < 70% e atingimento < 85%</strong>: ${intersect.map(d => d.cen.nome.split(' ')[0]).join(', ')}. Correlação sugere que aumentar contato com base pode destravar vendas.`
    });
  }

  // Insight 4: crescimento mensal
  const emCrescimento = detalhes.filter(d => {
    const s = d.serie;
    if (s.length < 3) return false;
    const ultimos3 = s.slice(-3);
    return ultimos3[2].vendas > ultimos3[0].vendas * 1.15;
  });
  if (emCrescimento.length > 0) {
    insights.push({
      tipo: 'ok',
      icon: '📈',
      texto: `<strong>${emCrescimento.length} CEN${emCrescimento.length > 1 ? 's' : ''} em crescimento acima de 15%</strong> nos últimos 3 meses: ${emCrescimento.map(d => d.cen.nome.split(' ')[0]).join(', ')}. Sinal positivo de aceleração no fim do ano fiscal.`
    });
  }

  // Insight 5: ticket médio alto/baixo
  if (ids.length > 1) {
    const sorted = [...detalhes].sort((a, b) => b.ticket_medio_fytd - a.ticket_medio_fytd);
    const alto = sorted[0], baixo = sorted[sorted.length - 1];
    if (alto.ticket_medio_fytd / baixo.ticket_medio_fytd > 1.4) {
      insights.push({
        tipo: 'info',
        icon: '💰',
        texto: `<strong>Discrepância de ticket médio</strong>: ${alto.cen.nome.split(' ')[0]} (${fmtBRLcompact(alto.ticket_medio_fytd)}) vende ${((alto.ticket_medio_fytd / baixo.ticket_medio_fytd - 1) * 100).toFixed(0)}% acima de ${baixo.cen.nome.split(' ')[0]} (${fmtBRLcompact(baixo.ticket_medio_fytd)}). Verificar mix de produto e perfil de cliente.`
      });
    }
  }

  // Insight 6: piorou vs início FYTD
  const piorou = detalhes.filter(d => {
    const s = d.serie;
    const primeiros3 = s.slice(0, 3).reduce((sum, m) => sum + m.vendas, 0) / 3;
    const ultimos3 = s.slice(-3).reduce((sum, m) => sum + m.vendas, 0) / 3;
    return ultimos3 < primeiros3 * 0.9;
  });
  if (piorou.length > 0) {
    insights.push({
      tipo: 'warn',
      icon: '📉',
      texto: `<strong>${piorou.length} CEN${piorou.length > 1 ? 's' : ''} com queda > 10% vs início do FYTD</strong>: ${piorou.map(d => d.cen.nome.split(' ')[0]).join(', ')}. Investigar causa: mudança de território, perda de clientes chave, ou desengajamento.`
    });
  }

  // No modo CEN mostra insights pessoais
  if (ids.length === 1) {
    const d = detalhes[0];
    const mediaRegional = calcularPerfGrupo(PERF_CENS.filter(c => c.regional === d.cen.regional).map(c => c.id));
    insights.push({
      tipo: d.fytd_atingimento >= mediaRegional.fytd_atingimento ? 'ok' : 'warn',
      icon: d.fytd_atingimento >= mediaRegional.fytd_atingimento ? '✅' : '⚠️',
      texto: `<strong>Comparado com média MT Norte</strong>: seu atingimento FYTD é ${(d.fytd_atingimento * 100).toFixed(0)}% vs ${(mediaRegional.fytd_atingimento * 100).toFixed(0)}% da regional. Ticket médio: ${fmtBRLcompact(d.ticket_medio_fytd)} vs ${fmtBRLcompact(mediaRegional.ticket_medio_fytd)}.`
    });
    if (d.mes_atingimento < 0.85) {
      insights.push({
        tipo: 'warn',
        icon: '🎯',
        texto: `<strong>Atingimento de agosto em ${(d.mes_atingimento * 100).toFixed(0)}%</strong> — parcial de 17 dias úteis. Faltam ${fmtBRLcompact(d.mes_meta - d.mes_vendas)} para bater os R$ ${(d.mes_meta / 1e6).toFixed(1)}M do mês. Pipeline aberto: ${fmtBRLcompact(d.pipeline_aberto)}.`
      });
    }
  }

  const el = document.getElementById('perfInsights');
  el.innerHTML = insights.length === 0
    ? '<div class="perf-insight-vazio">Nenhum insight relevante para o recorte atual.</div>'
    : insights.map(i => `
        <div class="perf-insight ${i.tipo}">
          <span class="pi-icon">${i.icon}</span>
          <div class="pi-texto">${i.texto}</div>
        </div>
      `).join('');
}

// ===== NOVA OPORTUNIDADE =====
// Fluxo funcional: cadastra oportunidade que aparece em Pipeline, Ficha Cliente,
// Funil de Vendas e Performance de CEN (modo pessoal João Ribeiro).
// Estado vive só na sessão (some no F5). Aviso explícito no toast.

// Estado global das oportunidades criadas na sessão
window.OPORTUNIDADES_NOVAS = [];

// Contador de ID (começa depois do maior número no PIPELINE_MOCK)
window.__contadorOportId = 8601;

// Estado do formulário
const NOVA_OP_STATE = {
  cliente_id: null,      // id numérico (referência CARTEIRA_CEN ou clientes_outras)
  cliente_nome: '',      // razão social (se prospect novo digitado)
  cliente_classe: 'B',
  cliente_cidade: '',
  cliente_novo: false,   // true se digitou manual em vez de escolher
  linha: '',
  modelo: '',
  quantidade: 1,
  valor_unitario: 0,
  fase_inicial: 'qualificacao',
  previsao_fechamento: '',
  probabilidade: 15,
  titulo: '',
  observacao: ''
};

// Catálogo de modelos por linha (subset realista JD)
const CATALOGO_MODELOS = {
  'Tratores': [
    { modelo: '5075E',   valor_ref: 320000  },
    { modelo: '5090E',   valor_ref: 380000  },
    { modelo: '6110J',   valor_ref: 745000  },
    { modelo: '6135J',   valor_ref: 890000  },
    { modelo: '7230J',   valor_ref: 1150000 },
    { modelo: '8R 250',  valor_ref: 1890000 },
    { modelo: '8R 340',  valor_ref: 2440000 }
  ],
  'Colheitadeiras': [
    { modelo: 'S770',    valor_ref: 2700000 },
    { modelo: 'S780',    valor_ref: 3100000 },
    { modelo: 'S680 seminova', valor_ref: 2200000 }
  ],
  'Pulverizadores': [
    { modelo: '4630',    valor_ref: 2100000 },
    { modelo: '4730',    valor_ref: 2400000 }
  ],
  'Plantadeiras': [
    { modelo: 'DB40',    valor_ref: 1800000 },
    { modelo: 'DB44',    valor_ref: 2650000 },
    { modelo: 'DB50',    valor_ref: 2900000 }
  ],
  'Implementos': [
    { modelo: '2730',    valor_ref: 420000  },
    { modelo: 'Arado 4 discos', valor_ref: 180000 }
  ],
  'Peças & Serviços': [
    { modelo: 'Contrato anual', valor_ref: 520000 }
  ]
};

// Reseta form ao entrar
function resetNovaOpState() {
  NOVA_OP_STATE.cliente_id = null;
  NOVA_OP_STATE.cliente_nome = '';
  NOVA_OP_STATE.cliente_classe = 'B';
  NOVA_OP_STATE.cliente_cidade = '';
  NOVA_OP_STATE.cliente_novo = false;
  NOVA_OP_STATE.linha = '';
  NOVA_OP_STATE.modelo = '';
  NOVA_OP_STATE.quantidade = 1;
  NOVA_OP_STATE.valor_unitario = 0;
  NOVA_OP_STATE.fase_inicial = 'qualificacao';
  NOVA_OP_STATE.probabilidade = 15;
  NOVA_OP_STATE.titulo = '';
  NOVA_OP_STATE.observacao = '';
  // Previsão default: 60 dias à frente
  const dt = new Date(2026, 7, 25);
  dt.setDate(dt.getDate() + 60);
  NOVA_OP_STATE.previsao_fechamento = dt.toISOString().slice(0, 10);
}

function renderNovaOportunidade() {
  resetNovaOpState();

  return `
    <div class="novaop-header">
      <div>
        <div class="novaop-title">Nova Oportunidade</div>
        <div class="novaop-subtitle">Cadastro do zero · aparece em Pipeline, Cliente, Funil e Performance imediatamente</div>
        ${(window.OPORTUNIDADES_NOVAS || []).length > 0 ? `
          <div class="novaop-persist-hint ${storageDisponivel() ? '' : 'sem-persist'}">
            <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M20 6L9 17l-5-5"/></svg>
            <span>${window.OPORTUNIDADES_NOVAS.length} oportunidade${window.OPORTUNIDADES_NOVAS.length > 1 ? 's' : ''} nesta sessão · ${storageDisponivel() ? 'salvas no navegador (resistem a F5)' : 'apenas em memória (F5 apaga)'}</span>
            <button class="btn-limpar-sessao" onclick="confirmarLimparSessao()">Limpar</button>
          </div>
        ` : ''}
      </div>
      <div class="novaop-cen-badge">
        <div class="novaop-cen-avatar" style="background:#367C2B">JR</div>
        <div>
          <div class="novaop-cen-nome">João Ribeiro</div>
          <div class="novaop-cen-role">CEN atribuído · MT Norte</div>
        </div>
      </div>
    </div>

    <div class="novaop-grid">
      <!-- SEÇÃO 1: CLIENTE -->
      <div class="novaop-section">
        <div class="novaop-section-num">1</div>
        <div class="novaop-section-body">
          <div class="novaop-section-title">Cliente</div>
          <div class="novaop-section-hint">Escolha da carteira ou informe um prospect novo</div>

          <div class="novaop-modo-cliente">
            <label class="novaop-modo-item active" data-modo="carteira">
              <input type="radio" name="modoCliente" value="carteira" checked>
              <span>Da carteira</span>
            </label>
            <label class="novaop-modo-item" data-modo="novo">
              <input type="radio" name="modoCliente" value="novo">
              <span>Prospect novo</span>
            </label>
          </div>

          <div id="novaop-cliente-carteira" class="novaop-cliente-box">
            <label class="novaop-field">
              <span class="novaop-label">Cliente da carteira</span>
              <select id="novaopCliente" class="novaop-input">
                <option value="">Selecione...</option>
                ${CARTEIRA_CEN.map(c => `
                  <option value="${c.id}" data-classe="${c.classe}" data-cidade="${c.cidade}" data-razao="${c.razao}">
                    ${c.razao} · Classe ${c.classe} · ${c.cidade}
                  </option>
                `).join('')}
              </select>
            </label>
            <div id="novaopClienteInfo" class="novaop-cliente-info oculto">
              <!-- preenchido dinamicamente -->
            </div>
          </div>

          <div id="novaop-cliente-novo" class="novaop-cliente-box oculto">
            <label class="novaop-field">
              <span class="novaop-label">Razão social</span>
              <input id="novaopNovoRazao" class="novaop-input" placeholder="Ex: Fazenda São João Agropecuária Ltda">
            </label>
            <div class="novaop-row-2">
              <label class="novaop-field">
                <span class="novaop-label">Classe</span>
                <select id="novaopNovoClasse" class="novaop-input">
                  <option value="A">A · Faturamento > R$ 3M/ano</option>
                  <option value="B" selected>B · R$ 500k - 3M/ano</option>
                  <option value="C">C · R$ 100k - 500k/ano</option>
                  <option value="D">D · Prospect ou < R$ 100k</option>
                </select>
              </label>
              <label class="novaop-field">
                <span class="novaop-label">Cidade / UF</span>
                <input id="novaopNovaCidade" class="novaop-input" placeholder="Ex: Sinop / MT">
              </label>
            </div>
            <div class="novaop-alerta">
              <strong>Atenção:</strong> prospect novo será criado sem passar por Protheus. Em produção, cadastro real exige aprovação do Admin Comercial.
            </div>
          </div>
        </div>
      </div>

      <!-- SEÇÃO 2: EQUIPAMENTO -->
      <div class="novaop-section">
        <div class="novaop-section-num">2</div>
        <div class="novaop-section-body">
          <div class="novaop-section-title">Equipamento e valor</div>
          <div class="novaop-section-hint">Selecione a linha e o modelo. Valor referência do catálogo, ajustável.</div>

          <div class="novaop-row-2">
            <label class="novaop-field">
              <span class="novaop-label">Linha</span>
              <select id="novaopLinha" class="novaop-input">
                <option value="">Selecione a linha...</option>
                ${Object.keys(CATALOGO_MODELOS).map(l => `
                  <option value="${l}">${LINHA_ICON[l] || ''} ${l}</option>
                `).join('')}
              </select>
            </label>
            <label class="novaop-field">
              <span class="novaop-label">Modelo</span>
              <select id="novaopModelo" class="novaop-input" disabled>
                <option value="">Selecione a linha primeiro</option>
              </select>
            </label>
          </div>

          <div class="novaop-row-3">
            <label class="novaop-field">
              <span class="novaop-label">Quantidade</span>
              <input type="number" id="novaopQtde" class="novaop-input" min="1" value="1">
            </label>
            <label class="novaop-field">
              <span class="novaop-label">Valor unitário (R$)</span>
              <input type="number" id="novaopValor" class="novaop-input" placeholder="0" step="1000">
            </label>
            <label class="novaop-field">
              <span class="novaop-label">Valor total</span>
              <div class="novaop-valor-total" id="novaopValorTotal">R$ 0</div>
            </label>
          </div>
        </div>
      </div>

      <!-- SEÇÃO 3: DETALHES E CONFIRMAÇÃO -->
      <div class="novaop-section">
        <div class="novaop-section-num">3</div>
        <div class="novaop-section-body">
          <div class="novaop-section-title">Detalhes da oportunidade</div>
          <div class="novaop-section-hint">Fase inicial, previsão e título. Nasce em Qualificação por padrão.</div>

          <label class="novaop-field">
            <span class="novaop-label">Título</span>
            <input id="novaopTitulo" class="novaop-input" placeholder="Ex: Renovação frota tratores compactos">
          </label>

          <div class="novaop-row-3">
            <label class="novaop-field">
              <span class="novaop-label">Fase inicial</span>
              <select id="novaopFase" class="novaop-input">
                <option value="qualificacao" selected>Qualificação (15%)</option>
                <option value="diagnostico">Diagnóstico (30%)</option>
                <option value="proposta">Proposta (55%)</option>
              </select>
            </label>
            <label class="novaop-field">
              <span class="novaop-label">Previsão fechamento</span>
              <input type="date" id="novaopPrevisao" class="novaop-input">
            </label>
            <label class="novaop-field">
              <span class="novaop-label">Probabilidade (%)</span>
              <input type="number" id="novaopProb" class="novaop-input" min="0" max="100" value="15">
            </label>
          </div>

          <label class="novaop-field">
            <span class="novaop-label">Observação (opcional)</span>
            <textarea id="novaopObs" class="novaop-input" rows="2" placeholder="Notas relevantes sobre o cliente, contexto, próximas ações..."></textarea>
          </label>
        </div>
      </div>

      <!-- PREVIEW DE IMPACTO -->
      <div class="novaop-preview" id="novaopPreview">
        <div class="novaop-preview-title">Impacto ao salvar esta oportunidade:</div>
        <div class="novaop-preview-grid" id="novaopPreviewGrid">
          <div class="novaop-preview-item">
            <div class="npi-label">Aparecerá em Pipeline</div>
            <div class="npi-value">Coluna Qualificação · MT Norte · João Ribeiro</div>
          </div>
          <div class="novaop-preview-item">
            <div class="npi-label">Somará ao pipeline aberto do CEN</div>
            <div class="npi-value" id="npiPipeAberto">+ R$ 0</div>
          </div>
          <div class="novaop-preview-item">
            <div class="npi-label">Contará no Funil de Vendas</div>
            <div class="npi-value">Fase Qualificação</div>
          </div>
          <div class="novaop-preview-item">
            <div class="npi-label">Aparecerá na Ficha do Cliente</div>
            <div class="npi-value" id="npiClienteFicha">— selecione um cliente —</div>
          </div>
        </div>
      </div>

      <!-- BOTÕES -->
      <div class="novaop-actions">
        <button id="novaopCancelar" class="btn-cancelar">Cancelar</button>
        <button id="novaopSalvar" class="btn-salvar" disabled>
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
          Salvar oportunidade
        </button>
      </div>
    </div>
  `;
}

function mountNovaOportunidade() {
  // Toggle carteira vs prospect novo
  document.querySelectorAll('[data-modo]').forEach(item => {
    item.addEventListener('click', () => {
      document.querySelectorAll('[data-modo]').forEach(i => i.classList.toggle('active', i === item));
      const modo = item.dataset.modo;
      document.getElementById('novaop-cliente-carteira').classList.toggle('oculto', modo !== 'carteira');
      document.getElementById('novaop-cliente-novo').classList.toggle('oculto', modo !== 'novo');
      NOVA_OP_STATE.cliente_novo = (modo === 'novo');
      // Limpa seleção anterior ao alternar
      NOVA_OP_STATE.cliente_id = null;
      NOVA_OP_STATE.cliente_nome = '';
      atualizarPreviewNovaOp();
      validarNovaOp();
    });
  });

  // Cliente da carteira
  document.getElementById('novaopCliente').addEventListener('change', (e) => {
    const opt = e.target.options[e.target.selectedIndex];
    if (!e.target.value) {
      NOVA_OP_STATE.cliente_id = null;
      NOVA_OP_STATE.cliente_nome = '';
      document.getElementById('novaopClienteInfo').classList.add('oculto');
    } else {
      const c = CARTEIRA_CEN.find(x => x.id === parseInt(e.target.value));
      NOVA_OP_STATE.cliente_id = c.id;
      NOVA_OP_STATE.cliente_nome = c.razao;
      NOVA_OP_STATE.cliente_classe = c.classe;
      NOVA_OP_STATE.cliente_cidade = c.cidade;
      const info = document.getElementById('novaopClienteInfo');
      info.classList.remove('oculto');
      info.innerHTML = `
        <div class="nci-item"><span class="nci-label">Classe</span><span class="nci-value classe-${c.classe.toLowerCase()}">${c.classe}</span></div>
        <div class="nci-item"><span class="nci-label">Faturamento 12m</span><span class="nci-value">${fmtBRLcompact(c.fat_12m)}</span></div>
        <div class="nci-item"><span class="nci-label">Oportunidades ativas</span><span class="nci-value">${c.oportunidades} · ${c.oportunidades > 0 ? fmtBRLcompact(c.oportunidades_valor) : '—'}</span></div>
        <div class="nci-item"><span class="nci-label">Última interação</span><span class="nci-value">${c.ult_int ? c.ult_int.data + ' · ' + c.ult_int.cat : 'nunca'}</span></div>
      `;
    }
    atualizarPreviewNovaOp();
    validarNovaOp();
  });

  // Prospect novo
  ['novaopNovoRazao','novaopNovoClasse','novaopNovaCidade'].forEach(id => {
    document.getElementById(id).addEventListener('input', () => {
      NOVA_OP_STATE.cliente_nome = document.getElementById('novaopNovoRazao').value.trim();
      NOVA_OP_STATE.cliente_classe = document.getElementById('novaopNovoClasse').value;
      NOVA_OP_STATE.cliente_cidade = document.getElementById('novaopNovaCidade').value.trim();
      // Gera id sintético para prospect novo
      if (NOVA_OP_STATE.cliente_nome && !NOVA_OP_STATE.cliente_id) {
        NOVA_OP_STATE.cliente_id = 99000 + Math.floor(Math.random() * 999);
      }
      atualizarPreviewNovaOp();
      validarNovaOp();
    });
  });

  // Linha e modelo
  document.getElementById('novaopLinha').addEventListener('change', (e) => {
    NOVA_OP_STATE.linha = e.target.value;
    const selModelo = document.getElementById('novaopModelo');
    if (!e.target.value) {
      selModelo.innerHTML = '<option value="">Selecione a linha primeiro</option>';
      selModelo.disabled = true;
      NOVA_OP_STATE.modelo = '';
    } else {
      const modelos = CATALOGO_MODELOS[e.target.value];
      selModelo.innerHTML = '<option value="">Selecione o modelo...</option>' +
        modelos.map(m => `<option value="${m.modelo}" data-valor="${m.valor_ref}">${m.modelo} · ref. R$ ${(m.valor_ref/1000).toFixed(0)}k</option>`).join('');
      selModelo.disabled = false;
    }
    atualizarValorTotal();
    validarNovaOp();
  });

  document.getElementById('novaopModelo').addEventListener('change', (e) => {
    NOVA_OP_STATE.modelo = e.target.value;
    const opt = e.target.options[e.target.selectedIndex];
    if (opt && opt.dataset.valor) {
      NOVA_OP_STATE.valor_unitario = parseInt(opt.dataset.valor);
      document.getElementById('novaopValor').value = NOVA_OP_STATE.valor_unitario;
    }
    atualizarValorTotal();
    validarNovaOp();
  });

  document.getElementById('novaopQtde').addEventListener('input', (e) => {
    NOVA_OP_STATE.quantidade = Math.max(1, parseInt(e.target.value) || 1);
    atualizarValorTotal();
  });
  document.getElementById('novaopValor').addEventListener('input', (e) => {
    NOVA_OP_STATE.valor_unitario = parseInt(e.target.value) || 0;
    atualizarValorTotal();
  });

  // Fase inicial → probabilidade default
  document.getElementById('novaopFase').addEventListener('change', (e) => {
    NOVA_OP_STATE.fase_inicial = e.target.value;
    const probsDefault = { qualificacao: 15, diagnostico: 30, proposta: 55 };
    document.getElementById('novaopProb').value = probsDefault[e.target.value];
    NOVA_OP_STATE.probabilidade = probsDefault[e.target.value];
  });

  document.getElementById('novaopProb').addEventListener('input', (e) => {
    NOVA_OP_STATE.probabilidade = Math.max(0, Math.min(100, parseInt(e.target.value) || 0));
  });

  document.getElementById('novaopPrevisao').value = NOVA_OP_STATE.previsao_fechamento;
  document.getElementById('novaopPrevisao').addEventListener('input', (e) => {
    NOVA_OP_STATE.previsao_fechamento = e.target.value;
  });

  document.getElementById('novaopTitulo').addEventListener('input', (e) => {
    NOVA_OP_STATE.titulo = e.target.value.trim();
    validarNovaOp();
  });

  document.getElementById('novaopObs').addEventListener('input', (e) => {
    NOVA_OP_STATE.observacao = e.target.value;
  });

  // Cancelar
  document.getElementById('novaopCancelar').addEventListener('click', () => {
    window.location.hash = '#/';
  });

  // Salvar
  document.getElementById('novaopSalvar').addEventListener('click', salvarNovaOportunidade);

  atualizarValorTotal();
}

function atualizarValorTotal() {
  const total = NOVA_OP_STATE.quantidade * NOVA_OP_STATE.valor_unitario;
  document.getElementById('novaopValorTotal').textContent = fmtBRLcompact(total);
  atualizarPreviewNovaOp();
}

function atualizarPreviewNovaOp() {
  const total = NOVA_OP_STATE.quantidade * NOVA_OP_STATE.valor_unitario;
  document.getElementById('npiPipeAberto').textContent = '+ ' + fmtBRLcompact(total);
  const nomeCliente = NOVA_OP_STATE.cliente_nome || '— selecione um cliente —';
  document.getElementById('npiClienteFicha').textContent = nomeCliente;
}

function validarNovaOp() {
  const valido =
    NOVA_OP_STATE.cliente_nome &&
    NOVA_OP_STATE.linha &&
    NOVA_OP_STATE.modelo &&
    NOVA_OP_STATE.valor_unitario > 0 &&
    NOVA_OP_STATE.titulo;
  document.getElementById('novaopSalvar').disabled = !valido;
}

function salvarNovaOportunidade() {
  const numOp = window.__contadorOportId++;
  const id = 'OP-2026-0' + numOp;
  const modeloDisplay = NOVA_OP_STATE.quantidade > 1 ? `${NOVA_OP_STATE.modelo} × ${NOVA_OP_STATE.quantidade}` : NOVA_OP_STATE.modelo;
  const valorTotal = NOVA_OP_STATE.quantidade * NOVA_OP_STATE.valor_unitario;

  const nova = {
    id,
    titulo: NOVA_OP_STATE.titulo,
    cliente: NOVA_OP_STATE.cliente_nome,
    cliente_id: NOVA_OP_STATE.cliente_id,
    classe: NOVA_OP_STATE.cliente_classe,
    cidade: NOVA_OP_STATE.cliente_cidade,
    cen: 'joao',   // sempre atribuído ao CEN logado
    linha: NOVA_OP_STATE.linha,
    modelo: modeloDisplay,
    valor: valorTotal,
    probabilidade: NOVA_OP_STATE.probabilidade,
    previsao: NOVA_OP_STATE.previsao_fechamento,
    dias_fase: 0,
    fase: NOVA_OP_STATE.fase_inicial,
    observacao: NOVA_OP_STATE.observacao,
    criada_agora: true,       // flag visual para destacar no Pipeline
    criada_em: new Date().toISOString(),
    prospect_novo: NOVA_OP_STATE.cliente_novo
  };

  OPORTUNIDADES_NOVAS.push(nova);
  // Também injeta no PIPELINE_MOCK para as telas que iteram sobre ele
  PIPELINE_MOCK.push(nova);

  // Persiste no localStorage
  salvarOportunidadesPersistidas();

  // Toast de sucesso
  mostrarToastNovaOp(nova);

  // Redireciona para o Pipeline com filtro no CEN João
  setTimeout(() => {
    window.location.hash = '#/pipeline';
  }, 1500);
}

function mostrarToastNovaOp(op) {
  const toast = document.createElement('div');
  toast.className = 'novaop-toast';
  toast.innerHTML = `
    <div class="nt-icon">
      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5">
        <polyline points="20 6 9 17 4 12"/>
      </svg>
    </div>
    <div class="nt-body">
      <div class="nt-title">Oportunidade ${op.id} criada</div>
      <div class="nt-sub">${op.cliente} · ${fmtBRLcompact(op.valor)} · fase ${PIPELINE_FASES.find(f => f.id === op.fase).label}</div>
      <div class="nt-hint">Já visível no Pipeline, Ficha do Cliente, Funil e Performance. Redirecionando...</div>
    </div>
  `;
  document.body.appendChild(toast);
  setTimeout(() => toast.classList.add('show'), 50);
  setTimeout(() => toast.remove(), 3000);
}

// Mapa entre fases do Pipeline (kanban) e fases do Funil (relatório)
function mapFaseFunil(fasePipeline) {
  const mapa = {
    'qualificacao': 'Orçamento',
    'diagnostico': 'Orçamento',
    'proposta':    'Proposta',
    'negociacao':  'Negociação',
    'fechamento':  'Negócio Fechado'
  };
  return mapa[fasePipeline] || 'Orçamento';
}

// ===== MODAL: Ficha de oportunidade indisponível =====
window.mostrarModalFichaIndispo = function(opId, titulo, isNova) {
  // Remove modal anterior se existir
  const existente = document.querySelector('.modal-ficha-indispo-overlay');
  if (existente) existente.remove();

  const overlay = document.createElement('div');
  overlay.className = 'modal-ficha-indispo-overlay';
  overlay.innerHTML = `
    <div class="modal-ficha-indispo">
      <div class="mfi-header">
        <div class="mfi-icon">
          <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.2">
            <circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/>
          </svg>
        </div>
        <div>
          <div class="mfi-title">Ficha completa não disponível neste protótipo</div>
          <div class="mfi-sub">${titulo} · <span class="mono">${opId}</span></div>
        </div>
        <button class="mfi-close" onclick="this.closest('.modal-ficha-indispo-overlay').remove()" title="Fechar">×</button>
      </div>
      <div class="mfi-body">
        ${isNova ? `
          <p><strong>Esta oportunidade foi criada nesta sessão</strong> através da tela "Nova Oportunidade" e já está refletida no Pipeline, Funil, KPIs deste cliente e Performance do CEN.</p>
          <p>A <strong>ficha completa</strong> (timeline de aprovações, itens, documentação, histórico auditado) só está prototipada para a oportunidade <span class="mono">#1517613 · Salvador Arena</span>, que serve de referência visual para o MVP.</p>
        ` : `
          <p>Neste protótipo, a <strong>ficha completa de oportunidade</strong> (8 fases, 6 aprovações, itens, documentação, histórico auditado) foi construída apenas para a oportunidade <span class="mono">#1517613 · Salvador Arena</span>.</p>
          <p>As demais oportunidades aparecem apenas como <strong>cards de listagem</strong> em Pipeline, Ficha do Cliente e Funil. No MVP real, todas terão ficha completa gerada dinamicamente.</p>
        `}
      </div>
      <div class="mfi-actions">
        <button class="btn-cancelar" onclick="this.closest('.modal-ficha-indispo-overlay').remove()">Fechar</button>
        ${!isNova ? '' : `
          <a href="#/oportunidades/1517613" class="btn-salvar" onclick="this.closest('.modal-ficha-indispo-overlay').remove()">
            Ver ficha de referência (#1517613)
          </a>
        `}
      </div>
    </div>
  `;
  document.body.appendChild(overlay);
  // Fecha ao clicar no overlay
  overlay.addEventListener('click', (e) => {
    if (e.target === overlay) overlay.remove();
  });
};

// ===== Confirmar limpeza de sessão =====
window.confirmarLimparSessao = function() {
  const overlay = document.createElement('div');
  overlay.className = 'modal-ficha-indispo-overlay';
  const n = (window.OPORTUNIDADES_NOVAS || []).length;
  overlay.innerHTML = `
    <div class="modal-ficha-indispo">
      <div class="mfi-header">
        <div class="mfi-icon" style="background:#FEF3C7;color:#B45309">
          <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.2">
            <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><path d="M12 9v4"/><path d="M12 17h.01"/>
          </svg>
        </div>
        <div>
          <div class="mfi-title">Limpar oportunidades da sessão?</div>
          <div class="mfi-sub">${n} oportunidade${n > 1 ? 's' : ''} criada${n > 1 ? 's' : ''} será${n > 1 ? 'ão' : ''} removida${n > 1 ? 's' : ''}</div>
        </div>
        <button class="mfi-close" onclick="this.closest('.modal-ficha-indispo-overlay').remove()">×</button>
      </div>
      <div class="mfi-body">
        <p>Esta ação remove <strong>apenas as oportunidades criadas nesta sessão</strong> via tela "Nova Oportunidade". As oportunidades originais do protótipo permanecem.</p>
        <p>Após confirmar, Pipeline, Ficha do Cliente, Funil e Performance voltarão ao estado inicial.</p>
      </div>
      <div class="mfi-actions">
        <button class="btn-cancelar" onclick="this.closest('.modal-ficha-indispo-overlay').remove()">Cancelar</button>
        <button class="btn-limpar-confirmar" onclick="executarLimparSessao()">Sim, limpar ${n} oportunidade${n > 1 ? 's' : ''}</button>
      </div>
    </div>
  `;
  document.body.appendChild(overlay);
  overlay.addEventListener('click', (e) => { if (e.target === overlay) overlay.remove(); });
};

window.executarLimparSessao = function() {
  limparOportunidadesPersistidas();
  document.querySelectorAll('.modal-ficha-indispo-overlay').forEach(el => el.remove());
  // Toast de confirmação
  const toast = document.createElement('div');
  toast.className = 'novaop-toast';
  toast.style.borderLeftColor = '#B45309';
  toast.innerHTML = `
    <div class="nt-icon" style="background:#B45309">
      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-2 14a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2L5 6"/></svg>
    </div>
    <div class="nt-body">
      <div class="nt-title">Sessão limpa</div>
      <div class="nt-sub">Oportunidades removidas</div>
      <div class="nt-hint">Recarregando a tela...</div>
    </div>
  `;
  document.body.appendChild(toast);
  setTimeout(() => toast.classList.add('show'), 50);
  setTimeout(() => {
    toast.remove();
    // Re-renderiza a tela atual
    router();
  }, 1200);
};
// ===== VISÃO 360 =====
// Dashboard executivo com 3 perfis (CEN / Gerente / Diretor) e drill down Filial > CEN > Cliente

// -------- Dados de mercado (mock coerente) --------
// Fonte de verdade: John Deere Connect (emplacamentos por praça) + Protheus (vendas)
// Escopo: FY 2026 (abr/26 → mar/27), com dados até ago/26

// Total de máquinas do mercado por praça/filial (John Deere Connect)
const MERCADO_JD_PRACAS = {
  'MT Norte': { total_mercado: 480, vendemos: 156, indicamos_perdida: 138, sem_conhecimento: 186 },
  'MT Sul':   { total_mercado: 320, vendemos:  98, indicamos_perdida:  78, sem_conhecimento: 144 },
  'GO':       { total_mercado: 410, vendemos: 142, indicamos_perdida:  95, sem_conhecimento: 173 },
  'BA':       { total_mercado: 260, vendemos:  76, indicamos_perdida:  52, sem_conhecimento: 132 }
};

// Vendas perdidas por motivo (últimos 12 meses)
const VENDAS_PERDIDAS_MOTIVOS = [
  { motivo: 'Preço acima do concorrente', qtd: 87, valor: 42800000, cor: '#DC2626' },
  { motivo: 'Prazo de entrega',           qtd: 52, valor: 24500000, cor: '#EA580C' },
  { motivo: 'Financiamento negado',       qtd: 41, valor: 18200000, cor: '#B45309' },
  { motivo: 'Preferência por concorrente',qtd: 78, valor: 38900000, cor: '#7C3AED' },
  { motivo: 'Cliente adiou investimento', qtd: 65, valor: 29100000, cor: '#6B7280' },
  { motivo: 'Configuração indisponível',  qtd: 40, valor: 17800000, cor: '#9CA3AF' }
];

// Previsão vs Realizado 12 meses (linha)
// Meses: set/25 até ago/26 (12 pontos, com set-mar do FY25 e abr-ago do FY26)
const FATURAMENTO_12M = {
  labels: ['set/25','out/25','nov/25','dez/25','jan/26','fev/26','mar/26','abr/26','mai/26','jun/26','jul/26','ago/26'],
  realizado_global:  [28.4, 32.1, 35.8, 41.2, 22.5, 18.9, 24.3, 27.8, 31.5, 34.2, 38.6, 36.2], // R$ milhões
  previsto_global:   [27.0, 31.5, 36.0, 40.0, 24.0, 20.0, 25.0, 28.0, 32.0, 34.0, 38.0, 37.5],
  meta_global:       [30.0, 33.0, 36.0, 42.0, 25.0, 21.0, 26.0, 30.0, 33.0, 35.0, 39.0, 39.0]
};
// Multiplicadores para gerar séries por perfil
const FAT_MULT_MT_NORTE = 0.32; // gerente vê ~1/3 do global
const FAT_MULT_CEN_JOAO = 0.11; // CEN vê ~1/9 do global (1 de 8 CENs)

// Mix por linha de produto
const MIX_LINHAS = [
  { linha: 'Tratores',      pct: 42, valor: 152000000, cor: '#367C2B' },
  { linha: 'Colheitadeiras',pct: 28, valor: 102000000, cor: '#1B5E20' },
  { linha: 'Plantadeiras',  pct: 14, valor:  51000000, cor: '#4A9040' },
  { linha: 'Pulverizadores',pct:  9, valor:  32000000, cor: '#7CB342' },
  { linha: 'Peças e Serviços',pct: 7, valor: 25000000, cor: '#FFDE00' }
];

// Top clientes (agregado a partir do CARTEIRA_CEN quando perfil = CEN, sintético para os outros)
const TOP_CLIENTES_GLOBAL = [
  { razao: 'Grupo Amaggi',                 fat_fytd: 18500000, cidade: 'Sapezal/MT',      cen: 'José Rufino' },
  { razao: 'Grupo SLC Agrícola',           fat_fytd: 15200000, cidade: 'Sinop/MT',        cen: 'José Rufino' },
  { razao: 'Grupo Bom Futuro',             fat_fytd: 12800000, cidade: 'Sinop/MT',        cen: 'João Ribeiro' },
  { razao: 'Grupo Wehrmann',               fat_fytd: 11400000, cidade: 'Campo Novo/MT',   cen: 'Ricardo Alves' },
  { razao: 'Agroindustrial Salvador Arena',fat_fytd:  9800000, cidade: 'Lucas do R.V./MT',cen: 'José Rufino' },
  { razao: 'Fazenda Boa Vista',            fat_fytd:  8600000, cidade: 'Sorriso/MT',      cen: 'Ana Paula S.' },
  { razao: 'Grupo Terra Santa',            fat_fytd:  7900000, cidade: 'Rio Verde/GO',    cen: 'Renata Costa' },
  { razao: 'Fazenda Santa Clara',          fat_fytd:  7200000, cidade: 'Sinop/MT',        cen: 'João Ribeiro' }
];

// Alertas gerenciais (mudam por perfil)
const ALERTAS_POR_PERFIL = {
  cen: [
    { tipo: 'critico', titulo: '4 clientes Classe A sem visita há 90+ dias', detalhe: 'Salvador Arena, Boa Vista, SLC Bocaiúva, Três Marias', acao: 'Ver Cobertura' },
    { tipo: 'aviso',   titulo: 'Meta do mês em 82% · faltam 8 dias úteis', detalhe: 'R$ 780k em pipeline de fase Negociação', acao: 'Ver Pipeline' },
    { tipo: 'info',    titulo: 'Nova oportunidade criada em ago/26', detalhe: 'OP-2026-08471 · Salvador Arena · R$ 3.2M', acao: 'Ver oportunidade' }
  ],
  gerente: [
    { tipo: 'critico', titulo: 'Fernanda Melo abaixo de 70% da meta há 3 meses', detalhe: 'Ticket médio caindo · convocar 1:1', acao: 'Ver Performance' },
    { tipo: 'aviso',   titulo: 'Conhecimento de mercado MT Norte em 61%', detalhe: '186 máquinas emplacadas na região sem registro nosso', acao: 'Ver detalhe' },
    { tipo: 'info',    titulo: 'Regional bateu 108% da meta em jul/26', detalhe: 'Melhor mês em 8 meses · reconhecer time', acao: 'Ver relatório' }
  ],
  diretor: [
    { tipo: 'critico', titulo: 'BA e MT Sul abaixo do FY plano', detalhe: 'BA em 78% do FYTD · MT Sul em 84%', acao: 'Ver por filial' },
    { tipo: 'aviso',   titulo: 'Concorrente ganhou 78 negócios em 12m', detalhe: '"Preferência por concorrente" é 2ª causa · rever posicionamento', acao: 'Ver perdidas' },
    { tipo: 'info',    titulo: 'Mix de Peças e Serviços cresceu 3pp YoY', detalhe: 'Melhor margem · manter foco', acao: 'Ver mix' }
  ]
};

// Perfis
const PERFIS_360 = {
  cen: {
    id: 'cen',
    nome: 'João Ribeiro',
    cargo: 'CEN · MT Norte · Lucas do Rio Verde',
    avatar: 'JR',
    cor: '#367C2B',
    escopo_label: 'Minha carteira'
  },
  gerente: {
    id: 'gerente',
    nome: 'Roberto Silveira',
    cargo: 'Gerente Regional · MT Norte',
    avatar: 'RS',
    cor: '#1B5E20',
    escopo_label: 'Regional MT Norte'
  },
  diretor: {
    id: 'diretor',
    nome: 'Cláudia Nunes',
    cargo: 'Diretora Comercial · Tracbel Agro',
    avatar: 'CN',
    cor: '#7C3AED',
    escopo_label: 'Nacional · 4 filiais'
  }
};

// Perfil ativo (persistido em memória)
window.__perfil360 = window.__perfil360 || 'cen';
window.__drill360 = window.__drill360 || null; // { nivel: 'filial'|'cen'|'cliente', valor: 'MT Norte' }

// -------- Helpers de agregação --------
function agregar360(perfilId, drill) {
  const p = PERFIS_360[perfilId];
  let pracas = [];
  let fatMult = 1;

  if (perfilId === 'cen') {
    pracas = ['MT Norte'];
    fatMult = FAT_MULT_CEN_JOAO;
  } else if (perfilId === 'gerente') {
    pracas = ['MT Norte'];
    fatMult = FAT_MULT_MT_NORTE;
  } else {
    pracas = Object.keys(MERCADO_JD_PRACAS);
    fatMult = 1;
  }

  // Drill sobrepõe
  if (drill && drill.nivel === 'filial') {
    pracas = [drill.valor];
    // recalcula fatMult para essa filial isolada
    const pctFilial = MERCADO_JD_PRACAS[drill.valor].total_mercado /
      Object.values(MERCADO_JD_PRACAS).reduce((s, v) => s + v.total_mercado, 0);
    fatMult = pctFilial;
  }

  const totMercado = pracas.reduce((s, k) => s + MERCADO_JD_PRACAS[k].total_mercado, 0);
  const totVendemos = pracas.reduce((s, k) => s + MERCADO_JD_PRACAS[k].vendemos, 0);
  const totPerdida = pracas.reduce((s, k) => s + MERCADO_JD_PRACAS[k].indicamos_perdida, 0);
  const conhecimento = ((totVendemos + totPerdida) / totMercado) * 100;

  // Faturamento
  const fat12m = FATURAMENTO_12M.realizado_global.map(v => v * fatMult);
  const prev12m = FATURAMENTO_12M.previsto_global.map(v => v * fatMult);
  const meta12m = FATURAMENTO_12M.meta_global.map(v => v * fatMult);
  const fatMes = fat12m[fat12m.length - 1]; // ago/26
  const metaMes = meta12m[meta12m.length - 1];
  // FYTD = abr → ago (últimos 5 pontos)
  const fatFYTD = fat12m.slice(-5).reduce((s, v) => s + v, 0);
  const metaFYTD = meta12m.slice(-5).reduce((s, v) => s + v, 0);
  // Previsão FY = FYTD realizado + previsão dos 7 meses restantes (set → mar)
  // Usa média + ajuste sazonal simples (set-dez são fortes, jan-mar médios)
  const mediaPrevistoRestante = (prev12m.reduce((s, v) => s + v, 0) / 12) * 7;
  const prevFY = fatFYTD + mediaPrevistoRestante;
  const metaFY = meta12m.reduce((s, v) => s + v, 0);

  // Clientes na carteira
  let clientesCarteira;
  if (perfilId === 'cen') {
    clientesCarteira = CARTEIRA_CEN.length;
  } else if (perfilId === 'gerente') {
    clientesCarteira = 118; // MT Norte: 5 CENs × ~24 clientes
  } else {
    clientesCarteira = 380; // Nacional: 8 CENs × ~48 clientes (média)
  }

  // Cobertura (% clientes com interação nos últimos N dias)
  let coberturaEmDia, coberturaAviso, coberturaAtraso, coberturaCritico, coberturaNunca;
  if (perfilId === 'cen') {
    // Usa cálculo real do CARTEIRA_CEN
    const cont = { em_dia: 0, aviso: 0, atraso: 0, critico: 0, nunca: 0 };
    CARTEIRA_CEN.forEach(c => cont[statusCobertura(c)]++);
    coberturaEmDia = cont.em_dia;
    coberturaAviso = cont.aviso;
    coberturaAtraso = cont.atraso;
    coberturaCritico = cont.critico;
    coberturaNunca = cont.nunca;
  } else if (perfilId === 'gerente') {
    coberturaEmDia = 62;
    coberturaAviso = 24;
    coberturaAtraso = 18;
    coberturaCritico = 9;
    coberturaNunca = 5;
  } else {
    coberturaEmDia = 198;
    coberturaAviso = 78;
    coberturaAtraso = 58;
    coberturaCritico = 32;
    coberturaNunca = 14;
  }
  const coberturaTotal = coberturaEmDia + coberturaAviso + coberturaAtraso + coberturaCritico + coberturaNunca;
  const coberturaAtiva = coberturaEmDia + coberturaAviso;
  const coberturaPct = (coberturaAtiva / coberturaTotal) * 100;

  return {
    perfil: p,
    escopo_label: (drill && drill.nivel === 'filial') ? `Filial ${drill.valor}` : p.escopo_label,
    pracas,
    fatMes, metaMes,
    fatFYTD, metaFYTD,
    prevFY, metaFY,
    fat12m, prev12m, meta12m,
    clientesCarteira,
    coberturaEmDia, coberturaAviso, coberturaAtraso, coberturaCritico, coberturaNunca,
    coberturaTotal, coberturaAtiva, coberturaPct,
    totMercado, totVendemos, totPerdida, conhecimento,
    vendasPerdidas: VENDAS_PERDIDAS_MOTIVOS.map(m => ({
      ...m,
      qtd: Math.round(m.qtd * fatMult * (perfilId === 'diretor' ? 1 : 1)),
      valor: Math.round(m.valor * fatMult)
    })).filter(m => m.qtd > 0)
  };
}

// -------- Render principal --------
function renderVisao360() {
  const dados = agregar360(window.__perfil360, window.__drill360);
  const p = dados.perfil;

  return `
    <div class="v360-container">
      <!-- HEADER: toggle de perfil -->
      <div class="v360-header">
        <div class="v360-header-left">
          <h1 class="v360-title">Visão 360</h1>
          <div class="v360-subtitle">
            Dashboard executivo · <strong>${dados.escopo_label}</strong> · FY 2026 · atualizado hoje 08:45
          </div>
        </div>
        <div class="v360-perfil-toggle" role="tablist">
          ${Object.values(PERFIS_360).map(perfil => `
            <button
              class="v360-perfil-btn ${perfil.id === window.__perfil360 ? 'active' : ''}"
              onclick="selecionarPerfil360('${perfil.id}')"
              role="tab"
            >
              <div class="v360-perfil-avatar" style="background:${perfil.cor}">${perfil.avatar}</div>
              <div class="v360-perfil-info">
                <div class="v360-perfil-cargo">${perfil.cargo.split(' · ')[0]}</div>
                <div class="v360-perfil-nome">${perfil.nome}</div>
              </div>
            </button>
          `).join('')}
        </div>
      </div>

      <!-- BREADCRUMB DRILL DOWN -->
      ${renderDrill360Breadcrumb(dados)}

      <!-- ROW 1: 5 KPIs principais -->
      <div class="v360-kpi-row">
        ${renderKPI360('Faturamento do mês', fmtBRLcompact(dados.fatMes * 1000000), pctVs(dados.fatMes, dados.metaMes), 'vs meta mês', '#367C2B', 'trending-up')}
        ${renderKPI360('Previsão FY 2026', fmtBRLcompact(dados.prevFY * 1000000), pctVs(dados.prevFY, dados.metaFY), 'vs meta FY', '#1B5E20', 'target')}
        ${renderKPI360('Clientes na carteira', dados.clientesCarteira.toLocaleString('pt-BR'), null, window.__perfil360 === 'cen' ? 'sob minha responsabilidade' : (window.__perfil360 === 'gerente' ? 'em 5 CENs' : 'em 8 CENs · 4 filiais'), '#0EA5E9', 'users')}
        ${renderKPI360('Cobertura ativa', `${dados.coberturaPct.toFixed(0)}%`, null, `${dados.coberturaAtiva} de ${dados.coberturaTotal} em dia/aviso`, '#7C3AED', 'shield')}
        ${renderKPI360Conhecimento(dados)}
      </div>

      <!-- ROW 2: Gauge Conhecimento + Donut Cobertura + Linha Previsão -->
      <div class="v360-grid-row2">
        <div class="v360-card v360-card-md">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">Conhecimento de mercado</div>
              <div class="v360-card-sub">Máquinas emplacadas com registro nosso · FYTD</div>
            </div>
            <span class="v360-badge-fonte" title="Fonte: John Deere Connect (emplacamentos por praça) cruzada com Protheus">
              <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.2"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>
              JD Connect
            </span>
          </div>
          <div class="v360-gauge-wrap">
            <canvas id="v360GaugeConhec" width="240" height="140"></canvas>
            <div class="v360-gauge-legenda">
              <div class="v360-gauge-item"><span class="dot" style="background:#367C2B"></span>Vendemos <strong>${dados.totVendemos}</strong></div>
              <div class="v360-gauge-item"><span class="dot" style="background:#FFDE00"></span>Perda registrada <strong>${dados.totPerdida}</strong></div>
              <div class="v360-gauge-item"><span class="dot" style="background:#E5E7EB"></span>Sem conhecimento <strong>${dados.totMercado - dados.totVendemos - dados.totPerdida}</strong></div>
              <div class="v360-gauge-total">Total mercado: <strong>${dados.totMercado}</strong> máquinas</div>
            </div>
          </div>
          <div class="v360-card-footer v360-footer-warn">
            <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M12 9v4"/><path d="M12 17h.01"/><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/></svg>
            KPI depende de registro sistemático de perda no CRM. Baixa disciplina = número inflado.
          </div>
        </div>

        <div class="v360-card v360-card-md">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">Status da cobertura</div>
              <div class="v360-card-sub">Distribuição de ${dados.coberturaTotal} clientes por status</div>
            </div>
            <a href="#/cobertura" class="v360-link">Ver detalhes →</a>
          </div>
          <div class="v360-donut-wrap">
            <canvas id="v360DonutCobertura" width="200" height="200"></canvas>
            <div class="v360-donut-legenda">
              <div class="v360-legenda-item"><span class="dot" style="background:#367C2B"></span>Em dia <strong>${dados.coberturaEmDia}</strong></div>
              <div class="v360-legenda-item"><span class="dot" style="background:#B45309"></span>Aviso <strong>${dados.coberturaAviso}</strong></div>
              <div class="v360-legenda-item"><span class="dot" style="background:#DC2626"></span>Atraso <strong>${dados.coberturaAtraso}</strong></div>
              <div class="v360-legenda-item"><span class="dot" style="background:#7F1D1D"></span>Crítico <strong>${dados.coberturaCritico}</strong></div>
              <div class="v360-legenda-item"><span class="dot" style="background:#9CA3AF"></span>Nunca <strong>${dados.coberturaNunca}</strong></div>
            </div>
          </div>
        </div>

        <div class="v360-card v360-card-lg">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">Faturamento — 12 meses</div>
              <div class="v360-card-sub">Realizado vs Previsto vs Meta · R$ milhões</div>
            </div>
            <div class="v360-legenda-inline">
              <span><span class="line-swatch" style="background:#367C2B"></span>Realizado</span>
              <span><span class="line-swatch line-dashed" style="background:#0EA5E9"></span>Previsto</span>
              <span><span class="line-swatch" style="background:#9CA3AF"></span>Meta</span>
            </div>
          </div>
          <div class="v360-linha-wrap">
            <canvas id="v360LinhaFat" width="600" height="220"></canvas>
          </div>
        </div>
      </div>

      <!-- ROW 3: Top clientes + Top CENs/Filiais + Mix -->
      <div class="v360-grid-row3">
        <div class="v360-card v360-card-md">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">Top 5 clientes · FYTD</div>
              <div class="v360-card-sub">Maior faturamento acumulado no exercício</div>
            </div>
          </div>
          <div class="v360-topbar-list">
            ${renderTopClientes360(dados)}
          </div>
        </div>

        <div class="v360-card v360-card-md">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">${window.__perfil360 === 'diretor' ? 'Top filiais · FYTD' : 'Top CENs · FYTD'}</div>
              <div class="v360-card-sub">${window.__perfil360 === 'diretor' ? 'Clique para drill down' : 'Ranking por faturamento'}</div>
            </div>
          </div>
          <div class="v360-topbar-list">
            ${renderTopCensOuFiliais360(dados)}
          </div>
        </div>

        <div class="v360-card v360-card-md">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">Mix por linha</div>
              <div class="v360-card-sub">Participação no faturamento FYTD</div>
            </div>
          </div>
          <div class="v360-mix-wrap">
            <canvas id="v360MixLinha" width="180" height="180"></canvas>
            <div class="v360-mix-legenda">
              ${MIX_LINHAS.map(l => `
                <div class="v360-legenda-item">
                  <span class="dot" style="background:${l.cor}"></span>
                  <span class="v360-mix-linha">${l.linha}</span>
                  <strong>${l.pct}%</strong>
                </div>
              `).join('')}
            </div>
          </div>
        </div>
      </div>

      <!-- ROW 4: Vendas perdidas + Alertas -->
      <div class="v360-grid-row4">
        <div class="v360-card v360-card-lg">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">Vendas perdidas por motivo · 12 meses</div>
              <div class="v360-card-sub">Total: ${dados.vendasPerdidas.reduce((s,m) => s+m.qtd, 0)} negócios · ${fmtBRLcompact(dados.vendasPerdidas.reduce((s,m) => s+m.valor, 0))}</div>
            </div>
            <span class="v360-badge-fonte" title="Origem: registro do CEN no fechamento da oportunidade como perdida">
              <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.2"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>
              Registro CEN
            </span>
          </div>
          <div class="v360-perdidas-wrap">
            <canvas id="v360Perdidas" width="600" height="260"></canvas>
          </div>
        </div>

        <div class="v360-card v360-card-md">
          <div class="v360-card-header">
            <div>
              <div class="v360-card-title">Alertas gerenciais</div>
              <div class="v360-card-sub">Para atenção neste perfil</div>
            </div>
          </div>
          <div class="v360-alertas-list">
            ${ALERTAS_POR_PERFIL[window.__perfil360].map(a => `
              <div class="v360-alerta v360-alerta-${a.tipo}">
                <div class="v360-alerta-icon">
                  ${a.tipo === 'critico' ? '<svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.4"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>' : ''}
                  ${a.tipo === 'aviso' ? '<svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.4"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><path d="M12 9v4"/><path d="M12 17h.01"/></svg>' : ''}
                  ${a.tipo === 'info' ? '<svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.4"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>' : ''}
                </div>
                <div class="v360-alerta-body">
                  <div class="v360-alerta-titulo">${a.titulo}</div>
                  <div class="v360-alerta-detalhe">${a.detalhe}</div>
                </div>
                <button class="v360-alerta-acao" onclick="alertaAcao360('${a.acao}')">${a.acao}</button>
              </div>
            `).join('')}
          </div>
        </div>
      </div>

      <!-- Rodapé com nota -->
      <div class="v360-footer-nota">
        <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>
        Dados fictícios coerentes com Tracbel Agro · protótipo desktop 1280px+ · no MVP real, atualização via Fabric a cada 15 min.
      </div>
    </div>
  `;
}

function renderKPI360(titulo, valor, delta, subtexto, cor, icone) {
  const deltaEl = delta !== null ? `
    <div class="v360-kpi-delta ${delta >= 0 ? 'up' : 'down'}">
      ${delta >= 0 ? '↑' : '↓'} ${Math.abs(delta).toFixed(0)}%
    </div>
  ` : '';
  return `
    <div class="v360-kpi-card">
      <div class="v360-kpi-header">
        <div class="v360-kpi-icone" style="background:${cor}20;color:${cor}">
          ${iconeKPI360(icone)}
        </div>
        <div class="v360-kpi-titulo">${titulo}</div>
      </div>
      <div class="v360-kpi-valor">${valor}</div>
      <div class="v360-kpi-rodape">
        ${deltaEl}
        <span class="v360-kpi-sub">${subtexto}</span>
      </div>
    </div>
  `;
}

function renderKPI360Conhecimento(dados) {
  const cor = dados.conhecimento >= 70 ? '#367C2B' : (dados.conhecimento >= 55 ? '#B45309' : '#DC2626');
  return `
    <div class="v360-kpi-card v360-kpi-highlight">
      <div class="v360-kpi-header">
        <div class="v360-kpi-icone" style="background:${cor}20;color:${cor}">
          ${iconeKPI360('eye')}
        </div>
        <div class="v360-kpi-titulo">Conhecimento de mercado</div>
      </div>
      <div class="v360-kpi-valor" style="color:${cor}">${dados.conhecimento.toFixed(0)}%</div>
      <div class="v360-kpi-rodape">
        <span class="v360-kpi-sub">${dados.totVendemos + dados.totPerdida} de ${dados.totMercado} máquinas do mercado</span>
      </div>
    </div>
  `;
}

function iconeKPI360(nome) {
  const icones = {
    'trending-up': '<svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.2"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/></svg>',
    'target':      '<svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.2"><circle cx="12" cy="12" r="10"/><circle cx="12" cy="12" r="6"/><circle cx="12" cy="12" r="2"/></svg>',
    'users':       '<svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>',
    'shield':      '<svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>',
    'eye':         '<svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>'
  };
  return icones[nome] || '';
}

function pctVs(atual, meta) {
  if (!meta) return 0;
  return ((atual / meta) - 1) * 100;
}

function renderDrill360Breadcrumb(dados) {
  if (!window.__drill360) return '';
  return `
    <div class="v360-drill-bar">
      <span class="v360-drill-label">Drill down ativo:</span>
      <button class="v360-drill-chip" onclick="limparDrill360()">
        ${window.__drill360.valor}
        <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.4"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
      </button>
      <span class="v360-drill-hint">← clique para voltar ao escopo global</span>
    </div>
  `;
}

function renderTopClientes360(dados) {
  let topClientes;
  if (window.__perfil360 === 'cen') {
    // Top da carteira do João (real)
    topClientes = CARTEIRA_CEN
      .filter(c => c.fat_12m > 0)
      .sort((a, b) => b.fat_12m - a.fat_12m)
      .slice(0, 5)
      .map(c => ({
        razao: c.apelido,
        fat_fytd: c.fat_12m,
        cidade: `${c.cidade}/${c.uf}`,
        cen: 'João Ribeiro',
        cliente_id: c.id
      }));
  } else if (window.__perfil360 === 'gerente') {
    topClientes = TOP_CLIENTES_GLOBAL.filter(c => c.cidade.includes('MT')).slice(0, 5);
  } else {
    topClientes = TOP_CLIENTES_GLOBAL.slice(0, 5);
  }
  const max = Math.max(...topClientes.map(c => c.fat_fytd));

  return topClientes.map((c, i) => `
    <div class="v360-topbar-item" ${c.cliente_id === 84391 ? `onclick="window.location.hash='#/clientes/84391'" style="cursor:pointer"` : ''}>
      <div class="v360-topbar-rank">#${i+1}</div>
      <div class="v360-topbar-info">
        <div class="v360-topbar-nome">${c.razao}</div>
        <div class="v360-topbar-meta">${c.cidade} · ${c.cen}</div>
      </div>
      <div class="v360-topbar-valor-wrap">
        <div class="v360-topbar-valor">${fmtBRLcompact(c.fat_fytd)}</div>
        <div class="v360-topbar-progress">
          <div class="v360-topbar-fill" style="width:${(c.fat_fytd/max*100).toFixed(0)}%; background:${i === 0 ? '#367C2B' : i === 1 ? '#4A9040' : '#7CB342'}"></div>
        </div>
      </div>
    </div>
  `).join('');
}

function renderTopCensOuFiliais360(dados) {
  if (window.__perfil360 === 'diretor') {
    // Filiais
    const filiais = Object.entries(MERCADO_JD_PRACAS).map(([nome, m]) => {
      const pctFilial = m.total_mercado / Object.values(MERCADO_JD_PRACAS).reduce((s, v) => s + v.total_mercado, 0);
      const fatFYTD = dados.fat12m.slice(-5).reduce((s, v) => s + v, 0) * pctFilial;
      return { nome, fatFYTD, mercado: m.total_mercado, vendemos: m.vendemos };
    }).sort((a, b) => b.fatFYTD - a.fatFYTD);
    const max = Math.max(...filiais.map(f => f.fatFYTD));

    return filiais.map((f, i) => `
      <div class="v360-topbar-item" onclick="drillDownFilial360('${f.nome}')" style="cursor:pointer">
        <div class="v360-topbar-rank">#${i+1}</div>
        <div class="v360-topbar-info">
          <div class="v360-topbar-nome">Filial ${f.nome}</div>
          <div class="v360-topbar-meta">${f.vendemos} máquinas · ${f.mercado} mercado</div>
        </div>
        <div class="v360-topbar-valor-wrap">
          <div class="v360-topbar-valor">${fmtBRLcompact(f.fatFYTD * 1000000)}</div>
          <div class="v360-topbar-progress">
            <div class="v360-topbar-fill" style="width:${(f.fatFYTD/max*100).toFixed(0)}%; background:${i === 0 ? '#367C2B' : i === 1 ? '#4A9040' : '#7CB342'}"></div>
          </div>
        </div>
      </div>
    `).join('');
  }
  // NB: os valores fatFYTD acima estão em milhões (herdados de FATURAMENTO_12M), por isso o *1000000.
  // CEN ou Gerente: top CENs
  let cens;
  if (window.__perfil360 === 'gerente') {
    cens = PERF_CENS.filter(c => c.regional === 'MT Norte');
  } else {
    // CEN vê apenas si mesmo + comparativo com 4 pares
    cens = PERF_CENS.filter(c => c.regional === 'MT Norte').slice(0, 5);
  }
  // Estimar fat FYTD baseado no PERFIL de gerarSerieCEN (simplificado)
  const cenComFat = cens.map(c => {
    const perfilData = { joao: 3600000, rufino: 4400000, ana: 2900000, ricardo: 3200000, fernanda: 2100000 };
    const baseMes = perfilData[c.id] || 3200000;
    return {
      ...c,
      fatFYTD: baseMes * 5 // ~5 meses de FY (já em R$)
    };
  }).sort((a, b) => b.fatFYTD - a.fatFYTD);
  const maxCen = Math.max(...cenComFat.map(c => c.fatFYTD));

  return cenComFat.map((c, i) => `
    <div class="v360-topbar-item ${c.id === 'joao' && window.__perfil360 === 'cen' ? 'v360-eu' : ''}" ${window.__perfil360 === 'gerente' ? `onclick="drillDownCEN360('${c.id}','${c.nome.replace(/'/g,'')}')" style="cursor:pointer"` : ''}>
      <div class="v360-topbar-rank">#${i+1}</div>
      <div class="v360-topbar-avatar" style="background:${c.avatar}">${c.foto}</div>
      <div class="v360-topbar-info">
        <div class="v360-topbar-nome">${c.nome}${c.id === 'joao' && window.__perfil360 === 'cen' ? ' · você' : ''}</div>
        <div class="v360-topbar-meta">${c.regional}</div>
      </div>
      <div class="v360-topbar-valor-wrap">
        <div class="v360-topbar-valor">${fmtBRLcompact(c.fatFYTD)}</div>
        <div class="v360-topbar-progress">
          <div class="v360-topbar-fill" style="width:${(c.fatFYTD/maxCen*100).toFixed(0)}%; background:${i === 0 ? '#367C2B' : i === 1 ? '#4A9040' : '#7CB342'}"></div>
        </div>
      </div>
    </div>
  `).join('');
}

// -------- Ações de UI --------
window.selecionarPerfil360 = function(perfilId) {
  window.__perfil360 = perfilId;
  window.__drill360 = null; // reseta drill ao trocar perfil
  router();
};

window.drillDownFilial360 = function(filial) {
  window.__drill360 = { nivel: 'filial', valor: filial };
  router();
};

window.drillDownCEN360 = function(cenId, cenNome) {
  // Como o CEN só existe em Performance, redireciona para lá
  window.location.hash = '#/relatorios/performance';
  setTimeout(() => {
    // Se for João, tenta ativar o filtro na tela de Performance
    if (cenId === 'joao') return;
    // Para outros, apenas navega (deep-link real seria implementado no MVP)
  }, 100);
};

window.limparDrill360 = function() {
  window.__drill360 = null;
  router();
};

window.alertaAcao360 = function(acao) {
  const rotas = {
    'Ver Cobertura': '#/cobertura',
    'Ver Pipeline': '#/pipeline',
    'Ver oportunidade': '#/oportunidades/1517613',
    'Ver Performance': '#/relatorios/performance',
    'Ver detalhe': null,
    'Ver relatório': '#/relatorios/performance',
    'Ver por filial': null,
    'Ver perdidas': null,
    'Ver mix': null
  };
  const dest = rotas[acao];
  if (dest) window.location.hash = dest;
  else {
    // Toast informativo
    const toast = document.createElement('div');
    toast.className = 'novaop-toast';
    toast.style.borderLeftColor = '#0EA5E9';
    toast.innerHTML = `
      <div class="nt-icon" style="background:#0EA5E9">
        <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>
      </div>
      <div class="nt-body">
        <div class="nt-title">Tela ainda não implementada</div>
        <div class="nt-sub">"${acao}" — próxima iteração do protótipo</div>
      </div>
    `;
    document.body.appendChild(toast);
    setTimeout(() => toast.classList.add('show'), 50);
    setTimeout(() => toast.remove(), 2200);
  }
};

// -------- Mount: gráficos Chart.js --------
function mountVisao360() {
  const dados = agregar360(window.__perfil360, window.__drill360);

  // Gauge Conhecimento de Mercado (semicircle)
  const gaugeEl = document.getElementById('v360GaugeConhec');
  if (gaugeEl && window.Chart) {
    const ctx = gaugeEl.getContext('2d');
    const semConhec = dados.totMercado - dados.totVendemos - dados.totPerdida;
    new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Vendemos', 'Perda registrada', 'Sem conhecimento'],
        datasets: [{
          data: [dados.totVendemos, dados.totPerdida, semConhec],
          backgroundColor: ['#367C2B', '#FFDE00', '#E5E7EB'],
          borderWidth: 0
        }]
      },
      options: {
        responsive: false,
        rotation: -90,
        circumference: 180,
        cutout: '72%',
        plugins: { legend: { display: false }, tooltip: { enabled: true } }
      },
      plugins: [{
        id: 'gaugeCenterText',
        afterDraw(chart) {
          const {ctx, chartArea} = chart;
          if (!chartArea) return;
          const cx = (chartArea.left + chartArea.right) / 2;
          const cy = chartArea.bottom - 10;
          ctx.save();
          ctx.textAlign = 'center';
          ctx.font = '700 32px Inter, sans-serif';
          ctx.fillStyle = dados.conhecimento >= 70 ? '#367C2B' : (dados.conhecimento >= 55 ? '#B45309' : '#DC2626');
          ctx.fillText(`${dados.conhecimento.toFixed(0)}%`, cx, cy);
          ctx.font = '500 11px Inter, sans-serif';
          ctx.fillStyle = '#6B7280';
          ctx.fillText('conhecimento', cx, cy + 16);
          ctx.restore();
        }
      }]
    });
  }

  // Donut Cobertura
  const donutEl = document.getElementById('v360DonutCobertura');
  if (donutEl && window.Chart) {
    new Chart(donutEl.getContext('2d'), {
      type: 'doughnut',
      data: {
        labels: ['Em dia', 'Aviso', 'Atraso', 'Crítico', 'Nunca'],
        datasets: [{
          data: [dados.coberturaEmDia, dados.coberturaAviso, dados.coberturaAtraso, dados.coberturaCritico, dados.coberturaNunca],
          backgroundColor: ['#367C2B', '#B45309', '#DC2626', '#7F1D1D', '#9CA3AF'],
          borderWidth: 2,
          borderColor: '#FFFFFF'
        }]
      },
      options: {
        responsive: false,
        cutout: '62%',
        plugins: { legend: { display: false }, tooltip: { enabled: true } }
      },
      plugins: [{
        id: 'donutCenterCobertura',
        afterDraw(chart) {
          const {ctx, chartArea} = chart;
          if (!chartArea) return;
          const cx = (chartArea.left + chartArea.right) / 2;
          const cy = (chartArea.top + chartArea.bottom) / 2;
          ctx.save();
          ctx.textAlign = 'center';
          ctx.font = '700 22px Inter, sans-serif';
          ctx.fillStyle = '#111827';
          ctx.fillText(`${dados.coberturaPct.toFixed(0)}%`, cx, cy);
          ctx.font = '500 10px Inter, sans-serif';
          ctx.fillStyle = '#6B7280';
          ctx.fillText('ativa', cx, cy + 14);
          ctx.restore();
        }
      }]
    });
  }

  // Linha Faturamento 12m
  const linhaEl = document.getElementById('v360LinhaFat');
  if (linhaEl && window.Chart) {
    new Chart(linhaEl.getContext('2d'), {
      type: 'line',
      data: {
        labels: FATURAMENTO_12M.labels,
        datasets: [
          {
            label: 'Realizado',
            data: dados.fat12m,
            borderColor: '#367C2B',
            backgroundColor: 'rgba(54,124,43,0.10)',
            tension: 0.35,
            fill: true,
            borderWidth: 2.5,
            pointRadius: 3,
            pointHoverRadius: 5,
            pointBackgroundColor: '#367C2B'
          },
          {
            label: 'Previsto',
            data: dados.prev12m,
            borderColor: '#0EA5E9',
            borderDash: [5, 4],
            tension: 0.3,
            fill: false,
            borderWidth: 2,
            pointRadius: 2,
            pointHoverRadius: 4,
            pointBackgroundColor: '#0EA5E9'
          },
          {
            label: 'Meta',
            data: dados.meta12m,
            borderColor: '#9CA3AF',
            borderDash: [2, 3],
            tension: 0.2,
            fill: false,
            borderWidth: 1.5,
            pointRadius: 0
          }
        ]
      },
      options: {
        responsive: false,
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (item) => `${item.dataset.label}: R$ ${item.parsed.y.toFixed(1)}M`
            }
          }
        },
        scales: {
          y: {
            beginAtZero: false,
            grid: { color: 'rgba(0,0,0,0.05)' },
            ticks: { callback: (v) => `R$ ${v.toFixed(0)}M`, font: { size: 10 } }
          },
          x: {
            grid: { display: false },
            ticks: { font: { size: 10 } }
          }
        }
      }
    });
  }

  // Mix por linha
  const mixEl = document.getElementById('v360MixLinha');
  if (mixEl && window.Chart) {
    new Chart(mixEl.getContext('2d'), {
      type: 'doughnut',
      data: {
        labels: MIX_LINHAS.map(l => l.linha),
        datasets: [{
          data: MIX_LINHAS.map(l => l.pct),
          backgroundColor: MIX_LINHAS.map(l => l.cor),
          borderWidth: 2,
          borderColor: '#FFFFFF'
        }]
      },
      options: {
        responsive: false,
        cutout: '55%',
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (item) => `${item.label}: ${item.parsed}%`
            }
          }
        }
      }
    });
  }

  // Vendas Perdidas — barra horizontal
  const perdEl = document.getElementById('v360Perdidas');
  if (perdEl && window.Chart) {
    const dataPerd = [...dados.vendasPerdidas].sort((a, b) => b.qtd - a.qtd);
    new Chart(perdEl.getContext('2d'), {
      type: 'bar',
      data: {
        labels: dataPerd.map(m => m.motivo),
        datasets: [{
          label: 'Negócios perdidos',
          data: dataPerd.map(m => m.qtd),
          backgroundColor: dataPerd.map(m => m.cor),
          borderRadius: 4,
          borderWidth: 0
        }]
      },
      options: {
        responsive: false,
        indexAxis: 'y',
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (item) => {
                const m = dataPerd[item.dataIndex];
                return [`${m.qtd} negócios`, `Valor: ${fmtBRLcompact(m.valor)}`];
              }
            }
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            grid: { color: 'rgba(0,0,0,0.05)' },
            ticks: { font: { size: 10 } }
          },
          y: {
            grid: { display: false },
            ticks: { font: { size: 11 } }
          }
        }
      }
    });
  }
}
/* ============================================================
   ABA PÓS-VENDAS — Ficha do Cliente
   ------------------------------------------------------------
   Integra visão de pós-vendas ao cliente 84391 (Agroindustrial
   Salvador Arena). Cobre: faturamento peças/serviços, OS em
   aberto, PMPs (campanhas de campo John Deere), alertas críticos
   de telemetria/garantia, contratos JDCP, NPS pós-serviço.
   ============================================================ */

// ==== DADOS MOCK COERENTES COM O CLIENTE ====
const POSVENDAS_CLIENTE_84391 = {
  // Faturamento pós-vendas FYTD (ago/25 → jul/26) e YTD 2026
  fat_pv: {
    fytd_pecas: 480_000,
    fytd_servicos: 210_000,
    fytd_total: 690_000,
    ytd_2026_pecas: 312_000,
    ytd_2026_servicos: 148_000,
    ytd_2026_total: 460_000,
    ano_2025_total: 780_000,
    ano_2024_total: 620_000,
    ticket_medio_pecas: 8_450,
    mix_canal: {
      balcao: 42,       // % faturamento peças
      oficina_interna: 33,
      campo_tecnico: 25,
    },
  },

  // Série 12 meses (R$ mil) - realizado peças + serviços
  fat_12m: [
    { mes: 'set/25', pecas: 42, servicos: 18 },
    { mes: 'out/25', pecas: 51, servicos: 22 },
    { mes: 'nov/25', pecas: 38, servicos: 14 },
    { mes: 'dez/25', pecas: 29, servicos:  8 }, // baixa entressafra
    { mes: 'jan/26', pecas: 33, servicos: 12 },
    { mes: 'fev/26', pecas: 45, servicos: 19 },
    { mes: 'mar/26', pecas: 58, servicos: 26 },
    { mes: 'abr/26', pecas: 62, servicos: 28 }, // pico plantio
    { mes: 'mai/26', pecas: 39, servicos: 17 },
    { mes: 'jun/26', pecas: 41, servicos: 20 },
    { mes: 'jul/26', pecas: 34, servicos: 15 },
    { mes: 'ago/26', pecas: 28, servicos: 11 }, // parcial mês corrente
  ],

  // Ordens de Serviço em aberto (protheus - módulo oficina)
  os_abertas: [
    {
      id: 'OS-2026-04871',
      chassi: '1HDB44PXVMR345678',
      modelo: 'Plantadeira DB44',
      tipo: 'Corretiva',
      abertura: '2026-08-19',
      previsao: '2026-08-29',
      status: 'Aguardando peça',
      status_cor: 'amber',
      valor: 42_800,
      tecnico: 'Marcos Pereira',
      diagnostico: 'Ruído em caixa de transmissão + folga em cardan. Peças 3 dias em trânsito.',
      dias_aberta: 8,
      sla_dias: 10,
    },
    {
      id: 'OS-2026-04903',
      chassi: '1H0S770PXMR456789',
      modelo: 'Colheitadeira S770',
      tipo: 'Preventiva',
      abertura: '2026-08-22',
      previsao: '2026-08-30',
      status: 'Em execução',
      status_cor: 'green',
      valor: 18_450,
      tecnico: 'Anderson Lima',
      diagnostico: 'Revisão pré-safra 500h — troca de filtros, correias, calibração da mesa.',
      dias_aberta: 5,
      sla_dias: 7,
    },
    {
      id: 'OS-2026-04812',
      chassi: '1H4630PVXMR567890',
      modelo: 'Pulverizador 4630',
      tipo: 'Corretiva',
      abertura: '2026-08-11',
      previsao: '2026-08-21',
      status: 'Atrasada',
      status_cor: 'red',
      valor: 28_900,
      tecnico: 'Marcos Pereira',
      diagnostico: 'Vazamento no sistema hidráulico + falha eletrônica na barra. Peça importada em atraso na aduana.',
      dias_aberta: 16,
      sla_dias: 10,
    },
    {
      id: 'OS-2026-04945',
      chassi: '1RW7250PVMR123456',
      modelo: 'Trator 7250R',
      tipo: 'Garantia',
      abertura: '2026-08-25',
      previsao: '2026-09-01',
      status: 'Aguardando aprovação JD',
      status_cor: 'blue',
      valor: 0,  // garantia
      tecnico: 'Anderson Lima',
      diagnostico: 'Erro DTC 522.16 no motor. Sensor de pressão de rail. Sob análise JD Reman.',
      dias_aberta: 2,
      sla_dias: 5,
    },
  ],

  // PMP - Programa de Manutenção Preventiva (campanhas de campo John Deere)
  pmp_pendentes: [
    {
      id: 'PMP-2026-4471',
      codigo_jd: 'DFF-25-042',
      titulo: 'Atualização de software ECU — Colheitadeiras S780',
      criticidade: 'Alta',
      criticidade_cor: 'amber',
      chassi_afetado: ['1H0S780PXMR234567', '1H0S780PXMR234568'],
      modelo: 'Colheitadeira S780',
      qtd_equipamentos: 2,
      publicacao: '2026-07-15',
      prazo: '2026-09-30',
      dias_restantes: 34,
      cobertura: 'Cortesia (sem custo)',
      duracao_estimada: '2h por máquina',
      descricao: 'Boletim DFF-25-042: nova calibração do sistema HydraFlex melhora eficiência de trilha em 4-6%. Requer conexão JDLink + presença de técnico.',
      status: 'Não agendada',
      status_cor: 'amber',
    },
    {
      id: 'PMP-2026-4520',
      codigo_jd: 'DFF-26-011',
      titulo: 'Substituição preventiva — Filtro de particulados DPF',
      criticidade: 'Crítica',
      criticidade_cor: 'red',
      chassi_afetado: ['1RW7250PVMR123457'],
      modelo: 'Trator 7250R',
      qtd_equipamentos: 1,
      publicacao: '2026-08-05',
      prazo: '2026-09-05',
      dias_restantes: 9,
      cobertura: 'Cortesia (sem custo)',
      duracao_estimada: '4h',
      descricao: 'Recall JD DFF-26-011: filtro DPF de lote afetado pode causar entupimento em regime severo. Substituição obrigatória para manter garantia.',
      status: 'Agendada',
      status_cor: 'green',
      agenda: '2026-08-30',
    },
    {
      id: 'PMP-2026-4388',
      codigo_jd: 'PIP-25-098',
      titulo: 'Programa Fidelidade Peças Genuínas — check-up cortesia',
      criticidade: 'Baixa',
      criticidade_cor: 'info',
      chassi_afetado: [
        '1RW7250PVMR123456','1RW7250PVMR123457',
        '1H0S780PXMR234567','1H0S780PXMR234568',
        '1HDB44PXVMR345678','1H0S770PXMR456789','1H4630PVXMR567890',
      ],
      modelo: 'Todos',
      qtd_equipamentos: 7,
      publicacao: '2026-06-01',
      prazo: '2026-12-31',
      dias_restantes: 126,
      cobertura: 'Cortesia (sem custo)',
      duracao_estimada: '1h por máquina',
      descricao: 'Programa JD 2026: cliente elegível a check-up gratuito da frota. Oportunidade comercial de identificar necessidades de peças/revisão antes da safra.',
      status: 'Não agendada',
      status_cor: 'gray',
    },
  ],

  // Alertas críticos (telemetria JDLink + análises pós-venda)
  alertas_criticos: [
    {
      id: 'ALT-01',
      tipo: 'telemetria',
      cor: 'red',
      titulo: 'DTC 522.16 recorrente — Trator 7250R (chassi ...123456)',
      detalhe: '3 ocorrências em 15 dias. OS-2026-04945 aberta em análise pela JD Reman. Cliente sem histórico prévio de falhas eletrônicas.',
      origem: 'JDLink',
      criado_em: '2026-08-25T09:14',
      acao_cta: 'Ver OS',
      acao_link: '#/pos-vendas/os/04945',
    },
    {
      id: 'ALT-02',
      tipo: 'garantia',
      cor: 'amber',
      titulo: 'Trator 7250R (chassi ...123456) — garantia vence em 267 dias',
      detalhe: 'Elegível para extensão JDCP Premium por 24 meses. Cliente Classe A · zero inadimplência. Oportunidade estimada R$ 42.000.',
      origem: 'Motor de regras',
      criado_em: '2026-08-20T00:00',
      acao_cta: 'Gerar orçamento JDCP',
      acao_link: '#',
    },
    {
      id: 'ALT-03',
      tipo: 'PMP',
      cor: 'red',
      titulo: 'PMP DFF-26-011 obrigatória — prazo em 9 dias',
      detalhe: 'Trator 7250R (chassi ...123457) precisa da substituição de DPF. Já agendado 30/08. Confirmação com Carlos Eduardo (Gerente de Frota) pendente.',
      origem: 'John Deere · Boletim técnico',
      criado_em: '2026-08-05T00:00',
      acao_cta: 'Confirmar agendamento',
      acao_link: '#',
    },
    {
      id: 'ALT-04',
      tipo: 'ciclo_pv',
      cor: 'amber',
      titulo: 'Sem visita pós-venda há 41 dias',
      detalhe: 'Última visita técnica em 01/08 (Ricardo Moretti). SLA da Tracbel para Classe A: 30 dias. Convém agendar visita antes da safra iniciar (setembro).',
      origem: 'Tracbel · SLA relacionamento',
      criado_em: '2026-08-27T08:45',
      acao_cta: 'Agendar visita',
      acao_link: '#',
    },
  ],

  // Contratos JDCP
  contratos_jdcp: [
    { chassi: '1H0S780PXMR234567', modelo: 'Colheitadeira S780', plano: 'JDCP Premium', inicio: '2024-03-15', fim: '2027-03-15', horas_cobertas: '3000h', valor_anual: 38_500 },
    { chassi: '1H0S780PXMR234568', modelo: 'Colheitadeira S780', plano: 'JDCP Premium', inicio: '2024-03-15', fim: '2027-03-15', horas_cobertas: '3000h', valor_anual: 38_500 },
  ],
  // Equipamentos sem contrato JDCP (oportunidade comercial pós-venda)
  frota_sem_jdcp: [
    { chassi: '1RW7250PVMR123456', modelo: 'Trator 7250R', ano: 2023, potencial: 42_000 },
    { chassi: '1RW7250PVMR123457', modelo: 'Trator 7250R', ano: 2023, potencial: 42_000 },
  ],

  // NPS pós-serviço últimos 12 meses
  nps: {
    score: 72,
    respostas: 14,
    detratores: 1,
    neutros: 2,
    promotores: 11,
    comentario_recente: {
      data: '2026-08-01',
      autor: 'Salvador Arena Jr.',
      nota: 10,
      texto: 'Atendimento do Ricardo é excelente. Peças chegaram no prazo prometido. Só melhorar a comunicação de status.',
    },
  },
};

// ==== RENDER ABA PÓS-VENDAS ====
function renderPosVendasTab() {
  const p = POSVENDAS_CLIENTE_84391;
  const criticos = p.alertas_criticos.filter(a => a.cor === 'red').length;
  const atrasadas = p.os_abertas.filter(o => o.status_cor === 'red').length;

  return `
    <div class="tab-content" data-tab-content="pos-vendas">

      <!-- ROW 1: KPIs pós-vendas -->
      <div class="pv-kpi-row">
        <div class="pv-kpi pv-kpi-primary">
          <div class="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
            <span>Faturamento pós-vendas FYTD</span>
          </div>
          <div class="pv-kpi-valor">${fmtBRL(p.fat_pv.fytd_total)}</div>
          <div class="pv-kpi-hint">
            <span class="pv-kpi-split"><b>${fmtBRLcompact(p.fat_pv.fytd_pecas)}</b> peças</span>
            <span class="pv-kpi-sep">·</span>
            <span class="pv-kpi-split"><b>${fmtBRLcompact(p.fat_pv.fytd_servicos)}</b> serviços</span>
          </div>
          <div class="pv-kpi-sub">2025: ${fmtBRL(p.fat_pv.ano_2025_total)} · 2024: ${fmtBRL(p.fat_pv.ano_2024_total)}</div>
        </div>
        <div class="pv-kpi">
          <div class="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
            <span>Ordens de serviço abertas</span>
          </div>
          <div class="pv-kpi-valor">${p.os_abertas.length}</div>
          <div class="pv-kpi-hint">
            ${atrasadas > 0
              ? `<span class="pv-badge pv-badge-red">${atrasadas} atrasada${atrasadas > 1 ? 's' : ''}</span>`
              : '<span class="pv-badge pv-badge-green">todas no prazo</span>'}
            <span class="pv-kpi-sep">·</span>
            <span>${fmtBRLcompact(p.os_abertas.reduce((s,o) => s + o.valor, 0))} em execução</span>
          </div>
        </div>
        <div class="pv-kpi">
          <div class="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
            <span>PMPs pendentes</span>
          </div>
          <div class="pv-kpi-valor">${p.pmp_pendentes.length}</div>
          <div class="pv-kpi-hint">
            ${p.pmp_pendentes.filter(x => x.criticidade === 'Crítica').length > 0
              ? `<span class="pv-badge pv-badge-red">${p.pmp_pendentes.filter(x => x.criticidade === 'Crítica').length} crítica</span>`
              : ''}
            <span>${p.pmp_pendentes.reduce((s,x) => s + x.qtd_equipamentos, 0)} equipamentos afetados</span>
          </div>
        </div>
        <div class="pv-kpi ${criticos > 0 ? 'pv-kpi-alert' : ''}">
          <div class="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
            <span>Alertas críticos</span>
          </div>
          <div class="pv-kpi-valor">${p.alertas_criticos.length}</div>
          <div class="pv-kpi-hint">
            <span class="pv-badge pv-badge-red">${criticos} críticos</span>
            <span class="pv-kpi-sep">·</span>
            <span>${p.alertas_criticos.length - criticos} atenção</span>
          </div>
        </div>
        <div class="pv-kpi">
          <div class="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 11h-6M20 8v6"/></svg>
            <span>NPS pós-serviço · 12m</span>
          </div>
          <div class="pv-kpi-valor" style="color:${p.nps.score >= 70 ? 'var(--jd-green)' : p.nps.score >= 50 ? '#F59E0B' : '#DC2626'}">${p.nps.score}</div>
          <div class="pv-kpi-hint">${p.nps.respostas} respostas · ${p.nps.promotores} promotores</div>
        </div>
      </div>

      <!-- ROW 2: Gráfico 12m + Mix canal -->
      <div class="pv-row-2">
        <div class="card pv-card-grafico">
          <div class="card-header">
            <div>
              <div class="card-title">Faturamento pós-vendas — 12 meses</div>
              <div class="card-subtitle">Peças vs Serviços · R$ mil</div>
            </div>
            <div class="pv-legend-inline">
              <span class="pv-legend-item"><span class="pv-dot" style="background:#367C2B"></span>Peças</span>
              <span class="pv-legend-item"><span class="pv-dot" style="background:#FFDE00"></span>Serviços</span>
            </div>
          </div>
          <div style="padding:16px 20px 20px">
            <canvas id="pv-fat-12m" style="height:220px;max-height:220px"></canvas>
          </div>
        </div>
        <div class="card pv-card-mix">
          <div class="card-header">
            <div>
              <div class="card-title">Canal de faturamento</div>
              <div class="card-subtitle">Peças FYTD por origem</div>
            </div>
          </div>
          <div style="padding:16px 20px 20px">
            <canvas id="pv-mix-canal" style="height:220px;max-height:220px"></canvas>
            <div class="pv-ticket-medio">
              <span class="pv-ticket-label">Ticket médio de peças</span>
              <span class="pv-ticket-valor">${fmtBRL(p.fat_pv.ticket_medio_pecas)}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- ROW 3: Alertas críticos -->
      <div class="card">
        <div class="card-header">
          <div>
            <div class="card-title">Alertas de pós-vendas</div>
            <div class="card-subtitle">Telemetria JDLink · garantia · ciclo de relacionamento · boletins JD</div>
          </div>
          <span class="pv-badge pv-badge-neutral">${p.alertas_criticos.length} ativos</span>
        </div>
        <div class="pv-alertas-lista">
          ${p.alertas_criticos.map(a => `
            <div class="pv-alerta pv-alerta-${a.cor}">
              <div class="pv-alerta-icon">
                ${a.cor === 'red'
                  ? '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>'
                  : a.cor === 'amber'
                  ? '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>'
                  : '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'}
              </div>
              <div class="pv-alerta-corpo">
                <div class="pv-alerta-titulo">${a.titulo}</div>
                <div class="pv-alerta-detalhe">${a.detalhe}</div>
                <div class="pv-alerta-meta">
                  <span class="pv-alerta-origem">${a.origem}</span>
                  <span class="pv-kpi-sep">·</span>
                  <span>${new Date(a.criado_em).toLocaleDateString('pt-BR')}</span>
                </div>
              </div>
              <div class="pv-alerta-acao">
                <button class="btn btn-secondary btn-sm" onclick="alert('Ação do protótipo: ${a.acao_cta}')">${a.acao_cta}</button>
              </div>
            </div>
          `).join('')}
        </div>
      </div>

      <!-- ROW 4: OS abertas -->
      <div class="card" style="margin-top:20px">
        <div class="card-header">
          <div>
            <div class="card-title">Ordens de serviço em aberto</div>
            <div class="card-subtitle">Módulo Oficina · Protheus · atualizado 08:45</div>
          </div>
          <button class="btn btn-ghost btn-sm">Ver todas as OS</button>
        </div>
        <div style="overflow-x:auto">
          <table class="pv-table">
            <thead>
              <tr>
                <th>OS</th>
                <th>Equipamento</th>
                <th>Tipo</th>
                <th>Aberta</th>
                <th>Prazo</th>
                <th>Status</th>
                <th>Técnico</th>
                <th class="num">Valor</th>
              </tr>
            </thead>
            <tbody>
              ${p.os_abertas.map(o => `
                <tr>
                  <td><span class="pv-mono">${o.id}</span></td>
                  <td>
                    <div class="pv-eqp-info">
                      <span class="pv-eqp-modelo">${o.modelo}</span>
                      <span class="pv-eqp-chassi">${o.chassi.slice(-6)}</span>
                    </div>
                  </td>
                  <td>${o.tipo === 'Garantia' ? '<span class="pv-badge pv-badge-blue">Garantia</span>' : o.tipo}</td>
                  <td>${new Date(o.abertura).toLocaleDateString('pt-BR')} <span class="pv-tenue">(${o.dias_aberta}d)</span></td>
                  <td>${new Date(o.previsao).toLocaleDateString('pt-BR')}</td>
                  <td><span class="pv-status pv-status-${o.status_cor}">${o.status}</span></td>
                  <td>${o.tecnico}</td>
                  <td class="num pv-mono">${o.valor > 0 ? fmtBRLcompact(o.valor) : '<span class="pv-tenue">—</span>'}</td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>

      <!-- ROW 5: PMP -->
      <div class="card" style="margin-top:20px">
        <div class="card-header">
          <div>
            <div class="card-title">PMPs · Campanhas de campo John Deere</div>
            <div class="card-subtitle">Programa de Manutenção Preventiva + Boletins técnicos (DFF/PIP) · fonte: portal JD Dealer</div>
          </div>
          <button class="btn btn-ghost btn-sm">Histórico de PMPs concluídas</button>
        </div>
        <div class="pv-pmp-lista">
          ${p.pmp_pendentes.map(pmp => `
            <div class="pv-pmp">
              <div class="pv-pmp-lado">
                <span class="pv-pmp-cod">${pmp.codigo_jd}</span>
                <span class="pv-pmp-badge pv-pmp-crit-${pmp.criticidade_cor}">${pmp.criticidade}</span>
              </div>
              <div class="pv-pmp-corpo">
                <div class="pv-pmp-titulo">${pmp.titulo}</div>
                <div class="pv-pmp-desc">${pmp.descricao}</div>
                <div class="pv-pmp-meta">
                  <span><b>${pmp.qtd_equipamentos}</b> ${pmp.qtd_equipamentos === 1 ? 'equipamento' : 'equipamentos'} · ${pmp.modelo}</span>
                  <span class="pv-kpi-sep">·</span>
                  <span>${pmp.cobertura}</span>
                  <span class="pv-kpi-sep">·</span>
                  <span>Duração: ${pmp.duracao_estimada}</span>
                  <span class="pv-kpi-sep">·</span>
                  <span class="pv-pmp-prazo pv-pmp-prazo-${pmp.dias_restantes < 15 ? 'red' : pmp.dias_restantes < 45 ? 'amber' : 'green'}">
                    Prazo ${new Date(pmp.prazo).toLocaleDateString('pt-BR')} · <b>${pmp.dias_restantes}d restantes</b>
                  </span>
                </div>
              </div>
              <div class="pv-pmp-acao">
                <span class="pv-status pv-status-${pmp.status_cor}">${pmp.status}${pmp.agenda ? ` · ${new Date(pmp.agenda).toLocaleDateString('pt-BR')}` : ''}</span>
                <button class="btn btn-secondary btn-sm" onclick="alert('${pmp.status === 'Agendada' ? 'Ver detalhes do agendamento' : 'Agendar PMP com Carlos Eduardo (Gerente de Frota)'}')">
                  ${pmp.status === 'Agendada' ? 'Ver detalhes' : 'Agendar'}
                </button>
              </div>
            </div>
          `).join('')}
        </div>
      </div>

      <!-- ROW 6: Contratos JDCP + oportunidade -->
      <div class="pv-row-6">
        <div class="card">
          <div class="card-header">
            <div>
              <div class="card-title">Contratos JDCP ativos</div>
              <div class="card-subtitle">John Deere Complete Protection · cobertura estendida</div>
            </div>
            <span class="pv-badge pv-badge-green">${p.contratos_jdcp.length} de ${p.contratos_jdcp.length + p.frota_sem_jdcp.length} equipamentos</span>
          </div>
          <table class="pv-table">
            <thead>
              <tr>
                <th>Equipamento</th>
                <th>Plano</th>
                <th>Vigência</th>
                <th class="num">Valor/ano</th>
              </tr>
            </thead>
            <tbody>
              ${p.contratos_jdcp.map(c => `
                <tr>
                  <td>
                    <div class="pv-eqp-info">
                      <span class="pv-eqp-modelo">${c.modelo}</span>
                      <span class="pv-eqp-chassi">${c.chassi.slice(-6)}</span>
                    </div>
                  </td>
                  <td><span class="pv-badge pv-badge-blue">${c.plano}</span></td>
                  <td>${new Date(c.inicio).toLocaleDateString('pt-BR')} → ${new Date(c.fim).toLocaleDateString('pt-BR')}</td>
                  <td class="num pv-mono">${fmtBRLcompact(c.valor_anual)}</td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
        <div class="card pv-card-oportunidade">
          <div class="card-header">
            <div>
              <div class="card-title">Oportunidade pós-venda</div>
              <div class="card-subtitle">Frota sem contrato JDCP</div>
            </div>
          </div>
          <div class="pv-op-corpo">
            <div class="pv-op-valor">
              <span class="pv-op-label">Potencial anual estimado</span>
              <span class="pv-op-num">${fmtBRLcompact(p.frota_sem_jdcp.reduce((s,x) => s + x.potencial, 0))}</span>
            </div>
            <ul class="pv-op-lista">
              ${p.frota_sem_jdcp.map(e => `
                <li>
                  <span>${e.modelo} · ${e.ano} · chassi ...${e.chassi.slice(-6)}</span>
                  <span class="pv-mono">${fmtBRLcompact(e.potencial)}/ano</span>
                </li>
              `).join('')}
            </ul>
            <button class="btn btn-primary btn-sm" style="width:100%">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              Criar proposta JDCP
            </button>
          </div>
        </div>
      </div>

      <!-- ROW 7: NPS comentário -->
      <div class="card pv-card-nps" style="margin-top:20px">
        <div class="card-header">
          <div>
            <div class="card-title">Último feedback do cliente</div>
            <div class="card-subtitle">NPS pós-serviço · ${new Date(p.nps.comentario_recente.data).toLocaleDateString('pt-BR')}</div>
          </div>
          <div class="pv-nps-nota">
            <span class="pv-nps-num">${p.nps.comentario_recente.nota}</span>
            <span class="pv-nps-max">/10</span>
          </div>
        </div>
        <div class="pv-nps-corpo">
          <div class="pv-nps-autor">${p.nps.comentario_recente.autor}</div>
          <div class="pv-nps-texto">"${p.nps.comentario_recente.texto}"</div>
        </div>
      </div>

      <div class="pv-footer-note">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
        <span>
          Fontes: <b>Protheus</b> (OS, faturamento, contratos) · <b>JDLink</b> (telemetria, DTCs) · <b>Portal JD Dealer</b> (PMPs, boletins DFF/PIP) · <b>Fabric</b> (agregações). Sincronização a cada 15 minutos no MVP.
        </span>
      </div>

    </div>
  `;
}

// ==== MOUNT DOS GRÁFICOS DA ABA PÓS-VENDAS ====
function mountPosVendasCharts() {
  const p = POSVENDAS_CLIENTE_84391;

  // Gráfico 1: faturamento 12m barras empilhadas peças + serviços
  const el1 = document.getElementById('pv-fat-12m');
  if (el1 && !el1.__mounted) {
    el1.__mounted = true;
    new Chart(el1, {
      type: 'bar',
      data: {
        labels: p.fat_12m.map(x => x.mes),
        datasets: [
          {
            label: 'Peças',
            data: p.fat_12m.map(x => x.pecas),
            backgroundColor: '#367C2B',
            borderRadius: 4,
          },
          {
            label: 'Serviços',
            data: p.fat_12m.map(x => x.servicos),
            backgroundColor: '#FFDE00',
            borderRadius: 4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (ctx) => `${ctx.dataset.label}: R$ ${ctx.parsed.y}k`,
            },
          },
        },
        scales: {
          x: { stacked: true, grid: { display: false }, ticks: { font: { size: 11 } } },
          y: {
            stacked: true,
            beginAtZero: true,
            grid: { color: 'rgba(0,0,0,0.05)' },
            ticks: {
              font: { size: 11 },
              callback: (v) => `R$ ${v}k`,
            },
          },
        },
      },
    });
  }

  // Gráfico 2: donut mix canal
  const el2 = document.getElementById('pv-mix-canal');
  if (el2 && !el2.__mounted) {
    el2.__mounted = true;
    new Chart(el2, {
      type: 'doughnut',
      data: {
        labels: ['Balcão', 'Oficina interna', 'Campo (técnico)'],
        datasets: [{
          data: [p.fat_pv.mix_canal.balcao, p.fat_pv.mix_canal.oficina_interna, p.fat_pv.mix_canal.campo_tecnico],
          backgroundColor: ['#367C2B', '#7CB342', '#C5E1A5'],
          borderWidth: 0,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '62%',
        plugins: {
          legend: {
            position: 'right',
            labels: {
              font: { size: 12 },
              padding: 12,
              generateLabels: (chart) => {
                const data = chart.data;
                return data.labels.map((label, i) => ({
                  text: `${label}  ${data.datasets[0].data[i]}%`,
                  fillStyle: data.datasets[0].backgroundColor[i],
                  strokeStyle: data.datasets[0].backgroundColor[i],
                  hidden: false,
                  index: i,
                }));
              },
            },
          },
          tooltip: {
            callbacks: {
              label: (ctx) => `${ctx.label}: ${ctx.parsed}%`,
            },
          },
        },
      },
    });
  }
}
