import { chromium } from 'playwright';
const nav = await chromium.launch();
const pg = await nav.newPage({ viewport: { width: 1280, height: 900 }, deviceScaleFactor: 2 });
await pg.goto('http://localhost:5173/#/', { waitUntil: 'networkidle' });
await pg.waitForTimeout(1500);
// troca para o perfil de gerente
const alvo = pg.getByText(/Roberto Silveira|GERENTE/i).first();
if (await alvo.isVisible().catch(()=>false)) { await alvo.click(); await pg.waitForTimeout(2000); }
const alt = await pg.evaluate(() => document.body.scrollHeight);
const graficos = await pg.evaluate(() => document.querySelectorAll('canvas, svg[class*=grafico], .v360-card canvas').length);
console.log('altura no perfil gerente:', alt, '| referencia: 1981 (css px)');
console.log('canvas/graficos na tela:', graficos);
await pg.screenshot({ path: 'c:/tmp/h-gerente.png', fullPage: true });
await nav.close();
