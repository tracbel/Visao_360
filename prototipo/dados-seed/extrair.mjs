#!/usr/bin/env node
/**
 * extrair.mjs — Extrator reexecutável dos mocks do protótipo CRM Tracbel Agro.
 *
 * Carrega `prototipo/referencia/assets/app.js` dentro de um contexto isolado
 * (`node:vm`) com stubs de window/document/localStorage/Chart/L e serializa
 * cada constante de dados para um arquivo JSON em `prototipo/dados-seed/`.
 *
 * Truque central: declarações `const`/`let` de topo em `vm.runInContext` não
 * viram propriedades do objeto de contexto — elas moram no *global lexical
 * scope* do contexto, que sobrevive entre chamadas. Por isso rodamos o app.js
 * primeiro e, em seguida, um segundo script no MESMO contexto que apenas
 * referencia os identificadores e devolve um objeto com eles.
 *
 * Uso:
 *   node prototipo/dados-seed/extrair.mjs
 *   node prototipo/dados-seed/extrair.mjs --check   (não escreve, só valida)
 */

import fs from 'node:fs';
import path from 'node:path';
import vm from 'node:vm';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const RAIZ = path.resolve(__dirname, '..', '..');
const APP_JS = path.join(RAIZ, 'prototipo', 'referencia', 'assets', 'app.js');
const SAIDA = __dirname;
const APENAS_CHECAR = process.argv.includes('--check');

// ---------------------------------------------------------------------------
// 1. Stubs de ambiente de browser
// ---------------------------------------------------------------------------

function criarElementoFalso() {
  const el = {
    style: {},
    dataset: {},
    classList: { add() {}, remove() {}, toggle() {}, contains: () => false },
    children: [],
    innerHTML: '',
    textContent: '',
    value: '',
    checked: false,
    addEventListener() {},
    removeEventListener() {},
    appendChild(c) { el.children.push(c); return c; },
    removeChild() {},
    remove() {},
    setAttribute() {},
    getAttribute: () => null,
    querySelector: () => null,
    querySelectorAll: () => [],
    closest: () => null,
    getBoundingClientRect: () => ({ top: 0, left: 0, width: 0, height: 0, right: 0, bottom: 0 }),
    getContext: () => ({}),
    insertBefore() {},
    focus() {},
    scrollIntoView() {},
  };
  return el;
}

function criarLocalStorageFalso() {
  const mapa = new Map();
  return {
    getItem: (k) => (mapa.has(k) ? mapa.get(k) : null),
    setItem: (k, v) => { mapa.set(k, String(v)); },
    removeItem: (k) => { mapa.delete(k); },
    clear: () => mapa.clear(),
    key: (i) => Array.from(mapa.keys())[i] ?? null,
    get length() { return mapa.size; },
  };
}

function ChartFalso() { return { destroy() {}, update() {}, data: {}, options: {} }; }
ChartFalso.register = () => {};
ChartFalso.defaults = { font: {}, plugins: {}, scales: {} };
ChartFalso.getChart = () => null;

const LeafletFalso = {
  map: () => ({
    setView() { return this; }, addLayer() { return this; }, removeLayer() {},
    fitBounds() {}, on() {}, remove() {}, invalidateSize() {},
  }),
  tileLayer: () => ({ addTo() { return this; } }),
  layerGroup: () => ({ addTo() { return this; }, clearLayers() {}, addLayer() {} }),
  marker: () => ({ addTo() { return this; }, bindPopup() { return this; }, on() { return this; } }),
  divIcon: () => ({}),
  latLngBounds: () => ({ extend() {}, isValid: () => false }),
  Icon: { Default: { prototype: {}, mergeOptions() {} } },
};

