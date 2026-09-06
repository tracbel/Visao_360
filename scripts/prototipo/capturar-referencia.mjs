// Captura a baseline visual do protótipo de referência.
//
// Serve o protótipo por HTTP (file:// quebra o Leaflet e as fontes), abre cada rota
// no Chromium e salva a página inteira em docs/prototipo/capturas-referencia/.
// Essas imagens são o critério de aceite do port em React.
//
//   node scripts/prototipo/capturar-referencia.mjs
//
// Requer: npm i -D playwright && npx playwright install chromium

import { chromium } from 'playwright';
import { createServer } from 'node:http';
import { readFile, mkdir, access } from 'node:fs/promises';
import { extname, join, normalize, resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const AQUI = dirname(fileURLToPath(import.meta.url));
const RAIZ = resolve(AQUI, '..', '..');

// A cópia congelada dentro do repositório é a fonte preferida. Se ainda não existir,
// cai para a pasta original em Downloads, de onde o protótipo veio.
const ORIGENS = [
  resolve(RAIZ, 'prototipo', 'referencia'),
  'C:\\Users\\ricardo.moretti\\Downloads\\crm-tracbel-agro-prototipo',
];
const SAIDA = resolve(RAIZ, 'docs', 'prototipo', 'capturas-referencia');
const PORTA = 4599;

const TIPOS = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.png': 'image/png',
  '.svg': 'image/svg+xml',
};

const ROTAS = [
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
];

async function existe(caminho) {
  try {
    await access(caminho);
    return true;
  } catch {
    return false;
  }
}

async function escolherOrigem() {
  for (const origem of ORIGENS) {
    if (await existe(join(origem, 'index.html'))) return origem;
  }
  throw new Error(`Protótipo não encontrado. Procurei em:\n  ${ORIGENS.join('\n  ')}`);
}

const origem = await escolherOrigem();
await mkdir(SAIDA, { recursive: true });
console.log('origem:', origem);
console.log('saída :', SAIDA);

const servidor = createServer(async (req, res) => {
  try {
    const url = decodeURIComponent(req.url.split('?')[0]);
    const arquivo = join(origem, normalize(url === '/' ? '/index.html' : url));
    if (!arquivo.startsWith(origem)) throw new Error('fora da raiz');
    const conteudo = await readFile(arquivo);
    res.writeHead(200, { 'Content-Type': TIPOS[extname(arquivo)] ?? 'application/octet-stream' });
    res.end(conteudo);
  } catch {
    res.writeHead(404);
    res.end('nao encontrado');
  }
});
await new Promise((pronto) => servidor.listen(PORTA, pronto));

const navegador = await chromium.launch();
const pagina = await navegador.newPage({
  viewport: { width: 1280, height: 900 },
  deviceScaleFactor: 2,
});

const erros = [];
pagina.on('pageerror', (e) => erros.push(e.message));

for (const [rota, nome] of ROTAS) {
  await pagina.goto(`http://localhost:${PORTA}/index.html#${rota}`, { waitUntil: 'networkidle' });
  await pagina.waitForTimeout(1400); // gráficos e tiles do mapa terminam de desenhar
  await pagina.screenshot({ path: join(SAIDA, `${nome}.png`), fullPage: true });
  console.log('ok', rota, '->', `${nome}.png`);
}

await navegador.close();
servidor.close();

if (erros.length > 0) {
  console.log('ERROS DE PÁGINA:', [...new Set(erros)].slice(0, 10));
  process.exitCode = 1;
}
