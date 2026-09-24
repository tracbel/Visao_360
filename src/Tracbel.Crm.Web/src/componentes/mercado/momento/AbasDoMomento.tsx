/**
 * AS ABAS SUBLINHADAS DO MOMENTO DO MERCADO (fidelidade às maquetes, 23/09/2026).
 *
 * POR QUE NÃO O `AbasInternas`. Aquele é o alternador SEGMENTADO dos mapas e do
 * Potencial estrutural — botões colados com `aria-pressed` —, e é o que eles
 * pedem: trocar um recorte dentro de um cartão. O Momento é outra coisa: cinco
 * leituras inteiras, cada uma com cartões, painéis e tabela. A maquete as
 * desenha como ABAS de verdade — texto, a ativa em verde com o sublinhado —, e
 * a semântica acompanha: `tablist`, `tab` e `tabpanel`. Mudar o `AbasInternas`
 * mudaria os outros blocos que o usam.
 *
 * O TECLADO É O DE UM `tablist`: só a aba ativa entra na ordem do Tab; as setas
 * andam entre elas (e dão a volta), Home e End vão às pontas. Como na barra da
 * página, a seta já ATIVA a aba — são cinco leituras leves, e ativação
 * automática é o que o leitor de tela anuncia sem passo extra.
 *
 * OS IDs SÃO GERADOS (`useId`): a página já tem `aba-mercado` e
 * `painel-mercado`, e um id repetido faria o `aria-controls` apontar para o
 * lugar errado.
 */

import { useId, useRef, type ReactNode } from 'react';

export type AbaDoMomento<T extends string> = {
  id: T;
  rotulo: string;
};

export function AbasDoMomento<T extends string>({
  abas,
  ativa,
  aoTrocar,
  rotulo,
  children,
}: {
  abas: readonly AbaDoMomento<T>[];
  ativa: T;
  aoTrocar: (id: T) => void;
  /** O que o leitor de tela anuncia ao entrar na barra. */
  rotulo: string;
  /** O conteúdo da aba ativa — só ela é montada. */
  children: ReactNode;
}) {
  const base = useId();
  const barra = useRef<HTMLDivElement>(null);
  const idDaAba = (id: T) => `${base}-aba-${id}`;
  const idDoPainel = (id: T) => `${base}-painel-${id}`;

  function andar(evento: React.KeyboardEvent) {
    const atual = abas.findIndex((a) => a.id === ativa);
    let destino: number | null = null;
    if (evento.key === 'ArrowRight') destino = (atual + 1) % abas.length;
    else if (evento.key === 'ArrowLeft') destino = (atual - 1 + abas.length) % abas.length;
    else if (evento.key === 'Home') destino = 0;
    else if (evento.key === 'End') destino = abas.length - 1;
    if (destino === null) return;

    evento.preventDefault();
    const proxima = abas[destino];
    aoTrocar(proxima.id);
    barra.current?.querySelector<HTMLButtonElement>(`[data-aba-do-momento="${proxima.id}"]`)?.focus();
  }

  return (
    <>
      <div className="mom-abas" role="tablist" aria-label={rotulo} ref={barra} onKeyDown={andar}>
        {abas.map((aba) => (
          <button
            key={aba.id}
            id={idDaAba(aba.id)}
            type="button"
            role="tab"
            className="mom-aba"
            data-aba-do-momento={aba.id}
            aria-selected={aba.id === ativa}
            // SÓ A ATIVA APONTA PARA O PAINEL: só o dela existe (cada aba faz a
            // própria leitura e não monta escondida), e um `aria-controls` para
            // um id ausente é um controle que não controla nada.
            aria-controls={aba.id === ativa ? idDoPainel(aba.id) : undefined}
            tabIndex={aba.id === ativa ? 0 : -1}
            onClick={() => aoTrocar(aba.id)}
          >
            {aba.rotulo}
          </button>
        ))}
      </div>
      <div
        id={idDoPainel(ativa)}
        className="mom-painel-da-aba"
        role="tabpanel"
        aria-labelledby={idDaAba(ativa)}
        data-aba-do-momento-ativa={ativa}
      >
        {children}
      </div>
    </>
  );
}
