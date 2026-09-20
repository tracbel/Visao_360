/**
 * O PRIMEIRO TESTE DE COMPONENTE DO PROJETO (issue [031]).
 *
 * O que ele prende é o que o `title=` do navegador não entrega: chegar na
 * explicação pelo teclado, pelo toque e pelo ponteiro, e o balão anunciado como
 * descrição do gatilho — não como um texto solto na tela.
 */

import { act, fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { InfoTooltip } from './InfoTooltip';

const TEXTO = 'Cobertura: clientes com interação nos últimos 180 dias (fonte: Vórtice, 09/2026).';

function montar() {
  render(<InfoTooltip texto={TEXTO} rotulo="Como a cobertura é calculada" />);
  return screen.getByRole('button', { name: 'Como a cobertura é calculada' });
}

/**
 * O foco de verdade, e não um evento simulado: é `elemento.focus()` que a tecla
 * Tab provoca. O `act` está aqui porque a tela só é redesenhada quando o React
 * termina o trabalho — sem ele, a afirmação leria o DOM de antes.
 */
function focarComOTeclado(elemento: HTMLElement) {
  act(() => elemento.focus());
}

describe('InfoTooltip', () => {
  it('não mostra a dica antes de alguém pedir', () => {
    montar();

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  it('abre com o foco do teclado e fecha com Esc', () => {
    const gatilho = montar();

    focarComOTeclado(gatilho);

    expect(screen.getByRole('tooltip')).toHaveTextContent(TEXTO);

    fireEvent.keyDown(gatilho, { key: 'Escape' });

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
    expect(gatilho).toHaveFocus();
  });

  it('liga o balão ao gatilho por aria-describedby, e só enquanto ele está aberto', () => {
    const gatilho = montar();

    expect(gatilho).not.toHaveAttribute('aria-describedby');

    focarComOTeclado(gatilho);

    expect(gatilho).toHaveAttribute('aria-describedby', screen.getByRole('tooltip').id);
  });

  it('abre com o ponteiro e fecha quando ele sai', () => {
    const gatilho = montar();
    const raiz = gatilho.parentElement!;

    fireEvent.mouseEnter(raiz);

    expect(screen.getByRole('tooltip')).toBeInTheDocument();

    fireEvent.mouseLeave(raiz);

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  it('abre e fecha no toque, que não tem ponteiro parado sobre nada', () => {
    const gatilho = montar();

    fireEvent.click(gatilho);

    expect(screen.getByRole('tooltip')).toBeInTheDocument();

    fireEvent.click(gatilho);

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  it('fecha ao tocar fora, para não ficar na tela depois que a pessoa seguiu adiante', () => {
    const gatilho = montar();

    fireEvent.click(gatilho);
    expect(screen.getByRole('tooltip')).toBeInTheDocument();

    fireEvent.mouseDown(document.body);

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  it('usa o gatilho que a tela mandar, no lugar do "i" redondo', () => {
    render(<InfoTooltip texto={TEXTO}>Cobertura</InfoTooltip>);

    expect(screen.getByRole('button', { name: 'Mais informação' })).toHaveTextContent('Cobertura');
  });
});
