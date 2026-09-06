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
        ${renderKPI360('Faturamento do mês', fmtBRLcompact(dados.fatMes), pctVs(dados.fatMes, dados.metaMes), 'vs meta mês', '#367C2B', 'trending-up')}
        ${renderKPI360('Previsão FY 2026', fmtBRLcompact(dados.prevFY), pctVs(dados.prevFY, dados.metaFY), 'vs meta FY', '#1B5E20', 'target')}
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
      fatFYTD: baseMes * 5 // ~5 meses de FY
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
