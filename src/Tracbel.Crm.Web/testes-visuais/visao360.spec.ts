/**
 * A VISÃO 360 CABE NA LARGURA, E O ARRANJO SEGUE A LARGURA DO CONTEÚDO
 * (24/09/2026).
 *
 * Roda sobre o harness `#/dev/visao360-visual`, que monta a tela dentro do
 * `Layout` real — com o menu lateral, porque as quebras são pela largura do
 * CONTEÚDO, e o conteúdo é a janela menos o menu. Três estados: `completo`,
 * `vazio` (a produção de hoje) e `semCarteira` (cadastro carregado, nenhum
 * vínculo). Nenhum número daqui é dado da Tracbel.
 *
 * O QUE ELE PROVA, e o teste de componente não alcança por não ter CSS:
 *
 * - nenhuma largura ganha rolagem lateral — da de 390 à de 2.400;
 * - a página não deixa faixa vazia à direita: ela ocupa a coluna de conteúdo
 *   inteira até 2.100px de janela e, acima disso, fica centralizada;
 * - painel sem dado é compacto: não guarda a altura do vizinho;
 * - os cinco cartões ficam numa linha a partir de 1.100px de conteúdo (a tela
 *   do Ricardo tem ~1.240), 3 + 2 abaixo disso, depois dois e um;
 * - as linhas de painéis empilham abaixo das quebras declaradas;
 * - o número de cada cartão cabe com folga de 10% — a fonte do Linux do CI é
 *   mais larga que a do Windows, e o que cabe raspando aqui estoura lá;
 * - a tabela de composição, de onze colunas, rola dentro da caixa dela, e não a
 *   página.
 */

import { expect, test, type Page } from '@playwright/test';
import { LARGURAS } from '../playwright.config';

/**
 * AS LARGURAS DA CONFIGURAÇÃO E MAIS QUATRO: o monitor grande (2.400), a
 * maquete (1.672), a tela do Ricardo (1.536 — Windows a 125%) e a de 1.366 dos
 * notebooks. As quatro são as que a faixa vazia e a quebra dos cinco cartões
 * tinham errado.
 */
const TODAS = [
  ...LARGURAS,
  { nome: '2400x1200', largura: 2400, altura: 1200 },
  { nome: '1672x940', largura: 1672, altura: 940 },
  { nome: '1536x864', largura: 1536, altura: 864 },
  { nome: '1364x768', largura: 1364, altura: 768 },
] as const;

const ESTADOS = ['completo', 'vazio', 'semCarteira'] as const;

async function abrir(pagina: Page, estado: string) {
  await pagina.goto(`/#/dev/visao360-visual?estado=${estado}`);
  await expect(pagina.locator('[data-harness="visao360-visual"]')).toBeAttached();
  // Os cinco cartões e a última linha de painéis são as duas leituras que chegam por último.
  await expect(pagina.locator('[data-bloco="kpis"] [data-kpi]')).toHaveCount(5, { timeout: 30_000 });
  await expect(pagina.locator('[data-bloco="linha-4"]')).toBeVisible({ timeout: 30_000 });
  await pagina.waitForFunction(() => document.fonts.status === 'loaded');
}

/**
 * O quanto a página passa da janela, e os primeiros culpados. Como no teste de
 * Mercado, o que está dentro de um `<svg>` ou de um contêiner que rola não é
 * culpado: a tabela larga dentro de `overflow-x: auto` é o conserto.
 */
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

/** A largura do conteúdo da página — a mesma régua do `@container indicadores`. */
async function larguraDoConteudo(pagina: Page): Promise<number> {
  return pagina.locator('.dash-pagina').evaluate((n) => {
    const estilo = getComputedStyle(n);
    return n.clientWidth - parseFloat(estilo.paddingLeft) - parseFloat(estilo.paddingRight);
  });
}

/** Quantas linhas visuais os filhos de um bloco ocupam, pelo topo de cada um. */
async function linhasDe(pagina: Page, seletor: string): Promise<number> {
  const topos = await pagina.locator(seletor).evaluateAll((nós) => nós.map((n) => Math.round(n.getBoundingClientRect().top)));
  return new Set(topos).size;
}

