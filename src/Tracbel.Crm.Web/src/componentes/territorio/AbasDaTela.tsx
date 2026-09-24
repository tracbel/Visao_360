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

import type { ComponentType, ReactNode } from 'react';
import { useRef } from 'react';

export type Aba<T extends string> = {
  id: T;
  rotulo: string;
  /**
   * O ícone à esquerda do rótulo (maquete, fase T4.9).
   *
   * É `aria-hidden` — o nome da aba já está escrito ao lado, e repeti-lo no
   * leitor de tela só faria barulho. Ele existe para o olho achar a aba de
   * longe, que é o que a maquete pede.
   */
  icone?: ComponentType<{ size?: number | string; strokeWidth?: number | string }>;
};

export function AbasDaTela<T extends string>({
  abas,
  ativa,
  aoTrocar,
  rotulo,
  acessorio,
  children,
}: {
  abas: readonly Aba<T>[];
  ativa: T;
  aoTrocar: (id: T) => void;
  /** O que o leitor de tela anuncia ao entrar na barra. */
  rotulo: string;
  /**
   * Um controle à direita, NA LINHA DAS ABAS (fidelidade às maquetes,
   * 23/09/2026) — hoje, o "Comparar com período anterior" da aba Mercado, que
   * é onde a maquete o põe.
   *
   * Ele fica FORA do `tablist`: dentro, o leitor de tela o anunciaria como mais
   * uma aba, e as setas passariam por ele.
   */
  acessorio?: ReactNode;
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
      <div className="terr-abas-barra">
        <div className="terr-abas" role="tablist" aria-label={rotulo} ref={barra} onKeyDown={andarComAsSetas} data-bloco="abas">
          {abas.map((aba) => (
            <button
              key={aba.id}
              id={`aba-${aba.id}`}
              type="button"
              role="tab"
              className="terr-aba"
              aria-selected={aba.id === ativa}
              // SÓ A ATIVA APONTA PARA O PAINEL: o da outra aba não está no
              // documento, e um `aria-controls` para um id ausente é um
              // controle que não controla nada.
              aria-controls={aba.id === ativa ? `painel-${aba.id}` : undefined}
              tabIndex={aba.id === ativa ? 0 : -1}
              onClick={() => aoTrocar(aba.id)}
            >
              {aba.icone && <aba.icone size={16} strokeWidth={2} aria-hidden="true" />}
              {aba.rotulo}
            </button>
          ))}
        </div>
        {acessorio && <div className="terr-abas-acessorio">{acessorio}</div>}
      </div>
      {/* O PAINEL TEM O RITMO VERTICAL DA ABA (fidelidade às maquetes): os
          blocos de dentro eram irmãos sem espaço entre eles — a régua do
          mercado encostava nos cartões e o título dos mapas, na régua. */}
      <div
        id={`painel-${ativa}`}
        className="terr-aba-painel"
        role="tabpanel"
        aria-labelledby={`aba-${ativa}`}
        data-aba={ativa}
      >
        {children}
      </div>
    </>
  );
}
