/**
 * O formulário da sazonalidade e dos pesos do IOC (issue 256). O que se prende:
 * - ele abre com o que vale hoje, e a soma aparece enquanto se digita;
 * - o envio leva os doze meses e os sete pesos;
 * - a recusa de um mês cai na caixa daquele mês, e não num aviso solto.
 */

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../../dados/api/contexto';
import { ErroDaApi } from '../../../dados/api/http';
import type { ParametroDoPlanejamentoDetalhe } from '../../../tipos/potencial';
import { FormularioDoPlanejamento } from './FormularioDoPlanejamento';

const api = vi.hoisted(() => ({ informarParametroDoPlanejamento: vi.fn() }));
vi.mock('../../../dados/api/potencial', () => api);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const VIGENTE: ParametroDoPlanejamentoDetalhe = {
  sazonalidade: [6.52, 6.89, 8.56, 8.59, 8.87, 8.58, 7.84, 8.75, 9.15, 10.51, 7.44, 8.3],
  pesos: { potencial: 25, cobertura: 20, credito: 15, rentabilidade: 15, clientes: 10, realizacao: 5, penetracao: 10 },
  vigencia: {
    vigenteDesde: '2026-09-27', justificativa: 'protótipo', informadoPor: null, informadoEm: '2026-09-27T00:00:00',
    revogadoEm: null, revogadoPor: null, motivoDaRevogacao: null,
  },
};

function desenhar() {
  const aoGravar = vi.fn();
  render(
    <ProvedorDeContextoDeAcesso>
      <FormularioDoPlanejamento vigente={VIGENTE} hoje="2026-09-28" aoGravar={aoGravar} aoCancelar={() => {}} />
    </ProvedorDeContextoDeAcesso>,
  );
  return { aoGravar };
}

afterEach(() => vi.clearAllMocks());

describe('FormularioDoPlanejamento', () => {
  it('abre com a vigência de hoje e a soma fechando o ano', () => {
    desenhar();

    expect(screen.getByLabelText('Out')).toHaveValue('10,51');
    expect(screen.getByLabelText('Potencial')).toHaveValue('25');
    expect(screen.getByRole('status')).toHaveTextContent('Soma: 100% — fecha o ano.');
  });

  it('a soma avisa antes de enviar quando o ano não fecha', () => {
    desenhar();

    fireEvent.change(screen.getByLabelText('Jan'), { target: { value: '16,52' } });

    expect(screen.getByRole('status')).toHaveTextContent('Soma: 110%');
    expect(screen.getByRole('status')).toHaveTextContent('precisa dar 100%');
  });

  it('o envio leva os doze meses e os sete pesos', async () => {
    api.informarParametroDoPlanejamento.mockResolvedValue(VIGENTE);
    const { aoGravar } = desenhar();

    fireEvent.change(screen.getByLabelText('Realização'), { target: { value: '0' } });
    fireEvent.change(screen.getByLabelText(/Justificativa/), { target: { value: 'reunião do comercial' } });
    fireEvent.click(screen.getByRole('button', { name: 'Registrar vigência' }));

    await waitFor(() => expect(aoGravar).toHaveBeenCalled());
    const corpo = api.informarParametroDoPlanejamento.mock.calls[0][1];
    expect(corpo.sazonalidade).toHaveLength(12);
    expect(corpo.sazonalidade[9]).toBe('10,51');
    expect(corpo.pesoDaRealizacao).toBe('0');
    expect(corpo.vigenteDesde).toBe('2026-09-28');
  });

  it('a recusa de um mês cai na caixa daquele mês', async () => {
    api.informarParametroDoPlanejamento.mockRejectedValue(
      new ErroDaApi(
        422,
        {
          title: 'A sazonalidade e os pesos do IOC têm campos a corrigir.',
          erros: [{ campo: 'sazonalidade[3]', mensagem: 'O percentual do mês vai de 0 a 100.', valorRecebido: '-1' }],
        },
        'falhou',
      ),
    );
    desenhar();

    fireEvent.click(screen.getByRole('button', { name: 'Registrar vigência' }));

    const abril = screen.getByLabelText('Abr');
    await waitFor(() => expect(abril).toHaveAttribute('aria-invalid', 'true'));
    expect(screen.getByText('O percentual do mês vai de 0 a 100.')).toBeInTheDocument();
  });
});