function criarContexto() {
  const documento = {
    readyState: 'complete',
    body: criarElementoFalso(),
    documentElement: criarElementoFalso(),
    head: criarElementoFalso(),
    getElementById: () => null,
    querySelector: () => null,
    querySelectorAll: () => [],
    createElement: () => criarElementoFalso(),
    createElementNS: () => criarElementoFalso(),
    createTextNode: () => criarElementoFalso(),
    addEventListener() {},
    removeEventListener() {},
    getElementsByClassName: () => [],
    getElementsByTagName: () => [],
    fonts: { ready: Promise.resolve(), load: () => Promise.resolve() },
  };

  const janela = {
    location: { hash: '#/', href: 'http://localhost/', pathname: '/', search: '' },
    document: documento,
    localStorage: criarLocalStorageFalso(),
    sessionStorage: criarLocalStorageFalso(),
    addEventListener() {},
    removeEventListener() {},
    scrollTo() {},
    requestAnimationFrame: (fn) => setTimeout(fn, 0),
    cancelAnimationFrame: () => {},
    matchMedia: () => ({ matches: false, addListener() {}, removeListener() {} }),
    innerWidth: 1440,
    innerHeight: 900,
    devicePixelRatio: 1,
    getComputedStyle: () => ({ getPropertyValue: () => '', cssText: '' }),
    Chart: ChartFalso,
    L: LeafletFalso,
    console,
  };
  janela.window = janela;
  janela.self = janela;
  janela.top = janela;
  janela.globalThis = janela;

  const ctx = vm.createContext(janela);
  // Globais que o app.js usa sem prefixo `window.`
  Object.assign(ctx, {
    document: documento,
    localStorage: janela.localStorage,
    sessionStorage: janela.sessionStorage,
    Chart: ChartFalso,
    L: LeafletFalso,
    console,
    setTimeout,
    clearTimeout,
    setInterval,
    clearInterval,
    requestAnimationFrame: janela.requestAnimationFrame,
    cancelAnimationFrame: janela.cancelAnimationFrame,
    navigator: { userAgent: 'node', language: 'pt-BR', languages: ['pt-BR'] },
    alert() {}, confirm: () => true, prompt: () => null,
    Intl, JSON, Math, Date, URL, TextEncoder, TextDecoder,
  });
  return ctx;
}

// ---------------------------------------------------------------------------
// 2. Mapa constante -> arquivo de saída
//    (nome da constante, arquivo, descrição curta, linha aproximada em app.js)
// ---------------------------------------------------------------------------

