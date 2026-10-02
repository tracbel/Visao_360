/**
 * O REGISTRO DO CHART.JS PARA A LINHA MENSAL.
 *
 * POR QUE UM ARQUIVO SÓ PARA ISTO: o `padraoDosGraficos` escreve em `ChartJS.defaults.plugins.tooltip`, e esse objeto só
 * existe DEPOIS que o plugin de balão é registrado. Dentro do `GraficoLinhaMensal` o registro rodava depois dos `import`s — e
 * na tela em que a linha é o primeiro gráfico a carregar o padrão (o Preço de Commodities, issue 260), o padrão quebrava com
 * "Cannot set properties of undefined". Importado ANTES do padrão, este arquivo garante a ordem — o mesmo do
 * `registroDoGraficoCombinado` da Rentabilidade.
 */

import { CategoryScale, Chart as ChartJS, Filler, LineElement, LinearScale, PointElement, Tooltip } from 'chart.js';

ChartJS.register(CategoryScale, Filler, LineElement, LinearScale, PointElement, Tooltip);
