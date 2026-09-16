// Captura a tela de Indicadores Geográficos da ADR para a validação visual (documentos 32 e 36).
// Uso: node capturar-territorio.mjs [base] [pasta de saída] [prefixo dos arquivos]
// A saída tem número real da empresa e vai para dados-locais/, fora do Git.
//
// Funciona também com o território VAZIO — banco sem a carga do território —, que é justamente o estado
// que precisa de prova: nesse caso ela fotografa a tela como está e diz no resumo o que faltou.
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5199';
// Relativa ao script, e não a de onde se roda o comando (ver validar-territorio-perfis.mjs).
const saida = process.argv[3] ?? fileURLToPath(new URL('../../dados-locais/capturas', import.meta.url));
const prefixo = process.argv[4] ?? 'territorio';

const navegador = await chromium.launch();
const pagina = await navegador.newPage({ viewport: { width: 1600, height: 1000 } });
const erros = [];
const respostas = [];
pagina.on('console', (m) => { if (m.type() === 'error') erros.push(m.text()); });
pagina.on('pageerror', (e) => erros.push(e.message));
pagina.on('response', async (r) => {
  if (!r.url().includes('/api/v1/territorio/indicadores')) return;
  const item = { status: r.status(), consulta: new URL(r.url()).search };
  try {
    const corpo = await r.json();
    const ind = corpo?.dados?.indicadores;
    const adr = ind?.municipios?.filter((m) => m.pertenceAAdr) ?? [];
    item.municipios = ind?.municipios?.length ?? null;
    item.daAdr = adr.length;
    item.periodo = ind ? `${ind.competenciaInicial} a ${ind.competenciaFinal}` : null;
    item.adrElegiveis = adr.reduce((s, m) => s + m.cobertura.vinculosComCadencia, 0);
    item.adrCobertos = adr.reduce((s, m) => s + m.cobertura.cobertos, 0);
    item.adrVendas = Math.round(adr.reduce((s, m) => s + m.vendas.valorLiquido, 0));
    item.gruposForaDoMapa = ind?.foraDoMapa?.map((g) => g.grupo) ?? null;
  } catch {
    item.corpo = 'não é JSON';
  }
  respostas.push(item);
});

/** Espera a próxima resposta dos indicadores e o fim do carregamento. */
async function aposMudar(acao) {
  const resposta = pagina.waitForResponse((r) => r.url().includes('/api/v1/territorio/indicadores'), { timeout: 180000 });
  await acao();
  await resposta;
  await pagina.waitForTimeout(1200);
}

await pagina.goto(`${base}/#/relatorios/territorio`, { waitUntil: 'networkidle', timeout: 180000 });
await pagina.waitForSelector('.terr-svg, .cad-estado', { timeout: 180000 });
await pagina.waitForTimeout(1500);
await pagina.screenshot({ path: `${saida}/${prefixo}-1-geral.png`, fullPage: true });

const temMapas = (await pagina.locator('.terr-grade-mapas').count()) > 0;
let temRibeirao = false;

if (temMapas) {
  await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/${prefixo}-2-mapas.png` });

  // O FOCO COMPARTILHADO: o cursor sobre Ribeirão Preto no mapa de cobertura mostra o número dela nos três.
  const poligono = pagina.locator('svg.terr-svg').first().locator('path.terr-poligono', {
    has: pagina.locator('title', { hasText: /^Ribeirão Preto —/ }),
  });
  if ((await poligono.count()) > 0) {
    await poligono.first().hover({ force: true });
    await pagina.waitForTimeout(400);
    await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/${prefixo}-3-foco-ribeirao-preto.png` });
  }

  const ribeirao = pagina.getByRole('button', { name: 'Ribeirão Preto', exact: true }).first();
  temRibeirao = (await ribeirao.count()) > 0;
  if (temRibeirao) {
    await ribeirao.click();
    await pagina.waitForSelector('.terr-detalhe');
    await pagina.locator('.terr-detalhe').screenshot({ path: `${saida}/${prefixo}-4-detalhe-ribeirao-preto.png` });
  }

  await pagina.getByRole('button', { name: 'pendentes (qtd.)', exact: true }).click();
  await pagina.getByRole('button', { name: 'Pós-venda', exact: true }).click();
  await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/${prefixo}-5-mapas-quantidade-posvenda.png` });
  await pagina.getByRole('button', { name: '% no prazo', exact: true }).click();
  await pagina.getByRole('button', { name: 'Total líquido', exact: true }).click();

  // AS DUAS REGIÕES DA ADR, uma de cada vez: o resumo registra quantos municípios cada uma trouxe.
  for (const regiao of ['Norte', 'Noroeste']) {
    await aposMudar(() => pagina.getByLabel('Região da ADR').selectOption(regiao));
    await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/${prefixo}-6-regiao-${regiao.toLowerCase()}.png` });
  }
  await aposMudar(() => pagina.getByLabel('Região da ADR').selectOption(''));

  await aposMudar(() => pagina.getByRole('button', { name: 'Ano civil até o último mês fechado', exact: true }).click());
  await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/${prefixo}-7-ano-civil.png` });
  await aposMudar(() => pagina.getByRole('button', { name: '12 meses fechados', exact: true }).click());
}

// O fim da tabela: os grupos fora do mapa e a linha de total, que é onde a conferência fecha.
const tabela = pagina.locator('.terr-tabela-municipios');
if ((await tabela.count()) > 0) {
  await tabela.evaluate((e) => { e.scrollTop = e.scrollHeight; });
  await tabela.screenshot({ path: `${saida}/${prefixo}-8-tabela-fora-do-mapa-e-total.png` });
}

const resumo = await pagina.evaluate(() => {
  const svgs = [...document.querySelectorAll('.terr-svg')];
  return {
    poligonosPorMapa: svgs.map((s) => s.querySelectorAll('path.terr-poligono').length),
    // A ADR é desenhada com traço mais grosso (MapaDeMunicipios); fora dela o traço é o fino.
    naAdrPorMapa: svgs.map((s) => [...s.querySelectorAll('path.terr-poligono')].filter((p) => p.getAttribute('stroke-width') === '0.7').length),
    hachurados: svgs.map((s) => [...s.querySelectorAll('path.terr-poligono')].filter((p) => (p.getAttribute('fill') ?? '').startsWith('url(')).length),
    quadro: svgs.map((s) => s.getAttribute('viewBox')),
    resumosDosMapas: [...document.querySelectorAll('.terr-mapa-resumo')].map((e) => e.textContent?.replace(/\s+/g, ' ').trim()),
    indicadores: [...document.querySelectorAll('.cad-indicador, [class*="indicador"]')].slice(0, 5).map((e) => e.textContent?.replace(/\s+/g, ' ').trim()),
    avisos: [...document.querySelectorAll('.cad-estado, .terr-territorio-sem-carga')].map((e) => e.textContent?.replace(/\s+/g, ' ').trim()),
    titulo: document.querySelector('.page-title')?.textContent,
    linhaDeTotal: document.querySelector('.terr-linha-total')?.textContent?.replace(/\s+/g, ' ').trim(),
  };
});

console.log(JSON.stringify({ erros, respostas, temMapas, temRibeirao, resumo }, null, 2));
await navegador.close();