const MAPA = [
  ['ROUTES',                     'routes.json',                        'Tabela de rotas do router por hash', 3],
  ['FUNIL_DATA',                 'funil.json',                         'Relatório Funil de Vendas do Mês', 281],
  ['COBERTURA_DATA',             'cobertura.json',                     'Painel Regional de Cobertura de Carteira', 693],
  ['AGENDA_DATA',                'agenda.json',                        'Agenda do CEN (tarefas)', 1125],
  ['TIPO_META',                  'tipo-meta.json',                     'Taxonomia de tipo de tarefa da agenda', 1173],
  ['CLIENTE_DATA',               'cliente-84391.json',                 'Ficha 360 do cliente 84391', 1634],
  ['EQUIPAMENTO_DATA',           'equipamento-1RW7250PVMR123456.json', 'Ficha do equipamento por chassi', 2212],
  ['OPORTUNIDADE_DATA',          'oportunidade-1517613.json',          'Ficha da oportunidade OP-2026-08471', 2712],
  ['META_FREQ',                  'meta-frequencia.json',               'Meta de frequência de contato por classe ABCD', 3376],
  ['CAT_INTERACAO',              'config-categorias-interacao.json',   'Categorias de interação (cobertura)', 3379],
  ['CARTEIRA_CEN',               'carteira-cen.json',                  'Carteira de clientes do CEN', 3389],
  ['STATUS_LABELS',              'status-cobertura-labels.json',       'Rótulos de status de cobertura', 3445],
  ['CARTEIRA_STATE',             'estado-carteira.json',               'Estado inicial dos filtros da Cobertura', 3718],
  ['CIDADE_LATLNG',              'cidades-latlng.json',                'Geocodificação das cidades do mapa', 3750],
  ['PIPELINE_FASES',             'pipeline-fases.json',                'Fases do kanban de pipeline', 4049],
  ['CENS',                       'cens.json',                          'Cadastro de CENs (consultores)', 4059],
  ['LINHA_ICON',                 'linha-icon.json',                    'Emoji por linha de produto', 4071],
  ['PIPELINE_MOCK',              'pipeline.json',                      'Oportunidades do kanban', 4081],
  ['PIPELINE_STATE',             'estado-pipeline.json',               'Estado inicial dos filtros do Pipeline', 4130],
  ['CLIENTES_EXTRA',             'clientes-extra.json',                'Enriquecimento da carteira para a lista de Clientes', 4622],
  ['CLIENTES_OUTRAS',            'clientes-outras.json',               'Clientes de outras regionais/CENs', 4651],
  ['CLIENTES_STATE',             'estado-clientes.json',               'Estado inicial dos filtros de Clientes', 4721],
  ['CONFIG_STATE',               'estado-config.json',                 'Estado inicial de Configurações', 5096],
  ['CONFIG_METAS',               'config-metas.json',                  'Metas comerciais e SLAs de aprovação', 5102],
  ['CONFIG_MOTIVOS_PERDA',       'config-motivos-perda.json',          'Taxonomia de motivos de perda', 5113],
  ['CONFIG_CATEGORIAS_INTERACAO','config-categorias-interacao-cfg.json','Taxonomia de categorias de interação (Config)', 5124],
  ['CONFIG_INTEGRACOES',         'config-integracoes.json',            'Integrações e jobs de sincronização', 5133],
  ['CONFIG_USUARIOS',            'config-usuarios.json',               'Usuários do sistema', 5220],
  ['CONFIG_ROLES',               'config-roles.json',                  'Perfis de acesso (roles)', 5235],
  ['PERF_STATE',                 'estado-performance.json',            'Estado inicial de Performance de CEN', 5923],
  ['FYTD_MESES',                 'fytd-meses.json',                    'Calendário fiscal Tracbel (nov-out)', 5931],
  ['PERF_CENS',                  'performance-cens.json',              'CENs com metas para o painel de performance', 5945],
  ['NOVA_OP_STATE',              'estado-nova-oportunidade.json',      'Estado inicial do formulário Nova Oportunidade', 6583],
  ['CATALOGO_MODELOS',           'catalogo-modelos.json',              'Catálogo de modelos por linha de produto', 6601],
  ['MERCADO_JD_PRACAS',          'mercado-pracas.json',                'Mercado John Deere por praça', 7203],
  ['VENDAS_PERDIDAS_MOTIVOS',    'vendas-perdidas-motivos.json',       'Vendas perdidas por motivo (12m)', 7211],
  ['FATURAMENTO_12M',            'faturamento-12m.json',               'Faturamento realizado x meta, 12 meses', 7222],
  ['MIX_LINHAS',                 'mix-linhas.json',                    'Mix de faturamento por linha de produto', 7233],
  ['TOP_CLIENTES_GLOBAL',        'top-clientes.json',                  'Top clientes por faturamento FYTD', 7242],
  ['ALERTAS_POR_PERFIL',         'alertas.json',                       'Alertas da Visão 360 por perfil', 7254],
  ['PERFIS_360',                 'perfis-360.json',                    'Perfis/personas da Visão 360', 7273],
  ['POSVENDAS_CLIENTE_84391',    'pos-vendas-cliente-84391.json',      'Aba Pós-Vendas da ficha do cliente 84391', 8095],
];

// Escalares e Datas isolados, agrupados num único arquivo de constantes
const ESCALARES = [
  ['NOVAS_STORAGE_KEY',   66],
  ['CONTADOR_STORAGE_KEY', 67],
  ['AGENDA_HOJE',         1123],
  ['HOJE_CARTEIRA',       3373],
  ['FAT_MULT_MT_NORTE',   7229],
  ['FAT_MULT_CEN_JOAO',   7230],
];

// ---------------------------------------------------------------------------
// 3. Execução
// ---------------------------------------------------------------------------

function serializar(valor) {
  // ROUTES contém referências a funções (render/mount). Substituímos pelo nome
  // da função para não perder a informação e manter o JSON válido.
  return JSON.parse(JSON.stringify(valor, (_k, v) => {
    if (typeof v === 'function') return `[function ${v.name || 'anonima'}]`;
    if (typeof v === 'undefined') return null;
    return v;
  }));
}

function contarRegistros(valor) {
  if (Array.isArray(valor)) return valor.length;
  if (valor && typeof valor === 'object') return Object.keys(valor).length;
  return 1;
}

