/**
 * O MENU LATERAL — e o que ele deliberadamente não oferece.
 *
 * Mora fora do `Layout.tsx` porque é dado, não componente: assim o teste do menu
 * o lê sem montar a aplicação inteira, e o recarregamento rápido do Vite
 * continua valendo para o `Layout`.
 *
 * **A Agenda saiu do menu em 19/09/2026 (issue 029), e continua existindo.** A
 * rota `/agenda` abre por URL e pelos links que já levam a ela — a Visão 360, o
 * painel executivo, a Ficha do Cliente e a aba de pós-vendas. O que saiu foi a
 * porta permanente na lateral: a Agenda é o andamento de uma tarefa já
 * escolhida, e o dia do CEN começa na Cobertura de Carteira, que é onde a
 * prioridade é calculada. Duas portas para o mesmo começo de dia é o caminho de
 * volta para "telas demais" (`docs/prototipo/08` §3.1 e §5.2).
 *
 * **Nada de dado foi mexido:** as tarefas continuam alimentando o Meu dia, o
 * 360, a ficha de oportunidade e o consolidado.
 */

import type { ComponentType } from 'react';
import {
  IconeClientes,
  IconeCobertura,
  IconeCoberturaRegional,
  IconeConfiguracoes,
  IconeEquipamentos,
  IconeFunil,
  IconeIndicadoresGeograficos,
  IconePerformance,
  IconePipeline,
  IconeVisao360,
} from './Icones';

export type ItemNav = { caminho: string; rotulo: string; Icone: ComponentType<{ tamanho?: number }> };

export type SecaoNav = { titulo: string; itens: ItemNav[] };

export const SECOES: SecaoNav[] = [
  {
    titulo: 'Executivo',
    itens: [{ caminho: '/', rotulo: 'Visão 360', Icone: IconeVisao360 }],
  },
  {
    titulo: 'Comercial',
    itens: [
      { caminho: '/cobertura', rotulo: 'Cobertura de Carteira', Icone: IconeCobertura },
      { caminho: '/pipeline', rotulo: 'Pipeline de Vendas', Icone: IconePipeline },
      { caminho: '/clientes', rotulo: 'Clientes', Icone: IconeClientes },
      { caminho: '/equipamentos', rotulo: 'Equipamentos', Icone: IconeEquipamentos },
    ],
  },
  {
    titulo: 'Relatórios',
    itens: [
      { caminho: '/relatorios/funil', rotulo: 'Funil de Vendas', Icone: IconeFunil },
      { caminho: '/relatorios/performance', rotulo: 'Performance de CEN', Icone: IconePerformance },
      { caminho: '/relatorios/cobertura', rotulo: 'Cobertura por Filial', Icone: IconeCoberturaRegional },
      // O mapa aberto da maquete (23/09/2026): antes era o mesmo ícone da linha de cima.
      { caminho: '/relatorios/territorio', rotulo: 'Indicadores Geográficos', Icone: IconeIndicadoresGeograficos },
    ],
  },
  {
    titulo: 'Sistema',
    itens: [
      { caminho: '/config', rotulo: 'Configurações', Icone: IconeConfiguracoes },
    ],
  },
];
