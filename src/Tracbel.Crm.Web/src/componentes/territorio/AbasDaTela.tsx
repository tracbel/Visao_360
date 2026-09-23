/**
 * As abas da página — Mercado × Território (fase T1 do documento 50).
 *
 * ELAS FICAM ABAIXO DOS FILTROS E DO MUNICÍPIO ESCOLHIDO, e é isso que faz as
 * duas serem leituras do MESMO recorte: trocar de aba mantém o filtro e o
 * município. Se os filtros vivessem dentro de uma aba, a outra seria outra
 * página, com outro recorte, e a comparação entre elas deixaria de valer.
 *
 * O TECLADO ANDA COM AS SETAS, que é o que se espera de um `tablist`: só a aba
 * ativa entra na ordem do Tab, e as setas trocam entre elas.
 */

import type { ReactNode } from 'react';
import { useRef } from 'react';

export type Aba<T extends string> = { id: T; rotulo: string };

export function AbasDaTela<T extends string>({
  abas,
  ativa,
  aoTrocar,
  rotulo,
  children,
}: {
  abas: readonly Aba<T>[];
  ativa: T;
  aoTrocar: (id: T) => void;
  /** O que o leitor de tela anuncia ao entrar na barra. */
  rotulo: string;
  /** O conteúdo da aba ativa. */
  children: ReactNode;
}) {
  const barra = useRef<HTMLDivElement>(null);

  function andarComAsSetas(evento: React.KeyboardEvent) {
    const passo = evento.key === 'ArrowRight' ? 1 : evento.key === 'ArrowLeft' ? -1 : 0;
    if (passo === 0) return;
    evento.preventDefault();
    const atual = abas.findIndex((a) => a.id === ativa);
    const proxima = abas[(atual + passo + abas.length) % abas.length];
    aoTrocar(proxima.id);
    barra.current?.querySelector<HTMLButtonElement>(`#aba-${proxima.id}`)?.focus();
  }

  return (
    <>
      <div className="terr-abas" role="tablist" aria-label={rotulo} ref={barra} onKeyDown={andarComAsSetas} data-bloco="abas">
        {abas.map((aba) => (
          <button
            key={aba.id}
            id={`aba-${aba.id}`}
            type="button"
            role="tab"
            className="terr-aba"
            aria-selected={aba.id === ativa}
            aria-controls={`painel-${aba.id}`}
            tabIndex={aba.id === ativa ? 0 : -1}
            onClick={() => aoTrocar(aba.id)}
          >
            {aba.rotulo}
          </button>
        ))}
      </div>
      <div id={`painel-${ativa}`} role="tabpanel" aria-labelledby={`aba-${ativa}`} data-aba={ativa}>
        {children}
      </div>
    </>
  );
}
