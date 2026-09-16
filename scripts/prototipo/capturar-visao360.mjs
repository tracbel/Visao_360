// Captura a Visão 360 (os cinco cartões e o painel) para a validação visual (documento 36).
// Uso: node capturar-visao360.mjs [base] [pasta de saída] [prefixo dos arquivos]
// A saída tem número real da empresa e vai para dados-locais/, fora do Git.
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5199';
const saida = process.argv[3] ?? fileURLToPath(new URL('../../dados-locais/capturas', import.meta.url));
const prefixo = process.argv[4] ?? 'visao360';

const navegador = await chromium.launch();
const pagina = await navegador.newPage({ viewport: { width: 1600, height: 1000 } });
const erros = [];
const falhas = [];
pagina.on('console', (m) => { if (m.type() === 'error') erros.push(m.text()); });
pagina.on('pageerror', (e) => erros.push(e.message));
pagina.on('response', (r) => { if (r.url().includes('/api/') && r.status() >= 400) falhas.push(`${r.status()} ${new URL(r.url()).pathname}`); });

await pagina.goto(`${base}/#/`, { waitUntil: 'networkidle', timeout: 180000 });
await pagina.waitForSelector('.v360-kpi-row, .cad-estado-erro', { timeout: 180000 });
await pagina.waitForTimeout(2000);
await pagina.screenshot({ path: `${saida}/${prefixo}-1-geral.png`, fullPage: true });
if ((await pagina.locator('.v360-kpi-row').count()) > 0) {
  await pagina.locator('.v360-kpi-row').screenshot({ path: `${saida}/${prefixo}-2-cartoes.png` });
}

const cartoes = await pagina.evaluate(() =>
  [...document.querySelectorAll('.v360-kpi-row .v360-kpi-card')].map((c) => ({
    titulo: c.querySelector('.v360-kpi-titulo')?.textContent?.trim(),
    valor: c.querySelector('.v360-kpi-valor')?.textContent?.trim(),
    rodape: c.querySelector('.v360-kpi-rodape')?.textContent?.replace(/\s+/g, ' ').trim(),
  })),
);

console.log(JSON.stringify({ erros, falhas: [...new Set(falhas)], cartoes }, null, 2));
await navegador.close();
