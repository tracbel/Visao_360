// Prova os ESTADOS da Visão 360 e dos mapas que o banco real não produz sob demanda (documento 36):
// carregando, sem permissão, falha parcial de filiais e sem dado. A API é interceptada NO NAVEGADOR
// (page.route) — nada é alterado no banco nem no servidor, e as respostas simuladas não têm dado real.
// Uso: node capturar-estados-visao360.mjs [base] [pasta de saída]
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5199';
const saida = process.argv[3] ?? fileURLToPath(new URL('../../dados-locais/capturas', import.meta.url));

const ROTA_DOS_CARTOES = '**/api/v1/relatorios/indicadores-executivos**';
const ROTA_DOS_MAPAS = '**/api/v1/territorio/indicadores**';

const semAcesso = {
  status: 403,
  contentType: 'application/problem+json',
  body: JSON.stringify({
    type: 'https://crm.tracbel.com.br/erros/sem-acesso',
    title: 'Sem acesso',
    status: 403,
    detail: 'Simulação da validação: o perfil não alcança esta filial.',
  }),
};

const falhaDoServidor = {
  status: 500,
  contentType: 'application/problem+json',
  body: JSON.stringify({ type: 'about:blank', title: 'Falha', status: 500, detail: 'Simulação da validação: falha na leitura.' }),
};

/** Um painel de filial SEM DADO NENHUM — o que uma filial recém-aberta devolveria. */
const painelVazio = {
  dados: {
    indicadores: {
      referenciaUtc: new Date().toISOString(),
      faturamentoDoMes: null,
      ano: {
        ano: new Date().getFullYear(), primeiraCompetencia: null, ultimaCompetencia: null, mesesComFaturamento: 0,
        comCliente: 0, semCliente: 0, metasDaFilial: 0, alvoDaFilial: null, metasDetalhadas: 0, metasQueCruzamOAno: 0, total: 0,
      },
      carteira: {
        clientesCadastradosComVinculo: 0, clientes: 0, prospects: 0, suspects: 0, outrasSituacoes: 0, semDocumento: 0,
        clientesNasCarteirasDaFilial: 0, vinculos: 0, vinculosComerciais: 0, carteiras: 0, carteirasComerciais: 0,
      },
      cobertura: {
        vinculosComerciais: 0, elegiveis: 0, cobertos: 0, foraDaCadencia: 0, nuncaContatados: 0, semCadencia: 0,
        contatoMaisRecente: null, tiposDeAtividade: 0, tiposMarcadosComoVisita: 0, pendentes: 0,
      },
      mercado: {
        vendasPerdidasRegistradas: 0, comConcorrente: 0, comModeloDoConcorrente: 0, comOsDoisPrecos: 0, unidades: 0,
        primeiraEm: null, ultimaEm: null,
      },
    },
    metricasSemDado: [{ metrica: 'faturamentoDoMes', motivo: 'Simulação: nenhuma nota carregada.' }],
  },
  procedencia: null,
};

const navegador = await chromium.launch();
const relatorio = {};

async function cenario(nome, preparar, { url = `${base}/#/`, esperar = '.v360-kpi-row, .cad-estado-erro, .cad-estado', ms = 2500 } = {}) {
  const pagina = await navegador.newPage({ viewport: { width: 1600, height: 1000 } });
  const erros = [];
  pagina.on('pageerror', (e) => erros.push(e.message));
  await preparar(pagina);
  await pagina.goto(url, { waitUntil: 'domcontentloaded', timeout: 180000 });
  await pagina.waitForSelector(esperar, { timeout: 240000 });
  await pagina.waitForTimeout(ms);
  await pagina.screenshot({ path: `${saida}/estado-${nome}.png`, fullPage: false });
  relatorio[nome] = {
    erros,
    periodo: await pagina.locator('.v360-periodo').textContent().catch(() => null),
    estados: await pagina.locator('.cad-estado, .cad-estado-erro, .cad-estado-carregando').allTextContents().catch(() => []),
    cartoes: await pagina.locator('.v360-kpi-card').evaluateAll((cs) =>
      cs.map((c) => `${c.querySelector('.v360-kpi-titulo')?.textContent} = ${c.querySelector('.v360-kpi-valor')?.textContent} | ${c.querySelector('.v360-kpi-rodape')?.textContent}`),
    ).catch(() => []),
  };
  await pagina.close();
}

// 1. CARREGANDO: os cartões demoram; a tela mostra o bloco de carregamento, e não zeros.
await cenario('1-carregando', async (p) => {
  await p.route(ROTA_DOS_CARTOES, async (r) => { await new Promise((ok) => setTimeout(ok, 60000)); await r.continue(); });
}, { esperar: '.v360-container', ms: 6000 });

// 2. SEM PERMISSÃO: todas as filiais recusam com 403.
await cenario('2-sem-permissao', async (p) => { await p.route(ROTA_DOS_CARTOES, (r) => r.fulfill(semAcesso)); });

// 3. FALHA PARCIAL: três filiais falham; o total diz quantas responderam e a composição nomeia as que faltaram.
await cenario('3-falha-parcial', async (p) => {
  let contagem = 0;
  await p.route(ROTA_DOS_CARTOES, (r) => (++contagem <= 3 ? r.fulfill(falhaDoServidor) : r.continue()));
});

// 4. SEM DADO: todas as filiais respondem vazio.
await cenario('4-sem-dado', async (p) => {
  await p.route(ROTA_DOS_CARTOES, (r) => r.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(painelVazio) }));
});

// 5. MAPAS SEM PERMISSÃO: a rota dos mapas recusa com 403.
await cenario('5-mapas-sem-permissao', async (p) => { await p.route(ROTA_DOS_MAPAS, (r) => r.fulfill(semAcesso)); }, {
  url: `${base}/#/relatorios/territorio`,
  esperar: '.cad-estado-erro, .cad-estado',
});

console.log(JSON.stringify(relatorio, null, 2));
await navegador.close();
