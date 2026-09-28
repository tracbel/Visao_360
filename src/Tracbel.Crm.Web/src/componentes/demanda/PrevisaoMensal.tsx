/**
 * A PREVISÃO MENSAL — o que a Tracbel tem de entregar em cada mês do ano fiscal, pela sazonalidade (issue 258).
 *
 * Barras em HTML, e não um gráfico de biblioteca: são doze números com o rótulo em cima, e cada barra é um botão que
 * escolhe o mês — o mesmo que o campo "Mês" dos filtros. A altura é relativa ao maior mês, e o número vai escrito:
 * ninguém precisa medir barra para ler.
 */

import type { PrevisaoDoMes } from '../../tipos/mercado';
import { n, NOME_DO_MES, NOME_DO_MES_POR_EXTENSO } from './demanda';

export function PrevisaoMensal({
  meses,
  ajustada,
  mesEscolhido,
  aoEscolherMes,
}: {
  meses: PrevisaoDoMes[];
  /** Mostra a entrega pela demanda ajustada; sem ela, pela estrutural. */
  ajustada: boolean;
  mesEscolhido: number | null;
  aoEscolherMes: (mes: number | null) => void;
}) {
  const valor = (m: PrevisaoDoMes) => (ajustada ? m.aEntregarAjustada : m.aEntregar) ?? null;
  const maior = Math.max(0, ...meses.map((m) => valor(m) ?? 0));

  return (
    <div className="dem-previsao" role="group" aria-label="Escolher o mês da previsão">
      {meses.map((m) => {
        const v = valor(m);
        const escolhido = mesEscolhido === m.mes;
        return (
          <button
            key={m.mes}
            type="button"
            className="dem-previsao-mes"
            aria-pressed={escolhido}
            aria-label={`${NOME_DO_MES_POR_EXTENSO[m.mes]}: ${v === null ? 'sem dado' : `${n(v)} máquinas a entregar`}, ${n(m.fracao * 100)}% do ano`}
            onClick={() => aoEscolherMes(escolhido ? null : m.mes)}
            data-apagado={mesEscolhido !== null && !escolhido ? 'true' : undefined}
          >
            <span className="dem-previsao-valor">{v === null ? '—' : n(v)}</span>
            <span className="dem-previsao-trilho" aria-hidden="true">
              <span style={{ height: `${maior > 0 && v !== null ? (100 * v) / maior : 0}%` }} />
            </span>
            <span className="dem-previsao-nome">{NOME_DO_MES[m.mes]}</span>
            <span className="dem-previsao-fracao">{n(m.fracao * 100)}%</span>
          </button>
        );
      })}
    </div>
  );
}
