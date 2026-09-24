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

  if (estado === 'municipioSelecionado' || estado === 'fichaAberta') {
    // COM MUNICÍPIO NA URL A TELA ABRE EM TERRITÓRIO (fidelidade às maquetes,
    // 23/09/2026): a ficha mora só lá, e um endereço com município é o de quem
    // estava olhando uma ficha. Não há mapa a esperar — a ficha é a última
    // coisa a desenhar.
    await expect(pagina.locator('[data-bloco="ficha-do-municipio"]')).toBeVisible({ timeout: 30_000 });
  } else if (estado !== 'carregando' && estado !== 'erro') {
    // Os mapas são a última coisa a desenhar, e é depois deles que a altura para
    // de mudar. Capturar antes pegaria a página no meio do caminho.
    await expect(pagina.locator('[data-bloco="mapas"]')).toBeVisible({ timeout: 30_000 });
  }

  // O harness abre os `<details>` da ficha 300 ms depois de montar.
  if (estado === 'fichaAberta') await pagina.waitForTimeout(600);

  await pagina.waitForFunction(() => document.fonts.status === 'loaded');
}

/**
 * O quanto a página passa da largura da janela. Zero é o único valor aceito.
 *
 * A LISTA DE CULPADOS IGNORA DUAS COISAS, e as duas custaram tempo para
 * descobrir:
 *
 * 1. **O que está dentro de um `<svg>`.** Os mapas desenham os municípios
 *    vizinhos e deixam o SVG recortar o que passa da borda — então todo `path`
 *    tem caixa geométrica maior que o desenho. Eles apareciam no topo da lista
 *    em toda falha e não empurram a página um pixel.
 * 2. **O que está dentro de um contêiner que rola.** Uma tabela larga dentro de
 *    um `overflow-x: auto` é o CONSERTO, não o defeito.
 *
 * Sem esses dois filtros a mensagem apontava para o lugar errado, que é pior que
 * não apontar para lugar nenhum.
 */
