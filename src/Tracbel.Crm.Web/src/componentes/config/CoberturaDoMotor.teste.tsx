/**
 * A cobertura dos dados do motor (issue 150). O aceite que este teste prende: o item incompleto fica em destaque,
 * com a frase que o servidor mandou; o completo não ganha destaque; o dado interno diz em quantas filiais foi
 * contado; e bloco longo esconde o resto atrás de um botão.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { BlocoDaCoberturaDoMotor, ItemDaCoberturaDoMotor, PainelDeCoberturaDoMotor } from '../../tipos/potencial';
import { PainelDeCobertura } from './CoberturaDoMotor';

function item(parcial: Partial<ItemDaCoberturaDoMotor> & Pick<ItemDaCoberturaDoMotor, 'codigo' | 'nome'>): ItemDaCoberturaDoMotor {
  return {
    unidade: 'municípios da ADR',
    total: 203,
    cobertos: 203,
    percentual: 100,
    situacao: 'Completa',
    motivo: '203 de 203 municípios da ADR com o dado.',
    paraQue: 'o potencial estrutural desta cultura',
    periodoInicial: '2022',
    periodoFinal: '2024',
    detalhe: null,
    ...parcial,
  };
}

function bloco(parcial: Partial<BlocoDaCoberturaDoMotor> & Pick<BlocoDaCoberturaDoMotor, 'codigo' | 'nome' | 'itens'>): BlocoDaCoberturaDoMotor {
  return { fonte: 'IBGE — PAM (SIDRA 5457), 2024', ehInterno: false, incompletos: 0, ...parcial };
}

const PAINEL: PainelDeCoberturaDoMotor = {
  municipiosDaAdr: 203,
  filiaisNoAlcance: 1,
  todasAsFiliais: false,
  incompletos: 2,
  grupos: [
    bloco({
      codigo: 'PAM',
      nome: 'Produção agrícola por cultura',
      incompletos: 1,
      itens: [
        item({ codigo: 'PAM.40106', nome: 'Cana-de-açúcar', detalhe: 'planta-se em 198' }),
        item({
          codigo: 'PAM.40139', nome: 'Café (em grão) Total', cobertos: 190, percentual: 93.6, situacao: 'Parcial',
          motivo: '190 de 203 municípios da ADR com o dado — faltam 13 para o potencial estrutural desta cultura.',
        }),
      ],
    }),
    bloco({
      codigo: 'VENDAS_DE_MAQUINA',
      nome: 'Vendas de máquina da Tracbel',
      fonte: 'ART',
      ehInterno: true,
      incompletos: 1,
      itens: [
        item({
          codigo: 'INTERNO.VENDA.MUNICIPIO', nome: 'Município do comprador, com código IBGE', unidade: 'vendas', total: 0, cobertos: 0,
          percentual: null, situacao: 'Vazia', motivo: 'Nenhum registro no banco — sem base para as vendas da Tracbel por município.',
          periodoInicial: null, periodoFinal: null,
        }),
      ],
    }),
  ],
};

function linhaDe(nome: string): HTMLElement {
  return screen.getByText(nome).closest('tr') as HTMLElement;
}

describe('PainelDeCobertura', () => {
  it('põe em destaque o item parcial e o vazio, com a frase do servidor', () => {
    render(<PainelDeCobertura painel={PAINEL} />);

    const parcial = linhaDe('Café (em grão) Total');
    expect(parcial).toHaveClass('pot-fonte-semdado');
    expect(within(parcial).getByText('Parcial')).toBeInTheDocument();
    expect(within(parcial).getByText(/faltam 13 para o potencial/)).toBeInTheDocument();
    expect(within(parcial).getByText(/93,6%/)).toBeInTheDocument();

    const vazio = linhaDe('Município do comprador, com código IBGE');
    expect(vazio).toHaveClass('pot-fonte-atrasada');
    expect(within(vazio).getByText(/Nenhum registro no banco/)).toBeInTheDocument();
    expect(within(vazio).getByText('—')).toBeInTheDocument();
  });

  it('não destaca o item completo nem repete a frase dele', () => {
    render(<PainelDeCobertura painel={PAINEL} />);

    const completo = linhaDe('Cana-de-açúcar');
    expect(completo).not.toHaveClass('pot-fonte-semdado');
    expect(completo).not.toHaveClass('pot-fonte-atrasada');
    expect(within(completo).queryByText(/com o dado/)).not.toBeInTheDocument();
    expect(within(completo).getByText('2022 a 2024')).toBeInTheDocument();
    expect(within(completo).getByText('planta-se em 198')).toBeInTheDocument();
  });

  it('o resumo conta os incompletos e diz em quantas filiais o dado interno foi contado', () => {
    render(<PainelDeCobertura painel={PAINEL} />);

    const resumo = screen.getByRole('status');
    expect(resumo).toHaveTextContent('2 itens incompletos');
    expect(resumo).toHaveTextContent('1 filial ao seu alcance');
    expect(screen.getByText(/dado interno, só contagem/)).toBeInTheDocument();
  });

  it('bloco longo mostra os primeiros oito e o botão abre o resto', () => {
    const muitos = Array.from({ length: 11 }, (_, i) => item({ codigo: `PAM.${i}`, nome: `Cultura ${i + 1}` }));
    render(<PainelDeCobertura painel={{ ...PAINEL, grupos: [bloco({ codigo: 'PAM', nome: 'Produção agrícola por cultura', itens: muitos })] }} />);

    expect(screen.queryByText('Cultura 9')).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Mostrar todos os 11' }));
    expect(screen.getByText('Cultura 11')).toBeInTheDocument();
  });
});
