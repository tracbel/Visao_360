/**
 * O SHELL CABE NA LARGURA, E O MENU CONTINUA USÁVEL — nas seis larguras
 * (fidelidade às maquetes, 23/09/2026).
 *
 * Roda sobre o harness `#/dev/shell-visual`, que monta o `Layout` real com
 * sessão e filial fictícias. O que ele prova, e que o teste de componente não
 * alcança por não ter CSS nem teclado de verdade:
 *
 * - nenhuma largura ganha rolagem lateral por causa do menu ou da barra;
 * - "Indicadores Geográficos" cabe inteiro no item, sem reticências;
 * - abaixo de 900 px o menu é uma gaveta que abre no ☰, recebe o foco e fecha
 *   no Esc;
 * - o Sair se alcança só com o teclado: Tab até o usuário, Enter, Tab.
 */

import { expect, test, type Page } from '@playwright/test';
import { LARGURAS } from '../playwright.config';

async function abrir(pagina: Page, rota: string) {
  await pagina.goto(`/#/dev/shell-visual?estado=completo&rota=${rota}`);
  await expect(pagina.locator('.app-shell')).toBeVisible({ timeout: 30_000 });
  // O nome da pessoa só aparece depois que a sessão fictícia respondeu.
  await expect(pagina.getByText('Amostra Fictícia').first()).toBeAttached();
  await pagina.waitForFunction(() => document.fonts.status === 'loaded');
}

async function sobraLateral(pagina: Page) {
  return pagina.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
}

