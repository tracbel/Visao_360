/**
 * A tela baixada sob demanda (documento 54 §3.4): mostra o carregando, depois a tela; e, quando o pedaço sumiu depois de
 * uma publicação, recarrega a página uma vez em vez de deixar a tela em branco.
 */

import { render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { sobDemanda } from './sobDemanda';

// Armazenamento em memória: o `sessionStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('sessionStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

afterEach(() => {
  guardado.clear();
});

describe('a tela baixada sob demanda', () => {
  it('mostra o carregando e depois a tela', async () => {
    const Tela = sobDemanda(async () => ({ Pronta: () => <p>tela pronta</p> }), (m) => m.Pronta);
    render(<Tela />);
    expect(await screen.findByText('tela pronta')).toBeInTheDocument();
  });

  it('abre a tela mesmo com o armazenamento do navegador bloqueado', async () => {
    // NAVEGAÇÃO ANÔNIMA OU ARMAZENAMENTO BLOQUEADO: o acesso ao sessionStorage estoura, e a marca da recarga é conveniência.
    const bloqueado = () => {
      throw new DOMException('O armazenamento está bloqueado.', 'SecurityError');
    };
    vi.stubGlobal('sessionStorage', { getItem: bloqueado, setItem: bloqueado, removeItem: bloqueado, clear: bloqueado });
    try {
      const Tela = sobDemanda(async () => ({ Pronta: () => <p>tela com o armazenamento bloqueado</p> }), (m) => m.Pronta);
      render(<Tela />);
      expect(await screen.findByText('tela com o armazenamento bloqueado')).toBeInTheDocument();
    } finally {
      vi.stubGlobal('sessionStorage', {
        getItem: (chave: string) => guardado.get(chave) ?? null,
        setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
        removeItem: (chave: string) => void guardado.delete(chave),
        clear: () => guardado.clear(),
      });
    }
  });

  it('recarrega a página uma vez quando o pedaço sumiu depois de uma publicação', async () => {
    const recarregar = vi.fn();
    const original = window.location;
    Object.defineProperty(window, 'location', { configurable: true, value: { ...original, reload: recarregar } });
    try {
      const Tela = sobDemanda(() => Promise.reject(new TypeError('Failed to fetch dynamically imported module')), () => () => null);

      render(<Tela />);

      await waitFor(() => expect(recarregar).toHaveBeenCalledTimes(1));
    } finally {
      Object.defineProperty(window, 'location', { configurable: true, value: original });
    }
  });
});
