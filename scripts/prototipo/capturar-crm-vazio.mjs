// Confere as telas do CRM com o banco SANITIZADO (documento 38): cada tela abre, nao quebra e mostra
// estado vazio em vez de erro. So leitura: nao grava nada.
// Uso: node capturar-crm-vazio.mjs [front] [pasta de saida]
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5199';
const saida = process.argv[3] ?? fileURLToPath(new URL('../../dados-locais/capturas', import.meta.url));

const TELAS = [
  { nome: 'visao360-diretoria', caminho: '/' },
  { nome: 'clientes', caminho: '/clientes' },
  { nome: 'equipamentos', caminho: '/equipamentos' },
  { nome: 'agenda', caminho: '/agenda' },
  { nome: 'pipeline', caminho: '/pipeline' },
  { nome: 'cobertura', caminho: '/cobertura' },
  { nome: 'funil', caminho: '/relatorios/funil' },
  { nome: 'performance', caminho: '/relatorios/performance' },
  { nome: 'cobertura-filial', caminho: '/relatorios/cobertura' },
  { nome: 'territorio', caminho: '/relatorios/territorio' },
  { nome: 'configuracoes', caminho: '/config' },
];

const navegador = await chromium.launch();
const pagina = await navegador.newPage({ viewport: { width: 1440, height: 900 } });
const resultado = [];
let erros = [];
let falhasDaApi = [];
pagina.on('console', (m) => { if (m.type() === 'error') erros.push(m.text()); });
pagina.on('pageerror', (e) => erros.push(`pageerror: ${e.message}`));
pagina.on('response', (r) => {
  if (r.url().includes('/api/') && r.status() >= 400) falhasDaApi.push(`${r.status()} ${new URL(r.url()).pathname}`);
});

async function conferir(nome) {
  await pagina.waitForTimeout(2500);
  const texto = await pagina.locator('main').innerText().catch(() => '');
  const quebrou = /Cannot read|undefined is not|TypeError|is not a function/i.test(texto + erros.join(' '));
  await pagina.screenshot({ path: `${saida}/vazio-${nome}.png` });
  resultado.push({
    tela: nome,
    quebrou,
    errosDeConsole: erros.length,
    respostasDeErroDaApi: [...new Set(falhasDaApi)],
    trecho: texto.replace(/\s+/g, ' ').slice(0, 160),
  });
  erros = [];
  falhasDaApi = [];
}

for (const tela of TELAS) {
  await pagina.goto(`${base}/#${tela.caminho}`, { waitUntil: 'networkidle', timeout: 120000 });
  await conferir(tela.nome);
}

// Visao 360 no perfil do CEN: o "meu dia" e a busca de cliente, com o banco vazio.
await pagina.goto(`${base}/#/`, { waitUntil: 'networkidle', timeout: 120000 });
await pagina.locator('.v360-perfil-btn', { hasText: 'CEN' }).first().click();
await pagina.waitForLoadState('networkidle');
await conferir('visao360-cen');

await navegador.close();
console.log(JSON.stringify(resultado, null, 2));