for (const { nome, largura, altura } of LARGURAS) {
  test.describe(`Shell em ${nome}`, () => {
    test.use({ viewport: { width: largura, height: altura } });
    const gaveta = largura <= 900;

    for (const rota of ['/relatorios/territorio', '/clientes', '/config']) {
      test(`${rota} cabe na largura e é capturado`, async ({ page }) => {
        await abrir(page, rota);
        await page.screenshot({ path: `capturas/${nome}/shell${rota.replace(/\//g, '-')}.png` });
        expect(await sobraLateral(page), `a página passa da janela de ${largura}px`).toBeLessThanOrEqual(0);
      });
    }

    test('com o aviso da filial recusada (P-20), a barra do topo cabe na largura e na própria altura', async ({ page }) => {
      // O DEFEITO QUE ISTO IMPEDE DE VOLTAR (revisão de 24/09/2026): a caixa da
      // filial não encolhia e o aviso não quebrava — com ele, a barra passava
      // 28px da janela em 1.024 e 175px em 390, e TODA tela ganhava rolagem
      // lateral no dia em que a filial era trocada sem a pessoa pedir.
      await page.goto('/#/dev/shell-visual?estado=completo&rota=/relatorios/territorio&filial=recusada');
      const aviso = page.locator('.topbar .cad-filial-aviso');
      await expect(aviso).toBeVisible({ timeout: 30_000 });
      await page.waitForFunction(() => document.fonts.status === 'loaded');
      await page.screenshot({ path: `capturas/${nome}/shell-filial-recusada.png`, clip: { x: 0, y: 0, width: largura, height: 90 } });

      expect(await sobraLateral(page), `a barra com o aviso passa da janela de ${largura}px`).toBeLessThanOrEqual(0);
      const barra = (await page.locator('.topbar').boundingBox())!;
      const caixa = (await page.locator('.topbar .cad-filial').boundingBox())!;
      expect(Math.round(caixa.x + caixa.width), 'a caixa da filial sai pela direita da janela').toBeLessThanOrEqual(largura);
      // E NÃO VAZA PARA BAIXO DA BARRA: o aviso desce para uma segunda linha
      // dentro da caixa, e a caixa continua dentro dos 56px.
      expect(Math.round(caixa.y), 'a caixa da filial passa do topo da barra').toBeGreaterThanOrEqual(Math.round(barra.y));
      expect(Math.round(caixa.y + caixa.height), 'a caixa da filial passa da barra por baixo').toBeLessThanOrEqual(
        Math.round(barra.y + barra.height),
      );
    });

    test('clicar no mapa leva à aba Território, e ela para abaixo da barra do topo — não embaixo dela', async ({ page }) => {
      // O DEFEITO (revisão de 24/09/2026): o clique no mapa rola até a aba com
      // `scrollIntoView({ block: 'start' })`, e a barra do topo é fixa — sem o
      // `scroll-margin-top` a aba parava ATRÁS dela, e a pessoa via a página
      // rolar sem ver a aba que abriu. Só o shell tem a barra; o harness de
      // Mercado não a desenha, e por isso a prova mora aqui.
      await abrir(page, '/relatorios/territorio');
      const mapa = page.locator('[data-mapa="cobertura"]');
      await expect(mapa).toBeVisible({ timeout: 30_000 });
      // A ADR é desenhada por último (por cima dos vizinhos): o último polígono é dela.
      const municipio = mapa.locator('path.terr-poligono').last();
      await municipio.scrollIntoViewIfNeeded();
      await municipio.dispatchEvent('click');

      const aba = page.getByRole('tab', { name: 'Território' });
      await expect(aba).toHaveAttribute('aria-selected', 'true');
      await expect(aba).toBeFocused();
      const barra = (await page.locator('.topbar').boundingBox())!;
      const posicao = (await aba.boundingBox())!;
      expect(
        Math.round(posicao.y),
        `a aba parou em ${Math.round(posicao.y)}px, atrás da barra do topo (que vai até ${Math.round(barra.y + barra.height)}px)`,
      ).toBeGreaterThanOrEqual(Math.round(barra.y + barra.height));
    });

    test('"Indicadores Geográficos" cabe no item do menu sem cortar', async ({ page }) => {
      await abrir(page, '/relatorios/territorio');
      if (gaveta) await page.getByRole('button', { name: 'Abrir menu' }).click();

      const rotulo = page.locator('.nav-item.active .nav-item-rotulo');
      await expect(rotulo).toHaveText('Indicadores Geográficos');
      const cortado = await rotulo.evaluate((n) => n.scrollWidth > n.clientWidth);
      expect(cortado, 'o rótulo do item ativo está sendo cortado com reticências').toBe(false);
    });

    test('o Sair se alcança pelo teclado', async ({ page }) => {
      await abrir(page, '/relatorios/territorio');
      if (gaveta) await page.getByRole('button', { name: 'Abrir menu' }).click();

      const usuario = page.getByRole('button', { name: /Amostra Fictícia/ });
      await usuario.focus();
      await page.keyboard.press('Enter');
      await expect(usuario).toHaveAttribute('aria-expanded', 'true');

      await page.keyboard.press('Tab');
      const sair = page.getByRole('link', { name: 'Sair' });
      await expect(sair).toBeFocused();
      await expect(sair).toHaveAttribute('href', '/auth/sair');
      await page.screenshot({ path: `capturas/${nome}/shell-menu-do-usuario.png` });

      await page.keyboard.press('Escape');
      await expect(sair).toHaveCount(0);
      await expect(usuario).toBeFocused();
    });

    test(gaveta ? 'o menu é uma gaveta: abre no ☰, recebe o foco e fecha no Esc' : 'o menu fica à vista e não há ☰', async ({ page }) => {
      await abrir(page, '/relatorios/territorio');
      const menu = page.getByRole('navigation', { name: 'Menu principal' });
      const botao = page.getByRole('button', { name: 'Abrir menu' });

      if (!gaveta) {
        await expect(menu).toBeVisible();
        await expect(botao).toBeHidden();
        return;
      }

      await expect(menu).toBeHidden();
      await botao.click();
      await expect(menu).toBeVisible();
      await expect(page.locator('.nav-item.active')).toBeFocused();
      await page.screenshot({ path: `capturas/${nome}/shell-gaveta-aberta.png` });
      expect(await sobraLateral(page), 'a gaveta aberta empurrou a página para o lado').toBeLessThanOrEqual(0);

      await page.keyboard.press('Escape');
      await expect(menu).toBeHidden();
      await expect(page.getByRole('button', { name: 'Abrir menu' })).toBeFocused();
    });
  });
}
