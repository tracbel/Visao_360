/**
 * O REGISTRO DO CHART.JS PARA O GRÁFICO COMBINADO DA RENTABILIDADE.
 *
 * POR QUE UM ARQUIVO SÓ PARA ISTO: o `padraoDosGraficos` escreve em
 * `ChartJS.defaults.plugins.tooltip`, e esse objeto só existe DEPOIS que o
 * plugin de balão é registrado. Dentro do próprio arquivo do gráfico o registro
 * roda depois dos `import`s — e se ele for o primeiro gráfico a carregar o
 * padrão (o que acontece nos testes, e pode acontecer num pacote dividido), o
 * padrão quebra com "Cannot set properties of undefined". Importado ANTES do
 * padrão, este arquivo garante a ordem.
 */

import {
  BarController,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LineController,
  LineElement,
  LinearScale,
  PointElement,
  Tooltip,
} from 'chart.js';

ChartJS.register(BarController, BarElement, CategoryScale, LineController, LineElement, LinearScale, PointElement, Tooltip);
