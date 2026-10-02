/**
 * AS TELAS QUE PASSARAM PARA O PADRÃO DOS INDICADORES GEOGRÁFICOS (29/09/2026) — Funil de Vendas, Pipeline de Vendas e
 * Cobertura de Carteira (bloco 1); Agenda, Performance de CEN e Cobertura por Filial (bloco 2); Forecast da Gerência,
 * Estoque e Cobertura e Conferência com a GN (bloco 3) — CABEM NA LARGURA E TÊM A FORMA DO PADRÃO.
 *
 * Roda sobre o harness da Visão 360 aberto na rota de cada tela (`#/dev/visao360-visual?rota=…`): as três leem as
 * mesmas rotas que a Visão 360 soma, com as mesmas amostras fictícias, dentro do `Layout` real com o menu lateral —
 * as quebras são pela largura do CONTEÚDO. Nenhum número daqui é dado da Tracbel.
 *
 * O QUE ELE PROVA, e o teste de componente não alcança por não ter CSS:
 *
 * - nenhuma largura ganha rolagem lateral, da de 390 à de 2.400, com dado e sem dado;
 * - a página ocupa a coluna de conteúdo inteira até 2.100px de janela (`dash-pagina-larga`), e acima disso fica
 *   centralizada — o pedido do Ricardo de 27/09 ("sem faixa vazia") vale para as telas novas do padrão;
 * - os cartões de uma mesma linha têm a mesma altura, e o número de cada um cabe no cartão;
 * - as peças do padrão estão lá: cabeçalho com a hora da leitura, barra de filtros, cartões de decisão e seções.
 */

import { expect, test, type Page } from '@playwright/test';
import { LARGURAS } from '../playwright.config';

const TODAS = [
  ...LARGURAS,
  { nome: '2400x1200', largura: 2400, altura: 1200 },
  { nome: '1536x864', largura: 1536, altura: 864 },
] as const;

const TELAS = [
  // Bloco 1 (#294).
  { nome: 'funil', rota: '/relatorios/funil', cartoes: 4, secoes: 2 },
  { nome: 'pipeline', rota: '/pipeline', cartoes: 4, secoes: 2 },
  { nome: 'cobertura', rota: '/cobertura', cartoes: 5, secoes: 2 },
  // Bloco 2 (#293, 29/09/2026).
  { nome: 'agenda', rota: '/agenda', cartoes: 6, secoes: 2 },
  { nome: 'performance', rota: '/relatorios/performance', cartoes: 4, secoes: 2 },
  { nome: 'cobertura-por-filial', rota: '/relatorios/cobertura', cartoes: 5, secoes: 2 },
  // Bloco 3 (#293, 29/09/2026): os relatórios da Gestão de Negócios, com as amostras deles no mesmo harness.
  { nome: 'forecast', rota: '/relatorios/forecast', cartoes: 5, secoes: 1 },
  { nome: 'estoque', rota: '/relatorios/estoque', cartoes: 6, secoes: 2 },
  { nome: 'conferencia', rota: '/relatorios/conferencia', cartoes: 5, secoes: 2 },
] as const;

const ESTADOS = ['completo', 'vazio'] as const;

async function abrir(pagina: Page, rota: string, estado: string, cartoes: number) {
  await pagina.goto(`/#/dev/visao360-visual?estado=${estado}&rota=${encodeURIComponent(rota)}`);
  await expect(pagina.locator('[data-harness="visao360-visual"]')).toBeAttached();
  await expect(pagina.locator('[data-bloco="kpis"] [data-kpi]')).toHaveCount(cartoes, { timeout: 30_000 });
  // A LEITURA VOLTOU quando nenhum cartão pulsa mais.
  await expect(pagina.locator('[data-bloco="kpis"] .cad-kpi-esqueleto')).toHaveCount(0, { timeout: 30_000 });
  await pagina.waitForFunction(() => document.fonts.status === 'loaded');
}

/** O quanto a página passa da janela, e os primeiros culpados — o que está dentro de algo que rola não é culpado. */
async function sobraLateral(pagina: Page) {
  return pagina.evaluate(() => {
    const raiz = document.documentElement;
    const dentroDeAlgoQueRola = (e: HTMLElement) => {
      for (let p = e.parentElement; p && p !== document.body; p = p.parentElement) {
        const x = getComputedStyle(p).overflowX;
        if (x === 'auto' || x === 'scroll' || x === 'hidden') return true;
      }
      return false;
    };
    return {
      sobra: raiz.scrollWidth - raiz.clientWidth,
      culpados: [...document.querySelectorAll<HTMLElement>('body *')]
        .filter((e) => !e.closest('svg') && !dentroDeAlgoQueRola(e))
        .filter((e) => e.getBoundingClientRect().right > raiz.clientWidth + 1)
        .slice(0, 5)
        .map((e) => `${e.tagName.toLowerCase()}.${e.className || '(sem classe)'}`),
    };
  });
}

