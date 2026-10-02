/**
 * A PREVISÃO MENSAL — a demanda prevista de cada mês do ano fiscal ao lado da entrega realizada, e o atendimento
 * (issue 258; desenho da maquete do Ricardo em 02/10/2026).
 *
 * TRÊS SÉRIES, COMO A MAQUETE: a barra escura é a demanda do mês pela sazonalidade (a base escolhida — ajustada ou
 * estrutural), a clara é a entrega do ART no mês (pela data da entrega), e a linha é o atendimento: entregue ÷ previsto.
 * O número vai escrito em cima de cada barra e de cada ponto: ninguém precisa medir barra para ler.
 *
 * O MÊS QUE AINDA NÃO COMEÇOU NÃO TEM ENTREGA — nem barra clara, nem ponto —, e sem ART a série clara some inteira, com o
 * motivo nas limitações. Cada mês é um botão que escolhe o mês, como antes.
 */

import type { KeyboardEvent } from 'react';
import type { PrevisaoDoMes } from '../../tipos/mercado';
import { n, NOME_DO_MES, NOME_DO_MES_POR_EXTENSO } from './demanda';

const LARGURA = 760;
const ALTURA = 262;
const MARGEM = { esquerda: 44, direita: 48, topo: 26, base: 26 };
/** A fração da altura útil que as barras ocupam; o atendimento corre na faixa de cima, como na maquete. */
const ALTURA_DAS_BARRAS = 0.74;

/** Um teto "redondo" acima do maior valor, e os quatro degraus até ele. */
function escala(maior: number): number[] {
  if (maior <= 0) return [0, 1, 2, 3, 4];
  const bruto = maior / 4;
  const potencia = 10 ** Math.floor(Math.log10(bruto));
  const passo = [1, 2, 2.5, 5, 10].map((f) => f * potencia).find((p) => p >= bruto) ?? 10 * potencia;
  return [0, 1, 2, 3, 4].map((i) => i * passo);
}

