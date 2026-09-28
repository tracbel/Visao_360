/**
 * Tabela de rotas.
 *
 * A base é o `ROUTES` do protótipo de referência
 * (`prototipo/referencia/assets/app.js`, linha 3), com os mesmos caminhos e
 * breadcrumbs, para a comparação com as capturas continuar valendo nas telas
 * que ainda são o porte do protótipo.
 *
 * **O campo `usaApi`** marca as telas ligadas ao nosso banco pela API. Ele tem
 * um efeito visível: o seletor de filial só aparece no cabeçalho dessas rotas.
 * A filial é a fronteira de acesso da API — nas telas que leem o JSON do
 * protótipo ela não existe, e mostrar o seletor lá prometeria um efeito que não
 * acontece. Também é o que mantém as outras treze rotas idênticas às capturas.
 *
 * **Rotas com parâmetro** (`:chave`) usam o GUID que a API devolve. As rotas de
 * identificador fixo do protótipo (`/clientes/84391`,
 * `/equipamentos/1RW7250PVMR123456`) são declaradas ANTES das rotas com
 * parâmetro, porque `acharRota` casa a primeira que bater.
 *
 * **As telas do protótipo que leem JSON fictício só existem no `npm run dev`**
 * (issue 191): a ficha do cliente 84391, a do equipamento 1RW7250…, a Nova
 * Oportunidade e o mapa do protótipo. Elas mostravam cliente, frota,
 * faturamento e telemetria inventados sem aviso nenhum, e a Nova Oportunidade
 * "salvava" só no navegador. Continuam no desenvolvimento porque a comparação
 * visual as mede. No pacote publicado, o endereço delas cai na rota real (a
 * ficha pela API diz que a chave não existe) ou na Visão 360.
 *
 * O TERNÁRIO É O QUE TIRA AS TELAS DO PACOTE, como os harness do `App.tsx`: no
 * `build`, `import.meta.env.DEV` vira `false`, o ramo morre e os quatro
 * componentes deixam de ser alcançáveis. `npm run visual:conferir-pacote`
 * reprova se alguma marca delas voltar ao `dist`.
 */

import type { ComponentType } from 'react';
import { Agenda } from './telas/Agenda';
import { ClienteFicha } from './telas/ClienteFicha';
import { CoberturaCarteira } from './telas/CoberturaCarteira';
import { CoberturaRegional } from './telas/CoberturaRegional';
import { Configuracoes } from './telas/Configuracoes';
import { DiagnosticoComercial } from './telas/DiagnosticoComercial';
import { EquipamentoFicha } from './telas/EquipamentoFicha';
import { ForecastGerencia } from './telas/ForecastGerencia';
import { Funil } from './telas/Funil';
import { IndicadoresGeograficos } from './telas/IndicadoresGeograficos';
import { Inicio } from './telas/Inicio';
import { NovaOportunidade } from './telas/NovaOportunidade';
import { OportunidadeFicha } from './telas/OportunidadeFicha';
import { PerformanceCen } from './telas/PerformanceCen';
import { Pipeline } from './telas/Pipeline';
import { Visao360 } from './telas/Visao360';
import { ClienteCadastro } from './telas/cadastro/ClienteCadastro';
import { ClientesLista } from './telas/cadastro/ClientesLista';
import { EquipamentoCadastro } from './telas/cadastro/EquipamentoCadastro';
import { EquipamentosLista } from './telas/cadastro/EquipamentosLista';

export type Rota = {
  caminho: string;
  titulo: string;
  trilha: string[];
  Componente: ComponentType;
  /** Lê e grava pela nossa API. Ganha o seletor de filial no cabeçalho. */
  usaApi?: boolean;
  // NÃO HÁ MAIS `larga` (27/09/2026): toda tela usa a janela inteira. O teto de 1.400 px do
  // `.content` valia para todas menos três, e deixava um terço da tela vazio num monitor de 1.700 px.
};

