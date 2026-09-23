/**
 * A TELA DE MERCADO CABE NA LARGURA — em seis larguras e nove estados (fase T4.5).
 *
 * A AFIRMAÇÃO É UMA SÓ, e é objetiva: `document.documentElement.scrollWidth` não
 * passa da largura da janela. Não é gosto — rolagem lateral numa tela de
 * indicadores esconde coluna, e ninguém rola para o lado atrás de um número que
 * não sabe que existe.
 *
 * AS CAPTURAS NÃO SÃO COMPARADAS PIXEL A PIXEL AQUI, e é decisão, não preguiça:
 * a referência da issue 171 é o protótipo, e adotar como base uma captura do
 * estado atual congelaria o layout de hoje como se fosse o alvo. Elas ficam em
 * `capturas/`, para olho humano e para a comparação da 171 quando ela vier.
 *
 * NENHUM NÚMERO AQUI É DADO DA TRACBEL: tudo vem do harness `#/dev/mercado-visual`,
 * que responde a API com amostra fictícia e carimba isso na barra do topo.
 */

import { expect, test, type Page } from '@playwright/test';
import { LARGURAS } from '../playwright.config';

/** Os nove estados, na ordem em que o harness os lista. */
const ESTADOS = [
  'completo',
  'parcialmenteVazio',
  'municipioSelecionado',
  'fichaAberta',
  'sigiloIbge',
  'muitasCulturas',
  'textosLongos',
  'carregando',
  'erro',
] as const;

/** Abre um estado do harness e espera a página assentar. */
async function abrir(pagina: Page, estado: string) {
  await pagina.goto(`/#/dev/mercado-visual?estado=${estado}`);
  await expect(pagina.locator('[data-harness="mercado-visual"]')).toBeVisible();

  // O cabeçalho da tela aparece antes da resposta da API: é o que garante que a
  // casca montou, inclusive nos estados `carregando` e `erro`.
  await expect(pagina.locator('[data-bloco="cabecalho"]')).toBeVisible();

  if (estado !== 'carregando' && estado !== 'erro') {
    // Os mapas são a última coisa a desenhar, e é depois deles que a altura para
    // de mudar. Capturar antes pegaria a página no meio do caminho.
    await expect(pagina.locator('[data-bloco="mapas"]')).toBeVisible({ timeout: 30_000 });
  }

  // O harness abre os `<details>` da ficha 300 ms depois de montar.
  if (estado === 'fichaAberta') await pagina.waitForTimeout(600);

  await pagina.waitForFunction(() => document.fonts.status === 'loaded');
}

/** O quanto a página passa da largura da janela. Zero é o único valor aceito. */
async function sobraLateral(pagina: Page) {
  return pagina.evaluate(() => {
    const raiz = document.documentElement;
    return {
      sobra: raiz.scrollWidth - window.innerWidth,
      // Quem está estourando: ajuda a corrigir sem abrir o navegador à mão.
      culpados: [...document.querySelectorAll<HTMLElement>('body *')]
        .filter((e) => e.getBoundingClientRect().right > window.innerWidth + 1)
        .slice(0, 5)
        .map((e) => `${e.tagName.toLowerCase()}.${e.className || '(sem classe)'}`),
    };
  });
}

for (const { nome, largura, altura } of LARGURAS) {
  test.describe(`Mercado em ${nome}`, () => {
    test.use({ viewport: { width: largura, height: altura } });

    for (const estado of ESTADOS) {
      test(`${estado} cabe na largura e é capturado`, async ({ page }) => {
        await abrir(page, estado);

        await page.screenshot({
          path: `capturas/${nome}/mercado-${estado}.png`,
          fullPage: true,
        });

        const { sobra, culpados } = await sobraLateral(page);
        expect(
          sobra,
          `A página passa ${sobra}px da janela de ${largura}px. Primeiros culpados: ${culpados.join(' · ') || 'nenhum identificado'}`,
        ).toBeLessThanOrEqual(0);
      });
    }

    test('Território cabe na largura e é capturado', async ({ page }) => {
      await abrir(page, 'completo');
      await page.getByRole('tab', { name: 'Território' }).click();
      await expect(page.locator('[role="tabpanel"]')).toBeVisible();
      await page.screenshot({ path: `capturas/${nome}/territorio.png`, fullPage: true });

      const { sobra, culpados } = await sobraLateral(page);
      expect(sobra, `Território passa ${sobra}px. Culpados: ${culpados.join(' · ')}`).toBeLessThanOrEqual(0);
    });

    test('a dica junto da borda direita não sai da tela', async ({ page }) => {
      // ============================================================================
      // DEFEITO MEDIDO NA T4.5, CORRIGIDO NA T4.6 — e é por isso que este teste
      // deixou de ser uma falha esperada.
      //
      // O balão era posicionado só por CSS, sem detecção de colisão e sem portal.
      // Junto da borda direita ele vazava a janela — 2016px numa tela de 1920, 803px
      // numa de 768, 472px numa de 390 — e dentro de um cartão com `overflow:hidden`
      // chegava a ser RECORTADO: o texto que explica o número desaparecia justamente
      // para quem tinha ido procurá-lo.
      //
      // O conserto foi `@radix-ui/react-tooltip` por dentro do `InfoTooltip`, em modo
      // CONTROLADO — o Radix cuida de portal, colisão e posição; abrir e fechar
      // continua nosso, porque o Radix é ponteiro-e-foco por design e o toque é
      // requisito declarado do componente.
      //
      // Se esta afirmação voltar a falhar, foi porque alguém pôs posição de volta no
      // CSS do balão, ou tirou o portal.
      // ============================================================================
      await abrir(page, 'completo');

      const dicas = page.locator('.dica-gatilho');
      const quantas = await dicas.count();
      test.skip(quantas === 0, 'nenhuma dica na tela nesta largura');

      // A dica mais à direita é a que tem mais chance de vazar.
      let maisADireita = 0;
      let maiorX = -Infinity;
      for (let i = 0; i < quantas; i++) {
        const caixa = await dicas.nth(i).boundingBox();
        if (caixa && caixa.x > maiorX) {
          maiorX = caixa.x;
          maisADireita = i;
        }
      }

      await dicas.nth(maisADireita).focus();
      const balao = page.locator('[role="tooltip"]').first();
      await expect(balao).toBeVisible();

      await page.screenshot({ path: `capturas/${nome}/dica-na-borda.png` });

      const caixa = await balao.boundingBox();
      expect(caixa, 'o balão da dica não tem caixa — ele foi recortado').not.toBeNull();
      expect(
        caixa!.x + caixa!.width,
        `o balão termina em ${Math.round(caixa!.x + caixa!.width)}px, fora da janela de ${largura}px`,
      ).toBeLessThanOrEqual(largura);
      expect(caixa!.x, 'o balão começa fora da janela, à esquerda').toBeGreaterThanOrEqual(0);
    });
  });
}
