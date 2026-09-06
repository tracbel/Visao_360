/**
 * Formatação de moeda compacta usada em toda a Performance de CEN — porte de
 * `fmtBRLcompact` (prototipo/referencia/assets/app.js linha 323). Fica aqui
 * (e não na tela) para telas/PerformanceCen.tsx e os componentes de
 * apresentação poderem importar sem depender um do outro.
 */
export function fmtBRLcompact(v: number): string {
  if (v >= 1_000_000) return `R$ ${(v / 1_000_000).toFixed(v >= 10_000_000 ? 1 : 2)}M`;
  if (v >= 1_000) return `R$ ${(v / 1_000).toFixed(0)}k`;
  return `R$ ${v}`;
}