for (const { nome, largura, altura } of TODAS) {
  test.describe(`Telas no padrão dos Indicadores em ${nome}`, () => {
    test.use({ viewport: { width: largura, height: altura } });

    for (const tela of TELAS) {
      for (const estado of ESTADOS) {
        test(`${tela.nome} ${estado} cabe na largura e é capturado`, async ({ page }) => {
          await abrir(page, tela.rota, estado, tela.cartoes);
          await page.screenshot({ path: `capturas/${nome}/padrao-${tela.nome}-${estado}.png`, fullPage: true });

          const { sobra, culpados } = await sobraLateral(page);
          expect(
            sobra,
            `A página passa ${sobra}px da janela de ${largura}px. Primeiros culpados: ${culpados.join(' · ') || 'nenhum'}`,
          ).toBeLessThanOrEqual(0);
        });
      }

      test(`${tela.nome}: as peças do padrão, a página na coluna inteira e os cartões iguais`, async ({ page }) => {
        await abrir(page, tela.rota, 'completo', tela.cartoes);

        // AS PEÇAS DO PADRÃO.
        await expect(page.locator('.dash-pagina.dash-pagina-larga')).toHaveCount(1);
        await expect(page.locator('.page-header .dash-atualizado')).toBeVisible();
        await expect(page.locator('[data-bloco="filtros"] .dash-filtros-linha')).toBeVisible();
        await expect(page.locator('section.dash-secao .terr-secao-titulo')).toHaveCount(tela.secoes);
        expect(await page.locator('.mom-painel').count()).toBeGreaterThanOrEqual(tela.secoes);

        // SEM FAIXA VAZIA até 2.100px de janela; acima disso, centralizada.
        const { esquerda, direita } = await page.evaluate(() => {
          const conteudo = document.querySelector<HTMLElement>('.content')!;
          const estilo = getComputedStyle(conteudo);
          const caixa = conteudo.getBoundingClientRect();
          const pagina = document.querySelector('.dash-pagina')!.getBoundingClientRect();
          return {
            esquerda: pagina.left - (caixa.left + parseFloat(estilo.paddingLeft)),
            direita: caixa.right - parseFloat(estilo.paddingRight) - pagina.right,
          };
        });
        const sobra = `sobra ${Math.round(esquerda)}px à esquerda e ${Math.round(direita)}px à direita`;
        if (largura < 2100) {
          expect(Math.round(esquerda), sobra).toBe(0);
          expect(Math.round(direita), sobra).toBe(0);
        } else {
          expect(Math.abs(esquerda - direita), sobra).toBeLessThanOrEqual(2);
        }

        // OS CARTÕES DE UMA MESMA LINHA TÊM A MESMA ALTURA, e o número cabe em cada um.
        const cartoes = await page.locator('[data-bloco="kpis"] .mv-kpi').evaluateAll((nós) =>
          nós.map((n) => {
            const caixa = n.getBoundingClientRect();
            const valor = n.querySelector<HTMLElement>('.mv-kpi-valor');
            return {
              topo: Math.round(caixa.top),
              altura: Math.round(caixa.height),
              cabe: !valor || valor.scrollWidth <= valor.clientWidth + 1,
              nome: n.getAttribute('data-kpi'),
            };
          }),
        );
        const porLinha = new Map<number, number[]>();
        for (const c of cartoes) porLinha.set(c.topo, [...(porLinha.get(c.topo) ?? []), c.altura]);
        for (const [topo, alturas] of porLinha)
          expect(Math.max(...alturas) - Math.min(...alturas), `cartões na linha de topo ${topo}: ${alturas.join(', ')}`).toBeLessThanOrEqual(1);
        for (const c of cartoes) expect(c.cabe, `o número de "${c.nome}" não cabe no cartão`).toBe(true);
      });
    }
  });
}

/**
 * AS TELAS DE INTELIGÊNCIA DE MERCADO (bloco 4 do #293, 29/09/2026) — Demanda e Previsão, Diagnóstico Comercial e (02/10) o
 * Dimensionamento ADR. Elas
 * leem as rotas de mercado, que o harness do SHELL responde com as amostras de Mercado (`#/dev/shell-visual?rota=…`), e
 * não o da Visão 360. As mesmas provas das outras: cabem na largura, página na coluna inteira, as peças do padrão e os
 * cartões iguais.
 */
