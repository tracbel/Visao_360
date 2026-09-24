/**
 * A PAGINAÇÃO DA TABELA DE MUNICÍPIOS (fidelidade às maquetes, 23/09/2026 —
 * fase 4): "Mostrando 1–10 de 203 municípios", "10 por página" e
 * "‹ 1 2 3 4 5 … 21 ›".
 *
 * POR QUE NÃO A `BarraDePaginacao` DO CADASTRO. Aquela é a paginação da API —
 * tamanhos de 25 a 100, "Anterior / Próxima" e o total que o servidor devolve —,
 * e o desenho dela é outro. Aqui a lista inteira já chegou (a resposta traz a ADR
 * toda), a página é só uma janela sobre ela, e a maquete pede números de página.
 *
 * ANTES NÃO HAVIA PÁGINA: a tabela rolava dentro de um cartão de 520px de
 * altura. Rolagem dentro de rolagem esconde o fim da lista e prende a roda do
 * mouse; dez linhas por vez, com a página escrita, dizem onde se está.
 */

import { ChevronDown, ChevronLeft, ChevronRight } from 'lucide-react';
import { nº } from '../indicadoresDaAdr';

/** Os tamanhos da maquete. Cinquenta já é a ADR de uma sub-região inteira. */
const TAMANHOS = [10, 25, 50] as const;

/**
 * Os números que aparecem: todos até sete páginas; acima disso a primeira, a
 * última e a vizinhança da atual, com reticências no que sobra — "1 2 3 4 5 … 21".
 */
function paginasVisiveis(atual: number, total: number): (number | 'antes' | 'depois')[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  if (atual <= 4) return [1, 2, 3, 4, 5, 'depois', total];
  if (atual >= total - 3) return [1, 'antes', total - 4, total - 3, total - 2, total - 1, total];
  return [1, 'antes', atual - 1, atual, atual + 1, 'depois', total];
}

export function PaginacaoDaTabela({
  total,
  pagina,
  tamanho,
  oQue,
  deQuantos,
  aoTrocarPagina,
  aoTrocarTamanho,
}: {
  /** Quantas linhas a lista tem depois da busca. */
  total: number;
  pagina: number;
  tamanho: number;
  /** O que se lista, no plural: "municípios". */
  oQue: string;
  /** Quantas havia antes da busca — quando difere, a faixa diz de onde veio. */
  deQuantos: number;
  aoTrocarPagina: (pagina: number) => void;
  aoTrocarTamanho: (tamanho: number) => void;
}) {
  const totalDePaginas = Math.max(1, Math.ceil(total / tamanho));
  const primeiro = total === 0 ? 0 : (pagina - 1) * tamanho + 1;
  const ultimo = Math.min(pagina * tamanho, total);

  return (
    <div className="terr-paginacao" data-bloco="paginacao-municipios">
      {/* A FAIXA É ANUNCIADA: trocar de página, buscar ou reordenar muda o que
          está na tela, e quem usa leitor de tela precisa ouvir onde foi parar. */}
      <p className="terr-paginacao-faixa" aria-live="polite">
        {total === 0 ? (
          <>Nenhum município na lista</>
        ) : (
          <>
            Mostrando {nº(primeiro)}–{nº(ultimo)} de {nº(total)} {oQue}
            {/* O QUE A BUSCA FEZ, ESCRITO: sem isto, uma lista filtrada seria
                indistinguível de uma ADR que encolheu. */}
            {total !== deQuantos && <> encontrados, de {nº(deQuantos)}</>}
          </>
        )}
      </p>

      <div className="terr-paginacao-controles">
        <label className="terr-paginacao-tamanho">
          <select value={tamanho} onChange={(e) => aoTrocarTamanho(Number(e.target.value))} aria-label="Municípios por página">
            {TAMANHOS.map((t) => (
              <option key={t} value={t}>
                {t} por página
              </option>
            ))}
          </select>
          <ChevronDown size={14} strokeWidth={2.2} aria-hidden="true" />
        </label>

        <nav className="terr-paginacao-paginas" aria-label="Páginas da tabela de municípios">
          <button
            type="button"
            className="terr-pagina terr-pagina-seta"
            onClick={() => aoTrocarPagina(pagina - 1)}
            disabled={pagina <= 1}
            aria-label="Página anterior"
          >
            <ChevronLeft size={15} strokeWidth={2.2} aria-hidden="true" />
          </button>
          {paginasVisiveis(pagina, totalDePaginas).map((p) =>
            typeof p === 'number' ? (
              <button
                key={p}
                type="button"
                className="terr-pagina"
                onClick={() => aoTrocarPagina(p)}
                aria-current={p === pagina ? 'page' : undefined}
                aria-label={`Página ${p}`}
              >
                {p}
              </button>
            ) : (
              <span key={p} className="terr-pagina-reticencias" aria-hidden="true">
                …
              </span>
            ),
          )}
          <button
            type="button"
            className="terr-pagina terr-pagina-seta"
            onClick={() => aoTrocarPagina(pagina + 1)}
            disabled={pagina >= totalDePaginas}
            aria-label="Próxima página"
          >
            <ChevronRight size={15} strokeWidth={2.2} aria-hidden="true" />
          </button>
        </nav>
      </div>
    </div>
  );
}
