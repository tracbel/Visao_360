/**
 * O painel de fontes públicas (issue 77). O aceite que este teste prende: "fonte atrasada fica em destaque" —
 * a linha muda de cor, a situação vem escrita e a frase diz por quê. Fonte em dia não ganha destaque nenhum.
 */

import { render, screen, within } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { FontePublicaResumo, PainelDeFontesPublicas } from '../../tipos/potencial';
import { TabelaDeFontes } from './ConfigSecaoFontes';

function fonte(parcial: Partial<FontePublicaResumo> & Pick<FontePublicaResumo, 'fluxo' | 'nome' | 'situacao'>): FontePublicaResumo {
  return {
    orgao: 'IBGE',
    oQueTraz: 'dado de teste',
    tabela: 'organizacao.Teste',
    rotina: 'Preços, custos e crédito',
    rotinaCodigo: 'PRECOS_MENSAIS',
    rotinaLigada: true,
    agenda: 'todo mês no dia 20, às 04:00',
    cadencia: 'Mensal',
    motivo: '',
    ultimaAtualizacaoEm: '2026-09-20T07:30:00',
    ultimaExecucaoPrevistaEm: '2026-09-20T07:00:00',
    proximaExecucaoEm: '2026-10-20T07:00:00',
    linhas: 100,
    periodoInicial: '2025-09',
    periodoFinal: '2026-08',
    municipiosCobertos: null,
    registrosLidos: 100,
    registrosGravados: 100,
    recusados: 0,
    recusasPendentes: 0,
    exemplosDeRecusa: [],
    ...parcial,
  };
}

const PAINEL: PainelDeFontesPublicas = {
  municipiosDaAdr: 203,
  atrasadas: 1,
  semDado: 1,
  fontes: [
    fonte({ fluxo: 'IBGE.PRODUCAO_AGRICOLA', nome: 'Produção Agrícola Municipal', situacao: 'EmDia', municipiosCobertos: 203, periodoInicial: '2022', periodoFinal: '2024' }),
    fonte({
      fluxo: 'BCB.PTAX_MENSAL', nome: 'Dólar PTAX', situacao: 'Atrasada',
      motivo: 'A rotina TracbelCrmPrecos devia ter rodado em 20/09/2026 e o fluxo não foi atualizado depois disso.',
      ultimaAtualizacaoEm: '2026-07-20T07:30:00',
    }),
    fonte({
      fluxo: 'CONAB.PRECO_RECEBIDO', nome: 'Preço recebido pelo produtor', situacao: 'SemDado',
      motivo: 'A tabela está vazia e o fluxo nunca rodou neste banco.', ultimaAtualizacaoEm: null, linhas: 0,
      periodoInicial: null, periodoFinal: null, recusasPendentes: 2, exemplosDeRecusa: ['Produto sem código.'],
    }),
  ],
};

function linhaDe(nome: string): HTMLElement {
  return screen.getByText(nome).closest('tr') as HTMLElement;
}

describe('TabelaDeFontes', () => {
  it('põe em destaque a fonte atrasada e a sem dado, com a frase que explica', () => {
    render(<TabelaDeFontes painel={PAINEL} />);

    const atrasada = linhaDe('Dólar PTAX');
    expect(atrasada).toHaveClass('pot-fonte-atrasada');
    expect(within(atrasada).getByText('Atrasada')).toBeInTheDocument();
    expect(within(atrasada).getByText(/devia ter rodado em 20\/09\/2026/)).toBeInTheDocument();

    const semDado = linhaDe('Preço recebido pelo produtor');
    expect(semDado).toHaveClass('pot-fonte-semdado');
    expect(within(semDado).getByText('nunca')).toBeInTheDocument();
    expect(within(semDado).getByText('Produto sem código.')).toBeInTheDocument();
  });

  it('não destaca a fonte em dia e mostra a cobertura sobre a ADR', () => {
    render(<TabelaDeFontes painel={PAINEL} />);

    const emDia = linhaDe('Produção Agrícola Municipal');
    expect(emDia).not.toHaveClass('pot-fonte-atrasada');
    expect(emDia).not.toHaveClass('pot-fonte-semdado');
    expect(within(emDia).getByText('203 de 203')).toBeInTheDocument();
    expect(within(emDia).getByText('2022 a 2024')).toBeInTheDocument();
  });

  it('o resumo conta as atrasadas e as sem dado', () => {
    render(<TabelaDeFontes painel={PAINEL} />);

    const resumo = screen.getByRole('status');
    expect(resumo).toHaveTextContent('3 fontes');
    expect(resumo).toHaveTextContent('1 atrasada');
    expect(resumo).toHaveTextContent('1 sem dado');
  });
});
