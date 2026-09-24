/**
 * UMA ROSCA EM SVG — a "Distribuição da percepção comercial" da maquete.
 *
 * POR QUE SVG E NÃO O CHART.JS: são quatro fatias e um número no meio. Em SVG o
 * desenho tem tamanho declarado, não mede nada, não espera fonte e o número do
 * centro é texto de verdade. Uma fatia de valor zero simplesmente não aparece.
 */

export function Rosca({
  fatias,
  centro,
  legendaDoCentro,
  tamanho = 150,
  espessura = 26,
}: {
  fatias: readonly { nome: string; valor: number; cor: string }[];
  centro: string;
  legendaDoCentro: string;
  tamanho?: number;
  espessura?: number;
}) {
  const raio = (tamanho - espessura) / 2;
  const circunferencia = 2 * Math.PI * raio;
  const total = fatias.reduce((s, f) => s + f.valor, 0);

  // Cada fatia começa onde a anterior acabou; o desenho parte do topo (−90°).
  const desenhadas = fatias.reduce<{ nome: string; cor: string; comprimento: number; inicio: number }[]>(
    (acumuladas, f) => {
      const inicio = acumuladas.length === 0 ? 0 : acumuladas[acumuladas.length - 1].inicio + acumuladas[acumuladas.length - 1].comprimento;
      const comprimento = total > 0 ? (f.valor / total) * circunferencia : 0;
      return [...acumuladas, { nome: f.nome, cor: f.cor, comprimento, inicio }];
    },
    [],
  );

  // O TAMANHO NA TELA É DO CSS (`.mom-rosca`): o desenho é feito numa caixa de
  // `tamanho` e escala junto, para caber na coluna de 1.300 px sem refazer conta.
  return (
    <div className="mom-rosca">
      <svg width="100%" height="100%" viewBox={`0 0 ${tamanho} ${tamanho}`} aria-hidden="true">
        <circle cx={tamanho / 2} cy={tamanho / 2} r={raio} fill="none" stroke="#F0F2F1" strokeWidth={espessura} />
        {desenhadas
          .filter((f) => f.comprimento > 0)
          .map((f) => (
            <circle
              key={f.nome}
              cx={tamanho / 2}
              cy={tamanho / 2}
              r={raio}
              fill="none"
              stroke={f.cor}
              strokeWidth={espessura}
              strokeDasharray={`${f.comprimento} ${circunferencia - f.comprimento}`}
              strokeDashoffset={-f.inicio}
              transform={`rotate(-90 ${tamanho / 2} ${tamanho / 2})`}
            />
          ))}
      </svg>
      <div className="mom-rosca-centro">
        <strong>{centro}</strong>
        <span>{legendaDoCentro}</span>
      </div>
    </div>
  );
}
