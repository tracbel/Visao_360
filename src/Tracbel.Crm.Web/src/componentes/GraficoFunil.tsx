/**
 * O funil de vendas em SVG — porte direto de `mountFunil()`
 * (`prototipo/referencia/assets/app.js`, linha 568), com a legenda de conversão
 * ao lado.
 *
 * A GEOMETRIA É A DO ORIGINAL, unidade por unidade: viewBox 640×440, largura
 * máxima 480 e mínima 60, altura de faixa `(H - 40) / n`, e a largura de cada
 * faixa proporcional ao valor absoluto — **não forçada a decrescer**. Um funil
 * que estreita sempre desenha uma conversão que pode não ter acontecido; o
 * original já sabia disso, e a fase que cresce aparece mais larga que a
 * anterior, com a legenda marcando `↑` na conversão.
 *
 * O QUE MUDA EM RELAÇÃO AO PROTÓTIPO É SÓ A ORIGEM DO DADO. Lá as sete fases
 * eram uma constante no JavaScript; aqui elas chegam de `/api/v1/relatorios/funil`,
 * que agrupa `processo.Processo` no banco dentro da filial do contexto.
 *
 * O rótulo dentro da faixa segue a mesma regra do original: cabe dentro quando a
 * faixa passa de 55 unidades de largura, senão vai para a direita, na cor da
 * fase. E o texto sobre amarelo e laranja é escuro, porque branco sobre
 * `#FFDE00` não se lê.
 */

import type { CSSProperties } from 'react';

/** Uma faixa do funil. `valor` é o que a faixa mede na métrica escolhida. */
export type FaixaDoFunil = {
  chave: string;
  rotulo: string;
  valor: number;
  cor: string;
  /** O que aparece dentro da faixa. Deixa a formatação com quem chama. */
  texto: string;
};

const LARGURA = 640;
const ALTURA = 440;
const LARGURA_MAXIMA = 480;
const LARGURA_MINIMA = 60;

/**
 * Abaixo de quantos processos na fase anterior a conversão deixa de ser exibida.
 *
 * Não é um número mágico: é o ponto em que uma razão passa a dizer mais sobre o
 * acaso do que sobre o funil. Com base 1, qualquer fase seguinte vira um
 * percentual de quatro ou cinco dígitos.
 */
const BASE_MINIMA_DE_CONVERSAO = 10;

/** Sobre estas duas cores o texto branco desaparece (app.js:617). */
const CORES_CLARAS = ['#FFDE00', '#F59E0B'];

export function GraficoFunil({
  faixas,
  aoClicarNaFaixa,
}: {
  faixas: FaixaDoFunil[];
  aoClicarNaFaixa?: (chave: string) => void;
}) {
  if (faixas.length === 0) return null;

  const valores = faixas.map((f) => f.valor);
  const maior = Math.max(...valores, 1);
  const alturaDaFaixa = (ALTURA - 40) / faixas.length;
  const centro = LARGURA / 2;

  const larguras = valores.map(
    (v) => LARGURA_MINIMA + (LARGURA_MAXIMA - LARGURA_MINIMA) * (v / maior),
  );

  return (
    <svg
      id="funil-svg"
      viewBox={`0 0 ${LARGURA} ${ALTURA}`}
      role="img"
      aria-label={`Funil de vendas por fase: ${faixas.map((f) => `${f.rotulo}, ${f.texto}`).join('; ')}`}
    >
      {faixas.map((faixa, i) => {
        const y0 = 20 + i * alturaDaFaixa;
        const y1 = y0 + alturaDaFaixa;
        const w0 = larguras[i];
        const w1 = i < faixas.length - 1 ? larguras[i + 1] : w0 * 0.7;

        const meio = (y0 + y1) / 2;
        const cabeDentro = w0 > 55;
        const tamanhoDaFonte = w0 > 200 ? 16 : w0 > 100 ? 13 : 11;
        const corDoTexto = CORES_CLARAS.includes(faixa.cor) ? '#1B5E20' : '#fff';

        return (
          <g key={faixa.chave}>
            <path
              d={`M ${centro - w0 / 2} ${y0} L ${centro + w0 / 2} ${y0} L ${centro + w1 / 2} ${y1} L ${centro - w1 / 2} ${y1} Z`}
              fill={faixa.cor}
              stroke="rgba(255,255,255,0.4)"
              strokeWidth={1}
              className="funil-slice"
              data-stage={faixa.chave}
              onClick={aoClicarNaFaixa ? () => aoClicarNaFaixa(faixa.chave) : undefined}
              style={aoClicarNaFaixa ? ({ cursor: 'pointer' } as CSSProperties) : undefined}
            >
              <title>
                {faixa.rotulo}: {faixa.texto}
              </title>
            </path>

            {cabeDentro ? (
              <text
                x={centro}
                y={meio + tamanhoDaFonte / 3}
                textAnchor="middle"
                fill={corDoTexto}
                fontSize={tamanhoDaFonte}
                fontWeight={600}
                fontFamily="Inter, sans-serif"
                style={{ pointerEvents: 'none' }}
              >
                {faixa.texto}
              </text>
            ) : (
              <text
                x={centro + w0 / 2 + 8}
                y={meio + 4}
                textAnchor="start"
                fill={faixa.cor}
                fontSize={11}
                fontWeight={600}
                fontFamily="Inter, sans-serif"
                style={{ pointerEvents: 'none' }}
              >
                {faixa.texto}
              </text>
            )}
          </g>
        );
      })}
    </svg>
  );
}

