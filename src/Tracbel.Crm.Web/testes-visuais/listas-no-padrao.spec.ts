/**
 * AS LISTAS DO CADASTRO NO PADRÃO DOS INDICADORES GEOGRÁFICOS (29/09/2026, #293 bloco 5) — Clientes e Equipamentos —
 * CABEM NA LARGURA E TÊM A FORMA DO PADRÃO.
 *
 * Roda sobre o harness da Visão 360 aberto na rota de cada lista (`#/dev/visao360-visual?rota=/clientes`), com as
 * amostras de `amostrasDoCadastro.ts`. Nenhum número daqui é dado da Tracbel.
 *
 * Arquivo próprio, e não mais um bloco em `telas-no-padrao.spec.ts`: as listas não têm cartão de decisão, e é pela
 * contagem dos cartões que aquele arquivo sabe que a leitura voltou.
 *
 * O QUE ELE PROVA:
 *
 * - nenhuma largura ganha rolagem lateral, da de 390 à de 2.400, com dado e sem dado — a tabela pode rolar dentro do
 *   painel dela, a página não;
 * - a página ocupa a coluna de conteúdo inteira até 2.100px de janela;
 * - as peças do padrão estão lá: cabeçalho com a hora da leitura, barra de filtros, seção e painel;
 * - "Mais filtros" dos Equipamentos abre o popover dentro da janela, e o selo conta o filtro ligado.
 */

import { expect, test, type Page } from '@playwright/test';
import { LARGURAS } from '../playwright.config';

const TODAS = [
  ...LARGURAS,
  { nome: '2400x1200', largura: 2400, altura: 1200 },
  { nome: '1536x864', largura: 1536, altura: 864 },
] as const;

const LISTAS = [
  { nome: 'clientes', rota: '/clientes' },
  { nome: 'equipamentos', rota: '/equipamentos' },
] as const;

const ESTADOS = ['completo', 'vazio'] as const;

async function abrir(pagina: Page, rota: string, estado: string) {
  await pagina.goto(`/#/dev/visao360-visual?estado=${estado}&rota=${encodeURIComponent(rota)}`);
  await expect(pagina.locator('[data-harness="visao360-visual"]')).toBeAttached();
  // A LEITURA VOLTOU quando o painel tem a tabela (com dado) ou o estado vazio (sem dado) — o "Carregando" também é
  // `cad-estado`, mas é o único com `role="status"`.
  await expect(pagina.locator('.mom-painel .mom-tabela, .mom-painel .cad-estado:not([role="status"])').first()).toBeVisible({
    timeout: 30_000,
  });
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
  test.describe(`Listas do cadastro no padrão dos Indicadores em ${nome}`, () => {
    test.use({ viewport: { width: largura, height: altura } });

    for (const lista of LISTAS) {
      for (const estado of ESTADOS) {
        test(`${lista.nome} ${estado} cabe na largura e é capturada`, async ({ page }) => {
          await abrir(page, lista.rota, estado);
          await page.screenshot({ path: `capturas/${nome}/padrao-${lista.nome}-${estado}.png`, fullPage: true });

          const { sobra, culpados } = await sobraLateral(page);
          expect(
            sobra,
            `A página passa ${sobra}px da janela de ${largura}px. Primeiros culpados: ${culpados.join(' · ') || 'nenhum'}`,
          ).toBeLessThanOrEqual(0);
        });
      }

      test(`${lista.nome}: as peças do padrão e a página na coluna inteira`, async ({ page }) => {
        await abrir(page, lista.rota, 'completo');

        await expect(page.locator('.dash-pagina.dash-pagina-larga')).toHaveCount(1);
        await expect(page.locator('.page-header .dash-atualizado')).toBeVisible();
        await expect(page.locator('[data-bloco="filtros"] .dash-filtros-linha')).toBeVisible();
        await expect(page.locator('section.dash-secao .terr-secao-titulo')).toHaveCount(1);
        await expect(page.locator('.mom-painel .mom-tabela')).toHaveCount(1);

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
      });
    }

    test('equipamentos: "Mais filtros" abre dentro da janela, e o selo conta o filtro ligado', async ({ page }) => {
      await abrir(page, '/equipamentos', 'completo');

      const botao = page.getByRole('button', { name: /Mais filtros/ });
      await botao.click();
      const popover = page.locator('.dash-popover');
      await expect(popover).toBeVisible();
      await page.screenshot({ path: `capturas/${nome}/padrao-equipamentos-mais-filtros.png` });

      const caixa = (await popover.boundingBox())!;
      expect(caixa.x, 'o popover começa fora da janela').toBeGreaterThanOrEqual(0);
      expect(caixa.x + caixa.width, 'o popover passa da janela').toBeLessThanOrEqual(largura);

      await popover.getByLabel('Mostrar baixados').check();
      await page.keyboard.press('Escape');
      await expect(botao).toContainText('1');
    });
  });
}
