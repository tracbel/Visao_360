/**
 * O formulário do share-alvo (issue 256): só categoria ativa aparece, e escolher a categoria traz o share de hoje.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../../dados/api/contexto';
import type { CategoriaNoCatalogo, ShareAlvoDetalhe } from '../../../tipos/potencial';
import { FormularioDoShare } from './FormularioDoShare';

const api = vi.hoisted(() => ({ informarShareAlvo: vi.fn() }));
vi.mock('../../../dados/api/potencial', () => api);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const categoria = (codigo: string, nome: string, estaAtiva = true): CategoriaNoCatalogo => ({
  codigo, nome, ordem: 1, estaAtiva, produtosDoSicor: [], linhasDeProduto: [],
});

const SHARE_DO_TRATOR: ShareAlvoDetalhe = {
  categoriaDeMaquinaCodigo: 'TRATOR',
  categoriaDeMaquinaNome: 'Trator',
  percentual: 31,
  vigencia: {
    vigenteDesde: '2026-09-27', justificativa: 'protótipo', informadoPor: null, informadoEm: '2026-09-27T00:00:00',
    revogadoEm: null, revogadoPor: null, motivoDaRevogacao: null,
  },
};

function desenhar() {
  const aoGravar = vi.fn();
  render(
    <ProvedorDeContextoDeAcesso>
      <FormularioDoShare
        categorias={[categoria('TRATOR', 'Trator'), categoria('IMPLEMENTO', 'Implemento'), categoria('ANTIGA', 'Antiga', false)]}
        vigentes={[SHARE_DO_TRATOR]}
        hoje="2026-09-28"
        aoGravar={aoGravar}
        aoCancelar={() => {}}
      />
    </ProvedorDeContextoDeAcesso>,
  );
  return { aoGravar };
}

afterEach(() => vi.clearAllMocks());

describe('FormularioDoShare', () => {
  it('só oferece categoria ativa', () => {
    desenhar();

    const opcoes = within(screen.getByLabelText(/Categoria de máquina/)).getAllByRole('option').map((o) => o.textContent);
    expect(opcoes).toEqual(['Selecione…', 'Trator', 'Implemento']);
  });

  it('escolher a categoria traz o share de hoje, e a que não tem share abre vazia', () => {
    desenhar();

    fireEvent.change(screen.getByLabelText(/Categoria de máquina/), { target: { value: 'TRATOR' } });
    expect(screen.getByLabelText(/Share-alvo/)).toHaveValue('31');

    fireEvent.change(screen.getByLabelText(/Categoria de máquina/), { target: { value: 'IMPLEMENTO' } });
    expect(screen.getByLabelText(/Share-alvo/)).toHaveValue('');
  });

  it('grava e diz o que ficou registrado', async () => {
    api.informarShareAlvo.mockResolvedValue({ ...SHARE_DO_TRATOR, percentual: 35 });
    const { aoGravar } = desenhar();

    fireEvent.change(screen.getByLabelText(/Categoria de máquina/), { target: { value: 'TRATOR' } });
    fireEvent.change(screen.getByLabelText(/Share-alvo/), { target: { value: '35' } });
    fireEvent.change(screen.getByLabelText(/Justificativa/), { target: { value: 'meta da diretoria' } });
    fireEvent.click(screen.getByRole('button', { name: 'Registrar share-alvo' }));

    await waitFor(() => expect(aoGravar).toHaveBeenCalledWith('Share-alvo de Trator registrado: 35%.'));
    expect(api.informarShareAlvo.mock.calls[0][1]).toEqual({
      categoriaDeMaquinaCodigo: 'TRATOR', percentual: '35', vigenteDesde: '2026-09-28', justificativa: 'meta da diretoria',
    });
  });
});
