/**
 * As telas do protótipo com dado fictício só existem no desenvolvimento (issue 191).
 *
 * O `rotas.tsx` decide pelo `import.meta.env.DEV` na hora em que o módulo carrega. Para ver o pacote de produção, o
 * teste troca a variável e carrega o módulo de novo.
 */

import { afterEach, describe, expect, it, vi } from 'vitest';

const FIXAS_DO_PROTOTIPO = [
  '/inicio-antigo',
  '/clientes/84391',
  '/equipamentos/1RW7250PVMR123456',
  '/oportunidades/nova',
];

async function rotasCom(desenvolvimento: boolean) {
  vi.stubEnv('DEV', desenvolvimento);
  vi.resetModules();
  return import('./rotas');
}

afterEach(() => {
  vi.unstubAllEnvs();
  vi.resetModules();
});

describe('as rotas no pacote de produção', () => {
  it('não declaram nenhuma das quatro telas do protótipo', async () => {
    const { ROTAS } = await rotasCom(false);
    const caminhos = ROTAS.map((r) => r.caminho);

    for (const fixa of FIXAS_DO_PROTOTIPO) expect(caminhos).not.toContain(fixa);
  });

  it('mandam o endereço antigo para a tela real, que lê a API', async () => {
    const { acharRota } = await rotasCom(false);

    expect(acharRota('/clientes/84391').caminho).toBe('/clientes/:chave');
    expect(acharRota('/equipamentos/1RW7250PVMR123456').caminho).toBe('/equipamentos/:chave');
    expect(acharRota('/oportunidades/nova').caminho).toBe('/oportunidades/:chave');
    expect(acharRota('/inicio-antigo').caminho).toBe('/');
  });

  it('só levam telas que leem a API, além de Configurações', async () => {
    const { ROTAS } = await rotasCom(false);
    const semApi = ROTAS.filter((r) => !r.usaApi).map((r) => r.caminho);

    // Configurações lê a API por seção, cada uma com a sua permissão; a rota em si não tem seletor de filial.
    expect(semApi).toEqual(['/config']);
  });
});

describe('as rotas no desenvolvimento', () => {
  it('mantêm as quatro telas do protótipo para a comparação visual', async () => {
    const { ROTAS } = await rotasCom(true);
    const caminhos = ROTAS.map((r) => r.caminho);

    for (const fixa of FIXAS_DO_PROTOTIPO) expect(caminhos).toContain(fixa);
  });

  it('declaram a rota fixa antes da rota com parâmetro, senão a fixa nunca casaria', async () => {
    const { ROTAS, acharRota } = await rotasCom(true);
    const posicao = (caminho: string) => ROTAS.findIndex((r) => r.caminho === caminho);

    expect(posicao('/clientes/84391')).toBeLessThan(posicao('/clientes/:chave'));
    expect(posicao('/equipamentos/1RW7250PVMR123456')).toBeLessThan(posicao('/equipamentos/:chave'));
    expect(posicao('/oportunidades/nova')).toBeLessThan(posicao('/oportunidades/:chave'));
    expect(acharRota('/oportunidades/nova').caminho).toBe('/oportunidades/nova');
  });
});

/**
 * NENHUMA TELA PUBLICADA LÊ `public/dados`. Quem lê a pasta é o `carregar()`, pelo `useDados()`. Este teste varre o
 * código e só aceita esses dois nas telas do protótipo, que o `rotas.tsx` deixa fora do pacote.
 */
describe('quem lê o JSON do protótipo', () => {
  const PODEM_LER = [
    '/src/dados/carregar.ts',
    '/src/dados/useDados.ts',
    '/src/telas/ClienteFicha.tsx',
    '/src/telas/EquipamentoFicha.tsx',
    '/src/telas/NovaOportunidade.tsx',
  ];

  const fontes = import.meta.glob<string>('/src/**/*.{ts,tsx}', { query: '?raw', import: 'default', eager: true });

  it('são só as telas do protótipo', () => {
    const leitores = Object.entries(fontes)
      .filter(([arquivo]) => !arquivo.includes('.teste.'))
      .filter(([, codigo]) => /from '[./]+\/?dados\/(useDados|carregar)'|from '\.\/(useDados|carregar)'/.test(codigo))
      .map(([arquivo]) => arquivo);

    expect(leitores.filter((l) => !PODEM_LER.includes(l))).toEqual([]);
  });

  it('e as telas do protótipo só entram pelo rotas.tsx, que as deixa fora do pacote', () => {
    const telas = ['ClienteFicha', 'EquipamentoFicha', 'NovaOportunidade', 'Inicio'];
    const importadores = Object.entries(fontes)
      .filter(([arquivo]) => !arquivo.includes('.teste.'))
      .filter(([, codigo]) => telas.some((t) => new RegExp(`from '[./]+/?(telas/)?${t}'`).test(codigo)))
      .map(([arquivo]) => arquivo);

    expect(importadores).toEqual(['/src/rotas.tsx']);
  });
});
