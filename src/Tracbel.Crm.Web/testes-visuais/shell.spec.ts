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