/**
 * A legenda do funil: a fase, o valor dela e a conversão desde a fase anterior.
 *
 * A CONVERSÃO É ENTRE FAIXAS VIZINHAS, como no original — a fatia de cima não
 * tem conversão e mostra travessão. Acima de 100% ela vira `↑` em azul, porque
 * "120% de conversão" não é conversão: é uma fase que tem mais processos que a
 * anterior, o que acontece de verdade quando o fluxo não é linear.
 */
export function LegendaDoFunil({ faixas }: { faixas: FaixaDoFunil[] }) {
  return (
    <div className="funil-legend">
      <div className="legend-header">
        <span>Fase</span>
        <span style={{ float: 'right' }}>Conv.</span>
      </div>
      {faixas.map((faixa, i) => {
        const anterior = i > 0 ? faixas[i - 1].valor : faixa.valor;

        // UMA TAXA SOBRE UMA BASE MINÚSCULA NÃO É UMA TAXA. No funil
        // consolidado das treze filiais, a fase "* Não iniciado *" tem UM
        // processo e a seguinte tem 14.307 — a conta dá "↑ 1.430.700%", que é
        // aritmeticamente correto e não informa nada. Abaixo da base mínima a
        // legenda mostra travessão e diz por quê ao passar o cursor, em vez de
        // imprimir um número que só pode ser lido errado.
        const baseInsuficiente = i > 0 && anterior < BASE_MINIMA_DE_CONVERSAO;
        const conversao =
          i === 0 || baseInsuficiente ? null : anterior > 0 ? (faixa.valor / anterior) * 100 : 0;

        return (
          <div className="legend-row" key={faixa.chave}>
            <span className="legend-swatch" style={{ background: faixa.cor }} />
            <div className="legend-info">
              <span className="legend-label">{faixa.rotulo}</span>
              <span className="legend-value">{faixa.texto}</span>
            </div>
            {conversao === null ? (
              <span
                className="legend-pct"
                style={{ color: 'var(--text-tertiary)' }}
                title={
                  baseInsuficiente
                    ? `A fase anterior tem ${anterior.toLocaleString('pt-BR')} — base pequena demais para uma taxa significar alguma coisa.`
                    : 'Primeira fase do funil: não há fase anterior de onde converter.'
                }
              >
                —
              </span>
            ) : conversao > 100 ? (
              <span className="legend-pct" style={{ color: 'var(--info)' }}>
                ↑ {conversao.toFixed(0)}%
              </span>
            ) : (
              <span
                className="legend-pct"
                style={{
                  color:
                    conversao >= 70
                      ? 'var(--success)'
                      : conversao >= 40
                        ? 'var(--warning)'
                        : 'var(--danger)',
                }}
              >
                {conversao.toFixed(0)}%
              </span>
            )}
          </div>
        );
      })}
    </div>
  );
}

/**
 * As cores das faixas, na ordem do protótipo — azul no topo, vermelho no fim.
 *
 * O original tinha sete fases fixas e sete cores escritas ao lado de cada uma.
 * Como as fases agora vêm do dado e são em número variável, a paleta é
 * percorrida na ordem e repetida se precisar: a cor diz a POSIÇÃO no funil, que
 * é o que ela dizia antes também.
 */
export const CORES_DO_FUNIL = [
  '#3B82F6',
  '#0EA5E9',
  '#06B6D4',
  '#14B8A6',
  '#FFDE00',
  '#F59E0B',
  '#DC2626',
] as const;

/**
 * A cor da posição `i`, espalhando a paleta inteira sobre o número de fases.
 *
 * **Nunca repete o ciclo.** Uma primeira versão caía num `i % 7` quando havia
 * mais fases do que cores, e o resultado era um funil que ia do azul ao vermelho
 * e **voltava ao azul no meio** — a cor deixava de dizer a posição no funil, que
 * é a única coisa que ela diz. Espalhar o índice mantém a leitura monotônica com
 * qualquer número de fases: o fluxo de vendas real tem dez.
 */
export function corDaFaixa(i: number, total: number): string {
  const passo = total <= 1 ? 0 : (CORES_DO_FUNIL.length - 1) / (total - 1);
  return CORES_DO_FUNIL[Math.round(i * passo)];
}
