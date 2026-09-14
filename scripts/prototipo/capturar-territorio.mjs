// Captura a tela de Indicadores Geográficos da ADR para a validação visual (documento 32).
// Uso: node capturar-territorio.mjs [base] [pasta de saída]
// A saída tem número real da empresa e vai para dados-locais/, fora do Git.
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5199';
// Relativa ao script, e não a de onde se roda o comando (ver validar-territorio-perfis.mjs).
const saida = process.argv[3] ?? fileURLToPath(new URL('../../dados-locais/capturas', import.meta.url));

const navegador = await chromium.launch();
const pagina = await navegador.newPage({ viewport: { width: 1600, height: 1000 } });
const erros = [];
pagina.on('console', (m) => { if (m.type() === 'error') erros.push(m.text()); });
pagina.on('pageerror', (e) => erros.push(e.message));

await pagina.goto(`${base}/#/relatorios/territorio`, { waitUntil: 'networkidle' });
await pagina.waitForSelector('.terr-svg', { timeout: 90000 });
await pagina.screenshot({ path: `${saida}/territorio-1-geral.png`, fullPage: true });
await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/territorio-2-mapas.png` });

await pagina.getByRole('button', { name: 'Ribeirão Preto', exact: true }).first().click();
await pagina.waitForSelector('.terr-detalhe');
await pagina.locator('.terr-detalhe').screenshot({ path: `${saida}/territorio-3-detalhe-ribeirao-preto.png` });

await pagina.getByRole('button', { name: 'quantidade', exact: true }).click();
await pagina.getByRole('button', { name: 'Pós-venda', exact: true }).click();
await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/territorio-4-mapas-quantidade-posvenda.png` });

// O fim da tabela: os grupos fora do mapa e a linha de total, que é onde a conferência fecha.
const tabela = pagina.locator('.terr-tabela-municipios');
await tabela.evaluate((e) => { e.scrollTop = e.scrollHeight; });
await tabela.screenshot({ path: `${saida}/territorio-5-tabela-fora-do-mapa-e-total.png` });

const resumo = await pagina.evaluate(() => ({
  poligonosPorMapa: [...document.querySelectorAll('.terr-svg')].map((s) => s.querySelectorAll('path.terr-poligono').length),
  hachurados: [...document.querySelectorAll('.terr-svg')].map((s) => [...s.querySelectorAll('path.terr-poligono')].filter((p) => (p.getAttribute('fill') ?? '').startsWith('url(')).length),
  indicadores: [...document.querySelectorAll('.cad-indicador, [class*="indicador"]')].slice(0, 5).map((e) => e.textContent?.replace(/\s+/g, ' ').trim()),
  filtrosDesligados: [...document.querySelectorAll('.terr-filtro select:disabled')].length,
  titulo: document.querySelector('.page-title')?.textContent,
  gruposForaDoMapa: [...document.querySelectorAll('.terr-tabela-municipios tbody tr')]
    .map((tr) => tr.querySelector('td')?.childNodes[0]?.textContent?.trim())
    .filter((nome) => nome && /^[A-Z][a-z]+(?:[A-Z][a-z]+)+$/.test(nome)),
  linhaDeTotal: document.querySelector('.terr-linha-total')?.textContent?.replace(/\s+/g, ' ').trim(),
}));

console.log(JSON.stringify({ erros, resumo }, null, 2));
await navegador.close();
