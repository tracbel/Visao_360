/**
 * A CONFERÊNCIA VISUAL (fase T4.5).
 *
 * O QUE ELA PROVA: que a tela de Mercado cabe na largura em que é usada. A regra
 * é uma só e é objetiva — `document.documentElement.scrollWidth` não pode passar
 * da largura da janela. Rolagem lateral numa tela de indicadores esconde coluna,
 * e ninguém rola para o lado procurando número que não sabe que existe.
 *
 * ELA RODA CONTRA O SERVIDOR DE DESENVOLVIMENTO, e não contra o `dist`: o
 * harness visual só existe em modo de desenvolvimento, de propósito. É por isso
 * que `webServer` chama `npm run dev`.
 *
 * SÓ CHROMIUM. Três navegadores triplicariam o tempo para provar a mesma regra
 * de largura; o parque da Tracbel é Chromium, e o dia em que houver diferença de
 * motor para investigar, acrescenta-se o projeto correspondente aqui.
 */

import { defineConfig, devices } from '@playwright/test';

/** As larguras que a diretoria e o comercial usam de verdade. */
export const LARGURAS = [
  { nome: '1920x1080', largura: 1920, altura: 1080 },
  { nome: '1440x900', largura: 1440, altura: 900 },
  { nome: '1280x800', largura: 1280, altura: 800 },
  { nome: '1024x768', largura: 1024, altura: 768 },
  { nome: '768x1024', largura: 768, altura: 1024 },
  { nome: '390x844', largura: 390, altura: 844 },
] as const;

/** A mesma porta que o `vite.config.ts` fixa para `npm run dev`. */
const PORTA = 5199;

export default defineConfig({
  testDir: './testes-visuais',
  // Uma tela de cada vez: as capturas são sequenciais e comparáveis, e o
  // servidor de desenvolvimento é um só.
  workers: 1,
  fullyParallel: false,
  // NADA DE `retries`: um teste de layout que passa na segunda tentativa está
  // escondendo uma corrida, não provando uma largura.
  retries: 0,
  reporter: [['list'], ['html', { outputFolder: 'capturas/relatorio', open: 'never' }]],
  timeout: 60_000,
  use: {
    baseURL: `http://localhost:${PORTA}`,
    ...devices['Desktop Chrome'],
    // O tema claro é o aprovado; deixar o do sistema decidir faria a captura
    // mudar conforme a máquina de quem roda.
    colorScheme: 'light',
    locale: 'pt-BR',
    timezoneId: 'America/Sao_Paulo',
  },
  webServer: {
    command: 'npm run dev',
    url: `http://localhost:${PORTA}`,
    reuseExistingServer: !process.env.CI,
    timeout: 120_000,
  },
});
