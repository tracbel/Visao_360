
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
  const pipe_aberto = fytd_pipe - fytd_vendas;

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
