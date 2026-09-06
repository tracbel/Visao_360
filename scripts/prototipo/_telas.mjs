import { chromium } from 'playwright';
const ROTAS = [
  ['/cobertura','cobertura'], ['/pipeline','pipeline'], ['/agenda','agenda'],
  ['/relatorios/funil','funil'], ['/relatorios/performance','performance'],
  ['/relatorios/cobertura','cobertura-filial'],
];
const nav = await chromium.launch();
const pg = await nav.newPage({ viewport: { width: 1280, height: 900 }, deviceScaleFactor: 2 });
const erros = [];
pg.on('pageerror', e => erros.push(e.message));
for (const [rota, nome] of ROTAS) {
  erros.length = 0;
  await pg.goto('http://localhost:5200/#' + rota, { waitUntil: 'networkidle' });
  await pg.waitForTimeout(2000);
  const alt = await pg.evaluate(() => document.body.scrollHeight);
  await pg.screenshot({ path: `c:/tmp/tela-${nome}.png`, fullPage: true });
  console.log(nome.padEnd(18), 'altura:', alt, '| erros:', erros.length ? erros.slice(0,2) : 'nenhum');
}
await nav.close();
