/**
 * Formatação de moeda compacta usada em toda a Performance de CEN — porte de
 * `fmtBRLcompact` (prototipo/referencia/assets/app.js linha 323). Fica aqui
 * (e não na tela) para telas/PerformanceCen.tsx e os componentes de
 * apresentação poderem importar sem depender um do outro. O corpo mora em
 * dados/formatadores (documento 54 §3.5): este arquivo só dá o nome do protótipo.
 */
export { formatarBRLCompacto as fmtBRLcompact } from '../../dados/formatadores';
