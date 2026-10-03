/**
 * A ARQUITETURA DO FRONT, VIGIADA (documento 54 §3.5) — as fronteiras que o código já segue, escritas para não se perderem:
 * o que é de desenvolvimento não entra no código de produção, componente não depende de tela, e tela não depende de outra
 * tela. As exceções conhecidas estão listadas pelo nome e só podem diminuir.
 */

import { describe, expect, it } from 'vitest';

const FONTES = import.meta.glob<string>('/src/**/*.{ts,tsx}', { query: '?raw', import: 'default', eager: true });
const PRODUCAO = Object.entries(FONTES).filter(([arquivo]) => !arquivo.includes('.teste.'));

/** Os módulos que um arquivo importa (estático ou `import()`), resolvidos para o caminho em `/src`, sem extensão. */
function importados(arquivo: string, codigo: string): string[] {
  return [...codigo.matchAll(/(?:from|import\()\s*'(\.[^']+)'/g)].map(([, alvo]) => {
    const partes = arquivo.split('/').slice(0, -1);
    for (const parte of alvo.split('/')) {
      if (parte === '..') partes.pop();
      else if (parte !== '.') partes.push(parte);
    }
    return partes.join('/');
  });
}

const ehTela = (modulo: string) => modulo.startsWith('/src/telas/') && `${modulo}.tsx` in FONTES;

/** As exceções de hoje (03/10/2026), e só podem diminuir. */
const COMPONENTE_QUE_IMPORTA_TELA: string[] = [];
const TELA_QUE_IMPORTA_TELA: string[] = [];

describe('arquitetura do front', () => {
  it('a varredura enxerga os imports, inclusive o import() das telas baixadas sob demanda', () => {
    // SEM ESTA CONFERÊNCIA, um glob ou uma expressão quebrada deixaria as três regras abaixo verdes sem olhar nada.
    const doRoteador = importados('/src/rotas.tsx', FONTES['/src/rotas.tsx']).filter(ehTela);
    expect(doRoteador).toContain('/src/telas/Funil');
    expect(doRoteador).toContain('/src/telas/cadastro/ClientesLista');
  });

  it('o código de produção não importa nada de src/dev (só o App, pelo ternário do DEV)', () => {
    const violacoes = PRODUCAO.filter(([arquivo]) => !arquivo.startsWith('/src/dev/') && arquivo !== '/src/App.tsx').flatMap(
      ([arquivo, codigo]) => importados(arquivo, codigo).filter((m) => m.startsWith('/src/dev/')).map((m) => `${arquivo} → ${m}`),
    );
    expect(violacoes).toEqual([]);
  });

  it('componente não importa tela', () => {
    const violacoes = PRODUCAO.filter(([arquivo]) => arquivo.startsWith('/src/componentes/')).flatMap(([arquivo, codigo]) =>
      importados(arquivo, codigo).filter(ehTela).map((m) => `${arquivo} → ${m}`),
    );
    expect(violacoes.sort()).toEqual(COMPONENTE_QUE_IMPORTA_TELA);
  });

  it('tela não importa outra tela', () => {
    const violacoes = PRODUCAO.filter(([arquivo]) => arquivo.startsWith('/src/telas/') && arquivo.endsWith('.tsx')).flatMap(
      ([arquivo, codigo]) =>
        importados(arquivo, codigo)
          .filter((m) => ehTela(m) && `${m}.tsx` !== arquivo)
          .map((m) => `${arquivo} → ${m}`),
    );
    expect(violacoes.sort()).toEqual(TELA_QUE_IMPORTA_TELA);
  });
});
