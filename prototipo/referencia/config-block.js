
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
