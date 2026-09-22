/**
 * A tela de Usuários (issue 113). O que se prende: quem administra vê a fila de liberação e libera na filial
 * escolhida; quem só vê (a gerência) não tem a fila nem as ações.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import type { UsuarioDetalhado, UsuarioNaAdministracao } from '../../dados/api/administracao';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { ConfigSecaoUsuarios } from './ConfigSecaoUsuarios';

const api = vi.hoisted(() => ({
  listarUsuarios: vi.fn(),
  obterUsuario: vi.fn(),
  listarPerfisParaConceder: vi.fn(),
  liberarUsuario: vi.fn(),
  concederPerfil: vi.fn(),
  revogarConcessao: vi.fn(),
  desativarUsuario: vi.fn(),
  reativarUsuario: vi.fn(),
}));

vi.mock('../../dados/api/administracao', () => api);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'seguranca.Usuario', lidoEmUtc: '2026-09-22T12:00:00Z', dadoMaisRecenteEm: null };
const FILIAIS = [
  { codigo: '010101', nome: 'Ribeirão Preto', ehCasa: true },
  { codigo: '010103', nome: 'Barretos', ehCasa: false },
];

const PENDENTE: UsuarioNaAdministracao = {
  chave: 'a1', nome: 'Pessoa Nova', nomePrincipal: 'pessoa.nova@exemplo.invalid', natureza: 'Pessoa',
  filialCodigo: '010101', filialNome: 'Ribeirão Preto', estaAtivo: true,
  aguardandoLiberacaoDesde: '2026-09-22T11:00:00Z', ultimoLoginEm: '2026-09-22T11:00:00Z', perfis: [],
};

function pagina(itens: UsuarioNaAdministracao[]) {
  return { dados: { itens, pagina: 1, tamanho: 25, total: itens.length, totalDePaginas: 1, temProxima: false }, procedencia: PROCEDENCIA };
}

function abrir(podeAdministrar: boolean) {
  render(
    <ProvedorDeContextoDeAcesso>
      <ConfigSecaoUsuarios podeAdministrar={podeAdministrar} filiais={FILIAIS} />
    </ProvedorDeContextoDeAcesso>,
  );
}

describe('ConfigSecaoUsuarios', () => {
  afterEach(() => {
    Object.values(api).forEach((f) => f.mockClear());
    guardado.clear();
  });

  it('quem administra começa pela fila de liberação e libera na filial escolhida', async () => {
    api.listarUsuarios.mockResolvedValue(pagina([PENDENTE]));
    api.obterUsuario.mockResolvedValue({ dados: { usuario: PENDENTE, concessoes: [] } satisfies UsuarioDetalhado, procedencia: PROCEDENCIA });
    api.listarPerfisParaConceder.mockResolvedValue({ dados: [], procedencia: PROCEDENCIA });
    api.liberarUsuario.mockResolvedValue({
      usuario: { ...PENDENTE, aguardandoLiberacaoDesde: null, filialCodigo: '010103', filialNome: 'Barretos' },
      concessoes: [],
    } satisfies UsuarioDetalhado);

    abrir(true);

    const situacoes = screen.getByRole('tablist', { name: 'Situação da conta' });
    expect(within(situacoes).getAllByRole('tab').map((t) => t.textContent)).toEqual(['Aguardando liberação', 'Ativas', 'Desativadas']);
    expect(api.listarUsuarios.mock.calls[0][1]).toMatchObject({ situacao: 'AguardandoLiberacao' });

    fireEvent.click(await screen.findByRole('button', { name: 'Pessoa Nova' }));
    expect(await screen.findByText('Liberar a conta')).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText('Filial de casa'), { target: { value: '010103' } });
    fireEvent.click(screen.getByRole('button', { name: 'Liberar' }));

    // A LIBERAÇÃO TERMINOU quando o bloco some; aí o campo "Filial de casa" é o da conta, e não a escolha.
    await waitFor(() => expect(screen.queryByText('Liberar a conta')).not.toBeInTheDocument());
    expect(api.liberarUsuario).toHaveBeenCalledWith(expect.anything(), 'a1', '010103');
    expect(screen.getByLabelText('Filial de casa')).toHaveValue('Barretos');
  });

  it('quem só vê não tem a fila de liberação nem as ações', async () => {
    const ativo = { ...PENDENTE, aguardandoLiberacaoDesde: null, perfis: ['Gerência'] };
    api.listarUsuarios.mockResolvedValue(pagina([ativo]));
    api.obterUsuario.mockResolvedValue({ dados: { usuario: ativo, concessoes: [] } satisfies UsuarioDetalhado, procedencia: PROCEDENCIA });

    abrir(false);

    const situacoes = screen.getByRole('tablist', { name: 'Situação da conta' });
    expect(within(situacoes).getAllByRole('tab').map((t) => t.textContent)).toEqual(['Ativas', 'Desativadas']);

    fireEvent.click(await screen.findByRole('button', { name: 'Pessoa Nova' }));
    expect(await screen.findByText('Perfis')).toBeInTheDocument();
    expect(screen.queryByText('Conceder perfil')).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Desativar conta' })).not.toBeInTheDocument();
    expect(api.listarPerfisParaConceder).not.toHaveBeenCalled();
  });
});