for (const { nome, largura, altura } of TODAS) {
  test.describe(`Visão 360 em ${nome}`, () => {
    test.use({ viewport: { width: largura, height: altura } });

    for (const estado of ESTADOS) {
      test(`${estado} cabe na largura e é capturado`, async ({ page }) => {
        await abrir(page, estado);
        await page.screenshot({ path: `capturas/${nome}/visao360-${estado}.png`, fullPage: true });

        const { sobra, culpados } = await sobraLateral(page);
        expect(
          sobra,
          `A página passa ${sobra}px da janela de ${largura}px. Primeiros culpados: ${culpados.join(' · ') || 'nenhum'}`,
        ).toBeLessThanOrEqual(0);
      });
    }

    test('a página ocupa a coluna de conteúdo inteira até 2.100px de janela, e acima disso fica centralizada', async ({ page }) => {
      // O DEFEITO: a rota não era larga, e o `.content` parava em 1.400px — numa janela de 1.920
      // sobravam ~320px vazios à direita do painel, e na tela real do Ricardo (~1.700) ~460px.
      // Pedido dele: sem faixa nenhuma; teto, só acima de 2.100px, e centralizado.
      await abrir(page, 'completo');
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
    });

    test('painel sem dado é compacto: não guarda a altura do vizinho nem a do gráfico que não tem', async ({ page }) => {
      // Pedido do Ricardo (24/09/2026): Top CENs, Mix, Conhecimento de mercado, Faturamento e Top 5
      // vazios ficavam altíssimos — esticados até o painel mais alto da linha. Duas afirmações:
      // o painel não tem sobra além do que desenha (nenhum espaço reservado), e com a página a
      // partir de 700px ele cabe em 240px — título, subtítulo e a frase do motivo em até quatro
      // linhas medem 196px no pior caso (1.440, coluna de um terço); o resto é a folga da fonte
      // do Linux do CI. Antes, esticados, eram 360 a 440px.
      await abrir(page, 'semCarteira');
      const conteudo = await larguraDoConteudo(page);
      const paineis = await page.locator('.v360-card:has(.v360-sem-dado)').evaluateAll((nós) =>
        nós.map((n) => {
          const estilo = getComputedStyle(n);
          const miolo = n.clientHeight - parseFloat(estilo.paddingTop) - parseFloat(estilo.paddingBottom);
          const desenhado = [...n.children].reduce((s, filho) => {
            const e = getComputedStyle(filho);
            return s + (filho as HTMLElement).offsetHeight + parseFloat(e.marginTop) + parseFloat(e.marginBottom);
          }, 0);
          return {
            titulo: n.querySelector('.v360-card-title')?.textContent,
            altura: Math.round(n.getBoundingClientRect().height),
            sobra: Math.round(miolo - desenhado),
          };
        }),
      );
      expect(paineis.length, 'o estado sem carteira deveria ter painéis sem dado').toBeGreaterThan(0);
      for (const { titulo, altura: h, sobra } of paineis) {
        expect(sobra, `"${titulo}" sem dado guarda ${sobra}px vazios abaixo do motivo`).toBeLessThanOrEqual(2);
        if (conteudo >= 700) expect(h, `"${titulo}" sem dado tem ${h}px`).toBeLessThanOrEqual(240);
      }
    });

    test('os cinco cartões seguem o arranjo declarado para a largura do conteúdo, e o número cabe com folga', async ({ page }) => {
      await abrir(page, 'completo');
      const conteudo = await larguraDoConteudo(page);
      const linhas = await linhasDe(page, '[data-bloco="kpis"] > [data-kpi]');

      // 5 numa linha a partir de 1.100px; 3 + 2 de 700 a 1.099; 2 + 2 + 1 de 520 a 699; um por linha abaixo.
      const esperado = conteudo >= 1100 ? 1 : conteudo >= 700 ? 2 : conteudo >= 520 ? 3 : 5;
      expect(linhas, `com ${conteudo}px de conteúdo esperava ${esperado} linha(s) de cartões`).toBe(esperado);

      // FOLGA DE 10% NO QUE É APERTADO: o número (medido pelo texto, e não pela caixa) e cada
      // palavra do título, que só quebra entre palavras — a última vai colada ao ⓘ.
      const medidas = await page.evaluate(() => {
        const larguraDoTexto = (no: Node, inicio: number, fim: number) => {
          const r = document.createRange();
          r.setStart(no, inicio);
          r.setEnd(no, fim);
          return r.getBoundingClientRect().width;
        };
        return [...document.querySelectorAll<HTMLElement>('[data-bloco="kpis"] > [data-kpi]')].map((cartao) => {
          const valor = cartao.querySelector<HTMLElement>('.v360-kpi-valor')!;
          const texto = valor.firstChild!;
          const titulo = cartao.querySelector<HTMLElement>('.v360-kpi-titulo')!;
          const palavras: number[] = [];
          for (const no of titulo.childNodes) {
            if (no.nodeType !== Node.TEXT_NODE) continue;
            const conteudo = no.textContent ?? '';
            let inicio = 0;
            for (const palavra of conteudo.split(' ')) {
              if (palavra) palavras.push(larguraDoTexto(no, inicio, inicio + palavra.length));
              inicio += palavra.length + 1;
            }
          }
          const fim = titulo.querySelector<HTMLElement>('.v360-kpi-titulo-fim');
          if (fim) palavras.push(fim.getBoundingClientRect().width);
          return {
            kpi: cartao.dataset.kpi,
            valor: larguraDoTexto(texto, 0, (texto.textContent ?? '').length),
            caixaDoValor: valor.clientWidth,
            maiorPalavra: Math.max(...palavras),
            caixaDoTitulo: titulo.clientWidth,
          };
        });
      });

      for (const m of medidas) {
        expect(m.valor, `o número de "${m.kpi}" ocupa ${Math.round(m.valor)} de ${m.caixaDoValor}px`).toBeLessThanOrEqual(m.caixaDoValor * 0.9);
        expect(
          m.maiorPalavra,
          `a maior palavra do título de "${m.kpi}" ocupa ${Math.round(m.maiorPalavra)} de ${m.caixaDoTitulo}px`,
        ).toBeLessThanOrEqual(m.caixaDoTitulo * 0.9);
      }
    });

    test('as linhas de painéis empilham abaixo das quebras declaradas', async ({ page }) => {
      await abrir(page, 'completo');
      const conteudo = await larguraDoConteudo(page);

      // Linhas 2 e 3: três colunas a partir de 1.100px; duas + uma inteira de 700 a 1.099; empilhadas abaixo.
      const esperadoDasTres = conteudo >= 1100 ? 1 : conteudo >= 700 ? 2 : 3;
      for (const linha of ['linha-2', 'linha-3']) {
        expect(await linhasDe(page, `[data-bloco="${linha}"] > *`), `${linha} com ${conteudo}px de conteúdo`).toBe(esperadoDasTres);
      }

      // Linha 4 (vendas perdidas 1,6 : 1 alertas): lado a lado a partir de 900px.
      expect(await linhasDe(page, '[data-bloco="linha-4"] > *'), `linha-4 com ${conteudo}px de conteúdo`).toBe(conteudo >= 900 ? 1 : 2);

      // Nenhum painel passa da borda da página.
      const paginaDireita = await page.locator('.dash-pagina').evaluate((n) => n.getBoundingClientRect().right);
      const direitas = await page
        .locator('[data-bloco^="linha-"] > *')
        .evaluateAll((nós) => nós.map((n) => n.getBoundingClientRect().right));
      for (const d of direitas) expect(d).toBeLessThanOrEqual(paginaDireita + 1);
    });

    test('os perfis e o cabeçalho cabem na largura', async ({ page }) => {
      await abrir(page, 'completo');
      const direita = await page.locator('.v360-perfil-toggle').evaluate((n) => n.getBoundingClientRect().right);
      const janela = await page.evaluate(() => document.documentElement.clientWidth);
      expect(Math.round(direita), 'a faixa de perfis sai pela direita da janela').toBeLessThanOrEqual(janela);
    });

    test('a tabela de composição rola dentro da própria caixa, e não a página', async ({ page }) => {
      await abrir(page, 'vazio');
      await page.locator('[data-bloco="composicao"] > summary').click();
      const caixa = page.locator('[data-bloco="composicao"] .cad-tabela-wrap');
      await expect(caixa).toBeVisible();
      await page.screenshot({ path: `capturas/${nome}/visao360-composicao.png`, fullPage: true });

      const m = await caixa.evaluate((n) => ({
        tabela: n.querySelector('table')!.getBoundingClientRect().width,
        visivel: n.clientWidth,
        rolagem: n.scrollWidth,
        direita: n.getBoundingClientRect().right,
        overflow: getComputedStyle(n).overflowX,
      }));
      const janela = await page.evaluate(() => document.documentElement.clientWidth);

      expect(Math.round(m.direita), 'a caixa da tabela sai pela direita da janela').toBeLessThanOrEqual(janela);
      if (m.tabela > m.visivel + 1) {
        expect(m.overflow, 'a tabela é mais larga que a caixa e a caixa não rola').toBe('auto');
        expect(m.rolagem).toBeGreaterThan(m.visivel);
      }

      const { sobra, culpados } = await sobraLateral(page);
      expect(sobra, `com a composição aberta a página passa ${sobra}px. Culpados: ${culpados.join(' · ')}`).toBeLessThanOrEqual(0);
    });
  });
}
