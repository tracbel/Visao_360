/**
 * Ranking detalhado FYTD (tabela) — porte de `renderPerfRanking`
 * (prototipo/referencia/assets/app.js linha 6396 / perf-block.js linha 474).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a ordenação passou a existir.
 *
 * O cartão dizia, em letras miúdas, *"Clique no cabeçalho para ordenar"*, e os
 * cabeçalhos eram `<th>` sem ação nenhuma — o texto prometia um comportamento
 * que a tabela não tinha. Como o dado inteiro já está na memória, ordenar é
 * barato e a promessa vira verdade em vez de sair da tela.
 *
 * A forma segue a seção 8 do padrão de tela: o cabeçalho clicável é um
 * `<button>` dentro do `<th>`, não um `<th>` com `onClick` — só o `<button>`
 * chega pelo teclado e é anunciado como acionável —, e a coluna ativa leva
 * `aria-sort`.
 *
 * **A coluna do rank não ordena, e não é esquecimento.** O `#` é a posição em
 * vendas FYTD; ordenar por ele é o mesmo que ordenar por "Vendas FYTD", que já
 * é a ordem inicial. O número da posição continua sendo o do ranking de vendas
 * mesmo quando a tabela está ordenada por outra coluna — senão "1º lugar"
 * mudaria de significado a cada clique.
 */
import { useState } from 'react';
import { classAtingimento } from '../../dados/performanceCen';
import type { PerfDetalheCen } from '../../tipos/performanceCen';
import { fmtBRLcompact } from './formatoPerformance';

function classeCobertura(cobertura: number): string {
  if (cobertura >= 80) return 'val-ok';
  if (cobertura >= 65) return 'val-warn';
  return 'val-danger';
}

function classeRank(rank: number): string {
  if (rank === 1) return 'rank-1';
  if (rank === 2) return 'rank-2';
  if (rank === 3) return 'rank-3';
  return '';
}

/** As colunas que ordenam, e por qual valor de cada linha. */
type ChaveOrdem =
  | 'cen'
  | 'regional'
  | 'mes_vendas'
  | 'mes_atingimento'
  | 'fytd_vendas'
  | 'fytd_atingimento'
  | 'pipeline_aberto'
  | 'conversao_fytd'
  | 'ticket_medio_fytd'
  | 'cobertura';

type Coluna = {
  chave: ChaveOrdem;
  rotulo: string;
  /** Texto ordena por comparação de string; número, por valor. */
  texto?: boolean;
  direita?: boolean;
};

const COLUNAS: Coluna[] = [
  { chave: 'cen', rotulo: 'CEN', texto: true },
  { chave: 'regional', rotulo: 'Regional', texto: true },
  { chave: 'mes_vendas', rotulo: 'Vendas Ago/26', direita: true },
  { chave: 'mes_atingimento', rotulo: '% Meta mês', direita: true },
  { chave: 'fytd_vendas', rotulo: 'Vendas FYTD', direita: true },
  { chave: 'fytd_atingimento', rotulo: '% Meta FYTD', direita: true },
  { chave: 'pipeline_aberto', rotulo: 'Pipeline', direita: true },
  { chave: 'conversao_fytd', rotulo: 'Conversão', direita: true },
  { chave: 'ticket_medio_fytd', rotulo: 'Ticket médio', direita: true },
  { chave: 'cobertura', rotulo: 'Cobertura', direita: true },
];

function valorDe(p: PerfDetalheCen, chave: ChaveOrdem): string | number {
  if (chave === 'cen') return p.cen.nome;
  if (chave === 'regional') return p.cen.regional;
  return p[chave];
}

export function PerfRanking({ detalhes }: { detalhes: PerfDetalheCen[] }) {
  // A ordem inicial é a do ranking: maior venda FYTD primeiro.
  const [ordem, setOrdem] = useState<{ chave: ChaveOrdem; descendente: boolean }>({
    chave: 'fytd_vendas',
    descendente: true,
  });

  // A posição no ranking é sempre por vendas FYTD, independente da ordem da
  // tabela — é o que o número "1" significa.
  const posicao = new Map(
    [...detalhes].sort((a, b) => b.fytd_vendas - a.fytd_vendas).map((p, i) => [p.cen.id, i + 1]),
  );

  const linhas = [...detalhes].sort((a, b) => {
    const va = valorDe(a, ordem.chave);
    const vb = valorDe(b, ordem.chave);
    const comparacao =
      typeof va === 'string' && typeof vb === 'string' ? va.localeCompare(vb, 'pt-BR') : Number(va) - Number(vb);
    return ordem.descendente ? -comparacao : comparacao;
  });

  function trocarOrdem(chave: ChaveOrdem) {
    setOrdem((atual) =>
      atual.chave === chave
        ? { chave, descendente: !atual.descendente }
        : // Texto começa de A para Z; número começa do maior, que é o que se
          // quer ver primeiro num ranking.
          { chave, descendente: !COLUNAS.find((c) => c.chave === chave)?.texto },
    );
  }

  return (
    <div id="perfRankingTable">
      <table className="perf-tabela">
        <thead>
          <tr>
            <th>#</th>
            {COLUNAS.map((coluna) => (
              <th
                key={coluna.chave}
                scope="col"
                style={coluna.direita ? { textAlign: 'right' } : undefined}
                aria-sort={
                  ordem.chave === coluna.chave ? (ordem.descendente ? 'descending' : 'ascending') : undefined
                }
              >
                <button type="button" className="perf-th-ordenar" onClick={() => trocarOrdem(coluna.chave)}>
                  {coluna.rotulo}
                  <span aria-hidden="true">
                    {ordem.chave === coluna.chave ? (ordem.descendente ? ' ▾' : ' ▴') : ''}
                  </span>
                </button>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {linhas.map((p) => {
            const rank = posicao.get(p.cen.id) ?? 0;
            return (
              <tr key={p.cen.id}>
                <td>
                  <span className={`perf-rank ${classeRank(rank)}`}>{rank}</span>
                </td>
                <td>
                  <div className="perf-cen-cell">
                    <div className="perf-cen-avatar" style={{ background: p.cen.avatar }}>
                      {p.cen.foto}
                    </div>
                    <div>
                      <div className="perf-cen-nome">{p.cen.nome}</div>
                      <div className="perf-cen-sub">desde {new Date(p.cen.admissao).getFullYear()}</div>
                    </div>
                  </div>
                </td>
                <td>{p.cen.regional}</td>
                <td className="mono num">{fmtBRLcompact(p.mes_vendas)}</td>
                <td className={`mono ${classAtingimento(p.mes_atingimento)}`}>{(p.mes_atingimento * 100).toFixed(0)}%</td>
                <td className="mono num">
                  <strong>{fmtBRLcompact(p.fytd_vendas)}</strong>
                </td>
                <td className={`mono ${classAtingimento(p.fytd_atingimento)}`}>{(p.fytd_atingimento * 100).toFixed(0)}%</td>
                <td className="mono num">{fmtBRLcompact(p.pipeline_aberto)}</td>
                <td className="mono num">{(p.conversao_fytd * 100).toFixed(1)}%</td>
                <td className="mono num">{fmtBRLcompact(p.ticket_medio_fytd)}</td>
                <td className={`mono ${classeCobertura(p.cobertura)}`}>{p.cobertura}%</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
