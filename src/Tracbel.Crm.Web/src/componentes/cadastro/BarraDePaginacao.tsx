/**
 * A barra de páginas de uma listagem.
 *
 * MOSTRA A FAIXA E O TOTAL, e não só os botões: "26–50 de 312" diz onde a pessoa
 * está; "página 2" sozinho não diz. O total vem da API já respeitada a fronteira
 * de filial, então é o total que ESTA pessoa pode ver — nunca o da base inteira.
 *
 * DÍVIDA HERDADA E DECLARADA: a paginação da API é por `OFFSET`, não por cursor
 * (dívida D-3 do documento 23). Enquanto as duas tabelas desta fase forem
 * pequenas, a numeração de páginas é o que a tela aprovada pede; quando
 * `processo.Interacao` entrar, essa listagem nasce por cursor e esta barra não
 * serve para ela.
 */

import type { PaginaDe } from '../../tipos/api';

type Props<T> = {
  pagina: PaginaDe<T>;
  /** O nome do que está sendo listado, no plural. Ex.: `clientes`. */
  oQue: string;
  aoTrocarPagina: (pagina: number) => void;
  aoTrocarTamanho: (tamanho: number) => void;
};

/** Os tamanhos oferecidos. O teto da API é 200 e não é oferecido por engano. */
const TAMANHOS = [25, 50, 100];

export function BarraDePaginacao<T>({ pagina, oQue, aoTrocarPagina, aoTrocarTamanho }: Props<T>) {
  const primeiro = pagina.total === 0 ? 0 : (pagina.pagina - 1) * pagina.tamanho + 1;
  const ultimo = Math.min(pagina.pagina * pagina.tamanho, pagina.total);

  return (
    <div className="cad-paginacao">
      <div className="cad-paginacao-faixa" aria-live="polite">
        {pagina.total === 0 ? (
          <>Nenhum registro</>
        ) : (
          <>
            <strong>
              {primeiro}–{ultimo}
            </strong>{' '}
            de {pagina.total} {oQue}
          </>
        )}
      </div>

      <div className="cad-paginacao-controles">
        <label className="cad-paginacao-tamanho">
          Por página
          <select
            value={pagina.tamanho}
            onChange={(e) => aoTrocarTamanho(Number(e.target.value))}
            aria-label="Registros por página"
          >
            {TAMANHOS.map((t) => (
              <option key={t} value={t}>
                {t}
              </option>
            ))}
          </select>
        </label>

        <nav className="cad-paginacao-botoes" aria-label="Paginação">
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            disabled={pagina.pagina <= 1}
            onClick={() => aoTrocarPagina(pagina.pagina - 1)}
          >
            Anterior
          </button>
          <span className="cad-paginacao-atual">
            Página {pagina.pagina} de {Math.max(pagina.totalDePaginas, 1)}
          </span>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            disabled={!pagina.temProxima}
            onClick={() => aoTrocarPagina(pagina.pagina + 1)}
          >
            Próxima
          </button>
        </nav>
      </div>
    </div>
  );
}