const TELAS_DE_MERCADO = [
  { nome: 'demanda', rota: '/mercado/demanda', cartoes: 4, secoes: 2 },
  { nome: 'diagnostico', rota: '/mercado/diagnostico', cartoes: 4, secoes: 2 },
  { nome: 'dimensionamento', rota: '/mercado/dimensionamento', cartoes: 4, secoes: 3 },
  { nome: 'financiamentos', rota: '/mercado/financiamentos', cartoes: 4, secoes: 2 },
  { nome: 'precos', rota: '/mercado/precos', cartoes: 4, secoes: 2 },
] as const;

async function abrirNoShell(pagina: Page, rota: string, cartoes: number) {
  await pagina.goto(`/#/dev/shell-visual?estado=completo&rota=${encodeURIComponent(rota)}`);
  await expect(pagina.locator('.app-shell')).toBeVisible({ timeout: 30_000 });
  await expect(pagina.locator('[data-bloco="kpis"] [data-kpi]')).toHaveCount(cartoes, { timeout: 30_000 });
  await expect(pagina.locator('[data-bloco="kpis"] .cad-kpi-esqueleto')).toHaveCount(0, { timeout: 30_000 });
  await expect(pagina.locator('section.dash-secao').first()).toBeVisible({ timeout: 30_000 });
  await pagina.waitForFunction(() => document.fonts.status === 'loaded');
}

for (const { nome, largura, altura } of TODAS) {
  test.describe(`Telas de mercado no padrão dos Indicadores em ${nome}`, () => {
    test.use({ viewport: { width: largura, height: altura } });

    for (const tela of TELAS_DE_MERCADO) {
      test(`${tela.nome}: cabe na largura, tem as peças do padrão e os cartões iguais`, async ({ page }) => {
        await abrirNoShell(page, tela.rota, tela.cartoes);
        await page.screenshot({ path: `capturas/${nome}/padrao-${tela.nome}.png`, fullPage: true });

        const { sobra, culpados } = await sobraLateral(page);
        expect(
          sobra,
          `A página passa ${sobra}px da janela de ${largura}px. Primeiros culpados: ${culpados.join(' · ') || 'nenhum'}`,
        ).toBeLessThanOrEqual(0);

        await expect(page.locator('.dash-pagina.dash-pagina-larga')).toHaveCount(1);
        await expect(page.locator('.page-header .dash-atualizado')).toBeVisible();
        await expect(page.locator('[data-bloco="filtros"] .dash-filtros-linha')).toBeVisible();
        await expect(page.locator('section.dash-secao .terr-secao-titulo')).toHaveCount(tela.secoes);
        expect(await page.locator('.mom-painel').count()).toBeGreaterThanOrEqual(tela.secoes);

        const { esquerda, direita } = await page.evaluate(() => {
          const conteudo = document.querySelector<HTMLElement>('.content')!;
          const estilo = getComputedStyle(conteudo);
          const caixa = conteudo.getBoundingClientRect();
          const pagina = document.querySelector('.dash-pagina')!.getBoundingClientRect();
          return {
            esquerda: pagina.left - (caixa.left + parseFloat(estilo.paddingLeft)),
            direita: caixa.right - parseFloat(estilo.paddingRight) - pagina.right,
          };
        });
        const faixa = `sobra ${Math.round(esquerda)}px à esquerda e ${Math.round(direita)}px à direita`;
        if (largura < 2100) {
          expect(Math.round(esquerda), faixa).toBe(0);
          expect(Math.round(direita), faixa).toBe(0);
        } else {
          expect(Math.abs(esquerda - direita), faixa).toBeLessThanOrEqual(2);
        }

        const cartoes = await page.locator('[data-bloco="kpis"] .mv-kpi').evaluateAll((nós) =>
          nós.map((n) => {
            const caixa = n.getBoundingClientRect();
            const valor = n.querySelector<HTMLElement>('.mv-kpi-valor');
            return {
              topo: Math.round(caixa.top),
              altura: Math.round(caixa.height),
              cabe: !valor || valor.scrollWidth <= valor.clientWidth + 1,
              nome: n.getAttribute('data-kpi'),
            };
          }),
        );
        const porLinha = new Map<number, number[]>();
        for (const c of cartoes) porLinha.set(c.topo, [...(porLinha.get(c.topo) ?? []), c.altura]);
        for (const [topo, alturas] of porLinha)
          expect(Math.max(...alturas) - Math.min(...alturas), `cartões na linha de topo ${topo}: ${alturas.join(', ')}`).toBeLessThanOrEqual(1);
        for (const c of cartoes) expect(c.cabe, `o número de "${c.nome}" não cabe no cartão`).toBe(true);
      });
    }
  });
}