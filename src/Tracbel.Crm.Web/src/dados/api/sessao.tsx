/**
 * A sessão — se alguém entrou, quem é, e o que a tela deve desenhar por causa disso.
 *
 * É DIFERENTE DO CONTEXTO DE ACESSO, e os dois não se misturam. O contexto
 * (`contexto.tsx`) diz ONDE a pessoa está olhando — a filial. A sessão diz QUEM
 * ela é. A filial se troca a toda hora pelo seletor; a sessão só muda ao entrar
 * e ao sair.
 *
 * QUEM DECIDE É O SERVIDOR. A tela nunca conclui sozinha que alguém entrou: ela
 * pergunta `/auth/eu` e desenha o que ouviu. Um cookie de sessão é `HttpOnly` de
 * propósito — o JavaScript não consegue lê-lo, e por isso não consegue ser
 * enganado por ele.
 */

import { createContext, use, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { EVENTO_SESSAO_EXPIRADA } from './http';

export type Sessao =
  /** Ainda perguntando ao servidor. */
  | { estado: 'carregando' }
  /** O login pelo Entra ID está desligado nesta instalação — vale o cabeçalho provisório. */
  | { estado: 'provisorio' }
  /** Entrou, e o CRM conhece a pessoa. */
  | { estado: 'autenticado'; nome: string; nomePrincipal: string; email: string }
  /** Ninguém entrou, ou a sessão expirou. */
  | { estado: 'anonimo' }
  /** Entrou na Microsoft, mas o CRM não tem cadastro para essa conta. */
  | { estado: 'sem-acesso'; mensagem: string }
  /** O servidor não respondeu direito. */
  | { estado: 'erro'; mensagem: string };

type ValorDaSessao = {
  sessao: Sessao;
  /** Leva à Microsoft e, na volta, à tela em que a pessoa estava. */
  entrar: () => void;
};

const Contexto = createContext<ValorDaSessao | null>(null);

async function perguntarAoServidor(sinal: AbortSignal): Promise<Sessao> {
  let resposta: Response;
  try {
    resposta = await fetch('/auth/eu', {
      headers: { Accept: 'application/json' },
      credentials: 'same-origin',
      signal: sinal,
    });
  } catch (causa) {
    if (causa instanceof DOMException && causa.name === 'AbortError') throw causa;
    return { estado: 'erro', mensagem: 'Não foi possível falar com o servidor do CRM.' };
  }

  if (resposta.status === 401) return { estado: 'anonimo' };

  if (resposta.status === 403) {
    const problema = (await resposta.json().catch(() => ({}))) as { title?: string };
    return {
      estado: 'sem-acesso',
      mensagem: problema.title ?? 'Sua conta entrou, mas não tem cadastro no CRM.',
    };
  }

  if (!resposta.ok) {
    return { estado: 'erro', mensagem: `O servidor respondeu com erro (HTTP ${resposta.status}).` };
  }

  const corpo = (await resposta.json()) as {
    modo: 'entra' | 'provisorio';
    nome?: string;
    nomePrincipal?: string;
    email?: string;
  };

  if (corpo.modo !== 'entra') return { estado: 'provisorio' };

  return {
    estado: 'autenticado',
    nome: corpo.nome ?? corpo.nomePrincipal ?? '',
    nomePrincipal: corpo.nomePrincipal ?? '',
    email: corpo.email ?? corpo.nomePrincipal ?? '',
  };
}

/**
 * O endereço de volta depois do login.
 *
 * O roteador é por hash (`#/cobertura`), e o hash NUNCA viaja até o servidor —
 * sem mandá-lo explicitamente, toda pessoa voltaria para a Visão 360 depois de
 * entrar, mesmo tendo aberto o link de uma ficha de cliente. Da tela de login,
 * volta para o início.
 */
function enderecoDeVolta(): string {
  const hash = window.location.hash;
  if (!hash || hash.startsWith('#/login')) return '/';
  return `/${hash}`;
}

export function ProvedorDeSessao({ children }: { children: ReactNode }) {
  const [sessao, setSessao] = useState<Sessao>({ estado: 'carregando' });

  useEffect(() => {
    const controle = new AbortController();
    perguntarAoServidor(controle.signal)
      .then(setSessao)
      .catch(() => undefined);
    return () => controle.abort();
  }, []);

  // A SESSÃO QUE EXPIRA NO MEIO DO USO volta para o login, e não para um cartão
  // de erro. Oito horas depois de entrar, a próxima chamada à API responde 401;
  // `http.ts` avisa aqui, e a tela troca inteira.
  useEffect(() => {
    const aoExpirar = () => setSessao((atual) => (atual.estado === 'autenticado' ? { estado: 'anonimo' } : atual));
    window.addEventListener(EVENTO_SESSAO_EXPIRADA, aoExpirar);
    return () => window.removeEventListener(EVENTO_SESSAO_EXPIRADA, aoExpirar);
  }, []);

  const entrar = useCallback(() => {
    window.location.assign(`/auth/entrar?voltarPara=${encodeURIComponent(enderecoDeVolta())}`);
  }, []);

  const valor = useMemo(() => ({ sessao, entrar }), [sessao, entrar]);

  return <Contexto value={valor}>{children}</Contexto>;
}

/** A sessão corrente. Lança fora do provedor, pela mesma razão de `useContextoDeAcesso`. */
export function useSessao(): ValorDaSessao {
  const valor = use(Contexto);
  if (!valor) {
    throw new Error('useSessao foi chamado fora de <ProvedorDeSessao>. Confira App.tsx.');
  }
  return valor;
}
