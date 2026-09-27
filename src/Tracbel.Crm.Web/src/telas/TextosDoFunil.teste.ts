/**
 * OS TEXTOS QUE FICARAM FALSOS NÃO VOLTAM (27/09/2026, documento 52 §8).
 *
 * O Funil, a Performance de CEN e a Visão 360 diziam números da carga legada de processos — "358 de 45.397"
 * processos com valor, "16.422" de fluxos que não são de venda, "50 fases" e "32 pares" do catálogo de fases, "45 mil
 * processos e 103 mil tarefas", e o "recorte carregado" do último contato. O funil agora é o estágio do Vórtice, o
 * último contato vem do histórico inteiro, e nenhuma dessas frases descreve mais o dado. Este teste lê o código das três
 * telas e recusa cada uma delas — comentário incluído, para ninguém copiar a frase de volta de um comentário.
 */

import { describe, expect, it } from 'vitest';
import funil from './Funil.tsx?raw';
import performance from './PerformanceCen.tsx?raw';
import painel from '../componentes/painel360/PainelExecutivo.tsx?raw';

const PROIBIDOS = [
  /358 de 45/,
  /45\.397/,
  /16\.422/,
  /50 fases/,
  /32 pares/,
  /pares fluxo × fase/,
  /45 mil processos/,
  /103 mil tarefas/,
  /recorte carregado/,
];

describe('os textos velhos do funil (documento 52 §8)', () => {
  it.each([
    ['Funil.tsx', funil],
    ['PerformanceCen.tsx', performance],
    ['PainelExecutivo.tsx', painel],
  ])('%s não diz mais nenhum deles', (_, fonte) => {
    for (const proibido of PROIBIDOS) expect(fonte).not.toMatch(proibido);
  });

  it('o Funil lê o funil por estágio, e não a fase do fluxo', () => {
    expect(funil).toContain('obterFunilPorEstagio');
    expect(funil).not.toMatch(/obterFunil\(/);
  });
});
