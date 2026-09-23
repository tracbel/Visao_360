/**
 * AS DUAS REGRAS DE COMPARAÇÃO (documento 50, §7; issue 163).
 *
 * O que está sob prova aqui é a distinção que produz número sem significado
 * quando se perde: grandeza somável tem FATIA; razão tem DISTÂNCIA. Dizer
 * "captura 14,8% · 0,8% de SP" é somar peras com a árvore.
 */

import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { MedidaDeRazao, MedidaSomavel } from '../../tipos/territorio';
import { ComparacaoDeRazao, ComparacaoSomavel } from './Comparacao';
import { distanciasEmTexto, fatia, fatiasEmTexto, montarSomavel } from './comparacoes';
import { ValorAusente } from './ValorAusente';

const somavel = (parcial: Partial<MedidaSomavel> = {}): MedidaSomavel => ({
  ...montarSomavel(37_226, 886_333, 8_000_000),
  ...parcial,
});

const razao = (parcial: Partial<MedidaDeRazao> = {}): MedidaDeRazao => ({
  valor: 1.63,
  referenciaRegiaoTracbel: 1.54,
  referenciaSaoPaulo: 1.48,
  ehPercentual: false,
  unidade: 't/ha',
  motivoDaAusencia: null,
  procedencia: null,
  ...parcial,
});

describe('a fatia', () => {
  it('divide a parte pelo total', () => {
    expect(fatia(37_226, 886_333)).toBeCloseTo(4.2, 1);
  });

  it('denominador ausente NÃO vira 0% — ausência e zero são coisas diferentes', () => {
    expect(fatia(37_226, null)).toBeNull();
    expect(fatia(null, 886_333)).toBeNull();
  });

  it('denominador zerado não vira divisão por zero nem 0%', () => {
    expect(fatia(37_226, 0)).toBeNull();
  });

  it('zero COM denominador é zero de verdade, e aparece', () => {
    expect(fatia(0, 886_333)).toBe(0);
  });
});

describe('grandeza somável', () => {
  it('mostra as duas fatias, e diz "Região Tracbel" — não "região"', () => {
    expect(fatiasEmTexto(somavel())).toBe('4,2% da Região Tracbel · 0,5% de SP');
  });

  it('sem os dois denominadores, não sobra texto nenhum — e não sobra 0%', () => {
    expect(fatiasEmTexto(montarSomavel(37_226, null, null))).toBeNull();
  });

  it('com um denominador só, mostra só a comparação que existe', () => {
    expect(fatiasEmTexto(montarSomavel(37_226, 886_333, null))).toBe('4,2% da Região Tracbel');
  });

  it('renderiza valor e contexto, e o contexto não disputa o olho com o número', () => {
    render(<ComparacaoSomavel medida={somavel()} oQue="a área plantada" formatar={(v) => `${v.toLocaleString('pt-BR')} ha`} />);

    expect(screen.getByText('37.226 ha')).toBeInTheDocument();
    expect(screen.getByText('4,2% da Região Tracbel · 0,5% de SP')).toBeInTheDocument();
  });

  it('valor ausente vira traço com motivo, e não zero', () => {
    render(
      <ComparacaoSomavel
        medida={somavel({ valor: null, motivoDaAusencia: 'o IBGE suprimiu o valor por sigilo' })}
        oQue="a área plantada"
        formatar={String}
      />,
    );

    expect(screen.queryByText('0')).not.toBeInTheDocument();
    fireEvent.focus(screen.getByRole('button', { name: 'Por que a área plantada não aparece' }));
    expect(screen.getByRole('tooltip')).toHaveTextContent('o IBGE suprimiu o valor por sigilo');
  });
});

describe('grandeza do tipo razão', () => {
  it('mostra a DISTÂNCIA até cada referência, e nunca uma fatia', () => {
    const texto = distanciasEmTexto(razao())!;

    expect(texto).toBe('6% acima da Região Tracbel · 10% acima de SP');
    expect(texto).not.toMatch(/% de SP/);
    expect(texto).not.toMatch(/fatia/i);
  });

  it('quando a grandeza já é percentual, a distância é em PONTOS PERCENTUAIS', () => {
    // Captura 14,8% contra 11,6% da Região: +3,2 p.p. — e não "+27% acima",
    // que seria outra afirmação.
    const captura = razao({ valor: 14.8, referenciaRegiaoTracbel: 11.6, referenciaSaoPaulo: null, ehPercentual: true, unidade: '%' });
    expect(distanciasEmTexto(captura)).toBe('+3,2 p.p. vs Região Tracbel');
  });

  it('abaixo da referência diz abaixo, com o sinal certo', () => {
    expect(distanciasEmTexto(razao({ valor: 1.4, referenciaRegiaoTracbel: 1.54, referenciaSaoPaulo: null }))).toBe(
      '9% abaixo da Região Tracbel',
    );
    expect(
      distanciasEmTexto(razao({ valor: 9.5, referenciaRegiaoTracbel: 11.6, referenciaSaoPaulo: null, ehPercentual: true })),
    ).toBe('−2,1 p.p. vs Região Tracbel');
  });

  it('praticamente igual à referência não vira "0% acima": diz que está na média', () => {
    expect(distanciasEmTexto(razao({ valor: 1.541, referenciaRegiaoTracbel: 1.54, referenciaSaoPaulo: null }))).toBe(
      'na média da Região Tracbel',
    );
  });

  it('sem referência, não há comparação — e não há 0%', () => {
    expect(distanciasEmTexto(razao({ referenciaRegiaoTracbel: null, referenciaSaoPaulo: null }))).toBeNull();
  });

  it('renderiza valor, unidade e distância', () => {
    render(<ComparacaoDeRazao medida={razao()} oQue="a produtividade" formatar={(v) => v.toLocaleString('pt-BR')} />);

    expect(screen.getByText('1,63')).toBeInTheDocument();
    expect(screen.getByText('t/ha')).toBeInTheDocument();
    expect(screen.getByText('6% acima da Região Tracbel · 10% acima de SP')).toBeInTheDocument();
  });
});

describe('ValorAusente', () => {
  it('põe traço no lugar do número e o motivo na dica — nunca prosa dentro do valor', () => {
    render(<ValorAusente motivo="o ciclo de renovação ainda não foi definido" oQue="a demanda anual" />);

    expect(screen.getByText('—')).toBeInTheDocument();
    // A prosa NÃO está no corpo: só aparece quando a dica abre.
    expect(screen.queryByText('o ciclo de renovação ainda não foi definido')).not.toBeInTheDocument();

    fireEvent.focus(screen.getByRole('button', { name: 'Por que a demanda anual não aparece' }));
    expect(screen.getByRole('tooltip')).toHaveTextContent('o ciclo de renovação ainda não foi definido');
  });

  it('o leitor de tela ouve "sem dado", e não um travessão mudo', () => {
    render(<ValorAusente motivo="fonte ainda não integrada" oQue="o rebanho" />);
    expect(screen.getByText('sem dado')).toBeInTheDocument();
  });
});
