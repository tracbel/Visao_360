/** O cabeçalho de coluna que ordena a tabela — o mesmo botão e o mesmo `aria-sort` da matriz da Demanda. */

import type { Ordem } from './ordenacao';

export function CabecalhoOrdenavel<C extends string>({
  coluna,
  rotulo,
  texto = false,
  ordem,
  aoOrdenar,
  titulo,
}: {
  coluna: C;
  rotulo: string;
  /** Coluna de texto: alinha à esquerda e começa de A a Z. */
  texto?: boolean;
  ordem: Ordem<C>;
  aoOrdenar: (coluna: C, texto: boolean) => void;
  /** O nome completo da coluna, quando o rótulo é curto. */
  titulo?: string;
}) {
  return (
    <th
      scope="col"
      className={texto ? undefined : 'mom-num'}
      title={titulo}
      aria-sort={ordem.coluna === coluna ? (ordem.sentido === 1 ? 'ascending' : 'descending') : 'none'}
    >
      <button type="button" className="diag-ordenar" onClick={() => aoOrdenar(coluna, texto)}>
        {rotulo}
        {ordem.coluna === coluna && <span aria-hidden="true">{ordem.sentido === 1 ? ' ▲' : ' ▼'}</span>}
      </button>
    </th>
  );
}
