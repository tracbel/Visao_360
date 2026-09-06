
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
