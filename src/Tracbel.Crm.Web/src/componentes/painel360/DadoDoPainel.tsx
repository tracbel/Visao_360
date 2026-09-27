/**
 * Um par rótulo e valor dos blocos do 360 do cliente — `dt`/`dd` de verdade, dentro do `dl` `.p360-dados`: o leitor
 * de tela anuncia "rótulo, valor" em par, o que uma grade de `div` não faz.
 */

import type { ReactNode } from 'react';

export function Dado({ rotulo, valor, detalhe }: { rotulo: string; valor: ReactNode; detalhe?: ReactNode }) {
  return (
    <div className="p360-dado">
      <dt className="p360-dado-rotulo">{rotulo}</dt>
      <dd className="p360-dado-valor">
        {valor}
        {detalhe && <span className="p360-dado-detalhe">{detalhe}</span>}
      </dd>
    </div>
  );
}

/** Ausência escrita em palavra, e nunca um zero ou um traço solto. */
export function SemValor({ texto }: { texto: string }) {
  return <span className="cad-nada">{texto}</span>;
}
