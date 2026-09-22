/**
 * A tela de Perfis (issue 113, parte 2b). O que se prende: o perfil do sistema abre só para ler, com "Duplicar";
 * duplicar abre um perfil novo já com as permissões da base, e criar manda o conjunto escolhido.
 */

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import type { PerfilNaAdministracao } from '../../dados/api/perfis';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { ConfigSecaoPerfis } from './ConfigSecaoPerfis';

const api = vi.hoisted(() => ({
  listarPerfisDaAdministracao: vi.fn(),
  listarCatalogoDePermissoes: vi.fn(),
  criarPerfil: vi.fn(),
  editarPerfil: vi.fn(),
  desativarPerfil: vi.fn(),
  reativarPerfil: vi.fn(),
}));

vi.mock('../../dados/api/perfis', () => api);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'seguranca.Perfil', lidoEmUtc: '2026-09-22T12:00:00Z', dadoMaisRecenteEm: null };

const GERENCIA: PerfilNaAdministracao = {
  codigo: 'GERENCIA', nome: 'Gerência', descricao: 'A gerência', ehPadrao: false, ehDoSistema: true, estaAtivo: true, pessoasComOPerfil: 3,
  permissoes: [{ codigo: 'Integracao.Ler', descricao: 'Ler a situação das integrações', profundidade: 'EmpresaEAbaixo' }],
};

describe('ConfigSecaoPerfis', () => {
  afterEach(() => {
    Object.values(api).forEach((f) => f.mockClear());
    guardado.clear();
  });

  it('o perfil do sistema abre só para ler, e duplicar cria um próprio com as permissões da base', async () => {
    api.listarPerfisDaAdministracao.mockResolvedValue({ dados: [GERENCIA], procedencia: PROCEDENCIA });
    api.listarCatalogoDePermissoes.mockResolvedValue({
      dados: [
        { codigo: 'Integracao.Ler', descricao: 'Ler a situação das integrações' },
        { codigo: 'Cliente.Excluir', descricao: 'Excluir cliente' },
      ],
      procedencia: PROCEDENCIA,
    });
    api.criarPerfil.mockResolvedValue({ ...GERENCIA, codigo: 'GERENCIA_PROPRIO', nome: 'Gerência (próprio)', ehDoSistema: false, pessoasComOPerfil: 0 });

    render(
      <ProvedorDeContextoDeAcesso>
        <ConfigSecaoPerfis />
      </ProvedorDeContextoDeAcesso>,
    );

    fireEvent.click(await screen.findByRole('button', { name: /Gerência/ }));
    expect(await screen.findByText(/Perfil do sistema: vem do código/)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Salvar alterações' })).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Onde vale: Excluir cliente')).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Duplicar' }));
    expect(await screen.findByLabelText('Código')).toHaveValue('GERENCIA_PROPRIO');
    expect(screen.getByLabelText('Onde vale: Ler a situação das integrações')).toHaveValue('EmpresaEAbaixo');

    fireEvent.change(screen.getByLabelText('Onde vale: Excluir cliente'), { target: { value: 'Empresa' } });
    fireEvent.click(screen.getByRole('button', { name: 'Criar perfil' }));

    await waitFor(() => expect(api.criarPerfil).toHaveBeenCalled());
    expect(api.criarPerfil.mock.calls[0][1]).toMatchObject({
      codigo: 'GERENCIA_PROPRIO',
      permissoes: [
        { codigo: 'Integracao.Ler', profundidade: 'EmpresaEAbaixo' },
        { codigo: 'Cliente.Excluir', profundidade: 'Empresa' },
      ],
    });
  });
});
