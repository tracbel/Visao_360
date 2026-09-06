/**
 * O padrão visual dos gráficos, num lugar só.
 *
 * ---------------------------------------------------------------------------
 * POR QUE ISTO EXISTE: os componentes portados do protótipo fixavam, cada um,
 * a pilha de fontes padrão do Chart.js — `'Helvetica Neue', Helvetica, Arial`.
 * Era proposital enquanto a comparação era pixel a pixel com a referência: o
 * protótipo nunca trocou a fonte da biblioteca, então o porte também não podia.
 *
 * O efeito, com dado real na tela, é que o rótulo do gráfico saía numa fonte e
 * o resto da interface em outra — o "JOAO PESSOA DOS SANTOS" do ranking e o
 * "JOAO PESSOA DOS SANTOS" da tabela logo abaixo não pareciam a mesma coisa. É
 * o tipo de detalhe que faz um painel parecer montado às pressas.
 *
 * Aqui a fonte, a cor do texto, a grade e o balão passam a ser os mesmos da
 * aplicação, definidos uma vez. Importar este módulo já aplica o padrão — é
 * por isso que ele não exporta função de inicialização: quem importa um
 * componente de gráfico recebe o padrão junto, sem depender de alguém lembrar
 * de chamar algo no arranque.
 */
import { Chart as ChartJS } from 'chart.js';

/** A mesma pilha do `body` em `design-system.css`. */
export const FONTE_DOS_GRAFICOS =
  "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif";

/** A mesma família dos números da interface. */
export const FONTE_MONO_DOS_GRAFICOS = "'JetBrains Mono', 'SFMono-Regular', Consolas, monospace";

/* As cores são as variáveis do tema, escritas em hexadecimal porque o Chart.js
   desenha em canvas e não enxerga `var(--…)`. */
const TEXTO_TERCIARIO = '#7A857D';
const TEXTO_PRIMARIO = '#1A2420';
const GRADE = 'rgba(26, 36, 32, 0.06)';

ChartJS.defaults.font.family = FONTE_DOS_GRAFICOS;
ChartJS.defaults.font.size = 11;
ChartJS.defaults.color = TEXTO_TERCIARIO;

ChartJS.defaults.plugins.tooltip.backgroundColor = TEXTO_PRIMARIO;
ChartJS.defaults.plugins.tooltip.titleFont = { family: FONTE_DOS_GRAFICOS, size: 11, weight: 600 };
ChartJS.defaults.plugins.tooltip.bodyFont = { family: FONTE_DOS_GRAFICOS, size: 11 };
ChartJS.defaults.plugins.tooltip.padding = 10;
ChartJS.defaults.plugins.tooltip.cornerRadius = 6;
ChartJS.defaults.plugins.tooltip.displayColors = true;
ChartJS.defaults.plugins.tooltip.boxPadding = 4;

/** A grade horizontal discreta que os eixos de valor usam. */
export const COR_DA_GRADE = GRADE;
