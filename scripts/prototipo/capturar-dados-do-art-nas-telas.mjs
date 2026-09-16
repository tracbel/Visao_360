// Captura os dados do ART nas telas que já existem (documento 35, seção 11): a ficha da máquina com
// "Divergências abertas", a ficha do cliente com "Máquinas compradas" e Configurações › TI e Integrações ›
// Integrações com "Sincronizações do servidor".
//
// Uso: node capturar-dados-do-art-nas-telas.mjs [front] [api] [pasta de saída]
// As capturas têm nome real de cliente e vão para dados-locais/, fora do Git. O resumo impresso só tem
// contagem e chassi.
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5199';
const api = process.argv[3] ?? 'http://localhost:5145';
const saida = process.argv[4] ?? fileURLToPath(new URL('../../dados-locais/capturas', import.meta.url));
const cabecalhos = { 'X-Tracbel-Usuario': 'cen.ribeiraopreto@tracbel.com.br', 'X-Tracbel-Empresa': '010101' };

async function dados(caminho) {
  const r = await fetch(`${api}/api/v1${caminho}`, { headers: cabecalhos });
  if (!r.ok) throw new Error(`${caminho} respondeu ${r.status}`);
  return (await r.json()).dados;
}

// OS EXEMPLOS VÊM DA API, e não de arquivo: a máquina com divergência aberta e o comprador com mais compras.
const lista = await dados('/equipamentos?somenteComVenda=true&tamanho=200');
let maquina = null;
for (const item of lista.itens) {
  const ficha = await dados(`/equipamentos/${item.chave}`);
  if (ficha.divergenciasAbertas.length > 0) { maquina = ficha; break; }
}
let comprador = null;
let compras = [];
for (const chave of [...new Set(lista.itens.map((i) => i.compradorNaUltimaVendaChave).filter(Boolean))].slice(0, 40)) {
  const c = await dados(`/clientes/${chave}/maquinas-compradas`);
  if (c.length > compras.length) { comprador = chave; compras = c; }
}

const navegador = await chromium.launch();
const pagina = await navegador.newPage({ viewport: { width: 1600, height: 1000 } });
const erros = [];
const falhasDaApi = [];
pagina.on('console', (m) => { if (m.type() === 'error') erros.push(m.text()); });
pagina.on('pageerror', (e) => erros.push(e.message));
pagina.on('response', (r) => { if (r.url().includes('/api/') && r.status() >= 400) falhasDaApi.push(`${r.status()} ${new URL(r.url()).pathname}`); });

const cartao = (titulo) =>
  pagina.locator('.card').filter({ has: pagina.locator('.card-title', { hasText: new RegExp(`^${titulo}$`) }) });

const resumo = {};

if (maquina) {
  await pagina.goto(`${base}/#/equipamentos/${maquina.chave}`, { waitUntil: 'networkidle', timeout: 120000 });
  const c = cartao('Divergências abertas');
  await c.waitFor({ timeout: 60000 });
  await c.scrollIntoViewIfNeeded();
  await c.screenshot({ path: `${saida}/art-telas-1-ficha-da-maquina-divergencias.png` });
  resumo.fichaDaMaquina = {
    chassi: maquina.chassi,
    linhasNoCartao: await c.locator('tbody tr').count(),
    divergenciasNaApi: maquina.divergenciasAbertas.length,
    historicoComercial: (await cartao('Histórico comercial').count()) > 0,
  };
}

if (comprador) {
  await pagina.goto(`${base}/#/clientes/${comprador}`, { waitUntil: 'networkidle', timeout: 120000 });
  const c = cartao('Máquinas compradas');
  await c.locator('tbody tr, .cad-nota').first().waitFor({ timeout: 60000 });
  await c.scrollIntoViewIfNeeded();
  await c.screenshot({ path: `${saida}/art-telas-2-ficha-do-cliente-maquinas-compradas.png` });
  resumo.fichaDoCliente = {
    linhasNoCartao: await c.locator('tbody tr').count(),
    maquinasNaApi: compras.length,
    chassis: compras.map((m) => m.chassi),
    comoDonoAtual: compras.filter((m) => m.ehDonoAtual).length,
  };
}

await pagina.goto(`${base}/#/config`, { waitUntil: 'networkidle', timeout: 120000 });
await pagina.locator('.config-aba', { hasText: 'TI e Integrações' }).click();
await pagina.getByText('Integrações', { exact: true }).first().click();
const sinc = pagina.locator('.config-card').filter({ has: pagina.locator('h3', { hasText: 'Sincronizações do servidor' }) });
await sinc.locator('tbody tr, .config-hint').first().waitFor({ timeout: 60000 });
await sinc.screenshot({ path: `${saida}/art-telas-3-configuracoes-sincronizacoes.png` });
resumo.configuracoes = {
  execucoesNaTabela: await sinc.locator('tbody tr').count(),
  resultados: await sinc.locator('tbody tr td:nth-child(2)').allInnerTexts(),
};

resumo.menuTemIntegracaoArt = (await pagina.locator('nav, aside').getByText('Integração ART').count()) > 0;
resumo.errosDeConsole = erros;
resumo.respostasDeErroDaApi = falhasDaApi;

await navegador.close();
console.log(JSON.stringify(resumo, null, 2));
