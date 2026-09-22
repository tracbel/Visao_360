/**
 * "Todas as filiais" (21/09/2026: "o perfil admin tem todas as filiais e todos os recursos"). O que este
 * teste prende: a opção só aparece quando a rota de escopo diz que a pessoa pode, e em "Todas as filiais" o
 * botão de cadastro novo fica desligado — o registro novo precisa nascer numa filial.
 */

import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { EscopoDoUsuario } from '../../dados/api/acesso';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { BotaoDeNovoCadastro } from './BotaoDeNovoCadastro';
import { SeletorDeFilial } from './SeletorDeFilial';

const obterEscopo = vi.hoisted(() => vi.fn());

vi.mock('../../dados/api/acesso', async (original) => ({
  ...(await original<typeof import('../../dados/api/acesso')>()),
  obterEscopo,
}));

const CHAVE_GUARDADA = 'tracbel-crm:contexto-acesso';

// UM ARMAZENAMENTO EM MEMÓRIA, e não o localStorage do ambiente: a partir do Node 25 existe um global
// próprio que, sem --localstorage-file, não guarda nada e encobre o do jsdom. O teste não pode depender
// de qual Node está rodando (o CI usa o do .nvmrc).
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

function escopo(podeVerTodasAsFiliais: boolean): EscopoDoUsuario {
  return {
    usuario: 'Pessoa de teste',
    filialAtual: { codigo: '010101', nome: 'Ribeirão Preto', ehCasa: true },
    filialPedidaRecusada: null,
    filiaisPermitidas: [
      { codigo: '010101', nome: 'Ribeirão Preto', ehCasa: true },
      { codigo: '010102', nome: 'Franca', ehCasa: false },
    ],
    permissoes: [],
    podeVerTodasAsFiliais,
    perfis: [],
  };
}

function responder(podeVerTodasAsFiliais: boolean) {
  obterEscopo.mockResolvedValue({
    dados: escopo(podeVerTodasAsFiliais),
    procedencia: { sistema: 'CRM Tracbel', objeto: 'seguranca.Perfil', lidoEmUtc: '2026-09-21T12:00:00Z', dadoMaisRecenteEm: null },
  });
}

function guardarFilial(empresa: string) {
  localStorage.setItem(CHAVE_GUARDADA, JSON.stringify({ usuario: 'pessoa@tracbel.com.br', empresa }));
}

describe('SeletorDeFilial', () => {
  beforeEach(() => guardarFilial('010101'));
  afterEach(() => {
    localStorage.clear();
    obterEscopo.mockReset();
  });

  it('oferece "Todas as filiais" a quem pode', async () => {
    responder(true);
    render(
      <ProvedorDeContextoDeAcesso>
        <SeletorDeFilial />
      </ProvedorDeContextoDeAcesso>,
    );

    expect(await screen.findByRole('option', { name: 'Todas as filiais' })).toHaveValue('TODAS');
    expect(screen.getByRole('option', { name: 'Franca' })).toBeInTheDocument();
  });

  it('não oferece "Todas as filiais" a quem não pode', async () => {
    responder(false);
    render(
      <ProvedorDeContextoDeAcesso>
        <SeletorDeFilial />
      </ProvedorDeContextoDeAcesso>,
    );

    expect(await screen.findByRole('option', { name: 'Franca' })).toBeInTheDocument();
    expect(screen.queryByRole('option', { name: 'Todas as filiais' })).not.toBeInTheDocument();
  });
});

describe('BotaoDeNovoCadastro', () => {
  afterEach(() => localStorage.clear());

  it('em "Todas as filiais" fica desligado e diz por quê', () => {
    guardarFilial('TODAS');
    render(
      <ProvedorDeContextoDeAcesso>
        <MemoryRouter>
          <BotaoDeNovoCadastro para="/clientes/novo" rotulo="Novo cliente" />
        </MemoryRouter>
      </ProvedorDeContextoDeAcesso>,
    );

    const botao = screen.getByRole('button', { name: 'Novo cliente' });
    expect(botao).toBeDisabled();
    expect(botao).toHaveAttribute('title', expect.stringMatching(/escolha no seletor a filial/));
  });

  it('numa filial é o link para o formulário', () => {
    guardarFilial('010101');
    render(
      <ProvedorDeContextoDeAcesso>
        <MemoryRouter>
          <BotaoDeNovoCadastro para="/clientes/novo" rotulo="Novo cliente" />
        </MemoryRouter>
      </ProvedorDeContextoDeAcesso>,
    );

    expect(screen.getByRole('link', { name: 'Novo cliente' })).toHaveAttribute('href', '/clientes/novo');
  });
});
