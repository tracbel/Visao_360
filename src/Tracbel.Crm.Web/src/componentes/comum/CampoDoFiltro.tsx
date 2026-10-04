/**
 * O FILTRO DO PADRÃO DAS TELAS (documento 54 §3.5): o ícone à esquerda, o rótulo em cima e o controle embaixo. Era o
 * mesmo JSX escrito à mão em dezessete telas; agora mora aqui, e a regra de arquitetura reprova a cópia nova.
 */

import type { LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';

export function CampoDoFiltro({
  icone: Icone,
  rotulo,
  bloco,
  cor,
  dica,
  classe,
  tamanhoDoIcone = 17,
  espessura = 2,
  children,
}: {
  icone: LucideIcon;
  /** O texto em cima do controle; pode levar o ⓘ ao lado. */
  rotulo: ReactNode;
  /** O `data-bloco`, por onde o CSS da tela posiciona o campo. */
  bloco?: string;
  /** O `data-cor` do ícone, nas telas que colorem cada filtro. */
  cor?: string;
  /** A dica do campo inteiro (`title`). */
  dica?: string;
  /** Uma classe a mais, além de `dash-filtro`. */
  classe?: string;
  tamanhoDoIcone?: number;
  espessura?: number;
  /** O controle: `select`, `input` ou o que a tela precisar. */
  children: ReactNode;
}) {
  return (
    <label className={classe ? `dash-filtro ${classe}` : 'dash-filtro'} data-bloco={bloco} data-cor={cor} title={dica}>
      <span className="dash-filtro-icone" aria-hidden="true">
        <Icone size={tamanhoDoIcone} strokeWidth={espessura} />
      </span>
      <span className="dash-filtro-corpo">
        <span className="dash-filtro-rotulo">{rotulo}</span>
        {children}
      </span>
    </label>
  );
}