async function sobraLateral(pagina: Page) {
  return pagina.evaluate(() => {
    const raiz = document.documentElement;

    const dentroDeAlgoQueRola = (e: HTMLElement) => {
      for (let p = e.parentElement; p && p !== document.body; p = p.parentElement) {
        const estilo = getComputedStyle(p);
        if (estilo.overflowX === 'auto' || estilo.overflowX === 'scroll' || estilo.overflowX === 'hidden') return true;
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

/**
 * A LARGURA DO CONTEÚDO — a régua das quebras desta tela (fidelidade às
 * maquetes, 23/09/2026).
 *
 * As quebras deixaram de ser pela largura da JANELA e passaram a ser pela da
 * página (`@container indicadores`, no `dashboard.css`): a janela inclui o menu
 * lateral e as folgas, e as regras achavam que tinham um espaço que o conteúdo
 * não tinha. As afirmações de arranjo medem a mesma régua que o CSS usa — medir
 * a janela aqui seria testar uma regra que não existe mais.
 */
async function larguraDoConteudo(pagina: Page): Promise<number> {
  return pagina.locator('.dash-pagina').evaluate((n) => {
    const estilo = getComputedStyle(n);
    return n.clientWidth - parseFloat(estilo.paddingLeft) - parseFloat(estilo.paddingRight);
  });
}

/** Quantas linhas visuais um conjunto de elementos ocupa, pelo topo de cada um. */
async function linhasDe(pagina: Page, seletor: string): Promise<number> {
  const topos = await pagina
    .locator(seletor)
    .evaluateAll((nós) => nós.map((n) => Math.round(n.getBoundingClientRect().top)));
  return new Set(topos).size;
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

    /**
     * AS AFIRMAÇÕES DE ARRANJO (item 18 da T4.6).
     *
     * "Cabe na largura" é necessário e não é suficiente: uma página pode caber e
     * ainda estar errada — quatro KPIs empilhados num monitor de 1920, ou
     * comprimidos em quatro colunas num celular. Estas medem o ARRANJO, que é o
     * que a fase entregou, e não só a ausência de rolagem lateral.
     *
     * Elas leem a posição real na tela (`getBoundingClientRect`), e não a regra
     * de CSS: o que importa é onde o elemento ficou, não o que a folha de estilo
     * pretendia.
     */
    test('os quatro números de decisão ficam numa linha no desktop e empilham no celular', async ({ page }) => {
      await abrir(page, 'completo');

      const cartoes = page.locator('[data-bloco="kpis-executivos"] [data-kpi]');
      await expect(cartoes).toHaveCount(4);

      const linhas = await linhasDe(page, '[data-bloco="kpis-executivos"] [data-kpi]');
      const conteudo = await larguraDoConteudo(page);

      // PELA LARGURA DO CONTEÚDO, e não da janela (decisão do usuário de
      // 23/09/2026): quatro numa linha a partir de 900px, 2 × 2 a partir de 520,
      // um por linha abaixo disso. Numa janela de 1024 o conteúdo tem 960px, e
      // os quatro cabem numa linha — como na maquete.
      if (conteudo >= 900) {
        expect(linhas, `com ${conteudo}px de conteúdo os quatro KPIs deveriam estar numa linha só, e estão em ${linhas}`).toBe(1);
      } else if (conteudo >= 520) {
        expect(linhas, `com ${conteudo}px de conteúdo esperava 2×2`).toBe(2);
      } else {
        expect(linhas, `com ${conteudo}px de conteúdo cada KPI deveria ter a própria linha`).toBe(4);
      }
    });

    test('os quatro mapas seguem o arranjo declarado para esta largura', async ({ page }) => {
      await abrir(page, 'completo');

      const cartoes = page.locator('[data-bloco="mapas"] [data-mapa]');
      await expect(cartoes).toHaveCount(4);

      const linhas = await linhasDe(page, '[data-bloco="mapas"] [data-mapa]');
      const conteudo = await larguraDoConteudo(page);

      // PELA LARGURA DO CONTEÚDO (decisão do usuário de 23/09/2026): quatro
      // colunas a partir de 1100px, 2 × 2 de 600 a 1099, um por linha abaixo
      // de 600.
      //
      // ERA 2100px DE JANELA PARA OS QUATRO LADO A LADO, e 851 para o 2 × 2. Com
      // o menu lateral, nenhum monitor do comercial chegava aos quatro numa
      // linha — que é o que a maquete mostra numa coluna de ~1440px — e os mapas
      // ocupavam duas dobras. A régua agora é a página, não a janela.
      const esperado = conteudo >= 1100 ? 1 : conteudo >= 600 ? 2 : 4;
      expect(linhas, `com ${conteudo}px de conteúdo esperava ${esperado} linha(s) de mapa, e deu ${linhas}`).toBe(esperado);
    });

    test('a linha final tem Potencial e Performance lado a lado a partir de 1000px de conteúdo', async ({ page }) => {
      // O MOMENTO DO MERCADO SAIU DESTA LINHA e foi para largura inteira, logo
      // abaixo dos quatro números (decisão do usuário de 23/09/2026): num terço
      // de linha, o Crédito crescia para baixo o que não tinha de largura.
      await abrir(page, 'completo');

      const conteudo = await larguraDoConteudo(page);
      const linhas = await linhasDe(page, '.dash-linha-final > *');
      expect(linhas, `com ${conteudo}px de conteúdo`).toBe(conteudo >= 1000 ? 1 : 2);

      // E o Momento ocupa a largura inteira da página, em qualquer largura.
      const momento = await page.locator('[data-bloco="momento-do-mercado"]').boundingBox();
      expect(Math.round(momento!.width), 'o Momento do mercado não está em largura inteira').toBe(Math.round(conteudo));
    });

    test('os filtros secundários não ocupam a primeira dobra', async ({ page }) => {
      // O DEFEITO QUE ISTO IMPEDE DE VOLTAR: treze filtros em duas fileiras, cada
      // um com o motivo escrito embaixo, empurravam os números para fora da
      // primeira tela. Quem abria a página para saber o tamanho do mercado lia
      // primeiro um parágrafo sobre o que a tela NÃO filtra.
      await abrir(page, 'completo');

      // O popover está fechado: nenhum filtro secundário na tela, em largura nenhuma.
      await expect(page.locator('.dash-popover')).toHaveCount(0);

      // E OS FILTROS NÃO PODEM VALER MAIS QUE UM TERÇO DA PRIMEIRA DOBRA. Esta é
      // a afirmação que mede o pedido, e não "o KPI cabe na primeira dobra": num
      // celular de 390×844 tudo empilha, e exigir o número acima da dobra ali
      // seria exigir que o cabeçalho da página sumisse.
      const filtros = page.locator('[data-bloco="filtros"]');
      const caixa = await filtros.boundingBox();
      expect(
        caixa!.height,
        `os filtros ocupam ${Math.round(caixa!.height)}px de uma dobra de ${altura}px`,
      ).toBeLessThan(altura / 3);
    });

    test('Mais filtros abre, mostra os secundários e é capturado', async ({ page }) => {
      await abrir(page, 'completo');

      await page.locator('[data-bloco="mais-filtros"]').click();
      const popover = page.locator('.dash-popover');
      await expect(popover).toBeVisible();

      // NADA FOI REMOVIDO: os filtros sem dado continuam lá, desligados.
      for (const rotulo of ['Tipo de cliente', 'Tipo de produto', 'Modelo', 'CEN / gestor'])
        await expect(popover.getByText(rotulo, { exact: true })).toBeVisible();

      await page.screenshot({ path: `capturas/${nome}/mais-filtros-aberto.png` });

      const { sobra } = await sobraLateral(page);
      expect(sobra, `o popover de filtros passa ${sobra}px da janela`).toBeLessThanOrEqual(0);
    });

    test('Território cabe na largura e é capturado', async ({ page }) => {
      await abrir(page, 'completo');
      await page.getByRole('tab', { name: 'Território' }).click();
      await expect(page.locator('[role="tabpanel"]')).toBeVisible();
      await page.screenshot({ path: `capturas/${nome}/territorio.png`, fullPage: true });

      const { sobra, culpados } = await sobraLateral(page);
      expect(sobra, `Território passa ${sobra}px. Culpados: ${culpados.join(' · ')}`).toBeLessThanOrEqual(0);

      // QUANTAS COLUNAS FICAM, por largura: NOVE no desktop, cinco no tablet,
      // duas no aparelho de mão.
      //
      // A nona é a COLUNA DE AÇÃO da maquete (T4.9) — o chevron que abre a ficha
      // na ponta da linha. Ela sai já na tela de mão: 26px de botão ali são
      // 26px roubados das colunas que respondem "onde vender", e o nome do
      // município continua sendo a porta para a mesma ficha.
      //
      // OS DEGRAUS SÃO PELA LARGURA DO CONTEÚDO (23/09/2026): cinco colunas
      // abaixo de 760px, duas abaixo de 520px — eram 768 e 560 de janela.
      const conteudo = await larguraDoConteudo(page);
      const esperadas = conteudo < 520 ? 2 : conteudo < 760 ? 5 : 9;
      const colunas = await page.locator('[data-bloco="tabela-municipios"] thead th:visible').count();
      expect(colunas, `em ${largura}px esperava ${esperadas} colunas`).toBe(esperadas);

      // ============================================================================
      // E QUE ELAS CABEM — que é o que o teste anterior NÃO media.
      //
      // `:visible` em Playwright quer dizer "não está `display:none`", e não "cabe
      // na tela". Com cinco colunas em 390px o teste passava enquanto a tabela
      // virava uma lista de nomes com as outras quatro escondidas atrás da rolagem
      // horizontal do cartão — dado presente e invisível, que é pior que dado
      // adiado. Só a revisão humana das capturas pegou isso (T4.7).
      //
      // Aqui a afirmação é geométrica: a tabela não é mais larga que o cartão que
      // a segura. No desktop a rolagem interna é aceitável para oito colunas, e
      // por isso a exigência vale onde ela engana — no celular.
      // ============================================================================
      if (largura <= 560) {
        const sobra = await page.locator('[data-bloco="tabela-municipios"] .cad-tabela-wrap').evaluate((n) => {
          const tabela = n.querySelector('table')!;
          return Math.round(tabela.getBoundingClientRect().width - n.clientWidth);
        });
        expect(sobra, `a tabela passa ${sobra}px do cartão em ${largura}px — as colunas escondidas rolam para o lado`).toBeLessThanOrEqual(0);
      }
    });

    test('a ficha do município cabe na largura, com as evidências abertas', async ({ page }) => {
      await abrir(page, 'fichaAberta');

      const ficha = page.locator('[data-bloco="ficha-do-municipio"]');
      await expect(ficha).toBeVisible();

      // A CAPTURA É DO ELEMENTO, e não da janela (T4.7): com `page.screenshot`
      // depois de rolar até ela, a imagem pegava o MEIO da camada de evidência —
      // a ficha inteira, que é o que se revisa, não aparecia em lugar nenhum.
      await ficha.screenshot({ path: `capturas/${nome}/ficha.png` });

      const caixa = await ficha.boundingBox();
      expect(
        Math.round(caixa!.x + caixa!.width),
        `a ficha termina em ${Math.round(caixa!.x + caixa!.width)}px, fora da janela de ${largura}px`,
      ).toBeLessThanOrEqual(largura);

      // TABELA E FICHA LADO A LADO a partir de 1150px de conteúdo, e a ficha
      // embaixo abaixo disso (fidelidade às maquetes, 23/09/2026).
      const conteudo = await larguraDoConteudo(page);
      const linhas = await linhasDe(page, '.terr-territorio > *');
      expect(linhas, `com ${conteudo}px de conteúdo`).toBe(conteudo >= 1150 ? 1 : 2);
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
