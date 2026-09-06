/**
 * O contexto de acesso da sessão — quem está usando e em qual filial.
 *
 * POR QUE ISTO É UMA COISA SÓ, e não dois estados soltos: a filial não é um
 * filtro de tela, é a FRONTEIRA. O mesmo registro responde 200 numa filial e
 * 404 na outra, na leitura e na escrita (documento 23, seção 4.3). Trocar de
 * filial não é "filtrar menos linhas" — é passar a olhar outra base. Por isso
 * ela mora aqui, no alto, e não dentro de cada listagem.
 *
 * ISTO NÃO AUTENTICA NINGUÉM. O usuário e a filial viajam em cabeçalho HTTP, que
 * qualquer um escreve; é o andaime provisório da seção 4.1 do documento 23,
 * declarado como dívida D-1. Quando o Entra ID entrar, o valor passa a sair do
 * token e só `dados/api/http.ts` muda.
 */

import { createContext, use, useCallback, useMemo, useState, type ReactNode } from 'react';
import type { ContextoDeAcesso } from './http';

/** A filial que a API usa como padrão em Desenvolvimento (`appsettings.Development.json`). */
const PADRAO: ContextoDeAcesso = {
  usuario: 'cen.ribeiraopreto@tracbel.com.br',
  empresa: '010101',
};

const CHAVE_GUARDADA = 'tracbel-crm:contexto-acesso';

type ValorDoContexto = {
  contexto: ContextoDeAcesso;
  /** Troca a filial corrente. É o que o seletor do cabeçalho chama. */
  trocarEmpresa: (codigo: string) => void;
};

const Contexto = createContext<ValorDoContexto | null>(null);

function lerDoNavegador(): ContextoDeAcesso {
  try {
    const guardado = localStorage.getItem(CHAVE_GUARDADA);
    if (!guardado) return PADRAO;
    const lido = JSON.parse(guardado) as Partial<ContextoDeAcesso>;
    return {
      usuario: lido.usuario?.trim() || PADRAO.usuario,
      empresa: lido.empresa?.trim() || PADRAO.empresa,
    };
  } catch {
    // Navegação anônima, armazenamento bloqueado, valor corrompido — em todos os
    // casos o padrão serve. Guardar a filial é conveniência, não estado de negócio.
    return PADRAO;
  }
}

/** Envolve a aplicação inteira: toda chamada à API lê a filial daqui. */
export function ProvedorDeContextoDeAcesso({ children }: { children: ReactNode }) {
  const [contexto, setContexto] = useState<ContextoDeAcesso>(lerDoNavegador);

  const trocarEmpresa = useCallback((codigo: string) => {
    setContexto((atual) => {
      const novo = { ...atual, empresa: codigo };
      try {
        localStorage.setItem(CHAVE_GUARDADA, JSON.stringify(novo));
      } catch {
        // A troca vale para esta sessão mesmo sem conseguir guardar.
      }
      return novo;
    });
  }, []);

  const valor = useMemo(() => ({ contexto, trocarEmpresa }), [contexto, trocarEmpresa]);

  return <Contexto value={valor}>{children}</Contexto>;
}

/**
 * O contexto de acesso corrente.
 *
 * LANÇA quando usado fora do provedor, em vez de devolver um padrão silencioso:
 * um contexto vazio faria as consultas filtrarem por uma filial que não existe e
 * devolverem nada, e alguém gastaria horas procurando o dado que "sumiu". É a
 * mesma escolha que `ContextoAcessoDaRequisicao` faz no servidor.
 */
export function useContextoDeAcesso(): ValorDoContexto {
  const valor = use(Contexto);
  if (!valor) {
    throw new Error(
      'useContextoDeAcesso foi chamado fora de <ProvedorDeContextoDeAcesso>. ' +
        'O provedor precisa envolver o roteador — confira App.tsx.',
    );
  }
  return valor;
}