function main() {
  if (!fs.existsSync(APP_JS)) {
    console.error(`[erro] app.js não encontrado em ${APP_JS}`);
    process.exit(1);
  }
  const fonte = fs.readFileSync(APP_JS, 'utf8');
  const ctx = criarContexto();

  try {
    vm.runInContext(fonte, ctx, { filename: 'app.js', timeout: 30000 });
  } catch (e) {
    console.error('[erro] falha ao avaliar app.js no contexto isolado:', e.message);
    process.exit(1);
  }

  const nomes = [...MAPA.map(([n]) => n), ...ESCALARES.map(([n]) => n)];
  const expr = `({ ${nomes.map((n) => `${n}: (typeof ${n} !== 'undefined' ? ${n} : undefined)`).join(', ')} })`;
  const capturado = vm.runInContext(expr, ctx, { filename: 'captura.js' });

  const relatorio = [];
  let escritos = 0;

  for (const [nome, arquivo, descricao, linha] of MAPA) {
    const valor = capturado[nome];
    if (valor === undefined) {
      relatorio.push({ nome, arquivo, linha, registros: 0, status: 'NAO CAPTURADO' });
      console.warn(`[aviso] ${nome} não foi capturado`);
      continue;
    }
    const dados = serializar(valor);
    if (!APENAS_CHECAR) {
      fs.writeFileSync(path.join(SAIDA, arquivo), JSON.stringify(dados, null, 2) + '\n', 'utf8');
    }
    escritos++;
    relatorio.push({ nome, arquivo, linha, registros: contarRegistros(dados), descricao, status: 'ok' });
  }

  // Escalares num único arquivo
  const escalares = {};
  for (const [nome, linha] of ESCALARES) {
    const v = capturado[nome];
    escalares[nome] = { valor: v instanceof Date ? v.toISOString() : v, app_js_linha: linha };
  }
  if (!APENAS_CHECAR) {
    fs.writeFileSync(path.join(SAIDA, 'constantes-escalares.json'),
      JSON.stringify(escalares, null, 2) + '\n', 'utf8');
  }
  escritos++;
  relatorio.push({ nome: '(escalares)', arquivo: 'constantes-escalares.json', linha: '66,67,1123,3373,7229,7230', registros: ESCALARES.length, status: 'ok' });

  // PERF_SERIES é preenchido sob demanda por gerarSerieCEN(). Materializa aqui.
  try {
    const series = vm.runInContext(
      `(function(){ PERF_CENS.forEach((c,i) => { if (!PERF_SERIES[c.id]) PERF_SERIES[c.id] = gerarSerieCEN(c.id, i+1); }); return PERF_SERIES; })()`,
      ctx, { filename: 'perf-series.js' });
    if (!APENAS_CHECAR) {
      fs.writeFileSync(path.join(SAIDA, 'performance-series.json'),
        JSON.stringify(serializar(series), null, 2) + '\n', 'utf8');
    }
    escritos++;
    relatorio.push({ nome: 'PERF_SERIES', arquivo: 'performance-series.json', linha: 6022, registros: contarRegistros(series), descricao: 'Séries mensais FYTD por CEN (geradas por mulberry32)', status: 'ok (materializado)' });
  } catch (e) {
    console.warn('[aviso] PERF_SERIES não materializado:', e.message);
    relatorio.push({ nome: 'PERF_SERIES', arquivo: '-', linha: 6022, registros: 0, status: `FALHOU: ${e.message}` });
  }

  // Índice de manifesto
  if (!APENAS_CHECAR) {
    fs.writeFileSync(path.join(SAIDA, 'manifesto.json'),
      JSON.stringify({
        gerado_por: 'prototipo/dados-seed/extrair.mjs',
        fonte: 'prototipo/referencia/assets/app.js',
        arquivos: relatorio,
      }, null, 2) + '\n', 'utf8');
  }

  console.table(relatorio.map(r => ({ constante: r.nome, arquivo: r.arquivo, linha: r.linha, registros: r.registros, status: r.status })));
  console.log(`\n${escritos} arquivo(s) ${APENAS_CHECAR ? 'validado(s)' : 'gerado(s)'} em ${SAIDA}`);
}

main();
