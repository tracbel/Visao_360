// Validação funcional e visual da visão da empresa com PERFIS DE TESTE num banco isolado (documento 32).
//
// Uso: node validar-territorio-perfis.mjs [base] [pasta de saída]
//
// Pré-requisitos, e nenhum deles toca usuário real:
//   - banco de validação (cópia do de desenvolvimento) com os perfis de teste semeados por
//     scripts/banco/validacao/perfis-de-teste-da-visao-da-empresa.sql;
//   - API em Desenvolvimento apontando para esse banco, na porta 5145, e o Vite na 5199.
// A saída tem número real da empresa e vai para dados-locais/, fora do Git.
import { writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const base = process.argv[2] ?? 'http://localhost:5199';
// A PASTA DE SAÍDA É RELATIVA AO SCRIPT, e não a de onde se roda o comando: rodado da raiz, um caminho
// relativo gravaria capturas com número real fora de dados-locais/.
const saida = process.argv[3] ?? fileURLToPath(new URL('../../dados-locais/capturas', import.meta.url));
const ROTA = '/api/v1/territorio/indicadores';
const PERFIS = [
  { nome: 'autorizado', usuario: 'perfil.autorizado@exemplo.com' },
  { nome: 'sem-acesso', usuario: 'perfil.sem.acesso@exemplo.com' },
];

const navegador = await chromium.launch();
const resultado = {};

const esperarPainel = async (pagina) => {
  await pagina.waitForSelector('.terr-svg', { timeout: 120000 });
  await pagina.waitForLoadState('networkidle');
};

// O contorno da ADR é o traço de 0,7; o resto de São Paulo, 0,35 (MapaDeMunicipios.tsx).
const contarPoligonos = (pagina) =>
  pagina.evaluate(() =>
    [...document.querySelectorAll('.terr-svg')].map((svg) => {
      const todos = [...svg.querySelectorAll('path.terr-poligono')];
      return { total: todos.length, adr: todos.filter((p) => p.getAttribute('stroke-width') === '0.7').length };
    }),
  );

const gruposForaDoMapa = (pagina) =>
  pagina.evaluate(() =>
    [...document.querySelectorAll('.terr-tabela-municipios tbody tr td:first-child')]
      .map((td) => td.childNodes[0]?.textContent?.trim())
      .filter((nome) => nome && /^[A-Z][a-z]+(?:[A-Z][a-z]+)+$/.test(nome)),
  );

const pedirPelaApi = (pagina, usuario, consulta) =>
  pagina.evaluate(
    async ([rota, u, q]) => {
      const resposta = await fetch(`${rota}${q}`, { headers: { 'X-Tracbel-Usuario': u, 'X-Tracbel-Empresa': '010101' } });
      return resposta.status;
    },
    [ROTA, usuario, consulta],
  );

const filtro = (pagina, rotulo) => pagina.locator('label.terr-filtro', { hasText: rotulo }).locator('select');

const trocarFiltro = async (pagina, rotulo, valor, trechoDaUrl) => {
  const resposta = pagina.waitForResponse((r) => r.url().includes(ROTA) && r.url().includes(trechoDaUrl), { timeout: 120000 });
  await filtro(pagina, rotulo).selectOption(valor);
  await resposta;
  await esperarPainel(pagina);
};

for (const perfil of PERFIS) {
  const contexto = await navegador.newContext({ viewport: { width: 1600, height: 1000 } });
  await contexto.addInitScript(
    (usuario) => localStorage.setItem('tracbel-crm:contexto-acesso', JSON.stringify({ usuario, empresa: '010101' })),
    perfil.usuario,
  );

  const pagina = await contexto.newPage();
  const erros = [];
  pagina.on('pageerror', (e) => erros.push(e.message));
  pagina.on('console', (m) => {
    if (m.type() === 'error' && !m.text().includes('403')) erros.push(m.text());
  });

  const r = { erros };
  await pagina.goto(`${base}/#/relatorios/territorio`, { waitUntil: 'networkidle' });
  await esperarPainel(pagina);

  // LIDO NO DOM: o selectOption do Playwright escolhe até opção desligada, e isDisabled não é confiável em <option>.
  r.opcaoEmpresaDesligada = await filtro(pagina, 'Visão').evaluate((s) => s.querySelector('option[value="Empresa"]').disabled);
  r.alcance = (await pagina.locator('.terr-alcance').innerText()).replace(/\s+/g, ' ');
  r.selos = await pagina.locator('.terr-selo').allInnerTexts();
  r.poligonosNaVisaoDaFilial = await contarPoligonos(pagina);
  r.gruposNaVisaoDaFilial = await gruposForaDoMapa(pagina);
  await pagina.screenshot({ path: `${saida}/validacao-${perfil.nome}-1-filial.png`, fullPage: true });

  // O QUE A TELA NÃO DEIXA PEDIR, A API TAMBÉM RECUSA — pedido direto, com o cabeçalho do perfil.
  r.apiVisaoDaEmpresa = await pedirPelaApi(pagina, perfil.usuario, '?visao=Empresa');
  r.apiFilialDoClienteDeOutraLojaNaVisaoDaFilial = await pedirPelaApi(pagina, perfil.usuario, '?filialDoCliente=010103');
  r.apiFilialDoClienteDeOutraLojaNaVisaoDaEmpresa = await pedirPelaApi(pagina, perfil.usuario, '?visao=Empresa&filialDoCliente=010103');

  if (perfil.nome === 'sem-acesso') {
    // TELA ADULTERADA: força a opção desligada, como faria alguém mexendo no navegador. A API recusa.
    const resposta = pagina.waitForResponse((x) => x.url().includes(ROTA) && x.url().includes('visao=Empresa'), { timeout: 120000 });
    await filtro(pagina, 'Visão').selectOption('Empresa');
    r.tentativaForcadaDaEmpresa = (await resposta).status();
    await pagina.waitForSelector('text=O seu perfil não alcança o que foi pedido', { timeout: 60000 });
    r.mensagemDaRestricao = (await pagina.locator('main').innerText()).split('\n').find((l) => l.includes('exige a permissão')) ?? null;
    await pagina.screenshot({ path: `${saida}/validacao-${perfil.nome}-2-tentativa-forcada.png`, fullPage: true });
    await trocarFiltro(pagina, 'Visão', 'Filial', 'indicadores');
  }

  if (perfil.nome === 'autorizado') {
    await trocarFiltro(pagina, 'Visão', 'Empresa', 'visao=Empresa');
    r.alcanceNaVisaoDaEmpresa = (await pagina.locator('.terr-alcance').innerText()).replace(/\s+/g, ' ');
    r.poligonosNaVisaoDaEmpresa = await contarPoligonos(pagina);
    r.gruposNaVisaoDaEmpresa = await gruposForaDoMapa(pagina);
    r.indicadoresNaVisaoDaEmpresa = (await pagina.locator('.cad-kpi').allInnerTexts()).map((t) => t.replace(/\s+/g, ' '));
    await pagina.screenshot({ path: `${saida}/validacao-${perfil.nome}-2-empresa.png`, fullPage: true });

    const lojas = await filtro(pagina, 'Filial que vendeu').locator('option').evaluateAll((os) => os.map((o) => o.value).filter(Boolean));
    if (lojas.length > 0) {
      await trocarFiltro(pagina, 'Filial que vendeu', lojas[0], `filialDaVenda=${lojas[0]}`);
      r.filtroDaFilialQueVendeu = lojas[0];
      r.indicadoresComFiltroDaFilialQueVendeu = (await pagina.locator('.cad-kpi').allInnerTexts()).map((t) => t.replace(/\s+/g, ' '));
      await pagina.locator('.terr-grade-mapas').screenshot({ path: `${saida}/validacao-${perfil.nome}-3-empresa-filial-que-vendeu.png` });
      await trocarFiltro(pagina, 'Filial que vendeu', '', 'visao=Empresa');
    }

    await pagina.getByRole('button', { name: 'Ribeirão Preto', exact: true }).first().click();
    await pagina.waitForSelector('.terr-detalhe');
    await pagina.locator('.terr-detalhe').screenshot({ path: `${saida}/validacao-${perfil.nome}-4-empresa-detalhe.png` });
    await pagina.getByRole('button', { name: 'Fechar' }).click();
    await trocarFiltro(pagina, 'Visão', 'Filial', 'indicadores');
  }

  // A ADR NO MAPA, pelo filtro de região: 203 no total, e cada região com a sua parte.
  r.adrPorRegiao = {};
  for (const [rotulo, valor, trecho] of [['Norte', 'Norte', 'regiao=Norte'], ['Noroeste', 'Noroeste', 'regiao=Noroeste']]) {
    await trocarFiltro(pagina, 'Região da ADR', valor, trecho);
    r.adrPorRegiao[rotulo] = (await contarPoligonos(pagina))[0];
  }
  const semRegiao = pagina.waitForResponse((resposta) => resposta.url().includes(ROTA) && !resposta.url().includes('regiao='), { timeout: 120000 });
  await filtro(pagina, 'Região da ADR').selectOption('');
  await semRegiao;
  await esperarPainel(pagina);
  r.adrPorRegiao['Norte e Noroeste'] = (await contarPoligonos(pagina))[0];

  resultado[perfil.nome] = r;
  await contexto.close();
}

writeFileSync(`${saida}/validacao-perfis.json`, JSON.stringify(resultado, null, 2));
console.log(JSON.stringify(resultado, null, 2));
await navegador.close();