export const ROTAS: Rota[] = [
  {
    caminho: '/',
    titulo: 'Visão 360',
    trilha: ['Visão 360'],
    Componente: Visao360,
    usaApi: true,
  },
  ...(import.meta.env.DEV
    ? [
        {
          // Não é tela de produto: é o índice de desenvolvimento, e fica fora do menu
          // lateral de propósito (documentos 06 §4 e 08 §3).
          caminho: '/inicio-antigo',
          titulo: 'Mapa do protótipo',
          trilha: ['Mapa do protótipo'],
          Componente: Inicio,
        },
      ]
    : []),
  {
    caminho: '/agenda',
    titulo: 'Agenda do CEN',
    trilha: ['Comercial', 'Agenda do CEN'],
    Componente: Agenda,
    usaApi: true,
  },
  {
    caminho: '/cobertura',
    titulo: 'Cobertura de Carteira',
    trilha: ['Comercial', 'Cobertura'],
    Componente: CoberturaCarteira,
    usaApi: true,
  },
  {
    caminho: '/pipeline',
    titulo: 'Pipeline de Vendas',
    trilha: ['Comercial', 'Pipeline'],
    Componente: Pipeline,
    usaApi: true,
  },

  /* ---- Cadastro de clientes, pela API ---------------------------------- */
  {
    caminho: '/clientes',
    titulo: 'Clientes',
    trilha: ['Comercial', 'Clientes'],
    Componente: ClientesLista,
    usaApi: true,
  },
  {
    caminho: '/clientes/novo',
    titulo: 'Novo cliente',
    trilha: ['Comercial', 'Clientes', 'Novo'],
    Componente: ClienteCadastro,
    usaApi: true,
  },
  ...(import.meta.env.DEV
    ? [
        {
          // A ficha rica do protótipo, que lê JSON. Só no desenvolvimento, e ANTES da
          // rota com parâmetro, porque é ela que a comparação visual mede.
          caminho: '/clientes/84391',
          titulo: 'Ficha do Cliente',
          trilha: ['Comercial', 'Clientes', 'Agroindustrial Salvador Arena Ltda'],
          Componente: ClienteFicha,
        },
      ]
    : []),
  {
    caminho: '/clientes/:chave',
    titulo: 'Ficha do Cliente',
    trilha: ['Comercial', 'Clientes', 'Ficha'],
    Componente: ClienteCadastro,
    usaApi: true,
  },

  /* ---- Cadastro de equipamentos, pela API ------------------------------ */
  {
    caminho: '/equipamentos',
    titulo: 'Equipamentos',
    trilha: ['Comercial', 'Equipamentos'],
    Componente: EquipamentosLista,
    usaApi: true,
  },
  {
    caminho: '/equipamentos/novo',
    titulo: 'Novo equipamento',
    trilha: ['Comercial', 'Equipamentos', 'Novo'],
    Componente: EquipamentoCadastro,
    usaApi: true,
  },
  ...(import.meta.env.DEV
    ? [
        {
          // A ficha rica do protótipo (telemetria, revisões, garantia, peças), que lê
          // JSON. Nenhum desses blocos existe na API ainda; ver o documento
          // `docs/prototipo/07-PADRAO-DE-TELA.md`, seção "o que ficou de fora".
          caminho: '/equipamentos/1RW7250PVMR123456',
          titulo: 'Ficha do Equipamento',
          trilha: ['Comercial', 'Equipamentos', 'Trator 7250R · 1RW7250PVMR123456'],
          Componente: EquipamentoFicha,
        },
      ]
    : []),
  {
    caminho: '/equipamentos/:chave',
    titulo: 'Ficha do Equipamento',
    trilha: ['Comercial', 'Equipamentos', 'Ficha'],
    Componente: EquipamentoCadastro,
    usaApi: true,
  },

  ...(import.meta.env.DEV
    ? [
        {
          // O formulário do protótipo grava só no `localStorage` do navegador. A
          // criação de oportunidade pelo CRM entra com a issue 52.
          caminho: '/oportunidades/nova',
          titulo: 'Nova Oportunidade',
          trilha: ['Comercial', 'Oportunidades', 'Nova'],
          Componente: NovaOportunidade,
        },
      ]
    : []),
  {
    // A ROTA DEIXOU DE SER FIXA. Era `/oportunidades/1517613` e servia a UMA
    // oportunidade inventada, de `public/dados/oportunidade-1517613.json` —
    // existia uma oportunidade só no sistema inteiro. Agora a chave vem da URL
    // e abre qualquer processo que a rotina PROCESSOS_VORTICE trouxe.
    //
    // Precisa vir DEPOIS de `/oportunidades/nova` (no desenvolvimento), porque
    // `acharRota` casa a primeira declaração que bater e `nova` seria engolida
    // por `:chave`.
    caminho: '/oportunidades/:chave',
    titulo: 'Ficha de Oportunidade',
    trilha: ['Comercial', 'Oportunidades', 'Ficha'],
    Componente: OportunidadeFicha,
    usaApi: true,
  },
  {
    caminho: '/relatorios/funil',
    // "do Mês" saiu: o funil tem o período escolhido na própria tela (o padrão é o
    // ano fiscal até o último mês fechado), e um título com "do Mês" prometeria um
    // recorte que ele não usa.
    titulo: 'Funil de Vendas',
    trilha: ['Relatórios', 'Funil de Vendas'],
    Componente: Funil,
    usaApi: true,
  },
  {
    caminho: '/relatorios/performance',
    titulo: 'Performance de CEN',
    trilha: ['Relatórios', 'Performance de CEN'],
    Componente: PerformanceCen,
    usaApi: true,
  },
  {
    // A tela "Forecast Gerência" da API Gestão de Negócios, com o realizado que o CRM tem (28/09/2026).
    caminho: '/relatorios/forecast',
    titulo: 'Forecast da Gerência',
    trilha: ['Relatórios', 'Forecast da Gerência'],
    Componente: ForecastGerencia,
    usaApi: true,
  },
  {
    // O NOME MUDOU PORQUE O AGRUPAMENTO MUDOU. Era "Painel Regional", e a
    // regional não existe: `IVS_Regional` tem zero linhas no Vórtice
    // (documento 26, §1). O que existe preenchido é filial → carteira →
    // cidades, e é o que a tela mostra.
    caminho: '/relatorios/cobertura',
    titulo: 'Cobertura por Filial e Carteira',
    trilha: ['Relatórios', 'Cobertura por Filial e Carteira'],
    Componente: CoberturaRegional,
    usaApi: true,
  },
  {
    // Os três mapas da ADR — documento 32. Lê a API e a malha do IBGE em
    // `public/geo`; nenhum número é do protótipo.
    caminho: '/relatorios/territorio',
    titulo: 'Indicadores Geográficos da ADR',
    trilha: ['Relatórios', 'Indicadores Geográficos'],
    Componente: IndicadoresGeograficos,
    usaApi: true,
  },
  /* ---- Inteligência de Mercado: o protótipo da pasta 360 no CRM (épico 264) ---- */
  {
    caminho: '/mercado/diagnostico',
    titulo: 'Diagnóstico Comercial',
    trilha: ['Inteligência de Mercado', 'Diagnóstico Comercial'],
    Componente: DiagnosticoComercial,
    usaApi: true,
  },
  {
    caminho: '/config',
    titulo: 'Configurações',
    trilha: ['Sistema', 'Configurações'],
    Componente: Configuracoes,
  },
];

/** Um caminho declarado vira o padrão que casa com ele, com `:parametro` virando um trecho. */
function casa(declarado: string, caminho: string): boolean {
  if (!declarado.includes(':')) return declarado === caminho;
  const padrao = new RegExp(`^${declarado.replace(/:[^/]+/g, '[^/]+')}$`);
  return padrao.test(caminho);
}

/**
 * A rota de um caminho. Casa a PRIMEIRA declarada que bater, e é por isso que
 * `/clientes/84391` precisa vir antes de `/clientes/:chave` na lista.
 */
export function acharRota(caminho: string): Rota {
  return ROTAS.find((r) => casa(r.caminho, caminho)) ?? ROTAS[0];
}
