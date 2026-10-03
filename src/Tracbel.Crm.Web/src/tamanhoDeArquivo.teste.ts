/**
 * O TAMANHO DE ARQUIVO NO FRONT (documento 54 §3.5): arquivo novo nasce com até 600 linhas; os grandes de hoje estão
 * listados com o tamanho de 03/10/2026 e só podem diminuir. Os testes ficam de fora — eles crescem com os casos.
 */

import { describe, expect, it } from 'vitest';

const LIMITE = 600;
const FONTES = import.meta.glob<string>('/src/**/*.{ts,tsx}', { query: '?raw', import: 'default', eager: true });

/** Os grandes de hoje (03/10/2026). */
const CONHECIDOS: Record<string, number> = {
  '/src/componentes/mercado/ComposicaoDoFator.tsx': 734,
  '/src/componentes/mercado/PainelDeRentabilidade.tsx': 786,
  '/src/componentes/painel360/PainelExecutivo.tsx': 1713,
  '/src/componentes/territorio/DetalheDoMunicipio.tsx': 680,
  '/src/componentes/territorio/FiltrosDosIndicadores.tsx': 629,
  '/src/componentes/territorio/PainelDeCredito.tsx': 655,
  '/src/dados/api/consolidado.ts': 885,
  '/src/dev/amostras.ts': 767,
  '/src/dev/amostrasDaVisao360.ts': 1488,
  '/src/dev/amostrasDeMercado.ts': 1219,
  '/src/telas/Agenda.tsx': 673,
  '/src/telas/cadastro/ClienteCadastro.tsx': 626,
  '/src/telas/cadastro/EquipamentoCadastro.tsx': 910,
  '/src/telas/cadastro/EquipamentosLista.tsx': 718,
  '/src/telas/ClienteFicha.tsx': 782,
  '/src/telas/CoberturaCarteira.tsx': 1089,
  '/src/telas/CoberturaRegional.tsx': 972,
  '/src/telas/EquipamentoFicha.tsx': 707,
  '/src/telas/ForecastGerencia.tsx': 1029,
  '/src/telas/Funil.tsx': 663,
  '/src/telas/NovaOportunidade.tsx': 609,
  '/src/telas/PerformanceCen.tsx': 1598,
  '/src/telas/Pipeline.tsx': 889,
  '/src/tipos/mercado.ts': 1027,
  '/src/tipos/relacionamento.ts': 654,
  '/src/tipos/territorio.ts': 1053,
};

describe('tamanho de arquivo', () => {
  it('nenhum arquivo passa do limite, e os grandes conhecidos só diminuem', () => {
    const problemas = Object.entries(FONTES)
      .filter(([arquivo]) => !arquivo.includes('.teste.'))
      .flatMap(([arquivo, codigo]) => {
        const linhas = codigo.split('\n').length;
        const teto = CONHECIDOS[arquivo];
        if (teto !== undefined) {
          if (linhas > teto) return [`${arquivo}: ${linhas} linhas, acima das ${teto} de 03/10/2026`];
          if (linhas <= LIMITE) return [`${arquivo}: já está com ${linhas} linhas — tire-o da lista`];
          return [];
        }
        return linhas > LIMITE ? [`${arquivo}: ${linhas} linhas, acima do limite de ${LIMITE}`] : [];
      });

    expect(problemas).toEqual([]);
  });
});
