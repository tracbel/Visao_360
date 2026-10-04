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
const FILTRO_DO_PADRAO_ESCRITO_A_MAO: string[] = [];
/** O ranking das culturas do Momento desempata o vazio pelo nome da cultura — é outra regra, e não cópia. */
const ORDENACAO_COM_REGRA_PROPRIA = ['/src/componentes/mercado/momento/contas.ts'];

/**
 * AS CÓPIAS DE FORMATO QUE JÁ EXISTIRAM, cada uma idêntica em duas ou mais telas até 03/10/2026. O `reaisCurtos` do
 * Momento do Mercado fica: ele tem regra própria (a faixa "bi" e a moeda com casas), e não é cópia.
 */
const COPIAS_DE_FORMATO: { oQue: string; padrao: RegExp; excecoes?: string[] }[] = [
  { oQue: 'moeda compacta', padrao: /function (fmtBRLcompact|formatarBRLCompacto)\(/ },
  { oQue: 'moeda, número ou data de ficha (fmtBRL, fmtNum, fmtDataHora)', padrao: /function (fmtBRL|fmtNum|fmtDataHora)\(/ },
  { oQue: 'reais curtos', padrao: /function reaisCurtos\(/, excecoes: ['/src/componentes/mercado/momento/formatos.ts'] },
  { oQue: 'mês da competência (mesCurto, mesAno, mesPorExtenso)', padrao: /(?:function|const) (mesCurto|mesCurtoDe|mesAno|mesPorExtenso)\b/ },
  { oQue: 'dia e hora do instante da API', padrao: /function diaEHora\(/ },
  { oQue: 'percentual da parte', padrao: /function percentual\(parte: number, todo: number\)/ },
  {
    oQue: 'número (com ou sem casas)',
    padrao: /const [\wº]+ = \(v: number(?:, casas = \d)?\) => v\.toLocaleString\('pt-BR'(?:, \{ maximumFractionDigits: casas \})?\);/,
  },
  { oQue: 'a lista dos meses curtos', padrao: /['"]jan['"],\s*['"]fev['"],\s*['"]mar['"]/ },
  { oQue: 'a lista dos dias da semana', padrao: /['"]dom['"],\s*['"]seg['"],\s*['"]ter['"]/ },
];

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

  it('os formatos de número, moeda e data moram em dados/formatadores e em nenhum outro arquivo (documento 54 §3.5)', () => {
    // AS AMOSTRAS DE src/dev ficam de fora: a lista dos meses nelas é dado de exemplo, e não formato.
    const fora = (arquivo: string) => arquivo === '/src/dados/formatadores.ts' || arquivo.startsWith('/src/dev/');
    const copias = PRODUCAO.filter(([arquivo]) => !fora(arquivo)).flatMap(([arquivo, codigo]) =>
      COPIAS_DE_FORMATO.filter((c) => c.padrao.test(codigo) && !c.excecoes?.includes(arquivo)).map((c) => `${arquivo}: ${c.oQue}`),
    );
    expect(copias).toEqual([]);
  });

  it('a ordenação das tabelas, com o vazio no fim, mora em componentes/comum/ordenacao (documento 54 §3.5)', () => {
    const copias = PRODUCAO.filter(([arquivo]) => arquivo !== '/src/componentes/comum/ordenacao.ts')
      .filter(([, codigo]) => /if \(va === null\) return 1;/.test(codigo))
      .map(([arquivo]) => arquivo);
    expect(copias.sort()).toEqual(ORDENACAO_COM_REGRA_PROPRIA);
  });

  it('o filtro do padrão mora em componentes/comum/CampoDoFiltro (documento 54 §3.5)', () => {
    const copias = PRODUCAO.filter(([arquivo]) => arquivo !== '/src/componentes/comum/CampoDoFiltro.tsx')
      .filter(([, codigo]) => codigo.includes('dash-filtro-icone'))
      .map(([arquivo]) => arquivo);
    expect(copias.sort()).toEqual(FILTRO_DO_PADRAO_ESCRITO_A_MAO);
  });
});
