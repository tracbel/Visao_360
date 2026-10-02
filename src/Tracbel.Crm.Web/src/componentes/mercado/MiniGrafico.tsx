/**
 * O GRÁFICO PEQUENO DO CANTO DOS CARTÕES — as barrinhas e a linha das maquetes do Diagnóstico Comercial e da Demanda e
 * Previsão (02/10/2026).
 *
 * SÓ COM SÉRIE DE VERDADE: cada tela passa a série que o cartão tem (a previsão mês a mês, a entrega mês a mês, o parque
 * por cultura, os municípios por classe…), e a dica diz qual é. Série com menos de dois pontos não desenha nada — um
 * risco solto pareceria tendência.
 */

const LARGURA = 54;
const ALTURA = 36;

export function MiniGrafico({
  tipo,
  valores,
  cor,
  rotulo,
}: {
  tipo: 'barras' | 'linha';
  valores: number[];
  /** A cor da série — a do tom do cartão. */
  cor: string;
  /** O que a série é, para a dica e para o leitor de tela: "Previsão mês a mês, de novembro a outubro". */
  rotulo: string;
}) {
  const serie = valores.filter((v) => Number.isFinite(v) && v >= 0);
  if (serie.length < 2) return null;
  const maior = Math.max(...serie);

  if (tipo === 'barras') {
    const passo = LARGURA / serie.length;
    const largura = Math.max(2.5, Math.min(7, passo * 0.6));
    return (
      <svg className="mini-grafico" width={LARGURA} height={ALTURA} viewBox={`0 0 ${LARGURA} ${ALTURA}`} role="img" aria-label={rotulo}>
        <title>{rotulo}</title>
        {serie.map((v, i) => {
          const altura = maior > 0 ? Math.max(3, (v / maior) * (ALTURA - 2)) : 3;
          return (
            <rect
              key={i}
              x={i * passo + (passo - largura) / 2}
              y={ALTURA - altura}
              width={largura}
              height={altura}
              rx="1.5"
              fill={cor}
              opacity={0.3 + 0.45 * (maior > 0 ? v / maior : 0)}
            />
          );
        })}
      </svg>
    );
  }

  const menor = Math.min(...serie);
  const amplitude = maior - menor || 1;
  const pontos = serie
    .map((v, i) => `${((i / (serie.length - 1)) * (LARGURA - 4) + 2).toFixed(1)},${(ALTURA - 4 - ((v - menor) / amplitude) * (ALTURA - 10)).toFixed(1)}`)
    .join(' ');
  return (
    <svg className="mini-grafico" width={LARGURA + 8} height={ALTURA} viewBox={`0 0 ${LARGURA} ${ALTURA}`} role="img" aria-label={rotulo}>
      <title>{rotulo}</title>
      <polyline points={pontos} fill="none" stroke={cor} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" opacity="0.85" />
    </svg>
  );
}
