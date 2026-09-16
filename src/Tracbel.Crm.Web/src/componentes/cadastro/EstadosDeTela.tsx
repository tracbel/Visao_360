/**
 * Os três estados que toda tela ligada à API precisa saber mostrar.
 *
 * POR QUE SÃO TRÊS COMPONENTES E NÃO UM `if` EM CADA TELA: carregando, vazio e
 * erro pedem AÇÕES DIFERENTES do usuário — esperar, criar o primeiro registro,
 * ou tentar de novo. Quando cada tela escreve o seu, umas ganham o botão de
 * tentar de novo e outras não, e a mais parecida com "sem resultado" acaba sendo
 * a que falhou. Aqui a diferença está no tipo, não na disposição de quem escreveu
 * a tela (documento 05-MELHORIAS-COMBINADAS §3).
 */

import type { ReactNode } from 'react';
import { ErroDaApi, ErroDeRede } from '../../dados/api/http';

/** Está buscando. O texto diz O QUE está buscando, não só "carregando". */
export function BlocoCarregando({ oQue }: { oQue: string }) {
  return (
    <div className="cad-estado" role="status" aria-live="polite">
      <span className="cad-girando" aria-hidden="true" />
      <div className="cad-estado-titulo">Carregando {oQue}…</div>
      <div className="cad-estado-texto">Buscando na API do CRM.</div>
    </div>
  );
}

/**
 * Não há o que mostrar. SEMPRE COM A AÇÃO QUE FAZ SENTIDO ALI: lista vazia de
 * verdade pede "cadastrar o primeiro"; busca sem resultado pede "limpar o
 * filtro". São situações diferentes e o botão precisa ser diferente.
 */
export function BlocoVazio({
  titulo,
  texto,
  acao,
}: {
  titulo: string;
  texto: ReactNode;
  acao?: ReactNode;
}) {
  return (
    <div className="cad-estado">
      <svg viewBox="0 0 24 24" width="32" height="32" fill="none" stroke="currentColor" strokeWidth={1.5} aria-hidden="true">
        <rect x="3" y="3" width="18" height="18" rx="2" />
        <line x1="3" y1="9" x2="21" y2="9" />
        <line x1="9" y1="21" x2="9" y2="9" />
      </svg>
      <div className="cad-estado-titulo">{titulo}</div>
      <div className="cad-estado-texto">{texto}</div>
      {acao && <div className="cad-estado-acao">{acao}</div>}
    </div>
  );
}

/**
 * Traduz a falha no que fazer a respeito.
 *
 * O TEXTO É A PARTE ÚTIL. "Erro ao carregar" não ajuda ninguém; "a API não
 * respondeu, confira se ela está no ar" e "esta filial não enxerga este
 * registro" levam a uma ação. É a diferença medida contra o legado, que devolve
 * `{"Message":"An error has occurred."}` para tudo.
 */
export function BlocoErro({ erro, aoTentarDeNovo }: { erro: Error; aoTentarDeNovo?: () => void }) {
  const { titulo, texto } = explicar(erro);

  return (
    <div className="cad-estado cad-estado-erro" role="alert">
      <svg viewBox="0 0 24 24" width="32" height="32" fill="none" stroke="currentColor" strokeWidth={1.8} aria-hidden="true">
        <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
        <path d="M12 9v4" />
        <path d="M12 17h.01" />
      </svg>
      <div className="cad-estado-titulo">{titulo}</div>
      <div className="cad-estado-texto">{texto}</div>
      {aoTentarDeNovo && (
        <div className="cad-estado-acao">
          <button type="button" className="btn btn-secondary" onClick={aoTentarDeNovo}>
            Tentar de novo
          </button>
        </div>
      )}
    </div>
  );
}

/** A frase que cada natureza de falha merece. */
function explicar(erro: Error): { titulo: string; texto: string } {
  if (erro instanceof ErroDeRede) {
    return {
      titulo: 'A API do CRM não respondeu',
      texto: erro.message,
    };
  }

  if (erro instanceof ErroDaApi) {
    if (erro.ehNaoEncontrado) {
      return {
        titulo: 'Este registro não está ao alcance desta filial',
        texto:
          'Ele pode não existir, ou pertencer a outra filial. Troque a filial no cabeçalho ' +
          'para conferir — a API não distingue os dois casos de propósito, para não contar ' +
          'a quem não pode ver que o registro existe.',
      };
    }
    // FALTA DE PERMISSÃO NÃO É FALHA. O dado existe e a API recusou mostrar a este perfil — tentar de novo
    // não muda nada; quem resolve é a concessão de acesso.
    if (erro.status === 403) {
      return {
        titulo: 'Sem permissão para esta consulta',
        texto: [erro.message, erro.detalhe].filter(Boolean).join(' — '),
      };
    }
    if (erro.status === 401) {
      return {
        titulo: 'A sessão expirou',
        texto: 'Entre de novo com a conta Microsoft para continuar. Nenhum dado foi alterado.',
      };
    }
    if (erro.status === 503) {
      return {
        titulo: 'Uma dependência externa não respondeu',
        texto: erro.message,
      };
    }
    return { titulo: erro.message, texto: erro.detalhe ?? 'A API recusou a requisição.' };
  }

  return { titulo: 'Não foi possível carregar', texto: erro.message };
}
