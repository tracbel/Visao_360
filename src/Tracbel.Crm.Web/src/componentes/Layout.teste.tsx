/**
 * O SHELL — menu lateral e barra do topo (fidelidade às maquetes, 23/09/2026).
 *
 * O visual veio da maquete, e o que este teste prende é o que o visual NÃO
 * pode ter levado junto:
 *
 * 1. **Os itens são os nossos**, na ordem de `navegacao.ts` — a maquete lista
 *    outras telas, e nenhuma delas pode entrar por descuido.
 * 2. **A tela atual é a única marcada**, por `aria-current`, que é o que o
 *    leitor de tela anuncia — a pílula verde é só a versão para os olhos.
 * 3. **Sair continua ao alcance do teclado**, agora dentro do menu do usuário:
 *    um botão de verdade, o Sair logo depois dele, e o `Esc` devolvendo o foco.
 *
 * `rotas` é substituído por uma tabela mínima: este teste é do shell, e montar
 * as treze telas só o faria quebrar pelo motivo de outra.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import type { Sessao } from '../dados/api/sessao';
import { Layout } from './Layout';
import { SECOES } from './navegacao';

const useSessao = vi.hoisted(() => vi.fn());
vi.mock('../dados/api/sessao', () => ({ useSessao }));

vi.mock('../rotas', () => ({
  acharRota: (caminho: string) => ({
    caminho,
    titulo: 'Tela',
    trilha: caminho === '/relatorios/territorio' ? ['Relatórios', 'Indicadores Geográficos'] : ['Tela'],
    // Sem API, sem seletor de filial: ele tem o próprio teste.
    usaApi: false,
  }),
}));

// O mesmo armazenamento em memória do teste do seletor de filial: o Node 25 tem
// um `localStorage` global que não guarda nada e encobre o do jsdom.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const AUTENTICADO: Sessao = {
  estado: 'autenticado',
  nome: 'Pessoa de Teste',
  nomePrincipal: 'pessoa.teste@exemplo.invalid',
  email: 'pessoa.teste@exemplo.invalid',
};

function montar(caminho: string, sessao: Sessao = AUTENTICADO) {
  useSessao.mockReturnValue({ sessao, entrar: vi.fn() });
  const roteador = createMemoryRouter(
    [{ path: '/', element: <Layout />, children: [{ path: '*', element: <p>conteúdo</p> }] }],
    { initialEntries: [caminho] },
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <RouterProvider router={roteador} />
    </ProvedorDeContextoDeAcesso>,
  );
}

function menuPrincipal() {
  return screen.getByRole('navigation', { name: 'Menu principal' });
}

describe('menu lateral', () => {
  afterEach(() => useSessao.mockReset());

  it('mostra as nossas telas, na ordem de navegacao.ts, sob as quatro seções', () => {
    montar('/relatorios/territorio');

    const rotulos = within(menuPrincipal())
      .getAllByRole('link')
      .map((link) => link.textContent);
    expect(rotulos).toEqual(SECOES.flatMap((secao) => secao.itens.map((item) => item.rotulo)));

    for (const secao of SECOES) expect(within(menuPrincipal()).getByText(secao.titulo)).toBeInTheDocument();
  });

  it('marca só a tela atual com aria-current', () => {
    montar('/relatorios/territorio');

    const marcados = within(menuPrincipal())
      .getAllByRole('link')
      .filter((link) => link.getAttribute('aria-current') === 'page');
    expect(marcados.map((link) => link.textContent)).toEqual(['Indicadores Geográficos']);
  });

  it('a trilha do topo termina na tela atual', () => {
    montar('/relatorios/territorio');

    const trilha = screen.getByRole('navigation', { name: 'Você está em' });
    expect(trilha).toHaveTextContent('RelatóriosIndicadores Geográficos');
    expect(within(trilha).getByText('Indicadores Geográficos')).toHaveAttribute('aria-current', 'page');
  });
});

describe('menu do usuário', () => {
  afterEach(() => useSessao.mockReset());

  it('o Sair fica atrás de um botão, e o Esc fecha e devolve o foco ao botão', () => {
    montar('/relatorios/territorio');

    const botao = screen.getByRole('button', { name: /Pessoa de Teste/ });
    expect(botao).toHaveAttribute('aria-expanded', 'false');
    expect(screen.queryByRole('link', { name: 'Sair' })).not.toBeInTheDocument();

    // Um <button> de verdade: Enter e Espaço disparam o clique no navegador.
    expect(botao.tagName).toBe('BUTTON');
    botao.focus();
    fireEvent.click(botao);

    expect(botao).toHaveAttribute('aria-expanded', 'true');
    const sair = screen.getByRole('link', { name: 'Sair' });
    // O mesmo endereço de antes: sair passa pelo servidor, e não pelo roteador.
    expect(sair).toHaveAttribute('href', '/auth/sair');
    expect(sair).not.toHaveAttribute('tabindex', '-1');

    sair.focus();
    fireEvent.keyDown(sair, { key: 'Escape' });
    expect(screen.queryByRole('link', { name: 'Sair' })).not.toBeInTheDocument();
    expect(botao).toHaveAttribute('aria-expanded', 'false');
    expect(botao).toHaveFocus();
  });

  it('o e-mail mora DENTRO do menu, e não num title= — e o aria-controls só aponta para o painel aberto', () => {
    // A REVISÃO DE 24/09/2026 ACHOU DOIS DEFEITOS AQUI: o e-mail era um `title`
    // sobre o nome (o teclado e o toque não o alcançam), e o `aria-controls`
    // apontava para o painel também fechado, quando ele não existe.
    montar('/relatorios/territorio');

    const botao = screen.getByRole('button', { name: /Pessoa de Teste/ });
    expect(document.querySelector('.sidebar [title]')).toBeNull();
    expect(botao).not.toHaveAttribute('aria-controls');
    expect(screen.queryByText('pessoa.teste@exemplo.invalid')).not.toBeInTheDocument();

    fireEvent.click(botao);
    const painel = document.getElementById(botao.getAttribute('aria-controls')!)!;
    expect(painel).toBeInTheDocument();
    expect(within(painel).getByText('pessoa.teste@exemplo.invalid')).toBeInTheDocument();
  });

  it('no acesso provisório não há o que sair, e o rodapé diz que é provisório', () => {
    montar('/relatorios/territorio', { estado: 'provisorio' });

    expect(screen.queryByRole('button', { name: /filial/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Sair' })).not.toBeInTheDocument();
    expect(screen.getByText(/acesso provisório/)).toBeInTheDocument();
  });
});

describe('gaveta (abaixo de 900 px)', () => {
  afterEach(() => useSessao.mockReset());

  it('o ☰ abre e fecha, e o Esc fecha devolvendo o foco ao ☰', () => {
    montar('/relatorios/territorio');

    const abrir = screen.getByRole('button', { name: 'Abrir menu' });
    expect(abrir).toHaveAttribute('aria-controls', 'menu-lateral');
    expect(abrir).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(abrir);
    const fechar = screen.getByRole('button', { name: 'Fechar menu' });
    expect(fechar).toHaveAttribute('aria-expanded', 'true');

    fireEvent.keyDown(document, { key: 'Escape' });
    expect(screen.getByRole('button', { name: 'Abrir menu' })).toHaveFocus();
  });
});