export function PrevisaoMensal({
  meses,
  base,
  mesEscolhido,
  aoEscolherMes,
}: {
  meses: PrevisaoDoMes[];
  /** A base da previsão: a demanda ajustada pelo momento ou a estrutural (o filtro "Meta / Previsão"). */
  base: 'ajustada' | 'estrutural';
  mesEscolhido: number | null;
  aoEscolherMes: (mes: number | null) => void;
}) {
  const prevista = (m: PrevisaoDoMes) => (base === 'ajustada' ? (m.demandaAjustada ?? m.demandaEstrutural) : m.demandaEstrutural);
  const atendimento = (m: PrevisaoDoMes) => {
    const p = prevista(m);
    return m.entregues !== null && p !== null && p > 0 ? m.entregues / p : null;
  };

  // AS BARRAS OCUPAM OS TRÊS QUARTOS DE BAIXO — o degrau de cima do eixo fica no topo dessa faixa — e o atendimento corre
  // numa faixa própria em cima delas, como a maquete: na mesma escala, a linha de 30% cruzava as barras e os números das
  // duas se sobrepunham.
  const degraus = escala(Math.max(0, ...meses.flatMap((m) => [prevista(m) ?? 0, m.entregues ?? 0])));
  const teto = degraus[4];
  const percentuais = meses.map(atendimento).filter((v): v is number => v !== null);
  const maiorPct = percentuais.length ? Math.max(...percentuais) : 0;
  const menorPct = percentuais.length ? Math.min(...percentuais) : 0;

  const larguraUtil = LARGURA - MARGEM.esquerda - MARGEM.direita;
  const alturaUtil = ALTURA - MARGEM.topo - MARGEM.base;
  const coluna = larguraUtil / meses.length;
  const barra = Math.min(26, coluna * 0.4);
  const barraClara = Math.min(20, coluna * 0.3);
  const base0 = MARGEM.topo + alturaUtil;
  const y = (v: number) => base0 - alturaUtil * ALTURA_DAS_BARRAS * (teto > 0 ? v / teto : 0);
  const faixaTopo = MARGEM.topo + 6;
  const faixaBase = MARGEM.topo + alturaUtil * (1 - ALTURA_DAS_BARRAS) - 4;
  const yPct = (v: number) =>
    maiorPct > menorPct ? faixaBase - ((v - menorPct) / (maiorPct - menorPct)) * (faixaBase - faixaTopo) : (faixaTopo + faixaBase) / 2;
  const centro = (i: number) => MARGEM.esquerda + coluna * i + coluna / 2;

  const pontos = meses
    .map((m, i) => ({ i, v: atendimento(m) }))
    .filter((p): p is { i: number; v: number } => p.v !== null);

  function teclado(e: KeyboardEvent<SVGGElement>, mes: number, escolhido: boolean) {
    if (e.key !== 'Enter' && e.key !== ' ') return;
    e.preventDefault();
    aoEscolherMes(escolhido ? null : mes);
  }

  return (
    <div className="dem-grafico-rolagem">
      <svg className="dem-grafico" viewBox={`0 0 ${LARGURA} ${ALTURA}`} role="group" aria-label="Escolher o mês da previsão">
        {/* AS GRADES E OS DOIS EIXOS: unidades à esquerda; à direita, o maior e o menor atendimento, nas pontas da faixa. */}
        {degraus.map((d, i) => (
          <g key={`g${i}`}>
            <line x1={MARGEM.esquerda} x2={LARGURA - MARGEM.direita} y1={y(d)} y2={y(d)} className="dem-grafico-grade" />
            <text x={MARGEM.esquerda - 8} y={y(d) + 4} textAnchor="end" className="dem-grafico-eixo">
              {n(d, 0)}
            </text>
          </g>
        ))}
        {percentuais.length > 0 &&
          [...new Set([maiorPct, menorPct])].map((p) => (
            <text key={`pct${p}`} x={LARGURA - MARGEM.direita + 8} y={yPct(p) + 4} textAnchor="start" className="dem-grafico-eixo">
              {n(p * 100, 0)}%
            </text>
          ))}

        {meses.map((m, i) => {
          const p = prevista(m);
          const escolhido = mesEscolhido === m.mes;
          const apagado = mesEscolhido !== null && !escolhido;
          const at = atendimento(m);
          return (
            <g
              key={m.mes}
              role="button"
              tabIndex={0}
              aria-pressed={escolhido}
              aria-label={
                `${NOME_DO_MES_POR_EXTENSO[m.mes]}: ${p === null ? 'demanda sem dado' : `${n(p)} máquinas de demanda prevista`}` +
                `${m.entregues === null ? '' : `, ${n(m.entregues, 0)} entregues`}${at === null ? '' : `, ${n(at * 100, 0)}% de atendimento`}`
              }
              className="dem-grafico-mes"
              data-apagado={apagado ? 'true' : undefined}
              onClick={() => aoEscolherMes(escolhido ? null : m.mes)}
              onKeyDown={(e) => teclado(e, m.mes, escolhido)}
            >
              <rect x={centro(i) - coluna / 2} y={MARGEM.topo - 24} width={coluna} height={alturaUtil + 24 + MARGEM.base} className="dem-grafico-alvo" />
              {p !== null && (
                <>
                  <rect x={centro(i) - barra - 1} y={y(p)} width={barra} height={Math.max(0, y(0) - y(p))} className="dem-barra-prevista" rx="2" />
                  <text x={centro(i) - barra / 2 - 1} y={y(p) - 5} textAnchor="middle" className="dem-grafico-valor">
                    {n(p, 0)}
                  </text>
                </>
              )}
              {m.entregues !== null && (
                <>
                  <rect x={centro(i) + 1} y={y(m.entregues)} width={barraClara} height={Math.max(0, y(0) - y(m.entregues))} className="dem-barra-realizada" rx="2" />
                  <text x={centro(i) + barraClara / 2 + 1} y={y(m.entregues) - 5} textAnchor="middle" className="dem-grafico-valor-claro">
                    {n(m.entregues, 0)}
                  </text>
                </>
              )}
              <text x={centro(i)} y={ALTURA - 10} textAnchor="middle" className="dem-grafico-mes-nome">
                {NOME_DO_MES[m.mes].toUpperCase()}
              </text>
            </g>
          );
        })}

        {/* A LINHA DO ATENDIMENTO, tracejada como a maquete, com o percentual em cima de cada ponto. */}
        {pontos.length > 1 && (
          <polyline
            points={pontos.map((p) => `${centro(p.i)},${yPct(p.v)}`).join(' ')}
            className="dem-linha-atendimento"
            aria-hidden="true"
          />
        )}
        {pontos.map((p) => (
          <g key={`p${p.i}`} aria-hidden="true">
            <circle cx={centro(p.i)} cy={yPct(p.v)} r="3.6" className="dem-ponto-atendimento" />
            <text x={centro(p.i)} y={yPct(p.v) - 9} textAnchor="middle" className="dem-grafico-pct">
              {n(p.v * 100, 0)}%
            </text>
          </g>
        ))}
      </svg>
    </div>
  );
}
