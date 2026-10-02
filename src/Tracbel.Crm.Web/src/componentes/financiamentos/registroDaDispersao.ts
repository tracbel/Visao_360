/**
 * O REGISTRO DO CHART.JS PARA A DISPERSÃO DO CRÉDITO (issue 261).
 *
 * POR QUE UM ARQUIVO SÓ PARA ISTO: o `padraoDosGraficos` escreve em `ChartJS.defaults.plugins.tooltip`, e esse objeto só
 * existe DEPOIS que o plugin de balão é registrado. Dentro do próprio arquivo do gráfico o registro roda depois dos
 * `import`s — e, na tela dos Financiamentos, a dispersão é o primeiro gráfico a carregar o padrão. Importado ANTES do padrão,
 * este arquivo garante a ordem (o mesmo do `registroDoGraficoCombinado` da Rentabilidade).
 */

import { BubbleController, Chart as ChartJS, LinearScale, PointElement, Tooltip } from 'chart.js';

ChartJS.register(BubbleController, LinearScale, PointElement, Tooltip);
