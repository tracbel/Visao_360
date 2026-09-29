/**
 * AS FICHAS DO CADASTRO E AS CONFIGURAÇÕES NO PADRÃO DOS INDICADORES GEOGRÁFICOS (29/09/2026, #293 bloco 5b) — CABEM
 * NA LARGURA E TÊM A FORMA DO PADRÃO.
 *
 * Roda sobre o harness da Visão 360 aberto na rota de cada tela, com as amostras de `amostrasDoCadastro.ts`: a ficha
 * de um cliente e a de uma máquina, os dois formulários de cadastro novo e as Configurações. Nenhum número daqui é dado
 * da Tracbel.
 *
 * O QUE ELE PROVA:
 *
 * - nenhuma largura ganha rolagem lateral, da de 390 à de 2.400 — as tabelas da ficha podem rolar dentro do painel, a
 *   página não;
 * - a página ocupa a coluna de conteúdo inteira até 2.100px de janela;
 * - a ficha tem as peças do padrão: cabeçalho, seções com título e o formulário num painel, com a grade dos campos em
 *   mais de uma coluna quando cabe.
 */

import { expect, test, type Page } from '@playwright/test';
import { LARGURAS } from '../playwright.config';

const TODAS = [
  ...LARGURAS,
  { nome: '2400x1200', largura: 2400, altura: 1200 },
  { nome: '1536x864', largura: 1536, altura: 864 },
] as const;

/** `secoes`: quantos títulos de seção a tela tem; as Configurações não têm seção — têm o menu e o conteúdo. */
const TELAS = [
  // O cliente 004 tem frota pela sincronia e compras no ART; a máquina 004 tem duas vendas e divergências abertas.
  { nome: 'ficha-cliente', rota: '/clientes/cliente-ficticio-004', secoes: 3 },
  { nome: 'ficha-equipamento', rota: '/equipamentos/maquina-ficticia-004', secoes: 3 },
  { nome: 'novo-cliente', rota: '/clientes/novo', secoes: 1 },
  { nome: 'novo-equipamento', rota: '/equipamentos/novo', secoes: 1 },
  { nome: 'configuracoes', rota: '/config', secoes: 0 },
] as const;

async function abrir(pagina: Page, rota: string) {
  await pagina.goto(`/#/dev/visao360-visual?estado=completo&rota=${encodeURIComponent(rota)}`);
  await expect(pagina.locator('[data-harness="visao360-visual"]')).toBeAttached();
  await expect(pagina.locator('.dash-pagina .page-header')).toBeVisible({ timeout: 30_000 });
  // A LEITURA VOLTOU quando nenhum "Carregando…" sobrou na página.
  await expect(pagina.locator('.cad-estado[role="status"]')).toHaveCount(0, { timeout: 30_000 });
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
  test.describe(`Fichas e Configurações no padrão dos Indicadores em ${nome}`, () => {
    test.use({ viewport: { width: largura, height: altura } });

    for (const tela of TELAS) {
      test(`${tela.nome}: cabe na largura, na coluna inteira e com as peças do padrão`, async ({ page }) => {
        await abrir(page, tela.rota);
        await page.screenshot({ path: `capturas/${nome}/padrao-${tela.nome}.png`, fullPage: true });

        const { sobra, culpados } = await sobraLateral(page);
        expect(
          sobra,
          `A página passa ${sobra}px da janela de ${largura}px. Primeiros culpados: ${culpados.join(' · ') || 'nenhum'}`,
        ).toBeLessThanOrEqual(0);

        await expect(page.locator('.dash-pagina.dash-pagina-larga')).toHaveCount(1);
        await expect(page.locator('section.dash-secao .terr-secao-titulo')).toHaveCount(tela.secoes);

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

        // A GRADE DOS CAMPOS CONTINUA MEDINDO A PRÓPRIA LARGURA: sem o `card-body`, quem abre o container é o
        // `.ficha-campos` — e a grade tem mais de uma coluna quando o painel passa de 560px.
        if (tela.nome !== 'configuracoes') {
          const grade = page.locator('[data-bloco="identificacao"] .cad-grade');
          const colunas = await grade.evaluate((g) => getComputedStyle(g).gridTemplateColumns.split(' ').length);
          const larguraDoPainel = await page.locator('[data-bloco="identificacao"] .ficha-campos').evaluate((e) => e.clientWidth);
          if (larguraDoPainel >= 560) expect(colunas, `a grade tem ${colunas} coluna(s) num painel de ${larguraDoPainel}px`).toBeGreaterThan(1);
        }
      });
    }
  });
}
