/**
 * O editor da API com chave (#138): a Gestão de Negócios tem endereço e chave, e NÃO tem usuário. O endereço sugerido é o
 * NOME do servidor (D-M1), e a chave vai numa chamada própria, depois do endereço — como a senha das outras conexões.
 */

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import type { ConexaoNaTela } from '../../../dados/api/integracoes';
import { ProvedorDeContextoDeAcesso } from '../../../dados/api/contexto';
import { EditorDeConexao } from './EditorDeConexao';

const api = vi.hoisted(() => ({
  configurarConexao: vi.fn(),
  definirSegredo: vi.fn(),
  removerSegredo: vi.fn(),
}));

vi.mock('../../../dados/api/integracoes', () => api);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const GN: ConexaoNaTela = {
  codigo: 'GESTAO_NEGOCIOS', nome: 'Gestão de Negócios — API', tipo: 'ApiComChave', descricao: null, ehDoSistema: true, estaAtiva: true,
  endereco: null, porta: null, banco: null, objeto: null, usuario: null, nomeDoCabecalho: null, statusEsperado: 200,
  minutosEntreVerificacoes: null, enderecoEditavel: true, aceitaSegredo: true, origemDaCredencial: 'Nenhuma', temSegredoNaTela: false,
  segredoAlteradoEm: null, segredoAlteradoPor: null, ultimaVerificacaoEm: null, ultimaVerificacaoOk: null, ultimaVerificacaoResumo: null,
  rotinas: ['Metas de venda (Gestão de Negócios)'],
};

function montar(aoSalvar = vi.fn()) {
  render(
    <ProvedorDeContextoDeAcesso>
      <EditorDeConexao conexao={GN} aoSalvar={aoSalvar} aoCancelar={vi.fn()} />
    </ProvedorDeContextoDeAcesso>,
  );
  return aoSalvar;
}

describe('EditorDeConexao — API com chave', () => {
  it('não tem campo de usuário, pede a chave e sugere o endereço pelo nome', () => {
    montar();

    expect(screen.queryByLabelText('Usuário')).toBeNull();
    expect(screen.getByLabelText('Chave da API')).toBeTruthy();
    expect(screen.getByLabelText('Endereço (URL)').getAttribute('placeholder')).toBe('https://agro-sistemas-w.tracbel.com.br:5001');
    expect(screen.getByText(/Use o NOME do servidor, e não o IP/)).toBeTruthy();
  });

  it('grava o endereço e, numa chamada própria, a chave', async () => {
    api.configurarConexao.mockResolvedValue({ ...GN, endereco: 'https://agro-sistemas-w.tracbel.com.br:5001' });
    api.definirSegredo.mockResolvedValue({ ...GN, temSegredoNaTela: true, origemDaCredencial: 'Tela' });
    const aoSalvar = montar();

    fireEvent.change(screen.getByLabelText('Endereço (URL)'), { target: { value: 'https://agro-sistemas-w.tracbel.com.br:5001' } });
    fireEvent.change(screen.getByLabelText('Chave da API'), { target: { value: 'chave-de-teste' } });
    fireEvent.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => expect(aoSalvar).toHaveBeenCalled());
    expect(api.configurarConexao.mock.calls[0][1]).toBe('GESTAO_NEGOCIOS');
    expect(api.configurarConexao.mock.calls[0][2]).toMatchObject({ endereco: 'https://agro-sistemas-w.tracbel.com.br:5001', usuario: '' });
    expect(api.definirSegredo).toHaveBeenCalledWith(expect.anything(), 'GESTAO_NEGOCIOS', 'chave-de-teste');
  });
});
