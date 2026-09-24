/**
 * AS ABAS SUBLINHADAS DO MOMENTO SÃO UM `tablist` DE VERDADE (fidelidade às
 * maquetes, fase 3): papel, estado, ligação com o painel e teclado. O
 * `AbasInternas` segmentado, usado pelos outros blocos, não foi tocado.
 */

import { fireEvent, render, screen } from '@testing-library/react';
import { useState } from 'react';
import { describe, expect, it } from 'vitest';
import { AbasDoMomento } from './AbasDoMomento';

type Id = 'a' | 'b' | 'c';

export function Abas() {
  const [ativa, setAtiva] = useState<Id>('a');
  return (
    <AbasDoMomento
      rotulo="O que o momento mostra"
      ativa={ativa}
      aoTrocar={setAtiva}
      abas={[
        { id: 'a', rotulo: 'Composição do fator' },
        { id: 'b', rotulo: 'Rentabilidade' },
        { id: 'c', rotulo: 'Crédito' },
      ]}
    >
      <p>conteúdo {ativa}</p>
    </AbasDoMomento>
  );
}

describe('as abas do Momento do mercado', () => {
  it('têm os papéis do padrão de abas, e o painel aponta para a aba ativa', () => {
    render(<Abas />);

    expect(screen.getByRole('tablist', { name: 'O que o momento mostra' })).toBeInTheDocument();
    const ativa = screen.getByRole('tab', { name: 'Composição do fator' });
    expect(ativa).toHaveAttribute('aria-selected', 'true');
    expect(ativa).toHaveAttribute('tabindex', '0');
    expect(screen.getByRole('tab', { name: 'Rentabilidade' })).toHaveAttribute('tabindex', '-1');

    const painel = screen.getByRole('tabpanel');
    expect(painel).toHaveAttribute('aria-labelledby', ativa.id);
    expect(ativa).toHaveAttribute('aria-controls', painel.id);
  });

  it('as setas andam entre as abas (e dão a volta); Home e End vão às pontas', () => {
    render(<Abas />);
    const primeira = screen.getByRole('tab', { name: 'Composição do fator' });

    fireEvent.keyDown(primeira, { key: 'ArrowRight' });
    expect(screen.getByRole('tab', { name: 'Rentabilidade' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByRole('tab', { name: 'Rentabilidade' })).toHaveFocus();
    expect(screen.getByRole('tabpanel')).toHaveTextContent('conteúdo b');

    fireEvent.keyDown(screen.getByRole('tab', { name: 'Rentabilidade' }), { key: 'End' });
    expect(screen.getByRole('tab', { name: 'Crédito' })).toHaveAttribute('aria-selected', 'true');

    fireEvent.keyDown(screen.getByRole('tab', { name: 'Crédito' }), { key: 'ArrowRight' });
    expect(screen.getByRole('tab', { name: 'Composição do fator' })).toHaveAttribute('aria-selected', 'true');

    fireEvent.keyDown(screen.getByRole('tab', { name: 'Composição do fator' }), { key: 'ArrowLeft' });
    expect(screen.getByRole('tab', { name: 'Crédito' })).toHaveAttribute('aria-selected', 'true');

    fireEvent.keyDown(screen.getByRole('tab', { name: 'Crédito' }), { key: 'Home' });
    expect(screen.getByRole('tab', { name: 'Composição do fator' })).toHaveAttribute('aria-selected', 'true');
  });

  it('o clique troca a aba', () => {
    render(<Abas />);
    fireEvent.click(screen.getByRole('tab', { name: 'Crédito' }));
    expect(screen.getByRole('tabpanel')).toHaveTextContent('conteúdo c');
  });
});
