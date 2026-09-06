// Compara as telas do CRM em React com as capturas do protótipo de referência.
//
// Para cada rota: sobe o Vite (ou usa um servidor já em pé), tira a captura da
// tela nova, compara pixel a pixel com a imagem correspondente em
// docs/prototipo/capturas-referencia/ e grava um diff em docs/prototipo/comparacao/.
//
//   node scripts/prototipo/comparar-telas.mjs              # todas as rotas
//   node scripts/prototipo/comparar-telas.mjs /cobertura   # só uma
//
// Saída: tabela com % de pixels diferentes por rota. É o critério de "igualzinho".

import { chromium } from 'playwright';
import { readFile, mkdir, writeFile, access } from 'node:fs/promises';
import { resolve, dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { PNG } from 'pngjs';
import pixelmatch from 'pixelmatch';

const AQUI = dirname(fileURLToPath(import.meta.url));
const RAIZ = resolve(AQUI, '..', '..');
const REFERENCIA = resolve(RAIZ, 'docs', 'prototipo', 'capturas-referencia');
const SAIDA = resolve(RAIZ, 'docs', 'prototipo', 'comparacao');
const BASE = process.env.CRM_URL ?? 'http://localhost:5199';

// rota -> nome do arquivo de referência
const ROTAS = new Map([
  ['/', 'visao-360'],
  ['/agenda', 'agenda'],
  ['/cobertura', 'cobertura'],
  ['/pipeline', 'pipeline'],
  ['/clientes', 'clientes'],
  ['/clientes/84391', 'clientes_84391'],
  ['/equipamentos/1RW7250PVMR123456', 'equipamentos_1RW7250PVMR123456'],
  ['/oportunidades/1517613', 'oportunidades_1517613'],
  ['/oportunidades/nova', 'oportunidades_nova'],
  ['/equipamentos', 'equipamentos'],
  ['/relatorios/funil', 'relatorios_funil'],
  ['/relatorios/performance', 'relatorios_performance'],
  ['/relatorios/cobertura', 'relatorios_cobertura'],
  ['/config', 'config'],
  ['/inicio-antigo', 'inicio-antigo'],
]);

const pedidas = process.argv.slice(2);
const alvo = pedidas.length > 0 ? pedidas : [...ROTAS.keys()];

await mkdir(SAIDA, { recursive: true });

const navegador = await chromium.launch();
const pagina = await navegador.newPage({
  viewport: { width: 1280, height: 900 },
  deviceScaleFactor: 2,
});

const erros = [];
pagina.on('pageerror', (e) => erros.push(e.message));

const linhas = [];

for (const rota of alvo) {
  const nome = ROTAS.get(rota);
  if (!nome) {
    console.log(`rota desconhecida: ${rota}`);
    continue;
  }

  const arquivoRef = join(REFERENCIA, `${nome}.png`);
  try {
    await access(arquivoRef);
  } catch {
    linhas.push({ rota, situacao: 'sem referência' });
    continue;
  }

  await pagina.goto(`${BASE}/#${rota}`, { waitUntil: 'networkidle' });
  await pagina.waitForTimeout(1400);
  const bufferNovo = await pagina.screenshot({ fullPage: true });
  await writeFile(join(SAIDA, `${nome}-novo.png`), bufferNovo);

  const referencia = PNG.sync.read(await readFile(arquivoRef));
  const novo = PNG.sync.read(bufferNovo);

  if (referencia.width !== novo.width || referencia.height !== novo.height) {
    linhas.push({
      rota,
      situacao: `tamanho diferente — referência ${referencia.width}x${referencia.height}, novo ${novo.width}x${novo.height}`,
    });
    continue;
  }

  const diff = new PNG({ width: referencia.width, height: referencia.height });
  const diferentes = pixelmatch(
    referencia.data,
    novo.data,
    diff.data,
    referencia.width,
    referencia.height,
    { threshold: 0.1 },
  );
  await writeFile(join(SAIDA, `${nome}-diff.png`), PNG.sync.write(diff));

  const total = referencia.width * referencia.height;
  linhas.push({
    rota,
    situacao: `${((diferentes / total) * 100).toFixed(2)}% diferente (${diferentes} px)`,
  });
}

await navegador.close();

console.log('\nrota'.padEnd(40) + 'resultado');
console.log('-'.repeat(80));
for (const { rota, situacao } of linhas) console.log(rota.padEnd(40) + situacao);

if (erros.length > 0) console.log('\nERROS DE PÁGINA:', [...new Set(erros)].slice(0, 10));
