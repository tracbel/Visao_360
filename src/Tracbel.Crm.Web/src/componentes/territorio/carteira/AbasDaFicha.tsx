/**
 * AS ABAS DA FICHA DO MUNICÍPIO — Visão geral · Lavoura · Estrutura ·
 * Oportunidades · Histórico (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * O TECLADO É O DE UM `tablist`, como nas abas da página: só a aba ativa entra
 * na ordem do Tab, as setas andam entre elas (e dão a volta), Home e End vão às
 * pontas. A troca é imediata ao andar, porque os painéis já estão montados — não
 * há leitura nova a esperar.
 *
 * OS CINCO PAINÉIS ESTÃO SEMPRE NO DOCUMENTO, os inativos com `hidden` — o
 * `aria-controls` de cada aba aponta para algo que existe. O CONTEÚDO de um
 * painel monta na primeira vez que a aba abre, e fica: a ficha não paga as
 * quarenta medidas da Lavoura e da Estrutura a cada troca de município só para
 * mostrar a Visão geral, e um `<details>` aberto na aba Lavoura continua aberto
 * quando se volta a ela.
 */

import { useRef, type KeyboardEvent, type ReactNode } from 'react';

const ABAS_DA_FICHA = [
  { id: 'visao-geral', rotulo: 'Visão geral' },
  { id: 'lavoura', rotulo: 'Lavoura' },
  { id: 'estrutura', rotulo: 'Estrutura' },
  { id: 'oportunidades', rotulo: 'Oportunidades' },
  { id: 'historico', rotulo: 'Histórico' },
] as const;

export type AbaDaFicha = (typeof ABAS_DA_FICHA)[number]['id'];

export function AbasDaFicha({
  ativa,
  aoTrocar,
  nomeDoMunicipio,
}: {
  ativa: AbaDaFicha;
  aoTrocar: (aba: AbaDaFicha) => void;
  nomeDoMunicipio: string;
}) {
  const barra = useRef<HTMLDivElement>(null);

  function andar(evento: KeyboardEvent) {
    const atual = ABAS_DA_FICHA.findIndex((a) => a.id === ativa);
    const ultima = ABAS_DA_FICHA.length - 1;
    const destino =
      evento.key === 'ArrowRight'
        ? (atual + 1) % ABAS_DA_FICHA.length
        : evento.key === 'ArrowLeft'
          ? (atual - 1 + ABAS_DA_FICHA.length) % ABAS_DA_FICHA.length
          : evento.key === 'Home'
            ? 0
            : evento.key === 'End'
              ? ultima
              : null;
    if (destino === null) return;
    evento.preventDefault();
    const proxima = ABAS_DA_FICHA[destino].id;
    aoTrocar(proxima);
    barra.current?.querySelector<HTMLButtonElement>(`#ficha-aba-${proxima}`)?.focus();
  }

  return (
    <div className="terr-ficha-abas" role="tablist" aria-label={`Leituras de ${nomeDoMunicipio}`} ref={barra} onKeyDown={andar}>
      {ABAS_DA_FICHA.map((a) => (
        <button
          key={a.id}
          id={`ficha-aba-${a.id}`}
          type="button"
          role="tab"
          className="terr-ficha-aba"
          aria-selected={a.id === ativa}
          aria-controls={`ficha-painel-${a.id}`}
          tabIndex={a.id === ativa ? 0 : -1}
          onClick={() => aoTrocar(a.id)}
        >
          {a.rotulo}
        </button>
      ))}
    </div>
  );
}

export function PainelDaFicha({
  id,
  ativa,
  montado,
  children,
}: {
  id: AbaDaFicha;
  ativa: AbaDaFicha;
  /** Se a aba já foi aberta alguma vez — o conteúdo só monta a partir daí. */
  montado: boolean;
  children: ReactNode;
}) {
  return (
    <div
      id={`ficha-painel-${id}`}
      role="tabpanel"
      aria-labelledby={`ficha-aba-${id}`}
      className="terr-ficha-painel"
      data-aba-da-ficha={id}
      hidden={id !== ativa}
      // O painel recebe foco: é o próximo passo do Tab depois da aba, como o
      // padrão de abas da WAI-ARIA pede quando o painel começa por texto.
      tabIndex={0}
    >
      {montado ? children : null}
    </div>
  );
}
