/**
 * A página de Configurações com dado de verdade (issue 134): o cabeçalho e "Minha conta" mostram quem está
 * usando, e as abas aparecem conforme a permissão. Nada de "Hugo Rocha · Admin TI" para todo mundo.
 */

import { render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import type { EscopoDoUsuario } from '../dados/api/acesso';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { Configuracoes } from './Configuracoes';

const obterEscopo = vi.hoisted(() => vi.fn());

vi.mock('../dados/api/acesso', async (original) => ({
  ...(await original<typeof import('../dados/api/acesso')>()),
  obterEscopo,
}));

vi.mock('../dados/api/sessao', () => ({
  useSessao: () => ({
    sessao: { estado: 'autenticado', nome: 'Pessoa Exemplo', nomePrincipal: 'pessoa.exemplo@exemplo.invalid', email: 'pessoa.exemplo@exemplo.invalid' },
  }),
}));

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const RIBEIRAO = { codigo: '010101', nome: 'Ribeirão Preto', ehCasa: true };

function escopo(parcial: Partial<EscopoDoUsuario>): EscopoDoUsuario {
  return {
    usuario: 'pessoa.exemplo',
    filialAtual: RIBEIRAO,
    filialPedidaRecusada: null,
    filiaisPermitidas: [RIBEIRAO],
    podeVerTodasAsFiliais: false,
    permissoes: [{ codigo: 'Cliente.Ler', descricao: 'Ler clientes', profundidade: 'EmpresaEAbaixo' }],
    perfis: [{ codigo: 'PADRAO', nome: 'Padrão', ehPadrao: true, filialCodigo: null, filialNome: null, expiraEm: null }],
    ...parcial,
  };
}

function responder(dados: EscopoDoUsuario) {
  obterEscopo.mockResolvedValue({
    dados,
    procedencia: { sistema: 'CRM Tracbel', objeto: 'seguranca.Perfil', lidoEmUtc: '2026-09-22T12:00:00Z', dadoMaisRecenteEm: null },
  });
}

function abrir() {
  render(
    <ProvedorDeContextoDeAcesso>
      <Configuracoes />
    </ProvedorDeContextoDeAcesso>,
  );
}

describe('Configurações', () => {
  afterEach(() => {
    obterEscopo.mockReset();
    guardado.clear();
  });

  it('o usuário comum vê a própria conta, com os dados reais, e nenhuma aba de administração', async () => {
    responder(escopo({}));
    abrir();

    expect(await screen.findByText('Quem você é')).toBeInTheDocument();
    expect(screen.queryByRole('tablist')).not.toBeInTheDocument();
    expect(screen.queryByText(/Hugo Rocha/)).not.toBeInTheDocument();

    expect(screen.getAllByText('Pessoa Exemplo').length).toBeGreaterThan(0);
    expect(screen.getByDisplayValue('pessoa.exemplo@exemplo.invalid')).toBeInTheDocument();
    expect(screen.getByText('Ler clientes')).toBeInTheDocument();
    expect(screen.getByText('a filial escolhida e as que estão abaixo dela')).toBeInTheDocument();
    expect(screen.getByText('todo usuário recebe')).toBeInTheDocument();
  });

  it('a gerência vê as três abas, e o cabeçalho diz o perfil concedido', async () => {
    responder(
      escopo({
        permissoes: [
          { codigo: 'PercepcaoDoGestor.Informar', descricao: 'Informar a percepção do gestor por município', profundidade: 'Organizacao' },
          { codigo: 'Integracao.Ler', descricao: 'Ler a situação das integrações', profundidade: 'EmpresaEAbaixo' },
        ],
        perfis: [
          { codigo: 'PADRAO', nome: 'Padrão', ehPadrao: true, filialCodigo: null, filialNome: null, expiraEm: null },
          { codigo: 'GERENCIA', nome: 'Gerência', ehPadrao: false, filialCodigo: null, filialNome: null, expiraEm: null },
        ],
      }),
    );
    abrir();

    const abas = await screen.findByRole('tablist');
    expect(within(abas).getAllByRole('tab').map((aba) => aba.textContent)).toEqual(['Minha conta', 'Comercial', 'Administração']);
    expect(screen.getAllByText('Gerência').length).toBeGreaterThan(0);
    expect(screen.getByText('em todas as filiais que você pode escolher, sem data para expirar')).toBeInTheDocument();
  });
});
