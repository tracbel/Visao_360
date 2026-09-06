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
